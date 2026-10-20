# Task-099: Register KNXPROJ fixtures and safe import preparation

## Subject

Register KNXPROJ fixtures and safe import preparation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-099 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / project-import boundary |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Define the exact exported project forms the first migration implementation will support.

## Detailed Requirements

- DR-001: Acquire a user-approved redacted/reference KNXPROJ with known expected devices, parameters, topology, group bindings and any DCA state.
- DR-002: Record archive hash, export/schema version, encryption/password requirements and permitted extraction route.
- DR-003: Apply bounded safe archive/XML handling; obtain passwords only through a protected input and never log/store them.
- DR-004: Distinguish project-archive encryption from excluded KNX Secure/keyrings.
- DR-005: Allow documented one-time ETS export preparation, but no ongoing ETS dependency.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The supported reference export can be prepared/extracted with a complete manifest and no data mutation.
- [ ] AC-002: Unsupported/encrypted forms without a supported password route fail with clear preparation instructions.
- [ ] AC-003: Private project data and passwords remain out of public Git.

## Dependencies to Other Tasks

- [Task-092](Task-092.md): Approve the alpha integration gate. Required predecessor behavior/evidence must be accepted.
- [Task-044](Task-044.md): Harden KNXPROD archive and XML ingestion. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Fixture: verify archive identity and expected import inventory.
- TEST-002: Negative: wrong/missing password, unsupported schema and malicious archive.
- TEST-003: Privacy: inspect logs/fixtures for real addresses/names/credentials.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / project-import boundary**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No live ETS synchronization, bypass of archive passwords or KNX Secure import.

## Risks and Blockers

No KNXPROJ is supplied in this chat. A matching export and expected-result inventory are external prerequisites.

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
