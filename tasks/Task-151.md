# Task-151: Implement bounded connected KNX management sessions

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-151 |
| Status | Done |
| Stage | PoC dependency |
| Component / Git project | knust-fork / KNX transport |
| Owner | Primary coordinator and protocol agent |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description and requirements

Implement the smallest generic, bounded point-to-point transport session needed
for connected management. A KNXnet/IP tunnel is not a device transport session.
The fork owns framing, sequence handling, acknowledgments, timers and session
lifecycle; it must not select manufacturer properties or grant write approval.

- DR-001: Address one explicitly selected client and target over an already
  selected tunnel; retain exclusive ownership of the receive path.
- DR-002: Establish and close a device transport session using verified KNX
  semantics, distinguishing local network confirmation, target acknowledgments
  and application responses. Do not equate a tunnel ACK with device connect.
- DR-003: Bound time, queues and retries; handle sequence counters, duplicates,
  NAK, cancellation and uncertain outcomes without inventing success.
- DR-004: Expose typed generic operations without manufacturer policy, hidden
  discovery, implicit device writes or automatic replay of uncertain writes.
- DR-005: Preserve cleanup evidence and fail closed on malformed or mismatched
  source, destination, channel, sequence and transport frames.

## Acceptance criteria

- [x] AC-001: Simulator tests demonstrate correct connect, exchange and close,
  including target responsiveness and independent transport/tunnel ACKs.
- [x] AC-002: Missing confirmations, wrong sequence, duplicate frames, NAK,
  timeout and cancellation produce bounded, explicit outcomes.
- [x] AC-003: Uncertain write dispatch cannot silently become an automatic write
  replay; the component grants no human authorization.

## Dependencies and tests

Accepted [Task-016](Task-016.md), [Task-017](Task-017.md) and
[Task-019](Task-019.md); no phase gate applies. Use original primary KNX
Transport Layer v01.02.03 §§3.7, 3.9 and 5 and cEMI evidence, recording exact
locators and hashes. Test only owned synthetic loopback peers by default,
including normal lifecycle, confirmation loss, wrong address/channel/sequence,
duplicates, target ACK/NAK, deadline, cancellation and teardown. Never contact
physical hardware or write a device under this task without separate scoped
authorization. Preserve Secure exclusion and upstream license notices.

## Deliverables and completion

Provide reviewed generic fork code, focused reproducible tests and a completion
record with exact commits, source hashes, commands and limitations. A real
device write, property policy, durable backend approval and Task020 completion
are not implied by passing simulator tests.

## Evidence and reason for dependency

On 2026-11-06, the selected device's standard control property was read
successfully. KNX Resources v01.10.01 §4.2.14.7 requires a device transport
connection for its Verify Mode bit. The current Task020 writer is explicitly
connectionless, and the older address probe lacks the target ACK/session
lifecycle. The supplied Transport Layer v01.02.03, SHA-256
`4ec1e952d0d9bc75a624467f781e17d01e764e90768a814af7b5084523354d44`, defines
the necessary confirmation, sequence and timer behavior. No hardware write has
been performed.

| Field | Current value |
|---|---|
| Implementation / report status | Done for bounded generic offline session behavior; physical execution and backend authorization remain separate |
| Source commit(s) | Local knust fork `7ebcdbfc8c69dd86c2db7b0adcc3b2cf3b4b0855` and read-only harness `ae1ef36eb190b40a1fab02f7d673e12e9b2d9757`, based on Task020 `da9a66679d6d49108f97ec8afeaed8d56c382a63`; fork not published |
| Tests executed | 14 session and nine read-harness synthetic loopback tests PASS; focused Clippy, formatting and diff checks PASS; one selected physical connected read PASS; Windows NOT RUN |
| Evidence location | Fork `TASK151-CONNECTED-MANAGEMENT.md` at source commit; private Task020 device-control read and supplied KNX standard |
| Acceptance criteria met | AC-001–003 for the documented offline scope |
| Reviewer | Independent protocol and code review PASS after correcting idle timing and error-state handling |

## Bounded session result — 2026-11-06

The fork supplies a generic exclusively owned connected management session,
distinguishing tunnel receipts, local network confirmation, target transport
acknowledgments and application responses. It bounds sequences, timeouts,
receive work and supported read retries. A potentially dispatched write is
latched as uncertain on cancellation or error and is not automatically
replayed. A connected descriptor response is required before a write; the
component does not grant or enforce a human write authorization.

On macOS arm64 with the pinned offline toolchain, `cargo test --locked --offline
--no-default-features --features dpt --test test_management_session` passed
14 synthetic loopback tests, independently repeated by the coordinator.
Focused Clippy, formatting and diff checks passed. Independent review found and
verified fixes for an idle clock extended by delayed gateway receipts and for
property decode errors leaving the session reusable. Source SHA-256 is
`372657ddcaf703678f331e3332f0d7599bf0d9e248a64691e815893a819d16d6`; test
source SHA-256 is
`da629186cecb35e3329c44034dfe757e508b13427b488b2fd57d6c9b33dfbb00`.
At this checkpoint no live session, physical write, device readback or backend
authorization test had been performed. In particular this result does not
complete Task020.

## Connected read-only hardware observation — 2026-11-06

The separately reviewed `readonly_connected_property` harness at local fork
revision `ae1ef36eb190b40a1fab02f7d673e12e9b2d9757` passed nine synthetic
loopback tests and then ran once against the user-selected device under the
private plan `task151-connected-read-20261106/plan.json`. It read a matching
DD0 mask and the selected one-byte control property (`00`), with exit 0. Device
session teardown was locally network-confirmed; gateway teardown was remotely
confirmed. No write was sent. The plan SHA-256 is
`098e4a9823ba84348a8510af852661cb96cd21252dcea8f78c0437ca5c4bf768`, binary
SHA-256 is `975a293cbd2f58050902497d15bbe374544b95078e1d44001d151e7e033aa154`,
and stdout SHA-256 is
`0fe052938dc7cab832bb4e4bce3deeaf1236605849f7a77b1af50fe7b1c2d9f7`.
Independent review verified the source and private evidence hashes. This is a
read-only session observation, not evidence of unique identity, write access,
durable authorization or physical restoration.
