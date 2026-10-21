# Task-143: Probe explicit Windows DCA assembly binding policy

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-143 |
| Status | Done |
| Component | Original-DCA compatibility research; no product gate unlocked |
| Baseline | Accepted Task-142, revision `85c4712` |
| Requirements | R-003, R-011, R-023, R-028, R-030 |
| Owner | Coordinator with bounded DCA implementation and independent verification/review |

## Scope and readiness

Test the narrow version-binding concern left by Task-142: whether an explicit
app-local redirect can select a newer strong-named assembly, and whether the
original DCA's ReadProperty/WriteProperty MemberRefs can resolve against the
selected private ETS 6.4 SDK candidates. Read [DCA-CONTRACT](../docs/DCA-CONTRACT.md).
Task-142 is accepted Done. This independent technical experiment requires no
phase GO and grants none. Simon's 2026-10-21 assumed-private-use decision applies
(Task-005 owner decision revisions `5c58448`, `629575f`); no agent certifies rights.
Original binaries remain outside Git and distributions.

## Acceptance criteria

- [x] Owned strong-named executable fixture distinguishes the actual CLR
  no-redirect manifest mismatch from app-local redirect success on Windows.
- [x] Hash-pinned original IL-only assemblies are inspected only in reflection-only
  context, with requested identities, ApplyPolicy results and exact MemberRef
  resolution recorded; an incomplete experiment may conclude INCONCLUSIVE.
- [x] Exact commands, environment, private evidence hashes and independent
  verification/review are recorded; no vendor method/attribute/constructor is
  invoked and no runtime/UI/state/hardware success is inferred.

## Implementation boundary

Owned source: [BindingProbe.cs](../experiments/dca-binding-probe/BindingProbe.cs)
and [Test-BindingProbe.ps1](../experiments/dca-binding-probe/Test-BindingProbe.ps1).
The console uses .NET Framework reflection-only loading for original assemblies.
It does not execute their module initializers, constructors, methods or attributes.
Its only execution-context assembly load/invocation is the deliberately named
owned synthetic contract. Raw PE preflight rejects missing ILONLY, native entry
points and managed-native images before reflection loading. All selected hashes
are checked before the first reflection-only load; files are <=16 MiB.
The resolver applies app-local policy and requires exact selected identities at
fixed staged paths. Framework fallback permits only the trusted runtime core
identity; .NET reflection unifies primitive parameter types with that trusted
core. Every vendor declaring/parameter type remains reflection-only. Loaded
reflection-only assembly paths are asserted against the staged allowlist; no
original assembly may enter the execution context.

There is no vendor loading from installed ETS, arbitrary GAC probing, dependency
fetching, global binding change, GAC write, signature bypass, host implementation
or protocol code. Each driver child has a 20-second deadline. Captured output is
checked against 65,536 characters after completion; this is not a general memory
containment guarantee. Inputs are the exact five baseline or eight extended
hash-pinned files, not an open untrusted-upload service. Temporary stages and
private raw logs remain outside Git. Original bytes are unchanged.

## Completion record

Windows experiment completed on 2026-10-21; independent verification/review pending.
The final fixed eight-input reflection-only profile resolves both actual original
DCA MemberRefs. The earlier five-input profile remains reproducible and records
its missing-dependency blocker. No source commit is created before review.

| Check | Result |
|---|---|
| Owned strong-named 5.7 request, only 6.4 candidate, no redirect | PASS negative oracle: FileLoadException, manifest mismatch `0x80131040` |
| Owned app-local 5.7 to 6.4 redirect | PASS: `OWNED_RESULT owned-6.4.0.0` |
| Original reflection-only resolver, no policy | PASS negative oracle: `POLICY_IDENTITY_MISMATCH` for SDK `5.7.554.37315` |
| Original five-input profile, explicit SDK policy | INCONCLUSIVE: missing BCL dependency prevents resolving first member |
| Final eight-input profile | PASS for explicit-policy reflection-only member resolution |
| ReadProperty MemberRef `0x0A0001F6` | Resolves to SDK 6.4 MethodDef `0x06001C5D` (row 7261) |
| WriteProperty MemberRef `0x0A0001F7` | Resolves to SDK 6.4 MethodDef `0x06001C60` (row 7264) |
| Both PropType parameter types | `Knx.Ets.Common.Types.Enumerations.PropType`, Common.Types `6.4.8718.0` |
| New PropType enum metadata | PASS raw metadata: underlying `System.Byte`; constants inspected without static-field execution |
| Old PropType value/semantic comparison | NOT RUN: old 5.7 enum definition unavailable |
| Vendor execution, original UI, state round trip, hardware | NOT RUN |
| Scope checker and whitespace | PASS |

Version mismatch alone does not prevent these two member bindings under the
observed explicit policy and selected dependencies. This is not a full DCA
compatibility result. Reflection-only inspection never applies binding policy
automatically: the resolver deliberately calls `AppDomain.ApplyPolicy`, reports
requested and selected identities separately and loads only the exact staged
match. The owned fixture alone establishes actual execution-context CLR redirect
behavior. No original vendor operation is executed or simulated as successful.

### Exact policy and inputs

| Assembly | Requested version | Selected version |
|---|---|---|
| Knx.Ets.Sdk / AddInViews (configured; AddInViews not exercised) | `5.7.554.37315` | `6.4.8718.0` |
| Knx.Ets.Common.Types | `5.7.286.36952` | `6.4.8718.0` |
| System.Threading.Tasks.Extensions | `4.2.0.1` | `4.2.1.0` |

The first five input pins are embedded in the console and reproduce Task-140 plus
Task-005's Common.Types/Common.Resources inventory. The baseline uses only those
five inputs even if more files exist in the stage. Each extension was explicitly
authorized in response to an observed dependency, using the exact known installed
file and a fixed allowlist amendment. No generic resolver search was added.

| Extra staged file | Bytes | Identity | SHA-256 |
|---|---:|---|---|
| Microsoft.Bcl.AsyncInterfaces.dll | 26,376 | `Microsoft.Bcl.AsyncInterfaces, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51` | `1c95b1a776931ca1c4f2911b2ae1cc99eb78f547cd5feeaf5cdbdf10cedad0db` |
| netstandard.dll | 102,856 | `netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51` | `bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb` |
| System.Threading.Tasks.Extensions.dll | 27,944 | `System.Threading.Tasks.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51` | `692c324c02a922f890aaf5948c54ec9758f5ab7019b8e8ab169c106ddd3231f4` |

BCL and Tasks.Extensions are copied unchanged from their exact known ETS root
paths into the temporary stage. The runtime facade is copied from its exact known
GAC file. No ETS process executes and no original assembly loads from those
installation directories. Runtime independence from installed ETS is **not**
established; clean-machine dependency closure remains separate. All eight staged
files report CLR flags 9 (ILONLY plus strong-name flag); this does not verify
signatures or activation. No signature checks are disabled.

### Environment and reproduction

Windows x64; PowerShell `5.1.26100.9168`; CLR `4.0.30319.42000`; installed Framework
4.8.1 Release `533509` from accepted Task-003 inventory; installed Roslyn compiler
file version `5.900.26.35703`. Compile with `/noconfig /nostdlib /platform:x64`,
only the privately provisioned Microsoft net48 mscorlib reference: 2,731,056 bytes,
SHA-256 `6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9`.
Reference package/provisioning evidence is in Task-003 / POC-TOOLCHAINS. No install,
restore or network dependency resolution is performed.

Exact Windows commands with private paths normalized:

```powershell
. "$Source\Test-BindingProbe.ps1" `
  -Compiler $InstalledRoslynCompiler `
  -CoreReference "$References\mscorlib.dll" `
  -CoreReferenceSha256 '6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9' `
  -InputDirectory $PrivateInputs -OutputDirectory $NewOutput
# Exit 0: expected baseline negatives recognized and owned redirect PASS.
Run "$NewOutput\reflection-policy\BindingProbe.exe" `
  @('reflection-extended', $PrivateInputs) 0 'REFLECTION_RESULT PASS'
# Exit 0: both actual MemberRefs resolve; REFLECTION_RESULT PASS.
```

Dot-sourcing retains the driver's `Run` helper for the extended case, enforcing
its 20-second child timeout, termination and expected output marker. Earlier
direct extended invocations had only an outer SSH client timeout; they did not
establish remote process termination. The final bounded rerun is recorded below.

Local commands actually run:

```sh
python3 scripts/check_project_scope.py
git diff --check
```

Private evidence outside Git:

| Evidence | Bytes | SHA-256 |
|---|---:|---|
| task143-result04.txt, historical five-input baseline | 3,313 | `f7ce09c3201eab964995312d085608142935b07edb9e93699a9e9c5fafe1bf06` |
| task143-result05.txt, historical baseline plus seven-input blocker | 5,192 | `5775b30fe45698c6ca54558dcd1a097206160880c88db8f10fd08f6018282b58` |
| task143-result07.txt, final baseline plus eight-input success | 10,127 | `35429642b4c0196405fa8ab2ab541bd467b65efc68b02e44c278fba5da41f58d` |
| verifier-final-05af81c37414428baeb0eeb1a53fa8ac.txt, independently repeated bounded baseline and extension | 15,115 | `83cd7babdd44b0a0c0f32d92de8201360f857b5ef9e3c8e9c165486a53a84792` |

Final frozen source SHA-256: BindingProbe.cs
`5865cc3afda869a53095dd14e2a346cfc98fbb524881d44e9b34d37210cb221b`;
Test-BindingProbe.ps1
`e9dfc16c188d58c60040ae51c681b377f7791a85043a13babda0ad125f1c95dc`.
Source baseline `85c4712`; implementation commit pending independent review.

Independent verifier PASS: local and Windows source hashes match the frozen
values above. The final dot-sourced driver and `Run`-bounded extension completed
in about 15 seconds, exit 0, under a 60-second local SSH deadline. All baseline
oracles, both original MemberRef resolutions and the required extended PASS
marker were reproduced. Scope and whitespace checks PASS. Vendor execution,
original UI/state, clean-machine runtime qualification and hardware remain NOT RUN.

Independent critical review PASS: all five explicit source/documentation paths,
the frozen source pins and bounded verifier transcript were inspected. No
unresolved findings remain. The reviewer confirmed no tracked DLL, executable
or signing-key files and that the private transcript is ignored. Review does
not claim original vendor behavior, full closure, old enum semantics or phase GO.

Next technical work remains dependency/activation and clean-machine runtime
qualification in Task-005, followed by gated original hosting. Task-005, original
UI/state evidence, Rust mediation and all phase gates remain unchanged.
