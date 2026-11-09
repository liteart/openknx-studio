<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task156 guard storage bridge

Historical checkpoint: this report predates native Windows execution. Current
Task156 outcomes and scope are recorded in
[`Task-156`](../../tasks/Task-156.md). The later bounded Windows and Linux
test results do not qualify power-loss behavior, production-service
integration or hardware.

Base: editor `2297ec5`. Task003/151 are Done; Task156 has no phase gate.
The bridge implements actual Task020 `JournalPersistence` using Task156's
storage primitives, not Task156's separate policy state machine. No approval,
observation, network or device capability is added. Existing Task020 filesystem
behavior and the fixed Task153 service activation remain unchanged.

## Exact consumer types and complete evidence

`guard_persistence_backend!(Name, actual_guard_alias)` creates a consumer-local
newtype implementing that consumer's exact guard trait. It does not import a
lookalike production guard module. This is necessary because Task020 guard
source is path-included separately by its consumers. Tests use the actual
Task020 guard/dispatcher and a deterministic Task156 storage fixture. A private
compile-only fixture also instantiated the macro against the actual Task152
`guard` module while constructing Task153 `ConfiguredPlan` from the same
`PlanInput`; the compiler accepted the exact trait and data type identities.
The real broker was not executed or rewired.

The full guard intent is preserved byte-for-byte, up to 16,384 bytes, as at
most eight immutable 2,048-byte parts. A versioned manifest containing exact
binding, length, part count and SHA-256 is created and flushed last. Missing,
partial, wrong-length, mismatched-hash or extra modeled pieces fail recovery.
The existing native 2KiB record representation and its 128-key chunk limit
are unchanged. New guard-specific operation, approval, part, manifest and
record keys are distinct and shorter than the existing longest registry path.
No digest-only policy record substitutes for the complete plan.

An explicit `NamespaceStorage` contract requires record-only recovery leases
to exclude every route lease. A route-local implementation cannot implement it
by using an unrelated sentinel. Windows uses the existing namespace-wide file
lease without reading or creating a synthetic route head. Tests verify both
cross-route exclusion and record-only contention. Route identity remains the
actual guard's gateway-plus-bus key, independent of target.

The bridge persists only guard-generated raw evidence and validates its binding
and ledger integrity. The guard remains the authority for freshness, exact
readback, safe restoration and cleanup facts. Release requires the complete
matching ledger and a checked barrier; visible release after a failed publisher
barrier is reconciled under the namespace lease before reuse. An incompatible
old policy head fails closed rather than being treated as free.

Blocking storage methods are synchronous. A real service integration must keep
these calls off async executor threads and qualify native behavior. The default
Windows adapter remains absent unless its explicit experimental feature is
enabled; that feature is not device authorization.

## Verification

On macOS arm64, pinned Rust 1.99.1 and locked offline cache:

- `cargo test --locked --offline --manifest-path experiments/task156-operation-journal/Cargo.toml`: final default parallel run PASS, 69 tests and one compile-fail doctest.
- Focused Clippy and Windows GNU all-target checks, feature off/on: PASS.
- The native full-16KiB and namespace-lease regression was type-checked, NOT RUN.
- A private Task152/Task153 consumer compile fixture passed; no runtime entrypoint was executed.
- Formatting and diff checks: PASS.

Seven new bridge functions cover full-size storage, partial/corrupt parts,
actual guard sequencing, replay/cancellation/uncertainty, namespace exclusion,
and failures before/during/after each reservation or dispatcher persistence
step. Synthetic grant construction remains test-only. Tests were implemented
alongside the bridge; no historical failure is claimed for an absent API.

The first test compile needed a test-only lease visibility correction. An
initial parallel run passed 67 cases but failed an existing immediate lock
reacquire assertion with Busy; the serial 68-case run passed. Independent
review reproduced the parallel failure and found it absent when the subprocess
test was excluded. The exact Rust process-spawn mechanism remains unproven.
A deliberate duplicate-descriptor diagnostic reproduced the documented Rust
lock lifetime: dropping one handle does not release while another duplicated
or inherited handle survives. The OS-lock/process tests now serialize only
their own group with a test-local mutex. Production zero-wait locking is
unchanged; the final default parallel suite passed. See the [Rust File lock
contract](https://doc.rust-lang.org/std/fs/struct.File.html#method.try_lock).

At this report's checkpoint, native Windows registry/concurrency/crash tests,
physical-service wiring, power-loss behavior and hardware were NOT RUN. The
later bounded native test result is recorded in [`Task-156`](../../tasks/Task-156.md).
The compile fixture is evidence of type compatibility, not a claim that the
protected broker uses this backend.

The next test-only increment prepares [native actual-guard integration cases](NATIVE-GUARD-TESTS.md)
over `WindowsStorage`, including independent processes, abrupt termination,
partial publications and recovery. At the time of this report, they remained
NOT RUN; see [`Task-156`](../../tasks/Task-156.md) for the later result.
