# Task-070: Enforce human-only exact-plan and DCA operation grants

## Subject

Enforce human-only exact-plan and DCA operation grants

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-070 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / authorization and operator CLI |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Implement authorization outside the agent-accessible MCP capability set for physical effects.

## Detailed Requirements

- DR-001: Compiled plan grants bind immutable plan hash, exact targets, permitted effect, issuer and expiry; consume once at execution start.
- DR-002: DCA grants bind package/session/device/operation class and bounded allowed services/ranges, with separate explicit firmware authorization.
- DR-003: Use a trusted human credential/channel unavailable to the MCP principal; a tool annotation, isatty check or agent-provided confirmation string is not proof of human approval.
- DR-004: Refuse stale/replayed/expired/overbroad grants and record audit metadata without secrets.
- DR-005: State the threat boundary: unrestricted code execution under the approving principal can bypass an application-only control and is not a claimed security boundary.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: MCP credentials cannot mint or elevate deployment grants.
- [ ] AC-002: Exact permitted operations execute once; altered targets/plans and replay attempts fail.
- [ ] AC-003: Expiry before start does not become an unsafe forced interruption of an already approved firmware operation.

## Dependencies to Other Tasks

- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.
- [Task-068](Task-068.md): Create reproducible immutable plans and compiler fingerprints. Required predecessor behavior/evidence must be accepted.
- [Task-069](Task-069.md): Implement target identity and capacity preflight. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: grant binding/expiry/consumption/replay.
- TEST-002: Security: MCP principal attempts operator CLI/broker authorization and is denied.
- TEST-003: Integration: changed plan/target and DCA operation escalation.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / authorization and operator CLI**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No AI self-approval, arbitrary whole-bus capability or assumption that a local prompt is a security boundary.

## Risks and Blockers

A real separation between operator approval capability and agent credentials must be chosen and tested. If the deployment environment cannot provide it, label the limitation and do not claim strong human-only enforcement.

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
