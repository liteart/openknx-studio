# Task153 local development package — review only

The prepared package contains two ad-hoc hardened executables and a disabled
launchd job. It is unsigned as an installer, not notarized, and establishes no
publisher identity or human approval. Ad-hoc signing uses no user signing key.
No private configuration, account, passkey or device access is included.
The product's supported architectures are the intersection reported by `lipo`
for every actual packaged executable, including the installer preflight. The
current local build is arm64-only; an x86_64 claim is not inferred from Rust
source portability or productbuild's synthesized defaults.
The product contains two leaf-root components: helpers install into the existing
`/Library/PrivilegedHelperTools`, and the disabled job installs into the existing
`/Library/LaunchDaemons`. No ancestor directory is part of either payload.
The component-root modes are fixed at `01755` and `0755`, respectively, and the
bundled read-only preflight refuses a host with different root/wheel ownership,
mode, unsafe ACLs, symlink parents or existing component destinations. Each
component has `overwrite-permissions=false`; this flag alone is not treated as
proof of actual Installer behavior. Installed preservation is **NOT RUN**.

Source quarantine and unknown xattrs are rejected before copying. Existing
source and staging xattrs are never deleted. Metadata-free generated cpio
archives omit staging AppleDouble companions; actual expanded payload, BOM and
script entries are verified and recorded in `inventory.json`. This is an
explicit packaging policy, not removal of security attributes from an input.
The service still drops its assertion witness; real execution is unavailable.

1. Independently review the source, inventory.json, package hash, signatures,
   both component payloads/BOMs and their preinstall/preflight scripts. Building and hashing by the agent is
   not approval. This artifact is ready for review, not installation acceptance.
2. Use the existing macOS account, as selected by the owner on 2026-11-07.
   Set the protected manifest's operator and agent UID to that same nonzero UID.
   This permits assertions against an already enrolled key but does not isolate
   the user from Codex or other code in that account. First enrollment still
   requires an actual root/wheel bootstrap connection after independent OS
   authentication. That bootstrap client is not implemented; an ordinary
   browser connection cannot enroll or replace a key. No password or PIN is
   requested by the preparer.
3. In the existing account prepare a fresh Safari profile, with no existing
   localhost:15300 Service Worker/site storage, extensions or agent-controlled
   automation. Isolation and profile state require independent qualification;
   neither this package nor its signature proves them.
4. Before any installation, review actual parent-directory ownership and ACLs,
   launch environment/library validation, artifact provenance and existing
   fixed destinations. Existing files cause the read-only preinstall to refuse;
   each component checks its own leaves so sequential component preflight does
   not reject files just installed by the other component. There is no
   overwrite, deletion or automatic migration procedure. Product installation
   is not transactional across components: interruption can leave a partial
   install, requiring independent inspection rather than blind retry.
5. Only after explicit owner approval, the human may open the reviewed local
   pkg in Apple's Installer and perform its OS authentication. No root command
   or installation is run by the preparer. If macOS rejects the unsigned local
   package, obtain an independently reviewed signing solution; do not bypass
   platform checks. Installation is NOT RUN.
6. Private root provisioning remains separate: the existing service requires
   a root-owned 0711 store with no allow ACL, protected 0600 manifest/plan/lock,
   selected operator/agent UIDs (equal for the chosen same-account mode),
   operator primary GID/native UUID,
   reviewed executable SHA256/CDHash, approved private gateway and exact plan
   digest/expiry. Do not commit actual identities, gateway, plan or keys. The
   package intentionally supplies none of these values and does not launch.
7. Qualify installed ownership/ACL, kernel peer denial/acceptance and the actual
   Safari enrollment/assertion before activation. QualifiedExecution still has
   no constructor: a reviewed service activation change is required before a
   real write. Do not remove a one-shot assertion record merely to retry.

All root/account/profile/service/enrollment/physical actions above are NOT RUN.
A future live test remains limited to the previously approved device 1.1.231,
with its immutable reviewed plan, fresh identity/current reads and independent
write/readback/restore/readback plus confirmed cleanup. This package cannot
perform that test. Windows and Linux packaging are NOT RUN.

Preparation (normal user, pinned toolchain, no installation):

```sh
cargo build --offline --locked --manifest-path experiments/task153-broker/Cargo.toml --bin task153-operator-service --bin task153-operator-browser --bin task153-install-preflight
python3 -B experiments/task153-broker/provisioning/prepare_package.py --bin-dir experiments/task153-broker/target/debug --output "$NEW_PRIVATE_OUTPUT_DIRECTORY"
```

For a reviewed exact-export service build, provide both `--service-bin` and
`--service-evidence`. The preparer reads the original binary directly, checks
its security attributes before copying, and verifies its original path, hash,
size and disabled host qualification against the export's `result.json`.
Evidence is read once with bounded no-follow access and duplicate-key rejection.
The preparer requires the pinned dependency revisions, the successful expected
build/test command sequence and toolchain, and rechecks every actual compiled
file, final lock and dependency blob. Dependency Git trees must match tree IDs
independently pinned from the reviewed commit objects. No symlink ancestor or
unlisted export file is accepted. It also compares the artifact with the
exported build output.
That verified original service hash is carried into packaging: the preparer
checks both the original input and the actual copied bytes against it before
signing. It repeats the input security-attribute check after the copy. A change
between evidence validation and copying aborts package creation.
The inventory records the evidence hash and original service hash separately
from the ad-hoc-signed package copy. Do not copy inputs into an unchecked
intermediate directory or remove security attributes to make them acceptable.
An export build still does not enable the unconstructible execution gate.
These checks establish snapshot consistency, not cryptographic execution
attestation: report/log contents and the build itself still require independent
review. An agent-owned report is never hardware approval or provenance signed
by an independent authority.

Apple's local pkgbuild(1) documents install-location and recommended archive
ownership without changing staged ownership. bsdtar(1) documents metadata-free
archive options; mkbom(8) documents reconstruction from verified BOM listings.
The generated flat component is independently decoded with pkgutil and then
embedded with productbuild(1). launchd.plist(5) documents Disabled/RunAtLoad.
Actual installed access and parent preservation still require qualified testing.
