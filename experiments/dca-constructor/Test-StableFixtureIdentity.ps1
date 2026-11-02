# Two fresh original SDK contexts; test IDs only, no physical identity or UI claim.
[CmdletBinding(DefaultParameterSetName='Windows')]
param(
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$Launcher,
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$InputDirectory,
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$EvidenceDirectory,
 [Parameter(Mandatory,ParameterSetName='SelfTest')][switch]$SelfTest
)
$ErrorActionPreference='Stop'
function ReadIdentity([string]$raw){
 $pattern='(?m)^\S+ OWNED_STABLE_IDENTITY process=([1-9][0-9]{0,9}) project=([A-Za-z0-9+/=]{1,1400}) installation=([A-Za-z0-9+/=]{1,1400}) device=([A-Za-z0-9+/=]{1,1400}) puid-first=([0-9]{1,2}) puid-second=([0-9]{1,2}) encoding=UTF8_BASE64 identity=OFFLINE_TEST_NOT_PHYSICAL\r?$'
 $matchesFound=[regex]::Matches($raw,$pattern)
 if($matchesFound.Count -ne 1){throw 'STABLE_IDENTITY_RECEIPT_REQUIRED_ONCE'}
 $g=$matchesFound[0].Groups
 $receipt=[pscustomobject]@{Process=[long]$g[1].Value;Project=$g[2].Value;Installation=$g[3].Value;Device=$g[4].Value;PuidFirst=[int]$g[5].Value;PuidSecond=[int]$g[6].Value}
 foreach($name in @('Project','Installation','Device')){
  $bytes=[Convert]::FromBase64String($receipt.$name)
  if($bytes.Length -lt 1 -or $bytes.Length -gt 1024 -or [Convert]::ToBase64String($bytes) -cne $receipt.$name){throw 'STABLE_IDENTITY_ENCODING'}
 }
 if($receipt.PuidFirst -lt 1 -or $receipt.PuidSecond -gt 16 -or $receipt.PuidSecond -ne $receipt.PuidFirst+1){throw 'STABLE_IDENTITY_PUID_SEQUENCE'}
 $callback='OWNED_STABLE_CALLBACKS project-same=True installation-same=True creator-same=True repeated-public-IDs=True puid-consecutive=True source=ACTUAL_TYPED_REMOTING_CALLBACKS GetDcaUi=NOT_RUN'
 if([regex]::Matches($raw,[regex]::Escape($callback)).Count -ne 1 -or -not $raw.Contains('PROJECT_RESULT exit=0') -or $raw.Contains('DCA_UI_BEGIN')){throw 'STABLE_IDENTITY_CONTEXT_NOT_HEALTHY'}
 return $receipt
}
function CompareIdentities($first,$second){
 if($first.Process -eq $second.Process){throw 'STABLE_IDENTITY_FRESH_PROCESSES_REQUIRED'}
 foreach($name in @('Project','Installation','Device','PuidFirst','PuidSecond')){if($first.$name -cne $second.$name){throw ('STABLE_IDENTITY_CONTEXT_MISMATCH '+$name)}}
}
if($SelfTest){
 $source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
 $budget=[regex]::Matches($source,'internal static int Identity(?:Write|Context)Budget\(bool identity,int ordinary\)\{[^}]+\}')
 if($budget.Count -ne 2 -or -not $source.Contains('IdentityWriteBudget(FixtureObservationProfile(),ReferenceDcaRequiredParameters.ReadBudget(ParameterNegativeProfile(),SdkUiRequested?200000:16))') -or -not $source.Contains('IdentityContextBudget(FixtureObservationProfile(),ReferenceDcaRequiredParameters.ContextBudget(ParameterNegativeProfile(),SdkUiRequested?10200000:SdkProductMode?64:SdkDeviceMode?32:16))')){throw 'STABLE_IDENTITY_BUDGET_POLICY_WIRING'}
 Add-Type -TypeDefinition ('public static class StableIdentityBudgetProof {'+(($budget | ForEach-Object {$_.Value.Replace('internal static','public static')}) -join "`n")+'}') -Language CSharp
 foreach($ordinary in @(16,32,64,200000,1000000,10200000)){
  if([StableIdentityBudgetProof]::IdentityWriteBudget($false,$ordinary) -ne $ordinary -or [StableIdentityBudgetProof]::IdentityWriteBudget($true,$ordinary) -ne 128){throw 'STABLE_IDENTITY_WRITE_POLICY'}
  if([StableIdentityBudgetProof]::IdentityContextBudget($false,$ordinary) -ne $ordinary -or [StableIdentityBudgetProof]::IdentityContextBudget($true,$ordinary) -ne 256){throw 'STABLE_IDENTITY_CONTEXT_POLICY'}
 }
 $coalescerStart=$source.IndexOf('internal sealed class ReferenceHealthyLogCoalescer',[StringComparison]::Ordinal)
 $coalescerEnd=$source.IndexOf('// Admission accounting only:',$coalescerStart,[StringComparison]::Ordinal)
 if($coalescerStart -lt 0 -or $coalescerEnd -le $coalescerStart -or -not $source.Contains('FixtureObservationProfile()?healthyLog.ShouldEmitIdentity(CompactProductDiagnostics(),value):healthyLog.ShouldEmit(CompactProductDiagnostics(),value)')){throw 'STABLE_IDENTITY_COALESCER_WIRING'}
 $proof=@'
public static class StableIdentityLogProof {
 public static void Run() {
  var log=new ReferenceHealthyLogCoalescer();
  string reply="CONTEXT_ADAPTER_REPLY method=get_Project kind=GENUINE_CAPTURED_PROJECT values=NOT_LOGGED";
  if(!log.ShouldEmitIdentity(true,reply)||log.ShouldEmitIdentity(true,reply))throw new Exception("IDENTITY_CONTEXT_REPEAT");
  string write="SDK_WRITE_FIXTURE_REPLY method=IamDirty entry=7 synchronization=OWNED_EVENT_NONBLOCKING service=OFFLINE_CONTROLLED_FIXTURE_NOT_PERSISTENT_CONFIGURATION";
  if(!log.ShouldEmitIdentity(true,write)||log.ShouldEmitIdentity(true,write.Replace("entry=7","entry=8")))throw new Exception("IDENTITY_WRITE_REPEAT");
  foreach(string line in new[]{"SDK_WRITE_FIXTURE_FAILURE stage=ENTRY","CONTEXT_ADAPTER_FIRST_FAILURE stage=PHASE","SDK_WRITE_FIXTURE_AUDIT phase=PROCESS_FINAL fault=False","OWNED_STABLE_IDENTITY test-receipt=True"})
   if(!log.ShouldEmitIdentity(true,line)||!log.ShouldEmitIdentity(true,line))throw new Exception("FAULT_AUDIT_RECEIPT_OMITTED");
  var other=new ReferenceHealthyLogCoalescer();
  if(!other.ShouldEmit(true,reply)||!other.ShouldEmit(true,reply)||!other.ShouldEmit(true,write)||!other.ShouldEmit(true,write))throw new Exception("OTHER_COMPACT_PROFILE_CHANGED");
  var legacy=new ReferenceHealthyLogCoalescer();if(!legacy.ShouldEmitIdentity(false,reply)||!legacy.ShouldEmitIdentity(false,reply))throw new Exception("NONCOMPACT_CHANGED");
  if(log.Omitted!=2)throw new Exception("IDENTITY_OMITTED_COUNT");
 }
}
'@
 Add-Type -TypeDefinition ('using System;using System.Collections.Generic;'+$source.Substring($coalescerStart,$coalescerEnd-$coalescerStart)+$proof) -Language CSharp
 [StableIdentityLogProof]::Run()
 Write-Output 'STABLE_IDENTITY_LOG_POLICY PASS first-context-and-write-provenance=RETAINED repeated-healthy=OMITTED faults-audits-receipts=RETAINED other-profiles=UNCHANGED SDK-runtime=NOT_RUN'
 Write-Output 'STABLE_IDENTITY_BUDGET_POLICY PASS write-cap=128 context-cap=256 other-profile-caps=UNCHANGED actual-owned-policy=True SDK-runtime=NOT_RUN'
 function Sample([int]$pidValue,[string]$device='RA==',[string]$callbacks='True'){
  return "2000-01-01T00:00:00Z OWNED_STABLE_IDENTITY process=$pidValue project=UA== installation=SQ== device=$device puid-first=3 puid-second=4 encoding=UTF8_BASE64 identity=OFFLINE_TEST_NOT_PHYSICAL`n2000-01-01T00:00:00Z OWNED_STABLE_CALLBACKS project-same=$callbacks installation-same=True creator-same=True repeated-public-IDs=True puid-consecutive=True source=ACTUAL_TYPED_REMOTING_CALLBACKS GetDcaUi=NOT_RUN`n2000-01-01T00:00:00Z PROJECT_RESULT exit=0`n"
 }
 $first=ReadIdentity (Sample 101);$second=ReadIdentity (Sample 102);CompareIdentities $first $second
 $rejected=0
 foreach($case in @('device','project','installation','puid','callback','same-process','missing','duplicate','ui','child-result')){
  try{
   $raw=Sample 102
   switch($case){
    'device'{$raw=Sample 102 'WA=='}
    'project'{$raw=$raw.Replace('project=UA==','project=WA==')}
    'installation'{$raw=$raw.Replace('installation=SQ==','installation=WA==')}
    'puid'{$raw=$raw.Replace('puid-first=3 puid-second=4','puid-first=4 puid-second=5')}
    'callback'{$raw=Sample 102 'RA==' 'False'}
    'same-process'{$raw=Sample 101}
    'missing'{$raw='PROJECT_RESULT exit=0'}
    'duplicate'{$raw=$raw+$raw}
    'ui'{$raw+="DCA_UI_BEGIN`n"}
    'child-result'{$raw=$raw.Replace('PROJECT_RESULT exit=0','PROJECT_RESULT exit=2')}
   }
   $candidate=ReadIdentity $raw;CompareIdentities $first $candidate
  }catch{$rejected++;continue}
  throw ('STABLE_IDENTITY_SELFTEST_ACCEPTED_INVALID '+$case)
 }
 if($rejected -ne 10){throw 'STABLE_IDENTITY_SELFTEST_COUNT'}
 Write-Output 'STABLE_IDENTITY_COMPARATOR_SELFTEST PASS positive=1 negatives=10 synthetic-receipts=True SDK-runtime=NOT_RUN'
 return
}
if($env:OS -cne 'Windows_NT' -or -not [Environment]::Is64BitProcess){throw 'WINDOWS_X64_REQUIRED'}
$Launcher=(Resolve-Path -LiteralPath $Launcher).Path
$InputDirectory=(Resolve-Path -LiteralPath $InputDirectory).Path
if(Test-Path -LiteralPath $EvidenceDirectory){throw 'STABLE_IDENTITY_EVIDENCE_ALREADY_EXISTS'}
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
$null=New-Item -ItemType Directory -Path $EvidenceDirectory
$receipts=@()
foreach($number in 1..2){
 $directory=Join-Path $EvidenceDirectory ('context-'+$number)
 $null=New-Item -ItemType Directory -Path $directory
 $log=Join-Path $directory 'identity.log';$stdout=Join-Path $directory 'launcher-stdout.txt';$stderr=Join-Path $directory 'launcher-stderr.txt'
 Push-Location -LiteralPath $directory
 try{& $Launcher --seconds 45 --mode original-sdk-product-store-fixture --input $InputDirectory --log $log --owned-stable-identity observe > $stdout 2> $stderr;$exitCode=$LASTEXITCODE}finally{Pop-Location}
 if($exitCode -ne 0){throw ('STABLE_IDENTITY_CHILD_FAIL context='+$number+' exit='+$exitCode)}
 foreach($path in @($log,$stdout,$stderr)){if(-not (Test-Path -LiteralPath $path) -or (Get-Item -LiteralPath $path).Length -gt 65536){throw 'STABLE_IDENTITY_CAPTURE_BOUND'}}
 $receipt=ReadIdentity ([IO.File]::ReadAllText($log));$receipts+=,$receipt
 Write-Output ('STABLE_IDENTITY_CONTEXT PASS context='+$number+' process='+$receipt.Process+' IDs=ORIGINAL_PUBLIC_VALUES callback-references=SAME_RETAINED_OBJECTS')
}
CompareIdentities $receipts[0] $receipts[1]
$receipts | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'identity-receipts.json') -Encoding UTF8
Write-Output 'STABLE_IDENTITY_SUITE PASS contexts=2 fresh-processes=True normal-original-Bind=True public-ID-values=EQUAL callback-reference-consistency=True puid-sequence=EQUAL hardware=NOT_RUN UI=NOT_RUN ProjectGuid-and-reload=NOT_TESTED'
