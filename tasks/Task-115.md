# Task-115: Fuzz and bound untrusted archive and configuration inputs

## Subject

Fuzz and bound untrusted archive and configuration inputs

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-115 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor + knust-fork / defensive tests |
| Owner | Unassigned |
| Requirement references | [R-008](../docs/REQUIREMENTS.md), [R-016](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Test malicious/corrupt inputs across the boundaries most likely to process third-party content.

## Detailed Requirements

- DR-001: Add bounded fuzz/property harnesses for archive metadata, XML/JSON/schema handling, public protocol parsers and IPC/MCP envelopes in their owning repositories.
- DR-002: Seed with redacted/synthetic valid and invalid cases, never publicly embed proprietary binaries.
- DR-003: Assert no panic, filesystem escape, network fetch or unbounded allocation under EP limits.
- DR-004: Minimize and commit only redistributable regression cases for discovered failures.
- DR-005: Keep Secure crypto outside the fuzz target set.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The declared bounded fuzz campaigns complete without unresolved crash/hang/resource violations.
- [ ] AC-002: Each discovered defect has a regression case and fix evidence.
- [ ] AC-003: Fuzz targets respect component ownership.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-044](Task-044.md): Harden KNXPROD archive and XML ingestion. Required predecessor behavior/evidence must be accepted.
- [Task-045](Task-045.md): Introduce versioned product schema adapters and normalized IR. Required predecessor behavior/evidence must be accepted.
- [Task-065](Task-065.md): Regression-test the non-secure protocol feature set. Required predecessor behavior/evidence must be accepted.
- [Task-090](Task-090.md): Test MCP misuse, idempotency and authority boundaries. Required predecessor behavior/evidence must be accepted.
- [Task-099](Task-099.md): Register KNXPROJ fixtures and safe import preparation. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Fuzz: record duration/iterations/seed/toolchain for each harness.
- TEST-002: Adversarial: ZIP/XML/JSON/protocol/IPC limit cases.
- TEST-003: Regression: rerun all minimized failures.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor + knust-fork / defensive tests**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No claim that finite fuzzing proves absence of vulnerabilities or permission to redistribute original packages.

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
