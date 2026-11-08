<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task156 operation-journal policy increment

Base editor revision: `a80b769b645d51af2008f299117a9c7801c10798`.
Task003 and Task151 are Done; Task156 has no phase gate. The existing Task020
guard and Task152 journal were inspected as contracts and remain unchanged.

## Implemented scope

This executable-free crate implements a portable persistence policy and an
owned deterministic fault simulator. It is **not a Windows journal adapter**,
an OS mutex, an approval mechanism, or a network dispatcher. No native or
physical device operation can be selected through its API.

The caller supplies opaque route, operation, plan and approval identifiers.
Their nonzero format is checked, but this does not validate their origin or
approve a plan. Each operation reserves the route before persisting globally
unique operation/approval reservations and intent. Every write is followed by
the storage barrier before the policy returns a successful live attempt.

One non-clone live attempt can record only the ordered desired-intent,
desired-readback, restore-intent, restored-readback and cleanup sequence.
The caller must supply independently verified observations; this policy does
not establish that a readback or cleanup actually occurred. The exact plan
digest and complete existing ledger are rechecked before each transition.
Any failed transition or cancellation makes that live attempt unusable.
No restoration, retry, command or observation is generated automatically.

The route lease must serialize cooperating processes, while the persisted
active head survives the release or loss of the OS lease. Reopening active
state reports quarantine even if intermediate readback records exist. A
released head is accepted only with all exact matching records present.
It also requires a fresh successful storage barrier while holding the route
lease, both for inspection and reuse. Visible registry values alone cannot
establish durable route release. Inspection is therefore explicit persistence
reconciliation rather than a pure read.
Malformed, unknown, oversized, missing or mismatched records cannot make a
route available. Records are bounded JSON with duplicate/unknown struct fields
rejected; there is no unbounded history scan or automatic record deletion.

The storage trait is a trusted adapter contract: bounded reads, atomic global
create-new operation/approval keys, exclusive route lease and real durable
barriers. The included fake meets it only within its explicit in-memory crash
model. It does not demonstrate actual registry, filesystem, cross-process or
power-loss behavior. Calls are synchronous and any future integration must
keep blocking native storage work off async executor threads.

## Fault and recovery interpretation

The simulator distinguishes visible state from a durable snapshot and can
fail each mutation/barrier before, during or after its effect. A failure before
the initial route reservation becomes durable may recover a free route: no
live attempt or dispatch capability was returned. Once durable active state
exists, incomplete operations recover quarantined.

A final flush can fail before persistence while leaving a released head visible,
or can persist and nevertheless return an error. The live attempt reports
OutcomeUnknown, never Released. Another cooperating journal instance cannot
reuse that visible release without validating the complete terminal ledger and
completing its own barrier under the route lease. If that barrier fails, both
inspection and begin fail closed. A later successful barrier can reconcile the
recorded restoration/cleanup and release; this retries persistence only, never
device work. Native storage must qualify this recovery interpretation.

## Checks executed

Platform: macOS arm64 with pinned Rust 1.99.1 and project-private offline Cargo
cache. The task-owned target is outside source. Exact commands:

```text
cargo test --locked --offline --manifest-path experiments/task156-operation-journal/Cargo.toml
cargo clippy --locked --offline --manifest-path experiments/task156-operation-journal/Cargo.toml --all-targets -- -D warnings
cargo check --locked --offline --manifest-path experiments/task156-operation-journal/Cargo.toml --target x86_64-pc-windows-gnu --all-targets
cargo fmt --manifest-path experiments/task156-operation-journal/Cargo.toml --all -- --check
git diff --check
```

Results: 14 simulated unit tests PASS; focused Clippy, Windows-target type
check, formatting and whitespace checks PASS. No doctests exist. The tests
were added together with the implementation. An initial Clippy run rejected
two explicit `drop` calls on a non-Drop test value; scopes replaced them and
the final test/Clippy run passed. No network, registry, filesystem durability
or hardware was exercised by the tests.

Independent review found two policy gaps. Before the fixes, focused regressions
FAILED because an immediately visible release was reported available after a
failed final flush, and wrong-plan validation left a live attempt Ready. The
same tests now pass: all existing released-state reads/reuse require their own
successful barrier, and every `record` error latches the attempt before any
validation or storage action. The release regression checks a second journal
instance before simulated crash, including both failed inspection and failed
begin, not only a reopened durable snapshot.

The fault matrix covers every begin/transition persistence point, partial
records, failed flush after persistence, replay across routes, wrong plans,
invalid ordering, cancellation, lost attempts, missing prior evidence, malformed
terminal state and concurrent cooperating creators. The fake uses one global
mutex, which is stronger than a per-route lease; it does not prove native
cross-process exclusivity.

All 11 dependency archives were matched to Cargo.lock checksums and inspected
for license metadata in the existing cache. Versions are pinned; no package
was fetched. See `DEPENDENCIES.tsv`. Owned source forbids unsafe code; no
claim is made that inherited dependencies contain no unsafe.

## Remaining work

Native registry persistence and route mutex adapters, protected exact-plan
approval/observation integration, native Windows process/crash tests, Linux
execution and storage power-loss qualification are NOT RUN or absent. Native
APIs must remain behind a non-default experimental feature until qualified.
This policy is not integrated into Task020 or Task153 and cannot replace their
existing safety boundaries. Task156 remains incomplete; no gate is advanced.
