// SPDX-License-Identifier: Apache-2.0
//! Configured-operator-peer-authenticated Unix connection handler. No listener or operator UI.
//!
//! The installed root service and OS-authenticated operator client are still
//! missing. Running this from a writable checkout does not establish protection.

use crate::ConfiguredPlan;
use crate::enrollment_store::{
    ActivationOutcome, activate_first_enrollment, persist_pending_enrollment,
};
use crate::macos_peer::{ExpectedOperator, check_operator_peer};
use crate::root_passkey::{RootPasskeyVerifier, operator_webauthn, unique_json};
use nix::unistd::{getegid, geteuid};
use serde::Deserialize;
use std::io::{Read, Write};
use std::os::unix::net::UnixStream;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};
use webauthn_rs::prelude::*;

const MAX_FRAME: usize = 32_768;
const MAX_REPLY: usize = 65_536;
const SESSION_TIMEOUT: Duration = Duration::from_secs(120);

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

fn require_operator_peer(stream: &UnixStream, policy: &ExpectedOperator) -> Result<(), Error> {
    // The service obtains policy from protected configuration, never request IDs.
    check_operator_peer(stream, policy).map_err(|_| Error::OperatorPeerDenied)?;
    Ok(())
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
        let count = stream.read(bytes).map_err(|_| Error::Io)?;
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

/// Consume one connected stream for exactly one ceremony. Authentication uses
/// that owned connection before any request is read. The caller must be an
/// independently provisioned root service, with a protected operator client.
/// The immutable plan is service-owned; IPC cannot select its route/effect.
/// No return or reply constitutes device authorization.
pub fn handle_operator_connection(
    mut stream: UnixStream,
    plan: &ConfiguredPlan,
    policy: &ExpectedOperator,
) -> Result<(), Error> {
    if geteuid().as_raw() != 0 || getegid().as_raw() != 0 {
        return Err(Error::RootServiceRequired);
    }
    require_operator_peer(&stream, policy)?;
    let deadline = Instant::now()
        .checked_add(SESSION_TIMEOUT)
        .ok_or(Error::Timeout)?;
    match read_request(&mut stream, deadline)?.command {
        Command::BeginEnrollment { issuer } => {
            if issuer.is_empty()
                || issuer.len() > 128
                || !issuer
                    .bytes()
                    .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
            {
                return Err(Error::InvalidRequest);
            }
            let (registration, options) = Registration::begin()?;
            write_reply(
                &mut stream,
                &serde_json::json!({"schema":1,"status":"enrollment_ready","issuer":issuer,"options":options,"physical_authorization":false}),
                deadline,
            )?;
            let Command::FinishEnrollment { credential } =
                read_request(&mut stream, deadline)?.command
            else {
                return Err(Error::InvalidRequest);
            };
            let credential = registration.finish(credential)?;
            persist_pending_enrollment(&issuer, &credential)
                .map_err(|_| Error::ProtectedStorageUnavailable)?;
            let outcome = activate_first_enrollment(&issuer, &credential)
                .map_err(|_| Error::ProtectedStorageUnavailable)?;
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
                &serde_json::json!({"schema":1,"status":"assertion_ready","plan_summary":plan.summary(),"plan_digest":plan.digest(),"options":options,"physical_authorization":false}),
                deadline,
            )?;
            let Command::FinishAssertion { credential } =
                read_request(&mut stream, deadline)?.command
            else {
                return Err(Error::InvalidRequest);
            };
            let bytes = serde_json::to_vec(&credential).map_err(|_| Error::InvalidRequest)?;
            let observation = verifier
                .finish_protected(plan, &bytes, now()?)
                .map_err(|_| Error::VerificationFailed)?;
            write_reply(
                &mut stream,
                &serde_json::json!({"schema":1,"status":"assertion_verified","observation":observation,"physical_authorization":false}),
                deadline,
            )
        }
        _ => Err(Error::InvalidRequest),
    }
}

#[cfg(test)]
mod tests;
