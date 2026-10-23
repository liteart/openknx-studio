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
 $id=Join-Path $root 'OwnedProjectId.dll';[Task144OwnedFixtures]::CreateProjectId($id)
 function ProjectId([string] $p=$id){[Task144.EntrypointMetadata]::ReadOwnedProjectId($p,(Hash $p))}
 $idRaw=ProjectId;$idReport=$idRaw|ConvertFrom-Json;$idType=$idReport.types[0]
 Assert ($idReport.operation -eq 'static_dca_project_id' -and $idReport.runtime_execution -eq 'NOT_RUN' -and $idReport.inputs.Count -eq 1 -and $idReport.inputs[0].identity -eq 'Task009.OwnedProjectId, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $idReport.selected_root -eq 'Knx.Ets.Ids.Project.ProjectId') 'Owned identity and sole fixed ProjectId root';$passed++
 Assert ($idType.attributes -eq 257 -and $idType.methods.Count -eq 3 -and $idType.methods[0].name -eq '.ctor' -and $idType.methods[0].accessibility -eq 'Private' -and $idType.methods[1].name -eq 'Parse' -and $idType.methods[2].accessibility -eq 'Family') 'ProjectId raw type flags and constructor/public/protected declarations';$passed++
 Assert ($idType.fields.Count -eq 2 -and $idType.fields[0].attributes -eq 33 -and $idType.fields[0].signature -eq '[mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null]System.Guid' -and $idType.fields[1].signature -eq 'String') 'Declared private/public field flags and bounded signatures';$passed++
 Assert (-not $idRaw.Contains('OWNED_CONSTANT_VALUE') -and -not $idRaw.Contains('PrivateIgnored') -and -not $idRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedProjectId'}).Count -eq 0) 'No field values/private methods/private paths/fixture runtime load';$passed++
 foreach($profile in @(1,2,3,4,5,8,9,10,11,12,13,14,15,16,17,18,20,21,22,23,24,28,30,31)){
  $bad=Join-Path $root ('ProjectIdBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectId($bad,$profile)
  $code=@{1='ROOT_NOT_FOUND';2='DUPLICATE_ROOT';3='MALFORMED_SIGNATURE';4='MALFORMED_SIGNATURE';5='INPUT_IDENTITY';8='FIELD_LIMIT';9='METHOD_LIMIT';10='ROW_LIMIT';11='STRING_LIMIT';12='SIGNATURE_LIMIT';13='SIGNATURE_SHAPE';14='SIGNATURE_SHAPE';15='SIGNATURE_LIMIT';16='REPORT_LIMIT';17='BASE_CYCLE';18='BASE_LIMIT';20='SIGNATURE_SHAPE';21='INTERFACE_LIMIT';22='FIELD_LIMIT';23='METHOD_LIMIT';24='SIGNATURE_SHAPE';28='ROW_LIMIT';30='SIGNATURE_LIMIT';31='MALFORMED_SIGNATURE'}[$profile]
  Reject {ProjectId $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(25,26,27,29)){
  $boundary=Join-Path $root ('ProjectIdBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectId($boundary,$profile)
  $boundaryReport=ProjectId $boundary|ConvertFrom-Json
  Assert ($boundaryReport.types.Count -eq 1 -and ($profile -ne 25 -or $boundaryReport.types[0].fields.Count -eq 256) -and ($profile -ne 26 -or $boundaryReport.types[0].methods.Count -eq 256)) ('ProjectId field/method/table boundary '+$profile);$passed++
 }
 foreach($profile in @(6,19)){
  $positive=Join-Path $root ('ProjectIdChain'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectId($positive,$profile)
  $chain=ProjectId $positive|ConvertFrom-Json
  if($profile -eq 6){Assert ($chain.types.Count -eq 1 -and $chain.base_chain_disposition -eq 'TYPESPEC_BASE_NOT_FOLLOWED' -and $chain.types[0].base_type.Contains('<String>')) 'TypeSpec base declaration retained without ancestry resolution'}
  else{Assert ($chain.types.Count -eq 2 -and $chain.base_chain_disposition -eq 'EXTERNAL_BASE_NOT_RESOLVED') 'Bounded same-assembly base declarations'};$passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectId($broken,('0'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadProjectId($id,(Hash $id))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectId($broken,'43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877')} 'DCAENTRY_INPUT_HASH'
 $idWorker=Join-Path $PSScriptRoot 'Read-ProjectIdMetadata.ps1'
 Reject {& $idWorker -InputPath $id -InputSha256 (Hash $id)} 'DCAENTRY_INPUT_PIN'
 Assert ((Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All earlier reader reports unchanged after ProjectId metadata';$passed++
 $undo=Join-Path $root 'OwnedUndoManager.dll';[Task144OwnedFixtures]::CreateProjectUndoManager($undo)
 function UndoManager([string] $p=$undo){[Task144.EntrypointMetadata]::ReadOwnedProjectUndoManager($p,(Hash $p))}
 $undoRaw=UndoManager;$u=$undoRaw|ConvertFrom-Json;$ut=$u.types[0]
 Assert ($u.operation -eq 'static_dca_project_undo_manager' -and $u.runtime_execution -eq 'NOT_RUN' -and $u.inputs.Count -eq 1 -and $u.inputs[0].identity -eq 'Task009.OwnedUndoManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $u.selected_root -eq 'Owned.Task009.UndoManagerFixture') 'New fixed owned UndoManager root/identity, not vendor impersonation';$passed++
 $undoCtors=@($ut.methods|Where-Object name -eq '.ctor')
 Assert ($ut.attributes -eq 257 -and $ut.methods.Count -eq 4 -and $undoCtors.Count -eq 2 -and $undoCtors[0].accessibility -eq 'Public' -and $undoCtors[1].accessibility -eq 'Family' -and $ut.methods[2].accessibility -eq 'Family' -and -not $undoRaw.Contains('PrivateIgnored')) 'Public/protected ctor/method declarations only, private malformed constructor ignored';$passed++
 Assert ($ut.fields.Count -eq 2 -and $ut.fields[0].attributes -eq 33 -and $ut.fields[0].signature -eq '[mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null]System.Guid' -and -not $undoRaw.Contains('OWNED_CONSTANT_VALUE') -and -not $undoRaw.Contains($root)) 'Field flags/signatures retained, values and paths omitted';$passed++
 foreach($profile in @(1,2,3,4,5,8,9,10,11,12,13,14,15,16,17,18,20,21,22,23,24,30,31,34)){
  $bad=Join-Path $root ('UndoManagerBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectUndoManager($bad,$profile)
  $code=@{1='ROOT_NOT_FOUND';2='DUPLICATE_ROOT';3='MALFORMED_SIGNATURE';4='MALFORMED_SIGNATURE';5='INPUT_IDENTITY';8='FIELD_LIMIT';9='METHOD_LIMIT';10='ROW_LIMIT';11='STRING_LIMIT';12='SIGNATURE_LIMIT';13='SIGNATURE_SHAPE';14='SIGNATURE_SHAPE';15='SIGNATURE_LIMIT';16='REPORT_LIMIT';17='BASE_CYCLE';18='BASE_LIMIT';20='SIGNATURE_SHAPE';21='INTERFACE_LIMIT';22='FIELD_LIMIT';23='METHOD_LIMIT';24='SIGNATURE_SHAPE';30='SIGNATURE_LIMIT';31='MALFORMED_SIGNATURE';34='ROW_LIMIT'}[$profile]
  Reject {UndoManager $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(25,26,27,32,33,35)){
  $boundary=Join-Path $root ('UndoManagerBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectUndoManager($boundary,$profile)
  $boundaryReport=UndoManager $boundary|ConvertFrom-Json
  Assert ($boundaryReport.types.Count -eq 1 -and ($profile -ne 25 -or $boundaryReport.types[0].fields.Count -eq 256) -and ($profile -ne 26 -or $boundaryReport.types[0].methods.Count -eq 256)) ('UndoManager field/method/table boundary '+$profile);$passed++
 }
 foreach($profile in @(6,19)){
  $positive=Join-Path $root ('UndoManagerChain'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectUndoManager($positive,$profile)
  $chain=UndoManager $positive|ConvertFrom-Json
  Assert (($profile -eq 6 -and $chain.types.Count -eq 1 -and $chain.base_chain_disposition -eq 'TYPESPEC_BASE_NOT_FOLLOWED') -or ($profile -eq 19 -and $chain.types.Count -eq 2 -and $chain.base_chain_disposition -eq 'EXTERNAL_BASE_NOT_RESOLVED')) 'UndoManager local/TypeSpec ancestry bound';$passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoManager($broken,('0'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoManager($id,(Hash $id))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectUndoManager($undo,(Hash $undo))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectUndoManager($broken,'73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a')} 'DCAENTRY_INPUT_HASH'
 $undoWorker=Join-Path $PSScriptRoot 'Read-ProjectUndoManagerMetadata.ps1'
 Reject {& $undoWorker -InputPath $undo -InputSha256 (Hash $undo)} 'DCAENTRY_INPUT_PIN'
 Assert ((ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All old report bytes and policies preserved after UndoManager mode';$passed++
 $trace=Join-Path $root 'OwnedTraceEncryptor.dll';[Task144OwnedFixtures]::CreateProjectTraceEncryptor($trace)
 function Trace([string] $p=$trace){[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptor($p,(Hash $p))}
 $traceRaw=Trace;$tr=$traceRaw|ConvertFrom-Json
 Assert ($tr.operation -eq 'static_dca_project_trace_encryptor' -and $tr.runtime_execution -eq 'NOT_RUN' -and $tr.inputs.Count -eq 1 -and $tr.inputs[0].identity -eq 'Task009.OwnedTraceEncryptor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $tr.selected_root -eq 'Owned.Task009.TraceContract' -and $tr.forwarded_declarations -eq 'UNEXAMINED' -and $tr.creation_route -eq 'INCONCLUSIVE') 'Fixed independent owned declaration root/identity and explicit limits';$passed++
 Assert ($tr.types[0].methods.Count -eq 4 -and $tr.types[0].fields.Count -eq 2 -and $tr.candidates.Count -eq 0 -and -not $traceRaw.Contains('PrivateIgnored') -and -not $traceRaw.Contains('OWNED_CONSTANT_VALUE') -and -not $traceRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedTraceEncryptor'}).Count -eq 0) 'Declarations only, no values/private method/path';$passed++
 foreach($profile in @(1,2,3,4,5,8,9,10,11,12,13,14,15,16,17,18,20,21,22,23,24,28,30,31,32,44,45,46)){
  $bad=Join-Path $root ('TraceBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectTraceEncryptor($bad,$profile)
  $code=@{1='ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED';2='DUPLICATE_ROOT';3='MALFORMED_SIGNATURE';4='MALFORMED_SIGNATURE';5='INPUT_IDENTITY';8='FIELD_LIMIT';9='METHOD_LIMIT';10='ROW_LIMIT';11='STRING_LIMIT';12='SIGNATURE_LIMIT';13='SIGNATURE_SHAPE';14='SIGNATURE_SHAPE';15='SIGNATURE_LIMIT';16='REPORT_LIMIT';17='BASE_CYCLE';18='BASE_LIMIT';20='SIGNATURE_SHAPE';21='INTERFACE_LIMIT';22='FIELD_LIMIT';23='METHOD_LIMIT';24='SIGNATURE_SHAPE';28='ROW_LIMIT';30='SIGNATURE_LIMIT';31='MALFORMED_SIGNATURE';32='ROW_LIMIT';44='PROVIDER_TYPE_LIMIT';45='PROVIDER_EDGE_LIMIT';46='INTERFACE_LIMIT'}[$profile]
  Reject {Trace $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(6,19,25,26,27,29,40,41,42,43,48,49,50,51)){
  $positive=Join-Path $root ('TracePositive'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectTraceEncryptor($positive,$profile)
  $t=Trace $positive|ConvertFrom-Json
  if($profile -eq 6){Assert ($t.base_chain_disposition -eq 'TYPESPEC_BASE_NOT_FOLLOWED') 'TypeSpec ancestry not followed'}
  elseif($profile -eq 19){Assert ($t.types.Count -eq 2 -and $t.base_chain_disposition -eq 'EXTERNAL_BASE_NOT_RESOLVED') 'Bounded same-assembly base'}
  elseif($profile -eq 25){Assert ($t.types[0].fields.Count -eq 256) 'Field bound accepted'}
  elseif($profile -eq 26){Assert ($t.types[0].methods.Count -eq 256) 'Method bound accepted'}
  elseif($profile -eq 40){Assert ($t.candidates.Count -eq 2 -and $t.matching_edges -eq 2 -and $t.candidates[0].attributes -eq 128 -and $t.candidates[1].attributes -eq 1 -and $t.base_chain_disposition -eq 'NO_BASE' -and $t.candidates[0].relationships[0].target_assembly -eq $t.inputs[0].identity -and $t.candidates[0].relationships[0].target_token -eq $t.types[0].token -and $t.candidates[0].relationships[0].kind -eq 'implements' -and $null -eq $t.candidates[0].PSObject.Properties['methods']) 'Direct owned internal/abstract candidate identity/token/flags only'}
  elseif($profile -eq 41){Assert ($t.candidates.Count -eq 0 -and $t.uncertainty_counts.TARGET_NAME_OWNER_MISMATCH -eq 1) 'Foreign exact-name collision not matched'}
  elseif($profile -eq 42){Assert ($t.candidates.Count -eq 1 -and -not (@($t.candidates|Where-Object name -eq 'Owned.Candidates.Indirect').Count)) 'Indirect candidate not claimed'}
  elseif($profile -eq 43){Assert ($t.candidates.Count -eq 0 -and $t.uncertainty_counts.TYPESPEC_NOT_INSPECTED -eq 1) 'Generic relationship uncertainty retained'}
  elseif($profile -eq 48){Assert ($t.base_chain_disposition -eq 'NO_BASE' -and ($t.types[0].attributes -band 32) -eq 32) 'Actual interface flag/nil base reported without name inference'}
  elseif($profile -eq 49){Assert ($t.candidates.Count -eq 1 -and $t.matching_edges -eq 1) 'Exact same-owner TypeRef relationship accepted'}
  elseif($profile -eq 50){Assert ($t.matching_edges -eq 128 -and $t.candidates.Count -eq 4) '128 edge boundary accepted'}
  elseif($profile -eq 51){Assert ($t.candidates.Count -eq 64) '64 candidate boundary accepted'}
  else{Assert ($t.types.Count -eq 1) '32768 row boundary accepted'}
  $passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptor($broken,('0'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptor($id,(Hash $id))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptor($trace,(Hash $trace))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptor($broken,'4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd')} 'DCAENTRY_INPUT_HASH'
 $traceWorker=Join-Path $PSScriptRoot 'Read-ProjectTraceEncryptorMetadata.ps1'
 Reject {& $traceWorker -InputPath $trace -InputSha256 (Hash $trace)} 'DCAENTRY_INPUT_PIN'
 Assert ((UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reader bytes unchanged after Common inquiry';$passed++
 $traceConstruction=Join-Path $root 'OwnedTraceConstruction.dll';[Task144OwnedFixtures]::CreateTraceConstruction($traceConstruction)
 function TraceConstruction([string] $p=$traceConstruction){[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorConstruction($p,(Hash $p))}
 $tcRaw=TraceConstruction;$tc=$tcRaw|ConvertFrom-Json;$ct=$tc.types[0]
 Assert ($tc.operation -eq 'static_dca_project_trace_encryptor_construction' -and $tc.runtime_execution -eq 'NOT_RUN' -and $tc.inputs.Count -eq 1 -and $tc.inputs[0].identity -eq 'Task009.OwnedTraceConstruction, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $tc.selected_root -eq 'Owned.Task009.TraceCandidate' -and $ct.token -eq '02000002' -and $tc.creation_route -eq 'INCONCLUSIVE' -and $tc.forwarded_declarations -eq 'UNEXAMINED') 'New fixed independent construction declaration route';$passed++
 $ctors=@($ct.methods|Where-Object declaration_kind -eq 'CONSTRUCTOR_DECLARATION_ONLY');$factories=@($ct.methods|Where-Object declaration_kind -eq 'FACTORY_CANDIDATE_DECLARATION_ONLY')
 Assert ($ctors.Count -eq 6 -and (($ctors.accessibility -join ',') -eq 'Public,Family,Assembly,Private,FamORAssem,FamANDAssem') -and $ctors[0].signature -eq 'instance Void([Owned.Foreign, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.Dependencies.Dependency) generic=0' -and $ctors[0].signature_header -eq 32) 'All constructor access declarations and exact dependency owner retained, not callable proof';$passed++
 Assert ($factories.Count -eq 4 -and ($factories.name -join ',') -eq 'CreateCandidate,CreateInterface,CreateSameOwnerRef,GenericCreate' -and $factories[3].signature -eq 'static Owned.Task009.TraceCandidate() generic=1' -and $factories[0].signature_header -eq 0 -and $factories[3].signature_header -eq 16) 'Only public static exact same-owner return declarations, generic remains explicit';$passed++
 Assert (@($ct.methods|Where-Object {$_.owner_assembly -ne $tc.inputs[0].identity -or $_.owner_type -ne $ct.name -or $_.owner_token -ne $ct.token}).Count -eq 0 -and -not $tcRaw.Contains('ForeignIgnored') -and -not $tcRaw.Contains('InstanceIgnored') -and -not $tcRaw.Contains('PrivateIgnored') -and -not $tcRaw.Contains('VoidIgnored') -and -not $tcRaw.Contains($root) -and $ct.interfaces[0] -eq 'Owned.Task009.ITraceContract' -and $ct.base_type -like '*System.Object') 'Exact declaration ownership/filtering/direct base and interface only';$passed++
 Assert (@([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedTraceConstruction'}).Count -eq 0) 'Owned metadata fixture was not runtime loaded';$passed++
 foreach($profile in @(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,20,23)){
  $bad=Join-Path $root ('TraceConstructionBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateTraceConstruction($bad,$profile)
  $code=@{1='ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED';2='DUPLICATE_ROOT';3='MALFORMED_SIGNATURE';4='SIGNATURE_LIMIT';5='INPUT_IDENTITY';6='ROOT_TOKEN';7='SIGNATURE_SHAPE';8='METHOD_LIMIT';9='INTERFACE_LIMIT';10='ROW_LIMIT';11='STRING_LIMIT';12='SIGNATURE_LIMIT';13='SIGNATURE_SHAPE';14='SIGNATURE_SHAPE';15='SIGNATURE_SHAPE';16='REPORT_LIMIT';20='ROW_LIMIT';23='TYPE_IDENTIFIER'}[$profile]
  Reject {TraceConstruction $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(17,18,19,21,24)){
  $positive=Join-Path $root ('TraceConstructionBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateTraceConstruction($positive,$profile)
  $t=TraceConstruction $positive|ConvertFrom-Json
  Assert ($t.types.Count -eq 1 -and ($profile -ne 17 -or $t.types[0].interfaces.Count -eq 32) -and ($profile -ne 18 -or $t.types[0].methods.Count -eq 256) -and ($profile -ne 24 -or ($t.types[0].methods.Count -eq 0 -and $t.creation_route -eq 'INCONCLUSIVE'))) ('Construction declaration bound/empty route '+$profile);$passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorConstruction($broken,('0'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorConstruction($trace,(Hash $trace))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptorConstruction($traceConstruction,(Hash $traceConstruction))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptorConstruction($broken,'4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd')} 'DCAENTRY_INPUT_HASH'
 $tcWorker=Join-Path $PSScriptRoot 'Read-ProjectTraceEncryptorConstructionMetadata.ps1'
 Reject {& $tcWorker -InputPath $traceConstruction -InputSha256 (Hash $traceConstruction)} 'DCAENTRY_INPUT_PIN'
 Assert ((Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reader bytes unchanged after fixed candidate construction declaration mode';$passed++
 $traceParameter=Join-Path $root 'OwnedTraceParameter.dll';[Task144OwnedFixtures]::CreateTraceParameter($traceParameter)
 function TraceParameter([string] $p=$traceParameter){[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorParameter($p,(Hash $p))}
 $tpRaw=TraceParameter;$tp=$tpRaw|ConvertFrom-Json;$arg=$tp.constructor.parameters[0]
 Assert ($tp.operation -eq 'static_dca_project_trace_encryptor_parameter' -and $tp.runtime_execution -eq 'NOT_RUN' -and $tp.inputs[0].identity -eq 'Task009.OwnedTraceParameter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $tp.selected_root -eq 'Owned.Task009.ParameterCandidate' -and $tp.type_token -eq '02000002' -and $tp.constructor.token -eq '06000001' -and $tp.constructor.attributes -eq 6278 -and $tp.constructor.signature -eq 'instance Void(Int32) generic=0' -and $tp.constructor.signature_header -eq 32 -and $tp.creation_route -eq 'INCONCLUSIVE') 'Fixed unsigned owned parameter contract, not original token impersonation';$passed++
 Assert ($tp.parameter_metadata -eq 'DECLARATIONS_ONLY' -and $arg.sequence -eq 1 -and $arg.token -eq '08000001' -and $arg.name -ceq 'OwnedParameter' -and $arg.attributes -eq 0 -and $arg.default_disposition -eq 'ABSENT' -and $null -eq $arg.default) 'Raw argument declaration and genuine absence, no generated name/default';$passed++
 Assert (-not $tpRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedTraceParameter'}).Count -eq 0) 'No private paths or target runtime load';$passed++
 foreach($profile in @(7,8,9,10,11,12,13,14,15,16,17,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,41,42,44)){
  $bad=Join-Path $root ('TraceParameterBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateTraceParameter($bad,$profile)
  $code=@{7='PARAMETER_DUPLICATE';8='PARAMETER_SEQUENCE';9='PARAMETER_LIMIT';10='DEFAULT_INCONSISTENT';11='DEFAULT_INCONSISTENT';12='CONSTANT_TYPE';13='CONSTANT_TYPE';14='CONSTANT_TYPE';15='CONSTANT_SIZE';16='CONSTANT_SIZE';17='CONSTANT_SIZE';21='STRING_LIMIT';22='CTOR_CONTRACT';23='CTOR_CONTRACT';24='CTOR_CONTRACT';25='CTOR_CONTRACT';26='CTOR_NOT_FOUND';27='ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED';28='DUPLICATE_ROOT';29='ROOT_TOKEN';30='INPUT_IDENTITY';31='TYPE_CONTRACT';32='ROW_LIMIT';33='ROW_LIMIT';34='PARAMETER_LIMIT';35='MALFORMED_SIGNATURE';36='SIGNATURE_LIMIT';41='CTOR_CONTRACT';42='CTOR_CONTRACT';44='CONSTANT_TYPE'}[$profile]
  Reject {TraceParameter $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(1,2,3,4,5,6,18,19,20,37,38,39)){
  $positive=Join-Path $root ('TraceParameterPositive'+$profile+'.dll');[Task144OwnedFixtures]::CreateTraceParameter($positive,$profile)
  $tRaw=TraceParameter $positive;$t=$tRaw|ConvertFrom-Json;$argumentRows=@($t.constructor.parameters|Where-Object sequence -eq 1);$returns=@($t.constructor.parameters|Where-Object sequence -eq 0)
  if($profile -eq 1){Assert ($returns.Count -eq 1 -and $returns[0].default_disposition -eq 'RETURN_NOT_INSPECTED' -and $null -eq $returns[0].default -and $argumentRows[0].default_disposition -eq 'ABSENT' -and -not $tRaw.Contains('RETURN_CONSTANT_MUST_NOT_BE_READ')) 'Return default never inspected or confused with argument'}
  elseif($profile -eq 2){Assert ($argumentRows[0].attributes -eq 4112 -and $argumentRows[0].default.type -eq 'Int32' -and $argumentRows[0].default.bytes -eq 4 -and $argumentRows[0].default.raw_hex -eq '85FFFFFF' -and $argumentRows[0].default.declared_value -eq -123) 'Associated signed little-endian constant only'}
  elseif($profile -eq 3){Assert ($argumentRows[0].attributes -eq 16 -and $argumentRows[0].default_disposition -eq 'ABSENT' -and $null -eq $argumentRows[0].default) 'Optional alone is not a default'}
  elseif($profile -eq 4){Assert ($argumentRows[0].name -ceq '' -and $argumentRows[0].name_handle_is_nil) 'Empty/nil name retained without replacement'}
  elseif($profile -eq 5 -or $profile -eq 6){Assert ($argumentRows.Count -eq 0 -and $t.parameter_metadata -eq 'MISSING_PARAMETER_METADATA' -and $t.creation_route -eq 'INCONCLUSIVE') 'Missing argument row is explicit, return-only row is distinct'}
  elseif($profile -eq 18){Assert ($argumentRows[0].default.declared_value -eq [int]::MinValue -and $argumentRows[0].default.raw_hex -eq '00000080') 'Int32 minimum declared constant'}
  elseif($profile -eq 19){Assert ($argumentRows[0].default.declared_value -eq [int]::MaxValue -and $argumentRows[0].default.raw_hex -eq 'FFFFFF7F') 'Int32 maximum declared constant'}
  elseif($profile -eq 20){Assert ($argumentRows[0].default.declared_value -eq 0 -and $argumentRows[0].default_disposition -eq 'DECLARED_INT32_ONLY') 'Actual owned zero constant distinct from absent default, not a runtime argument'}
  elseif($profile -eq 37){Assert ($returns[0].default_disposition -eq 'RETURN_NOT_INSPECTED' -and $argumentRows[0].default.declared_value -eq -123 -and -not $tRaw.Contains('RETURN_CONSTANT_MUST_NOT_BE_READ')) 'Only sequence1 associated value decoded'}
  elseif($profile -eq 38){Assert ($t.constructor.parameters[0].sequence -eq 1 -and $t.constructor.parameters[1].sequence -eq 0 -and $t.parameter_metadata -eq 'DECLARATIONS_ONLY') 'Sequence numbers, not row position, distinguish argument/return'}
  else{Assert ($argumentRows[0].attributes -eq 8211 -and $argumentRows[0].default_disposition -eq 'ABSENT') 'Raw In/Out/Optional/HasFieldMarshal flags, no custom attribute or marshal interpretation'}
  $passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorParameter($broken,('0'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectTraceEncryptorParameter($traceConstruction,(Hash $traceConstruction))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptorParameter($traceParameter,(Hash $traceParameter))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectTraceEncryptorParameter($broken,'4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd')} 'DCAENTRY_INPUT_HASH'
 $tpWorker=Join-Path $PSScriptRoot 'Read-ProjectTraceEncryptorParameterMetadata.ps1'
 Reject {& $tpWorker -InputPath $traceParameter -InputSha256 (Hash $traceParameter)} 'DCAENTRY_INPUT_PIN'
 Assert ((TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All previous report bytes unchanged after parameter-only profile';$passed++
 Write-Output ('PASS '+$passed+' / FAIL 0')
}finally{Remove-Item -LiteralPath $root -Recurse -Force}
