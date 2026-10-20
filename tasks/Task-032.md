# Task-032: Define versioned project and entity JSON schemas

## Subject

Define versioned project and entity JSON schemas

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-032 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / domain schemas |
| Owner | Unassigned |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md) |

## Description

Establish stable identities and explicit serialized contracts for the v1 engineering model.

## Detailed Requirements

- DR-001: Define schemas for project, topology, devices, signals, functions, allocations, product/extension locks and DCA state envelopes as specified in DATA-MODEL.
- DR-002: Use opaque UUIDs for local identity and dedicated original-source IDs.
- DR-003: Represent explicit/default parameter assignments and module/object identities without labels or addresses as keys.
- DR-004: Specify schema majors, permitted extension namespaces, required fields and unknown-field behavior.
- DR-005: Keep locale/display preferences separate from execution-affecting values.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Minimal valid fixtures for every entity pass schema validation.
- [ ] AC-002: Renaming/readdressing an entity does not change its identity or filename.
- [ ] AC-003: Unsupported schema majors and duplicate keys are rejected without rewriting input.

## Dependencies to Other Tasks

- [Task-029](Task-029.md): Create the production Rust workspace with boundary checks. Required predecessor behavior/evidence must be accepted.
- [Task-030](Task-030.md): Define stable errors and diagnostic event envelopes. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/schema: valid/minimal/missing/extra-field fixtures.
- TEST-002: Property: serialize/deserialize stable IDs and references.
- TEST-003: Negative: duplicate keys, malformed IDs and unsupported major version.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / domain schemas**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No cache schema, file writes, UI layout schema, or migration of unknown future versions.

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
