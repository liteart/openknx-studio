// SPDX-License-Identifier: Apache-2.0
//! Development tests only: all grants/identities/data are synthetic. The fork-owned
//! process is the only peer; this file contains no KNX wire representation.
use super::*;
use crate::guard::dispatcher::{self, CleanupEvidence, Clock, SessionError, TypedSession};
use crate::knust_adapter::{KnustAdapter, descriptor_identity};
use knust::protocol::{DeviceDescriptorResponse, IndividualAddress};
use std::future::Future;
use std::io::{BufRead, BufReader, Write};
use std::process::{Child, ChildStdin, Command, Stdio};
use std::task::{Context, Poll, Waker};
use std::time::Duration;
use tokio::sync::mpsc;
use tokio::time::{sleep, timeout};

const ARTIFACTS: &str = env!("TASK020_TEST_ARTIFACT_DIR");
const SIMULATOR: &str = env!("TASK020_TEST_SIMULATOR_BIN");
struct FixedClock;
impl Clock for FixedClock {
    fn now(&self) -> u64 {
        100
    }
}

pub(super) struct Peer {
    child: Child,
    _controls: ChildStdin,
    lines: mpsc::Receiver<String>,
    events: Vec<String>,
    pub(super) endpoint: String,
}
impl Drop for Peer {
    fn drop(&mut self) {
        let _ = self.child.kill();
        let _ = self.child.wait();
    }
}
impl Peer {
    async fn start(case: &str, mode: &str) -> Self {
        Self::start_profile(case, mode, "1.1.42", "127").await
    }
    pub(super) async fn start_profile(
        case: &str,
        mode: &str,
        target: &str,
        property: &str,
    ) -> Self {
        let stderr = File::create(format!("{ARTIFACTS}/e2e-{case}.stderr.log")).unwrap();
        let mut child = Command::new(SIMULATOR)
            .args([
                "--loopback-simulator",
                "--target",
                target,
                "--client",
                "1.1.254",
                "--mask",
                "0705",
                "--object-index",
                "0",
                "--property-id",
                property,
                "--start-index",
                "1",
                "--count",
                "1",
                "--element-size",
                "1",
                "--initial",
                "00",
                "--write-mode",
                mode,
                "--lifetime-ms",
                "20000",
            ])
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .stderr(stderr)
            .spawn()
            .unwrap();
        let stdout = child.stdout.take().unwrap();
        let controls = child.stdin.take().unwrap();
        let (tx, lines) = mpsc::channel(128);
        let log_path = format!("{ARTIFACTS}/e2e-{case}.sim.log");
        std::thread::spawn(move || {
            let mut log = File::create(log_path).unwrap();
            for (index, line) in BufReader::new(stdout).lines().enumerate() {
                assert!(index < 132, "simulator output bound");
                let line = line.unwrap();
                assert!(line.len() <= 512, "simulator line bound");
                writeln!(log, "{line}").unwrap();
                log.flush().unwrap();
                let _ = tx.blocking_send(line);
            }
            log.sync_all().unwrap();
        });
        let mut peer = Self {
            child,
            _controls: controls,
            lines,
            events: vec![],
            endpoint: String::new(),
        };
        let ready = peer.next().await;
        let endpoint = ready
            .strip_prefix("SIMULATOR_READY endpoint=")
            .unwrap()
            .strip_suffix(" synthetic=true")
            .unwrap();
        let parsed: std::net::SocketAddrV4 = endpoint.parse().unwrap();
        assert!(parsed.ip().is_loopback());
        assert_ne!(parsed.port(), 0);
        peer.endpoint = endpoint.to_owned();
        peer
    }
    async fn next(&mut self) -> String {
        let line = timeout(Duration::from_secs(10), self.lines.recv())
            .await
            .unwrap()
            .expect("simulator output ended");
        self.events.push(line.clone());
        line
    }
    async fn until(&mut self, marker: &str) {
        loop {
            if self.next().await.contains(marker) {
                break;
            }
        }
    }
    pub(super) async fn finish(&mut self) {
        self.until("SIMULATOR_DONE").await;
        let status = timeout(Duration::from_secs(2), async {
            loop {
                if let Some(status) = self.child.try_wait().unwrap() {
                    break status;
                }
                sleep(Duration::from_millis(10)).await;
            }
        })
        .await
        .unwrap();
        assert!(status.success(), "simulator process failed");
        let events: Vec<_> = self
            .events
            .iter()
            .filter(|line| line.starts_with("SIM_EVENT "))
            .collect();
        for (index, event) in events.iter().enumerate() {
            assert!(event.starts_with(&format!("SIM_EVENT id={} ", index + 1)));
        }
        let device = self
            .events
            .iter()
            .position(|line| line.contains("kind=DEVICE_DISCONNECTED"))
            .unwrap();
        let gateway = self
            .events
            .iter()
            .position(|line| line.contains("kind=GATEWAY_DISCONNECTED device_open=false"))
            .unwrap();
        assert!(device < gateway);
    }
    pub(super) fn selected(&self, marker: &str) -> Vec<&str> {
        self.events
            .iter()
            .filter(|line| line.contains(marker))
            .map(String::as_str)
            .collect()
    }
}

fn input(peer: &Peer, case: &str) -> PlanInput {
    let target: IndividualAddress = "1.1.42".parse().unwrap();
    let identity = descriptor_identity(
        target,
        &DeviceDescriptorResponse {
            descriptor: 0,
            data: vec![7, 5],
        },
    )
    .unwrap();
    PlanInput {
        operation_id: case.into(),
        route: Route {
            gateway: peer.endpoint.clone(),
            bus_path: "knxnetip-udp".into(),
            target: target.to_string(),
            identity,
        },
        selector: Selector {
            object: 0,
            property: 127,
            index: 1,
            count: 1,
            element_size: 1,
        },
        original: vec![0],
        desired: vec![4],
        purpose: "synthetic owned-loopback write/read/restore".into(),
        recovery: "synthetic restore to supplied original 00".into(),
        expires_at_utc_seconds: 200,
    }
}
fn approval(plan: &Plan, id: u8) -> HumanApproval {
    // Descendant of the generated guard harness, compiled only under cfg(test).
    // No production constructor, CLI, file grant or executable is introduced.
    HumanApproval {
        id: [id; 16],
        plan_digest: plan.digest(),
        expires_at_utc_seconds: 200,
    }
}
fn journal(case: &str) -> Journal {
    let dir = tempfile::Builder::new()
        .prefix(&format!("e2e-{case}-journal-"))
        .tempdir_in(ARTIFACTS)
        .unwrap()
        .keep();
    let root = std::fs::canonicalize(dir).unwrap();
    Journal::open(&root).unwrap()
}
fn assert_quarantined(journal: &Journal, input: &PlanInput) {
    assert_eq!(
        journal.recovered_status(&input.operation_id),
        Status::OutcomeUnknown
    );
    let mut next = input.clone();
    next.operation_id.push_str("-next");
    let plan = Plan::new(next).unwrap();
    let grant = approval(&plan, 99);
    let observed = Observation {
        route: plan.input.route.clone(),
        selector: plan.input.selector.clone(),
        value: vec![0],
        observed_at_utc_seconds: 100,
        receipt: 99,
    };
    let reopened = Journal::open(&journal.root).unwrap();
    assert!(matches!(
        reopened.begin(plan, Some(grant), &observed, 100),
        Err(Error::RouteUnresolved)
    ));
}

#[tokio::test(flavor = "current_thread")]
async fn actual_actor_roundtrip_has_independent_reads_and_distinct_cleanup() {
    timeout(Duration::from_secs(15), async {
        let mut peer = Peer::start("normal", "normal").await;
        let input = input(&peer, "actual-actor-normal");
        let journal = journal("normal");
        let plan = Plan::new(input.clone()).unwrap();
        let grant = approval(&plan, 1);
        let mut adapter = KnustAdapter::connect(input.route.clone()).await.unwrap();
        let report =
            dispatcher::execute(&journal, plan, Some(grant), &mut adapter, &FixedClock).await;
        assert!(report.succeeded(), "{report:?}");
        peer.finish().await;
        let reads = peer.selected("kind=PROPERTY_READ");
        assert_eq!(reads.len(), 4);
        for (read, value) in reads.iter().zip(["00", "00", "04", "00"]) {
            assert!(read.ends_with(&format!("value={value}")));
        }
        assert_eq!(peer.selected("kind=DESCRIPTOR_READ").len(), 4);
        let writes = peer.selected("kind=WRITE_APPLIED");
        assert_eq!(writes.len(), 2);
        assert!(writes[0].contains("value=04"));
        assert!(writes[1].contains("value=00"));
        assert!(
            peer.events
                .last()
                .unwrap()
                .contains("reason=gateway_closed value=00 writes=2 device_open=false")
        );
        let operations: Vec<_> = peer
            .events
            .iter()
            .filter(|line| {
                line.contains("kind=PROPERTY_READ") || line.contains("kind=WRITE_APPLIED")
            })
            .map(|line| line.split("kind=").nth(1).unwrap().to_owned())
            .collect();
        assert!(operations[2].starts_with("WRITE_APPLIED"));
        assert!(operations[3].starts_with("PROPERTY_READ value=04"));
        assert!(operations[4].starts_with("WRITE_APPLIED"));
        assert!(operations[5].starts_with("PROPERTY_READ value=00"));
        assert_eq!(
            Journal::open(&journal.root)
                .unwrap()
                .recovered_status(&input.operation_id),
            Status::RestoredVerified
        );
        let check = Plan::new(input).unwrap();
        assert!(
            !journal
                .path(&Journal::route_key(&check).unwrap(), "unresolved-route")
                .exists()
        );
    })
    .await
    .unwrap();
}

async fn cancelled_actor(mode: &str, marker: &str, value: &str, writes: usize) {
    let mut peer = Peer::start(mode, mode).await;
    let input = input(&peer, mode);
    let journal = journal(mode);
    let plan = Plan::new(input.clone()).unwrap();
    let grant = approval(&plan, 2);
    let mut adapter = KnustAdapter::connect(input.route.clone()).await.unwrap();
    {
        let execute = dispatcher::execute(&journal, plan, Some(grant), &mut adapter, &FixedClock);
        tokio::pin!(execute);
        tokio::select! {
            report = &mut execute => panic!("held request unexpectedly returned: {report:?}"),
            () = peer.until(marker) => {}
        }
    } // Cancel after actual peer observation: no automatic restore follows.
    assert_quarantined(&journal, &input);
    // Cleanup may happen in the actor, but a dropped execution cannot create
    // a complete receipt or release quarantine on a later close call.
    assert_eq!(
        timeout(Duration::from_secs(9), adapter.close())
            .await
            .unwrap(),
        Err(SessionError::Uncertain)
    );
    peer.finish().await;
    assert_eq!(peer.selected("kind=WRITE_APPLIED").len(), writes);
    assert_eq!(peer.selected("kind=PROPERTY_READ").len(), 2);
    assert!(
        peer.events
            .last()
            .unwrap()
            .contains(&format!("value={value} writes={writes} device_open=false"))
    );
    assert_quarantined(&journal, &input);
}
#[tokio::test(flavor = "current_thread")]
async fn actual_actor_cancel_before_effect_never_restores_or_retries() {
    timeout(
        Duration::from_secs(15),
        cancelled_actor("hold-before", "kind=WRITE_PENDING", "00", 0),
    )
    .await
    .unwrap();
}
#[tokio::test(flavor = "current_thread")]
async fn actual_actor_cancel_after_effect_never_restores_or_retries() {
    timeout(
        Duration::from_secs(15),
        cancelled_actor("hold-after", "kind=WRITE_INFLIGHT", "04", 1),
    )
    .await
    .unwrap();
}

#[tokio::test(flavor = "current_thread")]
async fn actual_actor_bounded_write_uncertainty_keeps_quarantine() {
    timeout(Duration::from_secs(15), async {
        let mut peer = Peer::start("deadline", "hold-after").await;
        let input = input(&peer, "deadline");
        let journal = journal("deadline");
        let plan = Plan::new(input.clone()).unwrap();
        let grant = approval(&plan, 3);
        let mut adapter = KnustAdapter::connect(input.route.clone()).await.unwrap();
        let report =
            dispatcher::execute(&journal, plan, Some(grant), &mut adapter, &FixedClock).await;
        assert!(!report.succeeded());
        assert!(report.operation.is_err());
        assert_ne!(
            report.cleanup,
            Some(Ok(CleanupEvidence::DeviceAndGatewayConfirmed))
        );
        peer.finish().await;
        assert_eq!(peer.selected("kind=WRITE_APPLIED").len(), 1);
        assert_eq!(peer.selected("kind=PROPERTY_READ").len(), 2);
        assert!(
            peer.events
                .last()
                .unwrap()
                .contains("value=04 writes=1 device_open=false")
        );
        assert_quarantined(&journal, &input);
    })
    .await
    .unwrap();
}

#[tokio::test(flavor = "current_thread")]
async fn actual_actor_cancelled_queued_write_never_reaches_peer() {
    timeout(Duration::from_secs(15), async {
        let mut peer = Peer::start("queued", "normal").await;
        let input = input(&peer, "queued");
        let journal = journal("queued");
        let plan = Plan::new(input.clone()).unwrap();
        let grant = approval(&plan, 4);
        let mut adapter = KnustAdapter::connect(input.route.clone()).await.unwrap();
        let read = adapter.observe(&input.selector).await.unwrap();
        let observed = Observation {
            route: read.route,
            selector: read.selector,
            value: read.value,
            observed_at_utc_seconds: 100,
            receipt: 1,
        };
        let mut attempt = journal.begin(plan, Some(grant), &observed, 100).unwrap();
        let selected = attempt
            .dispatch_desired(&observed, 100)
            .unwrap()
            .bind_for_execution(&attempt.plan)
            .unwrap();
        {
            let write = adapter.write(selected);
            tokio::pin!(write);
            let mut context = Context::from_waker(Waker::noop());
            assert!(matches!(write.as_mut().poll(&mut context), Poll::Pending));
        } // Current-thread runtime: queued request exists, actor has not run.
        attempt
            .record_write_response(WriteObservation::OutcomeUnknown, None, 100)
            .unwrap();
        assert_eq!(adapter.close().await, Err(SessionError::Uncertain));
        peer.finish().await;
        assert!(peer.selected("kind=WRITE_APPLIED").is_empty());
        assert!(peer.selected("kind=WRITE_PENDING").is_empty());
        assert_eq!(peer.selected("kind=PROPERTY_READ").len(), 1);
        assert!(
            peer.events
                .last()
                .unwrap()
                .contains("value=00 writes=0 device_open=false")
        );
        assert_quarantined(&journal, &input);
    })
    .await
    .unwrap();
}
