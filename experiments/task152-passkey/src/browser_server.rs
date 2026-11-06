// SPDX-License-Identifier: Apache-2.0
//! Fixed-origin temporary development server. This is not a production HTTP API.

use std::collections::BTreeMap;
use std::io::{Read, Write};
use std::net::{TcpListener, TcpStream};
use std::path::Path;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};
use task152_passkey_prototype::{BoundPlan, DEVELOPMENT_ORIGIN, DevelopmentVerifier, guard};
use webauthn_rs::prelude::Uuid;

const ADDRESS: &str = "127.0.0.1:15200";
const HOST: &str = "localhost:15200";
const MAX_HEADER: usize = 8192;
const MAX_BODY: usize = 32768;
const REQUEST_TIMEOUT: Duration = Duration::from_secs(5);
const RUN_LIFETIME: Duration = Duration::from_secs(900);

#[derive(Debug)]
pub(crate) enum ServerError {
    Io,
    InvalidRequest,
    Unavailable,
}

struct Request {
    method: String,
    path: String,
    headers: BTreeMap<String, String>,
    body: Vec<u8>,
}

struct Response {
    status: u16,
    content_type: &'static str,
    body: Vec<u8>,
}

struct Demo {
    verifier: DevelopmentVerifier,
    plan: BoundPlan,
    csrf: String,
    stopped: bool,
}

pub(crate) fn run(root: &Path) -> Result<(), ServerError> {
    let mut demo = Demo::new(root, now()?)?;
    let listener = TcpListener::bind(ADDRESS).map_err(|_| ServerError::Io)?;
    listener
        .set_nonblocking(true)
        .map_err(|_| ServerError::Io)?;
    println!("OpenKNX Studio temporary passkey test: {DEVELOPMENT_ORIGIN}");
    println!("Synthetic plan only; no hardware authorization. Expires after 15 minutes.");
    println!("Open the address manually. Enrollment starts only when you choose it in the page.");
    let started = Instant::now();
    while !demo.stopped && started.elapsed() < RUN_LIFETIME {
        match listener.accept() {
            Ok((mut stream, address)) => {
                if !address.ip().is_loopback() {
                    continue;
                }
                // Request errors are intentionally not logged: no assertion,
                // credential identifier, client header or response body is logged.
                let response = match read_request(&mut stream) {
                    Ok(request) => demo.handle(request),
                    Err(_) => failure(400, "invalid_request"),
                };
                let _ = write_response(&mut stream, response);
                // Every connection is closed here, irrespective of a browser's
                // keep-alive preference. No second/pipelined request is executed.
            }
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => {
                std::thread::sleep(Duration::from_millis(25));
            }
            Err(_) => return Err(ServerError::Io),
        }
    }
    println!("Local development test stopped. Its in-memory enrollment was discarded.");
    Ok(())
}

impl Demo {
    fn new(root: &Path, now: u64) -> Result<Self, ServerError> {
        let verifier = DevelopmentVerifier::open(root, "local-browser-development")
            .map_err(|_| ServerError::Unavailable)?;
        let plan = BoundPlan::new(guard::PlanInput {
            operation_id: format!("synthetic-browser-{}", Uuid::new_v4()),
            route: guard::Route { gateway: "synthetic-gateway-no-network".into(),
                bus_path: "synthetic-bus-no-device".into(), target: "synthetic-target-not-a-device".into(),
                identity: [7;32] },
            selector: guard::Selector { object: 0, property: 14, index: 1, count: 1, element_size: 1 },
            original: vec![0], desired: vec![4],
            purpose: "Verify this synthetic plan using a development passkey; execute no device operation".into(),
            recovery: "No device is connected or changed; no physical recovery is needed".into(),
            expires_at_utc_seconds: now.saturating_add(RUN_LIFETIME.as_secs()),
        }).map_err(|_| ServerError::Unavailable)?;
        Ok(Self {
            verifier,
            plan,
            csrf: Uuid::new_v4().simple().to_string(),
            stopped: false,
        })
    }

    fn handle(&mut self, request: Request) -> Response {
        if request.headers.get("host").map(String::as_str) != Some(HOST) {
            return failure(403, "host_rejected");
        }
        if request.method == "GET" {
            return match request.path.as_str() {
                "/" => Response {
                    status: 200,
                    content_type: "text/html; charset=utf-8",
                    body: include_bytes!("browser.html").to_vec(),
                },
                "/app.js" => Response {
                    status: 200,
                    content_type: "text/javascript; charset=utf-8",
                    body: include_bytes!("browser.js").to_vec(),
                },
                "/api/session" if same_origin_fetch(&request) => json(
                    200,
                    &serde_json::json!({
                        "csrf": self.csrf, "plan": self.plan.exact_plan_summary(),
                        "physical_authorization": false,
                    }),
                ),
                "/api/session" => failure(403, "origin_rejected"),
                _ => failure(404, "not_found"),
            };
        }
        if request.method != "POST" {
            return failure(405, "method_rejected");
        }
        if request.headers.get("origin").map(String::as_str) != Some(DEVELOPMENT_ORIGIN)
            || !same_origin_fetch(&request)
            || request.headers.get("content-type").map(String::as_str) != Some("application/json")
            || request.headers.get("x-openknx-development-csrf") != Some(&self.csrf)
        {
            return failure(403, "request_rejected");
        }
        let Ok(at) = now() else {
            return failure(503, "clock_unavailable");
        };
        match request.path.as_str() {
            "/api/enroll/start" if request.body == b"{}" => {
                match self.verifier.begin_development_enrollment(at) {
                    Ok(options) => json(200, &options),
                    Err(_) => failure(409, "enrollment_unavailable"),
                }
            }
            "/api/enroll/finish" => match self
                .verifier
                .finish_development_enrollment(&request.body, at)
            {
                Ok(()) => json(
                    200,
                    &serde_json::json!({ "enrolled": true, "physical_authorization": false }),
                ),
                Err(_) => failure(400, "enrollment_rejected"),
            },
            "/api/assert/start" if request.body == b"{}" => {
                match self.verifier.begin_assertion(&self.plan, at) {
                    Ok(options) => json(200, &options),
                    Err(_) => failure(409, "assertion_unavailable"),
                }
            }
            "/api/assert/finish" => {
                match self
                    .verifier
                    .finish_assertion(&self.plan, &request.body, at)
                {
                    // Return a minimal status, never a capability, credential ID or
                    // assertion. The full dev receipt remains in the local journal.
                    Ok(receipt) => json(
                        200,
                        &serde_json::json!({ "verified": true,
                    "physical_authorization": receipt.physical_authorization() }),
                    ),
                    Err(_) => failure(400, "assertion_rejected"),
                }
            }
            "/api/cancel" if request.body == b"{}" => {
                self.verifier.cancel();
                json(200, &serde_json::json!({ "cancelled": true }))
            }
            "/api/stop" if request.body == b"{}" => {
                self.stopped = true;
                json(200, &serde_json::json!({ "stopped": true }))
            }
            _ => failure(404, "not_found"),
        }
    }
}

fn same_origin_fetch(request: &Request) -> bool {
    request.headers.get("sec-fetch-site").map(String::as_str) == Some("same-origin")
        && request.headers.get("sec-fetch-mode").map(String::as_str) == Some("same-origin")
}

fn now() -> Result<u64, ServerError> {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|time| time.as_secs())
        .map_err(|_| ServerError::Unavailable)
}

fn json<T: serde::Serialize>(status: u16, value: &T) -> Response {
    match serde_json::to_vec(value) {
        Ok(body) if body.len() <= MAX_BODY => Response {
            status,
            content_type: "application/json",
            body,
        },
        _ => failure(500, "serialization_failed"),
    }
}

fn failure(status: u16, code: &'static str) -> Response {
    // Codes are internal, locale-independent diagnostics for this development UI.
    Response {
        status,
        content_type: "application/json",
        body: format!("{{\"error\":\"{code}\"}}").into_bytes(),
    }
}

fn read_request(stream: &mut TcpStream) -> Result<Request, ServerError> {
    let started = Instant::now();
    let mut bytes = Vec::new();
    let mut chunk = [0_u8; 1024];
    let (end, length) = loop {
        if let Some(end) = bytes.windows(4).position(|part| part == b"\r\n\r\n") {
            let (_, _, headers) = parse_head(&bytes[..end])?;
            let length = content_length(&headers)?;
            break (end + 4, length);
        }
        if bytes.len() >= MAX_HEADER {
            return Err(ServerError::InvalidRequest);
        }
        read_chunk(stream, &mut chunk, &mut bytes, started)?;
    };
    if end > MAX_HEADER {
        return Err(ServerError::InvalidRequest);
    }
    while bytes.len() < end + length {
        read_chunk(stream, &mut chunk, &mut bytes, started)?;
    }
    // Buffered extra bytes would be a second request or invalid framing.
    if bytes.len() != end + length {
        return Err(ServerError::InvalidRequest);
    }
    parse_request(&bytes)
}

fn read_chunk(
    stream: &mut TcpStream,
    chunk: &mut [u8],
    bytes: &mut Vec<u8>,
    started: Instant,
) -> Result<(), ServerError> {
    let remaining = REQUEST_TIMEOUT
        .checked_sub(started.elapsed())
        .ok_or(ServerError::InvalidRequest)?;
    stream
        .set_read_timeout(Some(remaining))
        .map_err(|_| ServerError::Io)?;
    let count = stream
        .read(chunk)
        .map_err(|_| ServerError::InvalidRequest)?;
    if count == 0 || bytes.len().saturating_add(count) > MAX_HEADER + MAX_BODY {
        return Err(ServerError::InvalidRequest);
    }
    bytes.extend_from_slice(&chunk[..count]);
    Ok(())
}

type Head = (String, String, BTreeMap<String, String>);

fn parse_head(bytes: &[u8]) -> Result<Head, ServerError> {
    if bytes.len() > MAX_HEADER || !bytes.is_ascii() {
        return Err(ServerError::InvalidRequest);
    }
    let text = std::str::from_utf8(bytes).map_err(|_| ServerError::InvalidRequest)?;
    let mut lines = text.split("\r\n");
    let parts: Vec<_> = lines
        .next()
        .ok_or(ServerError::InvalidRequest)?
        .split(' ')
        .collect();
    if parts.len() != 3
        || !["GET", "POST"].contains(&parts[0])
        || !parts[1].starts_with('/')
        || parts[1].len() > 256
        || !parts[1].bytes().all(|b| b.is_ascii_graphic())
        || parts[2] != "HTTP/1.1"
    {
        return Err(ServerError::InvalidRequest);
    }
    let mut headers = BTreeMap::new();
    for line in lines {
        let (name, value) = line.split_once(':').ok_or(ServerError::InvalidRequest)?;
        if name.is_empty()
            || !name.bytes().all(|b| b.is_ascii_alphanumeric() || b == b'-')
            || !value.bytes().all(|b| b.is_ascii_graphic() || b == b' ')
        {
            return Err(ServerError::InvalidRequest);
        }
        if headers
            .insert(name.to_ascii_lowercase(), value.trim().into())
            .is_some()
        {
            return Err(ServerError::InvalidRequest);
        }
    }
    if headers.contains_key("transfer-encoding")
        || headers.contains_key("expect")
        || headers.contains_key("upgrade")
    {
        return Err(ServerError::InvalidRequest);
    }
    Ok((parts[0].into(), parts[1].into(), headers))
}

fn content_length(headers: &BTreeMap<String, String>) -> Result<usize, ServerError> {
    let Some(value) = headers.get("content-length") else {
        return Ok(0);
    };
    if value.is_empty() || value.len() > 10 || !value.bytes().all(|b| b.is_ascii_digit()) {
        return Err(ServerError::InvalidRequest);
    }
    let length = value
        .parse::<usize>()
        .map_err(|_| ServerError::InvalidRequest)?;
    if length > MAX_BODY {
        return Err(ServerError::InvalidRequest);
    }
    Ok(length)
}

fn parse_request(bytes: &[u8]) -> Result<Request, ServerError> {
    let end = bytes
        .windows(4)
        .position(|part| part == b"\r\n\r\n")
        .ok_or(ServerError::InvalidRequest)?;
    let (method, path, headers) = parse_head(&bytes[..end])?;
    let body = bytes.get(end + 4..).ok_or(ServerError::InvalidRequest)?;
    if body.len() != content_length(&headers)? || (method == "GET" && !body.is_empty()) {
        return Err(ServerError::InvalidRequest);
    }
    Ok(Request {
        method,
        path,
        headers,
        body: body.to_vec(),
    })
}

fn write_response(stream: &mut TcpStream, response: Response) -> Result<(), ServerError> {
    let head = format!(
        "HTTP/1.1 {} Response\r\nContent-Type: {}\r\nContent-Length: {}\r\nConnection: close\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\nX-Frame-Options: DENY\r\nReferrer-Policy: no-referrer\r\nContent-Security-Policy: default-src 'none'; script-src 'self'; connect-src 'self'; style-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'\r\n\r\n",
        response.status,
        response.content_type,
        response.body.len()
    );
    let started = Instant::now();
    for mut bytes in [head.as_bytes(), response.body.as_slice()] {
        while !bytes.is_empty() {
            let remaining = REQUEST_TIMEOUT
                .checked_sub(started.elapsed())
                .ok_or(ServerError::Io)?;
            stream
                .set_write_timeout(Some(remaining))
                .map_err(|_| ServerError::Io)?;
            let count = stream.write(bytes).map_err(|_| ServerError::Io)?;
            if count == 0 {
                return Err(ServerError::Io);
            }
            bytes = &bytes[count..];
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests;
