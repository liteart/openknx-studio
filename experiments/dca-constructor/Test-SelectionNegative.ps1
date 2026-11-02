# Windows-only opt-in; default: pinned Ids API and owned preflight without DCA/UI.
# MissingParameterOnly builds/initializes the genuine context, then rejects an owned omission before GetDcaUi.
param(
 [Parameter(Mandatory)][string]$Launcher,
 [Parameter(Mandatory)][string]$InputDirectory,
 [Parameter(Mandatory)][string]$EvidenceDirectory,
 [switch]$MissingParameterOnly
)
$ErrorActionPreference='Stop'
if($env:OS -cne 'Windows_NT' -or -not [Environment]::Is64BitProcess){throw 'WINDOWS_X64_REQUIRED'}
$Launcher=(Resolve-Path -LiteralPath $Launcher).Path
$InputDirectory=(Resolve-Path -LiteralPath $InputDirectory).Path
if(Test-Path -LiteralPath $EvidenceDirectory){throw 'NEGATIVE_EVIDENCE_ALREADY_EXISTS'}
$EvidenceDirectory=[IO.Path]::GetFullPath($EvidenceDirectory)
$null=New-Item -ItemType Directory -Path $EvidenceDirectory
$cases=if($MissingParameterOnly){@('missing-definition')}else{@('actual-product','missing-context')}
foreach($case in $cases){
 $log=Join-Path $EvidenceDirectory ($case+'.log')
 $stdout=Join-Path $EvidenceDirectory ($case+'-stdout.txt')
 $stderr=Join-Path $EvidenceDirectory ($case+'-stderr.txt')
 if($MissingParameterOnly){
  & $Launcher --seconds 45 --mode original-sdk-product-store-fixture --input $InputDirectory --log $log --owned-parameter-negative missing-definition > $stdout 2> $stderr
 }else{
  & $Launcher --seconds 15 --mode original-sdk-product-store-fixture --input $InputDirectory --log $log --owned-selection-negative $case > $stdout 2> $stderr
 }
 if($LASTEXITCODE -ne 0){throw ('SELECTION_NEGATIVE_CHILD_FAIL '+$case+' exit='+$LASTEXITCODE)}
 if(-not (Test-Path -LiteralPath $log) -or (Get-Item -LiteralPath $log).Length -gt 65536){throw 'SELECTION_NEGATIVE_LOG_BOUND'}
 $raw=[IO.File]::ReadAllText($log)
 if($MissingParameterOnly){
  if(-not $raw.Contains('DCA_REQUIRED_PARAMETER_DEFINITION_READY source=ORIGINAL_SDK') -or -not $raw.Contains('OWNED_PARAMETER_DEFINITION_NEGATIVE_PASS genuine-SDK-definition-control=1 omission=OWNED_CONTEXT_VIEW_ONLY rejection=DCA_REQUIRED_PARAMETER_DEFINITION_MISSING original-GetDcaUi-attempts=0 UI-config-action-entries=0') -or -not $raw.Contains('PROJECT_RESULT exit=0') -or $raw.Contains('DCA_UI_BEGIN')){throw 'PARAMETER_DEFINITION_NEGATIVE_ORACLE_FAIL'}
  Write-Output 'PARAMETER_DEFINITION_NEGATIVE PASS actual-SDK-definition-control=OBSERVED omission=OWNED_CONTEXT_VIEW GetDcaUi=NOT_RUN vendor-missing-definition-behavior=NOT_RUN'
  continue
 }
 $expected=if($case -ceq 'actual-product'){'DCA_MANIFEST_APPLICATION_MISMATCH'}else{'DCA_REQUIRED_PRODUCT_CONTEXT_MISSING'}
 $marker='OWNED_SELECTION_NEGATIVE_PASS case='+$case+' rejection='+$expected+' constructor-path-entries=0'
 if(-not $raw.Contains($marker) -or -not $raw.Contains('PROJECT_RESULT exit=0') -or $raw.Contains('FACTORY_CONSTRUCTOR_BEGIN') -or $raw.Contains('DCA_UI_BEGIN')){throw ('SELECTION_NEGATIVE_ORACLE_FAIL '+$case)}
 if($case -ceq 'actual-product' -and -not $raw.Contains('original-Ids-projection=True')){throw 'GENUINE_IDS_PROJECTION_NOT_OBSERVED'}
 if($case -ceq 'missing-context' -and -not $raw.Contains('original-Ids-projection=False projection=ABSENT')){throw 'MISSING_CONTEXT_PREFLIGHT_NOT_OBSERVED'}
 Write-Output ('SELECTION_NEGATIVE PASS case='+$case+' constructor-path-entries=0 SDK-Ids-API='+($case -ceq 'actual-product')+' vendor-UI=NOT_RUN')
}
if($MissingParameterOnly){Write-Output 'PARAMETER_DEFINITION_NEGATIVE_SUITE PASS cases=1 original-context=True missing-definition=OWNED_PREFLIGHT vendor-missing-definition-behavior=NOT_RUN hardware=NOT_RUN'}else{Write-Output 'SELECTION_NEGATIVE_SUITE PASS cases=2 missing-context=OWNED_PREFLIGHT vendor-missing-parameter-behavior=NOT_RUN hardware=NOT_RUN'}
