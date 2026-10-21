// Synthetic original metadata images, no method bodies or vendor bytes.
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
public static class Task144OwnedFixtures {
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
