# Task-069: Implement target identity and capacity preflight

## Subject

Implement target identity and capacity preflight

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-069 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / deployment preflight |
| Owner | Unassigned |
| Requirement references | [R-010](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Check whether an immutable plan is valid for the exact connected target before any write.

## Detailed Requirements

- DR-001: Compare available device identity/application/mask/capacity evidence to plan requirements and current supported-profile matrix.
- DR-002: Verify project/artifact revisions, graph validity and non-secure capability.
- DR-003: Distinguish unknown identity from a positive match; require an explicit safe policy for devices without a stable serial identity.
- DR-004: Recheck immediately before execution and invalidate outdated authorization if required identity changes.
- DR-005: Return a read-only preflight report and never silently downgrade Secure.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Matching qualified target passes and every mismatching/unknown required field blocks writing.
- [ ] AC-002: Changed project/plan/artifact revisions are detected.
- [ ] AC-003: The preflight itself sends no write/reset command.

## Dependencies to Other Tasks

- [Task-053](Task-053.md): Publish per-product compatibility diagnostics. Required predecessor behavior/evidence must be accepted.
- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-068](Task-068.md): Create reproducible immutable plans and compiler fingerprints. Required predecessor behavior/evidence must be accepted.
- [Task-058](Task-058.md): Validate graph DPT and communication semantics. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Simulator: correct/wrong/unknown identity and capacity overflow.
- TEST-002: Integration: project/artifact changes after compilation.
- TEST-003: Hardware read-only: capture reference preflight evidence.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / deployment preflight**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No guessing of product from address alone, automatic reset or acceptance based only on a cached identity.

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
