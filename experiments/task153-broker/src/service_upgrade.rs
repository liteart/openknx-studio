// SPDX-License-Identifier: Apache-2.0
//! One-time upgrade of the unused Task153 service. All destination names are
//! fixed. No service start, enrollment, credential access or KNX is implemented.
//!
//! Requires root-only writable ancestry and no concurrent privileged writer or
//! service launch. A bounded, fixed lsof inspection and refused socket connect
//! are conservative observations, not isolation against another root process.
//! Publication spans two directories: it is deliberately not called atomic.
//! The old manifest rejects the new executable until the final manifest rename.
//! Any interruption leaves preserved evidence and refuses automatic retry.

use crate::{macos_config, macos_service, provisioning, root_passkey::unique_json};
use exacl::{AclEntry, AclOption, Perm};
use nix::fcntl::{AtFlags, FcntlArg, OFlag, fcntl, openat};
use nix::sys::stat::{Mode, fchmod, fstatat, mkdirat};
use nix::unistd::{fchown, getegid, geteuid};
use serde::Deserialize;
use sha2::{Digest, Sha256};
use socket2::{Domain, SockAddr, Socket, Type};
use std::ffi::OsString;
use std::fs::File;
use std::io::{Read, Write};
use std::os::unix::fs::MetadataExt;
use std::path::{Path, PathBuf};
use std::process::{Command, Stdio};
use std::str::FromStr;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const HELPERS: &str = "/Library/PrivilegedHelperTools";
const EXECUTABLE: &str =
    "/Library/PrivilegedHelperTools/org.openknxstudio.operator-service-upgrade";
const OLD: &str = "org.openknxstudio.operator";
const NEXT: &str = "org.openknxstudio.operator-service-next";
const SAVED: &str = "org.openknxstudio.operator.pre-nonblock";
const DB: &str = "/private/var/db";
const FENCE: &str = "org.openknxstudio.operator.service-upgrade";
const MANIFEST: &str = "service-manifest.json";
const PLAN: &str = "write-plan.json";
const MAX_BYTES: usize = 16_384;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    Arguments,
    RootRequired,
    ReceiptRejected,
    InstalledCodeRejected,
    StateRejected,
    ProcessInspectionInconclusive,
    ListenerNotProvenAbsent,
    ExistingOrUnavailable,
    Io,
    UpgradeUncertain,
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
    upgrader: Pins,
    old_service: Pins,
    new_service: Pins,
    manifest_sha256: [u8; 32],
    plan_sha256: [u8; 32],
    operator_uid: u32,
    socket_acl_uuid: String,
    expires_at_utc_seconds: u64,
}

fn arguments(args: &[OsString]) -> Result<(PathBuf, [u8; 32]), Error> {
    if args.len() != 4 || args[0] != "--receipt" || args[2] != "--receipt-sha256" {
        return Err(Error::Arguments);
    }
    let hex = args[3].to_str().ok_or(Error::Arguments)?;
    if hex.len() != 64 || !hex.bytes().all(|byte| byte.is_ascii_hexdigit()) {
        return Err(Error::Arguments);
    }
    let mut hash = [0; 32];
    for (index, byte) in hash.iter_mut().enumerate() {
        *byte =
            u8::from_str_radix(&hex[index * 2..index * 2 + 2], 16).map_err(|_| Error::Arguments)?;
    }
    Ok((PathBuf::from(&args[1]), hash))
}

fn flags(directory: bool) -> OFlag {
    OFlag::O_RDONLY
        | OFlag::O_CLOEXEC
        | OFlag::O_NOFOLLOW
        | OFlag::O_NONBLOCK
        | if directory {
            OFlag::O_DIRECTORY
        } else {
            OFlag::empty()
        }
}

fn checked(
    parent: &File,
    name: &str,
    owner: u32,
    mode: u32,
    directory: bool,
) -> Result<File, Error> {
    let file = File::from(
        openat(parent, name, flags(directory), Mode::empty()).map_err(|_| Error::StateRejected)?,
    );
    let meta = file.metadata().map_err(|_| Error::Io)?;
    if meta.uid() != owner
        || meta.gid() != getegid().as_raw()
        || meta.mode() & 0o7777 != mode
        || meta.is_dir() != directory
        || (!directory && (!meta.is_file() || meta.nlink() != 1))
    {
        return Err(Error::StateRejected);
    }
    macos_config::no_granting_acl(&file).map_err(|_| Error::StateRejected)?;
    Ok(file)
}

fn absent(parent: &File, name: &str) -> Result<(), Error> {
    match fstatat(parent, name, AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(nix::errno::Errno::ENOENT) => Ok(()),
        _ => Err(Error::ExistingOrUnavailable),
    }
}

fn entries(directory: &File) -> Result<Vec<Vec<u8>>, Error> {
    let mut result = Vec::new();
    for entry in rustix::fs::Dir::read_from(directory).map_err(|_| Error::Io)? {
        let entry = entry.map_err(|_| Error::Io)?;
        let name = entry.file_name().to_bytes();
        if name == b"." || name == b".." {
            continue;
        }
        if result.len() == 4 {
            return Err(Error::StateRejected);
        }
        result.push(name.to_vec());
    }
    result.sort();
    Ok(result)
}

fn read_hash(parent: &File, name: &str, owner: u32, hash: [u8; 32]) -> Result<Vec<u8>, Error> {
    let mut file = checked(parent, name, owner, 0o600, false)?;
    let mut bytes = Vec::new();
    (&mut file)
        .take((MAX_BYTES + 1) as u64)
        .read_to_end(&mut bytes)
        .map_err(|_| Error::Io)?;
    if bytes.is_empty() || bytes.len() > MAX_BYTES || Sha256::digest(&bytes).as_slice() != hash {
        return Err(Error::StateRejected);
    }
    Ok(bytes)
}

fn signature(path: &Path, pins: &Pins) -> Result<(), Error> {
    use core_foundation::url::CFURL;
    use security_framework::os::macos::code_signing::{Flags, SecRequirement, SecStaticCode};
    if pins.executable_sha256 == [0; 32]
        || pins.executable_cdhash.len() != 40
        || !pins
            .executable_cdhash
            .bytes()
            .all(|b| b.is_ascii_hexdigit())
        || macos_config::hash_installed_service(path).map_err(|_| Error::InstalledCodeRejected)?
            != pins.executable_sha256
    {
        return Err(Error::InstalledCodeRejected);
    }
    let url = CFURL::from_path(path, false).ok_or(Error::InstalledCodeRejected)?;
    let code =
        SecStaticCode::from_path(&url, Flags::NONE).map_err(|_| Error::InstalledCodeRejected)?;
    let requirement = SecRequirement::from_str(&format!("cdhash H\"{}\"", pins.executable_cdhash))
        .map_err(|_| Error::InstalledCodeRejected)?;
    code.check_validity(
        Flags::STRICT_VALIDATE | Flags::NO_NETWORK_ACCESS,
        &requirement,
    )
    .map_err(|_| Error::InstalledCodeRejected)
}

// Fixed system executable and arguments, never a shell or receipt-selected
// program. Both pipes are nonblocking, capped, and deadline-bound. Empty output
// with lsof's documented no-match status is the only accepted observation.
fn no_open_service_image() -> Result<(), Error> {
    let system = macos_config::checked_directory(Path::new("/usr/sbin"))
        .map_err(|_| Error::ProcessInspectionInconclusive)?;
    checked(&system, "lsof", 0, 0o755, false).map_err(|_| Error::ProcessInspectionInconclusive)?;
    let mut child = Command::new("/usr/sbin/lsof")
        .args([
            "-n",
            "-P",
            "-a",
            "-d",
            "txt",
            "-F",
            "p",
            "/Library/PrivilegedHelperTools/org.openknxstudio.operator",
        ])
        .env_clear()
        .env("LC_ALL", "C")
        .stdin(Stdio::null())
        .stdout(Stdio::piped())
        .stderr(Stdio::piped())
        .spawn()
        .map_err(|_| Error::ProcessInspectionInconclusive)?;
    let inspection = (|| {
        let mut stdout = child
            .stdout
            .take()
            .ok_or(Error::ProcessInspectionInconclusive)?;
        let mut stderr = child
            .stderr
            .take()
            .ok_or(Error::ProcessInspectionInconclusive)?;
        fcntl(&stdout, FcntlArg::F_SETFL(OFlag::O_NONBLOCK))
            .map_err(|_| Error::ProcessInspectionInconclusive)?;
        fcntl(&stderr, FcntlArg::F_SETFL(OFlag::O_NONBLOCK))
            .map_err(|_| Error::ProcessInspectionInconclusive)?;
        let deadline = Instant::now() + Duration::from_secs(3);
        let mut buffer = [0; 4096];
        loop {
            for pipe in [&mut stdout as &mut dyn Read, &mut stderr as &mut dyn Read] {
                match pipe.read(&mut buffer) {
                    Ok(0) => (),
                    Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => (),
                    _ => return Err(Error::ProcessInspectionInconclusive),
                }
            }
            match child
                .try_wait()
                .map_err(|_| Error::ProcessInspectionInconclusive)?
            {
                Some(status) => {
                    // Drain again after exit so a late diagnostic cannot be
                    // mistaken for an empty, successful process observation.
                    for pipe in [&mut stdout as &mut dyn Read, &mut stderr as &mut dyn Read] {
                        if !matches!(pipe.read(&mut buffer), Ok(0)) {
                            return Err(Error::ProcessInspectionInconclusive);
                        }
                    }
                    return if status.code() == Some(1) {
                        Ok(())
                    } else {
                        Err(Error::ProcessInspectionInconclusive)
                    };
                }
                None if Instant::now() < deadline => std::thread::sleep(Duration::from_millis(10)),
                None => return Err(Error::ProcessInspectionInconclusive),
            }
        }
    })();
    if inspection.is_err() {
        let _ = child.kill();
        let _ = child.wait();
    }
    inspection
}

fn socket_stamp(
    store: &File,
    path: &Path,
    owner: u32,
    uuid: &str,
) -> Result<Option<(u64, u64)>, Error> {
    let stat = match fstatat(store, "operator.sock", AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(nix::errno::Errno::ENOENT) => return Ok(None),
        Ok(stat) => stat,
        Err(_) => return Err(Error::StateRejected),
    };
    if stat.st_uid != owner
        || stat.st_gid != getegid().as_raw()
        || stat.st_mode & 0o7777 != 0o600
        || stat.st_nlink != 1
        || stat.st_mode & nix::libc::S_IFMT != nix::libc::S_IFSOCK
        || exacl::getfacl(path, Some(AclOption::NATIVE_ID | AclOption::SYMLINK_ACL))
            .map_err(|_| Error::StateRejected)?
            != [AclEntry::allow_user(uuid, Perm::WRITE, None)]
    {
        return Err(Error::StateRejected);
    }
    let socket = Socket::new(Domain::UNIX, Type::STREAM, None)
        .map_err(|_| Error::ListenerNotProvenAbsent)?;
    let address = SockAddr::unix(path).map_err(|_| Error::ListenerNotProvenAbsent)?;
    match socket.connect_timeout(&address, Duration::from_secs(1)) {
        Err(error) if error.raw_os_error() == Some(nix::libc::ECONNREFUSED) => {
            Ok(Some((stat.st_dev as u64, stat.st_ino)))
        }
        _ => Err(Error::ListenerNotProvenAbsent),
    }
}

fn replacement_manifest(bytes: &[u8], receipt: &Receipt) -> Result<Vec<u8>, Error> {
    let mut manifest: serde_json::Value = unique_json(bytes).map_err(|_| Error::StateRejected)?;
    if manifest["operator_uid"] != receipt.operator_uid
        || manifest["agent_uid"] != receipt.operator_uid
        || manifest["operator_acl_uuid"] != receipt.socket_acl_uuid
        || manifest["executable_sha256"] != serde_json::json!(receipt.old_service.executable_sha256)
        || manifest["executable_cdhash"] != receipt.old_service.executable_cdhash
    {
        return Err(Error::StateRejected);
    }
    manifest["executable_sha256"] = serde_json::json!(receipt.new_service.executable_sha256);
    manifest["executable_cdhash"] = serde_json::json!(receipt.new_service.executable_cdhash);
    serde_json::to_vec(&manifest).map_err(|_| Error::StateRejected)
}

fn write_new(parent: &File, name: &str, bytes: &[u8]) -> Result<(), Error> {
    let mut file = File::from(
        openat(
            parent,
            name,
            OFlag::O_WRONLY | OFlag::O_CREAT | OFlag::O_EXCL | OFlag::O_NOFOLLOW | OFlag::O_CLOEXEC,
            Mode::S_IRUSR | Mode::S_IWUSR,
        )
        .map_err(|_| Error::ExistingOrUnavailable)?,
    );
    fchown(&file, None, Some(getegid())).map_err(|_| Error::Io)?;
    fchmod(&file, Mode::S_IRUSR | Mode::S_IWUSR).map_err(|_| Error::Io)?;
    file.write_all(bytes)
        .and_then(|()| file.sync_all())
        .map_err(|_| Error::Io)
}

fn move_new(from: &File, name: &str, to: &File, destination: &str) -> Result<(), Error> {
    rustix::fs::renameat_with(
        from,
        name,
        to,
        destination,
        rustix::fs::RenameFlags::NOREPLACE,
    )
    .map_err(|_| Error::UpgradeUncertain)?;
    from.sync_all()
        .and_then(|()| to.sync_all())
        .map_err(|_| Error::UpgradeUncertain)
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Stage {
    Prepared,
    SocketPreserved,
    OldBinaryPreserved,
    NewBinaryPublished,
    OldManifestPreserved,
    Published,
}

fn publish<F: FnMut(Stage) -> Result<(), Error>>(
    helpers: &File,
    db: &File,
    store: &File,
    manifest: &[u8],
    receipt_bytes: &[u8],
    socket_present: bool,
    mut fault: F,
) -> Result<(), Error> {
    absent(helpers, SAVED)?;
    absent(db, FENCE)?;
    mkdirat(db, FENCE, Mode::S_IRWXU).map_err(|_| Error::ExistingOrUnavailable)?;
    // No cleanup beyond this point: even preparation interruption leaves a
    // fixed fence that refuses blind retry, preserving its exact receipt.
    let transaction: Result<(), Error> = (|| {
        let fence =
            File::from(openat(db, FENCE, flags(true), Mode::empty()).map_err(|_| Error::Io)?);
        fchown(&fence, None, Some(getegid())).map_err(|_| Error::Io)?;
        fchmod(&fence, Mode::S_IRWXU).map_err(|_| Error::Io)?;
        checked(db, FENCE, geteuid().as_raw(), 0o700, true)?;
        write_new(&fence, "receipt.json", receipt_bytes)?;
        write_new(&fence, "new-manifest.json", manifest)?;
        fence
            .sync_all()
            .and_then(|()| db.sync_all())
            .map_err(|_| Error::Io)?;
        fault(Stage::Prepared)?;
        if socket_present {
            move_new(store, "operator.sock", &fence, "operator.sock")?;
        }
        fault(Stage::SocketPreserved)?;
        move_new(helpers, OLD, helpers, SAVED)?;
        fault(Stage::OldBinaryPreserved)?;
        move_new(helpers, NEXT, helpers, OLD)?;
        fault(Stage::NewBinaryPublished)?;
        move_new(store, MANIFEST, &fence, MANIFEST)?;
        fault(Stage::OldManifestPreserved)?;
        move_new(&fence, "new-manifest.json", store, MANIFEST)?;
        fault(Stage::Published)?;
        Ok(())
    })();
    transaction.map_err(|_| Error::UpgradeUncertain)
}

pub fn run(args: Vec<OsString>) -> Result<(), Error> {
    let (path, digest) = arguments(&args)?;
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
        || receipt.expires_at_utc_seconds > now.saturating_add(3600)
        || receipt.old_service.executable_sha256 == receipt.new_service.executable_sha256
    {
        return Err(Error::ReceiptRejected);
    }
    signature(Path::new(EXECUTABLE), &receipt.upgrader)?;
    macos_service::verify_running_image(&receipt.upgrader.executable_cdhash)
        .map_err(|_| Error::InstalledCodeRejected)?;
    let helpers =
        macos_config::checked_directory(Path::new(HELPERS)).map_err(|_| Error::StateRejected)?;
    let db = macos_config::checked_directory(Path::new(DB)).map_err(|_| Error::StateRejected)?;
    absent(&helpers, SAVED)?;
    absent(&db, FENCE)?;
    checked(&helpers, OLD, 0, 0o755, false)?;
    checked(&helpers, NEXT, 0, 0o755, false)?;
    signature(&Path::new(HELPERS).join(OLD), &receipt.old_service)?;
    signature(&Path::new(HELPERS).join(NEXT), &receipt.new_service)?;
    let store = checked(&db, OLD, 0, 0o711, true)?;
    let execution = checked(&store, "execution", 0, 0o700, true)?;
    let manifest = read_hash(&store, MANIFEST, 0, receipt.manifest_sha256)?;
    let plan = read_hash(&store, PLAN, 0, receipt.plan_sha256)?;
    macos_service::validate_provisioning_inputs(&manifest, &plan, input.owner, now)
        .map_err(|_| Error::StateRejected)?;
    let new_manifest = replacement_manifest(&manifest, &receipt)?;
    let socket_path = Path::new(DB).join(OLD).join("operator.sock");
    let socket = socket_stamp(&store, &socket_path, 0, &receipt.socket_acl_uuid)?;
    let mut expected = vec![
        b"execution".to_vec(),
        MANIFEST.as_bytes().to_vec(),
        PLAN.as_bytes().to_vec(),
    ];
    if socket.is_some() {
        expected.push(b"operator.sock".to_vec());
    }
    expected.sort();
    if entries(&store)? != expected || !entries(&execution)?.is_empty() {
        return Err(Error::StateRejected);
    }
    no_open_service_image()?;
    // Recheck evidence after the process inspection; no cooperating root writer
    // or start is permitted during this operator-coordinated transaction.
    if entries(&store)? != expected
        || !entries(&execution)?.is_empty()
        || socket_stamp(&store, &socket_path, 0, &receipt.socket_acl_uuid)? != socket
        || read_hash(&store, MANIFEST, 0, receipt.manifest_sha256)? != manifest
        || read_hash(&store, PLAN, 0, receipt.plan_sha256)? != plan
    {
        return Err(Error::StateRejected);
    }
    publish(
        &helpers,
        &db,
        &store,
        &new_manifest,
        &input.bytes,
        socket.is_some(),
        |_| Ok(()),
    )?;
    println!(
        "Service and manifest upgraded; prior binary, manifest and stale socket preserved. No enrollment, service start or device operation was performed."
    );
    Ok(())
}

#[cfg(test)]
mod tests;
