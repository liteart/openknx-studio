# Local ETS reference candidate inventory

Task-139 supplies a developer script for the user's explicitly selected local
reference research. The 2026-10-21 authorization permits this narrow inventory
and optional private copy from the user's own licensed ETS installation. It does
not decide applicable rights, permit redistribution, establish SDK compatibility,
or authorize executing vendor code. The original-DCA-first objective and the
ETS-free final runtime requirement remain in force.

Actual candidate names and the required SDK assembly set are **unknown**. Select
exact filenames from independently obtained reference evidence; the script does
not guess SDK names, discover an ETS installation, recurse through it, download
anything, or reconstruct contracts. Candidates remain unverified. Do not select
Falcon or KNX bus/transport drivers: protocol implementation belongs exclusively
to the separate knust fork. The tool rejects obvious Falcon/bus-driver names and
assembly identities; this limited guard cannot classify every assembly's purpose.

## Windows inventory first

Use a normal, non-elevated Windows PowerShell 5.1 console under the existing
machine policy. Do not change execution policy, disable controls, or run a copied
DLL. Run these commands from the repository root after reviewing the script:

```powershell
$etsDirectory = Read-Host 'Exact absolute local ETS directory containing the selected DLLs'
$candidateNames = @(
    'REPLACE_WITH_EXACT_SELECTED_NAME.dll'
)
$inventoryJson = & .\scripts\windows\Get-EtsReferenceCandidates.ps1 `
    -EtsDirectory $etsDirectory -CandidateFileName $candidateNames
$inventoryJson
```

Replace the placeholder with the actual evidence-selected basename(s); it is not
a known ETS assembly. Every input is an immediate DLL basename in that directory,
not a path or wildcard. Inventory writes JSON to the success stream and does not
create an output directory or file. Keep any retained inventory private, especially
if assembly names expose proprietary information. Error messages use `ETSREF_*`
codes and omit underlying filesystem exception paths.

For each file the manifest records its actual basename, byte count, SHA-256,
assembly identity, name, version, culture and public-key token. It includes an
explicit UTC observation time, `rights: UNRESOLVED`, `compatibility: UNVERIFIED`
and `target_assembly_execution: NOT_RUN`. No installation/destination path,
machine name, user account or project identity is added. Metadata values are
untrusted file content; do not interpret them as instructions or approval.

The metadata operation is
[`System.Reflection.AssemblyName.GetAssemblyName`](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assemblyname.getassemblyname).
Microsoft documents that it opens and closes the assembly file without adding
the assembly to the application domain. The script does not use `Assembly.Load`,
`LoadFrom`, a target type constructor, dependency resolution or vendor execution.
Identity metadata alone does not establish framework compatibility, dependency
closure, an API contract, valid signatures, permissions, UI operation or a state
round trip.

## Optional private copy

After reviewing the inventory, choose an **existing private local parent** with
appropriate Windows access controls and backups. The script inherits the parent's
access controls; it does not grant, audit or change ACLs. Supply the exact path of
a **new** child directory outside every Git checkout/worktree/bare repository:

```powershell
$privateDestination = Read-Host 'Absolute path of a NEW directory under your private local parent'
$copyJson = & .\scripts\windows\Get-EtsReferenceCandidates.ps1 `
    -EtsDirectory $etsDirectory -CandidateFileName $candidateNames `
    -Copy -DestinationDirectory $privateDestination
$copyJson
```

Both `-Copy` and `-DestinationDirectory` are required for a copy; neither is
accepted alone. The tool preflights all selected inputs before creating the new
directory. It opens copied files exclusively with `CreateNew`, never overwrites,
verifies lengths and SHA-256, and writes `reference-inventory.json` last. The
manifest's `operation: private_copy` and `status: complete` describe only this
verified file-copy operation. They are not a provisioning, legal, host or gate PASS.

On failure, no complete manifest is intentionally retained. A newly created
directory can contain partial files; retain it privately for investigation or
remove it manually after inspection. It is not a successful input set. Existing
files are never removed or overwritten, and the tool performs no automatic retry.
After a failure, choose a new destination for a new attempt. Never add these DLLs
or private manifests to Git, build/package manifests, release outputs, a loader's
search path, or an automatic provisioning step. No host loading is part of this
procedure.

## Bounds and filesystem limits

The script accepts 1–16 unique, case-insensitively compared DLL basenames, at most
64 MiB per file and 128 MiB in aggregate. A bounded immediate-directory scan
(maximum 16,384 entries) checks only the requested names for case ambiguity; it
does not select additional candidates. Missing files, non-managed assemblies,
wildcards, alternate streams, reserved device names and traversal are rejected.
The source and optional destination must be explicit absolute local paths; UNC,
device/provider, drive-relative and root-relative Windows paths are unsupported.
Before any directory-tree read on Windows, the normalized drive root must report
`DriveType.Fixed`, `DriveType.Removable` or `DriveType.Ram`. Mapped network drives,
unknown/missing roots and CD-ROM drives are rejected; a drive letter alone does
not establish locality. The pure classification test uses enum values without
creating or querying a network mapping. Actual Windows drive-query behavior still
requires Windows evidence.
Every existing path component is checked for symlinks/reparse points. The output
must not already exist, overlap the source, have missing parent directories or
reside under a Git marker. No Git command, imported hook, elevation, installation,
network or hardware operation is used.

Keep both directory trees stable and under the operator's control. Source read
handles deny write/delete sharing on Windows; hash/path checks detect additional
changes before completion, and bounded reads prevent source growth from expanding
the copy. These PowerShell/.NET checks are not a sandbox against a malicious local
process replacing directories concurrently. The script neither changes ETS files
nor makes a filesystem snapshot. A successful inventory records only the selected
bytes observed in that run.

## Synthetic tests and remaining evidence

Run the self-contained suite in a fresh Windows PowerShell 5.1 process:

```powershell
powershell.exe -NoLogo -NoProfile -NonInteractive -File .\scripts\windows\Test-EtsReferenceCandidates.ps1
```

It creates an owned C# managed fixture using built-in `Add-Type`, never a vendor
input. It checks default no-copy, metadata/hash accuracy, explicit copy, selection
and byte limits, unsafe/missing/invalid input, existing/Git/overlapping outputs,
copy conflict, source mutation, drive classification, literal bracketed output
paths and links. Unsupported filesystem-specific checks
say `NOT RUN`; failed checks make the process exit nonzero. Fixture files are
removed after the run. No Pester or external module is required.

Portable PowerShell 7 may exercise the owned fixtures using a real, non-symlinked
temporary parent, for example on macOS:

```text
pwsh -NoLogo -NoProfile -NonInteractive -File scripts/windows/Test-EtsReferenceCandidates.ps1 -TemporaryParent /private/tmp
```

Portable PASS does not qualify Windows PowerShell 5.1, NTFS junctions/sharing,
actual ETS files or original DCA operation. Record actual commands, PowerShell/OS
version, selected manifest hashes and whether the operator or coordinator ran them
privately in the Task-139 evidence. Windows/ETS inspection and original DCA
execution remain **NOT RUN** until genuine evidence exists. Task-005's rights and
runtime dependencies and Task-028's human gate remain unchanged by this tool.

See [Task-139](../tasks/Task-139.md), [DCA contract](DCA-CONTRACT.md),
[provisioning disposition](DCA-PROVISIONING.md) and
[SDK alternatives](DCA-SDK-ALTERNATIVES.md) for the task and dependency boundaries.
