# Task-001: Establish the executable scope and decision baseline

## Subject

Establish the executable scope and decision baseline

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-001 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / documentation |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Install the approved planning documents into the actual development repository without treating the documentation kit as implemented functionality.

## Detailed Requirements

- DR-001: Retain every confirmed requirement and mark engineering defaults separately.
- DR-002: Record the logical repository names and obtain the actual owner/remote choices before external creation.
- DR-003: Keep all tasks Not started until work and evidence exist; preserve stable IDs and relative links.
- DR-004: Record excluded Secure, legacy, general UI, and native-replacement scope in a machine-checkable project scope file or equivalent testable configuration.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Goals, AGENTS, task index, and requirements can be navigated in the repository.
- [ ] AC-002: No earlier Java, all-.NET, Free/Pro, or database-authority proposal remains normative.
- [ ] AC-003: A reviewer can identify each PoC success condition and the production stop gate.

## Dependencies to Other Tasks

- No predecessor implementation tasks. Follow the confirmed baseline and external authorization rules.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Documentation: validate all local links and task ID uniqueness.
- TEST-002: Review: compare the baseline against explicit user decisions.
- TEST-003: Negative: a scope change without an ADR is identified by the review checklist.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / documentation**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No protocol, product parser, DCA execution, or repository publication.

## Risks and Blockers

Actual repository owner and remote URLs have not been supplied. Do not invent them or create a public repository without authorization.

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
