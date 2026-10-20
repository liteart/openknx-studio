# Task-057: Implement parameter and graph edit domain commands

## Subject

Implement parameter and graph edit domain commands

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-057 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / application commands |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Expose a consistent non-UI command surface for device configuration and connections.

## Detailed Requirements

- DR-001: Commands take exact stable IDs, expected revision/changeset and typed parameter values or endpoint memberships.
- DR-002: Re-evaluate the affected configuration after parameter edits and surface invalidated connections.
- DR-003: Apply related changes atomically within one changeset; no partial accepted state.
- DR-004: Reject ambiguous names instead of resolving them to the first matching device.
- DR-005: Keep commands independent from MCP transport and hardware execution.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The same operation via a unit test, CLI adapter or MCP adapter has identical domain results.
- [ ] AC-002: A mode change that invalidates a connection requires explicit resolution before commit.
- [ ] AC-003: Invalid edits produce no file or device mutation.

## Dependencies to Other Tasks

- [Task-036](Task-036.md): Implement isolated changesets and optimistic revision checks. Required predecessor behavior/evidence must be accepted.
- [Task-047](Task-047.md): Implement typed parameter definitions and value validation. Required predecessor behavior/evidence must be accepted.
- [Task-049](Task-049.md): Evaluate dynamic parameter conditions deterministically. Required predecessor behavior/evidence must be accepted.
- [Task-055](Task-055.md): Implement shared semantic signals and endpoint memberships. Required predecessor behavior/evidence must be accepted.
- [Task-056](Task-056.md): Allocate and preserve stable KNX group-address bindings. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: parameter edit plus graph update in one changeset.
- TEST-002: Integration: stale revision and ambiguous label handling.
- TEST-003: Negative: inactive endpoint and partial-operation validation failure.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / application commands**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No natural-language interpretation inside the core or direct SQL updates.

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
