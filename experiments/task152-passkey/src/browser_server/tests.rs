// SPDX-License-Identifier: Apache-2.0
//! Local HTTP and synthetic passkey tests; no platform or user keys are used.

use super::*;
use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};
use webauthn_rs::prelude::*;

fn fixture() -> (tempfile::TempDir, Demo) {
    let directory = tempfile::tempdir().unwrap();
    let demo = Demo::new(
        &std::fs::canonicalize(directory.path()).unwrap(),
        now().unwrap(),
    )
    .unwrap();
    (directory, demo)
}

fn request(demo: &Demo, method: &str, path: &str, body: &[u8]) -> Request {
    let head = format!(
        "{method} {path} HTTP/1.1\r\nHost: {HOST}\r\nOrigin: {DEVELOPMENT_ORIGIN}\r\nSec-Fetch-Site: same-origin\r\nSec-Fetch-Mode: same-origin\r\nContent-Type: application/json\r\nX-OpenKNX-Development-CSRF: {}\r\nContent-Length: {}\r\n\r\n",
        demo.csrf,
        body.len()
    );
    let mut bytes = head.into_bytes();
    bytes.extend_from_slice(body);
    parse_request(&bytes).unwrap()
}

#[test]
fn cross_origin_missing_csrf_wrong_host_and_request_type_cannot_start_enrollment() {
    for (field, value) in [
        ("origin", "https://untrusted.example"),
        ("origin", "null"),
        ("x-openknx-development-csrf", "wrong-token"),
        ("host", "127.0.0.1:15200"),
        ("host", "rebound.example:15200"),
        ("sec-fetch-site", "cross-site"),
        ("sec-fetch-mode", "cors"),
        ("content-type", "text/plain"),
    ] {
        let (_directory, mut demo) = fixture();
        let mut bad = request(&demo, "POST", "/api/enroll/start", b"{}");
        bad.headers.insert(field.into(), value.into());
        assert_eq!(demo.handle(bad).status, 403);
        let good = request(&demo, "POST", "/api/enroll/start", b"{}");
        assert_eq!(demo.handle(good).status, 200);
    }
    let (_directory, mut demo) = fixture();
    for field in [
        "origin",
        "x-openknx-development-csrf",
        "sec-fetch-site",
        "sec-fetch-mode",
        "content-type",
    ] {
        let mut bad = request(&demo, "POST", "/api/enroll/start", b"{}");
        bad.headers.remove(field);
        assert_eq!(demo.handle(bad).status, 403);
    }
}

#[test]
fn session_token_is_not_returned_to_cross_site_fetches() {
    let (_directory, mut demo) = fixture();
    let mut bad = request(&demo, "GET", "/api/session", b"");
    bad.headers
        .insert("sec-fetch-site".into(), "cross-site".into());
    let response = demo.handle(bad);
    assert_eq!(response.status, 403);
    assert!(
        !String::from_utf8(response.body)
            .unwrap()
            .contains(&demo.csrf)
    );
    let good = request(&demo, "GET", "/api/session", b"");
    let response = demo.handle(good);
    assert_eq!(response.status, 200);
    let session: serde_json::Value = serde_json::from_slice(&response.body).unwrap();
    assert_eq!(session["plan"], demo.plan.exact_plan_summary());
    assert_eq!(session["physical_authorization"], false);
    assert!(
        session["plan"]
            .as_str()
            .unwrap()
            .contains("synthetic-target-not-a-device")
    );
}

#[test]
fn malformed_lengths_duplicate_headers_chunking_and_pipelining_are_rejected() {
    for raw in [
        "POST /api/stop HTTP/1.1\r\nHost: localhost:15200\r\nContent-Length: 0\r\ncontent-length: 0\r\n\r\n",
        "POST /api/stop HTTP/1.1\r\nHost: localhost:15200\r\nHost: bad\r\n\r\n",
        "POST /api/stop HTTP/1.1\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n\r\n",
        "POST /api/stop HTTP/1.1\r\nExpect: 100-continue\r\n\r\n",
        "GET / HTTP/1.1\r\nUpgrade: websocket\r\n\r\n",
        "POST /api/stop HTTP/1.1\r\nContent-Length: +2\r\n\r\n{}",
        "POST /api/stop HTTP/1.1\r\nContent-Length: 32769\r\n\r\n",
        "POST /api/stop HTTP/1.1\r\nContent-Length: 2\r\n\r\n{",
        "POST /api/stop HTTP/1.1\r\nContent-Length: 1\r\n\r\n{}",
        "GET / HTTP/1.1\r\nHost: localhost:15200\r\n\r\nGET / HTTP/1.1\r\n\r\n",
        "GET / HTTP/1.0\r\nHost: localhost:15200\r\n\r\n",
        "GET / HTTP/1.1\r\n Folded: invalid\r\n\r\n",
        "GET /\ninvalid HTTP/1.1\r\n\r\n",
        "GET / HTTP/1.1\r\nContent-Length: 1\r\n\r\nx",
    ] {
        assert!(
            parse_request(raw.as_bytes()).is_err(),
            "unexpectedly accepted test framing"
        );
    }
    let oversized = format!(
        "GET / HTTP/1.1\r\nX-Large: {}\r\n\r\n",
        "a".repeat(MAX_HEADER)
    );
    assert!(parse_request(oversized.as_bytes()).is_err());
}

#[test]
fn browser_handlers_verify_a_real_synthetic_signature_without_device_grant() {
    let (directory, mut demo) = fixture();
    let mut authenticator = SoftPasskey::new(true);
    let start = request(&demo, "POST", "/api/enroll/start", b"{}");
    let response = demo.handle(start);
    assert_eq!(response.status, 200);
    let options: CreationChallengeResponse = serde_json::from_slice(&response.body).unwrap();
    let registration = authenticator
        .do_registration(Url::parse(DEVELOPMENT_ORIGIN).unwrap(), options)
        .unwrap();
    let finish = request(
        &demo,
        "POST",
        "/api/enroll/finish",
        &serde_json::to_vec(&registration).unwrap(),
    );
    assert_eq!(demo.handle(finish).status, 200);
    let start = request(&demo, "POST", "/api/assert/start", b"{}");
    let response = demo.handle(start);
    assert_eq!(response.status, 200);
    let options: RequestChallengeResponse = serde_json::from_slice(&response.body).unwrap();
    let assertion = authenticator
        .do_authentication(Url::parse(DEVELOPMENT_ORIGIN).unwrap(), options)
        .unwrap();
    let body = serde_json::to_vec(&assertion).unwrap();
    let finish = request(&demo, "POST", "/api/assert/finish", &body);
    let response = demo.handle(finish);
    assert_eq!(response.status, 200);
    assert_eq!(
        serde_json::from_slice::<serde_json::Value>(&response.body).unwrap(),
        serde_json::json!({ "verified":true,"physical_authorization":false })
    );
    assert_eq!(std::fs::read_dir(directory.path()).unwrap().count(), 1);
    let replay = request(&demo, "POST", "/api/assert/finish", &body);
    assert_eq!(demo.handle(replay).status, 400);
}

#[test]
fn no_assertion_without_enrollment_and_stop_requires_same_origin_token() {
    let (_directory, mut demo) = fixture();
    let request = request(&demo, "POST", "/api/assert/start", b"{}");
    assert_eq!(demo.handle(request).status, 409);
    let mut stop = super::tests::request(&demo, "POST", "/api/stop", b"{}");
    stop.headers.remove("x-openknx-development-csrf");
    assert_eq!(demo.handle(stop).status, 403);
    assert!(!demo.stopped);
    let stop = super::tests::request(&demo, "POST", "/api/stop", b"{}");
    assert_eq!(demo.handle(stop).status, 200);
    assert!(demo.stopped);
}

#[test]
fn loopback_connection_is_read_bounded_and_response_closes_connection() {
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let address = listener.local_addr().unwrap();
    let worker = std::thread::spawn(move || {
        let (mut stream, _) = listener.accept().unwrap();
        let request = read_request(&mut stream).unwrap();
        assert_eq!(request.path, "/");
        write_response(
            &mut stream,
            Response {
                status: 200,
                content_type: "text/plain",
                body: b"synthetic response".to_vec(),
            },
        )
        .unwrap();
    });
    let mut client = TcpStream::connect(address).unwrap();
    client.set_read_timeout(Some(REQUEST_TIMEOUT)).unwrap();
    client
        .write_all(b"GET / HTTP/1.1\r\nHost: localhost:15200\r\nConnection: keep-alive\r\n\r\n")
        .unwrap();
    let mut bytes = Vec::new();
    client.read_to_end(&mut bytes).unwrap();
    let response = String::from_utf8(bytes).unwrap();
    assert!(response.contains("Connection: close\r\n"));
    assert!(response.contains("Content-Security-Policy: default-src 'none'"));
    assert!(response.ends_with("synthetic response"));
    worker.join().unwrap();
}
