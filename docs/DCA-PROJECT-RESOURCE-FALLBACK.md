# Task-009 bounded Project constructor resource fallback

## Readiness and purpose

Baseline `5401e66` records actual19b FAIL2 with independent result audit and
critical documentation review PASS. Project constructor `06002830` did not
return after one synthetic get_Id request: the existing handler fatally denied
exact Autofac.resources8.2/en-GB outside its Factory-only constructor window.
The underlying Project/service failure is UNKNOWN. Earlier source/owned tests
and unchanged inputs are prerequisites, not usable Project/DCA qualification.
Tasks 003/004/005/006/008 are accepted; no phase gate applies or advances.

This increment prepares a separate diagnostic with explicit modes
`original-project-genuine-factory-resources` and
`owned-project-genuine-factory-resources`. Existing modes and their results
remain unchanged. It does not retry original19b or assume a resource DLL is
needed. Source/owned verification, critical review and source commit precede
any separately frozen original20 plan.

## Format evidence and interpretation limits

Microsoft's [.NET Framework AssemblyResolve reference](https://learn.microsoft.com/en-us/dotnet/api/system.appdomain.assemblyresolve?view=netframework-4.8.1)
and [handler guidance](https://learn.microsoft.com/en-us/dotnet/standard/assembly/resolve-loads)
describe satellite resolution events and returning null for requests the handler
does not resolve. The explicit [.NET Framework resource fallback section](https://learn.microsoft.com/en-us/dotnet/core/extensions/package-and-deploy-resources#net-framework-resource-fallback-process)
describes continued parent-culture fallback after a null result. Only that
Framework section applies to this net48 host, not the separate .NET Core rules.
Primary references were checked on 2026-10-23.

Returning null supplies no assembly, stub, service or successful constructor
result. CLR probing/loading may continue, including before the existing
after-load audit. NULL RequestingAssembly does not prove an Autofac caller.
The diagnostic retains these limitations and the existing fatal dependency
latch; it is not a sandbox, probing-prevention or vendor-behavior guarantee.

## Smallest source change

The four assigned implementation files are ProjectConstructorProbe.cs,
ProbeLauncher.cs, OwnedProjectConstructorFixtures.cs and
Test-ConstructorProbe.ps1 under `experiments/dca-constructor/`. No build-script,
configuration, input-array, dependency, SDK contract or artifact-layout change.
All 19 private/two fixed OS input pins, 18 artifacts, 15-second child deadline
and 64-KiB output cap remain fixed. All Windows actions stay with the coordinator.

For only the new mode family, start one thread-local aggregate budget immediately
before WithRegisteredTrace. Enable resolution eligibility only around each exact
Factory and Project ConstructorInfo.Invoke expression. End eligibility before
return validation, logging, error handling or disposal. The outer budget remains
through the existing managed lifetimes/final audits and is cleared in finally.
The two constructor windows share at most eight eligible probes; the ninth
refuses and saturates. Do not reset the counter between Factory and Project.
Preserve WithFactoryResourcePhase and legacy mode/reset behavior unchanged.

Require the exact original mode, complete held profile, pinned actual Autofac
owner/path/identity, same-thread active constructor phase, and only the existing
full en-GB/en Autofac.resources8.2 identities. A non-null requester must be the
same owner; a null requester remains explicitly NULL_UNKNOWN. Return only null
from the existing resolver branch; no recursive Load, replacement assembly,
satellite copy or automatic dependency fetching is added.

All unapproved identities/cultures/versions, owners, profiles, modes, threads or
phases remain fatal. Constructor windows are nonnested. Budget/window flags do
not reset sticky dependency, provider or store-denial latches. Final audits
still run after Project, container and trace managed cleanup, including caught
denials. No Project Dispose is invented for an unreturned constructor object.

The synthetic get_Id proxy runs synchronously inside Project Invoke. Interpret
the getter exclusion literally: suspend the new-mode window throughout the
owned ProjectStoreIdProxy.Invoke callback and restore it in finally, preserving
the aggregate count and all denial latches. Apply the same suspension throughout
ProjectDenyProxy.Invoke, so no owned service callback inherits a constructor
window. Nested callbacks fail explicitly; prior modes remain unchanged.
Fixture ID construction/numeric
getter, builder/registration/Build, Project/container/trace disposal, errors and
DCA/UI/state work receive no resolution window. Old modes remain unchanged.

## Required owned tests and completion

Prove shared capacity across both windows, exact eighth/ninth boundaries and
saturation; cleanup on return/exception; nesting/off-thread refusal; strict
identity/profile/owner/requester predicates; getter suspension/restoration;
and denial outside each constructor, during validation/logging and every
cleanup scope. Caught refusals must fail final audit. Use fresh child processes
for sticky-latch cases, never production resets. Owned fixtures cannot
impersonate pinned Autofac or count as actual CLR/vendor fallback evidence.

Required checks: existing C#5/x64/warnings-as-errors three-target compile against
six pinned net48 references; PowerShell parser/source guards and preservation;
then fresh pinned Windows build and complete owned Session0, limited-interactive
and administrator-negative suites, independent verification and critical review.
Retain all earlier failure/source snapshots. No original20 outcome is asserted
until its separate reviewed committed-source plan is genuinely executed once.

Status: source20 implementation is frozen with local checks PASS. Independent
source verification/review and Windows20 owned/original execution remain pending.

On resumption, the local SSH master control check returned running, but the first
actual read-only Windows readiness request failed exit255 with a broken multiplex
pipe followed by BatchMode authentication denial. No Windows20 build/test or
vendor operation started. The owner has the existing connection command; local
work continues independently. Private `owned20-resume-host-readiness-stderr.txt`
retains the actual transport failure; this is not an SDK/runtime result.

Task-009 remains In progress, all ACs unchecked, qualification NOT_ESTABLISHED.
No DCA initialization/UI/state, intentional device operation, gate, isolation,
network or descendant containment is qualified. The existing owner diagnostic
exception and private-only proprietary DLL boundary remain in force.


## Source20 local evidence checkpoint

The uncommitted implementation on baseline `cf0b89b` is frozen by private
`source-freeze-20.json` (9,435 bytes, SHA-256
`068e73d6292d65f22b8d3302798fdf363020803823e47f973670df1a7188f082`).
It records eight runtime/ten source hashes; `source20/evidence-manifest.json`
(SHA-256 `b2c50e1443db61f40a37f05708f54109ce1a9b7a92b6af4f75b37ffdf7b1833c`)
binds 55 local artifacts, including ten frozen source snapshots. The freeze-time
contract bytes are retained separately as `source20-contract-at-freeze.md`;
subsequent result entries do not retroactively change that contract hash.

Actual local commands are in `source20/commands.json`: provisioned PowerShell
7.6.7 runs `source20/local-sourcecheck.ps1` with the task worktree and the six
pinned private net48 references. The third log pair records PASS for all three
C#5/x64/warnings-as-errors compilations, both PowerShell parses and old/new source
guards. An initial ambiguous Thread delegate compile FAIL and its correction
remain in earlier logs. `source20/preservation.py` records PASS for all 13 input
arrays, 13 preserved methods, both unchanged proxy cores, six other frozen
sources and other fixture families. Coordinator `git diff --check` and
`python3 scripts/check_project_scope.py` also PASS.

Five fresh processes run `source20/pure-resource-proof.ps1` for normal,
nested-window, nested-budget, off-thread and outside-budget cases, PASS in
third log pairs. This proof extracts exact new method bodies and binds their
hashes/full source, using owned adapters under .NET10.0.13 on macOS. It does
not exercise Framework CLR resolution, RealProxy, Windows or vendor code.
Windows owned suites include a fresh factory refusal case that invokes the
actual owned deny proxy and observes window suspension/restoration; this
Windows case is written but NOT RUN.

Prepared Windows20 helpers are private, separately named and NOT EXECUTED.
Static audits are in `verifier20/orchestration-audit.txt` and
`verifier20/orchestration-refinement-audit.txt`. The refined helpers refuse an
existing source/build destination, protect task/decoded evidence paths and
remove dynamic legacy collector imports. Initial helper bytes, manifests and
the corrected preparation-only AST insertion failure are retained. Future
source/build/stage and sibling-output freshness require live preflight; these
audits prove no Windows behavior.

No original20 plan or execution is implied by this source freeze. The next
steps are independent source verification/review, renewed Windows access and
the complete owned suite before a separately committed-source original plan.
Task-009 remains In progress, all ACs unchecked and qualification NOT_ESTABLISHED.


## Source20 review finding and follow-up

Independent local-source verification PASS is recorded in private
`verifier20/source-audit.txt`, SHA-256
`f886a7880bb998cae177f30834794769914f38af6670323067a5f44ac82ac7e4`.
It recompiled all three targets and reran all five extracted-policy cases;
preservation was audited without rerunning its fixed-output writer.

Critical source/local review `review20/critical-source-local.txt`, SHA-256
`4d55e363d9277fae7fbc64f7a355f234f0af9365dfbd25628acea2e0518a6468`,
returned CHANGES_REQUESTED for one P2 coverage gap: the existing nested-callback
refusal had no direct regression. No policy bypass was demonstrated. Source20
freeze/evidence remain unchanged; source20a adds a fresh nested-callback case
and an owned one-shot same-proxy reentry observation. It must assert exact named
refusal, unchanged aggregate count, outer suspension/restoration and sticky final
audit failure. Local proof includes a removed-guard mutation which must fail.
Source20a checks/freeze/review and Windows execution are pending at this entry.

Windows access was subsequently restored: a new master and the actual read-only
`owned20-resume-host-readiness02` request returned PASS, 64-bit, ETS0 and owned
processes0. `owned20a-stage-freshness01` proves the existing private stage and
five absent new paths (source, build and three owned output directories). Its
console count of six was a preparation-message error; the five-key result JSON
is authoritative. No build, owned runtime or original call was made by either
readiness check. Refined new 20a helpers remain PREPARED_NOT_EXECUTED.


## Source20a regression and actual owned Windows results

Source20a is frozen by `source-freeze-20a.json` (9,966 bytes, SHA-256
`48bd6125064df399b82d85db98ae168e4fbe815f651754f347f9212367285ea2`).
The freeze-time contract is preserved as `source20a-contract-at-freeze.md`.
Only the main probe, launcher and test driver changed from20; the production
nested-callback guard and fixture source remain unchanged. Nineteen fixed
owned selector cases include one bounded same-store-proxy reentry.

Local compile/parse/guards, preservation and six fresh extracted-policy cases
PASS. Removing the exact guard only in memory produces the expected exit1
`PROOF_NESTED_CALLBACK_STATE`; the unmodified proof passes. All58 prior source20
files remain byte-identical. Exact commands are in `source20a/commands.json`.
Independent verification `verifier20a/source-audit.txt`, SHA-256
`2e6c0c7f1e77e78cb1e431fc403dab265f3e08fef7eeb02988eaa7e3dab280a0`,
reproduced those checks. Focused critical source/local review
`review20a/critical-source-local.txt`, SHA-256
`d9f3af56edf043ebd65ffc822ddbf54f0be45209737e19ba07c288c804af4b88`,
returns PASS and closes the P2 gap locally; it does not certify Windows results.

Actual coordinator Windows commands, each run once against this freeze:

- `python3 -B private/evidence/task009-constructor-windows/build_owned20a.py`: PASS, pinned Windows compiler, eight compiled sources and18 artifacts.
- `python3 -B private/evidence/task009-constructor-windows/run_session0_20a.py`: PASS79 Session0 cases.
- `python3 -B private/evidence/task009-constructor-windows/run_owned20a.py`, then terminal poll, `collect_owned20a.py` and `cleanup_owned20a.py`: PASS137 limited Session2 cases, exit0.
- `python3 -B private/evidence/task009-constructor-windows/run_owned_admin20a.py`, then terminal poll, `collect_owned_admin20a.py` and `cleanup_owned_admin20a.py`: PASS84 administrator-negative Session2 cases, exit0.

Each command was executed through an owned Python capture wrapper retaining a
separate console file; this does not imply an uncaptured direct invocation.
The limited suite genuinely exercised the owned RealProxy reentry once. It
recorded named refusal while outer window0/suspended, unchanged aggregate count,
then outer Project window2 restored; final dependency audit failed as required,
with expected child2 confirmed by the driver. This proves the owned fixture
path, not vendor behavior or actual Autofac/CLR resource fallback.

Both scheduled jobs were terminal before removal, last result0, zero owned
processes and no junction. All330 limited and171 administrator case files are
retained. `record_owned20a.py` ran against saved files only: PASS, producing
`owned20a-observation.json` (123,419 bytes, SHA-256
`c8e648b2e9be92bfc3fd7376a20b1c03be6a07a93ff5acf3fed5f064ea6912f4`)
with628 evidence bindings and the exact source/build/profile/cleanup checks.
Independent Windows-result verification and critical evidence review are pending
at this checkpoint; source commit is still pending.

Original20a remains NOT RUN and has no fixed execution plan yet. Actual19b
FAIL2 remains the latest original Project result, with underlying service failure
UNKNOWN. DCA initialization/UI/state, actual resource fallback, manufacturer
behavior, network and descendant containment are not qualified. Task-009 stays
In progress, all ACs unchecked, no gate advances.


### Source20a subsequent independent result acceptance for preparation

Independent saved Windows-result audit `verifier20a/owned-result-audit.txt`
(SHA-256 `d357a3d3e59917cac5890e2be7d41219262eb1f08c0c8250bbcf0b798ba08bd3`)
PASS: all628 contained file bindings, current10 source hashes, build-source and
runner pins, actual79/137/84 counts, roles, raw/decoded logs and501 cleanup files.
Owned artifact payloads are not present locally for independent byte rehash;
the saved executed Windows preflight reports18 size/hash checks PASS and the
review cross-checks every artifact declaration/pin in the fixed runners.

Critical owned-results/final-six-path review
`review20a/critical-owned-results.txt` (SHA-256
`c3a41d135c521cbbd8c9d96119f6d2ae0681c56f585cf314a95de5ede26321f1`)
PASS, no unresolved findings. The coordinator also checked the actual source20a
regression diff. The six preparation paths are ready for a local source commit.
This is evidence acceptance for this preparation increment, not maintainer Task-009
acceptance, original SDK qualification, a gate or an operation approval. A new
original20a committed-source plan and its separate review/preflight remain next.


### Reviewed implementation source commit

Implementation commit `8e640e697e223b71ed4864c0546cd83e951254ad` contains the six reviewed
preparation paths. Private `source-commit20a.json` records the post-owned-execution,
pre-original-execution audit: every one of ten `git show COMMIT:experiments/PATH`
blobs and current source files matches freeze20a, PASS. It is not retroactive
pre-owned-run commit provenance. The freeze correctly retains its uncommitted
source checkpoint, while the original plan must pin a committed revision.
Original20a is still NOT RUN; no original execution plan or retry was created
by this source record.


## Actual20a original result

The separate fixed original20a plan executed once at committed source
`eb3b8cfe23950f1bd94e4bdc0f9ca54f647fc066` (implementation `8e640e6`).
Actual **FAIL exit10**, direct child exit confirmed normally by the launcher.
The 15-second/65,536-byte bounds were retained; the diagnostic log is18,848 bytes.
The limited interactive context is Session2, administrator-role false.

Trace/registration/container/Factory returned. Project constructor06002830
attempted once and requested the single labeled synthetic get_Id fixture.
Four exact Autofac.resources probes were allowed in Project window2, aggregate
1..4: two en-GB and two en. Each handler result supplied only null, no assembly
or service. Requester remained NULL_UNKNOWN and owner proof NOT_ESTABLISHED;
these events do not identify the requesting vendor caller or prove universal
resource fallback. No denied-resource retry or extra DLL was added.

The observed failure is now explicit for this attempt:
`Autofac.Core.Registration.ComponentNotRegisteredException` reports
`Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider` unregistered during
Project construction. The same exception is logged twice by nested error
reporting, not two constructor attempts. No interface behavior, implementation
or successful service registration follows from that name. Earlier actual19b
FAIL2 retains its own UNKNOWN underlying failure; it is not retroactively
reclassified. Remaining service requirements are UNKNOWN.

Project did not return; body entry remains NOT_ESTABLISHED because it was not
instrumented, and no Project Dispose was attempted. Container and trace Dispose
each returned once. Trace lifetime audit explicitly PASS; final audit points
were reached after both lifetimes with provider-denied0/store-denied0/fixture1.
These observations do not establish Project or SDK-context qualification.
The aggregate budget ended inactive with4 probes, maximum8.

Cleanup PASS: exact task removed, last result10 matches done10, zero owned
processes and scratch files. Post-run all19 private input hashes/24,278,056 bytes
and both existing OS pins remain unchanged, PASS. No new original call was made
by collection, cleanup or the post-input audit. Harness DCA initialization, UI,
state and intentional device operations remain NOT RUN; vendor internals remain
UNKNOWN and network/descendants NOT_CONTAINED.

### Fixed plan, helper findings and execution evidence

Private plan `project20a-plan.json` (13,239 bytes, SHA-256
`c1749fc43637468a0b8eca55f6e02f27ee3e1b2a0d0e82233ad0c446715cfcdd`)
and runner (13,822 bytes, SHA-256
`7e0a0e746c18dabdd298979bc18354f22c6bd0944cfe8b1c872e66f84e649b87`)
bind10 source hashes,19 private inputs,2 fixed OS pins and18 build20a artifacts.
All29 derivation steps from the retained19b runner reverse byte-exactly; a new
seven-path refusal precedes catch/finally so stale evidence is never overwritten.

Initial critical plan review CHANGES_REQUESTED identified two P2 helper issues:
a relative input-stage path and absent plan checksum in collector/cleanup.
Their old versions remain unchanged. Active preflightv3 joins the fixed stage;
collector/cleanupv2 verify the immutable plan before adapter import. Two owned
invalid-plan tests with a trap adapter PASS; executable harness, fixture hashes
and raw transcripts are retained under `plan-pin-negative-proof20a/`. No real
remote adapter was loaded by those negative tests.

Independent plan/refinement audits PASS (`verifier20a/plan-audit.txt` and
`plan-refinement-audit.txt`). Final critical `review20a/plan-review-v2.txt` PASS,
SHA-256 `6eb7629c27d8c80b8672d932e700b924e2652b88fe5c69bf0bad60e6415e3fe4`,
closes both findings. This is development evidence review, not human device
operation approval. Initial rejected review and every helper version are retained.

Actual execution commands used owned capture wrappers (all once):

- The reviewed `project20a-live-preflight-v3.ps1` was transferred by hash and invoked through a short SSH command: actual PASS19private+2OS+18artifacts, x64, ETS0/owned0 and fresh paths/task.
- `run_project20a_v2.py` with the exact saved final review digest: the one fixed original task started. `project20a-poll01` recorded terminal Ready/10/done.
- `collect_project20a_v2.py` and `cleanup_project20a_v2.py`: actual evidence collected and exact terminal task removed.
- Reviewed `project20a-post-inputs.ps1`: actual post-input/cleanup PASS, no new original call.
- `python3 -B private/evidence/task009-constructor-windows/record_project20a.py`: saved-file/source audit PASS, producing `project20a-observation.json` (13,945 bytes, SHA-256 `72a128da69fff87b14b5d2754003b47d9d11f9c82530d24ccb5bd838467ae0aa`) with73 evidence bindings.

`project20a-execution-bindings.json` pins the pre-execution helpers/review and
clean committed source; immutable plan fields still describe their preparation
checkpoint. Runtime results are in the separate observation, not rewritten into
the plan. Source10 current/committed hashes still match. Independent original
result audit and critical result review are pending at this entry.

Next unblocked work is a bounded investigation of existing public metadata for
the observed UndoManager store-provider requirement before defining any new
shim. No speculative registration, success/null stub or automatic original retry.
Task-009 remains In progress, all ACs unchecked, qualification NOT_ESTABLISHED;
no gate advances.


### Actual20a independent result review

Independent original-result audit `verifier20a/original-result-audit.txt`
(4,566 bytes, SHA-256 `754363ec851d66d1cd397020a95ba92339aa821d76abc3877d938ebe0544e24f`)
PASS for provenance/result consistency: all73 bindings,10 current/committed
sources, source/plan/review pins, event counts/order, actual preflight, terminal
FAIL10, cleanup and post-input integrity match. No SDK/runtime rerun or binary
read was used by this audit.

Critical result/documentation review `review20a/critical-original-result.txt`,
SHA-256 `75cae1c8dfbe765cab41594328733ede03196610bf7026b522486ccbceda680c`,
PASS, no unresolved findings. It confirms the constructor failure and unknown
provider contract remain distinct from cleanup/hash PASS; earlier19b evidence
and its UNKNOWN underlying failure are preserved. Coordinator scope/whitespace
checks PASS. This accepts the result documentation, not Task-009, vendor
qualification, an operation or a gate. Next is the public-contract inquiry.
