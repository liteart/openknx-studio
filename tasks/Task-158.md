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
