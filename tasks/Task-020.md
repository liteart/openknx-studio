# Task-020: Implement an explicitly authorized reversible property write

## Subject

Implement an explicitly authorized reversible property write

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-020 |
| Status | Blocked |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / property services |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Add the generic write service and demonstrate one reversible write only after the exact property is approved.

## Detailed Requirements

- DR-001: Implement bounded typed property writes with correct response/error interpretation.
- DR-002: The caller provides the lab-approved property and original/desired values; the fork must not choose writable properties.
- DR-003: Read back the changed value and restore/read back the original value in the test procedure.
- DR-004: Reject ambiguous target identity, unsupported access and oversized payloads.
- DR-005: Do not replay an uncertain write automatically unless protocol semantics establish safety.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The physical reference property changes as expected and is restored with read-back evidence.
- [ ] AC-002: The API remains manufacturer-independent.
- [ ] AC-003: Failure or uncertain completion is reported accurately and cannot become an automatic next write.

## Dependencies to Other Tasks

- [Task-019](Task-019.md): Implement minimum generic KNX property reads. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Simulator: success, denial, dropped acknowledgement and malformed reply.
- TEST-002: Hardware authorized: read/change/verify/restore/verify.
- TEST-003: Negative: wrong target/property authorization prevents the test operation.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / property services**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Contains explicit real-device qualification or execution-path tests. Physical writes require a separately recorded human lab grant, exact target identity and recovery plan; defaults remain offline/read-only.

## Out of Scope

No speculative memory edits, factory reset, or full application-download claim.

## Risks and Blockers

A genuinely reversible documented property and explicit operator approval are mandatory. If none is available, stop rather than selecting a plausible property ID.
The user has permitted changes to the selected device and offered later ETS
reprogramming, but no exact documented reversible property, original/desired
values and property-specific recovery procedure have been identified. The
offline primitive does not implement the backend approval/identity guard.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Blocked on exact reversible property, scoped write plan and backend authorization/identity enforcement; generic low-level increment reviewed offline |
| Source commit(s) | Local knust fork `da9a66679d6d49108f97ec8afeaed8d56c382a63`, based on Task019 `e1d3d9b0ad7ec0edad6b4c82782cd3148ff65d56`; fork not published |
| Tests executed | Seven property codec regression tests, nine synthetic loopback write tests, focused Clippy, formatting and diff checks PASS; hardware writes/readback and backend authorization negative NOT RUN |
| Evidence location | Fork `TASK020-PROPERTY-WRITE.md` at `da9a66679d6d49108f97ec8afeaed8d56c382a63`; checkpoint below |
| Acceptance criteria met | AC-002 supported by offline review; AC-001 and AC-003 remain pending physical readback/restore and durable authorization/uncertainty enforcement |
| Technical outcome | One-dispatch low-level write attempt reports uncertainty after possible dispatch and does not automatically replay; this does not authorize any real write or prove restoration |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent protocol, safety and framing review PASS for the bounded offline increment; full task acceptance pending |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Bounded offline write primitive — 2026-11-06

The generic fork primitive accepts explicit typed selectors and opaque caller
bytes, sends at most one request per in-memory attempt and latches an unknown
outcome before dispatch can be polled. Cancellation, lost acknowledgments and
malformed or ambiguous replies do not trigger an automatic replay. An exact
response is a protocol observation, not independent readback or restoration.
Caller-owned durable approval, target verification, cleanup and recovery remain
unimplemented. A fresh attempt or process restart is not prevented by the
in-memory latch and must not be treated as authorization to retry.

On macOS arm64, seven property codec regression tests and nine synthetic
loopback write tests passed with the pinned offline toolchain. The coordinator
independently reran `cargo test --locked --offline --no-default-features
--features dpt --test test_property_write` (nine PASS). Focused Clippy,
formatting and diff checks passed. Independent review found a standard-frame
length issue; the corrected implementation rejects cEMI length above 15 before
dispatch and tests both sides of the 10-byte data boundary. Re-review passed.
No physical device write, hardware readback, restore or Windows test was run.
