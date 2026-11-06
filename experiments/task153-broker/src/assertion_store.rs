// SPDX-License-Identifier: Apache-2.0
//! One-shot protected assertion/counter ledger. No grant loader or executor.
use crate::enrollment_store::exclusive_store_lock;
use crate::macos_config::{
    CONFIGURATION, MAX_CONFIGURATION_BYTES, checked_directory, ensure_no_activation_fence,
    no_granting_acl, read_checked_leaf,
};
use crate::root_passkey::VerifiedAssertionRecord;
use nix::errno::Errno;
use nix::fcntl::{AtFlags, OFlag, openat};
use nix::sys::stat::{Mode, fchmod, fstatat};
use nix::unistd::{getegid, geteuid};
use sha2::{Digest, Sha256};
use std::ffi::OsStr;
use std::fs::File;
use std::io::Write;
use std::os::unix::fs::MetadataExt;
use std::path::Path;

const RECORD: &str = "verified-assertion.json";
#[derive(Debug, PartialEq, Eq)]
pub(super) enum Error {
    RootRequired,
    Unavailable,
    AlreadyConsumed,
    ConfigurationChanged,
    RecordRejected,
    Io,
}
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Stage {
    Created,
    ReservationDurable,
    Written,
    FileSynced,
    Committed,
}

fn root_parent() -> Result<File, Error> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    checked_directory(
        Path::new(CONFIGURATION)
            .parent()
            .ok_or(Error::Unavailable)?,
    )
    .map_err(|_| Error::Unavailable)
}
fn ensure_unused(parent: &File) -> Result<(), Error> {
    match fstatat(parent, RECORD, AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(Errno::ENOENT) => Ok(()),
        _ => Err(Error::AlreadyConsumed),
    }
}
pub(super) fn ensure_available() -> Result<(), Error> {
    let parent = root_parent()?;
    ensure_no_activation_fence(&parent).map_err(|_| Error::Unavailable)?;
    ensure_unused(&parent)
}
pub(super) fn persist(record: &VerifiedAssertionRecord<'_>) -> Result<(), Error> {
    let parent = root_parent()?;
    let _lock = exclusive_store_lock(&parent).map_err(|_| Error::Unavailable)?;
    ensure_no_activation_fence(&parent).map_err(|_| Error::Unavailable)?;
    ensure_unused(&parent)?;
    let snapshot =
        read_checked_leaf(&parent, OsStr::new("config.json")).map_err(|_| Error::Unavailable)?;
    let digest: [u8; 32] = Sha256::digest(snapshot.as_bytes()).into();
    if digest != record.enrollment_digest() {
        return Err(Error::ConfigurationChanged);
    }
    let bytes = serde_json::to_vec(record).map_err(|_| Error::RecordRejected)?;
    create_record(&parent, &bytes, |_| Ok(()))
}
fn create_record<F: FnMut(Stage) -> Result<(), Error>>(
    parent: &File,
    bytes: &[u8],
    mut stage: F,
) -> Result<(), Error> {
    if bytes.is_empty() || bytes.len() > MAX_CONFIGURATION_BYTES {
        return Err(Error::RecordRejected);
    }
    ensure_unused(parent)?;
    let mut file = openat(
        parent,
        RECORD,
        OFlag::O_WRONLY | OFlag::O_CREAT | OFlag::O_EXCL | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC,
        Mode::S_IRUSR | Mode::S_IWUSR,
    )
    .map(File::from)
    .map_err(|_| Error::AlreadyConsumed)?;
    fchmod(&file, Mode::S_IRUSR | Mode::S_IWUSR).map_err(|_| Error::RecordRejected)?;
    let metadata = file.metadata().map_err(|_| Error::Io)?;
    if !metadata.is_file()
        || metadata.uid() != geteuid().as_raw()
        || metadata.mode() & 0o7777 != 0o600
        || metadata.nlink() != 1
    {
        return Err(Error::RecordRejected);
    }
    no_granting_acl(&file).map_err(|_| Error::RecordRejected)?;
    stage(Stage::Created)?;
    // First persist the empty reservation. Partial/empty records block future
    // use; before directory sync a power failure may lose the name, but no
    // successful observation/capability has been issued at that point.
    file.sync_all()
        .and_then(|_| parent.sync_all())
        .map_err(|_| Error::Io)?;
    stage(Stage::ReservationDurable)?;
    file.write_all(bytes).map_err(|_| Error::Io)?;
    stage(Stage::Written)?;
    file.sync_all().map_err(|_| Error::Io)?;
    stage(Stage::FileSynced)?;
    parent.sync_all().map_err(|_| Error::Io)?;
    stage(Stage::Committed)
}

#[cfg(test)]
mod tests;

#[cfg(test)]
pub(super) fn store_synthetic_record_for_test(parent: &File, bytes: &[u8]) -> Result<(), Error> {
    // Actual owned fixture persistence only. This creates no RootOwnedSnapshot,
    // protected-principal proof, approval or dispatch capability.
    create_record(parent, bytes, |_| Ok(()))
}
