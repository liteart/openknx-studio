# Task-108: Qualify original MDT readback, diagnostics and gear replacement

## Subject

Qualify original MDT readback, diagnostics and gear replacement

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-108 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / original DCA DALI qualification |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Verify the remaining critical reference workflows needed to maintain the DALI installation through the original DCA.

## Detailed Requirements

- DR-001: Inventory configuration readback, diagnostics, import/export and replacement features actually offered by the reference DCA.
- DR-002: Back up the test configuration before a deliberately selected supported gear replacement.
- DR-003: Use operation-specific approval and preserve identity/group/scene mappings only as the documented vendor workflow guarantees.
- DR-004: Reconcile readback with editor state as a reviewed changeset, not an unreviewed overwrite.
- DR-005: Record unknown/stale/partial device state and each workflow coverage result.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected replacement and configuration readback scenario completes with verified expected mappings.
- [ ] AC-002: Diagnostic/readback information does not silently overwrite desired configuration.
- [ ] AC-003: Missing critical functions are explicit release blockers rather than a generic supported badge.

## Dependencies to Other Tasks

- [Task-104](Task-104.md): Qualify original MDT DALI discovery and identification. Required predecessor behavior/evidence must be accepted.
- [Task-105](Task-105.md): Qualify original MDT DALI address and group assignments. Required predecessor behavior/evidence must be accepted.
- [Task-106](Task-106.md): Qualify original MDT DALI scene editing and transfer. Required predecessor behavior/evidence must be accepted.
- [Task-107](Task-107.md): Qualify original MDT HCL and schedule configuration. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware/Windows: one approved replace/readback/diagnostic scenario.
- TEST-002: Persistence: import/export or equivalent vendor state round-trip.
- TEST-003: Negative: wrong replacement target, mismatching state and interrupted readback.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / original DCA DALI qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No bulk factory reset, generic replacement algorithm or pretending all diagnostic reads are side-effect-free.

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
