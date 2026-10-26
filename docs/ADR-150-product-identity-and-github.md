# ADR-150: OpenKNX Studio identity and GitHub workflow

Status: Accepted — explicit owner instructions, 2026-10-26.
Related task: [Task-150](../tasks/Task-150.md).

## Decision and authority

Simon named the product OpenKNX Studio and instructed the agent to record that
name directly in the project. He then provided
`https://github.com/liteart/openknx-studio.git` and authorized direct pushes of
changes to main. These explicit instructions resolve the editor owner/remote
choices left undecided in the original planning baseline.

Record product_name OpenKNX Studio, editor owner liteart and the exact HTTPS
remote in the machine-readable scope. Keep knx-editor as the logical component
identifier. Record the tested/reviewed direct-main workflow in AGENTS.md.
No force-push, branch-rule bypass, third-party redistribution or publication of
the separate protocol fork is authorized by this decision.

## Scope and verification

This changes identity and source-development publication defaults only. Requirement
IDs, product functionality, component boundaries, license, Secure exclusions,
normal offline operation and phase gates are unchanged. Update the canonical
parsed-scope hash after reviewing these exact fields, and run the existing scope,
link and task-identity checker plus all twelve regression tests. Publication
requires an actual push and read-back; this ADR alone does not claim that result.
