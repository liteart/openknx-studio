<!-- SPDX-License-Identifier: Apache-2.0 -->
# Task156 experimental Windows storage adapter

Base editor revision: `2b4207228cb1cbef1e90500dd3d412768e81c53b`.
Task003 and Task151 remain Done; no phase gate applies or is advanced.
The existing policy, Task020 guard and Task153 executor are not integrated or
replaced by this increment.

## Implemented behavior

`experimental-windows-storage` is a non-default feature. Only Windows builds
with that explicit opt-in expose `windows::WindowsStorage`. It is a storage
qualification switch, not an approval or device capability. The backend uses
the reviewed safe `winsafe` API; owned source and tests forbid unsafe code.
The dependency contains inherited FFI and is part of the trust boundary.

The caller supplies an existing absolute canonical non-reparse directory.
Its native UTF-16 representation is hashed to select a fixed HKCU experimental
namespace. The directory also holds one stable coordination file, opened read
and write without truncation or append-only mode. The adapter never deletes,
replaces or clones that file/handle. All cooperating users of this journal
must use the same canonical root and preserve that lockfile's identity.

Each lease opens a fresh file handle and uses stable Rust `File::try_lock`.
It never waits or retries: contention returns Busy. This is a stronger
namespace-wide cooperative mutex, serializing route state and global
operation/approval reservations together. It is **not a named Win32 mutex** and
does not expose a WAIT_ABANDONED signal. Closing or losing the process handle
releases only the OS lock. The policy always rechecks persisted route state;
active/unknown records still quarantine the route after an owner dies.

The lease holds the file and registry namespace handles through every policy
read, mutation and flush. Operation, approval and event records are globally
create-new keys; the actual NEW/EXISTING disposition is checked before setting
the bounded JSON record encoded in indexed hex child-key names. Existing partial keys are never overwritten
or deleted. The route head is the only replaceable record. Its permanent parent key is
never deleted; missing or partial Data children are an error, never an absent
free route. Replacement removes at most 128 known chunk keys individually
(nonrecursive deletes), then writes the new complete chunk set. Any failed
delete or torn write leaves an existing malformed record and blocks reuse.
Explicit checked `RegFlushKey` provides each policy barrier, including the
fresh barrier required before a visible released head can permit reuse.
Closing registry handles alone is not treated as persistence.

## Bounded reads and trust boundary

No registry-value reader is used. Each JSON record is at most2,048 bytes,
encoded as at most128 child keys under a fixed Data subkey. Every chunk name
contains a four-hex-digit index, the total byte length and up to16 bytes as
hexadecimal. Decoding requires consistent total lengths, exact contiguous
indices, no duplicates and the exact expected chunk widths/count. The longest
generated full key path is254 characters, within Microsoft's255-character
limit. Missing trailing chunks cannot masquerade as a shorter complete record.

The inspected safe RegEnumKeyEx wrapper allocates one buffer using the maximum
subkey-name length. Windows hard-limits that length to255 characters, so even
a concurrent noncooperating mutation cannot make this allocation unbounded.
The wrapper makes one native call per item and stops on MORE_DATA/errors;
it contains no resize/retry loop. The adapter independently takes at most129
items, rejects extra or changed metadata and bounds every parsed name/payload.
A portable infinite-source regression proves the129-item cap. No history,
class-string or value-data enumeration is performed.

Independent review rejected the first draft's metadata precheck followed by
RegQueryValueEx: that helper re-queries, allocates and retries, so concurrent
mutation could violate the hard read bound. The implementation was replaced,
not merely documented as safe under cooperative locking. Current production
source uses neither RegQueryValueEx nor RegGetValue. Chunk decode and all
intermediate deletion/creation states have deterministic regressions.

The cooperative lock protects supported writers' state integrity. Arbitrary
same-account code can still tamper with keys or replace lock paths; this is
not a sandbox or protected operator service. Resource bounds do not rely on
that account cooperating. Canonical path, symlink and Windows reparse checks
reduce accidental aliases without claiming descriptor-bound protection from
same-user races. All configured processes must preserve the lockfile identity.

## Primary API evidence

- [Rust 1.99.1 File::try_lock](https://doc.rust-lang.org/std/fs/struct.File.html#method.try_lock):
  exclusive, immediate acquisition; Windows uses LockFileEx; locks release when
  all owned/duplicated/inherited handles close. The adapter duplicates none.
- [Microsoft file byte-range locking](https://learn.microsoft.com/en-us/windows/win32/fileio/locking-and-unlocking-byte-ranges-in-files):
  process termination/handle closure releases locks; this does not establish
  persistent operation recovery.
- [RegCreateKeyExW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regcreatekeyexw):
  nonvolatile key creation and NEW versus EXISTING disposition.
- [Registry element size limits](https://learn.microsoft.com/en-us/windows/win32/sysinfo/registry-element-size-limits):
  the native hard key-name bound limits the wrapper allocation.
- [RegEnumKeyExW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regenumkeyexw):
  one indexed key per call, error on an undersized buffer.
- [RegDeleteKeyW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regdeletekeyw):
  nonrecursive deletion fails if a child has subkeys.
- [RegQueryInfoKeyW](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regqueryinfokeyw):
  metadata count/name queries before bounded chunk enumeration.
- [RegFlushKey](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regflushkey):
  explicit hive persistence barrier. It can be expensive and block unrelated
  writes in that user's hive; storage hardware and power-loss behavior still
  need qualification.

The pinned winsafe package source was inspected, including its query helpers,
checked status propagation, NEW disposition and owned-handle disposal. Package
checksums and license metadata for all 34 locked packages matched the existing
offline cache; no package was downloaded for this increment. New direct pins:
`winsafe = 0.0.29` (MIT, MSRV 1.87, `advapi` implies `kernel`, no transitive
crate dependencies), `sha2 = 0.11.0` (also used by the portable guard bridge), and test-only
`tempfile = 3.24.0`. See `DEPENDENCIES.tsv` and Cargo.lock.

Inspected winsafe source SHA-256:

| File | SHA-256 |
|---|---|
| LICENSE.md | `3ee62f0338e87ec3b8c1730c9d2d1ccbb169b8b869924511681833599cfb8a3d` |
| src/advapi/handles/hkey.rs | `a36eff01e88b196e7703a63a5b031ec250e4358f0ff2bce7794df1483d17de16` |
| src/advapi/guards.rs | `761b617dab4cab9940c6b17b80ce750f00a37adf6f7a8713f58fcb7f917d445b` |
| src/advapi/iterators.rs | `f5d95685bf1ac7a09dc76f8559ae0392812216366bab9856ac79bbb16d2a89f5` |
| src/advapi/enums.rs | `031bb257bb15e018884d8e15f8b5389f1928b565f128dd080641d915bb09ba74` |

## Checks executed

macOS arm64, pinned Rust 1.99.1, offline project-private cache and task-owned
target directory. No Windows program, registry operation, remote host or
hardware was executed. Exact commands, with the common manifest option
`--manifest-path experiments/task156-operation-journal/Cargo.toml`:

| Command | Result |
|---|---|
| `cargo test --locked --offline` | PASS, 24 test functions and one feature-off compile-fail doctest |
| `cargo check --locked --offline --target x86_64-pc-windows-gnu --all-targets` | PASS, feature off |
| Same check with `--features experimental-windows-storage` | PASS, native source/tests type-check only |
| `cargo clippy --locked --offline --all-targets -- -D warnings` | PASS, macOS |
| `cargo clippy --locked --offline --target x86_64-pc-windows-gnu --features experimental-windows-storage --all-targets -- -D warnings` | PASS, Windows feature on |
| `cargo fmt --all -- --check` | PASS |
| `git diff --check` | PASS |

The macOS suite includes the 14 previous fault-policy tests, five chunk/layout
tests and five OS-lock test functions (one is a subprocess entrypoint). Real
separate handles and a terminated child process were tested for cooperative
OS-lock release on macOS. This is not Windows OS-lock evidence.

An initial amended Windows check caught a test-only Option result mismatch;
it was corrected. Clippy required a fixed-size chunk iterator; its correction
also passed the final suite.

Prepared Windows tests cover exact registry ledger readback/reopen/release,
partial/oversized chunks, unexpected values and torn route payloads, competing threads and separate processes,
and terminated lock owners whose durable active route remains quarantined.
They create only unique synthetic namespaces and leave their registry evidence
in place; no production deletion API is added. All native Windows tests are
**NOT RUN**. Native process/storage recovery, power loss, Linux execution,
protected-service/guard integration and physical device qualification remain
open. Independent review is required before accepting this source increment;
Task156 itself remains incomplete.

## Native test child cleanup correction — 2026-11-08

At editor base `7aae1bb`, this older native test group still used blocking
`Child::wait` in its cleanup guard and abrupt-owner test. Both paths now use
deadline-bound `try_wait` polling. A failed status query still attempts
termination; kill failure or a missing exit observation is an explicit error.
The abrupt-owner test first requires an expired polling deadline to return a
timeout, then confirms a non-successful terminated child before checking that
the persisted route remains quarantined. Test cleanup reports uncertainty
instead of blocking indefinitely or claiming a child was reaped.

macOS regression: 69 tests and one compile-fail doctest PASS. Windows GNU
feature-on all-target check and Clippy with warnings denied PASS; formatting
and diff checks PASS. An initial macOS build emitted a `rust-objcopy`
debug-stripping warning while tests passed; a repeat with explicit no-strip
configuration passed without that warning. The native cleanup assertions are
type-checked only: Windows execution remains NOT RUN. Production storage,
locking and dependencies are unchanged. Previously cross-linked test
executables must be rebuilt before running this complete native group.
