# DCA and ETS SDK provisioning disposition

Historical evidence snapshot. Active owner decision and current technical
disposition are recorded at the end of this file; later decision supersedes
historical legal-review prerequisites below.

## Scope and evidence boundary

This report records the dependency evidence available for Task-005 on
2026-10-20. It does not determine legal rights, identify a compatible SDK
binary, or qualify a standalone host. Task-004 was accepted after independent
verification and review. Its private MDT DCA input is identified by SHA-256
`cd5bbe33e8b13ba92d7575d33b3ca8f895589f5ec5d87706f847f997cddc3ec9`; it was
not extracted or executed for Task-004. The historical static inventory in
[REFERENCE-DCA.md](REFERENCE-DCA.md) records a .NET Framework 4.6.1 runtime
token and SDK-related strings only. It does not establish assembly references,
strong names, executed contracts, or licensing.

The operator reports having licensed ETS 6 access, and an existing ETS 6
Home 6.4.1 Build 8718 UI was observed over RDP. This is not evidence of active
KNX membership, SDK package access, SDK use rights, or a clean standalone host.
Automation input failed with `noWindowsAvailable`; no Windows PowerShell,
assembly, MSBuild, framework-pack, or SDK inventory command ran. OS and toolchain
versions remain unknown. No SDK package or applicable package terms were
provided for review.

## Dependency disposition

| Dependency or evidence | Disposition | Observed evidence and limit |
|---|---|---|
| Review of public SDK support pages | allowed | Read-only public documentation review is within this task. The pages list demo archives; their package terms and binaries were not assessed. No SDK package access, binary compatibility, or runtime rights are established. |
| Exact ETS SDK NuGet package and compile-time references | unavailable | KNX documentation says the SDK package is retrieved through MyKNX Member Resources and requires active KNX membership. No exact package, version, hash, or terms were supplied. |
| SDK use and redistribution terms for this host | needs review | No applicable SDK terms or package license was supplied. Public availability and the user's ETS license do not establish these rights. Human review is required when applicable terms are obtained; this report makes no rights determination. |
| MDT DCA archive | needs review | The exact private archive is available and hashed, but the original DCA's applicable license/provisioning terms and complete CLR dependency identities were not reviewed. Availability does not establish rights. |
| Concrete ETS runtime services, activation, and strong-named dependencies | unavailable | Actual assembly references, identities, executed SDK types, activation checks, and runtime calls have not been inventoried. Whether these are needed remains unknown. |
| Clean Windows host and pinned compatible build/runtime toolchain | unavailable | No clean host, Windows OS, MSBuild, or framework-pack inventory was run. The DCA's .NET Framework 4.6.1 token and KNX's documented SDK target of .NET Framework 4.8 are unqualified together; this is not evidence of incompatibility. |
| Arbitrary ETS installation files as a dependency source | incompatible | The project contract requires separately inventoried lawful SDK references and a clean host without ETS at runtime. Copying arbitrary files from an ETS installation is not an approved provisioning procedure. |

The table uses `incompatible` only for the proposed arbitrary-installation-copy
route under the stated project contract. No incompatibility finding is made
about the DCA or SDK combination itself.

## Primary-source notes

Public KNX support pages were inspected on 2026-10-20:

- [How to get the ETS6 SDK](https://support.knx.org/hc/en-us/articles/7588377788562-How-to-get-the-ETS6-SDK), updated 2026-08-27: describes retrieval from MyKNX Member Resources and an active membership requirement.
- [Requirements](https://support.knx.org/hc/en-us/articles/7588671243410-Requirements), updated 2024-10-11: describes the SDK NuGet package, .NET Framework 4.8 compilation target, compatible development tools, and DLL availability through ETS setup. It does not establish that an arbitrary ETS installation copy is a permitted standalone provisioning route.
- [Documentation and demos](https://support.knx.org/hc/en-us/articles/7588568212626-Documentation-Demos), updated 2026-09-01: lists public documentation and demo archives; these do not provide the gated SDK package or establish binary contracts.
- [ETS Apps overview](https://support.knx.org/hc/en-us/articles/360018452500-ETS-Apps-overview) and [About Apps, DCAs and Plug-ins](https://support.knx.org/hc/en-us/articles/360000228850-About-Apps-DCAs-and-Plug-ins): describe the ETS-hosted app/DCA model; they do not establish this DCA's standalone SDK rights.

The public [KNX Tools SLA](https://support.knx.org/hc/en-us/article_attachments/18554600032786)
was not established as applicable to this MDT package or the SDK components.
It is therefore not used to infer rights or a prohibition. No third party was
contacted, no account was created, and no agreement was accepted.

## Blocker and next evidence

Task-005 remains **Blocked**. The narrow next inputs are:

1. Lawful access through the documented MyKNX route to the exact SDK package,
   with package version, hash, and applicable terms retained privately.
2. Human review of those terms and of the MDT DCA's applicable terms for the
   proposed local build/use. Seek written clarification only if the existing
   terms do not resolve a material question.
3. Working Windows input control for the already authorized tool/assembly
   inventory, with exact OS, MSBuild, framework packs, assembly identities and
   strong names recorded. Separately qualify a clean ETS-free Windows runtime
   for the independent host proof; inventory on an ETS-installed development
   machine alone does not provide that proof.
4. A documented compatible build and runtime provisioning route that does not
   rely on an ETS installation at runtime.

Until these inputs exist, do not add SDK binaries, invent compatibility
surfaces, build a fake SDK shim, patch vendor binaries, or treat public docs and
demos as package evidence. No source implementation or phase gate is authorized
by this report. No hardware operation was performed or approved.

## Task-005 checks

Evidence was gathered from the current local source at baseline `94358b5918ddc7a3ff0f3f0d16153d1b40c0954a` on
branch `task/Task-005-sdk-provisioning`, plus the Task-004 record. Task-004's
implementation source revision is recorded there as `4f3ddb7`; Task-001 and
Task-002 have independent verifier/reviewer PASS. Task-003 remains overall
Blocked but is not a Task-005 dependency. No Task-005 source commit is claimed.

| Check | Result | Evidence |
|---|---|---|
| TEST-001: terms and packaging review | INCONCLUSIVE | Public source pages inspected, but actual SDK package, its applicable terms, and DCA license/provisioning terms were not supplied. |
| TEST-002: obtain permitted references | NOT RUN | No SDK package was supplied and Windows input automation was unavailable; RDP access itself was authorized. |
| TEST-003: absent prerequisite closes input check | PASS (negative case) | Command: `python3 scripts/check_reference_inputs.py --require sdk-references-rights`; exit 1, `FAIL: sdk-references-rights: required input is Missing`. This is an input-inventory result, not a legal decision or gate approval. |
| Windows, SDK, or vendor assembly inspection | NOT RUN | Windows automation was unavailable; no SDK package was supplied. |
| Hardware operation | NOT RUN | No hardware operation was part of this task. |
| `python3 scripts/check_project_scope.py` | PASS | Exit 0; pinned planning scope, local Markdown paths, and unique task identities validated. The tool reports product, Windows DCA, hardware, rights, and human gate approval NOT RUN. |
| `git diff --check` | PASS | Exit 0 for tracked changes. |


Final predefined verifier/reviewer: PASS for the bounded blocker-report increment,
with no blocking findings. Both independently checked the required SDK and clean
Windows inputs (exit 1 as expected), scope navigation and whitespace. They did not
fetch source pages again or verify actual package rights. Task-005 remains Blocked;
this report supplies no provisioning, compatibility, operation or phase approval.

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

## Alternatives investigation follow-up, 2026-10-21

[ETS SDK alternatives](DCA-SDK-ALTERNATIVES.md) records the online research and
candidate dispositions. Official membership-gated provisioning is not assumed to
be the only possible technical investigation. Public documentation and static
metadata can help assess a minimal compatibility surface, but no usable lawful
reference/runtime route was proven. Falcon and ETS-hosted bridges do not replace
DCA hosting. MDT PRO64 offers documented partial web workflows on a different
product; substituting it for original-DCA-first remains an explicit scope decision.
Task-005's missing runtime inputs and Blocked status remain unchanged.

## Selected local-reference investigation, 2026-10-21

The user subsequently selected the minimal compatibility investigation and
explicitly authorized a user-run script that inventories and optionally copies
selected DLL candidates from their own ETS installation into private local storage.
[Task-139](../tasks/Task-139.md) implements that narrow evidence-gathering route;
[the guide](LOCAL-ETS-REFERENCES.md) defines exact selection, metadata-only default,
new non-Git destination, bounds and known filesystem limits. This supersedes the
earlier blanket rejection of installation copying for this authorized research
step. Arbitrary installation copying, runtime loading and redistribution remain
outside this route. Rights and actual standalone provisioning are unresolved;
local copying does not decide them. Task-005 remains Blocked.

## Observed installed reference candidates, 2026-10-21

The operator supplied a successful Windows metadata-only inventory of
`Knx.Ets.Sdk.dll` and `Knx.Ets.Sdk.AddIns.AddInViews.dll`, both reporting assembly
version `6.4.8718.0`, neutral culture and public-key token `61439873ec5e1159`.
[Task-139](../tasks/Task-139.md) records exact byte counts, SHA-256 and the operator's
observation timestamp. This supersedes the earlier total absence of candidate
identity evidence. The independently distributed SDK package and its applicable
terms remain unavailable; this is an installed-reference candidate route, not
proof of package equivalence, dependency closure, signature validity or use rights.
No copying, vendor execution or independent host test is established by the JSON.
The next authorized private-copy list is limited to those two exact filenames;
verify its output against the recorded hashes before further analysis.

## Matched private-copy evidence, 2026-10-21

The operator's successful copy report for the two observed SDK candidates is now
matched to the supplied Git-ignored ZIP by streamed lengths/SHA-256 and the bounded
unique-key JSON manifest. [Task-139](../tasks/Task-139.md) records the archive hash,
manifest hash, timestamp, actual Windows suite results (19 PASS, 0 FAIL, 2 NOT RUN)
and ConsoleHost 5.1.26100.9168 observation. Selected candidate bytes are available
privately for static analysis. This resolves candidate acquisition evidence, not
the missing terms or an approved independent runtime. Task-005 remains Blocked.

## Current owner decision and technical disposition, 2026-10-21

Simon explicitly accepts responsibility and directs the project to assume local
use is authorized. See [ADR-005](ADR-005-owner-authorized-private-dependencies.md).
This is a binding project decision, not legal certification by an agent. The
earlier requests for SDK/DCA license documents, lawyer review, or legal
provisioning approval are historical and no longer prerequisites. The selected
DLLs may be kept in private local storage under the existing extraction route;
no original DLL may be committed to Git or included in any build distribution.

Task-005 remains **Blocked on technical qualification only**. The DCA requests
SDK/AddInViews `5.7.554.37315`, while the supplied private candidates are
`6.4.8718.0`; the candidate map records 55 unmatched members and 209 unsupported
TypeSpec member parents, with additional dependencies unsupplied. These counts
are unresolved compatibility evidence, not a proven incompatibility. Clean
ETS-free Windows runtime closure and binding/activation behavior remain
unverified. Task-003 and Task-006 Windows/Rust checks have PASS evidence; tooling
availability is not a blocker. Hardware identity, isolated bus/network path,
operation-specific human authorization, and recovery requirements remain under
the existing hardware process and are not waived by ADR-005.

## Current static compatibility evidence after Task-142

Reviewed source `0b3cfef884a2181e12bf58acf1d08b76312789ed` provides the bounded
member comparison. It supersedes the earlier unresolved-count snapshot:
53 direct structural signature matches, two unresolved external Common.Types
identity comparisons, and 209 decoded TypeSpec owner scopes (none declares a
member in the supplied candidate assemblies). Same-name ReadProperty and
WriteProperty direct definitions exist; different PropType assembly identities
remain deliberately unresolved. Static shape similarity is not ABI or binding
compatibility. SDK versions still differ and runtime closure is unqualified.
The static experiment's 31/9/12 suites, guarded private reproduction and review
PASS do not advance the original-DCA runtime gate. ADR-005 remains applicable.

## Exact additional private candidates, 2026-10-21

The existing Task-139 extractor was reused on the authorized Windows host for
exactly two filenames, first inventory then explicit private-copy mode. It uses
`System.Reflection.AssemblyName.GetAssemblyName`; target execution was NOT_RUN.
Inventory UTC: `2026-10-21T13:28:41.8012922Z`; complete copy UTC:
`2026-10-21T13:28:58.0512005Z`. The task performed no writes to the installed ETS source. Only the
owned remote temporary script/copy was removed after SFTP return.

| Private candidate | Bytes | SHA-256 |
|---|---:|---|
| `Knx.Ets.Common.Types.dll` | 215,504 | `5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0` |
| `Knx.Ets.Common.Resources.dll` | 78,800 | `6e07df9e247b1d5e6a11c9e8131ec8cc8ab8aabeca9af02b1911608d1eefe661` |

Both Windows metadata identities report version `6.4.8718.0`, neutral culture,
and token `61439873ec5e1159`; tokens are not signature verification. The local
manifest is 1,925 bytes, SHA-256
`08122ad0600f7a226baf721060aa8cda3649835cbdaf0c91f19add499815c9ef`.
The two files and manifest are private at
`private/downloads/task005-sdk-dependencies/task005-20261021-01/` under the
coordinator checkout, Git-ignored. Local size/hash/manifest comparisons PASS.
No original DLL or generated manifest is committed.

The previously guarded Task-140 AssemblyRef observations distinguish:

- DCA row 13 requests Common.Types `5.7.286.36952`: the new candidate differs.
- Supplied SDK row 5 requests Common.Types `6.4.8718.0`: identity fields agree.
- AddInViews row 5 requests Common.Resources `6.4.8718.0`: identity fields agree.

The earlier report was read as JSON for this comparison; the new DLLs received
no additional unguarded metadata decoding. No candidate member body, dependency
closure, CLR binding, activation, DCA or network operation was tested. Simon's
assumed-use authorization applies. The older requested Types assembly and clean
ETS-free runtime qualification remain unresolved; Task-005 is not Done.

The explorer reported no `ETS6.exe.config` at the selected installation root
from its bounded read-only exact-path check. XML parsing, file hashing and binding-redirect extraction
were therefore NOT RUN. Other paths, configuration files and programmatic
binding policies were not inspected. This is not evidence that ETS has no
binding policy. The exact temporary reader script was removed afterward.


## Current selected-route provisioning contract, 2026-10-22

ADR-005 remains Simon's private-use assumption and responsibility decision.
Accepted Task-143 proves its explicit app-local binding policy for the selected
member references; Task-144 identifies actual public entrypoint/contracts.
Accepted Task-148 at final record `134ebb4243c349f7128e655f60274f6465f526b8`
provides actual ETS-free startup/one-constructor evidence. Tasks 003/006 are Done.
Earlier unqualified counts, runtime absence and tooling-blocked snapshots above
remain historical; they do not supersede those accepted results.

The selected route uses separately inventoried, unchanged private DLLs without
installed/running ETS on the execution machine. Startup/constructor qualification
is sufficient for Task-005 AC-003. Additional initialization/UI/state SDK services
are investigated in Tasks 008–012, not invented here or made a circular
Task-005 prerequisite. This is neither complete SDK compatibility nor full PoC A.
Actual isolation FAIL and the owner's narrow Task-148 exception remain recorded;
this compile-only increment authorizes no further original code execution.

| Selected item | Technical disposition | Evidence and limit |
|---|---|---|
| Private SDK/AddInViews 6.4.8718.0 | Available | Exact accepted hashes/identities from Tasks 140/143/144; compile-only proof PASS; see observed result below |
| Private primary DCA 4.0.0.0 | Available | Unchanged constructor returned in accepted Task-148 on ETS-free Windows |
| Common.Types, Common.Resources, BCL, netstandard and Tasks.Extensions stage inputs | Available, path-specific use unverified | Hash-pinned for Task-148; none was recorded loaded on its constructor path |
| Public net48 compiler references and Framework64 C#5 compiler | Available | Accepted Task-148 six-file public profile and exact Windows compiler pin |
| Installed/active ETS on runtime machine | Not required for selected startup/constructor route | Task-148 actual evidence; not a claim about later methods |
| Other methods, activation and service dependencies | Unverified for later capabilities | Tasks 008–012; no fake service or broader compatibility inference |
| Network isolation / device-operation mediation | Not qualified | Task-148 isolation FAIL/explicit constructor-only exception; no intentional device operation |

### Exact-private-reference compile-only experiment

The Task-005 contract is written before source. An owned interface-only library
names six actual public types: IInitializationContext, IDeviceConfigurationAddIn,
IEts4AddIn, IEts4AddInV2, Knx.Ets.Sdk.Project.Project and
Knx.Ets.Sdk.Project.Device. The four interfaces reside in
Knx.Ets.Sdk.AddIns.AddInViews. Their names/type references come from the accepted
Task-144 guarded report (52,492 bytes, SHA-256
`8195260cb622508d32c11946ddcef84887d5f8284b9c9ea3ed5539aeac70b0c5`).
No contract implementation, device object or lifecycle method is supplied.

Only two private compiler references are selected:

| File | Bytes | SHA-256 |
|---|---:|---|
| Knx.Ets.Sdk.dll | 6,979,024 | `7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989` |
| Knx.Ets.Sdk.AddIns.AddInViews.dll | 72,704 | `5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d` |

Both identities are version 6.4.8718.0, neutral culture and public-key token
61439873ec5e1159. Token matching is not signature verification; no checks are
bypassed. The compiler is Framework64 csc.exe, file version
`4.8.9221.0 built by: NET481REL1LAST_25H2`, SHA-256
`46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00`.
The fixed public profile is mscorlib, System, WindowsBase, PresentationCore,
PresentationFramework and System.Xaml from the pinned net48 1.0.3 reference
package; Task-148 records all six hashes. No other original DLL is a compiler
reference. The output is a private net48/x64/C#5 library and manifest, never
execution-loaded or distributed.

The driver checks attributes before descent and permits only a specifically
missing final output component. All input hashes and the two private metadata
identities are checked before output creation. Fixed input files remain read-
only through compilation. The compiler is supervised for 20 seconds with a live
65,536-byte combined stdout/stderr cap and confirmed exit. A fresh output outside
Git prevents overwriting prior evidence. This is a bounded selected experiment,
not a race-free filesystem/security sandbox or generic upload service.

Planned coordinator commands (not yet executed):

```powershell
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass `
  -File "$Source\Test-ReferenceCompilation.ps1" -Compiler $Framework64Compiler `
  -ReferenceDirectory $PublicNet48References `
  -PrivateReferenceDirectory $PrivateSdkStage -OutputDirectory $FreshCompileOutput
# Missing case: use an empty private local directory; require nonzero exit and
# no output directory. Mismatch case: use explicitly invalid owned bytes for
# Knx.Ets.Sdk.dll; require the hash rejection before output creation.
```

The process-only owned-script invocation matches Task-148; it changes no machine
execution policy, signature/activation setting or Windows feature. Positive
compilation and both private-reference negative oracles are NOT RUN. Local
checks and final independent verification/review are pending. Original binaries
remain excluded from Git and every build distribution.

### Observed Task-005 compile result, 2026-10-22

The documented command above was executed on the ETS-free test VM. PASS:
compiler exit 0 confirmed, no stdout/stderr bytes, six public references and
exactly the two pinned SDK/AddInViews 6.4.8718.0 references. The 3,072-byte owned
library SHA-256 is `4a6015e5f1ef1d54b71521301846daaab69c88172c053110a1d7ac4a13ae9780`.
It and all original assemblies remained execution-unloaded in this experiment.
Missing SDK input produced `REFERENCE_COMPILE_MISSING_PATH`; an invalid owned
SDK-named file produced `REFERENCE_COMPILE_REFERENCE_HASH_Knx.Ets.Sdk.dll`. Both
exited 1 before creating output. Frozen sources, exact compiler/reference/output
identities and actual results are recorded in Task-005 and the ignored evidence
directory `private/evidence/task005-selected-runtime`.

This establishes the selected compile route alongside accepted Task-148's
startup/constructor route, not complete SDK runtime closure. The five additional
private staged dependencies were not compiler references and remain unexercised
on that constructor path. Later Initialize/UI/state dependencies require their
own evidence. Original DLLs remain excluded from Git and distributions.
