# Task-076: Map required ETS project/device contracts to Rust services

## Subject

Map required ETS project/device contracts to Rust services

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-076 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / DCA bridge and SDK adapter |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md) |

## Description

Replace fixture-only project context with real editor data through the minimum approved SDK contract surface.

## Detailed Requirements

- DR-001: Map stable project/device/product/parameter identities and events into the exact SDK objects used by the reference DCA.
- DR-002: Keep Rust as the authority for validation and state changes; .NET performs necessary object/IPC adaptation only.
- DR-003: Handle reference versions/callback lifetimes explicitly and reject unsupported SDK services.
- DR-004: Supply a session snapshot/revision and detect concurrent project change.
- DR-005: Do not instantiate ETS executables/project stores or retain fictitious fixture values.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The original DCA opens against a real reference project snapshot.
- [ ] AC-002: Reads/writes target the intended stable device and parameter IDs.
- [ ] AC-003: Unsupported service or stale revision returns a precise failure without partial accepted state.

## Dependencies to Other Tasks

- [Task-009](Task-009.md): Implement only the required offline SDK compatibility surface. Required predecessor behavior/evidence must be accepted.
- [Task-011](Task-011.md): Supply one coherent device and product context to the DCA. Required predecessor behavior/evidence must be accepted.
- [Task-054](Task-054.md): Implement device and topology lifecycle commands. Required predecessor behavior/evidence must be accepted.
- [Task-057](Task-057.md): Implement parameter and graph edit domain commands. Required predecessor behavior/evidence must be accepted.
- [Task-075](Task-075.md): Supervise the Windows DCA worker with versioned local IPC. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Contract: Rust/.NET DTO round-trip and callback lifetime.
- TEST-002: Integration: original DCA opens the real reference device.
- TEST-003: Negative: stale revision, missing concrete SDK dependency and wrong application context.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / DCA bridge and SDK adapter**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No broad ETS runtime reimplementation or business-rule duplication in C#.

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
