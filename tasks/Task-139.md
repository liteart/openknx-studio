# Task-139: Inventory and privately copy selected local ETS reference candidates

## Subject

Provide a bounded user-run script for metadata-only inventory and optional private
copying of explicitly selected reference DLL candidates from the user's own ETS.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-139 |
| Status | Done |
| Stage / required gate | PoC dependency investigation; no product gate required or unlocked |
| Priority | P0 |
| Component / Git project | knx-editor / developer provisioning scripts |
| Owner | Primary coordinator with predefined dca, verifier and reviewer agents |
| Requirement references | R-003, R-011, R-023, R-028 |
| Evidence level | Synthetic file tests plus Windows evidence when available |

## Description and authorization

On 2026-10-21 the user selected the minimal original-DCA compatibility investigation
and explicitly authorized extracting selected DLLs from their own licensed ETS
installation, retaining only a user-run extraction script in the project. This
supersedes the earlier blanket rejection of local-installation copying for this
narrow private research step. It is not authorization to redistribute binaries,
load vendor code, bypass identities or declare license compliance. The original
DCA-first goal and ETS-free final runtime requirement remain unchanged.

This task supplies evidence to Task-005 and the later Task-008 contract map. It
is separate from the original-package inspector in Task-006 and Windows host
implementation. No actual SDK contract definitions or required DLL list are invented.

## Detailed Requirements

- DR-001: Default to metadata-only inventory of exact user-selected candidate filenames
  under an explicit local installation directory, with no recursive discovery or downloads.
- DR-002: Record filename, byte count, SHA-256, managed assembly identity/version/culture
  and public-key token using metadata inspection that does not execute target assemblies.
  Mark candidates and rights unresolved; file identity is not a compatibility proof.
- DR-003: Copy only on an explicit switch and exact DLL basename list, into a new
  private local destination outside Git checkouts. Never overwrite, edit ETS, install,
  activate, elevate, disable policies or load copied DLLs. No wildcard/folder copy.
- DR-004: Bound file count, per-file bytes and aggregate bytes. Reject path traversal,
  ambiguous names, symlinks/reparse paths, missing/invalid assemblies, overlaps and
  existing destinations before copying. Preserve byte equality and completion evidence;
  failed copies must never produce a successful complete manifest.
- DR-005: Output no machine/user/project identities or absolute source paths in the
  inventory manifest. Preserve SHA-256 and UTC evidence privately. No binaries enter
  Git, package manifests, release outputs, automatic search paths or runtime loaders.

## Non-functional Requirements

Follow [AGENTS.md](../AGENTS.md), [DCA-CONTRACT.md](../docs/DCA-CONTRACT.md) and
[DATA-MODEL.md](../docs/DATA-MODEL.md). This is development PowerShell tooling,
not product domain or protocol code. Use Windows PowerShell 5.1-compatible syntax
and no external module dependency. Cross-platform PowerShell may test owned
synthetic fixtures; that is not Windows/NTFS/ETS qualification.

## Acceptance Criteria

- [x] AC-001: Default inventory reports bounded metadata without copying or executing assemblies.
- [x] AC-002: Explicit copy preserves selected bytes/hashes privately and rejects unsafe paths,
  missing/invalid inputs and existing destinations without claiming completion.
- [x] AC-003: Focused positive/negative synthetic tests pass and the real Windows command,
  selected file identities and outcome are recorded without publishing vendor binaries.

## Dependencies to Other Tasks

- [Task-001](Task-001.md), [Task-002](Task-002.md), [Task-004](Task-004.md): accepted
  scope, repository/private-input policy and fixture inventory. All are Done.
- Explicit user authorization above. No SDK or Windows host build is needed to
  implement/test a file inventory script. Actual Windows execution needs reliable
  command access; unavailable access prevents full task acceptance.

Task-003 and Task-005 remain blocked on their own criteria; this task cannot
waive rights review, compatible runtime provisioning, host execution or Task-028 GO.

## Required Tests

- TEST-001: Owned synthetic managed assembly: inventory identity/hash and default no-copy.
- TEST-002: Explicit local copy: byte/hash equality and accurate private manifest.
- TEST-003: Negative: traversal, duplicate names, links/reparse points, limits,
  missing/non-managed DLL, Git-contained/existing output, overlap and copy failure.
- TEST-004: Windows PowerShell 5.1 execution against actual explicitly selected local
  ETS candidates; distinguish user-run/coordinator-run, copied/not-copied and unknowns.

## Deliverables

- `scripts/windows/Get-EtsReferenceCandidates.ps1`, focused synthetic tests and usage guide.
- Updated Task-005 research disposition; this task record with commands/revisions,
  actual evidence, reviewer result, platform limits and the next unmet prerequisite.

## Out of Scope

Host implementation, SDK emulation, full SDK reconstruction, vendor execution,
license decisions/circumvention, ETS project/device changes, network/hardware,
proprietary runtime drivers, publishing or redistribution. Copying references does
not demonstrate an independent DCA runtime.

## Risks and Blockers

Two candidate identities were supplied in the operator inventory below; required
dependency closure and applicable terms remain unknown. No
Windows/NTFS success may be inferred from portable tests. Installed ETS files may
be runtime implementations rather than adequate or permitted standalone contracts.
Stop the affected route if concrete services/activation/identity cannot be provided
without modifying vendor files or depending on ETS at runtime.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / task status | Done: operator Windows tests/copy supplied, ZIP/manifest independently matched, PowerShell 5.1 host observed, final reviewer PASS |
| Source baseline | `104d0e3`; reviewed implementation revision recorded after commit |
| Files | Script, self-contained synthetic suite, local-reference guide; task/index/traceability/private-binary policy and Task-005 cross-links |
| Platform | macOS 27 arm64; repository-private PowerShell 7.6.7; no global installation |
| Provisioning | Official PowerShell release `v7.6.7`, asset `powershell-7.6.7-osx-arm64.tar.gz`, 70,628,335 bytes; SHA-256 `50dde99bb066adacbb9a748182d09eb9beff25119dc3b12e85c84b6b6b1c5da9` matched official release asset metadata |
| Test command | `private/toolchains/powershell-7.6.7/pwsh -NoLogo -NoProfile -NonInteractive -File scripts/windows/Test-EtsReferenceCandidates.ps1 -TemporaryParent /private/tmp` |
| Implementation test result | Initial portable result PASS 17, FAIL 0, NOT RUN 2; after mapped-drive and bracket-path coverage, PASS 19, FAIL 0, NOT RUN 2. Owned managed fixtures only; initial UNC classification failure corrected |
| Open platform checks | Windows suite supplied: 19 PASS / 0 FAIL / 2 NOT RUN (symbolic-link creation unavailable; distinct case-colliding files unsupported). Windows junction rejection PASS. Portable symbolic-link tests PASS. Unsupported filesystem cases remain explicit NOT RUN |
| Actual ETS inspection / copy / DCA execution | Operator inventory and private-copy reports PASS; coordinator ZIP byte/manifest checks PASS. Target assembly execution NOT RUN; no vendor binaries in Git |
| RDP observation | Clipboard timeout; an attempted version query was corrupted and returned CommandNotFound. A subsequent file-list command showed corrupted path/separator characters and was canceled with Escape before execution; prompt verified clear. No coordinator-run successful ETS metadata result; subsequent operator result is recorded below |
| Acceptance | AC-001/002 portable + operator Windows evidence; AC-003 now has Windows test/copy output, exact identities and independent ZIP/manifest match. Final reviewer PASS for ordinary-task closure |
| Verifier / reviewer | Initial verifier reproduced 17 PASS; reviewer found mapped-network-drive acceptance (P2). Corrected before any commit with fail-closed local drive types and synthetic classification tests; final verifier and reviewer independently reproduced PASS 19 / FAIL 0 / NOT RUN 2 plus scope, whitespace and encoding checks; reviewer returned PASS for this partial script increment with no remaining findings |
| Gate | None reviewed or advanced; Tasks 003/005 retain their own blockers |
| Next action | Investigate the candidate dependency graph statically in a separate bounded task. Runtime and rights gates remain closed |

Official tool provenance: [PowerShell release v7.6.7](https://github.com/PowerShell/PowerShell/releases/tag/v7.6.7).
The local test runtime is not part of product dependencies or redistribution.

Reviewed implementation source commit: `8517859335c7f20fd71e815da10c57a36e69061a`. Full task remains Blocked
until Windows/real-ETS evidence; this commit accepts only the tested script increment.

## Operator-reported Windows inventory, 2026-10-21

The operator supplied a complete inventory JSON response with observation time
`2026-10-21T07:04:57.7520442Z`, `operation=inventory`, `status=complete`, count 2,
and total bytes 7,051,728. These are user-run results, not a coordinator reproduction.
The output explicitly retains `rights=UNRESOLVED`, `compatibility=UNVERIFIED`,
`metadata_method=System.Reflection.AssemblyName.GetAssemblyName` and
`target_assembly_execution=NOT_RUN`.

| Candidate | Bytes | Assembly version | SHA-256 |
|---|---:|---|---|
| `Knx.Ets.Sdk.AddIns.AddInViews.dll` | 72,704 | `6.4.8718.0` | `5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d` |
| `Knx.Ets.Sdk.dll` | 6,979,024 | `6.4.8718.0` | `7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989` |

Their respective assembly names are `Knx.Ets.Sdk.AddIns.AddInViews` and
`Knx.Ets.Sdk`; both report `Culture=neutral` and
`PublicKeyToken=61439873ec5e1159`. Filename and identity remain distinct fields.
No signature validity or trust conclusion follows from the public-key token.

No Windows synthetic-test summary or exact PowerShell version was included in
this response. No copied-file manifest or DLL bytes were supplied. This inventory
therefore improves actual reference provenance, but does not establish the complete
MDT dependency set, API/type compatibility, permissions, private-copy success or
an ETS-free runtime. No SDK substitution or DCA load is claimed.

The predefined reviewer checked the operator-evidence transcription against source
baseline `779beedc2f8b1c9f593976495686fa4cb9e08383`, independently confirmed both
hashes/identities/timestamp and the 7,051,728-byte sum, and returned PASS for this
evidence-only increment. Scope checker and `git diff --check` passed. This does
not close the outstanding Windows test/copy criteria.

## Windows copy, synthetic tests and private ZIP verification, 2026-10-21

The operator supplied `operation=private_copy`, `status=complete`, timestamp
`2026-10-21T07:10:26.2421521Z`, count 2, and the same 7,051,728 total bytes,
assembly identities and SHA-256 values as the inventory above. The operator also
supplied the Windows suite command `.\Test-EtsReferenceCandidates.ps1` and its
complete summary: 19 PASS, 0 FAIL, 2 NOT RUN. Windows drive-relative rejection
and junction rejection passed; creating a symbolic link and distinct case-colliding
files were unavailable. These are genuine operator-run tests, not a coordinator
rerun. On the same visible console, the coordinator ran the read-only `get-host`
command and observed ConsoleHost version **5.1.26100.9168**. This resolves the
previously missing PowerShell-major/version evidence.

The supplied private ZIP is `private/downloads/Knx.Ets.Sdk.AddIns.AddInViews.zip`,
1,064,624 bytes, SHA-256
`35a4790a9a3c8e87264cfb566896b73efe0683ff194680fa7eff27a4108b5100`.
The predefined explorer independently bounded and streamed it without extraction:
3 root-level regular entries, 7,053,643 total expanded bytes, no unsafe paths,
case collisions, links or special entries. Both DLL streams exactly match the
recorded byte counts and hashes. The third entry is `reference-inventory.json`,
1,915 bytes, SHA-256
`1869a665f3a86d8939411f4498c7ed58edaba7c8b4e376e6ce5e17c34a5d51b7`.

The coordinator parsed that bounded JSON with duplicate-key rejection and
confirmed exact copy timestamp, count/sum, filenames, sizes, hashes, versions,
neutral culture, token and unresolved/unverified/not-executed statuses. The
archive is ignored by the root `/private/` rule; no DLL is tracked. These checks
confirm input/copy evidence only, not permissions, signatures, complete dependency
closure, a compatible host or original DCA operation. Source baseline `13b5eb8`.


### Windows command evidence

The coordinator supplied the following private-copy invocation; the operator
confirmed completion with the copy JSON and supplied archive above. Paths are
represented by the same variables to avoid publishing private installation/home
paths. The exact interactive history was not captured; this records the instructed
and operator-confirmed operation with its flags and exact two-name selection.

```powershell
$names = @(
    'Knx.Ets.Sdk.AddIns.AddInViews.dll'
    'Knx.Ets.Sdk.dll'
)
$destination = Join-Path $env:LOCALAPPDATA `
    ('KnxEditor-EtsReferences-' + [guid]::NewGuid().ToString('N'))
& $inventoryTool -EtsDirectory $etsDirectory -CandidateFileName $names `
    -Copy -DestinationDirectory $destination
```

`$inventoryTool` is the local copy of the reviewed
`Get-EtsReferenceCandidates.ps1`; `$etsDirectory` is the operator's actual local
ETS directory. The operator explicitly supplied the synthetic test command:

```powershell
.\Test-EtsReferenceCandidates.ps1
```

Its 19/0/2 summary is recorded above; the coordinator's separate `get-host`
observation is version evidence, not a coordinator rerun of those tests.

Final predefined reviewer: PASS for Task-139 closure after independently verifying
ZIP bytes/entries, both DLL hashes, duplicate-key-rejecting manifest, exact copy
metadata, private-input exclusion, scope and whitespace. No vendor code was loaded.
The two unavailable filesystem cases stay NOT RUN; no rights/runtime gate changes.
