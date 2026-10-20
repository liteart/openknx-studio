# Task-002: Create two isolated repositories and preserve licenses

## Subject

Create two isolated repositories and preserve licenses

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-002 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor + knust-fork / repository setup |
| Owner | Primary coordinator |
| Requirement references | [R-002](../docs/REQUIREMENTS.md), [R-003](../docs/REQUIREMENTS.md), [R-004](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-029](../docs/REQUIREMENTS.md) |

## Description

Create the editor repository and a separate knust fork/checkout with an auditable upstream baseline.

## Detailed Requirements

- DR-001: Record the upstream URL and immutable commit SHA; preserve upstream history and notices.
- DR-002: Put the optional .NET host only under editor/dca-host.
- DR-003: Add Apache-2.0 licensing for owned editor source and document the licensing treatment of fork contributions without relabeling upstream files.
- DR-004: Configure ignore rules for private artifacts, credentials, captures, caches, build outputs, and real project data.
- DR-005: Do not introduce a reverse dependency from knust to the editor.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The two working trees have separate Git histories and remotes when authorized.
- [ ] AC-002: LICENSE/notice handling is documented and no manufacturer files are committed.
- [ ] AC-003: A clean editor checkout does not contain copied KNX wire implementation files.

## Dependencies to Other Tasks

- [Task-001](Task-001.md): Establish the executable scope and decision baseline. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Static: inspect manifests and dependency graph for repository independence.
- TEST-002: Git: confirm upstream SHA and absence of ignored private files in tracked paths.
- TEST-003: Review: confirm license changes preserve upstream material.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / repository setup**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No full product workspace or complex CI infrastructure before the PoCs.

## Risks and Blockers

External fork creation/push requires the maintainer-provided remote and permission; local preparation alone must not be reported as a published fork.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Local fork preparation complete; full task acceptance pending |
| Source commit(s) | Upstream `287bd5c94644688ac2eb93bf09f10a37bc509e0c`; local fork `d6e25fb206850dd6da18d8acf9b82ecdf5234733`; editor evidence commit recorded below after commit |
| Platform | macOS 27.0 arm64; Git CLI; Python 3.14.6 |
| Tests executed | Static history/license/dependency/ignore assertions PASS; scope checker PASS; both repository diffs pass `git diff --check` |
| Evidence location | This record; local fork `FORK.md` and `AGENTS.md`; [repository setup evidence](../docs/LOCAL-REPOSITORIES.md) |
| Acceptance criteria met | AC-001 locally demonstrated; AC-002 partially (upstream MIT preserved, editor LICENSE setup outstanding); AC-003 demonstrated for tracked editor files |
| Technical outcome | Full upstream history cloned; independent local protocol checkout and task branch; editor excludes fork sources/history; no protocol implementation changed |
| Gate outcome | None advanced; Task-001 remains In review pending human acceptance |
| Reviewer | Independent Sol review complete; contribution-license wording corrected and re-reviewed; no remaining local-setup findings |
| Hardware / Windows / Cargo | NOT RUN; static repository setup only; inherited test safety/features await Task-015 |
| Remaining work | Task-001 maintainer acceptance and full Task-002 editor license/notice setup review; runtime agent installation in fork not established |
| Publication | No GitHub fork, push, PR, merge, tag or release; user explicitly selected local fork and local commits only |

### Authorization and readiness

On 2026-10-20 the user explicitly requested retrieving the original GitHub
repository and creating the local fork here, then confirmed local commits suffice.
This authorizes this bounded preparation while Task-001 is still In review;
it is not a claim that its predecessor was accepted or that full Task-002 is Done.
No publication destination is needed for local operation. Repository owners and
publication remotes remain undecided rather than invented.
