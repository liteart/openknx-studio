# Task-007: Build a minimum Windows STA DCA host shell

## Subject

Build a minimum Windows STA DCA host shell

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-007 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host (C#/.NET) |
| Owner | Unassigned |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Create only the Windows process/UI lifecycle needed to host an original WPF-based DCA.

## Detailed Requirements

- DR-001: Use the verified runtime/bitness requirements, an STA UI thread, and a controlled message loop.
- DR-002: Show a neutral test view before attempting vendor loading.
- DR-003: Add startup/shutdown diagnostics, exception reporting, and explicit operation timeouts without pretending a timeout killed a vendor action.
- DR-004: Run as a normal interactive user; no administrator or ETS service dependency.
- DR-005: Do not add bus, Git, JSON domain, or compiler implementations.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: The neutral host opens, renders and closes its test window on the ETS-free Windows VM.
- [x] AC-002: UI callbacks run on the x64 STA thread; the bounded launcher confirms child exit and cleanup evidence records no remaining owned host process.
- [x] AC-003: Session 0 is explicitly refused, and the shared Task-148 host-build pipeline diagnosed a missing public compile reference. The diagnostic points to `ConstructorProbe.cs`, not the neutral executable. Installed-CLR absence was not tested.

## Dependencies to Other Tasks

- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-005](Task-005.md): Resolve lawful DCA and SDK dependency provisioning. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows: open/close cycle and UI-thread assertion.
- TEST-002: Negative: noninteractive session and missing runtime/reference produce diagnostic failure.
- TEST-003: Static: verify no KNX network or product-domain implementation was added.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host (C#/.NET)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No original DCA behavior yet, no headless UI automation, no production IPC.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Done; accepted unchanged Task-144 neutral shell reused and qualified by accepted Task-148 evidence |
| Source commit(s) | Reused source `5b8e36ffed5c5f6f9fe8a5ec69eb771892168db6`; Task-148 reviewed source `1daec2717b16625452eae8020c54e4eaa35385b6`, evidence record `134ebb4243c349f7128e655f60274f6465f526b8`; Task-005 prerequisite accepted at `ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`; Task-003 accepted in base `546b126585541f77df0851419bfdbb7ea649970f` |
| Tests executed | TEST-001 PASS from reused genuine Windows evidence: interactive owned neutral shell x64/STA/session 2 loaded/rendered/closed, child exit 0. TEST-002 PASS for session-0 refusal (`NEUTRAL_INTERACTIVE_SESSION_REQUIRED`, exit 4) and a missing public compile-reference diagnostic in the shared host-build pipeline; the diagnostic is at `ConstructorProbe.cs(216,13)`, not in `NeutralStaHost.cs`. Runtime/CLR absence negative NOT RUN. TEST-003 PASS by accepted Task-144 static scope review. No Windows commands rerun for this record. Task-148 `Build-ConstructorProbe.ps1` six-reference build: PASS (`build02-stdout.txt`, build report SHA-256 `e73efced913cc33cb30342a3e537f9b7e4c92432bb38cfa4a6554fc598d1638e`). Earlier five-reference compiler attempt: FAIL with CS0012 naming missing public `System.Xaml` (`build01-process-policy-stdout.txt`); corrected six-reference build passed. Current worktree `python3 -B scripts/check_project_scope.py`: PASS; `git diff --check`: PASS. |
| Evidence location | Private, unchanged Task-148 evidence under `private/evidence/task148-clean-host`: `inventory-stdout.json`; `owned-evidence/interactive02/neutral.log` (SHA-256 `fdf8fafa26db4e826e3d9e7e9c640f38d1e4b9692eda8c00b319887f28c04103`); `owned-evidence/session0-02/session0-neutral.log` (SHA-256 `ed72e1ac0501013b2125f37d8fd6d18761356cb2d87e750458b877db62983321`); `owned-evidence/build02/build-report.json`; `interactive02-cleanup-stdout.txt`. Evidence manifest pins the private outputs. |
| Acceptance criteria met | AC-001 PASS; AC-002 PASS for owned UI thread and direct child cleanup; AC-003 PASS for session-0 refusal and missing public compile-reference diagnostic. Missing installed CLR was not exercised. |
| Technical outcome | PASS for the neutral shell lifecycle and explicit session/reference diagnostics on the selected ETS-free Windows VM. No claim about original DCA behavior, runtime absence, or general Windows compatibility. |
| Gate outcome, when applicable | No phase gate applies or was advanced. |
| Reviewer | Predefined `binding_review`: PASS, no findings; source/evidence hashes and scope/whitespace checks reproduced. Task-148 evidence remains accepted under its own review. |

### Evidence and reproduction notes (2026-10-22)

Readiness is satisfied by Task-003 Done in the assigned base and the latest
Task-005 acceptance at `ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`; older
historical Blocked snapshots in Task-005 are superseded by its dated
“Coordinator acceptance” section. The neutral source is unchanged from accepted
Task-144 (`experiments/dca-integration/NeutralStaHost.cs`, source commit
`5b8e36ffed5c5f6f9fe8a5ec69eb771892168db6`). Task-148 compiled that source in
its amended six-public-reference build and exercised it on Windows 11 Pro x64,
.NET Framework Release 533509. Its inventory reports the ETS install roots
absent. The recorded test session was interactive session 2, non-administrator;
the inventory itself was collected from session 0 with an elevated token, so
that inventory is not evidence of the test process's privilege level.

The accepted Task-148 reproduction commands were:

```powershell
& "$Source\Build-ConstructorProbe.ps1" -Compiler $Framework64Compiler `
  -ReferenceDirectory $Net48References -OutputDirectory $NewBuild
& "$Source\Test-ConstructorProbe.ps1" -BuildDirectory $NewBuild `
  -OutputDirectory $NewSession0Evidence -ExpectedSession Session0
& "$Source\Test-ConstructorProbe.ps1" -BuildDirectory $NewBuild `
  -OutputDirectory $NewInteractiveEvidence -ExpectedSession Interactive
```

The actual amended build reports `CONSTRUCTOR_BUILD PASS target=net48
language=CSharp5 process=x64 vendor=NOT_RUN hardware=NOT_RUN`. Its earlier
shared host-build pipeline emitted CS0012 at `ConstructorProbe.cs(216,13)`
because `System.Xaml, Version=4.0.0.0` was not referenced; this was not a
diagnostic from compiling `NeutralStaHost.cs` alone and did not test CLR absence.
Task-148 then added the evidenced public reference and built in a fresh output
directory. The shell log records `NEUTRAL_START process=x64 apartment=STA
session=2 interactive=True`, UI-thread PASS, rendered content, timer close,
application exit 0 and neutral result PASS. The Session-0 log records
`interactive=False` and `NEUTRAL_INTERACTIVE_SESSION_REQUIRED`; its launcher
confirmed child exit 4. Cleanup evidence records the exact temporary task
removed and zero matching owned processes. These are reused Task-148 results,
not executions by this Task-007 report author.

The shared-pipeline public-reference diagnostic satisfies the reference arm
of TEST-002; it does not establish a missing-CLR diagnostic. A negative run
with the CLR absent was deliberately NOT RUN because the CLR was not removed
or disabled. No original DCA code or hardware was executed for these
neutral-shell observations. Static scope evidence is inherited from
accepted Task-144 review; the neutral implementation contains no KNX or product
domain implementation. No phase gate, PoC A result, or original-DCA
compatibility claim follows.

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

Coordinator acceptance: all three criteria are evidenced in the precise scope
above. The coordinator independently rechecked both neutral logs and the
build-report hashes. Final critical review PASS allows this artifact to commit.
No executable was changed or Windows experiment repeated. Next ready task:
Task-008 actual initialization-dependency mapping.
