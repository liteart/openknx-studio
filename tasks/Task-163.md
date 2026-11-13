# Task-163: Encode bounded classic long memory blocks

| Field | Value |
|---|---|
| Status | In progress |
| Stage | PoC protocol codec |
| Component | knust-fork / classic memory services |
| Dependencies | Task-016, Task-019, Task-151; related: Task-021, Task-160, Task-162 |
| Required phase gate | None |

Implement typed classic A_Memory_Read, A_Memory_Write and matching response
encoding for caller-selected blocks beyond the existing standard-frame limit,
using the verified six-bit count and 16-bit address in the supplied primary
application-layer specification. Preserve existing standard-only behavior and
do not add MemoryExtended services, session dispatch, automatic splitting,
retries or product-specific memory addresses.

The owner-authorized ETS observation contains classic memory writes with up to
52 data bytes and matching service-internal responses. That is useful evidence
of the selected device's observed traffic, not proof of a complete download or
authorization to write. An APDU for a 52-byte classic write has cEMI length
55; runtime use still requires a qualified end-to-end path and separate guarded
execution.

Acceptance requires normative wire vectors, 1/12/13/52/63 and overflow
boundaries, address overflow, malformed and mismatched response tests,
preservation of existing standard-session restrictions, offline checks and
independent review. No hardware contact, Secure activation or fork publication
is included.

## Completion record

In progress. No long-frame session dispatch or full download is claimed.
