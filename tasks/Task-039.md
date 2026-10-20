# Task-039: Expose automatic local configuration history and restore

## Subject

Expose automatic local configuration history and restore

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-039 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / local Git service |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Provide transparent local Git history to every user, independent of Git remotes or paid accounts.

## Detailed Requirements

- DR-001: Initialize a repository and explicit local author identity for new projects without requiring remote credentials.
- DR-002: Commit one validated logical changeset with actor/operation metadata and no commit for a no-op.
- DR-003: Expose history, exact revision diff and restore-preview using canonical project validation.
- DR-004: Restore as a new accepted commit, preserving audit history instead of rewriting it.
- DR-005: Mark physical state independently; restore never initiates a device download.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A project can be created, edited, inspected and restored fully offline.
- [ ] AC-002: Restoring an old revision produces a new valid desired state and retained history.
- [ ] AC-003: No-op and history reads leave Git unchanged.

## Dependencies to Other Tasks

- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.
- [Task-038](Task-038.md): Recover interrupted configuration transactions deterministically. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: init/edit/diff/restore round-trip.
- TEST-002: Negative: dirty/unrelated content and invalid historical snapshot are rejected safely.
- TEST-003: Safety: protocol mock receives zero calls for every Git operation.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / local Git service**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No remote integration, force push, or hardware rollback.

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
