# Task-137: Evaluate a native fallback only after an explicit original-DCA blocker

## Subject

Evaluate a native fallback only after an explicit original-DCA blocker

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-137 |
| Status | Not started |
| Stage | Post-v1 |
| Priority within stage | P2 |
| Component / Git project | knx-editor / optional native compatibility research |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-026](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Provide a controlled path for an alternative implementation without abandoning the original-DCA-first decision.

## Detailed Requirements

- DR-001: Select one concrete unsupported original-DCA case only after documenting the actual blocker and receiving a maintainer scope decision.
- DR-002: Obtain permitted protocol/format evidence and define one minimal function, not a universal replacement.
- DR-003: Implement owned logic in Rust above the knust public API and use the same state/approval contracts.
- DR-004: Keep original binaries unchanged and do not copy proprietary implementation code.
- DR-005: Require real-device evidence before claiming parity for the selected function.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The alternative has an explicit approved reason and narrowly stated capability.
- [ ] AC-002: Licensing/evidence and repository boundaries are preserved.
- [ ] AC-003: Any capability claim is limited to the tested function/device/version.

## Dependencies to Other Tasks

- [Task-129](Task-129.md): Publish the approved stable release and archive evidence. Required predecessor behavior/evidence must be accepted.
- [Task-134](Task-134.md): Specify an open native extension capability contract. Required predecessor behavior/evidence must be accepted.
- [Task-135](Task-135.md): Implement one synthetic Rust extension conformance example. Required predecessor behavior/evidence must be accepted.
- [Task-111](Task-111.md): Publish per-function DCA qualification and unresolved gaps. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** Stable v1 published ([Task-129](Task-129.md)) and explicit approval to start this future scope.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/golden: the selected permitted format/function.
- TEST-002: Simulator: unauthorized/unsupported operations.
- TEST-003: Hardware authorized: compare one actual function outcome without claiming whole-DCA parity.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / optional native compatibility research**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No default native-first strategy, general reverse-engineering project, legacy plug-in support or implicit manufacturer adoption.

## Risks and Blockers

This is conditional future research. Without a specific blocker, permitted evidence and approval it remains Deferred/Not started; do not invent a device or scope to activate it.

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
