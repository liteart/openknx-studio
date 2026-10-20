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
