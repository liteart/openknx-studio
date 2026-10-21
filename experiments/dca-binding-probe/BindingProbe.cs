// Original, bounded .NET Framework research tool. Never execution-load vendor code.
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

internal static class BindingProbe
{
    static string stage;
    static int inputCount;
    static readonly string[] names = { "MDT-DaliDcaPlus", "Knx.Ets.Sdk", "Knx.Ets.Sdk.AddIns.AddInViews", "Knx.Ets.Common.Types", "Knx.Ets.Common.Resources", "Microsoft.Bcl.AsyncInterfaces", "netstandard", "System.Threading.Tasks.Extensions" };
    static readonly string[] hashes = { "4507e7b0e1eb10fea9556f19dd8d34c5f9fcf3cd8efa79e891b13f2585d7cf8d", "7d28a8d549dd25b08fe798b8e0ee072eb5d5916713808f7ceffa1c35eaf7d989", "5a59b733cbbecb8afe67f54e3b354045a61e092470a13129971f4f99e0b6d87d", "5c4e08541a123825b0ed501aec1f5fbbbc3d1f756b3be41ff2728925f906c9f0", "6e07df9e247b1d5e6a11c9e8131ec8cc8ab8aabeca9af02b1911608d1eefe661", "1c95b1a776931ca1c4f2911b2ae1cc99eb78f547cd5feeaf5cdbdf10cedad0db", "bbc58e1e5b1976b8093116007dbc428b09495c7c810a1350d4a379b87716c4bb", "692c324c02a922f890aaf5948c54ec9758f5ab7019b8e8ab169c106ddd3231f4" };

    static void Require(bool ok, string code) { if (!ok) throw new IOException(code); }
    static int At(byte[] b, int p, int count) { Require(p >= 0 && count >= 0 && p <= b.Length - count, "PE_BOUNDS"); return p; }
    static uint U32(byte[] b, int p) { return BitConverter.ToUInt32(b, At(b, p, 4)); }
    static ushort U16(byte[] b, int p) { return BitConverter.ToUInt16(b, At(b, p, 2)); }
    static int Rva(byte[] b, uint rva, int sections, int count)
    {
        for (int n = 0; n < count; n++) {
            int s = At(b, sections + n * 40, 40); uint start = U32(b, s + 12), size = U32(b, s + 16);
            if (rva >= start && rva - start < size) return checked((int)(U32(b, s + 20) + rva - start));
        }
        throw new IOException("PE_RVA");
    }
    static void Preflight(string path, string hash)
    {
        FileInfo f = new FileInfo(path);
        Require(f.Exists && f.Length > 0 && f.Length <= 16 * 1024 * 1024 && (f.Attributes & FileAttributes.ReparsePoint) == 0, "INPUT_FILE");
        byte[] b = File.ReadAllBytes(path);
        using (SHA256 sha = SHA256.Create()) Require(BitConverter.ToString(sha.ComputeHash(b)).Replace("-", "").ToLowerInvariant() == hash, "INPUT_HASH");
        Require(U16(b, 0) == 0x5a4d, "PE_MZ"); int pe = checked((int)U32(b, 0x3c));
        Require(U32(b, pe) == 0x4550, "PE_SIGNATURE"); int count = U16(b, pe + 6), opt = pe + 24;
        Require(count > 0 && count <= 96, "PE_SECTIONS"); ushort magic = U16(b, opt);
        Require(magic == 0x10b || magic == 0x20b, "PE_MAGIC");
        int directory = opt + (magic == 0x10b ? 96 : 112);
        int clr = Rva(b, U32(b, directory + 14 * 8), opt + U16(b, pe + 20), count);
        At(b, clr, 72); uint flags = U32(b, clr + 16);
        Require((flags & 1) != 0 && (flags & 0x10) == 0 && U32(b, clr + 64) == 0 && U32(b, clr + 68) == 0, "IL_ONLY_REQUIRED");
        Console.WriteLine("PIN " + f.Name + " bytes=" + b.Length + " sha256=" + hash + " corflags=" + flags);
    }
    static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        AssemblyName requested = new AssemblyName(args.Name);
        string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
        AssemblyName selected = new AssemblyName(policy);
        Console.WriteLine("RESOLVE requested=" + args.Name + " policy=" + policy);
        for (int i = 1; i < inputCount; i++) if (selected.Name == names[i]) {
            string path = Path.Combine(stage, names[i] + ".dll");
            if (AssemblyName.GetAssemblyName(path).FullName != selected.FullName) {
                Console.WriteLine("POLICY_REJECT POLICY_IDENTITY_MISMATCH requested=" + args.Name);
                throw new IOException("POLICY_IDENTITY_MISMATCH");
            }
            return Assembly.ReflectionOnlyLoadFrom(path);
        }
        // No arbitrary disk/GAC probing: only already trusted runtime core identity.
        if (selected.FullName == typeof(object).Assembly.FullName) return Assembly.ReflectionOnlyLoad(typeof(object).Assembly.FullName);
        Console.WriteLine("DEPENDENCY_REJECT " + requested.FullName);
        throw new FileNotFoundException("DEPENDENCY_NOT_ALLOWLISTED: " + requested.FullName);
    }
    static int Main(string[] args)
    {
        try {
            Require(args.Length == 2, "ARGUMENTS");
            if (args[0] == "owned") {
                AssemblyName request = new AssemblyName(args[1]);
                Require(request.Name == "Owned.BindingContract" && request.Version == new Version("5.7.0.0") && String.IsNullOrEmpty(request.CultureName) && request.GetPublicKeyToken().Length == 8, "OWNED_IDENTITY_REQUIRED");
                Assembly a = Assembly.Load(args[1]);
                Console.WriteLine("OWNED_RESULT " + a.GetType("OwnedContract").GetMethod("Run").Invoke(null, null));
                return 0;
            }
            Require(args[0] == "reflection" || args[0] == "reflection-extended", "MODE"); stage = Path.GetFullPath(args[1]);
            inputCount = args[0] == "reflection-extended" ? 8 : 5;
            for (int i = 0; i < inputCount; i++) Preflight(Path.Combine(stage, names[i] + ".dll"), hashes[i]);
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += Resolve;
            Assembly dca = Assembly.ReflectionOnlyLoadFrom(Path.Combine(stage, names[0] + ".dll"));
            Require(dca.ReflectionOnly, "REFLECTION_ONLY_REQUIRED");
            foreach (int token in new[] { 0x0a0001f6, 0x0a0001f7 }) {
                MethodBase method = dca.ManifestModule.ResolveMethod(token);
                Require(method.DeclaringType.Assembly.ReflectionOnly, "METHOD_REFLECTION_ONLY");
                Require(method.Name == (token == 0x0a0001f6 ? "ReadProperty" : "WriteProperty") && method.MetadataToken == (token == 0x0a0001f6 ? 0x06001c5d : 0x06001c60), "RESOLVED_MEMBER_MISMATCH");
                Console.WriteLine("MEMBER token=" + token.ToString("X8") + " name=" + method.Name + " declaring=" + method.DeclaringType.AssemblyQualifiedName + " resolved-token=" + method.MetadataToken.ToString("X8"));
                foreach (ParameterInfo p in method.GetParameters()) {
                    Require(p.ParameterType.Assembly.ReflectionOnly || p.ParameterType.Assembly == typeof(object).Assembly, "PARAM_REFLECTION_ONLY_OR_TRUSTED_CORE");
                    Console.WriteLine("PARAM " + p.Position + " " + p.ParameterType.AssemblyQualifiedName);
                }
            }
            Assembly common = Assembly.ReflectionOnlyLoadFrom(Path.Combine(stage, "Knx.Ets.Common.Types.dll"));
            Type prop = common.GetType("Knx.Ets.Common.Types.Enumerations.PropType", true);
            Console.WriteLine("PROPTYPE " + prop.AssemblyQualifiedName + " enum=" + prop.IsEnum + " underlying=" + Enum.GetUnderlyingType(prop).FullName);
            FieldInfo[] fields = prop.GetFields(BindingFlags.Public | BindingFlags.Static);
            Require(fields.Length <= 256, "ENUM_BOUND");
            foreach (FieldInfo f in fields) Console.WriteLine("ENUM " + f.Name + "=" + f.GetRawConstantValue());
            Require(AppDomain.CurrentDomain.GetAssemblies().Length < 64, "ASSEMBLY_BOUND");
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) Require(Array.IndexOf(names, a.GetName().Name) < 0, "VENDOR_EXECUTION_CONTEXT");
            foreach (Assembly a in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()) {
                int index = Array.IndexOf(names, a.GetName().Name);
                if (index >= 0) Require(String.Equals(a.Location, Path.Combine(stage, names[index] + ".dll"), StringComparison.OrdinalIgnoreCase), "REFLECTION_INPUT_PATH");
                else Require(a.FullName == typeof(object).Assembly.FullName, "REFLECTION_ASSEMBLY_ALLOWLIST");
                Console.WriteLine("REFLECTION_ASSEMBLY " + a.FullName + " location=" + a.Location);
            }
            Console.WriteLine("REFLECTION_RESULT PASS vendor-code=NOT_RUN binding-proof=EXPLICIT_POLICY_METADATA_ONLY");
            return 0;
        } catch (Exception e) { Console.WriteLine("PROBE_ERROR " + e.GetType().FullName + " " + e.Message); return 2; }
    }
}
