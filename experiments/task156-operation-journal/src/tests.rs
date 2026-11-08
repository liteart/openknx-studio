// SPDX-License-Identifier: Apache-2.0
// Owned fault simulator only: no filesystem, registry, credentials or network.
use super::*;
use std::collections::HashMap;
use std::sync::{Arc, Mutex, MutexGuard, TryLockError};

#[derive(Clone, Default, Debug, PartialEq, Eq)]
struct Image {
    routes: HashMap<Digest, Vec<u8>>,
    records: HashMap<Key, Vec<u8>>,
}

#[derive(Clone, Copy, Debug)]
enum Failure {
    Before,
    Partial,
    After,
}

#[derive(Default)]
struct State {
    visible: Image,
    durable: Image,
    calls: usize,
    failure: Option<(usize, Failure)>,
}

#[derive(Clone, Default)]
struct FakeStorage(Arc<Mutex<State>>);

impl FakeStorage {
    fn crash(&self) {
        let mut state = self.0.lock().unwrap();
        state.visible = state.durable.clone();
        state.calls = 0;
        state.failure = None;
    }

    fn fail(&self, call: usize, mode: Failure) {
        let mut state = self.0.lock().unwrap();
        state.calls = 0;
        state.failure = Some((call, mode));
    }

    fn calls(&self) -> usize {
        self.0.lock().unwrap().calls
    }
}

struct Lease<'a> {
    route: Digest,
    state: MutexGuard<'a, State>,
}

impl Storage for FakeStorage {
    type Lease<'a> = Lease<'a>;

    fn lock_route(&self, route: Digest) -> Result<Self::Lease<'_>, StorageError> {
        let state = self.0.try_lock().map_err(|error| match error {
            TryLockError::WouldBlock => StorageError::Busy,
            TryLockError::Poisoned(_) => StorageError::Unavailable,
        })?;
        Ok(Lease { route, state })
    }
}

impl Lease<'_> {
    fn failure(&mut self) -> Option<Failure> {
        self.state.calls += 1;
        self.state
            .failure
            .filter(|(call, _)| *call == self.state.calls)
            .map(|(_, mode)| mode)
    }
}

fn bounded(value: Option<&Vec<u8>>, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
    match value {
        Some(bytes) if bytes.len() > limit => Err(StorageError::Unavailable),
        Some(bytes) => Ok(Some(bytes.clone())),
        None => Ok(None),
    }
}

impl RouteLease for Lease<'_> {
    fn read_route(&mut self, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        bounded(self.state.visible.routes.get(&self.route), limit)
    }

    fn set_route(&mut self, bytes: &[u8]) -> Result<(), StorageError> {
        let failure = self.failure();
        if matches!(failure, Some(Failure::Before)) {
            return Err(StorageError::Unavailable);
        }
        self.state.visible.routes.insert(
            self.route,
            if matches!(failure, Some(Failure::Partial)) {
                b"{".to_vec()
            } else {
                bytes.to_vec()
            },
        );
        if failure.is_some() {
            Err(StorageError::Unavailable)
        } else {
            Ok(())
        }
    }

    fn read(&mut self, key: Key, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        bounded(self.state.visible.records.get(&key), limit)
    }

    fn create_new(&mut self, key: Key, bytes: &[u8]) -> Result<(), StorageError> {
        if self.state.visible.records.contains_key(&key) {
            return Err(StorageError::Existing);
        }
        let failure = self.failure();
        if matches!(failure, Some(Failure::Before)) {
            return Err(StorageError::Unavailable);
        }
        self.state.visible.records.insert(
            key,
            if matches!(failure, Some(Failure::Partial)) {
                b"{".to_vec()
            } else {
                bytes.to_vec()
            },
        );
        if failure.is_some() {
            Err(StorageError::Unavailable)
        } else {
            Ok(())
        }
    }

    fn flush(&mut self) -> Result<(), StorageError> {
        let failure = self.failure();
        if !matches!(failure, Some(Failure::Before | Failure::Partial)) {
            self.state.durable = self.state.visible.clone();
        }
        if failure.is_some() {
            Err(StorageError::Unavailable)
        } else {
            Ok(())
        }
    }
}

fn binding(operation: u8, approval: u8) -> Binding {
    Binding::new([1; 32], [operation; 32], [3; 32], [approval; 16]).unwrap()
}

fn advance(journal: &Journal<FakeStorage>, attempt: &mut Attempt, events: &[Event]) {
    for event in events {
        journal.record(attempt, [3; 32], *event).unwrap();
    }
}

#[test]
fn exact_sequence_is_durable_and_only_terminal_cleanup_releases_route() {
    let storage = FakeStorage::default();
    let journal = Journal::new(storage.clone());
    let mut attempt = journal.begin(binding(2, 4)).unwrap();
    assert_eq!(storage.calls(), 8); // Active, operation, approval, intent: write+flush.
    for event in &EVENTS[1..5] {
        journal.record(&mut attempt, [3; 32], *event).unwrap();
        assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
    }
    journal
        .record(&mut attempt, [3; 32], Event::CleanupConfirmed)
        .unwrap();
    assert_eq!(attempt.status(), AttemptStatus::Released);
    storage.crash();
    assert_eq!(
        Journal::new(storage.clone()).inspect_route([1; 32]),
        Ok(RouteStatus::Available)
    );
    assert!(journal.begin(binding(5, 6)).is_ok());
}

#[test]
fn replay_and_cross_route_grant_reuse_are_rejected_without_replacement() {
    let storage = FakeStorage::default();
    let journal = Journal::new(storage.clone());
    let mut attempt = journal.begin(binding(2, 4)).unwrap();
    advance(&journal, &mut attempt, &EVENTS[1..]);
    let before = storage.calls();
    let prior_image = storage.0.lock().unwrap().visible.clone();
    assert!(matches!(
        journal.begin(binding(2, 7)),
        Err(Error::AlreadyConsumed)
    ));
    assert!(matches!(
        journal.begin(binding(8, 4)),
        Err(Error::AlreadyConsumed)
    ));
    let mut other_route = binding(9, 4);
    other_route.route = [10; 32];
    assert!(matches!(
        journal.begin(other_route),
        Err(Error::AlreadyConsumed)
    ));
    assert_eq!(storage.calls(), before + 2); // two read-side release barriers only.
    assert_eq!(storage.0.lock().unwrap().visible, prior_image);
}

#[test]
fn active_route_is_exclusive_even_for_another_target_or_plan() {
    let storage = FakeStorage::default();
    let journal = Journal::new(storage.clone());
    let _attempt = journal.begin(binding(2, 4)).unwrap();
    let mut different = binding(5, 6);
    different.plan = [7; 32];
    assert!(matches!(
        journal.begin(different),
        Err(Error::RouteQuarantined)
    ));
    storage.crash();
    assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
}

#[test]
fn failed_begin_withholds_attempt_at_every_write_and_barrier() {
    for mode in [Failure::Before, Failure::Partial, Failure::After] {
        for point in 1..=8 {
            let storage = FakeStorage::default();
            storage.fail(point, mode);
            assert!(Journal::new(storage.clone()).begin(binding(2, 4)).is_err());
            storage.crash();
            let state = storage.0.lock().unwrap();
            let reserved = state.durable.routes.contains_key(&[1; 32]);
            drop(state);
            assert_eq!(
                Journal::new(storage).inspect_route([1; 32]).unwrap(),
                if reserved {
                    RouteStatus::Quarantined
                } else {
                    RouteStatus::Available
                }
            );
        }
    }
}

#[test]
fn failed_transition_latches_unknown_and_reopen_never_resumes() {
    for event_index in 1..EVENTS.len() {
        let mutations = if EVENTS[event_index] == Event::CleanupConfirmed {
            4
        } else {
            2
        };
        for point in 1..=mutations {
            for mode in [Failure::Before, Failure::Partial, Failure::After] {
                let storage = FakeStorage::default();
                let journal = Journal::new(storage.clone());
                let mut attempt = journal.begin(binding(2, 4)).unwrap();
                advance(&journal, &mut attempt, &EVENTS[1..event_index]);
                storage.fail(point, mode);
                assert!(
                    journal
                        .record(&mut attempt, [3; 32], EVENTS[event_index])
                        .is_err()
                );
                assert_eq!(attempt.status(), AttemptStatus::OutcomeUnknown);
                assert_eq!(
                    journal.record(&mut attempt, [3; 32], EVENTS[event_index]),
                    Err(Error::InvalidTransition)
                );
                storage.crash();
                let recovered = journal.inspect_route([1; 32]).unwrap();
                // A flush may persist and still return an error. Only a complete
                // terminal ledger may recover Available; no live call returned Released.
                let final_flush_persisted =
                    event_index == 5 && point == 4 && matches!(mode, Failure::After);
                assert_eq!(
                    recovered,
                    if final_flush_persisted {
                        RouteStatus::Available
                    } else {
                        RouteStatus::Quarantined
                    }
                );
            }
        }
    }
}

#[test]
fn wrong_plan_out_of_order_cancel_and_dropped_attempt_never_release() {
    let storage = FakeStorage::default();
    let journal = Journal::new(storage.clone());
    {
        let mut attempt = journal.begin(binding(2, 4)).unwrap();
        let before = storage.calls();
        assert_eq!(
            journal.record(&mut attempt, [7; 32], Event::DesiredIntent),
            Err(Error::PlanMismatch)
        );
        assert_eq!(
            journal.record(&mut attempt, [3; 32], Event::RestoredReadback),
            Err(Error::InvalidTransition)
        );
        assert_eq!(storage.calls(), before);
        attempt.cancel();
        assert_eq!(
            journal.record(&mut attempt, [3; 32], Event::DesiredIntent),
            Err(Error::InvalidTransition)
        );
    }
    storage.crash();
    assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
}

#[test]
fn released_head_without_full_exact_ledger_is_never_available() {
    for key in [
        Key::Operation([2; 32]),
        Key::Approval([4; 16]),
        Key::Event([2; 32], Event::DesiredReadback),
        Key::Event([2; 32], Event::CleanupConfirmed),
    ] {
        for mutation in 0..3 {
            let storage = FakeStorage::default();
            let journal = Journal::new(storage.clone());
            let mut attempt = journal.begin(binding(2, 4)).unwrap();
            advance(&journal, &mut attempt, &EVENTS[1..]);
            let mut state = storage.0.lock().unwrap();
            match mutation {
                0 => {
                    state.visible.records.remove(&key);
                }
                1 => {
                    state.visible.records.insert(key, b"{".to_vec());
                }
                _ => {
                    state
                        .visible
                        .records
                        .insert(key, record(&binding(6, 7), RecordKind::Operation).unwrap());
                }
            }
            drop(state);
            assert_ne!(journal.inspect_route([1; 32]), Ok(RouteStatus::Available));
        }
    }
}

#[test]
fn missing_or_corrupted_prior_evidence_prevents_a_live_release() {
    for key in [
        Key::Operation([2; 32]),
        Key::Approval([4; 16]),
        Key::Event([2; 32], Event::DesiredReadback),
    ] {
        let storage = FakeStorage::default();
        let journal = Journal::new(storage.clone());
        let mut attempt = journal.begin(binding(2, 4)).unwrap();
        advance(&journal, &mut attempt, &EVENTS[1..5]);
        storage.0.lock().unwrap().visible.records.remove(&key);
        assert_eq!(
            journal.record(&mut attempt, [3; 32], Event::CleanupConfirmed),
            Err(Error::RouteQuarantined)
        );
        assert_eq!(attempt.status(), AttemptStatus::OutcomeUnknown);
        assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
    }
}

#[test]
fn malformed_unknown_duplicate_and_oversized_heads_fail_closed() {
    let heads = [
        b"{".to_vec(),
        b"{\"schema\":1,\"schema\":1}".to_vec(),
        vec![b' '; MAX_RECORD_BYTES + 1],
        encode(&RouteRecord {
            schema: 9,
            binding: binding(2, 4),
            released: true,
        })
        .unwrap(),
        encode(&RouteRecord {
            schema: 1,
            binding: binding(2, 4),
            released: true,
        })
        .unwrap(),
    ];
    for head in heads {
        let storage = FakeStorage::default();
        storage
            .0
            .lock()
            .unwrap()
            .visible
            .routes
            .insert([1; 32], head);
        assert_ne!(
            Journal::new(storage).inspect_route([1; 32]),
            Ok(RouteStatus::Available)
        );
    }
}

#[test]
fn held_lease_blocks_competing_creator_and_no_os_lock_releases_active_route() {
    let storage = FakeStorage::default();
    let held = storage.lock_route([1; 32]).unwrap();
    assert!(matches!(
        Journal::new(storage.clone()).begin(binding(2, 4)),
        Err(Error::Storage(StorageError::Busy))
    ));
    drop(held);
    {
        let _attempt = Journal::new(storage.clone()).begin(binding(2, 4)).unwrap();
    }
    assert_eq!(
        Journal::new(storage).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

#[test]
fn concurrent_cooperating_creators_have_only_one_route_winner() {
    let storage = FakeStorage::default();
    let outcomes = std::thread::scope(|scope| {
        let first = storage.clone();
        let second = storage.clone();
        let a = scope.spawn(move || Journal::new(first).begin(binding(2, 4)));
        let b = scope.spawn(move || Journal::new(second).begin(binding(5, 6)));
        [a.join().unwrap(), b.join().unwrap()]
    });
    assert_eq!(outcomes.iter().filter(|result| result.is_ok()).count(), 1);
    assert_eq!(
        Journal::new(storage).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

#[test]
fn invalid_binding_and_zero_route_never_mutate_store() {
    assert_eq!(
        Binding::new([0; 32], [2; 32], [3; 32], [4; 16]),
        Err(Error::InvalidBinding)
    );
    let storage = FakeStorage::default();
    assert_eq!(
        Journal::new(storage.clone()).inspect_route([0; 32]),
        Err(Error::InvalidBinding)
    );
    assert_eq!(storage.calls(), 0);
}

#[test]
fn visible_release_is_not_available_without_a_successful_reader_barrier() {
    let storage = FakeStorage::default();
    let journal = Journal::new(storage.clone());
    let mut attempt = journal.begin(binding(2, 4)).unwrap();
    advance(&journal, &mut attempt, &EVENTS[1..5]);
    storage.fail(4, Failure::Before); // released head visible, final flush fails.
    assert!(
        journal
            .record(&mut attempt, [3; 32], Event::CleanupConfirmed)
            .is_err()
    );
    assert_eq!(attempt.status(), AttemptStatus::OutcomeUnknown);
    let persisted: RouteRecord = decode(
        storage
            .0
            .lock()
            .unwrap()
            .durable
            .routes
            .get(&[1; 32])
            .unwrap(),
    )
    .unwrap();
    assert!(!persisted.released);

    // Another cooperating caller observes the same visible registry state.
    // Its barrier must succeed before this state can justify reusing the route.
    storage.fail(1, Failure::Before);
    let other = Journal::new(storage.clone());
    assert!(matches!(
        other.begin(binding(5, 6)),
        Err(Error::Storage(StorageError::Unavailable))
    ));
    assert!(
        !storage
            .0
            .lock()
            .unwrap()
            .visible
            .records
            .contains_key(&Key::Operation([5; 32]))
    );
    storage.fail(1, Failure::Before);
    assert_eq!(
        other.inspect_route([1; 32]),
        Err(Error::Storage(StorageError::Unavailable))
    );
    let persisted: RouteRecord = decode(
        storage
            .0
            .lock()
            .unwrap()
            .durable
            .routes
            .get(&[1; 32])
            .unwrap(),
    )
    .unwrap();
    assert!(!persisted.released);

    storage.fail(usize::MAX, Failure::Before);
    assert_eq!(other.inspect_route([1; 32]), Ok(RouteStatus::Available));
    let persisted: RouteRecord = decode(
        storage
            .0
            .lock()
            .unwrap()
            .durable
            .routes
            .get(&[1; 32])
            .unwrap(),
    )
    .unwrap();
    assert!(persisted.released);
}

#[test]
fn wrong_plan_and_order_latch_the_live_attempt_before_any_storage_call() {
    for wrong_plan in [true, false] {
        let storage = FakeStorage::default();
        let journal = Journal::new(storage.clone());
        let mut attempt = journal.begin(binding(2, 4)).unwrap();
        let before = storage.calls();
        let result = if wrong_plan {
            journal.record(&mut attempt, [9; 32], Event::DesiredIntent)
        } else {
            journal.record(&mut attempt, [3; 32], Event::RestoredReadback)
        };
        assert_eq!(
            result,
            Err(if wrong_plan {
                Error::PlanMismatch
            } else {
                Error::InvalidTransition
            })
        );
        assert_eq!(attempt.status(), AttemptStatus::OutcomeUnknown);
        assert_eq!(
            journal.record(&mut attempt, [3; 32], Event::DesiredIntent),
            Err(Error::InvalidTransition)
        );
        assert_eq!(storage.calls(), before);
        assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
    }
}
