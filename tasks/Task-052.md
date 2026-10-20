# Task-052: Pin product versions and preview explicit application upgrades

## Subject

Pin product versions and preview explicit application upgrades

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-052 |
| Status | Not started |
| Stage | Alpha |
| Priority within stage | P1 |
| Component / Git project | knx-editor / catalog lifecycle |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md) |

## Description

Prevent catalog updates from silently changing existing device configurations.

## Detailed Requirements

- DR-001: Device instances reference immutable product/application hashes and versions.
- DR-002: Importing another version creates another catalog entry without mutating projects.
- DR-003: Provide a comparison/upgrade preview with required parameter/object mapping, lost values and unsupported changes.
- DR-004: Apply only a reviewed explicit changeset; leave unresolved migrations blocked.
- DR-005: Never perform a device download or firmware update as part of catalog import/upgrade.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Existing project/compiler inputs remain unchanged after importing a newer package.
- [ ] AC-002: An incompatible upgrade produces a precise preview and no mutation.
- [ ] AC-003: A qualified simple upgrade can be accepted through the normal changeset transaction.

## Dependencies to Other Tasks

- [Task-034](Task-034.md): Implement immutable artifacts and versioned opaque state blobs. Required predecessor behavior/evidence must be accepted.
- [Task-046](Task-046.md): Resolve product hardware and application catalog relationships. Required predecessor behavior/evidence must be accepted.
- [Task-051](Task-051.md): Support qualified module and repeated-channel instances. Required predecessor behavior/evidence must be accepted.
- [Task-036](Task-036.md): Implement isolated changesets and optimistic revision checks. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-POC = GO ([Task-028](Task-028.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: import two versions and compare existing snapshot hashes.
- TEST-002: Unit: stable mapping/lost-parameter diagnostics.
- TEST-003: Negative: unresolved object mapping and implicit upgrade attempt.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / catalog lifecycle**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No automatic arbitrary-version migration or claim that application upgrade equals firmware update.

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
