# Task-138: Configure project agents and model routing

## Subject

Prepare project-local Codex roles with explicit Sol/Luna routing and bounded delegation.

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-138 |
| Status | In review |
| Stage / required gate | Development tooling; no product gate required or unlocked |
| Priority | P1 |
| Component / Git project | knx-editor / development configuration |
| Owner | Primary coding agent |
| Requirement references | Existing AGENTS.md workflow; explicit user request on 2026-10-20 |
| Evidence level | Offline configuration checks only |

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
- [ ] AC-004: Templates installed in the real project's protected `.codex/` directory and confirmed in a new trusted session.
- [ ] AC-005: Source commit and independent review recorded after repository setup.

## Dependencies to Other Tasks

No predecessor needed to prepare this explicitly requested tooling increment.
The user initialized the local Git repository and installed the configuration.
Git metadata is now writable and the task branch has been created.
New-session project discovery/trust remains to be confirmed. Task-002 as a whole
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
`task/Task-138-project-agent-config`. Real-project `codex --no-daemon debug prompt-input` also failed
with `Operation not permitted`, so new-session role discovery remains unproven. Cached model metadata omitted Sol's
requested alias despite the local user setting/current runtime exposing it; live
access is untested. The instructions cannot enforce billing quotas or device policy.

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

## Completion Record

| Field | Observed result |
|---|---|
| Implementation / report status | In review; runtime files installed and matched, session discovery pending |
| Source commit(s) | Initial source commit pending below; task branch created successfully |
| Platform | macOS; Python 3.14.6; codex-cli 0.163.0 |
| Tests executed | `python3 -B scripts/check_agent_configs.py`: PASS for templates and installed copies |
| Regression tests | `python3 -B scripts/test_agent_configs.py`: 9 offline tests PASS after the review fix |
| Documentation checks | 3423 local Markdown links checked; zero unresolved paths; 138 unique task files with matching headings; changed files UTF-8/LF/final newline |
| CLI observations | Isolated temporary CODEX_HOME: `codex features list` exit 0; `codex --no-daemon debug prompt-input 'Inspect configuration only.'` exit 0; all seven role names present |
| Unsupported CLI checks | Adding `--strict-config` to features/debug failed because those subcommands do not support it; strict schema validation NOT RUN |
| Evidence location | This record and AGENT-WORKFLOW.md; validation scripts reproduce offline checks |
| Acceptance criteria met | AC-001 through AC-003; AC-004/005 outstanding |
| Technical outcome | Prepared configurations parse and are locally discoverable in an isolated temporary Codex home; installation verified; new-session discovery unproven |
| Gate outcome | No gate reviewed or unlocked |
| Reviewer | Independent Sol subagent review: additional installed-role detection gap found and fixed with regression coverage; no maintainer acceptance claimed |
| Hardware / Windows / live model test | NOT RUN; outside this tooling increment |
| Next action | Confirm roles in a new trusted project session and obtain maintainer acceptance; Task-001 remains first ready baseline task |

CLI reproduction without touching personal configuration or making model requests:

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
