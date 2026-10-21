# Static member and TypeSpec comparison

Task-142 extends the [Task-140 static candidate map](DCA-CONTRACT-MAP.md).
It compares metadata signatures against direct definitions in the same private
candidate assemblies. It does not establish runtime binding, ABI compatibility,
Windows UI operation, state round trips or bus mediation.

## Tools and reproduction

The original research tools are in `experiments/dca-member-map/`. They add no
product dependencies. Owned fixtures contain synthetic managed metadata and no
executable method bodies. Run the local checks from the task worktree:

```sh
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-member-map/Test-MemberMetadata.ps1
"$PWSH" -NoLogo -NoProfile -NonInteractive -File experiments/dca-metadata/Test-ReferenceMetadata.ps1
python3 experiments/dca-metadata/test_run_probe.py
```

`PWSH` denotes the separately provisioned PowerShell 7.6.7 executable. Actual
runtime introspection observed .NET 10.0.13 and System.Reflection.Metadata
assembly version 10.0.0.0, informational version
`10.0.13+34ac57b7d95c2d9e34f072b04d3bf06e7fb0bf64`.
No target assembly is supplied to the compiler or loaded into that runtime.

Real manufacturer inputs must use the existing guarded wrapper and its fixed
`member-map` experiment. Never invoke the PowerShell worker directly on those
files. The Task-140 default experiment and restriction policy are preserved.
There is no arbitrary worker-path argument.

```sh
python3 experiments/dca-metadata/run_probe.py --stage "$PRIVATE_STAGE" --pwsh "$PWSH" --experiment member-map --output "$PRIVATE_STAGE/member-report-task142.json"
```

`PRIVATE_STAGE` is an owner-only private directory outside Git. Reports are
exclusive new owner-only files; they contain private metadata and remain outside
Git. The wrapper repeats actual file-write and network-denial probes before each
metadata read. Unavailable restrictions block that read. As in Task-140,
[Microsoft warns that PEReader is not designed for untrusted input](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutable.pereader).
This research restriction does not establish a production sandbox.

## Selected grammar and observation rules

The decoder uses public `BlobReader`, signature enums and metadata handles.
Important API/grammar evidence was checked against Microsoft's
[signature-provider contract](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.isignaturetypeprovider-2),
[MemberReference decoder](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.memberreference.decodemethodsignature),
and [TypeSpecification decoder](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata.typespecification.decodesignature).
The primary implementation references are .NET runtime tag `v10.0.0`
[SignatureDecoder.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Reflection.Metadata/src/System/Reflection/Metadata/Ecma335/SignatureDecoder.cs)
and [BlobReader.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Reflection.Metadata/src/System/Reflection/Metadata/BlobReader.cs),
which identify ECMA-335 II.23.2 as the signature specification. These are primary
references for public encodings, not proprietary SDK implementation sources.
The tool uses its own smaller bounded grammar to check counts before iteration;
it does not reproduce an SDK implementation or decode arbitrary IL.

Supported shapes are field and default-convention method signatures, primitive
and named class/value types, nested type names, type/method generic parameters,
generic instantiations, SZ arrays, pointers, references and required/optional
modifiers. It preserves instance/explicit-instance/generic header bits and generic
parameter counts. Array shapes, function pointers, pinned types, sentinels and
nondefault calling conventions return explicit unsupported dispositions.
Malformed/truncated encodings and trailing bytes are separate from unsupported
encodings. This selected profile rejects an entire input with
`DCAMEMBER_UNSUPPORTED_TYPE_IDENTIFIER` if a type's metadata name contains `.` or
`+`, its namespace contains `+`, or a nested type has a nonempty namespace.
Both definition and reference identities are checked before flattening a display
name or signature shape. This deliberately narrow profile prevents namespace,
name and nesting delimiters from merging distinct identities; supporting broader
legal identifier forms requires a separate structured-identity increment. This is a structural comparison, not full metadata validation.
Generic constraints, accessibility, implementation, inheritance and forwarding
are outside this comparison.

Candidate selection uses assembly names as an explicit observation mapping.
Only candidate assembly versions are omitted from named-type structural shapes;
name, culture and token remain, and other assemblies retain their version.
The direct declaring owner must also agree on name, culture and token before
a structural match is reported. A differing owner token or culture returns
`OWNER_IDENTITY_MISMATCH_UNRESOLVED`, including for signatures containing only
primitive types. Requested and supplied identities, including versions, are
reported separately.
This mapping is not an assembly redirect. Canonical identity atoms use JSON
escaping so untrusted names cannot introduce ambiguous shape delimiters.

Each member is compared only with same-name definitions of the same member kind
on the direct declaring TypeDef. A matching shape is
`DIRECT_SIGNATURE_STRUCTURAL_MATCH`. A missing or differing direct definition
retains explicit inheritance/forwarding uncertainty; it never proves absence.
Unsupported candidate signatures remain unresolved. TypeSpec parents record
the decoded shape, root declaring owner and requested owner assembly, including
noncandidate generic owners. Generic arguments do not change the declaring owner.

TargetFrameworkAttribute values are reported as bounded raw custom-attribute
blob length, SHA-256 and hex, plus constructor assembly identity. No attribute is
instantiated. No method bodies, type constructors or target code are read or run;
no dependency is resolved or fetched.

## Bounds and synthetic evidence

Files remain limited to 16 MiB each, 32 MiB aggregate and at most three inputs.
All hashes are verified before constructing any metadata reader. Every metadata
table is limited to 32,768 rows; names are limited to 1,024 characters. Signatures
and selected attribute blobs are limited to 512 bytes. Signature nodes and counts
are limited to 128; recursive type shapes and nested scopes to 16. Total work is
limited to 1,000,000 units. Conservative incremental accounting and the final
serialized report both enforce 2 MiB. The existing parent retains its 30-second
child deadline, combined 2 MiB stdout/stderr cap and process-group cancellation.

The initial owned regression failed with `DCAMEMBER_IMPLEMENTATION_MISSING`.
Coordinator review subsequently identified a declaring-owner identity gap: two
new owned negative cases initially failed because primitive-only signatures
could match despite different owner token/culture. The focused identity check
now rejects both while retaining the version-only observation rule. Independent
review then reproduced a namespace/name/nesting flattening collision. Five owned
negative cases each produced five false structural matches before the identifier
profile guard; all now reject explicitly. A positive genuine nested-owner case
retains five structural matches.
After implementation, the member suite reports **PASS 31 / FAIL 0**. It exercises
direct method/field/constructor matches, generic owners and parameters, version
mismatch, changed signature, owner-token/culture mismatches, ambiguous type
identifiers in definition/reference paths, genuine nested owners, inheritance
uncertainty, modifiers, malformed and
unsupported signatures, file/count/aggregate/row/name/scope/blob/count/node/depth/
work/incremental-report/final-report limits, and hash checks before metadata
reading. An oversized raw framework-attribute fixture exercises its distinct
512-byte guard through the actual managed-input Read path. Independent
verification remains required.
Task-140's worker reports **PASS 9 / FAIL 0**, and its wrapper reports
**PASS 12 / FAIL 0** (the original eleven plus a fixed member-profile regression).
The wrapper suite covers deadlines, output limits, process-group cancellation,
fail-closed probes and exclusive publication using owned fixtures.

## Private result and interpretation

The coordinator's guarded run completed on 2026-10-21 on macOS arm64,
PowerShell 7.6.7: exit 0, empty stdout/stderr, both negative restriction probes
PASS. The private report is 183,722 bytes, SHA-256
`fa494688a2fc5e98b6b22e84e173a5222a72d14cde613169176697d194824667`,
stored as `member-report-task142.json` in the Task-140 private stage outside Git.
The three reported input names, sizes and SHA-256 values exactly reproduce
[the Task-140 input pins](DCA-CONTRACT-MAP.md).

| Static observation | Result |
|---|---|
| Candidate-directed TypeRef-parent members | 55 |
| Direct signature structural matches under the stated mapping | 53 |
| Direct signature mismatches, inheritance unresolved | 2 |
| TypeSpec-parent members decoded to owner scopes | 209 |
| TypeSpec root owners in supplied candidate assemblies | 0 |
| Remaining unsupported/malformed member parents | 0 |

The two unresolved direct comparisons are DCA MemberRef rows 502 (`ReadProperty`) and 503
(`WriteProperty`) on `Knx.Ets.Sdk.Network.DeviceManagement`. Their same-name
candidate direct definitions are MethodDef rows 7261 and 7264. In both comparisons
the reported difference is the external `Knx.Ets.Common.Types` identity associated
with `PropType`: requested version `5.7.286.36952`, candidate signature version
`6.4.8718.0`. That assembly is unsupplied and is deliberately outside the two
candidate-version observation mappings. These rows do not establish a change in
argument count, a writable service implementation or compatibility of that type.
No definition in the missing dependency was examined.

All 209 TypeSpec root owners were noncandidate scopes: 167 `mscorlib`, 18 the
DCA itself, 12 `System`, 7 `System.Core`, 3 `PresentationFramework`, and 2
`System.Memory`. Some generic arguments may refer to candidate types; the root
owner still determines where the referenced member is declared.

Raw target-framework attribute metadata contains declarations for
`.NETFramework,Version=v4.6.1` in the DCA and `.NETFramework,Version=v4.8` in both
candidates. The observed DCA attribute blob is 77 bytes, SHA-256
`ed452979751bca2f46fbcaaaaa7053fe64b086ed1deb2ec78b27353d3ff0d97a`;
each candidate blob is 73 bytes, SHA-256
`8d970075d6ad8d125f8781d89080f2da387336bdde5f345649407737ee3cd4e4`.
Those declarations were inspected from the private raw hex/bytes only. They are
not a framework compatibility or successful CLR execution test.

The 53 shape observations retain the requested SDK version `5.7.554.37315`
versus supplied version `6.4.8718.0`; no binding redirect was implemented or tested.
The next narrow static dependency experiment can inventory explicitly selected
Common.Types/Common.Resources files, preserving their actual identities and
hashes, before deciding whether any additional comparison is justified.
Final independent verification and critical review PASS. Windows execution and hardware
operations are **NOT RUN**. Task-005 runtime qualification and all product gates
remain unchanged. Simon's explicit assumed-local-use authorization applies; this
research does not provide an agent legal certification. The preserved
`rights=UNRESOLVED` report field means that the tool makes no legal finding; it
is not an active document-acquisition gate on the local work Simon authorized.
The owner decision is recorded separately in Task-005 revisions `5c58448` and
`629575f`; technical runtime qualification remains separate.

## Initial independent verification before identifier correction, 2026-10-21

The project verifier reproduced all 25 member fixtures, 9 existing metadata
fixtures and 12 wrapper fixtures, plus scope and whitespace checks: PASS.
It reran the final frozen worker through the guarded `member-map` profile with
both restriction probes enabled. The independent report was byte-identical to
the coordinator report above (183,722 bytes, same SHA-256). Final helper source
SHA-256 before/after verification:
`ad6fc65f0aa1249c540c17a3a167d9b95e5d1bdf11992988d3de077b7c45fc2f`.
The initial outer-sandbox attempt could not establish the child restriction;
the identical authorized command then succeeded with the same deny-default
child policy. This did not relax its write/network restrictions.
No input bytes or source files were changed by the verifier. No runtime binding,
ABI, Windows, original DCA behavior or hardware qualification follows.

## Final-source independent verification, 2026-10-21

After the identifier correction, the coordinator and independent verifier each
reran the frozen final source through the guarded member-map profile. Both final
reports are byte-identical to the report above: 183,722 bytes, SHA-256
`fa494688a2fc5e98b6b22e84e173a5222a72d14cde613169176697d194824667`.
Both restriction probes PASS. Final helper source hash before/after verification:
`adb7bff2901ab183c22c295fec246cfa60f27d7703d53cf0bd1d5dd14ab1cddc`.
The independent verifier reproduced 31 member, 9 existing metadata and 12 wrapper
cases, scope and whitespace checks: PASS. The final exact inputs satisfy the
narrowed identifier profile; no source or original input bytes were changed.
The earlier 23/25-case observations and helper hashes describe intermediate
increments. Final critical review remains pending. Windows, CLR binding,
original DCA behavior and hardware qualification remain NOT RUN.

## Critical review

The independent reviewer returned PASS after the identifier-profile correction.
Its original owned ambiguity reproduction now rejects explicitly. It confirmed
final source/report hashes, restriction probes, static counts, source provenance,
UTF-8/whitespace and original-DLL exclusion. Complete 31/9/12 suites were run by
the verifier; the reviewer reran its focused owned regression only. No unresolved
finding remains in this static tool. Runtime compatibility remains unqualified.
