# Task-027: Execute and verify the minimum independent application download

## Subject

Execute and verify the minimum independent application download

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-027 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download hardware test |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Prove the end-to-end KNXPROD-to-device chain on the approved reference device, without ETS performing the download.

## Detailed Requirements

- DR-001: Start from a recorded different or unconfigured state, not an already matching configuration.
- DR-002: Verify the exact target/application and operator-approved plan on an isolated bench before writing.
- DR-003: Execute solely through the knust fork; record raw outcome and postconditions without fabricated success.
- DR-004: Verify available application/load status and at least one functional effect of the changed parameter/group association.
- DR-005: Record a recovery procedure and final device state; keep ETS absent from the execution path.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected device runs the compiled application/configuration and responds to the intended group binding.
- [ ] AC-002: Evidence links the original KNXPROD hash, editor/fork commits and plan/image hashes.
- [ ] AC-003: Failed/uncertain steps are reflected in the result and do not count as a verified download.

## Dependencies to Other Tasks

- [Task-022](Task-022.md): Record PoC B hardware read-write evidence. Required predecessor behavior/evidence must be accepted.
- [Task-026](Task-026.md): Assemble the minimum complete reference download plan. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware authorized: full selected-profile download and observable function.
- TEST-002: Independent check: compare observed identity/configuration behavior to the planned result.
- TEST-003: Simulator negative: interruption/error produces controlled failure and no false completion.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download hardware test**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No live household rollout, firmware update, DALI integration, or broad device compatibility claim.

## Risks and Blockers

Requires actual lab hardware, exact product input, protocol evidence and explicit write approval. No simulated-only substitute satisfies acceptance.

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
