# Task-111: Publish per-function DCA qualification and unresolved gaps

## Subject

Publish per-function DCA qualification and unresolved gaps

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-111 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / compatibility matrix |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Convert the actual original-DCA test results into precise user-visible support claims.

## Detailed Requirements

- DR-001: List each reference package/hash, hardware/application/firmware/runtime/OS combination and required original function.
- DR-002: Mark functional availability and automation level separately, with evidence links and date.
- DR-003: Record missing critical original DALI/update functions as blockers requiring new small tasks or an explicit scope decision.
- DR-004: Preserve import-state and restart compatibility results, not only fresh-device tests.
- DR-005: Do not generalize one DCA/version to an entire manufacturer.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every claimed function has traceable real evidence and stated interaction requirements.
- [ ] AC-002: No unsupported or untested function is hidden behind a generic DCA supported status.
- [ ] AC-003: The published matrix matches what MCP capabilities return.

## Dependencies to Other Tasks

- [Task-103](Task-103.md): Preview and accept one-time KNXPROJ migration atomically. Required predecessor behavior/evidence must be accepted.
- [Task-104](Task-104.md): Qualify original MDT DALI discovery and identification. Required predecessor behavior/evidence must be accepted.
- [Task-105](Task-105.md): Qualify original MDT DALI address and group assignments. Required predecessor behavior/evidence must be accepted.
- [Task-106](Task-106.md): Qualify original MDT DALI scene editing and transfer. Required predecessor behavior/evidence must be accepted.
- [Task-107](Task-107.md): Qualify original MDT HCL and schedule configuration. Required predecessor behavior/evidence must be accepted.
- [Task-108](Task-108.md): Qualify original MDT readback, diagnostics and gear replacement. Required predecessor behavior/evidence must be accepted.
- [Task-110](Task-110.md): Run an approved original-DCA firmware update and verify recovery policy. Required predecessor behavior/evidence must be accepted.
- [Task-080](Task-080.md): Describe DCA capabilities and interaction requirements. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Review: audit each capability against its task evidence.
- TEST-002: Contract: matrix-to-MCP capability consistency.
- TEST-003: Negative: absent proof or changed package hash invalidates the corresponding claim.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / compatibility matrix**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No broader manufacturer certification or automatic support inheritance across versions.

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
