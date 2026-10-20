# Task-106: Qualify original MDT DALI scene editing and transfer

## Subject

Qualify original MDT DALI scene editing and transfer

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-106 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / original DCA DALI qualification |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Verify one complete original-DCA scene configuration/write/read-back behavior for the reference gateway.

## Detailed Requirements

- DR-001: Choose supported scene controls from the actual DCA and record the expected participating gear/settings.
- DR-002: Edit/persist state, approve transfer, send through the mediated path and verify the stored/observed scene behavior.
- DR-003: Preserve untouched scene entries and enforce actual documented capacity/value ranges.
- DR-004: Record whether the operation is UI-driven or available through a tested adapter.
- DR-005: Include before/after state and exact source revisions in evidence.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected scene behaves as configured and survives reopen/read-back.
- [ ] AC-002: Unchanged scene entries remain unchanged.
- [ ] AC-003: Invalid or denied transfer does not become a successful capability claim.

## Dependencies to Other Tasks

- [Task-105](Task-105.md): Qualify original MDT DALI address and group assignments. Required predecessor behavior/evidence must be accepted.
- [Task-077](Task-077.md): Persist real DCA state through editor changesets. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware/Windows: configure/transfer/recall the selected safe scene.
- TEST-002: Persistence: compare saved state and reopened scene.
- TEST-003: Negative: invalid value, denied grant and simulated transfer interruption.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / original DCA DALI qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No assumption that web-UI scene support equals DCA support or generic scene protocol implementation.

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
