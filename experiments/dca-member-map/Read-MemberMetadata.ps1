#requires -Version 7.6
<# Task-142 metadata only. Real inputs MUST use the guarded run_probe.py member-map profile.
No target assembly loading, method bodies, attribute instantiation or dependency resolution.
Original helper code is compiled in memory; target files are never compiler references. #>
[CmdletBinding()]
param(
 [Parameter(Mandatory)][string] $DcaPath,
 [Parameter(Mandatory)][string] $DcaSha256,
 [Parameter(Mandatory)][string[]] $ReferencePath,
 [Parameter(Mandatory)][string[]] $ReferenceSha256
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
try {
 if ($PSVersionTable.PSVersion.ToString() -ne '7.6.7') { throw 'DCAMEMBER_RUNTIME' }
 if (-not ('Task142.MemberMetadata' -as [type])) {
  Add-Type -ReferencedAssemblies @('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography') -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'MemberMetadata.cs')))
 }
 [Task142.MemberMetadata]::Read($DcaPath,$DcaSha256,$ReferencePath,$ReferenceSha256)
} catch {
 $code='DCAMEMBER_IO_OR_METADATA'
 if ($_.Exception.ToString() -match 'DCAMEMBER_[A-Z_]+') { $code=$Matches[0] }
 throw [InvalidOperationException]::new($code + ': Static member observation failed; no runtime conclusion.')
}
