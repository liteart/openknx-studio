// SPDX-License-Identifier: Apache-2.0
//! Real CLI failure path; no service connection, credentials or device access.

#[cfg(target_os = "macos")]
#[test]
fn generic_browser_failure_never_claims_no_effect_or_invites_blind_retry() {
    // Invalid arguments return before any socket, bootstrap or provider access.
    // Every runtime error uses this same final diagnostic, including failures
    // that could follow a guarded finish whose final reply was lost.
    let output = std::process::Command::new(env!("CARGO_BIN_EXE_task153-operator-browser"))
        .arg("--invalid-test-argument")
        .stdin(std::process::Stdio::null())
        .output()
        .unwrap();
    assert_eq!(output.status.code(), Some(1));
    let diagnostic = std::str::from_utf8(&output.stderr).unwrap();
    assert!(diagnostic.contains("operation status may be unknown"));
    assert!(diagnostic.contains("Inspect protected state"));
    assert!(diagnostic.contains("do not retry automatically"));
    assert!(!diagnostic.contains("no device authorization issued"));
    assert!(!diagnostic.contains("no write"));
}
