# Task-047: Implement typed parameter definitions and value validation

## Subject

Implement typed parameter definitions and value validation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-047 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / configuration engine |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md) |

## Description

Validate supported parameter types and constraints before any binary compilation.

## Detailed Requirements

- DR-001: Implement the types actually required by the qualification corpus, including their ranges/enums/widths/scales and exact representation.
- DR-002: Preserve default versus explicit values, units, localized labels and source identity.
- DR-003: Validate lengths, signedness and unsupported encodings; never infer a type from a label.
- DR-004: Report errors with stable keys and precise parameter paths.
- DR-005: Keep parameter semantics distinct from their memory layout/KNX transport encoding.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Valid boundary values pass and invalid values are rejected with machine-readable reasons.
- [ ] AC-002: Equivalent display locales produce identical accepted parameter values.
- [ ] AC-003: Unsupported required types reduce product compatibility explicitly.

## Dependencies to Other Tasks

- [Task-045](Task-045.md): Introduce versioned product schema adapters and normalized IR. Required predecessor behavior/evidence must be accepted.
- [Task-046](Task-046.md): Resolve product hardware and application catalog relationships. Required predecessor behavior/evidence must be accepted.
- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/property: min/max/enum/string-length/decimal cases for each implemented type.
- TEST-002: Regression: PoC selected parameter behaves identically.
- TEST-003: Negative: unknown enum, overflow and localized numeric input ambiguity.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / configuration engine**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No silent clamping, inferred conversions or promise to support every KNX parameter type.

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
