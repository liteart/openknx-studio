# Windows-only opt-in: genuine pinned Ids API plus owned preflight; no DCA/UI invocation.
param(
 [Parameter(Mandatory)][string]$Launcher,
 [Parameter(Mandatory)][string]$InputDirectory,
 [Parameter(Mandatory)][string]$EvidenceDirectory
)
$ErrorActionPreference='Stop'
if($env:OS -cne 'Windows_NT' -or -not [Environment]::Is64BitProcess){throw 'WINDOWS_X64_REQUIRED'}
$Launcher=(Resolve-Path -LiteralPath $Launcher).Path
$InputDirectory=(Resolve-Path -LiteralPath $InputDirectory).Path
if(Test-Path -LiteralPath $EvidenceDirectory){throw 'NEGATIVE_EVIDENCE_ALREADY_EXISTS'}
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
$null=New-Item -ItemType Directory -Path $EvidenceDirectory
foreach($case in @('actual-product','missing-context')){
 $log=Join-Path $EvidenceDirectory ($case+'.log')
 $stdout=Join-Path $EvidenceDirectory ($case+'-stdout.txt')
 $stderr=Join-Path $EvidenceDirectory ($case+'-stderr.txt')
 & $Launcher --seconds 15 --mode original-sdk-product-store-fixture --input $InputDirectory --log $log --owned-selection-negative $case > $stdout 2> $stderr
 if($LASTEXITCODE -ne 0){throw ('SELECTION_NEGATIVE_CHILD_FAIL '+$case+' exit='+$LASTEXITCODE)}
 if(-not (Test-Path -LiteralPath $log) -or (Get-Item -LiteralPath $log).Length -gt 65536){throw 'SELECTION_NEGATIVE_LOG_BOUND'}
 $raw=[IO.File]::ReadAllText($log)
 $expected=if($case -ceq 'actual-product'){'DCA_MANIFEST_APPLICATION_MISMATCH'}else{'DCA_REQUIRED_PRODUCT_CONTEXT_MISSING'}
 $marker='OWNED_SELECTION_NEGATIVE_PASS case='+$case+' rejection='+$expected+' constructor-path-entries=0'
 if(-not $raw.Contains($marker) -or -not $raw.Contains('PROJECT_RESULT exit=0') -or $raw.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $raw.Contains('DCA_UI_BEGIN')){throw ('SELECTION_NEGATIVE_ORACLE_FAIL '+$case)}
 if($case -ceq 'actual-product' -and -not $raw.Contains('original-Ids-projection=True')){throw 'GENUINE_IDS_PROJECTION_NOT_OBSERVED'}
 if($case -ceq 'missing-context' -and -not $raw.Contains('original-Ids-projection=False projection=ABSENT')){throw 'MISSING_CONTEXT_PREFLIGHT_NOT_OBSERVED'}
 Write-Output ('SELECTION_NEGATIVE PASS case='+$case+' constructor-path-entries=0 SDK-Ids-API='+($case -ceq 'actual-product')+' vendor-UI=NOT_RUN')
}
Write-Output 'SELECTION_NEGATIVE_SUITE PASS cases=2 missing-context=OWNED_PREFLIGHT vendor-missing-parameter-behavior=NOT_RUN hardware=NOT_RUN'
