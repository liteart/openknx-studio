//! Synthetic offline fixtures only; no Windows or vendor execution.
#![cfg(unix)]

use sha2::{Digest, Sha256};
use std::path::{Path, PathBuf};
use std::process::Command;

fn digest(bytes: &[u8]) -> String {
    const HEX: &[u8; 16] = b"0123456789abcdef";
    Sha256::digest(bytes)
        .iter()
        .flat_map(|byte| {
            [
                HEX[(byte >> 4) as usize] as char,
                HEX[(byte & 15) as usize] as char,
            ]
        })
        .collect()
}

fn import(directory: &Path, label: &str, value: &str) -> (String, PathBuf) {
    let identity = serde_json::json!({"device_uuid":"ba0332b4-cc78-4c94-80b2-829e3c527fd1","application_id":"owned-application","product_sha256":"a".repeat(64),"dca_sha256":"b".repeat(64),"dca_version":"owned-1","parameter_unique_id":"owned-parameter","parameter_internal_name":"owned-description"});
    let candidate=serde_json::to_vec(&serde_json::json!({"format_version":1,"kind":"ordinary-sdk-string","host_process_id":7,"project_id":"owned-project","installation_id":"owned-installation","device_id":"owned-device","application_id":identity["application_id"],"product_sha256":identity["product_sha256"],"dca_sha256":identity["dca_sha256"],"dca_version":identity["dca_version"],"sdk_sha256":"d".repeat(64),"object_model_sha256":"e".repeat(64),"host_sha256":"f".repeat(64),"parameter_unique_id":identity["parameter_unique_id"],"parameter_internal_name":identity["parameter_internal_name"],"value":value,"host_final_validation":"PASS_BEFORE_EXIT","requires_confirmed_exit_zero":true,"accepted_configuration":false})).unwrap();
    let launcher=b"LAUNCH_START pid=7 mode=original-sdk-product-store-fixture deadline-seconds=660 capture-bytes=65536 descendants=NOT_CONTAINED\nLAUNCH_RESULT reason=EXIT child-exit=0 child-exit-confirmed=true captured-bytes=0 descendants=NOT_CONTAINED\n";
    let completion=serde_json::to_vec(&serde_json::json!({"status":{"state":"Ready","result":0,"done":true,"owned":0},"source":"OWNED_TEST","qualification":"NOT_ESTABLISHED","hardware":"NOT_RUN"})).unwrap();
    let expected=serde_json::to_vec(&serde_json::json!({"identity":identity,"originating_revision":"c".repeat(40),"source":{"kind":"owned-offline-fixture","uuid_revision_synthetic":true,"project_id":"owned-project","installation_id":"owned-installation","device_id":"owned-device","sdk_sha256":"d".repeat(64),"object_model_sha256":"e".repeat(64)},"host_sha256":"f".repeat(64),"host_process_id":7,"candidate_sha256":digest(&candidate),"launcher_stdout_sha256":digest(launcher),"completion_sha256":digest(&completion),"expected_value_sha256":digest(value.as_bytes())})).unwrap();
    let candidate_path = directory.join(format!("{label}-candidate.json"));
    let launcher_path = directory.join(format!("{label}-launcher.txt"));
    let completion_path = directory.join(format!("{label}-completion.json"));
    let expected_path = directory.join(format!("{label}-expected.json"));
    for (path, bytes) in [
        (&candidate_path, candidate.as_slice()),
        (&launcher_path, launcher.as_slice()),
        (&completion_path, completion.as_slice()),
        (&expected_path, expected.as_slice()),
    ] {
        std::fs::write(path, bytes).unwrap();
    }
    let output = Command::new(env!("CARGO_BIN_EXE_dca-state-poc"))
        .arg("import-fixture")
        .arg(candidate_path)
        .arg(launcher_path)
        .arg(completion_path)
        .arg(&expected_path)
        .arg(directory.join("store"))
        .output()
        .unwrap();
    assert!(
        output.status.success(),
        "{}",
        String::from_utf8_lossy(&output.stderr)
    );
    let receipt: serde_json::Value = serde_json::from_slice(&output.stdout).unwrap();
    (
        receipt["artifact_sha256"].as_str().unwrap().to_owned(),
        expected_path,
    )
}

#[test]
fn projection_is_private_even_with_umask_022() {
    use std::os::unix::fs::PermissionsExt;
    let directory = tempfile::tempdir().unwrap();
    std::fs::create_dir(directory.path().join("store")).unwrap();
    let (hash, expected) = import(directory.path(), "first", "owned fixture");
    let projection = directory.path().join("restore.json");
    // Test-only shell, with all dynamic paths passed as distinct positional args.
    let output = Command::new("/bin/sh")
        .args(["-c", "umask 022; exec \"$@\"", "dca-state-test"])
        .arg(env!("CARGO_BIN_EXE_dca-state-poc"))
        .arg("export-fixture")
        .arg(directory.path().join("store"))
        .arg(hash)
        .arg(expected)
        .arg(&projection)
        .output()
        .unwrap();
    assert!(
        output.status.success(),
        "{}",
        String::from_utf8_lossy(&output.stderr)
    );
    assert_eq!(
        std::fs::metadata(projection).unwrap().permissions().mode() & 0o777,
        0o600
    );
}

#[test]
fn another_valid_artifact_value_cannot_be_exported_under_the_first_expectation() {
    let directory = tempfile::tempdir().unwrap();
    std::fs::create_dir(directory.path().join("store")).unwrap();
    let (_, expected) = import(directory.path(), "first", "owned fixture");
    let (other_hash, _) = import(directory.path(), "second", "different fixture");
    let projection = directory.path().join("must-not-exist.json");
    let output = Command::new(env!("CARGO_BIN_EXE_dca-state-poc"))
        .arg("export-fixture")
        .arg(directory.path().join("store"))
        .arg(other_hash)
        .arg(expected)
        .arg(&projection)
        .output()
        .unwrap();
    assert_eq!(output.status.code(), Some(2));
    assert_eq!(output.stderr, b"dca_state.capture_mismatch\n");
    assert!(output.stdout.is_empty());
    assert!(!projection.exists());
}
