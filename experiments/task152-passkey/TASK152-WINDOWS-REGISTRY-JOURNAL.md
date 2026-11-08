<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task152 Windows development journal

Base: editor `e539c82`. This code is confined to development observations;
it changes neither the protected Task153 service nor physical approval.
Native Windows execution is required before qualification. Task152 remains open.

Default Windows construction remains fail-closed with
`UnsupportedJournalPlatform` before a ceremony or consumption write. The new
registry backend and its dependency are selected only by the non-default
`experimental-windows-journal` feature. This is an explicit qualification
opt-in, not authorization or a claim of supported Windows operation.

## Persistence and race handling

With the experimental feature, Windows uses the current user's registry under the fixed prefix
`Software\OpenKNXStudio\DevelopmentApproval\JournalsV1`. A namespace is the
SHA-256 of the canonical journal path's native UTF-16 code units. Names
contain neither the local path nor private credential data. The code uses
`RegOpenCurrentUser` and explicitly selects the 64-bit view for create
operations. Registry/files remain mutable by the same principal; there is no
claim of independent protection or trusted transaction display.

Preflight opens/creates the development namespace with required query/set/
create rights and flushes it. It creates no per-challenge slot or credential,
but may create the persistent namespace: this Windows probe is not
mutation-free. The per-assertion sequence is:

1. Create one nonvolatile subkey named by the validated 64-character challenge
   digest. Only the API's `REG_CREATED_NEW_KEY` disposition may proceed. Any
   existing key is Replay, including malformed or empty partial state. No
   separate existence check, unbounded value read or overwrite is performed.
2. Flush the new key, persisting its reservation.
3. Create the portable `.spent.json` copy exclusively and synchronize its file.
4. Set one `REG_BINARY` value containing the same JSON receipt, bounded to
   2,048 bytes, then flush the key again before returning success.

The registry keeps the complete JSON observation if a Windows file-directory
entry is lost. File synchronization alone is not the Windows durability proof.
No failure deletes a reservation or triggers retry. A failed flush withholds
success and leaves any present key refused; its survival after power loss is
not claimed. Two processes racing the same challenge rely on the disposition
of the single OS create/open operation; only one can create a new record.
The policy has no process-local lock presented as an interprocess boundary.
Fresh challenges can produce new development observations; physical-operation
replay prevention still belongs to the protected backend.

`RegFlushKey` is synchronous, potentially blocking and expensive across its
hive. This implementation is for the low-volume prototype; it does not claim
an IO cancellation deadline or performance qualification.

## Primary sources and dependency review

Inspected 2026-11-08:

- [RegCreateKeyExW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regcreatekeyexw):
  nonvolatile persistence, NEW/EXISTING disposition, rights and handle ownership.
- [RegFlushKey](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regflushkey):
  query access and waiting for the containing hive's data to reach its disk store.
- [RegSetValueExW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regsetvalueexw):
  explicit binary data and checked status.
- [RegOpenCurrentUser](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regopencurrentuser)
  and [registry view guidance](https://learn.microsoft.com/en-us/windows/win32/winprog64/accessing-an-alternate-registry-view):
  thread-user selection and consistent view flags.
- [Registry size guidance](https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry-element-size-limits):
  the prototype limits values to 2,048 bytes and bounds all key names.

The final implementation uses the safe `winsafe = 0.0.29` API, target-Windows
only, optional behind `experimental-windows-journal`, with its default features
off and `advapi` enabled. The published normalized
manifest declares MIT, Rust 1.87, no dependencies and `advapi => kernel`.
Only this exact crate was fetched, through `cargo info winsafe@0.0.29`; no
project dependency update was run. The final offline lock resolution adds
one package and its root dependency edge, with no unrelated version changes.
It is included in `DEPENDENCIES.tsv`.

Reviewed source: `src/advapi/handles/hkey.rs` supplies safe current-user
opening, nonvolatile key creation/disposition, value write and checked flush.
`src/advapi/guards.rs` supplies the non-clone `RegCloseKeyGuard`, which closes
owned handles on Drop. Durability is still established by checked flushes,
never by Drop. The project module uses `#![forbid(unsafe_code)]`, including its
native tests. The dependency itself contains unsafe FFI; this review does not
claim it is absent or audit every optional API. No PIN, private key, arbitrary
registry path or shell command is accepted by the production API. The safe
value reader is used only by the native test on its own synthetic key;
production does not read existing registry values to make reuse decisions.

Exact reviewed crate checksum:
`9ef0ffc427f045c0cc9ebffd6f4f91153dcd2a1547b5c3c29ee3f541e16e95c6`.
License SHA-256:
`3ee62f0338e87ec3b8c1730c9d2d1ccbb169b8b869924511681833599cfb8a3d`.
Registry API source SHA-256:
`a36eff01e88b196e7703a63a5b031ec250e4358f0ff2bce7794df1483d17de16`.
Guard source SHA-256:
`761b617dab4cab9940c6b17b80ce750f00a37adf6f7a8713f58fcb7f917d445b`.
The upstream MIT notice remains in the dependency and must accompany any
future binary distribution. No upstream source was copied into owned files.
This is not a full dependency audit or legal opinion.

## Verification and remaining work

macOS arm64, pinned Rust 1.99.1, locked offline dependencies:

- `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --lib`:
  PASS, 60 tests. Four new policy tests use an owned fake registry to cover
  ordering, each failure stage, retained reservations, concurrent exclusion
  and bounds. These do not exercise Windows APIs.
- `cargo clippy --locked --offline --all-targets --manifest-path experiments/task152-passkey/Cargo.toml -- -D warnings`:
  PASS for macOS; Windows code is not type-checked by this command.
- `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --doc`:
  PASS, one compile-fail test.
- `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --bin task152-browser-demo -- --skip loopback_connection`:
  PASS, six browser-handler tests; the socket test was explicitly filtered out.
- Formatting and `git diff --check`: PASS.

The first isolated macOS build failed with ENOSPC; its incomplete generated
cache was removed. The successful run reused this agent's existing compiled
target. No shared toolchain was changed. Docker reported its daemon unavailable,
so the current Linux regression is NOT RUN; no image was pulled or container
started. Initially no Windows target was installed, and target downloads were
deferred while disk space was constrained. Subsequently, the coordinator
installed the pinned Windows GNU standard-library target. With an isolated
agent-owned target directory and `CARGO_PROFILE_DEV_DEBUG=0`, this command
completed successfully:

`cargo check --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --target x86_64-pc-windows-gnu --all-targets`

The same command with `--features experimental-windows-journal` also passed.
The default configuration type-checks the fail-closed Windows path; the
feature-on configuration type-checks the registry backend and its native tests.
Neither links or executes Windows code. Windows-target Clippy passed for both
configurations with `--all-targets -- -D warnings`; an initial test-only
range-loop warning was corrected before the final passes. macOS library
regression (60 tests), formatting and whitespace checks passed after feature
gating. An actual `cargo tree --locked --offline --target
x86_64-pc-windows-gnu --edges normal --prefix none --format '{p}'` comparison
confirmed `winsafe` is absent by default and present only with the explicit
feature. This verifies dependency selection, not native registry behavior.

Native Windows tests are supplied for key/value readback, empty/malformed key
refusal, competing threads and two separate child processes. They are NOT RUN.
The default Windows regression checks the public constructor's explicit
unsupported error and that even the private persistence entry refuses before
creating consumption files. It is compiled by the feature-off check but native
execution remains NOT RUN.
Their synthetic HKCU keys intentionally remain consumed evidence. No native
Windows passkey provider, registry execution, power-loss recovery, protected
service, remote machine or KNX operation was exercised. Unix production
behavior is preserved. Independent review and native Windows validation remain.

The imported Task020 guard retains its own separate filesystem journal and
directory-sync behavior. This change does not replace that journal or establish
Windows execution of the guard's synthetic integration tests.
