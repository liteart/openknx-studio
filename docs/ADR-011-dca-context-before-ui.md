# ADR-011: Prepare the original DALI device context before displaying its UI

Status: Implementation ordering correction; independently reviewed; maintainer acceptance pending.
Related tasks: [Task-010](../tasks/Task-010.md), [Task-011](../tasks/Task-011.md).

## Evidence and problem

The original plan made Task011 depend on the completed Task010 UI test. Exact
private metadata of the unchanged reference DCA shows GetAddInUI06000017 returns
null and GetDcaUi0600001E only constructs its real DALI control for exactly one
SDK Device. Its view model reads genuine product, application and parameter data.
Displaying that UI therefore requires the fixture context already assigned to
Task011. Initialization alone now passes on the independent Windows dev host;
it does not remove this device/product prerequisite.

Evidence: Task010 Source21 run7c91d52e4fa0 and private mdt-ui-01.json, SHA256
36e96d7545f3d58b2faccc136ecd9d16c2bc5499b338e2d2bc3cc174c777fbd7.
Source28 subsequently reaches the real ICacheProvider.HardwareCache request.
The official KNX [App validation rules](https://support.knx.org/hc/en-us/articles/360001508820-App-validation-rules)
also exclude the standard AddIn UI feature for DCAs. That corroborates why a
generic App window is not an alternative evidence path for this reference.

## Decision

Execute the existing Task011 fixture work after accepted Task009 and Task004.
Task011 may exercise original context/control construction to prove recognition;
Task010 then verifies actual rendering, a benign visible interaction and closing.
Keep the complete Task010 and Task011 acceptance criteria and required tests.
Do not create a duplicate task or call initialization-only evidence UI success.

This corrects an evidence-disproved implementation order under the owner's
ongoing instruction to complete the plan. It is not maintainer acceptance of
either task. No phase gate is advanced, waived or changed. State round-trip,
bus mediation and the PoC gate retain their existing requirements.

## Boundaries

Use one actual hash-pinned manufacturer product/application fixture. Preserve
full original IDs and unresolved compatibility differences; no fake hardware,
parameter definitions, cache bytes or SDK classes. Original local SDK cache/import
APIs may supply the required private fixture when verified. Owned production
product parsing and compilation remain Rust responsibilities. No .NET product
engine, online catalog, production database or ETS runtime dependency is added.

All bus operations remain denied. Physical hardware/profile qualification is not
inferred from an offline fixture. Comprehensive internal regressions and review
remain required before final acceptance, following the owner's requested direct
Windows iteration during implementation.
