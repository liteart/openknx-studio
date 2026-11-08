# Bounded product static IR contract

Tasks: [Task-145](../tasks/Task-145.md), [Task-154](../tasks/Task-154.md). The standalone crate is offline inspection
only. The existing `inspect_archive` entry point always selects the original
BE-04000.02 profile. `inspect_archive_with_profile` requires an explicit profile;
`GlassV31` is a separately bounded structural observation of the registered
MDT V3.1 non-temperature family application. Archive identity does not qualify an
actual BE-GT20S.02S device, firmware or security mode.
`GlassLiteV13` is a separate project/11 candidate profile described below;
references to Glass in the existing resource rules mean `GlassV31` only.

IR version 2 adds anonymous union structure and explicit memory ownership.
A union's source identity is the selected immutable XML SHA-256 plus byte range;
it is an inspection identity, not a persistent business identity or an evaluated
union choice. Its direct memory/parameter members retain source order and ranges,
attributes and module scope. Parameters link back to an owning union source;
memory distinguishes a parameter ID from an anonymous union source. Existing
`owner_id` remains the optional literal parent XML ID.

Known Glass picture/baggage/rename declarations retain opaque names, attributes,
source ranges and scope. IR version 3 adds validated external baggage resources
and references for the Glass profile, as specified below. Rename
semantics remain unverified. Load controls retain opaque attributes, source ranges
and order. No numeric field is interpreted as KNX wire data. Absent segment Data
is `None`; inspection never supplies zeros or infers transfer behavior.

All profiles reject unknown application elements, attributes, placements,
reference kinds and unapproved scope crossings, malformed archives/XML and
exceeded bounds. The sole Glass scope observation is `choose/@ParamRefId`
inside a ModuleDef targeting an existing global ParameterRef: its exact kind
is checked, but scope semantics remain unverified. Each edge is retained in
`unverified_scope_references` with immutable source identity, node/attribute,
target ID/kind and source/target module scope, plus an
`UnverifiedGlobalCondition` compilation blocker. The registered input has 7,310
such edges (ModuleDef 1: 3,706; ModuleDef 2: 3,604). Missing/wrong-kind targets,
cross-module edges, all other scope crossings and default-profile crossings
remain fatal. This does not evaluate a global condition or validate its meaning.
Glass's
additional observed allowances never apply to the default profile. Every
successful snapshot has nonempty compilation blockers. Parameter semantics,
conditions, modules, unions, assignments, external resources, rename behavior and
load procedures remain outside this contract; no snapshot is an executable plan.

The original limits remain 2 MiB compressed/member, 8 MiB expanded, 16 entries,
20,000 elements and depth 32. Glass uses separately fixed 2 MiB compressed,
12 MiB/member, 32 MiB expanded, 64 entries, 100,000 elements and depth 32.
The compression-ratio cap stays 64. Glass alone permits internal ASCII spaces
in flat `M-0083/Baggages/*.png` leaves; leading/trailing spaces, nested paths,
traversal, case collisions, symlinks and CRC failures remain rejected.

## Glass Lite project/11 candidate

`GlassLiteV13` selects namespace `http://knx.org/xml/project/11`, the fixed
`BE-GTL4Tx.01` catalog/product/hardware chain and application
`M-0083_A-0090-13-D12F` (number 144, version 19, mask `MV-0705`,
ProductProcedure). The public `inspect_archive_with_profile` entrypoint also
requires the exact registered archive SHA-256
`c8feaa000ca891df90b455a4ae27de9e1c0e3d3e5cf53e63cf944807f755c7e9`.
Matching labels or XML structure cannot accept a different archive. Size and
cancellation checks run before hashing. The unpinned structural helper is
private; only crate-internal synthetic tests exercise the Lite grammar through
it. Existing project/20 profile acceptance remains unchanged.

Its fixed bounds are 2 MiB archive, 4 MiB/member, 16 MiB expanded, 16 entries,
50,000 XML nodes and depth 32, with the same ratio/path/CRC/XML safety rules.
The observed application requires TypeText Pattern, ParameterBlockRename,
ParameterSeparator Access and additional conditional placements. These
allowances apply only to the explicit Lite profile. Rename targets are
kind/scope checked. Conditional, assignment and module declarations retain
opaque names, raw attributes, source ranges and scope; source bytes remain
immutable. No condition, module allocation, pattern or load procedure is
executed or evaluated.

The selected private inventory has six segments: four with data and two with
Data absent (RAM address `0x0700`, size 316; address `0x083C`, size 1). These
payloads remain `None`, including when referenced by a raw load declaration.
No fill bytes, omission semantics or transfer behavior are inferred. The
current encoder rejects Lite snapshots even if caller-visible blocker labels
are cleared. Archive acceptance establishes structural inspection only; the
physical application, fingerprint `9BDB`, compilation semantics and download
eligibility remain unqualified. See
[the bounded implementation evidence](../poc/product-download/TASK154-GLASS-LITE-INSPECTION.md).

## IR version 3 external resources

All profiles report IR version 3. The default profile keeps its existing
inspection behavior and has empty `external_resources` and
`external_resource_references`. Glass with no external references also needs no
baggage document; unreferenced documents are not interpreted. When a Glass
application has any `TypePicture/@RefId` or `Extension/Baggage/@RefId`, inspection
requires `M-0083/Baggages.xml` and validates every declaration in that document.
The existing bounded namespace-aware XML parser, cancellation checkpoints,
archive limits, path checks and CRC checks apply to the additional input.

Only the observed project/20 hierarchy is supported:
`KNX/ManufacturerData/Manufacturer/Baggages/Baggage/FileInfo`, with exactly one
manufacturer `M-0083`, one baggage container and one FileInfo per declaration.
Baggage attributes are `Id`, `TargetPath` and `Name`; FileInfo permits only the
observed `TimeInfo` metadata, retained without date interpretation in the source
bytes. Unknown elements/attributes, invalid cardinality, non-whitespace text,
duplicate IDs, missing/wrong-kind reference targets and ambiguous payload paths
fail inspection. Unobserved `FileIntegrity` and other FileInfo attributes are
unsupported, even when the broader schema permits them.

The sole supported mapping has an explicitly empty `TargetPath` and a safe flat
ASCII PNG `Name`, resolving exactly and case-sensitively to
`M-0083/Baggages/{Name}`. Nested paths and inferred mappings are unsupported.
Each declaration must resolve to an archive member. Multiple declarations cannot
map to the same path, including case variants. Payload bytes remain immutable
and uninterpreted; no PNG decoding or execution occurs.

`external_resources` preserves manifest declaration order, exact declaration
attributes/range and the payload member name, bytes and SHA-256. Its declaration
range indexes the manifest appended to `sources` after the original three
members. Payload length is `payload.bytes.len()`; archive declared/actual lengths
and CRC are checked by the existing reader. The observed manifest declares no
payload length/checksum: computed hashes identify inspected bytes and do not
claim vendor authentication or FileIntegrity verification.
`external_resource_references` preserves each application reference in source
order, including duplicates, with element name, exact target ID, module scope
and immutable application source identity. Each target is checked against a
Baggage declaration before inspection succeeds.

Opaque UI declarations and `ExternalResourceReference`, rename, global-condition
and load-procedure compilation blockers remain. This validates the bounded
reference/payload inventory, not external-resource interpretation, a complete
device image, runtime behavior or a download plan. Optional segment Data remains
`None`; neither zero fill nor transfer behavior is inferred.
