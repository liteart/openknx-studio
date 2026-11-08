<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task152 development-journal preflight

Base: editor `ef5e33187c50663f22607bf752308889ebce69d9`. Task152's accepted
Task019/151 prerequisites and existing offline guard fixture permit this
bounded verifier change; no phase gate or physical operation is unlocked.

## Reproducible problem and changed behavior

Previously the verifier opened without checking its containing-directory
persistence operation. Only after a user completed registration and assertion
did it attempt to synchronize a new receipt's file and directory. A platform
or filesystem without the required directory operation could therefore fail
late, after user interaction and file creation. The previous code correctly
withheld the receipt on failure, but provided no early platform qualification.
There was no native Windows run; the Windows limitation is source-derived.

The public constructor now calls the concrete directory-sync operation on the
actual canonical journal before constructing the verifier. This preflight
creates no file and cannot enroll or consume a credential or challenge.
Failure returns `JournalDurabilityUnavailable`; an unimplemented platform
returns `UnsupportedJournalPlatform`. The browser runner constructs its
verifier before opening its listener, so a failed preflight cannot expose
ceremony endpoints. Both file synchronization and a new directory barrier
remain mandatory after each receipt write. A failure preserves any newly
created consumption file and returns no successful observation; it never
deletes it to permit a retry.

The concrete implementation is enabled for macOS and Linux. At this original
checkpoint tests ran only on macOS arm64. A later Linux-container result is
recorded below; it does not establish native-host filesystem durability. Windows
and other platforms have no reviewed parent-directory persistence implementation
and fail explicitly at construction. The native toolchain has only the
`aarch64-apple-darwin` target. This increment adds no Windows implementation,
platform bypass, dependency, credential store or OS service abstraction.

The path remains owner-writable and is not a trust boundary against the same
principal. Preflight cannot promise that a future barrier will succeed or that
every filesystem/device survives power loss; each receipt still performs and
checks both barriers. The protected Task153 store and installed service are
unchanged.

## Primary-source basis

Inspected 2026-11-08:

- [Rust 1.99.1 File API](https://doc.rust-lang.org/std/fs/struct.File.html):
  `File::open` is read-only, and `sync_all` reports synchronization errors.
- [Microsoft FlushFileBuffers](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-flushfilebuffers):
  the API requires write access. A read-only directory handle is not established
  as a Windows metadata-persistence barrier. This is why the existing Unix
  sequence is not silently declared portable.
- [Linux man-pages fsync](https://man7.org/linux/man-pages/man2/fsync.2.html):
  persisting a file does not by itself persist its directory entry; a separate
  directory synchronization is required. Actual filesystem behavior remains
  subject to native qualification.

These sources support retaining explicit file and directory barriers and
rejecting unimplemented platforms. They do not prove Windows or Linux runtime
behavior in this experiment.

## Tests and results

All paths and credentials are synthetic. New tests check:

- Constructor rejection on unavailable barriers, without creating any file or
  returning a verifier from which a ceremony could begin.
- Actual directory preflight on a temporary directory creates no probe file;
  actual receipt persistence uses file and directory synchronization.
- Injected file-sync failure prevents the directory barrier; injected
  directory failure occurs only after successful file sync. Both retain the
  receipt path and reject overwrite as Replay. These are injected ordering
  tests, not real disk-fault evidence.
- After successful preflight, making the actual journal unavailable causes a
  genuinely signed synthetic assertion to fail without a receipt. The pending
  challenge is consumed and cannot be retried.

Temporarily omitting the new preflight invocation made the focused constructor
regression FAIL; restoring it made the test PASS. This is a deliberate mutation
check of the new regression, not a claimed observed Windows failure.

Executed on macOS arm64 with pinned Rust 1.99.1 and locked offline dependencies,
using the repository-private toolchain/cache and an isolated private target:

| Command | Result |
|---|---|
| `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --lib` | PASS, 56 tests |
| `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --doc` | PASS, one compile-fail test |
| `cargo test --locked --offline --manifest-path experiments/task152-passkey/Cargo.toml --bin task152-browser-demo -- --skip loopback_connection` | PASS, six handler tests; one socket test explicitly not run |
| `cargo clippy --offline --locked --all-targets --manifest-path experiments/task152-passkey/Cargo.toml -- -D warnings` | PASS |
| `cargo fmt --manifest-path experiments/task152-passkey/Cargo.toml --all -- --check` | PASS |
| `git diff --check` | PASS |

No external network, authenticator UI, real credential, protected service,
installer, remote host or KNX hardware was used. Native Windows/Linux,
power-loss behavior, protected cross-platform service and provider ceremonies
remain **NOT RUN**. Task152 acceptance and source integration still require
independent review and coordinator action.

## Linux container verification — 2026-11-08

The coordinator and an independent verifier each ran the 56 library tests at
editor revision `5a36820` under Linux arm64 in the local Docker LinuxKit VM
(kernel 5.15.49), using Rust 1.99.1 and a tmpfs `/tmp`. The pinned image was
`rust@sha256:f2c004e3d2d8b547e8cf7ffbc29205385f4e71d23e53e4e383065db5e9f822cd`.
Both runs passed with no network, read-only source and Cargo registry mounts,
and temporary build output. The command inside the container was
`cargo test --locked --offline --manifest-path /work/experiments/task152-passkey/Cargo.toml --lib`.
This covers Linux compilation and the actual directory barrier on tmpfs; it
does not qualify a native-host filesystem, power-loss durability, browser
socket, real passkey provider, or Windows. The original checkpoint above is
historical, and Task152 remains in progress.

At editor revision `6aea59752cd15d340cc32b0c571ef8e1de329df9`, the same
locked/offline library command passed 66 tests in two independent Linux arm64
container runs using isolated persistent Docker volumes for `TMPDIR` rather
than tmpfs. LinuxKit reported the filesystem as `ext2/ext3`; the test volumes
were removed afterwards. This exercises the real directory barrier on that
VM filesystem but does not simulate power loss or qualify a native host,
browser/provider ceremony or Windows.
