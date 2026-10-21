# Task-003: Pin minimum PoC toolchains and verification commands

## Subject

Pin minimum PoC toolchains and verification commands

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-003 |
| Status | Done |
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

- [x] AC-001: A new developer can reproduce the offline PoC builds from the documented commands.
- [x] AC-002: The Rust build does not require .NET or ETS.
- [x] AC-003: Tools and resolved dependency versions are recorded, not floating latest references.

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

## Initial Rust-only completion record (historical)

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

## Operator-supplied Build Tools inventory, 2026-10-21

The operator supplied `vswhere` JSON reporting Visual Studio Build Tools 2026,
product `Microsoft.VisualStudio.Product.BuildTools`, display version `18.9.2`,
installation version `18.9.12120.119`, and catalog build `18.9.12120.119`.
The installation record reports complete, launchable, non-prerelease, with no
reboot required. This is operator-supplied installer inventory; it is not an
independently executed MSBuild command or a successful build.

The response did not include the architecture result, command-discovery output,
or .NET Framework reference-assembly directory listing. Their absence from this
response does not establish absent tools or packs. Exact executable version,
compatible targeting packs and SDK references remain unverified. Task-003 remains
Blocked; the SDK access blocker is unchanged and no framework/toolchain compatibility
selection or host build is claimed. No private machine identity or full installer
record is committed.

Inventory evidence source commit: `cd0597dc1e061019c96893f61f22f2ff354edfac`. The predefined
reviewer returned PASS for this partial documentation increment; scope and
whitespace checks passed. No task acceptance or phase GO follows.

## Operator-supplied command results, 2026-10-21

The operator subsequently supplied PowerShell command output:

- `[Environment]::Is64BitOperatingSystem` returned `True`. This confirms a
  64-bit OS; it does not distinguish x64 from ARM64.
- Executing the installed Build Tools MSBuild executable with `-version -nologo`
  returned `18.9.1.35102`. This is the executable's reported version, distinct
  from the previously recorded installer version `18.9.12120.119`.
- Listing the conventional .NET Framework reference-assembly root returned
  `PathNotFound`. That root is absent at the queried location; neither framework
  runtime absence nor absence of every possible alternate reference source follows.

These operator-run results supersede the corresponding unknowns above. They are
not coordinator-run commands or a host compilation. No package was installed.
Task-003 remains Blocked on usable verified targeting references, SDK provisioning
and the required Windows host compile/test.

Public prerequisite references were rechecked on 2026-10-21:
[KNX SDK requirements](https://support.knx.org/hc/en-us/articles/7588671243410-Requirements)
describe a .NET Framework 4.8 SDK compilation target;
[Microsoft developer installation guidance](https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers)
and the [4.8 download page](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
distinguish build-time Developer Packs from runtimes. A 4.8 Developer Pack is a
candidate prerequisite provisioning step, not a verified DCA target selection,
SDK substitute, or demonstrated compatibility result. A later installation must
be observed and verified before its references are claimed available.

Command-result evidence source: `8dd077a244f1f98b5aecb53524fdcb1ea5eff033`; predefined reviewer PASS for
this partial increment. Scope and whitespace checks PASS; full task remains Blocked.

## Direct SSH and framework smoke increment, 2026-10-21

The operator enabled an authenticated SSH master. Coordinator-run PowerShell now
confirms AMD64/64-bit execution, MSBuild `18.9.1.35102`, and .NET Framework 4.8.1
registry Release `533509`. This supersedes earlier CLI access/inventory blockers;
RDP remains a separate requirement for later interactive vendor UI.

Five exact Microsoft .NET Framework 4.8 reference assemblies from public package
`Microsoft.NETFramework.ReferenceAssemblies.net48` 1.0.3 were hash-verified and
privately staged without system installation. The original tiny MSBuild/C# smoke
compiled and ran on Windows: net48/x64, STA, three WPF type references, no UI or
vendor reference. The predefined verifier independently reproduced the success
and five expected preflight failures. The predefined reviewer returned PASS for
the three source files, no P1/P2 findings. Full versions, package hashes, source
references, commands and boundaries are in [POC-TOOLCHAINS.md](../docs/POC-TOOLCHAINS.md).

This framework-only proof is partial evidence for TEST-002, not its required
SDK-qualified host proof. The Task-140 SDK version mismatch, lawful provisioning
and ETS-free dependencies remain unresolved; Task-005 remains Blocked.

The Windows Rust prerequisites (MSVC and Windows SDK) are present. A private,
pinned Rust 1.99.1 provisioning attempt failed during extraction with insufficient
space (OS error 112); only about 69 MiB remained. No active compiler was available
after rollback, so Windows Rust build/test is NOT RUN. The user was asked to free
space or enlarge the VM disk; no unrelated data is deleted. Linux remains NOT RUN
because no runner is available. Task-003 remains **Blocked**.

| Increment check | Result |
|---|---|
| Actual SSH inventory | PASS; version/architecture/reference availability observed directly |
| Owned framework compiler smoke | PASS; Windows compile and executable run, independently reproduced |
| Negative preflight cases | PASS: 5 expected failures, output not created or overwritten |
| Rust bootstrap SHA-256 / native prerequisite inventory | PASS; official bootstrap hash and actual MSVC/Windows SDK files |
| Rust Windows provisioning | FAIL, insufficient disk space; compiler build/test NOT RUN |
| Original DCA / hardware | NOT RUN; no operation on ETS projects or KNX devices |
| Source baseline | `2667b65412a7c39450c3b53ea9cbe6029467f8e3`; reviewed increment `f267998c58db182dccebe8f2c16b9f8a08fc02dd` |
| Final evidence review | Predefined sequential_review PASS, no findings; no Task-003 acceptance or phase GO claimed |

Source validation commands actually run:

```sh
python3 scripts/check_project_scope.py
git diff --check
```

Windows smoke/negative commands and public prerequisite sources are recorded in
the linked toolchain evidence. The work does not introduce an editor .NET runtime
dependency or a KNX implementation.

Cleanup/access limitation: the incomplete own Rust installation was not removed.
The first cleanup preflight failed closed on its proxy links; subsequent bounded
inspection identified only owned relative executable proxies, but the SSH master
ended before revised cleanup could connect. SSH access must be restored after
freeing disk space. No cleanup success, Rust compiler installation or Windows
Rust build is asserted. Successful C# evidence was already captured locally.


## Resumed Windows toolchain completion assessment, 2026-10-21

The user enlarged the Windows disk and restored the SSH master. The coordinator
observed more than 31 GiB available, completed cleanup of only the prior owned
incomplete Rust installation, and successfully provisioned the exact pinned Rust
1.99.1 MSVC toolchain with rustfmt and Clippy into private task directories.
Windows fmt, Clippy, test target (0 unit tests), executable run, metadata and
feature-tree commands all exited 0. The actual marker is
`toolchain-smoke offline=true protocol=none`. Metadata confirms no dependencies
or enabled features. The five copied owned source/configuration/license files
matched repository hashes. Full commands, versions and evidence hashes are in
[POC-TOOLCHAINS.md](../docs/POC-TOOLCHAINS.md).

The predefined reviewer re-read the original task contract and corrected its
earlier interpretation: Task-003 requires a minimal Windows host-toolchain test
with pinned references after SDK inventory, not an SDK-qualified original-DCA
host. Task-140 supplies that inventory, and the independently reproduced pinned
net48/x64/STA/WPF-reference smoke satisfies TEST-002 for compiler readiness.
Original hosting and provisioning are explicitly separated in Tasks 007 and 005;
Task-007 still depends on Task-005. No acceptance criterion has been weakened or
removed. The previous SDK-qualified-host blocker notes remain historical evidence
of that overly strong interpretation, not an added requirement of this task.

The fresh independent Windows Rust reproduction completed successfully.
Final evidence review passed; status is Done for toolchain readiness. Linux remains NOT RUN under DR-004 because no runner was
supplied. No original DCA, device or protocol operation occurred. Task-005 stays
Blocked, and no phase gate or vendor-runtime compatibility is accepted here.


## Current completion record

| Field | Observed result |
|---|---|
| Status | Done; selected toolchain checks independently verified and final evidence review PASS |
| Implementation revisions | Rust smoke `faa1befc1f73c3e6880c969fe5cd4b149a2aba47`; framework smoke `f267998c58db182dccebe8f2c16b9f8a08fc02dd`; fork tooling `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e` |
| Current source baseline | `5a7f3f5af06388bef10a30f7efc189f7269185ac`; reviewed final evidence `a764f260a66db52d19b921c9beab0ba94665e00f` |
| TEST-001 | PASS on macOS and Windows: pinned Rust build and actual offline executable run |
| TEST-002 | PASS: owned net48/x64/STA/WPF reference smoke on Windows with five exact Microsoft references and pinned MSBuild; independently reproduced |
| TEST-003 | PASS: default features disabled; actual metadata sole node, no features/dependencies/deps; no Secure |
| Windows reproduction | Verifier fresh target PASS, source hashes unchanged; all 10 version/check commands completed; initial hung harness attempt stopped and not counted as PASS |
| Cleanup / capacity | PASS: own incomplete installation removed; expanded disk and private pinned provisioning observed directly |
| Rust test coverage | 0 unit tests; actual executable run is the build proof, no device/behavior claim |
| AC-001 / AC-002 / AC-003 | Met for selected tooling experiments: reproducible commands, Rust independent of .NET/ETS, versions/references/lockfiles pinned |
| Linux | NOT RUN, no runner supplied; recorded under DR-004 |
| Fork inherited library tests | NOT RUN; Task-015 audits their features and side effects before execution |
| Original DCA / rights / hardware | NOT RUN / UNRESOLVED / NOT RUN; Task-005 and original-host prerequisites remain separate |
| Phase gate | None advanced; ordinary toolchain readiness only |
| Next ready task | Task-006; independent Task-015 and Task-023 may begin once this record is accepted; Task-005 research continues separately |

Checks actually run after this evidence update:

```sh
python3 scripts/check_project_scope.py
git diff --check
```

The predefined sequential_verifier completed the new Windows Rust proof. The
predefined sequential_review supplied the contract assessment; final diff review
and the exact evidence commit are recorded after completion. Earlier failures and
blocked interpretations above are historical and superseded by this record.
