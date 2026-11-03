# Task-012 scalar-state artifact proof

This Rust library stores one known SDK string value as a bounded, immutable
JSON artifact. Its fixture CLI can import a completed private SDK capture and
export a restore projection. It does not implement the production host bridge, accepted project
changesets, Git transactions, an opaque vendor format, or DCA restart/reload.
The caller must obtain the value and qualified identity through the real SDK
path. Creating an artifact receipt does not acknowledge accepted configuration.

`State::capture` binds the value to a device UUID, application identity, product
archive hash, original DCA hash/version, parameter identity/name, and originating
revision. `State::decode` verifies those fields against an independently supplied
identity and verifies the content hash. The version-1 encoding uses fixed struct
field order and compact UTF-8 JSON. Duplicate/unknown keys, noncanonical encoding,
unsupported versions, invalid fields, oversized input and integrity mismatches
are rejected. SHA-256 detects corruption; it does not authenticate an untrusted
sender. The bridge must supply authorization separately.

`Store` uses an existing, dedicated caller-controlled directory. Artifact names
are SHA-256 hashes of the complete canonical envelope. On Unix, save flushes a
same-directory temporary file before no-clobber publication, then flushes the
directory before returning an artifact receipt. An existing valid artifact is
an exact no-op; existing corrupt evidence is never overwritten. A flush failure
is returned even if a file was already published. Staging cleanup is best effort
through `tempfile`; interruption may leave an unreferenced staging file. This
PoC does not provide a current-state pointer or multi-file transaction recovery.

Saving on non-Unix platforms returns `dca_state.unsupported_durability` before
writing; Windows filesystem durability is NOT RUN. Loading/encoding is portable.
The existing directory must not be concurrently replaced by an attacker; static
symlinks are rejected, but this is not a capability-based filesystem sandbox.
The caller must run blocking operations off any async executor. Each operation
is bounded to a 16-KiB envelope, 1-KiB value and 256-byte identity fields. It is
one synchronous artifact operation, with no background task to cancel.

Direct dependencies were checked against downloaded registry manifests:

| Dependency | Exact version | License | Features used |
|---|---|---|---|
| serde | 1.0.229 | MIT OR Apache-2.0 | std, derive |
| serde_json | 1.0.154 | MIT OR Apache-2.0 | default std; recursion bound enabled |
| sha2 | 0.11.0 | MIT OR Apache-2.0 | default features disabled |
| tempfile | 3.24.0 | MIT OR Apache-2.0 | default features |

`Cargo.lock` pins transitive dependencies. Their manifest licenses offer MIT or
Apache-2.0 alternatives; `unicode-ident` additionally requires Unicode-3.0.
Original dependency license files remain in the dependency distributions. The
maximum declared transitive MSRV is 1.87 (target-specific WASI dependency), below
the repository's pinned Rust 1.99.1. Only macOS execution is currently verified.

Tests use synthetic owned identities and strings. They cover canonical encode/
decode, all seven identity mismatches, duplicate/unknown/corrupt/noncanonical/
oversized input, invalid capture, reopen/no-op, missing/corrupt artifact, static
symlink rejection and concurrent identical publication. These are file/codec
tests, not Windows DCA or hardware tests. No actual product configuration or
proprietary input is included.

## Offline fixture integration

`dca-state-poc import-fixture <candidate> <launcher-stdout> <collected-receipt>
<expected-fixture> <existing-store>` requires separately supplied expectations.
Those expectations pin all candidate identities, the exact host executable and
process ID, the three evidence-file hashes, and the expected test-value hash.
Candidate success flags alone never suffice: the independently collected task
receipt must be terminal/exit0 with no owned process, and the independently
collected launcher output must confirm the same child PID exited normally at0.
Strict JSON visitors reject unknown/duplicate fields in candidate, expectations
and completion. This is a trusted offline test coordinator's input contract,
not an authenticated production peer or process attestation.

The artifact additionally binds the measured SDK project/installation/device
IDs and SDK/ObjectModel hashes. Its source label explicitly identifies the
editor UUID and revision as synthetic offline-fixture values. Ephemeral PID
and completion receipts remain external test evidence so a repeated identical
configuration retains the same artifact hash.

`dca-state-poc export-fixture <store> <artifact-hash> <expected-fixture>
<new-projection-path>` loads and verifies the artifact in a fresh process, then
exports compact JSON for a separately pinned Windows host. The projection is
derived scratch data; the hash-addressed artifact remains authoritative for this
PoC. Export rechecks the expected value hash as well as the identity. Its file
is created without overwrite with Unix mode0600 and flushed; output creation is
unsupported on non-Unix platforms pending a tested private ACL path. No separate durable
projection receipt is claimed. Both CLI commands print only hashes, sizes,
technical outcomes and `accepted_configuration=false`; they do not print the
captured value. Windows restore execution and production Git acceptance remain
separate work.
