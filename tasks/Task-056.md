# Task-056: Allocate and preserve stable KNX group-address bindings

## Subject

Allocate and preserve stable KNX group-address bindings

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-056 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / graph allocation |
| Owner | Unassigned |
| Requirement references | [R-010](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md) |

## Description

Manage the technical group addresses behind semantic signals predictably.

## Detailed Requirements

- DR-001: Use an explicit allowed range and stable allocation ledger; retain imported raw address values/display style.
- DR-002: Allocate new signals deterministically from the same base state and input set.
- DR-003: Validate collisions, reserved/invalid values through the public typed address API, and exhaustion.
- DR-004: Never renumber existing addresses because a label/room or sort order changes.
- DR-005: Preserve required association/send order in downstream compilation.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Repeated allocation from the same accepted inputs yields the same bindings.
- [ ] AC-002: Imported/existing mappings remain unchanged when adding another signal.
- [ ] AC-003: Exhaustion/collision fails without partial allocations.

## Dependencies to Other Tasks

- [Task-055](Task-055.md): Implement shared semantic signals and endpoint memberships. Required predecessor behavior/evidence must be accepted.
- [Task-054](Task-054.md): Implement device and topology lifecycle commands. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: empty/full range, explicit address, duplicate and reserved-value cases.
- TEST-002: Regression: import-style ledger plus new signal.
- TEST-003: Property: allocation uniqueness and deterministic replay.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / graph allocation**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No conventional main/middle grouping requirement imposed on semantic functions or auto-renumbering.

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
