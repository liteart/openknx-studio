# Task-012: Round-trip one original DCA configuration change

## Subject

Round-trip one original DCA configuration change

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-012 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host state bridge |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-018](../docs/REQUIREMENTS.md), [R-021](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Prove that a real user change in the original DCA can leave the host process and survive a restart.

## Detailed Requirements

- DR-001: Select one non-bus configuration operation actually offered by the DCA and document its expected changed value.
- DR-002: Capture the real state/parameter change through the discovered storage contract; preserve opaque bytes if semantics are unknown.
- DR-003: Persist a minimal file envelope with device/app/DCA hashes, then close/restart the host and reload it.
- DR-004: Detect no-op versus changed state and reject state for a different device/application.
- DR-005: Keep the original assembly unchanged.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: After restart the same original DCA displays the selected changed value.
- [ ] AC-002: The saved state is not only in memory or SQLite and its hash is recorded.
- [ ] AC-003: A failed state capture is not reported as a successful round-trip.

## Dependencies to Other Tasks

- [Task-011](Task-011.md): Supply one coherent device and product context to the DCA. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows integration: change, save, terminate normally, reload, compare.
- TEST-002: Negative: corrupted/mismatched state envelope and missing blob.
- TEST-003: No-op: reopening without edits preserves the captured configuration payload.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host state bridge**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No production transaction engine, merge of opaque state, or live gateway transfer.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; read-only original UI inspection only |
| Source commit(s) | Worktree based on `1325a53`; source audit below |
| Tests executed | Windows read-only inspections and owned regression; required TEST-001–003 NOT RUN |
| Evidence location | Private run `4913a4b620f9` and source audit below |
| Acceptance criteria met | None asserted |
| Technical outcome | Two SDK parameter mappings observed; editable operation and round-trip not established |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Unassigned |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).


## Read-only readiness checkpoint — 2026-11-02

At this checkpoint Task011 remained in progress. Source103 original UI run
`54b41270cd58` rendered and closed, but recorded parameter values not read,
configuration acceptance not established, a transient fixture-only writer and
`state=NOT_RUN`. These observations do not establish a user configuration change
or persistent state. Once Task011 is accepted, the first bounded investigation
is one actually offered non-bus editable value and its genuine before/after
read, write and save path; only then can the file envelope and restart/reload
requirements be implemented. Hardware and bus operations are not required for
that first investigation.

## Read-only Windows inspection — 2026-11-03

Task011 is now Done for the fixed reference fixture at `1325a53`. On Windows
dev host `.60`, original DCA inspection run `4913a4b620f9` rendered and closed
normally (exit 0). Its bounded visual traversal visited 512 nodes and reached
its limit. Two original `Description` bindings on `TextBlock` controls mapped
to genuine SDK parameters of the fixture device; this does not establish that
either control is visible or editable. No setter, change, capture or reload was
performed. The input audit passed for 42 pinned files and two OS files; the
test process and its scheduled task were gone at collection.

Private evidence: `private/evidence/task010-sdk-wrapper/iteration01/`
`run-4913a4b620f9/original.log` (SHA-256
`6a26bb02c8e98449b0f819265ab7a842e781d21c6e7041267d3f0b6704da8dcc`)
and `source-audit110.json` (SHA-256
`481802e6f97fde01b26daf078f0efc246d27ba3c49e8ed9433512644330c83d5`).
The source was an uncommitted worktree based on `1325a53`, pinned by that
audit. TEST-001, TEST-002 and TEST-003 remain NOT RUN; no acceptance criterion
is claimed. The next step is to identify an offered, editable non-bus action.

Follow-up diagnostic run `dca012e11101` observed eight candidate
`DataGridTextColumn` Description metadata entries and no actual text editor.
The scanner did not verify that each column binding uses the row as its source;
therefore the reported SDK mappings are unqualified candidates. The run exited
2 with `PROJECT_LOG_LIMIT` after inspection, not PASS. Original inputs still
passed the post-run hash check and the test process and task were removed.
Private log SHA-256:
`be450b7c021a15eb84bb3b37d05df1105a1b9024e627c6c338b3535e43c7fbe4`;
source audit SHA-256:
`1dab4f010486655820dc58e5fa51f22ec8b45116cdd6e5e8072672bd0ed9b7f8`.
The next diagnostic must qualify binding sources, accurately label visibility,
and leave room for the bounded final log before repeating the observation.

Corrected run `dca012e11201` exited 0 and retained the unchanged input pins.
It observed one genuine SDK parameter associated with a visible row and a
non-read-only `DataGridTextColumn` declaring a TwoWay Description binding with
default row context. It did not enter edit mode or prove a visible cell editor.
The one-candidate limit was reached. Private original log SHA-256:
`dc3165b5d9f1d0875b73bc769086bf8cd175635d8b2c630340e73f46f657d097`;
source audit SHA-256:
`4c2782fa0053ca88cba28f7a93c7dcae018b89205f017eec503fd2551a6a5f13`.

Verification of this source snapshot: `Test-StateInspection.ps1` passed its
owned policy tests; `scripts/check_project_scope.py`, `cargo fmt --all --
--check`, `cargo clippy --workspace --all-targets --locked -- -D warnings`,
`cargo test --workspace --locked`, and `git diff --check` passed. The Rust
workspace currently contains zero tests. Windows owned run `dca012f11201`
reported 423 `OWNED_CASE PASS` entries, `OWNED_SUITE PASS`, task exit 0, and
zero remaining owned processes. Its collector failed afterward because it
expected a `sdk-project.log` file not produced by this suite; the driver output
was inspected separately and the completed scheduled task was removed. This
owned regression does not execute the original DCA or prove state round-trip.
