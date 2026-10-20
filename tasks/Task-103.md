# Task-103: Preview and accept one-time KNXPROJ migration atomically

## Subject

Preview and accept one-time KNXPROJ migration atomically

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-103 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / import orchestration and MCP |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Expose migration as a one-time reviewed transition into an independent project.

## Detailed Requirements

- DR-001: Return a dry-run report with imported counts, addresses, retained/unsupported data, required missing artifacts and deployment limitations.
- DR-002: Accept only a validated complete candidate for the selected scope, preserving the original export hash and migration report.
- DR-003: Commit the initial independent snapshot atomically through the normal transaction layer.
- DR-004: Re-import into an existing project is not a synchronization/upsert feature; require a separate explicit migration flow.
- DR-005: Never contact devices or reinterpret imported download flags as present physical verification.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The supported fixture becomes a standalone Git-versioned project with preserved configuration.
- [ ] AC-002: A blocked preview cannot be silently committed as complete.
- [ ] AC-003: Opening/editing the resulting project works without ETS or the original ETS machine.

## Dependencies to Other Tasks

- [Task-100](Task-100.md): Import project identity, topology and device instances. Required predecessor behavior/evidence must be accepted.
- [Task-101](Task-101.md): Import parameter assignments and group-object bindings. Required predecessor behavior/evidence must be accepted.
- [Task-102](Task-102.md): Preserve imported product artifacts and DCA state. Required predecessor behavior/evidence must be accepted.
- [Task-035](Task-035.md): Load and validate a complete accepted project snapshot. Required predecessor behavior/evidence must be accepted.
- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.
- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.
- [Task-085](Task-085.md): Expose project, catalog and changeset MCP tools. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: MCP integration: preview/accept/open/edit on a fresh environment.
- TEST-002: Fault: cancellation/disk failure during migration commit.
- TEST-003: Negative: partial loss report, repeated import into existing project and imported false synchronized state.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / import orchestration and MCP**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No ongoing ETS sync, automatic repair of source omissions or device downloads during migration.

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
