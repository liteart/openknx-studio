# Reference inputs and bench prerequisites

Task-004 registers input availability only. The authoritative development
inventory is [reference-inputs.toml](../config/reference-inputs.toml). Every entry
has an Available/Missing status, identity or explicit UNVERIFIED identity,
acquisition owner role, source and evidence. Missing entries include a reason.
Availability of a package does not establish product compatibility, SDK rights,
bench qualification, a PoC PASS, or permission to operate a device.

## Available files

The current inventory contains **5 Available files and 14 Missing prerequisites**.
The initial inventory contained 3 Available files and 16 Missing prerequisites;
its accepted evidence is preserved below. The firmware follow-up registers two
additional candidate files without qualifying a device or approving a write.

On 2026-10-20 the implementer streamed and hashed these local files without
extracting archives, parsing product data, or executing vendor code:

| Input ID / exact file identity | Bytes | SHA-256 |
|---|---:|---|
| `dali-dca`: `SCN-DA64x-04_MDT_DCA_v4_0_0_0.etsapp` | 1,730,085 | `cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9` |
| `dali-knxprod`: `SCN-DA64x-04_MDT_KP_V40a.knxprod` | 254,893 | `0cf819f5a9148bdfd5cd9c63d96e6c4eef273ec3593aec218cb7ebe04320b3cc` |
| `simple-knxprod`: `MDT_KP_BE_02_Binary_Input_V32a.knxprod` | 393,923 | `51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709` |

Files are under ignored `private/fixtures/mdt/`, outside public Git. The manifest
records exact official acquisition URLs. Artifact acquisition was explicitly
authorized by the user; the checker never downloads anything. File availability
does not authorize redistribution. The DALI DCA hash matches the historical
[static DCA evidence](REFERENCE-DCA.md); that historical inspection is unchanged.

The coordinator's acquisition evidence identifies the [MDT DALI product
page](https://www.mdt.de/produkte/produktdetail/aktorik/dimmaktoren/dali-control-64-gateway.html)
listing DCA 4.0.0.0 (06/24) and database 4.0a (06/25), and the
[MDT binary-input page](https://www.mdt.de/produkte/produktdetail/sensorik/binaereingaenge/binaereingang-be.html)
listing database V3.2a (01/22). BE-04000.02 is only a candidate from that page.
No actual simple device has been selected or identified, and its non-secure
profile has not been verified. Internal application IDs and package/DCA/device
matching remain UNVERIFIED for both product packages.

### Firmware input registration follow-up

The coordinator downloaded the original
[MDT Update Tool package](https://www.mdt.de/fileadmin/user_upload/user_upload/download/MDT_DCA_Update_Tool_v5_1_1_0.etsapp)
under the user's explicit original-artifact acquisition authorization. It is
preserved unchanged in `private/downloads/` and `private/fixtures/mdt/`.
The filename identifies this input; it does not establish that it is the current
version, that it loads independently, or that it supports an actual target.

| Input ID / exact file identity | Bytes | SHA-256 |
|---|---:|---|
| `firmware-dca`: `MDT_DCA_Update_Tool_v5_1_1_0.etsapp` | 9,328,498 | `c0a67d64c489e640500c0a6d8455c4fda8db3bc659c32d4cfab606317439338f` |
| `firmware-image`: `SCN-RTC20.02_R21.hex` | 275,716 | `0aeeb509bda3f2bda39688c013870a0fd77e78d9b900c3cf88c44412343db3ba` |

The explorer's bounded central-directory and manifest-only observation reported
117 entries, `AddInManifest.xml` of 19,090 bytes, AppId `M0083-A0096`, assembly
`UpdateTool.dll`, display name `UpdateTool`, and 323 unique DeviceConfiguration
application program IDs. The six IDs under the manifest comment
`Zeitschaltuhr2.Generation` range from `M-0083_A-0255-20-0000` through
`M-0083_A-0255-25-0000`. These are declared static metadata, not observed target
identities or compatibility evidence for a physical device.

The coordinator copied only the exact member
`Firmware/SCN-RTC20.02_R21.hex` from that package to
`private/fixtures/mdt/SCN-RTC20.02_R21.hex`. Its bounded read allowed 275,717 bytes
and asserted the exact 275,716-byte member size; encrypted entries and symlinks
were denied and the member was required to be a regular file. No bulk extraction,
firmware decoding, vendor execution or programming was performed. This follow-up
only registers and hashes the resulting candidate file; it does not repeat the
archive inspection or member copy.

The coordinator's source evidence includes the
[2024 RTC technical manual](https://www.mdt.de/fileadmin/user_upload/user_upload/download/SCN-RTC20-02_MDT_TM_V11_DE.pdf)
identifying firmware updates through a DCA, and the
[2020 MDT DCA update guide](https://www.mdt.de/download/MDT_LSG_Update_MDT_Produkte_mittels_DCA_App.pdf).
The current generic Firmware Update App is a distinct artifact and has not been
substituted for this original package. No current-version, runtime-load, SDK
compatibility, firmware/target match, non-secure profile, or rights certification
is claimed. This bounded registration is not Task-006 or Task-109 implementation.
`firmware-device` and all actual physical approval prerequisites remain Missing.

## Missing inputs

These 14 entries remain Missing. Their exact identity is UNVERIFIED; the
inventory does not invent a model, firmware, application ID, property, address,
gateway endpoint, or approval. The manifest records each source and missing reason.

| Input ID | Missing prerequisite | Acquisition owner |
|---|---|---|
| `simple-device` | Selected physical non-secure device and hardware/firmware/mask identity | Project operator |
| `dali-device` | Physical DALI gateway identity and application | Project operator |
| `dali-application-match` | Verified DCA/product/application/profile/target match | Project maintainer |
| `simple-application-match` | Verified non-secure product/application/profile/target match | Project maintainer |
| `knxnetip-gateway` | Exact gateway identity and isolated commissioning path | Project operator |
| `isolated-bench` | Confirmed bench isolation, topology, target inventory and operator | Project operator |
| `dali-gear` | Gear identities and isolated DALI test circuit | Project operator |
| `power-recovery` | Target-specific power, restoration and supported recovery procedure | Project operator |
| `reversible-property` | Authoritative evidence for a bounded reversible property/read-back/restoration sequence | Project maintainer |
| `human-write-grant` | Explicit human approval for the exact bench/target/operation | Project operator |
| `clean-windows` | Clean interactive Windows environment without ETS/runtime dependency | Project operator |
| `sdk-references-rights` | Exact SDK identities, compatible pinned build tools and human rights/provisioning review | Project maintainer |
| `firmware-device` | Exact supported firmware-update target and recovery evidence | Project operator |
| `knxproj` | Permitted export/hash and expected project inventory | Project operator |

An existing Windows ETS host was user-reported and its RDP UI showed ETS 6 Home
6.4.1 Build 8718. Subsequent automation reported `noWindowsAvailable`. Actual
Windows OS, MSBuild and SDK inventories are NOT RUN. This observation establishes
neither a clean Windows qualification environment nor a verified SDK reference
set. No private endpoint or screenshot is recorded here.

The separate firmware-update package now supplies the two candidate files; the
DALI archive is not their source. A device and application must be identified
again for the actual experiment; a file hash or an earlier identity record
cannot prove the current physical state.

## Bench and permitted-write policy

The repository's user-supplied [agent contract](../AGENTS.md) adopts offline,
read-only defaults and requires a human grant before writes. **No actual bench
or write grant exists in this inventory. All bus operations remain denied.**
Task-004 performs no hardware operation and cannot approve one.

Before a hardware experiment may begin its approved operation, its operator and
reviewer must provide the [hardware authorization record](TEST-STRATEGY.md):

1. Exact bench identity, confirmed isolation from household/live installations,
   topology, target inventory and responsible human operator.
2. Exact gateway/bus path and target hardware/application/firmware/profile
   identities; re-read the applicable identity immediately before operation.
3. Authoritative property/range/payload and reversible old/new/read-back/restore
   evidence for PoC B, or the immutable validated plan for the selected task.
4. Target-specific power arrangements and manufacturer-supported recovery steps.
   Do not assume a reset, arbitrary memory range, or interrupted firmware is safe.
5. Explicit human approval scoped to that operation, purpose, target, write
   allowlist and recovery record. Development authorization and agent output do
   not grant that approval. The Rust backend must enforce approval for execution.

PoC A begins with all bus requests denied. No missing input is waived by a
successful inventory check. AC-003 concerns this policy before hardware-write
work; actual bench approval remains Missing and no hardware-write task is started.
The coordinator/reviewer decides acceptance of the inventory task; this document
does not approve a gate.

## Offline checker and evidence

The standard-library Python tool is development tooling, not a product parser,
domain service, approval engine or experiment runner. It validates the closed
19-entry manifest, owners/statuses and Missing reasons, then checks Available
file sizes and streamed SHA-256 hashes. It caps the manifest at 128 KiB and each
file at 256 MiB (EP-03 compressed-input limit), rejects non-regular files and
symlink paths, and confines files to `private/fixtures/`. It does not extract,
discover hardware, invoke a shell, access a network, or run vendor code.

Run from the editor root with Python 3.11 or newer (`tomllib` is required):

```sh
python3 scripts/check_reference_inputs.py
python3 scripts/check_reference_inputs.py --require dali-dca dali-knxprod simple-knxprod
python3 scripts/check_reference_inputs.py --require isolated-bench human-write-grant
python3 scripts/check_reference_inputs.py --require not-an-input
python3 -m unittest discover -s scripts -p test_reference_inputs.py -v
python3 scripts/check_project_scope.py
python3 -m unittest discover -s scripts -p test_project_scope.py -v
git diff --check
```

`--require` checks only the listed input IDs. Missing/unknown IDs or an invalid
manifest/file produce a nonzero exit. A dependent experiment cannot use that
failed prerequisite result to report PASS. A zero exit only confirms inventory
checks and requested input availability; it never qualifies hardware or approves
execution. The tool prints this limitation on every invocation.

### Initial accepted evidence

The initial 3 Available / 16 Missing inventory evidence was gathered on macOS
27.0 arm64 with Python 3.14.6 at source baseline
`5ab53ba5c8b4766d92f80b12cad9adfbb23de34e`, branch
`task/Task-004-reference-inventory`. Task-001 and Task-002 were accepted with
verification/review PASS; Task-004 has no phase gate and does not depend on the
blocked Windows part of Task-003. Initial implementation revision:
`4f3ddb7f904c37f9b7a159a4bbd6653d1610a1b8`; acceptance record: `94358b5`.
The following results remain the initial evidence rather than new test claims.

| Check | Actual outcome |
|---|---|
| Baseline `python3 scripts/check_reference_inputs.py` | FAIL as expected, exit 2: tool did not exist |
| TEST-001 inventory/file hashes | PASS: actual available files match all three sizes and hashes above |
| Required three Available file IDs | PASS: checker exits 0 and verifies each actual file |
| TEST-003 required bench/grant | PASS negative case: checker exits 1, identifies both as Missing |
| Unknown required ID | PASS negative case: checker exits 1 and identifies the unknown ID |
| Synthetic input regressions | PASS: 11 tests; no vendor files copied into test fixtures |
| TEST-002 bench-policy/status review | PASS for inventory/policy review; actual bench isolation, target identity and approval remain NOT RUN/Missing |
| Scope checker and existing scope regressions | PASS: scope checker exits 0; all 12 scope tests pass |
| Diff whitespace check | PASS: `git diff --check` exits 0 |
| Windows/DCA/hardware operations | NOT RUN by this inventory increment |

The synthetic cases cover complete Missing inventories, valid available files,
same-size corruption, missing files/wrong size, unknown/missing required inputs,
missing/duplicate entries, malformed fields, traversal, file/parent symlinks and
size bounds. Consumer experiments remain blocked only on their own unmet inputs;
no PoC or gate is marked PASS by this inventory.

Final independent predefined verifier/reviewer: PASS for the bounded inventory,
actual hashes, negative required-input checks, 11 new synthetic tests and 12
scope regressions. Reviewer accepted AC-003 as the adopted policy prerequisite,
not a physical bench or write grant. The coordinator closes ordinary Task-004
under the user's review-and-commit authorization. No consumer or phase gate is
unlocked merely by that closure; each checks its own Missing prerequisites.

### Firmware follow-up verification

The follow-up source baseline is `96da20eb409280266a2f65ba463a1e4f7ffe11e9`,
branch `task/Task-004-firmware-inputs`. The coordinator records the reviewed
follow-up commit after verification/review; no new source commit is claimed yet.
Task-004's original acceptance and bench policy remain unchanged; this input
registration follow-up passed independent verification/review. Task-005 remains Blocked on its own SDK
and Windows prerequisites. No phase gate or device operation is approved here.

| Exact command | Follow-up outcome |
|---|---|
| `python3 scripts/check_reference_inputs.py` | PASS, exit 0: five Available files match their recorded sizes and SHA-256 hashes |
| `python3 scripts/check_reference_inputs.py --require firmware-dca firmware-image` | PASS, exit 0: both candidate files verified |
| `python3 scripts/check_reference_inputs.py --require firmware-device human-write-grant` | PASS negative case, exit 1: both prerequisites remain Missing |
| `python3 scripts/check_project_scope.py` | PASS, exit 0 |
| `git diff --check` | PASS, exit 0 |

No checker code or tests changed in this registration. The initial synthetic
test results above are historical, not reruns. Windows/vendor runtime,
firmware decoding/programming and hardware tests are NOT RUN by this follow-up.

Follow-up predefined verifier/reviewer: PASS, no findings. Both confirmed the
five available-file hashes, expected rejection of missing target/grant, unique
bounded archive-member byte equality, scope navigation and whitespace. The
coordinator accepts only this inventory update; actual firmware target matching,
rights, Windows runtime and physical effects remain unqualified.
