# Reference-only owned rule tests. No SDK, WPF, archive parser or device operation executes.
$ErrorActionPreference='Stop'
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
$start=$source.IndexOf('internal static class ReferenceDcaManifestSelection',[StringComparison]::Ordinal)
$end=$source.IndexOf('internal sealed class ProjectDenyProxy',$start,[StringComparison]::Ordinal)
if($start -lt 0 -or $end -le $start){throw 'MANIFEST_RULE_SOURCE_NOT_FOUND'}
$compiled=Add-Type -TypeDefinition ('using System; using System.IO;'+$source.Substring($start,$end-$start)) -PassThru
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
