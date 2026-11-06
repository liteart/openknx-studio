# Task-013: Demonstrate fail-closed PoC DCA bus isolation

## Subject

Demonstrate fail-closed PoC DCA bus isolation

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-013 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knx-editor / poc/dca-host test harness |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-011](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md), [R-030](../docs/REQUIREMENTS.md) |

## Description

Prove the offline DCA experiment cannot accidentally write to a KNX installation and expose any bypass requirement.

## Detailed Requirements

- DR-001: Instrument every discovered bus service to deny live access.
- DR-002: Run with network access to real KNX interfaces unavailable and record attempted host calls.
- DR-003: Trigger a benign UI path that requests a connection and verify an explicit unsupported/offline response.
- DR-004: Detect direct vendor network attempts where observable and classify them as a mediation blocker.
- DR-005: Document that process separation alone is not a sandbox.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: No live KNX traffic is sent during the offline experiment.
- [ ] AC-002: A requested bus operation fails explicitly instead of appearing successful.
- [ ] AC-003: Any direct-driver requirement is preserved as an unresolved risk for production integration.

## Dependencies to Other Tasks

- [Task-012](Task-012.md): Round-trip one original DCA configuration change. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Integration: controlled attempted connection with denied bridge.
- TEST-002: Observation: verify no permitted path to real hardware exists.
- TEST-003: Negative: an unregistered bus callback is rejected and logged.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knx-editor / poc/dca-host test harness**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Includes Windows/vendor execution where stated. No live device write is authorized unless the Required Tests and a separate human lab grant explicitly allow it.

## Out of Scope

No full OS sandbox, real communication, or safe-write claim.

## Risks and Blockers

Before an original DCA connection test, configure and verify Windows Firewall
blocking on the test host, as directed by Simon. Temporary rules expire after
15 minutes and must be verified again for each test. Firewall
rules alone do not establish protection against a direct USB/driver path, an
allowed management endpoint, or a privileged process changing those rules. The
host isolation guard remains in place until the actual test environment and
recovery are verified. Unsupported or undocumented behavior must be recorded
and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; owned deny-only profile and pre-start isolation guard only |
| Source commit(s) | `9bb5eb7dea7c3ec9e0ca77efde60f7cc13fb2065`; source audit below |
| Tests executed | Owned Windows denial/entry tests PASS; TEST-001–002 NOT RUN, TEST-003 owned fixture PASS |
| Evidence location | Private Task013 evidence below |
| Acceptance criteria met | None asserted |
| Technical outcome | Known and unregistered owned host callbacks deny; original runtime/network isolation not established |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Unassigned |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

## Temporary Windows firewall observation — 2026-11-06

At Simon's direction, temporary outbound IPv4/IPv6 block rules were installed on
dev host `.60`, with `192.168.1.208` excluded for the existing management
connection and IPv4 loopback excluded. A SYSTEM task was registered to remove
the rules after 15 minutes. ActiveStore reported both rules enabled; a TCP
connect attempt to reserved benchmarking address `198.18.0.1:9` failed with
`AccessDenied`. The private inventory stdout SHA-256 is
`91f5538b6de86ebb25f75bf2c389571268acca1853db77d944f4daafba97752e` and the
benchmark-denial stdout SHA-256 is
`82c232dfb14951beacb20ec371ce94c6c2205ed04a6b1ce4b2ca0d3f06f4f492`; both
are under `private/evidence/task013-firewall-20261106/`. This observation
demonstrates a single blocked outbound TCP attempt, not comprehensive process,
driver or hardware isolation. The management endpoint remains an exception,
and a limited name-based PnP check did not qualify all passthrough devices.
The original-probe guard remains in place; vendor connection and UI tests were
NOT RUN. The rules are temporary and are not evidence for any later test run.

## Bounded denial checkpoint — 2026-11-03

Original metadata shows that the reference DCA connection path can reach an
authorization operation; it is not treated as a harmless live-device action.
The genuine SDK `AddinExecutionContext` exposes an `IHostContext` seam. The
new owned deny-only proxy throws for known connection/download calls and any
unregistered callback, without reading arguments or returning a connection.
It records bounded denial/lifetime evidence. The experimental original SDK
probe is unconditionally refused at the launcher and direct child entry before
vendor effects until a verifiable isolated execution capability exists.

Windows owned run `b13502` passed six typed denials and three synthetic
negative guards, and proved the original probe is blocked before child start
even with a nonexistent input path. Scheduled task exit 0; the test task was
removed. No original vendor code, driver, network or hardware was exercised.
The preceding SSH-session attempt `b13401` exited 4 because an interactive
session was required; the subsequent `b13402` run passed in the interactive
session. Private `b13502` owned log SHA-256:
`1b57a0f0ce7ada9623b6de141193fde46c8764c386cb1c075401939d0e9e5919`;
driver output SHA-256:
`570035f21232af3b3fcbe692f10dd968fdcbdb9ecb82e3487d40d6a16920abfa`;
source audit SHA-256:
`91fc1cbd927db35ea8a48c8878e2907bad71b029d3d98ca09b733fccb09e84a3`.
Focused local policy, compilation and parser checks passed. This establishes
the owned negative behavior only. An actual SDK/UI denial, absence of every
route to real KNX interfaces, and direct-vendor bypass observation remain
NOT RUN; no network-isolation or hardware safety claim is made.

Windows owned regression `dca013b13501` also reported 423 passing cases and
task exit 0. Its collector expected an absent `sdk-project.log`; the driver
output was checked separately, no owned processes remained, and the finished
task was removed. This regression did not execute original vendor code.
