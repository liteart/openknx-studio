// SPDX-License-Identifier: Apache-2.0
//! Root-only fixed-endpoint diagnostic, never a device approval issuer.

use nix::unistd::{getegid, geteuid};
use std::ffi::OsString;
use std::fs::{self, DirBuilder};
use std::io::Write;
use std::os::unix::fs::{DirBuilderExt, MetadataExt, PermissionsExt};
use std::os::unix::net::{UnixListener, UnixStream};
use std::time::{Duration, Instant};
use task153_broker_core::macos_peer::{ExpectedOperator, check_operator_peer};

const DIRECTORY: &str = "/private/var/db/org.openknxstudio.operator-diagnostic";
const SOCKET: &str = "/private/var/db/org.openknxstudio.operator-diagnostic/peer.sock";
const LIFETIME: Duration = Duration::from_secs(300);
const MAX_CONNECTIONS: usize = 32;
const CONFIRMED: &[u8] =
    b"{\"schema\":1,\"peer\":\"operator_identity_matched\",\"physical_authorization\":false}\n";
const DENIED: &[u8] = b"{\"schema\":1,\"peer\":\"denied\",\"physical_authorization\":false}\n";

#[derive(Debug, PartialEq, Eq)]
pub(crate) enum Error {
    Arguments,
    RootRequired,
    PathUnavailable,
    Io,
}

fn parse_arguments(arguments: &[OsString]) -> Result<u32, Error> {
    if arguments.len() != 3 || arguments[0] != "--diagnostic-only" || arguments[1] != "--agent-uid"
    {
        return Err(Error::Arguments);
    }
    let uid = arguments[2]
        .to_str()
        .and_then(|text| text.parse::<u32>().ok())
        .filter(|uid| *uid != 0)
        .ok_or(Error::Arguments)?;
    Ok(uid)
}

fn require_root() -> Result<(), Error> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootRequired);
    }
    Ok(())
}

fn check_fixed_ancestors() -> Result<(), Error> {
    for path in ["/", "/private", "/private/var", "/private/var/db"] {
        let metadata = fs::symlink_metadata(path).map_err(|_| Error::PathUnavailable)?;
        if !metadata.is_dir()
            || metadata.file_type().is_symlink()
            || metadata.uid() != 0
            || metadata.mode() & 0o022 != 0
        {
            return Err(Error::PathUnavailable);
        }
    }
    Ok(())
}

struct Endpoint;

impl Endpoint {
    fn create() -> Result<(Self, UnixListener), Error> {
        check_fixed_ancestors()?;
        // Exclusive creation: never reuse, chmod, chown or unlink an existing
        // object. The newly created private directory contains the socket.
        DirBuilder::new()
            .mode(0o700)
            .create(DIRECTORY)
            .map_err(|_| Error::PathUnavailable)?;
        let endpoint = Self;
        let listener = UnixListener::bind(SOCKET).map_err(|_| Error::Io)?;
        fs::set_permissions(SOCKET, fs::Permissions::from_mode(0o600)).map_err(|_| Error::Io)?;
        listener.set_nonblocking(true).map_err(|_| Error::Io)?;
        Ok((endpoint, listener))
    }
}

impl Drop for Endpoint {
    fn drop(&mut self) {
        // Best-effort fixed-path cleanup is not inode-anchored. A leftover
        // after a crash is deliberately not reused; errors are ignored.
        let _ = fs::remove_file(SOCKET);
        let _ = fs::remove_dir(DIRECTORY);
    }
}

fn respond_to_peer(stream: &mut UnixStream, operator: &ExpectedOperator) -> Result<(), Error> {
    let response = if check_operator_peer(stream, operator).is_ok() {
        CONFIRMED
    } else {
        DENIED
    };
    // No request body is consumed, credential is inspected, or identity echoed.
    // Matching is only a diagnostic result and never an execution capability.
    stream
        .set_write_timeout(Some(Duration::from_secs(1)))
        .and_then(|()| stream.write_all(response))
        .map_err(|_| Error::Io)
}

pub(crate) fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    let agent_uid = parse_arguments(&arguments)?;
    require_root()?;
    let operator = ExpectedOperator::new(0, 0, agent_uid).map_err(|_| Error::Arguments)?;
    let (_endpoint, listener) = Endpoint::create()?;
    println!("Temporary macOS peer diagnostic at {SOCKET}");
    println!("No enrollment, approval or device operation is available. Expires after 5 minutes.");
    let started = Instant::now();
    let mut connections = 0;
    while started.elapsed() < LIFETIME && connections < MAX_CONNECTIONS {
        match listener.accept() {
            Ok((mut stream, _)) => {
                connections += 1;
                let _ = respond_to_peer(&mut stream, &operator);
            }
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => {
                std::thread::sleep(Duration::from_millis(20));
            }
            Err(_) => return Err(Error::Io),
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Read;

    fn arguments(values: &[&str]) -> Vec<OsString> {
        values.iter().map(OsString::from).collect()
    }

    #[test]
    fn arbitrary_paths_and_nonexplicit_or_root_agent_arguments_are_rejected() {
        for values in [
            vec![],
            vec!["--agent-uid", "501"],
            vec!["--diagnostic-only", "--socket", "/tmp/other"],
            vec!["--diagnostic-only", "--agent-uid", "0"],
            vec!["--diagnostic-only", "--agent-uid", "not-a-uid"],
            vec!["--diagnostic-only", "--agent-uid", "501", "extra"],
        ] {
            assert_eq!(parse_arguments(&arguments(&values)), Err(Error::Arguments));
        }
        assert_eq!(
            parse_arguments(&arguments(&["--diagnostic-only", "--agent-uid", "501"])),
            Ok(501)
        );
    }

    #[test]
    fn actual_nonroot_process_cannot_reach_endpoint_creation() {
        if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
            assert_eq!(
                run(arguments(&["--diagnostic-only", "--agent-uid", "501"])),
                Err(Error::RootRequired)
            );
        }
    }

    #[test]
    fn fixed_ancestor_preflight_checks_actual_host_metadata_without_writing() {
        assert_eq!(check_fixed_ancestors(), Ok(()));
    }

    #[test]
    fn real_local_agent_connection_gets_only_fixed_denial_without_a_grant() {
        let directory = tempfile::Builder::new()
            .prefix("task153-diag-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let path = directory.path().join("socket");
        let listener = UnixListener::bind(path.clone()).unwrap();
        let mut client = UnixStream::connect(path).unwrap();
        let (mut server, _) = listener.accept().unwrap();
        let uid = geteuid().as_raw();
        let operator =
            ExpectedOperator::new(uid.checked_add(1).unwrap(), getegid().as_raw(), uid).unwrap();
        respond_to_peer(&mut server, &operator).unwrap();
        drop(server);
        let mut response = Vec::new();
        client.read_to_end(&mut response).unwrap();
        assert_eq!(response, DENIED);
        assert!(response.len() < 128);
    }
}
