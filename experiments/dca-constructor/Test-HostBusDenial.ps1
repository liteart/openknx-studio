# Task013: owned denial tests only. This script never selects an original/vendor mode.
[CmdletBinding(DefaultParameterSetName='SelfTest')]
param(
 [Parameter(ParameterSetName='SelfTest')][switch]$SelfTest,
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$Launcher,
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$InputDirectory,
 [Parameter(Mandatory,ParameterSetName='Windows')][string]$EvidenceDirectory
)
$ErrorActionPreference='Stop'
if($PSCmdlet.ParameterSetName -eq 'SelfTest'){
 $source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
 $shared=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ConstructorProbe.cs'));$launcherSource=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProbeLauncher.cs'))
 $begin=$shared.IndexOf('    internal static bool OriginalBusProbeBlocked(');$finish=$shared.IndexOf('    internal const string ConfigText',$begin)
 if($begin -lt 0 -or $finish -le $begin){throw 'BUS_ORIGINAL_ISOLATION_GUARD_SOURCE'}
 Add-Type -TypeDefinition ('using System;public static class OriginalBusIsolationProof {'+$shared.Substring($begin,$finish-$begin).Replace('internal static','public static')+'}') -Language CSharp
 foreach($case in @(@([string[]]@('--mode','original-sdk-product-store-fixture','--offline-host-bus-denial','probe'),$true),@([string[]]@('--offline-host-bus-denial'),$true),@([string[]]@('--offline-host-bus-denial','probe','--isolated','true'),$true),@([string[]]@('--offline-host-bus-denial','probe','proof-isolated.json'),$true),@([string[]]@('--mode','owned-sdk-project-fixture','--owned-host-bus-denial','check'),$false),@([string[]]@('--mode','original-sdk-product-store-fixture'),$false),@([string[]]::new(17),$true))){if([OriginalBusIsolationProof]::OriginalBusProbeBlocked([string[]]$case[0]) -ne [bool]$case[1]){throw 'BUS_ORIGINAL_ISOLATION_ENTRY_DENIAL'}}
 foreach($item in @(@($source,'int outcome = 2;'),@($launcherSource,'Process child = null;'))){$main=$item[0].Substring($item[0].IndexOf('    internal static int Main(string[] args)'));$guard=$main.IndexOf('if(ProbeFiles.OriginalBusProbeBlocked(args))');if($guard -lt 0 -or $guard -gt $main.IndexOf($item[1]) -or -not $main.Contains('OFFLINE_BUS_ISOLATION_CAPABILITY_REQUIRED')){throw 'BUS_ORIGINAL_ISOLATION_BEFORE_ALL_EFFECTS'}}
 Write-Output 'HOST_BUS_ORIGINAL_ENTRY_DENIAL_POLICY PASS cases=7 launcher-before-child=True direct-before-MainCore=True synthetic-attestations=NOT_ACCEPTED owned-profile=UNCHANGED original-SDK=NOT_RUN'

 $a=$source.IndexOf('internal sealed class OfflineBusDenialState');$b=$source.IndexOf('internal sealed class ProjectDenyProxy',$a)
 if($a -lt 0 -or $b -le $a){throw 'BUS_DENIAL_STATE_SOURCE'}
 $proof=@'
public static class BusDenialProof {
 static void Require(bool b,string name){if(!b)throw new Exception(name);}
 static bool Failed(Action a){try{a();return false;}catch(InvalidOperationException){return true;}}
 public static void Run(){
  var healthy=new OfflineBusDenialState();healthy.Begin();bool inspect;bool tracked=healthy.Deny(out inspect);Require(inspect&&tracked&&healthy.Faulted&&healthy.Entries==1&&healthy.Inflight==1,"LATCH_BEFORE_MESSAGE");healthy.Exit(tracked);healthy.Close();healthy.Verify(1,true);Require(healthy.Faulted,"NO_LATCH_RESET");
  var repeated=new OfflineBusDenialState();repeated.Begin();for(int i=1;i<=16;i++){tracked=repeated.Deny(out inspect);Require(inspect==(i==1),"FIRST_METADATA_ONLY");repeated.Exit(tracked);}repeated.Close();repeated.Verify(16,true);
  var cap=new OfflineBusDenialState();cap.Begin();for(int i=0;i<24;i++){tracked=cap.Deny(out inspect);cap.Exit(tracked);}cap.Close();Require(cap.Entries==17&&cap.Broken&&Failed(delegate{cap.Verify(17,true);}),"SATURATED_CAP_FATAL");
  var late=new OfflineBusDenialState();late.Begin();late.Close();tracked=late.Deny(out inspect);Require(!inspect,"CLOSED_NO_MESSAGE_INSPECTION");late.Exit(tracked);Require(late.Broken&&Failed(delegate{late.Verify(1,true);}),"LATE_FATAL");
  var queued=new OfflineBusDenialState();queued.Begin();tracked=queued.Deny(out inspect);queued.Close();Require(queued.Inflight==1&&queued.Broken&&Failed(delegate{queued.Verify(1,true);}),"CLOSE_WITH_INFLIGHT_FAILS");queued.Exit(tracked);Require(queued.Broken,"CLOSE_FAULT_STICKY");
  var recorder=new OfflineBusDenialState();recorder.Begin();tracked=recorder.Deny(out inspect);recorder.Break();recorder.Exit(tracked);recorder.Close();Require(Failed(delegate{recorder.Verify(1,true);}),"RECORDER_FAULT_FATAL");
  var worker=new OfflineBusDenialState();worker.Begin();Exception failure=null;var thread=new System.Threading.Thread(delegate(){try{bool first;bool entered=worker.Deny(out first);worker.Exit(entered);}catch(Exception e){failure=e;}});thread.IsBackground=true;thread.Start();Require(thread.Join(2000)&&failure==null,"WORKER_DENIAL_COMPLETES");worker.Close();worker.Verify(1,true);
  var unopened=new OfflineBusDenialState();tracked=unopened.Deny(out inspect);unopened.Exit(tracked);Require(!inspect&&unopened.Broken,"NOT_BEGUN_NO_METADATA");
 }
}
'@
 Add-Type -TypeDefinition ('using System;'+$source.Substring($a,$b-$a)+$proof) -Language CSharp
 [BusDenialProof]::Run()
 $a=$source.IndexOf('internal sealed class OfflineHostContextProxy');$b=$source.IndexOf('internal sealed class SdkWriteContextProxy',$a);$proxy=$source.Substring($a,$b-$a)
 if($proxy.IndexOf('State.Deny(out inspect)') -ge $proxy.IndexOf('ContentMessageDescriptor(') -or $proxy.Contains('.Args') -or $proxy.Contains('.MethodName') -or $proxy.Contains('new ReturnMessage') -or -not $proxy.Contains('throw new NotSupportedException("DCA_BUS_OFFLINE_UNSUPPORTED")') -or -not $proxy.Contains('catch{State.Break();throw;}') -or -not $proxy.Contains('finally{State.Exit(tracked);}')){throw 'BUS_DENIAL_PROXY_ZERO_RETURN_AND_ARGUMENT_READ'}
 foreach($pin in @('0x0200001c','0x06000178','0x0600017f','0x06000180','0x06000181','0x06000182','constructor.MetadataToken==0x06000013','Object.ReferenceEquals(host.Invoke(execution,new object[0]),reference)')){if(-not $source.Contains($pin)){throw ('BUS_DENIAL_ORIGINAL_ABI_PIN '+$pin)}}
 if(-not $source.Contains('if(sdkOfflineHost!=null)sdkOfflineHost.State.Close();') -or -not $source.Contains('ProbeFiles.Require(denied,"BUS_DENIAL_NO_SUCCESS_RETURN")') -or -not $source.Contains('original-DCA-UI-trigger=NOT_RUN network-observation=NOT_PROVIDED')){throw 'BUS_DENIAL_LIFETIME_AND_CLAIM'}
 Write-Output 'HOST_BUS_DENIAL_POLICY PASS latch-before-metadata=True first-only=True cap16-saturated17=True late-unbegun-recorder-inflight=FAIL_CLOSED worker-denial=True arguments-read=0 success-return=NONE driver=NOT_CREATED proof=OWNED_STATE_AND_SOURCE typed-Net48=NOT_RUN vendor-and-network-isolation=NOT_RUN'
 return
}
if($env:OS -cne 'Windows_NT'){throw 'BUS_DENIAL_WINDOWS_REQUIRED'}
$Launcher=[IO.Path]::GetFullPath($Launcher);$InputDirectory=[IO.Path]::GetFullPath($InputDirectory);$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
if(-not [IO.File]::Exists($Launcher) -or -not [IO.Directory]::Exists($InputDirectory) -or -not [IO.Directory]::Exists($EvidenceDirectory)){throw 'BUS_DENIAL_EXISTING_INPUTS_REQUIRED'}
foreach($path in @($Launcher,$InputDirectory,$EvidenceDirectory)){if(([IO.File]::GetAttributes($path) -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'BUS_DENIAL_REPARSE_INPUT'}}
$log=Join-Path $EvidenceDirectory 'owned-host-bus.log';$stdout=Join-Path $EvidenceDirectory 'owned-host-bus.stdout.txt';$stderr=Join-Path $EvidenceDirectory 'owned-host-bus.stderr.txt'
foreach($path in @($log,$stdout,$stderr)){if(Test-Path -LiteralPath $path){throw 'BUS_DENIAL_FRESH_OUTPUT_REQUIRED'}}
& $Launcher --seconds 15 --mode owned-sdk-project-fixture --input $InputDirectory --log $log --owned-host-bus-denial check > $stdout 2> $stderr
if($LASTEXITCODE -ne 0){throw ('BUS_DENIAL_OWNED_CHILD_EXIT '+$LASTEXITCODE)}
$raw=[IO.File]::ReadAllText($log)
if([regex]::Matches($raw,'OWNED_HOST_BUS_DENIAL_CASE_PASS case=').Count -ne 6 -or -not $raw.Contains('OWNED_HOST_BUS_DENIAL_SUITE_PASS typed-cases=6 synthetic-cases=3') -or -not $raw.Contains('PROJECT_RESULT exit=0')){throw 'BUS_DENIAL_OWNED_RECEIPT'}
Write-Output 'HOST_BUS_DENIAL_WINDOWS_PASS typed-denials=6 synthetic-negative-cases=3 unregistered-callback=DENIED SDK-runtime=NOT_RUN network-isolation=NOT_PROVEN'

# This is an owned entry rejection, not an original SDK/connection test.
$missing=Join-Path $EvidenceDirectory 'must-not-exist-original-inputs';$blockedLog=Join-Path $EvidenceDirectory 'isolation-blocked.log';$blockedOut=Join-Path $EvidenceDirectory 'isolation-blocked.stdout.txt';$blockedError=Join-Path $EvidenceDirectory 'isolation-blocked.stderr.txt'
foreach($path in @($missing,$blockedLog,$blockedOut,$blockedError)){if(Test-Path -LiteralPath $path){throw 'BUS_DENIAL_FRESH_ISOLATION_NEGATIVE_REQUIRED'}}
& $Launcher --seconds 45 --mode original-sdk-product-store-fixture --input $missing --log $blockedLog --offline-host-bus-denial probe > $blockedOut 2> $blockedError
if($LASTEXITCODE -ne 2 -or -not [IO.File]::ReadAllText($blockedOut).Contains('OFFLINE_BUS_ISOLATION_CAPABILITY_REQUIRED child-started=False vendor-runtime=NOT_RUN') -or (Test-Path -LiteralPath $missing) -or (Test-Path -LiteralPath $blockedLog)){throw 'BUS_ORIGINAL_ISOLATION_ENTRY_NEGATIVE'}
Write-Output 'HOST_BUS_ORIGINAL_ENTRY_DENIAL_WINDOWS_PASS launcher-exit=2 child-started=False original-inputs-missing=True original-log-not-created=True vendor-SDK-and-driver=NOT_RUN'
