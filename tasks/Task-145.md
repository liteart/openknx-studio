# Task-145: Inspect the explicit Glass V3.1 static profile

## Metadata and readiness

| Field | Value |
|---|---|
| Task ID | Task-145 |
| Status | Done |
| Component | knx-editor / poc/product-download (Rust) |
| Requirements | R-003, R-008, R-009, R-024, R-028, R-030 |
| Baseline | Task-144 `0856e71`; reviewed Task-023 bounded parser `8cdcb569fc224fd4bddaa21f38d9aacc7a5d44c0`, locally imported as `baaa788` |
| Reference evidence | Reviewed Task-023 investigation `8c05df2517bf249d073abaec44efc26a40040e6a` |

The coordinator authorized this independent static increment while the operator
prepares the isolated Windows VM. The accepted bounded parser and reviewed
reference investigation are prerequisites; whole Task-023 acceptance is not
claimed. Task-023 remains Blocked on AC-002 resource semantics, and Task-024 and
phase gates remain closed. No phase gate is required for this offline task.
Read [the static IR contract](../docs/POC-C-STATIC-IR.md), AGENTS, GOALS,
DATA-MODEL and ARCHITECTURE before editing.

## Scope and acceptance

- AC-001: Explicit opt-in `GlassV31` profile selects only the pinned non-temperature
  application/family links. The original API, identity and fixed limits remain.
- AC-002: Fixed separate Glass limits bound archive/member/total/entries/elements/
  depth. Known new constructs/attributes/placements are allowed only for Glass;
  unknown critical shapes fail closed and typed reference checks remain.
  The coordinator-approved scope amendment below preserves one unverified case.
- AC-003: IR v2 preserves ordered anonymous-union members, owning memory kind,
  immutable-source identities/ranges and module scope. No array position becomes
  a business identity. Absent segment Data stays absent.
- AC-004: Opaque load/resource/rename declarations remain unverified blockers;
  every snapshot retains compilation blockers. No evaluation, compiler, SDK,
  protocol, device qualification or network functionality is implemented.
- AC-005: Synthetic default/profile/isolation/union/scope/reference/limits/opaque
  declaration regressions and explicitly selected private fixture inventory pass;
  format, Clippy, standalone locked tests and scope checks pass. Independent
  verification/review and coordinator acceptance remain required.

## Ownership and tests

Allowed paths: `poc/product-download/src/{lib,archive,xml}.rs`,
`poc/product-download/tests/parser.rs`, this task, TASKS.md, TRACEABILITY.md and
`docs/POC-C-{GLASS-PARSER,STATIC-IR}.md`. No dependency/toolchain changes.
Private manufacturer bytes must not enter Git. Test fixtures are owned synthetic
XML; private inventory is ignored by default and requires an explicit environment
path and exact archive/member hashes.

## Initial observation

The original parser cannot inspect Glass: its identity and 16-entry/2 MiB member/
8 MiB expanded limits reject the 57-entry, 21,193,051-byte expanded archive and
9,861,680-byte selected XML. Independent bounded static inspection observes
90,988 elements and depth 27. Picture, baggage and rename constructs and several
attributes/placements require profile-specific handling. No resource omission
semantics follow from this observation.

## Completion record

Implementation checks PASS: format, Clippy warnings denied, 32 synthetic tests,
two explicitly selected private inventories, planning scope and whitespace.
See [the evidence record](../docs/POC-C-GLASS-PARSER.md) for exact commands,
source hashes, platform and input identities. Source/tests are frozen at
predecessor HEAD `baaa788fb6293b04c054d001a51f854b7ebe56fb` plus the reviewable
reviewed diff. Independent verification and critical review PASS with no
unresolved findings; the coordinator accepts this bounded increment. All five
acceptance criteria are met. Reviewed source commit:
`426110e6cc958bd87043906b51842b9817ee16aa`.
Hardware, Windows,
DCA, compiler and device operations: NOT RUN. Resource omission/global condition
semantics: INCONCLUSIVE. Task-023 and phase gates remain unchanged.

## Coordinator-approved scope amendment

The real input exposes 7,310 `choose/@ParamRefId` edges inside ModuleDef to
global ParameterRef declarations (MD-1: 3,706; MD-2: 3,604). The coordinator
authorized preserving only this observed shape as typed unverified-scope records
and explicit compilation blockers. Existing target-ID/kind checks still run;
missing/wrong-kind, module-to-module, every other scope crossing and all default
profile crossings remain fatal. This is static observation, never semantic
reference qualification. Owned regressions cover both profiles and negative
cases. No condition evaluation was added.

Missing ModuleDef IDs are rejected before contained IR is formed, in both
profiles; anonymous unions retain source identities. This fixes malformed-input
conflation with global scope without altering valid default identity/limits.

Independent critical review covered all nine explicit paths against `baaa788`.
The reviewer confirmed the corrected Rename scope and missing ModuleDef ID
regressions, frozen source/lock hashes, binary exclusion and planning/whitespace
checks. The verifier independently reproduced format, Clippy, 32 synthetic
tests and both private inventories. This is no compiler, hardware or phase-GO
acceptance.
