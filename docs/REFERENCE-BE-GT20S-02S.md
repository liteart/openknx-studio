# MDT BE-GT20S.02S fallback reference investigation

Task: [Task-023](../tasks/Task-023.md). Observation date: 2026-10-21 UTC.
The operator explicitly selected BE-GT20S.02S if primary documentation could not
resolve the first reference's absent memory Data. This records the new requested
reference and its privately acquired family archive. It does not claim that the
existing binary-input parser accepts the replacement or that AC-002 is resolved.

## Primary sources and provenance

The official [MDT product page](https://www.mdt.de/produkte/produktdetail/glastaster-ii-smart-serie-02.html)
lists BE-GT20S.02S as the black variant without the temperature-sensor designation,
and links family database V3.1 dated April 2024. It also links a separate DCA;
this experiment does not acquire or execute that DCA. The
[operating instructions](https://www.mdt.de/fileadmin/user_upload/user_upload/download/BE-GT2xx-02_MDT_AOI_DE_EN.pdf)
name the exact SKU; the [technical manual](https://www.mdt.de/fileadmin/user_upload/user_upload/download/MDT_THB_BE_02_Glastaster_II_Smart_Taster_Smart_86_V10.pdf)
describes the family, but no verified application-memory omission semantics were
found. These product documents do not prove the exact physical target/firmware.

The [official product database](https://www.mdt.de/fileadmin/user_upload/user_upload/download/MDT_KP_BE_02_Glass_Push_Button_II_Smart_V31.knxprod)
was acquired by the explorer with curl: 32 MiB cap, 120-second total timeout,
HTTPS-only initial/redirect protocols and at most three redirects. The command
reported HTTP 200, 900,549 bytes and the original MDT URL as the final URL.
The earlier Python attempt failed DNS before receiving bytes; its 45-second
timeout and MDT-host allowlist were not controls of the successful curl command.
The successful command imposed no redirect-host allowlist. Exact acquisition
command/output notes remain private as `task023-be-gt20s-acquisition.txt`.
Original bytes remain private and unchanged.

| Artifact | Observed identity |
|---|---|
| Filename | `MDT_KP_BE_02_Glass_Push_Button_II_Smart_V31.knxprod` |
| Private location | `private/downloads/task023-be-gt20s/` under the coordinator checkout |
| Bytes / SHA-256 | 900,549 / `b5ea5653091bf95d3650216eafbc26c080b3d6c14924bc315c2dfdcfaa63530d` |
| ZIP entries / expanded bytes | 57 / 21,193,051 |
| Selected non-temperature XML | `M-0083/M-0083_A-018B-31-369F.xml` |
| Selected XML bytes / SHA-256 | 9,861,680 / `f5bfbf9edb0ec799d2182f6962b20195b72d19268bbbe4c7dbe3458b7c0e0f6e` |
| Selected XML element count | 90,988 |
| Application | `M-0083_A-018B-31-369F`, number 395, version 49 |
| Schema / mask / procedure | `project/20`, `MV-0705`, `ProductProcedure` |
| Catalog item | `M-0083_H-3.2E38-1_HP-018B-31-369F_CI-BE.2DGT20x.2E02-1` |
| Hardware-to-program | `M-0083_H-3.2E38-1_HP-018B-31-369F` |

The archive uses the generic `BE-GT20x.02` family identity. The exact
BE-GT20S.02S-to-family association follows the manufacturer page, not a literal
exact-SKU identity in the archive. Actual device/application/firmware match is
unverified. The temperature-sensor application is a different alternative and
must not be selected accidentally. Neither a name suffix nor a search finding
no Secure-named XML fields proves the required runtime security profile.

## Resource observation and remaining blocker

| Segment suffix | Address | Declared bytes | Decoded Data bytes |
|---|---:|---:|---:|
| AS-4000 | 16384 | 513 | 513 |
| AS-4201 | 16897 | 511 | 511 |
| AS-0700 | 1792 | 666 | absent |
| AS-4400 | 17408 | 656 | 656 |
| AS-099A (RAM) | 2458 | 1 | absent |
| AS-4690 (EEPROM) | 18064 | 1968 | 1968 |

All four present payloads match declared segment sizes. Two omit Data, matching
the unresolved pattern in the first reference. No zero-fill, preserve-device,
implicit initialization or resource-completeness interpretation is inferred.
The [KNX Project Scheme 2.0 description](https://support.knx.org/hc/en-us/article_attachments/360024169360)
discusses other Data omission contexts, not these application memory segments.
The additional public search therefore did not establish an authoritative
resolution of AC-002 for either reference. Task-023 remains Blocked.

## Parser boundary and next experiment

The reviewed parser still selects only its original BE-04000.02 profile. It
must reject this candidate's identity and archive bounds. The candidate exceeds
its 16-entry, 2 MiB member and 8 MiB expanded-total bounds; these defaults were
not weakened. The selected new XML is approximately 9.9 MB and contains 90,988
elements. A candidate-specific parser increment needs separately explicit bounds,
owned regression tests, exact identity selection and reference validation before
claiming support. No universal parser or compiler is implied by acquisition.
The original reference's evidence remains reproducible and is not relabeled.

## Checks and limits

The explorer performed bounded static ZIP/XML/Base64 inspection; the coordinator
independently reproduced archive/member hashes, counts, selected application
attributes and all six resource lengths using Python standard-library readers
without extraction. XML DTD/entity declarations were excluded, input was bounded
at 32 MiB aggregate expanded and selected XML below 12 MiB. The original archive
was unchanged. No manufacturer content was executed or committed.

Rust parser compatibility with this candidate, compilation, runtime signature
validation, DCA execution, Windows and hardware are NOT RUN. The findings are
static format observations, not a manufacturer schema implementation or device
qualification. Existing Rust suites were not repeated for this documentation-only
increment. Scope/whitespace checks and independent review are recorded in Task-023.

## KNX Standard v3.0 resource-semantics review, 2026-10-22

The private KNX Standard v3.0.0 copies were checked against the rendered pages
and source text for Resources, Management Procedures, Profiles, and XML Data
Encoding. Their SHA-256 identities are recorded below so this documentation
review can be reproduced without placing the source PDFs in Git.

Management Procedures v02.01.02, §3.31.3.4, pages 139–140, distinguishes
`AllocAbsDataSeg` (segment type 0, absolute allocation of data or code) from
`AllocAbsStackSeg` (segment type 1, absolute stack allocation). The same section
defines memory type 2 as RAM. This classifies the observed ProductProcedure
entries: AS-0700 is absolute data/code allocation, and AS-099A is absolute stack
allocation in RAM. It does not define how this manufacturer's XML encodes the
contents or initialization of either range.

Profiles v02.01.01, Appendix A.2.4.1, table 7, page 144, attaches
footnote 66 to the marked System 300 mask `0300h`/`2300h` cells: those
additional load controls are conditional on additional data being downloaded.
The observed `MV-0705` is a separate column; this footnote cannot establish
optional controls or payload omission for that profile. Page 145 additionally
distinguishes global device support from controls required by each resource
realization. None of this makes absent KNXPROD `Data` evidence of optional
transfer. Resources v01.10.01, §4.23.3, page 297, says the
memory-mapped load-state-machine realization is not specified in that document.
Together, these references do not establish whether AS-0700 bytes or the
AS-099A RAM byte need a payload, are initialized by another mechanism, are
preserved, or may be omitted for this product.

XML Data Encoding v01.01.01 describes XML encoding for exchanging data between
KNXnet/IP instances; it is not the KNXPROD project-schema definition for
optional application-memory `Data`. It therefore does not resolve the observed
omissions. The remaining AC-002 question is limited to the required content and
initialization/transfer semantics, if any, for these two procedure ranges under
the selected product profile. AC-002 stays INCONCLUSIVE, Task-023 stays
Blocked, and the fallback remains unqualified parser input.

Private source identities (SHA-256): Resources
`1142650dd3f9a820a47a4c710b9de829b00f145d62091423b1f69349ef138fe7`;
Management Procedures
`a59e8ad58a4a0a0b0b755d5be255fbf03268afc1dbf684792da035a168ff92fe`;
Profiles
`39d1258c1d9177178ad6e7f597adef8c9e0696c112ade54ad27d604adf1d8d14`;
XML Data Encoding
`8587bac20f638fdbe48721d19703f953089288fd2267229a0c0c1d5c489536dd`.

## Omitted-data range references, 2026-10-21

A follow-up bounded static inspection, independently reproduced by the
coordinator against the archive and selected XML hashes above, narrows the
unresolved resource question. Each omitted-data segment ID occurs as an exact
attribute value only on its own `AbsoluteSegment/@Id`; neither is a table or
parameter memory target. All 539 `Memory` elements target AS-4690. The
communication-object, address and association tables target AS-4400, AS-4000
and AS-4201 respectively, each at offset zero; all four segments have Data.

The omitted ranges are contiguous: AS-0700 spans [1792, 2458), and AS-099A
spans [2458, 2459). `ProductProcedure/LdCtrlAbsSegment` includes each exact
address/size pair. Both entries have `LsmIdx=3`, `Access=0`, `MemType=2` and
`SegFlags=0`; their `SegType` values are 0 and 1 respectively. These numeric
observations do not interpret the load-control fields or establish that omitted
Data means zero-fill, optional transfer or an incomplete manufacturer package.

Task-023 AC-002 requires all **required** binary resources, rather than Data in
every declared segment. The existing parser preserves absent Data explicitly;
no universal Data-required rule was added. The remaining question is which
bytes, if any, these two custom-procedure ranges require. Public primary
documentation reviewed so far does not resolve that question. AC-002 remains
INCONCLUSIVE; parser support for this candidate and device download remain
unqualified. No implementation, default bounds or phase gate changed.


## Original project/20 schema evidence, 2026-10-22

The earlier lack of evidence for optional segment Data is superseded for the
selected project/20 reference. A private copy of the installed ETS 6.4.8718.0
`Knx.Ets.Xml.ObjectModel.dll` is 4,967,888 bytes, SHA-256
`db6f21679d046eaef4909121a553af52fa377fa582552faca6ec0fed5f7f51e1`.
The bounded static byte-slice observation did not load or execute that assembly.
Its exact project/20 XSD slice starts at byte 4,168,168, is 277,963 bytes, and
has SHA-256
`5dce7ad6f1c742f3772963c9ca00f7fd5809ae9538b4993fa816266cdc55de8a`.
Private provenance and declarations are recorded under
`private/evidence/task023-schema-original/`, including
`schema-fragments.json` and `selected-schema-declarations.json`.
No original DLL or XSD is included in Git.

The schema's AbsoluteSegment extends SegmentBase_t. Its Data and Mask child
elements are optional (minOccurs=0); segment Id, Size and Address are required.
Consequently, the selected application's two absent Data values are structurally
valid absence, retained as None. This establishes neither zero-fill nor initial
RAM contents, transfer semantics, a complete image or a valid load procedure.
It does not qualify the distinct project/11 Light reference.

Independent critical review verified the pinned DLL, exact slice, namespace
and declarations. AC-002 remains pending the separate bounded external-resource
reference inventory; schema validity alone does not validate baggage payloads.
Existing downstream evaluation/load/compilation blockers remain enforced.

The coordinator reproduced the schema assertion with
`python3 private/evidence/task023-schema-original/verify_project20_schema.py`
(exit 0). The private owned script SHA-256 is
`1e8f9d792e029dfebc43b458981306e56ca69412177bf8010c37632c86d8c1f6`;
its JSON result SHA-256 is
`260ddc01acb7d2a882192cfe5402a25ff042c74cd2625b22e5b0b324460ddca7`.
The script compares exact pinned bytes and schema declarations only; it makes
no initialization or executable-host claim.


## Final Task-023 structural outcome, 2026-10-22

Task-023 is Done for selected GlassV31/project20 structural import after
independent verification and review PASS. Source:
`3a1d5db5609c7c4aee4997b5127e1858a58c5426`. The schema proof resolves optional Data absence;
the typed join validates all 93 references and 48 payloads (56,378 bytes), with
independently checked identity/length/hash vector
`97fb12d42558d742119cc098cfdcd8ec693f29accf1fa00222db51fd9c9959ab`.
This supersedes this reference investigation's earlier resource-completeness
INCONCLUSIVE outcome only. No missing bytes are synthesized. Compilation/load
semantics, actual device match, project/11 support, a complete downloadable image
and hardware behavior remain unqualified. See the final [task record](../tasks/Task-023.md)
and [parser evidence](POC-C-GLASS-PARSER.md) for tests and remaining boundaries.
