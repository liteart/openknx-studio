# Task-161: Read the KNXnet/IP tunnel APDU capacity

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC protocol capability discovery |
| Component | knust-fork / KNXnet/IP tunnel |
| Dependencies | Task-017, Task-151; related: Task-021, Task-160 |
| Required phase gate | None |

Add a bounded, typed, read-only Tunnelling Feature GET for the maximum APDU
length of an established KNXnet/IP tunnel, based on the supplied primary
standard. Validate channel, sequence, feature identifier, response length,
return code, acknowledgement and deadlines. No feature SET, bus write,
manufacturer-specific behavior or automatic long-frame activation belongs
in this increment.

The user-authorized ETS observation includes extended frames involving the
selected device, but its missing request direction does not establish their
full operation sequence. A gateway's advertised bound alone is insufficient
to enable long transfers: device and intervening-router limits must also be
known. Unknown capacity remains conservative.

Acceptance requires primary-source wire vectors, deterministic peer tests for
success, mismatch, malformed reply, timeout and cleanup, explicit unsupported
errors, default-offline behavior and independent review. Record source, exact
fork revision and checks. No hardware contact or fork publication is included.

## Completion record

The read-only Feature07 codec and owning query are committed in local
unpublished fork `152e1f0f4a56be93babf361d9dd8672f31b1a900`, based on
`1275bacee12c087dc2a6d0335aedd862c023d58e`. Interleaved traffic returns an
explicit error and prevents normal tunnel reuse; failures and cancellation
retain only explicit disconnect cleanup. No long-frame limit is increased.

Eleven synthetic UDP peer tests and two codec tests passed; 23 connected
management regression tests and 161 protocol unit tests passed, as did focused
Clippy, feature-empty library and DPT all-target checks, formatting and diff
checks. An independent reviewer repeated the 11 peer and two codec tests;
its sequence-ordering finding was fixed and rechecked. Exact commands and
standard references are in `TASK161-TUNNEL-CAPACITY.md` at the fork commit.
Gateway compatibility, Windows, long APDUs and a full download remain NOT RUN;
final acceptance is in review and the fork is not published.

### Gateway observation — 2026-11-13

The reviewed offline-default example was committed in local unpublished fork
`f9441237b878b48c4423076044c50a80ec0f4202`, based on Task162's
`f9f7354429387be3e899203431e57cc13fa4049c`. Six example tests and focused
Clippy passed; independent review repeated all six tests. With the owner's
gateway-read authorization, the example was run once on macOS arm64 using
`--execute 192.168.1.50:3671 --non-secure`. The gateway returned Feature07
maximum APDU length 240; the gateway disconnect received remote confirmation.
No device operation was requested. Private stdout SHA-256 is
`b85622caff1526c6fa3e4a3ffdfc37f323f3bff79b7c1a6baf9716076198c769`; stderr
was empty. An independent reviewer checked the result and source semantics.
The earlier gateway NOT RUN statement is historical. This qualifies only the
observed gateway response, not the target, router path or long-frame support.
