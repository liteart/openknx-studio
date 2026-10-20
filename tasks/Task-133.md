# Task-133: Persist visual layouts without affecting device compilation

## Subject

Persist visual layouts without affecting device compilation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-133 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / visual layout persistence |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md) |

## Description

Retain graph layout preferences separately from execution-affecting configuration.

## Detailed Requirements

- DR-001: Define a versioned layout document keyed by stable entity IDs, with explicit personal versus shareable storage policy.
- DR-002: Preserve coordinates/group display state without duplicating signal memberships.
- DR-003: Allow explicit Git-versioned shared layouts while keeping personal transient state separate.
- DR-004: Exclude all layout values from program fingerprints and hardware impact.
- DR-005: Handle missing/removed entities gracefully without recreating devices.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Layout survives restart and optional sharing without changing device payloads.
- [ ] AC-002: A layout-only commit requires no programming.
- [ ] AC-003: Invalid layout data cannot corrupt the engineering project.

## Dependencies to Other Tasks

- [Task-131](Task-131.md): Implement a read-only device and signal graph view. Required predecessor behavior/evidence must be accepted.
- [Task-132](Task-132.md): Implement previewed visual connection editing. Required predecessor behavior/evidence must be accepted.
- [Task-033](Task-033.md): Implement deterministic multi-file serialization. Required predecessor behavior/evidence must be accepted.
- [Task-068](Task-068.md): Create reproducible immutable plans and compiler fingerprints. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/schema: layout version/identity/bounds.
- TEST-002: Integration: move/reopen/share and compare compiler fingerprint.
- TEST-003: Negative: deleted node, corrupt layout and unsupported version.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / visual layout persistence**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No layout data as source of KNX connections or accidental renumbering.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Not started |
| Source commit(s) | Not created |
| Tests executed | NOT RUN |
| Evidence location | Not produced |
| Acceptance criteria met | None asserted |
| Technical outcome | Not evaluated |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Unassigned |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).
