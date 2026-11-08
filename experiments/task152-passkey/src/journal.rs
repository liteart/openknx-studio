// SPDX-License-Identifier: Apache-2.0
//! Concrete development-journal barriers; no platform success is simulated.

use crate::Error;
#[cfg(any(target_os = "macos", target_os = "linux", test))]
use std::fs::{File, OpenOptions};
#[cfg(any(target_os = "macos", target_os = "linux", test))]
use std::io::Write;
use std::path::Path;

#[cfg(any(all(windows, feature = "experimental-windows-journal"), test))]
mod registry;
#[cfg(all(windows, feature = "experimental-windows-journal"))]
mod windows;

pub(super) fn preflight(directory: &Path) -> Result<(), Error> {
    preflight_platform(directory).map_err(|error| match error {
        Error::UnsupportedJournalPlatform => error,
        _ => Error::JournalDurabilityUnavailable,
    })
}

#[cfg(all(windows, feature = "experimental-windows-journal"))]
fn preflight_platform(directory: &Path) -> Result<(), Error> {
    windows::preflight(directory)
}

#[cfg(not(all(windows, feature = "experimental-windows-journal")))]
fn preflight_platform(directory: &Path) -> Result<(), Error> {
    sync_directory(directory)
}

// The Linux fsync contract explicitly requires synchronizing the containing
// directory in addition to the file. This implementation has native macOS
// tests and Linux-container tests; native filesystem qualification remains open.
#[cfg(any(target_os = "macos", target_os = "linux"))]
fn sync_directory(directory: &Path) -> Result<(), Error> {
    File::open(directory)
        .and_then(|directory| directory.sync_all())
        .map_err(|_| Error::Io)
}

// A read-only std File is not a reviewed Windows metadata-persistence barrier.
// In particular, FlushFileBuffers requires GENERIC_WRITE. No fallback may skip
// the parent-directory barrier and release a successful observation.
#[cfg(not(any(
    target_os = "macos",
    target_os = "linux",
    all(windows, feature = "experimental-windows-journal")
)))]
fn sync_directory(_directory: &Path) -> Result<(), Error> {
    Err(Error::UnsupportedJournalPlatform)
}

pub(super) fn persist(directory: &Path, path: &Path, bytes: &[u8]) -> Result<(), Error> {
    #[cfg(all(windows, feature = "experimental-windows-journal"))]
    {
        windows::persist(directory, path, bytes)
    }
    #[cfg(any(target_os = "macos", target_os = "linux"))]
    {
        persist_with_barriers(directory, path, bytes, File::sync_all, sync_directory)
    }
    #[cfg(not(any(
        target_os = "macos",
        target_os = "linux",
        all(windows, feature = "experimental-windows-journal")
    )))]
    {
        let _ = (directory, path, bytes);
        Err(Error::UnsupportedJournalPlatform)
    }
}

#[cfg(any(target_os = "macos", target_os = "linux", test))]
fn persist_with_barriers(
    directory: &Path,
    path: &Path,
    bytes: &[u8],
    sync_file: impl FnOnce(&File) -> std::io::Result<()>,
    sync_parent: impl FnOnce(&Path) -> Result<(), Error>,
) -> Result<(), Error> {
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
        .and_then(|_| sync_file(&file))
        .map_err(|_| Error::Io)?;
    sync_parent(directory)
    // Every failure preserves the new file; it never automatically clears a
    // possibly consumed challenge. Success requires both completed barriers.
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::cell::Cell;
    use std::fs;

    #[test]
    fn file_barrier_failure_does_not_reach_directory_or_remove_consumption() {
        let directory = tempfile::tempdir().unwrap();
        let path = directory.path().join("synthetic.spent.json");
        let parent_called = Cell::new(false);
        assert_eq!(
            persist_with_barriers(
                directory.path(),
                &path,
                b"synthetic consumed challenge",
                |_| Err(std::io::Error::other("injected file barrier failure")),
                |_| {
                    parent_called.set(true);
                    Ok(())
                },
            ),
            Err(Error::Io)
        );
        assert!(!parent_called.get());
        assert_eq!(fs::read(&path).unwrap(), b"synthetic consumed challenge");
        assert_eq!(
            persist_with_barriers(
                directory.path(),
                &path,
                b"replacement",
                |_| Ok(()),
                |_| Ok(())
            ),
            Err(Error::Replay)
        );
    }

    #[test]
    fn directory_barrier_failure_follows_file_barrier_and_preserves_consumption() {
        let directory = tempfile::tempdir().unwrap();
        let path = directory.path().join("synthetic.spent.json");
        let file_synced = Cell::new(false);
        assert_eq!(
            persist_with_barriers(
                directory.path(),
                &path,
                b"synthetic consumed challenge",
                |file| {
                    file.sync_all()?;
                    file_synced.set(true);
                    Ok(())
                },
                |actual| {
                    assert!(file_synced.get());
                    assert_eq!(actual, directory.path());
                    Err(Error::Io)
                },
            ),
            Err(Error::Io)
        );
        assert!(file_synced.get());
        assert_eq!(fs::read(&path).unwrap(), b"synthetic consumed challenge");
        assert_eq!(
            persist_with_barriers(
                directory.path(),
                &path,
                b"replacement",
                |_| Ok(()),
                |_| Ok(())
            ),
            Err(Error::Replay)
        );
    }

    #[cfg(any(target_os = "macos", target_os = "linux"))]
    #[test]
    fn actual_preflight_and_persistence_use_directory_barriers_without_probe_files() {
        let directory = tempfile::tempdir().unwrap();
        preflight(directory.path()).unwrap();
        assert_eq!(fs::read_dir(directory.path()).unwrap().count(), 0);
        let path = directory.path().join("synthetic.spent.json");
        persist(directory.path(), &path, b"synthetic consumed challenge").unwrap();
        assert_eq!(fs::read(&path).unwrap(), b"synthetic consumed challenge");
        assert_eq!(
            preflight(&directory.path().join("missing-directory")),
            Err(Error::JournalDurabilityUnavailable)
        );
    }

    #[cfg(not(any(
        target_os = "macos",
        target_os = "linux",
        all(windows, feature = "experimental-windows-journal")
    )))]
    #[test]
    fn unsupported_platform_preflight_is_explicit_and_creates_nothing() {
        let directory = tempfile::tempdir().unwrap();
        assert_eq!(
            preflight(directory.path()),
            Err(Error::UnsupportedJournalPlatform)
        );
        assert_eq!(fs::read_dir(directory.path()).unwrap().count(), 0);
    }

    #[cfg(all(windows, not(feature = "experimental-windows-journal")))]
    #[test]
    fn default_windows_cannot_start_a_ceremony_or_create_consumption() {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        assert!(matches!(
            crate::DevelopmentVerifier::open(&root, "synthetic-issuer"),
            Err(Error::UnsupportedJournalPlatform)
        ));
        assert_eq!(
            persist(&root, &root.join("synthetic.spent.json"), b"synthetic"),
            Err(Error::UnsupportedJournalPlatform)
        );
        assert_eq!(fs::read_dir(&root).unwrap().count(), 0);
    }
}
