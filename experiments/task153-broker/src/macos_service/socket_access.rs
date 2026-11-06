// SPDX-License-Identifier: Apache-2.0
//! UID-specific socket ACL. Caller first validates the root-owned parent.

use super::Error;
use exacl::{AclEntry, AclOption, Perm, getfacl, setfacl};
use nix::unistd::geteuid;
use std::fs;
use std::os::unix::fs::{FileTypeExt, MetadataExt};
use std::path::Path;

fn socket_stamp(path: &Path) -> Result<(u64, u64, u32), Error> {
    let metadata = fs::symlink_metadata(path).map_err(|_| Error::EndpointUnavailable)?;
    if !metadata.file_type().is_socket()
        || metadata.uid() != geteuid().as_raw()
        || metadata.mode() & 0o7777 != 0o600
        || metadata.nlink() != 1
    {
        return Err(Error::EndpointUnavailable);
    }
    Ok((metadata.dev(), metadata.ino(), metadata.uid()))
}

pub(super) fn valid_acl_uuid(value: &str) -> bool {
    let bytes = value.as_bytes();
    bytes.len() == 38
        && bytes[0] == b'{'
        && bytes[37] == b'}'
        && bytes[1..37].iter().enumerate().all(|(index, byte)| {
            if [8, 13, 18, 23].contains(&index) {
                *byte == b'-'
            } else {
                byte.is_ascii_digit() || (b'a'..=b'f').contains(byte)
            }
        })
        && bytes[1..37]
            .iter()
            .any(|byte| matches!(byte, b'1'..=b'9' | b'a'..=b'f'))
}

pub(super) fn restrict_socket(
    path: &Path,
    operator_uid: u32,
    expected_uuid: &str,
) -> Result<(), Error> {
    if !valid_acl_uuid(expected_uuid) {
        return Err(Error::OperatorPolicyRejected);
    }
    let before = socket_stamp(path)?;
    // Decimal input is parsed directly as a UID by exacl, then converted by
    // mbr_uid_to_uuid. Never resolve an operator name or compare name aliases.
    let entries = [AclEntry::allow_user(
        &operator_uid.to_string(),
        Perm::WRITE,
        None,
    )];
    let expected = [AclEntry::allow_user(expected_uuid, Perm::WRITE, None)];
    let options = Some(AclOption::SYMLINK_ACL);
    // No-follow applies even if a path is replaced with a symlink. In service
    // use the previously validated root-only writable parent forbids ordinary
    // replacement; this is not a general arbitrary-path ACL writer.
    setfacl(&[path], &entries, options).map_err(|_| Error::EndpointUnavailable)?;
    let actual = getfacl(path, Some(AclOption::SYMLINK_ACL | AclOption::NATIVE_ID))
        .map_err(|_| Error::EndpointUnavailable)?;
    if actual != expected || socket_stamp(path)? != before {
        return Err(Error::EndpointUnavailable);
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::os::unix::fs::{PermissionsExt, symlink};
    use std::os::unix::net::{UnixListener, UnixStream};

    #[test]
    fn actual_socket_has_only_selected_uid_write_acl_and_still_connects() {
        let directory = tempfile::Builder::new()
            .prefix("t153-acl-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let path = directory.path().join("s");
        let listener = UnixListener::bind(&path).unwrap();
        fs::set_permissions(&path, fs::Permissions::from_mode(0o600)).unwrap();
        // Derive a pin only for this owned fixture, never a protected manifest.
        setfacl(
            &[&path],
            &[AclEntry::allow_user(
                &geteuid().as_raw().to_string(),
                Perm::WRITE,
                None,
            )],
            Some(AclOption::SYMLINK_ACL),
        )
        .unwrap();
        let options = Some(AclOption::SYMLINK_ACL | AclOption::NATIVE_ID);
        let pin = getfacl(&path, options).unwrap()[0].name.clone();
        assert!(valid_acl_uuid(&pin));
        restrict_socket(&path, geteuid().as_raw(), &pin).unwrap();
        let acl = getfacl(&path, options).unwrap();
        assert_eq!(acl, [AclEntry::allow_user(&pin, Perm::WRITE, None)]);
        let wrong = if pin == "{00000000-0000-4000-8000-000000000001}" {
            "{00000000-0000-4000-8000-000000000002}"
        } else {
            "{00000000-0000-4000-8000-000000000001}"
        };
        assert_eq!(
            restrict_socket(&path, geteuid().as_raw(), wrong),
            Err(Error::EndpointUnavailable)
        );
        let _client = UnixStream::connect(&path).unwrap();
        let _server = listener.accept().unwrap();
        // This runs as the socket owner, so it validates actual ACL storage
        // and socket compatibility, not another UID's effective permissions.
    }

    #[test]
    fn symlink_and_regular_file_are_rejected_without_changing_target_acl() {
        let directory = tempfile::tempdir_in("/private/tmp").unwrap();
        let target = directory.path().join("file");
        let link = directory.path().join("link");
        fs::write(&target, b"unchanged").unwrap();
        fs::set_permissions(&target, fs::Permissions::from_mode(0o600)).unwrap();
        symlink(&target, &link).unwrap();
        let before = getfacl(&target, None).unwrap();

        for path in [&target, &link] {
            assert_eq!(
                restrict_socket(
                    path,
                    geteuid().as_raw(),
                    "{00000000-0000-4000-8000-000000000001}"
                ),
                Err(Error::EndpointUnavailable)
            );
        }
        assert_eq!(getfacl(&target, None).unwrap(), before);
        assert_eq!(fs::read(&target).unwrap(), b"unchanged");
    }
}
