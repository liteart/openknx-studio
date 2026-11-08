// SPDX-License-Identifier: Apache-2.0
//! Native synthetic registry tests. Type checking is not native execution.

use super::*;
use crate::{AttemptStatus, Binding, Event, Journal, RouteStatus};
use std::fs;
use std::process::{Child, Command};
use std::time::{Duration, Instant};

fn binding(operation: u8, approval: u8) -> Binding {
    Binding::new([1; 32], [operation; 32], [3; 32], [approval; 16]).unwrap()
}

#[test]
fn native_exact_ledger_is_read_back_and_released_only_after_cleanup() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let storage = WindowsStorage::open(&root).unwrap();
    let journal = Journal::new(storage);
    let mut attempt = journal.begin(binding(2, 4)).unwrap();
    for event in &crate::EVENTS[1..5] {
        journal.record(&mut attempt, [3; 32], *event).unwrap();
        assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Quarantined));
    }
    journal
        .record(&mut attempt, [3; 32], Event::CleanupConfirmed)
        .unwrap();
    assert_eq!(attempt.status(), AttemptStatus::Released);
    assert_eq!(journal.inspect_route([1; 32]), Ok(RouteStatus::Available));
    drop(journal);
    let reopened = Journal::new(WindowsStorage::open(&root).unwrap());
    assert_eq!(reopened.inspect_route([1; 32]), Ok(RouteStatus::Available));
    assert!(matches!(
        reopened.begin(binding(2, 5)),
        Err(crate::Error::AlreadyConsumed)
    ));
    assert!(reopened.begin(binding(6, 7)).is_ok());
}

#[test]
fn native_partial_oversized_or_missing_chunks_are_not_reusable() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let storage = WindowsStorage::open(&root).unwrap();
    let mut lease = storage.lock_route([1; 32]).unwrap();
    let id = Key::Operation([2; 32]);
    let (partial, new) = create(&lease.namespace, &native_layout::record_path(id)).unwrap();
    assert!(new);
    lease.flush().unwrap();
    assert_eq!(
        lease.read(id, MAX_RECORD_BYTES),
        Err(StorageError::Unavailable)
    );
    assert_eq!(
        lease.create_new(id, b"replacement"),
        Err(StorageError::Existing)
    );
    let (data, _) = create(&partial, native_layout::DATA).unwrap();
    let (invalid, _) = create(&data, "0000_0801_00").unwrap();
    drop(invalid);
    assert_eq!(
        lease.read(id, MAX_RECORD_BYTES),
        Err(StorageError::Unavailable)
    );
    data.RegDeleteKeyEx("0000_0801_00", co::KEY::WOW64_64KEY)
        .unwrap();
    let valid = native_layout::encode_chunks(b"synthetic").unwrap();
    let (chunk, _) = create(&data, &valid[0]).unwrap();
    drop(chunk);
    assert_eq!(
        lease.read(id, MAX_RECORD_BYTES),
        Ok(Some(b"synthetic".to_vec()))
    );
    // No value reader is used, even when arbitrary oversized values exist.
    data.RegSetValueEx(
        Some("unexpected"),
        winsafe::RegistryValue::Binary(vec![0; MAX_RECORD_BYTES + 1]),
    )
    .unwrap();
    assert_eq!(
        lease.read(id, MAX_RECORD_BYTES),
        Err(StorageError::Unavailable)
    );
    lease.set_route(b"{").unwrap();
    lease.flush().unwrap();
    drop(lease);
    assert_eq!(
        Journal::new(storage).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

#[test]
fn native_torn_route_data_never_becomes_an_absent_available_route() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let storage = WindowsStorage::open(&root).unwrap();
    let journal = Journal::new(WindowsStorage::open(&root).unwrap());
    let mut attempt = journal.begin(binding(2, 4)).unwrap();
    for event in &crate::EVENTS[1..5] {
        journal.record(&mut attempt, [3; 32], *event).unwrap();
    }
    let mut lease = storage.lock_route([1; 32]).unwrap();
    let route = open(&lease.namespace, &native_layout::route_path([1; 32]))
        .unwrap()
        .unwrap();
    let data = open(&route, native_layout::DATA).unwrap().unwrap();
    let chunks = names(&data).unwrap();
    data.RegDeleteKeyEx(&chunks[0], co::KEY::WOW64_64KEY)
        .unwrap();
    lease.flush().unwrap();
    drop(data);
    drop(route);
    drop(lease);
    assert!(
        journal
            .record(&mut attempt, [3; 32], Event::CleanupConfirmed)
            .is_err()
    );
    assert_eq!(attempt.status(), AttemptStatus::OutcomeUnknown);
    assert_ne!(journal.inspect_route([1; 32]), Ok(RouteStatus::Available));
}

#[test]
fn native_cooperating_threads_have_at_most_one_begin_winner() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let barrier = std::sync::Arc::new(std::sync::Barrier::new(2));
    let results = std::thread::scope(|scope| {
        let handles: Vec<_> = (0..2)
            .map(|index| {
                let root = root.clone();
                let barrier = barrier.clone();
                scope.spawn(move || {
                    let journal = Journal::new(WindowsStorage::open(&root).unwrap());
                    barrier.wait();
                    journal.begin(binding(index + 2, index + 4))
                })
            })
            .collect();
        handles
            .into_iter()
            .map(|handle| handle.join().unwrap())
            .collect::<Vec<_>>()
    });
    assert_eq!(results.iter().filter(|result| result.is_ok()).count(), 1);
    assert_eq!(
        Journal::new(WindowsStorage::open(&root).unwrap()).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

struct ChildGuard(Child);
impl Drop for ChildGuard {
    fn drop(&mut self) {
        let _ = self.0.kill();
        let _ = self.0.wait();
    }
}

fn child(root: &Path, mode: &str, index: &str) -> ChildGuard {
    ChildGuard(
        Command::new(std::env::current_exe().unwrap())
            .args([
                "--exact",
                "windows::tests::native_child_process",
                "--nocapture",
            ])
            .env("OPENKNX_TASK156_NATIVE_ROOT", root)
            .env("OPENKNX_TASK156_NATIVE_MODE", mode)
            .env("OPENKNX_TASK156_NATIVE_INDEX", index)
            .spawn()
            .unwrap(),
    )
}

fn wait_marker(root: &Path, name: &str) {
    let deadline = Instant::now() + Duration::from_secs(5);
    while !root.join(name).exists() {
        assert!(Instant::now() < deadline, "child marker deadline expired");
        std::thread::sleep(Duration::from_millis(10));
    }
}

fn wait_exit(child: &mut ChildGuard) {
    let deadline = Instant::now() + Duration::from_secs(5);
    loop {
        if let Some(status) = child.0.try_wait().unwrap() {
            assert!(status.success());
            break;
        }
        assert!(Instant::now() < deadline, "child exit deadline expired");
        std::thread::sleep(Duration::from_millis(10));
    }
}

#[test]
fn native_child_process() {
    let Some(root) = std::env::var_os("OPENKNX_TASK156_NATIVE_ROOT") else {
        return;
    };
    let root = PathBuf::from(root);
    let mode = std::env::var("OPENKNX_TASK156_NATIVE_MODE").unwrap();
    let index = std::env::var("OPENKNX_TASK156_NATIVE_INDEX").unwrap();
    assert!(index == "0" || index == "1");
    let storage = WindowsStorage::open(&root).unwrap();
    if mode == "hold" {
        let journal = Journal::new(WindowsStorage::open(&root).unwrap());
        let _attempt = journal.begin(binding(2, 4)).unwrap();
        let _lease = storage.lock_route([1; 32]).unwrap();
        fs::write(root.join("native-active-held"), b"synthetic").unwrap();
        std::thread::sleep(Duration::from_secs(5));
    } else {
        assert_eq!(mode, "contend");
        wait_marker(&root, "go");
        let result = Journal::new(storage).begin(binding(
            if index == "0" { 2 } else { 5 },
            if index == "0" { 4 } else { 6 },
        ));
        let bytes = match result {
            Ok(_) => b"reserved".as_slice(),
            Err(crate::Error::RouteQuarantined | crate::Error::Storage(StorageError::Busy)) => {
                b"blocked".as_slice()
            }
            Err(error) => panic!("unexpected native error {error:?}"),
        };
        fs::write(root.join(format!("outcome-{index}")), bytes).unwrap();
    }
}

#[test]
fn native_process_termination_releases_os_lock_but_preserves_active_quarantine() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let mut owner = child(&root, "hold", "0");
    wait_marker(&root, "native-active-held");
    let storage = WindowsStorage::open(&root).unwrap();
    assert!(matches!(
        storage.lock_route([1; 32]),
        Err(StorageError::Busy)
    ));
    owner.0.kill().unwrap();
    owner.0.wait().unwrap();
    assert_eq!(
        Journal::new(storage).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

#[test]
fn native_two_processes_cannot_both_reserve_the_same_route() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let mut first = child(&root, "contend", "0");
    let mut second = child(&root, "contend", "1");
    fs::write(root.join("go"), b"synthetic").unwrap();
    wait_exit(&mut first);
    wait_exit(&mut second);
    let results = [
        fs::read(root.join("outcome-0")).unwrap(),
        fs::read(root.join("outcome-1")).unwrap(),
    ];
    assert_eq!(
        results
            .iter()
            .filter(|bytes| bytes.as_slice() == b"reserved")
            .count(),
        1
    );
    assert_eq!(
        results
            .iter()
            .filter(|bytes| bytes.as_slice() == b"blocked")
            .count(),
        1
    );
    assert_eq!(
        Journal::new(WindowsStorage::open(&root).unwrap()).inspect_route([1; 32]),
        Ok(RouteStatus::Quarantined)
    );
}

#[test]
fn native_guard_full_intent_and_record_namespace_lease() {
    use crate::NamespaceStorage;
    let directory = tempfile::tempdir().unwrap();
    let root = std::fs::canonicalize(directory.path()).unwrap();
    let storage = WindowsStorage::open(&root).unwrap();
    let lease = storage.lock_route([1; 32]).unwrap();
    assert!(matches!(storage.lock_records(), Err(StorageError::Busy)));
    assert!(matches!(
        storage.lock_route([2; 32]),
        Err(StorageError::Busy)
    ));
    drop(lease);
    {
        let _records = storage.lock_records().unwrap();
        assert!(matches!(
            storage.lock_route([2; 32]),
            Err(StorageError::Busy)
        ));
    }
    let bytes = vec![0xa5; crate::guard_storage::MAX_INTENT_BYTES];
    let binding = Binding::new([1; 32], [0x22; 32], [3; 32], [4; 16]).unwrap();
    let bridge = crate::guard_storage::GuardStore::new(storage);
    bridge.reserve(binding, &bytes).unwrap();
    assert_eq!(
        bridge.intent([0x22; 32], bytes.len()).unwrap(),
        Some(bytes.clone())
    );
    drop(bridge);
    let reopened = crate::guard_storage::GuardStore::new(WindowsStorage::open(&root).unwrap());
    assert_eq!(
        reopened.intent([0x22; 32], bytes.len()).unwrap(),
        Some(bytes)
    );
    assert!(reopened.intent([0x22; 32], 2048).is_err());
}
