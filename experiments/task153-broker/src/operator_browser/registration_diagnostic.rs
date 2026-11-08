// SPDX-License-Identifier: Apache-2.0
//! Ordinary-user Safari registration probe. No protected socket or storage.
use super::*;
use crate::operator_connection::{
    Registration, diagnostic_read_registration_begin, diagnostic_read_registration_finish,
    write_reply,
};
use std::os::unix::fs::{FileTypeExt, MetadataExt, PermissionsExt};
use std::os::unix::net::UnixListener;
use std::process::{Command, Stdio};

fn result(stage: &str) -> Value {
    json!({"schema":1,"status":"diagnostic_registration_result","stage":stage,
        "physical_authorization":false,"credential_persisted":false})
}

pub(super) fn validate_result(value: &Value) -> Result<(), Error> {
    if value.as_object().is_none_or(|fields| fields.len() != 5)
        || value["schema"] != 1
        || value["status"] != "diagnostic_registration_result"
        || value["physical_authorization"] != false
        || value["credential_persisted"] != false
        || !matches!(
            value["stage"].as_str(),
            Some("verified" | "service_decode" | "credential_verification" | "session_expired")
        )
    {
        return Err(Error::InvalidReply);
    }
    Ok(())
}

fn trace(stage: &'static str) {
    eprintln!("Synthetic registration stage: {stage}");
}

fn registration_session(stream: &mut UnixStream, deadline: Instant) -> Result<Value, Error> {
    diagnostic_read_registration_begin(stream, deadline).map_err(|_| Error::InvalidReply)?;
    trace("service_begin_decoded");
    let (registration, options) = Registration::begin().map_err(|_| Error::InvalidReply)?;
    write_reply(
        stream,
        &json!({"schema":1,"status":"enrollment_ready","diagnostic":true,
        "options":options,"physical_authorization":false}),
        deadline,
    )
    .map_err(|_| Error::Io)?;
    trace("service_waiting_finish");
    // Parse the actual service Request/Command after the child relay has parsed
    // and serialized its own typed EnrollmentRequest. No storage is reachable.
    let response = match diagnostic_read_registration_finish(stream, deadline) {
        Ok(credential) => {
            trace("service_finish_decoded");
            match registration.finish(credential) {
                Ok(_credential) => result("verified"),
                Err(crate::operator_connection::Error::Timeout) => result("session_expired"),
                Err(_) => result("credential_verification"),
            }
        }
        Err(crate::operator_connection::Error::Timeout) => result("session_expired"),
        Err(_) => result("service_decode"),
    };
    write_reply(stream, &response, deadline).map_err(|_| Error::Io)?;
    trace("service_result_written");
    Ok(response)
}

fn run_pipe_relay(path: &Path, deadline: Instant) -> Result<(), Error> {
    // Only a current-user private temporary diagnostic socket is accepted.
    // This executable has no route to the installed protected operator socket.
    let parent = path.parent().ok_or(Error::Arguments)?;
    if parent.parent() != Some(Path::new("/private/tmp"))
        || !parent.file_name().is_some_and(|name| {
            name.to_string_lossy()
                .starts_with("openknx-registration-diagnostic-")
        })
        || path.file_name() != Some(std::ffi::OsStr::new("socket"))
    {
        return Err(Error::Arguments);
    }
    let directory = std::fs::symlink_metadata(parent).map_err(|_| Error::Denied)?;
    let socket = std::fs::symlink_metadata(path).map_err(|_| Error::Denied)?;
    if !directory.is_dir()
        || directory.uid() != geteuid().as_raw()
        || directory.mode() & 0o7777 != 0o700
        || !socket.file_type().is_socket()
        || socket.uid() != geteuid().as_raw()
    {
        return Err(Error::Denied);
    }
    let mut service = connect_service(path, deadline.min(Instant::now() + Duration::from_secs(2)))?;
    let mut io = enrollment::StandardIo::from_stdio()?;
    enrollment::relay_with_validation(&mut io, &mut service, deadline, validate_result, trace)
}

pub(super) fn run(arguments: Vec<OsString>) -> Result<(), Error> {
    if geteuid().as_raw() == 0 {
        return Err(Error::OperatorRequired);
    }
    let deadline = Instant::now().checked_add(LIFETIME).ok_or(Error::Timeout)?;
    let headless = match arguments.as_slice() {
        [] => false,
        [argument] if argument == "--headless-test" => true,
        [argument, path] if argument == "--synthetic-pipe-relay" => {
            return run_pipe_relay(Path::new(path), deadline);
        }
        _ => return Err(Error::Arguments),
    };
    let directory = tempfile::Builder::new()
        .prefix("openknx-registration-diagnostic-")
        .permissions(std::fs::Permissions::from_mode(0o700))
        .tempdir_in("/private/tmp")
        .map_err(|_| Error::Io)?;
    let path = directory.path().join("socket");
    let listener = UnixListener::bind(&path).map_err(|_| Error::EndpointUnavailable)?;
    std::fs::set_permissions(&path, std::fs::Permissions::from_mode(0o600))
        .map_err(|_| Error::Io)?;
    listener.set_nonblocking(true).map_err(|_| Error::Io)?;
    let child = Command::new(std::env::current_exe().map_err(|_| Error::Io)?)
        .arg("--synthetic-pipe-relay")
        .arg(&path)
        .env_clear()
        .env("PATH", "/usr/bin:/bin:/usr/sbin:/sbin")
        .current_dir("/")
        .stdin(Stdio::piped())
        .stdout(Stdio::piped())
        .stderr(Stdio::inherit())
        .spawn()
        .map_err(|_| Error::BootstrapUnavailable)?;
    let mut browser = enrollment::Connection::from_child(child)?;
    let worker = std::thread::spawn(move || {
        let mut synthetic_service = loop {
            remaining(deadline)?;
            match listener.accept() {
                Ok((stream, _)) => {
                    crate::macos_service::prepare_accepted_stream(&stream)
                        .map_err(|_| Error::Io)?;
                    break stream;
                }
                Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => pause(deadline)?,
                Err(_) => return Err(Error::Io),
            }
        };
        registration_session(&mut synthetic_service, deadline)
    });
    let ready = read_frame(&mut browser, deadline)?;
    let outcome = if headless {
        // Test-driver framing only; normal invocation always uses the unchanged
        // Safari HTTP/JS path and its direct operator click.
        let mut io = enrollment::StandardIo::from_stdio()?;
        write_frame(&mut io, &ready, deadline)?;
        let request = enrollment::read_request_frame(&mut io, deadline)?;
        let response = exchange(&mut browser, &request, deadline)?;
        validate_result(&response)?;
        write_frame(&mut io, &response, deadline)
    } else {
        serve(&mut browser, ready, Mode::Enroll, deadline, true)
    };
    drop(browser);
    let response = worker.join().map_err(|_| Error::Io)??;
    outcome?;
    validate_result(&response)?;
    if !headless {
        println!(
            "Synthetic registration diagnostic: {}. No credential was persisted; no device authorization was issued.",
            response["stage"].as_str().ok_or(Error::InvalidReply)?
        );
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};
    use webauthn_rs::prelude::Url;

    #[test]
    fn real_registration_and_same_relay_parser_are_in_memory_only() {
        for scenario in ["valid", "decode", "verification"] {
            let deadline = Instant::now() + Duration::from_secs(3);
            let (mut browser, mut service) = UnixStream::pair().unwrap();
            let worker = std::thread::spawn(move || registration_session(&mut service, deadline));
            write_frame(&mut browser, &Mode::Enroll.begin(), deadline).unwrap();
            let ready = read_frame(&mut browser, deadline).unwrap();
            assert_eq!(ready["diagnostic"], true);
            let options = serde_json::from_value(ready["options"].clone()).unwrap();
            let mut authenticator = SoftPasskey::new(true);
            let credential = authenticator
                .do_registration(Url::parse(ORIGIN).unwrap(), options)
                .unwrap();
            let credential = if scenario == "decode" {
                json!({"unexpected":"must not be disclosed"})
            } else {
                let mut value = serde_json::to_value(credential).unwrap();
                if scenario == "verification" {
                    value["response"]["clientDataJSON"] = json!("e30");
                }
                value
            };
            let response = exchange(&mut browser, &json!({"schema":1,"command":{"operation":"finish_enrollment","credential":credential}}), deadline).unwrap();
            assert_eq!(
                response["stage"],
                match scenario {
                    "decode" => "service_decode",
                    "verification" => "credential_verification",
                    _ => "verified",
                }
            );
            assert_eq!(validate_result(&response), Ok(()));
            assert_eq!(
                validate_enrollment_result(&response),
                Err(Error::InvalidReply)
            );
            assert!(!response.to_string().contains("must not be disclosed"));
            assert_eq!(worker.join().unwrap().unwrap(), response);
        }
    }

    #[test]
    fn diagnostic_result_rejects_extra_data_and_real_enrollment_status() {
        let mut response = result("verified");
        response["credential"] = json!("must not be disclosed");
        assert_eq!(validate_result(&response), Err(Error::InvalidReply));
        assert_eq!(
            validate_result(
                &json!({"schema":1,"status":"enrollment_activated","physical_authorization":false})
            ),
            Err(Error::InvalidReply)
        );
        assert_eq!(run(vec!["--enroll".into()]), Err(Error::Arguments));
    }
}
