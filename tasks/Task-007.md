# Task-007: Build a minimum Windows STA DCA host shell

## Subject

Build a minimum Windows STA DCA host shell

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-007 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host (C#/.NET) |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Create only the Windows process/UI lifecycle needed to host an original WPF-based DCA.

## Detailed Requirements

- DR-001: Use the verified runtime/bitness requirements, an STA UI thread, and a controlled message loop.
- DR-002: Show a neutral test view before attempting vendor loading.
- DR-003: Add startup/shutdown diagnostics, exception reporting, and explicit operation timeouts without pretending a timeout killed a vendor action.
- DR-004: Run as a normal interactive user; no administrator or ETS service dependency.
- DR-005: Do not add bus, Git, JSON domain, or compiler implementations.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The host opens and closes its test window on the clean Windows environment.
- [ ] AC-002: UI work executes on the required thread and shutdown leaves no child process.
- [ ] AC-003: Missing interactive session/runtime is reported explicitly.

## Dependencies to Other Tasks

- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows: open/close cycle and UI-thread assertion.
- TEST-002: Negative: noninteractive session and missing runtime/reference produce diagnostic failure.
- TEST-003: Static: verify no KNX network or product-domain implementation was added.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host (C#/.NET)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No original DCA behavior yet, no headless UI automation, no production IPC.

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
