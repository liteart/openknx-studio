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
| `0600002F` | `get_Project` | `Knx.Ets.Sdk.Project.Project`, SDK 6.4.8718.0 | Unsupported; proprietary concrete type remains unresolved |
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
| Project, resources, state documents, notification and dialog services | Unknown | Every member remains unsupported until an actual request and compatible implementation are reviewed |
| Activation and concrete SDK object behavior | Unknown | A matching identity or interface cannot establish these behaviors |
| Direct network/download access | Unknown; blocker if it bypasses Rust mediation | No driver, hook or bus service is supplied or exercised intentionally |

## Ordered plan

1. Verify an owned public six-getter interface on the pinned Windows net48
   runtime. Each getter must throw the stable unsupported-service code, direct
   owned callers must observe it, and reflective Initialize must preserve it
   inside `TargetInvocationException`. No vendor assembly is used in this test.
2. Validate the actual reflected type, member tokens, signatures and return
   identities before constructing the unchanged original entrypoint. Retain
   the exact binding configuration and eight pinned read handles.
3. On the coordinator-authorized interactive non-administrator session, call
   one constructor and one Initialize. Record every requested member, then
   fail explicitly. A named unsupported-service failure is useful evidence;
   an assembly/type/load/activation failure is a different named dependency.
4. Review the first actual dependency before implementing any bridge member.
   Configuration blobs require genuine state; Project requires actual concrete
   SDK compatibility evidence; callbacks require bounded marshaling. None can
   be replaced with a guessed service, empty document or null return.
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
Initialize call is **NOT RUN**. The local Mono executables failed with
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

The proxy supplies no SDK implementation and returns no service objects.
It logs at most 16 context requests with member text limited to 512 characters;
the count saturates at 17 and reports saturation explicitly. Later requests
remain denied with `DCA_UNSUPPORTED_SERVICE:REQUEST_LIMIT`.
Strong names, concrete objects, activation, original UI lifecycle and state
round trips remain unresolved. Actual network isolation is not established
by the launcher or the proxy. No hardware test or phase gate is approved here.
