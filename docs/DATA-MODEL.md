# Data Model, Durability, and Version Control

## Sources of truth

There are three kinds of durable data, and none may exist only in SQLite.

| Data | Authority | Git treatment |
|---|---|---|
| Desired engineering configuration | Accepted canonical multi-file JSON snapshot plus referenced non-reconstructible state blobs | Versioned, one commit per accepted changeset |
| Original catalog/DCA/firmware inputs | Immutable, hash-addressed user-provided artifact store with provenance | References in Git; redistribution/synchronization only when permitted and explicitly configured |
| Deployment/jobs/observations | Durable per-job JSON records and bounded JSONL event journals | Local operational history, not normal configuration commits; included in explicit full backups |

SQLite holds indexes/read projections of these files. Deleting all database files must not lose configuration, DCA state, pending-job recovery evidence, the last known deployed identity, or the ability to rebuild indexes. Redis and a second authoritative operations database are not introduced.

Git is version control, not a hardware rollback mechanism or a guarantee that separately stored artifacts exist remotely. A snapshot is restorable only when all referenced mandatory payloads are available and hash-verified.

## Project layout

```text
project/
  project.json
  topology/areas/<uuid>.json
  topology/lines/<uuid>.json
  devices/<uuid>.json
  functions/<uuid>.json
  signals/<uuid>.json
  allocations/group-addresses.json
  products/lock.json
  extensions/lock.json
  extensions/state/<device-uuid>.json
  configuration-blobs/sha256/<hash>.bin
  .gitignore
  .git/
  .runtime/transactions/<transaction-id>/
  .runtime/jobs/<job-id>.json
  .runtime/events/<job-id>.jsonl
  .cache/project.sqlite
```

The configuration JSON and `configuration-blobs` are tracked. Non-reconstructible opaque DCA state is stored as a versioned blob with a JSON envelope containing hash, size, owning device, DCA hash/version, state format, and provenance. It is not labeled a cache and is not discarded just because the current host cannot interpret it. Document optional LFS storage only when upload/download verification and offline availability are tested.

Original product packages and executable DCAs can live in a shared user artifact store outside the repository. Their lockfile entries record content hashes and source identity, not secret download URLs. Full project export gathers permitted required originals or reports a complete missing/excluded artifact list. Do not treat the artifact store as disposable if it contains the only copy of a required input.

`.runtime` holds recoverable local operation records and transaction staging; `.cache` is freely disposable. Both are excluded from normal Git commits. Secrets and remote credentials live in an OS-protected credential facility, never in these JSON files or diagnostic exports.

## Core entities

`Project` owns schema version, opaque ID, label, locale preference, topology references, source migration provenance, and referenced catalog/extension locks. Do not put a revision hash inside the data being hashed in a self-referential way; accepted revision/commit metadata is an external transaction/result envelope.

`ProductSnapshot` is immutable normalized product data addressed by original archive hash plus parser/IR version and application identity. It includes parameter definitions/reference scopes, conditional rules, communication object templates, modules, program/load metadata, binary segments, and original translations/resources. A schema namespace or unsupported construct is preserved as provenance and diagnostic information, not silently interpreted as another format.

`DeviceInstance` has an opaque UUID, human label, topology placement, individual-address value, immutable product/application reference, parameter assignments, selected extension, and supported instance/module paths. Physical addresses are mutable properties; they are not file names or IDs. Explicit assignments are distinguishable from defaults. Keep an assignment for an inactive field only if the product semantics allow it; it must not accidentally activate an object or write unused storage.

`Endpoint` is a resolved active communication object identified by device UUID plus immutable object-reference identity and module instance path. DPT, direction/flags, and parameter-dependent activation are explicit. A human label is not an identity. A KNX communication object can participate in several associations where its profile allows this; preserve the designated transmit association/order and reject ambiguous send mappings rather than picking the first group.

`Signal` is a shared semantic channel with an opaque UUID, label, DPT/semantics, memberships and role information, and stable group-address allocation. It is a hyperedge: several producers/consumers can share it. A switching command and a feedback state normally remain distinct signals; no implicit converter, logic function, or bridge is created. Preserve explicitly permitted source multiplicity and flag semantics.

`Function` groups related signals, such as switch/dim/feedback for one light, without introducing a bus behavior engine. Rooms and visual grouping are independent of topology.

`GroupAllocation` binds signal IDs to typed KNX group addresses and preserves imported raw values and display style. No implicit renumbering. Allocate new addresses deterministically within an explicit range; detect exhaustion/collisions. Protocol-level address validation/encoding is delegated to knust; allocation policy stays in the editor.

`CompiledPlan` references the accepted project revision, product hashes, compiler/profile versions, exact targets, required identity/capacity checks, ordered typed operations, expected results and safe cancellation boundaries. Separate nondeterministic runtime data such as issue time, approval nonce, and job ID from the deterministic plan hash.

`DeploymentReceipt` records what was attempted, on which identified device, using which immutable plan, with which actual result and verification. Imported ETS flags are historical assertions, not proof that the current physical device matches. Observed state includes a timestamp and source; stale or unavailable state is explicitly unknown.

## Transaction semantics

The service presents an **accepted snapshot**. Uncommitted filesystem edits are candidate input, not silently accepted configuration. Readers see the complete previous or next accepted snapshot, never a partly applied changeset. External programs reading raw files may see intermediate replacements; do not claim that several file renames are atomically visible to arbitrary observers.

For one accepted mutation:

1. Acquire the project writer lease, record the expected accepted revision/Git parent, and detect conflicting external edits.
2. Resolve the changeset against that snapshot, validate all references and invariants, and serialize only changed documents in a staging directory.
3. Persist a transaction manifest, old/new file hashes and recoverable content, intended parent/tree, and mandatory artifact references. Flush data before the commit point using platform-tested filesystem primitives.
4. Replace affected working files. Stage only explicitly owned transaction paths into a controlled Git index. Create the intended commit and compare-and-swap the branch reference against the recorded parent.
5. After the commit point, publish the new accepted snapshot and invalidate/update its cache projection. Return the accepted revision and Git commit even if the cache needs rebuilding.
6. Complete journal cleanup only once recovery no longer depends on the staging material.

Recovery uses the durable journal plus Git commit/tree evidence. If the intended new commit was published, complete the new snapshot; if not, restore the previous accepted snapshot. If Git or the filesystem shows an unrelated external revision, stop for explicit reconciliation instead of overwriting it. A crash after commit but before the MCP reply must be recoverable by operation ID without creating another commit.

A single-file atomic write is insufficient for this contract. SQLite transactions do not make JSON or Git changes atomic. Git reference updates also do not make the entire working tree atomic. These are our application-level recovery obligations; see the primary references in [REFERENCES.md](REFERENCES.md).

## Git behavior

Initialize local versioning for every new project with explicit local author configuration. Do not require a remote or hosting-provider login. Use one logical configuration commit for a changeset, not one per parameter, and none for a no-op. Preserve imported group addresses and stable IDs in subsequent commits.

Source-code development commits and automatic user project commits are different workflows. User project history supports inspect/diff/restore and optional remote operations. Pull/fetch/merge/checkout never causes a download. Restore creates a new accepted desired-state snapshot/commit; previously programmed devices become different/unknown as appropriate.

A remote fast-forward must first produce an isolated candidate snapshot, verify all referenced configuration artifacts, and run semantic validation. Refuse dirty worktrees, unresolved conflicts, unknown schema majors, missing mandatory blobs, or incompatible accepted revisions. A semantic merge uses stable entity IDs and field paths; do not merge opaque DCA blobs or disagreeing allocations arbitrarily.

## Cache and external edits

Every cache projection records accepted revision, schema/parser versions, and relevant artifact hashes. A cache hit is valid only for that snapshot. Rebuild into a new database, then swap only after integrity/reference checks. A failed rebuild must not corrupt the files or cause a write to use stale data.

Filesystem watchers are an optimization. A rescan on open and before mutation/deployment verifies hashes. Invalid external edits produce actionable diagnostics and a quarantined candidate view. Never normalize or auto-commit an external change before validation and explicit adoption.

## Backup acceptance

A full backup can restore accepted configuration, versioned DCA state, required permitted original artifacts, Git history, and operational receipts onto a fresh machine with no SQLite database. Restore validates hashes and schema support before acceptance. Missing secrets require reauthorization and must not be reconstructed from logs. A configuration-only Git clone must state which artifacts/history/secrets it lacks; do not call it a full backup.
