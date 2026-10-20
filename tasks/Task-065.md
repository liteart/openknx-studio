# Task-065: Regression-test the non-secure protocol feature set

## Subject

Regression-test the non-secure protocol feature set

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-065 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knust-fork / protocol qualification |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Establish a reproducible regression suite for the exact fork features used by v1.

## Detailed Requirements

- DR-001: Combine verified vectors, simulator failures and separately identified hardware evidence for the supported services.
- DR-002: Add malformed/truncated/oversized frame tests and deterministic randomized/property tests.
- DR-003: Assert secure-required requests are rejected without testing or enabling Secure cryptography.
- DR-004: Preserve source provenance for protocol fixtures and remove private installation details.
- DR-005: Record library revision and feature flags in the support report.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The approved non-secure test matrix passes at the pinned fork revision.
- [ ] AC-002: Invalid traffic cannot panic or produce unbounded allocation.
- [ ] AC-003: Unexecuted hardware cases are not marked covered by unit tests.

## Dependencies to Other Tasks

- [Task-061](Task-061.md): Bound segmented property and memory transfers. Required predecessor behavior/evidence must be accepted.
- [Task-062](Task-062.md): Enforce protocol session lifecycle and bounded failure behavior. Required predecessor behavior/evidence must be accepted.
- [Task-063](Task-063.md): Qualify individual-address programming on the reference bench. Required predecessor behavior/evidence must be accepted.
- [Task-064](Task-064.md): Expose bounded group monitoring and verified DPT helpers. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Automated: codec/property/session/fault regression suite.
- TEST-002: Negative: secure-required selection, unsupported service and malformed traffic.
- TEST-003: Hardware: rerun the declared reference service subset with approval.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / protocol qualification**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No claim of KNX certification, universal protocol coverage or Secure interoperability.

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
