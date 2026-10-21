# Checked frame constructor contract and evidence

[Task-147](../tasks/Task-147.md). Contract established before code changes.
Status: bounded implementation, local checks, independent verification and critical review PASS.
Corrective fork commit: `4b0d4a5be6334e161989df6f24588da3b56b8666`.
The editor record revision is identified by this file's Git history.

## Existing invariant and API decision

Fork baseline `75c31485525eb65e232d325a9dba130897273b24` uses a six-byte fixed
header and a u16 total-length field. Therefore at most 65529 body bytes fit.
`KnxIpHeader::new` currently adds six in u16; body length 65530 panics with
checked overflow and wraps total to zero in release. `KnxIpFrame::new` casts
body.len() to u16 first: 65536 body bytes become a declared body length zero.
This follows existing Rust source and numeric bounds; no new protocol rule,
manufacturer behavior, trace or standards interpretation is introduced.

Change both existing public `new` constructors to return the existing Result.
Header accepts usize, uses checked addition and checked u16 conversion, and
returns ProtocolError::InvalidFrame when the complete length does not fit.
Frame passes body.len() directly to Header; neither narrows unchecked. Document
the explicit local API break; no silent compatibility constructor. Public fields
and serialize remain unchanged: callers can still construct/mutate fields, so
this task guarantees constructor-produced frames only, not arbitrary public
struct literals or manual mutation. Total input allocation policy is separate.

## Call sites and component boundary

The fork owns all changes. Existing Result-returning tunnel/routing/probe/server
paths propagate errors with ?. Internal byte-building helpers become Result;
test-only helpers/calls may deliberately unwrap owned tiny fixtures. Application
background send paths route construction errors through their existing send-error
logging path, without sending invalid bytes or claiming success. The sequence
example propagates through its existing Result main. Inherited Secure server
call sites receive only necessary mechanical Result adaptation and static review;
Secure remains disabled and is not compiled/tested/qualified.

Allowed files are explicitly listed in Task-147; no dependency, feature, lockfile,
license metadata or historical audit changes. New owned guard/tests/adaptations
and original evidence follow Apache-2.0 contribution policy; inherited MIT source
and notices remain. The fork notice records this before code, without legal
certification. No editor KNX implementation or new active fork dependency.

## Required evidence

Establish genuine old constructor failures with synthetic direct tests and retain
source/logs before changing the API. Debug and release characterize overflow vs
wrapping. New API tests must reject header 65530 and usize::MAX without allocation,
reject frame 65530 and 65536, retain header/frame 65529 and ordinary serialized
frames, and keep Task-146's ten extraction tests passing. No service-body validity
or device/frame transport conformance follows from these synthetic data.

Use pinned Rust/Cargo 1.99.1 on macOS arm64, provisioned private RUSTUP_HOME and
CARGO_HOME, target private/build/task147-protocol and private command logs under
private/evidence/task147-checked-lengths. Only inspected no-I/O test filters run;
compile-only all-target dpt and dpt,server checks do not execute examples/tests.
No Secure, network/socket, hardware, Windows or gate operation is authorized.
Exact commands, hashes, results, independent review and corrective revisions
must be recorded before completion. Task-016/017 remain independently blocked.

## Implemented API and propagation

`KnxIpHeader::new(ServiceType, usize) -> Result<Self>` checks addition of the
fixed header and uses `u16::try_from`; InvalidFrame describes an oversized body.
`KnxIpFrame::new(ServiceType, Vec<u8>) -> Result<Self>` passes body.len() directly
without a narrowing cast. Both return before producing an invalid frame.
The previous infallible APIs are deliberately replaced; callers must handle Result.

Production call-site updates:

- `src/application/knx.rs`: internal outgoing-frame builder returns Result;
  sender construction failures use the same existing send-error branch as failed
  sends. Disconnect-response construction/send share one Result and existing
  success/failure logging; failure cannot reach the success message.
- `src/transport/address_probe.rs`: wrapping helper returns Result and all
  three construction sites propagate it. Existing best-effort teardown transport
  errors are still ignored; construction failure cannot send a fallback.
- `src/transport/routing.rs` and `src/transport/tunnel.rs`: propagate frame
  constructor errors; routing's public Errors documentation includes InvalidFrame.
- `src/transport/server.rs`: reply/session send and the internal byte builder
  propagate Result. The two Secure response calls only gain ?. Inherited Secure
  test fixtures only unwrap owned constructor inputs. No Secure code was enabled.
- `src/transport/router.rs`, `tests/test_sequence_validation.rs`, routing/server
  tests and the Task-146 fixture helper deliberately unwrap owned small inputs.
  `examples/sequence_validation_demo.rs` propagates via its existing Result main.

Protocol/service encodings remain unchanged. The checked
constructors do not promise independent protocol validation, bounds on incoming
allocation, prevention of public-field mutation or correction of other encoders.
No production unwrap/expect was added; there is no fallback frame or new
compatibility constructor.

## Reproduction and observed results

Observed 2026-10-21 on macOS 27.0 arm64, aarch64-apple-darwin.
`rustc 1.99.1 (58cf8f9c3 2026-10-13)`;
`cargo 1.99.1 (5f94df478 2026-08-27)`.
Environment: repository-local private/toolchains/rustup as RUSTUP_HOME,
private/toolchains/cargo as CARGO_HOME, and
private/toolchains/rustup/toolchains/1.99.1-aarch64-apple-darwin/bin on PATH.
CARGO_TARGET_DIR is ignored private/build/task147-protocol. No provisioning,
network download, dependency or lockfile changes occurred.

Before changing the API, two direct baseline unit tests asserted that declared
length must agree with the untruncated length. The exact old commands were:

```sh
cargo test --lib --locked --offline --no-default-features --features dpt protocol::knxip::tests::constructor_ -- --test-threads=1
cargo test --lib --locked --offline --no-default-features --features dpt protocol::knxip::tests::constructor_ --release -- --test-threads=1
```

Both are expected FAIL exit 101, two failed cases each. Debug header body 65530
panicked at u16 addition; release returned total zero and failed the expected
65536 assertion. In both profiles, frame body 65536 returned declared total 6
while actual serialization had 65542 bytes. These assertions characterize the
bug, not an intended acceptance rule for impossible u16 totals.
Original regression-only source SHA-256:
`cae9e64132d68a15f6bd3e5df0d9d0c4a9af3b593b4d1368517dd9add5373252`.

The corrected API tests replace those observations with explicit rejection and
boundary checks. Exact corrected debug/library test command for each filter:

```sh
cargo test --lib --locked --offline --no-default-features --features dpt FILTER -- --test-threads=1
```

| FILTER | PASS cases | Filtered out at initial check |
|---|---:|---:|
| protocol::knxip::tests::constructor_ | 9 | 261 |
| protocol::knxip::tests::frame_ | 10 | 260 |
| protocol::knxip::tests::connect_request_ | 2 | 268 |
| protocol::knxip::tests::connect_response_ | 3 | 267 |
| transport::address_probe::tests:: | 10 | 260 |
| transport::router::tests:: | 3 | 267 |
| transport::tunnel::tunneling_address_tests:: | 3 | 267 |

All rows exit 0 with no failed/ignored tests. These are Task-015's previously
inspected no-I/O selections plus new constructor tests and Task-146 parser cases.
Only the affected owned fixtures changed. The address-probe selection inspects
in-memory codec/classification/candidate selection and errors before probing;
router tests use in-process channels; tunnel-address tests construct unconnected
state. No gateway/socket/session integration or Secure test is executed.

Other exact commands actually run:

```sh
cargo test --lib --locked --offline --no-default-features --features dpt --release protocol::knxip::tests::constructor_ -- --test-threads=1
cargo test --test test_sequence_validation --locked --offline --no-default-features --features dpt -- --test-threads=1
cargo fmt --all
cargo fmt --all -- --check
cargo clippy --lib --locked --offline --no-default-features --features dpt -- -D warnings
cargo check --all-targets --locked --offline --no-default-features --features dpt
cargo check --all-targets --locked --offline --no-default-features --features dpt,server
cargo metadata --format-version 1 --filter-platform aarch64-apple-darwin --locked --offline --no-default-features --features dpt
git diff --check
git diff --exit-code HEAD -- Cargo.toml Cargo.lock LICENSE LICENSE-APACHE rust-toolchain.toml AGENTS.md FORK.md AUDIT.md TEST-RELIABILITY.md FRAME-BOUNDS.md
```

All corrected commands exit 0. Release constructor selection reports 9 PASS
and 261 filtered out. Sequence selection reports 13 PASS, none ignored/failed;
its tiny synthetic frames use only the corrected Task-141 offline harness.
The initial selections total 53 debug cases, with nine constructor cases
repeated in release. The review correction below adds one pure regression and
repeats the affected selections on the final source. These runner passes do not qualify services or actual transports.
Library Clippy checks implementation, not every inherited integration-test target.
Both all-target checks are compile-only: no example, server or test was started.
Metadata shows root features exactly ["dpt"], 78 package entries, and no aes,
ctr, sha2, pbkdf2, x25519-dalek or zeroize packages. The server compile command
adds the existing server feature only; Secure stays disabled.

Both old and fixed release builds warn that rust-objcopy could not strip debug
info (SIGABRT) because @rpath/libLLVM.dylib is missing. Cargo completed those
builds and actually ran the tests; old expected FAIL and corrected PASS above
are genuine execution results. This warning limits stripping/toolchain packaging
claims, not the observed constructor behavior. No toolchain repair is included.

## Frozen evidence and limits

Private evidence directory: private/evidence/task147-checked-lengths.
Files include old-debug.log, old-release.log, old-regression-only-knxip.rs and
old-regression-only-sha256.txt; constructors-debug.log, constructors-release.log;
frame.log, connect-request.log, connect-response.log, address-probe.log,
router.log, tunnel-address.log, sequence.log, format-apply.log, fmt.log,
clippy.log, all-targets-dpt.log, all-targets-server.log, metadata.json,
metadata-stderr.log, whitespace.log, protected-paths.log, source-freeze.json and
implementation.diff. Full private metadata/logs are not published.
The frozen record hashes all nine modified source/test/example files. The
final src/protocol/knxip.rs SHA-256 is
`a2a4ef0949eab2e4511b4931c5a1c161162c0b90d9019dd4fcfb2252fe4002a2`.
Tracked-source whitelist matches the task's nine explicit allowed paths; upstream
notices, manifests, feature definitions, lockfile, toolchain and historical
Task-015/141/146 evidence are unchanged.

Editor scope/whitespace checks run separately and are recorded with this task.
Independent verification and critical review PASS. The reviewed fork was committed
first and its immutable revision is recorded above, before the editor record.
No active editor Cargo dependency is introduced.
Public mutable fields remain outside constructor guarantees. Secure adaptation
has static review only, with compile/test/runtime NOT RUN. Hardware, network,
Windows/Linux/MSRV and protocol interoperability remain NOT RUN; no phase gate
or Task-016/017 dependency is unlocked.

## Review correction: sequence reservation after checked construction

Critical review identified one correctness issue in the first Result adaptation:
application build_outgoing_frame_for_connection reserved next_sequence() before
fallible frame construction. Rejection sent nothing but advanced the local
counter, which could make the following valid frame skip the peer's expectation.
The coordinator authorized the bounded correction and its pure regression.

The application now serializes a typed TunnellingRequest with placeholder
sequence zero and validates the complete frame with checked KnxIpFrame::new.
Only after success does it reserve the sequence, set the typed request's counter
and replace the frame body with that same-length serialization. This bounded
second serialization avoids duplicated wire offsets and preserves the checked
length. Constructor rejection occurs before sequence state changes. Other
call-site inspection found no externally oversized reservation path: the probe's
private builder only receives fixed small internally built messages; server
sending already loads sequence state and stores only after acknowledgment.
No protocol redesign or address-probe change was added for this finding.

New exact command before and after correction:

```sh
cargo test --lib --locked --offline --no-default-features --features dpt application::knx::checked_frame_tests::rejected_frame_preserves_tunnel_sequence -- --exact --test-threads=1
```

Before correction: expected FAIL exit 101, one failed case, 270 filtered out;
rejected construction changed current_sequence from 1 to 2. Log and source/hash:
sequence-regression-before-fix.log, sequence-regression-before-fix-knx.rs and
sequence-regression-before-fix-sha256.txt. After correction: PASS exit 0,
one passed and 270 filtered out. The test constructs only lazy Tunnel::new_udp
state behind Arc<dyn Connection>, never connects/opens a socket or sends. It
checks InvalidFrame, unchanged nonzero sequence 1 after rejection, and the next
valid typed TunnellingRequest carries 1 with local counter then 2.

After final formatting, repeated commands are the same earlier exact commands
for constructor_ (9 PASS, 262 filtered out), frame_ (10 PASS, 261 filtered out),
fmt, library Clippy, both all-target dpt/dpt,server checks and whitespace/protected
paths: all exit 0. Other unchanged audited filters and the release constructor
checks were not repeated after this application-only correction; their earlier
results above remain identified by the initial source freeze. No new feature,
transport test or unsupported environment was enabled for the correction.

Additional private logs: application-final.log, constructors-debug-final.log,
frame-final.log, format-apply-final.log, fmt-final.log, clippy-final.log,
all-targets-dpt-final.log, all-targets-server-final.log, whitespace-final.log and
protected-paths-final.log. source-freeze-before-sequence-fix.json and
implementation-before-sequence-fix.diff preserve the first source freeze;
source-freeze.json and implementation.diff now identify the final source.
Only application source changed between these freezes, final SHA-256
`8383776740b632576fb6f31eb186aa382051419f913bfc9b0fa7147104afe403`.
The other eight source/test/example hashes match, including knxip.rs above.
Independent final verification and critical review PASS; the corrective fork
commit is recorded above.

## Independent final verification and review

Verifier `frame_verify` reports PASS for the updated application regression and
all nine frozen source hashes plus saved old-regression source hashes. Final
format, library Clippy, both all-target compile-only checks, metadata,
protected-path comparison, editor scope and whitespace PASS. Final metadata
retains exactly dpt, 78 packages and none of the six optional Secure packages.
The earlier unchanged selections independently produced 53 debug and nine
release passes; the known release debug-info stripping warning is documented.
These observations preserve the earlier/final source distinction above.

Private independent evidence is retained in private/evidence/task147-verifier:
sequence-preservation-final.log, fmt-final.log, clippy-final.log,
all-targets-dpt-final.log, all-targets-server-final.log, metadata-final.json,
metadata-final-stderr.log, fork-protected-paths-adjusted-final.log,
fork-whitespace-final.log, editor-scope-final.log and editor-whitespace-final.log,
plus earlier constructor/frame/audited-filter logs. The initial
fork-protected-paths-final.log compared the assigned sequence test file as though
it were protected and exited 1 on its authorized API adaptation. The verifier
corrected that classification and reran the comparison: exit 0. This initial
check mismatch is retained as verification history, not a product regression.

Critical reviewer `binding_review` reports final PASS: the meaningful
sequence-preservation regression and typed validation-before-reservation fix
resolve the medium finding; no unresolved source/documentation findings remain.
The reviewed fork was committed first and its immutable revision is recorded
above. Task-147 is Done for this bounded offline increment. Independent agent
verification/review grants no human acceptance, rights decision or gate approval.
