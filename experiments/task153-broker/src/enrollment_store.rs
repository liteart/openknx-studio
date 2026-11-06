// SPDX-License-Identifier: Apache-2.0
//! Private first-enrollment persistence. No public arbitrary importer.
//!
//! Only the root-peer-authenticated connection handler reaches this module.
//! Installed service protection and operator UI initiation remain prerequisites.

use crate::macos_config::{
    CONFIGURATION, MAX_CONFIGURATION_BYTES, checked_directory, ensure_no_activation_fence,
    no_granting_acl,
};
use nix::errno::Errno;
use nix::fcntl::{AtFlags, OFlag, openat};
use nix::sys::stat::{Mode, fchmod, fstatat};
use nix::unistd::{UnlinkatFlags, getegid, geteuid, unlinkat};
use serde::Serialize;
use std::fs::File;
use std::io::{Read, Write};
use std::os::unix::fs::MetadataExt;
use std::path::Path;
use webauthn_rs::prelude::Passkey;

#[derive(Debug, PartialEq, Eq)]
pub(super) enum StoreError {
    OperatorProvisioningUnavailable,
    InvalidRecord,
    AlreadyExistsOrUnavailable,
    FileRejected,
    Io,
    Busy,
    ActivationUncertain,
}

#[derive(Serialize)]
struct EnrollmentRecord<'a> {
    schema: u8,
    kind: &'static str,
    issuer: &'a str,
    credential: &'a Passkey,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Stage {
    Created,
    Written,
    FileSynced,
    DirectorySynced,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum ActivationStage {
    FenceDurable,
    Renamed,
    CommitSynced,
    FenceRemoved,
}

#[derive(Debug, PartialEq, Eq)]
pub(super) enum ActivationOutcome {
    Activated,
    CommittedCleanupUncertain,
}

struct BoundedRecord(Vec<u8>);

impl Write for BoundedRecord {
    fn write(&mut self, bytes: &[u8]) -> std::io::Result<usize> {
        if self.0.len().saturating_add(bytes.len()) > MAX_CONFIGURATION_BYTES {
            return Err(std::io::Error::new(
                std::io::ErrorKind::InvalidData,
                "record too large",
            ));
        }
        self.0.extend_from_slice(bytes);
        Ok(bytes.len())
    }

    fn flush(&mut self) -> std::io::Result<()> {
        Ok(())
    }
}

fn encode_record(issuer: &str, credential: &Passkey) -> Result<Vec<u8>, StoreError> {
    if issuer.is_empty()
        || issuer.len() > 128
        || !issuer
            .bytes()
            .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
    {
        return Err(StoreError::InvalidRecord);
    }
    let record = EnrollmentRecord {
        schema: 1,
        kind: "operator",
        issuer,
        credential,
    };
    let mut output = BoundedRecord(Vec::new());
    serde_json::to_writer(&mut output, &record).map_err(|_| StoreError::InvalidRecord)?;
    Ok(output.0)
}

/// Dormant root-only wrapper. A future caller must supply only the result of
/// its explicit protected WebAuthn operator registration, never imported keys.
pub(super) fn persist_pending_enrollment(
    issuer: &str,
    credential: &Passkey,
) -> Result<(), StoreError> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(StoreError::OperatorProvisioningUnavailable);
    }
    let path = Path::new(CONFIGURATION);
    let parent = checked_directory(
        path.parent()
            .ok_or(StoreError::OperatorProvisioningUnavailable)?,
    )
    .map_err(|_| StoreError::OperatorProvisioningUnavailable)?;
    let bytes = encode_record(issuer, credential)?;
    create_new_record(&parent, &bytes, |_| Ok(()))
}

fn create_new_record<F: FnMut(Stage) -> Result<(), StoreError>>(
    parent: &File,
    bytes: &[u8],
    mut stage: F,
) -> Result<(), StoreError> {
    if bytes.is_empty() || bytes.len() > MAX_CONFIGURATION_BYTES {
        return Err(StoreError::InvalidRecord);
    }
    let _lock = exclusive_store_lock(parent)?;
    ensure_no_activation_fence(parent).map_err(|_| StoreError::ActivationUncertain)?;
    match fstatat(parent, "config.json", AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(Errno::ENOENT) => (),
        _ => return Err(StoreError::AlreadyExistsOrUnavailable),
    }
    // The fixed pending leaf is never read as active config.json. Even a fully
    // written but unacknowledged pending file cannot activate an enrollment.
    // Fixed leaf and create-new: neither a symlink nor an existing record is
    // opened, truncated, chmodded, replaced or removed. Parent is fd-relative.
    let mut file = openat(
        parent,
        "enrollment.pending.json",
        OFlag::O_WRONLY | OFlag::O_CREAT | OFlag::O_EXCL | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC,
        Mode::S_IRUSR | Mode::S_IWUSR,
    )
    .map(File::from)
    .map_err(|_| StoreError::AlreadyExistsOrUnavailable)?;
    fchmod(&file, Mode::S_IRUSR | Mode::S_IWUSR).map_err(|_| StoreError::FileRejected)?;
    let metadata = file.metadata().map_err(|_| StoreError::Io)?;
    if !metadata.is_file()
        || metadata.uid() != geteuid().as_raw()
        || metadata.nlink() != 1
        || metadata.mode() & 0o777 != 0o600
    {
        return Err(StoreError::FileRejected);
    }
    no_granting_acl(&file).map_err(|_| StoreError::FileRejected)?;
    stage(Stage::Created)?;
    file.write_all(bytes).map_err(|_| StoreError::Io)?;
    stage(Stage::Written)?;
    file.sync_all().map_err(|_| StoreError::Io)?;
    stage(Stage::FileSynced)?;
    parent.sync_all().map_err(|_| StoreError::Io)?;
    stage(Stage::DirectorySynced)?;
    // The code never removes a failed pending file. Before directory sync, its
    // name may still be lost on power failure; this is not a durable lock.
    Ok(())
}

fn validate_private_file(file: &File) -> Result<(), StoreError> {
    let metadata = file.metadata().map_err(|_| StoreError::Io)?;
    if !metadata.is_file()
        || metadata.uid() != geteuid().as_raw()
        || metadata.nlink() != 1
        || metadata.mode() & 0o777 != 0o600
    {
        return Err(StoreError::FileRejected);
    }
    no_granting_acl(file).map_err(|_| StoreError::FileRejected)
}

fn exclusive_store_lock(parent: &File) -> Result<File, StoreError> {
    let file = match openat(
        parent,
        "enrollment.lock",
        OFlag::O_RDWR | OFlag::O_CREAT | OFlag::O_EXCL | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC,
        Mode::S_IRUSR | Mode::S_IWUSR,
    ) {
        Ok(fd) => {
            let file = File::from(fd);
            fchmod(&file, Mode::S_IRUSR | Mode::S_IWUSR).map_err(|_| StoreError::FileRejected)?;
            file
        }
        Err(Errno::EEXIST) => openat(
            parent,
            "enrollment.lock",
            OFlag::O_RDWR | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC | OFlag::O_NONBLOCK,
            Mode::empty(),
        )
        .map(File::from)
        .map_err(|_| StoreError::FileRejected)?,
        Err(_) => return Err(StoreError::Io),
    };
    validate_private_file(&file)?;
    if file.metadata().map_err(|_| StoreError::Io)?.len() != 0 {
        return Err(StoreError::FileRejected);
    }
    rustix::fs::flock(&file, rustix::fs::FlockOperation::NonBlockingLockExclusive)
        .map_err(|_| StoreError::Busy)?;
    file.sync_all().map_err(|_| StoreError::Io)?;
    parent.sync_all().map_err(|_| StoreError::Io)?;
    Ok(file)
}

fn named_inode_matches(parent: &File, name: &str, file: &File) -> bool {
    let Ok(named) = fstatat(parent, name, AtFlags::AT_SYMLINK_NOFOLLOW) else {
        return false;
    };
    let Ok(opened) = file.metadata() else {
        return false;
    };
    u64::try_from(named.st_dev).ok() == Some(opened.dev())
        && named.st_ino == opened.ino()
        && (named.st_mode & nix::libc::S_IFMT) == nix::libc::S_IFREG
}

/// Dormant activation only; a future protected ceremony must supply the exact
/// same public record that it registered and staged, never an imported record.
pub(super) fn activate_first_enrollment(
    issuer: &str,
    credential: &Passkey,
) -> Result<ActivationOutcome, StoreError> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(StoreError::OperatorProvisioningUnavailable);
    }
    let path = Path::new(CONFIGURATION);
    let parent = checked_directory(
        path.parent()
            .ok_or(StoreError::OperatorProvisioningUnavailable)?,
    )
    .map_err(|_| StoreError::OperatorProvisioningUnavailable)?;
    activate_pending(&parent, &encode_record(issuer, credential)?, |_| Ok(()))
}

fn activate_pending<F: FnMut(ActivationStage) -> Result<(), StoreError>>(
    parent: &File,
    expected: &[u8],
    mut stage: F,
) -> Result<ActivationOutcome, StoreError> {
    if expected.is_empty() || expected.len() > MAX_CONFIGURATION_BYTES {
        return Err(StoreError::InvalidRecord);
    }
    let _lock = exclusive_store_lock(parent)?;
    ensure_no_activation_fence(parent).map_err(|_| StoreError::ActivationUncertain)?;
    match fstatat(parent, "config.json", AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(Errno::ENOENT) => (),
        _ => return Err(StoreError::AlreadyExistsOrUnavailable),
    }
    let pending = openat(
        parent,
        "enrollment.pending.json",
        OFlag::O_RDONLY | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC | OFlag::O_NONBLOCK,
        Mode::empty(),
    )
    .map(File::from)
    .map_err(|_| StoreError::FileRejected)?;
    validate_private_file(&pending)?;
    let mut bytes = Vec::new();
    (&pending)
        .take((MAX_CONFIGURATION_BYTES + 1) as u64)
        .read_to_end(&mut bytes)
        .map_err(|_| StoreError::Io)?;
    if bytes != expected || !named_inode_matches(parent, "enrollment.pending.json", &pending) {
        return Err(StoreError::InvalidRecord);
    }
    pending.sync_all().map_err(|_| StoreError::Io)?;

    // A durable fence must precede any publication. Readers reject it under
    // their shared lock, including after a crash or failed directory sync.
    let mut fence = openat(
        parent,
        "activation.pending",
        OFlag::O_WRONLY | OFlag::O_CREAT | OFlag::O_EXCL | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC,
        Mode::S_IRUSR | Mode::S_IWUSR,
    )
    .map(File::from)
    .map_err(|_| StoreError::ActivationUncertain)?;
    fchmod(&fence, Mode::S_IRUSR | Mode::S_IWUSR).map_err(|_| StoreError::ActivationUncertain)?;
    validate_private_file(&fence).map_err(|_| StoreError::ActivationUncertain)?;
    fence
        .write_all(b"activation-uncommitted-v1\n")
        .map_err(|_| StoreError::ActivationUncertain)?;
    fence
        .sync_all()
        .map_err(|_| StoreError::ActivationUncertain)?;
    parent
        .sync_all()
        .map_err(|_| StoreError::ActivationUncertain)?;
    stage(ActivationStage::FenceDurable).map_err(|_| StoreError::ActivationUncertain)?;
    rustix::fs::renameat_with(
        parent,
        "enrollment.pending.json",
        parent,
        "config.json",
        rustix::fs::RenameFlags::NOREPLACE,
    )
    .map_err(|_| StoreError::ActivationUncertain)?;
    stage(ActivationStage::Renamed).map_err(|_| StoreError::ActivationUncertain)?;
    if !named_inode_matches(parent, "config.json", &pending) {
        return Err(StoreError::ActivationUncertain);
    }
    parent
        .sync_all()
        .map_err(|_| StoreError::ActivationUncertain)?;
    // The enrolled record is now committed. Later cleanup cannot honestly be
    // reported as no effect, even if the fence remains or its removal is lost.
    if stage(ActivationStage::CommitSynced).is_err()
        || !named_inode_matches(parent, "activation.pending", &fence)
        || unlinkat(parent, "activation.pending", UnlinkatFlags::NoRemoveDir).is_err()
        || stage(ActivationStage::FenceRemoved).is_err()
        || parent.sync_all().is_err()
    {
        return Ok(ActivationOutcome::CommittedCleanupUncertain);
    }
    Ok(ActivationOutcome::Activated)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;
    use std::os::unix::fs::{PermissionsExt, symlink};
    use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};
    use webauthn_rs::prelude::*;

    fn fixture() -> (tempfile::TempDir, File) {
        let directory = tempfile::Builder::new()
            .prefix("task153-store-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let parent = File::open(directory.path()).unwrap();
        (directory, parent)
    }

    fn synthetic_credential() -> Passkey {
        let origin = Url::parse(crate::root_passkey::OPERATOR_ORIGIN).unwrap();
        let webauthn = WebauthnBuilder::new("localhost", &origin)
            .unwrap()
            .build()
            .unwrap();
        let (options, state) = webauthn
            .start_passkey_registration(Uuid::new_v4(), "synthetic", "Synthetic operator", None)
            .unwrap();
        let mut authenticator = SoftPasskey::new(true);
        let response = authenticator.do_registration(origin, options).unwrap();
        webauthn
            .finish_passkey_registration(&response, &state)
            .unwrap()
    }

    fn synthetic_record() -> Vec<u8> {
        encode_record("synthetic", &synthetic_credential()).unwrap()
    }

    #[test]
    fn actual_create_new_record_syncs_file_then_directory_and_never_overwrites() {
        let (directory, parent) = fixture();
        let synthetic = synthetic_record();
        let mut stages = Vec::new();
        create_new_record(&parent, &synthetic, |stage| {
            stages.push(stage);
            Ok(())
        })
        .unwrap();
        assert_eq!(
            stages,
            [
                Stage::Created,
                Stage::Written,
                Stage::FileSynced,
                Stage::DirectorySynced
            ]
        );
        let path = directory.path().join("enrollment.pending.json");
        assert_eq!(fs::read(&path).unwrap(), synthetic);
        assert_eq!(fs::metadata(&path).unwrap().mode() & 0o777, 0o600);
        assert_eq!(
            create_new_record(&parent, b"replacement", |_| Ok(())),
            Err(StoreError::AlreadyExistsOrUnavailable)
        );
        assert_eq!(fs::read(path).unwrap(), synthetic);
    }

    #[test]
    fn existing_symlink_is_not_followed_or_replaced() {
        let (directory, parent) = fixture();
        let target = directory.path().join("target");
        fs::write(&target, b"keep target").unwrap();
        fs::set_permissions(&target, fs::Permissions::from_mode(0o640)).unwrap();
        symlink("target", directory.path().join("enrollment.pending.json")).unwrap();
        assert_eq!(
            create_new_record(&parent, &synthetic_record(), |_| Ok(())),
            Err(StoreError::AlreadyExistsOrUnavailable)
        );
        assert_eq!(fs::read(target.clone()).unwrap(), b"keep target");
        assert_eq!(fs::metadata(target).unwrap().mode() & 0o777, 0o640);
    }

    #[test]
    fn existing_active_configuration_prevents_staging_without_changes() {
        let (directory, parent) = fixture();
        let active = directory.path().join("config.json");
        fs::write(&active, b"existing active enrollment").unwrap();
        assert_eq!(
            create_new_record(&parent, &synthetic_record(), |_| Ok(())),
            Err(StoreError::AlreadyExistsOrUnavailable)
        );
        assert_eq!(fs::read(active).unwrap(), b"existing active enrollment");
        assert!(!directory.path().join("enrollment.pending.json").exists());
    }

    #[test]
    fn injected_interruption_keeps_quarantine_at_each_persistence_stage() {
        for fail_at in [Stage::Created, Stage::Written, Stage::FileSynced] {
            let (directory, parent) = fixture();
            let synthetic = synthetic_record();
            let mut observed = Vec::new();
            assert_eq!(
                create_new_record(&parent, &synthetic, |stage| {
                    observed.push(stage);
                    if stage == fail_at {
                        Err(StoreError::Io)
                    } else {
                        Ok(())
                    }
                }),
                Err(StoreError::Io)
            );
            assert_eq!(observed.last(), Some(&fail_at));
            assert!(!observed.contains(&Stage::DirectorySynced));
            let path = directory.path().join("enrollment.pending.json");
            assert!(path.is_file());
            assert_eq!(
                create_new_record(&parent, &synthetic, |_| Ok(())),
                Err(StoreError::AlreadyExistsOrUnavailable)
            );
        }
    }

    #[test]
    fn empty_or_oversized_input_creates_no_file() {
        let (directory, parent) = fixture();
        for bytes in [Vec::new(), vec![0; MAX_CONFIGURATION_BYTES + 1]] {
            assert_eq!(
                create_new_record(&parent, &bytes, |_| Ok(())),
                Err(StoreError::InvalidRecord)
            );
        }
        assert!(!directory.path().join("enrollment.pending.json").exists());
    }

    #[test]
    fn renamed_parent_does_not_redirect_descriptor_relative_write() {
        let temporary = tempfile::Builder::new()
            .prefix("task153-parent-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let path = temporary.path().join("store");
        let moved = temporary.path().join("original");
        fs::create_dir(&path).unwrap();
        let parent = File::open(&path).unwrap();
        fs::rename(&path, &moved).unwrap();
        fs::create_dir(&path).unwrap();
        let synthetic = synthetic_record();
        create_new_record(&parent, &synthetic, |_| Ok(())).unwrap();
        assert_eq!(
            fs::read(moved.join("enrollment.pending.json")).unwrap(),
            synthetic
        );
        assert!(!path.join("enrollment.pending.json").exists());
    }

    #[test]
    fn actual_unprivileged_process_cannot_persist_operator_enrollment() {
        if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
            assert_eq!(
                persist_pending_enrollment("synthetic", &synthetic_credential()),
                Err(StoreError::OperatorProvisioningUnavailable)
            );
        }
    }

    #[test]
    fn atomic_activation_publishes_exact_staged_record_and_clears_durable_fence() {
        let (directory, parent) = fixture();
        let bytes = synthetic_record();
        create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
        assert_eq!(
            activate_pending(&parent, &bytes, |_| Ok(())),
            Ok(ActivationOutcome::Activated)
        );
        assert_eq!(
            fs::read(directory.path().join("config.json")).unwrap(),
            bytes
        );
        assert!(!directory.path().join("enrollment.pending.json").exists());
        assert_eq!(ensure_no_activation_fence(&parent), Ok(()));
    }

    #[test]
    fn changed_record_and_existing_config_cannot_be_activated() {
        let (directory, parent) = fixture();
        let bytes = synthetic_record();
        create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
        assert_eq!(
            activate_pending(&parent, b"wrong record", |_| Ok(())),
            Err(StoreError::InvalidRecord)
        );
        fs::write(directory.path().join("config.json"), b"keep active").unwrap();
        assert_eq!(
            activate_pending(&parent, &bytes, |_| Ok(())),
            Err(StoreError::AlreadyExistsOrUnavailable)
        );
        assert_eq!(
            fs::read(directory.path().join("config.json")).unwrap(),
            b"keep active"
        );
        assert!(directory.path().join("enrollment.pending.json").exists());
    }

    #[test]
    fn precommit_interruption_keeps_reader_fence_across_reopen() {
        for fail_at in [ActivationStage::FenceDurable, ActivationStage::Renamed] {
            let (directory, parent) = fixture();
            let bytes = synthetic_record();
            create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
            assert_eq!(
                activate_pending(&parent, &bytes, |stage| if stage == fail_at {
                    Err(StoreError::Io)
                } else {
                    Ok(())
                }),
                Err(StoreError::ActivationUncertain)
            );
            drop(parent);
            let reopened = File::open(directory.path()).unwrap();
            assert_eq!(
                ensure_no_activation_fence(&reopened),
                Err(crate::macos_config::ConfigError::ActivationUncertain)
            );
            assert_eq!(
                activate_pending(&reopened, &bytes, |_| Ok(())),
                Err(StoreError::ActivationUncertain)
            );
        }
    }

    #[test]
    fn postcommit_cleanup_fault_is_reported_as_committed_not_no_effect() {
        for fail_at in [ActivationStage::CommitSynced, ActivationStage::FenceRemoved] {
            let (directory, parent) = fixture();
            let bytes = synthetic_record();
            create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
            assert_eq!(
                activate_pending(&parent, &bytes, |stage| if stage == fail_at {
                    Err(StoreError::Io)
                } else {
                    Ok(())
                }),
                Ok(ActivationOutcome::CommittedCleanupUncertain)
            );
            assert_eq!(
                fs::read(directory.path().join("config.json")).unwrap(),
                bytes
            );
            assert_eq!(
                directory.path().join("activation.pending").exists(),
                fail_at == ActivationStage::CommitSynced
            );
        }
    }

    #[test]
    fn kernel_noreplace_rejects_destination_created_after_validation() {
        let (directory, parent) = fixture();
        let bytes = synthetic_record();
        create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
        let active = directory.path().join("config.json");
        assert_eq!(
            activate_pending(&parent, &bytes, |stage| {
                if stage == ActivationStage::FenceDurable {
                    fs::write(&active, b"racing destination").unwrap();
                }
                Ok(())
            }),
            Err(StoreError::ActivationUncertain)
        );
        assert_eq!(fs::read(active).unwrap(), b"racing destination");
        assert!(directory.path().join("enrollment.pending.json").exists());
    }

    #[test]
    fn shared_reader_lock_blocks_activation_before_any_fence() {
        let (directory, parent) = fixture();
        let bytes = synthetic_record();
        create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
        let shared = File::open(directory.path().join("enrollment.lock")).unwrap();
        rustix::fs::flock(&shared, rustix::fs::FlockOperation::NonBlockingLockShared).unwrap();
        assert_eq!(
            activate_pending(&parent, &bytes, |_| Ok(())),
            Err(StoreError::Busy)
        );
        assert!(!directory.path().join("activation.pending").exists());
        drop(shared);
        assert_eq!(
            activate_pending(&parent, &bytes, |_| Ok(())),
            Ok(ActivationOutcome::Activated)
        );
    }

    #[test]
    fn substituted_pending_inode_is_quarantined_after_rename() {
        let (directory, parent) = fixture();
        let bytes = synthetic_record();
        create_new_record(&parent, &bytes, |_| Ok(())).unwrap();
        assert_eq!(
            activate_pending(&parent, &bytes, |stage| {
                if stage == ActivationStage::FenceDurable {
                    fs::rename(
                        directory.path().join("enrollment.pending.json"),
                        directory.path().join("original"),
                    )
                    .unwrap();
                    fs::write(
                        directory.path().join("enrollment.pending.json"),
                        b"substituted inode",
                    )
                    .unwrap();
                }
                Ok(())
            }),
            Err(StoreError::ActivationUncertain)
        );
        assert_eq!(
            ensure_no_activation_fence(&parent),
            Err(crate::macos_config::ConfigError::ActivationUncertain)
        );
    }
}
