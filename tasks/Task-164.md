# Task-164: Prepare a bounded read-only memory snapshot

| Field | Value |
|---|---|
| Status | In review |
| Stage | PoC reference observation |
| Component | knust-fork / read-only management example |
| Dependencies | Task-018, Task-019, Task-151; related: Task-021, Task-158 |
| Required phase gate | None |

Provide an offline-default, explicitly selected read-only memory diagnostic
using the fork's public typed API. A single device session must validate the
caller-supplied mask and application identifier before reading an explicitly
supplied bounded range in standard-frame chunks. Reject range overflow,
unexpected identity, malformed responses and excess work; stop on the first
error, report device and gateway cleanup accurately, and never dispatch a
write or silently retry uncertain operations.

Use deterministic peer tests for the selected read path and negative cases.
Record exact revisions, primary-source locators, limits and independent review.
This task prepares the example only: no hardware read, device snapshot,
product-specific memory map, full-download claim or fork publication is
implied. Any later real observation remains explicitly coordinated and private.

## Completion record

The offline-default example is committed in local unpublished fork
`cff2be7d7a80097e51eb9ee498efbab3128b4c7e`, based on
`bc8133e9912ea31bfabf5921fd3dd43cf9c51f55`. Eleven deterministic tests,
focused Clippy, formatting and offline-default execution passed. An independent
reviewer repeated all eleven tests and checked the identity, range, failure
and cleanup boundaries. Commands and limits are recorded in
`TASK164-READONLY-MEMORY.md` at that commit.

Hardware, Windows, writes and full-download qualification remain NOT RUN.
Final acceptance is in review; the fork has not been published.
