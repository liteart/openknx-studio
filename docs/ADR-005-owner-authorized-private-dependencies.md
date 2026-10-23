# ADR-005: Owner-authorized private DCA and SDK dependencies

## Status

Accepted by Simon, 2026-10-21. Applies to Task-005 and the original DCA/SDK
technical investigation. This records an owner decision, not legal advice or
legal certification by an agent.

## Context and evidence

The project uses Apache-2.0 for project-owned source. Original manufacturer
DLLs are third-party files. Task-005 previously treated unavailable license
documents and legal review as active blockers. Simon explicitly directed the
project to assume local use is authorized, keep original DLLs out of the
repository, and assign responsibility to Simon.

Technical evidence remains separate. At the 2026-10-21 decision snapshot, the selected DCA requested SDK/AddInViews
`5.7.554.37315`; the private candidate assemblies report `6.4.8718.0`. The
candidate map records 55 unmatched member signatures, 209 unsupported
TypeSpec member parents, and additional unsupplied dependencies. This does not
prove incompatibility. Clean ETS-free runtime closure and binding/activation
behavior have not been qualified.

## Decision

On 2026-10-23 Simon explicitly reaffirmed that he has clarified the permission:
private copies of the data are allowed. The original DLLs are proprietary and
must not be redistributed. This is Simon's supplied rights clarification and
supersedes the earlier description as an unresolved owner assumption for private
copying; it is not an independent legal determination by an agent. The existing
authorization for local technical investigation remains in effect.

Assume local use of the selected private references and original DCA is
authorized for this project investigation. Simon accepts responsibility for
that assumption. No license documents, lawyer review, vendor confirmation, or
legal/provisioning approval is a prerequisite for Task-005 technical work.

Original DLLs must remain outside Git and outside every build distribution.
Approved extraction is limited to private local storage. This decision does
not change the Apache-2.0 license for project-owned source and does not permit
redistribution of original DLLs.

Continue to require evidence for actual SDK API/assembly compatibility,
dependency closure, binding and activation behavior, and a clean Windows host
that does not require installed or active ETS at runtime. Do not patch binaries,
bypass signatures or activation, or invent successful host behavior. Existing
hardware identity, isolation, operation-specific human authorization and
recovery controls remain in force.

## Alternatives considered

- Keep missing terms and human legal review as a prerequisite: rejected by the
  owner's explicit decision.
- Put original DLLs in the repository or build distribution: rejected by the
  owner's explicit exclusion.
- Treat owner authorization as proof of technical compatibility: rejected;
  compatibility evidence is incomplete.

## Consequences and risks

Task-005 is no longer blocked on legal-document acquisition or legal review. It
was Blocked on technical qualification at that snapshot. The dated Task-005
qualification record now governs current technical readiness. Private reference files must not
enter source control or build distributions. This decision assigns responsibility to
Simon and is not an agent-authored rights determination.

## Validation and rollout

Update Task-005, the DCA host contract, provisioning and terms evidence, and the
stable `sdk-references-rights` inventory entry. Preserve dated evidence as
historical where it records what was known before this decision. The reference
input checker must continue to identify unresolved technical qualification.
No hardware operation or phase gate is approved by this ADR.

## Approval

Simon, 2026-10-21. Scope: assumed authorized local use for technical
investigation; original DLLs excluded from Git and all build distributions.


## Technical evidence update, 2026-10-22

Accepted Tasks 143/144/148 supersede the old unresolved metadata/runtime snapshot
for their exact scopes. Task-148 demonstrates ETS-free startup/one original
constructor with exact private inputs; its network-isolation FAIL and explicit
constructor-only owner exception do not authorize later lifecycle/device calls.
Task-005 records actual PASS for its separate exact-private-reference compile
proof and missing/mismatched-reference negatives; final independent review
governs acceptance. Later Initialize/UI/state/service compatibility belongs to
Tasks 008–012. These technical observations do not change this owner assumption,
assign legal certification to an agent, allow original DLL distribution, or
approve any phase gate or device operation.

## Owner reaffirmation and status reconciliation, 2026-10-23

Simon also reaffirmed that the DCA supports ETS 5.7 and later, including his ETS
6.4 installation. The requested/supplied assembly-version difference is not an
incompatibility finding. Accepted Task-143 supplies selected binding/member
resolution evidence; Task-148 supplies actual startup/constructor execution with
SDK 6.4 on Windows without installed/running ETS. Task-005 is Done for that
selected route at `ee3cbdf0afd1e80184c43a0a9c4a1a2f2127f75b`, followed by its
exact-revision record `546b126585541f77df0851419bfdbb7ea649970f`.

The coordinator's later claim that Task-005 was still Blocked came from reading
an older checkout without reconciling the existing task worktrees. That claim
is withdrawn. The accepted evidence has not been replaced by a new runtime
attempt; the current check verified its retained artifact hashes. Later
Initialize/UI/state/services and device effects retain their own actual outcomes
and prerequisites. No vendor DLL is committed or redistributed.
