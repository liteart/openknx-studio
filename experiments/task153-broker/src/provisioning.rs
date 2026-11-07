// SPDX-License-Identifier: Apache-2.0
//! Fixed-purpose first provisioning; no overwrite, deletion, enrollment or KNX.
use crate::{macos_config, macos_service, root_passkey::unique_json};
use nix::fcntl::{AtFlags, OFlag, open, openat};
use nix::sys::stat::{Mode, fchmod, fstatat, mkdirat};
use nix::unistd::{fchown, getegid, geteuid};
use serde::Deserialize;
use sha2::{Digest, Sha256};
use std::ffi::OsString;
use std::fs::File;
use std::io::{Read, Write};
use std::os::unix::fs::MetadataExt;
use std::path::{Component, Path, PathBuf};
use std::time::{SystemTime, UNIX_EPOCH};

const EXECUTABLE: &str = "/Library/PrivilegedHelperTools/org.openknxstudio.operator-provision";
const PARENT: &str = "/private/var/db";
const STAGING: &str = "org.openknxstudio.operator.provisioning";
const DESTINATION: &str = "org.openknxstudio.operator";
const MAX_BYTES: usize = 16384;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    Arguments,
    RootRequired,
    InstalledCodeRejected,
    InputRejected,
    PolicyRejected,
    ExistingOrUnavailable,
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
struct Input {
    path: PathBuf,
    sha256: [u8; 32],
}
#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Receipt {
    schema: u8,
    provisioner: Pins,
    manifest: Input,
    plan: Input,
    expires_at_utc_seconds: u64,
}
struct Snapshot {
    bytes: Vec<u8>,
    owner: u32,
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
fn snapshot(path: &Path, expected: [u8; 32]) -> Result<Snapshot, Error> {
    if !path.is_absolute() || path.as_os_str().len() > 1024 {
        return Err(Error::InputRejected);
    }
    let components: Vec<_> = path.components().collect();
    if components.len() < 2 || components.len() > 32 {
        return Err(Error::InputRejected);
    }
    let mut parent =
        File::from(open("/", flags(true), Mode::empty()).map_err(|_| Error::InputRejected)?);
    macos_config::no_granting_acl(&parent).map_err(|_| Error::InputRejected)?;
    let mut ancestors = vec![parent.metadata().map_err(|_| Error::InputRejected)?];
    for component in &components[1..components.len() - 1] {
        let Component::Normal(name) = component else {
            return Err(Error::InputRejected);
        };
        parent = File::from(
            openat(&parent, *name, flags(true), Mode::empty()).map_err(|_| Error::InputRejected)?,
        );
        macos_config::no_granting_acl(&parent).map_err(|_| Error::InputRejected)?;
        ancestors.push(parent.metadata().map_err(|_| Error::InputRejected)?);
    }
    let Component::Normal(name) = components.last().ok_or(Error::InputRejected)? else {
        return Err(Error::InputRejected);
    };
    let file = File::from(
        openat(&parent, *name, flags(false), Mode::empty()).map_err(|_| Error::InputRejected)?,
    );
    let before = file.metadata().map_err(|_| Error::InputRejected)?;
    if !before.is_file()
        || before.uid() == 0
        || before.nlink() != 1
        || before.mode() & 0o7777 != 0o600
        || before.len() == 0
        || before.len() > MAX_BYTES as u64
    {
        return Err(Error::InputRejected);
    }
    macos_config::no_granting_acl(&file).map_err(|_| Error::InputRejected)?;
    for ancestor in ancestors {
        let sticky_root = ancestor.uid() == 0 && ancestor.mode() & 0o1000 != 0;
        if !ancestor.is_dir()
            || ![0, before.uid()].contains(&ancestor.uid())
            || (ancestor.mode() & 0o022 != 0 && !sticky_root)
        {
            return Err(Error::InputRejected);
        }
    }
    let mut bytes = Vec::new();
    (&file)
        .take((MAX_BYTES + 1) as u64)
        .read_to_end(&mut bytes)
        .map_err(|_| Error::InputRejected)?;
    // The descriptor and content hash bind inputs even if the source name is
    // later exchanged. The reviewed receipt SHA must be independently pinned.
    let after = file.metadata().map_err(|_| Error::InputRejected)?;
    if bytes.len() > MAX_BYTES
        || Sha256::digest(&bytes).as_slice() != expected
        || before.dev() != after.dev()
        || before.ino() != after.ino()
        || before.len() != after.len()
        || before.mtime() != after.mtime()
        || before.mtime_nsec() != after.mtime_nsec()
        || before.ctime() != after.ctime()
        || before.ctime_nsec() != after.ctime_nsec()
    {
        return Err(Error::InputRejected);
    }
    Ok(Snapshot {
        bytes,
        owner: before.uid(),
    })
}
fn arguments(args: &[OsString]) -> Result<(PathBuf, [u8; 32]), Error> {
    if args.len() != 4 || args[0] != "--receipt" || args[2] != "--receipt-sha256" {
        return Err(Error::Arguments);
    }
    let hex = args[3].to_str().ok_or(Error::Arguments)?;
    if hex.len() != 64 || !hex.bytes().all(|b| b.is_ascii_hexdigit()) {
        return Err(Error::Arguments);
    }
    let mut digest = [0; 32];
    for (i, byte) in digest.iter_mut().enumerate() {
        *byte = u8::from_str_radix(&hex[i * 2..i * 2 + 2], 16).map_err(|_| Error::Arguments)?;
    }
    Ok((PathBuf::from(&args[1]), digest))
}
fn check_created(file: &File, mode: u32, directory: bool) -> Result<(), Error> {
    let meta = file.metadata().map_err(|_| Error::Io)?;
    if meta.uid() != geteuid().as_raw()
        || meta.gid() != getegid().as_raw()
        || meta.mode() & 0o7777 != mode
        || meta.is_dir() != directory
        || (!directory && (!meta.is_file() || meta.nlink() != 1))
    {
        return Err(Error::PolicyRejected);
    }
    macos_config::no_granting_acl(file).map_err(|_| Error::PolicyRejected)
}
fn directory(parent: &File, name: &str, mode: u16) -> Result<File, Error> {
    mkdirat(parent, name, Mode::from_bits_truncate(mode))
        .map_err(|_| Error::ExistingOrUnavailable)?;
    let file = File::from(openat(parent, name, flags(true), Mode::empty()).map_err(|_| Error::Io)?);
    fchown(&file, None, Some(getegid())).map_err(|_| Error::Io)?;
    fchmod(&file, Mode::from_bits_truncate(mode)).map_err(|_| Error::Io)?;
    check_created(&file, u32::from(mode), true)?;
    Ok(file)
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
    check_created(&file, 0o600, false)?;
    file.write_all(bytes)
        .and_then(|()| file.sync_all())
        .map_err(|_| Error::Io)
}
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum Stage {
    Created,
    DataSynced,
    Published,
}
fn publish<F: FnMut(Stage) -> Result<(), Error>>(
    parent: &File,
    manifest: &[u8],
    plan: &[u8],
    mut fault: F,
) -> Result<(), Error> {
    match fstatat(parent, DESTINATION, AtFlags::AT_SYMLINK_NOFOLLOW) {
        Err(nix::errno::Errno::ENOENT) => (),
        _ => return Err(Error::ExistingOrUnavailable),
    }
    let staging = directory(parent, STAGING, 0o700)?;
    // From this point no cleanup occurs on error. A stale staging directory
    // refuses retry; the installed service never reads this alternate path.
    fault(Stage::Created)?;
    write_new(&staging, "service-manifest.json", manifest)?;
    write_new(&staging, "write-plan.json", plan)?;
    let execution = directory(&staging, "execution", 0o700)?;
    execution.sync_all().map_err(|_| Error::Io)?;
    fchmod(&staging, Mode::from_bits_truncate(0o711)).map_err(|_| Error::Io)?;
    check_created(&staging, 0o711, true)?;
    staging
        .sync_all()
        .and_then(|()| parent.sync_all())
        .map_err(|_| Error::Io)?;
    fault(Stage::DataSynced)?;
    rustix::fs::renameat_with(
        parent,
        STAGING,
        parent,
        DESTINATION,
        rustix::fs::RenameFlags::NOREPLACE,
    )
    .map_err(|_| Error::ExistingOrUnavailable)?;
    // The files were synchronized before publication. A lost acknowledgement
    // or final sync failure may leave a valid published store: never retry or
    // claim rollback/no effect. No files or existing destination are deleted.
    fault(Stage::Published).map_err(|_| Error::PublishedUncertain)?;
    parent.sync_all().map_err(|_| Error::PublishedUncertain)
}

pub fn run(args: Vec<OsString>) -> Result<(), Error> {
    let (receipt_path, receipt_sha) = arguments(&args)?;
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    if std::env::current_exe().map_err(|_| Error::InstalledCodeRejected)? != Path::new(EXECUTABLE) {
        return Err(Error::InstalledCodeRejected);
    }
    let input = snapshot(&receipt_path, receipt_sha)?;
    let receipt: Receipt = unique_json(&input.bytes).map_err(|_| Error::InputRejected)?;
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_err(|_| Error::InputRejected)?
        .as_secs();
    if receipt.schema != 1
        || receipt.expires_at_utc_seconds <= now
        || receipt.expires_at_utc_seconds > now.saturating_add(86400)
    {
        return Err(Error::InputRejected);
    }
    if macos_config::hash_installed_service(Path::new(EXECUTABLE))
        .map_err(|_| Error::InstalledCodeRejected)?
        != receipt.provisioner.executable_sha256
    {
        return Err(Error::InstalledCodeRejected);
    }
    macos_service::verify_running_image(&receipt.provisioner.executable_cdhash)
        .map_err(|_| Error::InstalledCodeRejected)?;
    let manifest = snapshot(&receipt.manifest.path, receipt.manifest.sha256)?;
    let plan = snapshot(&receipt.plan.path, receipt.plan.sha256)?;
    if manifest.owner != input.owner || plan.owner != input.owner {
        return Err(Error::InputRejected);
    }
    macos_service::validate_provisioning_inputs(&manifest.bytes, &plan.bytes, input.owner, now)
        .map_err(|_| Error::PolicyRejected)?;
    let parent =
        macos_config::checked_directory(Path::new(PARENT)).map_err(|_| Error::PolicyRejected)?;
    publish(&parent, &manifest.bytes, &plan.bytes, |_| Ok(()))?;
    println!("Protected inputs published. No service, enrollment or device operation was started.");
    Ok(())
}

#[cfg(test)]
mod tests;
