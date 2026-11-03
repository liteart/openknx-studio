# Task-023: Parse only the selected KNXPROD application for PoC C

## Subject

Parse only the selected KNXPROD application for PoC C

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-023 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/product-download (Rust) |
| Owner | Rust implementer; bounded increment independently verified/reviewed |
| Requirement references | [R-003](../docs/REQUIREMENTS.md), [R-008](../docs/REQUIREMENTS.md), [R-009](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Extract only the product structures required for the chosen simple device/application.

## Detailed Requirements

- DR-001: Select the exact product/application identity and record package hash/schema namespace.
- DR-002: Parse the necessary parameter definitions/references, communication objects, binary resources, mask/load metadata and required offsets.
- DR-003: Keep original resources immutable and reject unknown constructs affecting the selected compilation.
- DR-004: Apply basic archive/XML limits and deny external entities/network access.
- DR-005: Report unsupported branches rather than implementing a universal parser.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [x] AC-001: The selected application resolves to a small explicit intermediate model.
- [x] AC-002: All required binary resources and references have validated identities/lengths.
- [x] AC-003: A different/unsupported application is rejected before compilation.

## Dependencies to Other Tasks

- [Task-003](Task-003.md): Pin minimum PoC toolchains and verification commands. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: necessary XML constructs and reference resolution.
- TEST-002: Fixture: extract the registered application and compare independently reviewed values.
- TEST-003: Negative: missing resource, unexpected namespace and unknown critical construct.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/product-download (Rust)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No catalog database, complete schema support, DCA execution, or MCP layer.

## Risks and Blockers

The matching KNXPROD and profile documentation remain external inputs. Do not derive a complete product image from the DCA manifest alone.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Done for selected GlassV31/project20 structural import; current-main reconciliation independently reviewed |
| Source commit(s) | Accepted profile `426110e6cc958bd87043906b51842b9817ee16aa`; resource validation `3a1d5db5609c7c4aee4997b5127e1858a58c5426` |
| Tests executed | Format, Clippy warnings denied, 38 synthetic tests and both registered private inventories PASS; scope and whitespace PASS |
| Evidence location | [Glass parser evidence](../docs/POC-C-GLASS-PARSER.md), [reference/schema evidence](../docs/REFERENCE-BE-GT20S-02S.md), private/evidence/task023-resource-inventory/final-verifier/ |
| Acceptance criteria met | AC-001, AC-002 and AC-003 PASS for structural inspection only |
| Technical outcome | PASS: selected identity/IR and required structural resources; compilation, full image and download NOT RUN |
| Gate outcome, when applicable | No gate required or advanced; no hardware approval |
| Reviewer | Independent project reviewer PASS for frozen source, resource evidence and all three structural acceptance criteria |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Current-main reconciliation — 2026-11-03

The reviewed source and evidence below existed on the historical
`task/Task-023-resource-qualification` branch but had not reached current main.
This reconciliation restores only the selected three parser files and four
Task023/component documents on base `66ce052`. Modern project identity, DCA
work, dependency qualifications, other tasks, Cargo manifests and lockfiles
are preserved. The older binary-input checkpoint below remains historical;
the accepted resource result applies only to the explicit GlassV31 profile.

Before restoring the source, the focused
`glass_external_reference_requires_baggage_document` regression failed: the
old parser returned an IR v2 snapshot for a missing required baggage document.
The restored validation rejects it. With the pinned Rust 1.99.1 toolchain and
offline locked cache, the following reconciliation checks passed:

- `cargo fmt --manifest-path poc/product-download/Cargo.toml --all -- --check`
- `cargo clippy --manifest-path poc/product-download/Cargo.toml --workspace --all-targets --locked --offline --no-default-features -- -D warnings`
- `cargo test --manifest-path poc/product-download/Cargo.toml --workspace --locked --offline --no-default-features` — 38 synthetic tests; two private tests ignored by default.
- The same test command with `registered_ -- --ignored --nocapture` and explicit `KNXPROD_FIXTURE` / `KNXPROD_GLASS_FIXTURE` paths — both registered fixtures passed.
- `python3 scripts/check_project_scope.py` and `git diff --check`.

The unchanged private fixture hashes matched the registered values:
binary input `51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709`
and Glass `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d`.
Independent integration review passed for the selected source and evidence;
the main-branch integration commit records this checkpoint. It does not
advance a phase gate or qualify encoding, download or hardware.


## Bounded increment and remaining acceptance

The standalone `poc/product-download` crate inspects only the selected
BE-04000.02 product/application candidate and preserves original bytes, hashes,
IDs, scopes and declared offsets. It rejects alternate application IDs,
unknown application constructs, malformed archives/XML and invalid typed
references. It does not evaluate product scripts, module instances, unions,
conditional activation or parameter type semantics, and cannot compile or
execute an operation. Exact observed metadata and reproducible commands are in
the evidence document.

- AC-001: PASS for a small static declaration IR; this is not a resolved
  effective application configuration.
- AC-002: PARTIAL / INCONCLUSIVE. Embedded resources have validated identities,
  strict decoding, bounded lengths and source declarations; known required
  static references have validated kinds/scopes. Two declared segments lack
  embedded Data; all four present Data payloads match declared segment sizes. No
  authoritative selected-profile evidence establishes whether this is legitimate
  or whether all required resources are present. Downstream parameter/module/
  union/dynamic/load evaluation is separate Task-024 work, not an added
  Task-023 acceptance criterion.
- AC-003: PASS for rejection of unsupported application IDs and conflicting
  selected identity before any compiler (none exists in this increment).

Task status is Blocked, not Done. No device/application/profile match,
non-secure qualification, complete image, human hardware grant or phase GO is
asserted. Independent verification and critical review PASS for the bounded increment.
The next prerequisite is authoritative selected-profile resource evidence for
AC-002; Task-024 remains locked until Task-023 meets its stated criteria.

## Independent verification and review, 2026-10-21

The project verifier reproduced format, Clippy, 22 synthetic tests (one private
test ignored by default), the separate private fixture test, scope/diff checks
and locked offline feature metadata using Rust 1.99.1 on macOS arm64 and an
isolated target directory. Root features were empty; 22 external packages were
resolved. Fixture bytes and lockfile hashes matched the recorded values.

The critical reviewer found no actionable issue in the eight-file bounded
increment, reran the verifier-built synthetic and private-fixture tests and
independently compared selected XML hashes with Python. The reviewer did not
rerun compilation, formatting or Clippy. Review PASS permits committing this
increment, while ordinary Task-023 acceptance remains INCONCLUSIVE solely on
the unproved resource-completeness obligation. Windows/Linux, hardware, vendor
execution and compilation of a device image remain NOT RUN.

## Operator-selected fallback, 2026-10-21

The user asked for another primary-document search and selected MDT BE-GT20S.02S
as fallback if the first reference could not be qualified. The explorer found
and privately downloaded the official V3.1 family archive. The coordinator
independently reproduced hashes, counts, application selection and resource
lengths. See [fallback reference evidence](../docs/REFERENCE-BE-GT20S-02S.md).

The new requested reference has four exact-length Data payloads and two absent
payloads, so AC-002 remains INCONCLUSIVE. Public documentation did not establish
omission semantics. No parser profile or bound was changed: the original tested
profile remains reproducible, and candidate-specific support is NOT RUN. The
replacement is an investigated reference, not accepted compiler input. Task-023
remains Blocked until its stated resource obligation is resolved. No manufacturer
binary/XML was committed and no hardware, DCA or compiled image was exercised.

Checks: private archive and selected XML SHA-256/count/resource comparisons PASS;
`python3 scripts/check_project_scope.py` and `git diff --check` PASS. No Rust test
rerun is claimed for this documentation-only increment. Independent critical review PASS: the reviewer reproduced archive/member hashes,
counts, selected identity, catalog links and six resource lengths, checked primary
sources and matched the corrected acquisition provenance. No acceptance or gate
was advanced.

Reviewed fallback-reference source commit: `7097e84c20c70a9a708337b52ea00721ee13fcec`.

## Follow-up range-reference investigation, 2026-10-21

Static research and independent coordinator reproduction distinguish omitted
Data from proven missing required bytes. In the selected BE-GT20S.02S family
input, all 539 parameter/union Memory references and all three table placements
target segments with Data. Neither omitted-data segment ID is referenced by
another attribute. Both omitted address/size ranges nevertheless occur in the
custom ProductProcedure. Exact findings and limits are recorded in
[the reference evidence](../docs/REFERENCE-BE-GT20S-02S.md).

The coordinator reran a bounded Python standard-library ZIP/XML observation:
archive and member SHA-256 pins match, DTD/entity declarations are excluded,
entry/expanded/member/element counts are bounded, and the exact attribute and
numeric-range observations agree. No original payload was extracted or changed.
No Rust, vendor execution, compiler or hardware test was run for this
documentation increment. AC-002 remains INCONCLUSIVE and Task-023 Blocked;
the investigation does not establish transfer or zero-fill semantics.

Independent critical review PASS: the reviewer reproduced the bounded private
ZIP/XML observations and both hashes, confirmed the parser's optional Data
representation, and found no unsupported semantic or gate claim. Commands
`python3 scripts/check_project_scope.py` and `git diff --check` PASS.

Reviewed range-reference investigation source:
`8c05df2517bf249d073abaec44efc26a40040e6a`.

## KNX Standard v3.0 resource-semantics review, 2026-10-22

Private KNX Standard v3.0.0 source copies and their rendered pages were reviewed
for the two ProductProcedure ranges. Management Procedures v02.01.02,
§3.31.3.4, pages 139–140, defines segment type 0 as absolute data/code
allocation and segment type 1 as absolute stack allocation; memory type 2 is
RAM. This identifies AS-0700 as a data/code range and AS-099A as a RAM stack
range, but does not state what omitted KNXPROD `Data` means for either range.
Profiles v02.01.01, Appendix A.2.4.1, table 7, page 144, footnote 66
conditions the marked System 300 mask `0300h`/`2300h` cells on additional
data being downloaded. The observed `MV-0705` occupies a separate column,
so this footnote does not prove its controls or these ranges optional.
Page 145 distinguishes global device support from individual resource controls. Resources
v01.10.01, §4.23.3, page 297, leaves memory-mapped load-state-machine
realization unspecified. XML Data Encoding v01.01.01 concerns KNXnet/IP XML
data exchange, not KNXPROD application-memory omission semantics. See the
[reference investigation](../docs/REFERENCE-BE-GT20S-02S.md) for private source
hashes and detailed limits.

- AC-002: INCONCLUSIVE. The applicable product profile and required
  initialization/transfer semantics for the AS-0700 and AS-099A ranges remain
  unknown. Existing observations and parser behavior are unchanged.
- Task status: Blocked. This standards review does not qualify the candidate,
  establish resource completeness, or unlock Task-024 or any gate.
- Technical result: INCONCLUSIVE for resource completeness; PASS only for this
  bounded source-document review.
- Gate result: NOT APPLICABLE; no gate was advanced.
- Implementation and compiler semantics: NOT RUN and unchanged.
- Rust, Windows, DCA, vendor execution, compiled image, and hardware checks:
  NOT RUN for this documentation increment.

The reviewed private source PDFs are identified by SHA-256 in the linked
reference note; PDF bytes are not included in Git. The source baseline is `26512a409452c2c406ebaf2176fb56e9aaa4fbe3`;
this reviewed increment changes documentation only.

Independent critical review: predefined `project_review` verified all four PDF
hashes and relevant rendered pages. One source-scope finding was corrected in
both records: Profiles footnote 66 is restricted to its marked System 300
cells and cannot be generalized to MV-0705. Re-review PASS without findings;
scope and whitespace checks independently PASS. Task-023 remains Blocked.


## Original project/20 schema evidence, 2026-10-22

The earlier lack of evidence for optional segment Data is superseded for the
selected project/20 reference. A private copy of the installed ETS 6.4.8718.0
`Knx.Ets.Xml.ObjectModel.dll` is 4,967,888 bytes, SHA-256
`db6f21679d046eaef4909121a553af52fa377fa582552faca6ec0fed5f7f51e1`.
The bounded static byte-slice observation did not load or execute that assembly.
Its exact project/20 XSD slice starts at byte 4,168,168, is 277,963 bytes, and
has SHA-256
`5dce7ad6f1c742f3772963c9ca00f7fd5809ae9538b4993fa816266cdc55de8a`.
Private provenance and declarations are recorded under
`private/evidence/task023-schema-original/`, including
`schema-fragments.json` and `selected-schema-declarations.json`.
No original DLL or XSD is included in Git.

The schema's AbsoluteSegment extends SegmentBase_t. Its Data and Mask child
elements are optional (minOccurs=0); segment Id, Size and Address are required.
Consequently, the selected application's two absent Data values are structurally
valid absence, retained as None. This establishes neither zero-fill nor initial
RAM contents, transfer semantics, a complete image or a valid load procedure.
It does not qualify the distinct project/11 Light reference.

Independent critical review verified the pinned DLL, exact slice, namespace
and declarations. AC-002 remains pending the separate bounded external-resource
reference inventory; schema validity alone does not validate baggage payloads.
Existing downstream evaluation/load/compilation blockers remain enforced.

The coordinator reproduced the schema assertion with
`python3 private/evidence/task023-schema-original/verify_project20_schema.py`
(exit 0). The private owned script SHA-256 is
`1e8f9d792e029dfebc43b458981306e56ca69412177bf8010c37632c86d8c1f6`;
its JSON result SHA-256 is
`260ddc01acb7d2a882192cfe5402a25ff042c74cd2625b22e5b0b324460ddca7`.
The script compares exact pinned bytes and schema declarations only; it makes
no initialization or executable-host claim.


## Resource qualification implementation, 2026-10-22

The current explicit selected profile is GlassV31, application
M-0083_A-018B-31-369F, namespace project/20, archive SHA-256
`b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d`.
This follows the accepted Task-145 parser source
`426110e6cc958bd87043906b51842b9817ee16aa` and recorded revision
`54fff13475b7b4b91942f96859b5cff36978f370`. Earlier BE-04000.02 and fallback
investigation entries are historical; this increment qualifies the selected
Glass profile's structural resources. No exact physical device match is asserted.

The original project/20 schema resolves optional absent Data structurally.
The new typed resource join resolves each TypePicture/Baggage reference through
Baggages.xml, validates its exact observed hierarchy and flat empty-TargetPath
mapping, requires its archive payload, and retains immutable bytes, length,
SHA-256 and source identity. Missing references/payloads, duplicate mappings,
wrong kinds, unsupported paths/attributes and malformed XML fail explicitly.
IR version 3 adds external resource/reference records; default profile resource
semantics remain unchanged. All compiler blockers remain.

The independent inventory is
`private/evidence/task023-resource-inventory/smart-glass-baggage-inventory.json`,
SHA-256 `d3980d9743a2a28cae7f2627f99eb8f71f1ed02aeb5940b747caaec3813ec7a6`.
All 93 application references resolve uniquely to 48 present payloads, totaling
56,378 bytes, with no missing or ambiguous resource. The coordinator separately
re-read all 48 payloads and the 9,400-byte Baggages.xml from the unchanged pinned
archive and reproduced their lengths/CRC/hash observations. The sorted full
identity/length/hash vector is 7,090 bytes, SHA-256
`97fb12d42558d742119cc098cfdcd8ec693f29accf1fa00222db51fd9c9959ab`;
the private registered fixture test checks this independent golden value.
No manufacturer payload bytes are committed.

Initial missing-document regression FAIL (exit 101) reproduced the old opaque
success. Final writer checks PASS: formatting, Clippy with warnings denied,
38 synthetic tests (two private tests ignored), one explicit Glass fixture,
scope and whitespace. The independent verifier then reproduced formatting,
Clippy, all 38 synthetic tests and BOTH registered private fixtures (original
BE-04000.02 and Glass: one test each), scope and whitespace PASS with Rust/Cargo
1.99.1 on macOS 27.0 arm64. Exact commands, frozen source/fixture hashes and
logs reside in
`private/evidence/task023-resource-inventory/final-verifier/`.
The detailed public commands and supported subset are in
[POC-C-GLASS-PARSER.md](../docs/POC-C-GLASS-PARSER.md) and
[POC-C-STATIC-IR.md](../docs/POC-C-STATIC-IR.md).

Critical review and final Task-023 acceptance are pending. AC-002 remains
unchecked until that review confirms the structural evidence suffices. The
remaining 7,310 global-condition edges, parameter/union/module evaluation,
initial RAM contents, UI behavior and load procedure semantics are compiler
or downstream proof obligations. No complete image, downloadable plan, hardware
qualification, Windows/DCA runtime result or phase GO follows from this work.


## Final structural acceptance, 2026-10-22

Independent critical review PASS confirms AC-001/002/003 for the selected
GlassV31/project20 input. This final entry and the completion table supersede
all earlier blocked/pending resource-completeness entries above. Source is
committed at `3a1d5db5609c7c4aee4997b5127e1858a58c5426`. The reviewer reproduced formatting,
38 synthetic tests, scope and whitespace PASS and inspected the independent
Clippy/both-private-fixture PASS logs; the reviewer did not rerun those private
fixtures. No unresolved source review finding remains.

Task-023 is Done. Optional segment Data remains None, with no inferred zeros.
All known external resource identities, lengths and references are validated
for this selected profile. Compilation blockers remain explicit and unchanged;
resource availability does not authenticate the vendor or validate PNG content.
Task-024 may now begin its own minimal offline parameter-encoding work.
Conditional/module/union semantics, initial RAM contents and load behavior are
not accepted by this task. Hardware, device identity, full image construction,
Windows/DCA runtime and application download remain NOT RUN. G-POC stays closed.
