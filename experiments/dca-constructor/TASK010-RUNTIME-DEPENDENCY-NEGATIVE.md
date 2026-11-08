# Task010 late explicit import dependency denial

Base: editor `3115765ec4a13ff51ec58a51ce5bccef6975c857`. Task009 and
Task011 are accepted predecessors; Task010 has no phase gate. This increment
prepares one missing-runtime-dependency negative. It does not close Task010.

## Observed route and test boundary

The existing private successful Windows log
`private/evidence/task010-sdk-wrapper/iteration01/run-dca012a13301/original.log`
records DotNetZip loading directly from the staged directory, followed by an
after-load audit. An `AssemblyResolve` handler cannot deny a dependency that
the CLR already found through normal LoadFrom probing. No DotNetZip resolver
interception or isolation is claimed here.

The existing product import explicitly calls `LoadProductImportConverter`,
which invokes the pinned `ResolveProductDependency` path for
`Knx.Ets.CvNext, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159`.
This is a genuine late import dependency, but an explicit owned import loader,
not an observed CLR resolution callback. The selected original dependency hash
and input requirements remain unchanged.

The finite diagnostic option `--runtime-dependency-negative cvnext` is accepted
only with `original-sdk-product-store-fixture`. After the normal exact-profile
check, and before `Assembly.LoadFrom`, it latches rejection, records the exact
identity and throws `FileNotFoundException` with code
`PRODUCT_RUNTIME_DEPENDENCY_DENIED`. The named failure remains even if the
recorder fails. It never returns an alternative assembly or UI. If import
returns without reaching this exact loader, the negative fails with
`PRODUCT_RUNTIME_DEPENDENCY_NEGATIVE_NOT_REACHED` before the DCA UI path.
Without the option, the existing loading path is unchanged.

## Checks actually executed

On macOS arm64:

- `python3 -m unittest discover -s experiments/dca-constructor -p test_runtime_dependency_negative.py`:
  PASS, two tests. The first compiles and runs the extracted owned policy with
  the local PowerShell/Roslyn C#5 compiler: normal mode, near identities,
  version/token mismatch, exact named denial and recorder failure. The second
  checks source wiring, pin consistency, denial ordering and the pre-UI guard.
- The existing private `compile-product-import03.ps1` was run with the assigned
  worktree and the pinned references from
  `private/evidence/task148-clean-host/references`: PASS for full host, launcher
  and owned fixtures compilation with C#5/.NET48 references; two PowerShell
  files parsed. This is compilation only, not Windows runtime evidence.
- `git diff --check`: PASS.

The first local policy-test attempt used the installed Mono binary, which
failed with `Bad CPU type in executable` before compilation. It was replaced
by the available native PowerShell/Roslyn path. A separate attempted Add-Type
full-host compile could not locate the reference metadata correctly; the
existing descriptor-based Roslyn compilation with exact reference pins then
passed. Neither failed tool attempt ran the host or vendor code.

## Required later Windows result

A fresh, qualified offline Windows session and the exact pinned dependency set
are still required. A later explicit original test may select the finite flag
through the existing supervised launcher. It must show the named CvNext denial
and its exact dependency identity, exit with failure, contain no
`DCA_UI_BEGIN`, no successful project/UI result, and no replacement control.
A pre-admission missing-input error or `NEGATIVE_NOT_REACHED` is a failed test,
not the desired runtime-dependency result.

Original SDK/vendor runtime, actual Windows loader behavior, UI absence after
an actual vendor failure, host/network isolation and hardware: NOT RUN.
No Windows host, ETS project, bus interface or manufacturer binary was accessed
or modified by this increment. Task010 acceptance and gates remain open.
