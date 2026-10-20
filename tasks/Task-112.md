# Task-112: Approve the beta functionality and portability gate

## Subject

Approve the beta functionality and portability gate

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-112 |
| Status | Not started |
| Stage | Beta gate |
| Priority within stage | P1 |
| Component / Git project | knx-editor / release governance |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Confirm the planned user workflows and project portability before stabilization starts.

## Detailed Requirements

- DR-001: Review original DALI and separate firmware-update evidence, one-time project import and remote Git/merge/backup behavior.
- DR-002: Require a finite platform/device/DCA matrix and complete critical-workflow coverage or an explicit approved scope change.
- DR-003: Confirm every database is disposable and no Git operation triggers hardware.
- DR-004: Check unsupported Secure/legacy/general-app paths and lawful original dependencies.
- DR-005: Record maintainer GO/NO-GO with unresolved defects by severity.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: All mandatory beta tasks are accepted and linked to real evidence.
- [ ] AC-002: Missing firmware/hardware/import prerequisites cannot be waived silently.
- [ ] AC-003: NO-GO blocks the release candidate gate.

## Dependencies to Other Tasks

- [Task-098](Task-098.md): Verify artifact portability across clones and remotes. Required predecessor behavior/evidence must be accepted.
- [Task-103](Task-103.md): Preview and accept one-time KNXPROJ migration atomically. Required predecessor behavior/evidence must be accepted.
- [Task-111](Task-111.md): Publish per-function DCA qualification and unresolved gaps. Required predecessor behavior/evidence must be accepted.
- [Task-092](Task-092.md): Approve the alpha integration gate. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO; this task decides G-BETA.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Gate audit: trace requirements to results and inspect clean restored projects.
- TEST-002: Negative: missing critical DCA function/import data triggers NO-GO.
- TEST-003: Review: verify portability and no-ETS evidence.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / release governance**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No new feature code or public stable-release claim.

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
