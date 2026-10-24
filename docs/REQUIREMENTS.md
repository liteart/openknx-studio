# Requirements Baseline

Later confirmed decisions take precedence over superseded proposals. Engineering baselines make the design executable; amend them through an ADR rather than guessing.

## R-001: ETS independence

**Authority:** Confirmed.

ETS is allowed only for one-time project export/import preparation. No ETS installation, process, project store, or automated ETS operation is required afterwards.

## R-002: Open source and licensing

**Authority:** Confirmed.

Project-owned source uses Apache-2.0. No paid tiers or feature gates. Preserve upstream/third-party licenses and never redistribute vendor material without rights.

## R-003: Rust with a narrow .NET exception

**Authority:** Confirmed.

Owned implementation is Rust except necessary Windows original-DCA/ETS-SDK compatibility code in a separate .NET process.

## R-004: Separate protocol fork

**Authority:** Confirmed.

Every KNX wire protocol, codec, transport, and low-level management implementation belongs only in a separate knust fork.

## R-005: Cross-platform core

**Authority:** Confirmed.

Build and run the non-DCA core on Windows, Linux, and macOS without .NET or ETS. Original DCA execution may be Windows-only.

## R-006: MCP-first deterministic operation

**Authority:** Confirmed.

Expose all owned engineering workflows through typed MCP tools/resources. AI proposes intent; the backend validates and executes deterministically.

## R-007: Headless first

**Authority:** Confirmed.

No general editor GUI in PoCs or v1. A vendor DCA window and a small trusted operator CLI are permitted necessities.

## R-008: KNXPROD support

**Authority:** Confirmed.

Import original product packages, retain provenance, and resolve the configuration semantics needed by qualified products. Explicitly classify unsupported constructs.

## R-009: Product configuration semantics

**Authority:** Confirmed.

Resolve parameters, overrides, dynamic conditions, modules, and effective communication objects correctly; never expose inactive/misbound objects as active.

## R-010: Independent device programming

**Authority:** Confirmed.

Compile and execute valid application/parameter/group-association downloads and individual addressing for qualified non-secure devices without ETS.

## R-011: Unchanged original DCAs first

**Authority:** Confirmed.

Prioritize hosting unchanged manufacturer DCAs on Windows. Do not silently replace them with native adapters or vendor web interfaces.

## R-012: MDT DALI use case

**Authority:** Confirmed.

Support the actual reference gateway DCA configuration/commissioning workflows, with per-function evidence rather than assumed web-UI parity.

## R-013: Firmware-update DCA use case

**Authority:** Confirmed.

Include a separately qualified original MDT firmware-update DCA/device path; distinguish firmware update from application/configuration download.

## R-014: No legacy or generic app scope

**Authority:** Confirmed.

Do not support legacy ETS plug-ins. General ETS utility apps are not the targeted DCA requirement.

## R-015: No KNX Secure in v1

**Authority:** Confirmed.

Exclude KNX IP Secure and Data Secure implementation, keys, keyrings, and cryptographic qualification; reject required-secure operations without downgrade.

## R-016: One-time KNXPROJ migration

**Authority:** Confirmed.

Import an existing exported project once, preserving supported configuration/addresses/artifacts; it becomes an independent project, not a live ETS synchronization.

## R-017: Git for all users

**Authority:** Confirmed.

Use automatic local Git versioning and optional remote repositories, branches, merges, and restore without account or feature-tier restrictions.

## R-018: Multi-JSON authority

**Authority:** Confirmed.

Configuration persists as stable, readable, multiple JSON files in the filesystem and is versioned in Git. Accepted writes cannot exist only in a cache.

## R-019: Disposable databases

**Authority:** Confirmed.

SQLite is only a rebuildable cache. No Redis/PostgreSQL dependency and no non-reconstructible configuration or operation evidence stored only in a database.

## R-020: Semantic connections

**Authority:** Confirmed.

Model shared signals and device endpoints, including many-to-many bindings. Group addresses are explicit stable transport mappings; visual connections come later.

## R-021: Reproducible artifacts and state

**Authority:** Engineering baseline.

Pin product/DCA inputs by hash. Preserve non-reconstructible DCA state, immutable compile inputs, and honest durable deployment evidence.

## R-022: Controlled physical effects

**Authority:** Engineering baseline.

Use exact target validation, scoped human approvals, locks, bounded operations, dry-runs, and truthful interrupted/unknown states. No Git action triggers a device write.

## R-023: Evidence-based compatibility

**Authority:** Engineering baseline.

Separate parsed, configured, compiled, programmed, verified, and DCA-automated capabilities. Publish only supported combinations with matching evidence.

## R-024: Minimum PoCs before product

**Authority:** Confirmed.

Test original DCA hosting, fork device management, and a minimum KNXPROD-to-download chain before investing in full product infrastructure.

## R-025: Local and offline operation

**Authority:** Confirmed.

Normal operation needs no cloud, online catalog, user account, LLM, Git remote, or online license service owned by this product.

Version 1 imports locally supplied manufacturer files; users obtain them directly from manufacturers. An owned online catalog/download service is outside v1. A possible v2/v3 catalog requires a later explicit decision. Catalog-related original SDK dependency requests require separate offline DCA necessity evidence and do not authorize a product catalog feature.

## R-026: Open extension direction

**Authority:** Confirmed direction.

Keep interfaces open for later native manufacturer plugins. Do not depend on a promise that manufacturers will adopt them.

## R-027: English and localization

**Authority:** Confirmed.

All descriptions/development use English. Production messages use a locale/localization framework with English fallback; stable technical identifiers never change with locale.

## R-028: Dependency and input provenance

**Authority:** Engineering baseline.

Inventory lawful SDK/vendor dependencies and primary protocol evidence. Unknown license/runtime requirements block affected support claims.

## R-029: Small tasks and disciplined Git

**Authority:** Confirmed.

Use numbered small tasks with exact requirements, prerequisites, tests, component ownership, acceptance, and a reviewed task-scoped commit workflow.

## R-030: Defensive non-functional behavior

**Authority:** Engineering baseline.

Bound untrusted input and asynchronous work, recover file transactions, detect stale state, isolate vendor failures, and test performance on a declared reference system.

## Concrete engineering defaults

These are proposed implementation policies, not measurements or newly attributed user statements. Once implementation starts, use them consistently until an approved ADR changes them.

| Policy | Baseline |
|---|---|
| EP-01 Storage | Local filesystem only for active repositories/cache/journals; network shares and cloud-sync folders are unsupported active stores in v1 |
| EP-02 Concurrency | One writer per project; immutable accepted snapshots for readers; one commissioning session per selected gateway/bus path |
| EP-03 Archive limits | Default 256 MiB compressed, 1 GiB expanded, 20,000 entries, maximum XML depth 128; configurable explicit offline import limits, never silent increases |
| EP-04 Input discipline | Reject duplicate JSON keys, XML DTD/external entities, archive path traversal/symlinks/case collisions, and unsupported critical features |
| EP-05 API limits | Default page size 100, maximum 500; 1 MiB control message limit; artifacts transferred by verified reference, not unbounded inline tool output |
| EP-06 Identifiers | Opaque UUIDs for local entities; keep original manufacturer/application/reference IDs in dedicated fields; no names/addresses as keys |
| EP-07 Serialization | UTF-8, LF, two-space indentation, newline at EOF, recursively lexically sorted object keys; sort set-like collections by stable ID, preserve semantically ordered arrays |
| EP-08 Canonical values | JSON integers/booleans/strings; exact decimals as schema-defined decimal strings where binary float would be ambiguous; UTC RFC3339 timestamps in records, never injected into deterministic compiler input |
| EP-09 Mutation durability | Validate full candidate snapshot, durable transaction journal, exact-path Git commit, then publish/acknowledge the new revision; cache updates cannot change success semantics |
| EP-10 DCA IPC | Local per-user named pipe with versioned framed JSON, peer authorization, timeouts, 1 MiB bounded messages, request/response correlation, and callback support |
| EP-11 Approvals | Human-only CLI/OS-trusted path; plan hash + target set + permitted effect + expiry; one execution grant, default maximum lifetime 10 minutes, no MCP self-grant |
| EP-12 Reproducibility | Same accepted input, product artifacts, compiler/profile versions and options produce identical compiler payload and semantic plan hash |
| EP-13 Locale | English `en-US` fallback; BCP-47 selection; Fluent for Rust-owned human messages; resource-based .NET host text; original DCA text is vendor-owned |
| EP-14 Versioning | Explicit project schema, catalog IR, plan, IPC, and MCP capability versions; fail closed on unsupported major versions; no auto-migration during reads |
| EP-15 Platform floor | Initial qualification targets: Windows 11 x64, Linux x86_64 on a pinned supported distribution, and macOS arm64 on a pinned supported release. Extra architectures are unclaimed until tested |
| EP-16 Network | Non-secure KNXnet/IP tunneling is the commissioning baseline; no hidden multicast routing/USB/RF/remote-host requirement |
| EP-17 Git automation | Local commit after each accepted logical changeset, zero commits for a no-op; no automatic push/pull/force operation; external edits remain pending until validated |
| EP-18 Device cancellation | Cooperatively cancel only at profile-defined safe boundaries; never interpret a timeout as proof that the device did not change |

## Initial performance acceptance baseline

Use a generated, non-proprietary corpus of 1,000 devices, 10,000 active endpoints, 2,000 signals, and 100,000 explicit parameter values. Record an SSD-based reference system with at least four logical CPU cores and 8 GiB RAM. On that declared host, warmed single-entity inspection has p95 <= 250 ms; paginated catalog/project search p95 <= 500 ms; a 20-parameter accepted changeset including local Git commit p95 <= 2 s; cache reconstruction <= 30 s; peak core RSS <= 512 MiB for this fixture. Record at least 30 measured operations after warm-up and retain raw results.

These budgets exclude vendor DCA memory, hardware/bus latency, cold archive imports, and remote network operations, which need separately reported measurements. They are release engineering targets, not claims about current code. Revisit through an ADR before qualification if real fixture requirements demonstrate a conflict. Never remove a failing benchmark merely to pass a gate.
