# Task-135: Implement one synthetic Rust extension conformance example

## Subject

Implement one synthetic Rust extension conformance example

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-135 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / native extension example |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-026](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Validate the open contract with owned synthetic code rather than copying manufacturer behavior.

## Detailed Requirements

- DR-001: Implement a small Rust extension for a clearly fictional test capability with configuration/state round-trip.
- DR-002: Use the same resource/permission/IPC/version constraints as the contract and no real bus access by default.
- DR-003: Exercise registration, capability query, validation, state save/reload and controlled failure.
- DR-004: Keep any protocol request routed through the core/fork mock rather than encoding KNX.
- DR-005: Label the example as conformance material, not support for a real device.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The example runs on all core platforms and passes contract tests.
- [ ] AC-002: State is durable/versioned through the host services.
- [ ] AC-003: Unauthorized effects and malformed responses are rejected.

## Dependencies to Other Tasks

- [Task-134](Task-134.md): Specify an open native extension capability contract. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.
- [Task-077](Task-077.md): Persist real DCA state through editor changesets. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Cross-platform: conformance lifecycle and state round-trip.
- TEST-002: Negative: capability escalation, crash and protocol mismatch.
- TEST-003: Static: no copied vendor code or embedded wire driver.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / native extension example**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No production manufacturer adapter or claim of replacing the MDT DCA.

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
