# Task-162: Determine the qualified path APDU bound

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC protocol capability qualification |
| Component | knust-fork / management transport |
| Dependencies | Task-151; related: Task-021, Task-160, Task-161 |
| Required phase gate | None |

Provide a typed, bounded calculation of the KNX APDU length supported by a
caller-supplied complete route: interface, target and every intervening router.
Use the primary Configuration Procedures v02.01.01 §2.6 and relevant
Resources definitions. Missing or invalid evidence must not enable extended
traffic; distinguish an explicitly empty router path from an unknown path.
Do not infer topology, contact hardware, change transport limits or grant
device-write authority. Keep the observed ETS trace separate from normative
rules and product-specific decisions.

Acceptance requires boundary, byte-order, incomplete-route and conservative
router tests; accurate handling of cEMI length versus reconstructed APDU
length; independent review; and recorded source and tested fork revisions.
No hardware test, Secure activation or fork publication is included.

## Completion record

Local unpublished fork commit `f9f7354429387be3e899203431e57cc13fa4049c`,
based on `152e1f0f4a56be93babf361d9dd8672f31b1a900`, adds the bounded
calculation without activating transport. Seven offline tests passed for
limits, missing and unknown routes, conservative router masks, byte order,
resource bounds and the cEMI length conversion. Focused Clippy, DPT
all-target, formatting and diff checks passed. An independent reviewer
repeated the seven tests and verified the primary rules. Exact commands,
sources and the documented 14/15 terminology discrepancy are in
`TASK162-PATH-CAPACITY.md` at the fork commit.

Runtime route binding, long-frame session support, Windows, hardware and the
complete download remain unqualified. Final acceptance is in review and the
fork is not published.

### Device capacity observation — 2026-11-13

Under the owner's read authorization, the coordinated `readonly_connected_property`
example from local fork `f9441237b878b48c4423076044c50a80ec0f4202` ran once
on macOS arm64 via `192.168.1.50:3671` to `1.1.231`. It checked mask `0705`
and device object index 0/type 0, then read PID56 index 1, one two-byte value.
The result `0x0037` is 55 in the Resources §4.3.7 unsigned big-endian format.
Device cleanup was `LOCAL_NETWORK_CONFIRMED`; gateway cleanup was
`REMOTE_CONFIRMED`; no device write occurred. Private stdout SHA-256 is
`1c533124e3204d63a15bfe52540866794e184abfe5219549acb1b748e59312d6`;
stderr was empty. Independent review verified the source, result and semantics.
The gateway separately advertised 240 in Task161. Unknown router and route
limits prevent claiming a usable end-to-end limit or enabling long frames.
