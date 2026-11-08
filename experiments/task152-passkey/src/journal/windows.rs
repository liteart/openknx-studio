// SPDX-License-Identifier: Apache-2.0
//! HKCU development observations only. This is not a protected operator service.

#![forbid(unsafe_code)]

use super::registry::{self, Disposition, RecordKey, RegistryJournal};
use crate::Error;
use sha2::{Digest, Sha256};
use std::fs::{self, OpenOptions};
use std::io::Write;
use std::os::windows::ffi::OsStrExt;
use std::path::Path;
use winsafe::{HKEY, RegistryValue, co, guard::RegCloseKeyGuard};

const PREFIX: &str = "Software\\OpenKNXStudio\\DevelopmentApproval\\JournalsV1";
const RECORD_VALUE: &str = "receipt";

struct Key(RegCloseKeyGuard);

impl Key {
    fn current_user() -> Result<Self, Error> {
        HKEY::RegOpenCurrentUser(co::KEY::QUERY_VALUE | co::KEY::CREATE_SUB_KEY)
            .map(Self)
            .map_err(|_| Error::Io)
    }

    fn create(parent: &HKEY, name: &str, rights: co::KEY) -> Result<(Self, Disposition), Error> {
        // Every production caller supplies a fixed prefix or validated lowercase
        // hex component; no external registry path or embedded terminator is accepted.
        if name.is_empty() || name.len() > 240 || !name.is_ascii() || name.bytes().any(|b| b == 0) {
            return Err(Error::Io);
        }
        let (handle, disposition) = parent
            .RegCreateKeyEx(
                name,
                None,
                co::REG_OPTION::NON_VOLATILE,
                rights | co::KEY::WOW64_64KEY,
                None,
            )
            .map_err(|_| Error::Io)?;
        let disposition = if disposition == co::REG_DISPOSITION::CREATED_NEW_KEY {
            Disposition::New
        } else if disposition == co::REG_DISPOSITION::OPENED_EXISTING_KEY {
            Disposition::Existing
        } else {
            return Err(Error::Io);
        };
        Ok((Self(handle), disposition))
    }
}

impl RecordKey for Key {
    fn flush(&self) -> Result<(), Error> {
        // The safe guard owns the query-enabled handle. Success comes from the
        // checked explicit flush, never from the guard's close-on-drop behavior.
        self.0.RegFlushKey().map_err(|_| Error::Io)
    }

    fn set_receipt(&self, bytes: &[u8]) -> Result<(), Error> {
        if bytes.is_empty() || bytes.len() > registry::MAX_RECORD_BYTES {
            return Err(Error::Io);
        }
        self.0
            .RegSetValueEx(Some(RECORD_VALUE), RegistryValue::Binary(bytes.to_vec()))
            .map_err(|_| Error::Io)
    }
}

struct Namespace(Key);

impl RegistryJournal for Namespace {
    type Key = Key;

    fn create_record(&self, challenge: &str) -> Result<(Key, Disposition), Error> {
        if !registry::valid_challenge(challenge) {
            return Err(Error::Io);
        }
        Key::create(
            &self.0.0,
            challenge,
            co::KEY::QUERY_VALUE | co::KEY::SET_VALUE,
        )
    }
}

fn open_namespace(directory: &Path) -> Result<Namespace, Error> {
    let metadata = fs::symlink_metadata(directory).map_err(|_| Error::Io)?;
    if !metadata.is_dir()
        || metadata.file_type().is_symlink()
        || fs::canonicalize(directory).map_err(|_| Error::Io)? != directory
    {
        return Err(Error::Io);
    }
    // Hash native UTF-16 code units rather than a lossy display path. The key
    // contains neither the local path nor credential identifiers/private data.
    let mut hasher = Sha256::new();
    for unit in directory.as_os_str().encode_wide() {
        hasher.update(unit.to_le_bytes());
    }
    let namespace = format!("{PREFIX}\\{}", crate::hex(&hasher.finalize()));
    let current_user = Key::current_user()?;
    let (key, _) = Key::create(
        &current_user.0,
        &namespace,
        co::KEY::QUERY_VALUE | co::KEY::SET_VALUE | co::KEY::CREATE_SUB_KEY,
    )?;
    Ok(Namespace(key))
}

pub(super) fn preflight(directory: &Path) -> Result<(), Error> {
    // Establish actual namespace permissions and hive flushing before any
    // ceremony; no per-challenge reservation or reusable slot is created here.
    open_namespace(directory)?.0.flush()
}

pub(super) fn persist(directory: &Path, path: &Path, bytes: &[u8]) -> Result<(), Error> {
    let challenge = registry::record_name(directory, path)?;
    let namespace = open_namespace(directory)?;
    registry::commit(&namespace, &challenge, bytes, || {
        let mut file = OpenOptions::new()
            .write(true)
            .create_new(true)
            .open(path)
            .map_err(|error| {
                if error.kind() == std::io::ErrorKind::AlreadyExists {
                    Error::Replay
                } else {
                    Error::Io
                }
            })?;
        file.write_all(bytes)
            .and_then(|_| file.sync_all())
            .map_err(|_| Error::Io)
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn native_key_value_is_flushed_and_existing_or_partial_records_are_not_reused() {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        preflight(&root).unwrap();
        let name = "a".repeat(64);
        let path = root.join(format!("{name}.spent.json"));
        let bytes = b"{\"synthetic\":true,\"physical_authorization\":false}";
        persist(&root, &path, bytes).unwrap();
        let namespace = open_namespace(&root).unwrap();
        let (key, disposition) = namespace.create_record(&name).unwrap();
        assert_eq!(disposition, Disposition::Existing);
        // Read only this test's newly created synthetic value. Production code
        // never reads an existing registry value to decide whether it is reusable.
        match key.0.RegQueryValueEx(Some(RECORD_VALUE)).unwrap() {
            RegistryValue::Binary(actual) => assert_eq!(actual, bytes),
            _ => panic!("synthetic receipt did not retain its binary type"),
        }
        assert_eq!(persist(&root, &path, b"replacement"), Err(Error::Replay));

        for name in ["b".repeat(64), "c".repeat(64)] {
            let (key, disposition) = namespace.create_record(&name).unwrap();
            assert_eq!(disposition, Disposition::New);
            if name.starts_with('c') {
                key.set_receipt(b"malformed synthetic value").unwrap();
            }
            key.flush().unwrap();
            let path = root.join(format!("{name}.spent.json"));
            assert_eq!(persist(&root, &path, bytes), Err(Error::Replay));
            assert!(!path.exists());
        }
    }

    #[test]
    fn native_concurrent_creators_cannot_both_commit() {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        preflight(&root).unwrap();
        let barrier = std::sync::Arc::new(std::sync::Barrier::new(2));
        let outcomes = std::thread::scope(|scope| {
            let handles: Vec<_> = (0..2)
                .map(|_| {
                    let root = root.clone();
                    let barrier = barrier.clone();
                    scope.spawn(move || {
                        barrier.wait();
                        persist(
                            &root,
                            &root.join(format!("{}.spent.json", "d".repeat(64))),
                            b"synthetic receipt",
                        )
                    })
                })
                .collect();
            handles
                .into_iter()
                .map(|handle| handle.join().unwrap())
                .collect::<Vec<_>>()
        });
        assert_eq!(outcomes.iter().filter(|result| result.is_ok()).count(), 1);
        assert_eq!(
            outcomes
                .iter()
                .filter(|result| **result == Err(Error::Replay))
                .count(),
            1
        );
    }

    #[test]
    fn native_process_contender() {
        let Some(root) = std::env::var_os("OPENKNX_TASK152_TEST_JOURNAL") else {
            return;
        };
        let root = std::path::PathBuf::from(root);
        let index = std::env::var("OPENKNX_TASK152_TEST_CONTENDER").unwrap();
        assert!(index == "0" || index == "1");
        let deadline = std::time::Instant::now() + std::time::Duration::from_secs(5);
        while !root.join("go").exists() {
            assert!(
                std::time::Instant::now() < deadline,
                "parent barrier timed out"
            );
            std::thread::sleep(std::time::Duration::from_millis(10));
        }
        let outcome = persist(
            &root,
            &root.join(format!("{}.spent.json", "e".repeat(64))),
            b"synthetic interprocess receipt",
        );
        let outcome = match outcome {
            Ok(()) => b"committed".as_slice(),
            Err(Error::Replay) => b"consumed".as_slice(),
            Err(error) => panic!("unexpected native outcome: {error:?}"),
        };
        fs::write(root.join(format!("outcome-{index}")), outcome).unwrap();
    }

    #[test]
    fn native_separate_process_creators_have_one_winner() {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        preflight(&root).unwrap();
        let executable = std::env::current_exe().unwrap();
        let mut children = Vec::new();
        for index in 0..2 {
            let stdout = std::fs::File::create(root.join(format!("stdout-{index}"))).unwrap();
            let stderr = std::fs::File::create(root.join(format!("stderr-{index}"))).unwrap();
            children.push(
                std::process::Command::new(&executable)
                    .args([
                        "--exact",
                        "journal::windows::tests::native_process_contender",
                        "--test-threads=1",
                    ])
                    .env("OPENKNX_TASK152_TEST_JOURNAL", &root)
                    .env("OPENKNX_TASK152_TEST_CONTENDER", index.to_string())
                    .stdout(stdout)
                    .stderr(stderr)
                    .spawn()
                    .unwrap(),
            );
        }
        fs::write(root.join("go"), b"start").unwrap();
        let deadline = std::time::Instant::now() + std::time::Duration::from_secs(10);
        let mut failed = false;
        for child in &mut children {
            loop {
                match child.try_wait() {
                    Ok(Some(status)) => {
                        failed = !status.success();
                        break;
                    }
                    Err(_) => {
                        failed = true;
                        break;
                    }
                    Ok(None) => {}
                }
                if std::time::Instant::now() >= deadline {
                    failed = true;
                    break;
                }
                std::thread::sleep(std::time::Duration::from_millis(10));
            }
            if failed {
                break;
            }
        }
        if failed {
            for child in &mut children {
                let _ = child.kill();
                let _ = child.wait();
            }
            panic!("native registry contenders failed or timed out; no retry");
        }
        let outcomes = [
            fs::read(root.join("outcome-0")).unwrap(),
            fs::read(root.join("outcome-1")).unwrap(),
        ];
        assert_eq!(
            outcomes
                .iter()
                .filter(|value| *value == b"committed")
                .count(),
            1
        );
        assert_eq!(
            outcomes
                .iter()
                .filter(|value| *value == b"consumed")
                .count(),
            1
        );
    }
}
