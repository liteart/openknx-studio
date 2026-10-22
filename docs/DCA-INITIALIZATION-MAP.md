# Reference DCA initialization dependency map

## Scope and source

Task-008 investigates one unchanged `Mdt.Dali.DcaApp.DcaAppMain` constructor
and one `Initialize` call. It does not establish UI, state round trips or
device-operation mediation. Task-007 was accepted at
`3282a515d44aed7e9ccfc0bdd438811264827128`; Task-005 at
`ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`; Task-006 at `685dfbaa`.

The guarded Task-144 metadata report is 52,492 bytes, SHA-256
`8195260cb622508d32c11946ddcef84887d5f8284b9c9ea3ed5539aeac70b0c5`.
It identifies the primary DLL as 452,608 bytes, SHA-256
`4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d`,
`MDT-DaliDcaPlus, Version=4.0.0.0, Culture=neutral,
PublicKeyToken=1a3211b0275a5581`.
Its declared type token is `02000004`, public parameterless constructor
`0600000E`, and public instance `void Initialize(IInitializationContext)`
`0600001A`. The original reference names AddInViews 5.7.554.37315; the
accepted, explicit binding redirect selects 6.4.8718.0. Naming compatibility
alone does not prove runtime compatibility.

The selected context is the public interface
`Knx.Ets.Sdk.AddIns.AddInViews.IInitializationContext`, token `0200000F`,
from `Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, SHA-256
`5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d`.
It has no inherited interfaces and exactly these instance parameterless getters:

| Token | Getter | Actual return type | Current disposition |
|---|---|---|---|
| `0600002F` | `get_Project` | `Knx.Ets.Sdk.Project.Project`, SDK 6.4.8718.0 | Request logged once with explicit-denial disposition; original exception propagation unobserved; concrete compatibility unresolved |
| `06000030` | `get_Resources` | `Knx.Ets.Common.Resources.IEtsResources`, Common.Resources 6.4.8718.0 | Unsupported; contract usage unknown |
| `06000031` | `get_Configuration` | `System.Xml.XmlDocument`, System.Xml 4.0.0.0 | Unsupported; required original state and semantics unknown |
| `06000032` | `get_UserConfiguration` | `System.Xml.XmlDocument`, System.Xml 4.0.0.0 | Unsupported; required original state and semantics unknown |
| `06000033` | `get_HostNotification` | `Knx.Ets.Sdk.AddIns.AddInViews.IHostNotification` | Unsupported; bridge candidate after observed request |
| `06000034` | `get_DialogService` | `Knx.Ets.Sdk.AddIns.AddInViews.IDialogService` | Unsupported; UI contract usage unknown |

SDK, Common.Resources and AddInViews identities above use neutral culture and
public-key token `61439873ec5e1159`. System.Xml uses neutral culture and token
`b77a5c561934e089`. The context itself provides no network service getter.
Static direct DeviceManagement references remain a mediation risk, rather
than evidence that initialization invokes them. No download hook or network
service has been supplied. Existing pinning covers all eight selected inputs;
no additional private compiler reference is needed for the reflected proxy.

| Dependency category | Classification | Evidence boundary |
|---|---|---|
| Eight exact original/runtime inputs | Allowed reference under the accepted selected-route provisioning record | Private unchanged files; no redistribution or general rights decision |
| Reflected initialization context | Implementable bridge contract for an explicit unsupported diagnostic | Actual interface/tokens validated; no production service is implemented |
| `Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159` | Blocker for the selected route | Actual resolution request rejected outside the eight-input allowlist; additional file/hash/provisioning not qualified by this run |
| Concrete Project service | Unknown compatibility; unsupported | Actual `get_Project` request logged once with explicit-denial disposition; no Project object supplied or original exception return observed |
| Resources, state documents, notification and dialog services | Unknown | Every member remains unsupported; no original request observed before timeout |
| Initialize completion/timeout cause | Unknown | No return, exception or end marker before the 15-second supervised timeout |
| Activation and concrete SDK object behavior | Unknown | A matching identity or interface cannot establish these behaviors |
| Direct network/download access | Unknown; blocker if it bypasses Rust mediation | No driver, hook or bus service is supplied or exercised intentionally |

The static surface below is inherited from accepted Tasks 142/143/144; it was
not re-extracted or invoked to complete this report.

| Required area | Actual static surface | Runtime disposition and limit |
|---|---|---|
| Storage | Context `get_Configuration` `06000031` and `get_UserConfiguration` `06000032` return `System.Xml.XmlDocument` | No original request observed; required XML/state semantics unknown; no empty document or persistence service supplied |
| Project context | Context `get_Project` `0600002F` returns concrete `Knx.Ets.Sdk.Project.Project` | One original request logged with denial disposition; concrete construction/behavior and original exception propagation unknown |
| Device context | DCA `GetDcaUi` `0600001E` accepts `IEnumerable<Knx.Ets.Sdk.Project.Device>` and returns `FrameworkElement` | Static Task-144 declaration only; no Device object or UI invocation supplied |
| Network services | DCA MemberRefs `0A0001F6`/`0A0001F7` name `Knx.Ets.Sdk.Network.DeviceManagement.ReadProperty`/`WriteProperty`; Task-143 reflection-only policy resolves SDK MethodDefs `06001C5D`/`06001C60` | Static/member-binding evidence only; actual runtime use and mediation UNKNOWN; no property operation invoked intentionally |
| Download hooks | No download hook is qualified by these selected declarations or property-service member bindings | Required load/download behavior UNKNOWN; ReadProperty/WriteProperty binding is not download proof |
| UI entrypoints | `GetAddInUI` `06000017`: `FrameworkElement ()`; `GetDcaUi` `0600001E`: `FrameworkElement (IEnumerable<Device>)` | Task-144 signatures only; both uninvoked and UI readiness unqualified |
| Host callbacks | Context `get_HostNotification` `06000033` returns `IHostNotification`; its declarations are `SelectionChanged(IEnumerable<Object>)`, `SetStatusbarText(String,String)`, `ShowAppHelp()`, `SuggestContentSelectorWidth(Double)` returning void, and `GetToolbarItem(String)` returning `IAddInToolbarItem` | Task-144 public metadata only; context getter unrequested and callbacks unimplemented/uninvoked |
| Dialog service | Context `get_DialogService` `06000034` returns `IDialogService` | Task-144 getter signature only; this selected report does not inventory IDialogService members; getter unrequested, service unimplemented/uninvoked |

## Ordered plan

1. Completed owned prerequisite: the pinned Windows net48 six-getter proxy
   test proves explicit denial, direct/wrapped failures and an unqualified
   normal return after a caught denial.
2. Completed original diagnostic: exact context contracts validated; one
   constructor returned and one Initialize began. The run recorded ObjectModel
   resolution rejection and one Project request with explicit-denial
   disposition, then timed out.
3. Qualify the actual ObjectModel reference: establish the exact unchanged
   file, identity/hash, permitted provisioning and dependency surface before
   proposing any additional allowlisted input. Do not invent the assembly or
   satisfy its name with a replacement implementation.
4. Investigate the concrete SDK Project compatibility required by the observed
   getter. Providing a same-named interface, null, empty project or guessed
   configuration cannot satisfy it. Preserve timeout causality as unknown;
   any later controlled diagnostic requires its own reviewed bounded plan.
5. Inspect any later network/download expectation without enabling it. A
   private unmediated driver blocks compliance. Rust owns policy and state;
   public knust services own protocol execution.

## Evidence status

Static metadata extraction and original constructor evidence are inherited
from accepted Tasks 144/148, with their original limits. Re-reading the guarded
report verifies its hash and six getter declarations. The coordinator's
frozen-source net48/x64/C#5 build is PASS, as are seven Session-0 and thirteen
interactive owned test cases. The interactive session was 2, administrator
false; its log shows all six getters denied, direct/wrapped exception
propagation, and a normal caught-denial return remaining
`qualification=NOT_ESTABLISHED`. These are owned-fixture results. The original
run at reviewed source `89b08d24631021eb9863e7f1737753af0b8cfbd9` validated
all six contracts, returned from one constructor and began one Initialize.
It rejected the named ObjectModel dependency, logged one `get_Project`
request with explicit-denial disposition and reached the 15-second launcher timeout (exit 124, direct child
exit confirmed). There is no Initialize end, exception or result marker;
initialization completion and timeout cause are **INCONCLUSIVE**. Actual
exception propagation into original Initialize was not observed; only the
owned fixture proves that failure shape. The runtime attempt is **FAIL**
(timeout 124), while this dependency-analysis report remains subject to
separate acceptance. The local Mono executables failed with
`bad CPU type in executable` and provide no Windows compatibility evidence.

Private evidence is under `private/evidence/task008-initialization`.
`build-report.json` SHA-256 is
`889cd0b610513cc438a72309592ecba698fc96d13511efb40eb35502937bb7f2`.
The 53-file `owned-evidence-manifest.json`, SHA-256
`a1d5857dd80d70ac43eb1e26b5ff58840cbda2f988cf482dd6980f5ffee13f57`,
was locally verified against every file's bytes and hash. All compiled source
hashes match the worktree. Cleanup confirms zero owned processes and removal
of the temporary task. Full pins and test limits are in
[Task-008](../tasks/Task-008.md).

Original evidence has a separate six-file manifest, SHA-256
`b93d4ce680427cb01ac78a1428fee76e9c10a2201cc0ac200dbd147410140e96`;
all six bytes/hashes were locally verified. The original log SHA-256 is
`083af4049ab864a1b80d3f5e2a40bab36eb02c254b8347dddd8daf1d20f7be16`.
Post-run inspection confirms the eight original input sizes/hashes unchanged;
cleanup confirms zero owned processes and task removal. The coordinator
records standing Task-008 authority plus the user's explicit VM connectivity
exception; actual isolation remains **FAIL**. No explicit UI/state/Dispose or
intentional device operation was requested, and no network containment or
bus-mediation claim follows.
Only offline reference/provisioning and concrete-context investigation is
ready as follow-up work. This report does not establish Task-009 UI readiness
or authorize another original execution.

The proxy supplies no SDK implementation and returns no service objects.
It logs at most 16 context requests with member text limited to 512 characters;
the count saturates at 17 and reports saturation explicitly. Later requests
remain denied with `DCA_UNSUPPORTED_SERVICE:REQUEST_LIMIT`.
Strong names, concrete objects, activation, original UI lifecycle and state
round trips remain unresolved. Actual network isolation is not established
by the launcher or the proxy. No hardware test or phase gate is approved here.
