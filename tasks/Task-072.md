# Task-072: Recover interrupted deployment jobs without false rollback

## Subject

Recover interrupted deployment jobs without false rollback

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-072 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / deployment recovery |
| Owner | Unassigned |
| Requirement references | [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Classify and safely reconcile jobs that stopped while the device may have changed.

## Detailed Requirements

- DR-001: Reconstruct state from durable job intent/results rather than SQLite or in-memory task state.
- DR-002: Determine whether a profile allows verification, restart or no automated recovery at each safe boundary.
- DR-003: Mark ambiguous execution as unknown/interrupted and require renewed scoped approval for new writes.
- DR-004: Never restore Git state and claim the device rolled back.
- DR-005: Preserve evidence after crash/timeout and avoid duplicate side effects during repeated recovery.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every injected interruption is classified with a documented permitted next action.
- [ ] AC-002: Repeated startup cannot accidentally resume or repeat a firmware/configuration step.
- [ ] AC-003: Unverifiable device state remains explicit unknown.

## Dependencies to Other Tasks

- [Task-042](Task-042.md): Persist job journals and deployment receipts as files. Required predecessor behavior/evidence must be accepted.
- [Task-071](Task-071.md): Execute durable sequential programming jobs. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Fault matrix: termination before dispatch, after dispatch, before receipt and after receipt.
- TEST-002: Simulator: safe restart allowed/denied by profile.
- TEST-003: Negative: missing evidence or external device change.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / deployment recovery**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No universal rollback, automatic factory reset or restart without profile evidence.

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
