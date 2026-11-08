# Task020 offline exact-plan safety core

The default Cargo target is a test-only editor-side model. It contains no
executable, networking, KNX codec, fork dependency, credential discovery or
physical-device adapter. Its only Cargo registration is the `task020-guard`
test target. The separate development adapter below is built only through an
explicit offline runner and cannot run the planned hardware test.

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

The default remains this filesystem journal. An explicitly supplied trusted
persistence backend can replace storage through the same guard validation;
see [the Task156 persistence contract and tests](../task156-operation-journal/GUARD-PERSISTENCE.md).
That seam preserves complete intent records and provides no approval authority.
The native Windows adapter is not connected to it yet.

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

## Reproducible development adapter check

`knust_adapter.rs` now contains a development-only concrete `TypedSession`
adapter. It uses the fork's public `Tunnel`, `ManagementSession`, typed property
and descriptor APIs. One bounded actor owns the tunnel and borrowed device
session. It consumes opaque dispatch tokens, checks the route and latest
descriptor observation, and sends each selected write once. After the device
session borrow ends, it consumes the tunnel through `GatewayDisconnectAttempt`.
Complete cleanup requires both device-local confirmation and a successful
same-channel gateway response. Cancellation or an incomplete receipt never
releases the durable route quarantine. An abandoned caller does not abort an
in-flight actor operation: the bounded operation settles and the actor closes
its device/gateway sessions without retrying or restoring uncertain writes.

This source has no root Cargo registration, production grant constructor,
approval CLI, hardware command, or release dependency. The exact development
fork pin is `4d296afb0906d17b6724b56c912f2e9e3f6680a5`. A clean local checkout
of that revision and cached dependencies are required. With the repository's
pinned Rust 1.99.1 toolchain available on `PATH`, run:

```sh
python3 experiments/task020-guard/run_adapter_check.py --fork-path /path/to/local/knust-fork
```

The runner verifies the exact clean revision before building anything. It
creates a fresh temporary directory outside the repository for the generated
manifest, lockfile, build output, journal evidence and logs. Fork sources are
exported from raw pinned Git blobs with verified hashes, lengths and modes;
worktree/index contents, hidden-index flags, ignored files, archive filters and
Git replacement objects cannot supply build inputs. Symlinks, submodules and
unreviewed Cargo configuration in the pinned tree are rejected. Every exported
file's provenance is recorded in the private result.

Cargo always runs offline using a fresh Cargo home with registry caches only.
User Cargo configuration, credentials, compiler wrappers, build overrides and
Rust flags are not inherited; ancestor Cargo configuration fails the check.
The exported fork's lockfile seeds only the temporary dependency resolution;
subsequent checks use `--locked`. Neither repository's manifest or lockfile is
changed, and no fork source enters the editor repository. The enabled fork
features must be exactly `dpt`; Secure is disabled.

The harness copies the reviewed core and dispatcher byte-for-byte, then appends
one visible `cfg(test)` descendant module to the temporary core copy. This is
the only instrumentation and permits synthetic test approvals without opening
the production approval type. Test fixtures contain an invented target and
opaque property, not the user-selected device or a manufacturer support claim.
The runner launches only the fork-owned simulator, which binds an ephemeral
IPv4 loopback endpoint. Tests validate that endpoint before passing it to the
actual adapter. No configurable hardware endpoint or target is accepted.

`actor_tests.rs` covers the actual adapter actor/observe/write/close paths:

- Normal `00` → `04` → `00`, with four separate descriptor/property reads and
  distinct device/gateway cleanup.
- Cancellation after queue handoff but before the actor processes a write.
- Cancellation before and after a synthetic write effect.
- Bounded write uncertainty, without automatic retry or restoration.

The failure paths retain quarantine across journal reopen and reject a new
operation. `validate_actor_logs.py` separately checks peer event order, exact
values/write counts, stderr, final cleanup events and durable journal files.
It uses explicit errors rather than Python assertions, bounds artifact reads
and directory entries, and rejects corrupted evidence even under `python -O`.
It is a read-only artifact parser, not independent review. Five focused Python
regressions cover raw exports, modes/hash mismatch, isolated build environments,
corrupted logs with optimization and oversized reads. The runner executes these
and the entire temporary Rust suite, Clippy and formatting, then writes `result.json`
and prints its private artifact directory. Missing caches, toolchain, loopback
permission, expected responses or cleanup fail the check explicitly. Individual
commands have a 180-second watchdog; simulator lifetimes are 20 seconds.

On macOS arm64 with Rust 1.99.1, the reproducible runner passed all 39 temporary
Rust tests (31 core/dispatcher, five actor and three adapter-port tests), Clippy,
formatting and separate artifact validation. The default editor workspace's
31 guard, 17 state and two CLI tests also passed, together with workspace
Clippy, formatting, scope and diff checks. Independent integration review is
pending. Windows,
physical-device writes/readback/restoration, trusted operator approval and
production integration remain NOT RUN or unavailable. Simulator success does
not satisfy Task020's physical acceptance criterion.
