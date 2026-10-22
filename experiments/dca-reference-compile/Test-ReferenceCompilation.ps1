#requires -Version 5.1
<# Fixed-source compile-only proof. Metadata identities are read, never execution-
load any original assembly or the output. No SDK services, lifecycle or network. #>
param(
 [Parameter(Mandatory)][string] $Compiler,
 [Parameter(Mandatory)][string] $ReferenceDirectory,
 [Parameter(Mandatory)][string] $PrivateReferenceDirectory,
 [Parameter(Mandatory)][string] $OutputDirectory
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
function Fail([string] $code){throw ('REFERENCE_COMPILE_'+$code)}
function MissingItem($record){
 return $record.FullyQualifiedErrorId -ceq 'PathNotFound' -or $record.FullyQualifiedErrorId -ceq 'PathNotFound,Microsoft.PowerShell.Commands.GetItemCommand'
}
function Local([string] $path,[bool] $new=$false){
 if($null -eq $path -or $path.Length -lt 3 -or $path.Length -gt 1024 -or $path -notmatch '^[A-Za-z]:\\' -or $path -match '["\r\n*?<>|/]' -or $path.Substring(2).Contains(':')){Fail 'LOCAL_PATH'}
 $parts=@($path.Substring(3).Split('\'));foreach($part in $parts){if($part.Length -eq 0 -or $part -in @('.','..') -or $part.EndsWith('.') -or $part.EndsWith(' ')){Fail 'PATH_COMPONENT'}}
 $p=[IO.Path]::GetFullPath($path);$root=[IO.Path]::GetPathRoot($p);$current=$root
 if(([IO.DriveInfo]::new($root)).DriveType -notin @([IO.DriveType]::Fixed,[IO.DriveType]::Removable,[IO.DriveType]::Ram)){Fail 'LOCAL_DRIVE'}
 if(((Get-Item -LiteralPath $root -Force -ErrorAction Stop).Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}
 for($i=0;$i -lt $parts.Count;$i++){
  $current=Join-Path $current $parts[$i]
  try{$item=Get-Item -LiteralPath $current -Force -ErrorAction Stop}
  catch [System.Management.Automation.ItemNotFoundException]{
   if(-not (MissingItem $_)){throw};if($new -and $i -eq $parts.Count-1){return $p};Fail 'MISSING_PATH'
  }
  if(($item.Attributes -band [IO.FileAttributes]::ReparsePoint)-ne 0){Fail 'REPARSE_PATH'}
  if($i -lt $parts.Count-1 -and ($item.Attributes -band [IO.FileAttributes]::Directory)-eq 0){Fail 'DIRECTORY_REQUIRED'}
 }
 if($new){Fail 'NEW_OUTPUT_REQUIRED'};return $p
}
function HashStream([IO.Stream] $stream){
 $stream.Position=0;$sha=[Security.Cryptography.SHA256]::Create()
 try{return ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','').ToLowerInvariant()}finally{$sha.Dispose();$stream.Position=0}
}
function HashFile([string] $path){
 $stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
 try{return HashStream $stream}finally{$stream.Dispose()}
}
function Identity([string] $path){return [Reflection.AssemblyName]::GetAssemblyName($path).FullName}
function Quote([string] $value){if($value -match '["\r\n]' -or $value.EndsWith('\')){Fail 'ARGUMENT'};return '"'+$value+'"'}

if($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitProcess){Fail 'WINDOWS_X64_REQUIRED'}
$compilerPath=Local $Compiler;$public=Local $ReferenceDirectory;$private=Local $PrivateReferenceDirectory;$output=Local $OutputDirectory $true
foreach($directory in @($public,$private)){if(-not ((Get-Item -LiteralPath $directory -Force -ErrorAction Stop).PSIsContainer)){Fail 'REFERENCE_DIRECTORY'}}
if([IO.Path]::GetFileName($compilerPath) -ine 'csc.exe' -or [Diagnostics.FileVersionInfo]::GetVersionInfo($compilerPath).FileVersion -cne '4.8.9221.0 built by: NET481REL1LAST_25H2' -or (HashFile $compilerPath) -cne '46809206887326d2d24db1eff1f3064de972c3451abe766b49111450a5e08e00'){Fail 'COMPILER_PIN'}
foreach($directory in @($public,$private,$PSScriptRoot)){
 if($output -ieq $directory -or $output.StartsWith($directory.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $directory.StartsWith($output.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase)){Fail 'OUTPUT_OVERLAP'}
}
$ancestor=[IO.Directory]::GetParent($output)
while($null -ne $ancestor){
 try{$git=Get-Item -LiteralPath (Join-Path $ancestor.FullName '.git') -Force -ErrorAction Stop;Fail 'OUTPUT_IN_GIT'}
 catch [System.Management.Automation.ItemNotFoundException]{if(-not (MissingItem $_)){throw}}
 $ancestor=$ancestor.Parent
}
$publicPins=[ordered]@{
 'mscorlib.dll'='6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9'
 'System.dll'='2fe343569f794f2ca92ee14a41875571a9f21bf92637b8f8ee86306534209cca'
 'WindowsBase.dll'='99591f58a3b54d5e38c9ca85ac2488780161da511748005c542ee7db03857081'
 'PresentationCore.dll'='7c6cf727ee790f6625537bd607d0d755db3510cfc7d83c3630ba375eb83555e3'
 'PresentationFramework.dll'='a1958e008541e47eca7828dc59d8ea60528ec235db4fdaf5222c494df1e318e2'
 'System.Xaml.dll'='f88a02dcc1dc1037884032624f3ab092aa5a3e85d215598d7d13ad7ddc814d81'
}
$privatePins=[ordered]@{
 'Knx.Ets.Sdk.dll'=@{hash='7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989';bytes=6979024;identity='Knx.Ets.Sdk, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159'}
 'Knx.Ets.Sdk.AddIns.AddInViews.dll'=@{hash='5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d';bytes=72704;identity='Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159'}
}
$handles=[Collections.Generic.List[IO.FileStream]]::new();$references=@();$source=$null;$process=$null;$started=$false;$confirmed=$false
try{
 $sourcePath=Local (Join-Path $PSScriptRoot 'OwnedReferenceContracts.cs')
 $source=[IO.File]::Open($sourcePath,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
 if($source.Length -le 0 -or $source.Length -gt 16384){Fail 'SOURCE_BOUND'}
 $sourceRecord=[ordered]@{name='OwnedReferenceContracts.cs';bytes=$source.Length;sha256=(HashStream $source)}
 # Pin every reference before any private identity metadata read or output creation.
 foreach($kind in @('public','private')){
  $pins=if($kind -eq 'public'){$publicPins}else{$privatePins};$directory=if($kind -eq 'public'){$public}else{$private}
  foreach($name in $pins.Keys){
   $path=Local (Join-Path $directory $name);$stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read);$handles.Add($stream)
   if($stream.Length -le 0 -or $stream.Length -gt 16777216){Fail 'REFERENCE_BOUND'}
   $expected=if($kind -eq 'public'){$pins[$name]}else{$pins[$name].hash}
   $hash=HashStream $stream;if($hash -cne $expected){Fail ('REFERENCE_HASH_'+$name)}
   if($kind -eq 'private' -and $stream.Length -ne $pins[$name].bytes){Fail ('REFERENCE_SIZE_'+$name)}
   $references+=@{name=$name;kind=$kind;bytes=$stream.Length;sha256=$hash;path=$path;identity=$null}
  }
 }
 foreach($reference in $references){
  $reference.identity=Identity $reference.path
  if($reference.kind -eq 'private' -and $reference.identity -cne $privatePins[$reference.name].identity){Fail ('REFERENCE_IDENTITY_'+$reference.name)}
 }
 $null=New-Item -ItemType Directory -Path $output
 $dll=Join-Path $output 'Knx.Editor.Task005.ReferenceContracts.dll'
 $arguments=@('/nologo','/noconfig','/nostdlib','/target:library','/langversion:5','/platform:x64','/warnaserror+',('/out:'+$dll))
 foreach($reference in $references){$arguments+=('/reference:'+$reference.path)};$arguments+=$sourcePath
 $process=[Diagnostics.Process]::new();$process.StartInfo.FileName=$compilerPath;$process.StartInfo.Arguments=(($arguments|ForEach-Object{Quote $_}) -join ' ')
 $process.StartInfo.WorkingDirectory=$output;$process.StartInfo.UseShellExecute=$false;$process.StartInfo.CreateNoWindow=$true
 $process.StartInfo.RedirectStandardOutput=$true;$process.StartInfo.RedirectStandardError=$true
 foreach($name in @('COR_ENABLE_PROFILING','COR_PROFILER','COR_PROFILER_PATH','COR_PROFILER_PATH_64','APPDOMAIN_MANAGER_ASM','APPDOMAIN_MANAGER_TYPE','DEVPATH','COMPLUS_Version','DOTNET_STARTUP_HOOKS')){$process.StartInfo.EnvironmentVariables.Remove($name)}
 if(-not $process.Start()){Fail 'COMPILER_START'};$started=$true
 $streams=@($process.StandardOutput.BaseStream,$process.StandardError.BaseStream)
 $buffers=@([byte[]]::new(4096),[byte[]]::new(4096));$captured=@([IO.MemoryStream]::new(),[IO.MemoryStream]::new())
 $reads=@($streams[0].ReadAsync($buffers[0],0,4096),$streams[1].ReadAsync($buffers[1],0,4096));$ended=@($false,$false);$total=0
 $clock=[Diagnostics.Stopwatch]::StartNew()
 try{
  while($true){
   for($i=0;$i -lt 2;$i++){
    if(-not $ended[$i] -and $reads[$i].IsCompleted){
     $count=$reads[$i].GetAwaiter().GetResult()
     if($count -eq 0){$ended[$i]=$true}else{
      if($count -gt 65536-$total){Fail 'COMPILER_OUTPUT_LIMIT'}
      $captured[$i].Write($buffers[$i],0,$count);$total+=$count
      $reads[$i]=$streams[$i].ReadAsync($buffers[$i],0,4096)
     }
    }
   }
   if($process.HasExited -and $ended[0] -and $ended[1]){break}
   if($clock.ElapsedMilliseconds -ge 20000){Fail 'COMPILER_TIMEOUT'}
   Start-Sleep -Milliseconds 10
  }
  $confirmed=$process.WaitForExit(2000);if(-not $confirmed){Fail 'COMPILER_EXIT_UNCONFIRMED'}
  [IO.File]::WriteAllBytes((Join-Path $output 'compiler.stdout.txt'),$captured[0].ToArray())
  [IO.File]::WriteAllBytes((Join-Path $output 'compiler.stderr.txt'),$captured[1].ToArray())
  if($process.ExitCode -ne 0 -or $captured[1].Length -ne 0){Write-Output ([Text.Encoding]::UTF8.GetString($captured[0].ToArray()));Fail ('COMPILER_EXIT_'+$process.ExitCode)}
 }finally{foreach($capture in $captured){$capture.Dispose()}}
 $dll=Local $dll;$outputIdentity=Identity $dll
 if($outputIdentity -cne 'Knx.Editor.Task005.ReferenceContracts, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'){Fail 'OUTPUT_IDENTITY'}
 $referenceRecords=@($references|ForEach-Object{[ordered]@{name=$_.name;kind=$_.kind;bytes=$_.bytes;sha256=$_.sha256;identity=$_.identity}})
 $report=[ordered]@{schema_version=1;operation='compile_only_selected_private_contracts';observed_utc=[DateTime]::UtcNow.ToString('o');result='PASS';framework='net48';platform='x64';language='CSharp5';sdk_version='6.4.8718.0';compiler_version=[Diagnostics.FileVersionInfo]::GetVersionInfo($compilerPath).FileVersion;compiler_sha256=(HashFile $compilerPath);source=$sourceRecord;references=$referenceRecords;output=[ordered]@{name=[IO.Path]::GetFileName($dll);bytes=(Get-Item -LiteralPath $dll -Force -ErrorAction Stop).Length;sha256=(HashFile $dll);identity=$outputIdentity};compiler_exit=$process.ExitCode;compiler_exit_confirmed=$confirmed;captured_bytes=$total;vendor_execution='NOT_RUN';output_execution='NOT_RUN';runtime_closure='selected startup/constructor only in accepted Task-148; later capabilities unverified';descendants='NOT_CONTAINED'}
 [IO.File]::WriteAllText((Join-Path $output 'compile-result.json'),($report|ConvertTo-Json -Depth 7),[Text.UTF8Encoding]::new($false))
 Write-Output 'REFERENCE_COMPILE PASS sdk=6.4.8718.0 private-references=2 public-references=6 target=net48/x64/CSharp5 vendor-execution=NOT_RUN output-execution=NOT_RUN'
}finally{
 if($started -and -not $confirmed){
  if(-not $process.HasExited){$process.Kill()};$confirmed=$process.WaitForExit(2000)
  Write-Output ('REFERENCE_COMPILE_CLEANUP compiler-exit-confirmed='+$confirmed)
  if(-not $confirmed){Fail 'COMPILER_EXIT_UNCONFIRMED'}
 }
 if($null -ne $process){$process.Dispose()};if($null -ne $source){$source.Dispose()}
 foreach($handle in $handles){$handle.Dispose()}
}
