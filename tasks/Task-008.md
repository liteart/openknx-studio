# Task-008: Map the actual ETS SDK surface used by the reference DCA

## Subject

Map the actual ETS SDK surface used by the reference DCA

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-008 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host analysis |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Identify the minimum external services and identities needed by this DCA rather than implementing an imaginary generic ETS runtime.

## Detailed Requirements

- DR-001: Use permitted metadata analysis and controlled load diagnostics to list referenced assemblies, public interfaces, required concrete types, callbacks and UI entry points.
- DR-002: Record exact assembly versions/public identities and distinguish missing type from missing service/activation.
- DR-003: Classify each dependency as implementable bridge contract, allowed reference, unknown, or blocker.
- DR-004: Explicitly inspect storage, product/device context, network service and download-hook expectations.
- DR-005: Preserve analysis provenance and do not modify manufacturer binaries.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Every attempted initialization failure maps to a named dependency with evidence.
- [ ] AC-002: A small ordered shim plan exists for the single reference DCA.
- [ ] AC-003: Strong naming or proprietary concrete types are not dismissed as solved by interface naming.

## Dependencies to Other Tasks

- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.
- [Task-006](Task-006.md): Inspect the original MDT DCA package without executing it. Required predecessor behavior/evidence must be accepted.
- [Task-007](Task-007.md): Build a minimum Windows STA DCA host shell. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: reproduce the dependency map from the pinned package.
- TEST-002: Controlled Windows: capture the first real missing dependency failure.
- TEST-003: Review: confirm unknown entries are not represented as implemented services.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

### Selected initialization diagnostic contract (2026-10-22)

Task-005 is accepted at `ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`,
Task-006 at `685dfbaa` in its separate worktree, and Task-007 at
`3282a515d44aed7e9ccfc0bdd438811264827128`. No phase gate applies.
This increment extends the accepted constructor probe with an explicit
`original-initialize` mode: one unchanged constructor (`0600000E`) followed
by one `Initialize` (`0600001A`) on `Mdt.Dali.DcaApp.DcaAppMain` (`02000004`).
It retains the exact eight original input pins, binding configuration,
assembly audit, bounded log and direct-child supervisor. Historical
constructor-only modes retain their behavior and claims.

The actual reflected `IInitializationContext` is represented by a deny-all
`RealProxy`. Every request records its member identity and returns an explicit
`DCA_UNSUPPORTED_SERVICE` exception; it supplies no null/default service,
configuration, project, callback, or bus result. An owned interface fixture
must first prove all six getters fail explicitly and reflection preserves
the named exception on the pinned Windows net48 runtime. New initialization
modes refuse administrator and noninteractive execution. No UI entrypoint,
vendor `Dispose`, state mutation, or intentional KNX operation is called.

The first unsupported request is a diagnostic observation, not successful
initialization. A vendor normal return after catching a denied request must
record the request count and remain unqualified. See the ordered dependency
plan in [DCA-INITIALIZATION-MAP.md](../docs/DCA-INITIALIZATION-MAP.md).
The coordinator owns Windows execution and records its authorization and
actual isolation status separately; Task-148's exception is not inherited.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host analysis**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No universal ETS API coverage, binary patches, license emulation, or proprietary code copying.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | In progress; frozen-source Windows build and owned diagnostic tests PASS; original Initialize pending |
| Source commit(s) | Uncommitted Task-008 increment on accepted Task-007 base `3282a515d44aed7e9ccfc0bdd438811264827128` |
| Tests executed | Static guarded-report/code comparison PASS; `python3 -B scripts/check_project_scope.py` PASS; `git diff --check` PASS. Coordinator Windows build PASS; owned Session-0 suite 7 cases PASS; owned interactive suite 13 cases PASS, including six getter denials, direct/wrapped failure and caught-denial unqualified return. Original Initialize NOT RUN. Local Mono commands FAIL before managed execution with bad CPU type. |
| Evidence location | [DCA-INITIALIZATION-MAP.md](../docs/DCA-INITIALIZATION-MAP.md); private coordinator evidence `private/evidence/task008-initialization`, including `build-report.json`, `interactive01-result.json`, `owned-evidence-manifest.json` and cleanup evidence |
| Acceptance criteria met | AC-002 draft ordered plan present; AC-001 runtime evidence pending; AC-003 explicitly retains strong-name/concrete-type limits. No acceptance asserted. |
| Technical outcome | Owned net48/x64 diagnostic compatibility PASS on the selected Windows environment; original initialization dependency outcome NOT RUN |
| Gate outcome, when applicable | None; no phase gate advanced |
| Reviewer | Independent pre-execution source/evidence review PASS; owned evidence verification PASS. Final original-result review pending. |

### Implementer verification (2026-10-22)

The static comparison reread the guarded report after verifying SHA-256
`8195260cb622508d32c11946ddcef84887d5f8284b9c9ea3ed5539aeac70b0c5`.
It compared all six helper getter names, their sequential `0600002F`–`06000034`
tokens and return type names against the actual declarations, plus context
`0200000F`, no inherited interfaces, and original `Initialize` token/signature.
This is a report/source comparison, not another metadata extraction from the
original DLL and not runtime compatibility evidence.

Actual local commands included:

```sh
python3 -B scripts/check_project_scope.py
git diff --check
shasum -a 256 experiments/dca-constructor/{ConstructorProbe.cs,ProbeLauncher.cs,Build-ConstructorProbe.ps1,Test-ConstructorProbe.ps1,InitializeContextProbe.cs,OwnedInitializationFixtures.cs,OwnedConstructorFixtures.cs}
"$MonoExecutable" --version
"$MonoCscExecutable" -version
```

The first two commands passed. Hashes were sent to the coordinator before
transfer. The two Mono commands failed before executing managed code with
`bad CPU type in executable`; they provide no Windows test result.

### Coordinator Windows owned verification (2026-10-22)

The coordinator built the frozen source using the pinned Framework64 compiler,
version `4.8.9221.0 built by: NET481REL1LAST_25H2`, SHA-256
`46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`,
and the unchanged six public reference pins. `build01-stdout.txt` reports
`CONSTRUCTOR_BUILD PASS target=net48 language=CSharp5 process=x64
vendor=NOT_RUN hardware=NOT_RUN`. The actual build report source hashes
match all six compiled owned C# files in the worktree. The eight-entry
`source-freeze.json` additionally pins the build and test scripts.

`session0-01-stdout.txt` records seven PASS cases, including refusal of the
new initialization mode with exit 4. `interactive01-result.json` records
thirteen PASS cases, process exit 0, session 2, interactive true and
administrator false. The owned proxy log proves all six getter calls return
named `DCA_UNSUPPORTED_SERVICE` exceptions; direct and reflective callers
observe the required failure shapes. Its caught-denial fixture returns
normally but records `qualification=NOT_ESTABLISHED`, as required. The
interactive administrator-negative branch was NOT RUN in this limited-user
session. Timeout, flood, missing dependency, wrong hash and path refusal
regressions passed. Cleanup records zero owned processes and removal of the
temporary task; descendants remain explicitly `NOT_CONTAINED`.

Actual Windows commands, with private source/stage locations represented by
`$Source` and `$Stage` and the accepted six-reference directory by
`$AcceptedTask148SixPublicReferences`, were:

```powershell
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "$Source\Build-ConstructorProbe.ps1" -Compiler 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' -ReferenceDirectory $AcceptedTask148SixPublicReferences -OutputDirectory "$Stage\build01"
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "$Source\Test-ConstructorProbe.ps1" -BuildDirectory "$Stage\build01" -OutputDirectory "$Stage\session0-01" -ExpectedSession Session0
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "$Source\Test-ConstructorProbe.ps1" -BuildDirectory "$Stage\build01" -OutputDirectory "$Stage\interactive01" -ExpectedSession Interactive
```

The interactive invocation and actual private locations are preserved in
`interactive01-runner.ps1`.
This evidence was produced by the coordinator and reviewed locally; no
Windows or vendor execution was performed by the implementer.

Private evidence SHA-256 pins:

| Evidence | SHA-256 |
|---|---|
| `source-freeze.json` (eight source/script pins) | `b5886f6a17343b69f1ff6f7c04adf5b607c6f0c5969e4425b96bf6c4eeafb4de` |
| `build-report.json` | `889cd0b610513cc438a72309592ecba698fc96d13511efb40eb35502937bb7f2` |
| `owned-evidence-manifest.json` (53 files, all locally verified) | `a1d5857dd80d70ac43eb1e26b5ff58840cbda2f988cf482dd6980f5ffee13f57` |
| `interactive01-result.json` | `8793d59dfea3ba52fd1743786c565126acb3744decae5e983619c68fee633d36` |
| `owned-evidence/interactive01/initialize-proxy.log` | `e7dcfbf7b28c84f214b4d749918c5ef27042e17fb77d3ef7a4bc9621a8acf313` |
| `interactive01-cleanup-stdout.txt` | `9e07c11aa3ae182d44730ee44f8e62a6306d075e7b998a2d46bbf214eeb464a7` |

Original Initialize, original UI/state behavior, hardware and network
mediation remain NOT RUN or unqualified. The owned PASS results do not
satisfy TEST-002 or accept Task-008.

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).
