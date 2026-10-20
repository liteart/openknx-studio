# Task-132: Implement previewed visual connection editing

## Subject

Implement previewed visual connection editing

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-132 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / visual client |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md) |

## Description

Make visual editing another controlled route into the existing changeset workflow.

## Detailed Requirements

- DR-001: Dragging a compatible endpoint proposes membership in a shared signal or an explicitly created signal.
- DR-002: Show DPT/direction conflicts, allocation and device impact before commit.
- DR-003: Support removal/cancel/undo as domain changesets, not direct JSON or SQL mutation.
- DR-004: Reject stale revisions and ambiguous source/target roles.
- DR-005: Editing/committing the graph must never automatically execute a device plan.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Visual edits and equivalent MCP edits produce the same accepted model/fingerprint.
- [ ] AC-002: Invalid or cancelled gestures cause no persistent mutation.
- [ ] AC-003: Existing group addresses remain stable.

## Dependencies to Other Tasks

- [Task-131](Task-131.md): Implement a read-only device and signal graph view. Required predecessor behavior/evidence must be accepted.
- [Task-057](Task-057.md): Implement parameter and graph edit domain commands. Required predecessor behavior/evidence must be accepted.
- [Task-058](Task-058.md): Validate graph DPT and communication semantics. Required predecessor behavior/evidence must be accepted.
- [Task-086](Task-086.md): Expose device and semantic-connection MCP tools. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: UI integration: connect/disconnect/cancel/shared signal scenarios.
- TEST-002: Equivalence: visual versus MCP changeset outputs.
- TEST-003: Negative: DPT mismatch, inactive port and concurrent revision change.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / visual client**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No visual-only business rules or implicit runtime logic/conversions.

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
