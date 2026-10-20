# Risks, Assumptions, and Required Inputs

## Status at documentation generation

This kit contains requirements and planned work. No Windows DCA execution, real KNX hardware operation, KNXPROD compiler test, remote fork creation or application implementation was performed as part of creating these Markdown files. The supplied DCA received a fresh read-only archive/config/token inspection, documented separately in REFERENCE-DCA. That is static evidence only.

## External inputs

| Input | Current status | Needed by |
|---|---|---|
| `SCN-DA64x-04_MDT_DCA_v4_0_0_0.etsapp` | Supplied; file identity/static inventory verified | PoC A and original DALI qualification |
| Matching DALI KNXPROD/application | Not supplied | Accurate DCA device context, catalog/migration and gateway qualification |
| Simple non-secure device and exact KNXPROD | Not selected/supplied | PoCs B/C and initial ordinary download support |
| Isolated KNXnet/IP gateway/test bench | No execution access supplied | All actual hardware results |
| Clean interactive Windows environment | Not exercised here | Original DCA/SDK/runtime proofs |
| Lawful SDK/dependency provisioning and rights | Not established | Independent original DCA host |
| DALI gear inventory/test circuit | Not supplied | Actual discovery/group/scene/HCL/replacement testing |
| Separate firmware-update DCA + device + firmware | Not supplied | Firmware qualification; the DALI archive is not a substitute |
| Supported KNXPROJ reference export and expected inventory | Not supplied | One-time migration tests |
| Git remote owner/URLs and publication permission | Not supplied | External repository creation and later releases |
| Qualified platform CI runners | Not established | Cross-platform execution evidence |

Missing input is a task blocker, not permission to invent an address, device type, update file, assembly dependency, application image or test result.

## Key risks and proof obligations

| Risk | Required proof / owner tasks | Failure response |
|---|---|---|
| Original DLL needs unavailable ETS concrete services, signatures or activation | Task-005 through Task-014; later Task-114 | Record exact blocker; no binary patch/fake success/native fallback without decision |
| SDK contract can load but cannot round-trip real state | Task-011/012/077 | Keep DCA configuration unsupported until preserved state reopens |
| Vendor bus access bypasses Rust/knust | Task-013/078/081 | Block that runtime path; do not embed Falcon or a second driver |
| WPF needs an interactive Windows session | Task-007/075/082 | Report requires_interaction; maintain headless non-DCA core |
| knust lacks required management operations | Task-015 through Task-022/060 | Implement only necessary generic services in the fork with evidence |
| Product load/parameter semantics are incomplete | Task-023 through Task-027/045 through Task-067 | Block affected product/profile; never emit guessed bytes |
| Imported project lacks originals or opaque DCA state mapping | Task-099 through Task-103 | Preserve source evidence and report incomplete migration |
| Acknowledged JSON changes lost between files and Git | Task-037/038/116 | Implement/test application recovery; SQLite is not a solution to external-file atomicity |
| Git clone missing mandatory state/artifacts | Task-043/098/127 | Refuse complete portability claim until payloads are available |
| Agent or worker can approve its own writes | Task-070/079/081/117 | Enforce a real privilege boundary or explicitly reject unsupported deployment mode |
| Device modified before an acknowledgement was lost | Task-071/072 | Mark interrupted/unknown, do not blindly replay or claim rollback |
| Firmware update needs a different protocol/online service/Secure | Task-109/110 | Record new prerequisite or v1 blocker; never use the ordinary download path blindly |
| Manufacturer plugin adoption never happens | Post-v1 tasks only | Core design remains independent; do not make roadmap success conditional on adoption |

## Deliberate exclusions, not unresolved mistakes

KNX Secure is intentionally excluded, including IP Secure, Data Secure, keys and keyring import. No attempt to repair an unsupported secure device by silently disabling its security is allowed. General application security remains necessary.

Legacy plug-ins and general ETS apps are outside scope. Universal unchanged DCA compatibility is not promised. Cross-platform applies to the Rust core; original Windows DCA limitations are accepted. The optional general visual editor, open native plugins, remote DCA worker and extra transport/platform targets are future work.

## Superseded suggestions from the conversation

Do not implement the earlier Java/Calimero backend, general .NET backend, Falcon core driver, Free/Pro split, SQLite authority, proprietary local runtime requirement, or Secure-first scope. React/Tauri/gRPC were suggestions before the final constraints, not binding technology choices. The final rule is Rust-owned application/protocol implementation plus only necessary .NET DCA compatibility, with all protocols in the fork.

The prior chat's detailed DCA descriptions were not accompanied by a successful runtime proof. Use the fresh archive observations as static starting evidence and run the planned metadata/runtime analysis. Binding redirects are not proof that a dependency is actually called; method/resource names are not proof of function availability.

## Change control

When an assumption is disproved, write an ADR/problem report with evidence, affected requirements/tasks, the smallest proposed experiment or change, and required approval. Keep the original-DCA-first and protocol-boundary constraints unless the user explicitly changes them. Do not respond to a blocked prerequisite by creating a large speculative replacement project.
