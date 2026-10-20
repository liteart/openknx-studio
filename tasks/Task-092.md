# Task-092: Approve the alpha integration gate

## Subject

Approve the alpha integration gate

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-092 |
| Status | Not started |
| Stage | Alpha gate |
| Priority within stage | P1 |
| Component / Git project | knx-editor / release governance |
| Owner | Unassigned |
| Requirement references | [R-005](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Decide whether the first integrated engineering backend is ready for explicitly limited alpha testing.

## Detailed Requirements

- DR-001: Require accepted relevant foundation/configuration/protocol/compiler/DCA/MCP tasks and PoC GO.
- DR-002: Review real reference download and clean Windows original-DCA evidence.
- DR-003: Publish a finite alpha capability matrix and known blocked/unverified functions.
- DR-004: Keep the release labeled for bench/testing use; no live-installation recommendation.
- DR-005: Require maintainer review and record GO/NO-GO; a report alone does not grant release permission.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every mandatory alpha criterion has a task/evidence link.
- [ ] AC-002: The support matrix distinguishes core OS support, Windows DCA support and interaction needs.
- [ ] AC-003: NO-GO leaves beta qualification blocked.

## Dependencies to Other Tasks

- [Task-091](Task-091.md): Run the first complete headless MCP engineering workflow. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO; this task decides G-ALPHA.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Gate audit: validate predecessor results and test artifacts.
- TEST-002: Negative: remove a hardware/no-ETS proof and confirm the gate fails.
- TEST-003: Review: check license, ownership and no-Secure statements against actual build.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / release governance**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No new feature code, public publishing or widening hardware support during the gate.

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
