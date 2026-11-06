# Task153 portable broker input contract

This non-shipping Rust library defines the bounded input and configured-plan
policy for a future protected operator service. It contains no executable,
listener, protected store, passkey enrollment, approval
issuer or KNX adapter. `UnprovisionedBroker` always rejects a valid request with
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

This increment is source-level input validation only. It neither implements
nor tests an OS privilege boundary and cannot release a physical capability.
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
protected configuration ownership/ACL/IPC installation remain **NOT RUN**.
The facade remains unprovisioned and cannot dispatch.

Primary API evidence inspected on 2026-11-06:

- [Apple getpeereid manual](https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man3/getpeereid.3.html)
  documents effective credentials fixed at connect/listen time.
- The downloaded `nix 0.31.3` source `src/unistd.rs:3999` implements a safe
  `AsFd` wrapper, enabled only by the `socket` and `user` features on BSD targets.
  Its narrowly encapsulated upstream FFI is not a claim that the dependency
  contains no unsafe code; this project increment adds no unsafe block.

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
The macOS provider pins `nix = 0.31.3` (MIT, Rust >=1.69) with defaults disabled
and only `socket` and `user` features. Its new dependencies are `memoffset 0.9.1`
(MIT), `cfg_aliases 0.2.2` (MIT) and `bitflags 2.13.2` (MIT OR Apache-2.0), with
the already resolved `libc` and `cfg-if`. Tests pin `tempfile = 3.24.0`
(MIT OR Apache-2.0). Its resolved dependencies, including non-host targets,
offer MIT/Apache-2.0 options; no license metadata was missing in the inspected
new package set. The exact versions are in Cargo.lock. Only missing
`memoffset` needed downloading; no dependency code was copied or relabeled.

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
