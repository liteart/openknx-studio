#requires -Version 7.6
# Owned non-executable metadata fixtures only.
param([string] $TemporaryParent='/private/tmp')
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$references=@('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography')
Add-Type -ReferencedAssemblies $references -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'EntrypointMetadata.cs')))
Add-Type -ReferencedAssemblies $references -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'OwnedEntrypointFixtures.cs')))
$root=Join-Path $TemporaryParent ('task144-owned-'+[guid]::NewGuid().ToString('N'));$null=New-Item -ItemType Directory $root
$passed=0
function Assert([bool] $ok,[string] $message){if(-not $ok){throw $message}}
function Hash([string] $p){(Get-FileHash $p -Algorithm SHA256).Hash.ToLowerInvariant()}
function Read([string] $p,[string] $referenceHash=(Hash $reference)){[Task144.EntrypointMetadata]::Read($p,(Hash $p),@($reference),@($referenceHash))}
function Reject([scriptblock] $action,[string] $code){$seen=$false;$observed='No exception';try{$null=& $action}catch{$observed=$_.Exception.ToString();$seen=$observed.Contains($code)};Assert $seen ('Expected '+$code+' observed '+$observed);$script:passed++}
try{
 $dca=Join-Path $root 'OwnedDca.dll';$reference=Join-Path $root 'OwnedContract.dll'
 [Task144OwnedFixtures]::Create($dca);[Task144OwnedFixtures]::Create($reference,$true)
 $raw=Read $dca;$r=$raw|ConvertFrom-Json
 Assert ($r.runtime_execution -eq 'NOT_RUN' -and $r.operation -eq 'static_dca_entrypoint_map') 'Runtime claim';$passed++
 Assert ($r.types.Count -eq 2 -and $r.types[0].name -eq 'Owned.Dca') 'Visibility/selected contract';$passed++
 Assert ($r.types[0].methods.Count -eq 2 -and $r.types[0].methods[0].name -eq '.ctor' -and $r.types[0].methods[1].name -eq 'Owned.Contract.Callback') 'Public constructor/explicit callback/hidden method selection';$passed++
 Assert ($r.types[0].methods[1].signature -eq 'instance Void(String) generic=0') 'Signature';$passed++
 Assert (-not $raw.Contains($root)) 'Private path leak';$passed++
 Assert ($r.types[0].interfaces[0] -eq '[Knx.Ets.Sdk.AddIns.AddInViews, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Knx.Ets.Sdk.AddIns.AddInViews.IEts4AddIn' -and $r.types[0].methods[0].signature_header -eq 32) 'Assembly identity or raw header lost';$passed++
 foreach($profile in @(1,2,3,4)){
  $bad=Join-Path $root ('Bad'+$profile+'.dll');[Task144OwnedFixtures]::Create($bad,$false,$profile)
  Reject {Read $bad} $(if($profile -eq 2){'DCAENTRY_MALFORMED_SIGNATURE'}else{'DCAENTRY_SIGNATURE_LIMIT'})
 }
 $broken=Join-Path $root 'NotManaged.dll';[IO.File]::WriteAllText($broken,'owned invalid image')
 Reject {Read $broken ('0'*64)} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::Read($dca,('0'*64),@($reference),@((Hash $reference)))} 'DCAENTRY_INPUT_HASH'
 $sdk=Join-Path $root 'OwnedSdk.dll';[Task144OwnedFixtures]::CreateProject($sdk)
 function Project([string] $p){[Task144.EntrypointMetadata]::ReadProject($dca,(Hash $dca),@($p),@((Hash $p)))}
 $projectRaw=Project $sdk;$p=$projectRaw|ConvertFrom-Json
 Assert ($p.operation -eq 'static_dca_project_context' -and $p.runtime_execution -eq 'NOT_RUN' -and $p.types.Count -eq 2 -and $p.base_chain_disposition -eq 'EXTERNAL_BASE_NOT_RESOLVED') 'Project root/base/runtime boundary';$passed++
 $project=$p.types|Where-Object name -eq 'Knx.Ets.Sdk.Project.Project'
 Assert ($project.methods.Count -eq 6 -and @($project.methods|Where-Object name -eq 'Hidden').Count -eq 0) 'Project method accessibility';$passed++
 $constructors=@($project.methods|Where-Object name -eq '.ctor')
 Assert ($constructors.Count -eq 3 -and (($constructors[0].attributes -band 7) -eq 1) -and (($constructors[1].attributes -band 7) -eq 4) -and (($constructors[2].attributes -band 7) -eq 6)) 'Private/protected/public constructor evidence';$passed++
 Assert ($project.methods[3].name -eq '.cctor' -and $project.methods[3].signature -eq 'static Void() generic=0' -and $p.inputs[1].assembly_references.Count -eq 1 -and -not $projectRaw.Contains($root)) 'Static constructor/reference metadata/path boundary';$passed++
 foreach($profile in @(1,2,3,4,5,6)){
  $bad=Join-Path $root ('ProjectBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProject($bad,$profile)
  $code=@{1='DUPLICATE_PROJECT';2='PROJECT_NOT_FOUND';3='BASE_CYCLE';4='BASE_LIMIT';5='MALFORMED_SIGNATURE';6='PROJECT_TYPE'}[$profile]
  Reject {Project $bad} ('DCAENTRY_'+$code)
 }
 Reject {[Task144.EntrypointMetadata]::ReadProject($dca,(Hash $dca),@($sdk,$sdk),@((Hash $sdk),(Hash $sdk)))} 'DCAENTRY_DUPLICATE_PROJECT'
 $constructionSdk=Join-Path $root 'ConstructionSdk.dll';$om=Join-Path $root 'ConstructionObjectModel.dll'
 [Task144OwnedFixtures]::CreateConstruction($constructionSdk);[Task144OwnedFixtures]::CreateConstruction($om,$true)
 function Construction([string] $s=$constructionSdk,[string] $o=$om){[Task144.EntrypointMetadata]::ReadConstruction($dca,(Hash $dca),@($reference,$s,$o),@((Hash $reference),(Hash $s),(Hash $o)))}
 $constructionRaw=Construction;$c=$constructionRaw|ConvertFrom-Json
 Assert ($c.operation -eq 'static_dca_project_construction' -and $c.runtime_execution -eq 'NOT_RUN' -and $c.inputs.Count -eq 4 -and $c.selected_roots.Count -eq 5 -and $c.types.Count -eq 7) 'Fixed construction roots and deduplicated local bases';$passed++
 $context=$c.selected_roots|Where-Object name -eq 'Knx.Ets.Sdk.ISdkContext'
 $contextType=$c.types|Where-Object name -eq 'Knx.Ets.Sdk.ISdkContext'
 Assert ($context.base_chain_disposition -eq 'NO_BASE' -and $null -eq $contextType.base_type -and ($contextType.attributes -band 32) -eq 32) 'Interface nil base is valid explicit NO_BASE';$passed++
 $sdkProject=$c.types|Where-Object name -eq 'Knx.Ets.Sdk.Project.Project'
 Assert ($sdkProject.methods.Count -eq 1 -and ($sdkProject.methods[0].attributes -band 7) -eq 3 -and $sdkProject.methods[0].signature.Contains('Knx.Ets.Sdk.Project.ProjectCollection') -and $sdkProject.methods[0].signature.Contains('Knx.Ets.ObjectModel.Project.Project') -and $sdkProject.methods[0].signature.Contains('Knx.Ets.Sdk.ISdkContext')) 'Internal constructor actual parameter categories';$passed++
 $collection=$c.types|Where-Object name -eq 'Knx.Ets.Sdk.Project.ProjectCollection'
 Assert (@($collection.methods|Where-Object name -eq 'HiddenFactory').Count -eq 0 -and ($collection.methods|Where-Object name -eq 'Create').factory_candidate -eq 'DECLARATION_ONLY' -and ($collection.methods|Where-Object name -eq 'ParameterOnly').factory_candidate -eq 'NOT_SELECTED_RETURN' -and ($collection.methods|Where-Object name -eq 'GetRoot').factory_candidate -eq 'DECLARATION_ONLY') 'Return-only/public/protected candidate declaration selection';$passed++
 $rootType=$c.types|Where-Object name -eq 'Knx.Ets.Sdk.Root'
 Assert (($rootType.methods|Where-Object name -eq 'GetNested').factory_candidate -eq 'DECLARATION_ONLY' -and ($rootType.methods|Where-Object name -eq 'GetForeign').factory_candidate -eq 'NOT_SELECTED_RETURN' -and ($rootType.methods|Where-Object name -eq 'GetPrefix').factory_candidate -eq 'NOT_SELECTED_RETURN' -and @($c.types|Where-Object name -eq 'Knx.Ets.Sdk.Project.ProjectCollectionFactoryValue').Count -eq 0 -and -not $constructionRaw.Contains($root)) 'Generic return/foreign assembly/prefix/nonrecursive boundary';$passed++
 foreach($profile in @(1,2,3,4)){
  $bad=Join-Path $root ('ConstructionBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateConstruction($bad,($profile -ge 3),$profile)
  $code=if($profile -in @(1,3)){'DCAENTRY_ROOT_NOT_FOUND'}else{'DCAENTRY_DUPLICATE_ROOT'}
  if($profile -ge 3){Reject {Construction $constructionSdk $bad} $code}else{Reject {Construction $bad} $code}
 }
 Reject {[Task144.EntrypointMetadata]::ReadConstruction($dca,(Hash $dca),@($reference,$constructionSdk,$om),@((Hash $reference),(Hash $constructionSdk),('0'*64)))} 'DCAENTRY_INPUT_HASH'
 # A later pin failure wins over an earlier invalid PE: every input is pinned before any reader.
 Reject {[Task144.EntrypointMetadata]::ReadConstruction($broken,(Hash $broken),@($reference,$constructionSdk,$om),@((Hash $reference),(Hash $constructionSdk),('0'*64)))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadConstruction($dca,(Hash $dca),@($constructionSdk,$om),@((Hash $constructionSdk),(Hash $om)))} 'DCAENTRY_INPUT_COUNT'
 Reject {[Task144.EntrypointMetadata]::ReadProject($dca,(Hash $dca),@($reference,$constructionSdk,$om),@((Hash $reference),(Hash $constructionSdk),(Hash $om)))} 'DCAENTRY_INPUT_COUNT'
 foreach($profile in @(5,6,7)){
  $bad=Join-Path $root ('ConstructionBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateConstruction($bad,$false,$profile)
  Reject {Construction $bad} $(@{5='DCAENTRY_BASE_CYCLE';6='DCAENTRY_MALFORMED_SIGNATURE';7='DCAENTRY_TYPE_IDENTIFIER'}[$profile])
 }
 $oversized=Join-Path $root 'OwnedOversized.dll';[Task144OwnedFixtures]::CreateOversizedMethodTable($oversized)
 $rowSeen=$false
 try{$null=[Task144.EntrypointMetadata]::ReadConstruction($dca,(Hash $dca),@($oversized,$constructionSdk,$om),@((Hash $oversized),(Hash $constructionSdk),(Hash $om)))}catch{
  $ownedException=$_.Exception
  for($depth=0;$depth -lt 8 -and $null -ne $ownedException;$depth++){
   if($ownedException -is [Task144.EntrypointMetadata+RowLimitObservationException]){$rowSeen=$ownedException.TableName -eq 'METHODDEF' -and $ownedException.RowCount -eq 32769;break}
   $ownedException=$ownedException.InnerException
  }
 }
 Assert $rowSeen 'Construction preserves structured finite table/count before type walking';$passed++
 $worker=Join-Path $PSScriptRoot 'Read-EntrypointMetadata.ps1'
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($oversized,$constructionSdk,$om) -ReferenceSha256 @((Hash $oversized),(Hash $constructionSdk),(Hash $om)) -ProjectConstruction} 'DCAENTRY_ROW_LIMIT_METHODDEF_32769'
 $oldCode=$null
 try{[Task144.EntrypointMetadata]::ReadProject($dca,(Hash $dca),@($oversized),@((Hash $oversized)))}catch{$oldCode=$_.Exception.ToString()}
 Assert ($null -ne $oldCode -and $oldCode.Contains('DCAENTRY_ROW_LIMIT') -and -not $oldCode.Contains('DCAENTRY_ROW_LIMIT_METHODDEF')) 'Old-mode generic row limit remains unchanged';$passed++
 foreach($rows in @(38472,65536)){
  $counted=Join-Path $root ('CountedMethods'+$rows+'.dll');[Task144OwnedFixtures]::CreateCountedConstruction($counted,$rows)
  $accepted=Construction $constructionSdk $counted|ConvertFrom-Json
  Assert ($accepted.operation -eq 'static_dca_project_construction' -and $accepted.selected_roots.Count -eq 5 -and $accepted.runtime_execution -eq 'NOT_RUN') ('Fourth ObjectModel MethodDef '+$rows+' accepted as metadata only');$passed++
 }
 $tooMany=Join-Path $root 'CountedMethods65537.dll';[Task144OwnedFixtures]::CreateCountedConstruction($tooMany,65537)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructionSdk,$tooMany) -ReferenceSha256 @((Hash $reference),(Hash $constructionSdk),(Hash $tooMany)) -ProjectConstruction} 'DCAENTRY_ROW_LIMIT_METHODDEF_65537'
 # Every earlier slot and every earlier mode keeps the original 32768 ceiling.
 Reject {[Task144.EntrypointMetadata]::ReadConstruction($oversized,(Hash $oversized),@($reference,$constructionSdk,$om),@((Hash $reference),(Hash $constructionSdk),(Hash $om)))} 'DCAENTRY_ROW_LIMIT'
 Reject {[Task144.EntrypointMetadata]::ReadConstruction($dca,(Hash $dca),@($reference,$oversized,$om),@((Hash $reference),(Hash $oversized),(Hash $om)))} 'DCAENTRY_ROW_LIMIT'
 Reject {[Task144.EntrypointMetadata]::Read($dca,(Hash $dca),@($oversized),@((Hash $oversized)))} 'DCAENTRY_ROW_LIMIT'
 $otherTable=Join-Path $root 'CountedTypes32769.dll';[Task144OwnedFixtures]::CreateCountedConstruction($otherTable,0,32769)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructionSdk,$otherTable) -ReferenceSha256 @((Hash $reference),(Hash $constructionSdk),(Hash $otherTable)) -ProjectConstruction} 'DCAENTRY_ROW_LIMIT_TYPEDEF_32769'
 $otherOwner=Join-Path $root 'CountedOtherOwner32769.dll';[Task144OwnedFixtures]::CreateCountedConstruction($otherOwner,32769,3,$false)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructionSdk,$otherOwner) -ReferenceSha256 @((Hash $reference),(Hash $constructionSdk),(Hash $otherOwner)) -ProjectConstruction} 'DCAENTRY_ROW_LIMIT_METHODDEF_32769'
 Write-Output ('PASS '+$passed+' / FAIL 0')
}finally{Remove-Item -LiteralPath $root -Recurse -Force}
