// SPDX-License-Identifier: Apache-2.0
//! Real local transport fixtures, synthetic public ceremony data; no browser.
use super::*;
use std::os::unix::net::UnixListener;

fn parsed(bytes: &[u8]) -> Result<http::Request, Error> {
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let mut client = TcpStream::connect(listener.local_addr().unwrap()).unwrap();
    client.write_all(bytes).unwrap();
    client.shutdown(std::net::Shutdown::Write).unwrap();
    let (mut server, _) = listener.accept().unwrap();
    http::read_request(&mut server, Instant::now() + Duration::from_secs(1))
}
fn authenticated(origin: &str, host: &str, secret: &str) -> Vec<u8> {
    format!("POST /ceremony HTTP/1.1\r\nHost: {host}\r\nOrigin: {origin}\r\nContent-Type: application/json\r\nSec-Fetch-Site: same-origin\r\nX-OpenKNX-Session: {secret}\r\nContent-Length: 2\r\n\r\n{{}}").into_bytes()
}
#[test]
fn real_loopback_requests_deny_cross_origin_wrong_host_missing_and_wrong_secret() {
    let secret = "a".repeat(64);
    let valid = parsed(&authenticated(ORIGIN, "localhost:15300", &secret)).unwrap();
    assert!(matches!(
        http::classify(&valid, &secret),
        Ok(http::Action::Ready)
    ));
    for bytes in [
        authenticated("http://evil.invalid", "localhost:15300", &secret),
        authenticated("null", "localhost:15300", &secret),
        authenticated(ORIGIN, "127.0.0.1:15300", &secret),
        authenticated(ORIGIN, "localhost:15300", &"b".repeat(64)),
    ] {
        assert!(matches!(
            http::classify(&parsed(&bytes).unwrap(), &secret),
            Err(Error::Denied)
        ));
    }
    for field in ["origin", "x-openknx-session"] {
        let mut missing = parsed(&authenticated(ORIGIN, "localhost:15300", &secret)).unwrap();
        missing.headers.remove(field);
        assert!(matches!(
            http::classify(&missing, &secret),
            Err(Error::Denied)
        ));
    }
    let mut cross = parsed(&authenticated(ORIGIN, "localhost:15300", &secret)).unwrap();
    cross
        .headers
        .insert("sec-fetch-site".into(), "cross-site".into());
    assert!(matches!(
        http::classify(&cross, &secret),
        Err(Error::Denied)
    ));
}
#[test]
fn framing_rejects_duplicate_headers_chunks_oversize_and_absolute_deadline() {
    for bytes in [
        b"POST /finish HTTP/1.1\r\nContent-Length: 0\r\ncontent-length: 0\r\n\r\n".to_vec(),
        b"POST /finish HTTP/1.1\r\nTransfer-Encoding: chunked\r\nContent-Length: 0\r\n\r\n"
            .to_vec(),
        b"POST /finish HTTP/1.1\r\nContent-Length: 32769\r\n\r\n".to_vec(),
        b"GET / HTTP/1.1\r\nContent-Length: 1\r\n\r\nx".to_vec(),
        vec![b'x'; 4097],
    ] {
        assert!(parsed(&bytes).is_err());
    }
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let _client = TcpStream::connect(listener.local_addr().unwrap()).unwrap();
    let (mut server, _) = listener.accept().unwrap();
    assert!(http::read_request(&mut server, Instant::now()).is_err());
}
#[test]
fn public_page_contains_no_session_secret_plan_or_ceremony_options() {
    let request =
        parsed(b"GET / HTTP/1.1\r\nHost: localhost:15300\r\nSec-Fetch-Site: cross-site\r\n\r\n")
            .unwrap();
    // Initial file-to-localhost navigation exposes only static public content.
    assert!(matches!(
        http::classify(&request, "a"),
        Ok(http::Action::Page)
    ));
    let page = include_str!("browser.html");
    assert!(
        !page.contains("plan_summary")
            && !page.contains("challenge")
            && !page.contains(&"a".repeat(64))
    );
    assert!(page.contains("disabled"));
    let script = include_str!("browser.js");
    assert!(script.find("history.replaceState").unwrap() < script.find("await post").unwrap());
    assert!(script.contains("textContent") && !script.contains("innerHTML"));
    let listener = TcpListener::bind("127.0.0.1:0").unwrap();
    let mut client = TcpStream::connect(listener.local_addr().unwrap()).unwrap();
    let (mut server, _) = listener.accept().unwrap();
    http::reply(
        &mut server,
        200,
        "text/html",
        page.as_bytes(),
        Instant::now() + Duration::from_secs(1),
    )
    .unwrap();
    drop(server);
    let mut response = String::new();
    client.read_to_string(&mut response).unwrap();
    assert!(response.contains("\r\nReferrer-Policy: same-origin\r\n"));
    assert!(!response.contains("Referrer-Policy: no-referrer"));
}
#[test]
fn actual_nonroot_unix_server_is_rejected_before_browser_bootstrap() {
    if geteuid().as_raw() == 0 {
        return;
    }
    let directory = tempfile::Builder::new()
        .prefix("t153-browser-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let path = directory.path().join("s");
    let listener = UnixListener::bind(&path).unwrap();
    let client = UnixStream::connect(&path).unwrap();
    let (_server, _) = listener.accept().unwrap();
    assert_eq!(require_root_server(&client), Err(Error::ServiceUnavailable));
    assert_eq!(run(vec![]), Err(Error::Arguments));
    assert_eq!(
        run(vec!["--assert".into(), "--socket".into()]),
        Err(Error::Arguments)
    );
}
#[test]
fn framed_synthetic_service_exchange_is_bounded_and_never_authorizing() {
    let (mut client, mut server) = UnixStream::pair().unwrap();
    let thread = std::thread::spawn(move || {
        let deadline = Instant::now() + Duration::from_secs(1);
        let mut size = [0; 4];
        read_exact(&mut server, &mut size, deadline).unwrap();
        let mut body = vec![0; u32::from_be_bytes(size) as usize];
        read_exact(&mut server, &mut body, deadline).unwrap();
        let request: Value = unique_json(&body).unwrap();
        assert_eq!(request, Mode::Assert.begin());
        let response = serde_json::to_vec(&json!({"schema":1,"status":"assertion_ready","options":{"publicKey":{"challenge":"synthetic"}},"plan_summary":"Synthetic canonical plan","plan_digest":vec![1u8;32],"physical_authorization":false})).unwrap();
        write_all(
            &mut server,
            &(response.len() as u32).to_be_bytes(),
            deadline,
        )
        .unwrap();
        write_all(&mut server, &response, deadline).unwrap();
    });
    // This tests framing only; no production peer check is bypassed by run().
    let ready = exchange(
        &mut client,
        &Mode::Assert.begin(),
        Instant::now() + Duration::from_secs(1),
    )
    .unwrap();
    validate_ready(&ready, Mode::Assert).unwrap();
    assert_eq!(ready["physical_authorization"], false);
    thread.join().unwrap();
    assert!(
        validate_ready(
            &json!({"status":"assertion_ready","options":{"publicKey":{}},"plan_digest":[1]}),
            Mode::Assert
        )
        .is_err()
    );
}

#[test]
fn saturated_actual_unix_backlog_obeys_connect_deadline() {
    let directory = tempfile::Builder::new()
        .prefix("t153-full-")
        .tempdir_in("/private/tmp")
        .unwrap();
    let path = directory.path().join("s");
    let listener =
        socket2::Socket::new(socket2::Domain::UNIX, socket2::Type::STREAM, None).unwrap();
    listener
        .bind(&socket2::SockAddr::unix(&path).unwrap())
        .unwrap();
    listener.listen(1).unwrap();
    let mut queued = Vec::new();
    let mut saturated = false;
    for _ in 0..16 {
        match connect_service(&path, Instant::now() + Duration::from_millis(20)) {
            Ok(client) => queued.push(client),
            Err(Error::Timeout | Error::ServiceUnavailable) => {
                assert!(!queued.is_empty(), "fixture must accept before saturating");
                saturated = true;
                break;
            }
            Err(error) => panic!(
                "unexpected fixture connect error after {} queued clients: {error:?}",
                queued.len()
            ),
        }
    }
    assert!(saturated, "actual small-backlog fixture must saturate");
    let start = Instant::now();
    assert!(matches!(
        connect_service(&path, start + Duration::from_millis(40)),
        Err(Error::Timeout | Error::ServiceUnavailable)
    ));
    assert!(start.elapsed() < Duration::from_millis(500));
    assert!(matches!(
        connect_service(&path, Instant::now()),
        Err(Error::Timeout)
    ));
    drop(queued);
}
