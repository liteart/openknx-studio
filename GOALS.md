# Goals and Scope

## Product objective

Build an independent, open-source KNX engineering backend that can configure supported devices and commission them without ETS. The primary operator is an AI agent using MCP, while all decisions affecting configurations and devices remain deterministic, validated, and auditable. The product must also be usable without an AI model, account, or cloud service.

ETS may be used only to obtain a one-time `.knxproj` export or necessary export-time preparation of an existing installation. Once that material is imported, project editing, product import, DCA operation, and device programming must not depend on installed or running ETS, an ETS project store, or an ETS automation service. For a new project created from `.knxprod` files, ETS is not required at all. A pre-existing reference capture may inform a lab test; ETS must not execute any part of the test operation being claimed as independent.

The objective is not a pixel-for-pixel ETS clone and not an immediate promise of universal KNX compatibility. Release claims apply to explicit combinations of product, application, hardware, firmware, mask/profile, DCA version, and operating environment.

## Confirmed product decisions

| Area | Required outcome |
|---|---|
| Ownership and license | All project-owned source is open source under Apache-2.0; no Free/Pro split, feature gates, subscriptions, or mandatory accounts |
| Implementation | Rust for the core, parsers, configuration, compiler, orchestration, MCP, persistence, and protocol work |
| Narrow exception | C#/.NET only where required to host unchanged original DCAs and their ETS-SDK contracts on Windows |
| Protocol ownership | Every KNX wire implementation belongs in a separate fork of `knust`, not in the editor or DCA bridge |
| Supported platforms | Core builds and runs on Windows, macOS, and Linux without .NET; original DCA execution may be Windows-only |
| Primary interface | MCP-first, headless-first, with a thin non-AI CLI for diagnostics and trusted human approvals |
| Product configuration | Import `.knxprod`, interpret supported parameter semantics, expose effective objects, and compile supported configurations |
| Programming | Individual addressing, application/configuration download, parameters, and group associations for qualified devices |
| DCA priority | Run original manufacturer DCAs unchanged first; Windows-only is acceptable; native replacements are secondary |
| Initial DCA examples | MDT DALI gateway configuration and MDT device firmware update DCAs; the latter package/device is not yet supplied |
| Persistence | Multiple stable JSON files and Git are the versioned configuration authority; SQLite is disposable cache only |
| Version control | Automatic local Git for everyone and optional user-controlled remote integration for everyone |
| Interaction model | Semantic signals and device endpoints, with stable technical group-address bindings; a visual client comes later |
| Language | English code, documentation, tool descriptions, tests, and default application messages; a localization framework from the production foundation |

Retain upstream and third-party notices rather than relabeling other people's work. Apache-2.0 for our source does not give redistribution rights over manufacturer DCAs, firmware, product data, or proprietary SDK components. Third-party original DCAs are user-provided add-ons, not relicensed project source.

## Explicit non-goals for version 1

No KNX IP Secure or KNX Data Secure implementation, commissioning, key management, secure keyring support, or cryptographic interoperability tests. Existing secure code in the upstream fork must not be enabled by the v1 product build. Reject a request requiring Secure explicitly; do not silently downgrade it to plaintext. Ordinary IPC access control and safe local execution are still necessary and are not KNX Secure.

No legacy ETS plug-ins, generic ETS utility-app compatibility, embedded ETS runtime, manufacturer-source rewrites as the primary strategy, paid feature tiers, or general-purpose desktop editor. The original DCA window is the one deliberate UI exception. USB/RF interfaces, routing/backbone commissioning, coupler/filter-table programming, remote Windows DCA workers, simultaneous collaborative editing, and multi-host databases are deferred unless separately approved. Model existing topology without claiming to commission every topology element.

Do not promise to automate every original DCA function headlessly. A hosted UI is a valid initial integration. Any interaction that still needs the user must be reported as `requires_interaction`, not simulated as a successful MCP operation. Do not assume the MDT web UI is functionally equivalent to the DCA.

## Stage 0: minimum risk-reduction PoCs

The purpose of the PoCs is to answer three technical questions with the smallest executable evidence, not to deliver a usable product.

### PoC A: unchanged original MDT DCA without ETS

Use the supplied MDT DALI archive. Establish lawful dependency provisioning, load the original assembly in a minimal Windows .NET host, display its original UI, provide only the reference device context it actually needs, and persist/reload one real configuration change. Run on a clean Windows environment with no ETS installation or runtime. Initially deny all bus operations. Record unresolved SDK services, assembly identities, UI-thread constraints, licensing/activation dependencies, and host calls.

A manifest read or an assembly load alone is not success. Success requires UI initialization and a state round-trip through the host; it still does not prove DALI commissioning or firmware updates.

### PoC B: minimum device management through the knust fork

On an isolated, explicitly approved test bench, establish non-secure KNXnet/IP tunneling, read device identity and a selected property, perform one documented reversible property write, read it back, and restore the prior value. Add only the protocol operations needed by this test and the selected compiler profile. Do not guess writable property IDs or memory addresses.

This proves a management path, not a full device download. A simulator cannot replace real-device evidence for this gate.

### PoC C: minimum KNXPROD-to-download path

Select one non-secure device/application and the exact matching product package. Parse only the needed structures, change a minimal parameter set, assign one group association, construct its complete required load procedure, and download using the fork. Verify application identity/status where supported and the externally visible functional effect. Start from a different or unconfigured target state, not a preloaded matching state that could hide an incomplete download.

No production database, full catalog framework, complete MCP server, Git workflow engine, or visual editor is required for these PoCs. A tiny CLI, fixture files, logs, and test harnesses are sufficient.

### PoC exit rule

Each PoC produces PASS, FAIL, or INCONCLUSIVE with reproducible evidence. The consolidated gate allows product work only after the necessary results are PASS and the maintainer approves the evidence. A failed experiment is useful information, but is not a successful feature or permission to silently substitute a different architecture. A blocked DCA host must not be replaced with native adapters without an explicit decision.

## Pre-releases

### Alpha: first integrated engineering backend

After the PoC gate, implement the modular Rust core, reusable product/configuration compiler, JSON/Git persistence with crash recovery, disposable cache, structured errors/localization, basic semantic graph, original Windows DCA host, and MCP workflows. Prove a complete create/configure/validate/approve/program/report flow for the initial supported device. Keep the original DCA window usable; its bus mediation must not leak protocol code into .NET.

Alpha is a bench/testing release. It is not a recommendation for a live household or commercial installation. Unsupported products must be visible as unsupported, and destructive operations require explicit human authorization.

### Beta: documented use cases and portability

Add one-time KNXPROJ migration, optional remote Git/branches/semantic conflict handling, complete fixture-qualified MDT DALI workflows, and a separately supplied original firmware-update DCA. Publish per-function DCA capability/automation status. Verify backups include non-reconstructible configuration and operational evidence, and verify the non-DCA core on all three operating systems.

The DALI scope is derived from the actual original DCA: discovery, identification, assignments, groups, scenes, available schedule/HCL features, read/write and replacement/diagnostic functions where exposed. Each supported function needs evidence; an untested function is not implicitly covered by successful UI hosting.

### Release candidate: stabilization, not expansion

Freeze the supported hardware/application/DCA matrix and public schemas/APIs, run the security, crash, localization, determinism, packaging, and clean-machine tests, resolve all release-blocking defects, and prepare support/recovery instructions. Add no unrelated new hardware family or plugin design during RC.

## Release 1.0

Release 1.0 is a headless MCP-first KNX editor for a finite, published support matrix, with a Windows original-DCA companion. Its acceptance is the Release gate in [ROADMAP.md](docs/ROADMAP.md), not a claim of ETS feature parity.

At least the selected ordinary device download, the supplied MDT DALI DCA's qualified workflow set, and one selected firmware-update DCA/device combination must have real hardware evidence. These initial release criteria are an engineering baseline to preserve the user's stated use cases; reducing them requires an explicit scope decision, not an implicit waiver.

All mandatory pre-release tasks and release gates must pass. No required acceptance test can remain skipped because hardware or a license was unavailable. Cross-platform packaging must clearly distinguish core support from Windows-only DCA support. Local operation must work with Git remotes disconnected and with SQLite deleted/rebuilt. No mandatory configuration may live only in memory, SQLite, the DCA host process, or an unavailable artifact cache.

## Later product direction

Build a visual connection editor over the same domain API: device ports connect to shared signal nodes, not independent duplicated pairwise wires. Preserve the semantic graph in v1 so a future view does not require a data-model rewrite. The final UI toolkit is not chosen; the Rust-first constraint still applies to owned application logic.

A documented open extension interface, manufacturer-developed native plugins, optional remote Windows workers, and alternative native adapters are future options. Manufacturer adoption is a hope, not a dependency or a promised outcome. KNX Secure remains a separate future scope decision and is not included in this task set.

## Success measurements

A developer can reproduce every claimed PoC from the pinned inputs and recorded commands. An operator can determine why a device is or is not supported, preview an exact plan, approve only that plan, and obtain honest completion or uncertainty evidence. A project can be restored from its versioned files and artifacts without a database. The fork remains useful without the editor, and the editor never implements KNX wire protocols itself.
