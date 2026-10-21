# PoC C selected product parser: bounded static inspection

Task-023 adds the standalone Rust library in `poc/product-download`. Its public
`inspect_archive` function returns a small explicit static declaration model and
compilation blockers. It has no compiler, configuration evaluator, network API,
archive extraction, persistence, DCA host, or device operation. This increment
is **PASS for offline static inspection; INCONCLUSIVE for a compilation-ready
selected application**. Independent verification and critical review PASS for this bounded increment.
Task-023 remains Blocked on AC-002 resource-completeness evidence; downstream
semantic evaluation/compiler gaps are separate obligations.

## Readiness and source

The assigned baseline is `be5b409` on
`task/Task-023-selected-product-parser`. Task-003 is recorded Done in its final
acceptance record (source `be5b409`, subsequent evidence `a764f26`); Task-004 is
Done. No phase gate is required for this task. This does not qualify a physical
device or grant hardware access. The reviewed implementation revision is recorded in Task-023 following the
local source commit.

The applicable contracts are [AGENTS.md](../AGENTS.md), [GOALS.md](../GOALS.md),
[ARCHITECTURE.md](ARCHITECTURE.md), [DATA-MODEL.md](DATA-MODEL.md), and
[Task-023](../tasks/Task-023.md). No separate product component contract exists
in this baseline. The [input inventory](REFERENCE-INPUTS.md) registers the
private original; no manufacturer archive, XML, script, or resource is copied
into source control. Synthetic tests are independently authored minimal XML.

## Observed selected identity

The private registered archive is `MDT_KP_BE_02_Binary_Input_V32a.knxprod`,
393923 bytes, SHA-256
`51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709`.
The actual archive has nine members and four application alternatives.
Only the following candidate is selected:

| Identity | Observed value |
|---|---|
| Catalog item | `M-0083_H-179-0_HP-00B3-32-723A_CI-BE.2D04000.2E02-1` |
| Product | `M-0083_H-179-0_P-BE.2D04000.2E02`; order `BE-04000.02` |
| Hardware-to-program | `M-0083_H-179-0_HP-00B3-32-723A` |
| Application/member | `M-0083_A-00B3-32-723A`; `M-0083/M-0083_A-00B3-32-723A.xml` |
| Number/version | 179 / 50 |
| Namespace | `http://knx.org/xml/project/20` |
| Mask / load style | `MV-0705` / `ProductProcedure` |

Catalog and hardware references must connect exactly, including a common
hardware owner, and the application member must contain exactly one matching
application. Alternate application IDs and conflicting identity metadata fail.
Unused root `xsi`/`xsd` namespace declarations are accepted only with their exact
observed standard URIs. Prefixed elements/attributes, namespace rebinding,
unexpected application elements/attributes, and unknown load controls fail.
This is observed format support, not an authoritative schema implementation,
a manufacturer signature verification, or proof of non-secure device support.

The IR preserves immutable selected source bytes/hashes, source ranges, IDs,
module scope, original attributes and order. It includes 95 parameter types,
266 parameter declarations, 1016 parameter references, 16 object declarations,
301 object references, 67 memory declarations, six segments, three tables, and
ordered declared load-control metadata. Type representations and default values
remain declarations, without evaluating ranges, overrides, scripts or branches.
Known parameter/object/dynamic/argument references are validated for target
kind and scope; ancillary translation references are retained without semantic
resolution. The original XML remains available for unsupported semantics.

| Selected source | Bytes | SHA-256 |
|---|---:|---|
| `M-0083/Catalog.xml` | 14854 | `67b07512e9359403fe97d1f2255e9ae25d66eb0a1c16a3127ba4acab2c341431` |
| `M-0083/Hardware.xml` | 16204 | `8de875c288b4e6cf6e47efc04c625ee0bcd9b472adf81485dd00ed6af274ffa8` |
| Selected application XML | 1264170 | `dcf00cc75720b44040c4c7482e67332487c518e330851e088644224321742f3a` |

## Resource and offset observations

Declared segment addresses/sizes and embedded resource bytes are preserved.
Base64 is strictly decoded, including padding bits. A resource cannot exceed
its segment's declared size; absence/short data is retained without inferred
zero fill. Exact embedded hashes are independently calculated with Python's
`zipfile`, `ElementTree`, `base64`, and `hashlib`, then asserted by the opt-in
Rust fixture test. No extraction or input modification occurs.

| Segment suffix | Declared address | Declared size | Data length | Mask length |
|---|---:|---:|---:|---:|
| `AS-4000` | 16384 | 513 | 513 | 513 |
| `AS-4201` | 16897 | 511 | 511 | absent |
| `AS-0700` | 1792 | 136 | absent | absent |
| `AS-4400` | 17408 | 152 | 152 | absent |
| `AS-0788` | 1928 | 1 | absent | absent |
| `AS-4498` | 17560 | 270 | 270 | absent |

The communication-object table declares `AS-4400` offset 0; address and
association tables declare `AS-4000`/`AS-4201` offset 0 and max entries 255.
Segment references, declared offsets and bit offsets are checked; full
parameter widths, union overlays, module allocation and table encodings are
not resolved. Segment address plus size is checked for integer overflow.
These declarations are not a writable-memory allowlist or a load plan.

## Explicit compilation gaps

Every successful inspection reports nonempty typed compilation blockers.
The actual fixture contains three `ModuleDef`, ten `Module`, 214 `choose`,
640 `when`, 15 `Assign`, 23 `Union`, two JavaScript `ParameterCalculation`,
one `Script`, and four `Dynamic` elements. There are 1082 `ParameterRefRef`
and 330 `ComObjectRefRef` declarations. All original source is retained;
counting these constructs does not resolve their semantics. No product XML
code is evaluated.

The blockers also identify uninterpreted parameter-type semantics and an
unverified product load procedure. The application is not ready for Task-024
image compilation. In particular, there is no authoritative interpretation
for the absent segment Data, complete effective parameter/object activation,
module allocation, calculations, or the exact profile load sequence. A
separate bounded semantic/profile follow-up and selected-device evidence are
required; this task does not silently implement the next compiler task.

## Limits and input safety

The fixed limits are 2 MiB archive, 16 indexed members, 2 MiB per expanded
member, 8 MiB total expanded bytes, compression ratio at most 64, 240-byte
ASCII member names, 20000 XML elements, depth 32 and 64 attributes per element.
ZIP metadata is checked with the pinned library's public
`validate_header_consistency`: duplicate central names, divergent local
headers and overlapping entries are rejected before traversal. Every member,
including unselected alternatives, is read to EOF with CRC checks and bounded
chunks. Traversal, absolute/drive/backslash paths, symlinks/special files,
case collisions, noncanonical components and file/directory shadowing fail.
No paths are materialized on disk.

Only UTF-8/XML 1.0 is accepted. DTDs, processing instructions, external/unknown
entities and namespace changes are rejected. There is no XML resource resolver
or network client. Cancellation is checked before archive work, each member,
each read chunk, each XML event, and each IR node. Base64/hash operations are
bounded by the input limits. The synchronous library is suitable for an
offline PoC; a later async caller must schedule this CPU work appropriately.

Typed errors expose stable localization keys and English fallback text. This
PoC adds no production localization framework, command shell or general UI.
Project-owned code forbids unsafe Rust; dependencies are not claimed unsafe-free.

## Dependencies and reproducible commands

The standalone manifest inherits the repository Rust 1.99.1 pin, sets MSRV
1.99.1, has an isolated `[workspace]`, and commits its own `Cargo.lock`.
No root manifest/lockfile or fork code is changed. Direct dependency primary
sources inspected in the provisioned official Cargo registry cache:

| Dependency | Enabled features | Declared license | Source/API reason |
|---|---|---|---|
| `zip = 9.0.5` | defaults off, `deflate-flate2-zlib-rs` | MIT | bounded in-memory ZIP/CRC/header inspection; `src/read/zip_archive.rs`, `src/read/zipfile.rs`, actual `LICENSE` |
| `quick-xml = 0.42.0` | defaults off | MIT | UTF-8 pull events/attribute normalization; `src/reader/slice_reader.rs`, `src/events/attributes.rs`, actual `LICENSE-MIT.md` |
| `sha2 = 0.11.0` | defaults off | MIT OR Apache-2.0 | archive/resource SHA-256 identity; actual manifest and `LICENSE-MIT`/`LICENSE-APACHE` |

Their declared MSRVs are 1.88, 1.86 and 1.85 respectively. The locked closure
contains 22 dependencies with MIT/Apache-2.0, Unlicense OR MIT, and Zlib
licenses; the actual metadata/feature graph was inspected. ZIP activates only
the selected deflate implementation, with `flate2`/`zlib-rs`; no archive AES,
other compressors, KNX dependency, Secure or network dependency is enabled.
Platform runtime claims remain limited to the actual macOS run.

Run from the repository root with the already provisioned pinned toolchain and
an isolated target directory. The implementation session used the ignored
repository-local `private/toolchains/{rustup,cargo}` and
`private/targets/task023`; these are not shipped provisioning requirements.
`KNXPROD_FIXTURE` must explicitly name the registered private original.

```sh
cargo generate-lockfile --manifest-path poc/product-download/Cargo.toml --offline
cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_private_fixture -- --ignored --nocapture
cargo metadata --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features --format-version 1
python3 scripts/check_project_scope.py
git diff --check
```

Observed on 2026-10-21, macOS 27.0 arm64, Rust/Cargo 1.99.1:

| Check | Actual outcome |
|---|---|
| Initial owned regression `selected_static_application_can_be_inspected` | FAIL as expected before implementation; exit 101 |
| Duplicate central-name regression | FAIL as expected before explicit ZIP header validation; duplicate selected-app entries were accepted; corrected with the library's public consistency check |
| First private fixture run | FAIL: exact unused root xsi/xsd declarations were initially rejected; narrowed observed declarations added, no namespace downgrade |
| Lock generation | PASS, 22 dependencies pinned offline |
| Initial Clippy | FAIL, three style lints; corrected without suppressions |
| Source-range regression `source_ranges_include_utf8_bom_offset` | FAIL before correction: reader byte positions omit UTF-8 BOM; explicit source offset fixes owned and actual-fixture range assertions |
| Final formatting / Clippy / default tests | PASS, exit 0; 22 owned tests passed, private fixture ignored by default |
| Explicit private fixture | PASS; exact identity, counts, embedded hashes and lengths match independent inspection |
| Scope and diff checks | PASS, exit 0 |
| Independent verifier / reviewer | PASS for bounded increment; verifier reproduced checks and both test runs; reviewer reran existing test binary and compared source hashes; see Task-023 |
| Windows / Linux parser execution | NOT RUN; no corresponding runner assigned |
| Hardware, original DCA, compiled image, application/device match | NOT RUN; outside this offline increment and required target/profile inputs missing |

No human acceptance, phase GO, rights approval or physical operation is asserted.
The reviewed source revision is recorded in Task-023. Next prerequisite:
resolve AC-002 with authoritative evidence for selected-profile resource
completeness, independently of later evaluator/compiler implementation.
