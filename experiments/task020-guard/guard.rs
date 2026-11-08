// SPDX-License-Identifier: Apache-2.0
//! Offline-only exact-plan and durable-attempt model for Task020.
//!
//! No network adapter, executable, real approval constructor or credential
//! verifier is provided. The private approval type cannot be deserialized or
//! constructed through this module's external API. A real trusted operator
//! channel and executor are prerequisites for physical use.

use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::fs::{self, File, OpenOptions};
use std::io::{Read, Write};
use std::path::{Path, PathBuf};
use std::sync::Arc;

const MAX_VALUE_BYTES: usize = 64;
const MAX_RECORD_BYTES: u64 = 16_384;
/// Every persistence adapter must enforce this bound before allocating a record.
pub const MAX_JOURNAL_RECORD_BYTES: usize = MAX_RECORD_BYTES as usize;
const MAX_OBSERVATION_AGE_SECONDS: u64 = 30;

#[path = "dispatcher.rs"]
pub(crate) mod dispatcher;

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Route {
    pub gateway: String,
    pub bus_path: String,
    pub target: String,
    pub identity: [u8; 32],
}

#[cfg(test)]
mod tests {
    use super::*;

    fn input(id: &str) -> PlanInput {
        PlanInput {
            operation_id: id.into(),
            route: Route {
                gateway: "synthetic-gateway".into(),
                bus_path: "synthetic-bus".into(),
                target: "synthetic-target".into(),
                identity: [7; 32],
            },
            selector: Selector {
                object: 2,
                property: 9,
                index: 1,
                count: 1,
                element_size: 1,
            },
            original: vec![1],
            desired: vec![2],
            purpose: "synthetic state transition".into(),
            recovery: "synthetic exact-byte restoration".into(),
            expires_at_utc_seconds: 200,
        }
    }

    fn observation(plan: &Plan, value: u8, at: u64, receipt: u64) -> Observation {
        Observation {
            route: plan.input.route.clone(),
            selector: plan.input.selector.clone(),
            value: vec![value],
            observed_at_utc_seconds: at,
            receipt,
        }
    }

    // No fixture approval can be obtained through the non-test API.
    fn synthetic_approval(plan: &Plan, id: u8) -> HumanApproval {
        synthetic_approval_for_test([id; 16], plan.digest(), 200)
    }

    fn confirm(attempt: &mut Attempt, now: u64) {
        let watermark = ObservationWatermark {
            plan_digest: attempt.plan.digest,
            receipt: attempt.last_receipt + 1,
            confirmed_at_utc_seconds: now,
        };
        attempt
            .record_write_response(WriteObservation::ResponseConfirmed, Some(watermark), now)
            .unwrap();
    }

    fn journal() -> (tempfile::TempDir, Journal) {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let journal = Journal::open(&root).unwrap();
        (directory, journal)
    }

    fn begin(journal: &Journal, id: &str) -> Attempt {
        let plan = Plan::new(input(id)).unwrap();
        let approval = synthetic_approval(&plan, 1);
        let observed = observation(&plan, 1, 100, 1);
        journal.begin(plan, Some(approval), &observed, 100).unwrap()
    }

    #[test]
    fn missing_real_approval_is_fail_closed_without_journal_mutation() {
        let (_directory, journal) = journal();
        let plan = Plan::new(input("missing-approval")).unwrap();
        let observed = observation(&plan, 1, 100, 1);
        assert_eq!(
            journal.begin(plan, None, &observed, 100).unwrap_err(),
            Error::ApprovalUnavailable
        );
        assert_eq!(
            fs::read_dir(journal.filesystem_root().unwrap())
                .unwrap()
                .count(),
            0
        );
    }

    #[test]
    fn altered_target_selector_payload_and_recovery_do_not_match_approval() {
        for mutate in [
            |p: &mut PlanInput| p.route.target.push('x'),
            |p: &mut PlanInput| p.selector.property += 1,
            |p: &mut PlanInput| p.desired[0] = 3,
            |p: &mut PlanInput| p.recovery.push('x'),
        ] {
            let (_directory, journal) = journal();
            let original = Plan::new(input("bound-plan")).unwrap();
            let approval = synthetic_approval(&original, 1);
            let mut changed = input("bound-plan");
            mutate(&mut changed);
            let changed = Plan::new(changed).unwrap();
            let observed = observation(&changed, 1, 100, 1);
            assert_eq!(
                journal
                    .begin(changed, Some(approval), &observed, 100)
                    .unwrap_err(),
                Error::ApprovalMismatch
            );
            assert_eq!(
                fs::read_dir(journal.filesystem_root().unwrap())
                    .unwrap()
                    .count(),
                0
            );
        }
    }

    #[test]
    fn identity_original_value_and_freshness_are_checked_before_intent() {
        for case in 0..5 {
            let (_directory, journal) = journal();
            let plan = Plan::new(input("preflight")).unwrap();
            let approval = synthetic_approval(&plan, 1);
            let mut observed = observation(&plan, 1, 100, 1);
            match case {
                0 => observed.route.identity[0] ^= 1,
                1 => observed.route.target.push('x'),
                2 => observed.value[0] = 3,
                3 => observed.observed_at_utc_seconds = 69,
                _ => observed.observed_at_utc_seconds = 101,
            }
            let error = journal
                .begin(plan, Some(approval), &observed, 100)
                .unwrap_err();
            assert!(matches!(
                error,
                Error::ObservationMismatch | Error::StaleObservation
            ));
            assert_eq!(
                fs::read_dir(journal.filesystem_root().unwrap())
                    .unwrap()
                    .count(),
                0
            );
        }
    }

    #[test]
    fn expiry_before_dispatch_never_returns_a_selection() {
        let (_directory, journal) = journal();
        let plan = Plan::new(input("expired-at-begin")).unwrap();
        let approval = synthetic_approval(&plan, 1);
        let observed = observation(&plan, 1, 200, 1);
        assert_eq!(
            journal
                .begin(plan, Some(approval), &observed, 200)
                .unwrap_err(),
            Error::Expired
        );
        let mut attempt = begin(&journal, "expired-before-dispatch");
        let observed = observation(&attempt.plan, 1, 200, 2);
        assert_eq!(
            attempt.dispatch_desired(&observed, 200).unwrap_err(),
            Error::Expired
        );
        assert_eq!(attempt.status(), Status::OutcomeUnknown);
    }

    #[test]
    fn durable_intent_precedes_selection_and_replay_is_refused_after_restart() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "consumed");
        assert_eq!(journal.recovered_status("consumed"), Status::OutcomeUnknown);
        let observed = observation(&attempt.plan, 1, 101, 1);
        let selected = attempt
            .dispatch_desired(&observed, 101)
            .unwrap()
            .bind_for_execution(&attempt.plan)
            .unwrap();
        assert_eq!(selected.route(), &attempt.plan.input.route);
        assert_eq!(selected.selector(), &attempt.plan.input.selector);
        assert_eq!(selected.bytes(), [2]);
        assert_eq!(selected.plan_digest(), attempt.plan.digest());
        assert_eq!(
            attempt.dispatch_desired(&observed, 101).unwrap_err(),
            Error::InvalidTransition
        );
        drop(attempt);
        let reopened = Journal::open(journal.filesystem_root().unwrap()).unwrap();
        let plan = Plan::new(input("consumed")).unwrap();
        let approval = synthetic_approval(&plan, 2);
        let observed = observation(&plan, 1, 102, 2);
        assert_eq!(
            reopened
                .begin(plan, Some(approval), &observed, 102)
                .unwrap_err(),
            Error::AlreadyConsumed
        );
    }

    #[test]
    fn unresolved_route_blocks_new_operation_grant_target_and_restart() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "unknown-first");
        let observed = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&observed, 101).unwrap();
        attempt
            .record_write_response(WriteObservation::OutcomeUnknown, None, 101)
            .unwrap();
        assert_eq!(
            attempt.dispatch_restore(102).unwrap_err(),
            Error::InvalidTransition
        );
        drop(attempt);
        let reopened = Journal::open(journal.filesystem_root().unwrap()).unwrap();
        let mut next = input("new-operation");
        next.route.target = "another-synthetic-target".into();
        let next = Plan::new(next).unwrap();
        let approval = synthetic_approval(&next, 2);
        let observed = observation(&next, 1, 102, 1);
        assert_eq!(
            reopened
                .begin(next, Some(approval), &observed, 102)
                .unwrap_err(),
            Error::RouteUnresolved
        );
        assert_eq!(
            reopened.recovered_status("unknown-first"),
            Status::OutcomeUnknown
        );
    }

    #[test]
    fn restore_requires_exact_new_readback_and_cannot_skip_stages() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "restore-sequence");
        assert_eq!(
            attempt.dispatch_restore(101).unwrap_err(),
            Error::InvalidTransition
        );
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        assert_eq!(
            attempt.dispatch_restore(101).unwrap_err(),
            Error::InvalidTransition
        );
        confirm(&mut attempt, 101);
        assert_eq!(
            attempt.dispatch_restore(101).unwrap_err(),
            Error::InvalidTransition
        );
        let changed = observation(&attempt.plan, 2, 102, 1);
        assert_eq!(
            attempt.record_desired_readback(&changed, 102).unwrap_err(),
            Error::ObservationMismatch
        );
        assert_eq!(attempt.status(), Status::OutcomeUnknown);
        assert_eq!(
            attempt.dispatch_restore(102).unwrap_err(),
            Error::InvalidTransition
        );
    }

    #[test]
    fn exact_restore_persists_completion_and_allows_a_new_authorized_operation() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "success");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        confirm(&mut attempt, 101);
        let changed = observation(&attempt.plan, 2, 102, 3);
        attempt.record_desired_readback(&changed, 102).unwrap();
        assert_eq!(attempt.status(), Status::RestoreReady);
        let restored = attempt
            .dispatch_restore(103)
            .unwrap()
            .bind_for_execution(&attempt.plan)
            .unwrap();
        assert_eq!(restored.bytes(), [1]);
        confirm(&mut attempt, 103);
        let restored = observation(&attempt.plan, 1, 104, 5);
        attempt.record_restored_readback(&restored, 104).unwrap();
        assert_eq!(attempt.status(), Status::RestoredVerified);
        assert_eq!(
            journal.recovered_status("success"),
            Status::RestoredVerified
        );
        // Verified property state does not release the commissioning path
        // until independent device and gateway cleanup has been confirmed.
        let blocked = Plan::new(input("before-cleanup")).unwrap();
        let approval = synthetic_approval(&blocked, 2);
        let observed = observation(&blocked, 1, 105, 1);
        assert_eq!(
            journal
                .begin(blocked, Some(approval), &observed, 105)
                .unwrap_err(),
            Error::RouteUnresolved
        );
        attempt
            .release_after_cleanup(
                CleanupReceipt {
                    plan_digest: attempt.plan.digest,
                },
                105,
            )
            .unwrap();
        let plan = Plan::new(input("next-success")).unwrap();
        let approval = synthetic_approval(&plan, 2);
        let observed = observation(&plan, 1, 105, 1);
        assert!(journal.begin(plan, Some(approval), &observed, 105).is_ok());
    }

    #[test]
    fn prewrite_observation_cannot_be_reused_as_restore_proof() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "stale-restore");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        confirm(&mut attempt, 101);
        let changed = observation(&attempt.plan, 2, 102, 3);
        attempt.record_desired_readback(&changed, 102).unwrap();
        attempt.dispatch_restore(103).unwrap();
        confirm(&mut attempt, 103);
        assert_eq!(
            attempt
                .record_restored_readback(&original, 104)
                .unwrap_err(),
            Error::ObservationMismatch
        );
        assert_eq!(
            journal.recovered_status("stale-restore"),
            Status::OutcomeUnknown
        );
    }

    #[test]
    fn failed_evidence_write_prevents_restore_and_retains_quarantine() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "evidence-failure");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        confirm(&mut attempt, 101);
        let path = journal
            .path(&attempt.operation, "desired-readback")
            .unwrap();
        fs::write(path, b"untrusted existing evidence").unwrap();
        let changed = observation(&attempt.plan, 2, 102, 3);
        assert_eq!(
            attempt.record_desired_readback(&changed, 102).unwrap_err(),
            Error::AlreadyConsumed
        );
        assert_eq!(
            attempt.dispatch_restore(103).unwrap_err(),
            Error::InvalidTransition
        );
        assert_eq!(
            journal.recovered_status("evidence-failure"),
            Status::OutcomeUnknown
        );
    }

    #[test]
    fn partial_or_mismatched_recovery_records_remain_unknown() {
        let (_directory, journal) = journal();
        let attempt = begin(&journal, "torn-result");
        fs::write(
            journal.path(&attempt.operation, "complete").unwrap(),
            b"{\"schema\":1",
        )
        .unwrap();
        assert_eq!(
            journal.recovered_status("torn-result"),
            Status::OutcomeUnknown
        );
        fs::write(
            journal.path(&attempt.operation, "complete").unwrap(),
            json(&Record {
                schema: 1,
                plan_digest: [9; 32],
                state: "RESTORED_VERIFIED".into(),
                at_utc_seconds: 105,
            })
            .unwrap(),
        )
        .unwrap();
        assert_eq!(
            journal.recovered_status("torn-result"),
            Status::OutcomeUnknown
        );
    }

    #[test]
    fn start_expiry_does_not_interrupt_already_started_exact_restoration() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "expiry-during-recovery");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        confirm(&mut attempt, 101);
        let changed = observation(&attempt.plan, 2, 199, 3);
        attempt.record_desired_readback(&changed, 199).unwrap();
        assert_eq!(
            attempt
                .dispatch_restore(201)
                .unwrap()
                .bind_for_execution(&attempt.plan)
                .unwrap()
                .bytes(),
            [1]
        );
        confirm(&mut attempt, 201);
        let restored = observation(&attempt.plan, 1, 202, 5);
        attempt.record_restored_readback(&restored, 202).unwrap();
        assert_eq!(attempt.status(), Status::RestoredVerified);
    }

    #[test]
    fn concurrent_operations_cannot_both_reserve_the_same_bus_path() {
        let (_directory, journal) = journal();
        let barrier = std::sync::Arc::new(std::sync::Barrier::new(2));
        let workers: Vec<_> = (1..=2)
            .map(|id| {
                let journal = journal.clone();
                let barrier = barrier.clone();
                std::thread::spawn(move || {
                    let plan = Plan::new(input(&format!("concurrent-{id}"))).unwrap();
                    let approval = synthetic_approval(&plan, id);
                    let observed = observation(&plan, 1, 100, 1);
                    barrier.wait();
                    journal.begin(plan, Some(approval), &observed, 100)
                })
            })
            .collect();
        let outcomes: Vec<_> = workers.into_iter().map(|w| w.join().unwrap()).collect();
        assert_eq!(outcomes.iter().filter(|r| r.is_ok()).count(), 1);
        assert_eq!(
            outcomes
                .iter()
                .filter(|r| matches!(r, Err(Error::RouteUnresolved)))
                .count(),
            1
        );
    }

    #[test]
    fn invalid_or_unbounded_plan_is_rejected() {
        let mut plan = input("invalid");
        plan.desired = vec![3; MAX_VALUE_BYTES + 1];
        assert_eq!(Plan::new(plan).unwrap_err(), Error::InvalidPlan);
        let mut plan = input("invalid");
        plan.original = plan.desired.clone();
        assert_eq!(Plan::new(plan).unwrap_err(), Error::InvalidPlan);
    }

    #[test]
    fn dispatch_binding_rejects_changed_target_selector_bytes_digest_or_phase() {
        // These direct field mutations are possible only inside this private
        // child test module. They model a corrupted future conversion boundary;
        // external callers have no field access, constructor or deserializer.
        for case in 0..5 {
            let (_directory, journal) = journal();
            let mut attempt = begin(&journal, "dispatch-boundary");
            let observed = observation(&attempt.plan, 1, 101, 1);
            let mut selection = attempt.dispatch_desired(&observed, 101).unwrap();
            match case {
                0 => selection.route.target.push('x'),
                1 => selection.selector.property += 1,
                2 => selection.bytes[0] = 3,
                3 => selection.plan_digest[0] ^= 1,
                _ => selection.phase = DispatchPhase::Restore,
            }
            assert_eq!(
                selection.bind_for_execution(&attempt.plan).unwrap_err(),
                Error::DispatchMismatch
            );
            assert_eq!(attempt.status(), Status::DesiredDispatched);
            assert_eq!(
                journal.recovered_status("dispatch-boundary"),
                Status::OutcomeUnknown
            );
        }
    }

    #[test]
    fn dispatch_binding_rejects_another_validated_plan() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "dispatch-plan");
        let observed = observation(&attempt.plan, 1, 101, 1);
        let selection = attempt.dispatch_desired(&observed, 101).unwrap();
        let mut other = input("dispatch-plan");
        other.desired[0] = 3;
        let other = Plan::new(other).unwrap();
        assert_eq!(
            selection.bind_for_execution(&other).unwrap_err(),
            Error::DispatchMismatch
        );
    }

    #[test]
    fn missing_trusted_observation_watermark_leaves_write_unknown() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "missing-watermark");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        assert_eq!(
            attempt
                .record_write_response(WriteObservation::ResponseConfirmed, None, 101)
                .unwrap_err(),
            Error::ObservationAuthorityUnavailable
        );
        assert_eq!(attempt.status(), Status::OutcomeUnknown);
        let changed = observation(&attempt.plan, 2, 102, 3);
        assert_eq!(
            attempt.record_desired_readback(&changed, 102).unwrap_err(),
            Error::InvalidTransition
        );
        assert_eq!(
            journal.recovered_status("missing-watermark"),
            Status::OutcomeUnknown
        );
    }

    #[test]
    fn same_second_pre_restore_sample_cannot_pass_the_post_response_watermark() {
        let (_directory, journal) = journal();
        let mut attempt = begin(&journal, "same-second");
        let original = observation(&attempt.plan, 1, 101, 1);
        attempt.dispatch_desired(&original, 101).unwrap();
        confirm(&mut attempt, 101);
        let changed = observation(&attempt.plan, 2, 102, 3);
        attempt.record_desired_readback(&changed, 102).unwrap();
        // This original-value sample predates restore dispatch but shares its
        // UTC second. The trusted adapter's post-response watermark includes it.
        let pre_restore = observation(&attempt.plan, 1, 103, 4);
        attempt.dispatch_restore(103).unwrap();
        confirm(&mut attempt, 103);
        assert_eq!(
            attempt
                .record_restored_readback(&pre_restore, 103)
                .unwrap_err(),
            Error::ObservationMismatch
        );
        assert_eq!(attempt.status(), Status::OutcomeUnknown);
        assert_eq!(
            journal.recovered_status("same-second"),
            Status::OutcomeUnknown
        );
    }
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Selector {
    pub object: u8,
    pub property: u8,
    pub index: u16,
    pub count: u8,
    pub element_size: u16,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct PlanInput {
    pub operation_id: String,
    pub route: Route,
    pub selector: Selector,
    pub original: Vec<u8>,
    pub desired: Vec<u8>,
    pub purpose: String,
    pub recovery: String,
    pub expires_at_utc_seconds: u64,
}

/// Validated owned snapshot. No mutable access to the input is exposed.
#[derive(Debug)]
pub struct Plan {
    input: PlanInput,
    digest: [u8; 32],
}

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    InvalidPlan,
    ApprovalUnavailable,
    ApprovalMismatch,
    Expired,
    ObservationMismatch,
    StaleObservation,
    ObservationAuthorityUnavailable,
    AlreadyConsumed,
    RouteUnresolved,
    InvalidTransition,
    DispatchMismatch,
    UnsafeJournalPath,
    Io,
}

fn bounded_text(value: &str, max: usize) -> bool {
    !value.is_empty()
        && value.len() <= max
        && value.bytes().all(|b| b.is_ascii_graphic() || b == b' ')
}

fn digest(bytes: &[u8]) -> [u8; 32] {
    Sha256::digest(bytes).into()
}

fn json<T: Serialize>(value: &T) -> Result<Vec<u8>, Error> {
    let bytes = serde_json::to_vec(value).map_err(|_| Error::InvalidPlan)?;
    if bytes.len() as u64 > MAX_RECORD_BYTES {
        return Err(Error::InvalidPlan);
    }
    Ok(bytes)
}

fn hex(value: &[u8]) -> String {
    const DIGITS: &[u8; 16] = b"0123456789abcdef";
    let mut text = String::with_capacity(value.len() * 2);
    for byte in value {
        text.push(char::from(DIGITS[usize::from(byte >> 4)]));
        text.push(char::from(DIGITS[usize::from(byte & 15)]));
    }
    text
}

impl Plan {
    pub fn new(input: PlanInput) -> Result<Self, Error> {
        let value_length = usize::from(input.selector.count)
            .checked_mul(usize::from(input.selector.element_size))
            .ok_or(Error::InvalidPlan)?;
        if !bounded_text(&input.operation_id, 128)
            || !bounded_text(&input.route.gateway, 256)
            || !bounded_text(&input.route.bus_path, 128)
            || !bounded_text(&input.route.target, 128)
            || input.route.identity == [0; 32]
            || !bounded_text(&input.purpose, 1_024)
            || !bounded_text(&input.recovery, 1_024)
            || value_length == 0
            || value_length > MAX_VALUE_BYTES
            || input.original.len() != value_length
            || input.desired.len() != value_length
            || input.original == input.desired
            || input.expires_at_utc_seconds == 0
        {
            return Err(Error::InvalidPlan);
        }
        let hash = digest(&json(&input)?);
        Ok(Self {
            input,
            digest: hash,
        })
    }

    pub fn digest(&self) -> [u8; 32] {
        self.digest
    }
}

/// An authenticated operator grant. There is deliberately no public constructor,
/// deserializer or agent-accessible minting API in this offline increment.
pub struct HumanApproval {
    id: [u8; 16],
    plan_digest: [u8; 32],
    expires_at_utc_seconds: u64,
}

/// Synthetic fixture access exists only in a test build. It is not an issuer,
/// credential verifier or executable capability available to shipped callers.
#[cfg(test)]
pub(crate) fn synthetic_approval_for_test(
    id: [u8; 16],
    plan_digest: [u8; 32],
    expires_at_utc_seconds: u64,
) -> HumanApproval {
    HumanApproval {
        id,
        plan_digest,
        expires_at_utc_seconds,
    }
}

#[derive(Debug)]
pub struct Observation {
    route: Route,
    selector: Selector,
    value: Vec<u8>,
    observed_at_utc_seconds: u64,
    /// Monotonic receipt number from the future trusted execution adapter.
    /// This offline model does not authenticate the supplied observation.
    receipt: u64,
}

/// A trusted adapter's monotonic event fence captured after a confirmed write
/// response. No constructor/deserializer is exposed by this offline increment.
/// Capturing a fence before dispatch is insufficient: a sample could arrive
/// between that fence and the actual send.
#[derive(Debug)]
pub struct ObservationWatermark {
    plan_digest: [u8; 32],
    receipt: u64,
    confirmed_at_utc_seconds: u64,
}

/// Internal adapter evidence that both device and gateway cleanup have been
/// confirmed. No public constructor or deserializer can release quarantine.
struct CleanupReceipt {
    plan_digest: [u8; 32],
}

fn validate_observation(
    plan: &Plan,
    observation: &Observation,
    expected: &[u8],
    now: u64,
) -> Result<(), Error> {
    if observation.route != plan.input.route
        || observation.selector != plan.input.selector
        || observation.value != expected
    {
        return Err(Error::ObservationMismatch);
    }
    if observation.observed_at_utc_seconds > now
        || now - observation.observed_at_utc_seconds > MAX_OBSERVATION_AGE_SECONDS
    {
        return Err(Error::StaleObservation);
    }
    Ok(())
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Status {
    DesiredReady,
    DesiredDispatched,
    AwaitDesiredReadback,
    RestoreReady,
    RestoreDispatched,
    AwaitRestoreReadback,
    RestoredVerified,
    OutcomeUnknown,
}

#[derive(Debug, Clone, Copy)]
pub enum WriteObservation {
    ResponseConfirmed,
    OutcomeUnknown,
}

#[derive(Debug, PartialEq, Eq)]
enum DispatchPhase {
    Desired,
    Restore,
}

/// Opaque one-use selection returned only after durable intent. It grants no
/// permission beyond the current in-memory attempt, and contains no I/O API.
/// A future dispatcher must consume it with the independently validated plan.
#[derive(Debug)]
pub struct DispatchSelection {
    route: Route,
    selector: Selector,
    bytes: Vec<u8>,
    plan_digest: [u8; 32],
    phase: DispatchPhase,
}

impl DispatchSelection {
    /// Consume the opaque selection and rebind every field to the exact plan.
    /// No mutable access, deserialization or cloning API is provided.
    pub fn bind_for_execution(self, plan: &Plan) -> Result<BoundDispatch, Error> {
        let expected = match self.phase {
            DispatchPhase::Desired => &plan.input.desired,
            DispatchPhase::Restore => &plan.input.original,
        };
        if self.plan_digest != plan.digest
            || self.route != plan.input.route
            || self.selector != plan.input.selector
            || self.bytes != *expected
        {
            return Err(Error::DispatchMismatch);
        }
        Ok(BoundDispatch { selection: self })
    }
}

/// Immutable view for a future consuming typed dispatcher. This is not an
/// executor or a trusted-human approval adapter and performs no I/O.
#[derive(Debug)]
pub struct BoundDispatch {
    selection: DispatchSelection,
}

impl BoundDispatch {
    pub fn plan_digest(&self) -> [u8; 32] {
        self.selection.plan_digest
    }

    pub fn route(&self) -> &Route {
        &self.selection.route
    }

    pub fn selector(&self) -> &Selector {
        &self.selection.selector
    }

    pub fn bytes(&self) -> &[u8] {
        &self.selection.bytes
    }
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Intent {
    schema: u8,
    plan: PlanInput,
    plan_digest: [u8; 32],
    approval_id: [u8; 16],
    consumed_at_utc_seconds: u64,
    physical_state: String,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Record {
    schema: u8,
    plan_digest: [u8; 32],
    state: String,
    at_utc_seconds: u64,
}

/// Exact immutable keys supplied only after the guard validates approval and
/// the fresh initial observation. These identifiers are not an approval token.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct JournalBinding {
    route: [u8; 32],
    operation: [u8; 32],
    plan: [u8; 32],
    approval: [u8; 16],
}

impl JournalBinding {
    pub fn route(&self) -> [u8; 32] {
        self.route
    }
    pub fn operation(&self) -> [u8; 32] {
        self.operation
    }
    pub fn plan(&self) -> [u8; 32] {
        self.plan
    }
    pub fn approval(&self) -> [u8; 16] {
        self.approval
    }
}

/// Storage keys for facts already validated by the existing guard. A backend
/// does not independently evaluate observations or select a restoration.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum JournalRecord {
    DesiredReadback,
    RestoreIntent,
    RestoredReadback,
    CleanupConfirmed,
}

impl JournalRecord {
    fn labels(self) -> (&'static str, &'static str) {
        match self {
            Self::DesiredReadback => ("desired-readback", "DESIRED_READBACK_VERIFIED"),
            Self::RestoreIntent => ("restore-intent", "RESTORE_OUTCOME_UNKNOWN"),
            Self::RestoredReadback => ("complete", "RESTORED_VERIFIED"),
            Self::CleanupConfirmed => ("cleanup", "DEVICE_AND_GATEWAY_CLEANUP_CONFIRMED"),
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum PersistenceError {
    AlreadyConsumed,
    RouteUnresolved,
    Unavailable,
}

impl From<PersistenceError> for Error {
    fn from(value: PersistenceError) -> Self {
        match value {
            PersistenceError::AlreadyConsumed => Self::AlreadyConsumed,
            PersistenceError::RouteUnresolved => Self::RouteUnresolved,
            PersistenceError::Unavailable => Self::Io,
        }
    }
}

/// Trusted synchronous storage contract, not an authorization or observation
/// provider. Native adapters require independent qualification.
///
/// Reserve must atomically exclude cooperating operations for the gateway/bus
/// route, globally consume operation/approval keys, and persist the complete
/// exact intent before returning success. Failure after a possible reservation
/// retains unknown/quarantined state. Append is create-new and success requires
/// the record and its namespace metadata to be durable. Reads enforce the
/// requested limit before allocation. Release verifies the exact binding and
/// durable cleanup evidence under route exclusion; uncertain release cannot
/// report a free route without a successful durability reconciliation.
///
/// Full intent records can be 16KiB; a digest-only or 2KiB store cannot implement
/// this contract by truncating or omitting plan bytes. Keep blocking calls off
/// async executor threads. Arbitrary same-principal mutation is outside scope.
pub trait JournalPersistence: std::fmt::Debug + Send + Sync {
    fn preflight(&self) -> Result<(), PersistenceError>;
    fn reserve(&self, binding: &JournalBinding, intent: &[u8]) -> Result<(), PersistenceError>;
    fn append(
        &self,
        binding: &JournalBinding,
        kind: JournalRecord,
        record: &[u8],
    ) -> Result<(), PersistenceError>;
    fn read_intent(
        &self,
        operation: [u8; 32],
        limit: usize,
    ) -> Result<Option<Vec<u8>>, PersistenceError>;
    fn read_record(
        &self,
        operation: [u8; 32],
        kind: JournalRecord,
        limit: usize,
    ) -> Result<Option<Vec<u8>>, PersistenceError>;
    fn release(&self, binding: &JournalBinding) -> Result<(), PersistenceError>;
}

#[derive(Debug, Clone)]
enum JournalBackend {
    Filesystem(PathBuf),
    External(Arc<dyn JournalPersistence>),
}

/// The existing filesystem backend remains the default. An explicitly supplied
/// trusted backend replaces persistence only; all guard validation is shared.
#[derive(Debug, Clone)]
pub struct Journal {
    backend: JournalBackend,
}

impl Journal {
    pub fn open(root: &Path) -> Result<Self, Error> {
        if !root.is_absolute()
            || fs::canonicalize(root).map_err(|_| Error::UnsafeJournalPath)? != root
            || !fs::symlink_metadata(root)
                .map_err(|_| Error::UnsafeJournalPath)?
                .is_dir()
        {
            return Err(Error::UnsafeJournalPath);
        }
        // Failure to synchronously flush directories prevents dispatch.
        File::open(root)
            .and_then(|directory| directory.sync_all())
            .map_err(|_| Error::Io)?;
        Ok(Self {
            backend: JournalBackend::Filesystem(root.into()),
        })
    }

    pub fn with_persistence(persistence: Arc<dyn JournalPersistence>) -> Result<Self, Error> {
        persistence.preflight()?;
        Ok(Self {
            backend: JournalBackend::External(persistence),
        })
    }

    fn filesystem_root(&self) -> Result<&Path, Error> {
        match &self.backend {
            JournalBackend::Filesystem(root) => Ok(root),
            JournalBackend::External(_) => Err(Error::UnsafeJournalPath),
        }
    }

    fn path(&self, key: &[u8], suffix: &str) -> Result<PathBuf, Error> {
        Ok(self
            .filesystem_root()?
            .join(format!("{}-{suffix}.json", hex(key))))
    }

    fn write_new<T: Serialize>(&self, path: &Path, value: &T) -> Result<(), Error> {
        let bytes = json(value)?;
        let mut file = OpenOptions::new()
            .write(true)
            .create_new(true)
            .open(path)
            .map_err(|error| {
                if error.kind() == std::io::ErrorKind::AlreadyExists {
                    Error::AlreadyConsumed
                } else {
                    Error::Io
                }
            })?;
        file.write_all(&bytes).map_err(|_| Error::Io)?;
        file.sync_all().map_err(|_| Error::Io)?;
        File::open(self.filesystem_root()?)
            .and_then(|directory| directory.sync_all())
            .map_err(|_| Error::Io)
    }

    fn operation_key(plan: &Plan) -> [u8; 32] {
        digest(plan.input.operation_id.as_bytes())
    }

    fn route_key(plan: &Plan) -> Result<[u8; 32], Error> {
        // One commissioning operation per gateway/bus path, even across targets.
        Ok(digest(&json(&(
            &plan.input.route.gateway,
            &plan.input.route.bus_path,
        ))?))
    }

    pub fn begin(
        &self,
        plan: Plan,
        approval: Option<HumanApproval>,
        observation: &Observation,
        now: u64,
    ) -> Result<Attempt, Error> {
        let approval = approval.ok_or(Error::ApprovalUnavailable)?;
        if approval.plan_digest != plan.digest || approval.id == [0; 16] {
            return Err(Error::ApprovalMismatch);
        }
        if now >= plan.input.expires_at_utc_seconds || now >= approval.expires_at_utc_seconds {
            return Err(Error::Expired);
        }
        validate_observation(&plan, observation, &plan.input.original, now)?;
        let operation = Self::operation_key(&plan);
        let binding = JournalBinding {
            route: Self::route_key(&plan)?,
            operation,
            plan: plan.digest,
            approval: approval.id,
        };
        let intent = Intent {
            schema: 1,
            plan: plan.input.clone(),
            plan_digest: plan.digest,
            approval_id: approval.id,
            consumed_at_utc_seconds: now,
            physical_state: "UNKNOWN_UNTIL_VERIFIED_RESTORED".into(),
        };
        if let JournalBackend::External(persistence) = &self.backend {
            persistence.reserve(&binding, &json(&intent)?)?;
        } else {
            let intent_path = self.path(&operation, "intent")?;
            let grant_path = self.path(&approval.id, "consumed-grant")?;
            if intent_path.exists() || grant_path.exists() {
                return Err(Error::AlreadyConsumed);
            }
            let route_path = self.path(&binding.route, "unresolved-route")?;
            self.write_new(&route_path, &hex(&plan.digest))
                .map_err(|error| {
                    if error == Error::AlreadyConsumed {
                        Error::RouteUnresolved
                    } else {
                        error
                    }
                })?;
            // Every failure after reserving the route leaves it quarantined. A
            // new grant/operation/process cannot silently clear uncertain state.
            self.write_new(&grant_path, &hex(&plan.digest))?;
            self.write_new(&intent_path, &intent)?;
        }
        Ok(Attempt {
            journal: self.clone(),
            plan,
            operation,
            binding,
            status: Status::DesiredReady,
            expires_at_start: approval.expires_at_utc_seconds,
            last_receipt: observation.receipt,
            last_verified_at: observation.observed_at_utc_seconds,
            dispatch_at: None,
        })
    }

    /// Missing, partial or unrecognized records conservatively remain unknown.
    pub fn recovered_status(&self, operation_id: &str) -> Status {
        let key = digest(operation_id.as_bytes());
        let intent: Intent = match self.read_intent(key) {
            Ok(record) => record,
            Err(_) => return Status::OutcomeUnknown,
        };
        let Ok(plan) = Plan::new(intent.plan) else {
            return Status::OutcomeUnknown;
        };
        if intent.schema != 1
            || intent.plan_digest != plan.digest
            || Self::operation_key(&plan) != key
        {
            return Status::OutcomeUnknown;
        }
        match self.read_completion(key) {
            Ok(record)
                if record.schema == 1
                    && record.plan_digest == plan.digest
                    && record.state == "RESTORED_VERIFIED" =>
            {
                Status::RestoredVerified
            }
            _ => Status::OutcomeUnknown,
        }
    }

    fn read_intent(&self, operation: [u8; 32]) -> Result<Intent, Error> {
        match &self.backend {
            JournalBackend::Filesystem(_) => read_record(&self.path(&operation, "intent")?),
            JournalBackend::External(persistence) => {
                decode_persisted(persistence.read_intent(operation, MAX_JOURNAL_RECORD_BYTES)?)
            }
        }
    }

    fn read_completion(&self, operation: [u8; 32]) -> Result<Record, Error> {
        match &self.backend {
            JournalBackend::Filesystem(_) => read_record(&self.path(&operation, "complete")?),
            JournalBackend::External(persistence) => decode_persisted(persistence.read_record(
                operation,
                JournalRecord::RestoredReadback,
                MAX_JOURNAL_RECORD_BYTES,
            )?),
        }
    }
}

fn decode_persisted<T: for<'de> Deserialize<'de>>(bytes: Option<Vec<u8>>) -> Result<T, Error> {
    let bytes = bytes.ok_or(Error::Io)?;
    if bytes.is_empty() || bytes.len() > MAX_JOURNAL_RECORD_BYTES {
        return Err(Error::Io);
    }
    serde_json::from_slice(&bytes).map_err(|_| Error::Io)
}

fn read_record<T: for<'de> Deserialize<'de>>(path: &Path) -> Result<T, Error> {
    let metadata = fs::symlink_metadata(path).map_err(|_| Error::Io)?;
    if !metadata.is_file() || metadata.len() > MAX_RECORD_BYTES {
        return Err(Error::Io);
    }
    let mut bytes = Vec::new();
    File::open(path)
        .map_err(|_| Error::Io)?
        .take(MAX_RECORD_BYTES + 1)
        .read_to_end(&mut bytes)
        .map_err(|_| Error::Io)?;
    if bytes.len() as u64 > MAX_RECORD_BYTES {
        return Err(Error::Io);
    }
    serde_json::from_slice(&bytes).map_err(|_| Error::Io)
}

#[derive(Debug)]
pub struct Attempt {
    journal: Journal,
    plan: Plan,
    operation: [u8; 32],
    binding: JournalBinding,
    status: Status,
    expires_at_start: u64,
    last_receipt: u64,
    last_verified_at: u64,
    dispatch_at: Option<u64>,
}

impl Attempt {
    pub fn status(&self) -> Status {
        self.status
    }

    pub fn dispatch_desired(
        &mut self,
        observation: &Observation,
        now: u64,
    ) -> Result<DispatchSelection, Error> {
        if self.status != Status::DesiredReady {
            return Err(Error::InvalidTransition);
        }
        self.status = Status::OutcomeUnknown;
        if now >= self.expires_at_start || now >= self.plan.input.expires_at_utc_seconds {
            return Err(Error::Expired);
        }
        validate_observation(&self.plan, observation, &self.plan.input.original, now)?;
        if observation.receipt < self.last_receipt {
            return Err(Error::ObservationMismatch);
        }
        self.last_receipt = observation.receipt;
        self.dispatch_at = Some(now);
        self.status = Status::DesiredDispatched;
        Ok(self.selection(self.plan.input.desired.clone(), DispatchPhase::Desired))
    }

    fn selection(&self, bytes: Vec<u8>, phase: DispatchPhase) -> DispatchSelection {
        DispatchSelection {
            route: self.plan.input.route.clone(),
            selector: self.plan.input.selector.clone(),
            bytes,
            plan_digest: self.plan.digest,
            phase,
        }
    }

    pub fn record_write_response(
        &mut self,
        observation: WriteObservation,
        watermark: Option<ObservationWatermark>,
        now: u64,
    ) -> Result<(), Error> {
        if !matches!(
            self.status,
            Status::DesiredDispatched | Status::RestoreDispatched
        ) {
            return Err(Error::InvalidTransition);
        }
        if matches!(observation, WriteObservation::OutcomeUnknown) {
            self.status = Status::OutcomeUnknown;
        } else {
            let prior = self.status;
            self.status = Status::OutcomeUnknown;
            let watermark = watermark.ok_or(Error::ObservationAuthorityUnavailable)?;
            if watermark.plan_digest != self.plan.digest
                || watermark.receipt < self.last_receipt
                || self
                    .dispatch_at
                    .is_none_or(|at| watermark.confirmed_at_utc_seconds < at)
                || watermark.confirmed_at_utc_seconds > now
                || now - watermark.confirmed_at_utc_seconds > MAX_OBSERVATION_AGE_SECONDS
            {
                return Err(Error::ObservationMismatch);
            }
            self.last_receipt = watermark.receipt;
            self.dispatch_at = Some(watermark.confirmed_at_utc_seconds);
            self.status = if prior == Status::DesiredDispatched {
                Status::AwaitDesiredReadback
            } else {
                Status::AwaitRestoreReadback
            };
        }
        Ok(())
    }

    pub fn record_desired_readback(
        &mut self,
        observation: &Observation,
        now: u64,
    ) -> Result<(), Error> {
        if self.status != Status::AwaitDesiredReadback {
            return Err(Error::InvalidTransition);
        }
        self.status = Status::OutcomeUnknown;
        validate_observation(&self.plan, observation, &self.plan.input.desired, now)?;
        self.validate_new_readback(observation)?;
        self.record(JournalRecord::DesiredReadback, now)?;
        self.last_receipt = observation.receipt;
        self.last_verified_at = observation.observed_at_utc_seconds;
        self.status = Status::RestoreReady;
        Ok(())
    }

    pub fn dispatch_restore(&mut self, now: u64) -> Result<DispatchSelection, Error> {
        if self.status != Status::RestoreReady {
            return Err(Error::InvalidTransition);
        }
        self.status = Status::OutcomeUnknown;
        if now < self.last_verified_at || now - self.last_verified_at > MAX_OBSERVATION_AGE_SECONDS
        {
            return Err(Error::StaleObservation);
        }
        self.record(JournalRecord::RestoreIntent, now)?;
        self.dispatch_at = Some(now);
        self.status = Status::RestoreDispatched;
        Ok(self.selection(self.plan.input.original.clone(), DispatchPhase::Restore))
    }

    pub fn record_restored_readback(
        &mut self,
        observation: &Observation,
        now: u64,
    ) -> Result<(), Error> {
        if self.status != Status::AwaitRestoreReadback {
            return Err(Error::InvalidTransition);
        }
        self.status = Status::OutcomeUnknown;
        validate_observation(&self.plan, observation, &self.plan.input.original, now)?;
        self.validate_new_readback(observation)?;
        self.record(JournalRecord::RestoredReadback, now)?;
        self.status = Status::RestoredVerified;
        Ok(())
    }

    fn release_after_cleanup(&mut self, receipt: CleanupReceipt, now: u64) -> Result<(), Error> {
        if self.status != Status::RestoredVerified || receipt.plan_digest != self.plan.digest {
            return Err(Error::InvalidTransition);
        }
        self.record(JournalRecord::CleanupConfirmed, now)?;
        if let JournalBackend::External(persistence) = &self.journal.backend {
            return persistence.release(&self.binding).map_err(Error::from);
        }
        fs::remove_file(self.journal.path(&self.binding.route, "unresolved-route")?)
            .map_err(|_| Error::Io)?;
        File::open(self.journal.filesystem_root()?)
            .and_then(|directory| directory.sync_all())
            .map_err(|_| Error::Io)
    }

    fn record(&self, kind: JournalRecord, now: u64) -> Result<(), Error> {
        let (suffix, state) = kind.labels();
        let record = Record {
            schema: 1,
            plan_digest: self.plan.digest,
            state: state.into(),
            at_utc_seconds: now,
        };
        match &self.journal.backend {
            JournalBackend::Filesystem(_) => self
                .journal
                .write_new(&self.journal.path(&self.operation, suffix)?, &record),
            JournalBackend::External(persistence) => persistence
                .append(&self.binding, kind, &json(&record)?)
                .map_err(Error::from),
        }
    }

    fn validate_new_readback(&self, observation: &Observation) -> Result<(), Error> {
        if observation.receipt <= self.last_receipt
            || self
                .dispatch_at
                .is_none_or(|at| observation.observed_at_utc_seconds < at)
        {
            return Err(Error::ObservationMismatch);
        }
        Ok(())
    }
}
