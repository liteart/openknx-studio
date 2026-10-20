# Task-082: Qualify original DCA lifecycle, state and mediated read integration

## Subject

Qualify original DCA lifecycle, state and mediated read integration

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-082 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / DCA integration tests |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Run an integrated reference DCA test against the production host and real project model before exposing it through MCP.

## Detailed Requirements

- DR-001: Use the exact original archive/application and clean Windows environment with no ETS.
- DR-002: Open the original UI, read permitted device information, change one offline configuration value, commit, close and reopen.
- DR-003: Verify state/parameter persistence and trace all bus access through the fork.
- DR-004: Record package/core/fork/host versions and actual test environment.
- DR-005: Keep full DALI functional coverage and firmware qualification separate.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The original DCA works for the stated lifecycle/state/read scenario with real evidence.
- [ ] AC-002: Reopening produces the accepted configuration without database-only state.
- [ ] AC-003: The test does not depend on installed ETS or a hidden vendor driver path.

## Dependencies to Other Tasks

- [Task-076](Task-076.md): Map required ETS project/device contracts to Rust services. Required predecessor behavior/evidence must be accepted.
- [Task-077](Task-077.md): Persist real DCA state through editor changesets. Required predecessor behavior/evidence must be accepted.
- [Task-078](Task-078.md): Mediate original DCA bus requests through Rust and knust. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.
- [Task-080](Task-080.md): Describe DCA capabilities and interaction requirements. Required predecessor behavior/evidence must be accepted.
- [Task-081](Task-081.md): Constrain DCA worker failures and access. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows/hardware read-only plus offline configuration: complete scenario.
- TEST-002: Negative: wrong DCA version, stale context and denied bus capability.
- TEST-003: Restart: remove SQLite and repeat reopen.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / DCA integration tests**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No claim of complete DALI commissioning, firmware updating or universal DCA compatibility.

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
