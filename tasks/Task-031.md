# Task-031: Introduce English-first localization in the Rust core

## Subject

Introduce English-first localization in the Rust core

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-031 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / localization |
| Owner | Unassigned |
| Requirement references | [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Install the localization framework before production user-facing messages spread through the codebase.

## Detailed Requirements

- DR-001: Use the approved Fluent baseline with an en-US fallback catalog and explicit BCP-47 locale selection.
- DR-002: Reference stable keys plus typed arguments from errors/status/tool descriptions; do not concatenate translated sentence fragments.
- DR-003: Keep machine identifiers, JSON fields, enum values and canonical numbers untouched by locale.
- DR-004: Preserve user/manufacturer text as data and keep vendor translations separate.
- DR-005: Add pseudo-localization and missing-key diagnostics; a second real language is not required.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every initial owned human message resolves through the catalog.
- [ ] AC-002: Unknown locale/key falls back predictably without a panic or changed API data.
- [ ] AC-003: A pseudo-locale changes display messages but not project/compiler bytes.

## Dependencies to Other Tasks

- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: fallback chain, argument substitution and plural forms where used.
- TEST-002: Snapshot: en-US default and pseudo-locale results.
- TEST-003: Negative: invalid locale, absent key and missing argument.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / localization**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No rewriting original DCA strings, mandatory German translation, or localization of API identifiers.

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
