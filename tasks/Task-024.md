# Task-024: Encode a minimum parameter change into the reference image

## Subject

Encode a minimum parameter change into the reference image

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-024 |
| Status | Blocked |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download compiler |
| Owner | Coordinator; original-reference compiler semantics prerequisite |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Compile the selected small parameter set to its documented binary layout without implementing KNX transport.

## Detailed Requirements

- DR-001: Use explicit bit offsets, lengths, signedness, byte order and constraints from the selected product/profile evidence.
- DR-002: Start from the correct immutable application/segment data; preserve unrelated bytes and reserved bits.
- DR-003: Compile at least one changed value and the default/reference value.
- DR-004: Reject overlap/out-of-bounds/unsupported encodings.
- DR-005: Emit an offline image/diff and no network packets.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Expected and actual changed byte ranges match reviewed reference vectors.
- [ ] AC-002: Repeated compilation from the same input produces identical bytes.
- [ ] AC-003: Invalid parameters cannot produce a downloadable image.

## Dependencies to Other Tasks

- [Task-023](Task-023.md): Parse only the selected KNXPROD application for PoC C. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: min/max/invalid values, bit-boundary changes and reserved-bit preservation.
- TEST-002: Golden: reference default and changed image.
- TEST-003: Negative: missing layout evidence and out-of-range placement.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download compiler**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No general parameter engine or bus write. Parameter image encoding is editor-owned; KNX telegram encoding is not.

## Risks and Blockers

The selected DisplayTimeout candidate has unqualified reference activation and
module occupancy semantics. Task-023 acceptance establishes structural inspection,
not those compiler semantics. Original-reference encoding fails closed until the
missing evidence and qualifier implementation are reviewed. See the
[bounded encoding contract and evidence](../docs/POC-C-PARAMETER-ENCODING.md).

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Blocked on original-reference activation and module-allocation evidence; reviewed byte core and fail-closed boundary remain implemented |
| Source commit(s) | Baseline `8fc4eb51d3c514432637f095a9592b3dc62cb08f`; reviewed partial increment `11619d9a057e4abdaf0ea86318a2e096c2d755e9` |
| Tests executed | Format/Clippy PASS; six byte-core unit and 39 synthetic integration tests PASS; one explicit pinned private original rejection test PASS; three private tests ignored in the default suite |
| Evidence location | [Parameter encoding contract/evidence](../docs/POC-C-PARAMETER-ENCODING.md) |
| Acceptance criteria met | All criteria pending; synthetic byte vectors and rejection tests do not qualify original-reference encoding or a complete image |
| Technical outcome | PASS for synthetic byte encoding and original rejection boundary; INCONCLUSIVE for original-reference encoding |
| Gate outcome, when applicable | No phase gate required or advanced; downstream work remains locked |
| Reviewer | Independent verification and critical review PASS for the partial increment; no Task-024 acceptance |

Update this record only with observed results. Follow the source-code Git policy in [AGENTS.md](../AGENTS.md).


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

## Compiler-semantics readiness review, 2026-10-23

Task-023 is accepted for structural inspection; no phase gate is required.
The remaining original-reference qualification is Blocked on an external
semantic source or independently produced reference compilation. The raw
segment bytes are already available: the pinned Glass V31 archive contains
AS-4690 Data (1,968 bytes, SHA-256
`66d62fbcc9a0a32c8475a69bb7ce3c1ac695c62718741c599c146fb54b26cc30`).
P-252's candidate byte 1522 is 20; the recorded 20-to-21 single-byte vector
can be reproduced without acquiring a separate image. That observation does
not prove parameter activation or module non-overlap.

A bounded official-source search and targeted reads of the supplied standard
confirm that the reviewed material does not define the required project/20
`BaseOffset`, `NumericArg.Value`, `Allocates` or `choose/when` semantics.
[Project Scheme Documentation v1.0.0](https://support.knx.org/hc/en-us/article_attachments/360024169360),
22 March 2019, covers ETS5/MT5 5.7 and schema project/20; section 1.1 p4
explicitly excludes manufacturer product dependencies/visibility and
download-image creation from its scope. The official
[ETS5 SDK overview](https://support.knx.org/hc/en-us/articles/360015340940-How-to-modernize-your-App)
describes module activation and allocators at a high level, without the
missing arithmetic or evaluation rules. The newer
[project/23 description](https://support.knx.org/hc/en-us/article_attachments/17389755651474)
is corroboration of document scope, not compatibility evidence for project/20.
The bounded search does not prove that no restricted or other documentation
exists. No licensing condition is added by this technical prerequisite.

Three additional supplied PDFs were extracted locally with the existing
bounded Swift PDFKit helper: all exit 0 with empty stderr. The compile command, PDF/text/helper hashes and results are in private
`task024-primary-additional/extraction-record.json`; the three exact text-mode
argument arrays reconstructed from the coordinator execution record are in
`task024-primary-additional/extraction-command-record.json`. No extraction was
repeated to prepare that supplemental command record. Application Programmer's
Interface v01.01.01 describes device APIs; Configuration Procedures
v02.01.01 pp39-42 defines device-segment allocation and Mode/Fill, while
pp66-68 describes manufacturer-specific mask-5705 memory/merge inputs; XML
Data Encoding v01.01.01 refers to KNXnet/IP XML interchange. Their bounded
text searches contain no relevant module/conditional definitions. Device
segment allocation cannot establish XML ModuleDef arithmetic by similar names.
Independent explorer inspection confirmed those limits without executing
manufacturer code, build tests, network packets or hardware.

To unblock original output, obtain compatible authoritative manufacturer-
product semantics for these fields, or an independently produced and reviewed
reference compilation for this exact application/profile that establishes
activation and memory behavior. Existing private candidate vectors and
synthetic core tests are not such an independent reference. Keep the public
original encoder fail-closed; no source, API, test expectation, acceptance
criterion or downstream gate changes. Current status is Blocked for that
remaining prerequisite; the earlier In progress entries are historical
implementation/research checkpoints. Independent critical documentation review PASS; no unresolved documentation finding. Task-024 remains Blocked; source acceptance, acceptance criteria and gates are unchanged. Private review evidence: `task024-primary-additional/review/critical-review.txt`.


## Historical reconciliation, 2026-11-03

The clean integration base is `1457d78` on `task/Task-024-reconcile`.
Task-023 is Done at this base; no phase gate is required. Historical source
`11619d9a057e4abdaf0ea86318a2e096c2d755e9`, review record `a59009d` and
semantics blocker record `386fc0e` were selectively reconciled. Only the
encoder module, its library exports and tests, this task record and the encoding
contract were integrated. No branch merge was performed. The current Task-023
resource implementation and all unrelated components were preserved.

The missing public encoding API on this base was the reproducible observation;
its historical tests and implementation were restored together. Source
`lib.rs`, `parameter_encoding.rs` and `tests/parser.rs` now match the reviewed
historical file hashes in the encoding contract. `src/resources.rs` remains
unchanged at SHA-256
`cb941e2dc57dda13568c099111dbabb82ded8dca522ec66eafced74dd18d95c3`.
Cargo.lock is unchanged. No original manufacturer bytes were added to Git.

Reproduction used macOS/Darwin 27.0 arm64, provisioned Rust 1.99.1, offline
Cargo cache and an isolated temporary target directory. Commands actually run:

```sh
cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check
cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings
cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features
cargo test --manifest-path poc/product-download/Cargo.toml --locked --offline --no-default-features registered_ -- --ignored --nocapture
python3 scripts/check_project_scope.py
git diff --check
shasum -a 256 poc/product-download/src/lib.rs poc/product-download/src/resources.rs poc/product-download/src/parameter_encoding.rs poc/product-download/tests/parser.rs poc/product-download/Cargo.lock
```

The explicit private run supplied KNXPROD_FIXTURE and KNXPROD_GLASS_FIXTURE;
the tests verified the archive hashes recorded above before inspection.
Format, Clippy, six byte-core unit tests, 39 synthetic integration tests,
all three registered private tests, scope and whitespace checks PASS.
The private tests establish the two structural inventories and original encoder
rejection only. Original-reference encoding remains INCONCLUSIVE/unsupported;
complete-image, Windows/Linux, DCA, network and hardware tests are NOT RUN.
Independent review of this bounded reconciliation passed. Task-024 remains
Blocked, all acceptance criteria remain pending and Task-025 is not unlocked.
