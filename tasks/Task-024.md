# Task-024: Encode a minimum parameter change into the reference image

## Subject

Encode a minimum parameter change into the reference image

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-024 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download compiler |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Compile the selected small parameter set to its documented binary layout without implementing KNX transport.

## Detailed Requirements

- DR-001: Use explicit bit offsets, lengths, signedness, byte order and constraints from the selected product/profile evidence.
- DR-002: Start from the correct immutable application/segment data; preserve unrelated bytes and reserved bits.
- DR-003: Compile at least one changed value and the default/reference value.
- DR-004: Reject overlap/out-of-bounds/unsupported encodings.
- DR-005: Emit an offline image/diff and no network packets.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Expected and actual changed byte ranges match reviewed reference vectors.
- [ ] AC-002: Repeated compilation from the same input produces identical bytes.
- [ ] AC-003: Invalid parameters cannot produce a downloadable image.

## Dependencies to Other Tasks

- [Task-023](Task-023.md): Parse only the selected KNXPROD application for PoC C. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: min/max/invalid values, bit-boundary changes and reserved-bit preservation.
- TEST-002: Golden: reference default and changed image.
- TEST-003: Negative: missing layout evidence and out-of-range placement.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download compiler**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No general parameter engine or bus write. Parameter image encoding is editor-owned; KNX telegram encoding is not.

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
