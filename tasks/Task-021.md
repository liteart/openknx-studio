# Task-021: Add only the selected profile memory and load-state primitives

## Subject

Add only the selected profile memory and load-state primitives

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-021 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / low-level management |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Implement the generic low-level services required by the one chosen application download profile.

## Detailed Requirements

- DR-001: Identify the needed memory/property/load-state services from the legal reference evidence and selected product.
- DR-002: Keep addresses, segment layout and ordering in the editor test input, not hard-coded for MDT in the fork.
- DR-003: Implement payload bounds, response/error validation, and documented connection/session requirements.
- DR-004: Mark destructive primitives as unavailable to casual examples; expose them only through the controlled test flow.
- DR-005: No general firmware protocol is implied.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Each required primitive has a deterministic simulator test and public typed API.
- [ ] AC-002: Product-specific values occur only in private caller fixtures.
- [ ] AC-003: Unsupported/ambiguous service behavior blocks the download PoC.

## Dependencies to Other Tasks

- [Task-015](Task-015.md): Audit the pinned knust fork against the PoC operations. Required predecessor behavior/evidence must be accepted.
- [Task-016](Task-016.md): Build a tiny deterministic KNX management peer simulator. Required predecessor behavior/evidence must be accepted.
- [Task-018](Task-018.md): Read and identify the reference device through KNX management. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: encode/decode boundary vectors from verified evidence.
- TEST-002: Simulator: invalid load state, denied memory operation, partial transfer and timeout.
- TEST-003: Static: confirm no KNXPROD parser or device-profile table was added to the fork.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / low-level management**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No product load-procedure interpreter, universal memory support, or real destructive write outside Task-027.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; generic reads, writes, allocation controls and basic restart reviewed; exact profile completeness remains unqualified |
| Source commit(s) | Local unpublished fork `776ac0545780c578801ff48221aa877014b3d5ab`, based on `fcf3af5e669eca9fc4661106c17308e1bd983499` and prior Task021 increments |
| Tests executed | Basic restart codec and 33 connected-session loopback tests, both focused Clippy configurations, feature-off doctest, formatting and diff checks PASS. Independent review repeated the restart codec and five focused loopback tests; earlier allocation and partial-effect results remain recorded below. Hardware and Windows NOT RUN |
| Evidence location | Fork `TASK021-BASIC-RESTART.md`, `TASK021-ABSOLUTE-ALLOCATION.md`, `TASK021-SIMULATOR-COMPLETENESS.md` and preceding Task021 reports at the source commits above |
| Acceptance criteria met | AC-001 supported for implemented generic reads and writes, but the complete profile-required primitive set remains unqualified; AC-002 and the local fail-closed portion of AC-003 supported by independent audit |
| Technical outcome | Typed reads and opt-in one-attempt writes reject invalid replies and retain uncertainty. Verify-mode responses are distinguished from independent readback; target ACK without Verify Mode is explicitly unverified. Feature opt-in is not operation authorization |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent protocol and simulator reviews PASS for the bounded increments and partial-effect test; full task acceptance remains open |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

The Glass Lite candidate's declared load sequence requires absolute data,
stack and task allocation controls. Their generic codecs and opt-in
one-attempt session operations are now covered by simulator tests; checksum
attributes other than disabled remain unsupported. The caller must supply
qualified PEI/application identity and ranges. The candidate's omitted task
fields and its property-comparison representation remain unqualified. Basic
restart now implements one attempt, own disconnect and six-second retirement;
its acknowledgement is transport evidence, not proof of device restart. A
dropped session still needs caller-owned durable quarantine and wait tracking.
No System B or IP example is assumed to define
the candidate's mask `0705` procedure; physical application matching and
missing RAM payload semantics remain open.
