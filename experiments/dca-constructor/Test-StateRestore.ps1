# Fixed Rust projection codec and host scope checks; vendor/Windows execution is not performed.
$ErrorActionPreference='Stop'
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'ProjectConstructorProbe.cs'))
$start=$source.IndexOf('internal sealed class ReferenceDcaStateRestore')
$end=$source.IndexOf('// An absent device is rejected',$start)
if($start -lt 0 -or $end -le $start){throw 'STATE_RESTORE_POLICY_SOURCE'}
$types=Add-Type -TypeDefinition ('using System;using System.IO;using System.Text;'+$source.Substring($start,$end-$start)) -Language CSharp -PassThru
$type=$types|Where-Object Name -CEQ 'ReferenceDcaStateRestore'
$static=[Reflection.BindingFlags]'Static,NonPublic';$instance=[Reflection.BindingFlags]'Instance,NonPublic'
$decode=$type.GetMethod('Decode',$static);$field=$type.GetMethod('Field',$instance)
$hash='a'*64
$projection=[ordered]@{format_version=1;kind='owned-offline-scalar-restore';fixture_uuid_revision_synthetic=$true;accepted_configuration=$false;state_artifact_sha256=$hash;device_uuid='12345678-1234-1234-1234-123456789abc';originating_revision=('b'*40);project_id='P-owned';installation_id='I-owned';device_id='D-owned';application_id='A-owned';product_sha256=$hash;dca_sha256=$hash;dca_version='4.0.0.0';sdk_sha256=$hash;object_model_sha256=$hash;parameter_unique_id='parameter-owned';parameter_internal_name='description-owned';value='owned fixture'}
$json=$projection|ConvertTo-Json -Compress -Depth 4
function Decode([string]$text){return $decode.Invoke($null,[object[]]@(,[Text.UTF8Encoding]::new($false,$true).GetBytes($text)))}
$identity=$type.GetMethod('ExactParameterIdentity',$static)
foreach($case in @(@($true,$true,$true,$true,$true),@($false,$true,$true,$true,$false),@($true,$false,$true,$true,$false),@($true,$true,$false,$true,$false),@($true,$true,$true,$false,$false))){if([bool]$identity.Invoke($null,[object[]]@([bool]$case[0],[bool]$case[1],[bool]$case[2],[bool]$case[3])) -ne [bool]$case[4]){throw 'STATE_RESTORE_PARAMETER_IDENTITY_GUARD'}}
Write-Output 'STATE_RESTORE_PARAMETER_IDENTITY PASS cases=5 exact-original-UID-internalname-parent=REQUIRED raw-configuration=NOT_LOGGED identity-rejection=UNCHANGED actual-SDK=NOT_RUN'
$heldHash=$type.GetMethod('HeldHash',$static);$raw=[Text.Encoding]::UTF8.GetBytes($json);$stream=[IO.MemoryStream]::new($raw,$false)
try{$expected=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($raw)).ToLowerInvariant();foreach($position in @(0,5,$stream.Length)){$stream.Position=$position;if($heldHash.Invoke($null,@($stream)) -cne $expected -or $stream.Position -ne $position){throw 'STATE_RESTORE_WHOLE_HELD_INPUT_HASH'}}}finally{$stream.Dispose()}
Write-Output 'STATE_RESTORE_HELD_HASH PASS positions=START_MIDDLE_EOF exact-full-input=True original-position-restored=True SDK-runtime=NOT_RUN'
$actual=Decode $json
if($field.Invoke($actual,@('value')) -cne 'owned fixture' -or $field.Invoke($actual,@('project_id')) -cne 'P-owned'){throw 'STATE_RESTORE_VALUE'}
foreach($text in @('quote" and slash\',"line`nnext",[string][char]0x03a9)){$sample=$json.Replace('owned fixture',(($text|ConvertTo-Json -Compress).Trim('"')));$value=Decode $sample;if($field.Invoke($value,@('value')) -cne $text){throw 'STATE_RESTORE_JSON_ESCAPES'}}
$denied=0
foreach($case in @('version','kind','synthetic','accepted','unknown','duplicate','missing','trailing','truncated','null','nested','value','hash','uuid','revision','field-bound')){
 $bad=$json
 switch($case){
  'version'{$bad=$bad.Replace('"format_version":1','"format_version":2')}
  'kind'{$bad=$bad.Replace('owned-offline-scalar-restore','other')}
  'synthetic'{$bad=$bad.Replace('"fixture_uuid_revision_synthetic":true','"fixture_uuid_revision_synthetic":false')}
  'accepted'{$bad=$bad.Replace('"accepted_configuration":false','"accepted_configuration":true')}
  'unknown'{$bad=$bad.Replace('"kind":','"extra":1,"kind":')}
  'duplicate'{$bad=$bad.Replace('"kind":','"format_version":1,"kind":')}
  'missing'{$bad=$bad.Replace('"format_version":1,','')}
  'trailing'{$bad+=' '}
  'truncated'{$bad=$bad.Substring(0,$bad.Length-1)}
  'null'{$bad=$bad.Replace('"value":"owned fixture"','"value":null')}
  'nested'{$bad=$bad.Replace('"value":"owned fixture"','"value":{}')}
  'value'{$bad=$bad.Replace('owned fixture',('x'*31))}
  'hash'{$bad=$bad.Replace('"product_sha256":"'+$hash+'"','"product_sha256":"'+('A'*64)+'"')}
  'uuid'{$bad=$bad.Replace('12345678-1234-1234-1234-123456789abc','not-a-uuid')}
  'revision'{$bad=$bad.Replace(('b'*40),'not-a-revision')}
  'field-bound'{$bad=$bad.Replace('P-owned',('x'*257))}
 }
 try{$null=Decode $bad}catch{$denied++;continue};throw ('STATE_RESTORE_INVALID_ACCEPTED '+$case)
}
foreach($bytes in @([byte[]]@(255),[byte[]]::new(16385))){try{$null=$decode.Invoke($null,[object[]]@(,$bytes))}catch{$denied++;continue};throw 'STATE_RESTORE_INVALID_BYTES_ACCEPTED'}
if($denied -ne 18){throw 'STATE_RESTORE_NEGATIVE_COUNT'}
$loadStart=$source.IndexOf('    static void LoadStateRestoreProjection(');$loadEnd=$source.IndexOf('    static void InspectOriginalStateBindings(',$loadStart);$restoreSource=$source.Substring($loadStart,$loadEnd-$loadStart)
foreach($required in @('log.Name+".restore.json"','FileShare.Read','ReferenceDcaStateRestore.HeldHash(sdkRestoreInput)==sdkRestoreHash','sdkRestoreInput.Length<=16384','STATE_RESTORE_REFERENCE_PINS','STATE_RESTORE_GENUINE_CONTEXT_IDENTITY','0x060013f9','0x060013cd','STATE_RESTORE_EXACT_PARAMETER_IDENTITY','DCA_STATE_RESTORE_MODULE_SCAN modules=','++count<=1024','matches==1','Object.ReferenceEquals(parameterModule.Invoke(candidate,new object[0]),current)','0x060010e0','0x0600100b','0x060013cb','DCA_STATE_RESTORE_PARAMETER_IDENTITY stage=','ReferenceDcaStateRestore.ExactParameterIdentity(original,uidMatch,nameMatch,parentMatch)','if(changed)write.Invoke(parameter,new object[]{value})','finally{sdkRestoreActive=false','STATE_RESTORE_ORIGINAL_DISPLAY_AND_SDK_VALUE','PrepareStateCandidate(target,(string)actual)')){if(-not $restoreSource.Contains($required)){throw ('STATE_RESTORE_REQUIRED '+$required)}}
if($restoreSource.Contains('SetValue(') -or $restoreSource.Contains('sdkStateEnterEvent=') -or $restoreSource.Contains('sdkProductUiActive=true') -or $restoreSource.Contains('BindingFlags.NonPublic')){throw 'STATE_RESTORE_PRIVATE_OR_FAKE_SCOPE'}
$uiStart=$source.IndexOf('    static void ShowSdkProductUiCore(');$uiEnd=$source.IndexOf('    static void ProbeSdkProject(',$uiStart);$ui=$source.Substring($uiStart,$uiEnd-$uiStart)
if($ui.IndexOf('if(sdkStateReload)ApplyStateRestore(device)') -gt $ui.IndexOf('sdkUiCallAttempted=true') -or -not $source.Contains('return StateEditDispatcherEligible()&&(sdkStateEdit&&sdkStateEnterEvent!=null||sdkStateReload&&sdkRestoreActive);')){throw 'STATE_RESTORE_SETTER_SCOPE_AND_ORDER'}
$main=$source.Substring($source.IndexOf('    static int MainCore('))
if($main.IndexOf('LoadStateRestoreProjection()') -gt $main.IndexOf('VerifyBindingConfiguration()')){throw 'STATE_RESTORE_PIN_BEFORE_VENDOR_EFFECTS'}
Write-Output 'STATE_RESTORE_POLICY PASS codec-roundtrips=4 negatives=18 max-bytes=16384 keys=19 exact-Rust-flat-schema=True independent-input-pin-before-effects=True same-genuine-identity-before-setter=True explicit-owner-scope=True fake-input-and-UI-publication=NOT_USED original-public-setter-before-GetDcaUi=True actual-Windows-reload=NOT_RUN accepted-config=NOT_CLAIMED'
