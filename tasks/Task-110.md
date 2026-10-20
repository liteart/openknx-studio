# Task-110: Run an approved original-DCA firmware update and verify recovery policy

## Subject

Run an approved original-DCA firmware update and verify recovery policy

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-110 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / original firmware DCA integration |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Prove one actual firmware update through the original DCA without ETS and without moving protocol code into the host.

## Detailed Requirements

- DR-001: Use only the registered supported hardware/firmware pair and manufacturer-approved update/recovery procedure.
- DR-002: Require a distinct human firmware grant with exact artifact hashes/target; persist intent/results and use the mediated protocol path.
- DR-003: Reject target/version mismatch before side effects and record configuration retention/loss exactly.
- DR-004: Verify final firmware identity and representative device function after update.
- DR-005: Test interruption logic with a simulator or manufacturer-approved recoverable scenario; never cut power randomly for test coverage.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A real supported firmware update completes with exact version and function verification.
- [ ] AC-002: Failed/uncertain outcomes remain distinguishable and cannot auto-retry without approved recovery semantics.
- [ ] AC-003: No ETS runtime, unmediated driver or patched vendor binary is used.

## Dependencies to Other Tasks

- [Task-109](Task-109.md): Inspect a separate original firmware-update DCA and workflow. Required predecessor behavior/evidence must be accepted.
- [Task-078](Task-078.md): Mediate original DCA bus requests through Rust and knust. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.
- [Task-081](Task-081.md): Constrain DCA worker failures and access. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.
- [Task-072](Task-072.md): Recover interrupted deployment jobs without false rollback. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware/Windows authorized: one complete supported update.
- TEST-002: Negative: wrong device/hash/version and missing firmware grant.
- TEST-003: Failure simulation: timeout/worker loss/restart with durable unknown state and controlled recovery.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / original firmware DCA integration**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No universal firmware support, arbitrary downgrade, unsafe interruption test or native replacement updater.

## Risks and Blockers

Any missing generic KNX service requires its own fork task. Any unavoidable prohibited runtime/driver requirement blocks this support claim.

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
