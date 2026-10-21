# Task-138: Configure project agents and model routing

## Subject

Prepare project-local Codex roles with explicit Sol/Luna routing and bounded delegation.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-138 |
| Status | Done |
| Stage / required gate | Development tooling; no product gate required or unlocked |
| Priority | P1 |
| Component / Git project | knx-editor / development configuration |
| Owner | Primary coding agent |
| Requirement references | Existing AGENTS.md workflow; explicit user request on 2026-10-20 |
| Evidence level | Offline configuration checks, read-only CLI config probes, observed harness role invocations |

## Description

The user requested autonomous project agents, with high-volume work assigned to
`gpt-6.1-sol` or `gpt-6-luna`. Supply small specialist roles, coordinator defaults,
a routing contract, installation instructions, and reproducible offline validation.
This is an independent tooling task, not completion of Task-001 or Task-002.

## Detailed Requirements

- DR-001: Explicitly pin all role/default models to the requested Sol/Luna names.
- DR-002: Separate editor, protocol-fork, and minimal Windows DCA responsibilities.
- DR-003: Bound concurrent subagents to three and prevent recursive delegation by instruction.
- DR-004: Preserve readiness gates, human device authorization, source Git policy, and honest evidence.
- DR-005: Provide installable templates when managed protection denies `.codex/` writes.

## Non-functional Requirements

Follow [AGENTS.md](../AGENTS.md) and [ARCHITECTURE.md](../docs/ARCHITECTURE.md).
All owned files use English. Add no dependencies, remote services, credentials,
product code, firmware access, or permission bypasses. Read-only roles and the
model allowlist are behavioral/configuration choices, not runtime access controls.

## Acceptance Criteria

- [x] AC-001: Seven narrow role templates and coordinator defaults use only Sol/Luna.
- [x] AC-002: Routing, handoffs, ownership, concurrency and continuation are documented.
- [x] AC-003: Offline validation rejects malformed/drifted configs and wrong routing.
- [x] AC-004: Templates installed in the real project's protected `.codex/` directory and confirmed in a new trusted session.
- [x] AC-005: Source commit and independent review recorded after repository setup.

## Dependencies to Other Tasks

No predecessor needed to prepare this explicitly requested tooling increment.
The user initialized the local Git repository and installed the configuration.
Git metadata is now writable and the task branch has been created.
Project config-layer discovery passed in the read-only follow-up. The operator
subsequently confirmed fresh-session role execution, as recorded below. Task-002 as a whole
is not accepted by this observation; no existing product gate is changed.

## Required Tests

- TEST-001: Parse TOML and validate exact role identities, models, efforts, and defaults.
- TEST-002: Reject malformed TOML, disallowed model, wrong identity, permission override, missing role, changed defaults, installed drift, unexpected installed role.
- TEST-003: Load prepared configuration using the installed CLI without model calls.
- TEST-004: Check real-project discovery and model access after human installation.

## Deliverables

- `config/codex/config.toml` and seven files under `config/codex/agents/`.
- [AGENT-WORKFLOW.md](../docs/AGENT-WORKFLOW.md), root AGENTS instructions, README/task-index links.
- [check_agent_configs.py](../scripts/check_agent_configs.py) and [test_agent_configs.py](../scripts/test_agent_configs.py).

## Side Effects and Authorization

User explicitly authorized agent configuration. No hardware, vendor execution,
remote publication, global Codex edits, or license changes were performed.

## Out of Scope

Product implementation, actual fork creation, Windows hosting, live model billing
checks, and execution of the full backlog.

## Risks and Blockers

The user installed `.codex/` and initialized Git. Installed copies match templates.
Earlier sessions could not write `.git/`; the resumed session successfully created
`task/Task-138-project-agent-config`. The initial real-project CLI diagnostic failed with `Operation not permitted`;
a later authorized retry and config-layer probe are recorded below. The standalone
CLI probe did not expose role-file discovery; later operator-reported functional
confirmation is recorded below. Cached model metadata omitted the requested
Sol alias despite successful role invocations in the current harness. The instructions cannot enforce billing quotas or device policy.

## Follow-up installation verification

On 2026-10-20 the user reported `git init` and the installation commands complete.
`git status --short` confirmed all files untracked; `git symbolic-ref --short HEAD`
confirmed `main`; `git log -1` confirmed the repository has no commits.
Both offline checks passed again, including exact installed/template comparison.
Independent Sol review found that additional installed agent files were not rejected.
The checker now rejects them; a ninth regression case covers that failure.
`git switch -c task/Task-138-project-agent-config` failed to write `HEAD.lock`;
its additional success-looking output did not change HEAD. No staging or commit
was attempted on `main`. Real-project prompt rendering was blocked by OS permissions.

The user subsequently explicitly authorized adding the complete project to Git.
A retry of `git switch -c task/Task-138-project-agent-config` still exited 255
with `HEAD.lock: Operation not permitted`; `git symbolic-ref --short HEAD`
confirmed unchanged `main`. No staging/commit was attempted after this failure.
The configuration checker and all nine regression tests passed again.
All 3423 local Markdown links resolved. Inventory found a generated Python
bytecode cache; it was removed and `.gitignore` now excludes Python caches.
The remaining 178 files are Markdown/TOML/Python source plus `.gitignore`,
with no symlinks or unexpected file types. This is not a legal rights review.
No task acceptance or product gate was changed.

## Resumed Git verification

On 2026-10-20, `git switch -c task/Task-138-project-agent-config` exited 0.
The user explicitly repeated the request to retry Git. Both offline validation
commands passed: installed copies match and all nine regression tests pass.
A Python documentation scan checked 178 files, 138 matching task headings and
3423 local Markdown links with zero errors and no symlinks. The configured Git
author identity already existed; no identity was created or changed. The runtime
exposes all seven custom role definitions, but no new model calls or trusted-project
discovery test were performed. AC-004 remains open pending that confirmation.

The initial local commit succeeded: `c4b17ed970e4894b1dd6c13b14037aa90fccd701`
contains the 178 reviewed source/documentation/configuration files.
`git diff --cached --check` passed before commit and `git status --short` was empty
afterward. No remote push, merge, tag, or publication was performed.

## Runtime evidence follow-up, 2026-10-20 (historical)

On 2026-10-20, the active harness completed successful delegations through all
seven configured roles across Tasks 001-005: explorer (`gpt-6-luna`), implementer,
protocol, and dca (`gpt-6.1-sol`), verifier (`gpt-6-luna`), reviewer
(`gpt-6.1-sol`), and evidence (`gpt-6-luna`). This is evidence that the current
harness can invoke the predefined roles with their configured model names. It does
not prove independent CLI project-role discovery, account billing setup, or a
separate model-access entitlement. No model access error was reported.

The installed `codex-cli 0.163.0` real-project `codex --no-daemon debug prompt-input`
attempt initially failed with `Operation not permitted`. An authorized read-only
retry exited 0 and returned 37,465 bytes of rendered JSON. All seven names appeared,
but exact role-file descriptions did not. Because the rendered prompt does not
include the runtime tools schema and names may originate from project/user
instructions, this is **INCONCLUSIVE** for custom role discovery. The tested CLI
commands rejected `--strict-config` as unsupported, so strict schema validation is
NOT RUN. No global settings or trust state were changed.

The actual runtime description fields are present in all seven installed/template
files and are validated by the local checker; independent checks at this source
revision confirmed template regular-file/byte equality and nine regression tests.
That static result does not resolve the runtime discovery question. AC-004 remains
unchecked, Task-138 remains In review, and TEST-004 remains INCONCLUSIVE pending a
clear project-role discovery result. No product gate, hardware permission, or billing
claim follows from this experiment.

A subsequent fresh `codex app-server --stdio` process was tested using its
locally generated JSON-RPC schema: `initialize`, `initialized`, then `config/read`
with the actual project root as `cwd` and `includeLayers=true`. Initialization and
config reading passed. Three layers were returned; the project `.codex/config.toml`
layer supplied all four expected public agent defaults: enabled, concurrency limit
3, `gpt-6-luna`, and high effort. Field origins confirmed that project layer.
Only filtered public evidence was printed; no settings, trust, or credentials changed
and no model request was made by the probe. The response schema exposes config,
layers, and origins, but no standalone role definitions. Thus new-process project
configuration discovery is PASS while standalone role-file discovery remains
INCONCLUSIVE; AC-004 is not fully met and is not redefined by this partial result.
An earlier probe used an unsupported transport spelling before the corrected
`--stdio` handshake succeeded.

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | Done; operator confirmed fresh-session explorer/reviewer execution on 2026-10-21; independent final review PASS |
| Source commit(s) | `c4b17ed970e4894b1dd6c13b14037aa90fccd701` (initial source snapshot); runtime follow-up based on `242689194f669fd647e90c531f4c2bbe1857034a`; reviewed runtime follow-up `34d2fead068ab429ee390b58d8e0ff7ddf0eeec6` |
| Platform | macOS; Python 3.14.6; codex-cli 0.163.0 |
| Tests executed | `python3 -B scripts/check_agent_configs.py`: PASS for templates and installed copies; installed/template byte equality independently confirmed |
| Regression tests | `python3 -B scripts/test_agent_configs.py`: 9 offline tests PASS after the review fix; repeated on `242689194f669fd647e90c531f4c2bbe1857034a` |
| Documentation checks | Initial snapshot: 3423 local Markdown links and 138 task identities checked. Current follow-up: scope checker PASS; changed files UTF-8/LF/final newline |
| CLI observations | Earlier isolated temporary CODEX_HOME prompt render contained all seven role names. Real-project CLI retry exited 0 and rendered 37,465 bytes; names appeared, role descriptions did not, so standalone custom-role discovery is INCONCLUSIVE. Harness completed role delegations for all seven roles on Tasks 001-005 without a reported model-access error; this does not establish billing or independent CLI discovery. |
| Unsupported CLI checks | `--strict-config` was rejected as unsupported by tested commands; strict schema validation NOT RUN |
| Scope / whitespace | `python3 -B scripts/check_project_scope.py`: PASS, exit 0; reports product/Windows/hardware/rights/human gate work NOT RUN. `git diff --check`: PASS, exit 0. |
| Evidence location | This record and AGENT-WORKFLOW.md; validation scripts reproduce offline checks |
| Acceptance criteria met | AC-001 through AC-004, including operator-confirmed new-session execution recorded below; AC-005: initial source commit recorded and independent follow-up review PASS; reviewed follow-up revision `34d2fead068ab429ee390b58d8e0ff7ddf0eeec6`. |
| Technical outcome | PASS for task scope: installed copies match, all seven harness roles were used, project config-layer discovery passed, and the operator confirmed new-session explorer/reviewer execution. Historical standalone CLI probe remains INCONCLUSIVE. |
| Gate outcome | No gate reviewed or unlocked |
| Reviewer | Prior independent Sol subagent review: installed-role detection gap found and fixed with regression coverage; current follow-up: predefined sequential_review PASS for this partial evidence increment, after two stale metadata phrases were corrected; no full task acceptance claimed |
| Hardware / Windows / standalone CLI model test | NOT RUN for this tooling increment; current harness role invocations are observed separately above |
| Next action | Task-138 complete. Tasks 003 and 005 remain blocked on Windows inventory/SDK prerequisites; no product gate is unlocked. |

Historical names-only CLI reproduction without model requests. This assertion does
not establish standalone role-file discovery:

```sh
python3 - <<'PY'
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
with tempfile.TemporaryDirectory(prefix="knx-codex-config-") as tmp:
    shutil.copyfile("config/codex/config.toml", Path(tmp) / "config.toml")
    shutil.copytree("config/codex/agents", Path(tmp) / "agents")
    env = dict(os.environ, CODEX_HOME=tmp)
    subprocess.run(["codex", "features", "list"], env=env, check=True)
    result = subprocess.run(
        ["codex", "--no-daemon", "debug", "prompt-input", "Inspect configuration only."],
        env=env, check=True, capture_output=True, text=True, timeout=30,
    )
    for role in ("explorer", "implementer", "protocol", "dca", "verifier", "reviewer", "evidence"):
        assert role in result.stdout, role
    print("PASS: all seven names in locally rendered prompt")
PY
```

## Operator-confirmed new-session execution, 2026-10-21

After the coordinator requested a new project session that invokes `explorer`
and `reviewer`, the operator explicitly answered that both were successfully
executed. This is operator-reported functional evidence, not a captured standalone
CLI tool schema or an independently reproduced transcript. Together with installed
byte equality, current harness use of all seven roles and fresh-process project
config-layer discovery, it satisfies the requested new-session confirmation in
AC-004. No account/billing or all-client compatibility claim follows. Historical
INCONCLUSIVE probe results above are retained and are not relabeled PASS.

On this follow-up, `python3 -B scripts/check_agent_configs.py` passed for templates
and installed copies; `python3 -B scripts/test_agent_configs.py` passed all nine
cases. The predefined sequential_review agent independently reproduced the agent checker,
nine tests, scope checker and whitespace check with PASS, and returned PASS for
ordinary-task closure. Task-138 is Done under the authorized workflow. No product
phase gate changes.
