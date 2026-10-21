#requires -Version 7.6
# Owned synthetic metadata only; no vendor input or executable target bodies.
[CmdletBinding()]
param([string] $TemporaryParent='/private/tmp')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$tool=Join-Path $PSScriptRoot 'Read-MemberMetadata.ps1'
if (-not (Test-Path $tool)) { throw 'DCAMEMBER_IMPLEMENTATION_MISSING' }
$references=@('System.Reflection.Metadata','System.Collections.Immutable','System.Collections','System.Text.Json','System.Security.Cryptography')
Add-Type -ReferencedAssemblies $references -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'MemberMetadata.cs')))
Add-Type -ReferencedAssemblies $references -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'OwnedFixtures.cs')))
$root=Join-Path $TemporaryParent ('task142-owned-'+[Guid]::NewGuid().ToString('N'))
$passed=0;$failed=0
function Assert([bool] $Condition,[string] $Message) { if(-not $Condition){throw $Message} }
function Case([string] $Label,[scriptblock] $Action) {
 try { & $Action;$script:passed++;Write-Output "PASS: $Label" }
 catch { $script:failed++;Write-Output ("FAIL: ${Label}: "+$_.Exception.Message) }
}
function Hash([string] $Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Reject([scriptblock] $Action,[string] $Code) {
 $seen=$false;try{$null=& $Action}catch{$seen=$true;Assert ($_.Exception.ToString().Contains($Code)) ('Expected '+$Code+' got '+$_.Exception.Message)}
 Assert $seen ('Expected rejection '+$Code)
}
try {
 $null=New-Item -ItemType Directory -Path $root
 $sdk=Join-Path $root 'OwnedSdk.dll';$consumer=Join-Path $root 'OwnedConsumer.dll';$changed=Join-Path $root 'Changed.dll'
 $probes=[Task142OwnedFixtures]::Create($sdk,$true,$false,1)
 $null=[Task142OwnedFixtures]::Create($consumer,$false,$false,1)
 $null=[Task142OwnedFixtures]::Create($changed,$true,$true,1)
 function Report([string] $Candidate=$sdk) { & $tool -DcaPath $consumer -DcaSha256 (Hash $consumer) -ReferencePath @($Candidate) -ReferenceSha256 @((Hash $Candidate)) }
 Case 'direct constructors methods fields and generic parameters match structurally' {
  $raw=Report;$r=$raw|ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count -eq 5) 'Expected five direct signature matches'
  Assert ($r.direct_typeref_candidate_members -eq 7) 'Expected seven direct TypeRef members'
  Assert ($r.runtime_execution -eq 'NOT_RUN') 'Runtime claim'
  Assert (-not $raw.Contains($root)) 'Path leak'
 }
 Case 'bounded generic TypeSpec parent resolves candidate scope' {
  $r=Report|ConvertFrom-Json
  Assert ($r.typespec_member_parents.Count -eq 1 -and $r.typespec_member_parents[0].candidate_directed) 'Generic scope unresolved'
  Assert ($r.typespec_member_parents[0].owner_type -eq 'Owned.Owner`1') 'Wrong generic owner'
 }
 Case 'unresolved inherited member is not claimed absent' {
  $r=Report|ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object { $_.member_name -eq 'Inherited' -and $_.disposition -eq 'NO_DIRECT_NAMED_DEFINITION_INHERITANCE_UNRESOLVED' }).Count -eq 1) 'Inheritance uncertainty missing'
 }
 Case 'version observation and direct mismatch remain separate' {
  $r=Report $changed|ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object exact_assembly_identity_match).Count -eq 0) 'Version mismatch hidden'
  Assert (@($r.candidate_members|Where-Object { $_.member_name -eq 'Ping' -and $_.disposition -eq 'NO_DIRECT_SIGNATURE_MATCH_INHERITANCE_UNRESOLVED' }).Count -eq 1) 'Signature mismatch missing'
  Assert (@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count -eq 4) 'Version changed structural observation'
 }
 Case 'different declaring-owner token cannot match primitive signatures' {
  $wrongTokenConsumer=Join-Path $root 'WrongTokenConsumer.dll'
  $null=[Task142OwnedFixtures]::Create($wrongTokenConsumer,$false,$false,1,0,0,0,$null,$true)
  $r=& $tool -DcaPath $wrongTokenConsumer -DcaSha256 (Hash $wrongTokenConsumer) -ReferencePath @($sdk) -ReferenceSha256 @((Hash $sdk)) | ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count -eq 0) 'Wrong owner token yielded a structural match'
  Assert (@($r.candidate_members|Where-Object { $_.member_name -eq '.ctor' -and $_.disposition -eq 'OWNER_IDENTITY_MISMATCH_UNRESOLVED' }).Count -eq 1) 'Wrong owner token not explicitly unresolved'
  Assert ($r.candidate_members[0].requested_assembly.public_key_token -eq '0102030405060708' -and $r.candidate_members[0].supplied_assembly.public_key_token -eq 'null') 'Token evidence missing'
 }
 Case 'different declaring-owner culture cannot match primitive signatures' {
  $wrongCultureConsumer=Join-Path $root 'WrongCultureConsumer.dll'
  $null=[Task142OwnedFixtures]::Create($wrongCultureConsumer,$false,$false,1,0,0,0,'fr-FR',$false)
  $r=& $tool -DcaPath $wrongCultureConsumer -DcaSha256 (Hash $wrongCultureConsumer) -ReferencePath @($sdk) -ReferenceSha256 @((Hash $sdk)) | ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count -eq 0) 'Wrong owner culture yielded a structural match'
  Assert (@($r.candidate_members|Where-Object { $_.member_name -eq '.ctor' -and $_.disposition -eq 'OWNER_IDENTITY_MISMATCH_UNRESOLVED' }).Count -eq 1) 'Wrong owner culture not explicitly unresolved'
  Assert ($r.candidate_members[0].requested_assembly.culture -eq 'fr-FR' -and $r.candidate_members[0].supplied_assembly.culture -eq 'neutral') 'Culture evidence missing'
 }
 foreach($profile in @(1,2,3,4,5)) {
  Case ("ambiguous type identifier profile $profile rejects before matching") {
   $ambiguousSdk=Join-Path $root ("AmbiguousSdk$profile.dll")
   $ambiguousConsumer=Join-Path $root ("AmbiguousConsumer$profile.dll")
   $null=[Task142OwnedFixtures]::Create($ambiguousSdk,$true,$false,1,0,0,0,$null,$false,$profile)
   $null=[Task142OwnedFixtures]::Create($ambiguousConsumer,$false,$false,1,0,0,0,$null,$false,$profile)
   Reject {
    $r=& $tool -DcaPath $ambiguousConsumer -DcaSha256 (Hash $ambiguousConsumer) -ReferencePath @($ambiguousSdk) -ReferenceSha256 @((Hash $ambiguousSdk)) | ConvertFrom-Json
    throw ('Unexpected report with structural matches: '+@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count)
   } 'DCAMEMBER_UNSUPPORTED_TYPE_IDENTIFIER'
  }
 }
 Case 'unambiguous nested declaring owner preserves structural matches' {
  $nestedSdk=Join-Path $root 'NestedSdk.dll';$nestedConsumer=Join-Path $root 'NestedConsumer.dll'
  $null=[Task142OwnedFixtures]::Create($nestedSdk,$true,$false,1,0,0,0,$null,$false,6)
  $null=[Task142OwnedFixtures]::Create($nestedConsumer,$false,$false,1,0,0,0,$null,$false,6)
  $r=& $tool -DcaPath $nestedConsumer -DcaSha256 (Hash $nestedConsumer) -ReferencePath @($nestedSdk) -ReferenceSha256 @((Hash $nestedSdk)) | ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object disposition -eq 'DIRECT_SIGNATURE_STRUCTURAL_MATCH').Count -eq 5) 'Unambiguous nested owners failed comparison'
  Assert ($r.candidate_members[0].declaring_type -eq 'Owned.Outer+Owner`1') 'Wrong nested owner display'
 }
 Case 'malformed versus unsupported member signatures' {
  $r=Report|ConvertFrom-Json
  Assert (@($r.candidate_members|Where-Object disposition -eq 'MALFORMED_SIGNATURE').Count -eq 1) 'Malformed distinction missing'
  Assert (@($r.candidate_members|Where-Object disposition -eq 'UNSUPPORTED_CALLING_CONVENTION_VarArgs').Count -eq 1) 'Unsupported distinction missing'
 }
 Case 'raw target-framework attribute bytes and deterministic report' {
  $one=Report;$two=Report;Assert ($one -eq $two) 'Nondeterministic report'
  $r=$one|ConvertFrom-Json
  Assert ($r.inputs[0].target_framework_attributes.Count -eq 1 -and $r.inputs[1].target_framework_attributes.Count -eq 1) 'Framework attribute missing'
  Assert ($r.inputs[0].target_framework_attributes[0].interpretation -eq 'RAW_METADATA_ONLY') 'Attribute executed or decoded claim'
 }
 Case 'valid required and optional modifiers remain distinct' {
  $required=[Task142.MemberMetadata]::ProbeSignature($sdk,$probes['modifier'],$true)
  $optional=[Task142.MemberMetadata]::ProbeSignature($sdk,$probes['optional'],$true)
  Assert ($required -ne $optional -and $required.Contains('RequiredModifier') -and $optional.Contains('OptionalModifier')) 'Modifiers conflated'
 }
 Case 'truncation trailing bytes invalid code and void field rejected' {
  foreach($name in @('truncated','trailing','invalid','voidfield')) { Reject { [Task142.MemberMetadata]::ProbeSignature($sdk,$probes[$name],$true) } 'BadImageFormatException' }
 }
 Case 'unsupported array grammar never becomes a signature' { Reject { [Task142.MemberMetadata]::ProbeSignature($sdk,$probes['array'],$true) } 'UNSUPPORTED_TYPE_Array' }
 foreach($name in @('count','depth','nodes','blob')) {
  $expected=@{count='COUNT_LIMIT';depth='DEPTH_LIMIT';nodes='NODE_LIMIT';blob='BLOB_LIMIT'}[$name]
  Case ("bounded $name rejection") { Reject { [Task142.MemberMetadata]::ProbeSignature($sdk,$probes[$name],$true) } ('DCAMEMBER_'+$expected) }
 }
 Case 'framework attribute blob byte limit rejects through managed Read path' {
  $oversized=Join-Path $root 'OversizedAttribute.dll'
  $null=[Task142OwnedFixtures]::Create($oversized,$true,$false,1,0,0,513)
  Reject { Report $oversized } 'DCAMEMBER_ATTRIBUTE_BLOB_LIMIT'
 }
 Case 'incremental report budget rejects before final report serialization' { Reject { [Task142.MemberMetadata]::ProbeIncrementalReportLimit() } 'DCAMEMBER_REPORT_LIMIT' }
 Case 'report byte limit rejects' { Reject { [Task142.MemberMetadata]::ProbeReportLimit() } 'DCAMEMBER_REPORT_LIMIT' }
 Case 'metadata row limit rejects before iteration' {
  $many=Join-Path $root 'Many.dll';$null=[Task142OwnedFixtures]::Create($many,$true,$false,32769)
  Reject { Report $many } 'DCAMEMBER_ROW_LIMIT'
 }
 Case 'all hashes verified before any metadata reader' {
  $bad=Join-Path $root 'Bad.dll';[IO.File]::WriteAllText($bad,'not metadata')
  Reject { & $tool -DcaPath $bad -DcaSha256 (Hash $bad) -ReferencePath @($sdk) -ReferenceSha256 @(('0'*64)) } 'DCAMEMBER_HASH_MISMATCH'
 }
 Case 'work budget rejects' { Reject { [Task142.MemberMetadata]::ProbeWorkLimit() } 'DCAMEMBER_WORK_LIMIT' }
 Case 'input count duplicate paths and hash format reject' {
  Reject { & $tool -DcaPath $consumer -DcaSha256 (Hash $consumer) -ReferencePath @($sdk,$sdk,$sdk) -ReferenceSha256 @((Hash $sdk),(Hash $sdk),(Hash $sdk)) } 'DCAMEMBER_INPUT_COUNT'
  Reject { & $tool -DcaPath $consumer -DcaSha256 (Hash $consumer) -ReferencePath @($consumer) -ReferenceSha256 @((Hash $consumer)) } 'DCAMEMBER_DUPLICATE_PATH'
  Reject { & $tool -DcaPath $consumer -DcaSha256 'invalid' -ReferencePath @($sdk) -ReferenceSha256 @((Hash $sdk)) } 'DCAMEMBER_EXPECTED_HASH_FORMAT'
 }
 Case 'per-file and aggregate byte limits precede metadata' {
  $large=Join-Path $root 'Large.dll';$stream=[IO.File]::Create($large)
  try{$stream.SetLength(16MB+1)}finally{$stream.Dispose()}
  Reject { & $tool -DcaPath $large -DcaSha256 ('0'*64) -ReferencePath @($sdk) -ReferenceSha256 @((Hash $sdk)) } 'DCAMEMBER_FILE_LIMIT'
  $paths=@();$hashes=@()
  foreach($i in 1..3) {
   $path=Join-Path $root ("Total$i.dll");$stream=[IO.File]::Create($path)
   try{$stream.SetLength(12MB)}finally{$stream.Dispose()}
   $paths+=$path;$hashes+=Hash $path
  }
  Reject { & $tool -DcaPath $paths[0] -DcaSha256 $hashes[0] -ReferencePath @($paths[1],$paths[2]) -ReferenceSha256 @($hashes[1],$hashes[2]) } 'DCAMEMBER_TOTAL_LIMIT'
 }
 Case 'name and declaring-type nesting bounds reject' {
  $long=Join-Path $root 'Long.dll';$null=[Task142OwnedFixtures]::Create($long,$true,$false,1,1025,0)
  Reject { Report $long } 'DCAMEMBER_STRING_LIMIT'
  $deep=Join-Path $root 'Deep.dll';$null=[Task142OwnedFixtures]::Create($deep,$true,$false,1,0,16)
  Reject { Report $deep } 'DCAMEMBER_TYPE_SCOPE_LIMIT'
 }
 Case 'nonmanaged bytes fail bounded' {
  $bad=Join-Path $root 'Bad.dll'
  Reject { & $tool -DcaPath $bad -DcaSha256 (Hash $bad) -ReferencePath @($sdk) -ReferenceSha256 @((Hash $sdk)) } 'DCAMEMBER_MALFORMED_METADATA'
 }
} finally { if(Test-Path $root){Remove-Item -LiteralPath $root -Recurse -Force} }
Write-Output "Results: PASS=$passed FAIL=$failed"
if($failed -gt 0){exit 1}
