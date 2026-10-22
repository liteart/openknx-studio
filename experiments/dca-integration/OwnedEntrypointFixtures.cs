// Synthetic original metadata images, no method bodies or vendor bytes.
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
public static class Task144OwnedFixtures {
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
