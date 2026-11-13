#requires -Version 5.1
# SPDX-License-Identifier: Apache-2.0
# Owned native Windows job tests; no vendor files, network or device use.
param(
 [Parameter(Mandatory)][string]$Compiler,
 [Parameter(Mandatory)][string]$ReferenceDirectory,
 [Parameter(Mandatory)][string]$OutputDirectory
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if($env:OS-ne'Windows_NT'-or-not[Environment]::Is64BitProcess){throw 'WINDOWS_X64_REQUIRED'}
if(Test-Path -LiteralPath $OutputDirectory){throw 'FRESH_OUTPUT_REQUIRED'}
if((Get-FileHash -LiteralPath $Compiler -Algorithm SHA256).Hash.ToLowerInvariant()-cne'46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00'){throw 'COMPILER_PIN'}
$references=@{
 'mscorlib.dll'='6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9'
 'System.dll'='2fe343569f794f2ca92ee14a41875571a9f21bf92637b8f8ee86306534209cca'
 'WindowsBase.dll'='99591f58a3b54d5e38c9ca85ac2488780161da511748005c542ee7db03857081'
 'PresentationCore.dll'='7c6cf727ee790f6625537bd607d0d755db3510cfc7d83c3630ba375eb83555e3'
 'PresentationFramework.dll'='a1958e008541e47eca7828dc59d8ea60528ec235db4fdaf5222c494df1e318e2'
 'System.Xaml.dll'='f88a02dcc1dc1037884032624f3ab092aa5a3e85d215598d7d13ad7ddc814d81'
}
$arguments=@('/nologo','/noconfig','/nostdlib','/langversion:5','/platform:x64','/warnaserror+','/target:exe','/main:ProcessContainmentProbe')
foreach($name in $references.Keys){
 $path=Join-Path $ReferenceDirectory $name
 if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()-cne$references[$name]){throw 'REFERENCE_PIN'}
 $arguments+=('/reference:'+$path)
}
$null=New-Item -ItemType Directory -Path $OutputDirectory
$exe=Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) 'ProcessContainmentProbe.exe'
$arguments+=('/out:'+$exe)
foreach($name in @('ConstructorProbe.cs','InitializeContextProbe.cs','ProbeLauncher.cs','ProcessContainmentProbe.cs')){$arguments+=(Join-Path $PSScriptRoot $name)}
foreach($argument in $arguments){if($argument-match'["\r\n]' -or $argument.EndsWith('\')){throw 'ARGUMENT_SHAPE'}}
function RunBounded([string]$path,[string]$argumentText,[int]$milliseconds){
 $p=[Diagnostics.Process]::new();$p.StartInfo.FileName=$path;$p.StartInfo.Arguments=$argumentText;$p.StartInfo.UseShellExecute=$false
 $p.StartInfo.RedirectStandardOutput=$true;$p.StartInfo.RedirectStandardError=$true
 try{
  if(-not$p.Start()){throw 'START_FAILED'};$out=$p.StandardOutput.ReadToEndAsync();$err=$p.StandardError.ReadToEndAsync()
  if(-not$p.WaitForExit($milliseconds)){
   # The test roots retain kill-on-close jobs; killing a test root also kills its host.
   # This outer watchdog must not be interpreted as confirmed descendant cleanup.
   $p.Kill();$null=$p.WaitForExit(2000);throw 'TEST_TIMEOUT_INSPECT_CHILDREN_NO_RETRY'
  }
  $stdout=$out.Result;$stderr=$err.Result
  if($stdout.Length+$stderr.Length-gt65536){throw 'OUTPUT_BOUND'}
  [IO.File]::WriteAllText((Join-Path $OutputDirectory ($path.Split('\')[-1]+'.stdout.txt')),$stdout)
  [IO.File]::WriteAllText((Join-Path $OutputDirectory ($path.Split('\')[-1]+'.stderr.txt')),$stderr)
  Write-Output $stdout
  if($p.ExitCode-ne0-or$stderr.Length-ne0){throw 'OWNED_JOB_TEST_FAILED'}
 }finally{$p.Dispose()}
}
RunBounded $Compiler (($arguments|ForEach-Object{'"'+$_+'"'})-join' ') 20000
RunBounded $exe '' 30000
Write-Output 'OWNED_JOB_NATIVE_COMPLETE vendor=NOT_RUN hardware=NOT_RUN'
