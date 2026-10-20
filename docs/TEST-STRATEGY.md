# Test Strategy and Evidence Rules

## Levels of evidence

| Level | Demonstrates | Does not demonstrate |
|---|---|---|
| Static archive/metadata | Actual file structure, declared IDs, textual/metadata references | Runtime binding, DCA functionality or hardware interoperability |
| Unit/golden/property | Deterministic logic and known encoding/validation cases | Complete manufacturer/device behavior |
| Simulator/fault injection | Modeled protocol states, failures, recovery and denial paths | Real gateway/device compliance or firmware compatibility |
| Clean Windows execution | Original DCA/runtime/state behavior without ETS in that environment | Every DCA function or another package/version |
| Real isolated hardware | A recorded operation on an exact identified device/version | General support for related products or permanent physical-state truth |
| Cross-platform execution | A package actually runs on its declared target | Other architectures/OS versions or original WPF execution there |

Every report records PASS, FAIL, INCONCLUSIVE or NOT RUN, exact commands, source revisions, enabled features, fixture hashes, environment, reviewer/actor, output, and limitations. Store output hashes/locations rather than unsupported claims. A screenshot can prove that a UI rendered, not that its data was transferred correctly.

## Default test modes

All ordinary tests run offline with synthetic/redistributable fixtures. Network/hardware tests are opt-in and cannot discover or write to a live installation by default. Proprietary archives and actual household exports remain private; a public test may consume them only from an explicitly configured local path and verify the expected hash.

Tests in the fork cover KNX codecs and protocol sessions. Editor tests cover product semantics, memory/table compilation, graph, persistence, approvals, jobs and MCP. Windows tests cover SDK binding, UI lifecycle, state and bridge behavior. Do not duplicate protocol serializers in editor test helpers: consume fork-provided test support or test at the public API boundary.

## Mandatory negative cases

Inputs: unsupported schema/critical construct, duplicate/scoped IDs, unresolved/cyclic refs, invalid parameter values, inactive objects, incompatible DPTs, wrong product/application/mask, invalid/colliding addresses, missing mandatory state/original artifact, bad package hashes, archive/JSON/XML/IPC size violations, path escapes and XML entities.

State: stale changeset/plan/cursor, external dirty edit/branch move, invalid remote merge, cache corruption/version mismatch, disk-full and permission failures, truncated operation logs, process interruption at every commit/dispatch boundary, and restart without SQLite.

Effects: wrong/unknown target identity, expired/replayed/wrong-scope grant, MCP attempt to approve itself, vendor session escalation, unmediated driver request, lost acknowledgement, unsafe cancellation, and secure-required or legacy/general-app requests. Rejecting Secure-required input is a negative scope test, not implementation or qualification of Secure cryptography.

## Hardware authorization record

Before writes, record the isolated bench, gateway, target hardware/application/firmware identity, approved operation/payload or bounded DCA purpose, permitted property/range, power/recovery plan and human operator. A generic instruction to run tests is not approval to change an unknown device.

Choose a genuinely documented reversible property for PoC B. Confirm old value, approved new value, read-back and restoration. Do not guess an available writable property. For application downloads, start from a different/unconfigured state and verify both available status/identity and a functional effect. For individual addressing, explicitly handle multiple responders/collisions and hardware lacking stable pre-address identity.

Firmware updates need a separate artifact/target grant and manufacturer-supported recovery plan. Do not unplug a device during flash simply to get an error-path test. Use simulations or a documented recoverable test method. Readback/diagnostic operations must be classified from actual behavior; some discovery procedures write addresses.

## Golden tests and reference truth

Each expected vector/image/table must have a source: documented protocol/profile definition, a permitted independently reviewed reference capture, or a clearly synthetic model. Do not generate the expected output with the same function under test and call that independent evidence.

Old ETS-produced exports/captures can be reference inputs. No ETS operation may execute the claimed independent test. Product-specific binary layout is verified in the compiler; generic KNX packet vectors live in the fork. A successful generic property write is not an application download proof.

## Persistence/recovery matrix

Test before/after journal persistence, each changed-file replacement, Git object/index/ref publication, accepted-state publication, cache swap and final acknowledgement. Readers see complete accepted snapshots. Outside programs may observe intermediate raw files; document this rather than asserting impossible global multi-file atomicity.

For physical jobs test before intent, after intent/before dispatch, after dispatch/before acknowledgement, after acknowledgement/before receipt, and after receipt. Cases without enough evidence become interrupted/unknown. Re-running recovery must be idempotent and may not auto-repeat a side effect.

## Localization and performance

Test English fallback, BCP-47 selection, missing keys, arguments/plural forms where used, pseudo-localization and locale-invariant canonical data/API identifiers. Preserve vendor labels and opaque state; do not rewrite original DLLs. All owned docs/tests/commits use English.

Use the exact corpus, host description, sample count and budgets in REQUIREMENTS. Report hardware/DCA/network timing separately. No benchmark claim without raw data; no silently relaxed threshold.

## Completion and release evidence

A required hardware/Windows/platform test cannot be marked PASS because CI did not have that environment. Record NOT RUN and keep the task/gate blocked as applicable. A failure-analysis report may be complete while its technical outcome is FAIL. Readiness checks must inspect outcome, not just file existence or a Done status.

Release claims refer to exact versions/conditions. Requalify after changing a compiler/profile, protocol dependency, original DCA, relevant SDK binding, schema migration or platform environment. Public reports should be redacted but still reproducible for someone with lawful copies of the inputs.
