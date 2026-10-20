# Task-073: Compare desired, compiled, deployed and observed states

## Subject

Compare desired, compiled, deployed and observed states

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-073 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / deployment status |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Answer which devices need review/programming without confusing a Git diff with actual device state.

## Detailed Requirements

- DR-001: Compare program fingerprints against durable verified receipts while retaining source revision and observation timestamps.
- DR-002: Report unchanged, pending change, unverified, unknown and failed/interrupted distinctions.
- DR-003: A label/locale-only edit must not create a false payload difference; a Git restore must not change a receipt.
- DR-004: Treat physical reads and imported historic flags as separate evidence levels.
- DR-005: Rebuild results identically after SQLite deletion.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference edit/program/rename/restore scenario reports the expected device status.
- [ ] AC-002: Unverified or stale physical state is not called synchronized.
- [ ] AC-003: Cache rebuild preserves all status distinctions.

## Dependencies to Other Tasks

- [Task-059](Task-059.md): Calculate deterministic configuration impact. Required predecessor behavior/evidence must be accepted.
- [Task-068](Task-068.md): Create reproducible immutable plans and compiler fingerprints. Required predecessor behavior/evidence must be accepted.
- [Task-071](Task-071.md): Execute durable sequential programming jobs. Required predecessor behavior/evidence must be accepted.
- [Task-072](Task-072.md): Recover interrupted deployment jobs without false rollback. Required predecessor behavior/evidence must be accepted.
- [Task-040](Task-040.md): Build a disposable revision-tagged SQLite read projection. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: fingerprint versus source-commit comparisons.
- TEST-002: Integration: compile/program/cosmetic edit/restore and missing receipt.
- TEST-003: Negative: stale observation, external device change and corrupted cache.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / deployment status**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No continuous monitoring daemon or guarantee that an old verified receipt describes the device forever.

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
