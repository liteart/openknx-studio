#requires -Version 7.6
<# Metadata only. Real inputs MUST use a fixed run_probe.py metadata mode, including project-providers.
No assembly loading, dependency resolution, attributes, method bodies or vendor code. #>
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string] $DcaPath,
 [Parameter(Mandatory)][string] $DcaSha256,
 [Parameter(Mandatory)][string[]] $ReferencePath,
 [Parameter(Mandatory)][string[]] $ReferenceSha256,
 [switch] $ProjectContext,
 [switch] $ProjectConstruction,
 [switch] $ProjectServices,
 [switch] $ProjectProviders
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
try {
 if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAENTRY_RUNTIME' }
 $modeCount=0;foreach($mode in @($ProjectContext,$ProjectConstruction,$ProjectServices,$ProjectProviders)){if($mode){$modeCount++}}
 if ($modeCount -gt 1) { throw 'DCAENTRY_MODE' }
 if (-not ('Task144.EntrypointMetadata' -as [type])) {
  Add-Type -ReferencedAssemblies @('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography') -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'EntrypointMetadata.cs')))
 }
 if($ProjectProviders){[Task144.EntrypointMetadata]::ReadProviders($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 elseif($ProjectServices){[Task144.EntrypointMetadata]::ReadServices($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 elseif($ProjectConstruction){[Task144.EntrypointMetadata]::ReadConstruction($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 elseif($ProjectContext){[Task144.EntrypointMetadata]::ReadProject($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
 else{[Task144.EntrypointMetadata]::Read($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)}
} catch {
 $code='DCAENTRY_IO_OR_METADATA'
 $rowObserved=$false
 $rowType='Task144.EntrypointMetadata+RowLimitObservationException' -as [type]
 if(($ProjectConstruction -or $ProjectServices -or $ProjectProviders) -and $null -ne $rowType){
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
