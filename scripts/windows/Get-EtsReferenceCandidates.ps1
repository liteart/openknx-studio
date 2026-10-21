#requires -Version 5.1
<#
.SYNOPSIS
Inventories exact local managed DLL candidates; optionally copies them privately.
.DESCRIPTION
No discovery, target assembly execution, network access, installation or rights
decision. Output is JSON without the installation or destination path. See
docs/LOCAL-ETS-REFERENCES.md before use. Keep the input directory stable during use.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $EtsDirectory,
    [Parameter(Mandatory = $true)][string[]] $CandidateFileName,
    [switch] $Copy,
    [string] $DestinationDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$maximumFileCount = 16
$maximumFileBytes = 64MB
$maximumTotalBytes = 128MB
$maximumDirectoryEntries = 16384
$streams = New-Object 'System.Collections.Generic.List[System.IO.FileStream]'

function Stop-Inventory([string] $Code) {
    throw [System.InvalidOperationException]::new("ETSREF_${Code}: See the local reference guide; no complete copy is claimed.")
}

function Test-LocalDriveType([System.IO.DriveType] $DriveType) {
    # Pure allowlist, also testable without touching any real or mapped drive.
    return $DriveType -in @([System.IO.DriveType]::Fixed, [System.IO.DriveType]::Removable, [System.IO.DriveType]::Ram)
}

function Get-NormalPath([string] $Value) {
    # UNC, device paths and provider syntax are outside this local-only tool.
    if (-not [string]::IsNullOrEmpty($Value) -and
        ($Value.StartsWith('\\') -or $Value.StartsWith('//') -or $Value.Contains('::'))) {
        Stop-Inventory 'LOCAL_PATH_REQUIRED'
    }
    if ([string]::IsNullOrWhiteSpace($Value) -or -not [System.IO.Path]::IsPathRooted($Value)) {
        Stop-Inventory 'ABSOLUTE_PATH_REQUIRED'
    }
    # .NET Framework lacks IsPathFullyQualified. IsPathRooted alone accepts
    # C:relative and \root-relative on Windows; neither is an explicit location.
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT -and $Value -notmatch '^[A-Za-z]:[\\/]') {
        Stop-Inventory 'ABSOLUTE_PATH_REQUIRED'
    }
    $full = [System.IO.Path]::GetFullPath($Value)
    $root = [System.IO.Path]::GetPathRoot($full)
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        # A drive letter can map to a network share. Reject that before any tree
        # access (including ancestor/link checks); unknown classifications fail.
        try { $driveType = [System.IO.DriveInfo]::new($root).DriveType }
        catch { Stop-Inventory 'LOCAL_DRIVE_REQUIRED' }
        if (-not (Test-LocalDriveType $driveType)) { Stop-Inventory 'LOCAL_DRIVE_REQUIRED' }
    }
    if ($full.Length -gt $root.Length) {
        $full = $full.TrimEnd([char[]]@([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar))
    }
    return $full
}

function Assert-NoLinks([string] $Path, [bool] $AllowMissingLeaf = $false) {
    $current = $Path
    $leaf = $true
    while (-not [string]::IsNullOrEmpty($current)) {
        # Get-Item -LiteralPath also reports dangling links on supported providers.
        $item = Get-Item -LiteralPath $current -Force -ErrorAction SilentlyContinue
        if ($null -eq $item) {
            if (-not ($leaf -and $AllowMissingLeaf)) { Stop-Inventory 'PATH_MISSING' }
        }
        elseif (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
            Stop-Inventory 'LINK_PATH'
        }
        $parent = [System.IO.Directory]::GetParent($current)
        if ($null -eq $parent) { break }
        $current = $parent.FullName
        $leaf = $false
    }
}

function Test-UnderPath([string] $Path, [string] $Ancestor) {
    $prefix = $Ancestor
    if (-not $prefix.EndsWith([string][System.IO.Path]::DirectorySeparatorChar)) {
        $prefix += [System.IO.Path]::DirectorySeparatorChar
    }
    return $Path.Equals($Ancestor, [System.StringComparison]::OrdinalIgnoreCase) -or
        $Path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-OutsideGit([string] $Path) {
    $current = $Path
    while (-not [string]::IsNullOrEmpty($current)) {
        # Detect both normal checkout directories and linked-worktree .git files.
        if (Test-Path -LiteralPath ([System.IO.Path]::Combine($current, '.git'))) { Stop-Inventory 'GIT_DESTINATION' }
        # A bare repository is not a private output location either.
        if ((Test-Path -LiteralPath ([System.IO.Path]::Combine($current, 'HEAD'))) -and
            (Test-Path -LiteralPath ([System.IO.Path]::Combine($current, 'objects'))) -and
            (Test-Path -LiteralPath ([System.IO.Path]::Combine($current, 'refs')))) { Stop-Inventory 'GIT_DESTINATION' }
        $parent = [System.IO.Directory]::GetParent($current)
        if ($null -eq $parent) { break }
        $current = $parent.FullName
    }
}

function Get-StreamHash([System.IO.FileStream] $Stream) {
    $Stream.Position = 0
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return ([System.BitConverter]::ToString($sha.ComputeHash($Stream))).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose(); $Stream.Position = 0 }
}

function Open-Source([string] $Path) {
    Assert-NoLinks $Path
    # Windows sharing denies concurrent writes/deletion while metadata is read.
    return [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
}

function Assert-SourceUnchanged([string] $Path, [System.IO.FileStream] $Stream, [long] $Size, [string] $Hash) {
    if ($Stream.Length -ne $Size -or (Get-StreamHash $Stream) -ne $Hash) { Stop-Inventory 'SOURCE_CHANGED' }
    # Also check the pathname: a held handle alone cannot detect Unix renames.
    $check = Open-Source $Path
    try {
        if ($check.Length -ne $Size -or (Get-StreamHash $check) -ne $Hash) { Stop-Inventory 'SOURCE_CHANGED' }
    }
    finally { $check.Dispose() }
}

try {
    if ($CandidateFileName.Count -lt 1 -or $CandidateFileName.Count -gt $maximumFileCount) { Stop-Inventory 'FILE_COUNT' }
    if ($Copy.IsPresent -ne (-not [string]::IsNullOrWhiteSpace($DestinationDirectory))) { Stop-Inventory 'COPY_ARGUMENTS' }
    $source = Get-NormalPath $EtsDirectory
    Assert-NoLinks $source
    if (-not [System.IO.Directory]::Exists($source)) { Stop-Inventory 'SOURCE_DIRECTORY' }
    $names = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $CandidateFileName) {
        if ([string]::IsNullOrWhiteSpace($name) -or $name.Length -gt 200 -or $name -ne $name.Trim() -or
            $name -match '[\x00-\x1f\x7f/\\:*?<>|"\[\]]' -or $name.StartsWith('.') -or
            -not $name.EndsWith('.dll', [System.StringComparison]::OrdinalIgnoreCase) -or
            $name -match '^(?i:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)') { Stop-Inventory 'DLL_BASENAME' }
        if (-not $names.Add($name)) { Stop-Inventory 'DUPLICATE_NAME' }
        if ($name -match '(?i)falcon|(?:^|[._ -])busdrivers?(?:[._ -]|$)') { Stop-Inventory 'DRIVER_OUT_OF_SCOPE' }
    }
    # Bounded, immediate-directory ambiguity check only. Never select extra files.
    $matches = @{}
    $entryCount = 0
    foreach ($entry in [System.IO.Directory]::EnumerateFileSystemEntries($source)) {
        $entryCount++
        if ($entryCount -gt $maximumDirectoryEntries) { Stop-Inventory 'DIRECTORY_LIMIT' }
        $leafName = [System.IO.Path]::GetFileName($entry)
        if ($names.Contains($leafName)) {
            if ($matches.ContainsKey($leafName)) { Stop-Inventory 'CASE_COLLISION' }
            $matches[$leafName] = $entry
        }
    }

    $destination = $null
    if ($Copy) {
        $destination = Get-NormalPath $DestinationDirectory
        Assert-NoLinks $destination $true
        if (Test-Path -LiteralPath $destination) { Stop-Inventory 'DESTINATION_EXISTS' }
        if ((Test-UnderPath $destination $source) -or (Test-UnderPath $source $destination)) { Stop-Inventory 'PATH_OVERLAP' }
        Assert-OutsideGit $destination
    }

    $records = New-Object 'System.Collections.Generic.List[object]'
    $paths = New-Object 'System.Collections.Generic.List[string]'
    [long] $totalBytes = 0
    foreach ($name in $CandidateFileName) {
        if (-not $matches.ContainsKey($name)) { Stop-Inventory 'CANDIDATE_MISSING' }
        $path = Get-NormalPath $matches[$name]
        $stream = Open-Source $path
        $streams.Add($stream)
        if ($stream.Length -gt $maximumFileBytes) { Stop-Inventory 'FILE_SIZE' }
        $totalBytes += $stream.Length
        if ($totalBytes -gt $maximumTotalBytes) { Stop-Inventory 'TOTAL_SIZE' }
        $hash = Get-StreamHash $stream
        # Reads assembly identity metadata; does not add the target to the domain.
        try { $identity = [System.Reflection.AssemblyName]::GetAssemblyName($path) }
        catch { Stop-Inventory 'INVALID_MANAGED_ASSEMBLY' }
        if ($identity.Name -match '(?i)falcon|(?:^|[._ -])busdrivers?(?:[._ -]|$)') { Stop-Inventory 'DRIVER_OUT_OF_SCOPE' }
        $tokenBytes = $identity.GetPublicKeyToken()
        $token = 'null'
        if ($null -ne $tokenBytes -and $tokenBytes.Length -gt 0) {
            $token = ([System.BitConverter]::ToString($tokenBytes)).Replace('-', '').ToLowerInvariant()
        }
        $culture = $identity.CultureName
        if ([string]::IsNullOrEmpty($culture)) { $culture = 'neutral' }
        Assert-SourceUnchanged $path $stream $stream.Length $hash
        $paths.Add($path)
        $records.Add([pscustomobject][ordered]@{
            filename = [System.IO.Path]::GetFileName($path)
            bytes = $stream.Length
            sha256 = $hash
            assembly_identity = $identity.FullName
            assembly_name = $identity.Name
            version = $identity.Version.ToString()
            culture = $culture
            public_key_token = $token
            classification = 'unverified_reference_candidate'
        })
    }

    $mode = 'inventory'
    if ($Copy) {
        Assert-NoLinks $destination $true
        Assert-OutsideGit $destination
        if (Test-Path -LiteralPath $destination) { Stop-Inventory 'DESTINATION_EXISTS' }
        # No -Force: an existing destination is never an accepted output directory.
        $null = New-Item -ItemType Directory -Path $destination -ErrorAction Stop
        Assert-NoLinks $destination
        Assert-OutsideGit $destination
        for ($index = 0; $index -lt $records.Count; $index++) {
            $record = $records[$index]
            Assert-SourceUnchanged $paths[$index] $streams[$index] $record.bytes $record.sha256
            $outputPath = [System.IO.Path]::Combine($destination, $record.filename)
            $output = [System.IO.File]::Open($outputPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
            try {
                $streams[$index].Position = 0
                # Bounded copy: do not let a growing source defeat the size limits.
                $buffer = New-Object byte[] 65536
                [long] $remaining = $record.bytes
                while ($remaining -gt 0) {
                    $read = $streams[$index].Read($buffer, 0, [int][Math]::Min($buffer.Length, $remaining))
                    if ($read -le 0) { Stop-Inventory 'SOURCE_CHANGED' }
                    $output.Write($buffer, 0, $read)
                    $remaining -= $read
                }
                $output.Flush($true)
                if ($output.Length -ne $record.bytes -or (Get-StreamHash $output) -ne $record.sha256) { Stop-Inventory 'COPY_VERIFICATION' }
            }
            finally { $output.Dispose() }
        }
        $mode = 'private_copy'
    }
    for ($index = 0; $index -lt $records.Count; $index++) {
        Assert-SourceUnchanged $paths[$index] $streams[$index] $records[$index].bytes $records[$index].sha256
        if ($Copy) {
            Assert-NoLinks $destination
            Assert-OutsideGit $destination
            $copied = Open-Source ([System.IO.Path]::Combine($destination, $records[$index].filename))
            try {
                if ($copied.Length -ne $records[$index].bytes -or (Get-StreamHash $copied) -ne $records[$index].sha256) { Stop-Inventory 'COPY_VERIFICATION' }
            }
            finally { $copied.Dispose() }
        }
    }
    $manifest = [ordered]@{
        schema_version = 1
        operation = $mode
        status = 'complete'
        observed_utc = [DateTime]::UtcNow.ToString('o', [System.Globalization.CultureInfo]::InvariantCulture)
        rights = 'UNRESOLVED'
        compatibility = 'UNVERIFIED'
        metadata_method = 'System.Reflection.AssemblyName.GetAssemblyName'
        target_assembly_execution = 'NOT_RUN'
        count = $records.Count
        total_bytes = $totalBytes
        candidates = @($records.ToArray())
    }
    $json = $manifest | ConvertTo-Json -Depth 6
    if ($Copy) {
        # Publish completion last, only after every source and copied file passed.
        $manifestPath = [System.IO.Path]::Combine($destination, 'reference-inventory.json')
        $manifestStream = [System.IO.File]::Open($manifestPath, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
        try {
            $utf8 = New-Object System.Text.UTF8Encoding $false
            $jsonBytes = $utf8.GetBytes($json + [Environment]::NewLine)
            $manifestStream.Write($jsonBytes, 0, $jsonBytes.Length)
            $manifestStream.Flush($true)
        }
        catch {
            $manifestStream.Dispose()
            [System.IO.File]::Delete($manifestPath)
            throw
        }
        finally { $manifestStream.Dispose() }
    }
    Write-Output $json
}
catch {
    # Do not leak private source paths through framework exception messages.
    $message = $_.Exception.Message
    if ($message -notmatch '^ETSREF_[A-Z_]+:') { $message = 'ETSREF_IO_OR_METADATA: File operation failed; no complete copy is claimed.' }
    throw [System.InvalidOperationException]::new($message)
}
finally {
    foreach ($stream in $streams) { $stream.Dispose() }
}
