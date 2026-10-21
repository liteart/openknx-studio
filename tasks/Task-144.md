# Task-144: Inspect original DCA entrypoints and exercise an owned STA shell

## Metadata and readiness

| Field | Value |
|---|---|
| Task ID | Task-144 |
| Status | Done |
| Component | Bounded DCA compatibility research; no product gate unlocked |
| Baseline | Accepted Task-143 `438455a685d07cff1b71584cede8260d72028217` |
| Requirements | R-003, R-011, R-023, R-028, R-030 |
| Owner | Coordinator with DCA implementation and independent verification/review |

Read [DCA-CONTRACT](../docs/DCA-CONTRACT.md). The coordinator approved this
independent investigation after the user's request for a Windows integration
test and continued SSH use. Accepted Tasks 006, 142 and 143 supply package,
metadata and binding evidence. Task-005's owner decision (`5c58448`, `629575f`)
authorizes assumed private local use; it remains technically Blocked. No phase
gate is required or advanced. This task does not waive Task-005 or make Tasks
007–012 ready. Original activation needs an isolated Windows environment that
has not yet been provided; this increment completes the independent metadata
and owned-shell observations first.

## Scope and acceptance criteria

- [x] AC-001: Report actual original public entrypoints and selected AddInViews
  interfaces from hash-pinned metadata, without loading original assemblies,
  instantiating attributes or reading method bodies. Preserve full referenced
  assembly identity and the raw method signature header.
- [x] AC-002: Exercise an owned net48/x64 WPF shell's STA open/render/close
  lifecycle and explicit session-0 failure on Windows. The shell has no SDK,
  vendor assembly loader, domain state, bridge, network or KNX implementation.
- [x] AC-003: Record commands, source/input/evidence identities, limitations,
  independent verification/review and exact reviewed source commit.

Original vendor construction, initialization, UI, device context, state round
trips and bus access are outside this bounded increment and remain NOT RUN.
A manual owned button click is optional diagnostic interaction, not an
acceptance prerequisite. It must not be claimed from a rendered button.

## Owned implementation and bounds

`experiments/dca-integration/` contains a metadata reader, synthetic metadata
fixtures, a neutral WPF program and its explicit-reference build script. The
existing research wrapper adds only the fixed `entrypoint-map` choice. The old
`reference-map` and `member-map` choices, input pins and denial policy are
unchanged. Real metadata input must use that wrapper; no arbitrary worker path
is accepted. Both actual write-denial and socket-denial probes must pass before
metadata reading. The child has the existing 30-second deadline, 2 MiB combined
output cap and process-group cancellation. This research restriction does not
establish a production sandbox.

All input hashes are checked before any PEReader is constructed. Limits are
three inputs, 16 MiB/file, 32 MiB aggregate, 32,768 rows/table, 1,024-character
names, scope/signature depth 16, 128 signature bytes/nodes, 32 arguments/generic
arguments, 1,024 reported types, 2,048 reported methods and 512 KiB JSON. A
bounded grammar validates counts before the public SignatureDecoder can allocate
argument arrays. Unsupported shapes fail explicitly. The tool inventories
public types and reports method details only for direct AddInViews implementers
and five selected contract declarations. It does not resolve inherited members,
forwarders or dependencies. Public System.Reflection.Metadata APIs and the
accepted Task-142 primary-source grammar evidence are used; no proprietary SDK
implementation is reconstructed or copied.

The neutral shell rejects UNC and drive-relative log text before filesystem
access, checks a local drive and every existing component from root to leaf for
reparse points, and creates a new log exclusively. It refuses session 0 or a
noninteractive environment. UI callbacks assert the same STA thread. A dispatcher
timer closes the owned window after 1–60 seconds. Original code never executes.
The private launch used one temporary scheduled task with Interactive logon,
Limited run level and a 90-second execution limit; the exact task was removed
after process exit. No Windows feature, firewall, ETS installation or service was
changed. The existing machine has ETS installed and is not a clean independent
runtime environment.

## Observed entrypoints

The unchanged primary DLL is 452,608 bytes, SHA-256
`4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d`.
Input SDK/AddInViews pins are the accepted Task-140/143 pins. The actual class is
`Mdt.Dali.DcaApp.DcaAppMain` (TypeDef `02000004`), implementing
`IDeviceConfigurationAddIn`, `IEts4AddInV2`, `IEts4AddIn` and `IDisposable`.

| Original declared method | Token | Metadata signature |
|---|---|---|
| Constructor | `0600000E` | Public instance, no parameters |
| `Initialize` | `0600001A` | `void (IInitializationContext)` |
| `GetAddInUI` | `06000017` | `FrameworkElement ()` |
| `GetDcaUi` | `0600001E` | `FrameworkElement (IEnumerable<Device>)` |
| `Dispose` | `06000024` | `void ()` |

The candidate `IInitializationContext` declares Project, Resources
(`IEtsResources`), Configuration/UserConfiguration (`XmlDocument`),
HostNotification and DialogService getters. `IHostNotification` declares
SelectionChanged, SetStatusbarText, ShowAppHelp, SuggestContentSelectorWidth and
GetToolbarItem. These are observations, not supplied services. Concrete SDK
Project/Device requirements and direct DeviceManagement references remain
important barriers to a safe independent host. A parameterless constructor or
matching signature never establishes successful initialization.

## Commands and results

Actual local commands from the task worktree:

```sh
python3 -m unittest discover -s experiments/dca-metadata -p test_run_probe.py
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1
python3 experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment entrypoint-map --output "$PRIVATE_STAGE/entrypoint-report-task144-identity.json"
python3 scripts/check_project_scope.py
git diff --check
```

`PWSH` is the provisioned PowerShell 7.6.7 executable; public metadata runtime
versions are those observed in Task-142. `PRIVATE_STAGE` is the accepted
owner-only stage outside Git. The default nested sandbox could not establish
its denial probes and failed closed. The authorized outer invocation retained
the identical restrictive child policy and both probes passed. An initial
128-type report cap failed; method output was narrowed to direct contract
implementers while retaining the bounded public-type inventory. The new wrapper
regression initially failed with the unknown-experiment diagnostic, then passed.
Malformed-signature testing exposed an unsupported-versus-truncated diagnostic;
truncation now has its own explicit failure.

| Check | Observed result |
|---|---|
| Wrapper regression suite | PASS, 13 tests |
| Owned entrypoint metadata suite | PASS, 12 checks; no executable fixture bodies |
| Real guarded metadata | PASS; 143 reported public/selected types, 42 methods, both denial probes PASS |
| Windows compilation | PASS; net48/x64, warnings as errors, no restore/fetch/vendor references |
| Invalid UNC/drive-relative log, excessive lifetime | PASS negative oracles, exit 2 |
| Owned local junction path | PASS negative oracle, exit 2; target unchanged and junction removed |
| Actual SSH session 0 | PASS negative oracle, exit 4; `interactive=False` |
| Interactive owned window | PASS final source independently: session 2, x64/STA, loaded/rendered, three-second close, exit 0; historical 60-second visual observation retained separately |
| Manual owned button click | NOT RUN: coordinator observed the window, but CUA input failed |
| Temporary launcher cleanup | PASS final independent run: exact task removed, exact owned executable process absent |
| Final root-first source interactive rerun | PASS independently: complete load/render/timer/close/shutdown markers and NEUTRAL_RESULT PASS |
| Original vendor execution/UI/state, hardware | NOT RUN |
| Clean ETS-free runtime, mediation, activation | NOT RUN |
| Independent verification | PASS: fresh local suites, byte-identical guarded report, fresh Windows build, negative cases, final-source interactive run and cleanup |
| Independent critical review | Pending final verdict/source-commit record |

Exact Windows build command, with private paths normalized:

```powershell
& "$Source\Build-NeutralHost.ps1" `
  -Compiler $InventoriedRoslynCompiler `
  -ReferenceDirectory $Task003PrivateNet48References `
  -OutputDirectory $NewOutput
& "$NewOutput\NeutralStaHost.exe" --seconds 1 --log $NewLocalLog
# Actual SSH session: expected exit 4 and NEUTRAL_INTERACTIVE_SESSION_REQUIRED.
```

Compiler file version is `5.900.26.35703`; Framework 4.8.1 Release `533509`,
PowerShell `5.1.26100.9168`, Windows 11 Pro x64 build 26200. The five net48
reference DLL hashes are embedded in Build-NeutralHost.ps1 and reproduce
Task-003. Only the two owned build/program files were transferred. Source hashes
were checked on Windows. The interactive command was the same executable with
`--seconds 60 --log $NewLocalLog`, launched using New-ScheduledTaskAction,
New-ScheduledTaskPrincipal `-LogonType Interactive -RunLevel Limited`,
New-ScheduledTaskSettingsSet `-ExecutionTimeLimit (New-TimeSpan -Seconds 90)`,
Register-ScheduledTask and Start-ScheduledTask. Get-ScheduledTaskInfo,
Get-CimInstance filtered to the exact owned executable path, and
Unregister-ScheduledTask confirmed exit/cleanup. Raw stdout/stderr remain
private; PowerShell progress CLIXML is preserved separately.

## Evidence identities and completion record

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Final private entrypoint-report-task144-identity.json | 52,492 | `8195260cb622508d32c11946ddcef84887d5f8284b9c9ea3ed5539aeac70b0c5` |
| Historical interactive result stdout | 720 | `7e0342d2c408cd770db5fe3124dd88b7fc0eb18eb2184683434dc77927e0d6a8` |
| Historical raw owned UI log | 494 | `93c31df09affde9795b9173b88135f64376a0975a717eaff625e5dd56500327d` |
| Final-source build and local negative-result stdout | 613 | `1b9b33e9bf83f8c071c8fdf2b56fb9c4b51972f87130ece7f4da26f29583dfb0` |
| Independent final-source three-second UI log | 494 | `639f27279fa461edc184565c04c7eae2a18eafe3558ef6028c3e1f1757adee9e` |

The historical neutral program hash was
`89602c8f4439bbaecf3c331fed03897b365d5f0d314c012a0da276f2062f5e3c`.
Review then required root-to-leaf ancestor validation before touching a child
path. The final source hash is
`7f3045efbc29efe73a743c47724df57c19fa3cc285556e62d850412baa70863c`.
The final build script hash is
`ba5b3167897afc37278894d3d92f86aa536c2ff2a7a9cd08de0a0926c52336d5`.
The final metadata source hash is
`96b5193d84be9f5b1680acdc49643de120c7f54eb2902ab3fe1886579c327123`.
Evidence stays private outside Git. No manufacturer DLL, generated executable,
SDK reference or source signing key is committed.

| Completion field | Result |
|---|---|
| Implementation/report status | Done for this bounded investigation; implementation, independent verification and critical review PASS |
| Source baseline | `438455a685d07cff1b71584cede8260d72028217` |
| Reviewed source commit | Not yet created |
| Technical outcome | PASS for bounded static metadata and final-source owned STA lifecycle; original vendor execution remains NOT RUN |
| Product/phase gate | No gate advanced; Task-005 and Tasks 007–012 unchanged |
| Remaining blocker | Original activation requires the isolated Windows environment and an exact discovered-service experiment |

The independent verifier reproduced 12 metadata checks, 13 wrapper tests and
both guarded denial probes. Its private metadata report matches the final
52,492-byte report byte-for-byte. It checked the frozen source hashes, compiled
a fresh executable, reproduced UNC/drive-relative exit 2 and actual SSH session-0
exit 4, then launched a three-second window using its own unique Limited /
Interactive task. The final-source log records session 2, x64/STA, load, render,
timer, close, application exit 0 and owned-only PASS. Its exact task was removed
and no process for its executable remained. This is genuine Windows owned-code
execution; manual button input and all original vendor behavior remain NOT RUN.

Independent critical review PASS for all eleven explicit paths against the
baseline above. The reviewer reran the 12 metadata checks, 13 wrapper tests and
scope/whitespace checks, and directly verified the matching metadata reports
and final Windows log hashes. No unresolved findings remain. Windows execution
was performed by the verifier; review inspected source and evidence. This
acceptance grants no phase, vendor-runtime or device-operation approval.

## Next environment and experiment

The current development VM remains useful for compilation, metadata and the
owned shell. Its disabled Sandbox/Hyper-V features and unavailable nested
virtualization do not provide the required isolated vendor-execution route.
The user identified Proxmox and said a second test VM is possible. A separate
normal Windows interactive VM can provide the next experiment environment:

- No installed/running ETS, ETS services or ETS project store; audit this before
  making an independent-runtime claim.
- Host-enforced bus/network isolation during original execution, with observable
  checks; no USB/bus adapter passthrough. File/process boundaries alone do not
  prove mediation.
- Supported x64 .NET Framework/WPF runtime and an interactive user desktop.
- Exact unchanged, hash-pinned original package, selected SDK 6.4 references and
  explicitly inventoried dependencies staged privately, outside Git/distribution.
- Owned source compiled with the accepted toolchain, narrowly scoped read-only
  vendor inputs and fresh writable scratch/log output, with a bounded child
  lifetime and genuine startup/shutdown evidence.

That follow-up should attempt only the discovered constructor/entrypoint and
stop at the first real missing dependency/service. It must not invent Project,
Device, activation, SDK or bus success, and cannot silently grow a general ETS
clone. Original initialization, actual UI, state round trips and mediated bus
operations remain separate claims and evidence requirements.
