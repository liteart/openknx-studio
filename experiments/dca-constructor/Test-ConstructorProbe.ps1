#requires -Version 5.1
<# Owned tests only. The coordinator runs this in the stated Windows session.
No original inputs, vendor staging, scheduled task creation, network or hardware. #>
param(
 [Parameter(Mandatory)][string] $BuildDirectory,
 [Parameter(Mandatory)][string] $OutputDirectory,
 [Parameter(Mandatory)][ValidateSet('Session0','Interactive')][string] $ExpectedSession
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
function Fail([string] $code){throw ('CONSTRUCTOR_TEST_'+$code)}
function Local([string] $path,[bool] $new=$false){
 if($path.Length -gt 1024 -or $path -notmatch '^[A-Za-z]:\\' -or $path -match '["\r\n*?<>|/]' -or $path.Substring(2).Contains(':')){Fail 'LOCAL_PATH'}
 $parts=@($path.Substring(3).Split('\'));foreach($part in $parts){if($part.Length -eq 0 -or $part -in @('.','..') -or $part.EndsWith('.') -or $part.EndsWith(' ')){Fail 'PATH_COMPONENT'}}
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
 if($new){Fail 'NEW_OUTPUT_REQUIRED'};return $p
}
function Run([string] $name,[string] $exe,[string[]] $arguments,[int] $expected,[string] $marker,[string] $logMarker=''){
 $p=[Diagnostics.Process]::new();$p.StartInfo.FileName=$exe
 $p.StartInfo.Arguments=(($arguments|ForEach-Object{if($_ -match '["\r\n]' -or $_.EndsWith('\')){Fail 'ARGUMENT'};'"'+$_+'"'}) -join ' ')
 $p.StartInfo.UseShellExecute=$false;$p.StartInfo.CreateNoWindow=$false;$p.StartInfo.RedirectStandardOutput=$true;$p.StartInfo.RedirectStandardError=$true
 try{
  if(-not $p.Start()){Fail 'START'};$stdout=$p.StandardOutput.ReadToEndAsync();$stderr=$p.StandardError.ReadToEndAsync()
  if(-not $p.WaitForExit(45000)){$p.Kill();if(-not $p.WaitForExit(2000)){Fail 'DRIVER_EXIT_UNCONFIRMED'};Fail 'DRIVER_TIMEOUT'}
  $o=$stdout.Result;$e=$stderr.Result;if($o.Length+$e.Length -gt 90000){Fail 'OWNED_DRIVER_OUTPUT_BOUND'}
  [IO.File]::WriteAllText((Join-Path $output ($name+'.stdout.txt')),$o,$utf8);[IO.File]::WriteAllText((Join-Path $output ($name+'.stderr.txt')),$e,$utf8)
  if($p.ExitCode -ne $expected -or $e.Length -ne 0 -or -not $o.Contains($marker)){Write-Output $o;Fail ('CASE_'+$name+'_exit_'+$p.ExitCode)}
  if($logMarker.Length -gt 0){$log=$arguments[$arguments.Length-1];if(-not (Test-Path -LiteralPath $log) -or (Get-Item -LiteralPath $log).Length -gt 65536 -or -not ([IO.File]::ReadAllText($log)).Contains($logMarker)){Fail ('LOG_'+$name)}}
  Write-Output ('OWNED_CASE PASS '+$name+' exit='+$expected)
 }finally{$p.Dispose()}
}
function Case([string] $name,[string] $mode,[int] $expected,[string] $marker,[string] $logMarker='', [string] $inputPath=$fixtures,[int] $seconds=10){
 Run $name $launcher @('--seconds',([string]$seconds),'--mode',$mode,'--input',$inputPath,'--log',(Join-Path $output ($name+'.log'))) $expected $marker $logMarker
}
if($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitProcess){Fail 'WINDOWS_X64_REQUIRED'}
$session=[Diagnostics.Process]::GetCurrentProcess().SessionId;$interactive=[Environment]::UserInteractive -and $session -ne 0
$identity=[Security.Principal.WindowsIdentity]::GetCurrent();try{$administrator=([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)}finally{$identity.Dispose()}
if(($ExpectedSession -eq 'Interactive') -ne $interactive){Fail 'SESSION_EXPECTATION'}
$build=Local $BuildDirectory;$output=Local $OutputDirectory $true
if($output.StartsWith($build.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $build.StartsWith($output.TrimEnd('\')+'\',[StringComparison]::OrdinalIgnoreCase) -or $build -ieq $output){Fail 'OUTPUT_OVERLAP'}
$current=[IO.Directory]::GetParent($output);while($null -ne $current){if(Test-Path -LiteralPath (Join-Path $current.FullName '.git')){Fail 'OUTPUT_IN_GIT'};$current=$current.Parent}
$launcher=Local (Join-Path $build 'bin\ProbeLauncher.exe');$fixtures=Local (Join-Path $build 'fixtures')
$null=New-Item -ItemType Directory -Path $output;$utf8=[Text.UTF8Encoding]::new($false)
Write-Output ('OWNED_SUITE_START session='+$session+' interactive='+$interactive+' administrator-role='+$administrator+' original=NOT_RUN')
# These are source invariants for the fixed original metadata route; no original
# input is opened by the owned tests. Both hashes must precede either identity.
$projectSource=[IO.File]::ReadAllText((Local (Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs')))
$pinMatch=[regex]::Match($projectSource,'(?s)static void PinEnumInputs\(\)\s*\{(.*?)\n    \}')
if(-not $pinMatch.Success){Fail 'ENUM_PIN_SOURCE'}
$pinBody=$pinMatch.Groups[1].Value
$firstPin=$pinBody.IndexOf('Pin(common, CommonHash);',[StringComparison]::Ordinal)
$secondPin=$pinBody.IndexOf('Pin(facade, Hashes[6]);',[StringComparison]::Ordinal)
$firstIdentity=$pinBody.IndexOf('AssemblyName.GetAssemblyName(common)',[StringComparison]::Ordinal)
$secondIdentity=$pinBody.IndexOf('AssemblyName.GetAssemblyName(facade)',[StringComparison]::Ordinal)
$complete=$pinBody.IndexOf('enumInputsPinned = true;',[StringComparison]::Ordinal)
if(-not $pinBody.Contains('CountInputs(2);') -or $firstPin -lt 0 -or $secondPin -le $firstPin -or $firstIdentity -le $secondPin -or $secondIdentity -le $firstIdentity -or $complete -le $secondIdentity -or $pinBody.Contains('LoadFrom(')){Fail 'ENUM_PIN_ORDER'}
if(-not $projectSource.Contains('if (EnumOnly && enumInputsPinned && args.Name == Identities[6])') -or -not $projectSource.Contains('Assembly.ReflectionOnlyLoadFrom(Path.Combine(stage, "netstandard.dll"))')){Fail 'ENUM_FIXED_RESOLVER_SOURCE'}
foreach($literal in @('static readonly bool OriginalConstructorReady = true;','const string ReviewedLiteral = "ThreeLevel";','const string ReviewedRawValue = "1";','static readonly int ReviewedLiteralToken = 0x04000176;','static readonly int ReviewedEnumTypeToken = 0x0200008e;','const string ReviewedUnderlyingType = "System.Byte";','const string ReviewedEnumReportSha256 = "59323bf7eb53d6c6a130c523e49d82bb3e3d58830f43dcc2f13b2dcabb58bcf7";')){if(-not $projectSource.Contains($literal)){Fail 'REVIEWED_LITERAL_SOURCE'}}
$originalMatch=[regex]::Match($projectSource,'(?s)else if \(Original\)\s*\{(.*?)\n            \} else')
if(-not $originalMatch.Success){Fail 'ORIGINAL_PIN_SOURCE'}
$originalBody=$originalMatch.Groups[1].Value
$countTwelve=$originalBody.IndexOf('CountInputs(12);',[StringComparison]::Ordinal)
$pinTwelve=$originalBody.IndexOf('for (int i = 0; i < Names.Length; i++) Pin(',[StringComparison]::Ordinal)
$platformPreflight=$originalBody.IndexOf('PinPlatformFacade();',[StringComparison]::Ordinal)
$identityTwelve=$originalBody.IndexOf('for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(',[StringComparison]::Ordinal)
if($countTwelve -lt 0 -or $pinTwelve -le $countTwelve -or $platformPreflight -le $pinTwelve -or $identityTwelve -le $platformPreflight -or $originalBody.Contains('LoadFrom(')){Fail 'ORIGINAL_PIN_ORDER'}
foreach($pin in @('"FluentResults"','"99adb97bc209d19696c9a42db8eceef49b55fc2c3f4cc6d6f5654fe7e5df9159"','"FluentResults, Version=3.16.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"','"Knx.Ets.Common"','"4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd"','"Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159"','"Knx.Ets.Ids" };','"43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877" };','"Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159" };')){if(-not $projectSource.Contains($pin)){Fail 'PRIVATE_PIN_SOURCE'}}
$platformMatch=[regex]::Match($projectSource,'(?s)static void PinPlatformFacade\(\)\s*\{(.*?)\n    \}')
if(-not $platformMatch.Success){Fail 'PLATFORM_PIN_SOURCE'}
$platformBody=$platformMatch.Groups[1].Value
$platformHash=$platformBody.IndexOf('ProbeFiles.Hash(stream) == Hashes[6]',[StringComparison]::Ordinal)
$platformIdentity=$platformBody.IndexOf('AssemblyName.GetAssemblyName(platformFacadePath).FullName == Identities[6]',[StringComparison]::Ordinal)
$platformComplete=$platformBody.IndexOf('platformFacadePinned = true;',[StringComparison]::Ordinal)
if($platformHash -lt 0 -or $platformIdentity -le $platformHash -or $platformComplete -le $platformIdentity -or -not $platformBody.Contains('FileShare.Read') -or -not $platformBody.Contains('stream.Length == 102856') -or -not $platformBody.Contains('OWNED_PLATFORM_FACADE_INCOMPLETE_PIN') -or $platformBody.Contains('Record("PIN ')){Fail 'PLATFORM_PIN_ORDER'}
foreach($guard in @('const string PlatformFacadeRelativePath = "Microsoft.Net\\assembly\\GAC_MSIL\\netstandard\\v4.0_2.0.0.0__cc7b13ffcd2ddd51\\netstandard.dll";','return (Original || OwnedProject) && platformFacadePinned && identity == Identities[6]','ProbeFiles.Same(location, platformFacadePath) && cache && !reflectionOnly;','assembly.GetName().Name == "netstandard"','" reflection-only=" + assembly.ReflectionOnly','" pinned-file-sha256=" + (platform ? Hashes[6] : "NOT_ESTABLISHED")')){if(-not $projectSource.Contains($guard)){Fail 'PLATFORM_ALIAS_SOURCE'}}
if([regex]::Matches($projectSource,'platformFacadePinned\s*=').Count -ne 1 -or $pinBody.Contains('PinPlatformFacade')){Fail 'PLATFORM_PIN_BOUNDARY'}
Write-Output 'OWNED_CASE PASS enum-source-guards two-hashes-before-identities=true resolver=fixed-reflection-only reviewed-literal=ThreeLevel-byte-1 twelve-hashes-before-platform-and-identities=true platform=exact-held-pin private-common-ids=exact-pins'
foreach($guard in @('const uint FixtureProjectNumber = 1;','const string ReviewedIdMetadataSha256 = "37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4";','const string ReviewedIdObservationSha256 = "89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a";','(int)id.Attributes == 1056769','type == 0x0200002c && ctor == 0x060001ef && getter == 0x060001f2 && owner == 0x02000057 && storeGetter == 0x060002f5','ReviewedIdTokens(id.MetadataToken, ctor.MetadataToken, getter.MetadataToken, store.MetadataToken, storeGetter.MetadataToken)','method.MetadataToken == getter.MetadataToken','method.ReturnType == idType','requests <= 16','services=fixture-id-only','return mode == "original-project" || mode == "original-project-store-id";')){if(-not $projectSource.Contains($guard)){Fail 'FIXED_ID_CONTRACT_SOURCE'}}
$fixtureMatch=[regex]::Match($projectSource,'(?s)static void OriginalProjectStoreId\(\)\s*\{(.*?)\n    \}')
if(-not $fixtureMatch.Success){Fail 'ID_FIXTURE_SOURCE'}
$fixtureBody=$fixtureMatch.Groups[1].Value
$tokensIndex=$fixtureBody.IndexOf('OriginalIdTokens(id, idCtor, idGetter, store, storeGetter);',[StringComparison]::Ordinal)
$createIndex=$fixtureBody.IndexOf('object value = CreateFixtureId(id, idCtor, idGetter);',[StringComparison]::Ordinal)
$proxyIndex=$fixtureBody.IndexOf('new ProjectStoreIdProxy(store, storeGetter, value, Record)',[StringComparison]::Ordinal)
$invokeIndex=$fixtureBody.IndexOf('InvokeProjectWithId(',[StringComparison]::Ordinal)
if($tokensIndex -lt 0 -or $createIndex -le $tokensIndex -or $proxyIndex -le $createIndex -or $invokeIndex -le $proxyIndex){Fail 'ID_FIXTURE_ORDER'}
Write-Output 'OWNED_CASE PASS project-id-source-guards exact-reviewed-contract=true synthetic-uint32=1 no-override=true fixture-before-project=true'
$invalidLog='\\owned-invalid\share\never.log'
Run 'unc-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log',$invalidLog) 127 'LOCAL_PATH_REQUIRED'
Run 'drive-relative-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log','C:owned-never.log') 127 'LOCAL_PATH_REQUIRED'
Run 'excessive-deadline' $launcher @('--seconds','31','--mode','owned-Success','--input',$fixtures,'--log',(Join-Path $output 'never.log')) 127 'LAUNCH_ARGUMENTS'
Run 'fixture-excessive-deadline' $launcher @('--seconds','16','--mode','owned-project-store-id','--input',$fixtures,'--log',(Join-Path $output 'fixture-never.log')) 127 'PROJECT_FIXTURE_DEADLINE'
$existing=Join-Path $output 'existing.log';[IO.File]::WriteAllText($existing,'owned-preserved',$utf8)
Run 'existing-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log',$existing) 127 'NEW_PATH_REQUIRED'
if([IO.File]::ReadAllText($existing) -cne 'owned-preserved'){Fail 'EXISTING_LOG_CHANGED'}
if(-not $interactive){
 Case 'session0-probe' 'owned-Success' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-initialize' 'owned-initialize' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-neutral' 'neutral' 4 'child-exit-confirmed=true' 'NEUTRAL_INTERACTIVE_SESSION_REQUIRED' $build
 Case 'session0-project' 'owned-project' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-project-enum' 'project-enum-metadata' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-project' 'original-project' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-project-store-id' 'owned-project-store-id' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-project-store-id' 'original-project-store-id' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
}else{
 Case 'neutral' 'neutral' 0 'child-exit-confirmed=true' 'NEUTRAL_RESULT PASS' $build
 Case 'success' 'owned-Success' 0 'OWNED_FIXTURE_SUCCESS' 'OWNED_CONSTRUCTOR_RETURN'
 if($administrator){
  Case 'initialize-admin' 'owned-initialize' 5 'child-exit-confirmed=true' 'INITIALIZE_NONADMIN_REQUIRED'
  Case 'project-admin' 'owned-project' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-admin' 'original-project' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'project-store-id-admin' 'owned-project-store-id' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-store-id-admin' 'original-project-store-id' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
 }else{
  Case 'initialize-proxy' 'owned-initialize' 0 'child-exit-confirmed=true' 'OWNED_INITIALIZE_PROXY_PASS getters=6 direct-denial=true wrapped-denial=true requests=8'
  $initializeLog=[IO.File]::ReadAllText((Join-Path $output 'initialize-proxy.log'))
  if(-not $initializeLog.Contains('OWNED_INITIALIZE_CAUGHT_DENIAL_PASS qualification=NOT_ESTABLISHED') -or -not $initializeLog.Contains('constructor-only=false initialization-qualification=NOT_ESTABLISHED')){Fail 'INITIALIZE_QUALIFICATION'}
  Case 'project-proxy' 'owned-project' 0 'child-exit-confirmed=true' 'OWNED_PROJECT_CONSTRUCTOR_PASS'
  $projectLog=[IO.File]::ReadAllText((Join-Path $output 'project-proxy.log'))
  foreach($marker in @('OWNED_PROJECT_ENUM_PASS','OWNED_PROJECT_DENIAL_PASS','PROJECT_CONSTRUCTOR_END returned=False denied-requests=1 qualification=NOT_ESTABLISHED','PROJECT_CONSTRUCTOR_END returned=True denied-requests=1 qualification=NOT_ESTABLISHED','PROJECT_CONSTRUCTOR_END returned=True denied-requests=0 qualification=NOT_ESTABLISHED')){if(-not $projectLog.Contains($marker)){Fail 'PROJECT_OWNED_QUALIFICATION'}}
  foreach($marker in @('OWNED_PLATFORM_FACADE_INCOMPLETE_PIN_PASS','OWNED_PLATFORM_FACADE_PASS microsoft-only=true exact-pin=true wrong-identity-path-cache-reflection=denied','PLATFORM_FACADE_AUDIT alias-allowed=True','origin=FIXED_OS_GAC_PATH','gac=True','reflection-only=False')){if(-not $projectLog.Contains($marker)){Fail 'OWNED_PLATFORM_ALIAS'}}
  if([regex]::Matches($projectLog,' PIN name=').Count -ne 1 -or [regex]::Matches($projectLog,' PLATFORM_FACADE_PIN ').Count -ne 1){Fail 'PLATFORM_PRIVATE_PIN_COUNT'}
  Case 'project-store-id-proxy' 'owned-project-store-id' 0 'child-exit-confirmed=true' 'OWNED_PROJECT_STORE_ID_PASS'
  $idLog=[IO.File]::ReadAllText((Join-Path $output 'project-store-id-proxy.log'))
  foreach($marker in @('OWNED_ID_CONTRACT_PASS rejected=11 before-construction=true token-policy-negatives=5','PROJECT_SERVICE_FIXTURE number=16','services=fixture-id-only','PROJECT_CONSTRUCTOR_END returned=True fixture-calls=2 denied-requests=0 qualification=NOT_ESTABLISHED','OWNED_PROJECT_STORE_ID_PASS created=1 validated=1 stable-object=true')){if(-not $idLog.Contains($marker)){Fail 'PROJECT_ID_OWNED_QUALIFICATION'}}
  if([regex]::Matches($idLog,'PROJECT_ID_CONSTRUCTOR_BEGIN type=Owned.Task009.ProjectIdFixture ').Count -ne 1 -or [regex]::Matches($idLog,'PROJECT_ID_GETTER_BEGIN type=Owned.Task009.ProjectIdFixture ').Count -ne 1 -or $idLog.Contains('PROJECT_SERVICE_DENIED number=0')){Fail 'PROJECT_ID_ONCE_OR_COUNTERS'}
  # Only OWNED bytes are used: missing input count and exact-twelve wrong-hash
  # dispatch must stop before any original identity inspection or assembly load.
  $projectBad=Join-Path $output 'project-twelve-bad-hash';$null=New-Item -ItemType Directory -Path $projectBad
  $projectNames=@('MDT-DaliDcaPlus.dll','Knx.Ets.Sdk.dll','Knx.Ets.Sdk.AddIns.AddInViews.dll','Knx.Ets.Common.Types.dll','Knx.Ets.Common.Resources.dll','Microsoft.Bcl.AsyncInterfaces.dll','netstandard.dll','System.Threading.Tasks.Extensions.dll','Knx.Ets.ObjectModel.dll','FluentResults.dll','Knx.Ets.Common.dll','Knx.Ets.Ids.dll')
  foreach($name in $projectNames[0..10]){Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $projectBad $name)}
  Case 'project-missing-twelve' 'original-project' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $projectBad
  Case 'fixture-missing-twelve' 'original-project-store-id' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $projectBad
  Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $projectBad $projectNames[11])
  Case 'project-twelve-wrong-hash' 'original-project' 2 'INPUT_HASH' 'INPUT_HASH' $projectBad
  Case 'fixture-twelve-wrong-hash' 'original-project-store-id' 2 'INPUT_HASH' 'INPUT_HASH' $projectBad
  foreach($name in @('project-missing-twelve','project-twelve-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ') -or $blocked.Contains('PROJECT_PHASE original-type-materialization')){Fail 'PROJECT_NEGATIVE_LOADED'}}
  foreach($name in @('fixture-missing-twelve','fixture-twelve-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('PROJECT_ID_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ') -or $blocked.Contains('PROJECT_PHASE original-fixture-type-materialization')){Fail 'PROJECT_FIXTURE_NEGATIVE_LOADED'}}
  $enumBad=Join-Path $output 'enum-bad-hash';$null=New-Item -ItemType Directory -Path $enumBad
  # Deliberately renamed OWNED bytes test rejection; no SDK identity is supplied.
  Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $enumBad 'Knx.Ets.Common.Types.dll')
  Case 'project-enum-missing-facade' 'project-enum-metadata' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $enumBad
  Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $enumBad 'netstandard.dll')
  Case 'project-enum-wrong-hash' 'project-enum-metadata' 2 'INPUT_HASH' 'INPUT_HASH' $enumBad
  [IO.File]::WriteAllText((Join-Path $enumBad 'extra.txt'),'owned-extra',$utf8)
  Case 'project-enum-extra-input' 'project-enum-metadata' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $enumBad
 }
 Case 'throw' 'owned-Throw' 10 'child-exit-confirmed=true' 'OWNED_FIXTURE_THROW'
 Case 'missing' 'owned-Missing' 10 'child-exit-confirmed=true' 'DEPENDENCY_REJECT Owned.Task148.Absent'
 Case 'hang' 'owned-Hang' 124 'reason=TIMEOUT' 'CONSTRUCTOR_BEGIN' $fixtures 2
 Case 'flood' 'owned-Flood' 125 'reason=OUTPUT_LIMIT' 'CONSTRUCTOR_BEGIN' $fixtures 5
 $bad=Join-Path $output 'bad-fixture';$null=New-Item -ItemType Directory -Path $bad
 Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination $bad
 [IO.File]::WriteAllText((Join-Path $bad 'Owned.Task148.ConstructorFixtures.dll.sha256'),('0'*64),$utf8)
 Case 'wrong-hash' 'owned-Success' 2 'child-exit-confirmed=true' 'INPUT_HASH' $bad
 $target=Join-Path $output 'junction-target';$link=Join-Path $output 'junction';$null=New-Item -ItemType Directory -Path $target
 [IO.File]::WriteAllText((Join-Path $target 'sentinel.txt'),'owned-preserved',$utf8)
 try{
  $null=New-Item -ItemType Junction -Path $link -Target $target
  Run 'junction-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log',(Join-Path $link 'never.log')) 127 'REPARSE_PATH'
  if([IO.File]::ReadAllText((Join-Path $target 'sentinel.txt')) -cne 'owned-preserved' -or (Test-Path -LiteralPath (Join-Path $target 'never.log'))){Fail 'JUNCTION_TARGET_CHANGED'}
 }finally{if(Test-Path -LiteralPath $link){[IO.Directory]::Delete($link)}}
}
$active=@(Get-CimInstance Win32_Process -Filter "Name='ConstructorProbe.exe' OR Name='ProjectConstructorProbe.exe' OR Name='NeutralStaHost.exe'" | Where-Object {$_.ExecutablePath -and ($_.ExecutablePath -ieq (Join-Path $build 'bin\ConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\NeutralStaHost.exe'))})
if($active.Count -ne 0){Fail 'OWNED_CHILD_REMAINS'}
Write-Output ('OWNED_SUITE PASS session='+$ExpectedSession+' original=NOT_RUN hardware=NOT_RUN descendants=NOT_CONTAINED')
