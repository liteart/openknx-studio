# Task-006: Inspect the original MDT DCA package without executing it

## Subject

Inspect the original MDT DCA package without executing it

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-006 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-inspect (Rust) |
| Owner | Rust implementer; independently verified and reviewed |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Build a small read-only Rust inspection tool for the supplied .etsapp and produce a reproducible dependency inventory.

## Detailed Requirements

- DR-001: Read archive entries and manifest without loading assemblies; apply bounded sizes and path checks.
- DR-002: Report file and directory counts separately, manifest application IDs, declared bitness, assembly names, hashes, and detected metadata references.
- DR-003: Record the manifest/actual assembly filename case difference without renaming the original archive.
- DR-004: Label strings/metadata as static observations, not verified runtime dependencies or behavior.
- DR-005: Never execute embedded content or fetch dependencies automatically.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: The supplied archive hash matches the recorded fixture and remains unchanged.
- [x] AC-002: The report includes the six declared application IDs and exact manifest spelling.
- [x] AC-003: Invalid paths/case collisions or limit violations fail before extraction/execution.

## Dependencies to Other Tasks

- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: manifest parsing, case-insensitive lookup with ambiguity rejection, and count separation.
- TEST-002: Fixture: compare report with the supplied archive inventory.
- TEST-003: Negative: traversal, oversized entries, duplicate/case-colliding names, malformed manifest.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-inspect (Rust)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No DCA initialization or claim of complete semantic reverse engineering.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Done; bounded Rust inspection independently verified and critically reviewed PASS |
| Source baseline | `be5b4090740151ecf7cd0b3e66d9a7f445a863ac` on `task/Task-006-dca-inspect`; reviewed implementation `607bd5f760104e1f79799dc524b4df17957ea7e8` |
| Predecessor readiness | Tasks 001/002/004 Done; Task-003 current completion record Done with final reviewed evidence `a764f260a66db52d19b921c9beab0ba94665e00f`; coordinator supplied accepted readiness; no phase gate required |
| Platform / toolchain | macOS 27.0 build 26A428, aarch64-apple-darwin; rustc 1.99.1 (`58cf8f9c35b355199727627d07ae34a4e2846d67`), LLVM 23.1.1 |
| TEST-001 | PASS: owned manifest, case-insensitive resolution, count separation, deterministic inventory and cancellation coverage |
| TEST-002 | PASS: separately invoked registered private fixture test; exact archive/primary hashes, counts, six IDs, spellings and unchanged input bytes |
| TEST-003 | PASS: traversal/Windows aliases, case/duplicate/component collisions, entry/count/input/aggregate/ratio/XML bounds, special files, encryption, unsupported compression, header inconsistency/overlap, CRC and malformed XML rejection |
| Required Rust checks | PASS: format check; Clippy workspace/all-targets locked/offline with warnings denied; workspace tests 16 PASS, 1 explicitly ignored private fixture; separate private fixture invocation 1 PASS |
| Feature / dependency review | PASS for bounded PoC selection: exact direct pins and standalone lockfile; 22 external packages' manifests/notices inspected; defaults disabled, only ZIP deflate-flate2-zlib-rs enabled; feature tree observed |
| Evidence location | [DCA-INSPECT.md](../docs/DCA-INSPECT.md); owned tests in `poc/dca-inspect/tests/inspection.rs`; private generated `private/task006-report.txt` in task worktree |
| Private report identity | 16,858 bytes, SHA-256 `12d69a4b2cf686f05070efdc654f5737761c0c4ad0b09ac7b6649ecc81f31fce`; no manufacturer bytes committed |
| Lockfile identity | SHA-256 `78fb2d9c993a5e15ad2775b6d12714471134ea0180dde3b9f75824270ba927ba` |
| Acceptance criteria met | AC-001/002/003 demonstrated by implementation checks; independent critical review PASS; ordinary task criteria met |
| Technical outcome | PASS for selected offline archive/manifest/token inspection on macOS |
| Windows follow-up | PASS: Windows 11 Pro 10.0.26200 x64, Rust/Cargo 1.99.1; fmt, Clippy and 16 synthetic tests; 1 private fixture ignored and not transferred |
| Linux / CLR / vendor runtime / hardware | NOT RUN; Windows checks do not qualify these environments or behavior |
| Gate outcome | No gate approved or advanced; Task-005 rights/runtime prerequisites remain separate |
| Independent verifier | Project verifier: macOS format, Clippy, 16 synthetic tests, separate private fixture, CLI hash reproduction, feature tree, scope and diff checks PASS; no source edits |
| Reviewer | Project reviewer PASS: all seven files, archive/XML bounds, reports/hashes, scope and whitespace; no Cargo rerun by reviewer |
| Current follow-up | Static inspection scope complete. Later Task-005/007 outcomes are recorded in their current task files; this restored evidence grants no runtime or rights approval. |

The registered `.etsapp` is 1,730,085 bytes with SHA-256
`cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9`.
The inspector observed 112 entries: 108 files and 4 directories, totaling
3,393,880 expanded bytes. The primary DLL has SHA-256
`4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d`.
Its exact archive spelling differs from the manifest only by case. Full output
stays private; concise expected facts are in the owned fixture assertions.

The first fixture run returned FAIL because the selected manifest's UTF-8 BOM
had not yet been handled. The focused owned test
`accepts_selected_utf8_bom_without_accepting_other_encodings` reproduced FAIL
before the correction and passed afterward. A malformed attribute/declaration
regression likewise reproduced FAIL before the profile was tightened. An initial
aggregate-limit test data construction hit ArchiveLimit rather than ExpandedLimit;
its synthetic compression data was corrected and the intended guard passed.
These failures are retained here rather than represented as initial success.

Commands actually executed in the task worktree with the accepted private
`RUSTUP_HOME`/`CARGO_HOME`/`PATH`, `CARGO_TARGET_DIR=private/target`, and the explicit
registered input in `DCA_REFERENCE_INPUT`:

```sh
cargo fetch --manifest-path poc/dca-inspect/Cargo.toml
cargo check --manifest-path poc/dca-inspect/Cargo.toml --locked --offline
cargo test --manifest-path poc/dca-inspect/Cargo.toml --locked --offline accepts_selected_utf8_bom_without_accepting_other_encodings -- --exact
cargo test --manifest-path poc/dca-inspect/Cargo.toml --locked --offline rejects_malformed_attribute_text_and_declaration_ambiguity -- --exact
cargo fmt --manifest-path poc/dca-inspect/Cargo.toml --all
cargo fmt --manifest-path poc/dca-inspect/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/dca-inspect/Cargo.toml --workspace --all-targets --locked --offline -- -D warnings
cargo test --manifest-path poc/dca-inspect/Cargo.toml --workspace --locked --offline
cargo test --manifest-path poc/dca-inspect/Cargo.toml --locked --offline registered_reference_matches_inventory_and_remains_unchanged -- --ignored --exact
cargo tree --manifest-path poc/dca-inspect/Cargo.toml --locked --offline -e features
cargo run --manifest-path poc/dca-inspect/Cargo.toml --locked --offline -- "$DCA_REFERENCE_INPUT" > private/task006-report.txt
python3 scripts/check_project_scope.py
git diff --check
```

The first fetch attempt failed sandbox DNS resolution and was stopped; the
explicitly authorized official-registry fetch then completed. Initial `cargo
check` exposed actual upstream API differences and failed; these were corrected,
and the final Clippy build/test commands above passed. The two focused regression
commands first returned the recorded FAIL; their final cases are included in the
passing full suite. No blanket all-features check, product import, assembly
execution, hardware action, rights decision, publication or commit was performed.
The source increment remains reviewable in only `poc/dca-inspect/**`, this task
record and `docs/DCA-INSPECT.md`.

## Independent verification, 2026-10-21

The project verifier reproduced the listed format, Clippy, workspace tests
(16 PASS, 1 explicitly ignored), separate registered fixture test (1 PASS), CLI
report hash, feature tree, scope and diff checks using a separate target directory
on macOS with Rust 1.99.1. The lockfile and registered input hashes remained
unchanged. It also inspected the duplicate-central-name and valid-entry-overlap
regressions and the production `validate_header_consistency()` call.

At the initial verification, Windows was NOT RUN. Automatic approval review rejected the proposed transfer of
the five owned crate files because explicit user authorization for that source
transfer was not established. No transfer occurred in that attempt. The user
subsequently explicitly authorized transfer and the Windows follow-up below
resolved this blocker.

Final critical review PASS at the original checkpoint: all seven files and required acceptance criteria were
reviewed against actual source and independent verification. The reviewer matched
both private reports byte-for-byte and ran scope and whitespace checks. Cargo was
not rerun by the reviewer. Task-006 is Done for its stated offline inspection
scope. The Windows follow-up below was recorded later; CLR, original DCA hosting
and rights qualification remain outside this task.

## Authorized Windows follow-up, 2026-10-21

The user explicitly authorized the Windows test and source transfer. The project
verifier transferred only the five committed crate files from implementation
`607bd5f760104e1f79799dc524b4df17957ea7e8` and a private owned verification harness.
The source-only ZIP was 14,358 bytes, SHA-256
`6eb41cef79727e5c5c21dbc9833f184ae776bb426a05f8999bc3b2c343bb752d`.
All five member hashes matched before checks and after the completed run; the
lockfile retained its recorded hash. No manufacturer archive or DLL was transferred.

Observed host: Windows 11 Pro, version 10.0.26200, build 26200, 64-bit/AMD64.
The child batch initialized Visual Studio Build Tools 18 using VsDevCmd x64;
Rust/Cargo 1.99.1 reported host `x86_64-pc-windows-msvc`. Source and target were
fresh private temporary directories. Toolchain/cache variables were process-local.
An explicit locked Windows-target dependency fetch succeeded; all subsequent
build/test commands were offline. The bounded wrapper exited 0 with all four
fetch/fmt/Clippy/test success markers. Exact portable command arguments are in
[DCA-INSPECT.md](../docs/DCA-INSPECT.md#windows-follow-up).

- Formatting: PASS.
- Clippy workspace/all-targets, locked/offline, no-default-features, warnings denied: PASS.
- Workspace tests: PASS, 16 passed, 0 failed, 1 explicitly ignored private fixture.
- Post-run source and lockfile hashes: PASS, unchanged.
- Windows registered manufacturer fixture / CLR / vendor execution / hardware: NOT RUN.

Private raw records: `task006-windows-result-verified-stdout.txt` and
`task006-windows-postrun-stdout.txt`, plus corresponding stderr files under the
coordinator's ignored artifacts directory. Setup initially encountered SSH
command-size limits, batch quoting and a temporary archive-name mismatch;
those attempts did not run Cargo and are not counted as passing checks.
Final independent review of this evidence increment is recorded below.

Windows evidence source commit: `107064fef94118e99d9f361d277b70f0746fb7b4`.
The independent project reviewer compared the private result/post-run inventory
against committed source, verified all success markers, counts and hashes, and
ran scope and diff checks: PASS. It did not rerun Cargo. No private machine paths
were found in public additions; no broader qualification was inferred.

## Scoped integration verification — 2026-11-03

The main branch still carried the initial `Not started` record and lacked this
inspector. The five crate files were restored byte-for-byte from accepted source
`607bd5f760104e1f79799dc524b4df17957ea7e8`; the component contract and historical
result record were restored from evidence tip
`685dfbaa91122f423d56e0f6b0b241027b5f7cc8`. No files from the competing
`f308e301839386b2a7ec0489a13f0f3c60598696` implementation were combined with it.
The standalone crate, root dependency graph and original DCA hosting remain
separate. The current Task-003 and Task-004 records were checked Done; this task
has no required phase gate.

On macOS, the restored source was rechecked using the pinned Rust/Cargo 1.99.1
and offline locked cache:

```sh
cargo fmt --manifest-path poc/dca-inspect/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/dca-inspect/Cargo.toml --workspace --all-targets --locked --offline -- -D warnings
cargo test --manifest-path poc/dca-inspect/Cargo.toml --workspace --locked --offline
cargo test --manifest-path poc/dca-inspect/Cargo.toml --locked --offline registered_reference_matches_inventory_and_remains_unchanged -- --ignored --exact
cargo run --manifest-path poc/dca-inspect/Cargo.toml --locked --offline -- "$DCA_REFERENCE_INPUT" > private/reconcile006/report.txt
```

All commands passed. The ordinary suite reported 16 passed and one explicitly
ignored fixture; the separate registered fixture test passed. The private CLI
report is 16,858 bytes and again has SHA-256
`12d69a4b2cf686f05070efdc654f5737761c0c4ad0b09ac7b6649ecc81f31fce`.
The lockfile hash remains
`78fb2d9c993a5e15ad2775b6d12714471134ea0180dde3b9f75824270ba927ba`.
The fixture test verified the original archive hash and unchanged bytes. These
are static-only checks; Windows was not rerun for this integration, and the
earlier Windows result remains tied to its original source and evidence.
