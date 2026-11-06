# Task-153: Prototype a protected local operator broker for KNX writes

## Subject

Build a protected operator approval and execution path for one exact KNX write plan.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-153 |
| Status | In progress |
| Stage / required gate | PoC; no phase gate unlocked |
| Priority | P0 |
| Component / Git project | knx-editor / operator approval and guarded KNX executor |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-025](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |
| Evidence level | Offline and simulator; privileged installation and hardware are separate follow-up actions |

## Description

Connect the user-tested WebAuthn passkey verifier to the existing Task020
offline guard and connected-session adapter through a locally protected
operator service. The service, its trust anchor and its execution authority
must be outside the agent/MCP principal's writable and callable boundary. The
service consumes the approval and executes the single immutable approved plan
itself; it must not return a bearer grant that the agent can reuse or redirect.

The first implementation targets the current macOS test host using an
OS-protected service boundary. Define a portable Rust protocol and explicit
platform-provider boundary for Windows and Linux; platform support is not
claimed until it has been run and reviewed on those systems. A same-user
localhost process, file ACL claim without a test, `sudo` command assembled by
the agent, UI text, or successful passkey ceremony alone does not establish
the required boundary.

The target-scoped hardware grant already recorded for Simon remains limited to
KNX device `1.1.231`. It does not grant authority to write another device,
install a privileged service, or claim a hardware test passed. This task is
offline/simulator-only; privileged installation and the separately coordinated
hardware test remain later explicit operator actions.

## Detailed Requirements

- DR-001: Keep the service executable, enrollment store, credential public key,
  one-use ledger and configuration protected from ordinary writes and process
  impersonation by the agent/MCP principal. Fail closed if the host cannot
  demonstrate the required ownership, peer identity and access policy. State
  the remaining risk if the approving human and agent share an unrestricted
  principal or administrative authority.
- DR-002: Persist an operator-enrolled passkey credential in the protected
  store. Require WebAuthn signature validation, user verification, exact RP and
  origin, fresh challenge, expiry and one-use consumption. No PIN is collected
  by the service or written to logs or files.
- DR-003: Display the canonical immutable plan in the protected operator
  surface before requesting the passkey. Bind the challenge and assertion to
  the plan digest, exact device identity, route, property, before/desired/
  restore bytes, purpose, issuer and expiry. Explain that the authenticator
  dialog does not display or attest to the KNX transaction details.
- DR-004: The service owns the complete narrowly allowlisted execution path.
  For this PoC, use only the already prepared `1.1.231`, Device Object 0,
  PID_DEVICE_CONTROL 14, element 1, one byte `00 -> 04 -> 00` candidate with
  fresh reads before change and after restore, using the tested connected KNX
  session adapter. Do not contact the device in this task. Do not permit an MCP
  caller to select another target/property/value or invoke a generic write.
- DR-005: Persist operation intent and one-use consumption before dispatch;
  keep unknown or interrupted effects quarantined across process restart.
  Never replay an uncertain write or automatically restore after an uncertain
  write response. Preserve the existing requirement for independent desired
  and restore readback and confirmed channel cleanup.
- DR-006: Provide a platform-neutral, bounded IPC contract. Authenticate local
  peers using OS peer credentials and narrow file/service permissions. Reject
  forged clients, changed plans, stale/replayed challenges, expired approvals,
  malformed and oversized requests, missing user verification, modified
  protected files, cancellation and restart recovery uncertainty.
- DR-007: Separate source/package preparation and offline tests from privileged
  registration or installation. Provide an auditable human-driven installation
  and enrollment procedure; do not invoke it from an agent-built generic shell
  command. The owner must explicitly perform OS authentication for this action.
- DR-008: Use English-owned source, documentation and diagnostics; keep
  technical values locale-independent. The passkey provider boundary must
  support platform/browser authenticators without a macOS-only WebAuthn API.

## Non-functional Requirements

- NFR-001: Follow repository component boundaries in [AGENTS.md](../AGENTS.md).
  Do not move KNX protocol work into the editor broker or duplicate the
  protocol adapter in the service.
- NFR-002: Keep the privileged interface small, local-only, bounded, typed,
  cancellable and fail-closed. Do not add remote access, general shell
  execution, a general-purpose write API or automatic network access.
- NFR-003: Preserve the exact pinned WebAuthn dependency review and upstream
  license notices. Review new dependencies, versions, licenses and security
  properties before adding them.
- NFR-004: Record macOS, Windows, Linux, privileged installation, hardware and
  gate outcomes separately. A simulated operation is never hardware evidence.

## Acceptance Criteria

- [ ] AC-001: An unprivileged agent/MCP process cannot enroll a credential,
  change the trusted key, mint a grant, access the protected execution API, or
  alter the service/ledger; tests prove each denied path on the supported host.
- [ ] AC-002: The operator surface displays the same canonical plan that the
  protected service validates and binds to one passkey assertion; mutation,
  replay, wrong target, wrong effect, wrong origin/RP, missing UV and expiry
  are rejected before any adapter call.
- [ ] AC-003: The service owns the only approved dispatch path and calls the
  existing typed connected-session adapter at most once per approved phase.
  The offline exact write/readback/restore/readback sequence remains guarded
  across cancellation, crash/restart and uncertain outcomes.
- [ ] AC-004: Local integration tests exercise the service boundary and mock
  adapter without network or physical device access; no test or development
  credential can issue a physical capability.
- [ ] AC-005: A reviewed macOS installation/enrollment procedure is ready for
  explicit operator action. Windows/Linux implementation and validation are
  reported independently and are not inferred from portable source code.

## Dependencies to Other Tasks

- [Task-152](Task-152.md): Standard WebAuthn assertion verifier and successful
  user-driven macOS browser ceremony for a synthetic plan. This dependency is
  limited to observed verifier behavior; Task-152 remains In progress until
  its protected-enrollment and guard-integration criteria are accepted.
- [Task-020](Task-020.md): Existing offline exact-plan guard and typed adapter
  are the execution contract. This task must not mark Task-020 or its hardware
  acceptance criteria Done.
- [Task-151](Task-151.md): Connected device session and confirmed teardown
  behavior used by the offline adapter contract; does not authorize hardware
  access under this task.
- [Task-070](Task-070.md): This PoC does not satisfy or bypass its required
  G-POC phase gate or production DCA authorization criteria.

Privileged service registration/enrollment and a physical write require a
separate operator action after implementation and independent review. The
existing device-scoped user consent does not extend to another target.

## Required Tests

- TEST-001: Host boundary: verify service files/store are protected and
  unprivileged agent UID cannot modify or impersonate the protected service.
- TEST-002: WebAuthn: successful enrolled assertion; changed plan/target/effect,
  unknown credential, invalid RP/origin, absent UV, replay and expiry fail.
- TEST-003: IPC/authority: forged or unauthenticated peer cannot enroll,
  mint, invoke execution or alter persisted state; authorized UI cannot widen
  the exact approved plan.
- TEST-004: Fault injection: cancellation, crash between durable intent and
  dispatch, uncertain response, failed readback and failed cleanup remain
  unknown/quarantined after restart and never trigger retry.
- TEST-005: End-to-end simulator: passkey-verifier flow drives only the
  synthetic KNX peer through exact desired/readback/restore/readback; record
  simulator evidence separately from hardware.
- TEST-006: macOS system registration, owner enrollment, Windows, Linux and
  physical KNX write are NOT RUN until their distinct prerequisites/actions
  have been satisfied and separately recorded.

Use pinned repository toolchains and exact reproducible commands. Preserve
reviewable evidence and fixture hashes without storing the PIN, private key,
household data or proprietary files in Git.

## Deliverables

- Minimal broker, OS service boundary, portable IPC contract, focused tests and
  operator installation/enrollment instructions in the editor repository.
- Updated Task-020/Task-152 integration records and evidence identifying
  platform, commands, revisions, test results and remaining limits.

## Out of Scope

- Installing/registering a privileged service or enrolling a production
  credential without the owner performing the OS-authenticated action.
- Any physical KNX access or write; Task-020 remains the hardware test task.
- General KNX programming, arbitrary properties/targets, DCA grants, remote
  approval, cloud services, account/key enumeration, production release,
  Windows/Linux qualification, or claiming resistance to an agent that has the
  same unrestricted administrative authority as the operator.
- Unlocking Task-028 or satisfying Task-070's broader production gate.

## Risks and Blockers

- The actual privilege relationship of the operator and agent on this Mac is
  unverified. Do not inspect credentials or assume a separate principal.
- A privileged macOS helper requires an explicit operator-authenticated
  registration/install and enrollment. Source code or a launch configuration
  alone is not a trust boundary.
- Browser passkey authentication does not provide a trusted display of KNX
  transaction details; the operator surface must display the canonical plan.
- The selected device/property's actual write permission and full reversible
  behavior remain unproven until the separately authorized Task-020 test.
- If OS ownership/IPC controls, protected display or persistence cannot be
  proven, stop with the physical dispatcher disabled.

## Completion Record

The first source-only increment defines bounded request parsing and a configured
plan policy; its unprovisioned facade always refuses execution. Five offline
tests, formatting and Clippy passed on macOS arm64. An independent review
passed for this bounded increment. See
[the experiment](../experiments/task153-broker/README.md) and Git history for
the source and commands. The service, OS privilege/IPC boundary, protected
enrollment, actual adapter integration and hardware tests remain NOT RUN.
All acceptance criteria remain open; no gate is advanced.
