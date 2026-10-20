# Task-062: Enforce protocol session lifecycle and bounded failure behavior

## Subject

Enforce protocol session lifecycle and bounded failure behavior

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-062 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knust-fork / protocol sessions |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Prevent stale responses, leaked management sessions and unbounded retries during commissioning.

## Detailed Requirements

- DR-001: Model the required connection/session states and validate response correlation for the supported protocol.
- DR-002: Reject out-of-order/wrong-session messages and handle documented duplicate/retry behavior.
- DR-003: Use bounded timers/queues and explicit disconnect/cancellation cleanup.
- DR-004: Return uncertain execution where acknowledgement loss prevents a definitive result.
- DR-005: Leave project/gateway job locks and operation approval above the library.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: All modeled state transitions and timeout exits are covered by tests.
- [ ] AC-002: A reply from an old session cannot complete a new operation.
- [ ] AC-003: Closing/cancelling a client releases local resources without falsely declaring device rollback.

## Dependencies to Other Tasks

- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-061](Task-061.md): Bound segmented property and memory transfers. Required predecessor behavior/evidence must be accepted.
- [Task-017](Task-017.md): Implement and verify minimum non-secure tunneling. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: State-machine tests: normal/failure/close transitions.
- TEST-002: Fault simulator: delayed prior-session reply, duplicate acknowledgement and connection loss.
- TEST-003: Stress: repeated open/close with no unbounded resource growth.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / protocol sessions**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No project locking, UI state, persistent job database or guessing whether a device changed.

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
