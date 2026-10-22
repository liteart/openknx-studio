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
