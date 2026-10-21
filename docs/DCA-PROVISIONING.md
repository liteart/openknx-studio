# DCA and ETS SDK provisioning disposition

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
