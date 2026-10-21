#requires -Version 7.6
<# Metadata only. Real inputs MUST use run_probe.py's fixed entrypoint-map mode.
No assembly loading, dependency resolution, attributes, method bodies or vendor code. #>
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string] $DcaPath,
 [Parameter(Mandatory)][string] $DcaSha256,
 [Parameter(Mandatory)][string[]] $ReferencePath,
 [Parameter(Mandatory)][string[]] $ReferenceSha256
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
try {
 if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAENTRY_RUNTIME' }
 if (-not ('Task144.EntrypointMetadata' -as [type])) {
  Add-Type -ReferencedAssemblies @('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography') -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'EntrypointMetadata.cs')))
 }
 [Task144.EntrypointMetadata]::Read($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)
} catch {
 $code='DCAENTRY_IO_OR_METADATA'
 if ($_.Exception.ToString() -match 'DCAENTRY_[A-Z_]+') { $code=$Matches[0] }
 throw [InvalidOperationException]::new($code+': Static entrypoint observation failed; no runtime conclusion.')
}
