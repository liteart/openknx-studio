// SPDX-License-Identifier: Apache-2.0
//! Synthetic authenticator tests use genuine signatures but no user/device keys.

use super::*;
use webauthn_authenticator_rs::{
    AuthenticatorBackend, WebauthnAuthenticator, softpasskey::SoftPasskey,
};

fn input() -> guard::PlanInput {
    guard::PlanInput {
        operation_id: "synthetic-passkey-plan".into(),
        route: guard::Route {
            gateway: "synthetic-gateway".into(),
            bus_path: "synthetic-bus".into(),
            target: "synthetic-target".into(),
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
        purpose: "Synthetic verification".into(),
        recovery: "Synthetic exact-byte restoration".into(),
        expires_at_utc_seconds: 1000,
    }
}

fn fixture() -> (tempfile::TempDir, DevelopmentVerifier) {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let verifier = DevelopmentVerifier::open(&root, "synthetic-issuer").unwrap();
    (directory, verifier)
}

fn enrolled() -> (tempfile::TempDir, DevelopmentVerifier, SoftPasskey) {
    let (directory, mut verifier) = fixture();
    // This test authenticator explicitly falsifies UV; it is a dev-only dependency.
    let mut authenticator = SoftPasskey::new(true);
    let options = verifier.begin_development_enrollment(100).unwrap();
    assert_eq!(
        serde_json::to_value(
            options
                .public_key
                .authenticator_selection
                .as_ref()
                .unwrap()
                .user_verification
        )
        .unwrap(),
        "required"
    );
    assert!(
        options
            .public_key
            .authenticator_selection
            .as_ref()
            .unwrap()
            .authenticator_attachment
            .is_none()
    );
    let response = authenticator
        .do_registration(Url::parse(DEVELOPMENT_ORIGIN).unwrap(), options)
        .unwrap();
    verifier
        .finish_development_enrollment(&serde_json::to_vec(&response).unwrap(), 101)
        .unwrap();
    (directory, verifier, authenticator)
}

fn sign(authenticator: &mut SoftPasskey, options: RequestChallengeResponse) -> Vec<u8> {
    let response = authenticator
        .do_authentication(Url::parse(DEVELOPMENT_ORIGIN).unwrap(), options)
        .unwrap();
    serde_json::to_vec(&response).unwrap()
}

#[test]
fn no_enrollment_or_challenge_can_yield_an_observation() {
    let (_directory, mut verifier) = fixture();
    let plan = BoundPlan::new(input()).unwrap();
    assert!(matches!(
        verifier.begin_assertion(&plan, 100),
        Err(Error::EnrollmentUnavailable)
    ));
    assert_eq!(
        verifier.finish_assertion(&plan, b"{}", 100).unwrap_err(),
        Error::ChallengeUnavailable
    );
}

#[test]
fn valid_signature_is_durable_but_cannot_authorize_a_device() {
    let (directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    assert_eq!(
        serde_json::to_value(options.public_key.user_verification).unwrap(),
        "required"
    );
    let response = sign(&mut authenticator, options);
    let receipt = verifier.finish_assertion(&plan, &response, 103).unwrap();
    assert_eq!(receipt.plan_digest(), plan.guard_plan().digest());
    assert!(!receipt.physical_authorization());
    let stored = fs::read(
        directory
            .path()
            .join(format!("{}.spent.json", receipt.challenge_id)),
    )
    .unwrap();
    assert_eq!(stored, serde_json::to_vec(&receipt).unwrap());
    assert_eq!(
        verifier
            .finish_assertion(&plan, &response, 104)
            .unwrap_err(),
        Error::ChallengeUnavailable
    );
    // Integration boundary: the cryptographic observation never becomes the
    // guard's private HumanApproval; the existing guard remains fail-closed.
    let journal_root = directory.path().join("guard-journal");
    fs::create_dir(&journal_root).unwrap();
    let journal = guard::Journal::open(&fs::canonicalize(&journal_root).unwrap()).unwrap();
    assert_eq!(
        journal.recovered_status("synthetic-passkey-plan"),
        guard::Status::OutcomeUnknown
    );
}

#[test]
fn changed_target_payload_recovery_and_plan_expiry_are_rejected() {
    for mutate in [
        |p: &mut guard::PlanInput| p.route.target.push('x'),
        |p: &mut guard::PlanInput| p.desired[0] = 8,
        |p: &mut guard::PlanInput| p.recovery.push('x'),
        |p: &mut guard::PlanInput| p.expires_at_utc_seconds += 1,
    ] {
        let (_directory, mut verifier, mut authenticator) = enrolled();
        let plan = BoundPlan::new(input()).unwrap();
        let options = verifier.begin_assertion(&plan, 102).unwrap();
        let response = sign(&mut authenticator, options);
        let mut changed = input();
        mutate(&mut changed);
        assert_eq!(
            verifier
                .finish_assertion(&BoundPlan::new(changed).unwrap(), &response, 103)
                .unwrap_err(),
            Error::PlanMismatch
        );
    }
}

#[test]
fn exact_summary_comes_from_the_same_validated_input() {
    let input = input();
    let plan = BoundPlan::new(input.clone()).unwrap();
    let shown: guard::PlanInput = serde_json::from_str(plan.exact_plan_summary()).unwrap();
    assert_eq!(
        guard::Plan::new(shown).unwrap().digest(),
        plan.guard_plan().digest()
    );
    assert!(plan.exact_plan_summary().contains("synthetic-target"));
    assert!(
        plan.exact_plan_summary()
            .contains("Synthetic exact-byte restoration")
    );
}

#[test]
fn expiration_clock_rollback_and_cancel_are_fail_closed() {
    for (now, expected) in [(222, Error::Expired), (101, Error::InvalidTime)] {
        let (_directory, mut verifier, mut authenticator) = enrolled();
        let plan = BoundPlan::new(input()).unwrap();
        let options = verifier.begin_assertion(&plan, 102).unwrap();
        let response = sign(&mut authenticator, options);
        assert_eq!(
            verifier
                .finish_assertion(&plan, &response, now)
                .unwrap_err(),
            expected
        );
    }
    let (_directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    verifier.cancel();
    assert_eq!(
        verifier
            .finish_assertion(&plan, &response, 103)
            .unwrap_err(),
        Error::ChallengeUnavailable
    );
}

#[test]
fn plan_expiry_shortens_challenge_lifetime() {
    let (_directory, mut verifier, mut authenticator) = enrolled();
    let mut input = input();
    input.expires_at_utc_seconds = 110;
    let plan = BoundPlan::new(input).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    assert_eq!(
        verifier
            .finish_assertion(&plan, &response, 110)
            .unwrap_err(),
        Error::Expired
    );
    assert!(matches!(
        verifier.begin_assertion(&plan, 110),
        Err(Error::Expired)
    ));
}

#[test]
fn wrong_exact_origin_port_and_rp_are_rejected_with_valid_signatures() {
    for case in 0..3 {
        let (_directory, mut verifier, mut authenticator) = enrolled();
        let plan = BoundPlan::new(input()).unwrap();
        let mut options = verifier.begin_assertion(&plan, 102).unwrap();
        let origin = match case {
            0 => Url::parse("http://localhost:15201").unwrap(),
            1 => Url::parse("https://localhost:15200").unwrap(),
            _ => {
                options.public_key.rp_id = "other.example".into();
                Url::parse("https://other.example").unwrap()
            }
        };
        let response = authenticator.do_authentication(origin, options).unwrap();
        assert_eq!(
            verifier
                .finish_assertion(&plan, &serde_json::to_vec(&response).unwrap(), 103)
                .unwrap_err(),
            Error::VerificationFailed
        );
    }
}

#[test]
fn missing_uv_unknown_credential_and_bad_signature_are_rejected() {
    for case in 0..3 {
        let (_directory, mut verifier, mut authenticator) = enrolled();
        let plan = BoundPlan::new(input()).unwrap();
        let mut options = verifier.begin_assertion(&plan, 102).unwrap();
        if case == 0 {
            options.public_key.user_verification = serde_json::from_str("\"discouraged\"").unwrap();
        }
        let mut response = authenticator
            .do_authentication(Url::parse(DEVELOPMENT_ORIGIN).unwrap(), options)
            .unwrap();
        if case == 1 {
            response.raw_id = vec![9; 16];
        }
        if case == 2 {
            response.response.signature[0] ^= 1;
        }
        assert_eq!(
            verifier
                .finish_assertion(&plan, &serde_json::to_vec(&response).unwrap(), 103)
                .unwrap_err(),
            Error::VerificationFailed
        );
    }
}

#[test]
fn signed_wrong_rp_hash_is_rejected_even_with_the_expected_origin() {
    let (_directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let mut options = verifier.begin_assertion(&plan, 102).unwrap();
    options.public_key.rp_id = "other.example".into();
    // The synthetic backend signs the wrong RP hash while retaining the exact
    // expected origin. No handwritten signature or WebAuthn encoding is used.
    let response = authenticator
        .perform_auth(
            Url::parse(DEVELOPMENT_ORIGIN).unwrap(),
            options.public_key,
            120_000,
        )
        .unwrap();
    assert_eq!(
        verifier
            .finish_assertion(&plan, &serde_json::to_vec(&response).unwrap(), 103)
            .unwrap_err(),
        Error::VerificationFailed
    );
}

#[test]
fn response_for_another_challenge_cannot_be_replayed() {
    let (_directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    verifier.cancel();
    let fresh = verifier.begin_assertion(&plan, 103).unwrap();
    assert_eq!(
        verifier
            .finish_assertion(&plan, &response, 104)
            .unwrap_err(),
        Error::VerificationFailed
    );
    assert!(!fresh.public_key.challenge.is_empty());
}

#[test]
fn durable_existing_consumption_is_rejected_before_observation() {
    let (directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let challenge = verifier.assertion.as_ref().unwrap().challenge_id.clone();
    fs::write(
        directory.path().join(format!("{challenge}.spent.json")),
        b"already consumed",
    )
    .unwrap();
    let response = sign(&mut authenticator, options);
    assert_eq!(
        verifier
            .finish_assertion(&plan, &response, 103)
            .unwrap_err(),
        Error::Replay
    );
}

#[test]
fn restart_has_neither_enrollment_nor_resumable_challenge() {
    let (directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    drop(verifier);
    let mut reopened = DevelopmentVerifier::open(
        &fs::canonicalize(directory.path()).unwrap(),
        "synthetic-issuer",
    )
    .unwrap();
    assert_eq!(
        reopened
            .finish_assertion(&plan, &response, 103)
            .unwrap_err(),
        Error::ChallengeUnavailable
    );
    assert!(matches!(
        reopened.begin_assertion(&plan, 103),
        Err(Error::EnrollmentUnavailable)
    ));
}

#[test]
fn response_size_pending_ceremony_and_enrollment_timeout_are_bounded() {
    let (_directory, mut verifier) = fixture();
    verifier.begin_development_enrollment(100).unwrap();
    assert!(matches!(
        verifier.begin_development_enrollment(100),
        Err(Error::ChallengePending)
    ));
    assert_eq!(
        verifier
            .finish_development_enrollment(b"{}", 220)
            .unwrap_err(),
        Error::Expired
    );
    verifier.begin_development_enrollment(221).unwrap();
    assert_eq!(
        verifier
            .finish_development_enrollment(&vec![b' '; MAX_RESPONSE_BYTES + 1], 222)
            .unwrap_err(),
        Error::InvalidResponse
    );
    let (_directory, mut verifier, _authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    assert!(matches!(
        verifier.begin_development_enrollment(102),
        Err(Error::AlreadyEnrolled)
    ));
    verifier.begin_assertion(&plan, 102).unwrap();
    assert!(matches!(
        verifier.begin_assertion(&plan, 102),
        Err(Error::ChallengePending)
    ));
    assert_eq!(
        verifier.finish_assertion(&plan, b"{}", 103).unwrap_err(),
        Error::InvalidResponse
    );
}

#[test]
fn invalid_issuer_or_noncanonical_journal_is_rejected() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    assert!(matches!(
        DevelopmentVerifier::open(&root, "../issuer"),
        Err(Error::InvalidIssuer)
    ));
    assert!(matches!(
        DevelopmentVerifier::open(&root.join(".."), "issuer"),
        Err(Error::UnsafeJournalPath)
    ));
}
