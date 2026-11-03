# Task-016: Build a tiny deterministic KNX management peer simulator

## Subject

Build a tiny deterministic KNX management peer simulator

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-016 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / tests and test support |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Create only the simulator/fault injection needed to test the selected management operations before hardware access.

## Detailed Requirements

- DR-001: Model the selected tunnel/session responses and a small deterministic property/memory fixture.
- DR-002: Support injected dropped/delayed/duplicate replies, protocol error replies, and disconnects.
- DR-003: Keep simulator frames/specimens in the fork and label the simulated device model explicitly.
- DR-004: Use a fake clock or deterministic timers where feasible; never discover real gateways.
- DR-005: Do not implement project, catalog, or manufacturer configuration semantics.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: Tests run offline and deterministically.
- [x] AC-002: Each supported injected fault produces an observable expected result.
- [x] AC-003: The simulator is not advertised as a compliant real KNX device or hardware proof.

## Dependencies to Other Tasks

- [Task-015](Task-015.md): Audit the pinned knust fork against the PoC operations. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: state transitions and injected faults.
- TEST-002: Integration: a non-secure client communicates with the simulated peer.
- TEST-003: Negative: assert no default network target points outside the test process/loopback.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / tests and test support**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No comprehensive KNX simulator, DALI simulator, or Secure simulation.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Done for the bounded synthetic peer; no hardware or wire-conformance claim |
| Source commit(s) | Fork `ca45fa6d7f5e3f23f27d275830bf425c87385550`, based on `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e` |
| Tests executed | TEST-001–003 PASS; 14 named target tests, including loopback client |
| Evidence location | Fork commit and reproduction details below |
| Acceptance criteria met | AC-001–003 for the explicitly simulated scope |
| Technical outcome | Deterministic tunnel/session peer, bounded fault script and synthetic property/memory fixtures |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent agent review; maintainer acceptance not asserted |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Completion evidence — 2026-11-03

The fork commit is local on `task/Task-016-simulator`; it has not been pushed
or published. Its new test-support files are Apache-2.0 and retain the fork's
existing MIT license and upstream notices. Source SHA-256:

- `SIMULATOR.md`: `4f9695c212fca439fd528dcdf3bb1fb353561db7f27e80314a71f53ee476b125`
- `tests/support/management_peer.rs`: `cd7589bc0f975c58dac723d485720de59543c37620503bc970fa13b8e4358cd4`
- `tests/test_management_peer.rs`: `8a0b53830d1a37fa4fc14cdadf737c7e0a86a7e023ad8105bb3bec0ba71afcc7`

On macOS with the pinned toolchain and offline cache, `cargo test --locked
--test test_management_peer` passed all 14 tests, including the existing
non-secure client against a loopback-only peer. The initial sandboxed loopback
bind was denied; the explicitly local rerun with sandbox escalation passed.
`cargo clippy --locked --offline --no-default-features --features dpt --test
test_management_peer -- -D warnings`, `cargo fmt --all -- --check`, and
`git diff --check` passed. The reviewer independently checked 13 socket-free
tests and the corrected malformed-frame regression; the reviewer's loopback
attempt was sandbox-denied and is not counted as a separate pass.

The property and memory values are bounded synthetic in-process fixtures;
their wire encodings are not implemented or qualified here. Existing codecs
are reused rather than independently validated against a specification. No
discovery, external gateway, Secure feature, real device, hardware access or
KNX wire-conformance proof was exercised.
