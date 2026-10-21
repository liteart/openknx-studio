# Bounded product static IR contract

Task: [Task-145](../tasks/Task-145.md). The standalone crate is offline inspection
only. The existing `inspect_archive` entry point always selects the original
BE-04000.02 profile. `inspect_archive_with_profile` requires an explicit profile;
`GlassV31` is a separately bounded structural observation of the registered
MDT V3.1 non-temperature family application. Archive identity does not qualify an
actual BE-GT20S.02S device, firmware or security mode.

IR version 2 adds anonymous union structure and explicit memory ownership.
A union's source identity is the selected immutable XML SHA-256 plus byte range;
it is an inspection identity, not a persistent business identity or an evaluated
union choice. Its direct memory/parameter members retain source order and ranges,
attributes and module scope. Parameters link back to an owning union source;
memory distinguishes a parameter ID from an anonymous union source. Existing
`owner_id` remains the optional literal parent XML ID.

Known Glass picture/baggage/rename declarations retain opaque names, attributes,
source ranges and scope. External baggage payload completeness and rename
semantics are unverified. Load controls retain opaque attributes, source ranges
and order. No numeric field is interpreted as KNX wire data. Absent segment Data
is `None`; inspection never supplies zeros or infers transfer behavior.

Both profiles reject unknown application elements, attributes, placements,
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
