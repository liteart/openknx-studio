# Task-023: Parse only the selected KNXPROD application for PoC C

## Subject

Parse only the selected KNXPROD application for PoC C

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-023 |
| Status | Blocked |
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
- [ ] AC-002: All required binary resources and references have validated identities/lengths.
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
| Implementation / report status | Blocked on AC-002 resource evidence; bounded increment verification/review PASS |
| Source commit(s) | Baseline be5b409; reviewed increment commit pending coordinator |
| Tests executed | Owned regression and registered private fixture PASS; final checks recorded in linked evidence |
| Evidence location | [PoC C parser evidence](../docs/POC-C-PRODUCT-PARSER.md) |
| Acceptance criteria met | AC-001 static declaration IR and AC-003 tested; AC-002 resource completeness remains INCONCLUSIVE; evaluator/compiler work is separate |
| Technical outcome | PASS for bounded offline static inspection; INCONCLUSIVE for compilation-ready application |
| Gate outcome, when applicable | No gate required or advanced |
| Reviewer | Project reviewer PASS for bounded increment; AC-002 remains INCONCLUSIVE for resource completeness |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).


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
  embedded Data and some segment data is shorter than declared capacity. No
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
