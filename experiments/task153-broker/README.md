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
unprovisioned broker facade and diagnostic cannot issue a device capability.

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
no arbitrary path/byte import API or public raw writer. The module
is only connected to the configured-operator-peer-authenticated handler described below; no
listener, operator UI or diagnostic-helper invocation enables it. No
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

## Configured operator Unix connection handler

`operator_connection::handle_operator_connection` owns one Unix stream and
requires actual root/wheel service credentials plus the configured operator
UID/GID from kernel-observed peer credentials before reading any byte. The
service obtains this policy from its protected manifest; the agent UID is
explicitly denied. Wire data cannot provide IDs,
select a target/property/value, import a stored key or request a device action.
The service supplies one immutable configured plan. These checks do not prove
the process was independently installed or that the configured operator client represents a
human; the missing operator client must perform explicit OS-authenticated
initiation. The handler itself provides no HTTP enrollment endpoint or listener.

The framed JSON protocol is version 1 with a four-byte big-endian body length,
32 KiB request limit, 64 KiB reply limit and a 120-second deadline for socket
reads and writes; cryptographic verification and filesystem synchronization are
not bounded by that deadline. Duplicate keys and unknown commands/fields are
rejected. Exactly one
enrollment or assertion ceremony is allowed per connection, with private
server-held challenge state. Enrollment uses required-UV WebAuthn registration
and the shared exact RP/origin policy before private stage/activate calls.
Assertion uses only the fixed descriptor-validated root credential and sends
the same service-owned canonical plan summary/digest that it verifies.

All replies and assertion observations remain `physical_authorization:false`.
Successful registration reports activation or committed-cleanup-uncertain
accurately when its reply is delivered. If the final reply is lost, enrollment
may already be committed; a future client must inspect protected state and must
not retry blindly. This handler has no grant or dispatcher and cannot contact KNX.
The localhost RP does not prove phone-only custody or make a browser page an
independent operator channel. A same-user HTTP bootstrap must not be used to
pin an agent-chosen key; the protected operator client/UI is still needed.

Seven tests use actual local Unix sockets and synthetic registration credentials
to establish non-root process/peer denial before reading a ceremony, bounded
framing, rejected caller identity/target fields, required UV, genuine synthetic
registration and wrong-origin/challenge/expiry rejection. They do not impersonate
root, call the protected store, load real credentials or claim a complete
privileged end-to-end test. The existing diagnostic binary still runs only its
peer diagnostic; no executable currently starts this enrollment handler.

## Source-only fixed operator service listener

`task153-operator-service` now provides a separate explicit macOS entry point.
It accepts only `--operator-service-only` and requires actual root/wheel
credentials. Startup from the development checkout is refused: the executable
path must be `/Library/PrivilegedHelperTools/org.openknxstudio.operator`.
No elevated launch or installation has been performed.

Startup requires a descriptor-validated root-owned `service-manifest.json` in
the fixed store, with schema 1, `executable_sha256` (32-byte JSON array),
`executable_cdhash` (40 hexadecimal characters), `plan_digest` (32-byte
array), exact `gateway` (canonical IPv4 address and nonzero port), and
`operator_uid`, `operator_gid`, `agent_uid` numeric OS policy, plus an
`operator_acl_uuid` native principal pin (canonical lowercase braced UUID,
nonzero). Operator and agent
UIDs must be distinct and nonzero; no identity is accepted from IPC. The
selected operator account must resolve to that UID and primary GID. No host
account IDs or deployment manifest are committed.
Native `SecCode::for_self` dynamic validation must match the explicit
CDHash requirement; this binds the running image rather than trusting a
pathname. A bounded descriptor-read SHA256 of the protected installed file
must independently match the manifest. Requirements cannot be supplied as
arbitrary text, and native validation uses no-network flags.

These checks establish integrity **under the protected provisioning
assumption**, not publisher provenance or human approval. A root-owned
self-referential manifest is not a signature. The operator must independently
verify the reviewed artifact and provision its pins through a trusted path;
this source creates no manifest and offers no way to enable service from an
agent-writable artifact. No resistance to malicious root installation is
claimed. The protected operator client/UI remains missing.

The root-owned `write-plan.json` is loaded from the fixed store by descriptor.
It must match the pinned canonical digest, exact protected manifest gateway, bus path
`knxnetip-udp`, target `1.1.231`, object 0/PID 14/element 1 and exact `00` to `04`
candidate. Actual deployment must privately provision the gateway Simon
approved for the selected bench; no actual gateway/configuration is committed.
Static policy tests use TEST-NET addresses only. The plan must be unexpired
and contain the reviewed descriptor-0/`0705`
evidence fingerprint using the existing Task020 evidence tuple. This is a
profile-correlation fingerprint; it does not uniquely authenticate hardware.
The real executor must still re-read descriptor/current value immediately
before writing. No such device read or write occurs in this binary.

Only after every startup check passes can it bind the fixed
`/private/var/db/org.openknxstudio.operator/operator.sock` under the protected
existing parent. This directory must already be root-owned mode 0711 with no
allow ACL: non-root users may search it but cannot list or modify it. Root
configuration, plan, manifest and reader lock must be mode 0600 with no allow
ACL. Provisioning must prepare this policy independently; startup changes no
account or directory permissions.

The root-owned socket has mode 0600 and exactly one non-inheriting WRITE_DATA
ACE. The setter receives the decimal operator UID, parsed directly by pinned
exacl and converted by Apple's mbr_uid_to_uuid; usernames and aliases are never
used for ACL creation. Native-ID ACL readback must match the separately
protected operator_acl_uuid pin exactly, including the WRITE-only permission
and empty inheritance flags. A UID-to-UUID mismatch closes the listener before
admission. Provisioning must independently establish that this UUID belongs to
the selected operator, and protect that account/directory-service identity. No group grant or ACL,
owner or deletion authority is delegated. ACL installation uses exacl's safe
no-follow path API under the previously validated root-only writable parent;
readback must match the exact ACE and inode/type/owner/mode must remain stable.
A symlink or ordinary file fails without modifying its target. Kernel UID/GID
checking on the owned connection independently precedes every request and
rejects the agent principal. An ACL failure stops admission and retains the
endpoint for reviewed cleanup. This is not a general path-writing API.

Apple's Unix socket implementation authorizes connect using WRITE_DATA on the
socket vnode ([XNU uipc_usrreq.c](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/kern/uipc_usrreq.c)).
An actual temporary socket test under the normal test UID proves ACL storage,
exact native UUID readback, rejection of a mismatched UUID pin and socket
compatibility. Because that test UID also owns the
fixture, it does **not** prove another principal's effective access or denial.
Dedicated-group authority was avoided: the pinned nix version does not expose
getgrouplist on macOS, and primary-GID comparison cannot prove supplementary
group exclusion. The UID ACL grants no group access.

A separately provisioned operator login and normal Safari can avoid custom
browser entitlements. That principal must not be available to the agent via
login/session/process control or privilege escalation. The operator bridge/UI
is still missing; no browser ceremony through this new principal has run. No
account creation, root execution, installation or real key access occurred.
Real distinct-user socket acceptance/agent denial, protected enrollment and
launch-environment isolation remain **NOT RUN**. This source does not establish
that the current host has those identities or isolation.

Existing endpoints are never reused or unlinked. The
single-threaded listener admits at most eight sessions during a 15-minute
accept window. A handler admitted before the window closes can finish later;
its frame I/O uses the 120-second connection deadline, while synchronous
filesystem/native operations, including fsync, have no hard completion bound.
An accepted durable commit is not forcibly interrupted to meet a wall-clock
shutdown claim. Shutdown retains the endpoint for
independent operator cleanup. All handlers remain non-authorizing; no KNX
adapter is connected. No HTTP server or unprivileged browser bridge is enabled.

Main-image validation does not establish loaded-library or launch-environment
integrity. Provisioning review must independently cover hardened runtime/library
validation where applicable, trusted launch configuration and inherited
environment, including DYLD injection paths. A hash match does not make an
agent-controlled elevated launch safe. No such launch hardening or installed
service behavior has been qualified by this increment.

Source tests establish actual unprivileged startup denial, strict reference-plan
and manifest validation, bounded local session denial, and native dynamic-code
matching against the current test image's public CDHash. The latter invokes
the fixed vendor `codesign --display` tool only on its own test executable;
it does not sign code, inspect identities/keychains or write a trusted pin.
Positive installed-root service startup, protected UI/client enrollment and
physical execution remain **NOT RUN**. No test artifact, manifest or private
bench configuration is committed.

Primary code-identity references:

- [Apple exact code requirements](https://developer.apple.com/documentation/technotes/tn3127-inside-code-signing-requirements)
- [Safe Security.framework API](https://docs.rs/security-framework/3.7.0/security_framework/os/macos/code_signing/struct.SecCode.html)

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

## Separate-operator Safari bridge source

`task153-operator-browser` accepts only `--enroll` or `--assert`. It must run
under the independently provisioned operator login/session, using an artifact
reviewed and installed outside the agent's writable boundary. A development
checkout is not a trusted operator surface.

Protected enrollment additionally requires a freshly isolated Safari profile in
that operator account, with no pre-existing localhost:15300 Service Worker,
origin/site storage or extensions/content blockers. A previously installed
Service Worker can replace the page or JavaScript before these header checks
and observe/intercept the bootstrap and authenticated requests. Extensions or
browser/session control can similarly bypass the intended display and secret
isolation. Private files, exact Origin and the passkey prompt do not mitigate
that compromised-browser case. The bridge neither clears nor proves browser
profile state; a new profile and its isolation must be independently provisioned
and qualified before treating enrollment as protected. This prerequisite is
**unverified / NOT RUN**; no such browser profile was created or inspected.
 Installation, artifact integrity,
separate-principal login/session/process isolation (including accessibility,
remote control and privilege escalation), and actual browser behavior remain
**NOT RUN**. This increment installs nothing and does not establish that the
current host has an isolated operator principal. Never launch an agent-writable
bridge artifact as the approving operator.

Before HTTP binding or bootstrap creation, the client connects the fixed Unix
socket with a two-second nonblocking connect deadline, verifies kernel-observed root/wheel server identity, sends the selected
begin request and requires a valid non-authorizing service reply. The service
independently checks the client against its protected operator UID/GID policy.
No caller can choose an endpoint, plan, target, property or value. Successful
socket identity assumes the existing protected service provisioning; it does
not establish publisher trust or authorize a device operation.

The bridge binds only `127.0.0.1:15300`, generates a 256-bit secret using the
safe pinned Security.framework `SecRandom` wrapper, and creates an exclusive
0600 bootstrap file in an explicitly 0700 owned directory. Descriptor ACL
checks reject inherited allow ACEs before secret contents are written. Safari
is opened with only the file path in arguments. The secret travels in the
navigation fragment, is immediately removed with history.replaceState, and is
then sent only in a custom same-origin request header. It is never placed in
command arguments, environment, server logs or query/path data. The bootstrap navigation uses no-referrer; the served page, HTTP response and
POST fetches use same-origin so Safari preserves the exact Origin rather than
Origin:null. Same-origin referrers contain only the fragment-free localhost
URL. The bootstrap is removed after the first authenticated request.
Normal cleanup removes only matching owned fixture inodes and the resulting
empty directory; substitution/unknown contents retain quarantine. A process
crash may leave a private bootstrap file for operator cleanup.

Unauthenticated GETs expose only static disabled UI and JavaScript. Requests
must use the exact localhost Host; POSTs additionally require the exact origin,
JSON content type and session secret. Foreign/null/missing POST origin,
wrong/missing secret, duplicate headers, transfer encoding, oversized data and
unexpected routes fail closed. No CORS endpoint or cookies are used. Header
bytes are bounded to 4096, request bodies to 32768, replies to 65536; the bridge
admits at most 64 connections within 110 seconds, with two-second HTTP request
I/O deadlines. I/O is nonblocking with absolute deadlines: actual Darwin
closed-peer fixtures exposed EINVAL when resetting SO_RCVTIMEO after a reply,
so this bridge does not rely on that socket-option behavior. The Safari opener
has a two-second child-process admission wait; OS scheduling and process
cleanup are not hard realtime guarantees.

The authenticated page renders the service's canonical summary and digest
using textContent, requires acknowledgement and a fresh explicit passkey click,
and invokes standard navigator.credentials.create/get. It submits exactly one
finish command on the original service connection. The service verifies the
WebAuthn ceremony and handles protected enrollment; every response remains
physical_authorization:false. Failed/uncertain enrollment responses require
protected operator inspection rather than a retry claim. No KNX adapter or
bearer grant is added. The existing service's ledger/executor integration is
still missing.

Eight new tests use actual temporary loopback/Unix sockets, private filesystem
fixtures and synthetic public ceremony data. They cover HTTP/CSRF denials,
framing/deadlines, saturated Unix-connect backlog, static-page secrecy, wrong
server UID, service framing,
private bootstrap permissions and replacement-preserving cleanup. Two additional simulated DOM/WebAuthn tests exercise both complete browser
flows, exact same-origin fetch policy, immediate fragment removal and fresh
operator click before the provider call. No test opens Safari or accesses a
real user credential. Browser UX, complete protected
enrollment/assertion, distinct-user denial, Windows/Linux and hardware remain
**NOT RUN**. Thirty focused framing repetitions passed after the Darwin I/O
fix. `tempfile = 3.24.0` is now a macOS runtime dependency for private bootstrap
creation; its version/license/dependency review is unchanged and no new package
was introduced.

Primary RNG reference: [Apple SecRandomCopyBytes](https://developer.apple.com/documentation/security/secrandomcopybytes(_:_:_:)).

## One-shot protected assertion/counter ledger

The operator connection now calls finish_protected: standard WebAuthn signature
and UV verification still run first, then successful completion requires a
fixed verified-assertion.json record in the descriptor-validated root store.
The private record contains the exact plan digest, original enrollment hash,
assertion-response hash, issuer, verification/expiry times and updated public
Passkey state (including authenticator counter/backup flags). It contains no
PIN, authenticator private key or reusable bearer grant. The caller cannot
supply a destination or deserialize a verified record.

The writer requires actual root/wheel process credentials, uses the existing
exclusive enrollment lock, rejects an activation fence, and re-reads the
root-owned enrollment by descriptor. Its hash must still match the state used
for verification. A fixed descriptor-relative O_EXCL/O_NOFOLLOW create is
mode 0600 with descriptor ownership/type/link/mode/ACL checks. Empty-file and
parent fsync first establish a reservation; record data, file fsync and parent
fsync must then finish before the success observation. Errors after creation
never remove the record; earlier validation or open failures may leave none. A
failure before the initial directory sync may lose its name after a
power failure, but no successful observation or device capability has been
issued at that stage. Fault tests observe actual filesystem calls and injected
process-stage failures; they are not power-loss qualification.

Any existing record, including an empty/partial file, symlink or unreadable
entry, makes later protected loading unavailable. O_EXCL independently prevents
a concurrent verifier from replacing/overwriting it. Therefore the original
config cannot regain counter authority after a successful record, even across
restart. This is intentionally a single protected assertion for the single
prepared write/restore candidate; no automatic reset, multi-operation counter
migration, record replay or resume API exists. Do not delete a record simply
to retry; recovery requires separate protected operator review. The updated
counter remains in this immutable record rather than overwriting config.json.
The public read-only verifier finish method still returns only a non-authorizing
observation; only the protected connection uses this persistence path.

Six new tests cover private ordered persistence, every injected write/sync
stage remaining quarantined, no overwrite, symlink refusal, renamed-parent
descriptor binding and unprivileged runtime denial. Genuine software-generated
WebAuthn assertions additionally persist the updated synthetic counter into an
actual owned filesystem fixture before returning an observation; a second
commit fails and persistence failure consumes the challenge. These fixtures
establish no root-owned snapshot or protected-principal claim. Installed-root
commit/restart behavior and real operator keys remain **NOT RUN**.

This ledger is not Task020's execution-intent/route journal. The dispatcher
still has no production approval issuer or registered broker adapter, and the
service executes no device operation or future replay of this stored record.
The exact-source integration below now consumes this durable verification
result into the existing guard and typed adapter. The default broker still has
no qualified execution entrypoint; no public guard visibility or physical gate
was widened.

## Exact-source guard and typed-adapter integration

run_execution_check.py reuses Task020's verified Git-blob exporter to build a
temporary, non-shipping crate from exact broker/guard source plus fork revision
4d296afb0906d17b6724b56c912f2e9e3f6680a5 and exacl revision
18275aa1a82aae9a786b108b0db0992a4d06e6e2. No fork was published and no local
fork path or temporary lockfile enters a committed release manifest. The
runner preserves both input locks' cached resolutions, preferring the broker's
existing packages where names overlap, then resolves and checks only the
temporary graph offline. knust features must be exactly dpt; Secure stays off.
It exports Git blobs rather than copying dependency worktree/build files.

The temporary crate registers guard_execution.rs as a private child of an
exact Task020 guard copy. That child alone can construct the guard's private
HumanApproval from a consuming PersistedAssertion witness. Only successful
standard WebAuthn verification plus protected ledger persistence emits that
witness in non-test code. It has no public constructor, serializer, Clone or
observation-to-witness conversion; the default UI drops it and returns only its
non-authorizing observation. No grant constructor was added to Task020 or to
an MCP/IPC operation. Task152's ordinary guard copy is left unmodified.

The private path parses the canonical displayed plan into the same exact guard
model and compares its recomputed digest to the configured digest and witness.
It hard-requires target 1.1.231, the reviewed descriptor profile, object0/PID14/
element1 and exactly 00 to04, and rechecks expiry before opening the actual
KnustAdapter. The adapter uses only the pinned fork's public typed API. The
existing dispatcher owns desired/readback/restore/readback and durable route,
consumption, intent, uncertain-state and cleanup evidence. Its existing
prewrite descriptor/current-property reads remain in force. A profile digest
is correlation evidence, not unique hardware authentication.

Production execution requires an uninhabited QualifiedExecution context:
there is no constructor until independently reviewed installation, principal,
browser/profile and operator qualification are implemented. The default crate
never registers this execution module. There is no executable flag or claimed
manifest boolean to enable it. Its dormant root wrapper additionally requires
root/wheel and a descriptor-checked fixed execution directory under the
protected store, root-owned mode0700 with no allow ACL. That descriptor stays
alive through execution. The one-shot wrapper now scans that directory through
the checked descriptor and rejects every existing entry, including hidden
files, subdirectories and symlinks, without reading or deleting them. Existing
or copied evidence cannot qualify a fresh attempt. Ordinary temporary-directory
tests cover empty/nonempty scans and descriptor binding after rename. This is
not automatic recovery or a cleanup procedure. It confines the existing
path-based journal under the
assumption that privileged actors do not maliciously replace the root-protected
ancestry. Root substitution and installed-root confinement were **NOT RUN**;
this source preparation establishes neither qualified host nor hardware access.

The combined offline run passed108 Rust tests, including the actual typed
adapter/owned fork loopback peer driven by a genuine synthetic WebAuthn
assertion and actual filesystem counter record: exactly two writes (04,00),
four property reads, four descriptor reads and separately confirmed device and
gateway cleanup. A changed plan or expired witness emitted zero UDP packets.
The positive test deliberately uses an ordinary owned journal fixture through
a private test path; it never constructs QualifiedExecution or pretends to
prove protected-root storage. Existing cancellation/unknown-outcome tests also
passed. Combined source check, Clippy and Task020 artifact validation passed.

Run with the pinned toolchain, a clean local fork checkout at the exact revision
and the existing reviewed exacl checkout (source only):

```sh
python3 -B experiments/task153-broker/run_execution_check.py --fork "$PINNED_KNUST_CHECKOUT" --exacl "$PINNED_EXACL_CHECKOUT"
```

The runner reports its temporary artifact directory and exact input hashes.
Normal broker verification remains67 library tests,6 helper tests and2
compile-fail doctests; default builds gain no adapter or network authority.
Actual protected installation, profile qualification, service-to-executor
activation and the real write remain **NOT RUN**. Their missing qualification
cannot be replaced by this passing synthetic integration.

## Dependencies and verification

The bridge directly pins already-resolved `socket2 = 0.6.5`, default features
disabled (MIT OR Apache-2.0, MSRV 1.70). Only existing libc/windows-sys packages
are used; the lockfile adds only the direct dependency edge. Reviewed upstream
Socket::new sets close-on-exec and Apple NOSIGPIPE; connect_timeout switches to
nonblocking connect and polls against elapsed time, including interrupted poll
handling. The bridge additionally checks deadline and connected peer address,
then authenticates kernel UID/GID. OwnedFd conversions are safe upstream APIs;
no project unsafe code or FFI is added. Upstream contains unsafe code; this is a
focused API/source review, not a full audit. An actual small-backlog Unix socket
fixture fills the queue and verifies bounded timeout or immediate refusal
instead of an indefinite connect. This macOS run observed immediate refusal
after the first queued client; it did not exercise a delayed OS poll timeout.

The experiment-only path dependency reuses the existing Task152 library and
its reviewed pinned dependencies. It is not a release manifest or production
dependency pin; upstream caveats remain in Task152's dependency review.
The read-only verifier directly pins the same `webauthn-rs = 0.6.2`, defaults
disabled; the `softpasskey` authenticator is a dev-dependency only and was
already reviewed in Task152. No dangerous credential/state features are used.
Native running-image validation pins `security-framework = 3.7.0`
(MIT OR Apache-2.0, Rust >=1.85), defaults disabled. Its new packages
`core-foundation 0.10.1` and `security-framework-sys 2.17.0` retain their
MIT/Apache-2.0 notices; no build script or keychain feature is invoked by this
code. Only safe self-code/requirement validation APIs are used. Upstream FFI
contains unsafe code; the project adds none. The already reviewed `sha2 0.11.0`
is directly pinned for executable/plan evidence hashing.
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
node --test experiments/task153-broker/browser_test.mjs
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
The current complete run passed 67 library tests, six helper tests and two
compile-fail doctest on macOS arm64. Windows, Linux, installed-service,
production-enrollment and hardware validation remain **NOT RUN**.
