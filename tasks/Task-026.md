# Task-026: Assemble the minimum complete reference download plan

## Subject

Assemble the minimum complete reference download plan

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-026 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download load plan |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Build an explicit ordered plan for the chosen product without conflating it with protocol transmission.

## Detailed Requirements

- DR-001: Include required preconditions, application segments, parameter/group tables, load-state transitions and postconditions for the one verified profile.
- DR-002: Express operations through the fork public services and record the pinned API revision.
- DR-003: Reject unknown mandatory operations and incompatible target descriptors.
- DR-004: Separate pure plan construction from execution and require an explicit dry-run review.
- DR-005: Define safe cancellation/recovery limits; do not promise arbitrary rollback.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The offline plan accounts for every required selected-profile operation.
- [ ] AC-002: Unknown steps or wrong target identity prevent execution eligibility.
- [ ] AC-003: A reviewer can see the exact intended writes before hardware is contacted.

## Dependencies to Other Tasks

- [Task-021](Task-021.md): Add only the selected profile memory and load-state primitives. Required predecessor behavior/evidence must be accepted.
- [Task-024](Task-024.md): Encode a minimum parameter change into the reference image. Required predecessor behavior/evidence must be accepted.
- [Task-025](Task-025.md): Compile one group and object association for the reference device. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: ordering, missing segment/transition and unsupported operation.
- TEST-002: Simulator: execute the plan against the selected peer and verify expected sequence.
- TEST-003: Negative: wrong mask/application causes zero writes.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download load plan**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No universal load interpreter, Git persistence, or production approval system.

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
