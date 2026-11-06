// SPDX-License-Identifier: Apache-2.0
//! Descriptor-bound root-owned configuration inspection, not a service grant.

use exacl::{AclEntry, AclEntryKind, AclOption, getfacl};
use nix::errno::Errno;
use nix::fcntl::AtFlags;
use nix::fcntl::{OFlag, open, openat};
use nix::sys::stat::Mode;
use sha2::{Digest, Sha256};
use std::ffi::OsStr;
use std::fs::File;
use std::io::Read;
use std::os::fd::AsFd;
use std::os::unix::fs::MetadataExt;
use std::path::{Component, Path};

pub(super) const CONFIGURATION: &str = "/private/var/db/org.openknxstudio.operator/config.json";
pub const MAX_CONFIGURATION_BYTES: usize = 16_384;
const MAX_PATH_BYTES: usize = 1_024;
const MAX_COMPONENTS: usize = 32;
const MAX_ACL_ENTRIES: usize = 128;

#[derive(Debug, PartialEq, Eq)]
pub enum ConfigError {
    PathRejected,
    PermissionsRejected,
    FileTypeRejected,
    ExtendedAclRejected,
    AclUnavailable,
    TooLarge,
    ChangedDuringRead,
    Io,
    ConfigurationBusy,
    ActivationUncertain,
}

/// Bytes read from an opened root-owned file after descriptor checks. This does
/// not certify installed service integrity, operator identity or enrollment.
/// There is deliberately no constructor, deserializer or approval conversion.
pub struct RootOwnedSnapshot {
    bytes: Vec<u8>,
}

impl RootOwnedSnapshot {
    pub fn as_bytes(&self) -> &[u8] {
        &self.bytes
    }
}

fn directory_flags() -> OFlag {
    OFlag::O_RDONLY | OFlag::O_DIRECTORY | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC
}

pub(super) fn no_granting_acl(file: &File) -> Result<(), ConfigError> {
    // Native IDs avoid resolving ACL principals through account/name services.
    // Deny entries cannot add rights; reject all allow entries, including
    // read-only ones, rather than speculate about effective ACL semantics.
    let entries = getfacl(file.as_fd(), Some(AclOption::NATIVE_ID))
        .map_err(|_| ConfigError::AclUnavailable)?;
    recognized_deny_entries_only(&entries)
}

fn recognized_deny_entries_only(entries: &[AclEntry]) -> Result<(), ConfigError> {
    // Upstream maps unknown native tags to allow=false and kind=Unknown.
    // Therefore allow=false alone does not establish an explicit deny ACE.
    if entries.len() > MAX_ACL_ENTRIES
        || entries.iter().any(|entry| {
            entry.allow || !matches!(entry.kind, AclEntryKind::User | AclEntryKind::Group)
        })
    {
        return Err(ConfigError::ExtendedAclRejected);
    }
    Ok(())
}

fn checked_metadata(file: &File, directory: bool) -> Result<std::fs::Metadata, ConfigError> {
    let metadata = file.metadata().map_err(|_| ConfigError::Io)?;
    if (directory && !metadata.is_dir()) || (!directory && !metadata.is_file()) {
        return Err(ConfigError::FileTypeRejected);
    }
    if metadata.uid() != 0 || metadata.mode() & 0o022 != 0 || (!directory && metadata.nlink() != 1)
    {
        return Err(ConfigError::PermissionsRejected);
    }
    no_granting_acl(file)?;
    Ok(metadata)
}

fn open_directory_at(parent: &File, name: &OsStr) -> Result<File, ConfigError> {
    openat(parent, name, directory_flags(), Mode::empty())
        .map(File::from)
        .map_err(|_| ConfigError::PathRejected)
}

pub(super) fn checked_directory(path: &Path) -> Result<File, ConfigError> {
    if !path.is_absolute() || path.as_os_str().len() > MAX_PATH_BYTES {
        return Err(ConfigError::PathRejected);
    }
    let components: Vec<_> = path.components().collect();
    if components.len() > MAX_COMPONENTS || components.first() != Some(&Component::RootDir) {
        return Err(ConfigError::PathRejected);
    }
    let mut parent = open("/", directory_flags(), Mode::empty())
        .map(File::from)
        .map_err(|_| ConfigError::PathRejected)?;
    checked_metadata(&parent, true)?;
    for component in &components[1..] {
        let Component::Normal(name) = component else {
            return Err(ConfigError::PathRejected);
        };
        let child = open_directory_at(&parent, name)?;
        checked_metadata(&child, true)?;
        parent = child;
    }
    Ok(parent)
}

/// Inspect each root-owned ancestor using no-follow, descriptor-relative opens,
/// metadata and native ACLs. It does not establish a complete protected host.
pub fn inspect_root_owned_directory(path: &Path) -> Result<(), ConfigError> {
    checked_directory(path).map(drop)
}

fn open_leaf(parent: &File, name: &OsStr) -> Result<File, ConfigError> {
    // NONBLOCK prevents an unexpected FIFO from blocking before type rejection.
    openat(
        parent,
        name,
        OFlag::O_RDONLY | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC | OFlag::O_NONBLOCK,
        Mode::empty(),
    )
    .map(File::from)
    .map_err(|_| ConfigError::PathRejected)
}

fn read_bounded(file: &File) -> Result<Vec<u8>, ConfigError> {
    if file.metadata().map_err(|_| ConfigError::Io)?.len() > MAX_CONFIGURATION_BYTES as u64 {
        return Err(ConfigError::TooLarge);
    }
    let mut bytes = Vec::new();
    file.take((MAX_CONFIGURATION_BYTES + 1) as u64)
        .read_to_end(&mut bytes)
        .map_err(|_| ConfigError::Io)?;
    if bytes.len() > MAX_CONFIGURATION_BYTES {
        return Err(ConfigError::TooLarge);
    }
    Ok(bytes)
}

#[derive(Clone, Copy, PartialEq, Eq)]
struct SnapshotStamp {
    device: u64,
    inode: u64,
    length: u64,
    modified: i64,
    modified_nsec: i64,
    changed: i64,
    changed_nsec: i64,
}

impl From<&std::fs::Metadata> for SnapshotStamp {
    fn from(metadata: &std::fs::Metadata) -> Self {
        Self {
            device: metadata.dev(),
            inode: metadata.ino(),
            length: metadata.len(),
            modified: metadata.mtime(),
            modified_nsec: metadata.mtime_nsec(),
            changed: metadata.ctime(),
            changed_nsec: metadata.ctime_nsec(),
        }
    }
}

fn ensure_unchanged(
    before: SnapshotStamp,
    after: SnapshotStamp,
    read_length: usize,
) -> Result<(), ConfigError> {
    if before != after || read_length as u64 != after.length {
        return Err(ConfigError::ChangedDuringRead);
    }
    Ok(())
}

/// Read only the fixed future operator configuration. No path from IPC is used.
/// Installation and enrollment of this file remain separate operator actions.
pub fn load_fixed_configuration() -> Result<RootOwnedSnapshot, ConfigError> {
    let path = Path::new(CONFIGURATION);
    let parent = checked_directory(path.parent().ok_or(ConfigError::PathRejected)?)?;
    let _lock = lock_existing_configuration(&parent)?;
    ensure_no_activation_fence(&parent)?;
    read_checked_leaf(&parent, path.file_name().ok_or(ConfigError::PathRejected)?)
}

fn read_checked_leaf(parent: &File, name: &OsStr) -> Result<RootOwnedSnapshot, ConfigError> {
    let file = open_leaf(parent, name)?;
    let before = checked_metadata(&file, false)?;
    if before.mode() & 0o7777 != 0o600 {
        return Err(ConfigError::PermissionsRejected);
    }
    let bytes = read_bounded(&file)?;
    let after = checked_metadata(&file, false)?;
    ensure_unchanged((&before).into(), (&after).into(), bytes.len())?;
    Ok(RootOwnedSnapshot { bytes })
}

pub(super) fn load_service_manifest() -> Result<RootOwnedSnapshot, ConfigError> {
    let parent = checked_directory(
        Path::new(CONFIGURATION)
            .parent()
            .ok_or(ConfigError::PathRejected)?,
    )?;
    read_checked_leaf(&parent, OsStr::new("service-manifest.json"))
}

pub(super) fn load_write_plan() -> Result<RootOwnedSnapshot, ConfigError> {
    let parent = checked_directory(
        Path::new(CONFIGURATION)
            .parent()
            .ok_or(ConfigError::PathRejected)?,
    )?;
    read_checked_leaf(&parent, OsStr::new("write-plan.json"))
}

pub(super) fn hash_installed_service(path: &Path) -> Result<[u8; 32], ConfigError> {
    let parent = checked_directory(path.parent().ok_or(ConfigError::PathRejected)?)?;
    let file = open_leaf(&parent, path.file_name().ok_or(ConfigError::PathRejected)?)?;
    let before = checked_metadata(&file, false)?;
    if before.len() > 128 * 1024 * 1024 {
        return Err(ConfigError::TooLarge);
    }
    let mut digest = Sha256::new();
    let mut count = 0u64;
    let mut file_ref = &file;
    let mut buffer = [0; 32768];
    loop {
        let length = file_ref.read(&mut buffer).map_err(|_| ConfigError::Io)?;
        if length == 0 {
            break;
        }
        count = count
            .checked_add(length as u64)
            .ok_or(ConfigError::TooLarge)?;
        if count > 128 * 1024 * 1024 {
            return Err(ConfigError::TooLarge);
        }
        digest.update(&buffer[..length]);
    }
    let after = checked_metadata(&file, false)?;
    ensure_unchanged((&before).into(), (&after).into(), count as usize)?;
    Ok(digest.finalize().into())
}

fn lock_existing_configuration(parent: &File) -> Result<File, ConfigError> {
    let file = open_leaf(parent, OsStr::new("enrollment.lock"))?;
    let metadata = checked_metadata(&file, false)?;
    if metadata.len() != 0 || metadata.mode() & 0o7777 != 0o600 {
        return Err(ConfigError::PermissionsRejected);
    }
    rustix::fs::flock(&file, rustix::fs::FlockOperation::NonBlockingLockShared)
        .map_err(|_| ConfigError::ConfigurationBusy)?;
    // Keeping this File alive holds the lock; closing it releases it without
    // a custom Drop that could panic on an explicit unlock error.
    Ok(file)
}

pub(super) fn ensure_no_activation_fence(parent: &File) -> Result<(), ConfigError> {
    match nix::sys::stat::fstatat(parent, "activation.pending", AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(Errno::ENOENT) => Ok(()),
        _ => Err(ConfigError::ActivationUncertain),
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use exacl::{Flag, Perm, setfacl};
    use nix::unistd::geteuid;
    use std::fs;
    use std::os::unix::fs::{PermissionsExt, symlink};

    fn fixture() -> tempfile::TempDir {
        tempfile::Builder::new()
            .prefix("task153-acl-")
            .tempdir_in("/private/tmp")
            .unwrap()
    }

    #[test]
    fn actual_root_owned_ancestors_pass_descriptor_metadata_and_acl_checks() {
        assert_eq!(
            inspect_root_owned_directory(Path::new("/private/var/db")),
            Ok(())
        );
    }

    #[test]
    fn nonroot_owned_configuration_is_rejected_even_with_private_mode() {
        let directory = fixture();
        let path = directory.path().join("config");
        fs::write(&path, b"synthetic").unwrap();
        fs::set_permissions(&path, fs::Permissions::from_mode(0o600)).unwrap();
        let file = File::open(path).unwrap();
        if geteuid().as_raw() != 0 {
            assert!(matches!(
                checked_metadata(&file, false),
                Err(ConfigError::PermissionsRejected)
            ));
        }
    }

    #[test]
    fn actual_allow_acl_is_rejected_even_when_posix_mode_is_private() {
        let directory = fixture();
        let path = directory.path().join("config");
        fs::write(&path, b"synthetic").unwrap();
        fs::set_permissions(&path, fs::Permissions::from_mode(0o600)).unwrap();
        let file = File::open(path).unwrap();
        assert_eq!(no_granting_acl(&file), Ok(()));
        let entry = AclEntry::allow_user(&geteuid().as_raw().to_string(), Perm::WRITE, None);
        setfacl(file.as_fd(), &[entry], None).unwrap();
        assert_eq!(file.metadata().unwrap().mode() & 0o777, 0o600);
        assert_eq!(
            no_granting_acl(&file),
            Err(ConfigError::ExtendedAclRejected)
        );
    }

    #[test]
    fn only_recognized_deny_entries_are_accepted_including_unknown_tag_regression() {
        let deny = AclEntry::deny_user("synthetic", Perm::READ, None);
        assert_eq!(recognized_deny_entries_only(&[deny]), Ok(()));
        let read_only_allow = AclEntry::allow_user("synthetic", Perm::READ, None);
        assert_eq!(
            recognized_deny_entries_only(&[read_only_allow]),
            Err(ConfigError::ExtendedAclRejected)
        );
        // This models the pinned library's representation of an unrecognized
        // native tag; no invalid ACL is written to the operating system.
        let unknown_tag = AclEntry {
            kind: AclEntryKind::Unknown,
            name: "@tag 0".into(),
            perms: Perm::empty(),
            flags: Flag::empty(),
            allow: false,
        };
        assert_eq!(
            recognized_deny_entries_only(&[unknown_tag]),
            Err(ConfigError::ExtendedAclRejected)
        );
    }

    #[test]
    fn same_size_same_mtime_snapshot_with_changed_ctime_is_rejected() {
        let directory = fixture();
        let path = directory.path().join("config");
        fs::write(&path, b"synthetic").unwrap();
        let before = SnapshotStamp::from(&File::open(path).unwrap().metadata().unwrap());
        assert_eq!(
            ensure_unchanged(before, before, before.length as usize),
            Ok(())
        );
        for change_nsec in [false, true] {
            // Deterministic metadata fault injection models a same-size write
            // whose mtime was restored, without depending on filesystem ticks.
            let mut after = before;
            if change_nsec {
                after.changed_nsec += 1;
            } else {
                after.changed += 1;
            }
            assert_eq!(
                ensure_unchanged(before, after, before.length as usize),
                Err(ConfigError::ChangedDuringRead)
            );
        }
    }

    #[test]
    fn leaf_and_directory_symlinks_are_never_followed() {
        let directory = fixture();
        let parent = File::open(directory.path()).unwrap();
        fs::write(directory.path().join("config"), b"synthetic").unwrap();
        fs::create_dir(directory.path().join("child")).unwrap();
        symlink("config", directory.path().join("file-link")).unwrap();
        symlink("child", directory.path().join("dir-link")).unwrap();
        assert!(matches!(
            open_leaf(&parent, OsStr::new("file-link")),
            Err(ConfigError::PathRejected)
        ));
        assert!(matches!(
            open_directory_at(&parent, OsStr::new("dir-link")),
            Err(ConfigError::PathRejected)
        ));
    }

    #[test]
    fn renamed_file_keeps_acl_and_contents_bound_to_the_original_descriptor() {
        let directory = fixture();
        let parent = File::open(directory.path()).unwrap();
        let path = directory.path().join("config");
        fs::write(&path, b"original snapshot").unwrap();
        let original = open_leaf(&parent, OsStr::new("config")).unwrap();
        fs::rename(&path, directory.path().join("old")).unwrap();
        fs::write(&path, b"replacement snapshot").unwrap();
        let replacement = File::open(&path).unwrap();
        let entry = AclEntry::allow_user(&geteuid().as_raw().to_string(), Perm::WRITE, None);
        setfacl(replacement.as_fd(), &[entry], None).unwrap();
        assert_eq!(no_granting_acl(&original), Ok(()));
        assert_eq!(read_bounded(&original).unwrap(), b"original snapshot");
        assert_eq!(
            no_granting_acl(&replacement),
            Err(ConfigError::ExtendedAclRejected)
        );
    }

    #[test]
    fn configuration_reads_are_bounded_and_relative_paths_are_rejected() {
        let directory = fixture();
        let path = directory.path().join("config");
        fs::write(&path, vec![0; MAX_CONFIGURATION_BYTES + 1]).unwrap();
        assert_eq!(
            read_bounded(&File::open(path).unwrap()),
            Err(ConfigError::TooLarge)
        );
        assert_eq!(
            inspect_root_owned_directory(Path::new("relative")),
            Err(ConfigError::PathRejected)
        );
        assert_eq!(
            inspect_root_owned_directory(Path::new("/private/var/../var/db")),
            Err(ConfigError::PathRejected)
        );
    }
}
