# Task-127: Perform an independent cache-free restore and operation drill

## Subject

Perform an independent cache-free restore and operation drill

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-127 |
| Status | Not started |
| Stage | Release |
| Priority within stage | P1 |
| Component / Git project | knx-editor / release acceptance |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md) |

## Description

Validate portability with the actual final packages rather than the development tree.

## Detailed Requirements

- DR-001: On a fresh qualified machine restore a complete permitted project backup and rebuild all caches.
- DR-002: Verify configuration, Git history, DCA state, required originals and durable receipts; reauthorize secrets rather than copying them from logs.
- DR-003: Run safe MCP inspection/compile and the qualified Windows DCA reopen path without ETS.
- DR-004: Check that missing artifact/credential scenarios are actionable and do not destroy restored data.
- DR-005: No device programming is implied by restoring or opening a project.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The final packages reconstruct the project without the original SQLite/runtime process.
- [ ] AC-002: Restored state and hashes match the accepted backup.
- [ ] AC-003: Restore/open performs zero unsolicited hardware writes.

## Dependencies to Other Tasks

- [Task-126](Task-126.md): Create immutable release artifacts and provenance. Required predecessor behavior/evidence must be accepted.
- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.
- [Task-098](Task-098.md): Verify artifact portability across clones and remotes. Required predecessor behavior/evidence must be accepted.
- [Task-114](Task-114.md): Verify clean-machine ETS and service independence. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-RC = GO ([Task-125](Task-125.md)); Task-129 additionally requires G-RELEASE = GO (Task-128).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Clean-machine: full restore/rebuild/inspect/compile and Windows DCA reopen.
- TEST-002: Negative: missing original, corrupt state blob and unavailable credential.
- TEST-003: Safety: protocol mock/log verifies no restore-triggered download.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / release acceptance**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No live migration of an uncontrolled installation or claim that every Git clone is a full backup.

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
