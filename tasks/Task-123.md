# Task-123: Write operator, recovery and contributor documentation

## Subject

Write operator, recovery and contributor documentation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-123 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / documentation |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Document the actual implemented commands, supported use cases and failure procedures in English.

## Detailed Requirements

- DR-001: Provide quickstart for local MCP/core, Windows DCA prerequisites, one-time project import and optional remote Git.
- DR-002: Explain approval/bench setup, unsupported Secure/legacy paths, DCA interaction requirements and per-function matrix.
- DR-003: Include configuration backup/restore, cache rebuild, interrupted download/update handling and safe stop conditions.
- DR-004: Verify examples against implemented CLI/MCP schemas; label vendor-specific prerequisites and limitations.
- DR-005: Include developer task/commit/localization workflows and contribution boundaries.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A new tester can follow the docs on a clean qualified environment.
- [ ] AC-002: No example implies an unimplemented tool or unsafe automatic device write.
- [ ] AC-003: English text and local links are consistent with the frozen contracts.

## Dependencies to Other Tasks

- [Task-111](Task-111.md): Publish per-function DCA qualification and unresolved gaps. Required predecessor behavior/evidence must be accepted.
- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.
- [Task-114](Task-114.md): Verify clean-machine ETS and service independence. Required predecessor behavior/evidence must be accepted.
- [Task-117](Task-117.md): Review threat model and enforce dangerous-operation boundaries. Required predecessor behavior/evidence must be accepted.
- [Task-119](Task-119.md): Audit English resources and localization invariants. Required predecessor behavior/evidence must be accepted.
- [Task-122](Task-122.md): Produce the dependency inventory and redistribution manifest. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Documentation tests: execute safe examples and validate payloads.
- TEST-002: Walkthrough: clean install/import/backup/recovery scenario.
- TEST-003: Link/resource lint and terminology review.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / documentation**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No marketing ETS-parity promise or unsupported platform/how-to instructions.

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
