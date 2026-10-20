# Task-005: Resolve lawful DCA and SDK dependency provisioning

## Subject

Resolve lawful DCA and SDK dependency provisioning

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-005 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / dca-host dependency review |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-002](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Determine whether the original reference DCA can legally and technically receive the SDK contracts it needs without requiring ETS at runtime.

## Detailed Requirements

- DR-001: Inventory license/provisioning terms for the original DCA, referenced SDK assemblies, and any runtime dependencies.
- DR-002: Separate compile-time reference contracts from concrete proprietary ETS runtime services.
- DR-003: Record whether dependencies can be obtained and used on a clean machine without installing or activating ETS.
- DR-004: Record strong-name/assembly identity and activation constraints for follow-up; do not bypass them.
- DR-005: Keep a dependency disposition table: allowed, unavailable, needs review, or incompatible.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: There is an approved dependency provisioning procedure or an explicit blocker report.
- [ ] AC-002: The procedure does not copy an arbitrary ETS installation or publish vendor DLLs by default.
- [ ] AC-003: Host implementation cannot silently depend on a prohibited ETS runtime.

## Dependencies to Other Tasks

- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Review: examine actual terms and packaging/provisioning evidence.
- TEST-002: Reproduction: obtain permitted references through the documented route.
- TEST-003: Negative: absent rights or activation prerequisites close the affected gate.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / dca-host dependency review**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No legal certainty inferred from public download availability, no binary patching, no full SDK reimplementation.

## Risks and Blockers

The necessary SDK terms and redistribution/runtime rights are not established by the chat. A human review is required where rights are unclear.

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
