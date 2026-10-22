// Owned deny-all diagnostic. No SDK service, configuration value or bus operation.
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;

internal sealed class InitializeContextProbe : RealProxy
{
    internal const string UnsupportedCode = "DCA_UNSUPPORTED_SERVICE";
    const string ContextName = "Knx.Ets.Sdk.AddIns.AddInViews.IInitializationContext";
    const string AddInIdentity = "Knx.Ets.Sdk.AddIns.AddInViews, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string SdkIdentity = "Knx.Ets.Sdk, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string ResourcesIdentity = "Knx.Ets.Common.Resources, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string XmlIdentity = "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
    static readonly string[] Getters = { "get_Project", "get_Resources", "get_Configuration", "get_UserConfiguration", "get_HostNotification", "get_DialogService" };
    static readonly string[] Returns = { "Knx.Ets.Sdk.Project.Project", "Knx.Ets.Common.Resources.IEtsResources", "System.Xml.XmlDocument", "System.Xml.XmlDocument", "Knx.Ets.Sdk.AddIns.AddInViews.IHostNotification", "Knx.Ets.Sdk.AddIns.AddInViews.IDialogService" };
    readonly Action<string> record;
    readonly MethodInfo initialize;
    readonly object requestLock = new object();
    int requests;

    InitializeContextProbe(Type contextType, MethodInfo method, Action<string> recorder) : base(contextType)
    {
        ProbeFiles.Require(contextType.IsInterface && contextType.IsPublic, "INITIALIZE_PUBLIC_INTERFACE_REQUIRED");
        initialize = method; record = recorder;
    }
    internal int Requests { get { return Volatile.Read(ref requests); } }

    public override IMessage Invoke(IMessage message)
    {
        IMethodCallMessage call = message as IMethodCallMessage;
        if (call == null || call.MethodBase == null)
            throw new NotSupportedException(UnsupportedCode + ":INVALID_MESSAGE");
        MethodBase member = call.MethodBase;
        string name = member.DeclaringType.FullName + "." + member.Name;
        name = name.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\0", "\\0");
        if (name.Length > 512) name = name.Substring(0, 512) + "[truncated]";
        int count;
        lock (requestLock) { if (requests < 17) requests++; count = requests; }
        // Do not inspect arguments or call ToString on a proxied object. Even
        // non-getter/object calls are denied; nothing receives a default value.
        if (count <= 16)
            record("INITIALIZE_CONTEXT_REQUEST number=" + count + " member=" + name + " token=" + member.MetadataToken.ToString("X8")
                + " arguments=" + call.ArgCount + " thread=" + Thread.CurrentThread.ManagedThreadId + " result=" + UnsupportedCode);
        return new ReturnMessage(new NotSupportedException(UnsupportedCode + ":" + (count <= 16 ? name : "REQUEST_LIMIT")), call);
    }

    internal static InitializeContextProbe ForOriginal(Type originalType, Action<string> recorder)
    {
        MethodInfo method = originalType.Module.ResolveMethod(0x0600001a) as MethodInfo;
        ProbeFiles.Require(method != null && method.DeclaringType == originalType && method.Name == "Initialize"
            && method.IsPublic && !method.IsStatic && !method.IsGenericMethod && method.ReturnType == typeof(void), "INITIALIZE_ENTRYPOINT_SIGNATURE");
        ParameterInfo[] parameters = method.GetParameters();
        ProbeFiles.Require(parameters.Length == 1, "INITIALIZE_PARAMETER_COUNT");
        Type context = parameters[0].ParameterType;
        ProbeFiles.Require(context.FullName == ContextName && context.Assembly.FullName == AddInIdentity
            && context.MetadataToken == 0x0200000f && context.IsPublic && context.IsInterface && context.GetInterfaces().Length == 0, "INITIALIZE_CONTEXT_IDENTITY");
        MethodInfo[] methods = context.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(methods.Length == Getters.Length, "INITIALIZE_CONTEXT_MEMBER_COUNT");
        for (int i = 0; i < Getters.Length; i++) {
            MethodInfo getter = context.GetMethod(Getters[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            string identity = i == 0 ? SdkIdentity : i == 1 ? ResourcesIdentity : i < 4 ? XmlIdentity : AddInIdentity;
            ProbeFiles.Require(getter != null && getter.IsSpecialName && !getter.IsStatic && !getter.IsGenericMethod
                && getter.MetadataToken == 0x0600002f + i && getter.GetParameters().Length == 0
                && getter.ReturnType.FullName == Returns[i] && getter.ReturnType.Assembly.FullName == identity, "INITIALIZE_CONTEXT_GETTER_SIGNATURE");
            recorder("INITIALIZE_CONTEXT_CONTRACT member=" + getter.Name + " token=" + getter.MetadataToken.ToString("X8")
                + " returns=" + getter.ReturnType.AssemblyQualifiedName);
        }
        var proxy = new InitializeContextProbe(context, method, recorder);
        ProbeFiles.Require(context.IsInstanceOfType(proxy.GetTransparentProxy()), "INITIALIZE_CONTEXT_ASSIGNMENT");
        return proxy;
    }

    internal void InvokeOriginal(object original)
    {
        bool returned = false;
        record("INITIALIZE_BEGIN type=" + original.GetType().FullName + " token=" + initialize.MetadataToken.ToString("X8") + " context=deny-all");
        try { initialize.Invoke(original, new object[] { GetTransparentProxy() }); returned = true; }
        finally {
            record("INITIALIZE_END returned=" + returned + " denied-requests=" + Requests
                + " qualification=NOT_ESTABLISHED request-count-saturated=" + (Requests == 17) + " ui=NOT_RUN state=NOT_RUN hardware=NOT_RUN");
        }
    }

    internal static void TestOwned(Assembly fixtures, Action<string> recorder)
    {
        Type context = fixtures.GetType("Owned.Task008.IInitializationContextFixture", true, false);
        Type caller = fixtures.GetType("Owned.Task008.InitializationFixture", true, false);
        var proxy = new InitializeContextProbe(context, null, recorder);
        object transparent = proxy.GetTransparentProxy();
        ProbeFiles.Require(context.IsInstanceOfType(transparent), "OWNED_PROXY_INTERFACE_ASSIGNMENT");
        for (int i = 0; i < Getters.Length; i++) {
            MethodInfo getter = context.GetMethod(Getters[i]);
            ProbeFiles.Require(getter != null, "OWNED_PROXY_GETTER_REQUIRED");
            bool denied = false;
            try { getter.Invoke(transparent, new object[0]); }
            catch (TargetInvocationException error) {
                denied = error.InnerException is NotSupportedException
                    && error.InnerException.Message == UnsupportedCode + ":" + context.FullName + "." + Getters[i];
            }
            ProbeFiles.Require(denied && proxy.Requests == i + 1, "OWNED_PROXY_GETTER_DENIAL");
        }
        MethodInfo direct = caller.GetMethod("AssertDirectFailure", BindingFlags.Public | BindingFlags.Static);
        ProbeFiles.Require(direct != null && Object.Equals(direct.Invoke(null, new object[] { transparent }), true)
            && proxy.Requests == 7, "OWNED_PROXY_DIRECT_DENIAL");
        MethodInfo initialize = caller.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        object instance = Activator.CreateInstance(caller);
        bool wrapped = false;
        try { initialize.Invoke(instance, new object[] { transparent }); }
        catch (TargetInvocationException error) {
            wrapped = error.InnerException is NotSupportedException
                && error.InnerException.Message == UnsupportedCode + ":" + context.FullName + ".get_Project";
        }
        ProbeFiles.Require(wrapped && proxy.Requests == 8, "OWNED_PROXY_INITIALIZE_WRAPPED_DENIAL");
        bool unqualified = false;
        var catching = new InitializeContextProbe(context, caller.GetMethod("InitializeCatching"), delegate(string line) {
            recorder(line);
            if (line.Contains("INITIALIZE_END returned=True denied-requests=1 qualification=NOT_ESTABLISHED")) unqualified = true;
        });
        catching.InvokeOriginal(instance);
        ProbeFiles.Require(unqualified && catching.Requests == 1, "OWNED_PROXY_CAUGHT_DENIAL_UNQUALIFIED");
        recorder("OWNED_INITIALIZE_PROXY_PASS getters=6 direct-denial=true wrapped-denial=true requests=" + proxy.Requests + " original=NOT_RUN hardware=NOT_RUN");
        recorder("OWNED_INITIALIZE_CAUGHT_DENIAL_PASS qualification=NOT_ESTABLISHED original=NOT_RUN");
        GC.KeepAlive(proxy);
        GC.KeepAlive(catching);
    }
}
