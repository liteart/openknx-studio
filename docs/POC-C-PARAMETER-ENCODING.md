# Staged DisplayTimeout byte encoding

## Current original-reference qualification, 2026-10-23

Task-024 is **Blocked** on verified parameter activation and module-allocation
semantics or a reviewed independent reference compilation. The 1,968-byte
AS-4690 baseline Data is present in the pinned archive; obtaining the raw
bytes is not the missing prerequisite. The candidate byte diff remains an
observation, while original-reference encoding stays fail-closed.

KNX's [Project Scheme v1.0.0 documentation, section 1.1 p4](https://support.knx.org/hc/en-us/article_attachments/360024169360)
explicitly omits the producer-side parameter/dependency/download-image rules.
It identifies ETS5/MT5 5.7 and project/20, matching this archive's namespace.
The [official SDK overview](https://support.knx.org/hc/en-us/articles/360015340940-How-to-modernize-your-App)
provides only high-level activation/allocator descriptions. Targeted searches
of the supplied device API, Configuration Procedures and XML Data Encoding
texts do not establish BaseOffset/NumericArg/Allocates units or choose/when
evaluation. Private `task024-primary-additional/extraction-record.json`
binds three successful bounded PDFKit text extractions; Task-024 records the
source scope and precise missing input. No new compiler or hardware test was
run, and no partial byte vector is promoted to qualified output. Independent critical documentation review PASS; no unresolved documentation finding. Task-024 remains Blocked; source acceptance, acceptance criteria and gates are unchanged. Private review evidence: `task024-primary-additional/review/critical-review.txt`.

## Partial byte-core handoff (historical)

Task: [Task-024](../tasks/Task-024.md). Source baseline:
`8fc4eb51d3c514432637f095a9592b3dc62cb08f`, branch
`task/Task-024-display-timeout-encoding`. Task-023 is accepted for structural
inspection at this baseline, including resource source
`3a1d5db5609c7c4aee4997b5127e1858a58c5426`. No phase gate is required for this
offline increment. Task-024 remains In progress; all acceptance criteria remain
pending. This record is an uncommitted handoff for independent verification and
review, not task acceptance or a working original-product compiler claim.

## Evidence and qualification boundary

The private candidate record is
`private/evidence/task024-parameter-candidate/display-timeout-candidate.json`,
SHA-256 `41c03666917df5a249a9a7b3958a79da41c6ea317556bcdc88a28a82792d122a`.
It includes the independently observed candidate, source pins, conditions,
memory declarations, module inventory and primary-source limitations. No
manufacturer XML, segment bytes, DLL or image is committed.

| Observed selected candidate | Value |
|---|---|
| Archive SHA-256 | `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d` |
| Application | `M-0083_A-018B-31-369F` |
| Application member SHA-256 | `f5bfbf9edb0ec799d2182f6962b20195b72d19268bbbe4c7dbe3458b7c0e0f6e` |
| Parameter | `M-0083_A-018B-31-369F_P-252` / DisplayTimeout |
| Type | unsignedInt, 8 bits, minimum 0, maximum 60 |
| Segment | `M-0083_A-018B-31-369F_AS-4690` |
| Segment Data bytes / SHA-256 | 1,968 / `66d62fbcc9a0a32c8475a69bb7ce3c1ac695c62718741c599c146fb54b26cc30` |
| Byte offset / bit offset | 1522 / 0 |
| Definition default / observed byte | 20 / `14` hex |

The static byte observation is insufficient to encode this original product.
Selector P-2336_R-113 has a test-0 branch containing P-252_R-114 and a test-1
branch containing P-252_R-116 with Access=None and Value=0. Exact choose/when,
reference override and activation semantics remain unqualified. All 78
module-relative memory widths and 18 module instances are inventoried, but
BaseOffset/NumericArg/Allocates arithmetic and units are not established by the
reviewed schema/specifications. Conditional additive arithmetic is a hypothesis,
not proof of non-overlap. This increment does not interpret those constructs.

`encode_display_timeout(snapshot, value, cancelled)` is the sole public original
entry point. It first checks cancellation and the 0..60 unsigned value bound.
Other archive/application identity labels return UnsupportedReference. Even
matching labels always return UnverifiedReferenceContext: no original-product
input can currently produce bytes. Mutable snapshot labels, clearing parser
blockers and caller assertions cannot unlock encoding. There is no public
layout constructor, proof boolean, fixture bypass or approval path. The
registered original fixture is tested against this rejection boundary.

A future reviewed implementation of the private qualifier must supply actual
immutable baseline bytes and a verified layout before the public path can reach
the byte encoder. That success path is currently unreachable. The existing
parser, resource bounds, optional Data=None and all compilation blockers remain
unchanged. Qualified activation, module occupancy, complete-image construction,
load controls and device operation are not supplied by this increment.

## Private byte core and owned fixture

The production-compiled private core accepts only a checked full unsigned byte:
bit offset 0, width 8 and value 0..60. It verifies declared/actual segment size,
checked byte/bit arithmetic, segment bounds and non-overlap with all supplied
other occupied bit ranges. Unsupported layouts and malformed occupied ranges
fail. Fixed bounds are 1,968 baseline bytes and 539 other occupied ranges.
Cancellation is checked before work, during occupancy checks and before return.
No endianness choice is needed for one byte; partial-bit, signed and multibyte
encodings are unsupported. There are no reserved bits inside the selected full
byte; every other byte, including adjacent bits, is preserved exactly.

The encoder copies the immutable baseline and returns an OfflinePartialSegment
with baseline/encoded SHA-256, bytes and either zero changes or one ByteChange.
Offsets are segment-relative, not device addresses. No output is a download
plan, device image or transport command. Errors have stable localization keys
and English fallbacks. No dependency, feature, toolchain or lockfile changed.

Unit tests construct a private layout for an original owned fixture: bytes
0 through 63, with byte 17 replaced by 20. This is distinct from the private
manufacturer input and carries no manufacturer qualification. The independent
Python hashlib observation used this construction:

```python
baseline = bytes(range(64))
baseline = baseline[:17] + bytes([20]) + baseline[18:]
changed = baseline[:17] + bytes([21]) + baseline[18:]
```

| Owned vector | Expected SHA-256 |
|---|---|
| Default 20, unchanged 64 bytes | `5a26f8719cbf3605d84c13001ae2e33e37467513e94d88d3b259151547a559a1` |
| Changed 21, only offset 17 differs | `eeb4621685ecfe7466df45f79c1935ea747bfb9cf243f13c1b7d3e634c568ea9` |

Tests were added with the new implementation; there was no pre-existing encoder
API whose absence is claimed as a historical regression. The reproducible
original-input observation remains an immutable byte value plus unresolved
activation/occupancy, now enforced as an explicit public rejection. Synthetic
tests cover default/changed vectors, deterministic bytes/hashes/diff, min/max,
invalid values, unsupported bit widths/offsets, bounds, arithmetic overflow,
overlap, unrelated-byte preservation, fixed limits and cancellation.

## Reproduction and results

Platform: macOS 27.0 build 26A428, Darwin arm64; pinned Rust 1.99.1. Commands ran
from the assigned worktree using provisioned `private/toolchains/rustup` as
RUSTUP_HOME, `private/toolchains/cargo` as CARGO_HOME and its bin directory on
PATH. CARGO_TARGET_DIR was the isolated ignored
`private/targets/task024-display-timeout-encoding`. The original qualification
test explicitly selected the pinned private Glass archive with
KNXPROD_GLASS_FIXTURE. No network or vendor code was used.

```sh
cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_glass_display_timeout_context_rejects_original_encoding -- --ignored --nocapture
python3 scripts/check_project_scope.py
git diff --check
shasum -a 256 poc/product-download/src/lib.rs poc/product-download/src/parameter_encoding.rs poc/product-download/tests/parser.rs poc/product-download/Cargo.lock
```

| Check or capability | Outcome |
|---|---|
| Format / Clippy warnings denied | PASS |
| Owned byte-core unit tests | PASS: 6 |
| Parser and public boundary synthetic tests | PASS: 39; three private tests ignored |
| Explicit pinned original qualification rejection | PASS: one negative fixture test |
| Original-reference byte encoding | INCONCLUSIVE / unsupported; no bytes emitted |
| Scope and whitespace checks | PASS |
| Independent verification / critical review | NOT RUN at implementation handoff |
| Both older private parser inventory tests | NOT RUN for this increment |
| Windows/Linux execution, DCA, complete image, hardware | NOT RUN |

| Frozen implementation file | SHA-256 |
|---|---|
| src/lib.rs | `58676bf157d93acc322a96f91d102ae76b72cbe0b6ba1cf27ded8031e4382054` |
| src/parameter_encoding.rs | `b209a67f9b8288453d061b3657a3ec38a068033a7c3880eca365d8f5260c2926` |
| tests/parser.rs | `f5a73c6274ee80ff6b6280a67302a5033f2c818ebd4097010a25352a43672606` |
| Unchanged Cargo.lock | `b4a04ff0e1f3003ee0304fdb3b69161dc113cc9f66ee920c57e04264a5c6310f` |

File paths in the table are relative to `poc/product-download/`. All Task-024
acceptance criteria remain pending: no reviewed original-reference output exists
for AC-001/002, and the bounded rejection tests do not establish the complete
AC-003 image-construction contract. The next dependency is evidence qualifying
the exact activation context and complete occupancy, followed by review of any
qualifier implementation. No gate or downstream task is unlocked.


## Reviewed partial source, 2026-10-22

The bounded increment is committed at `11619d9a057e4abdaf0ea86318a2e096c2d755e9`.
Independent verification reproduced formatting, Clippy with warnings denied,
six unit tests and 39 synthetic integration tests PASS (three private tests
ignored by default), then all three registered private tests PASS: the original
and Glass Task-023 inventories plus original encoding rejection. The original
archive remains `51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709`;
Glass remains `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d`.
Exact commands, five-path hashes, platform and logs are in
`private/evidence/task024-parameter-encoding/verifier/`.

Critical review PASS reproduced formatting, six unit and 39 integration tests,
scope and whitespace checks and inspected the independent Clippy/all-three-private
PASS logs; the reviewer did not rerun those private tests. No source finding
remains. This supersedes only the handoff's pending verification/review and
older-private-inventories NOT RUN. Original encoding remains unsupported, all
Task-024 criteria remain pending, and status remains In progress. The core's
owned-fixture PASS and the original-input rejection PASS are different claims.
No complete image, Windows/Linux execution, DCA/vendor runtime, hardware or
phase GO is established. Task-025 remains dependent on Task-024 completion.


## Current-main reconciliation, 2026-11-03

The reviewed partial source and historical blocker evidence above were restored
selectively on base `1457d78`; the current Task-023 resource implementation was
preserved. All three frozen implementation hashes and Cargo.lock still match
the table above. Fresh offline checks PASS: formatting, Clippy with warnings
denied, six unit tests, 39 synthetic integration tests, and three explicit
registered private tests (both inventory fixtures and original encoding
rejection). Scope and whitespace checks also PASS. See the current
[Task-024 reconciliation record](../tasks/Task-024.md) for exact commands,
environment and limitations. Independent integration review is pending; the
historical review does not accept the current integration. Task-024 remains
Blocked and no original bytes, complete image or gate qualification is claimed.
