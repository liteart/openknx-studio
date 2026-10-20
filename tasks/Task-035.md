# Task-035: Load and validate a complete accepted project snapshot

## Subject

Load and validate a complete accepted project snapshot

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-035 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / project repository reader |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Construct an immutable coherent project view directly from authoritative files without trusting a cache.

## Detailed Requirements

- DR-001: Validate schemas, unique IDs, cross-file references, product locks, topology and required state blobs.
- DR-002: Report each invalid path/entity with structured errors and preserve original files.
- DR-003: Compute the snapshot identity from canonical content and mandatory hashes without self-reference.
- DR-004: Keep unsupported-but-retained source data explicitly separate from deployable configuration.
- DR-005: Never silently repair, migrate or commit a read.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A valid multi-file project loads identically with and without SQLite.
- [ ] AC-002: Missing/dangling/corrupt references prevent acceptance with precise diagnostics.
- [ ] AC-003: Reading a project produces no Git or filesystem mutation.

## Dependencies to Other Tasks

- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.
- [Task-033](Task-033.md): Implement deterministic multi-file serialization. Required predecessor behavior/evidence must be accepted.
- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: valid project load and cache-free load equality.
- TEST-002: Negative: duplicate IDs, dangling endpoint, missing blob and wrong major schema.
- TEST-003: Determinism: repeat load produces the same snapshot identity.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / project repository reader**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No cache optimization, project migration, implicit repair or device communication.

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
