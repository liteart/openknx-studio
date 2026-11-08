// SPDX-License-Identifier: Apache-2.0
//! Experimental HKCU adapter for cooperating processes under the same account.
//! No protected-service, physical-operation or power-loss qualification is implied.

#![forbid(unsafe_code)]

use crate::{
    Digest, Key, MAX_RECORD_BYTES, NamespaceStorage, RouteLease, Storage, StorageError,
    cooperative_lock, native_layout,
};
use sha2::{Digest as _, Sha256};
use std::os::windows::ffi::OsStrExt;
use std::path::{Path, PathBuf};
use winsafe::{HKEY, co, guard::RegCloseKeyGuard};

const PREFIX: &str = "Software\\OpenKNXStudio\\J156KeysV1";

/// The canonical directory identifies the namespace and holds one stable
/// coordination file. Do not delete, replace or alias that file while using
/// this store. Arbitrary same-account path/registry mutation is outside scope.
pub struct WindowsStorage {
    root: PathBuf,
    namespace: String,
}

impl WindowsStorage {
    pub fn open(root: &Path) -> Result<Self, StorageError> {
        let root = cooperative_lock::qualified_root(root)?;
        let mut hash = Sha256::new();
        for unit in root.as_os_str().encode_wide() {
            hash.update(unit.to_le_bytes());
        }
        let namespace = format!("{PREFIX}\\{}", native_layout::hex(&hash.finalize()));
        Ok(Self { root, namespace })
    }
}

/// Holds the actual OS file-lock handle and registry namespace together. The
/// stronger namespace-wide lock serializes global create-new keys across routes.
pub struct WindowsLease {
    _lock: cooperative_lock::FileLease,
    namespace: RegCloseKeyGuard,
    route: Digest,
}

fn rights() -> co::KEY {
    co::KEY::QUERY_VALUE
        | co::KEY::SET_VALUE
        | co::KEY::CREATE_SUB_KEY
        | co::KEY::ENUMERATE_SUB_KEYS
        | co::KEY::WOW64_64KEY
}

fn create(parent: &HKEY, path: &str) -> Result<(RegCloseKeyGuard, bool), StorageError> {
    let (key, disposition) = parent
        .RegCreateKeyEx(path, None, co::REG_OPTION::NON_VOLATILE, rights(), None)
        .map_err(|_| StorageError::Unavailable)?;
    if disposition == co::REG_DISPOSITION::CREATED_NEW_KEY {
        Ok((key, true))
    } else if disposition == co::REG_DISPOSITION::OPENED_EXISTING_KEY {
        Ok((key, false))
    } else {
        Err(StorageError::Unavailable)
    }
}

fn open(parent: &HKEY, path: &str) -> Result<Option<RegCloseKeyGuard>, StorageError> {
    match parent.RegOpenKeyEx(
        Some(path),
        co::REG_OPTION::default(),
        co::KEY::QUERY_VALUE | co::KEY::ENUMERATE_SUB_KEYS | co::KEY::WOW64_64KEY,
    ) {
        Ok(key) => Ok(Some(key)),
        Err(error) if error == co::ERROR::FILE_NOT_FOUND || error == co::ERROR::PATH_NOT_FOUND => {
            Ok(None)
        }
        Err(_) => Err(StorageError::Unavailable),
    }
}

fn shape(key: &HKEY) -> Result<(u32, u32, u32), StorageError> {
    let (mut subkeys, mut values, mut max_name) = (0, 0, 0);
    key.RegQueryInfoKey(
        None,
        Some(&mut subkeys),
        Some(&mut max_name),
        None,
        Some(&mut values),
        None,
        None,
        None,
        None,
    )
    .map_err(|_| StorageError::Unavailable)?;
    Ok((subkeys, values, max_name))
}

fn names(data: &HKEY) -> Result<Vec<String>, StorageError> {
    let before = shape(data)?;
    native_layout::validate_data_shape(before.0, before.1, before.2)?;
    // Windows limits a key name to255 characters. The safe wrapper allocates
    // at most that OS-bounded name buffer, then makes ONE call per item and
    // stops on MORE_DATA; it does not resize/retry. Bound count independently
    // of metadata, including concurrent noncooperating mutation.
    let result = data
        .RegEnumKeyEx()
        .map_err(|_| StorageError::Unavailable)?
        .take(native_layout::MAX_CHUNKS + 1)
        .map(|name| name.map_err(|_| StorageError::Unavailable))
        .collect::<Result<Vec<_>, _>>()?;
    for name in &result {
        let child = open(data, name)?.ok_or(StorageError::Unavailable)?;
        if shape(&child)? != (0, 0, 0) {
            return Err(StorageError::Unavailable);
        }
    }
    if result.len() != before.0 as usize
        || result.len() > native_layout::MAX_CHUNKS
        || shape(data)? != before
    {
        return Err(StorageError::Unavailable);
    }
    Ok(result)
}

fn set(key: &HKEY, bytes: &[u8]) -> Result<(), StorageError> {
    let chunks = native_layout::encode_chunks(bytes)?;
    let metadata = shape(key)?;
    if metadata.1 != 0 || metadata.0 > 1 || metadata.2 > native_layout::DATA.len() as u32 {
        return Err(StorageError::Unavailable);
    }
    if metadata.0 == 1 {
        let data = open(key, native_layout::DATA)?.ok_or(StorageError::Unavailable)?;
        let old = names(&data)?;
        native_layout::decode_chunks(old.iter().cloned().map(Ok), MAX_RECORD_BYTES)?;
        for name in old {
            // Deliberately nonrecursive: nested/mutated keys cause an error.
            data.RegDeleteKeyEx(&name, co::KEY::WOW64_64KEY)
                .map_err(|_| StorageError::Unavailable)?;
        }
        drop(data);
        key.RegDeleteKeyEx(native_layout::DATA, co::KEY::WOW64_64KEY)
            .map_err(|_| StorageError::Unavailable)?;
    }
    // This parent is NEVER deleted. Torn replacement means invalid Data under
    // an existing record, never an absent free route.
    let (data, new) = create(key, native_layout::DATA)?;
    if !new {
        return Err(StorageError::Unavailable);
    }
    for name in chunks {
        let (_, new) = create(&data, &name)?;
        if !new {
            return Err(StorageError::Unavailable);
        }
    }
    Ok(())
}

fn read(key: &HKEY, limit: usize) -> Result<Vec<u8>, StorageError> {
    let metadata = shape(key)?;
    if metadata != (1, 0, native_layout::DATA.len() as u32) {
        return Err(StorageError::Unavailable);
    }
    let data = open(key, native_layout::DATA)?.ok_or(StorageError::Unavailable)?;
    native_layout::decode_chunks(names(&data)?.into_iter().map(Ok), limit)
}

impl Storage for WindowsStorage {
    type Lease<'a> = WindowsLease;

    fn lock_route(&self, route: Digest) -> Result<WindowsLease, StorageError> {
        if route == [0; 32] {
            return Err(StorageError::Unavailable);
        }
        let lock = cooperative_lock::acquire(&self.root)?;
        let current = HKEY::RegOpenCurrentUser(co::KEY::QUERY_VALUE | co::KEY::CREATE_SUB_KEY)
            .map_err(|_| StorageError::Unavailable)?;
        let (namespace, _) = create(&current, &self.namespace)?;
        Ok(WindowsLease {
            _lock: lock,
            namespace,
            route,
        })
    }
}

impl NamespaceStorage for WindowsStorage {
    fn lock_records(&self) -> Result<WindowsLease, StorageError> {
        // lock_route already takes the one namespace-wide file lease. The
        // route field is unused by record-only operations; no head is touched.
        self.lock_route([0xfe; 32])
    }
}

impl RouteLease for WindowsLease {
    fn read_route(&mut self, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        match open(&self.namespace, &native_layout::route_path(self.route))? {
            Some(key) => read(&key, limit).map(Some),
            None => Ok(None),
        }
    }

    fn set_route(&mut self, bytes: &[u8]) -> Result<(), StorageError> {
        let (key, _) = create(&self.namespace, &native_layout::route_path(self.route))?;
        set(&key, bytes)
    }

    fn read(&mut self, id: Key, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        match open(&self.namespace, &native_layout::record_path(id))? {
            Some(key) => read(&key, limit).map(Some),
            None => Ok(None),
        }
    }

    fn create_new(&mut self, id: Key, bytes: &[u8]) -> Result<(), StorageError> {
        if bytes.is_empty() || bytes.len() > MAX_RECORD_BYTES {
            return Err(StorageError::Unavailable);
        }
        let (key, new) = create(&self.namespace, &native_layout::record_path(id))?;
        if !new {
            return Err(StorageError::Existing);
        }
        set(&key, bytes)
    }

    fn flush(&mut self) -> Result<(), StorageError> {
        // Explicit checked hive barrier, not close-on-drop or value visibility.
        self.namespace
            .RegFlushKey()
            .map_err(|_| StorageError::Unavailable)
    }
}

#[cfg(test)]
mod tests;
