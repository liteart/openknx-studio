// SPDX-License-Identifier: Apache-2.0
//! Configured-operator-peer-authenticated Unix connection handler. No listener or operator UI.
//!
//! The installed root service and OS-authenticated operator client are still
//! missing. Running this from a writable checkout does not establish protection.

use crate::ConfiguredPlan;
use crate::enrollment_store::{
    ActivationOutcome, activate_first_enrollment, persist_pending_enrollment,
};
use crate::macos_peer::{ExpectedOperator, PeerIdentity, check_operator_peer};
use crate::root_passkey::{
    PersistedAssertion, RootAssertionObservation, RootPasskeyVerifier, operator_webauthn,
    unique_json,
};
use nix::unistd::{getegid, geteuid};
use serde::{Deserialize, Serialize};
use std::io::{Read, Write};
use std::os::unix::net::UnixStream;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};
use webauthn_rs::prelude::*;

const MAX_FRAME: usize = 32_768;
const MAX_REPLY: usize = 65_536;
const SESSION_TIMEOUT: Duration = Duration::from_secs(120);

/// Internal result, never an approval token or a caller-selected operation.
#[derive(Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
#[allow(dead_code)] // Constructed by the gated exact-source executor only.
pub(crate) enum OperationOutcome {
    RestoredVerified,
    RejectedBeforeAdapter,
    OutcomeUnknown,
}

#[derive(Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
#[allow(dead_code)] // Constructed by the gated exact-source executor only.
pub(crate) enum CleanupOutcome {
    DeviceAndGatewayConfirmed,
    DeviceOnly,
    Uncertain,
    NotAttempted,
}

#[derive(Debug, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub(crate) struct GuardedExecutionReply {
    pub(crate) operation: OperationOutcome,
    pub(crate) cleanup: CleanupOutcome,
}

impl GuardedExecutionReply {
    pub(crate) fn succeeded(&self) -> bool {
        self.operation == OperationOutcome::RestoredVerified
            && self.cleanup == CleanupOutcome::DeviceAndGatewayConfirmed
    }
}

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    RootServiceRequired,
    OperatorPeerDenied,
    InvalidFrame,
    InvalidRequest,
    Timeout,
    Io,
    ClockUnavailable,
    VerificationFailed,
    ProtectedStorageUnavailable,
}

/// Fixed diagnostics only. Never include credential data or library errors.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub(crate) enum EnrollmentFailureStage {
    Challenge,
    FinishRequest,
    SessionExpired,
    CredentialVerification,
    Storage,
    Activation,
}

impl EnrollmentFailureStage {
    pub(crate) fn storage_state(self) -> &'static str {
        match self {
            Self::Storage | Self::Activation => "inspection_required",
            _ => "not_started_by_this_attempt",
        }
    }
}

fn enrollment_failure(
    stream: &mut UnixStream,
    stage: EnrollmentFailureStage,
    deadline: Instant,
) -> Result<(), Error> {
    // A timed-out ceremony may report its fixed failure code for at most one
    // additional second. This never renews its WebAuthn state or retries it.
    let error_deadline = Instant::now()
        .checked_add(Duration::from_secs(1))
        .ok_or(Error::Timeout)?;
    write_reply(
        stream,
        &serde_json::json!({"schema":1,"status":"enrollment_failed","stage":stage,
            "storage_state":stage.storage_state(),"physical_authorization":false}),
        deadline.max(error_deadline),
    )
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Request {
    schema: u8,
    command: Command,
}

#[derive(Deserialize)]
#[serde(tag = "operation", rename_all = "snake_case", deny_unknown_fields)]
enum Command {
    BeginEnrollment {
        issuer: String,
    },
    FinishEnrollment {
        credential: RegisterPublicKeyCredential,
    },
    BeginAssertion {},
    FinishAssertion {
        credential: PublicKeyCredential,
    },
}

fn require_operator_peer(
    stream: &UnixStream,
    policy: &ExpectedOperator,
) -> Result<PeerIdentity, Error> {
    // The service obtains policy from protected configuration, never request IDs.
    check_operator_peer(stream, policy).map_err(|_| Error::OperatorPeerDenied)
}

fn require_ceremony_permission(
    command: &Command,
    peer: &PeerIdentity,
    policy: &ExpectedOperator,
) -> Result<(), Error> {
    let allowed = match command {
        Command::BeginEnrollment { .. } => policy.permits_enrollment(peer),
        Command::BeginAssertion {} => policy.permits_assertion(peer),
        _ => false,
    };
    if allowed {
        Ok(())
    } else {
        Err(Error::OperatorPeerDenied)
    }
}

fn remaining(deadline: Instant) -> Result<Duration, Error> {
    deadline
        .checked_duration_since(Instant::now())
        .filter(|time| !time.is_zero())
        .ok_or(Error::Timeout)
}

fn read_exact(
    stream: &mut UnixStream,
    mut bytes: &mut [u8],
    deadline: Instant,
) -> Result<(), Error> {
    while !bytes.is_empty() {
        stream
            .set_read_timeout(Some(remaining(deadline)?))
            .map_err(|_| Error::Io)?;
        let count = stream.read(bytes).map_err(|error| {
            if matches!(
                error.kind(),
                std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
            ) {
                Error::Timeout
            } else {
                Error::Io
            }
        })?;
        if count == 0 {
            return Err(Error::InvalidFrame);
        }
        bytes = &mut bytes[count..];
    }
    Ok(())
}

fn read_request(stream: &mut UnixStream, deadline: Instant) -> Result<Request, Error> {
    let mut prefix = [0; 4];
    read_exact(stream, &mut prefix, deadline)?;
    let length = u32::from_be_bytes(prefix) as usize;
    if length == 0 || length > MAX_FRAME {
        return Err(Error::InvalidFrame);
    }
    let mut bytes = vec![0; length];
    read_exact(stream, &mut bytes, deadline)?;
    let request: Request = unique_json(&bytes).map_err(|_| Error::InvalidRequest)?;
    if request.schema != 1 {
        return Err(Error::InvalidRequest);
    }
    Ok(request)
}

fn write_reply(
    stream: &mut UnixStream,
    value: &serde_json::Value,
    deadline: Instant,
) -> Result<(), Error> {
    let bytes = serde_json::to_vec(value).map_err(|_| Error::InvalidFrame)?;
    if bytes.len() > MAX_REPLY {
        return Err(Error::InvalidFrame);
    }
    let mut frame = (bytes.len() as u32).to_be_bytes().to_vec();
    frame.extend_from_slice(&bytes);
    let mut remaining_bytes = frame.as_slice();
    while !remaining_bytes.is_empty() {
        stream
            .set_write_timeout(Some(remaining(deadline)?))
            .map_err(|_| Error::Io)?;
        let count = stream.write(remaining_bytes).map_err(|_| Error::Io)?;
        if count == 0 {
            return Err(Error::Io);
        }
        remaining_bytes = &remaining_bytes[count..];
    }
    Ok(())
}

fn now() -> Result<u64, Error> {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|time| time.as_secs())
        .map_err(|_| Error::ClockUnavailable)
}

struct Registration {
    webauthn: Webauthn,
    state: PasskeyRegistration,
    issued: Instant,
}

impl Registration {
    fn begin() -> Result<(Self, CreationChallengeResponse), Error> {
        let webauthn = operator_webauthn().map_err(|_| Error::VerificationFailed)?;
        let (options, state) = webauthn
            .start_passkey_registration(Uuid::new_v4(), "operator", "OpenKNX Studio operator", None)
            .map_err(|_| Error::VerificationFailed)?;
        Ok((
            Self {
                webauthn,
                state,
                issued: Instant::now(),
            },
            options,
        ))
    }

    fn finish(self, credential: RegisterPublicKeyCredential) -> Result<Passkey, Error> {
        if self.issued.elapsed() >= SESSION_TIMEOUT {
            return Err(Error::Timeout);
        }
        self.webauthn
            .finish_passkey_registration(&credential, &self.state)
            .map_err(|_| Error::VerificationFailed)
    }
}

fn complete_registration<F>(
    registration: Registration,
    credential: RegisterPublicKeyCredential,
    store: F,
) -> Result<ActivationOutcome, EnrollmentFailureStage>
where
    F: FnOnce(&Passkey) -> Result<ActivationOutcome, EnrollmentFailureStage>,
{
    let credential = registration
        .finish(credential)
        .map_err(|error| match error {
            Error::Timeout => EnrollmentFailureStage::SessionExpired,
            _ => EnrollmentFailureStage::CredentialVerification,
        })?;
    store(&credential)
}

/// Consume one connected stream for exactly one ceremony. Authentication uses
/// that owned connection before any request is read. The caller must be an
/// independently provisioned root service, with a protected operator client.
/// The immutable plan is service-owned; IPC cannot select its route/effect.
/// No return or reply constitutes device authorization.
pub fn handle_operator_connection(
    stream: UnixStream,
    plan: &ConfiguredPlan,
    policy: &ExpectedOperator,
) -> Result<(), Error> {
    handle_operator_connection_with_execution(
        stream,
        plan,
        policy,
        None::<fn(&ConfiguredPlan, PersistedAssertion) -> GuardedExecutionReply>,
    )
}

/// In-crate handoff for the exact-source executor only. Normal service startup
/// calls the observation-only wrapper above. No IPC field chooses a callback
/// or manufactures its persisted verification witness.
pub(crate) fn handle_operator_connection_with_execution<F>(
    mut stream: UnixStream,
    plan: &ConfiguredPlan,
    policy: &ExpectedOperator,
    execute: Option<F>,
) -> Result<(), Error>
where
    F: FnOnce(&ConfiguredPlan, PersistedAssertion) -> GuardedExecutionReply,
{
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootServiceRequired);
    }
    let peer = require_operator_peer(&stream, policy)?;
    let deadline = Instant::now()
        .checked_add(SESSION_TIMEOUT)
        .ok_or(Error::Timeout)?;
    let command = read_request(&mut stream, deadline)?.command;
    require_ceremony_permission(&command, &peer, policy)?;
    match command {
        Command::BeginEnrollment { issuer } => {
            if issuer.is_empty()
                || issuer.len() > 128
                || !issuer
                    .bytes()
                    .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
            {
                return Err(Error::InvalidRequest);
            }
            let (registration, options) = match Registration::begin() {
                Ok(value) => value,
                Err(_) => {
                    return enrollment_failure(
                        &mut stream,
                        EnrollmentFailureStage::Challenge,
                        deadline,
                    );
                }
            };
            write_reply(
                &mut stream,
                &serde_json::json!({"schema":1,"status":"enrollment_ready","issuer":issuer,"options":options,"physical_authorization":false}),
                deadline,
            )?;
            let credential = match read_request(&mut stream, deadline) {
                Ok(Request {
                    command: Command::FinishEnrollment { credential },
                    ..
                }) => credential,
                Err(Error::Timeout) => {
                    return enrollment_failure(
                        &mut stream,
                        EnrollmentFailureStage::SessionExpired,
                        deadline,
                    );
                }
                _ => {
                    return enrollment_failure(
                        &mut stream,
                        EnrollmentFailureStage::FinishRequest,
                        deadline,
                    );
                }
            };
            let outcome = match complete_registration(registration, credential, |credential| {
                persist_pending_enrollment(&issuer, credential)
                    .map_err(|_| EnrollmentFailureStage::Storage)?;
                activate_first_enrollment(&issuer, credential)
                    .map_err(|_| EnrollmentFailureStage::Activation)
            }) {
                Ok(outcome) => outcome,
                Err(stage) => return enrollment_failure(&mut stream, stage, deadline),
            };
            let status = match outcome {
                ActivationOutcome::Activated => "enrollment_activated",
                ActivationOutcome::CommittedCleanupUncertain => {
                    "enrollment_committed_cleanup_uncertain"
                }
            };
            write_reply(
                &mut stream,
                &serde_json::json!({"schema":1,"status":status,"physical_authorization":false}),
                deadline,
            )
        }
        Command::BeginAssertion {} => {
            let mut verifier =
                RootPasskeyVerifier::load().map_err(|_| Error::ProtectedStorageUnavailable)?;
            let options = verifier
                .begin(plan, now()?)
                .map_err(|_| Error::VerificationFailed)?;
            write_reply(
                &mut stream,
                &serde_json::json!({"schema":1,"status":"assertion_ready","execution_mode":if execute.is_some() { "guarded_write" } else { "observe_only" },"plan_summary":plan.summary(),"plan_digest":plan.digest(),"options":options,"physical_authorization":false}),
                deadline,
            )?;
            let Command::FinishAssertion { credential } =
                read_request(&mut stream, deadline)?.command
            else {
                return Err(Error::InvalidRequest);
            };
            let bytes = serde_json::to_vec(&credential).map_err(|_| Error::InvalidRequest)?;
            let (observation, witness) = verifier
                .finish_for_execution(plan, &bytes, now()?)
                .map_err(|_| Error::VerificationFailed)?;
            let reply = complete_verified_assertion(plan, observation, witness, execute);
            write_reply(&mut stream, &reply, deadline)
        }
        _ => Err(Error::InvalidRequest),
    }
}

pub(crate) fn complete_verified_assertion<F>(
    plan: &ConfiguredPlan,
    observation: RootAssertionObservation,
    witness: PersistedAssertion,
    execute: Option<F>,
) -> serde_json::Value
where
    F: FnOnce(&ConfiguredPlan, PersistedAssertion) -> GuardedExecutionReply,
{
    match execute {
        None => {
            serde_json::json!({"schema":1,"status":"assertion_verified","execution_mode":"observe_only","observation":observation,"physical_authorization":false})
        }
        Some(execute) => {
            // Consume the sealed witness exactly once; no wire reply can be
            // imported to invoke this callback. A lost final reply may follow
            // physical effects and must never cause an automatic retry.
            let report = execute(plan, witness);
            serde_json::json!({"schema":1,"status":"guarded_execution_report","execution_mode":"guarded_write","succeeded":report.succeeded(),"execution":report,"observation":observation})
        }
    }
}

#[cfg(test)]
mod tests;
