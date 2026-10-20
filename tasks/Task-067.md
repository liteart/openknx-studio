# Task-067: Interpret only qualified product load procedures

## Subject

Interpret only qualified product load procedures

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-067 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / compiler load profiles |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Generate ordered typed operations for the profiles actually supported by the reference corpus.

## Detailed Requirements

- DR-001: Select procedures from verified application/mask/profile metadata, not manufacturer name or UI label.
- DR-002: Implement a finite allowlist of load steps/conditions required by qualified products.
- DR-003: Validate required state transitions, parameter/table/application stages and pre/postconditions.
- DR-004: Reject arbitrary scripts, unknown instructions or unsupported profile/version combinations.
- DR-005: Annotate safe cancellation boundaries and distinguish configuration download from firmware update.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A supported reference procedure produces the reviewed complete operation sequence.
- [ ] AC-002: Unknown steps and wrong mask/profile combinations block plan creation.
- [ ] AC-003: The plan uses knust services without embedding its wire format.

## Dependencies to Other Tasks

- [Task-026](Task-026.md): Assemble the minimum complete reference download plan. Required predecessor behavior/evidence must be accepted.
- [Task-053](Task-053.md): Publish per-product compatibility diagnostics. Required predecessor behavior/evidence must be accepted.
- [Task-066](Task-066.md): Implement production device-image compilation. Required predecessor behavior/evidence must be accepted.
- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/golden: selected profile sequences and condition branches.
- TEST-002: Simulator integration: expected calls/order and load-state errors.
- TEST-003: Negative: unknown instruction, missing transition and firmware payload supplied as an application.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / compiler load profiles**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No universal ETS download engine or execution of manufacturer script code.

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
