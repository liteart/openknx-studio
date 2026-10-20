# Task-041: Detect and validate external filesystem and Git changes

## Subject

Detect and validate external filesystem and Git changes

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-041 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / project watcher and adoption |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Make direct JSON editing and external Git operations safe without silently accepting an inconsistent project.

## Detailed Requirements

- DR-001: Use watchers only as hints and rescan on open/before mutations and deployment.
- DR-002: Treat modified files or branch changes as candidate snapshots requiring complete validation/adoption.
- DR-003: Reject conflict markers, missing files and invalid schemas while preserving the last accepted service snapshot.
- DR-004: Invalidate stale changesets, plans and cache projections after a validated revision change.
- DR-005: Do not auto-commit unrelated external edits or assert that the app lock blocks all external programs.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Valid external changes can be previewed and explicitly adopted.
- [ ] AC-002: Invalid/partial edits cannot leak into compilation or accepted queries.
- [ ] AC-003: A missed watcher notification is caught by the mandatory rescan.

## Dependencies to Other Tasks

- [Task-035](Task-035.md): Load and validate a complete accepted project snapshot. Required predecessor behavior/evidence must be accepted.
- [Task-036](Task-036.md): Implement isolated changesets and optimistic revision checks. Required predecessor behavior/evidence must be accepted.
- [Task-038](Task-038.md): Recover interrupted configuration transactions deterministically. Required predecessor behavior/evidence must be accepted.
- [Task-040](Task-040.md): Build a disposable revision-tagged SQLite read projection. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: external edit/rename/checkout and adoption.
- TEST-002: Negative: partial multi-file write, conflict markers and stale plan execution.
- TEST-003: Fault: dropped watcher notification and external Git race.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / project watcher and adoption**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No background merge, silent repair or shared-network-filesystem support.

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
