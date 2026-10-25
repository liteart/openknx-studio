# Task-009 fixed SDK static-return declaration inquiry

## Readiness and new question

Task-008 is accepted, Task-009 remains In progress/all ACs unchecked, no gate.
Source29 baded77c87f7468b73611ebd5773c1da540bf02e and result c722484 record
the concrete ObjectModel ProjectCollection dependency fanout. The known SDK
Project is public/sealed with an internal constructor requiring a concrete
ObjectModel Project. Known navigation candidates require receivers and do not
establish bootstrap. Context-injected ObjectModel constructors have unknown
argument/behavior requirements. No catalog dependency is acquired or admitted.

Private next-context-readiness29.md SHA-256
9527214d4575aed332e7a39ce910097d87df9f66570632dc27d5f48487a8ce54
recommends a NEW selection: public declared static SDK methods on ALL externally
public SDK type definitions returning one of three exact concrete SDK types.
The earlier fixed-root construction report does not cover other public owners.
Do not repeat target constructor queries or instance navigation; constructors
return Void and cannot satisfy this new direct-return selection. A negative
result does not rule out legitimate instance/nonpublic/wrapped/external routes.

## Exact input, targets and scope

Add fixed selector sdk-static-factories, operation static_dca_sdk_static_factories,
worker Read-SdkStaticFactoriesMetadata.ps1. Only Knx.Ets.Sdk.dll6,979,024 bytes,
SHA-2567d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989,
identity6.4.8718.0/neutral/token61439873ec5e1159. Existing private input is under
task148-clean-host/private-inputs; no new SDK provisioning is needed.

Check size/hash before PE identity; require unique targets with these exact
names and tokens: Knx.Ets.Sdk.Project.Project020001D1,
Knx.Ets.Sdk.Root0200002F, Knx.Ets.Sdk.Project.ProjectCollection020001D4.
Report target raw flags, visibility/generic arity and these actual tokens.
Own unsigned fixtures use distinct input identity/namespace/targets/tokens;
original worker must never expose an owned selector or arbitrary roots.

Scan TypeDefs and their declared methods only. Externally public owners are
public top-level types or NestedPublic types with an entirely public enclosing
chain; bound owner-chain traversal to16 and reject cycles/depth overflow.
Generic owner/method arity must be visible in selected declarations, not assumed
constructible. Select MethodAttributes.Public AND Static, excluding constructors,
with a DIRECT class return encoded as the exact target TypeDef handle in this
assembly. No same-name external TypeRef inference, TypeSpec/array/byref/pointer/
generic/wrapped result inference or receiver/argument graph traversal.

Use PEReader/System.Reflection.Metadata only. Bound a nonexecuting return-prefix
prefilter to16 signature bytes per examined public static method; parse header,
optional generic count, parameter count and return kind/token only. Do not
allocate/decode whole excluded signatures. Class return handles to other types
are excluded. Other return shapes and unsupported prefixes must have explicit
counts/limitations, not be treated as proof no alternate SDK route exists.
Malformed/truncated prefix/header/token fails explicitly, rather than being
counted as a valid unsupported return shape. Validate that a method header is
a method signature with defined calling-convention/flag combinations; field,
property, local-variable, reserved or invalid headers are malformed here.
This MethodDef-only profile accepts Default(0), Generic+Default with a positive
generic count, and nongeneric VarArgs(5). Refuse other calling-convention nibbles,
reserved bit0x80 and nonmethod headers. Generic+VarArgs is refused as
UNSUPPORTED_METHODDEF_HEADER; no claim of universal CLI validation is made.
ExplicitThis requires HasThis structurally. Generic count zero is malformed.
For excluded return shapes inspect only a recognized leading kind and immediate
coded type-token where present (class/value/modifier/generic-instance head);
deeper wrapper tails/parameters remain explicitly unexamined.
Selected signatures must pass the existing strict full decoder and its bounds.
Prefix recognition of nongeneric VarArgs does not widen that decoder; a selected
VarArgs candidate may therefore be refused by the unchanged strict decoder.
Additionally require selected Static attributes to agree with the signature:
HasThis or ExplicitThis on a selected static candidate is refused explicitly,
never reported as a no-receiver factory. These consistency checks are new-mode
only; do not silently change older readers. Report scan totals,
visibility/static exclusions, direct-target match count and mutually consistent
nonselected return categories. State that nonselected parameters/bodies were
not validated. Do not claim signature completeness beyond this exact selection.

For each selected method record owner token/name/raw flags/public enclosing
chain/generic arity, MethodDef token/name/raw flags, signature header/calling
convention/instance flag/method arity, ordered parameters and return. Static
shape means no receiver, not usable arguments, available services or a working
bootstrap. Report inherited/external/nonpublic/instance and wrapped paths as
unexamined, never impossible. No IL/bodies, fields/values, custom attributes,
Assembly.Load, activation, dependency resolution, SDK execution or online use.

## Bounds, ownership and required evidence

Retain file16MiB/aggregate32MiB, table rows32768, strings1024. Selected full
signatures retain blob128 bytes/rendered4096 characters/nodes128/depth16/
parameters32/generic counts32. At most64 selected methods; overflow refuses,
never truncates. UTF-8 report at most512KiB. The bounded16-byte prefilter is
new-mode-only and does not widen any old signature/report/table limit.

Only six source paths: experiments/dca-integration/EntrypointMetadata.cs,
OwnedEntrypointFixtures.cs, Test-EntrypointMetadata.ps1, NEW
Read-SdkStaticFactoriesMetadata.ps1; experiments/dca-metadata/run_probe.py,
test_run_probe.py. No constructor10/build/config/dependency/protocol/core changes.
Preserve all old blocks and outputs, including source29. Owned tests must cover
public/nested/private enclosing owners, static/instance/private methods,
constructor exclusion, exact versus same-name external/wrapped returns,
all three targets and owner/method generic arity, absence/duplicates/token/root/
identity/input mismatch, malformed prefixes/headers/selected signatures,
forged static-attribute/instance-header and ExplicitThis refusal, row/owner-chain/
selection/signature/output bounds and original/owned route/report spoof refusal.
Do not add meaningless mirrored cases just to raise the test count.

Run complete owned PowerShell metadata/Python guards, compile/parsers,
prior-mode and constructor10 preservation, scope/diff checks; freeze six source
snapshots and actual commands/results/any failures. Independent verification,
critical source/helper review and local source commit precede any original read.

Only then one separately frozen guarded metadata extraction may read the exact
existing SDK input. Retain PS7.6.7, unchanged deny-default inner sandbox, fresh
write/network denial probes each time, outside-Git private0700/0600 stage/input,
sanitized environment,30-second/2MiB capture and exclusive attempt/output markers.
Use known outer context enabling that unchanged inner profile; no retry or
relaxed fallback. Vendor code/constructors/import callbacks must not execute.
Rights marker remains historical; ADR005 private copy/no redistribution stands.

Pre-implementation checkpoint: source30 implementation/tests/extraction NOT RUN.
Factory availability, legitimate arguments, SDK initialization and catalog
independence remain UNKNOWN. No Secure activation, device operation, catalog
feature/admission, Task-009 acceptance or gate advance is authorized by this query.


## Primary signature-format references

[ECMA-335, 6th edition, June2012](https://ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf#page=285),
PartitionII23.2.1 (printed259), defines MethodDefSig; II23.2.3 separates standalone
call-site signatures. II23.2.8 and II23.2.11-12 define coded type handles and
return/type shapes. Consulted2026-10-25 for the bounded MethodDef profile above.
The [Microsoft SignatureCallingConvention enum](https://learn.microsoft.com/dotnet/api/system.reflection.metadata.signaturecallingconvention)
also includes unmanaged calling conventions used in broader signature contexts;
its enum membership alone does not prove MethodDef validity. This query validates
the fixed profile and selected declarations, not every CLI metadata constraint.


## Source30 owned implementation and evidence

The six assigned sources are implemented/frozen before source commit at baseline
c722484. The pre-implementation statement above remains a historical checkpoint.
Native source-freeze30.json8,617 bytes SHA-256
96497670fa67e0c8ba6b3e9b3187658184698824687d94415e84d05d9cf0a952
binds six source snapshots and nine references, including the final contract and
both contract reviews. source30/evidence-manifest.json3,643 bytes SHA-256
d30f48155dfa63c3159d46c670001372c6a0b80dd9e898217fc795b22d777736
binds23 artifacts. Exact commands and stdout/stderr are under source30/commands.json
and associated files. Initial runs passed; no failed test/compiler run occurred.

Owned PowerShell596 cases and Python28 tests PASS. Reader/fixture compilation,
two PowerShell parsers/two Python AST checks, exact source29-and-earlier reader/
fixture/test/guard/report preservation, constructor10 preservation, project scope
and diff whitespace PASS. Independent verifier30/audit.txt SHA-256
7a9020da26e0ccad98a1e270e13bda2e4f8e2860b06282c296079a00fdffea8a
reproduced those results on Darwin27 arm64/Python3.11.2/PowerShell7.6.7 and
matched all six source pins,23 manifest entries and nine named freeze references.
These are owned fixture/local static checks, not an original SDK result.

The selected_targets schema is an ordered array of three fixed names, with
actual target tokens/flags/visibility/arity reported separately. Excluded blobs
are not allocated wholesale: the prefix reader counts each byte, maximum16;
selected signatures retain the unchanged strict decoder/bounds. Tests prove
64 selected candidates,16 owner-chain nodes and32768 table rows at the accepted
boundaries, selected VarArgs refusal versus excluded VarArgs prefix recognition,
selected static/receiver contradictions and excluded wrapped/foreign returns.
A large excluded malformed tail is explicitly unvalidated and never executed.

Prepared run_original30.py is NOT RUN. Independent critical frozen source/helper
review and local source commit must precede one fixed original extraction.
Factory availability, legitimate arguments, bootstrap and catalog independence
remain UNKNOWN. No SDK activation, Windows runtime, UI/state or hardware proof;
Task-009 remains In progress/all ACs unchecked, no gate advances.
