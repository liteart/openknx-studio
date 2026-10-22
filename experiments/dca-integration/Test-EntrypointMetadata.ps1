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
 $serviceSdk=Join-Path $root 'OwnedServiceSdk.dll';$serviceOm=Join-Path $root 'OwnedServiceObjectModel.dll'
 [Task144OwnedFixtures]::CreateServices($serviceSdk);[Task144OwnedFixtures]::CreateServices($serviceOm,$true)
 function Services([string] $s=$serviceSdk,[string] $o=$serviceOm){[Task144.EntrypointMetadata]::ReadServices($dca,(Hash $dca),@($reference,$s,$o),@((Hash $reference),(Hash $s),(Hash $o)))}
 $serviceRaw=Services;$services=$serviceRaw|ConvertFrom-Json
 Assert ($services.operation -eq 'static_dca_project_services' -and $services.selected_roots.Count -eq 5 -and $services.types.Count -eq 6 -and $services.runtime_execution -eq 'NOT_RUN') 'Fixed five service roots plus local base only';$passed++
 $serviceContext=$services.types|Where-Object name -eq 'Knx.Ets.ObjectModel.IObjectContext'
 $sdkFactory=$services.types|Where-Object name -eq 'Knx.Ets.Sdk.ISdkFactory'
 Assert (($serviceContext.attributes -band 7) -eq 0 -and ($sdkFactory.attributes -band 7) -eq 0 -and @($services.selected_roots|Where-Object base_chain_disposition -eq 'NO_BASE').Count -eq 4) 'Nonpublic service interfaces and explicit nil bases';$passed++
 $execution=$services.types|Where-Object name -eq 'Knx.Ets.Sdk.ExecutionContext'
 Assert (($execution.methods[0].attributes -band 7) -eq 3 -and $execution.methods.Count -eq 3 -and ($execution.methods|Where-Object name -eq 'get_Store').factory_candidate -eq 'DECLARATION_ONLY' -and ($execution.methods|Where-Object name -eq 'ParameterOnly').factory_candidate -eq 'NOT_SELECTED_RETURN') 'Declared constructor/return annotations/private exclusion';$passed++
 $serviceFactory=$services.types|Where-Object name -eq 'Knx.Ets.ObjectModel.IFactory'
 Assert ($serviceFactory.interfaces.Count -eq 1 -and $serviceFactory.interfaces[0].Contains('Owned.IUnselected') -and @($services.types|Where-Object name -eq 'Owned.IUnselected').Count -eq 0 -and -not $serviceRaw.Contains($root)) 'Referenced interface not recursively catalogued/path boundary';$passed++
 Assert ((Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'Earlier mode reports unchanged after isolated service selection';$passed++
 foreach($objectModel in @($false,$true)){foreach($profile in @(1,2)){
  $bad=Join-Path $root ('ServiceBad'+$objectModel+$profile+'.dll');[Task144OwnedFixtures]::CreateServices($bad,$objectModel,$profile)
  $code=if($profile -eq 1){'DCAENTRY_ROOT_NOT_FOUND'}else{'DCAENTRY_DUPLICATE_ROOT'}
  if($objectModel){Reject {Services $serviceSdk $bad} $code}else{Reject {Services $bad} $code}
 }}
 $serviceMethods=Join-Path $root 'ServiceSelectedMethodLimit.dll';[Task144OwnedFixtures]::CreateServices($serviceMethods,$false,3)
 Reject {Services $serviceMethods} 'DCAENTRY_METHOD_LIMIT'
 foreach($rows in @(38472,65536)){
  $counted=Join-Path $root ('ServiceMethodRows'+$rows+'.dll');[Task144OwnedFixtures]::CreateServices($counted,$true,0,$rows)
  $accepted=Services $serviceSdk $counted|ConvertFrom-Json
  Assert ($accepted.operation -eq 'static_dca_project_services' -and $accepted.selected_roots.Count -eq 5) ('New fixed service route fourth MethodDef '+$rows+' accepted');$passed++
 }
 $serviceRows=Join-Path $root 'ServiceMethodRows65537.dll';[Task144OwnedFixtures]::CreateServices($serviceRows,$true,0,65537)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$serviceSdk,$serviceRows) -ReferenceSha256 @((Hash $reference),(Hash $serviceSdk),(Hash $serviceRows)) -ProjectServices} 'DCAENTRY_ROW_LIMIT_METHODDEF_65537'
 $serviceTypes=Join-Path $root 'ServiceTypeRows32769.dll';[Task144OwnedFixtures]::CreateServices($serviceTypes,$true,0,3,32769)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$serviceSdk,$serviceTypes) -ReferenceSha256 @((Hash $reference),(Hash $serviceSdk),(Hash $serviceTypes)) -ProjectServices} 'DCAENTRY_ROW_LIMIT_TYPEDEF_32769'
 Reject {[Task144.EntrypointMetadata]::ReadServices($dca,(Hash $dca),@($oversized,$serviceSdk,$serviceOm),@((Hash $oversized),(Hash $serviceSdk),(Hash $serviceOm)))} 'DCAENTRY_ROW_LIMIT'
 Reject {[Task144.EntrypointMetadata]::ReadServices($broken,(Hash $broken),@($reference,$serviceSdk,$serviceOm),@((Hash $reference),(Hash $serviceSdk),('0'*64)))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadServices($dca,(Hash $dca),@($serviceSdk,$serviceOm),@((Hash $serviceSdk),(Hash $serviceOm)))} 'DCAENTRY_INPUT_COUNT'
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$serviceSdk,$serviceOm) -ReferenceSha256 @((Hash $reference),(Hash $serviceSdk),(Hash $serviceOm)) -ProjectServices -ProjectConstruction} 'DCAENTRY_MODE'
 $providerSdk=Join-Path $root 'ProviderSdk.dll';$providerOm=Join-Path $root 'ProviderObjectModel.dll'
 [Task144OwnedFixtures]::CreateProviders($providerSdk);[Task144OwnedFixtures]::CreateProviders($providerOm,$true)
 function Providers([string] $s=$providerSdk,[string] $o=$providerOm){[Task144.EntrypointMetadata]::ReadProviders($dca,(Hash $dca),@($reference,$s,$o),@((Hash $reference),(Hash $s),(Hash $o)))}
 $providerRaw=Providers;$providers=$providerRaw|ConvertFrom-Json
 Assert ($providers.operation -eq 'static_dca_project_providers' -and $providers.targets.Count -eq 6 -and $providers.candidates.Count -eq 4 -and $providers.matching_edges -eq 8 -and $providers.creation_route -eq 'INCONCLUSIVE' -and $providers.runtime_execution -eq 'NOT_RUN') 'Six exact targets/direct relationship counts/creation uncertainty';$passed++
 $internal=$providers.candidates|Where-Object name -eq 'Owned.InternalProvider';$abstract=$providers.candidates|Where-Object name -eq 'Owned.AbstractProvider'
 Assert (($internal.attributes -band 7) -eq 0 -and $internal.relationships.Count -eq 3 -and ($abstract.attributes -band 128) -eq 128 -and $internal.token -match '^02[0-9A-F]{6}$') 'Internal/abstract attributes and candidate token retained';$passed++
 $cross=$providers.candidates|Where-Object name -eq 'Owned.CrossAssemblyProvider'
 Assert ($cross.relationships[0].target_assembly -ceq $providers.inputs[2].identity -and $cross.relationships[0].target_name -eq 'Knx.Ets.Sdk.ISdkFactory' -and $cross.relationships[0].kind -eq 'implements' -and $cross.relationships[0].target_token -eq '02000003') 'Exact selected TypeRef owner and target token';$passed++
 Assert ($providers.uncertainty_counts.TARGET_NAME_OWNER_MISMATCH -eq 3 -and $providers.uncertainty_counts.TYPESPEC_NOT_INSPECTED -eq 1 -and @($providers.candidates|Where-Object name -match 'ForeignOwner|PrefixProvider|IndirectProvider|GenericProvider').Count -eq 0) 'Foreign name/version/token/prefix excluded; indirect/generic not claimed';$passed++
 Assert (-not $providerRaw.Contains($root) -and @($internal.PSObject.Properties.Name|Where-Object {$_ -in @('methods','constructors')}).Count -eq 0 -and $providers.limitations[0].Contains('indirect')) 'No methods/constructors/private paths or global absence claim';$passed++
 Assert ((Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All previous reader reports preserved after provider lookup';$passed++
 $emptySdk=Join-Path $root 'ProviderEmptySdk.dll';$emptyOm=Join-Path $root 'ProviderEmptyOm.dll';[Task144OwnedFixtures]::CreateProviders($emptySdk,$false,1);[Task144OwnedFixtures]::CreateProviders($emptyOm,$true,1)
 $empty=Providers $emptySdk $emptyOm|ConvertFrom-Json
 Assert ($empty.candidates.Count -eq 0 -and $empty.matching_edges -eq 0 -and $empty.creation_route -eq 'INCONCLUSIVE') 'Empty direct lookup remains inconclusive';$passed++
 foreach($objectModel in @($false,$true)){foreach($profile in @(2,3)){
  $bad=Join-Path $root ('ProviderTargetBad'+$objectModel+$profile+'.dll');[Task144OwnedFixtures]::CreateProviders($bad,$objectModel,$profile)
  $code=if($profile -eq 2){'DCAENTRY_PROVIDER_TARGET_NOT_FOUND'}else{'DCAENTRY_DUPLICATE_PROVIDER_TARGET'}
  if($objectModel){Reject {Providers $providerSdk $bad} $code}else{Reject {Providers $bad} $code}
 }}
 $providerBounds=Join-Path $root 'ProviderBounds.dll';[Task144OwnedFixtures]::CreateProviderBounds($providerBounds,64,1)
 $atLimit=Providers $providerBounds $emptyOm|ConvertFrom-Json
 Assert ($atLimit.candidates.Count -eq 64 -and $atLimit.matching_edges -eq 128) 'Exact candidate/edge ceilings accepted';$passed++
 [Task144OwnedFixtures]::CreateProviderBounds($providerBounds,65);Reject {Providers $providerBounds $emptyOm} 'DCAENTRY_PROVIDER_TYPE_LIMIT'
 [Task144OwnedFixtures]::CreateProviderBounds($providerBounds,64,1,$true);Reject {Providers $providerBounds $emptyOm} 'DCAENTRY_PROVIDER_EDGE_LIMIT'
 foreach($rows in @(38472,65536)){
  $counted=Join-Path $root ('ProviderMethodRows'+$rows+'.dll');[Task144OwnedFixtures]::CreateProviders($counted,$true,1,$rows)
  $accepted=Providers $emptySdk $counted|ConvertFrom-Json
  Assert ($accepted.candidates.Count -eq 0 -and $accepted.creation_route -eq 'INCONCLUSIVE') ('Provider fourth MethodDef '+$rows+' accepted without method decoding');$passed++
 }
 $providerRows=Join-Path $root 'ProviderMethodRows65537.dll';[Task144OwnedFixtures]::CreateProviders($providerRows,$true,1,65537)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$emptySdk,$providerRows) -ReferenceSha256 @((Hash $reference),(Hash $emptySdk),(Hash $providerRows)) -ProjectProviders} 'DCAENTRY_ROW_LIMIT_METHODDEF_65537'
 Reject {[Task144.EntrypointMetadata]::ReadProviders($dca,(Hash $dca),@($oversized,$providerSdk,$providerOm),@((Hash $oversized),(Hash $providerSdk),(Hash $providerOm)))} 'DCAENTRY_ROW_LIMIT'
 Reject {[Task144.EntrypointMetadata]::ReadProviders($broken,(Hash $broken),@($reference,$providerSdk,$providerOm),@((Hash $reference),(Hash $providerSdk),('0'*64)))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadProviders($dca,(Hash $dca),@($providerSdk,$providerOm),@((Hash $providerSdk),(Hash $providerOm)))} 'DCAENTRY_INPUT_COUNT'
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$providerSdk,$providerOm) -ReferenceSha256 @((Hash $reference),(Hash $providerSdk),(Hash $providerOm)) -ProjectProviders -ProjectServices} 'DCAENTRY_MODE'
 $constructorSdk=Join-Path $root 'ConstructorSdk.dll';$constructorOm=Join-Path $root 'ConstructorOm.dll'
 [Task144OwnedFixtures]::CreateConstructorCandidates($constructorSdk);[Task144OwnedFixtures]::CreateConstructorCandidates($constructorOm,$true)
 function Constructors([string] $s=$constructorSdk,[string] $o=$constructorOm){[Task144.EntrypointMetadata]::ReadConstructors($dca,(Hash $dca),@($reference,$s,$o),@((Hash $reference),(Hash $s),(Hash $o)))}
 $constructorRaw=Constructors;$constructors=$constructorRaw|ConvertFrom-Json
 Assert ($constructors.operation -eq 'static_dca_project_constructors' -and $constructors.selected_roots.Count -eq 5 -and $constructors.types.Count -eq 5 -and $constructors.runtime_execution -eq 'NOT_RUN' -and $constructors.creation_route -eq 'INCONCLUSIVE') 'Fixed five constructor roots and no creation claim';$passed++
 $dcaConstructor=$constructors.types|Where-Object name -eq 'Knx.Ets.Sdk.DcaExecutionContext'
 Assert ($dcaConstructor.constructors.Count -eq 4 -and $dcaConstructor.constructors[0].accessibility -eq 'Private' -and $dcaConstructor.constructors[1].accessibility -eq 'Family' -and $dcaConstructor.constructors[2].accessibility -eq 'Public' -and $dcaConstructor.constructors[3].name -eq '.cctor' -and $dcaConstructor.constructors[3].is_static) 'Every constructor visibility and static declaration preserved';$passed++
 $ctxConstructor=$constructors.types|Where-Object name -eq 'Knx.Ets.Sdk.Context'
 Assert ($ctxConstructor.constructors[0].accessibility -eq 'Assembly' -and $ctxConstructor.constructors[0].signature -eq 'instance Void(Object) generic=0' -and -not $ctxConstructor.constructors[0].is_static -and ($ctxConstructor.attributes -band 7) -eq 0) 'Internal constructor signature and nonpublic type flags';$passed++
 Assert (@($constructors.types|Where-Object name -match 'UnselectedBase|FileProjectStore').Count -eq 0 -and @($dcaConstructor.PSObject.Properties.Name|Where-Object {$_ -in @('methods','interfaces')}).Count -eq 0 -and -not $constructorRaw.Contains('UnrelatedMalformedMethod') -and -not $constructorRaw.Contains($root)) 'No inherited/store constructors, nonconstructor signatures, interfaces or paths';$passed++
 Assert ((Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All earlier reader reports preserved after constructor-only selection';$passed++
 foreach($objectModel in @($false,$true)){foreach($profile in @(1,2,3,4,5)){
  $bad=Join-Path $root ('ConstructorBad'+$objectModel+$profile+'.dll');[Task144OwnedFixtures]::CreateConstructorCandidates($bad,$objectModel,$profile)
  $code=@{1='DCAENTRY_ROOT_NOT_FOUND';2='DCAENTRY_DUPLICATE_ROOT';3='DCAENTRY_MALFORMED_SIGNATURE';4='DCAENTRY_METHOD_LIMIT';5='DCAENTRY_REFERENCE_LIMIT'}[$profile]
  if($objectModel){Reject {Constructors $constructorSdk $bad} $code}else{Reject {Constructors $bad} $code}
 }}
 foreach($rows in @(38472,65536)){
  $counted=Join-Path $root ('ConstructorMethodRows'+$rows+'.dll');[Task144OwnedFixtures]::CreateConstructorCandidates($counted,$true,0,$rows)
  $accepted=Constructors $constructorSdk $counted|ConvertFrom-Json
  Assert ($accepted.types.Count -eq 5 -and $accepted.creation_route -eq 'INCONCLUSIVE') ('Constructor mode fourth MethodDef '+$rows+' accepted without unrelated methods');$passed++
 }
 $constructorRows=Join-Path $root 'ConstructorMethodRows65537.dll';[Task144OwnedFixtures]::CreateConstructorCandidates($constructorRows,$true,0,65537)
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructorSdk,$constructorRows) -ReferenceSha256 @((Hash $reference),(Hash $constructorSdk),(Hash $constructorRows)) -ProjectConstructors} 'DCAENTRY_ROW_LIMIT_METHODDEF_65537'
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructorSdk,$otherTable) -ReferenceSha256 @((Hash $reference),(Hash $constructorSdk),(Hash $otherTable)) -ProjectConstructors} 'DCAENTRY_ROW_LIMIT_TYPEDEF_32769'
 Reject {[Task144.EntrypointMetadata]::ReadConstructors($dca,(Hash $dca),@($oversized,$constructorSdk,$constructorOm),@((Hash $oversized),(Hash $constructorSdk),(Hash $constructorOm)))} 'DCAENTRY_ROW_LIMIT'
 Reject {[Task144.EntrypointMetadata]::ReadConstructors($broken,(Hash $broken),@($reference,$constructorSdk,$constructorOm),@((Hash $reference),(Hash $constructorSdk),('0'*64)))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadConstructors($dca,(Hash $dca),@($constructorSdk,$constructorOm),@((Hash $constructorSdk),(Hash $constructorOm)))} 'DCAENTRY_INPUT_COUNT'
 Reject {& $worker -DcaPath $dca -DcaSha256 (Hash $dca) -ReferencePath @($reference,$constructorSdk,$constructorOm) -ReferenceSha256 @((Hash $reference),(Hash $constructorSdk),(Hash $constructorOm)) -ProjectConstructors -ProjectProviders} 'DCAENTRY_MODE'
 Write-Output ('PASS '+$passed+' / FAIL 0')
}finally{Remove-Item -LiteralPath $root -Recurse -Force}
