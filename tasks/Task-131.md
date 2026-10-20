# Task-131: Implement a read-only device and signal graph view

## Subject

Implement a read-only device and signal graph view

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-131 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / visual client |
| Owner | Unassigned |
| Requirement references | [R-005](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md) |

## Description

Render the accepted semantic graph as an optional client without adding any editing or commissioning behavior.

## Detailed Requirements

- DR-001: Display stable device/endpoint/signal identities and shared memberships from revision-tagged domain views.
- DR-002: Show effective/inactive/unsupported state and preserve command/feedback separation.
- DR-003: Bound loading with paging/filtering and indicate stale revisions.
- DR-004: Use English resource keys and the selected cross-platform client framework.
- DR-005: Treat visual grouping as presentation, not a new electrical or KNX relationship.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Reference one-to-many and many-to-many graphs render without duplicate semantic signals.
- [ ] AC-002: Selecting a node identifies the correct stable backend entity.
- [ ] AC-003: The read-only client performs no writes or device operations.

## Dependencies to Other Tasks

- [Task-130](Task-130.md): Define the later visual client over the existing domain API. Required predecessor behavior/evidence must be accepted.
- [Task-084](Task-084.md): Provide revision-aware paginated project and catalog resources. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: UI: graph fixtures, selection and keyboard navigation.
- TEST-002: Contract: compare displayed memberships with backend state.
- TEST-003: Negative: stale/unknown endpoint and unavailable backend.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / visual client**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No drag-to-edit, automatic address allocation or device programming buttons.

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
