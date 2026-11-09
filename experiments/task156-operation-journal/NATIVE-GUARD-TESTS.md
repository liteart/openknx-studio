<!-- SPDX-License-Identifier: Apache-2.0 -->
# Native guard integration tests

Base editor: `041f01e`. Task003 and Task151 are Done; Task156 has no required
phase gate. This increment adds test source only. The Windows adapter remains
disabled by default and the protected service is not rewired.

`src/guard_bridge_tests/native.rs` instantiates the actual path-included Task020
guard and dispatcher, the consumer-local `ActualGuardPersistence` bridge and
`WindowsStorage`. It uses the existing guard's test-only synthetic approval
constructor and an immediate synthetic `TypedSession`; no network, device,
production approval issuer or real observation source is involved.

Four native integration cases plus a subprocess helper are prepared:

- Complete plan larger than 2 KiB survives reopen with every plan field intact;
  operation and approval replay are rejected, and a new operation can use the
  route only after successful restoration and cleanup/release.
- A test-only native-storage wrapper publishes and reads back partial intent,
  cleanup-record and release-head bytes through real native create/set/flush
  calls, then returns an error. Reopen must block another target on the same
  gateway/bus. Verified restoration is checked separately from route release.
- Two independent OS processes race actual guard reservations for different
  synthetic targets on the same gateway/bus. Exactly one reaches the first
  synthetic write. The loser must report route quarantine or an I/O error
  accompanied by an observed native Busy lock result; unrelated failures fail
  the test. Dropping the winner's pending dispatcher future leaves quarantine.
- An OS process holds a native namespace lease while its actual dispatcher
  future is pending in the first synthetic write. Abrupt termination releases
  the OS lease, while reopening the persisted operation remains unknown and
  the route remains blocked.

Subprocess readiness, outcome and exit handshakes have 30-second bounds.
Termination uses kill followed by deadline-bound nonblocking exit polling,
including unwind cleanup; no blocking child wait is used. Failed cleanup is
reported without claiming the child was reaped.
Native tests create unique temporary-directory-derived HKCU namespaces and
retain their registry evidence, like the existing adapter tests. They do not
add registry cleanup or change production locking. These tests cannot prove
power-loss durability or protection against arbitrary same-account mutation.

On a separately authorized native Windows test environment, the focused command
is (from the repository root):

```text
cargo test --locked --offline --manifest-path experiments/task156-operation-journal/Cargo.toml --features experimental-windows-storage guard_bridge_tests::native:: -- --test-threads=1
```

The filter selects this actual-guard/native-storage integration group. Existing
filesystem-default guard tests are a separate platform qualification; they are
not silently substituted for native adapter tests.

## Checks performed here

macOS arm64, pinned Rust 1.99.1, offline cache, base `041f01e` plus this diff.
Common option: `--manifest-path experiments/task156-operation-journal/Cargo.toml`.

| Command | Result |
|---|---|
| `cargo test --locked --offline` | PASS: 69 tests and one compile-fail doctest |
| `cargo check --locked --offline --target x86_64-pc-windows-gnu --all-targets` | PASS: feature off |
| Same Windows check with `--features experimental-windows-storage` | PASS: native source type-check only |
| `cargo clippy --locked --offline --all-targets -- -D warnings` | PASS: macOS |
| Windows feature-on all-target Clippy with `-D warnings` | PASS: type-check only |
| `cargo fmt --all -- --check` and `git diff --check` | PASS |
| Focused native Windows command above | NOT RUN at this macOS checkpoint; later native run is recorded below |

No registry, remote host or hardware operation was executed at this macOS
checkpoint. The earlier NOT RUN outcome is historical; cross-compilation alone
did not establish native success. The later native run is recorded below.

## Native outcome — 2026-11-09

The feature-on harness built from editor source
`91bea8c7dc45b8e6f2ca49013c45d886cd8bfe01` ran once on Windows host
`DESKTOP-3UKTEIP` as `dev`. The combined selected run passed 15 tests in total:
four Task152 tests, seven Task156 native storage tests and the four actual
Task020 guard, dispatcher and consumer-bridge cases described above. All 11
Task156 tests passed, each with exit code 0. The run was not retried; the
runner exited 0 and scoped process inspection found no remaining test
processes. An independent reviewer audited the captured outputs and saved
pins; no separate reviewer artifact or hash was persisted.

Evidence is under
`private/evidence/task152-156-windows-native-20261109/`. The combined
`result.json` SHA-256 is
`e7baf24638e5d4c6cb32295a5f5a7295228bf67d38c61f148864724199645b5a`;
`run.json` SHA-256 is
`54685c077e3f9693e48ba4d706a71a9e65407038bf040f1cce6c85b92bb832a0`;
launcher result SHA-256 is
`5a30bb557bc06e9fc06c1af68a14b185b909ce0e14d66ff8e762377639d990ae`;
post-run task status SHA-256 is
`f2aaf5f6031d70771cafbd0722cce2693ef1c6fcbe9bca02ff93dae6bd2b3933`.
The saved run identifies the source, test binaries and runtime dependency by
hash. It records synthetic temporary paths and HKCU namespaces, with no
network, KNX, browser, service, vendor DLL or real credentials.

Separately, at editor revision
`30c4a9871acc4f67cc849bb4dffaf5434f8648a9`, the coordinator and an
independent verifier each ran `cargo test --locked --offline --manifest-path
experiments/task156-operation-journal/Cargo.toml` in Linux arm64 containers.
Each run passed 69 tests and one compile-fail doctest using an isolated
persistent Docker volume for temporary files. LinuxKit reported `ext2/ext3`;
source and Cargo registry mounts were read-only, networking was disabled, and
the volumes were removed after the runs.

PASS applies only to these bounded synthetic Windows tests and Linux container
suite. Power-loss behavior, arbitrary same-account security, passkey-provider
behavior, protected-service or production integration, and physical device
qualification remain NOT RUN. Task156 remains In review; these experiments do
not constitute maintainer acceptance or advance a phase gate.
