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
service is added in this increment. Task-009 remains **Blocked** on a verified
concrete construction contract and meaningful offline fixture semantics.
A separate reviewed follow-up may inspect the fixed SDK roots
`ProjectCollection`, `ISdkContext` and `Root`, plus the exact
`Knx.Ets.ObjectModel.Project.Project` constructor parameter in a separately
pinned four-input static mode. Inspect declared constructors, public/protected
methods, interfaces and bounded same-assembly bases only, without method
bodies, attributes, dependency loading or vendor execution. A collection or
factory signature is evidence for a next inquiry, not permission to invoke it;
absence on these roots cannot prove global construction impossibility.
