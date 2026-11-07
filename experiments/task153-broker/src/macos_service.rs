// SPDX-License-Identifier: Apache-2.0
//! Fixed, bounded local service listener. No installer, operator UI or KNX.

use crate::macos_peer::ExpectedOperator;
use crate::{ConfiguredPlan, macos_config, operator_connection, root_passkey::unique_json};
use nix::unistd::{Uid, User, getegid, geteuid};
use security_framework::os::macos::code_signing::{Flags, SecCode, SecRequirement};
use serde::Deserialize;
use sha2::{Digest, Sha256};
use std::ffi::OsString;
use std::fs;
use std::net::SocketAddrV4;
use std::os::unix::fs::PermissionsExt;
use std::os::unix::net::{UnixListener, UnixStream};
use std::path::Path;
use std::str::FromStr;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};
use task152_passkey_prototype::guard;

const EXECUTABLE: &str = "/Library/PrivilegedHelperTools/org.openknxstudio.operator";
pub(crate) const ENROLLMENT_EXECUTABLE: &str =
    "/Library/PrivilegedHelperTools/org.openknxstudio.operator-enrollment";
const STORE: &str = "/private/var/db/org.openknxstudio.operator";
const SOCKET: &str = "/private/var/db/org.openknxstudio.operator/operator.sock";
// Admission window, not a hard shutdown deadline: an admitted connection and
// synchronous filesystem commit may complete after this instant.
const LIFETIME: Duration = Duration::from_secs(900);
const MAX_SESSIONS: usize = 8;

/// Opaque runtime evidence, created only after installed startup checks. This
/// proves the checked service/configuration facts, not same-user UI isolation.
pub(crate) struct VerifiedStartup {
    plan: ConfiguredPlan,
    policy: ExpectedOperator,
}

impl VerifiedStartup {
    pub(crate) fn plan(&self) -> &ConfiguredPlan {
        &self.plan
    }
    pub(crate) fn policy(&self) -> &ExpectedOperator {
        &self.policy
    }
}

type ConnectionHandler = fn(UnixStream, &VerifiedStartup) -> Result<(), operator_connection::Error>;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    Arguments,
    RootRequired,
    InstallationUnavailable,
    ManifestRejected,
    RunningImageRejected,
    PlanRejected,
    EndpointUnavailable,
    Io,
    OperatorPolicyRejected,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Manifest {
    schema: u8,
    executable_sha256: [u8; 32],
    executable_cdhash: String,
    plan_digest: [u8; 32],
    gateway: String,
    operator_uid: u32,
    operator_gid: u32,
    operator_acl_uuid: String,
    agent_uid: u32,
    #[serde(default)]
    enrollment_bootstrap: Option<EnrollmentBootstrapPins>,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct EnrollmentBootstrapPins {
    executable_sha256: [u8; 32],
    executable_cdhash: String,
}

fn valid_code_pins(sha256: &[u8; 32], cdhash: &str) -> bool {
    *sha256 != [0; 32] && cdhash.len() == 40 && cdhash.bytes().all(|byte| byte.is_ascii_hexdigit())
}

fn valid_gateway(gateway: &str) -> bool {
    if gateway.len() > 21 {
        return false;
    }
    match SocketAddrV4::from_str(gateway) {
        Ok(address) => address.port() != 0 && address.to_string() == gateway,
        Err(_) => false,
    }
}

fn parse_manifest(bytes: &[u8]) -> Result<Manifest, Error> {
    let manifest: Manifest = unique_json(bytes).map_err(|_| Error::ManifestRejected)?;
    if manifest.schema != 1
        || manifest.executable_cdhash.len() != 40
        || !manifest
            .executable_cdhash
            .bytes()
            .all(|byte| byte.is_ascii_hexdigit())
        || manifest.executable_sha256 == [0; 32]
        || manifest.plan_digest == [0; 32]
        || !valid_gateway(&manifest.gateway)
        || !socket_access::valid_acl_uuid(&manifest.operator_acl_uuid)
        || manifest.operator_uid == 0
        || manifest.agent_uid == 0
        || manifest
            .enrollment_bootstrap
            .as_ref()
            .is_some_and(|pins| !valid_code_pins(&pins.executable_sha256, &pins.executable_cdhash))
    {
        return Err(Error::ManifestRejected);
    }
    Ok(manifest)
}

/// Verify only the fixed enrollment helper. An installed pin provides integrity
/// under protected provisioning, not proof that a human initiated the process.
pub(crate) fn verify_enrollment_bootstrap() -> Result<(), Error> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    if std::env::current_exe().map_err(|_| Error::InstallationUnavailable)?
        != Path::new(ENROLLMENT_EXECUTABLE)
    {
        return Err(Error::InstallationUnavailable);
    }
    let snapshot =
        macos_config::load_service_manifest().map_err(|_| Error::InstallationUnavailable)?;
    let manifest = parse_manifest(snapshot.as_bytes())?;
    if manifest.operator_uid != manifest.agent_uid {
        return Err(Error::OperatorPolicyRejected);
    }
    let pins = manifest
        .enrollment_bootstrap
        .ok_or(Error::ManifestRejected)?;
    verify_running_image(&pins.executable_cdhash)?;
    if macos_config::hash_installed_service(Path::new(ENROLLMENT_EXECUTABLE))
        .map_err(|_| Error::InstallationUnavailable)?
        != pins.executable_sha256
    {
        return Err(Error::RunningImageRejected);
    }
    Ok(())
}

pub(crate) fn verify_running_image(cdhash: &str) -> Result<(), Error> {
    if cdhash.len() != 40 || !cdhash.bytes().all(|byte| byte.is_ascii_hexdigit()) {
        return Err(Error::RunningImageRejected);
    }
    let requirement = SecRequirement::from_str(&format!("cdhash H\"{cdhash}\""))
        .map_err(|_| Error::RunningImageRejected)?;
    let code = SecCode::for_self(Flags::NONE).map_err(|_| Error::RunningImageRejected)?;
    code.check_validity(
        Flags::STRICT_VALIDATE | Flags::NO_NETWORK_ACCESS,
        &requirement,
    )
    .map_err(|_| Error::RunningImageRejected)
}

pub(crate) fn validate_provisioning_inputs(
    manifest: &[u8],
    plan: &[u8],
    owner: u32,
    now: u64,
) -> Result<(), Error> {
    use core_foundation::url::CFURL;
    use security_framework::os::macos::code_signing::SecStaticCode;
    let manifest = parse_manifest(manifest)?;
    if owner == 0 || manifest.operator_uid != owner || manifest.agent_uid != owner {
        return Err(Error::OperatorPolicyRejected);
    }
    User::from_uid(Uid::from_raw(owner))
        .map_err(|_| Error::OperatorPolicyRejected)?
        .filter(|user| user.gid.as_raw() == manifest.operator_gid)
        .ok_or(Error::OperatorPolicyRejected)?;
    parse_plan(plan, manifest.plan_digest, &manifest.gateway, now)?;
    let bootstrap = manifest
        .enrollment_bootstrap
        .ok_or(Error::ManifestRejected)?;
    for (path, hash, cdhash) in [
        (
            EXECUTABLE,
            manifest.executable_sha256,
            manifest.executable_cdhash,
        ),
        (
            ENROLLMENT_EXECUTABLE,
            bootstrap.executable_sha256,
            bootstrap.executable_cdhash,
        ),
    ] {
        if macos_config::hash_installed_service(Path::new(path))
            .map_err(|_| Error::InstallationUnavailable)?
            != hash
        {
            return Err(Error::RunningImageRejected);
        }
        let url = CFURL::from_path(path, false).ok_or(Error::RunningImageRejected)?;
        let code =
            SecStaticCode::from_path(&url, Flags::NONE).map_err(|_| Error::RunningImageRejected)?;
        let requirement = SecRequirement::from_str(&format!("cdhash H\"{cdhash}\""))
            .map_err(|_| Error::RunningImageRejected)?;
        code.check_validity(
            Flags::STRICT_VALIDATE | Flags::NO_NETWORK_ACCESS,
            &requirement,
        )
        .map_err(|_| Error::RunningImageRejected)?;
    }
    Ok(())
}

fn reference_descriptor_fingerprint() -> [u8; 32] {
    // Same editor evidence tuple as Task020's reviewed descriptor_identity;
    // this is correlation evidence, not KNX wire encoding or device identity.
    let evidence = ("task020-dd0-v1", "1.1.231", 0u8, [0x07u8, 0x05]);
    // A fixed tuple of literals cannot fail JSON serialization. Still retain
    // explicit failure behavior through the caller instead of a panic.
    match serde_json::to_vec(&evidence) {
        Ok(bytes) => Sha256::digest(bytes).into(),
        Err(_) => [0; 32],
    }
}

fn parse_plan(
    bytes: &[u8],
    expected: [u8; 32],
    gateway: &str,
    now: u64,
) -> Result<ConfiguredPlan, Error> {
    let input: guard::PlanInput = unique_json(bytes).map_err(|_| Error::PlanRejected)?;
    let identity = reference_descriptor_fingerprint();
    if input.route.target != "1.1.231"
        || !valid_gateway(gateway)
        || input.route.gateway != gateway
        || input.route.bus_path != "knxnetip-udp"
        || identity == [0; 32]
        || input.route.identity != identity
        || input.expires_at_utc_seconds <= now
    {
        return Err(Error::PlanRejected);
    }
    let plan = ConfiguredPlan::new(input).map_err(|_| Error::PlanRejected)?;
    if plan.digest() != expected {
        return Err(Error::PlanRejected);
    }
    Ok(plan)
}

fn serve_connections(
    listener: &UnixListener,
    startup: &VerifiedStartup,
    execute: Option<ConnectionHandler>,
    deadline: Instant,
    limit: usize,
) -> Result<(), Error> {
    let mut sessions = 0;
    while Instant::now() < deadline && sessions < limit {
        match listener.accept() {
            Ok((stream, _)) => {
                sessions += 1;
                match execute {
                    Some(handle) => {
                        let _ = handle(stream, startup);
                    }
                    None => {
                        let _ = operator_connection::handle_operator_connection(
                            stream,
                            startup.plan(),
                            startup.policy(),
                        );
                    }
                }
            }
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => {
                std::thread::sleep(Duration::from_millis(20))
            }
            Err(_) => return Err(Error::Io),
        }
    }
    Ok(())
}

/// Starts only from the fixed owner-provisioned executable with protected pins.
/// Manifest ownership and matching signatures identify bytes, not human trust;
/// reviewed provisioning and a protected operator client are still required.
/// Every connection remains non-authorizing and no KNX adapter is linked here.
pub fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    run_with_connection(arguments, None)
}

/// Used only by the exact-source integrated entrypoint. Callers cannot supply
/// a startup witness; this function obtains it through the same real checks.
#[allow(dead_code)] // Registered only by the exact-source integrated build.
pub(crate) fn run_with_execution(
    arguments: Vec<OsString>,
    execute: ConnectionHandler,
) -> Result<(), Error> {
    run_with_connection(arguments, Some(execute))
}

fn run_with_connection(
    arguments: Vec<OsString>,
    execute: Option<ConnectionHandler>,
) -> Result<(), Error> {
    if arguments != [OsString::from("--operator-service-only")] {
        return Err(Error::Arguments);
    }
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    if std::env::current_exe().map_err(|_| Error::InstallationUnavailable)? != Path::new(EXECUTABLE)
    {
        return Err(Error::InstallationUnavailable);
    }
    let manifest =
        macos_config::load_service_manifest().map_err(|_| Error::InstallationUnavailable)?;
    let manifest = parse_manifest(manifest.as_bytes())?;
    verify_running_image(&manifest.executable_cdhash)?;
    if macos_config::hash_installed_service(Path::new(EXECUTABLE))
        .map_err(|_| Error::InstallationUnavailable)?
        != manifest.executable_sha256
    {
        return Err(Error::RunningImageRejected);
    }
    let policy = if manifest.operator_uid == manifest.agent_uid {
        ExpectedOperator::same_account(manifest.operator_uid, manifest.operator_gid)
    } else {
        ExpectedOperator::new(
            manifest.operator_uid,
            manifest.operator_gid,
            manifest.agent_uid,
        )
    }
    .map_err(|_| Error::OperatorPolicyRejected)?;
    // Resolve only the selected account; no enumeration or credential access.
    let _operator = User::from_uid(Uid::from_raw(policy.uid()))
        .map_err(|_| Error::OperatorPolicyRejected)?
        .filter(|user| user.uid.as_raw() == policy.uid() && user.gid.as_raw() == policy.gid())
        .ok_or(Error::OperatorPolicyRejected)?;
    let snapshot = macos_config::load_write_plan().map_err(|_| Error::InstallationUnavailable)?;
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map_err(|_| Error::PlanRejected)?
        .as_secs();
    let plan = parse_plan(
        snapshot.as_bytes(),
        manifest.plan_digest,
        &manifest.gateway,
        now,
    )?;
    macos_config::inspect_root_owned_directory(Path::new(STORE))
        .map_err(|_| Error::InstallationUnavailable)?;
    // Operator can search this root-owned directory, never list or replace it.
    // Provisioning is separate: the service never changes directory authority.
    if fs::symlink_metadata(STORE)
        .map_err(|_| Error::InstallationUnavailable)?
        .permissions()
        .mode()
        & 0o7777
        != 0o711
    {
        return Err(Error::InstallationUnavailable);
    }
    match fs::symlink_metadata(SOCKET) {
        Err(error) if error.kind() == std::io::ErrorKind::NotFound => (),
        _ => return Err(Error::EndpointUnavailable),
    }
    let listener = UnixListener::bind(SOCKET).map_err(|_| Error::EndpointUnavailable)?;
    // The protected parent prevents ordinary replacement. A connection in the
    // brief bind/chmod interval still faces kernel configured-peer authentication.
    fs::set_permissions(SOCKET, fs::Permissions::from_mode(0o600))
        .map_err(|_| Error::EndpointUnavailable)?;
    socket_access::restrict_socket(Path::new(SOCKET), policy.uid(), &manifest.operator_acl_uuid)?;
    listener.set_nonblocking(true).map_err(|_| Error::Io)?;
    let deadline = Instant::now().checked_add(LIFETIME).ok_or(Error::Io)?;
    // Closing the listener never unlinks paths. A stale endpoint blocks restart
    // until an independently reviewed operator cleanup has checked its identity.
    let startup = VerifiedStartup { plan, policy };
    serve_connections(&listener, &startup, execute, deadline, MAX_SESSIONS)
}

#[cfg(test)]
pub(crate) fn synthetic_startup_fixture(
    plan: ConfiguredPlan,
    policy: ExpectedOperator,
) -> VerifiedStartup {
    // Test-only values are never installed-host evidence. Runtime qualification
    // still checks actual effective identity and protected journal before use.
    VerifiedStartup { plan, policy }
}

mod socket_access;

#[cfg(test)]
mod tests;
