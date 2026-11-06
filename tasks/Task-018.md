# Task-018: Read and identify the reference device through KNX management

## Subject

Read and identify the reference device through KNX management

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-018 |
| Status | Done |
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

- [x] AC-001: Returned supported identity fields match the registered physical fixture.
- [x] AC-002: Unsupported identity fields remain explicit unknown/unsupported.
- [x] AC-003: No device write occurs.

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

The physical identity comparison used independently sourced expected mask
data and a bounded read-only operation. Simon selected a specific device
and supplied gateway, individual address, model and ETS application reference;
these household-specific details are recorded privately rather than published.
The selected DD0 operation reports a mask version and does not itself verify
the supplied application version or fingerprint. Simon deferred work with the
production ETS project and its full-device write/capture. Unsupported or
undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Done for the selected read-only DD0 mask comparison; wider product/application qualification remains outside this result |
| Source commit(s) | Local knust fork `322f02fbeeee6e5c9a85646c9640deb9562da84a`, `e73c435c05a4305062bce239fb8f03632c82ade4` and tested harness `2f51b48f96a9de2734f368f259c03c8bec5f2399`; fork not published |
| Tests executed | 15 synthetic descriptor loopback tests, seven gateway-address tests and seven lifecycle-harness tests PASS; focused Clippy, formatting and diff checks PASS; one authorized physical DD0 read and independent mask comparison PASS |
| Evidence location | Private `task018-device-read-20261106/` and `task018-reference-device-20261106.json`; fork `TASK018-IDENTITY.md` and checkpoints below |
| Acceptance criteria met | AC-001 for the supported DD0 mask field; AC-002 and AC-003 within the selected read-only scope |
| Technical outcome | Selected device returned DD0 mask `0x0705`, matching the independent manufacturer product archive; channel cleanup confirmed. Installed application version, fingerprint and unique physical identity are not established. |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent code, hardware-result hashes and manufacturer archive mapping review PASS for the bounded Task018 scope |

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

## Selected device and gateway address — 2026-11-06

Simon supplied a specific target, model, ETS application version and fingerprint;
the unverified user-supplied reference is preserved in private
`task018-reference-device-20261106.json`. No device access follows merely from
recording these values. A separately reviewed local fork increment
`e73c435c05a4305062bce239fb8f03632c82ade4` exposes the address reported by the
successful gateway connection response without enabling address-management
fallback probing. Four unit and three synthetic loopback tests passed; focused
Clippy, formatting and diff checks passed. The gateway-provided address does
not prove uniqueness or the target's identity. At that checkpoint, actual device
identification and hardware comparison had not yet run; see the following result.

## Authorized physical mask comparison — 2026-11-06

After Simon selected and authorized the reference device, the independently
reviewed `readonly_device_identity --execute <gateway> --target <individual-address>
--non-secure` harness was run once using the exact private plan in
`private/evidence/task018-device-read-20261106/plan.json`. On macOS arm64 it
returned a DD0 mask of `0x0705`, exit 0, and confirmed disconnect of its own
channel. No device write or retry was performed. The plan SHA-256 is
`f9a5d404bea4c07a0f92ad9b7bb8afa0b7b3e0fe96be6d5a0c2e40fc5ded2141`, binary
SHA-256 is `dab385e561e6189ad935f776479efdcae4eba232b6610ef2905b673a8df2011b`,
and stdout SHA-256 is
`edcd6001fdc75ad646b6c70863fa6eea9511505e49004ea2cd9551e93c576675`.

The independent private manufacturer archive has SHA-256
`c8feaa000ca891df90b455a4ae27de9e1c0e3d3e5cf53e63cf944807f755c7e9`.
Its selected product maps through hardware `M-0083_H-142-1` to application
`M-0083_A-0090-13-D12F`, declaring mask `MV-0705`. This matches the observed
DD0 field. The independently reviewed mapping and hashes are recorded in the
private `independent-mask-comparison.json`, SHA-256
`4ca20964e63d3d15eb1158ac1476dcca4803a6af5f6a37e846a174e8e505f531`. The
archive itself is not redistributed. This comparison does not prove a unique
physical model, installed application version, fingerprint, programming
compatibility or safety of any subsequent device write.
