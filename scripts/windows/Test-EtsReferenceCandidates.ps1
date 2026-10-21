#requires -Version 5.1
<# Self-contained tests. Generates and inspects only project-owned C# fixtures. #>
[CmdletBinding()]
param([string] $TemporaryParent = [System.IO.Path]::GetTempPath())

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$tool = Join-Path $PSScriptRoot 'Get-EtsReferenceCandidates.ps1'
$testRoot = Join-Path $TemporaryParent ('task139-' + [Guid]::NewGuid().ToString('N'))
$passed = 0
$failed = 0
$notRun = 0

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-Rejected([scriptblock] $Action, [string] $Code) {
    $rejected = $false
    try { $null = & $Action }
    catch {
        $rejected = $true
        Assert-True ($_.Exception.Message -match ('^ETSREF_' + $Code + ':')) ('Unexpected rejection: ' + $_.Exception.Message)
        Assert-True (-not $_.Exception.Message.Contains($testRoot)) 'Private filesystem path leaked in error message.'
    }
    Assert-True $rejected ('Expected rejection: ' + $Code)
}

function Test-Case([string] $Name, [scriptblock] $Action) {
    try { & $Action; $script:passed++; Write-Output ('PASS: ' + $Name) }
    catch { $script:failed++; Write-Output ('FAIL: ' + $Name + ': ' + $_.Exception.Message) }
}

function Skip-Case([string] $Name, [string] $Reason) {
    $script:notRun++
    Write-Output ('NOT RUN: ' + $Name + ': ' + $Reason)
}

function New-TestDirectory([string] $Name) {
    $path = Join-Path $testRoot $Name
    $null = Microsoft.PowerShell.Management\New-Item -ItemType Directory -Path $path
    return $path
}

try {
    $null = Microsoft.PowerShell.Management\New-Item -ItemType Directory -Path $testRoot
    $source = New-TestDirectory 'source'
    $fixture = Join-Path $source 'Owned.Reference.dll'
    # No proprietary inputs. No target load or constructor invocation by the tool.
    Add-Type -TypeDefinition @'
using System;
using System.Reflection;
[assembly: AssemblyVersion("1.2.3.4")]
public sealed class Task139OwnedReferenceFixture {
    static Task139OwnedReferenceFixture() { throw new InvalidOperationException("Fixture must not execute."); }
    public string Value { get { return "owned fixture"; } }
}
'@ -OutputAssembly $fixture -OutputType Library
    $fixtureHash = (Get-FileHash -LiteralPath $fixture -Algorithm SHA256).Hash.ToLowerInvariant()
    $fixtureSize = (Get-Item -LiteralPath $fixture).Length
    $inputNames = @('Owned.Reference.dll')

    Test-Case 'default inventory: identity/hash, no disk writes, path redaction' {
        $before = @(Get-ChildItem -LiteralPath $testRoot -Recurse -Force | ForEach-Object { $_.FullName }) -join "`n"
        $raw = & $tool -EtsDirectory $source -CandidateFileName $inputNames
        $result = $raw | ConvertFrom-Json
        $after = @(Get-ChildItem -LiteralPath $testRoot -Recurse -Force | ForEach-Object { $_.FullName }) -join "`n"
        Assert-True ($before -eq $after) 'Inventory wrote files.'
        Assert-True (-not $raw.Contains($testRoot)) 'Manifest contains private paths.'
        Assert-True ($result.operation -eq 'inventory' -and $result.status -eq 'complete') 'Incorrect inventory status.'
        Assert-True ($result.rights -eq 'UNRESOLVED' -and $result.compatibility -eq 'UNVERIFIED') 'Candidate claims exceed evidence.'
        Assert-True ($result.target_assembly_execution -eq 'NOT_RUN') 'Incorrect execution claim.'
        Assert-True ($result.count -eq 1 -and $result.total_bytes -eq $fixtureSize) 'Incorrect inventory bounds.'
        Assert-True ($result.candidates[0].sha256 -eq $fixtureHash) 'Incorrect hash.'
        Assert-True ($result.candidates[0].version -eq '1.2.3.4') 'Incorrect metadata version.'
        Assert-True ($result.candidates[0].culture -eq 'neutral' -and $result.candidates[0].public_key_token -eq 'null') 'Incorrect metadata culture/token.'
        Assert-True ($result.candidates[0].assembly_identity -like '*, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null') 'Incomplete assembly identity.'
    }
    Test-Case 'explicit copy: exact bytes and complete private manifest' {
        $destination = Join-Path $testRoot 'copied'
        $raw = & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory $destination
        $result = $raw | ConvertFrom-Json
        Assert-True ($result.operation -eq 'private_copy') 'Incorrect copy mode.'
        Assert-True ((Get-FileHash -LiteralPath (Join-Path $destination $inputNames[0])).Hash.ToLowerInvariant() -eq $fixtureHash) 'Copied bytes differ.'
        $manifest = Get-Content -LiteralPath (Join-Path $destination 'reference-inventory.json') -Raw | ConvertFrom-Json
        Assert-True ($manifest.status -eq 'complete' -and $manifest.candidates[0].sha256 -eq $fixtureHash) 'Invalid completion manifest.'
        Assert-True (@(Get-ChildItem -LiteralPath $destination -Force).Count -eq 2) 'Unexpected copied files.'
    }
    Test-Case 'Windows drive-type classifier fails closed without probing drives' {
        # Dot-source only the owned script with the owned, bounded fixture. The
        # resulting helper is exercised on enum values, never a drive mapping.
        & {
            $null = . $tool -EtsDirectory $source -CandidateFileName $inputNames
            foreach ($type in @([System.IO.DriveType]::Fixed, [System.IO.DriveType]::Removable, [System.IO.DriveType]::Ram)) {
                Assert-True (Test-LocalDriveType $type) ('Expected local drive classification: ' + $type)
            }
            foreach ($type in @([System.IO.DriveType]::Network, [System.IO.DriveType]::Unknown, [System.IO.DriveType]::NoRootDirectory, [System.IO.DriveType]::CDRom)) {
                Assert-True (-not (Test-LocalDriveType $type)) ('Unsafe drive classification accepted: ' + $type)
            }
        }
    }
    Test-Case 'bracketed destination components are treated literally' {
        $parent = New-TestDirectory 'private[lab]'
        foreach ($destination in @((Join-Path $testRoot 'output[lab]'), (Join-Path $parent 'output'))) {
            $null = & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory $destination
            Assert-True ([System.IO.File]::Exists((Join-Path $destination 'reference-inventory.json'))) 'Literal destination manifest is missing.'
            Assert-True ((Get-FileHash -LiteralPath (Join-Path $destination $inputNames[0])).Hash.ToLowerInvariant() -eq $fixtureHash) 'Literal destination bytes differ.'
        }
    }
    Test-Case 'copy requires both explicit switch and destination' {
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -DestinationDirectory (Join-Path $testRoot 'unused') } 'COPY_ARGUMENTS'
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy } 'COPY_ARGUMENTS'
    }
    Test-Case 'reject traversal, wildcards, nested and device basenames' {
        foreach ($name in @('../Owned.Reference.dll', '..\Owned.Reference.dll', 'a/b.dll', 'a\b.dll', '*.dll', '[a].dll', 'a?.dll', 'a.dll:stream', 'CON.dll', ' Owned.Reference.dll', '.hidden.dll', 'not-a-dll.exe')) {
            Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @($name) } 'DLL_BASENAME'
        }
    }
    Test-Case 'reject duplicate and case-duplicate selection' {
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @('Owned.Reference.dll', 'owned.reference.DLL') } 'DUPLICATE_NAME'
    }
    Test-Case 'reject Falcon and explicitly named bus drivers' {
        foreach ($name in @('Knx.Falcon.Sdk.dll', 'Knx.BusDriver.dll')) {
            Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @($name) } 'DRIVER_OUT_OF_SCOPE'
        }
    }
    Test-Case 'reject relative source and nonlocal paths' {
        Assert-Rejected { & $tool -EtsDirectory '.' -CandidateFileName $inputNames } 'ABSOLUTE_PATH_REQUIRED'
        Assert-Rejected { & $tool -EtsDirectory '\\server\share' -CandidateFileName $inputNames } 'LOCAL_PATH_REQUIRED'
    }
    Test-Case 'reject missing and nonmanaged files before creating output' {
        $invalid = Join-Path $source 'Invalid.dll'
        [System.IO.File]::WriteAllText($invalid, 'Owned nonmanaged fixture')
        $destination = Join-Path $testRoot 'invalid-output'
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @('Missing.dll') -Copy -DestinationDirectory $destination } 'CANDIDATE_MISSING'
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @('Invalid.dll') -Copy -DestinationDirectory $destination } 'INVALID_MANAGED_ASSEMBLY'
        Assert-True (-not (Test-Path -LiteralPath $destination)) 'Rejected preflight created a destination.'
    }
    Test-Case 'reject count and per-file byte limits' {
        $many = @(1..17 | ForEach-Object { 'Owned' + $_ + '.dll' })
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $many } 'FILE_COUNT'
        $largePath = Join-Path $source 'Large.dll'
        $large = [System.IO.File]::Create($largePath)
        try { $large.SetLength(64MB + 1) } finally { $large.Dispose() }
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @('Large.dll') } 'FILE_SIZE'
        Remove-Item -LiteralPath $largePath
    }
    Test-Case 'reject aggregate byte limit using owned padded managed fixtures' {
        $largeSource = New-TestDirectory 'aggregate'
        foreach ($name in @('One.dll', 'Two.dll', 'Three.dll')) {
            $path = Join-Path $largeSource $name
            [System.IO.File]::Copy($fixture, $path)
            $large = [System.IO.File]::OpenWrite($path)
            try { $large.SetLength(64MB) } finally { $large.Dispose() }
        }
        Assert-Rejected { & $tool -EtsDirectory $largeSource -CandidateFileName @('One.dll', 'Two.dll', 'Three.dll') } 'TOTAL_SIZE'
        Remove-Item -LiteralPath $largeSource -Recurse
    }
    Test-Case 'reject existing output without overwriting' {
        $existing = New-TestDirectory 'existing'
        $sentinel = Join-Path $existing 'sentinel.txt'
        [System.IO.File]::WriteAllText($sentinel, 'preserve')
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory $existing } 'DESTINATION_EXISTS'
        Assert-True ([System.IO.File]::ReadAllText($sentinel) -eq 'preserve') 'Existing data overwritten.'
    }
    Test-Case 'reject checkout, worktree and bare repository destinations' {
        foreach ($kind in @('checkout', 'worktree', 'bare')) {
            $gitRoot = New-TestDirectory $kind
            if ($kind -eq 'checkout') { $null = Microsoft.PowerShell.Management\New-Item -ItemType Directory -Path (Join-Path $gitRoot '.git') }
            elseif ($kind -eq 'worktree') { [System.IO.File]::WriteAllText((Join-Path $gitRoot '.git'), 'gitdir: owned-fixture') }
            else {
                [System.IO.File]::WriteAllText((Join-Path $gitRoot 'HEAD'), 'ref: refs/heads/main')
                $null = Microsoft.PowerShell.Management\New-Item -ItemType Directory -Path (Join-Path $gitRoot 'objects')
                $null = Microsoft.PowerShell.Management\New-Item -ItemType Directory -Path (Join-Path $gitRoot 'refs')
            }
            Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory (Join-Path $gitRoot 'output') } 'GIT_DESTINATION'
        }
    }
    Test-Case 'reject source/output overlap and missing destination parent' {
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory (Join-Path $source 'output') } 'PATH_OVERLAP'
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory (Join-Path (Join-Path $testRoot 'missing-parent') 'output') } 'PATH_MISSING'
    }
    Test-Case 'copy failure leaves no complete manifest and does not overwrite' {
        $destination = Join-Path $testRoot 'copy-conflict'
        # Local command injection is a test seam, never a tool parameter. Simulate
        # a file appearing after directory creation and before CreateNew copy.
        function New-Item {
            param([string] $ItemType, [string] $Path, [string] $ErrorAction)
            $item = Microsoft.PowerShell.Management\New-Item -ItemType $ItemType -Path $Path -ErrorAction $ErrorAction
            [System.IO.File]::WriteAllText((Join-Path $Path 'Owned.Reference.dll'), 'preserve concurrent file')
            return $item
        }
        Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory $destination } 'IO_OR_METADATA'
        Assert-True (-not (Test-Path -LiteralPath (Join-Path $destination 'reference-inventory.json'))) 'Failed copy published completion.'
        Assert-True ([System.IO.File]::ReadAllText((Join-Path $destination 'Owned.Reference.dll')) -eq 'preserve concurrent file') 'Concurrent file overwritten.'
    }
    Test-Case 'source mutation between preflight and copy cannot publish completion' {
        $mutationSource = New-TestDirectory 'mutation-source'
        [System.IO.File]::Copy($fixture, (Join-Path $mutationSource 'Owned.Reference.dll'))
        $destination = Join-Path $testRoot 'mutation-output'
        function New-Item {
            param([string] $ItemType, [string] $Path, [string] $ErrorAction)
            $item = Microsoft.PowerShell.Management\New-Item -ItemType $ItemType -Path $Path -ErrorAction $ErrorAction
            # Windows denies the write while the read handle is held; Unix may
            # allow it. Either refusal or detection must prevent completion.
            [System.IO.File]::WriteAllText((Join-Path $mutationSource 'Owned.Reference.dll'), 'changed')
            return $item
        }
        $rejected = $false
        try { $null = & $tool -EtsDirectory $mutationSource -CandidateFileName $inputNames -Copy -DestinationDirectory $destination }
        catch { $rejected = $_.Exception.Message -match '^ETSREF_(SOURCE_CHANGED|IO_OR_METADATA):' }
        Assert-True $rejected 'Mutation was not rejected.'
        Assert-True (-not (Test-Path -LiteralPath (Join-Path $destination 'reference-inventory.json'))) 'Mutation published completion.'
    }

    $link = Join-Path $testRoot 'source-link'
    $linkAvailable = $false
    try {
        $null = Microsoft.PowerShell.Management\New-Item -ItemType SymbolicLink -Path $link -Target $source
        $linkAvailable = $true
    }
    catch { Skip-Case 'symbolic link rejection' 'Creating an owned symbolic link is unavailable in this environment.' }
    if ($linkAvailable) {
        Test-Case 'reject symlink source ancestor and output ancestor' {
            Assert-Rejected { & $tool -EtsDirectory $link -CandidateFileName $inputNames } 'LINK_PATH'
            Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName $inputNames -Copy -DestinationDirectory (Join-Path $link 'output') } 'LINK_PATH'
        }
        Test-Case 'reject symlink candidate file' {
            $fileLink = Join-Path $source 'Linked.dll'
            $null = Microsoft.PowerShell.Management\New-Item -ItemType SymbolicLink -Path $fileLink -Target $fixture
            Assert-Rejected { & $tool -EtsDirectory $source -CandidateFileName @('Linked.dll') } 'LINK_PATH'
        }
    }
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        Test-Case 'Windows rejects drive-relative and root-relative paths' {
            foreach ($path in @('C:relative', 'C:', '\relative', '/relative')) {
                Assert-Rejected { & $tool -EtsDirectory $path -CandidateFileName $inputNames } 'ABSOLUTE_PATH_REQUIRED'
            }
        }
        Test-Case 'Windows junction source ancestor rejection' {
            $junction = Join-Path $testRoot 'junction'
            $null = Microsoft.PowerShell.Management\New-Item -ItemType Junction -Path $junction -Target $source
            Assert-Rejected { & $tool -EtsDirectory $junction -CandidateFileName $inputNames } 'LINK_PATH'
        }
    }
    else { Skip-Case 'Windows junction/NTFS, drive-type query and Windows PowerShell 5.1' 'Requires Windows; portable execution does not qualify Windows behavior.' }

    $caseSource = New-TestDirectory 'case-collision'
    [System.IO.File]::Copy($fixture, (Join-Path $caseSource 'Case.dll'))
    try {
        [System.IO.File]::Copy($fixture, (Join-Path $caseSource 'case.dll'))
        Test-Case 'reject case-colliding immediate files' {
            Assert-Rejected { & $tool -EtsDirectory $caseSource -CandidateFileName @('Case.dll') } 'CASE_COLLISION'
        }
    }
    catch { Skip-Case 'case-colliding immediate files' 'Filesystem does not permit distinct case-colliding names.' }
}
finally {
    # Fixture-only cleanup. Do not follow directory links into their targets.
    if (Test-Path -LiteralPath $testRoot) {
        foreach ($item in @(Get-ChildItem -LiteralPath $testRoot -Force -Recurse | Where-Object { ($_.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0 })) {
            if ($item.PSIsContainer) { [System.IO.Directory]::Delete($item.FullName) }
            else { [System.IO.File]::Delete($item.FullName) }
        }
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
Write-Output ("Results: PASS=$passed FAIL=$failed NOT_RUN=$notRun")
if ($failed -gt 0) { exit 1 }
exit 0
