# Task-064: Expose bounded group monitoring and verified DPT helpers

## Subject

Expose bounded group monitoring and verified DPT helpers

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-064 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knust-fork / monitoring and DPT API |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Provide basic diagnostic observation and value conversion without moving codecs into the editor.

## Detailed Requirements

- DR-001: Expose a bounded subscription/stream of observed group events with timestamps/source metadata.
- DR-002: Report dropped events when a consumer is slow; do not grow memory indefinitely or block commissioning indefinitely.
- DR-003: Expose the DPT/address helpers required by the supported corpus through the public API.
- DR-004: Distinguish observed group values from confirmed device configuration.
- DR-005: Keep higher-level filter/UI/project labeling in the editor.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference group telegram can be observed and decoded through public helpers.
- [ ] AC-002: Slow consumers receive loss information and bounded memory use.
- [ ] AC-003: Unsupported DPT conversion returns an explicit error/raw representation.

## Dependencies to Other Tasks

- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-062](Task-062.md): Enforce protocol session lifecycle and bounded failure behavior. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: supported DPT boundary vectors and invalid payloads.
- TEST-002: Simulator: ordering, backpressure and dropped-event accounting.
- TEST-003: Hardware read-only: observe the approved functional verification event.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / monitoring and DPT API**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automation engine, unbounded bus recording or inference of full device state from traffic.

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
