// SPDX-License-Identifier: Apache-2.0
//! Separate-operator browser bridge. No grants, installation or KNX adapter.

use crate::{macos_peer::inspect_peer, root_passkey::unique_json};
use nix::unistd::geteuid;
use security_framework::random::SecRandom;
use serde::Deserialize;
use serde_json::{Value, json};
use std::ffi::OsString;
use std::io::{Read, Write};
use std::net::{TcpListener, TcpStream};
use std::os::unix::net::UnixStream;
use std::path::Path;
use std::time::{Duration, Instant};

mod bootstrap;
mod enrollment;
mod http;
const SOCKET: &str = "/private/var/db/org.openknxstudio.operator/operator.sock";
const ADDRESS: &str = "127.0.0.1:15300";
const ORIGIN: &str = "http://localhost:15300";
const MAX_FRAME: usize = 65_536;
const MAX_COMMAND: usize = 32_768;
const LIFETIME: Duration = Duration::from_secs(110);

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    Arguments,
    OperatorRequired,
    ServiceUnavailable,
    InvalidReply,
    EndpointUnavailable,
    RandomUnavailable,
    BootstrapUnavailable,
    InvalidHttp,
    Denied,
    Timeout,
    Io,
}

#[derive(Clone, Copy)]
enum Mode {
    Enroll,
    Assert,
}
impl Mode {
    fn begin(self) -> Value {
        match self {
            Self::Enroll => {
                json!({"schema":1,"command":{"operation":"begin_enrollment","issuer":"operator-browser"}})
            }
            Self::Assert => json!({"schema":1,"command":{"operation":"begin_assertion"}}),
        }
    }
    fn ready(self) -> &'static str {
        match self {
            Self::Enroll => "enrollment_ready",
            Self::Assert => "assertion_ready",
        }
    }
    fn finish(self) -> &'static str {
        match self {
            Self::Enroll => "finish_enrollment",
            Self::Assert => "finish_assertion",
        }
    }
}

trait TimedIo: Read + Write {
    fn nonblocking(&self) -> std::io::Result<()>;
}
macro_rules! timed_io {
    ($kind:ty) => {
        impl TimedIo for $kind {
            fn nonblocking(&self) -> std::io::Result<()> {
                self.set_nonblocking(true)
            }
        }
    };
}
timed_io!(UnixStream);
timed_io!(TcpStream);
fn remaining(deadline: Instant) -> Result<Duration, Error> {
    deadline
        .checked_duration_since(Instant::now())
        .filter(|time| !time.is_zero())
        .ok_or(Error::Timeout)
}
fn pause(deadline: Instant) -> Result<(), Error> {
    std::thread::sleep(remaining(deadline)?.min(Duration::from_millis(1)));
    Ok(())
}
fn read_exact(io: &mut impl TimedIo, mut bytes: &mut [u8], deadline: Instant) -> Result<(), Error> {
    io.nonblocking().map_err(|_| Error::Io)?;
    while !bytes.is_empty() {
        remaining(deadline)?;
        match io.read(bytes) {
            Ok(0) => return Err(Error::Io),
            Ok(size) => bytes = &mut bytes[size..],
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => pause(deadline)?,
            Err(error) if error.kind() == std::io::ErrorKind::Interrupted => (),
            Err(_) => return Err(Error::Io),
        }
    }
    Ok(())
}
fn write_all(io: &mut impl TimedIo, mut bytes: &[u8], deadline: Instant) -> Result<(), Error> {
    io.nonblocking().map_err(|_| Error::Io)?;
    while !bytes.is_empty() {
        remaining(deadline)?;
        match io.write(bytes) {
            Ok(0) => return Err(Error::Io),
            Ok(size) => bytes = &bytes[size..],
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => pause(deadline)?,
            Err(error) if error.kind() == std::io::ErrorKind::Interrupted => (),
            Err(_) => return Err(Error::Io),
        }
    }
    Ok(())
}
fn write_frame(stream: &mut impl TimedIo, command: &Value, deadline: Instant) -> Result<(), Error> {
    write_bounded_frame(stream, command, MAX_FRAME, deadline)
}
fn write_bounded_frame(
    stream: &mut impl TimedIo,
    command: &Value,
    maximum: usize,
    deadline: Instant,
) -> Result<(), Error> {
    let bytes = serde_json::to_vec(command).map_err(|_| Error::InvalidReply)?;
    if bytes.len() > maximum {
        return Err(Error::InvalidReply);
    }
    write_all(stream, &(bytes.len() as u32).to_be_bytes(), deadline)?;
    write_all(stream, &bytes, deadline)
}
fn read_frame(stream: &mut impl TimedIo, deadline: Instant) -> Result<Value, Error> {
    let mut size = [0; 4];
    read_exact(stream, &mut size, deadline)?;
    let size = u32::from_be_bytes(size) as usize;
    if size == 0 || size > MAX_FRAME {
        return Err(Error::InvalidReply);
    }
    let mut bytes = vec![0; size];
    read_exact(stream, &mut bytes, deadline)?;
    let value: Value = unique_json(&bytes).map_err(|_| Error::InvalidReply)?;
    if value["schema"] != 1 {
        return Err(Error::InvalidReply);
    }
    if value["status"] == "guarded_execution_report" {
        let report: crate::operator_connection::GuardedExecutionReply =
            serde_json::from_value(value["execution"].clone()).map_err(|_| Error::InvalidReply)?;
        if value["execution_mode"] != "guarded_write"
            || value.get("physical_authorization").is_some()
            || value["succeeded"] != report.succeeded()
        {
            return Err(Error::InvalidReply);
        }
    } else if value["physical_authorization"] != false {
        return Err(Error::InvalidReply);
    }
    Ok(value)
}
fn exchange(stream: &mut impl TimedIo, command: &Value, deadline: Instant) -> Result<Value, Error> {
    write_bounded_frame(stream, command, MAX_COMMAND, deadline)?;
    read_frame(stream, deadline)
}
fn connect_service(path: &Path, deadline: Instant) -> Result<UnixStream, Error> {
    remaining(deadline)?;
    // Safe upstream wrapper sets CLOEXEC and Apple NOSIGPIPE. Its timeout
    // path performs nonblocking connect and polls using elapsed time.
    let socket = socket2::Socket::new(socket2::Domain::UNIX, socket2::Type::STREAM, None)
        .map_err(|_| Error::ServiceUnavailable)?;
    let address = socket2::SockAddr::unix(path).map_err(|_| Error::ServiceUnavailable)?;
    socket
        .connect_timeout(&address, remaining(deadline)?)
        .map_err(|error| {
            if error.kind() == std::io::ErrorKind::TimedOut {
                Error::Timeout
            } else {
                Error::ServiceUnavailable
            }
        })?;
    remaining(deadline)?;
    socket.peer_addr().map_err(|_| Error::ServiceUnavailable)?;
    socket
        .set_nonblocking(true)
        .map_err(|_| Error::ServiceUnavailable)?;
    Ok(UnixStream::from(std::os::fd::OwnedFd::from(socket)))
}

fn require_root_server(stream: &UnixStream) -> Result<(), Error> {
    let peer = inspect_peer(stream).map_err(|_| Error::ServiceUnavailable)?;
    if peer.uid() != 0 || peer.gid() != 0 {
        return Err(Error::ServiceUnavailable);
    }
    Ok(())
}
fn validate_ready(value: &Value, mode: Mode) -> Result<(), Error> {
    if value["status"] != mode.ready() || !value["options"]["publicKey"].is_object() {
        return Err(Error::InvalidReply);
    }
    if matches!(mode, Mode::Assert)
        && (!value["plan_summary"].is_string()
            || !matches!(
                value["execution_mode"].as_str(),
                Some("observe_only" | "guarded_write")
            )
            || value["plan_digest"].as_array().is_none_or(|digest| {
                digest.len() != 32
                    || digest
                        .iter()
                        .any(|byte| byte.as_u64().is_none_or(|byte| byte > 255))
            }))
    {
        return Err(Error::InvalidReply);
    }
    Ok(())
}
#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Finish {
    credential: Value,
}

/// Run in the configured operator account. The protected
/// service must accept this connection before a secret or bootstrap is created.
/// An operator browser bridge is not approval or permission to execute KNX.
pub fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    let mode = match arguments.as_slice() {
        [argument] if argument == "--enroll" => Mode::Enroll,
        [argument] if argument == "--assert" => Mode::Assert,
        _ => return Err(Error::Arguments),
    };
    if geteuid().as_raw() == 0 {
        return Err(Error::OperatorRequired);
    }
    let deadline = Instant::now().checked_add(LIFETIME).ok_or(Error::Timeout)?;
    let connect_deadline = deadline.min(Instant::now() + Duration::from_secs(2));
    let (mut service, ready) = match mode {
        Mode::Assert => {
            let stream = connect_service(Path::new(SOCKET), connect_deadline)?;
            require_root_server(&stream)?;
            let mut connection = enrollment::Connection::Direct(stream);
            let ready = exchange(&mut connection, &mode.begin(), deadline)?;
            (connection, ready)
        }
        Mode::Enroll => {
            let mut connection = enrollment::Connection::start()?;
            let ready = read_frame(&mut connection, deadline)?;
            (connection, ready)
        }
    };
    validate_ready(&ready, mode)?;
    let listener = TcpListener::bind(ADDRESS).map_err(|_| Error::EndpointUnavailable)?;
    listener.set_nonblocking(true).map_err(|_| Error::Io)?;
    let mut random = [0; 32];
    SecRandom::default()
        .copy_bytes(&mut random)
        .map_err(|_| Error::RandomUnavailable)?;
    let secret: String = random.iter().map(|byte| format!("{byte:02x}")).collect();
    let bootstrap = bootstrap::Bootstrap::create(&secret)?;
    bootstrap.open_safari()?;
    let mut viewed = false;
    for _ in 0..64 {
        let mut stream = loop {
            remaining(deadline)?;
            match listener.accept() {
                Ok((stream, peer)) if peer.ip().is_loopback() => break stream,
                Ok(_) => return Err(Error::Denied),
                Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => {
                    std::thread::sleep(Duration::from_millis(10))
                }
                Err(_) => return Err(Error::Io),
            }
        };
        let request_deadline = deadline.min(Instant::now() + Duration::from_secs(2));
        let request = match http::read_request(&mut stream, request_deadline) {
            Ok(request) => request,
            Err(_) => {
                let _ = http::reply(
                    &mut stream,
                    403,
                    "text/plain",
                    b"Request denied",
                    request_deadline,
                );
                continue;
            }
        };
        match http::classify(&request, &secret) {
            Ok(http::Action::Page) => http::reply(
                &mut stream,
                200,
                "text/html",
                include_bytes!("operator_browser/browser.html"),
                request_deadline,
            )?,
            Ok(http::Action::Script) => http::reply(
                &mut stream,
                200,
                "text/javascript",
                include_bytes!("operator_browser/browser.js"),
                request_deadline,
            )?,
            Ok(http::Action::Ready) if !viewed => {
                viewed = true;
                // No further browser-file access is needed after authenticated
                // bootstrap. The private directory remains until process exit.
                bootstrap.remove_file()?;
                http::reply(
                    &mut stream,
                    200,
                    "application/json",
                    &serde_json::to_vec(&ready).map_err(|_| Error::InvalidReply)?,
                    request_deadline,
                )?;
            }
            Ok(http::Action::Finish) if viewed => {
                let finish: Finish = unique_json(&request.body).map_err(|_| Error::InvalidHttp)?;
                let response = exchange(
                    &mut service,
                    &json!({"schema":1,"command":{"operation":mode.finish(),"credential":finish.credential}}),
                    deadline,
                )?;
                let valid = match mode {
                    Mode::Enroll => matches!(
                        response["status"].as_str(),
                        Some("enrollment_activated" | "enrollment_committed_cleanup_uncertain")
                    ),
                    Mode::Assert if ready["execution_mode"] == "guarded_write" => {
                        response["status"] == "guarded_execution_report"
                    }
                    Mode::Assert => response["status"] == "assertion_verified",
                };
                if !valid {
                    return Err(Error::InvalidReply);
                }
                http::reply(
                    &mut stream,
                    200,
                    "application/json",
                    &serde_json::to_vec(&response).map_err(|_| Error::InvalidReply)?,
                    deadline,
                )?;
                return Ok(());
            }
            _ => {
                let _ = http::reply(
                    &mut stream,
                    403,
                    "text/plain",
                    b"Request denied",
                    request_deadline,
                );
            }
        }
    }
    Err(Error::Denied)
}

/// Dedicated installed root helper entry point. It only relays one first-key
/// enrollment ceremony and cannot select a plan, assert, or execute a device.
pub fn run_enrollment_bootstrap(arguments: Vec<OsString>) -> Result<(), Error> {
    enrollment::run(arguments)
}

#[cfg(test)]
mod tests;
