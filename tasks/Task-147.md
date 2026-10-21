# Task-147: Check frame constructor lengths before narrowing

## Metadata and readiness

| Field | Value |
|---|---|
| Task ID | Task-147 |
| Status | Done |
| Component | knust-fork / checked frame construction |
| Requirements | R-004, R-015, R-023, R-028, R-030 |
| Fork baseline | `75c31485525eb65e232d325a9dba130897273b24` |
| Editor baseline | `3def997ec5e27ecaa7417ee769e8875b01e9d4c9` |

Accepted Tasks 015, 141 and 146 establish the audit, reliable offline assertion
runner and raw-frame parse bounds. The coordinator authorizes this independent
defensive increment while DCA clean-VM input is pending. No phase gate is
required or advanced; Task-016 references and all hardware/management gates
remain separate prerequisites. Read AGENTS, GOALS, ARCHITECTURE, fork AGENTS,
FORK/contribution policy and [the constructor contract](../docs/POC-CHECKED-FRAME-LENGTHS.md)
before code edits. The contract precedes implementation.

## Scope and acceptance

- AC-001: Reproduce old constructor overflow with header body 65530 and frame
  body 65536 using synthetic direct tests before the fix; inspect debug/release
  differences without treating wrapping or panic as acceptable success.
- AC-002: Change public Header and Frame `new` constructors to return Result;
  Header takes usize body length and checks addition and narrowing. Reject body
  lengths above 65529 with existing ProtocolError::InvalidFrame; no truncation,
  wrap, production unwrap/expect, silent fallback or unchecked compatibility API.
- AC-003: Propagate or explicitly report construction failures at every internal
  call site. A rejected outgoing build must not consume a tunnel sequence
  number. Preserve ordinary frame bytes and inherited Secure source. Purely
  mechanical Secure call-site adaptations are static-only and NOT RUN.
- AC-004: Tests cover header 65529/65530/usize::MAX, frame 65529/65530/65536 and
  ordinary/empty bodies. Task-146 parse regressions and audited no-I/O tests pass.
  Format, library Clippy, dpt all-target compile checks and dpt,server compile-only
  checks pass or accurately record unrelated baseline limitations.
- AC-005: Independent verification/review, exact frozen source/evidence and fork
  first commit precede the editor record. No active editor dependency is invented.

## Ownership and checks

Fork: `src/protocol/knxip.rs`, necessary calls in `src/application/knx.rs`,
`src/transport/{address_probe,routing,tunnel,router,server}.rs`,
`tests/test_sequence_validation.rs`, `examples/sequence_validation_demo.rs`,
and `CHECKED-FRAME-LENGTHS.md` contribution notice. Editor: this task, TASKS,
TRACEABILITY and POC-CHECKED-FRAME-LENGTHS. No dependencies, features, lockfile,
license metadata, prior audit or manufacturer/editor semantics change.

Use pinned Rust 1.99.1 and private offline Cargo cache; task-specific target and
logs stay ignored. Pure library tests use exact filters, never unfiltered suites.
All Cargo commands use `--locked --offline --no-default-features --features dpt`
(except fmt; compile-only server check uses dpt,server). Clippy checks library
with warnings denied. All-target check compiles without executing tests/examples.
Secure is never enabled. No socket, hardware, publication or rights decision.

## Completion record

| Field | Observed result |
|---|---|
| Implementation / report status | Bounded implementation, local checks, independent verification and critical review PASS |
| Source commit(s) | Fork `4b0d4a5be6334e161989df6f24588da3b56b8666`; editor record identified by this file's Git history |
| Tests executed | Old debug/release expected FAIL (2 each); new 9 constructor tests PASS in both profiles; 44 existing selected offline cases and one new sequence-preservation case PASS; fmt, library Clippy, dpt/dpt,server compile-only, metadata and integrity PASS |
| Evidence location | docs/POC-CHECKED-FRAME-LENGTHS.md; private/evidence/task147-checked-lengths |
| Acceptance criteria met | AC-001 through AC-005 met for this bounded increment; reviewed fork committed first and immutable revision recorded |
| Technical outcome | PASS for checked constructors and source adaptation; no transport qualification |
| Gate outcome | No gate required or advanced |
| Reviewer | `binding_review` final critical review PASS; sequence finding resolved, no unresolved findings |

Hardware, live network, Windows/Linux/MSRV, Secure and interoperability are
NOT RUN. Local source/test results cannot qualify device support or open gates.

The explicit API break is `KnxIpHeader::new(ServiceType, usize) -> Result<Self>`
and `KnxIpFrame::new(ServiceType, Vec<u8>) -> Result<Self>`. Production errors
propagate via ? or the existing background send-error path; unwrap additions are
confined to tests with owned small fixtures. Nine source/test/example files are
frozen in the private source hash record. Public fields/literal bypass and other
serializers remain unchanged, so guarantees concern these checked constructors.
Old and fixed release test binaries actually ran despite a toolchain warning:
rust-objcopy could not strip debug info because libLLVM.dylib was missing.
This packaging/toolchain limitation is recorded and not repaired by this task.

Critical review found that the first Result adaptation reserved a tunnel sequence
before rejecting an oversized application frame. The coordinator authorized its
correction within this task. A pure regression first FAILed (counter 2 instead
of 1), then PASSed after typed complete-frame validation moved before sequence
reservation. No other externally oversized reservation path was found; private
probe inputs are fixed small messages and the server stores after acknowledgment.
The final source has application hash
`8383776740b632576fb6f31eb186aa382051419f913bfc9b0fa7147104afe403`;
the other eight source hashes remain unchanged. Constructor/parser, fmt,
library Clippy and both all-target compile-only checks were rerun PASS after
this correction. Independent final verification and critical review PASS. The
fork correction is committed as `4b0d4a5be6334e161989df6f24588da3b56b8666`.

Independent verifier `frame_verify` matched all nine final frozen source hashes
and saved old-regression source hashes. Final application regression, fmt,
library Clippy, dpt/dpt,server all-target compile-only, metadata, protected paths,
editor scope and whitespace PASS. Earlier unchanged selections independently
reported 53 debug and nine release passes, with the stripping warning retained.
Logs are private at `private/evidence/task147-verifier`. The verifier corrected
an initial protected-path comparison that mistakenly included the assigned
sequence test file; the final protected-path check PASSes. Critical reviewer
`binding_review` then reported final PASS, with the sequence finding resolved
and no unresolved source/documentation findings. The reviewed fork was committed
first and its immutable revision is recorded above; this bounded task is Done.
No agent result grants maintainer acceptance or a phase-gate approval.
