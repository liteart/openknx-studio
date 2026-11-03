# Original DCA state round-trip

This component implements Task-012 under [DCA-CONTRACT.md](DCA-CONTRACT.md) and
[DATA-MODEL.md](DATA-MODEL.md). Task-011 was accepted at `1325a53`. There is no
phase gate. The first increment is read-only inspection, not a state round-trip.

## Fixed reference and current evidence

Use the unchanged MDT DALI DCA v4 (`MDT-DaliDcaPlus.dll`, SHA-256
`4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d`),
the previously verified SCN-DA641.04 product/application and genuine SDK context.
The current fixed 42-DLL profile and original archive checks remain mandatory.
The dev host has no ETS dependency. This increment grants no bus operation,
network capability or additional assembly admission; vendor internals remain
outside demonstrated containment.

Pinned PE evidence identifies a candidate local edit path:

- Original `ControllerItem` (TypeDef `0200001B`) exposes `Name` (`157`/`158`)
  and `Description` (`155`/`156`). Selected setter bodies call SDK parameter
  `set_Value`; they contain no direct bus call. This is not a whole-callgraph or
  vendor-network containment proof.
- Its public `get_ETSParameter` (`06000152`) and original
  `ETSParameters.get_Item` (`06000233`) locate a real parameter instance. The
  public static readonly key fields `040000EE`/`040000EF` supply the original
  keys. The host must not guess them or inspect private fields.
- SDK `ParameterInstanceRef` (`020001CA`) exposes `get_Value` (`060013CC`),
  `set_Value` (`060013CD`), `get_UniqueId` (`060013D0`) and `get_InternalName`
  (`060013D1`). The setter uses original dispatcher/write-access and
  customer-adjustable checks before original ObjectModel mutation. None of these
  checks may be bypassed. No setter is invoked by this increment.

Private primary reports live under `private/evidence/task012-state-roundtrip/`:
`sdk-parameter-01.json`, `mdt-controller-01.json` and `mdt-mapping-01.json`.
They are metadata-only evidence, not a Windows result or a copied implementation.

## Read-only inspection profile

The explicit `--ui-state-inspect 600` option uses the existing genuine GetDcaUi
route and 660-second outer child bound. The original returned control is hosted
inside a disabled owned WPF container. The inspection invokes no UI edit, command, parameter setter, persistence
acknowledgement or accepted configuration operation. Original UI initialization
retains its existing behavior; this is not proof that vendor code makes no local
model changes.
The ordinary UI mode is unchanged.

After actual rendering, inspect at most 512 visual nodes/depth32 and emit at most
8 distinct Name/Description bindings whose resolved source is a genuine original
ControllerItem. Use only public original mapping/getter APIs. Log bounded binding
metadata and the actual SDK parameter identity/current string value as UTF-8
Base64, with at most256 characters per field and a4096-character total
detail-log budget (maximum1536 per record including timestamp reserve). Do not call arbitrary object
ToString, inspect private fields, or serialize an unknown object. No candidate
means INCONCLUSIVE; a UI model-only value without a real SDK parameter is not a
selected persisted value. The existing65,536-byte log bound remains in force.

A Windows observation must establish the actual visible/editable binding and
parameter before selecting a changed value. Static setters alone do not prove
that an edit is offered in the currently displayed UI. Inspection snapshots are
private diagnostics, not durable production state, no-op evidence or reload proof.

## Next boundary

After a concrete operation is observed, separately implement genuine change and
capture, then a minimal Rust-owned file envelope/blob with exact device,
application, product and DCA hashes. Preserve opaque payloads without guessing a
format. Corruption, missing blob and mismatched identity must fail closed. A
successful capture requires actual bytes/hash and restart/reload evidence; a
log, in-memory SDK value, transient scratch store or SQLite row cannot substitute.
No Task-012 acceptance criterion is asserted by this inspection increment.

## Verification status

The primary inquiry is PE-only. Focused local C#5/x64/warnings-as-errors builds,
PowerShell parsing and bounded-policy tests accompany the implementation.
Windows run `4913a4b620f9` rendered the original view, inspected 512 visual
nodes, and closed normally. The traversal reached its bound and observed two
genuine fixture parameter mappings on `TextBlock` controls; visibility and
editability were not established. The private log SHA-256 is
`6a26bb02c8e98449b0f819265ab7a842e781d21c6e7041267d3f0b6704da8dcc`.
The source audit and reproduction details are recorded in Task-012. Any user
edit, state capture, no-op check, restart/reload and hardware operations remain
NOT RUN.

The additional explicit `--ui-state-editors 600` profile scans visible rows
and editors with bounded breadth-first traversal. It reports at most one
candidate and qualifies grid binding sources; a visible row and declared
column are not proof of a visible editor. Corrected Windows run `dca012e11201`
ended normally and observed one default-row TwoWay Description column linked
to a genuine fixture parameter. No grid edit or state capture was attempted.
The earlier `dca012e11101` diagnostic failed its global log limit and had an
unqualified source attribution; see Task-012 for both runs and hashes.

The separate opt-in `--ui-state-edit 600` diagnostic attempts one exact empty
fixture description through the original grid editor and synthetic WPF Enter.
It validates the target, observes the normal commit and reads the original SDK
parameter; it does not call the SDK setter directly. This is an automated
fixture attempt, not a human edit or a persistence implementation. Windows
run `dca012e11801` reached the SDK and failed because the host does not yet
support `IDispatcher.EnableDefer`; the SDK value remained unchanged. The
original `DispatcherDeferrer` pairs EnableDefer with DisableDefer, but the
actual provider's event and nesting semantics are not yet established. No
placeholder is substituted. A subsequent bounded adapter implements the
statically observed owner-thread deferral path; unsupported and ambiguous
failure cases remain fail-closed. Windows run `dca012e12701` completed one
automated edit, displayed the requested value in the original grid and read
the same value from the SDK. It ended normally. This is not a human edit or a
durable save. The task record contains earlier failed attempts and exact
evidence hashes; durable capture and restart remain NOT RUN.

The explicit `--ui-state-capture 600` profile prepares a bounded candidate
from the actual SDK value and identities after the same successful fixture edit.
Only after host final validation does it create a new log-sidecar candidate;
the consumer must independently confirm the launcher exit and input integrity.
Windows run `dca012c12801` produced such a candidate and exited 0. This is a
private handoff candidate, not a Rust durability receipt or accepted project
configuration. On macOS the Rust PoC verified and stored that candidate as a
hash-addressed artifact. Fresh Windows run `dca012a13201` loaded the validated
restore projection, resolved the exact module-scoped SDK parameter, applied
the value through the public SDK setter, and displayed it in the original DCA.
Both the child process and final checks exited 0. This is an offline fixture
proof; synthetic editor UUID/revision identify the fixture, not a physical
device or accepted Git revision. Windows filesystem durability, production IPC,
opaque vendor state and hardware operations remain NOT RUN.
