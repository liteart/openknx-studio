# Task-009 deny-only Undo provider diagnostic

## Readiness and purpose

Original20a SDK construction remains FAIL10: the Project invocation reported an
unregistered IUndoManagerStoreProvider. Original21 guarded metadata at source
55c3617 now verifies its public nongeneric interface and three declarations;
the query and independent reviews are recorded by f5ffb14. See
[the provider declaration contract](DCA-UNDO-STORE-PROVIDER.md).
Task-008/provisioning predecessors are accepted; no phase gate applies or advances.

This increment registers only a typed deny-all provider to discover the first
required callback during the same bounded Project construction. It returns no
path, null, store or successful service result. IStreamStore behavior and RootPath
meaning remain unknown and are not needed to reject calls. This is neither a
positive provider implementation nor a retry of the frozen original20a plan.

Private readiness note `task009-undo-provider-denial-readiness/readiness.md`,
SHA-256 `89bfe21cd5ec1a3de37043c89b3d4561073a48cc4a8f6544ece2a1af12e38436`,
cross-checks21 source/evidence references and confirms this deny-only scope.
It supplies no implementation or operation approval.

## Fixed boundary

Use distinct fixed modes `original-project-undo-provider-denied` and
`owned-project-undo-provider-denied`. Preserve all previous modes, inputs,
configuration, artifact layout and results. Keep the same19 private/two OS pins,
18 build artifacts,15-second child deadline and65,536-byte output limit.

Before SDK value construction or registration, validate the provider in the
pinned ObjectModel: TypeDef02000121, attributes1048737, public nongeneric
interface, no direct inherited interfaces. Validate exact public methods:
06000AF3 GetStore(Guid)->ObjectModel.IStreamStore, attributes1478;
06000AF4 NewStore()->ObjectModel.IStreamStore, attributes1478;
06000AF5 get_RootPath()->String, attributes3526. All instance/default/header32,
generic arity0. Return-type identity/name comparison does not qualify a usable
IStreamStore. Type-load or dependency failure stops the affected path; no new DLL.

Register this exact interface instance alongside genuine trace before Build,
using the already verified public Autofac RegisterInstance<T>/ExternallyOwned
surface. No Resolve, store construction, RootPath call/value, SDK factory method,
device service or speculative business behavior is introduced by the harness.
The reference SDK may request a callback during its one constructor invocation;
the proxy must record a bounded named unsupported denial.

Count/latch every provider callback before message validation, including malformed
messages and exceptions. Do not inherit the old deny proxy's uncounted invalid-
message branch. Preserve old proxy behavior; a separately scoped new proxy is
acceptable. Suspend new-mode resource eligibility throughout each callback,
restore it in finally, and keep all denial latches/counters sticky. Do not log
actual argument values. No returned value or caught exception may conceal denial.

Retain the provider and its request counter through every managed lifetime.
Final audits after Project/container/trace disposal reject any provider request,
including caught requests during registration, construction, error handling or
cleanup. No Project Dispose is invented when construction did not return.
Registration failure must still leave trace/container cleanup and counter evidence
accurate. ExternallyOwned means no fabricated provider Dispose call.

Only the new mode pair extends the exact resource-mode admission predicate;
the shared aggregate8/eighth/ninth saturation, pinned owner/identity/culture,
NULL requester UNKNOWN, exact constructor windows and callback exclusions remain.
Never reset dependency/provider/store latches to make a test pass.

## Source ownership and required evidence

Implementation paths are the four existing experiments/dca-constructor files:
ProjectConstructorProbe.cs, ProbeLauncher.cs, OwnedProjectConstructorFixtures.cs
and Test-ConstructorProbe.ps1. No metadata-reader, compiler/build script, SDK
contract declaration, dependency, protocol or application persistence change.

Owned tests must cover exact API/type/method shape rejection before effects;
the two interface registrations/order and ownership; all three callback denials;
invalid/null message counting; sticky caught denial across all lifetime/error
scopes; callback resource suspension/restoration and legacy preservation.
Use fresh processes for sticky cases, not production latch resets. An owned
fixture must not impersonate the pinned SDK type or original Autofac owner.

Required progression: C#5/x64/warnings-as-errors compilation against the same
six pinned references, PowerShell parsing/source guards/preservation; fresh
Windows build and full owned Session0/limited/admin-negative suites; independent
verification and critical review; reviewed local source commit. Only then may
the coordinator prepare/review a separately frozen one-attempt original plan
under the existing bounded diagnostic authorization. No original runtime in this
implementation assignment and no intentional device operation.

Status: contract prepared; new implementation/tests/runtime NOT RUN. Original21
metadata remains declarations-only PASS, original20a runtime remains FAIL10.
Task-009 stays In progress/all ACs unchecked/qualification NOT_ESTABLISHED, no
gate advance. Vendor internals/network/descendants remain unqualified; this is
not a sandbox or proof of a working Project/DCA. Owner private-copy permission
and prohibition on proprietary DLL redistribution remain unchanged.


## Source22 preparation and owned Windows results

Source22 is frozen by private `task009-undo-provider-denial/source-freeze22.json`
(9,336 bytes, SHA-256 `9be524673d4cd614b5c9f35625cc1dcd7c9953e8b66eca61ab6908cd2b8c53a8`).
It records10 source hashes,8 compiled sources and4 changed paths. The34-entry
local manifest and snapshots remain unchanged. Byte-identical copies of the
freeze and35 local evidence files are retained under constructor-windows for
a contained Windows-result audit; copy manifests preserve origin and hashes.
Freeze-time contract bytes are `source22-contract-at-freeze.md` there.

Local checks PASS: three C#5/x64/warnings-as-errors compilations with the same
six pinned net48 references; PowerShell parse/source guards; preservation, scope
and whitespace. The extracted counter-only final-audit guard passes under owned
modern .NET; removing its throw in memory produces expected FAIL1
`OWNED_UNDO_FINAL_AUDIT_MISSING`. This proof does not run RealProxy or SDK code.
Exact commands/failure-free final logs are in source22; preliminary check logs
remain separately retained. Old proxy cores, fixture families,14 readonly arrays,
18 old method bodies and legacy driver oracles are preserved, as are metadata6
and the other6 constructor sources. Only the new exact original mode extends
existing admission predicates.

Independent local verification `constructor-windows/verifier22/source-audit.txt`
(5,353 bytes, SHA-256 `fa81a3b9f8ff2eb30d3e7929325386524b43360f6d23f696358d6961ead02df1`)
PASS; it reproduced compilation, parsing and the guard/mutant proof and checked
all source/evidence bindings. Critical source/local review
`constructor-windows/review22/critical-source.txt` (8,037 bytes, SHA-256
`e559186f1e7086e707331ff5cb61130d038b542b90d005cb4eec30f9952ae41c`)
PASS, no unresolved source findings. Those reviews did not run Windows or SDK.

### Actual owned Windows verification

The8 fresh Windows22 helper clones retain prior20a evidence unchanged. Their
independent static audit PASS is `verifier22/orchestration-audit.txt` SHA-256
`1fd0e54e34e631e9f3a19c21902c80d62654832f326077af90c3a14d63cc5b2e`.
Before execution, the coordinator's live preflight proved19 new runner/source/
build/output paths absent, x64, ETS0 and owned-processes0. The source freeze
copy/current10 and all8 helper pins were also checked.

Actual commands, each executed once through separate owned capture wrappers:

- `build_owned22.py`: PASS, pinned Windows compiler,8 sources/18 artifacts.
- `run_session0_22.py`: PASS116 Session0 checks.
- `run_owned22.py`, terminal poll, `collect_owned22.py` and `cleanup_owned22.py`: PASS195 limited interactive checks, Session2/non-administrator, done0/task removed/processes0/no junction.
- `run_owned_admin22.py`, terminal poll, `collect_owned_admin22.py` and `cleanup_owned_admin22.py`: PASS121 administrator-negative checks, Session2/administrator, done0/task removed/processes0/no junction.

All helpers are private under task009-constructor-windows. The actual fixture
path verifies two exact external registrations/order, all3 provider methods,
null/invalid messages, saturation17, nested/resource/recorder exceptions and
caught denials in registration, construction, errors and every cleanup scope.
The isolated final-audit case has no other dependency latch; it still fails.
Registration/external-null negatives return expected child10; other denial
cases return expected child2. These are owned fixture observations, not SDK
behavior or a working provider. Legacy nested-callback results remain exercised.

Both cleanup file lists match every copied case file. The saved-file-only
`python3 -B private/evidence/task009-constructor-windows/record_owned22.py`
PASS produced `owned22-observation.json` (165,695 bytes, SHA-256
`f57314f3575a6eded7582a3e01f3f3c3eac1f3ec56913448cb86d65972c51656`),
binding842 evidence files and exact source/build/profile/cleanup checks.
Independent Windows-result verification and critical evidence review are pending
at this checkpoint. Source22 is not yet committed; original22 has no execution
plan or runtime attempt. Task-009 remains In progress with all ACs unchecked.


### Source22 Windows result verification and critical review

Independent `constructor-windows/verifier22/owned-result-audit.txt`
(5,394 bytes, SHA-256 `fd75c8620848d24d38b8264e2f9976916d749a24f50da4de527d7b7333cb956e`)
PASS: all842 contained bindings,10 source pins, byte-exact freeze/proof copies,
8 build-source records/18 artifact declarations/compiler,19 fresh paths,
actual116/195/121 results, role/done records and467+245 case files/encoding.
It checked all21 new denial cases and lifecycle/final-counter assertions.
Built executable/DLL payloads were not local for an independent byte rehash;
actual saved Windows preflight verifies their18 size/hash pins, and the audit
cross-checks declarations and every fixed runner pin. No universal binary or
SDK qualification follows.

Critical Windows-owned/final-six-path review
`constructor-windows/review22/critical-owned-results.txt` (7,076 bytes, SHA-256
`f11d7afc1a7e1d08a4bb894ffba6c8748c6cf581cc653b3ee6ef6369314e4ba1`)
PASS, no unresolved findings. It independently checks the bounded claims and
source/evidence consistency; no SDK payload read or runtime rerun. Coordinator
scope/whitespace checks PASS. The preparation is ready for local source commit
and a separately frozen/reviewed original22 plan. Task-009 remains In progress;
no operation approval, gate or working provider/Project/DCA is asserted.
