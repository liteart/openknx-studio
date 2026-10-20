# Task-104: Qualify original MDT DALI discovery and identification

## Subject

Qualify original MDT DALI discovery and identification

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-104 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / original DCA DALI qualification |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Prove the reference DCA can perform its real discovery/identification workflow through the independent host.

## Detailed Requirements

- DR-001: Inventory the actual original UI/workflow and classify any side effects of discovery.
- DR-002: Use the exact gateway/application/DCA versions and a known isolated DALI ballast inventory.
- DR-003: Obtain an operation grant for any addressing/commissioning effects; use only the mediated knust path.
- DR-004: Compare discovered/identified devices against the physical inventory and record cancellation/error behavior.
- DR-005: Keep DALI short addresses distinct from KNX individual/group addresses.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The original DCA finds/identifies the expected bench devices without ETS.
- [ ] AC-002: No unsupported/extra target is modified and all effects are recorded.
- [ ] AC-003: Capability status states which actions remain interactive.

## Dependencies to Other Tasks

- [Task-092](Task-092.md): Approve the alpha integration gate. Required predecessor behavior/evidence must be accepted.
- [Task-082](Task-082.md): Qualify original DCA lifecycle, state and mediated read integration. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware/Windows: discovery and identify the registered ballasts.
- TEST-002: Negative/simulator: no response, duplicate/ambiguous identity and denied grant.
- TEST-003: Audit: verify every bus path is mediated and reference versions match.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / original DCA DALI qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No native DALI reimplementation, vendor web-UI substitute or discovery on a live installation.

## Risks and Blockers

Requires the physical MDT gateway, matching product/application, and a controlled DALI test circuit with known ballast identities.

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
