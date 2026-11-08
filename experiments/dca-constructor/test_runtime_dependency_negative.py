# SPDX-License-Identifier: Apache-2.0
"""Execute the owned C# denial policy; never load SDK/vendor code or start a UI."""
from pathlib import Path
import shutil
import re
import subprocess
import tempfile
import unittest

SOURCE = Path(__file__).resolve().parent


class RuntimeDependencyNegativeTests(unittest.TestCase):
    def test_owned_denial_policy_on_exact_and_nearby_inputs(self):
        candidates = [Path(shutil.which("pwsh") or "/nonexistent-pwsh")]
        candidates.extend(parent / "private/toolchains/powershell-7.6.7/pwsh" for parent in SOURCE.parents)
        runtime = next((path for path in candidates if path.is_file()), None)
        if runtime is None:
            self.skipTest("PowerShell C# compiler/runtime unavailable; Windows original test remains NOT RUN")
        shared = (SOURCE / "ConstructorProbe.cs").read_text()
        start = shared.index("internal static class ProductRuntimeDependencyNegative")
        end = shared.index("internal static class ProbeFiles", start)
        policy = shared[start:end]
        proof = r'''
public static class Proof {
 static void Require(bool value,string name){if(!value)throw new Exception(name);}
 public static int Main(){
  string mode=ProductRuntimeDependencyNegative.Mode, flag=ProductRuntimeDependencyNegative.Flag;
  string identity=ProductRuntimeDependencyNegative.Identity;
  Require(ProductRuntimeDependencyNegative.Selected(mode,flag,"cvnext"),"EXACT_CASE");
  foreach(string wrong in new string[]{null,"owned-sdk-project-fixture",mode+"x"})
   Require(!ProductRuntimeDependencyNegative.Selected(wrong,flag,"cvnext"),"WRONG_MODE");
  Require(!ProductRuntimeDependencyNegative.Selected(mode,flag+"x","cvnext") &&
   !ProductRuntimeDependencyNegative.Selected(mode,flag,"dotnetzip"),"WRONG_FLAG_OR_CASE");
  int calls=0;
  ProductRuntimeDependencyNegative.BeforeLoad(false,identity,identity,delegate{calls++;});
  foreach(string wrong in new string[]{null,identity+".resources",identity.Replace("6.4.8718.0","6.4.8718.1"),identity.Replace("61439873ec5e1159","61439873ec5e1158")}){
   ProductRuntimeDependencyNegative.BeforeLoad(true,wrong,identity,delegate{calls++;});
   ProductRuntimeDependencyNegative.BeforeLoad(true,identity,wrong,delegate{calls++;});
  }
  Require(calls==0,"NORMAL_AND_NEARBY_NO_EFFECT");
  bool denied=false;
  try{ProductRuntimeDependencyNegative.BeforeLoad(true,identity,identity,delegate{calls++;});}
  catch(FileNotFoundException error){denied=error.Message==ProductRuntimeDependencyNegative.Failure && error.FileName==identity;}
  Require(denied && calls==1,"EXACT_NAMED_DENIAL");
  denied=false;
  try{ProductRuntimeDependencyNegative.BeforeLoad(true,identity,identity,delegate{throw new Exception("RECORDER");});}
  catch(FileNotFoundException error){denied=error.Message==ProductRuntimeDependencyNegative.Failure && error.FileName==identity;}
  Require(denied,"RECORDER_CANNOT_SUPPRESS_DENIAL");
  Console.WriteLine("OWNED_RUNTIME_DEPENDENCY_POLICY_PASS vendor=NOT_RUN CLR-resolution=NOT_RUN UI=NOT_RUN");
  return 0;
 }
}
'''
        with tempfile.TemporaryDirectory(prefix="task010-runtime-negative-", dir="/private/tmp") as tmp:
            root = Path(tmp)
            source, runner = root / "proof.cs", root / "proof.ps1"
            source.write_text("using System; using System.IO;\n" + policy + proof)
            runner.write_text("param([string]$SourcePath)\n$ErrorActionPreference='Stop'\n"
                              "Add-Type -TypeDefinition ([IO.File]::ReadAllText($SourcePath)) -Language CSharp -CompilerOptions '/langversion:5','/warnaserror+'\n"
                              "if([Proof]::Main() -ne 0){throw 'OWNED_PROOF_FAILED'}\n")
            executed = subprocess.run([str(runtime), "-NoLogo", "-NoProfile", "-NonInteractive", "-File", str(runner), str(source)], capture_output=True, text=True, timeout=30)
            self.assertEqual(executed.returncode, 0, executed.stdout + executed.stderr)
            self.assertIn("OWNED_RUNTIME_DEPENDENCY_POLICY_PASS", executed.stdout)

    def test_loader_denial_is_after_profile_validation_and_before_assembly_load(self):
        source = (SOURCE / "ProjectConstructorProbe.cs").read_text()
        shared = (SOURCE / "ConstructorProbe.cs").read_text()
        original = (SOURCE / "OriginalProductImportProbe.cs").read_text()
        pinned = re.search(r'internal const string CvNextIdentity = "([^"]+)";', original).group(1)
        selected = re.search(r'internal const string Identity = "([^"]+)";', shared).group(1)
        self.assertEqual(selected, pinned)
        start = source.index("    static Assembly ResolveProductDependency(")
        end = source.index("    static Assembly ResolveProductLoggingLegacy(", start)
        body = source[start:end]
        self.assertLess(body.index('"PRODUCT_IMPORT_DEPENDENCY_EXACT_PROFILE"'), body.index("ProductRuntimeDependencyNegative.BeforeLoad("))
        self.assertLess(body.index("ProductRuntimeDependencyNegative.BeforeLoad("), body.index("Assembly.LoadFrom("))
        self.assertIn("rejected=true;", body)
        self.assertIn("loader=EXPLICIT_IMPORT_LOADER assembly-load=NOT_RUN ui=NOT_STARTED", body)
        construction = source.index("sdkProductImport=new OriginalProductImportProbe(")
        guard = source.index('ProbeFiles.Require(!runtimeDependencyNegative,"PRODUCT_RUNTIME_DEPENDENCY_NEGATIVE_NOT_REACHED")', construction)
        self.assertLess(guard, source.index('Record("DCA_UI_BEGIN', construction))
        self.assertIn('ProductRuntimeDependencyNegative.Selected(mode,args[6],args[7])', source)
        self.assertIn('runtimeDependencyNegative=true;', source)
        launcher = (SOURCE / "ProbeLauncher.cs").read_text()
        self.assertIn('ProductRuntimeDependencyNegative.Selected(args[3],args[8],args[9])', launcher)
        self.assertIn('!originalHostBus && !runtimeDependencyNegative;', launcher)
        self.assertIn('if(runtimeDependencyNegative)child.StartInfo.Arguments+=" --runtime-dependency-negative cvnext";', launcher)


if __name__ == "__main__":
    unittest.main()
