use super::*;

fn fixture() -> (Vec<u8>, Vec<u8>, Vec<u8>, FixtureExpected) {
    let identity = crate::tests::identity();
    let source = FixtureSource {
        kind: "owned-offline-fixture".into(),
        uuid_revision_synthetic: true,
        project_id: "owned-project".into(),
        installation_id: "owned-installation".into(),
        device_id: "owned-device".into(),
        sdk_sha256: "d".repeat(64),
        object_model_sha256: "e".repeat(64),
    };
    let candidate = serde_json::to_vec(&Candidate {
        format_version: 1,
        kind: "ordinary-sdk-string".into(),
        host_process_id: 7,
        project_id: source.project_id.clone(),
        installation_id: source.installation_id.clone(),
        device_id: source.device_id.clone(),
        application_id: identity.application_id.clone(),
        product_sha256: identity.product_sha256.clone(),
        dca_sha256: identity.dca_sha256.clone(),
        dca_version: identity.dca_version.clone(),
        sdk_sha256: source.sdk_sha256.clone(),
        object_model_sha256: source.object_model_sha256.clone(),
        host_sha256: "f".repeat(64),
        parameter_unique_id: identity.parameter_unique_id.clone(),
        parameter_internal_name: identity.parameter_internal_name.clone(),
        value: "owned fixture".into(),
        host_final_validation: "PASS_BEFORE_EXIT".into(),
        requires_confirmed_exit_zero: true,
        accepted_configuration: false,
    })
    .unwrap();
    let launcher = b"LAUNCH_START pid=7 mode=original-sdk-product-store-fixture deadline-seconds=660 capture-bytes=65536 descendants=NOT_CONTAINED\nLAUNCH_RESULT reason=EXIT child-exit=0 child-exit-confirmed=true captured-bytes=0 descendants=NOT_CONTAINED\n".to_vec();
    let completion = serde_json::to_vec(&Completion {
        status: Status {
            state: "Ready".into(),
            result: 0,
            done: true,
            owned: 0,
        },
        source: "OWNED_TEST".into(),
        qualification: "NOT_ESTABLISHED".into(),
        hardware: "NOT_RUN".into(),
    })
    .unwrap();
    let expected = FixtureExpected {
        identity,
        originating_revision: "c".repeat(40),
        source,
        host_sha256: "f".repeat(64),
        host_process_id: 7,
        candidate_sha256: digest(&candidate),
        launcher_stdout_sha256: digest(&launcher),
        completion_sha256: digest(&completion),
        expected_value_sha256: digest(b"owned fixture"),
    };
    (candidate, launcher, completion, expected)
}

#[test]
fn imports_only_confirmed_fixture_and_binds_restore_source_to_artifact() {
    let (candidate, launcher, completion, expected) = fixture();
    let state = State::import_fixture(&candidate, &launcher, &completion, &expected).unwrap();
    assert_eq!(state.value(), "owned fixture");
    let encoded = state.canonical_bytes().unwrap();
    assert_eq!(State::decode(&encoded, &expected.identity).unwrap(), state);
    let projection = state.fixture_restore_projection(&expected).unwrap();
    let object: serde_json::Value = serde_json::from_slice(&projection).unwrap();
    assert_eq!(object["project_id"], "owned-project");
    assert_eq!(object["state_artifact_sha256"], digest(&encoded));
    assert_eq!(object["fixture_uuid_revision_synthetic"], true);
    let mut wrong = expected.clone();
    wrong.source.project_id = "another-project".into();
    assert!(matches!(
        state.fixture_restore_projection(&wrong),
        Err(Error::IdentityMismatch)
    ));
}

#[test]
fn rejects_identity_pin_value_or_receipt_mismatch() {
    let (candidate, launcher, completion, expected) = fixture();
    for field in 0..8 {
        let mut wrong = expected.clone();
        match field {
            0 => wrong.source.project_id.push('x'),
            1 => wrong.source.installation_id.push('x'),
            2 => wrong.source.device_id.push('x'),
            3 => wrong.source.sdk_sha256 = "a".repeat(64),
            4 => wrong.source.object_model_sha256 = "a".repeat(64),
            5 => wrong.host_sha256 = "a".repeat(64),
            6 => wrong.expected_value_sha256 = "a".repeat(64),
            _ => wrong.identity.application_id.push('x'),
        }
        assert!(matches!(
            State::import_fixture(&candidate, &launcher, &completion, &wrong),
            Err(Error::CaptureMismatch)
        ));
    }
    let mut wrong = expected;
    wrong.candidate_sha256 = "a".repeat(64);
    assert!(matches!(
        State::import_fixture(&candidate, &launcher, &completion, &wrong),
        Err(Error::IntegrityMismatch)
    ));
}

#[test]
fn candidate_success_claim_cannot_replace_failed_independent_completion() {
    let (candidate, launcher, mut completion, mut expected) = fixture();
    let mut parsed: Completion = serde_json::from_slice(&completion).unwrap();
    parsed.status.result = 2;
    completion = serde_json::to_vec(&parsed).unwrap();
    expected.completion_sha256 = digest(&completion);
    assert!(matches!(
        State::import_fixture(&candidate, &launcher, &completion, &expected),
        Err(Error::UnconfirmedExit)
    ));
    parsed.status.result = 0;
    parsed.status.owned = 1;
    completion = serde_json::to_vec(&parsed).unwrap();
    expected.completion_sha256 = digest(&completion);
    assert!(matches!(
        State::import_fixture(&candidate, &launcher, &completion, &expected),
        Err(Error::UnconfirmedExit)
    ));
}

#[test]
fn failed_or_unconfirmed_wrong_process_launcher_is_rejected() {
    let (candidate, launcher, completion, expected) = fixture();
    for change in ["child-exit=2", "child-exit-confirmed=false", "pid=8"] {
        let original = if change.starts_with("pid") {
            "pid=7"
        } else if change.starts_with("child-exit-confirmed") {
            "child-exit-confirmed=true"
        } else {
            "child-exit=0"
        };
        let changed = String::from_utf8(launcher.clone())
            .unwrap()
            .replace(original, change)
            .into_bytes();
        let mut expected = expected.clone();
        expected.launcher_stdout_sha256 = digest(&changed);
        assert!(matches!(
            State::import_fixture(&candidate, &changed, &completion, &expected),
            Err(Error::UnconfirmedExit)
        ));
    }
}

#[test]
fn rejects_candidate_duplicates_unknowns_and_bounds_even_with_matching_external_hash() {
    let (candidate, launcher, completion, expected) = fixture();
    for inserted in ["\"format_version\":1,", "\"unknown\":false,"] {
        let invalid = String::from_utf8(candidate.clone())
            .unwrap()
            .replacen('{', &format!("{{{inserted}"), 1)
            .into_bytes();
        let mut expected = expected.clone();
        expected.candidate_sha256 = digest(&invalid);
        assert!(matches!(
            State::import_fixture(&invalid, &launcher, &completion, &expected),
            Err(Error::Json(_))
        ));
    }
    let invalid = vec![b' '; MAX_CANDIDATE_BYTES + 1];
    let mut expected = expected;
    expected.candidate_sha256 = digest(&invalid);
    assert!(matches!(
        State::import_fixture(&invalid, &launcher, &completion, &expected),
        Err(Error::Limit)
    ));
}

#[test]
fn ephemeral_process_receipt_changes_do_not_change_configuration_artifact() {
    let (candidate, launcher, completion, expected) = fixture();
    let first = State::import_fixture(&candidate, &launcher, &completion, &expected).unwrap();
    let mut parsed: Candidate = serde_json::from_slice(&candidate).unwrap();
    parsed.host_process_id = 8;
    let second_candidate = serde_json::to_vec(&parsed).unwrap();
    let second_launcher = String::from_utf8(launcher)
        .unwrap()
        .replace("pid=7", "pid=8")
        .into_bytes();
    let mut second_expected = expected;
    second_expected.host_process_id = 8;
    second_expected.candidate_sha256 = digest(&second_candidate);
    second_expected.launcher_stdout_sha256 = digest(&second_launcher);
    let second = State::import_fixture(
        &second_candidate,
        &second_launcher,
        &completion,
        &second_expected,
    )
    .unwrap();
    assert_eq!(
        first.canonical_bytes().unwrap(),
        second.canonical_bytes().unwrap()
    );
}
