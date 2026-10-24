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
