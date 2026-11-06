// SPDX-License-Identifier: Apache-2.0
//! Registered ONLY as a private Task020 guard child by the exact-source runner.
//! The shipping/default broker has no executor or qualification constructor.
use super::dispatcher::{self, Clock, ExecutionReport};
use super::{HumanApproval, Journal, Plan};
use crate::{ConfiguredPlan, knust_adapter::KnustAdapter, root_passkey::PersistedAssertion};
use std::path::Path;
use std::time::{SystemTime, UNIX_EPOCH};

const JOURNAL: &str = "/private/var/db/org.openknxstudio.operator/execution";
// No variant/constructor exists: source/package preparation cannot establish
// independently qualified installation, principal and browser/profile trust.
enum MissingHostQualification {}
pub(crate) struct QualifiedExecution {
    _qualification: MissingHostQualification,
}
#[derive(Debug, PartialEq, Eq)]
pub(crate) enum Error {
    UnqualifiedHost,
    PlanMismatch,
    Expired,
    JournalUnavailable,
    SessionUnavailable,
}
struct WallClock;
impl Clock for WallClock {
    fn now(&self) -> u64 {
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .map(|time| time.as_secs())
            .unwrap_or(u64::MAX)
    }
}

/// No production caller can construct the required host qualification. This is
/// the real typed path for later integration, never an agent/MCP grant API.
pub(crate) async fn execute_qualified(
    configured: &ConfiguredPlan,
    witness: PersistedAssertion,
    _qualified: QualifiedExecution,
) -> Result<ExecutionReport, Error> {
    if nix::unistd::geteuid().as_raw() != 0 || nix::unistd::getegid().as_raw() != 0 {
        return Err(Error::UnqualifiedHost);
    }
    let directory = crate::macos_config::checked_fresh_execution_journal(Path::new(JOURNAL))
        .map_err(|_| Error::JournalUnavailable)?;
    // Every path component is root-owned and denies ordinary writes/allow ACLs;
    // root-only 0700 leaf confines the existing guard's path-based files. The
    // checked descriptor stays alive through execution. Malicious root/another
    // privileged actor is outside that provisioning assumption.
    let journal = Journal::open(Path::new(JOURNAL)).map_err(|_| Error::JournalUnavailable)?;
    let result = execute_selected(&journal, configured, witness, &WallClock).await;
    drop(directory);
    result
}

async fn execute_selected<C: Clock>(
    journal: &Journal,
    configured: &ConfiguredPlan,
    witness: PersistedAssertion,
    clock: &C,
) -> Result<ExecutionReport, Error> {
    let input: super::PlanInput = crate::root_passkey::unique_json(configured.summary().as_bytes())
        .map_err(|_| Error::PlanMismatch)?;
    let target: knust::protocol::IndividualAddress =
        "1.1.231".parse().map_err(|_| Error::PlanMismatch)?;
    let descriptor = knust::protocol::DeviceDescriptorResponse {
        descriptor: 0,
        data: vec![7, 5],
    };
    let identity = crate::knust_adapter::descriptor_identity(target, &descriptor)
        .map_err(|_| Error::PlanMismatch)?;
    if input.route.target != "1.1.231"
        || input.route.bus_path != "knxnetip-udp"
        || input.route.identity != identity
        || input.selector.object != 0
        || input.selector.property != 14
        || input.selector.index != 1
        || input.selector.count != 1
        || input.selector.element_size != 1
        || input.original != [0]
        || input.desired != [4]
    {
        return Err(Error::PlanMismatch);
    }
    let (id, digest, expires) = witness.consume_for_guard();
    let plan = Plan::new(input.clone()).map_err(|_| Error::PlanMismatch)?;
    if id == [0; 16] || digest != configured.digest() || digest != plan.digest() {
        return Err(Error::PlanMismatch);
    }
    if clock.now() >= expires || clock.now() >= input.expires_at_utc_seconds {
        return Err(Error::Expired);
    }
    // The child module is within guard's private field boundary. No general
    // constructor is exported: only the consuming durable witness reaches here.
    let approval = HumanApproval {
        id,
        plan_digest: digest,
        expires_at_utc_seconds: expires,
    };
    let mut adapter = KnustAdapter::connect(input.route)
        .await
        .map_err(|_| Error::SessionUnavailable)?;
    Ok(dispatcher::execute(journal, plan, Some(approval), &mut adapter, clock).await)
}

#[cfg(test)]
mod tests {
    use super::super::actor_tests::Peer;
    use super::*;
    struct FixedClock;
    impl Clock for FixedClock {
        fn now(&self) -> u64 {
            101
        }
    }
    fn fixture_input(gateway: &str) -> task152_passkey_prototype::guard::PlanInput {
        let target = "1.1.231".parse().unwrap();
        let identity = crate::knust_adapter::descriptor_identity(
            target,
            &knust::protocol::DeviceDescriptorResponse {
                descriptor: 0,
                data: vec![7, 5],
            },
        )
        .unwrap();
        task152_passkey_prototype::guard::PlanInput {
            operation_id: "synthetic-broker-execution".into(),
            route: task152_passkey_prototype::guard::Route {
                gateway: gateway.into(),
                bus_path: "knxnetip-udp".into(),
                target: "1.1.231".into(),
                identity,
            },
            selector: task152_passkey_prototype::guard::Selector {
                object: 0,
                property: 14,
                index: 1,
                count: 1,
                element_size: 1,
            },
            original: vec![0],
            desired: vec![4],
            purpose: "Synthetic verified broker execution".into(),
            recovery: "Synthetic restoration".into(),
            expires_at_utc_seconds: 1000,
        }
    }
    #[tokio::test(flavor = "current_thread")]
    async fn changed_plan_and_expired_witness_fail_before_any_udp_contact() {
        struct ExpiredClock;
        impl Clock for ExpiredClock {
            fn now(&self) -> u64 {
                221
            }
        }
        for expired in [false, true] {
            let probe = std::net::UdpSocket::bind("127.0.0.1:0").unwrap();
            probe.set_nonblocking(true).unwrap();
            let input = fixture_input(&probe.local_addr().unwrap().to_string());
            let configured = ConfiguredPlan::new(input.clone()).unwrap();
            let directory = tempfile::tempdir_in("/private/tmp").unwrap();
            let parent = std::fs::File::open(directory.path()).unwrap();
            let witness =
                crate::root_passkey::tests::persisted_assertion_fixture(&configured, &parent);
            let journal = Journal::open(directory.path()).unwrap();
            let result = if expired {
                execute_selected(&journal, &configured, witness, &ExpiredClock).await
            } else {
                let mut changed = input;
                changed.purpose.push_str(" changed");
                execute_selected(
                    &journal,
                    &ConfiguredPlan::new(changed).unwrap(),
                    witness,
                    &FixedClock,
                )
                .await
            };
            assert!(
                matches!(result, Err(Error::Expired)) && expired
                    || matches!(result, Err(Error::PlanMismatch)) && !expired
            );
            assert_eq!(
                probe.recv(&mut [0; 1]).unwrap_err().kind(),
                std::io::ErrorKind::WouldBlock
            );
        }
    }
    #[tokio::test(flavor = "current_thread")]
    async fn verified_persisted_assertion_drives_real_typed_loopback_adapter_and_guard() {
        let mut peer = Peer::start_profile("broker-verified", "normal", "1.1.231", "14").await;
        let gateway: std::net::SocketAddrV4 = peer.endpoint.parse().unwrap();
        assert!(gateway.ip().is_loopback());
        let input = fixture_input(&peer.endpoint);
        let configured = ConfiguredPlan::new(input).unwrap();
        let directory = tempfile::tempdir_in("/private/tmp").unwrap();
        let parent = std::fs::File::open(directory.path()).unwrap();
        let witness = crate::root_passkey::tests::persisted_assertion_fixture(&configured, &parent);
        let journal = Journal::open(directory.path()).unwrap();
        // This private test helper deliberately uses an ordinary owned fixture,
        // never constructs QualifiedExecution and contacts only the owned peer.
        let report = execute_selected(&journal, &configured, witness, &FixedClock)
            .await
            .unwrap();
        assert!(report.succeeded(), "{report:?}");
        peer.finish().await;
        assert_eq!(peer.selected("kind=WRITE_APPLIED").len(), 2);
        assert_eq!(peer.selected("kind=PROPERTY_READ").len(), 4);
        assert_eq!(peer.selected("kind=DESCRIPTOR_READ").len(), 4);
        assert_eq!(
            Journal::open(directory.path())
                .unwrap()
                .recovered_status("synthetic-broker-execution"),
            super::super::Status::RestoredVerified
        );
    }
}
