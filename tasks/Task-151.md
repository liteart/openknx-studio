# Task-151: Implement bounded connected KNX management sessions

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-151 |
| Status | Ready |
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

- [ ] AC-001: Simulator tests demonstrate correct connect, exchange and close,
  including target responsiveness and independent transport/tunnel ACKs.
- [ ] AC-002: Missing confirmations, wrong sequence, duplicate frames, NAK,
  timeout and cancellation produce bounded, explicit outcomes.
- [ ] AC-003: Uncertain write dispatch cannot silently become an automatic write
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
| Implementation / report status | Ready; no implementation or test run yet |
| Source commit(s) | Not created |
| Tests executed | NOT RUN |
| Evidence location | Private Task020 device-control read and supplied KNX standard |
| Acceptance criteria met | None |
| Reviewer | Unassigned |
