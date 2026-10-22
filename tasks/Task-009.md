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
| Implementation / report status | Blocked; guarded static Project qualification implemented, no SDK service |
| Source commit(s) | Project-only `f9549f67c10b81216021fe8de5ccdf7f496cb614`; four-input tooling `6c9d03d7cb11be764d1ba572a7116ce7655c25dc`; service inquiry source `5c7bc46e43f1cdd013214fce796b5b8fd6962719`; provider lookup source `b11c234273e3e61e0563c6474cfd694dacf674eb`; constructor-only baseline `a110775b07dcdb9f1238abae24e54c716c2b3e12`, new source/result commit pending |
| Tests executed | Metadata-only owned PowerShell 112 cases PASS; Python harness 18 tests PASS; guarded three-input Project extraction PASS; initial guarded four-input extraction FAIL with ROW_LIMIT; guarded diagnostic observed MethodDef 38472; narrow-policy construction extraction PASS (static only); project-services guarded extraction PASS (static only); project-providers guarded extraction PASS (static relationship only); project-constructors guarded extraction NOT RUN; scope and whitespace checks PASS. SDK service, Windows execution and hardware tests NOT RUN |
| Evidence location | [Project qualification](../docs/DCA-PROJECT-CONTEXT.md); private `task009-objectmodel` inventory/Project report and `task009-construction`, `task009-services` and `task009-providers` reports/verifier logs; dated records below |
| Acceptance criteria met | None asserted |
| Technical outcome | Eighteen direct provider candidates/edges observed, including nonpublic SDK Context/DcaExecutionContext/SdkFactory and public ObjectModel Factory/ObjectContext; constructors and behavior uninspected, 829 TypeSpec edges unknown, creation route INCONCLUSIVE |
| Gate outcome, when applicable | No gate applies or advances |
| Reviewer | Prior construction/service/provider static verification and source/result review PASS; constructor-only verification/review pending; no runtime service acceptance |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

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
