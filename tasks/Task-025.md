# Task-025: Compile one group and object association for the reference device

## Subject

Compile one group and object association for the reference device

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-025 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download compiler |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Generate the minimal product-defined group/address/object table changes needed to prove a functioning connection.

## Detailed Requirements

- DR-001: Select one active compatible communication object and one valid non-secure group binding.
- DR-002: Use the documented reference profile for table contents, object number/flags and transmit association ordering.
- DR-003: Use knust public address/DPT codecs where protocol encoding is required; do not duplicate wire codecs.
- DR-004: Preserve unrelated entries and reject capacity overflow or ambiguous object identity.
- DR-005: Emit deterministic offline tables with a human-readable association summary.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected object maps to the intended group with correct flags/order for the profile.
- [ ] AC-002: Capacity and identity errors block output.
- [ ] AC-003: The artifact contains no hidden auto-renumbering.

## Dependencies to Other Tasks

- [Task-023](Task-023.md): Parse only the selected KNXPROD application for PoC C. Required predecessor behavior/evidence must be accepted.
- [Task-024](Task-024.md): Encode a minimum parameter change into the reference image. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: zero/one entry, duplicate, capacity boundary and wrong object.
- TEST-002: Golden: expected table bytes from reviewed evidence.
- TEST-003: Determinism: same binding yields identical output.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download compiler**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No full semantic graph, group-address allocator, multicast router configuration, or live telegram test.

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
