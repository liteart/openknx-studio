# Project Agent Workflow

## Purpose and routing

Use a small coordinator with on-demand specialists. Delegate long searches, test logs,
and repetitive evidence work; keep task selection and integration in the primary thread.
All project-owned instructions remain in English. User conversation can use German.

| Role | Model | Effort | Responsibility |
|---|---|---|---|
| Primary coordinator | gpt-6.1-sol | medium | Readiness, delegation, integration, bounded autonomous progress |
| explorer | gpt-6-luna | high | Targeted code/input/documentation investigation; no edits |
| implementer | gpt-6.1-sol | high | Rust editor implementation and focused regression tests |
| protocol | gpt-6.1-sol | high | Generic KNX implementation in the separate knust fork |
| dca | gpt-6.1-sol | high | Minimal Windows host, SDK compatibility, Rust bridge |
| verifier | gpt-6-luna | high | Assigned offline checks and failure reproduction; no source edits |
| reviewer | gpt-6.1-sol | high | Independent correctness, ownership, durability, security review |
| evidence | gpt-6-luna | medium | Task records and documentation based on observed results |

Use only the needed roles. A typical implementation needs one writer, then verification
and review, with evidence preparation after results exist. Do not launch all seven roles
for each task. No separate architect role is needed: resolve routine choices in the
coordinator and use a focused reviewer for consequential contract changes.

These are project choices, not measured cost guarantees. All configured model calls use
Sol or Luna, including the primary and generic subagent default. There is no token quota
or billing enforcement here. Avoid full-history forks when a self-contained task brief
is sufficient. Prefer Luna for large scans; use Sol for ambiguous implementation.

## Installation

The source templates are [config/codex](../config/codex/config.toml). Codex loads project
agents from `.codex/agents/`. This workspace's managed filesystem denies agent writes
to `.codex/`. The user installed the runtime files on 2026-10-20; the
checker confirmed that all seven roles and project defaults match their templates.

From the project root, the human operator can install the reviewed templates:

```sh
mkdir -p .codex/agents
cp -n config/codex/config.toml .codex/config.toml
cp -n config/codex/agents/*.toml .codex/agents/
python3 scripts/check_agent_configs.py
```

`cp -n` preserves existing files. If destinations already exist, compare them and merge
intentionally; a successful copy command alone does not establish that they match.
The checker compares installed files with templates when the runtime directory exists.
Keep the templates and installed copies synchronized after future changes.

Open a new Codex session in a trusted project and confirm the custom role names are
available before relying on routing. Untrusted project configuration can be skipped.
If the pre-Git folder is not discovered as a project, finish the separately authorized
repository setup before claiming activation. Do not grant trust automatically or alter
global configuration. A running session is not proof that new settings were loaded.

Compatibility baseline: installed CLI `0.163.0`; local account config selected
`gpt-6.1-sol`, while the cached model catalog listed Luna but omitted that Sol alias.
The current session exposes both model names. Cache presence/absence does not prove
live access. A Sol review subagent was requested during follow-up verification;
this uses explicit model routing and does not prove discovery of project-local roles.
The resumed session successfully created the task branch on 2026-10-20; the earlier
Git metadata write restriction no longer blocks the initial local commit.
If Sol cannot be selected, report the access failure and ask for an explicit routing
change; never silently substitute another model.

The fork will need its own applicable AGENTS.md and agent installation after Task-002.
These editor-local files do not configure an independent sibling checkout automatically.

## Delegation contract

Send each specialist this concise brief, adding only task-specific context:

```text
Task: Task-NNN and exact requested behavior
Readiness: accepted predecessors; required gate evidence or no gate
Repository / revision / branch / worktree:
Read: task, relevant AGENTS.md, component contract, evidence paths
Write: explicit paths, or none
Inputs: verified references, hashes, APIs; unresolved prerequisites
Checks: exact allowed commands; platform; offline/simulator distinction
Deliver: result, paths, command outcomes, revisions, blockers, next action
Constraints: no further delegation; no hardware or publication authorization
```

The parent checks readiness before starting a writer. An explorer may investigate
missing prerequisites without claiming the blocked implementation is ready. Independent
read-only investigations may run together. Verification follows implementation; review
uses the actual diff and test results. Parallel writers use separate task worktrees and
never share a device session. Stop repetition when an external input is required and
continue only independent work inside the user's scope.

An agent that cannot access custom roles can use explicit model selection with the same
brief and role instructions. Project prompts request this delegation; tools and managed
policies still determine what the client permits. Three concurrent subagents is a cap,
not a target. Read-only roles are behavioral instructions, not separate sandboxes;
subagents inherit parent permissions. These files cannot enforce KNX write approval,
provide a Windows host, certify rights, or grant maintainer acceptance.

## Suggested starting prompt

```text
Execute Task-001 within its acceptance criteria. Check readiness first.
Use explorer on gpt-6-luna for targeted baseline/link inspection when useful.
Use gpt-6.1-sol for implementation and independent review, and gpt-6-luna
for verification and evidence. Keep one writer until Git setup exists.
Do not begin gated product work or contact hardware. Report exact results
and the next ready task; do not accept the task on the maintainer's behalf.
```

## Sources and validation

Format and routing were checked on 2026-10-20 against the official
[Subagents documentation](https://learn.chatgpt.com/docs/agent-configuration/subagents)
and [Configuration Reference](https://learn.chatgpt.com/docs/config-file/config-reference).
Standalone files define the role identity, description, model, effort, and instructions.
Permissions remain inherited from the parent session.

Run `python3 scripts/test_agent_configs.py` for the nine regression cases.
Run `python3 scripts/check_agent_configs.py` for offline TOML, routing, role-identity,
and installed-copy consistency checks. It requires Python 3.11+ and no dependencies.
This does not validate live account access or runtime agent discovery. See
[Task-138](../tasks/Task-138.md) for execution evidence and remaining prerequisites.
