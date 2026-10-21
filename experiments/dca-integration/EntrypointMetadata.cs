// Original bounded metadata research. Target assemblies never enter a CLR load context.
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

namespace Task144 {
public static class EntrypointMetadata {
 const int FileLimit=16*1024*1024, TotalLimit=32*1024*1024, RowLimit=32768;
 static Exception Fail(string code) { return new InvalidDataException("DCAENTRY_"+code); }
 static Dictionary<string,object> Obj(params object[] pairs) {
  var d=new Dictionary<string,object>();for(int i=0;i<pairs.Length;i+=2)d.Add((string)pairs[i],pairs[i+1]);return d;
 }
 static string Str(MetadataReader r,StringHandle h) { string s=r.GetString(h);if(s.Length>1024)throw Fail("STRING_LIMIT");return s; }
 static string Hash(byte[] b) { return Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant(); }
 static byte[] Pinned(string p,string hash,ref long total) {
  if(hash==null||hash.Length!=64)throw Fail("HASH_FORMAT");foreach(char c in hash)if(!Uri.IsHexDigit(c))throw Fail("HASH_FORMAT");
  if(!Path.IsPathFullyQualified(p)||p.StartsWith("\\\\")||p.StartsWith("//"))throw Fail("LOCAL_PATH");
  var f=new FileInfo(p);if(!f.Exists||f.Length<=0||f.Length>FileLimit||(f.Attributes&FileAttributes.ReparsePoint)!=0)throw Fail("INPUT_FILE");
  total=checked(total+f.Length);if(total>TotalLimit)throw Fail("TOTAL_LIMIT");
  byte[] b=File.ReadAllBytes(p);if(b.Length!=f.Length||Hash(b)!=hash.ToLowerInvariant())throw Fail("INPUT_HASH");return b;
 }
 static string Identity(MetadataReader r,StringHandle name,Version version,StringHandle culture,BlobHandle keyHandle,bool fullKey) {
  byte[] key=r.GetBlobBytes(keyHandle);if(key.Length>16384||(!fullKey&&key.Length!=0&&key.Length!=8))throw Fail("KEY_LIMIT");string token="null";
  if(key.Length>0){if(fullKey){byte[] digest=SHA1.HashData(key),t=new byte[8];for(int i=0;i<8;i++)t[i]=digest[digest.Length-1-i];token=Convert.ToHexString(t).ToLowerInvariant();}else token=Convert.ToHexString(key).ToLowerInvariant();}
  return Str(r,name)+", Version="+version+", Culture="+(culture.IsNil?"neutral":Str(r,culture))+", PublicKeyToken="+token;
 }
 static string AssemblyIdentity(MetadataReader r) {var a=r.GetAssemblyDefinition();return Identity(r,a.Name,a.Version,a.Culture,a.PublicKey,true);}
 static string ReferenceIdentity(MetadataReader r,AssemblyReferenceHandle h) {var a=r.GetAssemblyReference(h);return Identity(r,a.Name,a.Version,a.Culture,a.PublicKeyOrToken,(a.Flags&AssemblyFlags.PublicKey)!=0);}
 sealed class Names : ISignatureTypeProvider<string,object> {
  readonly MetadataReader r;int nodes,scopeDepth;
  public Names(MetadataReader reader){r=reader;}
  string Bound(string s){if(++nodes>128||s.Length>4096)throw Fail("SIGNATURE_LIMIT");return s;}
  string Atom(string ns,string name){if(name.Contains('.')||name.Contains('+')||ns.Contains('+'))throw Fail("TYPE_IDENTIFIER");return ns.Length==0?name:ns+"."+name;}
  public string Named(EntityHandle h,int depth=0){
   if(depth>=16)throw Fail("SCOPE_LIMIT");if(h.IsNil)return null;
   if(h.Kind==HandleKind.TypeDefinition){var t=r.GetTypeDefinition((TypeDefinitionHandle)h);var p=t.GetDeclaringType();string ns=Str(r,t.Namespace),name=Str(r,t.Name);if(!p.IsNil&&ns.Length!=0)throw Fail("TYPE_IDENTIFIER");return Bound(p.IsNil?Atom(ns,name):Named(p,depth+1)+"+"+Atom("",name));}
   if(h.Kind==HandleKind.TypeReference){var t=r.GetTypeReference((TypeReferenceHandle)h);var s=t.ResolutionScope;string ns=Str(r,t.Namespace),name=Str(r,t.Name);if(s.Kind==HandleKind.TypeReference){if(ns.Length!=0)throw Fail("TYPE_IDENTIFIER");return Bound(Named(s,depth+1)+"+"+Atom("",name));}string owner=s.Kind==HandleKind.AssemblyReference?ReferenceIdentity(r,(AssemblyReferenceHandle)s):s.Kind.ToString();return Bound("["+owner+"]"+Atom(ns,name));}
   if(h.Kind==HandleKind.TypeSpecification)return GetTypeFromSpecification(r,null,(TypeSpecificationHandle)h,0);
   throw Fail("TYPE_HANDLE");
  }
  int checkedNodes;
  void CheckType(ref BlobReader b,int depth,bool allowVoid=false){
   if(depth>16||++checkedNodes>128)throw Fail("SIGNATURE_LIMIT");
   if(b.RemainingBytes==0)throw Fail("MALFORMED_SIGNATURE");var c=b.ReadSignatureTypeCode();
   switch(c){
   case SignatureTypeCode.TypeHandle:b.ReadTypeHandle();return;
   case SignatureTypeCode.SZArray:case SignatureTypeCode.ByReference:case SignatureTypeCode.Pointer:CheckType(ref b,depth+1);return;
   case SignatureTypeCode.GenericTypeParameter:case SignatureTypeCode.GenericMethodParameter:if(b.ReadCompressedInteger()>32)throw Fail("SIGNATURE_LIMIT");return;
   case SignatureTypeCode.RequiredModifier:case SignatureTypeCode.OptionalModifier:b.ReadTypeHandle();CheckType(ref b,depth+1,allowVoid);return;
   case SignatureTypeCode.GenericTypeInstance:
    int kind=b.ReadCompressedInteger();if(kind!=(int)SignatureTypeKind.Class&&kind!=(int)SignatureTypeKind.ValueType)throw Fail("SIGNATURE_SHAPE");b.ReadTypeHandle();
    int count=b.ReadCompressedInteger();if(count<1||count>32)throw Fail("SIGNATURE_LIMIT");for(int i=0;i<count;i++)CheckType(ref b,depth+1);return;
   case SignatureTypeCode.Void:if(!allowVoid)throw Fail("SIGNATURE_SHAPE");return;
   case SignatureTypeCode.Boolean:case SignatureTypeCode.Char:case SignatureTypeCode.SByte:case SignatureTypeCode.Byte:case SignatureTypeCode.Int16:case SignatureTypeCode.UInt16:case SignatureTypeCode.Int32:case SignatureTypeCode.UInt32:case SignatureTypeCode.Int64:case SignatureTypeCode.UInt64:case SignatureTypeCode.Single:case SignatureTypeCode.Double:case SignatureTypeCode.IntPtr:case SignatureTypeCode.UIntPtr:case SignatureTypeCode.Object:case SignatureTypeCode.String:return;
   default:throw Fail("UNSUPPORTED_SIGNATURE");
   }
  }
  public string Method(BlobHandle h){
   nodes=0;checkedNodes=0;var b=r.GetBlobReader(h);if(b.Length>128)throw Fail("SIGNATURE_LIMIT");
   var pre=b;var header=pre.ReadSignatureHeader();if(header.Kind!=SignatureKind.Method||header.CallingConvention!=SignatureCallingConvention.Default)throw Fail("SIGNATURE_SHAPE");
   if(header.IsGeneric&&pre.ReadCompressedInteger()>32)throw Fail("SIGNATURE_LIMIT");
   int count=pre.ReadCompressedInteger();if(count<0||count>32)throw Fail("SIGNATURE_LIMIT");
   CheckType(ref pre,0,true);for(int i=0;i<count;i++)CheckType(ref pre,0);if(pre.RemainingBytes!=0)throw Fail("SIGNATURE_SHAPE");
   var d=new SignatureDecoder<string,object>(this,r,null);var s=d.DecodeMethodSignature(ref b);
   return Bound((s.Header.IsInstance?"instance ":"static ")+s.ReturnType+"("+string.Join(",",s.ParameterTypes)+") generic="+s.GenericParameterCount);
  }
  public string GetArrayType(string t,ArrayShape a){throw Fail("UNSUPPORTED_ARRAY");}
  public string GetByReferenceType(string t){return Bound(t+"&");}
  public string GetFunctionPointerType(MethodSignature<string> s){throw Fail("UNSUPPORTED_FUNCTION_POINTER");}
  public string GetGenericInstantiation(string t,ImmutableArray<string> args){return Bound(t+"<"+string.Join(",",args)+">");}
  public string GetGenericMethodParameter(object c,int i){return Bound("!!"+i);}
  public string GetGenericTypeParameter(object c,int i){return Bound("!"+i);}
  public string GetModifiedType(string m,string t,bool required){return Bound((required?"modreq":"modopt")+"("+m+")"+t);}
  public string GetPinnedType(string t){throw Fail("UNSUPPORTED_PINNED");}
  public string GetPointerType(string t){return Bound(t+"*");}
  public string GetPrimitiveType(PrimitiveTypeCode c){return Bound(c.ToString());}
  public string GetSZArrayType(string t){return Bound(t+"[]");}
  public string GetTypeFromDefinition(MetadataReader reader,TypeDefinitionHandle h,byte kind){return Named(h);}
  public string GetTypeFromReference(MetadataReader reader,TypeReferenceHandle h,byte kind){return Named(h);}
  public string GetTypeFromSpecification(MetadataReader reader,object c,TypeSpecificationHandle h,byte kind){
   if(++scopeDepth>16)throw Fail("SCOPE_LIMIT");try{var b=r.GetBlobReader(r.GetTypeSpecification(h).Signature);if(b.Length>128)throw Fail("SIGNATURE_LIMIT");var pre=b;CheckType(ref pre,0);if(pre.RemainingBytes!=0)throw Fail("SIGNATURE_SHAPE");var d=new SignatureDecoder<string,object>(this,r,null);string t=d.DecodeType(ref b);if(b.RemainingBytes!=0)throw Fail("SIGNATURE_SHAPE");return t;}finally{scopeDepth--;}
  }
 }
 static bool Visible(MetadataReader r,TypeDefinitionHandle h,int depth=0){if(depth>=16)throw Fail("SCOPE_LIMIT");var t=r.GetTypeDefinition(h);var v=t.Attributes&TypeAttributes.VisibilityMask;return v==TypeAttributes.Public||(v==TypeAttributes.NestedPublic&&Visible(r,t.GetDeclaringType(),depth+1));}
 static readonly HashSet<string> Contracts=new HashSet<string>(StringComparer.Ordinal){"Knx.Ets.Sdk.AddIns.AddInViews.IDeviceConfigurationAddIn","Knx.Ets.Sdk.AddIns.AddInViews.IEts4AddInV2","Knx.Ets.Sdk.AddIns.AddInViews.IEts4AddIn","Knx.Ets.Sdk.AddIns.AddInViews.IInitializationContext","Knx.Ets.Sdk.AddIns.AddInViews.IHostNotification"};
 public static string Read(string dca,string hash,string[] refs,string[] hashes){
  if(refs==null||hashes==null||refs.Length<1||refs.Length>2||refs.Length!=hashes.Length)throw Fail("INPUT_COUNT");
  var paths=new List<string>{dca};paths.AddRange(refs);var pins=new List<string>{hash};pins.AddRange(hashes);var bytes=new List<byte[]>();long total=0;
  // Verify every selected file before creating any reader.
  for(int i=0;i<paths.Count;i++)bytes.Add(Pinned(paths[i],pins[i],ref total));
  var inputs=new List<object>();var types=new List<object>();int methods=0;
  for(int i=0;i<paths.Count;i++)using(var pe=new PEReader(ImmutableArray.Create(bytes[i]))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex)))if(r.GetTableRowCount(table)>RowLimit)throw Fail("ROW_LIMIT");
   inputs.Add(Obj("filename",Path.GetFileName(paths[i]),"bytes",bytes[i].Length,"sha256",pins[i].ToLowerInvariant(),"identity",AssemblyIdentity(r)));
   foreach(var h in r.TypeDefinitions){
    var t=r.GetTypeDefinition(h);var n=new Names(r);string name=n.Named(h);if(!(i==0?Visible(r,h):Contracts.Contains(name)))continue;
    if(types.Count>=1024)throw Fail("TYPE_LIMIT");var interfaces=new List<string>();
    foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r).Named(r.GetInterfaceImplementation(ih).Interface));}
    var declared=new List<object>();
    bool contractCandidate=i!=0||interfaces.Exists(x=>x.StartsWith("[Knx.Ets.Sdk.AddIns.AddInViews, Version=",StringComparison.Ordinal));
    foreach(var mh in t.GetMethods()){
     if(!contractCandidate)continue;
     var m=r.GetMethodDefinition(mh);string mn=Str(r,m.Name);bool pub=(m.Attributes&MethodAttributes.MemberAccessMask)==MethodAttributes.Public;
     if(!pub&&(m.Attributes&MethodAttributes.Virtual)==0)continue;if(++methods>2048||declared.Count>=256)throw Fail("METHOD_LIMIT");
     string signature;try{signature=new Names(r).Method(m.Signature);}catch(InvalidDataException){throw;}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     declared.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",mn,"attributes",(int)m.Attributes,"signature_header",r.GetBlobReader(m.Signature).ReadByte(),"signature",signature));
    }
    types.Add(Obj("input",Path.GetFileName(paths[i]),"token",MetadataTokens.GetToken(h).ToString("X8"),"name",name,"attributes",(int)t.Attributes,"base_type",new Names(r).Named(t.BaseType),"interfaces",interfaces,"methods",declared));
   }
  }
  string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_entrypoint_map","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",inputs,"types",types,"limitations",new[]{"DCA method details only for direct AddInViews interface implementers; public type inventory and selected contract declarations only; no inherited member or runtime activation proof.","Names/signatures are metadata observations; no SDK implementation or method bodies inspected."}));
  if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
 }
}
}
