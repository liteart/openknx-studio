# PoC toolchains and offline verification

Task-003 adds one dependency-free editor executable, `toolchain-smoke`, to
verify the Rust build environment. It has no domain, KNX, device, DCA, network,
or persistence behavior. Its output is a technical experiment marker, not a
device-operation result. The modular product workspace remains later work.

## Pinned Rust environment

`rust-toolchain.toml` selects Rust **1.99.1**, the `minimal` profile, and the
`rustfmt` and `clippy` components. `Cargo.toml` uses edition 2024 and explicitly
sets the minimum Rust version to 1.99.1. This is the selected PoC baseline;
older compiler compatibility has not been tested. `Cargo.lock` contains only
the editor smoke package; there are no third-party dependencies.

The coordinator verified the version against the [official Rust 1.99.1 release
announcement](https://blog.rust-lang.org/2026/10/15/Rust-1.99.1/) and the
[official versioned channel manifest](https://static.rust-lang.org/dist/channel-rust-1.99.1.toml).
The locally inspected manifest has release date `2026-10-15` and SHA256
`fb152be80975c194aa5fc684b2aec3a6e14e2b855efe2faa4e5f8ae6e590f463`.
Its private evidence copy is `private/downloads/channel-rust-1.99.1.toml`.
These sources select the compiler; they do not qualify a device or SDK.

Observed on 2026-10-20, macOS 27.0 build `26A428`, `aarch64-apple-darwin`:

| Command | Observed version |
|---|---|
| `rustc -Vv` | `rustc 1.99.1 (58cf8f9c3 2026-10-13)`; full commit `58cf8f9c35b355199727627d07ae34a4e2846d67`; LLVM `23.1.1` |
| `cargo -V` | `cargo 1.99.1 (5f94df478 2026-08-27)` |
| `rustfmt -V` | `rustfmt 1.10.0-stable (58cf8f9c35 2026-10-13)` |
| `cargo clippy -V` | `clippy 0.1.99 (58cf8f9c35 2026-10-13)` |
| `rustup -V` | `rustup 1.29.1 (d95a37b6a 2026-08-13)` |

The version inspection used explicit `+1.99.1` selectors before the pin file
existed. The subsequent build commands below use the repository pin.

## Editor commands

Provision the exact pinned toolchain and components before offline use. Cargo's
`--offline` flag does not provision Rust; rustup may otherwise attempt a download
when the selected toolchain is absent. No network provisioning command is part
of this offline verification. The Rust experiment requires neither .NET nor ETS.

Run from the editor repository root using an already provisioned toolchain:

```sh
cargo fmt --all -- --check
cargo clippy --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --workspace --locked --offline --no-default-features
cargo run --locked --offline --no-default-features --bin toolchain-smoke
cargo metadata --format-version 1 --locked --offline --no-default-features
cargo tree --locked --offline --no-default-features --edges features
```

The local experiment used only the ignored repository-local installation. In a
POSIX shell its command environment was:

```sh
export RUSTUP_HOME="$PWD/private/toolchains/rustup"
export CARGO_HOME="$PWD/private/toolchains/cargo"
export PATH="$PWD/private/toolchains/cargo/bin:$PATH"
```

These paths are local provisioning details, not required repository contents.
Another pre-provisioned installation can run the same Cargo commands. No global
shell configuration was changed. `cargo generate-lockfile --offline` generated
the lockfile; normal verification keeps `--locked`.

The expected executable output is:

```text
toolchain-smoke offline=true protocol=none
```

The package declares only `default = []`; the commands disable default features
explicitly. For TEST-003, inspect the full `cargo metadata` resolution graph:
the observed sole node has `features: []`, `dependencies: []`, and `deps: []`.
The feature tree contains only `knx-editor-toolchain-smoke v0.0.0`. No Secure
implementation or dependency is present or activated. No `--all-features`
command is authorized by this command set. This checks the editor smoke build;
the independent fork needs its own feature evidence.

## Observed editor results

The source baseline was editor commit
`a89fe47b7448e2ec9e4b872f01300d7f2d3045d7` on
`task/Task-003-poc-toolchain`. Task-002 is recorded Done with final verification
and review PASS. Task-003 requires no phase gate. The coordinator records the
reviewed implementation commit after committing this increment; this document
does not claim that uncommitted files already have a source revision.

| Check | Outcome and actual observation |
|---|---|
| Baseline reproduction | FAIL as expected: before scaffolding, `cargo +1.99.1 run --offline --no-default-features --bin toolchain-smoke` exited 101 because no `Cargo.toml` existed |
| Initial formatting | FAIL: the first `cargo fmt --all -- --check` requested multiline `writeln!` formatting; corrected with `cargo fmt --all` |
| Lock generation | PASS: `cargo generate-lockfile --offline` exited 0 |
| Final formatting | PASS: the documented format check exited 0 |
| Clippy | PASS: the documented lint command completed without warnings |
| Test target | PASS: the documented test command completed; 0 unit tests, no device or behavior qualification asserted |
| TEST-001, macOS | PASS: the documented run command compiled and executed the binary, exited 0, and printed the marker above |
| TEST-003, editor | PASS: the documented metadata and feature-tree commands confirmed no enabled features and no dependencies |
| Linux execution | NOT RUN: no Linux runner available in this work session |
| Windows Rust execution | NOT RUN: a Windows environment is reachable by RDP; toolchain inventory and the build proof are pending |
| TEST-002, Windows host | NOT RUN: a Windows environment is reachable by RDP; verified SDK/MSBuild/.NET Framework inventory and host build are pending |
| Hardware / original DCA execution | NOT RUN: this smoke experiment performs neither |

There are no fixture archives or manufacturer inputs in the Rust experiment.
The actual executable run is the build proof; a unit test repeating its fixed
marker would not add independent evidence. Source inspection and the resolved
dependency graph establish that no .NET, ETS, or device executable is invoked.

## Scope checker regression from local provisioning

Installing the toolchain under ignored `private/toolchains/` exposed a scope
checker defect: the Markdown scan entered ignored private inputs and SDK
documentation. It reported broken links unrelated to editor documentation.
The scanner now excludes only the root `private`, `artifacts`, `captures`,
`credentials`, `projects`, and `cache` directories, matching those root ignore
rules. Existing hidden-directory, build-output, nested-Git and symlink exclusions
remain in place. Matching names below ordinary documentation directories remain
checked; this is not a general-purpose Git ignore parser.

Focused synthetic fixtures create `private/sdk/README.md` with a broken local
link, exercise all six root directories, and verify that broken links under
`docs/private/` and the other matching names are still rejected.

| Exact command | Observed result |
|---|---|
| `python3 -m unittest discover -s scripts -p test_project_scope.py -k root_private_artifact -v` before scanner fix | FAIL as expected: 1 test, 6 failing directory subtests; the first incorrectly reported `private/sdk/README.md: unresolved local link missing-sdk-document.md` |
| `python3 scripts/check_project_scope.py` after scanner fix | PASS, exit 0 |
| `python3 -m unittest discover -s scripts -p test_project_scope.py -v` after scanner fix | PASS: all 12 tests, including root exclusions and ordinary nested documentation coverage |
| `git diff --check` | PASS, exit 0 |

## Windows host prerequisite

Task-003 remains partial until the actual SDK inventory selects the compatible
.NET Framework target, MSBuild version, assembly identities and lawful pinned
references, and the Windows environment compiles a minimal host test. No host project,
reference list, target framework, Windows build command, or PASS is invented
before that inventory. Record the exact Windows build/test command and result
when those prerequisites exist. Cross-compilation or this macOS run cannot
replace Windows execution evidence.

## Fork source and provisioning evidence

The fork tooling increment was verified/reviewed and committed first as
`f85e40d8709d0ad31f029b3eab7b0b6e00abd48e` (upstream baseline
`287bd5c94644688ac2eb93bf09f10a37bc509e0c`). It pins the same compiler and
preserves the upstream Cargo.lock, source, MIT LICENSE and declared MSRV 1.85.1.
Fork `cargo metadata --format-version 1 --no-deps --locked --offline --no-default-features --features dpt`
passed with the prepared local toolchain. Its `resolve` is null; this does not
prove the transitive feature closure or a library build. Fork builds, Clippy,
inherited tests and examples remain NOT RUN until the dependency cache and
Task-015 test/feature audit are ready. No editor dependency on the fork is added
by the smoke experiment; this source revision records tooling provenance only.

Official `rustup-init` was downloaded for `aarch64-apple-darwin` from
https://static.rust-lang.org/rustup/dist/aarch64-apple-darwin/rustup-init and its
SHA256 checked against the accompanying official `.sha256` file:
`ec1b9233e7f72990ecd8e62063fa7f6c3dfc2bec8e97f88bff165f9100ac696a`.
The first sandboxed install failed DNS resolution; the authorized network retry
installed the requested toolchain/components under the ignored local directories
using `--no-modify-path --default-toolchain 1.99.1 --profile minimal --component rustfmt --component clippy`.

## Windows inventory observation

The user supplied an ETS6-installed Windows machine and authorized RDP access
through Windows App. RDP connected and screenshots showed an already running
ETS6 Home 6.4.1 (Build 8718). This is not an ETS-free qualification environment.
The predefined DCA agent and coordinator could observe the desktop, but input
control then returned `Computer Use server error -10005: noWindowsAvailable`.
No inventory command ran on Windows, so OS/architecture, MSBuild, targeting packs,
SDK assemblies and Windows Rust tools are unverified. No Windows host was built
and no ETS project or device operation was performed. The private host address
and household screenshots are intentionally absent from source-control evidence.

Windows input recovery and lawful exact SDK reference provisioning are outstanding.
Task-003 remains Blocked overall, with a reviewed Rust-only increment. Independent
fixture/provisioning inventory tasks may proceed on their own satisfied dependencies;
this does not accept Task-003 or unlock a dependent implementation.

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


## Direct Windows SSH inventory, 2026-10-21

The operator enabled a password-authenticated OpenSSH master connection from the
coordinator's Mac. The coordinator independently ran read-only PowerShell commands
through its private local control socket. No password, socket, account/host address
or Windows profile path is part of the source record. This supersedes historical
RDP input failures for command-line access; interactive vendor UI is untested.

At `2026-10-21T10:19:10.1166105Z` the inventory observed:

| Check | Result |
|---|---|
| Windows process/OS | `AMD64`, 64-bit OS and PowerShell process |
| PowerShell | `5.1.26100.9168` |
| OS API version | `10.0.26200.0`; distinct from the full OS build already reported by the operator |
| Explicit installed MSBuild executable | `18.9.1.35102` |
| .NET Framework registry | Release `533509`, Version `4.8.09221` |
| Conventional framework reference roots | Neither Program Files nor Program Files (x86) root exists |
| PATH command lookup | `msbuild`, `dotnet`, `rustc`, `cargo`, `git` not found; this is not a whole-disk absence claim |

Microsoft's [version/dependency table](https://learn.microsoft.com/dotnet/framework/install/versions-and-dependencies)
maps Release 533509 to .NET Framework 4.8.1. This installed runtime does not supply
proof of build-time reference packs or original SDK/DCA compatibility.

The private inventory is `private/artifacts/task003-ssh-inventory.json`;
its SHA-256 is `2ba6bac3e3785f4515a2a867f6b5449570a0ddc2ec39b034e03d4d433a737f1d`.
The actual query reads Environment architecture/OS values, PSVersionTable,
Get-Command for the five names above, the installed MSBuild `-version -nologo`,
the two conventional reference roots and the framework v4 Full registry values.
It does not enumerate ETS projects or invoke ETS/vendor code.

### Private framework-only reference provisioning

For an owned compiler smoke test, select .NET Framework 4.8 reference assemblies
from Microsoft's public `Microsoft.NETFramework.ReferenceAssemblies.net48` NuGet
package **1.0.3**. This is a compiler prerequisite experiment, not selection or
qualification of an ETS runtime contract. Microsoft [documents reference packages](https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies)
as an alternative to an installed Developer Pack.

| Package observation | Result |
|---|---|
| Exact public source | `https://api.nuget.org/v3-flatcontainer/microsoft.netframework.referenceassemblies.net48/1.0.3/microsoft.netframework.referenceassemblies.net48.1.0.3.nupkg` |
| Archive bytes / SHA-256 | 20,997,929 / `8a7e348538e7eb91351696911689f49e3d4f63f8bab517432bbe159b8b1104a2` |
| ZIP inventory | 374 entries, expanded sum 116,705,757 bytes |
| Actual nuspec | Microsoft publisher, version 1.0.3, developmentDependency true; no dependencies |
| Actual license pointer | `https://github.com/Microsoft/dotnet/blob/master/LICENSE`; checked MIT, copyright 2019 Microsoft |
| License packaging limitation | No embedded LICENSE entry; the nuspec's external pointer was inspected separately |

The [official package listing](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net48/1.0.3)
agrees with the observed identity. The archive was downloaded over HTTPS, pinned
by SHA-256 and validated for bounded entries/sizes, safe names, case collisions
and special files. This is content identity/provenance evidence, not an independent
NuGet-signature verification claim. After transfer to a new Windows user-temporary
directory, both archive SHA-256 and the five selected reference DLL hashes matched.
Only `mscorlib`, `System`, `WindowsBase`, `PresentationCore` and
`PresentationFramework` are staged. No global installation, PATH change or ETS
file modification occurred. The Microsoft package remains private and is not
relicensed or redistributed in this repository; ETS usage rights remain unresolved.

### Owned Windows framework compiler smoke

The original sources in `experiments/windows-toolchain-smoke/` contain a tiny
MSBuild project, C# program and PowerShell 5.1 driver. The project invokes the
installed Roslyn Csc task with five explicit reference files, `NoConfig` and
`NoStandardLib`; it imports no targets and has no restore/download step. It
neither references an ETS assembly nor constructs UI objects. The executable
checks x64, STA and three framework type references, then exits.

The driver checks the exact expected MSBuild version and reference DLL hashes
before creating a new output directory. Sources, references and output must be
separate trees. Version/run deadlines are 10 seconds each, build deadline is 60
seconds, combined captured output is limited to 65,536 characters. The runner
attempts to terminate its owned process tree on interruption/timeout; it is not
a general malicious-code containment boundary.

Run on Windows, using provisioned local paths (normalized placeholders):

```powershell
& "$SourceDirectory\Test-ToolchainSmoke.ps1" `
  -MSBuildPath $MSBuildExe `
  -ExpectedMSBuildVersion '18.9.1.35102' `
  -FrameworkReferenceDirectory $PrivateReferences `
  -OutputDirectory $NewPrivateOutput
```

The actual Windows command completed with exit 0 and `status=PASS`, emitting:

```text
toolchain-smoke target=net48 process=x64 apartment=STA wpf-types=3 vendor=none protocol=none
```

Executable SHA-256:
`1b7a15acd69b522eb9e9d6d229a13ea73d19625e190b59771302e38152df054d`.
The initial invocation placed output under the source directory and was correctly
rejected with `SMOKE_OUTPUT_OVERLAP` before building; the successful run used
separate source/reference/output siblings. Transfer hashes matched the three
local source files before execution. Actual private stdout/stderr evidence is in
`private/artifacts/task003-windows-smoke-stdout.txt` and its stderr counterpart.

This is Windows framework/compiler prerequisite evidence only. It does not
complete Task-003 TEST-002's SDK-qualified host proof, instantiate an original
DCA, test an interactive desktop or resolve Task-005. The full task remains
Blocked while those stated prerequisites are unmet.

The predefined verifier independently checked all three uploaded source hashes,
built and ran the smoke successfully, and reproduced five negative cases:
existing output (hash unchanged), wrong MSBuild version, source/output overlap,
missing references, and owned fake reference files. Each failed before output
creation where applicable. Timeout/output-overflow cleanup is source-reviewed,
not separately exercised on Windows. The predefined reviewer returned PASS with
no P1/P2 source findings; final evidence/commit review is recorded in Task-003.

### Windows Rust attempt and capacity blocker

The explorer independently confirmed MSVC **14.51.36231** (`cl.exe`, `link.exe`)
and Windows SDK **10.0.26100.0** headers, libraries, `rc.exe` and `mt.exe`.
The current Windows machine can therefore supply these native prerequisites;
Rust itself was not initially available through PATH.

Following the [official Windows rustup route](https://rust-lang.github.io/rustup/installation/other.html),
the coordinator downloaded the Windows x64 MSVC bootstrap and checked its
SHA-256 against the official accompanying `.sha256` file before transfer:

| Artifact | Observation |
|---|---|
| Source | `https://static.rust-lang.org/rustup/dist/x86_64-pc-windows-msvc/rustup-init.exe` |
| Bytes / SHA-256 | 12,721,664 / `6f4bef66261261fcb43131be8720bab817d403a09edec7455c371974b90bdb7e` |
| Observed bootstrap manager | `rustup 1.29.1 (d95a37b6a 2026-08-13)` |
| Requested toolchain | `1.99.1-x86_64-pc-windows-msvc`, minimal profile, rustfmt and Clippy |
| Installation scope | New task-temporary `RUSTUP_HOME` and `CARGO_HOME`, `--no-modify-path` |
| Actual result | FAIL: component extraction reported OS error 112, insufficient disk space, and rolled back |
| Windows Rust build/test | NOT RUN; subsequent version probes confirmed no active compiler/Cargo |
| Capacity observation | Only 72,056,832 bytes available on the sole local fixed volume, about 69 MiB |

The process-end-status query in the initial PowerShell provisioning wrapper failed
separately; this is not treated as a captured installer exit code. Subsequent
bounded process/version checks and the actual installer stderr establish the
failed extraction and absence of an active compiler. No switch to `stable`, GNU,
or another Rust version was used to manufacture a passing result. The requested
1.99.1 Windows package identities are present in the already pinned official
channel manifest; no Windows Rust build is inferred from those entries.

Actual installer arguments were:

```text
-y --no-modify-path --default-host x86_64-pc-windows-msvc --default-toolchain 1.99.1 --profile minimal --component rustfmt --component clippy
```

The user was asked to provide several GB of free space (5 GB suggested reserve)
or enlarge the VM disk. No unrelated files are deleted to provide room. Private
logs remain in `private/artifacts/task003-rust-*-stdout.txt` and their stderr
counterparts. Provisioning and Windows Rust execution remain incomplete.

Cleanup state: an initial conservative cleanup preflight stopped on rustup's 13
owned executable symbolic-link proxies before deleting anything. A bounded,
non-following inspection confirmed those files point to the relative owned
`rustup.exe`, with no directory links and no current-user Rustup uninstall key.
Before the revised cleanup could connect, the SSH master was gone and the new
connection timed out. Removal of the incomplete private Rust directories is
therefore **NOT RUN / pending**, not claimed complete. No unrelated Windows
files were removed; the successful C# sources/references/output remain available
in their separate task-temporary siblings. Restore access and capacity before
retrying provisioning or completing cleanup.
