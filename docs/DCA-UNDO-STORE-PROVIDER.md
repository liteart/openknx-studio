# Task-009 Undo store-provider declaration inquiry

## Readiness and purpose

Actual20a at `eb3b8cfe23950f1bd94e4bdc0f9ca54f647fc066` ended FAIL10:
Project construction reported an unregistered
`Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider`. Its result and
independent reviews are recorded by `a504882`. Task-008 and the earlier
provisioning/compatibility predecessors are accepted; no phase gate applies
or advances. Task-009 remains In progress with every AC unchecked.

Existing UndoManager metadata already identifies public get_StoreProvider
06000B9E and field04000357 with this type. It does not identify the provider
declaration, visibility, members or behavior. Private readiness note
`task009-undo-provider-readiness/readiness.md` (SHA-256
`5f7c29a8594daa0790d07c2f0536b15633dc2df44da12585f90969bdbabf0b2e`)
records the checked references and unknowns. Repeating the old fixed
UndoManager-root query would not answer this question.

This increment adds only a fixed metadata query, experiment
`project-undo-store-provider`, for the newly observed type. It is directly
required by Task-009 dependency discovery. It neither implements nor registers
a provider, invokes a constructor, retries original20a, or changes task criteria.

## Exact input and observation boundary

- Input: Knx.Ets.ObjectModel.dll, 14,204,880 bytes, SHA-256
  `73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`.
- Identity: Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
  PublicKeyToken=61439873ec5e1159.
- Fixed root: Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider.

Use PEReader/System.Reflection.Metadata after exact size/hash checks, and
validate identity before selecting exactly one root. No Assembly.Load,
reflection invocation, constructor, field value, custom attribute execution,
method body, dependency loading or implementation search. A namespace/name
prefix is not evidence of public interface status.

Report root token/name/flags/generic arity, direct interface names and bounded
public declared method flags/signatures, including calling convention, generic
arity, return and parameter types. Accessors may be reported as methods.
Do not follow interface/base closure; label referenced declarations unresolved.
Missing, nonpublic, noninterface or unsupported shapes must fail explicitly or
be reported unsupported without claiming a usable contract. No guessed tokens,
fixture behavior, ownership, persistence or undo semantics.

Preserve existing limits: file16MiB/aggregate32MiB, strings1024,
signatures4096/nodes128/depth16, direct interfaces32, declared methods256,
escaped report512KiB. Only this new exact profile adopts the already evidenced
same-input MethodDef65536 ceiling; other tables remain32768. Every old mode and
its limits/output behavior remain unchanged.

## Implementation and tests

Assigned source paths are EntrypointMetadata.cs, OwnedEntrypointFixtures.cs,
Test-EntrypointMetadata.ps1 and new Read-ProjectUndoStoreProviderMetadata.ps1
under experiments/dca-integration, plus run_probe.py and test_run_probe.py
under experiments/dca-metadata. No constructor/runtime source, dependency,
assembly configuration, protocol or DCA service change.

Use a distinct unsigned owned identity/root. Cover declared signatures and
unresolved direct interfaces; identity/hash/root/visibility/type negatives;
malformed metadata/signatures and applicable exact bounds; original-vs-owned
route separation; wrapper/guard selection and all prior report-byte preservation.
Do not alter old fixture families to conceal a regression. Run the complete
owned PowerShell metadata suite, Python guard suite, parsing, scope and whitespace
checks. Freeze source and record actual outcomes before independent verification,
critical review and a local source commit.

Only afterward may the coordinator run one separately frozen original metadata
query with the existing guarded run_probe path: unchanged deny-default profile,
owned write/network denial probes first, 30-second/2MiB child bounds, private
stage and outputs. If the guard fails, record NOT RUN/affected prerequisite;
do not relax it or read the DLL through an unguarded substitute. Owner-reported
private-copy permission remains recorded in ADR-005; DLL redistribution stays
prohibited and no legal-document gate is reopened.

Status: contract prepared; new implementation/tests/extraction NOT RUN.
Even a successful declaration report will leave behavior, inherited/external
contracts, fixture suitability and SDK/DCA operation unqualified. No Windows
vendor execution, intentional device operation, gate or task acceptance follows.

Environment prerequisite: owned restriction probes from the immutable a504882
guard passed on the current macOS host with PowerShell7.6.7, write_denied true
and network_denied true. Private readiness-probes evidence retains the exact
baseline source and raw child output. No SDK bytes or metadata were accessed;
the future new query must still run its own unchanged restriction probes.


## Source21 owned checkpoint

The six assigned source paths are frozen by private `source-freeze21.json`
(4,149 bytes, SHA-256 `e910bc97ad57e411f76498904c5a17adb1904bfe673010ff0c20c97c4dcae2aa`).
`source21/evidence-manifest.json` binds25 evidence files and the source snapshots;
freeze-time contract bytes remain in `contract-at-freeze21.md`. Source commit
and independent verification/review are pending at this checkpoint.

Actual owned checks PASS: PowerShell metadata suite370/FAIL0, Python guard
suite24, both PowerShell parses, both Python AST parses, preservation, scope and
whitespace. Exact captured commands are in `source21/commands.json`: the pinned
PowerShell7.6.7 runs `Test-EntrypointMetadata.ps1`; Python runs
`python3 -B -m unittest discover -s experiments/dca-metadata -p test_run_probe.py`.
`source21/check_source21.py` verifies old reader/fixture/test/guard byte behavior
after removing only the new blocks, and all10 constructor runtime sources.
Old-mode report byte equality is also exercised by the full owned suite.

Three initial PowerShell failures remain retained: strict root scanning rejected
standard Module metadata, an overly specific row-exception test oracle needed
exact table/count checks, and a reserved PowerShell variable was renamed. Final
fourth run PASS; no SDK data was used to repair those owned observations.

Original provider metadata extraction, SDK binary reads by this new route,
Windows/vendor runtime and hardware remain NOT RUN. Provider declarations and
behavior remain UNKNOWN until actual evidence exists. The inherited report
rights marker UNRESOLVED is parser metadata, not a new authorization decision;
ADR-005 already records permitted private copying and no DLL redistribution.


### Source21 independent verification and review

Independent `verifier21/independent-audit.txt` (3,947 bytes, SHA-256
`3421d1d26934471edf6f201b1fb26b914e4567aa91d00e7dd90c95a0aa6768df`)
PASS: six source/snapshot pins, all25 evidence bindings, frozen contract,
reproduced370 owned PowerShell and24 Python tests, parsing, preservation, scope
and whitespace. No SDK bytes or target code were used by verification.

Critical source/owned-evidence review `review21/critical-source.txt` (8,919 bytes,
SHA-256 `f2d938c58da914f04718fff8d8dbdd34fb4536ad61f0f8267dbc700102257afe`)
PASS, no unresolved findings. The optional fixed inquiry wrapper
`run_original21.py` also passed static review (SHA-256
`0ef6239695ed68affd83bf72e7754773ac5674f695c23a879ba8da9e2e8d53d9`),
without invocation. It pins the freeze, six committed/current sources, clean
revision and saved review digest, delegates the unchanged guard, captures raw
child results exclusively and checks original input integrity. Parent hashing
and copying are provenance operations; PE decoding occurs only in the restricted
child after fresh denial probes. Source commit precedes that single inquiry.

These reviews accept the preparation increment only. Provider declarations
remain UNKNOWN and the original metadata query remains NOT RUN at this entry.
