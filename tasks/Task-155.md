# Task-155: Qualify a connected application-version read

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC input qualification |
| Component | knust-fork / read-only management example |
| Dependencies | Task-018, Task-019, Task-151 |
| Required phase gate | None |

Extend the existing read-only connected-property harness with an explicit
object-type precondition, keeping all selectors and expected values caller
supplied. A matching descriptor and an exactly matching two-byte object type
must be observed in the same device session before the selected property read.
Any mismatch, denial, malformed response or cancellation must prevent that
selected read and proceed only with documented cleanup. No write service or
automatic object discovery belongs in this harness.

Use primary KNX Resources v01.10.01 §§4.2.1 and 4.2.13 for property types and
Profiles v02.01.01 A.2.6 for the selected mask. The candidate master data maps
an Application Program object and its five-byte version property, but that
mapping must be explicitly supplied and verified at runtime. A version value
does not prove unique physical identity, the exact imported product file or
the ETS fingerprint.

## Tests and acceptance

- Offline deterministic peers exercise matching descriptor/type/value, wrong
  descriptor or type with zero selected-property requests, denial, malformed
  or truncated responses, cancellation and cleanup failure.
- The default remains offline. Any live observation requires the already
  coordinated gateway/target, no competing commissioning session, explicit
  execution arguments and recorded device/gateway cleanup. User has authorized
  reads; no additional device write is permitted by this task.
- Record exact source commit, command, primary-source locators, platform,
  result and limits; independent review and applicable format/Clippy checks are
  required. Proprietary inputs and actual household configuration remain
  private. No gate or product/download compatibility claim is implied.

## Completion record

The read-only harness now accepts an optional same-session object-type
precondition after descriptor validation. Wrong type, denial, malformed
response or cancellation prevent the selected property request. Its default
remains offline; no write service was added. Independent review and 13 owned
loopback tests passed, along with focused Clippy, formatting and diff checks.
The local unpublished fork commit is
`6acedecebc5ae3b7f5dec07a9afb97a30cd3cc02`; details and commands are in
`TASK155-OBJECT-TYPE-PRECONDITION.md` at that commit. A sandbox exception was
needed for owned UDP loopback tests.

No live version read has been performed. Fingerprint `9BDB`, installed
application and unique device identity remain unverified. The live read
requires a coordinated idle bus and remains pending.

### Coordinated live read — 2026-11-13

After Simon confirmed the bus was idle, the read-only harness was executed
once from macOS arm64 through gateway `192.168.1.50:3671` for target `1.1.231`, using local
fork commit `3ebe3d979f61c699283c1f7ac07c7de920eb2c3e`. It observed mask
`0705`, object index 3 type 3, and five bytes for property 13. It reported
device cleanup `LOCAL_NETWORK_CONFIRMED` and gateway cleanup `REMOTE_CONFIRMED`;
no device writes occurred. Private stdout
SHA-256 is `3f6d06b88ab37df58fec2c2df28485d443a9596d333f5baed99461dd8c8e48c1`;
stderr was empty.

The built `readonly_connected_property` example used
`--execute 192.168.1.50:3671 --target 1.1.231 --expected-mask 0705
--object-index 3 --property-id 13 --start-index 1 --count 1 --element-size 5
--expected-object-type 3 --non-secure`. It was built locked and offline with
`--no-default-features --features dpt`; exit status was zero.

Resources v01.10.01 §4.2.13.1.2 defines the returned value as two-byte
manufacturer, two-byte device type and one-byte application version in
MSB-first order. The observed identifier `0x0083 / 0x0090 / 0x13` and mask `0705`
match the Task154 candidate application's declared manufacturer, application
number and version. This does not prove the exact archive, installed image,
hardware variant, serial identity or ETS fingerprint `9BDB`. Historical
NOT RUN statements above describe the earlier checkpoint. Final task acceptance
remains in review; no full-download or phase-gate claim is made.
