# Task-114: Verify clean-machine ETS and service independence

## Subject

Verify clean-machine ETS and service independence

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-114 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / installation qualification |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Prove that the release candidate does not accidentally depend on a developer machine with ETS installed.

## Detailed Requirements

- DR-001: Use clean declared OS environments and record installed components/processes/service dependencies.
- DR-002: Provision only permitted project/DCA prerequisites through the approved route.
- DR-003: Run core import/edit/compile and Windows original-DCA qualification flows with ETS absent.
- DR-004: Test offline operation with remotes and internet unavailable after permitted inputs are provisioned.
- DR-005: Report vendor-required activation/runtime blockers rather than hiding them in setup instructions.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Qualified core/DCA workflows complete without installed/running ETS or a copied ETS project store.
- [ ] AC-002: Normal offline operations do not demand cloud/account/model access.
- [ ] AC-003: The installation guide lists every real prerequisite accurately.

## Dependencies to Other Tasks

- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.
- [Task-082](Task-082.md): Qualify original DCA lifecycle, state and mediated read integration. Required predecessor behavior/evidence must be accepted.
- [Task-103](Task-103.md): Preview and accept one-time KNXPROJ migration atomically. Required predecessor behavior/evidence must be accepted.
- [Task-110](Task-110.md): Run an approved original-DCA firmware update and verify recovery policy. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Clean environment: core on all OSes and original DCA on Windows.
- TEST-002: Offline: disable external connectivity while retaining the approved lab path.
- TEST-003: Negative: remove an explicitly required permitted dependency and check precise diagnostics.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / installation qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No assertion that an SDK reference package itself proves standalone runtime legality.

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
