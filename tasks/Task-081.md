# Task-081: Constrain DCA worker failures and access

## Subject

Constrain DCA worker failures and access

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-081 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Windows worker containment |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Apply practical least-privilege controls and document their actual limits for third-party original code.

## Detailed Requirements

- DR-001: Restrict IPC peers, session scratch paths and access to canonical project/credential locations where the OS allows enforcement.
- DR-002: Use an appropriate Windows process/job lifecycle and bounded worker supervision without claiming it is a complete sandbox.
- DR-003: Verify a worker cannot bypass the required KNX mediation in the supported deployment policy; document necessary external OS network restrictions.
- DR-004: Test crash/hang behavior and defer forced termination during unsafe hardware stages to explicit recovery policy.
- DR-005: Preserve evidence rather than suppressing vendor exceptions.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Unauthorized IPC and project-write attempts are denied under the tested policy.
- [ ] AC-002: Core remains responsive after an offline worker crash/hang.
- [ ] AC-003: Security documentation states residual privileges and whether direct network access is actually blocked.

## Dependencies to Other Tasks

- [Task-075](Task-075.md): Supervise the Windows DCA worker with versioned local IPC. Required predecessor behavior/evidence must be accepted.
- [Task-078](Task-078.md): Mediate original DCA bus requests through Rust and knust. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows security: peer/path/access negative tests using a test worker.
- TEST-002: Failure: crash, hang, malformed reply and oversized state.
- TEST-003: Observation: attempted unmediated network path under the qualified host policy.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Windows worker containment**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No claim that process isolation protects against all malicious code or that same-user arbitrary execution cannot bypass application controls.

## Risks and Blockers

If required restrictions cannot be enforced without breaking the original DCA, record the incompatibility and keep affected support claims blocked.

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
