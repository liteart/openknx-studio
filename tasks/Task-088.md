# Task-088: Expose original DCA session tools with honest interaction status

## Subject

Expose original DCA session tools with honest interaction status

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-088 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / MCP DCA tools |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Let an agent manage the original DCA integration without falsely promising headless control of its UI.

## Detailed Requirements

- DR-001: Implement dca_inspect/capabilities/session_open/session_status/session_close and only evidence-qualified operation adapters.
- DR-002: Return requires_interaction for UI-only workflows, with session identity and the action the human must perform.
- DR-003: Bind sessions to exact package/device/project revisions and the scoped approval system.
- DR-004: Non-Windows/missing worker/package yields a precise requirement, not silent fallback to native logic.
- DR-005: Do not reflect every discovered .NET method into an MCP tool.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference Windows DCA can be opened/tracked/closed from MCP.
- [ ] AC-002: UI-only functionality never returns completed until actual vendor/host evidence confirms it.
- [ ] AC-003: Unsupported platform and unauthorized action are explicit.

## Dependencies to Other Tasks

- [Task-080](Task-080.md): Describe DCA capabilities and interaction requirements. Required predecessor behavior/evidence must be accepted.
- [Task-082](Task-082.md): Qualify original DCA lifecycle, state and mediated read integration. Required predecessor behavior/evidence must be accepted.
- [Task-083](Task-083.md): Implement a bounded MCP stdio server using the Rust SDK. Required predecessor behavior/evidence must be accepted.
- [Task-087](Task-087.md): Expose compile, preflight and approved deployment MCP tools. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows MCP integration: open/status/interaction/close.
- TEST-002: Negative: Linux/macOS execution, missing artifact, wrong app and unapproved operation.
- TEST-003: Contract: no unconstrained reflection or arbitrary method invocation.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / MCP DCA tools**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No universal DCA automation, native replacement or remote Windows host.

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
