# Task-122: Produce the dependency inventory and redistribution manifest

## Subject

Produce the dependency inventory and redistribution manifest

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-122 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / supply chain |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Make the OSS release auditable while keeping manufacturer executables and SDK rights separate.

## Detailed Requirements

- DR-001: Generate an SBOM/resolved dependency list with exact versions, source revisions, licenses and shipped components.
- DR-002: Include upstream notices and the actual treatment of fork modifications.
- DR-003: Explicitly list user-provided DCA/product/firmware/SDK prerequisites excluded from public artifacts.
- DR-004: Check archives for private fixtures, secrets, captures, caches and unlicensed binaries.
- DR-005: Document vulnerability/license review findings and accepted/blocked disposition.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Public release artifacts contain only reviewed redistributable material.
- [ ] AC-002: The inventory matches the binaries and lockfiles.
- [ ] AC-003: Original vendor components are not presented as Apache-2.0 code.

## Dependencies to Other Tasks

- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.
- [Task-118](Task-118.md): Audit release features, licenses and repository ownership. Required predecessor behavior/evidence must be accepted.
- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Automated: dependency/license/secret inventory scans.
- TEST-002: Manual: inspect archive contents and SDK provisioning guide.
- TEST-003: Negative: inject a forbidden fixture into a test package and verify rejection.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / supply chain**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No legal certification or authorization inferred from a dependency scanner alone.

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
