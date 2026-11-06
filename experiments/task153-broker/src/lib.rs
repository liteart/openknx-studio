// SPDX-License-Identifier: Apache-2.0
//! Portable input contract for a future OS-protected operator broker.
//!
//! macOS peer identity checking is available, but no protected host boundary,
//! storage, enrollment or execution authority is established here. Input
//! validation alone never authenticates a local peer.

use serde::{Deserialize, Serialize};
use task152_passkey_prototype::{BoundPlan, guard};

#[cfg(target_os = "macos")]
mod enrollment_store;
#[cfg(target_os = "macos")]
pub mod macos_config;
#[cfg(target_os = "macos")]
pub mod macos_peer;
#[cfg(target_os = "macos")]
pub mod operator_connection;
#[cfg(target_os = "macos")]
pub mod root_passkey;

pub const MAX_REQUEST_BYTES: usize = 4_096;
pub const PROTOCOL_VERSION: u8 = 1;

#[derive(Debug, PartialEq, Eq, Serialize)]
#[serde(rename_all = "snake_case")]
pub enum Error {
    RequestTooLarge,
    InvalidRequest,
    UnsupportedVersion,
    InvalidRequestId,
    InvalidConfiguredPlan,
    PlanMismatch,
    ProtectedHostUnavailable,
}

/// The IPC contract deliberately has no route, selector, payload, enrollment,
/// grant-import or generic write command. Protected configuration selects one
/// operation; a caller can only request its display or present its exact hash.
#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Request {
    pub schema: u8,
    pub request_id: String,
    pub command: Command,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(tag = "operation", rename_all = "snake_case", deny_unknown_fields)]
pub enum Command {
    InspectPlan {},
    ExecuteApproved { plan_digest: [u8; 32] },
}

pub fn decode_request(bytes: &[u8]) -> Result<Request, Error> {
    if bytes.len() > MAX_REQUEST_BYTES {
        return Err(Error::RequestTooLarge);
    }
    let request: Request = serde_json::from_slice(bytes).map_err(|_| Error::InvalidRequest)?;
    if request.schema != PROTOCOL_VERSION {
        return Err(Error::UnsupportedVersion);
    }
    if request.request_id.is_empty()
        || request.request_id.len() > 64
        || !request
            .request_id
            .bytes()
            .all(|b| b.is_ascii_alphanumeric() || b == b'-')
    {
        return Err(Error::InvalidRequestId);
    }
    Ok(request)
}

/// This is validated configuration, not proof of trusted configuration or an
/// operator grant. A future protected provider must load it from its own store.
pub struct ConfiguredPlan {
    plan: BoundPlan,
    expires_at: u64,
}

impl ConfiguredPlan {
    pub fn new(input: guard::PlanInput) -> Result<Self, Error> {
        // Only the reviewed Verify Mode candidate is representable. The route
        // and identity come from future protected configuration, never IPC.
        if input.selector.object != 0
            || input.selector.property != 14
            || input.selector.index != 1
            || input.selector.count != 1
            || input.selector.element_size != 1
            || input.original != [0]
            || input.desired != [4]
        {
            return Err(Error::InvalidConfiguredPlan);
        }
        let expires_at = input.expires_at_utc_seconds;
        let plan = BoundPlan::new(input).map_err(|_| Error::InvalidConfiguredPlan)?;
        Ok(Self { plan, expires_at })
    }

    pub fn digest(&self) -> [u8; 32] {
        self.plan.guard_plan().digest()
    }

    /// The existing canonical snapshot supplies both display and digest.
    pub fn summary(&self) -> &str {
        self.plan.exact_plan_summary()
    }

    pub fn expires_at_utc_seconds(&self) -> u64 {
        self.expires_at
    }

    /// Matching a digest does not verify peer identity, assertion or privilege.
    pub fn check_request(&self, request: &Request) -> Result<(), Error> {
        match request.command {
            Command::InspectPlan {} => Ok(()),
            Command::ExecuteApproved { plan_digest } if plan_digest == self.digest() => Ok(()),
            Command::ExecuteApproved { .. } => Err(Error::PlanMismatch),
        }
    }
}

/// Deliberately unprovisioned facade. No caller-supplied UID, boolean, filename,
/// claimed role or fixture token can turn it into a protected service.
pub struct UnprovisionedBroker {
    plan: ConfiguredPlan,
}

impl UnprovisionedBroker {
    pub fn new(plan: ConfiguredPlan) -> Self {
        Self { plan }
    }

    /// Decode and validate without releasing plan contents or accessing a
    /// journal, credential store or adapter. Actual OS verification is missing.
    pub fn handle(&self, bytes: &[u8]) -> Result<(), Error> {
        let request = decode_request(bytes)?;
        self.plan.check_request(&request)?;
        Err(Error::ProtectedHostUnavailable)
    }
}

#[cfg(test)]
mod tests;
