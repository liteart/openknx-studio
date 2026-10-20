# Task-109: Inspect a separate original firmware-update DCA and workflow

## Subject

Inspect a separate original firmware-update DCA and workflow

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-109 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / firmware DCA qualification input |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-014](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Establish the actual firmware-update case instead of assuming the supplied DALI DCA covers it.

## Detailed Requirements

- DR-001: Obtain the exact original MDT update DCA, supported switch/device, current/target firmware files and manufacturer recovery procedure.
- DR-002: Inventory SDK/runtime/bus/bootloader dependencies and required authorization separately from application download.
- DR-003: Verify whether the workflow requires KNX Secure or unavailable services; such a requirement blocks v1 rather than being bypassed.
- DR-004: Record artifact rights/hashes and any online activation/download requirements.
- DR-005: Define a safe bench procedure and expected version/configuration result before implementing an adapter.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A complete update fixture/procedure exists with verified identities and no invented firmware source.
- [ ] AC-002: Missing/prohibited/Secure-only prerequisites are explicitly classified.
- [ ] AC-003: The DALI package is not misrepresented as the update package.

## Dependencies to Other Tasks

- [Task-092](Task-092.md): Approve the alpha integration gate. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.
- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.
- [Task-074](Task-074.md): Register and verify unchanged DCA packages. Required predecessor behavior/evidence must be accepted.
- [Task-080](Task-080.md): Describe DCA capabilities and interaction requirements. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: archive/dependency/hash/manifest review.
- TEST-002: Manual: review manufacturer-supported target/recovery/power requirements.
- TEST-003: Negative: wrong hardware/firmware family and secure-required path.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / firmware DCA qualification input**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No firmware modification, signature bypass, update execution or generic bootloader guess.

## Risks and Blockers

The firmware-update DCA, target device and firmware are not supplied. This task remains blocked for actual qualification until they are provided.

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
