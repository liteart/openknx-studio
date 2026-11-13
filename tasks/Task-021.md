# Task-021: Add only the selected profile memory and load-state primitives

## Subject

Add only the selected profile memory and load-state primitives

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-021 |
| Status | In progress |
| Stage | PoC |
| Priority within stage | P0 |
| Component / Git project | knust-fork / low-level management |
| Owner | Unassigned |
| Requirement references | [R-004](../docs/REQUIREMENTS.md), [R-010](../docs/REQUIREMENTS.md), [R-022](../docs/REQUIREMENTS.md), [R-024](../docs/REQUIREMENTS.md) |

## Description

Implement the generic low-level services required by the one chosen application download profile.

## Detailed Requirements

- DR-001: Identify the needed memory/property/load-state services from the legal reference evidence and selected product.
- DR-002: Keep addresses, segment layout and ordering in the editor test input, not hard-coded for MDT in the fork.
- DR-003: Implement payload bounds, response/error validation, and documented connection/session requirements.
- DR-004: Mark destructive primitives as unavailable to casual examples; expose them only through the controlled test flow.
- DR-005: No general firmware protocol is implied.

## Non-functional Requirements

- NFR-001: Apply the repository/language ownership rules in [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md); do not add a temporary implementation in the wrong component.
- NFR-002: Use English for owned documentation, identifiers, comments, test names and commits. Production human-facing messages use resource keys and the English fallback; serialized technical values remain locale-independent.
- NFR-003: Keep the implementation minimal and reference-specific. Do not add full MCP, Git/cache infrastructure, a general editor UI, Secure, or legacy support merely to complete this experiment.
- NFR-004: Required tests must have reproducible inputs and actual evidence. Unavailable Windows/hardware/rights are recorded as blockers or NOT RUN, never inferred PASS.

## Acceptance Criteria

- [ ] AC-001: Each required primitive has a deterministic simulator test and public typed API.
- [ ] AC-002: Product-specific values occur only in private caller fixtures.
- [ ] AC-003: Unsupported/ambiguous service behavior blocks the download PoC.

## Dependencies to Other Tasks

- [Task-015](Task-015.md): Audit the pinned knust fork against the PoC operations. Required predecessor behavior/evidence must be accepted.
- [Task-016](Task-016.md): Build a tiny deterministic KNX management peer simulator. Required predecessor behavior/evidence must be accepted.
- [Task-018](Task-018.md): Read and identify the reference device through KNX management. Required predecessor behavior/evidence must be accepted.
- [Task-004](Task-004.md): Register reference artifacts and the isolated test bench. Required predecessor behavior/evidence must be accepted.

**Required phase gate:** None; execute only satisfied prerequisites and approved lab actions.

A completed report with FAIL/INCONCLUSIVE or a gate with NO-GO does not satisfy a required successful proof. Required external inputs and side-effect permissions remain separate prerequisites even if a manifest/documentation task is complete.

## Required Tests

- TEST-001: Unit: encode/decode boundary vectors from verified evidence.
- TEST-002: Simulator: invalid load state, denied memory operation, partial transfer and timeout.
- TEST-003: Static: confirm no KNXPROD parser or device-profile table was added to the fork.

Use [TEST-STRATEGY.md](../docs/TEST-STRATEGY.md) and record the actual commands, source revisions, fixture hashes and outcomes. Do not replace a required hardware test with a simulator and keep the same claim.

## Deliverables

- The smallest implementation or review artifact for **knust-fork / low-level management**, with the specified tests and no unrelated refactor.
- A task result/evidence record identifying actual source commits, inputs, platform, test outcomes and remaining limitations; use the supplied templates.
- Updated public contracts, localization resources or support records only where this task changes them. Any scope change requires an ADR and corresponding task/test updates.

## Side Effects and Authorization

Offline implementation/review by default. Any stated hardware observation is separately opt-in; this task is not blanket permission for device writes or remote publication.

## Out of Scope

No product load-procedure interpreter, universal memory support, or real destructive write outside Task-027.

## Risks and Blockers

None beyond listed prerequisites. Unsupported or undocumented behavior must be recorded and must not be guessed.

See [RISKS-AND-INPUTS.md](../docs/RISKS-AND-INPUTS.md). Do not guess missing application IDs, protocol layouts, device properties, credentials, SDK rights or hardware outcomes.

## Completion Record

### ETS reference observation — 2026-11-13

Simon authorized passive observation and reported that ETS completed a full
download to the selected bench device successfully. A private NIC-level
capture contains traffic involving the selected device, but the Windows
capture recorded only gateway-to-ETS IP packets. Its original ETL metadata
shows receive observations and no transmitted packets at the chosen NIC
capture point. This is incomplete reference evidence: it does not qualify
the complete load sequence or resolve the profile ambiguities below. The
observed cEMI frames include positive link-layer confirmations addressed
to the selected device, including six PropertyValue reads and 19 writes;
these confirmations are not independent application-level readbacks. Two
KNXnet/IP `0x0423` messages are tunnelling feature responses, not routing
loss indications. The original ETS requests were not captured. The private
PCAPNG SHA-256 is
`83b2d26fba26e1f37138721043eb7b74c46be9bc59f7fbe987b85293c7e124e6`;
the corresponding private metadata text SHA-256 is
`a6539ba2164e9f89203df484320ecfb7bf4f259e8060ae93545ee869480188ff`.
The capture was stopped and its temporary filter removed. No independent
download or write by OpenKNX Studio was performed in this observation.
An independent count also found a discrepancy in the fork's extended-frame
layout. Its correction is independently reviewed in local unpublished fork
`3ebe3d979f61c699283c1f7ac07c7de920eb2c3e`; Task-157 records its scope and
tests. This codec fix does not itself resolve the complete download profile.
The Task021 management branch and corrected codec have since been combined
and tested in local unpublished fork integration `8d827644a93d5cf548885449879742290c5d2271`;
Task-159 records the evidence and remaining limits.
The connected session's required short extended EFF=0 receive tolerance is
implemented and reviewed in local fork `1275bacee12c087dc2a6d0335aedd862c023d58e`;
Task-160 records its bounded tests. Long APDUs and download-specific
capability negotiation remain unqualified.

The subsequent coordinated Task155 live read observed mask `0705` and a
five-byte application identifier matching the selected Task154 candidate's
manufacturer, application number and version. This narrows the profile
identity uncertainty; it does not establish the exact archive, configuration,
hardware variant or fingerprint, nor resolve the load semantics below.

| Field | Initial value |
|---|---|
| Implementation / report status | In progress; generic reads, writes, allocation controls and basic restart reviewed; exact profile completeness remains unqualified |
| Source commit(s) | Local unpublished fork `776ac0545780c578801ff48221aa877014b3d5ab`, based on `fcf3af5e669eca9fc4661106c17308e1bd983499` and prior Task021 increments |
| Tests executed | Basic restart codec and 33 connected-session loopback tests, both focused Clippy configurations, feature-off doctest, formatting and diff checks PASS. Independent review repeated the restart codec and five focused loopback tests; earlier allocation and partial-effect results remain recorded below. Hardware and Windows NOT RUN |
| Evidence location | Fork `TASK021-BASIC-RESTART.md`, `TASK021-ABSOLUTE-ALLOCATION.md`, `TASK021-SIMULATOR-COMPLETENESS.md` and preceding Task021 reports at the source commits above |
| Acceptance criteria met | AC-001 supported for implemented generic reads and writes, but the complete profile-required primitive set remains unqualified; AC-002 and the local fail-closed portion of AC-003 supported by independent audit |
| Technical outcome | Typed reads and opt-in one-attempt writes reject invalid replies and retain uncertainty. Verify-mode responses are distinguished from independent readback; target ACK without Verify Mode is explicitly unverified. Feature opt-in is not operation authorization |
| Gate outcome, when applicable | Not evaluated |
| Reviewer | Independent protocol and simulator reviews PASS for the bounded increments and partial-effect test; full task acceptance remains open |

Update this record only with observed results. Follow the task-scoped branch/commit policy in [AGENTS.md](../AGENTS.md).

The Glass Lite candidate's declared load sequence requires absolute data,
stack and task allocation controls. Their generic codecs and opt-in
one-attempt session operations are now covered by simulator tests; checksum
attributes other than disabled remain unsupported. The caller must supply
qualified PEI/application identity and ranges. The candidate's omitted task
fields and its property-comparison representation remain unqualified. Basic
restart now implements one attempt, own disconnect and six-second retirement;
its acknowledgement is transport evidence, not proof of device restart. A
dropped session still needs caller-owned durable quarantine and wait tracking.
No System B or IP example is assumed to define
the candidate's mask `0705` procedure; physical application matching and
missing RAM payload semantics remain open.

The selected candidate uses allocation flag `0x80`. Independent source review
found that the property-based allocation specification explicitly documents
only disabled checksum control, while an enabled bit is described for a
different memory-mapped procedure restricted to another mask family. The
candidate's master data also needs qualification to resolve property versus
memory mapping. No enabled-bit behavior is inferred. The source investigation
is recorded in local unpublished fork `9d479c8d160830c84e9b54a0e8563f5a0ea5d4d5`,
`TASK021-CHECKSUM-CONTROL-INVESTIGATION.md`; it changes no protocol code.
The candidate's property 78 comparison data is ten bytes although both the
resource specification and candidate master describe a six-byte value. Its
comparison representation remains unqualified; no truncation is assumed.
