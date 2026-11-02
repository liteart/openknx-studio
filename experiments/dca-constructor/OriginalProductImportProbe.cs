// Original SDK import orchestration only. No owned product/cache format implementation.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;

internal sealed class OriginalProductImportProbe : IDisposable
{
    internal const string Mode = "original-sdk-product-store-fixture";
    internal const string ArchiveFile = "SCN-DA64x-04_MDT_KP_V40a.knxprod";
    internal const string ArchiveHash = "0cf819f5a9148bdfd5cd9c63d96e6c4eef273ec3593aec218cb7ebe04320b3cc";
    internal const int ArchiveSize = 254893;
    internal const string DependencyFile = "DotNetZip.dll";
    internal const int DependencySize = 252928;
    internal const string DependencyHash = "b0f7707b0b0d83ea11d9b683f00a3e05ecd1836e5463408b937e1d87e5ecdf60";
    internal const string DependencyIdentity = "DotNetZip, Version=1.17.0.0, Culture=neutral, PublicKeyToken=6583c7c814667745";
    internal const string CodePagesFile = "System.Text.Encoding.CodePages.dll";
    internal const int CodePagesSize = 766216;
    internal const string CodePagesHash = "a01c215594d3a98d11bc4a776d4767536e004aac1f2c3d16e943b12d19540680";
    internal const string CodePagesIdentity = "System.Text.Encoding.CodePages, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    internal const string CatalogAbiFile = "Knx.Ets.OnlineCatalog.Download.dll";
    internal const int CatalogAbiSize = 234960;
    internal const string CatalogAbiHash = "55f6675ca3c441da326bcfa0c5c47907dc94f81bb0583fe323e294f366abcdc1";
    internal const string CatalogAbiIdentity = "Knx.Ets.OnlineCatalog.Download, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    internal const string XmlModelFile = "Knx.Ets.Xml.ObjectModel.dll";
    internal const int XmlModelSize = 4967888;
    internal const string XmlModelHash = "db6f21679d046eaef4909121a553af52fa377fa582552faca6ec0fed5f7f51e1";
    internal const string XmlModelIdentity = "Knx.Ets.Xml.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    internal const string CvNextFile = "Knx.Ets.CvNext.dll";
    internal const int CvNextSize = 5620176;
    internal const string CvNextHash = "8714bd0ce0bb698c2e99de9b6b78b70b05a2ee8affdbad0f5ae01c8a4f8fac3f";
    internal const string CvNextIdentity = "Knx.Ets.CvNext, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    internal const string ExtractFile = "Knx.Ets.InformationExtract.dll";
    internal const int ExtractSize = 507344;
    internal const string ExtractHash = "2b182e9de42899fe4e749e461d4e77d76522d71fdec0a7a7bdb95e1824706c00";
    internal const string ExtractIdentity = "Knx.Ets.InformationExtract, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    internal const string SigningFile = "Knx.Ets.XmlSigning.dll";
    internal const int SigningSize = 268240;
    internal const string SigningHash = "2a9e240f12635f43b57030152a9ea143a9fc5fe2b70dd2e4c528708d972eb350";
    internal const string SigningIdentity = "Knx.Ets.XmlSigning, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    internal const string CryptoXmlFile = "System.Security.Cryptography.Xml.dll";
    internal const int CryptoXmlSize = 57648;
    internal const string CryptoXmlHash = "86499f4fe918cb3b20757b44115b6d5545dec4162a01f5acadb3aa9daebd04c1";
    internal const string CryptoXmlIdentity = "System.Security.Cryptography.Xml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
    internal const string LoggingFile = "Microsoft.Extensions.Logging.Abstractions.dll";
    internal const int LoggingSize = 69384;
    internal const string LoggingHash = "c071f7bb1dac06185232727d8b43412537d856a7ee23b8d128f45d5774b70270";
    internal const string LoggingIdentity = "Microsoft.Extensions.Logging.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60";
    internal const string LoggingLegacyIdentity = "Microsoft.Extensions.Logging.Abstractions, Version=2.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60";
    const string MasterHash = "d78681374d9828095d2c05d16105474818a62e4f3bd415f6366142b24208f705";
    const string OmIdentity = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string CommonIdentity = "Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string AutofacIdentity = "Autofac, Version=8.2.0.0, Culture=neutral, PublicKeyToken=17863af14b0044da";
    readonly Action<string> record;
    readonly object progressGate=new object(),treeGate=new object();
    bool progressActive,progressEntered;
    int progressAccepted,progressBackground;
    readonly string root;
    readonly int owner = Thread.CurrentThread.ManagedThreadId;
    readonly List<object> retained = new List<object>();
    readonly List<Creator> creators = new List<Creator>();
    FileStream source, convertedSource;
    object zip, convertedZip, container, caches, target;
    string convertedHash, conversionTemporaryRoot;
    bool completed, disposed, faulted, failureCaptured;
    int callbackFailureRecorded;
    string currentStage = "PREPARE";
    int cacheReplies, progressCalls, directoriesCreated;
    int uiCacheLookups,treeChecks;long uiLastTreeTicks,treeTicks;
    Type manufacturerType;
    MethodInfo hardware, application, manufacturerProvider, dynamicTranslations, parameterRefs, comObjectRefs, calculatedParameters,cacheWeakGetter,cacheWeakSetter,cacheRelease;int cacheWeakCalls,cacheReleases;
    Type manufacturerProviderType, applicationStoreType;MethodInfo applicationStore;int applicationStores;
    MethodInfo applicationPrograms,hardwareStore,baggageStore,applicationManufacturer,targetFilePath;Type applicationListType;int manufacturerStoreReads;
    ConstructorInfo projectSerializationCtor;object[] projectSerializationArgs;object projectSerializationStore;MethodInfo[] projectStateGetters;MethodInfo projectOriginalGetter,projectSerializationPath;int projectStateReads;
    Creator projectStreamCreator;ConstructorInfo projectLargeStringCtor;object projectStringStream,projectLargeStrings,projectContentStore;MethodInfo projectAsWriter;int projectWriterCalls;

    // Qualified by PE-only identity/bootstrap records; runtime is still an observation.
    internal static void RequireDependencyReady(bool complete)
    { ProbeFiles.Require(complete, "SDK_PRODUCT_IMPORT_QUALIFIED_DEPENDENCY_REQUIRED"); }

    internal OriginalProductImportProbe(string archive, string scratch, Assembly om, Assembly common,
        Assembly autofac, object factory, Action<string> output)
    {
        record = output;
        ProbeFiles.Require(om.FullName == OmIdentity && common.FullName == CommonIdentity && autofac.FullName == AutofacIdentity,
            "PRODUCT_IMPORT_PINNED_ASSEMBLIES");
        source = new FileStream(ProbeFiles.Local(archive, true, false), FileMode.Open, FileAccess.Read, FileShare.Read);
        try {
            ProbeFiles.Require(Path.GetFileName(archive) == ArchiveFile && source.Length == 254893 && ProbeFiles.Hash(source) == ArchiveHash,
                "PRODUCT_IMPORT_ARCHIVE_PIN");
            root = Path.Combine(ProbeFiles.Local(scratch, true, true), "original-product-store");
            ProbeFiles.Local(root, false, true);
            Prepare(om, common, autofac, factory);
        } catch (Exception error) {
            faulted = true;
            CaptureFailure(error);
            try { Dispose(); } catch (Exception cleanup) { record("PRODUCT_IMPORT_CONSTRUCTOR_CLEANUP_FAILURE type=" + cleanup.GetType().FullName); }
            throw;
        }
    }
    static void RequireObject(object value, Type type, string code)
    { ProbeFiles.Require(value != null && !RemotingServices.IsTransparentProxy(value) && value.GetType() == type, code); }
    static Type OriginalType(Assembly assembly, string name, int token)
    {
        Type value = assembly.GetType(name, true, false);
        ProbeFiles.Require(value.Assembly == assembly && value.Module == assembly.ManifestModule && value.MetadataToken == token
            && !value.IsGenericType && !value.IsAbstract && value.IsClass, "PRODUCT_IMPORT_ORIGINAL_TYPE:" + name);
        return value;
    }
    static ConstructorInfo Constructor(Type type, int token, params Type[] parameters)
    {
        ConstructorInfo value = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, parameters, null);
        ProbeFiles.Require(value != null && value.DeclaringType == type && value.MetadataToken == token && value.IsPublic
            && !value.IsStatic && value.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis),
            "PRODUCT_IMPORT_ORIGINAL_CONSTRUCTOR:" + type.Name);
        return value;
    }
    static MethodInfo Method(Type type, string name, int token, Type result, params Type[] parameters)
    {
        MethodInfo value = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, parameters, null);
        ProbeFiles.Require(value != null && value.DeclaringType == type && value.MetadataToken == token && value.ReturnType == result
            && !value.IsGenericMethod && value.IsPublic && !value.IsStatic, "PRODUCT_IMPORT_ORIGINAL_METHOD:" + name);
        return value;
    }
    static void Optional(ParameterInfo value, object expected)
    { ProbeFiles.Require(value.IsOptional && value.HasDefaultValue && Object.Equals(value.RawDefaultValue, expected), "PRODUCT_IMPORT_ORIGINAL_DEFAULT"); }

    void Prepare(Assembly om, Assembly common, Assembly autofac, object factory)
    {
        Type productStore = om.GetType("Knx.Ets.ObjectModel.IProductStore", true), store = om.GetType("Knx.Ets.ObjectModel.IStore", true);
        Type streamStore = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true), cacheStore = om.GetType("Knx.Ets.ObjectModel.ICacheStreamStore", true);
        Type writerContract = om.GetType("Knx.Ets.ObjectModel.FileSystemStore.IInstallationGuidWriter", true);
        Type factoryContract = om.GetType("Knx.Ets.ObjectModel.IFactory", true), cacheContract = om.GetType("Knx.Ets.ObjectModel.ICacheProvider", true);
        ProbeFiles.Require(factory != null && !RemotingServices.IsTransparentProxy(factory) && factoryContract.IsInstanceOfType(factory), "PRODUCT_IMPORT_GENUINE_FACTORY");
        Type zipType = OriginalType(common, "Knx.Ets.Common.Zip.ZipWrapper", 0x02000027), zipContract = common.GetType("Knx.Ets.Common.Zip.IZipWrapper", true);
        ConstructorInfo zipCtor = Constructor(zipType, 0x06000141, typeof(Stream), typeof(System.Text.Encoding)); Optional(zipCtor.GetParameters()[1], null);
        Type sourceType = OriginalType(om, "Knx.Ets.ObjectModel.ZipContainerStore.ZipContainerProductStore", 0x020000bb);
        ConstructorInfo sourceCtor = Constructor(sourceType, 0x06000680, zipContract, typeof(bool)); Optional(sourceCtor.GetParameters()[1], false);
        Type guidType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.InstallationGuidWriter", 0x02000ddb);
        ConstructorInfo guidCtor = Constructor(guidType, 0x0600869e, typeof(DirectoryInfo));
        Type singleType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.SingleFileStore", 0x02000de4);
        Type fileStreamType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.FileStreamStore", 0x02000dcd);
        Type cacheType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.CacheFileStreamStore", 0x02000da7);
        MethodInfo manufacturerGetter = cacheStore.GetMethod("get_ManufacturerId");
        ProbeFiles.Require(manufacturerGetter != null && manufacturerGetter.MetadataToken == 0x060001cb && manufacturerGetter.Module == om.ManifestModule && manufacturerGetter.GetParameters().Length == 0, "PRODUCT_IMPORT_MANUFACTURER_GETTER");
        manufacturerType = manufacturerGetter.ReturnType;
        ProbeFiles.Require(manufacturerType != null && manufacturerType.FullName == "Knx.Ets.Ids.Product.ManufacturerId"
            && manufacturerType.Assembly.FullName == "Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", "PRODUCT_IMPORT_MANUFACTURER_ABI");
        ConstructorInfo singleCtor = Constructor(singleType, 0x060086e4, typeof(DirectoryInfo), writerContract, typeof(string), typeof(bool)); Optional(singleCtor.GetParameters()[3], true);
        ConstructorInfo streamCtor = Constructor(fileStreamType, 0x060085fa, typeof(DirectoryInfo), typeof(string), typeof(bool)); Optional(streamCtor.GetParameters()[2], true);
        projectLargeStringCtor=Constructor(OriginalType(om,"Knx.Ets.ObjectModel.Caching.LargeStringCache",0x02000f24),0x0600914f,streamStore);
        ConstructorInfo cacheCtor = Constructor(cacheType, 0x06008442, typeof(DirectoryInfo), manufacturerType, typeof(string), typeof(bool)); Optional(cacheCtor.GetParameters()[3], true);
        Type funcStore = typeof(Func<,,>).MakeGenericType(typeof(DirectoryInfo), typeof(string), store);
        Type funcStream = typeof(Func<,,>).MakeGenericType(typeof(DirectoryInfo), typeof(string), streamStore);
        Type funcCache = typeof(Func<,,,>).MakeGenericType(typeof(DirectoryInfo), manufacturerType, typeof(string), cacheStore);
        Type metaType = autofac.GetType("Autofac.Features.Metadata.Meta`1", true).MakeGenericType(funcStore);
        Type storeCreators = typeof(IEnumerable<>).MakeGenericType(metaType);
        Type fileStoreType=om.GetType("Knx.Ets.ObjectModel.FileSystemStore.FileStore",true,false);
        ProbeFiles.Require(fileStoreType.Assembly==om&&fileStoreType.Module==om.ManifestModule&&fileStoreType.MetadataToken==0x02000dcc&&(int)fileStoreType.Attributes==1048704&&fileStoreType.IsAbstract&&fileStoreType.IsNotPublic&&fileStoreType.IsClass&&!fileStoreType.IsGenericType,"PRODUCT_CONTENT_ORIGINAL_ABSTRACT_WRITER_BASE");
        projectAsWriter=Method(fileStoreType,"AsWriter",0x060085ec,om.GetType("Knx.Ets.ObjectModel.IWriter",true));
        Type targetType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.FileProductStore", 0x02000db6);
        ConstructorInfo targetCtor = Constructor(targetType, 0x060084ec, typeof(DirectoryInfo), storeCreators, funcStream, funcCache);
        Type projectStoreType=OriginalType(om,"Knx.Ets.ObjectModel.FileSystemStore.FileProjectStore",0x02000dca);
        projectSerializationCtor=Constructor(projectStoreType,0x060085a4,typeof(DirectoryInfo),storeCreators,funcStream,funcCache,writerContract);
        projectOriginalGetter=Method(projectStoreType,"get_Project",0x060085ab,store);projectSerializationPath=Method(projectStoreType,"get_FilePath",0x060085c9,typeof(string));
        int[] saveTokens={0x060085af,0x060085b0,0x060085b3,0x060085b4,0x060085b5,0x060085b6,0x060085b7,0x060085b8,0x060085c4,0x060085c5,0x060085c6,0x060085b1,0x060085b2,0x060085ae,0x060085b9,0x060085ba,0x060085bb,0x060085bc};
        string[] saveNames={"get_HistoryEntries","get_DeviceCertificates","get_ProjectTraces","get_IpSecurityDomains","get_GroupAddresses","get_BusAccess","get_Buildings","get_Areas","get_Trades","get_SplitInfos","get_UnassignedDevices","get_UserFiles","get_AddinDatas","get_Installation","get_AllAreas","get_AllUserFiles","get_AllDeviceBinaryData","get_AllAddInData"};
        string[] enumerationIds={"Knx.Ets.Ids.Project.AreaId","Knx.Ets.Ids.Project.UserFileId","Knx.Ets.Ids.Id","Knx.Ets.Ids.Project.AddinDataId"};
        projectStateGetters=new MethodInfo[18];for(int i=0;i<saveTokens.Length;i++){Type result=i<14?store:typeof(IEnumerable<>).MakeGenericType(manufacturerType.Assembly.GetType(enumerationIds[i-14],true,false));projectStateGetters[i]=Method(projectStoreType,saveNames[i],saveTokens[i],result);}
        applicationStore=Method(targetType,"ApplicationProgram",0x06008503,store,manufacturerType.Assembly.GetType("Knx.Ets.Ids.Product.ApplicationProgramId",true));applicationStoreType=singleType;
        Type applicationType=applicationStore.GetParameters()[0].ParameterType;applicationListType=typeof(List<>).MakeGenericType(applicationType);
        applicationPrograms=Method(targetType,"ApplicationPrograms",0x0600850a,typeof(IEnumerable<>).MakeGenericType(applicationType),manufacturerType);
        hardwareStore=Method(targetType,"Hardware",0x06008501,store,manufacturerType);baggageStore=Method(targetType,"Baggages",0x06008502,store,manufacturerType);
        targetFilePath=Method(targetType,"get_FilePath",0x0600850e,typeof(string));
        applicationManufacturer=applicationType.GetMethod("get_ManufacturerId",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
        ProbeFiles.Require(applicationManufacturer!=null&&applicationManufacturer.Module==applicationType.Module&&applicationManufacturer.ReturnType==manufacturerType&&!applicationManufacturer.IsStatic&&!applicationManufacturer.IsGenericMethod,"PRODUCT_APPLICATION_ENUMERATION_ID_ABI");
        Type masterStoreType = OriginalType(om, "Knx.Ets.ObjectModel.FileSystemStore.StoreMasterDataStore", 0x02000de6);
        ConstructorInfo masterStoreCtor = Constructor(masterStoreType, 0x060086f1, store);
        Type masterType = OriginalType(om, "Knx.Ets.ObjectModel.MasterData.KnxMasterData", 0x02000c45);
        Type masterContract = om.GetType("Knx.Ets.ObjectModel.IMasterDataStore", true);
        ConstructorInfo masterCtor = Constructor(masterType, 0x060078a7, masterContract, factoryContract, writerContract);
        MethodInfo load = Method(masterType, "Load", 0x060078aa, typeof(void));
        Type cachesType = OriginalType(om, "Knx.Ets.ObjectModel.Import.Caches", 0x02000cd2);
        cacheWeakGetter=Method(cachesType,"get_AllowCachesToGoWeak",0x06007d0a,typeof(bool));cacheWeakSetter=Method(cachesType,"set_AllowCachesToGoWeak",0x06007d0b,typeof(void),typeof(bool));
        cacheRelease=Method(cachesType,"DisposeCachedInformation",0x06007d06,typeof(void));
        ConstructorInfo cachesCtor = Constructor(cachesType, 0x06007cf9, productStore);
        Type importerType = OriginalType(om, "Knx.Ets.ObjectModel.Import.ProductImporter", 0x02000d59);
        Type importerContract = om.GetType("Knx.Ets.ObjectModel.Import.IProductImporter", true);
        ConstructorInfo importerCtor = Constructor(importerType, 0x06008188, productStore, productStore, factoryContract, cacheContract,
            typeof(Action<>).MakeGenericType(importerContract), masterType);
        Optional(importerCtor.GetParameters()[4], null); Optional(importerCtor.GetParameters()[5], null);
        Type progress = typeof(Action<>).MakeGenericType(om.GetType("Knx.Ets.ObjectModel.ProgressEventArgs", true));
        MethodInfo import = Method(importerType, "Import", 0x06008189, typeof(void), typeof(Predicate<string>), typeof(Predicate<string>), progress, productStore, typeof(bool));
        ParameterInfo[] defaults = import.GetParameters(); for (int i = 0; i < 4; i++) Optional(defaults[i], null); Optional(defaults[4], false);
        hardware = Method(cachesType, "HardwareCache", 0x06007cfb, om.GetType("Knx.Ets.ObjectModel.Caching.HardwareCache", true), manufacturerType);
        application = cachesType.GetMethod("ApplicationProgramCache", new Type[] { manufacturerType });
        ProbeFiles.Require(cacheContract.IsAssignableFrom(cachesType) && application != null && application.Module == om.ManifestModule
            && application.IsPublic && !application.IsStatic && application.ReturnType.FullName == "Knx.Ets.ObjectModel.Caching.ApplicationProgramCache", "PRODUCT_IMPORT_CACHE_ABI");
        Type providerContract=om.GetType("Knx.Ets.ObjectModel.IManufacturerCacheProvider",true);
        manufacturerProviderType=OriginalType(om,"Knx.Ets.ObjectModel.ManufacturerCacheProvider",0x0200006e);
        ProbeFiles.Require(providerContract.IsPublic&&providerContract.IsInterface&&!providerContract.IsGenericType&&providerContract.IsAssignableFrom(manufacturerProviderType),"PRODUCT_IMPORT_MANUFACTURER_PROVIDER_TYPE");
        manufacturerProvider=Method(cachesType,"CacheProvider",0x06007d05,providerContract,manufacturerType);
        dynamicTranslations=Method(cachesType,"GetDynamicTranslations",0x06007d02,typeof(Dictionary<string,string>),manufacturerType.Assembly.GetType("Knx.Ets.Ids.Product.ApplicationProgramId",true));
        Type applicationId=dynamicTranslations.GetParameters()[0].ParameterType;
        parameterRefs=Method(cachesType,"GetParameterRefCache",0x06007cff,om.GetType("Knx.Ets.ObjectModel.Caching.ParameterRefCache",true),applicationId);
        comObjectRefs=Method(cachesType,"GetComObjectRefCache",0x06007cfd,om.GetType("Knx.Ets.ObjectModel.Caching.ComObjectRefCache",true),applicationId);
        calculatedParameters=Method(cachesType,"GetCalculatedParameterCache",0x06007d04,om.GetType("Knx.Ets.ObjectModel.Caching.CalculatedParameterCache",true),applicationId);
        record("PRODUCT_IMPORT_API_READY source=ORIGINAL_ARCHIVE target=ORIGINAL_FILE_STORE checks=True product-parser=ORIGINAL_ONLY");
        Directory.CreateDirectory(root); CheckTree();
        string products=Path.Combine(root,"products");ProbeFiles.Local(products,false,true);Directory.CreateDirectory(products);ProbeFiles.Local(products,true,true);CheckTree();
        source.Position = 0; zip = zipCtor.Invoke(new object[] { source, Type.Missing }); RequireObject(zip, zipType, "PRODUCT_IMPORT_ZIP_RESULT");
        object sourceStore = Keep(sourceCtor.Invoke(new object[] { zip, Type.Missing }));
        object guid = Keep(guidCtor.Invoke(new object[] { new DirectoryInfo(products) }));
        container = CreateContainer(autofac, writerContract, guid, singleType, store, fileStreamType, streamStore, cacheType, cacheStore);
        object actualMeta = Resolve(autofac, container, metaType);
        object actualStoreCreator = metaType.GetProperty("Value").GetValue(actualMeta, null);
        object metadata = metaType.GetProperty("Metadata").GetValue(actualMeta, null);
        ProbeFiles.Require(metadata is IDictionary<string, object> && actualStoreCreator != null && actualStoreCreator.GetType() == funcStore, "PRODUCT_IMPORT_ACTUAL_AUTOFAC_META");
        var single = new Creator(this, actualStoreCreator, singleType); creators.Add(single);
        object boundedStore = single.Bind(funcStore, "Create", store);
        object boundedMeta = metaType.GetConstructor(new Type[] { funcStore, typeof(IDictionary<string, object>) }).Invoke(new object[] { boundedStore, metadata });
        Array list = Array.CreateInstance(metaType, 1); list.SetValue(boundedMeta, 0);
        var stream = new Creator(this, Resolve(autofac, container, funcStream), fileStreamType); creators.Add(stream);projectStreamCreator=stream;
        var cache = new Creator(this, Resolve(autofac, container, funcCache), cacheType); creators.Add(cache);
        target = Keep(targetCtor.Invoke(new object[] { new DirectoryInfo(products), list, stream.Bind(funcStream, "Create", streamStore), cache.BindCache(funcCache, manufacturerType, cacheStore) }));
        projectSerializationArgs=new object[]{new DirectoryInfo(products),list,stream.Bind(funcStream,"Create",streamStore),cache.BindCache(funcCache,manufacturerType,cacheStore),guid};
        CheckTree(); record("PRODUCT_IMPORT_STORES_READY registrations=THREE_ORIGINAL_TYPES metadata=ACTUAL_AUTOFAC checks=True");
        currentStage = "MASTER_SOURCE"; record("PRODUCT_IMPORT_MASTER_SOURCE_BEGIN");
        object masterFile = Keep(Method(sourceType, "MasterData", 0x06000699, store).Invoke(sourceStore, new object[0]));
        record("PRODUCT_IMPORT_MASTER_SOURCE_END returned=True");
        currentStage = "MASTER_STREAM_OPEN"; record("PRODUCT_IMPORT_MASTER_STREAM_BEGIN");
        using (Stream bytes = (Stream)Method(streamStore, "AsStream", 0x06000368, typeof(Stream), typeof(bool)).Invoke(masterFile, new object[] { true })) {
            ProbeFiles.Require(bytes != null && bytes.CanRead, "PRODUCT_IMPORT_ARCHIVE_MASTER_STREAM");
            record("PRODUCT_IMPORT_MASTER_STREAM_END opened=True");
            currentStage = "MASTER_HASH"; record("PRODUCT_IMPORT_MASTER_HASH_BEGIN");
            using (var copy = new MemoryStream()) { byte[] buffer = new byte[65536]; int read; while ((read = bytes.Read(buffer, 0, buffer.Length)) != 0) { ProbeFiles.Require(copy.Length <= 1050824 - read, "PRODUCT_IMPORT_ARCHIVE_MASTER_SIZE"); copy.Write(buffer, 0, read); }
                copy.Position = 0; ProbeFiles.Require(copy.Length == 1050824 && ProbeFiles.Hash(copy) == MasterHash, "PRODUCT_IMPORT_ARCHIVE_MASTER_PIN"); }
            record("PRODUCT_IMPORT_MASTER_HASH_END verified=True"); currentStage = "MASTER_STREAM_CLOSE"; record("PRODUCT_IMPORT_MASTER_STREAM_CLOSE_BEGIN");
        }
        record("PRODUCT_IMPORT_MASTER_STREAM_CLOSE_END");
        currentStage="CONVERSION";string converted=ConvertArchive(om,common);
        convertedSource=new FileStream(ProbeFiles.Local(converted,true,false),FileMode.Open,FileAccess.Read,FileShare.Read);
        ProbeFiles.Require(convertedSource.Length>0&&convertedSource.Length<=16*1024*1024,"PRODUCT_IMPORT_CONVERTED_ARCHIVE_BOUND");convertedHash=ProbeFiles.Hash(convertedSource);convertedSource.Position=0;
        record("PRODUCT_IMPORT_CONVERTED_ARCHIVE bytes="+convertedSource.Length+" sha256="+convertedHash+" provenance=ORIGINAL_CVNEXT_DERIVATIVE source-unchanged=True");
        convertedZip=zipCtor.Invoke(new object[]{convertedSource,Type.Missing});RequireObject(convertedZip,zipType,"PRODUCT_IMPORT_CONVERTED_ZIP");
        sourceStore=Keep(sourceCtor.Invoke(new object[]{convertedZip,Type.Missing}));masterFile=Keep(Method(sourceType,"MasterData",0x06000699,store).Invoke(sourceStore,new object[0]));
        record("PRODUCT_IMPORT_MASTER_SOURCE_CONVERTED archive-hash=RECORDED input-master-hash=VERIFIED_BEFORE_CONVERSION");

        currentStage = "MASTER_STORE_CTOR"; record("PRODUCT_IMPORT_MASTER_STORE_CTOR_BEGIN token=060086F1");
        object masterStore = Keep(masterStoreCtor.Invoke(new object[] { masterFile }));
        record("PRODUCT_IMPORT_MASTER_STORE_CTOR_END returned=True");
        currentStage = "MASTER_DATA_CTOR"; record("PRODUCT_IMPORT_MASTER_DATA_CTOR_BEGIN token=060078A7");
        object master = Keep(masterCtor.Invoke(new object[] { masterStore, factory, guid }));
        record("PRODUCT_IMPORT_MASTER_DATA_CTOR_END returned=True");
        currentStage = "MASTER_LOAD";
        record("PRODUCT_IMPORT_MASTER_BEGIN source=ARCHIVE_KNX_MASTER"); load.Invoke(master, new object[0]); CheckTree(); record("PRODUCT_IMPORT_MASTER_END returned=True");
        ProjectConstructorProbe.BindProductMaster(master);
        currentStage = "CACHES_CTOR"; record("PRODUCT_IMPORT_CACHES_CTOR_BEGIN token=06007CF9 purpose=ORIGINAL_LOCAL_IMPORT");
        caches = Keep(cachesCtor.Invoke(new object[] { target }));
        record("PRODUCT_IMPORT_CACHES_CTOR_END returned=True");
        currentStage = "IMPORTER_CTOR"; record("PRODUCT_IMPORT_IMPORTER_CTOR_BEGIN token=06008188");
        object importer = Keep(importerCtor.Invoke(new object[] { sourceStore, target, factory, caches, Type.Missing, master }));
        record("PRODUCT_IMPORT_IMPORTER_CTOR_END returned=True");
        currentStage = "IMPORT";
        record("PRODUCT_IMPORT_BEGIN engine=ORIGINAL_PRODUCT_IMPORTER archive=FIXED_PRIVATE_REFERENCE");
        Delegate progressCallback = Delegate.CreateDelegate(progress, this, GetType().GetMethod("Progress", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(progress.GetGenericArguments()[0]));
        lock(progressGate){ProbeFiles.Require(!progressActive&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner,"PRODUCT_IMPORT_PROGRESS_WINDOW");progressActive=true;}
        try{ProjectConstructorProbe.WithSdkProductImportResources(om, delegate { import.Invoke(importer, new object[] { Type.Missing, Type.Missing, progressCallback, Type.Missing, Type.Missing }); }, true);}
        finally{lock(progressGate){progressActive=false;}record("PRODUCT_IMPORT_PROGRESS_END callbacks="+progressCalls+" accepted="+progressAccepted+" background="+progressBackground+" active=False values=NOT_INSPECTED");}
        ProbeFiles.Require(!faulted, "PRODUCT_IMPORT_CAUGHT_CALLBACK_FAULT");
        CheckTree(); source.Position = 0; ProbeFiles.Require(source.Length == 254893 && ProbeFiles.Hash(source) == ArchiveHash, "PRODUCT_IMPORT_SOURCE_CHANGED");
        convertedSource.Position=0;ProbeFiles.Require(ProbeFiles.Hash(convertedSource)==convertedHash,"PRODUCT_IMPORT_CONVERTED_SOURCE_CHANGED");
        completed = true; record("PRODUCT_IMPORT_END returned=True caches=ORIGINAL_PERSIST_PATH compatibility=NOT_ESTABLISHED");
    }
    string ConvertArchive(Assembly om,Assembly common)
    {
        ProbeFiles.Require(!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner,"PRODUCT_IMPORT_CONVERSION_PHASE");
        string output=Path.Combine(root,"converted-reference.knxprod"),temporary=Environment.GetEnvironmentVariable("OPENKNX_IMPORT_TEMP"),token=Environment.GetEnvironmentVariable("OPENKNX_IMPORT_TEMP_TOKEN");
        ProbeFiles.Require(token!=null&&token.Length==32,"PRODUCT_IMPORT_TEMP_OWNER_TOKEN");foreach(char value in token)ProbeFiles.Require(value>='0'&&value<='9'||value>='a'&&value<='f',"PRODUCT_IMPORT_TEMP_OWNER_TOKEN");
        string expected=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Temp","kc-"+token.Substring(0,12));
        ProbeFiles.Require(temporary!=null&&ProbeFiles.Same(temporary,expected),"PRODUCT_IMPORT_TEMP_EXACT_ROOT");ProbeFiles.Local(temporary,true,true);
        string marker=ProbeFiles.Local(Path.Combine(temporary,".openknx-import-owner"),true,false);ProbeFiles.Require(new FileInfo(marker).Length==32&&File.ReadAllText(marker)==token,"PRODUCT_IMPORT_TEMP_OWNERSHIP");
        conversionTemporaryRoot=temporary;ProbeFiles.Local(output,false,false);CheckTree();
        record("PRODUCT_IMPORT_TEMP_ROOT source=EXACT_LAUNCHER_OWNED_SHORT_ROOT quota=COMBINED_WITH_PRODUCT_SCRATCH cleanup=LAUNCHER_AFTER_EXIT");
        string oldTemp=Environment.GetEnvironmentVariable("TEMP"),oldTmp=Environment.GetEnvironmentVariable("TMP");
        try{
            Environment.SetEnvironmentVariable("TEMP",temporary);Environment.SetEnvironmentVariable("TMP",temporary);
            ProbeFiles.Require(ProbeFiles.Same(Path.GetTempPath().TrimEnd('\\'),temporary),"PRODUCT_IMPORT_CONVERSION_TEMP_ROOT");
            Assembly cv=ProjectConstructorProbe.LoadProductImportConverter();
            ProbeFiles.Require(cv.FullName==CvNextIdentity,"PRODUCT_IMPORT_CONVERTER_IDENTITY");
            Type converter=cv.GetType("Knx.Ets.CvNext.Converter",true),resultType=cv.GetType("Knx.Ets.CvNext.ConversionResult",true),schema=common.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion",true);
            Type validator=cv.GetType("Knx.Ets.CvNext.Validation.IFileValidator",true),sink=cv.GetType("Knx.Ets.CvNext.Compatibility.ConversionStepsUserInfoOutput",true);
            ConstructorInfo ctor=Constructor(converter,0x0600002c,typeof(string),typeof(string),validator,sink);
            foreach(ParameterInfo parameter in ctor.GetParameters())ProbeFiles.Require(parameter.IsOptional&&parameter.HasDefaultValue&&parameter.DefaultValue==null,"PRODUCT_IMPORT_CONVERTER_DECLARED_DEFAULTS");
            Type key=common.GetType("Knx.Ets.Common.Security.IKeyProvider",true),keys=typeof(IEnumerable<>).MakeGenericType(key);
            MethodInfo convert=Method(converter,"Convert",0x0600002e,resultType,typeof(string),typeof(string),schema,typeof(CancellationToken),keys);
            ParameterInfo access=convert.GetParameters()[4];ProbeFiles.Require(access.IsOptional&&access.HasDefaultValue&&access.DefaultValue==null,"PRODUCT_IMPORT_CONVERTER_DEFAULT_ACCESS_KEYS");
            FieldInfo target=converter.GetField("DefaultTargetSchemaVersion",BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
            ProbeFiles.Require(target!=null&&target.MetadataToken==0x0400000d&&target.IsInitOnly&&target.FieldType==schema&&target.Module==cv.ManifestModule,"PRODUCT_IMPORT_CONVERTER_DEFAULT_TARGET");
            MethodInfo succeeded=Method(resultType,"get_Succeeded",0x06000020,typeof(bool));
            object actual=Keep(ctor.Invoke(new object[]{Type.Missing,Type.Missing,Type.Missing,Type.Missing}));RequireObject(actual,converter,"PRODUCT_IMPORT_CONVERTER_RESULT");
            object version=target.GetValue(null);ProbeFiles.Require(version!=null&&version.GetType()==schema,"PRODUCT_IMPORT_CONVERTER_TARGET_ENUM");
            record("PRODUCT_IMPORT_CONVERSION_BEGIN source=PINNED_UNCHANGED_ARCHIVE target=ORIGINAL_DEFAULT_SCHEMA engine=ORIGINAL_CVNEXT checks=UNCHANGED");
            object result=null;using(var cancellation=new CancellationTokenSource(35000)){
                ProjectConstructorProbe.WithSdkProductConversionResources(om,delegate{
                    result=convert.Invoke(actual,new object[]{source.Name,output,version,cancellation.Token,Type.Missing});
                });
            }
            RequireObject(result,resultType,"PRODUCT_IMPORT_CONVERSION_RESULT");Keep(result);
            bool success=(bool)succeeded.Invoke(result,new object[0]);record("PRODUCT_IMPORT_CONVERSION_END succeeded="+success+" source-identity=UNCHANGED namespace-remapping=ORIGINAL_ONLY");
            ProbeFiles.Require(success,"PRODUCT_IMPORT_ORIGINAL_CONVERSION_FAILED");
            source.Position=0;ProbeFiles.Require(source.Length==ArchiveSize&&ProbeFiles.Hash(source)==ArchiveHash,"PRODUCT_IMPORT_SOURCE_CHANGED_BY_CONVERSION");
            ProbeFiles.Local(output,true,false);CheckTree();return output;
        }catch(Exception error){faulted=true;CaptureFailure(error);throw;}
        finally{Environment.SetEnvironmentVariable("TEMP",oldTemp);Environment.SetEnvironmentVariable("TMP",oldTmp);CheckTree();}
    }
    void CaptureFailure(Exception error)
    {
        if (failureCaptured) return; failureCaptured = true;
        try {
            Exception cause = error; for (int depth = 0; cause.InnerException != null && depth < 8; depth++) cause = cause.InnerException;
            record("PRODUCT_IMPORT_FAILURE stage=" + currentStage + " type=" + ProjectConstructorProbe.Text(cause.GetType().FullName) + " hresult=" + cause.HResult.ToString("X8"));
            var trace = new StackTrace(cause, false);
            for (int i = 0; i < trace.FrameCount && i < 6; i++) {
                try {
                    MethodBase method = trace.GetFrame(i).GetMethod();
                    if (method == null) { record("PRODUCT_IMPORT_EXCEPTION_FRAME index=" + i + " category=UNAVAILABLE"); continue; }
                    record("PRODUCT_IMPORT_EXCEPTION_FRAME index=" + i + " owner=" + ProjectConstructorProbe.Text(method.DeclaringType == null ? "GLOBAL" : method.DeclaringType.FullName)
                        + " token=" + method.MetadataToken.ToString("X8") + " name=" + ProjectConstructorProbe.Text(method.Name));
                } catch (Exception unavailable) { record("PRODUCT_IMPORT_EXCEPTION_FRAME index=" + i + " category=UNAVAILABLE type=" + ProjectConstructorProbe.Text(unavailable.GetType().FullName)); }
            }
        } catch { /* Original failure remains fatal; never replace it with diagnostic success. */ }
    }
    void Progress<T>(T unused)
    {
        lock(progressGate){
            bool beforeFault=Volatile.Read(ref faulted),beforeDisposed=Volatile.Read(ref disposed);bool entered=false;
            if(progressCalls<4097)progressCalls++;
            try{
                ProbeFiles.Require(progressActive&&!beforeDisposed&&!beforeFault&&!progressEntered&&progressCalls<=4096,"PRODUCT_IMPORT_PROGRESS_BOUND");
                progressEntered=true;entered=true;CheckTree();ProbeFiles.Require(!Volatile.Read(ref faulted),"PRODUCT_IMPORT_PROGRESS_CAUGHT_FAULT");
                progressAccepted++;if(Thread.CurrentThread.ManagedThreadId!=owner)progressBackground++;
            }catch(Exception error){Volatile.Write(ref faulted,true);RecordCallbackFailure("PROGRESS",error,beforeFault,beforeDisposed,progressCalls,null,null,null);throw;}
            finally{if(entered)progressEntered=false;}
        }
    }
    void RecordCallbackFailure(string stage,Exception error,bool beforeFault,bool beforeDisposed,int count,Type expected,DirectoryInfo directory,string name)
    {
        if(Interlocked.CompareExchange(ref callbackFailureRecorded,1,0)!=0)return;
        try{
            Exception cause=error;for(int i=0;cause.InnerException!=null&&i<8;i++)cause=cause.InnerException;
            record("PRODUCT_IMPORT_CALLBACK_FAILURE stage="+stage+" fault-before="+beforeFault+" disposed-before="+beforeDisposed+" fault-latched=True owner-thread="+owner
                +" current-thread="+Thread.CurrentThread.ManagedThreadId+" owner-match="+(Thread.CurrentThread.ManagedThreadId==owner)+" callback-count="+count
                +" expected-type="+(expected==null?"NOT_APPLICABLE":ProjectConstructorProbe.Text(expected.FullName))+" exception-type="+ProjectConstructorProbe.Text(cause.GetType().FullName)+" hresult="+cause.HResult.ToString("X8"));
            if(expected!=null){
                bool bounded=name!=null&&name.Length<=256,rooted=false,leaf=false,separators=false,dot=false,inside=false;
                if(bounded){rooted=Path.IsPathRooted(name);leaf=Path.GetFileName(name)==name;separators=name.IndexOfAny(new char[]{':','/','\\'})>=0;dot=name=="."||name=="..";
                    if(rooted){string full=Path.GetFullPath(name);inside=full.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase);}}
                record("PRODUCT_IMPORT_CREATOR_ARGUMENT_DIAGNOSTIC directory-null="+(directory==null)+" name-null="+(name==null)+" name-length="+(name==null?-1:name.Length)
                    +" name-empty="+(name==String.Empty)+" bounded="+bounded+" rooted="+rooted+" leaf="+leaf+" separators="+separators+" dot="+dot+" absolute-under-owned-root="+inside+" values=NOT_LOGGED guard=UNCHANGED");
            }
        }catch{ /* The already latched callback fault and original exception remain fatal. */ }
    }
    internal void Audit(string phase)
    {
        CheckTree(); ProbeFiles.Require(completed && !faulted && cacheReplies <= ProjectConstructorProbe.ProductUiCacheRequestLimit() && progressCalls<=4096 && progressAccepted<=progressCalls && !progressActive && !progressEntered, "PRODUCT_IMPORT_FINAL_FAULT");
        if(ProjectConstructorProbe.CompactProductDiagnostics()&&(phase=="DCA_UI_RETURNED"||ProjectConstructorProbe.SdkAuditLog(phase))){lock(treeGate){record((ProjectConstructorProbe.ParameterNegativeProfile()?"PARAMETER_READ_IO_AUDIT phase=":"PRODUCT_UI_IO_AUDIT phase=")+phase+" ui-cache-lookups="+uiCacheLookups+" full-tree-checks="+treeChecks+" full-tree-check-ms="+(treeTicks*1000/Stopwatch.Frequency)+" read-checkpoint=FIRST_OR_256_OR_NEXT_READ_AFTER_1S creator-and-terminal-checks=UNCHANGED quota=BOUNDARY_NOT_OS_CONTAINED");}}
        if (phase == "PROCESS_FINAL"&&(!ProjectConstructorProbe.CompactProductDiagnostics()||ProjectConstructorProbe.SdkAuditLog(phase))) record("PRODUCT_IMPORT_AUDIT phase=PROCESS_FINAL completed=" + completed + " disposed=" + disposed + " fault=" + faulted + " cache-replies=" + cacheReplies + " progress-calls=" + progressCalls);
    }
    object Keep(object value) { ProbeFiles.Require(value != null, "PRODUCT_IMPORT_NON_NULL"); retained.Add(value); return value; }

    static object CreateContainer(Assembly autofac, Type guidContract, object guid, Type single, Type store, Type stream, Type streamContract, Type cache, Type cacheContract)
    {
        Type builderType = autofac.GetType("Autofac.ContainerBuilder", true), extensions = autofac.GetType("Autofac.RegistrationExtensions", true);
        object builder = builderType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        Register(extensions, builderType, builder, single, store, null);
        Register(extensions, builderType, builder, stream, streamContract, null);
        Register(extensions, builderType, builder, cache, cacheContract, null);
        Register(extensions, builderType, builder, guidContract, guidContract, guid);
        Type options = autofac.GetType("Autofac.Builder.ContainerBuildOptions", true);
        ProbeFiles.Require(Enum.GetName(options, 0) == "None", "PRODUCT_IMPORT_BUILD_NONE");
        MethodInfo build = builderType.GetMethod("Build", new Type[] { options });
        object result = build.Invoke(builder, new object[] { Enum.ToObject(options, 0) });
        ProbeFiles.Require(result is IDisposable && autofac.GetType("Autofac.IContainer", true).IsInstanceOfType(result), "PRODUCT_IMPORT_CONTAINER"); return result;
    }
    static void Register(Type extensions, Type builderType, object builder, Type implementation, Type service, object instance)
    {
        string name = instance == null ? "RegisterType" : "RegisterInstance"; MethodInfo selected = null;
        foreach (MethodInfo m in extensions.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) {
            ParameterInfo[] p = m.GetParameters(); if (m.Name != name || !m.IsGenericMethodDefinition || m.GetGenericArguments().Length != 1
                || p.Length != (instance == null ? 1 : 2) || p[0].ParameterType != builderType) continue;
            if (instance != null && p[1].ParameterType != m.GetGenericArguments()[0]) continue;
            ProbeFiles.Require(selected == null, "PRODUCT_IMPORT_REGISTRATION_AMBIGUOUS"); selected = m;
        }
        ProbeFiles.Require(selected != null, "PRODUCT_IMPORT_REGISTRATION_MISSING"); MethodInfo closed = selected.MakeGenericMethod(implementation);
        object value = closed.Invoke(null, instance == null ? new object[] { builder } : new object[] { builder, instance }); Type api = closed.ReturnType;
        if (instance == null) {
            MethodInfo asMethod = null; foreach (MethodInfo m in api.GetMethods()) if (m.Name == "As" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0) { ProbeFiles.Require(asMethod == null, "PRODUCT_IMPORT_AS_AMBIGUOUS"); asMethod = m; }
            ProbeFiles.Require(asMethod != null && Object.ReferenceEquals(value, asMethod.MakeGenericMethod(service).Invoke(value, new object[0])), "PRODUCT_IMPORT_AS_SAME_BUILDER");
        } else {
            MethodInfo external = api.GetMethod("ExternallyOwned", Type.EmptyTypes);
            ProbeFiles.Require(external != null && Object.ReferenceEquals(value, external.Invoke(value, new object[0])), "PRODUCT_IMPORT_GUID_EXTERNAL_OWNERSHIP");
        }
    }
    static object Resolve(Assembly autofac, object container, Type result)
    {
        Type context = autofac.GetType("Autofac.IComponentContext", true); MethodInfo selected = null;
        foreach (MethodInfo m in autofac.GetType("Autofac.ResolutionExtensions", true).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) {
            ParameterInfo[] p = m.GetParameters(); if (m.Name == "Resolve" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && p.Length == 1 && p[0].ParameterType == context) { ProbeFiles.Require(selected == null, "PRODUCT_IMPORT_RESOLVE_AMBIGUOUS"); selected = m; }
        }
        ProbeFiles.Require(selected != null, "PRODUCT_IMPORT_RESOLVE_API"); return selected.MakeGenericMethod(result).Invoke(null, new object[] { container });
    }
    internal object[] PrepareLargeStrings()
    {
        lock(progressGate){
            ProbeFiles.Require(completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductDefinitionContextRequested()&&projectLargeStrings==null,"PRODUCT_LARGE_STRING_PREPARE_ONCE");
            try{projectStringStream=projectStreamCreator.CreateOriginal(new DirectoryInfo(Path.Combine(root,"project-state")),"large-strings.cache");
                projectLargeStrings=Keep(projectLargeStringCtor.Invoke(new object[]{projectStringStream}));RequireObject(projectLargeStrings,projectLargeStringCtor.DeclaringType,"PRODUCT_LARGE_STRING_ORIGINAL_CACHE");CheckTree();
                record("PRODUCT_LARGE_STRING_READY cache-ctor=0600914F stream=ORIGINAL_FILE_STREAM_STORE initial-project-state=FRESH_TRANSIENT_SCRATCH codec=ORIGINAL_ONLY");return new object[]{projectStringStream,projectLargeStrings};
            }catch{faulted=true;throw;}
        }
    }
    internal object LargeStringStream(object ignored)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){lock(progressGate){ProbeFiles.Require(ignored==null&&completed&&!disposed&&!faulted&&ProjectConstructorProbe.ProductDataReadLifetime()&&projectStringStream!=null,"PRODUCT_LARGE_STRING_STORE_UI_LIFETIME");CheckTree();return projectStringStream;}
        }
    }
    void EnsureProjectSerializationStore()
    {
        ProbeFiles.Require(completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_PROJECT_SERIALIZATION_PHASE");
        if(projectSerializationStore==null){CheckTree();projectSerializationStore=Keep(projectSerializationCtor.Invoke(projectSerializationArgs));RequireObject(projectSerializationStore,projectSerializationCtor.DeclaringType,"PRODUCT_PROJECT_ORIGINAL_STORE");
            string actual=projectSerializationPath.Invoke(projectSerializationStore,new object[0]) as string;ProbeFiles.Require(actual!=null&&ProbeFiles.Same(actual,(string)FilePath(null)),"PRODUCT_PROJECT_SAME_TRANSACTION_ROOT");ProbeFiles.Local(actual,true,true);CheckTree();record("PRODUCT_PROJECT_SERIALIZATION_STORE_READY ctor=060085A4 source=ORIGINAL_FILE_PROJECT_STORE target=OWNED_TRANSIENT_SCRATCH local-serialization-only=True");}
    }
    internal object ProjectStateRead(int selected,object ignored)
    {
        lock(progressGate){
            ProbeFiles.Require(ignored==null&&selected>=0&&selected<projectStateGetters.Length&&projectStateReads<64,"PRODUCT_PROJECT_SAVE_GETTER_LIMIT");projectStateReads++;
            try{currentStage="PROJECT_SAVE_STORE_GETTER";EnsureProjectSerializationStore();CheckTree();object value=projectStateGetters[selected].Invoke(projectSerializationStore,new object[0]);
                ProbeFiles.Require(value!=null&&!RemotingServices.IsTransparentProxy(value)&&projectStateGetters[selected].ReturnType.IsInstanceOfType(value),"PRODUCT_PROJECT_ORIGINAL_STORE_RESULT");
                if(selected>=14){
                    Type element=projectStateGetters[selected].ReturnType.GetGenericArguments()[0],listType=typeof(List<>).MakeGenericType(element);ConstructorInfo listCtor=listType.GetConstructor(Type.EmptyTypes);
                    ProbeFiles.Require(element.Module==manufacturerType.Module&&listCtor!=null&&listCtor.Module==typeof(List<>).Module,"PRODUCT_PROJECT_ENUMERATION_MARSHAL_ABI");
                    IList list=(IList)listCtor.Invoke(new object[0]);var iterator=((IEnumerable)value).GetEnumerator();
                    try{while(iterator.MoveNext()){ProbeFiles.Require(list.Count<128,"PRODUCT_PROJECT_ENUMERATION_LIMIT");object id=iterator.Current;ProbeFiles.Require(id!=null&&!RemotingServices.IsTransparentProxy(id)&&element.IsInstanceOfType(id)&&id.GetType().Module==element.Module,"PRODUCT_PROJECT_ENUMERATION_ORIGINAL_ID");list.Add(id);}}
                    finally{IDisposable close=iterator as IDisposable;if(close!=null)close.Dispose();}
                    value=list;record("PRODUCT_PROJECT_ENUMERATED member="+projectStateGetters[selected].Name+" count="+list.Count+" source=ORIGINAL_STORE_ID_REFERENCES no-host-parser=True values=NOT_LOGGED");
                }else ProbeFiles.Require(value.GetType().Module==projectSerializationCtor.Module,"PRODUCT_PROJECT_ORIGINAL_STORE_MODULE");
                Keep(value);CheckTree();
                ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_PROJECT_SAVE_GETTER_COMPLETION");return value;
            }catch(Exception error){faulted=true;CaptureFailure(error);throw;}
        }
    }
    internal object ProjectContentWriter(object ignored)
    {
        lock(progressGate){
            currentStage="PROJECT_CONTENT_WRITER_PHASE";
            try{ProbeFiles.Require(ignored==null&&completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductStoreDisposalLifetime()&&projectWriterCalls<16,"PRODUCT_CONTENT_WRITER_DISPOSAL_PHASE");projectWriterCalls++;
                currentStage="PROJECT_CONTENT_WRITER_TREE";CheckTree();currentStage="PROJECT_CONTENT_WRITER_CREATE_STORE";if(projectContentStore==null){EnsureProjectSerializationStore();projectContentStore=Keep(projectOriginalGetter.Invoke(projectSerializationStore,new object[0]));}currentStage="PROJECT_CONTENT_WRITER_VERIFY_STORE";
                ProbeFiles.Require(projectContentStore!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(projectContentStore)&&projectContentStore.GetType()==applicationStoreType&&projectAsWriter.DeclaringType.IsInstanceOfType(projectContentStore),"PRODUCT_CONTENT_ORIGINAL_SINGLE_STORE");
                currentStage="PROJECT_CONTENT_WRITER_ORIGINAL_OPEN";object writer=projectAsWriter.Invoke(projectContentStore,new object[0]);currentStage="PROJECT_CONTENT_WRITER_VERIFY_RESULT";ProbeFiles.Require(writer!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(writer)&&projectAsWriter.ReturnType.IsInstanceOfType(writer)&&writer.GetType().Module==projectAsWriter.Module,"PRODUCT_CONTENT_ORIGINAL_WRITER");Keep(writer);CheckTree();
                currentStage="PROJECT_CONTENT_WRITER_COMPLETION";ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_CONTENT_WRITER_COMPLETION");return writer;
            }catch(Exception error){faulted=true;CaptureFailure(error);throw;}
        }
    }
    internal void DisposeCachedInformation()
    {
        lock(progressGate){
            ProbeFiles.Require(completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductStoreDisposalLifetime()&&cacheReleases<16,"PRODUCT_CACHE_RELEASE_DISPOSAL_PHASE");cacheReleases++;
            try{currentStage="PROJECT_CACHE_RELEASE";CheckTree();RequireObject(caches,cacheRelease.DeclaringType,"PRODUCT_CACHE_RELEASE_ORIGINAL_INSTANCE");object result=cacheRelease.Invoke(caches,new object[0]);ProbeFiles.Require(result==null,"PRODUCT_CACHE_RELEASE_VOID_RESULT");CheckTree();
                ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_CACHE_RELEASE_COMPLETION");record("PRODUCT_CACHE_RELEASE original-method=06007D06 call="+cacheReleases+" action=ORIGINAL_WEAK_REFERENCE_EVICTION ownership=NO_EXTRA_DISPOSE_INVENTED");
            }catch(Exception error){faulted=true;CaptureFailure(error);throw;}
        }
    }
    internal object CacheWeakPolicy(bool setter,object argument)
    {
        lock(progressGate){
            ProbeFiles.Require(completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductStoreDisposalLifetime()&&cacheWeakCalls<16,"PRODUCT_CACHE_WEAK_DISPOSAL_PHASE");cacheWeakCalls++;
            try{ProbeFiles.Require(setter?argument!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(argument)&&argument.GetType()==typeof(bool):argument==null,"PRODUCT_CACHE_WEAK_ARGUMENT");CheckTree();
                RequireObject(caches,cacheWeakGetter.DeclaringType,"PRODUCT_CACHE_WEAK_ORIGINAL_INSTANCE");object result=(setter?cacheWeakSetter:cacheWeakGetter).Invoke(caches,setter?new object[]{argument}:new object[0]);
                ProbeFiles.Require(setter?result==null:result!=null&&result.GetType()==typeof(bool),"PRODUCT_CACHE_WEAK_ORIGINAL_RESULT");CheckTree();ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_CACHE_WEAK_COMPLETION");
                record("PRODUCT_CACHE_WEAK_POLICY operation="+(setter?"SET":"GET")+" source=ORIGINAL_IMPORTED_CACHES call="+cacheWeakCalls+" values=NOT_LOGGED");return result;
            }catch{faulted=true;throw;}
        }
    }
    internal object FilePath(object ignored)
    {
        lock(progressGate){
            ProbeFiles.Require(ignored==null&&completed&&!disposed&&!faulted&&Thread.CurrentThread.ManagedThreadId==owner&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_FILE_PATH_DISPOSAL_PHASE");
            try{CheckTree();object result=targetFilePath.Invoke(target,new object[0]);string path=result as string;
                ProbeFiles.Require(path!=null&&path.Length<=4096&&ProbeFiles.Same(path,Path.Combine(root,"products")),"PRODUCT_FILE_PATH_ORIGINAL_TARGET_ONLY");ProbeFiles.Local(path,true,true);CheckTree();
                ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductStoreDisposalLifetime(),"PRODUCT_FILE_PATH_COMPLETION");return result;
            }catch{faulted=true;throw;}
        }
    }
    internal object ManufacturerRead(int selected,object manufacturer,object selectedApplication)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        lock(progressGate){
            ProbeFiles.Require(completed&&!disposed&&!faulted&&ProjectConstructorProbe.ProductDataReadLifetime()&&manufacturerStoreReads<16&&selected>=0&&selected<3,"PRODUCT_MANUFACTURER_STORE_UI_PHASE");RequireObject(manufacturer,manufacturerType,"PRODUCT_MANUFACTURER_STORE_EXACT_ARGUMENT");
            try{CheckTree();MethodInfo method=selected==0?applicationPrograms:selected==1?hardwareStore:baggageStore;object value=method.Invoke(target,new object[]{manufacturer});
                if(selected==0){RequireObject(value,applicationListType,"PRODUCT_APPLICATIONS_ORIGINAL_LIST");var list=(IList)value;ProbeFiles.Require(list.Count<=1024,"PRODUCT_APPLICATIONS_LIST_BOUND");bool present=false;
                    for(int i=0;i<list.Count;i++){object id=list[i];RequireObject(id,applicationStore.GetParameters()[0].ParameterType,"PRODUCT_APPLICATIONS_ORIGINAL_ID");ProbeFiles.Require(Object.Equals(applicationManufacturer.Invoke(id,new object[0]),manufacturer),"PRODUCT_APPLICATIONS_SAME_MANUFACTURER");if(Object.Equals(id,selectedApplication))present=true;}
                    record("PRODUCT_APPLICATIONS_ENUMERATED count="+list.Count+" selected-present="+present+" identity=ORIGINAL_IDS_NO_REMAP compatibility=NOT_QUALIFIED");
                }else RequireObject(value,applicationStoreType,"PRODUCT_MANUFACTURER_ORIGINAL_SINGLE_FILE");
                CheckTree();ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductDataReadLifetime(),"PRODUCT_MANUFACTURER_STORE_COMPLETION");Keep(value);manufacturerStoreReads++;return value;
            }catch{faulted=true;throw;}
        }

        }
    }
    internal object ApplicationProgram(object application)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        lock(progressGate){
            ProbeFiles.Require(completed&&!disposed&&!faulted&&ProjectConstructorProbe.ProductDataReadLifetime()&&applicationStores<16,"PRODUCT_APPLICATION_STORE_HELPER_UI_PHASE");
            RequireObject(application,applicationStore.GetParameters()[0].ParameterType,"PRODUCT_APPLICATION_STORE_ARGUMENT_TYPE");
            try{CheckTree();object value=applicationStore.Invoke(target,new object[]{application});RequireObject(value,applicationStoreType,"PRODUCT_APPLICATION_STORE_ORIGINAL_SINGLE_FILE");CheckTree();ProbeFiles.Require(!faulted&&!disposed&&ProjectConstructorProbe.ProductDataReadLifetime(),"PRODUCT_APPLICATION_STORE_HELPER_COMPLETION");Keep(value);applicationStores++;return value;}
            catch{faulted=true;throw;}
        }

        }
    }
    internal object Cache(int selected, object manufacturer)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        lock(progressGate){
            bool background=Thread.CurrentThread.ManagedThreadId!=owner;
            ProbeFiles.Require(completed && !disposed && !faulted && (!background||ProjectConstructorProbe.ProductUiCacheLifetime()||ProjectConstructorProbe.ParameterDefinitionCacheLifetime()) && cacheReplies < ProjectConstructorProbe.ProductUiCacheRequestLimit() && selected >= 0 && selected < 7, "PRODUCT_IMPORT_CACHE_PHASE");
            RequireObject(manufacturer, selected>=3?dynamicTranslations.GetParameters()[0].ParameterType:manufacturerType, "PRODUCT_IMPORT_CACHE_MANUFACTURER_OR_APPLICATION_TYPE");
            try { CheckCacheReadTree(); MethodInfo selectedMethod=selected==0?hardware:selected==1?application:selected==2?manufacturerProvider:selected==3?dynamicTranslations:selected==4?parameterRefs:selected==5?comObjectRefs:calculatedParameters; object value = selectedMethod.Invoke(caches, new object[] { manufacturer });
                RequireObject(value, selected==2?manufacturerProviderType:selectedMethod.ReturnType, "PRODUCT_IMPORT_CACHE_ORIGINAL_RESULT");ProbeFiles.Require(!faulted&&!disposed&&(!background||ProjectConstructorProbe.ProductUiCacheLifetime()||ProjectConstructorProbe.ParameterDefinitionCacheLifetime()),"PRODUCT_IMPORT_CACHE_COMPLETION");if(selected>=2)Keep(value); cacheReplies++; return value;
            } catch { faulted = true; throw; }
        }

        }
    }
    void CheckPath(DirectoryInfo directory, string name)
    {
        ProbeFiles.Require(!disposed && !faulted && (Thread.CurrentThread.ManagedThreadId == owner || ReferenceDcaRequiredParameters.AllowsDerivedReadCreation(completed,ProjectConstructorProbe.ProductUiCacheLifetime(),ProjectConstructorProbe.ParameterDefinitionCacheLifetime())) && directory != null && name != null && name.Length <= 256
            && !Path.IsPathRooted(name) && Path.GetFileName(name) == name && name != "." && name != ".." && name.IndexOfAny(new char[] { ':', '/', '\\' }) < 0,
            "PRODUCT_IMPORT_CREATOR_ARGUMENTS");
        string stem = name.Split('.')[0].ToUpperInvariant();
        ProbeFiles.Require(stem != "CON" && stem != "PRN" && stem != "AUX" && stem != "NUL"
            && !(stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal)) && stem[3] >= '1' && stem[3] <= '9'), "PRODUCT_IMPORT_DEVICE_NAME");
        string path = Path.GetFullPath(Path.Combine(directory.FullName, name));
        ProbeFiles.Require(path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "PRODUCT_IMPORT_CREATOR_PATH_ESCAPE");
        EnsureCreatorDirectory(directory.FullName);
        try { FileAttributes existing = File.GetAttributes(path); ProbeFiles.Require((existing & (FileAttributes.ReparsePoint | FileAttributes.Directory | FileAttributes.Device)) == 0, "PRODUCT_IMPORT_CREATOR_TARGET_TYPE"); ProbeFiles.Local(path, true, false); }
        catch (FileNotFoundException) { ProbeFiles.Local(path, false, false); }
        catch (DirectoryNotFoundException) { ProbeFiles.Local(path, false, false); }
        CheckTree();
    }
    void EnsureCreatorDirectory(string requested)
    {
        string target=Path.GetFullPath(requested);
        ProbeFiles.Require(ProbeFiles.Same(target,root)||target.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase),"PRODUCT_IMPORT_DIRECTORY_ESCAPE");
        ProbeFiles.Local(root,true,true);
        if(ProbeFiles.Same(target,root))return;
        string[] parts=target.Substring(root.Length+1).Split(Path.DirectorySeparatorChar);
        ProbeFiles.Require(parts.Length<=128,"PRODUCT_IMPORT_DIRECTORY_LIMIT");string current=root;
        foreach(string part in parts){
            string stem=part.Split('.')[0].ToUpperInvariant();
            ProbeFiles.Require(stem!="CON"&&stem!="PRN"&&stem!="AUX"&&stem!="NUL"
                &&!(stem.Length==4&&(stem.StartsWith("COM",StringComparison.Ordinal)||stem.StartsWith("LPT",StringComparison.Ordinal))&&stem[3]>='1'&&stem[3]<='9'),"PRODUCT_IMPORT_DIRECTORY_DEVICE_NAME");
            current=Path.Combine(current,part);bool missing=false;
            try{File.GetAttributes(current);}catch(FileNotFoundException){missing=true;}catch(DirectoryNotFoundException){missing=true;}
            if(missing){
                ProbeFiles.Local(current,false,true);ProbeFiles.Require(CheckTree()<128,"PRODUCT_IMPORT_DIRECTORY_LIMIT");
                Directory.CreateDirectory(current);directoriesCreated++;ProbeFiles.Local(current,true,true);CheckTree();
            }else ProbeFiles.Local(current,true,true);
        }
    }
    void CheckCacheReadTree()
    {
        if(!ProjectConstructorProbe.ProductDataReadLifetime()){CheckTree();return;}
        uiCacheLookups++;long now=Stopwatch.GetTimestamp();
        if(uiLastTreeTicks==0||uiCacheLookups%256==0||now-uiLastTreeTicks>=Stopwatch.Frequency){CheckTree();uiLastTreeTicks=Stopwatch.GetTimestamp();}
    }
    int CheckTree() { lock(treeGate){long started=Stopwatch.GetTimestamp();try{return CheckTreeCore();}finally{treeChecks++;treeTicks+=Stopwatch.GetTimestamp()-started;}} }
    int CheckTreeCore()
    {
        if (!Directory.Exists(root)) return 0;
        long total = 0; int count = 0, directories = 0; var queue = new Queue<string>(); queue.Enqueue(root);if(conversionTemporaryRoot!=null){ProbeFiles.Local(conversionTemporaryRoot,true,true);queue.Enqueue(conversionTemporaryRoot);}
        while (queue.Count != 0) {
            string directory = queue.Dequeue(); ProbeFiles.Local(directory, true, true); ProbeFiles.Require(++directories <= 128, "PRODUCT_IMPORT_DIRECTORY_LIMIT");
            foreach (string path in Directory.EnumerateFileSystemEntries(directory)) {
                FileAttributes attributes = File.GetAttributes(path); ProbeFiles.Require((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) == 0, "PRODUCT_IMPORT_REPARSE_OUTPUT");
                if ((attributes & FileAttributes.Directory) != 0) { queue.Enqueue(path); ProbeFiles.Require(queue.Count <= 128, "PRODUCT_IMPORT_DIRECTORY_LIMIT"); }
                else { ProbeFiles.Local(path, true, false); long size = new FileInfo(path).Length; ProbeFiles.Require(size <= 16 * 1024 * 1024 && ++count <= 128, "PRODUCT_IMPORT_FILE_LIMIT"); total += size; ProbeFiles.Require(total <= 64L * 1024 * 1024, "PRODUCT_IMPORT_TOTAL_LIMIT"); }
            }
        }
        return directories;
    }
    sealed class Creator
    {
        readonly OriginalProductImportProbe parent; readonly object original; readonly MethodInfo invoke; readonly Type expected; int calls;
        internal Creator(OriginalProductImportProbe scope, object callback, Type concrete)
        { parent = scope; original = callback; expected = concrete; ProbeFiles.Require(callback is Delegate, "PRODUCT_IMPORT_ORIGINAL_CREATOR"); invoke = callback.GetType().GetMethod("Invoke"); }
        internal object Bind(Type callback, string name, Type result)
        { return Delegate.CreateDelegate(callback, this, GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(result)); }
        internal object BindCache(Type callback, Type manufacturer, Type result)
        { return Delegate.CreateDelegate(callback, this, GetType().GetMethod("CreateCache", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(manufacturer, result)); }
        T Create<T>(DirectoryInfo directory, string name) { return (T)Run(directory, name, new object[] { directory, name }); }
        internal object CreateOriginal(DirectoryInfo directory,string name){return Run(directory,name,new object[]{directory,name});}
        TCache CreateCache<TManufacturer, TCache>(DirectoryInfo directory, TManufacturer manufacturer, string name)
        { return (TCache)Run(directory, name, new object[] { directory, manufacturer, name }); }
        object Run(DirectoryInfo directory, string name, object[] args)
        {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
            lock(parent.progressGate){
                bool beforeFault=Volatile.Read(ref parent.faulted),beforeDisposed=Volatile.Read(ref parent.disposed);string stage="CREATOR_LIMIT";
                bool background=Thread.CurrentThread.ManagedThreadId!=parent.owner;
                try { int limit=ProjectConstructorProbe.ProductUiCreatorLimit();if(calls<limit+1)calls++;ProbeFiles.Require(calls <= limit, "PRODUCT_IMPORT_CREATOR_LIMIT");stage="CREATOR_PATH";parent.CheckPath(directory, name);
                    stage="CREATOR_ORIGINAL";object result = invoke.Invoke(original, args);stage="CREATOR_RESULT";RequireObject(result, expected, "PRODUCT_IMPORT_CREATOR_GENUINE_RESULT");
                    ProbeFiles.Require(!parent.disposed&&!parent.faulted&&(!background||ReferenceDcaRequiredParameters.AllowsDerivedReadCreation(parent.completed,ProjectConstructorProbe.ProductUiCacheLifetime(),ProjectConstructorProbe.ParameterDefinitionCacheLifetime())),"PRODUCT_IMPORT_CREATOR_COMPLETION_LIFETIME");
                    parent.Keep(result);stage="CREATOR_TREE";parent.CheckTree();return result;
                } catch(Exception error) { Volatile.Write(ref parent.faulted,true);parent.RecordCallbackFailure(stage,error,beforeFault,beforeDisposed,calls,expected,directory,name);throw; }
            }

        }
    }
    }
    public void Dispose()
    {
        lock(progressGate){if(disposed)return;disposed=true;progressActive=false;}Exception failure=null;
        try { CheckTree(); } catch (Exception error) { failure = error; faulted = true; }
        try { if (container != null) ((IDisposable)container).Dispose(); } catch (Exception error) { if (failure == null) failure = error; faulted = true; }
        try { IDisposable value = convertedZip as IDisposable; if (value != null) value.Dispose(); } catch (Exception error) { if (failure == null) failure = error; faulted = true; }
        try { IDisposable value = zip as IDisposable; if (value != null) value.Dispose(); } catch (Exception error) { if (failure == null) failure = error; faulted = true; }
        finally { if (convertedSource != null) convertedSource.Dispose(); if (source != null) source.Dispose(); foreach (object value in retained) GC.KeepAlive(value); GC.KeepAlive(creators); GC.KeepAlive(caches); }
        record("PRODUCT_IMPORT_CLEANUP completed=" + completed + " fault=" + faulted + " cache-replies=" + cacheReplies + " created-directories=" + directoriesCreated + " quota=BOUNDARY_CHECKED_NOT_OS_CONTAINED");
        if (failure != null) throw failure;
    }
}
