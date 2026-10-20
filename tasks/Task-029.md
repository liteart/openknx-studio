# Task-029: Create the production Rust workspace with boundary checks

## Subject

Create the production Rust workspace with boundary checks

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-029 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / workspace |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Create the smallest production module structure after the explicit PoC GO decision.

## Detailed Requirements

- DR-001: Separate deterministic domain/product/compiler code from side-effecting persistence, deployment, MCP and DCA adapters.
- DR-002: Pin the accepted fork revision through its public API, not a copied source directory or committed local path override.
- DR-003: Add an architecture check for forbidden reverse dependencies and KNX wire implementations outside the fork.
- DR-004: Keep the .NET host out of the required core build on Linux/macOS.
- DR-005: Port only PoC code that has accepted behavior; label remaining fixtures as test-only.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The workspace builds with the approved non-secure features.
- [ ] AC-002: An intentional forbidden dependency makes the architecture check fail.
- [ ] AC-003: The core can be built without the DCA host.

## Dependencies to Other Tasks

- [Task-028](Task-028.md): Decide the PoC gate before any product infrastructure. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build: clean locked workspace build.
- TEST-002: Static: dependency/ownership rule tests including a deliberate violation fixture.
- TEST-003: Cross-platform: build smoke test on the declared core targets.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / workspace**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No additional protocol implementation, GUI toolkit selection, or product feature expansion.

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
