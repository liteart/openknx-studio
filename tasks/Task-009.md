# Task-009: Implement only the required offline SDK compatibility surface

## Subject

Implement only the required offline SDK compatibility surface

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-009 |
| Status | Blocked |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host (C#/.NET) |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Supply the smallest lawful SDK-compatible services needed to progress reference DCA initialization.

## Detailed Requirements

- DR-001: Implement only the approved contracts identified by Task-008.
- DR-002: Use explicit fixture responses for offline project/product services and label them as fixtures in diagnostics.
- DR-003: Fail unsupported methods with named diagnostics; never return generic success/null for an unimplemented required operation.
- DR-004: Record requested services/callback order and assembly binding failures.
- DR-005: Keep all bus methods denied and preserve exact original DLL bytes.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Required shim contracts are covered by tests and map to the actual dependency inventory.
- [ ] AC-002: Unknown service requests fail deterministically with no external side effect.
- [ ] AC-003: No shim contains KNX encoding or hidden ETS executable calls.

## Dependencies to Other Tasks

- [Task-008](Task-008.md): Map the actual ETS SDK surface used by the reference DCA. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: approved interface behavior and callback lifecycle.
- TEST-002: Negative: invoke every unsupported service category and confirm explicit failure.
- TEST-003: Static: compare original DLL hash before/after and scan forbidden dependencies.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host (C#/.NET)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No broad ETS clone, live bus access, or production-quality adapter yet.

## Risks and Blockers

The exact ObjectModel candidate has been privately acquired and the Project constructor declaration has guarded metadata evidence. A compatible construction route and fixture semantics remain unresolved. ADR-005 already governs owner-authorized private use; no legal-document gate is reopened. Do not replace the missing concrete SDK functionality with a guessed object or fixture.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Blocked; latest original Factory13 diagnostic FAIL10 at unregistered IProjectTraceEncryptor, contract/semantics UNKNOWN; no qualified Factory/Project/service |
| Source commit(s) | Original13 reviewed source `44b1f13e4b07a8ea5251fb0eeb2ef14c62de5227`; prior source/result history retained |
| Tests executed | Source13/owned Windows29/60/34/full18 preflight PASS remains separate. One original13 FAIL10: four null resource probes, no satellite load observed, context READY but Factory not returned/unregistered service. Earlier original12 FAIL2/Factory11 unknown origin retained; explicit Project/Id/UndoManager/DCA/UI/state/hardware operations NOT RUN |
| Evidence location | [Project qualification](../docs/DCA-PROJECT-CONTEXT.md); private `task009-constructor-windows/factory13-observation.json`, `factory13.log`, and `verifier13/factory-result-audit.txt`; earlier dated records below |
| Acceptance criteria met | None asserted |
| Technical outcome | THIS13 ValueTuple GAC pin/audit allowed, builder/Build/context READY; Factory0600012E ATTEMPTED_NOT_RETURNED/body entry NOT_ESTABLISHED. Four NULL_UNKNOWN probes/no ownership proof; phase inactive before disposal. ComponentNotRegisteredException names IProjectTraceEncryptor; owner/interface/semantics/licensing/KNX Secure relationship NOT metadata-qualified. No service implementation/qualification |
| Gate outcome, when applicable | No gate applies or advances |
| Reviewer | Source13 source/owned critical review PASS; original13 provenance audit and critical result documentation review PASS (the diagnostic remains FAIL exit 10; private review13/critical-review-result.txt). Prior reviewed results preserved; no task/service acceptance |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Factory13 actual result

Reviewed source `44b1f13e4b07a8ea5251fb0eeb2ef14c62de5227`: one **FAIL10**,
no retry/widening; full 18 artifacts/nineteen private+OS pins passed. THIS13 ValueTuple
exact GAC/path/hash allowed; builder/Build/context READY, not qualified. Factory `0600012E`
not returned/body entry unestablished. Four allowed null probes 2 en-GB/2 en, all requester
NULL_UNKNOWN/no ownership proof; phase inactive count4 before disposal. No satellite
load observed, not prevented/guaranteed. TargetInvocation→ComponentNotRegistered names
IProjectTraceEncryptor unregistered; assembly/interface/methods/semantics/licensing/
KNX Secure relation NOT metadata-qualified, no crypto/no-op fixture. Provider requests0/context
NOT_INTERCEPTED/disposal1/auditFalse0; PROJECT_RESULT10 present/no dependency blocker,
launcher/done/cleanup10 confirm. Session2 limited 15s/64KiB cleanup expected/last10,
removed/0process/0scratch. Observation 7,412B SHA-256
`96f4ce76f3e729fcf86d2066a05ee77d9fa03074de308e187189a8a2356e2867`
raw 15,548B `2f7156c23a91b55dcfbf0fa1fed240e68602bb62fb4255e6fb1dc7f06acd2c1f`;
independent provenance audit PASS `3abe83b4dd622f8b209d229e459804fd4c0210520cad0339cd0814fd10cf1487`.
Four exact Python commands/limits in contract; critical result documentation review PASS (private review13/critical-review-result.txt); diagnostic remains FAIL10.
Owned 29/60/34 history separate; after-load/final-guard/no-containment caveat preserved,
explicit Project/Id/UndoManager/DCA/UI/state/hardware NOTRUN/internals UNKNOWN.
Task Blocked/ACs unchecked/no qualification/gate; Factory11 origin still UNKNOWN.

## Source13 exact resource-probe source freeze

Reviewed baseline `a58e675c0cd918eb58f10a0c5dccfc80d7ecc126` retains original12
FAIL2. New diagnostic exception returns null/no Assembly only for exact two
Autofac.resources8.2/en-GB/en identities, original-empty/completed19 profile,
verified pinned owner and invoking thread's active Factory constructor phase.
Nonnull requester must equal that actual Assembly; NULL is UNKNOWN, not ownership
proof. Eight probes maximum, finally-clear before disposal, fatal unknown/overflow
and all actual-satellite load audits remain. No SDK null service/file/admission/
culture/config/input/build changes. Underlying Factory cause is a hypothesis.
Separate real owned CLR fallback proof/audit hashes and primary Microsoft URLs
are in the contract; every observed requester was NULL, no Autofac proof.
Local C#5/x64 six-reference compile/PS parse/guards/preservation/scope/diff PASS;
prepared4positive/26negative/bounds/actual-phase-thread-cleanup tests, helper SHA-256
`ed322e4436329b6a0cec72b20c9deaf4712c35d2c046cb11cf081f3d41c04bac`.
Windows13/original13 NOT RUN at this freeze; independent verification/critical
review pending, no future verdict/commit claimed. Latest actual original12 FAIL2,
Task Blocked/all ACs unchecked/no qualification/gate advance.

### Subsequent Windows13 owned result

Same pinned build/eight sources/eighteen artifacts/configs; owned **PASS 29/60/34**,
all three/full18 artifact preflight/frozen driver. Session2 roles/done 0/cleanup 0/removal/
zero processes/no junction/151/73 files/strict encoding preserved. Pure policy4+/26-,
bound8/thread lifecycle/non-admission and earlier modeled/disposal regressions PASS;
no actual Autofac/OS/resource fallback proof. Observation 56,339B SHA-256
`257db46eafe59bf94cca5e28ebf09189321c45ca8042930e637dc52da96d8b43`
binds 283 files; independent result audit PASS SHA-256
`f43d20ed63b2569882397bae2f6484150bff6116f27dd5a71501ba908db6cf90`.
Handler returns NO Assembly but CLR satellite load/use MAY occur before AFTER-LOAD
latch; final guard AFTER STA callback/constructor/disposal, no immediate post-ctor
check. No prevention/sandbox claim. Nine Python helpers ran once/first publication PASS
(commands in contract). Critical boundary review PASS; private review13/critical-review-source.txt records the bounded verdict. Original13 remains NOT RUN/resource fallback NOT QUALIFIED.
Latest actual12 FAIL2/root unknown/Factory11 origin unknown remain, Task Blocked/ACs
unchecked/no gate or qualification. Historical source-NOT_RUN retained.

## Source12 pre-Build guard and OS reference freeze

Against reviewed baseline `8434aec9c4e78eb4aeec64d5638c4d6ee0f98ca7`, production
latches are checked immediately after builder/exact-type and before Build;
after-Build/disposal/provider checks remain. Private owned controls use the same
guard and never reset production flags. Prepared modeled baseline Build1/use1
then both controlled flags require builder1/Build0/use0/Factory0, no vendor rerun.
Only original-empty gains exact held OS ValueTuple reference pin25544B SHA-256
`62216986c754ad8f0ee68412c42cfc3fc6e025bd908ae1fef1a3a8d528b1495a`,
after19private hashes/before file identities; completed pin/exact identity/path/
GAC/nonreflection required, with denied-path logs and owned predicate negatives.
Independent Microsoft-only observation/audit provenance is in the contract;
not facade classification or historical Factory11 origin proof. No input/config/
redirect/Framework expansion; nineteen private pins/seven other sources unchanged.
Local six-reference C#5/x64 compile/PS parse/guards/preservation/scope/diff **PASS**;
fresh helper SHA-256 `c168266e38f1038f378411603799ad335f924ffd18ecb0daf2b8bf94174dd226`.
Windows12/original12 **NOT RUN** at source freeze; verification/critical review
pending. Task Blocked/ACs unchecked/no gate or qualification; Factory11 FAIL2 latest.

### Subsequent Windows12 owned verification

Build PASS/eight compiled sources/eighteen artifacts/same compiler/configs;
owned **PASS 28 Session0 /59 limited /33 admin-negative**, full 18 artifact size/hash
preflight/frozen driver in all runners. Session2 roles/done 0/cleanup 0/removal/
zero processes/no junction/151/73 files/strict encoding preserved. Modeled local
baseline Build1/use1 and both fixed flags Build0/use0/Factory0 PASS, no actual latch
reset; existing post-disposal regression/nine disposal markers and pure ValueTuple
predicate cases PASS. Actual original OS pin/Factory12 NOT RUN; separate Microsoft
observation proves neither historical Factory11 origin nor facade classification.
Observation 55,872B SHA-256 `15cd3120fd81e4ece4a72c1c3032f260e89668e200835b6d8bae377702d7389e`
binds 284 files; independent result audit PASS SHA-256
`ee3a0fd789df708fdf9e85894fc16084d4a7c522120d33be94ab531fa46179e3`.
Exact nine Python commands in contract executed once, first observation publication
PASS. Critical review PASS; private review12/critical-review-source.txt records the bounded verdict. Latest actual Factory11 FAIL2/no Factory BEGIN preserved; Task Blocked/
ACs unchecked/no qualification/gate; original12 remains NOT RUN.

## Factory11 actual original result

Source `49d4c155bf13d5ca92cdaf9f4650a8dc6cfd81c8`: one original11 **FAIL2**,
no retry/widening, full18artifact/nineteen-input guards. Builder02000015 ctor06000026
returned after disallowed ValueTuple4.0.0.0/cc7 loaded; path/hash/origin UNKNOWN,
after-load detection not prevention or CLR/ABI proof. Build0600002E was then invoked
and returned nonnull before EMPTY_CONTAINER_BUILD_DEPENDENCY stopped readiness.
Disposal returned once/final audit scopeFalse requests0; no READY or Factory BEGIN,
Factory constructor NOT ATTEMPTED. No standalone PROJECT_RESULT; launcher/done/
cleanup confirm2. Session2 limited interactive15s/64KiB, cleanup expected/last2,
task removed/zero processes/scratch. Observation6,078B SHA-256
`89873990517c6c9901a564bc5d926aa5dd4f706bc1cc7781e63aeef47e501ba4`
binds25files; raw11,707B SHA-256 `62b7ba9c4c4b11306e56fdeae2e655f62885f6b984a88f9878e4888d844ec6db`.
Independent provenance audit PASS SHA-256
`b8ac5e00816eb8987d854aa050175e7404eae72c02d516745fc7c083c3bb7b4b`;
Critical result documentation review PASS; the diagnostic remains FAIL exit 2. Actual commands in contract. Source11 source/owned
PASS27/58/32 and earlier NOTRUN/source10 FAIL history retained separately; no
Project/Id/UndoManager/DCA/UI/state/hardware operations or qualification/gate.
Task Blocked/ACs unchecked; pre-Build guard/ValueTuple readiness are proposals only.

## Empty-container diagnostic source freeze

Baseline `61f3e43c785b015abd22c2006260d8ea7d94c137` retains reviewed Factory09
FAIL10. New fixed original/owned-factory-empty-container modes use only separate
ProjectFactoryContainerProbe.exe/config: genuine public builder/Build(None), zero
application registrations (defaults retained), then reviewed Factory constructor,
with Kim denied. No Register/Resolve/Factory method/Project/Id/UndoManager/DCA call.
Genuine context requests are NOT_INTERCEPTED; caught provider denial still fails.
Shape guards precede builder effects; retained container disposal is attempted once
on managed outcomes, disposal failure fails; hard-kill disposal NOT_ESTABLISHED.

Initial fourteen-input readiness was held at actual DiagnosticSource8 metadata.
Explicitly approved nineteen-input profile keeps the fourteen prefix exact and
adds only five private pins/24,278,056 total bytes; manifest SHA-256
`a3f133f73e6240493264485d4c7ce5fd87855e7ffde158bc173db5ca4d872d98` and audit PASS
`0c17029082f47decbc15def3860f6ab402e916e25656ac2c9356d8af8a5e66d0`
bind bounded declared closure across six libraries, not the whole SDK/runtime.
All nineteen hashes/sizes precede OS/identity; only this completed original profile
admits the five exact private identities/paths, overriding Framework/GAC fallback.
The separate config preserves old Bcl exact8-to9 and adds four exact policies,
no broad ranges/vector redirect; SHA-256 `60ac03885b7bbd3b944bb91d27017220fadcde8f7ade15cc1d59ea1d16177225`.
Old modes/configs/fourteen arrays/constructor methods remain; eighteen artifacts.
Immutable Autofac8.2 API evidence and all pin/identity/redirect details are in
[the contract](../docs/DCA-PROJECT-CONTEXT.md).

Local **PASS**: six-reference C#5/x64 compile, Build/Test PS parse/source guards,
manifest/preservation/scope/diff checks. Fresh helper SHA-256
`12e73fa105abb32aa813c1b8e715c64e5bc6b42f5208b76561172f625e9bd2eb`;
exact normalized command in contract. Runtime policy/API/lifetime tests and new
Windows owned/original execution **NOT RUN** at this chronological freeze;
source/owned verification and critical review pending. Original Factory09 FAIL10
and owned Windows09 PASS23/42/28 remain separate. No ABI/service/Project/DCA/UI/state/
hardware/gate qualification; Task remains Blocked, all ACs unchecked.

### Subsequent Windows10 owned result

Pinned compiler/build **PASS**, eight source bindings/eighteen artifacts; observed
owned cases **PASS 27 Session0 smoke /58 limited Session2 nonadmin /32 admin-negative
Session2 admin**. Only limited/admin preflighted all 18 artifact sizes/hashes.
Session0 checked compiler/source report/test-driver hash, but full 18 artifact binding
is **INCONCLUSIVE**, not retroactively established; no rerun. Both done/cleanup 0,
tasks removed/zero owned processes/no junction, 151/73 case files and strict raw UTF16/
UTF8 derivatives preserved; cleanup UTF8/Session0 CLIXML stderr retained.
Observation 55,878B SHA-256 `b6530e8ed3ca9d104e2ca3f047201fbbb06c1aed70fb743813cf87f38cef67bd`
binds 291 files/freeze `bb4557c60bc0b5086665efc4c50412ec73cb02495dc40948f7821a6dcc6f9f2e`.
Independent bounded source/owned-result audits **PASS**, latter SHA-256
`f576b686002dbd839c539fea7a7d0b34474e691ed3d28683f27d3dafcce784ac`.
CLR policies/near-misses/no target-load and owned lifetime/API/provider/count/config
negatives passed; original Autofac Build was NOT RUN. Input19 copy/hash-only and
prepared original10 helpers source-only; original requires full 18 artifact preflight.
Critical source/owned review pending, original empty-container **NOT RUN**; latest
actual Factory09 FAIL10 remains. No qualification/gate; Task Blocked/ACs unchecked.

### Generation11 disposal-denial source repair

Generation10 critical source readiness review **FAIL**, one P2: Dispose could
invoke/catch a denied provider call after the constructor-only check. Historical
freeze10/build10/owned27/58/32/audits are preserved, not accepted readiness; no
original10 attempt occurred. This is a source finding, not original SDK behavior.
The shared original/owned WithFactoryContainer wrapper now audits provider requests
in finally after the complete managed lifetime/disposal and fails on any denial;
constructor precheck/exact-once disposal/error failure remain. The owned regression
requires an unaudited return with denial1/disposal1, then production-wrapper failure
with denial1/disposal1, using owned bytes only. No other source/config/pin/mode change.
Local C#5/x64 six-reference compile/PS parse/source guards/scope/diff **PASS**;
fresh helper SHA-256 `fdcdc0fa74aaa9c11fe2365dedf6d23bcf9310b23b48a8abfa47fcc11456fc82`.
Windows11 owned regression/runtime and original container execution **NOT RUN**
at this freeze; verification/re-review pending. Hard-kill disposal/audit is
NOT_ESTABLISHED, asynchronous requests outside observed lifetime UNKNOWN; no timing
promise. TaskBlocked/ACs unchecked/no qualification/gate, Factory09FAIL10 retained.

### Subsequent Windows11 owned verification

Build PASS (same pinned compiler/eight sources/eighteen artifacts); owned suites
**PASS 27 Session0 /58 limited /32 admin-negative**, with full 18 artifact size/hash
preflight in all three runners. Generation10's Session0 gap remains INCONCLUSIVE,
source10 critical review remains FAIL; no original10 attempt/source10 commit.
Owned regression reproduced unaudited caught-Dispose denial1/disposal1, then shared
original wrapper rejection with exact counts; expected failure asserted PASS,
genuine SDK/container NOT RUN. Session2 roles/done 0/cleanup 0/removal/0process/
no junction and 151/73 files/strict encoding bound. Observation 55,147B SHA-256
`b7016e423ac53dd56007aadff4a525d318ea8fb28a1373bc0f99b480e34ac713`
binds 283 files; independent result audit PASS SHA-256
`13919348234faef7b69ea2e58b32327b5efb9f20778bfd204a01e22f62509f09`.
Exact executed Python helper commands/local audit command are in the contract;
initial record-helper hash assertion failed before publication then corrected,
not a Windows/SDK rerun. Critical re-review PASS/original11 NOT RUN; private review11/critical-review-source.txt records the bounded verdict.
Factory09 FAIL10 remains latest actual original; Task Blocked/ACs unchecked/no gate
or qualification. Hard-kill audit/asynchronous-outside-lifetime limits remain.

## Factory09 actual exact-binding result

Source `28f53d27a24b4f11d5507df0a8178063f23bc872`: one original-factory-bcl9
attempt **FAIL10**, no retry/widening. CLR exact8-to9 preflight precedes fourteen
pins/identities READY; pinned9 identity/private path/hash observed after ctor BEGIN,
with no Bcl Resolve event. Previous dependency blocker not reproduced this attempt,
not ABI proof. Factory0600012E returnedFalse with one denied zero-argument
Autofac.get_ComponentRegistry06000035; TargetInvocationException wraps
NotSupportedException. ATTEMPTED_NOT_RETURNED/body NOT_ESTABLISHED.
Observation4,632B SHA-256 `e62347b3872e81a2ed5d6db96d271fba35810704509e3e458f8cba35178ed675`
binds nineteen files; raw8,654B SHA-256 `7c681006c4d431deb82cf209a1146450624bf56ce7bf37f5942b325028e49279`.
Session2 limited interactive, child10 confirmed15s/64KiB, cleanup expected/last10,
task removed/zero owned processes/scratch. Independent result audit **PASS** SHA-256
`13fb7419b2e95ed23ad0e43bd63ac8d7a1f9e4fcfb28bd9ae6a80d7f308c527a`;
Critical result documentation review PASS; the diagnostic remains FAIL exit 10. Historical source-NOTRUN/owned23/42/28/Factory08FAIL2
retained separately. Harness Project/Id/UndoManager/DCA/UI/state/hardware NOTRUN,
internals UNKNOWN/network and descendants NOT_CONTAINED. No qualification or gate
advance; Task Blocked, ACs unchecked. Any empty Autofac candidate is separate readiness.

## Isolated exact Factory binding source freeze

From reviewed baseline `93575a2ea27bb04cc20c660de0d612fe6cbe42c4`, new fixed
`original-factory-bcl9` / `owned-factory-bcl9` modes select only a separate
ProjectFactoryBindingProbe.exe/config. The four legacy redirects remain exact;
only AsyncInterfaces8.0.0.0 -> existing pinned9.0.0.0 is added for this alias.
Installed policy observation `90be6ee9043337af24311e3c743adba35f3742102a7c5c64d4d955a5fa6d1da9`
is declaration evidence; its broad range is not copied or treated as compatibility.
Exact config/executable/mode and real CLR policy checks precede input inspection;
all fourteen existing pins, deny-all dependencies and limits remain unchanged.

Local **PASS**: pinned-six-reference C#5/x64 compilation, Build/Test PS parse,
source guards and old config/arrays/method preservation. Fresh helper SHA-256
`d198b388b71158517e25f91ad12e22edd7895bb36152856764fceb609fa74a45` and normalized
command are recorded in the linked contract. Old config SHA-256 `b72d33cb400c6a9de0565ece3698308c569836dc41f9003d70d9d78a31b4315a`;
alias config `e1309cb227dcf7962a8443c06fe979b11d97db078fa0507456a2b627f5d8a3ba`.
At this chronological source freeze Windows owned/real-policy checks and new
original execution are **NOT RUN**, independent verification/critical review pending.
Factory08 remains the latest actual runtime **FAIL exit 2**; owned policy results
cannot establish ABI, Factory/Project/Id/UndoManager/DCA/UI/state/hardware support.
Task stays Blocked, all acceptance criteria unchecked and no gate advanced.

### Subsequent Windows09 owned result

Pinned Windows build **PASS**, eight sources/fifteen artifacts; owned suites
**PASS 23 Session0 / 42 limited Session2 nonadmin / 28 admin-negative Session2 admin**.
Both done/cleanup exits are 0, tasks removed, zero owned processes/no junction;
107/65 case files plus raw UTF-16/strict UTF-8 derivatives are preserved.
Observation `owned09-observation.json` (43,681 bytes), SHA-256
`e02782b11b55d1cebfea56bb7976fe278b5c33652a37dc5ec17193ecfa20e7c5`,
binds 229 files and source freeze `7231879c2d167252def04f35364ee98ccf486082ce07fd6c5b85754d2a6dc112`.
Real CLR exact8-to9 preflight precedes owned fixture pin without Bcl target load;
near misses, old policy and five config negatives pass. Original fourteen pins
are unchanged and not loaded. Initial local observation/audit newline/encoding
assumptions were corrected without a Windows failure or rerun. Independent
source/owned-result audits **PASS**; latter SHA-256
`9413878d59aa1b11389f458529c77f47dc9c7500c7c827683a2d0792ff92de86`.
Critical source/owned review PASS; original09 helpers are unexecuted
source only and original bcl9 is **NOT RUN**. Latest actual original Factory08
remains **FAIL exit 2**, no qualification/gate advance; Task Blocked and ACs unchecked.
The helper is preserved as `private/evidence/task009-constructor-windows/owned09-sourcecheck.ps1`.

## Readiness and blocker record, 2026-10-22

Task-008 is accepted as dependency analysis at reviewed source
`89b08d24631021eb9863e7f1737753af0b8cfbd9` and result commit `73b869c56855851512badb3a70841dcd76c1fbd5`.
Its actual initialization remains FAIL (15-second timeout, exit 124), with
compatibility INCONCLUSIVE. Before the timeout, the selected runtime rejected
`Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, then logged a request for
`IInitializationContext.get_Project` (`0600002F`), whose concrete return type
is `Knx.Ets.Sdk.Project.Project`. No Initialize end or exception/result marker
was captured. The timeout cause is unknown; these observations do not prove
that the rejected assembly caused the timeout.

The exact eight-input runtime inventory does not contain ObjectModel. Existing
guarded reports identify Project member references but do not provide the
missing ObjectModel dependency surface or establish compatible concrete Project
construction. HostNotification signatures alone do not establish a required
service: its getter and callbacks were not requested. No useful offline shim
can yet be selected from these facts. Repeating Task-008's deny-all diagnostic
would not satisfy this implementation task. All acceptance criteria remain
unchecked; TEST-001/002/003 are NOT RUN for a Task-009 implementation.

The bounded next step is private qualification of the named ObjectModel file
(exact bytes/hash/assembly identity and guarded metadata), then inspection of
the concrete Project construction contract before selecting one evidenced
offline service/fixture. Keep original bytes unchanged, bus services denied
and original DLLs outside Git/distributions. No input allowlist is expanded
from a filename or guessed identity.

The ETS-source SSH control socket was absent when checked; the operator has
been asked to reopen the existing connection. The new test VM connection
remains available. This is a missing technical input, not a request for
renewed legal approval. No Task-009 source, SDK service, original execution,
hardware action or gate result is produced by this blocker record.

Predefined `next_dca_readiness` independently inspected the actual Task-008
result and guarded Task-142/144 reports and confirmed that no substantive
service implementation is ready without guessing. Scope and whitespace
checks for this record are recorded after review.

Blocker-record validation: `python3 -B scripts/check_project_scope.py` PASS;
`git diff --check` PASS. Predefined `binding_review` independently reproduced
these checks and returned PASS without findings. This permits the report to
commit; Task-009 remains Blocked and no implementation criteria are accepted.


## Static Project qualification update, 2026-10-22

This update supersedes the initial missing-source-connection and missing-file
blockers above. The coordinator privately acquired `Knx.Ets.ObjectModel.dll`,
14,204,880 bytes, SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`.
The private copy/source manifest verification and `reference-inventory.json`
identify `Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, exactly the identity rejected in Task-008.
The inventory JSON is 860 bytes, SHA-256
`ff4711dda239231aa51d80d2a20771d4658d8f024514a540a659825615d8e8ad`;
it records compatibility UNVERIFIED and target assembly execution NOT_RUN.
ADR-005's owner-authorized private-use scope remains in force. The file is not
added to Git, distributions or a runtime allowlist.

A fixed `project-context` mode now uses only the existing three exact input
pins and guarded metadata reader. The coordinator's private guarded report,
`private/evidence/task009-objectmodel/project-report-task009.json`, is 20,450
bytes, SHA-256
`85a3d77be7da708e7732618ab2b494457b342018f259f41e26b131985858bf40`.
Write and network denial probes are both true; selected input hashes/sizes
match the pins. The report was re-read as JSON and its bytes/hash verified.
The exact normalized command and detailed observations are recorded in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

Project `020001D1` is public/sealed. Its sole declared instance constructor,
`06001409`, has attributes `6275`, access mask `3` (assembly/internal), and
parameters SDK `ProjectCollection`, ObjectModel `Project.Project`, and SDK
`ISdkContext`. The same-assembly bases are `ProjectRelatedObject` and
`DomObject`, ending at unresolved external `System.Object`. Public
`get_AddinData` (`0600140F`) and `get_Name` (`06001422`) agree with the previous
member evidence; `get_Parent` returns SDK `Root`. No public Project instance
constructor was observed. This is not proof that no public factory or
collection route exists. Those parameter/factory contracts require the next
bounded static inspection before any actual context service can be selected.
No nonpublic activation, null/uninitialized object or guessed fixture is supplied.

Validation: the provisioned PowerShell 7.6.7 owned metadata test command
reported **PASS 23 / FAIL 0**; the Python harness command reported **PASS**,
14 tests; guarded original Project metadata extraction **PASS**, with runtime
execution **NOT RUN**. Scope and whitespace checks **PASS**. These results
qualify only static tooling/observations. TEST-001/002/003 for an actual SDK
service remain **NOT RUN**, all acceptance criteria remain unchecked, and
Task-009 remains **Blocked**. No original initialization retry, Windows SDK
execution, device operation or gate advancement occurs. Commit/review of this
metadata increment precedes any separately reviewed four-input extension.

Independent final review: predefined `project_review` PASS without findings
for the bounded static metadata increment. It reproduced all 23 owned
PowerShell cases, 14 Python tests, scope and whitespace checks, matched all six
source-code hashes, and independently checked the guarded report JSON/pins and
constructor observations. Predefined `objectmodel_verify` separately verified
private candidate integrity and extraction-script provenance. Task-009 remains
Blocked pending an evidenced construction/service route.


## Four-input construction mode source freeze

Following the reviewed Project-only commit `f9549f6`, a separate fixed
`project-construction` metadata mode adds the exact acquired ObjectModel pin.
The five allowed roots are SDK `Project.Project`, `Project.ProjectCollection`,
`ISdkContext`, `Root`, and ObjectModel `Project.Project`. Constructor parameters
are not recursively catalogued. Root-specific nil bases are valid `NO_BASE`;
external bases/dependencies remain unresolved. Public/protected methods returning
a selected type, including generic wrappers, receive declaration-only inquiry
annotations, without any claim that they construct a usable Project. Existing
three-input modes and their pins remain unchanged. See the contract and bounded
fixture evidence in [DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

The existing PowerShell command now reports **PASS 39 / FAIL 0** using owned
metadata images; the Python harness command reports **PASS**, 15 tests. Scope
and whitespace checks **PASS**. The coordinator's guarded four-input original
metadata extraction is **NOT RUN** at source freeze, pending independent
review. No ObjectModel runtime load, SDK implementation execution, constructor
activation or service fixture is supplied. Task-009 remains **Blocked** and
all acceptance criteria remain unchecked. The next authorized action is source
review followed by the coordinator's fixed guarded metadata extraction, not
an initialization retry.


## Four-input extraction limit and bounded diagnostic

The coordinator's initial four-input guarded read failed closed before type
inspection with `DCAENTRY_ROW_LIMIT` under the existing 32,768-row per-table
bound. No report, type declarations, SDK execution or construction result was
produced. An earlier staging attempt correctly rejected mode `0400` with
`DCAPROBE_INPUT_ATTRIBUTES`; staging was corrected to the required `0600`,
without changing validation. The coordinator retained private metadata-child
stderr under `private/evidence/task009-construction/diagnostic-child-2-stderr.txt`.

Before considering any limit change, construction mode now exposes the first
exceeding table/count through an owned structured exception and a path-free
worker diagnostic. The worker unwraps at most eight exceptions and preserves
fields only from the exact owned exception type; generic exception redaction
and all old-mode diagnostics remain unchanged. No row, file, aggregate,
signature, report or process bound is increased.

An owned metadata image containing exactly 32,769 MethodDef rows verifies the
typed count and `DCAENTRY_ROW_LIMIT_METHODDEF_32769` worker failure. The owned
PowerShell command reports **PASS 42 / FAIL 0**, Python harness **PASS**,
15 tests, and scope/whitespace checks **PASS**. The original table count remains
unknown; the diagnostic retry is **NOT RUN** pending source review and the
coordinator's guarded invocation. Task-009 remains **Blocked**. No working
service, fixture or construction route is asserted.


## Measured MethodDef count and narrow policy

The coordinator's guarded diagnostic retained
`DCAENTRY_ROW_LIMIT_METHODDEF_38472` in private
`task009-construction/row-observation-child-2-stderr.txt` for the exact fourth
ObjectModel input, SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`.
That count explains this particular bound failure, without proving any SDK
construction behavior. The coordinator supplied reviewer POLICY approval for
only fourth-slot MethodDef rows in `project-construction`, with ceiling 65,536.

The reader now applies that ceiling only to this mode/slot/table and requires
the fourth assembly name `Knx.Ets.ObjectModel`; the guarded parent's exact
ObjectModel pin and identity checks remain in force. Every other table,
input slot or earlier mode stays at 32,768. File/aggregate, selected-work,
signature, report/output and process bounds are unchanged. No new parameter,
input discovery, original execution or runtime allowlist is added.

The owned PowerShell suite reports **PASS 50 / FAIL 0**: complete synthetic
reports at 38,472 and 65,536 MethodDef rows, rejection at 65,537, and unchanged
32,769-row failures for old modes, the first three inputs, TypeDef, and another
fourth-slot assembly owner. Python harness **PASS**, 15 tests; scope and
whitespace checks **PASS**. The original policy-adjusted extraction retry is
**NOT RUN** pending source review and the coordinator's guarded invocation.
Windows execution, SDK/vendor implementation execution and hardware remain
**NOT RUN**. Task-009 remains **Blocked** and no service acceptance is asserted.


## Guarded construction result and remaining context boundary

The coordinator's narrow-policy retry completed **PASS** for static metadata
extraction on macOS/Darwin arm64, PowerShell 7.6.7. The private report is
`private/evidence/task009-construction/project-construction-report-task009.json`,
103,345 bytes, SHA-256
`5253681cc48e42142a7d214e9229d2d9b68c8f3580152c0ff4a63fb6129d19a2`.
A JSON-only re-read verified its bytes/hash, both denial probes true, all four
fixed input sizes/hashes/identities, five selected roots, eleven types and
`runtime_execution=NOT_RUN`. This supersedes the earlier freeze's **NOT RUN**
for static extraction; it does not supersede any unavailable runtime proof.
The normalized exact command, source hashes and parameter details are in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

SDK `ISdkContext` (`0200001F`, attributes `1048736`) is a nonpublic interface
with nil base. SDK `Root` constructor `0600026C` is internal and requires
ObjectModel `Project.Project`, `IProjectStore`, SDK `ExecutionContext` and
ObjectModel `Compatibility.IPluginInstaller`. Its `get_Projects` (`06000274`)
returns SDK `ProjectCollection`. SDK Project constructor `06001409` and
ProjectCollection constructor `06001446` are also internal and require the
observed ObjectModel project/collection and SDK context/root objects.

ObjectModel `Project.Project` declares five public constructors, `06002830`
through `06002834`, all requiring `IFactory` and `IProjectStore`. Overloads add
String/GroupAddressStyle, ProjectCollection, Guid and/or `IObjectContext`.
These are signature observations, not a qualified constructor or fixture.
All selected `DECLARATION_ONLY` return annotations are getters, indexers or an
enumerator; none proves a creation factory. No global construction-route
availability or impossibility is inferred from this selected surface.

The coordinator reports independent owned PowerShell **PASS 50 / FAIL 0**,
Python **PASS** (15 tests), scope/whitespace **PASS**, and independent static
review **PASS**. Private `task009-construction` verifier logs are
`verifier-powershell.log`, `verifier-python.log`, `verifier-scope.log` and
`verifier-diff-check.log`. Source baseline remains
`f9549f67c10b81216021fe8de5ccdf7f496cb614`; the reviewed four-input
source is committed at `6c9d03d7cb11be764d1ba572a7116ce7655c25dc`; this evidence update follows source review. The private successful-run
source record is `source-freeze-success.json`, 1,008 bytes, SHA-256
`5e621640dd851b966a69f6de3fc63a57e1819f57a715ae3024422b54145ce2c6`;
all six code hashes match the owned files. It was recorded after execution.
Older freeze files are historical pre-policy snapshots, not current source
records. Independent JSON verification also reports PASS for the report hash, guards,
inputs and selected constructor declarations. No safety check was relaxed,
and no code changes accompany this result update.

Task-009 remains **Blocked** with all service acceptance criteria unchecked.
SDK `ExecutionContext`/`ISdkFactory` and ObjectModel
`IFactory`/`IProjectStore`/`IObjectContext` are the smallest next fixed technical
inquiry named by actual declarations. Their contracts and all other dependencies
remain uninspected. No root growth, null/uninitialized object, nonpublic
activation, runtime loader expansion or service implementation occurs now.
Windows/SDK implementation execution, original initialization retry, UI/state
round trips and hardware remain **NOT RUN** for this increment.


## Fixed project-services inquiry source freeze

At reviewed baseline `e037bead9c00d116342baa561d833253adb5c615`, a separate
`project-services` metadata mode now selects only SDK `ExecutionContext` and
`ISdkFactory`, plus ObjectModel `IFactory`, `IProjectStore` and `IObjectContext`.
These names were observed in the prior construction report. Existing
construction roots, commands and report schema/content are preserved. The new
mode uses the same four exact pins and guarded route, including the reviewed
fourth ObjectModel MethodDef ceiling only; no other limit changes.

The mode reports attributes, interfaces, constructors/public/protected member
signatures and bounded local bases, with declaration-only return annotations.
No interface reference or constructor parameter is recursively catalogued, no
method body/attribute is inspected, and no service implementation, extra DLL,
SDK load or runtime allowlist expansion is supplied. The contract was written
before code in [DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

The owned PowerShell suite reports **PASS 68 / FAIL 0**, and Python harness
**PASS**, 16 tests. Tests preserve earlier reports byte-for-byte, cover exact
service roots and missing/duplicate/nonpublic-interface cases, and retain the
selected-method and row-bound failure paths. Scope/whitespace **PASS**.
Independent source verification/review and the coordinator's original guarded
`project-services` extraction are **NOT RUN** at this freeze. A selected-method
limit failure must remain FAIL pending coordinator review; it does not permit
an automatic bound or root increase. Task-009 remains **Blocked** with all
service acceptance criteria unchecked. Runtime/Windows/UI/state and hardware
proofs remain **NOT RUN** for this inquiry.


## Guarded project-services result, 2026-10-22

The coordinator's fixed service inquiry completed **PASS** for static metadata
extraction at source `5c7bc46e43f1cdd013214fce796b5b8fd6962719` on
macOS/Darwin arm64, PowerShell 7.6.7. The private report is
`private/evidence/task009-services/project-services-report-task009.json`,
54,190 bytes, SHA-256
`19194b3a2c0b4faf91ef412045291f2c3ea7dc155a72842079566ed174899cdd`.
Its JSON-only re-read verified both denial probes true, all four exact pins
and identities, five selected roots/five types and `runtime_execution=NOT_RUN`.
This supersedes the prior service-freeze **NOT RUN** and pending source review,
while runtime proof remains unavailable. The normalized guarded command and
detailed member evidence are in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

SDK `ExecutionContext` (`0200001A`, attributes `1048704`) is nonpublic/abstract;
its protected constructor `0600015C` takes ObjectModel `IDomObject` and Object.
The reader reports 20 declarations. SDK `ISdkFactory` (`02000020`, attributes
`1048736`) is a nonpublic interface with 13 declarations. ObjectModel `IFactory`
is a public interface with 52 declarations; `IObjectContext` has 25 declarations
and inherited `ICacheProvider`, while `IProjectStore` has 36 declarations and
inherited `IProductStore`. Referenced interfaces were not expanded.

`IFactory` declares two public instance `CreateProject` methods, `060001FE`
and `060001FF`, returning ObjectModel Project from store/name/style or
Guid/collection/store inputs. No concrete factory/store implementation was
qualified. Their `NOT_SELECTED_RETURN` tag only means the return is outside
the five service roots, not that creation declarations are absent. The sole
`DECLARATION_ONLY` match, `IProjectStore.Clone` (`06000315`), requires an existing
store receiver and ProjectId and does not prove a startup factory. No method
or constructor was invoked; the previously inspected SDK Root constructor remains
internal.
A compatible standalone creation/context route is still unproven.

The coordinator reports independent owned PowerShell **PASS 68 / FAIL 0**,
Python **PASS** (16 tests), scope/whitespace **PASS**, and source review **PASS**.
Private verifier logs under `task009-services` are `verifier-powershell.log`,
`verifier-python.log`, `verifier-scope.log` and `verifier-diff-check.log`.
The post-execution `source-freeze-success.json` is 876 bytes, SHA-256
`6c113b623ff78bd11d2df3985b44779cffafc850c4a923045b20928a47ef2f79`;
all six owned source hashes match its committed-source record.

This update changes only task/evidence documents. Task-009 remains **Blocked**
on concrete fixture/runtime context semantics, with all service acceptance
criteria unchecked. Remaining contract questions concern actual factory/store
provisioning, inherited `IProductStore`/`ICacheProvider` obligations and nonpublic
SDK context/factory requirements. No new mode, root, parameter catalog, service
implementation or experiment is added. Current static documentation work needs
no additional human input. Windows/SDK implementation execution, original
initialization retry, UI/state round trips and hardware remain **NOT RUN**.


## Direct provider lookup source freeze

From reviewed baseline `69a4c16c0ced39f9c95b5bd1fb2e613267893a7e`, a separate
fixed `project-providers` mode uses the same four exact pins and guarded route.
Its six targets are SDK `ExecutionContext`, `ISdkFactory`, `ISdkContext`, and
ObjectModel `IFactory`, `IProjectStore`, `IObjectContext`, all named by actual
prior metadata. It records only direct base/interface matching candidates,
their identities/tokens/attributes and exact target owner relationships.
There is no constructor/method inventory, recursive hierarchy/parameter graph,
implementation, runtime loader expansion or original execution.

The contract was written before code in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md). Match limits are 64
candidate types and 128 edges, with explicit failure rather than truncation.
Generic/unsupported edges and target-name owner mismatches remain uncertain;
indirect/external implementations are unsearched. Empty results remain
INCONCLUSIVE for creation, and positive relationships do not qualify services.
All existing pin, row, file/aggregate, selected-work, report/output and deadline
bounds remain in force.

The owned PowerShell suite reports **PASS 89 / FAIL 0** and Python harness
**PASS**, 17 tests. Fixtures exercise exact ownership and foreign/version/token
collisions, abstract/internal candidates, direct versus indirect relationships,
generic uncertainty, empty results and exact match ceilings/failures. Malformed
method signatures in matching candidates are ignored. Earlier reader reports
remain byte-for-byte identical after provider lookup; a read-only AST comparison
against the baseline verifies eleven prior pin/guard/bound/command expressions
unchanged. Scope/whitespace checks **PASS**.

Independent verification/review and the coordinator's guarded original
`project-providers` extraction are **NOT RUN** at source freeze. No matched
constructor surface is inspected before actual candidates are known. Task-009
remains **Blocked** with all service acceptance criteria unchecked. No new
runtime, Windows, initialization/UI/state or hardware proof is produced.


## Guarded provider result, 2026-10-22

The coordinator's fixed provider lookup completed **PASS** for static metadata
at source `b11c234273e3e61e0563c6474cfd694dacf674eb` on macOS/Darwin arm64,
PowerShell 7.6.7. The preserved private report is
`private/evidence/task009-providers/project-providers-report-task009.json`,
10,498 bytes, SHA-256
`3433af94b9c47ea2218b1b0b59a72bbe59ea880f8173057e0db5d01691560dd7`.
Its JSON-only re-read verified the report hash, both denial probes true, four
exact pins/identities, six targets and eighteen direct candidates/edges.
`runtime_execution=NOT_RUN`; `creation_route=INCONCLUSIVE`; 829 TypeSpec edges
remain uninspected. This supersedes the provider source-freeze's **NOT RUN**
and pending source review, without establishing a usable service.

Observed SDK candidates include internal/nonabstract `Context` (`0200000B`),
`DcaExecutionContext` (`0200000D`) and `SdkFactory` (`0200002D`), each with
attributes `1048576`. ObjectModel `Factory` (`02000023`) and `ObjectContext`
(`0200007D`) are public/nonabstract, attributes `1048577`. Their exact direct
relationships and all eighteen candidate identities/tokens/raw flags are
listed in [DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md), together
with the normalized exact guarded command.

Public/nonabstract ObjectModel store candidates are
`ZipContainerStore.ZipContainerProjectStore` (`020000BE`) and
`FileSystemStore.FileProjectStore` (`02000DCA`); internal/nonabstract
`FileSystemStore.ImportFileProjectStore` is `02000DD7`. The additional
`FileSystemStore.IFileProjectStore` (`02000DD1`, attributes `1048737`) is an
interface, not a concrete store. GOALS excludes dependence on an ETS project
store: these original type names do not approve provisioning or using vendor
storage as editor authority. No candidate constructors, methods, custom
attributes, implementation behavior or activation were inspected.

The coordinator reports independent owned PowerShell **PASS 89 / FAIL 0**,
Python **PASS** (17 tests), scope/whitespace **PASS**, and source review **PASS**.
Four verifier logs were originally written under the worktree-relative
`private/evidence/task009-providers`; unchanged copies now reside in the
canonical evidence directory, with originals preserved. This was not another
execution. The canonical `log-copy-provenance.json` is 851 bytes, SHA-256
`23783ce1091f9eb7f2e6bd84d6b16179fdc928b479cfd66a1d88c36ca5e22e6f`,
and records the four source filenames, identical hashes and copy operation.

The post-execution provider `source-freeze-success.json` is 876 bytes,
SHA-256 `f404fd0b51ef9d858456c55f722f6ace09b77db7e955228a4543668510a6d4ed`;
it records source `b11c234273e3e61e0563c6474cfd694dacf674eb`, and all six
owned code hashes match the committed files. The manifest and copy provenance
were read as JSON only; no vendor DLL was read or executed in this update.

Only result documents change now. The next minimal inquiry is constructor-only
metadata for the five observed SDK DcaExecutionContext/Context/SdkFactory and
ObjectModel Factory/ObjectContext types; it is not implemented or run here.
No unrelated store constructors, recursive catalog, original invocation or
nonpublic activation is authorized by this report. Task-009 remains **Blocked**
on a genuine fixture/context route with all service acceptance criteria
unchecked. Windows/vendor execution, original initialization, UI/state and
hardware remain **NOT RUN** for this increment.


## Fixed provider-constructor source freeze

At reviewed baseline `a110775b07dcdb9f1238abae24e54c716c2b3e12`, a separate
`project-constructors` mode selects exactly the observed SDK DcaExecutionContext,
Context, SdkFactory and ObjectModel Factory/ObjectContext types. Its contract
was written before code in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md). The mode records
only their type flags/base names and declared `.ctor`/`.cctor` signatures,
accessibility and static flags, using the same four exact pins and guarded
route. No nonconstructor member, base/interface closure, store constructor,
parameter catalog, method body or custom attribute is inspected. No bound,
prior command/report, runtime allowlist or service behavior is changed.

Owned PowerShell **PASS 112 / FAIL 0**, Python harness **PASS** (18 tests),
scope/whitespace **PASS**. Fixtures cover private, protected, public and internal constructor visibility categories,
static declarations, missing/duplicate roots, malformed constructor failure,
selected-method and table ceilings, and ignoring unrelated/member/base/store
constructor signatures. Earlier reader reports remain byte-for-byte unchanged,
and a read-only AST comparison confirms twelve previous pin/guard/bound/command
expressions identical to baseline.

Independent verification/review and the coordinator's original guarded
constructor-only extraction are **NOT RUN** at source freeze. Source/result
commits are pending. No original constructor, initializer, nonpublic activation,
SDK runtime load or device operation is attempted. Task-009 remains **Blocked**
with no service acceptance asserted; Windows, initialization/UI/state and
hardware proofs remain **NOT RUN** for this increment.


## Guarded provider-constructor result, 2026-10-22

The coordinator's constructor-only static observation PASS uses reviewed source
`1df83e9385bf7bfcc46c927756eab06a5126e06d`.
Independent verification and critical review reproduced owned PowerShell
112/112, Python 18/18, scope and whitespace PASS. Logs and the source hash
manifest are in `private/evidence/task009-constructors/verifier/`.
The exact guarded command from this task worktree was:

```sh
python3 experiments/dca-metadata/run_probe.py --stage <private-four-pin-stage> --pwsh <provisioned-powershell-7.6.7> --experiment project-constructors --output <private-four-pin-stage>/project-constructors-report-task009.json
```

The placeholders normalize private local paths; they do not select inputs or
weaken the fixed four pins. Exit status was 0 on macOS/Darwin arm64 with
PowerShell 7.6.7. Both actual restriction probes report true: filesystem writes
and network access were denied before the static read. The private result
`private/evidence/task009-constructors/project-constructors-report-task009.json`
is 5,969 bytes, SHA-256
`df361cb84fc232b6f7da6943114096cd3e14b638495dcc8b68e10dbd42524ea9`.
The post-run `source-freeze-success.json` records the exact source commit and
six owned source hashes. All four pinned original input identities match.

Exactly five types each declare one instance constructor and one private static
initializer. Context and DcaExecutionContext instance constructors are internal;
SdkFactory's is public although its declaring type is internal. ObjectModel
Factory and ObjectContext types and instance constructors are public. Observed
instance signatures, with owner versions retained in the private JSON:

| Type | Instance constructor arguments |
|---|---|
| SDK Context | SDK Root, ObjectModel IProjectStore, ObjectModel Project, SDK ExecutionContext, ObjectModel IPluginInstaller, Autofac IComponentContext |
| SDK DcaExecutionContext | Func of IEnumerable of ObjectModel Device, Object, String |
| SDK SdkFactory | None |
| ObjectModel Factory | Autofac IComponentContext, Knx.Kim.DataProvider IKimDataProvider |
| ObjectModel ObjectContext | ObjectModel Project, IFactory, IProjectStore |

Autofac is referenced at version 8.2.0.0; Knx.Kim.DataProvider at 1.0.0.0.
These are signature dependencies, not provisioned runtime components or working
services. No constructor or static initializer was invoked, and no null-backed
or uninitialized Project was supplied. No original store is approved as editor
authority. The earlier Root constructor declaration already accepts an
ObjectModel Project, IProjectStore, ExecutionContext and IPluginInstaller; a
controlled fixture route must resolve those arguments and its access before a
Windows construction experiment. Factory method bodies and behavior remain
unknown. Further metadata catalogs do not substitute for that experiment.

Task-009 remains Blocked; creation_route is INCONCLUSIVE and runtime_execution
is NOT_RUN. This supersedes only the constructor source-freeze's pending static
extraction, verification and review. Windows hosting, initialization/UI/state,
hardware, SDK runtime allowlist changes and nonpublic activation remain NOT RUN
for this increment.

## Public Project constructor diagnostic: first source freeze

The source baseline is `9a4e283c98809c4e91bd89dd495f74166c2692df`.
The approved bounded diagnostic and normalized commands are recorded in
[DCA-PROJECT-CONTEXT](../docs/DCA-PROJECT-CONTEXT.md#public-project-constructor-diagnostic-first-source-freeze).
This increment adds a separate fixed Windows tool and owned fixtures, with
bounded integration into the existing build, launcher and test driver. Earlier
metadata modes and the eight-input DCA constructor/Initialize behavior remain
unchanged. No source ETS host execution, SDK Root activation, service success
response or dependency expansion beyond the exact qualified ObjectModel pin
is supplied.

The only enabled original stage is reflection-only GroupAddressStyle metadata
inspection from the single exact Common.Types input. No enum literal is guessed.
The original Project constructor mode is source-frozen off and fails
`ENUM_LITERAL_NOT_REVIEWED` before input pinning or any original load. An actual
enum report must be reviewed, followed by a separately frozen literal identity,
token/value/report hash, build, owned tests and independent review before the
public constructor can be invoked. If later enabled, every IFactory and
IProjectStore call still throws `PROJECT_FIXTURE_UNSUPPORTED`; constructor
return remains unqualified, including a caught denial. This is an investigation
of the first actual required service, not a working fixture implementation.

Local macOS C#5 syntax checks PASS (seven sources), PowerShell parsing PASS
(two scripts), and the new owned fixture compiles locally with signed enum
constants verified. Local Roslyn C#5/x64/warnings-as-errors in-memory compilation
against the exact six pinned net48 framework references PASS for all four owned
assembly groups. This does not replace the pinned Windows compiler or execution.
Windows net48 build, owned RealProxy regressions, original
enum inspection and original Project constructor invocation are **NOT RUN** at
this source freeze. The coordinator owns Windows staging/execution after source
freeze and sequential independent verification/review. The existing supervisor
has bounded direct-child termination; descendants and network are not contained.
Task-009 remains **Blocked** with all service acceptance criteria unchecked;
creation_route is **INCONCLUSIVE**, and hardware/UI/state/DCA initialization are
**NOT RUN** for this increment. Source/result hashes and actual Windows results
must be appended only after they exist.

## First enum result and fixed facade follow-up

The coordinator's actual clean-Windows metadata-only enum01 run used source
`61c768d1785c723c94985ac36e952601ed38f0b6` and is **FAIL**, exit 2. Prior owned
Windows tests passed 10 Session-0 and 17 interactive cases. The exact Common.Types
file passed its hash and reflection-only audit; the tool then denied
`netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51`.
No literal, enum underlying type or original constructor was observed. The
value remains UNKNOWN and original construction remains DISABLED_NOT_RUN.

The actual evidence and log/end-record hashes are in
[the component contract](../docs/DCA-PROJECT-CONTEXT.md#enum-preflight-result-and-fixed-facade-follow-up-2026-10-22)
and private `task009-constructor-windows/enum01*` artifacts. This follow-up admits
only the exact previously pinned netstandard facade to the reflection-only
enum resolver, after both fixed input hashes and identities pass. Missing,
extra or wrong-hash inputs fail; every other dependency still fails. Neither
the prior eight-input route nor the disabled nine-input constructor pinset is
expanded. New owned source guards test ordering and the fixed resolver; the
negative staged files are owned fixtures only.

Fresh source02/build02/inputs-enum02 staging, Windows owned re-verification,
independent review and an enum02 retry remain pending. Original Project
invocation remains source-disabled, not merely unattempted. Task-009 remains
**Blocked**, creation_route is **INCONCLUSIVE**, and no acceptance criterion
or gate is satisfied by this diagnostic correction.

Follow-up owned local verification PASS: Roslyn C#5/x64/warnings-as-errors
compilation against the six exact pinned net48 framework references, PowerShell
parsing, the checked-in source-order/fixed-resolver guard block, unchanged
nine-pin/other-source comparison, repository scope and whitespace checks. These
do not replace the pending clean-Windows build/tests or enum02 evidence. The
two changed tool hashes are recorded in the component contract; source commit
and actual enum02 results remain pending.


## First Windows diagnostic freeze results, 2026-10-22

The coordinator transferred the ten owned tool/host sources recorded in
`private/evidence/task009-constructor-windows/source-freeze-01.json` to the
fresh clean-host stage. All staged source hashes match the frozen worktree.
The pinned Windows C#5/x64/net48 build PASS uses compiler 4.8.9221.0,
SHA-256 `46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`,
and the six existing fixed framework-reference pins. The raw `build01-report.json`
records eight compiled source hashes and twelve output/config/sidecar artifacts;
the two PowerShell tool hashes are also in the full source-freeze record.
No SDK/DCA assembly was executed by the build.

| Built owned executable | SHA-256 |
|---|---|
| ProjectConstructorProbe.exe | `1951c8f91b12ea2b2c356361f9b81e56ecb3ad2685bc6b3bbe2f9ca9e3426dcd` |
| ProbeLauncher.exe | `49873f7bfd79183d7cef7bb118d681fc8265f8f93fec0a40d9d4b7f144040c13` |
| ProjectConstructorProbe.exe.config | `b72d33cb400c6a9de0565ece3698308c569836dc41f9003d70d9d78a31b4315a` |

The coordinator ran the stated owned test driver with fresh output directories:
Session0 PASS 10/10 cases; limited interactive session 2 PASS 17/17 cases.
These are two suites, not 27 distinct assertions. The interactive context
records UserInteractive=true and administrator_role=false. Both test processes
exit 0. The first-freeze original-project dispatch records
ENUM_LITERAL_NOT_REVIEWED with no PIN, assembly or constructor marker.
Inherited/generic/ref-out/void/property denials, saturated counters, enum
metadata negatives, once-only constructor behavior and caught/request-free
unqualified returns pass in the owned proxy case. Prior constructor/Initialize,
timeout/output, hash/path and junction tests remain passing.

Canonical evidence includes `owned-session0-01-stdout.txt`,
`owned-interactive01-context.json`, `owned-interactive01-done.json`, the raw
PowerShell driver streams and selected per-case logs. PowerShell's raw driver
stdout is UTF-16LE; unchanged raw bytes and strictly decoded UTF-8 derivatives
are linked by `owned-interactive01-log-encoding.json`. No test rerun is inferred
from this decoding. Driver stderr is empty. The fresh owned scheduled task was
removed after completion; cleanup records zero active owned tool processes,
LastTaskResult=0 and no remaining test junction. Descendants and network access
are not contained by this supervisor.

Independent verification PASS audited all ten source hashes, source bounds and
stage guards, raw/normalized log hashes, actual session/result evidence and
build-report/runner preflight consistency for all twelve artifacts. The verifier
did not independently rehash target binaries or rerun Windows execution.
Its source/report/artifact audit, scope and whitespace logs are under
`private/evidence/task009-constructor-windows/verifier/`. This is an independent
audit of the coordinator's Windows runs, not another Windows run.

This supersedes the first source-freeze's Windows build/owned-tests NOT RUN.
Original enum inspection and Project constructor invocation remain NOT RUN.
OriginalConstructorReady remains false. No working Project fixture, DCA
initialization, UI/state proof, hardware result or gate acceptance is asserted.
Task-009 remains Blocked. Critical source/result review precedes any original
enum metadata inspection; an observed literal and second reviewed source freeze
remain prerequisites for the original constructor diagnostic.

## Corrected facade-route Windows owned results

The coordinator's corrected Windows C#5/x64/net48 build **PASS** retains compiler
4.8.9221.0 and its exact prior hash. Actual owned suites **PASS** 11/11 Session0
and 19/19 interactive cases, including the fixed two-hash source guard and
missing-facade case. Interactive context is session 2, UserInteractive=true,
administrator_role=false; the end record reports exit 0, OWNED_TESTS_ONLY and
original/vendor execution NOT_RUN. The first generation's 10/17 suites and
enum01 FAIL exit 2 remain separate evidence, not overwritten results.

Canonical private evidence is `task009-constructor-windows/source-freeze-02.json`,
`build02-report.json`, `owned-session0-02-stdout.txt`,
`owned-interactive02-driver.stdout.txt.utf8`, its context/end records and
`owned-interactive02-log-encoding.json`. The component contract records their
hashes and the built ProjectConstructorProbe.exe hash
`7447540d8ec318ab1ab2b8c36b2ab6bc9fd4b2ef7ab291b083868af0b3f166b5`.
Local read-only checks PASS for all ten frozen source hashes, suite counts and
strict raw/decoded stream provenance. These audit the coordinator's actual
owned runs; they are not an independent Windows rerun. Detailed case logs,
cleanup and independent follow-up review are recorded separately when available.

This supersedes the follow-up freeze's Windows build/owned-tests NOT RUN only.
Enum02 remains **NOT RUN**, GroupAddressStyle remains **UNKNOWN**, and original
Project construction remains source-disabled and **NOT RUN**. Task-009 stays
**Blocked**; no working Project, DCA context, initialization/UI/state, hardware
or gate acceptance follows. The correction's source commit and original enum02
result remain pending.

Coordinator follow-up: owned case logs and cleanup are now preserved. The
cleanup record reports LastTaskResult=0, task removed, zero owned processes
and no junction. Independent verification and critical source/result review
PASS; their audit records are under `private/evidence/task009-constructor-windows/`
`verifier02/independent-audit.log` and `review02/critical-review-source-result.txt`.
These are evidence audits, not independent Windows reruns or target-binary
rehashes. The reviewed facade-only correction may proceed to a fixed enum-only
metadata retry; original construction remains disabled.

## Actual enum02 observation

The coordinator's clean-Windows enum02 reflection-only metadata run **PASS**,
exit 0, at source `41b5f6927c8b17d2d7781e4f2955595e09904250`. Both exact
Common.Types/netstandard inputs passed their pins; both assemblies and the
facade resolution are recorded in reflection-only context. Session 2 is
interactive, administrator_role=false. GroupAddressStyle has type token
`0200008E`, underlying `System.Byte`, four declared fields, and literals
TwoLevel `04000175`=0, ThreeLevel `04000176`=1 and Free `04000177`=2.
The selected next diagnostic value is **ThreeLevel=1**, not a working Project
configuration or qualified service response.

The bounded deterministic private `task009-constructor-windows/enum02-observation.json`
is 4,062 bytes, SHA-256
`59323bf7eb53d6c6a130c523e49d82bb3e3d58830f43dcc2f13b2dcabb58bcf7`.
It records source/input pins, exact literals, selected diagnostic input and
eight-file provenance including context/end/supervision/cleanup. The raw log
is 2,912 bytes, SHA-256
`c78cf325972616b3bfe3730fd8c16f8765ff6181e995ac731ebb69498684d8f0`.
The component contract records the plan, summary and cleanup hashes. Cleanup
reports LastTaskResult=0, task removed, zero owned processes and scratch files;
the supervisor does not establish systemwide network or descendant containment.

This actual result supersedes enum02 NOT RUN and the unknown literal; enum01
FAIL and both owned-test generations remain preserved. The source still has
OriginalConstructorReady=false, and no activation edit occurs in this result
increment. Original Project invocation remains **DISABLED_NOT_RUN**; Project
fixture qualification is **NOT_QUALIFIED**, creation_route **INCONCLUSIVE**,
DCA initialization/UI/state and hardware **NOT RUN**. Task-009 remains
**Blocked** with all service acceptance criteria unchecked. Independent
verification/review of this observation is required before a separate source
freeze can enable the already bounded constructor diagnostic.

Independent enum02 verification PASS (58 assertions) and critical observation
review PASS. Records: `private/evidence/task009-constructor-windows/`
`verifier-enum02/independent-audit.log` and
`review-enum02/critical-review-observation-result.txt`. Review confirms only the
observed ThreeLevel=1 literal and provenance as input to a separate reviewed
source freeze; original constructor dispatch remains disabled. Neither reviewer
nor verifier reran Windows or read original DLLs. Task-009 remains Blocked.

## Second constructor-diagnostic source freeze

At baseline `4ad5b523194103b51ea1ab400566bcc10f7424fc`, the enum02 observation
has independent verification 58 PASS and critical review PASS. This separate
source freeze pins its exact ThreeLevel/04000176/raw 1, enum 0200008E/System.Byte
and report SHA-256, then sets readiness true for only the existing nine-pin
public Project constructor diagnostic. Its public token/signature, deny-all
proxies, loader policy and bounds are unchanged. No fixture success, private
activation, new dependency or DCA operation is implemented.

The owned driver checks source-pinned literal/report constants and nine-hash
ordering, original dispatch Session-0/admin rejection, and missing-nine/exact-nine
wrong-hash rejection using only renamed owned bytes. Prior proxy/enum regressions
remain. Local pinned-reference C#5 compilation, PowerShell parse, source guards
and unchanged pinset/other-source comparison PASS. Changed tool hashes are in
the component contract. New Windows build/owned tests, independent source review
and original constructor invocation remain **NOT RUN** pending coordinator
execution after freeze. No source commit is claimed yet. Task-009 stays
**Blocked**; creation_route is INCONCLUSIVE, no Project fixture is qualified,
and DCA initialization/UI/state/hardware remain NOT RUN for this increment.

### Second-freeze Windows owned verification

The pinned Windows build03 PASS with the unchanged compiler and six framework
reference pins. Owned suites PASS: 11/11 Session0 and 20/20 interactive cases
(two suites, not 31 distinct assertions). Session 2 is interactive, non-admin,
and exits 0. Exact-nine wrong-hash and missing-input cases stop before original
identity/load/constructor phases. Cleanup reports task removed, zero owned
processes and no junction. Original execution and hardware remain NOT RUN.

Private `task009-constructor-windows/source-freeze-03.json` SHA-256
`7e6ce5e837d9b84a051461ef80ef94aa7b7de6ef78a824f161afddcd5c0afe84`
records ten source hashes. `build03-report.json` SHA-256
`b0960fafe4c1747314dcb9dd5700fb131ec7c4b75928416003b69c1f8c42b598`
records eight compiled source pins and twelve artifacts. ProjectConstructorProbe.exe
is 52,224 bytes, SHA-256 `db5ac98023c39e4725b4a9543f2fcf36d0393fb337dffe239c805d86b875ab30`.
The same private directory preserves `owned-session0-03-stdout.txt`,
`owned-interactive03-context.json`, its end record, raw driver streams and
strict UTF-16-to-UTF-8 provenance in `owned-interactive03-log-encoding.json`,
case logs and `owned-interactive03-cleanup-stdout.txt`. This supersedes only
the second freeze's Windows build/owned-tests NOT RUN. Independent source/result
verification and review precede any single original constructor diagnostic;
Task-009 remains Blocked, creation route INCONCLUSIVE and no fixture qualifies.

An additional owned-only elevated interactive suite PASS, 16/16 cases,
including original-project-admin rejection with exit 5 before original input
inspection. Its context confirms session 2, interactive=true, administrator
role=true. This closes the admin-branch runtime coverage gap; the Session0
suite had exited on session identity first. Private `owned-admin03-*` records,
raw/decoded provenance and case logs preserve this separate run. Cleanup
reports exit 0, task removed, zero owned processes and no junction. Original
SDK/manufacturer execution and hardware remain NOT RUN in all owned suites.
Session0 stderr retains PowerShell CLIXML preparation progress; it is not
classified as empty or a failing test result.

Independent source/result verification and critical review PASS for the second
freeze; records are `private/evidence/task009-constructor-windows/`
`verifier03/independent-audit.log` and `review03/critical-review-source-result.txt`.
The fixed single 15-second public constructor diagnostic may now follow its
source commit. A constructor-begin marker proves a reflection invocation attempt,
not body entry; return does not qualify a Project fixture. Any dependency or
constructor failure stops this attempt without pin widening or automatic retry.
Original invocation remains NOT RUN at this reviewed source commit.

## Project03 runtime diagnostic result

Source `b8fcc21ef126a4d00f8f4c6c65101f23cd4b1358`: actual project03 **FAIL**,
exit 2, interactive session 2, non-admin. Nine exact pins PASS; original
ObjectModel/Common.Types runtime assembly loads are observed. Type materialization
requests `FluentResults, Version=3.16.0.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`; the fixed resolver denies it and reports
PROJECT_DEPENDENCY_BLOCKER. No constructor-begin or proxy request occurs:
invocation **NOT_REACHED**, body entry **NOT_ESTABLISHED**. This is a failed
runtime diagnostic, not an all-runtime NOT RUN claim.

Private `task009-constructor-windows/project03-observation.json`, 5,030 bytes,
SHA-256 `f55f82bdea5ee8edf7a30ce5d9cda8c96930a0f6d4833d2183d431be93334486`,
binds the raw log and eight-file provenance. Child exit 2 is confirmed; cleanup
records expected/last exit 2, task removed and zero owned processes/scratch files.
Task-009 remains **Blocked**, creation_route INCONCLUSIVE, fixture NOT_QUALIFIED;
DCA initialization/UI/state/hardware remain NOT RUN. No source change, pin
expansion or retry occurs. A future dependency inquiry is a separate increment.

Independent project03 evidence audit PASS (17 assertions) and critical result
review PASS; the experiment itself remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project03/independent-audit.log`
and `review-project03/critical-review-result.txt` under the same directory.
They verify retained evidence without rerunning Windows or reading DLLs; neither
review changes the pinset or establishes constructor/fixture/DCA qualification.

## Controlled FluentResults source freeze

Baseline `8d5482406b7d7c20720b9c44dcebad1069c6113f` preserves reviewed project03
FAIL before constructor. Private identity/hash inventory and guarded static
reference observation qualify only the exact observed FluentResults file for
the next diagnostic pin: 139,776 bytes, SHA-256
`99adb97bc209d19696c9a42db8eceef49b55fc2c3f4cc6d6f5654fe7e5df9159`,
version 3.16.0.0, neutral, token 61439873ec5e1159. The fixed profile now requires
ten inputs, all hashes before identity/load; every other dependency stays denied.
Logging is not added/redirected and no config or old eight-input route changes.

Owned source guards/negative sets now require ten inputs, with owned bytes only.
Local C#5 compile, PowerShell parse, source guards and unchanged prior-nine-pin/
seven-other-source checks PASS. New Windows build/tests, source review/commit
and original ten-input attempt remain **NOT RUN** pending coordinator handling.
The component contract records provenance and tool hashes. Task-009 stays
**Blocked**, creation route INCONCLUSIVE; no fixture/DCA/hardware qualification.

### Ten-input profile Windows owned verification

Pinned Windows build04 PASS. Owned suites PASS: 11/11 Session0, 20/20
limited interactive and 16/16 elevated interactive (separate suites). Both
interactive contexts are session 2; limited/admin roles are confirmed separately.
Missing-ten, exact-ten wrong-hash and original-project-admin cases stop before
original input identity/load/invocation. Both scheduled tasks were removed,
with zero owned processes and no junctions. Original execution remains NOT RUN
in these owned suites; the ten-input runtime attempt is still NOT RUN.

Private `task009-constructor-windows/source-freeze-04.json` SHA-256
`105f5b6a5e4939b0b132f43271efd011d5ffa081585709185f8ad6f10bd8e315`
and `build04-report.json` SHA-256
`405345fb701d52f9f353a4792c4a36f5b130843fbf4046104fc554adc62436a4`
bind the ten owned sources, unchanged compiler/framework pins and twelve built
artifacts. ProjectConstructorProbe.exe is 52,736 bytes, SHA-256
`965239f941a08dce5101aa8e11910e61164355c7b5fdb9b7f886687d60c55b18`. The same private directory retains
`owned-session0-04-*`, `owned-interactive04-*` and `owned-admin04-*`: raw
streams, strict UTF-16/UTF-8 provenance, contexts/end records, case logs and
cleanup. This supersedes only the ten-input freeze's Windows build/owned-tests
NOT RUN. Independent source/result verification and review must precede its
source commit and single original attempt. Project03 FAIL is preserved;
Task-009 remains Blocked, with no Project fixture or DCA qualification.

Independent source04 verification and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier04/independent-audit.log`
and `review04/critical-review-source-result.txt` under the same directory.
Only the exact observed FluentResults dependency is added; Logging remains
excluded. A single fixed 15-second ten-input attempt follows the source commit;
any next dependency or constructor failure stops without automatic widening or
retry. The new original attempt remains NOT RUN at this reviewed source commit.

## Project04 runtime diagnostic result

Source `d4af5da8af23ce6039042b935ab310b2d6cf445f`: single project04 **FAIL**,
exit 2, non-admin interactive session 2; ten pins PASS. ObjectModel/Common.Types/
FluentResults runtime loads occur. Runtime netstandard 2.0.0.0/token cc7b13ffcd2ddd51
is audited allowed=False (after-load detection, prevention NOT_ESTABLISHED);
its loaded path/hash and actual origin are not logged. The resolver then denies
Knx.Ets.Common 6.4.8718.0/neutral/token 61439873ec5e1159. Both are blockers.
No constructor-begin/proxy-request marker: invocation NOT_REACHED, body entry
NOT_ESTABLISHED. Runtime loads are observed, not all NOT RUN.

Private `project04-observation.json`, 5,805 bytes, SHA-256
`81f8db7297802cbf222c4c9db85d3e827e3d4b4ad2d70e29051325bc64f5d1b4`,
binds raw log and eight-file provenance. Child exit 2 is confirmed; cleanup has
expected/last exit 2, task removed, zero owned processes/scratch files. Earlier
project03 FAIL and owned 11/20/16 PASS remain. No source/pin/config change or
retry; Task-009 **Blocked**, creation INCONCLUSIVE, fixture NOT_QUALIFIED,
DCA initialization/UI/state/hardware NOT RUN. Separate inquiries are not part
of this attempt and do not qualify the loaded facade's origin or expand runtime.

Independent project04 evidence audit PASS (20 assertions) and critical result
review PASS; the recorded experiment remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project04/independent-audit.log`
and `review-project04/critical-review-result.txt` under the same directory.
Neither audit reran Windows or changes the unknown loaded origin, pinset,
constructor-not-reached outcome or closed fixture/DCA qualification.

## Profile11 and fixed OS facade source freeze

Baseline `ad8f2e84b5dc1e5932bc9e89384bebdee9ac7c6c` preserves project04 FAIL.
Only exact Common 6.4.8718.0/neutral/token 61439873ec5e1159 is private input eleven,
503,248 bytes, SHA-256
`4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd`.
All eleven hashes precede a fixed OS netstandard length/hash/identity preflight
with held read handle, then original identity/load. The runtime-only alias also
requires exact GAC path/identity, cache=true and ReflectionOnly=false. Its source
is a separate Microsoft-only observation; it does not identify project04 origin.
No new transitive pins, redirects, generic whitelist or caller proof is added.

Owned tests cover incomplete pin, wrong facade identity/path/cache/reflection,
actual Microsoft facade load, and missing-eleven/exact-eleven wrong-hash failure
before OS pinning, with owned bytes only. Prior proxy/enum/session/admin cases
remain. Local pinned-reference C#5 compile, PS parse, source/ownership comparisons
and guards PASS; seven other sources, old ten pins, enum/config/resolvers remain
unchanged. The component contract records exact provenance/tool hashes and local
reproduction. Windows generation05 results are separate; original eleven-input
attempt NOT RUN. Task-009 **Blocked**, creation INCONCLUSIVE; no service/gate or
fixture/DCA/hardware qualification. New source commit and review remain pending.

Generation05 actual Windows build PASS with existing pinned compiler/frameworks;
owned suites PASS 11/11 Session0, 20/20 limited and 16/16 elevated interactive.
The two interactive contexts are session 2, expected admin roles and exit 0.
Exact alias/incomplete-pin/negative tests pass with Microsoft/owned inputs only;
vendor execution and hardware remain NOT_RUN. The component contract records
freeze/build/artifact and 8,432-byte facade-log hashes. Raw/decoded provenance,
53 limited and 41 elevated case files, task removal/zero-process/no-junction
cleanup are preserved privately. Initial directory precheck and early collection
observations were corrected before build/completion collection, not failed tests
or test reruns. Source/result independent verification/review and source commit
remain pending before original generation05 invocation, which is NOT RUN.

Independent source05/profile11 verification and critical review PASS. Records
are `private/evidence/task009-constructor-windows/verifier05/independent-audit.log`
and `review05/critical-review-source-result.txt`. The verifier retained its
original report and appended a test-source hash transcription correction;
corrected audit SHA-256 is
`05bcc1bcc483f8291d2eae6e46eb00de6c5dd61fcc9095c800c2623b78b58002`.
Review also corrected the component wording to distinguish the unchanged
framework allowlist from the new exact facade alias. Neither correction changes
source bytes or test runs. These audits do not rerun Windows or rehash target
binaries. After the explicit source commit, one fixed 15-second original
diagnostic may follow; any failure stops without automatic pin expansion or
retry. Original generation05 remains NOT RUN at this source commit. Task-009
remains Blocked, without Project fixture, DCA, hardware or gate qualification.

## Project05 reflection invocation result

At source `1b0eb7dcb9158778ae4ce3e40c554f2956f800c4`, project05 **FAIL**, exit 2,
non-admin interactive session 2. Eleven pins/fixed OS alias PASS; actual alias
path/held hash and Common runtime load are logged, with no unexpected-assembly
marker. This proves project05's alias only, not project04's unknown origin.
Public constructor token 06002830 has a begin marker: invocation
**ATTEMPTED_NOT_RETURNED**, body entry **NOT_ESTABLISHED** (JIT/type initialization
may fail first). Exact Knx.Ets.Ids 6.4.8718.0/neutral/token 61439873ec5e1159 is denied;
END returned=False, denied-requests=0, final PROJECT_DEPENDENCY_BLOCKER.

Private `project05-observation.json`, 6,783 bytes, SHA-256
`dd1dfc6bdc08ba6a61279273045f7f9deb8943183496be43ebb6f85ee0e27955`,
binds raw log/eight-file provenance. Child exit 2 and expected/last cleanup exit 2,
task removal and zero owned processes/scratch are confirmed. No source/pin change
or retry; result verification/review pending. Task-009 **Blocked**, creation
INCONCLUSIVE, fixture NOT_QUALIFIED; DCA initialization/UI/state/hardware NOT RUN.

Independent project05 result audit and critical review PASS; the experiment
remains FAIL, exit 2. Records are
`private/evidence/task009-constructor-windows/verifier-project05/independent-audit.log`
and `review-project05/critical-review-result.txt`. They reproduce source/report/
provenance consistency and scope/whitespace checks without rerunning Windows
or reading original DLLs. No source change, runtime widening, retry, fixture
qualification or phase approval follows from this result review.

## Profile12 Ids source freeze

Baseline `167740ea8e724b6f69bacfcf75c10a9fa72bab06` preserves reviewed project05
FAIL. Only exact Ids 6.4.8718.0/neutral/token 61439873ec5e1159 is appended as pin
twelve, 318,928 bytes, SHA-256
`43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877`.
Private inventory and guarded static-reference evidence are recorded in the
component contract; direct references are only three existing framework names.
All twelve hashes precede unchanged OS facade preflight and original identity/load.

Owned negatives use eleven renamed owned files, then the twelfth for wrong hash,
and stop before OS pinning. Prior cases and exact Microsoft facade proof remain.
Local pinned-reference C#5 compile, PS parse/guards, first-eleven/other-seven-source
and whole non-pin-method comparisons PASS. Contract records helper/tool hashes.
Generation06 Windows build/tests, source review/commit and original twelve-input
attempt remain NOT RUN at freeze. No new service/redirect/activation; Task-009
**Blocked**, creation INCONCLUSIVE, fixture/DCA/hardware NOT_QUALIFIED/NOT_RUN.

Generation06 actual Windows pinned build and owned suites PASS 11/20/16,
session 2 expected limited/elevated roles and exit 0. Existing exact facade proof
and negatives pass. Private source-freeze06/build06/artifact/log hashes are in
the component contract; raw/decoded provenance and cleanup inventories retain
53 limited/41 elevated case files, task removal and zero processes/junctions.
No setup failure, early collection or rerun occurred in generation06. Original
twelve-input execution remains NOT RUN; verification/review/source commit are
pending. Task-009 stays Blocked, without fixture/DCA/hardware/gate qualification.

Independent source06/profile12 audit and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier06/independent-audit.log`
and `review06/critical-review-source-result.txt`. The twelve-name wrong-hash
owned set fails at its first private pin; it is not an isolated twelfth-input
hash test. Source/report and Windows evidence audits do not rerun Windows or
rehash target binaries. After the explicit source commit, one fixed 15-second
original diagnostic may follow; failures stop without automatic expansion or
retry. Original twelve-input execution remains NOT RUN at this source commit;
Task-009 stays Blocked and no fixture, DCA, hardware or gate qualifies.

## Project06 observed service-request result

At `de5d65056510b240deb93971313bca9f164b7b40`, project06 **FAIL**, exit 10:
twelve pins/OS facade PASS, Ids runtime load allowed, no dependency/audit blocker.
After public constructor token 06002830 begin, SDK request **OBSERVED**:
IProjectStore.get_Id, 060002F5, zero arguments, first request. Explicit unsupported
denial yields TargetInvocationException → NotSupportedException; END false,
denied-requests=1. Invocation ATTEMPTED_NOT_RETURNED, body entry
**NOT_ESTABLISHED_BY_STACK_TRACE**; no constructor/initializer origin is inferred.

Private `project06-observation.json`, 7,782 bytes, SHA-256
`0b6dbcc79c3f280b75137f0824982ab692fbd8f73fa6e03efc7a452e72cf474f`,
binds raw log and eight-file provenance. Non-admin session 2, child exit 10 and
expected/last cleanup 10/removal/zero processes/scratch are confirmed. No value,
fixture/service implementation, source/pin change or retry follows. Task-009
**Blocked**, creation INCONCLUSIVE, fixture NOT_QUALIFIED; DCA initialization/
UI/state/hardware NOT RUN. Result verification/review pending; earlier failures
and project04's unknown origin remain intact.

Independent project06 result audit and critical review PASS. Records are
`private/evidence/task009-constructor-windows/verifier-project06/independent-audit.log`
and `review-project06/critical-review-result.txt`. These consistency audits
retain the diagnostic FAIL exit 10 and observed unsupported service request;
they do not establish constructor body/caller provenance or supply a getter
value. No Windows rerun, original DLL read, source change, retry, fixture
qualification or phase approval follows. ProjectId contract investigation
remains a separate prerequisite before any positive fixture implementation.

## Fixed ProjectId metadata source increment

After reviewed project06 result commit
`28398f0de002c6bef613325dc3431fe48e7cb656`, add only a separate `project-id`
metadata experiment for `Knx.Ets.Ids.Project.ProjectId`. The fixed one-file
Ids pin is 318,928 bytes, SHA-256
`43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877`,
6.4.8718.0/neutral/PKT61439873ec5e1159. The original reader checks this hash
before PEReader and metadata identity before root selection. A separate unsigned
owned reader and fixture identity cannot impersonate the fixed original input.
No original ProjectId bytes were read for this increment; declarations remain
UNKNOWN until the coordinator's later reviewed guarded extraction.

The [fixed contract and six source hashes](../docs/DCA-PROJECT-CONTEXT.md)
record declared constructors/public-protected methods and fields, flags,
bounded local bases and an explicit TypeSpec ancestry stop. No values, method
bodies, custom attributes, dependency loading or service implementation are added.
All prior pins, modes, reader reports and constructor tools are preserved.

Commands: provisioned PowerShell 7.6.7 runs
`-NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1`
(**PASS 151 / FAIL 0**); `python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py`
(**PASS 19**); `python3 -B scripts/check_project_scope.py` and
`git diff --check` (**PASS**). Tests are synthetic metadata/owned process fixtures
on macOS, including field decoding/selection/output bounds and unchanged reports;
they are not original parsing or Windows runtime evidence. Independent source
verification/review and commit are pending. Original ProjectId extraction and
new Windows/fixture/DCA initialization/UI/state/hardware execution are **NOT RUN**.
Task-009 remains Blocked; the observed get_Id request receives no guessed value.

Independent fixed ProjectId metadata-source verification and critical review
PASS. Records are `private/evidence/task009-project-id-metadata/verifier/` and
`review/critical-review-source-result.txt`. Both independently reran the owned
PowerShell 151-case and Python 19-test suites, scope and whitespace checks;
legacy modes and reports remain preserved. The reviewer additionally checked
malformed method handles and the report's escaped JSON size bound. These
checks do not read original DLL bytes or establish the ProjectId API. After
the explicit source commit, one fixed guarded metadata-only extraction may
follow. Runtime constructors, getter values, fixtures and DCA/hardware/gate
qualification remain outside this source increment; Task-009 stays Blocked.

## Actual fixed ProjectId metadata result

At `a2976171e6bd571243c8f1a0f3ab88bbf9674911`, one guarded extraction
**PASS**, exit 0, exact Ids pin and six source hashes unchanged; PowerShell
7.6.7 write/network denial probes true, target runtime **NOT RUN**. The prior
inside-Git/0755 setup was rejected `DCAPROBE_PRIVATE_STAGE`, exit 1, before
parsing (**NOT RUN**); corrected outside-Git 0700 stage/0600 input/absolute
immediate output follows existing rules. Both records remain; no automatic retry.
Private project-id-report.json is 8,834 bytes, SHA-256
`37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4`;
project-id-observation.json is 7,734 bytes, SHA-256
`89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a`.
Four type declarations show ProjectId `0200002C` -> HexNumberId -> NumberId ->
Id -> unresolved external System.Object. Public `.ctor(UInt32)` `060001EF`,
`.ctor(StringSection)` `060001EE` and UInt32 getter `060001F2` are declaration
facts; generators likewise were not invoked. No numeric value is chosen,
constructed or supplied to get_Id. See the [result/provenance and normalized command](../docs/DCA-PROJECT-CONTEXT.md).
Result audit/review and a separate runtime fixture experiment remain pending;
Task-009 stays Blocked, project06 FAIL retained, no fixture/DCA/UI/state/hardware
or gate qualification. Scope/diff checks PASS; source tools remain unchanged.

Independent actual ProjectId metadata-result audit and critical review PASS.
Records are `private/evidence/task009-project-id-metadata/verifier/result-audit.txt`
and `review/critical-review-result.txt`. They verify all eleven provenance
artifacts, six source hashes, exact declaration tokens, denial probes and
staging chronology without reading original DLL bytes or rerunning extraction.
Only PRIVATE_STAGE is the observed first rejection; later setup corrections
satisfy unchanged guards. Public constructor/getter declarations do not qualify
behavior or approve a future runtime fixture. Task-009 stays Blocked; the
project06 FAIL and absence of DCA/hardware/gate qualification remain preserved.

## Id-only offline fixture source increment

After reviewed metadata result `91121b184043e32db930fd596e751a53ee69751a`,
add only explicit `original-project-store-id` / `owned-project-store-id` modes.
The unchanged twelve-pin original route validates reviewed ProjectId public
UInt32 constructor/numeric getter and exact IProjectStore.get_Id owner/signature/
tokens before attempting one constructor and getter. Fixed UInt32 `1` is owned
synthetic fixture data, not a real/manufacturer identity or claimed-valid value.
Only exact type plus exact numeric getter validation enables a bounded proxy
returning that same object for get_Id. All other requests and saturation are
unsupported; positive/denied counters are separate. Old deny-all behavior remains.

The [contract/source hashes](../docs/DCA-PROJECT-CONTEXT.md) record limits and
owned regressions. Private `knx-task009-store-id-sourcecheck.ps1` with provisioned
PowerShell 7.6.7, `-Worktree WORKTREE -FrameworkReferences PRIVATE_EVIDENCE/task148-clean-host/references`,
**PASS**: six pinned net48 references, C#5/x64/warnings-as-errors in-memory
compiles (Project 66,560 / Launcher 37,888 / fixtures 13,824 bytes), PS parse,
and old/new source guards. Legacy method/pin/other-source comparison **PASS**;
`python3 -B scripts/check_project_scope.py` and `git diff --check` **PASS**.
This is local source verification, not runtime evidence. Generation07 Windows
build/owned suites and original Id/fixture execution are **NOT RUN** at freeze;
independent verification/review/commit precede any original run. No dependency,
config, metadata tooling or device path changed. Task-009 remains Blocked; even
a returned Project would not qualify services, DCA initialization/UI/state or
hardware. The earlier project06 unsupported-service FAIL remains actual history.

## Generation07 actual Windows owned verification

After the historical source-only freeze above, unchanged sources built **PASS**
with pinned Windows compiler 4.8.9221.0: eight sources/twelve artifacts.
Private source-freeze-07.json SHA-256
`0ed5171bf2522d78356f44fcafbf5da6be64d285f651546528375dbae3649c92`
and owned07-observation.json, 33,508 bytes, SHA-256
`36bf34ce75f5539dca24e0e23eb26a28e5ae0a46b4a0d5ebbe3fe978ef2927a4`
bind actual **PASS 15 Session0 / 25 limited / 20 admin negative** cases.
Both interactive runs used Session2 with correct roles, exit 0, removed tasks,
zero owned processes/no junction; 64 limited/49 admin case files retained.
The limited new fixture case validates one owned Id constructor/getter, stable
Id-only returns, contract negatives/saturation/unsupported requests; its Project
return remains unqualified. Original runtime **NOT RUN** in all owned profiles.
Raw UTF-16/strict UTF-8 provenance and nonempty Session0 CLIXML progress stderr
are preserved. An early limited status03 collection failed missing done;
status05 retrieved the same single run, with no test rerun. See [build/log hashes
and commands](../docs/DCA-PROJECT-CONTEXT.md). Independent review/commit and original
fixture run remain pending; Task-009 Blocked, ACs unchecked, no DCA/UI/state/
hardware or gate qualification. Source hashes remain unchanged.

Independent critical review PASS for generation07 at baseline
`91121b184043e32db930fd596e751a53ee69751a`. The reviewer inspected the actual
six-path diff and complete 15/25/20 Windows evidence appendix, checked the frozen
source/report and 182-file provenance, old-mode preservation, scope and whitespace,
and did not rerun Windows or read SDK binaries. The exact synthetic getter boundary
and unqualified-return limits are preserved. Original store-ID fixture execution
remains NOT RUN; no original positive service, Project fixture, DCA/UI/state,
hardware or gate is accepted. After the explicit source commit, only the fixed
single 15-second diagnostic may follow under existing authorization; failures stop
without automatic expansion or retry. The private review record binds the
pre-append document hashes; it does not claim those hashes identify this later
review note.

## Project07 actual original Id-only fixture result

At committed `3161dd889fe4a8f995cf122542a06503c94b9172`, exactly one original
attempt **FAIL**, exit 10, after twelve pins/OS facade PASS. Public ProjectId
constructor `060001EF` returned and getter `060001F2` validated exact UInt32 1,
explicit owned synthetic data, not a claimed-valid project identity. One
get_Id `060002F5` fixture return preceded denied IFactory.UndoManager
`060001FD` (two arguments). Project `06002830` returned=False,
fixture-calls=1/denied-requests=1; invocation ATTEMPTED_NOT_RETURNED, body entry
NOT_ESTABLISHED_BY_STACK_TRACE. TargetInvocationException -> NotSupportedException;
no dependency/unexpected assembly blocker, retry or service/dependency expansion.
Private project07-observation.json SHA-256
`4f307c0b31dce38319c5fa51384c7b8ab236fbbc6aa7a1139fe54c6505482fdc`
and raw-log/audit hashes are in the [result record](../docs/DCA-PROJECT-CONTEXT.md).
Independent result audit PASS; Session2/nonadmin child exit10 confirmed under
15s/64KiB, cleanup expected/last10 removed task/zero owned processes/scratch.
Owned15/25/20 PASS remains separate; historical NOT_RUN records are preserved.
Task-009 Blocked/ACs unchecked, no Project/fixture/DCA/UI/state/hardware/gate
qualification. Next work is metadata readiness only, no UndoManager fixture.
Scope/diff checks PASS; independent documentation critical review PASS, source unchanged. This record does not qualify a fixture or authorize an UndoManager response.

## Fixed UndoManager declaration implementation freeze

After reviewed project07 result `0e3ca948aaa1816c847200825cdc61d4e7727f23`,
add only explicit `project-undo-manager` guarded metadata mode: one exact
ObjectModel 14,204,880-byte/SHA73ffaaa... pin and identity, fixed
Knx.Ets.ObjectModel.UndoRedo.UndoManager root. The verified services report
SHA19194b... establishes the return type name; original class token/members
remain UNKNOWN until a later reviewed extraction. Exact hash/size precede
PEReader and metadata identity precedes root selection. A separate unsigned
owned root/identity tests declarations without original impersonation.

Coordinator policy explicitly permits only this new profile's MethodDef ceiling
65,536 after rechecking retained guarded count 38,472; every other table stays
32,768. Old CheckRows/four-input policies and ProjectId bounds are unchanged.
All file/chain/member/signature/report/capture/time bounds remain. Public/protected
constructors/methods and declared field flags/signatures only: no values, bodies,
custom attributes, dependencies or target execution. Constructor tools stay frozen.

Provisioned PowerShell 7.6.7 runs
`-NoLogo -NoProfile -NonInteractive -File experiments/dca-integration/Test-EntrypointMetadata.ps1`
(**PASS 192 / FAIL 0**); `python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py`
(**PASS 20**); `python3 -B scripts/check_project_scope.py` and
`git diff --check` (**PASS**). Prior 151 PowerShell/19 Python cases pass, including old report
byte equality; new tests cover fixed selection/identity/denials and row/bound
policies. [Contract, primary count evidence and six frozen hashes](../docs/DCA-PROJECT-CONTEXT.md)
record the exact scope. Independent verification and critical source review PASS; original metadata extraction remains NOT RUN. No UndoManager
fixture/activation/runtime is implemented or run. Task-009 remains Blocked/ACs
unchecked; no Project/DCA/UI/state/hardware or gate qualification follows.

## Actual fixed UndoManager metadata result

At source `9d86e5e98f0d1a2afd9d32bdbce869067aa73a9d`, one fixed guarded
metadata extraction **PASS**, exit 0; write/network denial probes true, all
target runtime **NOT RUN for this extraction**. Report, 7,574 bytes, SHA-256
`0e51ea640ce8d2c38433f2b3c0c03366e3bc1821899397d2a59ad37da14dedf6`
and observation, 1,909 bytes, SHA-256
`14821ce663e6afde30083de6ce42b66b381749cf7487b7e7f87aff1d1a4bef40`
bind source/provenance; independent result audit PASS. Root `02000131`, flags
`1048577`, has 24 filtered method declarations/eight fields and no reported
public constructor; protected ctor `06000B9C` is parameterless and NOT INVOKED. External
DisposableBase remains unresolved, no interfaces reported on the selected type.
The filter omits private/internal methods; no other-constructor/public-factory
absence or impossibility claim follows. [Full result/audit hashes and limits](../docs/DCA-PROJECT-CONTEXT.md).
Historical source-freeze NOT_RUN and owned 192 PowerShell/20 Python PASS remain separate.
Latest actual runtime remains project07 FAIL exit 10. No UndoManager instance/positive
service or Project/DCA/UI/state/hardware/gate qualification; Task-009 Blocked/ACs
unchecked. Next scope is read-only public-factory readiness inquiry only.
Scope/diff PASS; source/pins unchanged, no new runtime authorized by this record.

## Public Factory constructor-only implementation freeze

After reviewed UndoManager result `f6c441611c4c7aafc61b75904eecaa1c4b2c4bb7`,
add isolated `original-factory` / `owned-factory` modes. The original-only profile
preserves the twelve-input prefix, appending only exact Autofac 8.2.0.0 and
Knx.Kim.DataProvider 1.0.0.0 candidates from the audited private inventory.
All fourteen hashes/sizes precede platform pin/identity/load. The additions are
admitted only for this completed profile at exact identities/paths; no new
redirect/framework/compiler reference or generic resolver is added. Earlier
Project/Id/enum modes and twelve-pin arrays/config remain unchanged.

Factory type `02000023`/flags `1048577` and public constructor `0600012E`/flags `6278` must
match the guarded declaration and exact public nongeneric interface parameters.
Actual dependency tokens/compatibility remain UNKNOWN until checked. Both
arguments use unchanged deny-all proxies; only one public constructor attempt
is allowed. No original Factory method, Project/Id/UndoManager operation or
positive service is supplied; a returned/caught-denial object stays unqualified.

Local private `knx-task009-factory-sourcecheck.ps1` with provisioned PowerShell
7.6.7, `-Worktree WORKTREE -FrameworkReferences PRIVATE_EVIDENCE/task148-clean-host/references`,
**PASS**: six pinned framework references, C#5/x64/warnings-as-errors emits
76,800/37,888/15,360 bytes for Project/Launcher/owned fixtures, PS parse and
old/new source guards. Legacy source comparisons, scope script and diff check
**PASS**. [Contract, exact candidate pins and four frozen source hashes](../docs/DCA-PROJECT-CONTEXT.md)
record the bounded experiment. At this freeze, Windows08 build/owned tests and
original Factory runtime are **NOT RUN**; independent review/commit precede any
original attempt. Task-009 Blocked/ACs unchecked; latest actual runtime remains
project07 FAIL exit 10, with no Factory/service/Project/DCA/UI/state/hardware/gate proof.

## Generation08 actual Windows owned result

The unchanged frozen sources subsequently built **PASS** on pinned Windows
compiler 4.8.9221.0: eight sources/twelve artifacts. Actual owned suites **PASS
19 Session0 / 32 limited / 24 admin negative**, Session2 roles correct, exit 0;
both cleanups removed tasks, zero owned processes/no junction, 81/57 case files.
Private owned08-observation.json, 38,748 bytes, SHA-256
`220690e58f01f59cbf4500ebe92a9841ab2edad6edfa00d6ce8c5438c924acaa`
binds 206 evidence files; independent verifier08 audit PASS. First collection
succeeded for all three runs. Raw UTF-16/strict UTF-8 and Session0 nonempty
CLIXML progress stderr are retained. Fourteen-input preparation was copy/hash only;
initial Windows CLI length failure occurred before PowerShell and remains
preserved, corrected copy02/verify02 passed without SDK execution/bound change.
The freeze's legacy original_inputs=12 and explicit new_factory_original_inputs=14
are clarified by a separate note, with frozen JSON unchanged. [Build, freeze,
audit hashes and commands](../docs/DCA-PROJECT-CONTEXT.md) bind the results.
Historical source-freeze NOT_RUN is preserved; owned PASS establishes no original
Factory/service semantics. Critical source/owned-evidence review PASS; original Factory remains NOT RUN until the explicit source commit and separately authorized fixed attempt; Task-009 Blocked/ACs unchecked, latest runtime project07 FAIL exit 10,
metadata declarations-only PASS separate, no Project/Id/UndoManager/DCA/UI/state/
hardware/gate qualification. Four source hashes unchanged; scope/diff PASS.

## Factory08 actual original constructor result

At `edf408eb08fd60b4954fc61c349efbf14a6748dc`, exactly one original-factory
attempt **FAIL exit 2** after fourteen pins/OS facade/profile identities passed.
Actual interfaces Autofac `02000017` / Kim `02000005` were checked; Factory
`0600012E` BEGIN/END returned=False, zero denied proxy requests, invocation
ATTEMPTED_NOT_RETURNED/body entry NOT_ESTABLISHED. Requested AsyncInterfaces
8.0.0.0/PKTcc7b13ffcd2ddd51 was denied against the pinned/validated 9 profile,
without an alias, retry or widening. Version 9 is source/profile-bound, not new
binary inspection or an individual staged-identity raw-log line. Captured
TargetInvocationException/FileNotFoundException denial precedes final
IOException PROJECT_DEPENDENCY_BLOCKER. [Observation/log/audit hashes and limits](../docs/DCA-PROJECT-CONTEXT.md)
bind nineteen artifacts; independent result audit and critical documentation review PASS.
Nonadmin interactive Session2, child exit 2 confirmed under 15s/64KiB; cleanup
expected/last 2 removed task, zero owned processes/scratch. Owned 19/32/24 PASS
and historical original-NOT_RUN records remain separate. Harness Project/Id/
UndoManager/DCA/UI/state/hardware operations NOT RUN; constructor internals
UNKNOWN, network/descendants NOT_CONTAINED. Task-009 Blocked/ACs unchecked,
no Factory/service/Project/gate qualification. Source/pins unchanged; scope/diff PASS.

## Project12 actual Factory diagnostic

At source `f29d3c2d571a34bcae360c8a028ca2abbc37d180`, the single original
Factory12 diagnostic **FAIL**, exit 2. Owned Windows verification remains
**PASS 28/59/33** with all 18 artifacts preflighted. The Builder type
`02000015`, constructor `06000026`, and Build `0600002E` returned; Build produced
a non-null container and the log reached `EMPTY_CONTAINER_READY` in a genuine
context. The ValueTuple pin was the exact 25,544-byte Windows GAC file, SHA-256
`62216986c754ad8f0ee68412c42cfc3fc6e025bd908ae1fef1a3a8d528b1495a`, with
held-pin, exact identity/path, GAC and non-reflection facts allowed for this
attempt only. It does not establish Factory11's historical assembly origin or
facade proof.

Factory token `0600012E` logged `FACTORY_CONSTRUCTOR_BEGIN`; resolution of
`Autofac.resources, Version=8.2.0.0, Culture=en-GB,
PublicKeyToken=17863af14b0044da` was denied as
`PROJECT_DEPENDENCY_NOT_ALLOWLISTED`. The exception chain ends in
`PROJECT_DEPENDENCY_BLOCKER`; constructor end says returned=False, provider
denials=0. The evidence does not establish constructor body entry or the
lower-level causal root. Disposal returned once; the post-disposal audit found
scope=false and zero denied requests. No standalone Project result was emitted.
No resource fallback, dependency expansion or retry is evidenced or authorized.

Private `factory12-observation.json` (6,778 bytes, SHA-256
`51d900d625656f43b916537e1cb09ad98e7f616a893ee5d7abaccc75e6fc9fb8`) binds
the 13,894-byte raw `factory12.log` (SHA-256
`3578aad25d55133b7f42b0c0b1a9e104f05836762636e4f666fa9f942261a2d0`). The
independent `verifier12/factory-result-audit.txt` (4,650 bytes, SHA-256
`2e410a662214877188040ad631efce9d0e8f6a10665ef515ef41c38ced216edb`) is
**PASS** for provenance and bounded-result consistency; it confirms the
diagnostic remains **FAIL**, not a qualification result. The observation binds
25 files. Its audit did not rerun Windows or read SDK binaries. The four recorded
coordinator commands were:

```text
python3 private/evidence/task009-constructor-windows/prepare_factory12.py
python3 private/evidence/task009-constructor-windows/run_factory12.py
python3 private/evidence/task009-constructor-windows/collect_factory12.py
python3 private/evidence/task009-constructor-windows/cleanup_factory12.py
```

The bounded context was interactive session 2, non-administrator; child limit
15 seconds and capture limit 65,536 bytes. Cleanup recorded expected/last exit 2,
task removed, zero owned processes and zero scratch files. Network and descendant
processes were not contained. Factory body entry, Project/Id/UndoManager calls,
DCA initialization/UI/state and hardware operations are **NOT RUN** or
**NOT ESTABLISHED** as applicable. Task-009 remains **Blocked**, acceptance
criteria unchecked, and no phase gate advances. Critical documentation review PASS for this appended result; the diagnostic remains FAIL exit 2.
