# Task-059: Calculate deterministic configuration impact

## Subject

Calculate deterministic configuration impact

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-059 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / impact analysis |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Explain which devices and compiled artifacts may change before a changeset is committed or programmed.

## Detailed Requirements

- DR-001: Compare accepted/candidate parameters, effective objects, allocations and product locks by stable ID.
- DR-002: Distinguish display-only edits from execution-affecting changes and identify dependency-driven effects.
- DR-003: Return affected devices, reasons, required review and any unsupported partial-download limitations.
- DR-004: Keep calculation pure and deterministic with no live device assumptions.
- DR-005: Do not equate an impacted device with proof that a write is required; later compiler/receipt comparison decides that.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A label-only edit has no hardware-impact reason.
- [ ] AC-002: A changed active object/group binding identifies all affected devices without unrelated ones.
- [ ] AC-003: The same snapshots always produce the same impact report.

## Dependencies to Other Tasks

- [Task-052](Task-052.md): Pin product versions and preview explicit application upgrades. Required predecessor behavior/evidence must be accepted.
- [Task-054](Task-054.md): Implement device and topology lifecycle commands. Required predecessor behavior/evidence must be accepted.
- [Task-057](Task-057.md): Implement parameter and graph edit domain commands. Required predecessor behavior/evidence must be accepted.
- [Task-058](Task-058.md): Validate graph DPT and communication semantics. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: rename, parameter, mode, allocation and product-version changes.
- TEST-002: Golden: impact paths/reasons for the reference scenario.
- TEST-003: Negative: unknown compile capability remains explicit rather than a false no-change result.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / impact analysis**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No device read/write, automatic deployment or assumed equality of physical state.

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
