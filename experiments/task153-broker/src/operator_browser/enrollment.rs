// SPDX-License-Identifier: Apache-2.0
//! Fixed-purpose enrollment relay. No password input or generic privileged IPC.

use super::*;
use nix::fcntl::{FcntlArg, OFlag, fcntl};
use std::os::fd::AsFd;
use std::process::{Child, ChildStdin, ChildStdout, Command, Stdio};
use webauthn_rs::prelude::RegisterPublicKeyCredential;

fn nonblocking(fd: &impl AsFd) -> std::io::Result<()> {
    let flags = fcntl(fd, FcntlArg::F_GETFL).map_err(std::io::Error::from)?;
    fcntl(
        fd,
        FcntlArg::F_SETFL(OFlag::from_bits_retain(flags) | OFlag::O_NONBLOCK),
    )
    .map(|_| ())
    .map_err(std::io::Error::from)
}

pub(super) struct ProcessIo {
    input: Option<ChildStdin>,
    output: ChildStdout,
    child: Child,
}

pub(super) enum Connection {
    Direct(UnixStream),
    Enrollment(ProcessIo),
}

fn bootstrap_command() -> Command {
    let mut command = Command::new("/usr/bin/sudo");
    // Fixed executable, no shell, askpass, -S, password environment or arbitrary
    // helper arguments. sudo obtains authentication through the user's terminal.
    // -k ignores cached authentication for this command under sudoers policy.
    command
        .args(["-k", "--", crate::macos_service::ENROLLMENT_EXECUTABLE])
        .env_clear()
        .env("PATH", "/usr/bin:/bin:/usr/sbin:/sbin")
        .current_dir("/")
        .stdin(Stdio::piped())
        .stdout(Stdio::piped())
        .stderr(Stdio::inherit());
    command
}

impl Connection {
    pub(super) fn start() -> Result<Self, Error> {
        let mut child = bootstrap_command()
            .spawn()
            .map_err(|_| Error::BootstrapUnavailable)?;
        let input = child.stdin.take().ok_or(Error::BootstrapUnavailable)?;
        let output = child.stdout.take().ok_or(Error::BootstrapUnavailable)?;
        Ok(Self::Enrollment(ProcessIo {
            input: Some(input),
            output,
            child,
        }))
    }
}

impl Read for Connection {
    fn read(&mut self, bytes: &mut [u8]) -> std::io::Result<usize> {
        match self {
            Self::Direct(stream) => stream.read(bytes),
            Self::Enrollment(io) => io.output.read(bytes),
        }
    }
}

impl Write for Connection {
    fn write(&mut self, bytes: &[u8]) -> std::io::Result<usize> {
        match self {
            Self::Direct(stream) => stream.write(bytes),
            Self::Enrollment(io) => io
                .input
                .as_mut()
                .ok_or(std::io::ErrorKind::BrokenPipe)?
                .write(bytes),
        }
    }
    fn flush(&mut self) -> std::io::Result<()> {
        match self {
            Self::Direct(stream) => stream.flush(),
            Self::Enrollment(io) => io
                .input
                .as_mut()
                .ok_or(std::io::ErrorKind::BrokenPipe)?
                .flush(),
        }
    }
}

impl TimedIo for Connection {
    fn nonblocking(&self) -> std::io::Result<()> {
        match self {
            Self::Direct(stream) => stream.set_nonblocking(true),
            Self::Enrollment(io) => {
                nonblocking(&io.output)?;
                nonblocking(io.input.as_ref().ok_or(std::io::ErrorKind::BrokenPipe)?)
            }
        }
    }
}

impl Drop for ProcessIo {
    fn drop(&mut self) {
        // EOF prevents a still-waiting relay from receiving a registration.
        // Do not wait indefinitely for privileged storage synchronization.
        self.input.take();
        let deadline = Instant::now() + Duration::from_millis(100);
        loop {
            match self.child.try_wait() {
                Ok(Some(_)) | Err(_) => return,
                Ok(None) if Instant::now() < deadline => {
                    std::thread::sleep(Duration::from_millis(5));
                }
                Ok(None) => {
                    let _ = self.child.kill();
                    return;
                }
            }
        }
    }
}

struct StandardIo {
    input: std::fs::File,
    output: std::fs::File,
}
impl StandardIo {
    fn from_files(input: std::fs::File, output: std::fs::File) -> Result<Self, Error> {
        use std::os::unix::fs::FileTypeExt;
        if !input
            .metadata()
            .map_err(|_| Error::Io)?
            .file_type()
            .is_fifo()
            || !output
                .metadata()
                .map_err(|_| Error::Io)?
                .file_type()
                .is_fifo()
        {
            return Err(Error::Denied);
        }
        Ok(Self { input, output })
    }
}
impl Read for StandardIo {
    fn read(&mut self, bytes: &mut [u8]) -> std::io::Result<usize> {
        self.input.read(bytes)
    }
}
impl Write for StandardIo {
    fn write(&mut self, bytes: &[u8]) -> std::io::Result<usize> {
        self.output.write(bytes)
    }
    fn flush(&mut self) -> std::io::Result<()> {
        self.output.flush()
    }
}
impl TimedIo for StandardIo {
    fn nonblocking(&self) -> std::io::Result<()> {
        nonblocking(&self.input)?;
        nonblocking(&self.output)
    }
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct EnrollmentRequest {
    schema: u8,
    command: EnrollmentFinish,
}
#[derive(Deserialize)]
#[serde(tag = "operation", rename_all = "snake_case", deny_unknown_fields)]
enum EnrollmentFinish {
    FinishEnrollment {
        credential: RegisterPublicKeyCredential,
    },
}

fn validate_finish(value: Value) -> Result<Value, Error> {
    let request: EnrollmentRequest = serde_json::from_value(value).map_err(|_| Error::Denied)?;
    if request.schema != 1 {
        return Err(Error::Denied);
    }
    let EnrollmentFinish::FinishEnrollment { credential } = request.command;
    Ok(json!({"schema":1,"command":{"operation":"finish_enrollment","credential":credential}}))
}

fn relay(io: &mut impl TimedIo, service: &mut UnixStream, deadline: Instant) -> Result<(), Error> {
    let ready = exchange(service, &Mode::Enroll.begin(), deadline)?;
    validate_ready(&ready, Mode::Enroll)?;
    write_frame(io, &ready, deadline)?;
    let finish = validate_finish(read_request_frame(io, deadline)?)?;
    let result =
        exchange(service, &finish, deadline).map_err(|_| Error::EnrollmentReplyUnavailable)?;
    validate_enrollment_result(&result)?;
    write_frame(io, &result, deadline)
}

// Requests do not carry the reply-only physical_authorization envelope.
fn read_request_frame(io: &mut impl TimedIo, deadline: Instant) -> Result<Value, Error> {
    let mut size = [0; 4];
    read_exact(io, &mut size, deadline)?;
    let size = u32::from_be_bytes(size) as usize;
    if size == 0 || size > MAX_COMMAND {
        return Err(Error::InvalidReply);
    }
    let mut bytes = vec![0; size];
    read_exact(io, &mut bytes, deadline)?;
    unique_json(&bytes).map_err(|_| Error::InvalidReply)
}

pub(super) fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    if !arguments.is_empty() {
        return Err(Error::Arguments);
    }
    crate::macos_service::verify_enrollment_bootstrap().map_err(|_| Error::BootstrapUnavailable)?;
    let deadline = Instant::now().checked_add(LIFETIME).ok_or(Error::Timeout)?;
    let mut service = connect_service(
        Path::new(SOCKET),
        deadline.min(Instant::now() + Duration::from_secs(2)),
    )?;
    require_root_server(&service)?;
    // Use unbuffered duplicated pipe descriptors: a buffered Stdout could keep
    // the ready frame until process exit while the browser waits for it.
    let input = std::io::stdin()
        .as_fd()
        .try_clone_to_owned()
        .map(std::fs::File::from)
        .map_err(|_| Error::Io)?;
    let output = std::io::stdout()
        .as_fd()
        .try_clone_to_owned()
        .map(std::fs::File::from)
        .map_err(|_| Error::Io)?;
    let mut io = StandardIo::from_files(input, output)?;
    relay(&mut io, &mut service, deadline)
}

#[cfg(test)]
mod tests;
