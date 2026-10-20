# Task-120: Benchmark the declared performance and resource budgets

## Subject

Benchmark the declared performance and resource budgets

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-120 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / performance tests |
| Owner | Unassigned |
| Requirement references | [R-006](../docs/REQUIREMENTS.md), [R-019](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Measure the specified engineering budgets with reproducible fixtures instead of assuming SQLite makes the system fast.

## Detailed Requirements

- DR-001: Generate the exact EP performance corpus and record reference CPU/RAM/SSD/OS/toolchain.
- DR-002: Measure warmed inspect/search, 20-parameter commit, cache rebuild and peak core memory with at least the specified samples.
- DR-003: Separate cold import, vendor DCA, hardware latency and remote network timing from core budgets.
- DR-004: Keep outputs deterministic and check correctness alongside timing.
- DR-005: Any changed threshold needs an ADR and retained failing measurements.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The declared baseline budgets pass on the recorded reference host or a reviewed blocking result exists.
- [ ] AC-002: Raw measurements and fixture generator inputs are retained.
- [ ] AC-003: Performance optimizations do not change authoritative state or omit validation.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-040](Task-040.md): Build a disposable revision-tagged SQLite read projection. Required predecessor behavior/evidence must be accepted.
- [Task-059](Task-059.md): Calculate deterministic configuration impact. Required predecessor behavior/evidence must be accepted.
- [Task-084](Task-084.md): Provide revision-aware paginated project and catalog resources. Required predecessor behavior/evidence must be accepted.
- [Task-116](Task-116.md): Execute the full file, Git and cache crash matrix. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Benchmark: required p95/time/memory metrics from REQUIREMENTS.
- TEST-002: Regression: cache on/off correctness and same compiled output.
- TEST-003: Load: bounded monitor/history behavior under a slow client.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / performance tests**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No performance claim for every device/host or unsupported million-device workload.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | Not started |
| Source commit(s) | Not created |
| Tests executed | NOT RUN |
| Evidence location | Not produced |
| Acceptance criteria met | None asserted |
| Technical outcome | Not evaluated |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Unassigned |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).
