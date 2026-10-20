# Task-126: Create immutable release artifacts and provenance

## Subject

Create immutable release artifacts and provenance

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-126 |
| Status | Not started |
| Stage | Release |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / release build |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Build final distributable archives from the accepted frozen source with verifiable provenance.

## Detailed Requirements

- DR-001: Record editor/fork/host commits, toolchain/dependency locks and platform build environment.
- DR-002: Package core and optional Windows host separately with schemas/docs/notices and reviewed prerequisites.
- DR-003: Generate content checksums and SBOM references; report actual signature status without claiming unsigned artifacts are signed.
- DR-004: Exclude all private fixtures, credentials, caches and prohibited vendor binaries.
- DR-005: Use the agreed release version; do not overwrite an existing released artifact.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Each package maps to exact source/provenance and passes content/integrity checks.
- [ ] AC-002: Checksums match final bytes and optional components are clearly separated.
- [ ] AC-003: Public packaging contains no unapproved content.

## Dependencies to Other Tasks

- [Task-125](Task-125.md): Approve the release-candidate stabilization gate. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-RC = GO ([Task-125](Task-125.md)); Task-129 additionally requires G-RELEASE = GO (Task-128).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build: clean locked release build per target.
- TEST-002: Package audit: extract and check required/forbidden files.
- TEST-003: Integrity: verify checksum/provenance matching and safe failure on tampered bytes.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / release build**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No public publishing, new dependencies or universal reproducible-bit-identical native binary promise.

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
