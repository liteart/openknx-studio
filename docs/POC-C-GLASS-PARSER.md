# Glass V3.1 static parser evidence

Task: [Task-145](../tasks/Task-145.md). Contract:
[static IR v2](POC-C-STATIC-IR.md). Observation date: 2026-10-21 UTC.
Implementation, independent verification and critical review PASS; the
coordinator accepts this bounded increment. No phase gate or Task-023
acceptance is advanced.

## Readiness and source

Baseline Task-144: `0856e71`. The reviewed bounded Task-023 parser source
`8cdcb569fc224fd4bddaa21f38d9aacc7a5d44c0` was locally cherry-picked without
conflict as `baaa788fb6293b04c054d001a51f854b7ebe56fb`. The reviewed reference
investigation source is `8c05df2517bf249d073abaec44efc26a40040e6a`.
Task-023 remains Blocked on AC-002; Task-024 remains locked. This independent
static profile needs no phase gate. Reviewed source commit:
`426110e6cc958bd87043906b51842b9817ee16aa`.

| Frozen owned file | SHA-256 |
|---|---|
| src/lib.rs | `9b446df20ff87f32415a6d285a8aa957872d34e983de561ffbeb4559c115f8d8` |
| src/archive.rs | `03710e510b82a390d0e6ce0fe7fef32d2d99c7dcb1b46db5265fd84935532975` |
| src/xml.rs | `76080c143e16453383b2ea5719edc2f8d8e04aebf6b16dc8a81946a777bd2616` |
| tests/parser.rs | `76bb259e07490176193943fc2389b2f8860ada7972c7f99fc644368354ec4c72` |
| Unchanged Cargo.lock | `b4a04ff0e1f3003ee0304fdb3b69161dc113cc9f66ee920c57e04264a5c6310f` |

Paths in the table are relative to `poc/product-download/`. No dependencies,
feature flags, toolchain pins or lockfile were changed.

## Private input and independent observation

Original manufacturer bytes remain private and immutable; no archive/XML/image
is committed. Acquisition and source provenance were reviewed in the reference
investigation. Owner-assumed private use does not establish redistribution rights.
The registered generic family link does not prove an actual BE-GT20S.02S target,
firmware, mask/runtime security mode or complete image.

| Input or observation | Value |
|---|---|
| Private archive filename | MDT_KP_BE_02_Glass_Push_Button_II_Smart_V31.knxprod |
| Archive bytes / SHA-256 | 900,549 / `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d` |
| ZIP entries / expanded bytes | 57 / 21,193,051 |
| Selected member | M-0083/M-0083_A-018B-31-369F.xml |
| Member bytes / SHA-256 | 9,861,680 / `f5bfbf9edb0ec799d2182f6962b20195b72d19268bbbe4c7dbe3458b7c0e0f6e` |
| XML elements / maximum observed depth | 90,988 / 27 |
| Application / number / version | M-0083_A-018B-31-369F / 395 / 49 |
| Namespace / mask / load style | project/20 / MV-0705 / ProductProcedure |
| Family product | M-0083_H-3.2E38-1_P-BE.2DGT20x.2E02 |
| Hardware-to-program | M-0083_H-3.2E38-1_HP-018B-31-369F |
| Catalog item | M-0083_H-3.2E38-1_HP-018B-31-369F_CI-BE.2DGT20x.2E02-1 |

The distinct temperature application `M-0083_A-018A-31-EEE8` is rejected.
The coordinator independently reproduced the archive/member pins and structural
counts using bounded Python ZIP/XML inspection before implementation.

## Behavior and test oracles

Only the explicit `GlassV31` profile receives the separate 2 MiB compressed,
12 MiB/member, 32 MiB expanded, 64-entry, 100,000-element bounds; depth remains
32 and compression ratio 64. Original default identity/API and bounds remain.
Known Glass-only attributes/placements are narrowly allowed; unknown critical
shapes still fail closed. Internal ASCII spaces are allowed only within a flat
Glass baggage PNG filename. No archive path is extracted.

IR v2 preserves anonymous union source identities, ordered direct members,
member attributes/ranges, parameter membership, module scope and typed memory
ownership. Missing ModuleDef IDs now fail as metadata errors rather than being
mistaken for global scope; empty IDs retain the existing duplicate-ID guard.
This malformed-input bug fix applies to both profiles. Rename targets are
kind-checked and must share module scope. Opaque picture/baggage/rename and load
attributes remain unverified and blocked.

| Inventory oracle | Observed Rust result |
|---|---:|
| Parameter types / parameters / parameter references | 213 / 1,174 / 2,920 |
| Communication objects / references | 89 / 650 |
| Module definitions / union declarations | 2 / 98 |
| Memory declarations: union / parameter owners | 539: 98 / 441 |
| Opaque picture / baggage / rename declarations | 48 / 45 / 12 |
| Load steps | 22 |
| Unverified module-to-global condition edges | 7,310: MD-1 3,706 / MD-2 3,604 |

The 7,310 edges are only `choose/@ParamRefId` from a module to an existing global
ParameterRef. ID and kind checks run first. Exact source identity, node,
attribute, target and both scopes are exposed in typed unverified records and
an `UnverifiedGlobalCondition` blocker. Other cross-scope edges, missing/wrong
kinds, cross-module targets and all default-profile crossings are rejected.
No condition/reference scope semantics are claimed.

The six declared segment lengths are 513, 511, 666, 656, 1 and 1,968 bytes;
Data lengths are Some(513), Some(511), None, Some(656), None and Some(1,968).
Absent bytes are never zero-filled. All parameter/union Memory CodeSegment
references target AS-4690 when present. Raw custom load ranges remain opaque;
Task-023 AC-002 resource completeness is still INCONCLUSIVE.

## Reproduction and results

Platform: macOS 27.0 build 26A428, Darwin arm64; provisioned Rust 1.99.1.
Use repository-local `private/toolchains/rustup` as RUSTUP_HOME,
`private/toolchains/cargo` as CARGO_HOME, and the pinned toolchain's `bin` on
PATH. Set CARGO_TARGET_DIR to an ignored private build directory. Explicitly set
KNXPROD_FIXTURE to the original registered binary-input archive and
KNXPROD_GLASS_FIXTURE to the private Glass archive; no absolute local path is
stored in source. Both fixture tests are ignored by default.

Actual final commands, from the assigned worktree:

```sh
cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_ -- --ignored --nocapture
python3 scripts/check_project_scope.py
git diff --check
shasum -a 256 poc/product-download/src/lib.rs poc/product-download/src/archive.rs poc/product-download/src/xml.rs poc/product-download/tests/parser.rs poc/product-download/Cargo.lock
```

| Check | Result |
|---|---|
| Format / Clippy warnings denied | PASS |
| Synthetic suite | PASS: 32; two private tests ignored |
| Explicit original and Glass private inventories | PASS: 2 |
| Planning scope / local links / task IDs / whitespace | PASS |
| Independent verification | PASS: verifier reproduced format, Clippy, 32 synthetic and both private inventories |
| Independent critical review | PASS; no unresolved findings |
| Windows, DCA, compiler, image download, hardware | NOT RUN |
| Resource omission or global-condition semantics | INCONCLUSIVE |

Initial real Glass inspection reproducibly failed with ArchivePath because 12
PNG leaves contain spaces, then Reference because of the module/global edges.
The narrow observed allowances above corrected these inspection failures while
preserving explicit uncertainty. An initial synthetic element-limit positive
used highly repeated Deflate data and tripped the unchanged ratio guard; the
owned Glass test builder now stores its small XML to isolate the element bound.
Initial tool invocations without the provisioned Cargo/RUSTUP_HOME environment
could not run; final commands used the pinned offline environment.

The original private archive also retains its exact SHA-256
`51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709`
and expected 95 types, 266 parameters, 1,016 parameter references, 16 objects,
301 object references and six segments. Every successful snapshot retains
nonempty compilation blockers. No vendor code was executed, network contacted,
hardware operated, input redistributed, license changed, gate approved or
publication performed. The reviewed bounded diff does not resolve the separate
semantic/profile resource dependencies or unlock the compiler tasks.

## Task-023 external baggage validation, 2026-10-22

The bounded follow-up uses source baseline
`54fff13475b7b4b91942f96859b5cff36978f370` on the assigned
`task/Task-023-resource-qualification` branch. Task-003/004 prerequisite evidence
and the accepted Task-145 inspection profile remain the basis for offline work;
no phase gate is required. This is an uncommitted implementation handoff pending
independent verification, critical review and coordinator acceptance. Task-023
is not accepted by this record.

The original project/20 schema slice identified in Task-023 confirms optional
segment Data; absence is retained as None. The same slice declares required
Baggage Id/TargetPath/Name and FileInfo. The broader optional FileIntegrity and
FileInfo fields are not interpreted or enabled. The observed input has only
FileInfo TimeInfo, no declared payload size/checksum and empty TargetPath in
every declaration. The mapping below is selected-reference evidence, not a
universal baggage format claim.

IR version 3 now rejects dangling external references before inspection success.
It requires the observed manifest hierarchy and exact ID-to-Baggage kind checks,
validates each empty-TargetPath/flat-PNG declaration, rejects duplicate or
case-ambiguous mappings and retains immutable payload bytes, lengths and SHA-256.
Each application reference retains its exact target, module scope and source
range/hash. The [static IR contract](POC-C-STATIC-IR.md) defines the public fields
and supported subset. Default-profile behavior is unchanged apart from version 3
and empty added fields. Glass with no external references needs no manifest.

The independently observed private inventory is
`private/evidence/task023-resource-inventory/smart-glass-baggage-inventory.json`;
the separately reproduced vector is recorded in `resource-vector-digest.json`
in that directory. Neither file contains original PNG/XML payload bytes.

| Private input or oracle | Value |
|---|---|
| Archive SHA-256 | `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d` |
| Baggages.xml bytes / SHA-256 | 9,400 / `550d7f910f7091a5cfc78c9dc4bfe3bd7ce08fb7f934f28d3846212325e0d6e1` |
| TypePicture / Baggage application references | 48 / 45 |
| Unique manifest IDs / payloads | 48 / 48 |
| Total payload bytes | 56,378 |
| Sorted resource vector bytes / SHA-256 | 7,090 / `97fb12d42558d742119cc098cfdcd8ec693f29accf1fa00222db51fd9c9959ab` |

The vector has one UTF-8 row per manifest ID, sorted by exact ID:
`id TAB member TAB decimal_length TAB lowercase_sha256 LF`. The ignored private
fixture test compares this independent digest, covering every payload identity,
length and hash without committing manufacturer content. It also checks the
manifest pin and all 93 retained application references. The existing selected
XML pin, segment Data lengths, opaque declarations and blockers still match.

Initial regression command (expected FAIL, exit 101):

```sh
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features glass_external_reference_requires_baggage_document
```

It observed an Ok snapshot for a synthetic dangling baggage reference instead of
MissingMember before implementation. Final commands used the same pinned local
toolchain environment described above, with isolated ignored target directory
`private/targets/task023-resource-implementation`:

```sh
cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_glass_private_fixture_matches_independent_inventory -- --ignored --nocapture
python3 scripts/check_project_scope.py
git diff --check
shasum -a 256 poc/product-download/src/lib.rs poc/product-download/src/resources.rs poc/product-download/tests/parser.rs poc/product-download/Cargo.lock
```

Platform: macOS 27.0 build 26A428, Darwin arm64. Explicit
KNXPROD_GLASS_FIXTURE selected the registered private Glass archive. Format and
Clippy PASS; synthetic tests PASS (38, two private tests ignored); explicit Glass
private inventory PASS (one). Scope and whitespace checks PASS. The original
BE-04000.02 private fixture was NOT RUN for this increment. Windows, DCA, vendor
execution, compiler, image download and hardware are NOT RUN. No dependencies,
features, toolchain pins or lockfile changed.

| Frozen implementation file | SHA-256 |
|---|---|
| src/lib.rs | `f02113761139b97b9e2f75f6231423541de8550294bebe37d0c0a69f04cd8521` |
| src/resources.rs | `cb941e2dc57dda13568c099111dbabb82ded8dca522ec66eafced74dd18d95c3` |
| tests/parser.rs | `1ffae0523e8534ea20942f7378446a2fdd61b535e05b9c90fc54c187481f3310` |
| Unchanged Cargo.lock | `b4a04ff0e1f3003ee0304fdb3b69161dc113cc9f66ee920c57e04264a5c6310f` |

Paths are relative to `poc/product-download/`. Technical outcome: PASS for the
bounded static baggage reference/payload inventory. Acceptance remains with the
coordinator after independent verification and review. No claim is made about
PNG validity, vendor authentication, UI interpretation, image compilation,
initial RAM contents, load-procedure semantics or real device compatibility.
All compilation blockers remain, including 93 external-resource occurrences,
12 rename occurrences and 7,310 unverified global-condition edges. No gate or
downstream task is unlocked by this implementation handoff.
