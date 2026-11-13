# Task-158: Qualify the selected Glass Lite download profile

| Field | Value |
|---|---|
| Status | In progress |
| Stage | PoC input qualification |
| Component | knx-editor / product-download compiler |
| Dependencies | Task-154 |
| Required phase gate | None |

Establish the product-specific evidence needed to compile and load the
selected Glass Lite project/11 application. Investigate the absent RAM
payloads, allocation/property mapping, property comparison representation,
and conditional or module activation. Distinguish values present in the
registered manufacturer input from independently observed ETS behavior and
from generic KNX protocol rules. Preserve conflicting or incomplete evidence;
do not guess executable bytes or steps.

The owner-authorized ETS observation in Task-021 is incomplete and cannot by
itself establish a full download sequence. Task-155's matched application
identifier and mask narrow identity uncertainty but do not prove the exact
archive, configuration, hardware variant or fingerprint.

Acceptance requires reviewed primary or independent reference evidence for
the selected profile, reproducible offline input hashes, explicit unsupported
cases, and sufficient qualified information for Task-024's image work and
Task-021's applicable primitive selection. This task creates no device write,
does not inspect the owner's deferred production project, and does not publish
manufacturer inputs or private captures.

## Completion record

Investigation in progress. No complete image, load plan or independent device
download is claimed.

The registered archive and selected application member were rechecked against
their Task154 SHA-256 pins. The two RAM segment declarations have no `Data`
payload; the input does not establish whether they are generated or filled.
The load procedure's property-78 comparison contains ten bytes, whereas
Resources v01.10.01 §4.3.28 defines `PID_HARDWARE_TYPE` as six bytes. The
relationship remains unresolved; no truncation is assumed. The procedure
also uses allocation checksum flag `0x80`, whose property-based semantics
remain unqualified as documented in Task-021. These findings are structural
evidence only, not downloadable bytes or an executable plan.

An independently produced ETS reference for the exact selected application in
a separate scratch project is the next candidate for resolving these gaps.
Its ability to expose the needed compiled data has not been established. The
owner's production project remains deferred and outside this investigation.

The owner-authorized ETS capture was subsequently inspected offline against
the corrected cEMI layout and primary application-layer definitions. It
contains 20 positive classic MemoryWrite confirmations to the selected device
and 20 matching MemoryResponses, with exact address, length and data equality
in capture order. The observed write counts are 1, 10, 15, sixteen blocks of
52, and 30 bytes. This supports the observed traffic and service-internal
responses, not independent readback or a complete transaction trace.

The observed writes cover all 364 bytes of candidate segment AS-4400 and all
498 bytes of AS-456C, but only 11 of 513 bytes of AS-4000 and 15 of 511 bytes
of AS-4201. No direct writes to the two RAM segments were identified. The two
complete observed segment snapshots differ from the manufacturer archive
baselines; their actual contents and configuration remain private. These
observations do not resolve RAM omission, profile activation or checksum
semantics. Independent review reproduced the pair and coverage counts; no
physical operation was performed during analysis.

A further offline count of positive link confirmations in the same pinned
capture found four property-based absolute data-allocation events with memory
attribute bit 7 set, and one with it clear. One stack-allocation event had the
bit clear. The decoder used the ten-byte load-event layout in Management
Procedures v02.01.02, pages 139–140, and selected property 5 writes to the
observed target. This shows that ETS sent those event payloads on this run;
link confirmation alone does not show device acceptance, checksum behavior,
or justify enabling this attribute in an independent download.
Reproduce with `python3 private/evidence/task158-trace-reference-20261113/count_allocations.py`;
the private script SHA-256 is
`148055325229183a8e72e290bc52f5b389c1605741b5d8bb9151a295eb176680`.
For each of these six confirmations the next observed PID5 response with the
same object and selector returned `02`; Resources v01.10.01 defines load
state 2 as `Loading` (page 294). The separate pinned-capture check
`python3 private/evidence/task158-trace-reference-20261113/count_allocation_responses.py`
(SHA-256 `6907f8993b1d2c71480606a4515e5df98edd8ee915baf1bef1d7f15f0c3d5b67`)
reproduces the ordered associations. This is evidence of a subsequent state
response, not proof that the checksum was calculated or verified; the capture
still lacks the original outbound ETS packets and a complete procedure trace.

Two observed PID78 reads and their matching six-byte responses have identical
values. Those bytes equal the first six bytes of the candidate's ten-byte
inline comparison data. Independent review confirmed the selectors and prefix
equality. The remaining four bytes and ETS comparison semantics are still
unknown; this observation does not justify truncation.

Further primary-source review narrows the uncertainty. Configuration
Procedures Management v02.01.01 maps `LdCtrlCompareProp` to
`DM_InterfaceObjectVerify` (page 73, load-control implementation); Management
Procedures v02.01.02 §3.26 describes reading the selected property and
comparing the returned value with the supplied data. A mismatch or missing
response is an error. The project/22 schema defines `InlineData` as
`xs:hexBinary` without a length restriction and defaults `Count` to one.
Neither source establishes how ETS handles this candidate's ten-byte value
against the six-byte property response. The prefix match therefore remains
INCONCLUSIVE, and no executable comparison rule follows from it.

The observation was reproduced and saved as private, pinned reference
artifacts under `private/evidence/task158-trace-reference-20261113/`. The
offline extractor SHA-256 is
`68c34ba53b311d03cf62614773fda062395f969f6816bc88031c2ec464df0fc6`;
its report SHA-256 is
`b0666516fc75aeae203ee426b229d90638d1c985587dd293b85083ef4d406cbe`.
An independent reviewer re-parsed the capture and verified the source pins,
pairing, coverage, private artifacts and stated limitations. This is a bounded
reference observation, not a general packet-parser qualification or complete
download image. No manufacturer input or actual configuration was published.

## Coordinated read-only snapshot — 2026-11-13

After the owner confirmed the bus was free, the Task164 diagnostic from local
fork `cff2be7d7a80097e51eb9ee498efbab3128b4c7e` read the concrete address
range `4000`–`43FF` on `1.1.231` through the selected gateway. Mask `0705`,
application object type 3 and program identifier `0083009013` matched before
the memory requests. All 1,024 bytes were read in 86 standard-frame blocks;
device cleanup was locally confirmed and gateway cleanup remotely confirmed.
No writes were performed. The private stdout SHA-256 is
`843b460077b6bd83ee67ab8fe49cf6a33a290a6e7966e00e86c91431bb24927e`;
the example binary SHA-256 is
`c24fd88c4030cbd3de76715ed434e8b28521b5d8f39aab5536ed55121435a5ad`.

All 11 bytes previously observed in candidate segment AS-4000 and all 15 in
AS-4201 matched the corresponding snapshot offsets. This is a current raw
memory observation and partial cross-check against the ETS trace, not proof
that the full range is the correct compiled image, writable, dynamically
allocated as assumed, or sufficient for a download. Data remains private.
