# Task-154: Inspect the Glass Lite project/11 application candidate

| Field | Value |
|---|---|
| Status | Done |
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

The bounded project/11 inspection was implemented in reviewed source commit
`5f9052bdf99f74b7a549dec28cc4b53b2891f0d8` and integrated on main as
`2937b7a`. The public Lite entrypoint requires the registered archive hash;
the inspection preserves absent RAM data and cannot make the existing compiler
accept this profile. Independent review found and verified the correction of
an initially missing runtime hash check. The existing project/20 tests remain
passing.

On macOS arm64, the implementer ran the locked offline product-download suite
(13 unit and 39 integration tests), all four private fixture tests, Clippy,
format and diff checks; independent review repeated the suite and registered
Lite fixture. On main the coordinator repeated the suite and Lite fixture:
`cargo test --manifest-path poc/product-download/Cargo.toml --workspace
--locked --offline --no-default-features` and `cargo test --manifest-path
poc/product-download/Cargo.toml --locked --offline --no-default-features
registered_lite_private_fixture -- --ignored`, with the private archive path
provided via `KNXPROD_GLASS_LITE_FIXTURE`; both PASS. Exact commands, hashes
and limits are in `poc/product-download/TASK154-GLASS-LITE-INSPECTION.md`.

Physical application match, fingerprint interpretation, compiler semantics,
Windows and hardware are NOT RUN or unverified. The selected candidate is not
a downloadable image. No phase gate is advanced.
