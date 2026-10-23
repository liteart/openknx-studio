// Fixed Task-009 diagnostic. Labeled fixture data never qualifies a Project or DCA.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

internal sealed class ProjectDenyProxy : RealProxy
{
    readonly Action<string> record;
    readonly object sync = new object();
    int requests;
    internal ProjectDenyProxy(Type type, Action<string> recorder) : base(type)
    { ProbeFiles.Require(type.IsPublic && type.IsInterface, "PROJECT_PUBLIC_INTERFACE_REQUIRED"); record = recorder; }
    internal int Requests { get { lock (sync) { return requests; } } }
    public override IMessage Invoke(IMessage message)
    {
        IMethodCallMessage call = message as IMethodCallMessage;
        if (call == null || call.MethodBase == null) throw new NotSupportedException("PROJECT_FIXTURE_UNSUPPORTED:INVALID_MESSAGE");
        int count;
        lock (sync) { if (requests < 17) requests++; count = requests; }
        string name = ProjectConstructorProbe.Text(call.MethodBase.DeclaringType.FullName + "." + call.MethodName);
        // Never inspect arguments, synthesize out values, or call a proxy's ToString.
        if (count <= 16) record("PROJECT_SERVICE_DENIED number=" + count + " first-for-interface=" + (count == 1)
            + " member=" + name + " token=" + call.MethodBase.MetadataToken.ToString("X8") + " arguments=" + call.ArgCount);
        return new ReturnMessage(new NotSupportedException("PROJECT_FIXTURE_UNSUPPORTED:" + (count <= 16 ? name : "REQUEST_LIMIT")), call);
    }
}

// One explicitly labeled fixture getter; no other SDK operation is supplied.
internal sealed class ProjectStoreIdProxy : RealProxy
{
    readonly Type owner, idType;
    readonly MethodInfo getter;
    readonly object value, sync = new object();
    readonly Action<string> record;
    int requests, fixtureCalls, denied;
    internal ProjectStoreIdProxy(Type type, MethodInfo method, object id, Action<string> recorder) : base(type)
    {
        ProjectConstructorProbe.StoreIdSignature(type, method, id == null ? null : id.GetType());
        owner = type; getter = method; idType = id.GetType(); value = id; record = recorder;
    }
    internal int FixtureCalls { get { lock (sync) { return fixtureCalls; } } }
    internal int DeniedRequests { get { lock (sync) { return denied; } } }
    public override IMessage Invoke(IMessage message)
    {
        lock (sync) {
            if (requests < 17) requests++;
            IMethodCallMessage call = message as IMethodCallMessage;
            MethodInfo method = call == null ? null : call.MethodBase as MethodInfo;
            bool allowed = requests <= 16 && method != null && method.DeclaringType == owner && method.Module == getter.Module
                && method.MetadataToken == getter.MetadataToken && method.Name == "get_Id" && method.IsPublic && !method.IsStatic
                && method.IsSpecialName && !method.IsGenericMethod && !method.ContainsGenericParameters
                && method.ReturnType == idType && method.GetParameters().Length == 0 && call.MethodName == method.Name
                && call.ArgCount == 0 && call.Args != null && call.Args.Length == 0;
            if (allowed) {
                fixtureCalls++;
                record("PROJECT_SERVICE_FIXTURE number=" + fixtureCalls + " member=" + owner.FullName + ".get_Id token=" + getter.MetadataToken.ToString("X8")
                    + " arguments=0 fixture=OWNED_SYNTHETIC_ID same-object=true qualification=NOT_ESTABLISHED");
                return new ReturnMessage(value, null, 0, call.LogicalCallContext, call);
            }
            if (denied < 17) denied++;
            string name = method == null ? "INVALID_MESSAGE" : ProjectConstructorProbe.Text((method.DeclaringType == null ? "UNKNOWN_OWNER" : method.DeclaringType.FullName) + "." + method.Name);
            if (requests <= 16) record("PROJECT_SERVICE_DENIED number=" + denied + " member=" + name + " fixture-id-only=true");
            var error = new NotSupportedException("PROJECT_FIXTURE_UNSUPPORTED:" + (requests <= 16 ? name : "REQUEST_LIMIT"));
            if (call == null || method == null) throw error;
            return new ReturnMessage(error, call);
        }
    }
}

internal static class ProjectConstructorProbe
{
    // Observed enum02 metadata, independently verified/reviewed before this freeze.
    // Enables only the fixed deny-all public constructor diagnostic, not a fixture.
    static readonly bool OriginalConstructorReady = true;
    const string ReviewedLiteral = "ThreeLevel";
    const string ReviewedRawValue = "1";
    static readonly int ReviewedLiteralToken = 0x04000176;
    static readonly int ReviewedEnumTypeToken = 0x0200008e;
    const string ReviewedUnderlyingType = "System.Byte";
    const string ReviewedEnumReportSha256 = "59323bf7eb53d6c6a130c523e49d82bb3e3d58830f43dcc2f13b2dcabb58bcf7";
    const string EnumName = "Knx.Ets.Common.Types.Enumerations.GroupAddressStyle";
    const string FixtureName = "Task009 offline constructor diagnostic";
    const string OmIdentity = "Knx.Ets.ObjectModel, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string CommonIdentity = "Knx.Ets.Common.Types, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string CommonHash = "5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0";
    const string PlatformFacadeRelativePath = "Microsoft.Net\\assembly\\GAC_MSIL\\netstandard\\v4.0_2.0.0.0__cc7b13ffcd2ddd51\\netstandard.dll";
    const string ReviewedIdMetadataSha256 = "37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4";
    const string ReviewedIdObservationSha256 = "89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a";
    const uint FixtureProjectNumber = 1; // Owned synthetic data, not a real or claimed-valid project identity.
    const string ReviewedFactoryConstructorReportSha256 = "df361cb84fc232b6f7da6943114096cd3e14b638495dcc8b68e10dbd42524ea9";
    const string ReviewedFactoryCandidatesAuditSha256 = "8f18be8da62da7d06f11634467b3c869e2ac260f20956cb5ab31f552ff324432";
    const string Async8Identity = "Microsoft.Bcl.AsyncInterfaces, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
    static readonly string[] FactoryExtraNames = { "Autofac", "Knx.Kim.DataProvider" };
    static readonly string[] FactoryExtraHashes = { "747ee8605301b8bd4284cf16b7b8ab900356b7dd01f70800e08d2ff0286bbc12", "1158afb2d6c1bb18febcbc5e837c7248682cfafe44ecf7f38c853ecc6258671a" };
    static readonly string[] FactoryExtraIdentities = { "Autofac, Version=8.2.0.0, Culture=neutral, PublicKeyToken=17863af14b0044da", "Knx.Kim.DataProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159" };
    static readonly int[] FactoryPrefixSizes = { 452608, 6979024, 72704, 215504, 78800, 26376, 102856, 27944, 14204880, 139776, 503248, 318928 };
    static readonly int[] FactoryExtraSizes = { 374784, 282064 };
    static readonly string[] ContainerNames = { "System.Diagnostics.DiagnosticSource", "System.Memory", "System.Runtime.CompilerServices.Unsafe", "System.Buffers", "System.Numerics.Vectors" };
    static readonly string[] ContainerHashes = { "9c4f0069f18d0ffc71056562318d1caec0b60820188426d7005a39612853993c", "b8fe216aff0f6d162f8eefe7be1712162b7d8199e20ce2e70ffac36c7ce20a4c", "2d6f419d641682d8efc4a40e850d2e85de519d6823b93daf2aeee63206d4e32d", "3af1e07305bed0abe0cf32d9854e0a7f5f470216adee1b87d02350be09d31b35", "22c84be385fe8ba6d0e0138952748f28d781fce36078b5a7aed91a6104bcd99b" };
    static readonly string[] ContainerIdentities = { "System.Diagnostics.DiagnosticSource, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Memory, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Runtime.CompilerServices.Unsafe, Version=6.0.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Buffers, Version=4.0.4.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Numerics.Vectors, Version=4.1.5.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" };
    static readonly int[] ContainerSizes = { 199944, 145192, 19224, 23856, 110344 };
    static readonly string[] ContainerRequests = { "System.Diagnostics.DiagnosticSource, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Buffers, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" };
    static readonly string[] Names = { "MDT-DaliDcaPlus", "Knx.Ets.Sdk", "Knx.Ets.Sdk.AddIns.AddInViews", "Knx.Ets.Common.Types", "Knx.Ets.Common.Resources", "Microsoft.Bcl.AsyncInterfaces", "netstandard", "System.Threading.Tasks.Extensions", "Knx.Ets.ObjectModel", "FluentResults", "Knx.Ets.Common", "Knx.Ets.Ids" };
    static readonly string[] Hashes = { "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d", "7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989", "5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d", CommonHash, "6e07df9e247b1d5e6a11c9e8131ec8cc8ab8aabeca9af02b1911608d1eefe661", "1c95b1a776931ca1c4f2911b2ae1cc99eb78f547cd5feeaf5cdbdf10cedad0db", "bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb", "692c324c02a922f890aaf5948c54ec9758f5ab7019b8e8ab169c106ddd3231f4", "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a", "99adb97bc209d19696c9a42db8eceef49b55fc2c3f4cc6d6f5654fe7e5df9159", "4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd", "43b41d5e3a1eda83079c7f96d34ab94daa3d8823ce291efc262bd340d6c7d877" };
    static readonly string[] Identities = {
        "MDT-DaliDcaPlus, Version=4.0.0.0, Culture=neutral, PublicKeyToken=1a3211b0275a5581",
        "Knx.Ets.Sdk, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", CommonIdentity,
        "Knx.Ets.Common.Resources, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Microsoft.Bcl.AsyncInterfaces, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "System.Threading.Tasks.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", OmIdentity,
        "FluentResults, Version=3.16.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159" };
    static readonly List<FileStream> Inputs = new List<FileStream>();
    static readonly object LogLock = new object();
    static FileStream log;
    static string stage, mode, fixture;
    static volatile bool unexpected, rejected;
    static volatile bool enumInputsPinned;
    static volatile bool platformFacadePinned;
    static volatile bool factoryInputsReady, containerInputsReady;
    static string platformFacadePath;
    static int logBytes, assemblyCount;
    static object constructed;
    static bool EnumOnly { get { return mode == "project-enum-metadata"; } }
    static bool Original { get { return mode == "original-project" || mode == "original-project-store-id"; } }
    static bool OwnedProject { get { return mode == "owned-project" || mode == "owned-project-store-id"; } }
    static bool StoreIdFixture { get { return mode == "original-project-store-id" || mode == "owned-project-store-id"; } }
    static bool FactoryOriginal { get { return mode == "original-factory" || mode == "original-factory-bcl9" || mode == "original-factory-empty-container"; } }
    static bool FactoryOwned { get { return mode == "owned-factory" || mode == "owned-factory-bcl9" || mode == "owned-factory-empty-container"; } }
    static bool FactoryOnly { get { return FactoryOriginal || FactoryOwned; } }
    static bool EmptyContainer { get { return mode == "original-factory-empty-container" || mode == "owned-factory-empty-container"; } }
    static bool FactoryBinding { get { return EmptyContainer || mode == "original-factory-bcl9" || mode == "owned-factory-bcl9"; } }
    static bool FactoryProfileReady { get { return factoryInputsReady || (EmptyContainer && containerInputsReady); } }
    internal static string Text(string value)
    {
        value = value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\0", "\\0");
        return value.Length <= 512 ? value : value.Substring(0, 512) + "[truncated]";
    }
    static void Record(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) + " " + value + "\n");
        lock (LogLock) {
            ProbeFiles.Require(logBytes <= 65536 - bytes.Length, "PROJECT_LOG_LIMIT");
            log.Write(bytes, 0, bytes.Length); log.Flush(); logBytes += bytes.Length;
        }
    }
    static void Pin(string path, string hash)
    {
        var stream = new FileStream(ProbeFiles.Local(path, true, false), FileMode.Open, FileAccess.Read, FileShare.Read);
        Inputs.Add(stream);
        ProbeFiles.Require(stream.Length > 0 && stream.Length <= 16 * 1024 * 1024, "INPUT_SIZE");
        ProbeFiles.Require(ProbeFiles.Hash(stream) == hash, "INPUT_HASH");
        Record("PIN name=" + Path.GetFileName(path) + " bytes=" + stream.Length + " sha256=" + hash);
    }
    static void CountInputs(int expected)
    {
        int count = 0;
        foreach (string entry in Directory.EnumerateFileSystemEntries(stage)) { count++; ProbeFiles.Require(count <= expected, "PROJECT_EXACT_INPUTS"); }
        ProbeFiles.Require(count == expected, "PROJECT_EXACT_INPUTS");
    }
    static void PinEnumInputs()
    {
        CountInputs(2);
        string common = Path.Combine(stage, "Knx.Ets.Common.Types.dll"), facade = Path.Combine(stage, "netstandard.dll");
        Pin(common, CommonHash);
        Pin(facade, Hashes[6]);
        ProbeFiles.Require(AssemblyName.GetAssemblyName(common).FullName == CommonIdentity, "INPUT_IDENTITY");
        ProbeFiles.Require(AssemblyName.GetAssemblyName(facade).FullName == Identities[6], "INPUT_IDENTITY");
        enumInputsPinned = true;
    }
    static void PinPlatformFacade()
    {
        ProbeFiles.Require(Original || OwnedProject || FactoryOnly, "PLATFORM_FACADE_MODE");
        platformFacadePath = ProbeFiles.Local(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), PlatformFacadeRelativePath), true, false);
        if (OwnedProject || FactoryOwned) {
            ProbeFiles.Require(!platformFacadePinned && !PlatformFacadeAllowed(Identities[6], platformFacadePath, true, false), "OWNED_PLATFORM_FACADE_INCOMPLETE_PIN");
            Record("OWNED_PLATFORM_FACADE_INCOMPLETE_PIN_PASS matching-facts=true pin-complete=false");
        }
        var stream = new FileStream(platformFacadePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        Inputs.Add(stream);
        ProbeFiles.Require(stream.Length == 102856, "PLATFORM_FACADE_SIZE");
        ProbeFiles.Require(ProbeFiles.Hash(stream) == Hashes[6], "PLATFORM_FACADE_HASH");
        ProbeFiles.Require(AssemblyName.GetAssemblyName(platformFacadePath).FullName == Identities[6], "PLATFORM_FACADE_IDENTITY");
        platformFacadePinned = true;
        Record("PLATFORM_FACADE_PIN origin=FIXED_OS_GAC_PATH location=" + Text(platformFacadePath) + " bytes=" + stream.Length + " sha256=" + Hashes[6]);
    }
    // Private bounded predicate seam. Production facts come from Assembly and the
    // completed held-file pin, never caller-supplied identity/proof parameters.
    static bool PlatformFacadeAllowed(string identity, string location, bool cache, bool reflectionOnly)
    {
        return (Original || OwnedProject || FactoryOnly) && platformFacadePinned && identity == Identities[6]
            && ProbeFiles.Same(location, platformFacadePath) && cache && !reflectionOnly;
    }
    static void PinFactoryInputs()
    {
        CountInputs(14);
        for (int i = 0; i < Names.Length; i++) {
            Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count - 1].Length == FactoryPrefixSizes[i], "FACTORY_INPUT_SIZE");
        }
        for (int i = 0; i < FactoryExtraNames.Length; i++) {
            Pin(Path.Combine(stage, FactoryExtraNames[i] + ".dll"), FactoryExtraHashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count - 1].Length == FactoryExtraSizes[i], "FACTORY_INPUT_SIZE");
        }
        PinPlatformFacade();
        for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "INPUT_IDENTITY");
        for (int i = 0; i < FactoryExtraNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, FactoryExtraNames[i] + ".dll")).FullName == FactoryExtraIdentities[i], "FACTORY_INPUT_IDENTITY");
        factoryInputsReady = true;
        Record("FACTORY_PROFILE_READY private-pins=14 extra-reference-admission=THIS_MODE_ONLY");
    }
    static void PinContainerInputs()
    {
        CountInputs(19);
        for (int i = 0; i < Names.Length; i++) {
            Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count - 1].Length == FactoryPrefixSizes[i], "CONTAINER_INPUT_SIZE");
        }
        for (int i = 0; i < FactoryExtraNames.Length; i++) {
            Pin(Path.Combine(stage, FactoryExtraNames[i] + ".dll"), FactoryExtraHashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count - 1].Length == FactoryExtraSizes[i], "CONTAINER_INPUT_SIZE");
        }
        for (int i = 0; i < ContainerNames.Length; i++) {
            Pin(Path.Combine(stage, ContainerNames[i] + ".dll"), ContainerHashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count - 1].Length == ContainerSizes[i], "CONTAINER_INPUT_SIZE");
        }
        PinPlatformFacade();
        for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < FactoryExtraNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, FactoryExtraNames[i] + ".dll")).FullName == FactoryExtraIdentities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < ContainerNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, ContainerNames[i] + ".dll")).FullName == ContainerIdentities[i], "CONTAINER_INPUT_IDENTITY");
        containerInputsReady = true;
        Record("FACTORY_CONTAINER_PROFILE_READY private-pins=19 total-bytes=24278056 extra-reference-admission=THIS_MODE_ONLY");
    }
    static bool ContainerReferencePolicy(string selectedMode, bool complete, string identity, string location, bool reflectionOnly)
    {
        if (selectedMode != "original-factory-empty-container" || !complete || reflectionOnly) return false;
        for (int i = 0; i < ContainerNames.Length; i++)
            if (identity == ContainerIdentities[i] && ProbeFiles.Same(location, Path.Combine(stage, ContainerNames[i] + ".dll"))) return true;
        return false;
    }
    // Private finite policy seam; real facts come from the completed pin and Assembly.
    static bool FactoryReferencePolicy(string selectedMode, bool complete, string identity, string location, bool reflectionOnly)
    {
        if ((selectedMode != "original-factory" && selectedMode != "original-factory-bcl9" && selectedMode != "original-factory-empty-container") || !complete || reflectionOnly) return false;
        for (int i = 0; i < FactoryExtraNames.Length; i++)
            if (identity == FactoryExtraIdentities[i] && ProbeFiles.Same(location, Path.Combine(stage, FactoryExtraNames[i] + ".dll"))) return true;
        return false;
    }
    static bool Framework(Assembly assembly)
    {
        AssemblyName name = assembly.GetName();
        string token = BitConverter.ToString(name.GetPublicKeyToken()).Replace("-", "").ToLowerInvariant();
        bool allowed = name.Name == "mscorlib" || name.Name == "System" || name.Name.StartsWith("System.", StringComparison.Ordinal)
            || name.Name == "WindowsBase" || name.Name.StartsWith("Presentation", StringComparison.Ordinal)
            || name.Name.StartsWith("UIAutomation", StringComparison.Ordinal) || name.Name == "ReachFramework"
            || name.Name == "Accessibility" || name.Name == "Microsoft.CSharp";
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows).TrimEnd('\\') + "\\";
        return allowed && (token == "b77a5c561934e089" || token == "b03f5f7f11d50a3a" || token == "31bf3856ad364e35")
            && (assembly.Location.StartsWith(windows + "Microsoft.NET\\", StringComparison.OrdinalIgnoreCase)
                || assembly.Location.StartsWith(windows + "assembly\\", StringComparison.OrdinalIgnoreCase));
    }
    static void Audit(Assembly assembly)
    {
        if (Interlocked.Increment(ref assemblyCount) > 96) { unexpected = true; throw new IOException("ASSEMBLY_COUNT_BOUND"); }
        bool allowed = assembly == typeof(ProjectConstructorProbe).Assembly;
        if (!assembly.IsDynamic) {
            if (Framework(assembly)) allowed = true;
            bool platform = PlatformFacadeAllowed(assembly.FullName, assembly.Location, assembly.GlobalAssemblyCache, assembly.ReflectionOnly);
            if (platform) allowed = true;
            if ((Original || OwnedProject || FactoryOnly) && assembly.GetName().Name == "netstandard")
                Record("PLATFORM_FACADE_AUDIT alias-allowed=" + platform + " identity=" + Text(assembly.FullName) + " origin=" + (platform ? "FIXED_OS_GAC_PATH" : "UNQUALIFIED_FOR_ALIAS")
                    + " location=" + Text(assembly.Location) + " gac=" + assembly.GlobalAssemblyCache
                    + " reflection-only=" + assembly.ReflectionOnly
                    + " pinned-file-sha256=" + (platform ? Hashes[6] : "NOT_ESTABLISHED"));
            if (EnumOnly && enumInputsPinned && assembly.ReflectionOnly
                && ((assembly.FullName == CommonIdentity && ProbeFiles.Same(assembly.Location, Path.Combine(stage, "Knx.Ets.Common.Types.dll")))
                    || (assembly.FullName == Identities[6] && ProbeFiles.Same(assembly.Location, Path.Combine(stage, "netstandard.dll"))))) allowed = true;
            if (Original) for (int i = 0; i < Names.Length; i++)
                if (assembly.FullName == Identities[i] && ProbeFiles.Same(assembly.Location, Path.Combine(stage, Names[i] + ".dll"))) allowed = true;
            if (FactoryOriginal && FactoryProfileReady && !assembly.ReflectionOnly) for (int i = 0; i < Names.Length; i++)
                if (assembly.FullName == Identities[i] && ProbeFiles.Same(assembly.Location, Path.Combine(stage, Names[i] + ".dll"))) allowed = true;
            if (FactoryReferencePolicy(mode, FactoryProfileReady, assembly.FullName, assembly.Location, assembly.ReflectionOnly)) allowed = true;
            if (FactoryBinding && FactoryOriginal && assembly.GetName().Name == "Microsoft.Bcl.AsyncInterfaces") {
                bool pinned = FactoryProfileReady && assembly.FullName == Identities[5] && !assembly.ReflectionOnly
                    && ProbeFiles.Same(assembly.Location, Path.Combine(stage, Names[5] + ".dll"));
                Record("FACTORY_BINDING_ASSEMBLY identity=" + Text(assembly.FullName) + " location=" + Text(assembly.Location)
                    + " pinned=" + pinned + " held-pin-sha256=" + (pinned ? Hashes[5] : "NOT_ESTABLISHED") + " trace=ACTUAL_ASSEMBLY_LOAD");
            }
            if (!Original && !FactoryOriginal && !EnumOnly && assembly.FullName == "Owned.Task148.ConstructorFixtures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" && ProbeFiles.Same(assembly.Location, fixture)) allowed = true;
            // These five names never inherit Framework admission, including GAC paths.
            for (int i = 0; i < ContainerNames.Length; i++) if (assembly.GetName().Name == ContainerNames[i]) {
                allowed = ContainerReferencePolicy(mode, containerInputsReady, assembly.FullName, assembly.Location, assembly.ReflectionOnly);
                Record("CONTAINER_REFERENCE_AUDIT allowed=" + allowed + " identity=" + Text(assembly.FullName) + " location=" + Text(assembly.Location)
                    + " held-pin-sha256=" + (allowed ? ContainerHashes[i] : "NOT_ESTABLISHED") + " framework-fallback=false");
            }
        }
        Record("PROJECT_ASSEMBLY allowed=" + allowed + " reflection-only=" + assembly.ReflectionOnly + " identity=" + Text(assembly.FullName));
        if (!allowed) { unexpected = true; Record("UNEXPECTED_ASSEMBLY_BLOCKER detection=after-load prevention=NOT_ESTABLISHED"); }
    }
    static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        Record("PROJECT_RESOLVE requested=" + Text(args.Name));
        if (Original) {
            string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            for (int i = 1; i < Names.Length; i++) if (policy == Identities[i]) return Assembly.LoadFrom(Path.Combine(stage, Names[i] + ".dll"));
        }
        if (EmptyContainer && FactoryOriginal && containerInputsReady) {
            string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            Record("CONTAINER_BINDING_RESOLVE requested=" + Text(args.Name) + " post-policy=" + Text(policy));
            for (int i = 1; i < Names.Length; i++) if (policy == Identities[i]) return ResolvePinnedFactoryReference(args.Name, policy, Names[i], Hashes[i]);
            for (int i = 0; i < FactoryExtraNames.Length; i++) if (policy == FactoryExtraIdentities[i]) return ResolvePinnedFactoryReference(args.Name, policy, FactoryExtraNames[i], FactoryExtraHashes[i]);
            for (int i = 0; i < ContainerNames.Length; i++) if (policy == ContainerIdentities[i]) return ResolvePinnedFactoryReference(args.Name, policy, ContainerNames[i], ContainerHashes[i]);
        } else if (FactoryBinding && FactoryOriginal && factoryInputsReady) {
            string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            Record("FACTORY_BINDING_RESOLVE requested=" + Text(args.Name) + " post-policy=" + Text(policy));
            for (int i = 1; i < Names.Length; i++) if (policy == Identities[i]) return ResolvePinnedFactoryReference(args.Name, policy, Names[i], Hashes[i]);
            for (int i = 0; i < FactoryExtraNames.Length; i++) if (args.Name == FactoryExtraIdentities[i]) return ResolvePinnedFactoryReference(args.Name, policy, FactoryExtraNames[i], FactoryExtraHashes[i]);
        } else if (FactoryOriginal && factoryInputsReady) {
            string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            for (int i = 1; i < Names.Length; i++) if (policy == Identities[i]) return Assembly.LoadFrom(Path.Combine(stage, Names[i] + ".dll"));
            for (int i = 0; i < FactoryExtraNames.Length; i++) if (args.Name == FactoryExtraIdentities[i]) return Assembly.LoadFrom(Path.Combine(stage, FactoryExtraNames[i] + ".dll"));
        }
        rejected = true;
        throw new FileNotFoundException("PROJECT_DEPENDENCY_NOT_ALLOWLISTED", args.Name);
    }
    static Assembly ResolvePinnedFactoryReference(string requested, string policy, string name, string hash)
    {
        ProbeFiles.Require(FactoryBinding && FactoryOriginal && FactoryProfileReady && (!EmptyContainer || containerInputsReady), "FACTORY_BINDING_PIN_REQUIRED");
        string path = Path.Combine(stage, name + ".dll");
        Assembly assembly = Assembly.LoadFrom(path);
        bool facade = PlatformFacadeAllowed(assembly.FullName, assembly.Location, assembly.GlobalAssemblyCache, assembly.ReflectionOnly);
        ProbeFiles.Require(assembly.FullName == policy && (ProbeFiles.Same(assembly.Location, path) || facade) && !assembly.ReflectionOnly, "FACTORY_BINDING_RESOLVED_IDENTITY_PATH");
        Record("FACTORY_BINDING_RESOLVED requested=" + Text(requested) + " post-policy=" + Text(policy) + " location=" + Text(assembly.Location)
            + " origin=" + (facade ? "EXISTING_FIXED_OS_FACADE" : "FIXED_PRIVATE_STAGE") + " held-pin-sha256=" + hash);
        return assembly;
    }
    static void VerifyBindingConfiguration()
    {
        string config = ProbeFiles.Local(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true, false);
        string expected = EmptyContainer ? ProbeFiles.FactoryContainerConfigText : FactoryBinding ? ProbeFiles.FactoryBindingConfigText : ProbeFiles.ConfigText;
        ProbeFiles.Require(new FileInfo(config).Length <= 4096 && File.ReadAllText(config) == expected, "EXACT_CONFIG_REQUIRED");
        if (FactoryBinding) ProbeFiles.Require(Encoding.UTF8.GetString(File.ReadAllBytes(config)) == expected, "EXACT_CONFIG_BYTES_REQUIRED");
        bool bindingExe = Path.GetFileName(typeof(ProjectConstructorProbe).Assembly.Location) == "ProjectFactoryBindingProbe.exe";
        ProbeFiles.Require(bindingExe == (FactoryBinding && !EmptyContainer), "FACTORY_BINDING_MODE_EXECUTABLE");
        ProbeFiles.Require((Path.GetFileName(typeof(ProjectConstructorProbe).Assembly.Location) == "ProjectFactoryContainerProbe.exe") == EmptyContainer, "FACTORY_CONTAINER_MODE_EXECUTABLE");
        if (!FactoryBinding) {
            if (FactoryOwned) {
                ProbeFiles.Require(AppDomain.CurrentDomain.ApplyPolicy(Async8Identity) == Async8Identity, "OWNED_FACTORY_LEGACY_POLICY");
                Record("OWNED_FACTORY_LEGACY_POLICY_PASS version8=UNCHANGED no-target-load=true");
                VerifyContainerPolicies(false);
            }
            return;
        }
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) ProbeFiles.Require(assembly.GetName().Name != "Microsoft.Bcl.AsyncInterfaces", "FACTORY_POLICY_TARGET_ALREADY_LOADED");
        string mapped = AppDomain.CurrentDomain.ApplyPolicy(Async8Identity);
        Record("FACTORY_BINDING_POLICY requested=" + Async8Identity + " post-policy=" + Text(mapped) + " trace=EXPLICIT_CLR_POLICY_PREFLIGHT target-load=NOT_RUN");
        ProbeFiles.Require(mapped == Identities[5], "FACTORY_BINDING_POLICY_EXACT_8_TO_9");
        foreach (string identity in new string[] {
            Async8Identity.Replace("8.0.0.0", "7.0.0.0"), Async8Identity.Replace("8.0.0.0", "8.0.0.1"),
            Async8Identity.Replace("cc7b13ffcd2ddd51", "cc7b13ffcd2ddd50"), Async8Identity.Replace("Culture=neutral", "Culture=fr"),
            Async8Identity.Replace("Microsoft.Bcl.AsyncInterfaces,", "Owned.WrongName,")
        }) ProbeFiles.Require(AppDomain.CurrentDomain.ApplyPolicy(identity) == identity, "FACTORY_BINDING_POLICY_NEGATIVE");
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) ProbeFiles.Require(assembly.GetName().Name != "Microsoft.Bcl.AsyncInterfaces", "FACTORY_POLICY_TARGET_LOADED");
        Record("FACTORY_BINDING_POLICY_PASS exact8-to9=true version7-patch-token-culture-name=UNCHANGED target-load=NOT_RUN");
        if (EmptyContainer || FactoryOwned) VerifyContainerPolicies(EmptyContainer);
    }
    static void VerifyContainerPolicies(bool enabled)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (string name in ContainerNames) ProbeFiles.Require(assembly.GetName().Name != name, "CONTAINER_POLICY_TARGET_ALREADY_LOADED");
        for (int i = 0; i < ContainerRequests.Length; i++) {
            string requested = ContainerRequests[i], mapped = AppDomain.CurrentDomain.ApplyPolicy(requested);
            ProbeFiles.Require(mapped == (enabled ? ContainerIdentities[i] : requested), "CONTAINER_BINDING_POLICY_EXACT");
            Record("CONTAINER_BINDING_POLICY enabled=" + enabled + " requested=" + requested + " post-policy=" + Text(mapped) + " target-load=NOT_RUN");
            AssemblyName name = new AssemblyName(requested);
            foreach (string identity in new string[] {
                requested.Replace(name.Version.ToString(), "0.0.0.0"),
                requested.Replace(name.Version.ToString(), new Version(name.Version.Major, name.Version.Minor, name.Version.Build, name.Version.Revision + 1).ToString()),
                requested.Substring(0, requested.Length - 1) + "0", requested.Replace("Culture=neutral", "Culture=fr"), requested.Replace(name.Name + ",", "Owned.WrongName,")
            }) ProbeFiles.Require(AppDomain.CurrentDomain.ApplyPolicy(identity) == identity, "CONTAINER_BINDING_POLICY_NEGATIVE");
        }
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (string name in ContainerNames) ProbeFiles.Require(assembly.GetName().Name != name, "CONTAINER_POLICY_TARGET_LOADED");
        Record("CONTAINER_BINDING_POLICY_PASS enabled=" + enabled + " exact4=true near-misses=UNCHANGED target-load=NOT_RUN vector-redirect=NONE");
    }
    static Assembly ReflectionResolve(object sender, ResolveEventArgs args)
    {
        if (EnumOnly && enumInputsPinned && args.Name == Identities[6]) {
            Assembly facade = Assembly.ReflectionOnlyLoadFrom(Path.Combine(stage, "netstandard.dll"));
            ProbeFiles.Require(facade.ReflectionOnly && facade.FullName == Identities[6]
                && ProbeFiles.Same(facade.Location, Path.Combine(stage, "netstandard.dll")), "PROJECT_REFLECTION_FACADE_IDENTITY");
            Record("PROJECT_REFLECTION_FACADE_ALLOW requested=" + Text(args.Name));
            return facade;
        }
        rejected = true; Record("PROJECT_REFLECTION_DEPENDENCY_REJECT requested=" + Text(args.Name));
        throw new FileNotFoundException("PROJECT_REFLECTION_DEPENDENCY_NOT_ALLOWLISTED", args.Name);
    }
    static void EnumFields(Type type, Action<string> record)
    {
        ProbeFiles.Require(type != null && type.IsEnum && type.IsPublic, "PROJECT_ENUM_REQUIRED");
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(fields.Length > 1 && fields.Length <= 64, "PROJECT_ENUM_FIELD_BOUND");
        Type underlying = Enum.GetUnderlyingType(type);
        string code = underlying.FullName;
        ProbeFiles.Require(code == "System.SByte" || code == "System.Byte" || code == "System.Int16" || code == "System.UInt16" || code == "System.Int32" || code == "System.UInt32" || code == "System.Int64" || code == "System.UInt64", "PROJECT_ENUM_UNDERLYING");
        record("PROJECT_ENUM type=" + Text(type.FullName) + " token=" + type.MetadataToken.ToString("X8") + " underlying=" + underlying.FullName + " fields=" + fields.Length + " reflection-only=" + type.Assembly.ReflectionOnly);
        int count = 0;
        foreach (FieldInfo field in fields) {
            if (!field.IsStatic) { ProbeFiles.Require(field.Name == "value__" && field.FieldType == underlying, "PROJECT_ENUM_STORAGE"); continue; }
            ProbeFiles.Require(field.IsPublic && field.IsLiteral && field.FieldType == type, "PROJECT_ENUM_LITERAL");
            object raw = field.GetRawConstantValue();
            ProbeFiles.Require(raw != null && raw.GetType().FullName == underlying.FullName, "PROJECT_ENUM_CONSTANT_TYPE");
            record("PROJECT_ENUM_LITERAL name=" + Text(field.Name) + " token=" + field.MetadataToken.ToString("X8") + " raw=" + Convert.ToString(raw, CultureInfo.InvariantCulture));
            count++;
        }
        ProbeFiles.Require(count > 0, "PROJECT_ENUM_NO_LITERALS");
    }
    static void InvokeProject(ConstructorInfo ctor, object factory, string name, object style, object store, Func<int> requests)
    {
        bool returned = false;
        Record("PROJECT_CONSTRUCTOR_BEGIN type=" + ctor.DeclaringType.FullName + " token=" + ctor.MetadataToken.ToString("X8") + " services=deny-all");
        try { constructed = ctor.Invoke(new object[] { factory, name, style, store }); returned = true; }
        finally { Record("PROJECT_CONSTRUCTOR_END returned=" + returned + " denied-requests=" + requests() + " qualification=NOT_ESTABLISHED ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN"); }
    }
    static ConstructorInfo ProjectCtor(Type type, Type factory, Type style, Type store)
    {
        ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { factory, typeof(string), style, store }, null);
        ProbeFiles.Require(ctor != null && !type.IsAbstract, "PROJECT_PUBLIC_CONSTRUCTOR_SIGNATURE"); return ctor;
    }
    static void OriginalTokens(Type project, ConstructorInfo ctor)
    {
        ProbeFiles.Require(project.IsPublic && project.Assembly.FullName == OmIdentity && project.MetadataToken == 0x020003a5
            && ctor.MetadataToken == 0x06002830, "PROJECT_ORIGINAL_TOKENS");
    }
    static void IdSignature(Type type, ConstructorInfo ctor, MethodInfo getter)
    {
        ProbeFiles.Require(type != null && type.IsPublic && type.IsClass && !type.IsAbstract && !type.IsGenericType && !type.ContainsGenericParameters,
            "PROJECT_ID_TYPE");
        ProbeFiles.Require(ctor != null && ctor.DeclaringType == type && ctor.IsPublic && !ctor.IsStatic
            && ctor.GetParameters().Length == 1 && ctor.GetParameters()[0].ParameterType == typeof(uint), "PROJECT_ID_CONSTRUCTOR_SIGNATURE");
        ProbeFiles.Require(getter != null && getter.DeclaringType == type && getter.Name == "get_ProjectNumber" && getter.IsPublic && getter.IsSpecialName
            && !getter.IsStatic && !getter.IsGenericMethod && !getter.ContainsGenericParameters && getter.GetParameters().Length == 0
            && getter.ReturnType == typeof(uint), "PROJECT_ID_GETTER_SIGNATURE");
    }
    internal static void StoreIdSignature(Type owner, MethodInfo getter, Type id)
    {
        ProbeFiles.Require(owner != null && owner.IsPublic && owner.IsInterface && !owner.IsGenericType && id != null
            && getter != null && getter.DeclaringType == owner && getter.Name == "get_Id" && getter.IsPublic && getter.IsSpecialName
            && !getter.IsStatic && !getter.IsGenericMethod && !getter.ContainsGenericParameters
            && getter.GetParameters().Length == 0 && getter.ReturnType == id, "PROJECT_STORE_ID_SIGNATURE");
    }
    static void OriginalIdTokens(Type id, ConstructorInfo ctor, MethodInfo getter, Type store, MethodInfo storeGetter)
    {
        IdSignature(id, ctor, getter); StoreIdSignature(store, storeGetter, id);
        ProbeFiles.Require(id.FullName == "Knx.Ets.Ids.Project.ProjectId" && id.Assembly.FullName == Identities[11]
            && (int)id.Attributes == 1056769 && store.FullName == "Knx.Ets.ObjectModel.IProjectStore" && store.Assembly.FullName == OmIdentity
            && ReviewedIdTokens(id.MetadataToken, ctor.MetadataToken, getter.MetadataToken, store.MetadataToken, storeGetter.MetadataToken), "PROJECT_ID_ORIGINAL_TOKENS");
    }
    // Production inputs are reflected members from the fixed pinned assemblies.
    // The owned tests exercise this finite token policy without forging CLR metadata.
    static bool ReviewedIdTokens(int type, int ctor, int getter, int owner, int storeGetter)
    { return type == 0x0200002c && ctor == 0x060001ef && getter == 0x060001f2 && owner == 0x02000057 && storeGetter == 0x060002f5; }
    static object CreateFixtureId(Type id, ConstructorInfo ctor, MethodInfo getter)
    {
        IdSignature(id, ctor, getter);
        object value; bool returned = false;
        Record("PROJECT_ID_CONSTRUCTOR_BEGIN type=" + id.FullName + " token=" + ctor.MetadataToken.ToString("X8")
            + " fixture-uint32=1 identity=OWNED_SYNTHETIC_NOT_REAL_OR_CLAIMED_VALID metadata-sha256=" + ReviewedIdMetadataSha256
            + " observation-sha256=" + ReviewedIdObservationSha256);
        try { value = ctor.Invoke(new object[] { FixtureProjectNumber }); returned = true; }
        finally { Record("PROJECT_ID_CONSTRUCTOR_END returned=" + returned + " body-entry=NOT_ESTABLISHED_BY_STACK_TRACE qualification=NOT_ESTABLISHED"); }
        ProbeFiles.Require(value != null && value.GetType() == id, "PROJECT_ID_EXACT_RETURN_TYPE");
        ProbeFiles.Require(!unexpected && !rejected, "PROJECT_ID_CONSTRUCTOR_DEPENDENCY");
        bool getterReturned = false, validated = false;
        Record("PROJECT_ID_GETTER_BEGIN type=" + id.FullName + " token=" + getter.MetadataToken.ToString("X8"));
        try {
            object number = getter.Invoke(value, new object[0]); getterReturned = true;
            ProbeFiles.Require(number != null && number.GetType() == typeof(uint) && (uint)number == FixtureProjectNumber, "PROJECT_ID_FIXTURE_NUMBER");
            ProbeFiles.Require(!unexpected && !rejected, "PROJECT_ID_GETTER_DEPENDENCY");
            validated = true;
        } finally { Record("PROJECT_ID_GETTER_END returned=" + getterReturned + " fixture-number-validated=" + validated + " qualification=NOT_ESTABLISHED"); }
        return value;
    }
    static void InvokeProjectWithId(ConstructorInfo ctor, object factory, object style, ProjectStoreIdProxy store, Func<int> factoryDenied)
    {
        bool returned = false;
        Record("PROJECT_CONSTRUCTOR_BEGIN type=" + ctor.DeclaringType.FullName + " token=" + ctor.MetadataToken.ToString("X8") + " services=fixture-id-only");
        try { constructed = ctor.Invoke(new object[] { factory, FixtureName, style, store.GetTransparentProxy() }); returned = true; }
        finally { Record("PROJECT_CONSTRUCTOR_END returned=" + returned + " fixture-calls=" + store.FixtureCalls + " denied-requests=" + (factoryDenied() + store.DeniedRequests)
            + " qualification=NOT_ESTABLISHED ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN"); }
    }
    static void OriginalProjectStoreId()
    {
        Record("PROJECT_PHASE original-fixture-type-materialization");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll"));
        Assembly common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.Types.dll"));
        Assembly ids = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Ids.dll"));
        Type project = om.GetType("Knx.Ets.ObjectModel.Project.Project", true, false);
        Type factory = om.GetType("Knx.Ets.ObjectModel.IFactory", true, false), store = om.GetType("Knx.Ets.ObjectModel.IProjectStore", true, false);
        Type style = common.GetType(EnumName, true, false), id = ids.GetType("Knx.Ets.Ids.Project.ProjectId", true, false);
        ProbeFiles.Require(project.Assembly == om && factory.Assembly == om && store.Assembly == om && style.Assembly == common && id.Assembly == ids, "PROJECT_TYPE_OWNERSHIP");
        ConstructorInfo ctor = ProjectCtor(project, factory, style, store); OriginalTokens(project, ctor);
        ConstructorInfo idCtor = id.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { typeof(uint) }, null);
        MethodInfo idGetter = id.GetMethod("get_ProjectNumber", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        MethodInfo storeGetter = store.GetMethod("get_Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        OriginalIdTokens(id, idCtor, idGetter, store, storeGetter);
        FieldInfo literal = style.GetField(ReviewedLiteral, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(style.IsEnum && style.MetadataToken == ReviewedEnumTypeToken && Enum.GetUnderlyingType(style).FullName == ReviewedUnderlyingType
            && literal != null && literal.IsLiteral && literal.FieldType == style && literal.MetadataToken == ReviewedLiteralToken, "PROJECT_REVIEWED_ENUM_LITERAL");
        object raw = literal.GetRawConstantValue();
        ProbeFiles.Require(raw != null && raw.GetType().FullName == ReviewedUnderlyingType && Convert.ToString(raw, CultureInfo.InvariantCulture) == ReviewedRawValue, "PROJECT_REVIEWED_ENUM_VALUE");
        object selectedStyle = Enum.ToObject(style, raw);
        ProbeFiles.Require(!unexpected && !rejected, "PROJECT_PREFIXTURE_DEPENDENCY");
        object value = CreateFixtureId(id, idCtor, idGetter);
        var f = new ProjectDenyProxy(factory, Record); var s = new ProjectStoreIdProxy(store, storeGetter, value, Record);
        ProbeFiles.Require(!unexpected && !rejected, "PROJECT_PRECONSTRUCTOR_DEPENDENCY");
        InvokeProjectWithId(ctor, f.GetTransparentProxy(), selectedStyle, s, delegate { return f.Requests; });
        GC.KeepAlive(f); GC.KeepAlive(s); GC.KeepAlive(value);
    }
    static void OriginalProject()
    {
        ProbeFiles.Require(OriginalConstructorReady && ReviewedEnumReportSha256.Length == 64 && ReviewedLiteral.Length > 0 && ReviewedRawValue.Length > 0 && ReviewedLiteralToken != 0
            && ReviewedEnumTypeToken != 0 && ReviewedUnderlyingType.Length > 0, "ENUM_LITERAL_NOT_REVIEWED");
        Record("PROJECT_PHASE original-type-materialization");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll"));
        Assembly common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.Types.dll"));
        Type project = om.GetType("Knx.Ets.ObjectModel.Project.Project", true, false);
        Type factory = om.GetType("Knx.Ets.ObjectModel.IFactory", true, false), store = om.GetType("Knx.Ets.ObjectModel.IProjectStore", true, false);
        Type style = common.GetType(EnumName, true, false);
        ProbeFiles.Require(project.Assembly == om && factory.Assembly == om && store.Assembly == om && style.Assembly == common, "PROJECT_TYPE_OWNERSHIP");
        ConstructorInfo ctor = ProjectCtor(project, factory, style, store);
        OriginalTokens(project, ctor);
        FieldInfo literal = style.GetField(ReviewedLiteral, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(style.IsEnum && style.MetadataToken == ReviewedEnumTypeToken && Enum.GetUnderlyingType(style).FullName == ReviewedUnderlyingType
            && literal != null && literal.IsLiteral && literal.FieldType == style && literal.MetadataToken == ReviewedLiteralToken, "PROJECT_REVIEWED_ENUM_LITERAL");
        object raw = literal.GetRawConstantValue();
        ProbeFiles.Require(raw != null && raw.GetType().FullName == ReviewedUnderlyingType && Convert.ToString(raw, CultureInfo.InvariantCulture) == ReviewedRawValue, "PROJECT_REVIEWED_ENUM_VALUE");
        object selectedStyle = Enum.ToObject(style, raw);
        Record("PROJECT_PHASE deny-proxy-materialization enum-report-sha256=" + ReviewedEnumReportSha256);
        var f = new ProjectDenyProxy(factory, Record); var s = new ProjectDenyProxy(store, Record);
        ProbeFiles.Require(!unexpected && !rejected, "PROJECT_PRECONSTRUCTOR_DEPENDENCY");
        InvokeProject(ctor, f.GetTransparentProxy(), FixtureName, selectedStyle, s.GetTransparentProxy(), delegate { return f.Requests + s.Requests; });
        GC.KeepAlive(f); GC.KeepAlive(s);
    }
    static void Owned()
    {
        // Only the original Microsoft facade is loaded here, never an SDK input.
        Assembly platform = Assembly.Load(Identities[6]);
        ProbeFiles.Require(PlatformFacadeAllowed(platform.FullName, platform.Location, platform.GlobalAssemblyCache, platform.ReflectionOnly), "OWNED_PLATFORM_FACADE");
        ProbeFiles.Require(!PlatformFacadeAllowed("Owned.WrongIdentity", platform.Location, true, false)
            && !PlatformFacadeAllowed(platform.FullName, Path.Combine(stage, "netstandard.dll"), true, false)
            && !PlatformFacadeAllowed(platform.FullName, platform.Location, false, false)
            && !PlatformFacadeAllowed(platform.FullName, platform.Location, true, true), "OWNED_PLATFORM_FACADE_NEGATIVE");
        Record("OWNED_PLATFORM_FACADE_PASS microsoft-only=true exact-pin=true wrong-identity-path-cache-reflection=denied");
        Assembly metadata = Assembly.ReflectionOnlyLoadFrom(fixture);
        string enumLines = "";
        EnumFields(metadata.GetType("Owned.Task009.GroupAddressStyleFixture", true, false), delegate(string line) { enumLines += line + "\n"; Record(line); });
        ProbeFiles.Require(enumLines.Contains("raw=-2") && enumLines.Contains("raw=7") && enumLines.Contains("underlying=System.Int16") && enumLines.Contains("reflection-only=True"), "OWNED_ENUM_VALUES");
        foreach (string name in new string[] { "Owned.Task009.NotEnumFixture", "Owned.Task009.TooManyFieldsFixture", "Owned.Task009.Absent" }) {
            bool denied = false;
            try { EnumFields(metadata.GetType(name, false, false), Record); } catch (IOException) { denied = true; }
            ProbeFiles.Require(denied, "OWNED_ENUM_NEGATIVE");
        }
        Record("OWNED_PROJECT_ENUM_PASS literals=true signed=true reflection-only=true missing=true not-enum=true field-bound=true cctor=NOT_RUN");
        Assembly assembly = Assembly.LoadFrom(fixture);
        Type factory = assembly.GetType("Owned.Task009.IFactoryFixture", true), store = assembly.GetType("Owned.Task009.IProjectStoreFixture", true), style = assembly.GetType("Owned.Task009.GroupAddressStyleFixture", true);
        Type throwsType = assembly.GetType("Owned.Task009.ProjectThrowsFixture", true);
        bool wrongSignature = false, wrongToken = false;
        try { ProjectCtor(throwsType, store, style, factory); } catch (IOException) { wrongSignature = true; }
        try { OriginalTokens(throwsType, ProjectCtor(throwsType, factory, style, store)); } catch (IOException) { wrongToken = true; }
        ProbeFiles.Require(wrongSignature && wrongToken, "OWNED_PROJECT_SIGNATURE_TOKEN_REJECTION");
        var f = new ProjectDenyProxy(factory, Record); var s = new ProjectDenyProxy(store, Record);
        assembly.GetType("Owned.Task009.ProjectAssertions", true).GetMethod("AssertAll").Invoke(null, new object[] { f.GetTransparentProxy(), s.GetTransparentProxy() });
        ProbeFiles.Require(f.Requests == 2 && s.Requests == 17, "OWNED_PROJECT_DENIAL_COUNTS");
        Record("OWNED_PROJECT_DENIAL_PASS inherited=true generic=true ref-out=true void=true property=true saturation=true");
        object selected = Enum.ToObject(style, (short)7); // Deliberate owned value, never an original enum.
        foreach (string name in new string[] { "ProjectThrowsFixture", "ProjectCatchesFixture", "ProjectNoRequestsFixture" }) {
            var cf = new ProjectDenyProxy(factory, Record); var cs = new ProjectDenyProxy(store, Record);
            Type type = assembly.GetType("Owned.Task009." + name, true);
            bool failed = false;
            try { InvokeProject(ProjectCtor(type, factory, style, store), cf.GetTransparentProxy(), FixtureName, selected, cs.GetTransparentProxy(), delegate { return cf.Requests + cs.Requests; }); }
            catch (TargetInvocationException error) { failed = error.InnerException is NotSupportedException; }
            ProbeFiles.Require(failed == (name == "ProjectThrowsFixture"), "OWNED_PROJECT_CONSTRUCTOR_RESULT");
            if (failed) ProbeFiles.Require(Object.Equals(type.GetMethod("CalledOnce").Invoke(null, new object[0]), true), "OWNED_PROJECT_ONCE");
        }
        Record("OWNED_PROJECT_CONSTRUCTOR_PASS denied=true caught-denial-unqualified=true no-request-unqualified=true original=NOT_RUN");
    }
    static void OwnedProjectStoreId()
    {
        Owned(); // Retain the existing owned-only enum, platform and deny-all regressions.
        Assembly assembly = Assembly.LoadFrom(fixture);
        Type id = assembly.GetType("Owned.Task009.ProjectIdFixture", true), store = assembly.GetType("Owned.Task009.IProjectIdStoreFixture", true);
        Type foreign = assembly.GetType("Owned.Task009.IForeignIdStoreFixture", true);
        ConstructorInfo ctor = id.GetConstructor(new Type[] { typeof(uint) });
        MethodInfo getter = id.GetMethod("get_ProjectNumber"), storeGetter = store.GetMethod("get_Id");
        Type wrong = assembly.GetType("Owned.Task009.WrongIdShapeFixture", true);
        Type privateCtor = assembly.GetType("Owned.Task009.PrivateIdCtorFixture", true), privateGetter = assembly.GetType("Owned.Task009.PrivateIdGetterFixture", true);
        int rejections = 0;
        foreach (Action action in new Action[] {
            delegate { IdSignature(typeof(string), ctor, getter); },
            delegate { IdSignature(id, id.GetConstructor(new Type[] { typeof(string) }), getter); },
            delegate { IdSignature(id, ctor, wrong.GetMethod("get_ProjectNumber")); },
            delegate { IdSignature(wrong, wrong.GetConstructor(new Type[] { typeof(uint) }), wrong.GetMethod("get_ProjectNumber")); },
            delegate { IdSignature(privateCtor, privateCtor.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(uint) }, null), privateCtor.GetMethod("get_ProjectNumber")); },
            delegate { IdSignature(privateGetter, privateGetter.GetConstructor(new Type[] { typeof(uint) }), privateGetter.GetMethod("get_ProjectNumber", BindingFlags.NonPublic | BindingFlags.Instance)); },
            delegate { StoreIdSignature(store, foreign.GetMethod("get_Id"), id); },
            delegate { StoreIdSignature(store, store.GetMethod("get_Name"), id); },
            delegate { StoreIdSignature(store, storeGetter, typeof(string)); },
            delegate { new ProjectStoreIdProxy(store, storeGetter, new object(), Record); },
            delegate { OriginalIdTokens(id, ctor, getter, store, storeGetter); }
        }) { bool denied = false; try { action(); } catch (IOException) { denied = true; } ProbeFiles.Require(denied, "OWNED_ID_CONTRACT_REJECTION"); rejections++; }
        int[] tokens = { 0x0200002c, 0x060001ef, 0x060001f2, 0x02000057, 0x060002f5 };
        ProbeFiles.Require(ReviewedIdTokens(tokens[0], tokens[1], tokens[2], tokens[3], tokens[4]), "OWNED_ID_TOKEN_POLICY");
        for (int i = 0; i < tokens.Length; i++) { tokens[i]++; ProbeFiles.Require(!ReviewedIdTokens(tokens[0], tokens[1], tokens[2], tokens[3], tokens[4]), "OWNED_ID_TOKEN_NEGATIVE"); tokens[i]--; }
        ProbeFiles.Require(Object.Equals(id.GetMethod("Untouched").Invoke(null, new object[0]), true), "OWNED_ID_NO_EARLY_SIDE_EFFECT");
        Record("OWNED_ID_CONTRACT_PASS rejected=" + rejections + " before-construction=true token-policy-negatives=5 no-original-token-impersonation=true");
        object value = CreateFixtureId(id, ctor, getter);
        var proxy = new ProjectStoreIdProxy(store, storeGetter, value, Record);
        assembly.GetType("Owned.Task009.ProjectIdAssertions", true).GetMethod("AssertAll").Invoke(null, new object[] { value, proxy.GetTransparentProxy() });
        ProbeFiles.Require(proxy.FixtureCalls == 2 && proxy.DeniedRequests == 17, "OWNED_ID_PROXY_COUNTS");
        var saturated = new ProjectStoreIdProxy(store, storeGetter, value, Record);
        assembly.GetType("Owned.Task009.ProjectIdAssertions", true).GetMethod("AssertSaturation").Invoke(null, new object[] { value, saturated.GetTransparentProxy() });
        ProbeFiles.Require(saturated.FixtureCalls == 16 && saturated.DeniedRequests == 17, "OWNED_ID_PROXY_SATURATION");
        ProbeFiles.Require(Object.Equals(id.GetMethod("CreatedAndValidatedOnce").Invoke(null, new object[0]), true), "OWNED_ID_ONCE");
        foreach (string name in new string[] { "ThrowingIdFixture", "WrongNumberIdFixture" }) {
            Type bad = assembly.GetType("Owned.Task009." + name, true); bool denied = false;
            try { CreateFixtureId(bad, bad.GetConstructor(new Type[] { typeof(uint) }), bad.GetMethod("get_ProjectNumber")); }
            catch (TargetInvocationException) { denied = name == "ThrowingIdFixture"; }
            catch (IOException) { denied = name == "WrongNumberIdFixture"; }
            ProbeFiles.Require(denied, "OWNED_ID_CREATION_FAILURE");
            ProbeFiles.Require(Object.Equals(bad.GetMethod("AttemptedOnce").Invoke(null, new object[0]), true), "OWNED_ID_FAILURE_ONCE");
        }
        Type factory = assembly.GetType("Owned.Task009.IFactoryFixture", true), style = assembly.GetType("Owned.Task009.GroupAddressStyleFixture", true);
        var f = new ProjectDenyProxy(factory, Record); var s = new ProjectStoreIdProxy(store, storeGetter, value, Record);
        Type project = assembly.GetType("Owned.Task009.ProjectReadsIdFixture", true);
        InvokeProjectWithId(ProjectCtor(project, factory, style, store), f.GetTransparentProxy(), Enum.ToObject(style, (short)7), s, delegate { return f.Requests; });
        ProbeFiles.Require(s.FixtureCalls == 2 && s.DeniedRequests == 0 && f.Requests == 0, "OWNED_ID_PROJECT_CALLS");
        Record("OWNED_PROJECT_STORE_ID_PASS created=1 validated=1 stable-object=true unknown-generic-ref-out-void-property-malformed=denied bounded=true original=NOT_RUN qualification=NOT_ESTABLISHED");
    }
    static ConstructorInfo FactoryCtor(Type type, Type first, Type second)
    {
        ProbeFiles.Require(type != null && type.IsPublic && type.IsClass && !type.IsAbstract && !type.IsGenericType && !type.ContainsGenericParameters,
            "FACTORY_PUBLIC_TYPE_REQUIRED");
        foreach (Type parameter in new Type[] { first, second })
            ProbeFiles.Require(parameter != null && parameter.IsPublic && parameter.IsInterface && !parameter.IsGenericType && !parameter.ContainsGenericParameters,
                "FACTORY_PUBLIC_INTERFACE_REQUIRED");
        ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { first, second }, null);
        ProbeFiles.Require(ctor != null && ctor.DeclaringType == type && ctor.IsPublic && !ctor.IsStatic
            && ctor.GetParameters().Length == 2 && ctor.GetParameters()[0].ParameterType == first && ctor.GetParameters()[1].ParameterType == second,
            "FACTORY_PUBLIC_CONSTRUCTOR_SIGNATURE");
        return ctor;
    }
    static bool ReviewedFactoryTokens(int type, int ctor) { return type == 0x02000023 && ctor == 0x0600012e; }
    static void OriginalFactoryTokens(Type type, ConstructorInfo ctor, Type first, Type second)
    {
        ProbeFiles.Require(type.FullName == "Knx.Ets.ObjectModel.Factory" && type.Assembly.FullName == OmIdentity && (int)type.Attributes == 1048577
            && ctor.DeclaringType == type && (int)ctor.Attributes == 6278 && ReviewedFactoryTokens(type.MetadataToken, ctor.MetadataToken)
            && first.FullName == "Autofac.IComponentContext" && first.Assembly.FullName == FactoryExtraIdentities[0]
            && second.FullName == "Knx.Kim.DataProvider.IKimDataProvider" && second.Assembly.FullName == FactoryExtraIdentities[1], "FACTORY_ORIGINAL_CONTRACT");
    }
    static void InvokeFactory(ConstructorInfo ctor, ProjectDenyProxy first, ProjectDenyProxy second)
    {
        bool returned = false;
        Record("FACTORY_CONSTRUCTOR_BEGIN type=" + ctor.DeclaringType.FullName + " token=" + ctor.MetadataToken.ToString("X8")
            + " services=deny-all constructor-only=true report-sha256=" + ReviewedFactoryConstructorReportSha256 + " candidate-audit-sha256=" + ReviewedFactoryCandidatesAuditSha256);
        try {
            constructed = ctor.Invoke(new object[] { first.GetTransparentProxy(), second.GetTransparentProxy() }); returned = true;
            ProbeFiles.Require(constructed != null && constructed.GetType() == ctor.DeclaringType, "FACTORY_EXACT_RETURN_TYPE");
        } finally {
            Record("FACTORY_CONSTRUCTOR_END returned=" + returned + " denied-requests=" + (first.Requests + second.Requests)
                + " qualification=NOT_ESTABLISHED project=NOT_RUN Id=NOT_RUN UndoManager=NOT_RUN DCA=NOT_RUN ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
        }
    }
    static void OriginalFactory()
    {
        Record("FACTORY_PHASE original-type-materialization");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll"));
        Assembly autofac = Assembly.LoadFrom(Path.Combine(stage, FactoryExtraNames[0] + ".dll"));
        Assembly kim = Assembly.LoadFrom(Path.Combine(stage, FactoryExtraNames[1] + ".dll"));
        Type factory = om.GetType("Knx.Ets.ObjectModel.Factory", true, false);
        Type first = autofac.GetType("Autofac.IComponentContext", true, false), second = kim.GetType("Knx.Kim.DataProvider.IKimDataProvider", true, false);
        ProbeFiles.Require(factory.Assembly == om && first.Assembly == autofac && second.Assembly == kim, "FACTORY_TYPE_OWNERSHIP");
        ConstructorInfo ctor = FactoryCtor(factory, first, second); OriginalFactoryTokens(factory, ctor, first, second);
        Record("FACTORY_INTERFACE type=" + first.FullName + " token=" + first.MetadataToken.ToString("X8") + " identity=" + Text(first.Assembly.FullName));
        Record("FACTORY_INTERFACE type=" + second.FullName + " token=" + second.MetadataToken.ToString("X8") + " identity=" + Text(second.Assembly.FullName));
        var a = new ProjectDenyProxy(first, Record); var b = new ProjectDenyProxy(second, Record);
        ProbeFiles.Require(!unexpected && !rejected, "FACTORY_PRECONSTRUCTOR_DEPENDENCY");
        InvokeFactory(ctor, a, b); GC.KeepAlive(a); GC.KeepAlive(b);
    }
    static void OwnedFactory()
    {
        Assembly platform = Assembly.Load(Identities[6]);
        ProbeFiles.Require(PlatformFacadeAllowed(platform.FullName, platform.Location, platform.GlobalAssemblyCache, platform.ReflectionOnly), "OWNED_FACTORY_PLATFORM_FACADE");
        for (int i = 0; i < FactoryExtraNames.Length; i++) {
            string path = Path.Combine(stage, FactoryExtraNames[i] + ".dll");
            ProbeFiles.Require(FactoryReferencePolicy("original-factory", true, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-factory", false, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-factory", true, FactoryExtraIdentities[i], path, true)
                && !FactoryReferencePolicy("original-factory", true, "Owned.WrongIdentity", path, false)
                && !FactoryReferencePolicy("original-factory", true, FactoryExtraIdentities[i], fixture, false), "OWNED_FACTORY_REFERENCE_POLICY");
            foreach (string oldMode in new string[] { "original-project", "original-project-store-id", "owned-project", "owned-project-store-id", "owned-factory" })
                ProbeFiles.Require(!FactoryReferencePolicy(oldMode, true, FactoryExtraIdentities[i], path, false), "OWNED_FACTORY_LEGACY_REFERENCE_DENIAL");
        }
        Record("OWNED_FACTORY_REFERENCE_POLICY_PASS old-modes=denied incomplete-pin-wrong-identity-path-reflection=denied policy-only=true extra-runtime-load=NOT_RUN");
        Assembly assembly = Assembly.LoadFrom(fixture);
        Type first = assembly.GetType("Owned.Task009.IContainerDependencyFixture", true), second = assembly.GetType("Owned.Task009.IDataDependencyFixture", true);
        Type normal = assembly.GetType("Owned.Task009.FactoryReturnsFixture", true), hidden = assembly.GetType("Owned.Task009.FactoryPrivateConstructorFixture", true);
        int negatives = 0;
        foreach (Action action in new Action[] {
            delegate { FactoryCtor(typeof(string), first, second); },
            delegate { FactoryCtor(normal, typeof(string), second); },
            delegate { FactoryCtor(normal, first, first); },
            delegate { FactoryCtor(hidden, first, second); },
            delegate { OriginalFactoryTokens(normal, FactoryCtor(normal, first, second), first, second); }
        }) { bool denied = false; try { action(); } catch (IOException) { denied = true; } ProbeFiles.Require(denied, "OWNED_FACTORY_CONTRACT_NEGATIVE"); negatives++; }
        ProbeFiles.Require(!ReviewedFactoryTokens(0x02000024, 0x0600012e) && !ReviewedFactoryTokens(0x02000023, 0x0600012f), "OWNED_FACTORY_TOKEN_NEGATIVE");
        ProbeFiles.Require(Object.Equals(normal.GetMethod("Untouched").Invoke(null, new object[0]), true), "OWNED_FACTORY_NO_EARLY_EFFECTS");
        Record("OWNED_FACTORY_CONTRACT_PASS negatives=" + negatives + " token-policy-negatives=2 before-construction=true no-sdk-impersonation=true");
        var a = new ProjectDenyProxy(first, Record); var b = new ProjectDenyProxy(second, Record);
        assembly.GetType("Owned.Task009.FactoryAssertions", true).GetMethod("AssertAll").Invoke(null, new object[] { a.GetTransparentProxy(), b.GetTransparentProxy() });
        ProbeFiles.Require(a.Requests == 17 && b.Requests == 17, "OWNED_FACTORY_DENIAL_COUNTS");
        foreach (string name in new string[] { "FactoryReturnsFixture", "FactoryThrowsFixture", "FactoryCatchesFixture" }) {
            Type type = assembly.GetType("Owned.Task009." + name, true); var x = new ProjectDenyProxy(first, Record); var y = new ProjectDenyProxy(second, Record);
            bool failed = false;
            try { InvokeFactory(FactoryCtor(type, first, second), x, y); }
            catch (TargetInvocationException error) { failed = error.InnerException is NotSupportedException; }
            ProbeFiles.Require(failed == (name == "FactoryThrowsFixture"), "OWNED_FACTORY_RESULT");
            ProbeFiles.Require(Object.Equals(type.GetMethod("CalledOnce").Invoke(null, new object[0]), true), "OWNED_FACTORY_ONCE");
            ProbeFiles.Require(x.Requests + y.Requests == (name == "FactoryReturnsFixture" ? 0 : name == "FactoryThrowsFixture" ? 1 : 2), "OWNED_FACTORY_CONSTRUCTOR_DENIAL_COUNTS");
        }
        Record("OWNED_FACTORY_CONSTRUCTOR_PASS return=true denial=true caught-denial=true all-unqualified=true dependency-proxies-bounded=true original=NOT_RUN");
    }
    // Reflective shape checks precede every builder side effect. No registrations.
    sealed class EmptyContainerApi
    {
        internal Type Builder, Options, Container, Context;
        internal ConstructorInfo Constructor;
        internal MethodInfo Build;
    }
    static bool PublicInterface(Type type)
    { return type != null && type.IsPublic && type.IsInterface && !type.IsGenericType && !type.ContainsGenericParameters; }
    static EmptyContainerApi EmptyApi(Type builder, Type options, Type container, Type context)
    {
        ProbeFiles.Require(builder != null && builder.IsPublic && builder.IsClass && !builder.IsAbstract && !builder.IsGenericType && !builder.ContainsGenericParameters,
            "EMPTY_CONTAINER_BUILDER_TYPE");
        ProbeFiles.Require(PublicInterface(container) && PublicInterface(context) && context.IsAssignableFrom(container)
            && typeof(IDisposable).IsAssignableFrom(container), "EMPTY_CONTAINER_INTERFACES");
        ProbeFiles.Require(options != null && options.IsPublic && options.IsEnum && !options.IsGenericType && Enum.GetUnderlyingType(options) == typeof(int), "EMPTY_CONTAINER_OPTIONS_TYPE");
        ProbeFiles.Require(builder.Assembly == options.Assembly && builder.Assembly == container.Assembly && builder.Assembly == context.Assembly
            && !builder.Assembly.ReflectionOnly, "EMPTY_CONTAINER_API_OWNERSHIP");
        FieldInfo none = options.GetField("None", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(none != null && none.DeclaringType == options && none.FieldType == options && none.IsLiteral && !none.IsInitOnly
            && Object.Equals(none.GetRawConstantValue(), 0), "EMPTY_CONTAINER_NONE_LITERAL");
        ConstructorInfo ctor = builder.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        MethodInfo build = builder.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { options }, null);
        ProbeFiles.Require(ctor != null && ctor.DeclaringType == builder && ctor.IsPublic && !ctor.IsStatic && ctor.GetParameters().Length == 0
            && (ctor.CallingConvention & CallingConventions.VarArgs) == 0, "EMPTY_CONTAINER_BUILDER_CONSTRUCTOR");
        ProbeFiles.Require(build != null && build.DeclaringType == builder && build.IsPublic && !build.IsStatic && !build.IsGenericMethod && !build.ContainsGenericParameters
            && build.ReturnType == container && build.GetParameters().Length == 1 && build.GetParameters()[0].ParameterType == options
            && (build.CallingConvention & CallingConventions.VarArgs) == 0, "EMPTY_CONTAINER_BUILD_SIGNATURE");
        return new EmptyContainerApi { Builder = builder, Options = options, Container = container, Context = context, Constructor = ctor, Build = build };
    }
    static void EmptyContainerReturn(EmptyContainerApi api, object value)
    {
        ProbeFiles.Require(value != null && api.Container.IsInstanceOfType(value) && api.Context.IsInstanceOfType(value) && value is IDisposable
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value) && value.GetType().Assembly == api.Builder.Assembly,
            "EMPTY_CONTAINER_GENUINE_RETURN");
    }
    static void WithEmptyContainer(EmptyContainerApi api, Action<object> use)
    {
        object container = null, builder = null;
        try {
            bool created = false;
            Record("EMPTY_CONTAINER_BUILDER_BEGIN type=" + api.Builder.FullName + " token=" + api.Constructor.MetadataToken.ToString("X8") + " token-source=OBSERVED");
            try { builder = api.Constructor.Invoke(new object[0]); created = true; }
            finally { Record("EMPTY_CONTAINER_BUILDER_END returned=" + created); }
            ProbeFiles.Require(builder != null && builder.GetType() == api.Builder, "EMPTY_CONTAINER_EXACT_BUILDER");
            bool built = false;
            Record("EMPTY_CONTAINER_BUILD_BEGIN token=" + api.Build.MetadataToken.ToString("X8") + " options=None raw=0 application-registrations=0 defaults=RETAINED");
            try { container = api.Build.Invoke(builder, new object[] { Enum.ToObject(api.Options, 0) }); built = true; }
            finally { Record("EMPTY_CONTAINER_BUILD_END returned=" + built + " nonnull=" + (container != null)); }
            EmptyContainerReturn(api, container);
            ProbeFiles.Require(!unexpected && !rejected, "EMPTY_CONTAINER_BUILD_DEPENDENCY");
            Record("EMPTY_CONTAINER_READY genuine-context=true context-requests=NOT_INTERCEPTED application-registrations=0 defaults=RETAINED qualification=NOT_ESTABLISHED");
            use(container);
        } finally {
            IDisposable disposable = container as IDisposable;
            if (disposable != null) {
                bool disposed = false;
                // Even a bounded-log failure must not skip managed disposal.
                try { Record("EMPTY_CONTAINER_DISPOSE_BEGIN attempts=1 hard-termination-disposal=NOT_ESTABLISHED"); }
                finally {
                    try { disposable.Dispose(); disposed = true; }
                    finally { Record("EMPTY_CONTAINER_DISPOSE_END returned=" + disposed + " attempts=1"); }
                }
            }
            GC.KeepAlive(builder); GC.KeepAlive(container);
        }
    }
    static void InvokeFactoryWithContainer(ConstructorInfo ctor, object context, ProjectDenyProxy provider)
    {
        bool returned = false;
        Record("FACTORY_CONSTRUCTOR_BEGIN type=" + ctor.DeclaringType.FullName + " token=" + ctor.MetadataToken.ToString("X8")
            + " services=genuine-empty-container+deny-all-provider constructor-only=true context-requests=NOT_INTERCEPTED qualification=NOT_ESTABLISHED");
        try {
            ProbeFiles.Require(!unexpected && !rejected, "FACTORY_PRECONSTRUCTOR_DEPENDENCY");
            constructed = ctor.Invoke(new object[] { context, provider.GetTransparentProxy() }); returned = true;
            ProbeFiles.Require(constructed != null && constructed.GetType() == ctor.DeclaringType, "FACTORY_EXACT_RETURN_TYPE");
            if (provider.Requests != 0) throw new NotSupportedException("FACTORY_PROVIDER_DENIAL_RECORDED");
        } finally {
            Record("FACTORY_CONSTRUCTOR_END returned=" + returned + " provider-denied-requests=" + provider.Requests
                + " context-requests=NOT_INTERCEPTED qualification=NOT_ESTABLISHED project=NOT_RUN Id=NOT_RUN UndoManager=NOT_RUN DCA=NOT_RUN ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
            GC.KeepAlive(provider);
        }
    }
    // The same post-disposal audit is used by the original route and owned regression.
    static void WithFactoryContainer(EmptyContainerApi api, ConstructorInfo ctor, ProjectDenyProxy provider)
    {
        bool returned = false;
        try {
            WithEmptyContainer(api, delegate(object value) { InvokeFactoryWithContainer(ctor, value, provider); });
            returned = true;
        } finally {
            int denied = provider.Requests;
            Record("FACTORY_PROVIDER_LIFETIME_AUDIT after-managed-disposal=true scope-returned=" + returned + " denied-requests=" + denied
                + " hard-termination-audit=NOT_ESTABLISHED asynchronous-outside-lifetime=UNKNOWN qualification=NOT_ESTABLISHED");
            if (denied != 0) throw new NotSupportedException("FACTORY_PROVIDER_LIFETIME_DENIAL_RECORDED");
        }
    }
    static void OriginalEmptyContainer()
    {
        ProbeFiles.Require(EmptyContainer && FactoryOriginal && containerInputsReady, "EMPTY_CONTAINER_ORIGINAL_PROFILE");
        Record("FACTORY_PHASE original-empty-container-type-materialization");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll"));
        Assembly autofac = Assembly.LoadFrom(Path.Combine(stage, FactoryExtraNames[0] + ".dll"));
        Assembly kim = Assembly.LoadFrom(Path.Combine(stage, FactoryExtraNames[1] + ".dll"));
        ProbeFiles.Require(autofac.FullName == FactoryExtraIdentities[0] && !autofac.ReflectionOnly
            && ProbeFiles.Same(autofac.Location, Path.Combine(stage, FactoryExtraNames[0] + ".dll")), "EMPTY_CONTAINER_PINNED_ASSEMBLY");
        Type context = autofac.GetType("Autofac.IComponentContext", true, false), provider = kim.GetType("Knx.Kim.DataProvider.IKimDataProvider", true, false);
        Type factory = om.GetType("Knx.Ets.ObjectModel.Factory", true, false);
        ProbeFiles.Require(factory.Assembly == om && provider.Assembly == kim && context.Assembly == autofac, "FACTORY_TYPE_OWNERSHIP");
        ConstructorInfo ctor = FactoryCtor(factory, context, provider); OriginalFactoryTokens(factory, ctor, context, provider);
        EmptyContainerApi api = EmptyApi(autofac.GetType("Autofac.ContainerBuilder", true, false), autofac.GetType("Autofac.Builder.ContainerBuildOptions", true, false),
            autofac.GetType("Autofac.IContainer", true, false), context);
        ProbeFiles.Require(api.Builder.Assembly == autofac && !unexpected && !rejected, "EMPTY_CONTAINER_PREBUILDER_DEPENDENCY");
        Record("EMPTY_CONTAINER_API type-token=" + api.Builder.MetadataToken.ToString("X8") + " ctor-token=" + api.Constructor.MetadataToken.ToString("X8")
            + " build-token=" + api.Build.MetadataToken.ToString("X8") + " token-source=OBSERVED identity=" + Text(autofac.FullName)
            + " api-source-commit=07cac9047f4059de2d72834042c9649cbd228085");
        var denied = new ProjectDenyProxy(provider, Record);
        WithFactoryContainer(api, ctor, denied);
    }
    static void OwnedEmptyContainer()
    {
        Assembly assembly = Assembly.LoadFrom(fixture);
        Func<string, Type> type = delegate(string name) { return assembly.GetType("Owned.Task009." + name, true, false); };
        Type builder = type("EmptyBuilderFixture"), options = type("EmptyOptionsFixture"), container = type("IEmptyContainerFixture"), context = type("IContainerDependencyFixture"), control = type("EmptyControlFixture");
        Action<string> reset = delegate(string behavior) { control.GetMethod("Reset").Invoke(null, new object[] { behavior }); };
        Func<string, int> count = delegate(string name) { return (int)control.GetField(name).GetValue(null); };
        EmptyContainerApi api = EmptyApi(builder, options, container, context);
        foreach (string oldMode in new string[] { "original-project", "original-project-store-id", "original-factory", "original-factory-bcl9", "owned-factory-empty-container" })
            for (int i = 0; i < ContainerNames.Length; i++) ProbeFiles.Require(!ContainerReferencePolicy(oldMode, true, ContainerIdentities[i], Path.Combine(stage, ContainerNames[i] + ".dll"), false), "OWNED_CONTAINER_OLD_MODE_REFERENCE");
        for (int i = 0; i < ContainerNames.Length; i++) {
            string path = Path.Combine(stage, ContainerNames[i] + ".dll");
            ProbeFiles.Require(ContainerReferencePolicy("original-factory-empty-container", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-factory-empty-container", false, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-factory-empty-container", true, ContainerIdentities[i], path, true)
                && !ContainerReferencePolicy("original-factory-empty-container", true, "Owned.WrongIdentity", path, false)
                && !ContainerReferencePolicy("original-factory-empty-container", true, ContainerIdentities[i], Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ContainerNames[i] + ".dll"), false), "OWNED_CONTAINER_REFERENCE_POLICY");
        }
        Record("OWNED_CONTAINER_REFERENCE_POLICY_PASS old-modes-incomplete-identity-location-reflection=denied framework-fallback=false policy-only=true");
        reset("normal"); int negatives = 0;
        foreach (Action action in new Action[] {
            delegate { EmptyApi(type("EmptyPrivateBuilderFixture"), options, container, context); },
            delegate { EmptyApi(type("EmptyStaticBuilderFixture"), options, container, context); },
            delegate { EmptyApi(type("EmptyGenericBuilderFixture"), options, container, context); },
            delegate { EmptyApi(type("EmptyWrongReturnBuilderFixture"), options, container, context); },
            delegate { EmptyApi(builder, type("EmptyByteOptionsFixture"), container, context); },
            delegate { EmptyApi(builder, type("EmptyWrongOptionsFixture"), container, context); },
            delegate { EmptyApi(builder, options, context, context); },
            delegate { EmptyApi(builder, options, container, typeof(IDisposable)); },
            delegate { EmptyApi(builder, options, container, typeof(string)); },
            delegate { EmptyContainerReturn(api, null); },
            delegate { EmptyContainerReturn(api, new object()); },
            delegate { EmptyContainerReturn(api, new ProjectDenyProxy(container, Record).GetTransparentProxy()); }
        }) { bool failed = false; try { action(); } catch (IOException) { failed = true; } ProbeFiles.Require(failed, "OWNED_EMPTY_API_NEGATIVE"); negatives++; }
        ProbeFiles.Require(count("Constructors") == 0 && count("Builds") == 0 && count("Disposals") == 0, "OWNED_EMPTY_NO_EARLY_EFFECTS");
        Record("OWNED_EMPTY_API_PASS negatives=" + negatives + " before-builder=true unsigned-owned-only=true");
        foreach (string behavior in new string[] { "normal", "ctor-throw", "build-throw", "null", "dispose-throw" }) {
            reset(behavior); int used = 0; bool failed = false;
            try { WithEmptyContainer(api, delegate(object value) { used++; }); } catch (Exception error) {
                failed = error is IOException || (error is TargetInvocationException && error.InnerException is NotSupportedException) || error is NotSupportedException;
                if (!failed) throw;
            }
            ProbeFiles.Require(failed == (behavior != "normal") && count("Constructors") == 1
                && count("Builds") == (behavior == "ctor-throw" ? 0 : 1)
                && count("Disposals") == (behavior == "normal" || behavior == "dispose-throw" ? 1 : 0)
                && used == (behavior == "normal" || behavior == "dispose-throw" ? 1 : 0), "OWNED_EMPTY_LIFETIME_RESULT");
        }
        Type provider = type("IDataDependencyFixture");
        foreach (string name in new string[] { "EmptyFactoryReturnsFixture", "EmptyFactoryThrowsFixture", "EmptyFactoryCatchesFixture" }) {
            reset("normal"); var denied = new ProjectDenyProxy(provider, Record); bool failed = false;
            try { WithFactoryContainer(api, FactoryCtor(type(name), context, provider), denied); }
            catch (TargetInvocationException error) { failed = error.InnerException is NotSupportedException; if (!failed) throw; }
            catch (NotSupportedException) { failed = true; }
            ProbeFiles.Require(failed == (name != "EmptyFactoryReturnsFixture") && count("Constructors") == 1 && count("Builds") == 1 && count("Disposals") == 1
                && count("Factories") == 1 && denied.Requests == (name == "EmptyFactoryReturnsFixture" ? 0 : 1), "OWNED_EMPTY_FACTORY_RESULT");
        }
        // Reproduce the old unaudited lifetime using OWNED bytes only, then test the production wrapper.
        ConstructorInfo disposalCtor = FactoryCtor(type("EmptyFactoryDisposalProviderFixture"), context, provider);
        reset("dispose-denial-caught"); var baseline = new ProjectDenyProxy(provider, Record);
        WithEmptyContainer(api, delegate(object value) { InvokeFactoryWithContainer(disposalCtor, value, baseline); });
        ProbeFiles.Require(baseline.Requests == 1 && count("Disposals") == 1 && count("Factories") == 1, "OWNED_DISPOSAL_DENIAL_BASELINE");
        Record("OWNED_DISPOSAL_DENIAL_BASELINE reproduced=true unaudited-returned=True provider-denied-requests=1 disposals=1 original=NOT_RUN");
        reset("dispose-denial-caught"); var guarded = new ProjectDenyProxy(provider, Record); bool disposalDenied = false;
        try { WithFactoryContainer(api, disposalCtor, guarded); }
        catch (NotSupportedException error) { disposalDenied = error.Message == "FACTORY_PROVIDER_LIFETIME_DENIAL_RECORDED"; if (!disposalDenied) throw; }
        ProbeFiles.Require(disposalDenied && guarded.Requests == 1 && count("Disposals") == 1 && count("Factories") == 1
            && count("Constructors") == 1 && count("Builds") == 1, "OWNED_DISPOSAL_DENIAL_FIX");
        Record("OWNED_DISPOSAL_DENIAL_FIX_PASS failed=true provider-denied-requests=1 disposals=1 shared-original-wrapper=true original=NOT_RUN");
        Record("OWNED_EMPTY_CONTAINER_PASS builder-build-once=true null-wrong-return-denied=true disposal-once=true disposal-failure-fails=true provider-denial-caught-still-fails=true all-unqualified=true original=NOT_RUN");
    }
    [STAThread]
    internal static int Main(string[] args)
    {
        int result = 2;
        try {
            ProbeFiles.Require(args.Length == 6 && args[0] == "--mode" && args[2] == "--input" && args[4] == "--log", "PROJECT_ARGUMENTS");
            mode = args[1]; ProbeFiles.Require(EnumOnly || Original || OwnedProject || FactoryOnly, "PROJECT_MODE");
            log = new FileStream(ProbeFiles.Local(args[5], false, false), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            Record("PROJECT_START mode=" + mode + " original-constructor-ready=" + OriginalConstructorReady + " qualification=NOT_ESTABLISHED");
            // Source-frozen readiness precedes input pins and every original load.
            if (Original) ProbeFiles.Require(OriginalConstructorReady, "ENUM_LITERAL_NOT_REVIEWED");
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || !Environment.Is64BitProcess) { Record("WINDOWS_X64_REQUIRED"); return 3; }
            using (Process process = Process.GetCurrentProcess()) if (!Environment.UserInteractive || process.SessionId == 0) { Record("INTERACTIVE_SESSION_REQUIRED"); return 4; }
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) if (new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) { Record("PROJECT_NONADMIN_REQUIRED"); return 5; }
            VerifyBindingConfiguration();
            stage = ProbeFiles.Local(args[3], true, true);
            if (EnumOnly) {
                Record("PROJECT_PHASE enum-reflection-only-metadata");
                PinEnumInputs();
            } else if (Original) {
                CountInputs(12);
                for (int i = 0; i < Names.Length; i++) Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);
                PinPlatformFacade();
                for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "INPUT_IDENTITY");
            } else if (EmptyContainer && FactoryOriginal) {
                PinContainerInputs();
            } else if (FactoryOriginal) {
                PinFactoryInputs();
            } else {
                fixture = Path.Combine(stage, "Owned.Task148.ConstructorFixtures.dll"); Pin(fixture, ProbeFiles.OwnedHash(fixture));
                PinPlatformFacade();
                ProbeFiles.Require(AssemblyName.GetAssemblyName(fixture).FullName == "Owned.Task148.ConstructorFixtures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "OWNED_IDENTITY");
            }
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionResolve;
            AppDomain.CurrentDomain.AssemblyLoad += delegate(object sender, AssemblyLoadEventArgs e) { Audit(e.LoadedAssembly); };
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) Audit(assembly);
            ProbeFiles.Require(!unexpected, "UNEXPECTED_BASELINE_ASSEMBLY");
            if (EnumOnly) {
                Assembly assembly = Assembly.ReflectionOnlyLoadFrom(Path.Combine(stage, "Knx.Ets.Common.Types.dll"));
                ProbeFiles.Require(assembly.ReflectionOnly && assembly.FullName == CommonIdentity, "PROJECT_REFLECTION_IDENTITY");
                EnumFields(assembly.GetType(EnumName, true, false), Record);
                result = 0;
            } else {
                int thread = Thread.CurrentThread.ManagedThreadId;
                var app = new Application(); app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                app.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                    try {
                        ProbeFiles.Require(Thread.CurrentThread.ManagedThreadId == thread && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA, "STA_DISPATCHER_REQUIRED");
                        if (EmptyContainer) { if (FactoryOriginal) OriginalEmptyContainer(); else OwnedEmptyContainer(); }
                        else if (FactoryOnly) { if (FactoryOriginal) OriginalFactory(); else OwnedFactory(); }
                        else if (StoreIdFixture) { if (Original) OriginalProjectStoreId(); else OwnedProjectStoreId(); }
                        else if (Original) OriginalProject(); else Owned(); result = 0;
                    }
                    catch (Exception error) { Errors(error); result = 10; }
                    finally { app.Shutdown(result); }
                }));
                app.Run();
            }
            ProbeFiles.Require(!unexpected && !rejected, "PROJECT_DEPENDENCY_BLOCKER");
            Record("PROJECT_RESULT exit=" + result + " metadata-only=" + EnumOnly + " qualification=NOT_ESTABLISHED ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
            return result;
        } catch (Exception error) { if (log != null) { try { Errors(error); } catch (IOException) { } } Console.WriteLine("PROJECT_FAILURE " + error.GetType().FullName + " " + Text(error.Message)); return 2; }
        finally { GC.KeepAlive(constructed); /* Held input/log handles live until supervised process exit. */ }
    }
    static void Errors(Exception error)
    {
        for (int i = 0; error != null && i < 8; i++, error = error.InnerException)
            Record("PROJECT_EXCEPTION depth=" + i + " type=" + error.GetType().FullName + " hresult=" + error.HResult.ToString("X8") + " message=" + Text(error.Message));
        if (error != null) Record("PROJECT_EXCEPTION_CHAIN_TRUNCATED");
    }
}
