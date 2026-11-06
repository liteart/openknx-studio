# Task153 portable broker input contract

This non-shipping Rust library defines the bounded input and configured-plan
policy for a future protected operator service. It contains no executable,
listener, peer-identity provider, protected store, passkey enrollment, approval
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
dependency pin. No new registry package or feature is introduced; upstream
license and security caveats remain in Task152's dependency review.

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
boundary, hardware or protected-approval proof.
