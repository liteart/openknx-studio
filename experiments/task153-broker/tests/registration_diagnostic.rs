// SPDX-License-Identifier: Apache-2.0
//! Real ordinary-user subprocess pipes and synthetic keys; no browser or store.
#![cfg(target_os = "macos")]

use nix::fcntl::{FcntlArg, OFlag, fcntl};
use serde_json::{Value, json};
use std::io::{Read, Write};
use std::os::fd::AsFd;
use std::process::{Child, Command, Stdio};
use std::time::{Duration, Instant};
use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};
use webauthn_rs::prelude::Url;

struct KillOnDrop(Child);
impl Drop for KillOnDrop {
    fn drop(&mut self) {
        let _ = self.0.kill();
        let _ = self.0.wait();
        if std::thread::panicking() {
            let mut trace = String::new();
            if let Some(mut stderr) = self.0.stderr.take() {
                let _ = stderr.read_to_string(&mut trace);
                eprintln!("{trace}");
            }
        }
    }
}
fn read_exact(reader: &mut impl Read, mut bytes: &mut [u8], deadline: Instant) {
    while !bytes.is_empty() {
        assert!(Instant::now() < deadline, "synthetic pipe deadline expired");
        match reader.read(bytes) {
            Ok(0) => panic!("synthetic child closed its protocol pipe"),
            Ok(size) => bytes = &mut bytes[size..],
            Err(error) if error.kind() == std::io::ErrorKind::WouldBlock => {
                std::thread::sleep(Duration::from_millis(1))
            }
            Err(error) => panic!("synthetic pipe read failed: {:?}", error.kind()),
        }
    }
}
fn read_frame(reader: &mut impl Read, deadline: Instant) -> Value {
    let mut prefix = [0; 4];
    read_exact(reader, &mut prefix, deadline);
    let size = u32::from_be_bytes(prefix) as usize;
    assert!(size > 0 && size <= 65_536);
    let mut bytes = vec![0; size];
    read_exact(reader, &mut bytes, deadline);
    serde_json::from_slice(&bytes).unwrap()
}

#[test]
fn actual_child_nonblocking_pipes_and_double_typed_roundtrip_preserve_registration() {
    for corrupt in [false, true] {
        let mut child = KillOnDrop(
            Command::new(env!("CARGO_BIN_EXE_task153-registration-diagnostic"))
                .arg("--headless-test")
                .stdin(Stdio::piped())
                .stdout(Stdio::piped())
                .stderr(Stdio::piped())
                .spawn()
                .unwrap(),
        );
        let mut input = child.0.stdin.take().unwrap();
        let mut output = child.0.stdout.take().unwrap();
        let flags = fcntl(output.as_fd(), FcntlArg::F_GETFL).unwrap();
        fcntl(
            output.as_fd(),
            FcntlArg::F_SETFL(OFlag::from_bits_retain(flags) | OFlag::O_NONBLOCK),
        )
        .unwrap();
        let deadline = Instant::now() + Duration::from_secs(10);
        let ready = read_frame(&mut output, deadline);
        assert_eq!(ready["diagnostic"], true);
        let options = serde_json::from_value(ready["options"].clone()).unwrap();
        let mut authenticator = SoftPasskey::new(true);
        let credential = authenticator
            .do_registration(Url::parse("http://localhost:15300").unwrap(), options)
            .unwrap();
        let value = serde_json::to_value(credential).unwrap();
        // Browser serialization intentionally omits transport metadata, just as
        // the shared Safari browser.js does. Only synthetic data is used here.
        let mut credential = json!({"id":value["id"],"rawId":value["rawId"],"type":value["type"],
            "extensions":value["extensions"],"response":{"attestationObject":value["response"]["attestationObject"],"clientDataJSON":value["response"]["clientDataJSON"]}});
        if corrupt {
            credential["response"]["clientDataJSON"] = json!("e30");
        }
        let request = serde_json::to_vec(&json!({"schema":1,"command":{"operation":"finish_enrollment","credential":credential}})).unwrap();
        assert!(request.len() < 4096);
        input
            .write_all(&(request.len() as u32).to_be_bytes())
            .unwrap();
        input.write_all(&request).unwrap();
        let result = read_frame(&mut output, deadline);
        assert_eq!(
            result["stage"],
            if corrupt {
                "credential_verification"
            } else {
                "verified"
            }
        );
        assert_eq!(result["credential_persisted"], false);
        assert_eq!(result["physical_authorization"], false);
        assert_eq!(result.as_object().unwrap().len(), 5);
        loop {
            assert!(Instant::now() < deadline, "synthetic child did not exit");
            if let Some(status) = child.0.try_wait().unwrap() {
                assert!(status.success());
                break;
            }
            std::thread::sleep(Duration::from_millis(1));
        }
        let mut trace = String::new();
        child
            .0
            .stderr
            .take()
            .unwrap()
            .read_to_string(&mut trace)
            .unwrap();
        for stage in [
            "relay_waiting_finish",
            "relay_finish_frame_received",
            "relay_finish_decoded",
            "service_finish_decoded",
            "relay_reply_written",
        ] {
            assert!(trace.contains(stage), "missing static pipe stage");
        }
        assert!(!trace.contains("attestationObject"));
        assert!(!trace.contains("clientDataJSON"));
    }
}
