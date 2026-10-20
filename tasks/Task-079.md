# Task-079: Authorize interactive DCA operations and session effects

## Subject

Authorize interactive DCA operations and session effects

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-079 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / DCA authorization |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Handle stateful original DCA workflows without pretending every dynamic operation is a precomputed configuration image.

## Detailed Requirements

- DR-001: Create operation grants tied to package hash, device identity, source revision, purpose, service/range allowlist and bounded session.
- DR-002: Treat discovery/assignment/replace/update according to their actual effects; do not call an operation read-only because its UI says scan.
- DR-003: Separate firmware, reset, addressing and configuration grants and require a trusted human approval path.
- DR-004: Record each mediated effect and deny escalation to another target/method.
- DR-005: Expiration prevents new operation starts; active unsafe-to-interrupt stages follow documented completion/recovery rules.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: An approved DCA operation can proceed while unrelated writes are denied.
- [ ] AC-002: The agent cannot elevate a read session to update/reset.
- [ ] AC-003: Dynamic results remain within the approved purpose/target scope and are audited.

## Dependencies to Other Tasks

- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.
- [Task-075](Task-075.md): Supervise the Windows DCA worker with versioned local IPC. Required predecessor behavior/evidence must be accepted.
- [Task-078](Task-078.md): Mediate original DCA bus requests through Rust and knust. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: scope/expiry/purpose/target grants.
- TEST-002: Simulator: discovery that invokes a write and denied escalation.
- TEST-003: Windows integration: user approval, permitted operation and explicit cancellation behavior.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / DCA authorization**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No unrestricted lifetime whole-bus session or unsafe force-termination on approval expiry.

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
