// SPDX-License-Identifier: Apache-2.0
//! Actual Task020 guard and bridge over native storage; every session is synthetic.
//! Compiling this module does not qualify Windows behavior or execute a device.

use super::*;
use crate::guard_storage::Evidence;
use crate::windows::{WindowsLease, WindowsStorage};
use crate::{Digest, RouteLease, StorageError};
use std::fs;
use std::io;
use std::path::{Path, PathBuf};
use std::process::{Child, Command, ExitStatus};
use std::sync::atomic::{AtomicBool, Ordering};
use std::time::{Duration, Instant};

const TIMEOUT: Duration = Duration::from_secs(30);

fn native_guard(root: &Path) -> Journal {
    Journal::with_persistence(Arc::new(ActualGuardPersistence::new(
        WindowsStorage::open(root).unwrap(),
    )))
    .unwrap()
}

/// The guard deliberately normalizes native Busy to Io. Record the actual
/// lock result in this test-only forwarding adapter, so a race cannot pass
/// because of an unrelated I/O, approval or session error.
struct ContentionStorage {
    native: WindowsStorage,
    busy: Arc<AtomicBool>,
}
impl ContentionStorage {
    fn observe<T>(&self, result: Result<T, StorageError>) -> Result<T, StorageError> {
        if matches!(result, Err(StorageError::Busy)) {
            self.busy.store(true, Ordering::SeqCst);
        }
        result
    }
}
impl Storage for ContentionStorage {
    type Lease<'a> = WindowsLease;
    fn lock_route(&self, route: Digest) -> Result<Self::Lease<'_>, StorageError> {
        self.observe(self.native.lock_route(route))
    }
}
impl NamespaceStorage for ContentionStorage {
    fn lock_records(&self) -> Result<Self::Lease<'_>, StorageError> {
        self.observe(self.native.lock_records())
    }
}

fn route(selected: &PlanInput) -> Digest {
    hash(&serde_json::to_vec(&(&selected.route.gateway, &selected.route.bus_path)).unwrap())
}

fn assert_quarantined(root: &Path) {
    let journal = native_guard(root);
    let mut selected = input("native-after-fault");
    selected.route.target = "different-target-same-gateway-and-bus".into();
    let mut session = Session::new(&selected);
    assert!(matches!(
        execute(&journal, selected, 99, &mut session).operation,
        Err(dispatcher::DispatchError::Guard(
            actual_guard::Error::RouteUnresolved | actual_guard::Error::Io
        ))
    ));
    assert_eq!(session.writes, 0);
}

#[test]
fn native_actual_guard_full_plan_reopen_replay_and_released_route() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let selected = input("native-complete");
    let journal = native_guard(&root);
    let mut session = Session::new(&selected);
    assert!(execute(&journal, selected.clone(), 1, &mut session).succeeded());
    assert_eq!((session.writes, session.reads), (2, 4));
    drop(journal);

    let intent = GuardStore::new(WindowsStorage::open(&root).unwrap())
        .intent(hash(selected.operation_id.as_bytes()), MAX_INTENT_BYTES)
        .unwrap()
        .unwrap();
    assert!(intent.len() > crate::MAX_RECORD_BYTES);
    let recovered: serde_json::Value = serde_json::from_slice(&intent).unwrap();
    assert_eq!(recovered["plan"], serde_json::to_value(&selected).unwrap());
    let reopened = native_guard(&root);
    assert_eq!(
        reopened.recovered_status(&selected.operation_id),
        Status::RestoredVerified
    );
    let mut replay = Session::new(&selected);
    assert!(matches!(
        execute(&reopened, selected, 2, &mut replay).operation,
        Err(dispatcher::DispatchError::Guard(
            actual_guard::Error::AlreadyConsumed
        ))
    ));
    assert_eq!(replay.writes, 0);
    let reused_approval = input("native-reused-approval");
    let mut approval_session = Session::new(&reused_approval);
    assert!(matches!(
        execute(&reopened, reused_approval, 1, &mut approval_session).operation,
        Err(dispatcher::DispatchError::Guard(
            actual_guard::Error::AlreadyConsumed
        ))
    ));
    assert_eq!(approval_session.writes, 0);
    let next = input("native-after-release");
    let mut next_session = Session::new(&next);
    assert!(execute(&reopened, next, 3, &mut next_session).succeeded());
}

#[derive(Clone, Copy)]
enum Fault {
    IntentPart,
    CleanupRecord,
    ReleaseHead,
}

/// Fault injection wraps the actual native adapter. Corrupt bytes pass through
/// its real create-new/set/flush API; no modeled registry or guard is substituted.
struct FaultStorage {
    native: WindowsStorage,
    fault: Fault,
    attempted: Arc<AtomicBool>,
    published: Arc<AtomicBool>,
}
struct FaultLease<'a> {
    native: WindowsLease,
    fault: Fault,
    attempted: &'a AtomicBool,
    published: &'a AtomicBool,
}
impl Storage for FaultStorage {
    type Lease<'a> = FaultLease<'a>;
    fn lock_route(&self, route: Digest) -> Result<Self::Lease<'_>, StorageError> {
        Ok(FaultLease {
            native: self.native.lock_route(route)?,
            fault: self.fault,
            attempted: &self.attempted,
            published: &self.published,
        })
    }
}
impl NamespaceStorage for FaultStorage {
    fn lock_records(&self) -> Result<Self::Lease<'_>, StorageError> {
        Ok(FaultLease {
            native: self.native.lock_records()?,
            fault: self.fault,
            attempted: &self.attempted,
            published: &self.published,
        })
    }
}
impl RouteLease for FaultLease<'_> {
    fn read_route(&mut self, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        self.native.read_route(limit)
    }
    fn set_route(&mut self, bytes: &[u8]) -> Result<(), StorageError> {
        let release = matches!(self.fault, Fault::ReleaseHead)
            && bytes.len() <= crate::MAX_RECORD_BYTES
            && serde_json::from_slice::<serde_json::Value>(bytes)
                .is_ok_and(|value| value["released"] == true);
        if release && !self.attempted.swap(true, Ordering::SeqCst) {
            self.native.set_route(b"{").unwrap();
            self.native.flush().unwrap();
            assert_eq!(
                self.native.read_route(crate::MAX_RECORD_BYTES).unwrap(),
                Some(b"{".to_vec())
            );
            self.published.store(true, Ordering::SeqCst);
            return Err(StorageError::Unavailable);
        }
        self.native.set_route(bytes)
    }
    fn read(&mut self, key: Key, limit: usize) -> Result<Option<Vec<u8>>, StorageError> {
        self.native.read(key, limit)
    }
    fn create_new(&mut self, key: Key, bytes: &[u8]) -> Result<(), StorageError> {
        let selected = matches!(
            (self.fault, key),
            (Fault::IntentPart, Key::GuardIntentPart(_, 0))
                | (
                    Fault::CleanupRecord,
                    Key::GuardEvidence(_, Evidence::CleanupConfirmed)
                )
        );
        if selected && !self.attempted.swap(true, Ordering::SeqCst) {
            self.native.create_new(key, b"{").unwrap();
            self.native.flush().unwrap();
            assert_eq!(
                self.native.read(key, crate::MAX_RECORD_BYTES).unwrap(),
                Some(b"{".to_vec())
            );
            self.published.store(true, Ordering::SeqCst);
            return Err(StorageError::Unavailable);
        }
        self.native.create_new(key, bytes)
    }
    fn flush(&mut self) -> Result<(), StorageError> {
        self.native.flush()
    }
}

#[test]
fn native_actual_guard_partial_intent_cleanup_and_release_fail_closed_after_reopen() {
    for fault in [Fault::IntentPart, Fault::CleanupRecord, Fault::ReleaseHead] {
        let directory = tempfile::tempdir().unwrap();
        let root = fs::canonicalize(directory.path()).unwrap();
        let attempted = Arc::new(AtomicBool::new(false));
        let published = Arc::new(AtomicBool::new(false));
        let storage = Arc::new(ActualGuardPersistence::new(FaultStorage {
            native: WindowsStorage::open(&root).unwrap(),
            fault,
            attempted: attempted.clone(),
            published: published.clone(),
        }));
        let journal = Journal::with_persistence(storage.clone()).unwrap();
        let selected = input("native-torn");
        let mut session = Session::new(&selected);
        assert!(!execute(&journal, selected, 4, &mut session).succeeded());
        // Selection alone proves nothing: require successful native publication,
        // checked flush and exact readback before the deliberately returned error.
        // Unexpected setup failures panic in the wrapper rather than imitating it.
        drop(journal);
        assert!(attempted.load(Ordering::SeqCst));
        assert!(published.load(Ordering::SeqCst));
        assert_eq!(
            session.writes,
            if matches!(fault, Fault::IntentPart) {
                0
            } else {
                2
            }
        );
        let reopened = native_guard(&root);
        assert_eq!(
            reopened.recovered_status("native-torn"),
            if matches!(fault, Fault::IntentPart) {
                Status::OutcomeUnknown
            } else {
                Status::RestoredVerified
            }
        );
        assert_quarantined(&root);
    }
}

fn marker(root: &Path, name: &str) {
    let path = root.join(name);
    let temporary = path.with_extension("tmp");
    fs::write(&temporary, b"ready").unwrap();
    fs::rename(temporary, path).unwrap();
}
fn wait_marker(root: &Path, name: &str) {
    let deadline = Instant::now() + TIMEOUT;
    while !root.join(name).is_file() {
        assert!(Instant::now() < deadline, "timed out waiting for {name}");
        std::thread::sleep(Duration::from_millis(10));
    }
}
struct ChildGuard(Child);
impl Drop for ChildGuard {
    fn drop(&mut self) {
        if matches!(self.0.try_wait(), Ok(Some(_))) {
            return;
        }
        if let Err(error) = self.0.kill() {
            eprintln!("synthetic child termination failed: {error}");
        }
        // Drop must not block indefinitely or panic during another assertion's
        // unwind. Failure to observe termination is reported, never claimed as
        // a successfully reaped child.
        if let Err(error) = reap_child(&mut self.0) {
            eprintln!("synthetic child was not confirmed reaped: {error}");
        }
    }
}
fn child(root: &Path, mode: &str, index: u8) -> ChildGuard {
    ChildGuard(
        Command::new(std::env::current_exe().unwrap())
            .args([
                "--exact",
                "guard_bridge_tests::native::native_actual_guard_child",
                "--nocapture",
            ])
            .env("OPENKNX_TASK156_GUARD_ROOT", root)
            .env("OPENKNX_TASK156_GUARD_MODE", mode)
            .env("OPENKNX_TASK156_GUARD_INDEX", index.to_string())
            .spawn()
            .unwrap(),
    )
}
fn reap_child(child: &mut Child) -> io::Result<ExitStatus> {
    let deadline = Instant::now() + TIMEOUT;
    loop {
        if let Some(status) = child.try_wait()? {
            return Ok(status);
        }
        if Instant::now() >= deadline {
            return Err(io::Error::new(
                io::ErrorKind::TimedOut,
                "synthetic child termination was not observed before the deadline",
            ));
        }
        std::thread::sleep(Duration::from_millis(10));
    }
}

#[test]
fn native_actual_guard_child() {
    let Some(root) = std::env::var_os("OPENKNX_TASK156_GUARD_ROOT") else {
        return;
    };
    let root = PathBuf::from(root);
    let mode = std::env::var("OPENKNX_TASK156_GUARD_MODE").unwrap();
    let index: u8 = std::env::var("OPENKNX_TASK156_GUARD_INDEX")
        .unwrap()
        .parse()
        .unwrap();
    let busy = Arc::new(AtomicBool::new(false));
    let journal =
        Journal::with_persistence(Arc::new(ActualGuardPersistence::new(ContentionStorage {
            native: WindowsStorage::open(&root).unwrap(),
            busy: busy.clone(),
        })))
        .unwrap();
    let mut selected = input(&format!("native-child-{index}"));
    selected.route.target = format!("synthetic-target-{index}");
    let mut session = Session::new(&selected);
    session.wait = true;
    let plan = Plan::new(selected.clone()).unwrap();
    let approval = actual_guard::synthetic_approval_for_test([index; 16], plan.digest(), 200);
    marker(&root, &format!("present-{index}"));
    wait_marker(&root, "start");
    {
        let mut future = pin!(dispatcher::execute(
            &journal,
            plan,
            Some(approval),
            &mut session,
            &FixedClock
        ));
        match future
            .as_mut()
            .poll(&mut Context::from_waker(Waker::noop()))
        {
            Poll::Pending => {
                // The only pending point is the first synthetic write, after
                // actual durable guard reservation. Keep that future alive.
                if mode == "holder" {
                    let storage = WindowsStorage::open(&root).unwrap();
                    let _lease = storage.lock_records().unwrap();
                    marker(&root, "holding");
                    wait_marker(&root, "finish");
                } else {
                    marker(&root, &format!("winner-{index}"));
                    wait_marker(&root, "finish");
                }
            }
            Poll::Ready(report) => {
                assert_eq!(mode, "race");
                match report.operation {
                    Err(dispatcher::DispatchError::Guard(actual_guard::Error::RouteUnresolved)) => {
                        assert!(!busy.load(Ordering::SeqCst));
                    }
                    Err(dispatcher::DispatchError::Guard(actual_guard::Error::Io)) => {
                        assert!(
                            busy.load(Ordering::SeqCst),
                            "Io must originate from actual native lock contention"
                        );
                    }
                    other => panic!("unexpected race-loser result: {other:?}"),
                }
                marker(&root, &format!("loser-{index}"));
            }
        }
    }
    assert_eq!(
        session.writes,
        usize::from(root.join(format!("winner-{index}")).exists())
    );
    marker(&root, &format!("done-{index}"));
}

#[test]
fn native_actual_guard_competing_processes_have_one_synthetic_dispatch() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let first = child(&root, "race", 11);
    wait_marker(&root, "present-11");
    let second = child(&root, "race", 12);
    wait_marker(&root, "present-12");
    let mut children = [first, second];
    marker(&root, "start");
    let deadline = Instant::now() + TIMEOUT;
    while [11, 12].iter().any(|index| {
        !root.join(format!("winner-{index}")).exists()
            && !root.join(format!("loser-{index}")).exists()
    }) {
        assert!(
            Instant::now() < deadline,
            "children did not publish outcomes"
        );
        std::thread::sleep(Duration::from_millis(10));
    }
    assert_eq!(
        [11, 12]
            .iter()
            .filter(|index| root.join(format!("winner-{index}")).exists())
            .count(),
        1
    );
    marker(&root, "finish");
    for child in &mut children {
        assert!(reap_child(&mut child.0).unwrap().success());
    }
    assert_quarantined(&root);
}

#[test]
fn native_actual_guard_abrupt_holder_death_releases_os_lock_but_not_route() {
    let directory = tempfile::tempdir().unwrap();
    let root = fs::canonicalize(directory.path()).unwrap();
    let mut child = child(&root, "holder", 13);
    wait_marker(&root, "present-13");
    marker(&root, "start");
    wait_marker(&root, "holding");
    let storage = WindowsStorage::open(&root).unwrap();
    assert!(matches!(
        storage.lock_route(route(&input("probe"))),
        Err(StorageError::Busy)
    ));
    child.0.kill().unwrap();
    assert!(!reap_child(&mut child.0).unwrap().success());
    drop(storage.lock_records().unwrap());
    let reopened = native_guard(&root);
    assert_eq!(
        reopened.recovered_status("native-child-13"),
        Status::OutcomeUnknown
    );
    assert_quarantined(&root);
}
