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
