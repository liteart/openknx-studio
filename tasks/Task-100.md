# Task-100: Import project identity, topology and device instances

## Subject

Import project identity, topology and device instances

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-100 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / KNXPROJ structural import |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md) |

## Description

Map the structural data of the supported exported project into independent editor entities.

## Detailed Requirements

- DR-001: Preserve source IDs/provenance, individual addresses, device/application references, topology and available building labels.
- DR-002: Generate stable local UUID mappings deterministically within an import session and persist the mapping in provenance.
- DR-003: Distinguish modeled unsupported nodes from supported programmable devices.
- DR-004: Reject ambiguous/missing product references rather than selecting similarly named products.
- DR-005: Produce an isolated candidate project, not edits to the existing accepted project.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Imported structural counts/identities match the expected fixture inventory.
- [ ] AC-002: Existing addresses and relationships are retained.
- [ ] AC-003: Unsupported nodes/references are reported instead of silently omitted.

## Dependencies to Other Tasks

- [Task-099](Task-099.md): Register KNXPROJ fixtures and safe import preparation. Required predecessor behavior/evidence must be accepted.
- [Task-046](Task-046.md): Resolve product hardware and application catalog relationships. Required predecessor behavior/evidence must be accepted.
- [Task-054](Task-054.md): Implement device and topology lifecycle commands. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: topology/device source-to-local mapping.
- TEST-002: Negative: duplicate source IDs, missing app and invalid address.
- TEST-003: Determinism: rerun with the same import mapping produces equivalent canonical structure.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / KNXPROJ structural import**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No parameter/group/DCA state import yet or claim that modeled couplers can be programmed.

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
