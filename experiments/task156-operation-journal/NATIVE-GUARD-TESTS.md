<!-- SPDX-License-Identifier: Apache-2.0 -->
# Prepared native guard integration tests

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
| Focused native Windows command above | NOT RUN: no Windows execution environment used |

No registry, remote host or hardware operation was executed here. Native
runtime outcomes, Linux execution, physical service integration and Task156
acceptance remain open. Cross-compilation does not establish native success.
