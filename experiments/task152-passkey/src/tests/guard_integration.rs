// SPDX-License-Identifier: Apache-2.0
//! Test-only bridge: synthetic authenticator evidence and a simulated session.
//! This module is never linked into a normal library or browser executable.

use super::*;
use crate::guard::dispatcher::{
    CleanupEvidence, Clock, DispatchError, SessionError, SessionRead, TypedSession, execute,
};
use std::future::Future;
use std::pin::pin;
use std::task::{Context, Poll, Waker};

struct FixedClock(u64);
impl Clock for FixedClock {
    fn now(&self) -> u64 {
        self.0
    }
}

fn ready<F: Future>(future: F) -> F::Output {
    let mut future = pin!(future);
    match future
        .as_mut()
        .poll(&mut Context::from_waker(Waker::noop()))
    {
        Poll::Ready(result) => result,
        Poll::Pending => panic!("immediate synthetic session unexpectedly pending"),
    }
}

struct SyntheticSession {
    route: guard::Route,
    value: Vec<u8>,
    reads: usize,
    writes: Vec<Vec<u8>>,
    closes: usize,
    uncertain_write: bool,
}

impl SyntheticSession {
    fn new() -> Self {
        Self {
            route: input().route,
            value: input().original,
            reads: 0,
            writes: Vec::new(),
            closes: 0,
            uncertain_write: false,
        }
    }

    fn untouched(&self) {
        assert_eq!(self.reads, 0);
        assert!(self.writes.is_empty());
        assert_eq!(self.closes, 0);
    }
}

impl TypedSession for SyntheticSession {
    fn route(&self) -> &guard::Route {
        &self.route
    }

    async fn observe(&mut self, selector: &guard::Selector) -> Result<SessionRead, SessionError> {
        self.reads += 1;
        Ok(SessionRead {
            route: self.route.clone(),
            selector: selector.clone(),
            value: self.value.clone(),
        })
    }

    async fn write(&mut self, dispatch: guard::BoundDispatch) -> Result<SessionRead, SessionError> {
        assert_eq!(dispatch.route(), &self.route);
        self.value = dispatch.bytes().to_vec();
        self.writes.push(self.value.clone());
        if self.uncertain_write {
            return Err(SessionError::Uncertain);
        }
        Ok(SessionRead {
            route: self.route.clone(),
            selector: dispatch.selector().clone(),
            value: self.value.clone(),
        })
    }

    async fn close(&mut self) -> Result<CleanupEvidence, SessionError> {
        self.closes += 1;
        Ok(CleanupEvidence::DeviceAndGatewayConfirmed)
    }
}

fn journal() -> (tempfile::TempDir, guard::Journal) {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let journal = guard::Journal::open(&root).unwrap();
    (directory, journal)
}

fn verified_fixture() -> (tempfile::TempDir, DevelopmentReceipt) {
    let (directory, mut verifier, mut authenticator) = enrolled();
    let plan = BoundPlan::new(input()).unwrap();
    let options = verifier.begin_assertion(&plan, 102).unwrap();
    let response = sign(&mut authenticator, options);
    let receipt = verifier.finish_assertion(&plan, &response, 103).unwrap();
    (directory, receipt)
}

/// Test-only seam, not a conversion offered by the library. It deliberately
/// produces a fixture used only with a simulated session in this module.
fn synthetic_grant(receipt: &DevelopmentReceipt) -> guard::HumanApproval {
    assert!(!receipt.physical_authorization);
    assert!(receipt.user_verified);
    let digest = Sha256::digest(receipt.challenge_id.as_bytes());
    let mut id = [0; 16];
    id.copy_from_slice(&digest[..16]);
    guard::synthetic_approval_for_test(id, receipt.plan_digest, receipt.expires_at_utc_seconds)
}

#[test]
fn verified_synthetic_assertion_drives_exact_mock_write_read_restore_and_cleanup() {
    let (_credential_dir, receipt) = verified_fixture();
    let (_journal_dir, journal) = journal();
    let mut session = SyntheticSession::new();
    let report = ready(execute(
        &journal,
        guard::Plan::new(input()).unwrap(),
        Some(synthetic_grant(&receipt)),
        &mut session,
        &FixedClock(104),
    ));
    assert!(report.succeeded());
    assert_eq!(session.reads, 4);
    assert_eq!(session.writes, [vec![4], vec![0]]);
    assert_eq!(session.value, vec![0]);
    assert_eq!(session.closes, 1);
    assert_eq!(
        journal.recovered_status(&input().operation_id),
        guard::Status::RestoredVerified
    );
}

#[test]
fn verified_development_receipt_alone_still_causes_zero_session_calls() {
    let (_credential_dir, receipt) = verified_fixture();
    assert!(!receipt.physical_authorization());
    let (_journal_dir, journal) = journal();
    let mut session = SyntheticSession::new();
    let report = ready(execute(
        &journal,
        guard::Plan::new(input()).unwrap(),
        None,
        &mut session,
        &FixedClock(104),
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(guard::Error::ApprovalUnavailable))
    );
    assert_eq!(report.cleanup, None);
    session.untouched();
}

#[test]
fn changed_plan_after_verified_assertion_is_rejected_before_session_calls() {
    for mutate in [
        |p: &mut guard::PlanInput| p.route.target.push('x'),
        |p: &mut guard::PlanInput| p.selector.property += 1,
        |p: &mut guard::PlanInput| p.desired[0] = 8,
        |p: &mut guard::PlanInput| p.recovery.push('x'),
    ] {
        let (_credential_dir, receipt) = verified_fixture();
        let (_journal_dir, journal) = journal();
        let mut session = SyntheticSession::new();
        let mut altered = input();
        mutate(&mut altered);
        let report = ready(execute(
            &journal,
            guard::Plan::new(altered).unwrap(),
            Some(synthetic_grant(&receipt)),
            &mut session,
            &FixedClock(104),
        ));
        assert_eq!(
            report.operation,
            Err(DispatchError::Guard(guard::Error::ApprovalMismatch))
        );
        session.untouched();
    }
}

#[test]
fn verified_assertion_cannot_start_after_its_bound_expiry() {
    let (_credential_dir, receipt) = verified_fixture();
    let (_journal_dir, journal) = journal();
    let mut session = SyntheticSession::new();
    let report = ready(execute(
        &journal,
        guard::Plan::new(input()).unwrap(),
        Some(synthetic_grant(&receipt)),
        &mut session,
        &FixedClock(receipt.expires_at_utc_seconds),
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(guard::Error::Expired))
    );
    session.untouched();
}

#[test]
fn consumed_synthetic_assertion_grant_cannot_repeat_a_write_after_journal_reopen() {
    let (_credential_dir, receipt) = verified_fixture();
    let (journal_dir, first_journal) = journal();
    let mut session = SyntheticSession::new();
    assert!(
        ready(execute(
            &first_journal,
            guard::Plan::new(input()).unwrap(),
            Some(synthetic_grant(&receipt)),
            &mut session,
            &FixedClock(104),
        ))
        .succeeded()
    );
    drop(first_journal);
    let journal = guard::Journal::open(&fs::canonicalize(journal_dir.path()).unwrap()).unwrap();
    let mut retried = SyntheticSession::new();
    let report = ready(execute(
        &journal,
        guard::Plan::new(input()).unwrap(),
        Some(synthetic_grant(&receipt)),
        &mut retried,
        &FixedClock(105),
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(guard::Error::AlreadyConsumed))
    );
    assert!(retried.writes.is_empty());
    // The dispatcher takes one read before checking the durable reservation.
    assert_eq!(retried.reads, 1);
    assert_eq!(retried.closes, 1);
}

#[test]
fn passkey_derived_fixture_does_not_restore_after_uncertain_mock_write() {
    let (_credential_dir, receipt) = verified_fixture();
    let (_journal_dir, journal) = journal();
    let mut session = SyntheticSession::new();
    session.uncertain_write = true;
    let report = ready(execute(
        &journal,
        guard::Plan::new(input()).unwrap(),
        Some(synthetic_grant(&receipt)),
        &mut session,
        &FixedClock(104),
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Session(SessionError::Uncertain))
    );
    assert_eq!(session.writes, [vec![4]]);
    assert_eq!(session.closes, 1);
    assert_eq!(
        journal.recovered_status(&input().operation_id),
        guard::Status::OutcomeUnknown
    );
}
