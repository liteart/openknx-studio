// SPDX-License-Identifier: Apache-2.0
//! Development-only adapter against exact fork revision 4d296af.
//! Not registered by the root package; no executable or real grant issuer.

use crate::guard::dispatcher::{CleanupEvidence, SessionError, SessionRead, TypedSession};
use crate::guard::{BoundDispatch, Route, Selector};
use knust::protocol::{
    DeviceDescriptorResponse, IndividualAddress, PropertyValueRead, PropertyValueWrite,
};
use knust::transport::{
    Tunnel,
    gateway_disconnect::{GatewayDisconnectAttempt, GatewayDisconnectStatus},
    management_session::{ConnectedPropertyWrite, ManagementSession},
};
use sha2::{Digest, Sha256};
use std::{net::SocketAddrV4, time::Duration};
use tokio::{
    sync::{mpsc, oneshot},
    task::JoinHandle,
    time::timeout,
};

type Reply<T> = oneshot::Sender<Result<T, SessionError>>;
enum Command {
    Observe {
        selector: Selector,
        reply: Reply<SessionRead>,
    },
    Write {
        dispatch: BoundDispatch,
        reply: Reply<SessionRead>,
    },
    Close {
        reply: Reply<CleanupEvidence>,
    },
}

/// Evidence digest, not a wire encoding, authentication or unique identity.
pub(crate) fn descriptor_identity(
    target: IndividualAddress,
    descriptor: &DeviceDescriptorResponse,
) -> Result<[u8; 32], SessionError> {
    let evidence = serde_json::to_vec(&(
        "task020-dd0-v1",
        target.to_string(),
        descriptor.descriptor,
        &descriptor.data,
    ))
    .map_err(|_| SessionError::InvalidSelection)?;
    Ok(Sha256::digest(evidence).into())
}

/// One command task owns the actual tunnel and scoped device session. Capacity
/// one and non-clone port prevent parallel operations through this adapter.
pub(crate) struct KnustAdapter {
    route: Route,
    commands: mpsc::Sender<Command>,
    worker: Option<JoinHandle<()>>,
    closing: bool,
}
impl KnustAdapter {
    pub(crate) async fn connect(route: Route) -> Result<Self, SessionError> {
        let gateway: SocketAddrV4 = route
            .gateway
            .parse()
            .map_err(|_| SessionError::InvalidSelection)?;
        let target: IndividualAddress = route
            .target
            .parse()
            .map_err(|_| SessionError::InvalidSelection)?;
        if gateway.to_string() != route.gateway
            || target.to_string() != route.target
            || target.raw() == 0
            || gateway.port() == 0
            || gateway.ip().is_unspecified()
            || gateway.ip().is_multicast()
            || gateway.ip().is_broadcast()
            || route.bus_path != "knxnetip-udp"
            || route.identity == [0; 32]
        {
            return Err(SessionError::InvalidSelection);
        }
        let (commands, receiver) = mpsc::channel(1);
        let (ready_tx, ready_rx) = oneshot::channel();
        let worker = tokio::spawn(run(route.clone(), gateway, target, receiver, ready_tx));
        if !matches!(
            timeout(Duration::from_secs(30), ready_rx).await,
            Ok(Ok(Ok(())))
        ) {
            drop(commands); // No app command exists; actor finishes bounded cleanup.
            return Err(SessionError::Uncertain);
        }
        Ok(Self {
            route,
            commands,
            worker: Some(worker),
            closing: false,
        })
    }
}

impl TypedSession for KnustAdapter {
    fn route(&self) -> &Route {
        &self.route
    }
    async fn observe(&mut self, selector: &Selector) -> Result<SessionRead, SessionError> {
        if self.closing {
            return Err(SessionError::Uncertain);
        }
        let (reply, received) = oneshot::channel();
        self.commands
            .send(Command::Observe {
                selector: selector.clone(),
                reply,
            })
            .await
            .map_err(|_| SessionError::Uncertain)?;
        received.await.map_err(|_| SessionError::Uncertain)?
    }
    async fn write(&mut self, dispatch: BoundDispatch) -> Result<SessionRead, SessionError> {
        if self.closing || dispatch.route() != &self.route {
            return Err(SessionError::InvalidSelection);
        }
        let (reply, received) = oneshot::channel();
        self.commands
            .send(Command::Write { dispatch, reply })
            .await
            .map_err(|_| SessionError::Uncertain)?;
        // Cancellation after handoff cannot prove the command was not sent.
        // Durable unknown intent was already recorded before this await.
        received.await.map_err(|_| SessionError::Uncertain)?
    }
    async fn close(&mut self) -> Result<CleanupEvidence, SessionError> {
        if self.closing {
            return Err(SessionError::Uncertain);
        }
        self.closing = true; // Cancellation cannot cause a second close request.
        let (reply, received) = oneshot::channel();
        self.commands
            .send(Command::Close { reply })
            .await
            .map_err(|_| SessionError::Uncertain)?;
        let cleanup = received.await.map_err(|_| SessionError::Uncertain)?;
        if let Some(worker) = self.worker.take() {
            worker.await.map_err(|_| SessionError::Uncertain)?;
        }
        cleanup
    }
}

async fn run(
    route: Route,
    gateway: SocketAddrV4,
    target: IndividualAddress,
    mut commands: mpsc::Receiver<Command>,
    ready: Reply<()>,
) {
    let mut tunnel = Tunnel::new_udp_with_timeout(gateway.into(), Duration::from_secs(10));
    if !matches!(
        timeout(Duration::from_secs(11), tunnel.connect()).await,
        Ok(Ok(()))
    ) {
        let _ = ready.send(Err(SessionError::Uncertain));
        return; // Unknown channel is never guessed and connect is never retried.
    }
    let expected_channel = tunnel.channel_id();
    let mut close_reply = None;
    let device_confirmed = {
        let mut last_observed = None;
        match ManagementSession::new(&mut tunnel, target) {
            Err(_) => {
                let _ = ready.send(Err(SessionError::Uncertain));
                false
            }
            Ok(mut session) => {
                let opened = session.connect(Duration::from_secs(6)).await.is_ok();
                let delivered = ready
                    .send(if opened {
                        Ok(())
                    } else {
                        Err(SessionError::Uncertain)
                    })
                    .is_ok();
                if opened && delivered {
                    // Local queue-idle bound only; never extends the device's
                    // independently enforced transport inactivity interval.
                    while let Ok(Some(command)) =
                        timeout(Duration::from_secs(5), commands.recv()).await
                    {
                        let keep_running = match command {
                            Command::Observe { selector, reply } => {
                                if reply.is_closed() {
                                    break;
                                }
                                let result = observe(&mut session, &route, target, &selector).await;
                                if let Ok(value) = &result {
                                    last_observed = Some(value.route.clone());
                                }
                                let good = result.is_ok();
                                good && reply.send(result).is_ok()
                            }
                            Command::Write { dispatch, reply } => {
                                if reply.is_closed() {
                                    break;
                                }
                                let result = write(
                                    &mut session,
                                    &route,
                                    target,
                                    last_observed.as_ref(),
                                    dispatch,
                                )
                                .await;
                                let good = result.is_ok();
                                good && reply.send(result).is_ok()
                            }
                            Command::Close { reply } => {
                                close_reply = Some(reply);
                                break;
                            }
                        };
                        if !keep_running {
                            break;
                        }
                    }
                }
                session.close(Duration::from_secs(6)).await.is_ok()
            }
        }
    }; // Every device-session borrow has ended before owning gateway close.
    let mut close = GatewayDisconnectAttempt::new(tunnel);
    let remote = close.execute(Duration::from_secs(1)).await;
    let gateway_confirmed = matches!(remote,Ok(confirmation) if confirmation.channel()==expected_channel)
        && close.status()
            == (GatewayDisconnectStatus::RemoteConfirmed {
                channel: expected_channel,
            });
    let result = cleanup_evidence(device_confirmed, gateway_confirmed);
    if let Some(reply) = close_reply {
        let _ = reply.send(result);
    }
    // No legacy disconnect, second request or retry follows this owning attempt.
}

fn cleanup_evidence(
    device_confirmed: bool,
    gateway_confirmed: bool,
) -> Result<CleanupEvidence, SessionError> {
    match (device_confirmed, gateway_confirmed) {
        (true, true) => Ok(CleanupEvidence::DeviceAndGatewayConfirmed),
        (true, false) => Ok(CleanupEvidence::DeviceOnly),
        _ => Err(SessionError::Uncertain),
    }
}

async fn observe(
    session: &mut ManagementSession<'_>,
    route: &Route,
    target: IndividualAddress,
    selector: &Selector,
) -> Result<SessionRead, SessionError> {
    let request = PropertyValueRead::new(
        selector.object,
        selector.property,
        selector.index,
        selector.count,
        selector.element_size,
    )
    .map_err(|_| SessionError::InvalidSelection)?;
    let descriptor = session
        .read_descriptor(Duration::from_secs(5))
        .await
        .map_err(|_| SessionError::Uncertain)?;
    let mut observed = route.clone();
    observed.identity = descriptor_identity(target, &descriptor)?;
    let value = session
        .read_property(request, Duration::from_secs(5))
        .await
        .map_err(|_| SessionError::Uncertain)?;
    if value.request != request {
        return Err(SessionError::Uncertain);
    }
    Ok(SessionRead {
        route: observed,
        selector: selector.clone(),
        value: value.data,
    })
}

async fn write(
    session: &mut ManagementSession<'_>,
    route: &Route,
    target: IndividualAddress,
    last_observed: Option<&Route>,
    dispatch: BoundDispatch,
) -> Result<SessionRead, SessionError> {
    if dispatch.route() != route || last_observed != Some(dispatch.route()) {
        return Err(SessionError::InvalidSelection);
    }
    let selector = dispatch.selector().clone();
    let request = PropertyValueWrite::new(
        selector.object,
        selector.property,
        selector.index,
        selector.count,
        selector.element_size,
        dispatch.bytes().to_vec(),
    )
    .map_err(|_| SessionError::InvalidSelection)?;
    let selection = request.selection();
    let mut attempt = ConnectedPropertyWrite::new(target, request);
    let result = session
        .write_property(&mut attempt, Duration::from_secs(5))
        .await
        .map_err(|_| SessionError::Uncertain)?;
    if result.request != selection || result.data != dispatch.bytes() {
        return Err(SessionError::Uncertain);
    }
    Ok(SessionRead {
        route: route.clone(),
        selector,
        value: result.data,
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn complete_cleanup_requires_both_independent_confirmations() {
        assert_eq!(
            cleanup_evidence(true, true),
            Ok(CleanupEvidence::DeviceAndGatewayConfirmed)
        );
        assert_eq!(
            cleanup_evidence(true, false),
            Ok(CleanupEvidence::DeviceOnly)
        );
        assert_eq!(cleanup_evidence(false, true), Err(SessionError::Uncertain));
        assert_eq!(cleanup_evidence(false, false), Err(SessionError::Uncertain));
    }

    fn synthetic_route() -> Route {
        Route {
            gateway: "synthetic-gateway".into(),
            bus_path: "synthetic-path".into(),
            target: "synthetic-target".into(),
            identity: [7; 32],
        }
    }

    #[tokio::test(flavor = "current_thread")]
    async fn close_port_preserves_incomplete_cleanup_and_is_one_use() {
        // This mock worker never calls run(), opens sockets or mints approval.
        let (commands, mut receiver) = mpsc::channel(1);
        let worker = tokio::spawn(async move {
            let Some(Command::Close { reply }) = receiver.recv().await else {
                panic!("expected explicit close");
            };
            let _ = reply.send(Ok(CleanupEvidence::DeviceOnly));
            assert!(receiver.try_recv().is_err());
        });
        let mut adapter = KnustAdapter {
            route: synthetic_route(),
            commands,
            worker: Some(worker),
            closing: false,
        };
        assert_eq!(adapter.close().await, Ok(CleanupEvidence::DeviceOnly));
        assert_eq!(adapter.close().await, Err(SessionError::Uncertain));
    }

    #[tokio::test(flavor = "current_thread")]
    async fn cancelled_close_port_cannot_be_promoted_or_reissued() {
        // A held synthetic reply models a pending observation; cancellation
        // cannot manufacture confirmation or dispatch a second close command.
        let (commands, mut receiver) = mpsc::channel(1);
        let (received, observed) = oneshot::channel();
        let worker = tokio::spawn(async move {
            let Some(Command::Close { reply }) = receiver.recv().await else {
                panic!("expected close");
            };
            let _ = received.send(());
            std::future::pending::<()>().await;
            drop(reply);
        });
        let mut adapter = KnustAdapter {
            route: synthetic_route(),
            commands,
            worker: Some(worker),
            closing: false,
        };
        assert!(
            timeout(Duration::from_millis(10), adapter.close())
                .await
                .is_err()
        );
        observed.await.unwrap();
        assert_eq!(adapter.close().await, Err(SessionError::Uncertain));
        // Only this owned synthetic worker is aborted; the real actor's
        // dropped port instead finishes its bounded cleanup without abort.
        if let Some(worker) = adapter.worker.take() {
            worker.abort();
            let _ = worker.await;
        }
    }
}
