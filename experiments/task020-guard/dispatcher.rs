// SPDX-License-Identifier: Apache-2.0
//! Consuming dispatcher for the offline exact-plan core. No grant can be minted here.

use super::{
    Attempt, BoundDispatch, CleanupReceipt, Error, HumanApproval, Journal, Observation,
    ObservationWatermark, Plan, Route, Selector, Status, WriteObservation,
};

/// Implemented only by reviewed in-crate adapters, never by imported data or MCP.
/// The implementation must own its session exclusively, validate protocol
/// responses and perform each write at most once. Errors never prove no effect.
pub(crate) trait TypedSession {
    fn route(&self) -> &Route;
    async fn observe(&mut self, selector: &Selector) -> Result<SessionRead, SessionError>;
    async fn write(&mut self, dispatch: BoundDispatch) -> Result<SessionRead, SessionError>;
    async fn close(&mut self) -> Result<CleanupEvidence, SessionError>;
}

/// Reviewed typed adapter evidence, not an operator approval or input flag.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum CleanupEvidence {
    DeviceAndGatewayConfirmed,
    DeviceOnly,
}

/// Typed adapter result, revalidated against the immutable plan by this module.
pub(crate) struct SessionRead {
    pub route: Route,
    pub selector: Selector,
    pub value: Vec<u8>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum SessionError {
    InvalidSelection,
    Uncertain,
}

pub(crate) trait Clock {
    fn now(&self) -> u64;
}

#[derive(Debug, PartialEq, Eq)]
pub(crate) enum DispatchError {
    Guard(Error),
    Session(SessionError),
    ResponseMismatch,
    ReceiptExhausted,
}

impl From<Error> for DispatchError {
    fn from(error: Error) -> Self {
        Self::Guard(error)
    }
}

/// A successful sequence is still separate from session/tunnel cleanup.
#[derive(Debug, PartialEq, Eq)]
pub(crate) struct ExecutionReport {
    pub operation: Result<Status, DispatchError>,
    pub cleanup: Option<Result<CleanupEvidence, SessionError>>,
}

impl ExecutionReport {
    pub(crate) fn succeeded(&self) -> bool {
        self.operation == Ok(Status::RestoredVerified)
            && self.cleanup == Some(Ok(CleanupEvidence::DeviceAndGatewayConfirmed))
    }
}

struct Ordinal(u64);
impl Ordinal {
    fn next(&mut self) -> Result<u64, DispatchError> {
        self.0 = self
            .0
            .checked_add(1)
            .ok_or(DispatchError::ReceiptExhausted)?;
        Ok(self.0)
    }
}

fn observed(
    read: SessionRead,
    ordinal: &mut Ordinal,
    at: u64,
) -> Result<Observation, DispatchError> {
    Ok(Observation {
        route: read.route,
        selector: read.selector,
        value: read.value,
        observed_at_utc_seconds: at,
        receipt: ordinal.next()?,
    })
}

/// Cancellation or any error after durable reservation poisons this in-memory
/// attempt as well. The durable unresolved route already survives process loss.
struct Active<'a>(&'a mut Attempt);
impl Drop for Active<'_> {
    fn drop(&mut self) {
        if self.0.status != Status::RestoredVerified {
            self.0.status = Status::OutcomeUnknown;
        }
    }
}

/// Consume approval only through Journal::begin; missing approval does not even
/// invoke the session. No file, string, flag or test receipt becomes a grant.
pub(crate) async fn execute<S: TypedSession, C: Clock>(
    journal: &Journal,
    plan: Plan,
    approval: Option<HumanApproval>,
    session: &mut S,
    clock: &C,
) -> ExecutionReport {
    let mut touched = false;
    let result = execute_inner(journal, plan, approval, session, clock, &mut touched).await;
    // An unapproved call must not cause even a cleanup packet on a caller's
    // supplied session. Every approved operation attempts explicit close on a
    // normal return; cancellation cannot perform async cleanup in Drop.
    let cleanup = if touched {
        Some(session.close().await)
    } else {
        None
    };
    let operation = match result {
        Ok(mut attempt) => {
            if cleanup == Some(Ok(CleanupEvidence::DeviceAndGatewayConfirmed)) {
                attempt
                    .release_after_cleanup(
                        CleanupReceipt {
                            plan_digest: attempt.plan.digest,
                        },
                        clock.now(),
                    )
                    .map(|()| attempt.status())
                    .map_err(DispatchError::Guard)
            } else {
                Ok(attempt.status())
            }
        }
        Err(error) => Err(error),
    };
    ExecutionReport { operation, cleanup }
}

async fn execute_inner<S: TypedSession, C: Clock>(
    journal: &Journal,
    plan: Plan,
    approval: Option<HumanApproval>,
    session: &mut S,
    clock: &C,
    touched: &mut bool,
) -> Result<Attempt, DispatchError> {
    let approval = approval.ok_or(Error::ApprovalUnavailable)?;
    if approval.plan_digest != plan.digest || approval.id == [0; 16] {
        return Err(Error::ApprovalMismatch.into());
    }
    if clock.now() >= approval.expires_at_utc_seconds
        || clock.now() >= plan.input.expires_at_utc_seconds
    {
        return Err(Error::Expired.into());
    }
    if session.route() != &plan.input.route {
        return Err(Error::ObservationMismatch.into());
    }
    let mut ordinal = Ordinal(0);
    *touched = true;
    let initial = session
        .observe(&plan.input.selector)
        .await
        .map_err(DispatchError::Session)?;
    let initial = observed(initial, &mut ordinal, clock.now())?;
    let mut attempt = journal.begin(plan, Some(approval), &initial, clock.now())?;
    let active = Active(&mut attempt);

    // A fresh descriptor/property observation is required immediately before
    // selecting the first write, independently of pre-journal observation.
    let fresh = session
        .observe(&active.0.plan.input.selector)
        .await
        .map_err(DispatchError::Session)?;
    let fresh = observed(fresh, &mut ordinal, clock.now())?;
    let selected = active.0.dispatch_desired(&fresh, clock.now())?;
    let dispatch = selected.bind_for_execution(&active.0.plan)?;
    confirm_write(active.0, dispatch, session, clock, &mut ordinal).await?;
    let changed = session
        .observe(&active.0.plan.input.selector)
        .await
        .map_err(DispatchError::Session)?;
    let changed = observed(changed, &mut ordinal, clock.now())?;
    active.0.record_desired_readback(&changed, clock.now())?;

    // Restoration is selected only after independent desired-value readback.
    // Any earlier error returns immediately, without an automatic extra write.
    let selected = active.0.dispatch_restore(clock.now())?;
    let dispatch = selected.bind_for_execution(&active.0.plan)?;
    confirm_write(active.0, dispatch, session, clock, &mut ordinal).await?;
    let restored = session
        .observe(&active.0.plan.input.selector)
        .await
        .map_err(DispatchError::Session)?;
    let restored = observed(restored, &mut ordinal, clock.now())?;
    active.0.record_restored_readback(&restored, clock.now())?;
    drop(active);
    Ok(attempt)
}

async fn confirm_write<S: TypedSession, C: Clock>(
    attempt: &mut Attempt,
    dispatch: BoundDispatch,
    session: &mut S,
    clock: &C,
    ordinal: &mut Ordinal,
) -> Result<(), DispatchError> {
    // Copy expected evidence before consuming the one-use bound dispatch. These
    // copies cannot be sent by this dispatcher; the adapter receives the token.
    let route = dispatch.route().clone();
    let selector = dispatch.selector().clone();
    let value = dispatch.bytes().to_vec();
    if session.route() != &route {
        return Err(Error::DispatchMismatch.into());
    }
    let response = match session.write(dispatch).await {
        Ok(response) => response,
        Err(error) => {
            attempt.record_write_response(WriteObservation::OutcomeUnknown, None, clock.now())?;
            return Err(DispatchError::Session(error));
        }
    };
    if response.route != route || response.selector != selector || response.value != value {
        attempt.record_write_response(WriteObservation::OutcomeUnknown, None, clock.now())?;
        return Err(DispatchError::ResponseMismatch);
    }
    let watermark = ObservationWatermark {
        plan_digest: attempt.plan.digest,
        receipt: ordinal.next()?,
        confirmed_at_utc_seconds: clock.now(),
    };
    attempt.record_write_response(
        WriteObservation::ResponseConfirmed,
        Some(watermark),
        clock.now(),
    )?;
    Ok(())
}

#[cfg(test)]
#[path = "dispatcher_tests.rs"]
mod tests;
