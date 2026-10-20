# Task-071: Execute durable sequential programming jobs

## Subject

Execute durable sequential programming jobs

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-071 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / deployment executor |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Execute only approved immutable plans with durable evidence and conservative bus/session ownership.

## Detailed Requirements

- DR-001: Take a gateway/bus-path commissioning lease and per-device lock before the first effect.
- DR-002: Persist intent before each side-effecting step and result/verification afterwards; use the fork public API exclusively.
- DR-003: Respect profile ordering, bounded timeouts and safe cancellation points.
- DR-004: Do not blindly replay uncertain application-level writes when transport acknowledgements are lost.
- DR-005: End succeeded only when required postconditions and receipts are durable; otherwise failed/interrupted/unknown as evidenced.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference plan completes with exact step/results and required verification.
- [ ] AC-002: Competing jobs cannot program the same gateway path concurrently under the v1 policy.
- [ ] AC-003: Lost acknowledgement/persistence failure cannot appear as successful completion.

## Dependencies to Other Tasks

- [Task-042](Task-042.md): Persist job journals and deployment receipts as files. Required predecessor behavior/evidence must be accepted.
- [Task-065](Task-065.md): Regression-test the non-secure protocol feature set. Required predecessor behavior/evidence must be accepted.
- [Task-067](Task-067.md): Interpret only qualified product load procedures. Required predecessor behavior/evidence must be accepted.
- [Task-069](Task-069.md): Implement target identity and capacity preflight. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Simulator: normal job, denial, timeout, disconnect, cancellation and lock contention.
- TEST-002: Persistence fault: before/after each effect record.
- TEST-003: Hardware authorized: one qualified full programming job.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / deployment executor**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No automatic parallel commissioning, factory reset, blind resume or embedded KNX protocol retry logic.

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
