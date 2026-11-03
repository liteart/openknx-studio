# Bounded offline DCA package inspection

Task-006 provides the standalone Rust crate in `poc/dca-inspect/`. It reads the
selected MDT Knxapp package profile without extraction, assembly loading,
decompilation, vendor execution, dependency resolution or network access. It is
an inspection experiment, not a product importer or a Windows DCA host.

The report preserves exact archive names, hashes every entry, separates files
and directories, and reports the manifest application IDs, bitness declaration
and assembly spelling. A `.dll` suffix is reported as an assembly filename only;
it is not a verified CLR identity. Paths and strings in the text report are
quoted/escaped, entries are sorted, and output is deterministic for identical
input bytes. Directory hashes describe their empty data, not their descendants.

Literal ASCII token scans of the primary DLL and its configuration are labeled
as byte observations. They do not parse CLR metadata or establish dependencies,
runtime binding, compatibility, signatures, trust or rights. A missing token
proves no absence. The report points to the separate
[Task-140 metadata evidence](DCA-CONTRACT-MAP.md), which applies only to its
recorded input hashes; this tool does not reproduce or extend that experiment.

## Selected profile and limits

The actual 1,025-byte `AddInManifest.xml` begins with a UTF-8 BOM. The parser
accepts that optional prefix, then the observed ASCII UTF-8 XML profile. It
requires the `Knxapp` root with AppId `M0083-A0158`, assembly declaration
`Mdt-DaliDCAPlus.dll`, the observed configuration attributes and container/leaf
structure, and six distinct application IDs. It preserves IDs in manifest order.
Unknown elements/attributes, duplicate singleton fields/attributes/IDs, malformed
trees, text content, comments, processing instructions, CDATA, DTDs and all entity
references are rejected. This deliberate subset is not a universal ETS schema.
The `Is64BitCompatible` value is a declaration only.

| Bound | Limit |
|---|---:|
| Input archive | 8 MiB |
| Central-directory records | 256 |
| Central-directory bytes | 128 KiB |
| Each expanded entry | 4 MiB |
| Aggregate expanded data | 16 MiB |
| Each entry expansion ratio | 200 times compressed size, with a one-byte denominator floor |
| Path / XML attribute bytes | 240 |
| Each local/central extra field region | 1 KiB |
| Manifest bytes including optional BOM | 16 KiB |
| XML nesting / events | 8 / 512 |
| Expanded read chunk | 8 KiB |

Before ZIP library metadata allocation, a small preflight accepts only ordinary
single-disk ZIP32 with an exact final uncommented EOCD, bounded central records,
and local header ranges. Flags are restricted to zero or UTF-8; only the selected
package's NTFS timestamp extra fields are admitted and ignored. ZIP64, SFX,
archive/entry comments, trailing data, descriptors, encryption, unknown extras
and non-Stored/non-Deflate compression fail closed. The upstream ZIP library's
strict consistency check rejects duplicated central names, local/central header
disagreement and header/data overlap, including central-directory overlap.

All entry metadata is checked before any content is decompressed. ASCII paths
must be relative with slash separators, ordinary nonempty components, and no
traversal, control bytes, Windows reserved names, drive/stream syntax or trailing
dots/spaces. Case-folded collisions, file/directory conflicts, file parents and
inconsistent parent-component casing are rejected. Unix symlink/special-file
modes and nonempty directory data are rejected. No extraction API is called.
Every member is streamed to EOF to verify size and CRC, including resources
unrelated to the manifest. The primary assembly is resolved by an unambiguous
case-insensitive lookup, without changing the source spelling or archive.

The synchronous library accepts an `AtomicBool` cancellation token, checked
before work, between entries, at each expanded read chunk and each XML event.
Initial local-file reading, bounded ZIP metadata/consistency work and hashing of
the bounded archive finish before the next cancellation point. The CLI uses
ordinary process termination; it does not promise a timing bound or add a signal
runtime. The entry path must identify a regular local file; symlink/special-file
inputs are refused. The tool does not provide a filesystem race-isolation sandbox.
Errors have stable `dca_inspect.*` keys and English fallback messages. This PoC
does not introduce the later production localization framework.

## Reproduction

Use the repository-pinned Rust 1.99.1 from the accepted Task-003 provisioning.
`RUSTUP_HOME`, `CARGO_HOME` and `PATH` must identify that provisioned developer
toolchain; `DCA_REFERENCE_INPUT` must identify the privately registered archive.
These are explicit operator/developer inputs, not application downloads. Keep
outputs under the ignored worktree `private/` directory.

```sh
export CARGO_TARGET_DIR="$PWD/private/target"
cargo fmt --manifest-path poc/dca-inspect/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/dca-inspect/Cargo.toml --workspace --all-targets --locked --offline -- -D warnings
cargo test --manifest-path poc/dca-inspect/Cargo.toml --workspace --locked --offline
cargo test --manifest-path poc/dca-inspect/Cargo.toml --locked --offline registered_reference_matches_inventory_and_remains_unchanged -- --ignored --exact
cargo run --manifest-path poc/dca-inspect/Cargo.toml --locked --offline -- "$DCA_REFERENCE_INPUT" > private/task006-report.txt
python3 scripts/check_project_scope.py
git diff --check
```

The normal suite uses only owned synthetic data. The separately ignored fixture
test requires an explicit path, first checks the recorded archive hash, then
checks counts, six IDs, exact spellings, primary size/hash, token observations,
and unchanged file bytes afterward. It must be invoked separately to claim
TEST-002. No manufacturer content is checked into the repository. The standalone
crate has its own workspace and lockfile; root toolchain smoke sources/manifests
are unchanged. `--all-features` is neither needed nor approved.

## Dependency evidence

On 2026-10-21 the explicitly authorized developer `cargo fetch` retrieved these
packages from the official crates.io registry after the first sandboxed attempt
could not resolve DNS. All subsequent verification used `--locked --offline`.
Exact registry sources and checksums are pinned in the standalone Cargo.lock.
The downloaded primary Cargo manifests, source APIs, feature graph and included
license files were inspected before use. This is a source/provenance record, not
a vendor/SDK rights decision or a redistribution approval.

| Direct package | Version | Declared MSRV | License | Selected capability |
|---|---|---|---|---|
| zip | 9.0.5 | 1.88 | MIT | Defaults off; `deflate-flate2-zlib-rs`; bounded ZIP read, consistency and CRC APIs |
| quick-xml | 0.42.0 | 1.86 | MIT | Defaults off; streaming events, checked attributes; no entity resolution |
| sha2 | 0.11.0 | 1.85 | MIT OR Apache-2.0 | Defaults off; incremental SHA-256 |

Registry package source pages:
[zip 9.0.5](https://crates.io/crates/zip/9.0.5),
[quick-xml 0.42.0](https://crates.io/crates/quick-xml/0.42.0),
[sha2 0.11.0](https://crates.io/crates/sha2/0.11.0).
The actual `zip::ZipArchive::validate_header_consistency`, `by_index_data`,
`read::Config`/`ArchiveOffset`, `ZipFile` size/mode/read APIs, quick-xml UTF-8 event
and checked attribute APIs, and `sha2::Digest` were verified in those downloaded
sources. The selected deflater is the Rust `zlib-rs` backend; no runtime helper,
SDK or network transport is used. Upstream dependencies may contain unsafe code;
only owned Rust forbids it, and this record does not claim a transitive audit.

All 22 locked external packages have included license notices. In addition to
the three direct packages, the following transitives were checked:

| Packages at locked versions | Included license |
|---|---|
| bitflags 2.13.2; block-buffer 0.12.1; cfg-if 1.0.5; cpufeatures 0.3.1; crc32fast 1.5.3; crypto-common 0.2.2; digest 0.11.3 | MIT OR Apache-2.0 |
| equivalent 1.0.3; flate2 1.1.10; hashbrown 0.17.1; hybrid-array 0.4.15; indexmap 2.14.2; libc 0.2.191; typed-path 0.12.4; typenum 1.20.1 | MIT OR Apache-2.0 |
| cfg_aliases 0.2.2; nix 0.31.3 | MIT |
| memchr 2.8.3 | Unlicense OR MIT; MIT notice inspected |
| zlib-rs 0.6.8 | Zlib |

The highest declared external MSRV is 1.88, below the selected 1.99.1 toolchain;
`cfg_aliases` declares no MSRV. The observed macOS feature tree includes nix's
`fs` feature, SHA CPU detection and standard allocation facilities, with no ZIP
crypto, alternate codecs, KNX or Secure dependencies. The later Windows follow-up below verifies the owned synthetic suite and build.
Linux remains NOT RUN; source metadata/platform conditions do not substitute
for a platform build. Preserve all upstream notices in any future
packaging task.

## Observed result

The implementation run on macOS 27 arm64 matched the
[registered static inventory](REFERENCE-DCA.md): SHA-256
`cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9`,
1,730,085 input bytes, 112 entries, 108 files, 4 directories and 3,393,880 expanded
bytes. The manifest's six IDs and `Mdt-DaliDCAPlus.dll` declaration are preserved;
the actual entry is `MDT-DaliDcaPlus.dll`, 452,608 bytes, SHA-256
`4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d`.
The bitness flag is true. The package bytes remained unchanged.

The first private run failed on the previously unhandled UTF-8 BOM; an owned
regression reproduced FAIL before the correction and PASS afterward. A second
owned regression exposed permissive malformed attribute/declaration handling and
also went from FAIL to PASS after tightening the selected profile. An initial
aggregate-limit synthetic case hit the input limit instead; its owned data was
corrected to exercise the aggregate guard. These are implementation observations,
not successful vendor execution. Final command outcomes and independent review
status are recorded in [Task-006](../tasks/Task-006.md).

## Windows follow-up

After explicit user transfer authorization on 2026-10-21, the independent verifier
copied the five original crate files into a fresh Windows temporary source tree
and verified their SHA-256 hashes before and after checking. Source revision:
`607bd5f760104e1f79799dc524b4df17957ea7e8`. No manufacturer fixture or SDK DLL was
transferred. A private bounded wrapper captured child output and exit status;
raw machine paths and setup logs remain private.

Windows 11 Pro 10.0.26200 x64 ran Rust/Cargo 1.99.1 for
`x86_64-pc-windows-msvc`, with Visual Studio Build Tools 18 x64 initialized by
VsDevCmd. The child process selected the existing private toolchain/cache and a
fresh target directory. With `MANIFEST` identifying the staged Cargo.toml, these
exact Cargo arguments ran:

```powershell
cargo fetch --manifest-path "$env:MANIFEST" --locked --target x86_64-pc-windows-msvc
cargo fmt --manifest-path "$env:MANIFEST" --all -- --check
cargo clippy --manifest-path "$env:MANIFEST" --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path "$env:MANIFEST" --workspace --locked --offline --no-default-features
```

`MANIFEST` is portable notation for the explicit absolute argument used by the
private batch; no floating toolchain or application download behavior was added.
The developer dependency fetch, format check, Clippy and test command all exited
0. Tests reported 16 passed, 0 failed and 1 ignored private fixture. All five
source hashes, including the lockfile, were unchanged afterward. The actual
manufacturer-fixture proof remains the separately recorded macOS run. This
Windows result does not establish CLR, SDK, original DCA or hardware behavior.
