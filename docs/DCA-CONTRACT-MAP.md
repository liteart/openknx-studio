# Static reference-candidate experiment

**Historical Task-140 snapshot.** Its unresolved-member and Blocked statements
below describe 2026-10-21, not current readiness. Subsequent
[Task-142](../tasks/Task-142.md), [Task-143](../tasks/Task-143.md) and
[Task-148](../tasks/Task-148.md) provide the selected signature, binding and
ETS-free startup/constructor evidence. [Task-005](../tasks/Task-005.md) is Done
for its selected route. [ADR-005](ADR-005-owner-authorized-private-dependencies.md)
records Simon's private-copy permission and prohibition on proprietary DLL
redistribution. No blanket SDK or later-lifecycle compatibility is inferred.

Task-140 investigates the unchanged MDT DALI DCA using private, operator-supplied
ETS reference candidates. It does not establish runtime compatibility or rights.
No vendor assembly is loaded; the original helper reads managed metadata bytes.

## Reproduction and restrictions

The owned tools live in `experiments/dca-metadata/`. PowerShell 7.6.7 is a private
developer tool, not a dependency of the Rust product. Run the synthetic tests with:

```sh
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-metadata/Test-ReferenceMetadata.ps1
```

`PWSH` identifies the provisioned PowerShell executable. Real input requires the
separate macOS guarded wrapper, private staged inputs, and successful restriction
probes. Do not invoke the raw PowerShell worker directly on manufacturer files.
The child policy denies writes and network access; it allows reads, process
operations, system queries and Mach lookup needed by the runtime. It is an added
research restriction, not a complete security sandbox or a production importer.
Microsoft explicitly warns that [PEReader is not designed for untrusted input](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutable.pereader).

The worker validates all expected input hashes before creating any metadata
reader. It bounds files (16 MiB each, 32 MiB total), table rows (32,768 each),
strings (1,024 characters), nested type scopes (16) and signatures (512 bytes).
The guarded parent caps each child at 30 seconds and combined stdout/stderr at
2 MiB, killing its own process group on timeout, overflow or cancellation.
Reports are private; only concise observations belong in source control.

## Input identities

| Input | Bytes | SHA-256 |
|---|---:|---|
| `MDT-DaliDcaPlus.dll` | 452,608 | `4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d` |
| `Knx.Ets.Sdk.AddIns.AddInViews.dll` | 72,704 | `5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d` |
| `Knx.Ets.Sdk.dll` | 6,979,024 | `7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989` |

DCA archive identity and manifest observations are in [REFERENCE-DCA.md](REFERENCE-DCA.md).
The SDK ZIP supplied by the operator has SHA-256
`35a4790a9a3c8e87264cfb566896b73efe0683ff194680fa7eff27a4108b5100`.
Its three regular root entries are the two candidates and their private inventory
manifest. Bounded streaming verified member size/hash/CRC, exact entry names,
absence of traversal/case collisions/encryption/special files, and the manifest's
reported identity/hash values. Private staging preserved the bytes outside Git;
no vendor file is committed. Task-139 records the operator's Windows copy evidence.

## Observed result, 2026-10-21

The guarded experiment completed on macOS 27 arm64 with PowerShell 7.6.7.
Both restriction probes passed on this run. The predefined verifier repeated
the guarded experiment and obtained a byte-identical private report. Private report: 54,179 bytes,
SHA-256 `1c1b0f4490565e5518cc517040a7e0bacfbb0d4338cea3a715e69dc295bceb6b`.
The original DCA assembly identity is `MDT-DaliDcaPlus`, version `4.0.0.0`,
neutral culture, public-key token `1a3211b0275a5581`. Tokens are observations,
not cryptographic signature verification.

| Observation | Result |
|---|---|
| DCA references to both supplied SDK assembly names | Requested `5.7.554.37315`; supplied `6.4.8718.0` |
| Other identity fields for those two requests | Name, neutral culture and token `61439873ec5e1159` agree |
| Candidate-directed DCA TypeRef rows | 27: 22 same-name class definitions in SDK, 5 same-name interface definitions in AddInViews |
| Missing same-name candidate TypeDefs in this subset | 0; forwarding and ABI remain unresolved |
| Candidate-directed MemberRef rows with TypeRef parent | 55: 50 SDK, 5 AddInViews; signatures fingerprinted, definitions not matched |
| Unsupported DCA member parents | 209 TypeSpecification rows, not decoded or assigned to candidate assemblies |
| Unsupported DCA TypeRef scopes | 0 |
| Direct AssemblyRef rows | DCA 15, AddInViews 8, SDK 29; total 52 |

The AddInViews-to-SDK reference exactly matches the supplied SDK identity. The
DCA-to-candidate references do not match exactly because their versions differ.
No runtime binding or compatibility policy was tested.

The DCA also directly references unsupplied `Knx.Ets.Common.Types` version
`5.7.286.36952`. AddInViews references `Knx.Ets.Common.Resources` version
`6.4.8718.0`. The SDK references multiple additional ETS assemblies, including
ObjectModel, Common and DynamicData, and a static `Knx.Falcon` reference. These
are metadata dependencies, not a claim that all are loaded on every path or that
the DCA accesses a bus directly. No dependency was resolved or downloaded.

These two files therefore provide useful type evidence but do not yet establish
a standalone, compatible host dependency set. Do not copy the entire ETS tree or
add Falcon to the host to satisfy a static reference list.

## Smallest next experiment

Before constructing a host, separately scope an offline comparison of the 55
observed member signatures against candidate definitions and decode the unsupported
TypeSpec scopes. Establish the original target framework and an explicit lawful
provisioning disposition. If additional private input is necessary, inventory only
named `Knx.Ets.Common.Types.dll` and `Knx.Ets.Common.Resources.dll` through the
existing opt-in script; their actual versions/hashes must be observed first.
No assembly-version redirect or shim was implemented. Any later approach needs
demonstrated compatibility and permitted provisioning. Windows loading and state
round trips remain separate gated experiments.

## Interpretation limits

Assembly identity equality and same-name TypeDef presence are separate observations.
Neither establishes member ABI, framework compatibility, activation, WPF lifecycle,
state round trips or bus mediation. Forwarders are unresolved. TypeSpec and other
unsupported member-parent scopes can hide candidate-directed references. Member
signatures are fingerprinted, not decoded or matched to definitions. No method
body, constructor, attribute or target runtime code is executed.

Task-003 and Task-005 remain blocked. Tasks 007/008 and the Task-028 product gate
are not unlocked by this experiment. Provisioning terms and an ETS-free runtime
remain separate requirements; user-local copying does not certify usage rights.
