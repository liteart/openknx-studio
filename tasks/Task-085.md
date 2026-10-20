# Task-085: Expose project, catalog and changeset MCP tools

## Subject

Expose project, catalog and changeset MCP tools

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-085 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / MCP project tools |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Adapt the existing domain services to typed agent tools without bypassing validation.

## Detailed Requirements

- DR-001: Implement project_create/inspect/status, catalog_import/search, changeset_begin/validate/commit/discard and local history/diff/restore-preview.
- DR-002: Require stable IDs, allowed artifact references and expected revisions for writes.
- DR-003: Return structured changeset/commit IDs and no-op status, not only a prose summary.
- DR-004: Distinguish preview from accepted mutation and enforce permissions in the underlying services.
- DR-005: Never accept arbitrary shell/Git commands or execute imported product content.
- DR-006: Also adapt catalog_upgrade_preview, device_upgrade_apply, project_backup, project_restore_backup_preview/commit and cache_rebuild/validate through the existing validated services; these remain non-hardware operations.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: An agent can create/import/configure an offline project using structured tools.
- [ ] AC-002: A commit only happens after complete validation and expected-revision checking.
- [ ] AC-003: Tool calls cannot sweep unrelated files or trigger hardware.

## Dependencies to Other Tasks

- [Task-036](Task-036.md): Implement isolated changesets and optimistic revision checks. Required predecessor behavior/evidence must be accepted.
- [Task-037](Task-037.md): Commit multi-file changes with a recoverable Git transaction. Required predecessor behavior/evidence must be accepted.
- [Task-039](Task-039.md): Expose automatic local configuration history and restore. Required predecessor behavior/evidence must be accepted.
- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.
- [Task-044](Task-044.md): Harden KNXPROD archive and XML ingestion. Required predecessor behavior/evidence must be accepted.
- [Task-083](Task-083.md): Implement a bounded MCP stdio server using the Rust SDK. Required predecessor behavior/evidence must be accepted.
- [Task-084](Task-084.md): Provide revision-aware paginated project and catalog resources. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: MCP integration: create/import/begin/validate/commit/discard/history sequence.
- TEST-002: Negative: stale revision, unauthorized path and invalid changeset.
- TEST-003: Safety: protocol adapter receives zero calls for these tools.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / MCP project tools**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No remote Git commands yet, KNXPROJ migration tool yet, or direct SQL interface.

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
