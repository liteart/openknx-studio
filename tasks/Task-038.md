# Task-038: Recover interrupted configuration transactions deterministically

## Subject

Recover interrupted configuration transactions deterministically

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-038 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / startup recovery |
| Owner | Unassigned |
| Requirement references | [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Recover the accepted configuration from journals and Git evidence after process or storage failure.

## Detailed Requirements

- DR-001: Inspect transaction phase, intended parent/tree/commit, current Git reference and old/new content hashes.
- DR-002: Complete the new snapshot only when the intended commit was published; otherwise restore the previous accepted state.
- DR-003: Stop for reconciliation if an unrelated external revision or conflicting file content prevents safe recovery.
- DR-004: Invalidate caches and prevent writes until recovery is complete.
- DR-005: Make recovery idempotent and retain diagnostic evidence for unresolved transactions.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Each injected crash position resolves to the complete old or new accepted snapshot, never a mixed service view.
- [ ] AC-002: Repeating recovery makes no additional change.
- [ ] AC-003: External conflicting edits are not overwritten silently.

## Dependencies to Other Tasks

- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Fault matrix: terminate at every journal/file/ref publication stage.
- TEST-002: Integration: repeat recovery and then load directly from files.
- TEST-003: Negative: unrelated branch move, corrupted journal and missing recovery content.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / startup recovery**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No rollback of physical devices, automatic force-reset, or reliance on SQLite for recovery.

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
