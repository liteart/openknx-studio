// SPDX-License-Identifier: Apache-2.0
//! Actual local sockets and synthetic registration keys; no root store access.

use super::*;
use std::os::unix::net::UnixListener;
use task152_passkey_prototype::guard;
use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};

fn connected() -> (tempfile::TempDir, UnixStream, UnixStream) {
    let directory = tempfile::Builder::new()
        .prefix("task153-operator-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let path = directory.path().join("socket");
    let listener = UnixListener::bind(&path).unwrap();
    let client = UnixStream::connect(path).unwrap();
    let (server, _) = listener.accept().unwrap();
    (directory, client, server)
}

fn plan() -> ConfiguredPlan {
    ConfiguredPlan::new(guard::PlanInput {
        operation_id: "synthetic-service-plan".into(),
        route: guard::Route {
            gateway: "synthetic-gateway".into(),
            bus_path: "synthetic-bus".into(),
            target: "synthetic-target".into(),
            identity: [7; 32],
        },
        selector: guard::Selector {
            object: 0,
            property: 14,
            index: 1,
            count: 1,
            element_size: 1,
        },
        original: vec![0],
        desired: vec![4],
        purpose: "Synthetic operator IPC".into(),
        recovery: "Synthetic restoration".into(),
        expires_at_utc_seconds: u64::MAX,
    })
    .unwrap()
}

fn deadline() -> Instant {
    Instant::now() + Duration::from_secs(2)
}

fn send(stream: &mut UnixStream, bytes: &[u8]) {
    stream
        .write_all(&(bytes.len() as u32).to_be_bytes())
        .unwrap();
    stream.write_all(bytes).unwrap();
}

#[test]
fn actual_nonroot_process_and_peer_are_denied_without_reading_a_ceremony() {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        let (_directory, mut client, server) = connected();
        let policy = ExpectedOperator::new(0, 0, geteuid().as_raw()).unwrap();
        assert_eq!(
            require_operator_peer(&server, &policy),
            Err(Error::OperatorPeerDenied)
        );
        assert_eq!(
            handle_operator_connection(server, &plan(), &policy),
            Err(Error::RootServiceRequired)
        );
        let mut reply = Vec::new();
        client.read_to_end(&mut reply).unwrap();
        assert!(reply.is_empty());
    }
}

#[test]
fn framing_rejects_oversize_empty_truncation_and_expired_reads() {
    for size in [0, MAX_FRAME as u32 + 1] {
        let (_directory, mut client, mut server) = connected();
        client.write_all(&size.to_be_bytes()).unwrap();
        assert!(matches!(
            read_request(&mut server, deadline()),
            Err(Error::InvalidFrame)
        ));
    }
    let (_directory, mut client, mut server) = connected();
    client.write_all(&10u32.to_be_bytes()).unwrap();
    client.write_all(b"{}").unwrap();
    client.shutdown(std::net::Shutdown::Write).unwrap();
    assert!(matches!(
        read_request(&mut server, deadline()),
        Err(Error::InvalidFrame)
    ));
    assert!(matches!(
        read_request(&mut server, Instant::now()),
        Err(Error::Timeout)
    ));
}

#[test]
fn wire_has_no_caller_identity_target_or_arbitrary_command_fields() {
    for payload in [
        b"{\"schema\":1,\"command\":{\"operation\":\"begin_assertion\",\"uid\":0}}".as_slice(),
        b"{\"schema\":1,\"command\":{\"operation\":\"begin_assertion\",\"target\":\"other\"}}"
            .as_slice(),
        b"{\"schema\":1,\"schema\":1,\"command\":{\"operation\":\"begin_assertion\"}}".as_slice(),
        b"{\"schema\":2,\"command\":{\"operation\":\"begin_assertion\"}}".as_slice(),
        b"{\"schema\":1,\"command\":{\"operation\":\"write\"}}".as_slice(),
    ] {
        let (_directory, mut client, mut server) = connected();
        send(&mut client, payload);
        assert!(matches!(
            read_request(&mut server, deadline()),
            Err(Error::InvalidRequest)
        ));
    }
    let (_directory, mut client, mut server) = connected();
    send(
        &mut client,
        b"{\"schema\":1,\"command\":{\"operation\":\"begin_assertion\"}}",
    );
    assert!(matches!(
        read_request(&mut server, deadline()).unwrap().command,
        Command::BeginAssertion {}
    ));
}

#[test]
fn registration_challenge_requires_uv_and_validates_genuine_synthetic_response() {
    let (registration, options) = Registration::begin().unwrap();
    assert_eq!(
        serde_json::to_value(
            options
                .public_key
                .authenticator_selection
                .as_ref()
                .unwrap()
                .user_verification
        )
        .unwrap(),
        "required"
    );
    let mut authenticator = SoftPasskey::new(true);
    let response = authenticator
        .do_registration(
            Url::parse(crate::root_passkey::OPERATOR_ORIGIN).unwrap(),
            options,
        )
        .unwrap();
    let credential = registration.finish(response).unwrap();
    assert!(!credential.cred_id().is_empty());
}

#[test]
fn registration_rejects_wrong_origin_challenge_and_expired_session() {
    for case in 0..3 {
        let (mut registration, options) = Registration::begin().unwrap();
        let mut authenticator = SoftPasskey::new(true);
        let origin = if case == 0 {
            Url::parse("http://localhost:15301").unwrap()
        } else {
            Url::parse(crate::root_passkey::OPERATOR_ORIGIN).unwrap()
        };
        let options = if case == 1 {
            Registration::begin().unwrap().1
        } else {
            options
        };
        let response = authenticator.do_registration(origin, options).unwrap();
        if case == 2 {
            registration.issued = Instant::now() - SESSION_TIMEOUT;
        }
        assert!(matches!(
            registration.finish(response),
            Err(Error::VerificationFailed | Error::Timeout)
        ));
    }
}

#[test]
fn replies_are_bounded_frames_without_implicit_authorization() {
    let (_directory, mut client, mut server) = connected();
    let response =
        serde_json::json!({"schema":1,"status":"synthetic","physical_authorization":false});
    write_reply(&mut server, &response, deadline()).unwrap();
    let mut size = [0; 4];
    client.read_exact(&mut size).unwrap();
    let mut bytes = vec![0; u32::from_be_bytes(size) as usize];
    client.read_exact(&mut bytes).unwrap();
    assert_eq!(
        serde_json::from_slice::<serde_json::Value>(&bytes).unwrap(),
        response
    );
    assert_eq!(
        write_reply(
            &mut server,
            &serde_json::json!({"large":"x".repeat(MAX_REPLY + 1)}),
            deadline()
        ),
        Err(Error::InvalidFrame)
    );
}

#[test]
fn configured_policy_matches_actual_peer_and_rejects_agent_on_same_connection() {
    let (_directory, _client, server) = connected();
    let uid = geteuid().as_raw();
    let gid = getegid().as_raw();
    // A deliberately different policy ID is not evidence of another account.
    let synthetic_other = uid.checked_add(1).unwrap();
    let accepted = ExpectedOperator::new(uid, gid, synthetic_other).unwrap();
    assert_eq!(require_operator_peer(&server, &accepted), Ok(()));
    let denied = ExpectedOperator::new(synthetic_other, gid, uid).unwrap();
    assert_eq!(
        require_operator_peer(&server, &denied),
        Err(Error::OperatorPeerDenied)
    );
}
