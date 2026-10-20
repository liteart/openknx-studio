# Task-094: Push only an explicitly selected validated Git revision

## Subject

Push only an explicitly selected validated Git revision

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-094 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Git push |
| Owner | Unassigned |
| Requirement references | [R-017](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md) |

## Description

Publish user project history only when requested and when required configuration data is portable.

## Detailed Requirements

- DR-001: Push an exact local branch/revision with reviewed destination and non-force semantics.
- DR-002: Verify the accepted snapshot, required tracked configuration blobs and configured external artifact availability policy.
- DR-003: Do not include ignored private files or credentials and do not silently broaden remote refs.
- DR-004: Report rejected/non-fast-forward/offline outcomes without changing desired state.
- DR-005: Never initiate device operations after a successful push.
- DR-006: Expose git_push with exact project/ref/destination identity, expected revision and structured remote result.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: An explicit push reaches only the requested ref with matching content.
- [ ] AC-002: Missing required portable state or remote rejection is reported safely.
- [ ] AC-003: No force operation or hardware call occurs.

## Dependencies to Other Tasks

- [Task-093](Task-093.md): Configure optional Git remotes and protected credentials. Required predecessor behavior/evidence must be accepted.
- [Task-039](Task-039.md): Expose automatic local configuration history and restore. Required predecessor behavior/evidence must be accepted.
- [Task-043](Task-043.md): Export and restore a complete offline project backup. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: local bare remote success and upstream divergence.
- TEST-002: Negative: unavailable remote, missing configuration blob and unreviewed destination.
- TEST-003: Safety: assert no protocol adapter calls.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Git push**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automatic push after commit, force push or upload of prohibited vendor artifacts.

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
