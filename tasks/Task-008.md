# Task-008: Map the actual ETS SDK surface used by the reference DCA

## Subject

Map the actual ETS SDK surface used by the reference DCA

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-008 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host analysis |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Identify the minimum external services and identities needed by this DCA rather than implementing an imaginary generic ETS runtime.

## Detailed Requirements

- DR-001: Use permitted metadata analysis and controlled load diagnostics to list referenced assemblies, public interfaces, required concrete types, callbacks and UI entry points.
- DR-002: Record exact assembly versions/public identities and distinguish missing type from missing service/activation.
- DR-003: Classify each dependency as implementable bridge contract, allowed reference, unknown, or blocker.
- DR-004: Explicitly inspect storage, product/device context, network service and download-hook expectations.
- DR-005: Preserve analysis provenance and do not modify manufacturer binaries.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every attempted initialization failure maps to a named dependency with evidence.
- [ ] AC-002: A small ordered shim plan exists for the single reference DCA.
- [ ] AC-003: Strong naming or proprietary concrete types are not dismissed as solved by interface naming.

## Dependencies to Other Tasks

- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.
- [Task-006](Task-006.md): Inspect the original MDT DCA package without executing it. Required predecessor behavior/evidence must be accepted.
- [Task-007](Task-007.md): Build a minimum Windows STA DCA host shell. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: reproduce the dependency map from the pinned package.
- TEST-002: Controlled Windows: capture the first real missing dependency failure.
- TEST-003: Review: confirm unknown entries are not represented as implemented services.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host analysis**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No universal ETS API coverage, binary patches, license emulation, or proprietary code copying.

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
