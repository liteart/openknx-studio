# Task-139: Inventory and privately copy selected local ETS reference candidates

## Subject

Provide a bounded user-run script for metadata-only inventory and optional private
copying of explicitly selected reference DLL candidates from the user's own ETS.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-139 |
| Status | Blocked |
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
- [ ] AC-003: Focused positive/negative synthetic tests pass and the real Windows command,
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

Actual candidate DLL names/versions and terms are unknown until inspected. No
Windows/NTFS success may be inferred from portable tests. Installed ETS files may
be runtime implementations rather than adequate or permitted standalone contracts.
Stop the affected route if concrete services/activation/identity cannot be provided
without modifying vendor files or depending on ETS at runtime.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / task status | Script increment implemented; full task Blocked until Windows/real-ETS evidence |
| Source baseline | `104d0e3`; reviewed implementation revision recorded after commit |
| Files | Script, self-contained synthetic suite, local-reference guide; task/index/traceability/private-binary policy and Task-005 cross-links |
| Platform | macOS 27 arm64; repository-private PowerShell 7.6.7; no global installation |
| Provisioning | Official PowerShell release `v7.6.7`, asset `powershell-7.6.7-osx-arm64.tar.gz`, 70,628,335 bytes; SHA-256 `50dde99bb066adacbb9a748182d09eb9beff25119dc3b12e85c84b6b6b1c5da9` matched official release asset metadata |
| Test command | `private/toolchains/powershell-7.6.7/pwsh -NoLogo -NoProfile -NonInteractive -File scripts/windows/Test-EtsReferenceCandidates.ps1 -TemporaryParent /private/tmp` |
| Implementation test result | Initial portable result PASS 17, FAIL 0, NOT RUN 2; after mapped-drive and bracket-path coverage, PASS 19, FAIL 0, NOT RUN 2. Owned managed fixtures only; initial UNC classification failure corrected |
| Open platform checks | Windows PowerShell 5.1, NTFS/junction/sharing and distinct case-colliding files NOT RUN; macOS volume cannot create the case fixture |
| Actual ETS inspection / copy / DCA execution | NOT RUN; no vendor binaries in source, no target assembly execution |
| RDP observation | Clipboard timeout; an attempted version query was corrupted and returned CommandNotFound. A subsequent file-list command showed corrupted path/separator characters and was canceled with Escape before execution; prompt verified clear. No successful actual-ETS metadata result |
| Acceptance | AC-001/002 verified with synthetic fixtures; AC-003 remains open for genuine Windows/ETS evidence |
| Verifier / reviewer | Initial verifier reproduced 17 PASS; reviewer found mapped-network-drive acceptance (P2). Corrected before any commit with fail-closed local drive types and synthetic classification tests; final verifier and reviewer independently reproduced PASS 19 / FAIL 0 / NOT RUN 2 plus scope, whitespace and encoding checks; reviewer returned PASS for this partial script increment with no remaining findings |
| Gate | None reviewed or advanced; Tasks 003/005 retain their own blockers |
| Next action | Run the reviewed script under Windows PowerShell 5.1 on exact operator-selected candidates; retain output privately. Then evaluate actual contract identities and rights separately |

Official tool provenance: [PowerShell release v7.6.7](https://github.com/PowerShell/PowerShell/releases/tag/v7.6.7).
The local test runtime is not part of product dependencies or redistribution.
