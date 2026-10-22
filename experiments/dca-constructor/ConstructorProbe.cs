// Owned constructor-only research tool. No SDK implementation, UI API or bus service.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

internal static class ProbeFiles
{
    internal const string ConfigText = "<configuration><startup><supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.8\"/></startup><runtime><assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">"
        + "<dependentAssembly><assemblyIdentity name=\"Knx.Ets.Sdk\" publicKeyToken=\"61439873ec5e1159\" culture=\"neutral\"/><bindingRedirect oldVersion=\"5.7.554.37315\" newVersion=\"6.4.8718.0\"/></dependentAssembly>"
        + "<dependentAssembly><assemblyIdentity name=\"Knx.Ets.Sdk.AddIns.AddInViews\" publicKeyToken=\"61439873ec5e1159\" culture=\"neutral\"/><bindingRedirect oldVersion=\"5.7.554.37315\" newVersion=\"6.4.8718.0\"/></dependentAssembly>"
        + "<dependentAssembly><assemblyIdentity name=\"Knx.Ets.Common.Types\" publicKeyToken=\"61439873ec5e1159\" culture=\"neutral\"/><bindingRedirect oldVersion=\"5.7.286.36952\" newVersion=\"6.4.8718.0\"/></dependentAssembly>"
        + "<dependentAssembly><assemblyIdentity name=\"System.Threading.Tasks.Extensions\" publicKeyToken=\"cc7b13ffcd2ddd51\" culture=\"neutral\"/><bindingRedirect oldVersion=\"4.2.0.1\" newVersion=\"4.2.1.0\"/></dependentAssembly>"
        + "</assemblyBinding></runtime></configuration>";

    internal static void Require(bool value, string code) { if (!value) throw new IOException(code); }
    internal static bool Same(string left, string right) { return String.Equals(left, right, StringComparison.OrdinalIgnoreCase); }
    internal static string Local(string path, bool exists, bool directory)
    {
        Require(path != null && path.Length >= 3 && path.Length <= 1024 && Char.IsLetter(path[0]) && path[1] == ':' && path[2] == '\\', "LOCAL_PATH_REQUIRED");
        Require(path.IndexOfAny(new char[] { '"', '\r', '\n', '*', '?', '<', '>', '|', '/' }) < 0 && path.IndexOf(':', 2) < 0, "LOCAL_PATH_TEXT");
        string[] raw = path.Substring(3).Split('\\');
        foreach (string part in raw) Require(part.Length > 0 && part != "." && part != ".." && !part.EndsWith(".", StringComparison.Ordinal) && !part.EndsWith(" ", StringComparison.Ordinal), "LOCAL_PATH_COMPONENT");
        string full = Path.GetFullPath(path), root = Path.GetPathRoot(full), current = root;
        DriveType drive = new DriveInfo(root).DriveType;
        Require(drive == DriveType.Fixed || drive == DriveType.Removable || drive == DriveType.Ram, "LOCAL_DRIVE_REQUIRED");
        Require((File.GetAttributes(root) & FileAttributes.ReparsePoint) == 0, "REPARSE_PATH");
        for (int i = 0; i < raw.Length; i++) {
            current = Path.Combine(current, raw[i]);
            // Query the component itself before any existence test or descent. Exists
            // can follow a reparse target before this code has had a chance to reject it.
            FileAttributes attributes;
            try { attributes = File.GetAttributes(current); }
            catch (FileNotFoundException) { Require(!exists && i == raw.Length - 1, "MISSING_PATH"); return full; }
            catch (DirectoryNotFoundException) { Require(!exists && i == raw.Length - 1, "MISSING_PATH"); return full; }
            Require((attributes & FileAttributes.ReparsePoint) == 0, "REPARSE_PATH");
            if (i < raw.Length - 1 || directory) Require((attributes & FileAttributes.Directory) != 0, "DIRECTORY_REQUIRED");
            else Require((attributes & FileAttributes.Directory) == 0, "FILE_REQUIRED");
        }
        Require(exists, "NEW_PATH_REQUIRED"); return full;
    }
    internal static string Hash(Stream stream)
    {
        using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }
    internal static string HashFile(string path)
    {
        using (FileStream stream = new FileStream(Local(path, true, false), FileMode.Open, FileAccess.Read, FileShare.Read)) return Hash(stream);
    }
    internal static string OwnedHash(string path)
    {
        string manifest = Local(path + ".sha256", true, false);
        Require(new FileInfo(manifest).Length <= 66, "OWNED_HASH_BOUND");
        string hash = File.ReadAllText(manifest).Trim();
        Require(hash.Length == 64, "OWNED_HASH_TEXT");
        foreach (char c in hash) Require((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'), "OWNED_HASH_TEXT");
        return hash;
    }
    internal static string Quote(string value)
    {
        Require(value.IndexOfAny(new char[] { '"', '\r', '\n' }) < 0 && !value.EndsWith("\\", StringComparison.Ordinal), "ARGUMENT_TEXT");
        return "\"" + value + "\"";
    }
}

internal static class ConstructorProbe
{
    static readonly string[] Names = { "MDT-DaliDcaPlus", "Knx.Ets.Sdk", "Knx.Ets.Sdk.AddIns.AddInViews", "Knx.Ets.Common.Types", "Knx.Ets.Common.Resources", "Microsoft.Bcl.AsyncInterfaces", "netstandard", "System.Threading.Tasks.Extensions" };
    static readonly string[] Hashes = { "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d", "7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989", "5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d", "5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0", "6e07df9e247b1d5e6a11c9e8131ec8cc8ab8aabeca9af02b1911608d1eefe661", "1c95b1a776931ca1c4f2911b2ae1cc99eb78f547cd5feeaf5cdbdf10cedad0db", "bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb", "692c324c02a922f890aaf5948c54ec9758f5ab7019b8e8ab169c106ddd3231f4" };
    static readonly string[] Identities = {
        "MDT-DaliDcaPlus, Version=4.0.0.0, Culture=neutral, PublicKeyToken=1a3211b0275a5581",
        "Knx.Ets.Sdk, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Common.Types, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Knx.Ets.Common.Resources, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159",
        "Microsoft.Bcl.AsyncInterfaces, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
        "System.Threading.Tasks.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51" };
    static readonly List<FileStream> Inputs = new List<FileStream>();
    static readonly object LogLock = new object();
    static FileStream log;
    static string stage, mode, fixture;
    static volatile bool unexpected;
    static volatile bool dependencyRejected;
    static int assemblyCount, logBytes, thread;
    static object constructed;

    static string Text(string value)
    {
        if (value == null) return "null";
        value = value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\0", "\\0");
        return value.Length <= 512 ? value : value.Substring(0, 512) + "[truncated]";
    }
    static void Record(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O") + " " + value + "\n");
        lock (LogLock) {
            ProbeFiles.Require(logBytes <= 65536 - bytes.Length, "PROBE_LOG_LIMIT");
            log.Write(bytes, 0, bytes.Length); log.Flush(); logBytes += bytes.Length;
        }
    }
    static void Exceptions(Exception error)
    {
        for (int i = 0; error != null && i < 8; i++, error = error.InnerException)
            Record("PROBE_EXCEPTION depth=" + i + " type=" + error.GetType().FullName + " hresult=" + error.HResult.ToString("X8") + " message=" + Text(error.Message));
        if (error != null) Record("PROBE_EXCEPTION_CHAIN_TRUNCATED");
    }
    static void Pin(string path, string hash)
    {
        path = ProbeFiles.Local(path, true, false);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Inputs.Add(stream); // Retain read handles to deny replacement/writes through the experiment.
        ProbeFiles.Require(stream.Length > 0 && stream.Length <= 16 * 1024 * 1024, "INPUT_SIZE");
        ProbeFiles.Require(ProbeFiles.Hash(stream) == hash, "INPUT_HASH");
        Record("PIN " + Path.GetFileName(path) + " bytes=" + stream.Length + " sha256=" + hash);
    }
    static bool Framework(Assembly assembly)
    {
        AssemblyName name = assembly.GetName();
        string token = BitConverter.ToString(name.GetPublicKeyToken()).Replace("-", "").ToLowerInvariant();
        bool nameAllowed = name.Name == "mscorlib" || name.Name == "System" || name.Name.StartsWith("System.", StringComparison.Ordinal)
            || name.Name == "WindowsBase" || name.Name.StartsWith("Presentation", StringComparison.Ordinal)
            || name.Name.StartsWith("UIAutomation", StringComparison.Ordinal) || name.Name == "ReachFramework"
            || name.Name == "Accessibility" || name.Name == "Microsoft.CSharp";
        if (!nameAllowed || (token != "b77a5c561934e089" && token != "b03f5f7f11d50a3a" && token != "31bf3856ad364e35")) return false;
        string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows).TrimEnd('\\') + "\\";
        string location = assembly.Location;
        return location.StartsWith(windows + "Microsoft.NET\\", StringComparison.OrdinalIgnoreCase)
            || location.StartsWith(windows + "assembly\\", StringComparison.OrdinalIgnoreCase);
    }
    static void Audit(Assembly assembly)
    {
        if (Interlocked.Increment(ref assemblyCount) > 96) { unexpected = true; throw new IOException("ASSEMBLY_COUNT_BOUND"); }
        string location = assembly.IsDynamic ? "dynamic" : assembly.Location;
        bool allowed = assembly == typeof(ConstructorProbe).Assembly;
        if (!assembly.IsDynamic && mode == "original") {
            for (int i = 0; i < Names.Length; i++)
                if (assembly.FullName == Identities[i] && ProbeFiles.Same(location, Path.Combine(stage, Names[i] + ".dll"))) allowed = true;
        } else if (!assembly.IsDynamic && assembly.FullName == "Owned.Task148.ConstructorFixtures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" && ProbeFiles.Same(location, fixture)) allowed = true;
        if (!assembly.IsDynamic && Framework(assembly)) allowed = true;
        Record("ASSEMBLY allowed=" + allowed + " identity=" + Text(assembly.FullName) + " location=" + Text(location));
        if (!allowed) { unexpected = true; Record("UNEXPECTED_ASSEMBLY_BLOCKER detection=after-load prevention=NOT_ESTABLISHED"); }
    }
    static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
        Record("RESOLVE requested=" + Text(args.Name) + " policy=" + Text(policy));
        if (mode == "original") {
            for (int i = 1; i < Identities.Length; i++) if (policy == Identities[i]) return Assembly.LoadFrom(Path.Combine(stage, Names[i] + ".dll"));
        }
        dependencyRejected = true;
        Record("DEPENDENCY_REJECT " + Text(args.Name));
        throw new FileNotFoundException("DEPENDENCY_NOT_ALLOWLISTED", args.Name);
    }
    static void Construct()
    {
        ProbeFiles.Require(Thread.CurrentThread.ManagedThreadId == thread && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA, "STA_DISPATCHER_REQUIRED");
        Record("CONSTRUCTOR_DISPATCHER_STA_PASS mode=" + mode);
        Assembly assembly = Assembly.LoadFrom(mode == "original" ? Path.Combine(stage, Names[0] + ".dll") : fixture);
        ProbeFiles.Require(!unexpected, "UNEXPECTED_ASSEMBLY_BEFORE_CONSTRUCTOR");
        string typeName = mode == "original" ? "Mdt.Dali.DcaApp.DcaAppMain" : "Owned.Task148." + mode.Substring("owned-".Length);
        Type type = assembly.GetType(typeName, true, false);
        ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        ProbeFiles.Require(ctor != null, "PUBLIC_PARAMETERLESS_CONSTRUCTOR_REQUIRED");
        if (mode == "original") ProbeFiles.Require(ctor.MetadataToken == 0x0600000e && type.MetadataToken == 0x02000004, "ORIGINAL_ENTRYPOINT_TOKEN");
        ProbeFiles.Require(!unexpected, "UNEXPECTED_ASSEMBLY_BEFORE_CONSTRUCTOR");
        Record("CONSTRUCTOR_BEGIN type=" + typeName + " token=" + ctor.MetadataToken.ToString("X8"));
        constructed = ctor.Invoke(new object[0]);
        ProbeFiles.Require(!unexpected, "UNEXPECTED_ASSEMBLY_DURING_CONSTRUCTOR");
        ProbeFiles.Require(!dependencyRejected, "DEPENDENCY_REJECTED_DURING_CONSTRUCTOR");
        Record((mode == "original" ? "ORIGINAL_CONSTRUCTOR_RETURN" : "OWNED_CONSTRUCTOR_RETURN") + " type=" + typeName + " ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
    }
    [STAThread]
    internal static int Main(string[] args)
    {
        int result = 2;
        try {
            ProbeFiles.Require(args.Length == 6 && args[0] == "--mode" && args[2] == "--input" && args[4] == "--log", "ARGUMENTS");
            mode = args[1];
            ProbeFiles.Require(mode == "original" || mode == "owned-Success" || mode == "owned-Throw" || mode == "owned-Missing" || mode == "owned-Hang" || mode == "owned-Flood", "MODE");
            string logPath = ProbeFiles.Local(args[5], false, false);
            log = new FileStream(logPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            int session; using (Process process = Process.GetCurrentProcess()) session = process.SessionId;
            bool administrator;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) administrator = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            Record("PROBE_START mode=" + mode + " process=" + (Environment.Is64BitProcess ? "x64" : "x86") + " apartment=" + Thread.CurrentThread.GetApartmentState() + " session=" + session + " interactive=" + Environment.UserInteractive + " administrator-role=" + administrator);
            if (Environment.OSVersion.Platform != PlatformID.Win32NT || !Environment.Is64BitProcess) { Record("WINDOWS_X64_REQUIRED"); return 3; }
            if (!Environment.UserInteractive || session == 0) { Record("INTERACTIVE_SESSION_REQUIRED"); return 4; }
            stage = ProbeFiles.Local(args[3], true, true);
            string config = ProbeFiles.Local(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, true, false);
            ProbeFiles.Require(new FileInfo(config).Length <= 4096 && File.ReadAllText(config) == ProbeFiles.ConfigText, "EXACT_CONFIG_REQUIRED");
            if (mode == "original") {
                int count = 0;
                foreach (string entry in Directory.EnumerateFileSystemEntries(stage)) { count++; ProbeFiles.Require(count <= Names.Length, "EXACT_EIGHT_INPUTS_REQUIRED"); }
                ProbeFiles.Require(count == Names.Length, "EXACT_EIGHT_INPUTS_REQUIRED");
                for (int i = 0; i < Names.Length; i++) Pin(Path.Combine(stage, Names[i] + ".dll"), Hashes[i]);
                // GetAssemblyName reads identity metadata; every original hash is already pinned.
                for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "INPUT_IDENTITY");
            } else {
                fixture = Path.Combine(stage, "Owned.Task148.ConstructorFixtures.dll");
                Pin(fixture, ProbeFiles.OwnedHash(fixture));
                ProbeFiles.Require(AssemblyName.GetAssemblyName(fixture).FullName == "Owned.Task148.ConstructorFixtures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "OWNED_IDENTITY");
            }
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            AppDomain.CurrentDomain.AssemblyLoad += delegate(object sender, AssemblyLoadEventArgs e) { Audit(e.LoadedAssembly); };
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) Audit(assembly);
            ProbeFiles.Require(!unexpected, "UNEXPECTED_BASELINE_ASSEMBLY");
            thread = Thread.CurrentThread.ManagedThreadId;
            var app = new Application(); app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate {
                try { Construct(); result = 0; }
                catch (Exception error) { Exceptions(error); result = 10; }
                finally { app.Shutdown(result); }
            }));
            app.Run();
            GC.KeepAlive(constructed);
            ProbeFiles.Require(!unexpected, "UNEXPECTED_ASSEMBLY_AFTER_CONSTRUCTOR");
            Record("PROBE_RESULT mode=" + mode + " exit=" + result + " constructor-only=true ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
            return result;
        } catch (Exception error) {
            if (log != null) { try { Exceptions(error); } catch (IOException) { } }
            Console.WriteLine("PROBE_FAILURE " + error.GetType().FullName + " " + Text(error.Message)); return 2;
        } finally {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
            // Keep pinned input handles/log alive until supervised process teardown. An event
            // callback or vendor thread can remain active: process exit is supervised.
            // Do not call vendor Dispose or other lifecycle methods in this increment.
        }
    }
}
