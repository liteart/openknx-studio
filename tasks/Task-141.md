# Task-141: Propagate inherited sequence property-test failures

## Subject

Repair the discarded assertion result identified by the accepted Task-015 audit.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-141 |
| Status | Done |
| Stage | PoC test reliability; no phase gate required or unlocked |
| Priority | P0 |
| Component / Git project | knust-fork / inherited offline sequence tests |
| Owner | Coordinator with predefined protocol, verifier and reviewer agents |
| Requirement references | R-004, R-022, R-028, R-030 |

## Description

`property_sequence_validation_consistency` discards the `Result` returned by
its async block. A failed `prop_assert_*` can consequently produce a passing
test. Correct this test harness without changing protocol behavior or claiming
that the inherited synthetic assertions establish standards compliance.

## Detailed Requirements

- DR-001: Propagate every property assertion failure to the outer proptest case.
- DR-002: Establish a reproducible failing observation using a deliberately false
  owned assertion or narrowly controlled test mutation. Show that the old harness
  accepts it and the corrected harness rejects it. Restore all intentional faults.
- DR-003: Keep source changes confined to the inherited sequence-test harness;
  preserve upstream notices, protocol implementation, dependencies and features.
- DR-004: Use only the audited offline sequence tests; construct no real gateway
  connection and do not run an unfiltered test suite or enable Secure.
- DR-005: Preserve Task-015's historical observation and record the corrective
  source revision separately; do not rewrite its 75 observed runner passes.

## Non-functional Requirements

Follow [AGENTS.md](../AGENTS.md), [ARCHITECTURE.md](../docs/ARCHITECTURE.md) and
[TEST-STRATEGY.md](../docs/TEST-STRATEGY.md). Use pinned Rust 1.99.1 and the unchanged
fork lockfile, offline checks, English evidence and original owned test data.
No product, manufacturer, SDK, hardware, network or runtime behavior is added.

## Acceptance Criteria

- [x] AC-001: A deliberately false property assertion fails the corrected runner,
  with the original discarded-result behavior reproduced as a false pass.
- [x] AC-002: The unmutated selected offline sequence tests pass with propagated
  assertion failures; formatting and applicable targeted Clippy checks pass.
- [x] AC-003: Independent verification/review records exact source revisions,
  commands and limits; no implementation, feature, lockfile or license change.

## Dependencies to Other Tasks

- [Task-015](Task-015.md): Done; fork audit `7e44751fb6681812fcb6690d8159a9b8c5c3e816`,
  editor audit `678fc7f0b8f0b0b6b2250361341c6ed0bf03c7f8` and record `37c1e0f`.

No phase gate is required or advanced. This independent repair does not unblock
management operations, original DCA execution or a physical bench test.

## Required Tests

- TEST-001: Controlled false-assertion comparison, old versus corrected harness.
- TEST-002: `cargo test --test test_sequence_validation --locked --offline --no-default-features --features dpt`.
- TEST-003: `cargo fmt --all -- --check` and targeted sequence-test Clippy with
  the same locked/offline feature selection; no blanket Cargo tests.
- TEST-004: Source/manifest/lock/license integrity and restoration of mutations.

## Deliverables

A minimal fork test repair and reproducible evidence; editor task/index/traceability
records. Commit the reviewed fork first, then record its exact revision here.

## Out of Scope

Protocol fixes, simulation, source removal, Secure activation, new dependencies,
manufacturer behavior, real network/device tests, publication and rights decisions.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Done; independent verification and critical review PASS |
| Source commit(s) | Fork baseline `7e44751fb6681812fcb6690d8159a9b8c5c3e816`; reviewed correction `0fc99a9c0d551093c8eea4883c052c75e59ee6c8`; editor task/evidence record `9b6bf43615597df22c0d89e27ea26b3005495aaa` |
| Tests executed | Controlled false assertion: old harness false PASS exit 0; corrected harness expected FAIL exit 101; restored 13 sequence tests, fmt, targeted Clippy and integrity checks PASS, independently reproduced |
| Evidence location | Separate fork `TEST-RELIABILITY.md`; private implementation and independent verification logs |
| Acceptance criteria met | AC-001/002/003 demonstrated and independently reviewed |
| Technical outcome | PASS for offline assertion propagation; no protocol or hardware qualification |
| Gate outcome | No gate required or advanced |
| Reviewer | Project reviewer PASS: two-line propagation, both controlled log sets, restored hash, scoped evidence and protected-path checks; Cargo not rerun by reviewer |

## Implemented increment and verification

The fork changes only two lines in `tests/test_sequence_validation.rs`:
remove `let _ =` and propagate the async result with `?`. Independent verification
reproduced the exact controlled false-assertion comparison, then restored source
SHA-256 `e44eac81005c4d0901d57318db2cdc7de21d242e7fa8cbe3212d23d6499d8d26`.
The 13 unmutated offline sequence tests, format check, targeted Clippy and
protected-path integrity checks PASS with Rust/Cargo 1.99.1 on macOS arm64.
The inherited implementation, dependencies, lockfile, license notices and
Task-015's historical audit are unchanged. No regression artifact remains.
Private evidence basenames: `task141-evidence-wixq9bk6` and
`task141-independent-evidence-rb13oi98`. Critical review PASS; the fork correction was committed first at
`0fc99a9c0d551093c8eea4883c052c75e59ee6c8`. Windows/Linux, hardware, live
network and protocol interoperability are NOT RUN for this repair.

The reviewer found no unresolved correctness or boundary issue. A minor index
formatting issue was corrected before committing; scope and whitespace checks
passed again. Task-141 is Done for its offline test-reliability scope. No phase
gate was advanced, and Task-015's historical evidence remains unchanged.
