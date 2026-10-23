# Task-005: Qualify owner-authorized DCA and SDK dependencies

## Subject

Qualify owner-authorized local DCA and SDK dependency compatibility

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-005 |
| Status | Done |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / dca-host dependency review |
| Owner | Simon |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-002](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

**Current status reaffirmed 2026-10-23: Done.** Simon confirmed permission for
private copies and prohibited redistribution of the proprietary DLLs. The
existing selected-route compile and ETS-free constructor evidence already
satisfies this task; no new Windows execution was needed for this reaffirmation.
See the final acceptance and the status reconciliation below.

Historical dated evidence below records earlier blockers and decisions. The
owner decision supersedes earlier legal-document, provisioning-rights and
legal-review prerequisites. Earlier technical blockers are dated snapshots;
the accepted completion record governs the current Done result.

## Description

Following [ADR-005](../docs/ADR-005-owner-authorized-private-dependencies.md), use the private local references under Simon's owner decision and qualify whether the original reference DCA can receive the contracts it needs without ETS at runtime. Keep original DLLs outside Git and build distributions. Distinguish original binary compatibility from partial feature alternatives; any change to original-DCA-first scope requires an explicit decision.

## Detailed Requirements

- DR-001: Record Simon's owner decision and the private-only boundary for original DLLs; do not present it as an agent legal determination.
- DR-002: Separate compile-time reference contracts from concrete proprietary ETS runtime services.
- DR-003: Record whether dependencies can be built and run on a clean machine without installed/active ETS; distinguish private development references from build/distribution contents.
- DR-004: Record strong-name/assembly identity and activation constraints for follow-up; do not bypass them.
- DR-005: Keep a technical dependency disposition table: available, missing, technically unsupported, or unverified.
- DR-006: Compare public SDK documentation/demos, independently implemented minimal compatibility surfaces, protocol libraries, ETS-hosted bridges, and manufacturer standalone features using primary sources. Record unknown technical identities/capabilities and reject unproven drop-in claims.
- DR-007: Separate static metadata investigation from runtime qualification. Do not waive task dependencies or runtime acceptance; describe the smallest technical experiment needed.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/technical inputs are recorded as blockers or NOT RUN, never inferred PASS. Apply ADR-005 for the owner-authorized local-use assumption.

## Acceptance Criteria

- [x] AC-001: Simon's owner decision authorizes assumed local use of private references and accepts responsibility; it is recorded as a project decision, not legal certification.
- [x] AC-002: Original DLLs remain outside Git and all build distributions; approved extraction is limited to private local storage.
- [x] AC-003: Technical qualification demonstrates that the selected startup/constructor route does not require installed/active ETS at runtime; accepted Task-148 supplies that bounded proof. Initialize/UI/state compatibility remains assigned to Tasks 008–012.

## Dependencies to Other Tasks

- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Review: verify owner decision record, private-only extraction boundary, and technical dependency evidence.
- TEST-002: Reproduction: inventory and build against the exact selected private references; record assembly identities and dependency closure.
- TEST-003: Negative: unresolved or mismatched contracts, activation dependencies, or ETS runtime requirements block the affected capability.
- TEST-004: Alternatives review: each candidate has a primary source, exact claimed capability, unresolved prerequisites and disposition. A partial web workflow, transport API or running-ETS bridge must not count as unchanged original-DCA hosting.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / dca-host dependency review**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No agent legal certification, no binary patching, no full SDK reimplementation.

## Risks and Blockers

The current selected-route status and remaining compile check are recorded in the latest 2026-10-22 section below. Accepted Task-148 supplies ETS-free startup/constructor evidence; it does not prove Initialize/UI/state compatibility or bus mediation. Local owner authorization does not certify legal rights or permit original DLLs in Git or distributions.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, technical SDK compatibility or hardware outcomes.

## Historical completion record: initial blocker report

The current selected-route completion record is in the latest dated section.

| Field | Observed result |
|---|---|
| Implementation / report status | Blocked; owner authorization decision recorded, technical host/dependency qualification remains incomplete |
| Source commit(s) | Blocker report `6eff4797d37d3c6f9b22ac6191e0cb33b7b47c01`; source baseline `94358b5918ddc7a3ff0f3f0d16153d1b40c0954a` on `task/Task-005-sdk-provisioning`; Task-004 source revision `4f3ddb7f904c37f9b7a159a4bbd6653d1610a1b8` |
| Tests executed | `python3 scripts/check_reference_inputs.py --require sdk-references-rights`: expected negative case PASS, exit 1; `python3 scripts/check_project_scope.py`: PASS, exit 0; `git diff --check`: PASS, exit 0. TEST-001: INCONCLUSIVE; TEST-002: NOT RUN; see [DCA-PROVISIONING.md](../docs/DCA-PROVISIONING.md) |
| Evidence location | [DCA-PROVISIONING.md](../docs/DCA-PROVISIONING.md); [reference inputs](../docs/REFERENCE-INPUTS.md); [static DCA inventory](../docs/REFERENCE-DCA.md) |
| Acceptance criteria met | AC-001/002 met by ADR-005 and private-only handling. AC-003 remains unmet pending technical Windows qualification. |
| Technical outcome | INCONCLUSIVE: exact requested SDK 5.7.554.37315 versus private candidate 6.4.8718.0; Task-142 finds 53 structural matches and two unresolved external Common.Types identity comparisons; all 209 TypeSpec owner scopes decoded; dependency closure and ETS-free runtime unverified |
| Gate outcome, when applicable | No phase gate applies or was advanced |
| Reviewer | Predefined sequential_verifier and sequential_review: PASS for blocker-report commit only; full Task-005 remains Blocked |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

The independent verifier and reviewer reproduced the SDK/clean-Windows required
input check (expected exit 1), scope checker PASS and diff check PASS. The reviewer
returned PASS for this bounded report with no blocking findings. This permits the
report to commit under the user's ordinary-task workflow; TEST-001 remains
INCONCLUSIVE and TEST-002 NOT RUN, so Task-005 is not Done and no dependent
implementation or phase gate is unlocked. The exact blocker-report source commit is
`6eff4797d37d3c6f9b22ac6191e0cb33b7b47c01`. No SDK assemblies were copied or executed.

## Operator provisioning update, 2026-10-21

The operator explicitly confirmed having neither the ETS6 SDK package nor active
KNX membership. The documented member-resource acquisition route is therefore
unavailable through this operator's account. This supersedes the earlier open
question about that access. The operator's expectation that the license is
restrictive is not treated as evidence of any actual term or prohibition.

Task-005 remains Blocked: exact SDK references and applicable terms are absent;
no compatible standalone provisioning route is established. No membership purchase,
third-party contact, copied ETS runtime dependency, or substitute SDK implementation
is authorized or performed by this update. Existing terms may be reviewed if a
lawful source becomes available. Rust-only work does not acquire an SDK dependency
from this report; existing task dependencies and gates remain unchanged.

## Alternatives research update, 2026-10-21

The user explicitly requested online alternatives research and corresponding task
adjustment. The coordinator and predefined explorer independently reviewed the
primary sources in [ETS SDK alternatives](../docs/DCA-SDK-ALTERNATIVES.md).
DR-006/007 and TEST-004 make that comparison explicit; no runtime criterion was
removed. No working membership-free DCA-contract replacement was established.

A concrete partial alternative is MDT PRO64 SCN-DA641P.04S's manufacturer web UI,
with documented ETS commissioning dependencies and different-device identity.
It is not the selected DALI64 DCA, a firmware-DCA replacement, or approved scope
change. Public docs plus actual CLR metadata may support a narrow compatibility
investigation; they do not establish permissible sources or executable compatibility.
Task-005 stays Blocked and all original gates remain closed. No SDK implementation,
manufacturer adapter, package installation or hardware action was performed.

Alternatives increment checks: `python3 -B scripts/check_project_scope.py` PASS;
`git diff --check` PASS. The predefined sequential_verifier reproduced those
checks. The predefined sequential_review independently checked the primary-source
capability distinctions and returned PASS with no findings for this bounded
research/task-description increment. TEST-004 PASS for the evidence comparison;
TEST-001 remains INCONCLUSIVE, TEST-002 NOT RUN, and runtime alternatives remain
INCONCLUSIVE. No full task acceptance or product GO follows.

Reviewed alternatives research source commit: `faed67cfb8501dbf8b2f98d7eca34f8777e14336`.

## Selected private-reference route, 2026-10-21

The user selected the first candidate (minimal original-DCA compatibility) and
explicitly authorized extracting selected DLLs from their own ETS installation
through a user-run script, with no binaries in this repository. The independent
[Task-139](Task-139.md) supplies bounded metadata inventory and optional private
copying. This is now an authorized research route; the earlier no-copy statement
was historical. Runtime use rights, complete reference identity/closure and
clean-machine compatibility remain unresolved. No SDK replacement or DCA load is
claimed and this task's required rights/runtime evidence remains unmet.


## Private candidate follow-up, Task-140

The operator supplied two hash-verified candidates from the local ETS 6.4.1
installation. The [static candidate map](../docs/DCA-CONTRACT-MAP.md) observes
that the reference DCA requests SDK/AddInViews `5.7.554.37315`, whereas supplied
assemblies are `6.4.8718.0`. All 27 candidate-directed TypeRef rows have same-name
definitions, but 55 member signatures remain unmatched and 209 TypeSpec member
parents remain unsupported. Additional ETS dependencies are unsupplied. No vendor
code, binding, activation, Windows UI or bus operation was executed. This improves
the dependency evidence without changing Blocked status or resolving usage rights.

## Public terms evidence update, 2026-10-21

The historical DCA and ETS SDK terms evidence report (reviewed source
`e9a8e990abcc093425d493212469eb2a4d9bffd7`) records the official
KNX SDK acquisition requirement and the ETS-only scope of KNX's free/no-license
DCA classification. The reviewed public sources do not provide the SDK package
terms or MDT DCA standalone-use terms. Exact artifact terms and human review are
still required; local reference availability does not establish usage rights.
No permission or prohibition finding was made. Task-005 remains **Blocked**.

Operator direction: retain user-run extraction scripts and keep all manufacturer
binaries outside source control. Do not make further broad licensing research a
prerequisite for independent technical development. Private use and an existing
ETS license were proposed as a possible basis, but no artifact-specific permission
or prohibition is inferred from that assumption. SDK execution rights remain
UNRESOLVED; offline owned tooling and independent Rust work continue. The original
DCA/runtime gate is not advanced by this documentation increment.

Terms-note checks: predefined sequential_verifier independently checked the four
official public sources and reproduced `python3 -B scripts/check_project_scope.py`
and `git diff --check` with PASS. Predefined sequential_review returned PASS with no findings for the two-file
evidence increment, committed as
`e9a8e990abcc093425d493212469eb2a4d9bffd7`. No Task-005 completion is claimed.

## Owner authorization decision update, 2026-10-21

Simon directed the project to assume local use is authorized, keep original
DLLs out of the repository, and assign responsibility to him.
[ADR-005](../docs/ADR-005-owner-authorized-private-dependencies.md) records this
as a binding project decision. This is not a legal opinion or agent
certification. Original DLLs stay outside Git and all build distributions;
selected extraction is private and approved. No legal-document or lawyer-review
prerequisite remains active for this task.

This update supersedes the active legal/provisioning blockers in the earlier
dated report sections while preserving them as historical observations. Task-005
remains **Blocked for technical reasons only**: the selected DCA requests
SDK/AddInViews `5.7.554.37315`, while private candidates are `6.4.8718.0`; the
candidate map records 55 unmatched member signatures, 209 unsupported TypeSpec
member parents, and additional unsupplied dependencies. Clean ETS-free
Windows runtime closure and binding/activation behavior also remain unverified.
Task-003 and Task-006 Windows/Rust checks are PASS and are not missing-tooling
blockers. This documentation decision does not claim host compatibility, DCA
load, configuration round-trip, bus operation, or gate approval.

Owner-decision increment checks on the assigned worktree: `python3
scripts/check_project_scope.py` PASS (exit 0); `python3
scripts/check_reference_inputs.py` FAIL (exit 1 because this worktree does not
contain the Git-ignored private fixture files; the manifest parses, but file
hash validation cannot complete); `git diff --check` PASS (exit 0).
`python3 -m unittest discover -s scripts -p test_reference_inputs.py -v` PASS
(11 synthetic manifest tests, exit 0). Hardware and Windows host execution were
NOT RUN. Task-005 remains Blocked pending technical qualification. This
documentation increment is ready for coordinator review.

Independent verifier follow-up: scope checker, all 11 synthetic reference-input
tests and whitespace check PASS. The revised manifest is schema-valid. The full
worktree CLI check returns exactly five missing private-file errors because the
ignored fixtures are not duplicated into this worktree. The verifier separately
called unchanged `check_reference_inputs.check_file` for every Available file
item from the revised manifest against the coordinator's main private-fixture
root: all five size/SHA-256 checks PASS. No fixture was copied or mocked, and the
SDK technical qualification remains Missing. This is verification of the owner
policy/config increment, not Task-005 host qualification. Critical review PASS for this increment. The reviewer independently reproduced
the manifest checks and all five actual-file hash checks; a wording correction
clarified that original DLLs are excluded from build distributions, while
authorized private build staging remains permitted.

Reviewed owner-decision source commit: `5c5844834914d40007d972ec03daf1164ee073af`.

Policy propagation also updates R-028 and Tasks-014/028 so later gates apply
ADR-005 without reintroducing license-document acquisition. Technical PoC evidence
and explicit human GO remain required; no phase gate is advanced.

Independent review PASS for policy propagation; source commit
`629575fa5fe5211772fd7094451590713dcb8ec4`. No technical or phase gate was advanced.

## Accepted static comparison follow-up, 2026-10-21

Task-142 is Done for the bounded metadata experiment at source
`0b3cfef884a2181e12bf58acf1d08b76312789ed`; its evidence is
`docs/DCA-MEMBER-MAP.md` at that revision. Independent verifier and reviewer PASS,
with 31 member, 9 original metadata and 12 wrapper tests passing and a
byte-identical final guarded private report. Both write/network-denial probes PASS.

The current result supersedes the earlier Task-140 unresolved-count snapshot:
53 of 55 candidate-directed member signatures have a direct structural match
under the documented version-only observation mapping; two remain unresolved
because `PropType` refers to different external `Knx.Ets.Common.Types` versions.
The candidate has same-name direct ReadProperty/WriteProperty definitions; the
comparison does not establish their compatibility. All 209 TypeSpec parent
owner scopes decode and none declares a member in the supplied candidate
assemblies. No CLR binding, SDK runtime or device operation was executed.

Task-005 remains Blocked on technical dependency closure, binding/activation and
ETS-free runtime qualification. Simon's assumed-use decision remains in force;
license-document acquisition is not restored. The next narrow technical step
is to inventory only the explicitly identified missing Common.Types and
Common.Resources dependencies through the private extraction procedure before
choosing any additional comparison or host experiment. Do not copy an ETS tree,
add Falcon or treat every static assembly reference as a required runtime load.

Reviewed static-comparison propagation source: `d79f241f541d8a96a87f8a6a5ccf402df221892e`.

## Selected private dependency acquisition, 2026-10-21

Under the existing extraction authorization and ADR-005, the explorer reused
`scripts/windows/Get-EtsReferenceCandidates.ps1` on the licensed Windows host.
Only exact Common.Types/Common.Resources filenames were selected. The script
inventoried managed identities/hashes, then copied the two files and its manifest
to a new private temporary directory; no target assembly was loaded or executed.
SFTP returned only those three files to the coordinator's ignored
`private/downloads/task005-sdk-dependencies/task005-20261021-01/` directory.
The task-created remote script/staging directory was removed afterward; installed
ETS files were not altered. No recursive ETS copy, driver or Falcon was selected.

Both candidates are version `6.4.8718.0`, neutral culture, public-key token
`61439873ec5e1159`. Exact bytes/hashes and the inventory-to-reference distinction
are recorded in [dependency provisioning](../docs/DCA-PROVISIONING.md).
The private-copy manifest reports complete at `2026-10-21T13:28:58.0512005Z`,
with target execution NOT_RUN; local size/SHA-256 checks match its two entries.
The original DCA archive and supplied SDK ZIP hashes remained unchanged.
The manifest's legacy rights field makes no independent legal finding and does
not reinstate a terms gate under Simon's owner decision.

The Resources candidate matches the observed AddInViews AssemblyRef identity.
Types matches the supplied SDK's AssemblyRef identity, but not the original
DCA's requested `5.7.286.36952`. That requested older dependency remains
unavailable. Input acquisition does not prove ABI, binding, initialization,
service mediation or ETS-free execution. Task-005 remains technically Blocked.

The explorer reported that a separate exact-path check for `ETS6.exe.config`
at the selected installation root returned Missing. No XML/binding policy was read and no other config was
searched. This limited negative result does not establish absence of binding
redirects elsewhere. The owned temporary reader was removed; no permanent
Windows change or target execution occurred.

Independent local verification PASS: both DLL byte hashes and the private
manifest size/hash, exact two entries, status/timestamp and metadata observation
fields agree. All three files are ignored and untracked. The verifier compared
only prior guarded JSON for AssemblyRef identities and performed no new DLL
metadata decoding. Original DCA/SDK archives retained their expected hashes;
scope/whitespace checks PASS. Runtime/hardware remain NOT RUN.

Independent critical review PASS for the private acquisition record: the reviewer
confirmed both local file hashes, the exact two-entry manifest, prior guarded
AssemblyRef comparisons, ignored/untracked status, scope and whitespace. No new
DLL decoding, remote execution or runtime qualification was performed by review.

Reviewed private dependency acquisition record source:
`901a04f64f42848594319e6b4cfc12787a89a986`.


## Selected ETS-free route qualification, 2026-10-22

This current section supersedes earlier technical-blocker snapshots for the
selected startup/constructor route. ADR-005 remains the owner authorization
record; no legal-document gate is restored. Baseline `134ebb4` contains accepted
Task-148 (implementation `1daec2717b16625452eae8020c54e4eaa35385b6`, final report
`134ebb4243c349f7128e655f60274f6465f526b8`). Tasks 003 and 006 are accepted Done;
old historical Blocked wording is not current readiness evidence. Task-004's
reference artifact registration is accepted. No phase gate applies or advances.

Task-148 proves startup and one unchanged original constructor on the selected
Windows machine without installed/running ETS. Only the MDT primary, SDK 6.4
and AddInViews 6.4 loaded from the selected private stage; eight fixed inputs
were pinned, not all necessarily exercised. AC-003 is satisfied for this route.
This does not depend circularly on later Initialize/UI/state qualification in
Tasks 008–012. The actual network-isolation FAIL and the owner's constructor-
only exception remain unchanged; no device operation or broader runtime call
is authorized by this Task-005 compile increment.

### Remaining TEST-002 contract, written before source

Create only `experiments/dca-reference-compile/OwnedReferenceContracts.cs` and
`Test-ReferenceCompilation.ps1`. The source is an owned compile-only interface
whose property types name the actual public IInitializationContext,
IDeviceConfigurationAddIn, IEts4AddIn/V2 and SDK Project/Device types observed
in accepted Task-144's guarded metadata report. It contains no SDK interface
implementation, executable method body, object creation, service, initialization
context, host lifecycle or bus logic. No original assembly or output library
is execution-loaded.

The driver pins the accepted Framework64 C#5 compiler version/hash, the public
Task-148 six-reference net48 profile and exactly two private SDK/AddInViews
6.4.8718.0 assembly hashes/metadata identities. The other five runtime-stage
inputs are not compiler references. It validates local non-reparse paths from
root to leaf, all pins and identities before creating new non-Git output, then
compiles only that fixed source to a private library. The compiler has a real
20-second deadline, confirmed exit/termination and a live 65,536-byte combined
output cap. Owned source and public/private references are held read-only while
used; the compiler executable is hash-checked, not held open by this driver. The private result manifest records source/reference/output hashes,
assembly identities, net48/x64/C#5 target and no-execution outcome.

The coordinator alone runs Windows. Required checks: fresh positive compilation;
missing private reference and mismatched private reference fail before output
creation. The latter uses unmistakably invalid owned bytes, not a substitute
SDK. No extra original reference is added silently if the compiler reports a
missing dependency. Keep original DLLs and all generated outputs outside Git
and distribution. No new dependency, Windows installation/configuration, rights
decision, remote publication or hardware contact is part of this increment.

### Pre-execution completion snapshot

| Field | Observed result |
|---|---|
| Status | In progress; selected ETS-free startup/constructor proof accepted, exact-reference compiler proof pending |
| Active source baseline | `134ebb4243c349f7128e655f60274f6465f526b8`; imported owner-decision revision `2046bd8` preserved |
| AC-001/002 | PASS: ADR-005 owner assumption and private-only original DLL handling |
| AC-003 | PASS for selected startup/constructor route under accepted Task-148; no Initialize/UI/state or mediation claim |
| TEST-001 | Owner record and private boundary present; final independent review of this qualification increment pending |
| TEST-002 | Inventory/static/binding/runtime evidence accepted in Tasks 139–144/148; exact private-reference compilation NOT RUN |
| TEST-003 | Compiler missing/mismatched private-reference negatives NOT RUN; prior binding/version negatives remain accepted |
| TEST-004 | Existing reviewed primary-source alternatives comparison PASS; no alternative replaces original hosting |
| Local checks | PASS: PowerShell parser, C#5 source syntax and whitespace. No private metadata was read and no compilation/library execution was performed locally. Scope check rerun after repairing an imported historical link to an absent document. |
| Windows execution | NOT RUN for new compile tooling; no new vendor execution requested |
| Source commit(s) / final review | Pending after genuine positive/negative evidence and independent review |
| Product/phase gate | Unchanged; no gate approved |

Next action: implement and locally check the two owned files, freeze source, then
coordinator Windows compilation/negative tests and sequential verification/review.

## Observed compile qualification, 2026-10-22

Status: In review. Actual Windows tests now supersede the pending compilation
snapshot above. The selected dependency provisioning task is complete in scope,
subject to independent verification and critical review. Initialize/UI/state
services remain separate Tasks 008–012; no device or gate outcome is inferred.

The coordinator executed the documented PowerShell command on the new ETS-free
Windows 11 Pro x64 VM (build 26200, .NET Release 533509). The exact Framework64
compiler and six public references were the accepted Task-148 pins. Both private
SDK references were version 6.4.8718.0, with the exact identities and hashes
recorded in DCA-PROVISIONING. No original assembly or output was executed.

| Check | Actual result |
|---|---|
| Positive compile, UTC 2026-10-22T17:27:14.9735848Z | PASS: exit 0, confirmed exit, zero compiler output bytes, net48/x64/CSharp5 |
| Missing private reference | PASS: exit 1, `REFERENCE_COMPILE_MISSING_PATH`, output directory absent |
| Modified private reference (three explicitly invalid owned bytes) | PASS: exit 1, `REFERENCE_COMPILE_REFERENCE_HASH_Knx.Ets.Sdk.dll`, output directory absent |
| Output | 3,072-byte owned interface library, SHA-256 `4a6015e5f1ef1d54b71521301846daaab69c88172c053110a1d7ac4a13ae9780`; never executed |
| Original runtime route | Accepted Task-148 startup and one original constructor on ETS-free VM; later services unverified |

Frozen source SHA-256:

- `OwnedReferenceContracts.cs`: `1dc7d7db6082eea2a09b920ed8b06a912c0241aed24c6ffbda0b037387cf5053`.
- `Test-ReferenceCompilation.ps1`: `07d170ba330f040434f3100c2c5775cd61200d33be3ae57cc55befef6b367405`.

Private reproducibility evidence is under `private/evidence/task005-selected-runtime`:
`source-freeze.json`, `positive-stdout.txt`, `results.json`,
`negatives-stdout.txt`, and `evidence-manifest.json`. The first negative harness
attempt stopped when Windows PowerShell converted expected native stderr into
a terminating harness error. Only the private harness capture was corrected;
the frozen source was unchanged. The rerun asserted both specific diagnostics
and absence of output. No success is inferred merely from a nonzero exit.

TEST-001 owner-decision/private-boundary review and TEST-004 alternatives remain
as previously recorded. TEST-002 now PASS for the exact two-reference build and
selected runtime closure; TEST-003 PASS for missing/mismatched references, with
unverified later SDK capabilities still blocked individually. All three ACs
are evidenced for this selected route. Network isolation remains FAIL under the
recorded Task-148 constructor exception; this compile test used no vendor code.
Source commit and final independent verification/review are recorded below
after acceptance. No phase gate, full SDK support or redistribution permission
is asserted.

Current worktree checks: `python3 -B scripts/check_project_scope.py` PASS;
`python3 -B scripts/check_reference_inputs.py --require clean-windows sdk-references-rights`
PASS after preserving the validator's acquisition-owner role vocabulary
(`Project maintainer`; Simon remains the responsibility owner in ADR-005).
`python3 -B -m unittest discover -s scripts -p test_reference_inputs.py` PASS
(11 tests); `git diff --check` PASS. The five existing ignored file fixtures
were materialized as regular hardlinks from verified local inputs, preserving
all byte/hash checks. No validator logic or other inventory disposition changed.

Negative diagnostic retention follow-up: the same frozen-source rejection cases
were rerun in fresh outputs at UTC 2026-10-22T17:29:54.7602061Z to retain the
actual captured error text in each `results.json` diagnostic field. Both remain
PASS with output absent. The positive compilation was not rerun.

## Coordinator acceptance

Done for the selected route. Predefined `constructor_verify` independently
verified the frozen source, compiler/reference pins, evidence manifest and
positive/negative outcome consistency. Predefined `binding_review` returned
PASS with no unresolved findings and reproduced scope, input inventory, all 11
synthetic reference-input tests and whitespace checks. The reviewer explicitly
accepts only selected private-reference compilation and ETS-free startup/
constructor provisioning. Later SDK/UI/state capabilities remain unverified.
All three acceptance criteria are met in that scope. This is agent review and
local coordinator acceptance, not maintainer release or phase-gate approval.
The reviewed implementation and qualification commit is the commit containing
this record; its exact revision is recorded in the subsequent evidence record.
Next ready task: Task-007 neutral shell acceptance.

Reviewed implementation/qualification source commit:
`ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`.

## Owner reaffirmation and retained-evidence audit, 2026-10-23

Simon reaffirmed that he has clarified private-copy permission; original DLLs
are proprietary and must not be redistributed. He also reaffirmed DCA support
for ETS 5.7 and later, including his ETS 6.4. This is recorded in ADR-005;
the selected compatibility result is additionally supported by accepted
Task-143 binding and Task-148 actual constructor evidence. The assembly-version
difference alone is not an incompatibility or a reason to reopen Task-005.

The coordinator corrected a stale-branch status error by reconciling existing
worktrees and continuing from DCA work baseline `d742cab`. The accepted Task-005
record is `ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`, with revision-record commit
`546b126585541f77df0851419bfdbb7ea649970f`. Task-148's accepted runtime record
is `134ebb4243c349f7128e655f60274f6465f526b8`.

Read-only local hash audit PASS: all 11 files in Task-005's private evidence
manifest and all five files in Task-148's original runtime evidence manifest
match their recorded sizes and SHA-256. The Task-148 result and evidence manifest
also match the hashes in the committed public report. No vendor code, Windows
command, hardware operation or new runtime test was executed by this audit.
Task-005 remains Done for the accepted bounded route; later initialization,
UI/state and services retain the results recorded in Tasks 008–012. No phase
gate or redistribution permission follows.

Documentation checks for this reaffirmation: `python3 -B scripts/check_project_scope.py`
and `git diff --check` PASS. The predefined independent reviewer returned PASS,
no findings, for these four documentation paths and their evidence limits.
This does not add a runtime rerun or replace the accepted source revisions above.
