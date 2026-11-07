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

Owner direction on 2026-11-07: use the same macOS account for Simon and
Codex; do not require a second account. This supersedes the separate-principal
assumption below. Keep protected initial enrollment, exact-plan WebAuthn user
verification, backend-enforced one-use execution and no MCP grant-minting tool.
Unrestricted code under the same principal can interfere with the local browser
or operator display; this mode does not claim isolation against that principal.
The existing target-scoped hardware consent is unchanged.

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
the source and commands. A subsequent macOS increment uses kernel-reported
Unix-socket peer credentials and rejects an agent UID or unexpected principal;
eight tests and one compile-fail test passed, including real local sockets.
Distinct-principal authorization and protected provisioning remain NOT RUN.
A macOS descriptor-bound configuration inspection increment checks root
ownership, mode and native ACLs through no-follow descriptor-relative traversal;
unknown ACL types fail closed. Its focused tests include real ACLs, symlinks,
rename binding and bounded reads. Independent review passed for the bounded
source increment, not for privileged launch or installed-service integrity.
A read-only verifier now loads a public passkey record only through the fixed
descriptor-validated configuration and checks plan-bound WebAuthn assertions.
Synthetic cryptographic tests and independent review passed. Its observation
cannot authorize hardware; actual protected enrollment, durable credential
updates and the execution ledger remain unimplemented or NOT RUN.
Private enrollment staging now writes a non-active pending record using
create-new and file/directory synchronization; it neither activates a key nor
provides an operator endpoint. Seven temporary-filesystem tests passed and the
bounded increment was independently reviewed. Failure before directory sync
does not guarantee a pending file survives power loss. Protected serialization
remains required. A private activation increment now serializes readers and
writers, uses an activation fence and no-replace rename, and tests interruption
and uncertain cleanup with temporary files. It is not connected to an operator
service or actual enrollment; power-loss hardware behavior remains unverified.
A root-peer-checked connection handler now connects a verified registration to
staging and activation, and checks assertions against the fixed root record;
its unprivileged/synthetic tests and independent review passed. No listener,
protected operator client or real privileged integration was available then.
A macOS listener source now verifies a protected manifest, running-code
identity and the fixed target plan before accepting root peers. Its test suite
and independent review passed for source-only use. It is not installed, has no
trusted operator UI or KNX executor, and has not run with elevated privileges.
The listener now supports a separately provisioned non-root operator principal:
socket ACL identity is pinned and kernel peer UID/GID is checked independently.
Synthetic socket tests and independent review passed. Actual separate-account
access, installation and browser bridge remain NOT RUN.
An experimental separate-operator Safari bridge and its synthetic tests are
now available. Protected enrollment must not run until an isolated browser
profile and origin state, including absence of service workers and untrusted
extensions, are independently qualified. Actual separate-user access and
Safari enrollment remain NOT RUN; the bridge does not authorize KNX writes.
A one-shot assertion record now stores the exact plan and updated public
credential before reporting a verified observation; existing records prevent
reuse. Synthetic cryptographic/filesystem tests and independent review passed.
Protected-root restart behavior and actual execution integration remain NOT RUN.
A gated offline integration now connects the persisted synthetic assertion to
the existing guard and exact exported knust adapter. The loopback peer completed
write/readback/restore/readback and teardown; altered or expired plans emitted
no UDP packets. The exact-export runner passed 108 tests, Clippy and artifact
validation; an independent reviewer reproduced the focused scenarios. Evidence
is under `private/evidence/task153-execution-integration-20261106/` (result
SHA-256 `1761dbce935f6489b78d5d59b90aaa256e21c8d3575444bde81edddc2e84bd51`).
The production qualification is deliberately unconstructible; installed host,
browser isolation, protected journal and physical write remain NOT RUN.
The execution path now also checks that its protected one-shot journal is empty
before opening the adapter. Eleven focused tests and independent review passed;
the exact-source integration passed 111 tests. Private evidence is under
`private/evidence/task153-journal-check-20261106/` (result SHA-256
`45e57ee8b7568a5d544a5c7f301a2b1a5d741a4ab01e87856c08969eae13e953`). A
positive root-owned journal and installed execution remain NOT RUN.
The exact-export runner now also builds the actual service entry point and
records its artifact hash. The 111-test run and build passed; independent review
verified the source and artifact hashes. Private evidence is in
`private/evidence/task153-service-build-20261106/` (result SHA-256
`d87c2a8a503a7b764e8352dacf62f1e722cb04bd6c1be6ff212b641e87811217`).
The normal service still discards the witness; building the binary does not
enable an execution path or prove that unreachable adapter code is retained.
The service handler now has a private, qualification-gated callback from a
durably verified assertion to the guarded adapter. Normal startup supplies no
callback and the current browser rejects guarded mode. The exact-source run
passed 114 tests and independent review reproduced focused scenarios; evidence
is in `private/evidence/task153-callback-20261106/` (result SHA-256
`1d27517b35d98bbcd99ee8c4de57853a8f54f2682a0a46d5f4e31e909e6e7c69`).
Actual host qualification, installed operation and hardware remain NOT RUN.
The local development installer was rebuilt as two narrow components after
review found parent-permission and AppleDouble problems. Static inspection of
the actual package, its BOM, contents, signatures and architecture passed
independent review; five packaging tests passed. Inventory is in private
`evidence/task153-installer-repair-20261106/`. Installer execution and actual
parent-metadata preservation remain NOT RUN; this does not authorize or enable
the device write.
The package preparer also accepts the exact-export service artifact and verifies
its source snapshot, pinned dependency trees and copied binary before signing.
Ten packaging regressions and independent static package review passed. The
private inventory is in `private/evidence/task153-exact-package-20261106/`;
the reviewed package SHA-256 is
`87fc6441964d58f5e03534b88beed356e092c7d6ab8eed37b794bf7037e536f2`.
This evidence is snapshot consistency, not independent build attestation.
An uninstalled, root-gated diagnostic helper source can later exercise the
peer checks without a passkey or device operation. Its unprivileged tests and
read-only macOS ancestor preflight passed; privileged launch remains gated on
ownership, ACL, executable integrity and cleanup review.
The service, OS privilege/IPC boundary, protected
enrollment, actual adapter integration and hardware tests remain NOT RUN.
All acceptance criteria remain open; no gate is advanced.
