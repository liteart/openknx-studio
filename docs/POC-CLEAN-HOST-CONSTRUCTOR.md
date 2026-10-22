# Clean Windows constructor experiment contract

Task-148 is a reference-specific research probe, not a production DCA host.
Accepted Task-143 supplies eight exact input pins and app-local binding policy;
Task-144 supplies the original entrypoint and an unchanged owned STA/WPF shell.
No SDK implementation or device/project context is supplied. No original
Initialize, GetAddInUI, GetDcaUi or Dispose method is called.

## Readiness and execution boundary

The new Windows x64 VM is inventoried separately by the coordinator. The original
readiness requirement was verified host-enforced isolation and no bus passthrough,
reviewed frozen source, and owned tests. The operator reported Proxmox OUT DROP,
active VM/global firewall and no USB/PCI KNX passthrough. A controlled TCP egress
test connected to its selected listener; another after reported firewall
activation also connected. Isolation is FAIL despite those configuration reports.
The machine is observably network-capable. SSH access is not isolation.

On 2026-10-22, after the second observed connection, the owner explicitly directed
testing on this machine despite failed isolation. The coordinator records a
limited exception for loading the exact pinned original inputs and invoking the
constructor once, in a bounded non-elevated interactive process. This replaces
the isolation prerequisite only for that experiment. It grants no intentional
KNX/device operation, device fixture, Initialize, UI, Dispose or other original
entrypoint call. Owned interactive tests and pre-original critical review
PASSed before the coordinator ran the constructor once. That actual call
returned successfully. No isolation, mediation, offline-operation,
safety PASS or product/phase GO is claimed. Task-005's technical qualification
does not complete automatically.

The probe's `hardware=NOT_RUN` markers mean that no intentional hardware
operation/test is supplied by this experiment. They do not prove that original
constructor code cannot attempt network/device effects. UI/state qualification
also remains NOT RUN; no additional original lifecycle method is invoked.

No Windows feature/firewall/install changes are part of this task. The owner's
assumed-private-use decision applies; proprietary DLLs remain unchanged and
excluded from Git/distribution.

The launcher enforces a 1–30 second direct-child deadline and a 65,536-byte
combined stdout/stderr capture bound while reading raw chunks. It kills and
waits for the exact direct child after timeout/overflow and reports failure if
exit is unconfirmed. It does not contain descendants, prevent file/network
access, or establish a security sandbox. An external interactive task deadline
is additional recovery, not a replacement for launcher evidence.

The probe records the actual process administrator-role check; an Interactive /
Limited task declaration alone is not proof of the resulting token. The probe
runs only x64 Windows, interactive nonzero session, STA, with an owned
WPF dispatcher. It verifies all selected file hashes before loading any original
assembly. Original input count is exactly eight with fixed names and hashes
from Task-143. Each file is bounded to 16 MiB and the stage is a regular local
directory without reparse ancestors. Fresh logs/output use exclusive creation.
Path validation reads each component's attributes directly, rejects a reparse
point, and only then descends. It does not first call Exists or Test-Path on that
component. Only a specifically missing final output component is permitted.
The owned junction sentinel test checks rejection and an unchanged target; it
does not establish that no target lookup occurred or prove race-free containment.
Exception chains, diagnostic strings, assembly count and probe log size are
bounded. Unknown dependencies fail; there is no resolver search, fetch or stub.

The CLR can resolve assemblies before AssemblyResolve is raised. Actual loaded
assembly identities and locations are audited; framework GAC/runtime assemblies
with recognized Microsoft identities are distinguished from exact staged inputs.
Unexpected proprietary loads are a blocker. Detection after a CLR load is not
retroactive prevention. Host isolation has failed; execution on this network-
capable machine relies on the explicit narrow owner exception above, not a
containment claim. There is no generic
GAC scan/load or Falcon allowance. Constructor return proves only that call.

## Compiler and references

This task explicitly selects the new host's built-in Framework64 `csc.exe`,
file version `4.8.9221.0 built by: NET481REL1LAST_25H2`, SHA-256
`46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`.
The coordinator inventories the actual version/hash before use. Compile with
`/noconfig /nostdlib /langversion:5 /platform:x64 /warnaserror+`, the same five
public reference pins as Task-144, and System.Xaml from that same package.
The actual Windows Framework compiler failed CS0012 without the latter,
requiring `System.Windows.Markup.IQueryAmbient`. The coordinator authorized this
sixth public reference after the observed failure. System.Xaml is 111,152 bytes,
SHA-256 `f88a02dcc1dc1037884032624f3ab092aa5a3e85d215598d7d13ad7ddc814d81`.
Task-144's historical five-reference build is unchanged. The earlier modern
Roslyn static check with five references did not prove the Windows build.
Microsoft's privately cached net48 reference
package 1.0.3 has SHA-256
`8a7e348538e7eb91351696911689f49e3d4f63f8bab517432bbe159b8b1104a2`.
No package restore, Build Tools installation or vendor compile reference occurs.
The unchanged Task-144 neutral source is built by this new explicit route;
Task-144's original compiler script and acceptance are unchanged.

## Planned commands and evidence

Commands below are reproduction instructions, not claimed outcomes. All paths
are private local Windows paths. Build output must not exist; references and
source are separate from writable run evidence.

The new host's initial default script policy rejected the owned driver before
the compiler ran. The coordinator then used `powershell.exe -NoLogo -NoProfile
-NonInteractive -ExecutionPolicy Bypass -File ...` for this owned invocation.
That flag is process-only; no Set-ExecutionPolicy or machine policy change was
made, and no vendor signature or strong-name check is disabled.

```powershell
& "$Source\Build-ConstructorProbe.ps1" -Compiler $Framework64Compiler `
  -ReferenceDirectory $Net48References -OutputDirectory $NewBuild
& "$Source\Test-ConstructorProbe.ps1" -BuildDirectory $NewBuild `
  -OutputDirectory $NewSession0Evidence -ExpectedSession Session0
# Coordinator launches this command in the active user's Limited / Interactive
# desktop using a unique temporary scheduled task with an outer 90-second limit:
& "$Source\Test-ConstructorProbe.ps1" -BuildDirectory $NewBuild `
  -OutputDirectory $NewInteractiveEvidence -ExpectedSession Interactive
```

The test driver never stages or executes original inputs. After reviewed frozen
source and owned tests pass, the coordinator may use this separate command under
the owner exception in the non-elevated interactive desktop on the network-
capable VM (never directly in SSH session 0):

```powershell
& "$NewBuild\bin\ProbeLauncher.exe" --seconds 15 --mode original `
  --input $PrivateExactEightInputStage --log $FreshOriginalLog
```

The stage contains only the eight fixed Task-143 DLLs; the app-local config is
built beside ConstructorProbe.exe. This normalized command was executed through
the reviewed private guarded runner. Record executable,
source, config, input and private log hashes, OS/CLR/compiler/session identity,
exit/cleanup evidence and the exact reviewed source commit. PASS owned fixtures
never implies vendor PASS. Original UI, state, mediation and hardware are NOT RUN.

## Observed outcomes

Local preparation checks PASS: PowerShell scripts parse, all four owned build
targets compile as C#5 against the five pinned public net48 reference files using
the local public Roslyn API. After the Windows CS0012 finding, the public archive
and new System.Xaml pin were verified and all four targets were statically
recompiled with six references: PASS. PowerShell parsing and scope/whitespace
checks also pass. This is static-only
macOS compilation; it does not validate the selected Windows compiler or execute
any result. The actual initial Windows compiler build FAILed CS0012 as recorded
above, after producing only the fixture DLL. Fresh amended six-reference Windows
build02 PASS. The session-0 owned suite PASSed six cases: UNC log, drive-relative
log, excessive deadline, existing log, probe noninteractive refusal and neutral
noninteractive refusal. The actual SSH token reports administrator-role=True;
the genuine Interactive/Limited run subsequently PASSed twelve cases in session
2 with administrator-role=False. The neutral shell opened, rendered and closed.
The hanging fixture returned timeout 124, and the flood fixture returned 125;
both had confirmed direct-child exit. The exact temporary task was removed and
no process for any of the three owned executable paths remained.

Both controlled egress connections remain FAIL for isolation. The explicit
2026-10-22 owner exception permits only the bounded constructor experiment after
those prerequisites were satisfied before the actual original run. Reviewed
implementation checkpoint `1daec2717b16625452eae8020c54e4eaa35385b6` was committed
before execution. Final runtime evidence verification and critical review PASS;
no gate approval is claimed. Coordinator evidence stays private under
`private/evidence/task148-clean-host`.

Independent verifier `constructor_verify` checked all 48 evidence-manifest hashes
and the actual Windows logs: PASS. This was evidence verification, not its own
Windows execution. `binding_review` returned pre-original critical review PASS
for frozen source, actual 48-artifact evidence and the private guarded runner,
with no findings. It does not approve safety, isolation or a phase gate.
The reviewed `original01-runner.ps1` has SHA-256
`b0721968ea470ae1c0ffebc2626f6dfcf78f7e499ef9a85273588a9eb7052f7f`;
it verifies the actual non-elevated interactive session and three runtime artifact
pins before launch. The private eight-input inventory is
`private-input-manifest.json`. Those checks preceded original staging. The five
owned source hashes remain unchanged from the reviewed committed checkpoint.


## Actual original constructor result

On 2026-10-22 at 17:12 UTC the coordinator executed the guarded original probe
in actual session 2, interactive=True and administrator-role=False. All eight
fixed Task-143 inputs matched their SHA-256 pins before original loading. The
log records `CONSTRUCTOR_BEGIN` for `Mdt.Dali.DcaApp.DcaAppMain`, token
`0600000E`, then `ORIGINAL_CONSTRUCTOR_RETURN` and `PROBE_RESULT ... exit=0`.
The launcher recorded `reason=EXIT child-exit=0 child-exit-confirmed=true`,
zero captured stdout/stderr bytes and a configured 15-second deadline. The
scheduled task's last result was 0. Technical result: PASS for this one call.

Only three private assemblies actually loaded: `MDT-DaliDcaPlus` 4.0.0.0,
`Knx.Ets.Sdk` 6.4.8718.0 and `Knx.Ets.Sdk.AddIns.AddInViews` 6.4.8718.0, each
from the exact private input stage. The other five files were staged and pinned;
this run does not establish that they were exercised. All other recorded
assemblies were expected Microsoft framework assemblies. No unexpected assembly
or dependency-rejection marker was recorded. No Initialize, GetAddInUI, GetDcaUi,
Dispose, state operation or intentional device operation was supplied.

This proves constructor return on this selected machine and exact inputs. It
does not establish full DCA initialization, UI hosting, state round trips,
activation/dependency closure for other methods, mediation, isolation or PoC A
success. Both isolation checks remain FAIL under the explicit owner exception.
Task-005 and Tasks 007–012 remain unchanged. Task-148 is Done for the bounded
constructor experiment after independent evidence verification and critical
review PASS.

Private result identities observed locally from the collected files:

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| original01-result-stdout.txt | 6,261 | `7dcbb8c1e2adb1900ec7628f36e1f82ec4de34138aaef7ddd0e282db09232245` |
| original01.log | 5,049 | `a4212bb8f14ee48b3f7106350ca999df90f76dc2255642b6447b33ee13acb424` |
| original01-driver.stdout.txt | 212 | `ada32b48295e26761fc944d0430c1681d8d20972d2b60fb68a6b1de0942184a2` |
| original-evidence-manifest.json | 734 | `f5ca99b07da207f843650acc41ae55908914deb335a6309675a6912306aac5fd` |
| private-input-manifest.json | 1,219 | `208f4751a3308dcb34b4c795d62e6e676fcccc74a2411c7494a6ebb412d0be8a` |
| original01-cleanup-stdout.txt | 2,463 | `489176312d0ccc3cd7f9a17b0af84e9177f6b998cb10c8c8fe93f481bc82cf45` |

Post-run integrity/cleanup PASS: all eight input byte counts and hashes match
the exact pre-run private manifest (7,955,816 aggregate bytes). All five raw
original evidence files match their evidence manifest. The exact scheduled task
was removed; zero processes matched the owned executable paths and no selected
ETS process names were present. Descendants remain explicitly NOT CONTAINED.
These are the recorded bounded checks, not proof of network isolation or absence
of every possible process/effect.

No Windows execution was performed by the documentation agent. It inspected the
actual private result/log bytes, verified all five raw evidence manifest entries,
compared all eight pre/post input records and computed the hashes above.
Independent verifier `constructor_verify` matched the five raw original files,
one-call markers, source and pre/post input hashes, result envelope and cleanup:
PASS. This was evidence verification without another Windows invocation.
Critical reviewer `binding_review` independently checked the actual original
evidence and returned final PASS with no unresolved findings. AC-001 through
AC-005 are met for this bounded experiment; isolation remains FAIL and no
broader DCA capability or phase gate is accepted.
