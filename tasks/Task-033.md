# Task-033: Implement deterministic multi-file serialization

## Subject

Implement deterministic multi-file serialization

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-033 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / persistence serialization |
| Owner | Unassigned |
| Requirement references | [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Write readable project documents that yield stable Git diffs and reproducible compiler inputs.

## Detailed Requirements

- DR-001: Apply EP-07 formatting and recursively sorted object keys.
- DR-002: Sort only schema-declared set-like collections; preserve semantically ordered associations and load sequences.
- DR-003: Use schema-defined exact numeric representations and reject non-finite/ambiguous values.
- DR-004: Generate paths from stable IDs and reject path/case collisions across supported filesystems.
- DR-005: Return changed-file hashes without touching disk when requested in dry-run mode.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Equivalent unordered inputs generate byte-identical files.
- [ ] AC-002: A one-parameter edit changes only its owning document and necessary derived records.
- [ ] AC-003: Ordered arrays and Unicode labels round-trip unchanged.

## Dependencies to Other Tasks

- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: exact bytes, newline, indentation and ordering.
- TEST-002: Property: serialize/parse/serialize idempotence.
- TEST-003: Negative: colliding paths, duplicate identities and invalid numeric values.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / persistence serialization**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No database export loop, Git commit, or automatic normalization of user edits.

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
