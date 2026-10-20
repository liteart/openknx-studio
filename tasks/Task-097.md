# Task-097: Merge project JSON semantically by stable identity

## Subject

Merge project JSON semantically by stable identity

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-097 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Git semantic merge |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md) |

## Description

Resolve independent project edits without relying exclusively on line-based JSON conflict handling.

## Detailed Requirements

- DR-001: Use a three-way merge by stable entity ID and schema field path.
- DR-002: Merge independent field changes and detect conflicting same-field edits, deletion/reference conflicts and allocation collisions.
- DR-003: Preserve ordered arrays and treat unequal opaque DCA state as an unresolved conflict, not text to merge.
- DR-004: Return a candidate changeset with explicit unresolved items and rerun complete semantic validation.
- DR-005: Never auto-select a side for execution-affecting ambiguity.
- DR-006: Expose git_merge_preview and git_merge_commit with structured field-level conflicts and explicit resolution changesets.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Independent device/parameter edits merge into a valid candidate.
- [ ] AC-002: Conflicting assignments and DCA blobs remain blocked until an explicit resolution.
- [ ] AC-003: The accepted project stays unchanged until the resolved candidate commits.

## Dependencies to Other Tasks

- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.
- [Task-035](Task-035.md): Load and validate a complete accepted project snapshot. Required predecessor behavior/evidence must be accepted.
- [Task-055](Task-055.md): Implement shared semantic signals and endpoint memberships. Required predecessor behavior/evidence must be accepted.
- [Task-056](Task-056.md): Allocate and preserve stable KNX group-address bindings. Required predecessor behavior/evidence must be accepted.
- [Task-095](Task-095.md): Fetch and adopt validated fast-forward changes. Required predecessor behavior/evidence must be accepted.
- [Task-096](Task-096.md): Support explicit branch switching and non-destructive restore. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: independent/same-field/delete-edit/membership/allocation cases.
- TEST-002: Property: merging identical branches is idempotent.
- TEST-003: Negative: reordered sending associations and opaque state conflict.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Git semantic merge**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No arbitrary automatic conflict policy, silent last-writer-wins or hardware side effects.

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
