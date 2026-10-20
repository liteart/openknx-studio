# Task-068: Create reproducible immutable plans and compiler fingerprints

## Subject

Create reproducible immutable plans and compiler fingerprints

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-068 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / compiler plan model |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Bind planned hardware effects to immutable inputs while distinguishing display-only changes from byte changes.

## Detailed Requirements

- DR-001: Record accepted project revision, exact product/artifact hashes, compiler/profile versions, options, targets and ordered operations.
- DR-002: Calculate a deterministic device-program fingerprint from execution-affecting artifacts and a separate approval plan hash including source revision/targets.
- DR-003: Exclude timestamps, random job IDs, locale and visual labels from execution payload fingerprints.
- DR-004: Store compiled artifacts by content hash and verify references on load.
- DR-005: Never compile from unaccepted external edits or stale cache rows.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Repeated compilation of the same inputs is byte-identical.
- [ ] AC-002: A label-only change can change source revision without claiming different device payload.
- [ ] AC-003: Tampered/missing plan artifacts cannot be executed.

## Dependencies to Other Tasks

- [Task-059](Task-059.md): Calculate deterministic configuration impact. Required predecessor behavior/evidence must be accepted.
- [Task-066](Task-066.md): Implement production device-image compilation. Required predecessor behavior/evidence must be accepted.
- [Task-067](Task-067.md): Interpret only qualified product load procedures. Required predecessor behavior/evidence must be accepted.
- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Golden: plan serialization/hash and program fingerprints.
- TEST-002: Regression: cosmetic versus parameter changes.
- TEST-003: Negative: stale cache, changed input artifact and missing compiled blob.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / compiler plan model**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No approval token generation inside the compiler or physical-state inference.

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
