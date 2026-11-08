<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task156 actual-guard persistence seam

Base editor revision: `d31ded9`. Task003 and Task151 are Done; Task156 has no
phase gate. This increment changes persistence selection below the existing
Task020 guard. It does not connect Task156's native storage or add execution,
approval, protocol, IPC, registry access or a device capability.

## Reproducible integration gap

The existing Task020 `Journal` was a concrete filesystem journal. Its complete
Intent contains the immutable plan and may use the existing 16,384-byte record
bound. Task156's independent policy stores only digest bindings and accepts
2,048-byte records. Its event sequence also does not own Task020's authenticated
observations or its exact restoration decisions. Treating the Task156 policy
as an observer or approval source would either omit full plan evidence or add a
second semantic authority. No such mirroring was introduced.

## Implemented boundary

`Journal::open` retains the filesystem backend, filenames, JSON record format,
create-new/file-sync/directory-sync ordering and route-release sequence.
`Journal::with_persistence` explicitly accepts an in-process trusted
`JournalPersistence` backend and requires its preflight before construction.
The same Task020 methods continue to validate approval, plan digest, exact
route/selector/value, observation freshness, response fences, readback,
restoration and cleanup. There is no new approval or observation constructor.

The backend receives the exact guard-generated intent/record bytes and a
read-only binding containing operation hash, plan digest, approval ID and the
route hash. The route remains gateway plus bus path, excluding target: a
second target on the same bus cannot bypass quarantine. The contract requires
complete bounded intent persistence before `begin` succeeds, global operation
and approval uniqueness, create-new durable records, bounded recovery reads,
and exact-binding durable route release only after cleanup evidence. Backend
failure maps to existing guard errors; the guard returns no successful dispatch
or complete report merely because a backend call was attempted.

`RestoredVerified` remains a physical-value observation distinct from cleanup
and route availability. The existing recovery method can report that restored
observation while failed cleanup or release keeps the route quarantined. No
recovery record grants approval or resumes a write.

The trait is a trusted synchronous implementation contract, not a sandbox or
agent-accessible approval API. Implementations must keep blocking storage work
off async executor threads, qualify native durability/concurrency, and reject
records exceeding the supplied limit before allocation. The guard independently
bounds and validates returned bytes as defense in depth. Arbitrary same-user
mutation remains outside the prototype's stated security boundary.

## Focused verification

Tests exercise the actual Task020 dispatcher and guard using an owned,
in-memory persistence fixture. Synthetic grant construction and observations
remain in test-only code. Six new test functions cover a complete plan exceeding
2KiB, exact write/readback/restore/cleanup order, preflight and reservation
failures at each stage, persisted-then-error record effects, failed cleanup or
release, uncertainty/cancellation without retry, target-independent route
exclusion, replay and malformed/oversized recovery. The existing 31 filesystem
and dispatcher cases remain in the same suite.

On macOS arm64 with pinned Rust 1.99.1 and project-private offline Cargo cache:

| Command | Result |
|---|---|
| `cargo test --locked --offline --test task020-guard` | PASS, 37 tests (31 existing plus six alternate-backend functions) |
| `cargo clippy --locked --offline --test task020-guard -- -D warnings` | PASS |
| `cargo check --locked --offline --target x86_64-pc-windows-gnu --test task020-guard` | PASS; type-check only |
| `cargo fmt --all -- --check` | PASS |
| `git diff --check` | PASS |

The first shell command used a nonexistent worktree-local toolchain path and
failed before invoking Cargo; it was corrected to the actual pinned private
toolchain. The first test compile reported an unused binding accessor; the fake
store now verifies that accessor against the exact serialized intent, followed
by a successful Clippy run. Tests were added together with this new seam; no
historical failed test is claimed.

## Remaining native integration

No implementation yet maps this 16KiB full-intent contract to the 2KiB native
Task156 record format. That requires a versioned bounded full-plan representation
and qualified atomic reservation/read/release mapping, retaining complete plan
and cleanup evidence. The existing Task156 policy is not a replacement semantic
engine. The exact-source adapter runner and Task152/153 integration need their
own subsequent regression verification before changing a physical service.
Native Windows storage, cross-process/crash behavior, guard-to-registry
integration, power-loss behavior and hardware are NOT RUN. No task or gate is
accepted by this report.
