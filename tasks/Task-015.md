# Task-015: Audit the pinned knust fork against the PoC operations

## Subject

Audit the pinned knust fork against the PoC operations

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-015 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / protocol audit |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Determine what the actual selected knust revision already provides and what must be implemented for the non-secure experiments.

## Detailed Requirements

- DR-001: Inspect public APIs/tests for discovery, tunneling, identity, properties, memory and management sessions.
- DR-002: Create a capability map of present-tested, present-unverified and absent behavior.
- DR-003: Record the exact feature set that excludes Secure; do not rely solely on README claims.
- DR-004: Identify protocol documentation/allowed captures required for missing operations.
- DR-005: Keep product-specific memory layouts out of the audit implementation.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The gap list names concrete public APIs/tests or missing services.
- [ ] AC-002: The PoC dependency graph uses the pinned fork revision.
- [ ] AC-003: No Secure work is added to fill an unrelated requirement.

## Dependencies to Other Tasks

- [Task-002](Task-002.md): Create two isolated repositories and preserve licenses. Required predecessor behavior/evidence must be accepted.
- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build/test: run existing non-secure tests with documented flags.
- TEST-002: Static: inspect feature graph and public exports.
- TEST-003: Review: compare capability claims with actual test coverage.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / protocol audit**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No wholesale protocol rewrite, full device support claim, or product import code.

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
