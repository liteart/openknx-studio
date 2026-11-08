// SPDX-License-Identifier: Apache-2.0
//! Actual guard/dispatcher plus the real bridge and an owned fault-model store.
use crate::guard as actual_guard;
use crate::guard::dispatcher::{
    self, CleanupEvidence, Clock, SessionError, SessionRead, TypedSession,
};
use crate::guard_storage::{GuardStore, MAX_INTENT_BYTES};
use crate::tests::{Failure, FakeStorage};
use crate::{Binding, Key, NamespaceStorage, Storage};
use actual_guard::{BoundDispatch, Journal, Plan, PlanInput, Route, Selector, Status};
use sha2::{Digest as _, Sha256};
use std::future::{Future, pending};
use std::pin::pin;
use std::sync::Arc;
use std::task::{Context, Poll, Waker};

crate::guard_persistence_backend!(ActualGuardPersistence, actual_guard);

fn binding(operation: u8, approval: u8) -> Binding {
    Binding::new([1; 32], [operation; 32], [3; 32], [approval; 16]).unwrap()
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
        purpose: "p".repeat(1024),
        recovery: "r".repeat(1024),
        expires_at_utc_seconds: 200,
    }
}
fn hash(bytes: &[u8]) -> [u8; 32] {
    Sha256::digest(bytes).into()
}
fn ready<F: Future>(future: F) -> F::Output {
    let mut future = pin!(future);
    match future
        .as_mut()
        .poll(&mut Context::from_waker(Waker::noop()))
    {
        Poll::Ready(value) => value,
        Poll::Pending => panic!("owned immediate session unexpectedly pending"),
    }
}
struct FixedClock;
impl Clock for FixedClock {
    fn now(&self) -> u64 {
        100
    }
}
struct Session {
    input: PlanInput,
    value: Vec<u8>,
    writes: usize,
    reads: usize,
    uncertain: bool,
    wait: bool,
    close_fails: bool,
}
impl Session {
    fn new(input: &PlanInput) -> Self {
        Self {
            input: input.clone(),
            value: input.original.clone(),
            writes: 0,
            reads: 0,
            uncertain: false,
            wait: false,
            close_fails: false,
        }
    }
}
impl TypedSession for Session {
    fn route(&self) -> &Route {
        &self.input.route
    }
    async fn observe(&mut self, selector: &Selector) -> Result<SessionRead, SessionError> {
        self.reads += 1;
        Ok(SessionRead {
            route: self.input.route.clone(),
            selector: selector.clone(),
            value: self.value.clone(),
        })
    }
    async fn write(&mut self, dispatch: BoundDispatch) -> Result<SessionRead, SessionError> {
        self.writes += 1;
        self.value = dispatch.bytes().to_vec();
        if self.wait {
            pending::<()>().await;
        }
        if self.uncertain {
            return Err(SessionError::Uncertain);
        }
        Ok(SessionRead {
            route: self.input.route.clone(),
            selector: dispatch.selector().clone(),
            value: self.value.clone(),
        })
    }
    async fn close(&mut self) -> Result<CleanupEvidence, SessionError> {
        if self.close_fails {
            Err(SessionError::Uncertain)
        } else {
            Ok(CleanupEvidence::DeviceAndGatewayConfirmed)
        }
    }
}
fn open_guard(store: &FakeStorage) -> Journal {
    Journal::with_persistence(Arc::new(ActualGuardPersistence::new(store.clone()))).unwrap()
}
fn execute(
    journal: &Journal,
    selected: PlanInput,
    approval_id: u8,
    session: &mut Session,
) -> dispatcher::ExecutionReport {
    let plan = Plan::new(selected).unwrap();
    let approval = actual_guard::synthetic_approval_for_test([approval_id; 16], plan.digest(), 200);
    ready(dispatcher::execute(
        journal,
        plan,
        Some(approval),
        session,
        &FixedClock,
    ))
}

#[test]
fn full_sixteen_kib_intent_is_lossless_and_oversize_fails_before_mutation() {
    for size in [1, 2048, 2049, MAX_INTENT_BYTES] {
        let storage = FakeStorage::default();
        let bridge = GuardStore::new(storage.clone());
        let bytes: Vec<u8> = (0..size).map(|index| (index % 251) as u8).collect();
        bridge.reserve(binding(2, 4), &bytes).unwrap();
        assert_eq!(
            bridge.intent([2; 32], MAX_INTENT_BYTES).unwrap(),
            Some(bytes.clone())
        );
        assert!(bridge.intent([2; 32], size.saturating_sub(1)).is_err());
        storage.crash();
        assert_eq!(
            bridge.intent([2; 32], MAX_INTENT_BYTES).unwrap(),
            Some(bytes)
        );
    }
    let storage = FakeStorage::default();
    assert!(
        GuardStore::new(storage.clone())
            .reserve(binding(2, 4), &vec![0; MAX_INTENT_BYTES + 1])
            .is_err()
    );
    assert_eq!(storage.calls(), 0);
    assert!(!storage.has_route([1; 32]));
}

#[test]
fn manifest_last_partial_missing_wrong_or_extra_parts_never_recover_success() {
    let storage = FakeStorage::default();
    let bridge = GuardStore::new(storage.clone());
    bridge.reserve(binding(2, 4), &vec![42; 4097]).unwrap();
    let part = storage
        .record_bytes(Key::GuardIntentPart([2; 32], 1))
        .unwrap();
    for bad in [None, Some(vec![1; 2048]), Some(vec![42; 2047])] {
        storage.replace_record(Key::GuardIntentPart([2; 32], 1), bad);
        assert!(bridge.intent([2; 32], MAX_INTENT_BYTES).is_err());
    }
    storage.replace_record(Key::GuardIntentPart([2; 32], 1), Some(part));
    storage.replace_record(Key::GuardIntentPart([2; 32], 3), Some(vec![1]));
    assert!(bridge.intent([2; 32], MAX_INTENT_BYTES).is_err());
    storage.replace_record(Key::GuardIntentPart([2; 32], 3), None);
    storage.replace_record(Key::GuardIntent([2; 32]), Some(b"{}".to_vec()));
    assert!(bridge.intent([2; 32], MAX_INTENT_BYTES).is_err());
}

#[test]
fn actual_guard_uses_exact_consumer_trait_and_full_intent_without_second_state_machine() {
    let store = FakeStorage::default();
    let journal = open_guard(&store);
    let selected = input("actual-bridge");
    let mut session = Session::new(&selected);
    assert!(execute(&journal, selected.clone(), 1, &mut session).succeeded());
    assert_eq!(session.writes, 2);
    assert_eq!(session.reads, 4);
    assert_eq!(
        journal.recovered_status("actual-bridge"),
        Status::RestoredVerified
    );
    let intent = GuardStore::new(store.clone())
        .intent(hash(b"actual-bridge"), MAX_INTENT_BYTES)
        .unwrap()
        .unwrap();
    assert!(intent.len() > 2048);
    let value: serde_json::Value = serde_json::from_slice(&intent).unwrap();
    assert_eq!(
        value["plan"],
        serde_json::to_value(selected.clone()).unwrap()
    );
    store.crash();
    let reopened = open_guard(&store);
    let mut replay = Session::new(&selected);
    assert!(matches!(
        execute(&reopened, selected, 2, &mut replay).operation,
        Err(dispatcher::DispatchError::Guard(
            actual_guard::Error::AlreadyConsumed
        ))
    ));
    assert_eq!(replay.writes, 0);
}

#[test]
fn namespace_exclusion_covers_distinct_routes_and_record_only_recovery() {
    let storage = FakeStorage::default();
    let lease = storage.lock_route([1; 32]).unwrap();
    assert!(matches!(
        storage.lock_route([2; 32]),
        Err(crate::StorageError::Busy)
    ));
    assert!(matches!(
        storage.lock_records(),
        Err(crate::StorageError::Busy)
    ));
    assert!(
        GuardStore::new(storage.clone())
            .intent([2; 32], MAX_INTENT_BYTES)
            .is_err()
    );
    drop(lease);
    assert!(storage.lock_records().is_ok());
    assert!(
        !storage.has_route([0xfe; 32]),
        "record lease must never manufacture a sentinel route head"
    );
}

#[test]
fn every_reservation_write_or_barrier_fault_withholds_the_actual_guard_write() {
    let baseline = FakeStorage::default();
    let bridge = GuardStore::new(baseline.clone());
    bridge.reserve(binding(2, 4), &vec![42; 4097]).unwrap();
    let calls = baseline.calls();
    for call in 1..=calls {
        for failure in [Failure::Before, Failure::Partial, Failure::After] {
            let store = FakeStorage::default();
            let bridge = GuardStore::new(store.clone());
            store.fail(call, failure);
            assert!(bridge.reserve(binding(2, 4), &vec![42; 4097]).is_err());
            store.crash();
            let value = bridge.intent([2; 32], MAX_INTENT_BYTES);
            // No caller received a successful reservation. Only a fully
            // durable final manifest may survive the persist-then-error case.
            if let Ok(Some(bytes)) = value {
                assert_eq!(bytes, vec![42; 4097]);
            }
        }
    }
    let store = FakeStorage::default();
    let journal = open_guard(&store);
    store.fail(2, Failure::Before); // initial active-route durability barrier
    let selected = input("actual-reserve-fault");
    let mut session = Session::new(&selected);
    assert!(!execute(&journal, selected, 1, &mut session).succeeded());
    assert_eq!(session.writes, 0);
}

#[test]
fn actual_guard_uncertainty_cancel_and_failed_cleanup_keep_route_quarantine() {
    for mode in 0..3 {
        let store = FakeStorage::default();
        let journal = open_guard(&store);
        let selected = input("guard-quarantine");
        let mut session = Session::new(&selected);
        if mode == 0 {
            session.uncertain = true;
        }
        if mode == 1 {
            session.wait = true;
        }
        if mode == 2 {
            session.close_fails = true;
        }
        if mode == 1 {
            let plan = Plan::new(selected.clone()).unwrap();
            let approval = actual_guard::synthetic_approval_for_test([1; 16], plan.digest(), 200);
            let mut future = pin!(dispatcher::execute(
                &journal,
                plan,
                Some(approval),
                &mut session,
                &FixedClock
            ));
            assert!(matches!(
                future
                    .as_mut()
                    .poll(&mut Context::from_waker(Waker::noop())),
                Poll::Pending
            ));
        } else {
            assert!(!execute(&journal, selected.clone(), 1, &mut session).succeeded());
        }
        assert_eq!(session.writes, if mode == 2 { 2 } else { 1 });
        store.crash();
        let reopened = open_guard(&store);
        let mut next = input("different-operation");
        next.route.target = "another-target-same-bus".into();
        let mut next_session = Session::new(&next);
        assert!(matches!(
            execute(&reopened, next, 9, &mut next_session).operation,
            Err(dispatcher::DispatchError::Guard(
                actual_guard::Error::RouteUnresolved
            ))
        ));
        assert_eq!(next_session.writes, 0);
    }
}

#[test]
fn actual_dispatcher_persistence_faults_never_report_success_or_free_changed_state() {
    let baseline = FakeStorage::default();
    let selected = input("fault-sweep");
    let normal = open_guard(&baseline);
    let mut session = Session::new(&selected);
    assert!(execute(&normal, selected.clone(), 1, &mut session).succeeded());
    // Exclude the construction preflight; inject into every later mutation or
    // barrier of the actual guard-to-storage bridge, not a modeled dispatcher.
    let calls = baseline.calls() - 1;
    for call in 1..=calls {
        for failure in [Failure::Before, Failure::Partial, Failure::After] {
            let store = FakeStorage::default();
            let actual = open_guard(&store);
            store.fail(call, failure);
            let mut session = Session::new(&selected);
            let report = execute(&actual, selected.clone(), 1, &mut session);
            assert!(
                !report.succeeded(),
                "fault at {call} must not release a success report"
            );
            assert!(session.writes <= 2);
            store.crash();
            let reopened = open_guard(&store);
            let next = input("post-fault");
            let mut next_session = Session::new(&next);
            let next_report = execute(&reopened, next, 2, &mut next_session);
            if next_report.succeeded() {
                // Reuse is safe only before any first write or with complete,
                // durable restoration/cleanup (e.g. persist-then-error final flush).
                assert_eq!(session.value, selected.original);
                assert!(session.writes == 0 || session.writes == 2);
            } else {
                assert_eq!(next_session.writes, 0);
            }
        }
    }
}
