// SPDX-License-Identifier: Apache-2.0
//! One-use registry commit policy, with native operations supplied only on Windows.

use crate::Error;
use std::path::Path;

pub(super) const MAX_RECORD_BYTES: usize = 2048;

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub(super) enum Disposition {
    New,
    Existing,
}

pub(super) trait RecordKey {
    fn flush(&self) -> Result<(), Error>;
    fn set_receipt(&self, bytes: &[u8]) -> Result<(), Error>;
}

pub(super) trait RegistryJournal {
    type Key: RecordKey;

    fn create_record(&self, challenge: &str) -> Result<(Self::Key, Disposition), Error>;
}

pub(super) fn record_name(directory: &Path, path: &Path) -> Result<String, Error> {
    if path.parent() != Some(directory) {
        return Err(Error::Io);
    }
    let challenge = path
        .file_name()
        .and_then(|name| name.to_str())
        .and_then(|name| name.strip_suffix(".spent.json"))
        .ok_or(Error::Io)?;
    if !valid_challenge(challenge) {
        return Err(Error::Io);
    }
    Ok(challenge.into())
}

pub(super) fn valid_challenge(challenge: &str) -> bool {
    challenge.len() == 64
        && challenge
            .bytes()
            .all(|byte| byte.is_ascii_digit() || (b'a'..=b'f').contains(&byte))
}

pub(super) fn commit(
    registry: &impl RegistryJournal,
    challenge: &str,
    bytes: &[u8],
    write_file: impl FnOnce() -> Result<(), Error>,
) -> Result<(), Error> {
    if !valid_challenge(challenge) || bytes.is_empty() || bytes.len() > MAX_RECORD_BYTES {
        return Err(Error::Io);
    }
    // RegCreateKeyExW supplies the disposition from the create/open operation;
    // there is no separate existence check or overwrite of an existing value.
    let (key, disposition) = registry.create_record(challenge)?;
    if disposition != Disposition::New {
        // Missing, malformed and partial values are all already consumed. Do
        // not read them, clear them, or decide that a fresh attempt is safe.
        return Err(Error::Replay);
    }
    // Persist the reservation before writing the portable JSON copy. Failure
    // at any later point retains the key; the production code never deletes it.
    key.flush()?;
    write_file()?;
    key.set_receipt(bytes)?;
    // RegFlushKey covers the key and value in the hive. Receipt success is not
    // released merely because the value became visible to another process.
    key.flush()
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::{Arc, Barrier, Mutex};

    #[derive(Clone)]
    struct FakeRegistry(Arc<Mutex<State>>);

    #[derive(Default)]
    struct State {
        exists: bool,
        operations: Vec<&'static str>,
        flushes: usize,
        fail_at: Option<&'static str>,
    }

    impl RegistryJournal for FakeRegistry {
        type Key = Self;

        fn create_record(&self, _challenge: &str) -> Result<(Self, Disposition), Error> {
            let mut state = self.0.lock().unwrap();
            state.operations.push("create");
            if state.fail_at == Some("create") {
                return Err(Error::Io);
            }
            let disposition = if state.exists {
                Disposition::Existing
            } else {
                state.exists = true;
                Disposition::New
            };
            Ok((self.clone(), disposition))
        }
    }

    impl RecordKey for FakeRegistry {
        fn flush(&self) -> Result<(), Error> {
            let mut state = self.0.lock().unwrap();
            state.flushes += 1;
            let stage = if state.flushes == 1 {
                "reserve"
            } else {
                "commit"
            };
            state.operations.push(stage);
            if state.fail_at == Some(stage) {
                Err(Error::Io)
            } else {
                Ok(())
            }
        }

        fn set_receipt(&self, bytes: &[u8]) -> Result<(), Error> {
            assert!(!bytes.is_empty() && bytes.len() <= MAX_RECORD_BYTES);
            let mut state = self.0.lock().unwrap();
            state.operations.push("set");
            if state.fail_at == Some("set") {
                Err(Error::Io)
            } else {
                Ok(())
            }
        }
    }

    fn fixture(fail_at: Option<&'static str>) -> FakeRegistry {
        FakeRegistry(Arc::new(Mutex::new(State {
            fail_at,
            ..State::default()
        })))
    }

    #[test]
    fn only_a_new_reserved_written_and_flushed_record_can_complete() {
        let registry = fixture(None);
        commit(&registry, &"a".repeat(64), b"synthetic receipt", || {
            registry.0.lock().unwrap().operations.push("file");
            Ok(())
        })
        .unwrap();
        assert_eq!(
            registry.0.lock().unwrap().operations,
            ["create", "reserve", "file", "set", "commit"]
        );
        assert_eq!(
            commit(&registry, &"a".repeat(64), b"different", || panic!(
                "replay wrote file"
            )),
            Err(Error::Replay)
        );
    }

    #[test]
    fn every_injected_failure_withholds_success_and_preserves_any_reservation() {
        for stage in ["create", "reserve", "file", "set", "commit"] {
            let registry = fixture(Some(stage));
            assert_eq!(
                commit(&registry, &"a".repeat(64), b"synthetic receipt", || {
                    registry.0.lock().unwrap().operations.push("file");
                    if stage == "file" {
                        Err(Error::Io)
                    } else {
                        Ok(())
                    }
                }),
                Err(Error::Io)
            );
            let expected = match stage {
                "create" => vec!["create"],
                "reserve" => vec!["create", "reserve"],
                "file" => vec!["create", "reserve", "file"],
                "set" => vec!["create", "reserve", "file", "set"],
                _ => vec!["create", "reserve", "file", "set", "commit"],
            };
            assert_eq!(registry.0.lock().unwrap().operations, expected);
            if stage != "create" {
                assert_eq!(
                    commit(&registry, &"a".repeat(64), b"retry", || panic!(
                        "partial key reused"
                    )),
                    Err(Error::Replay)
                );
            }
        }
    }

    #[test]
    fn concurrent_creators_have_at_most_one_commit_winner() {
        let registry = fixture(None);
        let barrier = Arc::new(Barrier::new(2));
        let outcomes = std::thread::scope(|scope| {
            let handles: Vec<_> = (0..2)
                .map(|_| {
                    let registry = registry.clone();
                    let barrier = barrier.clone();
                    scope.spawn(move || {
                        barrier.wait();
                        commit(&registry, &"a".repeat(64), b"synthetic receipt", || Ok(()))
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
    fn record_names_and_values_are_bounded_before_registry_creation() {
        let directory = Path::new("synthetic-journal");
        let challenge = "a".repeat(64);
        assert_eq!(
            record_name(
                directory,
                &directory.join(format!("{challenge}.spent.json"))
            )
            .unwrap(),
            challenge
        );
        for path in [
            "../outside.spent.json",
            "ABC.spent.json",
            "a.spent.json",
            "sub/aaaaaaaa.spent.json",
        ] {
            assert_eq!(
                record_name(directory, &directory.join(path)),
                Err(Error::Io)
            );
        }
        for bytes in [Vec::new(), vec![0; MAX_RECORD_BYTES + 1]] {
            let registry = fixture(None);
            assert_eq!(
                commit(&registry, &"a".repeat(64), &bytes, || panic!(
                    "invalid value wrote file"
                )),
                Err(Error::Io)
            );
            assert!(registry.0.lock().unwrap().operations.is_empty());
        }
    }
}
