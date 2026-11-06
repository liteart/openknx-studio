# Task-018: Read and identify the reference device through KNX management

## Subject

Read and identify the reference device through KNX management

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-018 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / device management |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Read the identity information actually supported by the reference device before any write experiments.

## Detailed Requirements

- DR-001: Use documented management services for descriptor/mask/application or serial identity where available.
- DR-002: Return typed values and capability/unknown markers rather than inventing absent fields.
- DR-003: Address only the selected device and record gateway/target identities.
- DR-004: Bound operations and distinguish transport failure from unsupported identity property.
- DR-005: Expose a public generic API with no product-file dependency.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Returned supported identity fields match the registered physical fixture.
- [ ] AC-002: Unsupported identity fields remain explicit unknown/unsupported.
- [ ] AC-003: No device write occurs.

## Dependencies to Other Tasks

- [Task-017](Task-017.md): Implement and verify minimum non-secure tunneling. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit/simulator: decode valid/invalid identity responses and device-not-found.
- TEST-002: Hardware read-only: compare observed identity with the fixture label/documentation.
- TEST-003: Negative: mismatch is surfaced to the caller.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / device management**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No inference that identity readability proves programming compatibility.

## Risks and Blockers

The physical identity comparison requires a specifically selected device,
expected independently sourced identity, gateway/target identities and a
separately coordinated read-only operation. Simon deferred work with the
production ETS project and its full-device write/capture. Unsupported or
undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; selected one-shot descriptor read implemented and reviewed offline; hardware identity comparison pending |
| Source commit(s) | Local knust fork `322f02fbeeee6e5c9a85646c9640deb9562da84a`, based on Task017 `c62a89ac03005e415bcba72c19d6242ebc683c4d`; fork not published |
| Tests executed | 15 synthetic loopback tests PASS; focused Clippy, formatting and diff checks PASS; physical device identity read NOT RUN |
| Evidence location | Fork `TASK018-IDENTITY.md` at `322f02fbeeee6e5c9a85646c9640deb9562da84a`; checkpoint below |
| Acceptance criteria met | AC-001 pending physical reference comparison; AC-002/003 supported by synthetic tests only, not a hardware claim |
| Technical outcome | Selected DD0 mask version can be requested and compared against independently supplied evidence; unavailable, denied, mismatched and timed-out outcomes remain distinct |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent code and evidence review PASS for bounded offline increment; full task acceptance pending |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Bounded offline descriptor increment — 2026-11-06

The local fork exposes a one-shot, individually addressed connectionless DD0
read over a selected connected UDP tunnel. It validates the gateway ACK and
selected response, bounds work and timeout, and preserves unknown/unsupported
results instead of inventing identity values. The caller must exclusively own
and retire the tunnel after the exchange, including cancellation: reuse can
accept a late same-target response and is not a proven freshness guarantee.
This is an offline PoC component, not a production management session.

On macOS arm64 with the pinned toolchain, `cargo test --locked --offline
--no-default-features --features dpt --test test_device_identity` passed 15
synthetic loopback tests, including response/ACK ordering and correlation,
wrong source/destination, deadlines, cancellation and fixture mismatch. The
implementer also ran focused Clippy with warnings denied, formatting and diff
checks successfully; the coordinator independently reran all 15 tests. The
reviewer checked the standard locators, codec use and tests, with no remaining
demonstrated must-fix finding within this bounded scope. No KNX device read,
write, production ETS project access or Windows test was performed.
