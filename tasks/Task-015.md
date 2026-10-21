# Task-015: Audit the pinned knust fork against the PoC operations

## Subject

Audit the pinned knust fork against the PoC operations

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-015 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / protocol audit |
| Owner | Protocol audit agent; independently verified and reviewed |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Determine what the actual selected knust revision already provides and what must be implemented for the non-secure experiments.

## Detailed Requirements

- DR-001: Inspect public APIs/tests for discovery, tunneling, identity, properties, memory and management sessions.
- DR-002: Create a capability map of present-tested, present-unverified and absent behavior.
- DR-003: Record the exact feature set that excludes Secure; do not rely solely on README claims.
- DR-004: Identify protocol documentation/allowed captures required for missing operations.
- DR-005: Keep product-specific memory layouts out of the audit implementation.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: The gap list names concrete public APIs/tests or missing services.
- [x] AC-002: The PoC dependency graph uses the pinned fork revision.
- [x] AC-003: No Secure work is added to fill an unrelated requirement.

## Dependencies to Other Tasks

- [Task-002](Task-002.md): Create two isolated repositories and preserve licenses. Required predecessor behavior/evidence must be accepted.
- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build/test: run existing non-secure tests with documented flags.
- TEST-002: Static: inspect feature graph and public exports.
- TEST-003: Review: compare capability claims with actual test coverage.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / protocol audit**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No wholesale protocol rewrite, full device support claim, or product import code.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Done; bounded audit independently verified and reviewed PASS |
| Source commit(s) | Audited fork `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e`; upstream `287bd5c94644688ac2eb93bf09f10a37bc509e0c`; reviewed fork audit documentation `7e44751fb6681812fcb6690d8159a9b8c5c3e816`; editor audit/scope record `678fc7f0b8f0b0b6b2250361341c6ed0bf03c7f8` |
| Tests executed | TEST-001 selected offline build/test execution PASS (75 runner passes; inherited sequence-consistency property assertion Result discarded); TEST-002 static inspection PASS with Secure feature/runtime paths disabled and inherited source preserved; TEST-003 coverage comparison and independent critical review PASS |
| Evidence location | Separate fork `AUDIT.md`, `BUILDING.md`; [editor summary and pinned dependency map](../docs/KNUST-AUDIT.md) |
| Acceptance criteria met | AC-001 concrete public APIs/tests and absent services mapped; AC-002 exact audit source pinned in dependency map; AC-003 no Secure implementation added. Independent verification/review PASS; ordinary audit criteria met |
| Technical outcome | Audit findings recorded; offline selected checks PASS with documented ineffective sequence-consistency property assertions; inherited Secure source presence permitted by 2026-10-21 operator clarification; Secure runtime/hardware and actual gateway/device operations NOT RUN |
| Gate outcome, when applicable | No phase gate required or advanced; no hardware authorization granted |
| Reviewer | Project reviewer PASS: source/API/test-body and verifier-log comparison, hashes, feature metadata, source integrity, scope and diff checks; no Cargo rerun by reviewer |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Audit handoff, 2026-10-21

Readiness: Tasks-002/004 are accepted; Task-003's final closure records Done and
verification/review PASS at evidence `a764f260a66db52d19b921c9beab0ba94665e00f`.
This worktree starts from editor `be5b4090740151ecf7cd0b3e66d9a7f445a863ac`; no phase gate is required for this
static/offline task. Missing actual bench/device/grants do not become satisfied
because the inventory or audit is complete.

The pinned fork provides group communication, UDP/TCP tunnel code, descriptor
and TPCI codecs, and an occupancy probe. It does not provide typed generic
PropertyValue/PropertyDescription, device memory/restart/load-state operations,
or a reusable device management session. Probe bool/timeout and a gateway's
identity are insufficient target identity evidence. Missing services stay in
fork Tasks-016 through 021; no editor/DCA protocol fallback was added.

The audit selected only inspected offline test bodies; the runner reported
75 passing cases on macOS 27.0 arm64 with pinned Rust/Cargo 1.99.1. It excluded inherited tests that can
contact gateways, initialize multicast or exercise Secure session codecs.
Full command arguments, feature evidence, counts, source hashes, provisioning
failures and limits are in the fork report. No proprietary fixture/capture,
real KNX packets, hardware test, publication or remote push was used.

Final review identified an inherited false-positive coverage gap in
`tests/test_sequence_validation.rs:262`:
`property_tests::property_sequence_validation_consistency` discards the async
block's `Result` using `let _`. Failed `prop_assert_*` checks at lines 273–285
return an error that never fails the outer test. The observed 75 runner passes
remain accurate, but this property is not verified by its reported PASS.
Separate deterministic sequence cases remain bounded evidence. The fork audit
and editor summary record the limitation; no source fix or additional task
implementation is hidden in Task-015. Future reliance on that property needs a
separately reviewed test correction.

Operator clarification, 2026-10-21: preserve inherited Secure source; v1 does
not use Secure, with possible later-version use to be evaluated separately.
DR-003 concerns disabled feature/runtime use, not source or public-type removal.
`--no-default-features --features dpt` disables the Secure feature and six
optional crypto dependencies. Inherited Secure message codecs/configuration
symbols compile unconditionally; their presence is permitted and is not a
blocker or a source-removal task. Source inspection finds explicit
unsupported/configuration errors for Secure requests; no silent plaintext
downgrade is permitted. Those rejection paths were inspected statically, not
runtime-tested here. All Secure tests/runtime/hardware qualification remain
NOT RUN. No Secure source was removed, enabled or changed. This audit makes no
license/right decision or gate approval.

Independent verification reproduced the selected 75 runner passes. Final
critical review PASS includes the documented inherited property-test limitation.
The fork audit documentation was committed first at `7e44751fb6681812fcb6690d8159a9b8c5c3e816`.
No phase gate was advanced. Next task: Task-016, subject to its own verified
reference inputs; no downstream acceptance or scope expansion is implied here.
