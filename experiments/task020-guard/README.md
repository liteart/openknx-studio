# Task020 offline exact-plan safety core

This increment is a test-only editor-side model. It contains no executable,
networking, KNX codec, fork dependency, credential discovery or physical-device
adapter. It cannot run the planned hardware test. Its only Cargo registration
is the `task020-guard` test target.

The model owns a bounded immutable plan and binds an exact route, identity
receipt, property selector, original bytes, desired bytes and recovery purpose
to its SHA-256 digest. It checks observations before returning an opaque
dispatch selection. The selection has private fields, no cloning or
deserialization API, and must be consumed by a binding operation that rechecks
the digest, route, selector, phase and bytes against the exact plan. The bound
view exposes only immutable accessors. A future actual dispatcher must consume
that bound view; there is no dispatcher in this increment.

Actual property validity and observations must come from a future verified
typed adapter. Observation and post-response watermark types have no external
constructor or deserializer here, so no external caller can supply actual
evidence yet. Private unit tests use explicitly synthetic fixtures. The module
does not itself authenticate observations or interpret protocol fields.

There is deliberately no external constructor or deserializer for
`HumanApproval`, no approval CLI and no way to obtain approval from a file,
boolean or prompt string. Missing real approval fails before journal mutation.
Synthetic approvals exist only inside the private `cfg(test)` unit-test module;
they cannot authorize real device operations. Choosing and implementing a real
operator channel unavailable to the agent principal remains a prerequisite.

Before any dispatch selection, the model synchronously writes and flushes a
consumed-grant record and intent containing the full plan. An exclusive durable
route record quarantines the gateway/bus path across operations, targets and
process restarts. Any incomplete, partial or unrecognized evidence remains
unknown. Merely constructing a fresh operation or synthetic approval does not
clear an unresolved route. Filesystem failures do not return a new selection.
The journal is deployment evidence, not an accepted configuration write, and
does not replace canonical project JSON/Git persistence.

The in-memory sequence requires confirmed write response, a new exact desired
readback, durable restore intent, confirmed restoration response, and a new
exact original readback before releasing quarantine. Each confirmed response
requires an opaque trusted adapter watermark captured after that response;
the readback receipt must exceed it. A pre-dispatch watermark is insufficient
because an observation can arrive before the actual send. UTC-second equality
therefore cannot make a pre-restore sample valid. Missing trusted watermark
fails closed, and no real watermark adapter exists yet. A response alone does
not prove restoration. Uncertainty prevents automatic restoration or another write.
Expiry blocks the first dispatch; it does not abruptly prevent an already
started approved restoration sequence. This is an offline state model, not
proof that a device will restore or that physical effects are reversible.

The caller must supply an already private, canonical journal directory. The
model uses exclusive file creation, synchronous file/directory flushes and
bounded structured recovery reads. It is not a security boundary against
unrestricted code or filesystem modification under the same OS principal.
Directory synchronization errors fail closed; Windows durability is NOT RUN.

## Verification

Baseline editor revision: `10bee8e`. Pinned Rust 1.99.1, cached locked
dependencies, default features (no Secure). All fixtures are synthetic.

- `cargo test --locked --test task020-guard`: final 18-case version PASS.
- `cargo clippy --workspace --all-targets --locked -- -D warnings`: final PASS.
- `cargo test --workspace --locked`: final PASS (18 guard, 17 existing DCA state,
  two existing CLI tests).
- `cargo fmt --all -- --check`: PASS.
- `git diff --check`: PASS.
- `python3 scripts/check_project_scope.py`: PASS for document scope; product,
  Windows, hardware and human gate approval are explicitly NOT RUN.
- Hardware, trusted approval channel, actual fork adapter, restore procedure,
  Windows behavior and Task020 acceptance: NOT RUN / pending.
