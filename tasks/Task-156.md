# Task-156: Qualify a Windows operation-journal boundary

| Field | Value |
|---|---|
| Status | In progress |
| Stage | PoC platform qualification |
| Component | knx-editor / guarded operation persistence |
| Dependencies | Task-003, Task-151; Task020 guard model as a test contract |
| Required phase gate | None; no gate is advanced |

Task152's experimental Windows journal persists a development passkey receipt;
it does not implement the separate Task020 operation journal or its reusable
gateway-route lock. Develop a Windows-capable, bounded persistence boundary
that preserves the Task020 guard's exact-plan, one-use and unknown-outcome
invariants. Work is offline and simulator-only; it grants no device authority.

## Requirements

- Preserve durable intent and one-use consumption before a dispatch can be
  selected. Incomplete, malformed, failed or interrupted operations recover as
  unknown and quarantine their route; never retry or restore automatically.
- Serialize cooperating processes for a route. A process crash may release an
  OS mutex but must not silently release persisted active or unknown state.
  Only verified restoration and confirmed device and gateway cleanup may
  release the route, and a failed persistence step must not report release.
- Keep all native APIs behind a non-default experimental feature until native
  Windows tests pass. The feature is not an authorization boundary. Avoid
  project-owned unsafe code where a reviewed safe wrapper is sufficient; any
  required unsafe must meet AGENTS.md's test and human-review requirements.
- Keep the existing Unix journal behavior intact. Do not widen the fixed
  Task153 plan, add an approval issuer, access KNX hardware, or treat a local
  same-user store or mutex as protection from arbitrary same-user code.
- Pin and review any dependencies, preserve bounds, and record the platform,
  exact commits and command results. A cross-check or fake store is not a
  native Windows crash or durability test.

## Acceptance and tests

- Deterministic fault tests cover each persistence boundary, replay, mismatched
  plan, cancellation, crash/reopen, cleanup failure and route exclusivity.
- Native Windows tests verify concurrent processes, durable state after process
  termination, failed/partial records and route release; power-loss limits and
  same-account interference are explicit. Windows provider and actual hardware
  remain separately qualified.
- macOS/Linux regressions and Windows cross-checks preserve existing behavior;
  independent review checks the real implementation and evidence before Done.

## Completion record

The first increment implements a portable policy and owned crash/fault
simulator, not a Windows backend or a Task020 integration. Independent review
found two persistence/transition issues; both were fixed and their regressions
passed. Source commit `bdb844067c6748e2ef2b164f215253234d9264cf` was
integrated as `32bcd86`.

The implementer and independent reviewer each ran 14 simulator tests; the
coordinator repeated `cargo test --locked --offline --manifest-path
experiments/task156-operation-journal/Cargo.toml` on main: 14 PASS. Focused
Clippy, Windows GNU type-check, formatting and diff checks passed in the
isolated worktree. Commands, fault model and limitations are recorded in
`experiments/task156-operation-journal/README.md`.

The next increment adds an opt-in Windows registry adapter and a stable,
namespace-wide cooperative file lock; it has no device capability and is not
integrated with Task020. The first review found an unbounded registry-value
read; it was replaced with bounded chunk-key enumeration and independently
re-reviewed. Reviewed source `ce0f82db6a05d7cb94e67d4aa772483b90c6d339` was
integrated as `757dd05`. The coordinator repeated the locked offline suite on
main: 24 macOS tests and one compile-fail doctest PASS. Independent review
also repeated those tests and the Windows GNU feature-on all-target check;
the implementer checked feature-off/on and Clippy. The exact storage format,
commands and limits are in `experiments/task156-operation-journal/WINDOWS-STORAGE.md`.

Native Windows registry, cross-process and crash tests, Task020 guard
integration and hardware are NOT RUN. The experimental Task152 receipt journal
is not evidence that this operation journal is qualified. The Windows feature
remains disabled by default and is not operation authorization.

A subsequent increment added an explicit trusted persistence seam below the
actual Task020 guard, retaining its approval/observation checks, full 16 KiB
intent and existing default filesystem behavior. It does not connect the
2 KiB Task156 storage format or provide a Windows backend for the guard.
Independent review passed 37 guard tests, 66 Task152 library tests and a
Task153 library compatibility check; the coordinator repeated `cargo test
--locked --offline --test task020-guard` on main: 37 PASS. Reviewed source
`5c82ae05d3c5bd3e5d3df4f235b565e7b3a0e285` was integrated as `805dd21`.
Contract, commands and limits are in
`experiments/task156-operation-journal/GUARD-PERSISTENCE.md`. Native Windows
storage mapping, cross-process tests and physical service redeployment remain
NOT RUN; no new device authorization or phase gate is implied.

The next increment connects the actual guard persistence contract to bounded
storage through a consumer-local bridge. It preserves a full 16 KiB intent as
immutable bounded parts with an exact binding/hash manifest, uses an explicit
namespace-wide lease, and keeps approval and observation authority in the
existing guard. The adapter is not activated in the physical service.
Reviewed source `327b3e5497d4a414c98d71ca4b425ba576689464` was integrated as
`194cc36`. The coordinator and independent reviewer each ran the normal
parallel `cargo test --locked --offline --manifest-path
experiments/task156-operation-journal/Cargo.toml`: 69 tests and one
compile-fail doctest PASS. Windows GNU feature-on type-check passed, as did a
compile-only fixture using the actual Task152/153 guard types. A prior parallel
lock-test failure was reproducible; test-only coordination corrected it without
changing the production lock. Details and limits are in
`experiments/task156-operation-journal/GUARD-STORAGE-BRIDGE.md`. Native Windows
registry/process recovery and physical-service integration remain NOT RUN.

Native Windows tests now exercise the actual Task020 guard, dispatcher,
consumer bridge and `WindowsStorage` with synthetic approvals and sessions.
They cover full intent and replay, injected partial persistence, process
contention and termination. Independent review found and verified corrections
to the race-loser assertion, bounded child cleanup and proof of fault
publication. Reviewed source `82b653f5edcda112779439dfc1e485dcd1363aaf` was
integrated as `8940f2e`. The macOS suite passed 69 tests and one doctest;
Windows GNU feature-on all-target check and Clippy passed. Exact commands and
limits are in `experiments/task156-operation-journal/NATIVE-GUARD-TESTS.md`.
These Windows tests have only been type-checked: native registry/process
execution, physical service integration and hardware remain NOT RUN.

The Windows x86_64 synthetic test harness was cross-linked from editor
`69b2f9b452176f56b2eb03c7078bc3526ad0c5b2` using pinned Rust 1.99.1 and
verified llvm-mingw 20261104. The locked/offline `cargo test --no-run` build
passed; the private executable SHA-256 is
`d1472ed00ee2dd23c9bab1d6d61ab0e99dafec8c7c011aa5b684875180eefa7a`.
An independent verifier checked saved hashes, architecture and static imports,
but did not reproduce the build. Commands, runtime dependency and limits are
in `private/evidence/task152-156-windows-cross-20261108/README.md`, SHA-256
`b860fb14269d7fc7c307c42daf6b5a86e2945990358b65cfe1d539410450d72c`.
Native Windows registry/process tests remain NOT RUN.

After a review identified unbounded child waits, source `1cc2204029aabd8c98373c0a34b99afdaab78916`
was integrated as `91bea8c` and independently checked. The corrected Windows
test harness was cross-linked from `91bea8c7dc45b8e6f2ca49013c45d886cd8bfe01`;
its SHA-256 is `cdcbf0e6229c772c53f8670d41008880d46544030bffc55cf39314f37c217d10`.
The private build record is
`private/evidence/task152-156-windows-cross-91bea8c-20261108/README.md`,
SHA-256 `8f336ef413fc7b4522dae413af44900d2f8822d54cb7e72340b78f3a8143fc5b`.
An independent static review verified artifact hashes and imports, not the
build itself. This supersedes the earlier test executable; native execution
remains NOT RUN.
