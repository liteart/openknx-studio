# Task-136: Evaluate an explicitly secured remote Windows DCA worker

## Subject

Evaluate an explicitly secured remote Windows DCA worker

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-136 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / remote DCA bridge |
| Owner | Unassigned |
| Requirement references | [R-005](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-026](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Investigate allowing a Linux/macOS core to use a separate Windows interactive worker without weakening v1 safety.

## Detailed Requirements

- DR-001: Design explicit pairing, peer authentication, encryption, per-user/session permissions, reconnect semantics and artifact/state transfer.
- DR-002: Preserve original DCA interactivity requirements and scoped human device-operation authorization.
- DR-003: Keep KNX protocols in the knust fork and define where the generic transport runs without introducing two uncontrolled drivers.
- DR-004: Treat network loss during a device operation as potentially unknown, not an automatic retry.
- DR-005: Gate implementation on a reviewed threat model and narrow remote PoC.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A design/PoC proves authenticated session setup and benign state round-trip or records specific blockers.
- [ ] AC-002: No unauthenticated listener or silent Windows dependency appears in the ordinary core.
- [ ] AC-003: Remote failures preserve honest job state.

## Dependencies to Other Tasks

- [Task-129](Task-129.md): Publish the approved stable release and archive evidence. Required predecessor behavior/evidence must be accepted.
- [Task-075](Task-075.md): Supervise the Windows DCA worker with versioned local IPC. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.
- [Task-081](Task-081.md): Constrain DCA worker failures and access. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Network test: authenticated/unauthorized peer and interrupted state transfer.
- TEST-002: Failure simulation: dropped link during an in-flight operation.
- TEST-003: Review: interactive desktop, permission and protocol ownership constraints.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / remote DCA bridge**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No v1 requirement, exposed unauthenticated gRPC/HTTP port or claim that remote UI actions are automatically headless.

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
