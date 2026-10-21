# Frame length bounds contract and evidence

Task: [Task-146](../tasks/Task-146.md). Status: contract established before code;
implementation, local checks, independent verification and critical review PASS.
Corrective fork revision recorded below. Scope: public raw-frame parsing
in the separate fork; no editor protocol implementation or service validation.

## Existing invariant and intended behavior

At fork baseline `0fc99a9c0d551093c8eea4883c052c75e59ee6c8`,
`KnxIpHeader::LENGTH` is 6. `KnxIpHeader::new` and `serialize` produce that fixed
header. Header parsing permits total lengths below 6, then `KnxIpFrame::parse`
checks only whether the input is shorter than the declared total. A six-byte
header with declared total 0 reaches `data[6..0]` and panics. Totals 1 through 5
have the same invalid slice ordering. This is an existing source invariant;
no standards claim, independent wire vector or trace interpretation is made.

Add one lower-bound guard in frame parsing before slicing, returning the existing
structured ParseError with offset 4 (the total-length field). Leave header parse
and every serialization/service behavior unchanged. Tests construct the header
through the existing typed `ServiceType::SearchRequest` builder then alter the
public total-length field; they do not claim an empty SearchRequest is valid.

All six malformed totals must return structured errors. Raw total 6 keeps an
empty body; total 7 keeps one byte. A total beyond available input still errors;
bytes after the declared total stay ignored. Tests are direct calls, so a panic
fails the old runner without a special panic-catching harness.

## Contribution and boundaries

New Task-146 guard, error documentation, tests and original evidence follow the
project-owned Apache-2.0 policy. Existing MIT upstream source and notices and
package metadata remain unchanged. The fork contribution notice records this
treatment before code changes; this is provenance, not legal certification.
No new dependency, feature, Secure change, hardware contact, socket operation,
manufacturer semantics, API type or protocol service is authorized.

## Reproduction and observed results

Observed 2026-10-21 on macOS 27.0 arm64 / `aarch64-apple-darwin`.
Compiler `rustc 1.99.1 (58cf8f9c3 2026-10-13)`; Cargo
`1.99.1 (5f94df478 2026-08-27)`. Use the repository-local provisioned
`private/toolchains/rustup` as RUSTUP_HOME, `private/toolchains/cargo` as
CARGO_HOME and `private/toolchains/rustup/toolchains/1.99.1-aarch64-apple-darwin/bin`
on PATH. CARGO_TARGET_DIR is ignored `private/build/task146-protocol`.
No network provisioning, dependency/feature/lockfile change or input archive.

Run from the assigned fork worktree; actual commands:

```sh
cargo test --lib --locked --offline --no-default-features --features dpt protocol::knxip::tests::frame_ -- --test-threads=1
cargo fmt --all
cargo fmt --all -- --check
cargo test --lib --locked --offline --no-default-features --features dpt protocol::knxip::tests::connect_request_ -- --test-threads=1
cargo test --lib --locked --offline --no-default-features --features dpt protocol::knxip::tests::connect_response_ -- --test-threads=1
cargo clippy --lib --locked --offline --no-default-features --features dpt -- -D warnings
cargo metadata --format-version 1 --filter-platform aarch64-apple-darwin --locked --offline --no-default-features --features dpt
git diff --check
git diff --exit-code HEAD -- Cargo.toml Cargo.lock LICENSE LICENSE-APACHE rust-toolchain.toml AGENTS.md FORK.md AUDIT.md TEST-RELIABILITY.md tests
```

The first test command ran before and after the guard, and again after format.
Only the explicit filters execute. The five inherited tests were inspected:
they parse/serialize in-memory ConnectRequest/Response fixtures and never create
a socket or run session, discovery or Secure functions. New tests use typed
headers and raw in-code bytes. This is offline synthetic parsing, not a device
simulator or independently sourced protocol vector. No unfiltered test suite ran.

| Check | Observed outcome |
|---|---|
| Old parser with regression tests only | Expected FAIL exit 101: all six malformed totals panic at inherited slice line 645; four boundary/control tests PASS |
| Corrected frame filter | PASS exit 0: 10 passed, 0 failed/ignored; 251 filtered out |
| Audited connect_request_ filter | PASS exit 0: 2 passed, 0 failed/ignored; 259 filtered out |
| Audited connect_response_ filter | PASS exit 0: 3 passed, 0 failed/ignored; 258 filtered out |
| Format | Initial FAIL exit 1 for two new test-layout differences; `cargo fmt --all` corrected them, final check PASS exit 0 |
| Library Clippy with warnings denied | PASS exit 0; this checks library implementation, not every inherited integration-test target |
| Resolved metadata | PASS exit 0: root features exactly `["dpt"]`, 78 package entries, no aes/ctr/sha2/pbkdf2/x25519-dalek/zeroize packages |
| Fork whitespace/protected-path checks | PASS exit 0; upstream notices, manifests, lockfile, toolchain, prior audit and test repair unchanged |
| Editor `python3 scripts/check_project_scope.py` | PASS exit 0: unique IDs, scope and local links; hardware/product/Windows/rights/gate approval NOT RUN |

All final source checks were repeated after rustfmt. Logs are private under
`private/evidence/task146-frame-bounds`: `old-regressions.log`,
`regression-only-knxip.rs`, `regression-only-sha256.txt`, `format.log` (initial
format failure), `format-apply.log`, `format-final.log`,
`fixed-regressions-final.log`, `connect-request-final.log`,
`connect-response-final.log`, `clippy-lib-final.log`, `metadata.json`,
`metadata-stderr.log`, `whitespace-final.log`, `protected-paths.log` and
`source-freeze.json`. Full metadata/private logs are not published.
Regression-only source SHA-256:
`06c747a08096a732f0d14e694f09821ba9cf749ad7cd33e56749fba22a26058e`.
Final `src/protocol/knxip.rs` SHA-256:
`8f0356d5ef432a2cf9a4f16a977c4bdb4136e934701340823fbaaf8c681a2f60`.
These hashes freeze original synthetic input/test source rather than a
proprietary fixture. Corrective fork commit:
`75c31485525eb65e232d325a9dba130897273b24`, committed after independent
verification/review and before the editor record. The editor record revision is
identified by this file's Git history.

No active editor Cargo dependency on knust is introduced or advanced by this
documentation-only editor increment. The reviewed fork was committed first and
its immutable corrective revision is recorded above. No Cargo pin or Task-015
historical audited source was changed.
Task-015 historical audit and Task-141 correction remain unchanged. No phase gate
is required or advanced; management services and Task-016/017 remain blocked by
their own reference/evidence requirements. Hardware/Windows/Linux/MSRV/live
network and protocol interoperability are NOT RUN. Complete service-body
validation, constructor overflow and unrelated malformed input are outside this
increment. This change proves only rejection of raw total lengths below 6.

## Independent verification

Verifier `frame_verify` reports PASS for the same frozen source hash: ten frame
tests, two connect-request tests and three connect-response tests; format,
library Clippy, metadata feature/dependency checks, protected paths, editor
scope/whitespace and fork whitespace. Private command logs are in
`private/evidence/task146-verifier`: `frame_.log`, `connect_request_.log`,
`connect_response_.log`, `fmt.log`, `clippy.log`, `metadata.json`,
`metadata-stderr.log`, `fork-protected-paths.log`, `fork-whitespace.log`,
`editor-scope.log` and `editor-whitespace.log`. The verifier inspected the
implementation's old expected-FAIL log and regression-only source/hash; that
old panic reproduction was not rerun independently. Critical reviewer
`binding_review` subsequently reported PASS for the frozen source, contract and
evidence, with no unresolved findings. The exact corrective fork revision is
recorded above; Task-146 is Done for this bounded offline increment. Agent
verification/review grants no human acceptance or phase-gate approval.
