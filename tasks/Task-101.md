# Task-101: Import parameter assignments and group-object bindings

## Subject

Import parameter assignments and group-object bindings

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-101 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / KNXPROJ configuration import |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md) |

## Description

Reconstruct supported engineering configuration without changing the installation's existing technical addresses.

## Detailed Requirements

- DR-001: Map explicit parameters, active object identities, flags, module scopes and existing group bindings using the selected product versions.
- DR-002: Create one semantic signal per canonical imported group binding with all memberships, preserving transmit association/order.
- DR-003: Retain source labels/display format and raw group-address values without renumbering.
- DR-004: Mark unknown DPTs/unsupported constructs unverified and block unsafe compilation while retaining source evidence.
- DR-005: Report exact losses or ambiguous mappings as blockers.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Fixture parameter and association inventories match the expected imported candidate.
- [ ] AC-002: No group address or sending association changes accidentally.
- [ ] AC-003: Unsupported critical content is visible and cannot be called successfully migrated for deployment.

## Dependencies to Other Tasks

- [Task-100](Task-100.md): Import project identity, topology and device instances. Required predecessor behavior/evidence must be accepted.
- [Task-047](Task-047.md): Implement typed parameter definitions and value validation. Required predecessor behavior/evidence must be accepted.
- [Task-048](Task-048.md): Resolve parameter and object references with correct scope. Required predecessor behavior/evidence must be accepted.
- [Task-050](Task-050.md): Resolve effective communication objects and associations. Required predecessor behavior/evidence must be accepted.
- [Task-055](Task-055.md): Implement shared semantic signals and endpoint memberships. Required predecessor behavior/evidence must be accepted.
- [Task-056](Task-056.md): Allocate and preserve stable KNX group-address bindings. Required predecessor behavior/evidence must be accepted.
- [Task-058](Task-058.md): Validate graph DPT and communication semantics. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: parameter/default distinctions, group memberships and send ordering.
- TEST-002: Negative: missing object ref, ambiguous module, unknown DPT and duplicate allocation.
- TEST-003: Comparison: pre/post canonical connectivity equivalence.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / KNXPROJ configuration import**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No new preferred address hierarchy, label-derived DPT guessing or silent simplification of imported connections.

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
