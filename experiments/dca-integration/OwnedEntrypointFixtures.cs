// Synthetic original metadata images, no method bodies or vendor bytes.
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
public static class Task144OwnedFixtures {
 // Unsigned owned identity, deliberately distinct from the fixed original Ids pin.
 public static void CreateProjectId(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedProjectId.dll"),m.GetOrAddGuid(new Guid("00990000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(profile==5?"Knx.Ets.Ids":"Task009.OwnedProjectId"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var valueType=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("ValueType"));var guid=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Guid"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  int baseCount=profile==16?3:profile==18?16:profile==19?1:profile==22||profile==23?8:0,fieldStart=1,methodStart=1;
  for(int i=0;i<=baseCount;i++){
   EntityHandle parent=i<baseCount?MetadataTokens.TypeDefinitionHandle(i+3):valueType;
   if(profile==17)parent=MetadataTokens.TypeDefinitionHandle(2);
   if(profile==6){var b=new BlobBuilder();b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(1);b.WriteByte(0x0e);parent=m.AddTypeSpecification(m.GetOrAddBlob(b));}
   var type=m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Knx.Ets.Ids.Project"),m.GetOrAddString(i>0?"OwnedBase"+i:profile==1?"Other":"ProjectId"),parent,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
   int fieldCount=profile==8?257:profile==16?200:profile==22||profile==25?256:2;
   for(int j=0;j<fieldCount;j++){
    byte[] sig=j==0?new byte[]{6,0x11,(byte)((MetadataTokens.GetRowNumber(guid)<<2)|1)}:new byte[]{6,0x0e};
    if(profile==3&&j==0)sig=new byte[]{6};if(profile==12&&j==0)sig=new byte[129];if(profile==13&&j==0)sig=new byte[]{6,8,8};if(profile==14&&j==0)sig=new byte[]{6,1};if(profile==20&&j==0)sig=new byte[]{0,8};if(profile==24&&j==0)sig=new byte[]{0x26,8};
    if(profile==15&&j==0){sig=new byte[20];sig[0]=6;for(int k=1;k<19;k++)sig[k]=0x1d;sig[19]=8;}
    if(profile==30&&j==0){
     EntityHandle scope=runtime;TypeReferenceHandle nested=default;for(int k=0;k<4;k++){nested=m.AddTypeReference(scope,k==0?m.GetOrAddString("Owned"):default,m.GetOrAddString("Nested"+k));scope=nested;}
     var b=new BlobBuilder();b.WriteByte(6);b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(32);
     for(int k=0;k<32;k++){b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(nested)<<2)|1);}sig=b.ToArray();
    }
    if(profile==31&&j==0)sig=Array.Empty<byte>();
    string name=(profile==11?new string('L',1025):profile==16?new string('L',1000):"Field")+j;
    var field=m.AddFieldDefinition(j==0?FieldAttributes.Private|FieldAttributes.InitOnly:FieldAttributes.Public|FieldAttributes.Static|FieldAttributes.Literal,m.GetOrAddString(name),m.GetOrAddBlob(sig));
    if(j==1)m.AddConstant(field,"OWNED_CONSTANT_VALUE_MUST_NOT_BE_READ");fieldStart++;
   }
   int methodCount=profile==9?257:profile==23||profile==26?256:4;
   for(int j=0;j<methodCount;j++){
    string name=j==0?".ctor":j==1?"Parse":j==2?"Protected":"PrivateIgnored";
    var access=j==0||j>=3?MethodAttributes.Private:j==1?MethodAttributes.Public:MethodAttributes.Family;
    byte[] sig=j==0?new byte[]{0x20,1,1,0x0e}:j==1?new byte[]{0,0,0x11,8}:j==2?new byte[]{0x20,0,0x0e}:new byte[]{0x20,1};
    if(profile==9||profile==23||profile==26){access=MethodAttributes.Public;if(j>=3)sig=new byte[]{0x20,0,1};}if(profile==4&&j==0)sig=new byte[]{0x20,1};
    Method(m,name,access,sig);methodStart++;
   }
   if(profile==21)for(int j=0;j<33;j++)m.AddInterfaceImplementation(type,valueType);
  }
  if(profile==2)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Ids.Project"),m.GetOrAddString("ProjectId"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==10||profile==27)for(int j=2;j<(profile==10?32769:32768);j++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+j),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==28||profile==29){m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));for(int j=4;j<(profile==28?32769:32768);j++)Method(m,"Padding",MethodAttributes.Public,new byte[]{0x20,1});}
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 // Unsigned owned identity, deliberately distinct from the fixed original Ids pin.
 public static void CreateProjectUndoManager(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedUndoManager.dll"),m.GetOrAddGuid(new Guid("009a0000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(profile==5?"Task009.ForeignUndoManager":"Task009.OwnedUndoManager"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var valueType=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("ValueType"));var guid=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Guid"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  int baseCount=profile==16?3:profile==18?16:profile==19?1:profile==22||profile==23?8:0,fieldStart=1,methodStart=1;
  for(int i=0;i<=baseCount;i++){
   EntityHandle parent=i<baseCount?MetadataTokens.TypeDefinitionHandle(i+3):valueType;
   if(profile==17)parent=MetadataTokens.TypeDefinitionHandle(2);
   if(profile==6){var b=new BlobBuilder();b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(1);b.WriteByte(0x0e);parent=m.AddTypeSpecification(m.GetOrAddBlob(b));}
   var type=m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Owned.Task009"),m.GetOrAddString(i>0?"OwnedBase"+i:profile==1?"Other":"UndoManagerFixture"),parent,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
   int fieldCount=profile==8?257:profile==16?200:profile==22||profile==25?256:2;
   for(int j=0;j<fieldCount;j++){
    byte[] sig=j==0?new byte[]{6,0x11,(byte)((MetadataTokens.GetRowNumber(guid)<<2)|1)}:new byte[]{6,0x0e};
    if(profile==3&&j==0)sig=new byte[]{6};if(profile==12&&j==0)sig=new byte[129];if(profile==13&&j==0)sig=new byte[]{6,8,8};if(profile==14&&j==0)sig=new byte[]{6,1};if(profile==20&&j==0)sig=new byte[]{0,8};if(profile==24&&j==0)sig=new byte[]{0x26,8};
    if(profile==15&&j==0){sig=new byte[20];sig[0]=6;for(int k=1;k<19;k++)sig[k]=0x1d;sig[19]=8;}
    if(profile==30&&j==0){
     EntityHandle scope=runtime;TypeReferenceHandle nested=default;for(int k=0;k<4;k++){nested=m.AddTypeReference(scope,k==0?m.GetOrAddString("Owned"):default,m.GetOrAddString("Nested"+k));scope=nested;}
     var b=new BlobBuilder();b.WriteByte(6);b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(32);
     for(int k=0;k<32;k++){b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(nested)<<2)|1);}sig=b.ToArray();
    }
    if(profile==31&&j==0)sig=Array.Empty<byte>();
    string name=(profile==11?new string('L',1025):profile==16?new string('L',1000):"Field")+j;
    var field=m.AddFieldDefinition(j==0?FieldAttributes.Private|FieldAttributes.InitOnly:FieldAttributes.Public|FieldAttributes.Static|FieldAttributes.Literal,m.GetOrAddString(name),m.GetOrAddBlob(sig));
    if(j==1)m.AddConstant(field,"OWNED_CONSTANT_VALUE_MUST_NOT_BE_READ");fieldStart++;
   }
   int methodCount=profile==9?257:profile==23||profile==26?256:6;
   for(int j=0;j<methodCount;j++){
    string name=j==0||j==4||j==5?".ctor":j==1?"OwnedFactory":j==2?"Protected":"PrivateIgnored";
    var access=j==3||j==4?MethodAttributes.Private:j==0||j==1?MethodAttributes.Public:MethodAttributes.Family;
    byte[] sig=j==0?new byte[]{0x20,1,1,0x0e}:j==1?new byte[]{0,0,0x11,8}:j==2?new byte[]{0x20,0,0x0e}:j==5?new byte[]{0x20,0,1}:new byte[]{0x20,1};
    if(profile==9||profile==23||profile==26){access=MethodAttributes.Public;if(j>=3)sig=new byte[]{0x20,0,1};}if(profile==4&&j==0)sig=new byte[]{0x20,1};
    Method(m,name,access,sig);methodStart++;
   }
   if(profile==21)for(int j=0;j<33;j++)m.AddInterfaceImplementation(type,valueType);
  }
  if(profile==2)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("UndoManagerFixture"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==10||profile==27)for(int j=2;j<(profile==10?32769:32768);j++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+j),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==28||profile==29||profile>=32&&profile<=35){m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));for(int j=6;j<(profile==32?38472:profile==33?65536:profile==34?65537:profile==35?32769:profile==28?32769:32768);j++)Method(m,"Padding",MethodAttributes.Public,new byte[]{0x20,1});}
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void CreateProjectTraceEncryptor(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedTraceEncryptor.dll"),m.GetOrAddGuid(new Guid("009b0000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(profile==5?"Task009.ForeignTraceEncryptor":"Task009.OwnedTraceEncryptor"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var valueType=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("ValueType"));var guid=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Guid"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  int baseCount=profile==16?3:profile==18?16:profile==19?1:profile==22||profile==23?8:0,fieldStart=1,methodStart=1;
  for(int i=0;i<=baseCount;i++){
   EntityHandle parent=i<baseCount?MetadataTokens.TypeDefinitionHandle(i+3):valueType;
   if(profile==17)parent=MetadataTokens.TypeDefinitionHandle(2);
   if(profile==6){var b=new BlobBuilder();b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(1);b.WriteByte(0x0e);parent=m.AddTypeSpecification(m.GetOrAddBlob(b));}
   var type=m.AddTypeDefinition((profile>=40?TypeAttributes.Public|TypeAttributes.Interface|TypeAttributes.Abstract:TypeAttributes.Public|TypeAttributes.Sealed),m.GetOrAddString("Owned.Task009"),m.GetOrAddString(i>0?"OwnedBase"+i:profile==1?"Other":"TraceContract"),profile>=40?default:parent,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
   int fieldCount=profile>=40?0:profile==8?257:profile==16?200:profile==22||profile==25?256:2;
   for(int j=0;j<fieldCount;j++){
    byte[] sig=j==0?new byte[]{6,0x11,(byte)((MetadataTokens.GetRowNumber(guid)<<2)|1)}:new byte[]{6,0x0e};
    if(profile==3&&j==0)sig=new byte[]{6};if(profile==12&&j==0)sig=new byte[129];if(profile==13&&j==0)sig=new byte[]{6,8,8};if(profile==14&&j==0)sig=new byte[]{6,1};if(profile==20&&j==0)sig=new byte[]{0,8};if(profile==24&&j==0)sig=new byte[]{0x26,8};
    if(profile==15&&j==0){sig=new byte[20];sig[0]=6;for(int k=1;k<19;k++)sig[k]=0x1d;sig[19]=8;}
    if(profile==30&&j==0){
     EntityHandle scope=runtime;TypeReferenceHandle nested=default;for(int k=0;k<4;k++){nested=m.AddTypeReference(scope,k==0?m.GetOrAddString("Owned"):default,m.GetOrAddString("Nested"+k));scope=nested;}
     var b=new BlobBuilder();b.WriteByte(6);b.WriteByte(0x15);b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(valueType)<<2)|1);b.WriteByte(32);
     for(int k=0;k<32;k++){b.WriteByte(0x12);b.WriteCompressedInteger((MetadataTokens.GetRowNumber(nested)<<2)|1);}sig=b.ToArray();
    }
    if(profile==31&&j==0)sig=Array.Empty<byte>();
    string name=(profile==11?new string('L',1025):profile==16?new string('L',1000):"Field")+j;
    var field=m.AddFieldDefinition(j==0?FieldAttributes.Private|FieldAttributes.InitOnly:FieldAttributes.Public|FieldAttributes.Static|FieldAttributes.Literal,m.GetOrAddString(name),m.GetOrAddBlob(sig));
    if(j==1)m.AddConstant(field,"OWNED_CONSTANT_VALUE_MUST_NOT_BE_READ");fieldStart++;
   }
   int methodCount=profile>=40?1:profile==9?257:profile==23||profile==26?256:6;
   for(int j=0;j<methodCount;j++){
    string name=j==0||j==4||j==5?".ctor":j==1?"OwnedFactory":j==2?"Protected":"PrivateIgnored";
    var access=j==3||j==4?MethodAttributes.Private:j==0||j==1?MethodAttributes.Public:MethodAttributes.Family;
    byte[] sig=j==0?new byte[]{0x20,1,1,0x0e}:j==1?new byte[]{0,0,0x11,8}:j==2?new byte[]{0x20,0,0x0e}:j==5?new byte[]{0x20,0,1}:new byte[]{0x20,1};
    if(profile==9||profile==23||profile==26){access=MethodAttributes.Public;if(j>=3)sig=new byte[]{0x20,0,1};}if(profile==4&&j==0)sig=new byte[]{0x20,1};
    if(profile>=40){name="Transform";access=MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual;sig=new byte[]{0x20,1,0x0e,0x0e};}
    Method(m,name,access,sig);methodStart++;
   }
   if(profile==21)for(int j=0;j<33;j++)m.AddInterfaceImplementation(type,valueType);
  }
  if(profile==2)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceContract"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==10||profile==27)for(int j=2;j<(profile==10?32769:32768);j++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+j),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
  if(profile==28||profile==29||profile>=32&&profile<=35){m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));for(int j=6;j<(profile==32?38472:profile==33?65536:profile==34?65537:profile==35?32769:profile==28?32769:32768);j++)Method(m,"Padding",MethodAttributes.Public,new byte[]{0x20,1});}
  if(profile>=40&&profile!=48){
   // Owned interface-only shape; no vendor namespace, signature or signing identity.
   // Class profiles below40 separately exercise constructor/field/base limits.
   var foreign=m.AddAssemblyReference(m.GetOrAddString("Owned.Foreign"),new Version(1,0,0,0),default,default,0,default);
   var foreignRoot=m.AddTypeReference(foreign,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceContract"));
   EntityHandle root=MetadataTokens.TypeDefinitionHandle(2);
   if(profile==49){var own=m.AddAssemblyReference(m.GetOrAddString("Task009.OwnedTraceEncryptor"),new Version(1,0,0,0),default,default,0,default);root=m.AddTypeReference(own,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceContract"));}
   int count=profile==44?65:profile==45?5:profile==50?4:profile==51?64:profile==40?2:1;
   TypeDefinitionHandle first=default;
   for(int j=0;j<count;j++){
    var candidate=m.AddTypeDefinition(j==0?TypeAttributes.Abstract:TypeAttributes.Public,m.GetOrAddString("Owned.Candidates"),m.GetOrAddString("Candidate"+j),valueType,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));
    if(j==0)first=candidate;
    EntityHandle target=profile==41?foreignRoot:profile==42&&j>0?first:root;
    if(profile==43){var sig=new BlobBuilder();sig.WriteByte(0x15);sig.WriteByte(0x12);sig.WriteCompressedInteger((MetadataTokens.GetRowNumber(root)<<2));sig.WriteByte(1);sig.WriteByte(0x0e);target=m.AddTypeSpecification(m.GetOrAddBlob(sig));}
    for(int k=0;k<(profile==45?26:profile==46?33:profile==50?32:1);k++)m.AddInterfaceImplementation(candidate,target);
   }
   if(profile==42){var indirect=m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned.Candidates"),m.GetOrAddString("Indirect"),first,MetadataTokens.FieldDefinitionHandle(fieldStart),MetadataTokens.MethodDefinitionHandle(methodStart));}
  }
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 // Independently owned, unsigned declaration shapes; no target bodies or original SDK names.
 public static void CreateTraceConstruction(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedTraceConstruction.dll"),m.GetOrAddGuid(new Guid("009c0000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(profile==5?"Owned.ForeignTraceConstruction":"Task009.OwnedTraceConstruction"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var foreign=m.AddAssemblyReference(m.GetOrAddString("Owned.Foreign"),new Version(1,0,0,0),default,default,0,default);
  var self=m.AddAssemblyReference(m.GetOrAddString("Task009.OwnedTraceConstruction"),new Version(1,0,0,0),default,default,0,default);
  var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  var dep=m.AddTypeReference(foreign,m.GetOrAddString("Owned.Dependencies"),m.GetOrAddString(profile==16?new string('D',1000):profile==23?"Invalid[Name]":"Dependency"));
  var same=m.AddTypeReference(self,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceCandidate"));
  var wrong=m.AddTypeReference(foreign,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceCandidate"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  if(profile==6)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("PaddingBefore"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  var root=m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Owned.Task009"),m.GetOrAddString(profile==1?"Other":"TraceCandidate"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  byte Ref(TypeReferenceHandle h){return (byte)((MetadataTokens.GetRowNumber(h)<<2)|1);}
  int count=profile==8?257:profile==16||profile==18?256:profile==24?1:15;
  for(int j=0;j<count;j++){
   string name=j<4||j==11||j==12?".ctor":j==4?"CreateCandidate":j==5?"CreateInterface":j==6?"CreateSameOwnerRef":j==7?"ForeignIgnored":j==8?"InstanceIgnored":j==9?"PrivateIgnored":j==10?"GenericCreate":j==13?"WrongReturn":"VoidIgnored";
   var access=j==1?MethodAttributes.Family:j==2?MethodAttributes.Assembly:j==3||j==9?MethodAttributes.Private:j==11?MethodAttributes.FamORAssem:j==12?MethodAttributes.FamANDAssem:MethodAttributes.Public;
   bool ctor=name==".ctor",instance=ctor||j==8;var attrs=access|(instance?0:MethodAttributes.Static);
   byte[] sig=ctor?(j==0?new byte[]{0x20,1,1,0x12,Ref(dep)}:new byte[]{0x20,0,1}):j==4?new byte[]{0,0,0x12,8}:j==5?new byte[]{0,0,0x12,12}:j==6?new byte[]{0,0,0x12,Ref(same)}:j==7?new byte[]{0,0,0x12,Ref(wrong)}:j==8?new byte[]{0x20,0,0x12,8}:j==9?new byte[]{0x20,1}:j==10?new byte[]{0x10,1,0,0x12,8}:new byte[]{0,0,j==13?(byte)0x0e:(byte)1};
   if(profile==8||profile==18){name=".ctor";attrs=MethodAttributes.Public;sig=new byte[]{0x20,0,1};}
   if(profile==16){name=".ctor";attrs=MethodAttributes.Public;sig=new byte[]{0x20,2,1,0x12,Ref(dep),0x12,Ref(dep)};}
   if(j==0){
    if(profile==3)sig=Array.Empty<byte>();if(profile==4)sig=new byte[129];if(profile==7)attrs|=MethodAttributes.Static;
    if(profile==11)name=new string('L',1025);
    if(profile==12){sig=new byte[23];sig[0]=0x20;sig[1]=1;sig[2]=1;for(int k=3;k<22;k++)sig[k]=0x1d;sig[22]=8;}
    if(profile==13)sig=new byte[]{0x20,0,1,8};if(profile==14)sig=new byte[]{0x20,1,1,1};if(profile==15)sig=new byte[]{6,0,1};
   }
   if(profile==24){name="InstanceOnly";attrs=MethodAttributes.Public;sig=new byte[]{0x20,0,0x0e};}
   Method(m,name,attrs,sig);
  }
  var contract=m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Interface|TypeAttributes.Abstract,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("ITraceContract"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(count+1));
  for(int j=0;j<(profile==9?33:profile==17?32:1);j++)m.AddInterfaceImplementation(root,contract);
  if(profile==2)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("TraceCandidate"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(count+1));
  if(profile==10||profile==21)for(int j=3;j<(profile==10?32769:32768);j++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+j),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(count+1));
  if(profile==19||profile==20){m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(count+1));for(int j=count;j<(profile==20?32769:32768);j++)Method(m,"Padding",MethodAttributes.Public,new byte[]{0x20,1});}
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 // Unsigned owned Param/Constant metadata; never original SDK identities, tokens or execution.
 public static void CreateTraceParameter(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedTraceParameter.dll"),m.GetOrAddGuid(new Guid("009d0000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(profile==30?"Owned.ForeignTraceParameter":"Task009.OwnedTraceParameter"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  if(profile==29)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Before"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(profile==31?TypeAttributes.NotPublic|TypeAttributes.Sealed:TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Owned.Task009"),m.GetOrAddString(profile==27?"Other":"ParameterCandidate"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(profile==25?2:1));
  byte[] sig=profile==22?new byte[]{0x20,1,1,9}:profile==35?Array.Empty<byte>():profile==36?new byte[129]:profile==41?new byte[]{0x30,1,1,1,8}:profile==42?new byte[]{0,1,1,8}:new byte[]{0x20,1,1,8};
  if(profile==25)m.AddMethodDefinition((MethodAttributes)6278,MethodImplAttributes.IL,m.GetOrAddString(".ctor"),m.GetOrAddBlob(new byte[]{0x20,1,1,8}),0,MetadataTokens.ParameterHandle(1));
  if(profile!=26)m.AddMethodDefinition((MethodAttributes)(profile==23?6273:6278),MethodImplAttributes.IL,m.GetOrAddString(profile==24?"NotCtor":".ctor"),m.GetOrAddBlob(sig),0,MetadataTokens.ParameterHandle(1));
  if(profile==28)m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Owned.Task009"),m.GetOrAddString("ParameterCandidate"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(2));
  int count=profile==5?0:profile==1||profile==7||profile==37||profile==38?2:profile==9||profile==22||profile==23?3:profile==33?32769:profile==34?32768:1;
  for(int j=0;j<count;j++){
   int sequence=profile==8?2:profile==6?0:profile==7?1:profile==38?1-j:count>1?(j==0?0:1):1;
   string name=sequence==0?"OwnedReturn":profile==4?"":profile==21?new string('N',1025):"OwnedParameter";
   bool defaultFlag=sequence==1&&(profile==2||profile==10||profile>=12&&profile<=20||profile==37||profile==44);
   var flags=defaultFlag?ParameterAttributes.HasDefault:ParameterAttributes.None;
   if(profile==2||profile==3)flags|=ParameterAttributes.Optional;
   if(profile==39)flags=ParameterAttributes.In|ParameterAttributes.Out|ParameterAttributes.Optional|ParameterAttributes.HasFieldMarshal;
   if(sequence==0&&(profile==1||profile==37))flags=ParameterAttributes.HasDefault;
   var ph=m.AddParameter(flags,m.GetOrAddString(name),sequence);
   if(sequence==0&&(profile==1||profile==37))m.AddConstant(ph,"RETURN_CONSTANT_MUST_NOT_BE_READ"+new string('R',1024));
   if(sequence==1&&(defaultFlag&&profile!=10||profile==11)){
    object value=profile==12||profile==15?(object)(short)-123:profile==13||profile==16?(object)(long)-123:profile==14?(object)"OWNED_NOT_INT32":profile==17?(object)"":profile==18?(object)int.MinValue:profile==19?(object)int.MaxValue:profile==20?(object)0:profile==44?null:(object)(-123);
    m.AddConstant(ph,value);
   }
  }
  if(profile==32)for(int j=2;j<32769;j++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+j),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(2));
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);byte[] data=image.ToArray();
  if(profile==15||profile==16||profile==17){
   // Corrupt only owned Constant type metadata to test invalid Int32 blob lengths.
   using(var pe=new PEReader(System.Collections.Immutable.ImmutableArray.Create(data))){var r=pe.GetMetadataReader();int offset=pe.PEHeaders.MetadataStartOffset+r.GetTableMetadataOffset(TableIndex.Constant);data[offset]=(byte)ConstantTypeCode.Int32;}
  }
  File.WriteAllBytes(path,data);
 }
 public static void CreateConstructorCandidates(string path,bool objectModel=false,int profile=0,int methodRows=0){
  int minimum=(objectModel?9:11)+(profile==4?257:0);if(methodRows==0)methodRows=minimum;if(methodRows<minimum||methodRows>65537)throw new ArgumentOutOfRangeException();
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedConstructors.dll"),m.GetOrAddGuid(new Guid("00970000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  if(profile==5)for(int i=0;i<256;i++)m.AddAssemblyReference(m.GetOrAddString("Owned.Reference"+i),new Version(1,0,0,0),default,default,0,default);
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("UnselectedBase"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  Method(m,".ctor",MethodAttributes.Public,new byte[]{0x20,1});
  m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.ObjectModel.FileSystemStore"),m.GetOrAddString("FileProjectStore"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(2));
  Method(m,".ctor",MethodAttributes.Public,new byte[]{0x20,1});int next=3;
  void Add(string name,MethodAttributes attributes,byte[] signature){Method(m,name,attributes,signature);next++;}
  for(int i=minimum;i<methodRows;i++)Add("UnselectedPadding",MethodAttributes.Public,new byte[]{0x20,1});
  string ns=objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk";string[] names=objectModel?new[]{"Factory","ObjectContext"}:new[]{"DcaExecutionContext","Context","SdkFactory"};
  var ctor=MethodAttributes.SpecialName|MethodAttributes.RTSpecialName;
  for(int i=0;i<names.Length;i++){
   string name=profile==1&&i==names.Length-1?"Other":names[i];
   m.AddTypeDefinition(objectModel?TypeAttributes.Public:TypeAttributes.NotPublic,m.GetOrAddString(ns),m.GetOrAddString(name),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(next));
   if(i==0){
    Add(".ctor",MethodAttributes.Private|ctor,profile==3?new byte[]{0x20,1}:new byte[]{0x20,0,1});
    Add(".ctor",MethodAttributes.Family|ctor,new byte[]{0x20,1,1,0x0e});
    Add(".ctor",MethodAttributes.Public|ctor,new byte[]{0x20,1,1,8});
    Add(".cctor",MethodAttributes.Private|MethodAttributes.Static|ctor,new byte[]{0,0,1});
    if(profile==4)for(int j=0;j<257;j++)Add(".ctor",MethodAttributes.Private|ctor,new byte[]{0x20,0,1});
   }else Add(".ctor",(objectModel?MethodAttributes.Public:MethodAttributes.Assembly)|ctor,new byte[]{0x20,1,1,0x1c});
   Add("UnrelatedMalformedMethod",MethodAttributes.Public,new byte[]{0x20,1});
  }
  if(profile==2)m.AddTypeDefinition(objectModel?TypeAttributes.Public:TypeAttributes.NotPublic,m.GetOrAddString(ns),m.GetOrAddString(names[0]),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(next));
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 static TypeDefinitionHandle ProviderType(MetadataBuilder m,string name,TypeAttributes attributes,EntityHandle parent,int methodStart=2){return m.AddTypeDefinition(attributes,m.GetOrAddString("Owned"),m.GetOrAddString(name),parent,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(methodStart));}
 public static void CreateProviders(string path,bool objectModel=false,int profile=0,int methodRows=1){
  if(methodRows<1||methodRows>65537)throw new ArgumentOutOfRangeException();
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedProviders.dll"),m.GetOrAddGuid(new Guid("00950000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  string ns=objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk";var iface=TypeAttributes.Interface|TypeAttributes.Abstract;
  m.AddTypeDefinition(objectModel?TypeAttributes.Public|iface:TypeAttributes.NotPublic|TypeAttributes.Abstract,m.GetOrAddString(ns),m.GetOrAddString(objectModel?"IFactory":"ExecutionContext"),objectModel?default:obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString(ns),m.GetOrAddString(objectModel?"IProjectStore":"ISdkFactory"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(methodRows+1));
  m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString(ns),m.GetOrAddString(profile==2?"Missing":objectModel?"IObjectContext":"ISdkContext"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(methodRows+1));
  // Deliberately malformed signatures prove the relationship mode never inspects methods.
  for(int i=0;i<methodRows;i++)Method(m,"IgnoredInvalidSignature",MethodAttributes.Public,new byte[]{0x20,1});
  int start=methodRows+1;
  if(profile==3)m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString(ns),m.GetOrAddString(objectModel?"IFactory":"ISdkContext"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(start));
  if(profile!=1){
   if(objectModel){
    var provider=ProviderType(m,"ObjectProvider",TypeAttributes.Public,obj,start);foreach(int target in new[]{2,3,4})m.AddInterfaceImplementation(provider,MetadataTokens.TypeDefinitionHandle(target));
    Method(m,"IgnoredCandidateSignature",MethodAttributes.Public,new byte[]{0x20,1});start++;
    var sdk=m.AddAssemblyReference(m.GetOrAddString("Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,default);var sdkFactory=m.AddTypeReference(sdk,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("ISdkFactory"));
    var cross=ProviderType(m,"CrossAssemblyProvider",TypeAttributes.Public,obj,start);m.AddInterfaceImplementation(cross,sdkFactory);
   }else{
    var provider=ProviderType(m,"InternalProvider",TypeAttributes.NotPublic,MetadataTokens.TypeDefinitionHandle(2),start);m.AddInterfaceImplementation(provider,MetadataTokens.TypeDefinitionHandle(3));m.AddInterfaceImplementation(provider,MetadataTokens.TypeDefinitionHandle(4));
    Method(m,"IgnoredCandidateSignature",MethodAttributes.Public,new byte[]{0x20,1});start++;
    ProviderType(m,"AbstractProvider",TypeAttributes.Public|TypeAttributes.Abstract,MetadataTokens.TypeDefinitionHandle(2),start);
    ProviderType(m,"IndirectProvider",TypeAttributes.Public,provider,start);
    foreach(int foreign in new[]{0,1,2}){
     var owner=m.AddAssemblyReference(m.GetOrAddString(foreign==0?"Owned.Foreign":"Knx.Ets.Sdk"),new Version(foreign==1?2:1,0,0,0),default,foreign==2?m.GetOrAddBlob(new byte[]{1,2,3,4,5,6,7,8}):default,0,default);
     var target=m.AddTypeReference(owner,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("ISdkFactory"));var collision=ProviderType(m,"ForeignOwner"+foreign,TypeAttributes.Public,obj,start);m.AddInterfaceImplementation(collision,target);
    }
    var own=m.AddAssemblyReference(m.GetOrAddString("Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,default);var prefix=m.AddTypeReference(own,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("ISdkFactoryExtra"));
    var prefixProvider=ProviderType(m,"PrefixProvider",TypeAttributes.Public,obj,start);m.AddInterfaceImplementation(prefixProvider,prefix);
    var generic=m.AddTypeReference(own,m.GetOrAddString("Owned"),m.GetOrAddString("GenericBase`1"));var sig=new BlobBuilder();sig.WriteByte(0x15);sig.WriteByte(0x12);sig.WriteCompressedInteger((MetadataTokens.GetRowNumber(generic)<<2)|1);sig.WriteByte(1);sig.WriteByte(0x0e);
    ProviderType(m,"GenericProvider",TypeAttributes.Public,m.AddTypeSpecification(m.GetOrAddBlob(sig)),start);
   }
  }
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void CreateProviderBounds(string path,int candidates,int interfacesPerCandidate=0,bool extraEdge=false){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedProviderBounds.dll"),m.GetOrAddGuid(new Guid("00960000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString("Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  foreach(string name in new[]{"ExecutionContext","ISdkFactory","ISdkContext"})m.AddTypeDefinition(TypeAttributes.NotPublic|(name=="ExecutionContext"?TypeAttributes.Abstract:TypeAttributes.Interface|TypeAttributes.Abstract),m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString(name),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  for(int i=0;i<candidates;i++){
   var type=ProviderType(m,"Candidate"+i,TypeAttributes.NotPublic,MetadataTokens.TypeDefinitionHandle(2),1);
   for(int j=0;j<interfacesPerCandidate;j++)m.AddInterfaceImplementation(type,MetadataTokens.TypeDefinitionHandle(3+j));
   if(extraEdge&&i==0)m.AddInterfaceImplementation(type,MetadataTokens.TypeDefinitionHandle(4));
  }
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void CreateServices(string path,bool objectModel=false,int profile=0,int methodRows=3,int typeRows=4){
  if(methodRows<3||methodRows>65537||typeRows<4||typeRows>32769)throw new ArgumentOutOfRangeException();
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedServices.dll"),m.GetOrAddGuid(new Guid("00940000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  var iface=TypeAttributes.Interface|TypeAttributes.Abstract;
  if(objectModel){
   var external=m.AddTypeReference(runtime,m.GetOrAddString("Owned"),m.GetOrAddString("IUnselected"));
   var factory=m.AddTypeDefinition(TypeAttributes.Public|iface,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString("IFactory"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));m.AddInterfaceImplementation(factory,external);
   Method(m,"CreateContext",MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual,new byte[]{0x20,0,0x12,16});
   m.AddTypeDefinition(TypeAttributes.Public|iface,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString(profile==1?"OtherStore":"IProjectStore"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(2));
   Method(m,"get_Factory",MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual,new byte[]{0x20,0,0x12,8});
   m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString("IObjectContext"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(3));
   Method(m,"get_Store",MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual,new byte[]{0x20,0,0x12,12});
   if(profile==2)m.AddTypeDefinition(TypeAttributes.Public|iface,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString("IFactory"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(4));
   else{
    int types=4;
    if(methodRows>3){m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("MethodPadding"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(4));types++;}
    for(int i=3;i<methodRows;i++)Method(m,"Padding",MethodAttributes.Public,new byte[]{0x20,0,1});
    for(int i=types;i<typeRows;i++)m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("TypePadding"+i),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(methodRows+1));
   }
  }else{
   var om=m.AddAssemblyReference(m.GetOrAddString("Knx.Ets.ObjectModel"),new Version(1,0,0,0),default,default,0,default);
   m.AddTypeReference(om,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString("IProjectStore"));
   m.AddTypeReference(om,m.GetOrAddString("Knx.Ets.ObjectModel"),m.GetOrAddString("IFactory"));
   m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Base"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   m.AddTypeDefinition(TypeAttributes.NotPublic|TypeAttributes.Sealed,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("ExecutionContext"),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   Method(m,".ctor",MethodAttributes.Assembly|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,new byte[]{0x20,1,1,0x12,16});
   Method(m,"get_Store",MethodAttributes.Public,new byte[]{0x20,0,0x12,9});
   Method(m,"ParameterOnly",MethodAttributes.Public,new byte[]{0x20,1,1,0x12,13});
   Method(m,"Hidden",MethodAttributes.Private,new byte[]{0x20,0,1});
   int extra=profile==3?257:0;for(int i=0;i<extra;i++)Method(m,"OverLimit",MethodAttributes.Public,new byte[]{0x20,0,1});
   m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString(profile==1?"OtherFactory":"ISdkFactory"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(5+extra));
   Method(m,"get_Context",MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual,new byte[]{0x20,0,0x12,12});
   if(profile==2)m.AddTypeDefinition(TypeAttributes.NotPublic|iface,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("ISdkFactory"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(6+extra));
  }
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void CreateOversizedMethodTable(string path){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedOversized.dll"),m.GetOrAddGuid(new Guid("00920000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString("Knx.Ets.ObjectModel"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Oversized"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  for(int i=0;i<32769;i++)Method(m,"OwnedMethod",MethodAttributes.Public,new byte[]{0x20,0,1});
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void CreateCountedConstruction(string path,int methodRows,int typeRows=3,bool objectModel=true){
  if(methodRows<0||methodRows>65537||typeRows<3||typeRows>32769)throw new ArgumentOutOfRangeException();
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedCounted.dll"),m.GetOrAddGuid(new Guid("00930000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(objectModel?"Knx.Ets.ObjectModel":"Owned.Other"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.ObjectModel.Project"),m.GetOrAddString("Project"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  // Padding belongs only to unselected types, preserving the selected-method budget.
  for(int i=2;i<typeRows;i++)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Padding"+i),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(i==2?1:methodRows+1));
  for(int i=0;i<methodRows;i++)Method(m,"OwnedMethod",MethodAttributes.Public,new byte[]{0x20,0,1});
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 static void Method(MetadataBuilder m,string name,MethodAttributes attributes,byte[] signature){
  m.AddMethodDefinition(attributes,MethodImplAttributes.IL,m.GetOrAddString(name),m.GetOrAddBlob(signature),0,MetadataTokens.ParameterHandle(1));
 }
 public static void CreateConstruction(string path,bool objectModel=false,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedConstruction.dll"),m.GetOrAddGuid(new Guid("00910000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(objectModel?"Knx.Ets.ObjectModel":"Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Base"),profile==5?MetadataTokens.TypeDefinitionHandle(3):obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  var ctor=MethodAttributes.SpecialName|MethodAttributes.RTSpecialName;
  if(objectModel){
   m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.ObjectModel.Project"),m.GetOrAddString(profile==3?"Other":"Project"),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   Method(m,".ctor",MethodAttributes.Public|ctor,new byte[]{0x20,0,1});
   Method(m,"Create",MethodAttributes.Public|MethodAttributes.Static,new byte[]{0,0,0x12,12});
   if(profile==4)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.ObjectModel.Project"),m.GetOrAddString("Project"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(3));
  }else{
   var om=m.AddAssemblyReference(m.GetOrAddString("Knx.Ets.ObjectModel"),new Version(1,0,0,0),default,default,0,default);
   m.AddTypeReference(om,m.GetOrAddString("Knx.Ets.ObjectModel.Project"),m.GetOrAddString("Project"));
   m.AddTypeReference(runtime,m.GetOrAddString("System.Collections.Generic"),m.GetOrAddString("IEnumerable`1"));
   var foreign=m.AddAssemblyReference(m.GetOrAddString("Owned.Foreign"),new Version(1,0,0,0),default,default,0,default);
   m.AddTypeReference(foreign,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString("Project"));
   m.AddTypeDefinition(TypeAttributes.Public|TypeAttributes.Sealed,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString("Project"),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
   Method(m,".ctor",MethodAttributes.Assembly|ctor,profile==6?new byte[]{0x20,1}:new byte[]{0x20,3,1,0x12,16,0x12,9,0x12,20});
   m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString("ProjectCollection"),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(2));
   Method(m,".ctor",MethodAttributes.Public|ctor,new byte[]{0x20,0,1});
   Method(m,"Create",MethodAttributes.Public,new byte[]{0x20,0,0x12,12});
   Method(m,"ParameterOnly",MethodAttributes.Public,new byte[]{0x20,1,0x0e,0x12,12});
   Method(m,"GetRoot",MethodAttributes.Family,new byte[]{0x20,0,0x12,24});
   Method(m,"HiddenFactory",MethodAttributes.Private,new byte[]{0x20,0,0x12,12});
   m.AddTypeDefinition(TypeAttributes.NotPublic|TypeAttributes.Interface|TypeAttributes.Abstract,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString(profile==1?"OtherContext":"ISdkContext"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(7));
   Method(m,"get_Root",MethodAttributes.Public|MethodAttributes.Abstract|MethodAttributes.Virtual,new byte[]{0x20,0,0x12,24});
   m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("Root"),MetadataTokens.TypeDefinitionHandle(2),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(8));
   Method(m,".ctor",MethodAttributes.Public|ctor,new byte[]{0x20,0,1});
   Method(m,"GetProjects",MethodAttributes.Public,new byte[]{0x20,0,0x12,16});
   Method(m,"GetNested",MethodAttributes.Public,new byte[]{0x20,0,0x15,0x12,13,1,0x12,12});
   Method(m,"GetForeign",MethodAttributes.Public,new byte[]{0x20,0,0x12,17});
   Method(m,"GetPrefix",MethodAttributes.Public,new byte[]{0x20,0,0x12,28});
   m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString(profile==7?"ProjectCollection>":"ProjectCollectionFactoryValue"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(13));
   Method(m,".ctor",MethodAttributes.Public|ctor,new byte[]{0x20,0,1});
   if(profile==2)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk"),m.GetOrAddString("Root"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(14));
  }
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 // Task-009 images contain only declarations; even constructors have no bodies.
 public static void CreateProject(string path,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("OwnedProject.dll"),m.GetOrAddGuid(new Guid("00900000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString("Knx.Ets.Sdk"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  int bases=profile==4?16:1;
  for(int i=0;i<bases;i++)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Owned"),m.GetOrAddString("Base"+i),i==0?(profile==3?MetadataTokens.TypeDefinitionHandle(bases+2):obj):MetadataTokens.TypeDefinitionHandle(i+1),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  m.AddTypeDefinition(profile==6?TypeAttributes.NotPublic:TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString(profile==2?"Other":"Project"),MetadataTokens.TypeDefinitionHandle(bases+1),MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  MethodAttributes[] access={MethodAttributes.Private,MethodAttributes.Family,MethodAttributes.Public};
  foreach(var a in access)m.AddMethodDefinition(a|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,MethodImplAttributes.IL,m.GetOrAddString(".ctor"),m.GetOrAddBlob(new byte[]{0x20,0,1}),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Private|MethodAttributes.Static|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,MethodImplAttributes.IL,m.GetOrAddString(".cctor"),m.GetOrAddBlob(new byte[]{0,0,1}),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Public,MethodImplAttributes.IL,m.GetOrAddString("get_Name"),m.GetOrAddBlob(profile==5?new byte[]{0x20,1}:new byte[]{0x20,0,0x0e}),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Family,MethodImplAttributes.IL,m.GetOrAddString("Changed"),m.GetOrAddBlob(new byte[]{0x20,0,1}),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Private,MethodImplAttributes.IL,m.GetOrAddString("Hidden"),m.GetOrAddBlob(new byte[]{0x20,0,1}),0,MetadataTokens.ParameterHandle(1));
  if(profile==1)m.AddTypeDefinition(TypeAttributes.Public,m.GetOrAddString("Knx.Ets.Sdk.Project"),m.GetOrAddString("Project"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(8));
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
 public static void Create(string path,bool contract=false,int profile=0){
  var m=new MetadataBuilder();m.AddModule(0,m.GetOrAddString("Owned.dll"),m.GetOrAddGuid(new Guid("14400000-1111-1111-1111-111111111111")),default,default);
  m.AddAssembly(m.GetOrAddString(contract?"OwnedContracts":"OwnedDca"),new Version(1,0,0,0),default,default,0,AssemblyHashAlgorithm.None);
  var runtime=m.AddAssemblyReference(m.GetOrAddString("mscorlib"),new Version(4,0,0,0),default,default,0,default);
  var views=m.AddAssemblyReference(m.GetOrAddString("Knx.Ets.Sdk.AddIns.AddInViews"),new Version(1,0,0,0),default,default,0,default);
  var obj=m.AddTypeReference(runtime,m.GetOrAddString("System"),m.GetOrAddString("Object"));
  var iface=m.AddTypeReference(views,m.GetOrAddString("Knx.Ets.Sdk.AddIns.AddInViews"),m.GetOrAddString("IEts4AddIn"));
  m.AddTypeDefinition(TypeAttributes.NotPublic,default,m.GetOrAddString("<Module>"),default,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  var owner=m.AddTypeDefinition(TypeAttributes.Public|(contract?TypeAttributes.Interface|TypeAttributes.Abstract:0),m.GetOrAddString(contract?"Knx.Ets.Sdk.AddIns.AddInViews":"Owned"),m.GetOrAddString(contract?"IInitializationContext":"Dca"),contract?default:obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(1));
  if(!contract)m.AddInterfaceImplementation(owner,iface);
  byte[] signature={0x20,0,1};if(profile==1)signature=new byte[]{0x20,0x80,0x80,1};if(profile==2)signature=new byte[]{0x20,1};if(profile==3)signature=new byte[129];
  if(profile==4){signature=new byte[]{0x20,0,0x15,0x12,5,0x80,0x80};}
  m.AddMethodDefinition(MethodAttributes.Public|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,MethodImplAttributes.IL,m.GetOrAddString(".ctor"),m.GetOrAddBlob(signature),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Private|MethodAttributes.Virtual|MethodAttributes.Final,MethodImplAttributes.IL,m.GetOrAddString("Owned.Contract.Callback"),m.GetOrAddBlob(new byte[]{0x20,1,1,0x0e}),0,MetadataTokens.ParameterHandle(1));
  m.AddMethodDefinition(MethodAttributes.Private,MethodImplAttributes.IL,m.GetOrAddString("Hidden"),m.GetOrAddBlob(new byte[]{0x20,0,1}),0,MetadataTokens.ParameterHandle(1));
  var hidden=m.AddTypeDefinition(TypeAttributes.NotPublic,m.GetOrAddString("Owned"),m.GetOrAddString("HiddenParent"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(4));
  var nested=m.AddTypeDefinition(TypeAttributes.NestedPublic,default,m.GetOrAddString("HiddenNested"),obj,MetadataTokens.FieldDefinitionHandle(1),MetadataTokens.MethodDefinitionHandle(4));m.AddNestedType(nested,hidden);
  var image=new BlobBuilder();new ManagedPEBuilder(new PEHeaderBuilder(),new MetadataRootBuilder(m),new BlobBuilder(),flags:CorFlags.ILOnly).Serialize(image);File.WriteAllBytes(path,image.ToArray());
 }
}
