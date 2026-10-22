# Static Project context qualification

Task-009 remains blocked on concrete SDK construction evidence. The accepted
Task-008 dependency analysis identified one denied `get_Project` request, not
successful initialization. This artifact defines the bounded static follow-up;
it does not authorize an original constructor, service, UI or device operation.

## Fixed observation contract

The explicit `project-context` experiment uses the existing three exact input
pins and the unchanged guarded metadata route. Its only root is
`Knx.Ets.Sdk.Project.Project` in the selected `Knx.Ets.Sdk` reference. There is
no user-supplied type name, directory search or dependency loading. The existing
`entrypoint-map` mode retains its behavior.

The report records assembly identities/references, the root and at most sixteen
same-assembly base definitions, type attributes, base names, interfaces,
declared constructor signatures with raw accessibility attributes, and declared
public/protected method signatures. Private constructors are included because
accessibility is part of construction evidence. External bases remain explicitly
unresolved; generic or otherwise unsupported local base shapes fail closed.
Missing/duplicate roots, cycles, malformed metadata and existing file, table,
signature, method and report limits fail with named diagnostics.

Only metadata tables and signature blobs are read using `PEReader` and
`MetadataReader`. No method body, custom attribute, CLR assembly load, dependency
resolution, constructor, static initializer or vendor code is executed. Owned
synthetic images exercise the selected contracts and denial paths. Vendor bytes
are read only by the existing restricted child after write/network denial probes.
All original bytes remain unchanged and outside Git.

## Interpretation boundary

Accessible constructors and matching signatures do not prove standalone
construction, correct fixture semantics, side-effect freedom, activation,
initialization completion or bus mediation. No uninitialized object, null/empty
Project, guessed service, ETS store or hidden executable is supplied. External
ObjectModel qualification requires the actual separately pinned candidate before
any new input mode or runtime allowlist can be proposed. Task-009 acceptance and
all runtime claims remain open pending actual evidence and independent review.

## Owned verification, 2026-10-22

On macOS, the provisioned PowerShell 7.6.7 command
`pwsh -NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1`
reported **PASS 23 / FAIL 0**. These are synthetic metadata images, including
the retained entrypoint tests and Project constructor visibility, external base,
cycle, depth, duplicate/missing root, malformed signature and type rejection
cases. They contain no vendor method bodies and are not Windows evidence.

`python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py`
reported **PASS**, 14 tests. The new fixed mode retains restriction-first
ordering, fixed worker selection and exact report-operation validation.
`python3 -B scripts/check_project_scope.py` and `git diff --check` reported
**PASS**. The coordinator subsequently completed the guarded original Project
metadata extraction described below.
Windows execution, original initialization and hardware tests are **NOT RUN**
for this increment. No Task-009 service acceptance criterion is asserted.

Source baseline: `4808d8a797a122dd815f388c60d2afdcc5a7d7b5` on
`task/Task-009-offline-context-qualification`. The reviewed source revision and
private report hashes must be added to Task-009 when available.


## Guarded original metadata evidence, 2026-10-22

The coordinator ran the fixed three-input mode, with the following command
(paths normalized as `PRIVATE_STAGE` and `PWSH`):

```text
python3 -B experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment project-context --output "$PRIVATE_STAGE/project-report-task009.json"
```

The private copy is `private/evidence/task009-objectmodel/project-report-task009.json`,
20,450 bytes, SHA-256
`85a3d77be7da708e7732618ab2b494457b342018f259f41e26b131985858bf40`.
Its guarded restriction results report PowerShell 7.6.7, `write_denied=true`
and `network_denied=true`; all three input hashes/sizes equal the fixed pins.
Report bytes/hash and the observations below were independently re-read from
JSON only. This is **PASS** for the selected static observation, with
`runtime_execution=NOT_RUN`; no SDK class or vendor implementation executed.

The selected Project type is `020001D1`, attributes `1048833` (public, sealed).
Its base chain is `Knx.Ets.Sdk.ProjectRelatedObject` -> `Knx.Ets.Sdk.DomObject`
-> external `System.Object`. The report explicitly leaves the external base
unresolved. Project declares one instance constructor, `06001409`, attributes
`6275`: access mask `3`, assembly/internal. Its parameters are:

- `Knx.Ets.Sdk.Project.ProjectCollection`
- `Knx.Ets.ObjectModel.Project.Project`, ObjectModel 6.4.8718.0
- `Knx.Ets.Sdk.ISdkContext`

The public `get_AddinData` (`0600140F`) and `get_Name` (`06001422`) declarations
match the previous direct member evidence. The public `get_Parent` (`06001435`)
returns `Knx.Ets.Sdk.Root`. No public Project instance constructor appears in
this selected declaration inventory. That alone does **not** prove that no
public factory or collection construction route exists; the parameter types
and factory surfaces have not yet been inspected. Nonpublic activation is not
attempted or justified by these signatures.

## ObjectModel acquisition and remaining dependency

The coordinator privately acquired one unchanged ObjectModel candidate. The
private `reference-inventory.json` is 860 bytes, SHA-256
`ff4711dda239231aa51d80d2a20771d4658d8f024514a540a659825615d8e8ad`.
Its record identifies 14,204,880 bytes, SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`,
`Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, exactly the missing requested identity.
The coordinator verified the private copy against the source-copy manifest;
this documentation update reads the inventory JSON, not the DLL. Acquisition
resolves the missing-file input. The inventory labels compatibility UNVERIFIED
and target assembly execution NOT_RUN; identity and hash do not establish
construction behavior. ADR-005's owner-authorized private-use scope remains
in force; no new legal or redistribution decision is made here.

No fourth input, ObjectModel metadata mode, runtime allowlist or fixture
service was added in the Project-only increment. Task-009 remains **Blocked** on a verified
concrete construction contract and meaningful offline fixture semantics.
A separate reviewed follow-up may inspect the fixed SDK roots
`ProjectCollection`, `ISdkContext` and `Root`, plus the exact
`Knx.Ets.ObjectModel.Project.Project` constructor parameter in a separately
pinned four-input static mode. Inspect declared constructors, public/protected
methods, interfaces and bounded same-assembly bases only, without method
bodies, attributes, dependency loading or vendor execution. A collection or
factory signature is evidence for a next inquiry, not permission to invoke it;
absence on these roots cannot prove global construction impossibility.

## Fixed construction inspection contract

The next static increment starts from reviewed metadata source
`f9549f6` and adds a separate `project-construction` mode. Existing modes keep
exactly their original three pins and commands. The new route adds only the
14,204,880-byte ObjectModel candidate with SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`
and requires its reported identity to match the acquired 6.4.8718.0 neutral
assembly, public-key token `61439873ec5e1159`. Four inputs total 21,709,216
bytes, within the unchanged 32 MiB aggregate limit. All pins are verified
before any PE reader; write/network denial probes remain mandatory first.

Only these exact roots are selected, without caller-supplied type names or
recursive constructor-parameter discovery:

- SDK `Knx.Ets.Sdk.Project.Project`
- SDK `Knx.Ets.Sdk.Project.ProjectCollection`
- SDK `Knx.Ets.Sdk.ISdkContext`
- SDK `Knx.Ets.Sdk.Root`
- ObjectModel `Knx.Ets.ObjectModel.Project.Project`

Each root reports its attributes, interfaces, constructors of every declared
accessibility, public/protected method declarations and at most sixteen local
base definitions. A nil interface base is reported as `NO_BASE`. Overlapping
base types are deduplicated; each root keeps its own base-chain disposition.
Missing/duplicate roots, cycles, unsupported base shapes and existing bounds
fail explicitly. The existing Project public-class check remains in force;
other fixed roots retain their observed visibility/class/interface attributes.

A method whose declared return signature contains one of the selected named
types may be marked `factory_candidate=DECLARATION_ONLY`. The annotation is a
bounded inquiry aid, including generic return wrappers, and proves no creation
behavior, semantics or callable standalone factory. No method body, custom
attribute, dependency resolution, SDK load or constructor/service invocation
is permitted. The result must keep unavailable external dependencies and
uninspected construction routes unresolved. Owned metadata fixtures and mocked
harness tests must pass before the coordinator's guarded original extraction.


## Construction mode owned verification and source freeze

At baseline `f9549f67c10b81216021fe8de5ccdf7f496cb614`, the fixed four-input
mode is implemented for static observation only. The same owned verification
commands listed above now report **PASS 39 / FAIL 0** for PowerShell and
**PASS**, 15 Python harness tests. The additional synthetic cases cover five
fixed roots, overlapping local bases, internal constructor parameter types,
an internal interface with nil base/`NO_BASE`, missing and duplicate SDK and
ObjectModel roots, base cycles, malformed signatures, all-four-pin verification
before any PE reader, old-mode input-count preservation, and named-return
candidate annotations. Generic selected returns are included; parameter-only,
foreign-assembly, prefix-name and hidden-method cases do not become candidates.
Construction signature identifiers containing display delimiters fail explicitly
to preserve unambiguous candidate matching. No constructor-parameter closure
is followed.

Harness tests verify restriction probes precede the metadata child, exact four
pin transfer, the fixed command/worker, ObjectModel report identity/hash
rejection, wrong-operation rejection and original-input output protection.
`python3 -B scripts/check_project_scope.py` and `git diff --check` report
**PASS**. Guarded four-input original metadata extraction is **NOT RUN** at
this freeze; the coordinator runs it after independent source review.
Windows execution, SDK/vendor implementation execution and hardware remain
**NOT RUN**. No service implementation, runtime allowlist or Task-009 acceptance
criterion is supplied by this mode. Source commit for this increment is pending
review; the prior Project-only increment is committed at `f9549f6`.

## Construction table-limit diagnostic contract

The coordinator's first four-input guarded extraction failed closed with
`DCAENTRY_ROW_LIMIT` before type inspection. No report or concrete construction
result was produced. The existing 32,768-row per-table bound remains unchanged.
Before considering any bound adjustment, a construction-only diagnostic must
identify the first exceeding ECMA table and its actual integer row count.

The reader emits an owned structured exception containing only a finite
`TableIndex` name and row count. The worker emits a path-free diagnostic such as
`DCAENTRY_ROW_LIMIT_METHODDEF_32769`. The worker may unwrap at most eight inner
exceptions and preserve fields only from that exact owned exception type;
it must not widen its generic raw-exception regular expression. Earlier modes
retain their existing generic `DCAENTRY_ROW_LIMIT` diagnostic and bounds.
An owned non-executable metadata fixture with exactly 32,769 MethodDef rows
must verify direct and worker diagnostics, plus the unchanged old-mode failure.
The original metadata retry remains coordinator-only after source review.


The owned PowerShell command now reports **PASS 42 / FAIL 0**, including the
exact MethodDef count, typed exception fields, worker-rendered
`DCAENTRY_ROW_LIMIT_METHODDEF_32769`, and the unchanged generic old-mode
`DCAENTRY_ROW_LIMIT`. The first test run failed because a test variable used
PowerShell's reserved `Error` name; renaming that test variable resolved it.
The Python harness remains **PASS**, 15 tests; scope and whitespace checks
**PASS**. At that diagnostic freeze the bound had not changed, and the original table
count was unknown pending the coordinator retry described below.

The coordinator reported that initial four-input staging at mode `0400` was
rejected with `DCAPROBE_INPUT_ATTRIBUTES`; input staging was corrected to the
existing required `0600`, without weakening validation. The later metadata
child failed with `DCAENTRY_ROW_LIMIT`; retained stderr is private under
`private/evidence/task009-construction/diagnostic-child-2-stderr.txt`.
This is a **FAIL** for extraction under current bounds, not SDK execution or
a construction result. No metadata report/types or working fixture were
produced. Original diagnostic retry, Windows execution, SDK implementation
execution and hardware are **NOT RUN** for this diagnostic increment.

## Evidence-backed fourth-input MethodDef policy

The coordinator's guarded diagnostic observed
`DCAENTRY_ROW_LIMIT_METHODDEF_38472` for the exact acquired fourth ObjectModel
input. The retained observation is private under
`private/evidence/task009-construction/row-observation-child-2-stderr.txt`.
This is a measured metadata count, not SDK execution or a construction result.

The reviewed policy permits a 65,536-row MethodDef ceiling only when all of
these fixed conditions hold: `project-construction` mode, fourth input slot,
and the exact pinned ObjectModel filename/size/hash. Every other table, the
first three inputs and all earlier modes retain the 32,768-row ceiling.
The fourth input's reported assembly identity remains checked by the guarded
parent. No file-size, aggregate-size, selected-method/type, signature, report,
output or deadline bound changes. All four hashes remain verified before any
PE reader. Owned fixtures must prove 38,472 and 65,536 MethodDef rows accepted,
65,537 rejected, and the unchanged denial of 32,769 rows for earlier modes,
input slots or other tables. Any original retry remains coordinator-only after
source freeze and review; Task-009 remains Blocked on concrete construction.


The narrow policy is implemented in the metadata reader: only construction
mode's fourth slot, with assembly name `Knx.Ets.ObjectModel`, may use 65,536
MethodDef rows. The guarded parent still fixes that slot's filename, size,
SHA-256 and reported identity; the owned synthetic tests use their own pinned
bytes with the same assembly name and never impersonate a runtime service.
All hashes are checked before a PE reader. No other table or mode receives
the exception.

The owned PowerShell command reports **PASS 50 / FAIL 0**. Full synthetic
construction reports succeed at 38,472 and 65,536 MethodDef rows; the worker
rejects 65,537 with `DCAENTRY_ROW_LIMIT_METHODDEF_65537`. Fixtures also prove
that the first three slots, both earlier reader modes, fourth-slot TypeDef
32,769 and a different fourth-slot assembly name retain the 32,768 ceiling.
Padding methods belong only to unselected fixture types; selected-method
budgets are unchanged. Python harness tests remain **PASS**, 15 tests; scope
and whitespace checks **PASS**. Guarded original extraction under this new
policy is **NOT RUN** at source freeze, pending review and coordinator retry.
Windows, SDK implementation execution and hardware remain **NOT RUN**.


## Guarded construction extraction result, 2026-10-22

After the narrow MethodDef policy review, the coordinator's original metadata
retry completed **PASS** on macOS/Darwin arm64 with PowerShell 7.6.7. This
supersedes the earlier source-freeze **NOT RUN** entries for static extraction
only. The normalized command was:

```text
python3 -B experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment project-construction --output "$PRIVATE_STAGE/project-construction-report-task009.json"
```

The report was preserved privately at
`private/evidence/task009-construction/project-construction-report-task009.json`,
103,345 bytes, SHA-256
`5253681cc48e42142a7d214e9229d2d9b68c8f3580152c0ff4a63fb6129d19a2`.
A JSON-only re-read verified those bytes/hash, `write_denied=true`,
`network_denied=true`, all four fixed input sizes/hashes/identities,
`operation=static_dca_project_construction` and `runtime_execution=NOT_RUN`.
There are five selected roots and eleven deduplicated selected/base types.
`ISdkContext` has the valid `NO_BASE` disposition; the other roots end in
unresolved external `System.Object` declarations. No dependency was loaded.

The actual declarations narrow the next inquiry:

| Observed type/member | Construction evidence and remaining requirement |
|---|---|
| SDK `ISdkContext`, `0200001F`, attributes `1048736` | Nonpublic interface. Its getters include ObjectModel `IProjectStore` (`060001A2`), ObjectModel `IFactory` (`060001A4`), SDK `ISdkFactory` (`060001A7`), and other uninspected service types. No context implementation is supplied. |
| SDK `Root` constructor `0600026C` | Assembly/internal access, attributes `6275`; parameters are ObjectModel `Project.Project`, `IProjectStore`, SDK `ExecutionContext`, and ObjectModel `Compatibility.IPluginInstaller`. Public `get_Projects` (`06000274`) returns SDK `ProjectCollection`. |
| SDK `Project.Project` constructor `06001409` | Assembly/internal access, attributes `6275`; requires SDK `ProjectCollection`, ObjectModel `Project.Project`, and SDK `ISdkContext`. |
| SDK `Project.ProjectCollection` constructor `06001446` | Assembly/internal access, attributes `6275`; requires ObjectModel `Project.ProjectCollection`, SDK `Root`, and SDK `ISdkContext`. |
| ObjectModel `Project.Project`, `020003A5` | Five public constructor declarations, attributes `6278`, access mask `6`; every overload requires `IFactory` and `IProjectStore`. Their implementations and these service contracts remain uninspected. |

The five public ObjectModel overloads have these additional parameter shapes:

- `06002830`: `String` and Common.Types `GroupAddressStyle`.
- `06002831`: ObjectModel `ProjectCollection` and `IObjectContext`.
- `06002832`: ObjectModel `ProjectCollection`.
- `06002833`: `Guid`, ObjectModel `ProjectCollection`, and `IObjectContext`.
- `06002834`: `Guid` and ObjectModel `ProjectCollection`.

These are overload signatures, not evidence of optional/default parameter
semantics or a safe constructor invocation. Every `DECLARATION_ONLY` annotation
in the selected report names a getter, indexer or enumerator. None establishes
a creation factory. Internal SDK constructors and the nonpublic context are
concrete compatibility constraints; this bounded surface cannot establish
that all possible construction routes are impossible or available.

The coordinator reports independent verification **PASS** for 50 owned
PowerShell cases, 15 Python harness tests, scope and whitespace checks, plus
independent static review **PASS**. Private verifier logs are
`verifier-powershell.log`, `verifier-python.log`, `verifier-scope.log` and
`verifier-diff-check.log` under `private/evidence/task009-construction`.
These results concern static tooling/metadata, not Task-009 service acceptance.
The owned reproduction commands remain:

```text
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1
python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py
python3 -B scripts/check_project_scope.py
git diff --check
```

Source baseline is `f9549f67c10b81216021fe8de5ccdf7f496cb614`; the reviewed
four-input source is committed at `6c9d03d7cb11be764d1ba572a7116ce7655c25dc`. The extracted reader SHA-256 is
`406951a0554d220f3bc285ed3d8267be6ce82d8979232e717a18d596317c10aa`,
worker `0656d1d1e1f45ad2a32521775e04b6be5ccad4185e8d59410838d42c0a575325`,
and guarded launcher
`93bbfcdd73644c508ce4fe438374a64edd2000191fd7c32844c1958588d13abf`.
The JSON-only documentation update did not read a DLL or change code. The
successful-run source record is private `source-freeze-success.json`, 1,008
bytes, SHA-256
`5e621640dd851b966a69f6de3fc63a57e1819f57a715ae3024422b54145ce2c6`.
It records the six reviewed code hashes following execution; all six match the
current owned files on independent re-hash. Earlier `source-freeze.json` and
`source-freeze-row-diagnostic.json` are historical snapshots from before the
narrow policy, not successful-run source manifests. Their expected differences
do not represent relaxed checks. The coordinator also reports independent
JSON verification PASS for the report hash, restrictions, four pins, five
roots/eleven types, internal SDK constructors and five public ObjectModel
constructor declarations.

Task-009 remains **Blocked**. The smallest next technical inquiry is a separately
reviewed fixed inspection of SDK `ExecutionContext`/`ISdkFactory` and ObjectModel
`IFactory`/`IProjectStore`/`IObjectContext` declarations, all named by observed
signatures. No additional root is selected now, and no recursive parameter
catalog, service implementation or runtime allowlist expansion follows.
Null/uninitialized objects and nonpublic activation are not supplied.
Windows execution, SDK/vendor implementation execution, original initialization
retry, UI/state round trips and hardware remain **NOT RUN** for this increment.

## Fixed service-contract inquiry

A separate `project-services` static experiment starts from reviewed source
`e037bead9c00d116342baa561d833253adb5c615`. It selects only SDK
`Knx.Ets.Sdk.ExecutionContext`, SDK `Knx.Ets.Sdk.ISdkFactory`, ObjectModel
`Knx.Ets.ObjectModel.IFactory`, ObjectModel `Knx.Ets.ObjectModel.IProjectStore`
and ObjectModel `Knx.Ets.ObjectModel.IObjectContext`. Each name comes from the
successful construction report's actual signatures. No root of the previous
`project-construction` experiment is replaced or expanded.

The new mode uses the same four exact pins, guarded write/network denial
probes, and all-hashes-before-PE-reader ordering. Only the fourth ObjectModel
slot's MethodDef table may use the reviewed 65,536-row ceiling; other tables,
inputs and earlier three-input modes stay at 32,768. File, aggregate, selected
method/type, signature, report/output and process bounds remain unchanged.
Each root reports declarations only: attributes, interfaces, constructors of
every visibility, public/protected methods and bounded same-assembly bases.
Nil interface bases are valid `NO_BASE`; absent/duplicate roots and existing
limit violations fail explicitly. Return annotations refer only to these five
selected service types and remain `DECLARATION_ONLY`.

No constructor parameters or interface references are followed recursively.
No method body, custom attribute, vendor assembly load, service implementation,
SDK clone, extra DLL or runtime allowlist is supplied. A selected-method limit
failure remains a failure for coordinator review rather than permission to
increase the limit. Owned tests and independent review precede any coordinator
guarded extraction. Task-009 remains Blocked on an evidenced usable context.


The new service inquiry's owned verification reports **PASS 68 / FAIL 0** for
the PowerShell metadata suite and **PASS**, 16 Python harness tests. Fixtures
cover exactly the new five roots, one local base, nonpublic interfaces with
nil bases, interface references without recursive inspection, constructor
visibility and return annotations, SDK/ObjectModel missing and duplicate roots,
and selected-method-limit failure. Earlier entrypoint/Project/construction
reports are compared byte-for-byte before and after service selection.
Fourth-slot MethodDef cases at 38,472 and 65,536 succeed, 65,537 fails; other
tables and earlier-slot ceilings, input counts, all-pin verification ordering
and mutually exclusive worker modes are tested. Python tests cover the fixed
service switch/operation, unchanged four pins and identity guard, restriction
ordering and output protection.

Scope and whitespace checks **PASS**. Source baseline is
`e037bead9c00d116342baa561d833253adb5c615`; at that freeze the service inquiry
commit and independent verification/review were pending. The new original
guarded metadata extraction was **NOT RUN** at that source freeze. The coordinator may run only
the fixed `project-services` mode after source review; no root or limit grows
automatically from a failed observation. Windows execution, SDK implementation
execution, original initialization, UI/state round trips and hardware remain
**NOT RUN**. No usable service or working factory is claimed.


## Guarded service-contract result, 2026-10-22

The coordinator's fixed `project-services` extraction completed **PASS** on
macOS/Darwin arm64, PowerShell 7.6.7, using committed source
`5c7bc46e43f1cdd013214fce796b5b8fd6962719`. This supersedes the service
source-freeze's **NOT RUN** for static extraction and pending source review.
The normalized exact guarded command was:

```text
python3 -B experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment project-services --output "$PRIVATE_STAGE/project-services-report-task009.json"
```

The preserved private report is
`private/evidence/task009-services/project-services-report-task009.json`,
54,190 bytes, SHA-256
`19194b3a2c0b4faf91ef412045291f2c3ea7dc155a72842079566ed174899cdd`.
A JSON-only re-read verified the bytes/hash, both write/network denial probes
true, all four exact input sizes/hashes/identities, operation
`static_dca_project_services`, and `runtime_execution=NOT_RUN`. Five selected
roots produced five types, with no same-assembly base expansion. Four interface
roots have `NO_BASE`; ExecutionContext ends at unresolved external System.Object.
Referenced inherited interfaces were recorded, not recursively inspected.

| Observed root | Declared surface and limit of the evidence |
|---|---|
| SDK `ExecutionContext`, `0200001A` | Nonpublic abstract class, attributes `1048704`; 20 reported declarations. Protected constructor `0600015C`, attributes `6276`, access mask `4`, accepts ObjectModel `IDomObject` and `Object`. Validation and context-access member names do not establish a valid host context. |
| SDK `ISdkFactory`, `02000020` | Nonpublic interface, attributes `1048736`; 13 declarations, including instance generic `Create` overloads requiring SDK objects/context. No concrete factory instance is qualified. |
| ObjectModel `IFactory`, `02000037` | Public interface, attributes `1048737`; 52 declarations. It includes two public instance `CreateProject` signatures, detailed below; no implementing factory or store behavior was inspected. |
| ObjectModel `IObjectContext`, `02000044` | Public interface; 25 declarations and inherited `ICacheProvider` reference. Project, validator, undo/cache and modification-service declarations do not supply state semantics. |
| ObjectModel `IProjectStore`, `02000057` | Public interface; 36 declarations and inherited `IProductStore` reference. Store accessors and an instance `Clone` declaration do not establish initial store provisioning. |

ObjectModel `IFactory.CreateProject` `060001FE` returns ObjectModel
`Project.Project` from `(IProjectStore, String, GroupAddressStyle)`.
Overload `060001FF` returns the same type from `(Guid, ProjectCollection,
IProjectStore)`. These are creation-related interface declarations requiring
an existing factory implementation and store; they are not a demonstrated
standalone creation route. The report marks them `NOT_SELECTED_RETURN`
because Project is outside this mode's five service return types. That
annotation is a return-type filter, not a semantic factory classifier or a
claim that `CreateProject` is absent.

`IProjectStore.Clone` `06000315` is the sole `DECLARATION_ONLY` return match.
Its signature is instance `IProjectStore(ProjectId)`: it requires an existing
store receiver and a ProjectId, so the declaration alone does not establish a
startup factory. No clone, CreateProject, validation method or constructor
was invoked. Generic constraints, implementations, inherited-interface member
closures and semantic state requirements were not inspected by this mode.
The prior SDK Root constructor remains internal; no compatible standalone
SDK context/Project construction path is proven.

The coordinator reports independent **PASS 68 / FAIL 0** for owned PowerShell
metadata tests, **PASS** for 16 Python harness tests, scope/whitespace **PASS**
and independent source review **PASS**. Private verifier logs are
`verifier-powershell.log`, `verifier-python.log`, `verifier-scope.log` and
`verifier-diff-check.log` under `private/evidence/task009-services`.
The reproduction commands are the same owned suite, Python, scope and diff
commands recorded above. These are static tooling/observation results only;
no Task-009 service acceptance or human gate approval follows.

The private successful-run manifest is
`private/evidence/task009-services/source-freeze-success.json`, 876 bytes,
SHA-256 `6c113b623ff78bd11d2df3985b44779cffafc850c4a923045b20928a47ef2f79`.
It records source commit `5c7bc46e43f1cdd013214fce796b5b8fd6962719` and six
code hashes **after** guarded execution. A JSON-only read and re-hash of the
six owned source files matched every entry. This documentation update changes
no code and reads no DLL.

Task-009 remains **Blocked** on concrete fixture/runtime context semantics.
The bounded unresolved contract questions are the concrete provisioning and
invariants for the observed `IFactory.CreateProject`/`IProjectStore` route,
inherited `IProductStore`/`ICacheProvider` obligations, and the nonpublic SDK
factory/context requirements. No new mode, root, parameter catalog or experiment
is added by this record. No additional human input is required to complete
this static evidence update. SDK/vendor implementation execution, Windows
execution, original initialization retry, UI/state round trips and hardware
remain **NOT RUN** for this increment.

## Fixed provider-relationship lookup contract

A separate `project-providers` mode starts from reviewed source
`69a4c16c0ced39f9c95b5bd1fb2e613267893a7e`. It uses the same four exact inputs
and guarded route. Its only targets are SDK `ExecutionContext`, `ISdkFactory`,
`ISdkContext`, and ObjectModel `IFactory`, `IProjectStore`, `IObjectContext`,
all observed in earlier actual metadata. Targets are fixed by owner assembly
and full type name, not supplied by the caller.

The lookup scans direct TypeDef base and InterfaceImpl edges within the
existing bounded tables. A matching candidate records its input identity,
token, full name, attributes and exact direct edge/target ownership. Target
ownership must equal the selected SDK/ObjectModel assembly identity; a
same-named type from another assembly/version/token is not a match. At most
64 matching candidate types and 128 matching edges may be reported; exceeding
either fails explicitly. No method, constructor, custom attribute, implementation,
recursive inheritance or parameter graph is inspected. Ordinary type names
alone do not identify providers.

Generic/unsupported edges and unresolved owner shapes are counted as
uncertainty without dumping a global type catalog. Indirect and external
implementation routes are explicitly unsearched. Empty direct matches remain
INCONCLUSIVE for creation, and positive matches prove only a relationship,
not construction or usable services. All hashes are verified before any PE
reader; the fourth ObjectModel MethodDef exception remains 65,536 only, while
other table/input and file, aggregate, output/report and deadline bounds stay
unchanged. Existing modes/commands/reports retain their behavior. Owned tests,
source freeze and independent review precede coordinator-only original lookup.


The provider mode's owned verification reports **PASS 89 / FAIL 0** for the
PowerShell suite and **PASS**, 17 Python harness tests. Synthetic cases prove
local TypeDef and exact cross-assembly TypeRef matches, internal/abstract
candidate attributes, foreign-owner/version/token and prefix-name exclusion,
indirect inheritance not claimed, generic TypeSpec uncertainty, missing and
duplicate fixed targets, empty-result INCONCLUSIVE, and acceptance of exactly
64 candidates/128 edges with explicit failure at 65 candidates or 129 edges.
Malformed method signatures on targets and matching candidates remain unread.
All earlier reader reports are compared byte-for-byte after provider lookup.

The reviewed fourth-input MethodDef cases remain accepted at 38,472 and
65,536 and fail at 65,537; earlier-slot row bounds, all-pin verification before
PE reading, input counts and exclusive worker modes are exercised. Python
tests cover the new fixed switch/operation through the existing restriction,
identity, pin and output guards. A read-only AST comparison against source
`69a4c16` verifies eleven prior pin, guard, bound and command expressions are
unchanged. The relationship walk is additionally bounded to 262,144 non-nil
direct edges, derived from four unchanged TypeDef/InterfaceImpl table limits;
per-type interface and report limits are retained.

Scope and whitespace checks **PASS**. Source baseline is
`69a4c16c0ced39f9c95b5bd1fb2e613267893a7e`; at that freeze the provider source
commit and independent verification/review were pending. Coordinator guarded
`project-providers` extraction was **NOT RUN** at that freeze. No original
method, constructor or vendor implementation executes. Task-009 remains
**Blocked**; an actual guarded relationship report is required before choosing
any matched constructor surface. Windows, initialization/UI/state and hardware
proofs remain **NOT RUN**.


## Guarded provider-relationship result, 2026-10-22

The coordinator's fixed lookup completed **PASS** for static metadata at source
`b11c234273e3e61e0563c6474cfd694dacf674eb`, on macOS/Darwin arm64 with
PowerShell 7.6.7. This supersedes the provider freeze's **NOT RUN** and pending
source review. The normalized exact guarded command was:

```text
python3 -B experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment project-providers --output "$PRIVATE_STAGE/project-providers-report-task009.json"
```

The preserved private report is
`private/evidence/task009-providers/project-providers-report-task009.json`,
10,498 bytes, SHA-256
`3433af94b9c47ea2218b1b0b59a72bbe59ea880f8173057e0db5d01691560dd7`.
The JSON-only re-read verified bytes/hash, both denial probes true, all four
exact input sizes/hashes/identities, six targets, eighteen candidates/eighteen
direct edges, `runtime_execution=NOT_RUN` and
`creation_route=INCONCLUSIVE`. It records 829 `TYPESPEC_NOT_INSPECTED` edges.
Generic/indirect/external routes remain unresolved; these matches do not give
a complete implementation inventory.

Every SDK name below is under `Knx.Ets.Sdk`; every ObjectModel name is under
`Knx.Ets.ObjectModel`. The candidate index is its zero-based index in
`metadata.candidates` in the private report. The owner identity on every listed
edge exactly matches the selected SDK or ObjectModel 6.4.8718.0 assembly.

| Index | Candidate type / token | Raw attributes | Direct relationship |
|---|---|---|---|
| 0 | SDK `AddinExecutionContext`, `02000007` | `1048576` | extends SDK `ExecutionContext` |
| 1 | SDK `CompatibilityExecutionContext`, `02000009` | `1048576` | extends SDK `ExecutionContext` |
| 2 | SDK `Context`, `0200000B` | `1048576` | implements SDK `ISdkContext` |
| 3 | SDK `DataProviderExecutionContext`, `0200000C` | `1048576` | extends SDK `ExecutionContext` |
| 4 | SDK `DcaExecutionContext`, `0200000D` | `1048576` | extends SDK `ExecutionContext` |
| 5 | SDK `DeviceInfoExecutionContext`, `02000011` | `1048576` | extends SDK `ExecutionContext` |
| 6 | SDK `DeviceScriptExecutionContext`, `02000012` | `1048576` | extends SDK `ExecutionContext` |
| 7 | SDK `DevicePluginExecutionContext`, `02000013` | `1048576` | extends SDK `ExecutionContext` |
| 8 | SDK `DownloadPluginExecutionContext`, `02000019` | `1048576` | extends SDK `ExecutionContext` |
| 9 | SDK `ParameterTypePluginExecutionContext`, `02000023` | `1048576` | extends SDK `ExecutionContext` |
| 10 | SDK `SdkFactory`, `0200002D` | `1048576` | implements SDK `ISdkFactory` |
| 11 | SDK `DataHandlerExecutionContext`, `0200003E` | `1048576` | extends SDK `ExecutionContext` |
| 12 | ObjectModel `Factory`, `02000023` | `1048577` | implements ObjectModel `IFactory` |
| 13 | ObjectModel `ObjectContext`, `0200007D` | `1048577` | implements ObjectModel `IObjectContext` |
| 14 | ObjectModel `ZipContainerStore.ZipContainerProjectStore`, `020000BE` | `1048577` | implements ObjectModel `IProjectStore` |
| 15 | ObjectModel `FileSystemStore.FileProjectStore`, `02000DCA` | `1048577` | implements ObjectModel `IProjectStore` |
| 16 | ObjectModel `FileSystemStore.IFileProjectStore`, `02000DD1` | `1048737` | interface edge to ObjectModel `IProjectStore` |
| 17 | ObjectModel `FileSystemStore.ImportFileProjectStore`, `02000DD7` | `1048576` | implements ObjectModel `IProjectStore` |

`1048576` (`0x100000`) means nonpublic/nonabstract/noninterface with
BeforeFieldInit; it does not establish constructor access. `1048577`
(`0x100001`) is public/nonabstract/noninterface with BeforeFieldInit.
`1048737` (`0x1000A1`) is public/abstract/interface with BeforeFieldInit;
`IFileProjectStore` is therefore not a concrete store implementation. No
candidate constructor, method, custom attribute, activation or implementation
was inspected. The prior report's abstract ExecutionContext is distinct from
these nonabstract derived type declarations. Names such as DcaExecutionContext
are evidence for selecting a next inquiry, not proof of correct DCA behavior.

The original store class names are observed dependencies only. GOALS excludes
requiring an ETS project store after migration or for a new project. Finding
FileProjectStore/ZipContainerProjectStore does not approve that dependence,
vendor persistence as editor authority, or provisioning either class. Their
requirements and independent-use behavior remain untested.

The coordinator reports independent owned PowerShell **PASS 89 / FAIL 0**,
Python **PASS** (17 tests), scope/whitespace **PASS**, and source review **PASS**.
The verifier originally wrote four logs under worktree-relative
`private/evidence/task009-providers`. The coordinator copied those unchanged to
the canonical evidence directory while preserving the originals; this was a
copy operation, not another test execution. Canonical filenames are
`verifier-powershell.log`, `verifier-python.log`, `verifier-scope.log` and
`verifier-diff-check.log`. The JSON-only re-read of
`private/evidence/task009-providers/log-copy-provenance.json`, 851 bytes,
SHA-256 `23783ce1091f9eb7f2e6bd84d6b16179fdc928b479cfd66a1d88c36ca5e22e6f`,
confirms the recorded source path, unchanged-copy operation, four file
sizes/hashes and preserved-originals flag. No relocated execution is claimed.

The private provider `source-freeze-success.json` is 876 bytes, SHA-256
`f404fd0b51ef9d858456c55f722f6ace09b77db7e955228a4543668510a6d4ed`.
Its JSON-only read verifies source commit
`b11c234273e3e61e0563c6474cfd694dacf674eb` and the explicit post-execution
recording flag. All six recorded owned code hashes match the current committed
files on re-hash. This successful-run source record does not replace or rewrite
historical freezes or the log-copy provenance.

The smallest next technical inquiry is constructor declarations only for the
five now-observed types: SDK `DcaExecutionContext`, `Context`, `SdkFactory`,
and ObjectModel `Factory`, `ObjectContext`. No new mode or root is added now,
and no store constructors or recursive parameter catalog are selected. A
separate reviewed inspection must precede any decision about a genuine fixture
or invocation. This documentation update changes no code and reads no DLL.
Task-009 remains **Blocked**; creation remains **INCONCLUSIVE**. Windows/vendor
execution, nonpublic activation, original initialization, UI/state and hardware
remain **NOT RUN** for this increment.

## Fixed provider-constructor inspection contract

A separate `project-constructors` mode begins from reviewed source `a110775`.
Its only roots are the five types observed by the provider lookup: SDK
`DcaExecutionContext` (`0200000D`), `Context` (`0200000B`), `SdkFactory`
(`0200002D`), and ObjectModel `Factory` (`02000023`), `ObjectContext`
(`0200007D`). Names and owner assemblies are fixed by the mode; the recorded
production tokens are provenance, not caller-controlled selectors.

The mode uses the same four exact pins, denial probes and all-hashes-before-PE
ordering. It records only selected type identity/token/attributes/base name
and declared `.ctor`/`.cctor` signatures with raw accessibility/static flags.
Every declared constructor visibility is included. No base/interface closure,
nonconstructor member, store constructor, parameter catalog, method body,
custom attribute or implementation is inspected. Missing/duplicate roots and
existing signature/method/table/report limits fail explicitly; old modes and
commands retain their content and behavior. The fourth ObjectModel MethodDef
ceiling remains 65,536 only on this fixed four-input route, with every other
bound unchanged.

Constructor declarations and public/nonpublic flags do not establish side-effect
freedom, valid arguments, fixture semantics or a standalone creation route.
No original constructor/static initializer is invoked, and no nonpublic
activation, runtime allowlist expansion or service is supplied. Owned tests,
source freeze and independent verification/review precede any coordinator-only
guarded original extraction. Task-009 remains Blocked pending genuine context
evidence.


The constructor-only increment's owned verification reports **PASS 112 / FAIL 0**
for the PowerShell suite and **PASS**, 18 Python harness tests. Synthetic images
cover the exact five names, private/protected/public/internal constructor flags,
static `.cctor` declarations, missing/duplicate roots in both owner assemblies,
malformed constructor rejection and the unchanged selected-method ceiling.
Malformed nonconstructor signatures, unselected base constructors and original
store-name constructors remain unread; the report contains no nonconstructor
member or interface inventory. Earlier entrypoint/Project/construction/service/
provider reports are compared byte-for-byte after constructor-only lookup.

Fourth-slot MethodDef counts 38,472 and 65,536 remain accepted, 65,537 fails;
other-table and earlier-slot ceilings, hash-before-reader ordering, input
counts and exclusive worker switches are tested. Python tests exercise the
fixed constructor switch/operation through existing restriction, exact-pin,
identity and output guards. A read-only AST comparison to baseline verifies
twelve prior pin/guard/bound/command expressions unchanged. Scope and
whitespace checks **PASS**.

Source baseline is `a110775b07dcdb9f1238abae24e54c716c2b3e12`; new source and
result commits are pending. Independent verification/review and the
coordinator's original guarded `project-constructors` extraction are **NOT RUN**
at this freeze. No original constructor or initializer invocation, nonpublic
activation, SDK runtime load or service is supplied. Task-009 remains
**Blocked**, and creation remains **INCONCLUSIVE** pending actual declarations
and genuine context evidence. Windows, initialization/UI/state and hardware
proofs remain **NOT RUN** for this increment.

The constructor-only mode also retains the 256 AssemblyRef ceiling without
reporting an assembly-reference catalog. Owned 257-reference images in both
selected owner assemblies fail with `DCAENTRY_REFERENCE_LIMIT`.


## Guarded provider-constructor result, 2026-10-22

The coordinator's constructor-only static observation PASS uses reviewed source
`1df83e9385bf7bfcc46c927756eab06a5126e06d`.
Independent verification and critical review reproduced owned PowerShell
112/112, Python 18/18, scope and whitespace PASS. Logs and the source hash
manifest are in `private/evidence/task009-constructors/verifier/`.
The exact guarded command from this task worktree was:

```sh
python3 experiments/dca-metadata/run_probe.py --stage <private-four-pin-stage> --pwsh <provisioned-powershell-7.6.7> --experiment project-constructors --output <private-four-pin-stage>/project-constructors-report-task009.json
```

The placeholders normalize private local paths; they do not select inputs or
weaken the fixed four pins. Exit status was 0 on macOS/Darwin arm64 with
PowerShell 7.6.7. Both actual restriction probes report true: filesystem writes
and network access were denied before the static read. The private result
`private/evidence/task009-constructors/project-constructors-report-task009.json`
is 5,969 bytes, SHA-256
`df361cb84fc232b6f7da6943114096cd3e14b638495dcc8b68e10dbd42524ea9`.
The post-run `source-freeze-success.json` records the exact source commit and
six owned source hashes. All four pinned original input identities match.

Exactly five types each declare one instance constructor and one private static
initializer. Context and DcaExecutionContext instance constructors are internal;
SdkFactory's is public although its declaring type is internal. ObjectModel
Factory and ObjectContext types and instance constructors are public. Observed
instance signatures, with owner versions retained in the private JSON:

| Type | Instance constructor arguments |
|---|---|
| SDK Context | SDK Root, ObjectModel IProjectStore, ObjectModel Project, SDK ExecutionContext, ObjectModel IPluginInstaller, Autofac IComponentContext |
| SDK DcaExecutionContext | Func of IEnumerable of ObjectModel Device, Object, String |
| SDK SdkFactory | None |
| ObjectModel Factory | Autofac IComponentContext, Knx.Kim.DataProvider IKimDataProvider |
| ObjectModel ObjectContext | ObjectModel Project, IFactory, IProjectStore |

Autofac is referenced at version 8.2.0.0; Knx.Kim.DataProvider at 1.0.0.0.
These are signature dependencies, not provisioned runtime components or working
services. No constructor or static initializer was invoked, and no null-backed
or uninitialized Project was supplied. No original store is approved as editor
authority. The earlier Root constructor declaration already accepts an
ObjectModel Project, IProjectStore, ExecutionContext and IPluginInstaller; a
controlled fixture route must resolve those arguments and its access before a
Windows construction experiment. Factory method bodies and behavior remain
unknown. Further metadata catalogs do not substitute for that experiment.

Task-009 remains Blocked; creation_route is INCONCLUSIVE and runtime_execution
is NOT_RUN. This supersedes only the constructor source-freeze's pending static
extraction, verification and review. Windows hosting, initialization/UI/state,
hardware, SDK runtime allowlist changes and nonpublic activation remain NOT RUN
for this increment.
