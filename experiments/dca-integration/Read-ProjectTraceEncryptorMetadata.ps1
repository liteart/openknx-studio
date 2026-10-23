#requires -Version 7.6
# Fixed original ProjectTraceEncryptor metadata only; real inputs require guarded run_probe.py project-trace-encryptor.
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string] $InputPath,
 [Parameter(Mandatory)][string] $InputSha256
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
try {
 if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAENTRY_RUNTIME' }
 if (-not ('Task144.EntrypointMetadata' -as [type])) {
  Add-Type -ReferencedAssemblies @('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography') -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'EntrypointMetadata.cs')))
 }
 [Task144.EntrypointMetadata]::ReadProjectTraceEncryptor($InputPath,$InputSha256)
} catch {
 $code='DCAENTRY_IO_OR_METADATA'
 $rowObserved=$false
 $rowType='Task144.EntrypointMetadata+RowLimitObservationException' -as [type]
 if($null -ne $rowType){
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
 throw [InvalidOperationException]::new($code+': Static ProjectTraceEncryptor observation failed; no runtime conclusion.')
}
