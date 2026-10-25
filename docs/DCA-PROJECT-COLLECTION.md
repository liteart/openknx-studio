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


## Original29 declaration result

Frozen source/helper critical review PASS, no unresolved findings:
review29/critical-source.txt SHA-256
cb6763e495bdef4b3b10dbe84d87dfb37882a39fd5ef7b4a8df234c7c59d3202.
Source commit baded77c87f7468b73611ebd5773c1da540bf02e was clean and all six
current/Git source pins matched before the one fixed extraction. Private
source-commit29.json SHA-256
e48edaffa98a4cf051a42291d99c41f7edc53adf558047e3127642994651f42b
records that checkpoint. From the repository root, the coordinator ran once:

```text
python3 -B private/evidence/task009-project-collection-metadata/run_original29.py --source-commit baded77c87f7468b73611ebd5773c1da540bf02e --review-sha256 cb6763e495bdef4b3b10dbe84d87dfb37882a39fd5ef7b4a8df234c7c59d3202
```

Result PASS for declarations only, terminal exit0. Fresh PowerShell7.6.7 probes
confirmed write_denied=true and network_denied=true before decoding under the
unchanged inner deny-default profile. Both children exited0, stderr empty;
metadata stdout14,638 bytes. Exact original input remained unchanged. No retry.
The outer execution context permitted that unchanged inner sandbox; it did not
relax its restrictions. The final helper differs from the preserved pending
checkpoint only by substituting the frozen hash.

original29-report.json13,950 bytes SHA-256
d7488b3043f9b4d1b89297dae2371f4b82c8c6f667dc030bffa60649a5803479;
original29-observation.json SHA-256
2915e2e15f2799b54d5299f2e3931a2788cb1c13919869b72911779b19dfc5eb;
original29-evidence-manifest.json SHA-256
5f65bfba238e21793f784ecfd0a0ecd7fa157cf8e5f6eb911ac6f81f6a20987b
binds48 artifacts and the exact command.

The selected ObjectModel ProjectCollection root is020003BD, flags1048577,
public nongeneric Class, with unresolved base ObjectModel.DomCollection and10
direct interface declarations. It has two declared constructors: public instance
.ctor0600298E(flags6278/header32) and private static .cctor0600299C
(flags6289/header0). public_constructor_count=1. The public constructor takes
ObjectModel.Interfaces.INamedDomObject, ObjectModel.IFactory and a Func8
importer delegate returning ObjectModel.Import.ProjectImporter. That delegate's
parameter declarations include IProjectStore, Project, Common.Security.IKeyProvider,
a String transform and baggage/import-information callbacks. These are type
references, not Secure activation or approved service implementations.

Five other public declared methods are all instance methods: get_Item06002990,
ImportAsync06002991, Import06002992, GetEnumerator06002998 and
Synchronize0600299A. There is no public static factory declared on this root.
Base/inherited/external declarations remain unexamined. The constructor's
legitimate argument values, callback/lifetime behavior, context bootstrap and
catalog independence remain UNKNOWN. No SDK/vendor method or constructor,
Windows runtime, UI/state, import operation or hardware was executed. Catalog
acquisition/admission remains deferred; Task-009 ACs unchecked, no gate advances.
Saved-result independent audit PASS, verifier29/original-result-audit.txt
SHA-256 e4f91653818cb46f3bd99548a93b1a81eca22a1a0d43d4cdfbab348b20dc7387.
It checked all48 bindings, committed source pins, raw-report equality and flags/
counts without reading SDK payloads or repeating the query. Critical result
review is pending at this checkpoint.


Critical saved-result/two-document review PASS, no unresolved findings:
review29/original-result-review.txt SHA-256
42b83c97480bcd3794ad2e434ff118c225b6ba0491eb0f0f4086d2ec7239033d.
It checked48 bindings, six source pins/current Git contents, raw JSON and
constructor/method flags without SDK payload access or query repetition.
This closes the preceding review-pending checkpoint for this result only.
