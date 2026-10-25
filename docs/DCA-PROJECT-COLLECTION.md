# Task-009 offline ProjectCollection declaration inquiry

## Readiness and chosen question

Task-008 is accepted; Task-009 remains In progress/all ACs unchecked, no gate.
Original27 FAIL2 and the reference-only original28 PASS are recorded in the
linked [reference inquiry](DCA-ONLINE-CATALOG-REFERENCES.md). v1 imports locally
supplied manufacturer files; catalog acquisition/admission/runtime remains
deferred pending offline necessity evidence. This inquiry implements no catalog.

The observed context getter returns concrete SDK Project. Saved metadata shows
that SDK class is public/sealed and ultimately derives from System.Object,
so an interface or MarshalByRefObject proxy cannot replace it. Its examined
internal constructor requires a concrete ObjectModel Project, SDK collection
and nonpublic context; no public bootstrap route is established.

The saved construction report also lists five PUBLIC ObjectModel Project
constructors06002830..06002834. Variants06002831 and06002833 take an explicit
IObjectContext. Their signatures do not prove different offline behavior.
They also need concrete ObjectModel ProjectCollection, whose declaration is
not present in the inspected reports. The saved genuine ObjectContext constructor
already takes Project, an apparent construction cycle, not proof it is unavoidable.

Private context-injection-comparison.md SHA-256
337e10e57341cd38876b1b3bd933c6fa6fa801400fe9f72cf11c01083b806d6e
under task009-minimal-offline-project-context-readiness recommends examining
this one missing root before an SDK-wide public direct-return scan. Corrected
readiness977563c050db188461dbde9316f364120cf80001eee5a5299653841cfc71dae3
preserves the concrete SDK boundary and limits. Do not repeat known Project
constructors, infer a nullable/default argument or activate a new route here.

## Fixed original input and output

Add only selector project-collection, worker Read-ProjectCollectionMetadata.ps1,
operation static_dca_project_collection. Root is EXACTLY
Knx.Ets.ObjectModel.Project.ProjectCollection, distinct from SDK
Knx.Ets.Sdk.Project.ProjectCollection. No arbitrary root or assembly selector.

Input Knx.Ets.ObjectModel.dll14,204,880 bytes, SHA-256
73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a,
definition6.4.8718.0/neutral/token61439873ec5e1159. Check size/hash before PE
identity/root selection. Use a separate unsigned owned input/root, never an
original-mode route. Missing/duplicate/nonpublic/generic/unsupported roots
must fail explicitly; do not assume class or interface from its name.

Use PEReader/System.Reflection.Metadata. Report root token/name/raw flags,
visibility, class-semantics/interface flag, generic arity and unresolved base/
direct interface identities. Report ALL declared constructor signatures and
raw access/static flags, with a public-constructor count. These are declarations
of constructors, not bodies, activation or supported argument semantics.
Report other PUBLIC declared methods only: tokens/names/flags/signature header,
calling convention/instance/generic arity, ordered parameters and returns.
Navigation/getter/static return shapes are candidate facts, never creation
proof. Other methods, inherited members, external declarations and factories
outside this root remain unexamined.

No IL/bodies, fields/values/layout, custom attribute execution, Assembly.Load,
reflection invocation, SDK/candidate loading, dependency resolution, arbitrary
graph or implementation search. No fake SDK class, uninitialized object,
constructor/signature/license bypass, guessed null success, stream values or
new online behavior. A constructor-only declaration cannot qualify its use.

## Bounds, ownership and owned proof

Preserve file16MiB/aggregate32MiB, ObjectModel profile MethodDef65536/other
tables32768, strings1024, signature blob128 bytes/rendered4096 characters/
nodes128/depth16/parameters32/generic counts32, direct interfaces32. New root
has at most32 constructors and256 other public methods; overflow refuses,
never truncates/widens. Report UTF-8 bytes at most512KiB. Base/interfaces stay
UNRESOLVED_NOT_FOLLOWED; no ancestry traversal in this query.

Own only experiments/dca-integration/EntrypointMetadata.cs,
OwnedEntrypointFixtures.cs, Test-EntrypointMetadata.ps1 and NEW
Read-ProjectCollectionMetadata.ps1; experiments/dca-metadata/run_probe.py and
test_run_probe.py. No constructor10/build/config/dependency/protocol/domain
changes. Keep existing selector/report bytes and old bounds unchanged; retain
legacy rights marker without reopening ADR-005 private copy/no redistribution.

Owned cases cover actual root kind/visibility/base/interface references,
public/internal/private/static constructor distinction and count, static/
instance public signatures, private nonconstructor and inherited-member
omission, wrong input/hash/identity/root/duplicates/generics, malformed/
unsupported signatures, exact table/work/output bounds and original/owned
route/report spoof rejection. Use separate fixtures; preserve old families.
Run complete owned PowerShell metadata and Python guard suites, compile/parsers,
preservation/scope/whitespace. Freeze six sources/snapshots/commands/results/
retained failures before independent verification, critical review and commit.

Only afterward may one separately frozen extraction use the existing exact
PowerShell7.6.7 and unchanged run_probe deny-default child. Fresh owned write/
network denial probes before EVERY decode, outside-Git private0700 stage/0600
pinned input, sanitized environment,30-second/2MiB aggregate capture and
exclusive output/attempt marker. Use the known outer execution context allowing
the unchanged inner sandbox, no relaxed/unguarded fallback. Failure stops that
inquiry; no retry or constructor invocation is implicit.

Pre-implementation checkpoint: source29 implementation/tests/extraction NOT RUN. No public/internal
SDK activation, catalog admission, device operation, task acceptance or gate
advance follows. The original initialization and state/UI round-trip remain
unqualified; Task-009 stays In progress.


## Source29 implementation and owned evidence

The six assigned metadata sources are implemented and frozen at baseline
bdb3a24eeeec4acfdf3f083aaaab8ee8528180d6, before source commit. The original
pre-implementation contract checkpoint above is historical. Native private
source-freeze29.json SHA-256
95883cb982a35828b187b65f3527e21f08d6ba86644f989b65f8b132b64f3d34
binds six source snapshots, exact original input/root and evidence references.
source29/evidence-manifest.json SHA-256
bd5cf8c8771e8b758d54dbe7e0b49ee24ba3057bd3088e93b5817d6115ac7ae0
binds20 artifacts. Exact commands/stdout/stderr are retained under source29/;
no initial test failures occurred.

Owned PowerShell534 cases and Python27 tests PASS. Reader/fixture compilation,
two PowerShell parsers, two Python AST checks, exact preservation of old reader/
fixture/test/guard blocks and report bytes, unchanged constructor10 sources,
project-scope and git diff whitespace checks PASS. Independent verifier29/audit.txt
SHA-256 c15d373bca402544c267ec1df7384396682af61dd47c1d18ca71993511e20f9f
reproduced those results on Darwin27 arm64, Python3.11.2, PowerShell7.6.7.
All20 manifest artifacts, eight freeze references and six source pins matched.
These are owned fixture/local static checks, not original SDK runtime results.

public_constructor_count counts public nonstatic .ctor declarations only;
all declared .ctor/.cctor access/static flags are still reported. Actual root
kind is recorded; class semantics alone does not qualify value-type/enum use.
The separate final extraction helper is prepared but NOT RUN. Original metadata
extraction, SDK activation, Windows/vendor runtime and hardware remain NOT RUN.
Critical frozen source/helper review and source commit precede any extraction.
Task-009 remains In progress, all ACs unchecked, no gate advances.
