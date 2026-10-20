# Task-102: Preserve imported product artifacts and DCA state

## Subject

Preserve imported product artifacts and DCA state

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-102 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / KNXPROJ artifact and extension import |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Retain all inputs required to keep the imported project independent from ETS.

## Detailed Requirements

- DR-001: Collect supported embedded product data/resources and DCA state into verified immutable artifact/state references.
- DR-002: Record external/missing required files explicitly; a KNXPROJ is not assumed to contain every DCA executable or resource.
- DR-003: Preserve opaque extension payloads byte-for-byte with source identity and mapping metadata.
- DR-004: Transform embedded IDs only where format/behavior is verified; otherwise block affected DCA functionality rather than corrupting state.
- DR-005: Validate the selected DCA can consume the preserved state after a clean import.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Required fixture artifacts/state survive a cache-free import/export round-trip.
- [ ] AC-002: Missing executable/resource or unverified opaque mapping is an explicit blocker.
- [ ] AC-003: A supported imported reference DCA state reopens without ETS.

## Dependencies to Other Tasks

- [Task-100](Task-100.md): Import project identity, topology and device instances. Required predecessor behavior/evidence must be accepted.
- [Task-101](Task-101.md): Import parameter assignments and group-object bindings. Required predecessor behavior/evidence must be accepted.
- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.
- [Task-074](Task-074.md): Register and verify unchanged DCA packages. Required predecessor behavior/evidence must be accepted.
- [Task-077](Task-077.md): Persist real DCA state through editor changesets. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: artifact hashes and opaque state preservation.
- TEST-002: Windows integration: open the supported imported DCA state.
- TEST-003: Negative: absent external DCA, corrupted blob and unresolved embedded source ID.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / KNXPROJ artifact and extension import**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No assumption that every KNXPROJ is self-contained or opaque proprietary data can be freely rewritten.

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
