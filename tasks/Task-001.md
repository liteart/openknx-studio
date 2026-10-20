# Task-001: Establish the executable scope and decision baseline

## Subject

Establish the executable scope and decision baseline

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-001 |
| Status | In review |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / documentation |
| Owner | Primary coordinator with implementer, verifier and reviewer agents |
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

- [x] AC-001: Goals, AGENTS, task index, and requirements can be navigated in the repository.
- [x] AC-002: No earlier Java, all-.NET, Free/Pro, or database-authority proposal remains normative.
- [x] AC-003: A reviewer can identify each PoC success condition and the production stop gate.

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

| Field | Observed result |
|---|---|
| Implementation / report status | In review; offline increment verified, independent review complete; human acceptance pending |
| Source commit(s) | Base `47959c19c43c84a762b70cc2e8db3b9bc567b54b`; implementation `42ed64d0f9b1d9f95ea9f88bc64fb8707f91f042` |
| Platform | macOS 27.0 arm64; Python 3.14.6 |
| Tests executed | `python3 -B scripts/check_project_scope.py`: PASS; `python3 -B scripts/test_project_scope.py`: 10 PASS |
| Existing tooling checks | `python3 -B scripts/check_agent_configs.py`: PASS, including installed copies; `python3 -B scripts/test_agent_configs.py`: 9 PASS; `git diff --check`: PASS |
| Evidence location | This record; [scope baseline](../docs/SCOPE-BASELINE.md), [scope configuration](../config/project-scope.toml), checker and synthetic regression tests under `scripts/` |
| Acceptance criteria met | AC-001 through AC-003 technically demonstrated; maintainer acceptance pending |
| Technical outcome | Planning scope is pinned; local Markdown path/task identity checks pass; unauthorized scope drift fails without accepting an agent-claimed ADR |
| Gate outcome | No product gate reviewed or unlocked; Task-002 still requires accepted predecessor evidence |
| Reviewer | Independent Sol reviewer: no substantive findings; stale evidence pin/count/revision corrected; no maintainer acceptance |
| Windows / hardware / product / rights | NOT RUN; this documentation increment supplies no such proof |
| Next action | Human acceptance of Task-001; Task-002 local fork preparation explicitly requested by user, no publication |

### Review against explicit decisions (TEST-002)

The coordinator compared GOALS, AGENTS, architecture and the requirements baseline
with the explicit user contract on 2026-10-20. The normative implementation is Rust;
.NET is limited to necessary Windows original-DCA hosting. Wire protocols belong
only in knust; Apache-2.0 applies to owned source with no paid tiers. Multiple JSON
files and Git remain configuration authority, with SQLite rebuildable. Secure,
legacy plug-ins and the general editor UI are excluded from v1; original vendor UI
is the stated exception. Original DCAs take priority over native replacements.
References to earlier Java/Calimero, all-.NET, Falcon-core, database-authority and
Free/Pro proposals occur as superseded/excluded choices, not normative designs.
All existing requirement authority classes and engineering policy references are
preserved separately in the scope file. No product scope was changed.

### Negative scope review (TEST-003)

The synthetic tests alter Secure scope without an ADR and replace Rust with Java
while adding an agent-claimed accepted ADR. Both are rejected by the pinned scope
check. The review checklist requires an actual human-approved ADR and affected
contract/task/test review for scope changes. The checker detects drift; it cannot
authenticate human approval or prevent a malicious edit to both pin and scope.

### Readiness and delegation

The resumed coordinator used a read-only explorer followed by an independent
reviewer. The coordinator ran the checks and finished the paused draft. No subagent
committed, contacted hardware or advanced a gate.

The explorer found no configured remotes or local manufacturer archive. The old
archive hash in REFERENCE-DCA is historical static evidence, not current file
availability or rights. Actual owner/remote choices remain undecided; this does
not block the local baseline but must be resolved before external creation.
Other tasks retain their existing statuses. Human acceptance is not inferred from
agent review or a passing script.
