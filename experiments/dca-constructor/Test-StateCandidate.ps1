# Pure candidate codec/publication policy. No vendor, WPF, Windows or disk publication.
$ErrorActionPreference='Stop'
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
$start=$source.IndexOf('internal static class ReferenceDcaStateCandidate')
$end=$source.IndexOf('// Fixed Rust PoC projection:',$start)
if($start -lt 0 -or $end -le $start){throw 'STATE_CANDIDATE_POLICY_SOURCE'}
$type=Add-Type -TypeDefinition $source.Substring($start,$end-$start) -Language CSharp -PassThru
$flags=[Reflection.BindingFlags]'Static,NonPublic'
$encode=$type.GetMethod('Encode',$flags);$publish=$type.GetMethod('CanPublish',$flags)
$hash='a'*64
[string[]]$fields=@('P-owned','I-owned','D-owned','M-owned',$hash,$hash,'4.0.0.0',$hash,$hash,$hash,'parameter-owned','description-owned','local "quoted" fixture')
$bytes=[byte[]]$encode.Invoke($null,[object[]]@($fields,123))
$text=[Text.UTF8Encoding]::new($false,$true).GetString($bytes)
$json=$text|ConvertFrom-Json -AsHashtable
if($bytes.Length -gt 16384 -or $bytes[0] -ne [byte][char]'{' -or $json.format_version -ne 1 -or $json.value -cne $fields[12] -or $json.parameter_unique_id -cne $fields[10] -or $json.host_final_validation -cne 'PASS_BEFORE_EXIT' -or -not $json.requires_confirmed_exit_zero -or $json.accepted_configuration -or $json.ContainsKey('device_uuid')){throw 'STATE_CANDIDATE_SCHEMA'}
if([Convert]::ToBase64String($bytes) -cne [Convert]::ToBase64String([byte[]]$encode.Invoke($null,[object[]]@($fields,123)))){throw 'STATE_CANDIDATE_EXACT_BYTES'}
foreach($value in @('',"line`nquote`"slash\",[string][char]0x03a9)){$copy=[string[]]$fields.Clone();$copy[12]=$value;$decoded=[Text.Encoding]::UTF8.GetString([byte[]]$encode.Invoke($null,[object[]]@($copy,123)))|ConvertFrom-Json;if($decoded.value -cne $value){throw 'STATE_CANDIDATE_JSON_ESCAPE'}}
$denied=0
foreach($case in @('null','count','field','value','hash','process','unicode')){
 $copy=[string[]]$fields.Clone();$pidValue=123
 switch($case){'null'{$copy=$null};'count'{$copy=[string[]]@('short')};'field'{$copy[0]='x'*257};'value'{$copy[12]='x'*31};'hash'{$copy[4]='A'*64};'process'{$pidValue=0};'unicode'{$copy[12]=[string][char]0xd800}}
 try{$null=$encode.Invoke($null,[object[]]@($copy,$pidValue))}catch{$denied++;continue}
 throw ('STATE_CANDIDATE_INVALID_ACCEPTED '+$case)
}
if($denied -ne 7){throw 'STATE_CANDIDATE_NEGATIVE_COUNT'}
foreach($case in @(@($true,$true,0,$true,$true),@($false,$true,0,$true,$false),@($true,$false,0,$true,$false),@($true,$true,2,$true,$false),@($true,$true,0,$false,$false))){if([bool]$publish.Invoke($null,[object[]]@([bool]$case[0],[bool]$case[1],[int]$case[2],[bool]$case[3])) -ne [bool]$case[4]){throw 'STATE_CANDIDATE_PUBLICATION_OUTCOME'}}
$main=$source.Substring($source.IndexOf('    internal static int Main(string[] args)'))
if(-not $main.Contains('if(sdkStateCapture&&outcome==0){try{if(sdkStateReload)CheckStateRestoreInput();PublishStateCandidate(outcome);}') -or $main.IndexOf('PublishStateCandidate(outcome)') -lt $main.IndexOf('SDK_UI_FINAL_REVALIDATION exit=')){throw 'STATE_CANDIDATE_AFTER_FINAL_VALIDATION'}
foreach($required in @('FileMode.CreateNew','output.Flush(true)','log.Name+".candidate.json"','if(sdkStateCapture)PrepareStateCandidate(target,(string)after)','target.Read.Invoke(target.Parameter','STATE_CAPTURE_SDK_READBACK_IDENTITY','STATE_CAPTURE_LIVE_DEVICE_MEMBERSHIP','STATE_CAPTURE_PINNED_HOST','--ui-state-capture')){if(-not $source.Contains($required)){throw ('STATE_CANDIDATE_REQUIRED '+$required)}}
Write-Output 'STATE_CANDIDATE_POLICY PASS JSON-roundtrips=4 negatives=7 publish-cases=5 max-bytes=16384 value-max-UTF16=30 value-max-UTF8=120 output=FRESH_LOG_SIDECAR final-validation-before-publication=True confirmed-launcher-exit0=REQUIRED actual-SDK-and-Windows=NOT_RUN Rust-durability=NOT_RUN accepted-config=NOT_CLAIMED'
