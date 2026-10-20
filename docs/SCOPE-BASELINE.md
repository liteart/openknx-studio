# Executable planning scope

Task-001 establishes a planning baseline, not implemented product capabilities.
The machine-readable [scope](../config/project-scope.toml) separates confirmed
requirements, confirmed direction, exclusions and engineering defaults. Its
requirement IDs refer to [REQUIREMENTS.md](REQUIREMENTS.md). Logical repository
names are recorded; owners and publication remotes remain undecided.

The [checker](../scripts/check_project_scope.py) pins the parsed TOML values,
checks local Markdown destination paths and task identities, and requires one
index row per task. It excludes hidden/build directories and nested Git repositories.
It does not check URL availability, heading fragments, all Markdown syntax, runtime
capabilities, legal rights or human acceptance. Python 3.11+ is required; no third-party
dependencies or network access are needed.

```sh
python3 -B scripts/check_project_scope.py
python3 -B scripts/test_project_scope.py
```

## Review checklist

- Compare confirmed decisions with GOALS, AGENTS and requirements R-001 through
  R-030. Rust owns application/protocol implementation; KNX wire code belongs
  only to the separate knust fork; .NET is limited to necessary original Windows
  DCA hosting. Preserve Apache-2.0 ownership and upstream notices, no paid tiers,
  JSON/Git authority, cache-only SQLite, local operation and English/localization.
- Verify exclusions: IP/Data Secure and keys, legacy plug-ins, generic utility apps,
  general editor UI, and native DCA replacement as the primary strategy.
- Check [PoC A/B/C success conditions](../GOALS.md#stage-0-minimum-risk-reduction-pocs)
  and the [production stop gate](ROADMAP.md#g-poc-minimum-technical-feasibility).
  Task-028 requires genuine PASS evidence and explicit maintainer GO. A passing
  scope check never unlocks that gate.
- For a scope change, require an explicitly approved human decision recorded in an
  ADR using the [template](../templates/ADR-TEMPLATE.md), plus corresponding
  requirement/task/test updates. Review the diff before updating the parsed-value
  hash in the checker. An ADR file or changed hash alone grants no approval.
- Resolve repository owner/remotes only from explicit user choices; local preparation
  does not require inventing publication destinations. Recheck links and task IDs.

## Task-001 evidence, 2026-10-20

Baseline inspected: `47959c19c43c84a762b70cc2e8db3b9bc567b54b`.
Environment: macOS 27.0 (26A428), Darwin arm64, Python 3.14.6.
Inputs are owned Markdown/TOML/Python planning files; no manufacturer fixture is used.
The parsed scope pin is `371458c1330c4e6f5d1275cca3f4321f184f84ef5c915c355fb53b85c82d4c95`.
Source commits are recorded in the Task-001 completion record.

Initial draft observation: the checker exited 1 because the hash was a placeholder
and index counting included dependency links. The corrected checker counts index
rows. Ten synthetic regression cases passed; final check results are recorded in Task-001. Negative cases change a Secure exclusion or Rust
scope (even with an agent-written ADR), remove/malform the scope, break links, or
duplicate task identities/index rows. All must be rejected. Comments alone pass; a nested Git repository is excluded.
The semantic pin detects drift; human review checks whether the baseline is correct.

A read-only explorer compared the baseline against the explicit decisions and found
no conflicting normative architecture. PoC criteria and stop conditions are in GOALS,
ROADMAP and Task-028. Historical package counts in PACKAGE-CHECKS describe the
original kit, not current task acceptance. This baseline work advances only Task-001; separately requested repository setup
is tracked under Task-002.

Hardware, Windows DCA execution, product functionality and rights certification:
NOT RUN; outside this documentation task. No gate or maintainer acceptance is claimed.
