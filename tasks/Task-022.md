# Task-022: Record PoC B hardware read-write evidence

## Subject

Record PoC B hardware read-write evidence

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-022 |
| Status | Not started |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / examples and PoC report |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Execute the minimal device-management experiment and publish its exact supported service evidence.

## Detailed Requirements

- DR-001: Use the registered isolated bench and an explicit operator-approved run.
- DR-002: Record fork commit, features, gateway/device identity, original/new/restored values, and redacted protocol traces.
- DR-003: Verify cleanup and report any uncertain device state.
- DR-004: Distinguish simulator coverage from actual hardware coverage of each primitive.
- DR-005: Do not label this a complete application download.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Read, write, verification and restoration are evidenced on the reference hardware.
- [ ] AC-002: The report names all untested low-level services.
- [ ] AC-003: The PoC outcome is reproducible and tied to the tested revision.

## Dependencies to Other Tasks

- [Task-018](Task-018.md): Read and identify the reference device through KNX management. Required predecessor behavior/evidence must be accepted.
- [Task-019](Task-019.md): Implement minimum generic KNX property reads. Required predecessor behavior/evidence must be accepted.
- [Task-020](Task-020.md): Implement an explicitly authorized reversible property write. Required predecessor behavior/evidence must be accepted.
- [Task-021](Task-021.md): Add only the selected profile memory and load-state primitives. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware: perform the approved reversible sequence.
- TEST-002: Failure review: inspect at least one simulated timeout/denial path.
- TEST-003: Evidence review: missing read-back or restoration prevents the intended PASS.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / examples and PoC report**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No product compiler, MCP server, or application programming claim.

## Risks and Blockers

Unavailable hardware or approval means the hardware result is NOT RUN/INCONCLUSIVE, never inferred from simulator tests.

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
