# Task-019: Implement minimum generic KNX property reads

## Subject

Implement minimum generic KNX property reads

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-019 |
| Status | In progress |
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
- DR-004: Distinguish unsupported property, denied access, timeout and malformed reply.
- DR-005: Do not silently retry indefinitely.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The selected property reads consistently and matches expected fixture data.
- [ ] AC-002: Invalid ranges fail before a packet is sent where locally detectable.
- [ ] AC-003: The service contains no MDT-specific property policy.

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
unsupported and other remote problems. The bounded implementation reports an
ambiguous remote failure; DR-004 is not fully met without further evidenced
means to distinguish the causes or an explicit requirement decision. A real
read also needs a specifically selected, documented property and element range.
Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; bounded generic classic read implemented and independently reviewed offline; DR-004 distinction and hardware test remain open |
| Source commit(s) | Local knust fork `2bb954fd876a9b5e4dfd14889cf332a733b722b6`, based on Task018 `2f51b48f96a9de2734f368f259c03c8bec5f2399`; fork not published |
| Tests executed | Four codec unit and nine synthetic loopback tests PASS; focused Clippy, formatting and diff checks PASS; physical property read NOT RUN |
| Evidence location | Fork `TASK019-PROPERTY-READ.md` at `2bb954fd876a9b5e4dfd14889cf332a733b722b6` |
| Acceptance criteria met | AC-002 and AC-003 supported by offline tests and review; AC-001 pending selected fixture and hardware read |
| Technical outcome | Typed selected property read checks range, payload length, response and gateway acknowledgment. Timeout and malformed responses are distinct; classic denied/unsupported causes are not distinguishable. |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent protocol and code review PASS for bounded offline increment; full task acceptance pending |

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
