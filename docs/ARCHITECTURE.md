# Architecture and Ownership

## Boundary diagram

```text
AI client / CLI / future visual client
                 |
              MCP / domain command API
                 |
       Rust editor application and policies
       |             |                    |
  Project/catalog   Compiler + jobs     DCA manager
  JSON + Git        deployment plans        |
  SQLite cache           |           versioned local IPC
                         |                  |
                  public knust API    Windows .NET host
                         |            original DCA + SDK shim
                  separate fork              |
                  KNX protocol <----- mediated requests through Rust
                         |
                 non-secure KNXnet/IP
                         |
                    lab gateway/device
```

The DCA host calls back into Rust; it does not own a second KNX driver. A manufacturer DLL's request to an ETS bus service must become a typed bridge request and pass through the Rust policy/session layer to knust. If an original DCA cannot operate without privately instantiating an unmediated KNX stack, that combination is unsupported until resolved without violating the boundary.

## Repositories

| Logical repository | Code | Allowed responsibilities | Forbidden responsibilities |
|---|---|---|---|
| `knust-fork` | Rust | KNX discovery, tunneling, protocol sessions, frame parsing/encoding, DPT codecs, generic property/memory/address services, protocol-level pacing/retries | KNXPROD/KNXPROJ interpretation, project JSON, graph, MCP, Git, SQL, ETS SDK, DALI business semantics |
| `knx-editor` | Rust | Catalog, configuration evaluator, project/graph, compiler, load procedure selection, deployments, safety policy, files/Git/cache, MCP/CLI | KNX wire packet construction, copied codecs, protocol sessions, secure implementation in v1 |
| `knx-editor/dca-host` | C#/.NET | Original assembly loading, required ETS-SDK compatibility, WPF lifecycle, IPC marshaling | Authoritative state, independent KNX transport, compiler, Git/cache policy, general product logic |

These repository names are provisional labels, not already-created remotes. The fork preserves its upstream notices; the editor and project-owned host use Apache-2.0. The current upstream license is recorded in [REFERENCES.md](REFERENCES.md), but capabilities must be audited at the pinned revision, not inferred from README claims.

## Editor modules

Use a Rust workspace with explicit crates only after the PoC gate: `domain`, `product-engine`, `project-import`, `compiler`, `deployment`, `persistence`, `mcp-server`, `cli`, and `dca-bridge`. Names are engineering defaults; merge tiny crates when it improves clarity without weakening dependencies. PoCs may initially use fewer crates and synthetic DTOs.

Domain code is deterministic and has no network, filesystem, database, clock, locale, or LLM side effects. Application services provide repositories, artifact readers, clocks, approvals, and protocol adapters. The compiler consumes an immutable resolved snapshot and returns a typed plan, never raw network traffic.

The language restriction applies to owned application and protocol implementation, not declarative build files or minimal orchestration scripts. No Python/Java service or JavaScript domain implementation is introduced as a shortcut. The future UI toolkit has not been selected.

## Compiler versus protocol execution

| Example | Owner |
|---|---|
| Resolve a parameter override, visibility condition, or module instance | Product/configuration engine |
| Compute a parameter's bit offset in a product-specific image | Compiler |
| Allocate and encode product-defined group/object table contents | Compiler, using knust's public scalar/address codecs where needed |
| Select the product/mask-specific sequence of load-state actions | Compiler/load-profile interpreter |
| Validate the target against the chosen application and capacity | Deployment preflight |
| Encode an APCI property or memory write request | knust |
| Match a KNX response, handle transport sequence/ack, retry a frame safely | knust |
| Decide whether an interrupted multi-step application load may restart | Deployment engine with profile-specific policy |
| Interpret an MDT DALI configuration operation | Original DCA or a future editor-side adapter |
| Send the operation's opaque value using KNX PropertyValue service | knust |

No universal load algorithm is assumed. Support is declared per verified profile and required schema features. Unknown critical product constructs block compilation. Opaque ancillary resources can be retained without claiming that their semantics are supported.

## Typed boundary contracts

The editor-to-knust API needs discovery/tunnel lifecycle, identity reads, property reads/writes, memory reads/writes where required, load-state primitives, individual-address services, and bounded group monitoring. Exact public method names are to be discovered/designed in the fork tasks rather than invented from the old conversation.

Each operation carries typed addresses, bounded sizes, an explicit timeout, and a cancellation context. Return structured protocol errors; do not expose arbitrary shell commands or unrestricted raw network transmission through MCP. Keep product-specific offsets and payload selection above this API.

The DCA bridge uses a versioned language-neutral contract. The engineering default for local Windows is framed JSON messages over a per-user named pipe, because the PoC should not introduce a network stack solely for IPC. A PoC may choose an equally small transport with an ADR and equivalent tests. gRPC was an earlier suggestion, not a confirmed requirement. No remote listener is needed for v1.

## DCA execution model

An original DCA is vendor code, not a declarative configuration file. Official KNX documentation describes DCAs as device-specific UI extensions; the SDK requirements and WPF platform constraints are referenced in [REFERENCES.md](REFERENCES.md). Hosting compatibility is not guaranteed by using C# or by supplying an interface with a matching name.

The host must solve actual assembly identities/version binding, required interfaces and concrete SDK objects, callback/event semantics, WPF STA/message loop behavior, configuration storage, lifecycle, and permitted dependencies. Strong-named references and proprietary internal classes may be blockers. Do not patch the DCA or bypass a signature/license check to force success.

Use one isolated worker per active DCA session as the starting policy, with explicit close/reload behavior. A separate process improves failure isolation but is not a security sandbox. The Rust core owns approvals, locks, lifecycle state, and durable storage. A crash produces interrupted/unknown results as appropriate, not an invented rollback.

## Independent deployment units

The Rust backend and CLI run without .NET on all supported OS targets. The optional local DCA companion is packaged separately for Windows and requires an interactive user session where the original UI requires it. A Windows service in a noninteractive session is not promised to display a DCA window.

Remote access from a Linux/macOS core to a Windows worker is post-v1 work, not a v1 cross-platform workaround. Core users on those systems can still use all qualified non-DCA functions. Capability queries must say exactly what requires Windows, a DCA package, interaction, or hardware.

## Dependency decisions

Candidate Rust libraries include Tokio, Serde, quick-xml, rmcp, a reviewed Git binding, and SQLite bindings. Fluent is the selected localization baseline for Rust; English `.resx` resources can cover the minimal .NET host's own text. Pin actual versions and review licensing/platform support in the assigned tasks. Do not upgrade dependencies implicitly to obtain unrelated features.

No runtime dependency on Falcon, Calimero, Java, ETS, Redis, PostgreSQL, a cloud service, or an LLM is introduced. Legal SDK contracts required by the optional host are separately inventoried and reviewed; unavailable runtime rights are a blocker, not permission to copy an ETS installation.
