# Task-159: Integrate reviewed KNX management and cEMI work

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC protocol integration |
| Component | knust-fork |
| Dependencies | Task-016, Task-019, Task-151; related: Task-021, Task-155, Task-157 |
| Required phase gate | None |

Create one local fork revision combining the reviewed Task021 management
primitives with the reviewed Task155 read harness and Task157 cEMI correction.
The work currently exists in separate local histories. Preserve each history,
keep generic protocol code in the fork, and do not publish the fork or add
product-specific behavior.

Verify the combined revision with focused cEMI, property, memory, load-state,
restart and connected-session tests, including the explicitly opted-in
experimental management write feature where required. Secure stays disabled.
Record exact parent and integration revisions, commands and independent review.
No hardware operation or application download is included.

## Completion record

Local fork merge `8d827644a93d5cf548885449879742290c5d2271` combines parents
`9d479c8d160830c84e9b54a0e8563f5a0ea5d4d5` and
`3ebe3d979f61c699283c1f7ac07c7de920eb2c3e`; evidence commit
`b86c9268f5f5f3b8e21e006862c266c6f7f45d1f` adds the detailed test report.
The fork remains unpublished and is not yet a product dependency.

Offline checks passed: 159 protocol unit tests, ten cEMI integration tests,
nine property-read and nine property-write tests, 18 management-session
tests and 13 read-example tests. With experimental management writes opted
in, 33 session tests passed, including memory, allocation, load and restart.
Clippy in both configurations, all-target DPT and feature-empty checks,
compile-fail doctest, formatting and diff checks also passed. Secure was not
enabled. An independent reviewer verified the merged parent paths and reran
the 159 protocol and ten cEMI tests. Commands and limitations are in
`TASK159-INTEGRATION.md` at the fork evidence commit. Windows, hardware,
product integration and full-download qualification remain NOT RUN.
Final acceptance remains in review.
