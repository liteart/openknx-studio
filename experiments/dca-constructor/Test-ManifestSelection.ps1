# Reference-only owned rule tests. No SDK, WPF, archive parser or device operation executes.
$ErrorActionPreference='Stop'
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
$start=$source.IndexOf('internal static class ReferenceDcaManifestSelection',[StringComparison]::Ordinal)
$end=$source.IndexOf('internal sealed class ProjectDenyProxy',$start,[StringComparison]::Ordinal)
if($start -lt 0 -or $end -le $start){throw 'MANIFEST_RULE_SOURCE_NOT_FOUND'}
$fenceProof=@'
public static class ParameterFenceProof
{
 public static string Run()
 {
  var publication=new ParameterReadPublication();publication.Begin();
  var entered=new ManualResetEvent(false);var release=new ManualResetEvent(false);Exception workerError=null;
  var worker=new Thread(delegate(){try{using(IDisposable lease=publication.TryEnter()){if(lease==null)throw new Exception("LEASE_NOT_ACQUIRED");entered.Set();if(!release.WaitOne(2000))throw new Exception("LEASE_RELEASE_TIMEOUT");}}catch(Exception error){workerError=error;}});worker.IsBackground=true;worker.Start();
  try{
   if(!entered.WaitOne(1000))throw new Exception("LEASE_ENTRY_TIMEOUT");
   bool rejected=false;try{publication.Close();}catch(InvalidOperationException error){if(error.Message!="PARAMETER_READ_INFLIGHT:1")throw;rejected=true;}
   if(!rejected||publication.Accepting||publication.Inflight!=1||publication.TryEnter()!=null)throw new Exception("CLOSE_ADMISSION_FAILED");
  }finally{release.Set();if(!worker.Join(2000))throw new Exception("LEASE_JOIN_TIMEOUT");entered.Dispose();release.Dispose();}
  if(workerError!=null)throw new Exception("LEASE_WORKER_FAILED",workerError);
  if(publication.Inflight!=0||!publication.Faulted)throw new Exception("LEASE_FAULT_RESET");
  bool finalRejected=false;try{publication.CheckClosed();}catch(InvalidOperationException){finalRejected=true;}if(!finalRejected)throw new Exception("FENCE_FALSE_SUCCESS");
  var normal=new ParameterReadPublication();normal.Begin();using(IDisposable lease=normal.TryEnter()){if(lease==null)throw new Exception("NORMAL_LEASE");}normal.Close();normal.CheckClosed();if(normal.TryEnter()!=null)throw new Exception("CLOSED_NEW_LEASE");
  var exceptional=new ParameterReadPublication();exceptional.Begin();bool propagated=false;try{using(IDisposable lease=exceptional.TryEnter()){throw new InvalidOperationException("OWNED_CALLBACK_THROW");}}catch(InvalidOperationException error){if(error.Message!="OWNED_CALLBACK_THROW")throw;propagated=true;}if(!propagated||exceptional.Inflight!=0)throw new Exception("EXCEPTION_LEASE_RELEASE");exceptional.Close();exceptional.CheckClosed();
  return "PARAMETER_READ_FENCE PASS held-before-gate-close=FAIL_CLOSED new-lease-after-close=DENIED fault-sticky-after-release=True normal-quiescence=PASS exception-finally-release=PASS SDK-runtime=NOT_RUN";
 }
}
'@
$compiled=Add-Type -TypeDefinition ('using System; using System.IO; using System.Text; using System.Threading; using System.Collections.Generic;'+$source.Substring($start,$end-$start)+$fenceProof) -PassThru
$type=$compiled|Where-Object FullName -CEQ 'ReferenceDcaManifestSelection'
if($null -eq $type){throw 'MANIFEST_RULE_TYPE_NOT_FOUND'}
$flags=[Reflection.BindingFlags]'NonPublic,Static'
$before=$type.GetMethod('BeforeMutation',$flags)
$declarations=[string[]]$type.GetMethod('Declarations',$flags).Invoke($null,[object[]]@())
$projection='M-0083_A-0155-40' # Owned test input; actual SDK projection is a separate Windows observation.
$cases=@(
 @{name='reference-different-full-suffix';declarations=$declarations;projection=$projection;selected=$true},
 @{name='exact-declaration-prefix';declarations=[string[]]@($projection);projection=$projection;selected=$true},
 @{name='wrong-manufacturer';declarations=[string[]]@('M-0084_A-0155-40-0000');projection=$projection;selected=$false},
 @{name='wrong-program';declarations=[string[]]@('M-0083_A-0156-40-0000');projection=$projection;selected=$false},
 @{name='wrong-version';declarations=[string[]]@('M-0083_A-0155-41-0000');projection=$projection;selected=$false},
 @{name='prefix-direction';declarations=[string[]]@('M-0083_A-0155');projection=$projection;selected=$false},
 @{name='case-sensitive';declarations=[string[]]@('m-0083_A-0155-40-0000');projection=$projection;selected=$false},
 @{name='null-declaration';declarations=[string[]]@($null);projection=$projection;selected=$false},
 @{name='full-id-not-host-projected';declarations=$declarations;projection='M-0083_A-0155-40-4D9F-O00EF';selected=$false},
 @{name='empty-projection';declarations=$declarations;projection='';selected=$false},
 @{name='declaration-count-bound';declarations=[string[]](@('M-0083_A-0155-40-0000')*17);projection=$projection;selected=$false},
 @{name='declaration-length-bound';declarations=[string[]]@('X'*129);projection=$projection;selected=$false}
)
foreach($case in $cases){
 $state=[pscustomobject]@{Mutations=0}
 $mutation=[Action]{$state.Mutations++}
 $rejected=$false
 try{$null=$before.Invoke($null,[object[]]@($case.declarations,$case.projection,$mutation))}
 catch{if($_.Exception.GetBaseException() -isnot [IO.InvalidDataException] -or $_.Exception.GetBaseException().Message -cne 'DCA_MANIFEST_APPLICATION_MISMATCH'){throw};$rejected=$true}
 if($case.selected){if($rejected -or $state.Mutations -ne 1){throw ('MANIFEST_POSITIVE_FAIL '+$case.name)}}
 elseif(-not $rejected -or $state.Mutations -ne 0){throw ('MANIFEST_PREMUTATION_NEGATIVE_FAIL '+$case.name)}
 Write-Output ('MANIFEST_SELECTION PASS case='+$case.name+' selected='+$case.selected+' mutations='+$state.Mutations)
}
Write-Output 'OWNED_MANIFEST_RULE_SUITE PASS projection=OWNED_TEST_INPUT actual-SDK-projection=NOT_RUN configuration-mutation=SENTINEL_ONLY vendor-runtime=NOT_RUN'
# Owned frontdoor sentinels: these do not execute an original SDK constructor.
$contextBefore=$type.GetMethod('BeforeContextMutation',$flags)
foreach($case in @(
 @{name='missing-selected-context';context=$null;projection=$projection;error='DCA_REQUIRED_PRODUCT_CONTEXT_MISSING'},
 @{name='wrong-context-projection';context=[object]::new();projection='M-0083_A-00B1-32';error='DCA_MANIFEST_APPLICATION_MISMATCH'}
)){
 $state=[pscustomobject]@{ConstructorPathEntries=0}
 $tripwire=[Action]{$state.ConstructorPathEntries++;throw 'OWNED_CONSTRUCTOR_TRIPWIRE'}
 $rejected=$false
 try{$null=$contextBefore.Invoke($null,[object[]]@($case.context,$declarations,$case.projection,$tripwire))}
 catch{if($_.Exception.GetBaseException() -isnot [IO.InvalidDataException] -or $_.Exception.GetBaseException().Message -cne $case.error){throw};$rejected=$true}
 if(-not $rejected -or $state.ConstructorPathEntries -ne 0){throw ('CONTEXT_PREMUTATION_NEGATIVE_FAIL '+$case.name)}
 Write-Output ('MANIFEST_CONTEXT PASS case='+$case.name+' constructor-path-entries=0 proof=OWNED_SENTINEL vendor-missing-parameter-behavior=NOT_RUN')
}
$state=[pscustomobject]@{ConstructorPathEntries=0}
$accepted=[Action]{$state.ConstructorPathEntries++}
$null=$contextBefore.Invoke($null,[object[]]@([object]::new(),$declarations,$projection,$accepted))
if($state.ConstructorPathEntries -ne 1){throw 'CONTEXT_POSITIVE_GUARD_MUTANT'}
Write-Output 'MANIFEST_CONTEXT PASS case=valid-context-control constructor-path-entries=1 proof=OWNED_SENTINEL'
Write-Output 'OWNED_SELECTION_PREFLIGHT_SUITE PASS rule-cases=12 context-cases=3 context-mutation=OWNED_SENTINEL actual-SDK-negative=NOT_RUN vendor-UI-missing-parameter-behavior=NOT_RUN'
$deviceType=$compiled|Where-Object FullName -CEQ 'ReferenceDcaRequiredDevice'
if($null -eq $deviceType){throw 'DEVICE_GUARD_TYPE_NOT_FOUND'}
$deviceBefore=$deviceType.GetMethod('BeforeUi',$flags)
$state=[pscustomobject]@{UiActions=0};$action=[Action]{$state.UiActions++}
$null=$deviceBefore.Invoke($null,[object[]]@([object]::new(),$action))
if($state.UiActions -ne 1){throw 'DEVICE_GUARD_POSITIVE_FAIL'}
Write-Output 'SDK_DEVICE_BOUNDARY_GUARD PASS case=present-control UI-actions=1 proof=OWNED_SENTINEL actual-SDK-device=NOT_RUN'
$state.UiActions=0;$denied=$false
try{$null=$deviceBefore.Invoke($null,[object[]]@($null,$action))}
catch{if($_.Exception.GetBaseException() -isnot [IO.InvalidDataException] -or $_.Exception.GetBaseException().Message -cne 'DCA_REQUIRED_SDK_DEVICE_MISSING'){throw};$denied=$true}
if(-not $denied -or $state.UiActions -ne 0){throw 'DEVICE_GUARD_MISSING_FAIL'}
Write-Output 'SDK_DEVICE_BOUNDARY_GUARD PASS case=missing-device UI-actions=0 proof=OWNED_SENTINEL vendor-missing-device-behavior=NOT_RUN'
$state.UiActions=0;$failure=[Action]{$state.UiActions++;throw [InvalidOperationException]::new('OWNED_DEVICE_UI_ACTION_FAILED')};$propagated=$false
try{$null=$deviceBefore.Invoke($null,[object[]]@([object]::new(),$failure))}
catch{if($_.Exception.GetBaseException() -isnot [InvalidOperationException] -or $_.Exception.GetBaseException().Message -cne 'OWNED_DEVICE_UI_ACTION_FAILED'){throw};$propagated=$true}
if(-not $propagated -or $state.UiActions -ne 1){throw 'DEVICE_GUARD_FAILURE_MASKED'}
Write-Output 'SDK_DEVICE_BOUNDARY_GUARD PASS case=action-failure-propagated proof=OWNED_SENTINEL'
$nullActionDenied=$false
try{$null=$deviceBefore.Invoke($null,[object[]]@([object]::new(),$null))}
catch{if($_.Exception.GetBaseException() -isnot [ArgumentNullException]){throw};$nullActionDenied=$true}
if(-not $nullActionDenied){throw 'DEVICE_GUARD_NULL_ACTION_ACCEPTED'}
Write-Output 'SDK_DEVICE_BOUNDARY_GUARD PASS case=null-action-rejected proof=OWNED_SENTINEL'
$parameterType=$compiled|Where-Object FullName -CEQ 'ReferenceDcaRequiredParameters'
if($null -eq $parameterType){throw 'PARAMETER_GUARD_TYPE_NOT_FOUND'}
$parameterBefore=$parameterType.GetMethod('BeforeUi',$flags)
$state=[pscustomobject]@{UiActions=0}
$action=[Action]{$state.UiActions++}
$null=$parameterBefore.Invoke($null,[object[]]@([object]::new(),$action))
if($state.UiActions -ne 1){throw 'PARAMETER_GUARD_POSITIVE_FAIL'}
Write-Output 'PARAMETER_DEFINITION_GUARD PASS case=present-control UI-actions=1 proof=OWNED_SENTINEL actual-SDK-definition=NOT_RUN'
$state.UiActions=0;$denied=$false
try{$null=$parameterBefore.Invoke($null,[object[]]@($null,$action))}
catch{if($_.Exception.GetBaseException() -isnot [IO.InvalidDataException] -or $_.Exception.GetBaseException().Message -cne 'DCA_REQUIRED_PARAMETER_DEFINITION_MISSING'){throw};$denied=$true}
if(-not $denied -or $state.UiActions -ne 0){throw 'PARAMETER_GUARD_MISSING_FAIL'}
Write-Output 'PARAMETER_DEFINITION_GUARD PASS case=missing-definition UI-actions=0 proof=OWNED_SENTINEL vendor-missing-definition-behavior=NOT_RUN'
$state.UiActions=0;$failure=[Action]{$state.UiActions++;throw [InvalidOperationException]::new('OWNED_UI_ACTION_FAILED')};$propagated=$false
try{$null=$parameterBefore.Invoke($null,[object[]]@([object]::new(),$failure))}
catch{if($_.Exception.GetBaseException() -isnot [InvalidOperationException] -or $_.Exception.GetBaseException().Message -cne 'OWNED_UI_ACTION_FAILED'){throw};$propagated=$true}
if(-not $propagated -or $state.UiActions -ne 1){throw 'PARAMETER_GUARD_FAILURE_MASKED'}
Write-Output 'PARAMETER_DEFINITION_GUARD PASS case=action-failure-propagated proof=OWNED_SENTINEL'
$allowsRead=$parameterType.GetMethod('AllowsReadOperation',$flags)
foreach($operation in -1..17){
 $inactive=[bool]$allowsRead.Invoke($null,[object[]]@($false,[int]$operation))
 $active=[bool]$allowsRead.Invoke($null,[object[]]@($true,[int]$operation))
 $expected=$operation -in @(3,10,11,12,13)
 if($inactive -or $active -ne $expected){throw ('PARAMETER_READ_SCOPE_POLICY_FAIL operation='+$operation)}
}
Write-Output 'PARAMETER_READ_POLICY PASS active-read-operations=3,10,11,12,13 inactive-denied=True other-operations-denied=True proof=OWNED_POLICY actual-SDK-lookup-scope=NOT_RUN'
$allowsBackground=$parameterType.GetMethod('AllowsBackgroundRead',$flags)
foreach($operation in -1..17){
 if([bool]$allowsBackground.Invoke($null,[object[]]@($false,[int]$operation)) -or ([bool]$allowsBackground.Invoke($null,[object[]]@($true,[int]$operation))) -ne ($operation -in @(10,11,12))){throw ('PARAMETER_BACKGROUND_POLICY_FAIL operation='+$operation)}
}
Write-Output 'PARAMETER_BACKGROUND_POLICY PASS published-read=operations10,11,12-only unpublished-denied=True unknown-and-write-operations-denied=True proof=OWNED_POLICY actual-SDK-worker=NOT_RUN'
$allowsCreator=$parameterType.GetMethod('AllowsDerivedReadCreation',$flags)
foreach($completed in @($false,$true)){foreach($uiLifetime in @($false,$true)){foreach($parameterLifetime in @($false,$true)){
 $accepted=[bool]$allowsCreator.Invoke($null,[object[]]@($completed,$uiLifetime,$parameterLifetime))
 if($accepted -ne ($completed -and ($uiLifetime -or $parameterLifetime))){throw 'PARAMETER_DERIVED_CREATOR_POLICY_FAIL'}
}}}
Write-Output 'PARAMETER_DERIVED_CREATOR_POLICY PASS completed-import-required=True active-ui-or-parameter-read-required=True combinations=8 proof=OWNED_POLICY actual-derived-cache-creation=NOT_RUN'
$allowsProductRead=$parameterType.GetMethod('AllowsProductStoreRead',$flags)
foreach($slot in -1..5){
 if([bool]$allowsProductRead.Invoke($null,[object[]]@($false,[int]$slot)) -or ([bool]$allowsProductRead.Invoke($null,[object[]]@($true,[int]$slot))) -ne ($slot -in @(0,1,2,3,4))){throw ('PARAMETER_PRODUCT_STORE_POLICY_FAIL slot='+$slot)}
}
Write-Output 'PARAMETER_PRODUCT_STORE_POLICY PASS published-slots=APPLICATION_PROGRAM_APPLICATIONS_HARDWARE_BAGGAGES_LARGE_STRING_STREAM unpublished-unknown-save-write-denied=True proof=OWNED_POLICY actual-original-store-reads=NOT_RUN'
$contextBudget=$parameterType.GetMethod('ContextBudget',$flags)
$readBudget=$parameterType.GetMethod('ReadBudget',$flags)
foreach($ordinary in @(16,32,64,10200000)){
 if([int]$contextBudget.Invoke($null,[object[]]@($false,[int]$ordinary)) -ne $ordinary -or [int]$contextBudget.Invoke($null,[object[]]@($true,[int]$ordinary)) -ne 1000000){throw 'PARAMETER_CONTEXT_BUDGET_POLICY_FAIL'}
}
foreach($ordinary in @(16,32,128,200000)){
 if([int]$readBudget.Invoke($null,[object[]]@($false,[int]$ordinary)) -ne $ordinary -or [int]$readBudget.Invoke($null,[object[]]@($true,[int]$ordinary)) -ne 200000){throw 'PARAMETER_READ_BUDGET_POLICY_FAIL'}
}
Write-Output 'PARAMETER_BUDGET_POLICY PASS negative-context-cap=1000000 negative-cache-creator-write-cap=200000 legacy-context-and-read-caps=UNCHANGED proof=OWNED_POLICY actual-SDK-volume=NOT_RUN'
$allowsPrepared=$parameterType.GetMethod('AllowsPreparedService',$flags)
foreach($case in @(
 @($false,$true,$true,-1,$false),@($true,$true,$true,-1,$true),@($true,$false,$true,-1,$false),
 @($true,$true,$false,-1,$false),@($true,$true,$false,14,$true),@($true,$false,$false,14,$true),
 @($true,$true,$false,2,$false),@($true,$false,$false,12,$false),@($false,$false,$false,14,$false)
)){
 $actual=[bool]$allowsPrepared.Invoke($null,[object[]]@([bool]$case[0],[bool]$case[1],[bool]$case[2],[int]$case[3]))
 if($actual -ne [bool]$case[4]){throw 'PARAMETER_PREPARED_SERVICE_POLICY_FAIL'}
}
Write-Output 'PARAMETER_PREPARED_SERVICE_POLICY PASS fixed-known-validators=OWNER_ONLY real-dispatch=SCOPED unknown-types-and-services=DENIED unpublished=DENIED UI-visibility=NOT_CLAIMED proof=OWNED_POLICY'
$compactDiagnostics=$parameterType.GetMethod('CompactDiagnostics',$flags)
foreach($ui in @($false,$true)){foreach($negative in @($false,$true)){
 if([bool]$compactDiagnostics.Invoke($null,[object[]]@($ui,$negative)) -ne ($ui -or $negative)){throw 'PRODUCT_DIAGNOSTIC_PROFILE_FAIL'}
}}
Write-Output 'PRODUCT_DIAGNOSTIC_PROFILE PASS legacy=False UI=True explicit-parameter-negative=True validation-and-permissions=NOT_CHANGED proof=OWNED_POLICY'

Write-Output ([ParameterFenceProof]::Run())
$coalescerType=$compiled|Where-Object FullName -CEQ 'ReferenceHealthyLogCoalescer'
$instanceFlags=[Reflection.BindingFlags]'NonPublic,Instance'
$emit=$coalescerType.GetMethod('ShouldEmit',$instanceFlags)
$coalescer=[Activator]::CreateInstance($coalescerType,$true)
$message='FACTORY_BINDING_RESOLVED requested=owned-fixture post-policy=owned-fixture checked=True'
if(-not [bool]$emit.Invoke($coalescer,[object[]]@($true,$message)) -or [bool]$emit.Invoke($coalescer,[object[]]@($true,$message))){throw 'HEALTHY_FIRST_PROVENANCE_FAIL'}
foreach($errorRecord in @('PROJECT_EXCEPTION owned-failure','SDK_NAME_RESOURCE_PROBE allowed=False requested=owned-failure')){foreach($repeat in 1..2){if(-not [bool]$emit.Invoke($coalescer,[object[]]@($true,$errorRecord))){throw 'ERROR_LOG_SUPPRESSED'}}}
foreach($repeat in 1..2){if(-not [bool]$emit.Invoke($coalescer,[object[]]@($false,$message))){throw 'LEGACY_LOG_SUPPRESSED'}}
$resource='SDK_NAME_RESOURCE_PROBE allowed=True requested=owned-fixture requester=KNOWN aggregate-probe='
if(-not [bool]$emit.Invoke($coalescer,[object[]]@($true,($resource+'1 action=RETURN_NULL'))) -or [bool]$emit.Invoke($coalescer,[object[]]@($true,($resource+'2 action=RETURN_NULL')))){throw 'HEALTHY_RESOURCE_COALESCING_FAIL'}
Write-Output 'HEALTHY_LOG_COALESCING PASS first-provenance=RETAINED exact-healthy-repeats=OMITTED denied-and-error-repeats=RETAINED legacy=UNCHANGED proof=OWNED_POLICY'
