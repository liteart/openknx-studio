# Task-003: Pin minimum PoC toolchains and verification commands

## Subject

Pin minimum PoC toolchains and verification commands

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-003 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor + knust-fork / build tooling |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Make the minimal experiments reproducibly buildable without scaffolding the complete product.

## Detailed Requirements

- DR-001: Pin the Rust toolchain and commit lockfiles for the experiment binaries.
- DR-002: Select and record the Windows .NET Framework/MSBuild environment after the SDK inventory; do not assume an old chat version is sufficient.
- DR-003: Provide documented commands for Rust format/lint/test and Windows host build/test.
- DR-004: Build non-DCA Rust experiments on all three target OS families where runners are available; record unavailable runners as NOT RUN.
- DR-005: Keep Secure features disabled in the selected build commands.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A new developer can reproduce the offline PoC builds from the documented commands.
- [ ] AC-002: The Rust build does not require .NET or ETS.
- [ ] AC-003: Tools and resolved dependency versions are recorded, not floating latest references.

## Dependencies to Other Tasks

- [Task-002](Task-002.md): Create two isolated repositories and preserve licenses. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build: compile and run a minimal Rust executable.
- TEST-002: Windows: compile a minimal host test with the pinned references.
- TEST-003: Negative: verify the documented command set does not activate Secure features.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / build tooling**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No packaging, installer, remote service, or full production CI.

## Risks and Blockers

Windows/other OS runners may not be present. Do not mark their tests passed from a Linux cross-build alone.

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
