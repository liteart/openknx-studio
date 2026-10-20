# Task-063: Qualify individual-address programming on the reference bench

## Subject

Qualify individual-address programming on the reference bench

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-063 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knust-fork / individual address services |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Provide and verify the low-level address programming path required for new supported devices.

## Detailed Requirements

- DR-001: Implement only the documented addressing method supported by the reference device/gateway.
- DR-002: Keep operator choice/approval in the caller; expose required discovery/programming-mode evidence without automatically targeting the first responder.
- DR-003: Reject ambiguous/multiple responding devices, collisions and unsupported addressing modes.
- DR-004: Read back the resulting identity/address and record restoration/recovery steps.
- DR-005: Do not readdress the gateway or live installation implicitly.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected isolated target receives and verifies the approved individual address.
- [ ] AC-002: Ambiguous/colliding target selection produces no write.
- [ ] AC-003: Support is scoped to the tested method and device capabilities.

## Dependencies to Other Tasks

- [Task-060](Task-060.md): Stabilize the fork public management API for the editor. Required predecessor behavior/evidence must be accepted.
- [Task-062](Task-062.md): Enforce protocol session lifecycle and bounded failure behavior. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Simulator: zero/one/multiple responder and collision cases.
- TEST-002: Hardware authorized: program/read-back the selected target address.
- TEST-003: Negative: wrong target/gateway exclusion and unsupported method.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / individual address services**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No bulk addressing, Secure commissioning, serial-number method assumed for every device or editor auto-discovery matching.

## Risks and Blockers

Requires a supported documented address-programming method and explicit bench approval; some devices may lack stable pre-address identity.

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
