# Task-012: Round-trip one original DCA configuration change

## Subject

Round-trip one original DCA configuration change

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-012 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host state bridge |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Prove that a real user change in the original DCA can leave the host process and survive a restart.

## Detailed Requirements

- DR-001: Select one non-bus configuration operation actually offered by the DCA and document its expected changed value.
- DR-002: Capture the real state/parameter change through the discovered storage contract; preserve opaque bytes if semantics are unknown.
- DR-003: Persist a minimal file envelope with device/app/DCA hashes, then close/restart the host and reload it.
- DR-004: Detect no-op versus changed state and reject state for a different device/application.
- DR-005: Keep the original assembly unchanged.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: After restart the same original DCA displays the selected changed value.
- [ ] AC-002: The saved state is not only in memory or SQLite and its hash is recorded.
- [ ] AC-003: A failed state capture is not reported as a successful round-trip.

## Dependencies to Other Tasks

- [Task-011](Task-011.md): Supply one coherent device and product context to the DCA. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows integration: change, save, terminate normally, reload, compare.
- TEST-002: Negative: corrupted/mismatched state envelope and missing blob.
- TEST-003: No-op: reopening without edits preserves the captured configuration payload.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host state bridge**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No production transaction engine, merge of opaque state, or live gateway transfer.

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


## Read-only readiness checkpoint — 2026-11-02

Task011 remains in progress, so Task012 is not ready. Source103 original UI run
`54b41270cd58` rendered and closed, but recorded parameter values not read,
configuration acceptance not established, a transient fixture-only writer and
`state=NOT_RUN`. These observations do not establish a user configuration change
or persistent state. Once Task011 is accepted, the first bounded investigation
is one actually offered non-bus editable value and its genuine before/after
read, write and save path; only then can the file envelope and restart/reload
requirements be implemented. Hardware and bus operations are not required for
that first investigation.
