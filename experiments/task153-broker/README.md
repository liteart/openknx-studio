# Task153 portable broker input contract

This non-shipping Rust library defines the bounded input and configured-plan
policy for a future protected operator service. A separately explicit macOS
peer-diagnostic executable is present; it is not an executing broker and has
no protected store, passkey enrollment, approval issuer or KNX adapter.
`UnprovisionedBroker` always rejects a valid request with
`protected_host_unavailable`; it does not open a journal or contact a device.
There is no test fixture that turns this facade into a protected service.

## Implemented boundary

The JSON request is limited to 4096 bytes and a 64-character ASCII request ID.
Unknown fields, duplicate fields, unsupported versions and unknown operations
are rejected. The two operations are `inspect_plan` and `execute_approved`;
the latter carries only the exact plan digest. Requests cannot select a route,
device, property, value, credential or grant, and cannot start enrollment.
The operation name is a request, never proof of approval or permission.

`ConfiguredPlan` reuses Task152's immutable validated snapshot and Task020's
digest. Its shape is restricted to object 0, property 14, element 1, one byte,
original `00`, desired `04`. Restoration is the original byte. The route and
identity must eventually be loaded from the protected service configuration;
constructing this value currently does not establish configuration trust or
limit its source to the selected physical bench. Test routes are synthetic.
The exact same snapshot supplies both the summary and request digest check.

The library also supplies tested macOS peer and file-inspection primitives.
No installed service privilege boundary is established, and it cannot release
a physical capability.
It deliberately reuses the existing plan representation instead of adding
another approval ledger or repeating the dispatcher simulator.

## macOS kernel peer identity

The macOS-only `macos_peer` module now obtains the effective UID and GID of a
connected Unix-domain peer through the safe `nix::unistd::getpeereid` wrapper.
It never reads a claimed identity from request JSON. The matching policy rejects
the configured agent UID, rejects a shared operator/agent UID, and requires the
expected operator UID and GID. `PeerIdentity` has private fields and cannot be
deserialized. It is connection evidence, not an approval or host token.

On macOS arm64, three tests used actual temporary Unix-domain sockets to check
the kernel-observed UID/GID at both endpoints and rejection of the current
principal and deliberately mismatched policy IDs. The tests created no users,
inspected no credentials and changed no service or system configuration. They
do not prove that another principal exists or that the agent cannot acquire
administrative authority. A successful distinct-UID operator connection and
installed configuration ownership/ACL/IPC protection remain **NOT RUN**.
The facade remains unprovisioned and cannot dispatch.

Primary API evidence inspected on 2026-11-06:

- [Apple getpeereid manual](https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man3/getpeereid.3.html)
  documents effective credentials fixed at connect/listen time.
- The downloaded `nix 0.31.3` source `src/unistd.rs:3999` implements a safe
  `AsFd` wrapper, enabled only by the `socket` and `user` features on BSD targets.
  Its narrowly encapsulated upstream FFI is not a claim that the dependency
  contains no unsafe code; this project increment adds no unsafe block.

## Root-only diagnostic helper source

`task153-peer-diagnostic` is source for a later operator-driven OS-boundary
diagnostic. Its only accepted arguments are `--diagnostic-only --agent-uid N`,
where N is nonzero. It checks actual effective UID and GID are both zero before
any filesystem operation. It accepts no configurable path, command, credential,
plan or device request.

The endpoint is fixed at
`/private/var/db/org.openknxstudio.operator-diagnostic/peer.sock`. The helper
checks fixed ancestors through descriptor-bound owner/type/mode and native ACL
inspection, exclusively creates its own
0700 directory, and creates a 0600 socket inside. It refuses any pre-existing
directory instead of unlinking or repurposing it. It has a five-minute lifetime
and a 32-connection limit. Normal shutdown closes the owned listener and leaves
the directory/socket quarantined. It performs no path-based unlink: a swapped
directory or socket must never be deleted by a stale owner. Leftovers after
normal shutdown, failed creation or a crash cause the next start to fail rather
than being reused. Removal needs a later independently reviewed operator
procedure; privileged launch remains gated on ACL and ownership review.

The socket returns only a fixed identity-match or denial JSON response with
`physical_authorization:false`. It reads no request body, echoes no identity,
and performs no enrollment or device effect. It matches only a root/wheel
operator peer distinct from the configured nonzero agent UID. These filesystem
checks do not validate executable integrity, actual enrollment or the installed
service's privilege relationship. They do not establish resistance to an
agent that can obtain root privileges. The broker facade stays unprovisioned.

Build source without administrative authority:

```sh
cargo build --offline --locked --manifest-path experiments/task153-broker/Cargo.toml --bin task153-peer-diagnostic
```

**Do not launch a privileged helper from the agent-writable checkout.** A later
reviewed provisioning step must verify the binary digest, install it into a
protected location, verify executable/configuration/ancestor ownership and
ACLs, establish the actual operator/agent privilege relationship and provide
the explicit OS-authenticated human action. No installer, package signature,
launchd registration or elevated start is supplied or invoked by this
increment. A successfully verified separate-principal diagnostic session is
still **NOT RUN**. The helper is deliberately not wired to the passkey or
dispatcher until those prerequisites have evidence.

Six focused helper tests check strict arguments, fixed ancestors' actual
read-only metadata, the actual unprivileged
process's startup denial, and a real temporary Unix-socket agent connection's
fixed denial response. Additional real temporary-socket tests prove endpoint
drop preserves quarantine/refuses reuse and retains substituted paths. They do
not start the fixed root endpoint. If tests are
run as root, the unprivileged-start test cannot establish that particular
negative; the recorded run used the normal macOS development principal.

## Descriptor-bound configuration provider

The macOS-only `macos_config` module uses
[`exacl` revision `18275aa1a82aae9a786b108b0db0992a4d06e6e2`](https://github.com/byllyfish/exacl/tree/18275aa1a82aae9a786b108b0db0992a4d06e6e2)
with `BorrowedFd` in `getfacl`, routing to macOS `acl_get_fd_np`. It opens each
component relative to the already opened parent using `openat` and
`O_NOFOLLOW`, and checks metadata and ACL against that same descriptor. Every
ancestor must be a root-owned directory without group/world write permission.
All extended allow entries are rejected conservatively, including read-only
ones. Only explicitly recognized user/group deny entries are accepted; the
pinned library maps unknown native tags to `allow=false` with unknown kind,
which is rejected rather than assumed to be a deny. Native IDs avoid account-name lookup.
An unsupported or failed ACL read rejects the path.

The fixed reader can inspect only
`/private/var/db/org.openknxstudio.operator/config.json`. The leaf must be a
root-owned regular file with one link, no group/world write and no granting
ACL. Reads are limited to 16 KiB from the opened descriptor; metadata is
checked again after reading, including nanosecond mtime and ctime. It creates
no store and writes no credential.
`RootOwnedSnapshot` is file-inspection evidence, not proof of installed service
integrity, enrollment or operator authority. The service facade stays disabled.

Eight tests use actual temporary files and native ACLs to reject an allow ACE
despite POSIX mode 0600, reject file/directory symlinks and non-root ownership,
prove rename leaves contents/ACL checks bound to the original descriptor, and
enforce bounds. Policy regression fixtures reject unknown native-tag output
and read-only allow entries while accepting recognized deny entries. A
deterministic metadata fault injection rejects changed ctime even when size
and mtime are identical. Read-only inspection of the actual root-owned
`/private/var/db` ancestry passed on this Mac. No operator configuration file was
created or successfully provisioned. These checks describe current ownership
and ACLs; they do not revoke a previously acquired writable descriptor. Later
protected installation must create fresh inaccessible objects and establish
the actual actor/administrative boundary. A privileged adversary is outside
the claimed primitive. No `/dev/fd` pathname workaround is used.

Primary references:

- [Apple descriptor ACL API](https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man3/acl_get.3.html)
- [Pinned safe descriptor input](https://github.com/byllyfish/exacl/blob/18275aa1a82aae9a786b108b0db0992a4d06e6e2/src/aclfile.rs)

## Read-only enrolled credential verifier

The macOS-only `root_passkey` module loads its enrollment only through the fixed
descriptor-validated root configuration reader. There is no public byte parser,
credential import, enrollment endpoint or mutable public-key setter. The file
schema is version 1 with `kind: "operator"`, bounded `issuer` and a serialized
`webauthn-rs::Passkey`. The WebAuthn library explicitly supports persistence
of this public verification record: credential ID, public key and security
metadata; it contains no authenticator private key or PIN. The kind label does
not prove enrollment happened: independently protected operator enrollment and
installation remain required and have not been performed.

The verifier fixes RP `localhost` and exact origin `http://localhost:15300`,
requires user verification, and holds each random challenge server-side against
the immutable configured plan digest for at most 120 seconds or plan expiry.
Every response consumes its ceremony, including invalid, altered, cancelled or
expired responses. Enrollment JSON and assertion JSON reject duplicate keys at
every depth as well as invalid/oversized input. Pending state is not serialized.

RP scope is the hostname, not the port: a `localhost` passkey is not inherently
exclusive to this operator service. The exact assertion-origin check and the
protected record/provisioning boundary remain essential. The successful earlier
Safari demonstration used the development origin on port 15200. No browser or
real passkey ceremony on port 15300 is claimed by these tests.

Successful signatures return only `RootAssertionObservation` with
`physical_authorization:false`. Credential counter/backup metadata updates are
applied in memory and mark `credential_update_pending`; nothing writes or
acknowledges them as durable. A durable protected credential update and one-use
execution ledger must exist before observations can authorize real effects.
The trust anchor public key is never replaced by assertion data. The
unprovisioned broker facade and diagnostic remain disconnected from this
verification path and cannot issue a device capability.

Six new tests use generated software-authenticator credentials and genuine
signatures. They exercise serialization roundtrip, counter-change status,
changed plan, replay, expiry, cancellation, wrong origin/RP, unknown credential,
missing UV, invalid signature, schema, nested duplicates and input bounds.
Private parser tests do not fabricate a `RootOwnedSnapshot`, load the native
configuration or access any actual user credential. A compile-fail test proves
callers cannot use the private parser. The public fixed-file loader remains
unexercised until a separately approved operator enrollment has been provisioned.

## Private durable enrollment staging

The private, dormant `enrollment_store` module supplies a fixed-leaf create-new
primitive for a future protected registration ceremony. Its wrapper requires
actual root/wheel credentials and an existing descriptor-validated parent. It
serializes only a standard public `Passkey` record with bounded issuer/schema,
then opens `enrollment.pending.json` relative to that parent with
`O_CREAT|O_EXCL|O_NOFOLLOW`. Existing active `config.json`, existing pending
files and symlinks are refused without alteration. New file mode is set to
0600 by descriptor, and creator ownership, regular-file type, link count and
native ACL are checked before writing. Success follows actual file `sync_all`
and parent-directory `sync_all`, in that order.

Staging alone never publishes `config.json`. On staging failure, the code does
not remove the pending file and refuses retry while it exists; its directory
entry is not guaranteed durable before successful directory sync. Staging and
private activation now hold an exclusive native flock on the descriptor-checked
empty `enrollment.lock`. Readers hold the shared lock while inspecting the
fence and reading the active record. A busy lock fails immediately. There is
no arbitrary path/byte import API or externally callable writer. The module
is not connected to an endpoint, the verifier or the diagnostic helper. No
real root store was created.

Seven tests use actual temporary directories and generated public passkey
records to check create-new/no-overwrite, existing active file and symlink
protection, descriptor-parent rename binding, bounds and the actual
unprivileged wrapper denial. Deterministic fault injection at creation, after
writing and after file sync leaves the pending file present; the successful path executes
native file and directory sync calls. These are stage-fault tests, not a
power-loss test or a trace proving storage hardware behavior.

Private activation reopens the staged record without following symlinks and
checks its exact bytes, inode, owner/mode/link count and ACL. It creates and
synchronizes an `activation.pending` fence and its parent directory before
publishing anything. `rustix::fs::renameat_with` with `NOREPLACE` invokes Darwin
`renameatx_np(RENAME_EXCL)`; the destination cannot be overwritten even if it
appears after validation. The published inode is rechecked. On unsupported
systems the safe library rejects the flagged rename rather than falling back
to an overwriting operation.

The commit point is successful parent-directory synchronization after the
rename. Once fence creation has succeeded, failures before that point leave the
fence; validation or open failures can occur before a fence exists. The reader
explicitly
rejects a fenced record after reopen, even if a complete `config.json` exists.
Activation never automatically retries or removes a failure fence. After the
commit point, the record is durable. A fence-unlink or final directory-sync
problem returns `CommittedCleanupUncertain`, rather than claiming no effect.
The fence may remain and block the reader, or its removal may be lost on a
crash; if it is absent, the active record was already synchronized before
cleanup. Fence cleanup uses a protected parent and cooperating lock; its
stat-then-unlink sequence is not an atomic inode-conditional deletion. Only
fully synchronized cleanup returns `Activated`. These are
enrollment outcomes, not device approval capabilities.

Seven additional real-filesystem tests cover exact activation, changed record,
existing and racing destination, reader-lock exclusion, precommit interruption
with fence/reopen refusal, postcommit cleanup uncertainty and substituted source
inode rejection. Fault injection tests the specified state transitions; this
does not establish power-loss behavior of the storage hardware.

A safe explicit enrollment ceremony still requires the independently installed
operator service and protected UI/peer path. That ceremony must obtain the
operator's real passkey registration result; root UID, a record label or this
private persistence primitive does not prove human initiation. Durable
credential updates, protected ceremony wiring and the execution ledger remain
unimplemented. No user action to run a root command is requested by this code.

## Remaining implementation

Before this contract can front an executing service, the macOS provider must
verify the actual service identity, executable/configuration ownership, access
policy and IPC peer credentials. Its protected store must persist the enrolled
public key, and the protected operator surface must display the canonical plan
and require a fresh verified WebAuthn assertion. The same protected execution
path must consume approval using Task020's existing durable journal and own
the adapter/session until confirmed cleanup. No caller-supplied identity or
boolean is accepted as evidence of that boundary.

Privileged registration and real enrollment require the separate explicit
operator action described in Task153; no installation command is provided or
invoked here. Windows and Linux need their own verified OS providers. Actual
service, macOS ownership/IPC, Windows, Linux and hardware tests are **NOT RUN**.

## Dependencies and verification

The experiment-only path dependency reuses the existing Task152 library and
its reviewed pinned dependencies. It is not a release manifest or production
dependency pin; upstream caveats remain in Task152's dependency review.
The read-only verifier directly pins the same `webauthn-rs = 0.6.2`, defaults
disabled; the `softpasskey` authenticator is a dev-dependency only and was
already reviewed in Task152. No dangerous credential/state features are used.
The macOS file operations also pin `rustix = 1.1.6` (MIT/Apache-2.0 options,
Rust >=1.65), defaults disabled with only `fs` and `std`. It was already present
through `tempfile`; no new registry package was introduced. The reviewed safe
rename and flock wrappers encapsulate upstream FFI; no project unsafe code is
added. The flagged Darwin rename has no unsafe overwrite fallback.
The macOS provider pins `nix = 0.31.3` (MIT, Rust >=1.69) with defaults disabled
and only `socket`, `user` and `fs` features. Its new dependencies are `memoffset 0.9.1`
(MIT), `cfg_aliases 0.2.2` (MIT) and `bitflags 2.13.2` (MIT OR Apache-2.0), with
the already resolved `libc` and `cfg-if`. Tests pin `tempfile = 3.24.0`
(MIT OR Apache-2.0). Its resolved dependencies, including non-host targets,
offer MIT/Apache-2.0 options; no license metadata was missing in the inspected
new package set. The exact versions are in Cargo.lock. The exact Git-pinned
`exacl` revision is MIT, Rust >=1.85, default features disabled. Its new
transitive package `scopeguard 1.2.0` is MIT OR Apache-2.0. The reviewed build
script copies packaged bindings; runtime bindgen and its extra toolchain are
disabled. This descriptor API is not in the published 0.13 documentation,
although the source manifest retains version 0.13.0; the exact Git revision is
therefore essential. Upstream native FFI contains unsafe code; this project
adds no unsafe code and does not claim a complete upstream audit. Provisioning
downloaded the exact Git source and missing `scopeguard`; no dependency code
was copied or relabeled.

From the repository root with the pinned toolchain:

```sh
cargo test --offline --locked --manifest-path experiments/task153-broker/Cargo.toml
cargo fmt --manifest-path experiments/task153-broker/Cargo.toml -- --check
cargo clippy --offline --locked --all-targets --manifest-path experiments/task153-broker/Cargo.toml -- -D warnings
```

Five offline tests exercise hostile JSON/framing inputs, bounded IDs/version,
effect restrictions, exact summary/digest binding and denial without a real
host provider. The initial test exposed Serde's acceptance of unknown fields
on a unit enum variant; the contract now uses an empty struct variant so that
the unknown-field policy is enforced. These are contract tests, not a service
boundary, hardware or protected-approval proof. Three macOS local-socket tests
and one compile-fail identity-construction test additionally passed. The local
socket tests require permission to create/connect their temporary Unix sockets;
they do not contact a network host or KNX interface.
The current complete run passed 36 library tests, six helper tests and two
compile-fail doctest on macOS arm64. Windows, Linux, installed-service,
production-enrollment and hardware validation remain **NOT RUN**.
