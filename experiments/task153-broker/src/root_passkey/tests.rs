// SPDX-License-Identifier: Apache-2.0
//! Synthetic keys only; no native root configuration or user credential access.

use super::*;
use task152_passkey_prototype::guard;
use webauthn_authenticator_rs::{WebauthnAuthenticator, softpasskey::SoftPasskey};

fn input() -> guard::PlanInput {
    guard::PlanInput {
        operation_id: "synthetic-root-verification".into(),
        route: guard::Route {
            gateway: "synthetic-gateway".into(),
            bus_path: "synthetic-bus".into(),
            target: "synthetic-device".into(),
            identity: [7; 32],
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
        purpose: "Synthetic enrolled verifier".into(),
        recovery: "Synthetic restoration".into(),
        expires_at_utc_seconds: 1000,
    }
}

fn enrollment_fixture() -> (Vec<u8>, SoftPasskey) {
    let origin = Url::parse(OPERATOR_ORIGIN).unwrap();
    let webauthn = WebauthnBuilder::new("localhost", &origin)
        .unwrap()
        .build()
        .unwrap();
    let (options, state) = webauthn
        .start_passkey_registration(Uuid::new_v4(), "synthetic", "Synthetic operator", None)
        .unwrap();
    // The dev authenticator deliberately simulates UV; it has no user key.
    let mut authenticator = SoftPasskey::new(true);
    let response = authenticator.do_registration(origin, options).unwrap();
    let credential = webauthn
        .finish_passkey_registration(&response, &state)
        .unwrap();
    let record = serde_json::to_vec(&serde_json::json!({
        "schema": 1, "kind": "operator", "issuer": "synthetic-root-issuer", "credential": credential,
    })).unwrap();
    (record, authenticator)
}

fn verifier_fixture() -> (RootPasskeyVerifier, SoftPasskey) {
    let (record, authenticator) = enrollment_fixture();
    // Private test seam only. No RootOwnedSnapshot or native load is fabricated.
    (
        RootPasskeyVerifier::from_record(&record).unwrap(),
        authenticator,
    )
}

fn sign(authenticator: &mut SoftPasskey, options: RequestChallengeResponse) -> Vec<u8> {
    let response = authenticator
        .do_authentication(Url::parse(OPERATOR_ORIGIN).unwrap(), options)
        .unwrap();
    serde_json::to_vec(&response).unwrap()
}

#[test]
fn serialized_public_enrollment_verifies_signature_but_returns_no_authorization() {
    let (mut verifier, mut authenticator) = verifier_fixture();
    let plan = ConfiguredPlan::new(input()).unwrap();
    let before = serde_json::to_value(&verifier.credential).unwrap();
    let options = verifier.begin(&plan, 100).unwrap();
    assert_eq!(
        serde_json::to_value(options.public_key.user_verification).unwrap(),
        "required"
    );
    let response = sign(&mut authenticator, options);
    let observed = verifier.finish(&plan, &response, 101).unwrap();
    assert_eq!(observed.plan_digest(), plan.digest());
    assert!(!observed.physical_authorization());
    assert!(observed.credential_update_pending());
    let after = serde_json::to_value(&verifier.credential).unwrap();
    assert_ne!(before["cred"]["counter"], after["cred"]["counter"]);
    assert_eq!(before["cred"]["cred"], after["cred"]["cred"]);
    assert_eq!(
        verifier.finish(&plan, &response, 102).unwrap_err(),
        Error::ChallengeUnavailable
    );
}

#[test]
fn altered_plan_expiry_cancellation_and_clock_rollback_consume_ceremony() {
    for case in 0..4 {
        let (mut verifier, mut authenticator) = verifier_fixture();
        let plan = ConfiguredPlan::new(input()).unwrap();
        let options = verifier.begin(&plan, 100).unwrap();
        let response = sign(&mut authenticator, options);
        let (selected, now, expected) = match case {
            0 => {
                let mut changed = input();
                changed.route.target.push('x');
                (
                    ConfiguredPlan::new(changed).unwrap(),
                    101,
                    Error::PlanMismatch,
                )
            }
            1 => (plan, 220, Error::Expired),
            2 => {
                verifier.cancel();
                (plan, 101, Error::ChallengeUnavailable)
            }
            _ => (plan, 99, Error::InvalidTime),
        };
        assert_eq!(
            verifier.finish(&selected, &response, now).unwrap_err(),
            expected
        );
        assert_eq!(
            verifier.finish(&selected, &response, now).unwrap_err(),
            Error::ChallengeUnavailable
        );
    }
}

#[test]
fn wrong_origin_rp_unknown_credential_missing_uv_and_signature_corruption_fail() {
    for case in 0..5 {
        let (mut verifier, mut authenticator) = verifier_fixture();
        let plan = ConfiguredPlan::new(input()).unwrap();
        let mut options = verifier.begin(&plan, 100).unwrap();
        let origin = match case {
            0 => Url::parse("http://localhost:15301").unwrap(),
            1 => {
                options.public_key.rp_id = "other.example".into();
                Url::parse("https://other.example").unwrap()
            }
            _ => Url::parse(OPERATOR_ORIGIN).unwrap(),
        };
        if case == 3 {
            options.public_key.user_verification = serde_json::from_str("\"discouraged\"").unwrap();
        }
        let mut response = authenticator.do_authentication(origin, options).unwrap();
        if case == 2 {
            response.raw_id = vec![99; 16];
        }
        if case == 4 {
            response.response.signature[0] ^= 1;
        }
        assert_eq!(
            verifier
                .finish(&plan, &serde_json::to_vec(&response).unwrap(), 101)
                .unwrap_err(),
            Error::VerificationFailed
        );
    }
}

#[test]
fn plan_expiry_and_pending_challenge_are_enforced_before_signing() {
    let (mut verifier, mut authenticator) = verifier_fixture();
    let mut selected = input();
    selected.expires_at_utc_seconds = 110;
    let plan = ConfiguredPlan::new(selected).unwrap();
    let options = verifier.begin(&plan, 100).unwrap();
    assert!(matches!(
        verifier.begin(&plan, 101),
        Err(Error::ChallengePending)
    ));
    let response = sign(&mut authenticator, options);
    assert_eq!(
        verifier.finish(&plan, &response, 110).unwrap_err(),
        Error::Expired
    );
    assert!(matches!(verifier.begin(&plan, 110), Err(Error::Expired)));
}

#[test]
fn enrollment_schema_development_records_unknown_fields_and_bounds_are_rejected() {
    let (record, _authenticator) = enrollment_fixture();
    for case in 0..5 {
        let mut altered: serde_json::Value = serde_json::from_slice(&record).unwrap();
        match case {
            0 => altered["schema"] = 2.into(),
            1 => altered["kind"] = "development".into(),
            2 => altered["issuer"] = "".into(),
            3 => altered["unexpected"] = true.into(),
            _ => altered["credential"] = serde_json::json!({}),
        }
        assert!(matches!(
            RootPasskeyVerifier::from_record(&serde_json::to_vec(&altered).unwrap()),
            Err(Error::InvalidEnrollment)
        ));
    }
    assert!(matches!(
        RootPasskeyVerifier::from_record(&vec![b' '; MAX_CONFIGURATION_BYTES + 1]),
        Err(Error::InvalidEnrollment)
    ));
    for duplicate in [
        b"{\"x\":1,\"x\":2}".as_slice(),
        b"{\"x\":[{\"a\":0,\"a\":1}]}".as_slice(),
    ] {
        assert!(unique_json::<serde_json::Value>(duplicate).is_err());
    }
}

#[test]
fn malformed_or_oversized_assertion_consumes_challenge_without_observation() {
    for response in [
        b"{}".to_vec(),
        vec![b' '; MAX_ASSERTION_BYTES + 1],
        b"{\"x\":0,\"x\":1}".to_vec(),
    ] {
        let (mut verifier, _authenticator) = verifier_fixture();
        let plan = ConfiguredPlan::new(input()).unwrap();
        verifier.begin(&plan, 100).unwrap();
        assert_eq!(
            verifier.finish(&plan, &response, 101).unwrap_err(),
            Error::InvalidResponse
        );
        assert_eq!(
            verifier.finish(&plan, &response, 101).unwrap_err(),
            Error::ChallengeUnavailable
        );
    }
}

#[test]
fn verified_synthetic_counter_and_exact_assertion_are_persisted_before_observation() {
    let (mut verifier, mut authenticator) = verifier_fixture();
    let plan = ConfiguredPlan::new(input()).unwrap();
    let options = verifier.begin(&plan, 100).unwrap();
    let response = sign(&mut authenticator, options);
    let directory = tempfile::tempdir_in("/private/tmp").unwrap();
    let parent = std::fs::File::open(directory.path()).unwrap();
    let observed = verifier
        .finish_with_store(&plan, &response, 101, |record| {
            assert_eq!(record.plan_digest, plan.digest());
            assert_eq!(
                record.assertion_sha256,
                <[u8; 32]>::from(Sha256::digest(&response))
            );
            assert_eq!(record.expires_at_utc_seconds, 220);
            let bytes = serde_json::to_vec(record).unwrap();
            crate::assertion_store::store_synthetic_record_for_test(&parent, &bytes)
        })
        .unwrap();
    assert!(!observed.credential_update_pending());
    assert!(!observed.physical_authorization());
    let stored: serde_json::Value = serde_json::from_slice(
        &std::fs::read(directory.path().join("verified-assertion.json")).unwrap(),
    )
    .unwrap();
    assert_eq!(
        stored["credential"],
        serde_json::to_value(&verifier.credential).unwrap()
    );
    assert_eq!(stored["physical_authorization"], false);
    // A second valid ceremony cannot overwrite the first record/counter state.
    let options = verifier.begin(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    assert_eq!(
        verifier
            .finish_with_store(&plan, &response, 103, |record| {
                crate::assertion_store::store_synthetic_record_for_test(
                    &parent,
                    &serde_json::to_vec(record).unwrap(),
                )
            })
            .unwrap_err(),
        Error::ProtectedLedgerUnavailable
    );
    assert_eq!(
        verifier.finish(&plan, &response, 104).unwrap_err(),
        Error::ChallengeUnavailable
    );
}

#[test]
fn persistence_failure_consumes_verified_ceremony_without_success_observation() {
    let (mut verifier, mut authenticator) = verifier_fixture();
    let plan = ConfiguredPlan::new(input()).unwrap();
    let options = verifier.begin(&plan, 100).unwrap();
    let response = sign(&mut authenticator, options);
    assert_eq!(
        verifier
            .finish_with_store(&plan, &response, 101, |_| Err(
                crate::assertion_store::Error::Io
            ))
            .unwrap_err(),
        Error::ProtectedLedgerUnavailable
    );
    assert_eq!(
        verifier.finish(&plan, &response, 102).unwrap_err(),
        Error::ChallengeUnavailable
    );
    assert!(verifier.credential_update_pending);
}

/// Actual standard WebAuthn+filesystem fixture, never root or hardware proof.
/// Only the isolated exact-source integration runner consumes this witness.
#[allow(dead_code)]
pub(crate) fn persisted_assertion_fixture(
    plan: &ConfiguredPlan,
    parent: &std::fs::File,
) -> PersistedAssertion {
    persisted_assertion_completion_fixture(plan, parent).1
}

pub(crate) fn persisted_assertion_completion_fixture(
    plan: &ConfiguredPlan,
    parent: &std::fs::File,
) -> (RootAssertionObservation, PersistedAssertion) {
    let (mut verifier, mut authenticator) = verifier_fixture();
    let options = verifier.begin(plan, 100).unwrap();
    let response = sign(&mut authenticator, options);
    verifier
        .complete_with_store(plan, &response, 101, |record| {
            crate::assertion_store::store_synthetic_record_for_test(
                parent,
                &serde_json::to_vec(record).unwrap(),
            )
        })
        .unwrap()
}
