# Task-009 ObjectModel catalog-reference inquiry

## Readiness and scope

Task-008 is accepted; Task-009 remains In progress/all ACs unchecked, no gate.
Actual original27 at planning19019940/implementation25eea43 ended FAIL2 after
the unallowlisted OnlineCatalog.Download assembly request. Result/audit/critical
review are recorded in [DCA-LARGE-STRING-CACHE-DENIAL.md](DCA-LARGE-STRING-CACHE-DENIAL.md),
result documentation0ea35ea. The Project did not return. No dependency was
admitted and no stream method was called.

The matching candidate DLL is not in the provided private files; its reachable
path has been requested. That blocks candidate verification/admission and a new
original attempt. This independent inquiry needs ONLY the existing ObjectModel.
Readiness reference-map-readiness.md SHA-256
aa3e479606ca523c1c4958124c3ccbf20c5827241652784279bb6026cbcddefc
under private task009-online-catalog-dependency-readiness documents feasibility.

Source28 adds exactly one fixed metadata selector project-online-catalog-references,
worker Read-ProjectOnlineCatalogReferences.ps1, operation
static_dca_project_online_catalog_references. It maps declarations of references
in the GLOBAL ObjectModel module; it is not a Project06002830 call-site map.
It supplies no DLL, replacement SDK assembly, service, dependency admission or
original execution. Keep candidate acquisition/verification separate.

## Exact input and token selection

Input Knx.Ets.ObjectModel.dll14,204,880 bytes, SHA-256
73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a,
definition Knx.Ets.ObjectModel6.4.8718.0/neutral/token61439873ec5e1159.
Select exactly one AssemblyRef with structured canonical identity:

```text
Knx.Ets.OnlineCatalog.Download, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159
```

Validate exact input size/hash before PE metadata/identity/selection. Missing or
duplicate exact target references fail explicitly. Never invent its token from
the retained JSON array index. Use PEReader/System.Reflection.Metadata only,
without Assembly.Load, dependency resolution or target execution.

Resolve TypeRef ownership by handles: direct AssemblyRef scope or nested
TypeRef scope chain ending at the selected AssemblyRef. Bound every chain to16
TypeRef nodes with visited-token cycle detection; nested namespaces must be empty.
The terminal AssemblyRef adds one token, so the recorded scope_chain contains
at most17 entries. Signature nesting has its separate depth16 bound.
ModuleRef/ModuleDef/forwarded/external declarations remain unresolved. Invalid,
nil, unsupported or cyclic scope must be a controlled refusal or explicit
uncertainty, never a false ownership match. Names/signature text are display
data, not a substring/prefix ownership test. Same names under other identities
must not match.

Report selected AssemblyRef token/identity/raw flags and selected TypeRefs in
table order: token, namespace/name, immediate scope kind/token, bounded scope
chain and qualified display. Include only MemberRefs whose IMMEDIATE Parent is
a selected TypeRef handle. Report token, parent kind/token, field/method kind,
name and bounded decoded signature. For methods include signature header,
calling convention/instance/generic arity and ordered return/parameter types.
Fields are signature types only, never FieldDef values/layout. A .ctor member
reference is not inspection/invocation of a constructor definition or evidence
it ran.

Do not classify TypeSpec-parent MemberRefs by generic arguments or name text.
Report ALL TypeSpec-parent MemberRefs as a GLOBAL UNEXAMINED_TYPESPEC_PARENT
count, explicitly not a target-specific count. Other parent kinds get bounded
counts/dispositions only. A bounded TypeSpec INSIDE a selected signature may
use existing pure decoding; unsupported shapes refuse explicitly. Do not infer
ownership from parameter or return types. MethodSpec use, forwarded types,
external definitions, inherited closure and call sites are unexamined. Empty
selected results do not prove absence of catalog use or completeness.

Report selected_assembly_reference as the exact canonical identity string,
assembly_reference as its token/identity/flags object, type_references,
member_references, selection/global omission counts and explicit limitations.
All referenced declarations remain UNRESOLVED_NOT_FOLLOWED. No IL/method bodies,
constructor definitions, attributes, resources, values, implementation search,
automatic closure or network behavior inference from names.

## Bounds and source ownership

Preserve file16MiB/aggregate32MiB, other table rows32768 and the exact new
ObjectModel profile's evidenced MethodDef65536 allowance. Bound AssemblyRefs256,
strings1024, raw signature blobs128 bytes, rendered signatures4096 characters,
signature nodes128/depth16/parameters32/generic counts32. Selected TypeRefs256
and MemberRefs256 maximum; overflow refuses, never truncates or widens. The new
report is at most512KiB UTF-8 bytes. All existing modes/limits/report bytes stay
unchanged.

Own only experiments/dca-integration/EntrypointMetadata.cs,
OwnedEntrypointFixtures.cs, Test-EntrypointMetadata.ps1, NEW
Read-ProjectOnlineCatalogReferences.ps1; experiments/dca-metadata/run_probe.py
and test_run_probe.py. No constructor10, build/config, dependency, protocol or
product-domain changes. Use distinct unsigned owned input and target identities;
the fixed original worker must reject them. Extend fixed guard routing and
report identity/selected-assembly checks only. Retain the legacy report rights
marker without reopening ADR-005's confirmed private copy/no redistribution.

## Tests and original extraction prerequisites

Owned cases cover direct method/field and nested target references; exact
identity versus version/culture/token/name-prefix collisions; missing/duplicate
AssemblyRefs; malformed/nil/module/cycle/depth-boundary scopes; TypeSpec-parent
global omissions without false target attribution; valid/unsupported/malformed
signatures; exact work/output limits; report spoof/original-versus-owned routing
and no target candidate dependency. Preserve old fixture families. Run complete
owned PowerShell metadata and Python guard suites, parser/preservation/scope/
whitespace checks; freeze sources, commands, snapshots and retained failures.
Independent verification, critical review and source commit precede any
separately frozen original extraction.

Extraction uses the existing exact PowerShell7.6.7 and unchanged deny-default
run_probe child. Fresh owned write/network denial probes precede EVERY decode.
Use outside-Git private0700 stage/0600 pinned input, sanitized child environment,
30-second deadline/2MiB aggregate capture and exclusive report output. Use the
already verified outer execution context permitting the unchanged inner guard;
do not repeat the known nested-sandbox failure or relax the profile. Failed
probes stop that inquiry; prior probes cannot substitute. Parent hash/copy is
provenance only; PE decoding occurs in the restricted child.

Status: source28 implementation/tests/extraction NOT RUN. This inquiry cannot
admit the absent assembly, qualify SDK behavior, activate a bus operation or
advance a gate. Task-009 stays In progress; candidate admission remains blocked.

## Source28 owned checkpoint

The six assigned metadata paths are implemented and frozen by private
task009-online-catalog-reference-map/source-freeze28.json,7,625 bytes,
SHA-2565711dc3e394a7a71bb95396b5adecc085013a8576bfe2ca1fb0fb2c57c43b0e6.
source28/evidence-manifest.json, SHA-256
fbd2a4425e57778561c355cee4e24f3763192c99abe7227c937e33916abbba75,
binds29 evidence files, six source snapshots, commands and retained initial
failures. The freeze includes eight reference bindings and the exact contract
checkpoint/review. It records uncommitted source28 at docs-only base5be5e5b.

Owned PowerShell478/FAIL0 and Python26 tests PASS. Compile, two PowerShell/two
Python parser checks, exact old reader/fixture/test/guard preservation, scope
and whitespace PASS; constructor10 remain unchanged from freeze27. Initial
owned malformed-scope/RID handling and Python worker-basename failures are
retained, followed by passing final suites. No SDK metadata was decoded to
repair those owned tests.

Independent verifier28/audit.txt, SHA-256
22900697b967313a620af27e54e9cfe7b858b62a1866eb3162dbaf8e8723ee2c,
PASS: six current/snapshot pins,29 evidence and eight references, reproduced478
PowerShell/26 Python tests, compile/parsers/preservation/scope/diff. An initial
verifier compile shell-quoting error is retained; only its shell-safe corrected
compile is counted. No SDK extraction, vendor runtime or hardware was executed.

The fixed original28 helper is prepared/AST-only, SHA-256
319c639764875b33ddda6367bdef3a19241304cc83aa87e44090b50e413c53f6.
It checks committed/current source pins, the source review, exclusive output/
attempt markers and delegates unchanged fresh restriction probes before PE
decoding. Critical source/helper review and source commit remain pending.
Original28 extraction NOT RUN. Catalog acquisition/admission/runtime is DEFERRED
pending offline DCA necessity evidence; this global reference query implements
no catalog feature and does not establish a constructor call path.

### Source28 critical review

review28/critical-source.txt,4,843 bytes, SHA-256
891b8dd879f4c75e60aac04f951b139db72d9add7446d44aee50994baf864d8c,
PASS with no unresolved findings. It checks the actual eight-path increment,
six current/snapshot pins,29 manifest files, named references, constructor10,
independent audit and prepared wrapper. Exact-boundary fixtures accept16
TypeRef nodes/reject17; the terminal AssemblyRef gives at most17 recorded tokens.
The wording clarification preserves frozen code/test behavior and the historical
contract checkpoint. Wrapper AST and diff checks PASS. No SDK metadata or
runtime was executed by review. Source commit/current-pin verification still
precedes the one fixed guarded extraction; no catalog feature/admission follows.
