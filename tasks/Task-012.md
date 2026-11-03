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

Edit attempt `dca012e11301`, source audit SHA-256
`7b57640362c57c2295a3c5c7a6814b7e6474f0857cb243aa8bc534968a41ca3f`,
reached the original grid editor for the selected fixture value but failed
the original cell commit and validation (exit 2). The post-attempt SDK change
was not established and a partial change was not ruled out; no rollback,
capture, or round-trip is asserted. Input hashes remained intact and no owned
process remained. Private log SHA-256:
`e71b92635768010a9636d1099b93ca738b83c852d9dab528c00ae64a4a159284`.
The WPF validation cause needs further diagnosis before another edit attempt.

Diagnostic retry `dca012e11401` also exited 2 with cell commit rejected. It
recorded an unchanged actual SDK value, an active binding, no validation errors
on editor/row/grid, an active row edit, and no keyboard focus on the editor.
The row commit was not attempted. Lack of focus is an observed correlation,
not yet a proven cause. The private log SHA-256 is
`98743990d7cd45a1ac099c1232157fa63e8ff61dbcd5ba3bd1083b821dc3e217`;
source audit SHA-256 is
`acac3ea38352580e8c947347d9956794bada9ef9f52822f7ab214388112a6747`.
The test confirmed input pins and cleanup, and did not establish persistence.

Focus retry `dca012e11501` confirmed the editor and grid had keyboard focus
before the change; the cell commit still returned false with no validation
errors and the SDK value unchanged. Focus alone is therefore insufficient.
The run exited 2 and also reached the global log limit during cleanup. Input
pins and process cleanup passed. Private log SHA-256:
`387fc39f144d81453a85cab826c48367056273bc4d556b1d213875c14430dc51`;
source audit SHA-256:
`1035b55986bf3efe0130d8dbe8680d34d21f8b380287f1018b8f6d1d5dea40c5`.

Event diagnostic `dca012e11601` exited 2: the requested fixture text was in
the original editor, its binding was dirty, and the passive cell-ending
observer saw `Cancel=false`. Commit returned false and the cell remained in
edit mode; no editor, row, grid or row binding-group errors were reported.
The SDK value remained unchanged. The observer does not rule out a later
handler changing cancellation. Original inputs and process cleanup passed.
Private log SHA-256:
`e3320bc58bde51a9090e533bf2affc8e5918544d2f74b7327f8b1c77bd220730`;
source audit SHA-256:
`8db820f6b9c8a025194f81baaa2be63d0510da9ef98c326e8ac74a61177785a5`.

Enter-input diagnostic `dca012e11701` reached both original cell and row
commit events. The SDK invoked `IDispatcher.EnableDefer` (`060001AD`), which
the current host rejected as unsupported; WPF exposed that exception as a
validation error and the SDK value remained unchanged. The run exited 2 and
did not capture or persist state. The next increment must implement only the
verified dispatcher contract; it must not bypass SDK write checks. Input pins
and process cleanup passed. Private log SHA-256:
`0a979fc7f7d20853bcc86310eed0c984681e3e3a8260454fe6d10a9727a60989`;
source audit SHA-256:
`ac65b368db19a8af0f3e1a04283cde3d8c9544f67cd3022d9ed6c2eefa051b2e`.
For this snapshot, the focused `Test-StateInspection.ps1` policy checks passed;
Windows owned regression `dca012f11701` reported 423 passing cases and task
exit 0. Its collector again expected an absent `sdk-project.log`; the driver
output was checked separately and the completed task was removed. These tests
do not establish a successful original edit or persistence.

After input-filter and reporting review fixes, source audit SHA-256
`c62330b182ea650a1544d1861845209ea50b9a6eb9f2f04979d871eedeb926d3`
built on Windows in run `dca012e11801`. The original edit attempt again exited
2 at the unsupported `EnableDefer` call; the SDK value remained unchanged.
Input hashes passed and the test process and task were removed. Private log
SHA-256:
`4cd50d27d7f96076c768f5b0f3a4ee797758688c478923b90ae6107d319e2388`.

Static PE inspection of the 50 privately copied, hash-verified ETS6 DLLs found
one direct `IDispatcher` implementer: `Knx.Ets.ViewModel.WpfDispatcherService`.
The inspected healthy path uses the original `WindowLock` counter, queues
events while nested, and drains them in order at the outermost release. The
provider was not executed or redistributed. Windows run `dca012e11901` with
an owned bounded adapter exited 2 because the host's edit-phase admission
guard denied `EnableDefer` before entering the adapter. The SDK value stayed
unchanged. This is not proof of a completed edit or of general provider
equivalence. Private original log SHA-256:
`33e79489deea6670c2e5f32e014a1531867198c9159a955cb362deeedf76268c`;
source audit SHA-256:
`4f2cf4ff705631e5d8d92ab5a4f3891f7902e974e0bc033aef689039d78700d9`.

Run `dca012e12001` passed the dispatcher scope admission (two calls, no pending
actions or dispatcher fault), then failed because the controlled write fixture
rejected the genuine `ParameterInstance` as an untracked object. A subsequent
phase error followed its sticky fault. The SDK value remained unchanged and
the run exited 2. The input hashes and cleanup passed. Private log SHA-256:
`de7487a7883ecb15d540ad69baaceea3883174f1cc79bb55c709d32137062f3f`;
source audit SHA-256:
`e6e77d97922d03eda7a4d111a47e4c51ee4f5dcaad64181b442cdcd5145bf024`.

Run `dca012e12201` read back the requested test text from the genuine SDK
parameter, but then `SetDirty` rejected a genuine `ProjectTrace`; the row
commit failed and the process exited 2. This is a partial in-memory fixture
mutation, not an accepted change, rollback, capture or durable save. Input
hashes and process cleanup passed. Private log SHA-256:
`3a6ec17767f4f6fc9be1e6f4d0772372bc5489e2580da2c988216c6471a0ec64`;
source audit SHA-256:
`2a49df0396eda3fb31aa37f38b08bbc1b34ca98d2700d9d2167b9d596ff63ad6`.

Run `dca012e12301` read back the requested SDK value, observed the row edit
end with no validation errors, but the previously retained cell still reported
`IsEditing=true`. The owned success guard therefore failed and the run exited
2. Whether that cell remains the live grid cell is not yet established; no
accepted edit, capture or persistence is asserted. Input pins and cleanup
passed. Private log SHA-256:
`eed8b32aa187a57ec916b5978c8b9457814c0967f743c177096c8f96dcf731ed`;
source audit SHA-256:
`0ca8c5d6f47ff52240b8be1ea35eac1794ead5254f31bc0a50a6ba2aac7a7b5c`.

Run `dca012e12401` re-resolved the exact live cell: it was the same connected
cell, still editing with a different TextBox. The stale-cell hypothesis is
therefore contradicted for this run. The SDK value matched, row editing had
ended and no validation errors were observed, but the UI success guard failed
and the run exited 2. Input pins and cleanup passed; no state capture is
claimed. Private log SHA-256:
`b43fb1ac936e99c9a228991b62a505a0cc3c864da27aec7515ee53f727f4d671`;
source audit SHA-256:
`bbd39f2b5cbae507decb1d8729cfbf3e5cbe3dbfe24293fd428fa283e614d09f`.

Run `dca012e12501` stopped before sending Tab because the required keyboard
focus was no longer inside the grid after Enter (`STATE_EDIT_TAB_REAL_FOCUS_SCOPE`,
exit 2). It did not complete the live-cell or SDK readback checks; the SDK
state after this attempt is not asserted. Input pins and cleanup passed.
Private log SHA-256:
`e7b9fc3e97e43dd36dec44e4bd73dc8d5b2cfe625006999ce737ed7c2dce5cac`;
source audit SHA-256:
`2c0b0d0955a3beec81e822462f590ffa6e1307c4b66003dca3d100ab78d79e56`.

Run `dca012e12601` re-resolved the exact live cell and attempted to focus it;
the grid acquired focus but the cell itself did not. The explicit focus guard
stopped before Tab (exit 2), so final SDK/readback and capture are not asserted.
Input pins and cleanup passed. Private log SHA-256:
`ff52bb62df58ada325f1bae5b22ef437e7c2e5b9ac2074047f50cf01cb181361`;
source audit SHA-256:
`f7be853a1c2be7fbea9ebfbaaf20e3a9a2ca69f0a1384504bcd506f8bcad4d19`.

Run `dca012e12701` completed the automated fixture edit through the original
WPF TextBox, Enter and Tab path. The live original cell displayed the requested
text and was no longer editing; original SDK readback matched, row edit ended,
and no validation errors were reported. The process exited 0, original input
hashes passed, and its task and processes were removed. This establishes one
automated in-memory edit only: capture, durable file, no-op and restart tests
remain NOT RUN. Private log SHA-256:
`ea1cdf8b1d0624c7348c48c40edb17d604c26c3a611c5218cf2cf5075350e71c`;
source audit SHA-256:
`3751be30cfc95f6a7b4d7b7cb12145e35543be5f6addc444809c2d331fc39b88`.
Owned Windows regression `dca012f12701` reported 423 passing cases and task
exit 0. The collector again expected an absent `sdk-project.log`; the driver
output was verified separately, with no owned process remaining, and the task
was removed. No original DCA or hardware was exercised by this owned suite.
