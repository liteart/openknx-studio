# Primary References and Evidence Boundaries

Checked while preparing this documentation on 2026-10-19. These sources support selected technical facts; they are not proof that our implementation or the reference hardware works. Pin the actual source/library/toolchain version before implementation. No source below authorizes copying proprietary code or redistributing manufacturer binaries.

## SRC-01 - knust repository and license

Source: [dphi/knust](https://github.com/dphi/knust) and [LICENSE](https://github.com/dphi/knust/blob/main/LICENSE).

The inspected upstream repository presents an asynchronous Rust KNX/IP library and an MIT license. Audit its actual chosen revision, feature graph and tests before making capability claims. The editor's Apache-2.0 decision does not remove upstream notices. Relevant tasks: Task-002, Task-015, Task-060, Task-118, Task-122.

## SRC-02 - Official Rust MCP SDK

Source: [modelcontextprotocol/rust-sdk](https://github.com/modelcontextprotocol/rust-sdk) and [rmcp README](https://github.com/modelcontextprotocol/rust-sdk/blob/main/crates/rmcp/README.md).

The project describes rmcp as the official Rust MCP SDK and documents transport/capability features. Actual dependency versions and supported MCP versions are selected and pinned in implementation; this kit does not claim a floating main branch is a release dependency. Relevant tasks: Task-003, Task-083, Task-090, Task-124.

## SRC-03 - Versioned MCP stdio transport contract

Source: [MCP transports, specification 2025-11-25](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports).

This versioned source documents stdio and Streamable HTTP and requires protocol-only stdout for stdio, with stderr available for diagnostics. It is a cited version, not a claim to be the latest specification. Recheck compatibility with the SDK version chosen by Task-083. The product's v1 stdio-only scope is our engineering choice.

## SRC-04 - Official DCA versus app/legacy distinction

Source: [KNX: About Apps, DCAs and Plug-ins](https://support.knx.org/hc/en-us/articles/360000228850-About-Apps-DCAs-and-Plug-ins).

The KNX article distinguishes device-specific DCA UI extensions from general apps and older plug-in types. The page is marked deprecated; it is used only for this conceptual distinction, not as a current complete SDK specification. The user's explicit no-legacy/original-DCA priority is the project requirement. Relevant tasks: Task-006 through Task-014, Task-074.

## SRC-05 - ETS6 SDK requirements

Source: [KNX: ETS6 SDK requirements](https://support.knx.org/hc/en-us/articles/7588671243410-Requirements).

The official requirements specify .NET Framework 4.8 or higher for that SDK and describe NuGet-based SDK provisioning and ETS-installed DLLs in the normal vendor workflow. This does not establish a legal or technical standalone DCA runtime. Task-005/008 must resolve the actual reference package's needs, rights and compatible contracts.

## SRC-06 - Windows Presentation Foundation

Source: [Microsoft: WPF overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/).

Microsoft documents WPF as a Windows UI framework. Using a separate Windows host is our architecture decision; runtime/STA/control compatibility still needs tests. A Rust core does not make an unchanged vendor WPF DLL natively cross-platform. Relevant tasks: Task-007, Task-075, Task-113, Task-114.

## SRC-07 - Git reference update semantics

Source: [git-update-ref documentation](https://git-scm.com/docs/git-update-ref).

The documented expected-old-value and reference transaction mechanisms inform the compare-and-swap commit design. They do not provide an atomic multi-file working-tree transaction. The journal/recovery protocol in DATA-MODEL is our design and must be tested on supported platforms. Relevant tasks: Task-037, Task-038, Task-116.

## SRC-08 - SQLite transaction boundaries

Source: [SQLite: Atomic Commit](https://www.sqlite.org/atomiccommit.html).

SQLite documents its database commit/recovery mechanisms. These do not by themselves transact arbitrary JSON files, Git references or physical devices. The cache-only role and durable file journals are explicit project decisions. Relevant tasks: Task-037 through Task-043, Task-116.

## SRC-09 - Fluent localization

Source: [Project Fluent](https://projectfluent.org/) and [fluent-bundle Rust documentation](https://docs.rs/fluent-bundle/latest/fluent_bundle/).

The sources describe a localization system and Rust resource/message formatting APIs. Fluent is the selected engineering baseline, not a user-specified library. Pin the reviewed crate version and test our fallback/locale rules rather than assuming the framework enforces all application policy. Relevant tasks: Task-031 and Task-119.

## SRC-10 - KNX device image and verification context

Source: [KNX: Device Compare](https://support.knx.org/hc/en-us/articles/360022136799-Device-Compare).

The article describes device configuration memory images and basic comparisons involving mask/application/load status. It is context for why configuration compilation and actual device verification are separate. It does not supply our selected product's memory layout, table formats or full load procedure. Those require Task-004/015/023 evidence.

## INPUT-01 - User-supplied original MDT DCA

Source: the user's `SCN-DA64x-04_MDT_DCA_v4_0_0_0.etsapp`, identified by hash in [REFERENCE-DCA.md](REFERENCE-DCA.md).

Fresh archive/config/token inspection was performed without executing the vendor code. The resulting observations are not a dependency load graph or an interoperability result. The original archive is deliberately excluded from this documentation ZIP.

## CONVERSATION - Requirements authority

The user's final decisions in the provided conversation are the authority for product scope and constraints. Earlier assistant suggestions are not equally binding and may have been superseded. The personal-context retrieval confirmed the same final Rust/fork/DCA/OSS/Git/no-Secure decisions; it was not used to infer unpublished implementation or hardware results.

No cited material proves that manufacturers will publish native plugins, that an arbitrary .etsapp can be hosted standalone, or that a successfully parsed product is programmable. Such claims require the specific tests and gates in this kit.
