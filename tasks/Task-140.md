# Task-140: Map static DCA references against privately supplied SDK candidates

## Subject

Produce a bounded metadata-only candidate dependency/type map for the exact MDT
DCA and two operator-supplied ETS reference DLLs, without loading vendor assemblies.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-140 |
| Status | Done |
| Stage / required gate | PoC reference research; no product gate required or unlocked |
| Priority | P0 |
| Component / Git project | knx-editor / original-DCA compatibility development tools |
| Owner | Primary coordinator with predefined dca, verifier and reviewer agents |
| Requirement references | R-003, R-011, R-023, R-028, R-030 |
| Evidence level | Offline static metadata and owned synthetic fixtures; no vendor execution |

## Description

The user selected a minimal original-DCA compatibility investigation and supplied
private SDK candidates obtained with Task-139. Compare actual assembly identities,
AssemblyRef/TypeRef and SDK-directed MemberRef metadata with candidate definitions.
Report identity differences, missing candidate files/types and unresolved signatures
as observations, not executable requirements or compatible services. No invented
SDK types, business configuration or runtime implementation is needed.

This is narrow .NET-based developer tooling for original-DCA compatibility, not
a product parser, importer or replacement for Task-006's Rust archive inspector.
Task-008's controlled Windows initialization remains separately blocked.

## Detailed Requirements

- DR-001: Restrict real inputs to the pinned original MDT DCA primary assembly and
  the two hash-verified Task-139 candidates. Verify archive/member shape, sizes,
  CRC and SHA-256 before private staging outside Git; no bulk extraction.
- DR-002: Use metadata-only standard readers. No Assembly.Load/LoadFrom/reflection-only
  assembly load, type constructors, attribute instantiation, IL execution or decompilation.
  Do not resolve/fetch external dependencies, alter original bytes or bypass identities.
- DR-003: Bound file count/size, metadata counts, names, signatures, report bytes and
  execution time. Treat malformed input as failure; preserve cancellation/timeout.
- DR-004: For real metadata reads use a separately tested local read-only/network-denied
  child-process policy and bounded parent capture. Microsoft warns PEReader is not
  designed for untrusted input; this research harness is not a production parser or
  a claim that a process alone is a complete security sandbox. Do not silently run
  real inputs if the required restriction/negative probes cannot be established.
- DR-005: Compare exact name/version/culture/public-key identities separately from
  same-name type presence. Type presence and metadata signature observations do not
  prove ABI, framework, binding policy, permissions, UI or state compatibility.
- DR-006: Keep raw SDK metadata/reports private. Commit only original tools, tests,
  source references and a small evidence-based dependency summary. No DLLs/vendor
  implementation, household data or machine paths enter Git.

## Non-functional Requirements

Follow [AGENTS.md](../AGENTS.md), [DCA-CONTRACT.md](../docs/DCA-CONTRACT.md) and
[DATA-MODEL.md](../docs/DATA-MODEL.md). This task uses existing private PowerShell
7.6.7 and System.Reflection.Metadata for compatibility research; it adds no product
runtime dependencies or .NET requirement to the Rust core. Protocol remains solely
in knust. English tooling and structured errors; no implicit network/provisioning.

## Acceptance Criteria

- [x] AC-001: Owned synthetic managed fixtures verify reference/type observations,
  identity mismatch and unsupported/malformed/limit behavior without target execution.
- [x] AC-002: Exact real inputs yield reproducible bounded metadata reports with no
  target assembly loading; required child-process restrictions pass negative probes.
- [x] AC-003: A reviewed summary identifies actual candidate gaps and the smallest
  next experiment, preserving unknown runtime/rights and closed product gates.

## Dependencies to Other Tasks

- [Task-001](Task-001.md), [Task-002](Task-002.md), [Task-004](Task-004.md): Done.
- [Task-139](Task-139.md): Done after final review; input identities/copy bytes verified.
- Explicit user authorization for this minimal compatibility investigation.

No Task-003 Windows host or Task-005 runtime/rights approval is inferred. This task
adds independent static evidence only; it does not start Task-007/008 host execution
or bypass the Task-028 original-DCA gate.

## Required Tests

- TEST-001: Owned synthetic reference/consumer fixtures: metadata-only identity,
  AssemblyRef/TypeRef and matching/missing candidate definitions.
- TEST-002: Negative: invalid/truncated inputs, expected-hash mismatch, file/count/row
  limits, unsupported scopes and bounded failure without target execution.
- TEST-003: Child-process network/write restrictions and deadline/report-size limit
  tested with owned fixtures before any real file reaches the metadata reader.
- TEST-004: Reproduce exact private SDK/DCA input hashes, candidate map and limitations.

## Deliverables

- Minimal compatibility research metadata tool/test harness; no SDK implementation.
- Private raw reports and a concise owned evidence summary, task record, traceability
  links, commands/versions/input hashes, independent verification/review and commits.

## Out of Scope

Original DCA execution, host construction, SDK emulation, whole-contract cloning,
product import implementation, license conclusions, vendor code copying, real
KNX traffic, Secure, publishing and any waiver of existing runtime gates.

## Risks and Blockers

Existing `monodis` cannot execute on this host (`bad CPU type`); it is not evidence
of a completed disassembly. The prepared PowerShell runtime starts under an additional macOS policy
denying network and writes. Owned negative probes independently passed: denied
file creation in a parent-proven-writable directory and socket AccessDenied.
The worker has nine passing synthetic cases; the wrapper has eleven passing
process/policy/publication cases. Real input verification remains required. A nested-policy attempt initially
failed with `sandbox_apply: Operation not permitted`; an authorized invocation of
the same restrictive policy returned PowerShell 7.6.7. No vendor input was used
for this startup probe. Unavailable restrictions block the affected real-file read.

## Completion Record

| Field | Observed result |
|---|---|
| Status | Done; bounded static research scope verified and reviewed |
| Source baseline | Task-139 closure `fe1bef61da837ec91e08f8faa8eb19e1b4556a97`; reviewed implementation `08d16fc5c5eac9469d88092d618860abb9115a2e` |
| Platform / tools | macOS 27 arm64; privately provisioned PowerShell 7.6.7; Python standard-library wrapper |
| Owned worker fixtures | PASS 9 / FAIL 0, independently reproduced by verifier and reviewer |
| Owned wrapper fixtures | PASS 11 / FAIL 0, independently reproduced; deadlines, combined output limit, cancellation cleanup, fail-closed probes/report validation and exclusive private output |
| Restrictions | Actual write-denied and socket AccessDenied probes PASS independently; every real run repeats both; same restrictive profile used under authorized invocation after nested sandbox_apply failure |
| Real input | Three pinned files; guarded metadata experiment PASS, no target assembly execution |
| Private report | 54,179 bytes; SHA-256 `1c1b0f4490565e5518cc517040a7e0bacfbb0d4338cea3a715e69dc295bceb6b`; private staged `metadata-report.json`, outside Git |
| Summary | [Static candidate map](../docs/DCA-CONTRACT-MAP.md) |
| Compatibility outcome | INCONCLUSIVE: requested SDK 5.7 versus supplied 6.4; 27 same-name types, 55 unmatched member signatures, 209 unsupported TypeSpec member parents, additional dependencies unsupplied |
| Limit coverage | File/count/aggregate/row/name negative cases exercised; signature-size and worker-final-JSON-limit rejection source-reviewed only, not separately exercised |
| Windows / hardware / rights | NOT RUN / NOT RUN / UNRESOLVED; no phase gate advanced |
| Review | Predefined sequential_review final PASS, no findings; AC-003 summary reviewed; verifier reproduced exact real report |
| Next action | Separate offline signature/TypeSpec comparison and narrowly selected dependency inventory; runtime provisioning remains Task-005 Blocked |

Commands actually run from repository root:

```sh
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-metadata/Test-ReferenceMetadata.ps1
python3 experiments/dca-metadata/test_run_probe.py
python3 experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --output "$PRIVATE_STAGE/metadata-report.json"
python3 scripts/check_project_scope.py
python3 scripts/check_reference_inputs.py
git diff --check
```

`PWSH` and `PRIVATE_STAGE` are normalized local-path placeholders. The actual
invocation used the private provisioned runtime and stage pointer, without shell
interpolation of imported values. The wrapper itself performs fresh denial probes
before the real metadata read. Raw reports and binaries remain private; no loading,
attribute construction, IL execution, dependency resolution or network fetch ran.
The predefined verifier repeated the guarded command with the new private
`metadata-report-verifier.json` output. Exit 0, empty stdout/stderr; reports are
byte-identical with the SHA-256 above. Both probes passed again. The reviewed source revision is
`08d16fc5c5eac9469d88092d618860abb9115a2e`. Agent review closes this
ordinary static task; it does not supply maintainer approval for any product gate.
