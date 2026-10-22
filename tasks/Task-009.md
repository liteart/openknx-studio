# Task-009: Implement only the required offline SDK compatibility surface

## Subject

Implement only the required offline SDK compatibility surface

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-009 |
| Status | Blocked |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host (C#/.NET) |
| Owner | Unassigned |
| Requirement references | [R-001](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md) |

## Description

Supply the smallest lawful SDK-compatible services needed to progress reference DCA initialization.

## Detailed Requirements

- DR-001: Implement only the approved contracts identified by Task-008.
- DR-002: Use explicit fixture responses for offline project/product services and label them as fixtures in diagnostics.
- DR-003: Fail unsupported methods with named diagnostics; never return generic success/null for an unimplemented required operation.
- DR-004: Record requested services/callback order and assembly binding failures.
- DR-005: Keep all bus methods denied and preserve exact original DLL bytes.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Required shim contracts are covered by tests and map to the actual dependency inventory.
- [ ] AC-002: Unknown service requests fail deterministically with no external side effect.
- [ ] AC-003: No shim contains KNX encoding or hidden ETS executable calls.

## Dependencies to Other Tasks

- [Task-008](Task-008.md): Map the actual ETS SDK surface used by the reference DCA. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: approved interface behavior and callback lifecycle.
- TEST-002: Negative: invoke every unsupported service category and confirm explicit failure.
- TEST-003: Static: compare original DLL hash before/after and scan forbidden dependencies.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host (C#/.NET)**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No broad ETS clone, live bus access, or production-quality adapter yet.

## Risks and Blockers

The exact ObjectModel candidate has been privately acquired and the Project constructor declaration has guarded metadata evidence. A compatible construction route and fixture semantics remain unresolved. ADR-005 already governs owner-authorized private use; no legal-document gate is reopened. Do not replace the missing concrete SDK functionality with a guessed object or fixture.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Blocked; guarded static Project qualification implemented, no SDK service |
| Source commit(s) | Current increment baseline `4808d8a797a122dd815f388c60d2afdcc5a7d7b5`; reviewed implementation/result commit pending |
| Tests executed | Metadata-only owned PowerShell 23 cases PASS; Python harness 14 tests PASS; guarded three-input Project extraction PASS; scope and whitespace checks PASS. SDK service, Windows execution and hardware tests NOT RUN |
| Evidence location | [Project qualification](../docs/DCA-PROJECT-CONTEXT.md); private `task009-objectmodel` inventory and guarded report; dated records below |
| Acceptance criteria met | None asserted |
| Technical outcome | ObjectModel file/hash/identity acquired; Project internal constructor and parameters observed; public construction/factory route and runtime compatibility unresolved |
| Gate outcome, when applicable | No gate applies or advances |
| Reviewer | Prior blocker report: predefined `binding_review` PASS; current metadata increment review pending |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Readiness and blocker record, 2026-10-22

Task-008 is accepted as dependency analysis at reviewed source
`89b08d24631021eb9863e7f1737753af0b8cfbd9` and result commit `73b869c56855851512badb3a70841dcd76c1fbd5`.
Its actual initialization remains FAIL (15-second timeout, exit 124), with
compatibility INCONCLUSIVE. Before the timeout, the selected runtime rejected
`Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, then logged a request for
`IInitializationContext.get_Project` (`0600002F`), whose concrete return type
is `Knx.Ets.Sdk.Project.Project`. No Initialize end or exception/result marker
was captured. The timeout cause is unknown; these observations do not prove
that the rejected assembly caused the timeout.

The exact eight-input runtime inventory does not contain ObjectModel. Existing
guarded reports identify Project member references but do not provide the
missing ObjectModel dependency surface or establish compatible concrete Project
construction. HostNotification signatures alone do not establish a required
service: its getter and callbacks were not requested. No useful offline shim
can yet be selected from these facts. Repeating Task-008's deny-all diagnostic
would not satisfy this implementation task. All acceptance criteria remain
unchecked; TEST-001/002/003 are NOT RUN for a Task-009 implementation.

The bounded next step is private qualification of the named ObjectModel file
(exact bytes/hash/assembly identity and guarded metadata), then inspection of
the concrete Project construction contract before selecting one evidenced
offline service/fixture. Keep original bytes unchanged, bus services denied
and original DLLs outside Git/distributions. No input allowlist is expanded
from a filename or guessed identity.

The ETS-source SSH control socket was absent when checked; the operator has
been asked to reopen the existing connection. The new test VM connection
remains available. This is a missing technical input, not a request for
renewed legal approval. No Task-009 source, SDK service, original execution,
hardware action or gate result is produced by this blocker record.

Predefined `next_dca_readiness` independently inspected the actual Task-008
result and guarded Task-142/144 reports and confirmed that no substantive
service implementation is ready without guessing. Scope and whitespace
checks for this record are recorded after review.

Blocker-record validation: `python3 -B scripts/check_project_scope.py` PASS;
`git diff --check` PASS. Predefined `binding_review` independently reproduced
these checks and returned PASS without findings. This permits the report to
commit; Task-009 remains Blocked and no implementation criteria are accepted.


## Static Project qualification update, 2026-10-22

This update supersedes the initial missing-source-connection and missing-file
blockers above. The coordinator privately acquired `Knx.Ets.ObjectModel.dll`,
14,204,880 bytes, SHA-256
`73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a`.
The private copy/source manifest verification and `reference-inventory.json`
identify `Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral,
PublicKeyToken=61439873ec5e1159`, exactly the identity rejected in Task-008.
The inventory JSON is 860 bytes, SHA-256
`ff4711dda239231aa51d80d2a20771d4658d8f024514a540a659825615d8e8ad`;
it records compatibility UNVERIFIED and target assembly execution NOT_RUN.
ADR-005's owner-authorized private-use scope remains in force. The file is not
added to Git, distributions or a runtime allowlist.

A fixed `project-context` mode now uses only the existing three exact input
pins and guarded metadata reader. The coordinator's private guarded report,
`private/evidence/task009-objectmodel/project-report-task009.json`, is 20,450
bytes, SHA-256
`85a3d77be7da708e7732618ab2b494457b342018f259f41e26b131985858bf40`.
Write and network denial probes are both true; selected input hashes/sizes
match the pins. The report was re-read as JSON and its bytes/hash verified.
The exact normalized command and detailed observations are recorded in
[DCA-PROJECT-CONTEXT.md](../docs/DCA-PROJECT-CONTEXT.md).

Project `020001D1` is public/sealed. Its sole declared instance constructor,
`06001409`, has attributes `6275`, access mask `3` (assembly/internal), and
parameters SDK `ProjectCollection`, ObjectModel `Project.Project`, and SDK
`ISdkContext`. The same-assembly bases are `ProjectRelatedObject` and
`DomObject`, ending at unresolved external `System.Object`. Public
`get_AddinData` (`0600140F`) and `get_Name` (`06001422`) agree with the previous
member evidence; `get_Parent` returns SDK `Root`. No public Project instance
constructor was observed. This is not proof that no public factory or
collection route exists. Those parameter/factory contracts require the next
bounded static inspection before any actual context service can be selected.
No nonpublic activation, null/uninitialized object or guessed fixture is supplied.

Validation: the provisioned PowerShell 7.6.7 owned metadata test command
reported **PASS 23 / FAIL 0**; the Python harness command reported **PASS**,
14 tests; guarded original Project metadata extraction **PASS**, with runtime
execution **NOT RUN**. Scope and whitespace checks **PASS**. These results
qualify only static tooling/observations. TEST-001/002/003 for an actual SDK
service remain **NOT RUN**, all acceptance criteria remain unchecked, and
Task-009 remains **Blocked**. No original initialization retry, Windows SDK
execution, device operation or gate advancement occurs. Commit/review of this
metadata increment precedes any separately reviewed four-input extension.

Independent final review: predefined `project_review` PASS without findings
for the bounded static metadata increment. It reproduced all 23 owned
PowerShell cases, 14 Python tests, scope and whitespace checks, matched all six
source-code hashes, and independently checked the guarded report JSON/pins and
constructor observations. Predefined `objectmodel_verify` separately verified
private candidate integrity and extraction-script provenance. Task-009 remains
Blocked pending an evidenced construction/service route.
