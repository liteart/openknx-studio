# Task-150: Record OpenKNX Studio identity and connect GitHub

## Metadata

| Field | Value |
|---|---|
| Task ID | Task-150 |
| Status | In progress |
| Component | Editor documentation and source repository |
| Owner | Primary coordinator |
| Authority | Explicit owner naming and GitHub/main push instructions on 2026-10-26 |

## Scope and decisions

The official product name is OpenKNX Studio. The canonical editor remote is
`https://github.com/liteart/openknx-studio.git`. The owner authorized direct pushes
of tested, reviewed changes to `origin/main` after each logical increment.
Technical component/crate names and the separate knust fork retain their identities.
No protocol, DCA functionality, license or product-scope change is included.

## Acceptance criteria

- [x] README, goals, ownership contract and project notice record the product name.
- [x] The official remote and standing direct-main workflow are recorded and configured.
- [x] Applicable documentation checks and bounded initial-publication audit pass.
- [ ] The intended main revision is pushed and independently read back from GitHub.

## Checks and evidence

Required: documentation/link/task-index checks, whitespace check, bounded review of
reachable tracked history for proprietary/private inputs and obvious credentials,
and remote ref comparison. No hardware or original vendor runtime is required.
Do not push ignored local inputs, sibling fork history, unrelated branches or tags.
No force-push or remote-history replacement is authorized.

## Completion record

Product name and editor owner/remote are recorded in config/project-scope.toml as
well as the human-facing documents. ADR-150 records the explicit owner decisions;
the canonical scope pin is now
e316176f102c57d11f4a979fe9b10d8e3f4086371ca2b37d92be282c9283221c.
No other parsed scope value changed. Origin is configured with the exact HTTPS URL;
main starts from accepted Task009 closure3eac1c233341f2811d409ccce49bf066775bffa8.

Executed checks:

- `python3 scripts/check_project_scope.py`: PASS.
- `python3 scripts/test_project_scope.py`:12 tests PASS. The initial run exposed
  an existing fixture omission: linked experiment source files were not copied.
  Adding the owned experiments directory to the fixture copy resolved it without
  removing tests or changing validator behavior.
- `git diff --check`: PASS.
- `git ls-remote --symref https://github.com/liteart/openknx-studio.git HEAD refs/heads/main`:
  exit0, empty remote before first publication.
- `GIT_TERMINAL_PROMPT=0 git push --dry-run origin main`: PASS using existing Git
  authentication. GitHub CLI login was unavailable and is not required by this route.

Independent bounded publication/source audit: PASS, no material findings.
Private receipt: private/evidence/task150-publication/audit.md, SHA256
4ffdadf5b911e81626bd8cfe5b22092474870d0319500d5775fb68b73d3aca46.
The reachable base has166 commits/756 unique UTF-8 blobs; no binary/manufacturer
inputs or common credential matches were found. This is a bounded hygiene check,
not legal certification or a general secret-detection guarantee.

Parallel task branches, including later Task006/016/017/023/024/149 work/evidence,
are not integrated by this publication. They remain local and unchanged. The
published lineage includes the current Task009 result; it is not a claim that all
parallel project work has been consolidated. The separate protocol fork is not
published or vendored here.

Actual first push and remote read-back are pending. No successful publication is
claimed from the dry run alone. Product/Windows/vendor/hardware tests NOT RUN for
this documentation/repository task; existing Task009 evidence is unchanged.
