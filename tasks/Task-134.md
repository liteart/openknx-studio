# Task-134: Specify an open native extension capability contract

## Subject

Specify an open native extension capability contract

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-134 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / extension API design |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-026](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Offer manufacturers a documented alternative to Windows-only SDK hosting without assuming they will adopt it.

## Detailed Requirements

- DR-001: Define versioned descriptors, supported product identities, configuration schema, operation capabilities, state format and localization resources.
- DR-002: Preserve the Rust/core/knust separation and use the same approvals/state transactions for native extensions.
- DR-003: Prefer a process/structured-contract boundary rather than promising a stable Rust binary ABI.
- DR-004: Keep original DCA hosting as first priority and require explicit selection/version pinning.
- DR-005: Document licensing/provenance and no automatic trust/execution.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A manufacturer can understand the contract without ETS internals.
- [ ] AC-002: The contract cannot introduce a second KNX driver or unreviewed device side effects.
- [ ] AC-003: Native availability is optional and never claimed before evidence.

## Dependencies to Other Tasks

- [Task-129](Task-129.md): Publish the approved stable release and archive evidence. Required predecessor behavior/evidence must be accepted.
- [Task-080](Task-080.md): Describe DCA capabilities and interaction requirements. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Schema/contract review with a synthetic capability descriptor.
- TEST-002: Negative: unknown major, overbroad capability and conflicting product identity.
- TEST-003: Architecture review: approvals and persistence remain owned by the core.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / extension API design**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No vendor adoption promise, native adapter for an unknown device or replacing original DCAs by default.

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
