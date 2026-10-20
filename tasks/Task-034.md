# Task-034: Implement immutable artifacts and versioned opaque state blobs

## Subject

Implement immutable artifacts and versioned opaque state blobs

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-034 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / artifact storage |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Store original product/DCA inputs and non-reconstructible DCA state without confusing them with disposable cache data.

## Detailed Requirements

- DR-001: Address content with lowercase SHA-256 plus byte size and provenance metadata; verify bytes before accepting references.
- DR-002: Separate external user-provided executable/catalog inputs from Git-tracked configuration state blobs.
- DR-003: Use bounded temporary writes and hash-verified publication; never overwrite different content under an existing hash.
- DR-004: Reject missing/corrupt required content and report permission-sensitive export exclusions.
- DR-005: Do not download or execute artifacts automatically.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Identical content is deduplicated and hash mismatches fail.
- [ ] AC-002: Deleting SQLite cannot remove any required original or DCA state.
- [ ] AC-003: Opaque DCA state is retained/versionable even when the host cannot interpret it.

## Dependencies to Other Tasks

- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.
- [Task-033](Task-033.md): Implement deterministic multi-file serialization. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: hash/size and duplicate publication.
- TEST-002: Integration: interrupted write, corruption and missing artifact.
- TEST-003: Negative: untrusted path traversal and an attempted automatic network fetch.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / artifact storage**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No public artifact hosting, vendor redistribution, or unsafe normalization of opaque DCA data.

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
