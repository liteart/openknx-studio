#requires -Version 7.6
<# Owned synthetic fixtures only. Real inputs require the separate Task-140 sandbox wrapper. #>
[CmdletBinding()]
param([string] $TemporaryParent = '/private/tmp')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$tool = Join-Path $PSScriptRoot 'Read-ReferenceMetadata.ps1'
$root = Join-Path $TemporaryParent ('task140-owned-' + [Guid]::NewGuid().ToString('N'))
$passed = 0
$failed = 0
function Assert-True([bool] $Condition, [string] $Message) { if (-not $Condition) { throw $Message } }
function Test-Case([string] $Name, [scriptblock] $Action) {
    try { & $Action; $script:passed++; Write-Output ('PASS: ' + $Name) }
    catch { $script:failed++; Write-Output ('FAIL: ' + $Name + ': ' + $_.Exception.Message) }
}
function Hash([string] $Path) { return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Rejected([scriptblock] $Action, [string] $Code) {
    $seen = $false
    try { $null = & $Action }
    catch {
        $seen = $true
        Assert-True ($_.Exception.Message.StartsWith('DCAMETA_' + $Code + ':')) ('Unexpected failure: ' + $_.Exception.Message)
        Assert-True (-not $_.Exception.Message.Contains($root)) 'Private path leaked in structured failure.'
    }
    Assert-True $seen ('Expected failure: ' + $Code)
}
function Directory([string] $Name) { $path = Join-Path $root $Name; $null = New-Item -ItemType Directory -Path $path; return $path }
function Compile([string] $Path, [string] $Source, [string[]] $References = @()) {
    if ($References.Count -eq 0) { Add-Type -TypeDefinition $Source -OutputAssembly $Path -OutputType Library }
    else { Add-Type -TypeDefinition $Source -ReferencedAssemblies $References -OutputAssembly $Path -OutputType Library }
}
try {
    $null = New-Item -ItemType Directory -Path $root
    # Add-Type generates random assembly names even with a fixed OutputAssembly.
    # Normalize ONLY these owned test images' existing name heap slots so that
    # separate versions can represent the same assembly identity name. This is
    # never called on vendor inputs and changes no tool/runtime binding policy.
    Add-Type -ReferencedAssemblies @('System.Reflection.Metadata', 'System.Collections.Immutable') -TypeDefinition @'
using System;
using System.IO;
using System.Text;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
public static class Task140OwnedFixtureName {
 public static void Normalize(string path) {
  byte[] bytes = File.ReadAllBytes(path);
  using (var pe = new PEReader(new MemoryStream(bytes, false))) {
   var r = pe.GetMetadataReader(MetadataReaderOptions.None);
   var name = r.GetAssemblyDefinition().Name;
   byte[] replacement = Encoding.UTF8.GetBytes("Owned");
   int oldLength = Encoding.UTF8.GetByteCount(r.GetString(name));
   if (oldLength < replacement.Length) throw new InvalidDataException("Owned fixture name slot is too short");
   int offset = pe.PEHeaders.MetadataStartOffset + r.GetHeapMetadataOffset(HeapIndex.String) + MetadataTokens.GetHeapOffset(name);
   Array.Clear(bytes, offset, oldLength);
   Array.Copy(replacement, 0, bytes, offset, replacement.Length);
  }
  File.WriteAllBytes(path, bytes);
 }
}
'@
    $reference = Join-Path (Directory 'reference') 'Owned.Reference.dll'
    Compile $reference @'
using System;
using System.Reflection;
[assembly: AssemblyVersion("1.2.3.4")]
namespace Task140Fixture {
 public interface IContract { int Ping(string value); }
 public class Concrete { public static int Value = 5; }
 public class Outer { public class Nested { public static int Run() { throw new Exception("Must not execute"); } } }
 public class Generic<T> { public void Touch() { throw new Exception("Must not execute"); } }
}
'@
    [Task140OwnedFixtureName]::Normalize($reference)
    $consumer = Join-Path (Directory 'consumer') 'Owned.Consumer.dll'
    Compile $consumer @'
using System;
using System.Reflection;
using Task140Fixture;
[assembly: AssemblyVersion("2.3.4.5")]
public class Consumer {
 static Consumer() { throw new Exception("Target execution forbidden"); }
 public int Invoke(IContract contract) { new Generic<int>().Touch(); return contract.Ping("owned") + Concrete.Value + Outer.Nested.Run(); }
}
'@ @($reference)
    $refHash = Hash $reference
    $dcaHash = Hash $consumer
    Test-Case 'exact identity, interface/class/nested types and SDK-directed members' {
        $raw = & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($reference) -ReferenceSha256 @($refHash)
        $report = $raw | ConvertFrom-Json
        Assert-True (-not $raw.Contains($root)) 'Private path leaked in report.'
        Assert-True ($report.runtime_execution -eq 'NOT_RUN' -and $report.rights -eq 'UNRESOLVED') 'Runtime or rights overclaim.'
        Assert-True ($report.inputs[0].sha256 -eq $dcaHash -and $report.inputs[1].sha256 -eq $refHash) 'Incorrect file pins.'
        Assert-True ($report.inputs[0].identity.version -eq '2.3.4.5' -and $report.inputs[1].identity.version -eq '1.2.3.4') 'Incorrect definition version.'
        $types = @($report.dca_types_to_candidates)
        Assert-True (@($types | Where-Object { $_.type_name -eq 'Task140Fixture.IContract' -and $_.definition_kind -eq 'interface' }).Count -eq 1) 'Interface TypeDef not observed.'
        Assert-True (@($types | Where-Object { $_.type_name -eq 'Task140Fixture.Concrete' -and $_.definition_kind -eq 'class' }).Count -eq 1) 'Class TypeDef not observed.'
        Assert-True (@($types | Where-Object { $_.type_name -eq 'Task140Fixture.Outer+Nested' }).Count -eq 1) 'Nested reference scope not resolved.'
        Assert-True (@($types | Where-Object { -not $_.exact_identity_match }).Count -eq 0) 'Exact fixture identities did not match.'
        Assert-True (@($report.dca_members_to_candidates | Where-Object { $_.member_name -eq 'Ping' -and $_.signature_bytes -gt 0 -and $_.signature_sha256.Length -eq 64 }).Count -eq 1) 'SDK-directed MemberRef signature not recorded.'
        Assert-True (-not $raw.Contains('signature_hex')) 'Private signature hex unexpectedly included.'
        Assert-True ($report.unsupported_member_parent_counts.TypeSpecification -gt 0) 'Unsupported generic MemberRef scope omitted.'
        Assert-True (@($report.inputs[1].direct_assembly_references).Count -gt 0) 'Candidate direct dependencies omitted.'
    }
    Test-Case 'deterministic output and explicitly enabled private signature hex' {
        $one = & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($reference) -ReferenceSha256 @($refHash) -IncludeSignatureHex
        $two = & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($reference) -ReferenceSha256 @($refHash) -IncludeSignatureHex
        Assert-True ($one -eq $two) 'Static report is not deterministic.'
        $report = $one | ConvertFrom-Json
        foreach ($member in $report.dca_members_to_candidates) {
            Assert-True ($member.signature_hex.Length -eq 2 * $member.signature_bytes -and $member.signature_bytes -le 512) 'Incorrect bounded signature hex.'
        }
    }
    $wrongVersion = Join-Path (Directory 'version') 'Owned.Reference.dll'
    Compile $wrongVersion @'
using System.Reflection;
[assembly: AssemblyVersion("9.8.7.6")]
namespace Task140Fixture { public interface IContract { int Ping(string value); } }
'@
    [Task140OwnedFixtureName]::Normalize($wrongVersion)
    Test-Case 'same-name version difference and missing TypeDef remain distinct' {
        $report = & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($wrongVersion) -ReferenceSha256 @((Hash $wrongVersion)) | ConvertFrom-Json
        $mismatch = @($report.dependency_matrix | Where-Object { $_.source_filename -eq 'Owned.Consumer.dll' -and $_.candidate_filename -eq 'Owned.Reference.dll' })
        Assert-True ($mismatch.Count -eq 1 -and -not $mismatch[0].exact_identity_match -and $mismatch[0].identity_differences -contains 'version') 'Exact version mismatch omitted.'
        Assert-True (@($report.dca_types_to_candidates | Where-Object { $_.type_name -eq 'Task140Fixture.IContract' -and $_.definition_observation -eq 'same_name_typedef_present' -and -not $_.exact_identity_match }).Count -eq 1) 'Name presence wrongly merged with identity match.'
        Assert-True (@($report.dca_types_to_candidates | Where-Object { $_.type_name -eq 'Task140Fixture.Concrete' -and $_.definition_observation -eq 'not_found_in_typedefs_forwarding_unresolved' }).Count -eq 1) 'Missing TypeDef or forwarding uncertainty omitted.'
    }
    Test-Case 'expected hash mismatch precedes any metadata read' {
        $invalid = Join-Path $root 'invalid.dll'
        [IO.File]::WriteAllText($invalid, 'not a PE image')
        Rejected { & $tool -DcaPath $invalid -DcaSha256 (Hash $invalid) -ReferencePath @($reference) -ReferenceSha256 @(('0' * 64)) } 'HASH_MISMATCH'
        Rejected { & $tool -DcaPath $consumer -DcaSha256 'invalid-hash' -ReferencePath @($reference) -ReferenceSha256 @($refHash) } 'EXPECTED_HASH_FORMAT'
    }
    Test-Case 'truncated and nonmanaged data fail with structured errors' {
        $truncated = Join-Path $root 'truncated.dll'
        [IO.File]::WriteAllBytes($truncated, [IO.File]::ReadAllBytes($consumer)[0..63])
        Rejected { & $tool -DcaPath $truncated -DcaSha256 (Hash $truncated) -ReferencePath @($reference) -ReferenceSha256 @($refHash) } 'MALFORMED_METADATA'
    }
    Test-Case 'input count, duplicate paths and per-file limit' {
        Rejected { & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($reference, $reference, $reference) -ReferenceSha256 @($refHash, $refHash, $refHash) } 'INPUT_COUNT'
        Rejected { & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($consumer) -ReferenceSha256 @($dcaHash) } 'DUPLICATE_PATH'
        $large = Join-Path $root 'large.dll'
        $stream = [IO.File]::Create($large)
        try { $stream.SetLength(16MB + 1) } finally { $stream.Dispose() }
        Rejected { & $tool -DcaPath $large -DcaSha256 ('0' * 64) -ReferencePath @($reference) -ReferenceSha256 @($refHash) } 'FILE_LIMIT'
    }
    Test-Case 'aggregate file-size limit precedes metadata' {
        $paths = @()
        $hashes = @()
        foreach ($name in @('total1.dll', 'total2.dll', 'total3.dll')) {
            $path = Join-Path $root $name
            $stream = [IO.File]::Create($path)
            try { $stream.SetLength(12MB) } finally { $stream.Dispose() }
            $paths += $path
            $hashes += Hash $path
        }
        Rejected { & $tool -DcaPath $paths[0] -DcaSha256 $hashes[0] -ReferencePath @($paths[1], $paths[2]) -ReferenceSha256 @($hashes[1], $hashes[2]) } 'TOTAL_LIMIT'
    }
    Test-Case 'metadata table row limit' {
        $many = Join-Path $root 'many-rows.dll'
        $text = [Text.StringBuilder]::new('public class ManyFields {')
        for ($i = 0; $i -lt 32769; $i++) { $null = $text.Append('public int F').Append($i).Append(';') }
        $null = $text.Append('}')
        Compile $many $text.ToString()
        Rejected { & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($many) -ReferenceSha256 @((Hash $many)) } 'ROW_LIMIT'
    }
    Test-Case 'metadata name length limit' {
        $longName = Join-Path $root 'long-name.dll'
        Compile $longName ('public class ' + ('A' * 600) + ' { public class ' + ('B' * 600) + ' {} }')
        Rejected { & $tool -DcaPath $consumer -DcaSha256 $dcaHash -ReferencePath @($longName) -ReferenceSha256 @((Hash $longName)) } 'STRING_LIMIT'
    }
}
finally { if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force } }
Write-Output ("Results: PASS=$passed FAIL=$failed")
if ($failed -gt 0) { exit 1 }
exit 0
