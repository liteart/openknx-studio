# Task-009 typed provider factory investigation

Status: investigation only; integrated implementation NOT RUN. Task008 is
accepted and no phase gate applies. The [superseded type-only investigation](DCA-UNDO-PROVIDER-TYPE.md)
records why RegisterInstance cannot provide this interface proxy faithfully.
Original22 remains FAIL2 before container Build/Factory/Project; no original
SDK retry has occurred. All provider business operations remain unsupported.

## Candidate and primary evidence

The existing public Autofac Register<T>(ContainerBuilder, Func<IComponentContext,T>)
API creates a delegate activator with truthful LimitType=T. RegisterInstance
instead derives LimitType from instance.GetType(), which yields the genuine
MarshalByRefObject representation for the observed interface proxy. The pinned
public CreateRegistration service-assignability check rejects the latter.
The typed factory can return the same already-created deny-only proxy without
inventing its GetType response or implementing any store/path behavior.

Public source is pinned to Autofac07cac9047f4059de2d72834042c9649cbd228085;
exact files/hashes and source-only qualification are in the type investigation.
Cached Register<T>, SingleInstance and ExternallyOwned declarations establish
the candidate API, not actual binary compatibility or SDK activation success.

A minimal owned program tests actual public API shape, old proxy GetType
refusal, genuine-type RegisterInstance Build rejection with an exact nonlocalized
throw-origin oracle, and typed delegate registration/Build/two own-interface
resolutions. It must verify same proxy/factory invocation1, RootPath denial1,
no container-driven provider Dispose and no selected vendor assembly. It also
pins the actual loaded Autofac identity/location/hash. These own-interface
resolutions do not authorize any original SDK Resolve or device operation.

## Actual24 compile result

One separately frozen/reviewed Windows plan was executed. The new output paths
were fresh. The compiler exited1 normally with direct-child exit confirmed;
1,989 aggregate captured bytes, stderr empty. The actual program was **NOT RUN**.
The observed errors are:

- CS0104: RegistrationExtensions is ambiguous between the Autofac and
  Autofac.Builder namespaces; the source needs its precise public owner.
- CS0012: System.Core4.0 is required by the exposed Func signature/reference graph.
- CS1701/CS1702: the supplied public BCL9/Tasks4.2.1 references differ from
  Autofac's requested versions; warnings-as-errors refused the build even
  though the existing six runtime redirects are present.

This is a test-build failure, not an actual provider/SDK runtime failure or
proof of successful registration. No original SDK or hardware was selected.
Post-run nine public inputs, six framework references, compiler, source and
plan hashes PASS unchanged. No automatic retry, dependency fallback, warning
suppression or SDK access occurred. Network/descendants remain NOT_CONTAINED;
no full cleanup claim is inferred from compilation/exit evidence.

Private `task009-undo-provider-delegate-proof/delegate24-observation.json`
(3,312 bytes), SHA-256
`a38b6ea9b8134a11d8416b640f7065e6ea81e7632c510c0acdbe21349dc9d8b1`,
binds16 source/plan/review/raw/report files. Exact executed command:
`python3 -B private/evidence/task009-undo-provider-delegate-proof/run_windows.py`.
Plan SHA-256 `f1eab79a0b84316be0a74c031787b9dad8e09301bcdf3d0367b250cebba3ca3b`;
source SHA-256 `76f195f5b70c4868e2f9089dfdb2818d64b8e346765e46e086591ff7db99c01b`.
Independent pre-execution verification and critical bundle review PASS; the
actual failure is retained separately and is not relabeled PASS. Independent
saved-result verification PASS, all16 bindings and terminal-envelope bytes:
private task009-undo-provider-delegate-proof/verifier/windows-result-audit.txt
(under private/evidence/) SHA-256
`4c72d3d3d0d0fd3c453d3054624ed9c37f4641c6c59f4095188a0a0a591adf22`.
No compiler or runtime was rerun by this audit. Critical result/documentation
review PASS, no unresolved findings: same private proof root
review/windows-result-review.txt (3,826 bytes), SHA-256
`4308a4f8d7721460272f205eefbddf2bf71a3558359609892122d2337869638b`.

## Corrective proof preparation

Version24b uses a new source/plan/output directory; the executed24 bundle is
immutable. The candidate corrections are namespace qualification, one compile-
only System.Core reference from the same existing Microsoft net48 reference
package, and explicit compiler /appconfig pointing at the unchanged public
binding configuration. Warnings-as-errors remains enabled. The actual24b result below shows that this compiler option did not resolve
these three warnings.

[Microsoft's compiler documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/appconfig-compiler-option)
describes /appconfig as the configuration input for assembly binding.
The existing package is Microsoft.NETFramework.ReferenceAssemblies.net48 1.0.3,
SHA-256 `8a7e348538e7eb91351696911689f49e3d4f63f8bab517432bbe159b8b1104a2`.
All six prior reference DLLs were byte-compared to that same archive, PASS.
System.Core reference:256,560 bytes SHA-256
`94a1fc97eb0d36f13acd209bb7207da504c8be10caf7e5096b2c46e671dc6fe2`;
exact entry and package metadata in private task009-system-core-reference.
This is a compiler reference, not a reference assembly copied for runtime use.
No new package download or vendor SDK reference is introduced.

Required next evidence: corrected frozen proof/source/runner review, actual
Windows compile and owned public API proof, then a separately bounded integrated
factory contract/implementation with negative/lifetime tests. No Task009 AC,
gate, provider behavior, SDK context, UI/state or hardware qualification follows
from these preparatory artifacts. ADR005 private use/no proprietary DLL
redistribution remains unchanged.


## Actual24b result and reflection-based corrective proof

Version24b corrected the namespace and System.Core errors. Its actual compiler
still exited1 with exactly the three CS1701/CS1702 version-policy diagnostics
(1,236 captured bytes, stderr empty). /appconfig did not remove them under the
pinned compiler. Warnings-as-errors stayed enabled; no warning was suppressed.
The actual public API program remains NOT RUN. Post9 public/seven reference/
compiler/source/plan pins PASS unchanged.

The first delivery exceeded Windows command-line length and did not start the
runner. A separately reviewed short transport checked the four uploaded file
pins and, authoritatively, the absent output root before the single runner
invocation. Start and read-only collection used separate short SSH commands.
The immutable source/plan/runner were not changed or retried after execution.
Exact actual command: proof-v2/run_windows_short.py under the private
task009-undo-provider-delegate-proof-v2 folder. Full original long-delivery
outputs and the short preflight/start/collection are retained separately.

The25-binding delegate24b-observation.json (5,088 bytes), SHA-256
`5ace2fedfb864136c5c7ab74dd693492f33f521ab9ada73f6cb7b82d0a4a6259`,
records compile FAIL, proof NOT RUN and post-pin PASS. Independent saved-result
audit PASS, all25 bindings and terminal-envelope bytes:
private proof-v2/verifier/windows-result-audit.txt (under
task009-undo-provider-delegate-proof-v2), SHA-256
`803f3379948044e1a7f3c8c0e28600fe093147746e33da6932e659843ceec202`.
No SDK runtime behavior,
network/process-tree containment or full cleanup is claimed.

The next proof uses the same fixed public runtime reflection bridge intended
for the host: exact pinned Autofac is loaded from the isolated AppBase and its
public type/method owners and shapes are checked before invocation. The same
real Register/Build/Resolve/denial/lifetime cases remain required. Public9
binaries remain runtime inputs; compiler references are only standard framework
assemblies, so this does not suppress warnings or change the supplied versions.
Version24c is preparation only; actual public API and integrated SDK behavior
remain unqualified until the corresponding genuine runtime evidence exists.


## Actual24c public runtime proof

The fixed reflection proof compiled with warnings-as-errors and **PASS0**, no
compiler output. Its actual program also **PASS0**,2,006 captured bytes, stderr
empty; both direct-child exits confirmed. One runner attempt, fresh paths and
post9 public/seven reference/tool/source/plan integrity PASS. Exact loaded
Autofac8.2 identity, isolated AppBase location and hash were checked in the
program before API use. The real public RegisterInstance/typed Register and
SingleInstance/ExternallyOwned/Build(None0)/Resolve methods matched the fixed
reflection shapes; no public-library compile reference or warning exception
was used.

Observed assertions: old RegisterInstance requests GetType and is denied once;
the genuine MarshalByRefObject type-only route reaches Build and throws exact
ArgumentException at RegistrationBuilder.CreateRegistration0600074B. Typed
factory registration succeeds, remains lazy through Build, and two own-interface
resolutions return the same existing proxy with factory invocation1. RootPath
is denied1, GetType/provider Dispose calls0, and container disposal completes.
These are own IProbe assertions, not original provider/SDK behavior.

Private task009-undo-provider-delegate-proof-v3/delegate24c-observation.json
(4,807 bytes), SHA-256
`912722bee7729ec3d9fe24212d354ca3d5ac253d1ef998551a595444e25fe4b4`,
binds21 source/plan/review/report/raw files. Exact executed command:
`python3 -B private/evidence/task009-undo-provider-delegate-proof-v3/run_windows.py`.
Independent saved-result audit PASS, all21 bindings/envelope bytes:
verifier/windows-result-audit.txt SHA-256
`e7530e37cec4f2925c27ada2b6208131d3a9603a0f5e97c77b143fba76a1fcbb`.
Inventory11 names contains no vendor name; individual BCL/OS assembly locations
are not attested by that inventory. Network/descendants NOT_CONTAINED, no full
cleanup or original SDK qualification. Task009 remains In progress/all ACs unchecked.

## Integrated source25 contract

Use new fixed modes original-project-undo-provider-factory and
owned-project-undo-provider-factory. Preserve source22 modes and evidence;
source23's held patch and public24/24b failures stay historical. Implementation
ownership is only ProjectConstructorProbe.cs, ProbeLauncher.cs,
OwnedProjectConstructorFixtures.cs and Test-ConstructorProbe.ps1. Preserve
build/config, metadata6, other constructor6,19 private/two OS input pins,18
artifacts and15-second/65,536-byte original child limits. No new library.

Before effects, validate exact pinned original21 provider shape through the
existing verifier and exact public Autofac factory-registration shape proven
above. Close Register<T> with that exact interface; argument is actual
Func<IComponentContext,T>, generic constraint flags0/no constraints, result
IRegistrationBuilder<T,SimpleActivatorData,SingleRegistrationStyle>. Validate
public parameterless SingleInstance/ExternallyOwned returning that same closed
interface. Register genuine trace as before, provider by typed factory, then
SingleInstance and ExternallyOwned; both must return the same builder before
Build. Exactly two registrations. No original harness Resolve or new SDK method.

Use the existing unchanged deny-only provider proxy, including its GetType
denial. An owned holder captures only that already-created proxy and exact
contract. Bind its fixed generic instance method T Provide<T>(object context)
with Delegate.CreateDelegate to the actual closed Func type, as proven24c;
check delegate target/method/type. No expression compiler, fabricated class,
GetType reply or runtime code derived from product input. The callback checks
context nonnull without inspecting, retaining or invoking it and returns the
same proxy cast to the exact T; all3 provider business methods stay denied.

Count every factory entry before validation/exclusion, saturating2; allow at
most one success. Any second, malformed, null-context, off-thread, nested,
stale/outside-lifetime/constructor-window call or handling fault latches a
sticky violation before throwing. Eligibility is same owned thread and exact
active Factory/Project constructor window1/2; registration, Build and cleanup
are ineligible. Suspend resource eligibility throughout the callback, including
recording/cast/return, restore in finally and latch restoration failures. No
argument values or context methods. Keep holder/proxy/delegate alive through
all managed cleanup and process-final audits; never reset counters/latches.
Final guards reject factory faults independently of provider/Kim/store denials;
a legitimate factory success alone is not a provider denial. Retained late
calls cannot lose their evidence after scopes close.

Owned regression coverage: exact factory API/delegate negatives before effects;
two registration order/ownership/sharing; first eligible activation returns
the exact proxy and accepts count1; null, second/saturated, nested/off-thread
and each ineligible/lifetime phase fails; all3 provider calls remain denied;
caught factory faults remain fatal through error/Project/container/trace/final
cleanup; callback resource suspension/restoration and legacy modes preserved.
A counter-only final-audit proof must fail when its guard is removed, without
relying on another dependency latch. Each sticky case uses a fresh process.

Progression: local CSharp5/x64/warnings-as-errors compilation against existing
six host references, parser/guards/preservation; independent verification;
actual owned Windows Session0/limited/admin-negative suites and critical
source/result review; source commit. Only then may a separate frozen/reviewed
one-attempt original25 plan be prepared under the existing bounded diagnostic
authorization. This contract does not authorize new hardware, publication or
a gate advance. Integrated implementation and original25 are NOT RUN.


Critical24b/24c result and integrated source25 contract review PASS, no blocking
findings. Private task009-undo-provider-delegate-proof-v3/review/
results-and-source25-contract.txt (4,111 bytes), SHA-256
`e038a0a2e349c8033f094a484bebee586d74bc01902c5395294b2cf5bef87df5`.
Reviewed contract checkpoint SHA-256
`ec77e93324a723d8ecaf4b867000c900bf5a60024e8dc5f314fbe646a2f6ec59`;
this review/audit-record append is later. Reviewer independently checked25+21
result bindings and bounded contract claims without rerun or SDK payload reads.
This enables the specified source25 implementation only; original25 still
requires its later checks/review/commit/immutable plan, no gate acceptance.
