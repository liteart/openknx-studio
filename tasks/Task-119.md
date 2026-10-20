# Task-119: Audit English resources and localization invariants

## Subject

Audit English resources and localization invariants

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-119 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / localization qualification |
| Owner | Unassigned |
| Requirement references | [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Ensure later translations can be added without changing schemas or compiler output.

## Detailed Requirements

- DR-001: Inventory all owned CLI/MCP/error/job/host messages and move human text into the resource framework.
- DR-002: Check en-US coverage, placeholder consistency, fallback and pseudo-localization; use .NET resources for host-owned text.
- DR-003: Verify no technical IDs/enum/schema/number formatting depends on locale.
- DR-004: Preserve user/manufacturer strings and do not claim original vendor UI has been translated.
- DR-005: Document the translator workflow and source-locale policy.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: No uncovered owned production message bypasses the localization mechanism without a documented technical-only exception.
- [ ] AC-002: Locale switches leave canonical JSON and compiled program fingerprints unchanged.
- [ ] AC-003: Missing translations fail safely to English.

## Dependencies to Other Tasks

- [Task-031](Task-031.md): Introduce English-first localization in the Rust core. Required predecessor behavior/evidence must be accepted.
- [Task-083](Task-083.md): Implement a bounded MCP stdio server using the Rust SDK. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.
- [Task-111](Task-111.md): Publish per-function DCA qualification and unresolved gaps. Required predecessor behavior/evidence must be accepted.
- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Catalog lint: missing/unused keys and placeholder compatibility.
- TEST-002: Snapshot: English/pseudo-locale CLI/MCP/host-owned messages.
- TEST-003: Determinism: same input under several locale settings yields identical execution artifacts.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / localization qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No mandatory full second-language translation or changes to vendor DLL resources.

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
