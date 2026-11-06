// SPDX-License-Identifier: Apache-2.0
//! Deliberately narrow HTTP/1.1, one request per connection, no CORS or proxy.
use super::{Error, MAX_COMMAND, ORIGIN, TcpStream, read_exact, write_all};
use std::collections::BTreeMap;
use std::time::Instant;

pub(super) struct Request {
    pub method: String,
    pub path: String,
    pub headers: BTreeMap<String, String>,
    pub body: Vec<u8>,
}
pub(super) enum Action {
    Page,
    Script,
    Ready,
    Finish,
}

pub(super) fn read_request(stream: &mut TcpStream, deadline: Instant) -> Result<Request, Error> {
    let mut header = Vec::new();
    while !header.ends_with(b"\r\n\r\n") {
        if header.len() >= 4096 {
            return Err(Error::InvalidHttp);
        }
        let mut byte = [0];
        read_exact(stream, &mut byte, deadline)?;
        header.push(byte[0]);
    }
    let text = std::str::from_utf8(&header).map_err(|_| Error::InvalidHttp)?;
    let mut lines = text.split("\r\n");
    let parts: Vec<_> = lines.next().ok_or(Error::InvalidHttp)?.split(' ').collect();
    let [method, path, "HTTP/1.1"] = parts.as_slice() else {
        return Err(Error::InvalidHttp);
    };
    let mut headers = BTreeMap::new();
    for line in lines.take_while(|line| !line.is_empty()) {
        let (name, value) = line.split_once(':').ok_or(Error::InvalidHttp)?;
        if name.is_empty()
            || !name
                .bytes()
                .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
            || value
                .bytes()
                .any(|byte| byte.is_ascii_control() && byte != b'\t')
        {
            return Err(Error::InvalidHttp);
        }
        if headers
            .insert(name.to_ascii_lowercase(), value.trim().to_owned())
            .is_some()
        {
            return Err(Error::InvalidHttp);
        }
    }
    if headers.contains_key("transfer-encoding") || headers.contains_key("expect") {
        return Err(Error::InvalidHttp);
    }
    let size = match headers.get("content-length") {
        Some(value) if !value.is_empty() && value.bytes().all(|byte| byte.is_ascii_digit()) => {
            value.parse::<usize>().map_err(|_| Error::InvalidHttp)?
        }
        Some(_) => return Err(Error::InvalidHttp),
        None if *method == "GET" => 0,
        None => return Err(Error::InvalidHttp),
    };
    if size > MAX_COMMAND || (*method == "GET" && size != 0) {
        return Err(Error::InvalidHttp);
    }
    let mut body = vec![0; size];
    read_exact(stream, &mut body, deadline)?;
    Ok(Request {
        method: (*method).into(),
        path: (*path).into(),
        headers,
        body,
    })
}

pub(super) fn classify(request: &Request, secret: &str) -> Result<Action, Error> {
    let header = |key| request.headers.get(key).map(String::as_str);
    if header("host") != Some("localhost:15300") {
        return Err(Error::Denied);
    }
    if let Some(origin) = header("origin")
        && origin != ORIGIN
    {
        return Err(Error::Denied);
    }
    if request.method == "GET" {
        return match request.path.as_str() {
            "/" => Ok(Action::Page),
            "/browser.js" => Ok(Action::Script),
            _ => Err(Error::Denied),
        };
    }
    if let Some(site) = header("sec-fetch-site")
        && !matches!(site, "same-origin" | "none")
    {
        return Err(Error::Denied);
    }
    if request.method != "POST"
        || header("origin") != Some(ORIGIN)
        || header("content-type") != Some("application/json")
        || header("x-openknx-session") != Some(secret)
    {
        return Err(Error::Denied);
    }
    match request.path.as_str() {
        "/ceremony" if request.body == b"{}" => Ok(Action::Ready),
        "/finish" => Ok(Action::Finish),
        _ => Err(Error::Denied),
    }
}

pub(super) fn reply(
    stream: &mut TcpStream,
    status: u16,
    content_type: &str,
    body: &[u8],
    deadline: Instant,
) -> Result<(), Error> {
    if body.len() > super::MAX_FRAME {
        return Err(Error::InvalidReply);
    }
    let header = format!(
        "HTTP/1.1 {status} Response\r\nContent-Type: {content_type}; charset=utf-8\r\nContent-Length: {}\r\nConnection: close\r\nCache-Control: no-store\r\nReferrer-Policy: same-origin\r\nX-Content-Type-Options: nosniff\r\nCross-Origin-Resource-Policy: same-origin\r\nContent-Security-Policy: default-src 'none'; script-src 'self'; connect-src 'self'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'\r\n\r\n",
        body.len()
    );
    write_all(stream, header.as_bytes(), deadline)?;
    write_all(stream, body, deadline)
}
