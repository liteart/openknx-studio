# Task-019: Implement minimum generic KNX property reads

## Subject

Implement minimum generic KNX property reads

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-019 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / property services |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Read a documented property and element range from the selected device using a generic public service.

## Detailed Requirements

- DR-001: Take explicit interface/object, property identifier, start index, count and destination as typed arguments.
- DR-002: Derive wire representation from verified protocol evidence; keep manufacturer selection outside the fork.
- DR-003: Validate range/count/length limits and return exact raw property data plus structured errors.
- DR-004: Distinguish timeout and malformed replies; preserve unsupported/denied/other remote failures as ambiguous when the selected response does not distinguish them, as decided in [ADR-019](../docs/ADR-019-classic-property-failure.md).
- DR-005: Do not silently retry indefinitely.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: The selected property reads consistently and matches expected fixture data.
- [x] AC-002: Invalid ranges fail before a packet is sent where locally detectable.
- [x] AC-003: The service contains no MDT-specific property policy.

## Dependencies to Other Tasks

- [Task-016](Task-016.md): Build a tiny deterministic KNX management peer simulator. Required predecessor behavior/evidence must be accepted.
- [Task-018](Task-018.md): Read and identify the reference device through KNX management. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: boundary/range and payload-length cases.
- TEST-002: Simulator: valid read, denied/unsupported property, truncated reply, timeout.
- TEST-003: Hardware read-only: read and record the authorized property.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / property services**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No arbitrary device scanning or property brute force.

## Risks and Blockers

The selected classic property response uses the same failure form for denied,
unsupported and other remote problems. Following the owner's decision in
ADR-019, the bounded implementation reports an ambiguous remote failure rather
than guessing its cause. Unsupported or undocumented behavior must be recorded
and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Done for the selected classic read and owner-approved ambiguous-failure semantics in ADR-019 |
| Source commit(s) | Local knust fork `2bb954fd876a9b5e4dfd14889cf332a733b722b6` and tested harness `e1d3d9b0ad7ec0edad6b4c82782cd3148ff65d56`, based on Task018 `2f51b48f96a9de2734f368f259c03c8bec5f2399`; fork not published |
| Tests executed | Four codec unit, nine synthetic property loopback and nine harness tests PASS; focused Clippy, formatting and diff checks PASS; two independently initiated physical property reads and reference comparison PASS |
| Evidence location | Fork `TASK019-PROPERTY-READ.md`; private `task019-manufacturer-read-20261106/`, `task019-manufacturer-repeat-20261106/` and Task018 fixture evidence |
| Acceptance criteria met | AC-001 for the selected manufacturer property; AC-002 and AC-003 supported by offline tests and review; DR-004 as revised by ADR-019 |
| Technical outcome | Selected device returned the expected manufacturer ID after a fresh mask check; both own-channel disconnects confirmed. Timeout and malformed responses are distinct; classic denied/unsupported causes are not distinguishable. |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent protocol, harness, reference, both hardware results and completion record reviews PASS |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Bounded offline property read — 2026-11-06

The fork implements a one-shot individually addressed classic property read
using verified KNX application-layer encodings and exact caller-provided
element-size metadata. It uses no device writes, scans, automatic retries or
manufacturer-specific property policy. The caller must own and retire the
tunnel after each attempt, including cancellation; reuse does not prove response
freshness. The selected classic failure response does not identify whether an
unavailable property, access denial or another remote cause occurred, and the
API returns this ambiguity explicitly.

On macOS arm64, `cargo test --locked --offline --no-default-features --features
dpt --lib protocol::property` passed four unit tests and `cargo test --locked
--offline --no-default-features --features dpt --test test_property_read` passed
nine synthetic loopback tests. Focused Clippy, formatting and diff checks also
passed. The coordinator independently reran the nine loopback tests, and a
separate reviewer checked primary-source locators and code. No property read was
sent to a physical device and no Windows test was run.

## Selected manufacturer property read — 2026-11-06

Simon selected the reference device and authorized its use. A one-shot harness
at fork revision `e1d3d9b0ad7ec0edad6b4c82782cd3148ff65d56` was reviewed and
tested with nine synthetic loopback lifecycle cases. The exact user-specific
gateway and device addresses are recorded only in the private execution plan.
The plan first re-read DD0 and required the independently matched mask
`0x0705` and confirmed own-channel cleanup. It then issued one property read
for Device Object index 0, manufacturer ID PID 12, start index 1, count 1 and
two-byte element size. The observed raw value `0083` matched the independent
manufacturer identifier in the reference product archive; cleanup was
confirmed and no device writes or retries occurred.

Primary references are KNX Resources v01.10.01 §4.2.12 p33 (PID 12 and PDT),
Management Procedures v02.01.02 §3.4.3 p74 (selectors), Standard Identifier
Tables v01.04.01 §4.1 p18 and §4.2 p20 (two-octet unsigned integer), and
Interworking Model v02.01.01 §3.4.8.2.2 p49 (octet order). The selectors are
also present for mask `MV-0705` in the matching private product archive's
master data. The actual operation was a standalone connectionless read, not the
complete connection-oriented management procedure illustrated by that source.

The private execution plan SHA-256 is
`75a73f0a9c1ac411d40a60249e0c0b6aa31301fca05825801c27e3868b225462`; the
property harness SHA-256 is
`d531e981ddb15f40371f1ac9cbd7ea54786bd3df4afdc8f8aedf83d06c91f815` and the
property stdout SHA-256 is
`e4f6b1af22a4baec59dfa5f51be3ac29bc7af5053aa902316d56cee45a576c7e`.
An independently initiated second run first repeated the same successful mask
check, then returned the same `0083` property value and confirmed both own-channel
disconnects. Its private plan SHA-256 is
`02d17d178008e3a3aaad366c35b2da26b89c1903f5d3ca10cc8b3e63ffda9808`; its property
stdout SHA-256 matches the first run. No write or automatic retry occurred.
This result does not verify installed application version or fingerprint, nor
does it establish that a subsequent write is safe. Classic denial versus
unsupported remains indistinguishable from the selected wire response.
