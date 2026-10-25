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
 const int ConstructionObjectModelMethodLimit=65536;
 public sealed class RowLimitObservationException : Exception {
  public string TableName { get; }
  public int RowCount { get; }
  internal RowLimitObservationException(TableIndex table,int count):base("DCAENTRY_ROW_LIMIT"){
   TableName=table.ToString().ToUpperInvariant();RowCount=count;
  }
 }
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
  readonly MetadataReader r;readonly bool strictIdentifiers;int nodes,scopeDepth;
  public string ReturnType { get; private set; }
  public Names(MetadataReader reader,bool strict=false){r=reader;strictIdentifiers=strict;}
  string Bound(string s){if(++nodes>128||s.Length>4096)throw Fail("SIGNATURE_LIMIT");return s;}
  string Atom(string ns,string name){if(name.Contains('.')||name.Contains('+')||ns.Contains('+')||(strictIdentifiers&&(name.IndexOfAny("[]<>,&*()".ToCharArray())>=0||ns.IndexOfAny("[]<>,&*()".ToCharArray())>=0)))throw Fail("TYPE_IDENTIFIER");return ns.Length==0?name:ns+"."+name;}
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
   ReturnType=s.ReturnType;
   return Bound((s.Header.IsInstance?"instance ":"static ")+s.ReturnType+"("+string.Join(",",s.ParameterTypes)+") generic="+s.GenericParameterCount);
  }
  public string Field(BlobHandle h){
   nodes=0;checkedNodes=0;var b=r.GetBlobReader(h);if(b.Length>128)throw Fail("SIGNATURE_LIMIT");
   var pre=b;if(pre.ReadByte()!=6)throw Fail("SIGNATURE_SHAPE");
   CheckType(ref pre,0);if(pre.RemainingBytes!=0)throw Fail("SIGNATURE_SHAPE");
   var d=new SignatureDecoder<string,object>(this,r,null);return Bound(d.DecodeFieldSignature(ref b));
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
 static readonly string[] SdkRoots={"Knx.Ets.Sdk.Project.Project","Knx.Ets.Sdk.Project.ProjectCollection","Knx.Ets.Sdk.ISdkContext","Knx.Ets.Sdk.Root"};
 static readonly string[] ObjectModelRoots={"Knx.Ets.ObjectModel.Project.Project"};
 static readonly string[] SdkServiceRoots={"Knx.Ets.Sdk.ExecutionContext","Knx.Ets.Sdk.ISdkFactory"};
 static readonly string[] ObjectModelServiceRoots={"Knx.Ets.ObjectModel.IFactory","Knx.Ets.ObjectModel.IProjectStore","Knx.Ets.ObjectModel.IObjectContext"};
 static readonly string[] SdkProviderTargets={"Knx.Ets.Sdk.ExecutionContext","Knx.Ets.Sdk.ISdkFactory","Knx.Ets.Sdk.ISdkContext"};
 static readonly string[] SdkConstructorRoots={"Knx.Ets.Sdk.DcaExecutionContext","Knx.Ets.Sdk.Context","Knx.Ets.Sdk.SdkFactory"};
 static readonly string[] ObjectModelConstructorRoots={"Knx.Ets.ObjectModel.Factory","Knx.Ets.ObjectModel.ObjectContext"};
 static string[] Roots(string assembly,bool services=false,bool constructors=false){return assembly=="Knx.Ets.Sdk"?(constructors?SdkConstructorRoots:services?SdkServiceRoots:SdkRoots):assembly=="Knx.Ets.ObjectModel"?(constructors?ObjectModelConstructorRoots:services?ObjectModelServiceRoots:ObjectModelRoots):Array.Empty<string>();}
 static bool ReturnContains(string returns,string atom){
  int start=0;while(start<returns.Length){int at=returns.IndexOf(atom,start,StringComparison.Ordinal);if(at<0)return false;int end=at+atom.Length;
   if((at==0||returns[at-1]=='<'||returns[at-1]==',')&&(end==returns.Length||returns[end]=='>'||returns[end]==','||returns[end]=='['||returns[end]=='&'||returns[end]=='*'))return true;start=at+1;
  }return false;
 }
 static List<string> ReturnAtoms(MetadataReader r,bool services){
  var atoms=new List<string>(Roots(Str(r,r.GetAssemblyDefinition().Name),services));
  foreach(var h in r.AssemblyReferences){var a=r.GetAssemblyReference(h);foreach(string root in Roots(Str(r,a.Name),services))atoms.Add("["+ReferenceIdentity(r,h)+"]"+root);}
  return atoms;
 }
 static bool FactoryCandidate(List<string> atoms,string returns){
  foreach(string atom in atoms)if(ReturnContains(returns,atom))return true;return false;
 }
 static string SelectChain(MetadataReader r,TypeDefinitionHandle root,HashSet<TypeDefinitionHandle> selected){
  var chain=new HashSet<TypeDefinitionHandle>();var current=root;
  while(true){
   if(chain.Contains(current))throw Fail("BASE_CYCLE");if(chain.Count>=16)throw Fail("BASE_LIMIT");chain.Add(current);selected.Add(current);
   var b=r.GetTypeDefinition(current).BaseType;
   if(b.IsNil)return "NO_BASE";
   if(b.Kind==HandleKind.TypeDefinition){current=(TypeDefinitionHandle)b;continue;}
   if(b.Kind==HandleKind.TypeReference){var tr=r.GetTypeReference((TypeReferenceHandle)b);if(tr.ResolutionScope.Kind!=HandleKind.AssemblyReference)throw Fail("BASE_SCOPE");return "EXTERNAL_BASE_NOT_RESOLVED";}
   throw Fail("BASE_SHAPE");
  }
 }
 static void CheckRows(MetadataReader r,bool fourInputs,int index){
  bool objectModelSlot=fourInputs&&index==3&&Str(r,r.GetAssemblyDefinition().Name)=="Knx.Ets.ObjectModel";
  foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);int limit=objectModelSlot&&table==TableIndex.MethodDef?ConstructionObjectModelMethodLimit:RowLimit;if(count>limit){if(fourInputs)throw new RowLimitObservationException(table,count);throw Fail("ROW_LIMIT");}}
 }
 sealed class ProviderTarget {public string Assembly,Name,Token;}
 static void Uncertain(Dictionary<string,int> counts,string reason){counts.TryGetValue(reason,out int count);counts[reason]=checked(count+1);}
 static ProviderTarget DirectTarget(MetadataReader r,string inputIdentity,EntityHandle edge,Dictionary<string,ProviderTarget> targets,Dictionary<string,int> uncertainty){
  if(edge.IsNil)return null;
  string name,owner;
  if(edge.Kind==HandleKind.TypeDefinition){name=new Names(r).Named(edge);owner=inputIdentity;}
  else if(edge.Kind==HandleKind.TypeReference){
   var reference=r.GetTypeReference((TypeReferenceHandle)edge);
   if(reference.ResolutionScope.Kind!=HandleKind.AssemblyReference){Uncertain(uncertainty,"UNSUPPORTED_REFERENCE_SCOPE");return null;}
   owner=ReferenceIdentity(r,(AssemblyReferenceHandle)reference.ResolutionScope);
   string qualified=new Names(r).Named(edge);name=qualified.Substring(owner.Length+2);
  }else{Uncertain(uncertainty,edge.Kind==HandleKind.TypeSpecification?"TYPESPEC_NOT_INSPECTED":"UNSUPPORTED_EDGE_KIND");return null;}
  if(!targets.TryGetValue(name,out var target))return null;
  if(owner!=target.Assembly){Uncertain(uncertainty,"TARGET_NAME_OWNER_MISMATCH");return null;}
  return target;
 }
 public static string ReadProviders(string dca,string hash,string[] refs,string[] hashes){
  if(refs==null||hashes==null||refs.Length!=3||hashes.Length!=3)throw Fail("INPUT_COUNT");
  var paths=new List<string>{dca};paths.AddRange(refs);var pins=new List<string>{hash};pins.AddRange(hashes);var bytes=new List<byte[]>();long total=0;
  for(int i=0;i<paths.Count;i++)bytes.Add(Pinned(paths[i],pins[i],ref total));
  var readers=new List<PEReader>();var metadata=new List<MetadataReader>();var identities=new List<string>();var assemblyNames=new List<string>();var inputs=new List<object>();
  try{
   for(int i=0;i<paths.Count;i++){
    var pe=new PEReader(ImmutableArray.Create(bytes[i]));readers.Add(pe);if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");CheckRows(r,true,i);
    string identity=AssemblyIdentity(r);metadata.Add(r);identities.Add(identity);assemblyNames.Add(Str(r,r.GetAssemblyDefinition().Name));
    inputs.Add(Obj("filename",Path.GetFileName(paths[i]),"bytes",bytes[i].Length,"sha256",pins[i].ToLowerInvariant(),"identity",identity));
   }
   var owners=new HashSet<string>(StringComparer.Ordinal);var targets=new Dictionary<string,ProviderTarget>(StringComparer.Ordinal);var targetReports=new List<object>();
   for(int i=0;i<metadata.Count;i++){
    string owner=assemblyNames[i];if(owner!="Knx.Ets.Sdk"&&owner!="Knx.Ets.ObjectModel")continue;if(!owners.Add(owner))throw Fail("DUPLICATE_PROVIDER_OWNER");
    string[] selected=owner=="Knx.Ets.Sdk"?SdkProviderTargets:ObjectModelServiceRoots;var r=metadata[i];
    foreach(var h in r.TypeDefinitions){string name=new Names(r).Named(h);if(Array.IndexOf(selected,name)<0)continue;
     var target=new ProviderTarget{Assembly=identities[i],Name=name,Token=MetadataTokens.GetToken(h).ToString("X8")};
     if(!targets.TryAdd(name,target))throw Fail("DUPLICATE_PROVIDER_TARGET");targetReports.Add(Obj("assembly",target.Assembly,"name",name,"token",target.Token));
    }
   }
   if(targets.Count!=6)throw Fail("PROVIDER_TARGET_NOT_FOUND");
   var candidates=new List<object>();var uncertainty=new Dictionary<string,int>(StringComparer.Ordinal);int edgeCount=0,work=0;
   for(int i=0;i<metadata.Count;i++){
    var r=metadata[i];foreach(var h in r.TypeDefinitions){
     var type=r.GetTypeDefinition(h);var matches=new List<object>();
     void Edge(EntityHandle edge,string kind){
      if(edge.IsNil)return;if(++work>262144)throw Fail("PROVIDER_WORK_LIMIT");var target=DirectTarget(r,identities[i],edge,targets,uncertainty);if(target==null)return;
      if(++edgeCount>128)throw Fail("PROVIDER_EDGE_LIMIT");matches.Add(Obj("kind",kind,"target_assembly",target.Assembly,"target_name",target.Name,"target_token",target.Token));
     }
     Edge(type.BaseType,"extends");int interfaces=0;foreach(var ih in type.GetInterfaceImplementations()){if(++interfaces>32)throw Fail("INTERFACE_LIMIT");Edge(r.GetInterfaceImplementation(ih).Interface,"implements");}
     if(matches.Count==0)continue;if(candidates.Count>=64)throw Fail("PROVIDER_TYPE_LIMIT");
     candidates.Add(Obj("input",Path.GetFileName(paths[i]),"assembly",identities[i],"token",MetadataTokens.GetToken(h).ToString("X8"),"name",new Names(r).Named(h),"attributes",(int)type.Attributes,"relationships",matches));
    }
   }
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_providers","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",inputs,"targets",targetReports,"candidates",candidates,"matching_edges",edgeCount,"uncertainty_counts",uncertainty,"creation_route","INCONCLUSIVE","limitations",new[]{"Only direct base/interface matches to six exact selected target owners; indirect inheritance, external implementations and TypeSpec/generic edges are not searched. Empty matches do not prove absence.","Candidate relationships and raw attributes do not establish concrete construction, public accessibility or usable services. No methods, constructors, method bodies or custom attributes inspected; no dependency resolution or vendor execution."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }finally{foreach(var reader in readers)reader.Dispose();}
 }
 public static string Read(string dca,string hash,string[] refs,string[] hashes){
  return ReadInternal(dca,hash,refs,hashes,false);
 }
 public static string ReadProject(string dca,string hash,string[] refs,string[] hashes){
  return ReadInternal(dca,hash,refs,hashes,true);
 }
 public static string ReadConstruction(string dca,string hash,string[] refs,string[] hashes){
  return ReadInternal(dca,hash,refs,hashes,true,true);
 }
 public static string ReadServices(string dca,string hash,string[] refs,string[] hashes){
  return ReadInternal(dca,hash,refs,hashes,true,true,true);
 }
 public static string ReadConstructors(string dca,string hash,string[] refs,string[] hashes){
  return ReadInternal(dca,hash,refs,hashes,true,true,false,true);
 }
 const string ProjectIdHash="43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877";
 const string ProjectIdIdentity="Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string OwnedProjectIdIdentity="Task009.OwnedProjectId, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string ProjectIdRoot="Knx.Ets.Ids.Project.ProjectId";
 public static string ReadProjectId(string path,string hash){
  if(hash!=ProjectIdHash)throw Fail("INPUT_PIN");
  return ReadProjectIdInternal(path,hash,false);
 }
 // Explicit synthetic route: rejects original identities and cannot be selected by the worker.
 public static string ReadOwnedProjectId(string path,string hash){return ReadProjectIdInternal(path,hash,true);}
 static string ReadProjectIdInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);
  if(!owned&&bytes.Length!=318928)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   CheckRows(r,false,0);string identity=AssemblyIdentity(r);
   if(identity!=(owned?OwnedProjectIdIdentity:ProjectIdIdentity))throw Fail("INPUT_IDENTITY");
   TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==ProjectIdRoot){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND");
   var selected=new HashSet<TypeDefinitionHandle>();var current=root;string disposition;
   while(true){
    if(!selected.Add(current))throw Fail("BASE_CYCLE");if(selected.Count>16)throw Fail("BASE_LIMIT");
    var b=r.GetTypeDefinition(current).BaseType;if(b.IsNil){disposition="NO_BASE";break;}
    if(b.Kind==HandleKind.TypeDefinition){current=(TypeDefinitionHandle)b;continue;}
    if(b.Kind==HandleKind.TypeReference){disposition="EXTERNAL_BASE_NOT_RESOLVED";break;}
    if(b.Kind==HandleKind.TypeSpecification){disposition="TYPESPEC_BASE_NOT_FOLLOWED";break;}
    throw Fail("BASE_SHAPE");
   }
   var types=new List<object>();int methods=0,fields=0;
   foreach(var h in r.TypeDefinitions){
    if(!selected.Contains(h))continue;if(types.Count>=1024)throw Fail("TYPE_LIMIT");var t=r.GetTypeDefinition(h);
    var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));}
    var declared=new List<object>();foreach(var mh in t.GetMethods()){
     var m=r.GetMethodDefinition(mh);string name=Str(r,m.Name);var access=m.Attributes&MethodAttributes.MemberAccessMask;bool constructor=name==".ctor"||name==".cctor";
     if(!constructor&&access!=MethodAttributes.Public&&access!=MethodAttributes.Family&&access!=MethodAttributes.FamORAssem&&access!=MethodAttributes.FamANDAssem)continue;
     if(++methods>2048||declared.Count>=256)throw Fail("METHOD_LIMIT");string signature;
     try{signature=new Names(r,true).Method(m.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     declared.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",name,"attributes",(int)m.Attributes,"accessibility",access.ToString(),"signature",signature));
    }
    var fieldDeclarations=new List<object>();foreach(var fh in t.GetFields()){
     if(++fields>2048||fieldDeclarations.Count>=256)throw Fail("FIELD_LIMIT");var f=r.GetFieldDefinition(fh);string signature;
     try{signature=new Names(r,true).Field(f.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     fieldDeclarations.Add(Obj("token",MetadataTokens.GetToken(fh).ToString("X8"),"name",Str(r,f.Name),"attributes",(int)f.Attributes,"signature",signature));
    }
    types.Add(Obj("token",MetadataTokens.GetToken(h).ToString("X8"),"name",new Names(r,true).Named(h),"attributes",(int)t.Attributes,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"methods",declared,"fields",fieldDeclarations));
   }
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_id","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",ProjectIdRoot,"base_chain_disposition",disposition,"types",types,"limitations",new[]{"Fixed ProjectId and bounded same-assembly bases only; TypeSpec ancestry is not followed, external declarations remain unresolved.","Constructor/method/field declarations only; no field values, custom attributes, bodies, dependency resolution or target execution. No ProjectId value or working service is supplied."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 // Fixed declaration-only assembly-reference map; no candidate assembly is required or resolved.
 const string OnlineCatalogInputHash="73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a";
 const string OnlineCatalogInputIdentity="Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string OnlineCatalogTarget="Knx.Ets.OnlineCatalog.Download, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string OwnedOnlineCatalogInput="Task009.OwnedOnlineCatalogReferences, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedOnlineCatalogTarget="Owned.OnlineCatalog.Download, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 public static string ReadProjectOnlineCatalogReferences(string path,string hash){
  if(hash!=OnlineCatalogInputHash)throw Fail("INPUT_PIN");return ReadOnlineCatalogReferences(path,hash,false);
 }
 public static string ReadOwnedProjectOnlineCatalogReferences(string path,string hash){return ReadOnlineCatalogReferences(path,hash,true);}
 static AssemblyReferenceHandle OnlineCatalogScope(MetadataReader r,TypeReferenceHandle start,out List<string> chain,out string uncertainty){
  chain=new List<string>();uncertainty=null;var seen=new HashSet<TypeReferenceHandle>();var current=start;
  while(true){
   if(current.IsNil||MetadataTokens.GetRowNumber(current)>r.TypeReferences.Count)throw Fail("TYPE_SCOPE_INVALID");if(!seen.Add(current))throw Fail("TYPE_SCOPE_CYCLE");
   if(chain.Count>=16)throw Fail("SCOPE_LIMIT");chain.Add(MetadataTokens.GetToken(current).ToString("X8"));
   TypeReference t;try{t=r.GetTypeReference(current);}catch(BadImageFormatException){throw Fail("TYPE_SCOPE_INVALID");}
   var scope=t.ResolutionScope;if(scope.IsNil){uncertainty="NIL_SCOPE_UNRESOLVED";return default;}
   if(scope.Kind==HandleKind.AssemblyReference){if(MetadataTokens.GetRowNumber(scope)>r.AssemblyReferences.Count)throw Fail("TYPE_SCOPE_INVALID");chain.Add(MetadataTokens.GetToken(scope).ToString("X8"));return (AssemblyReferenceHandle)scope;}
   if(scope.Kind==HandleKind.TypeReference){if(Str(r,t.Namespace).Length!=0)throw Fail("TYPE_SCOPE_NAMESPACE");current=(TypeReferenceHandle)scope;continue;}
   uncertainty=scope.Kind.ToString()+"_UNRESOLVED_NOT_FOLLOWED";return default;
  }
 }
 static string ReadOnlineCatalogReferences(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=14204880)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);if(count>(table==TableIndex.MethodDef?65536:RowLimit))throw new RowLimitObservationException(table,count);}
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedOnlineCatalogInput:OnlineCatalogInputIdentity))throw Fail("INPUT_IDENTITY");
   if(r.AssemblyReferences.Count>256)throw Fail("REFERENCE_LIMIT");string target=owned?OwnedOnlineCatalogTarget:OnlineCatalogTarget;AssemblyReferenceHandle selected=default;
   foreach(var h in r.AssemblyReferences)if(ReferenceIdentity(r,h)==target){if(!selected.IsNil)throw Fail("DUPLICATE_ASSEMBLY_REFERENCE");selected=h;}
   if(selected.IsNil)throw Fail("ASSEMBLY_REFERENCE_NOT_FOUND");
   var selectedTypes=new HashSet<TypeReferenceHandle>();var types=new List<object>();var uncertainties=new Dictionary<string,int>();int nonTarget=0;
   foreach(var h in r.TypeReferences){
    List<string> chain;string uncertainty;var owner=OnlineCatalogScope(r,h,out chain,out uncertainty);
    if(uncertainty!=null){if(!uncertainties.ContainsKey(uncertainty))uncertainties.Add(uncertainty,0);uncertainties[uncertainty]++;continue;}
    if(owner!=selected){nonTarget++;continue;}if(types.Count>=256)throw Fail("TYPE_REFERENCE_LIMIT");
    var t=r.GetTypeReference(h);string display=new Names(r,true).Named(h);selectedTypes.Add(h);
    types.Add(Obj("token",MetadataTokens.GetToken(h).ToString("X8"),"namespace",Str(r,t.Namespace),"name",Str(r,t.Name),"scope_kind",t.ResolutionScope.Kind.ToString(),"scope_token",MetadataTokens.GetToken(t.ResolutionScope).ToString("X8"),"scope_chain",chain,"qualified_name",display));
   }
   var members=new List<object>();var otherParents=new Dictionary<string,int>();int typeSpecParents=0,nonTargetMembers=0;
   foreach(var h in r.MemberReferences){
    var m=r.GetMemberReference(h);
    if(m.Parent.Kind==HandleKind.TypeSpecification){typeSpecParents++;continue;}
    if(m.Parent.Kind!=HandleKind.TypeReference){string kind=m.Parent.Kind.ToString();if(!otherParents.ContainsKey(kind))otherParents.Add(kind,0);otherParents[kind]++;continue;}
    if(!selectedTypes.Contains((TypeReferenceHandle)m.Parent)){nonTargetMembers++;continue;}
    if(members.Count>=256)throw Fail("MEMBER_REFERENCE_LIMIT");string name=Str(r,m.Name),signature;var item=Obj("token",MetadataTokens.GetToken(h).ToString("X8"),"parent_kind",m.Parent.Kind.ToString(),"parent_token",MetadataTokens.GetToken(m.Parent).ToString("X8"),"name",name);
    try{
     var kind=m.GetKind();item.Add("kind",kind.ToString());
     if(kind==MemberReferenceKind.Method){
      signature=new Names(r,true).Method(m.Signature);var decoded=m.DecodeMethodSignature(new Names(r,true),(object)null);
      item.Add("signature_header",decoded.Header.RawValue);item.Add("calling_convention",decoded.Header.CallingConvention.ToString());item.Add("is_instance",decoded.Header.IsInstance);item.Add("generic_arity",decoded.GenericParameterCount);item.Add("return_type",decoded.ReturnType);item.Add("parameter_types",decoded.ParameterTypes);
     }else if(kind==MemberReferenceKind.Field){signature=new Names(r,true).Field(m.Signature);item.Add("field_type",signature);}else throw Fail("UNSUPPORTED_SIGNATURE");
    }catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
    item.Add("signature",signature);members.Add(item);
   }
   var assembly=r.GetAssemblyReference(selected);
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_online_catalog_references","rights","UNRESOLVED","runtime_execution","NOT_RUN",
    "inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},
    "selected_assembly_reference",target,"assembly_reference",Obj("token",MetadataTokens.GetToken(selected).ToString("X8"),"identity",target,"flags",(int)assembly.Flags),
    "type_references",types,"member_references",members,"counts",Obj("assembly_references",r.AssemblyReferences.Count,"type_references_scanned",r.TypeReferences.Count,"selected_type_references",types.Count,"non_target_type_references",nonTarget,"unresolved_type_scopes",uncertainties,"member_references_scanned",r.MemberReferences.Count,"selected_member_references",members.Count,"non_selected_typeref_parent_members",nonTargetMembers,"global_unexamined_typespec_parent_members",typeSpecParents,"other_unresolved_parent_kinds",otherParents),
    "limits",Obj("file_bytes",FileLimit,"total_bytes",TotalLimit,"methoddef_rows",65536,"other_table_rows",RowLimit,"assembly_references",256,"selected_type_references",256,"selected_member_references",256,"string_chars",1024,"signature_blob_bytes",128,"signature_chars",4096,"signature_nodes",128,"depth",16,"parameters",32,"generic_count",32,"report_utf8_bytes",512*1024),
    "declaration_scope","GLOBAL_OBJECTMODEL_DIRECT_TYPEREF_PARENT_REFERENCES_ONLY","referenced_declarations","UNRESOLVED_NOT_FOLLOWED","typespec_parent_scope","GLOBAL_NOT_TARGET_SPECIFIC_UNEXAMINED_TYPESPEC_PARENT","qualification","DECLARATIONS_ONLY_NOT_CALLSITE_OR_COMPLETENESS_PROOF",
    "limitations",new[]{"Global module references only, not a Project constructor call map. Empty selections do not prove absence; TypeSpec-parent omission counts are global, not target-specific.","Only token-proven TypeRef scope and immediate selected TypeRef MemberRef parents. Other parents, forwarding, MethodSpec use, external declarations, inheritance and call sites remain unexamined.","Signatures only, including any constructor reference name; no constructor definitions, IL, attributes, resources, values, candidate DLL, dependency resolution, target execution or dependency admission."}));
   if(System.Text.Encoding.UTF8.GetByteCount(report)>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 // Fixed ProjectCollection declarations only; no activation or ancestry/factory graph traversal.
 const string ProjectCollectionHash="73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a";
 const string ProjectCollectionIdentity="Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string ProjectCollectionRoot="Knx.Ets.ObjectModel.Project.ProjectCollection";
 const string OwnedProjectCollectionIdentity="Task009.OwnedProjectCollection, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedProjectCollectionRoot="Owned.Task009.ProjectCollectionFixture";
 public static string ReadProjectCollection(string path,string hash){
  if(hash!=ProjectCollectionHash)throw Fail("INPUT_PIN");return ReadProjectCollectionInternal(path,hash,false);
 }
 public static string ReadOwnedProjectCollection(string path,string hash){return ReadProjectCollectionInternal(path,hash,true);}
 static string ReadProjectCollectionInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=14204880)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);if(count>(table==TableIndex.MethodDef?65536:RowLimit))throw new RowLimitObservationException(table,count);}
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedProjectCollectionIdentity:ProjectCollectionIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedProjectCollectionRoot:ProjectCollectionRoot;TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND");var t=r.GetTypeDefinition(root);
   if((t.Attributes&TypeAttributes.VisibilityMask)!=TypeAttributes.Public||!t.GetDeclaringType().IsNil)throw Fail("ROOT_NOT_PUBLIC");
   int arity=t.GetGenericParameters().Count;if(arity!=0)throw Fail("UNSUPPORTED_GENERIC_ROOT");
   var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){
    if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));
   }
   var constructors=new List<object>();var methods=new List<object>();int publicConstructors=0;
   foreach(var mh in t.GetMethods()){
    var m=r.GetMethodDefinition(mh);bool constructor=r.StringComparer.Equals(m.Name,".ctor")||r.StringComparer.Equals(m.Name,".cctor");
    var access=m.Attributes&MethodAttributes.MemberAccessMask;if(!constructor&&access!=MethodAttributes.Public)continue;
    if(constructor?constructors.Count>=32:methods.Count>=256)throw Fail(constructor?"CONSTRUCTOR_LIMIT":"METHOD_LIMIT");
    string name=Str(r,m.Name),signature;MethodSignature<string> decoded;
    try{signature=new Names(r,true).Method(m.Signature);decoded=m.DecodeSignature(new Names(r,true),(object)null);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
    bool isStatic=(m.Attributes&MethodAttributes.Static)!=0;
    var declaration=Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",name,"attributes",(int)m.Attributes,"accessibility",access.ToString(),"is_static",isStatic,
     "signature_header",decoded.Header.RawValue,"calling_convention",decoded.Header.CallingConvention.ToString(),"is_instance",decoded.Header.IsInstance,"generic_arity",decoded.GenericParameterCount,"return_type",decoded.ReturnType,"parameter_types",decoded.ParameterTypes,"signature",signature);
    if(constructor){constructors.Add(declaration);if(name==".ctor"&&access==MethodAttributes.Public&&!isStatic)publicConstructors++;}else methods.Add(declaration);
   }
   var type=Obj("token",MetadataTokens.GetToken(root).ToString("X8"),"name",rootName,"attributes",(int)t.Attributes,"visibility",(t.Attributes&TypeAttributes.VisibilityMask).ToString(),"class_semantics_raw",(int)(t.Attributes&TypeAttributes.ClassSemanticsMask),"class_semantics",(t.Attributes&TypeAttributes.ClassSemanticsMask)==TypeAttributes.Interface?"Interface":"Class","is_interface",(t.Attributes&TypeAttributes.ClassSemanticsMask)==TypeAttributes.Interface,"generic_arity",arity,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"constructor_count",constructors.Count,"public_constructor_count",publicConstructors,"constructors",constructors,"methods",methods);
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_collection","rights","UNRESOLVED","runtime_execution","NOT_RUN",
    "inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,"declaration_scope","ROOT_ONLY","referenced_declarations","UNRESOLVED_NOT_FOLLOWED","types",new[]{type},"qualification","DECLARATIONS_ONLY_NOT_CONSTRUCTION_OR_OFFLINE_BEHAVIOR_PROOF",
    "limits",Obj("file_bytes",FileLimit,"aggregate_bytes",TotalLimit,"methoddef_rows",65536,"other_table_rows",RowLimit,"string_chars",1024,"signature_blob_bytes",128,"signature_chars",4096,"signature_nodes",128,"depth",16,"parameters",32,"generic_count",32,"interfaces",32,"constructors",32,"other_public_methods",256,"report_utf8_bytes",512*1024),
    "limitations",new[]{"Root declarations only. Base/interfaces, inherited members, external declarations and factories on other owners are not followed. Return shapes and constructor access are not creation/argument/behavior qualification.","public_constructor_count counts declared public nonstatic .ctor methods only; .cctor and all other constructor access/static flags remain explicit. Class semantics alone does not establish class/value-type/enum usability.","No IL, fields/values/layout, custom attributes, activation, SDK/candidate loads, dependency resolution, graph or implementation search. No inference of catalog independence from overload signatures."}));
   if(System.Text.Encoding.UTF8.GetByteCount(report)>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string ProjectStreamStoreHash="73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a";
 const string ProjectStreamStoreIdentity="Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string ProjectStreamStoreRoot="Knx.Ets.ObjectModel.IStreamStore";
 const string OwnedProjectStreamStoreIdentity="Task009.OwnedProjectStreamStore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedProjectStreamStoreRoot="Owned.Task009.IProjectStreamStoreFixture";
 const int ProjectStreamStoreMethodLimit=65536;
 public static string ReadProjectStreamStore(string path,string hash){
  if(hash!=ProjectStreamStoreHash)throw Fail("INPUT_PIN");return ReadProjectStreamStoreInternal(path,hash,false);
 }
 // A separate unsigned owned image; never selectable by the fixed original worker.
 public static string ReadOwnedProjectStreamStore(string path,string hash){return ReadProjectStreamStoreInternal(path,hash,true);}
 static string ReadProjectStreamStoreInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=14204880)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);if(count>(table==TableIndex.MethodDef?ProjectStreamStoreMethodLimit:RowLimit))throw new RowLimitObservationException(table,count);}
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedProjectStreamStoreIdentity:ProjectStreamStoreIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedProjectStreamStoreRoot:ProjectStreamStoreRoot;TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND");var t=r.GetTypeDefinition(root);
   if((t.Attributes&TypeAttributes.VisibilityMask)!=TypeAttributes.Public||!t.GetDeclaringType().IsNil)throw Fail("ROOT_NOT_PUBLIC");
   int arity=t.GetGenericParameters().Count;if(arity!=0)throw Fail("UNSUPPORTED_GENERIC_ROOT");
   var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){
    if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));
   }
   var methods=new List<object>();foreach(var mh in t.GetMethods()){
    var m=r.GetMethodDefinition(mh);if((m.Attributes&MethodAttributes.MemberAccessMask)!=MethodAttributes.Public)continue;
    if(methods.Count>=256)throw Fail("METHOD_LIMIT");string signature;MethodSignature<string> decoded;
    try{signature=new Names(r,true).Method(m.Signature);decoded=m.DecodeSignature(new Names(r,true),(object)null);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
    methods.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",Str(r,m.Name),"attributes",(int)m.Attributes,
     "signature_header",decoded.Header.RawValue,"calling_convention",decoded.Header.CallingConvention.ToString(),"is_instance",decoded.Header.IsInstance,
     "generic_arity",decoded.GenericParameterCount,"return_type",decoded.ReturnType,"parameter_types",decoded.ParameterTypes,"signature",signature));
   }
   var type=Obj("token",MetadataTokens.GetToken(root).ToString("X8"),"name",rootName,"attributes",(int)t.Attributes,"visibility",(t.Attributes&TypeAttributes.VisibilityMask).ToString(),"class_semantics_raw",(int)(t.Attributes&TypeAttributes.ClassSemanticsMask),"class_semantics",(t.Attributes&TypeAttributes.ClassSemanticsMask)==TypeAttributes.Interface?"Interface":"Class","is_interface",(t.Attributes&TypeAttributes.ClassSemanticsMask)==TypeAttributes.Interface,"generic_arity",arity,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"methods",methods);
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_stream_store","rights","UNRESOLVED","runtime_execution","NOT_RUN",
    "inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,
    "declaration_scope","ROOT_ONLY","referenced_declarations","UNRESOLVED_NOT_FOLLOWED","types",new[]{type},"qualification","DECLARATIONS_ONLY_NOT_USABLE_SERVICE",
    "limitations",new[]{"Public nongeneric root flags and public declared methods only; class semantics do not establish class/value-type/enum usability. Base and direct interface references remain unresolved; no closure or implementation search.","No fields, values, method bodies, attributes, dependencies or target execution. Method generic constraints/parameter defaults and service behavior remain unexamined."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string UndoStoreProviderHash="73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a";
 const string UndoStoreProviderIdentity="Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string UndoStoreProviderRoot="Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider";
 const string OwnedUndoStoreProviderIdentity="Task009.OwnedUndoStoreProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedUndoStoreProviderRoot="Owned.Task009.IUndoStoreProviderFixture";
 const int UndoStoreProviderMethodLimit=65536;
 public static string ReadProjectUndoStoreProvider(string path,string hash){
  if(hash!=UndoStoreProviderHash)throw Fail("INPUT_PIN");return ReadUndoStoreProviderInternal(path,hash,false);
 }
 // A separate unsigned owned image; never selectable by the fixed original worker.
 public static string ReadOwnedProjectUndoStoreProvider(string path,string hash){return ReadUndoStoreProviderInternal(path,hash,true);}
 static string ReadUndoStoreProviderInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=14204880)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);if(count>(table==TableIndex.MethodDef?UndoStoreProviderMethodLimit:RowLimit))throw new RowLimitObservationException(table,count);}
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedUndoStoreProviderIdentity:UndoStoreProviderIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedUndoStoreProviderRoot:UndoStoreProviderRoot;TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND");var t=r.GetTypeDefinition(root);
   if((t.Attributes&TypeAttributes.VisibilityMask)!=TypeAttributes.Public||!t.GetDeclaringType().IsNil)throw Fail("ROOT_NOT_PUBLIC");
   if((t.Attributes&TypeAttributes.ClassSemanticsMask)!=TypeAttributes.Interface)throw Fail("ROOT_NOT_INTERFACE");
   int arity=t.GetGenericParameters().Count;if(arity!=0)throw Fail("UNSUPPORTED_GENERIC_ROOT");
   if(!t.BaseType.IsNil)throw Fail("UNSUPPORTED_INTERFACE_BASE");
   var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){
    if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));
   }
   var methods=new List<object>();foreach(var mh in t.GetMethods()){
    var m=r.GetMethodDefinition(mh);if((m.Attributes&MethodAttributes.MemberAccessMask)!=MethodAttributes.Public)continue;
    if(methods.Count>=256)throw Fail("METHOD_LIMIT");string signature;MethodSignature<string> decoded;
    try{signature=new Names(r,true).Method(m.Signature);decoded=m.DecodeSignature(new Names(r,true),(object)null);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
    methods.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",Str(r,m.Name),"attributes",(int)m.Attributes,
     "signature_header",decoded.Header.RawValue,"calling_convention",decoded.Header.CallingConvention.ToString(),"is_instance",decoded.Header.IsInstance,
     "generic_arity",decoded.GenericParameterCount,"return_type",decoded.ReturnType,"parameter_types",decoded.ParameterTypes,"signature",signature));
   }
   var type=Obj("token",MetadataTokens.GetToken(root).ToString("X8"),"name",rootName,"attributes",(int)t.Attributes,"generic_arity",arity,"interfaces",interfaces,"methods",methods);
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_undo_store_provider","rights","UNRESOLVED","runtime_execution","NOT_RUN",
    "inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,
    "declaration_scope","ROOT_ONLY","referenced_declarations","UNRESOLVED_NOT_FOLLOWED","types",new[]{type},"qualification","DECLARATIONS_ONLY_NOT_USABLE_SERVICE",
    "limitations",new[]{"Public nongeneric interface root and public declared methods only; direct interface references remain unresolved. No closure or implementation search.","No fields, values, method bodies, attributes, dependencies or target execution. Method generic constraints/parameter defaults and service behavior remain unexamined."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string ProjectUndoManagerHash="73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a";
 const string ProjectUndoManagerIdentity="Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string OwnedProjectUndoManagerIdentity="Task009.OwnedUndoManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string ProjectUndoManagerRoot="Knx.Ets.ObjectModel.UndoRedo.UndoManager";
 const string OwnedProjectUndoManagerRoot="Owned.Task009.UndoManagerFixture";
 const int UndoManagerMethodLimit=65536;
 public static string ReadProjectUndoManager(string path,string hash){
  if(hash!=ProjectUndoManagerHash)throw Fail("INPUT_PIN");
  return ReadProjectUndoManagerInternal(path,hash,false);
 }
 // Explicit synthetic route: rejects original identities and cannot be selected by the worker.
 public static string ReadOwnedProjectUndoManager(string path,string hash){return ReadProjectUndoManagerInternal(path,hash,true);}
 static string ReadProjectUndoManagerInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);
  if(!owned&&bytes.Length!=14204880)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   foreach(TableIndex table in Enum.GetValues(typeof(TableIndex))){int count=r.GetTableRowCount(table);if(count>(table==TableIndex.MethodDef?UndoManagerMethodLimit:RowLimit))throw new RowLimitObservationException(table,count);}
   string identity=AssemblyIdentity(r);
   if(identity!=(owned?OwnedProjectUndoManagerIdentity:ProjectUndoManagerIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedProjectUndoManagerRoot:ProjectUndoManagerRoot;
   TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND");
   var selected=new HashSet<TypeDefinitionHandle>();var current=root;string disposition;
   while(true){
    if(!selected.Add(current))throw Fail("BASE_CYCLE");if(selected.Count>16)throw Fail("BASE_LIMIT");
    var b=r.GetTypeDefinition(current).BaseType;if(b.IsNil){disposition="NO_BASE";break;}
    if(b.Kind==HandleKind.TypeDefinition){current=(TypeDefinitionHandle)b;continue;}
    if(b.Kind==HandleKind.TypeReference){disposition="EXTERNAL_BASE_NOT_RESOLVED";break;}
    if(b.Kind==HandleKind.TypeSpecification){disposition="TYPESPEC_BASE_NOT_FOLLOWED";break;}
    throw Fail("BASE_SHAPE");
   }
   var types=new List<object>();int methods=0,fields=0;
   foreach(var h in r.TypeDefinitions){
    if(!selected.Contains(h))continue;if(types.Count>=1024)throw Fail("TYPE_LIMIT");var t=r.GetTypeDefinition(h);
    var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));}
    var declared=new List<object>();foreach(var mh in t.GetMethods()){
     var m=r.GetMethodDefinition(mh);string name=Str(r,m.Name);var access=m.Attributes&MethodAttributes.MemberAccessMask;
     if(access!=MethodAttributes.Public&&access!=MethodAttributes.Family&&access!=MethodAttributes.FamORAssem&&access!=MethodAttributes.FamANDAssem)continue;
     if(++methods>2048||declared.Count>=256)throw Fail("METHOD_LIMIT");string signature;
     try{signature=new Names(r,true).Method(m.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     declared.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",name,"attributes",(int)m.Attributes,"accessibility",access.ToString(),"signature",signature));
    }
    var fieldDeclarations=new List<object>();foreach(var fh in t.GetFields()){
     if(++fields>2048||fieldDeclarations.Count>=256)throw Fail("FIELD_LIMIT");var f=r.GetFieldDefinition(fh);string signature;
     try{signature=new Names(r,true).Field(f.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     fieldDeclarations.Add(Obj("token",MetadataTokens.GetToken(fh).ToString("X8"),"name",Str(r,f.Name),"attributes",(int)f.Attributes,"signature",signature));
    }
    types.Add(Obj("token",MetadataTokens.GetToken(h).ToString("X8"),"name",new Names(r,true).Named(h),"attributes",(int)t.Attributes,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"methods",declared,"fields",fieldDeclarations));
   }
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_undo_manager","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,"base_chain_disposition",disposition,"types",types,"limitations",new[]{"Fixed UndoManager and bounded same-assembly bases only; TypeSpec ancestry is not followed, external declarations remain unresolved.","Public/protected constructor/method and declared field metadata only; no field values, custom attributes, bodies, dependency resolution or target execution. No UndoManager value or working service is supplied."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string ProjectTraceEncryptorHash="4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd";
 const string ProjectTraceEncryptorIdentity="Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
 const string OwnedProjectTraceEncryptorIdentity="Task009.OwnedTraceEncryptor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string ProjectTraceEncryptorRoot="Knx.Ets.Common.Security.IProjectTraceEncryptor";
 const string OwnedProjectTraceEncryptorRoot="Owned.Task009.TraceContract";
 public static string ReadProjectTraceEncryptor(string path,string hash){
  if(hash!=ProjectTraceEncryptorHash)throw Fail("INPUT_PIN");
  return ReadProjectTraceEncryptorInternal(path,hash,false);
 }
 // Explicit synthetic route: rejects original identities and cannot be selected by the worker.
 public static string ReadOwnedProjectTraceEncryptor(string path,string hash){return ReadProjectTraceEncryptorInternal(path,hash,true);}
 static string ReadProjectTraceEncryptorInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);
  if(!owned&&bytes.Length!=503248)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   CheckRows(r,false,0);
   string identity=AssemblyIdentity(r);
   if(identity!=(owned?OwnedProjectTraceEncryptorIdentity:ProjectTraceEncryptorIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedProjectTraceEncryptorRoot:ProjectTraceEncryptorRoot;
   TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED");
   var selected=new HashSet<TypeDefinitionHandle>();var current=root;string disposition;
   while(true){
    if(!selected.Add(current))throw Fail("BASE_CYCLE");if(selected.Count>16)throw Fail("BASE_LIMIT");
    var b=r.GetTypeDefinition(current).BaseType;if(b.IsNil){disposition="NO_BASE";break;}
    if(b.Kind==HandleKind.TypeDefinition){current=(TypeDefinitionHandle)b;continue;}
    if(b.Kind==HandleKind.TypeReference){disposition="EXTERNAL_BASE_NOT_RESOLVED";break;}
    if(b.Kind==HandleKind.TypeSpecification){disposition="TYPESPEC_BASE_NOT_FOLLOWED";break;}
    throw Fail("BASE_SHAPE");
   }
   var types=new List<object>();int methods=0,fields=0;
   foreach(var h in r.TypeDefinitions){
    if(!selected.Contains(h))continue;if(types.Count>=16)throw Fail("TYPE_LIMIT");var t=r.GetTypeDefinition(h);
    var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));}
    var declared=new List<object>();foreach(var mh in t.GetMethods()){
     var m=r.GetMethodDefinition(mh);string name=Str(r,m.Name);var access=m.Attributes&MethodAttributes.MemberAccessMask;
     if(access!=MethodAttributes.Public&&access!=MethodAttributes.Family&&access!=MethodAttributes.FamORAssem&&access!=MethodAttributes.FamANDAssem)continue;
     if(++methods>2048||declared.Count>=256)throw Fail("METHOD_LIMIT");string signature;
     try{signature=new Names(r,true).Method(m.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     declared.Add(Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",name,"attributes",(int)m.Attributes,"accessibility",access.ToString(),"signature",signature));
    }
    var fieldDeclarations=new List<object>();foreach(var fh in t.GetFields()){
     if(++fields>2048||fieldDeclarations.Count>=256)throw Fail("FIELD_LIMIT");var f=r.GetFieldDefinition(fh);string signature;
     try{signature=new Names(r,true).Field(f.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     fieldDeclarations.Add(Obj("token",MetadataTokens.GetToken(fh).ToString("X8"),"name",Str(r,f.Name),"attributes",(int)f.Attributes,"signature",signature));
    }
    types.Add(Obj("token",MetadataTokens.GetToken(h).ToString("X8"),"name",new Names(r,true).Named(h),"attributes",(int)t.Attributes,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"methods",declared,"fields",fieldDeclarations));
   }
   var target=new ProviderTarget{Assembly=identity,Name=rootName,Token=MetadataTokens.GetToken(root).ToString("X8")};
   var targets=new Dictionary<string,ProviderTarget>(StringComparer.Ordinal){{rootName,target}};
   var candidates=new List<object>();var uncertainty=new Dictionary<string,int>(StringComparer.Ordinal);int edgeCount=0,work=0;
   foreach(var h in r.TypeDefinitions){
    var t=r.GetTypeDefinition(h);var matches=new List<object>();
    void Edge(EntityHandle edge,string kind){
     if(edge.IsNil)return;if(++work>262144)throw Fail("PROVIDER_WORK_LIMIT");var match=DirectTarget(r,identity,edge,targets,uncertainty);if(match==null)return;
     if(++edgeCount>128)throw Fail("PROVIDER_EDGE_LIMIT");matches.Add(Obj("kind",kind,"target_assembly",match.Assembly,"target_name",match.Name,"target_token",match.Token));
    }
    Edge(t.BaseType,"extends");int count=0;foreach(var ih in t.GetInterfaceImplementations()){if(++count>32)throw Fail("INTERFACE_LIMIT");Edge(r.GetInterfaceImplementation(ih).Interface,"implements");}
    if(matches.Count==0)continue;if(candidates.Count>=64)throw Fail("PROVIDER_TYPE_LIMIT");
    candidates.Add(Obj("assembly",identity,"token",MetadataTokens.GetToken(h).ToString("X8"),"name",new Names(r,true).Named(h),"attributes",(int)t.Attributes,"relationships",matches));
   }
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_trace_encryptor","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,"base_chain_disposition",disposition,"types",types,"candidates",candidates,"matching_edges",edgeCount,"uncertainty_counts",uncertainty,"creation_route","INCONCLUSIVE","forwarded_declarations","UNEXAMINED","limitations",new[]{"Fixed exception-named root and bounded same-assembly bases only; kind/owner must be read from declarations. TypeSpec ancestry and external declarations remain unresolved; forwarders are unexamined.","Public/protected constructor/method and declared field metadata only; no field values, custom attributes, bodies, dependency resolution or target execution. No service instance, implementation behavior, registration or cryptographic semantics is established. Direct candidate edges only; indirect/external/generic implementations are unqualified, and empty results do not prove absence."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string ProjectTraceConstructionRoot="Knx.Ets.Common.Security.ProjectTraceEncryptor";
 const string OwnedTraceConstructionIdentity="Task009.OwnedTraceConstruction, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedTraceConstructionRoot="Owned.Task009.TraceCandidate";
 const string OwnedTraceConstructionInterface="Owned.Task009.ITraceContract";
 public static string ReadProjectTraceEncryptorConstruction(string path,string hash){
  if(hash!=ProjectTraceEncryptorHash)throw Fail("INPUT_PIN");
  return ReadTraceConstructionInternal(path,hash,false);
 }
 // Fixed independently owned unsigned route, unavailable from the original worker.
 public static string ReadOwnedProjectTraceEncryptorConstruction(string path,string hash){return ReadTraceConstructionInternal(path,hash,true);}
 static string ReadTraceConstructionInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=503248)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");CheckRows(r,false,0);
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedTraceConstructionIdentity:ProjectTraceEncryptorIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedTraceConstructionRoot:ProjectTraceConstructionRoot,interfaceName=owned?OwnedTraceConstructionInterface:ProjectTraceEncryptorRoot;
   TypeDefinitionHandle root=default;foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED");
   if(MetadataTokens.GetToken(root)!=(owned?0x02000002:0x0200007E))throw Fail("ROOT_TOKEN");
   var t=r.GetTypeDefinition(root);var interfaces=new List<string>();foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r,true).Named(r.GetInterfaceImplementation(ih).Interface));}
   var declared=new List<object>();int scanned=0;
   foreach(var mh in t.GetMethods()){
    if(++scanned>256)throw Fail("METHOD_LIMIT");var m=r.GetMethodDefinition(mh);string name=Str(r,m.Name);var access=m.Attributes&MethodAttributes.MemberAccessMask;
    bool constructor=name==".ctor",publicStatic=access==MethodAttributes.Public&&(m.Attributes&MethodAttributes.Static)!=0&&name!=".cctor";
    if(!constructor&&!publicStatic)continue;string signature;var names=new Names(r,true);
    try{signature=names.Method(m.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
    var header=r.GetBlobReader(m.Signature).ReadSignatureHeader();if(((m.Attributes&MethodAttributes.Static)!=0)==header.IsInstance)throw Fail("SIGNATURE_SHAPE");
    bool exactReturn=names.ReturnType==rootName||names.ReturnType==interfaceName||names.ReturnType=="["+identity+"]"+rootName||names.ReturnType=="["+identity+"]"+interfaceName;
    if(!constructor&&!exactReturn)continue;
    declared.Add(Obj("owner_assembly",identity,"owner_type",rootName,"owner_token",MetadataTokens.GetToken(root).ToString("X8"),"token",MetadataTokens.GetToken(mh).ToString("X8"),"name",name,"attributes",(int)m.Attributes,"accessibility",access.ToString(),"signature",signature,"signature_header",header.RawValue,"declaration_kind",constructor?"CONSTRUCTOR_DECLARATION_ONLY":"FACTORY_CANDIDATE_DECLARATION_ONLY"));
   }
   var type=Obj("token",MetadataTokens.GetToken(root).ToString("X8"),"name",rootName,"attributes",(int)t.Attributes,"base_type",new Names(r,true).Named(t.BaseType),"interfaces",interfaces,"methods",declared);
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_trace_encryptor_construction","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,"types",new[]{type},"creation_route","INCONCLUSIVE","forwarded_declarations","UNEXAMINED","limitations",new[]{"One fixed candidate's direct base/interface declarations and declared constructor/public-static exact-owner-return signatures only; no ancestry or dependency resolution, recursive catalog or global factory search.","All constructor visibilities and generic signatures are declarations, not callable/safe construction or registration proof. No values, custom attributes, bodies, target execution, crypto behavior or working service is established; missing local public routes do not prove impossibility."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 const string OwnedTraceParameterIdentity="Task009.OwnedTraceParameter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
 const string OwnedTraceParameterRoot="Owned.Task009.ParameterCandidate";
 public static string ReadProjectTraceEncryptorParameter(string path,string hash){
  if(hash!=ProjectTraceEncryptorHash)throw Fail("INPUT_PIN");return ReadTraceParameterInternal(path,hash,false);
 }
 // Independently owned unsigned declarations only; never selectable from original worker.
 public static string ReadOwnedProjectTraceEncryptorParameter(string path,string hash){return ReadTraceParameterInternal(path,hash,true);}
 static string ReadTraceParameterInternal(string path,string hash,bool owned){
  long total=0;byte[] bytes=Pinned(path,hash,ref total);if(!owned&&bytes.Length!=503248)throw Fail("INPUT_SIZE");
  using(var pe=new PEReader(ImmutableArray.Create(bytes))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");CheckRows(r,false,0);
   string identity=AssemblyIdentity(r);if(identity!=(owned?OwnedTraceParameterIdentity:ProjectTraceEncryptorIdentity))throw Fail("INPUT_IDENTITY");
   string rootName=owned?OwnedTraceParameterRoot:ProjectTraceConstructionRoot;TypeDefinitionHandle root=default;
   foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)==rootName){if(!root.IsNil)throw Fail("DUPLICATE_ROOT");root=h;}
   if(root.IsNil)throw Fail("ROOT_NOT_FOUND_FORWARDERS_UNEXAMINED");if(MetadataTokens.GetToken(root)!=(owned?0x02000002:0x0200007E))throw Fail("ROOT_TOKEN");
   var t=r.GetTypeDefinition(root);if((int)t.Attributes!=(owned?257:1048833))throw Fail("TYPE_CONTRACT");
   int row=owned?1:0x364;if(row>r.GetTableRowCount(TableIndex.MethodDef))throw Fail("CTOR_NOT_FOUND");
   var mh=MetadataTokens.MethodDefinitionHandle(row);var m=r.GetMethodDefinition(mh);
   if(m.GetDeclaringType()!=root||Str(r,m.Name)!=".ctor"||(int)m.Attributes!=6278)throw Fail("CTOR_CONTRACT");
   string signature;try{signature=new Names(r,true).Method(m.Signature);}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
   if(signature!="instance Void(Int32) generic=0"||r.GetBlobReader(m.Signature).ReadSignatureHeader().RawValue!=32)throw Fail("CTOR_CONTRACT");
   var parameters=new List<object>();var sequences=new HashSet<int>();
   foreach(var ph in m.GetParameters()){
    if(parameters.Count>=2)throw Fail("PARAMETER_LIMIT");var p=r.GetParameter(ph);int sequence=p.SequenceNumber;
    if(sequence<0||sequence>1)throw Fail("PARAMETER_SEQUENCE");if(!sequences.Add(sequence))throw Fail("PARAMETER_DUPLICATE");
    object declaredDefault=null;string disposition="RETURN_NOT_INSPECTED";
    if(sequence==1){
     var ch=p.GetDefaultValue();bool flag=(p.Attributes&ParameterAttributes.HasDefault)!=0;
     if(flag==ch.IsNil)throw Fail("DEFAULT_INCONSISTENT");disposition=ch.IsNil?"ABSENT":"DECLARED_INT32_ONLY";
     if(!ch.IsNil){
      var c=r.GetConstant(ch);if(c.Parent!=(EntityHandle)ph)throw Fail("CONSTANT_OWNER");if(c.TypeCode!=ConstantTypeCode.Int32)throw Fail("CONSTANT_TYPE");
      var blob=r.GetBlobReader(c.Value);if(blob.Length!=4)throw Fail("CONSTANT_SIZE");int value=blob.ReadInt32();
      declaredDefault=Obj("token",MetadataTokens.GetToken(ch).ToString("X8"),"type","Int32","bytes",4,"raw_hex",Convert.ToHexString(r.GetBlobBytes(c.Value)),"declared_value",value);
     }
    }
    parameters.Add(Obj("token",MetadataTokens.GetToken(ph).ToString("X8"),"sequence",sequence,"name",Str(r,p.Name),"name_handle_is_nil",p.Name.IsNil,"attributes",(int)p.Attributes,"default_disposition",disposition,"default",declaredDefault));
   }
   string report=JsonSerializer.Serialize(Obj("schema_version",1,"operation","static_dca_project_trace_encryptor_parameter","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",new[]{Obj("filename",Path.GetFileName(path),"bytes",bytes.Length,"sha256",hash.ToLowerInvariant(),"identity",identity)},"selected_root",rootName,"type_token",MetadataTokens.GetToken(root).ToString("X8"),"type_attributes",(int)t.Attributes,"constructor",Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"attributes",(int)m.Attributes,"signature",signature,"signature_header",32,"parameters",parameters),"parameter_metadata",sequences.Contains(1)?"DECLARATIONS_ONLY":"MISSING_PARAMETER_METADATA","creation_route","INCONCLUSIVE","limitations",new[]{"Only fixed constructor parameter declarations; sequence0 is return metadata and its default is not inspected. Only associated sequence1 Int32 default metadata, if present, is decoded; Optional is not a generated default.","Names/flags/default constants do not establish meaning, valid values, callability, construction, registration, crypto behavior or a working service. No bodies, custom attributes, field values, dependency resolution or target execution; no runtime value is chosen."}));
   if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
  }
 }
 static string ReadInternal(string dca,string hash,string[] refs,string[] hashes,bool project,bool construction=false,bool services=false,bool constructors=false){
  if(refs==null||hashes==null||refs.Length!=hashes.Length||(construction?refs.Length!=3:refs.Length<1||refs.Length>2))throw Fail("INPUT_COUNT");
  var paths=new List<string>{dca};paths.AddRange(refs);var pins=new List<string>{hash};pins.AddRange(hashes);var bytes=new List<byte[]>();long total=0;
  // Verify every selected file before creating any reader.
  for(int i=0;i<paths.Count;i++)bytes.Add(Pinned(paths[i],pins[i],ref total));
  var inputs=new List<object>();var types=new List<object>();int methods=0,projectRoots=0;string baseDisposition=null;
  var rootReports=new List<object>();var foundRoots=new HashSet<string>(StringComparer.Ordinal);
  for(int i=0;i<paths.Count;i++)using(var pe=new PEReader(ImmutableArray.Create(bytes[i]))){
   if(!pe.HasMetadata)throw Fail("MANAGED_METADATA");var r=pe.GetMetadataReader();if(!r.IsAssembly)throw Fail("ASSEMBLY_REQUIRED");
   // The guarded four-input route pins this slot to the exact acquired ObjectModel bytes.
   CheckRows(r,construction,i);
   if(constructors&&r.GetTableRowCount(TableIndex.AssemblyRef)>256)throw Fail("REFERENCE_LIMIT");
   var input=Obj("filename",Path.GetFileName(paths[i]),"bytes",bytes[i].Length,"sha256",pins[i].ToLowerInvariant(),"identity",AssemblyIdentity(r));
   if(project&&!constructors){var assemblyRefs=new List<string>();foreach(var ah in r.AssemblyReferences){if(assemblyRefs.Count>=256)throw Fail("REFERENCE_LIMIT");assemblyRefs.Add(ReferenceIdentity(r,ah));}input.Add("assembly_references",assemblyRefs);}
   inputs.Add(input);
   var returnAtoms=construction&&!constructors?ReturnAtoms(r,services):null;
   var selected=new HashSet<TypeDefinitionHandle>();
   if(construction&&i!=0){
    string assembly=Str(r,r.GetAssemblyDefinition().Name);string[] roots=Roots(assembly,services,constructors);
    foreach(var h in r.TypeDefinitions){
     string name=new Names(r).Named(h);if(Array.IndexOf(roots,name)<0)continue;
     if(!foundRoots.Add(assembly+":"+name))throw Fail("DUPLICATE_ROOT");
     if(name==SdkRoots[0]&&(!Visible(r,h)||(r.GetTypeDefinition(h).Attributes&TypeAttributes.Interface)!=0))throw Fail("PROJECT_TYPE");
     if(constructors){selected.Add(h);rootReports.Add(Obj("input",Path.GetFileName(paths[i]),"name",name,"token",MetadataTokens.GetToken(h).ToString("X8")));}
     else rootReports.Add(Obj("input",Path.GetFileName(paths[i]),"name",name,"token",MetadataTokens.GetToken(h).ToString("X8"),"base_chain_disposition",SelectChain(r,h,selected)));
    }
   }
   else if(project&&i!=0&&Str(r,r.GetAssemblyDefinition().Name)=="Knx.Ets.Sdk"){
    TypeDefinitionHandle root=default;
    foreach(var h in r.TypeDefinitions)if(new Names(r).Named(h)=="Knx.Ets.Sdk.Project.Project"){if(!root.IsNil)throw Fail("DUPLICATE_PROJECT");root=h;}
    if(!root.IsNil){
     if(!Visible(r,root)||(r.GetTypeDefinition(root).Attributes&TypeAttributes.Interface)!=0)throw Fail("PROJECT_TYPE");
     if(++projectRoots>1)throw Fail("DUPLICATE_PROJECT");var current=root;
     while(!current.IsNil){
      if(selected.Contains(current))throw Fail("BASE_CYCLE");if(selected.Count>=16)throw Fail("BASE_LIMIT");selected.Add(current);
      var b=r.GetTypeDefinition(current).BaseType;
      if(b.IsNil){baseDisposition="NO_BASE";break;}
      if(b.Kind==HandleKind.TypeDefinition){current=(TypeDefinitionHandle)b;continue;}
      if(b.Kind==HandleKind.TypeReference){var tr=r.GetTypeReference((TypeReferenceHandle)b);if(tr.ResolutionScope.Kind!=HandleKind.AssemblyReference)throw Fail("BASE_SCOPE");baseDisposition="EXTERNAL_BASE_NOT_RESOLVED";break;}
      throw Fail("BASE_SHAPE");
     }
    }
   }
   foreach(var h in r.TypeDefinitions){
    var t=r.GetTypeDefinition(h);var n=new Names(r);string name=n.Named(h);if(project?!selected.Contains(h):!(i==0?Visible(r,h):Contracts.Contains(name)))continue;
    if(types.Count>=1024)throw Fail("TYPE_LIMIT");var interfaces=new List<string>();
    if(!constructors)foreach(var ih in t.GetInterfaceImplementations()){if(interfaces.Count>=32)throw Fail("INTERFACE_LIMIT");interfaces.Add(new Names(r).Named(r.GetInterfaceImplementation(ih).Interface));}
    var declared=new List<object>();
    bool contractCandidate=project||i!=0||interfaces.Exists(x=>x.StartsWith("[Knx.Ets.Sdk.AddIns.AddInViews, Version=",StringComparison.Ordinal));
    foreach(var mh in t.GetMethods()){
     if(!contractCandidate)continue;
     var m=r.GetMethodDefinition(mh);string mn=Str(r,m.Name);bool constructor=mn==".ctor"||mn==".cctor";
     if(constructors&&!constructor)continue;bool pub=(m.Attributes&MethodAttributes.MemberAccessMask)==MethodAttributes.Public;
     var access=m.Attributes&MethodAttributes.MemberAccessMask;
     if(project?!(constructor||pub||access==MethodAttributes.Family||access==MethodAttributes.FamORAssem||access==MethodAttributes.FamANDAssem):!pub&&(m.Attributes&MethodAttributes.Virtual)==0)continue;if(++methods>2048||declared.Count>=256)throw Fail("METHOD_LIMIT");
     string signature;var names=new Names(r,construction);try{signature=names.Method(m.Signature);}catch(InvalidDataException){throw;}catch(BadImageFormatException){throw Fail("MALFORMED_SIGNATURE");}
     var method=Obj("token",MetadataTokens.GetToken(mh).ToString("X8"),"name",mn,"attributes",(int)m.Attributes,"signature_header",r.GetBlobReader(m.Signature).ReadByte(),"signature",signature);
     if(constructors){method.Add("accessibility",access.ToString());method.Add("is_static",(m.Attributes&MethodAttributes.Static)!=0);}
     else if(construction)method.Add("factory_candidate",!constructor&&FactoryCandidate(returnAtoms,names.ReturnType)?"DECLARATION_ONLY":"NOT_SELECTED_RETURN");
     declared.Add(method);
    }
    if(constructors)types.Add(Obj("input",Path.GetFileName(paths[i]),"token",MetadataTokens.GetToken(h).ToString("X8"),"name",name,"attributes",(int)t.Attributes,"base_type",new Names(r).Named(t.BaseType),"constructors",declared));
    else types.Add(Obj("input",Path.GetFileName(paths[i]),"token",MetadataTokens.GetToken(h).ToString("X8"),"name",name,"attributes",(int)t.Attributes,"base_type",new Names(r).Named(t.BaseType),"interfaces",interfaces,"methods",declared));
   }
  }
  if(construction&&foundRoots.Count!=5)throw Fail("ROOT_NOT_FOUND");
  if(project&&!construction&&projectRoots!=1)throw Fail("PROJECT_NOT_FOUND");
  var result=Obj("schema_version",1,"operation",constructors?"static_dca_project_constructors":services?"static_dca_project_services":construction?"static_dca_project_construction":project?"static_dca_project_context":"static_dca_entrypoint_map","rights","UNRESOLVED","runtime_execution","NOT_RUN","inputs",inputs,"types",types,"limitations",constructors?new[]{"Only constructor declarations of five fixed observed provider types; no nonconstructor members, inherited members, store constructors or parameter closure inspected. Access flags and signatures are not creation or fixture qualification.","Constructor/static initializer declarations only; no bodies, custom attributes or implementations inspected or executed; no dependency resolution or vendor execution."}:services?new[]{"Five fixed service-contract roots and bounded same-assembly base declarations only; referenced interfaces and parameter types are not followed. Return annotations are declarations only, not implemented services or working factories.","No SDK implementation, method bodies or attributes inspected; no dependency resolution or vendor execution."}:construction?new[]{"Five fixed construction roots and bounded same-assembly base declarations only; external dependencies unresolved. Return-type candidate annotations are declarations only, not working factories or global construction-route inventory.","No SDK implementation, method bodies or attributes inspected; no dependency resolution or vendor execution."}:project?new[]{"Fixed Project root and bounded same-assembly base declarations only; external bases unresolved; declared constructors and public/protected methods are metadata observations, not construction or runtime compatibility proof.","No SDK implementation, method bodies or attributes inspected; no dependency resolution or vendor execution."}:new[]{"DCA method details only for direct AddInViews interface implementers; public type inventory and selected contract declarations only; no inherited member or runtime activation proof.","Names/signatures are metadata observations; no SDK implementation or method bodies inspected."});
  if(constructors)result.Add("creation_route","INCONCLUSIVE");
  if(construction)result.Add("selected_roots",rootReports);
  else if(project)result.Add("base_chain_disposition",baseDisposition);
  string report=JsonSerializer.Serialize(result);
  if(report.Length>512*1024)throw Fail("REPORT_LIMIT");return report;
 }
}
}
