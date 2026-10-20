# Task-055: Implement shared semantic signals and endpoint memberships

## Subject

Implement shared semantic signals and endpoint memberships

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-055 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / domain graph |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-026](../docs/REQUIREMENTS.md) |

## Description

Represent visual connections as shared semantic channels rather than duplicated pairwise wires.

## Detailed Requirements

- DR-001: Store canonical memberships on signal entities using stable endpoint IDs and explicit roles.
- DR-002: Support one-to-many and many-to-many where object capabilities permit them.
- DR-003: Separate function grouping from signal transport and keep command/feedback signals distinct unless explicitly modeled otherwise.
- DR-004: Do not infer hardware logic, conversion, forwarding or a runtime automation engine from an edge.
- DR-005: Provide stable query ordering without storing contradictory membership lists in multiple entities.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Several compatible devices can share one signal and one allocation.
- [ ] AC-002: Removing a membership does not delete other participants.
- [ ] AC-003: Label changes do not change endpoint/signal identity or compiled connectivity.

## Dependencies to Other Tasks

- [Task-050](Task-050.md): Resolve effective communication objects and associations. Required predecessor behavior/evidence must be accepted.
- [Task-054](Task-054.md): Implement device and topology lifecycle commands. Required predecessor behavior/evidence must be accepted.
- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: one-to-many/many-to-many/remove/no-op graphs.
- TEST-002: Property: round-trip membership uniqueness and reference integrity.
- TEST-003: Negative: missing/inactive endpoint and contradictory duplicate membership.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / domain graph**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No GUI, point-to-point wire duplication, implicit converters or automation rules.

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
