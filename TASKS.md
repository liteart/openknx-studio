# Task Index

This backlog contains **138 individually specified tasks**, numbered Task-001 through Task-138. Every task has its own Markdown file. The remaining IDs through 999 are unallocated, not empty files.

All implementation/report statuses initially are **Not started**. This kit specifies work; it does not assert that tests, forks, code or releases already exist. Task dependencies and explicit phase-gate outcomes both govern readiness. A NO-GO report does not unlock the next stage.

Read [GOALS.md](GOALS.md), [AGENTS.md](AGENTS.md), [ROADMAP.md](docs/ROADMAP.md) and [RISKS-AND-INPUTS.md](docs/RISKS-AND-INPUTS.md) first. Functional/non-functional requirements, test cases, external blockers and completion records are inside each task.

## Starting order

Start with Task-001 through Task-004 and obtain the actual fixture/rights/environment inputs. Original-DCA work, fork protocol work and the narrow product parser can proceed in parallel only once their individual dependencies are satisfied. Do not start Task-029 or other Alpha work until Task-028 explicitly records GO.

## Stage summary

| Stage | Task IDs | Count | Gate |
|---|---|---:|---|
| PoCs and feasibility gate | Task-001 to Task-028 | 28 | Task-028 |
| Alpha integration | Task-029 to Task-092 | 64 | Task-092 |
| Beta functionality | Task-093 to Task-112 | 20 | Task-112 |
| Release candidate | Task-113 to Task-125 | 13 | Task-125 |
| Release packaging/publication | Task-126 to Task-129 | 4 | Task-128 / Task-129 |
| Post-v1 optional direction | Task-130 to Task-137 | 8 | Explicit future approval |

## PoCs and feasibility gate

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-001](tasks/Task-001.md) | Establish the executable scope and decision baseline | knx-editor / documentation | None |
| [Task-002](tasks/Task-002.md) | Create two isolated repositories and preserve licenses | knx-editor + knust-fork / repository setup | [Task-001](tasks/Task-001.md) |
| [Task-003](tasks/Task-003.md) | Pin minimum PoC toolchains and verification commands | knx-editor + knust-fork / build tooling | [Task-002](tasks/Task-002.md) |
| [Task-004](tasks/Task-004.md) | Register reference artifacts and the isolated test bench | knx-editor / private test fixtures | [Task-001](tasks/Task-001.md), [Task-002](tasks/Task-002.md) |
| [Task-005](tasks/Task-005.md) | Resolve lawful DCA and SDK dependency provisioning | knx-editor / dca-host dependency review | [Task-004](tasks/Task-004.md) |
| [Task-006](tasks/Task-006.md) | Inspect the original MDT DCA package without executing it | knx-editor / poc/dca-inspect (Rust) | [Task-003](tasks/Task-003.md), [Task-004](tasks/Task-004.md) |
| [Task-007](tasks/Task-007.md) | Build a minimum Windows STA DCA host shell | knx-editor / poc/dca-host (C#/.NET) | [Task-003](tasks/Task-003.md), [Task-005](tasks/Task-005.md) |
| [Task-008](tasks/Task-008.md) | Map the actual ETS SDK surface used by the reference DCA | knx-editor / poc/dca-host analysis | [Task-005](tasks/Task-005.md), [Task-006](tasks/Task-006.md), [Task-007](tasks/Task-007.md) |
| [Task-009](tasks/Task-009.md) | Implement only the required offline SDK compatibility surface | knx-editor / poc/dca-host (C#/.NET) | [Task-008](tasks/Task-008.md) |
| [Task-010](tasks/Task-010.md) | Initialize and display the unchanged original MDT DCA | knx-editor / poc/dca-host (C#/.NET) | [Task-009](tasks/Task-009.md) |
| [Task-011](tasks/Task-011.md) | Supply one coherent device and product context to the DCA | knx-editor / poc/dca-host fixture context | [Task-010](tasks/Task-010.md), [Task-004](tasks/Task-004.md) |
| [Task-012](tasks/Task-012.md) | Round-trip one original DCA configuration change | knx-editor / poc/dca-host state bridge | [Task-011](tasks/Task-011.md) |
| [Task-013](tasks/Task-013.md) | Demonstrate fail-closed PoC DCA bus isolation | knx-editor / poc/dca-host test harness | [Task-012](tasks/Task-012.md) |
| [Task-014](tasks/Task-014.md) | Record PoC A evidence and its exact limitations | knx-editor / poc reports | [Task-010](tasks/Task-010.md), [Task-012](tasks/Task-012.md), [Task-013](tasks/Task-013.md) |
| [Task-015](tasks/Task-015.md) | Audit the pinned knust fork against the PoC operations | knust-fork / protocol audit | [Task-002](tasks/Task-002.md), [Task-003](tasks/Task-003.md), [Task-004](tasks/Task-004.md) |
| [Task-016](tasks/Task-016.md) | Build a tiny deterministic KNX management peer simulator | knust-fork / tests and test support | [Task-015](tasks/Task-015.md) |
| [Task-017](tasks/Task-017.md) | Implement and verify minimum non-secure tunneling | knust-fork / KNXnet-IP transport | [Task-015](tasks/Task-015.md), [Task-016](tasks/Task-016.md) |
| [Task-018](tasks/Task-018.md) | Read and identify the reference device through KNX management | knust-fork / device management | [Task-017](tasks/Task-017.md) |
| [Task-019](tasks/Task-019.md) | Implement minimum generic KNX property reads | knust-fork / property services | [Task-016](tasks/Task-016.md), [Task-018](tasks/Task-018.md) |
| [Task-020](tasks/Task-020.md) | Implement an explicitly authorized reversible property write | knust-fork / property services | [Task-019](tasks/Task-019.md), [Task-004](tasks/Task-004.md) |
| [Task-021](tasks/Task-021.md) | Add only the selected profile memory and load-state primitives | knust-fork / low-level management | [Task-015](tasks/Task-015.md), [Task-016](tasks/Task-016.md), [Task-018](tasks/Task-018.md), [Task-004](tasks/Task-004.md) |
| [Task-022](tasks/Task-022.md) | Record PoC B hardware read-write evidence | knust-fork / examples and PoC report | [Task-018](tasks/Task-018.md), [Task-019](tasks/Task-019.md), [Task-020](tasks/Task-020.md), [Task-021](tasks/Task-021.md) |
| [Task-023](tasks/Task-023.md) | Parse only the selected KNXPROD application for PoC C | knx-editor / poc/product-download (Rust) | [Task-003](tasks/Task-003.md), [Task-004](tasks/Task-004.md) |
| [Task-024](tasks/Task-024.md) | Encode a minimum parameter change into the reference image | knx-editor / poc/product-download compiler | [Task-023](tasks/Task-023.md) |
| [Task-025](tasks/Task-025.md) | Compile one group and object association for the reference device | knx-editor / poc/product-download compiler | [Task-023](tasks/Task-023.md), [Task-024](tasks/Task-024.md) |
| [Task-026](tasks/Task-026.md) | Assemble the minimum complete reference download plan | knx-editor / poc/product-download load plan | [Task-021](tasks/Task-021.md), [Task-024](tasks/Task-024.md), [Task-025](tasks/Task-025.md) |
| [Task-027](tasks/Task-027.md) | Execute and verify the minimum independent application download | knx-editor / poc/product-download hardware test | [Task-022](tasks/Task-022.md), [Task-026](tasks/Task-026.md), [Task-004](tasks/Task-004.md) |
| [Task-028](tasks/Task-028.md) | Decide the PoC gate before any product infrastructure | knx-editor / governance | [Task-014](tasks/Task-014.md), [Task-022](tasks/Task-022.md), [Task-027](tasks/Task-027.md) |

## Alpha integration

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-029](tasks/Task-029.md) | Create the production Rust workspace with boundary checks | knx-editor / workspace | [Task-028](tasks/Task-028.md) |
| [Task-030](tasks/Task-030.md) | Define stable errors and diagnostic event envelopes | knx-editor / domain and diagnostics | [Task-029](tasks/Task-029.md) |
| [Task-031](tasks/Task-031.md) | Introduce English-first localization in the Rust core | knx-editor / localization | [Task-030](tasks/Task-030.md) |
| [Task-032](tasks/Task-032.md) | Define versioned project and entity JSON schemas | knx-editor / domain schemas | [Task-029](tasks/Task-029.md), [Task-030](tasks/Task-030.md) |
| [Task-033](tasks/Task-033.md) | Implement deterministic multi-file serialization | knx-editor / persistence serialization | [Task-032](tasks/Task-032.md) |
| [Task-034](tasks/Task-034.md) | Implement immutable artifacts and versioned opaque state blobs | knx-editor / artifact storage | [Task-032](tasks/Task-032.md), [Task-033](tasks/Task-033.md) |
| [Task-035](tasks/Task-035.md) | Load and validate a complete accepted project snapshot | knx-editor / project repository reader | [Task-032](tasks/Task-032.md), [Task-033](tasks/Task-033.md), [Task-034](tasks/Task-034.md) |
| [Task-036](tasks/Task-036.md) | Implement isolated changesets and optimistic revision checks | knx-editor / domain command service | [Task-030](tasks/Task-030.md), [Task-035](tasks/Task-035.md) |
| [Task-037](tasks/Task-037.md) | Commit multi-file changes with a recoverable Git transaction | knx-editor / persistence transaction writer | [Task-033](tasks/Task-033.md), [Task-035](tasks/Task-035.md), [Task-036](tasks/Task-036.md) |
| [Task-038](tasks/Task-038.md) | Recover interrupted configuration transactions deterministically | knx-editor / startup recovery | [Task-037](tasks/Task-037.md) |
| [Task-039](tasks/Task-039.md) | Expose automatic local configuration history and restore | knx-editor / local Git service | [Task-037](tasks/Task-037.md), [Task-038](tasks/Task-038.md) |
| [Task-040](tasks/Task-040.md) | Build a disposable revision-tagged SQLite read projection | knx-editor / cache | [Task-035](tasks/Task-035.md), [Task-038](tasks/Task-038.md), [Task-039](tasks/Task-039.md) |
| [Task-041](tasks/Task-041.md) | Detect and validate external filesystem and Git changes | knx-editor / project watcher and adoption | [Task-035](tasks/Task-035.md), [Task-036](tasks/Task-036.md), [Task-038](tasks/Task-038.md), [Task-040](tasks/Task-040.md) |
| [Task-042](tasks/Task-042.md) | Persist job journals and deployment receipts as files | knx-editor / operational state store | [Task-030](tasks/Task-030.md), [Task-033](tasks/Task-033.md), [Task-034](tasks/Task-034.md), [Task-038](tasks/Task-038.md) |
| [Task-043](tasks/Task-043.md) | Export and restore a complete offline project backup | knx-editor / backup service | [Task-034](tasks/Task-034.md), [Task-039](tasks/Task-039.md), [Task-040](tasks/Task-040.md), [Task-042](tasks/Task-042.md) |
| [Task-044](tasks/Task-044.md) | Harden KNXPROD archive and XML ingestion | knx-editor / product-engine import boundary | [Task-023](tasks/Task-023.md), [Task-029](tasks/Task-029.md), [Task-030](tasks/Task-030.md), [Task-034](tasks/Task-034.md) |
| [Task-045](tasks/Task-045.md) | Introduce versioned product schema adapters and normalized IR | knx-editor / product-engine schema layer | [Task-044](tasks/Task-044.md), [Task-032](tasks/Task-032.md) |
| [Task-046](tasks/Task-046.md) | Resolve product hardware and application catalog relationships | knx-editor / product catalog | [Task-045](tasks/Task-045.md) |
| [Task-047](tasks/Task-047.md) | Implement typed parameter definitions and value validation | knx-editor / configuration engine | [Task-045](tasks/Task-045.md), [Task-046](tasks/Task-046.md), [Task-030](tasks/Task-030.md) |
| [Task-048](tasks/Task-048.md) | Resolve parameter and object references with correct scope | knx-editor / configuration engine | [Task-047](tasks/Task-047.md) |
| [Task-049](tasks/Task-049.md) | Evaluate dynamic parameter conditions deterministically | knx-editor / configuration engine | [Task-048](tasks/Task-048.md) |
| [Task-050](tasks/Task-050.md) | Resolve effective communication objects and associations | knx-editor / configuration engine | [Task-048](tasks/Task-048.md), [Task-049](tasks/Task-049.md) |
| [Task-051](tasks/Task-051.md) | Support qualified module and repeated-channel instances | knx-editor / configuration engine | [Task-049](tasks/Task-049.md), [Task-050](tasks/Task-050.md) |
| [Task-052](tasks/Task-052.md) | Pin product versions and preview explicit application upgrades | knx-editor / catalog lifecycle | [Task-034](tasks/Task-034.md), [Task-046](tasks/Task-046.md), [Task-051](tasks/Task-051.md), [Task-036](tasks/Task-036.md) |
| [Task-053](tasks/Task-053.md) | Publish per-product compatibility diagnostics | knx-editor / catalog capability service | [Task-045](tasks/Task-045.md), [Task-047](tasks/Task-047.md), [Task-048](tasks/Task-048.md), [Task-049](tasks/Task-049.md), [Task-050](tasks/Task-050.md), [Task-051](tasks/Task-051.md), [Task-052](tasks/Task-052.md) |
| [Task-054](tasks/Task-054.md) | Implement device and topology lifecycle commands | knx-editor / domain devices and topology | [Task-036](tasks/Task-036.md), [Task-046](tasks/Task-046.md), [Task-050](tasks/Task-050.md), [Task-053](tasks/Task-053.md) |
| [Task-055](tasks/Task-055.md) | Implement shared semantic signals and endpoint memberships | knx-editor / domain graph | [Task-050](tasks/Task-050.md), [Task-054](tasks/Task-054.md), [Task-032](tasks/Task-032.md) |
| [Task-056](tasks/Task-056.md) | Allocate and preserve stable KNX group-address bindings | knx-editor / graph allocation | [Task-055](tasks/Task-055.md), [Task-054](tasks/Task-054.md) |
| [Task-057](tasks/Task-057.md) | Implement parameter and graph edit domain commands | knx-editor / application commands | [Task-036](tasks/Task-036.md), [Task-047](tasks/Task-047.md), [Task-049](tasks/Task-049.md), [Task-055](tasks/Task-055.md), [Task-056](tasks/Task-056.md) |
| [Task-058](tasks/Task-058.md) | Validate graph DPT and communication semantics | knx-editor / graph validation | [Task-050](tasks/Task-050.md), [Task-055](tasks/Task-055.md), [Task-056](tasks/Task-056.md), [Task-057](tasks/Task-057.md) |
| [Task-059](tasks/Task-059.md) | Calculate deterministic configuration impact | knx-editor / impact analysis | [Task-052](tasks/Task-052.md), [Task-054](tasks/Task-054.md), [Task-057](tasks/Task-057.md), [Task-058](tasks/Task-058.md) |
| [Task-060](tasks/Task-060.md) | Stabilize the fork public management API for the editor | knust-fork / public API | [Task-015](tasks/Task-015.md), [Task-021](tasks/Task-021.md), [Task-028](tasks/Task-028.md) |
| [Task-061](tasks/Task-061.md) | Bound segmented property and memory transfers | knust-fork / management transfers | [Task-060](tasks/Task-060.md), [Task-016](tasks/Task-016.md) |
| [Task-062](tasks/Task-062.md) | Enforce protocol session lifecycle and bounded failure behavior | knust-fork / protocol sessions | [Task-060](tasks/Task-060.md), [Task-061](tasks/Task-061.md), [Task-017](tasks/Task-017.md) |
| [Task-063](tasks/Task-063.md) | Qualify individual-address programming on the reference bench | knust-fork / individual address services | [Task-060](tasks/Task-060.md), [Task-062](tasks/Task-062.md), [Task-004](tasks/Task-004.md) |
| [Task-064](tasks/Task-064.md) | Expose bounded group monitoring and verified DPT helpers | knust-fork / monitoring and DPT API | [Task-060](tasks/Task-060.md), [Task-062](tasks/Task-062.md) |
| [Task-065](tasks/Task-065.md) | Regression-test the non-secure protocol feature set | knust-fork / protocol qualification | [Task-061](tasks/Task-061.md), [Task-062](tasks/Task-062.md), [Task-063](tasks/Task-063.md), [Task-064](tasks/Task-064.md) |
| [Task-066](tasks/Task-066.md) | Implement production device-image compilation | knx-editor / compiler | [Task-047](tasks/Task-047.md), [Task-048](tasks/Task-048.md), [Task-050](tasks/Task-050.md), [Task-051](tasks/Task-051.md), [Task-060](tasks/Task-060.md), [Task-024](tasks/Task-024.md), [Task-025](tasks/Task-025.md) |
| [Task-067](tasks/Task-067.md) | Interpret only qualified product load procedures | knx-editor / compiler load profiles | [Task-026](tasks/Task-026.md), [Task-053](tasks/Task-053.md), [Task-066](tasks/Task-066.md), [Task-060](tasks/Task-060.md) |
| [Task-068](tasks/Task-068.md) | Create reproducible immutable plans and compiler fingerprints | knx-editor / compiler plan model | [Task-059](tasks/Task-059.md), [Task-066](tasks/Task-066.md), [Task-067](tasks/Task-067.md), [Task-034](tasks/Task-034.md) |
| [Task-069](tasks/Task-069.md) | Implement target identity and capacity preflight | knx-editor / deployment preflight | [Task-053](tasks/Task-053.md), [Task-060](tasks/Task-060.md), [Task-068](tasks/Task-068.md), [Task-058](tasks/Task-058.md) |
| [Task-070](tasks/Task-070.md) | Enforce human-only exact-plan and DCA operation grants | knx-editor / authorization and operator CLI | [Task-030](tasks/Task-030.md), [Task-068](tasks/Task-068.md), [Task-069](tasks/Task-069.md) |
| [Task-071](tasks/Task-071.md) | Execute durable sequential programming jobs | knx-editor / deployment executor | [Task-042](tasks/Task-042.md), [Task-065](tasks/Task-065.md), [Task-067](tasks/Task-067.md), [Task-069](tasks/Task-069.md), [Task-070](tasks/Task-070.md) |
| [Task-072](tasks/Task-072.md) | Recover interrupted deployment jobs without false rollback | knx-editor / deployment recovery | [Task-042](tasks/Task-042.md), [Task-071](tasks/Task-071.md) |
| [Task-073](tasks/Task-073.md) | Compare desired, compiled, deployed and observed states | knx-editor / deployment status | [Task-059](tasks/Task-059.md), [Task-068](tasks/Task-068.md), [Task-071](tasks/Task-071.md), [Task-072](tasks/Task-072.md), [Task-040](tasks/Task-040.md) |
| [Task-074](tasks/Task-074.md) | Register and verify unchanged DCA packages | knx-editor / DCA registry | [Task-006](tasks/Task-006.md), [Task-034](tasks/Task-034.md), [Task-053](tasks/Task-053.md), [Task-029](tasks/Task-029.md) |
| [Task-075](tasks/Task-075.md) | Supervise the Windows DCA worker with versioned local IPC | knx-editor / Rust DCA manager + dca-host | [Task-007](tasks/Task-007.md), [Task-010](tasks/Task-010.md), [Task-029](tasks/Task-029.md), [Task-030](tasks/Task-030.md), [Task-074](tasks/Task-074.md) |
| [Task-076](tasks/Task-076.md) | Map required ETS project/device contracts to Rust services | knx-editor / DCA bridge and SDK adapter | [Task-009](tasks/Task-009.md), [Task-011](tasks/Task-011.md), [Task-054](tasks/Task-054.md), [Task-057](tasks/Task-057.md), [Task-075](tasks/Task-075.md) |
| [Task-077](tasks/Task-077.md) | Persist real DCA state through editor changesets | knx-editor / DCA state integration | [Task-012](tasks/Task-012.md), [Task-034](tasks/Task-034.md), [Task-036](tasks/Task-036.md), [Task-037](tasks/Task-037.md), [Task-076](tasks/Task-076.md) |
| [Task-078](tasks/Task-078.md) | Mediate original DCA bus requests through Rust and knust | knx-editor / DCA bridge + knust public API | [Task-013](tasks/Task-013.md), [Task-060](tasks/Task-060.md), [Task-069](tasks/Task-069.md), [Task-070](tasks/Task-070.md), [Task-071](tasks/Task-071.md), [Task-076](tasks/Task-076.md) |
| [Task-079](tasks/Task-079.md) | Authorize interactive DCA operations and session effects | knx-editor / DCA authorization | [Task-070](tasks/Task-070.md), [Task-075](tasks/Task-075.md), [Task-078](tasks/Task-078.md) |
| [Task-080](tasks/Task-080.md) | Describe DCA capabilities and interaction requirements | knx-editor / DCA capability service | [Task-053](tasks/Task-053.md), [Task-075](tasks/Task-075.md), [Task-077](tasks/Task-077.md), [Task-078](tasks/Task-078.md), [Task-079](tasks/Task-079.md) |
| [Task-081](tasks/Task-081.md) | Constrain DCA worker failures and access | knx-editor / Windows worker containment | [Task-075](tasks/Task-075.md), [Task-078](tasks/Task-078.md), [Task-079](tasks/Task-079.md) |
| [Task-082](tasks/Task-082.md) | Qualify original DCA lifecycle, state and mediated read integration | knx-editor / DCA integration tests | [Task-076](tasks/Task-076.md), [Task-077](tasks/Task-077.md), [Task-078](tasks/Task-078.md), [Task-079](tasks/Task-079.md), [Task-080](tasks/Task-080.md), [Task-081](tasks/Task-081.md) |
| [Task-083](tasks/Task-083.md) | Implement a bounded MCP stdio server using the Rust SDK | knx-editor / mcp-server | [Task-029](tasks/Task-029.md), [Task-030](tasks/Task-030.md), [Task-031](tasks/Task-031.md) |
| [Task-084](tasks/Task-084.md) | Provide revision-aware paginated project and catalog resources | knx-editor / MCP resources | [Task-035](tasks/Task-035.md), [Task-040](tasks/Task-040.md), [Task-053](tasks/Task-053.md), [Task-055](tasks/Task-055.md), [Task-080](tasks/Task-080.md), [Task-083](tasks/Task-083.md) |
| [Task-085](tasks/Task-085.md) | Expose project, catalog and changeset MCP tools | knx-editor / MCP project tools | [Task-036](tasks/Task-036.md), [Task-037](tasks/Task-037.md), [Task-039](tasks/Task-039.md), [Task-043](tasks/Task-043.md), [Task-044](tasks/Task-044.md), [Task-083](tasks/Task-083.md), [Task-084](tasks/Task-084.md) |
| [Task-086](tasks/Task-086.md) | Expose device and semantic-connection MCP tools | knx-editor / MCP configuration tools | [Task-054](tasks/Task-054.md), [Task-055](tasks/Task-055.md), [Task-056](tasks/Task-056.md), [Task-057](tasks/Task-057.md), [Task-058](tasks/Task-058.md), [Task-059](tasks/Task-059.md), [Task-085](tasks/Task-085.md) |
| [Task-087](tasks/Task-087.md) | Expose compile, preflight and approved deployment MCP tools | knx-editor / MCP deployment tools | [Task-068](tasks/Task-068.md), [Task-069](tasks/Task-069.md), [Task-070](tasks/Task-070.md), [Task-071](tasks/Task-071.md), [Task-072](tasks/Task-072.md), [Task-073](tasks/Task-073.md), [Task-083](tasks/Task-083.md) |
| [Task-088](tasks/Task-088.md) | Expose original DCA session tools with honest interaction status | knx-editor / MCP DCA tools | [Task-080](tasks/Task-080.md), [Task-082](tasks/Task-082.md), [Task-083](tasks/Task-083.md), [Task-087](tasks/Task-087.md) |
| [Task-089](tasks/Task-089.md) | Expose durable job status and bounded bus diagnostics | knx-editor / MCP jobs and diagnostics | [Task-042](tasks/Task-042.md), [Task-064](tasks/Task-064.md), [Task-071](tasks/Task-071.md), [Task-072](tasks/Task-072.md), [Task-073](tasks/Task-073.md), [Task-083](tasks/Task-083.md), [Task-087](tasks/Task-087.md) |
| [Task-090](tasks/Task-090.md) | Test MCP misuse, idempotency and authority boundaries | knx-editor / MCP security and contract tests | [Task-084](tasks/Task-084.md), [Task-085](tasks/Task-085.md), [Task-086](tasks/Task-086.md), [Task-087](tasks/Task-087.md), [Task-088](tasks/Task-088.md), [Task-089](tasks/Task-089.md) |
| [Task-091](tasks/Task-091.md) | Run the first complete headless MCP engineering workflow | knx-editor / end-to-end tests | [Task-043](tasks/Task-043.md), [Task-065](tasks/Task-065.md), [Task-073](tasks/Task-073.md), [Task-082](tasks/Task-082.md), [Task-085](tasks/Task-085.md), [Task-086](tasks/Task-086.md), [Task-087](tasks/Task-087.md), [Task-088](tasks/Task-088.md), [Task-089](tasks/Task-089.md), [Task-090](tasks/Task-090.md) |
| [Task-092](tasks/Task-092.md) | Approve the alpha integration gate | knx-editor / release governance | [Task-091](tasks/Task-091.md) |

## Beta functionality

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-093](tasks/Task-093.md) | Configure optional Git remotes and protected credentials | knx-editor / Git remote service | [Task-092](tasks/Task-092.md), [Task-039](tasks/Task-039.md), [Task-070](tasks/Task-070.md) |
| [Task-094](tasks/Task-094.md) | Push only an explicitly selected validated Git revision | knx-editor / Git push | [Task-093](tasks/Task-093.md), [Task-039](tasks/Task-039.md), [Task-043](tasks/Task-043.md) |
| [Task-095](tasks/Task-095.md) | Fetch and adopt validated fast-forward changes | knx-editor / Git fetch and pull | [Task-093](tasks/Task-093.md), [Task-035](tasks/Task-035.md), [Task-037](tasks/Task-037.md), [Task-041](tasks/Task-041.md) |
| [Task-096](tasks/Task-096.md) | Support explicit branch switching and non-destructive restore | knx-editor / Git branches | [Task-039](tasks/Task-039.md), [Task-041](tasks/Task-041.md), [Task-095](tasks/Task-095.md) |
| [Task-097](tasks/Task-097.md) | Merge project JSON semantically by stable identity | knx-editor / Git semantic merge | [Task-032](tasks/Task-032.md), [Task-035](tasks/Task-035.md), [Task-055](tasks/Task-055.md), [Task-056](tasks/Task-056.md), [Task-095](tasks/Task-095.md), [Task-096](tasks/Task-096.md) |
| [Task-098](tasks/Task-098.md) | Verify artifact portability across clones and remotes | knx-editor / Git artifact transport policy | [Task-034](tasks/Task-034.md), [Task-043](tasks/Task-043.md), [Task-094](tasks/Task-094.md), [Task-095](tasks/Task-095.md), [Task-097](tasks/Task-097.md) |
| [Task-099](tasks/Task-099.md) | Register KNXPROJ fixtures and safe import preparation | knx-editor / project-import boundary | [Task-092](tasks/Task-092.md), [Task-044](tasks/Task-044.md), [Task-004](tasks/Task-004.md) |
| [Task-100](tasks/Task-100.md) | Import project identity, topology and device instances | knx-editor / KNXPROJ structural import | [Task-099](tasks/Task-099.md), [Task-046](tasks/Task-046.md), [Task-054](tasks/Task-054.md) |
| [Task-101](tasks/Task-101.md) | Import parameter assignments and group-object bindings | knx-editor / KNXPROJ configuration import | [Task-100](tasks/Task-100.md), [Task-047](tasks/Task-047.md), [Task-048](tasks/Task-048.md), [Task-050](tasks/Task-050.md), [Task-055](tasks/Task-055.md), [Task-056](tasks/Task-056.md), [Task-058](tasks/Task-058.md) |
| [Task-102](tasks/Task-102.md) | Preserve imported product artifacts and DCA state | knx-editor / KNXPROJ artifact and extension import | [Task-100](tasks/Task-100.md), [Task-101](tasks/Task-101.md), [Task-034](tasks/Task-034.md), [Task-074](tasks/Task-074.md), [Task-077](tasks/Task-077.md) |
| [Task-103](tasks/Task-103.md) | Preview and accept one-time KNXPROJ migration atomically | knx-editor / import orchestration and MCP | [Task-100](tasks/Task-100.md), [Task-101](tasks/Task-101.md), [Task-102](tasks/Task-102.md), [Task-035](tasks/Task-035.md), [Task-037](tasks/Task-037.md), [Task-043](tasks/Task-043.md), [Task-085](tasks/Task-085.md) |
| [Task-104](tasks/Task-104.md) | Qualify original MDT DALI discovery and identification | knx-editor / original DCA DALI qualification | [Task-092](tasks/Task-092.md), [Task-082](tasks/Task-082.md), [Task-079](tasks/Task-079.md), [Task-088](tasks/Task-088.md), [Task-004](tasks/Task-004.md) |
| [Task-105](tasks/Task-105.md) | Qualify original MDT DALI address and group assignments | knx-editor / original DCA DALI qualification | [Task-104](tasks/Task-104.md), [Task-077](tasks/Task-077.md), [Task-079](tasks/Task-079.md) |
| [Task-106](tasks/Task-106.md) | Qualify original MDT DALI scene editing and transfer | knx-editor / original DCA DALI qualification | [Task-105](tasks/Task-105.md), [Task-077](tasks/Task-077.md), [Task-079](tasks/Task-079.md) |
| [Task-107](tasks/Task-107.md) | Qualify original MDT HCL and schedule configuration | knx-editor / original DCA DALI qualification | [Task-105](tasks/Task-105.md), [Task-106](tasks/Task-106.md), [Task-077](tasks/Task-077.md), [Task-079](tasks/Task-079.md) |
| [Task-108](tasks/Task-108.md) | Qualify original MDT readback, diagnostics and gear replacement | knx-editor / original DCA DALI qualification | [Task-104](tasks/Task-104.md), [Task-105](tasks/Task-105.md), [Task-106](tasks/Task-106.md), [Task-107](tasks/Task-107.md), [Task-079](tasks/Task-079.md) |
| [Task-109](tasks/Task-109.md) | Inspect a separate original firmware-update DCA and workflow | knx-editor / firmware DCA qualification input | [Task-092](tasks/Task-092.md), [Task-004](tasks/Task-004.md), [Task-005](tasks/Task-005.md), [Task-074](tasks/Task-074.md), [Task-080](tasks/Task-080.md) |
| [Task-110](tasks/Task-110.md) | Run an approved original-DCA firmware update and verify recovery policy | knx-editor / original firmware DCA integration | [Task-109](tasks/Task-109.md), [Task-078](tasks/Task-078.md), [Task-079](tasks/Task-079.md), [Task-081](tasks/Task-081.md), [Task-088](tasks/Task-088.md), [Task-072](tasks/Task-072.md) |
| [Task-111](tasks/Task-111.md) | Publish per-function DCA qualification and unresolved gaps | knx-editor / compatibility matrix | [Task-103](tasks/Task-103.md), [Task-104](tasks/Task-104.md), [Task-105](tasks/Task-105.md), [Task-106](tasks/Task-106.md), [Task-107](tasks/Task-107.md), [Task-108](tasks/Task-108.md), [Task-110](tasks/Task-110.md), [Task-080](tasks/Task-080.md) |
| [Task-112](tasks/Task-112.md) | Approve the beta functionality and portability gate | knx-editor / release governance | [Task-098](tasks/Task-098.md), [Task-103](tasks/Task-103.md), [Task-111](tasks/Task-111.md), [Task-092](tasks/Task-092.md) |

## Release candidate

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-113](tasks/Task-113.md) | Build and smoke-test the declared platform packages | knx-editor + knust-fork / CI and packaging | [Task-112](tasks/Task-112.md), [Task-003](tasks/Task-003.md), [Task-029](tasks/Task-029.md) |
| [Task-114](tasks/Task-114.md) | Verify clean-machine ETS and service independence | knx-editor / installation qualification | [Task-113](tasks/Task-113.md), [Task-082](tasks/Task-082.md), [Task-103](tasks/Task-103.md), [Task-110](tasks/Task-110.md) |
| [Task-115](tasks/Task-115.md) | Fuzz and bound untrusted archive and configuration inputs | knx-editor + knust-fork / defensive tests | [Task-112](tasks/Task-112.md), [Task-044](tasks/Task-044.md), [Task-045](tasks/Task-045.md), [Task-065](tasks/Task-065.md), [Task-090](tasks/Task-090.md), [Task-099](tasks/Task-099.md) |
| [Task-116](tasks/Task-116.md) | Execute the full file, Git and cache crash matrix | knx-editor / persistence reliability | [Task-112](tasks/Task-112.md), [Task-037](tasks/Task-037.md), [Task-038](tasks/Task-038.md), [Task-040](tasks/Task-040.md), [Task-041](tasks/Task-041.md), [Task-042](tasks/Task-042.md), [Task-097](tasks/Task-097.md), [Task-098](tasks/Task-098.md) |
| [Task-117](tasks/Task-117.md) | Review threat model and enforce dangerous-operation boundaries | knx-editor / security review | [Task-112](tasks/Task-112.md), [Task-070](tasks/Task-070.md), [Task-079](tasks/Task-079.md), [Task-081](tasks/Task-081.md), [Task-090](tasks/Task-090.md), [Task-093](tasks/Task-093.md) |
| [Task-118](tasks/Task-118.md) | Audit release features, licenses and repository ownership | knx-editor + knust-fork / architecture audit | [Task-112](tasks/Task-112.md), [Task-065](tasks/Task-065.md), [Task-076](tasks/Task-076.md), [Task-078](tasks/Task-078.md), [Task-113](tasks/Task-113.md) |
| [Task-119](tasks/Task-119.md) | Audit English resources and localization invariants | knx-editor / localization qualification | [Task-031](tasks/Task-031.md), [Task-083](tasks/Task-083.md), [Task-088](tasks/Task-088.md), [Task-111](tasks/Task-111.md), [Task-113](tasks/Task-113.md) |
| [Task-120](tasks/Task-120.md) | Benchmark the declared performance and resource budgets | knx-editor / performance tests | [Task-112](tasks/Task-112.md), [Task-040](tasks/Task-040.md), [Task-059](tasks/Task-059.md), [Task-084](tasks/Task-084.md), [Task-116](tasks/Task-116.md) |
| [Task-121](tasks/Task-121.md) | Run the final hardware and application compatibility corpus | knx-editor + knust-fork / integration qualification | [Task-112](tasks/Task-112.md), [Task-113](tasks/Task-113.md), [Task-114](tasks/Task-114.md), [Task-116](tasks/Task-116.md) |
| [Task-122](tasks/Task-122.md) | Produce the dependency inventory and redistribution manifest | knx-editor + knust-fork / supply chain | [Task-113](tasks/Task-113.md), [Task-118](tasks/Task-118.md), [Task-005](tasks/Task-005.md) |
| [Task-123](tasks/Task-123.md) | Write operator, recovery and contributor documentation | knx-editor / documentation | [Task-111](tasks/Task-111.md), [Task-113](tasks/Task-113.md), [Task-114](tasks/Task-114.md), [Task-117](tasks/Task-117.md), [Task-119](tasks/Task-119.md), [Task-122](tasks/Task-122.md) |
| [Task-124](tasks/Task-124.md) | Freeze schema, API and migration compatibility contracts | knx-editor / compatibility contracts | [Task-112](tasks/Task-112.md), [Task-032](tasks/Task-032.md), [Task-068](tasks/Task-068.md), [Task-075](tasks/Task-075.md), [Task-083](tasks/Task-083.md), [Task-103](tasks/Task-103.md), [Task-119](tasks/Task-119.md) |
| [Task-125](tasks/Task-125.md) | Approve the release-candidate stabilization gate | knx-editor / release governance | [Task-113](tasks/Task-113.md), [Task-114](tasks/Task-114.md), [Task-115](tasks/Task-115.md), [Task-116](tasks/Task-116.md), [Task-117](tasks/Task-117.md), [Task-118](tasks/Task-118.md), [Task-119](tasks/Task-119.md), [Task-120](tasks/Task-120.md), [Task-121](tasks/Task-121.md), [Task-122](tasks/Task-122.md), [Task-123](tasks/Task-123.md), [Task-124](tasks/Task-124.md) |

## Release packaging/publication

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-126](tasks/Task-126.md) | Create immutable release artifacts and provenance | knx-editor + knust-fork / release build | [Task-125](tasks/Task-125.md) |
| [Task-127](tasks/Task-127.md) | Perform an independent cache-free restore and operation drill | knx-editor / release acceptance | [Task-126](tasks/Task-126.md), [Task-043](tasks/Task-043.md), [Task-098](tasks/Task-098.md), [Task-114](tasks/Task-114.md) |
| [Task-128](tasks/Task-128.md) | Obtain final release sign-off for scope and evidence | knx-editor / release governance | [Task-126](tasks/Task-126.md), [Task-127](tasks/Task-127.md), [Task-125](tasks/Task-125.md) |
| [Task-129](tasks/Task-129.md) | Publish the approved stable release and archive evidence | knx-editor + knust-fork / authorized release publication | [Task-128](tasks/Task-128.md) |

## Post-v1 optional direction

| Task | Subject | Component / Git project | Dependencies |
|---|---|---|---|
| [Task-130](tasks/Task-130.md) | Define the later visual client over the existing domain API | knx-editor / future visual-client design | [Task-129](tasks/Task-129.md), [Task-055](tasks/Task-055.md), [Task-058](tasks/Task-058.md), [Task-086](tasks/Task-086.md) |
| [Task-131](tasks/Task-131.md) | Implement a read-only device and signal graph view | knx-editor / visual client | [Task-130](tasks/Task-130.md), [Task-084](tasks/Task-084.md) |
| [Task-132](tasks/Task-132.md) | Implement previewed visual connection editing | knx-editor / visual client | [Task-131](tasks/Task-131.md), [Task-057](tasks/Task-057.md), [Task-058](tasks/Task-058.md), [Task-086](tasks/Task-086.md) |
| [Task-133](tasks/Task-133.md) | Persist visual layouts without affecting device compilation | knx-editor / visual layout persistence | [Task-131](tasks/Task-131.md), [Task-132](tasks/Task-132.md), [Task-033](tasks/Task-033.md), [Task-068](tasks/Task-068.md) |
| [Task-134](tasks/Task-134.md) | Specify an open native extension capability contract | knx-editor / extension API design | [Task-129](tasks/Task-129.md), [Task-080](tasks/Task-080.md) |
| [Task-135](tasks/Task-135.md) | Implement one synthetic Rust extension conformance example | knx-editor / native extension example | [Task-134](tasks/Task-134.md), [Task-070](tasks/Task-070.md), [Task-077](tasks/Task-077.md) |
| [Task-136](tasks/Task-136.md) | Evaluate an explicitly secured remote Windows DCA worker | knx-editor / remote DCA bridge | [Task-129](tasks/Task-129.md), [Task-075](tasks/Task-075.md), [Task-079](tasks/Task-079.md), [Task-081](tasks/Task-081.md), [Task-088](tasks/Task-088.md) |
| [Task-137](tasks/Task-137.md) | Evaluate a native fallback only after an explicit original-DCA blocker | knx-editor / optional native compatibility research | [Task-129](tasks/Task-129.md), [Task-134](tasks/Task-134.md), [Task-135](tasks/Task-135.md), [Task-111](tasks/Task-111.md) |

## Development tooling

| Task | Subject | Component | Dependencies |
|---|---|---|---|
| [Task-138](tasks/Task-138.md) | Configure project agents and model routing | Editor development tooling | Explicit user request; no product gate unlocked |

## How to extend the backlog

Use [TASK-TEMPLATE.md](templates/TASK-TEMPLATE.md). Assign the next unused ID (initially Task-139), define exact requirements/tests/ownership and connect the dependency graph. Do not renumber completed or referenced tasks. Add protocol gaps to the fork, not as implementation shortcuts inside an editor or host task. Update the requirement traceability and affected phase gate.
