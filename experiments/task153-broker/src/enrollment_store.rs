// SPDX-License-Identifier: Apache-2.0
//! Dormant private first-enrollment persistence. No public importer or ceremony.
//!
//! This is deliberately not connected to the broker until protected operator
//! initiation, enrollment UI and service provisioning have been established.
#![allow(dead_code)]

use crate::macos_config::{
    CONFIGURATION, MAX_CONFIGURATION_BYTES, checked_directory, no_granting_acl,
};
use nix::errno::Errno;
use nix::fcntl::{AtFlags, OFlag, openat};
use nix::sys::stat::{Mode, fchmod, fstatat};
use nix::unistd::{getegid, geteuid};
use serde::Serialize;
use std::fs::File;
use std::io::Write;
use std::os::unix::fs::MetadataExt;
use std::path::Path;
use webauthn_rs::prelude::Passkey;

#[derive(Debug, PartialEq, Eq)]
enum StoreError {
    OperatorProvisioningUnavailable,
    InvalidRecord,
    AlreadyExistsOrUnavailable,
    FileRejected,
    Io,
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
fn persist_pending_enrollment(issuer: &str, credential: &Passkey) -> Result<(), StoreError> {
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
}
