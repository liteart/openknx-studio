# Task-037: Commit multi-file changes with a recoverable Git transaction

## Subject

Commit multi-file changes with a recoverable Git transaction

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-037 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / persistence transaction writer |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Implement the durable accepted-snapshot commit point described in DATA-MODEL instead of exporting SQLite periodically.

## Detailed Requirements

- DR-001: Use one writer lease and record expected Git parent/accepted revision plus old/new file content and hashes in a durable journal.
- DR-002: Stage only the transaction paths, validate the entire candidate, replace files and publish the intended Git commit with an expected-parent check.
- DR-003: Acknowledge success only after the configuration and commit point are durable; treat cache updates as optional projection work.
- DR-004: Define pre-commit rollback and post-commit completion recovery records.
- DR-005: Preserve unrelated external edits/index contents and fail on conflicting state.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A successful API mutation returns an exact accepted revision and local Git commit.
- [ ] AC-002: A failed pre-commit transaction leaves the previous accepted snapshot recoverable.
- [ ] AC-003: No unrelated file appears in the commit and no device write occurs.

## Dependencies to Other Tasks

- [Task-033](Task-033.md): Implement deterministic multi-file serialization. Required predecessor behavior/evidence must be accepted.
- [Task-035](Task-035.md): Load and validate a complete accepted project snapshot. Required predecessor behavior/evidence must be accepted.
- [Task-036](Task-036.md): Implement isolated changesets and optimistic revision checks. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: one-file/multi-file/no-op changesets and exact staged paths.
- TEST-002: Fault injection: disk-full, permission failure and changed Git parent at each commit phase.
- TEST-003: Durability: crash before/after reference publication using the recovery harness.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / persistence transaction writer**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No claim that multi-file rename is globally atomic; no remote push or automatic deployment.

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
