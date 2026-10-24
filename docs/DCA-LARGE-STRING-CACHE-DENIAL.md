# Task-009 deny-only large-string-cache reference experiment

## Readiness and single behavior

Task-008 is accepted; Task-009 remains In progress/all ACs unchecked, no gate.
Actual original25 FAIL10 remains the latest SDK runtime result: typed provider
registration, container Build and Factory returned, Project did not return,
and IProjectStore.get_LargeStringCache was denied once. Source26's separately
guarded original26b declaration result PASS and independent audit/critical
review PASS are recorded in [DCA-PROJECT-STREAM-STORE.md](DCA-PROJECT-STREAM-STORE.md),
result commit47ab8a0. That result executes no SDK method or cache behavior.

Private readiness task009-stream-store-deny-readiness/readiness.md,
SHA-256388422fcccbe95bdba90aa544736b53b83034ccc284c3f9b24f1cde6dc6a71d1,
identifies sufficient declarations to design the next bounded experiment.
Its feasibility, typed proxy marshaling and the first actual stream callback
remain UNKNOWN. This increment permits exactly one diagnostic service-reference
reply, with every received stream method denied. It implements no cache service.

Use a new fixed mode pair original-project-large-string-cache-denied and
owned-project-large-string-cache-denied. Preserve source25 and older modes.
Keep the genuine trace registration and typed Undo-provider factory: exactly
two application registrations, no third registration and no harness Resolve.
Precreate one diagnostic-owned deny-only IStreamStore proxy/reference after
shape validation and before constructors. Retain it through all cleanup and
process-final. Never call its methods from the original harness.

## Verified runtime identities

ObjectModel input remains14,204,880 bytes, SHA-256
73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a,
Knx.Ets.ObjectModel6.4.8718.0/neutral/token61439873ec5e1159. Validate its
IStreamStore02000061 public nongeneric interface, attributes1048737, no base
or direct interfaces, and precisely these seven declared method shapes:

| Token | Member | Return | Parameters | Attributes |
|---|---|---|---|---|
| 06000366 | get_Id | String | None | 3526 |
| 06000367 | get_Exists | Boolean | None | 3526 |
| 06000368 | AsStream | mscorlib4/System.IO.Stream | Boolean | 1478 |
| 06000369 | AsMultiWriteWithinOneTransactionStream | mscorlib4/System.IO.Stream | None | 1478 |
| 0600036A | Delete | Void | None | 1478 |
| 0600036B | GetUtcFileTime | Int64 | None | 1478 |
| 0600036C | set_Mask | Void | UInt16 | 3526 |

All are instance/default/header32/generic0. Validate exact Stream Type identity
from mscorlib4.0.0.0/b77a5c561934e089; instantiate no Stream. The only new
allowed getter is IProjectStore02000057.get_LargeStringCache06000313,
attributes3526, instance/header32/generic0, zero parameters, returning the exact
validated IStreamStore Type. The IObjectContext getter returning a concrete
cache class is unrelated and remains unsupported.

Before allowing a reply validate trusted runtime MethodInfo kind/module/token/
Equals, exact declaring type/assembly/flags and return Type reference identity,
public instance/special-name/nongeneric shape, MethodName, ArgCount/InArgCount
and nonnull empty Args. Reject custom/transparent MethodInfo and malformed
messages without trusting arbitrary virtual metadata getters. Keep existing
synthetic ProjectId fixture behavior and label the new reference diagnostic.
Return the same preexisting reference, never a fabricated class/GetType, null
business success, path, Id/Exists/time/Mask value, stream bytes or void success.

## Budgets, exclusion and final audits

Select the stricter budget: at most ONE successful cache-reference reply,
counting toward at most16 total new-mode project-store callbacks including Id;
total counter saturates17 and cache-attempt counter saturates2. A second cache
request is refused and sticky; do not widen automatically. Count before message
inspection or suspension. A normal diagnostic reference reply is not a denial.

Allow the reference only on the original owner thread, during the active exact
Project constructor window2. Never during trace construction, registration,
Build, Factory window1, validation/logging/error handling or any disposal/late
phase. Snapshot trusted eligibility before suspension, then exclude resource
eligibility through inspection, record and ReturnMessage creation. Restore in
finally. Already excluded nested calls must remain excluded and fail without
clearing the outer state. Handling/record/response/context/restore faults latch
independently, including faults caught by SDK code.

The stream proxy denies EVERY received callback: all seven interface methods,
Object/GetType/ToString/Equals/GetHashCode, foreign, malformed and disposal-shaped
requests. Count first, saturate17, log at most16. Inspect no callback argument
values and evaluate no stream properties; trusted message-shape/method-identity
checks remain permitted. Identify the first method only using trusted runtime identities; no
arbitrary custom MethodInfo metadata. Recorder/message/restore faults remain
counted and sticky. A cast not reaching Invoke is not an intercepted callback.
No fabricated IDisposable contract, disposal success or stream/provider Dispose.

Audit new wrapper counts/faults and stream denials independently of existing
provider/Kim/store/factory/dependency latches, at applicable phase boundaries,
after ALL managed cleanup and again at PROCESS_FINAL. Keep the proxy/reference/
wrapper strongly retained throughout. Project Dispose only if Project returned.
Caught or late callbacks must prevent success. Counters do not establish
asynchronous containment or timing guarantees.

Preserve the exact resource owner/two names, NULL requester UNKNOWN, shared8
Factory/Project windows1/2 and callback exclusion. No allowance reset. Keep
19 private/two OS pins,18 artifacts, existing6 compiler references/config,
15-second/64KiB child bounds and factory cap2/maxsuccess1. Vendor internals
UNKNOWN, network/descendants NOT_CONTAINED; this is no sandbox qualification.

## Source27 ownership and tests

Own only experiments/dca-constructor/ProjectConstructorProbe.cs, ProbeLauncher.cs,
OwnedProjectConstructorFixtures.cs and Test-ConstructorProbe.ps1 plus fresh
private evidence. No metadata6, other constructor6, dependency, build/config,
domain, protocol or persistence change. The bounded reuse exception permits
unsealing ProjectStoreIdProxy and making InvokeCore protected, preserving all
old bodies byte-for-byte. A new-mode subclass may call protected InvokeCore for
Id inside ONE exclusion scope; never base.Invoke under an already suspended
scope. Preserve old counts/behavior and audit new wrapper faults separately.
Use distinct owned stream/store fixture contracts and fixed owned-only routes;
do not silently alter old fixture families or broaden original selectors.

Prove actual typed getter dispatch/same-reference return on .NET Framework4.8,
existing Id behavior, strict one-reference/shared16 budgets, all seven stream
and received Object/foreign/disposal denials. Cover forged/custom/throwing
messages, exact method-shape negatives, recorder/restore faults, nested,
off-thread, outside and late callbacks, caught faults in constructor/error/
Project/container/trace/process-final cleanup. Use fresh processes for sticky
cases. Include independent final-audit mutants proving stream-counter-only and
wrapper-fault-only failures remain fatal with every other latch clean; normal
reference replies alone pass. No positive cache/store business values.

Run local CSharp5/x64/Werror builds with existing6 references, parser/guards/
preservation/scope/whitespace checks and full relevant owned suites. Freeze
source/evidence before independent verification. Actual Windows owned suites
and critical source/result review precede the source commit. Only afterward
may a separately frozen, reviewed one-attempt original runtime plan be prepared
under the existing bounded diagnostic authorization. No original25 retry or
vendor runtime follows directly from this contract. Unsupported marshaling or
new callbacks are observations, not permission to fake a service or expand scope.

ADR-005 private-copy permission/proprietary redistribution prohibition remain.
Status: source27 implementation/tests/original runtime NOT RUN. Task-009 stays
In progress, all ACs unchecked; no device operation, publication or gate advance.

## Source27 local checkpoint

The four assigned source paths are implemented and frozen by private
task009-large-string-cache-denial/source-freeze27.json,15,365 bytes, SHA-256
722bc379ff25041a5cadc6ee3f1f994d384c1c408ab439f719c909ebc953b505.
It maps10 current sources/runtime8, ten snapshots and the contract checkpoint.
source27/evidence-manifest.json, SHA-256
6279d359f0259a8149a28c6caeb70d236734b0676f6b3a141f0a388c0882cf81,
binds67 owned source/check artifacts. The freeze accurately records uncommitted
source27 at base47ab8a0; no source commit or original execution is asserted.

Actual local checks PASS: three CSharp5/x64/Werror builds against existing6
references, PowerShell parsing/legacy/new source guards,12 isolated exact-state
cases, normal final audit and two independent expected-fail audit mutants
(stream-counter-only and wrapper-fault-only), preservation, scope and whitespace.
Preservation covers16 old arrays,100 methods,145 case lines, old proxy cores
with the two authorized declaration exceptions, metadata6 from freeze26 and
the other6 constructor files from freeze25. Exact commands/raw outputs are
bound by the manifest. An initial stale driver-policy guard failure and a
preservation-harness method-name collision remain retained separately; final
checks PASS. No Windows CLR/RealProxy result follows from these local tests.

Independent verifier27/independent-audit.txt, SHA-256
acff7d43e40649a781fc098026786d8cf3bc0d64f3cf63951562eaca19673fbd,
PASS:10 source pins,67 evidence bindings and reproduced compiler/parser/guard/
preservation/scope/whitespace checks plus all15 expected state/audit outcomes.
The verifier used no SDK payloads, Windows invocation or hardware.

Contract review27/contract-review.txt2c747e67190925997c92f3fd4caee4f8275a454001c5880472d14530d862a2d3
and one-wording-delta addendum9c43bcd58a67e1b13bef55f640d24fcb71fb5232c5b8f009ed11c16f3e1a8806
PASS. The addendum binds the final contract checkpoint4843cb00644599709420d3fac4c92e999db1bcc3cff18843844caa417a83e5b8.

The new wrapper uses an owned snapshot of validated zero-argument message
facts before entering the preserved Id core, avoiding rereads of adversarial
virtual getters.50 fixed owned cases include typed return/reference identity,
mutable cache/Id messages, recorder/restore faults and independent late audits.
Their actual .NET Framework dispatch and three Windows suite contexts remain
NOT RUN. Source27 is not an SDK/DCA/cache compatibility result.

Owned Windows preparation is separate and not executed at this entry:
constructor-windows/source27-copy-manifest.json0f7f136f6080742baeab9dfcf44fbf9b881255d4246ccdd22e041b16280a90ad
binds68 copied artifacts; orchestration27-preparation.jsonb30a80fd1981eb861be15c957a4035e9ab0771fe53c41bec7c898e9568a877ab
pins11 exact derived owned helpers. An exclusive dispatch marker prevents
repeating a helper after failure; frozen source/copy/adapter/template/audit pins
are checked first. Static orchestration verification is pending. No original
SDK mode, original runtime retry, device operation or gate acceptance follows.

## Source27 actual owned Windows results

Static owned-orchestration audit constructor-windows/verifier27/orchestration-audit.txt,
SHA-2561e8e4b58a581b613034adf11a845cda8cc7e7b0a070cd361e457cf4f4430c569,
PASS before execution. Actual preflight PASS:19 absent new paths, x64 OS/process,
ETS processes0 and owned processes0. The existing owner-established SSH master
was live; no reconnection or new host authorization was requested.

Actual Windows build PASS with compiler4.8.9221.0,
SHA-25646809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00,
eight frozen C# sources and18 size/hash-checked artifacts. Actual owned suites:

| Context | Cases | Result |
|---|---|---|
| Session0, noninteractive | 194 | PASS |
| Session2, interactive nonadministrator | 350 | PASS |
| Session2, interactive administrator negative | 199 | PASS |

The typed .NET Framework4.8 getter returned the same precreated proxy. Normal
reference case, exact shared16 boundary and mutable cache/Id snapshots PASS.
All50 fixed cases have their expected exits. Seven stream and four Object
method identities are recorded exactly and denied without values. Recorder,
restore, malformed/forged messages, reentry, wrong-thread, phase and late/final
faults remain fatal. The stream-only and wrapper-only final cases report
otherwise clean dependency flags, complementing the separate local audit mutants.
These are controlled fixtures, not SDK cache behavior or an original DCA result.

Both scheduled tasks completed exit0, then were removed with owned processes0
and no junction.854 limited and401 administrator case files were downloaded
with their recorded sizes. The limited task was observed Running once and then
Ready/0/done/0 processes via a separate read-only poll02; no job restart occurred.
Every command/tool session is terminal. A recorder checks all source pins,
18 build artifacts' saved pins, raw/decoded logs, context/done/cleanup, prior
Undo/provider cases and new cache cases. Compiled payload bytes were not
independently downloaded/rehash-verified locally; remote preflights checked them.

The first private constructor-windows/owned27-observation.json,285,476 bytes,
SHA-256f82ba4cba7d93c083aa91c01a39783c9e31b5cb3590c20bf5051f9aba29dc4da,
bound1,439 files but incorrectly retained a prior25 top-level filename filter.
Its profile/case checks passed, while the artifact list mixed prior25 top-level
files with current27 directories and omitted some current27 preparation/raw
files. The original recorder dff805e0ca4cb4d1d2d35d353c94379e86dbf653bf244cff55a0c600632861b7
and first audit3abe78e9e26a259aa6fb05647091475b21dcc4f67835eb7431bef3d069862ae2
remain preserved; they are superseded for current27 artifact coverage.

Corrected owned27-observation-corrected.json,282,800 bytes, SHA-256
7ed37fba6aa0afbbbfc89bbd0359d65de0aafcc43d5d181158d0c7ac9ed2edf1,
binds1,421 current27 preparation/build/raw/case files, including poll02 and the
first observation as historical provenance. Corrected recorder SHA-256
30e7bfed9fb167b39da780c24430d24e8982666ff67b8c143fcb34300625794b
changes only that selector, exclusive output name and console label; every
source/result assertion remains unchanged. It ran once, PASS. Correction
provenanceff247142b15224e120c48fc32c3987d72fbc4e64ea8b441d7689022667db7525
binds both observations/scripts. No source change or test/runtime rerun occurred.
Corrected saved-result verification and critical actual-source/owned-result
review are pending at this checkpoint. Source remains uncommitted; original27
SDK execution, UI, state and hardware NOT RUN. Task-009 remains In progress.

### Corrected owned-result verification

constructor-windows/verifier27/owned-result-audit-corrected.txt, SHA-256
5c53e388c37b63e6194b49d80343eb418b6f11fb0f00c25330576411b45cc446,
PASS: all1,421 corrected bindings, exact three recorder differences, old
observation/correction provenance, unchanged outcome fields, current source
pins, three profile counts,50 cache outcomes and the now-bound poll02 artifacts.
The original audit remains unchanged. The verifier used saved data only, with
no Windows/SDK rerun or payload access. Critical actual-source/owned-result
review and source commit remain pending; original27 runtime NOT RUN.

### Source27 critical source and owned-result review

constructor-windows/review27/critical-source-and-owned-results.txt,5,361 bytes,
SHA-256d386775be84f4c9a53f858226e2f65148d2c19dc309f8f4e408f5932a93f8e03,
PASS, no unresolved findings. The reviewer checked the actual six-path diff,
10 current/snapshot pins, unchanged metadata6, frozen contract,67 native and
1,421 corrected Windows bindings, all50 cache-case audit oracles and11 exact
denial identities. Recorder correction is exactly the three recorded changes;
non-file result fields are unchanged. No tests/SDK runtime were rerun and no
compiled payload bytes were independently downloaded or rehashed by review.
This accepts the bounded owned increment only. The exact source commit must
precede preparation/review of a separate immutable original27 runtime plan.
