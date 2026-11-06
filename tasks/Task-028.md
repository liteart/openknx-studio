# Task-028: Decide the PoC gate before any product infrastructure

## Subject

Decide the PoC gate before any product infrastructure

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-028 |
| Status | Not started |
| Stage | PoC gate |
| Priority within stage | P0 |
| Component / Git project | knx-editor / governance |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Review all three minimal experiments and issue an explicit GO or NO-GO for production implementation.

## Detailed Requirements

- DR-001: Check each experiment against its separate success condition and immutable evidence.
- DR-002: Confirm ETS independence, unchanged original DCA, protocol repository boundaries and no Secure scope.
- DR-003: Record unresolved production risks separately from failures of the minimum proofs.
- DR-004: Require maintainer approval; a finished report with failed or missing hardware evidence is NO-GO.
- DR-005: On NO-GO create only a narrowly targeted follow-up experiment or explicit scope decision, not the full application.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The gate record has a named human reviewer, date, exact tested revisions and a GO/NO-GO outcome.
- [ ] AC-002: GO exists only when PoCs A/B/C have sufficient PASS evidence and lawful dependency conditions.
- [ ] AC-003: Downstream tasks are not considered Ready for a NO-GO outcome.

## Dependencies to Other Tasks

- [Task-014](Task-014.md): Record PoC A evidence and its exact limitations. Required predecessor behavior/evidence must be accepted.
- [Task-022](Task-022.md): Record PoC B hardware read-write evidence. Required predecessor behavior/evidence must be accepted.
- [Task-027](Task-027.md): Execute and verify the minimum independent application download. Required predecessor behavior/evidence must be accepted.

The selected Task020 write candidate also depends on the connected device
session in [Task-151](Task-151.md), through Task022. Simulator-only session
evidence does not substitute for the required hardware write and recovery.

**Required phase gate:** PoC A/B/C evidence must be reviewed; this task decides G-POC.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Review: audit evidence provenance and replay a selected run.
- TEST-002: Negative: replace one PASS with INCONCLUSIVE and verify the readiness rule closes.
- TEST-003: Boundary review: no product scaffold is used to disguise an unproven DCA/protocol path.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / governance**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No new feature implementation inside the gate review.

## Risks and Blockers

All hardware and Windows evidence must be real. Documentation creation itself does not pass this gate.

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
