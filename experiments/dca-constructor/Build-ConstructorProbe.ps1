#requires -Version 5.1
<# Owned C#5/net48/x64 build only. No vendor input, restore, install or network. #>
param(
 [Parameter(Mandatory)][string] $Compiler,
 [Parameter(Mandatory)][string] $ReferenceDirectory,
 [Parameter(Mandatory)][string] $OutputDirectory
)
$ErrorActionPreference='Stop'; Set-StrictMode -Version Latest
function Fail([string] $code){throw ('CONSTRUCTOR_BUILD_'+$code)}
function Local([string] $path,[bool] $new=$false){
 if($path.Length -gt 1024 -or $path -notmatch '^[A-Za-z]:\\' -or $path -match '["\r\n*?<>|/]' -or $path.Substring(2).Contains(':')){Fail 'LOCAL_PATH'}
 $parts=@($path.Substring(3).Split('\')); foreach($part in $parts){if($part.Length -eq 0 -or $part -in @('.','..') -or $part.EndsWith('.') -or $part.EndsWith(' ')){Fail 'PATH_COMPONENT'}}
 $p=[IO.Path]::GetFullPath($path);$root=[IO.Path]::GetPathRoot($p);$current=$root
 if(([IO.DriveInfo]::new($root)).DriveType -notin @([IO.DriveType]::Fixed,[IO.DriveType]::Removable,[IO.DriveType]::Ram)){Fail 'LOCAL_DRIVE'}
 if(((Get-Item -LiteralPath $root -Force -ErrorAction Stop).Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}
 for($i=0;$i -lt $parts.Count;$i++){
  $current=Join-Path $current $parts[$i]
  # Read this component's metadata before any existence test or descent.
  try{$item=Get-Item -LiteralPath $current -Force -ErrorAction Stop}
  catch [System.Management.Automation.ItemNotFoundException]{
   if($_.FullyQualifiedErrorId -cne 'PathNotFound' -and $_.FullyQualifiedErrorId -cne 'PathNotFound,Microsoft.PowerShell.Commands.GetItemCommand'){throw}
   if($new -and $i -eq $parts.Count-1){return $p};Fail 'MISSING_PATH'
  }
  if(($item.Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}
  if($i -lt $parts.Count-1 -and ($item.Attributes -band [IO.FileAttributes]::Directory)-eq 0){Fail 'DIRECTORY_REQUIRED'}
 }
 if($new){Fail 'NEW_OUTPUT_REQUIRED'}
 return $p
}
function Hash([string] $path){return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}
function Compile([string[]] $arguments){
 $p=[Diagnostics.Process]::new();$p.StartInfo.FileName=$compilerPath
 $p.StartInfo.Arguments=(($arguments|ForEach-Object{if($_ -match '["\r\n]' -or $_.EndsWith('\')){Fail 'ARGUMENT'};'"'+$_+'"'}) -join ' ')
 $p.StartInfo.UseShellExecute=$false;$p.StartInfo.CreateNoWindow=$true;$p.StartInfo.RedirectStandardOutput=$true;$p.StartInfo.RedirectStandardError=$true
 foreach($name in @('COR_ENABLE_PROFILING','COR_PROFILER','COR_PROFILER_PATH','COR_PROFILER_PATH_64','APPDOMAIN_MANAGER_ASM','APPDOMAIN_MANAGER_TYPE','DEVPATH','COMPLUS_Version','DOTNET_STARTUP_HOOKS')){$p.StartInfo.EnvironmentVariables.Remove($name)}
 try{
  if(-not $p.Start()){Fail 'START'};$stdout=$p.StandardOutput.ReadToEndAsync();$stderr=$p.StandardError.ReadToEndAsync()
  if(-not $p.WaitForExit(20000)){$p.Kill();if(-not $p.WaitForExit(2000)){Fail 'COMPILER_EXIT_UNCONFIRMED'};Fail 'COMPILER_TIMEOUT'}
  $o=$stdout.Result;$e=$stderr.Result;if($o.Length+$e.Length -gt 65536){Fail 'COMPILER_OUTPUT_LIMIT'}
  if($p.ExitCode -ne 0 -or $e.Length -gt 0){Write-Output $o;Write-Output $e;Fail 'COMPILATION'}
 }finally{$p.Dispose()}
}
if($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitProcess){Fail 'WINDOWS_X64_REQUIRED'}
$compilerPath=Local $Compiler;$refs=Local $ReferenceDirectory;$output=Local $OutputDirectory $true
if([IO.Path]::GetFileName($compilerPath) -ine 'csc.exe' -or [Diagnostics.FileVersionInfo]::GetVersionInfo($compilerPath).FileVersion -cne '4.8.9221.0 built by: NET481REL1LAST_25H2' -or (Hash $compilerPath) -cne '46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00'){Fail 'COMPILER_PIN'}
$current=[IO.Directory]::GetParent($output);while($null -ne $current){if(Test-Path -LiteralPath (Join-Path $current.FullName '.git')){Fail 'OUTPUT_IN_GIT'};$current=$current.Parent}
$sources=@('ConstructorProbe.cs','ProbeLauncher.cs','OwnedConstructorFixtures.cs','InitializeContextProbe.cs','OwnedInitializationFixtures.cs','ProjectConstructorProbe.cs','OwnedProjectConstructorFixtures.cs')|ForEach-Object{Local (Join-Path $PSScriptRoot $_)}
$neutral=Local (Join-Path ([IO.Directory]::GetParent($PSScriptRoot).FullName) 'dca-integration\NeutralStaHost.cs')
if((Hash $neutral) -cne '7f3045efbc29efe73a743c47724df57c19fa3cc285556e62d850412baa70863c'){Fail 'NEUTRAL_SOURCE_PIN'}
foreach($path in @($refs,$PSScriptRoot)){
 if($output.StartsWith($path.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $path.StartsWith($output.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $output -ieq $path){Fail 'OUTPUT_OVERLAP'}
}
$pins=[ordered]@{
 'mscorlib.dll'='6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9'
 'System.dll'='2fe343569f794f2ca92ee14a41875571a9f21bf92637b8f8ee86306534209cca'
 'WindowsBase.dll'='99591f58a3b54d5e38c9ca85ac2488780161da511748005c542ee7db03857081'
 'PresentationCore.dll'='7c6cf727ee790f6625537bd607d0d755db3510cfc7d83c3630ba375eb83555e3'
 'PresentationFramework.dll'='a1958e008541e47eca7828dc59d8ea60528ec235db4fdaf5222c494df1e318e2'
 'System.Xaml.dll'='f88a02dcc1dc1037884032624f3ab092aa5a3e85d215598d7d13ad7ddc814d81'
}
$common=@('/nologo','/noconfig','/nostdlib','/langversion:5','/platform:x64','/warnaserror+')
foreach($name in $pins.Keys){$path=Local (Join-Path $refs $name);if((Hash $path) -cne $pins[$name]){Fail 'REFERENCE_PIN'};$common+=('/reference:'+$path)}
$null=New-Item -ItemType Directory -Path $output
$bin=Join-Path $output 'bin';$fixtures=Join-Path $output 'fixtures';$null=New-Item -ItemType Directory -Path $bin,$fixtures
Compile ($common+@('/target:library',('/out:'+(Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll')),$sources[2],$sources[4],$sources[6]))
Compile ($common+@('/target:exe','/main:ConstructorProbe',('/out:'+(Join-Path $bin 'ConstructorProbe.exe')),$sources[0],$sources[3]))
Compile ($common+@('/target:exe','/main:ProbeLauncher',('/out:'+(Join-Path $bin 'ProbeLauncher.exe')),$sources[0],$sources[1],$sources[3]))
Compile ($common+@('/target:exe','/main:ProjectConstructorProbe',('/out:'+(Join-Path $bin 'ProjectConstructorProbe.exe')),$sources[0],$sources[3],$sources[5]))
Compile ($common+@('/target:exe','/main:ProjectConstructorProbe',('/out:'+(Join-Path $bin 'ProjectFactoryBindingProbe.exe')),$sources[0],$sources[3],$sources[5]))
Compile ($common+@('/target:exe',('/out:'+(Join-Path $bin 'NeutralStaHost.exe')),$neutral))
$utf8=[Text.UTF8Encoding]::new($false)
$config='<configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/></startup><runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">'+
 '<dependentAssembly><assemblyIdentity name="Knx.Ets.Sdk" publicKeyToken="61439873ec5e1159" culture="neutral"/><bindingRedirect oldVersion="5.7.554.37315" newVersion="6.4.8718.0"/></dependentAssembly>'+
 '<dependentAssembly><assemblyIdentity name="Knx.Ets.Sdk.AddIns.AddInViews" publicKeyToken="61439873ec5e1159" culture="neutral"/><bindingRedirect oldVersion="5.7.554.37315" newVersion="6.4.8718.0"/></dependentAssembly>'+
 '<dependentAssembly><assemblyIdentity name="Knx.Ets.Common.Types" publicKeyToken="61439873ec5e1159" culture="neutral"/><bindingRedirect oldVersion="5.7.286.36952" newVersion="6.4.8718.0"/></dependentAssembly>'+
 '<dependentAssembly><assemblyIdentity name="System.Threading.Tasks.Extensions" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="4.2.0.1" newVersion="4.2.1.0"/></dependentAssembly>'+
 '</assemblyBinding></runtime></configuration>'
[IO.File]::WriteAllText((Join-Path $bin 'ConstructorProbe.exe.config'),$config,$utf8)
[IO.File]::WriteAllText((Join-Path $bin 'ProjectConstructorProbe.exe.config'),$config,$utf8)
$factoryConfig=$config.Replace('</assemblyBinding>','<dependentAssembly><assemblyIdentity name="Microsoft.Bcl.AsyncInterfaces" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="8.0.0.0" newVersion="9.0.0.0"/></dependentAssembly></assemblyBinding>')
[IO.File]::WriteAllText((Join-Path $bin 'ProjectFactoryBindingProbe.exe.config'),$factoryConfig,$utf8)
[IO.File]::WriteAllText((Join-Path $bin 'ProjectFactoryBindingProbe.exe.sha256'),(Hash (Join-Path $bin 'ProjectFactoryBindingProbe.exe')),$utf8)
foreach($path in @((Join-Path $bin 'ConstructorProbe.exe'),(Join-Path $bin 'ProjectConstructorProbe.exe'),(Join-Path $bin 'ProbeLauncher.exe'),(Join-Path $bin 'NeutralStaHost.exe'),(Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll'))){[IO.File]::WriteAllText(($path+'.sha256'),(Hash $path),$utf8)}
$report=[ordered]@{compilerVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($compilerPath).FileVersion;compilerSha256=(Hash $compilerPath);vendor='NOT_RUN';sources=@();artifacts=@()}
foreach($path in @($sources)+@($neutral)){$report.sources+=@{name=[IO.Path]::GetFileName($path);sha256=(Hash $path)}}
foreach($path in Get-ChildItem -LiteralPath $bin,$fixtures -File){$report.artifacts+=@{name=$path.Name;bytes=$path.Length;sha256=(Hash $path.FullName)}}
[IO.File]::WriteAllText((Join-Path $output 'build-report.json'),($report|ConvertTo-Json -Depth 5),$utf8)
Write-Output 'CONSTRUCTOR_BUILD PASS target=net48 language=CSharp5 process=x64 vendor=NOT_RUN hardware=NOT_RUN'
