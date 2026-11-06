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
- [Task-151](Task-151.md): Provide the connected device transport session required by the selected Verify Mode candidate before that candidate can be written.

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
On 2026-11-06 the user explicitly permitted writes to the selected device only,
prohibited writes to any other device, and stated that its ETS state is saved
and can be restored. This standing target-scoped authorization is recorded
privately. A verified property, original/desired values and a concrete
property-specific operation and recovery plan must still be established. The
offline primitive does not implement the backend approval/identity guard.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Blocked on exact reversible property, scoped write plan and backend authorization/identity enforcement; generic low-level increment reviewed offline |
| Source commit(s) | Local knust fork `da9a66679d6d49108f97ec8afeaed8d56c382a63`, based on Task019 `e1d3d9b0ad7ec0edad6b4c82782cd3148ff65d56`; editor offline guard model `8c06571`; fork not published |
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

## Optional description property observation — 2026-11-06

Following Simon's target-scoped write authorization, an optional standard
Device Object description property was investigated as a possible reversible
candidate. The KNX profile lists the property as optional and its actual access
is implementation-dependent; the product archive does not establish that this
device implements it. A read-only attempt first confirmed the selected target's
DD0 mask and then requested the property's array count. The device returned the
classic ambiguous remote-failure response; own-channel cleanup was confirmed.
This does not distinguish unavailable property, access denial or another
remote problem. No property write was sent, and this result does not authorize
guessing a value or assuming that the property can be restored.

The exact private plan and result are in
`private/evidence/task020-description-discovery-20261106/`; the plan SHA-256 is
`08cca38c330e7684080d0a91cec08c91744397b67a6d4ea83860f3e259f93d49` and the
property stdout SHA-256 is
`532b2e36619899c6782dbd03b3103850b3de7d1009de88c3f260e5cacc9acf46`. The
standing user authorization is preserved separately in private
`task020-user-target-grant-20261106.json`. A supported writable property and
documented restoration procedure remain required before a physical write.

## Device-control property observation — 2026-11-06

The selected KNX profile documents Device Object `PID_DEVICE_CONTROL` as a
possible writable property for the observed mask. A separately bounded
read-only operation first rechecked the selected target's DD0 mask and then
read object 0, PID 14, element 1 as one byte. It returned `00`; both own-channel
disconnects were confirmed. No device write or retry occurred. The private
plan is `task020-device-control-read-20261106/plan.json`, SHA-256
`4a861b267406593462c953b902a6d2bb3ae0c62f6d00dd6a9e8bf0e3925595d6`;
property stdout SHA-256 is
`4ff87eb22455f9b6b657994f7587131726f1cc170d2ff6e1ffa5a5cbaf6cabce`.

This read does not prove write access or safe restoration. The Verify Mode bit
requires a device transport-layer connection according to KNX Resources
v01.10.01 §4.2.14.7; the current generic write primitive is connectionless.
A KNXnet/IP tunnel alone is not that device transport connection. No Verify
Mode write has been sent, and the candidate requires further protocol and
recovery review before any such operation.

Subsequently, the separately reviewed Task151 connected-session harness read
the same control byte (`00`) after a matching connected DD0 check. Device-session
teardown was locally network-confirmed and gateway teardown remotely confirmed;
no device write was sent. Private evidence is under
`task151-connected-read-20261106/`. A non-executable private draft for a
possible `00`→`04`→`00` Verify Mode test has SHA-256
`add2f653dd4f98b2e11de427f69f5f5114e6d2875ee401ff625c835322bf7612`.
The draft is not an execution capability: exact-plan enforcement, durable
intent, recovery review and physical readback remain required.

## Offline exact-plan guard model — 2026-11-06

The user explicitly requested the selected Verify Mode write/restore test.
An editor-owned offline model binds immutable plan and target, requires a
trusted approval and fresh observed identity/value, persists intent before
dispatch selection, quarantines an unresolved bus path and requires separate
post-write and post-restore observations. Synthetic approval and observation
constructors exist only in tests. Missing real approval or observation
watermarks fail closed. The model contains no executable device adapter and
cannot itself send a KNX packet or authorize this test.

Independent review found mutable dispatch and stale-readback risks; both were
corrected and re-reviewed. `cargo test --locked --test task020-guard` passed
18 offline tests, independently repeated on main. Full workspace tests, Clippy,
formatting and scope checks passed in the task worktree. The reviewed model
source SHA-256 is
`f668820bf4b2dfe8f2a12f8aa17236cadd9fe23c176ea33445afdf5a1258b1ee`.
Trusted human approval and authenticated observation adapters, a dispatcher
using the pinned fork API, power-loss/Windows durability qualification, the
actual device write and restoration are still NOT RUN or unavailable. User
consent is already recorded; no repeated consent is implied by these technical
gaps.

## Offline dispatcher model — 2026-11-06

A separately reviewed editor-side offline dispatcher model consumes an opaque
plan-bound selection and checks route, selector, payload and stage before a
mocked typed-session dispatch. It does not mint real approval or contact
hardware. Uncertain dispatch never triggers automatic retry or restoration.
After successful restore readback, route quarantine remains until both device
and gateway cleanup are confirmed and durable cleanup evidence is recorded.
Failure, pending or cancelled cleanup retains quarantine, including after
reopening the journal.

Independent review found and verified the correction of an earlier premature
quarantine release. `cargo test --locked --test task020-guard` passed 31 offline
tests, independently repeated by the reviewer. Full workspace tests, Clippy,
formatting and diff checks passed in the task worktree. Editor source commit is
`228cb75`; dispatcher SHA-256 is
`44f3ebc689741e0899d9211c598c846883b636321471cfa618ab87965b65a160`.
A separate non-shipping adapter draft compiled against the local fork, but did
not demonstrate confirmed gateway cleanup. Real trusted approval, authenticated
observations, complete concrete adapter integration and the physical write and
restoration remain unavailable or NOT RUN.
