# Task-118: Audit release features, licenses and repository ownership

## Subject

Audit release features, licenses and repository ownership

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-118 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / architecture audit |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-014](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Confirm the built release still follows the final architecture decisions rather than earlier proposals.

## Detailed Requirements

- DR-001: Inspect resolved feature/dependency graphs and shipped binaries for v1 Secure activation, legacy/general-app support and prohibited runtimes/drivers.
- DR-002: Confirm KNX protocol implementations exist only in the fork and original-host compatibility code only in the allowed .NET area.
- DR-003: Preserve inherited Secure source if present but do not enable/test its cryptography for v1.
- DR-004: Distinguish ordinary IPC/TLS/hash security libraries from actual KNX Secure functionality.
- DR-005: Verify upstream notices and project-owned Apache-2.0 coverage.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: No prohibited feature or dependency is required by the shipped core/DCA path.
- [ ] AC-002: Boundary rules pass against the exact release revisions.
- [ ] AC-003: Build documentation describes the actual selected feature set.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-065](Task-065.md): Regression-test the non-secure protocol feature set. Required predecessor behavior/evidence must be accepted.
- [Task-076](Task-076.md): Map required ETS project/device contracts to Rust services. Required predecessor behavior/evidence must be accepted.
- [Task-078](Task-078.md): Mediate original DCA bus requests through Rust and knust. Required predecessor behavior/evidence must be accepted.
- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: cargo feature/dependency and code ownership checks.
- TEST-002: Runtime negative: secure-required/legacy/general-app operations reject explicitly.
- TEST-003: License audit: compare notices with packaged dependencies.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / architecture audit**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No relabeling upstream source, removal of useful generic security controls or testing of out-of-scope Secure behavior.

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
