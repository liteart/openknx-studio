# Task-157: Correct extended cEMI frame layout

| Field | Value |
|---|---|
| Status | In progress |
| Stage | PoC protocol correction |
| Component | knust-fork / cEMI codec |
| Dependencies | Task-016; related: Task-021 |
| Required phase gate | None |

Investigate and correct the generic cEMI extended-frame layout against the
supplied primary cEMI specification, without adding product-specific behavior.
An independent inspection of the private ETS observation found that the fork
appears to insert an extra byte before the source address for extended frames.
This can misidentify addresses and obstruct interpretation of the reference
download. Do not treat packet observations alone as the protocol specification.

Acceptance requires verified standard and extended frame vectors, round-trip
and malformed-input tests, preservation of standard-frame behavior, and
independent review. Record exact fork commits, commands and source locators.
No hardware operation, capture publication or fork remote push is authorized
by this task. The existing user-authorized device scope remains unchanged.

## Completion record

Investigation and correction in progress. The private observation is recorded
in Task-021; no corrected fork or hardware qualification is yet claimed.
