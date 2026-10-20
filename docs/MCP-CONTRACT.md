# MCP and Operator Contract

## Contract status

These are planned public contracts, not tools already implemented by this kit. Tool schemas and the supported protocol version are frozen by the implementation tasks and versioned before release. The official Rust SDK and versioned MCP transport documentation are listed in [REFERENCES.md](REFERENCES.md).

The v1 transport is local stdio. The Rust process reads/writes only protocol messages on the protocol streams; diagnostics use stderr. It does not embed a model, require model credentials, or make autonomous decisions about whether a building should be changed. Remote HTTP/MCP service operation is a later explicit scope, not an unauthenticated listener added for convenience.

## Separation of responsibilities

The agent discovers exact entities, proposes a changeset and explains it to the operator. The backend resolves product semantics, validates the whole candidate, persists accepted configuration, compiles a deterministic plan, and enforces execution permission. A prompt, a tool annotation or a string saying yes is not authority to program a device.

The core's public application services implement these rules even when called outside MCP. MCP is an adapter over them, not a privileged bypass. A thin operator CLI uses the same services but a separate trusted approval capability. An agent with only MCP access cannot grant that capability.

## Planned tool families

| Family | Planned tools / behavior | Effects |
|---|---|---|
| Project | `project_create`, `project_inspect`, `project_status`, `project_validate` | Create/inspect validated local project state |
| Catalog | `catalog_import`, `catalog_search`, `catalog_inspect`, `catalog_upgrade_preview`, `device_upgrade_apply` | Store/inspect user-provided immutable product inputs; no execution |
| Changesets | `changeset_begin`, `changeset_validate`, `changeset_commit`, `changeset_discard` | Candidate editing and exact local configuration commit |
| Device | `device_create`, `device_inspect`, `device_get_parameters`, `device_set_parameters`, `device_get_objects` | Domain operations scoped to exact IDs/changesets |
| Graph | `signal_create`, `signal_connect`, `signal_disconnect`, `graph_inspect`, `impact_analyze` | Semantic memberships and deterministic impact |
| Local history | `project_history`, `project_diff`, `project_restore_preview`, `project_restore_commit` | Desired-state versioning only |
| Remote Git | `git_remote_configure`, `git_fetch`, `git_push`, `git_adopt`, `git_branch`, `git_merge_preview`, `git_merge_commit` | Explicit provider-neutral remote/branch operations; no auto-download |
| Project migration | `project_import_preview`, `project_import_commit` | One-time supported KNXPROJ migration |
| Compilation | `deployment_plan`, `deployment_preflight`, `deployment_diff` | Pure artifact creation plus separately identified read-only preflight |
| Execution | `deployment_execute` | Start a matching externally approved immutable plan; return job ID |
| Jobs | `job_status`, `job_history`, `job_cancel_request` | Durable status and cooperative cancellation |
| DCA | `dca_inspect`, `dca_capabilities`, `dca_session_open`, `dca_session_status`, `dca_session_close` | Original Windows vendor session lifecycle |
| Qualified DCA operations | Only explicitly registered tested operations, each with effect/interaction metadata | Scope-specific authorized vendor behavior, never arbitrary reflection |
| Diagnostics | `bus_discover`, `device_read_info`, bounded `bus_monitor_*` operations | Explicit lab/network reads and observation; not configuration proof |
| Storage | `project_backup`, `project_restore_backup_preview`, `project_restore_backup_commit`, `cache_rebuild`, `cache_validate` | Durable-file operations; no hardware writes |

The tool names in this baseline are canonical. Change a name only through an approved pre-freeze ADR, updating the task, schemas, tests and examples together; preserve coverage, permission separation and structured results. It must not add unrestricted raw telegram, arbitrary memory-write, shell, SQL, .NET reflection or approval-minting tools. New firmware/address/reset effects require explicit capability definitions and human authorization.

## Request and result rules

A mutation identifies a project, accepted base revision, operation/idempotency ID, and exact stable entity IDs. It either edits a candidate changeset or consumes a validated exact changeset; there is no silent mutation of an accepted project based only on an ambiguous display label.

Every result includes a structured outcome and relevant revision/changeset/plan/job IDs. Human display text has a message key, arguments and English fallback; decisions are made from codes/data, never by parsing translated prose. Resource pages contain a revision and cursor. A stale cursor or stale base revision is an explicit error.

EP-05 limits apply. Large original/compiled/state content is returned through approved hash references, not unbounded inline data. Paths are confined to configured project/artifact locations; a tool is not a general filesystem reader. Do not disclose credentials, original archive passwords, private SDK material or raw household data in diagnostic exports.

## Job behavior

Planned states include `created`, `awaiting_approval`, `approved`, `running`, `awaiting_interaction`, `cancel_requested`, `succeeded`, `failed`, `cancelled`, `interrupted`, and `unknown`. Define permitted transitions in code. A job ID or acknowledged start is not a completed download. `succeeded` requires the profile's postconditions and durable receipt.

A lost client connection does not automatically cancel the operation. Repeated start requests with the same idempotency identity must return the recorded job or an explicit conflict, not repeat physical effects. An interrupted process reconstructs state from durable records, not from a rebuilt cache that assumes nothing ran.

Cancellation is cooperative. Some configuration and firmware stages cannot safely stop immediately. Report the requested/actual state and recovery instructions without claiming rollback or aggressively killing the vendor worker.

## Trusted human approvals

The approval issuer is not an MCP tool and is not available to the MCP principal. Use a credential/channel protected from the agent execution environment. A local text prompt or TTY presence is insufficient by itself. The exact mechanism is accepted only when the permission-boundary tests pass. If an agent has arbitrary code execution as the approving OS principal, application-only enforcement is not a sufficient security boundary; disclose and restrict that deployment.

A compiled plan approval binds plan hash, exact targets, source revision, permitted effect and expiry. DCA operations need a separate bounded grant for package hash, session/device, operation class, service/range limits and purpose because discovery-driven vendor workflows may not reveal every future payload in advance. This is not a blanket all-bus grant. Firmware, addressing, reset and ordinary configuration must not share one unrestricted capability.

An expired or modified grant cannot start new effects. Once an approved non-interruptible operation has begun, expiry does not authorize abruptly cutting power/connection; completion and recovery follow the explicitly approved operation policy.

## Original DCA interaction

The first supported path is the original Windows UI, not universal headless reflection. MCP can request a session and report `requires_interaction` with a stable reason and session ID. Only successfully tested narrow adapters may claim a headless operation. The UI can be an important manual exception to the headless product design without turning the whole core into a GUI application.

Capability results distinguish package parsing, UI hosting, state read/write, mediated bus access, interactive functional support and automated support. Original method names, a successful DLL load, or the presence of an HCL XML resource do not prove these capabilities.

## No hidden execution coupling

Import, edit, changeset commit, Git push/pull/merge/restore, backup, cache rebuild, project migration, and graph visualization never cause a physical device write. A valid project commit only changes desired state. A later explicit plan/preflight/approval/execute flow is required.
