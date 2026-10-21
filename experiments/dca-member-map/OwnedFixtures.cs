// Synthetic metadata images only. No target code or executable method bodies.
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
public static class Task142OwnedFixtures {
 static BlobHandle Blob(MetadataBuilder m, params byte[] bytes) { return m.GetOrAddBlob(bytes); }
 static AssemblyReferenceHandle Ref(MetadataBuilder m,string name,Version version,string culture=null,bool wrongToken=false) { return m.AddAssemblyReference(m.GetOrAddString(name),version,culture==null?default:m.GetOrAddString(culture),wrongToken?Blob(m,1,2,3,4,5,6,7,8):default,0,default); }
 static BlobHandle Framework(MetadataBuilder m,int stringLength) {
  var b=new BlobBuilder(); b.WriteUInt16(1); byte[] text=Encoding.UTF8.GetBytes(stringLength>0?new string('X',stringLength):".NETFramework,Version=v4.8"); b.WriteCompressedInteger(text.Length);b.WriteBytes(text);b.WriteUInt16(0);return m.GetOrAddBlob(b);
 }
 public static Dictionary<string,int> Create(string path,bool candidate,bool changed=false,int fields=1,int nameLength=0,int scopeDepth=0,int frameworkStringLength=0,string ownerCulture=null,bool wrongToken=false,int typeProfile=0) {
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("Owned.dll"),m.GetOrAddGuid(new Guid("11111111-1111-1111-1111-111111111111")),default,default);
  var assembly=m.AddAssembly(m.GetOrAddString(candidate?"OwnedSdk":"OwnedConsumer"),new Version(changed?2:1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=Ref(m,"System.Runtime",new Version(10,0,0,0));
  var sdk= candidate?default(AssemblyReferenceHandle):Ref(m,"OwnedSdk",new Version(1,0,0,0),ownerCulture,wrongToken);
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  string ownerNamespace="Owned", ownerName=candidate?"Owner`1":"Consumer";
  if(candidate && typeProfile==1) { ownerNamespace=""; ownerName="Owned.Owner`1"; }
  if(candidate && typeProfile==3) ownerName="Outer+Owner`1";
  if(candidate && typeProfile==5) ownerNamespace="Owned+Outer";
  TypeDefinitionHandle outer=default;
  if(candidate && (typeProfile==4 || typeProfile==6)) {
   outer=m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Outer"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   ownerNamespace="";
  }
  var owner=m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString(ownerNamespace),m.GetOrAddString(nameLength>0?new string('X',nameLength):ownerName),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  if(!outer.IsNil) m.AddNestedType(owner,outer);
  var enclosing=owner;
  for(int i=0;i<scopeDepth;i++) {
   var nested=m.AddTypeDefinition(TypeAttributes.NestedPublic,default,m.GetOrAddString("Nested"+i),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   m.AddNestedType(nested,enclosing);enclosing=nested;
  }
  var tf=m.AddTypeReference(runtime,m.GetOrAddString("System.Runtime.Versioning"),m.GetOrAddString("TargetFrameworkAttribute"));
  var tfCtor=m.AddMemberReference(tf,m.GetOrAddString(".ctor"),Blob(m,0x20,1,1,0x0e));
  m.AddCustomAttribute(assembly,tfCtor,Framework(m,frameworkStringLength));
  if(candidate) {
   m.AddGenericParameter(owner,0,m.GetOrAddString("T"),0);
   for(int i=0;i<fields;i++) m.AddFieldDefinition(FieldAttributes.Public,m.GetOrAddString(i==0?"Value":"F"+i),Blob(m,6,8));
   m.AddMethodDefinition(MethodAttributes.Public,MethodImplAttributes.IL,m.GetOrAddString(".ctor"),Blob(m,0x20,0,1),0,MetadataTokens.ParameterHandle(1));
   m.AddMethodDefinition(MethodAttributes.Public,MethodImplAttributes.IL,m.GetOrAddString("Ping"),changed?Blob(m,0x20,1,0x0e,0x0e):Blob(m,0x20,1,8,8),0,MetadataTokens.ParameterHandle(1));
   m.AddMethodDefinition(MethodAttributes.Public,MethodImplAttributes.IL,m.GetOrAddString("Echo"),Blob(m,0x20,1,0x13,0,0x13,0),0,MetadataTokens.ParameterHandle(1));
   var generic=m.AddMethodDefinition(MethodAttributes.Public,MethodImplAttributes.IL,m.GetOrAddString("Generic"),Blob(m,0x30,1,1,0x1e,0,0x13,0),0,MetadataTokens.ParameterHandle(1));
   m.AddGenericParameter(generic,0,m.GetOrAddString("U"),0);
  } else {
   string typeNamespace=typeProfile==2?"":(typeProfile==5?"Owned+Outer":"Owned");
   string typeName=typeProfile==2?"Owned.Owner`1":(typeProfile==4?"Outer+Owner`1":"Owner`1");
   EntityHandle scope=sdk;
   if(typeProfile==3 || typeProfile==6) {
    scope=m.AddTypeReference(sdk,m.GetOrAddString("Owned"),m.GetOrAddString("Outer"));
    typeNamespace="";
   }
   var type=m.AddTypeReference(scope,m.GetOrAddString(typeNamespace),m.GetOrAddString(typeName));
   m.AddMemberReference(type,m.GetOrAddString(".ctor"),Blob(m,0x20,0,1));
   m.AddMemberReference(type,m.GetOrAddString("Ping"),Blob(m,0x20,1,8,8));
   m.AddMemberReference(type,m.GetOrAddString("Value"),Blob(m,6,8));
   m.AddMemberReference(type,m.GetOrAddString("Inherited"),Blob(m,0x20,0,1));
   // TargetFrameworkAttribute is row 1; a nested-owner fixture adds an outer row.
   byte ownerToken=(byte)((MetadataTokens.GetRowNumber(type)<<2)|1);
   var spec=m.AddTypeSpecification(Blob(m,0x15,0x12,ownerToken,1,8));
   m.AddMemberReference(spec,m.GetOrAddString("Echo"),Blob(m,0x20,1,0x13,0,0x13,0));
   m.AddMemberReference(type,m.GetOrAddString("Generic"),Blob(m,0x30,1,1,0x1e,0,0x13,0));
   m.AddMemberReference(type,m.GetOrAddString("Ping"),Blob(m,0x20,1)); // malformed
   m.AddMemberReference(type,m.GetOrAddString("Ping"),Blob(m,0x25,0,1)); // unsupported varargs
  }
  var probes=new Dictionary<string,int>();
  Action<string,byte[]> add=(name,bytes)=>probes.Add(name,MetadataTokens.GetHeapOffset(m.GetOrAddBlob(bytes)));
  add("field",new byte[]{6,8});add("method",new byte[]{0x20,1,8,8});
  add("truncated",new byte[]{0x20,1});add("trailing",new byte[]{6,8,8});add("voidfield",new byte[]{6,1});
  add("array",new byte[]{6,0x14});add("invalid",new byte[]{6,0xff});add("count",new byte[]{0,0x80,0x81,1});
  var depth=new List<byte>{6};for(int i=0;i<16;i++)depth.Add(0x1d);depth.Add(8);add("depth",depth.ToArray());
  var nodes=new List<byte>{0,0x80,0x80,1};for(int i=0;i<128;i++)nodes.Add(8);add("nodes",nodes.ToArray());
  add("blob",new byte[513]);
  // Modifier references the existing TargetFrameworkAttribute TypeRef: coded 5.
  add("modifier",new byte[]{6,0x1f,5,8});add("optional",new byte[]{6,0x20,5,8});
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);
  File.WriteAllBytes(path,image.ToArray());return probes;
 }
}
