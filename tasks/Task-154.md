# Task-154: Inspect the Glass Lite project/11 application candidate

| Field | Value |
|---|---|
| Status | Ready |
| Stage | PoC input qualification |
| Component | knx-editor / poc/product-download |
| Dependencies | Task-003, Task-004, Task-023 |
| Required phase gate | None |

Implement bounded offline structural inspection for the privately registered
Glass Push Button II Lite candidate, separate from the accepted project/20
GlassV31 profile. The owner's model and application version are useful input,
but do not by themselves verify the application currently on the physical
device or establish the meaning of fingerprint `9BDB`.

The candidate archive has SHA-256
`c8feaa000ca891df90b455a4ae27de9e1c0e3d3e5cf53e63cf944807f755c7e9`.
Its selected application member has SHA-256
`8b4142ac7d695e0d8536525a23781ecd45ddb52650d88d90627f2782eeb272a8`.
The existing private static inventory is
`private/evidence/task023-light-reference/static-inventory.json`, SHA-256
`b6fcc5f66421289bd0f4574ae56e8ada91a3ceed47539f5db32af560775158a5`.
It records a catalog chain from `BE-GTL4Tx.01` to
`M-0083_A-0090-13-D12F`, project/11 and mask `MV-0705`. This is a
software-candidate association only, not a physical-device qualification.

## Requirements

- Accept only the explicitly selected archive and application identity; retain
  existing archive/XML resource limits, reject ambiguous or unsafe input and
  avoid XML external entities or network access.
- Extract a bounded, explicit structural model for the selected project/11
  application, preserving missing segment data as absent and preserving
  unsupported conditional, module and load declarations as unevaluated.
- Reject unexpected namespaces, mismatched product/application references,
  unsafe resources and unsupported constructs where their interpretation would
  otherwise be needed. Do not infer activation, allocation or load semantics.
- Keep manufacturer input and generated evidence private; do not publish
  proprietary files or touch the deferred ETS production project or hardware.
- Keep compilation and downloadable-image eligibility fail-closed until the
  separately reviewed Task024 requirements and exact physical match are met.

## Acceptance and tests

- The selected private archive can be inspected deterministically and its
  product/application linkage and present versus absent segment data match the
  independently reviewed inventory.
- Synthetic fixtures exercise the supported grammar and negative cases for
  wrong product, application, namespace, missing references, archive limits
  and unsupported critical content. Private fixture tests identify their
  input hashes without committing the fixture.
- The existing project/20 profile and its tests remain unchanged in behavior;
  inspection cannot produce a downloadable image or enable a bus operation.
- Run focused tests, applicable formatting and Clippy checks, independent
  review, and record exact source revisions and limitations before Done.

## Completion record

Not started. Parser acceptance, physical application match, fingerprint
interpretation, compiler semantics, Windows and hardware are NOT RUN or
unverified. No phase gate is advanced.
