# Task-157: Correct extended cEMI frame layout

| Field | Value |
|---|---|
| Status | In review |
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

The generic correction is committed in the local unpublished fork at
`3ebe3d979f61c699283c1f7ac07c7de920eb2c3e`, based on
`6acedecebc5ae3b7f5dec07a9afb97a30cd3cc02`. It removes the spurious extra
octet, retains EFF in Ctrl2 and uses the shared cEMI parser in the address
probe. The pre-release `ExtendedControlField` API now exposes EFF only;
address type and hop count are owned by `ControlField`.

Six layout, four control-frame, nine cEMI and ten address-probe tests passed
offline; focused Clippy, all-target DPT check, formatting and diff checks
passed. An independent reviewer repeated the focused tests and checked the
layout against supplied cEMI v01.04.02 §§4.1.5.3.2–5. Exact commands and
limitations are in `TASK157-CEMI-EXTENDED-LAYOUT.md` at the fork commit.
The fork has not been published or integrated into a product dependency.
Hardware, Windows, a full download and maintainer acceptance remain NOT RUN.
