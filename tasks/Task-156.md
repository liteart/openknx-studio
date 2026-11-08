# Task-156: Qualify a Windows operation-journal boundary

| Field | Value |
|---|---|
| Status | Ready |
| Stage | PoC platform qualification |
| Component | knx-editor / guarded operation persistence |
| Dependencies | Task-003, Task-151; Task020 guard model as a test contract |
| Required phase gate | None; no gate is advanced |

Task152's experimental Windows journal persists a development passkey receipt;
it does not implement the separate Task020 operation journal or its reusable
gateway-route lock. Develop a Windows-capable, bounded persistence boundary
that preserves the Task020 guard's exact-plan, one-use and unknown-outcome
invariants. Work is offline and simulator-only; it grants no device authority.

## Requirements

- Preserve durable intent and one-use consumption before a dispatch can be
  selected. Incomplete, malformed, failed or interrupted operations recover as
  unknown and quarantine their route; never retry or restore automatically.
- Serialize cooperating processes for a route. A process crash may release an
  OS mutex but must not silently release persisted active or unknown state.
  Only verified restoration and confirmed device and gateway cleanup may
  release the route, and a failed persistence step must not report release.
- Keep all native APIs behind a non-default experimental feature until native
  Windows tests pass. The feature is not an authorization boundary. Avoid
  project-owned unsafe code where a reviewed safe wrapper is sufficient; any
  required unsafe must meet AGENTS.md's test and human-review requirements.
- Keep the existing Unix journal behavior intact. Do not widen the fixed
  Task153 plan, add an approval issuer, access KNX hardware, or treat a local
  same-user store or mutex as protection from arbitrary same-user code.
- Pin and review any dependencies, preserve bounds, and record the platform,
  exact commits and command results. A cross-check or fake store is not a
  native Windows crash or durability test.

## Acceptance and tests

- Deterministic fault tests cover each persistence boundary, replay, mismatched
  plan, cancellation, crash/reopen, cleanup failure and route exclusivity.
- Native Windows tests verify concurrent processes, durable state after process
  termination, failed/partial records and route release; power-loss limits and
  same-account interference are explicit. Windows provider and actual hardware
  remain separately qualified.
- macOS/Linux regressions and Windows cross-checks preserve existing behavior;
  independent review checks the real implementation and evidence before Done.

## Completion record

Not started. Windows registry/mutex operation-journal implementation and
native tests are NOT RUN. The experimental Task152 receipt journal is not
evidence that this operation journal exists.
