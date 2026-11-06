// SPDX-License-Identifier: Apache-2.0
//! Source-only service tests; no installation, root endpoint or user keys.

use super::*;
use std::io::Read;
use std::os::unix::net::UnixStream;

fn input() -> guard::PlanInput {
    guard::PlanInput {
        operation_id: "synthetic-listener-plan".into(),
        route: guard::Route {
            gateway: "192.0.2.50:3671".into(),
            bus_path: "knxnetip-udp".into(),
            target: "1.1.231".into(),
            identity: reference_descriptor_fingerprint(),
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
        purpose: "Synthetic policy validation; no hardware connection".into(),
        recovery: "Synthetic restoration".into(),
        expires_at_utc_seconds: 1000,
    }
}

#[test]
fn actual_nonroot_startup_and_arbitrary_arguments_are_denied() {
    assert_eq!(run(vec![]), Err(Error::Arguments));
    assert_eq!(
        run(vec![
            "--operator-service-only".into(),
            "--socket".into(),
            "/tmp/other".into()
        ]),
        Err(Error::Arguments)
    );
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        assert_eq!(
            run(vec!["--operator-service-only".into()]),
            Err(Error::RootRequired)
        );
    }
}

#[test]
fn fixed_reference_route_profile_descriptor_and_digest_are_all_required() {
    let selected = input();
    let digest = ConfiguredPlan::new(selected.clone()).unwrap().digest();
    assert!(
        parse_plan(
            &serde_json::to_vec(&selected).unwrap(),
            digest,
            "192.0.2.50:3671",
            100
        )
        .is_ok()
    );
    for mutate in [
        |plan: &mut guard::PlanInput| plan.route.target = "1.1.230".into(),
        |plan: &mut guard::PlanInput| plan.route.gateway = "192.0.2.1:3671".into(),
        |plan: &mut guard::PlanInput| plan.route.bus_path = "other".into(),
        |plan: &mut guard::PlanInput| plan.route.identity = [8; 32],
        |plan: &mut guard::PlanInput| plan.selector.property = 15,
        |plan: &mut guard::PlanInput| plan.desired = vec![8],
        |plan: &mut guard::PlanInput| plan.expires_at_utc_seconds = 100,
    ] {
        let mut changed = input();
        mutate(&mut changed);
        assert!(matches!(
            parse_plan(
                &serde_json::to_vec(&changed).unwrap(),
                digest,
                "192.0.2.50:3671",
                100
            ),
            Err(Error::PlanRejected)
        ));
    }
    assert!(matches!(
        parse_plan(
            &serde_json::to_vec(&selected).unwrap(),
            [8; 32],
            "192.0.2.50:3671",
            100
        ),
        Err(Error::PlanRejected)
    ));
}

#[test]
fn manifest_pin_cannot_inject_a_weaker_native_requirement() {
    let record = serde_json::json!({"schema":1,"executable_sha256":vec![1u8;32],"executable_cdhash":"a".repeat(40),"plan_digest":vec![2u8;32],"gateway":"192.0.2.50:3671"});
    assert!(parse_manifest(&serde_json::to_vec(&record).unwrap()).is_ok());
    for pin in [
        "always".to_owned(),
        "a".repeat(39),
        "0\" or always or cdhash H\"".to_owned(),
        "z".repeat(40),
    ] {
        let mut changed = record.clone();
        changed["executable_cdhash"] = pin.into();
        assert!(matches!(
            parse_manifest(&serde_json::to_vec(&changed).unwrap()),
            Err(Error::ManifestRejected)
        ));
    }
    assert!(matches!(
        parse_manifest(b"{\"schema\":1,\"schema\":1}"),
        Err(Error::ManifestRejected)
    ));
}

#[test]
fn gateway_pin_requires_canonical_ipv4_and_nonzero_port_and_exact_plan_match() {
    for invalid in [
        "",
        "example.invalid:3671",
        "[2001:db8::1]:3671",
        "192.0.2.50",
        "192.0.2.50:0",
        "192.0.2.050:3671",
        "192.0.2.50:3671 ",
    ] {
        assert!(!valid_gateway(invalid));
        let record = serde_json::json!({"schema":1,"executable_sha256":vec![1u8;32],"executable_cdhash":"a".repeat(40),"plan_digest":vec![2u8;32],"gateway":invalid});
        assert!(matches!(
            parse_manifest(&serde_json::to_vec(&record).unwrap()),
            Err(Error::ManifestRejected)
        ));
    }
    let selected = input();
    let digest = ConfiguredPlan::new(selected.clone()).unwrap().digest();
    assert!(valid_gateway("203.0.113.4:3671"));
    assert!(matches!(
        parse_plan(
            &serde_json::to_vec(&selected).unwrap(),
            digest,
            "203.0.113.4:3671",
            100
        ),
        Err(Error::PlanRejected)
    ));
}

#[test]
fn native_dynamic_code_check_matches_only_the_current_test_images_pin() {
    // Fixed vendor tool, display-only, against this test executable. It neither
    // signs code nor enumerates user certificates, identities or keychains.
    // A self-derived test pin is never written to a trusted manifest.
    let output = std::process::Command::new("/usr/bin/codesign")
        .args(["--display", "--verbose=4"])
        .arg(std::env::current_exe().unwrap())
        .output()
        .unwrap();
    assert!(output.status.success());
    let text = std::str::from_utf8(&output.stderr).unwrap();
    let pin = text
        .lines()
        .find_map(|line| line.strip_prefix("CDHash="))
        .unwrap();
    assert_eq!(verify_running_image(pin), Ok(()));
    assert_eq!(
        verify_running_image(&"0".repeat(40)),
        Err(Error::RunningImageRejected)
    );
}

#[test]
fn bounded_actual_local_listener_closes_denied_session_without_root_storage() {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        let directory = tempfile::Builder::new()
            .prefix("task153-listener-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let path = directory.path().join("socket");
        let listener = UnixListener::bind(&path).unwrap();
        listener.set_nonblocking(true).unwrap();
        let mut client = UnixStream::connect(&path).unwrap();
        let plan = ConfiguredPlan::new(input()).unwrap();
        assert_eq!(
            serve_connections(&listener, &plan, Instant::now() + Duration::from_secs(2), 1),
            Ok(())
        );
        let mut bytes = Vec::new();
        client.read_to_end(&mut bytes).unwrap();
        assert!(bytes.is_empty());
        drop(listener);
        assert!(path.exists());
    }
}
