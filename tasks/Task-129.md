# Task-129: Publish the approved stable release and archive evidence

## Subject

Publish the approved stable release and archive evidence

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-129 |
| Status | Not started |
| Stage | Release |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / authorized release publication |
| Owner | Unassigned |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Publish only the exact approved artifacts and retain their development/qualification record.

## Detailed Requirements

- DR-001: Use maintainer-authorized remote destinations and immutable version tags/releases.
- DR-002: Publish core/optional-host packages, checksums, notices/SBOM, support matrix and English release/recovery notes.
- DR-003: Reference the exact pinned fork release/commit and preserve separate repository histories.
- DR-004: Keep private hardware/artifact evidence private while publishing redacted reproducibility metadata.
- DR-005: Never overwrite an existing stable release or claim KNX certification/ETS parity.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Published bytes match the final signed-off hashes and version.
- [ ] AC-002: All required public documents and platform distinctions are present.
- [ ] AC-003: Evidence is archived with stable references and private inputs remain private.

## Dependencies to Other Tasks

- [Task-128](Task-128.md): Obtain final release sign-off for scope and evidence. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-RC = GO ([Task-125](Task-125.md)); Task-129 additionally requires G-RELEASE = GO (Task-128).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Post-publication: download/verify checksums from the actual authorized destination.
- TEST-002: Audit: tags point to the approved commits.
- TEST-003: Privacy: inspect release assets and notes for secrets/proprietary fixtures.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / authorized release publication**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No unauthorized push, release creation, marketing expansion or new code.

## Risks and Blockers

Publishing is an external action and requires explicit maintainer authorization and actual repository access.

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
