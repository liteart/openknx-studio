#requires -Version 7.6
# Fixed original ProjectId metadata only; real inputs require guarded run_probe.py project-id.
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
 [Task144.EntrypointMetadata]::ReadProjectId($InputPath,$InputSha256)
} catch {
 $code='DCAENTRY_IO_OR_METADATA'
 if ($_.Exception.ToString() -match 'DCAENTRY_[A-Z_]+') { $code=$Matches[0] }
 throw [InvalidOperationException]::new($code+': Static ProjectId observation failed; no runtime conclusion.')
}
