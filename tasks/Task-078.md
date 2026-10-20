# Task-078: Mediate original DCA bus requests through Rust and knust

## Subject

Mediate original DCA bus requests through Rust and knust

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-078 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / DCA bridge + knust public API |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Connect the original DCA to the same validated protocol path without adding a second driver.

## Detailed Requirements

- DR-001: Translate required SDK network/property/memory/service calls into typed Rust requests and knust public operations.
- DR-002: Check exact session/device/package authorization, gateway lease and operation capability before dispatch.
- DR-003: Preserve opaque manufacturer payloads without decoding KNX wire frames in C#.
- DR-004: Return genuine responses/error/uncertainty to the vendor lifecycle; do not fabricate successful SDK calls.
- DR-005: Identify direct Falcon/socket or other bypass requirements and reject unsupported execution paths until resolved.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference DCA can perform a permitted read through the mediated path.
- [ ] AC-002: The .NET host implements zero KNX packet/session code and cannot request an unauthorized target.
- [ ] AC-003: Any necessary unmediated vendor bus driver is documented as a blocking incompatibility.

## Dependencies to Other Tasks

- [Task-013](Task-013.md): Demonstrate fail-closed PoC DCA bus isolation. Required predecessor behavior/evidence must be accepted.
- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-069](Task-069.md): Implement target identity and capacity preflight. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.
- [Task-071](Task-071.md): Execute durable sequential programming jobs. Required predecessor behavior/evidence must be accepted.
- [Task-076](Task-076.md): Map required ETS project/device contracts to Rust services. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Contract: property/memory request/response mapping and cancellation.
- TEST-002: Security negative: wrong target/service/session and unauthorized write.
- TEST-003: Hardware read-only: original DCA request observed through the knust path.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / DCA bridge + knust public API**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No Falcon runtime driver fallback, raw MCP packet tool or claim that IPC alone blocks arbitrary vendor network code.

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
