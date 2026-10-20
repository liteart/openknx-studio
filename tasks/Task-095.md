# Task-095: Fetch and adopt validated fast-forward changes

## Subject

Fetch and adopt validated fast-forward changes

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-095 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Git fetch and pull |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md) |

## Description

Bring remote changes into the project only through an isolated validated candidate snapshot.

## Detailed Requirements

- DR-001: Fetch objects without running remote hooks/submodules or artifact code.
- DR-002: Build and validate a candidate against schema, IDs, graph and mandatory blobs before adopting it.
- DR-003: Require a clean/nonconflicting local state and a genuine fast-forward unless the semantic merge flow is used.
- DR-004: Invalidate stale changesets/plans/cache after adoption, preserve durable device receipts.
- DR-005: A remote update must never trigger device programming.
- DR-006: Expose git_fetch and git_adopt as separate MCP steps; fetch alone must not change accepted state.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A valid fast-forward becomes one coherent accepted snapshot.
- [ ] AC-002: Invalid/missing-artifact/diverged/dirty candidates leave current state unchanged.
- [ ] AC-003: Previously compiled plans cannot execute against an adopted different revision.

## Dependencies to Other Tasks

- [Task-093](Task-093.md): Configure optional Git remotes and protected credentials. Required predecessor behavior/evidence must be accepted.
- [Task-035](Task-035.md): Load and validate a complete accepted project snapshot. Required predecessor behavior/evidence must be accepted.
- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.
- [Task-041](Task-041.md): Detect and validate external filesystem and Git changes. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: fast-forward and accepted revision update.
- TEST-002: Negative: dirty worktree, conflict markers, unknown schema, missing blob and malicious repository config.
- TEST-003: Safety: pull/adoption produces zero device writes.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Git fetch and pull**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No auto-merge, auto-download, force reset or execution of repository hooks.

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
