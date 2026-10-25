#requires -Version 5.1
<# Owned tests only. The coordinator runs this in the stated Windows session.
No original inputs, vendor staging, scheduled task creation, network or hardware. #>
param(
 [Parameter(Mandatory)][string] $BuildDirectory,
 [Parameter(Mandatory)][string] $OutputDirectory,
 [Parameter(Mandatory)][ValidateSet('Session0','Interactive')][string] $ExpectedSession,
 [switch] $XmlRegistrationOnly
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
$constructorSourceDirectory=$PSScriptRoot
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
foreach($guard in @('const string PlatformFacadeRelativePath = "Microsoft.Net\\assembly\\GAC_MSIL\\netstandard\\v4.0_2.0.0.0__cc7b13ffcd2ddd51\\netstandard.dll";','return (Original || OwnedProject || FactoryOnly) && platformFacadePinned && identity == Identities[6]','ProbeFiles.Same(location, platformFacadePath) && cache && !reflectionOnly;','assembly.GetName().Name == "netstandard"','" reflection-only=" + assembly.ReflectionOnly','" pinned-file-sha256=" + (platform ? Hashes[6] : "NOT_ESTABLISHED")')){if(-not $projectSource.Contains($guard)){Fail 'PLATFORM_ALIAS_SOURCE'}}
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
$factoryMatch=[regex]::Match($projectSource,'(?s)static void PinFactoryInputs\(\)\s*\{(.*?)\n    \}')
if(-not $factoryMatch.Success){Fail 'FACTORY_PIN_SOURCE'}
$factoryBody=$factoryMatch.Groups[1].Value
$factoryCount=$factoryBody.IndexOf('CountInputs(14);',[StringComparison]::Ordinal)
$factoryPrefix=$factoryBody.IndexOf('Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);',[StringComparison]::Ordinal)
$factoryExtra=$factoryBody.IndexOf('Pin(Path.Combine(stage, FactoryExtraNames[i] + ".dll"), FactoryExtraHashes[i]);',[StringComparison]::Ordinal)
$factoryPlatform=$factoryBody.IndexOf('PinPlatformFacade();',[StringComparison]::Ordinal)
$factoryIdentity=$factoryBody.IndexOf('AssemblyName.GetAssemblyName(',[StringComparison]::Ordinal)
$factoryReady=$factoryBody.IndexOf('factoryInputsReady = true;',[StringComparison]::Ordinal)
if($factoryCount -lt 0 -or $factoryPrefix -le $factoryCount -or $factoryExtra -le $factoryPrefix -or $factoryPlatform -le $factoryExtra -or $factoryIdentity -le $factoryPlatform -or $factoryReady -le $factoryIdentity -or $factoryBody.Contains('LoadFrom(')){Fail 'FACTORY_PIN_ORDER'}
foreach($guard in @('FactoryExtraSizes = { 374784, 282064 }','747ee8605301b8bd4284cf16b7b8ab900356b7dd01f70800e08d2ff0286bbc12','1158afb2d6c1bb18febcbc5e837c7248682cfafe44ecf7f38c853ecc6258671a','Autofac, Version=8.2.0.0, Culture=neutral, PublicKeyToken=17863af14b0044da','Knx.Kim.DataProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159','(selectedMode != "original-factory" && selectedMode != "original-factory-bcl9" && selectedMode != "original-factory-empty-container" && selectedMode != "original-trace-encryptor" && selectedMode != "original-factory-registered-trace" && selectedMode != "original-project-genuine-factory" && selectedMode != "original-project-genuine-factory-resources" && selectedMode != "original-project-undo-provider-denied" && selectedMode != "original-project-undo-provider-factory" && selectedMode != "original-project-large-string-cache-denied" && selectedMode != "original-project-context-denied" && selectedMode != "original-project-context-xml-registration") || !complete || reflectionOnly','if (FactoryOriginal && factoryInputsReady)','type == 0x02000023 && ctor == 0x0600012e','(int)type.Attributes == 1048577','parameter.IsPublic && parameter.IsInterface && !parameter.IsGenericType')){if(-not $projectSource.Contains($guard)){Fail 'FACTORY_FIXED_PROFILE_SOURCE'}}
if([regex]::Matches($projectSource,'factoryInputsReady\s*=').Count -ne 1){Fail 'FACTORY_PIN_BOUNDARY'}
Write-Output 'OWNED_CASE PASS factory-source-guards fixed14=true hashes-before-platform-and-identities=true extra-reference-admission=new-mode-only constructor-only=true'
$mainStart=$projectSource.IndexOf('internal static int Main(',[StringComparison]::Ordinal)
$configGuard=$projectSource.IndexOf('VerifyBindingConfiguration();',$mainStart,[StringComparison]::Ordinal)
$inputInspection=$projectSource.IndexOf('stage = ProbeFiles.Local(args[3]',$mainStart,[StringComparison]::Ordinal)
if($configGuard -lt 0 -or $inputInspection -le $configGuard){Fail 'BINDING_CONFIG_BEFORE_INPUT'}
foreach($guard in @('FactoryBinding ? ProbeFiles.FactoryBindingConfigText : ProbeFiles.ConfigText','bindingExe == (FactoryBinding && !EmptyContainer)','AppDomain.CurrentDomain.ApplyPolicy(Async8Identity)','FACTORY_BINDING_POLICY_NEGATIVE','FACTORY_POLICY_TARGET_LOADED','FACTORY_BINDING_RESOLVED','FACTORY_BINDING_ASSEMBLY')){if(-not $projectSource.Contains($guard)){Fail 'BINDING_FIXED_SOURCE'}}
Write-Output 'OWNED_CASE PASS binding-source-guards separate-exe-config=true config-before-input=true real-CLR-policy=true pins14=unchanged'
foreach($guard in @('original-factory-empty-container','owned-factory-empty-container','static EmptyContainerApi EmptyApi(','Enum.GetUnderlyingType(options) == typeof(int)','Object.Equals(none.GetRawConstantValue(), 0)','build.ReturnType == container','typeof(IDisposable).IsAssignableFrom(container)','api.Context.IsInstanceOfType(value)','value.GetType().Assembly == api.Builder.Assembly','!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)','disposable.Dispose();','if (provider.Requests != 0) throw new NotSupportedException("FACTORY_PROVIDER_DENIAL_RECORDED")','context-requests=NOT_INTERCEPTED','hard-termination-disposal=NOT_ESTABLISHED')){if(-not $projectSource.Contains($guard)){Fail 'EMPTY_CONTAINER_SOURCE'}}
$emptyOriginal=$projectSource.Substring($projectSource.IndexOf('static void OriginalEmptyContainer()'),$projectSource.IndexOf('static void OwnedEmptyContainer()')-$projectSource.IndexOf('static void OriginalEmptyContainer()'))
if($emptyOriginal.IndexOf('EmptyApi(') -ge $emptyOriginal.IndexOf('WithFactoryContainer(') -or $emptyOriginal -match '\.(Register|Resolve)\('){Fail 'EMPTY_CONTAINER_GUARD_ORDER_OR_OPERATION'}
$containerPins=$projectSource.Substring($projectSource.IndexOf('static void PinContainerInputs()'),$projectSource.IndexOf('static bool ContainerReferencePolicy(')-$projectSource.IndexOf('static void PinContainerInputs()'))
$containerLastHash=$containerPins.IndexOf('Pin(Path.Combine(stage, ContainerNames[i] + ".dll"), ContainerHashes[i]);')
if($containerPins.IndexOf('CountInputs(19);') -lt 0 -or $containerLastHash -lt 0 -or $containerLastHash -ge $containerPins.IndexOf('PinPlatformFacade();') -or $containerPins.IndexOf('PinPlatformFacade();') -ge $containerPins.IndexOf('AssemblyName.GetAssemblyName') -or $containerPins.IndexOf('containerInputsReady = true;') -le $containerPins.LastIndexOf('AssemblyName.GetAssemblyName')){Fail 'CONTAINER_19_PIN_ORDER'}
foreach($guard in @('FactoryContainerConfigText','CONTAINER_BINDING_POLICY_NEGATIVE','CONTAINER_POLICY_TARGET_LOADED','allowed = ContainerReferencePolicy(mode, containerInputsReady','framework-fallback=false','if (EmptyContainer && FactoryOriginal && containerInputsReady)')){if(-not $projectSource.Contains($guard)){Fail 'CONTAINER_FIXED_PROFILE_SOURCE'}}
if([regex]::Matches($projectSource,'containerInputsReady\s*=').Count -ne 2){Fail 'CONTAINER_PIN_BOUNDARY'}
$lifetimeAudit=$projectSource.Substring($projectSource.IndexOf('static void WithFactoryContainer('),$projectSource.IndexOf('static void OriginalEmptyContainer()')-$projectSource.IndexOf('static void WithFactoryContainer('))
if($lifetimeAudit.IndexOf('finally {') -le $lifetimeAudit.IndexOf('WithEmptyContainer(') -or $lifetimeAudit.IndexOf('int denied = provider.Requests;') -le $lifetimeAudit.IndexOf('finally {') -or -not $lifetimeAudit.Contains('FACTORY_PROVIDER_LIFETIME_DENIAL_RECORDED') -or -not $emptyOriginal.Contains('WithFactoryContainer(api, ctor, denied);')){Fail 'PROVIDER_POST_DISPOSAL_AUDIT'}
Write-Output 'OWNED_CASE PASS empty-container-source-guards fixed19=true hashes-before-platform-identities=true shape-before-builder=true no-application-registrations=true dispose-finally=true provider-denial-fails=true'
$valuePin=$projectSource.Substring($projectSource.IndexOf('static void PinValueTupleReference()'),$projectSource.IndexOf('// Private finite predicate only.')-$projectSource.IndexOf('static void PinValueTupleReference()'))
if($containerPins.IndexOf('PinValueTupleReference();') -le $containerLastHash -or $containerPins.IndexOf('PinValueTupleReference();') -ge $containerPins.IndexOf('PinPlatformFacade();') -or $valuePin.IndexOf('ProbeFiles.Hash(stream)') -ge $valuePin.IndexOf('AssemblyName.GetAssemblyName') -or $valuePin.IndexOf('valueTuplePinned = true;') -le $valuePin.IndexOf('AssemblyName.GetAssemblyName')){Fail 'VALUETUPLE_PIN_ORDER'}
foreach($guard in @('stream.Length == 25544','62216986c754ad8f0ee68412c42cfc3fc6e025bd908ae1fef1a3a8d528b1495a','(selectedMode == "original-factory-empty-container" || selectedMode == "original-trace-encryptor" || selectedMode == "original-factory-registered-trace" || selectedMode == "original-project-genuine-factory" || selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration") && complete && identity == ValueTupleIdentity && cache && !reflectionOnly','allowed = selected;','VALUETUPLE_REFERENCE_AUDIT','CheckBuilderDependencies(unexpected, rejected);','ownedBuilderGuard == null || mode == "owned-factory-empty-container"')){if(-not $projectSource.Contains($guard)){Fail 'SOURCE12_FIXED_SCOPE'}}
$builderCore=$projectSource.Substring($projectSource.IndexOf('static void WithEmptyContainerCore('),$projectSource.IndexOf('static void InvokeFactoryWithContainer(')-$projectSource.IndexOf('static void WithEmptyContainerCore('))
if($builderCore.IndexOf('CheckBuilderDependencies(unexpected, rejected);') -le $builderCore.IndexOf('EMPTY_CONTAINER_EXACT_BUILDER') -or $builderCore.IndexOf('CheckBuilderDependencies(unexpected, rejected);') -ge $builderCore.IndexOf('EMPTY_CONTAINER_BUILD_BEGIN') -or -not $builderCore.Contains('EMPTY_CONTAINER_BUILD_DEPENDENCY')){Fail 'PREBUILD_DEPENDENCY_GUARD'}
if([regex]::Matches($projectSource,'valueTuplePinned\s*=').Count -ne 1){Fail 'VALUETUPLE_COMPLETE_BOUNDARY'}
Write-Output 'OWNED_CASE PASS source12-source-guards exact-os-reference=true hash-before-identity=true original-empty-or-trace-only=true builder-flags-before-build=true no-production-latch-reset=true'
foreach($guard in @('Autofac.resources, Version=8.2.0.0, Culture=en-GB, PublicKeyToken=17863af14b0044da','Autofac.resources, Version=8.2.0.0, Culture=en, PublicKeyToken=17863af14b0044da','[ThreadStatic] static bool resourcePhaseActive;','[ThreadStatic] static int resourceProbeCount;','if (AllowAutofacResourceNull(args)) return null;','Object.ReferenceEquals(owner, requester)','NULL_UNKNOWN','NOT_ESTABLISHED','action=','RETURN_NULL_NO_ASSEMBLY','count >= 1 && count <= 8','resourceOwner = autofac;','WithFactoryResourcePhase(delegate { constructed = ctor.Invoke(arguments); returned = true; });')){if(-not $projectSource.Contains($guard)){Fail 'RESOURCE_FIXED_POLICY_SOURCE'}}
$resourceResolve=$projectSource.Substring($projectSource.IndexOf('static Assembly Resolve('),$projectSource.IndexOf('static Assembly ResolvePinnedFactoryReference(')-$projectSource.IndexOf('static Assembly Resolve('))
if($resourceResolve.IndexOf('if (AllowAutofacResourceNull(args)) return null;') -ge $resourceResolve.IndexOf('rejected = true;')){Fail 'RESOURCE_BRANCH_BEFORE_FATAL'}
$resourcePhase=$projectSource.Substring($projectSource.IndexOf('static void WithFactoryResourcePhase('),$projectSource.IndexOf('static Assembly Resolve(')-$projectSource.IndexOf('static void WithFactoryResourcePhase('))
if(-not $resourcePhase.Contains('finally {') -or $resourcePhase.IndexOf('resourcePhaseActive = false;') -le $resourcePhase.IndexOf('finally {') -or $resourcePhase.IndexOf('resourcePhaseActive = false;') -ge $resourcePhase.IndexOf('Record(')){Fail 'RESOURCE_FINALLY_CLEAR'}
$resourceAllow=$projectSource.Substring($projectSource.IndexOf('static bool AllowAutofacResourceNull('),$projectSource.IndexOf('static void WithFactoryResourcePhase(')-$projectSource.IndexOf('static bool AllowAutofacResourceNull('))
if($resourceAllow -match 'Assembly\.Load|File\.|Directory\.|return null|rejected\s*=|unexpected\s*='){Fail 'RESOURCE_NO_LOAD_OR_LATCH_RESET'}
Write-Output 'OWNED_CASE PASS source13-source-guards exact2resources=true thread-phase8=true null-requester=UNKNOWN no-assembly-service-provided=true old-fatal-audit=preserved'




foreach($guard in @('original-trace-encryptor','owned-trace-encryptor','const string ReviewedTraceParameterReportSha256 = "07accca76a4ab722a7d6943578ab955659261cada7b7cc72feee00f730462bc2";','const int TraceDeclaredDefault = 1;','parameters[0].RawDefaultValue','parameters[0].Attributes == 4112','parameters[0].HasDefaultValue','CallingConventions.Standard | CallingConventions.HasThis','header32=STATIC_PROVENANCE_ONLY','parameter == 0x080001e4','ctor == 0x06000364','type == 0x0200007e','contract == 0x02000077','ownedGuard == null || TraceOwned','TraceDependencyGuard(unexpected, rejected, phase)','TRACE_CONSTRUCTOR_DISPOSE_RETURNED_NOT_QUALIFIED','if (TraceOnly) { if (TraceOriginal) OriginalTraceEncryptor(); else OwnedTraceEncryptor(); }')){if(-not $projectSource.Contains($guard)){Fail 'TRACE_FIXED_CONTRACT_SOURCE'}}
$traceCore=$projectSource.Substring($projectSource.IndexOf('static void WithTraceLifetimeCore('),$projectSource.IndexOf('static void OriginalTraceEncryptor()')-$projectSource.IndexOf('static void WithTraceLifetimeCore('))
if($traceCore.IndexOf('check("PRE_CTOR")') -ge $traceCore.IndexOf('api.Constructor.Invoke(') -or $traceCore.IndexOf('check("POST_CTOR")') -le $traceCore.IndexOf('api.Constructor.Invoke(') -or $traceCore.IndexOf('check("POST_CTOR")') -ge $traceCore.IndexOf('((IDisposable)instance).Dispose();') -or $traceCore.IndexOf('check("POST_DISPOSE")') -le $traceCore.IndexOf('((IDisposable)instance).Dispose();') -or [regex]::Matches($traceCore,'api.Constructor.Invoke\(').Count -ne 1 -or [regex]::Matches($traceCore,'\(\(IDisposable\)instance\).Dispose\(\)').Count -ne 1){Fail 'TRACE_SHARED_LIFETIME_GUARDS'}
$traceOriginal=$projectSource.Substring($projectSource.IndexOf('static void OriginalTraceEncryptor()'),$projectSource.IndexOf('static void OwnedTraceEncryptor()')-$projectSource.IndexOf('static void OriginalTraceEncryptor()'))
if($traceOriginal -match 'Register|Resolve\(|OriginalFactory\(|OriginalProject\(|EmptyApi\(|WithFactory|GetCustomAttributes|BindingFlags.NonPublic' -or $traceCore -match '(unexpected|rejected)\s*=' -or -not $traceOriginal.Contains('TraceOriginal && containerInputsReady')){Fail 'TRACE_ORIGINAL_SCOPE'}
Write-Output 'OWNED_CASE PASS source17-source-guards exact-original-default=true public-reflection-only=true one-ctor-dispose=true shared-phase-audit=true'

foreach($guard in @('original-factory-registered-trace','owned-factory-registered-trace','static TraceRegistrationApi RegistrationApi(','generic[0].GenericParameterAttributes == GenericParameterAttributes.ReferenceTypeConstraint','new object[] { builder, trace }','external.DeclaringType == closed','api.External.Invoke(registration, new object[0])','Object.ReferenceEquals(registration, external)','TRACE_REGISTER_END external-ownership=True','REGISTERED_TRACE_FINAL_AUDIT after-container-and-trace-disposal=True','if (use == null) Record("TRACE_CONSTRUCTOR_DISPOSE_RETURNED_NOT_QUALIFIED explicit-other-sdk-calls=0','if (register != null) { register(builder); CheckBuilderDependencies(unexpected, rejected); }')){if(-not $projectSource.Contains($guard)){Fail 'REGISTERED_TRACE_FIXED_SOURCE'}}
$registeredCore=$projectSource.Substring($projectSource.IndexOf('static void WithRegisteredTrace('),$projectSource.IndexOf('static void OriginalRegisteredTrace()')-$projectSource.IndexOf('static void WithRegisteredTrace('))
if($registeredCore.IndexOf('WithTraceLifetimeCore(') -ge $registeredCore.IndexOf('WithEmptyContainerCore(') -or $registeredCore.IndexOf('int denied = provider.Requests;') -le $registeredCore.IndexOf('finally {') -or $registeredCore -match '(unexpected|rejected)\s*=|BindingFlags.NonPublic'){Fail 'REGISTERED_TRACE_COMPLETE_LIFETIME'}
$registeredOriginal=$projectSource.Substring($projectSource.IndexOf('static void OriginalRegisteredTrace()'),$projectSource.IndexOf('static void OwnedRegisteredTrace()')-$projectSource.IndexOf('static void OriginalRegisteredTrace()'))
if($registeredOriginal.IndexOf('RegistrationApi(') -ge $registeredOriginal.IndexOf('WithRegisteredTrace(') -or $registeredOriginal -match 'Resolve\(|GetCustomAttributes|BindingFlags.NonPublic|Encrypt\(|Decrypt\(|IsEncrypted\('){Fail 'REGISTERED_TRACE_ORIGINAL_SCOPE'}
Write-Output 'OWNED_CASE PASS source18-source-guards exact-T-interface=true public-interface-ownership=true shape-before-effects=true two-lifetime-final-audit=true original=NOT_RUN'

foreach($guard in @('original-project-genuine-factory','owned-project-genuine-factory','static ProjectScopeApi ScopedProjectApi(','contract.IsAssignableFrom(factoryType)','project.GetInterfaceMap(typeof(IDisposable))','map.TargetMethods[0].MetadataToken == dispose.MetadataToken','dispose == 0x060028bd','factoryContract == 0x02000037','PROJECT_FACTORY_FINAL_AUDIT after-all-managed-disposal=True','denied != 0 || store.DeniedRequests != 0','PROJECT_SCOPE_RETURNED_NOT_QUALIFIED','if (projectUse == null) InvokeFactoryWithContainer','else InvokeFactoryForProject','try { use(factory); } finally { GC.KeepAlive(factory); }')){if(-not $projectSource.Contains($guard)){Fail 'PROJECT_SCOPE_FIXED_SOURCE'}}
$scopeLife=$projectSource.Substring($projectSource.IndexOf('static void WithProjectScope('),$projectSource.IndexOf('static void InvokeFactoryForProject(')-$projectSource.IndexOf('static void WithProjectScope('))
if($scopeLife.IndexOf('check("POST_CTOR")') -ge $scopeLife.IndexOf('((IDisposable)value).Dispose();') -or $scopeLife.IndexOf('check("POST_DISPOSE")') -le $scopeLife.IndexOf('((IDisposable)value).Dispose();') -or [regex]::Matches($scopeLife,'api.Constructor.Invoke\(').Count -ne 1 -or [regex]::Matches($scopeLife,'\(\(IDisposable\)value\).Dispose\(\)').Count -ne 1 -or $scopeLife -match '(unexpected|rejected)\s*='){Fail 'PROJECT_SCOPE_SHARED_LIFETIME'}
$scopeOriginal=$projectSource.Substring($projectSource.IndexOf('static void OriginalProjectScope()'),$projectSource.IndexOf('static void OwnedProjectScope()')-$projectSource.IndexOf('static void OriginalProjectScope()'))
if($scopeOriginal.IndexOf('ScopedProjectApi(') -ge $scopeOriginal.IndexOf('CreateFixtureId(') -or $scopeOriginal.IndexOf('OriginalIdTokens(') -ge $scopeOriginal.IndexOf('CreateFixtureId(') -or $scopeOriginal.IndexOf('PROJECT_SCOPE_ORIGINAL_CONTRACT') -ge $scopeOriginal.IndexOf('WithRegisteredTrace(') -or $scopeOriginal -match 'Resolve\(|GetCustomAttributes|BindingFlags.NonPublic|Encrypt\(|Decrypt\(|IsEncrypted\(|constructed\s*='){Fail 'PROJECT_SCOPE_SHAPES_BEFORE_EFFECTS'}
Write-Output 'OWNED_CASE PASS source19-source-guards public-dispose-map=true genuine-factory-local=true shapes-before-effects=true final-store-provider-after-all-lifetimes=true original=NOT_RUN'

$resourceCases=@('id-constructor','id-getter','getter','builder','registration','external','build','factory','project','validation','logging','error','project-dispose','container-dispose','trace-dispose','nested-window','nested-budget','nested-callback','off-thread')
$launcherSource=[IO.File]::ReadAllText((Join-Path $constructorSourceDirectory 'ProbeLauncher.cs'))
$caseLiteral='{ '+(($resourceCases|ForEach-Object{'"'+$_+'"'}) -join ', ')+' };'
if(-not $projectSource.Contains('static readonly string[] OwnedResourceCases = '+$caseLiteral) -or -not $launcherSource.Contains('string[] resourceCases = '+$caseLiteral)){Fail 'RESOURCE_CASE_EXACT_WHITELIST'}
foreach($guard in @('if (ProjectResources) return AllowProjectResourceNull','(selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration") && complete && budget && (window == 1 || window == 2) && !suspended && pinned','static bool ConsumeProjectResourceProbe(bool eligible)','if (eligible) projectResourceProbeCount = NextResourceProbe(projectResourceProbeCount);','int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback();','finally { ProjectConstructorProbe.RestoreProjectResourceCallback(suspended); }','if (ProjectResources) WithProjectResourceWindow(2, invoke); else invoke();','WithProjectResourceWindow(1, delegate { factory = ctor.Invoke(arguments); }); returned = true;','if (ProjectResources) WithProjectResourceBudget(run); else run();','Array.IndexOf(OwnedResourceCases, args[7]) >= 0')){if(-not $projectSource.Contains($guard)){Fail 'RESOURCE_PROJECT_FIXED_POLICY'}}
$newWindow=$projectSource.Substring($projectSource.IndexOf('static void WithProjectResourceWindow('),$projectSource.IndexOf('internal static int SuspendProjectResourceCallback(')-$projectSource.IndexOf('static void WithProjectResourceWindow('))
if(-not $newWindow.Contains('finally { projectResourceWindow = 0; }') -or $newWindow -match 'projectResourceProbeCount\s*=|Record\(|Assembly\.Load'){Fail 'RESOURCE_SHARED_WINDOW_NO_RESET'}
$newPolicy=$projectSource.Substring($projectSource.IndexOf('static bool ProjectResourcePolicy('),$projectSource.IndexOf('static void ProjectResourceRequire(')-$projectSource.IndexOf('static bool ProjectResourcePolicy('))
if($newPolicy -match 'Assembly\.Load|File\.|Directory\.|return null|(rejected|unexpected)\s*='){Fail 'RESOURCE_PROJECT_NO_LOAD_OR_RESET'}
if($projectSource.IndexOf('OWNED_RESOURCE_CASE_ARGUMENTS') -ge $projectSource.IndexOf('log = new FileStream(') -or $launcherSource.IndexOf('OWNED_RESOURCE_CASE_ARGUMENTS') -ge $launcherSource.IndexOf('string input = ProbeFiles.Local(')){Fail 'RESOURCE_SELECTOR_BEFORE_EFFECTS'}
if([regex]::Matches($projectSource,'int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback\(\);').Count -ne 2 -or [regex]::Matches($projectSource,'finally \{ ProjectConstructorProbe.RestoreProjectResourceCallback\(suspended\); \}').Count -ne 2 -or $projectSource.Contains('SuspendProjectResourceGetter') -or $projectSource.Contains('getter-suspended=')){Fail 'RESOURCE_BOTH_PROXY_SUSPENSIONS'}
foreach($guard in @('ownedResourceCase == "nested-callback" && !callbackReentryAttempted','storeGetter.Invoke(store.GetTransparentProxy(), new object[0]);','refused && rejected && projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == before','diagnostic=PROJECT_RESOURCE_NESTED_CALLBACK outer-window=0 outer-suspended=True aggregate-unchanged=True attempts=1')){if(-not $projectSource.Contains($guard)){Fail 'RESOURCE_NESTED_CALLBACK_REGRESSION'}}
Write-Output 'OWNED_CASE PASS source20a-source-guards nested-callback=one-shot-same-proxy exact-diagnostic=True counter-preserved=True outer-finally-restore=True sticky-final-audit=True original=NOT_RUN'
Write-Output 'OWNED_CASE PASS source20-source-guards explicit-new-mode-pair=True shared-budget8=True exact-invoke-only=True getter-suspension=True finite-owned-selector=True old-mode-preserved=True'

$undoCases=@('get','new','root','invalid','saturation','trace-ctor','builder','registration','external','build','factory','project','project-dispose','container-dispose','trace-dispose','error','final-audit','nested','resource','register-null','external-null')
$undoLiteral='{ '+(($undoCases|ForEach-Object{'"'+$_+'"'}) -join ', ')+' };'
if(-not $projectSource.Contains('static readonly string[] OwnedUndoCases = '+$undoLiteral) -or -not $launcherSource.Contains('string[] undoCases = '+$undoLiteral)){Fail 'UNDO_FIXED_CASES'}
foreach($guard in @('internal sealed class UndoProviderDenyProxy : RealProxy','if (requests < 17) requests++; count = requests;','previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true;','finally { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }','UNDO_PROVIDER_UNSUPPORTED:','UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True','finally { if (count != 0) throw new NotSupportedException("UNDO_PROVIDER_FINAL_DENIAL"); }','UndoContract(undoType, opaque); OriginalUndoTokens(undoType);','RegisterTrace(traceRegistration, builder, instance); RegisterUndoProvider(undoRegistration, builder, undo);','mode == "owned-project-undo-provider-denied" && args[6] == "--owned-undo-case"','CheckUndoProvider("PROCESS_FINAL")','activeUndo = undo;')){if(-not $projectSource.Contains($guard)){Fail 'UNDO_FIXED_SOURCE'}}
$undoProxy=$projectSource.Substring($projectSource.IndexOf('internal sealed class UndoProviderDenyProxy'),$projectSource.IndexOf('// A service-reference factory only.')-$projectSource.IndexOf('internal sealed class UndoProviderDenyProxy'))
if($undoProxy.IndexOf('requests++') -ge $undoProxy.IndexOf('SuspendProjectResourceCallback') -or $undoProxy.IndexOf('requests++') -ge $undoProxy.IndexOf('message as IMethodCallMessage') -or $undoProxy -match 'call\.Args|\.ToString\(|return new ReturnMessage\((?!error)'){Fail 'UNDO_COUNTER_BEFORE_INPUT_NO_SUCCESS'}
Write-Output 'OWNED_CASE PASS source22-source-guards exact-typed-denial=True counter-first=True two-fixed-registrations=True final-audit-after-all=True no-service-value=True old-proxy=preserved'

$factoryCases=@('project-activation', 'no-activation', 'null', 'wrong-type', 'second', 'saturation', 'nested', 'off-thread', 'recorder', 'resource', 'get', 'new', 'root', 'trace-ctor', 'builder', 'registration', 'single', 'external', 'build', 'factory', 'project', 'project-dispose', 'container-dispose', 'trace-dispose', 'error', 'final-audit', 'late')
$factoryLiteral='{ '+(($factoryCases|ForEach-Object{'"'+$_+'"'}) -join ', ')+' };'
if(-not$projectSource.Contains('static readonly string[] OwnedFactoryCases = '+$factoryLiteral) -or -not$launcherSource.Contains('string[] factoryCases = '+$factoryLiteral)){Fail 'FACTORY_PROVIDER_FIXED_CASES'}
foreach($guard in @('internal sealed class UndoProviderFactoryHolder','if (entries < 2) entries++; count = entries;','bool eligible = ProjectConstructorProbe.UndoFactoryWindowEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();','if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }','context != null && Thread.CurrentThread.ManagedThreadId == ownerThread','typeof(T) == contract','Delegate.CreateDelegate(callbackType, this, method, true)','Callback.Method.Equals(method)','T value = (T)instance;','finally { if (faulted || entries > 1 || successes > 1) throw new NotSupportedException("UNDO_FACTORY_FINAL_DENIAL"); }','activeUndo = undo; activeFactory = holder; holder.Begin();','holder.Close();','GC.KeepAlive(holder.Callback)','CheckUndoFactory("PROCESS_FINAL")','RegisterTrace(traceRegistration, builder, instance); RegisterUndoFactory(factoryRegistration, builder, holder);','generic.GenericParameterAttributes == GenericParameterAttributes.None','callback.GetGenericTypeDefinition() != typeof(Func<,>)','method.ReturnType == closed')){if(-not$projectSource.Contains($guard)){Fail 'FACTORY_PROVIDER_FIXED_SOURCE'}}
$holder=$projectSource.Substring($projectSource.IndexOf('internal sealed class UndoProviderFactoryHolder'),$projectSource.IndexOf('// Source27 keeps counters/faults')-$projectSource.IndexOf('internal sealed class UndoProviderFactoryHolder'))
if($holder.IndexOf('entries++')-ge$holder.IndexOf('UndoFactoryWindowEligible()') -or $holder.IndexOf('SuspendProjectResourceCallback()')-ge$holder.IndexOf('context != null') -or $holder.Contains('DynamicInvoke') -or $holder.Contains('instance.GetType(') -or $holder.Contains('context.')){Fail 'FACTORY_PROVIDER_COUNTER_EXCLUSION_NO_CONTEXT_USE'}
Write-Output 'OWNED_CASE PASS source25-factory-fixed-guard exact-typed-delegate=true count-first=true exclusion-before-validation=true no-positive-provider-value=true runtime=NOT_RUN'
$cacheCases=@('second', 'total-boundary', 'total-limit', 'stream-id', 'stream-exists', 'stream-read', 'stream-multi', 'stream-delete', 'stream-time', 'stream-mask', 'object-type', 'object-string', 'object-hash', 'object-equals', 'dispose-message', 'foreign', 'invalid', 'custom-method', 'throw-method', 'throw-args', 'throw-context', 'mutable', 'mutable-id', 'message-reentry', 'forged-name', 'forged-count', 'forged-in', 'forged-args', 'recorder', 'wrapper-restore', 'stream-recorder', 'stream-restore', 'nested', 'off-thread', 'trace-ctor', 'builder', 'registration', 'single', 'external', 'build', 'factory', 'validation', 'logging', 'error', 'project-dispose', 'container-dispose', 'trace-dispose', 'wrapper-final', 'stream-final', 'late')
$cacheLiteral='{ '+(($cacheCases|ForEach-Object{'"'+$_+'"'}) -join ', ')+' };'
if(-not$projectSource.Contains('static readonly string[] OwnedCacheCases = '+$cacheLiteral) -or -not$launcherSource.Contains('string[] cacheCases = '+$cacheLiteral)){Fail 'CACHE_FIXED_CASES'}
foreach($guard in @('internal class ProjectStoreIdProxy : RealProxy','protected IMessage InvokeCore(IMessage message)','internal sealed class LargeStringCacheStoreProxy : ProjectStoreIdProxy','int count = State.Count();','if (entries < 17) entries++; return entries;','if (attempts < 2) attempts++;','Thread.CurrentThread.ManagedThreadId != ownerThread','State.CacheAttempt(eligible);','new CacheCallSnapshot(cache ? cacheGetter : idGetter, context, uri)','IMessage result = base.InvokeCore(snapshot);','new ReturnMessage(reference, new object[0], 0, context, snapshot)','method.GetType() != typeof(object).GetMethod("GetType").GetType()','!State.Faulted','finally { State.Exit(entered, prepared); }','method.MetadataToken == 0x06000366 + i','owner.MetadataToken == 0x02000057','method.MetadataToken == 0x06000313','type.MetadataToken == 0x02000061','finally { if (faulted || entries > 16 || attempts > 1 || replies > 1 || streamDenied != 0) throw new NotSupportedException("CACHE_STREAM_FINAL_DENIAL"); }','CheckCache("PROCESS_FINAL")','store.State.Close();','GC.KeepAlive(stream.GetTransparentProxy())')){if(-not$projectSource.Contains($guard)){Fail 'CACHE_FIXED_SOURCE'}}
$cacheProxy=$projectSource.Substring($projectSource.IndexOf('internal sealed class LargeStringCacheStoreProxy'),$projectSource.IndexOf('internal static class ProjectConstructorProbe')-$projectSource.IndexOf('internal sealed class LargeStringCacheStoreProxy'))
if($cacheProxy.Contains('base.Invoke(') -or $cacheProxy.IndexOf('State.Count()')-ge$cacheProxy.IndexOf('message as IMethodCallMessage') -or $cacheProxy.IndexOf('SuspendProjectResourceCallback()')-ge$cacheProxy.IndexOf('call.MethodBase')){Fail 'CACHE_COUNTER_EXCLUSION_BEFORE_MESSAGE'}
$streamProxy=$projectSource.Substring($projectSource.IndexOf('internal sealed class StreamStoreDenyProxy'),$projectSource.IndexOf('internal sealed class LargeStringCacheStoreProxy')-$projectSource.IndexOf('internal sealed class StreamStoreDenyProxy'))
if($streamProxy.Contains('call.Args') -or $streamProxy.Contains('call.GetArg') -or $streamProxy.Contains('call.InArgs') -or $streamProxy.Contains('call.ToString') -or $streamProxy.IndexOf('requests++')-ge$streamProxy.IndexOf('message as IMethodCallMessage') -or -not$streamProxy.Contains('return new ReturnMessage(error, call);')){Fail 'STREAM_DENY_ONLY_COUNT_FIRST_NO_ARGUMENT_VALUES'}
Write-Output 'OWNED_CASE PASS source27-cache-fixed-guard one-reference=True shared16=True count-first=True immutable-zeroarg-snapshot=True no-stream-values=True original=NOT_RUN'

# Source32 finite construction and denial invariants; these are source checks only.
$contextCases=@('collection-parent','collection-importer','collection-throw','project-context','parent-inherited','parent-generic','parent-void','parent-object','context-object','parent-null','context-null','parent-custom','parent-mutable','parent-throw','parent-saturation','context-saturation','importer-saturation','importer-null','nested-parent','nested-context','nested-importer','off-thread-parent','off-thread-context','off-thread-importer','early-parent','early-context','early-importer','late-parent','late-context','late-importer','parent-recorder','context-recorder','importer-recorder','parent-restore','context-restore','importer-restore','project-dispose-parent','container-dispose-context','trace-dispose-importer','error-context','parent-count-only','context-count-only','importer-count-only','parent-fault-only','context-fault-only','importer-fault-only','process-final-parent','parent-getter-reentry','dispose-begin-log-fault')
$contextLiteral='{ '+(($contextCases|ForEach-Object{'"'+$_+'"'}) -join ', ')+' };'
if($contextCases.Count-gt64 -or -not$projectSource.Contains('static readonly string[] OwnedContextCases = '+$contextLiteral) -or -not$launcherSource.Contains('string[] contextCases = '+$contextLiteral)){Fail 'CONTEXT_FIXED_CASES'}
foreach($guard in @('static ContextConstructionApi ContextApi(','0x0600298e','0x06002831','CONTEXT_CONSTRUCTOR_VECTOR','CONTEXT_IMPORTER_INVOKE_ORDER','typeof(Func<,,,,,,,>)','Delegate.CreateDelegate(callbackType, this, method, true)','Callback.GetInvocationList().Length == 1','CONTEXT_COLLECTION_WINDOW_ZERO','contextRetained[6] = collection;','TraceDependencyGuard(unexpected, rejected, "CONTEXT_POST_COLLECTION")','CONTEXT_FINAL_AUDIT phase=','parentCount != 0 || parentFault || objectCount != 0 || objectFault || importerCount != 0 || importerFault','CheckContextArguments("AFTER_ALL_MANAGED_CLEANUP")','outcome = MainCore(args);','CheckContextArguments("PROCESS_FINAL")','internal readonly ContextDenialState State = new ContextDenialState(16);','internal readonly ContextDenialState State = new ContextDenialState(1);')){if(-not$projectSource.Contains($guard)){Fail 'CONTEXT_FIXED_SOURCE'}}
$contextAdapter=$projectSource.Substring($projectSource.IndexOf('internal sealed class ContextArgumentDenyProxy'),$projectSource.IndexOf('internal static class ProjectConstructorProbe')-$projectSource.IndexOf('internal sealed class ContextArgumentDenyProxy'))
if($contextAdapter.Contains('call.Args') -or $contextAdapter.Contains('call.GetArg') -or $contextAdapter.Contains('DynamicInvoke') -or $contextAdapter.Contains('return default') -or $contextAdapter.IndexOf('State.Deny()')-ge$contextAdapter.IndexOf('SuspendProjectResourceCallback()')){Fail 'CONTEXT_COUNT_FIRST_NO_VALUES'}
if($contextAdapter.IndexOf('if (count > 16) throw new NotSupportedException("CONTEXT_ARGUMENT_BUDGET:" + name);')-ge$contextAdapter.IndexOf('message as IMethodCallMessage') -or -not$contextAdapter.Contains('CONTEXT_ARGUMENT_BUDGET:')){Fail 'CONTEXT_HARD_CUTOFF_BEFORE_MESSAGE'}
if(-not$projectSource.Contains('try { ContextConstructionRecord("CONTEXT_PROJECT_DISPOSE_BEGIN attempts=1"); }') -or -not$projectSource.Contains('finally { try { ((IDisposable)project).Dispose(); }')){Fail 'CONTEXT_LOG_GUARANTEED_DISPOSE'}
if(-not$projectSource.Contains('ownedContextCase == null || contextCaseFired || ownedContextCase == "dispose-begin-log-fault"')){Fail 'CONTEXT_LOG_FAULT_NO_ARGUMENT_DENIAL'}
Write-Output 'OWNED_CASE PASS source32-context-fixed-guards collection-window0=True separate-ctor=True deny-all=True exceptional-process-final=True ABI=NOT_RUN'


# Fixed Source33 profile/source checks; no original input is read by this driver.
foreach($guard in @('const int XmlRegistrationSize = 188880;','a9bce396208e9d5b9680e65ef5d730ed9873c21f04de5028cb6f492264c70ac3','static void PinXmlRegistrationInputs()','CountInputs(20);','PinXmlRegistrationFile();','xmlRegistrationReady = true;','XmlRegistrationResolvePolicy(mode, xmlRegistrationReady && containerInputsReady','if (!selected) unexpected = true;','XML_REGISTRATION_AUDIT','catch { rejected = true; throw; }')){if(-not$projectSource.Contains($guard)){Fail 'XML_FIXED_PROFILE_SOURCE'}}
$xmlPin=$projectSource.Substring($projectSource.IndexOf('static void PinXmlRegistrationInputs()'),$projectSource.IndexOf('static void PinXmlRegistrationFile()')-$projectSource.IndexOf('static void PinXmlRegistrationInputs()'))
if($xmlPin.IndexOf('PinXmlRegistrationFile();')-ge$xmlPin.IndexOf('PinValueTupleReference();') -or $xmlPin.IndexOf('PinValueTupleReference();')-ge$xmlPin.IndexOf('AssemblyName.GetAssemblyName') -or $xmlPin.IndexOf('xmlRegistrationReady = true;')-le$xmlPin.LastIndexOf('AssemblyName.GetAssemblyName') -or [regex]::Matches($projectSource,'xmlRegistrationReady\s*=').Count-ne1){Fail 'XML_HASH_IDENTITY_READY_ORDER'}
Write-Output 'OWNED_CASE PASS source33-xml-fixed-guards all20-hashes-before-identities=True old19-preserved=True private-origin-only=True original=NOT_RUN'
if($XmlRegistrationOnly){
 $newMode='owned-project-context-xml-registration'
 foreach($oldXmlMode in @('original-project-context-xml-registration','original-project-context-denied','owned-project-context-denied','owned-project-large-string-cache-denied')){
  Run ('xml-selector-refused-'+$oldXmlMode) $launcher @('--seconds','1','--mode',$oldXmlMode,'--input',$fixtures,'--log',(Join-Path $output ('xml-selector-'+$oldXmlMode+'.log')),'--owned-xml-pin-case','missing') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
 }
 if(Get-ChildItem -LiteralPath $output -Filter 'xml-selector*.log'){Fail 'XML_SELECTOR_CREATED_LOG'}
 if(-not$interactive -or $administrator){Case 'xml-session-guard' $newMode $(if(-not$interactive){4}else{5}) 'child-exit-confirmed=true' $(if(-not$interactive){'INTERACTIVE_SESSION_REQUIRED'}else{'PROJECT_NONADMIN_REQUIRED'})}
 else{
  Case 'xml-policy-context-positive' $newMode 0 'child-exit-confirmed=true' 'OWNED_CONTEXT_LIFETIME_PASS'
  $xmlPositive=[IO.File]::ReadAllText((Join-Path $output 'xml-policy-context-positive.log'))
  if(-not$xmlPositive.Contains('OWNED_XML_POLICY_PASS identity-negatives=8 private-origin-negatives=6 predicate-only=True actual-XML-load=NOT_RUN')){Fail 'XML_POLICY_OWNED_POSITIVE'}
  foreach($selected in @('missing','size','hash')){
   $xmlInput=$fixtures
   if($selected-ne'missing'){
    $xmlInput=Join-Path $output ('xml-owned-input-'+$selected);$null=New-Item -ItemType Directory -Path $xmlInput
    foreach($name in @('Owned.Task148.ConstructorFixtures.dll','Owned.Task148.ConstructorFixtures.dll.sha256')){Copy-Item -LiteralPath (Join-Path $fixtures $name) -Destination (Join-Path $xmlInput $name)}
    $ownedBytes=[byte[]]::new($(if($selected-eq'size'){1}else{188880}));[IO.File]::WriteAllBytes((Join-Path $xmlInput 'Knx.Ets.Xml.RegistrationRelevanceInformation.dll'),$ownedBytes)
   }
   $caseLog=Join-Path $output ('xml-pin-'+$selected+'.log')
   Run ('xml-pin-'+$selected) $launcher @('--seconds','15','--mode',$newMode,'--input',$xmlInput,'--log',$caseLog,'--owned-xml-pin-case',$selected) 0 'child-exit-confirmed=true'
   $content=[IO.File]::ReadAllText($caseLog);if(-not$content.Contains('OWNED_XML_PIN_NEGATIVE_PASS case='+$selected+' xml-ready=False container-ready=False identity-load=NOT_REACHED') -or $content.Contains('CONTEXT_COLLECTION_BEGIN') -or $content.Contains('XML_REGISTRATION_RESOLVED')){Fail 'XML_PIN_NEGATIVE_LOADED'}
  }
  foreach($count in @(19,20)){
   $xmlInput=Join-Path $output ('xml-count-'+$count);$null=New-Item -ItemType Directory -Path $xmlInput
   for($i=0;$i-lt$count;$i++){[IO.File]::WriteAllText((Join-Path $xmlInput ('owned-'+$i+'.txt')),'owned-count-only',$utf8)}
   $mode=if($count-eq19){'original-project-context-xml-registration'}else{'original-project-context-denied'}
   Case ('xml-count-'+$count) $mode 2 'child-exit-confirmed=true' 'PROJECT_EXACT_INPUTS' $xmlInput
   $content=[IO.File]::ReadAllText((Join-Path $output ('xml-count-'+$count+'.log')));if($content.Contains(' PIN ') -or $content.Contains('PROJECT_ASSEMBLY ') -or $content.Contains('CONTEXT_COLLECTION_BEGIN')){Fail 'XML_COUNT_NEGATIVE_REACHED_PINS_LOAD'}
  }
 }
$active=@(Get-CimInstance Win32_Process -Filter "Name='ConstructorProbe.exe' OR Name='ProjectConstructorProbe.exe' OR Name='ProjectFactoryBindingProbe.exe' OR Name='ProjectFactoryContainerProbe.exe' OR Name='NeutralStaHost.exe'" | Where-Object {$_.ExecutablePath -and ($_.ExecutablePath -ieq (Join-Path $build 'bin\ConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectFactoryBindingProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectFactoryContainerProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\NeutralStaHost.exe'))})
if($active.Count -ne 0){Fail 'OWNED_CHILD_REMAINS'}
 Write-Output ('OWNED_XML_FOCUSED_SUITE PASS session='+$ExpectedSession+' original=NOT_RUN hardware=NOT_RUN actual-XML-load=NOT_RUN');exit 0
}

$invalidLog='\\owned-invalid\share\never.log'
foreach($oldMode in @('neutral','original','original-initialize','owned-initialize','owned-Success','owned-Throw','owned-Missing','owned-Hang','owned-Flood','project-enum-metadata','original-project','owned-project','original-project-store-id','owned-project-store-id','original-factory','owned-factory','original-factory-bcl9','owned-factory-bcl9','original-factory-empty-container','owned-factory-empty-container','original-trace-encryptor','owned-trace-encryptor','original-factory-registered-trace','owned-factory-registered-trace','original-project-genuine-factory','owned-project-genuine-factory','original-project-genuine-factory-resources')){
 Run ('resource-selector-refused-'+$oldMode) $launcher @('--seconds','1','--mode',$oldMode,'--input',$fixtures,'--log',(Join-Path $output ('selector-'+$oldMode+'.log')),'--owned-resource-case','getter') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
foreach($badCase in @('','unknown','factory|project','Getter')){
 Run ('resource-selector-invalid-'+[string]$badCase.Length) $launcher @('--seconds','1','--mode','owned-project-genuine-factory-resources','--input',$fixtures,'--log',(Join-Path $output ('selector-invalid-'+[string]$badCase.Length+'.log')),'--owned-resource-case',$badCase) 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
$resourceExe=Join-Path $build 'bin\ProjectFactoryContainerProbe.exe'
Run 'resource-selector-direct-original' $resourceExe @('--mode','original-project-genuine-factory-resources','--input',$fixtures,'--log',(Join-Path $output 'selector-direct-original.log'),'--owned-resource-case','getter') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'resource-selector-direct-unknown' $resourceExe @('--mode','owned-project-genuine-factory-resources','--input',$fixtures,'--log',(Join-Path $output 'selector-direct-unknown.log'),'--owned-resource-case','unknown') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'resource-selector-duplicate' $launcher @('--seconds','1','--mode','owned-project-genuine-factory-resources','--input',$fixtures,'--log',(Join-Path $output 'selector-duplicate.log'),'--owned-resource-case','getter','--owned-resource-case','project') 127 'LAUNCH_ARGUMENTS'
if(Get-ChildItem -LiteralPath $output -Filter 'selector*.log'){Fail 'RESOURCE_SELECTOR_CREATED_CHILD_LOG'}
foreach($oldUndoMode in @('neutral','original','original-initialize','owned-initialize','owned-Success','owned-Throw','owned-Missing','owned-Hang','owned-Flood','project-enum-metadata','original-project','owned-project','original-project-store-id','owned-project-store-id','original-factory','owned-factory','original-factory-bcl9','owned-factory-bcl9','original-factory-empty-container','owned-factory-empty-container','original-trace-encryptor','owned-trace-encryptor','original-factory-registered-trace','owned-factory-registered-trace','original-project-genuine-factory','owned-project-genuine-factory','original-project-genuine-factory-resources','owned-project-genuine-factory-resources','original-project-undo-provider-denied')){
 Run ('undo-selector-refused-'+$oldUndoMode) $launcher @('--seconds','1','--mode',$oldUndoMode,'--input',$fixtures,'--log',(Join-Path $output ('undo-selector-'+$oldUndoMode+'.log')),'--owned-undo-case','root') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
Run 'undo-selector-unknown' $launcher @('--seconds','1','--mode','owned-project-undo-provider-denied','--input',$fixtures,'--log',(Join-Path $output 'undo-selector-unknown.log'),'--owned-undo-case','unknown') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'undo-selector-old-control' $launcher @('--seconds','1','--mode','owned-project-undo-provider-denied','--input',$fixtures,'--log',(Join-Path $output 'undo-selector-old-control.log'),'--owned-resource-case','getter') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'undo-selector-direct-original' $resourceExe @('--mode','original-project-undo-provider-denied','--input',$fixtures,'--log',(Join-Path $output 'undo-selector-direct-original.log'),'--owned-undo-case','root') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'undo-selector-direct-unknown' $resourceExe @('--mode','owned-project-undo-provider-denied','--input',$fixtures,'--log',(Join-Path $output 'undo-selector-direct-unknown.log'),'--owned-undo-case','unknown') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'undo-deadline' $launcher @('--seconds','16','--mode','owned-project-undo-provider-denied','--input',$fixtures,'--log',(Join-Path $output 'undo-selector-deadline.log')) 127 'FACTORY_CONTAINER_DEADLINE'
if(Get-ChildItem -LiteralPath $output -Filter 'undo-selector*.log'){Fail 'UNDO_SELECTOR_CREATED_LOG'}
foreach($oldFactoryMode in @('neutral','original','original-initialize','owned-initialize','owned-Success','owned-Throw','owned-Missing','owned-Hang','owned-Flood','project-enum-metadata','original-project','owned-project','original-project-store-id','owned-project-store-id','original-factory','owned-factory','original-factory-bcl9','owned-factory-bcl9','original-factory-empty-container','owned-factory-empty-container','original-trace-encryptor','owned-trace-encryptor','original-factory-registered-trace','owned-factory-registered-trace','original-project-genuine-factory','owned-project-genuine-factory','original-project-genuine-factory-resources','owned-project-genuine-factory-resources','original-project-undo-provider-denied','owned-project-undo-provider-denied','original-project-undo-provider-factory')){
 Run ('factory-selector-refused-'+$oldFactoryMode) $launcher @('--seconds','1','--mode',$oldFactoryMode,'--input',$fixtures,'--log',(Join-Path $output ('factory-selector-'+$oldFactoryMode+'.log')),'--owned-factory-case','null') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
Run 'factory-selector-unknown' $launcher @('--seconds','1','--mode','owned-project-undo-provider-factory','--input',$fixtures,'--log',(Join-Path $output 'factory-selector-unknown.log'),'--owned-factory-case','unknown') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'factory-selector-old-control' $launcher @('--seconds','1','--mode','owned-project-undo-provider-factory','--input',$fixtures,'--log',(Join-Path $output 'factory-selector-old-control.log'),'--owned-undo-case','root') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'factory-selector-direct-original' $resourceExe @('--mode','original-project-undo-provider-factory','--input',$fixtures,'--log',(Join-Path $output 'factory-selector-direct-original.log'),'--owned-factory-case','null') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'factory-deadline' $launcher @('--seconds','16','--mode','owned-project-undo-provider-factory','--input',$fixtures,'--log',(Join-Path $output 'factory-selector-deadline.log')) 127 'FACTORY_CONTAINER_DEADLINE'
if(Get-ChildItem -LiteralPath $output -Filter 'factory-selector*.log'){Fail 'FACTORY_SELECTOR_CREATED_LOG'}
foreach($oldCacheMode in @('neutral','original','original-initialize','owned-initialize','owned-Success','owned-Throw','owned-Missing','owned-Hang','owned-Flood','project-enum-metadata','original-project','owned-project','original-project-store-id','owned-project-store-id','original-factory','owned-factory','original-factory-bcl9','owned-factory-bcl9','original-factory-empty-container','owned-factory-empty-container','original-trace-encryptor','owned-trace-encryptor','original-factory-registered-trace','owned-factory-registered-trace','original-project-genuine-factory','owned-project-genuine-factory','original-project-genuine-factory-resources','owned-project-genuine-factory-resources','original-project-undo-provider-denied','owned-project-undo-provider-denied','original-project-undo-provider-factory','owned-project-undo-provider-factory','original-project-large-string-cache-denied')){
 Run ('cache-selector-refused-'+$oldCacheMode) $launcher @('--seconds','1','--mode',$oldCacheMode,'--input',$fixtures,'--log',(Join-Path $output ('cache-selector-'+$oldCacheMode+'.log')),'--owned-cache-case','second') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
Run 'cache-selector-unknown' $launcher @('--seconds','1','--mode','owned-project-large-string-cache-denied','--input',$fixtures,'--log',(Join-Path $output 'cache-selector-unknown.log'),'--owned-cache-case','unknown') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'cache-selector-old-control' $launcher @('--seconds','1','--mode','owned-project-large-string-cache-denied','--input',$fixtures,'--log',(Join-Path $output 'cache-selector-old-control.log'),'--owned-factory-case','null') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'cache-selector-direct-original' $resourceExe @('--mode','original-project-large-string-cache-denied','--input',$fixtures,'--log',(Join-Path $output 'cache-selector-direct-original.log'),'--owned-cache-case','second') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'cache-deadline' $launcher @('--seconds','16','--mode','owned-project-large-string-cache-denied','--input',$fixtures,'--log',(Join-Path $output 'cache-selector-deadline.log')) 127 'FACTORY_CONTAINER_DEADLINE'
if(Get-ChildItem -LiteralPath $output -Filter 'cache-selector*.log'){Fail 'CACHE_SELECTOR_CREATED_LOG'}

foreach($oldContextMode in @('original-project-context-denied','original-project-large-string-cache-denied','owned-project-large-string-cache-denied','original-project-genuine-factory','owned-project-genuine-factory','owned-project-undo-provider-factory','owned-project-genuine-factory-resources','owned-Success','neutral')){
 Run ('context-selector-'+$oldContextMode) $launcher @('--seconds','1','--mode',$oldContextMode,'--input',$fixtures,'--log',(Join-Path $output ('context-selector-'+$oldContextMode+'.log')),'--owned-context-case','collection-parent') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
}
foreach($badContextCase in @('','unknown','parent|context')){Run ('context-selector-invalid-'+$badContextCase.Length) $launcher @('--seconds','1','--mode','owned-project-context-denied','--input',$fixtures,'--log',(Join-Path $output ('context-selector-invalid-'+$badContextCase.Length+'.log')),'--owned-context-case',$badContextCase) 127 'OWNED_RESOURCE_CASE_ARGUMENTS'}
Run 'context-selector-direct-original' $resourceExe @('--mode','original-project-context-denied','--input',$fixtures,'--log',(Join-Path $output 'context-selector-direct-original.log'),'--owned-context-case','collection-parent') 2 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'context-selector-duplicate' $launcher @('--seconds','1','--mode','owned-project-context-denied','--input',$fixtures,'--log',(Join-Path $output 'context-selector-duplicate.log'),'--owned-context-case','collection-parent','--owned-context-case','project-context') 127 'LAUNCH_ARGUMENTS'
Run 'context-selector-old-cache' $launcher @('--seconds','1','--mode','owned-project-context-denied','--input',$fixtures,'--log',(Join-Path $output 'context-selector-old-cache.log'),'--owned-cache-case','second') 127 'OWNED_RESOURCE_CASE_ARGUMENTS'
Run 'context-deadline' $launcher @('--seconds','16','--mode','owned-project-context-denied','--input',$fixtures,'--log',(Join-Path $output 'context-selector-deadline.log')) 127 'FACTORY_CONTAINER_DEADLINE'
if(Get-ChildItem -LiteralPath $output -Filter 'context-selector*.log'){Fail 'CONTEXT_SELECTOR_CREATED_LOG'}

Run 'unc-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log',$invalidLog) 127 'LOCAL_PATH_REQUIRED'
Run 'drive-relative-log' $launcher @('--seconds','1','--mode','owned-Success','--input',$fixtures,'--log','C:owned-never.log') 127 'LOCAL_PATH_REQUIRED'
Run 'excessive-deadline' $launcher @('--seconds','31','--mode','owned-Success','--input',$fixtures,'--log',(Join-Path $output 'never.log')) 127 'LAUNCH_ARGUMENTS'
Run 'fixture-excessive-deadline' $launcher @('--seconds','16','--mode','owned-project-store-id','--input',$fixtures,'--log',(Join-Path $output 'fixture-never.log')) 127 'PROJECT_FIXTURE_DEADLINE'
Run 'factory-excessive-deadline' $launcher @('--seconds','16','--mode','owned-factory','--input',$fixtures,'--log',(Join-Path $output 'factory-never.log')) 127 'FACTORY_DIAGNOSTIC_DEADLINE'
Run 'binding-excessive-deadline' $launcher @('--seconds','16','--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'binding-never.log')) 127 'FACTORY_BINDING_DEADLINE'
Run 'project-scope-excessive-deadline' $launcher @('--seconds','16','--mode','owned-project-genuine-factory','--input',$fixtures,'--log',(Join-Path $output 'project-scope-never.log')) 127 'FACTORY_CONTAINER_DEADLINE'
Run 'registered-trace-excessive-deadline' $launcher @('--seconds','16','--mode','owned-factory-registered-trace','--input',$fixtures,'--log',(Join-Path $output 'registered-trace-never.log')) 127 'FACTORY_CONTAINER_DEADLINE'
Run 'trace-excessive-deadline' $launcher @('--seconds','16','--mode','owned-trace-encryptor','--input',$fixtures,'--log',(Join-Path $output 'trace-never.log')) 127 'FACTORY_CONTAINER_DEADLINE'
Run 'empty-excessive-deadline' $launcher @('--seconds','16','--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-never.log')) 127 'FACTORY_CONTAINER_DEADLINE'
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
 Case 'session0-factory' 'owned-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-factory' 'original-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-factory-binding' 'owned-factory-bcl9' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-factory-binding' 'original-factory-bcl9' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-empty-container' 'owned-factory-empty-container' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-empty-container' 'original-factory-empty-container' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-cache' 'owned-project-large-string-cache-denied' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-cache' 'original-project-large-string-cache-denied' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-factory-provider' 'owned-project-undo-provider-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-factory-provider' 'original-project-undo-provider-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-undo' 'owned-project-undo-provider-denied' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-undo' 'original-project-undo-provider-denied' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-project-resources' 'owned-project-genuine-factory-resources' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-project-resources' 'original-project-genuine-factory-resources' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-project-scope' 'owned-project-genuine-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-project-scope' 'original-project-genuine-factory' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-registered-trace' 'owned-factory-registered-trace' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-registered-trace' 'original-factory-registered-trace' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-trace' 'owned-trace-encryptor' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
 Case 'session0-original-trace' 'original-trace-encryptor' 4 'child-exit-confirmed=true' 'INTERACTIVE_SESSION_REQUIRED'
}else{
 Case 'neutral' 'neutral' 0 'child-exit-confirmed=true' 'NEUTRAL_RESULT PASS' $build
 Case 'success' 'owned-Success' 0 'OWNED_FIXTURE_SUCCESS' 'OWNED_CONSTRUCTOR_RETURN'
 if($administrator){
  Case 'initialize-admin' 'owned-initialize' 5 'child-exit-confirmed=true' 'INITIALIZE_NONADMIN_REQUIRED'
  Case 'project-admin' 'owned-project' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-admin' 'original-project' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'project-store-id-admin' 'owned-project-store-id' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-store-id-admin' 'original-project-store-id' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'factory-admin' 'owned-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-factory-admin' 'original-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'factory-binding-admin' 'owned-factory-bcl9' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-factory-binding-admin' 'original-factory-bcl9' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'empty-container-admin' 'owned-factory-empty-container' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-empty-container-admin' 'original-factory-empty-container' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'cache-admin' 'owned-project-large-string-cache-denied' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-cache-admin' 'original-project-large-string-cache-denied' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'factory-provider-admin' 'owned-project-undo-provider-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-factory-provider-admin' 'original-project-undo-provider-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'undo-admin' 'owned-project-undo-provider-denied' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-undo-admin' 'original-project-undo-provider-denied' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'project-resources-admin' 'owned-project-genuine-factory-resources' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-resources-admin' 'original-project-genuine-factory-resources' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'project-scope-admin' 'owned-project-genuine-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-project-scope-admin' 'original-project-genuine-factory' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'registered-trace-admin' 'owned-factory-registered-trace' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-registered-trace-admin' 'original-factory-registered-trace' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'trace-admin' 'owned-trace-encryptor' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
  Case 'original-trace-admin' 'original-trace-encryptor' 5 'child-exit-confirmed=true' 'PROJECT_NONADMIN_REQUIRED'
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
  Case 'factory-proxy' 'owned-factory' 0 'child-exit-confirmed=true' 'OWNED_FACTORY_CONSTRUCTOR_PASS'
  $factoryLog=[IO.File]::ReadAllText((Join-Path $output 'factory-proxy.log'))
  foreach($marker in @('OWNED_FACTORY_REFERENCE_POLICY_PASS old-modes=denied','OWNED_FACTORY_CONTRACT_PASS negatives=5 token-policy-negatives=2 before-construction=true','FACTORY_CONSTRUCTOR_END returned=True denied-requests=0 qualification=NOT_ESTABLISHED','FACTORY_CONSTRUCTOR_END returned=False denied-requests=1 qualification=NOT_ESTABLISHED','FACTORY_CONSTRUCTOR_END returned=True denied-requests=2 qualification=NOT_ESTABLISHED')){if(-not $factoryLog.Contains($marker)){Fail 'OWNED_FACTORY_QUALIFICATION'}}
  if($factoryLog.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $factoryLog.Contains('PROJECT_ID_CONSTRUCTOR_BEGIN') -or $factoryLog.Contains('PROJECT_SERVICE_FIXTURE')){Fail 'FACTORY_NOT_CONSTRUCTOR_ONLY'}
  if(-not $factoryLog.Contains('CONTAINER_BINDING_POLICY_PASS enabled=False exact4=true') -or -not $factoryLog.Contains('OWNED_FACTORY_LEGACY_POLICY_PASS version8=UNCHANGED')){Fail 'LEGACY_CONFIG_POLICY'}
  Case 'factory-binding-policy' 'owned-factory-bcl9' 0 'child-exit-confirmed=true' 'FACTORY_BINDING_POLICY_PASS exact8-to9=true'
  $bindingLog=[IO.File]::ReadAllText((Join-Path $output 'factory-binding-policy.log'))
  if(-not $bindingLog.Contains('CONTAINER_BINDING_POLICY_PASS enabled=False exact4=true') -or -not $bindingLog.Contains('OWNED_FACTORY_CONSTRUCTOR_PASS') -or $bindingLog.Contains('FACTORY_BINDING_ASSEMBLY') -or $bindingLog.Contains('FACTORY_BINDING_RESOLVE ') -or $bindingLog.Contains('PROJECT_SERVICE_FIXTURE')){Fail 'BINDING_OWNED_TARGET_LOAD_OR_RESULT'}
  Case 'undo-original-missing19' 'original-project-undo-provider-denied' 2 'child-exit-confirmed=true' 'PROJECT_EXACT_INPUTS'
  $undoInputNegative=[IO.File]::ReadAllText((Join-Path $output 'undo-original-missing19.log'))
  if($undoInputNegative -match ' PIN |PLATFORM_FACADE_PIN|VALUETUPLE_REFERENCE_PIN|PROJECT_ASSEMBLY |UNDO_API_READY'){Fail 'UNDO_INPUT_NEGATIVE_LOADED'}
  Case 'undo-positive' 'owned-project-undo-provider-denied' 0 'child-exit-confirmed=true' 'OWNED_UNDO_LIFETIME_PASS'
  $undoLog=[IO.File]::ReadAllText((Join-Path $output 'undo-positive.log'))
  foreach($marker in @('OWNED_UNDO_API_PASS shape-negatives=10 token-negatives=4 callbacks=3 invalid-null-counted=True saturated=17 legacy-invalid-count=0 before-effects=True','UNDO_REGISTER_END external-ownership=True exact-interface=True application-registrations=2 before-build=True','UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True denied=0','order=trace-ctor>builder>trace-registration>trace-external>registration>undo-external>build>factory>project>project-dispose>container-dispose>trace-dispose>')){if(-not $undoLog.Contains($marker)){Fail 'UNDO_POSITIVE'}}
  foreach($selected in $undoCases){
   $name='undo-'+$selected;$caseLog=Join-Path $output ($name+'.log');$expected=if($selected -in @('register-null','external-null')){10}else{2}
   Run $name $launcher @('--seconds','15','--mode','owned-project-undo-provider-denied','--input',$fixtures,'--log',$caseLog,'--owned-undo-case',$selected) $expected 'child-exit-confirmed=true'
   $denial=[IO.File]::ReadAllText($caseLog)
   if(-not $denial.Contains('UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True') -or -not $denial.Contains('UNDO_OTHER_FINAL_AUDIT after-all-managed-disposal=True') -or -not $denial.Contains('TRACE_DISPOSE_END returned=True')){Fail ('UNDO_FINAL_LIFETIMES_'+$selected)}
   if($expected -eq 2 -and (-not $denial.Contains('OWNED_UNDO_DENIAL case='+$selected) -or -not $denial.Contains('UNDO_PROVIDER_FINAL_DENIAL'))){Fail ('UNDO_CAUGHT_DENIAL_'+$selected)}
   if($selected -eq 'final-audit' -and (-not $denial.Contains('phase=final-audit caught=True counter-sticky=True window-restored=0 dependency-flags=False') -or $denial.Contains('UNDO_PROVIDER_DENIED_POST_DISPOSE'))){Fail 'UNDO_COUNTER_ONLY_FINAL_ORACLE'}
   if($selected -eq 'saturation' -and -not $denial.Contains('after-all-managed-disposal=True denied=17')){Fail 'UNDO_SATURATION'}
   if($selected -in @('nested','resource') -and -not $denial.Contains('OWNED_UNDO_CALLBACK_REFUSAL case='+$selected+' caught=True sticky=True window=0 attempts=1')){Fail 'UNDO_CALLBACK_RESOURCE_EXCLUSION'}
   if($selected -in @('registration','register-null','external-null','trace-ctor','builder') -and $denial.Contains('EMPTY_CONTAINER_BUILD_BEGIN')){Fail 'UNDO_DENIAL_BEFORE_BUILD'}
   if($selected -in @('registration','register-null','external-null','trace-ctor','builder','build','factory') -and $denial.Contains('PROJECT_SCOPE_CONSTRUCTOR_BEGIN')){Fail 'UNDO_DENIAL_BEFORE_PROJECT'}
   if($selected -eq 'error' -and $denial.Contains('PROJECT_SCOPE_DISPOSE_BEGIN')){Fail 'UNDO_NO_DISPOSE_WITHOUT_RETURN'}
  }
  Case 'factory-provider-original-missing19' 'original-project-undo-provider-factory' 2 'child-exit-confirmed=true' 'PROJECT_EXACT_INPUTS'
  $factoryInput=[IO.File]::ReadAllText((Join-Path $output 'factory-provider-original-missing19.log'))
  if($factoryInput -match ' PIN |PLATFORM_FACADE_PIN|VALUETUPLE_REFERENCE_PIN|PROJECT_ASSEMBLY |UNDO_API_READY'){Fail 'FACTORY_PROVIDER_INPUT_NEGATIVE_LOADED'}
  Case 'factory-provider-positive' 'owned-project-undo-provider-factory' 0 'child-exit-confirmed=true' 'OWNED_FACTORY_LIFETIME_PASS'
  $factoryLog=[IO.File]::ReadAllText((Join-Path $output 'factory-provider-positive.log'))
  foreach($marker in @('OWNED_FACTORY_API_PASS negatives=7 before-effects=True','UNDO_FACTORY_REGISTER_END exact-two-registrations=True sharing=SingleInstance external-ownership=True before-build=True','OWNED_FACTORY_CALLBACK_EXCLUDED window=0 suspended=True','UNDO_FACTORY_FINAL_AUDIT after-all-managed-disposal=True entries=1 successes=1 faulted=False','UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True denied=0','order=trace-ctor>builder>trace-registration>trace-external>registration>single>external>build>factory>after-activation>project>project-dispose>container-dispose>trace-dispose>')){if(-not$factoryLog.Contains($marker)){Fail 'FACTORY_PROVIDER_POSITIVE'}}
  foreach($selected in $factoryCases){
   $name='factory-provider-'+$selected;$caseLog=Join-Path $output ($name+'.log');$expected=if($selected -in @('project-activation','no-activation')){0}else{2}
   Run $name $launcher @('--seconds','15','--mode','owned-project-undo-provider-factory','--input',$fixtures,'--log',$caseLog,'--owned-factory-case',$selected) $expected 'child-exit-confirmed=true'
   $result=[IO.File]::ReadAllText($caseLog)
   foreach($marker in @('UNDO_FACTORY_FINAL_AUDIT after-all-managed-disposal=True','UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True','UNDO_FACTORY_OTHER_FINAL_AUDIT','TRACE_DISPOSE_END returned=True')){if(-not$result.Contains($marker)){Fail ('FACTORY_PROVIDER_ALL_FINALS_'+$selected)}}
   if($expected-eq0 -and -not$result.Contains('OWNED_FACTORY_LIFETIME_PASS')){Fail ('FACTORY_PROVIDER_ALLOWED_'+$selected)}
   if($expected-eq2 -and (-not$result.Contains('OWNED_FACTORY_REFUSAL case='+$selected) -or (-not$result.Contains('UNDO_FACTORY_FINAL_DENIAL') -and -not$result.Contains('UNDO_PROVIDER_FINAL_DENIAL')))){Fail ('FACTORY_PROVIDER_STICKY_'+$selected)}
   if($selected-eq'final-audit' -and (-not$result.Contains('entries=1 successes=0 faulted=True') -or -not$result.Contains('window-restored=0 dependency-flags=False') -or -not$result.Contains('UNDO_FACTORY_FINAL_DENIAL'))){Fail 'FACTORY_PROVIDER_COUNTER_ONLY_ORACLE'}
   if($selected-eq'nested' -and -not$result.Contains('OWNED_FACTORY_NESTED_REFUSAL sticky=True window=0 suspended=True aggregate-unchanged=True')){Fail 'FACTORY_PROVIDER_NESTED_EXCLUSION'}
   if($selected-eq'saturation' -and -not$result.Contains('entries=2 successes=1 faulted=True')){Fail 'FACTORY_PROVIDER_SATURATION'}
   if($selected-in@('get','new','root') -and (-not$result.Contains('UNDO_PROVIDER_SERVICE_DENIED number=1') -or -not$result.Contains('denied=1'))){Fail 'FACTORY_PROVIDER_ALL_THREE_DENIED'}
   if($selected-in@('trace-ctor','builder','registration','single','external') -and $result.Contains('EMPTY_CONTAINER_BUILD_BEGIN')){Fail 'FACTORY_PROVIDER_EARLY_FAULT_BEFORE_BUILD'}
   if($selected-in@('null','wrong-type','second','saturation','nested','off-thread','recorder','resource','factory','trace-ctor','builder','registration','single','external','build') -and $result.Contains('PROJECT_SCOPE_CONSTRUCTOR_BEGIN')){Fail 'FACTORY_PROVIDER_FAULT_BEFORE_PROJECT'}
   if($selected-eq'error' -and $result.Contains('PROJECT_SCOPE_DISPOSE_BEGIN')){Fail 'FACTORY_PROVIDER_NO_DISPOSE_WITHOUT_RETURN'}
  }
  Case 'cache-original-missing19' 'original-project-large-string-cache-denied' 2 'child-exit-confirmed=true' 'PROJECT_EXACT_INPUTS'
  $cacheInput=[IO.File]::ReadAllText((Join-Path $output 'cache-original-missing19.log'))
  if($cacheInput -match ' PIN |PLATFORM_FACADE_PIN|VALUETUPLE_REFERENCE_PIN|PROJECT_ASSEMBLY |STREAM_DECLARATIONS_READY'){Fail 'CACHE_INPUT_NEGATIVE_LOADED'}

  Case 'context-original-missing19' 'original-project-context-denied' 2 'child-exit-confirmed=true' 'PROJECT_EXACT_INPUTS'
  Case 'context-positive' 'owned-project-context-denied' 0 'child-exit-confirmed=true' 'OWNED_CONTEXT_LIFETIME_PASS'
  $contextPositive=[IO.File]::ReadAllText((Join-Path $output 'context-positive.log'))
  foreach($marker in @('OWNED_CONTEXT_API_PASS negatives=8','CONTEXT_COLLECTION_END returned=True','CONTEXT_PROJECT_END returned=True','cache-reference=1 factory-success=1','CONTEXT_FINAL_AUDIT phase=AFTER_ALL_MANAGED_CLEANUP parent=0/False context=0/False importer=0/False','CONTEXT_FINAL_AUDIT phase=PROCESS_FINAL')){if(-not$contextPositive.Contains($marker)){Fail 'CONTEXT_POSITIVE'}}
  foreach($selected in $contextCases){
   $name='context-'+$selected;$caseLog=Join-Path $output ($name+'.log')
   $expected=if($selected-in@('collection-throw','dispose-begin-log-fault')){10}else{2}
   Run $name $launcher @('--seconds','15','--mode','owned-project-context-denied','--input',$fixtures,'--log',$caseLog,'--owned-context-case',$selected) $expected 'child-exit-confirmed=true'
   $result=[IO.File]::ReadAllText($caseLog)
   foreach($marker in @('CONTEXT_FINAL_AUDIT phase=AFTER_ALL_MANAGED_CLEANUP','CONTEXT_FINAL_AUDIT phase=PROCESS_FINAL','TRACE_DISPOSE_END returned=True')){if(-not$result.Contains($marker)){Fail ('CONTEXT_ALL_FINALS_'+$selected)}}
   if($selected-notin@('collection-throw','dispose-begin-log-fault') -and -not$result.Contains('CONTEXT_ARGUMENT_FINAL_DENIAL')){Fail ('CONTEXT_STICKY_'+$selected)}
   if($selected-eq'parent-getter-reentry' -and -not$result.Contains('OWNED_CONTEXT_GETTER_REENTRY_PASS reads=16 entries=17 hard-cutoff=1 sentinel=False outer-restored=True aggregate-unchanged=True')){Fail 'CONTEXT_GETTER_HARD_CUTOFF'}
   if($selected-eq'dispose-begin-log-fault' -and (-not$result.Contains('OWNED_CONTEXT_DISPOSE_BEGIN_LOG_FAULT injected=True') -or -not$result.Contains('OWNED_CONTEXT_DISPOSE_LOG_PASS project=1 dispose=1 container-dispose=1 trace-dispose=1 after-all-managed-cleanup=True') -or ([regex]::Matches($result,'CONTEXT_PROJECT_DISPOSE_END attempts=1')).Count-ne1)){Fail 'CONTEXT_DISPOSE_LOG_ATTEMPT'}
   if($selected-in@('collection-parent','collection-importer','collection-throw') -and $result.Contains('CONTEXT_PROJECT_BEGIN')){Fail 'CONTEXT_COLLECTION_STOP'}
   if($selected.StartsWith('nested-') -and -not$result.Contains('OWNED_CONTEXT_NESTED_PASS outer-exclusion-preserved=True aggregate-unchanged=True')){Fail 'CONTEXT_NESTED_EXCLUSION'}
   if($selected.EndsWith('-only') -and -not$result.Contains('OWNED_CONTEXT_AUDIT_MUTANT')){Fail 'CONTEXT_INDEPENDENT_MUTANT'}
   if($selected.EndsWith('-saturation') -and -not$result.Contains($(if($selected.StartsWith('parent-')){'parent=17/True'}elseif($selected.StartsWith('context-')){'context=17/True'}else{'importer=2/True'}))){Fail 'CONTEXT_SATURATION'}
  }

  Case 'cache-positive' 'owned-project-large-string-cache-denied' 0 'child-exit-confirmed=true' 'OWNED_CACHE_LIFETIME_PASS'
  $cacheLog=[IO.File]::ReadAllText((Join-Path $output 'cache-positive.log'))
  foreach($marker in @('OWNED_CACHE_API_PASS negatives=5 before-effects=True','LARGE_STRING_CACHE_REFERENCE reply=PREPARED same-preexisting-reference=True','CACHE_FINAL_AUDIT after-all-managed-disposal=True entries=3 attempts=1 replies=1 faulted=False stream-denied=0','UNDO_FACTORY_FINAL_AUDIT after-all-managed-disposal=True entries=1 successes=1 faulted=False','UNDO_FACTORY_OTHER_FINAL_AUDIT kim-denied=0 store-denied=0','OWNED_CACHE_LIFETIME_PASS entries=3 attempts=1 replies=1 same-preexisting-reference=True exact-two-registrations=True')){if(-not$cacheLog.Contains($marker)){Fail 'CACHE_TYPED_REFERENCE_POSITIVE'}}
  foreach($selected in $cacheCases){
   $name='cache-'+$selected;$caseLog=Join-Path $output ($name+'.log');$expected=if($selected-in@('total-boundary','mutable','mutable-id')){0}else{2}
   Run $name $launcher @('--seconds','15','--mode','owned-project-large-string-cache-denied','--input',$fixtures,'--log',$caseLog,'--owned-cache-case',$selected) $expected 'child-exit-confirmed=true'
   $result=[IO.File]::ReadAllText($caseLog)
   foreach($marker in @('CACHE_FINAL_AUDIT after-all-managed-disposal=True','UNDO_FACTORY_FINAL_AUDIT after-all-managed-disposal=True','UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True','UNDO_FACTORY_OTHER_FINAL_AUDIT','TRACE_DISPOSE_END returned=True')){if(-not$result.Contains($marker)){Fail ('CACHE_ALL_LIFETIMES_'+$selected)}}
   if($expected-eq0 -and -not$result.Contains('OWNED_CACHE_LIFETIME_PASS')){Fail ('CACHE_POSITIVE_'+$selected)}
   if($expected-eq2 -and (-not$result.Contains('OWNED_CACHE_REFUSAL case='+$selected) -or -not$result.Contains('CACHE_STREAM_FINAL_DENIAL'))){Fail ('CACHE_CAUGHT_STICKY_'+$selected)}
   if($selected-eq'total-boundary' -and -not$result.Contains('entries=16 attempts=1 replies=1 faulted=False stream-denied=0')){Fail 'CACHE_SHARED16_BOUNDARY'}
   if($selected-eq'total-limit' -and -not$result.Contains('entries=17')){Fail 'CACHE_SHARED17_SATURATION'}
   if($selected-eq'second' -and -not$result.Contains('attempts=2 replies=1 faulted=True')){Fail 'CACHE_STRICT_ONE_REFERENCE'}
   if($selected-in@('mutable','mutable-id') -and -not$result.Contains('OWNED_CACHE_MUTABLE_SNAPSHOT_PASS case='+$selected+' original-method-args-context-reads=1')){Fail 'CACHE_ORIGINAL_NOT_REREAD'}
   if($selected-eq'mutable-id' -and -not$result.Contains('protected-id-core=True')){Fail 'CACHE_MUTABLE_ID_CORE_REGRESSION'}
   if($selected-eq'message-reentry' -and -not$result.Contains('replies=0 wrapper-fault=True stream-denied=0 dependency-flags=False')){Fail 'CACHE_CONTEXT_CAUGHT_REENTRY'}
   if($selected-eq'nested' -and -not$result.Contains('OWNED_CACHE_NESTED_REFUSAL outer-exclusion-preserved=True sticky=True')){Fail 'CACHE_NESTED_EXCLUSION'}
   if($selected-eq'wrapper-final' -and (-not$result.Contains('wrapper-fault=True stream-denied=0 dependency-flags=False') -or -not$result.Contains('kim-denied=0 store-denied=0'))){Fail 'CACHE_WRAPPER_FAULT_ONLY_ORACLE'}
   if($selected-eq'stream-final' -and (-not$result.Contains('wrapper-fault=False stream-denied=1 dependency-flags=False') -or -not$result.Contains('kim-denied=0 store-denied=0'))){Fail 'CACHE_STREAM_COUNTER_ONLY_ORACLE'}
   if($selected-in@('stream-recorder','stream-restore') -and -not$result.Contains('stream-denied=1')){Fail 'STREAM_HANDLING_FAULT_COUNTED'}
   if($selected-eq'wrapper-restore' -and -not$result.Contains('replies=0 wrapper-fault=True')){Fail 'CACHE_RESTORE_FAULT_COUNTED'}
   if($selected-eq'error' -and $result.Contains('PROJECT_SCOPE_DISPOSE_BEGIN')){Fail 'CACHE_NO_PROJECT_DISPOSE_WITHOUT_RETURN'}
   if($result.Contains('OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED') -or $result.Contains('OWNED_CUSTOM_METHOD_METADATA_INSPECTED')){Fail 'CACHE_UNTRUSTED_VALUE_OR_METHOD_INSPECTION'}
  }
  Case 'project-resources-lifetime'  'owned-project-genuine-factory-resources' 0 'child-exit-confirmed=true' 'OWNED_PROJECT_RESOURCE_LIFETIME_PASS'
  $resourceLog=[IO.File]::ReadAllText((Join-Path $output 'project-resources-lifetime.log'))
  foreach($marker in @('shared8=3+5 ninth-saturates=True','actual-owner=NOT_PROVIDED modeled=True original=NOT_RUN','phase=getter window=0 callback-suspended=True','phase=project-after-getter window=2 callback-suspended=False','phase=validation window=0','phase=logging window=0','phase=project-dispose window=0','phase=container-dispose window=0','phase=trace-dispose window=0','PROJECT_FACTORY_FINAL_AUDIT after-all-managed-disposal=True','PROJECT_RESOURCE_BUDGET_END active=False aggregate-probes=0')){if(-not $resourceLog.Contains($marker)){Fail 'PROJECT_RESOURCE_POSITIVE'} }
  foreach($selected in $resourceCases){
   $name='resource-refusal-'+$selected;$caseLog=Join-Path $output ($name+'.log')
   Run $name $launcher @('--seconds','15','--mode','owned-project-genuine-factory-resources','--input',$fixtures,'--log',$caseLog,'--owned-resource-case',$selected) 2 'child-exit-confirmed=true'
   $refusal=[IO.File]::ReadAllText($caseLog)
   if(-not $refusal.Contains('OWNED_RESOURCE_REFUSAL case='+$selected+' caught=True sticky=True') -or -not $refusal.Contains('PROJECT_DEPENDENCY_BLOCKER') -or (Get-Item -LiteralPath $caseLog).Length -gt 65536){Fail ('PROJECT_RESOURCE_STICKY_'+$selected)}
   if($selected -in @('getter','project','validation','logging','error','project-dispose','container-dispose','trace-dispose','factory','nested-window','nested-budget','nested-callback','off-thread') -and (-not $refusal.Contains('PROJECT_FACTORY_FINAL_AUDIT after-all-managed-disposal=True') -or -not $refusal.Contains('TRACE_DISPOSE_END returned=True'))){Fail ('PROJECT_RESOURCE_FINAL_AUDIT_'+$selected)}
   if($selected -eq 'factory' -and (-not $refusal.Contains('phase=provider window=0 callback-suspended=True') -or -not $refusal.Contains('phase=factory window=1 callback-suspended=False') -or -not $refusal.Contains('provider-denied=1'))){Fail 'RESOURCE_DENY_PROXY_SUSPEND_RESTORE'}
   if($selected -eq 'nested-callback' -and (-not $refusal.Contains('diagnostic=PROJECT_RESOURCE_NESTED_CALLBACK outer-window=0 outer-suspended=True aggregate-unchanged=True attempts=1') -or -not $refusal.Contains('phase=project-after-getter window=2 callback-suspended=False') -or $refusal.Contains('PROJECT_RESOURCE_CALLBACK_RESTORE'))){Fail 'RESOURCE_NESTED_CALLBACK_CAUGHT_STICKY'}
  }
  Case 'project-scope-lifetime'  'owned-project-genuine-factory' 0 'child-exit-confirmed=true' 'OWNED_PROJECT_SCOPE_PASS'
  $scopeLog=[IO.File]::ReadAllText((Join-Path $output 'project-scope-lifetime.log'))
  foreach($marker in @('OWNED_PROJECT_SCOPE_API_PASS negatives=7 token-negatives=4 before-effects=True public-dispose-map=True factory-assignability=True','behavior=trace-store-caught expected-failure=True project-dispose=1 container-dispose=1 trace-dispose=1 provider-denied=0 store-denied=1','behavior=trace-provider-caught expected-failure=True project-dispose=1 container-dispose=1 trace-dispose=1 provider-denied=1 store-denied=0','PROJECT_SCOPE_DISPOSE_END returned=False','PROJECT_FACTORY_FINAL_AUDIT after-all-managed-disposal=True','modeled-guards=6 stable-id=True actual-factory-reference=True')){if(-not $scopeLog.Contains($marker)){Fail 'PROJECT_SCOPE_OWNED_REGRESSION'}}
  if([regex]::Matches($scopeLog,'OWNED_PROJECT_SCOPE_CASE').Count -ne 17 -or [regex]::Matches($scopeLog,'PROJECT_SCOPE_CONSTRUCTOR_BEGIN').Count -ne 15 -or [regex]::Matches($scopeLog,'PROJECT_SCOPE_DISPOSE_BEGIN').Count -ne 14 -or [regex]::Matches($scopeLog,'PROJECT_FACTORY_FINAL_AUDIT').Count -ne 12 -or $scopeLog -match 'TRACE_PHASE original-type|PROJECT_PHASE original-fixture-type|metadata-only=True'){Fail 'PROJECT_SCOPE_OWNED_COUNTS'}
  Case 'registered-trace-lifetime'  'owned-factory-registered-trace' 0 'child-exit-confirmed=true' 'OWNED_REGISTERED_TRACE_PASS'
  $registeredLog=[IO.File]::ReadAllText((Join-Path $output 'registered-trace-lifetime.log'))
  foreach($marker in @('OWNED_REGISTERED_TRACE_API_PASS negatives=9 before-effects=True public-interface-internal-implementation=True exact-T-default-mapping=True','TRACE_REGISTER_END external-ownership=True','application-registrations=1','REGISTERED_TRACE_FINAL_AUDIT after-container-and-trace-disposal=True scope-returned=False provider-denied=1','behavior=trace-denial-caught expected-failure=True explicit-trace-dispose=1 container-dispose=1 provider-denied=1','REGISTERED_TRACE_LIFETIME_RETURNED_NOT_QUALIFIED')){if(-not $registeredLog.Contains($marker)){Fail 'REGISTERED_TRACE_OWNED_REGRESSION'}}
  if([regex]::Matches($registeredLog,'TRACE_CONSTRUCTOR_BEGIN').Count -ne 13 -or [regex]::Matches($registeredLog,'TRACE_DISPOSE_BEGIN').Count -ne 12 -or [regex]::Matches($registeredLog,'OWNED_REGISTERED_TRACE_CASE').Count -ne 13 -or $registeredLog -match 'explicit-other-sdk-calls=0|TRACE_PHASE original-type|PROJECT_CONSTRUCTOR_BEGIN|PROJECT_ID_CONSTRUCTOR_BEGIN|PROJECT_SERVICE_FIXTURE'){Fail 'REGISTERED_TRACE_OWNED_SCOPE'}
  Case 'trace-lifetime'  'owned-trace-encryptor' 0 'child-exit-confirmed=true' 'OWNED_TRACE_LIFETIME_PASS'
  $traceLog=[IO.File]::ReadAllText((Join-Path $output 'trace-lifetime.log'))
  foreach($marker in @('OWNED_TRACE_REFERENCE_POLICY_PASS','OWNED_TRACE_CONTRACT_PASS negatives=10 token-negatives=4 trap-attributes=0','TRACE_CONSTRUCTOR_END returned=True','TRACE_CONSTRUCTOR_END returned=False','TRACE_DISPOSE_END returned=False attempts=1','TRACE_LIFETIME_AUDIT passed=False','phase-guards=6 caught-guards=2','TRACE_CONSTRUCTOR_DISPOSE_RETURNED_NOT_QUALIFIED')){if(-not $traceLog.Contains($marker)){Fail 'TRACE_OWNED_REGRESSION'}}
  if([regex]::Matches($traceLog,'TRACE_CONSTRUCTOR_BEGIN').Count -ne 9 -or [regex]::Matches($traceLog,'TRACE_DISPOSE_BEGIN').Count -ne 8 -or $traceLog -match 'FACTORY_CONSTRUCTOR_BEGIN|EMPTY_CONTAINER_BUILDER_BEGIN|PROJECT_CONSTRUCTOR_BEGIN|PROJECT_ID_CONSTRUCTOR_BEGIN|PROJECT_SERVICE_FIXTURE|TRACE_PHASE original-type'){Fail 'TRACE_OWNED_SCOPE_ONCE'}
  Case 'empty-container-lifetime' 'owned-factory-empty-container' 0 'child-exit-confirmed=true' 'OWNED_EMPTY_CONTAINER_PASS'
  $emptyLog=[IO.File]::ReadAllText((Join-Path $output 'empty-container-lifetime.log'))
  foreach($marker in @('OWNED_EMPTY_API_PASS negatives=12 before-builder=true','EMPTY_CONTAINER_BUILD_BEGIN','options=None raw=0 application-registrations=0 defaults=RETAINED','EMPTY_CONTAINER_DISPOSE_END returned=False attempts=1','FACTORY_CONSTRUCTOR_END returned=True provider-denied-requests=1 context-requests=NOT_INTERCEPTED','provider-denial-caught-still-fails=true','FACTORY_BINDING_POLICY_PASS exact8-to9=true','CONTAINER_BINDING_POLICY_PASS enabled=True exact4=true','OWNED_CONTAINER_REFERENCE_POLICY_PASS','OWNED_AUTOFAC_RESOURCE_POLICY_PASS positives=4 negatives=26 bounded8=true null-requester=UNKNOWN policy-only=true','OWNED_AUTOFAC_RESOURCE_PHASE_PASS return-throw-cross-thread-clear=true owned-mode-cannot-admit=true no-target-load=true','OWNED_VALUETUPLE_REFERENCE_POLICY_PASS incomplete-pin-identity-path-cache-reflection-other-modes=denied','OWNED_BUILDER_GUARD_BASELINE modeled=true flag=unexpected build=1 use=1 factory=0','OWNED_BUILDER_GUARD_BASELINE modeled=true flag=rejected build=1 use=1 factory=0','OWNED_BUILDER_GUARD_PASS flag=unexpected builder=1 build=0 use=0 factory=0 production-flags-never-reset=true','OWNED_BUILDER_GUARD_PASS flag=rejected builder=1 build=0 use=0 factory=0 production-flags-never-reset=true','OWNED_DISPOSAL_DENIAL_BASELINE reproduced=true unaudited-returned=True provider-denied-requests=1 disposals=1','FACTORY_PROVIDER_LIFETIME_AUDIT after-managed-disposal=true scope-returned=True denied-requests=1','OWNED_DISPOSAL_DENIAL_FIX_PASS failed=true provider-denied-requests=1 disposals=1 shared-original-wrapper=true')){if(-not $emptyLog.Contains($marker)){Fail 'EMPTY_CONTAINER_OWNED_LIFETIME'}}
  if([regex]::Matches($emptyLog,'EMPTY_CONTAINER_DISPOSE_BEGIN').Count -ne 9 -or [regex]::Matches($emptyLog,'FACTORY_CONSTRUCTOR_BEGIN').Count -ne 5 -or $emptyLog.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $emptyLog.Contains('PROJECT_ID_CONSTRUCTOR_BEGIN') -or $emptyLog.Contains('PROJECT_SERVICE_FIXTURE') -or $emptyLog.Contains('FACTORY_BINDING_ASSEMBLY')){Fail 'EMPTY_CONTAINER_OWNED_SCOPE_OR_DISPOSAL'}
  $legacyConfig=Join-Path $build 'bin\ProjectConstructorProbe.exe.config';$aliasConfig=Join-Path $build 'bin\ProjectFactoryBindingProbe.exe.config'
  if((Get-FileHash $legacyConfig -Algorithm SHA256).Hash.ToLowerInvariant() -cne 'b72d33cb400c6a9de0565ece3698308c569836dc41f9003d70d9d78a31b4315a'){Fail 'LEGACY_CONFIG_BYTES'}
  $oldText=[IO.File]::ReadAllText($legacyConfig)
  $containerConfig=Join-Path $build 'bin\ProjectFactoryContainerProbe.exe.config'
  $containerProbe=Local (Join-Path $build 'bin\ProjectFactoryContainerProbe.exe')
  $redirect='<dependentAssembly><assemblyIdentity name="Microsoft.Bcl.AsyncInterfaces" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="8.0.0.0" newVersion="9.0.0.0"/></dependentAssembly>'
  if([IO.File]::ReadAllText($aliasConfig) -cne $oldText.Replace('</assemblyBinding>',$redirect+'</assemblyBinding>')){Fail 'ALIAS_EXACT_CONFIG'}
  $containerRedirect='<dependentAssembly><assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="8.0.0.0" newVersion="9.0.0.0"/></dependentAssembly><dependentAssembly><assemblyIdentity name="System.Memory" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="4.0.1.2" newVersion="4.0.2.0"/></dependentAssembly><dependentAssembly><assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral"/><bindingRedirect oldVersion="6.0.0.0" newVersion="6.0.1.0"/></dependentAssembly><dependentAssembly><assemblyIdentity name="System.Buffers" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="4.0.3.0" newVersion="4.0.4.0"/></dependentAssembly>'
  if([IO.File]::ReadAllText($containerConfig) -cne ([IO.File]::ReadAllText($aliasConfig)).Replace('</assemblyBinding>',$containerRedirect+'</assemblyBinding>')){Fail 'CONTAINER_EXACT_CONFIG'}
  $oldProbe=Local (Join-Path $build 'bin\ProjectConstructorProbe.exe');$aliasProbe=Local (Join-Path $build 'bin\ProjectFactoryBindingProbe.exe')
  Run 'binding-newmode-old-exe' $oldProbe @('--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'binding-newmode-old-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'project-scope-old-exe' $oldProbe @('--mode','owned-project-genuine-factory','--input',$fixtures,'--log',(Join-Path $output 'project-scope-old-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'project-scope-binding-exe' $aliasProbe @('--mode','owned-project-genuine-factory','--input',$fixtures,'--log',(Join-Path $output 'project-scope-binding-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'registered-trace-old-exe'  $oldProbe @('--mode','owned-factory-registered-trace','--input',$fixtures,'--log',(Join-Path $output 'registered-trace-old-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'registered-trace-binding-exe' $aliasProbe @('--mode','owned-factory-registered-trace','--input',$fixtures,'--log',(Join-Path $output 'registered-trace-binding-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'trace-newmode-old-exe'  $oldProbe @('--mode','owned-trace-encryptor','--input',$fixtures,'--log',(Join-Path $output 'trace-old-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'trace-newmode-binding-exe' $aliasProbe @('--mode','owned-trace-encryptor','--input',$fixtures,'--log',(Join-Path $output 'trace-binding-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-newmode-old-exe' $oldProbe @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-newmode-old-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-newmode-binding-exe' $aliasProbe @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-newmode-binding-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-oldmode-container-exe' $containerProbe @('--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'empty-oldmode-container-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'binding-oldmode-alias-exe' $aliasProbe @('--mode','owned-factory','--input',$fixtures,'--log',(Join-Path $output 'binding-oldmode-alias-exe.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  $wrongBinding=Join-Path $output 'binding-wrong-config';$null=New-Item -ItemType Directory $wrongBinding
  $wrongProbe=Join-Path $wrongBinding 'ProjectFactoryBindingProbe.exe';Copy-Item -LiteralPath $aliasProbe -Destination $wrongProbe
  [IO.File]::WriteAllText(($wrongProbe+'.config'),$oldText,$utf8)
  Run 'binding-newmode-oldconfig' $wrongProbe @('--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'binding-newmode-oldconfig.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-newmode-oldconfig' $wrongProbe @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-newmode-oldconfig.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'binding-oldmode-renamed-exe' $wrongProbe @('--mode','owned-factory','--input',$fixtures,'--log',(Join-Path $output 'binding-oldmode-renamed-exe.log')) 2 'FACTORY_BINDING_MODE_EXECUTABLE' 'FACTORY_BINDING_MODE_EXECUTABLE'
  [IO.File]::WriteAllText(($wrongProbe+'.config'),([IO.File]::ReadAllText($aliasConfig)+' '),$utf8)
  Run 'binding-tampered-config' $wrongProbe @('--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'binding-tampered-config.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-tampered-config' $wrongProbe @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-tampered-config.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  $wrongContainer=Join-Path $wrongBinding 'ProjectFactoryContainerProbe.exe';Copy-Item -LiteralPath $containerProbe -Destination $wrongContainer
  [IO.File]::WriteAllText(($wrongContainer+'.config'),[IO.File]::ReadAllText($aliasConfig),$utf8)
  Run 'empty-container-binding-config' $wrongContainer @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-container-binding-config.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  Run 'empty-container-renamed-exe' $wrongContainer @('--mode','owned-factory-bcl9','--input',$fixtures,'--log',(Join-Path $output 'empty-container-renamed-exe.log')) 2 'FACTORY_BINDING_MODE_EXECUTABLE' 'FACTORY_BINDING_MODE_EXECUTABLE'
  [IO.File]::WriteAllText(($wrongProbe+'.config'),[IO.File]::ReadAllText($containerConfig),$utf8)
  Run 'empty-mode-renamed-binding-exe' $wrongProbe @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-mode-renamed-binding-exe.log')) 2 'FACTORY_BINDING_MODE_EXECUTABLE' 'FACTORY_BINDING_MODE_EXECUTABLE'
  [IO.File]::WriteAllText(($wrongContainer+'.config'),([IO.File]::ReadAllText($containerConfig)+' '),$utf8)
  Run 'empty-container-tampered-config' $wrongContainer @('--mode','owned-factory-empty-container','--input',$fixtures,'--log',(Join-Path $output 'empty-container-tampered-config.log')) 2 'EXACT_CONFIG_REQUIRED' 'EXACT_CONFIG_REQUIRED'
  foreach($name in @('binding-newmode-old-exe','binding-oldmode-alias-exe','binding-newmode-oldconfig','binding-oldmode-renamed-exe','binding-tampered-config','empty-newmode-old-exe','empty-newmode-oldconfig','empty-tampered-config','empty-newmode-binding-exe','empty-oldmode-container-exe','empty-container-binding-config','empty-container-renamed-exe','empty-container-tampered-config','empty-mode-renamed-binding-exe')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $blocked.Contains('EMPTY_CONTAINER_BUILDER_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ')){Fail 'BINDING_CONFIG_NEGATIVE_LOADED'}}
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
  foreach($name in @('project-missing-twelve','project-twelve-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('VALUETUPLE_REFERENCE_PIN') -or $blocked.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ') -or $blocked.Contains('PROJECT_PHASE original-type-materialization')){Fail 'PROJECT_NEGATIVE_LOADED'}}
  foreach($name in @('fixture-missing-twelve','fixture-twelve-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('VALUETUPLE_REFERENCE_PIN') -or $blocked.Contains('PROJECT_ID_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_CONSTRUCTOR_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ') -or $blocked.Contains('PROJECT_PHASE original-fixture-type-materialization')){Fail 'PROJECT_FIXTURE_NEGATIVE_LOADED'}}
  $factoryBad=Join-Path $output 'factory-fourteen-bad-hash';$null=New-Item -ItemType Directory -Path $factoryBad
  foreach($name in $projectNames+@('Autofac.dll')){Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $factoryBad $name)}
  Case 'factory-missing-fourteen' 'original-factory' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $factoryBad
  Case 'binding-missing-fourteen' 'original-factory-bcl9' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $factoryBad
  Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $factoryBad 'Knx.Kim.DataProvider.dll')
  Case 'factory-fourteen-wrong-hash' 'original-factory' 2 'INPUT_HASH' 'INPUT_HASH' $factoryBad
  Case 'binding-fourteen-wrong-hash' 'original-factory-bcl9' 2 'INPUT_HASH' 'INPUT_HASH' $factoryBad
  Case 'project-extra-factory-references' 'original-project' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $factoryBad
  Case 'project-id-extra-factory-references' 'original-project-store-id' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $factoryBad
  foreach($name in @('factory-missing-fourteen','factory-fourteen-wrong-hash','project-extra-factory-references','project-id-extra-factory-references')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('VALUETUPLE_REFERENCE_PIN') -or $blocked.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $blocked.Contains('EMPTY_CONTAINER_BUILDER_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ') -or $blocked.Contains('FACTORY_PHASE original-type-materialization')){Fail 'FACTORY_NEGATIVE_LOADED'}}
  foreach($name in @('binding-missing-fourteen','binding-fourteen-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('VALUETUPLE_REFERENCE_PIN') -or $blocked.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $blocked.Contains('EMPTY_CONTAINER_BUILDER_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ')){Fail 'BINDING_INPUT_NEGATIVE_LOADED'}}
  $containerBad=Join-Path $output 'container-nineteen-bad-hash';$null=New-Item -ItemType Directory $containerBad
  $containerNames=@('System.Diagnostics.DiagnosticSource.dll','System.Memory.dll','System.Runtime.CompilerServices.Unsafe.dll','System.Buffers.dll','System.Numerics.Vectors.dll')
  foreach($name in $projectNames+@('Autofac.dll','Knx.Kim.DataProvider.dll')+$containerNames[0..3]){Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $containerBad $name)}
  Case 'project-scope-missing-nineteen' 'original-project-genuine-factory' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  Case 'registered-trace-missing-nineteen' 'original-factory-registered-trace' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  Case 'trace-missing-nineteen' 'original-trace-encryptor' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  Case 'empty-missing-nineteen' 'original-factory-empty-container' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  Copy-Item -LiteralPath (Join-Path $fixtures 'Owned.Task148.ConstructorFixtures.dll') -Destination (Join-Path $containerBad $containerNames[4])
  Case 'project-scope-nineteen-wrong-hash' 'original-project-genuine-factory' 2 'INPUT_HASH' 'INPUT_HASH' $containerBad
  Case 'registered-trace-nineteen-wrong-hash' 'original-factory-registered-trace' 2 'INPUT_HASH' 'INPUT_HASH' $containerBad
  Case 'trace-nineteen-wrong-hash' 'original-trace-encryptor' 2 'INPUT_HASH' 'INPUT_HASH' $containerBad
  foreach($name in @('project-scope-missing-nineteen','project-scope-nineteen-wrong-hash','registered-trace-missing-nineteen','registered-trace-nineteen-wrong-hash','trace-missing-nineteen','trace-nineteen-wrong-hash')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked -match ' PIN |PLATFORM_FACADE_PIN|VALUETUPLE_REFERENCE_PIN|TRACE_PHASE original-type|TRACE_CONSTRUCTOR_BEGIN|PROJECT_ASSEMBLY '){Fail 'TRACE_INPUT_NEGATIVE_LOADED'}}
  Case 'empty-nineteen-wrong-hash' 'original-factory-empty-container' 2 'INPUT_HASH' 'INPUT_HASH' $containerBad
  Case 'factory-reject-container-extras' 'original-factory' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  Case 'binding-reject-container-extras' 'original-factory-bcl9' 2 'PROJECT_EXACT_INPUTS' 'PROJECT_EXACT_INPUTS' $containerBad
  foreach($name in @('empty-missing-nineteen','empty-nineteen-wrong-hash','factory-reject-container-extras','binding-reject-container-extras')){$blocked=[IO.File]::ReadAllText((Join-Path $output ($name+'.log')));if($blocked.Contains('PIN ') -or $blocked.Contains('PLATFORM_FACADE_PIN') -or $blocked.Contains('VALUETUPLE_REFERENCE_PIN') -or $blocked.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $blocked.Contains('EMPTY_CONTAINER_BUILDER_BEGIN') -or $blocked.Contains('PROJECT_ASSEMBLY ')){Fail 'CONTAINER_INPUT_NEGATIVE_LOADED'}}
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

if(-not$interactive -or $administrator){Case 'context-session-guard' 'owned-project-context-denied' $(if(-not$interactive){4}elseif($administrator){5}else{0}) 'child-exit-confirmed=true' $(if(-not$interactive){'INTERACTIVE_SESSION_REQUIRED'}elseif($administrator){'PROJECT_NONADMIN_REQUIRED'}else{'OWNED_CONTEXT_LIFETIME_PASS'})}
$active=@(Get-CimInstance Win32_Process -Filter "Name='ConstructorProbe.exe' OR Name='ProjectConstructorProbe.exe' OR Name='ProjectFactoryBindingProbe.exe' OR Name='ProjectFactoryContainerProbe.exe' OR Name='NeutralStaHost.exe'" | Where-Object {$_.ExecutablePath -and ($_.ExecutablePath -ieq (Join-Path $build 'bin\ConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectConstructorProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectFactoryBindingProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\ProjectFactoryContainerProbe.exe') -or $_.ExecutablePath -ieq (Join-Path $build 'bin\NeutralStaHost.exe'))})
if($active.Count -ne 0){Fail 'OWNED_CHILD_REMAINS'}
Write-Output ('OWNED_SUITE PASS session='+$ExpectedSession+' original=NOT_RUN hardware=NOT_RUN descendants=NOT_CONTAINED')
