# Task-113: Build and smoke-test the declared platform packages

## Subject

Build and smoke-test the declared platform packages

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-113 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / CI and packaging |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Produce and execute the non-DCA backend packages on the three required OS families.

## Detailed Requirements

- DR-001: Pin supported OS/architecture runners and toolchains per EP-15; record exact versions.
- DR-002: Build Rust core/CLI for Windows x64, Linux x86_64 and macOS arm64, with extra targets unclaimed until tested.
- DR-003: Package the optional Windows host separately and do not require it on other OSes.
- DR-004: Run actual startup/project/MCP smoke tests on each target, not cross-compilation alone.
- DR-005: Retain lockfiles and build provenance.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Each required package runs the offline smoke scenario on its declared target.
- [ ] AC-002: Linux/macOS installation has no .NET/ETS prerequisite.
- [ ] AC-003: Unsupported architectures and DCA limitations are stated clearly.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-029](Task-029.md): Create the production Rust workspace with boundary checks. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: CI/native: locked build, startup, local project and stdio MCP smoke test on each target.
- TEST-002: Windows: optional host package discovery.
- TEST-003: Negative: absence of optional host does not break core.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / CI and packaging**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No claim of all possible distributions/architectures or Windows vendor UI on Linux/macOS.

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
