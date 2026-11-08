// SPDX-License-Identifier: Apache-2.0
//! Operation-journal policy with an opt-in, unqualified Windows storage adapter.
//!
//! Callers must separately verify plans, approval and physical observations.
//! This crate persists their supplied bindings; no record or return value is a
//! dispatch capability. The storage contract is trusted and not implemented by
//! production code. Native Windows behavior requires separate qualification.

#![forbid(unsafe_code)]

use serde::{Deserialize, Serialize};

pub mod guard_storage;

#[cfg(any(test, all(windows, feature = "experimental-windows-storage")))]
mod cooperative_lock;
#[cfg(any(test, all(windows, feature = "experimental-windows-storage")))]
mod native_layout;
#[cfg(all(windows, feature = "experimental-windows-storage"))]
pub mod windows;

#[cfg_attr(
    not(all(windows, feature = "experimental-windows-storage")),
    doc = r#"
The native adapter is absent without an explicit Windows qualification opt-in:
```compile_fail
use task156_operation_journal_policy::windows::WindowsStorage;
```
"#
)]
const _: () = ();

pub const MAX_RECORD_BYTES: usize = 2048;
pub type Digest = [u8; 32];

/// Opaque caller-supplied identifiers. Construction is not approval.
#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Binding {
    route: Digest,
    operation: Digest,
    plan: Digest,
    approval: [u8; 16],
}

impl Binding {
    pub fn new(
        route: Digest,
        operation: Digest,
        plan: Digest,
        approval: [u8; 16],
    ) -> Result<Self, Error> {
        let value = Self {
            route,
            operation,
            plan,
            approval,
        };
        value.validate()?;
        Ok(value)
    }

    fn validate(&self) -> Result<(), Error> {
        if self.route == [0; 32]
            || self.operation == [0; 32]
            || self.plan == [0; 32]
            || self.approval == [0; 16]
        {
            Err(Error::InvalidBinding)
        } else {
            Ok(())
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum StorageError {
    Busy,
    Existing,
    Unavailable,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Error {
    InvalidBinding,
    PlanMismatch,
    AlreadyConsumed,
    RouteQuarantined,
    InvalidTransition,
    CorruptRecord,
    Storage(StorageError),
}

impl From<StorageError> for Error {
    fn from(value: StorageError) -> Self {
        Self::Storage(value)
    }
}

/// Fixed, bounded event sequence. These are caller observations, not commands.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum Event {
    Intent,
    DesiredIntent,
    DesiredReadback,
    RestoreIntent,
    RestoredReadback,
    CleanupConfirmed,
}

const EVENTS: [Event; 6] = [
    Event::Intent,
    Event::DesiredIntent,
    Event::DesiredReadback,
    Event::RestoreIntent,
    Event::RestoredReadback,
    Event::CleanupConfirmed,
];

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Key {
    Operation(Digest),
    Approval([u8; 16]),
    Event(Digest, Event),
    GuardOperation(Digest),
    GuardApproval([u8; 16]),
    GuardIntent(Digest),
    GuardIntentPart(Digest, u8),
    GuardEvidence(Digest, guard_storage::Evidence),
}

/// Backend contract, not a security boundary against its implementer.
///
/// The lease must exclude cooperating threads/processes for this route, remain
/// held through every read/write/flush in the call. Acquiring an OS lock must
/// never imply that persisted active/unknown route state is free. Operation
/// and approval `create_new` keys must be
/// globally atomic across routes. Reads must enforce the supplied byte bound
/// before allocating. Successful `flush` must durably publish all preceding
/// writes; errors may leave partial/visible records and must never delete them.
/// A future Windows backend must establish these guarantees with native tests.
pub trait Storage {
    type Lease<'a>: RouteLease
    where
        Self: 'a;
    fn lock_route(&self, route: Digest) -> Result<Self::Lease<'_>, StorageError>;
}

/// Stronger contract needed by full guard recovery, whose operation key does
/// not contain a route. All route leases and this record-only lease must share
/// one namespace-wide exclusion mechanism. This lease must not read or mutate
/// a synthetic route head. Implementing only route-local exclusion is invalid.
pub trait NamespaceStorage: Storage {
    fn lock_records(&self) -> Result<Self::Lease<'_>, StorageError>;
}

pub trait RouteLease {
    fn read_route(&mut self, limit: usize) -> Result<Option<Vec<u8>>, StorageError>;
    fn set_route(&mut self, bytes: &[u8]) -> Result<(), StorageError>;
    fn read(&mut self, key: Key, limit: usize) -> Result<Option<Vec<u8>>, StorageError>;
    fn create_new(&mut self, key: Key, bytes: &[u8]) -> Result<(), StorageError>;
    fn flush(&mut self) -> Result<(), StorageError>;
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum RouteStatus {
    Available,
    Quarantined,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum AttemptStatus {
    Ready,
    DesiredIntent,
    DesiredReadback,
    RestoreIntent,
    RestoredReadback,
    Released,
    OutcomeUnknown,
}

/// Non-clone in-process bookkeeping only; not a grant or dispatch capability.
#[derive(Debug)]
pub struct Attempt {
    binding: Binding,
    status: AttemptStatus,
}

impl Attempt {
    pub fn status(&self) -> AttemptStatus {
        self.status
    }

    /// Cancellation cannot infer an unchanged device or release the route.
    pub fn cancel(&mut self) {
        if self.status != AttemptStatus::Released {
            self.status = AttemptStatus::OutcomeUnknown;
        }
    }
}

#[derive(Debug, Serialize, Deserialize, PartialEq, Eq)]
#[serde(deny_unknown_fields)]
struct Record {
    schema: u8,
    binding: Binding,
    kind: RecordKind,
}

#[derive(Debug, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "snake_case")]
enum RecordKind {
    Operation,
    Approval,
    Event(Event),
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(deny_unknown_fields)]
struct RouteRecord {
    schema: u8,
    binding: Binding,
    released: bool,
}

fn encode<T: Serialize>(record: &T) -> Result<Vec<u8>, Error> {
    let bytes = serde_json::to_vec(record).map_err(|_| Error::CorruptRecord)?;
    if bytes.len() > MAX_RECORD_BYTES {
        return Err(Error::CorruptRecord);
    }
    Ok(bytes)
}

fn decode<T: for<'a> Deserialize<'a>>(bytes: &[u8]) -> Result<T, Error> {
    if bytes.is_empty() || bytes.len() > MAX_RECORD_BYTES {
        return Err(Error::CorruptRecord);
    }
    serde_json::from_slice(bytes).map_err(|_| Error::CorruptRecord)
}

fn record(binding: &Binding, kind: RecordKind) -> Result<Vec<u8>, Error> {
    encode(&Record {
        schema: 1,
        binding: binding.clone(),
        kind,
    })
}

fn persist(lease: &mut impl RouteLease, key: Key, bytes: &[u8]) -> Result<(), Error> {
    lease.create_new(key, bytes).map_err(|error| {
        if error == StorageError::Existing {
            Error::AlreadyConsumed
        } else {
            error.into()
        }
    })?;
    lease.flush()?;
    Ok(())
}

fn set_route(lease: &mut impl RouteLease, binding: &Binding, released: bool) -> Result<(), Error> {
    lease.set_route(&encode(&RouteRecord {
        schema: 1,
        binding: binding.clone(),
        released,
    })?)?;
    lease.flush()?;
    Ok(())
}

fn matches_record(
    lease: &mut impl RouteLease,
    key: Key,
    binding: &Binding,
    kind: RecordKind,
) -> Result<bool, Error> {
    let Some(bytes) = lease.read(key, MAX_RECORD_BYTES)? else {
        return Ok(false);
    };
    let actual: Record = decode(&bytes)?;
    Ok(actual
        == Record {
            schema: 1,
            binding: binding.clone(),
            kind,
        })
}

fn validate_prefix(
    lease: &mut impl RouteLease,
    binding: &Binding,
    count: usize,
) -> Result<bool, Error> {
    if !matches_record(
        lease,
        Key::Operation(binding.operation),
        binding,
        RecordKind::Operation,
    )? || !matches_record(
        lease,
        Key::Approval(binding.approval),
        binding,
        RecordKind::Approval,
    )? {
        return Ok(false);
    }
    for event in &EVENTS[..count] {
        if !matches_record(
            lease,
            Key::Event(binding.operation, *event),
            binding,
            RecordKind::Event(*event),
        )? {
            return Ok(false);
        }
    }
    Ok(true)
}

fn route_status(lease: &mut impl RouteLease, route: Digest) -> Result<RouteStatus, Error> {
    let Some(bytes) = lease.read_route(MAX_RECORD_BYTES)? else {
        return Ok(RouteStatus::Available);
    };
    let head: RouteRecord = match decode(&bytes) {
        Ok(value) => value,
        Err(_) => return Ok(RouteStatus::Quarantined),
    };
    if head.schema != 1
        || head.binding.validate().is_err()
        || head.binding.route != route
        || !head.released
    {
        return Ok(RouteStatus::Quarantined);
    }
    if !validate_prefix(lease, &head.binding, EVENTS.len())? {
        return Ok(RouteStatus::Quarantined);
    }
    // Registry reads can expose a released head whose writer's flush failed.
    // Visibility is not durability. Hold the route lease and require our own
    // successful barrier before reporting reuse. This reconciles storage only;
    // no device operation is retried or resumed.
    lease.flush()?;
    Ok(RouteStatus::Available)
}

pub struct Journal<S> {
    storage: S,
}

impl<S: Storage> Journal<S> {
    pub fn new(storage: S) -> Self {
        Self { storage }
    }

    /// Recovery never resumes an active operation, even with readback records.
    /// Existing released state needs a fresh successful storage barrier, so
    /// this inspection can reconcile persistence and is not a pure read.
    pub fn inspect_route(&self, route: Digest) -> Result<RouteStatus, Error> {
        if route == [0; 32] {
            return Err(Error::InvalidBinding);
        }
        route_status(&mut self.storage.lock_route(route)?, route)
    }

    /// Persist metadata only. The supplied binding does not establish approval.
    pub fn begin(&self, binding: Binding) -> Result<Attempt, Error> {
        binding.validate()?;
        let mut lease = self.storage.lock_route(binding.route)?;
        if route_status(&mut lease, binding.route)? != RouteStatus::Available {
            return Err(Error::RouteQuarantined);
        }
        if lease
            .read(Key::Operation(binding.operation), MAX_RECORD_BYTES)?
            .is_some()
            || lease
                .read(Key::Approval(binding.approval), MAX_RECORD_BYTES)?
                .is_some()
        {
            return Err(Error::AlreadyConsumed);
        }
        // A later error leaves this route active. The OS lock alone is not the
        // durable route reservation and cannot release it after process death.
        set_route(&mut lease, &binding, false)?;
        persist(
            &mut lease,
            Key::Operation(binding.operation),
            &record(&binding, RecordKind::Operation)?,
        )?;
        persist(
            &mut lease,
            Key::Approval(binding.approval),
            &record(&binding, RecordKind::Approval)?,
        )?;
        persist(
            &mut lease,
            Key::Event(binding.operation, Event::Intent),
            &record(&binding, RecordKind::Event(Event::Intent))?,
        )?;
        Ok(Attempt {
            binding,
            status: AttemptStatus::Ready,
        })
    }

    /// Record an already independently verified caller observation in order.
    /// This does not verify the observation or select a network operation.
    pub fn record(&self, attempt: &mut Attempt, plan: Digest, event: Event) -> Result<(), Error> {
        let previous = attempt.status;
        attempt.status = AttemptStatus::OutcomeUnknown;
        if plan != attempt.binding.plan {
            return Err(Error::PlanMismatch);
        }
        let (next, prior_count) = match (previous, event) {
            (AttemptStatus::Ready, Event::DesiredIntent) => (AttemptStatus::DesiredIntent, 1),
            (AttemptStatus::DesiredIntent, Event::DesiredReadback) => {
                (AttemptStatus::DesiredReadback, 2)
            }
            (AttemptStatus::DesiredReadback, Event::RestoreIntent) => {
                (AttemptStatus::RestoreIntent, 3)
            }
            (AttemptStatus::RestoreIntent, Event::RestoredReadback) => {
                (AttemptStatus::RestoredReadback, 4)
            }
            (AttemptStatus::RestoredReadback, Event::CleanupConfirmed) => {
                (AttemptStatus::Released, 5)
            }
            _ => return Err(Error::InvalidTransition),
        };
        let binding = &attempt.binding;
        let mut lease = self.storage.lock_route(binding.route)?;
        let head: RouteRecord = decode(
            &lease
                .read_route(MAX_RECORD_BYTES)?
                .ok_or(Error::CorruptRecord)?,
        )?;
        if head.schema != 1
            || head.released
            || head.binding != *binding
            || !validate_prefix(&mut lease, binding, prior_count)?
        {
            return Err(Error::RouteQuarantined);
        }
        persist(
            &mut lease,
            Key::Event(binding.operation, event),
            &record(binding, RecordKind::Event(event))?,
        )?;
        if event == Event::CleanupConfirmed {
            set_route(&mut lease, binding, true)?;
        }
        attempt.status = next;
        Ok(())
    }
}

#[cfg(test)]
mod tests;

#[cfg(test)]
#[path = "../../task020-guard/guard.rs"]
#[allow(dead_code)]
mod guard;

#[cfg(test)]
mod guard_bridge_tests;
