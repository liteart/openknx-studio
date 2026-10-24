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
binding configuration. Warnings-as-errors remains enabled. Whether that
compiler option resolves these warnings is NOT RUN, not assumed.

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
