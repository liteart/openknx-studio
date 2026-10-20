# Task-089: Expose durable job status and bounded bus diagnostics

## Subject

Expose durable job status and bounded bus diagnostics

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-089 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / MCP jobs and diagnostics |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Provide observable long operations without forcing a client to keep one RPC request open.

## Detailed Requirements

- DR-001: Implement job_status/history/cancel-request plus bounded monitor start/read/stop tools or equivalent resources.
- DR-002: Reconstruct jobs from durable files after restart and keep queued/running/interaction/interrupted/unknown distinct.
- DR-003: Cancellation is a request honored at safe boundaries, not guaranteed device rollback.
- DR-004: Paginate history/events and report monitor loss rather than buffering indefinitely.
- DR-005: Disconnecting an MCP client must not silently cancel or duplicate an approved physical operation.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A restarted client can retrieve the real existing job state by ID.
- [ ] AC-002: A slow monitor consumer sees bounded output and loss metadata.
- [ ] AC-003: Cancellation cannot force an unsafe immediate firmware interruption.

## Dependencies to Other Tasks

- [Task-042](Task-042.md): Persist job journals and deployment receipts as files. Required predecessor behavior/evidence must be accepted.
- [Task-064](Task-064.md): Expose bounded group monitoring and verified DPT helpers. Required predecessor behavior/evidence must be accepted.
- [Task-071](Task-071.md): Execute durable sequential programming jobs. Required predecessor behavior/evidence must be accepted.
- [Task-072](Task-072.md): Recover interrupted deployment jobs without false rollback. Required predecessor behavior/evidence must be accepted.
- [Task-073](Task-073.md): Compare desired, compiled, deployed and observed states. Required predecessor behavior/evidence must be accepted.
- [Task-083](Task-083.md): Implement a bounded MCP stdio server using the Rust SDK. Required predecessor behavior/evidence must be accepted.
- [Task-087](Task-087.md): Expose compile, preflight and approved deployment MCP tools. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: disconnect/reconnect and core restart with durable jobs.
- TEST-002: Simulator: cancellation at safe/unsafe boundary and timeout uncertainty.
- TEST-003: Load: slow consumer and large history pagination.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / MCP jobs and diagnostics**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No dependence on experimental MCP task extensions or unbounded bus trace storage.

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
