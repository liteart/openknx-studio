# Static Project context qualification

## Project07 original Id-only fixture diagnostic result

After reviewed source commit `3161dd889fe4a8f995cf122542a06503c94b9172`,
exactly one `original-project-store-id` attempt **FAIL**, exit 10. All twelve
private pins and the exact OS facade pin/alias passed. Original ProjectId public
constructor `060001EF` **RETURNED**; public numeric getter `060001F2`
**RETURNED**, validating exact UInt32 `1`. This remains explicitly owned
synthetic data, not a real/manufacturer identity or claimed-valid SDK ProjectId.
The proxy supplied that same labeled object for one observed
`IProjectStore.get_Id` request (`060002F5`, zero arguments).

The next request was `IFactory.UndoManager`, token `060001FD`, two arguments,
explicitly denied unsupported. Project constructor `06002830` was
**ATTEMPTED_NOT_RETURNED**: returned=False, fixture-calls=1, denied-requests=1,
TargetInvocationException -> NotSupportedException
`PROJECT_FIXTURE_UNSUPPORTED:Knx.Ets.ObjectModel.IFactory.UndoManager`.
Project body entry remains **NOT_ESTABLISHED_BY_STACK_TRACE**. No dependency
resolution blocker or unexpected assembly marker occurred. No retry, dependency
or service expansion followed. Earlier source/owned NOT_RUN records remain
historical; they are not rewritten as original execution evidence.

Private `task009-constructor-windows/project07-observation.json`, 4,058 bytes,
SHA-256 `4f307c0b31dce38319c5fa51384c7b8ab236fbbc6aa7a1139fe54c6505482fdc`,
binds fifteen provenance files. Raw project07.log, 7,321 bytes, SHA-256
`36ff5591ea9589d2f28e56081dd25b48e05146ad0fdab9331dc1a59f124618af`,
agrees with independent result-audit **PASS** at verifier07/result-audit.txt,
SHA-256 `ef16ee954718cac43a99116d09195cf0080f72dc35c13e2e96ddda36caef91d3`.
Context was interactive Session2/nonadmin; launcher confirmed child exit 10
under 15 seconds/65,536-byte capture. Cleanup expected/last exit 10, task removed,
zero owned processes/scratch files; network/descendants remain NOT_CONTAINED.

Owned Windows07 tests remain **PASS 15 / 25 / 20**, distinct from this actual
original diagnostic **FAIL**. Task-009 stays Blocked, ACs unchecked; no completed
Project, valid fixture/service, DCA initialization/UI/state, hardware or gate
qualification is established. Next work is metadata readiness investigation of
the observed service only; no positive UndoManager fixture is authorized here.
Scope/diff checks **PASS**; this result append changes no source or pin.

## Project-store Id fixture experiment contract, source freeze after metadata

Baseline `91121b184043e32db930fd596e751a53ee69751a` contains the reviewed
ProjectId metadata result. New explicit `original-project-store-id` and
`owned-project-store-id` modes retain the existing input, platform facade,
nonadmin interactive STA, loader, 15-second supervised/64 KiB capture and
96-assembly bounds. The original deny-all project mode remains available.
Only the already pinned twelve inputs are allowed; no new dependency or resolver
is added. Original execution waits for independent source review and commit.

The fixed original Id contract is ProjectId type `0200002C`, flags `1056769`,
public `.ctor(UInt32)` `060001EF` and public `get_ProjectNumber():UInt32`
`060001F2`, Ids 6.4.8718.0/neutral/PKT61439873ec5e1159. Source pins metadata
report SHA-256 `37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4`
and observation `89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a`.
The sole synthetic input is source-fixed UInt32 `1`, explicitly owned fixture
data, not a real/manufacturer project identity or a claimed valid value. Exactly
one constructor and one numeric getter attempt are logged BEGIN/END; only an
exact-type return and exact UInt32 `1` validation make the object available.
Any failure stops; no alternate value, string parser, generator or private API.

A separate proxy permits only exact ObjectModel IProjectStore owner `02000057`,
public instance get_Id `060002F5`, zero arguments and the exact reviewed Id
return type. It returns the same labeled fixture object, at most sixteen calls;
saturation and every other method/interface/malformed request fail explicitly
unsupported. Positive fixture calls and denied requests have separate bounded
counters and logs. The original Project constructor is always unqualified even
if it returns or catches a denial. No SDK Root/Factory activation, DCA
initialization, UI/state, device operation, gate or Task-009 acceptance follows.
Owned shapes test this narrow behavior without impersonating vendor tokens or
signatures. New original Id/fixture runtime execution is **NOT RUN** at freeze.

Local source verification **PASS** using the provisioned PowerShell 7.6.7 and
private `knx-task009-store-id-sourcecheck.ps1` helper, SHA-256
`098ed5c55bae0fd7917b89bc90818731e7b51c39a95700fdb0c62d6c3a2e40d8`:

```text
pwsh -NoLogo -NoProfile -NonInteractive -File PRIVATE_TMP/knx-task009-store-id-sourcecheck.ps1 -Worktree WORKTREE -FrameworkReferences PRIVATE_EVIDENCE/task148-clean-host/references
python3 -B scripts/check_project_scope.py
git diff --check
```

The helper verifies the six existing .NET 4.8 reference hashes, compiles C#5/x64
with warnings as errors in memory, parses the PowerShell driver and executes its
old/new source guards. Local emitted sizes: ProjectConstructorProbe 66,560 bytes,
ProbeLauncher 37,888 bytes, owned fixtures 13,824 bytes. This is local Roslyn
compilation against Windows reference assemblies, **not Windows runtime tests**.
The four changed tool hashes are frozen below; source commit/review are pending.

| Frozen source | SHA-256 |
|---|---|
| ProjectConstructorProbe.cs | `485abb9c6d9b8c324e7e781c02127cd69c77ce9ae0c4984e91ba1bb8e8d94549` |
| ProbeLauncher.cs | `3b34d54f629d505c3992b781137aad483da80699dc841f85adb59e275db3d94d` |
| OwnedProjectConstructorFixtures.cs | `f84737d465d6c149fa303f958406bed86e53d4d2ce085ab70bf8379e640a6889` |
| Test-ConstructorProbe.ps1 | `0e4d1a7bdee06f3303671ab3cb5502aa496778021c83621f8ebee1690af7de37` |

Owned Windows regressions retain the earlier cases and add Session0/admin
refusal, over-15-second fixture mode refusal, missing twelve inputs and exact
twelve wrong-hash rejection before platform pin/load/Id attempt (the hash
negative fails the first pin, not an isolated twelfth pin). The new owned fixture
case checks eleven wrong-shape/visibility/owner/return/original-contract denials
before construction, five pure token-policy mutations without forged metadata,
one positive Id constructor/getter validation, stable typed repeated get_Id,
foreign/malformed/unknown/generic/ref-out/void/property/download denial, sixteen
allowed getter calls then saturation denial, and explicit failed constructor /
wrong numeric getter outcomes without retries. Positive fixture calls and denied
requests remain distinct. A synthetic Project returning after two fixture calls
is explicitly unqualified; it supplies no evidence about original execution.

Source comparison to the baseline **PASS**: ProjectDenyProxy, OriginalProject,
Owned, original InvokeProject, public Project selection/token checks, enum path,
Framework check, resolvers and all twelve pins remain byte-identical. Platform
preflight/alias logic is unchanged apart from admitting the two new fixed modes.
Config/build, the other constructor sources and metadata tools are unchanged.
Generation07 Windows build/owned suites, original Id constructor/getter, positive
store fixture, DCA initialization/UI/state and hardware are **NOT RUN** at this
source freeze. Task-009 stays Blocked; prior actual diagnostic FAIL is retained.

## Generation07 Windows owned verification

The coordinator subsequently built the unchanged frozen sources on Windows
with pinned compiler 4.8.9221.0, SHA-256
`46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`:
**PASS**, eight source entries/twelve artifacts. Private source-freeze-07.json
SHA-256 `0ed5171bf2522d78356f44fcafbf5da6be64d285f651546528375dbae3649c92`
matches the source hashes above; build07-report.json SHA-256
`3373e982172c60806d8a1f26fa24cb5d8986c0408e47ac8eedde2751f916f15a`
records ProjectConstructorProbe.exe, 68,608 bytes, SHA-256
`d54678cb37b76cb9de79536df9e049742106ad3f40aa06219f5d2e9c51fe753e`.
The earlier NOT_RUN entries describe the historical source freeze, superseded
for owned build/tests by these actual results, not for original execution.

The unchanged Windows test command
`powershell.exe -NoProfile -ExecutionPolicy Bypass -File Test-ConstructorProbe.ps1 -BuildDirectory BUILD07 -OutputDirectory NEW_OUTPUT -ExpectedSession PROFILE`
reports **PASS 15 Session0 / 25 interactive limited / 20 administrator negative**
owned cases. Interactive profiles were Session2 with the expected nonadmin/admin
roles, each exit 0. Both tasks were removed, zero owned processes and no junction
remained; 64 limited and 49 admin case files were collected. Original runtime is
**NOT RUN in every owned profile**; old modes also passed their owned cases.

Private owned07-observation.json, 33,508 bytes, SHA-256
`36bf34ce75f5539dca24e0e23eb26a28e5ae0a46b4a0d5ebbe3fe978ef2927a4`,
binds build/profile/log provenance. The limited project-store-id-proxy.log is
17,143 bytes, SHA-256
`4b34d688961600acdcd76c9b2b97d2f0ca402cb03a8b8ddff68e15be01ffc439`:
eleven preconstruction negatives, five token-policy mutations, one owned Id
constructor/getter validation, stable positive getter calls, sixteen-call
saturation, and unknown/malformed requests denied. Its synthetic Project returned
with fixture-calls=2/denied-requests=0 and qualification=NOT_ESTABLISHED.

Raw UTF-16 driver logs remain alongside strict UTF-8 derivatives/encoding
manifests. Session0 stderr contains retained CLIXML progress; it is not empty.
The limited status03 collector ran before the done record existed and retained
that missing-done failure; status05 retrieved the same single successful run,
without a test rerun. Cleanup/provenance are retained. Independent source/result
verification/review and commit precede any original fixture attempt, still
**NOT RUN**. Task-009 remains Blocked; no fixture acceptance, DCA/UI/state,
hardware, gate qualification or descendant containment is asserted.

## ProjectId guarded metadata result

At source `a2976171e6bd571243c8f1a0f3ab88bbf9674911`, the coordinator's
single actual ProjectId extraction **PASS**, exit 0. PowerShell 7.6.7 reports
`write_denied=true`, `network_denied=true`, `runtime_execution=NOT_RUN`.
The exact single Ids pin/identity and all six frozen source hashes match.
Private `task009-project-id-metadata/project-id-report.json`, 8,834 bytes,
SHA-256 `37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4`,
contains four selected types: public ProjectId `0200002C`, flags `1056769`,
then HexNumberId -> NumberId -> Id -> external System.Object, disposition
`EXTERNAL_BASE_NOT_RESOLVED`. Public root declarations include `.ctor(UInt32)`
`060001EF`, `.ctor(StringSection)` `060001EE`, `get_ProjectNumber()` returning
UInt32 `060001F2`, and static Generate(String)/GenerateUnique. These are
declarations only: no method, constructor, generator or getter executed; no
ProjectId value or IProjectStore response is supplied or chosen.

The first owned setup (`run_fixed.py`/`fixed-run.json`) exited 1 with
`DCAPROBE_PRIVATE_STAGE` before the metadata parser child: stage inside Git,
mode 0755. Its extraction is **NOT RUN**. The separately corrected setup uses
an outside-Git 0700 private stage, exact 0600 input and absolute immediate output.
`fixed-run02.json`, SHA-256
`d479408b109903df3881227d2bcd651c7c3119940647db4e800accc757fd6ff8`, records
exit 0; no source, pin or guard rule changed, and no automatic metadata retry
occurred. Both attempts/raw streams remain immutable. Normalized successful command:

```text
python3 -B experiments/dca-metadata/run_probe.py --stage PRIVATE_STAGE --pwsh PWSH --experiment project-id --output PRIVATE_STAGE/project-id-report.json
```

Deterministic private `project-id-observation.json`, 7,734 bytes, SHA-256
`89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a`, binds
eleven provenance files, source, probes, pin, hierarchy and selected declaration
tokens. Historical `source-freeze.json` retains its pre-run NOT_RUN status;
its 941 bytes/SHA-256 `1a04c44c8bf6667d3c694e464e0c07e0870ddb91c48366fb96c67a0f38055f6c`
are preserved. Source/synthetic verification and critical review PASS; this
actual result's independent audit/review are pending. Raw `rights=UNRESOLVED`
certifies no rights decision; ADR-005's scope is unchanged. Probe denials do not
claim systemwide isolation. The next runtime fixture experiment is **NOT RUN**
and requires separate review. Task-009 remains Blocked; project06 FAIL exit 10
is preserved, with no qualified fixture, DCA/UI/state/hardware or gate outcome.

## Fixed ProjectId metadata contract, historical source freeze after project06

Baseline `28398f0de002c6bef613325dc3431fe48e7cb656` contains the reviewed
project06 result. Its unsupported `IProjectStore.get_Id` request motivates a
separate `project-id` metadata mode; it supplies no ProjectId value or service.
The guarded mode accepts only Knx.Ets.Ids.dll, 318,928 bytes, SHA-256
`43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877`,
identity `Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159`.
The hash is verified before PEReader; metadata identity precedes type selection.
The sole fixed root is `Knx.Ets.Ids.Project.ProjectId`, with at most sixteen
same-assembly base definitions. Type flags, base/interface declarations, all
declared constructors, public/protected methods, and declared field flags and
signatures are recorded. No field values, custom attributes, bodies, dependency
resolution or target execution are permitted. External bases remain unresolved;
a bounded TypeSpec base declaration is retained with
`TYPESPEC_BASE_NOT_FOLLOWED`, without following generic ancestry.

Existing 16 MiB file/32 MiB aggregate, 32,768 rows per table, 128-byte/128-node
signatures, depth sixteen, 32 parameters/interfaces, 256 methods per type/2,048
total, 1,024 selected types and 512 KiB report bounds remain. Fields separately
use 256 per type/2,048 total. The guarded child retains restriction probes,
30-second deadline and 2 MiB combined capture. No ObjectModel row exception is
used. Missing/duplicate root, bad identity/hash/signature and bounds fail closed.
The separate worker has no mode, root or identity override. Owned tests use an
explicit reader requiring unsigned `Task009.OwnedProjectId` 1.0.0.0 metadata;
they never impersonate the original signing identity. Earlier modes, pins and
reports remain unchanged. Original ProjectId extraction and APIs are **NOT RUN /
UNKNOWN** until independent source review and the coordinator's guarded run.
Task-009 remains Blocked; no fixture, DCA initialization or hardware claim follows.

Owned macOS verification at this source freeze: provisioned PowerShell 7.6.7
compiled the reader/fixtures and reported **PASS 151 / FAIL 0**; Python reported
**PASS 19**. Commands from the worktree:

```text
pwsh -NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1
python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py
python3 -B scripts/check_project_scope.py
git diff --check
```

The PowerShell command used the already provisioned private
`toolchains/powershell-7.6.7/pwsh`. Scope and diff checks **PASS**. New synthetic
checks cover one fixed root; missing/duplicate roots; distinct unsigned identity;
original-pin/hash rejection before parsing; private constructors, public/protected
methods and private/public field declarations; omitted constant values and ignored
private nonconstructor signatures; generic-base stop and local bases; field
headers (including reserved flags), empty/truncated/void/trailing signatures,
128-byte/128-node/depth/string bounds; 256 field/method boundaries and overflow
of the 2,048 totals; 32,768 TypeDef/MethodDef boundaries; interface and report
limits. The report-limit fixture selects four types with 200 fields each and
1,000-character field-name prefixes; it fails `DCAENTRY_REPORT_LIMIT` without
raising any cap. These images contain no vendor bytes or method bodies.

Earlier reader reports compare byte-for-byte equal. Removing only the additive
ProjectId blocks reproduces the baseline reader, fixture and prior test source;
the legacy Method decoder, SelectChain and ReadInternal are unchanged. All old
Python pin/command constants, the earlier worker and constructor tools are
unchanged. The wrapper test uses mocked child output and owned one-byte input;
it tests restriction-first fixed worker/pin/identity/root selection, no original
parse. No original API, value construction or service behavior is established.

| Frozen source | SHA-256 |
|---|---|
| EntrypointMetadata.cs | `d2141d41233dc0da91ff8676c0d8bd72b188031a5a6ed9dfd139a248448dd3b8` |
| OwnedEntrypointFixtures.cs | `468f72504cad76e3df34f508ccf3a05854ca5a8b093f89ac9ed6d9472b58ffb9` |
| Test-EntrypointMetadata.ps1 | `bc07178c8c80178d64f3ea11a27a146f9c99e508290238a4b29e252af14f4756` |
| Read-ProjectIdMetadata.ps1 | `6f3c24ae0ec9184a14ff748b53486510da102316dc042315760b000c1b457d2b` |
| run_probe.py | `76638cf02e64d60fe6d17d04f4ec8751ac3feb0dbbd3872be682407fd947f32f` |
| test_run_probe.py | `7b43ad0c981609c468ea9b1aebdaaf3c3605897b6aa60efa2d8af757e120e792` |

Source commit and independent verification/review are pending. After those
steps, only the coordinator may run the fixed guarded `--experiment project-id`
with a fresh private stage and output. Original metadata extraction, new Windows
execution, fixture construction, DCA initialization/UI/state and hardware are
**NOT RUN for this increment**; prior project06 runtime/service observations
remain the reviewed diagnostic FAIL below. Task-009 remains Blocked.

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

## Public Project constructor diagnostic: first source freeze

Source baseline is `9a4e283c98809c4e91bd89dd495f74166c2692df`. This
Task-009 experiment has two separately reviewed stages. It does not supply a
working Project, SDK service, ETS project store or DCA context. The initial
owned executable has `OriginalConstructorReady=false`; `original-project`
fails with `ENUM_LITERAL_NOT_REVIEWED` before input pinning, identity inspection
or any original assembly load. The reviewed enum name, underlying type, type
and literal tokens, raw value and report SHA-256 are deliberately unset. Only
an observed enum report followed by a second source freeze, build, owned tests
and independent review can enable that constructor stage.

The fixed `project-enum-metadata` stage accepts a directory containing exactly
one input: unchanged `Knx.Ets.Common.Types.dll`, 215,504 bytes, SHA-256
`5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0`,
identity `Knx.Ets.Common.Types, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`. It pins the file before identity inspection,
holds its read handle and uses Framework `ReflectionOnlyLoadFrom` for exactly
`Knx.Ets.Common.Types.Enumerations.GroupAddressStyle`. It reports the enum type
token, underlying integral type, declared literal names/tokens and raw constant
values; more than 64 declared fields fails with `PROJECT_ENUM_FIELD_BOUND`.
`GetRawConstantValue` reads metadata. No field `GetValue`, custom attribute,
constructor or static initializer is invoked. A reflection-only dependency
request fails explicitly without resolver fallback. No SDK or DCA assembly is
loaded by this stage. Its bounded UTC log is the evidence artifact; the
coordinator must hash and review that actual log before selecting a literal.
There is no assumed zero, default or sentinel enum value.

The disabled second stage has a fixed nine-input route: the previous eight
Task-008 names, identities and hashes plus only the qualified ObjectModel
14,204,880-byte file, SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`,
identity `Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`. Every hash is checked before any input
identity inspection or load. No Autofac, Kim or further dependency is allowed;
an unallowlisted dependency is an observed blocker, not permission to expand
the route. The legacy eight-input constructor/Initialize commands retain their
input policy and behavior. The new stage explicitly loads ObjectModel and
Common.Types and invokes no DCA or SDK entrypoint.

After a future reviewed enablement, the sole original invocation would be
public ObjectModel Project type `020003A5`, constructor `06002830`, signature
`(IFactory, String, GroupAddressStyle, IProjectStore)`, once. Its arguments are
the fixed name `Task009 offline constructor diagnostic`, the separately
reviewed exact enum literal, and transparent proxies for the original public
IFactory and IProjectStore interfaces. Framework `RealProxy` requires no new
package. Every routed method returns `PROJECT_FIXTURE_UNSUPPORTED`, including
inherited, generic, void, property and ref/out calls. It does not inspect input
arguments or synthesize output values. Each proxy logs at most 16 member
requests, then keeps denying with `REQUEST_LIMIT`; counters saturate at 17.
The first `PROJECT_SERVICE_DENIED` after `PROJECT_CONSTRUCTOR_BEGIN` identifies
the first observed required service. A caught denial or a return without any
service request remains `qualification=NOT_ESTABLISHED`. Type/proxy dependency
failures before the constructor have separate phase markers. No nonpublic
Root/Context activation or downstream lifecycle method is invoked.

Both stages use the existing pinned C#5/net48/x64 compiler and six framework
references. The constructor stage uses an STA dispatcher; both require an
interactive non-admin Windows session. Local path/reparse/config checks,
held input handles, 16 MiB per input, 96 audited assemblies, 64 KiB log/capture
and the launcher deadline remain bounded. The reviewed planned deadline is
15 seconds; the launcher rejects values above 30 seconds and confirms direct
child termination. Its framework/location audit detects unexpected assemblies
after load; it is not prevention. The supervisor does not contain descendants
or network access. Prior connectivity exceptions are not isolation PASS.
Cleanup must retain exit evidence and verify no owned processes remain.

Only the coordinator stages and executes on the clean `.60` Windows host;
the source ETS host is not an execution target. Private fresh paths are in
`private/evidence/task009-constructor-windows/remote-stage.json`. The normalized
commands below denote that exact fresh stage, not arbitrary discovered paths:

```powershell
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "<stage>\source\dca-constructor\Build-ConstructorProbe.ps1" -Compiler "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" -ReferenceDirectory "<stage>\references" -OutputDirectory "<stage>\build"
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "<stage>\source\dca-constructor\Test-ConstructorProbe.ps1" -BuildDirectory "<stage>\build" -OutputDirectory "<stage>\evidence\owned-session0" -ExpectedSession Session0
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "<stage>\source\dca-constructor\Test-ConstructorProbe.ps1" -BuildDirectory "<stage>\build" -OutputDirectory "<stage>\evidence\owned-interactive" -ExpectedSession Interactive
"<stage>\build\bin\ProbeLauncher.exe" --seconds 15 --mode project-enum-metadata --input "<stage>\inputs-enum" --log "<stage>\evidence\project-enum-metadata.log"
```

The execution-policy override applies only to the stated PowerShell process.
The interactive test must run in the established interactive session, not an
SSH Session-0 substitute. Original enum execution follows frozen source and
owned Windows results plus independent verification/review. Original Project
invocation is **NOT RUN** and remains disabled pending the second source freeze.

Owned regressions cover signed enum literals, metadata-only inspection,
missing/non-enum/oversized enum rejection, constructor signature/token rejection,
inherited/generic/ref-out denials and saturated counters, one invocation,
propagated denial, caught denial and request-free unqualified returns. The
Windows test driver also checks the original-stage boundary before all pins,
enum bad-hash/extra-input failures, Session-0 and administrator rejection, and
retains the earlier constructor/Initialize supervision tests. Local macOS
C#5 syntax parsing passes for seven sources; PowerShell parsing passes for the
two scripts; the new owned fixture compiles under local PowerShell's .NET and
its two signed enum values are verified. Local Roslyn also compiles all four
owned assembly groups in memory with C#5, x64 and warnings as errors against
the exact six pinned net48 framework references; those references are Microsoft
framework inputs, not original SDK/DCA inputs. These are local checks, not Windows
net48 proxy or original execution evidence. Windows build/owned tests and
original enum inspection are **NOT RUN** at this freeze. Task-009 remains
**Blocked**, its service acceptance criteria are unmet, and no gate advances.

The five changed/new tool-source hashes at this first freeze are:

| Source | SHA-256 |
|---|---|
| `Build-ConstructorProbe.ps1` | `1d6457000daca646ecd86f4feb9a2235195cbb96d1b9e7262b39cb3f523d951c` |
| `OwnedProjectConstructorFixtures.cs` | `f69f8f55bdf52a93ad898a59fae3cd37d4d51241680d557a2f17cd7ee71f39fa` |
| `ProbeLauncher.cs` | `c841aa69412dabf7ba7256d94441ff7b8667c01587669a3c3565db51c9a6728f` |
| `ProjectConstructorProbe.cs` | `4a75fd663f4e8749f975635b1382b672575877df3f80e4650743dfdb5a6b9c36` |
| `Test-ConstructorProbe.ps1` | `d406b26b6711d63c761b76517156a43e5c2c5ca12578c14901aa3223f173d98d` |

Read-only comparison against the baseline confirms ConstructorProbe,
InitializeContextProbe and both prior fixture sources byte-identical. A
separate source check confirms the first eight names, hashes and identities
in the new mode exactly match the prior selected route and the sole ninth
entry is ObjectModel. Source-boundary checks, repository scope and whitespace
checks PASS. No original inputs were read or executed during these owned
checks. The source commit, Windows artifact hashes and independent review
remain pending; the actual private enum report must be recorded separately.

## Enum preflight result and fixed facade follow-up, 2026-10-22

The coordinator's first clean-Windows enum metadata run at source
`61c768d1785c723c94985ac36e952601ed38f0b6` is **FAIL**, exit 2. The preceding
owned Windows suite passed 10 Session-0 and 17 interactive cases. Its actual
enum context was interactive session 2, administrator role false. The unchanged
215,504-byte Common.Types input passed its exact hash and reflection-only audit;
enum type materialization then requested `netstandard, Version=2.0.0.0,
Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51`. The first source freeze
correctly denied that unallowlisted reflection-only dependency. No enum literal
or underlying type was observed, and no original constructor executed. The
GroupAddressStyle value remains **UNKNOWN**; this outcome does not establish a
working constructor or fixture.

Canonical private evidence is under `private/evidence/task009-constructor-windows`:
`enum01-plan.json`, `enum01-context.json`, `enum01-done.json`,
`enum01-driver.stdout.txt`, `enum01-driver.stderr.txt` and `enum01.log`. The log
is 2,512 bytes, SHA-256
`2552c34920de231b079ebba7048d1a105c9ad63876b7bc1db2e95381b2955446`;
its end record is 286 bytes, SHA-256
`674b0987c1a00740e28b17e300ed8522537e4465816f836de3eee1fcb8e313e8`.
The JSON end record explicitly retains metadata-only status, vendor execution
NOT_RUN, original constructor DISABLED_NOT_RUN and hardware NOT_RUN. This actual
failure supersedes the first freeze's pending enum run, not its constructor
disablement or the retained failed evidence.

The smallest follow-up changes only the enum metadata route to exactly two
fixed input files: Common.Types plus the already selected 102,856-byte
`netstandard.dll`, SHA-256
`bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb`,
identity `netstandard, Version=2.0.0.0, Culture=neutral,
PublicKeyToken=cc7b13ffcd2ddd51`. Both held input handles are hashed before
either identity inspection or reflection-only load. A completed-pin flag is
set only after both exact identities are checked. The reflection-only resolver
allows only that facade identity, only in enum mode after the flag, and only
via `ReflectionOnlyLoadFrom` of its fixed staged path. Its returned identity,
path and ReflectionOnly flag are checked; the assembly audit likewise allows
only the exact facade metadata identity/path. All other dependencies still
fail. This is no generic resolver fallback or runtime SDK dependency expansion.
The original eight-input route and disabled nine-input constructor pinset are
unchanged; `OriginalConstructorReady` remains false.

Owned test changes use two deliberately renamed owned fixture files for the
hash-negative route, one file for missing-facade rejection and three files for
extra-input rejection. Source invariants check two hashes before either
identity, completion only after both identities, the fixed reflection-only
resolver and the disabled original stage. The coordinator will use fresh
`source02`, `build02` and `inputs-enum02` paths; the prior run and first freeze
remain intact. The normalized commands above apply with those fresh paths,
and the enum input directory must now contain exactly the two pinned files.
Owned Windows re-verification and independent review precede any enum02 retry.
The retry, literal selection, original Project invocation and fixture
qualification are **NOT RUN** at this follow-up source freeze. Task-009 remains
**Blocked** and no gate advances.

At the follow-up freeze, local Roslyn C#5/x64/warnings-as-errors compilation
against the six exact net48 framework references PASS, PowerShell parsing PASS,
and the exact checked-in source-invariant block PASS. The nine constructor pins
and seven other build/host/fixture sources compare byte-identical with source
`61c768d1785c723c94985ac36e952601ed38f0b6`. Scope and whitespace checks PASS.
These are owned local checks; Windows re-verification and enum02 remain NOT RUN.
Changed tool hashes are ProjectConstructorProbe.cs
`3f3cfd986f89a7737af0ad1c281d667e626c77e6205004981f9ea5dbbc14a3af`
and Test-ConstructorProbe.ps1
`61f6e8048f62faed4ff83617e15493fe8395e0822f1311f673a31ad3eb4ec4ff`.


## First Windows diagnostic freeze results, 2026-10-22

The coordinator transferred the ten owned tool/host sources recorded in
`private/evidence/task009-constructor-windows/source-freeze-01.json` to the
fresh clean-host stage. All staged source hashes match the frozen worktree.
The pinned Windows C#5/x64/net48 build PASS uses compiler 4.8.9221.0,
SHA-256 `46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`,
and the six existing fixed framework-reference pins. The raw `build01-report.json`
records eight compiled source hashes and twelve output/config/sidecar artifacts;
the two PowerShell tool hashes are also in the full source-freeze record.
No SDK/DCA assembly was executed by the build.

| Built owned executable | SHA-256 |
|---|---|
| ProjectConstructorProbe.exe | `1951c8f91b12ea2b2c356361f9b81e56ecb3ad2685bc6b3bbe2f9ca9e3426dcd` |
| ProbeLauncher.exe | `49873f7bfd79183d7cef7bb118d681fc8265f8f93fec0a40d9d4b7f144040c13` |
| ProjectConstructorProbe.exe.config | `b72d33cb400c6a9de0565ece3698308c569836dc41f9003d70d9d78a31b4315a` |

The coordinator ran the stated owned test driver with fresh output directories:
Session0 PASS 10/10 cases; limited interactive session 2 PASS 17/17 cases.
These are two suites, not 27 distinct assertions. The interactive context
records UserInteractive=true and administrator_role=false. Both test processes
exit 0. The first-freeze original-project dispatch records
ENUM_LITERAL_NOT_REVIEWED with no PIN, assembly or constructor marker.
Inherited/generic/ref-out/void/property denials, saturated counters, enum
metadata negatives, once-only constructor behavior and caught/request-free
unqualified returns pass in the owned proxy case. Prior constructor/Initialize,
timeout/output, hash/path and junction tests remain passing.

Canonical evidence includes `owned-session0-01-stdout.txt`,
`owned-interactive01-context.json`, `owned-interactive01-done.json`, the raw
PowerShell driver streams and selected per-case logs. PowerShell's raw driver
stdout is UTF-16LE; unchanged raw bytes and strictly decoded UTF-8 derivatives
are linked by `owned-interactive01-log-encoding.json`. No test rerun is inferred
from this decoding. Driver stderr is empty. The fresh owned scheduled task was
removed after completion; cleanup records zero active owned tool processes,
LastTaskResult=0 and no remaining test junction. Descendants and network access
are not contained by this supervisor.

Independent verification PASS audited all ten source hashes, source bounds and
stage guards, raw/normalized log hashes, actual session/result evidence and
build-report/runner preflight consistency for all twelve artifacts. The verifier
did not independently rehash target binaries or rerun Windows execution.
Its source/report/artifact audit, scope and whitespace logs are under
`private/evidence/task009-constructor-windows/verifier/`. This is an independent
audit of the coordinator's Windows runs, not another Windows run.

This supersedes the first source-freeze's Windows build/owned-tests NOT RUN.
Original enum inspection and Project constructor invocation remain NOT RUN.
OriginalConstructorReady remains false. No working Project fixture, DCA
initialization, UI/state proof, hardware result or gate acceptance is asserted.
Task-009 remains Blocked. Critical source/result review precedes any original
enum metadata inspection; an observed literal and second reviewed source freeze
remain prerequisites for the original constructor diagnostic.

## Corrected facade-route Windows owned results, 2026-10-22

The coordinator rebuilt the frozen facade correction on the clean Windows host
with the same pinned C#5/x64/net48 compiler 4.8.9221.0, SHA-256
`46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`,
and six framework-reference pins. Build **PASS** is recorded in
`private/evidence/task009-constructor-windows/build02-report.json`, 5,164 bytes,
SHA-256 `d45470a971ce86be056b4cc2dc40b1b3ff67f3b3d72e1e58a9ff3112ccdc9b0f`.
The built ProjectConstructorProbe.exe is 51,712 bytes, SHA-256
`7447540d8ec318ab1ab2b8c36b2ab6bc9fd4b2ef7ab291b083868af0b3f166b5`.
The manifest `source-freeze-02.json`, 1,379 bytes, SHA-256
`73b624d0bb74bb855d9ab736e9b614bcc7e354775e51360160182a2e2de6e9b2`,
records baseline `61c768d1785c723c94985ac36e952601ed38f0b6` plus ten frozen owned
source hashes, metadata input count 2 and original_constructor_ready=false.
A local read-only check confirms all ten current source hashes match it.
This correction's source commit remains pending.

The actual corrected owned Windows suites **PASS**: Session0 11/11 cases and
interactive 19/19 cases. These are two suites, not 30 distinct assertions. The
interactive context is session 2, UserInteractive=true, administrator_role=false;
its end record reports exit 0 and OWNED_TESTS_ONLY. New cases cover the two-hash
source guard and the missing facade, alongside the corrected two-file hash
negative and three-file extra-input negative. Original Project dispatch stays
blocked by ENUM_LITERAL_NOT_REVIEWED. All test artifacts identify original/vendor
execution and hardware as NOT_RUN. No original input is used by these tests.

Canonical files under the same private evidence directory include:

| Evidence | SHA-256 |
|---|---|
| `owned-session0-02-stdout.txt` | `6389329464b0922e2d303f8c332ed8569be44680116c55129e69ade36d5ba5ca` |
| `owned-interactive02-driver.stdout.txt.utf8` | `5574918423fc3619f7c0eeedb7902201367de7fbfef58224db12722a07ce3a1e` |
| `owned-interactive02-context.json` | `e4cef95d2c65a8e38399dff7ace56a3a87fd133390c2bd6b91b1847590677774` |
| `owned-interactive02-done.json` | `f0133e973a21d556006b7483135f83fc4b5b14863c836c5efede64cca86262fb` |
| `owned-interactive02-log-encoding.json` | `f2a7e27b46c5bf357d7b5136ae8d21e8c7ee6eea1238bd1c598602efd446037a` |

The encoding manifest preserves the raw UTF-16LE streams and links their exact
hashes to strictly decoded UTF-8 derivatives; local byte/hash/decoding checks
PASS for stdout and empty stderr. Decoding is not another run. The 11/19 case
counts were checked from the actual suite logs. Detailed case-log collection
and cleanup evidence are being recorded separately by the coordinator;
independent follow-up verification/review remains pending here.

This supersedes only the facade follow-up freeze's Windows build/owned-tests
NOT_RUN. The first generation's 10/17 PASS and original enum01 **FAIL**, exit 2,
remain preserved. Enum02 is **NOT RUN**; GroupAddressStyle is **UNKNOWN**.
Original Project construction remains source-disabled and **NOT RUN**. There
is no Project fixture, DCA initialization, UI/state, hardware or gate success
claim. Task-009 remains **Blocked**. Independent verification/review precedes
any original enum02 retry and no constructor can proceed without the observed
literal and a second reviewed source freeze.

Coordinator follow-up: owned case logs and cleanup are now preserved. The
cleanup record reports LastTaskResult=0, task removed, zero owned processes
and no junction. Independent verification and critical source/result review
PASS; their audit records are under `private/evidence/task009-constructor-windows/`
`verifier02/independent-audit.log` and `review02/critical-review-source-result.txt`.
These are evidence audits, not independent Windows reruns or target-binary
rehashes. The reviewed facade-only correction may proceed to a fixed enum-only
metadata retry; original construction remains disabled.

## Successful enum02 observation, 2026-10-22

The coordinator's fixed two-input reflection-only enum02 run **PASS**, exit 0,
uses source `41b5f6927c8b17d2d7781e4f2955595e09904250`. The actual context is
Windows interactive session 2, administrator_role=false, metadata_only=true.
Common.Types and the previously selected netstandard facade both pass their
exact input hash/size pins before reflection-only loading:

| Input | Bytes | SHA-256 |
|---|---:|---|
| `Knx.Ets.Common.Types.dll` | 215,504 | `5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0` |
| `netstandard.dll` | 102,856 | `bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb` |

The log records both selected assemblies as allowed and ReflectionOnly=true,
then the one exact facade resolution. It observes
`Knx.Ets.Common.Types.Enumerations.GroupAddressStyle`, type token `0200008E`,
underlying type `System.Byte`, four declared fields and these three literals:

| Literal | Field token | Raw byte value |
|---|---|---:|
| `TwoLevel` | `04000175` | 0 |
| `ThreeLevel` | `04000176` | 1 |
| `Free` | `04000177` | 2 |

The selected next diagnostic input is **ThreeLevel=1**, with exactly the
observed enum type, underlying type and field token. This is a proposed input
for a constructor diagnostic, not a qualified Project configuration or working
fixture. Source still records original-constructor-ready=false; no original
constructor marker or runtime-loaded Knx assembly is present. The original
constructor remains DISABLED_NOT_RUN. No activation source edit occurs in this
result increment. Independent review of this frozen observation precedes the
separate source freeze that could enable constructor dispatch.

The deterministic private `enum02-observation.json` is 4,062 bytes, SHA-256
`59323bf7eb53d6c6a130c523e49d82bb3e3d58830f43dcc2f13b2dcabb58bcf7`.
It records the source commit, two input pins/identities, raw log hash, actual
context/end records, supervisor result, bounded literal entries, selected
diagnostic literal, cleanup and eight-file provenance. It has sorted JSON keys,
UTF-8/LF, no generated timestamp, and no changing documentation or self hash.
Its 16 KiB maximum is enforced during generation. The selected literal's review
status is explicitly PENDING_AT_REPORT_FREEZE; constructor/fixture qualification
is NOT_ESTABLISHED. This private report must remain separate from proprietary
DLLs and is not a redistribution of those inputs.

Canonical evidence is under `private/evidence/task009-constructor-windows`:

| Evidence | Bytes | SHA-256 |
|---|---:|---|
| `enum02.log` | 2,912 | `c78cf325972616b3bfe3730fd8c16f8765ff6181e995ac731ebb69498684d8f0` |
| `enum02-plan.json` | 2,836 | `f37a1e968ddf858a920a5cd4dc8bd705dd20726de7c333f77ae5a54a709df161` |
| `enum02-done.json` | 286 | `c384ed3596f31c9b222e8c7b9f3386b0de51151f97707db412b00f2b99d52b06` |
| `enum02-summary.json` | 714 | `4605598945a90dd637f76084f58d02e8011f64b484f787e5f82a0c844ef45f57` |
| `enum02-cleanup-stdout.txt` | 116 | `1c244a6c0025a5197dd26e9e8e3de18ce258909ff37bb7af6a63ab8f392863c2` |

The supervisor records a 15-second deadline, 64 KiB capture bound, child exit 0
and direct-child exit confirmed. Driver stderr is empty. The cleanup record
reports LastTaskResult=0, task removed, zero owned processes and zero scratch
files. These are owned-scope checks, not systemwide network or descendant
containment. Context and both driver-stream hashes are included in the private
observation provenance; all summary bytes/hashes and parsed literal records were
checked locally without reading or executing original assemblies.

This supersedes only enum02's earlier NOT RUN and the unknown literal value.
The earlier enum01 **FAIL**, exit 2, and both generations of owned test results
remain preserved. Original Project invocation, DCA initialization/UI/state,
fixture qualification and hardware remain **NOT RUN** or **NOT_QUALIFIED** as
recorded in the observation. Creation route remains **INCONCLUSIVE** and
Task-009 remains **Blocked**. No gate or service acceptance advances.

Independent enum02 verification PASS (58 assertions) and critical observation
review PASS. Records: `private/evidence/task009-constructor-windows/`
`verifier-enum02/independent-audit.log` and
`review-enum02/critical-review-observation-result.txt`. Review confirms only the
observed ThreeLevel=1 literal and provenance as input to a separate reviewed
source freeze; original constructor dispatch remains disabled. Neither reviewer
nor verifier reran Windows or read original DLLs. Task-009 remains Blocked.

## Second constructor-diagnostic source freeze

Baseline `4ad5b523194103b51ea1ab400566bcc10f7424fc` includes the reviewed
enum02 observation: independent verification 58 PASS and critical review PASS.
This separate source increment sets OriginalConstructorReady=true only for the
existing fixed public Project constructor diagnostic. It pins ThreeLevel,
literal token `04000176`, raw value `1`, enum type `0200008E`, underlying
`System.Byte`, and the unchanged private observation SHA-256
`59323bf7eb53d6c6a130c523e49d82bb3e3d58830f43dcc2f13b2dcabb58bcf7`.
There is no runtime source override or caller-selected enum value.

The nine exact input names/hashes/identities, public constructor `06002830`,
type/signature/enum checks, deny-all RealProxy services, deadlines and loader
policy are unchanged. All nine hashes precede identity inspection or original
loading. No private Root/Factory activation, new dependency, service response,
DCA Initialize, UI/state or hardware operation is added. Readiness means only
that the fixed diagnostic can proceed after its input/environment checks;
constructor return still cannot qualify a Project fixture.

Owned regressions replace the former disabled-stage assertion with original
dispatch Session-0/admin rejection and, in a limited interactive session,
missing-nine-input and exact-nine-wrong-hash rejection before any original
identity/load/constructor phase. The nine negative files contain only renamed
owned fixture bytes. The positive owned proxy-denial cases and fixed enum-only
guards remain. Source guards verify every reviewed literal constant/report hash
and all-nine-hashes-before-identities ordering.

Local Roslyn C#5/x64/warnings-as-errors compilation against the six exact net48
framework references PASS; PowerShell parsing and the checked-in source guards
PASS. Pinset/report comparison and seven unchanged host/build/fixture sources
PASS. These are local checks. The new pinned Windows build/owned regressions,
independent source review and original constructor invocation are **NOT RUN**
at this freeze. The coordinator performs them in that order on the existing
clean Windows host. Task-009 remains **Blocked**, creation_route INCONCLUSIVE,
and no service acceptance or gate advances. Earlier disabled-stage logs remain
historical evidence; the private enum02 observation is unchanged.

| Changed tool | SHA-256 |
|---|---|
| `ProjectConstructorProbe.cs` | `14ccd1d2bf6fc956ef9a19453366825ab85ccc2a1de7a3843e7595732e834537` |
| `Test-ConstructorProbe.ps1` | `478049621398da6a85b03d51128c8308ebaa500421945c57e3104be0d0fe6ee7` |

### Second-freeze Windows owned verification

The pinned Windows build03 PASS with the unchanged compiler and six framework
reference pins. Owned suites PASS: 11/11 Session0 and 20/20 interactive cases
(two suites, not 31 distinct assertions). Session 2 is interactive, non-admin,
and exits 0. Exact-nine wrong-hash and missing-input cases stop before original
identity/load/constructor phases. Cleanup reports task removed, zero owned
processes and no junction. Original execution and hardware remain NOT RUN.

Private `task009-constructor-windows/source-freeze-03.json` SHA-256
`7e6ce5e837d9b84a051461ef80ef94aa7b7de6ef78a824f161afddcd5c0afe84`
records ten source hashes. `build03-report.json` SHA-256
`b0960fafe4c1747314dcb9dd5700fb131ec7c4b75928416003b69c1f8c42b598`
records eight compiled source pins and twelve artifacts. ProjectConstructorProbe.exe
is 52,224 bytes, SHA-256 `db5ac98023c39e4725b4a9543f2fcf36d0393fb337dffe239c805d86b875ab30`.
The same private directory preserves `owned-session0-03-stdout.txt`,
`owned-interactive03-context.json`, its end record, raw driver streams and
strict UTF-16-to-UTF-8 provenance in `owned-interactive03-log-encoding.json`,
case logs and `owned-interactive03-cleanup-stdout.txt`. This supersedes only
the second freeze's Windows build/owned-tests NOT RUN. Independent source/result
verification and review precede any single original constructor diagnostic;
Task-009 remains Blocked, creation route INCONCLUSIVE and no fixture qualifies.

An additional owned-only elevated interactive suite PASS, 16/16 cases,
including original-project-admin rejection with exit 5 before original input
inspection. Its context confirms session 2, interactive=true, administrator
role=true. This closes the admin-branch runtime coverage gap; the Session0
suite had exited on session identity first. Private `owned-admin03-*` records,
raw/decoded provenance and case logs preserve this separate run. Cleanup
reports exit 0, task removed, zero owned processes and no junction. Original
SDK/manufacturer execution and hardware remain NOT RUN in all owned suites.
Session0 stderr retains PowerShell CLIXML preparation progress; it is not
classified as empty or a failing test result.

Independent source/result verification and critical review PASS for the second
freeze; records are `private/evidence/task009-constructor-windows/`
`verifier03/independent-audit.log` and `review03/critical-review-source-result.txt`.
The fixed single 15-second public constructor diagnostic may now follow its
source commit. A constructor-begin marker proves a reflection invocation attempt,
not body entry; return does not qualify a Project fixture. Any dependency or
constructor failure stops this attempt without pin widening or automatic retry.
Original invocation remains NOT RUN at this reviewed source commit.

## Project03 runtime diagnostic result

At source `b8fcc21ef126a4d00f8f4c6c65101f23cd4b1358`, the coordinator's single
Windows project03 diagnostic **FAIL**, exit 2, in interactive session 2,
administrator_role=false. All nine exact input pins PASS. Original ObjectModel
and Common.Types assemblies load into runtime context; this is not an all-runtime
NOT RUN result. During `original-type-materialization`, the resolver requests
`FluentResults, Version=3.16.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159`
and correctly denies that identity outside the fixed allowlist. The final error
is `PROJECT_DEPENDENCY_BLOCKER` after `PROJECT_DEPENDENCY_NOT_ALLOWLISTED`.

No PROJECT_CONSTRUCTOR_BEGIN, proxy-materialization or service-denial request is
recorded: constructor invocation is **NOT_REACHED**, body entry
**NOT_ESTABLISHED**. Project fixture qualification remains **NOT_QUALIFIED** and
creation_route **INCONCLUSIVE**. DCA initialization/UI/state and hardware are
**NOT RUN**. No binary, pinset, service response or source is changed by this
result; there is no automatic retry or dependency widening.

Private `task009-constructor-windows/project03.log` is 4,335 bytes, SHA-256
`59dc4073855b9c38149a61ff86dd0cb46f9bf92fa14b97f1b7c41b4c51caf31b`.
The deterministic private `project03-observation.json` is 5,030 bytes, SHA-256
`f55f82bdea5ee8edf7a30ce5d9cda8c96930a0f6d4833d2183d431be93334486`;
it binds source, nine pins, runtime loads, blocker, context/end and eight-file
provenance, with a 16 KiB bound and no generated timestamp or self/docs hash.
The actual plan/context/done/summary and driver streams remain alongside it.
The supervisor confirms child exit 2 within its 15-second bound; stderr is empty.
Cleanup reports expected_exit=2, LastTaskResult=2, task removed, zero owned
processes and scratch files. Network/descendant containment is not established.
Earlier enum02 PASS and enum01 FAIL remain preserved. Task-009 stays **Blocked**;
any future exact dependency qualification requires a separate reviewed increment.

Independent project03 evidence audit PASS (17 assertions) and critical result
review PASS; the experiment itself remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project03/independent-audit.log`
and `review-project03/critical-review-result.txt` under the same directory.
They verify retained evidence without rerunning Windows or reading DLLs; neither
review changes the pinset or establishes constructor/fixture/DCA qualification.

## Controlled FluentResults source freeze

Baseline `8d5482406b7d7c20720b9c44dcebad1069c6113f` retains the stopped project03
FAIL and its independent result audit/review. The exact observed dependency was
privately acquired unchanged: FluentResults.dll, 139,776 bytes, SHA-256
`99adb97bc209d19696c9a42db8eceef49b55fc2c3f4cc6d6f5654fe7e5df9159`,
identity `FluentResults, Version=3.16.0.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`. The private acquisition inventory is
`task009-fluentresults/private-candidate/reference-inventory.json`.

The guarded static reference observation PASS (exit 0, write/network denial
probes true), `static-reference-observation.json` SHA-256
`ad82c8475108bf32c9a663ee68c158e625a4d678ae01727e0c7830f4593c320d`,
records netstandard 2.0.0.0, Tasks.Extensions 4.2.0.1 and Logging.Abstractions
2.1.1.0 references. This is metadata only, target execution NOT_RUN. The existing
Tasks.Extensions 4.2.0.1 to 4.2.1.0 redirect is unchanged. The separate Logging
candidate is not qualified, added, redirected or loaded by this increment.

Only the fixed original-project profile adds that exact FluentResults pin as
input ten. All ten hashes precede identities/load; every other dependency still
fails. The original eight-input route, prior nine pins, enum report/literal,
public constructor tokens/signature, deny-all proxies, config, non-admin checks
and bounds are unchanged. Fresh source04/build04/inputs-project04 are a separate
controlled attempt after review, not an automatic retry of project03.

Owned tests use nine-file missing-ten and exact-ten-wrong-hash sets of only owned
bytes; source guards assert the exact added pin and ten-hash ordering. Local
pinned-reference C#5/x64/warnings-as-errors compile, PowerShell parse, source
guards, prior-nine-pin/seven-other-source comparison PASS. New Windows build,
owned suites and original ten-input execution are **NOT RUN** at freeze; they
require coordinator execution and separate source verification/review/commit.
Task-009 remains **Blocked**, creation route INCONCLUSIVE; no fixture qualifies.

Changed tool SHA-256: ProjectConstructorProbe.cs
`0deaa1a02d588f3926396004b4c118cd7e3e1a88f0b4a81f3b35e1bcecef407b`;
Test-ConstructorProbe.ps1
`81f4f8dd49dd155b9bb21a5fdfa2c77ac48e80d1e780061a5e015f576648e799`.

### Ten-input profile Windows owned verification

Pinned Windows build04 PASS. Owned suites PASS: 11/11 Session0, 20/20
limited interactive and 16/16 elevated interactive (separate suites). Both
interactive contexts are session 2; limited/admin roles are confirmed separately.
Missing-ten, exact-ten wrong-hash and original-project-admin cases stop before
original input identity/load/invocation. Both scheduled tasks were removed,
with zero owned processes and no junctions. Original execution remains NOT RUN
in these owned suites; the ten-input runtime attempt is still NOT RUN.

Private `task009-constructor-windows/source-freeze-04.json` SHA-256
`105f5b6a5e4939b0b132f43271efd011d5ffa081585709185f8ad6f10bd8e315`
and `build04-report.json` SHA-256
`405345fb701d52f9f353a4792c4a36f5b130843fbf4046104fc554adc62436a4`
bind the ten owned sources, unchanged compiler/framework pins and twelve built
artifacts. ProjectConstructorProbe.exe is 52,736 bytes, SHA-256
`965239f941a08dce5101aa8e11910e61164355c7b5fdb9b7f886687d60c55b18`. The same private directory retains
`owned-session0-04-*`, `owned-interactive04-*` and `owned-admin04-*`: raw
streams, strict UTF-16/UTF-8 provenance, contexts/end records, case logs and
cleanup. This supersedes only the ten-input freeze's Windows build/owned-tests
NOT RUN. Independent source/result verification and review must precede its
source commit and single original attempt. Project03 FAIL is preserved;
Task-009 remains Blocked, with no Project fixture or DCA qualification.

Independent source04 verification and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier04/independent-audit.log`
and `review04/critical-review-source-result.txt` under the same directory.
Only the exact observed FluentResults dependency is added; Logging remains
excluded. A single fixed 15-second ten-input attempt follows the source commit;
any next dependency or constructor failure stops without automatic widening or
retry. The new original attempt remains NOT RUN at this reviewed source commit.

## Project04 runtime diagnostic result

The single ten-input project04 attempt at source
`d4af5da8af23ce6039042b935ab310b2d6cf445f` **FAIL**, exit 2, during
original-type-materialization. Context is interactive Windows session 2,
administrator_role=false; all ten exact input pins PASS. ObjectModel,
Common.Types and FluentResults runtime loads are observed. Two blockers remain:

- Runtime `netstandard, Version=2.0.0.0, Culture=neutral,
  PublicKeyToken=cc7b13ffcd2ddd51` is logged allowed=False, followed by
  UNEXPECTED_ASSEMBLY_BLOCKER, detection=after-load, prevention=NOT_ESTABLISHED.
  Its loaded path/hash is not logged; this attempt establishes neither GAC nor
  framework origin. The passing staged netstandard pin does not identify the
  loaded file.
- The resolver then denies `Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral,
  PublicKeyToken=61439873ec5e1159`. The final error is PROJECT_DEPENDENCY_BLOCKER;
  the separate audit blocker remains a failure, not an accepted assembly.

No constructor-begin, proxy-materialization or service request is recorded:
invocation **NOT_REACHED**, body entry **NOT_ESTABLISHED**. SDK runtime loads
are observed; they are not all NOT RUN. Creation remains **INCONCLUSIVE**,
fixture **NOT_QUALIFIED**; DCA initialization/UI/state/hardware **NOT RUN**.

Private `task009-constructor-windows/project04.log`, 4,929 bytes, has SHA-256
`85cd1b9f3f92e7902d06961b6bc24eb6ea990916a69aea615e67191e19d285bb`.
The deterministic bounded `project04-observation.json`, 5,805 bytes, SHA-256
`81f8db7297802cbf222c4c9db85d3e827e3d4b4ad2d70e29051325bc64f5d1b4`,
binds ten pins, both blockers, unknown facade origin, context/end/supervisor,
cleanup and eight-file provenance. It has no generated timestamp/self/docs hash.
Child exit 2 is confirmed under the 15-second bound; stderr is empty. Cleanup
records expected/last exit 2, task removed, zero owned processes and scratch
files. No network/descendant containment is established. Project03 FAIL and
owned 11/20/16 PASS remain preserved. This result changes no source, pinset or
configuration and performs no retry. No separate observation or acquisition is
used here to infer the loaded facade's origin or widen runtime.
Task-009 remains **Blocked**; no service acceptance or gate advances.

Independent project04 evidence audit PASS (20 assertions) and critical result
review PASS; the recorded experiment remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project04/independent-audit.log`
and `review-project04/critical-review-result.txt` under the same directory.
Neither audit reran Windows or changes the unknown loaded origin, pinset,
constructor-not-reached outcome or closed fixture/DCA qualification.

## Profile11 and fixed OS facade source freeze

Baseline `ad8f2e84b5dc1e5932bc9e89384bebdee9ac7c6c` retains reviewed project04
FAIL. Private Knx.Ets.Common.dll acquisition and guarded static metadata qualify only
the observed missing identity for input eleven: `Knx.Ets.Common, Version=6.4.8718.0,
Culture=neutral, PublicKeyToken=61439873ec5e1159`, 503,248 bytes, SHA-256
`4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd`.
Private `task009-common/static-reference-observation.json`, SHA-256
`67195f1f476ec44eefbf0ba6f43f436b014e9e83729fc91569ec8286a53d2731`,
records exit 0, write/network denial probes true and target execution NOT_RUN.
No declared transitive dependency, Logging pin or redirect is added.

The separate Microsoft-only Windows observation
`task009-framework-facade/framework-observation.json`, SHA-256
`f0e0c54095432f3704848fff059251eeddd174a2d110478a2cc008d308f7af34`,
records normal runtime netstandard 2.0.0.0/token cc7b13ffcd2ddd51 in the GAC,
102,856 bytes, same SHA-256 as staged netstandard:
`bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb`.
Its fixed Windows-relative path is
`Microsoft.Net\assembly\GAC_MSIL\netstandard\v4.0_2.0.0.0__cc7b13ffcd2ddd51\netstandard.dll`.
This independent observation does not establish project04's unknown origin.

Only original-project/owned-project runtime classification gets this exact alias.
All eleven private hashes precede OS preflight and original identities/load.
Preflight validates the fixed local path, exact length/hash and AssemblyName
identity, retaining FileAccess.Read/FileShare.Read until process exit; only then
does its completed flag become true. Audit also requires exact loaded identity,
path, GlobalAssemblyCache=true and ReflectionOnly=false. No caller-supplied proof
or new general framework allowance is added. Owned mode needs only its fixture and the
Microsoft OS file; the two-input reflection-only mode and old eight-input route,
first ten pins, config, resolvers, enum/constructor checks and bounds are unchanged.

PLATFORM_FACADE_PIN is distinct from private PIN records. Facade-name audit logs
actual identity/location/cache/reflection and alias decision, including denied
cases; a held-file hash is attributed only to a matching alias. Unknown locations
remain after-load audit failures; prevention is not claimed. Owned tests load
only the Microsoft facade and check matching facts with incomplete pin, plus
wrong identity/path/cache/reflection. Ten-file missing-eleven and eleven-file
wrong-hash sets use owned bytes and fail before OS pinning. Existing proxy,
enum, session and admin regressions remain. No original activation route expands.

Local C#5/x64/warnings-as-errors compile against six exact net48 reference pins,
PowerShell parse and source guards PASS. First-ten-pin, seven-other-source,
unchanged enum/framework/resolver and negative-order checks PASS. The private
reproduction helper `knx-task009-profile11-check.ps1` SHA-256 is
`cdbdac2a76151d924ab81a93c2e0e993a76cc78d4d837c82db8746b3555e2728`;
run it with the provisioned pwsh, `-Worktree <task-worktree>` and
`-FrameworkReferences <private-framework-references>`. This is local compilation,
not Windows execution. Generation05 Windows results are recorded separately;
original eleven-input execution remains NOT RUN. Task-009 stays **Blocked**,
creation route INCONCLUSIVE, with no fixture/DCA/hardware qualification.

| Changed tool | SHA-256 |
|---|---|
| `ProjectConstructorProbe.cs` | `60606baced3ff31b8b898f244e140149427ced85a1f27e0b92ceec5815dec921` |
| `Test-ConstructorProbe.ps1` | `87da923296f48ee5442e80d25da063c687d57df4ddee453041760dd0f4f4da41` |

### Generation05 owned Windows result

The coordinator's pinned Windows C#5/x64/net48 build PASS. Private
`task009-constructor-windows/source-freeze-05.json` SHA-256
`a836a4bfd89b23112a11d9dada6de3939b7920822c6f8dd876bd3541675bb7cf`
binds ten owned sources; `build05-report.json` SHA-256
`4e68ebbeb09c72b3cb5507d70afc9574eb1cd17e86909ad59cfa94357586728a`
retains compiler 4.8.9221.0 and its existing exact compiler/framework pins.
ProjectConstructorProbe.exe is 55,808 bytes, SHA-256
`978c138bc9adfec4409758f9563203892024463815b5ec481d6137b593cb07bc`.
Actual owned suites PASS: Session0 11/11, limited interactive 20/20 and elevated
interactive 16/16. Both interactive runs use session 2 with the recorded expected
admin role and exit 0; vendor execution/hardware are NOT_RUN. These are three
suites, not 47 distinct assertions. The limited `project-proxy.log` is 8,432 bytes,
SHA-256 `77bf886e01aa91edaf5f7b4ed50ba7cbb2692cc39021685c415d64f94e778423`;
it records matching-facts/incomplete-pin denial, the held OS pin, exact actual
GAC alias and wrong-identity/path/cache/reflection rejection. No SDK input is
needed for this owned proof. Original eleven-input execution is still NOT RUN.

Private `owned-session0-05-*`, `owned-interactive05-*` and `owned-admin05-*`
preserve contexts, raw streams and strict UTF-16/UTF-8 provenance. Both interactive
cleanup records report LastTaskResult=0, task removed, zero owned processes and
no junction; 53 limited and 41 elevated case files were retained. The initial
fresh-source directory precheck failure and corrected source05-prepare02 PASS
precede the build. An early result collection preceded the done file; diagnostic02
then observed completion and canonical status03 was retained. These preserved
harness observations are not failed tests or additional test executions.
Independent source/result verification and review remain pending before source
commit or any original generation05 attempt; no fixture/service/gate qualifies.

Independent source05/profile11 verification and critical review PASS. Records
are `private/evidence/task009-constructor-windows/verifier05/independent-audit.log`
and `review05/critical-review-source-result.txt`. The verifier retained its
original report and appended a test-source hash transcription correction;
corrected audit SHA-256 is
`05bcc1bcc483f8291d2eae6e46eb00de6c5dd61fcc9095c800c2623b78b58002`.
Review also corrected the component wording to distinguish the unchanged
framework allowlist from the new exact facade alias. Neither correction changes
source bytes or test runs. These audits do not rerun Windows or rehash target
binaries. After the explicit source commit, one fixed 15-second original
diagnostic may follow; any failure stops without automatic pin expansion or
retry. Original generation05 remains NOT RUN at this source commit. Task-009
remains Blocked, without Project fixture, DCA, hardware or gate qualification.

## Project05 reflection invocation result

The single original attempt at `1b0eb7dcb9158778ae4ce3e40c554f2956f800c4`
**FAIL**, exit 2, in non-admin interactive session 2. All eleven input pins and
the held fixed OS facade pin PASS. This run logs netstandard alias-allowed=True,
the exact GAC path/identity, cache=True, ReflectionOnly=False and held-file hash
`bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb`.
Common is allowed in runtime context. There is no unexpected-assembly audit
marker. These facts identify the alias only in project05; project04's origin
remains historically unknown.

After deny-proxy-materialization, PROJECT_CONSTRUCTOR_BEGIN names public token
06002830. Reflection invocation is **ATTEMPTED_NOT_RETURNED**, not proof of body
entry: JIT/type initialization may fail before body execution. The resolver denies
`Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159`.
Constructor END records returned=False, denied-requests=0. The recorded chain is
TargetInvocationException, FileNotFoundException, explicit dependency denial;
the final error is PROJECT_DEPENDENCY_BLOCKER. Body entry is **NOT_ESTABLISHED**,
creation **INCONCLUSIVE**, fixture **NOT_QUALIFIED**. No proxy service request is
logged; no DCA initialization/UI/state/hardware qualification follows.

Private `task009-constructor-windows/project05.log`, 6,441 bytes, SHA-256
`17fe086e2a727014634ecec4a06dd86b89b5ac4bc655d7e96c09b01780d7427e`,
is bound by deterministic `project05-observation.json`, 6,783 bytes, SHA-256
`dd1dfc6bdc08ba6a61279273045f7f9deb8943183496be43ebb6f85ee0e27955`.
The bounded report includes eleven pins, actual facade path/held hash, context/end,
supervision/cleanup and eight-file provenance; no generated timestamp/self/docs
hash. Child exit 2 is confirmed under the 15-second/64 KiB bounds; stderr is empty.
Cleanup has expected/last exit 2, task removed, zero owned processes/scratch files.
Network/descendants are not contained. No source/pin change or retry occurs;
independent result verification/review remains pending. Task-009 stays **Blocked**.

Independent project05 result audit and critical review PASS; the experiment
remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project05/independent-audit.log`
and `review-project05/critical-review-result.txt`. They reproduce source/report/
provenance consistency and scope/whitespace checks without rerunning Windows
or reading original DLLs. No source change, runtime widening, retry, fixture
qualification or phase approval follows from this result review.

## Profile12 Ids source-freeze contract

Baseline `167740ea8e724b6f69bacfcf75c10a9fa72bab06` retains reviewed project05
FAIL. The next separate prerequisite profile appends only its exact observed Ids
dependency as private input twelve. All twelve hashes must pass before the
unchanged OS facade preflight or original identities/load. First eleven pins,
config, resolvers, facade policy/diagnostics, enum and public-constructor/proxy
checks remain unchanged. Owned missing-twelve and exact-twelve wrong-hash tests
must stop before OS pinning; every other case remains. No new activation/service
or redirect is introduced. New Windows build/tests and the original twelve-input
attempt are NOT RUN at this initial source freeze; Task-009 remains Blocked.

Private `task009-ids/reference-inventory.json` records the unchanged 318,928-byte
Knx.Ets.Ids.dll, SHA-256
`43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877`,
identity `Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`. Guarded `static-reference-observation.json`,
SHA-256 `2f9fd4d2091630f454b0432ded48ed24c14e844a2a68635d8652881ddef4c939`,
records exit 0, write/network denial probes true, target execution NOT_RUN and
only mscorlib/System.Core/System 4.0.0.0 direct references. No extra pin follows.

Local C#5/x64/warnings-as-errors compilation against six exact net48 reference
pins, PowerShell parsing and checked-in guards PASS. First-eleven-pin,
seven-other-source, whole non-pin-code/method and owned negative-order comparisons
PASS. The fresh private helper `knx-task009-profile12-check.ps1` SHA-256
`f01c328f2f31c5f25356b841123bf5a4e91faa459e88270698eb865fc93e14fe`
runs with provisioned pwsh and the same Worktree/FrameworkReferences arguments
as profile11. Changed ProjectConstructorProbe.cs SHA-256:
`8580c897fcc5346c770ffe0294612ae99687c0f51a96928c8dcd9090662b3b35`;
Test-ConstructorProbe.ps1:
`26ee361344c3772d4567ee84a1b575ea500249ff3fa7e7ca7e839ac47da3837b`.
Generation06 Windows evidence, source review/commit and any original attempt
remain separate coordinator steps; no fixture/DCA/hardware or gate qualifies.

### Generation06 owned Windows result

Coordinator Windows build PASS with the unchanged pinned C#5/x64/net48 toolchain.
Private `source-freeze-06.json` SHA-256
`c821734872ffda0a3dac61427a282167d52853c07b184524df2fe2adc3f9c85e`
matches all ten owned sources; `build06-report.json` SHA-256
`2e5c1b43d687389b2fff6ba8979081257f219903dad085b68221056c0359d52e`
records ProjectConstructorProbe.exe, 55,808 bytes, SHA-256
`276b1d71d1188d2dfa02842e8c80c8d8922fc483515ae72e14908640ee2a6854`.
Owned suites PASS 11/11 Session0, 20/20 limited and 16/16 elevated interactive,
session 2 with expected roles and exit 0. Existing facade proof/negatives remain
passing: limited project-proxy.log, 8,432 bytes, SHA-256
`c084d9c56cd6f5328aed26915a141fad8c273e6cac7bab6f781732bc1f51b203`.
Canonical `owned-*-06-*`/interactive06/admin06 evidence preserves raw/decoded
stream provenance, contexts/end and cleanup. Both interactive cleanup inventories
record task removed, LastTaskResult=0, zero owned processes/no junction, and
53 limited/41 elevated case files. Generation06 has no failed setup/early
collection or test rerun. These owned results supersede only the initial freeze's
Windows NOT RUN; vendor/hardware and the original twelve-input attempt remain
NOT RUN. Independent source/result verification/review and source commit precede
that separate attempt. Task-009 remains Blocked; no fixture or gate qualifies.

Independent source06/profile12 audit and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier06/independent-audit.log`
and `review06/critical-review-source-result.txt`. The twelve-name wrong-hash
owned set fails at its first private pin; it is not an isolated twelfth-input
hash test. Source/report and Windows evidence audits do not rerun Windows or
rehash target binaries. After the explicit source commit, one fixed 15-second
original diagnostic may follow; failures stop without automatic expansion or
retry. Original twelve-input execution remains NOT RUN at this source commit;
Task-009 stays Blocked and no fixture, DCA, hardware or gate qualifies.

## Project06 observed service-request result

The single project06 attempt at `de5d65056510b240deb93971313bca9f164b7b40`
**FAIL**, exit 10, with a controlled unsupported-service response. All twelve
private pins/fixed OS facade PASS; Ids loads in allowed runtime context. There
is no dependency-resolution blocker or unexpected-assembly audit marker.
After public constructor begin token 06002830, the first observed SDK request is
`Knx.Ets.ObjectModel.IProjectStore.get_Id`, method 060002F5, zero arguments,
number 1/first-for-interface=True. It receives the explicit
`PROJECT_FIXTURE_UNSUPPORTED` denial; no value is supplied.

END records returned=False and denied-requests=1, followed by
TargetInvocationException → NotSupportedException and PROJECT_RESULT exit=10.
Reflection invocation remains **ATTEMPTED_NOT_RETURNED**; SDK service request is
**OBSERVED**, but body entry is **NOT_ESTABLISHED_BY_STACK_TRACE**. The log does
not identify constructor-body versus type-initializer call origin. This is
distinct from the prior dependency failures, not Project/fixture qualification.
The getter's return/value contract is a future inquiry; this result guesses none.

Private `task009-constructor-windows/project06.log`, 6,513 bytes, SHA-256
`6214345621218b7e69771a0ec5ec1a1b6e3e1e6be7a0f77e484c620ebe44db91`,
is bound by deterministic `project06-observation.json`, 7,782 bytes, SHA-256
`0b6dbcc79c3f280b75137f0824982ab692fbd8f73fa6e03efc7a452e72cf474f`.
The bounded report contains source/pins/alias, exact request, exception/end and
eight-file provenance, with no generated timestamp/self/docs hash. Context is
non-admin interactive session 2; child exit 10 is confirmed under 15-second/
64 KiB bounds, stderr empty. Cleanup has expected/last exit 10, task removed,
zero owned processes/scratch. Earlier project05 FAIL/project04 unknown origin
remain preserved. No source/pin change, retry or fixture response is added.
Task-009 remains **Blocked**, creation INCONCLUSIVE, fixture NOT_QUALIFIED;
DCA initialization/UI/state/hardware NOT RUN, independent result review pending.

Independent project06 result audit and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier-project06/independent-audit.log`
and `review-project06/critical-review-result.txt`. These consistency audits
retain the diagnostic FAIL exit 10 and observed unsupported service request;
they do not establish constructor body/caller provenance or supply a getter
value. No Windows rerun, original DLL read, source change, retry, fixture
qualification or phase approval follows. ProjectId contract investigation
remains a separate prerequisite before any positive fixture implementation.

Independent fixed ProjectId metadata-source verification and critical review
PASS. Records are `private/evidence/task009-project-id-metadata/verifier/` and
`review/critical-review-source-result.txt`. Both independently reran the owned
PowerShell 151-case and Python 19-test suites, scope and whitespace checks;
legacy modes and reports remain preserved. The reviewer additionally checked
malformed method handles and the report's escaped JSON size bound. These
checks do not read original DLL bytes or establish the ProjectId API. After
the explicit source commit, one fixed guarded metadata-only extraction may
follow. Runtime constructors, getter values, fixtures and DCA/hardware/gate
qualification remain outside this source increment; Task-009 stays Blocked.

Independent actual ProjectId metadata-result audit and critical review PASS.
Records are `private/evidence/task009-project-id-metadata/verifier/result-audit.txt`
and `review/critical-review-result.txt`. They verify all eleven provenance
artifacts, six source hashes, exact declaration tokens, denial probes and
staging chronology without reading original DLL bytes or rerunning extraction.
Only PRIVATE_STAGE is the observed first rejection; later setup corrections
satisfy unchanged guards. Public constructor/getter declarations do not qualify
behavior or approve a future runtime fixture. Task-009 stays Blocked; the
project06 FAIL and absence of DCA/hardware/gate qualification remain preserved.

Independent critical review PASS for generation07 at baseline
`91121b184043e32db930fd596e751a53ee69751a`. The reviewer inspected the actual
six-path diff and complete 15/25/20 Windows evidence appendix, checked the frozen
source/report and 182-file provenance, old-mode preservation, scope and whitespace,
and did not rerun Windows or read SDK binaries. The exact synthetic getter boundary
and unqualified-return limits are preserved. Original store-ID fixture execution
remains NOT RUN; no original positive service, Project fixture, DCA/UI/state,
hardware or gate is accepted. After the explicit source commit, only the fixed
single 15-second diagnostic may follow under existing authorization; failures stop
without automatic expansion or retry. The private review record binds the
pre-append document hashes; it does not claim those hashes identify this later
review note.
