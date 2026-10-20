# Task-066: Implement production device-image compilation

## Subject

Implement production device-image compilation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-066 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / compiler |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Compile effective device configuration into the qualified product-defined image/table artifacts.

## Detailed Requirements

- DR-001: Consume only immutable resolved product/device/graph input and required original segments.
- DR-002: Enforce bit/byte bounds, signedness, overlap, reserved-data preservation, capacity and object/address table rules.
- DR-003: Keep product memory layout in the compiler and use fork codecs for protocol-defined scalars where needed.
- DR-004: Fail unknown critical encodings instead of emitting partial apparently valid images.
- DR-005: Emit deterministic artifacts plus field-to-byte provenance for diagnostics.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The PoC golden outputs are reproduced by the production compiler.
- [ ] AC-002: Invalid/capacity-overflow inputs produce no executable artifact.
- [ ] AC-003: Compile output contains no network side effect or hardware assumption.

## Dependencies to Other Tasks

- [Task-047](Task-047.md): Implement typed parameter definitions and value validation. Required predecessor behavior/evidence must be accepted.
- [Task-048](Task-048.md): Resolve parameter and object references with correct scope. Required predecessor behavior/evidence must be accepted.
- [Task-050](Task-050.md): Resolve effective communication objects and associations. Required predecessor behavior/evidence must be accepted.
- [Task-051](Task-051.md): Support qualified module and repeated-channel instances. Required predecessor behavior/evidence must be accepted.
- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-024](Task-024.md): Encode a minimum parameter change into the reference image. Required predecessor behavior/evidence must be accepted.
- [Task-025](Task-025.md): Compile one group and object association for the reference device. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden/property: parameter/object/table images including boundary conditions.
- TEST-002: Negative: overlap, missing segment, unsupported type and table overflow.
- TEST-003: Determinism: input order/locale variations yield equivalent payload bytes.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / compiler**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No packet construction, full load scheduling, firmware image generation or silent partial compilation.

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
