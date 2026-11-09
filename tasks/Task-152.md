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

Verify with standards-based unit/negative tests, an operator-driven local
platform passkey enrollment and assertion, and an independent security review.
The later owner decision selected the macOS vault for this first ceremony.
Physical KNX
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
| Source commit(s) | Protected integrated exact-source build `de571a6a582c87a0af8386c7431968037e8d1ba5`; fork `4d296afb0906d17b6724b56c912f2e9e3f6680a5` |
| Tests executed | macOS: 56 verifier library tests, compile-fail doctest and browser tests PASS; exact-export 148 tests and actor validation PASS. Linux arm64 container: 56 library tests PASS twice independently. Owner-observed macOS enrollment/assertion and Task020 result recorded separately. Windows and Linux provider ceremonies NOT RUN |
| Evidence location | [Experiment README](../experiments/task152-passkey/README.md), tests and dependency inventory; local private evidence `private/evidence/task152-browser-demo/`; Task020 offline guard evidence |
| Acceptance criteria met | Positive macOS integration and negative-test evidence exist; final supported-host security review remains open. Synthetic credentials cannot authorize physical effects |
| Reviewer | Independent security review found no blocker for the bounded macOS PoC under the approved same-account policy; Windows directory synchronization and provider operation remain unverified |

The independent review traced plan binding, challenge expiry and one-use
consumption, protected credential verification, exact origin and RP, user
verification, synthetic-grant separation, and the visible plan to source and
negative tests. Commands were `cargo test --locked --offline --manifest-path
experiments/task152-passkey/Cargo.toml --lib` (51 PASS), the same command with
`--doc` (one compile-fail PASS), and `node --test
experiments/task152-passkey/browser_test.mjs` (nine PASS). Exact-export evidence
is under `private/evidence/task153-service-build-20261108T064526827601Z/`.
These results do not prove Windows or Linux operation, protect the browser from
unrestricted same-account interference, or qualify the fixed one-shot broker
as a reusable production workflow. The task remains in progress.

The development verifier now checks its actual journal directory barrier before
issuing any enrollment or assertion ceremony. A failed or unsupported barrier
returns an explicit error; receipt consumption still requires both file and
directory synchronization and is never automatically cleared after failure.
Reviewed source `0fd7df84f50e501e0e820cace66522c87af64927` was integrated as
`523e337`. The implementer and independent reviewer each ran 56 library tests,
one compile-fail doctest and six browser-handler tests (socket test excluded),
and the coordinator repeated the 56 library tests on main. Details and the
mutation test are in `experiments/task152-passkey/TASK152-JOURNAL-PREFLIGHT.md`.
This explicitly rejects the unqualified Windows barrier before a ceremony; it
does not implement Windows support. Subsequently, the coordinator and an
independent verifier ran all 56 library tests successfully in an isolated
Linux arm64 container on tmpfs. The image digest, environment and command are
in `experiments/task152-passkey/TASK152-JOURNAL-PREFLIGHT.md`. Native-host
filesystem durability, Linux socket/provider integration and power-loss
behavior remain NOT RUN.

An experimental Windows development-journal backend uses a per-user registry
reservation and explicit flushes through the pinned `winsafe` wrapper. It is
disabled by default; without `experimental-windows-journal`, Windows still
rejects verifier construction before a ceremony. The owned Windows code
forbids unsafe; the dependency contains inherited FFI and was reviewed as a
dependency. Reviewed source `9b3027aab13b950e674638fc30d0b63aa853be72` was
integrated as `392a97e`. Both feature-off and feature-on Windows GNU
cross-checks passed, as did 60 macOS library tests and independent review.
Native Windows registry, concurrency, restart and provider tests remain NOT
RUN; the imported guard journal also remains separately unqualified on Windows.
This opt-in is for qualification, not device authorization or Windows support.
Commands, dependency provenance and limitations are in
`experiments/task152-passkey/TASK152-WINDOWS-REGISTRY-JOURNAL.md`.

The Windows x86_64 synthetic test harness was cross-linked from editor
`69b2f9b452176f56b2eb03c7078bc3526ad0c5b2` using pinned Rust 1.99.1 and
verified llvm-mingw 20261104. The locked/offline `cargo test --no-run` build
passed; the private test executable SHA-256 is
`d1cf41e4ff92c3a0702dffefa21901cc4d7e89e3a4fb050fbc9094bf500d6796`.
An independent verifier checked saved hashes, architecture and static
imports, but did not reproduce the build. Commands, runtime dependency and
limits are in `private/evidence/task152-156-windows-cross-20261108/README.md`,
SHA-256 `b860fb14269d7fc7c307c42daf6b5a86e2945990358b65cfe1d539410450d72c`.
Native Windows execution and registry behavior remain NOT RUN.

At editor revision `6aea59752cd15d340cc32b0c571ef8e1de329df9`, the
coordinator and an independent verifier separately reran the locked offline
Linux arm64 library suite on isolated Docker volumes: 66 tests PASS in each
run. The LinuxKit filesystem reported `ext2/ext3`; source and registry-cache
mounts were read-only and networking was disabled. These tests do not establish
native-host power-loss durability, a Linux browser/provider ceremony, or
Windows behavior.

After a review identified unbounded child waits, source `0006012e08f6e8f97c03461c7b707a75d5daf767`
was integrated as `ed94ab4` and independently checked. The corrected Windows
test harness was cross-linked from `91bea8c7dc45b8e6f2ca49013c45d886cd8bfe01`;
its SHA-256 is `d499e3d100803281154dd83d9944f22a9b574b57454a4ee82f00c3876784577d`.
The private build record is
`private/evidence/task152-156-windows-cross-91bea8c-20261108/README.md`,
SHA-256 `8f336ef413fc7b4522dae413af44900d2f8822d54cb7e72340b78f3a8143fc5b`.
An independent static review verified artifact hashes and imports, not the
build itself. This supersedes the earlier test executable; native execution
remains NOT RUN.

The private Windows runner for this rebuilt harness is pinned to the reviewed
source, artifact hashes and dev-host identity. Independent static review and
PowerShell parsing passed; its SHA-256 is
`18a8fe0042966bfa1a06dadc81f794ea394d129dc6d14f45d113d8eac448255f`.
It selects the exact synthetic test inventory, checks bounded process cleanup
and does not automatically retry. The runner and native tests remain NOT RUN.

### Windows native synthetic tests — 2026-11-09

On Windows dev host `DESKTOP-3UKTEIP`, the owner-established connection was
used to run the reviewed feature-on harness once in the existing `dev` account
under the launcher's limited-token check. All four Task152 synthetic registry
tests passed, including same-process and separate-process contention and bounded
child termination. The independent reviewer checked each test's output and
the saved pins; the complete combined run contained 15 successful tests.
Private evidence is under `private/evidence/task152-156-windows-native-20261109/`;
the combined `result.json` SHA-256 is
`e7baf24638e5d4c6cb32295a5f5a7295228bf67d38c61f148864724199645b5a`.
The scheduled task exited 0 and a scoped process check found no remaining test
processes. Earlier NOT RUN statements are historical. These results cover the
experimental synthetic registry journal on this host, not a real passkey
provider, power-loss durability, Windows guard production integration or KNX.
Task152 remains in progress.
