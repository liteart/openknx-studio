# Task-146: Reject frame lengths shorter than the fixed header

## Metadata and readiness

| Field | Value |
|---|---|
| Task ID | Task-146 |
| Status | Done |
| Component | knust-fork / malformed-input parser bounds |
| Requirements | R-004, R-015, R-023, R-028, R-030 |
| Fork baseline | `0fc99a9c0d551093c8eea4883c052c75e59ee6c8` |
| Editor baseline | `54fff13475b7b4b91942f96859b5cff36978f370` |

Accepted predecessors are Task-015 (fork audit `7e44751fb6681812fcb6690d8159a9b8c5c3e816`,
audited source `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e`) and Task-141
(offline assertion propagation `0fc99a9c0d551093c8eea4883c052c75e59ee6c8`).
The coordinator authorizes this independent defensive increment while the DCA
clean-VM input is pending. No phase gate is required or advanced. Task-016
references and all hardware/management prerequisites remain separate blockers.
Read AGENTS, GOALS, the fork AGENTS/FORK/contribution policy, ARCHITECTURE and
[the bounded contract](../docs/POC-FRAME-BOUNDS.md) before implementation.

## Scope and acceptance

- AC-001: Reproduce the old public `KnxIpFrame::parse` panic with direct
  synthetic tests for each declared total length 0 through 5 before fixing it.
- AC-002: Reject those lengths with existing structured `ProtocolError::ParseError`
  before slicing. Document the public error. Do not change header parsing,
  service encoding, constructor behavior or trailing-byte policy.
- AC-003: Regressions preserve raw frame boundary length 6 with empty body,
  length 7 with one retained body byte, oversized declaration rejection and
  ignored trailing bytes. This is frame extraction, not service validation.
- AC-004: Focused regression, audited offline KNX/IP tests, format and applicable
  Clippy pass or report an exact pre-existing limitation. No unfiltered library,
  socket/session integration, Secure or hardware tests run.
- AC-005: Independent verification and review record genuine results. Commit
  the reviewed fork first, then record its immutable revision in editor evidence.
  Preserve Task-015 historical claims and all upstream notices.

## Ownership and checks

Fork writes: `src/protocol/knxip.rs` and `FRAME-BOUNDS.md` contribution notice.
Editor writes: this task, TASKS.md, TRACEABILITY.md and POC-FRAME-BOUNDS.md.
No API type, dependency, feature, lockfile, license metadata or editor protocol
implementation changes. No proprietary fixtures, manufacturer values, primary
protocol interpretation or network operation is needed: the existing fixed
`KnxIpHeader::LENGTH` and slice bounds establish this Rust safety invariant.

Use pinned Rust 1.99.1 and the existing offline Cargo cache. Exact commands and
results belong in the linked contract/evidence. Required checks are focused
`cargo test --lib` filters for `protocol::knxip::tests::frame_`,
`protocol::knxip::tests::connect_request_` and
`protocol::knxip::tests::connect_response_`; use `--locked --offline
--no-default-features --features dpt` and `-- --test-threads=1`.
Run `cargo fmt --all -- --check`, targeted library Clippy with warnings denied,
editor planning scope and whitespace checks. Do not fix unrelated warnings.

## Completion record

| Field | Observed result |
|---|---|
| Implementation / report status | Bounded implementation, local checks, independent verification and critical review PASS |
| Source commit(s) | Fork correction `75c31485525eb65e232d325a9dba130897273b24`; editor record identified by this file's Git history |
| Tests executed | Old expected FAIL: six panics, four controls pass; corrected ten regressions and five audited KNX/IP tests, fmt, library Clippy, metadata and integrity PASS |
| Evidence location | docs/POC-FRAME-BOUNDS.md; private/evidence/task146-frame-bounds command logs |
| Acceptance criteria met | AC-001 through AC-005 met for this bounded offline increment; fork committed first and immutable revision recorded |
| Technical outcome | PASS for bounded synthetic raw-frame parsing |
| Gate outcome | No gate required or advanced |
| Reviewer | `binding_review` critical review PASS; no unresolved findings |

Hardware, Windows, live network, interoperability and complete service validity
are NOT RUN and outside this task. Local tests do not unlock Task-016/017 or
qualify a device. No push, publication, rights decision or gate approval.

Independent verifier `frame_verify` reproduced the frozen-source results:
10 frame regressions, two connect-request and three connect-response cases,
fmt, library Clippy, resolved metadata, protected paths, editor scope,
whitespace and source SHA-256 comparison PASS. Logs are retained privately at
`private/evidence/task146-verifier`. The verifier inspected the old expected-FAIL
log and regression-only source/hash without rerunning the old panic reproduction.
Critical reviewer `binding_review` subsequently reported PASS for the frozen
source, contract and evidence, with no unresolved findings. The reviewed fork
correction was committed first as `75c31485525eb65e232d325a9dba130897273b24`.
This record marks bounded task completion; agent review does not grant
maintainer acceptance or gate approval.
