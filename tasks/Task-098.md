# Task-098: Verify artifact portability across clones and remotes

## Subject

Verify artifact portability across clones and remotes

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-098 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Git artifact transport policy |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md) |

## Description

Ensure the project Git workflow does not create apparently valid snapshots whose essential state is unavailable.

## Detailed Requirements

- DR-001: Verify tracked DCA configuration blobs and lockfile references on clone/checkout/merge.
- DR-002: Define permitted original-artifact transfer versus user-provisioned missing artifacts, with explicit export/license status.
- DR-003: If LFS is enabled, verify actual payload availability and offline behavior rather than trusting pointer files; otherwise reject LFS-only missing content clearly.
- DR-004: Provide a complete portability report before push/full backup and after clone.
- DR-005: No automatic credential-bearing downloads or vendor redistribution.
- DR-006: Expose an artifact portability report through MCP and include it in remote/backup preflight results.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A complete qualified project can be cloned/restored and opened without SQLite.
- [ ] AC-002: Missing LFS/original/state payloads produce exact actionable diagnostics.
- [ ] AC-003: No snapshot is called fully portable based solely on Git object presence.

## Dependencies to Other Tasks

- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.
- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.
- [Task-094](Task-094.md): Push only an explicitly selected validated Git revision. Required predecessor behavior/evidence must be accepted.
- [Task-095](Task-095.md): Fetch and adopt validated fast-forward changes. Required predecessor behavior/evidence must be accepted.
- [Task-097](Task-097.md): Merge project JSON semantically by stable identity. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: clone with ordinary tracked blobs and permitted artifact bundle.
- TEST-002: Negative: LFS pointer-only file, missing original and hash mismatch.
- TEST-003: Offline: reopen fully provisioned clone without network.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Git artifact transport policy**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No mandatory LFS server, proprietary artifact hosting or silent external fetch.

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
