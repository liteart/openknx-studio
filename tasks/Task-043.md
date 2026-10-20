# Task-043: Export and restore a complete offline project backup

## Subject

Export and restore a complete offline project backup

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-043 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / backup service |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Make file-based ownership real by restoring without the original process, database, or artifact cache.

## Detailed Requirements

- DR-001: Define configuration-only versus full backups with explicit included/excluded categories.
- DR-002: Full backup gathers accepted JSON/Git history, required versioned state, permitted original artifacts and operational receipts with hashes.
- DR-003: Exclude credentials/keys and report required reauthorization or prohibited artifact redistribution.
- DR-004: Restore into a new path, validate hashes/schemas/references, and rebuild the cache.
- DR-005: Never overwrite an active project or initiate device operations on restore.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A fresh cache-free environment can reopen a complete backup and inspect configuration/history/receipts.
- [ ] AC-002: Missing mandatory inputs are listed precisely and cannot be labeled a complete backup.
- [ ] AC-003: Original files and credentials remain protected.

## Dependencies to Other Tasks

- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.
- [Task-039](Task-039.md): Expose automatic local configuration history and restore. Required predecessor behavior/evidence must be accepted.
- [Task-040](Task-040.md): Build a disposable revision-tagged SQLite read projection. Required predecessor behavior/evidence must be accepted.
- [Task-042](Task-042.md): Persist job journals and deployment receipts as files. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: export/delete local cache/restore into empty directory/rebuild.
- TEST-002: Negative: corrupt archive, missing DCA state blob, absent product and existing destination.
- TEST-003: Safety: backup/restore dispatches no KNX operations.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / backup service**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automatic cloud backup, credential export or claim that a bare Git clone contains external artifacts.

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
