# Task013 bounded host process containment

Base editor revision: `ea56b99`. Task012 is Done; no phase gate applies.
This increment owns only process lifetime supervision, not network or device
access policy. It does not authorize an original-runtime test previously
rejected by automatic review.

`ProbeLauncher` joins an unnamed Windows job **before** calling
`Process.Start`. The job admits two active processes: the launcher and its
selected host. It forbids breakaway and further process creation while both
are active. Membership is checked immediately after start; return and temporary
file cleanup require direct-child exit and accounting showing only the launcher.
A failure to create, configure, verify or join the job fails before host start.

The non-inheritable job handle is retained for the launcher's entire lifetime.
Closing it explicitly would also kill the launcher; OS teardown closes it on
normal exit or abrupt termination. `KILL_ON_JOB_CLOSE` then terminates a still
running host. The handle has no name and is never passed to the host.
Existing outer jobs remain in effect: nested assignment or stricter outer
limits may prevent startup, which is an explicit failure.

Primary API contracts, checked 2026-11-13:

- [AssignProcessToJobObject](https://learn.microsoft.com/en-us/windows/win32/api/jobapi2/nf-jobapi2-assignprocesstojobobject)
  describes job inheritance and nested-job assignment constraints.
- [Job basic limits](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_basic_limit_information)
  define the active-process limit, breakaway flags and kill-on-close behavior.
- [Job accounting](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_basic_accounting_information)
  defines the active-process count used for cleanup confirmation.

The policy intentionally rejects DCAs requiring helper executables. It does not
contain out-of-process work delegated through an already running service, prevent
network/device access by the host itself, or isolate unrestricted same-account
interference. Those limitations still need their own runtime decision/evidence.

## Focused owned Windows test

`Test-ProcessContainment.ps1` compiles `ProcessContainmentProbe.cs` with the
exact existing pinned C#5 compiler and .NET48 references. Use a new private
output directory. It exercises child membership/descendant rejection, host
timeout cleanup, repeated setup refusal, and abrupt launcher termination while
a host is running. The independent test parent holds the actual host handle
before causing launcher termination and waits for that host to exit.
Before entering the job, the descendant case successfully runs the identical
executable/role that must subsequently be denied. A missing image or unrelated
startup failure therefore cannot substitute for the observed rejection.

```powershell
.\Test-ProcessContainment.ps1 -Compiler <pinned-csc.exe> `
  -ReferenceDirectory <pinned-net48-references> -OutputDirectory <fresh-private-output>
```

No manufacturer assembly or network/device service is needed. Native outcomes
must be recorded separately; compilation does not prove job behavior.

## Local checks

- macOS Roslyn C#5 compilation against pinned .NET48 references: PASS for full
  host, launcher, owned containment probe and fixtures.
- PowerShell parser for the focused test: PASS.
- `python3 -m unittest discover -s experiments/dca-constructor -p test_runtime_dependency_negative.py`:
  PASS, two tests.
- `git diff --check`: PASS.
- Native Windows containment execution and original DCA test: NOT RUN by this
  implementation increment. No host, firewall or hardware action was performed.

## Native owned result — 2026-11-13

On dev host `DESKTOP-3UKTEIP`, the four synthetic cases passed using the
pinned Windows compiler and framework references. The test ran through the
existing SSH session, not the limited interactive scheduled-task route. It
exercised the positive control, descendant denial, timeout cleanup, repeated
setup refusal and host termination after launcher death. The private captured
result is `private/evidence/task013-native-job-20261113.json`, SHA-256
`09c061caab427ef7b5e2e8ff4c40ebc7e47911f6cf2e7fb5704a6fd798f1d117`.
The remote invocation checked source SHA-256 pins immediately before the run:
`ProbeLauncher.cs` `4d53efa7ef419fd529dd899a6072c87f9a9cf66f0e597ecb875676e0fe11fd54`,
`ProcessContainmentProbe.cs` `05f9fe7195354d96a3e29d0d5009d915301f60a0dbcff0f29d610ee2a015dbae`,
and `Test-ProcessContainment.ps1` `c0dac0306ddfe93bc5705d0c42ceb87fa1df934a74cb4ccd5fdb81418722945e`.
Original vendor execution, restricted interactive execution and hardware
were NOT RUN at this checkpoint. These process tests do not establish
network/device isolation.

The same four owned cases subsequently passed on the same host in the
existing `dev` RDP session via a scheduled task configured as Interactive
and Limited. The test checked the limited token and pinned sources before
execution. The scheduler reported exit 0; no test process remained and the
finished task was removed. The private result
`private/evidence/task013-interactive-result-20261113.txt` has SHA-256
`1481a069a2c6df446d9a668036be2bf3a75610a9431c1e295cab91f4b910754c`.
No original DCA or hardware was exercised.
