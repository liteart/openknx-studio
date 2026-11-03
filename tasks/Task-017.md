# Task-017: Implement and verify minimum non-secure tunneling

## Subject

Implement and verify minimum non-secure tunneling

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-017 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / KNXnet-IP transport |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Provide the smallest reliable discovery/connect/disconnect path needed for the selected test gateway.

## Detailed Requirements

- DR-001: Use existing public implementations where verified; fill only demonstrated gaps.
- DR-002: Support an explicit gateway address so multicast discovery is not a hard dependency.
- DR-003: Bound connect timeout, session lifetime handling and retry counts; record the chosen protocol-supported values.
- DR-004: Treat secure-only interfaces/configurations as unsupported without fallback.
- DR-005: Keep wire parsing, response matching and connection lifecycle entirely in the fork.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Offline tests and the approved gateway connection produce a valid non-secure tunnel and clean disconnect.
- [ ] AC-002: Wrong/delayed replies cannot be accepted for another session.
- [ ] AC-003: Secure-required and invalid endpoint requests fail explicitly.

## Dependencies to Other Tasks

- [Task-015](Task-015.md): Audit the pinned knust fork against the PoC operations. Required predecessor behavior/evidence must be accepted.
- [Task-016](Task-016.md): Build a tiny deterministic KNX management peer simulator. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Simulator: normal connect/disconnect, timeout, wrong session and malformed frame.
- TEST-002: Hardware read-only: connect to the approved gateway and close.
- TEST-003: Negative: secure-required selection is rejected.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / KNXnet-IP transport**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No routing, USB, RF, keys, application download, or editor-specific session policy.

## Risks and Blockers

Hardware connection evidence requires the registered lab gateway; offline success alone is not hardware qualification.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; bounded offline tunnel hardening only |
| Source commit(s) | Local fork `e4ea7c198350d8a0fb819cd5d215a7b2f03ef6a4`, based on Task016 `ca45fa6d7f5e3f23f27d275830bf425c87385550` |
| Tests executed | TEST-001 and secure-selection negative PASS in named offline tests; TEST-002 hardware NOT RUN |
| Evidence location | Fork commit and checkpoint below |
| Acceptance criteria met | None asserted |
| Technical outcome | Invalid/malformed endpoints and replies rejected; stale transport retired in tested paths; hardware and protocol timing unqualified |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Unassigned |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Offline hardening checkpoint — 2026-11-03

Local fork branch `task/Task-017-tunnel` contains the source commit above; no
fork remote was pushed. On macOS, `cargo test --locked --test
test_tunnel_session --test test_management_peer` passed 15 tunnel tests and 14
simulator tests using only loopback sockets. The sandbox initially restricts
socket binding; the explicitly loopback-only run used sandbox escalation.
`cargo clippy --locked --offline --lib --test test_tunnel_session --test
test_management_peer -- -D warnings`, `cargo fmt --all -- --check`, and
`git diff --check` passed. Independent review reproduced and verified fixes
for a short-length frame panic and malformed successful connection responses.

This increment checks the selected tunnel response's exact CRD, correlates
disconnect replies with the active channel, rejects malformed frames and
invalid endpoints/timeouts, and retires tested stale transport paths. It does
not establish primary-source protocol timing/retry values, cancellation or
shared-close behavior, or hardware qualification. The required approved
read-only gateway connection/disconnect remains NOT RUN and no device was
contacted. Task017 remains In progress; no physical-operation approval is
implied by the simulator tests.

Source SHA-256: `src/protocol/knxip.rs`
`3b9f1c3d8dffb65956a7c356f67d9ae288d47bb5df022926aecad326515f74d0`;
`src/transport/tunnel.rs`
`7ef941cedfa66dc1f9e07dceb5eeff996daca2ee62696a5d7032030647337ab8`;
`tests/test_tunnel_session.rs`
`db5fde62c3684858187a6953dcc7d88892a06f7e3c25f252a7fe6f9cc5a49696`.
