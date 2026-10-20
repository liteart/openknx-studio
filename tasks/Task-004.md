# Task-004: Register reference artifacts and the isolated test bench

## Subject

Register reference artifacts and the isolated test bench

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-004 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / private test fixtures |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Define the exact inputs and hardware needed for reproducible experiments, separating available files from missing external inputs.

## Detailed Requirements

- DR-001: Record the supplied DALI archive name, byte size and SHA-256 from REFERENCE-DCA.md.
- DR-002: Inventory the matching KNXPROD, one simple non-secure test device/application, gateway details, and an isolated bench; record verified identities for available inputs and explicit Missing status plus acquisition owner for the rest. This inventory can complete without acquiring every input; each consuming experiment remains blocked on its own missing prerequisites.
- DR-003: Keep the firmware-update DCA/device/firmware requirement separate and mark it missing until supplied.
- DR-004: Record power/recovery arrangements and permitted reversible properties from authoritative evidence.
- DR-005: Keep private files outside public Git and maintain a redacted hash manifest.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every PoC input has an Available/Missing status and exact identity.
- [ ] AC-002: No writable property/address or hardware model is guessed.
- [ ] AC-003: A human-approved bench/write policy exists before any hardware-write task is started.

## Dependencies to Other Tasks

- [Task-001](Task-001.md): Establish the executable scope and decision baseline. Required predecessor behavior/evidence must be accepted.
- [Task-002](Task-002.md): Create two isolated repositories and preserve licenses. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: verify file hashes and manifest completeness.
- TEST-002: Review: verify the bench qualification checklist, permitted-write policy and recorded Available/Missing statuses. Actual bench isolation and target identity must be confirmed by each hardware experiment before its first operation.
- TEST-003: Negative: missing input prevents the dependent experiment from reporting PASS.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / private test fixtures**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No hardware writes, firmware flashing, or redistribution of inputs.

## Risks and Blockers

Only the DALI .etsapp is provided in the chat. Matching KNXPROD, a simple device, real hardware access, and firmware-update fixtures remain external prerequisites.

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
