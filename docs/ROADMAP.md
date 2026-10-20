# Roadmap and Stage Gates

## Rules

This roadmap defines sequencing, not dates or effort promises. Every implementation task starts Not started. A completed report is not a passed gate: require the recorded GO/PASS outcome and maintainer approval. Do not begin full product work while the minimum PoCs remain unproven.

Numbers identify work, not mandatory serial order. Independent tasks may proceed in parallel only when their dependencies, inputs, permissions and stage gate are satisfied. Shared code changes use separate branches/worktrees and explicit review; do not have two agents overwrite the same project/fixture state.

## Stages

| Stage | Task range | Purpose | Not a deliverable of this stage |
|---|---|---|---|
| PoCs | Task-001 through Task-028 | Resolve actual DCA, protocol and compiler risks using minimal executables | Complete MCP/catalog/Git/cache product, broad hardware support, general GUI |
| Alpha | Task-029 through Task-092 | Integrate the proven components into a headless engineering backend | Stable/live-installation recommendation or complete manufacturer coverage |
| Beta | Task-093 through Task-112 | Remote Git, one-time migration, actual DALI and separate firmware DCA workflows | Automatic native DCA parity on Linux/macOS |
| RC | Task-113 through Task-125 | Freeze contracts and qualify reliability, security, platforms and support matrix | New unrelated features/hardware families |
| Release | Task-126 through Task-129 | Package, independently restore, approve and publish exact artifacts | Marketing claim of ETS parity or KNX certification |
| Post-v1 | Task-130 through Task-137 | Visual client and optional open/native/remote extension direction | Requirements that block the initial backend release |

Version label baseline: PoCs are experiments, not releases; integrated testing builds may use `1.0.0-alpha.N`, `1.0.0-beta.N` and `1.0.0-rc.N`; stable uses `1.0.0` after explicit approval. Actual tags/remotes must be authorized. No artifacts/tags have been created by this documentation package.

## G-POC: minimum technical feasibility

Owner task: [Task-028](../tasks/Task-028.md).

Required: original MDT UI runs unchanged without ETS and one configuration change round-trips; a documented reversible real-device property write/readback/restore succeeds through the fork; a minimum independent KNXPROD-derived application/configuration download works on the reference device and its behavior is verified. Lawful dependency provisioning and the repository boundaries must hold.

Not sufficient: ZIP parsing, metadata strings, assembly loading alone, a fake DCA UI, simulator-only writes, cached data from ETS, a group-value telegram, or a device already carrying the supposedly new configuration. Missing Windows/hardware/SDK/product inputs yield INCONCLUSIVE/Blocked, not PASS.

On failure, create a minimal follow-up experiment or obtain an explicit scope/architecture decision. Do not quietly switch to a native replacement, activate Secure, or build the full UI while the main uncertainty remains.

## G-ALPHA: integrated headless workflow

Owner task: [Task-092](../tasks/Task-092.md).

All mandatory Alpha tasks must be accepted, not merely the final smoke test. The scripted MCP workflow must create/configure/commit/compile/approve/program the reference device with real verification. Original DCA lifecycle/state/mediated read integration must run on clean Windows. JSON/Git transactions, recovery, cache disposability, structured errors, English localization baseline and permission enforcement must work.

Release status remains testing-only. Publish exact device/profile/DCA/platform capabilities and manual-interaction limitations. No mandatory Beta feature may be silently described as already working.

## G-BETA: supported user workflows

Owner task: [Task-112](../tasks/Task-112.md).

All mandatory Beta tasks must pass: provider-neutral optional remote Git and semantic conflict handling; full supported one-time migration with preserved addresses/parameters/artifacts/DCA state; original DALI critical workflows; and one separate original firmware-update DCA/device combination. Record actual functional versus automation support for each operation. Core remains cross-platform without requiring the Windows host.

A missing firmware file or hardware test is not a waived requirement. Changing the initial stable scope needs an explicit maintainer/user decision recorded in an ADR and updated goals/tasks, not a silent downgrade of the support matrix.

## G-RC: stabilized and qualified build

Owner task: [Task-125](../tasks/Task-125.md).

Require all RC tasks: real target-OS package smoke tests, clean-machine no-ETS/offline checks, parser/fuzz and crash matrices, operator/agent/vendor boundary review, no-Secure feature audit, English/localization validation, benchmark results, frozen hardware matrix, dependency/redistribution inventory, actual operator docs and schema/API version freeze.

No unresolved defect can remain that may cause unapproved/wrong-device writes, corrupt authoritative state, lose an acknowledged change, fake deployment success, enable a prohibited runtime path, or prevent the claimed reference use cases. Noncritical limitations need explicit scope/effect/workaround documentation. Changed code/dependencies require affected tests again.

## G-RELEASE: exact artifact acceptance

Owner task: [Task-128](../tasks/Task-128.md), publication in [Task-129](../tasks/Task-129.md).

Approve only the exact packaged hashes/source revisions that passed the independent cache-free restore and qualified workflows. Include the separate core/Windows-host distinction, required user-provided inputs, license notices/SBOM and recovery guidance. The final approval must identify the human maintainer and intended destination.

Publication is a distinct authorized external action. A coding agent cannot infer permission from a GO report. Do not overwrite release artifacts or use an older passing report to support changed bytes.

## Useful initial workstreams

After Task-001/002/003, fixture/provisioning work (Task-004/005), static DCA inspection/host work (Task-006 onward), and the fork audit/simulator work (Task-015 onward) can proceed with their actual prerequisites. The narrow product reader (Task-023) can begin once its input exists. Real writes wait for their bench approvals. The product download test joins the fork and compiler paths at Task-027.

Do not start Alpha infrastructure to occupy time while a missing critical input blocks the PoC gate. Address the missing input, document the blocker, or perform another genuinely independent minimal experiment.
