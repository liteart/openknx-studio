# Task-087: Expose compile, preflight and approved deployment MCP tools

## Subject

Expose compile, preflight and approved deployment MCP tools

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-087 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / MCP deployment tools |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Expose safe engineering execution without allowing an AI tool to authorize itself.

## Detailed Requirements

- DR-001: Implement deployment_plan, deployment_preflight, deployment_execute and deployment_diff with immutable IDs/hashes.
- DR-002: Execute accepts only a matching externally granted capability; do not provide approval_grant through MCP.
- DR-003: Return a durable job ID promptly and separate start acknowledgement from completed/verified state.
- DR-004: Verify target/revision/artifact preconditions again before writing.
- DR-005: Expose raw/unknown failure states without converting them to optimistic prose.
- DR-006: Support an individual-address plan kind using the qualified Task-063 method with a distinct target-bound grant; firmware remains a separately qualified original-DCA operation, not a generic application plan.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: An unapproved/stale plan performs zero writes.
- [ ] AC-002: A correctly approved plan starts one job and returns its durable ID.
- [ ] AC-003: Repeated execution requests cannot cause an extra hardware run.

## Dependencies to Other Tasks

- [Task-068](Task-068.md): Create reproducible immutable plans and compiler fingerprints. Required predecessor behavior/evidence must be accepted.
- [Task-069](Task-069.md): Implement target identity and capacity preflight. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.
- [Task-071](Task-071.md): Execute durable sequential programming jobs. Required predecessor behavior/evidence must be accepted.
- [Task-072](Task-072.md): Recover interrupted deployment jobs without false rollback. Required predecessor behavior/evidence must be accepted.
- [Task-073](Task-073.md): Compare desired, compiled, deployed and observed states. Required predecessor behavior/evidence must be accepted.
- [Task-083](Task-083.md): Implement a bounded MCP stdio server using the Rust SDK. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: MCP/simulator: plan/preflight/denied execute/approved execute.
- TEST-002: Security: attempts to grant/escalate/replay approval.
- TEST-003: Fault: lost MCP response after accepted start is reconciled by operation/job ID.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / MCP deployment tools**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No arbitrary telegram/memory tool, implicit approval from an agent message or auto-execute after compile.

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
