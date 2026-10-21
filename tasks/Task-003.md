# Task-003: Pin minimum PoC toolchains and verification commands

## Subject

Pin minimum PoC toolchains and verification commands

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-003 |
| Status | Blocked |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor + knust-fork / build tooling |
| Owner | Primary coordinator with implementer, protocol, dca, verifier and reviewer agents |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-005](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-027](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Make the minimal experiments reproducibly buildable without scaffolding the complete product.

## Detailed Requirements

- DR-001: Pin the Rust toolchain and commit lockfiles for the experiment binaries.
- DR-002: Select and record the Windows .NET Framework/MSBuild environment after the SDK inventory; do not assume an old chat version is sufficient.
- DR-003: Provide documented commands for Rust format/lint/test and Windows host build/test.
- DR-004: Build non-DCA Rust experiments on all three target OS families where runners are available; record unavailable runners as NOT RUN.
- DR-005: Keep Secure features disabled in the selected build commands.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: A new developer can reproduce the offline PoC builds from the documented commands.
- [x] AC-002: The Rust build does not require .NET or ETS.
- [ ] AC-003: Tools and resolved dependency versions are recorded, not floating latest references.

## Dependencies to Other Tasks

- [Task-002](Task-002.md): Create two isolated repositories and preserve licenses. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Build: compile and run a minimal Rust executable.
- TEST-002: Windows: compile a minimal host test with the pinned references.
- TEST-003: Negative: verify the documented command set does not activate Secure features.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / build tooling**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No packaging, installer, remote service, or full production CI.

## Risks and Blockers

Windows/other OS runners may not be present. Do not mark their tests passed from a Linux cross-build alone.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Blocked overall; Rust-only increment verified and reviewed; overall prerequisites incomplete |
| Source commit(s) | Editor base `a89fe47b7448e2ec9e4b872f01300d7f2d3045d7`; reviewed fork tooling `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e`; editor Rust increment `faa1befc1f73c3e6880c969fe5cd4b149a2aba47` |
| Platform | macOS 27.0 arm64; Rust 1.99.1 (58cf8f9c3 2026-10-13); full tool versions in evidence |
| Tests executed | Editor fmt/Clippy/test/run/metadata/feature-tree PASS offline with defaults disabled; 0 Rust unit tests, actual smoke executable run is TEST-001 evidence |
| Scope checker regression | Initial FAIL scanning ignored private Rust docs; fixed root-only exclusions, 12 focused scope tests and actual checker independently PASS |
| Fork checks | Metadata --no-deps PASS; original source/Cargo/lock/MIT license unchanged; static pin/license assertions and diff checks PASS; no fork build/test claim |
| Evidence location | [PoC toolchains and exact commands](../docs/POC-TOOLCHAINS.md); fork BUILDING.md and rust-toolchain.toml |
| Acceptance criteria met | AC-002 for the Rust-only build; AC-001/003 partial, Windows host/reference evidence outstanding |
| Technical outcome | PASS for macOS Rust smoke and selected compiler pin; INCONCLUSIVE for full Task-003 |
| Gate outcome | No phase gate advanced; full Task-003 is not accepted |
| Reviewer | Fork increment: predefined sequential_review PASS; editor Rust-only increment: predefined sequential_review PASS, no findings; predefined sequential_verifier reproduced checks and regression fix |
| Windows / Linux / SDK / hardware | NOT RUN; Windows RDP is visible but input failed, exact SDK/reference selection unavailable; no Linux runner supplied; no device operation |
| Remaining blockers | Windows input control, actual MSBuild/framework/SDK inventory and lawful pinned SDK references, required Windows host build/test |
| Next action | Restore RDP input and resolve references; independent Task-004 inventory depends only on accepted Tasks 001/002 |

The original missing-manifest command exited 101; the minimal dependency-free
Rust executable now runs and emits `toolchain-smoke offline=true protocol=none`.
No production module workspace, protocol, MCP, database, original DCA execution
or device service is added. The inherited fork test suite is not authorized before
Task-015 audits feature selection and side effects.

The original Windows environment attempt observed only ETS6 Home 6.4.1 Build 8718.
The DCA agent and coordinator both encountered `noWindowsAvailable` when attempting
input. No PowerShell inventory command executed, and a screenshot is not a
build/runtime test. No SDK type, target framework, host command or success is
invented to fill the missing Windows evidence. Task-004/005 may inventory the
missing inputs without treating this task as Done.

Final editor increment review: the predefined reviewer independently reproduced
format/Clippy/test/run/metadata/feature-tree, the scope checker and all 12 scope
regressions with PASS. The smoke test target intentionally has zero unit tests;
actual executable execution is the build proof. Review permits this partial
increment to commit, not Task-003 acceptance or a phase GO. The selected fork
commit was created first, preserving separate histories.

## Renewed Windows access attempt, 2026-10-21

The operator reports Windows 11 25H2, OS build 26200.9168, and ETS 6.4.1.
A fresh Windows App RDP session was selected successfully; the visible Windows
About dialog independently confirmed Windows 11 Pro 25H2 build 26200.9168,
and the ETS title showed Home 6.4.1 (Build 8718). A keyboard action dismissed
the dialog. Subsequent search/control input failed with
`Computer Use server error -10005: noWindowsAvailable`. No PowerShell inventory
command ran; architecture, MSBuild, targeting packs and SDK references remain
unverified. Screenshots, host address and project contents are not source artifacts.

The operator also confirmed that no ETS6 SDK package or KNX membership is
available. This is a concrete provisioning blocker, not evidence about the actual
license terms or technical incompatibility. Task-003 remains Blocked. The existing
Windows machine is sufficient for read-only tool inventory once input works;
a local VM is not required for that step. A separate ETS-free Windows environment
is needed later for independent runtime qualification under the DCA contract.
