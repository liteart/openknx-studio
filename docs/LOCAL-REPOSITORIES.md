# Local repository setup evidence

Current editor product: **OpenKNX Studio**. Official editor remote:
`https://github.com/liteart/openknx-studio.git`, configured as `origin`;
[Task-150](../tasks/Task-150.md) records the owner-authorized direct-main workflow.
The Task-002 local-only observations below are historical; fork ownership and
push permissions remain separate.

Task-002, 2026-10-20; macOS 27.0 arm64, Python 3.14.6. User authorization covers
a local fork and local commits. No external fork creation or push was performed.

The editor root and `knust-fork/` have separate Git histories. The nested checkout
is ignored by the editor, not a submodule or a source import. Cloning the editor
alone does not recreate it. Reproduce its baseline with:

```sh
git -c core.hooksPath=/dev/null -c credential.helper= -c protocol.allow=never -c protocol.https.allow=always -c submodule.recurse=false clone --no-checkout --origin upstream https://github.com/dphi/knust.git knust-fork
git -C knust-fork config core.hooksPath /dev/null
git -C knust-fork config submodule.recurse false
git -C knust-fork config remote.upstream.pushurl DISABLED
git -C knust-fork -c core.hooksPath=/dev/null -c core.autocrlf=false switch -c task/Task-002-local-fork 287bd5c94644688ac2eb93bf09f10a37bc509e0c
```

The actual clone used a temporary directory then moved the complete checkout to
`knust-fork/`. The initial sandboxed attempt failed DNS resolution; the authorized
network retry succeeded. No repository-supplied hooks, submodules, build scripts
or examples were executed. The cloned history has 32 upstream commits and is not
shallow. The fetch remote is `https://github.com/dphi/knust.git`; push URL is
`DISABLED`, and no fork-owner remote exists. Local configuration is a convenience,
not a security boundary or a portable configuration embedded in commits.

The immutable upstream baseline is
`287bd5c94644688ac2eb93bf09f10a37bc509e0c`. LICENSE SHA256:
`7b1bbf04daff1697045c1fa8f746fce7626b65302f98e0651efa1d9a078422e0`.
The original MIT LICENSE, Cargo license metadata, Cargo.lock and protocol source
are unchanged. Only fork instructions/provenance and private-input ignore rules
are added. New project-owned contributions follow the project's Apache-2.0 requirement;
contribution notice/compatibility treatment remains for review before implementation.
This does not relabel the upstream MIT code. No manufacturer or SDK input was used.
The editor now includes the official Apache-2.0 LICENSE and a NOTICE identifying
the separate upstream/vendor boundaries. No upstream license was replaced.

Static assertions executed via Python subprocess and tomllib: distinct top-level
Git directories; exact upstream HEAD; non-shallow clone; expected fetch/disabled
push URL; no diff against baseline for LICENSE/Cargo manifests/source; no tracked
`knust-fork` entries in editor; ignore checks for private, artifacts, captures,
credentials, projects, cache, target, environment files and KNX archive/key formats
in both repositories; MIT package metadata and no editor dependency. All PASS.
These inspect repository setup; they do not qualify device support or dependencies.

Additional executed checks:

```sh
python3 -B scripts/check_project_scope.py
git -C knust-fork diff --check
git diff --check
```

All PASS. Reinspect provenance/ownership without building or contacting devices:

```sh
git -C knust-fork rev-parse --show-toplevel
git -C knust-fork rev-parse --is-shallow-repository
git -C knust-fork log -1 --format=fuller
git -C knust-fork diff 287bd5c94644688ac2eb93bf09f10a37bc509e0c -- LICENSE Cargo.toml Cargo.lock src
git ls-files -- knust-fork
```

Cargo builds/tests, hardware, Windows and rights certification: NOT RUN. Source
cloning is not a compatibility proof. Upstream Secure sources are preserved but
not activated; Task-015 must audit features and inherited tests before execution.
At the time of the initial local clone, Task-001 was pending human acceptance;
that historical preparation did not declare all Task-002 done. The later
ordinary-task acceptance and licensing increment are recorded below. No phase
gate is advanced.

Local fork setup commit: `d6e25fb206850dd6da18d8acf9b82ecdf5234733`.
An independent Sol reviewer verified the static setup and requested correction of
blanket MIT wording for future contributions. The correction was re-reviewed with
no remaining findings. After commit, the fork working tree was clean; an exact
baseline diff for LICENSE, Cargo manifests/lockfile and source was still empty.

## Task-002 final licensing increment

Predecessor Task-001 was accepted on 2026-10-20 under the user-authorized
ordinary-task reviewer process (`91ec308`). The new editor LICENSE was fetched
from https://www.apache.org/licenses/LICENSE-2.0.txt, the official Apache source,
without modifying its 11,358 bytes. SHA256:
`cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30`.
NOTICE and README apply the already confirmed Apache-2.0 decision to project-owned
material and keep upstream MIT/vendor rights separate. No copyright owner or
legal approval was invented. No proprietary input was added. Remotes and protected
main hosting are not configured because the user explicitly selected local-only
forks and commits; no remote protection is claimed.

Exact static checks for this increment are recorded in Task-002. Before Cargo
manifests or protocol changes are added, the current tree has no editor Rust
source or dependency graph; the fork Cargo manifest has no reverse editor
dependency. This is a source ownership review, not a transitive dependency or
redistribution audit (those have later dedicated tasks).

Final predefined verifier/reviewer result: PASS, no blocking findings. Both ran
`python3 -B scripts/check_project_scope.py`, `python3 -B scripts/test_project_scope.py`
(10 PASS), `git diff --check`, and `git -C knust-fork diff --check`. The first
verification reproduced four scope-test failures because temporary fixtures
omitted LICENSE/NOTICE referenced by README. Copying those two owned text files
into the fixtures fixed all four; the verifier and reviewer reproduced the pass.
This is the only test-code adjustment in the final licensing increment.
