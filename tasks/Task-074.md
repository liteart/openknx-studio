# Task-074: Register and verify unchanged DCA packages

## Subject

Register and verify unchanged DCA packages

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-074 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / DCA registry |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-014](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Accept original DCA packages as explicit user-provided executable dependencies with versioned identity.

## Detailed Requirements

- DR-001: Read manifest/assembly inventory safely and retain the original hash-qualified archive unchanged.
- DR-002: Classify device-specific DCA versus unsupported general ETS app/legacy package.
- DR-003: Match declared application IDs to the selected device and keep different DCA versions side by side.
- DR-004: Require explicit user trust/provisioning status before execution; distinguish integrity from publisher authenticity.
- DR-005: On non-Windows report inspectable but original-execution-unavailable.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reference DCA registers with exact manifest identity and immutable bytes.
- [ ] AC-002: A mismatching application, changed hash or unsupported package type cannot execute.
- [ ] AC-003: Package import alone does not mark DCA functions supported.

## Dependencies to Other Tasks

- [Task-006](Task-006.md): Inspect the original MDT DCA package without executing it. Required predecessor behavior/evidence must be accepted.
- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.
- [Task-053](Task-053.md): Publish per-product compatibility diagnostics. Required predecessor behavior/evidence must be accepted.
- [Task-029](Task-029.md): Create the production Rust workspace with boundary checks. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: manifest mapping and case-safe assembly lookup.
- TEST-002: Negative: tampered content, wrong application, legacy/non-DCA package and unsigned/untrusted status.
- TEST-003: Cross-platform: inspect package without executing vendor code.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / DCA registry**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automatic store download, vendor binary redistribution or cross-platform original execution claim.

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
