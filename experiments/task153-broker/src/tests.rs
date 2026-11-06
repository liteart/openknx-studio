// SPDX-License-Identifier: Apache-2.0
use super::*;

fn input() -> plan_guard::PlanInput {
    plan_guard::PlanInput {
        operation_id: "synthetic-operation".into(),
        route: plan_guard::Route {
            gateway: "synthetic-gateway".into(),
            bus_path: "synthetic-bus".into(),
            target: "synthetic-target".into(),
            identity: [7; 32],
        },
        selector: plan_guard::Selector {
            object: 0,
            property: 14,
            index: 1,
            count: 1,
            element_size: 1,
        },
        original: vec![0],
        desired: vec![4],
        purpose: "Synthetic broker validation".into(),
        recovery: "Synthetic exact restoration".into(),
        expires_at_utc_seconds: 1_000,
    }
}

fn request(command: Command) -> Vec<u8> {
    serde_json::to_vec(&Request {
        schema: 1,
        request_id: "synthetic-request".into(),
        command,
    })
    .unwrap()
}

#[test]
fn malformed_duplicate_unknown_and_oversized_requests_are_rejected() {
    for bytes in [
        b"".as_slice(),
        b"{}",
        b"{\"schema\":1,\"schema\":1,\"request_id\":\"a\",\"command\":{\"operation\":\"inspect_plan\"}}",
        b"{\"schema\":1,\"request_id\":\"a\",\"command\":{\"operation\":\"write\"}}",
        b"{\"schema\":1,\"request_id\":\"a\",\"command\":{\"operation\":\"inspect_plan\",\"target\":\"other\"}}",
        b"{\"schema\":1,\"request_id\":\"a\",\"command\":{\"operation\":\"inspect_plan\"},\"approved\":true}",
    ] {
        assert!(matches!(decode_request(bytes), Err(Error::InvalidRequest)));
    }
    assert!(matches!(
        decode_request(&vec![b' '; MAX_REQUEST_BYTES + 1]),
        Err(Error::RequestTooLarge)
    ));
}

#[test]
fn protocol_version_and_request_id_are_bounded() {
    let mut value: serde_json::Value =
        serde_json::from_slice(&request(Command::InspectPlan {})).unwrap();
    value["schema"] = 2.into();
    assert!(matches!(
        decode_request(&serde_json::to_vec(&value).unwrap()),
        Err(Error::UnsupportedVersion)
    ));
    value["schema"] = 1.into();
    for id in [
        "".to_string(),
        "x".repeat(65),
        "a/b".into(),
        "a\n".into(),
        "ä".into(),
    ] {
        value["request_id"] = id.into();
        assert!(matches!(
            decode_request(&serde_json::to_vec(&value).unwrap()),
            Err(Error::InvalidRequestId)
        ));
    }
}

#[test]
fn configured_candidate_rejects_other_properties_or_effects() {
    for mutate in [
        |p: &mut plan_guard::PlanInput| p.selector.object = 1,
        |p: &mut plan_guard::PlanInput| p.selector.property = 15,
        |p: &mut plan_guard::PlanInput| p.selector.index = 2,
        |p: &mut plan_guard::PlanInput| p.selector.count = 2,
        |p: &mut plan_guard::PlanInput| p.selector.element_size = 2,
        |p: &mut plan_guard::PlanInput| p.original = vec![1],
        |p: &mut plan_guard::PlanInput| p.desired = vec![8],
    ] {
        let mut configured = input();
        mutate(&mut configured);
        assert!(matches!(
            ConfiguredPlan::new(configured),
            Err(Error::InvalidConfiguredPlan)
        ));
    }
}

#[test]
fn summary_and_requested_digest_use_the_same_existing_guard_snapshot() {
    let plan = ConfiguredPlan::new(input()).unwrap();
    let shown: plan_guard::PlanInput = serde_json::from_str(plan.summary()).unwrap();
    assert_eq!(
        plan_guard::Plan::new(shown).unwrap().digest(),
        plan.digest()
    );
    let decoded = decode_request(&request(Command::ExecuteApproved {
        plan_digest: plan.digest(),
    }))
    .unwrap();
    assert_eq!(plan.check_request(&decoded), Ok(()));
    let mut changed = input();
    changed.route.target.push('x');
    let other = ConfiguredPlan::new(changed).unwrap();
    assert_eq!(other.check_request(&decoded), Err(Error::PlanMismatch));
}

#[test]
fn valid_requests_remain_fail_closed_without_actual_protected_host() {
    let plan = ConfiguredPlan::new(input()).unwrap();
    let digest = plan.digest();
    let broker = UnprovisionedBroker::new(plan);
    for command in [
        Command::InspectPlan {},
        Command::ExecuteApproved {
            plan_digest: digest,
        },
    ] {
        assert_eq!(
            broker.handle(&request(command)),
            Err(Error::ProtectedHostUnavailable)
        );
    }
}
