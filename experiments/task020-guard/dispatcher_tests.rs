// SPDX-License-Identifier: Apache-2.0
// All sessions, values, approvals and observations in this module are synthetic.
use super::*;
use crate::guard::PlanInput;
use std::future::{Future, pending};
use std::pin::pin;
use std::task::{Context, Poll, Waker};

struct FixedClock;
impl Clock for FixedClock {
    fn now(&self) -> u64 {
        100
    }
}

fn ready<F: Future>(future: F) -> F::Output {
    let mut future = pin!(future);
    let mut context = Context::from_waker(Waker::noop());
    match future.as_mut().poll(&mut context) {
        Poll::Ready(result) => result,
        Poll::Pending => panic!("synthetic immediate session unexpectedly pending"),
    }
}

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
        purpose: "synthetic write/read/restore".into(),
        recovery: "synthetic exact-byte restoration".into(),
        expires_at_utc_seconds: 200,
    }
}

fn approved(plan: &Plan, id: u8) -> HumanApproval {
    HumanApproval {
        id: [id; 16],
        plan_digest: plan.digest(),
        expires_at_utc_seconds: 200,
    }
}

fn journal() -> (tempfile::TempDir, Journal) {
    let dir = tempfile::tempdir().unwrap();
    let root = std::fs::canonicalize(dir.path()).unwrap();
    let journal = Journal::open(&root).unwrap();
    (dir, journal)
}

struct MockSession {
    route: Route,
    value: Vec<u8>,
    reads: usize,
    writes: Vec<Vec<u8>>,
    closes: usize,
    fail_write: Option<usize>,
    pending_write: bool,
    bad_echo: bool,
    wrong_read: Option<usize>,
    fail_read: Option<usize>,
    read_error: SessionError,
    close_fails: bool,
    close_pending: bool,
    close_evidence: CleanupEvidence,
    close_gate: Option<std::sync::Arc<std::sync::atomic::AtomicBool>>,
}

impl MockSession {
    fn new(plan: &Plan) -> Self {
        Self {
            route: plan.input.route.clone(),
            value: plan.input.original.clone(),
            reads: 0,
            writes: Vec::new(),
            closes: 0,
            fail_write: None,
            pending_write: false,
            bad_echo: false,
            wrong_read: None,
            fail_read: None,
            read_error: SessionError::Uncertain,
            close_fails: false,
            close_pending: false,
            close_evidence: CleanupEvidence::DeviceAndGatewayConfirmed,
            close_gate: None,
        }
    }
}

impl TypedSession for MockSession {
    fn route(&self) -> &Route {
        &self.route
    }
    async fn observe(&mut self, selector: &Selector) -> Result<SessionRead, SessionError> {
        self.reads += 1;
        if self.fail_read == Some(self.reads) {
            return Err(self.read_error);
        }
        Ok(SessionRead {
            route: self.route.clone(),
            selector: selector.clone(),
            value: if self.wrong_read == Some(self.reads) {
                vec![99]
            } else {
                self.value.clone()
            },
        })
    }
    async fn write(&mut self, dispatch: BoundDispatch) -> Result<SessionRead, SessionError> {
        assert_eq!(dispatch.route(), &self.route);
        let selector = dispatch.selector().clone();
        let bytes = dispatch.bytes().to_vec();
        self.writes.push(bytes.clone());
        self.value = bytes.clone(); // A failed response may follow an actual effect.
        if self.pending_write {
            pending::<()>().await;
        }
        if self.fail_write == Some(self.writes.len()) {
            return Err(SessionError::Uncertain);
        }
        Ok(SessionRead {
            route: self.route.clone(),
            selector,
            value: if self.bad_echo { vec![99] } else { bytes },
        })
    }
    async fn close(&mut self) -> Result<CleanupEvidence, SessionError> {
        self.closes += 1;
        if self.close_pending {
            pending::<()>().await;
        }
        if let Some(gate) = &self.close_gate {
            std::future::poll_fn(|_| {
                if gate.load(std::sync::atomic::Ordering::SeqCst) {
                    Poll::Ready(())
                } else {
                    Poll::Pending
                }
            })
            .await;
        }
        if self.close_fails {
            Err(SessionError::Uncertain)
        } else {
            Ok(self.close_evidence)
        }
    }
}

#[test]
fn missing_or_altered_approval_never_invokes_session() {
    for altered in [false, true] {
        let (_dir, journal) = journal();
        let plan = Plan::new(input("not-approved")).unwrap();
        let mut session = MockSession::new(&plan);
        let approval = if altered {
            let mut a = approved(&plan, 1);
            a.plan_digest[0] ^= 1;
            Some(a)
        } else {
            None
        };
        let report = ready(execute(&journal, plan, approval, &mut session, &FixedClock));
        assert!(!report.succeeded());
        assert_eq!(session.reads, 0);
        assert!(session.writes.is_empty());
        assert_eq!(session.closes, 0);
        assert_eq!(report.cleanup, None);
        assert_eq!(
            std::fs::read_dir(journal.filesystem_root().unwrap())
                .unwrap()
                .count(),
            0
        );
    }
}

#[test]
fn wrong_session_route_never_observes_or_dispatches() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("wrong-target")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.route.target.push('x');
    let report = ready(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(Error::ObservationMismatch))
    );
    assert_eq!(session.reads, 0);
    assert!(session.writes.is_empty());
    assert_eq!(session.closes, 0);
}

#[test]
fn exact_write_readback_restore_sequence_is_durable_and_cleanup_is_separate() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("complete-dispatch")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    let report = ready(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert!(report.succeeded());
    assert_eq!(session.writes, vec![vec![2], vec![1]]);
    assert_eq!(session.reads, 4);
    assert_eq!(session.closes, 1);
    assert_eq!(
        journal.recovered_status("complete-dispatch"),
        Status::RestoredVerified
    );
}

#[test]
fn uncertain_write_never_retries_or_automatically_restores() {
    for failed in [1, 2] {
        let (_dir, journal) = journal();
        let plan = Plan::new(input("uncertain-write")).unwrap();
        let approval = approved(&plan, 1);
        let mut session = MockSession::new(&plan);
        session.fail_write = Some(failed);
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert_eq!(
            report.operation,
            Err(DispatchError::Session(SessionError::Uncertain))
        );
        assert_eq!(session.writes.len(), failed);
        assert_eq!(session.closes, 1);
        assert_eq!(
            journal.recovered_status("uncertain-write"),
            Status::OutcomeUnknown
        );
        let next = Plan::new(input("new-attempt")).unwrap();
        let approval = approved(&next, 2);
        let mut next_session = MockSession::new(&next);
        let report = ready(execute(
            &journal,
            next,
            Some(approval),
            &mut next_session,
            &FixedClock,
        ));
        assert_eq!(
            report.operation,
            Err(DispatchError::Guard(Error::RouteUnresolved))
        );
        assert!(next_session.writes.is_empty());
    }
}

#[test]
fn wrong_readback_or_echo_never_starts_restoration() {
    for echo in [false, true] {
        let (_dir, journal) = journal();
        let plan = Plan::new(input("bad-response")).unwrap();
        let approval = approved(&plan, 1);
        let mut session = MockSession::new(&plan);
        session.bad_echo = echo;
        if !echo {
            session.wrong_read = Some(3);
        }
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert!(!report.succeeded());
        assert_eq!(session.writes, vec![vec![2]]);
        assert_eq!(
            journal.recovered_status("bad-response"),
            Status::OutcomeUnknown
        );
    }
}

#[test]
fn changed_current_value_blocks_first_write_after_durable_intent() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("changed-before-send")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.wrong_read = Some(2);
    let report = ready(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(Error::ObservationMismatch))
    );
    assert!(session.writes.is_empty());
    assert_eq!(session.closes, 1);
    assert_eq!(
        journal.recovered_status("changed-before-send"),
        Status::OutcomeUnknown
    );
}

#[test]
fn cancellation_during_dispatch_keeps_durable_unknown_and_sends_no_restore() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("cancel-dispatch")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.pending_write = true;
    {
        let mut future = pin!(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock
        ));
        let mut cx = Context::from_waker(Waker::noop());
        assert!(future.as_mut().poll(&mut cx).is_pending());
    }
    assert_eq!(session.writes, vec![vec![2]]);
    assert_eq!(session.closes, 0);
    assert_eq!(
        journal.recovered_status("cancel-dispatch"),
        Status::OutcomeUnknown
    );
    // Async cleanup cannot run from a dropped future. The caller must close it.
    ready(session.close()).unwrap();
    assert_eq!(session.closes, 1);
}

#[test]
fn read_error_and_cleanup_failure_cannot_be_reported_as_complete_success() {
    for read_failure in [false, true] {
        let (_dir, journal) = journal();
        let plan = Plan::new(input("read-or-close-error")).unwrap();
        let approval = approved(&plan, 1);
        let mut session = MockSession::new(&plan);
        if read_failure {
            session.fail_read = Some(3);
        } else {
            session.close_fails = true;
        }
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert!(!report.succeeded());
        if read_failure {
            assert_eq!(session.writes, vec![vec![2]]);
        } else {
            assert_eq!(report.operation, Ok(Status::RestoredVerified));
            assert_eq!(report.cleanup, Some(Err(SessionError::Uncertain)));
        }
    }
}

#[test]
fn session_identity_mismatch_is_rejected_before_observation() {
    // An unexpected identity in an adapter result is compared with the plan.
    let (_dir, journal) = journal();
    let plan = Plan::new(input("identity-binding")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.route.identity[0] ^= 1;
    let report = ready(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(Error::ObservationMismatch))
    );
    assert!(session.writes.is_empty());
}

#[test]
fn typed_adapter_rejected_selection_emits_no_write() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("adapter-preflight")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.fail_read = Some(1);
    session.read_error = SessionError::InvalidSelection;
    let report = ready(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Session(SessionError::InvalidSelection))
    );
    assert!(session.writes.is_empty());
    assert_eq!(session.closes, 1);
    assert_eq!(
        std::fs::read_dir(journal.filesystem_root().unwrap())
            .unwrap()
            .count(),
        0
    );
}

fn assert_route_still_quarantined(journal: &Journal) {
    let plan = Plan::new(input("later-operation")).unwrap();
    let approval = approved(&plan, 99);
    let mut session = MockSession::new(&plan);
    let reopened = Journal::open(journal.filesystem_root().unwrap()).unwrap();
    let report = ready(execute(
        &reopened,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(Error::RouteUnresolved))
    );
    assert!(session.writes.is_empty());
}

#[test]
fn device_only_or_failed_cleanup_keeps_route_quarantined_across_reopen() {
    for failure in [false, true] {
        let (_dir, journal) = journal();
        let plan = Plan::new(input("cleanup-incomplete")).unwrap();
        let approval = approved(&plan, 1);
        let mut session = MockSession::new(&plan);
        session.close_fails = failure;
        session.close_evidence = CleanupEvidence::DeviceOnly;
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert_eq!(report.operation, Ok(Status::RestoredVerified));
        assert!(!report.succeeded());
        assert_route_still_quarantined(&journal);
    }
}

#[test]
fn pending_or_cancelled_cleanup_never_releases_the_route() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("pending-cleanup")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    session.close_pending = true;
    let mut future = Box::pin(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    let mut cx = Context::from_waker(Waker::noop());
    assert!(future.as_mut().poll(&mut cx).is_pending());
    assert_eq!(
        journal.recovered_status("pending-cleanup"),
        Status::RestoredVerified
    );
    assert_route_still_quarantined(&journal);
    drop(future);
    assert_route_still_quarantined(&journal);
    assert_eq!(session.writes, vec![vec![2], vec![1]]);
    assert_eq!(session.closes, 1);
}

#[test]
fn cleanup_evidence_failure_keeps_quarantine_despite_confirmed_network_close() {
    let (_dir, journal) = journal();
    let plan = Plan::new(input("cleanup-io")).unwrap();
    let approval = approved(&plan, 1);
    let mut session = MockSession::new(&plan);
    let gate = std::sync::Arc::new(std::sync::atomic::AtomicBool::new(false));
    session.close_gate = Some(gate.clone());
    let operation = Journal::operation_key(&plan);
    let mut future = Box::pin(execute(
        &journal,
        plan,
        Some(approval),
        &mut session,
        &FixedClock,
    ));
    let mut cx = Context::from_waker(Waker::noop());
    assert!(future.as_mut().poll(&mut cx).is_pending());
    // Model failure to publish the exclusive cleanup evidence record.
    std::fs::create_dir(journal.path(&operation, "cleanup").unwrap()).unwrap();
    gate.store(true, std::sync::atomic::Ordering::SeqCst);
    let Poll::Ready(report) = future.as_mut().poll(&mut cx) else {
        panic!("cleanup must finish");
    };
    assert_eq!(
        report.operation,
        Err(DispatchError::Guard(Error::AlreadyConsumed))
    );
    assert_eq!(
        report.cleanup,
        Some(Ok(CleanupEvidence::DeviceAndGatewayConfirmed))
    );
    assert!(!report.succeeded());
    drop(future);
    assert_route_still_quarantined(&journal);
}

// Alternate persistence tests execute this same dispatcher and guard. The store
// below is an owned, in-memory durability fixture, never a native OS adapter.
mod alternate_persistence {
    use super::*;
    use crate::guard::{JournalBinding, JournalPersistence, JournalRecord, PersistenceError};
    use std::collections::{HashMap, HashSet};
    use std::sync::{Arc, Mutex};

    #[derive(Clone, Copy, Debug, PartialEq, Eq)]
    enum Fault {
        Preflight,
        ReserveBefore,
        ReserveRoute,
        ReserveApproval,
        ReserveIntent,
        Append(JournalRecord),
        Release,
    }

    #[derive(Debug, Default)]
    struct State {
        fault: Option<Fault>,
        routes: HashMap<[u8; 32], JournalBinding>,
        approvals: HashSet<[u8; 16]>,
        intents: HashMap<[u8; 32], Vec<u8>>,
        records: HashMap<([u8; 32], JournalRecord), Vec<u8>>,
        events: Vec<JournalRecord>,
    }

    #[derive(Debug, Default)]
    struct Store(Mutex<State>);

    impl JournalPersistence for Store {
        fn preflight(&self) -> Result<(), PersistenceError> {
            if self.0.lock().unwrap().fault == Some(Fault::Preflight) {
                Err(PersistenceError::Unavailable)
            } else {
                Ok(())
            }
        }

        fn reserve(&self, binding: &JournalBinding, intent: &[u8]) -> Result<(), PersistenceError> {
            assert!(intent.len() <= crate::guard::MAX_JOURNAL_RECORD_BYTES);
            let decoded: crate::guard::Intent = serde_json::from_slice(intent).unwrap();
            assert_eq!(decoded.plan_digest, binding.plan());
            let mut state = self.0.lock().unwrap();
            if state.fault == Some(Fault::ReserveBefore) {
                return Err(PersistenceError::Unavailable);
            }
            if state.intents.contains_key(&binding.operation())
                || state.approvals.contains(&binding.approval())
            {
                return Err(PersistenceError::AlreadyConsumed);
            }
            if state.routes.contains_key(&binding.route()) {
                return Err(PersistenceError::RouteUnresolved);
            }
            state.routes.insert(binding.route(), binding.clone());
            if state.fault == Some(Fault::ReserveRoute) {
                return Err(PersistenceError::Unavailable);
            }
            state.approvals.insert(binding.approval());
            if state.fault == Some(Fault::ReserveApproval) {
                return Err(PersistenceError::Unavailable);
            }
            state.intents.insert(binding.operation(), intent.to_vec());
            if state.fault == Some(Fault::ReserveIntent) {
                return Err(PersistenceError::Unavailable);
            }
            Ok(())
        }

        fn append(
            &self,
            binding: &JournalBinding,
            kind: JournalRecord,
            bytes: &[u8],
        ) -> Result<(), PersistenceError> {
            assert!(bytes.len() <= crate::guard::MAX_JOURNAL_RECORD_BYTES);
            let mut state = self.0.lock().unwrap();
            if state.routes.get(&binding.route()) != Some(binding) {
                return Err(PersistenceError::RouteUnresolved);
            }
            if state.records.contains_key(&(binding.operation(), kind)) {
                return Err(PersistenceError::AlreadyConsumed);
            }
            // A failed barrier may follow a persisted effect. The guard must
            // still return an error and retain route quarantine.
            state
                .records
                .insert((binding.operation(), kind), bytes.to_vec());
            state.events.push(kind);
            if state.fault == Some(Fault::Append(kind)) {
                return Err(PersistenceError::Unavailable);
            }
            Ok(())
        }

        fn read_intent(
            &self,
            operation: [u8; 32],
            limit: usize,
        ) -> Result<Option<Vec<u8>>, PersistenceError> {
            let state = self.0.lock().unwrap();
            bounded(state.intents.get(&operation), limit)
        }

        fn read_record(
            &self,
            operation: [u8; 32],
            kind: JournalRecord,
            limit: usize,
        ) -> Result<Option<Vec<u8>>, PersistenceError> {
            let state = self.0.lock().unwrap();
            bounded(state.records.get(&(operation, kind)), limit)
        }

        fn release(&self, binding: &JournalBinding) -> Result<(), PersistenceError> {
            let mut state = self.0.lock().unwrap();
            if state.routes.get(&binding.route()) != Some(binding)
                || !state
                    .records
                    .contains_key(&(binding.operation(), JournalRecord::CleanupConfirmed))
                || state.fault == Some(Fault::Release)
            {
                return Err(PersistenceError::Unavailable);
            }
            state.routes.remove(&binding.route());
            Ok(())
        }
    }

    fn bounded(value: Option<&Vec<u8>>, limit: usize) -> Result<Option<Vec<u8>>, PersistenceError> {
        match value {
            Some(bytes) if bytes.len() > limit => Err(PersistenceError::Unavailable),
            Some(bytes) => Ok(Some(bytes.clone())),
            None => Ok(None),
        }
    }

    fn external(fault: Option<Fault>) -> (Arc<Store>, Journal) {
        let store = Arc::new(Store::default());
        store.0.lock().unwrap().fault = fault;
        let journal = Journal::with_persistence(store.clone()).unwrap();
        (store, journal)
    }

    fn assert_locked(store: &Store, journal: &Journal, id: &str) {
        assert_eq!(store.0.lock().unwrap().routes.len(), 1);
        let mut next = input(id);
        next.route.target = "another-synthetic-target".into();
        let plan = Plan::new(next).unwrap();
        let approval = approved(&plan, 99);
        let mut session = MockSession::new(&plan);
        let report = ready(execute(
            journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert_eq!(
            report.operation,
            Err(DispatchError::Guard(Error::RouteUnresolved))
        );
        assert!(session.writes.is_empty());
    }

    #[test]
    fn actual_guard_persists_full_large_plan_and_releases_only_after_cleanup() {
        let (store, journal) = external(None);
        let mut selected = input("external-full-plan");
        selected.purpose = "p".repeat(1024);
        selected.recovery = "r".repeat(1024);
        let plan = Plan::new(selected.clone()).unwrap();
        let expected_digest = plan.digest();
        let approval = approved(&plan, 3);
        let mut session = MockSession::new(&plan);
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert!(report.succeeded());
        assert_eq!(session.writes, [vec![2], vec![1]]);
        assert_eq!(
            journal.recovered_status("external-full-plan"),
            Status::RestoredVerified
        );
        let state = store.0.lock().unwrap();
        let bytes = state.intents.values().next().unwrap();
        assert!(
            bytes.len() > 2048,
            "digest-only Task156 records cannot substitute for the complete guard intent"
        );
        let intent: crate::guard::Intent = serde_json::from_slice(bytes).unwrap();
        assert_eq!(intent.plan_digest, expected_digest);
        assert_eq!(
            serde_json::to_value(intent.plan).unwrap(),
            serde_json::to_value(selected).unwrap()
        );
        assert_eq!(
            state.events,
            [
                JournalRecord::DesiredReadback,
                JournalRecord::RestoreIntent,
                JournalRecord::RestoredReadback,
                JournalRecord::CleanupConfirmed
            ]
        );
        assert!(state.routes.is_empty());
    }

    #[test]
    fn external_preflight_and_reservation_failure_never_select_a_write() {
        let store = Arc::new(Store::default());
        store.0.lock().unwrap().fault = Some(Fault::Preflight);
        assert!(matches!(
            Journal::with_persistence(store.clone()),
            Err(Error::Io)
        ));
        assert!(store.0.lock().unwrap().routes.is_empty());
        for fault in [
            Fault::ReserveBefore,
            Fault::ReserveRoute,
            Fault::ReserveApproval,
            Fault::ReserveIntent,
        ] {
            let (store, journal) = external(Some(fault));
            let plan = Plan::new(input("reserve-failure")).unwrap();
            let approval = approved(&plan, 1);
            let mut session = MockSession::new(&plan);
            let report = ready(execute(
                &journal,
                plan,
                Some(approval),
                &mut session,
                &FixedClock,
            ));
            assert_eq!(report.operation, Err(DispatchError::Guard(Error::Io)));
            assert!(session.writes.is_empty());
            assert_eq!(session.closes, 1);
            assert_eq!(
                journal.recovered_status("reserve-failure"),
                Status::OutcomeUnknown
            );
            store.0.lock().unwrap().fault = None;
            if fault != Fault::ReserveBefore {
                assert_locked(&store, &journal, "reservation-retry");
            }
        }
    }

    #[test]
    fn external_failed_record_barriers_never_report_success_or_release_route() {
        for (kind, writes) in [
            (JournalRecord::DesiredReadback, 1),
            (JournalRecord::RestoreIntent, 1),
            (JournalRecord::RestoredReadback, 2),
            (JournalRecord::CleanupConfirmed, 2),
        ] {
            let (store, journal) = external(Some(Fault::Append(kind)));
            let plan = Plan::new(input("record-failure")).unwrap();
            let approval = approved(&plan, 1);
            let mut session = MockSession::new(&plan);
            let report = ready(execute(
                &journal,
                plan,
                Some(approval),
                &mut session,
                &FixedClock,
            ));
            assert_eq!(report.operation, Err(DispatchError::Guard(Error::Io)));
            assert_eq!(session.writes.len(), writes);
            assert!(!report.succeeded());
            store.0.lock().unwrap().fault = None;
            assert_locked(&store, &journal, "post-record-failure");
        }
    }

    #[test]
    fn external_restored_status_is_separate_from_cleanup_and_release() {
        for fail_release in [false, true] {
            let (store, journal) = external(if fail_release {
                Some(Fault::Release)
            } else {
                None
            });
            let plan = Plan::new(input("cleanup-failure")).unwrap();
            let approval = approved(&plan, 1);
            let mut session = MockSession::new(&plan);
            session.close_fails = !fail_release;
            let report = ready(execute(
                &journal,
                plan,
                Some(approval),
                &mut session,
                &FixedClock,
            ));
            assert!(!report.succeeded());
            assert_eq!(
                journal.recovered_status("cleanup-failure"),
                Status::RestoredVerified
            );
            assert_eq!(session.writes.len(), 2);
            store.0.lock().unwrap().fault = None;
            assert_locked(&store, &journal, "cleanup-retry");
        }
    }

    #[test]
    fn external_missing_approval_uncertain_write_and_cancel_keep_original_guard_rules() {
        let (store, journal) = external(None);
        let plan = Plan::new(input("no-approval")).unwrap();
        let mut session = MockSession::new(&plan);
        let report = ready(execute(&journal, plan, None, &mut session, &FixedClock));
        assert_eq!(
            report.operation,
            Err(DispatchError::Guard(Error::ApprovalUnavailable))
        );
        assert_eq!(session.reads, 0);
        assert!(store.0.lock().unwrap().routes.is_empty());
        for cancel in [false, true] {
            let (store, journal) = external(None);
            let plan = Plan::new(input("uncertain-or-cancelled")).unwrap();
            let approval = approved(&plan, 1);
            let mut session = MockSession::new(&plan);
            if cancel {
                session.pending_write = true;
                let mut future = pin!(execute(
                    &journal,
                    plan,
                    Some(approval),
                    &mut session,
                    &FixedClock
                ));
                let mut cx = Context::from_waker(Waker::noop());
                assert!(matches!(future.as_mut().poll(&mut cx), Poll::Pending));
            } else {
                session.fail_write = Some(1);
                let report = ready(execute(
                    &journal,
                    plan,
                    Some(approval),
                    &mut session,
                    &FixedClock,
                ));
                assert!(matches!(
                    report.operation,
                    Err(DispatchError::Session(SessionError::Uncertain))
                ));
            }
            assert_eq!(session.writes.len(), 1);
            assert_eq!(
                journal.recovered_status("uncertain-or-cancelled"),
                Status::OutcomeUnknown
            );
            assert_locked(&store, &journal, "uncertain-retry");
        }
    }

    #[test]
    fn external_replay_and_corrupt_recovery_are_not_promoted_to_success() {
        let (store, journal) = external(None);
        let plan = Plan::new(input("external-replay")).unwrap();
        let approval = approved(&plan, 1);
        let mut session = MockSession::new(&plan);
        assert!(
            ready(execute(
                &journal,
                plan,
                Some(approval),
                &mut session,
                &FixedClock
            ))
            .succeeded()
        );
        let plan = Plan::new(input("external-replay")).unwrap();
        let approval = approved(&plan, 2);
        let mut session = MockSession::new(&plan);
        let report = ready(execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock,
        ));
        assert_eq!(
            report.operation,
            Err(DispatchError::Guard(Error::AlreadyConsumed))
        );
        assert!(session.writes.is_empty());
        let operation = crate::guard::digest(b"external-replay");
        let original = store.0.lock().unwrap().intents[&operation].clone();
        for bad in [
            b"{}".to_vec(),
            vec![b'x'; crate::guard::MAX_JOURNAL_RECORD_BYTES + 1],
        ] {
            store.0.lock().unwrap().intents.insert(operation, bad);
            assert_eq!(
                journal.recovered_status("external-replay"),
                Status::OutcomeUnknown
            );
        }
        store.0.lock().unwrap().intents.insert(operation, original);
        store.0.lock().unwrap().records.insert(
            (operation, JournalRecord::RestoredReadback),
            br#"{"schema":1,"schema":1}"#.to_vec(),
        );
        assert_eq!(
            journal.recovered_status("external-replay"),
            Status::OutcomeUnknown
        );
    }
}
