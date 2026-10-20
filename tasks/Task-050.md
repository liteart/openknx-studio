# Task-050: Resolve effective communication objects and associations

## Subject

Resolve effective communication objects and associations

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-050 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / configuration engine |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md) |

## Description

Expose the exact active object set the compiler and graph may use.

## Detailed Requirements

- DR-001: Resolve object refs, instance paths, object numbers, flags and DPT metadata from the selected configuration.
- DR-002: Distinguish active, inactive and unsupported objects and retain stable identity across label/address changes.
- DR-003: Preserve profile-permitted multiple associations and the designated sending association/order.
- DR-004: Reject ambiguous object numbering, conflicting activation and unsupported flag combinations.
- DR-005: Expose direction as capabilities rather than assuming every object is a simple input or output.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Object sets match the reviewed configuration fixtures after each mode change.
- [ ] AC-002: Graph consumers cannot attach to inactive/ambiguous endpoints.
- [ ] AC-003: Object identity stays stable when only display labels change.

## Dependencies to Other Tasks

- [Task-048](Task-048.md): Resolve parameter and object references with correct scope. Required predecessor behavior/evidence must be accepted.
- [Task-049](Task-049.md): Evaluate dynamic parameter conditions deterministically. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: reference overrides, flags and object-number bounds.
- TEST-002: Integration: parameter changes update the effective set.
- TEST-003: Negative: duplicate effective number, inactive endpoint and ambiguous transmit association.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / configuration engine**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No wire telegram encoding or automatic object renumbering.

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
