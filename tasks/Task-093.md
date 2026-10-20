# Task-093: Configure optional Git remotes and protected credentials

## Subject

Configure optional Git remotes and protected credentials

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-093 |
| Status | Not started |
| Stage | Beta |
| Priority within stage | P1 |
| Component / Git project | knx-editor / Git remote service |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-017](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md) |

## Description

Add provider-neutral remote integration for every user without changing offline local behavior.

## Detailed Requirements

- DR-001: Support explicitly configured reviewed HTTPS/SSH or authorized local remote paths; keep actual provider URLs user-supplied.
- DR-002: Store credentials in an OS-protected facility and never in project JSON, Git URLs, logs or MCP output.
- DR-003: Reject unsafe remote helpers/command injection and do not inherit executable hooks/helpers from an imported repository automatically.
- DR-004: Use explicit remote actions, not background push/pull; no provider-specific account requirement for local use.
- DR-005: Record sanitized remote identity and permission errors.
- DR-006: Expose git_remote_configure/inspect/remove through typed MCP tools without returning credentials; no generic Git command tool.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A user can configure/remove a remote without exposing credentials.
- [ ] AC-002: Local project operations still work when disconnected or unconfigured.
- [ ] AC-003: Unsafe URL/helper schemes are rejected before execution.

## Dependencies to Other Tasks

- [Task-092](Task-092.md): Approve the alpha integration gate. Required predecessor behavior/evidence must be accepted.
- [Task-039](Task-039.md): Expose automatic local configuration history and restore. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-ALPHA = GO ([Task-092](Task-092.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: URL/path parsing and redaction.
- TEST-002: Integration: authenticated test remote and offline fallback.
- TEST-003: Negative: command-like URL, malicious helper/hook configuration and missing credential.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / Git remote service**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No hosted provider account management, paid remote feature or auto-sync.

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
