# KNX Editor Development Kit

**Planning baseline:** 2026-10-19. **Language:** English. **Project status:** requirements and proposed implementation work, not a working product.

This kit consolidates the decisions in the project conversation. Later explicit user decisions replace earlier proposals. It does not establish that any repository, fork, implementation, hardware test, standalone DCA host, or release already exists.

## Start here

1. Read [GOALS.md](GOALS.md) and [AGENTS.md](AGENTS.md).
2. Read [architecture](docs/ARCHITECTURE.md), [requirements](docs/REQUIREMENTS.md), and [phase gates](docs/ROADMAP.md).
3. Select an unblocked task from [TASKS.md](TASKS.md). Start with the PoC tasks; do not bootstrap the full product first.
4. Use the task's acceptance criteria and required tests. Record actual results, not expected results.

Agent roles, model routing, and installation: [AGENT-WORKFLOW.md](docs/AGENT-WORKFLOW.md).

Offline scope and documentation checks: [SCOPE-BASELINE.md](docs/SCOPE-BASELINE.md).

## Contents

| File | Purpose |
|---|---|
| [GOALS.md](GOALS.md) | Product purpose, non-goals, PoC/pre-release/release boundaries |
| [AGENTS.md](AGENTS.md) | Mandatory development, safety, testing, and source-code Git workflow |
| [TASKS.md](TASKS.md) | Ordered task index, phase, repository, dependencies |
| [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md) | Traceable functional/non-functional requirements and engineering defaults |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Ownership boundaries and component contracts |
| [docs/DATA-MODEL.md](docs/DATA-MODEL.md) | Authoritative JSON, artifacts, cache, transactions, and deployment evidence |
| [docs/MCP-CONTRACT.md](docs/MCP-CONTRACT.md) | MCP tool families, permissions, approval boundary, structured results |
| [docs/DCA-CONTRACT.md](docs/DCA-CONTRACT.md) | Original Windows DCA hosting and bridge constraints |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Gates and objective release criteria |
| [docs/TEST-STRATEGY.md](docs/TEST-STRATEGY.md) | Evidence levels, automated tests, hardware tests, failure injection |
| [docs/RISKS-AND-INPUTS.md](docs/RISKS-AND-INPUTS.md) | Unresolved questions and required external inputs |
| [docs/REFERENCE-DCA.md](docs/REFERENCE-DCA.md) | Fresh static observations of the supplied MDT archive |
| [docs/REFERENCES.md](docs/REFERENCES.md) | Primary technical sources and limits of their evidence |
| [docs/TRACEABILITY.md](docs/TRACEABILITY.md) | Requirement-to-task coverage |
| [docs/PACKAGE-CHECKS.md](docs/PACKAGE-CHECKS.md) | Documentation-only validation results |
| [templates/TASK-TEMPLATE.md](templates/TASK-TEMPLATE.md) | Template for future task files |
| [templates/POC-REPORT-TEMPLATE.md](templates/POC-REPORT-TEMPLATE.md) | Reproducible PoC result and stop/go report |
| [templates/ADR-TEMPLATE.md](templates/ADR-TEMPLATE.md) | Architecture decision record |
| [templates/TEST-EVIDENCE-TEMPLATE.md](templates/TEST-EVIDENCE-TEMPLATE.md) | Execution evidence and hardware authorization record |

Each work item has its own `tasks/Task-NNN.md` file. IDs are stable, never reused, and need not be executed numerically when their prerequisites are satisfied. Unused IDs up to 999 remain available; no empty placeholder tasks are created.

## How to use the kit in the repositories

Place this kit at the editor repository root, or move its documents as one unit and update relative links. The repository names `knx-editor` and `knust-fork` are logical names, not existing remote URLs. Store the DCA host under `knx-editor/dca-host/`; it is a separate executable, not a third mandatory repository. Put protocol implementation and its tests only in the fork. Copy or adapt the repository-boundary instructions into the fork's own `AGENTS.md` when it is created.

The kit does not contain the user's `.etsapp`, manufacturer binaries, SDK binaries, firmware, product packages, credentials, or real installation data. Fixture acquisition tasks reference them by identity and hash. All task statuses start at **Not started**. This documentation is not proof that their acceptance criteria have been met.

## Interpreting obligations

**Confirmed requirement** means explicitly decided by the user. **Engineering baseline** means a concrete default selected to make work testable; changes require an ADR and corresponding task/test updates. **External input** means evidence, hardware, rights, or a file still needed. Never convert an external input into an invented implementation detail.

The optional future work is separately marked **Post-v1**. It must not delay minimal PoCs or become an implicit release requirement.

## License

Project-owned source, documentation and configuration use [Apache-2.0](LICENSE).
See [NOTICE](NOTICE) for the third-party boundary. The independently cloned knust
repository preserves its upstream MIT license; manufacturer and SDK inputs are
not included or relicensed. Repository setup does not grant redistribution rights
for those private inputs.
