// SPDX-License-Identifier: Apache-2.0
//! Full-intent persistence for the existing guard, without its own execution state machine.

use crate::{Binding, Digest, Key, MAX_RECORD_BYTES, NamespaceStorage, RouteLease, StorageError};
use serde::{Deserialize, Serialize};
use sha2::{Digest as _, Sha256};

pub const MAX_INTENT_BYTES: usize = 16_384;
const MAX_PARTS: usize = MAX_INTENT_BYTES / MAX_RECORD_BYTES;
const FORMAT: &str = "task020-guard-v1";

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub enum Evidence {
    DesiredReadback,
    RestoreIntent,
    RestoredReadback,
    CleanupConfirmed,
}
impl Evidence {
    pub fn key(self) -> &'static str {
        match self {
            Self::DesiredReadback => "d",
            Self::RestoreIntent => "i",
            Self::RestoredReadback => "r",
            Self::CleanupConfirmed => "c",
        }
    }
    fn state(self) -> &'static str {
        match self {
            Self::DesiredReadback => "DESIRED_READBACK_VERIFIED",
            Self::RestoreIntent => "RESTORE_OUTCOME_UNKNOWN",
            Self::RestoredReadback => "RESTORED_VERIFIED",
            Self::CleanupConfirmed => "DEVICE_AND_GATEWAY_CLEANUP_CONFIRMED",
        }
    }
}
const EVIDENCE: [Evidence; 4] = [
    Evidence::DesiredReadback,
    Evidence::RestoreIntent,
    Evidence::RestoredReadback,
    Evidence::CleanupConfirmed,
];

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Error {
    Consumed,
    RouteUnresolved,
    Unavailable,
}
impl From<StorageError> for Error {
    fn from(value: StorageError) -> Self {
        match value {
            StorageError::Existing => Self::Consumed,
            _ => Self::Unavailable,
        }
    }
}

#[derive(Debug, Serialize, Deserialize, PartialEq, Eq)]
#[serde(deny_unknown_fields)]
struct Marker {
    format: String,
    binding: Binding,
}
#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Head {
    format: String,
    binding: Binding,
    released: bool,
}
#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct Manifest {
    format: String,
    binding: Binding,
    length: usize,
    parts: u8,
    sha256: Digest,
}
#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct GuardRecord {
    schema: u8,
    plan_digest: Digest,
    state: String,
    at_utc_seconds: u64,
}

fn encode<T: Serialize>(value: &T) -> Result<Vec<u8>, Error> {
    let bytes = serde_json::to_vec(value).map_err(|_| Error::Unavailable)?;
    if bytes.len() > MAX_RECORD_BYTES {
        return Err(Error::Unavailable);
    }
    Ok(bytes)
}
fn decode<T: for<'a> Deserialize<'a>>(bytes: &[u8]) -> Result<T, Error> {
    if bytes.is_empty() || bytes.len() > MAX_RECORD_BYTES {
        return Err(Error::Unavailable);
    }
    serde_json::from_slice(bytes).map_err(|_| Error::Unavailable)
}
fn marker(binding: &Binding) -> Marker {
    Marker {
        format: FORMAT.into(),
        binding: binding.clone(),
    }
}
fn matches_marker(lease: &mut impl RouteLease, key: Key, binding: &Binding) -> Result<bool, Error> {
    let Some(bytes) = lease.read(key, MAX_RECORD_BYTES)? else {
        return Ok(false);
    };
    Ok(decode::<Marker>(&bytes)? == marker(binding))
}
fn persist(lease: &mut impl RouteLease, key: Key, bytes: &[u8]) -> Result<(), Error> {
    lease.create_new(key, bytes)?;
    lease.flush()?;
    Ok(())
}
fn read_intent(
    lease: &mut impl RouteLease,
    operation: Digest,
    limit: usize,
) -> Result<Option<(Binding, Vec<u8>)>, Error> {
    let Some(op) = lease.read(Key::GuardOperation(operation), MAX_RECORD_BYTES)? else {
        return Ok(None);
    };
    let op: Marker = decode(&op)?;
    if op.format != FORMAT || op.binding.operation != operation || op.binding.validate().is_err() {
        return Err(Error::Unavailable);
    }
    let bytes = lease
        .read(Key::GuardIntent(operation), MAX_RECORD_BYTES)?
        .ok_or(Error::Unavailable)?;
    let manifest: Manifest = decode(&bytes)?;
    if manifest.format != FORMAT
        || manifest.binding != op.binding
        || manifest.length == 0
        || manifest.length > limit.min(MAX_INTENT_BYTES)
        || usize::from(manifest.parts) != manifest.length.div_ceil(MAX_RECORD_BYTES)
        || usize::from(manifest.parts) > MAX_PARTS
        || !matches_marker(lease, Key::GuardApproval(op.binding.approval), &op.binding)?
    {
        return Err(Error::Unavailable);
    }
    let mut intent = Vec::with_capacity(manifest.length);
    for index in 0..manifest.parts {
        let part = lease
            .read(Key::GuardIntentPart(operation, index), MAX_RECORD_BYTES)?
            .ok_or(Error::Unavailable)?;
        let remaining = manifest.length - intent.len();
        if part.len() != remaining.min(MAX_RECORD_BYTES) {
            return Err(Error::Unavailable);
        }
        intent.extend_from_slice(&part);
    }
    // A fixed maximum scan rejects extra/stale pieces without enumerating history.
    for index in manifest.parts..=MAX_PARTS as u8 {
        if lease
            .read(Key::GuardIntentPart(operation, index), MAX_RECORD_BYTES)?
            .is_some()
        {
            return Err(Error::Unavailable);
        }
    }
    let hash: Digest = Sha256::digest(&intent).into();
    if hash != manifest.sha256 {
        return Err(Error::Unavailable);
    }
    Ok(Some((manifest.binding, intent)))
}
fn validate_evidence(bytes: &[u8], binding: &Binding, kind: Evidence) -> Result<(), Error> {
    let value: GuardRecord = decode(bytes)?;
    if value.schema != 1 || value.plan_digest != binding.plan || value.state != kind.state() {
        return Err(Error::Unavailable);
    }
    // The timestamp remains exact guard evidence; this storage layer does not
    // independently decide whether an observation was fresh or authenticated.
    let _ = value.at_utc_seconds;
    Ok(())
}
fn prefix(lease: &mut impl RouteLease, binding: &Binding, count: usize) -> Result<(), Error> {
    let Some((actual, _)) = read_intent(lease, binding.operation, MAX_INTENT_BYTES)? else {
        return Err(Error::Unavailable);
    };
    if actual != *binding {
        return Err(Error::Unavailable);
    }
    for kind in EVIDENCE.iter().take(count) {
        let bytes = lease
            .read(
                Key::GuardEvidence(binding.operation, *kind),
                MAX_RECORD_BYTES,
            )?
            .ok_or(Error::Unavailable)?;
        validate_evidence(&bytes, binding, *kind)?;
    }
    Ok(())
}
fn head(lease: &mut impl RouteLease, binding: &Binding) -> Result<Head, Error> {
    let bytes = lease
        .read_route(MAX_RECORD_BYTES)?
        .ok_or(Error::RouteUnresolved)?;
    let head: Head = decode(&bytes)?;
    if head.format != FORMAT || head.binding != *binding || head.binding.validate().is_err() {
        return Err(Error::RouteUnresolved);
    }
    Ok(head)
}
fn available(lease: &mut impl RouteLease, route: Digest) -> Result<(), Error> {
    let Some(bytes) = lease.read_route(MAX_RECORD_BYTES)? else {
        return Ok(());
    };
    let head: Head = decode(&bytes).map_err(|_| Error::RouteUnresolved)?;
    if head.format != FORMAT
        || head.binding.route != route
        || head.binding.validate().is_err()
        || !head.released
    {
        return Err(Error::RouteUnresolved);
    }
    prefix(lease, &head.binding, EVIDENCE.len()).map_err(|_| Error::RouteUnresolved)?;
    // A previous publisher may have left a visible head after a failed flush.
    // Reconcile persistence under the same namespace lease before reuse.
    lease.flush()?;
    Ok(())
}

/// Trusted storage implementation only. It accepts no approval or observation,
/// and its methods never select a device operation. Use the macro below to bind
/// it to the actual consumer's guard trait instead of importing a lookalike.
pub struct GuardStore<S> {
    storage: S,
}
impl<S> std::fmt::Debug for GuardStore<S> {
    fn fmt(&self, formatter: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        formatter.debug_struct("GuardStore").finish_non_exhaustive()
    }
}
impl<S> GuardStore<S> {
    pub fn new(storage: S) -> Self {
        Self { storage }
    }
}
impl<S: NamespaceStorage> GuardStore<S> {
    pub fn preflight(&self) -> Result<(), Error> {
        self.storage.lock_records()?.flush().map_err(Error::from)
    }
    pub fn reserve(&self, binding: Binding, intent: &[u8]) -> Result<(), Error> {
        binding.validate().map_err(|_| Error::Unavailable)?;
        if intent.is_empty() || intent.len() > MAX_INTENT_BYTES {
            return Err(Error::Unavailable);
        }
        let mut lease = self.storage.lock_route(binding.route)?;
        available(&mut lease, binding.route)?;
        if lease
            .read(Key::GuardOperation(binding.operation), MAX_RECORD_BYTES)?
            .is_some()
            || lease
                .read(Key::GuardApproval(binding.approval), MAX_RECORD_BYTES)?
                .is_some()
        {
            return Err(Error::Consumed);
        }
        lease.set_route(&encode(&Head {
            format: FORMAT.into(),
            binding: binding.clone(),
            released: false,
        })?)?;
        lease.flush()?;
        let mark = encode(&marker(&binding))?;
        persist(&mut lease, Key::GuardOperation(binding.operation), &mark)?;
        persist(&mut lease, Key::GuardApproval(binding.approval), &mark)?;
        for (index, part) in intent.chunks(MAX_RECORD_BYTES).enumerate() {
            persist(
                &mut lease,
                Key::GuardIntentPart(binding.operation, index as u8),
                part,
            )?;
        }
        let manifest = Manifest {
            format: FORMAT.into(),
            binding: binding.clone(),
            length: intent.len(),
            parts: intent.len().div_ceil(MAX_RECORD_BYTES) as u8,
            sha256: Sha256::digest(intent).into(),
        };
        // The complete commitment is published last, never before all parts.
        persist(
            &mut lease,
            Key::GuardIntent(binding.operation),
            &encode(&manifest)?,
        )
    }
    pub fn append(&self, binding: Binding, kind: Evidence, bytes: &[u8]) -> Result<(), Error> {
        validate_evidence(bytes, &binding, kind)?;
        let mut lease = self.storage.lock_route(binding.route)?;
        if head(&mut lease, &binding)?.released {
            return Err(Error::RouteUnresolved);
        }
        let count = EVIDENCE
            .iter()
            .position(|value| *value == kind)
            .ok_or(Error::Unavailable)?;
        prefix(&mut lease, &binding, count)?;
        persist(
            &mut lease,
            Key::GuardEvidence(binding.operation, kind),
            bytes,
        )
    }
    pub fn intent(&self, operation: Digest, limit: usize) -> Result<Option<Vec<u8>>, Error> {
        let mut lease = self.storage.lock_records()?;
        let result = read_intent(&mut lease, operation, limit)?;
        match result {
            Some((_, bytes)) if bytes.len() > limit.min(MAX_INTENT_BYTES) => {
                Err(Error::Unavailable)
            }
            Some((_, bytes)) => Ok(Some(bytes)),
            None => Ok(None),
        }
    }
    pub fn evidence(
        &self,
        operation: Digest,
        kind: Evidence,
        limit: usize,
    ) -> Result<Option<Vec<u8>>, Error> {
        let mut lease = self.storage.lock_records()?;
        let Some((binding, _)) = read_intent(&mut lease, operation, MAX_INTENT_BYTES)? else {
            return Ok(None);
        };
        let result = lease.read(
            Key::GuardEvidence(operation, kind),
            limit.min(MAX_RECORD_BYTES),
        )?;
        if let Some(bytes) = &result {
            validate_evidence(bytes, &binding, kind)?;
        }
        Ok(result)
    }
    pub fn release(&self, binding: Binding) -> Result<(), Error> {
        let mut lease = self.storage.lock_route(binding.route)?;
        if head(&mut lease, &binding)?.released {
            return Err(Error::RouteUnresolved);
        }
        prefix(&mut lease, &binding, EVIDENCE.len())?;
        lease.set_route(&encode(&Head {
            format: FORMAT.into(),
            binding,
            released: true,
        })?)?;
        lease.flush()?;
        Ok(())
    }
}

/// Generate a consumer-local adapter for the *actual* guard module alias.
/// This avoids different Rust trait identities from path-included guard copies.
/// The alias must name the consumer's reviewed Task020 guard module. The macro
/// implements persistence only; it introduces no approval/observation issuer.
#[macro_export]
macro_rules! guard_persistence_backend {
    ($visibility:vis $name:ident, $guard:ident) => {
        $visibility struct $name<S> { inner: $crate::guard_storage::GuardStore<S> }
        impl<S> $name<S> {
            pub fn new(storage: S) -> Self { Self { inner: $crate::guard_storage::GuardStore::new(storage) } }
        }
        impl<S> std::fmt::Debug for $name<S> {
            fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result { f.debug_struct(stringify!($name)).finish_non_exhaustive() }
        }
        impl<S> $name<S> {
            fn binding(value: &$guard::JournalBinding) -> Result<$crate::Binding, $guard::PersistenceError> {
                $crate::Binding::new(value.route(), value.operation(), value.plan(), value.approval()).map_err(|_| $guard::PersistenceError::Unavailable)
            }
            fn error(value: $crate::guard_storage::Error) -> $guard::PersistenceError { match value { $crate::guard_storage::Error::Consumed => $guard::PersistenceError::AlreadyConsumed, $crate::guard_storage::Error::RouteUnresolved => $guard::PersistenceError::RouteUnresolved, $crate::guard_storage::Error::Unavailable => $guard::PersistenceError::Unavailable } }
            fn kind(value: $guard::JournalRecord) -> $crate::guard_storage::Evidence { match value { $guard::JournalRecord::DesiredReadback => $crate::guard_storage::Evidence::DesiredReadback, $guard::JournalRecord::RestoreIntent => $crate::guard_storage::Evidence::RestoreIntent, $guard::JournalRecord::RestoredReadback => $crate::guard_storage::Evidence::RestoredReadback, $guard::JournalRecord::CleanupConfirmed => $crate::guard_storage::Evidence::CleanupConfirmed } }
        }
        impl<S: $crate::NamespaceStorage + Send + Sync> $guard::JournalPersistence for $name<S> {
            fn preflight(&self) -> Result<(), $guard::PersistenceError> { self.inner.preflight().map_err(Self::error) }
            fn reserve(&self, binding: &$guard::JournalBinding, intent: &[u8]) -> Result<(), $guard::PersistenceError> { self.inner.reserve(Self::binding(binding)?, intent).map_err(Self::error) }
            fn append(&self, binding: &$guard::JournalBinding, kind: $guard::JournalRecord, record: &[u8]) -> Result<(), $guard::PersistenceError> { self.inner.append(Self::binding(binding)?, Self::kind(kind), record).map_err(Self::error) }
            fn read_intent(&self, operation: [u8;32], limit: usize) -> Result<Option<Vec<u8>>, $guard::PersistenceError> { self.inner.intent(operation, limit).map_err(Self::error) }
            fn read_record(&self, operation: [u8;32], kind: $guard::JournalRecord, limit: usize) -> Result<Option<Vec<u8>>, $guard::PersistenceError> { self.inner.evidence(operation, Self::kind(kind), limit).map_err(Self::error) }
            fn release(&self, binding: &$guard::JournalBinding) -> Result<(), $guard::PersistenceError> { self.inner.release(Self::binding(binding)?).map_err(Self::error) }
        }
    };
}
