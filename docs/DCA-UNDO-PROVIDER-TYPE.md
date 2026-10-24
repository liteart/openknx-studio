# Task-009 exact CLR type introspection

**Status: superseded compatibility investigation. Do not implement the
RegisterInstance/type-only approach below.** The pinned Autofac source check
below establishes a second incompatibility at container Build. No original
SDK retry or Source23 implementation is justified by this historical contract.

Readiness: Task008 accepted, no phase gate. Actual22 at source72bca2e FAIL2
denied System.Object.GetType during undo registration before Build or any
Factory/Project constructor. Exact internal caller unknown. Preserve source22
and all earlier modes/evidence. The DCA readiness investigation supports this metadata-only adaptation.
Implementation and integrated Windows validation are not yet run. The separate
minimal CLR inquiry below disproves the initial interface-equality assumption;
the current contract returns the actual observed CLR representation instead.

Use separate fixed original/owned-project-undo-provider-type modes. Only these
modes may return the actual RealProxy.GetProxiedType() for exact System.Object
GetType instance/public/nongeneric/no-parameter/System.Type-return identity.
Check MethodBase identity as well as MethodName, ArgCount, InArgCount and
signature metadata, nonnull empty Args and exact trusted runtime MethodInfo
concrete kind/module/token/Equals. Reject transparent or custom MethodInfo
without trusting its metadata getters. A same-named provider/member or malformed
message is not eligible. Do not inspect argument values. No recursive transparentProxy.GetType.
The declared provider service interface and CLR representation are distinct.
On the observed Framework profile, base.GetProxiedType() returns the genuine
System.MarshalByRefObject Type, not the interface passed to the proxy constructor.
Validate actual reference identity to typeof(MarshalByRefObject) for this profile
and retain the exact declared provider interface independently. ReturnMessage
must carry that actual base.GetProxiedType() object, no out arguments, and the
original call. Unexpected representation fails; never substitute the interface
or manufacture a concrete provider class merely to satisfy registration.

Retain separate bounded total/metadata/denial counters and sticky violation
state. Count before inspecting any untrusted message property; exceptions while
validating, recording or constructing a reply, reading LogicalCallContext or restoring exclusion must latch denial. At most8
successful type replies per process; ninth refuses and saturates. Unsupported
methods and malformed inputs always increment sticky denial before replying or
throwing. Final audit after all managed lifetimes and process-final rejects any
denial; successful bounded metadata replies alone do not imply a denial.
Keep callback resource windows suspended throughout inspection/response and
restore in finally. Do not reset sticky state; use fresh processes in tests.

All three provider interface methods remain denied; no store/path/null success,
GetHashCode/ToString/Equals success, dynamic reflection dispatch or new DLL.
No provider Dispose is fabricated. Preserve exact two external registrations,
trace lifetime, constructor windows, aggregate resource budget8,19 private/two
OS input pins,18 artifacts,15s/64KiB runtime bounds and previous denial behavior.

Owned registration fixture must actually call ((object)instance).GetType() in
its new mode before registration returns, to reproduce the actual22 boundary.
Prove old deny-only proxy refuses that owned call; new mode returns the exact actual CLR
representation (MarshalByRefObject for this profile), preserves the separate
exact service interface registration and completes the owned registration. Also test direct transparent proxy
call and actual represented-type reference identity, all provider denials, other Object methods, malformed/forged message
shapes, throwing accessors/recorder, callback/nesting suspension, metadata limit,
caught denial through all registration/construction/error/disposal paths and
counter-only final-audit mutation. Old modes and artifacts retain their tests.

Implementation ownership: four constructor experiment files only:
ProjectConstructorProbe.cs, ProbeLauncher.cs, OwnedProjectConstructorFixtures.cs,
Test-ConstructorProbe.ps1. Build/metadata/dependency/config/other sources unchanged.
No original SDK runtime in implementation. C#5/x64/Werror same six pinned net48
references; local parse/guards/preservation; independent verification; actual
Windows owned Session0/limited/admin-negative checks; critical review and source
commit before any separately reviewed immutable one-attempt original plan.

Primary contracts checked 2026-10-24:
[RealProxy.GetProxiedType](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.remoting.proxies.realproxy.getproxiedtype?view=netframework-4.8.1)
[ReturnMessage constructors](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.remoting.messaging.returnmessage.-ctor?view=netframework-4.8.1)
The pinned [Autofac activator source](https://github.com/autofac/Autofac/blob/07cac9047f4059de2d72834042c9649cbd228085/src/Autofac/Core/Activators/ProvidedInstance/ProvidedInstanceActivator.cs)
at commit07cac9047f4059de2d72834042c9649cbd228085 calls GetType(instance)
from its constructor; the helper returns instance.GetType(). The cached file
SHA-256 is `96055559f3e845b64c079148d411bd1776dcb279a58d890b8d267d19d7915ebe`,
bound by private task009-autofac-registration82/source-observation.json. This explains a plausible registration mechanism; it does
not identify the uninstrumented actual22 internal caller.

No gate advances; Task009 stays In progress/all ACs unchecked. Provider/store/
path semantics and IStreamStore declaration UNKNOWN; no UI/state/device proof.
Vendor internals UNKNOWN/network and descendants NOT_CONTAINED. Owner private
copy allowed/proprietary binary redistribution prohibited, ADR005 unchanged.


Historical initial contract review PASS, before the minimal CLR inquiry:
private `task009-undo-provider-type-contract-review.txt` (6,892 bytes), SHA-256
`a9252b2766fcb4a7a6fd36f66b8ff1b221e2611dfe0cddb524153538f24f90ba`.
Reviewed contract checkpoint SHA-256
`88a2f3cae1c7fd04ffbdef5497c2bb71e11636252a9ae6d86eea73be78cd0f5e`;
this review-record append is later. The final guards must use denial/fault
state rather than total callbacks; bounded successful metadata replies alone
must not trigger the old deny-only failure. This review supplies no runtime
qualification or operation approval.


## Actual minimal CLR inquiry and contract correction

The initial interface-equality requirement was an incorrect compatibility
assumption despite the static review. One owned Windows proof failed with
OWNED_TYPE_MISMATCH; actual type was not logged in that first proof. The failed
proof is retained unchanged by private constructor-windows/
clr-type23-failure-observation.json (2,236 bytes, SHA-256
`9b6bb45616491fd9bd5cdc7ed75e81ceb188ddb0963cc7e38d01deb348e08d40`).
The preliminary sandbox delivery was denied before remote access; a separate
authorized delivery used the existing SSH master and executed the proof once.
This is not a denied automatic approval review.

A newly named and separately reviewed owned inspection then PASS: actual
base.GetProxiedType() was System.MarshalByRefObject, mscorlib4.0.0.0, token
b77a5c561934e089. The caller received that same actual Type object via
ReturnMessage; the old proxy still denied GetType once, and the new proof denied
RootPath once. Runtime CLR4.0.30319.42000, PowerShell5.1.26100.9711, x64, Session0.
No SDK was selected and no original SDK attempt occurred. Minimal-proof PASS
is not integrated Source23, SDK registration or provider qualification.

Inspection observation: constructor-windows/clr-type23-inspect-observation.json
(2,039 bytes), SHA-256
`f8b49c87cc87ce8be0f3a8caee69b817a9dcf9412e68cadf9874028d5459fad7`.
Exact fixed commands: `run_clr_type23_proof_authorized.py` (runtime FAIL1) and
`run_clr_type23_inspect.py` (PASS0); preliminary sandbox helper delivery FAIL255
before runtime. New source/script/output names preserve both results. These
owned scripts contain no SDK path or vendor code, and only compile the owned
interface/proxies through Windows PowerShell Add-Type. No scheduled task was
created. A full cleanup/containment claim is not asserted by these captures.

The current contract corrects only the representation identity requirement;
all provider denials, malformed-message rules, callback exclusions and bounded
sticky audit state remain required. Source23 implementation was held before
any source edit. Independent saved-result audit PASS, all11 failure bindings
and8 inspection bindings, private constructor-windows/
verifier23/clr-result-audit.txt SHA-256
`76bc94d55f9599f4e7b0f5897f8a258c6931da4739e7128d24dd6a73b79ed828`.
Revised contract critical review PASS, no unresolved findings:
review23/revised-type-contract-review.txt (5,953 bytes), SHA-256
`9c633c222016e19850403ba56d1b116d881a766950398ae25bb4594554de0be6`.
Reviewed contract checkpoint SHA-256
`46cb6b8f23fef4740f4a5f04a80d7f2250d83c158d35def101d8555c7a2c0eff`;
this result/review-record append is later. No audit reran Windows or read SDK
payloads. No original retry or task/gate acceptance follows.


## Pinned Autofac assignability finding

After the genuine CLR representation was established, one additional pinned
public source query exposed the next failure before an original SDK attempt.
Autofac commit07cac9047f4059de2d72834042c9649cbd228085:

- ProvidedInstanceActivator passes instance.GetType() to its base constructor.
- InstanceActivator stores that constructor argument as LimitType.
- RegistrationBuilder.CreateRegistration checks each typed service with
  ServiceType.IsAssignableFrom(LimitType), except when LimitType is Object.
- The observed MarshalByRefObject representation does not implement the
  declared provider interface. Therefore a type-only RegisterInstance shim
  cannot complete this registration's container Build on this source contract.

Private task009-autofac-type-assignability preserves original MIT notices and
immutable public source bytes: RegistrationBuilder.cs11,500 bytes SHA-256
`757f24738a3cb57db1f811170822ff5041b2d1077885b6775612207d9121427e`,
InstanceActivator.cs1,726 bytes SHA-256
`b9fa63deca86201825d3bf7cc8153d2580b2a8795f1c86993dde52d28534f1eb`,
and DelegateActivator.cs2,553 bytes SHA-256
`2436fc49abc07b05b716bc5993ec39c4901cc6dfa74f01609a503a17508c4ed6`.
Source URLs, pinned commit,20-second/262,144-byte bounds and refused redirects
are recorded in source-record.json and activator-source-record.json. This is
source compatibility evidence, not an executed actual-binary Build failure.

The incomplete three-file Source23 patch was archived exactly and restored by
its owner before compilation. Private task009-undo-provider-type/source23/held-type-only-patch (under
private/evidence/) preserves
baseline/current bytes and diff (SHA-256
`d66b35adee934f7b7f307363e072fd9dc8a7605feae4724ee7e5f97a245ba273`).
The restore proof SHA-256 is
`c99e270c3d8080050d8b1affa4dd67781d9a44cfc08d37241c80a55f010de09d`;
all10 constructor sources match HEAD1ad7428 and freeze22 again. No source23
build, integrated owned run, freeze, source commit or original attempt occurred.

The next candidate is the existing public typed Register<T>(Func<context,T>)
API: its delegate activator uses the truthful declared T as LimitType and can
return the same already-created deny-only interface proxy. No GetType value
needs to be fabricated. This API's actual binary shape, relaxed delegate
binding and owned registration behavior require a separate minimal Windows
proof before a new integrated implementation contract. Readiness evidence:
private task009-undo-provider-delegate-readiness/readiness.md SHA-256
`2a1f15c2ed630d7b751eb9f1e6d21db03716b778f076d322c92e491f87ebe34e`.
All provider business methods remain unsupported; no SDK qualification follows.


Independent finding/documentation review PASS, no blocking findings:
private constructor-windows/review24/assignability-finding-review.txt
(7,189 bytes), SHA-256
`a95c4914c16d88ad7b851c0928f13231449dc264e8d29ad2e5e4a7f9813dcf57`.
It checked the three public source pins, archived patch/restore and current/
committed/frozen source10 without SDK payload reads or runtime. The requested
archive-path clarification is included above. No gate or qualification follows.
