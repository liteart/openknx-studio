// SPDX-License-Identifier: Apache-2.0
//! Development-only Task152 passkey prototype; no hardware executor exists.
//!
//! Enrollment here is deliberately untrusted development enrollment. A successful
//! assertion returns an observation, never Task020's private `HumanApproval`.
//! A protected enrollment/issuer service is still needed for physical use.

use serde::Serialize;
use sha2::{Digest, Sha256};
use std::fs::{self, File, OpenOptions};
use std::io::Write;
use std::path::{Path, PathBuf};
use std::time::{Duration, Instant};
use webauthn_rs::prelude::*;

#[path = "../../task020-guard/guard.rs"]
// Import the existing model intact; its unused executor is intentionally absent
// from this standalone verifier and remains private to that model.
#[allow(dead_code)]
pub mod guard;

pub const DEVELOPMENT_ORIGIN: &str = "http://localhost:15200";
const MAX_RESPONSE_BYTES: usize = 32_768;
const CHALLENGE_LIFETIME_SECONDS: u64 = 120;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    InvalidPlan,
    InvalidIssuer,
    UnsafeJournalPath,
    EnrollmentUnavailable,
    AlreadyEnrolled,
    ChallengePending,
    ChallengeUnavailable,
    PlanMismatch,
    Expired,
    InvalidTime,
    InvalidResponse,
    VerificationFailed,
    Replay,
    Io,
}

/// Exact validated Task020 input and immutable snapshot used by both the display
/// and assertion binding. The summary is serialized from that same input.
pub struct BoundPlan {
    plan: guard::Plan,
    summary: String,
    expires_at_utc_seconds: u64,
}

impl BoundPlan {
    pub fn new(input: guard::PlanInput) -> Result<Self, Error> {
        let expires_at_utc_seconds = input.expires_at_utc_seconds;
        let mut output = BoundedOutput(Vec::new());
        serde_json::to_writer_pretty(&mut output, &input).map_err(|_| Error::InvalidPlan)?;
        let summary = String::from_utf8(output.0).map_err(|_| Error::InvalidPlan)?;
        let plan = guard::Plan::new(input).map_err(|_| Error::InvalidPlan)?;
        Ok(Self {
            plan,
            summary,
            expires_at_utc_seconds,
        })
    }

    pub fn exact_plan_summary(&self) -> &str {
        &self.summary
    }
    pub fn guard_plan(&self) -> &guard::Plan {
        &self.plan
    }
}

struct EnrollmentChallenge {
    state: PasskeyRegistration,
    issued_at: u64,
    started: Instant,
}

struct AssertionChallenge {
    state: PasskeyAuthentication,
    plan_digest: [u8; 32],
    challenge_id: String,
    issued_at: u64,
    expires_at: u64,
    started: Instant,
}

/// A cryptographically verified development observation, deliberately distinct
/// from a device capability and not deserializable into one.
///
/// ```compile_fail
/// use task152_passkey_prototype::{guard, DevelopmentReceipt};
/// fn escalate(receipt: DevelopmentReceipt) -> guard::HumanApproval {
///     receipt
/// }
/// ```
#[derive(Debug, Serialize)]
pub struct DevelopmentReceipt {
    schema: &'static str,
    issuer: String,
    plan_digest: [u8; 32],
    challenge_id: String,
    verified_at_utc_seconds: u64,
    expires_at_utc_seconds: u64,
    user_verified: bool,
    physical_authorization: bool,
}

impl DevelopmentReceipt {
    pub fn plan_digest(&self) -> [u8; 32] {
        self.plan_digest
    }
    pub fn physical_authorization(&self) -> bool {
        self.physical_authorization
    }
}

/// Development ceremony state. No listener, MCP endpoint, key discovery,
/// credential import or physical approval constructor is provided.
pub struct DevelopmentVerifier {
    webauthn: Webauthn,
    journal: PathBuf,
    issuer: String,
    passkey: Option<Passkey>,
    enrollment: Option<EnrollmentChallenge>,
    assertion: Option<AssertionChallenge>,
}

impl DevelopmentVerifier {
    /// Use an existing canonical directory. This file journal is not a security
    /// boundary against code running as its owner; callers must not claim one.
    pub fn open(journal: &Path, issuer: &str) -> Result<Self, Error> {
        if issuer.is_empty()
            || issuer.len() > 128
            || !issuer
                .bytes()
                .all(|b| b.is_ascii_alphanumeric() || b == b'-')
        {
            return Err(Error::InvalidIssuer);
        }
        let metadata = fs::symlink_metadata(journal).map_err(|_| Error::UnsafeJournalPath)?;
        if !metadata.is_dir()
            || metadata.file_type().is_symlink()
            || fs::canonicalize(journal).map_err(|_| Error::UnsafeJournalPath)? != journal
        {
            return Err(Error::UnsafeJournalPath);
        }
        let origin = Url::parse(DEVELOPMENT_ORIGIN).map_err(|_| Error::VerificationFailed)?;
        let webauthn = WebauthnBuilder::new("localhost", &origin)
            .map_err(|_| Error::VerificationFailed)?
            .rp_name("OpenKNX Studio development approval")
            .allow_subdomains(false)
            .allow_any_port(false)
            .timeout(Duration::from_secs(CHALLENGE_LIFETIME_SECONDS))
            .build()
            .map_err(|_| Error::VerificationFailed)?;
        Ok(Self {
            webauthn,
            journal: journal.into(),
            issuer: issuer.into(),
            passkey: None,
            enrollment: None,
            assertion: None,
        })
    }

    pub fn begin_development_enrollment(
        &mut self,
        now: u64,
    ) -> Result<CreationChallengeResponse, Error> {
        if self.passkey.is_some() {
            return Err(Error::AlreadyEnrolled);
        }
        if self.enrollment.is_some() {
            return Err(Error::ChallengePending);
        }
        let (options, state) = self
            .webauthn
            .start_passkey_registration(
                Uuid::new_v4(),
                "development-operator",
                "Development operator",
                None,
            )
            .map_err(|_| Error::VerificationFailed)?;
        self.enrollment = Some(EnrollmentChallenge {
            state,
            issued_at: now,
            started: Instant::now(),
        });
        Ok(options)
    }

    /// This proves only possession of the newly created development credential.
    /// It does not establish a trusted operator identity or protect enrollment.
    pub fn finish_development_enrollment(
        &mut self,
        response: &[u8],
        now: u64,
    ) -> Result<(), Error> {
        let challenge = self.enrollment.take().ok_or(Error::ChallengeUnavailable)?;
        check_time(
            challenge.issued_at,
            challenge
                .issued_at
                .saturating_add(CHALLENGE_LIFETIME_SECONDS),
            challenge.started,
            now,
        )?;
        let credential: RegisterPublicKeyCredential = parse_response(response)?;
        self.passkey = Some(
            self.webauthn
                .finish_passkey_registration(&credential, &challenge.state)
                .map_err(|_| Error::VerificationFailed)?,
        );
        Ok(())
    }

    pub fn begin_assertion(
        &mut self,
        plan: &BoundPlan,
        now: u64,
    ) -> Result<RequestChallengeResponse, Error> {
        let passkey = self.passkey.as_ref().ok_or(Error::EnrollmentUnavailable)?;
        if self.assertion.is_some() {
            return Err(Error::ChallengePending);
        }
        let expires_at = plan
            .expires_at_utc_seconds
            .min(now.saturating_add(CHALLENGE_LIFETIME_SECONDS));
        if now >= expires_at {
            return Err(Error::Expired);
        }
        let (options, state) = self
            .webauthn
            .start_passkey_authentication(std::slice::from_ref(passkey))
            .map_err(|_| Error::VerificationFailed)?;
        // The WebAuthn implementation generates the random challenge. Only the
        // server-side state associates it with this exact plan and fixed issuer.
        let challenge_id = hex(&Sha256::digest(&options.public_key.challenge));
        self.assertion = Some(AssertionChallenge {
            state,
            plan_digest: plan.plan.digest(),
            challenge_id,
            issued_at: now,
            expires_at,
            started: Instant::now(),
        });
        Ok(options)
    }

    /// Consume the server-side challenge on every submission. A successful
    /// signature is recorded and fsynced before returning the observation.
    /// Restart invalidates all pending ceremonies; no state is accepted from a client.
    pub fn finish_assertion(
        &mut self,
        plan: &BoundPlan,
        response: &[u8],
        now: u64,
    ) -> Result<DevelopmentReceipt, Error> {
        let challenge = self.assertion.take().ok_or(Error::ChallengeUnavailable)?;
        check_time(
            challenge.issued_at,
            challenge.expires_at,
            challenge.started,
            now,
        )?;
        if challenge.plan_digest != plan.plan.digest() {
            return Err(Error::PlanMismatch);
        }
        let credential: PublicKeyCredential = parse_response(response)?;
        let result = self
            .webauthn
            .finish_passkey_authentication(&credential, &challenge.state)
            .map_err(|_| Error::VerificationFailed)?;
        if !result.user_verified() {
            return Err(Error::VerificationFailed);
        }
        let passkey = self.passkey.as_mut().ok_or(Error::EnrollmentUnavailable)?;
        if passkey.update_credential(&result).is_none() {
            return Err(Error::VerificationFailed);
        }
        let receipt = DevelopmentReceipt {
            schema: "task152-development-observation-v1",
            issuer: self.issuer.clone(),
            plan_digest: challenge.plan_digest,
            challenge_id: challenge.challenge_id,
            verified_at_utc_seconds: now,
            expires_at_utc_seconds: challenge.expires_at,
            user_verified: true,
            physical_authorization: false,
        };
        let bytes = serde_json::to_vec(&receipt).map_err(|_| Error::Io)?;
        let path = self
            .journal
            .join(format!("{}.spent.json", receipt.challenge_id));
        let mut file = OpenOptions::new()
            .write(true)
            .create_new(true)
            .open(path)
            .map_err(|e| {
                if e.kind() == std::io::ErrorKind::AlreadyExists {
                    Error::Replay
                } else {
                    Error::Io
                }
            })?;
        file.write_all(&bytes)
            .and_then(|_| file.sync_all())
            .map_err(|_| Error::Io)?;
        File::open(&self.journal)
            .and_then(|dir| dir.sync_all())
            .map_err(|_| Error::Io)?;
        Ok(receipt)
    }

    pub fn cancel(&mut self) {
        self.enrollment = None;
        self.assertion = None;
    }
}

fn check_time(issued: u64, expires: u64, started: Instant, now: u64) -> Result<(), Error> {
    if now < issued {
        return Err(Error::InvalidTime);
    }
    if now >= expires || started.elapsed() >= Duration::from_secs(CHALLENGE_LIFETIME_SECONDS) {
        return Err(Error::Expired);
    }
    Ok(())
}

fn parse_response<T: serde::de::DeserializeOwned>(response: &[u8]) -> Result<T, Error> {
    if response.is_empty() || response.len() > MAX_RESPONSE_BYTES {
        return Err(Error::InvalidResponse);
    }
    serde_json::from_slice(response).map_err(|_| Error::InvalidResponse)
}

fn hex(bytes: &[u8]) -> String {
    bytes.iter().map(|byte| format!("{byte:02x}")).collect()
}

struct BoundedOutput(Vec<u8>);

impl Write for BoundedOutput {
    fn write(&mut self, bytes: &[u8]) -> std::io::Result<usize> {
        if self.0.len().saturating_add(bytes.len()) > MAX_RESPONSE_BYTES {
            return Err(std::io::Error::other(
                "bounded development plan summary exceeded",
            ));
        }
        self.0.extend_from_slice(bytes);
        Ok(bytes.len())
    }
    fn flush(&mut self) -> std::io::Result<()> {
        Ok(())
    }
}

#[cfg(test)]
mod tests;
