# Task-011: Supply one coherent device and product context to the DCA

## Subject

Supply one coherent device and product context to the DCA

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-011 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host fixture context |
| Owner | Primary coordinator and DCA compatibility agent |
| Requirement references | [R-009](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Give the reference DCA the minimum internally consistent device context needed for one offline configuration operation.

Component contract: [DCA-REFERENCE-PRODUCT-STORE.md](../docs/DCA-REFERENCE-PRODUCT-STORE.md).

## Detailed Requirements

- DR-001: Choose exactly one application ID supported by both the DCA manifest and the acquired product fixture.
- DR-002: Supply stable fixture project/device IDs and only the required product/parameter context.
- DR-003: Record read/write accesses and distinguish host-managed values from opaque DCA state.
- DR-004: Reject mismatching application identity and missing required context.
- DR-005: Do not turn fixture services into persistent production state.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The DCA recognizes the selected device/application context.
- [ ] AC-002: Wrong application identity is rejected before configuration mutation.
- [ ] AC-003: All context values are traceable to the fixture or explicitly synthetic test identifiers.

## Dependencies to Other Tasks

- [Task-009](Task-009.md): Accepted finite offline SDK compatibility surface. Task009 is Done at closure3eac1c233341f2811d409ccce49bf066775bffa8.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

Ordering correction: [ADR-011](../docs/ADR-011-dca-context-before-ui.md). Exact
original metadata proves the DALI UI requires one coherent SDK Device first.
Task010's original initialization already passes, but its completed visible UI
test is not a prerequisite for preparing this context. No acceptance criterion,
required test or phase gate is waived. All hardware/bus operations remain denied.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: reference context initialization.
- TEST-002: Negative: mismatching app ID, missing parameter definition, and missing device context.
- TEST-003: Unit: stable IDs and consistent callback identity.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host fixture context**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No complete KNXPROD model, database, or multi-device DCA behavior.

## Risks and Blockers

The selected product fixture is available and hash-verified, and its original data
was imported into a genuine SDK device context. Its complete application ID suffix
differs from the DCA manifest declaration; compatibility and recognition are not
yet proven. The original DCA rendered and the owner interacted with its UI. A
connection attempt failed; no hardware connection or network isolation is claimed.
Source89 records a successful offline UI lifecycle
and cleanup, but does not qualify the selected application identity. See the
latest checkpoint below and the linked component contract.

## Completion Record

| Field | Current value |
|---|---|
| Implementation / report status | In progress; genuine product import and original UI observed; identity qualification and negative tests pending |
| Source commit(s) | 09cd05bf19636bb872f0c11ae24798a32d3f7c68 |
| Tests executed | Source89 original UI and clean exit PASS; 423 owned cases PASS; identity and missing-context negatives pending |
| Evidence location | private/evidence/task010-sdk-wrapper/iteration01/run-56aa362ec007 and run-60e2151ed943 |
| Acceptance criteria met | None asserted |
| Technical outcome | Genuine imported product and SDK device supplied; DCA/application match unqualified |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent agent review of Source88 and Source89 fixes completed; maintainer acceptance pending |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Original import helper checkpoint

Source29 adds OriginalProductImportProbe.cs and build wiring, a separate
original-sdk-product-store-fixture mode and an explicit45-second launcher bound.
It wraps only the original ZIP/product stores, real Autofac store registrations,
original GUID/HMAC writer, master-data loader and vendor importer/cache path.
The source archive is a fixed sibling of the DLL input stage; outputs stay under
the fresh outer scratch directory. Original profiles retain their existing bounds.

The first local compilation failed on a CSharp5 rethrow-in-finally restriction;
corrected second and final third compilations each passed all3 targets. Both
PowerShell scripts parse and git diff whitespace checks pass. Source29 pinned
Windows BUILD ONLY run `d4c873af26b1` also PASS:9 sources,18 artifacts. No original
assembly was executed by that build. Exact local commands/source hashes are in
private source29-import-handoff.json, SHA256
`b9178f883f682e57c80e0d081a8fafdba2faa663876481400330883491ed2303`.

The new mode unconditionally reports SDK_PRODUCT_IMPORT_DEPENDENCY_NOT_PROVISIONED
before original identity/load/effects until the real DotNetZip binary is qualified.
No guessed hash is present. The source-host control connection stopped responding;
a fresh SSH connection reached that host but authentication was unavailable. The
stale task-owned master was closed and the owner was asked to log in again.
Last control-socket check: absent. Runtime import remains blocked by that input;
this checkpoint is not a successful import, DCA recognition or task acceptance.

Blocker revalidation, next goal turn: source94 control socket is still absent.
Read-only bounded central-directory inspection of the existing reference-tools
ZIP, SDK ZIP and two original DCA archives found no DotNetZip/Ionic.Zip entry.
Pinned PE-only resource-name inspection of Common, SDK and ObjectModel found no
DotNetZip/Ionic.Zip/Costura-named resource. No contents were extracted or vendor
code executed, and this does not claim arbitrary encoded resources are absent.
Private results: task011-product-store/local-packed-dependency-inventory.json
and embedded-dependency-names.json. The external provisioning dependency remains;
Source29 code/build evidence is unchanged and original import/UI is still NOT RUN.

After the owner restored source94 SSH, original DotNetZip1.17 was privately copied
and qualified for the selected ZIP bootstrap. Source30 adds only its exact
product-mode pin (34 DLLs,34110536 bytes; prior33 modes unchanged). Provisioning,
archive pin and Windows build PASS. Run `ab9e51d7c630` stopped before import because
seven shared selectedMode predicates omitted the new mode. Source31 changes only
those exact memberships, with a reverse-substitution proof to Source30.

Run `8fe3a94b16d2`: Windows build and original DCA Initialize PASS; the helper
reaches PRODUCT_IMPORT_API_READY and DotNetZip load passes its exact pin audit.
Common.ZipWrapper then requests System.Text.Encoding.CodePages9.0.0.0; the
unqualified dependency is refused. Cleanup additionally requests WriteScope380,
still unsupported. Whole run FAIL2,34+2 pins and processes0 PASS. Read-only output
inventory found two directories but zero files/bytes; the separate archive hash
remained unchanged. No source/cache import milestone or UI success is claimed.

The actual CodePages candidate was privately copied for PE qualification:
766216 bytes, SHA256
`a01c215594d3a98d11bc4a776d4767536e004aac1f2c3d16e943b12d19540680`.
Primary Common helper13E registers that provider unconditionally, so choosing an
explicit encoding cannot bypass it legitimately. Selected provider bootstrap is
managed dictionary/lock construction; declared kernel32 locale/text calls are
recorded rather than misreported as absent. Source32 will add the exact
product-only35th pin and a real owner-bound nonreentrant write-scope lease.

Source32 run `3c0a7d85be12`: provision35 plus unchanged archive, Windows build,
original Initialize, both new dependency load audits and genuine product-store
construction PASS. The next step raises a duplicate-key ArgumentException before
PRODUCT_IMPORT_MASTER_BEGIN; the exact failing call is not yet proven. WriteScope
now returns a real owned lease, but later content-store final denial remains
visible. Whole run FAIL2,35+2 pins and processes0 PASS; scratch contains the owned
output directory. No master-load/import/product-recognition/UI success is claimed.
Source33 will record bounded per-stage markers and method/token-only exception
frames before changing the actual duplicate-key behavior.

Source33 local3 CSharp5/x64/Werror compilations,2 PS parses and diff check PASS.
The Windows build preparation for run `61a7be93d240` could not be observed:
SSH timed out40 seconds into the fresh-directory check/creation command, before
any source transfer/build result or original-runtime intent was recorded. The
Dev60 control socket subsequently was absent; a fresh transport reached the host
but authentication was unavailable. The owner was asked to reconnect Dev60.
Remote preparation state is UNKNOWN, not proven failed/stopped. Inspect those
exact paths after login before continuing; do not restart because observation
timed out. All qualified dependencies are already copied; Source94 is not the
current blocker. No Source33 original import/UI/hardware was started locally.

After reconnection, the exact Source33 paths and processes were absent. The same
run `61a7be93d240` then built successfully on Windows and ran the original DCA
initialization and product diagnostic. The original master stream opened, but
its hash stage failed with a duplicate ZIP entry: the captured stack includes
`ZipSingleFileStore.EntryWrapperStream.Dispose`, `ZipWrapper.AddEntry`, and
`Ionic.Zip.ZipFile.InternalAddEntry`. The original stream API semantics require
investigation before a correction. The run exited 2; the final content-store
denial also remains unresolved. All 35 private inputs and two OS pins remained
unchanged; the scheduled task was removed and no owned process remained. The
log SHA256 is `b40f110ce79baa75cf24ab550a2fb2c82f49678a97409af543e17b3fa18c0e00`.
Original UI, device recognition, import completion, and hardware remain NOT RUN.

Source34 corrects the verification call to the original `AsStream(isReadOnly)`
with `true`, established from the pinned SDK method metadata and implementation.
Local CSharp5/x64 builds and Windows build PASS. Run `5da926cd34a1` verified and
loaded the original archive master data, then failed with a denied request for
`Knx.Ets.OnlineCatalog.Download` during the next SDK construction step. Its role
in the local import path and offline alternatives are under investigation; this
does not authorize or implement an online catalog. Exit2, 35 private and two OS
input pins unchanged, task removed and owned processes0. The separate cleanup
denial persists. Log SHA256
`8a4b187b690f7cbedac9650a5c44c193666331e828e4a060dc104874ce8c3d0a`.
Product import, device recognition, UI and hardware remain unproven.

The bounded original metadata inquiry identifies the request at the genuine
`Caches` constructor after master load. Its cache interface exposes two types
from the requested assembly; no catalog download call appears in that constructor.
The precise CLR resolution trigger is not proven. The importer null-cache path
constructs the same cache, so no cache-free alternative is established. Further
qualification requires the original DLL from the ETS source host; the source
SSH socket is absent and independent authentication is denied. Private report:
`task011-product-store/online-catalog-local-import-readiness35.md`, SHA256
`4839b0f820a7c03c06308f85397e1e3f520f592178ef342dc2c21816f330507b`.

The required original DLL was copied privately and qualified: the two referenced
types are enums and the examined module initializer calls a no-op helper. Its
other native imports remain outside that bounded finding. Source36 adds the exact
private dependency pin, without an online catalog feature. Provision36 and Windows
build PASS. Run `30c43ea6b5f1` loaded master data, constructed the original caches
and importer and entered the local product import. It then denied an en-GB
ObjectModel resource request outside the currently scoped fallback; the required
fallback is under investigation. Exit2, 36 private and two OS pins unchanged,
scheduled task removed and owned processes0. The separate cleanup denial persists.
Log SHA256 `8880c1abb783384db302221b27feea0630c788835a7be5ad5ff83ac198898772`.
Import completion, DCA recognition, UI and hardware remain unproven.

Source37 scopes neutral resource fallback to the original import call, with
identity, thread and probe bounds. Local compilation and Windows build PASS.
Run `8c47fb6247a0` proceeds through four bounded resource probes and then fails
with `MISSING_PATH` in the owned cache path check, reached from the original
manufacturer cache constructor. Directory lifecycle is under investigation.
Exit2, 36 private and two OS pins unchanged, task removed, owned processes0;
cleanup denial remains. Log SHA256
`92d09e80568ee0e716295b6df523ab0ff58973448eb14fddbbc7765cbf5fd63f`.

Source38 creates only validated original-selected cache directories within the
private fixture root. Local and Windows builds PASS. Run `d281a7023f49` advances
past that path and requests `Knx.Ets.Xml.ObjectModel`; the matching original DLL
was copied privately for PE qualification. Exit2, 36 private and two OS pins
unchanged, task removed, owned processes0; cleanup denial remains. Log SHA256
`f63c9bd62ae57ec33a34d550023398552cb612993e163220b9eeff2c4ae024dc`.

Source39 qualifies the selected XML namespace and original initialization path,
then pins the unmodified original dependency for the product fixture. Local and
Windows builds and provisioning PASS. Run `7f229c2fb2e6` loads it during import,
then requests `Knx.Ets.CvNext`, now privately copied for qualification. Exit2,
37 private and two OS pins unchanged, task removed and owned processes0; the
separate cleanup denial persists. Log SHA256
`9bf863ec2cd1527a8a72b328882323b1978f125c30cdd4c695e33d362bacf2dd`.

Source40 qualifies and pins the requested `CvNext` dependency. Provision38 and
local and Windows builds PASS. Run `8e430f3c00aa` loads it and reaches original
import namespace validation, which rejects the fixture's project/20 namespace.
The original SDK conversion path is under investigation. Exit2, 38 private and
two OS pins unchanged, task removed and owned processes0; the cleanup denial
persists. Log SHA256
`387aaf3d1c7d376490f2c30f6a4793de0bb5d9b64bb22895c79f07c7c77469f1`.

Source41 wires the original SDK conversion into a separate derived archive,
retaining the source archive and its hash. The three additional conversion
dependencies are privately pinned; FluentResults was already in the baseline.
Local and Windows builds and provisioning PASS. Run `20c4d0e319cc` stopped
before conversion because the owned host looked for CvNext in the stage root
instead of the pinned input directory. Correcting that path is in progress.
Exit2, 41 private and two OS pins unchanged, task removed and owned processes0;
cleanup denial persists. Log SHA256
`1ce5db8073b8187be5645c65026087c2de306219909be361f4896786ba7500c3`.

Source42 resolves CvNext through the exact pinned input loader. Local and Windows
builds PASS. Run `5d60d387e3e2` starts the original converter, which fails while
extracting with Windows `PathTooLongException`; a shorter bounded temporary path
is under investigation. Exit2, 41 private and two OS pins unchanged, task removed
and owned processes0; cleanup denial persists. Log SHA256
`6e397521aff13f6eef97d97e810b408b37050b3c583234d9a123103a56da84a0`.

Source43 uses an owned short conversion temp root with terminal inventory and
checked cleanup. Local and Windows builds PASS. Run `918305ffb198` proceeds past
the path-length failure, then requests Logging.Abstractions 2.1.1 from the
original FluentResults initializer. The ETS host's config redirects this identity
to its version 9; selected ABI and bootstrap are under investigation. Exit2,
41 private and two OS pins unchanged, task removed, owned processes0. The short
temp root was inventoried and removed after child exit; separate project cleanup
denial persists. Log SHA256
`b2c5f24de18104faa066a48995c0ce46b061fe0f8d7239887c478c4ebbf6ad6b`.

Source44 applies a product-scoped exact logging version mapping supported by the
ETS source configuration. Local and Windows builds and provision42 PASS. Run
`c7409cfd134a` successfully converted the source to a separate archive, verified
the unchanged source and loaded converted master data. Original import then
stopped at an Autofac en-GB satellite request; bounded fallback is under review.
Exit2, 42 private and two OS pins unchanged, task removed, owned processes0 and
short temp removed after inventory. The separate project cleanup denial remains.
Log SHA256 `273b0b2aa38c9460df990cf88d660c4f135f90b05164756e0427c50601498191`.

Source45 scopes Autofac neutral resource fallback to original import. Local and
Windows builds PASS. Run `2e1e8162f20c` converts the archive, loads the converted
master data and reaches original catalog item processing. The original factory
then reports that `KnxMasterData` is not registered in its service container;
registration of the actual loaded object is under investigation. Exit2, 42
private and two OS pins unchanged, task removed, owned processes0, short temp
inventoried and removed; the separate project cleanup denial persists. Log SHA256
`36d86c035693b459a8a976bb5513187c1bfe62e5560c0776e604e11e0ecae884`.

Source46 registers a deferred typed service before the original factory container
is built, binding the genuine master data only after it loads. Local and Windows
builds PASS. Run `c09299ad9752` advances to original hardware merge, then the
owned path validator refuses creator arguments; exact SDK arguments are under
investigation. Exit2, 42 private and two OS pins unchanged, task removed, owned
processes0, short temp inventoried and removed; separate project cleanup denial
persists. Log SHA256
`b64ad6e4947350d8651c411287572f625a709af48da3fa4127c5717f7b6e4ecc`.

Source47 structural diagnostics found the same denial but no first-argument
record. Source48 records the first callback failure before latching its fault.
Run `6d3aa64252c0` shows the initial failure is an original progress callback on
thread6 while the host owner is thread1, before any prior fault; the later creator
denial follows. Bounded background progress handling is under implementation.
Exit2, 42 private and two OS pins unchanged, task removed, owned processes0,
short temp inventoried and removed; project cleanup denial remains. Log SHA256
`06777e75c570ee05a85da9bd3060ee3d0afbd6058088bb2f3565c6f3a4d9a74c`.

Source49 accepts bounded background progress notifications during original
import. Local and Windows builds PASS. Run `a11cab7c92d3` converted the source,
completed the original product import, and bound and attached the genuine
installation and device objects. A subsequent owned trace postcondition failed;
its expected counts are under investigation. Import cleanup reports completed,
while the separate project content-store cleanup denial persists. Whole run
exit2, 42 private and two OS pins unchanged, task removed, owned processes0,
short temp inventoried and removed. Log SHA256
`c6e89360f86c7c26a83becc882b47f572dd3177c83ddaab1b120a70368e460e1`.
SDK device wrapper, DCA recognition and UI remain unproven.

Source50 adds bounded provenance capture without changing dependency admission.
Run `e8ece78db497` repeats successful conversion, import and device binding.
The recorded events show Autofac expression compilation creates an anonymous
dynamic assembly, and the .NET XML serializer probes an optional serializer and
generates a dynamic assembly during original SDK conversion/signing. These were
still rejected by the existing audit; narrow provenance-based handling is under
investigation. Whole run exit2, 42 private and two OS pins unchanged, task
removed, owned processes0, short temp inventoried and removed; project cleanup
denial persists. Log SHA256
`f82a2c3a65cb5d33f2b70791345d6d5a96863eeb5a0ad8827ead6a9e93fea982`;
provenance SHA256
`b48cea258daebf1eaa1a4052a54537e9a5e4a9f1d771c509ad4801213c9864f1`.

Source51 scopes handling of the observed generated assemblies and optional
serializer probe to their verified framework and pinned SDK call chains. Local
and Windows builds PASS. Run `5e0d866c5f7f` completes conversion and original
product import, binds and attaches the genuine device, and advances beyond the
dependency trace check. The SDK device product lookup returns null; linkage to
the imported product is under investigation. Exit2, 42 private and two OS pins
unchanged, task removed, owned processes0, short temp inventoried and removed;
project cleanup denial persists. Log SHA256
`b3cf0a1c0b04210d79b1a0382d57253e10f6db563f4ac4ebc730afa309a2a93b`.

Source52 attaches the selected genuine manufacturer from loaded master data to
the project. Local and Windows builds PASS. Run `68aa4761f151` completes import,
attaches the manufacturer and device, and reaches the product lookup; it requests
original `ICacheProvider.CacheProvider` which the fixture currently denies.
The real imported cache provider route is under investigation. Exit2, 42 private
and two OS pins unchanged, task removed, owned processes0, short temp inventoried
and removed. Log SHA256
`89ff80cec8eded57f1bebd9320936c9e901c50d0b1af36f9f88d2aa96997df4e`.

Source53 delegates the selected manufacturer's cache-provider request to the
genuine imported cache. Local and Windows builds PASS. Run `e76bf49ed24b`
completes original import and obtains the genuine SDK device and product. The
application match and DCA recognition remain unqualified. During disposal the
context adapter reaches entry33 with a 32-entry limit and latches a denial;
the bounded product-mode budget is under investigation. Whole run exit2, 42
private and two OS pins unchanged, task removed and owned processes0. Log SHA256
`02bb54da1fa22eecb697704de59355530eb866d89a6aecfc6e4e29c82e538750`.

Source54 raises only the product fixture context callback budget to64. Local and
Windows builds PASS. Run `4da8a175c62d` obtains the genuine SDK device and product;
33 context callbacks and replies complete without a context fault. The project
store still rejects a separate unidentified disposal request, leading to final
denials. Exit2, 42 private and two OS pins unchanged, task removed and owned
processes0. Original UI has not been invoked; application match and DCA recognition
remain unqualified. Log SHA256
`2f1c1937e9c5711b20250691760bd92df5eade7af7fb2f83b59accf13b6fde8f`.


## Original UI and context checkpoint — 2026-11-02

Source75 runs `07c0bbcf827f` and `d837f91b763a` imported the original private
product data, constructed a genuine SDK device context, and rendered the original
DCA control. Simon reported successful UI interaction and dialog opening in the
second run. Neither rendering nor interaction establishes that the differing
application identities match, or that the DCA recognized a qualified device.
The second run logged a failed device connection attempt; no successful hardware
connection is asserted. The original product-store cleanup also needs correction.
All acceptance criteria and the identity-negative tests remain open. See Task010
for the source-audit hash and private run evidence.


Source88 `02d5ec78d2cb` completed the original UI lifecycle with child exit 0
and clean final fixture audits. Application identity matching and Task011 negative
tests remain unproven; the temporary SDK store is not accepted persistent state.
See Task010 for the private source audit and run evidence.


## DCA-side identity-call scan — 2026-11-02

A static metadata/IL scan of the pinned original DCA assembly examined 1,510
method bodies with zero bounded omissions. It found no direct references to
`Knx.Ets.Ids` or `ApplicationProgramId` and no selected equality or shape calls.
This constrains the DCA assembly only; it does not establish the ETS-side
manifest selection rule. Private evidence and exact command are recorded in
`private/evidence/task010-sdk-route/dca-application-match-callscan-task011-01.md`
(SHA-256 `d381550a2ef6e73b6cb3bb3885bfec015c11c5b48159f0ae33bc81f9110ad29c`).
The selected application match and negative pre-mutation test remain unqualified.
