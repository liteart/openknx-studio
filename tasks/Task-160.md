# Task-160: Accept bounded short extended management frames

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC protocol correction |
| Component | knust-fork / connected management session |
| Dependencies | Task-151; related: Task-021, Task-157, Task-159 |
| Required phase gate | None |

Investigate the connected-session rejection of extended cEMI frames whose
payload fits the existing standard-frame bound. Implement only the tolerance
required by verified KNX cEMI rules, preserving source, destination,
sequence, confirmation, acknowledgement and length checks. The user-approved
ETS observation includes extended frames, but does not by itself establish
which long-frame services the selected download requires.

Use supplied primary specifications and deterministic simulator tests for
accepted, mismatched, malformed and oversized cases. Do not enable long APDUs,
infer negotiated limits, add product-specific policy, or perform hardware
operations in this task. Record exact local fork revision, checks and review.

## Completion record

Local unpublished fork commit `1275bacee12c087dc2a6d0335aedd862c023d58e`,
based on integrated fork `b86c9268f5f5f3b8e21e006862c266c6f7f45d1f`, accepts
short extended EFF=0 frames within the existing length bound. Existing
address, sequence and confirmation checks remain; outbound framing and long
APDU limits are unchanged.

Five new synthetic tests passed; the full session suites passed with 23 tests
feature-off and 38 with experimental management writes enabled. Focused
Clippy in both configurations, formatting and diff checks passed. An
independent reviewer repeated the five new loopback tests and checked supplied
cEMI v01.04.02 §4.1.5.2.3. Exact commands and limitations are recorded in
`TASK160-SHORT-EXTENDED-RECEIVE.md` at the fork commit. No hardware, Windows,
long-frame or full-download qualification is claimed. Final acceptance remains
in review; the fork is not published.
