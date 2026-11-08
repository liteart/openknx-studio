# Task152 development passkey verifier

This standalone Rust experiment verifies a standard WebAuthn passkey assertion
against an exact immutable Task020 plan. An explicitly invoked temporary browser
runner exercises only a synthetic plan. It has no MCP endpoint, device
connection, physical approval issuer or credential import.
A successful assertion produces a `DevelopmentReceipt` with
`physical_authorization: false`. It cannot be converted into Task020's private
`HumanApproval`; a compile-fail test checks that boundary.

The test-only guard integration additionally exercises a genuine synthetic
WebAuthn signature through the existing Task020 dispatcher and an in-memory
simulated session. An explicitly `cfg(test)` fixture constructor supplies the
dispatcher approval only in those tests; no normal library or browser binary
can call it. The tests cover the exact write/readback/restore/readback/cleanup
sequence, changed plans, expiry, durable replay rejection and an uncertain
write without an automatic restore. A development receipt by itself still
causes zero session calls. This is offline integration evidence, not protected
enrollment, a physical grant adapter or device execution.

## Implemented behavior

- The same validated `PlanInput` supplies the immutable plan digest and the exact
  operator summary, including target, identity, values, purpose, recovery and
  expiry. Serialization and assertion responses are limited to 32 KiB.
- The pinned WebAuthn server library generates the unpredictable challenge. Its
  private in-memory state binds that challenge to one plan, one issuer and an
  expiry of at most 120 seconds or the plan expiry, whichever comes first.
- Registration and authentication require user verification. The PIN remains
  within the authenticator; no PIN field or PIN collection API exists here.
- Assertions verify signature, credential, challenge, exact origin
  `http://localhost:15200`, RP `localhost` and user verification. Subdomains and
  alternative ports are disabled. Every submission consumes its pending state.
- Success creates a new challenge-ID receipt and synchronizes the file and
  containing directory before returning. Existing receipt paths are rejected.
  Cancellation or restart discards pending ceremonies. Expiration, rollback of
  supplied UTC time and a monotonic timeout are rejected.
- Opening the verifier first checks the actual journal's directory-sync barrier
  without creating a probe, credential or consumption record. An unavailable
  barrier fails before enrollment or assertion can begin. Every successful
  assertion still requires its own file and directory synchronization; an
  earlier preflight never substitutes for those barriers.

## Platform direction and trust boundary

The owner selected the integrated macOS passkey vault and requires a design
usable on Windows and Linux. This verifier uses browser-standard WebAuthn and
leaves authenticator attachment unrestricted: a future browser UI can offer
the macOS vault, Windows platform credentials or supported Linux authenticators
and a phone where the browser supports it. It does not invoke a macOS-only
keychain API. Native browser, platform vault and phone compatibility are
**NOT RUN** until an operator completes the browser test. A synchronized credential
does not prove that its private key is held only on the phone.

Enrollment is explicitly **development enrollment**, stored only in process
memory. It proves possession of a new credential, not the operator's identity.
The journal is not independently protected from code running as its owner.
There is no tested operator/MCP separation, protected trust-anchor provisioning
or hardware-capability handoff. Those missing pieces remain fail-closed.
In particular, an MCP principal can instantiate this development library but
can obtain only development observations, never physical grants.

The browser runner displays the exact synthetic summary on the desktop before
assertion. Ordinary authenticator PIN/biometric dialogs must not be described
as a trusted display of the KNX transaction. Unrestricted code under the same
approving OS principal can modify enrollment, executable code and desktop
display. This experiment does not claim protection against that principal.

The directory synchronization path was tested only on macOS arm64. The same
concrete file-and-directory barriers have a Linux implementation. The library
tests passed in a Linux arm64 container on tmpfs; native-host filesystem and
power-loss durability remain unqualified. Windows
and other platforms have no reviewed parent-directory persistence operation
here: opening a verifier explicitly returns `UnsupportedJournalPlatform`
before any ceremony can start. This is a fail-closed limitation, not Windows
support. A failed preflight on the implemented platforms returns
`JournalDurabilityUnavailable`. A later write or barrier failure returns `Io`
and releases no receipt; a newly created consumption file is preserved and
cannot be silently overwritten or retried. No successful fsync call proves
power-loss behavior for every filesystem or device. Callers must provide a
trusted UTC clock and an independently protected service before any future
physical integration. See [the preflight evidence](TASK152-JOURNAL-PREFLIGHT.md).

## Temporary browser runner

After independent review, an operator can explicitly start the development test
from the repository root with an existing local evidence directory:

```sh
mkdir -p private/evidence/task152-browser-demo
cargo run --offline --locked --manifest-path experiments/task152-passkey/Cargo.toml \
  --bin task152-browser-demo -- --development-only \
  --journal-dir private/evidence/task152-browser-demo
```

Then open `http://localhost:15200` manually. No passkey ceremony starts on page
load. The operator chooses **Create temporary test passkey**, reviews the exact
synthetic plan and chooses **Verify synthetic plan with passkey**. The browser
selects the available provider; no PIN is entered into this page. Creating the
test passkey can leave a `localhost` credential in that provider after the test;
the operator can remove it there afterward. The server's public enrollment
exists only in memory and is discarded when the server stops. This runner is
not the reusable protected enrollment required for actual device approval.

The server binds only `127.0.0.1:15200` and accepts only the exact Host
`localhost:15200`. Mutating requests require the exact Origin, same-origin Fetch
Metadata, JSON content type and a random per-run CSRF token. The session token
endpoint rejects cross-site fetches. These checks prevent ordinary external web
pages from starting the ceremonies; they do not authenticate an independent OS
principal. Local clients can construct those headers and obtain development
observations. No endpoint can issue a physical capability.

Requests have an 8 KiB header limit, 32 KiB body limit and total five-second read
deadline. Responses have a total five-second write deadline. Duplicate headers,
ambiguous lengths, chunking, Expect/Upgrade and buffered pipelining are rejected.
Each connection executes at most one request and is closed even if a browser
requests keep-alive. There is no persistent HTTP connection or general server
framework. CSP blocks external content and framing; responses use no-store and
nosniff. Headers, tokens, credential identifiers and assertions are not logged.
The server expires after 15 minutes; its Stop button or Ctrl+C can stop it sooner.

## Dependency review

`webauthn-rs = 0.6.2` is an exact pin, with defaults disabled. Its public
passkey API enforces required user verification. None of the dangerous feature
flags for client-supplied state, credential internals or presence-only keys is
enabled. Pending state is never serialized to the client. The software
authenticator is a **dev-dependency only**, used to produce genuine signatures
over synthetic credentials; it explicitly falsifies UV for test coverage and
must never be a trusted enrollment mechanism.

The downloaded normalized manifest reports Rust >=1.89, below this project's
pinned 1.99.1. The resolved library uses Rust crypto through `crypto-glue`;
no OpenSSL provisioning was required in the observed macOS build. The lockfile
also contains upstream-selected pre-release crypto dependencies, including
`rsa 0.10.0-rc.19` and `kbkdf 0.1.0-rc.1`; this PoC does not establish release
qualification or a full transitive security audit.

The WebAuthn crates and `crypto-glue` retain MPL-2.0. Our independently written
files remain Apache-2.0; no upstream source is copied or relabeled. The complete
resolved manifest-license inventory is [DEPENDENCIES.tsv](DEPENDENCIES.tsv),
including development and non-host-target dependencies. No package had missing
license metadata. The remaining expressions offer permissive options or use
BSD/Unicode licenses. This is an engineering dependency inventory, not legal
certification or an assertion that transitive code was audited. Binary
distribution must preserve third-party notices and fulfill the MPL source
availability requirements; this experiment ships no binary.

Primary references inspected on 2026-11-06:

- [WebAuthn server API and state warning](https://docs.rs/webauthn-rs/0.6.2/webauthn_rs/)
- [Published manifest and feature flags](https://docs.rs/crate/webauthn-rs/0.6.2/source/Cargo.toml)
- [Upstream MPL-2.0 license](https://github.com/kanidm/webauthn-rs/blob/master/LICENSE.md)
- [WebAuthn assertion verification specification](https://www.w3.org/TR/webauthn-3/#sctn-verifying-assertion)

## Verification

From the repository root, with the pinned Cargo toolchain available:

```sh
cargo fmt --manifest-path experiments/task152-passkey/Cargo.toml -- --check
cargo clippy --offline --locked --all-targets --manifest-path experiments/task152-passkey/Cargo.toml -- -D warnings
cargo test --offline --locked --manifest-path experiments/task152-passkey/Cargo.toml
node --check experiments/task152-passkey/src/browser.js
node --test experiments/task152-passkey/browser_test.mjs
```

The initial reproducible observation was absence of a WebAuthn verifier or
enrollment path; Task020 rejected every missing real approval. Focused tests
cover changed target/payload/recovery/expiry, unknown credential, exact origin,
signed wrong RP hash, missing UV, signature corruption, different challenge,
replay, expiry, cancellation, restart, journal consumption and bounded inputs.
The unchanged Task020 guard tests also run through the imported model.

The browser runner adds negative HTTP framing/CSRF/origin/session tests, a
genuine-signature synthetic registration/assertion through its handlers and a
real localhost socket test. On macOS arm64 the final run passed 45 library tests,
six runner tests and one compile-fail doctest; fmt, clippy and JavaScript syntax
checks passed. The initial sandbox run passed the library and five handler tests
but the socket test failed because localhost bind was denied; the complete
offline suite then passed with authorized localhost socket access. No external
host or KNX interface was contacted.

The first Safari attempt was reported by the operator as cancelled/rejected,
without a useful error stage. The cause was not established. The follow-up UI
prepares and decodes a challenge before enabling the ceremony and invokes
`navigator.credentials.create/get` directly during the button click. Prepared
challenges refresh after 100 seconds, before the server's 120-second limit; a
stale click refreshes options and requires another explicit click. Preparing a
challenge never opens a credential prompt. Errors now show only a fixed stage,
an allowlisted exception name or HTTP status and the observed activation boolean;
browser error messages and credential details are never displayed or logged.
Eight simulated-DOM Node regression tests passed, including a provider mock
that rejects calls made outside the synchronous user gesture. This is not an
observed Safari success. [WebKit documents gesture requirements and Fetch
propagation](https://webkit.org/blog/11312/meet-face-id-and-touch-id-for-the-web/),
so an asynchronous fetch was a compatibility concern, not a confirmed diagnosis.

A later Safari attempt reached HTTP 403 during challenge preparation, before
any credential API call. Source inspection found a standards conflict: the
response used `Referrer-Policy: no-referrer` while POST used `mode: same-origin`.
[Fetch's Origin-header algorithm](https://fetch.spec.whatwg.org/#append-a-request-origin-header)
serializes the Origin as `null` for that combination. The document and POST now
use `same-origin` referrer policy; cross-origin referrers remain suppressed and
the exact Origin, Host, Fetch Metadata and CSRF checks remain required. Null
Origin is still rejected. Boolean-only rejection diagnostics distinguish
present/matching/null without exposing any header value or token. The 403 is an
operator observation; the null-Origin explanation is standards-derived until a
sanitized native request observation confirms it. Nine simulated browser tests
cover the explicit POST referrer policy and safe diagnostics.

An operator-driven native-vault browser enrollment/assertion, protected enrollment,
trusted-display/separation tests and hardware integration remain **NOT RUN**.
Task152 acceptance criteria and the Task020 hardware write remain incomplete.
