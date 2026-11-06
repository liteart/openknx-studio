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
that bound view. The crate-private dispatcher now consumes it through a typed
session adapter boundary; the default Cargo target supplies only synthetic
sessions and cannot connect to a device.

Actual property validity and observations must come from a reviewed typed
adapter. Observation and post-response watermark types still have no external
constructor or deserializer. The crate-private dispatcher creates them only
after awaited typed session results, using its own strictly increasing event
ordinal. It rechecks the route, selector, bytes and exact response against the
immutable plan. The current Cargo target uses only explicitly synthetic session
fixtures. It does not authenticate a real device or interpret protocol fields.

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
exact original readback, followed by confirmed device and gateway cleanup,
before releasing quarantine. A verified original value alone does not release
the bus path. Each confirmed response
requires an opaque trusted adapter watermark captured after that response;
the readback receipt must exceed it. A pre-dispatch watermark is insufficient
because an observation can arrive before the actual send. UTC-second equality
therefore cannot make a pre-restore sample valid. Missing trusted watermark
fails closed. The offline dispatcher has no independent real-device observation
authority yet. A response alone does
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

## Dispatcher increment

`dispatcher.rs` sequences fresh pre-write observation, one desired write,
independent readback, one restore write, and independent restored readback.
Only a successfully verified stage permits the next stage. A session error,
unexpected echoed route/selector/value, missing readback or cancelled future
retains durable uncertainty and prevents automatic retries or restoration.
Normal returns attempt cleanup separately; cancellation cannot
perform asynchronous cleanup from `Drop`, so the caller must drive it. Gateway
cleanup must be confirmed by the typed outer owner as well. Device-only, failed,
pending or cancelled cleanup retains the durable route quarantine even when
restoration was verified. Only complete confirmation followed by durable
cleanup evidence releases the route; reopening the journal does not bypass it.

`TypedSession` and its result types are crate-private reviewed-code boundaries,
not an extension or MCP interface. The dispatcher does not make an arbitrary
third-party implementation trustworthy. Its future concrete adapter must own
the exact gateway/session and produce protocol-correlated evidence. The root
Cargo target includes no such network adapter and no trusted human-grant
constructor; therefore this increment cannot perform the hardware write test.

On 2026-11-06, `cargo test --locked --test task020-guard` passed 31 tests
(18 core tests and 13 synthetic dispatcher tests). Workspace Clippy
with warnings denied, formatting and diff checks passed. The tests include
wrong approval/route, fresh current-value mismatch, uncertain desired/restore
writes, mismatched response/readback, cancellation and cleanup failure, pending
cleanup, journal reopen and cleanup-evidence publication failure.

This dispatcher increment is based on editor revision `2eb051a`. The final
`cargo test --workspace --locked` passed 31 guard, 17 existing state and two
existing CLI tests; formatting, scope and diff checks passed afterward.

A separate private development-only library was compiled with `cargo check
--locked --offline --lib` against the exact clean fork revision
`ae1ef36eb190b40a1fab02f7d673e12e9b2d9757`. Its concrete adapter uses only
public `Tunnel`, `ManagementSession`, typed property and descriptor APIs. The
private adapter source SHA-256 is
`da2edf28020f30a5e66a801006c8004c2c8db38f125d294353f8fde0d62fb8e2`.
The temporary local-path manifest and lockfile remain private and are not part
of the project/release manifest or lockfile. An initial fresh offline resolve
failed on a yanked transitive dependency; the check retained the fork's locked
dependency resolution before adding this private library's dependencies.

That check proves compilation only. No adapter function, network operation or
device was exercised. The draft adapter can report device cleanup only because
the used public gateway `disconnect` permits a missing response; it cannot
produce complete gateway-cleanup evidence or release the commissioning path.
Trusted human approval, actual observation authority, a verified gateway-close
adapter and a runnable end-to-end executor are still absent.
