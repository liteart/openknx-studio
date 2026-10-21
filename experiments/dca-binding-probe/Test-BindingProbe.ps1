param(
    [Parameter(Mandatory=$true)][string] $Compiler,
    [Parameter(Mandatory=$true)][string] $CoreReference,
    [Parameter(Mandatory=$true)][string] $CoreReferenceSha256,
    [Parameter(Mandatory=$true)][string] $InputDirectory,
    [Parameter(Mandatory=$true)][string] $OutputDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Run([string] $File, [string[]] $Arguments, [int] $Expected, [string] $Marker = '') {
    $p = New-Object Diagnostics.Process
    $p.StartInfo.FileName = $File
    $p.StartInfo.Arguments = (($Arguments | ForEach-Object { if ($_ -match '["\r\n]') { throw 'UNSAFE_ARGUMENT' }; '"' + $_ + '"' }) -join ' ')
    $p.StartInfo.UseShellExecute = $false
    $p.StartInfo.CreateNoWindow = $true
    $p.StartInfo.RedirectStandardOutput = $true
    $p.StartInfo.RedirectStandardError = $true
    try {
        if (-not $p.Start()) { throw 'START_FAILED' }
        $stdout = $p.StandardOutput.ReadToEndAsync()
        $stderr = $p.StandardError.ReadToEndAsync()
        if (-not $p.WaitForExit(20000)) { $p.Kill(); throw 'PROCESS_TIMEOUT' }
        $o = $stdout.Result; $e = $stderr.Result
        if (($o.Length + $e.Length) -gt 65536) { throw 'OUTPUT_BOUND' }
        Write-Output $o
        if ($Marker.Length -gt 0 -and -not $o.Contains($Marker)) { throw ('EXPECTED_MARKER_MISSING ' + $Marker) }
        if ($e.Length -gt 0 -or $p.ExitCode -ne $Expected) { throw ('UNEXPECTED_PROCESS_RESULT ' + $p.ExitCode + ' ' + $e) }
        return
    } finally { $p.Dispose() }
}
function Config([string] $Path, [string[]] $Names, [string] $Token, [string] $Old, [string] $New) {
    $entries = foreach ($n in $Names) {
        '<dependentAssembly><assemblyIdentity name="' + $n + '" publicKeyToken="' + $Token + '" culture="neutral"/><bindingRedirect oldVersion="' + $Old + '" newVersion="' + $New + '"/></dependentAssembly>'
    }
    [IO.File]::WriteAllText($Path, '<configuration><runtime><assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">' + ($entries -join '') + '</assemblyBinding></runtime></configuration>')
}
if ($env:OS -ne 'Windows_NT' -or -not [Environment]::Is64BitProcess) { throw 'WINDOWS_X64_REQUIRED' }
if (Test-Path -LiteralPath $OutputDirectory) { throw 'OUTPUT_MUST_BE_NEW' }
if ((Get-FileHash -LiteralPath $CoreReference -Algorithm SHA256).Hash.ToLowerInvariant() -cne $CoreReferenceSha256) { throw 'CORE_REFERENCE_HASH' }
if (-not (Test-Path -LiteralPath $Compiler -PathType Leaf)) { throw 'COMPILER_REQUIRED' }
$root = [IO.Path]::GetFullPath($OutputDirectory)
$null = New-Item -ItemType Directory -Path $root
Write-Output ('PLATFORM CLR=' + [Environment]::Version + ' PS=' + $PSVersionTable.PSVersion + ' compiler=' + [Diagnostics.FileVersionInfo]::GetVersionInfo($Compiler).FileVersion)
foreach ($d in @('old','new','owned-no-policy','owned-policy','reflection-no-policy','reflection-policy')) { $null = New-Item -ItemType Directory -Path (Join-Path $root $d) }
$key = New-Object Security.Cryptography.RSACryptoServiceProvider(2048)
try { $key.PersistKeyInCsp = $false; [IO.File]::WriteAllBytes((Join-Path $root 'owned.snk'), $key.ExportCspBlob($true)) } finally { $key.Dispose() }
foreach ($version in @('5.7.0.0','6.4.0.0')) {
    $d = if ($version -eq '5.7.0.0') { 'old' } else { 'new' }
    $source = Join-Path $root ($d + '.cs')
    [IO.File]::WriteAllText($source, '[assembly:System.Reflection.AssemblyVersion("' + $version + '")] public static class OwnedContract { public static string Run() { return "owned-' + $version + '"; } }')
    Run $Compiler @('/nologo','/noconfig','/nostdlib','/target:library',('/reference:' + $CoreReference),('/keyfile:' + (Join-Path $root 'owned.snk')),('/out:' + (Join-Path $root ($d + '\Owned.BindingContract.dll'))),$source) 0
}
$probe = Join-Path $root 'BindingProbe.exe'
Run $Compiler @('/nologo','/noconfig','/nostdlib','/platform:x64',('/reference:' + $CoreReference),('/out:' + $probe),(Join-Path $PSScriptRoot 'BindingProbe.cs')) 0
$identity = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $root 'old\Owned.BindingContract.dll'))
$token = ([BitConverter]::ToString($identity.GetPublicKeyToken())).Replace('-','').ToLowerInvariant()
foreach ($d in @('owned-no-policy','owned-policy','reflection-no-policy','reflection-policy')) {
    Copy-Item -LiteralPath $probe -Destination (Join-Path $root ($d + '\BindingProbe.exe'))
}
foreach ($d in @('owned-no-policy','owned-policy')) { Copy-Item -LiteralPath (Join-Path $root 'new\Owned.BindingContract.dll') -Destination (Join-Path $root ($d + '\Owned.BindingContract.dll')) }
Config (Join-Path $root 'owned-policy\BindingProbe.exe.config') @('Owned.BindingContract') $token '5.7.0.0' '6.4.0.0'
Write-Output 'CASE owned-no-policy expected=FAIL'
Run (Join-Path $root 'owned-no-policy\BindingProbe.exe') @('owned',$identity.FullName) 2 'System.IO.FileLoadException'
Write-Output 'CASE owned-policy expected=PASS'
Run (Join-Path $root 'owned-policy\BindingProbe.exe') @('owned',$identity.FullName) 0 'OWNED_RESULT owned-6.4.0.0'
Config (Join-Path $root 'reflection-policy\BindingProbe.exe.config') @('Knx.Ets.Sdk','Knx.Ets.Sdk.AddIns.AddInViews') '61439873ec5e1159' '5.7.554.37315' '6.4.8718.0'
# Common's observed requested version differs from the SDK version.
$path = Join-Path $root 'reflection-policy\BindingProbe.exe.config'
$text = [IO.File]::ReadAllText($path)
$common = '<dependentAssembly><assemblyIdentity name="Knx.Ets.Common.Types" publicKeyToken="61439873ec5e1159" culture="neutral"/><bindingRedirect oldVersion="5.7.286.36952" newVersion="6.4.8718.0"/></dependentAssembly>'
$tasks = '<dependentAssembly><assemblyIdentity name="System.Threading.Tasks.Extensions" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral"/><bindingRedirect oldVersion="4.2.0.1" newVersion="4.2.1.0"/></dependentAssembly>'
[IO.File]::WriteAllText($path, $text.Replace('</assemblyBinding>', $common + $tasks + '</assemblyBinding>'))
Write-Output 'CASE reflection-no-policy expected=FAIL'
Run (Join-Path $root 'reflection-no-policy\BindingProbe.exe') @('reflection',$InputDirectory) 2 'POLICY_REJECT POLICY_IDENTITY_MISMATCH requested=Knx.Ets.Sdk, Version=5.7.554.37315'
Write-Output 'CASE reflection-policy expected=INCONCLUSIVE-missing-dependency'
Run (Join-Path $root 'reflection-policy\BindingProbe.exe') @('reflection',$InputDirectory) 2 'DEPENDENCY_REJECT Microsoft.Bcl.AsyncInterfaces, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Write-Output 'SUITE COMPLETED owned-binding=PASS vendor-member-resolution=INCONCLUSIVE vendor-execution=NOT_RUN hardware=NOT_RUN'
