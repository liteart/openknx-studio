# Task-083: Implement a bounded MCP stdio server using the Rust SDK

## Subject

Implement a bounded MCP stdio server using the Rust SDK

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-083 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / mcp-server |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-006](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Expose the backend through the primary local agent interface without coupling domain logic to MCP.

## Detailed Requirements

- DR-001: Pin rmcp and explicitly supported protocol versions/capabilities using the current SDK evidence.
- DR-002: Use stdio for v1, valid protocol messages only on stdout and diagnostics on stderr.
- DR-003: Enforce EP-05 input limits and structured error mapping.
- DR-004: Tool annotations describe effects but do not replace authorization in application services.
- DR-005: Startup requires neither a model credential nor a network service.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: An MCP client initializes/lists tools and receives valid structured responses.
- [ ] AC-002: Logs do not corrupt stdout.
- [ ] AC-003: Invalid/oversized requests fail without invoking a domain/hardware operation.

## Dependencies to Other Tasks

- [Task-029](Task-029.md): Create the production Rust workspace with boundary checks. Required predecessor behavior/evidence must be accepted.
- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.
- [Task-031](Task-031.md): Introduce English-first localization in the Rust core. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Protocol contract: initialize, version negotiation, tools/list and malformed request.
- TEST-002: Integration: launch server as a subprocess and separate stdout/stderr.
- TEST-003: Negative: missing model/account/network does not block startup.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / mcp-server**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No remote HTTP server, LLM integration, sampling requirement or tool self-approval.

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
