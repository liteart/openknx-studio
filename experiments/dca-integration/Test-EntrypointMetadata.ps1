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
 Write-Output ('PASS '+$passed+' / FAIL 0')
}finally{Remove-Item -LiteralPath $root -Recurse -Force}
