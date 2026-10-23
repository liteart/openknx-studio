# Task-009 bounded Project constructor resource fallback

## Readiness and purpose

Baseline `5401e66` records actual19b FAIL2 with independent result audit and
critical documentation review PASS. Project constructor `06002830` did not
return after one synthetic get_Id request: the existing handler fatally denied
exact Autofac.resources8.2/en-GB outside its Factory-only constructor window.
The underlying Project/service failure is UNKNOWN. Earlier source/owned tests
and unchanged inputs are prerequisites, not usable Project/DCA qualification.
Tasks 003/004/005/006/008 are accepted; no phase gate applies or advances.

This increment prepares a separate diagnostic with explicit modes
`original-project-genuine-factory-resources` and
`owned-project-genuine-factory-resources`. Existing modes and their results
remain unchanged. It does not retry original19b or assume a resource DLL is
needed. Source/owned verification, critical review and source commit precede
any separately frozen original20 plan.

## Format evidence and interpretation limits

Microsoft's [.NET Framework AssemblyResolve reference](https://learn.microsoft.com/en-us/dotnet/api/system.appdomain.assemblyresolve?view=netframework-4.8.1)
and [handler guidance](https://learn.microsoft.com/en-us/dotnet/standard/assembly/resolve-loads)
describe satellite resolution events and returning null for requests the handler
does not resolve. The explicit [.NET Framework resource fallback section](https://learn.microsoft.com/en-us/dotnet/core/extensions/package-and-deploy-resources#net-framework-resource-fallback-process)
describes continued parent-culture fallback after a null result. Only that
Framework section applies to this net48 host, not the separate .NET Core rules.
Primary references were checked on 2026-10-23.

Returning null supplies no assembly, stub, service or successful constructor
result. CLR probing/loading may continue, including before the existing
after-load audit. NULL RequestingAssembly does not prove an Autofac caller.
The diagnostic retains these limitations and the existing fatal dependency
latch; it is not a sandbox, probing-prevention or vendor-behavior guarantee.

## Smallest source change

The four assigned implementation files are ProjectConstructorProbe.cs,
ProbeLauncher.cs, OwnedProjectConstructorFixtures.cs and
Test-ConstructorProbe.ps1 under `experiments/dca-constructor/`. No build-script,
configuration, input-array, dependency, SDK contract or artifact-layout change.
All 19 private/two fixed OS input pins, 18 artifacts, 15-second child deadline
and 64-KiB output cap remain fixed. All Windows actions stay with the coordinator.

For only the new mode family, start one thread-local aggregate budget immediately
before WithRegisteredTrace. Enable resolution eligibility only around each exact
Factory and Project ConstructorInfo.Invoke expression. End eligibility before
return validation, logging, error handling or disposal. The outer budget remains
through the existing managed lifetimes/final audits and is cleared in finally.
The two constructor windows share at most eight eligible probes; the ninth
refuses and saturates. Do not reset the counter between Factory and Project.
Preserve WithFactoryResourcePhase and legacy mode/reset behavior unchanged.

Require the exact original mode, complete held profile, pinned actual Autofac
owner/path/identity, same-thread active constructor phase, and only the existing
full en-GB/en Autofac.resources8.2 identities. A non-null requester must be the
same owner; a null requester remains explicitly NULL_UNKNOWN. Return only null
from the existing resolver branch; no recursive Load, replacement assembly,
satellite copy or automatic dependency fetching is added.

All unapproved identities/cultures/versions, owners, profiles, modes, threads or
phases remain fatal. Constructor windows are nonnested. Budget/window flags do
not reset sticky dependency, provider or store-denial latches. Final audits
still run after Project, container and trace managed cleanup, including caught
denials. No Project Dispose is invented for an unreturned constructor object.

The synthetic get_Id proxy runs synchronously inside Project Invoke. Interpret
the getter exclusion literally: suspend the new-mode window throughout the
owned ProjectStoreIdProxy.Invoke callback and restore it in finally, preserving
the aggregate count and all denial latches. Fixture ID construction/numeric
getter, builder/registration/Build, Project/container/trace disposal, errors and
DCA/UI/state work receive no resolution window. Old modes remain unchanged.

## Required owned tests and completion

Prove shared capacity across both windows, exact eighth/ninth boundaries and
saturation; cleanup on return/exception; nesting/off-thread refusal; strict
identity/profile/owner/requester predicates; getter suspension/restoration;
and denial outside each constructor, during validation/logging and every
cleanup scope. Caught refusals must fail final audit. Use fresh child processes
for sticky-latch cases, never production resets. Owned fixtures cannot
impersonate pinned Autofac or count as actual CLR/vendor fallback evidence.

Required checks: existing C#5/x64/warnings-as-errors three-target compile against
six pinned net48 references; PowerShell parser/source guards and preservation;
then fresh pinned Windows build and complete owned Session0, limited-interactive
and administrator-negative suites, independent verification and critical review.
Retain all earlier failure/source snapshots. No original20 outcome is asserted
until its separate reviewed committed-source plan is genuinely executed once.

Status: contract prepared, implementation/tests NOT RUN for this increment.
Task-009 remains In progress, all ACs unchecked, qualification NOT_ESTABLISHED.
No DCA initialization/UI/state, intentional device operation, gate, isolation,
network or descendant containment is qualified. The existing owner diagnostic
exception and private-only proprietary DLL boundary remain in force.
