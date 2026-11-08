<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task154 Glass Lite project/11 structural inspection

Base editor revision: `c81406c`. Task003, Task004 and Task023 are accepted;
Task154 has no phase gate. This increment is offline source inspection and does
not establish physical application identity, fingerprint meaning, compiler
semantics or complete download support.

## Observed input and boundary

The private registered candidate archive SHA-256 is
`c8feaa000ca891df90b455a4ae27de9e1c0e3d3e5cf53e63cf944807f755c7e9`.
The selected member `M-0083/M-0083_A-0090-13-D12F.xml` has SHA-256
`8b4142ac7d695e0d8536525a23781ecd45ddb52650d88d90627f2782eeb272a8`.
The existing independently produced static inventory SHA-256 is
`b6fcc5f66421289bd0f4574ae56e8ada91a3ceed47539f5db32af560775158a5`.
The inventory and the original input remain private. No manufacturer XML or
archive content is included in the tests or source repository.

The observed project/11 product chain selects `BE-GTL4Tx.01`, application
number 144 and version 19 (`0x13`), mask `MV-0705`, ProductProcedure. The
application family/version association does not prove that this application is
installed on the physical device, or interpret the owner's fingerprint `9BDB`.
The existing project/20 profiles reject this distinct application identity.

`InspectionProfile::GlassLiteV13` explicitly selects its namespace, product,
catalog item, hardware-to-program reference, application identity and bounds.
It does not reinterpret project/11 as project/20. Bounds are 2 MiB archive,
4 MiB/member, 16 MiB total, 16 ZIP entries, 50,000 XML nodes and depth 32;
the existing ratio, path, duplicate, CRC and prohibited XML checks remain.
The public Lite entrypoint enforces the exact registered archive SHA-256
after checking cancellation and compressed-size bounds. Matching XML identities
cannot accept different bytes. The structural helper is private and only
crate-internal tests use it for unpinned Lite synthetic fixtures; the private
qualification test uses the public entrypoint and also checks member SHA-256.
This is structural validation, not signature verification or full schema
validation, and no snapshot grants download eligibility.

Narrow observed project/11 additions are TypeText Pattern,
ParameterBlockRename, ParameterSeparator Access and conditional placements.
The existing IR retains raw attributes, immutable source bytes and ranges.
Conditional, assignment and module declarations are additionally retained as
opaque declarations; parameter-type semantics, dynamic activation, unions and
load procedures remain compilation blockers. The rename target is kind/scope
checked. No predicate, module allocation, regular expression or load event is
evaluated. Both absent RAM payloads remain `None`; no zero-fill is inferred.

## Regression and private inventory checks

Seven owned synthetic tests cover public rejection of unregistered matching
structures, deterministic inspection, preserved absence
and opaque declarations, modules, rejection by the current compiler even when
blocker labels are cleared, wrong namespace/product/application/version and
references, unknown critical content, XML external entities, traversal,
archive bound and cancellation. The private qualification test compares:

- 105 parameter types; 698 parameters; 2,893 parameter references.
- 88 communication objects; 971 object references; 239 memory declarations.
- Six segments, of which four have data and two have none.
- Absent payloads: address `0x0700`, size 316; address `0x083C`, size 1.
- Existing encoder rejects the inspected application as UnsupportedReference.

## Executed checks

macOS arm64, pinned Rust 1.99.1, offline locked dependencies and empty default
features. `PATH`, `CARGO_HOME` and `RUSTUP_HOME` selected the repository-private
pinned toolchain/cache. Private paths were supplied explicitly through
`KNXPROD_GLASS_LITE_FIXTURE`, `KNXPROD_GLASS_FIXTURE` and `KNXPROD_FIXTURE`.

| Command | Result |
|---|---|
| `cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features` after archive-pin correction | PASS: 13 unit tests (six existing and seven Task154), 39 existing integration tests; four private tests ignored |
| `cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_ -- --ignored --nocapture` with all three private paths | PASS: new candidate inventory plus three pre-existing private tests |
| `cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings` | PASS |
| `cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check` | PASS |
| `git diff --check` | PASS |

Independent review identified that matching structural fixtures were initially
accepted through the public Lite entrypoint despite the task's exact-archive
contract. The focused public-unregistered-bytes regression was run before the
fix and FAILED as expected. The pin was then enforced at the public boundary,
synthetic grammar tests moved inside the crate, and the same regression and
full suite passed. The public contract is updated in `docs/POC-C-STATIC-IR.md`.

No dependency, Cargo.lock, feature flag, compiler implementation, protocol or
bus capability changed. Windows, hardware, complete XML schema conformance,
physical application match, fingerprint mapping and actual compilation are
NOT RUN or unverified. Task154 acceptance and integration require independent
review by the coordinator; no task/gate is approved by this report.
