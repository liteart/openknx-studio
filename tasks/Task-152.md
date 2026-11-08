# Task-152: Prototype plan-bound phone passkey approval

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-152 |
| Status | In progress |
| Stage | PoC dependency |
| Component / Git project | knx-editor / human approval prototype |
| Owner | Primary coordinator and editor implementer |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Context and owner direction

On 2026-11-06 Simon requested a low-effort reusable PIN-based approval for
the selected physical write and later applicable tasks. Simon has a smartphone
supporting passkeys and device PIN or biometric verification; no separate
hardware security key is available. The user's device-write consent and the
actual technical approval mechanism are distinct. Never request or record the
PIN in chat, arguments, environment variables, logs, workspace or project data.
Simon subsequently selected the integrated macOS passkey vault and requested
support for Windows and Linux. Use platform-neutral WebAuthn and compatible
platform or browser providers; actual platform behavior must be tested.

## Requirements

- DR-001: Prototype a reusable, operator-controlled approval path using a
  reviewed standard passkey implementation with user verification. Bind an
  unpredictable, short-lived, one-use challenge to the exact immutable plan,
  target, effect, issuer and expiry; reject changes, replay and expiry.
- DR-002: Verify assertion signature, challenge, relying-party identity, exact
  origin, user-verification flags and enrollment against an independently
  protected trusted credential. No agent-accessible MCP tool may mint approval.
- DR-003: Provide an operator-visible exact-plan summary; document whether it
  appears on the phone or on the desktop and do not claim trusted transaction
  display where the authenticator does not provide one.
- DR-004: Use bounded inputs, local/offline defaults, structured failure and
  durable one-use consumption. Preserve the current explicit threat boundary:
  unrestricted code under the approving principal is not an application-only
  security boundary. Do not treat a local prompt, file or agent-supplied PIN as
  proof of human approval.
- DR-005: Keep an unavailable or unverified trust anchor fail-closed. No
  simulated approval grants a physical operation. No automatic device access,
  device write, account creation, key enumeration or external service is implied.

## Acceptance criteria

- [ ] AC-001: A user-controlled enrollment and passkey assertion bind a tested
  exact plan without disclosing the PIN to the agent or backend.
- [ ] AC-002: Changed plan/target, unknown credential, invalid origin/RP,
  absent user verification, replay and expiration are rejected.
- [ ] AC-003: Tests establish the stated operator/MCP separation and clearly
  record any same-principal or trusted-display limitations.
- [ ] AC-004: The prototype integrates with the existing editor guard without
  treating synthetic credentials or tests as a real grant.

## Dependencies, tests and boundaries

Accepted [Task-019](Task-019.md) and [Task-151](Task-151.md); Task020's tested
offline guard model supplies the integration fixture. No phase gate applies to
this bounded PoC. It does not mark the gated production authorization
[Task-070](Task-070.md) Done or advance [Task-028](Task-028.md).

Verify with standards-based unit/negative tests, an operator-driven local phone
enrollment and assertion, and an independent security review. Physical KNX
writes, production credentials and actual PINs are not test fixtures. Browser,
phone and authenticator compatibility are NOT ESTABLISHED until observed.
Any package dependency must have its version, license and security properties
reviewed and pinned before use.

## Completion record

On 2026-11-08 the owner completed protected macOS passkey enrollment and an
exact-plan assertion through the Task153 operator service. The owner-provided
Task020 execution journal reports desired readback, restored readback and
cleanup for the same plan digest; see Task020 for the transcript hash and
limitations. This supplements the earlier synthetic browser ceremony; it does
not establish Windows or Linux provider behavior, trusted transaction display,
or resistance to unrestricted interference by the same macOS principal.
The criteria and security review remain open pending task-level assessment.

| Field | Current value |
|---|---|
| Implementation / report status | Protected macOS enrollment and exact-plan assertion exercised via Task153; criterion-by-criterion security review remains open |
| Source commit(s) | See Git history for this task's verifier increment |
| Tests executed | Offline Cargo test: 45 library tests, 7 browser-runner tests and one compile-fail doctest PASS on macOS arm64; fmt and clippy PASS. Synthetic Safari ceremony and protected macOS enrollment/assertion observed; Task020 records the owner-reported hardware outcome. Windows and Linux NOT RUN |
| Evidence location | [Experiment README](../experiments/task152-passkey/README.md), tests and dependency inventory; local private evidence `private/evidence/task152-browser-demo/`; Task020 offline guard evidence |
| Acceptance criteria met | Positive macOS integration and negative-test evidence exist; final supported-host security review remains open. Synthetic credentials cannot authorize physical effects |
| Reviewer | Independent agent review: no blocker for development-only increment; Windows directory synchronization remains unverified and likely requires a platform-specific implementation |
