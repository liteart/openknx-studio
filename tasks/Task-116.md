# Task-116: Execute the full file, Git and cache crash matrix

## Subject

Execute the full file, Git and cache crash matrix

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-116 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / persistence reliability |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Validate durable-state invariants under the actual supported platform filesystem behavior.

## Detailed Requirements

- DR-001: Inject failures at staged file write, flush, rename, Git index/ref update, result publication, journal append and cache swap.
- DR-002: Cover disk-full, permissions, external branch/file conflicts and truncated operation records.
- DR-003: Verify the service recovers to coherent old/new accepted configuration or explicitly stops for reconciliation.
- DR-004: Confirm database deletion loses no authoritative information.
- DR-005: Record OS/filesystem-specific limitations instead of promising global multi-file atomicity.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every declared failure point has a repeatable expected outcome.
- [ ] AC-002: No acknowledged configuration mutation is lost after supported recovery.
- [ ] AC-003: External unrelated changes are not destroyed and uncertain hardware state is retained.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.
- [Task-038](Task-038.md): Recover interrupted configuration transactions deterministically. Required predecessor behavior/evidence must be accepted.
- [Task-040](Task-040.md): Build a disposable revision-tagged SQLite read projection. Required predecessor behavior/evidence must be accepted.
- [Task-041](Task-041.md): Detect and validate external filesystem and Git changes. Required predecessor behavior/evidence must be accepted.
- [Task-042](Task-042.md): Persist job journals and deployment receipts as files. Required predecessor behavior/evidence must be accepted.
- [Task-097](Task-097.md): Merge project JSON semantically by stable identity. Required predecessor behavior/evidence must be accepted.
- [Task-098](Task-098.md): Verify artifact portability across clones and remotes. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Fault-injection matrix on each required local filesystem/OS environment.
- TEST-002: Recovery repeatability: restart twice and compare accepted state.
- TEST-003: Safety: verify no auto-deploy after recovering Git state.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / persistence reliability**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No active network-share/cloud-sync support or physical device rollback test.

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
