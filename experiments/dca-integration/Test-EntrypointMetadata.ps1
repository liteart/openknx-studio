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
 $undoStore=Join-Path $root 'OwnedUndoStoreProvider.dll';[Task144OwnedFixtures]::CreateUndoStoreProvider($undoStore)
 function UndoStore([string] $p=$undoStore){[Task144.EntrypointMetadata]::ReadOwnedProjectUndoStoreProvider($p,(Hash $p))}
 $usRaw=UndoStore;$us=$usRaw|ConvertFrom-Json;$ust=$us.types[0]
 Assert ($us.operation -eq 'static_dca_project_undo_store_provider' -and $us.runtime_execution -eq 'NOT_RUN' -and $us.inputs.Count -eq 1 -and $us.inputs[0].identity -eq 'Task009.OwnedUndoStoreProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $us.selected_root -eq 'Owned.Task009.IUndoStoreProviderFixture') 'Provider fixed unsigned owned identity/root, no original impersonation';$passed++
 Assert ($us.declaration_scope -eq 'ROOT_ONLY' -and $us.referenced_declarations -eq 'UNRESOLVED_NOT_FOLLOWED' -and $us.qualification -eq 'DECLARATIONS_ONLY_NOT_USABLE_SERVICE' -and $us.types.Count -eq 1 -and $ust.attributes -eq 161 -and $ust.generic_arity -eq 0) 'Provider public interface flags, root-only/unresolved qualification';$passed++
 Assert ($ust.interfaces.Count -eq 2 -and $ust.interfaces[0] -eq 'Owned.Task009.UnresolvedBase' -and $ust.interfaces[1] -eq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.IBase') 'Direct local/external interfaces named but never followed';$passed++
 Assert ($ust.methods.Count -eq 3 -and $ust.methods[0].signature -eq 'instance String() generic=0' -and $ust.methods[0].signature_header -eq 32 -and $ust.methods[0].calling_convention -eq 'Default' -and $ust.methods[0].is_instance -and $ust.methods[0].return_type -eq 'String' -and $ust.methods[0].parameter_types.Count -eq 0) 'Exact public accessor signature/header/return declaration';$passed++
 Assert ($ust.methods[1].signature -eq 'instance Void(Int32,String[]) generic=0' -and $ust.methods[1].parameter_types.Count -eq 2 -and $ust.methods[1].parameter_types[0] -eq 'Int32' -and $ust.methods[1].parameter_types[1] -eq 'String[]' -and $ust.methods[2].generic_arity -eq 1 -and $ust.methods[2].signature_header -eq 48 -and $ust.methods[2].signature -eq 'instance !!0(!!0) generic=1') 'Parameter and method generic declarations are explicit, not service semantics';$passed++
 Assert (-not $usRaw.Contains('FIELD_MUST_NOT_BE_READ') -and -not $usRaw.Contains('PRIVATE_MALFORMED') -and -not $usRaw.Contains('INHERITED_MALFORMED') -and -not $usRaw.Contains('"fields"') -and -not $usRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedUndoStoreProvider'}).Count -eq 0) 'No fields/private/inherited declarations, private paths or target load';$passed++
 foreach($profile in @(1,2,3,4,5,6,8,10,11,12,13,14,15,16,17,18,19,20,21,22,24)){
  $bad=Join-Path $root ('UndoStoreBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateUndoStoreProvider($bad,$profile)
  $code=@{1='INPUT_IDENTITY';2='ROOT_NOT_FOUND';3='DUPLICATE_ROOT';4='ROOT_NOT_PUBLIC';5='ROOT_NOT_INTERFACE';6='METHOD_LIMIT';8='INTERFACE_LIMIT';10='UNSUPPORTED_GENERIC_ROOT';11='UNSUPPORTED_INTERFACE_BASE';12='MALFORMED_SIGNATURE';13='SIGNATURE_SHAPE';14='SIGNATURE_LIMIT';15='UNSUPPORTED_SIGNATURE';16='SIGNATURE_LIMIT';17='SIGNATURE_LIMIT';18='SIGNATURE_SHAPE';19='STRING_LIMIT';20='SIGNATURE_LIMIT';21='REPORT_LIMIT';22='ROW_LIMIT';24='ROW_LIMIT'}[$profile]
  Reject {UndoStore $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(22,24)){
  $rowPath=Join-Path $root ('UndoStoreBad'+$profile+'.dll');$seen=$false
  try{$null=UndoStore $rowPath}catch{$rowException=$_.Exception;for($depth=0;$depth -lt 8 -and $null -ne $rowException;$depth++){
   if($rowException.GetType().FullName -eq 'Task144.EntrypointMetadata+RowLimitObservationException'){$seen=$rowException.TableName -eq $(if($profile -eq 22){'TYPEDEF'}else{'METHODDEF'}) -and $rowException.RowCount -eq $(if($profile -eq 22){32769}else{65537});break};$rowException=$rowException.InnerException
  }}
  Assert $seen ('Provider exact diagnostic table/count '+$profile);$passed++
 }
 foreach($profile in @(7,9,23,25,26,28)){
  $positive=Join-Path $root ('UndoStoreBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateUndoStoreProvider($positive,$profile)
  $v=UndoStore $positive|ConvertFrom-Json
  Assert ($v.types.Count -eq 1 -and ($profile -ne 7 -or $v.types[0].methods.Count -eq 256) -and ($profile -ne 9 -or $v.types[0].interfaces.Count -eq 32) -and ($profile -ne 28 -or $v.types[0].methods[0].name.Length -eq 1024)) ('Provider exact method/interface/string/table boundary '+$profile);$passed++
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoStoreProvider($undoStore,('f'*63))} 'DCAENTRY_HASH_FORMAT'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoStoreProvider($undoStore,('f'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoStoreProvider($undo,(Hash $undo))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectUndoStoreProvider($undoStore,(Hash $undoStore))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectUndoStoreProvider($broken,'73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a')} 'DCAENTRY_INPUT_HASH'
 $oversize=Join-Path $root 'UndoStoreOversize.dll';$stream=[IO.File]::Create($oversize);try{$stream.SetLength(16*1024*1024+1)}finally{$stream.Dispose()}
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectUndoStoreProvider($oversize,('0'*64))} 'DCAENTRY_INPUT_FILE'
 $usWorker=Join-Path $PSScriptRoot 'Read-ProjectUndoStoreProviderMetadata.ps1'
 Reject {& $usWorker -InputPath $undoStore -InputSha256 (Hash $undoStore)} 'DCAENTRY_INPUT_PIN'
 Assert ((TraceParameter) -ceq $tpRaw -and (TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reports byte-identical after root-only provider metadata';$passed++
 $streamStore=Join-Path $root 'OwnedProjectStreamStore.dll';[Task144OwnedFixtures]::CreateProjectStreamStore($streamStore)
 function StreamStore([string] $p=$streamStore){[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($p,(Hash $p))}
 $ssRaw=StreamStore;$ss=$ssRaw|ConvertFrom-Json;$sst=$ss.types[0]
 Assert ($ss.operation -eq 'static_dca_project_stream_store' -and $ss.runtime_execution -eq 'NOT_RUN' -and $ss.inputs.Count -eq 1 -and $ss.inputs[0].identity -eq 'Task009.OwnedProjectStreamStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and $ss.selected_root -eq 'Owned.Task009.IProjectStreamStoreFixture') 'Stream-store fixed unsigned owned identity/root, no original impersonation';$passed++
 Assert ($ss.declaration_scope -eq 'ROOT_ONLY' -and $ss.referenced_declarations -eq 'UNRESOLVED_NOT_FOLLOWED' -and $ss.qualification -eq 'DECLARATIONS_ONLY_NOT_USABLE_SERVICE' -and $ss.types.Count -eq 1 -and $sst.attributes -eq 161 -and $sst.generic_arity -eq 0 -and $sst.visibility -eq 'Public' -and $sst.class_semantics_raw -eq 32 -and $sst.class_semantics -eq 'Interface' -and $sst.is_interface -and $null -eq $sst.base_type) 'Provider public interface flags, root-only/unresolved qualification';$passed++
 Assert ($sst.interfaces.Count -eq 2 -and $sst.interfaces[0] -eq 'Owned.Task009.UnresolvedBase' -and $sst.interfaces[1] -eq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.IBase') 'Direct local/external interfaces named but never followed';$passed++
 Assert ($sst.methods.Count -eq 3 -and $sst.methods[0].signature -eq 'instance String() generic=0' -and $sst.methods[0].signature_header -eq 32 -and $sst.methods[0].calling_convention -eq 'Default' -and $sst.methods[0].is_instance -and $sst.methods[0].return_type -eq 'String' -and $sst.methods[0].parameter_types.Count -eq 0) 'Exact public accessor signature/header/return declaration';$passed++
 Assert ($sst.methods[1].signature -eq 'instance Void(Int32,String[]) generic=0' -and $sst.methods[1].parameter_types.Count -eq 2 -and $sst.methods[1].parameter_types[0] -eq 'Int32' -and $sst.methods[1].parameter_types[1] -eq 'String[]' -and $sst.methods[2].generic_arity -eq 1 -and $sst.methods[2].signature_header -eq 48 -and $sst.methods[2].signature -eq 'instance !!0(!!0) generic=1') 'Parameter and method generic declarations are explicit, not service semantics';$passed++
 Assert (-not $ssRaw.Contains('FIELD_MUST_NOT_BE_READ') -and -not $ssRaw.Contains('PRIVATE_MALFORMED') -and -not $ssRaw.Contains('INHERITED_MALFORMED') -and -not $ssRaw.Contains('"fields"') -and -not $ssRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedProjectStreamStore'}).Count -eq 0) 'No fields/private/inherited declarations, private paths or target load';$passed++
 foreach($profile in @(1,2,3,4,6,8,10,12,13,14,15,16,17,18,19,20,21,22,24)){
  $bad=Join-Path $root ('StreamStoreBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectStreamStore($bad,$profile)
  $code=@{1='INPUT_IDENTITY';2='ROOT_NOT_FOUND';3='DUPLICATE_ROOT';4='ROOT_NOT_PUBLIC';6='METHOD_LIMIT';8='INTERFACE_LIMIT';10='UNSUPPORTED_GENERIC_ROOT';12='MALFORMED_SIGNATURE';13='SIGNATURE_SHAPE';14='SIGNATURE_LIMIT';15='UNSUPPORTED_SIGNATURE';16='SIGNATURE_LIMIT';17='SIGNATURE_LIMIT';18='SIGNATURE_SHAPE';19='STRING_LIMIT';20='SIGNATURE_LIMIT';21='REPORT_LIMIT';22='ROW_LIMIT';24='ROW_LIMIT'}[$profile]
  Reject {StreamStore $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(22,24)){
  $rowPath=Join-Path $root ('StreamStoreBad'+$profile+'.dll');$seen=$false
  try{$null=StreamStore $rowPath}catch{$rowException=$_.Exception;for($depth=0;$depth -lt 8 -and $null -ne $rowException;$depth++){
   if($rowException.GetType().FullName -eq 'Task144.EntrypointMetadata+RowLimitObservationException'){$seen=$rowException.TableName -eq $(if($profile -eq 22){'TYPEDEF'}else{'METHODDEF'}) -and $rowException.RowCount -eq $(if($profile -eq 22){32769}else{65537});break};$rowException=$rowException.InnerException
  }}
  Assert $seen ('Provider exact diagnostic table/count '+$profile);$passed++
 }
 foreach($profile in @(7,9,23,25,26,28)){
  $positive=Join-Path $root ('StreamStoreBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectStreamStore($positive,$profile)
  $v=StreamStore $positive|ConvertFrom-Json
  Assert ($v.types.Count -eq 1 -and ($profile -ne 7 -or $v.types[0].methods.Count -eq 256) -and ($profile -ne 9 -or $v.types[0].interfaces.Count -eq 32) -and ($profile -ne 28 -or $v.types[0].methods[0].name.Length -eq 1024)) ('Provider exact method/interface/string/table boundary '+$profile);$passed++
 }
 foreach($profile in @(5,11,29)){
  $kindPath=Join-Path $root ('StreamStoreKind'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectStreamStore($kindPath,$profile)
  $kind=StreamStore $kindPath|ConvertFrom-Json;$kt=$kind.types[0]
  Assert ($kind.types.Count -eq 1 -and $kind.referenced_declarations -eq 'UNRESOLVED_NOT_FOLLOWED' -and $kt.name -eq 'Owned.Task009.IProjectStreamStoreFixture' -and $kt.visibility -eq 'Public' -and $kt.generic_arity -eq 0) ('Root-only actual kind/name/visibility '+$profile);$passed++
  if($profile -eq 11){Assert ($kt.is_interface -and $kt.class_semantics_raw -eq 32 -and $kt.base_type -eq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.IBase') 'Non-nil metadata base recorded as unresolved even on interface flag';$passed++}
  else{Assert (-not $kt.is_interface -and $kt.class_semantics -eq 'Class' -and $kt.class_semantics_raw -eq 0 -and $kt.attributes -eq 1 -and $kt.base_type -eq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.BaseType') 'I-prefixed class observed honestly with unresolved base';$passed++}
  if($profile -eq 29){Assert ($kt.methods[0].name -eq '.ctor' -and $kt.methods[0].signature -eq 'instance Void() generic=0' -and $kt.methods[0].attributes -eq 6150 -and $kt.methods[1].signature_header -eq 0 -and -not $kt.methods[1].is_instance -and $kt.methods[1].attributes -eq 22 -and $kt.methods[1].signature -eq 'static Void(Int32,String[]) generic=0') 'Constructor/static declarations only, no invocation';$passed++}
 }
 $streamMalformed=$false
 try{[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($broken,(Hash $broken))}catch{
  $metadataFailure=$_.Exception
  for($depth=0;$depth -lt 8 -and $null -ne $metadataFailure;$depth++){
   if($metadataFailure -is [BadImageFormatException]){$streamMalformed=$true;break};$metadataFailure=$metadataFailure.InnerException
  }
 }
 Assert $streamMalformed 'Malformed owned PE rejected before root declarations';$passed++
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($streamStore,('f'*63))} 'DCAENTRY_HASH_FORMAT'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($streamStore,('f'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($undo,(Hash $undo))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectStreamStore($streamStore,(Hash $streamStore))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectStreamStore($broken,'73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a')} 'DCAENTRY_INPUT_HASH'
 $oversize=Join-Path $root 'StreamStoreOversize.dll';$stream=[IO.File]::Create($oversize);try{$stream.SetLength(16*1024*1024+1)}finally{$stream.Dispose()}
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectStreamStore($oversize,('0'*64))} 'DCAENTRY_INPUT_FILE'
 $ssWorker=Join-Path $PSScriptRoot 'Read-ProjectStreamStoreMetadata.ps1'
 Reject {& $ssWorker -InputPath $streamStore -InputSha256 (Hash $streamStore)} 'DCAENTRY_INPUT_PIN'
 Assert ((UndoStore) -ceq $usRaw -and (TraceParameter) -ceq $tpRaw -and (TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reports byte-identical after stream-store kind/root-only inquiry';$passed++
 $catalog=Join-Path $root 'OwnedOnlineCatalog.dll';[Task144OwnedFixtures]::CreateOnlineCatalogReferences($catalog)
 function Catalog([string] $p=$catalog){[Task144.EntrypointMetadata]::ReadOwnedProjectOnlineCatalogReferences($p,(Hash $p))}
 $catalogRaw=Catalog;$cm=$catalogRaw|ConvertFrom-Json
 Assert ($cm.operation -ceq 'static_dca_project_online_catalog_references' -and $cm.runtime_execution -ceq 'NOT_RUN' -and $cm.selected_assembly_reference -ceq 'Owned.OnlineCatalog.Download, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null') 'Catalog fixed unsigned input/target declaration report';$passed++
 Assert ($cm.assembly_reference.token -ceq '23000001' -and $cm.assembly_reference.flags -eq 0 -and $cm.type_references.Count -eq 2 -and $cm.member_references.Count -eq 2) 'Catalog actual token/direct and nested TypeRef/MemberRef selection';$passed++
 Assert ($cm.type_references[1].namespace -ceq '' -and $cm.type_references[1].scope_kind -ceq 'TypeReference' -and $cm.type_references[1].scope_chain.Count -eq 3 -and $cm.type_references[1].qualified_name.EndsWith('Owned.Catalog.Api+Nested')) 'Catalog token-proven nested chain';$passed++
 Assert ($cm.member_references[0].kind -ceq 'Method' -and $cm.member_references[0].signature -ceq 'instance String(Boolean) generic=0' -and $cm.member_references[0].signature_header -eq 32 -and $cm.member_references[1].kind -ceq 'Field' -and $cm.member_references[1].field_type -ceq 'String') 'Catalog method and field signatures without values';$passed++
 Assert ($cm.counts.global_unexamined_typespec_parent_members -eq 2 -and $cm.counts.non_selected_typeref_parent_members -eq 1 -and $cm.counts.other_unresolved_parent_kinds.ModuleReference -eq 1 -and $cm.typespec_parent_scope -ceq 'GLOBAL_NOT_TARGET_SPECIFIC_UNEXAMINED_TYPESPEC_PARENT') 'Global generic omission/other parent counts; no return-type ownership';$passed++
 Assert (-not $catalogRaw.Contains($root) -and -not $catalogRaw.Contains('NotTargetDespiteReturnType') -and -not $catalogRaw.Contains('OmittedGeneric') -and $cm.referenced_declarations -ceq 'UNRESOLVED_NOT_FOLLOWED') 'Catalog no paths/candidate/closure or omitted member details';$passed++
 foreach($profile in @(1,2,3,4,5,6,7,8,9,11,14,15,16,17,18,19,20,21,22,23,24,26,27,28,33,37,38,39,40,41)){
  $bad=Join-Path $root ('CatalogBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateOnlineCatalogReferences($bad,$profile)
  $code=@{1='ASSEMBLY_REFERENCE_NOT_FOUND';2='DUPLICATE_ASSEMBLY_REFERENCE';3='INPUT_IDENTITY';4='ASSEMBLY_REFERENCE_NOT_FOUND';5='ASSEMBLY_REFERENCE_NOT_FOUND';6='ASSEMBLY_REFERENCE_NOT_FOUND';7='ASSEMBLY_REFERENCE_NOT_FOUND';8='TYPE_SCOPE_CYCLE';9='SCOPE_LIMIT';11='TYPE_SCOPE_NAMESPACE';14='TYPE_SCOPE_INVALID';15='MALFORMED_SIGNATURE';16='SIGNATURE_SHAPE';17='SIGNATURE_LIMIT';18='SIGNATURE_SHAPE';19='SIGNATURE_SHAPE';20='STRING_LIMIT';21='TYPE_IDENTIFIER';22='TYPE_REFERENCE_LIMIT';23='MEMBER_REFERENCE_LIMIT';24='REFERENCE_LIMIT';26='ROW_LIMIT';27='ROW_LIMIT';28='REPORT_LIMIT';33='TYPE_SCOPE_CYCLE';37='SIGNATURE_LIMIT';38='SIGNATURE_LIMIT';39='SIGNATURE_LIMIT';40='SIGNATURE_LIMIT';41='UNSUPPORTED_SIGNATURE'}[$profile]
  Reject {Catalog $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(10,12,13,25,29,30,31,32,36)){
  $valid=Join-Path $root ('CatalogBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateOnlineCatalogReferences($valid,$profile);$cr=Catalog $valid|ConvertFrom-Json
  Assert ($cr.runtime_execution -ceq 'NOT_RUN' -and $cr.selected_assembly_reference -ceq $cm.selected_assembly_reference) ('Catalog accepted bounded profile '+$profile);$passed++
  if($profile -eq 10){Assert ($cr.type_references.Count -eq 18) 'Catalog sixteen nested TypeRef nodes accepted';$passed++}
  if($profile -in @(12,13)){Assert ($cr.type_references.Count -eq 0 -and $cr.member_references.Count -eq 0 -and @($cr.counts.unresolved_type_scopes.psobject.Properties).Count -gt 0) 'Nil/module uncertainty explicit without false target selection';$passed++}
  if($profile -eq 30){Assert ($cr.type_references.Count -eq 256) 'Catalog exact256 selected types';$passed++}
  if($profile -eq 31){Assert ($cr.member_references.Count -eq 256) 'Catalog exact256 selected members';$passed++}
  if($profile -eq 32){Assert ($cr.counts.assembly_references -eq 256) 'Catalog exact256 assembly refs';$passed++}
  if($profile -eq 36){Assert ($cr.member_references[0].parameter_types.Count -eq 31) 'Catalog valid128-byte signature';$passed++}
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectOnlineCatalogReferences($catalog,('f'*63))} 'DCAENTRY_HASH_FORMAT'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectOnlineCatalogReferences($catalog,('f'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectOnlineCatalogReferences($streamStore,(Hash $streamStore))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadProjectOnlineCatalogReferences($catalog,(Hash $catalog))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectOnlineCatalogReferences($broken,'73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a')} 'DCAENTRY_INPUT_HASH'
 $catalogWorker=Join-Path $PSScriptRoot 'Read-ProjectOnlineCatalogReferences.ps1'
 Reject {& $catalogWorker -InputPath $catalog -InputSha256 (Hash $catalog)} 'DCAENTRY_INPUT_PIN'
 Assert ((StreamStore) -ceq $ssRaw -and (Catalog) -ceq $catalogRaw -and (UndoStore) -ceq $usRaw -and (TraceParameter) -ceq $tpRaw -and (TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All old reports byte-identical after catalog reference map';$passed++
 $collection=Join-Path $root 'OwnedProjectCollection.dll';[Task144OwnedFixtures]::CreateProjectCollection($collection)
 function ProjectCollection([string] $p=$collection){[Task144.EntrypointMetadata]::ReadOwnedProjectCollection($p,(Hash $p))}
 $collectionRaw=ProjectCollection;$pc=$collectionRaw|ConvertFrom-Json;$pct=$pc.types[0]
 Assert ($pc.operation -ceq 'static_dca_project_collection' -and $pc.runtime_execution -ceq 'NOT_RUN' -and $pc.selected_root -ceq 'Owned.Task009.ProjectCollectionFixture' -and $pc.inputs[0].identity -ceq 'Task009.OwnedProjectCollection, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null') 'Collection fixed unsigned identity/root; not SDK collection';$passed++
 Assert ($pc.types.Count -eq 1 -and $pct.token -ceq '02000003' -and $pct.attributes -eq 257 -and $pct.visibility -ceq 'Public' -and $pct.generic_arity -eq 0 -and -not $pct.is_interface -and $pct.class_semantics -ceq 'Class' -and $pct.base_type -ceq 'Owned.Task009.UnresolvedBase') 'Collection actual kind/flags/unresolved base, no ancestry traversal';$passed++
 Assert ($pct.interfaces.Count -eq 2 -and $pct.interfaces[0] -ceq 'Owned.Task009.UnresolvedInterface' -and $pct.interfaces[1] -ceq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.IBase') 'Collection local/external interface references not followed';$passed++
 Assert ($pct.constructor_count -eq 4 -and $pct.public_constructor_count -eq 1 -and $pct.constructors.Count -eq 4 -and $pct.constructors[0].accessibility -ceq 'Public' -and $pct.constructors[1].accessibility -ceq 'Assembly' -and $pct.constructors[2].accessibility -ceq 'Private' -and $pct.constructors[3].name -ceq '.cctor') 'All public/internal/private/static constructor declarations and public-instance count';$passed++
 Assert ($pct.constructors[3].is_static -and -not $pct.constructors[3].is_instance -and $pct.constructors[3].signature_header -eq 0 -and $pct.constructors[1].signature -ceq 'instance Void(String) generic=0') 'Raw static versus signature-instance flags/constructor arguments';$passed++
 Assert ($pct.methods.Count -eq 3 -and $pct.methods[0].is_static -and -not $pct.methods[0].is_instance -and $pct.methods[0].return_type -ceq 'Owned.Task009.ProjectCollectionFixture' -and $pct.methods[1].signature -ceq 'instance String() generic=0' -and $pct.methods[2].signature -ceq 'instance !!0(!!0) generic=1') 'Public static root-return/getter/generic signatures are declarations only';$passed++
 Assert (-not $collectionRaw.Contains('MALFORMED_MUST_NOT_BE_READ') -and -not $collectionRaw.Contains('FIELD_VALUE_MUST_NOT_BE_READ') -and -not $collectionRaw.Contains('OWNED_CONSTANT_VALUE_MUST_NOT_BE_READ') -and -not $collectionRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedProjectCollection'}).Count -eq 0) 'Collection no private nonconstructor/inherited/fields/values/paths or target load';$passed++
 foreach($profile in @(1,2,3,4,6,7,9,11,13,14,15,16,17,18,19,20,22,24,28,31,32,37)){
  $bad=Join-Path $root ('CollectionBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectCollection($bad,$profile)
  $code=@{1='INPUT_IDENTITY';2='ROOT_NOT_FOUND';3='DUPLICATE_ROOT';4='ROOT_NOT_PUBLIC';6='UNSUPPORTED_GENERIC_ROOT';7='CONSTRUCTOR_LIMIT';9='METHOD_LIMIT';11='INTERFACE_LIMIT';13='MALFORMED_SIGNATURE';14='MALFORMED_SIGNATURE';15='UNSUPPORTED_SIGNATURE';16='SIGNATURE_LIMIT';17='SIGNATURE_SHAPE';18='SIGNATURE_SHAPE';19='STRING_LIMIT';20='SIGNATURE_LIMIT';22='ROW_LIMIT';24='ROW_LIMIT';28='REPORT_LIMIT';31='SIGNATURE_LIMIT';32='SIGNATURE_LIMIT';37='SIGNATURE_LIMIT'}[$profile]
  Reject {ProjectCollection $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(5,8,10,12,23,25,27,29,36,38)){
  $valid=Join-Path $root ('CollectionBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateProjectCollection($valid,$profile);$v=ProjectCollection $valid|ConvertFrom-Json;$vt=$v.types[0]
  Assert ($v.types.Count -eq 1 -and $v.runtime_execution -ceq 'NOT_RUN' -and $v.declaration_scope -ceq 'ROOT_ONLY' -and $v.referenced_declarations -ceq 'UNRESOLVED_NOT_FOLLOWED') ('Collection exact boundary/root-only profile '+$profile);$passed++
  if($profile -eq 5){Assert ($vt.is_interface -and $vt.attributes -eq 161 -and $null -eq $vt.base_type -and $vt.constructor_count -eq 0 -and $vt.public_constructor_count -eq 0) 'Collection interface kind recorded without class assumption';$passed++}
  if($profile -in @(8,38)){Assert ($vt.constructor_count -eq 32 -and $vt.public_constructor_count -eq 1) 'Collection32 constructor declarations';$passed++}
  if($profile -in @(10,38)){Assert ($vt.methods.Count -eq 256) 'Collection256 public methods independent of constructor budget';$passed++}
  if($profile -eq 12){Assert ($vt.interfaces.Count -eq 32) 'Collection32 interfaces';$passed++}
  if($profile -eq 27){Assert ($vt.base_type -ceq '[Owned.ForeignContracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]Owned.External.BaseType') 'Collection external base unresolved';$passed++}
  if($profile -eq 29){Assert ($vt.methods[1].name.Length -eq 1024) 'Collection1024-character name boundary';$passed++}
  if($profile -eq 36){Assert ($vt.methods[0].parameter_types.Count -eq 31) 'Collection valid128-byte signature boundary';$passed++}
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectCollection($collection,('f'*63))} 'DCAENTRY_HASH_FORMAT'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectCollection($collection,('f'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectCollection($streamStore,(Hash $streamStore))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedProjectCollection($oversize,('0'*64))} 'DCAENTRY_INPUT_FILE'
 Reject {[Task144.EntrypointMetadata]::ReadProjectCollection($collection,(Hash $collection))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadProjectCollection($broken,'73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a')} 'DCAENTRY_INPUT_HASH'
 $collectionWorker=Join-Path $PSScriptRoot 'Read-ProjectCollectionMetadata.ps1'
 Reject {& $collectionWorker -InputPath $collection -InputSha256 (Hash $collection)} 'DCAENTRY_INPUT_PIN'
 Assert ((ProjectCollection) -ceq $collectionRaw -and (StreamStore) -ceq $ssRaw -and (Catalog) -ceq $catalogRaw -and (UndoStore) -ceq $usRaw -and (TraceParameter) -ceq $tpRaw -and (TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reports byte-identical after collection root inquiry';$passed++
 $staticSdk=Join-Path $root 'OwnedSdkStaticFactories.dll';[Task144OwnedFixtures]::CreateSdkStaticFactories($staticSdk)
 function StaticFactories([string] $p=$staticSdk){[Task144.EntrypointMetadata]::ReadOwnedSdkStaticFactories($p,(Hash $p))}
 $sfRaw=StaticFactories;$sf=$sfRaw|ConvertFrom-Json;$sfc=$sf.counts
 Assert ($sf.operation -ceq 'static_dca_sdk_static_factories' -and $sf.runtime_execution -ceq 'NOT_RUN' -and $sf.inputs[0].identity -ceq 'Task009.OwnedSdkStaticFactories, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' -and ($sf.selected_targets -join '|') -ceq 'Owned.StaticFactories.Project|Owned.StaticFactories.Root|Owned.StaticFactories.ProjectCollection') 'Static-factory fixed unsigned input/three target names';$passed++
 Assert (($sf.targets.token -join '|') -ceq '02000002|02000003|02000004' -and @($sf.targets|Where-Object {$_.attributes -ne 257 -or $_.visibility -cne 'Public' -or $_.generic_arity -ne 0}).Count -eq 0) 'Actual target token/flags/visibility/arity';$passed++
 Assert ($sf.methods.Count -eq 4 -and ($sf.methods[0..2].target_name -join '|') -ceq ($sf.selected_targets -join '|') -and $sf.methods[0].signature -ceq 'static Owned.StaticFactories.Project(String) generic=0' -and $sf.methods[1].generic_arity -eq 1 -and $sf.methods[1].owner_generic_arity -eq 1 -and $sf.methods[3].owner_public_chain.Count -eq 2) 'All three direct local targets, owner/method arity and nested-public chain';$passed++
 Assert (@($sf.methods|Where-Object {-not $_.is_static -or $_.is_instance -or $_.explicit_this}).Count -eq 0 -and $sfc.declared_methods_scanned -eq 18 -and $sfc.excluded_owner_not_public -eq 3 -and $sfc.excluded_constructors -eq 1 -and $sfc.excluded_nonpublic_methods -eq 1 -and $sfc.excluded_instance_methods -eq 1 -and $sfc.public_static_prefixes -eq 12 -and $sfc.direct_target_matches -eq 4) 'Owner/public/static/constructor exclusions and no-receiver facts partition methods';$passed++
 $omitted=0;foreach($property in $sfc.nonselected_return_prefixes.psobject.Properties){$omitted+=$property.Value}
 Assert ($omitted -eq 8 -and $sfc.public_static_prefixes -eq $sfc.direct_target_matches+$omitted -and $sfc.nonselected_return_prefixes.OTHER_CLASS_TYPEDEF -eq 1 -and $sfc.nonselected_return_prefixes.CLASS_TYPEREF_UNRESOLVED -eq 1 -and $sfc.nonselected_return_prefixes.CLASS_TYPESPEC_UNEXAMINED -eq 1 -and $sfc.nonselected_return_prefixes.GENERIC_INSTANCE_PREFIX_UNEXAMINED -eq 1 -and $sfc.maximum_prefix_bytes_read -le 16) 'Direct foreign/TypeSpec/wrapped returns excluded with consistent prefix categories';$passed++
 Assert ($sfRaw.Length -lt 20000 -and -not $sfRaw.Contains('HiddenMalformed') -and -not $sfRaw.Contains('NeverRead') -and -not $sfRaw.Contains($root) -and @([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object {$_.GetName().Name -eq 'Task009.OwnedSdkStaticFactories'}).Count -eq 0) 'Two-MiB excluded malformed tail/long excluded name never decoded or reported; no target load/private path';$passed++
 foreach($profile in @(1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,19,20,21,22,23,24,25,26,27,29,30,32,34,36,37,38,41,42,43)){
  $bad=Join-Path $root ('StaticFactoryBad'+$profile+'.dll');[Task144OwnedFixtures]::CreateSdkStaticFactories($bad,$profile)
  $code=@{1='INPUT_IDENTITY';2='TARGET_NOT_FOUND';3='DUPLICATE_TARGET';4='TARGET_TOKEN';5='MALFORMED_METHODDEF_HEADER';6='MALFORMED_METHODDEF_HEADER';7='UNSUPPORTED_METHODDEF_HEADER';8='FACTORY_PREFIX_GENERIC';9='MALFORMED_FACTORY_PREFIX';10='MALFORMED_FACTORY_PREFIX';11='FACTORY_PREFIX_TOKEN';12='FACTORY_PREFIX_TOKEN';13='FACTORY_PREFIX_TOKEN';14='STATIC_SIGNATURE_FLAGS';15='STATIC_SIGNATURE_FLAGS';16='MALFORMED_METHODDEF_HEADER';17='SIGNATURE_SHAPE';19='SIGNATURE_LIMIT';20='MALFORMED_SIGNATURE';21='SIGNATURE_LIMIT';22='UNSUPPORTED_SIGNATURE';23='FACTORY_PREFIX_RETURN';24='FACTORY_PREFIX_INTEGER';25='FACTORY_PREFIX_INTEGER';26='UNSUPPORTED_METHODDEF_HEADER';27='STATIC_FACTORY_LIMIT';29='REPORT_LIMIT';30='OWNER_CHAIN_LIMIT';32='OWNER_CHAIN_CYCLE';34='ROW_LIMIT';36='STRING_LIMIT';37='OWNER_CHAIN_SHAPE';38='ROW_LIMIT';41='FACTORY_PREFIX_TOKEN';42='FACTORY_PREFIX_RETURN';43='FACTORY_PREFIX_INTEGER'}[$profile]
  Reject {StaticFactories $bad} ('DCAENTRY_'+$code)
 }
 foreach($profile in @(18,28,31,35,39,44)){
  $valid=Join-Path $root ('StaticFactoryBoundary'+$profile+'.dll');[Task144OwnedFixtures]::CreateSdkStaticFactories($valid,$profile);$sv=StaticFactories $valid|ConvertFrom-Json
  Assert ($sv.runtime_execution -ceq 'NOT_RUN' -and ($sv.selected_targets -join '|') -ceq ($sf.selected_targets -join '|')) ('Static-factory bounded prefix/selection/owner/table profile '+$profile);$passed++
  if($profile -eq 18){Assert ($sv.methods.Count -eq 3 -and $sv.counts.nonselected_return_prefixes.VOID_PREFIX -eq 2) 'Excluded nongeneric VarArgs prefix accepted without whole tail; selected VarArgs rejected above';$passed++}
  if($profile -eq 28){Assert ($sv.methods.Count -eq 64) 'Exact64 selected candidates';$passed++}
  if($profile -eq 31){Assert ($sv.methods[-1].owner_public_chain.Count -eq 16) 'Exact16 public owner-chain nodes';$passed++}
  if($profile -eq 35){Assert ($sv.counts.typedef_rows -eq 32768) 'Exact32768 TypeDef rows';$passed++}
  if($profile -eq 39){Assert ($sv.counts.methoddef_rows -eq 32768) 'Exact32768 MethodDef rows';$passed++}
  if($profile -eq 44){Assert ($sv.counts.maximum_prefix_bytes_read -eq 15 -and $sv.methods.Count -eq 3 -and $sv.counts.nonselected_return_prefixes.GENERIC_INSTANCE_PREFIX_UNEXAMINED -eq 2) 'Large generic/parameter counts in excluded head stay prefix-only within16 bytes';$passed++}
 }
 Reject {[Task144.EntrypointMetadata]::ReadOwnedSdkStaticFactories($staticSdk,('f'*63))} 'DCAENTRY_HASH_FORMAT'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedSdkStaticFactories($staticSdk,('f'*64))} 'DCAENTRY_INPUT_HASH'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedSdkStaticFactories($collection,(Hash $collection))} 'DCAENTRY_INPUT_IDENTITY'
 Reject {[Task144.EntrypointMetadata]::ReadOwnedSdkStaticFactories($oversize,('0'*64))} 'DCAENTRY_INPUT_FILE'
 Reject {[Task144.EntrypointMetadata]::ReadSdkStaticFactories($staticSdk,(Hash $staticSdk))} 'DCAENTRY_INPUT_PIN'
 Reject {[Task144.EntrypointMetadata]::ReadSdkStaticFactories($broken,'7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989')} 'DCAENTRY_INPUT_HASH'
 $sfWorker=Join-Path $PSScriptRoot 'Read-SdkStaticFactoriesMetadata.ps1'
 Reject {& $sfWorker -InputPath $staticSdk -InputSha256 (Hash $staticSdk)} 'DCAENTRY_INPUT_PIN'
 Assert ((StaticFactories) -ceq $sfRaw -and (ProjectCollection) -ceq $collectionRaw -and (StreamStore) -ceq $ssRaw -and (Catalog) -ceq $catalogRaw -and (UndoStore) -ceq $usRaw -and (TraceParameter) -ceq $tpRaw -and (TraceConstruction) -ceq $tcRaw -and (Trace) -ceq $traceRaw -and (UndoManager) -ceq $undoRaw -and (ProjectId) -ceq $idRaw -and (Constructors) -ceq $constructorRaw -and (Providers) -ceq $providerRaw -and (Services) -ceq $serviceRaw -and (Construction) -ceq $constructionRaw -and (Project $sdk) -ceq $projectRaw -and (Read $dca) -ceq $raw) 'All prior reports byte-identical after static direct-return selection';$passed++
 Write-Output ('PASS '+$passed+' / FAIL 0')
}finally{Remove-Item -LiteralPath $root -Recurse -Force}
