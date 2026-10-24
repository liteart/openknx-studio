# Task-009 exact CLR type introspection

Readiness: Task008 accepted, no phase gate. Actual22 at source72bca2e FAIL2
denied System.Object.GetType during undo registration before Build or any
Factory/Project constructor. Exact internal caller unknown. Preserve source22
and all earlier modes/evidence. The DCA readiness investigation supports this metadata-only adaptation.
Implementation and Windows validation are not yet run for this increment.

Use separate fixed original/owned-project-undo-provider-type modes. Only these
modes may return the actual RealProxy.GetProxiedType() for exact System.Object
GetType instance/public/nongeneric/no-parameter/System.Type-return identity.
Check MethodBase identity as well as MethodName, ArgCount, InArgCount and
signature metadata, nonnull empty Args and exact trusted runtime MethodInfo
concrete kind/module/token/Equals. Reject transparent or custom MethodInfo
without trusting its metadata getters. A same-named provider/member or malformed
message is not eligible. Do not inspect argument values. No recursive transparentProxy.GetType.
ReturnMessage must carry the actual Type, no out arguments, and original call.

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
Prove old deny-only proxy refuses that owned call; new mode returns exact owned
interface Type and completes registration. Also test direct transparent proxy
call, all provider denials, other Object methods, malformed/forged message
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


Independent contract review PASS, no blocking findings, before implementation:
private `task009-undo-provider-type-contract-review.txt` (6,892 bytes), SHA-256
`a9252b2766fcb4a7a6fd36f66b8ff1b221e2611dfe0cddb524153538f24f90ba`.
Reviewed contract checkpoint SHA-256
`88a2f3cae1c7fd04ffbdef5497c2bb71e11636252a9ae6d86eea73be78cd0f5e`;
this review-record append is later. The final guards must use denial/fault
state rather than total callbacks; bounded successful metadata replies alone
must not trigger the old deny-only failure. This review supplies no runtime
qualification or operation approval.
