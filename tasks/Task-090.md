# Task-090: Test MCP misuse, idempotency and authority boundaries

## Subject

Test MCP misuse, idempotency and authority boundaries

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-090 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / MCP security and contract tests |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Verify that agent-facing behavior remains safe under malicious or simply incorrect requests.

## Detailed Requirements

- DR-001: Exercise forged IDs/revisions, duplicated request IDs, artifact/path traversal, overlarge payloads and unsupported capabilities.
- DR-002: Include product descriptions and fixture text containing instructions and confirm they are treated only as data.
- DR-003: Verify backend authorization independently of tool annotations and human wording.
- DR-004: Validate all structured outputs against pinned schemas and English fallback behavior.
- DR-005: Use a fake protocol adapter to assert zero writes in denied cases.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Denied requests cannot mutate files or devices.
- [ ] AC-002: Replayed write requests resolve to existing outcomes or explicit conflicts rather than duplicate effects.
- [ ] AC-003: Prompt-like content cannot change tool permissions or code execution behavior.

## Dependencies to Other Tasks

- [Task-084](Task-084.md): Provide revision-aware paginated project and catalog resources. Required predecessor behavior/evidence must be accepted.
- [Task-085](Task-085.md): Expose project, catalog and changeset MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-086](Task-086.md): Expose device and semantic-connection MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-087](Task-087.md): Expose compile, preflight and approved deployment MCP tools. Required predecessor behavior/evidence must be accepted.
- [Task-088](Task-088.md): Expose original DCA session tools with honest interaction status. Required predecessor behavior/evidence must be accepted.
- [Task-089](Task-089.md): Expose durable job status and bounded bus diagnostics. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Adversarial contract corpus for each write tool family.
- TEST-002: Property/fuzz: bounded malformed RPC/schema inputs.
- TEST-003: Security: annotation removal and forged approval/operation IDs.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / MCP security and contract tests**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No claim to secure an agent with unrestricted OS access or guarantee that prompts cannot affect its reasoning.

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
