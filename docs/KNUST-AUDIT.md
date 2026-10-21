# Pinned knust capability audit

Task: [Task-015](../tasks/Task-015.md). Audit date: 2026-10-21 UTC.
Status: Done; independent verification and critical review PASS. No phase gate or physical operation is approved here.

The authoritative capability map, inherited-test side-effect review, exact
commands, counts and source hashes are `AUDIT.md` in the separate knust fork.
The audited source is `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e`, based on
upstream `287bd5c94644688ac2eb93bf09f10a37bc509e0c`; inherited source,
tests, examples, package manifest/lock and MIT license match that upstream
baseline. The audit changes only `AUDIT.md` and a narrow `BUILDING.md` link.
The reviewed fork audit documentation is committed at
`7e44751fb6681812fcb6690d8159a9b8c5c3e816`, committed before this editor record. No editor Cargo dependency is introduced by an audit.

| PoC dependency | Pinned audit finding / next required task |
|---|---|
| Task-015 source input | Fork `f85e40d8709d0ad31f029b3eab7b0b6e00abd48e`; upstream `287bd5c94644688ac2eb93bf09f10a37bc509e0c` |
| Task-016 simulator | Depends on accepted Task-015 evidence at that exact source; define only the selected generic peer/management states from verified references |
| Task-017 non-secure tunnel | Depends on Tasks-015/016; public UDP/TCP tunnel code exists, live interoperability remains unverified |
| Task-018 identity | Depends on Task-017; descriptor codecs and bool occupancy probe are not a full matched identity read |
| Tasks-019/020 properties | Depend on simulator/identity and stated input/approval prerequisites; typed generic PropertyValue/PropertyDescription services are absent |
| Task-021 memory/load | Depends on Tasks-015/016/018/004; typed device memory/restart/load-state services are absent; product layouts stay in the editor |
| Task-022 physical PoC B | Remains dependent on Tasks-018 through 021, exact bench/target and separate recorded human operation grants |

Any later code dependency must pin the tested, committed fork implementation
revision after that fork change is reviewed; the audit source pin is not a
claim that a missing service now exists. This report does not start those tasks.

Observed macOS 27.0 (26A428), arm64 results with Rust/Cargo 1.99.1:

| Task test | Result / limit |
|---|---|
| TEST-001 build/test | PASS for the selected scope: fmt, library build, library Clippy with `-D warnings`, 75 inherited offline synthetic/local-state runner passes, including the discarded-assertion coverage gap below. Every Cargo build/test/lint used `--locked --offline --no-default-features --features dpt`. No blanket suite, examples, discovery, gateway or hardware test ran. |
| TEST-002 static exports/features | PASS for inspection: resolved root features exactly `["dpt"]`; selected feature tree omits `aes`, `ctr`, `sha2`, `pbkdf2`, `x25519-dalek`, `zeroize`; public APIs/gaps mapped. Secure feature/crypto session execution is disabled; inherited Secure source remains as explicitly permitted below. |
| TEST-003 claim/coverage comparison | Capability-by-capability comparison recorded in fork `AUDIT.md`; independent project reviewer PASS (source/log comparison, not a Cargo rerun). Test success is limited to the named cases. |
| Hardware / other OS / MSRV | NOT RUN. No physical identity, property read/write/readback/restore, download, Windows or cross-platform claim. |

The runner reported 75 passing cases: seven descriptor, three group APCI, nine TPCI, nine
CEMI, ten probe helper/exhaustion, three router, five frame extraction, four
backoff, three tunnel address precondition, five non-secure connect codec,
thirteen public tunnel-sequence, and four selected DPT tests. The four
unconditional `protocol::knxip::tests::session_*` tests were filtered out.
The probe helpers perform no live probing. Existing non-ignored tests can
contact gateways or initialize routing, so unfiltered `cargo test` is unsafe
under the offline-only authorization. Exact safe filters are in fork `AUDIT.md`.

Inherited coverage gap: `tests/test_sequence_validation.rs:262` defines
`property_tests::property_sequence_validation_consistency` with
`let _ = tokio_test::block_on(async { ... })`. The `prop_assert_*` checks at
lines 273–285 return an error from the async block on failure, but the outer
test discards that `Result`. It can therefore report PASS even when those
property assertions fail. Its runner pass remains in the observed total of
75; it does not substantiate the sequence-consistency property. Other named
deterministic sequence cases are separate evidence. No test source was changed
in this audit; future reliance on that property needs a separately reviewed
correction rather than an assumed passing assertion.

Operator clarification, 2026-10-21: preserve inherited Secure source; v1 does
not use Secure, and possible later-version use will be evaluated separately.
Source/type presence is permitted and is not a blocker or a reason to remove
code or create a source-removal task. Task-015 DR-003 concerns disabled
feature/runtime use, not absence of inherited source or public symbols.

The selected graph disables KNX Secure cryptographic/session execution.
`src/protocol/knxip.rs` still unconditionally compiles Secure service IDs and
`SessionRequest/Response/Authenticate/Status` wire helpers;
`src/transport/mod.rs` exposes Secure variants/configuration. These are factual
observations consistent with the operator clarification. Source inspection
finds explicit unsupported/configuration errors for Secure requests with the
feature disabled; v1 must not silently downgrade to plaintext. This audit
inspected those rejection paths statically and did not run a runtime negative
test. Secure tests, handshake, cryptography and hardware qualification are
**NOT RUN**. No Secure source was removed, enabled, changed or qualified.

No versioned primary KNX protocol specification or permitted management capture
was supplied. Missing-operation tasks must obtain exact primary specification
editions/sections and independently reviewed permitted vectors/traces covering
cEMI/TPDU/APCI, transport ACK/sequence/retry, PropertyValue, identity/addressing,
and selected memory/load-state behavior. The inherited tests use synthetic
models, not independently verified management captures. No writable property,
layout, identity, recovery behavior or timing guarantee is inferred. See
[TEST-STRATEGY.md](TEST-STRATEGY.md), [ARCHITECTURE.md](ARCHITECTURE.md), and
[REFERENCE-INPUTS.md](REFERENCE-INPUTS.md) for boundaries and missing bench inputs.

Provisioning was limited to explicitly authorized locked packages from the
official Cargo registry. Initial sandbox DNS/cache failures and an unfiltered
metadata request missing an Android-only package are retained in the fork report;
host-filtered resolution and all subsequent build/test commands succeeded
offline. The inherited `Cargo.lock` SHA-256 remains
`b616b2a878eda527d603d8cc33f183b52ce47fa1eb10d8285d43952d0038b993`.
