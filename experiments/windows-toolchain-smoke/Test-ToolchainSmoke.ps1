#requires -Version 5.1
<#
.SYNOPSIS
Task-003: build/run only the owned net48/x64/STA/WPF-type smoke on Windows.
.DESCRIPTION
Requires explicit MSBuild and the five privately provisioned reference DLLs from
Microsoft.NETFramework.ReferenceAssemblies.net48 1.0.3 (NuGet package SHA-256
8a7e348538e7eb91351696911689f49e3d4f63f8bab517432bbe159b8b1104a2).
Package licenseUrl points to https://github.com/Microsoft/dotnet/blob/master/LICENSE
(MIT); no license text is embedded in the inspected package. Keep provisioning and
license evidence private. No restore/download/install, vendor reference, UI,
original-DCA execution, hardware, protocol or SDK qualification occurs here.
Official reference-pack guidance:
https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/reference-assemblies
.PARAMETER MSBuildPath
Explicit absolute local MSBuild.exe path; never resolved through PATH.
.PARAMETER ExpectedMSBuildVersion
Exact pre-inventoried MSBuild executable version; no floating default.
.PARAMETER FrameworkReferenceDirectory
Explicit directory containing the five exact pinned DLLs below; no fallback.
.PARAMETER OutputDirectory
New local directory outside Git, source and reference trees; parent must exist.
Failure may leave partial owned output. Never reuse an existing directory.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $MSBuildPath,
    [Parameter(Mandatory = $true)][string] $ExpectedMSBuildVersion,
    [Parameter(Mandatory = $true)][string] $FrameworkReferenceDirectory,
    [Parameter(Mandatory = $true)][string] $OutputDirectory
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail([string] $Code) { throw [InvalidOperationException]::new('SMOKE_' + $Code) }
function Local-Path([string] $Value, [bool] $MissingLeaf = $false) {
    if ($Value -notmatch '^[A-Za-z]:[\\/]' -or $Value -match '[\x00-\x1f"<>|?*]') { Fail 'LOCAL_ABSOLUTE_PATH_REQUIRED' }
    $path = [IO.Path]::GetFullPath($Value)
    if ($path.Length -gt 3) { $path = $path.TrimEnd([char[]]@('\', '/')) }
    $drive = [IO.DriveInfo]::new([IO.Path]::GetPathRoot($path)).DriveType
    if ($drive -notin @([IO.DriveType]::Fixed, [IO.DriveType]::Removable, [IO.DriveType]::Ram)) { Fail 'LOCAL_DRIVE_REQUIRED' }
    $current = $path
    $leaf = $true
    while ($null -ne $current) {
        $item = Get-Item -LiteralPath $current -Force -ErrorAction SilentlyContinue
        if ($null -eq $item) {
            if (-not ($leaf -and $MissingLeaf)) { Fail 'PATH_MISSING' }
        }
        elseif (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { Fail 'LINK_PATH' }
        $parent = [IO.Directory]::GetParent($current)
        $current = if ($null -eq $parent) { $null } else { $parent.FullName }
        $leaf = $false
    }
    return $path
}
function Under([string] $Path, [string] $Root) {
    return $Path.Equals($Root, [StringComparison]::OrdinalIgnoreCase) -or $Path.StartsWith($Root.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)
}
function Quote([string] $Value) { return '"' + ($Value -replace '(\\+)$', '$1$1') + '"' }
function Escape-MSBuild([string] $Value) {
    foreach ($char in @('%', '$', '@', '(', ')', ';', ',', "'")) {
        $Value = $Value.Replace($char, ('%' + ([int][char]$char).ToString('X2')))
    }
    return $Value
}
function Stop-OwnedTree([Diagnostics.Process] $Process) {
    if ($Process.HasExited) { return }
    $stop = [Diagnostics.Process]::new()
    $stop.StartInfo.FileName = [IO.Path]::Combine($env:SystemRoot, 'System32', 'taskkill.exe')
    $stop.StartInfo.Arguments = '/PID ' + $Process.Id + ' /T /F'
    $stop.StartInfo.UseShellExecute = $false
    $stop.StartInfo.CreateNoWindow = $true
    $stop.StartInfo.RedirectStandardOutput = $true
    $stop.StartInfo.RedirectStandardError = $true
    try {
        $null = $stop.Start()
        if (-not $stop.WaitForExit(5000)) { $stop.Kill() }
        if (-not $Process.HasExited) { $Process.Kill() }
        $null = $Process.WaitForExit(2000)
    }
    finally { $stop.Dispose() }
}
function Run-Owned([string] $Executable, [string] $Arguments, [int] $Seconds) {
    $process = [Diagnostics.Process]::new()
    $process.StartInfo.FileName = $Executable
    $process.StartInfo.Arguments = $Arguments
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.CreateNoWindow = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.EnvironmentVariables['MSBUILDDISABLENODEREUSE'] = '1'
    $process.StartInfo.EnvironmentVariables.Remove('MSBUILD_EXE_PATH')
    $process.StartInfo.EnvironmentVariables.Remove('MSBuildExtensionsPath')
    $process.StartInfo.EnvironmentVariables.Remove('MSBuildSDKsPath')
    $stdout = [Text.StringBuilder]::new()
    $stderr = [Text.StringBuilder]::new()
    $clock = [Diagnostics.Stopwatch]::StartNew()
    try {
        $null = $process.Start()
        $outBuffer = New-Object char[] 4096
        $errBuffer = New-Object char[] 4096
        $outTask = $process.StandardOutput.ReadAsync($outBuffer, 0, $outBuffer.Length)
        $errTask = $process.StandardError.ReadAsync($errBuffer, 0, $errBuffer.Length)
        while ($null -ne $outTask -or $null -ne $errTask) {
            if ($clock.Elapsed.TotalSeconds -ge $Seconds) { Fail 'PROCESS_TIMEOUT' }
            if ($null -ne $outTask -and $outTask.IsCompleted) {
                $count = $outTask.GetAwaiter().GetResult()
                $null = $stdout.Append($outBuffer, 0, $count)
                $outTask = if ($count -eq 0) { $null } else { $process.StandardOutput.ReadAsync($outBuffer, 0, $outBuffer.Length) }
            }
            if ($null -ne $errTask -and $errTask.IsCompleted) {
                $count = $errTask.GetAwaiter().GetResult()
                $null = $stderr.Append($errBuffer, 0, $count)
                $errTask = if ($count -eq 0) { $null } else { $process.StandardError.ReadAsync($errBuffer, 0, $errBuffer.Length) }
            }
            if ($stdout.Length + $stderr.Length -gt 65536) { Fail 'OUTPUT_LIMIT' }
            Start-Sleep -Milliseconds 10
        }
        $remaining = [Math]::Max(1, [int](1000 * ($Seconds - $clock.Elapsed.TotalSeconds)))
        if (-not $process.WaitForExit($remaining)) { Fail 'PROCESS_TIMEOUT' }
        return @{ exit_code = $process.ExitCode; stdout = $stdout.ToString(); stderr = $stderr.ToString() }
    }
    finally {
        if ($process.Id -gt 0) { Stop-OwnedTree $process }
        $process.Dispose()
    }
}

try {
    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) { Fail 'WINDOWS_REQUIRED' }
    if (-not [Environment]::Is64BitProcess) { Fail 'X64_POWERSHELL_REQUIRED' }
    if ($ExpectedMSBuildVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') { Fail 'EXPECTED_VERSION_FORMAT' }
    $builder = Local-Path $MSBuildPath
    if ([IO.Path]::GetFileName($builder) -ine 'MSBuild.exe') { Fail 'MSBUILD_EXE_REQUIRED' }
    $refs = Local-Path $FrameworkReferenceDirectory
    $source = Local-Path $PSScriptRoot
    $output = Local-Path $OutputDirectory $true
    if (Test-Path -LiteralPath $output) { Fail 'OUTPUT_EXISTS' }
    foreach ($root in @($refs, $source, [IO.Path]::GetDirectoryName($builder))) {
        if ((Under $output $root) -or (Under $root $output)) { Fail 'OUTPUT_OVERLAP' }
    }
    $parent = [IO.Directory]::GetParent($output)
    while ($null -ne $parent) {
        if (Test-Path -LiteralPath ([IO.Path]::Combine($parent.FullName, '.git'))) { Fail 'OUTPUT_IN_GIT' }
        $parent = $parent.Parent
    }
    $taskAssembly = Local-Path ([IO.Path]::Combine([IO.Path]::GetDirectoryName($builder), 'Roslyn', 'Microsoft.Build.Tasks.CodeAnalysis.dll'))
    if (-not [IO.File]::Exists($taskAssembly)) { Fail 'ROSLYN_TASK_MISSING' }
    $pins = [ordered]@{
        'mscorlib.dll' = '6b35530467b914b0b195146ca1d1485ddd219afaad6461103f16f54e952f75a9'
        'System.dll' = '2fe343569f794f2ca92ee14a41875571a9f21bf92637b8f8ee86306534209cca'
        'WindowsBase.dll' = '99591f58a3b54d5e38c9ca85ac2488780161da511748005c542ee7db03857081'
        'PresentationCore.dll' = '7c6cf727ee790f6625537bd607d0d755db3510cfc7d83c3630ba375eb83555e3'
        'PresentationFramework.dll' = 'a1958e008541e47eca7828dc59d8ea60528ec235db4fdaf5222c494df1e318e2'
    }
    foreach ($name in $pins.Keys) {
        $path = Local-Path ([IO.Path]::Combine($refs, $name))
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pins[$name]) { Fail 'REFERENCE_HASH' }
    }
    $version = Run-Owned $builder '-version -nologo' 10
    if ($version.exit_code -ne 0 -or $version.stdout.Trim() -ne $ExpectedMSBuildVersion -or $version.stderr.Length -ne 0) { Fail 'MSBUILD_VERSION' }
    $null = New-Item -ItemType Directory -Path $output -ErrorAction Stop
    $project = [IO.Path]::Combine($source, 'ToolchainSmoke.csproj')
    $arguments = (Quote $project) + ' /nologo /noautoresponse /nodeReuse:false /maxcpucount:1 /target:Build /verbosity:minimal ' +
        (Quote ('/property:FrameworkReferenceDirectory=' + (Escape-MSBuild $refs))) + ' ' +
        (Quote ('/property:SmokeOutputDirectory=' + (Escape-MSBuild $output)))
    $build = Run-Owned $builder $arguments 60
    if ($build.exit_code -ne 0 -or $build.stderr.Length -ne 0) { Fail 'BUILD_FAILED' }
    $executable = Local-Path ([IO.Path]::Combine($output, 'ToolchainSmoke.exe'))
    $run = Run-Owned $executable '' 10
    $marker = 'toolchain-smoke target=net48 process=x64 apartment=STA wpf-types=3 vendor=none protocol=none'
    if ($run.exit_code -ne 0 -or $run.stdout.Trim() -ne $marker -or $run.stderr.Length -ne 0) { Fail 'RUN_FAILED' }
    [ordered]@{
        operation = 'owned_windows_toolchain_smoke'
        status = 'PASS'
        msbuild_version = $ExpectedMSBuildVersion
        target = 'net48'
        references = $pins
        executable_sha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash.ToLowerInvariant()
        marker = $marker
        original_dca = 'NOT_RUN'
        sdk_compatibility = 'UNVERIFIED'
        hardware = 'NOT_RUN'
    } | ConvertTo-Json -Depth 4
}
catch {
    $code = 'SMOKE_IO_OR_PROCESS'
    if ($_.Exception.Message -match 'SMOKE_[A-Z_]+') { $code = $Matches[0] }
    throw [InvalidOperationException]::new($code + ': Owned toolchain smoke did not pass; no host compatibility is claimed.')
}
