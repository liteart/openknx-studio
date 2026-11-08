// SPDX-License-Identifier: Apache-2.0
//! Fixed-purpose preservation of an unused, failed-startup store. No deletion,
//! replacement, enrollment, service start or device operation is implemented.
//!
//! This assumes checked root-only writable ancestry and no noncooperating root
//! writer. The kernel socket probe sends no application bytes and accepts only
//! ECONNREFUSED; it is not a general process census. Exact unused store contents
//! and a stale pathname exclude this service's normal startup/operation path.
//! Whole-directory quarantine preserves every byte; it never clears a journal
//! or makes an old approval reusable. Fresh provisioning is a separate action.

use crate::{macos_config, macos_service, provisioning, root_passkey::unique_json};
use exacl::{AclEntry, AclOption, Perm, getfacl};
use nix::fcntl::{AtFlags, OFlag, openat};
use nix::sys::stat::Mode;
use nix::unistd::{getegid, geteuid};
use serde::Deserialize;
use sha2::{Digest, Sha256};
use socket2::{Domain, SockAddr, Socket, Type};
use std::ffi::OsString;
use std::fs::{File, Metadata};
use std::io::Read;
use std::os::unix::fs::MetadataExt;
use std::path::{Path, PathBuf};
use std::time::{Duration, SystemTime, UNIX_EPOCH};

const EXECUTABLE: &str = "/Library/PrivilegedHelperTools/org.openknxstudio.operator-recover";
const PARENT: &str = "/private/var/db";
const STORE: &str = "org.openknxstudio.operator";
const QUARANTINE: &str = "org.openknxstudio.operator.failed-startup";
const MAX_BYTES: usize = 16_384;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    Arguments,
    RootRequired,
    InstalledCodeRejected,
    ReceiptRejected,
    StoreRejected,
    StatePresent,
    ListenerNotProvenAbsent,
    QuarantineUnavailable,
    Io,
    PublishedUncertain,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Pins {
    executable_sha256: [u8; 32],
    executable_cdhash: String,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Receipt {
    schema: u8,
    recovery: Pins,
    manifest_sha256: [u8; 32],
    plan_sha256: [u8; 32],
    operator_uid: u32,
    socket_acl_uuid: String,
    expires_at_utc_seconds: u64,
}

#[derive(Deserialize)]
struct OldManifest {
    operator_uid: u32,
    agent_uid: u32,
}

#[derive(Deserialize)]
struct OldPlan {
    expires_at_utc_seconds: u64,
}

fn parse_arguments(arguments: &[OsString]) -> Result<(PathBuf, [u8; 32]), Error> {
    if arguments.len() != 4 || arguments[0] != "--receipt" || arguments[2] != "--receipt-sha256" {
        return Err(Error::Arguments);
    }
    let hex = arguments[3].to_str().ok_or(Error::Arguments)?;
    if hex.len() != 64 || !hex.bytes().all(|byte| byte.is_ascii_hexdigit()) {
        return Err(Error::Arguments);
    }
    let mut digest = [0; 32];
    for (index, byte) in digest.iter_mut().enumerate() {
        *byte =
            u8::from_str_radix(&hex[index * 2..index * 2 + 2], 16).map_err(|_| Error::Arguments)?;
    }
    Ok((PathBuf::from(&arguments[1]), digest))
}

fn flags(directory: bool) -> OFlag {
    OFlag::O_RDONLY
        | OFlag::O_NOFOLLOW
        | OFlag::O_CLOEXEC
        | OFlag::O_NONBLOCK
        | if directory {
            OFlag::O_DIRECTORY
        } else {
            OFlag::empty()
        }
}

fn same_inode(left: &Metadata, right: &Metadata) -> bool {
    left.dev() == right.dev() && left.ino() == right.ino()
}

fn checked_file(
    parent: &File,
    name: &str,
    owner: u32,
    mode: u32,
    directory: bool,
) -> Result<File, Error> {
    let file = File::from(
        openat(parent, name, flags(directory), Mode::empty()).map_err(|_| Error::StoreRejected)?,
    );
    let metadata = file.metadata().map_err(|_| Error::Io)?;
    if metadata.uid() != owner
        || metadata.gid() != getegid().as_raw()
        || metadata.mode() & 0o7777 != mode
        || metadata.is_dir() != directory
        || (!directory && (!metadata.is_file() || metadata.nlink() != 1))
    {
        return Err(Error::StoreRejected);
    }
    macos_config::no_granting_acl(&file).map_err(|_| Error::StoreRejected)?;
    Ok(file)
}

fn entries(directory: &File) -> Result<Vec<Vec<u8>>, Error> {
    let mut entries = Vec::new();
    for entry in rustix::fs::Dir::read_from(directory).map_err(|_| Error::Io)? {
        let entry = entry.map_err(|_| Error::Io)?;
        let name = entry.file_name().to_bytes();
        if name == b"." || name == b".." {
            continue;
        }
        if entries.len() >= 4 {
            return Err(Error::StatePresent);
        }
        entries.push(name.to_vec());
    }
    entries.sort();
    Ok(entries)
}

fn hashed_leaf(parent: &File, name: &str, owner: u32, digest: [u8; 32]) -> Result<Vec<u8>, Error> {
    let mut file = checked_file(parent, name, owner, 0o600, false)?;
    let before = file.metadata().map_err(|_| Error::Io)?;
    if before.len() == 0 || before.len() > MAX_BYTES as u64 {
        return Err(Error::StoreRejected);
    }
    let mut bytes = Vec::new();
    (&mut file)
        .take((MAX_BYTES + 1) as u64)
        .read_to_end(&mut bytes)
        .map_err(|_| Error::Io)?;
    let after = file.metadata().map_err(|_| Error::Io)?;
    if bytes.len() > MAX_BYTES
        || bytes.len() as u64 != after.len()
        || !same_inode(&before, &after)
        || before.len() != after.len()
        || before.mtime() != after.mtime()
        || before.mtime_nsec() != after.mtime_nsec()
        || before.ctime() != after.ctime()
        || before.ctime_nsec() != after.ctime_nsec()
        || Sha256::digest(&bytes).as_slice() != digest
    {
        return Err(Error::StoreRejected);
    }
    Ok(bytes)
}

fn socket_stamp(
    parent: &File,
    path: &Path,
    owner: u32,
    expected_uuid: &str,
) -> Result<(nix::libc::dev_t, nix::libc::ino_t), Error> {
    let metadata = nix::sys::stat::fstatat(parent, "operator.sock", AtFlags::AT_SYMLINK_NOFOLLOW)
        .map_err(|_| Error::StoreRejected)?;
    if metadata.st_uid != owner
        || metadata.st_gid != getegid().as_raw()
        || metadata.st_mode & 0o7777 != 0o600
        || metadata.st_mode & nix::libc::S_IFMT != nix::libc::S_IFSOCK
        || metadata.st_nlink != 1
    {
        return Err(Error::StoreRejected);
    }
    let expected = [AclEntry::allow_user(expected_uuid, Perm::WRITE, None)];
    if getfacl(path, Some(AclOption::SYMLINK_ACL | AclOption::NATIVE_ID))
        .map_err(|_| Error::StoreRejected)?
        != expected
    {
        return Err(Error::StoreRejected);
    }
    Ok((metadata.st_dev, metadata.st_ino))
}

fn require_stale_socket(path: &Path) -> Result<(), Error> {
    let address = SockAddr::unix(path).map_err(|_| Error::ListenerNotProvenAbsent)?;
    let socket = Socket::new(Domain::UNIX, Type::STREAM, None)
        .map_err(|_| Error::ListenerNotProvenAbsent)?;
    match socket.connect_timeout(&address, Duration::from_secs(1)) {
        Err(error) if error.raw_os_error() == Some(nix::libc::ECONNREFUSED) => Ok(()),
        _ => Err(Error::ListenerNotProvenAbsent),
    }
}

#[derive(Clone, Copy)]
enum Stage {
    Inspected,
    Renamed,
}

fn quarantine<F: FnMut(Stage) -> Result<(), Error>>(
    parent: &File,
    parent_path: &Path,
    owner: u32,
    receipt: &Receipt,
    now: u64,
    mut fault: F,
) -> Result<(), Error> {
    let store = checked_file(parent, STORE, owner, 0o711, true)?;
    let stamp = nix::sys::stat::fstat(&store).map_err(|_| Error::Io)?;
    let expected = vec![
        b"execution".to_vec(),
        b"operator.sock".to_vec(),
        b"service-manifest.json".to_vec(),
        b"write-plan.json".to_vec(),
    ];
    if entries(&store)? != expected {
        return Err(Error::StatePresent);
    }
    let execution = checked_file(&store, "execution", owner, 0o700, true)?;
    if !entries(&execution)?.is_empty() {
        return Err(Error::StatePresent);
    }
    let manifest = hashed_leaf(
        &store,
        "service-manifest.json",
        owner,
        receipt.manifest_sha256,
    )?;
    let plan = hashed_leaf(&store, "write-plan.json", owner, receipt.plan_sha256)?;
    let manifest: OldManifest = unique_json(&manifest).map_err(|_| Error::StoreRejected)?;
    let plan: OldPlan = unique_json(&plan).map_err(|_| Error::StoreRejected)?;
    if manifest.operator_uid != receipt.operator_uid
        || manifest.agent_uid != receipt.operator_uid
        || plan.expires_at_utc_seconds > now
    {
        return Err(Error::StoreRejected);
    }
    let socket_path = parent_path.join(STORE).join("operator.sock");
    let socket_before = socket_stamp(&store, &socket_path, owner, &receipt.socket_acl_uuid)?;
    require_stale_socket(&socket_path)?;
    fault(Stage::Inspected)?;
    if entries(&store)? != expected
        || !entries(&execution)?.is_empty()
        || socket_stamp(&store, &socket_path, owner, &receipt.socket_acl_uuid)? != socket_before
    {
        return Err(Error::StatePresent);
    }
    let current = nix::sys::stat::fstatat(parent, STORE, AtFlags::AT_SYMLINK_NOFOLLOW)
        .map_err(|_| Error::StoreRejected)?;
    if current.st_dev != stamp.st_dev || current.st_ino != stamp.st_ino {
        return Err(Error::StoreRejected);
    }
    rustix::fs::renameat_with(
        parent,
        STORE,
        parent,
        QUARANTINE,
        rustix::fs::RenameFlags::NOREPLACE,
    )
    .map_err(|_| Error::QuarantineUnavailable)?;
    // No content is deleted or edited. After rename the result may be present
    // even if the acknowledgement or parent sync is lost; never retry blindly.
    fault(Stage::Renamed).map_err(|_| Error::PublishedUncertain)?;
    parent.sync_all().map_err(|_| Error::PublishedUncertain)
}

pub fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    let (path, digest) = parse_arguments(&arguments)?;
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    if std::env::current_exe().map_err(|_| Error::InstalledCodeRejected)? != Path::new(EXECUTABLE) {
        return Err(Error::InstalledCodeRejected);
    }
    let input = provisioning::snapshot(&path, digest).map_err(|_| Error::ReceiptRejected)?;
    let receipt: Receipt = unique_json(&input.bytes).map_err(|_| Error::ReceiptRejected)?;
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_err(|_| Error::ReceiptRejected)?
        .as_secs();
    if receipt.schema != 1
        || receipt.operator_uid == 0
        || receipt.operator_uid != input.owner
        || receipt.expires_at_utc_seconds <= now
        || receipt.expires_at_utc_seconds > now.saturating_add(86400)
    {
        return Err(Error::ReceiptRejected);
    }
    if macos_config::hash_installed_service(Path::new(EXECUTABLE))
        .map_err(|_| Error::InstalledCodeRejected)?
        != receipt.recovery.executable_sha256
    {
        return Err(Error::InstalledCodeRejected);
    }
    macos_service::verify_running_image(&receipt.recovery.executable_cdhash)
        .map_err(|_| Error::InstalledCodeRejected)?;
    if provisioning::native_operator_acl_uuid(receipt.operator_uid)
        .map_err(|_| Error::ReceiptRejected)?
        != receipt.socket_acl_uuid
    {
        return Err(Error::ReceiptRejected);
    }
    let parent =
        macos_config::checked_directory(Path::new(PARENT)).map_err(|_| Error::StoreRejected)?;
    quarantine(&parent, Path::new(PARENT), 0, &receipt, now, |_| Ok(()))?;
    println!(
        "Unused failed-startup store preserved in fixed quarantine. No configuration, enrollment, service or device operation was started."
    );
    Ok(())
}

#[cfg(test)]
mod tests;
