#requires -Version 7.6
<# Metadata only. Real inputs MUST use run_probe.py's fixed entrypoint-map, project-context, project-construction or project-services mode.
No assembly loading, dependency resolution, attributes, method bodies or vendor code. #>
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string] $DcaPath,
 [Parameter(Mandatory)][string] $DcaSha256,
 [Parameter(Mandatory)][string[]] $ReferencePath,
 [Parameter(Mandatory)][string[]] $ReferenceSha256,
 [switch] $ProjectContext,
 [switch] $ProjectConstruction,
 [switch] $ProjectServices
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
try {
 if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAENTRY_RUNTIME' }
 if (($ProjectContext -and ($ProjectConstruction -or $ProjectServices)) -or ($ProjectConstruction -and $ProjectServices)) { throw 'DCAENTRY_MODE' }
 if (-not ('Task144.EntrypointMetadata' -as [type])) {
  Add-Type -ReferencedAssemblies @('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography') -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'EntrypointMetadata.cs')))
 }
 if($ProjectServices){[Task144.EntrypointMetadata]::ReadServices($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 elseif($ProjectConstruction){[Task144.EntrypointMetadata]::ReadConstruction($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 elseif($ProjectContext){[Task144.EntrypointMetadata]::ReadProject($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 else{[Task144.EntrypointMetadata]::Read($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
} catch {
 $code='DCAENTRY_IO_OR_METADATA'
 $rowObserved=$false
 $rowType='Task144.EntrypointMetadata+RowLimitObservationException' -as [type]
 if(($ProjectConstruction -or $ProjectServices) -and $null -ne $rowType){
  $exception=$_.Exception
  for($depth=0;$depth -lt 8 -and $null -ne $exception;$depth++){
   if($exception.GetType() -eq $rowType){
    $code='DCAENTRY_ROW_LIMIT_'+$exception.TableName+'_'+$exception.RowCount.ToString([Globalization.CultureInfo]::InvariantCulture)
    $rowObserved=$true;break
   }
   $exception=$exception.InnerException
  }
 }
 if (-not $rowObserved -and $_.Exception.ToString() -match 'DCAENTRY_[A-Z_]+') { $code=$Matches[0] }
 throw [InvalidOperationException]::new($code+': Static entrypoint observation failed; no runtime conclusion.')
}
