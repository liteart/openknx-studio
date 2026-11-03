# Task-149: Preserve the mandatory cEMI TPCI control octet

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-149 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / cEMI codec |
| Owner | Coordinator and protocol agent |
| Requirement references | R-004, R-030 |

## Description and boundary

Task-016 primary-reference inspection found that cEMI length zero still carries
one TPCI octet. The inherited parser/serializer instead discard that octet,
preventing correct connected control frames. Correct this separately before
the management peer uses the public codec; do not conceal a production codec
change inside simulator work or add a parallel control-frame encoder.

## Dependencies and readiness

Accepted Task-015 audit; accepted Task-147 fork source
`4b0d4a5be6334e161989df6f24588da3b56b8666`. No phase gate applies.
The operator supplied KNX Standard v3.0.0 privately. Exact reference observations:

- Volume 3 EMI/IMI v01.04.02, section 4.1.5.3.2, pages 76–77:
  length excludes TPCI; zero still requires the TPCI octet. SHA-256
  `6a80ead0a3e72efc43749141021b4d9e4d9da9b4d27dcead487bd55b7388c0b7`.
- Volume 3 Transport Layer v01.02.03, section 2 figure 3, page 6:
  selected connection/ACK control values. SHA-256
  `4ec1e952d0d9bc75a624467f781e17d01e764e90768a814af7b5084523354d44`.

Rendered private pages were inspected; no specification bytes are published.

## Detailed requirements

- Parse and preserve TPCI for every supported L_Data frame, including length zero.
- Serialize the mandatory octet even when APCI/data is empty.
- Reject a truncated zero-length frame whose TPCI octet is absent.
- Preserve existing nonempty data behavior; avoid unrelated codec/API changes.
- Keep all encoding in the fork and Secure disabled for the selected tests.

## Acceptance criteria

- [x] AC-001: Exact owned connect/disconnect/ACK specimens preserve their TPCI octets.
- [x] AC-002: Missing mandatory TPCI is rejected; valid nonempty payloads remain correct.
- [x] AC-003: Focused existing cEMI tests and approved feature checks pass with genuine before/after evidence.

## Required tests

Establish the inherited failure before implementing. Run focused public-codec
regressions, existing cEMI unit/property cases, fmt and focused Clippy with
`--locked --offline --no-default-features --features dpt`. Never run inherited
blanket tests, gateway discovery or real hardware. No network is required.

## Deliverables and ownership

Fork: only cEMI implementation and focused tests plus concise source evidence
if needed. Editor: this task, task catalog/traceability and explicit Task-016
dependency/readiness note. Commit reviewed fork changes first, then record its
exact revision here. No editor protocol code, dependency override or DLL/PDF
redistribution. No remote publication or hardware grant.

## Completion record

Implementation/tests/review pending. No acceptance, hardware outcome or gate GO
is asserted.

## Observed result and acceptance, 2026-10-22

Reviewed fork implementation commit:
`9daccba71885b03aa13b437c200ea0028220db55`, based on
`4b0d4a5be6334e161989df6f24588da3b56b8666`. The editor introduces no
Cargo dependency or development path override. The simulator must use this
accepted correction instead of a separate short-control encoder.

| Check | Observed result |
|---|---|
| Corrected-specimen baseline at the exact old revision | FAIL as expected: exit 101, one nonempty case passed and three mandatory-octet cases failed |
| New public-API regressions after correction | PASS, 4/4: control parsing, exact serialization, truncated control rejection and nonempty preservation |
| Existing `protocol::cemi::tests::` | PASS, 9/9, 262 tests filtered out |
| Formatting | PASS |
| Focused test Clippy and independent library Clippy | PASS with warnings denied |
| Source/manifest integrity | Only cEMI source and the focused test changed; Cargo.toml, Cargo.lock and toolchain remain unchanged |
| Hardware, real networking, Windows and simulator | NOT RUN; no claims follow |

The initial owned specimen used control byte BC, while the public constructor's
selected priority produces B4. The writer corrected that test-only mismatch
before changing production code and reran the baseline: the three independent
mandatory-octet failures persisted. The coordinator independently reconstructed
the old source with `git archive`, added the identical frozen regression test,
and retained the same one-pass/three-failure result under
`private/evidence/task149-cemi-control/baseline-result.json`,
`baseline-stdout.txt` and `baseline-stderr.txt`. These are actual retained
observations; no writer log file is invented.

Actual Cargo commands used provisioned Rust/Cargo 1.99.1 on macOS arm64, with
private CARGO_HOME/RUSTUP_HOME and isolated target directories:

```sh
cargo test --test test_cemi_control_frames --locked --offline --no-default-features --features dpt -- --test-threads=1
cargo test --lib --locked --offline --no-default-features --features dpt protocol::cemi::tests:: -- --test-threads=1
cargo fmt --all -- --check
cargo clippy --test test_cemi_control_frames --locked --offline --no-default-features --features dpt -- -D warnings
```

The independent verifier and reviewer also ran the corresponding focused
regressions and existing cEMI cases, plus library-only Clippy with the same
locked offline feature selection. No unfiltered inherited suite ran.
`python3 -B scripts/check_project_scope.py` and `git diff --check` PASS for
the editor record; the fork whitespace check also PASS.

Frozen source SHA-256:

- `src/protocol/cemi.rs`: `b659c3a1589be74fc2730b4f610b3b64b50e86b6b6cfa1cb3957e2af127a9254`.
- `tests/test_cemi_control_frames.rs`: `cfe2cd8161272d3db4cc50b61ca9123a92a526be0834151d464be3de271c605b`.

Predefined `objectmodel_verify` independently reproduced the after-state
checks without source edits. Predefined `project_review` independently checked
the retained baseline, exact source hashes, rendered primary-reference pages,
regressions and unchanged manifests; final review PASS with no findings. All
three acceptance criteria are met for this codec correction. This is not
hardware qualification, simulator acceptance or a phase-gate GO.
