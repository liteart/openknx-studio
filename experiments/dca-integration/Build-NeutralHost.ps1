#requires -Version 5.1
<# Build only the owned Task-144 neutral net48/x64 WPF shell. No restore/install,
SDK/vendor reference or load, hardware, ETS process or runtime qualification.
Uses Task-003's five pinned net48 reference-pack files and inventoried compiler. #>
param(
 [Parameter(Mandatory)][string] $Compiler,
 [Parameter(Mandatory)][string] $ReferenceDirectory,
 [Parameter(Mandatory)][string] $OutputDirectory
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
function Fail([string] $code){throw ('NEUTRAL_BUILD_'+$code)}
function Local([string] $path,[bool] $missing=$false){
 if($path -notmatch '^[A-Za-z]:\\' -or $path -match '["\r\n]'){Fail 'LOCAL_PATH'}
 $p=[IO.Path]::GetFullPath($path);$root=[IO.Path]::GetPathRoot($p)
 if(([IO.DriveInfo]::new($root)).DriveType -notin @([IO.DriveType]::Fixed,[IO.DriveType]::Removable,[IO.DriveType]::Ram)){Fail 'LOCAL_DRIVE'}
 $current=$root;$parts=@($p.Substring($root.Length).Split('\')|Where-Object { $_.Length -gt 0 })
 if(((Get-Item -LiteralPath $root -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}
 for($i=0;$i -lt $parts.Count;$i++){
  $current=Join-Path $current $parts[$i]
  if(Test-Path -LiteralPath $current){if(((Get-Item -LiteralPath $current -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}}elseif(-not ($missing -and $i -eq $parts.Count-1)){Fail 'MISSING_PATH'}
 }
 return $p
}
if($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitProcess){Fail 'WINDOWS_X64_REQUIRED'}
$compilerPath=Local $Compiler;$refs=Local $ReferenceDirectory;$output=Local $OutputDirectory $true
if([IO.Path]::GetFileName($compilerPath) -ine 'csc.exe' -or [Diagnostics.FileVersionInfo]::GetVersionInfo($compilerPath).FileVersion -ne '5.900.26.35703'){Fail 'COMPILER_VERSION'}
if(Test-Path -LiteralPath $output){Fail 'NEW_OUTPUT_REQUIRED'}
$current=[IO.Directory]::GetParent($output);while($null -ne $current){if(Test-Path -LiteralPath (Join-Path $current.FullName '.git')){Fail 'OUTPUT_IN_GIT'};$current=$current.Parent}
$pins=[ordered]@{
 'mscorlib.dll'='6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9'
 'System.dll'='2fe343569f794f2ca92ee14a41875571a9f21bf92637b8f8ee86306534209cca'
 'WindowsBase.dll'='99591f58a3b54d5e38c9ca85ac2488780161da511748005c542ee7db03857081'
 'PresentationCore.dll'='7c6cf727ee790f6625537bd607d0d755db3510cfc7d83c3630ba375eb83555e3'
 'PresentationFramework.dll'='a1958e008541e47eca7828dc59d8ea60528ec235db4fdaf5222c494df1e318e2'
}
$arguments=@('/nologo','/noconfig','/nostdlib','/platform:x64','/target:exe','/warnaserror+')
foreach($name in $pins.Keys){$path=Local (Join-Path $refs $name);if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $pins[$name]){Fail 'REFERENCE_HASH'};$arguments+=('/reference:'+$path)}
$null=New-Item -ItemType Directory -Path $output
$exe=Join-Path $output 'NeutralStaHost.exe';$source=Local (Join-Path $PSScriptRoot 'NeutralStaHost.cs')
$arguments+=('/out:'+$exe);$arguments+=$source
$p=[Diagnostics.Process]::new();$p.StartInfo.FileName=$compilerPath
$p.StartInfo.Arguments=(($arguments|ForEach-Object{if($_ -match '["\r\n]'){Fail 'ARGUMENT'};'"'+$_+'"'}) -join ' ')
$p.StartInfo.UseShellExecute=$false;$p.StartInfo.CreateNoWindow=$true;$p.StartInfo.RedirectStandardOutput=$true;$p.StartInfo.RedirectStandardError=$true
try{
 if(-not $p.Start()){Fail 'START'};$stdout=$p.StandardOutput.ReadToEndAsync();$stderr=$p.StandardError.ReadToEndAsync()
 if(-not $p.WaitForExit(20000)){$p.Kill();$null=$p.WaitForExit(2000);Fail 'COMPILER_TIMEOUT'}
 $o=$stdout.Result;$e=$stderr.Result;if($o.Length+$e.Length -gt 65536){Fail 'OUTPUT_LIMIT'}
 if($p.ExitCode -ne 0 -or $e.Length -gt 0){Write-Output $o;Fail 'COMPILATION'}
 Write-Output ('NEUTRAL_BUILD PASS source-sha256='+(Get-FileHash $source -Algorithm SHA256).Hash.ToLowerInvariant()+' executable-sha256='+(Get-FileHash $exe -Algorithm SHA256).Hash.ToLowerInvariant())
}finally{$p.Dispose()}
