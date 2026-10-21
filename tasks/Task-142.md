# Task-142: Compare static DCA member signatures with private SDK candidates

## Subject

Resolve the bounded signature and TypeSpec observations left open by Task-140.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-142 |
| Status | Done |
| Stage | PoC original-DCA compatibility research; no runtime gate unlocked |
| Priority | P0 |
| Component / Git project | knx-editor / metadata-only compatibility tools |
| Owner | Coordinator with predefined dca, verifier and reviewer agents |
| Requirement references | R-003, R-011, R-023, R-028, R-030 |

## Description

Task-140 observes 55 candidate-directed MemberRefs without matching their
signatures and 209 unsupported TypeSpec member parents. Compare those static
references with direct definitions in the two hash-pinned private SDK candidates.
Record target-framework metadata without loading any target assembly. Do not
turn structural similarity into a runtime binding, ABI or host-success claim.

## Detailed Requirements

- DR-001: Retain Task-140's exact private input identities, bounded reads and
  mandatory guarded real-input execution. Do not resolve or fetch dependencies.
- DR-002: Decode the selected signature grammar with explicit byte, row, node,
  recursion and report limits. Preserve unsupported/malformed distinctions;
  unknown encodings must not be treated as matches.
- DR-003: Compare member names, kind, signature shape and direct declaring type.
  Report requested and supplied assembly identities separately. Any deliberate
  candidate-name mapping is an observation rule, never a binding redirect.
- DR-004: Resolve bounded TypeSpec parent references sufficiently to identify
  candidate-directed generic owners, or report a concrete unsupported reason.
  Do not fabricate inherited/forwarded-member matches or claim absence where
  inheritance, missing dependencies or unsupported signatures remain unresolved.
- DR-005: Read target-framework custom-attribute metadata as bytes only, with
  no attribute instantiation, assembly loading, method-body reading or execution.
- DR-006: Keep reports and all original DLLs private. Commit only original tools,
  owned synthetic tests and a concise evidence summary. Preserve Task-140's
  reproducible default behavior and historical claims.

## Non-functional Requirements

Follow [AGENTS.md](../AGENTS.md) and [DCA-CONTRACT.md](../docs/DCA-CONTRACT.md).
Simon explicitly authorized assumed local use on 2026-10-21 and accepted
responsibility; license-document acquisition is not a prerequisite for this
research. Original DLLs remain outside Git and build distributions. This is not
an agent legal certification. Existing signature/activation checks, hardware
approval and runtime gates remain in force. No Rust-domain or protocol ownership
moves into .NET; this exception is original-DCA metadata research only.

## Acceptance Criteria

- [x] AC-001: Owned managed fixtures distinguish direct signature match,
  mismatch, generic-parent resolution, unsupported/inherited resolution,
  malformed signatures and all selected bounds without target execution.
- [x] AC-002: Guarded real-input runs reproduce a deterministic static member
  disposition and framework report for the exact Task-140 inputs, with input
  hashes unchanged and network/write negative probes passing.
- [x] AC-003: Independent verification/review records exact commands, revisions,
  hashes and limits; no CLR binding, original DCA behavior or phase GO is claimed.

## Dependencies to Other Tasks

- [Task-139](Task-139.md): Done; private extraction/inventory evidence.
- [Task-140](Task-140.md): Done; bounded static map and guarded runtime available.
- Simon's explicit assumed-use decision; Task-005 technical runtime qualification
  remains separate and does not become satisfied by this experiment.

No phase gate required. This task does not start Task-007/008 vendor hosting,
change binary identities, bypass checks or authorize hardware operations.

## Required Tests

- TEST-001: Synthetic direct field/method/constructor and generic-member cases.
- TEST-002: Negative malformed, unsupported, depth/row/blob/report/time limits.
- TEST-003: Existing guarded-wrapper negative probes and regression tests; any
  wrapper extension uses a fixed approved experiment choice, not arbitrary code.
- TEST-004: Independent guarded private-fixture reproduction and hash checks.

## Deliverables

Minimal original metadata comparison tools/tests, private deterministic report,
concise static compatibility evidence, task/index/traceability and source commits.

## Out of Scope

SDK emulation, vendor execution/decompilation, framework or strong-name bypass,
assembly redirects, proprietary code redistribution, dependency fetching,
network/device operations, configuration state proof and full-host compatibility.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Done; final guarded reproduction, independent verification and critical review PASS |
| Source commit(s) | Baseline `b1dd90ca45fdf8de04bb79786dd1d9bcd13b47d2`; reviewed implementation `0b3cfef884a2181e12bf58acf1d08b76312789ed` |
| Tests executed | PASS 31 owned member fixtures; PASS 9 Task-140 worker fixtures; PASS 12 wrapper fixtures; scope checker PASS; git diff --check PASS |
| Evidence location | [Static member evidence](../docs/DCA-MEMBER-MAP.md); private `member-report-task142.json`, 183,722 bytes, SHA-256 `fa494688a2fc5e98b6b22e84e173a5222a72d14cde613169176697d194824667` |
| Acceptance criteria met | AC-001/002/003 met; final static-only scope independently verified and reviewed |
| Technical outcome | Static-only; no target assembly load, attributes instantiated, method bodies, dependency resolution or runtime claim |
| Gate outcome | No phase gate required or advanced |
| Reviewer | Project reviewer PASS after focused owned identifier-reproduction rejection; source, final report hashes, probes, scope, whitespace and no-tracked-DLL checks; no vendor execution |

Implementation checks actually run from the assigned worktree (PowerShell path
normalized to `PWSH`, which denotes the provisioned 7.6.7 executable):

```sh
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-member-map/Test-MemberMetadata.ps1
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-metadata/Test-ReferenceMetadata.ps1
python3 experiments/dca-metadata/test_run_probe.py
python3 scripts/check_project_scope.py
python3 scripts/check_reference_inputs.py
git diff --check
```

The first owned member regression failed before implementation with
`DCAMEMBER_IMPLEMENTATION_MISSING`. Final owned results: 31/31 member cases,
9/9 existing metadata cases and 12/12 wrapper cases PASS. Scope and whitespace
checks PASS. The reference-input checker FAILS in this worktree because all five
configured available input paths are absent (`dali-dca`, `dali-knxprod`,
`simple-knxprod`, `firmware-dca`, `firmware-image`); no input path or availability
claim was changed. The separate Task-140 private staged pins are used only by
the guarded wrapper and do not satisfy this broader inventory check.

The worker and wrapper remain original research tooling only. The distinct framework-attribute
blob overflow guard is exercised through an oversized owned managed fixture,
and incremental report-budget overflow has a focused owned rejection test. Independent private guarded
reproduction, verification/review and source commit remain pending.
Windows and hardware execution: NOT RUN; no gate approval requested or inferred.

The coordinator subsequently ran the fixed guarded experiment successfully:

```sh
python3 experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment member-map --output "$PRIVATE_STAGE/member-report-task142.json"
```

Result: exit 0, empty stdout/stderr; actual write-denied and socket AccessDenied
probes both PASS with PowerShell 7.6.7. Reported input hashes/sizes remain exactly
Task-140's three pins. Static observations: 55 candidate-directed members,
53 direct signature structural matches and 2 direct mismatches with inheritance
unresolved; 209 TypeSpec parent owner scopes decoded, all noncandidate, and no
unsupported/malformed member parent. Both mismatches involve the external
Common.Types identity (`5.7.286.36952` versus `6.4.8718.0`), which is not mapped
or supplied. Raw framework metadata declares .NET Framework 4.6.1 for the DCA and
4.8 for both candidates. Identity, framework and signature observations do not
establish CLR binding, implementation compatibility, UI initialization or state
round trips. Independent verification/review remains required.

The member suite was extended after the initial coordinator run: two additional
owned checks now exercise attribute-blob overflow through the managed Read path
and incremental report-accounting overflow before final serialization. Final
rerun: member 23/23 PASS, Task-140 metadata 9/9 PASS, wrapper 12/12 PASS, whitespace
PASS. The code is frozen for independent verification; no private-input report
semantics changed. Member-helper SHA-256 at that intermediate boundary-test increment:
`0823c8b16976183fe87428b760d27bbe810d0737c403f08bbfe960bd952c67e6`.

The preserved report field `rights=UNRESOLVED` is the tool's no-legal-finding
sentinel, not a prerequisite for a license-document acquisition task. Simon's
explicit assumption authorizes the local static work; the owner decision is
recorded separately by Task-005 commits `5c58448` and `629575f`. Technical Windows
runtime qualification remains separate.

Coordinator pre-verification inspection found a declaring-owner identity gap:
name/type lookup could produce a structural match for a primitive-only member
signature even if the requested owner's culture or token differed. Two owned
negative managed fixtures reproduced it: PASS 23 / FAIL 2 before the fix. The
comparison now requires the requested owner and candidate to agree on assembly
name, culture and token; only version is omitted by the explicit observation
mapping. Otherwise it reports `OWNER_IDENTITY_MISMATCH_UNRESOLVED` without direct
matches. Both requested and supplied identities remain available separately.
Final targeted rerun: member 25/25 PASS, wrapper 12/12 PASS, diff check PASS.
No output-schema or expected real-input disposition changed; final-source
private reproduction remains required. Final member-helper SHA-256:
`ad6fc65f0aa1249c540c17a3a167d9b95e5d1bdf11992988d3de077b7c45fc2f`.

## Initial independent verification before identifier correction, 2026-10-21

The project verifier reproduced all final synthetic suites (25 member, 9
Task-140 metadata, 12 wrapper), scope and whitespace checks: PASS. The final
frozen helper source hash remained
`ad6fc65f0aa1249c540c17a3a167d9b95e5d1bdf11992988d3de077b7c45fc2f`.
The guarded real-input repetition returned exit 0 with both negative probes
PASS and a byte-identical 183,722-byte private report (SHA-256 above).
The verifier initially observed outer-sandbox restriction failure, then ran the
identical authorized command retaining the same deny-default child policy.
The generic input-inventory CLI still returns five absent private-file errors
in this worktree; this is unrelated to the three independently pinned inputs
of the successful guarded experiment. No file was copied or changed by the
verifier. Critical review and source commit recording remain pending.

Independent review finding P2 identified namespace/name/nesting flattening
ambiguity. Five owned regression profiles reproduced it before the fix: each
reported five false structural matches (overall PASS 26 / FAIL 5, including a
positive genuine nested-owner case). The bounded selected profile now rejects
metadata type names containing `.`/`+`, namespaces containing `+`, and nested
types with nonempty namespaces, before either TypeDef or TypeRef flattening.
The explicit failure is `DCAMEMBER_UNSUPPORTED_TYPE_IDENTIFIER`; no apparent
match or absence is reported for rejected input. Broader identifier support is
outside this profile. Both candidate and consumer ambiguity paths are exercised;
the positive nested-owner case continues to produce five structural matches.

Final rerun after P2: member 31/31 PASS, Task-140 metadata 9/9 PASS, wrapper
12/12 PASS, whitespace PASS. Final helper SHA-256:
`adb7bff2901ab183c22c295fec246cfa60f27d7703d53cf0bd1d5dd14ab1cddc`.
The frozen final source requires guarded private reproduction to confirm that
the actual inputs satisfy this selected identifier profile; no vendor file was
read outside the guarded child while implementing this fix.

## Final-source independent verification, 2026-10-21

After the identifier correction, the coordinator and independent verifier each
reran the frozen final source through the guarded member-map profile. Both final
reports are byte-identical to the report above: 183,722 bytes, SHA-256
`fa494688a2fc5e98b6b22e84e173a5222a72d14cde613169176697d194824667`.
Both restriction probes PASS. Final helper source hash before/after verification:
`adb7bff2901ab183c22c295fec246cfa60f27d7703d53cf0bd1d5dd14ab1cddc`.
The independent verifier reproduced 31 member, 9 existing metadata and 12 wrapper
cases, scope and whitespace checks: PASS. The final exact inputs satisfy the
narrowed identifier profile; no source or original input bytes were changed.
The earlier 23/25-case observations and helper hashes describe intermediate
increments. Final critical review remains pending. Windows, CLR binding,
original DCA behavior and hardware qualification remain NOT RUN.

## Final acceptance of static experiment

Independent critical review PASS with no unresolved findings. The original
identifier-collision reproduction now rejects with the documented unsupported
identifier error; genuine nested types retain positive coverage. The reviewer
confirmed the final helper hash and both final reports, and reproduced scope,
whitespace and no-tracked-DLL checks. It reran only its focused owned regression,
not the complete verifier suites. Task-142 is Done for this static experiment;
CLR binding, ABI, vendor behavior, Windows hosting and hardware remain unqualified.
No phase gate or Task-005 runtime qualification is advanced. The exact source
revision is `0b3cfef884a2181e12bf58acf1d08b76312789ed`.
