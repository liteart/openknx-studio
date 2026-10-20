# Task-121: Run the final hardware and application compatibility corpus

## Subject

Run the final hardware and application compatibility corpus

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-121 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / integration qualification |
| Owner | Unassigned |
| Requirement references | [R-005](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-012](../docs/REQUIREMENTS.md), [R-013](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Freeze and re-test every device/application/DCA combination intended for the first stable support matrix.

## Detailed Requirements

- DR-001: Use exact approved product/application/mask/firmware/DCA/core/fork combinations with registered fixture hashes.
- DR-002: Re-run full ordinary device download, individual addressing, original DALI workflows and the selected firmware-update scenario where a safe repeat is supported.
- DR-003: Use prior accepted update evidence only when the exact release path is unchanged and reviewers explicitly justify any nonrepeatable physical case.
- DR-004: Include imported-project/state and cache-free restart cases.
- DR-005: Mark missing or changed-version evidence as blocking/unqualified.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every stable-release support claim has sufficient hardware and platform evidence at the frozen revisions.
- [ ] AC-002: No model/version inherits support solely from a similar device.
- [ ] AC-003: Nonrepeatable tests have explicit reviewed evidence, not an unexplained skipped status.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-113](Task-113.md): Build and smoke-test the declared platform packages. Required predecessor behavior/evidence must be accepted.
- [Task-114](Task-114.md): Verify clean-machine ETS and service independence. Required predecessor behavior/evidence must be accepted.
- [Task-116](Task-116.md): Execute the full file, Git and cache crash matrix. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Hardware/Windows/platform matrix according to TEST-STRATEGY.
- TEST-002: Regression: parameter/group/scene/state outcomes.
- TEST-003: Negative: wrong application/profile/version and unsupported Secure-required device.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / integration qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No KNX certification or expansion to new hardware families during RC.

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
