# Task-148: Probe the original DCA constructor on a clean Windows host

## Metadata and readiness

| Field | Value |
|---|---|
| Task ID | Task-148 |
| Status | Done |
| Component | Bounded original-DCA runtime research |
| Requirements | R-001, R-003, R-011, R-023, R-028, R-030 |
| Source baseline | `2be14c0` |
| Branch | `task/Task-148-clean-host-constructor` |

Accepted Task-143 binding evidence and Task-144 metadata/owned STA evidence are
predecessors. Read AGENTS, GOALS, [DCA-CONTRACT](../docs/DCA-CONTRACT.md) and
[the experiment contract](../docs/POC-CLEAN-HOST-CONSTRUCTOR.md) before edits.
This independent technical experiment requires no phase GO and grants none.
Task-005's owner decision (`5c58448`, `629575f`) authorizes assumed private use
of exact privately supplied inputs; no agent certifies rights. Task-005 remains
technically Blocked. The operator reported Proxmox OUT DROP, active VM/global
firewall and no USB/PCI KNX passthrough. A controlled TCP egress check connected
to its selected listener. A second check after reported firewall activation
also connected. Isolation remains FAIL; the VM is observably network-capable.

On 2026-10-22 the owner explicitly directed proceeding on this machine despite
the failed isolation. The coordinator records that exception solely for loading
the exact pinned original inputs and one constructor invocation in a bounded,
non-elevated interactive process. It authorizes no intentional KNX/device
operation, device fixture, Initialize/UI/Dispose call or other vendor entrypoint.
The exception replaces the isolation prerequisite for this limited experiment;
it does not establish isolation, mediation, offline operation or safety PASS.
Owned interactive evidence and pre-original critical review PASSed before
execution. The reviewed source checkpoint was committed before the coordinator
ran the original constructor once: PASS for that call. This grants no phase GO
or safety approval and does not close Task-005's technical qualification.

## Scope and acceptance

- AC-001: Pin and exercise the new host's Framework64 C#5 compiler against the
  five public net48 references from Task-144 plus the evidenced System.Xaml
  reference from the same pinned public package; compile/run the unchanged neutral
  shell. Record session-0 refusal and genuine interactive render/close evidence.
- AC-002: Owned fixture tests distinguish constructor success, exception, missing
  dependency, hang/deadline and output overflow. Invalid paths, hashes and session
  checks fail closed. The launcher bounds output while reading, enforces a real
  child deadline and confirms direct-child exit; descendants are not contained.
- AC-003: Under the explicit 2026-10-22 owner exception above, after owned tests
  and critical review pass, verify all eight Task-143
  inputs before original loading, apply the same exact redirects, and invoke only
  `Mdt.Dali.DcaApp.DcaAppMain` public parameterless constructor token `0600000E`
  on a non-elevated interactive x64 STA dispatcher in the network-capable VM.
  Record real return/failure and actual
  loaded identities/paths, stopping at the first missing dependency/service.
- AC-004: No Initialize/UI/state/Dispose call, invented SDK service, Falcon,
  network/protocol implementation, signature bypass, binary modification or ETS
  installation/runtime requirement is introduced. No intentional bus request or
  device fixture is supplied. Original code may have effects; the owner exception
  does not convert this process boundary into isolation or mediation.
- AC-005: Preserve exact commands, source/input/output hashes, environment,
  limitations and independent verification/review. PASS, FAIL, INCONCLUSIVE and
  NOT RUN remain distinct. Constructor return is not PoC A success or phase GO.

## Ownership and checks

Only this task, TASKS, TRACEABILITY, POC-CLEAN-HOST-CONSTRUCTOR and the five files
under `experiments/dca-constructor/` are assigned. No product/fork code, dependency,
license, Windows feature, firewall, service or installed application changes.
The coordinator alone operates Windows. Owned source and private evidence stay
separate; original DLLs, generated executables and reference packs stay outside
Git/distribution. The contract documents exact planned build/test commands.

Required checks: local PowerShell parsing, scope and whitespace; fresh Windows
compilation, owned session-0 negatives and interactive fixture/neutral suite;
original constructor experiment only after its prerequisites. Final source is
frozen for independent verification/review before original execution.

## Completion record

| Field | Observed result |
|---|---|
| Implementation/report status | Done for the bounded constructor experiment; runtime evidence, independent verification and critical review PASS |
| Source commit(s) | Baseline `2be14c0`; reviewed implementation `1daec2717b16625452eae8020c54e4eaa35385b6` committed before original execution; evidence record identified by this file's Git history |
| Local checks | PASS: PowerShell parser, prior five-reference C#5 static compilation and amended six-reference compilation of all four targets, scope and whitespace; no executable was run by this agent |
| New-host build/owned tests | Initial script invocation blocked by execution policy; first compiler build FAILed CS0012 and produced only fixture DLL. Fresh amended six-reference build02 PASS. Session-0 suite PASS, six cases (administrator-role=True). Interactive suite PASS, twelve cases in actual session 2 with administrator-role=False; neutral open/render/close, timeout 124 and flood 125 with confirmed direct-child exit. Exact temporary task removed; zero remaining processes for all three owned executable paths. |
| Original constructor | PASS: actual x64/STA session 2, interactive=True, administrator-role=False; all eight fixed input pins checked before loading; constructor token 0600000E returned, probe/launcher exit 0 and direct-child exit confirmed under a 15-second deadline. Only MDT primary, SDK 6.4 and AddInViews 6.4 actually loaded from the private input stage; other five inputs were pinned, not necessarily exercised. |
| Original post-run integrity/cleanup | PASS: all eight sizes/hashes identical to pre-run private manifest; five raw evidence files match manifest; exact scheduled task removed, zero matching owned executable processes and no selected ETS process names. Descendants NOT CONTAINED. |
| Original UI/state/hardware | NOT RUN: no Initialize/GetAddInUI/GetDcaUi/Dispose or state call and no intentional device operation; constructor transitive effects are not excluded by this marker |
| Technical outcome | PASS for one unchanged original parameterless constructor on the selected machine and exact staged inputs; isolation remains FAIL, no full DCA/PoC A/mediation qualification |
| Evidence location | Private coordinator evidence under `private/evidence/task148-clean-host`; concise report in linked contract |
| Independent verification/review | constructor_verify PASS for the 48 owned evidence files and all five raw original evidence files, source hashes, one-call markers, pre/post inputs and cleanup; no independent Windows rerun. binding_review final critical review PASS for source, runner and actual result evidence, with no unresolved findings. AC-001 through AC-005 met for this bounded experiment; no safety/isolation/gate approval. |
| Phase/product gate | No gate advanced; Task-005 and Tasks 007–012 unchanged |

The coordinator authorized the narrowly evidenced sixth public reference after
the actual Framework compiler required `System.Windows.Markup.IQueryAmbient`
from System.Xaml. Local modern-Roslyn static compilation with five references
did not establish this older Windows compiler's build requirements. The added
111,152-byte file has SHA-256
`f88a02dcc1dc1037884032624f3ab092aa5a3e85d215598d7d13ad7ddc814d81`
and comes from the unchanged pinned net48 package. Task-144's historical five
reference selection is unchanged. The partial failed output is retained; retry
uses a fresh build directory. No vendor code executed during these build checks;
the later original constructor run is recorded separately.

Before original staging, the coordinator recorded the private original runner
`original01-runner.ps1`, SHA-256
`b0721968ea470ae1c0ffebc2626f6dfcf78f7e499ef9a85273588a9eb7052f7f`.
It checks the actual session/administrator role before launch and pins the three
reviewed runtime artifacts. Exact eight-input inventory remains in the private
`private-input-manifest.json`. The later actual result is recorded in
`original01-result-stdout.txt` and the raw original-evidence files. The five
source hashes are unchanged from the committed reviewed checkpoint.
