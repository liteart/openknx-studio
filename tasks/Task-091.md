# Task-091: Run the first complete headless MCP engineering workflow

## Subject

Run the first complete headless MCP engineering workflow

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-091 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / end-to-end tests |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Demonstrate an integrated workflow from a clean project to verified device programming and original DCA state access.

## Detailed Requirements

- DR-001: Use a scripted non-LLM MCP client to import the reference product, create/configure a device, bind a signal, validate, commit and compile.
- DR-002: Require a real operator grant before the isolated hardware download and verify the functional result.
- DR-003: Open the original Windows DCA through the session API and verify the qualified state path separately.
- DR-004: Delete SQLite and reproduce inspection/history/status from durable files.
- DR-005: Record all source/artifact revisions and identify manual UI actions.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The integrated non-DCA workflow completes without ETS, GUI editor, account or LLM.
- [ ] AC-002: Actual device evidence confirms the programmed effect.
- [ ] AC-003: DCA interaction and cache reconstruction work with accurately reported capability limits.

## Dependencies to Other Tasks

- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.
- [Task-065](Task-065.md): Regression-test the non-secure protocol feature set. Required predecessor behavior/evidence must be accepted.
- [Task-073](Task-073.md): Compare desired, compiled, deployed and observed states. Required predecessor behavior/evidence must be accepted.
- [Task-082](Task-082.md): Qualify original DCA lifecycle, state and mediated read integration. Required predecessor behavior/evidence must be accepted.
- [Task-085](Task-085.md): Expose project, catalog and changeset MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-086](Task-086.md): Expose device and semantic-connection MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-087](Task-087.md): Expose compile, preflight and approved deployment MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.
- [Task-089](Task-089.md): Expose durable job status and bounded bus diagnostics. Required predecessor behavior/evidence must be accepted.
- [Task-090](Task-090.md): Test MCP misuse, idempotency and authority boundaries. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Scripted MCP/simulator: full workflow on every core target.
- TEST-002: Hardware authorized: reference full download and verification.
- TEST-003: Windows: original DCA session/state path; cache-free restart.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / end-to-end tests**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No production installation claim or complete DALI/firmware function qualification.

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
