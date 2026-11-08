// SPDX-License-Identifier: Apache-2.0
//! Temporary transports and synthetic credentials only; sudo is never invoked.
use super::*;
use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};

#[test]
fn bootstrap_command_is_fixed_and_never_collects_password_over_its_pipe() {
    let command = bootstrap_command();
    assert_eq!(command.get_program(), "/usr/bin/sudo");
    assert_eq!(
        command.get_args().collect::<Vec<_>>(),
        ["-k", "--", crate::macos_service::ENROLLMENT_EXECUTABLE]
    );
    assert_eq!(command.get_current_dir(), Some(Path::new("/")));
    assert_eq!(
        command.get_envs().collect::<Vec<_>>(),
        [(
            std::ffi::OsStr::new("PATH"),
            Some(std::ffi::OsStr::new("/usr/bin:/bin:/usr/sbin:/sbin"))
        )]
    );
}

#[test]
fn root_helper_denies_arguments_and_actual_unprivileged_invocation_before_stdio() {
    assert_eq!(run(vec!["--assert".into()]), Err(Error::Arguments));
    assert_eq!(
        run(vec!["--socket".into(), "/tmp/arbitrary".into()]),
        Err(Error::Arguments)
    );
    if geteuid().as_raw() != 0 {
        assert_eq!(run(vec![]), Err(Error::BootstrapUnavailable));
    }
}

#[test]
fn relay_stdio_requires_actual_pipes_and_emits_unbuffered_frames() {
    let (input, input_writer) = nix::unistd::pipe().unwrap();
    let (output_reader, output) = nix::unistd::pipe().unwrap();
    let mut io = StandardIo::from_files(input.into(), output.into()).unwrap();
    let reply = json!({"schema":1,"physical_authorization":false,"status":"enrollment_ready"});
    write_frame(&mut io, &reply, Instant::now() + Duration::from_secs(1)).unwrap();
    let mut reader = std::fs::File::from(output_reader);
    nonblocking(&reader).unwrap();
    let mut prefix = [0; 4];
    reader.read_exact(&mut prefix).unwrap();
    let mut bytes = vec![0; u32::from_be_bytes(prefix) as usize];
    reader.read_exact(&mut bytes).unwrap();
    assert_eq!(serde_json::from_slice::<Value>(&bytes).unwrap(), reply);
    drop(input_writer);
    let file = tempfile::tempfile().unwrap();
    assert!(matches!(
        StandardIo::from_files(file.try_clone().unwrap(), file),
        Err(Error::Denied)
    ));
}

#[test]
fn relay_cannot_forward_assertion_write_or_caller_selected_identity() {
    for value in [
        json!({"schema":1,"command":{"operation":"begin_assertion"}}),
        json!({"schema":1,"command":{"operation":"finish_assertion","credential":{}}}),
        json!({"schema":1,"command":{"operation":"write","target":"1.1.231"}}),
        json!({"schema":1,"command":{"operation":"finish_enrollment","credential":{}},"uid":0}),
    ] {
        assert!(validate_finish(value).is_err());
    }
}

#[test]
fn relay_request_bounds_duplicate_keys_and_eof_are_enforced() {
    for bytes in [
        Vec::new(),
        (0u32).to_be_bytes().to_vec(),
        ((MAX_COMMAND + 1) as u32).to_be_bytes().to_vec(),
        {
            let body = b"{\"schema\":1,\"schema\":1}";
            let mut frame = (body.len() as u32).to_be_bytes().to_vec();
            frame.extend_from_slice(body);
            frame
        },
    ] {
        let (mut client, mut server) = UnixStream::pair().unwrap();
        client.write_all(&bytes).unwrap();
        client.shutdown(std::net::Shutdown::Write).unwrap();
        assert!(read_request_frame(&mut server, Instant::now() + Duration::from_secs(1)).is_err());
    }
}

#[test]
fn expired_relay_never_forwards_input_or_renews_the_ceremony() {
    let (mut browser, mut io) = UnixStream::pair().unwrap();
    let (mut relay_service, mut service) = UnixStream::pair().unwrap();
    browser.write_all(b"synthetic queued input").unwrap();
    assert_eq!(
        relay(&mut io, &mut relay_service, Instant::now()),
        Err(Error::Timeout)
    );
    service.set_nonblocking(true).unwrap();
    assert_eq!(
        service.read(&mut [0; 1]).unwrap_err().kind(),
        std::io::ErrorKind::WouldBlock
    );
}

#[test]
fn challenge_failure_cannot_be_mistaken_for_a_ready_ceremony() {
    let response = json!({"schema":1,"status":"enrollment_failed","stage":"challenge","storage_state":"not_started_by_this_attempt","physical_authorization":false});
    assert_eq!(validate_enrollment_result(&response), Ok(()));
    assert_eq!(
        validate_ready(&response, Mode::Enroll),
        Err(Error::InvalidReply)
    );
}

#[test]
fn real_synthetic_registration_crosses_only_the_two_enrollment_frames() {
    let origin = webauthn_rs::prelude::Url::parse(crate::root_passkey::OPERATOR_ORIGIN).unwrap();
    let webauthn = crate::root_passkey::operator_webauthn().unwrap();
    let (options, state) = webauthn
        .start_passkey_registration(
            webauthn_rs::prelude::Uuid::new_v4(),
            "synthetic",
            "Synthetic operator",
            None,
        )
        .unwrap();
    let (mut browser, mut io) = UnixStream::pair().unwrap();
    let (mut relay_service, mut service) = UnixStream::pair().unwrap();
    let service_thread = std::thread::spawn(move || {
        let deadline = Instant::now() + Duration::from_secs(3);
        assert_eq!(
            read_request_frame(&mut service, deadline).unwrap(),
            Mode::Enroll.begin()
        );
        write_frame(&mut service, &json!({"schema":1,"status":"enrollment_ready","options":options,"physical_authorization":false}), deadline).unwrap();
        let finish = validate_finish(read_request_frame(&mut service, deadline).unwrap()).unwrap();
        let credential = serde_json::from_value(finish["command"]["credential"].clone()).unwrap();
        webauthn
            .finish_passkey_registration(&credential, &state)
            .unwrap();
        write_frame(
            &mut service,
            &json!({"schema":1,"status":"enrollment_activated","physical_authorization":false}),
            deadline,
        )
        .unwrap();
    });
    let relay_thread = std::thread::spawn(move || {
        relay(
            &mut io,
            &mut relay_service,
            Instant::now() + Duration::from_secs(3),
        )
    });
    let deadline = Instant::now() + Duration::from_secs(3);
    let ready = read_frame(&mut browser, deadline).unwrap();
    validate_ready(&ready, Mode::Enroll).unwrap();
    let options = serde_json::from_value(ready["options"].clone()).unwrap();
    let mut authenticator = SoftPasskey::new(true);
    let credential = authenticator.do_registration(origin, options).unwrap();
    let reply = exchange(
        &mut browser,
        &json!({"schema":1,"command":{"operation":"finish_enrollment","credential":credential}}),
        deadline,
    )
    .unwrap();
    assert_eq!(reply["status"], "enrollment_activated");
    assert_eq!(reply["physical_authorization"], false);
    assert_eq!(relay_thread.join().unwrap(), Ok(()));
    service_thread.join().unwrap();
}

#[test]
fn relay_preserves_sanitized_service_failure_and_distinguishes_lost_service_reply() {
    for failed_reply in [true, false] {
        let webauthn = crate::root_passkey::operator_webauthn().unwrap();
        let (options, _) = webauthn
            .start_passkey_registration(
                webauthn_rs::prelude::Uuid::new_v4(),
                "synthetic",
                "Synthetic",
                None,
            )
            .unwrap();
        let (mut browser, mut io) = UnixStream::pair().unwrap();
        let (mut relay_service, mut service) = UnixStream::pair().unwrap();
        let service_thread = std::thread::spawn(move || {
            let deadline = Instant::now() + Duration::from_secs(3);
            read_request_frame(&mut service, deadline).unwrap();
            write_frame(&mut service, &json!({"schema":1,"status":"enrollment_ready","options":options,"physical_authorization":false}), deadline).unwrap();
            read_request_frame(&mut service, deadline).unwrap();
            if failed_reply {
                write_frame(&mut service, &json!({"schema":1,"status":"enrollment_failed","stage":"credential_verification","storage_state":"not_started_by_this_attempt","physical_authorization":false}), deadline).unwrap();
            }
        });
        let relay_thread = std::thread::spawn(move || {
            relay(
                &mut io,
                &mut relay_service,
                Instant::now() + Duration::from_secs(3),
            )
        });
        let deadline = Instant::now() + Duration::from_secs(3);
        let ready = read_frame(&mut browser, deadline).unwrap();
        let options = serde_json::from_value(ready["options"].clone()).unwrap();
        let credential = SoftPasskey::new(true)
            .do_registration(
                webauthn_rs::prelude::Url::parse(crate::root_passkey::OPERATOR_ORIGIN).unwrap(),
                options,
            )
            .unwrap();
        let response = exchange(
            &mut browser,
            &json!({"schema":1,"command":{"operation":"finish_enrollment","credential":credential}}),
            deadline,
        );
        if failed_reply {
            assert_eq!(response.unwrap()["stage"], "credential_verification");
            assert_eq!(relay_thread.join().unwrap(), Ok(()));
        } else {
            assert!(response.is_err());
            assert_eq!(
                relay_thread.join().unwrap(),
                Err(Error::EnrollmentReplyUnavailable)
            );
        }
        service_thread.join().unwrap();
    }
    for response in [
        json!({"status":"enrollment_failed","stage":"credential_verification","storage_state":"inspection_required"}),
        json!({"status":"enrollment_failed","stage":"arbitrary-secret-detail","storage_state":"not_started_by_this_attempt"}),
        json!({"schema":1,"status":"enrollment_failed","stage":"storage","storage_state":"inspection_required","physical_authorization":false,"credential":"must not forward"}),
    ] {
        assert_eq!(
            validate_enrollment_result(&response),
            Err(Error::InvalidReply)
        );
    }
}
