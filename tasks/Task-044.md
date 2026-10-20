# Task-044: Harden KNXPROD archive and XML ingestion

## Subject

Harden KNXPROD archive and XML ingestion

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-044 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / product-engine import boundary |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Promote the narrow PoC reader into a safe reusable import boundary without broadening configuration semantics implicitly.

## Detailed Requirements

- DR-001: Apply EP-03/04 archive/XML limits before allocating/extracting untrusted content.
- DR-002: Reject traversal, absolute paths, symlinks, ambiguous case collisions, duplicate names and XML external entities/DTD.
- DR-003: Verify available integrity metadata separately from manufacturer authenticity; never call a SHA/ZIP CRC a signature verification.
- DR-004: Retain original archive/hash and produce a no-side-effect import report.
- DR-005: Unknown schema/features remain visible for the downstream compatibility classifier.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Valid registered packages import offline into immutable storage.
- [ ] AC-002: Malicious or oversized fixtures fail without escaping the target directory or contacting a network.
- [ ] AC-003: Import reports distinguish byte integrity, signature status and semantic support.

## Dependencies to Other Tasks

- [Task-023](Task-023.md): Parse only the selected KNXPROD application for PoC C. Required predecessor behavior/evidence must be accepted.
- [Task-029](Task-029.md): Create the production Rust workspace with boundary checks. Required predecessor behavior/evidence must be accepted.
- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.
- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Adversarial: traversal, symlink, compression bomb, huge entry count, XML expansion/external entity.
- TEST-002: Integration: interrupted import and duplicate artifact.
- TEST-003: Regression: the PoC reference package still imports unchanged.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / product-engine import boundary**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No vendor signature bypass, code/script execution or automatic dependency download.

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
