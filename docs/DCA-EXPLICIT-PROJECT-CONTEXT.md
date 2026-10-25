# Task-009 explicit-context constructor denial experiment

## Readiness and one increment

Task-008 is accepted; Task-009 remains In progress/all ACs unchecked, no gate.
The parent type-shape result and reviews are recorded in
[DCA-PROJECT-CONTEXT-PARENT.md](DCA-PROJECT-CONTEXT-PARENT.md), source
5c3da7565fd31b42ca0a1c0898ddce661c34a7cf/result c16d24da1d7b185d4772e5bb3337503b273d62d7.
The original27 runtime FAIL2 remains historical: the prior constructor did not
return and requested an unallowlisted OnlineCatalog.Download assembly. Source30
found no matching public static direct-return route in its bounded selection.
Neither result proves unavoidable catalog coupling or a working alternate route.

Private context-construction-design32.md SHA-256
8d9b8ed5b74e3d0bd32c1297c8471141bc03fcf75a8005aace4e03aa9a8d812c
and runtime-reuse-observations31.md SHA-256
880087b8ca9e85c8b3c77b24d88aeba5faf8c2320c8e467ebce0070a6acc0afe
under task009-project-context-parent identify sufficient declarations for this
bounded runtime implementation. No more getter inventory is required.

Add only modes original-project-context-denied and owned-project-context-denied.
Implement exact Collection construction followed by the explicit-context Project
constructor, with diagnostic deny-all parent/context proxies and a closed typed
importer delegate that always throws. This supplies no parent/context/importer
business values. Actual CLR ABI/lifetime behavior is a required owned proof;
original SDK use requires its later separate immutable reviewed plan.

## Verified identities and exact argument API

Keep the same19 private/two OS input pins and18 artifacts; no new dependency.
ObjectModel remains14,204,880 bytes/SHA-256
73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a,
identity6.4.8718.0/neutral/token61439873ec5e1159. Reuse the approved original
Common/Autofac/Factory contracts and hashes unchanged.

Validate trusted runtime Types/ConstructorInfos, pinned module/location/identity,
exact token/flags/closedness/calling convention and ordered Type reference identity
before construction. Original API facts are:

| Owner/member | Verified declaration |
|---|---|
| ObjectModel Project020003A5 | public nongeneric class, flags1048577 |
| Project .ctor06002831 | public instance, flags6278, Standard+HasThis; (ProjectCollection,IProjectStore,IFactory,IObjectContext) |
| ObjectModel ProjectCollection020003BD | public nongeneric class, flags1048577 |
| Collection .ctor0600298E | public instance, flags6278, Standard+HasThis; (Interfaces.INamedDomObject,IFactory,closed Func8) |
| Interfaces.INamedDomObject02000C99 | public top-level nongeneric interface, flags1048737, nil declaring/base; direct IDisplayName02000C97 and IDomObject02000033 references remain unexamined |
| IObjectContext02000044 | public nongeneric interface, flags1048737; existing ICacheProvider reference remains unexamined |
| IFactory02000037/IProjectStore02000057 | existing validated public interface contracts |
| Project Dispose060028BD | preserve existing exact public IDisposable map validation; call once only for a genuine returned Project |

Do not reuse ScopedProjectApi's06002830 selection or its old argument array.
Create a separate exact API/argument path. No constructor bypass, uninitialized
object, guessed null success or fabricated concrete SDK type. Direct/inherited
interface methods need no positive implementation: ALL received callbacks deny.
No new interface closure or constructor cleanup semantics are inferred.

The actual third Collection parameter must be the closed BCL mscorlib Func8
with these seven reference-type arguments and return, in order:
IProjectStore; concrete Project; pinned Common.Security.IKeyProvider;
Func<String,String>; Func<IEnumerable<Import.BaggageInfo>,Boolean>;
Action<Import.IImportInformationCollector>;
Predicate<Import.IImportInformationCollector>; return Import.ProjectImporter.
Validate exact pinned owner and nested BCL generic definitions/Type identities,
closed reference-type arguments and return. Do not invent SDK tokens for types
whose numeric tokens have not been observed, or construct an importer/key provider.
These are signature references, not Secure activation.

Bind fixed owned holder method T Deny<T>(object a1,...,object a7), closed only
with the exact importer return Type, using Delegate.CreateDelegate. It always
throws a named unsupported exception. Verify actual delegate Type, single
invocation-list entry, exact owned closed Method/Target, seven Object parameters,
return Type and actual Func8.Invoke signature/parameter order. Actual Net48
relaxed binding is UNKNOWN until tested; incompatibility stops before vendor
construction. No DynamicInvoke, emitted/expression adapter, argument-value
inspection, invocation of argument delegates or default return.

## Construction order, existing progress and resources

Precreate validated non-null parent/context proxy references and importer holder/
delegate before construction and strongly retain them through all cleanup/final.
Reuse genuine trace, exactly two external-ownership application registrations,
Build and genuine Factory. Preserve the existing reviewed synthetic store-Id and
single cache-reference reply, shared16/saturating17 store budget and all stream
denials. No third registration, harness Resolve or positive new service value.
Preserve provider factory maximum2 entries/maximum1 success and shared resource
maximum8/saturating9 observations without resetting allowances mid-lifetime.

Factory remains window1. Invoke the exact Collection constructor once at window0,
with parent proxy, same genuine Factory and typed deny importer. Collection gains
neither resource fallback, cache-reference eligibility nor provider activation.
Audit dependencies and all old/new states immediately afterward. A throw, invalid
or proxy result, or caught callback denial stops before Project. Require the exact
original concrete Collection result, not a substitute or interface-only wrapper.

Only then invoke06002831 once in Project window2 with exact Collection, store,
same genuine Factory and deny context reference. Preserve the existing Project
Dispose validation/once-only cleanup when an exact Project returned. Use a
guaranteed-attempt try/finally structure: a failure of the disposal-begin record
must still attempt Project.Dispose once and preserve outer cleanup/audits.
Retain the
Collection through Project/container/trace cleanup and process-final; do not
invent Collection.Dispose. Constructor body entry on nonreturn remains
NOT_ESTABLISHED. A returned object is not DCA initialization or UI/state proof.

## Denial state, exclusion and audits

Parent/context/importer have independent strongly retained request counters and
sticky fault states, checked separately from provider/store/cache/dependency
latches. Parent/context counters saturate17, log at most16; importer saturates2,
log at most1. Every received invocation is unsupported, even the first. Count and
latch before message/context inspection, suspension or logging. Once a parent or
context count exceeds16, throw the named unsupported/budget error before any
incoming-message access or ReturnMessage construction; the saturated counter
must bound response-triggered reentrancy as well as logging. Preserve outer
resource exclusion/restoration on this hard refusal. All argument
values stay uninspected. Trusted runtime message/member identity checks may name
an observed callback but never permit business behavior. Malformed/null/custom/
throwing messages and foreign/Object/GetType/disposal requests stay denied;
a CLR operation not reaching Invoke is not an intercepted callback.

Exclude resource eligibility throughout validation/logging/error/response/restore.
Already-excluded nested callbacks must remain excluded; do not restore the outer
scope early. Every off-thread, early/late/nested/caught callback and recorder/
message/response/context/restore failure is sticky and independently fatal.
No state reset. Audit both counters and faults before later SDK phases, after ALL
managed cleanup and again at PROCESS_FINAL even on exceptional paths. Close any
activation flag without erasing history; retain proxy/holder/delegate references.

## Four-source ownership and required proof

Only experiments/dca-constructor/ProjectConstructorProbe.cs, ProbeLauncher.cs,
OwnedProjectConstructorFixtures.cs and Test-ConstructorProbe.ps1 plus fresh
private evidence. No metadata6, other constructor6, build/config/dependencies,
Rust/domain/protocol/persistence changes. Preserve old method/fixture bodies and
old mode behavior, except explicit localized additions to fixed mode predicates,
launcher/child routing, new-mode policy whitelist membership and new-state audit
hooks. Explicit phase-hook exceptions are the added CheckContextArguments calls
in CheckBuilderDependencies, WithEmptyContainerCore after Build, TraceDependencyGuard,
and InvokeFactoryForProject before/after Factory; each is a no-op outside the
new modes. The Main declaration-to-MainCore rename, STA Main wrapper and final
new-state hooks are also explicit exceptions. Owned injected log faults must use
a new-mode-only recording hook rather than change the legacy Record behavior.
List each exception in preservation evidence; do not
refactor old APIs to hide behavior changes. Existing eligibility predicates do not
automatically authorize Collection window0. New owned hooks belong to fixed new
cases/fixtures; never change old case meaning or allow an owned selector in an
original/legacy mode. At most64 fixed new owned cases; no automatic widening.

Owned actual .NET Framework4.8 proof must establish typed parent/context proxy
assignability and all received method-category denials, typed three-argument
Collection invocation, seven-reference-argument Func8 dispatch, and separate
four-argument context Project invocation. Prove normal owned construction with
zero new callbacks, existing Id/cache/provider progress, collection-before-project
order/window0 and Project window2, invalid/caught Collection failure preventing
Project, no unverified Collection disposal and correct Project/container/trace
cleanup/final retention.

Cover trusted API/delegate negatives, wrong/open/value argument profiles,
malformed/mutable/custom/throwing messages, null importer arguments, budget
boundaries, nested/off-thread/caught/early/late invocations and recorder/restore
faults across construction, error handling and cleanup. Include independent
counter-only and fault-only final-audit mutants for each of the three states,
with other latches clean. Each negative runs in a fresh process; malformed inputs
must not receive success. A cast/GetType not reaching Invoke cannot prove denial.

Run the three existing local CSharp5/x64/Werror variants with the same six
references, parser/guard/preservation/scope/diff checks and relevant owned suite.
Freeze source/snapshots/exact commands/results/retained failures; independent
local verification precedes fixed owned Windows orchestration. Actual owned
Windows tests in existing Session0, limited interactive and administrator-negative
contexts plus cleanup are required. Frozen source/owned-result critical review
and source commit precede any separately frozen one-attempt original plan.

Original/owned children keep15-second/64KiB bounds, strict dependency policy and
exact input/artifact preflight. Vendor internals remain UNKNOWN; network and
descendants NOT_CONTAINED. Process separation is not a sandbox. No original27
retry, new DLL admission, catalog feature, Secure activation, device action,
publication, task acceptance or gate advance follows from this source contract.
Original construction behavior and catalog independence remain UNKNOWN.

Pre-implementation checkpoint: source32 implementation/local/owned Windows/
original runtime NOT RUN. Task-009 remains In progress/all ACs unchecked.


## Initial source32 local checkpoint and pre-Windows review

Initial implementation was frozen at baselinec16d24da with freeze SHA-256
fc80be61e5cee0277a0265618e4e14a5079f442066e85365e7f2cf45865ba6e2
and source32 manifest SHA-256
d1e5d0fada8899f14552ed05e18edba1e31e2b19ab15d2e0b59ddb8f0391d8ac.
Independent verifier32/audit.txt SHA-256
e73c87479027f2c027b607473f24e6f2066651b7d50122aaaa38eb3694e6b4e5
PASS for the frozen local checks only: three compiler targets,14 state/audit
outcomes (eight normal plus six expected mutant refusals), six modern-runtime
holder proofs and preservation/scope/diff. No Net48/RealProxy proof follows.
Earlier failed guard/proof runs and packaging correction remain preserved.

Critical review32/pre-windows-source.txt SHA-256
aeccc760014d666055af4e32f88c186f42febf9a6eabfce7e0f56fd6fbcfdad6
requests changes: callback saturation did not stop message-dependent error
response construction, and a disposal-begin logging failure could skip the
Project.Dispose attempt. No Windows execution or source commit occurred.
This contract now makes both requirements explicit and names the already-needed
phase-audit preservation exceptions; it does not widen legacy behavior.

Add a finite owned reentrant-message regression proving count17 stops additional
property reads and outer exclusion restores, plus an owned disposal-begin-log
failure proving exactly one Project.Dispose and container/trace/final cleanup.
Retain the initial frozen sources/results/review unchanged as a checkpoint.
Fix only new-mode code/fixtures/tests, rerun affected local checks and freeze
fresh snapshots/results before independent verification/re-review and Windows.

The [Microsoft .NET Framework reference source](https://github.com/microsoft/referencesource/blob/main/mscorlib/system/runtime/remoting/message.cs)
(main, consulted2026-10-25, ReturnMessage exception constructor) shows incoming
message property reads during response construction. It motivates the bounded
reentrancy check; it is not a match assertion for the pinned host's mscorlib.
Actual target Net48 behavior must be exercised by the owned regression.


## Actual Source32 owned Windows result

Fix01 freeze SHA-256
`b2af8ccc7d50891913ed1876c5be0bc3ad2ff0b2e27e602bb8670af33e269c19`
and manifest `3da44af4b8d3b23d952336e4d28d726c97ce66fbda585611fb4712bf4565c1a6`
bind the four changed source/test files. Independent local verification PASS
(`verifier32-fix01/audit.txt`, SHA-256
`26ccf686a3b063ca4c4440c36f0ab7a81258258970f177cf99b9cde95d6f3956`);
pre-Windows source re-review PASS
(`review32/pre-windows-source-fix01.txt`, SHA-256
`c151b53c39de6462946bf6d14f6098c6597c818b3ac683fb48f8b40f9a12842e`).

The actual Windows .NET Framework build used compiler4.8.9221.0 and passed.
Owned suites PASS:212 Session0,418 limited interactive Session2 and217
administrator-negative Session2 cases. All49 context negatives and the separate
positive typed Collection-to-Project construction passed. The positive owns one
Project disposal, retains Collection without inventing its disposal contract,
and records one cache-reference reply and one successful factory activation.

Actual Net48 getter-reentry regression PASS:16 message-property reads,17 denied
entries, hard cutoff1, no sentinel execution and outer exclusion restored.
Actual disposal-begin logging-fault regression PASS:one Project, container and
trace disposal each, followed by independent final audits. Both original
review findings are closed at the owned-test scope.

Both scheduled tasks were removed, owned process counts were zero, no junction
remained;1039 limited and436 administrator log files were collected and matched
the saved remote size lists. Session0 and build stderr each contain616 bytes of
progress-only CLIXML; neither is described as empty. Limited/admin decoded
driver stderr is empty.18 compiled artifact pins were verified remotely before
each profile; compiled payloads were not downloaded or locally rehashed.

`private/evidence/task009-constructor-windows/owned32-observation.json`
(362399 bytes, SHA-256
`29f0b9140ede2b17e5d27a969b0fe4ac72fa8b04d1fa6cbdd00b87bca4f8a4c7`)
binds1658 local and82 native files. The initial saved-only recorder failed its
empty-build-stderr assumption before producing an observation; its exact
correction validates progress-only CLIXML. No Windows suite was rerun.

Commands actually executed by the coordinator from the repository root:

```text
python3 -B private/evidence/task009-constructor-windows/dispatch_owned32-corrected.py HELPER --orchestration-audit-sha256 c39caa0d61eeefa631f2e328bbaddcf3b22d841c63cf9142748994e87d059a95
python3 -B private/evidence/task009-constructor-windows/poll_owned32_02.py
python3 -B private/evidence/task009-constructor-windows/record_owned32.py
python3 -B private/evidence/task009-constructor-windows/record_owned32-corrected.py
```

The first command was invoked once for each exact helper:
`preflight_owned32.py`, `build_owned32.py`, `run_session0_32.py`,
`run_owned32.py`, `poll_owned32_01.py`, `collect_owned32.py`,
`cleanup_owned32.py`, `run_owned_admin32.py`, `poll_owned_admin32_01.py`,
`collect_owned_admin32.py`, `cleanup_owned_admin32.py`. Poll02 observed the same
limited task; it did not start another attempt. The first recorder failed as
recorded above; the corrected recorder passed.

Original-mode labels occurred only in guard negatives with owned staging;
`context-original-missing19` refused before input pin/hash/load. Original SDK
runtime, original Collection/Project behavior, SDK.Project adaptation and
get_Project success remain NOT RUN. DCA UI/state and hardware remain NOT RUN.
Owned execution used uncommitted frozen source at basec16d24da; the later source
commit must match those hashes and does not rewrite that execution history.
Task-009 remains In progress, all acceptance criteria unchecked, no gate advances.

Independent saved-result audit PASS:
`task009-constructor-windows/verifier32/owned-result-audit.txt`, SHA-256
`5e1d1c97e071b65e8a72fa6e249b630fee3328760aed596a97e0e7018875bed4`.
All1740 observation bindings match; independent raw-log checks confirm the
three profiles,49 context outcomes, both focused regressions and cleanup.
This audit did not rerun Windows or execute original SDK code.


## Actual Original32 explicit context attempt

Source commit `21e30df8da1902e89b74a788fb4a4e0dc46f6acc` exactly matches the
owned execution's frozen source bytes. The separately fixed original plan
`project32-plan.json` SHA-256
`c526a570f59272c4bf0e6a302334588ff8858b84c5dd14719f5d4b27e5805e26`
passed static audit `915c393f2c0255e3bb782bb0eb54813f2fe3cd9a3b415eac434f40b5e6977e63`
and critical review `9978e8a65c68b5e6e662cf55d40323b60cb2a9c08023d155004cdaf5023f893f`.
Live preflight verified19 private inputs, two OS pins and18 build artifacts.

The one actual Windows attempt **FAIL**, normal confirmed child exit2, with no
retry or timeout. Original trace construction, exactly two external
registrations, Build and Factory construction returned. Collection constructor
`0600298E` began in window0 and did not return; body entry is NOT_ESTABLISHED.
The resolver refused this previously unallowlisted identity:

```text
Knx.Ets.Xml.RegistrationRelevanceInformation, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159
```

Project constructor `06002831` was not reached. Parent/context/importer callback
counters and fault latches stayed zero/false, cache/provider callbacks and
resource probes were zero. Container and trace disposal each returned once.
The trace lifetime audit reportsFalse because the dependency was refused; this
is a failed attempt, not qualified construction. The online-catalog dependency
observed in the distinct Original27 route was not requested in this attempt.

Cleanup confirmed task removal, zero owned processes and empty scratch.
Post-check verified all19 private inputs and two OS pins unchanged, total
private input bytes24278056. Limited interactive Session2 was nonadministrator;
quser reported it disconnected, so no active desktop qualification is claimed.
The runtime still reports UserInteractive=true. Vendor internals remain UNKNOWN;
network and descendants NOT_CONTAINED. The harness invoked no DCA
initialization/UI/state or intentional hardware operation.

`private/evidence/task009-constructor-windows/project32-observation.json`
(19721 bytes, SHA-256
`9f8cf9af3b7c21c8977e0302abd13d4083145c26303cb7ad5cd6f5a707aa5f95`)
binds44 local files plus the native source/owned review.

Coordinator commands actually run from the repository root, each once:

```text
python3 -B private/evidence/task009-constructor-windows/preparation32/prepare_project32.py --receipt-sha256 c9f3c2abdd0f542707d4fc81c37ed18e020395af71bad9eda9cc5c6c1fa880c9
python3 -B private/evidence/task009-constructor-windows/run_project32_preflight.py
python3 -B private/evidence/task009-constructor-windows/run_project32.py 9978e8a65c68b5e6e662cf55d40323b60cb2a9c08023d155004cdaf5023f893f
python3 -B private/evidence/task009-constructor-windows/poll_project32.py 01
python3 -B private/evidence/task009-constructor-windows/collect_project32.py
python3 -B private/evidence/task009-constructor-windows/cleanup_project32.py
python3 -B private/evidence/task009-constructor-windows/run_project32_post_corrected.py
python3 -B private/evidence/task009-constructor-windows/record_project32.py --plan-review-sha256 9978e8a65c68b5e6e662cf55d40323b60cb2a9c08023d155004cdaf5023f893f --plan-audit-sha256 915c393f2c0255e3bb782bb0eb54813f2fe3cd9a3b415eac434f40b5e6977e63
```

Next bounded work is to examine the specific observed XML dependency using
existing private input provenance, then define a separate immutable diagnostic
profile if justified. The existing Original32 profile and failure stay fixed.
A dependency name does not establish a feature's necessity or behavior. Task-009
remains In progress/all criteria unchecked; genuine SDK.Project adaptation and
get_Project success are still outstanding.

Independent saved-result audit PASS for provenance and result consistency:
`task009-constructor-windows/verifier32/original-result-audit.txt`, SHA-256
`ec6cb2b523cd264b0fa1a8f69a501e3d4837cecd9fc8c616c990c04dbd82cf8b`.
The audit verified44 local observation bindings and the linked native review.
The constructor diagnostic itself remains FAIL2, with no Project returned.
