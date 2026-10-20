# Original DCA Host Contract

## Priority and operating envelope

Priority 1 is an unchanged original manufacturer DCA on Windows. Native macOS/Linux execution and native replacements are later options. Legacy plug-ins and general ETS utility apps are not supported. The original MDT DALI package is the first reference; a separate firmware-update DCA is still required for that use case.

The host's source is Apache-2.0, but the original manufacturer DLLs are not. The host cannot require installed/running ETS after project migration. Lawfully available SDK contracts/references and compatible runtime dependencies must be separately inventoried; proprietary internal services, activation checks or strong-named concrete types can prevent standalone operation. The PoC must establish the actual outcome without patching binaries or bypassing checks.

## Ownership

| Concern | Rust editor | Windows host | knust fork |
|---|---|---|---|
| Accepted configuration / Git / cache | Owns | Reads/requests changes through bridge | None |
| Original vendor UI lifecycle | Supervises | Hosts unchanged UI and required .NET objects | None |
| Product/device/parameter semantics | Owns | Adapts required SDK contract objects/callbacks | None |
| Opaque DCA state | Owns durable envelope/blob | Produces/consumes vendor payload | None |
| Session/operation approval | Owns/enforces | Requests; cannot self-grant | Caller-controlled generic service only |
| Device identity/preflight | Owns policy | Requests through bridge | Implements generic protocol reads |
| KNX frames/session/transport | Never implements | Never implements | Owns all implementation |

If the DCA directly requires a private network driver that cannot be mediated, record the blocker. Do not quietly add Falcon or a second KNX implementation just to make the demo appear successful.

## Lifecycle

1. Inspect package/manifest/hash without executing it.
2. Confirm selected device/application match, user trust, permitted dependencies and supported OS/runtime.
3. Start a peer-authorized worker under the controlled Windows user environment and negotiate the bridge contract version.
4. Create a session against one accepted project revision, provide required SDK context, and load the original control on the required UI thread.
5. Deliver callbacks/state operations using stable request/session identifiers. Surface required human interaction.
6. Submit state changes through Rust changesets; commit only after validation and durable persistence.
7. Allow specific bus operations only with the corresponding human grant and gateway/device locks.
8. Close the session, flush accepted state through the core and preserve real failure/interruption evidence.

A worker startup, UI load and configuration state round-trip are separate capability steps. The PoC initially denies all network operations. A worker or core crash during real communication requires conservative unknown/interrupted classification.

## Minimum bridge data

Each message includes bridge version, session ID, request/correlation ID, operation kind and bounded typed arguments. Responses include result/error code and relevant revision. Callbacks are correlated and must not deadlock the WPF UI thread or block Rust async executor threads with unbounded synchronous work.

The local baseline uses a per-user named pipe with explicit peer authorization and framed JSON. A source-controlled contract defines length limits, frame size, cancellation, timeout behavior and version negotiation. Structured payloads reference large blobs by hash; session scratch access is narrowly scoped. A remote transport is not part of v1.

SDK-facing adapters may require concrete managed objects or framework-specific events; they are implemented only where the actual DCA requires them. A matching class name is not proof of binary compatibility. Maintain a table of required, implemented, fixture-only, unsupported and blocked members. Production must never ship fixture-only data as a real project/device result.

## State contract

Preserve the exact original DCA/app/hash association with saved state. A state envelope identifies the device UUID, application ID, original source IDs where needed, package hash, state format version, blob hash/size, and originating accepted revision. Opaque state remains opaque; do not rewrite undocumented bytes to make Git diffs prettier.

If a DCA changes ordinary parameters and opaque state together, capture both in a single changeset. A stale session cannot overwrite a newer project. A no-op session should not create a new configuration commit; volatile vendor fields must only be excluded from comparison when their semantics are established by evidence.

Store non-reconstructible state in versioned configuration files/blobs, not in .NET process memory or SQLite. Backup/clone tests must reopen it in the original DCA on a clean eligible machine.

## Operation grants and direct-access risk

A DCA can have configuration-only, device-read, discovery/assignment, scene/HCL transfer, replacement or firmware-update effects. Determine each actual effect from evidence, not from the button label. Some scans commission or readdress gear and are not read-only.

The Rust core authorizes exact targets and operation classes and records every mediated side effect. It enforces service/range limits and shared commissioning locks. The host must not grant itself broader permissions, choose another target or create a separate bus connection.

A separate process is not a complete security sandbox. Qualified deployments must specify actual Windows IPC/file/network access controls and their residual risks. If third-party code can still bypass required mediation, do not claim that approvals cover its device writes. This is a concrete release blocker or explicitly unsupported deployment mode, not an implementation detail to ignore.

## Interaction and localization

An interactive Windows desktop may be necessary for the original WPF control. Opening it from a noninteractive service is not promised. The core remains headless and works without the worker. MCP reports session and interaction status rather than pretending all original controls have callable backend methods.

Our own host messages use English-first resources with culture fallback. Manufacturer UI/resources are unchanged and retain whatever localization they provide. This is not a failure of our localization framework and does not authorize patching the original DLL.

## Per-version qualification record

For each supported combination retain original archive hash, application IDs, hardware/firmware/mask, runtime/bitness/OS, required SDK dependencies, host/core/fork revisions, state format, workflow list, automation level, bus mediation proof, recovery evidence and known limitations. A package update invalidates inherited assumptions until regression tests pass.

The supplied DALI DCA's static manifest is recorded in [REFERENCE-DCA.md](REFERENCE-DCA.md). It is input evidence only; no successful Windows execution or hardware integration was performed while generating this documentation.
