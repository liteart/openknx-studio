# Task-051: Support qualified module and repeated-channel instances

## Subject

Support qualified module and repeated-channel instances

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-051 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / configuration engine |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-020](../docs/REQUIREMENTS.md) |

## Description

Handle the repeated structures needed by the qualification corpus without flattening away instance identity.

## Detailed Requirements

- DR-001: Implement supported module/channel instantiation rules, instance counts and scoped parameter/object references.
- DR-002: Derive offsets/object indices only from verified schema/profile evidence.
- DR-003: Preserve stable instance identity through permitted count changes and report removed dependencies explicitly.
- DR-004: Check product limits and memory/object range collisions.
- DR-005: Unsupported module constructs block configuration/compilation rather than using guessed repetition.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A multi-channel fixture resolves distinct correct parameters and endpoints per instance.
- [ ] AC-002: Changing count cannot silently repoint an existing signal to a different channel.
- [ ] AC-003: Out-of-range or overlapping instances are rejected.

## Dependencies to Other Tasks

- [Task-049](Task-049.md): Evaluate dynamic parameter conditions deterministically. Required predecessor behavior/evidence must be accepted.
- [Task-050](Task-050.md): Resolve effective communication objects and associations. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: one/multiple/maximum instances and index boundaries.
- TEST-002: Integration: remove an instance with existing graph memberships and require explicit resolution.
- TEST-003: Negative: overlapping offsets, unsupported module and duplicate instance identity.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / configuration engine**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No universal recursive module engine or implicit migration of existing installations.

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
