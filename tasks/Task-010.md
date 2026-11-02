# Task-010: Initialize and display the unchanged original MDT DCA

## Subject

Initialize and display the unchanged original MDT DCA

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-010 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host (C#/.NET) |
| Owner | Primary coordinator and DCA compatibility agent |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-007](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-023](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Demonstrate that the actual original DCA UI can run inside the minimum independent host.

The current SDK context/device probe boundaries are recorded in
[DCA-SDK-PROJECT-WRAPPER.md](../docs/DCA-SDK-PROJECT-WRAPPER.md).

## Detailed Requirements

- DR-001: Load the exact hash-qualified assembly through the approved dependency route.
- DR-002: Invoke the real discovered DCA entry point; do not substitute a recreated UI or screenshot.
- DR-003: Record lifecycle callbacks and complete startup errors.
- DR-004: Run on Windows with no installed/running ETS and no active network access to an ETS service.
- DR-005: Leave all bus operations denied.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The original vendor window/control renders and remains responsive to a benign UI action.
- [ ] AC-002: A recording/log identifies the original package hash and clean-machine environment.
- [ ] AC-003: Assembly loading alone is not reported as UI success.

## Dependencies to Other Tasks

- [Task-009](Task-009.md): Implement only the required offline SDK compatibility surface. Required predecessor behavior/evidence must be accepted.
- [Task-011](Task-011.md): Supply the coherent device/product context required by the real GetDcaUi path before the visible UI test. Initialization-only work remains covered by accepted Task009. See [ADR-011](../docs/ADR-011-dca-context-before-ui.md) for the corrected order.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Windows manual/integration: start, interact with a non-writing control, close.
- TEST-002: Clean-machine audit: verify absence of ETS executables/services/dependencies.
- TEST-003: Negative: unsupported/missing runtime dependency yields a named failure, not a replacement UI.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host (C#/.NET)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No configuration persistence, DALI commissioning, firmware update, or general DCA compatibility claim.

## Risks and Blockers

The original DCA initialized and rendered on the Windows dev host without an
installed ETS; Simon reported that UI interaction and dialogs worked. The selected
application identity is not yet qualified under Task011. Source89 completed the
original UI lifecycle with child exit 0. One earlier interactive attempt reported
a failed device connection. Network containment and absence of direct vendor network paths remain unproven. No live
device connection or hardware test is claimed. See the latest checkpoint below.

## Completion Record

| Field | Current value |
|---|---|
| Implementation / report status | In progress; Source89 original UI, clean process exit, owned regression and admission negatives passed; remaining acceptance evidence pending |
| Source commit(s) | 09cd05bf19636bb872f0c11ae24798a32d3f7c68 |
| Tests executed | Source89 original UI and clean exit PASS; owner interaction observed; 423 owned cases and two original-profile admission negatives PASS; selected ETS host inventory negative; network path and late resolver not established |
| Evidence location | private/evidence/task010-sdk-wrapper/iteration01/run-56aa362ec007; run-60e2151ed943; missing-original89-a15c9e8376d2 |
| Acceptance criteria met | AC-001 has positive render and manual-interaction evidence; full task acceptance pending |
| Technical outcome | Original DCA control rendered and clean teardown observed in Source89; product identity is not established |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent agent review of Source88 and Source89 fixes completed; maintainer acceptance pending |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).


## SDK wrapper experiment checkpoint — 2026-10-26

The first generic SDK-root implementation uses the verified original optional
defaults, distinct controlled IdMappingCache reference, and same preexisting
deny-only Kim provider reference. Windows net48/CSharp5/x64 build PASS. Initial
owned smoke exposed an incorrect negative-test exception catch (IOException vs
InvalidDataException); it was corrected and retained as a failed checkpoint.
The new fresh owned smoke PASS after that correction.

Original SDK run `7ce94a6f3182` on the independent dev host created the genuine
SDK Root successfully; IdMapping reference and Kim reference resolution each
completed once. Project enumeration then refused with SDK_COLLECTION_EMPTY: the
separately constructed ObjectModel Project is not yet a collection member. The
run is FAIL2 for the full wrapper route, not a working initialization/UI claim.
All managed cleanup completed;32 private plus2 OS pins remained unchanged
(28,770,936 input bytes), task removed, owned processes0, scratch0. Runtime host
preflight observed Session2 RDP Active and no selected ETS process. Network and
descendant containment remain NOT_CONTAINED.

Private raw evidence: `private/evidence/task010-sdk-wrapper/iteration01/` with
source-audit01/02 and run-b904e7ac1263 (owned failure, original NOT_RUN),
run-7ce94a6f3182 (owned PASS, original wrapper FAIL_EMPTY_COLLECTION). The next
step is the genuine typed collection-attachment operation for this explicit
offline fixture. No fabricated SDKProject, device or collection entry is supplied.
Task010 remains In progress; Initialize/UI/hardware NOT_RUN.


### SDK context and root follow-up checkpoints

Source03 attached the exact synthetic ObjectModel project using public
DomCollection.Add060000B5 and verified reference membership/count1. Source04/05
added bounded exception-chain/TargetSite diagnostics before cleanup could replace
the original failure. The exact null failure was the SDK project filter
IsValidBirdyObject06001448 -> Root.IsValidContextProject0600027D dereferencing the
declared-default null execution context. No name field was involved.

Source06 supplied genuine AddinExecutionContext06000015, the actual existing
project, and an explicitly owned nonempty offline fixture scope (not a DCA/device
application identity). Its first actual run identified HawkNET as the required
assembly for the internal IHostContext.StartDownload LoadDeviceOptions signature.
No download was requested or implemented. Static inspection found a public Int32
enum without methods/initializer and a module initializer calling a one-byte ret
helper; no driver activation in that inspected initialization chain.

Source07 therefore admits exactly the privately provisioned HawkNET file only for
the new SDK mode, retaining the accepted Guid profile unchanged at32 inputs.
No Falcon or other declared transitive dependency is admitted. New SDK profile:
33 files/33,857,608 bytes, exact hashes and identity checks before load and after
execution. Proprietary bytes remain private, excluded from GitHub publication.

Actual run946cb03e7d21: build PASS, focused owned smoke PASS0; genuine execution
context/reference filter, SDK Root and UndoManager succeeded. SDK Project
construction then reached the parent deny reference's GetType and failed named.
The original ObjectModel root lookup compares exact Root type; this proves that
the earlier interface parent is insufficient for this step. Full wrapper result
FAIL2, with normal cleanup and unchanged33+2 input pins/processes0/scratch0.

The next Source08 work supplies genuine ObjectModel.Root0600053E and its canonical
Projects collection, a controlled initially empty IDataStore, and an owned adapter
implementing the original IDispatcher contract using actual WPF Dispatcher work.
This host adapter is not an invented SDK implementation class or a replacement
manufacturer UI. Unsupported methods remain explicit failures. It is not a claim
that the original DCA Initialize/UI, full bus mediation or configuration state
round trips have already succeeded.

## SDK wrapper success and iteration policy — 2026-10-26

Source08-10 supplied the genuine ObjectModel Root and real WPF dispatcher adapter,
corrected the exact Common assembly identity, and scoped neutral Autofac resource
fallback to the original Root constructor. The next actual failure identified an
unregistered importer delegate. Source11 registers the same existing deny-only
delegate reference; it does not implement or invoke an importer.

Run `79b142e06c8a` on the independent dev host: Windows build PASS, focused owned
smoke PASS0, original SDK wrapper PASS0. The original ObjectModel Root, canonical
collection, project membership, execution context, SDK Root and exact SDK Project
wrapper all returned successfully. Managed cleanup completed, all33 private plus
2 OS pins remained unchanged, owned processes0 and scratch files0. Evidence:
`private/evidence/task010-sdk-wrapper/iteration01/run-79b142e06c8a/`.
The source is still uncommitted; this is an intermediate result, not task acceptance.
Original DCA Initialize/UI and hardware remain NOT RUN.

The owner explicitly requested faster direct dev-host iteration. Subsequent small
changes use compilation and a focused original Windows observation; comprehensive
internal regressions and independent review are deferred until the DCA flow runs,
before final acceptance. Integrity, cleanup and denial of bus operations remain
active. The next increment supplies the genuine SDK Project to a separate positive
initialization profile while preserving the existing denial baseline.

Source12 direct original run `92c5438abd17`: Windows build PASS; unchanged MDT
constructor0600000E returned, Initialize0600001A began and the positive context
committed two genuine SDK Project replies. No later request, refusal or Initialize
return was recorded before the supervised15-second timeout124. The timeout is
not a passing initialization or proof of its cause. All33+2 pins remained unchanged,
test processes0 and scratch files0 after termination. Evidence is in the sibling
`run-92c5438abd17/` directory. A bounded primary inquiry found an original modal
MessageBox error-reporting path; the next diagnostic must identify the actual
exception before changing service semantics or increasing any timeout.

Source13 run `18ca76bd405f` added bounded, owner-thread first-chance diagnostics
only around Initialize. Actual first exception after the two Project replies:
System.ArgumentNullException, parameter `key`, System.ThrowHelper0600033C.
Private primary metadata `mdt-initialize-03.json` identifies the immediate
DcaContext.Add sequence as Project.Name followed by Dictionary.TryGetValue;
`sdk-project-name-01.json` identifies the genuine ObjectModel-backed getter.
The original modal catch remained unchanged, followed by timeout124. Pins33+2
and cleanup PASS. This supports adding an explicit transient test-project name
through its genuine public setter, not bypassing the dictionary or vendor catch.

Source14 run `a324d7e96b10`: Windows build PASS; the genuine public name setter
reached IObjectContext.get_WriteContext06000266, which the current adapter refused
explicitly. Original initialization was not reached in this run; exit2, pins33+2
and cleanup PASS. The next change must provide a verified genuine write-context
or supported original initialization scope; it must not bypass the setter or
manufacture a successful lock response. Session2 was disconnected at this run's
preflight, so visible UI interaction will need a newly confirmed active session.

Sources15-17 implement an initialize-only controlled in-memory IWriteContext:
Wait uses a real owned synchronization event; SetDirty tracks only the retained
genuine project. No persistent configuration or general write service is claimed.
Run `04b25a97c3de` found eligibility evaluated after callback suspension; Source16
captures it before suspension. Run `4fbd9c372ae0` then returned successfully from
Wait and reached ObjectModel satellite resource lookup. Source17 permits only the
exact en-GB/en neutral fallback during the direct original Name setter, retaining
the shared8-probe bound and explicit NULL_UNKNOWN provenance where applicable.
Run `c7d920af56e3` passed that fallback and two Wait calls, then refused
IObjectContext.get_IdCreator06000259. Each run exited2 with genuine failure logs,
unchanged33+2 pins and processes0/scratch0. Initialization has not advanced beyond
the prior Source13 observation yet. The next increment evaluates the verified
original public UndoManager.OnDisabled(Action) scope for initial fixture setup,
preserving the genuine name validation/setter and dirty-state behavior.

Source18 run `85e0a62bd794` failed its pre-invocation API guard because the
IsEnabled getter had been assumed public. Primary metadata establishes its exact
assembly-internal visibility; Source19 corrects only that lookup/validation.
Run `9a50cd47b6ef` then passed genuine Name setter/readback, real Wait/SetDirty,
and the original OnDisabled scope with restored state. The wrapper and MDT
constructor returned, and Initialize made three genuine Project requests before
requesting UserConfiguration06000032. That unsupported getter was logged and the
unchanged original modal catch led to timeout124. Pins33+2 and cleanup PASS.
Existing primary `mdt-initialize-02.json` shows DcaAppMain getter06000012 checks
the forwarded document for null and calls original CreateDoc06000013 when absent.
The next increment supplies explicitly controlled absent initial user state, so
the unchanged vendor default executes; no arbitrary XML or persistence is claimed.

Source20 run `b8e24d65a0f9`: Windows build PASS. Original MDT Initialize returned
normally after three genuine Project replies and one controlled absent-state
UserConfiguration reply. Context audits reported zero denials and no fault through
POST_DISPOSE and AFTER_ALL_MANAGED_CLEANUP. The final diagnostic log then reached
PROJECT_LOG_LIMIT, so the complete run is FAIL2, not PASS. Input33+2 integrity,
processes0 and scratch0 PASS. The next change reduces repeated healthy diagnostic
lines without removing validation or increasing the log cap. The owner reports
RDP reconnected for the upcoming visible UI test; verify actual session state at
the next preflight. UI entrypoints and hardware remain NOT RUN.

Source21 run `7c91d52e4fa0`: Windows build PASS, complete original Initialize
PASS0, all managed/process-final audits healthy,33+2 pins unchanged, processes0
and scratch0. Only repeated healthy initialize-mode diagnostic lines were reduced;
all phase validation and the log cap remain. Preflight confirmed Session2 Active.
Evidence: `private/evidence/task010-sdk-wrapper/iteration01/run-7c91d52e4fa0/`;
source audit `source-audit21.json` SHA256
`77e90422a7c139cd3fbad28013b71c8d25b1c089e0d08c03175853712af03641`.
This proves finite offline original initialization, not UI or state persistence.

Private exact primary UI report `task010-sdk-route/mdt-ui-01.json`, SHA256
`36e96d7545f3d58b2faccc136ecd9d16c2bc5499b338e2d2bc3cc174c777fbd7`,
shows GetAddInUI06000017 returns null. GetDcaUi0600001E returns null for an empty
device collection and accepts only exactly one genuine SDK Device on the actual
MainDialog/DaliDcaVM path. No UI entrypoint has been invoked. The next investigation
must establish legitimate device/application data and the genuine construction
route; a fabricated Device or replacement settings window cannot satisfy AC-001.

The hash-verified private V40a KNXPROD contains actual SCN-DA641.04 product
`M-0083_H-193-1-O00EF_P-SCN.2DDA641.2E04`, Hardware2Program
`M-0083_H-193-1-O00EF_HP-0155-40-4D9F-O00EF` with MediumTypes MT-0, and application
`M-0083_A-0155-40-4D9F-O00EF` (number341, version64, mask MV-07B0). This is the
selected offline candidate, not a physical bench identity. The DCA manifest's
0155/40 declaration has a different full ID suffix; compatibility is unverified
and the full actual product IDs must be preserved. MinEtsVersion5.0 is a declared
application attribute. Missing secure-named fields do not prove non-Secure
qualification. No bus operation is enabled by this metadata inquiry.

Reproduced bounded static inquiry command:
`python3 -B private/evidence/task010-configuration-inputs/inspect_product.py`.
Archive hash/size, paths, entry count, expanded size, compression ratio and XML
DTD/entity rejection were checked before in-memory parsing; no extraction/SDK
execution. Script SHA256
`804c29388603c10ffd8728e8cd05bb3998ab743b08a92dc56c87b16f529e0938`;
private `product-static.json` SHA256
`ec8de0617faa31323bea7b8a7367d3c8f681c5753b9dae66103180039441228c`.
The first script invocation used an incorrect fixture-root path and failed before
reading the archive; the corrected command above completed successfully.

Source22 adds a separate `original-sdk-device-fixture` mode, preserving the
successful initialization mode. Exact original ID constructors parse and roundtrip
the actual product/Hardware2Program IDs without remapping; a required typed device
plugin delegate is registered as deny-only. No Product/application/parameter model
is fabricated and GetDcaUi is not called.
Run `e6a784bd12c9`: Windows build PASS and original DCA Initialize PASS; subsequent
installation/device setup failed with NullReferenceException at Object.GetType
after the installation begin marker. Whole run FAIL2,33+2 pins unchanged and
processes0/scratch0. The next narrow inquiry checks canonical project collection
initialization and adds named null guards before attachment; no field injection
or fake collection is permitted. RDP Session2 was Active at this preflight.

Source23 identifies the original protected AfterBindAction0600283B, whose bound
typed delegate targets the genuine project's private collection initializer28EA.
Run `31de8ab54c70`: build and original Initialize PASS, but the new precondition
rejected an already initialized Manufacturers collection before invoking that
delegate. Whole run FAIL2,33+2 pins and cleanup PASS. The next correction must
respect the actual original collection lifecycle rather than assume every child
collection is null. No original UI entrypoint has been invoked.

Source24 run `20cd67fa9b83`: build PASS; normal original after-bind setup now runs
before SDK consumers and verifies seven direct canonical collections. Existing
genuine empty constructor-prime collections are accepted, while the original
IsOpen guards on unrelated getters remain untouched. Name setup and MDT Initialize
PASS; original Installation factory returns an exact genuine object. Attachment
then fails with ArgumentNullException (`key`). Cleanup separately requests the
currently unsupported IWriteContext.IamDirty0600037A. Whole run FAIL2,33+2 pins
and processes0/scratch0 PASS. The next increment diagnoses the actual attachment
key without guessing Name and supplies only a verified tracked dirty-state query.

The bounded primary follow-up establishes that DomCollection.Add060000B5 keys
objects by IDomObject.get_Id060001D4, not Name. The verified original public
ProjectDomObject.Bind060029DD(targetCollection, context, optional true) creates
the real ID, binds the context and invokes the normal after-bind action. This is
the selected next correction for both new Installation and Device; a genuine
UniqueIdCreator and exact original consistency checkers must supply required
services. No fabricated ID, raw field write or IsOpen bypass is permitted.

Source25 run `62adf4b9c580`: build PASS; public Bind generated the Installation
ID through the genuine UniqueIdCreator, ran the original binding hook and attached
the exact instance with count1. Device creation then failed on an Autofac en-GB
satellite lookup outside the existing fallback scope. Cleanup answered tracked
IamDirty but refused get_IsLockedForWriting0600037E. The whole process exited
3221225477 (0xC0000005); its cause is unexplained, so managed cleanup is not PASS.
Post-run33+2 input integrity, processes0 and scratch0 PASS. A read-only Application
event-log query for IDs1000/1001/1026 in the exact minute returned no events.
The next increment limits neutral fallback to the actual device factory call and
supplies only the lock query derived from the controlled synchronization state.

Source26 corrects the existing plugin delegate registration to the exact generic
type resolved by Factory MethodSpec2B000019: Func<Device,PluginWrapper>, still
deny-only and without constructing a wrapper/plugin. Run `94ea2c8b5d71`: build,
original Initialize, Installation binding/attachment and the genuine Device
constructor succeed. Device.Bind then requests IObjectContext.get_AllDevices
0600025A, which remains explicitly unsupported. Whole run FAIL2 with normal
process termination this time; the prior crash cause remains unexplained.
Input33+2 integrity and processes0/scratch0 PASS. The next increment supplies only
a bounded typed view derived from actual canonical device collections.

Source27 run `f5b06a79d2c4`: build PASS and the real canonical AllDevices reference
allows binding to advance. The next exact request is get_Installation06000262,
still unsupported. Whole run FAIL2,33+2 pins and processes0/scratch0 PASS. The
next increment supplies the already retained genuine bound Installation with
explicit project/collection ownership checks and batches adjacent verified
reference getters required by this same binding/checker path.

Source28 batches ten exact readonly context mappings to the real default
Installation and its canonical collections. Run `5c34ab8de907` advances past these
requests; Device.Bind next calls inherited ICacheProvider0200002D.HardwareCache
060001B6, explicitly refused as an unverified contract member. Whole run FAIL2,
33+2 pins and processes0/scratch0 PASS. The next bounded inquiry must establish
the actual cache signature/provider and real product-data route. Returning an
invented Hardware, null success, or implementing a product compiler in this host
cannot satisfy that requirement. Original UI remains NOT RUN.


## Visible original UI checkpoint — 2026-11-02

Source75, uncommitted worktree based on `37fcb089b0d851210413af1122d2bf9773fd16a9`,
was built on Windows x64/.NET Framework 4.8 and executed on the dev host without
an installed ETS. The private run records are
`private/evidence/task010-sdk-wrapper/iteration01/run-07c0bbcf827f` and
`private/evidence/task010-sdk-wrapper/iteration01/run-d837f91b763a`; source audit
SHA-256 `7d3e181fbe760bd36b8912351302c04ba3db393798a7a8c585db0782b3b8fc42`.
Both runs logged `DCA_UI_RETURNED genuine-original-view=True` and
`DCA_UI_VISIBLE original-control=True rendered=True`. Simon reported that the
application ran, interacted smoothly, and opened dialogs in the second run. This
is a manual observation, not an automated interaction test or a device qualification.

Both complete processes returned exit 2. On the first run the window closed at
the 600-second deadline; disposal subsequently requested the unsupported
`IProductStore.get_FilePath` and the lifetime audit failed. On the second run,
a DCA device connection attempt raised `Device connection failed` and terminated
the run; this is not evidence of a successful connection or of network isolation.
The collected postchecks report 42 private input pins and two OS pins intact,
zero owned processes, removed scheduled tasks, and three scratch files each.

The visible rendering and the user's manual interaction are positive evidence for
AC-001. Task acceptance remains open: resolve and verify normal cleanup, inspect
the exact environment and package-hash evidence for AC-002, execute the required
missing-dependency negative test, review the source and commit the tested result.
The selected product/application match remains unqualified under Task011; hardware
testing and a complete network-isolation proof have not been performed.


### Source76 normal-close follow-up

Source76 local C# compilation (three configurations), PowerShell parsing (two
scripts), diff check, and Windows build passed. Its audited uncommitted source
is identified by `source-audit76.json` SHA-256
`14433cf9bd0f31b56fdb18c8193fe1d42b2dc40682fa36d661d7f313b450f109`.
Attempt `2bc6b15f3ca8` never launched the DCA: the launcher refused an unsupported
five-second UI argument (exit 127). Run `a975f23d8e11` used valid arguments,
rendered the original UI and closed it through its normal window request. The
original product-store path was then supplied during disposal, advancing beyond
the previous failure. Cleanup next refused unverified
`ICacheProvider.get_AllowCachesToGoWeak` (060001C4); the whole process exited 2.
Postchecks: 42 input and two OS pins intact, scheduled task removed, zero owned
processes, three scratch files. Detailed private evidence is in the run directory.
This is progress in diagnosis, not a clean lifecycle or task acceptance.


### Source77 normal-close follow-up

Run `b1b2709dc83f` built on Windows, rendered the original UI and closed it
through a normal window request. During disposal, the original product-store
path and genuine imported-cache weak-policy getter/setter were reached. The next
request, `IProjectStore.get_Project` (060002F9), was refused by the fixture's
content eligibility guard; lifetime audit failed and process exit was 2. Input
integrity (42+2 pins), task removal and zero owned processes passed; three scratch
files remained. Private evidence is under the run directory; source-audit77 SHA-256
`e1fc137dbbbd2acdf0cd2ff4ad4c6d3a5b093f1dd6c316c742995ca1173d6a60`.
The next step is to establish the original project-store contract before changing
the disposal behavior. Clean teardown and task acceptance remain unproven.


### Source78–79 transient-writer follow-up

Source78 `495c6318f2d0` exited 2 before the UI because its preparation incorrectly
required the original abstract `FileStore` base to be concrete. Source79 corrected
the type-shape guard; its Windows build passed and `0be7db829f47` rendered and
closed the original UI. Disposal reached the genuine project-store request but
its `AsWriter` call was refused; process exit remained 2. Input checks passed
(42+2 pins), owned processes were zero and scheduled run tasks were removed;
three scratch files remained after Source79. No project-write success is claimed.
Private run evidence and source-audit79 SHA-256
`c36bba412722b99db1e000b0227e32800f8e4711c04618f2e6d038ee2ce7aa18`
identify the attempted uncommitted code. Investigate the precise writer refusal
before another run.


### Source80 writer diagnostic

Run `71df208af413` built, rendered and closed the original UI, and exited 2.
Additional bounded diagnostic logging exposed the first writer error as
`System.InvalidOperationException` inside original `TransactionalFileStreamManager`
while opening the writer. No successful serialization is asserted. Input pins
(42+2) passed, the scheduled run task was removed, zero owned processes remained
and there were three scratch files. Private run evidence and source audit SHA-256
`8d0eef4f07125d22bf7410c4bfe8b90b49fed1774ba365e1623f4559debad311`
identify this uncommitted experiment. Determine the original transaction lifecycle
before modifying writer behavior.


### Source81 transaction-root follow-up

Run `901ee69ad4fc` built, rendered and closed the original UI. The original
transactional XML writer was prepared after placing the temporary content file
inside the genuine SDK save transaction root. Serialization then requested
`IProjectStore.get_HistoryEntries` (060002FB), which the fixture refused; process
exit remained 2. No completed save is asserted. Postchecks: 42+2 input pins intact,
zero owned processes, scheduled run task removed, three scratch files. Evidence
is under the private run directory; source-audit81 SHA-256
`86f36c252c7a9085a2dd4799b8223a1134b92eafde9d50875d40e7ee49d0697a`.


### Source82 project-store follow-up

Run `906b2ae6c3dd` built on Windows, rendered and closed the original UI. The
original `FileProjectStore` supplied HistoryEntries, ProjectTraces,
DeviceCertificates, AddinDatas, UserFiles and Installation store references
during the temporary save. The write-context `IamDirty` request was then denied
while the fixture reported an active scope; process exit was 2. A completed save
is not asserted. Postchecks show 42+2 input pins intact, zero owned processes,
the scheduled run task removed and three scratch files. The private source audit
SHA-256 is `056186b0bafb26daa07bf6ee29104c232d264d2c04a5d6ea81deb355fbeb1f90`;
full evidence is in the private run directory.


### Source83 dirty-state follow-up

Run `39527dce834f` built, rendered and closed the original UI. The scoped
same-project dirty-state query and `SetClean` advanced serialization; the owned
write scope was released. A later `IObjectContext.get_Project` request was denied
because the fixture did not admit that method in the disposal phase. Process exit
was 2; successful save or lifecycle is not claimed. Postchecks: 42+2 input pins
intact, scheduled run task removed, zero owned processes and three scratch files.
Private source-audit83 SHA-256
`4e38c722685e60814085c33581bcf5f374d06eef06827000b6f344904e3a0f0c`; detailed
evidence is in the private run directory.


### Source84 project-context follow-up

Run `0c148e6e70f7` built, rendered and closed the original UI. The retained
project reference was accepted during disposal; the next original serialization
request, `IProjectStore.get_AllAreas` (06000303), was refused. Exit remained 2;
no completed save is claimed. Postchecks: 42+2 input pins intact, scheduled run
task removed, zero owned processes, three scratch files. Private audit SHA-256
`a9325efd4004a49829c6b3b596f046b299117839e5f73e6999bd8ef1cd855af0`; run
evidence remains in the private run directory.


### Source85 store enumeration follow-up

Run `821ad039c25f` built, rendered and normally closed the original UI. The
original project store supplied its AllAreas enumeration and the write scope was
released. The next `ICacheProvider.DisposeCachedInformation` (060001C0) request
was refused; process exit was 2. This does not establish successful final cleanup
or a durable configuration round-trip. Postchecks: 42+2 pins intact, scheduled
run task removed, zero owned processes and three scratch files. Private source
audit SHA-256 `477e21db0489f739c8ea589ee3038d0782bd22576262ec9dcea0077b25327607`;
full evidence is in the private run directory.


### Source86 cache-release follow-up

Run `ad103e76f2a9` built, rendered and normally closed the original UI. Original
cache weak-reference eviction completed and the SDK project disposal returned.
The process then hit `PROJECT_LOG_LIMIT` before complete final audit evidence and
exited 2; successful overall cleanup is not claimed. Postchecks: 42+2 input pins
intact, zero owned processes, run task removed, three scratch files. Private audit
SHA-256 `77edccc6b8c22b05bf338d98ffc982bbd629f8ffca4b703703844d3b369bafe3`;
full evidence is in the private run directory.


### Source87 terminal-audit follow-up

Run `9d16336d40fb` built, rendered and closed the original UI. Original project
and import disposal completed; the first PROCESS_FINAL audit set recorded no
fixture faults and emitted PROJECT_RESULT exit=0. A duplicate terminal-audit pass
then exhausted the 65536-byte log cap: the launcher observed child exit 2. The
inner marker is therefore not a successful whole-process result. Postchecks: 42+2
input pins intact, zero owned processes, run task removed, three scratch files.
Private source audit SHA-256
`354acc6e44e1b17d0829e0ca98e5c00618a6e97386ef69865a2647026ba77a2e`.
Deduplicate terminal reporting while preserving every actual validation and rerun.


### Source88 successful offline lifecycle — 2026-11-02

Run `02d5ec78d2cb` built on Windows and rendered the original DCA control. A
separate interactive-session helper requested a normal window close; the SDK
project and imported product stores completed their managed cleanup. The final
revalidation marker recorded exit 0 and the launcher independently confirmed
child exit 0; the scheduled task result was 0. Postchecks reported 42 private
input and two OS pins intact, zero owned processes, removed run task, and three
scratch files. The launcher's conversion temporary directory was removed after
confirmed child exit. Private evidence is under
`private/evidence/task010-sdk-wrapper/iteration01/run-02d5ec78d2cb`; audit SHA-256
`e9c78cc59c8d85df9aed8d27e2ec97c9a1991a371a3ca938d48be0f232a94606`.

This is an original offline UI/lifecycle observation, not product/application
qualification, durable configuration round-trip, hardware validation, or network
isolation proof. The general PROJECT_RESULT diagnostic still says ui=NOT_RUN;
the separate DCA_UI_VISIBLE and DCA_UI_CLOSED markers are the actual UI evidence.
The required negative dependency/environment checks, independent review and source
commit are still pending.


### Current dev-host readiness recheck

A read-only PowerShell inventory on 2026-11-02T19:47:34.9737669Z reported host
`DESKTOP-3UKTEIP`; the selected ETS5/ETS6 Program Files and Program Files (x86)
roots were absent; matching uninstall-registry entries, named services and named
processes were empty. Output is stored in
`private/evidence/task010-readiness/windows-clean-readiness02-stdout.txt`. This
limited name/root search does not establish the absence of every ETS component or
network service path; the exact original DCA inputs in the test remain private.


### Source89 reviewed-finding follow-up

Run `56aa362ec007` used the repaired static harness and truthful UI lifecycle
reporting. On Windows the focused owned SDK project fixture test exited 0, then
the original DCA rendered and normally closed with child exit 0 and
`PROJECT_RESULT ui=RENDERED_CLOSED`. Both scheduled run tasks were removed;
original postchecks reported 42+2 pins intact, zero owned processes and three
scratch files. Private audit SHA-256
`817f1f25359f457e09baa29c1a40ef4f4a68336a23e0a6e250a16663e76fa5f5`; evidence
is in the private run directory. The focused fixture test does not replace the
full regression suite, original missing-dependency negative, identity qualification,
or an independent final review. Source remains uncommitted.


### Full owned Windows regression — 2026-11-02

The complete owned constructor suite ran in interactive Windows session2 as run
`60e2151ed943` using source-audit89 and returned `OWNED_SUITE PASS`, with 423
`OWNED_CASE PASS` records and scheduled result 0. It exercised owned fixtures;
original vendor execution and hardware were NOT RUN by that suite. Its complete
stdout, stderr, scheduled status, intent and source audit were downloaded to
`private/evidence/task010-sdk-wrapper/iteration01/run-60e2151ed943`. The generic
collector subsequently failed trying to fetch the SDK-smoke-only file
`sdk-project.log`, which the full suite does not create; the successful suite
result and captured driver output precede that collector error. This does not
replace Task010's separate original-runtime missing-dependency test or Task011's
identity negatives.


### Rust workspace check availability

On 2026-11-02, `cargo fmt --all -- --check`,
`cargo clippy --workspace --all-targets --locked -- -D warnings`, and
`cargo test --workspace --locked` could not start locally: `cargo` was not in
the environment (shell exit 127). A read-only `Get-Command cargo.exe` on the
Windows dev host also reported unavailable. The repository-private pinned toolchain was subsequently found at
`private/toolchains/rustup/toolchains/1.99.1-aarch64-apple-darwin/bin`. With this
directory prepended to PATH and `CARGO_NET_OFFLINE=true`, all three commands
passed on 2026-11-02. The Rust workspace currently contains only a toolchain smoke
binary; its test command ran zero tests. Windows DCA-specific results are
separate evidence.


### Original-profile missing-dependency negatives

On Windows, isolated run `missing-original89-a15c9e8376d2` exercised the unchanged
original product profile against fresh private copies without `DotNetZip.dll`.
The 41-entry set failed with `PROJECT_EXACT_INPUTS`; a 42-entry set containing a
non-DLL omission marker failed with `MISSING_PATH`. Both children exited 2, no
vendor load, initialization or replacement UI was observed, and all 42 canonical
inputs remained unchanged before and after. The scheduled test finished with
result 0 and was removed. This proves fail-closed original-profile admission,
not a late CLR resolver failure after vendor startup (NOT RUN). Private handoff
SHA-256 `5de5eca6f78480265ec3316261399eef6ff997257a518179bf46ddab37167221`;
the source audit is the Source89 audit above.
