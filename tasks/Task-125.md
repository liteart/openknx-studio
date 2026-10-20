# Task-125: Approve the release-candidate stabilization gate

## Subject

Approve the release-candidate stabilization gate

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-125 |
| Status | Not started |
| Stage | RC gate |
| Priority within stage | P1 |
| Component / Git project | knx-editor / release governance |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-002](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Decide whether the frozen implementation is ready for final release packaging and an independent recovery drill.

## Detailed Requirements

- DR-001: Require all declared RC qualification results and no unresolved release-blocking defects.
- DR-002: Confirm finite support matrix, hardware evidence, clean-machine independence, limits and legal/provisioning dispositions.
- DR-003: Review known noncritical limitations explicitly rather than silently dropping tests.
- DR-004: Record exact source revisions and require a named maintainer GO/NO-GO.
- DR-005: New source/dependency changes invalidate affected evidence and trigger requalification.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The RC gate record links every mandatory result and accepted limitation.
- [ ] AC-002: GO is impossible with untested mandatory hardware/platform or a boundary bypass.
- [ ] AC-003: Final package tasks use only the frozen approved revisions.

## Dependencies to Other Tasks

- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.
- [Task-114](Task-114.md): Verify clean-machine ETS and service independence. Required predecessor behavior/evidence must be accepted.
- [Task-115](Task-115.md): Fuzz and bound untrusted archive and configuration inputs. Required predecessor behavior/evidence must be accepted.
- [Task-116](Task-116.md): Execute the full file, Git and cache crash matrix. Required predecessor behavior/evidence must be accepted.
- [Task-117](Task-117.md): Review threat model and enforce dangerous-operation boundaries. Required predecessor behavior/evidence must be accepted.
- [Task-118](Task-118.md): Audit release features, licenses and repository ownership. Required predecessor behavior/evidence must be accepted.
- [Task-119](Task-119.md): Audit English resources and localization invariants. Required predecessor behavior/evidence must be accepted.
- [Task-120](Task-120.md): Benchmark the declared performance and resource budgets. Required predecessor behavior/evidence must be accepted.
- [Task-121](Task-121.md): Run the final hardware and application compatibility corpus. Required predecessor behavior/evidence must be accepted.
- [Task-122](Task-122.md): Produce the dependency inventory and redistribution manifest. Required predecessor behavior/evidence must be accepted.
- [Task-123](Task-123.md): Write operator, recovery and contributor documentation. Required predecessor behavior/evidence must be accepted.
- [Task-124](Task-124.md): Freeze schema, API and migration compatibility contracts. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO; this task decides G-RC.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Gate audit: validate task/evidence completeness and hashes.
- TEST-002: Negative: missing mandatory test, changed binary dependency and unresolved critical defect.
- TEST-003: Review: compare actual shipped feature set to scope.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / release governance**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No implementation fixes hidden inside the gate or stable publication yet.

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
