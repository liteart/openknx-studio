# Task-009 project stream-store declaration inquiry

## Readiness and scope

Task-008 is accepted; Task-009 is In progress with all acceptance criteria
unchecked and no phase gate. Actual original25 at planning commit622b466,
implementation6e2a920, ended FAIL10. Registration, container Build and the
genuine Factory returned; Project did not return. Its next denied callback was
IProjectStore.get_LargeStringCache06000313. The existing services report already
verifies that getter returns Knx.Ets.ObjectModel.IStreamStore. Do not repeat
the getter inquiry or confuse it with IObjectContext's concrete cache return.

Private readiness note task009-large-string-cache-readiness/readiness.md,
SHA-2561dac1861b90bb79610380647436c2ff5a35b75fd17e9fec6512c5f933e3f15b5,
finds no IStreamStore root declaration in the inspected retained reports.
Its kind, members and behavior remain UNKNOWN. This increment establishes
declarations only; it supplies no cache fixture or positive service response.

## Fixed input and output

Add exactly one fixed selector, project-stream-store, with root
Knx.Ets.ObjectModel.IStreamStore. Input Knx.Ets.ObjectModel.dll is14,204,880
bytes, SHA-25673ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a,
identity Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159. Validate size/hash before identity/root selection.
Use a distinct unsigned owned identity/root for tests.

Use PEReader/System.Reflection.Metadata only. Report the root TypeDef token,
name, raw flags, visibility, generic arity and actual class-semantics/interface
flag. Do not infer interface status from its name. A class-semantics flag alone
does not distinguish ordinary class, value type or enum: report the unresolved
base reference. Report direct interface identities and bounded public declared
method tokens/names/raw flags/signature headers/calling conventions, instance
or static flag, generic arity, ordered parameters and return identities.
Public constructors may appear as declarations only. Unsupported visibility,
generic or signature shapes must be explicit refusals/observations, never a
claim of usability. Base/interface references remain UNRESOLVED_NOT_FOLLOWED.

No Assembly.Load, reflection invocation, constructor execution, fields/layout,
custom attribute execution, IL/bodies, dependency loading, implementation search,
inherited closure, path contents, stream bytes or inferred store semantics.
No arbitrary root/worker/dependency/assembly options or automatic expansion.

Preserve existing bounds: file16MiB/aggregate32MiB, rows32768 except the exact
new ObjectModel profile's MethodDef65536, strings1024, signature text4096,
signature nodes128/depth16 and existing strict blob precheck128 bytes, direct
interfaces32, public declared methods256, report512*1024 characters. All old
modes, limits and report bytes remain unchanged. Do not raise limits on failure.

## Source26 implementation and verification

Owned paths: experiments/dca-integration/EntrypointMetadata.cs,
OwnedEntrypointFixtures.cs, Test-EntrypointMetadata.ps1 and new
Read-ProjectStreamStoreMetadata.ps1; experiments/dca-metadata/run_probe.py and
test_run_probe.py. No constructor source, dependencies, configuration, protocol,
SDK runtime or DCA-service change.

Cover observed class/interface flags without prefix inference, unresolved
references, signature/header/order, original-versus-owned routing, hash/identity,
missing/duplicate/nonpublic roots, malformed metadata/signatures and exact
bounds. Preserve existing fixtures and old report bytes. Run complete owned
PowerShell metadata and Python guard suites, parsing, scope and whitespace.
Freeze source/evidence; independent verification and critical review precede
the local source commit. Record failures as well as final results.

Only afterward may one separately frozen original declaration inquiry use the
existing guarded run_probe path. Keep PowerShell7.6.7, unchanged deny-default
profile, fresh owned write/network denial probes first,30-second/2MiB child
bounds and private inputs/outputs. If restriction probes fail, stop that inquiry;
no relaxed or unguarded substitute. Parent hash/copy is provenance, while PE
decoding occurs only in the restricted child. No Windows vendor execution,
original25 retry, device operation, gate advance or task acceptance follows.

ADR-005's owner-reported private-copy permission and proprietary redistribution
prohibition remain unchanged. The historical rights=UNRESOLVED report marker
does not reopen provisioning or certify rights.

Status: contract prepared; source26 implementation/tests/original query NOT RUN.
Even declaration PASS leaves cache/store behavior and SDK usability unresolved.

## Source26 owned checkpoint

The six assigned sources are implemented and frozen by private
task009-project-stream-store-metadata/source-freeze26.json,7,180 bytes,
SHA-2561f4c80edf982ddb118633e8992ef1edb5fc99f6f15e1281de89d3ba2b6ed0118.
Its source26/evidence-manifest-final.json binds24 artifacts, including six
source snapshots and the exact contract checkpoint. The freeze accurately
records UNCOMMITTED_SOURCE26_AT_FREEZE; a later commit must match those pins.

Actual owned checks PASS: full PowerShell metadata suite419/FAIL0, Python guard
suite25, two PowerShell parses/two Python AST parses, exact preservation of old
reader/fixture/test/guard bytes after removing only the added blocks/routes,
all10 constructor sources unchanged, scope and whitespace. The earlier full
PowerShell run418/FAIL0 remains retained; the second adds one malformed-owned-PE
assertion. No failing source tests occurred. A packaging-only missing review
path error and initial manifest are retained separately; correcting that path
did not change or rerun source tests. Exact commands/raw outputs are bound in
the freeze and source26/commands.json.

Independent verifier26/source-audit.txt,
SHA-2561e86e3b73e114755e7aaa25243ae029c87ea22307d5800263e6d4ef82c8c79c1,
PASS: reproduced419 PowerShell and25 Python tests, parser/preservation/scope/
whitespace checks; checked all24 manifest entries, six current/snapshot pins
and frozen contract. No SDK payload bytes or original query were used.

Contract-only review26/contract-review.txt,
SHA-2565da6b8825d501a05475648176500f42c02c7e2858cc19c587f6c7be30b9ee3a8,
PASS. Final frozen source/owned-evidence review and source commit are pending.
The prepared fixed run_original26.py is AST-checked only, SHA-256
0759d79d7edfbdc76fbbd11848d975bc4511b6ea9c7a08e3c573a00367b74d63.
It requires the committed/current source pins and final review digest, records
an exclusive persistent attempt marker, and delegates the unchanged guard.
It has NOT RUN; original declarations remain UNKNOWN. No runtime retry,
positive cache/store response, qualification or gate acceptance follows.

### Source26 critical review

Final review26/critical-source.txt,4,325 bytes, SHA-256
9b645bb20989aa97ee6bad5b0d9127d28501d8ed21793822ca3f12f8ee629169,
PASS with no unresolved findings. It checks the eight-path increment, six
current/snapshot pins, all24 manifest bindings, frozen contract, independent
audit and prepared helper hashes, wrapper AST and whitespace. No SDK reads,
query or test rerun occurred by the reviewer. The source commit and its exact
committed-pin check must precede the single original declaration inquiry.

## Committed source and restriction-launch failure

Source26 is committed as4799252731f60649dda0c94fb75f67f75cfaa1ca.
source-commit26.json, SHA-256
e2bea0087ccdfa7fabc20166edce282a2d8589f71a04d1d4010b3018bba12d93,
confirms six current/committed pins and a clean worktree before original26.

The fixed original26 helper ran once and stopped during the FIRST restriction
probe child: exit71, empty stdout,53-byte stderr reporting
`sandbox-exec: sandbox_apply: Operation not permitted`. Result is
FAIL/DCAPROBE_RESTRICTION; original declaration decoding NOT RUN, report absent.
Parent hashing/copying occurred and the input hash remained unchanged; no target
load, SDK runtime, Windows execution or hardware operation occurred. Observation
SHA-256b0c1c4cb738c9e5e7c1f25b6b4e5e46edd17a39400fe33e2997a1237494b9b55
and the persistent attempt marker preserve this terminal attempt. Do not rerun it.

The context comparison indicates an inner macOS sandbox launch conflict with
the enclosing tool sandbox; this cause is inferred, not an OS policy audit.
A separate owned-only readiness command used the tool's reviewed outer
execution escalation with the EXACT SAME deny-default inner profile. It accessed
no SDK input, returned exit0/stderr0 and observed PowerShell7.6.7, write_denied
true/network_denied true. Its observation SHA-256 is
2212682c4fa03d2dd48aa09c3af6bb79c80f750e3227be788174dd989fad50fc.
This is restriction readiness, not a metadata result or relaxed fallback.

A new separately frozen original26b plan635397c6cdd89c4659db03ec8a33273e5132a2a95dc91a9566a8842c981715a8
permits one inquiry using that outer execution context, with fresh restriction
probes and unchanged source/inner profile/limits. Fixed helper SHA-256
731abc453168d53caa427a40c772769d4ef7c8509a8b318206f98cd2e9bdab28
requires the saved26 failure/readiness hashes, committed/current source pins,
source review and a separate plan review digest. It uses new exclusive output
names and attempt marker. Plan review, planning commit and original26b execution
are pending. A second restriction failure stops this new inquiry too; no
unguarded decoding is authorized. Original26 remains FAIL; declarations UNKNOWN.

Independent saved-result audit verifier26/original-result-audit.txt, SHA-256
bce81decb0c54e36e09858bafc50007ef023b4ffdf4e710ec5604bb959e19fa4,
PASS for original26 provenance, with restriction setup FAIL/declarations NOT RUN.
It confirms six source pins, committed receipt, attempt marker and child output
bindings without reading SDK payloads or rerunning the failed inquiry.

New-plan critical review26b/plan-review.txt,3,619 bytes, SHA-256
49ff03c8c5d872f872dd297a1ed488fb2186b8c4fe331711e4399ceec37fe7b5,
PASS: fixed helper/plan, prior failure, owned restriction readiness, raw outputs,
six current/committed pins, unchanged profile/limits and helper AST checked.
The causal explanation is explicitly an inference. No original query ran in
review. This clears only the fixed preparation; planning commit/current-pin
checks and fresh child denial probes still precede original26b decoding.

## Original26b guarded declaration result

At planning commit7be86dddc4c70dbcbc53560cf69dafc05511acb0, source26
implementation4799252731f60649dda0c94fb75f67f75cfaa1ca, the new fixed
inquiry executed once with reviewed outer escalation. All six current/committed
source pins matched freeze26 and the worktree was clean beforehand. The first
child freshly reported PowerShell7.6.7/write_denied true/network_denied true,
exit0/stderr0; only then did the second child decode declarations, exit0/stderr0.
The inner profile,30-second/2MiB bounds and input pins were unchanged. Original26
remains the separate failed restriction-launch attempt, without a report.

Result **PASS for declarations only**. Actual root IStreamStore is
TypeDef02000061, attributes1048737, Public, Interface/class-semantics32,
generic arity0, no base and no direct interfaces. Its public declared methods:

| Token | Name | Return | Parameters | Attributes |
|---|---|---|---|---|
| 06000366 | get_Id | String | None | 3526 |
| 06000367 | get_Exists | Boolean | None | 3526 |
| 06000368 | AsStream | System.IO.Stream | Boolean | 1478 |
| 06000369 | AsMultiWriteWithinOneTransactionStream | System.IO.Stream | None | 1478 |
| 0600036A | Delete | Void | None | 1478 |
| 0600036B | GetUtcFileTime | Int64 | None | 1478 |
| 0600036C | set_Mask | Void | UInt16 | 3526 |

All are instance/default calling convention, raw signature header32, generic
arity0. Stream references are qualified mscorlib4.0.0.0/b77a5c561934e089 in
the raw report. No referenced declarations or implementations were followed.
No method was invoked, no cache/store value or stream bytes were supplied,
and no SDK target entered a CLR load context. Parent input hash remained
unchanged; Windows/vendor runtime, original25 retry and hardware NOT RUN.

Private original26b-report.json is3,359 bytes, SHA-256
734cc04300ee84a8a2de29a4cbef9b489cc948bad7e3184d29041174faa61b07;
original26b-observation.json SHA-256
25397661a026d46aaa278ad006eda023023e3f056576c98a8c70b95a8ba246c3.
original26b-evidence-manifest.json SHA-256
f8f1f60b0eb83dcfd9c11ef3af2f6cbcf3fff4f65abf12149c74473761b7b4ee
binds69 source/owned/failed26/readiness/plan/raw-result artifacts and the exact
command. Saved raw metadata equals the report's metadata object. Independent
saved-result verification and critical result review are pending at this entry.

Declaration knowledge permits a separate readiness assessment only: whether
the observed getter can return an explicitly labeled deny-only reference to
observe the first actual callback. No positive Id/Exists/path/stream/time/Mask,
deletion, transaction or persistence behavior is authorized or inferred.
Task-009 remains In progress/all ACs unchecked; SDK/DCA usability is unqualified.

### Original26b independent saved-result verification

verifier26b/original-result-audit-corrected.txt SHA-256
82086f90fa48636497dfa7d1b612c71aabca2314770b11807097b0b8b2b29c18
PASS: all69 manifest bindings, six frozen/current/committed source pins, raw
guard/report equivalence and every root/method token/flag/signature/order.
The verifier used saved files, without SDK payload access or query rerun.

The original audit efde1d4c471ac6d0d715ddbe0bbc01fdc7cb710c5f952da1246a75b528476f36
is retained. Its final rights sentence incorrectly treated the legacy parser
marker as a current rights conclusion. The corrected copy changes only that
clause; correction provenance21b0afea7ea428fce5e9b8a29e10e13229fc82ad675a39654ceb50b9ea3a773a
preserves both hashes. ADR-005 private-copy permission/no proprietary DLL
redistribution stays unchanged, not reopened. Declaration PASS is unchanged.
Critical result/documentation review is pending.

### Original26b critical result review

review26b/original-result-review.txt,3,933 bytes, SHA-256
226c33e019080a6a8285afb2d9449e3effc26feeb8afc8e043093aa6dcef5884,
PASS with no unresolved findings. It independently checks69 evidence bindings,
six current/committed source pins, raw guard/report equivalence, all seven exact
method declarations and corrected-audit provenance. The task's stale next step
was corrected to deny-only-reference readiness. No SDK payload access, query
rerun or runtime occurred in review. Task-009 and its criteria remain open.
