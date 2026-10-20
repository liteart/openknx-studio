# Task-117: Review threat model and enforce dangerous-operation boundaries

## Subject

Review threat model and enforce dangerous-operation boundaries

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-117 |
| Status | Not started |
| Stage | RC |
| Priority within stage | P1 |
| Component / Git project | knx-editor / security review |
| Owner | Unassigned |
| Requirement references | [R-011](../docs/REQUIREMENTS.md), [R-015](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-028](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Audit the actual security boundary rather than relying on labels such as local or separate process.

## Detailed Requirements

- DR-001: Document trust among agent, human operator, vendor worker, artifacts, Git remote, core and KNX network.
- DR-002: Verify operator credentials cannot be used by the MCP principal and worker access cannot bypass required mediation in the qualified deployment.
- DR-003: Test path injection, forged approval, replay, worker escalation and unsafe remote Git configuration.
- DR-004: Explicitly state that non-secure KNX does not cryptographically authenticate the peer or protect bus traffic; restrict v1 qualification to a trusted isolated network.
- DR-005: Record residual risks and resolve release-blocking bypasses.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Apply relevant EP policies in [REQUIREMENTS.md](../docs/REQUIREMENTS.md), including bounded inputs, accepted revisions, deterministic output and cache-only database use. A deviation requires an approved ADR and changed tests.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: The reviewed threat model matches implementation and deployment assumptions.
- [ ] AC-002: Required deny paths are enforced below MCP/UI annotations.
- [ ] AC-003: Plaintext KNX limitations and same-principal arbitrary-code limitations are disclosed, not presented as solved.

## Dependencies to Other Tasks

- [Task-112](Task-112.md): Approve the beta functionality and portability gate. Required predecessor behavior/evidence must be accepted.
- [Task-070](Task-070.md): Enforce human-only exact-plan and DCA operation grants. Required predecessor behavior/evidence must be accepted.
- [Task-079](Task-079.md): Authorize interactive DCA operations and session effects. Required predecessor behavior/evidence must be accepted.
- [Task-081](Task-081.md): Constrain DCA worker failures and access. Required predecessor behavior/evidence must be accepted.
- [Task-090](Task-090.md): Test MCP misuse, idempotency and authority boundaries. Required predecessor behavior/evidence must be accepted.
- [Task-093](Task-093.md): Configure optional Git remotes and protected credentials. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** G-BETA = GO ([Task-112](Task-112.md)).

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Adversarial: operator/agent/worker boundary cases and artifact/path attacks.
- TEST-002: Review: inspect actual permissions/network controls and credential storage.
- TEST-003: Negative: attempted unapproved reset/update/raw write produces no effect.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / security review**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No KNX Secure implementation, claim of a tamper-proof local app, or third-party security certification.

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
