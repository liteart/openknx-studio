# Task-045: Introduce versioned product schema adapters and normalized IR

## Subject

Introduce versioned product schema adapters and normalized IR

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-045 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / product-engine schema layer |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md) |

## Description

Make product-format support explicit and versioned rather than treating all KNXPROD namespaces as interchangeable.

## Detailed Requirements

- DR-001: Dispatch by actual schema namespace/version and selected application with an explicit adapter registry.
- DR-002: Normalize only implemented constructs into a versioned intermediate representation.
- DR-003: Preserve source locations and unknown constructs for diagnostics; critical unknown semantics block configuration/compile capability.
- DR-004: Separate harmless retained resources from unsupported executable/dynamic behavior.
- DR-005: Pin parser/IR versions in catalog cache keys and compiler provenance.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Supported fixtures select the correct adapter and stable IR version.
- [ ] AC-002: Unknown critical constructs cannot silently disappear.
- [ ] AC-003: An unsupported format remains inspectable as unsupported without being called programmable.

## Dependencies to Other Tasks

- [Task-044](Task-044.md): Harden KNXPROD archive and XML ingestion. Required predecessor behavior/evidence must be accepted.
- [Task-032](Task-032.md): Define versioned project and entity JSON schemas. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: adapter selection and critical/noncritical classification.
- TEST-002: Golden: normalized IR for reference packages.
- TEST-003: Negative: namespace mismatch, unsupported major version and missing mandatory element.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / product-engine schema layer**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No universal schema coverage, migration guessed from similar XML, or embedded script execution.

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
