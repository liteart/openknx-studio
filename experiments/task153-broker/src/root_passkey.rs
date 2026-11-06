// SPDX-License-Identifier: Apache-2.0
//! Read-only verifier for the fixed root-owned operator enrollment record.
//! No enrollment import, credential persistence, grant or executor is exposed.

use crate::ConfiguredPlan;
use crate::macos_config::{MAX_CONFIGURATION_BYTES, load_fixed_configuration};
use serde::{Deserialize, Serialize};
use std::collections::BTreeSet;
use std::fmt;
use std::time::{Duration, Instant};
use webauthn_rs::prelude::*;

pub const OPERATOR_ORIGIN: &str = "http://localhost:15300";
const LIFETIME: u64 = 120;
const MAX_ASSERTION_BYTES: usize = 32_768;

#[derive(Debug, PartialEq, Eq)]
pub enum Error {
    ProtectedConfigUnavailable,
    InvalidEnrollment,
    ChallengePending,
    ChallengeUnavailable,
    Expired,
    InvalidTime,
    PlanMismatch,
    InvalidResponse,
    VerificationFailed,
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct Enrollment {
    schema: u8,
    kind: EnrollmentKind,
    issuer: String,
    credential: Passkey,
}

#[derive(Deserialize)]
#[serde(rename_all = "snake_case")]
enum EnrollmentKind {
    Operator,
}

struct Pending {
    state: PasskeyAuthentication,
    digest: [u8; 32],
    issued: u64,
    expires: u64,
    started: Instant,
}

/// Signature observation only. No device capability or persisted grant exists.
#[derive(Debug, Serialize)]
pub struct RootAssertionObservation {
    issuer: String,
    plan_digest: [u8; 32],
    verified_at_utc_seconds: u64,
    user_verified: bool,
    credential_update_pending: bool,
    physical_authorization: bool,
}

impl RootAssertionObservation {
    pub fn physical_authorization(&self) -> bool {
        self.physical_authorization
    }

    pub fn credential_update_pending(&self) -> bool {
        self.credential_update_pending
    }

    pub fn plan_digest(&self) -> [u8; 32] {
        self.plan_digest
    }
}

/// The only public loader reads the fixed descriptor-validated root-owned file.
/// This type establishes no installed-service, UI or peer-authentication proof.
///
/// ```compile_fail
/// use task153_broker_core::root_passkey::RootPasskeyVerifier;
/// let verifier = RootPasskeyVerifier::from_record(b"untrusted record");
/// ```
pub struct RootPasskeyVerifier {
    webauthn: Webauthn,
    credential: Passkey,
    issuer: String,
    pending: Option<Pending>,
    credential_update_pending: bool,
}

impl RootPasskeyVerifier {
    pub fn load() -> Result<Self, Error> {
        let snapshot = load_fixed_configuration().map_err(|_| Error::ProtectedConfigUnavailable)?;
        Self::from_record(snapshot.as_bytes())
    }

    // Kept private: untrusted bytes and test fixtures cannot enter through the
    // normal API. An operator-enrolled protected record must be provisioned.
    fn from_record(bytes: &[u8]) -> Result<Self, Error> {
        if bytes.is_empty() || bytes.len() > MAX_CONFIGURATION_BYTES {
            return Err(Error::InvalidEnrollment);
        }
        let enrollment: Enrollment = unique_json(bytes).map_err(|_| Error::InvalidEnrollment)?;
        if enrollment.schema != 1
            || enrollment.issuer.is_empty()
            || enrollment.issuer.len() > 128
            || !enrollment
                .issuer
                .bytes()
                .all(|byte| byte.is_ascii_alphanumeric() || byte == b'-')
        {
            return Err(Error::InvalidEnrollment);
        }
        let EnrollmentKind::Operator = enrollment.kind;
        let webauthn = operator_webauthn()?;
        Ok(Self {
            webauthn,
            credential: enrollment.credential,
            issuer: enrollment.issuer,
            pending: None,
            credential_update_pending: false,
        })
    }

    pub fn begin(
        &mut self,
        plan: &ConfiguredPlan,
        now: u64,
    ) -> Result<RequestChallengeResponse, Error> {
        if self.pending.is_some() {
            return Err(Error::ChallengePending);
        }
        let expires = now
            .saturating_add(LIFETIME)
            .min(plan.expires_at_utc_seconds());
        if now >= expires {
            return Err(Error::Expired);
        }
        let (options, state) = self
            .webauthn
            .start_passkey_authentication(std::slice::from_ref(&self.credential))
            .map_err(|_| Error::VerificationFailed)?;
        self.pending = Some(Pending {
            state,
            digest: plan.digest(),
            issued: now,
            expires,
            started: Instant::now(),
        });
        Ok(options)
    }

    pub fn finish(
        &mut self,
        plan: &ConfiguredPlan,
        response: &[u8],
        now: u64,
    ) -> Result<RootAssertionObservation, Error> {
        // Every submission consumes the server-held ceremony, including errors.
        let pending = self.pending.take().ok_or(Error::ChallengeUnavailable)?;
        if now < pending.issued {
            return Err(Error::InvalidTime);
        }
        if now >= pending.expires || pending.started.elapsed() >= Duration::from_secs(LIFETIME) {
            return Err(Error::Expired);
        }
        if pending.digest != plan.digest() {
            return Err(Error::PlanMismatch);
        }
        if response.is_empty() || response.len() > MAX_ASSERTION_BYTES {
            return Err(Error::InvalidResponse);
        }
        let response: PublicKeyCredential =
            unique_json(response).map_err(|_| Error::InvalidResponse)?;
        let verified = self
            .webauthn
            .finish_passkey_authentication(&response, &pending.state)
            .map_err(|_| Error::VerificationFailed)?;
        if !verified.user_verified() {
            return Err(Error::VerificationFailed);
        }
        let changed = self
            .credential
            .update_credential(&verified)
            .ok_or(Error::VerificationFailed)?;
        self.credential_update_pending |= changed || verified.needs_update();
        Ok(RootAssertionObservation {
            issuer: self.issuer.clone(),
            plan_digest: pending.digest,
            verified_at_utc_seconds: now,
            user_verified: true,
            credential_update_pending: self.credential_update_pending,
            physical_authorization: false,
        })
    }

    pub fn cancel(&mut self) {
        self.pending = None;
    }
}

pub(super) fn operator_webauthn() -> Result<Webauthn, Error> {
    let origin = Url::parse(OPERATOR_ORIGIN).map_err(|_| Error::InvalidEnrollment)?;
    WebauthnBuilder::new("localhost", &origin)
        .map_err(|_| Error::InvalidEnrollment)?
        .rp_name("OpenKNX Studio operator approval")
        .allow_subdomains(false)
        .allow_any_port(false)
        .timeout(Duration::from_secs(LIFETIME))
        .build()
        .map_err(|_| Error::InvalidEnrollment)
}

pub(super) fn unique_json<T: serde::de::DeserializeOwned>(
    bytes: &[u8],
) -> Result<T, serde_json::Error> {
    // First pass rejects duplicate keys at every depth, including in the
    // library-owned credential/response types. Serde's recursion bound applies.
    serde_json::from_slice::<UniqueJson>(bytes)?;
    serde_json::from_slice(bytes)
}

struct UniqueJson;

impl<'de> Deserialize<'de> for UniqueJson {
    fn deserialize<D: serde::Deserializer<'de>>(deserializer: D) -> Result<Self, D::Error> {
        struct Visitor;
        impl<'de> serde::de::Visitor<'de> for Visitor {
            type Value = UniqueJson;
            fn expecting(&self, formatter: &mut fmt::Formatter) -> fmt::Result {
                formatter.write_str("JSON without duplicate keys")
            }
            fn visit_map<M: serde::de::MapAccess<'de>>(
                self,
                mut map: M,
            ) -> Result<UniqueJson, M::Error> {
                let mut seen = BTreeSet::new();
                while let Some(key) = map.next_key::<String>()? {
                    if !seen.insert(key) {
                        return Err(serde::de::Error::custom("duplicate key"));
                    }
                    map.next_value::<UniqueJson>()?;
                }
                Ok(UniqueJson)
            }
            fn visit_seq<S: serde::de::SeqAccess<'de>>(
                self,
                mut seq: S,
            ) -> Result<UniqueJson, S::Error> {
                while seq.next_element::<UniqueJson>()?.is_some() {}
                Ok(UniqueJson)
            }
            fn visit_str<E: serde::de::Error>(self, _: &str) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
            fn visit_bool<E: serde::de::Error>(self, _: bool) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
            fn visit_i64<E: serde::de::Error>(self, _: i64) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
            fn visit_u64<E: serde::de::Error>(self, _: u64) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
            fn visit_f64<E: serde::de::Error>(self, _: f64) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
            fn visit_unit<E: serde::de::Error>(self) -> Result<UniqueJson, E> {
                Ok(UniqueJson)
            }
        }
        deserializer.deserialize_any(Visitor)
    }
}

#[cfg(test)]
mod tests;
