# Task-046: Resolve product hardware and application catalog relationships

## Subject

Resolve product hardware and application catalog relationships

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-046 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / product catalog |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md) |

## Description

Represent the actual selectable product/hardware/application combinations without conflating marketing labels with identity.

## Detailed Requirements

- DR-001: Read the supported schema relationships between manufacturer, product variant, hardware and application programs.
- DR-002: Preserve immutable IDs, revision identifiers, mask/profile metadata and original display translations.
- DR-003: Reject unresolved/multiple ambiguous application selections rather than using the first XML record.
- DR-004: Return searchable catalog entries with explicit required DCA references and source provenance.
- DR-005: Do not change a device instance when a newer catalog package arrives.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The fixture product selects the intended hardware/application by exact identity.
- [ ] AC-002: Duplicate labels do not merge distinct versions.
- [ ] AC-003: Missing/mismatched references prevent a deployable catalog entry.

## Dependencies to Other Tasks

- [Task-045](Task-045.md): Introduce versioned product schema adapters and normalized IR. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: shared application/multiple hardware relationships.
- TEST-002: Fixture: match each selected variant against manifest/product evidence.
- TEST-003: Negative: duplicate IDs and ambiguous application mapping.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / product catalog**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automatic product upgrade, manufacturer lookup API or label-based guessing.

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
