// SPDX-License-Identifier: Apache-2.0
//! Stable cooperative lockfile. This module stores no durable operation state.

use crate::StorageError;
use std::fs::{self, File, OpenOptions, TryLockError};
use std::path::{Path, PathBuf};

const LOCK_NAME: &str = ".openknx-task156-cooperation.lock";

pub(crate) struct FileLease {
    // Never cloned, inherited deliberately, truncated, replaced or deleted.
    // Closing the unique owned handle releases the OS lock.
    _file: File,
}

pub(crate) fn qualified_root(root: &Path) -> Result<PathBuf, StorageError> {
    if !root.is_absolute()
        || root.as_os_str().len() > 4096
        || fs::canonicalize(root).map_err(|_| StorageError::Unavailable)? != root
    {
        return Err(StorageError::Unavailable);
    }
    let metadata = fs::symlink_metadata(root).map_err(|_| StorageError::Unavailable)?;
    if !metadata.is_dir() || metadata.file_type().is_symlink() {
        return Err(StorageError::Unavailable);
    }
    #[cfg(windows)]
    {
        use std::os::windows::fs::MetadataExt;
        if metadata.file_attributes() & 0x400 != 0 {
            // FILE_ATTRIBUTE_REPARSE_POINT
            return Err(StorageError::Unavailable);
        }
    }
    Ok(root.to_path_buf())
}

pub(crate) fn acquire(root: &Path) -> Result<FileLease, StorageError> {
    qualified_root(root)?;
    let path = root.join(LOCK_NAME);
    match fs::symlink_metadata(&path) {
        Ok(metadata) if !metadata.is_file() || metadata.file_type().is_symlink() => {
            return Err(StorageError::Unavailable);
        }
        Ok(_) => {}
        Err(error) if error.kind() == std::io::ErrorKind::NotFound => {}
        Err(_) => return Err(StorageError::Unavailable),
    }
    let mut options = OpenOptions::new();
    options.read(true).write(true).create(true).truncate(false);
    #[cfg(windows)]
    {
        use std::os::windows::fs::OpenOptionsExt;
        // Open the final component itself, not a reparse-point target.
        options.custom_flags(0x0020_0000); // FILE_FLAG_OPEN_REPARSE_POINT
    }
    let file = options.open(&path).map_err(|_| StorageError::Unavailable)?;
    if !file
        .metadata()
        .map_err(|_| StorageError::Unavailable)?
        .is_file()
        || fs::symlink_metadata(&path)
            .map_err(|_| StorageError::Unavailable)?
            .file_type()
            .is_symlink()
    {
        return Err(StorageError::Unavailable);
    }
    #[cfg(windows)]
    {
        use std::os::windows::fs::MetadataExt;
        if file
            .metadata()
            .map_err(|_| StorageError::Unavailable)?
            .file_attributes()
            & 0x400
            != 0
        {
            return Err(StorageError::Unavailable);
        }
    }
    // Zero-wait acquisition: no unbounded wait, retry loop or deadlock on a
    // duplicated handle. A new open handle is used for every acquisition.
    file.try_lock().map_err(|error| match error {
        TryLockError::WouldBlock => StorageError::Busy,
        TryLockError::Error(_) => StorageError::Unavailable,
    })?;
    Ok(FileLease { _file: file })
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::process::{Child, Command};
    use std::sync::Mutex;
    use std::time::{Duration, Instant};

    // Fork/exec can transiently retain another test's flock descriptor before
    // CLOEXEC runs. Isolate these OS-lock/process tests; production try_lock
    // remains zero-wait and no lockfile lifecycle guarantee is weakened.
    static OS_LOCK_TESTS: Mutex<()> = Mutex::new(());

    struct ChildGuard(Child);
    impl Drop for ChildGuard {
        fn drop(&mut self) {
            let _ = self.0.kill();
            let _ = self.0.wait();
        }
    }

    #[test]
    fn separate_handles_contend_and_the_stable_file_is_reused() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let lease = acquire(&root).unwrap();
        assert!(matches!(acquire(&root), Err(StorageError::Busy)));
        let path = root.join(LOCK_NAME);
        assert!(path.is_file());
        assert_eq!(fs::metadata(&path).unwrap().len(), 0);
        drop(lease);
        let _lease = acquire(&root).unwrap();
        assert!(path.is_file());
    }

    #[test]
    fn duplicated_descriptor_retains_lock_until_all_related_handles_close() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let lease = acquire(&root).unwrap();
        // Deliberately duplicate only in this regression to make the documented
        // duplicated/inherited descriptor lifetime deterministic.
        let duplicate = lease._file.try_clone().unwrap();
        drop(lease);
        assert!(matches!(acquire(&root), Err(StorageError::Busy)));
        assert!(duplicate.metadata().unwrap().is_file());
        drop(duplicate);
        let _next = acquire(&root).unwrap();
    }

    #[test]
    fn unqualified_roots_and_nonfile_lock_paths_are_rejected() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        assert!(qualified_root(Path::new("relative")).is_err());
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        fs::create_dir(root.join(LOCK_NAME)).unwrap();
        assert!(matches!(acquire(&root), Err(StorageError::Unavailable)));
    }

    #[cfg(unix)]
    #[test]
    fn symlink_lock_path_is_not_followed() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let target = root.join("unrelated");
        fs::write(&target, b"unchanged").unwrap();
        std::os::unix::fs::symlink(&target, root.join(LOCK_NAME)).unwrap();
        assert!(matches!(acquire(&root), Err(StorageError::Unavailable)));
        assert_eq!(fs::read(target).unwrap(), b"unchanged");
    }

    #[test]
    fn lock_child_process() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        let Some(root) = std::env::var_os("OPENKNX_TASK156_LOCK_CHILD") else {
            return;
        };
        let root = PathBuf::from(root);
        let _lease = acquire(&root).unwrap();
        fs::write(root.join("child-holds-lock"), b"synthetic").unwrap();
        std::thread::sleep(Duration::from_secs(5));
    }

    #[test]
    fn process_death_releases_only_the_cooperative_os_lock() {
        let _serial = OS_LOCK_TESTS.lock().unwrap();
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let mut child = ChildGuard(
            Command::new(std::env::current_exe().unwrap())
                .args([
                    "--exact",
                    "cooperative_lock::tests::lock_child_process",
                    "--nocapture",
                ])
                .env("OPENKNX_TASK156_LOCK_CHILD", &root)
                .spawn()
                .unwrap(),
        );
        let deadline = Instant::now() + Duration::from_secs(5);
        while !root.join("child-holds-lock").exists() {
            assert!(Instant::now() < deadline, "child lock deadline expired");
            assert!(
                child.0.try_wait().unwrap().is_none(),
                "child exited before acquiring lock"
            );
            std::thread::sleep(Duration::from_millis(10));
        }
        assert!(matches!(acquire(&root), Err(StorageError::Busy)));
        child.0.kill().unwrap();
        child.0.wait().unwrap();
        let _lease = acquire(&root).unwrap();
        assert!(root.join(LOCK_NAME).is_file());
        // This test proves only OS lock lifecycle, not journal route release.
    }
}
