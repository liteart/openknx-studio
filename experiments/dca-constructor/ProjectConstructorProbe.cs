// Fixed Task-009 diagnostic. No positive SDK service or fixture qualification.
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
    static readonly string[] Names = { "MDT-DaliDcaPlus", "Knx.Ets.Sdk", "Knx.Ets.Sdk.AddIns.AddInViews", "Knx.Ets.Common.Types", "Knx.Ets.Common.Resources", "Microsoft.Bcl.AsyncInterfaces", "netstandard", "System.Threading.Tasks.Extensions", "Knx.Ets.ObjectModel", "FluentResults", "Knx.Ets.Common" };
    static readonly string[] Hashes = { "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d", "7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989", "5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d", CommonHash, "6e07df9e247b1d5e6a11c9e8131ec8cc8ab8aabeca9af02b1911608d1eefe661", "1c95b1a776931ca1c4f2911b2ae1cc99eb78f547cd5feeaf5cdbdf10cedad0db", "bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb", "692c324c02a922f890aaf5948c54ec9758f5ab7019b8e8ab169c106ddd3231f4", "73ffaaa97640a4207819c182101d23d3da73d4c983056f5bca5881c0f02eda8a", "99adb97bc209d19696c9a42db8eceef49b55fc2c3f4cc6d6f5654fe7e5df9159", "4d4fcd3b0aed672979aa45b4642a5af772e6dcf0b0388062a96d8ee992f86ecd" };
    static readonly string[] Identities = {
        "MDT-DaliDcaPlus, Version=4.0.0.0, Culture=neutral, PublicKeyToken=1a3211b0275a5581",
        "Knx.Ets.Sdk, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", CommonIdentity,
        "Knx.Ets.Common.Resources, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Microsoft.Bcl.AsyncInterfaces, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "System.Threading.Tasks.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", OmIdentity,
        "FluentResults, Version=3.16.0.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Common, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159" };
    static readonly List<FileStream> Inputs = new List<FileStream>();
    static readonly object LogLock = new object();
    static FileStream log;
    static string stage, mode, fixture;
    static volatile bool unexpected, rejected;
    static volatile bool enumInputsPinned;
    static volatile bool platformFacadePinned;
    static string platformFacadePath;
    static int logBytes, assemblyCount;
    static object constructed;
    static bool EnumOnly { get { return mode == "project-enum-metadata"; } }
    static bool Original { get { return mode == "original-project"; } }
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
        ProbeFiles.Require(Original || mode == "owned-project", "PLATFORM_FACADE_MODE");
        platformFacadePath = ProbeFiles.Local(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), PlatformFacadeRelativePath), true, false);
        if (mode == "owned-project") {
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
        return (Original || mode == "owned-project") && platformFacadePinned && identity == Identities[6]
            && ProbeFiles.Same(location, platformFacadePath) && cache && !reflectionOnly;
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
            if ((Original || mode == "owned-project") && assembly.GetName().Name == "netstandard")
                Record("PLATFORM_FACADE_AUDIT alias-allowed=" + platform + " identity=" + Text(assembly.FullName) + " origin=" + (platform ? "FIXED_OS_GAC_PATH" : "UNQUALIFIED_FOR_ALIAS")
                    + " location=" + Text(assembly.Location) + " gac=" + assembly.GlobalAssemblyCache
                    + " reflection-only=" + assembly.ReflectionOnly
                    + " pinned-file-sha256=" + (platform ? Hashes[6] : "NOT_ESTABLISHED"));
            if (EnumOnly && enumInputsPinned && assembly.ReflectionOnly
                && ((assembly.FullName == CommonIdentity && ProbeFiles.Same(assembly.Location, Path.Combine(stage, "Knx.Ets.Common.Types.dll")))
                    || (assembly.FullName == Identities[6] && ProbeFiles.Same(assembly.Location, Path.Combine(stage, "netstandard.dll"))))) allowed = true;
            if (Original) for (int i = 0; i < Names.Length; i++)
                if (assembly.FullName == Identities[i] && ProbeFiles.Same(assembly.Location, Path.Combine(stage, Names[i] + ".dll"))) allowed = true;
            if (!Original && !EnumOnly && assembly.FullName == "Owned.Task148.ConstructorFixtures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" && ProbeFiles.Same(assembly.Location, fixture)) allowed = true;
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
        rejected = true;
        throw new FileNotFoundException("PROJECT_DEPENDENCY_NOT_ALLOWLISTED", args.Name);
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
    [STAThread]
    internal static int Main(string[] args)
    {
        int result = 2;
        try {
            ProbeFiles.Require(args.Length == 6 && args[0] == "--mode" && args[2] == "--input" && args[4] == "--log", "PROJECT_ARGUMENTS");
            mode = args[1]; ProbeFiles.Require(EnumOnly || Original || mode == "owned-project", "PROJECT_MODE");
            log = new FileStream(ProbeFiles.Local(args[5], false, false), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            Record("PROJECT_START mode=" + mode + " original-constructor-ready=" + OriginalConstructorReady + " qualification=NOT_ESTABLISHED");
            // Source-frozen readiness precedes input pins and every original load.
            if (Original) ProbeFiles.Require(OriginalConstructorReady, "ENUM_LITERAL_NOT_REVIEWED");
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || !Environment.Is64BitProcess) { Record("WINDOWS_X64_REQUIRED"); return 3; }
            using (Process process = Process.GetCurrentProcess()) if (!Environment.UserInteractive || process.SessionId == 0) { Record("INTERACTIVE_SESSION_REQUIRED"); return 4; }
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) if (new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) { Record("PROJECT_NONADMIN_REQUIRED"); return 5; }
            stage = ProbeFiles.Local(args[3], true, true);
            string config = ProbeFiles.Local(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true, false);
            ProbeFiles.Require(new FileInfo(config).Length <= 4096 && File.ReadAllText(config) == ProbeFiles.ConfigText, "EXACT_CONFIG_REQUIRED");
            if (EnumOnly) {
                Record("PROJECT_PHASE enum-reflection-only-metadata");
                PinEnumInputs();
            } else if (Original) {
                CountInputs(11);
                for (int i = 0; i < Names.Length; i++) Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);
                PinPlatformFacade();
                for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "INPUT_IDENTITY");
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
                    try { ProbeFiles.Require(Thread.CurrentThread.ManagedThreadId == thread && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA, "STA_DISPATCHER_REQUIRED"); if (Original) OriginalProject(); else Owned(); result = 0; }
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
