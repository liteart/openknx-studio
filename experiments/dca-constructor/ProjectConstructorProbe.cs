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
        int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback();
        try { return InvokeCore(message); }
        finally { ProjectConstructorProbe.RestoreProjectResourceCallback(suspended); }
    }
    IMessage InvokeCore(IMessage message)
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
internal class ProjectStoreIdProxy : RealProxy
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
        int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback();
        try { return InvokeCore(message); }
        finally { ProjectConstructorProbe.RestoreProjectResourceCallback(suspended); }
    }
    protected IMessage InvokeCore(IMessage message)
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

// Only the new diagnostic uses this counter-first adapter. It supplies no service value.
internal sealed class UndoProviderDenyProxy : RealProxy
{
    readonly object sync = new object();
    readonly Action<string> record;
    int requests;
    internal UndoProviderDenyProxy(Type contract, Action<string> recorder) : base(contract)
    { ProbeFiles.Require(ProjectConstructorProbe.UndoDeniedMode && contract.IsPublic && contract.IsInterface && !contract.IsGenericType, "UNDO_DENY_PROXY_CONTRACT"); record = recorder; }
    internal int Requests { get { lock (sync) { return requests; } } }
    public override IMessage Invoke(IMessage message)
    {
        int count; lock (sync) { if (requests < 17) requests++; count = requests; }
        int previous = 0; bool suspended = false;
        try {
            previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true;
            var call = message as IMethodCallMessage;
            string member = call == null || call.MethodBase == null || call.MethodBase.DeclaringType == null ? "INVALID_MESSAGE"
                : ProjectConstructorProbe.Text(call.MethodBase.DeclaringType.FullName + "." + call.MethodName);
            if (count <= 16) record("UNDO_PROVIDER_SERVICE_DENIED number=" + count + " member=" + member + " value=NOT_PROVIDED arguments=NOT_INSPECTED");
            var error = new NotSupportedException("UNDO_PROVIDER_UNSUPPORTED:" + (count <= 16 ? member : "REQUEST_LIMIT"));
            if (call == null || call.MethodBase == null) throw error;
            return new ReturnMessage(error, call);
        } finally { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
    }
}

// A service-reference factory only. The captured provider still denies every method.
internal sealed class UndoProviderFactoryHolder
{
    readonly object sync = new object(), instance;
    readonly Type contract;
    readonly int ownerThread;
    readonly Action<string> record;
    internal readonly Delegate Callback;
    int entries, successes, lifetime;
    bool violation, inCallback;
    internal UndoProviderFactoryHolder(Type service, object proxy, Type callbackType, Action<string> recorder)
    {
        ProbeFiles.Require(ProjectConstructorProbe.UndoFactoryMode && service != null && service.IsPublic && service.IsInterface && !service.IsGenericType
            && proxy != null && service.IsInstanceOfType(proxy) && recorder != null && callbackType != null && callbackType.IsGenericType
            && callbackType.GetGenericTypeDefinition() == typeof(Func<,>) && callbackType.GetGenericArguments()[0].IsPublic && callbackType.GetGenericArguments()[0].IsInterface
            && callbackType.GetGenericArguments()[1] == service, "UNDO_FACTORY_HOLDER_CONTRACT");
        contract = service; instance = proxy; record = recorder; ownerThread = Thread.CurrentThread.ManagedThreadId;
        MethodInfo open = typeof(UndoProviderFactoryHolder).GetMethod("Provide", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(open != null && open.IsGenericMethodDefinition && open.GetGenericArguments().Length == 1
            && open.GetGenericArguments()[0].GenericParameterAttributes == GenericParameterAttributes.ReferenceTypeConstraint
            && open.GetGenericArguments()[0].GetGenericParameterConstraints().Length == 0 && open.GetParameters().Length == 1
            && open.GetParameters()[0].ParameterType == typeof(object) && open.ReturnType == open.GetGenericArguments()[0], "UNDO_FACTORY_FIXED_OWNED_METHOD");
        MethodInfo method = open.MakeGenericMethod(service);
        Callback = Delegate.CreateDelegate(callbackType, this, method, true);
        ProbeFiles.Require(Callback != null && Object.ReferenceEquals(Callback.Target, this) && Callback.Method.Equals(method) && Callback.GetType() == callbackType, "UNDO_FACTORY_CLOSED_DELEGATE");
    }
    internal int Entries { get { lock (sync) { return entries; } } }
    internal int Successes { get { lock (sync) { return successes; } } }
    internal bool Faulted { get { lock (sync) { return violation; } } }
    internal void Begin()
    { lock (sync) { if (lifetime != 0) { violation = true; throw new NotSupportedException("UNDO_FACTORY_LIFETIME_BEGIN"); } lifetime = 1; } }
    internal void Close() { lock (sync) { if (lifetime != 1) violation = true; lifetime = 2; } }
    public T Provide<T>(object context) where T : class
    {
        int count; lock (sync) { if (entries < 2) entries++; count = entries; }
        bool entered = false, suspended = false, prepared = false; int previous = 0;
        try {
            bool eligible = ProjectConstructorProbe.UndoFactoryWindowEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            lock (sync) {
                ProbeFiles.Require(count == 1 && lifetime == 1 && !inCallback && !violation && !excluded && typeof(T) == contract
                    && context != null && Thread.CurrentThread.ManagedThreadId == ownerThread
                    && eligible, "UNDO_FACTORY_CALLBACK_CONTEXT");
                inCallback = true; entered = true;
            }
            T value = (T)instance;
            record("UNDO_PROVIDER_FACTORY_CALLBACK entries=1 same-existing-proxy=True provider-value=NOT_PROVIDED context=NOT_INSPECTED");
            ProbeFiles.Require(!Faulted, "UNDO_FACTORY_CALLBACK_CAUGHT_FAULT");
            prepared = true; return value;
        } catch { lock (sync) { violation = true; } throw; }
        finally {
            try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
            catch { lock (sync) { violation = true; } throw; }
            finally { lock (sync) { if (entered) inCallback = false; if (prepared && !violation) successes++; } }
        }
    }
}

// Source27 keeps counters/faults independent of the inherited Id fixture and stream denials.
internal sealed class CacheReferenceState
{
    readonly object sync = new object(); readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
    int entries, attempts, replies, lifetime; bool faulted, entered;
    internal int Entries { get { lock (sync) { return entries; } } }
    internal int Attempts { get { lock (sync) { return attempts; } } }
    internal int Replies { get { lock (sync) { return replies; } } }
    internal bool Faulted { get { lock (sync) { return faulted; } } }
    internal int Count() { lock (sync) { if (entries < 17) entries++; return entries; } }
    internal void Fail() { lock (sync) { faulted = true; } }
    internal void Begin() { lock (sync) { if (lifetime != 0) { faulted = true; throw new NotSupportedException("CACHE_LIFETIME_BEGIN"); } lifetime = 1; } }
    internal void Close() { lock (sync) { if (lifetime != 1) faulted = true; lifetime = 2; } }
    internal void Enter(int count, bool excluded)
    { lock (sync) { if (count > 16 || excluded || entered || faulted || lifetime != 1) { faulted = true; throw new NotSupportedException("CACHE_CALLBACK_CONTEXT"); } entered = true; } }
    internal void CacheAttempt(bool eligible)
    { lock (sync) { if (attempts < 2) attempts++; if (attempts != 1 || !eligible || Thread.CurrentThread.ManagedThreadId != ownerThread || lifetime != 1 || faulted) { faulted = true; throw new NotSupportedException("CACHE_REFERENCE_UNSUPPORTED"); } } }
    internal void Exit(bool ownsEntry, bool prepared)
    { lock (sync) { if (ownsEntry) entered = false; if (prepared && !faulted) replies++; } }
}

// Only canonical zero-argument calls reach the preserved Id core; no original getter is reread there.
internal sealed class CacheCallSnapshot : IMethodCallMessage
{
    readonly MethodInfo method; readonly LogicalCallContext context; readonly string uri;
    readonly System.Collections.IDictionary properties = new System.Collections.Hashtable();
    internal CacheCallSnapshot(MethodInfo value, LogicalCallContext callContext, string callUri) { method = value; context = callContext; uri = callUri; }
    public MethodBase MethodBase { get { return method; } }
    public string MethodName { get { return method.Name; } }
    public int ArgCount { get { return 0; } } public int InArgCount { get { return 0; } }
    public object[] Args { get { return new object[0]; } } public object[] InArgs { get { return new object[0]; } }
    public bool HasVarArgs { get { return false; } } public LogicalCallContext LogicalCallContext { get { return context; } }
    public object MethodSignature { get { return Type.EmptyTypes; } } public string TypeName { get { return method.DeclaringType.AssemblyQualifiedName; } }
    public string Uri { get { return uri; } set { throw new NotSupportedException("CACHE_CALL_SNAPSHOT_IMMUTABLE"); } }
    public System.Collections.IDictionary Properties { get { return properties; } }
    public object GetArg(int index) { throw new NotSupportedException("CACHE_CALL_NO_ARGUMENTS"); }
    public string GetArgName(int index) { throw new NotSupportedException("CACHE_CALL_NO_ARGUMENTS"); }
    public object GetInArg(int index) { throw new NotSupportedException("CACHE_CALL_NO_ARGUMENTS"); }
    public string GetInArgName(int index) { throw new NotSupportedException("CACHE_CALL_NO_ARGUMENTS"); }
}
internal sealed class StreamStoreDenyProxy : RealProxy
{
    readonly object sync = new object(); readonly Action<string> record; readonly MethodInfo[] methods;
    int requests;
    internal StreamStoreDenyProxy(Type contract, MethodInfo[] declarations, Action<string> recorder) : base(contract)
    { ProbeFiles.Require(ProjectConstructorProbe.CacheMode && contract.IsPublic && contract.IsInterface && declarations.Length == 7, "STREAM_PROXY_CONTRACT"); methods = declarations; record = recorder; }
    internal int Requests { get { lock (sync) { return requests; } } }
    public override IMessage Invoke(IMessage message)
    {
        int count; lock (sync) { if (requests < 17) requests++; count = requests; }
        bool suspended = false; int previous = 0;
        try {
            if (!ProjectConstructorProbe.UndoFactoryCallbackExcluded()) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            var call = message as IMethodCallMessage; MethodInfo method = call == null ? null : call.MethodBase as MethodInfo;
            string member = "UNTRUSTED_OR_FOREIGN_METHOD";
            foreach (MethodInfo expected in methods) if (ProjectConstructorProbe.ExactRuntimeMethod(method, expected)) { member = expected.DeclaringType.FullName + "." + expected.Name; break; }
            foreach (MethodInfo expected in new MethodInfo[] { typeof(object).GetMethod("GetType"), typeof(object).GetMethod("ToString"), typeof(object).GetMethod("GetHashCode"), typeof(object).GetMethod("Equals", new Type[] { typeof(object) }), typeof(IDisposable).GetMethod("Dispose") })
                if (ProjectConstructorProbe.ExactRuntimeMethod(method, expected)) { member = expected.DeclaringType.FullName + "." + expected.Name; break; }
            if (count <= 16) record("STREAM_STORE_DENIED number=" + count + " member=" + ProjectConstructorProbe.Text(member) + " value=NOT_PROVIDED arguments=NOT_INSPECTED");
            var error = new NotSupportedException("STREAM_STORE_UNSUPPORTED:" + (count <= 16 ? member : "REQUEST_LIMIT"));
            if (call == null || method == null) throw error;
            return new ReturnMessage(error, call);
        } finally { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
    }
}
internal sealed class LargeStringCacheStoreProxy : ProjectStoreIdProxy
{
    readonly MethodInfo idGetter, cacheGetter; readonly object reference; readonly Action<string> record;
    internal readonly CacheReferenceState State = new CacheReferenceState();
    internal LargeStringCacheStoreProxy(Type owner, MethodInfo idMethod, object id, MethodInfo cacheMethod, Type streamType, object stream, Action<string> recorder)
        : base(owner, idMethod, id, recorder)
    { ProjectConstructorProbe.CacheGetterSignature(owner, cacheMethod, streamType); ProbeFiles.Require(ProjectConstructorProbe.CacheMode && stream != null && streamType.IsInstanceOfType(stream), "CACHE_REFERENCE_CONTRACT"); idGetter = idMethod; cacheGetter = cacheMethod; reference = stream; record = recorder; }
    public override IMessage Invoke(IMessage message)
    {
        int count = State.Count(); bool suspended = false, entered = false, prepared = false; int previous = 0;
        MethodInfo observedMethod = null; bool methodCaptured = false, diagnosticAttempted = false; string phase = "ENTRY";
        try {
            bool eligible = ProjectConstructorProbe.CacheWindowEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            State.Enter(count, excluded); entered = true;
            phase = "METHOD_READ";
            IMethodCallMessage call = message as IMethodCallMessage; MethodInfo method = call == null ? null : call.MethodBase as MethodInfo;
            observedMethod = method; methodCaptured = true; phase = "METHOD_MATCH";
            bool cache = ProjectConstructorProbe.ExactRuntimeMethod(method, cacheGetter), id = ProjectConstructorProbe.ExactRuntimeMethod(method, idGetter);
            phase = "CACHE_ELIGIBILITY";
            if (cache) State.CacheAttempt(eligible);
            phase = "MESSAGE_SHAPE";
            ProbeFiles.Require((cache || id) && call.MethodName == (cache ? cacheGetter.Name : idGetter.Name) && call.ArgCount == 0 && call.InArgCount == 0, "CACHE_MESSAGE_SHAPE");
            phase = "ARGS_READ";
            object[] arguments = call.Args; phase = "ARGS_SHAPE"; ProbeFiles.Require(arguments != null && arguments.Length == 0, "CACHE_MESSAGE_ARGUMENT_SHAPE");
            phase = "CALL_CONTEXT_READ";
            LogicalCallContext context = call.LogicalCallContext; phase = "URI_READ"; string uri = call.Uri;
            phase = "URI_SHAPE";
            ProbeFiles.Require(uri == null || uri.Length <= 1024, "CACHE_MESSAGE_URI_BOUND");
            phase = "SNAPSHOT";
            var snapshot = new CacheCallSnapshot(cache ? cacheGetter : idGetter, context, uri);
            phase = "REENTRY_GUARD";
            ProbeFiles.Require(!State.Faulted, "CACHE_MESSAGE_CAUGHT_REENTRY");
            if (!cache) {
                phase = "ID_REPLY";
                IMessage result = base.InvokeCore(snapshot);
                var returned = result as IMethodReturnMessage; ProbeFiles.Require(returned != null && returned.Exception == null && !State.Faulted, "CACHE_ID_REPLY_FAULT"); return result;
            }
            phase = "CACHE_RECORD";
            record("LARGE_STRING_CACHE_REFERENCE reply=PREPARED same-preexisting-reference=True provider-values=NOT_PROVIDED");
            phase = "CACHE_REPLY";
            IMessage response = new ReturnMessage(reference, new object[0], 0, context, snapshot);
            ProbeFiles.Require(!State.Faulted, "CACHE_REPLY_CAUGHT_REENTRY"); prepared = true; return response;
        } catch {
            State.Fail();
            DiagnoseFailure(observedMethod, methodCaptured, phase, count, entered, ref diagnosticAttempted);
            throw;
        }
        finally {
            try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
            catch {
                State.Fail();
                DiagnoseFailure(observedMethod, methodCaptured, "RESTORE", count, entered, ref diagnosticAttempted);
                throw;
            }
            finally { State.Exit(entered, prepared); }
        }
    }
    void DiagnoseFailure(MethodInfo method, bool captured, string phase, int count, bool entered, ref bool attempted)
    {
        if (!ProjectConstructorProbe.StoreRequestDiagnostics || attempted || !entered || count > 16 || !ProjectConstructorProbe.UndoFactoryCallbackExcluded()) return;
        attempted = true;
        record("STORE_REQUEST_DIAGNOSTIC phase=" + phase + " entry=" + count + " " + StoreRequestDescriptor(method, cacheGetter, captured) + " values=NOT_INSPECTED");
    }
    internal static string StoreRequestDescriptor(MethodInfo method, MethodInfo anchor, bool captured)
    {
        if (!captured || method == null) return "category=MEMBER_UNAVAILABLE";
        if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method) || method.GetType() != typeof(object).GetMethod("GetType").GetType()) return "category=UNTRUSTED_METHOD_KIND";
        Type owner = anchor.DeclaringType;
        if (method.Module != anchor.Module || method.DeclaringType != owner) return "category=FOREIGN_OWNER";
        int token = method.MetadataToken;
        if ((token & unchecked((int)0xff000000)) != 0x06000000 || (token & 0x00ffffff) == 0) return "category=UNSUPPORTED_TOKEN";
        return "category=TRUSTED_STORE_MEMBER owner=" + ProjectConstructorProbe.Text(owner.FullName) + " token=" + token.ToString("X8") + " name=" + ProjectConstructorProbe.Text(method.Name);
    }

}


// Source32 state is retained independently from every earlier provider/store audit.
internal sealed class ContextDenialState
{
    readonly object sync = new object(); readonly int limit; int entries; bool faulted;
    internal ContextDenialState(int cap) { limit = cap; }
    internal int Entries { get { lock (sync) { return entries; } } }
    internal bool Faulted { get { lock (sync) { return faulted; } } }
    internal int Deny() { lock (sync) { if (entries < limit + 1) entries++; faulted = true; return entries; } }
    internal void Fail() { lock (sync) { faulted = true; } }
}
internal sealed class ContextArgumentDenyProxy : RealProxy
{
    internal readonly ContextDenialState State = new ContextDenialState(16);
    readonly string name; readonly Action<string> record;
    internal ContextArgumentDenyProxy(Type type, string label, Action<string> recorder) : base(type) { name = label; record = recorder; }
    public override IMessage Invoke(IMessage message)
    {
        int count = State.Deny(); bool suspended = false; int previous = 0;
        try {
            if (!ProjectConstructorProbe.UndoFactoryCallbackExcluded()) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            if (count > 16) throw new NotSupportedException("CONTEXT_ARGUMENT_BUDGET:" + name);
            record("CONTEXT_ARGUMENT_DENIED argument=" + name + " entry=" + count + " values=NOT_INSPECTED");
            var error = new NotSupportedException("CONTEXT_ARGUMENT_UNSUPPORTED:" + name);
            IMethodCallMessage call = message as IMethodCallMessage;
            if (call == null) throw error;
            return new ReturnMessage(error, call);
        } catch { State.Fail(); throw; }
        finally { try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); } catch { State.Fail(); throw; } }
    }
}
internal sealed class ContextImporterDenyHolder
{
    internal readonly ContextDenialState State = new ContextDenialState(1);
    internal readonly Delegate Callback; readonly Action<string> record;
    internal ContextImporterDenyHolder(Type callbackType, Type result, Action<string> recorder)
    {
        record = recorder;
        MethodInfo definition = typeof(ContextImporterDenyHolder).GetMethod("Deny", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(definition != null && definition.IsGenericMethodDefinition && definition.GetGenericArguments().Length == 1
            && definition.GetParameters().Length == 7, "CONTEXT_IMPORTER_OWNED_METHOD");
        foreach (ParameterInfo parameter in definition.GetParameters()) ProbeFiles.Require(parameter.ParameterType == typeof(object), "CONTEXT_IMPORTER_OBJECT_PARAMETERS");
        MethodInfo method = definition.MakeGenericMethod(new Type[] { result });
        Callback = Delegate.CreateDelegate(callbackType, this, method, true);
        ProbeFiles.Require(Callback.GetType() == callbackType && Callback.GetInvocationList().Length == 1 && Object.ReferenceEquals(Callback.Target, this)
            && Callback.Method.Equals(method) && method.ReturnType == result, "CONTEXT_IMPORTER_EXACT_DELEGATE");
    }
    public T Deny<T>(object a1, object a2, object a3, object a4, object a5, object a6, object a7)
    {
        int count = State.Deny(); bool suspended = false; int previous = 0;
        try {
            if (!ProjectConstructorProbe.UndoFactoryCallbackExcluded()) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            if (count <= 1) record("CONTEXT_IMPORTER_DENIED entry=" + count + " values=NOT_INSPECTED");
            throw new NotSupportedException("CONTEXT_IMPORTER_UNSUPPORTED");
        } catch { State.Fail(); throw; }
        finally { try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); } catch { State.Fail(); throw; } }
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
    const string ValueTupleIdentity = "System.ValueTuple, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
    const string ValueTupleHash = "62216986c754ad8f0ee68412c42cfc3fc6e025bd908ae1fef1a3a8d528b1495a";
    const string ValueTupleRelativePath = "Microsoft.Net\\assembly\\GAC_MSIL\\System.ValueTuple\\v4.0_4.0.0.0__cc7b13ffcd2ddd51\\System.ValueTuple.dll";
    const string ReviewedProjectConstructionReportSha256 = "5253681cc48e42142a7d214e9229d2d9b68c8f3580152c0ff4a63fb6129d19a2";
    const string ReviewedIdMetadataSha256 = "37fc63aa00825de9d694ae96721bd4395eaca039bc3e6bc92435f29e7e6311c4";
    const string ReviewedIdObservationSha256 = "89cad1fa998c5b5d6a85604055a698b402b6120fe247ef32b95b78198097560a";
    const uint FixtureProjectNumber = 1; // Owned synthetic data, not a real or claimed-valid project identity.
    const string ReviewedFactoryConstructorReportSha256 = "df361cb84fc232b6f7da6943114096cd3e14b638495dcc8b68e10dbd42524ea9";
    const string ReviewedFactoryCandidatesAuditSha256 = "8f18be8da62da7d06f11634467b3c869e2ac260f20956cb5ab31f552ff324432";
    const string Async8Identity = "Microsoft.Bcl.AsyncInterfaces, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
    const string AutofacResourceEnGb = "Autofac.resources, Version=8.2.0.0, Culture=en-GB, PublicKeyToken=17863af14b0044da";
    const string AutofacResourceEn = "Autofac.resources, Version=8.2.0.0, Culture=en, PublicKeyToken=17863af14b0044da";
    [ThreadStatic] static bool resourcePhaseActive;
    [ThreadStatic] static int resourceProbeCount;
    [ThreadStatic] static bool projectResourceBudgetActive, projectResourceCallbackSuspended;
    [ThreadStatic] static int projectResourceWindow, projectResourceProbeCount;
    static Assembly resourceOwner;
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
    static string ownedResourceCase;
    static string ownedUndoCase;
    static string ownedFactoryCase;
    static string ownedCacheCase;
    static LargeStringCacheStoreProxy activeCache;
    static StreamStoreDenyProxy activeStream;
    static readonly string[] OwnedCacheCases = { "second", "total-boundary", "total-limit", "stream-id", "stream-exists", "stream-read", "stream-multi", "stream-delete", "stream-time", "stream-mask", "object-type", "object-string", "object-hash", "object-equals", "dispose-message", "foreign", "invalid", "custom-method", "throw-method", "throw-args", "throw-context", "mutable", "mutable-id", "message-reentry", "forged-name", "forged-count", "forged-in", "forged-args", "recorder", "wrapper-restore", "stream-recorder", "stream-restore", "nested", "off-thread", "trace-ctor", "builder", "registration", "single", "external", "build", "factory", "validation", "logging", "error", "project-dispose", "container-dispose", "trace-dispose", "wrapper-final", "stream-final", "late" };
    static UndoProviderFactoryHolder activeFactory;
    static readonly string[] OwnedFactoryCases = { "project-activation", "no-activation", "null", "wrong-type", "second", "saturation", "nested", "off-thread", "recorder", "resource", "get", "new", "root", "trace-ctor", "builder", "registration", "single", "external", "build", "factory", "project", "project-dispose", "container-dispose", "trace-dispose", "error", "final-audit", "late" };
    static readonly string[] OwnedUndoCases = { "get", "new", "root", "invalid", "saturation", "trace-ctor", "builder", "registration", "external", "build", "factory", "project", "project-dispose", "container-dispose", "trace-dispose", "error", "final-audit", "nested", "resource", "register-null", "external-null" };
    static UndoProviderDenyProxy activeUndo;
    static readonly string[] OwnedResourceCases = { "id-constructor", "id-getter", "getter", "builder", "registration", "external", "build", "factory", "project", "validation", "logging", "error", "project-dispose", "container-dispose", "trace-dispose", "nested-window", "nested-budget", "nested-callback", "off-thread" };
    static volatile bool unexpected, rejected;
    static volatile bool enumInputsPinned;
    static volatile bool platformFacadePinned, valueTuplePinned;
    static volatile bool factoryInputsReady, containerInputsReady;
    static string platformFacadePath, valueTuplePath;
    static int logBytes, assemblyCount;
    static object constructed;
    static bool EnumOnly { get { return mode == "project-enum-metadata"; } }
    static bool Original { get { return mode == "original-project" || mode == "original-project-store-id"; } }
    static bool OwnedProject { get { return mode == "owned-project" || mode == "owned-project-store-id"; } }
    static bool StoreIdFixture { get { return mode == "original-project-store-id" || mode == "owned-project-store-id"; } }
    static bool FactoryOriginal { get { return mode == "original-factory" || mode == "original-factory-bcl9" || mode == "original-factory-empty-container" || TraceOriginal || RegisteredOriginal; } }
    static bool FactoryOwned { get { return mode == "owned-factory" || mode == "owned-factory-bcl9" || mode == "owned-factory-empty-container" || TraceOwned || RegisteredOwned; } }
    static bool FactoryOnly { get { return FactoryOriginal || FactoryOwned; } }
    static bool EmptyContainer { get { return mode == "original-factory-empty-container" || mode == "owned-factory-empty-container" || TraceOnly || RegisteredTrace; } }
    static bool TraceOriginal { get { return mode == "original-trace-encryptor"; } }
    static bool TraceOwned { get { return mode == "owned-trace-encryptor"; } }
    const string XmlRegistrationName = "Knx.Ets.Xml.RegistrationRelevanceInformation";
    const string XmlRegistrationHash = "a9bce396208e9d5b9680e65ef5d730ed9873c21f04de5028cb6f492264c70ac3";
    const string XmlRegistrationIdentity = "Knx.Ets.Xml.RegistrationRelevanceInformation, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const int XmlRegistrationSize = 188880;
    static volatile bool xmlRegistrationReady;
    static string ownedXmlPinCase;
    static string ownedStoreDiagnosticCase;
    static readonly string[] OwnedStoreDiagnosticCases = { "positive", "declared", "forged", "custom", "foreign", "method-throw", "args", "recorder", "reentry", "saturated" };
    static bool XmlRegistrationOriginal { get { return mode == "original-project-context-xml-registration"; } }
    internal static bool StoreRequestDiagnostics { get { return XmlRegistrationOriginal || XmlRegistrationOwned; } }
    static bool XmlRegistrationOwned { get { return mode == "owned-project-context-xml-registration"; } }
    static bool ContextOriginal { get { return mode == "original-project-context-denied" || XmlRegistrationOriginal; } }
    static bool ContextOwned { get { return mode == "owned-project-context-denied" || XmlRegistrationOwned; } }
    static bool ContextMode { get { return ContextOriginal || ContextOwned; } }
    static bool CacheOriginal { get { return mode == "original-project-large-string-cache-denied" || ContextOriginal; } }
    static bool CacheOwned { get { return mode == "owned-project-large-string-cache-denied" || ContextOwned; } }
    internal static bool CacheMode { get { return CacheOriginal || CacheOwned; } }
    internal static bool CacheWindowEligible() { return CacheMode && projectResourceBudgetActive && !projectResourceCallbackSuspended && projectResourceWindow == 2; }
    static bool UndoFactoryOriginal { get { return mode == "original-project-undo-provider-factory" || CacheOriginal; } }
    static bool UndoFactoryOwned { get { return mode == "owned-project-undo-provider-factory" || CacheOwned; } }
    internal static bool UndoFactoryMode { get { return UndoFactoryOriginal || UndoFactoryOwned; } }
    internal static bool UndoFactoryCallbackExcluded() { return projectResourceCallbackSuspended; }
    internal static bool UndoFactoryWindowEligible()
    { return UndoFactoryMode && projectResourceBudgetActive && !projectResourceCallbackSuspended && (projectResourceWindow == 1 || projectResourceWindow == 2); }
    static bool UndoDeniedOriginal { get { return mode == "original-project-undo-provider-denied" || UndoFactoryOriginal; } }
    static bool UndoDeniedOwned { get { return mode == "owned-project-undo-provider-denied" || UndoFactoryOwned; } }
    internal static bool UndoDeniedMode { get { return UndoDeniedOriginal || UndoDeniedOwned; } }
    static bool ProjectResourcesOriginal { get { return mode == "original-project-genuine-factory-resources" || UndoDeniedOriginal; } }
    static bool ProjectResourcesOwned { get { return mode == "owned-project-genuine-factory-resources" || UndoDeniedOwned; } }
    static bool ProjectResources { get { return ProjectResourcesOriginal || ProjectResourcesOwned; } }
    static bool ProjectScopeOriginal { get { return mode == "original-project-genuine-factory" || ProjectResourcesOriginal; } }
    static bool ProjectScopeOwned { get { return mode == "owned-project-genuine-factory" || ProjectResourcesOwned; } }
    static bool ProjectScope { get { return ProjectScopeOriginal || ProjectScopeOwned; } }
    static bool RegisteredOriginal { get { return mode == "original-factory-registered-trace" || ProjectScopeOriginal; } }
    static bool RegisteredOwned { get { return mode == "owned-factory-registered-trace" || ProjectScopeOwned; } }
    static bool RegisteredTrace { get { return RegisteredOriginal || RegisteredOwned; } }
    static bool TraceOnly { get { return TraceOriginal || TraceOwned || RegisteredOwned; } }
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
    static void PinValueTupleReference()
    {
        ProbeFiles.Require(mode == "original-factory-empty-container" || TraceOriginal || RegisteredOriginal, "VALUETUPLE_REFERENCE_MODE");
        valueTuplePath = ProbeFiles.Local(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ValueTupleRelativePath), true, false);
        var stream = new FileStream(valueTuplePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        Inputs.Add(stream);
        ProbeFiles.Require(stream.Length == 25544, "VALUETUPLE_REFERENCE_SIZE");
        ProbeFiles.Require(ProbeFiles.Hash(stream) == ValueTupleHash, "VALUETUPLE_REFERENCE_HASH");
        ProbeFiles.Require(AssemblyName.GetAssemblyName(valueTuplePath).FullName == ValueTupleIdentity, "VALUETUPLE_REFERENCE_IDENTITY");
        valueTuplePinned = true;
        Record("VALUETUPLE_REFERENCE_PIN origin=FIXED_OS_REFERENCE location=" + Text(valueTuplePath) + " bytes=" + stream.Length + " sha256=" + ValueTupleHash);
    }
    // Private finite predicate only. Real admission uses the completed held-file pin.
    static bool ValueTupleReferencePolicy(string selectedMode, bool complete, string identity, string location, bool cache, bool reflectionOnly)
    {
        return (selectedMode == "original-factory-empty-container" || selectedMode == "original-trace-encryptor" || selectedMode == "original-factory-registered-trace" || selectedMode == "original-project-genuine-factory" || selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration") && complete && identity == ValueTupleIdentity && cache && !reflectionOnly
            && ProbeFiles.Same(location, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ValueTupleRelativePath));
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
    static void PinXmlRegistrationInputs()
    {
        ProbeFiles.Require(XmlRegistrationOriginal && !xmlRegistrationReady && !containerInputsReady, "XML_PROFILE_NEW_MODE");
        CountInputs(20);
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
        PinXmlRegistrationFile();
        PinValueTupleReference();
        PinPlatformFacade();
        for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < FactoryExtraNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, FactoryExtraNames[i] + ".dll")).FullName == FactoryExtraIdentities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < ContainerNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, ContainerNames[i] + ".dll")).FullName == ContainerIdentities[i], "CONTAINER_INPUT_IDENTITY");
        ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, XmlRegistrationName + ".dll")).FullName == XmlRegistrationIdentity, "XML_INPUT_IDENTITY");
        xmlRegistrationReady = true;
        containerInputsReady = true;
        Record("XML_REGISTRATION_PROFILE_READY private-pins=20 total-bytes=24466936 extra-reference-admission=THIS_MODE_ONLY");
    }

    static void PinXmlRegistrationFile()
    {
        string path = Path.Combine(stage, XmlRegistrationName + ".dll");
        var stream = new FileStream(ProbeFiles.Local(path, true, false), FileMode.Open, FileAccess.Read, FileShare.Read);
        Inputs.Add(stream);
        ProbeFiles.Require(stream.Length == XmlRegistrationSize, "XML_INPUT_SIZE");
        ProbeFiles.Require(ProbeFiles.Hash(stream) == XmlRegistrationHash, "XML_INPUT_HASH");
        Record("XML_REGISTRATION_PIN bytes=" + stream.Length + " sha256=" + XmlRegistrationHash);
    }
    static bool XmlRegistrationResolvePolicy(string selectedMode, bool complete, string requested, string policy)
    { return selectedMode == "original-project-context-xml-registration" && complete && requested == XmlRegistrationIdentity && policy == XmlRegistrationIdentity; }
    static bool XmlRegistrationReferencePolicy(string selectedMode, bool complete, string identity, string location, bool gac, bool reflectionOnly, bool dynamicAssembly, string directory)
    {
        return selectedMode == "original-project-context-xml-registration" && complete && identity == XmlRegistrationIdentity && !gac && !reflectionOnly && !dynamicAssembly
            && !String.IsNullOrEmpty(location) && !String.IsNullOrEmpty(directory) && ProbeFiles.Same(location, Path.Combine(directory, XmlRegistrationName + ".dll"));
    }
    static Assembly ResolveXmlRegistration(string requested, string policy)
    {
        try {
        ProbeFiles.Require(XmlRegistrationResolvePolicy(mode, xmlRegistrationReady && containerInputsReady, requested, policy), "XML_RESOLVE_EXACT_COMPLETE_PROFILE");
        string path = Path.Combine(stage, XmlRegistrationName + ".dll");
        Assembly assembly = Assembly.LoadFrom(path);
        ProbeFiles.Require(XmlRegistrationReferencePolicy(mode, xmlRegistrationReady && containerInputsReady, assembly.FullName, assembly.Location, assembly.GlobalAssemblyCache, assembly.ReflectionOnly, assembly.IsDynamic, stage), "XML_RESOLVED_IDENTITY_PRIVATE_PATH");
        Record("XML_REGISTRATION_RESOLVED identity=" + Text(assembly.FullName) + " held-pin-sha256=" + XmlRegistrationHash + " origin=FIXED_PRIVATE_STAGE");
        return assembly;
        } catch { rejected = true; throw; }
    }
    static void OwnedXmlRegistrationPolicyProof()
    {
        string selected = "original-project-context-xml-registration", path = Path.Combine(stage, XmlRegistrationName + ".dll"); int negatives = 0;
        ProbeFiles.Require(XmlRegistrationResolvePolicy(selected, true, XmlRegistrationIdentity, XmlRegistrationIdentity)
            && XmlRegistrationReferencePolicy(selected, true, XmlRegistrationIdentity, path, false, false, false, stage), "OWNED_XML_POSITIVE_PREDICATE_ONLY");
        foreach (string oldMode in new string[] { "original-project-context-denied", "owned-project-context-denied", "original-factory-empty-container", "owned-project-context-xml-registration" }) {
            ProbeFiles.Require(!XmlRegistrationResolvePolicy(oldMode, true, XmlRegistrationIdentity, XmlRegistrationIdentity)
                && !XmlRegistrationReferencePolicy(oldMode, true, XmlRegistrationIdentity, path, false, false, false, stage), "OWNED_XML_OLD_OWNED_DENIED"); negatives++;
        }
        foreach (string wrong in new string[] { XmlRegistrationIdentity.Replace("6.4.8718.0", "6.4.8718.1"), XmlRegistrationIdentity.Replace("neutral", "en"), XmlRegistrationIdentity.Replace("61439873ec5e1159", "61439873ec5e1158"), XmlRegistrationIdentity + ".resources" }) {
            ProbeFiles.Require(!XmlRegistrationResolvePolicy(selected, true, wrong, XmlRegistrationIdentity) && !XmlRegistrationResolvePolicy(selected, true, XmlRegistrationIdentity, wrong)
                && !XmlRegistrationReferencePolicy(selected, true, wrong, path, false, false, false, stage), "OWNED_XML_IDENTITY_MISMATCH"); negatives++;
        }
        ProbeFiles.Require(!XmlRegistrationResolvePolicy(selected, false, XmlRegistrationIdentity, XmlRegistrationIdentity)
            && !XmlRegistrationReferencePolicy(selected, false, XmlRegistrationIdentity, path, false, false, false, stage)
            && !XmlRegistrationReferencePolicy(selected, true, XmlRegistrationIdentity, path, true, false, false, stage)
            && !XmlRegistrationReferencePolicy(selected, true, XmlRegistrationIdentity, path, false, true, false, stage)
            && !XmlRegistrationReferencePolicy(selected, true, XmlRegistrationIdentity, path, false, false, true, stage)
            && !XmlRegistrationReferencePolicy(selected, true, XmlRegistrationIdentity, fixture, false, false, false, stage), "OWNED_XML_COMPLETION_PRIVATE_ORIGIN");
        Record("OWNED_XML_POLICY_PASS identity-negatives=" + negatives + " private-origin-negatives=6 predicate-only=True actual-XML-load=NOT_RUN");
    }
    static void OwnedXmlRegistrationPinNegative()
    {
        ProbeFiles.Require(XmlRegistrationOwned && (ownedXmlPinCase == "missing" || ownedXmlPinCase == "size" || ownedXmlPinCase == "hash"), "OWNED_XML_PIN_FIXED_CASE");
        bool refused = false;
        try { PinXmlRegistrationFile(); }
        catch (IOException error) {
            refused = ownedXmlPinCase == "missing" ? !File.Exists(Path.Combine(stage, XmlRegistrationName + ".dll"))
                : error.Message == (ownedXmlPinCase == "size" ? "XML_INPUT_SIZE" : "XML_INPUT_HASH");
        }
        ProbeFiles.Require(refused && !xmlRegistrationReady && !containerInputsReady, "OWNED_XML_PIN_REFUSAL_NO_READY");
        Record("OWNED_XML_PIN_NEGATIVE_PASS case=" + ownedXmlPinCase + " xml-ready=False container-ready=False identity-load=NOT_REACHED");
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
        PinValueTupleReference();
        PinPlatformFacade();
        for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < FactoryExtraNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, FactoryExtraNames[i] + ".dll")).FullName == FactoryExtraIdentities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < ContainerNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, ContainerNames[i] + ".dll")).FullName == ContainerIdentities[i], "CONTAINER_INPUT_IDENTITY");
        containerInputsReady = true;
        Record("FACTORY_CONTAINER_PROFILE_READY private-pins=19 total-bytes=24278056 extra-reference-admission=THIS_MODE_ONLY");
    }
    static bool ContainerReferencePolicy(string selectedMode, bool complete, string identity, string location, bool reflectionOnly)
    {
        if ((selectedMode != "original-factory-empty-container" && selectedMode != "original-trace-encryptor" && selectedMode != "original-factory-registered-trace" && selectedMode != "original-project-genuine-factory" && selectedMode != "original-project-genuine-factory-resources" && selectedMode != "original-project-undo-provider-denied" && selectedMode != "original-project-undo-provider-factory" && selectedMode != "original-project-large-string-cache-denied" && selectedMode != "original-project-context-denied" && selectedMode != "original-project-context-xml-registration") || !complete || reflectionOnly) return false;
        for (int i = 0; i < ContainerNames.Length; i++)
            if (identity == ContainerIdentities[i] && ProbeFiles.Same(location, Path.Combine(stage, ContainerNames[i] + ".dll"))) return true;
        return false;
    }
    // Private finite policy seam; real facts come from the completed pin and Assembly.
    static bool FactoryReferencePolicy(string selectedMode, bool complete, string identity, string location, bool reflectionOnly)
    {
        if ((selectedMode != "original-factory" && selectedMode != "original-factory-bcl9" && selectedMode != "original-factory-empty-container" && selectedMode != "original-trace-encryptor" && selectedMode != "original-factory-registered-trace" && selectedMode != "original-project-genuine-factory" && selectedMode != "original-project-genuine-factory-resources" && selectedMode != "original-project-undo-provider-denied" && selectedMode != "original-project-undo-provider-factory" && selectedMode != "original-project-large-string-cache-denied" && selectedMode != "original-project-context-denied" && selectedMode != "original-project-context-xml-registration") || !complete || reflectionOnly) return false;
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
        // Always override general admission for this exact candidate name, even denied/dynamic loads.
        if (String.Equals(assembly.GetName().Name, "System.ValueTuple", StringComparison.OrdinalIgnoreCase)) {
            string location = assembly.IsDynamic ? "UNAVAILABLE_DYNAMIC" : assembly.Location;
            bool cache = !assembly.IsDynamic && assembly.GlobalAssemblyCache;
            bool selected = !assembly.IsDynamic && ValueTupleReferencePolicy(mode, valueTuplePinned, assembly.FullName, location, cache, assembly.ReflectionOnly)
                && ProbeFiles.Same(location, valueTuplePath);
            allowed = selected;
            Record("VALUETUPLE_REFERENCE_AUDIT allowed=" + selected + " identity=" + Text(assembly.FullName) + " location=" + Text(location)
                + " gac=" + cache + " reflection-only=" + assembly.ReflectionOnly + " pin-complete=" + valueTuplePinned
                + " origin=" + (selected ? "FIXED_OS_REFERENCE" : "UNQUALIFIED") + " held-pin-sha256=" + (selected ? ValueTupleHash : "NOT_ESTABLISHED"));
        }
        if (String.Equals(assembly.GetName().Name, XmlRegistrationName, StringComparison.OrdinalIgnoreCase)) {
            bool selected = XmlRegistrationReferencePolicy(mode, xmlRegistrationReady && containerInputsReady, assembly.FullName,
                assembly.IsDynamic ? "UNAVAILABLE_DYNAMIC" : assembly.Location, !assembly.IsDynamic && assembly.GlobalAssemblyCache, assembly.ReflectionOnly, assembly.IsDynamic, stage);
            allowed = selected;
            if (!selected) unexpected = true;
            Record("XML_REGISTRATION_AUDIT allowed=" + selected + " identity=" + Text(assembly.FullName) + " held-pin-sha256=" + (selected ? XmlRegistrationHash : "NOT_ESTABLISHED") + " private-origin-only=True detection=after-load");
        }
        Record("PROJECT_ASSEMBLY allowed=" + allowed + " reflection-only=" + assembly.ReflectionOnly + " identity=" + Text(assembly.FullName));
        if (!allowed) { unexpected = true; Record("UNEXPECTED_ASSEMBLY_BLOCKER detection=after-load prevention=NOT_ESTABLISHED"); }
    }
    // Finite private policy seam. Production facts come from the held profile/actual Assembly/phase.
    static bool AutofacResourcePolicy(string selectedMode, bool complete, bool active, bool ownerPinned, string requested, bool requesterNull, bool requesterMatches, int count)
    {
        return (selectedMode == "original-factory-empty-container" || selectedMode == "original-factory-registered-trace" || selectedMode == "original-project-genuine-factory") && complete && active && ownerPinned
            && (requested == AutofacResourceEnGb || requested == AutofacResourceEn) && (requesterNull ? !requesterMatches : requesterMatches)
            && count >= 1 && count <= 8;
    }
    static int NextResourceProbe(int count)
    { ProbeFiles.Require(count >= 0 && count <= 9, "AUTOFAC_RESOURCE_COUNTER"); return count < 9 ? count + 1 : 9; }
    static bool AutofacResourceOwnerPolicy(string identity, string location, bool reflectionOnly, bool dynamicAssembly)
    {
        return !dynamicAssembly && !reflectionOnly && identity == FactoryExtraIdentities[0]
            && ProbeFiles.Same(location, Path.Combine(stage, FactoryExtraNames[0] + ".dll"));
    }
    static bool AllowAutofacResourceNull(ResolveEventArgs args)
    {
        if (args.Name != AutofacResourceEnGb && args.Name != AutofacResourceEn) return false;
        Assembly owner = resourceOwner, requester = args.RequestingAssembly;
        bool pinned = owner != null && !owner.IsDynamic && AutofacResourceOwnerPolicy(owner.FullName, owner.Location, owner.ReflectionOnly, owner.IsDynamic);
        bool requesterNull = requester == null, matches = pinned && Object.ReferenceEquals(owner, requester);
        if (ProjectResources) return AllowProjectResourceNull(args.Name, pinned, requesterNull, matches, requester);
        if ((mode == "original-factory-empty-container" || RegisteredOriginal) && containerInputsReady && resourcePhaseActive && pinned)
            resourceProbeCount = NextResourceProbe(resourceProbeCount);
        bool allowed = AutofacResourcePolicy(mode, containerInputsReady, resourcePhaseActive, pinned, args.Name, requesterNull, matches, resourceProbeCount);
        Record("AUTOFAC_RESOURCE_PROBE allowed=" + allowed + " requested=" + Text(args.Name)
            + " requester=" + (requesterNull ? "NULL_UNKNOWN" : Text(requester.FullName))
            + " requester-is-pinned-owner=" + matches + " owner-proof=" + (requesterNull ? "NOT_ESTABLISHED" : matches ? "PINNED_AUTOFAC" : "FOREIGN")
            + " profile-complete=" + containerInputsReady + " invocation-thread-phase=" + resourcePhaseActive + " probe=" + resourceProbeCount
            + " action=" + (allowed ? "RETURN_NULL_NO_ASSEMBLY" : "CONTINUE_FATAL_DENY"));
        return allowed;
    }
    static void WithFactoryResourcePhase(Action invoke)
    {
        ProbeFiles.Require(mode == "original-factory-empty-container" || mode == "owned-factory-empty-container" || RegisteredTrace, "AUTOFAC_RESOURCE_PHASE_MODE");
        ProbeFiles.Require(!resourcePhaseActive, "AUTOFAC_RESOURCE_NESTED_PHASE");
        resourceProbeCount = 0; resourcePhaseActive = true;
        try { invoke(); }
        finally {
            int count = resourceProbeCount;
            resourcePhaseActive = false; resourceProbeCount = 0;
            Record("AUTOFAC_RESOURCE_PHASE_END active=False probes=" + count + " resource-policy=NULL_ONLY");
        }
    }
    // Separate new-mode budget: old Factory-only reset behavior above is unchanged.
    static bool ProjectResourcePolicy(string selectedMode, bool complete, bool budget, int window, bool suspended, bool pinned, string request, bool requesterNull, bool matches, int count)
    {
        return (selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration") && complete && budget && (window == 1 || window == 2) && !suspended && pinned
            && (request == AutofacResourceEnGb || request == AutofacResourceEn) && (requesterNull ? !matches : matches) && count >= 1 && count <= 8;
    }
    static bool AllowProjectResourceNull(string request, bool pinned, bool requesterNull, bool matches, Assembly requester)
    {
        bool eligible = ProjectResourcePolicy(mode, containerInputsReady, projectResourceBudgetActive, projectResourceWindow, projectResourceCallbackSuspended,
            pinned, request, requesterNull, matches, 1);
        bool allowed = ConsumeProjectResourceProbe(eligible);
        Record("PROJECT_AUTOFAC_RESOURCE_PROBE allowed=" + allowed + " requested=" + Text(request)
            + " requester=" + (requesterNull ? "NULL_UNKNOWN" : Text(requester.FullName))
            + " requester-is-pinned-owner=" + matches + " owner-proof=" + (requesterNull ? "NOT_ESTABLISHED" : matches ? "PINNED_AUTOFAC" : "FOREIGN")
            + " profile-complete=" + containerInputsReady + " invocation-thread-window=" + projectResourceWindow + " budget-active=" + projectResourceBudgetActive
            + " callback-suspended=" + projectResourceCallbackSuspended + " aggregate-probe=" + projectResourceProbeCount
            + " action=" + (allowed ? "RETURN_NULL_NO_ASSEMBLY" : "CONTINUE_FATAL_DENY"));
        return allowed;
    }
    static bool ConsumeProjectResourceProbe(bool eligible)
    {
        if (eligible) projectResourceProbeCount = NextResourceProbe(projectResourceProbeCount);
        return eligible && projectResourceProbeCount <= 8;
    }
    static void ProjectResourceRequire(bool condition, string code)
    { if (!condition) { rejected = true; throw new IOException(code); } }
    static void WithProjectResourceBudget(Action invoke)
    {
        ProjectResourceRequire(ProjectResources && !projectResourceBudgetActive && projectResourceWindow == 0 && !projectResourceCallbackSuspended, "PROJECT_RESOURCE_BUDGET_CONTEXT");
        projectResourceProbeCount = 0; projectResourceBudgetActive = true;
        try { invoke(); }
        finally {
            int count = projectResourceProbeCount;
            projectResourceWindow = 0; projectResourceCallbackSuspended = false; projectResourceBudgetActive = false; projectResourceProbeCount = 0;
            Record("PROJECT_RESOURCE_BUDGET_END active=False aggregate-probes=" + count + " maximum=8 resource-policy=NULL_ONLY");
        }
    }
    static void WithProjectResourceWindow(int window, Action invoke)
    {
        ProjectResourceRequire(ProjectResources && projectResourceBudgetActive && projectResourceWindow == 0 && !projectResourceCallbackSuspended
            && (window == 1 || window == 2), "PROJECT_RESOURCE_WINDOW_CONTEXT");
        projectResourceWindow = window;
        try { invoke(); }
        finally { projectResourceWindow = 0; }
    }
    internal static int SuspendProjectResourceCallback()
    {
        if (!ProjectResources) return 0;
        ProjectResourceRequire(!projectResourceCallbackSuspended, "PROJECT_RESOURCE_NESTED_CALLBACK");
        int previous = projectResourceWindow;
        projectResourceWindow = 0; projectResourceCallbackSuspended = true;
        return previous;
    }
    internal static void RestoreProjectResourceCallback(int previous)
    {
        if (!ProjectResources) return;
        ProjectResourceRequire(projectResourceCallbackSuspended && projectResourceWindow == 0 && previous >= 0 && previous <= 2
            && (previous == 0 || projectResourceBudgetActive), "PROJECT_RESOURCE_CALLBACK_RESTORE");
        projectResourceCallbackSuspended = false; projectResourceWindow = previous;
    }
    static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        Record("PROJECT_RESOLVE requested=" + Text(args.Name));
        if (AllowAutofacResourceNull(args)) return null;
        if (XmlRegistrationOriginal && args.Name == XmlRegistrationIdentity) {
            string policy = AppDomain.CurrentDomain.ApplyPolicy(args.Name);
            return ResolveXmlRegistration(args.Name, policy);
        }
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
    // Reflective shape checks precede every builder side effect.
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
    static void CheckBuilderDependencies(bool hasUnexpected, bool hasRejected)
    { ProbeFiles.Require(!hasUnexpected && !hasRejected, "EMPTY_CONTAINER_BUILDER_DEPENDENCY"); CheckUndoProvider("BUILDER"); CheckUndoFactory("BUILDER"); CheckCache("BUILDER"); CheckContextArguments("BUILDER"); }
    static void WithEmptyContainer(EmptyContainerApi api, Action<object> use)
    { WithEmptyContainerCore(api, use, null); }
    // Additional guard control exists only for finite owned fixtures. Actual latches are never bypassed/reset.
    static void WithEmptyContainerCore(EmptyContainerApi api, Action<object> use, Action ownedBuilderGuard)
    { WithEmptyContainerCore(api, use, ownedBuilderGuard, null); }
    static void WithEmptyContainerCore(EmptyContainerApi api, Action<object> use, Action ownedBuilderGuard, Action<object> register)
    {
        ProbeFiles.Require(register == null || RegisteredTrace, "REGISTERED_CONTAINER_MODE");
        ProbeFiles.Require(ownedBuilderGuard == null || mode == "owned-factory-empty-container", "OWNED_BUILDER_GUARD_MODE");
        object container = null, builder = null;
        try {
            bool created = false;
            Record("EMPTY_CONTAINER_BUILDER_BEGIN type=" + api.Builder.FullName + " token=" + api.Constructor.MetadataToken.ToString("X8") + " token-source=OBSERVED");
            try { builder = api.Constructor.Invoke(new object[0]); created = true; }
            finally { Record("EMPTY_CONTAINER_BUILDER_END returned=" + created); }
            ProbeFiles.Require(builder != null && builder.GetType() == api.Builder, "EMPTY_CONTAINER_EXACT_BUILDER");
            CheckBuilderDependencies(unexpected, rejected);
            if (ownedBuilderGuard != null) ownedBuilderGuard();
            if (register != null) { register(builder); CheckBuilderDependencies(unexpected, rejected); }
            bool built = false;
            Record("EMPTY_CONTAINER_BUILD_BEGIN token=" + api.Build.MetadataToken.ToString("X8") + " options=None raw=0 application-registrations=" + (register == null ? 0 : UndoDeniedMode ? 2 : 1) + " defaults=RETAINED");
            try { container = api.Build.Invoke(builder, new object[] { Enum.ToObject(api.Options, 0) }); built = true; }
            finally { Record("EMPTY_CONTAINER_BUILD_END returned=" + built + " nonnull=" + (container != null)); }
            EmptyContainerReturn(api, container); CheckUndoProvider("POST_BUILD"); CheckUndoFactory("POST_BUILD"); CheckCache("POST_BUILD"); CheckContextArguments("POST_BUILD");
            ProbeFiles.Require(!unexpected && !rejected, "EMPTY_CONTAINER_BUILD_DEPENDENCY");
            Record("EMPTY_CONTAINER_READY genuine-context=true context-requests=NOT_INTERCEPTED application-registrations=" + (register == null ? 0 : UndoDeniedMode ? 2 : 1) + " defaults=RETAINED qualification=NOT_ESTABLISHED");
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
            + " services=" + (RegisteredTrace ? "genuine-registered-trace-container+deny-all-provider" : "genuine-empty-container+deny-all-provider") + " constructor-only=true context-requests=NOT_INTERCEPTED qualification=NOT_ESTABLISHED");
        try {
            ProbeFiles.Require(!unexpected && !rejected, "FACTORY_PRECONSTRUCTOR_DEPENDENCY");
            object[] arguments = new object[] { context, provider.GetTransparentProxy() };
            WithFactoryResourcePhase(delegate { constructed = ctor.Invoke(arguments); returned = true; });
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
        resourceOwner = autofac; // Previously verified fixed identity/path/non-reflection from the held nineteen-pin profile.
        WithFactoryContainer(api, ctor, denied);
    }
    static void OwnedAutofacResourcePolicy()
    {
        int positives = 0, negatives = 0;
        foreach (string request in new string[] { AutofacResourceEnGb, AutofacResourceEn }) {
            foreach (bool requesterNull in new bool[] { true, false }) {
                ProbeFiles.Require(AutofacResourcePolicy("original-factory-empty-container", true, true, true, request, requesterNull, !requesterNull, 1), "OWNED_RESOURCE_POSITIVE"); positives++;
            }
            foreach (string wrong in new string[] { request.Replace("Autofac.resources", "Owned.Wrong.resources"), request.Replace("8.2.0.0", "8.2.0.1"),
                request.Replace("17863af14b0044da", "17863af14b0044db"), request.Replace("Culture=en-GB", "Culture=en-US").Replace("Culture=en,", "Culture=neutral,"),
                request + ", Retargetable=Yes", request.Replace("Culture=en-GB", "Culture=en-gb").Replace("Culture=en,", "Culture=EN,") }) {
                ProbeFiles.Require(!AutofacResourcePolicy("original-factory-empty-container", true, true, true, wrong, true, false, 1), "OWNED_RESOURCE_IDENTITY_NEGATIVE"); negatives++;
            }
        }
        foreach (string other in new string[] { "original-project", "original-project-store-id", "original-factory", "original-factory-bcl9", "owned-factory-empty-container" }) {
            ProbeFiles.Require(!AutofacResourcePolicy(other, true, true, true, AutofacResourceEnGb, true, false, 1), "OWNED_RESOURCE_MODE_NEGATIVE"); negatives++;
        }
        foreach (bool[] facts in new bool[][] { new bool[] { false, true, true, true, false }, new bool[] { true, false, true, true, false },
            new bool[] { true, true, false, true, false }, new bool[] { true, true, true, false, false }, new bool[] { true, true, true, true, true } }) {
            ProbeFiles.Require(!AutofacResourcePolicy("original-factory-empty-container", facts[0], facts[1], facts[2], AutofacResourceEnGb, facts[3], facts[4], 1), "OWNED_RESOURCE_FACT_NEGATIVE"); negatives++;
        }
        string ownerPath = Path.Combine(stage, FactoryExtraNames[0] + ".dll");
        ProbeFiles.Require(AutofacResourceOwnerPolicy(FactoryExtraIdentities[0], ownerPath, false, false), "OWNED_RESOURCE_OWNER_POSITIVE");
        foreach (bool denied in new bool[] { !AutofacResourceOwnerPolicy("Owned.WrongIdentity", ownerPath, false, false),
            !AutofacResourceOwnerPolicy(FactoryExtraIdentities[0], fixture, false, false), !AutofacResourceOwnerPolicy(FactoryExtraIdentities[0], ownerPath, true, false),
            !AutofacResourceOwnerPolicy(FactoryExtraIdentities[0], ownerPath, false, true) }) {
            ProbeFiles.Require(denied, "OWNED_RESOURCE_OWNER_NEGATIVE"); negatives++;
        }
        int count = 0;
        for (int i = 1; i <= 8; i++) { count = NextResourceProbe(count); ProbeFiles.Require(AutofacResourcePolicy("original-factory-empty-container", true, true, true, AutofacResourceEn, true, false, count), "OWNED_RESOURCE_BOUND_POSITIVE"); }
        for (int i = 0; i < 3; i++) { count = NextResourceProbe(count); ProbeFiles.Require(count == 9 && !AutofacResourcePolicy("original-factory-empty-container", true, true, true, AutofacResourceEn, true, false, count), "OWNED_RESOURCE_BOUND_NEGATIVE"); }
        ProbeFiles.Require(!AutofacResourcePolicy("original-factory-empty-container", true, true, true, AutofacResourceEn, true, false, 0)
            && !AutofacResourcePolicy("original-factory-empty-container", true, true, true, AutofacResourceEn, true, false, -1), "OWNED_RESOURCE_ZERO_NEGATIVE");
        ProbeFiles.Require(!resourcePhaseActive && resourceProbeCount == 0, "OWNED_RESOURCE_INITIAL_PHASE");
        WithFactoryResourcePhase(delegate {
            ProbeFiles.Require(resourcePhaseActive && resourceProbeCount == 0 && !AllowAutofacResourceNull(new ResolveEventArgs(AutofacResourceEnGb)), "OWNED_RESOURCE_SCOPE_CANNOT_ADMIT");
            bool nestedDenied = false;
            try { WithFactoryResourcePhase(delegate { throw new InvalidOperationException("OWNED_NESTED_ACTION_EXECUTED"); }); }
            catch (IOException error) { nestedDenied = error.Message == "AUTOFAC_RESOURCE_NESTED_PHASE"; if (!nestedDenied) throw; }
            ProbeFiles.Require(nestedDenied && resourcePhaseActive, "OWNED_RESOURCE_NESTED_PHASE");
            bool otherActive = true; int otherCount = -1;
            var thread = new Thread(delegate() { otherActive = resourcePhaseActive; otherCount = resourceProbeCount; }); thread.IsBackground = true; thread.Start();
            ProbeFiles.Require(thread.Join(1000) && !otherActive && otherCount == 0, "OWNED_RESOURCE_THREAD_SCOPE");
        });
        ProbeFiles.Require(!resourcePhaseActive && resourceProbeCount == 0, "OWNED_RESOURCE_RETURN_CLEAR");
        bool threw = false;
        try { WithFactoryResourcePhase(delegate { throw new NotSupportedException("OWNED_RESOURCE_PHASE_THROW"); }); }
        catch (NotSupportedException error) { threw = error.Message == "OWNED_RESOURCE_PHASE_THROW"; if (!threw) throw; }
        ProbeFiles.Require(threw && !resourcePhaseActive && resourceProbeCount == 0 && !AllowAutofacResourceNull(new ResolveEventArgs(AutofacResourceEn)), "OWNED_RESOURCE_EXCEPTION_CLEAR");
        Record("OWNED_AUTOFAC_RESOURCE_POLICY_PASS positives=" + positives + " negatives=" + negatives + " bounded8=true null-requester=UNKNOWN policy-only=true original=NOT_RUN");
        Record("OWNED_AUTOFAC_RESOURCE_PHASE_PASS return-throw-cross-thread-clear=true owned-mode-cannot-admit=true no-target-load=true original=NOT_RUN");
    }
    static void OwnedEmptyContainer()
    {
        OwnedAutofacResourcePolicy();
        Assembly assembly = Assembly.LoadFrom(fixture);
        Func<string, Type> type = delegate(string name) { return assembly.GetType("Owned.Task009." + name, true, false); };
        Type builder = type("EmptyBuilderFixture"), options = type("EmptyOptionsFixture"), container = type("IEmptyContainerFixture"), context = type("IContainerDependencyFixture"), control = type("EmptyControlFixture");
        Action<string> reset = delegate(string behavior) { control.GetMethod("Reset").Invoke(null, new object[] { behavior }); };
        Func<string, int> count = delegate(string name) { return (int)control.GetField(name).GetValue(null); };
        EmptyContainerApi api = EmptyApi(builder, options, container, context);
        string observedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ValueTupleRelativePath);
        ProbeFiles.Require(!valueTuplePinned && !ValueTupleReferencePolicy("original-factory-empty-container", false, ValueTupleIdentity, observedPath, true, false)
            && ValueTupleReferencePolicy("original-factory-empty-container", true, ValueTupleIdentity, observedPath, true, false), "OWNED_VALUETUPLE_PIN_POLICY");
        foreach (string other in new string[] { "original-project", "original-project-store-id", "original-factory", "original-factory-bcl9", "owned-factory-empty-container" })
            ProbeFiles.Require(!ValueTupleReferencePolicy(other, true, ValueTupleIdentity, observedPath, true, false), "OWNED_VALUETUPLE_OTHER_MODE");
        ProbeFiles.Require(!ValueTupleReferencePolicy("original-factory-empty-container", true, "Owned.WrongIdentity", observedPath, true, false)
            && !ValueTupleReferencePolicy("original-factory-empty-container", true, ValueTupleIdentity, fixture, true, false)
            && !ValueTupleReferencePolicy("original-factory-empty-container", true, ValueTupleIdentity, observedPath, false, false)
            && !ValueTupleReferencePolicy("original-factory-empty-container", true, ValueTupleIdentity, observedPath, true, true), "OWNED_VALUETUPLE_FACT_POLICY");
        Record("OWNED_VALUETUPLE_REFERENCE_POLICY_PASS incomplete-pin-identity-path-cache-reflection-other-modes=denied original-os-reference-load=NOT_RUN historical-origin=UNKNOWN");
        foreach (string flag in new string[] { "unexpected", "rejected" }) {
            bool latchedUnexpected = false, latchedRejected = false; int used = 0;
            Action latch = delegate { if (flag == "unexpected") latchedUnexpected = true; else latchedRejected = true; };
            reset("normal"); control.GetField("BuilderCallback").SetValue(null, latch);
            // Model the old missing check with local owned latches, never actual production flags.
            WithEmptyContainer(api, delegate(object value) { used++; });
            ProbeFiles.Require((latchedUnexpected || latchedRejected) && count("Constructors") == 1 && count("Builds") == 1 && used == 1 && count("Factories") == 0 && count("Disposals") == 1,
                "OWNED_BUILDER_GUARD_BASELINE");
            Record("OWNED_BUILDER_GUARD_BASELINE modeled=true flag=" + flag + " build=1 use=1 factory=0 original=NOT_RUN");
            latchedUnexpected = latchedRejected = false; used = 0; reset("normal"); control.GetField("BuilderCallback").SetValue(null, latch);
            bool stopped = false;
            try { WithEmptyContainerCore(api, delegate(object value) { used++; }, delegate { CheckBuilderDependencies(latchedUnexpected, latchedRejected); }); }
            catch (IOException error) { stopped = error.Message == "EMPTY_CONTAINER_BUILDER_DEPENDENCY"; if (!stopped) throw; }
            ProbeFiles.Require(stopped && (latchedUnexpected || latchedRejected) && count("Constructors") == 1 && count("Builds") == 0 && used == 0 && count("Factories") == 0 && count("Disposals") == 0,
                "OWNED_BUILDER_GUARD_STOP");
            Record("OWNED_BUILDER_GUARD_PASS flag=" + flag + " builder=1 build=0 use=0 factory=0 production-flags-never-reset=true original=NOT_RUN");
        }

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
    const string ReviewedTraceParameterReportSha256 = "07accca76a4ab722a7d6943578ab955659261cada7b7cc72feee00f730462bc2";
    const int TraceDeclaredDefault = 1; // Actual associated Int32 metadata default; not a guessed fixture value.
    sealed class TraceLifetimeApi
    {
        internal Type Type, Contract;
        internal ConstructorInfo Constructor;
        internal ParameterInfo Parameter;
        internal object Default;
    }
    static TraceLifetimeApi TraceApi(Type type, Type contract)
    {
        ProbeFiles.Require(type != null && type.IsPublic && type.IsClass && type.IsSealed && !type.IsAbstract && !type.IsGenericType && !type.ContainsGenericParameters, "TRACE_TYPE_SHAPE");
        ProbeFiles.Require(PublicInterface(contract) && typeof(IDisposable).IsAssignableFrom(contract) && contract.IsAssignableFrom(type)
            && type.Assembly == contract.Assembly && !type.Assembly.ReflectionOnly, "TRACE_CONTRACT_OWNERSHIP");
        ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, new Type[] { typeof(int) }, null);
        ProbeFiles.Require(ctor != null && ctor.DeclaringType == type && (int)ctor.Attributes == 6278 && !ctor.IsStatic && !ctor.ContainsGenericParameters
            && ctor.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "TRACE_PUBLIC_CONSTRUCTOR");
        ParameterInfo[] parameters = ctor.GetParameters();
        ProbeFiles.Require(parameters.Length == 1 && parameters[0].ParameterType == typeof(int) && parameters[0].Position == 0
            && parameters[0].Name == "iterations" && (int)parameters[0].Attributes == 4112 && parameters[0].IsOptional && parameters[0].HasDefaultValue, "TRACE_PARAMETER_CONTRACT");
        object value = parameters[0].RawDefaultValue;
        ProbeFiles.Require(value != null && value.GetType() == typeof(int) && (int)value == TraceDeclaredDefault, "TRACE_DECLARED_DEFAULT");
        return new TraceLifetimeApi { Type = type, Contract = contract, Constructor = ctor, Parameter = parameters[0], Default = value };
    }
    static bool ReviewedTraceTokens(int type, int ctor, int parameter, int contract)
    { return type == 0x0200007e && ctor == 0x06000364 && parameter == 0x080001e4 && contract == 0x02000077; }
    static void TraceDependencyGuard(bool hasUnexpected, bool hasRejected, string phase)
    { ProbeFiles.Require(!hasUnexpected && !hasRejected, "TRACE_DEPENDENCY_" + phase); CheckUndoProvider(phase); CheckUndoFactory(phase); CheckCache(phase); CheckContextArguments(phase); }
    static void WithTraceLifetime(TraceLifetimeApi api) { WithTraceLifetimeCore(api, null); }
    // Finite owned refusal controls cannot reset latches or activate the original route.
    static void WithTraceLifetimeCore(TraceLifetimeApi api, Action<string> ownedGuard)
    { WithTraceLifetimeCore(api, ownedGuard, null); }
    static void WithTraceLifetimeCore(TraceLifetimeApi api, Action<string> ownedGuard, Action<object> use)
    {
        ProbeFiles.Require(use == null || RegisteredTrace, "TRACE_CALLBACK_MODE");
        ProbeFiles.Require(ownedGuard == null || TraceOwned || RegisteredOwned, "TRACE_OWNED_GUARD_MODE");
        Action<string> check = delegate(string phase) { TraceDependencyGuard(unexpected, rejected, phase); if (ownedGuard != null) ownedGuard(phase); };
        object instance = null; bool exact = false;
        try {
            check("PRE_CTOR"); bool returned = false;
            Record("TRACE_CONSTRUCTOR_BEGIN token=" + api.Constructor.MetadataToken.ToString("X8") + " default-origin=REVIEWED_METADATA raw-int32=1 header32=STATIC_PROVENANCE_ONLY qualification=NOT_ESTABLISHED");
            try {
                instance = api.Constructor.Invoke(new object[] { api.Default }); returned = true;
                exact = instance != null && instance.GetType() == api.Type && api.Contract.IsInstanceOfType(instance) && instance is IDisposable
                    && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(instance);
                ProbeFiles.Require(exact, "TRACE_EXACT_RETURN"); check("POST_CTOR");
            } catch (Exception error) { Errors(error); throw; }
            finally { Record("TRACE_CONSTRUCTOR_END returned=" + returned + " body-entry=" + (returned ? "NOT_INSTRUMENTED" : "NOT_ESTABLISHED")); }
            if (use != null) use(instance);
        } finally {
            try {
                if (exact) {
                    bool disposed = false;
                    try { Record("TRACE_DISPOSE_BEGIN attempts=1 hard-termination-disposal=NOT_ESTABLISHED"); }
                    finally {
                        try { ((IDisposable)instance).Dispose(); disposed = true; }
                        catch (Exception error) { Errors(error); throw; }
                        finally { Record("TRACE_DISPOSE_END returned=" + disposed + " attempts=1"); }
                    }
                }
            } finally {
                bool auditPassed = false;
                try { check("POST_DISPOSE"); auditPassed = true; }
                finally { Record("TRACE_LIFETIME_AUDIT passed=" + auditPassed + " after-managed-disposal=true explicit-dispose-attempted=" + exact); GC.KeepAlive(instance); }
            }
        }
        if (use == null) Record("TRACE_CONSTRUCTOR_DISPOSE_RETURNED_NOT_QUALIFIED explicit-other-sdk-calls=0 network-descendants=NOT_CONTAINED");
        else Record("REGISTERED_TRACE_LIFETIME_RETURNED_NOT_QUALIFIED vendor-crypto-use=NOT_INTERCEPTED network-descendants=NOT_CONTAINED");
    }
    sealed class TraceRegistrationApi
    {
        internal MethodInfo Register, External;
        internal Type Return, Contract;
    }
    static TraceRegistrationApi RegistrationApi(EmptyContainerApi container, Type extension, Type open, Type data, Type style, Type contract)
    {
        Assembly owner = container.Builder.Assembly;
        ProbeFiles.Require(extension != null && extension.IsPublic && extension.IsAbstract && extension.IsSealed && !extension.IsGenericType
            && open != null && open.IsPublic && open.IsInterface && open.IsGenericTypeDefinition && open.GetGenericArguments().Length == 3
            && data != null && data.IsPublic && !data.ContainsGenericParameters && style != null && style.IsPublic && !style.ContainsGenericParameters
            && extension.Assembly == owner && open.Assembly == owner && data.Assembly == owner && style.Assembly == owner && !owner.ReflectionOnly
            && PublicInterface(contract), "TRACE_REGISTRATION_TYPES");
        MemberInfo[] matches = extension.GetMember("RegisterInstance", MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(matches.Length == 1, "TRACE_REGISTRATION_OVERLOAD");
        MethodInfo method = (MethodInfo)matches[0]; Type[] generic = method.GetGenericArguments(); ParameterInfo[] parameters = method.GetParameters();
        ProbeFiles.Require(method.IsPublic && method.IsStatic && method.DeclaringType == extension && method.IsGenericMethodDefinition && generic.Length == 1
            && generic[0].GenericParameterAttributes == GenericParameterAttributes.ReferenceTypeConstraint && generic[0].GetGenericParameterConstraints().Length == 0
            && parameters.Length == 2 && parameters[0].ParameterType == container.Builder && parameters[1].ParameterType == generic[0]
            && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == open
            && (method.CallingConvention & CallingConventions.VarArgs) == 0, "TRACE_REGISTRATION_SIGNATURE");
        Type[] result = method.ReturnType.GetGenericArguments();
        ProbeFiles.Require(result[0] == generic[0] && result[1] == data && result[2] == style, "TRACE_REGISTRATION_RETURN");
        Type closed = open.MakeGenericType(contract, data, style);
        MethodInfo external = closed.GetMethod("ExternallyOwned", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        ProbeFiles.Require(external != null && external.DeclaringType == closed && external.IsPublic && !external.IsStatic && !external.IsGenericMethod
            && external.ReturnType == closed && external.GetParameters().Length == 0 && (external.CallingConvention & CallingConventions.VarArgs) == 0,
            "TRACE_EXTERNAL_PUBLIC_INTERFACE");
        MethodInfo register = method.MakeGenericMethod(contract);
        ProbeFiles.Require(register.ReturnType == closed && register.GetParameters()[1].ParameterType == contract && !register.ContainsGenericParameters, "TRACE_CLOSED_REGISTRATION");
        return new TraceRegistrationApi { Register = register, External = external, Return = closed, Contract = contract };
    }
    static void RegisterTrace(TraceRegistrationApi api, object builder, object trace)
    {
        ProbeFiles.Require(RegisteredTrace && trace != null && api.Contract.IsInstanceOfType(trace), "TRACE_REGISTRATION_INSTANCE");
        TraceDependencyGuard(unexpected, rejected, "PRE_REGISTER");
        Record("TRACE_REGISTER_BEGIN service=" + api.Contract.FullName + " token=" + api.Register.MetadataToken.ToString("X8") + " token-source=OBSERVED application-registrations=1");
        object registration = api.Register.Invoke(null, new object[] { builder, trace });
        ProbeFiles.Require(registration != null && api.Return.IsInstanceOfType(registration) && registration.GetType().Assembly == api.Return.Assembly
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration), "TRACE_REGISTRATION_EXACT_RETURN");
        TraceDependencyGuard(unexpected, rejected, "POST_REGISTER");
        object external = api.External.Invoke(registration, new object[0]);
        ProbeFiles.Require(Object.ReferenceEquals(registration, external), "TRACE_EXTERNAL_SAME_BUILDER");
        TraceDependencyGuard(unexpected, rejected, "POST_EXTERNAL");
        Record("TRACE_REGISTER_END external-ownership=True exact-interface-default-service=True before-build=True method-owner=PUBLIC_INTERFACE vendor-service-calls=NOT_INTERCEPTED");
    }
    static void WithRegisteredTrace(TraceLifetimeApi trace, EmptyContainerApi container, TraceRegistrationApi registration, ConstructorInfo factory, ProjectDenyProxy provider)
    { WithRegisteredTrace(trace, container, registration, factory, provider, null, null); }
    static void WithRegisteredTrace(TraceLifetimeApi trace, EmptyContainerApi container, TraceRegistrationApi registration, ConstructorInfo factory, ProjectDenyProxy provider, Action<object> projectUse, ProjectStoreIdProxy store)
    {
        ProbeFiles.Require((projectUse == null && store == null) || (ProjectScope && projectUse != null && store != null), "PROJECT_SCOPED_CALLBACK_MODE");
        bool returned = false; object scopedFactory = null;
        try {
            WithTraceLifetimeCore(trace, null, delegate(object instance) {
                WithEmptyContainerCore(container, delegate(object context) {
                    if (projectUse == null) InvokeFactoryWithContainer(factory, context, provider);
                    else InvokeFactoryForProject(factory, context, provider, delegate(object actual) { scopedFactory = actual; projectUse(actual); });
                }, null,
                    delegate(object builder) { RegisterTrace(registration, builder, instance); });
            });
            returned = true;
        } finally {
            GC.KeepAlive(scopedFactory); // Factory remains live until Project/container/trace cleanup has finished.
            try { TraceDependencyGuard(unexpected, rejected, "REGISTERED_FINAL"); }
            finally {
                int denied = provider.Requests;
                Record("REGISTERED_TRACE_FINAL_AUDIT after-container-and-trace-disposal=True scope-returned=" + returned + " provider-denied=" + denied + " qualification=NOT_ESTABLISHED");
                if (store != null) {
                    Record("PROJECT_FACTORY_FINAL_AUDIT after-all-managed-disposal=True provider-denied=" + denied + " store-denied=" + store.DeniedRequests + " fixture-calls=" + store.FixtureCalls + " qualification=NOT_ESTABLISHED");
                    if (denied != 0 || store.DeniedRequests != 0) throw new NotSupportedException("PROJECT_FACTORY_LIFETIME_DENIAL");
                }
                if (denied != 0) throw new NotSupportedException("REGISTERED_TRACE_PROVIDER_LIFETIME_DENIAL");
            }
        }
    }
    static void OriginalRegisteredTrace()
    {
        ProbeFiles.Require(RegisteredOriginal && containerInputsReady, "REGISTERED_TRACE_COMPLETE_PROFILE");
        Assembly common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.dll"));
        Assembly autofac = Assembly.LoadFrom(Path.Combine(stage, "Autofac.dll"));
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll")), kim = Assembly.LoadFrom(Path.Combine(stage, "Knx.Kim.DataProvider.dll"));
        ProbeFiles.Require(common.FullName == Identities[10] && !common.ReflectionOnly && ProbeFiles.Same(common.Location, Path.Combine(stage, "Knx.Ets.Common.dll"))
            && autofac.FullName == FactoryExtraIdentities[0] && !autofac.ReflectionOnly && ProbeFiles.Same(autofac.Location, Path.Combine(stage, "Autofac.dll")), "REGISTERED_TRACE_PINNED_ASSEMBLIES");
        Type type = common.GetType("Knx.Ets.Common.Security.ProjectTraceEncryptor", true, false), contract = common.GetType("Knx.Ets.Common.Security.IProjectTraceEncryptor", true, false);
        TraceLifetimeApi trace = TraceApi(type, contract);
        ProbeFiles.Require(type.Assembly == common && contract.Assembly == common && (int)type.Attributes == 1048833
            && ReviewedTraceTokens(type.MetadataToken, trace.Constructor.MetadataToken, trace.Parameter.MetadataToken, contract.MetadataToken), "TRACE_ORIGINAL_TOKENS");
        Type context = autofac.GetType("Autofac.IComponentContext", true, false), provider = kim.GetType("Knx.Kim.DataProvider.IKimDataProvider", true, false), factory = om.GetType("Knx.Ets.ObjectModel.Factory", true, false);
        ProbeFiles.Require(context.Assembly == autofac && provider.Assembly == kim && factory.Assembly == om, "FACTORY_TYPE_OWNERSHIP");
        ConstructorInfo ctor = FactoryCtor(factory, context, provider); OriginalFactoryTokens(factory, ctor, context, provider);
        EmptyContainerApi container = EmptyApi(autofac.GetType("Autofac.ContainerBuilder", true), autofac.GetType("Autofac.Builder.ContainerBuildOptions", true), autofac.GetType("Autofac.IContainer", true), context);
        TraceRegistrationApi registration = RegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true),
            autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), contract);
        Record("TRACE_REGISTRATION_API_READY token-source=OBSERVED register-token=" + registration.Register.MetadataToken.ToString("X8") + " external-token=" + registration.External.MetadataToken.ToString("X8")
            + " upstream-commit=07cac9047f4059de2d72834042c9649cbd228085 api-observation-sha256=2f9fcfa159a07367b3e7414ded3470416729bbcbd02ffa0fd21ad9cf9fb9124f default-report-sha256=" + ReviewedTraceParameterReportSha256);
        resourceOwner = autofac;
        WithRegisteredTrace(trace, container, registration, ctor, new ProjectDenyProxy(provider, Record));
    }
    static void OwnedRegisteredTrace()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true, false); };
        Type traceControl = type("TraceLifetimeControl"), emptyControl = type("EmptyControlFixture"), registrationControl = type("RegistrationControl"), contract = type("ITraceLifetimeFixture");
        Action<string> reset = delegate(string behavior) {
            traceControl.GetMethod("Reset").Invoke(null, new object[] { behavior == "trace-ctor-throw" ? "ctor-throw" : behavior == "trace-dispose-throw" ? "dispose-throw" : behavior == "trace-denial-caught" ? "dispose-catch" : "return" });
            emptyControl.GetMethod("Reset").Invoke(null, new object[] { "return" }); registrationControl.GetMethod("Reset").Invoke(null, new object[] { behavior });
        };
        Func<Type, string, int> count = delegate(Type owner, string name) { return (int)owner.GetField(name).GetValue(null); };
        Action<string> step = delegate(string name) { registrationControl.GetMethod("Event").Invoke(null, new object[] { name }); };
        EmptyContainerApi container = EmptyApi(type("RegisteredBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        Type extension = type("RegistrationExtensionsFixture"), open = type("IRegistrationFixture`3"), data = type("RegistrationDataFixture"), style = type("RegistrationStyleFixture");
        reset("return"); int negatives = 0;
        foreach (string name in new string[] { "RegistrationNoConstraintFixture", "RegistrationWrongParameterFixture", "RegistrationWrongReturnFixture", "RegistrationWrongArgumentsFixture", "RegistrationInstanceMethodFixture" }) {
            bool denied = false; try { RegistrationApi(container, type(name), open, data, style, contract); } catch (IOException) { denied = true; }
            ProbeFiles.Require(denied, "OWNED_REGISTRATION_SHAPE_NEGATIVE"); negatives++;
        }
        foreach (Action wrong in new Action[] {
            delegate { RegistrationApi(container, type("RegistrationWrongExternalFixture"), type("IWrongExternalFixture`3"), data, style, contract); },
            delegate { RegistrationApi(container, extension, open, style, data, contract); },
            delegate { RegistrationApi(container, extension, open, data, style, typeof(string)); },
            delegate { RegistrationApi(container, typeof(string), open, data, style, contract); }
        }) { bool denied = false; try { wrong(); } catch (IOException) { denied = true; } ProbeFiles.Require(denied, "OWNED_REGISTRATION_CONTRACT_NEGATIVE"); negatives++; }
        ProbeFiles.Require(count(traceControl, "Constructors") == 0 && count(emptyControl, "Constructors") == 0 && count(registrationControl, "Registrations") == 0, "OWNED_REGISTRATION_BEFORE_EFFECTS");
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), contract);
        TraceRegistrationApi registration = RegistrationApi(container, extension, open, data, style, contract);
        ConstructorInfo factory = FactoryCtor(type("RegisteredFactoryFixture"), container.Context, type("IDataDependencyFixture"));
        ProbeFiles.Require(registration.External.DeclaringType.IsPublic && registration.External.DeclaringType.IsInterface
            && !type("RegistrationFixture`3").IsPublic && registration.Register.GetGenericArguments()[0] == contract, "OWNED_PUBLIC_INTERFACE_INTERNAL_IMPLEMENTATION");
        Record("OWNED_REGISTERED_TRACE_API_PASS negatives=" + negatives + " before-effects=True public-interface-internal-implementation=True exact-T-default-mapping=True original=NOT_RUN");
        foreach (string request in new string[] { AutofacResourceEnGb, AutofacResourceEn }) foreach (bool missingOwner in new bool[] { true, false })
            ProbeFiles.Require(AutofacResourcePolicy("original-factory-registered-trace", true, true, true, request, missingOwner, !missingOwner, 8)
                && !AutofacResourcePolicy("original-factory-registered-trace", true, true, true, request, missingOwner, !missingOwner, 9)
                && !AutofacResourcePolicy("owned-factory-registered-trace", true, true, true, request, missingOwner, !missingOwner, 1)
                && !AutofacResourcePolicy("original-factory-registered-trace", true, false, true, request, missingOwner, !missingOwner, 1)
                && !AutofacResourcePolicy("original-factory-registered-trace", false, true, true, request, missingOwner, !missingOwner, 1)
                && !AutofacResourcePolicy("original-factory-registered-trace", true, true, false, request, missingOwner, !missingOwner, 1), "OWNED_REGISTERED_RESOURCE_SCOPE");
        for (int i = 0; i < ContainerNames.Length; i++) {
            string path = Path.Combine(stage, ContainerNames[i] + ".dll");
            ProbeFiles.Require(ContainerReferencePolicy("original-factory-registered-trace", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("owned-factory-registered-trace", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-factory-registered-trace", false, ContainerIdentities[i], path, false), "OWNED_REGISTERED_COMPLETE_POLICY");
        }
        for (int i = 0; i < FactoryExtraNames.Length; i++) {
            string path = Path.Combine(stage, FactoryExtraNames[i] + ".dll");
            ProbeFiles.Require(FactoryReferencePolicy("original-factory-registered-trace", true, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("owned-factory-registered-trace", true, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-factory-registered-trace", false, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-factory-registered-trace", true, FactoryExtraIdentities[i], fixture, false)
                && !FactoryReferencePolicy("original-factory-registered-trace", true, FactoryExtraIdentities[i], path, true), "OWNED_REGISTERED_FACTORY_POLICY");
        }
        string reference = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ValueTupleRelativePath);
        ProbeFiles.Require(ValueTupleReferencePolicy("original-factory-registered-trace", true, ValueTupleIdentity, reference, true, false)
            && !ValueTupleReferencePolicy("owned-factory-registered-trace", true, ValueTupleIdentity, reference, true, false)
            && !ValueTupleReferencePolicy("original-factory-registered-trace", false, ValueTupleIdentity, reference, true, false)
            && !ValueTupleReferencePolicy("original-factory-registered-trace", true, ValueTupleIdentity, fixture, true, false)
            && !ValueTupleReferencePolicy("original-factory-registered-trace", true, ValueTupleIdentity, reference, false, false)
            && !ValueTupleReferencePolicy("original-factory-registered-trace", true, ValueTupleIdentity, reference, true, true), "OWNED_REGISTERED_OS_POLICY");
        string[] behaviors = { "return", "trace-ctor-throw", "register-throw", "register-null", "external-throw", "external-null", "build-throw", "factory-throw", "container-dispose-throw", "trace-dispose-throw", "provider-caught", "container-denial-caught", "trace-denial-caught" };
        foreach (string behavior in behaviors) {
            reset(behavior); var provider = new ProjectDenyProxy(type("IDataDependencyFixture"), Record);
            traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { step("trace-ctor"); }));
            traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { step("trace-dispose"); }));
            // Owned assertion invokes/catches a real deny-all proxy call; the production final audit must reject its count.
            if (behavior == "trace-denial-caught") traceControl.GetField("DisposeCallback").SetValue(null,
                new Action(delegate { step("trace-dispose"); owned.GetType("Owned.Task009.RegistrationAssertions", true).GetMethod("DenyProvider").Invoke(null, new object[] { provider.GetTransparentProxy() }); }));
            bool failed = false;
            try { WithRegisteredTrace(trace, container, registration, factory, provider); } catch (Exception error) {
                ProbeFiles.Require(error is IOException || error is NotSupportedException || error is TargetInvocationException || error is InvalidOperationException, "OWNED_REGISTERED_EXPECTED_ERROR"); failed = true;
            }
            bool traceReturned = behavior != "trace-ctor-throw";
            bool built = behavior == "return" || behavior == "factory-throw" || behavior == "container-dispose-throw" || behavior == "trace-dispose-throw" || behavior == "provider-caught" || behavior == "container-denial-caught" || behavior == "trace-denial-caught";
            ProbeFiles.Require(failed == (behavior != "return") && count(traceControl, "Constructors") == 1 && count(traceControl, "Disposals") == (traceReturned ? 1 : 0)
                && count(emptyControl, "Disposals") == (built ? 1 : 0) && count(emptyControl, "Factories") == (built ? 1 : 0)
                && count(traceControl, "Uses") == 0 && count(traceControl, "Attributes") == 0, "OWNED_REGISTERED_LIFETIME_COUNTS");
            bool externalAttempted = traceReturned && behavior != "register-throw" && behavior != "register-null";
            bool buildAttempted = externalAttempted && behavior != "external-throw" && behavior != "external-null";
            ProbeFiles.Require(count(registrationControl, "Registrations") == (traceReturned ? 1 : 0)
                && count(registrationControl, "ExternalCalls") == (externalAttempted ? 1 : 0) && count(emptyControl, "Builds") == (buildAttempted ? 1 : 0), "OWNED_REGISTERED_STAGE_COUNTS");
            string order = (string)registrationControl.GetField("Order").GetValue(null);
            if (built) ProbeFiles.Require(order.StartsWith("trace-ctor>builder>register>external>build>factory>container-dispose>trace-dispose>", StringComparison.Ordinal), "OWNED_REGISTERED_ORDER");
            ProbeFiles.Require(provider.Requests == (behavior == "provider-caught" || behavior == "container-denial-caught" || behavior == "trace-denial-caught" ? 1 : 0), "OWNED_REGISTERED_DENIAL_COUNT");
            Record("OWNED_REGISTERED_TRACE_CASE behavior=" + behavior + " expected-failure=" + failed + " explicit-trace-dispose=" + count(traceControl, "Disposals") + " container-dispose=" + count(emptyControl, "Disposals") + " provider-denied=" + provider.Requests);
        }
        Record("OWNED_REGISTERED_TRACE_PASS cases=13 external-before-build=True exact-interface=True same-live-instance=True disposal-order=container-then-trace caught-final-provider-denial=True no-crypto-harness-call=True original=NOT_RUN qualification=NOT_ESTABLISHED");
    }
    static void OriginalTraceEncryptor()
    {
        ProbeFiles.Require(TraceOriginal && containerInputsReady, "TRACE_COMPLETE_PROFILE_REQUIRED");
        Record("TRACE_PHASE original-type-materialization metadata-sha256=" + ReviewedTraceParameterReportSha256);
        string path = Path.Combine(stage, "Knx.Ets.Common.dll"); Assembly common = Assembly.LoadFrom(path);
        ProbeFiles.Require(common.FullName == Identities[10] && !common.ReflectionOnly && ProbeFiles.Same(common.Location, path), "TRACE_PINNED_ASSEMBLY");
        Type type = common.GetType("Knx.Ets.Common.Security.ProjectTraceEncryptor", true, false), contract = common.GetType("Knx.Ets.Common.Security.IProjectTraceEncryptor", true, false);
        TraceLifetimeApi api = TraceApi(type, contract);
        ProbeFiles.Require(type.Assembly == common && contract.Assembly == common && (int)type.Attributes == 1048833
            && ReviewedTraceTokens(type.MetadataToken, api.Constructor.MetadataToken, api.Parameter.MetadataToken, contract.MetadataToken), "TRACE_ORIGINAL_TOKENS");
        Record("TRACE_CONTRACT_READY original-default=true parameter-token=" + api.Parameter.MetadataToken.ToString("X8") + " default-constant-token=0B0000BF raw=01000000 source=STATIC_METADATA public-calling-convention=" + api.Constructor.CallingConvention);
        WithTraceLifetime(api);
    }
    static void OwnedTraceEncryptor()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true, false); };
        Type control = type("TraceLifetimeControl"), contract = type("ITraceLifetimeFixture");
        Action<string> reset = delegate(string behavior) { control.GetMethod("Reset").Invoke(null, new object[] { behavior }); };
        Func<string, int> count = delegate(string name) { return (int)control.GetField(name).GetValue(null); };
        for (int i = 0; i < FactoryExtraNames.Length; i++) {
            string path = Path.Combine(stage, FactoryExtraNames[i] + ".dll");
            ProbeFiles.Require(FactoryReferencePolicy("original-trace-encryptor", true, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-trace-encryptor", false, FactoryExtraIdentities[i], path, false)
                && !FactoryReferencePolicy("original-trace-encryptor", true, "Owned.WrongIdentity", path, false)
                && !FactoryReferencePolicy("original-trace-encryptor", true, FactoryExtraIdentities[i], fixture, false)
                && !FactoryReferencePolicy("original-trace-encryptor", true, FactoryExtraIdentities[i], path, true)
                && !FactoryReferencePolicy("owned-trace-encryptor", true, FactoryExtraIdentities[i], path, false), "OWNED_TRACE_EXTRA_REFERENCE_POLICY");
        }
        for (int i = 0; i < ContainerNames.Length; i++) {
            string path = Path.Combine(stage, ContainerNames[i] + ".dll");
            ProbeFiles.Require(ContainerReferencePolicy("original-trace-encryptor", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-trace-encryptor", false, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-trace-encryptor", true, ContainerIdentities[i], fixture, false)
                && !ContainerReferencePolicy("original-trace-encryptor", true, "Owned.WrongIdentity", path, false)
                && !ContainerReferencePolicy("original-trace-encryptor", true, ContainerIdentities[i], path, true)
                && !ContainerReferencePolicy("owned-trace-encryptor", true, ContainerIdentities[i], path, false), "OWNED_TRACE_CONTAINER_REFERENCE_POLICY");
        }
        string reference = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ValueTupleRelativePath);
        ProbeFiles.Require(ValueTupleReferencePolicy("original-trace-encryptor", true, ValueTupleIdentity, reference, true, false)
            && !ValueTupleReferencePolicy("original-trace-encryptor", false, ValueTupleIdentity, reference, true, false)
            && !ValueTupleReferencePolicy("original-trace-encryptor", true, "Owned.WrongIdentity", reference, true, false)
            && !ValueTupleReferencePolicy("original-trace-encryptor", true, ValueTupleIdentity, fixture, true, false)
            && !ValueTupleReferencePolicy("original-trace-encryptor", true, ValueTupleIdentity, reference, false, false)
            && !ValueTupleReferencePolicy("original-trace-encryptor", true, ValueTupleIdentity, reference, true, true)
            && !ValueTupleReferencePolicy("owned-trace-encryptor", true, ValueTupleIdentity, reference, true, false)
            && !AutofacResourcePolicy("original-trace-encryptor", true, true, true, AutofacResourceEnGb, true, false, 1), "OWNED_TRACE_OS_RESOURCE_POLICY");
        Record("OWNED_TRACE_REFERENCE_POLICY_PASS complete-exact-stage-only=true owned-mode=DENIED resources=DENIED policy-only=true");
        reset("return"); int negatives = 0;
        foreach (string name in new string[] { "TraceUnsealedFixture", "TraceAbstractFixture", "TraceGenericFixture`1", "TracePrivateConstructorFixture", "TraceWrongTypeFixture", "TraceWrongNameFixture", "TraceRequiredFixture", "TraceWrongDefaultFixture" }) {
            bool denied = false; try { TraceApi(type(name), contract); } catch (IOException) { denied = true; }
            ProbeFiles.Require(denied && count("Constructors") == 0 && count("Disposals") == 0 && count("Attributes") == 0, "OWNED_TRACE_SHAPE_BEFORE_EFFECT"); negatives++;
        }
        foreach (Type wrong in new Type[] { typeof(IDisposable), type("ITraceWrongContractFixture") }) {
            bool denied = false; try { TraceApi(type("TraceLifetimeFixture"), wrong); } catch (IOException) { denied = true; }
            ProbeFiles.Require(denied && count("Constructors") == 0, "OWNED_TRACE_CONTRACT_BEFORE_EFFECT"); negatives++;
        }
        ProbeFiles.Require(ReviewedTraceTokens(0x0200007e, 0x06000364, 0x080001e4, 0x02000077)
            && !ReviewedTraceTokens(0x0200007f, 0x06000364, 0x080001e4, 0x02000077) && !ReviewedTraceTokens(0x0200007e, 0x06000365, 0x080001e4, 0x02000077)
            && !ReviewedTraceTokens(0x0200007e, 0x06000364, 0x080001e5, 0x02000077) && !ReviewedTraceTokens(0x0200007e, 0x06000364, 0x080001e4, 0x02000078), "OWNED_TRACE_TOKEN_POLICY");
        TraceLifetimeApi api = TraceApi(type("TraceLifetimeFixture"), contract);
        ProbeFiles.Require(count("Attributes") == 0 && count("Constructors") == 0, "OWNED_TRACE_METADATA_NO_ATTRIBUTES");
        Record("OWNED_TRACE_CONTRACT_PASS negatives=" + negatives + " token-negatives=4 trap-attributes=0 default=OWNED_DECLARATION_ONLY before-effects=true");
        WithTraceLifetime(api); ProbeFiles.Require(count("Constructors") == 1 && count("Disposals") == 1 && count("Uses") == 0 && count("Attributes") == 0, "OWNED_TRACE_ONCE");
        foreach (string behavior in new string[] { "ctor-throw", "dispose-throw" }) {
            reset(behavior); bool denied = false;
            try { WithTraceLifetime(api); } catch (TargetInvocationException) { denied = behavior == "ctor-throw"; } catch (InvalidOperationException) { denied = behavior == "dispose-throw"; }
            ProbeFiles.Require(denied && count("Constructors") == 1 && count("Disposals") == (behavior == "ctor-throw" ? 0 : 1), "OWNED_TRACE_FAILURE_LIFETIME");
        }
        foreach (string flag in new string[] { "unexpected", "rejected" }) foreach (string phase in new string[] { "PRE_CTOR", "POST_CTOR", "POST_DISPOSE" }) {
            reset("return"); bool denied = false;
            try { WithTraceLifetimeCore(api, delegate(string point) { if (point == phase) TraceDependencyGuard(flag == "unexpected", flag == "rejected", point); }); }
            catch (IOException) { denied = true; }
            ProbeFiles.Require(denied && count("Constructors") == (phase == "PRE_CTOR" ? 0 : 1) && count("Disposals") == (phase == "PRE_CTOR" ? 0 : 1), "OWNED_TRACE_PHASE_GUARD");
        }
        foreach (string phase in new string[] { "ctor", "dispose" }) {
            reset(phase + "-catch"); bool latched = false, denied = false;
            Action callback = delegate { latched = true; throw new NotSupportedException("OWNED_TRACE_CAUGHT_AUDIT"); };
            control.GetField(phase == "ctor" ? "ConstructorCallback" : "DisposeCallback").SetValue(null, callback);
            try { WithTraceLifetimeCore(api, delegate(string point) { TraceDependencyGuard(false, latched, point); }); }
            catch (IOException) { denied = true; }
            ProbeFiles.Require(denied && count("Constructors") == 1 && count("Disposals") == 1 && count("Uses") == 0, "OWNED_TRACE_CAUGHT_DENIAL");
        }
        Record("OWNED_TRACE_LIFETIME_PASS ctor-dispose-once=true ctor-failure-no-return=true disposal-failure-fails=true phase-guards=6 caught-guards=2 production-latches-never-reset=true other-use=0 original=NOT_RUN");
    }

    // Source32 never selects the old new-project constructor.

    static readonly string[] OwnedContextCases = { "collection-parent", "collection-importer", "collection-throw", "project-context", "parent-inherited", "parent-generic", "parent-void", "parent-object", "context-object", "parent-null", "context-null", "parent-custom", "parent-mutable", "parent-throw", "parent-saturation", "context-saturation", "importer-saturation", "importer-null", "nested-parent", "nested-context", "nested-importer", "off-thread-parent", "off-thread-context", "off-thread-importer", "early-parent", "early-context", "early-importer", "late-parent", "late-context", "late-importer", "parent-recorder", "context-recorder", "importer-recorder", "parent-restore", "context-restore", "importer-restore", "project-dispose-parent", "container-dispose-context", "trace-dispose-importer", "error-context", "parent-count-only", "context-count-only", "importer-count-only", "parent-fault-only", "context-fault-only", "importer-fault-only", "process-final-parent", "parent-getter-reentry", "dispose-begin-log-fault" };
    static string ownedContextCase; static Assembly contextOwnedAssembly; static bool contextCaseFired, contextRecordEntered;
    static Type ContextOwnedType(string name) { return contextOwnedAssembly.GetType("Owned.Task009." + name, true); }
    static void ContextOwnedCall(string method, object[] arguments)
    { ContextOwnedType("ContextOwnedControl").GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Invoke(null, arguments); }
    static void ContextArgumentRecord(string message)
    {
        if (ContextOwned) {
            ProbeFiles.Require(projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_CONTEXT_CALLBACK_EXCLUDED");
            Record("OWNED_CONTEXT_CALLBACK_EXCLUDED window=0 suspended=True values=NOT_INSPECTED");
            string target = message.IndexOf("argument=parent", StringComparison.Ordinal) >= 0 ? "parent" : message.IndexOf("argument=context", StringComparison.Ordinal) >= 0 ? "context" : "importer";
            if (ownedContextCase == target + "-recorder") throw new IOException("OWNED_CONTEXT_RECORDER_FAULT");
            if (ownedContextCase == target + "-restore") projectResourceCallbackSuspended = false;
            if (ownedContextCase == "nested-" + target && !contextRecordEntered) {
                contextRecordEntered = true; int before = projectResourceProbeCount;
                ContextOwnedDeny(target, false);
                ProbeFiles.Require(projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == before, "OWNED_CONTEXT_NESTED_EXCLUSION");
                Record("OWNED_CONTEXT_NESTED_PASS outer-exclusion-preserved=True aggregate-unchanged=True");
            }
        }
        Record(message);
    }
    static void ContextOwnedDeny(string target, bool raw)
    {
        try {
            if (target == "importer") ContextOwnedCall("Importer", new object[] { contextImporter.Callback });
            else if (raw) { ContextArgumentDenyProxy proxy = target == "parent" ? contextParent : contextObject; proxy.Invoke(null); }
            else ContextOwnedCall(target == "parent" ? "ParentName" : "ContextProject", new object[0]);
        } catch (Exception error) { Record("OWNED_CONTEXT_CAUGHT type=" + error.GetType().FullName); }
    }
    static void OwnedContextPoint(string phase)
    {
        ProbeFiles.Require(ContextOwned, "OWNED_CONTEXT_POINT_MODE");
        if (phase == "collection") ProbeFiles.Require(projectResourceWindow == 0 && !projectResourceCallbackSuspended, "OWNED_CONTEXT_COLLECTION_WINDOW");
        if (phase == "project") ProbeFiles.Require(projectResourceWindow == 2 && !projectResourceCallbackSuspended, "OWNED_CONTEXT_PROJECT_WINDOW");
        if (ownedContextCase == null || contextCaseFired || ownedContextCase == "dispose-begin-log-fault") return;
        string selected = ownedContextCase; string target = selected.IndexOf("context", StringComparison.Ordinal) >= 0 ? "context" : selected.IndexOf("importer", StringComparison.Ordinal) >= 0 ? "importer" : "parent";
        string trigger = selected.StartsWith("early-", StringComparison.Ordinal) ? "trace-ctor" : selected.StartsWith("late-", StringComparison.Ordinal) || selected.EndsWith("-only", StringComparison.Ordinal) ? "late"
            : selected == "process-final-parent" ? "process-final" : (selected == "project-context" || selected == "parent-getter-reentry") ? "project" : selected == "project-dispose-parent" ? "project-dispose"
            : selected == "container-dispose-context" ? "container-dispose" : selected == "trace-dispose-importer" ? "trace-dispose" : selected == "error-context" ? "error" : "collection";
        if (phase != trigger || selected == "collection-throw") return;
        contextCaseFired = true;
        Record("OWNED_CONTEXT_REFUSAL case=" + selected + " phase=" + phase);
        if (selected.EndsWith("-only", StringComparison.Ordinal)) {
            ContextDenialState state = target == "parent" ? contextParent.State : target == "context" ? contextObject.State : contextImporter.State;
            string field = selected.EndsWith("-count-only", StringComparison.Ordinal) ? "entries" : "faulted";
            typeof(ContextDenialState).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(state, field == "entries" ? (object)1 : true);
            Record("OWNED_CONTEXT_AUDIT_MUTANT state=" + target + " count=" + state.Entries + " fault=" + state.Faulted + " other-states-clean=True"); return;
        }
        if (selected == "parent-getter-reentry") {
            Type messageType = ContextOwnedType("ContextReentryMessageFixture"); object message = Activator.CreateInstance(messageType);
            int window = projectResourceWindow, probes = projectResourceProbeCount;
            messageType.GetField("Callback").SetValue(message, new Action(delegate {
                ProbeFiles.Require(projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_CONTEXT_GETTER_EXCLUSION");
                contextParent.Invoke((IMessage)message);
            }));
            try { contextParent.Invoke((IMessage)message); } catch (NotSupportedException) { }
            ProbeFiles.Require((int)messageType.GetField("Reads").GetValue(message) == 16 && (int)messageType.GetField("Cutoffs").GetValue(message) == 1
                && !(bool)messageType.GetField("Sentinel").GetValue(message) && contextParent.State.Entries == 17 && contextParent.State.Faulted
                && projectResourceWindow == window && !projectResourceCallbackSuspended && projectResourceProbeCount == probes, "OWNED_CONTEXT_GETTER_HARD_CUTOFF");
            Record("OWNED_CONTEXT_GETTER_REENTRY_PASS reads=16 entries=17 hard-cutoff=1 sentinel=False outer-restored=True aggregate-unchanged=True"); return;
        }
        if (selected == "parent-inherited" || selected == "parent-generic" || selected == "parent-void" || selected == "parent-object" || selected == "context-object") {
            string member = selected == "parent-inherited" ? "ParentInherited" : selected == "parent-generic" ? "ParentGeneric" : selected == "parent-void" ? "ParentVoid" : selected == "parent-object" ? "ParentObject" : "ContextObject";
            ContextOwnedCall(member, new object[0]); return;
        }
        if (selected == "parent-custom" || selected == "parent-mutable" || selected == "parent-throw") {
            Type messageType = ContextOwnedType("CacheMessageFixture"); object message = Activator.CreateInstance(messageType);
            if (selected == "parent-custom") messageType.GetField("Method").SetValue(message, Activator.CreateInstance(ContextOwnedType("CacheCustomMethodFixture")));
            if (selected == "parent-throw") messageType.GetField("Failure").SetValue(message, "context");
            if (selected == "parent-mutable") messageType.GetField("SingleRead").SetValue(message, true);
            try { contextParent.Invoke((IMessage)message); } catch { }
            Record("OWNED_CONTEXT_MESSAGE_REFUSED values=NOT_INSPECTED"); return;
        }
        bool raw = selected.EndsWith("-null", StringComparison.Ordinal);
        if (selected.StartsWith("off-thread-", StringComparison.Ordinal)) { var thread = new Thread(delegate() { ContextOwnedDeny(target, raw); }); thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(2000), "OWNED_CONTEXT_THREAD_BOUND"); }
        else { int repeats = selected.EndsWith("-saturation", StringComparison.Ordinal) ? 20 : 1; for (int i = 0; i < repeats; i++) ContextOwnedDeny(target, raw); }
    }
    // Source34 owned-only diagnostics; each sticky case runs in a fresh child.
    static void OwnedStoreRequestDiagnostics()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type typed = type("StoreDiagnosticTypedFixture");
        Type storeType = type("IStoreRequestFixture"), streamType = type("ICacheStreamFixture"), idType = type("ScopedIdFixture"), messageType = type("StoreDiagnosticMessageFixture");
        MethodInfo idGetter = storeType.GetMethod("get_Id"), cacheGetter = storeType.GetMethod("get_LargeStringCache"), refused = storeType.GetMethod("get_Unprovided");
        object id = CreateFixtureId(idType, idType.GetConstructor(new Type[] { typeof(uint) }), idType.GetMethod("get_ProjectNumber"));
        var stream = new StreamStoreDenyProxy(streamType, StreamContract(streamType, false), Record);
        LargeStringCacheStoreProxy store = null; int diagnostics = 0; string diagnostic = null; bool reentered = false;
        Func<MethodInfo, object> message = delegate(MethodInfo method) { object m = Activator.CreateInstance(messageType); messageType.GetField("Method").SetValue(m, method); messageType.GetField("Name").SetValue(m, method == idGetter ? "get_Id" : "get_Unprovided"); return m; };
        Action<string> recorder = delegate(string line) {
            if (line.StartsWith("STORE_REQUEST_DIAGNOSTIC ", StringComparison.Ordinal)) {
                diagnostics++; diagnostic = line; ProbeFiles.Require(store.State.Faulted && UndoFactoryCallbackExcluded(), "OWNED_DIAGNOSTIC_BEFORE_DENIAL");
                if (ownedStoreDiagnosticCase == "recorder") throw new IOException("OWNED_DIAGNOSTIC_RECORDER");
                if (ownedStoreDiagnosticCase == "reentry" && !reentered) { reentered = true; try { store.Invoke((IMessage)message(refused)); } catch (Exception) { } }
            }
            Record(line);
        };
        store = new LargeStringCacheStoreProxy(storeType, idGetter, id, cacheGetter, streamType, stream.GetTransparentProxy(), recorder);
        WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate { WithProjectResourceWindow(2, delegate {
            string selected = ownedStoreDiagnosticCase;
            if (selected == "positive") {
                ProbeFiles.Require(Object.ReferenceEquals(typed.GetMethod("ReadId").Invoke(null, new object[] { store.GetTransparentProxy() }), id) && Object.ReferenceEquals(typed.GetMethod("ReadCache").Invoke(null, new object[] { store.GetTransparentProxy() }), stream.GetTransparentProxy()), "OWNED_DIAGNOSTIC_POSITIVE_REFERENCE");
                ProbeFiles.Require(!store.State.Faulted && store.State.Entries == 2 && store.State.Replies == 1 && diagnostics == 0, "OWNED_DIAGNOSTIC_POSITIVE_STATE");
            } else {
                object raw = message(refused);
                if (selected == "custom") messageType.GetField("Method").SetValue(raw, Activator.CreateInstance(type("CacheCustomMethodFixture")));
                if (selected == "foreign") messageType.GetField("Method").SetValue(raw, typeof(object).GetMethod("ToString", Type.EmptyTypes));
                if (selected == "forged") messageType.GetField("Failure").SetValue(raw, "name");
                if (selected == "method-throw") messageType.GetField("Failure").SetValue(raw, "method");
                if (selected == "args") { messageType.GetField("Method").SetValue(raw, idGetter); messageType.GetField("Name").SetValue(raw, "get_Id"); messageType.GetField("Failure").SetValue(raw, "args"); }
                if (selected == "saturated") { for (int i = 0; i < 16; i++) typed.GetMethod("ReadId").Invoke(null, new object[] { store.GetTransparentProxy() }); messageType.GetField("Failure").SetValue(raw, "method"); }
                bool caught = false;
                try { if (selected == "declared") typed.GetMethod("ReadRefused").Invoke(null, new object[] { store.GetTransparentProxy() }); else store.Invoke((IMessage)raw); } catch (Exception) { caught = true; }
                ProbeFiles.Require(caught && store.State.Faulted && stream.Requests == 0 && !UndoFactoryCallbackExcluded(), "OWNED_DIAGNOSTIC_STICKY_RESTORE");
                if (selected == "saturated") { for (int i = 0; i < 3; i++) { try { store.Invoke((IMessage)raw); } catch (Exception) { } } }
                if (selected == "saturated") ProbeFiles.Require(diagnostics == 0 && store.State.Entries == 17 && (int)messageType.GetField("MethodReads").GetValue(raw) == 0, "OWNED_DIAGNOSTIC_CAP");
                else {
                    ProbeFiles.Require(diagnostics == 1, "OWNED_DIAGNOSTIC_SINGLE_ATTEMPT");
                    string expected = selected == "custom" ? "category=UNTRUSTED_METHOD_KIND" : selected == "foreign" ? "category=FOREIGN_OWNER" : selected == "method-throw" ? "phase=METHOD_READ" : selected == "args" ? "phase=ARGS_READ" : "name=get_Unprovided";
                    ProbeFiles.Require(diagnostic.Contains(expected), "OWNED_DIAGNOSTIC_DESCRIPTOR");
                    ProbeFiles.Require(diagnostic.Contains("phase=" + (selected == "method-throw" ? "METHOD_READ" : selected == "args" ? "ARGS_READ" : "MESSAGE_SHAPE") + " "), "OWNED_DIAGNOSTIC_PHASE");
                    if (selected != "declared") ProbeFiles.Require((int)messageType.GetField("MethodReads").GetValue(raw) == 1, "OWNED_DIAGNOSTIC_SINGLE_MESSAGE_READ");
                    if (selected == "forged") ProbeFiles.Require((int)messageType.GetField("NameReads").GetValue(raw) == 0 && (int)messageType.GetField("ArgsReads").GetValue(raw) == 0, "OWNED_DIAGNOSTIC_NO_EXTRA_READS");
                    if (selected == "reentry") ProbeFiles.Require(store.State.Entries == 2, "OWNED_DIAGNOSTIC_REENTRY_BOUND");
                }
            }
            Record("OWNED_STORE_DIAGNOSTIC_CASE_PASS case=" + selected + " diagnostics=" + diagnostics + " entries=" + store.State.Entries + " faulted=" + store.State.Faulted + " values=NOT_INSPECTED");
        }); }); });
    }

    static void OwnedContextConstruction()
    {
        if (XmlRegistrationOwned) OwnedXmlRegistrationPolicyProof();
        contextOwnedAssembly = Assembly.LoadFrom(fixture); Func<string,Type> type = ContextOwnedType;
        Type project = type("ContextProjectFixture"), collection = type("ContextCollectionFixture"), parent = type("IContextParentFixture"), argument = type("IContextArgumentFixture"), factoryType = type("UndoFactoryDeliveryFixture"), contract = type("IFactoryFixture"), storeType = type("ICacheProjectStoreFixture");
        Type[] importerTypes = new Type[] { storeType, project, type("IContextKeyFixture"), typeof(Func<string,string>), typeof(Func<,>).MakeGenericType(new Type[] { typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(new Type[] { type("ContextBaggageFixture") }), typeof(bool) }), typeof(Action<>).MakeGenericType(new Type[] { type("IContextImportInformationFixture") }), typeof(Predicate<>).MakeGenericType(new Type[] { type("IContextImportInformationFixture") }), type("ContextImporterFixture") };
        ContextConstructionApi api = ContextApi(project, collection, parent, argument, factoryType, contract, storeType, importerTypes, false);
        if (ownedContextCase == null) {
            int refused = 0;
            for (int i = 0; i < 4; i++) {
                Type[] bad = (Type[])importerTypes.Clone(); if (i == 0) bad[2] = typeof(int); if (i == 1) bad[2] = typeof(System.Collections.Generic.List<>);
                try { ContextApi(i == 2 ? type("ContextWrongProjectFixture") : project, i == 3 ? type("ContextWrongCollectionFixture") : collection, parent, argument, factoryType, contract, storeType, bad, false); }
                catch (IOException) { refused++; }
            }
            try { new ContextImporterDenyHolder(typeof(Func<object>), importerTypes[7], Record); } catch (ArgumentException) { refused++; }
            try { new ContextImporterDenyHolder(api.Callback, typeof(string), Record); } catch (ArgumentException) { refused++; }
            try { ContextReference(new TypeDelegator(typeof(object))); } catch (IOException) { refused++; }
            try { ContextReference(typeof(string).MakeByRefType()); } catch (IOException) { refused++; }
            ProbeFiles.Require(refused == 8, "OWNED_CONTEXT_API_NEGATIVES"); Record("OWNED_CONTEXT_API_PASS negatives=8 before-effects=True");
        }
        PrepareContextArguments(api);
        Type control = type("ContextOwnedControl"), undoControl = type("UndoOwnedControl"), traceControl = type("TraceLifetimeControl");
        control.GetField("Parent").SetValue(null, contextRetained[3]); control.GetField("Context").SetValue(null, contextRetained[4]);
        control.GetField("Callback").SetValue(null, new Action<string>(OwnedContextPoint));
        control.GetField("ThrowCollection").SetValue(null, ownedContextCase == "collection-throw"); control.GetField("ThrowProject").SetValue(null, ownedContextCase == "error-context");
        undoControl.GetField("Callback").SetValue(null, new Action<string>(OwnedContextPoint));
        traceControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
        traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { OwnedContextPoint("trace-ctor"); }));
        traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { OwnedContextPoint("trace-dispose"); }));
        EmptyContainerApi container = EmptyApi(type("UndoBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture")); Type extension = type("UndoFactoryRegistrationExtensionsFixture");
        TraceRegistrationApi traceRegistration = RegistrationApi(container, extension, type("IRegistrationFixture`3"), type("RegistrationDataFixture"), type("RegistrationStyleFixture"), trace.Contract);
        Type undoType = type("IUndoDeniedFixture"); UndoContract(undoType, type("IOpaqueUndoFixture"));
        UndoFactoryRegistrationApi registration = FactoryRegistrationApi(container, extension, type("IUndoFactoryRegistrationFixture`3"), type("RegistrationDataFixture"), type("RegistrationStyleFixture"), undoType);
        ConstructorInfo factory = FactoryCtor(factoryType, container.Context, type("IDataDependencyFixture"));
        Type streamType = type("ICacheStreamFixture"), id = type("ScopedIdFixture");
        var stream = new StreamStoreDenyProxy(streamType, StreamContract(streamType, false), Record);
        object idValue = CreateFixtureId(id, id.GetConstructor(new Type[] { typeof(uint) }), id.GetMethod("get_ProjectNumber"));
        var store = new LargeStringCacheStoreProxy(storeType, storeType.GetMethod("get_Id"), idValue, storeType.GetMethod("get_LargeStringCache"), streamType, stream.GetTransparentProxy(), Record);
        var kim = new ProjectDenyProxy(type("IDataDependencyFixture"), Record); var undo = new UndoProviderDenyProxy(undoType, Record);
        var holder = new UndoProviderFactoryHolder(undoType, undo.GetTransparentProxy(), registration.CallbackType, Record);
        WithContextLifetime(delegate { WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate { WithUndoFactoryProvider(trace, container, traceRegistration, registration, factory, kim, store, undo, holder,
            delegate(object actual) { WithContextConstruction(api, actual, store); }); }); }); });
        ProbeFiles.Require(ownedContextCase == null && (int)control.GetField("Collections").GetValue(null) == 1 && (int)control.GetField("Projects").GetValue(null) == 1 && (int)control.GetField("Disposals").GetValue(null) == 1
            && store.State.Replies == 1 && store.State.Attempts == 1 && store.State.Entries == 3 && !store.State.Faulted && stream.Requests == 0 && store.DeniedRequests == 0 && holder.Entries == 1 && holder.Successes == 1 && !holder.Faulted
            && (int)undoControl.GetField("Registrations").GetValue(null) == 2 && (int)undoControl.GetField("ContainerDisposals").GetValue(null) == 1 && (int)traceControl.GetField("Disposals").GetValue(null) == 1, "OWNED_CONTEXT_LIFETIME");
        Record("OWNED_CONTEXT_LIFETIME_PASS collection=1 project=1 project-dispose=1 collection-dispose=NOT_PROVIDED typed-arguments=True cache-reference=1 factory-success=1");
    }

    sealed class ContextConstructionApi
    {
        internal Type Project, Collection, Parent, Context, Factory, Contract, Store, Callback;
        internal ConstructorInfo CollectionConstructor, ProjectConstructor; internal MethodInfo Dispose;
    }
    static void ContextReference(Type type)
    { ProbeFiles.Require(type != null && type.GetType() == typeof(object).GetType() && !type.ContainsGenericParameters && !type.IsValueType && !type.IsByRef && !type.IsPointer, "CONTEXT_REFERENCE_TYPE"); }
    static ContextConstructionApi ContextApi(Type project, Type collection, Type parent, Type context, Type factory, Type contract, Type store, Type[] importerTypes, bool original)
    {
        foreach (Type type in new Type[] { project, collection, parent, context, factory, contract, store }) { ContextReference(type); ProbeFiles.Require(type.Assembly == project.Assembly && !type.Assembly.ReflectionOnly, "CONTEXT_SAME_OWNER"); }
        ProbeFiles.Require(project.IsPublic && project.IsClass && !project.IsAbstract && !project.IsGenericType && collection.IsPublic && collection.IsClass && !collection.IsAbstract && !collection.IsGenericType
            && factory.IsPublic && factory.IsClass && contract.IsAssignableFrom(factory) && PublicInterface(parent) && PublicInterface(context) && PublicInterface(contract) && PublicInterface(store)
            && !parent.IsGenericType && !context.IsGenericType && parent.DeclaringType == null && parent.BaseType == null, "CONTEXT_API_PUBLIC_SHAPES");
        if (original) {
            Assembly om = project.Assembly; Assembly common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.dll"));
            ProbeFiles.Require(ContextOriginal && om.FullName == OmIdentity && ProbeFiles.Same(om.Location, Path.Combine(stage, "Knx.Ets.ObjectModel.dll"))
                && common.FullName == Identities[10] && ProbeFiles.Same(common.Location, Path.Combine(stage, "Knx.Ets.Common.dll")), "CONTEXT_PINNED_OWNERS");
            Type baggage = om.GetType("Knx.Ets.ObjectModel.Import.BaggageInfo", true), information = om.GetType("Knx.Ets.ObjectModel.Import.IImportInformationCollector", true);
            importerTypes = new Type[] { store, project, common.GetType("Knx.Ets.Common.Security.IKeyProvider", true), typeof(Func<string,string>),
                typeof(Func<,>).MakeGenericType(new Type[] { typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(new Type[] { baggage }), typeof(bool) }),
                typeof(Action<>).MakeGenericType(new Type[] { information }), typeof(Predicate<>).MakeGenericType(new Type[] { information }), om.GetType("Knx.Ets.ObjectModel.Import.ProjectImporter", true) };
            ProbeFiles.Require(project.MetadataToken == 0x020003a5 && (int)project.Attributes == 1048577 && collection.MetadataToken == 0x020003bd && (int)collection.Attributes == 1048577
                && parent.MetadataToken == 0x02000c99 && (int)parent.Attributes == 1048737 && context.MetadataToken == 0x02000044 && (int)context.Attributes == 1048737
                && contract.MetadataToken == 0x02000037 && store.MetadataToken == 0x02000057, "CONTEXT_ORIGINAL_TYPE_TOKENS");
        }
        ProbeFiles.Require(importerTypes != null && importerTypes.Length == 8 && importerTypes[0] == store && importerTypes[1] == project, "CONTEXT_IMPORTER_VECTOR");
        foreach (Type type in importerTypes) ContextReference(type);
        ProbeFiles.Require(importerTypes[3] == typeof(Func<string,string>) && importerTypes[4].IsGenericType && importerTypes[4].GetGenericTypeDefinition() == typeof(Func<,>)
            && importerTypes[4].GetGenericArguments()[1] == typeof(bool) && importerTypes[4].GetGenericArguments()[0].IsGenericType
            && importerTypes[4].GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)
            && importerTypes[5].IsGenericType && importerTypes[5].GetGenericTypeDefinition() == typeof(Action<>) && importerTypes[6].IsGenericType
            && importerTypes[6].GetGenericTypeDefinition() == typeof(Predicate<>) && importerTypes[5].GetGenericArguments()[0] == importerTypes[6].GetGenericArguments()[0], "CONTEXT_IMPORTER_NESTED_BCL");
        Type callback = typeof(Func<,,,,,,,>).MakeGenericType(importerTypes); MethodInfo invoke = callback.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        ParameterInfo[] args = invoke.GetParameters();
        ProbeFiles.Require(callback.Assembly == typeof(object).Assembly && invoke.ReturnType == importerTypes[7] && !invoke.IsGenericMethod && args.Length == 7
            && invoke.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "CONTEXT_IMPORTER_INVOKE");
        for (int i = 0; i < 7; i++) ProbeFiles.Require(args[i].ParameterType == importerTypes[i], "CONTEXT_IMPORTER_INVOKE_ORDER");
        Type[][] vectors = new Type[][] { new Type[] { parent, contract, callback }, new Type[] { collection, store, contract, context } };
        ConstructorInfo[] constructors = new ConstructorInfo[] { collection.GetConstructor(vectors[0]), project.GetConstructor(vectors[1]) };
        for (int i = 0; i < 2; i++) {
            ConstructorInfo ctor = constructors[i]; ProbeFiles.Require(ctor != null && ctor.GetType() == typeof(object).GetConstructor(Type.EmptyTypes).GetType() && ctor.DeclaringType == (i == 0 ? collection : project)
                && ctor.IsPublic && !ctor.IsStatic && !ctor.ContainsGenericParameters && ctor.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "CONTEXT_EXACT_CONSTRUCTOR");
            ParameterInfo[] actual = ctor.GetParameters(); ProbeFiles.Require(actual.Length == vectors[i].Length, "CONTEXT_CONSTRUCTOR_ARITY");
            for (int j = 0; j < actual.Length; j++) ProbeFiles.Require(actual[j].ParameterType == vectors[i][j], "CONTEXT_CONSTRUCTOR_VECTOR");
        }
        MethodInfo dispose = project.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        ProbeFiles.Require(typeof(IDisposable).IsAssignableFrom(project) && dispose != null && dispose.DeclaringType == project && dispose.ReturnType == typeof(void) && !dispose.IsGenericMethod
            && dispose.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "CONTEXT_PROJECT_DISPOSE");
        InterfaceMapping map = project.GetInterfaceMap(typeof(IDisposable));
        ProbeFiles.Require(map.TargetMethods.Length == 1 && map.InterfaceMethods.Length == 1 && map.InterfaceMethods[0].Equals(typeof(IDisposable).GetMethod("Dispose"))
            && map.TargetMethods[0].Equals(dispose), "CONTEXT_PROJECT_DISPOSE_MAP");
        if (original) ProbeFiles.Require(constructors[0].MetadataToken == 0x0600298e && constructors[1].MetadataToken == 0x06002831 && dispose.MetadataToken == 0x060028bd
            && (int)constructors[0].Attributes == 6278 && (int)constructors[1].Attributes == 6278 && (int)dispose.Attributes == 486, "CONTEXT_ORIGINAL_MEMBER_TOKENS");
        return new ContextConstructionApi { Project = project, Collection = collection, Parent = parent, Context = context, Factory = factory, Contract = contract, Store = store, Callback = callback,
            CollectionConstructor = constructors[0], ProjectConstructor = constructors[1], Dispose = dispose };
    }
    static ContextArgumentDenyProxy contextParent, contextObject; static ContextImporterDenyHolder contextImporter;
    static object[] contextRetained; static bool contextLifetime;
    static void PrepareContextArguments(ContextConstructionApi api)
    {
        ProbeFiles.Require(ContextMode && contextParent == null && contextObject == null && contextImporter == null, "CONTEXT_PREPARE_ONCE");
        contextParent = new ContextArgumentDenyProxy(api.Parent, "parent", ContextArgumentRecord);
        contextObject = new ContextArgumentDenyProxy(api.Context, "context", ContextArgumentRecord);
        contextImporter = new ContextImporterDenyHolder(api.Callback, api.Callback.GetGenericArguments()[7], ContextArgumentRecord);
        contextRetained = new object[] { contextParent, contextObject, contextImporter, contextParent.GetTransparentProxy(), contextObject.GetTransparentProxy(), contextImporter.Callback, null };
    }
    static void ContextFinalAudit(int parentCount, bool parentFault, int objectCount, bool objectFault, int importerCount, bool importerFault, string phase)
    {
        bool denied = parentCount != 0 || parentFault || objectCount != 0 || objectFault || importerCount != 0 || importerFault;
        try { Record("CONTEXT_FINAL_AUDIT phase=" + phase + " parent=" + parentCount + "/" + parentFault + " context=" + objectCount + "/" + objectFault + " importer=" + importerCount + "/" + importerFault + " after-managed-cleanup=" + !contextLifetime); }
        finally { if (denied) throw new NotSupportedException("CONTEXT_ARGUMENT_FINAL_DENIAL"); }
    }
    static void CheckContextArguments(string phase)
    {
        if (!ContextMode || (contextParent == null && contextObject == null && contextImporter == null)) return;
        ContextFinalAudit(contextParent == null ? 0 : contextParent.State.Entries, contextParent != null && contextParent.State.Faulted,
            contextObject == null ? 0 : contextObject.State.Entries, contextObject != null && contextObject.State.Faulted,
            contextImporter == null ? 0 : contextImporter.State.Entries, contextImporter != null && contextImporter.State.Faulted, phase);
    }
    static void WithContextLifetime(Action run)
    {
        ProbeFiles.Require(ContextMode && !contextLifetime, "CONTEXT_LIFETIME_ONCE"); contextLifetime = true;
        try { run(); }
        finally {
            contextLifetime = false;
            try { if (ContextOwned) { if (ownedContextCase == "dispose-begin-log-fault") OwnedContextDisposeLogProof(); OwnedContextPoint("late"); } }
            finally { try { CheckContextArguments("AFTER_ALL_MANAGED_CLEANUP"); } finally { GC.KeepAlive(contextRetained); } }
        }
    }
    static void ContextConstructionRecord(string message)
    {
        if (ContextOwned && ownedContextCase == "dispose-begin-log-fault" && message == "CONTEXT_PROJECT_DISPOSE_BEGIN attempts=1") {
            Record("OWNED_CONTEXT_DISPOSE_BEGIN_LOG_FAULT injected=True");
            throw new IOException("OWNED_CONTEXT_DISPOSE_BEGIN_LOG_FAULT");
        }
        Record(message);
    }
    static void OwnedContextDisposeLogProof()
    {
        Type control = ContextOwnedType("ContextOwnedControl"), undo = ContextOwnedType("UndoOwnedControl"), trace = ContextOwnedType("TraceLifetimeControl");
        ProbeFiles.Require((int)control.GetField("Projects").GetValue(null) == 1 && (int)control.GetField("Disposals").GetValue(null) == 1
            && (int)undo.GetField("ProjectDisposals").GetValue(null) == 1 && (int)undo.GetField("ContainerDisposals").GetValue(null) == 1
            && (int)trace.GetField("Disposals").GetValue(null) == 1, "OWNED_CONTEXT_DISPOSE_LOG_GUARANTEE");
        Record("OWNED_CONTEXT_DISPOSE_LOG_PASS project=1 dispose=1 container-dispose=1 trace-dispose=1 after-all-managed-cleanup=True");
    }
    static void WithContextConstruction(ContextConstructionApi api, object factory, ProjectStoreIdProxy store)
    {
        object collection = null, project = null; bool exact = false, collectionReturned = false, projectReturned = false;
        ProbeFiles.Require(ContextMode && contextLifetime && factory != null && factory.GetType() == api.Factory && api.Contract.IsInstanceOfType(factory)
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(factory), "CONTEXT_GENUINE_FACTORY");
        try {
            TraceDependencyGuard(unexpected, rejected, "CONTEXT_PRE_COLLECTION");
            ProbeFiles.Require(projectResourceWindow == 0 && !projectResourceCallbackSuspended, "CONTEXT_COLLECTION_WINDOW_ZERO");
            Record("CONTEXT_COLLECTION_BEGIN token=" + api.CollectionConstructor.MetadataToken.ToString("X8") + " window=0 explicit-attempts=1");
            try { collection = api.CollectionConstructor.Invoke(new object[] { contextRetained[3], factory, contextImporter.Callback }); collectionReturned = true; }
            finally { Record("CONTEXT_COLLECTION_END returned=" + collectionReturned + " body-entry=" + (collectionReturned ? "NOT_INSTRUMENTED" : "NOT_ESTABLISHED")); }
            contextRetained[6] = collection;
            ProbeFiles.Require(collection != null && collection.GetType() == api.Collection && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection), "CONTEXT_EXACT_COLLECTION");
            TraceDependencyGuard(unexpected, rejected, "CONTEXT_POST_COLLECTION"); ProbeFiles.Require(store.DeniedRequests == 0, "CONTEXT_COLLECTION_STORE_DENIAL");
            Record("CONTEXT_PROJECT_BEGIN token=" + api.ProjectConstructor.MetadataToken.ToString("X8") + " window=2 explicit-attempts=1");
            try {
                WithProjectResourceWindow(2, delegate { project = api.ProjectConstructor.Invoke(new object[] { collection, store.GetTransparentProxy(), factory, contextRetained[4] }); }); projectReturned = true;
                exact = project != null && project.GetType() == api.Project && project is IDisposable && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project);
                ProbeFiles.Require(exact, "CONTEXT_EXACT_PROJECT"); TraceDependencyGuard(unexpected, rejected, "CONTEXT_POST_PROJECT"); ProbeFiles.Require(store.DeniedRequests == 0, "CONTEXT_PROJECT_STORE_DENIAL");
            } finally { Record("CONTEXT_PROJECT_END returned=" + projectReturned + " body-entry=" + (projectReturned ? "NOT_INSTRUMENTED" : "NOT_ESTABLISHED")); }
        } catch { if (ContextOwned) OwnedContextPoint("error"); throw; } finally {
            try { if (exact) {
                try { ContextConstructionRecord("CONTEXT_PROJECT_DISPOSE_BEGIN attempts=1"); }
                finally { try { ((IDisposable)project).Dispose(); } finally { Record("CONTEXT_PROJECT_DISPOSE_END attempts=1"); } }
            } }
            finally { try { TraceDependencyGuard(unexpected, rejected, "CONTEXT_POST_PROJECT_DISPOSAL"); } finally { GC.KeepAlive(project); GC.KeepAlive(collection); GC.KeepAlive(contextRetained); } }
        }
    }

    sealed class ProjectScopeApi
    {
        internal Type Project, FactoryType, FactoryContract;
        internal ConstructorInfo Constructor;
        internal MethodInfo Dispose;
    }
    static ProjectScopeApi ScopedProjectApi(Type project, Type factoryType, Type contract, Type style, Type store)
    {
        ProbeFiles.Require(project != null && project.IsPublic && project.IsClass && !project.IsAbstract && !project.IsGenericType && !project.ContainsGenericParameters
            && factoryType != null && factoryType.IsPublic && factoryType.IsClass && !factoryType.IsAbstract && !factoryType.IsGenericType
            && PublicInterface(contract) && contract.IsAssignableFrom(factoryType) && PublicInterface(store) && style != null && style.IsPublic && style.IsEnum
            && project.Assembly == factoryType.Assembly && project.Assembly == contract.Assembly && project.Assembly == store.Assembly && !project.Assembly.ReflectionOnly,
            "PROJECT_SCOPE_PUBLIC_OWNERSHIP");
        ConstructorInfo ctor = ProjectCtor(project, contract, style, store);
        ParameterInfo[] parameters = ctor.GetParameters();
        ProbeFiles.Require(parameters.Length == 4 && parameters[0].ParameterType == contract && parameters[1].ParameterType == typeof(string)
            && parameters[2].ParameterType == style && parameters[3].ParameterType == store, "PROJECT_SCOPE_EXACT_PARAMETERS");
        ProbeFiles.Require(ctor.DeclaringType == project && ctor.IsPublic && !ctor.IsStatic && !ctor.ContainsGenericParameters
            && ctor.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "PROJECT_SCOPE_CONSTRUCTOR");
        ProbeFiles.Require(typeof(IDisposable).IsAssignableFrom(project), "PROJECT_SCOPE_DISPOSABLE");
        MethodInfo dispose = project.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        ProbeFiles.Require(dispose != null && dispose.DeclaringType == project && dispose.IsPublic && !dispose.IsStatic && !dispose.IsGenericMethod && !dispose.ContainsGenericParameters
            && dispose.ReturnType == typeof(void) && dispose.GetParameters().Length == 0
            && dispose.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "PROJECT_SCOPE_PUBLIC_DISPOSE");
        InterfaceMapping map = project.GetInterfaceMap(typeof(IDisposable));
        ProbeFiles.Require(map.InterfaceMethods.Length == 1 && map.TargetMethods.Length == 1 && map.InterfaceMethods[0].Equals(typeof(IDisposable).GetMethod("Dispose"))
            && map.TargetMethods[0].DeclaringType == project && map.TargetMethods[0].Module == dispose.Module && map.TargetMethods[0].MetadataToken == dispose.MetadataToken,
            "PROJECT_SCOPE_DISPOSE_MAP");
        return new ProjectScopeApi { Project = project, FactoryType = factoryType, FactoryContract = contract, Constructor = ctor, Dispose = dispose };
    }
    static bool ReviewedProjectScopeTokens(int project, int ctor, int dispose, int factoryContract)
    { return project == 0x020003a5 && ctor == 0x06002830 && dispose == 0x060028bd && factoryContract == 0x02000037; }
    static void WithProjectScope(ProjectScopeApi api, object factory, object style, ProjectStoreIdProxy store, Action<string> ownedGuard)
    {
        ProbeFiles.Require(ProjectScope && (ownedGuard == null || ProjectScopeOwned), "PROJECT_SCOPE_OWNED_GUARD");
        ProbeFiles.Require(factory != null && factory.GetType() == api.FactoryType && api.FactoryContract.IsInstanceOfType(factory)
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(factory), "PROJECT_SCOPE_GENUINE_FACTORY");
        Action<string> check = delegate(string phase) { TraceDependencyGuard(unexpected, rejected, "PROJECT_" + phase); if (ownedGuard != null) ownedGuard(phase); };
        object value = null; bool exact = false;
        try {
            check("PRE_CTOR"); bool returned = false;
            Record("PROJECT_SCOPE_CONSTRUCTOR_BEGIN token=" + api.Constructor.MetadataToken.ToString("X8") + " services=genuine-factory+fixture-id-only identity=OWNED_SYNTHETIC_NOT_REAL_OR_CLAIMED_VALID qualification=NOT_ESTABLISHED");
            try {
                object[] arguments = new object[] { factory, FixtureName, style, store.GetTransparentProxy() };
                Action invoke = delegate { value = api.Constructor.Invoke(arguments); };
                if (ProjectResources) WithProjectResourceWindow(2, invoke); else invoke();
                returned = true;
                if (ProjectResourcesOwned) OwnedProjectResourcePoint("validation");
                exact = value != null && value.GetType() == api.Project && value is IDisposable && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value);
                ProbeFiles.Require(exact, "PROJECT_SCOPE_EXACT_RETURN"); check("POST_CTOR");
                ProbeFiles.Require(store.DeniedRequests == 0, "PROJECT_SCOPE_STORE_DENIAL");
            } catch (Exception error) { if (ProjectResourcesOwned) OwnedProjectResourcePoint("error"); Errors(error); throw; }
            finally { if (ProjectResourcesOwned) OwnedProjectResourcePoint("logging"); Record("PROJECT_SCOPE_CONSTRUCTOR_END returned=" + returned + " body-entry=" + (returned ? "NOT_INSTRUMENTED" : "NOT_ESTABLISHED")); }
        } finally {
            try {
                if (exact) {
                    bool disposed = false;
                    try { Record("PROJECT_SCOPE_DISPOSE_BEGIN token=" + api.Dispose.MetadataToken.ToString("X8") + " explicit-attempts=1 hard-termination-disposal=NOT_ESTABLISHED"); }
                    finally {
                        try { ((IDisposable)value).Dispose(); disposed = true; }
                        catch (Exception error) { Errors(error); throw; }
                        finally { Record("PROJECT_SCOPE_DISPOSE_END returned=" + disposed + " explicit-attempts=1"); }
                    }
                }
            } finally {
                try { check("POST_DISPOSE"); }
                finally { GC.KeepAlive(value); GC.KeepAlive(factory); GC.KeepAlive(store); }
            }
        }
        Record("PROJECT_SCOPE_RETURNED_NOT_QUALIFIED factory-methods=NOT_INTERCEPTED explicit-project-other-methods=0");
    }
    static void InvokeFactoryForProject(ConstructorInfo ctor, object context, ProjectDenyProxy provider, Action<object> use)
    {
        object factory = null; bool returned = false;
        Record("FACTORY_CONSTRUCTOR_BEGIN token=" + ctor.MetadataToken.ToString("X8") + " services=genuine-registered-trace-container+deny-all-provider project=NOT_REACHED qualification=NOT_ESTABLISHED");
        try {
            ProbeFiles.Require(ProjectScope && !unexpected && !rejected, "PROJECT_FACTORY_PRECONSTRUCTOR"); CheckUndoProvider("FACTORY_PRE_CTOR"); CheckUndoFactory("FACTORY_PRE_CTOR"); CheckCache("FACTORY_PRE_CTOR"); CheckContextArguments("FACTORY_PRE_CTOR");
            if (ProjectResources) {
                object[] arguments = new object[] { context, provider.GetTransparentProxy() };
                WithProjectResourceWindow(1, delegate { factory = ctor.Invoke(arguments); }); returned = true; CheckUndoProvider("FACTORY_POST_CTOR"); CheckUndoFactory("FACTORY_POST_CTOR"); CheckCache("FACTORY_POST_CTOR"); CheckContextArguments("FACTORY_POST_CTOR");
            } else WithFactoryResourcePhase(delegate { factory = ctor.Invoke(new object[] { context, provider.GetTransparentProxy() }); returned = true; });
            ProbeFiles.Require(factory != null && factory.GetType() == ctor.DeclaringType && !unexpected && !rejected, "PROJECT_FACTORY_EXACT_RETURN");
            ProbeFiles.Require(provider.Requests == 0, "PROJECT_FACTORY_PROVIDER_DENIAL");
        } finally { Record("FACTORY_CONSTRUCTOR_END returned=" + returned + " provider-denied-requests=" + provider.Requests + " project=NOT_REACHED qualification=NOT_ESTABLISHED"); }
        // Local ownership survives Project cleanup; it never depends on the global diagnostic field.
        try { use(factory); } finally { GC.KeepAlive(factory); }
    }
    static void OriginalContextConstruction()
    {
        ProbeFiles.Require(ProjectScopeOriginal && containerInputsReady, "PROJECT_SCOPE_COMPLETE_PROFILE");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll")), common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.dll"));
        Assembly autofac = Assembly.LoadFrom(Path.Combine(stage, "Autofac.dll")), kim = Assembly.LoadFrom(Path.Combine(stage, "Knx.Kim.DataProvider.dll"));
        Assembly ids = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Ids.dll"));
        ProbeFiles.Require(common.FullName == Identities[10] && autofac.FullName == FactoryExtraIdentities[0] && !common.ReflectionOnly && !autofac.ReflectionOnly
            && ProbeFiles.Same(common.Location, Path.Combine(stage, "Knx.Ets.Common.dll")) && ProbeFiles.Same(autofac.Location, Path.Combine(stage, "Autofac.dll")), "PROJECT_SCOPE_PINNED_OWNERS");
        Type traceType = common.GetType("Knx.Ets.Common.Security.ProjectTraceEncryptor", true), traceContract = common.GetType("Knx.Ets.Common.Security.IProjectTraceEncryptor", true);
        TraceLifetimeApi trace = TraceApi(traceType, traceContract);
        ProbeFiles.Require((int)traceType.Attributes == 1048833 && ReviewedTraceTokens(traceType.MetadataToken, trace.Constructor.MetadataToken, trace.Parameter.MetadataToken, traceContract.MetadataToken), "TRACE_ORIGINAL_TOKENS");
        Type context = autofac.GetType("Autofac.IComponentContext", true), providerType = kim.GetType("Knx.Kim.DataProvider.IKimDataProvider", true), factory = om.GetType("Knx.Ets.ObjectModel.Factory", true);
        ConstructorInfo factoryCtor = FactoryCtor(factory, context, providerType); OriginalFactoryTokens(factory, factoryCtor, context, providerType);
        EmptyContainerApi container = EmptyApi(autofac.GetType("Autofac.ContainerBuilder", true), autofac.GetType("Autofac.Builder.ContainerBuildOptions", true), autofac.GetType("Autofac.IContainer", true), context);
        TraceRegistrationApi registration = RegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), traceContract);
        Type project = om.GetType("Knx.Ets.ObjectModel.Project.Project", true), factoryContract = om.GetType("Knx.Ets.ObjectModel.IFactory", true), storeType = om.GetType("Knx.Ets.ObjectModel.IProjectStore", true);
        Type id = ids.GetType("Knx.Ets.Ids.Project.ProjectId", true);
        Type collection = om.GetType("Knx.Ets.ObjectModel.Project.ProjectCollection", true), parent = om.GetType("Knx.Ets.ObjectModel.Interfaces.INamedDomObject", true), objectContext = om.GetType("Knx.Ets.ObjectModel.IObjectContext", true);
        ContextConstructionApi scope = ContextApi(project, collection, parent, objectContext, factory, factoryContract, storeType, null, true);
        ConstructorInfo idCtor = id.GetConstructor(new Type[] { typeof(uint) }); MethodInfo idGetter = id.GetMethod("get_ProjectNumber"), storeGetter = storeType.GetMethod("get_Id");
        OriginalIdTokens(id, idCtor, idGetter, storeType, storeGetter);
        TraceDependencyGuard(unexpected, rejected, "CONTEXT_SHAPES");
        TraceRegistrationApi undoRegistration = null; UndoProviderDenyProxy undo = null;
        UndoFactoryRegistrationApi factoryRegistration = null; UndoProviderFactoryHolder holder = null;
        if (UndoDeniedOriginal) {
            Type undoType = om.GetType("Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider", true), opaque = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true);
            UndoContract(undoType, opaque); OriginalUndoTokens(undoType);
            if (UndoFactoryOriginal) factoryRegistration = FactoryRegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), undoType);
            else undoRegistration = RegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), undoType);
            undo = new UndoProviderDenyProxy(undoType, Record);
            if (UndoFactoryOriginal) holder = new UndoProviderFactoryHolder(undoType, undo.GetTransparentProxy(), factoryRegistration.CallbackType, Record);
        }
        StreamStoreDenyProxy stream = null; Type streamType = null; MethodInfo cacheGetter = null;
        if (CacheOriginal) { streamType = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true); MethodInfo[] declarations = StreamContract(streamType, true); cacheGetter = storeType.GetMethod("get_LargeStringCache", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly); CacheGetterSignature(storeType, cacheGetter, streamType); stream = new StreamStoreDenyProxy(streamType, declarations, Record); }
        object value = CreateFixtureId(id, idCtor, idGetter); ProjectStoreIdProxy store;
        if (CacheOriginal) store = new LargeStringCacheStoreProxy(storeType, storeGetter, value, cacheGetter, streamType, stream.GetTransparentProxy(), Record);
        else store = new ProjectStoreIdProxy(storeType, storeGetter, value, Record);
        var denied = new ProjectDenyProxy(providerType, Record);
        resourceOwner = autofac;
        PrepareContextArguments(scope);
        Action run = delegate {
            if (UndoFactoryOriginal) WithUndoFactoryProvider(trace, container, registration, factoryRegistration, factoryCtor, denied, store, undo, holder,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); });
            else if (UndoDeniedOriginal) WithUndoProvider(trace, container, registration, undoRegistration, factoryCtor, denied, store, undo,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); });
            else WithRegisteredTrace(trace, container, registration, factoryCtor, denied,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); }, store);
        };
        WithContextLifetime(delegate { WithCacheLifetime((LargeStringCacheStoreProxy)store, stream, delegate { WithProjectResourceBudget(run); }); });
        GC.KeepAlive(value);
    }
    static void OriginalProjectScope()
    {
        ProbeFiles.Require(ProjectScopeOriginal && containerInputsReady, "PROJECT_SCOPE_COMPLETE_PROFILE");
        Assembly om = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.ObjectModel.dll")), common = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.dll"));
        Assembly autofac = Assembly.LoadFrom(Path.Combine(stage, "Autofac.dll")), kim = Assembly.LoadFrom(Path.Combine(stage, "Knx.Kim.DataProvider.dll"));
        Assembly types = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Common.Types.dll")), ids = Assembly.LoadFrom(Path.Combine(stage, "Knx.Ets.Ids.dll"));
        ProbeFiles.Require(common.FullName == Identities[10] && autofac.FullName == FactoryExtraIdentities[0] && !common.ReflectionOnly && !autofac.ReflectionOnly
            && ProbeFiles.Same(common.Location, Path.Combine(stage, "Knx.Ets.Common.dll")) && ProbeFiles.Same(autofac.Location, Path.Combine(stage, "Autofac.dll")), "PROJECT_SCOPE_PINNED_OWNERS");
        Type traceType = common.GetType("Knx.Ets.Common.Security.ProjectTraceEncryptor", true), traceContract = common.GetType("Knx.Ets.Common.Security.IProjectTraceEncryptor", true);
        TraceLifetimeApi trace = TraceApi(traceType, traceContract);
        ProbeFiles.Require((int)traceType.Attributes == 1048833 && ReviewedTraceTokens(traceType.MetadataToken, trace.Constructor.MetadataToken, trace.Parameter.MetadataToken, traceContract.MetadataToken), "TRACE_ORIGINAL_TOKENS");
        Type context = autofac.GetType("Autofac.IComponentContext", true), providerType = kim.GetType("Knx.Kim.DataProvider.IKimDataProvider", true), factory = om.GetType("Knx.Ets.ObjectModel.Factory", true);
        ConstructorInfo factoryCtor = FactoryCtor(factory, context, providerType); OriginalFactoryTokens(factory, factoryCtor, context, providerType);
        EmptyContainerApi container = EmptyApi(autofac.GetType("Autofac.ContainerBuilder", true), autofac.GetType("Autofac.Builder.ContainerBuildOptions", true), autofac.GetType("Autofac.IContainer", true), context);
        TraceRegistrationApi registration = RegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), traceContract);
        Type project = om.GetType("Knx.Ets.ObjectModel.Project.Project", true), factoryContract = om.GetType("Knx.Ets.ObjectModel.IFactory", true), storeType = om.GetType("Knx.Ets.ObjectModel.IProjectStore", true);
        Type style = types.GetType(EnumName, true), id = ids.GetType("Knx.Ets.Ids.Project.ProjectId", true);
        ProjectScopeApi scope = ScopedProjectApi(project, factory, factoryContract, style, storeType);
        ProbeFiles.Require(ReviewedProjectScopeTokens(project.MetadataToken, scope.Constructor.MetadataToken, scope.Dispose.MetadataToken, factoryContract.MetadataToken)
            && (int)project.Attributes == 1048577 && (int)scope.Constructor.Attributes == 6278 && (int)scope.Dispose.Attributes == 486 && (int)factoryContract.Attributes == 1048737
            && project.Assembly == om && factoryContract.Assembly == om && storeType.Assembly == om && style.Assembly == types && id.Assembly == ids, "PROJECT_SCOPE_ORIGINAL_CONTRACT");
        ConstructorInfo idCtor = id.GetConstructor(new Type[] { typeof(uint) }); MethodInfo idGetter = id.GetMethod("get_ProjectNumber"), storeGetter = storeType.GetMethod("get_Id");
        OriginalIdTokens(id, idCtor, idGetter, storeType, storeGetter);
        FieldInfo literal = style.GetField(ReviewedLiteral, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(style.MetadataToken == ReviewedEnumTypeToken && Enum.GetUnderlyingType(style).FullName == ReviewedUnderlyingType && literal != null && literal.IsLiteral
            && literal.FieldType == style && literal.MetadataToken == ReviewedLiteralToken, "PROJECT_REVIEWED_ENUM_LITERAL");
        object raw = literal.GetRawConstantValue();
        ProbeFiles.Require(raw != null && raw.GetType().FullName == ReviewedUnderlyingType && Convert.ToString(raw, CultureInfo.InvariantCulture) == ReviewedRawValue, "PROJECT_REVIEWED_ENUM_VALUE");
        object selectedStyle = Enum.ToObject(style, raw);
        TraceDependencyGuard(unexpected, rejected, "PROJECT_SCOPE_SHAPES");
        Record("PROJECT_SCOPE_API_READY public-ctor=06002830 public-dispose=060028BD IFactory=02000037 interface-map=VERIFIED metadata-sha256=" + ReviewedProjectConstructionReportSha256 + " original-runtime=NOT_YET_ATTEMPTED");
        TraceRegistrationApi undoRegistration = null; UndoProviderDenyProxy undo = null;
        UndoFactoryRegistrationApi factoryRegistration = null; UndoProviderFactoryHolder holder = null;
        if (UndoDeniedOriginal) {
            Type undoType = om.GetType("Knx.Ets.ObjectModel.UndoRedo.IUndoManagerStoreProvider", true), opaque = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true);
            UndoContract(undoType, opaque); OriginalUndoTokens(undoType);
            if (UndoFactoryOriginal) factoryRegistration = FactoryRegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), undoType);
            else undoRegistration = RegistrationApi(container, autofac.GetType("Autofac.RegistrationExtensions", true), autofac.GetType("Autofac.Builder.IRegistrationBuilder`3", true), autofac.GetType("Autofac.Builder.SimpleActivatorData", true), autofac.GetType("Autofac.Builder.SingleRegistrationStyle", true), undoType);
            undo = new UndoProviderDenyProxy(undoType, Record);
            if (UndoFactoryOriginal) holder = new UndoProviderFactoryHolder(undoType, undo.GetTransparentProxy(), factoryRegistration.CallbackType, Record);
        }
        StreamStoreDenyProxy stream = null; Type streamType = null; MethodInfo cacheGetter = null;
        if (CacheOriginal) { streamType = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true); MethodInfo[] declarations = StreamContract(streamType, true); cacheGetter = storeType.GetMethod("get_LargeStringCache", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly); CacheGetterSignature(storeType, cacheGetter, streamType); stream = new StreamStoreDenyProxy(streamType, declarations, Record); }
        object value = CreateFixtureId(id, idCtor, idGetter); ProjectStoreIdProxy store;
        if (CacheOriginal) store = new LargeStringCacheStoreProxy(storeType, storeGetter, value, cacheGetter, streamType, stream.GetTransparentProxy(), Record);
        else store = new ProjectStoreIdProxy(storeType, storeGetter, value, Record);
        var denied = new ProjectDenyProxy(providerType, Record);
        resourceOwner = autofac;
        Action run = delegate {
            if (UndoFactoryOriginal) WithUndoFactoryProvider(trace, container, registration, factoryRegistration, factoryCtor, denied, store, undo, holder,
                delegate(object actualFactory) { WithProjectScope(scope, actualFactory, selectedStyle, store, null); });
            else if (UndoDeniedOriginal) WithUndoProvider(trace, container, registration, undoRegistration, factoryCtor, denied, store, undo,
                delegate(object actualFactory) { WithProjectScope(scope, actualFactory, selectedStyle, store, null); });
            else WithRegisteredTrace(trace, container, registration, factoryCtor, denied,
                delegate(object actualFactory) { WithProjectScope(scope, actualFactory, selectedStyle, store, null); }, store);
        };
        if (CacheOriginal) WithCacheLifetime((LargeStringCacheStoreProxy)store, stream, delegate { WithProjectResourceBudget(run); });
        else if (ProjectResources) WithProjectResourceBudget(run); else run();
        GC.KeepAlive(value);
    }
    static void OwnedProjectScope()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type scopeControl = type("ProjectScopeControl"), traceControl = type("TraceLifetimeControl"), emptyControl = type("EmptyControlFixture"), registrationControl = type("RegistrationControl");
        Type factory = type("ScopedGenuineFactoryFixture"), contract = type("IFactoryFixture"), style = type("GroupAddressStyleFixture"), storeType = type("IScopedProjectStoreFixture"), id = type("ScopedIdFixture");
        Func<Type, string, int> count = delegate(Type owner, string name) { return (int)owner.GetField(name).GetValue(null); };
        Action<string> reset = delegate(string behavior) {
            scopeControl.GetMethod("Reset").Invoke(null, new object[] { behavior }); registrationControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
            emptyControl.GetMethod("Reset").Invoke(null, new object[] { "return" }); traceControl.GetMethod("Reset").Invoke(null, new object[] { behavior == "trace-dispose-throw" ? "dispose-throw" : "return" });
            traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { registrationControl.GetMethod("Event").Invoke(null, new object[] { "trace-ctor" }); }));
            traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { registrationControl.GetMethod("Event").Invoke(null, new object[] { "trace-dispose" }); scopeControl.GetMethod("Later").Invoke(null, new object[] { "trace" }); }));
        };
        reset("return"); int negatives = 0;
        foreach (string name in new string[] { "ScopedNoDisposeFixture", "ScopedExplicitDisposeFixture", "ScopedWrongMapFixture", "ScopedWrongConstructorFixture", "ScopedGenericProjectFixture`1" }) {
            bool denied = false; try { ScopedProjectApi(type(name), factory, contract, style, storeType); } catch (IOException) { denied = true; }
            Record("OWNED_PROJECT_SCOPE_NEGATIVE fixture=" + name + " refused=" + denied + " before-effects=True");
            ProbeFiles.Require(denied, "OWNED_PROJECT_SCOPE_SHAPE_NEGATIVE:" + name); negatives++;
        }
        foreach (Action bad in new Action[] {
            delegate { ScopedProjectApi(type("ScopedProjectFixture"), type("RegisteredFactoryFixture"), contract, style, storeType); },
            delegate { ScopedProjectApi(type("ScopedProjectFixture"), factory, typeof(IDisposable), style, storeType); }
        }) { bool denied = false; try { bad(); } catch (IOException) { denied = true; } ProbeFiles.Require(denied, "OWNED_PROJECT_SCOPE_ASSIGNABILITY_NEGATIVE"); negatives++; }
        ProbeFiles.Require(count(scopeControl, "Constructors") == 0 && count(scopeControl, "IdConstructors") == 0 && count(emptyControl, "Factories") == 0
            && ReviewedProjectScopeTokens(0x020003a5, 0x06002830, 0x060028bd, 0x02000037)
            && !ReviewedProjectScopeTokens(0x020003a6, 0x06002830, 0x060028bd, 0x02000037) && !ReviewedProjectScopeTokens(0x020003a5, 0x06002831, 0x060028bd, 0x02000037)
            && !ReviewedProjectScopeTokens(0x020003a5, 0x06002830, 0x060028be, 0x02000037) && !ReviewedProjectScopeTokens(0x020003a5, 0x06002830, 0x060028bd, 0x02000038), "OWNED_PROJECT_SCOPE_BEFORE_EFFECTS");
        ProjectScopeApi scope = ScopedProjectApi(type("ScopedProjectFixture"), factory, contract, style, storeType);
        EmptyContainerApi container = EmptyApi(type("ScopedBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture"));
        TraceRegistrationApi registration = RegistrationApi(container, type("ScopedRegistrationExtensionsFixture"), type("IRegistrationFixture`3"), type("RegistrationDataFixture"), type("RegistrationStyleFixture"), trace.Contract);
        ConstructorInfo factoryCtor = FactoryCtor(factory, container.Context, type("IDataDependencyFixture")), idCtor = id.GetConstructor(new Type[] { typeof(uint) });
        MethodInfo idGetter = id.GetMethod("get_ProjectNumber"), storeGetter = storeType.GetMethod("get_Id");
        IdSignature(id, idCtor, idGetter); StoreIdSignature(storeType, storeGetter, id);
        object selectedStyle = Enum.Parse(style, "ThreeLevel", false);
        Func<ProjectStoreIdProxy> makeStore = delegate { object value = CreateFixtureId(id, idCtor, idGetter); return new ProjectStoreIdProxy(storeType, storeGetter, value, Record); };
        Record("OWNED_PROJECT_SCOPE_API_PASS negatives=" + negatives + " token-negatives=4 before-effects=True public-dispose-map=True factory-assignability=True original=NOT_RUN");
        for (int i = 0; i < ContainerNames.Length; i++) {
            string path = Path.Combine(stage, ContainerNames[i] + ".dll");
            ProbeFiles.Require(ContainerReferencePolicy("original-project-genuine-factory", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("owned-project-genuine-factory", true, ContainerIdentities[i], path, false)
                && !ContainerReferencePolicy("original-project-genuine-factory", false, ContainerIdentities[i], path, false), "OWNED_PROJECT_SCOPE_PROFILE");
        }
        ProbeFiles.Require(AutofacResourcePolicy("original-project-genuine-factory", true, true, true, AutofacResourceEnGb, true, false, 1)
            && !AutofacResourcePolicy("original-project-genuine-factory", true, false, true, AutofacResourceEnGb, true, false, 1)
            && !AutofacResourcePolicy("owned-project-genuine-factory", true, true, true, AutofacResourceEnGb, true, false, 1), "OWNED_PROJECT_SCOPE_RESOURCE_PHASE");
        foreach (string behavior in new string[] { "return", "project-throw", "project-dispose-throw", "container-dispose-throw", "trace-dispose-throw", "project-store-caught", "project-provider-caught", "container-store-caught", "container-provider-caught", "trace-store-caught", "trace-provider-caught" }) {
            reset(behavior); var store = makeStore(); var provider = new ProjectDenyProxy(type("IDataDependencyFixture"), Record); bool failed = false;
            try { WithRegisteredTrace(trace, container, registration, factoryCtor, provider, delegate(object actual) { WithProjectScope(scope, actual, selectedStyle, store, null); }, store); }
            catch (Exception error) { ProbeFiles.Require(error is IOException || error is NotSupportedException || error is TargetInvocationException || error is InvalidOperationException, "OWNED_PROJECT_SCOPE_EXPECTED_ERROR"); failed = true; }
            ProbeFiles.Require(failed == (behavior != "return") && count(scopeControl, "Constructors") == 1 && count(scopeControl, "Disposals") == (behavior == "project-throw" ? 0 : 1)
                && count(scopeControl, "IdConstructors") == 1 && count(scopeControl, "IdGetters") == 1 && store.FixtureCalls == 2
                && count(emptyControl, "Factories") == 1 && count(emptyControl, "Disposals") == 1 && count(traceControl, "Disposals") == 1 && count(traceControl, "Uses") == 0,
                "OWNED_PROJECT_SCOPE_LIFETIME_COUNTS");
            ProbeFiles.Require(store.DeniedRequests == (behavior.EndsWith("-store-caught", StringComparison.Ordinal) ? 1 : 0)
                && provider.Requests == (behavior.EndsWith("-provider-caught", StringComparison.Ordinal) ? 1 : 0), "OWNED_PROJECT_SCOPE_FINAL_DENIAL_COUNTS");
            string order = (string)registrationControl.GetField("Order").GetValue(null);
            ProbeFiles.Require(order == "trace-ctor>builder>register>external>build>factory>project>" + (behavior == "project-throw" ? "" : "project-dispose>") + "container-dispose>trace-dispose>", "OWNED_PROJECT_SCOPE_DISPOSAL_ORDER");
            Record("OWNED_PROJECT_SCOPE_CASE behavior=" + behavior + " expected-failure=" + failed + " project-dispose=" + count(scopeControl, "Disposals") + " container-dispose=1 trace-dispose=1 provider-denied=" + provider.Requests + " store-denied=" + store.DeniedRequests);
        }
        // Six modeled refusal controls share one extra owner lifetime to keep the existing 64KiB log cap.
        reset("return"); var guardedStore = makeStore(); var guardedProvider = new ProjectDenyProxy(type("IDataDependencyFixture"), Record);
        WithRegisteredTrace(trace, container, registration, factoryCtor, guardedProvider, delegate(object actual) {
            foreach (string flag in new string[] { "unexpected", "rejected" }) foreach (string phase in new string[] { "PRE_CTOR", "POST_CTOR", "POST_DISPOSE" }) {
                int beforeCtor = count(scopeControl, "Constructors"), beforeDispose = count(scopeControl, "Disposals"); bool failed = false;
                try { WithProjectScope(scope, actual, selectedStyle, guardedStore, delegate(string point) { if (point == phase) TraceDependencyGuard(flag == "unexpected", flag == "rejected", point); }); }
                catch (IOException) { failed = true; }
                ProbeFiles.Require(failed && count(scopeControl, "Constructors") - beforeCtor == (phase == "PRE_CTOR" ? 0 : 1)
                    && count(scopeControl, "Disposals") - beforeDispose == (phase == "PRE_CTOR" ? 0 : 1), "OWNED_PROJECT_SCOPE_MODELED_PHASE");
                Record("OWNED_PROJECT_SCOPE_CASE behavior=modeled-" + flag + "-" + phase + " expected-failure=True production-latches-never-reset=True");
            }
        }, guardedStore);
        ProbeFiles.Require(count(scopeControl, "Constructors") == 4 && count(scopeControl, "Disposals") == 4 && guardedStore.FixtureCalls == 8 && guardedStore.DeniedRequests == 0
            && guardedProvider.Requests == 0 && count(scopeControl, "IdConstructors") == 1 && count(scopeControl, "IdGetters") == 1, "OWNED_PROJECT_SCOPE_GROUPED_GUARDS");
        Record("OWNED_PROJECT_SCOPE_PASS cases=17 shape-negatives=7 token-negatives=4 modeled-guards=6 stable-id=True actual-factory-reference=True disposal-order=project-container-trace caught-store-provider-all-cleanup-scopes=True original=NOT_RUN qualification=NOT_ESTABLISHED");
    }
    static void OwnedProjectResourcePoint(string point)
    {
        ProbeFiles.Require(ProjectResourcesOwned, "OWNED_RESOURCE_POINT_MODE");
        int expected = point == "factory" ? 1 : (point == "project" || point == "project-after-getter") ? 2 : 0;
        ProbeFiles.Require(projectResourceWindow == expected && ((point != "getter" && point != "provider") || projectResourceCallbackSuspended), "OWNED_RESOURCE_POINT_WINDOW:" + point);
        Record("OWNED_RESOURCE_POINT phase=" + point + " window=" + projectResourceWindow + " callback-suspended=" + projectResourceCallbackSuspended + " aggregate=" + projectResourceProbeCount);
        bool selected = (ownedResourceCase == point && point != "factory") || (ownedResourceCase == "factory" && point == "provider")
            || (point == "factory" && (ownedResourceCase == "nested-window" || ownedResourceCase == "nested-budget" || ownedResourceCase == "off-thread"));
        if (!selected) return;
        bool refused = false;
        if (ownedResourceCase == "off-thread") {
            var thread = new Thread(delegate() {
                try { WithProjectResourceWindow(2, delegate { throw new InvalidOperationException("OWNED_OFF_THREAD_ACTION_EXECUTED"); }); }
                catch (IOException) { refused = true; }
            });
            thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000), "OWNED_RESOURCE_THREAD_TIMEOUT");
        } else {
            try {
                if (ownedResourceCase == "nested-window") WithProjectResourceWindow(2, delegate { throw new InvalidOperationException("OWNED_NESTED_ACTION_EXECUTED"); });
                else if (ownedResourceCase == "nested-budget") WithProjectResourceBudget(delegate { throw new InvalidOperationException("OWNED_NESTED_BUDGET_EXECUTED"); });
                else Resolve(null, new ResolveEventArgs(AutofacResourceEnGb)); // Fixture owner cannot satisfy the original pin.
            } catch (IOException) { refused = true; }
        }
        ProbeFiles.Require(refused && rejected, "OWNED_RESOURCE_REFUSAL_NOT_STICKY");
        Record("OWNED_RESOURCE_REFUSAL case=" + ownedResourceCase + " caught=True sticky=True actual-owner=NOT_PROVIDED original=NOT_RUN");
    }
    static void OwnedProjectResourcePolicy()
    {
        const string selected = "original-project-genuine-factory-resources";
        int positives = 0, negatives = 0;
        foreach (int window in new int[] { 1, 2 }) foreach (string request in new string[] { AutofacResourceEnGb, AutofacResourceEn }) foreach (bool requesterNull in new bool[] { true, false }) {
            foreach (int count in new int[] { 1, 8 }) { ProbeFiles.Require(ProjectResourcePolicy(selected, true, true, window, false, true, request, requesterNull, !requesterNull, count), "OWNED_PROJECT_RESOURCE_POSITIVE"); positives++; }
            foreach (string wrong in new string[] { request.Replace("Autofac.resources", "Owned.resources"), request.Replace("8.2.0.0", "8.2.0.1"), request.Replace("17863af14b0044da", "17863af14b0044db"), request.Replace("Culture=en-GB", "Culture=en-US").Replace("Culture=en,", "Culture=neutral,"), request + ", Retargetable=Yes", request.Replace("Culture=en-GB", "Culture=en-gb").Replace("Culture=en,", "Culture=EN,") }) {
                ProbeFiles.Require(!ProjectResourcePolicy(selected, true, true, window, false, true, wrong, requesterNull, !requesterNull, 1), "OWNED_PROJECT_RESOURCE_IDENTITY"); negatives++;
            }
        }
        foreach (string other in new string[] { "original-project-genuine-factory", "owned-project-genuine-factory", "owned-project-genuine-factory-resources", "original-factory-empty-container", "original-factory-registered-trace", "original-project", "original-trace-encryptor", "" }) {
            ProbeFiles.Require(!ProjectResourcePolicy(other, true, true, 1, false, true, AutofacResourceEn, true, false, 1), "OWNED_PROJECT_RESOURCE_MODE"); negatives++;
        }
        foreach (int window in new int[] { -1, 0, 3 }) { ProbeFiles.Require(!ProjectResourcePolicy(selected, true, true, window, false, true, AutofacResourceEn, true, false, 1), "OWNED_PROJECT_RESOURCE_WINDOW"); negatives++; }
        foreach (int count in new int[] { -1, 0, 9, 10 }) { ProbeFiles.Require(!ProjectResourcePolicy(selected, true, true, 1, false, true, AutofacResourceEn, true, false, count), "OWNED_PROJECT_RESOURCE_COUNT"); negatives++; }
        ProbeFiles.Require(!ProjectResourcePolicy(selected, false, true, 1, false, true, AutofacResourceEn, true, false, 1)
            && !ProjectResourcePolicy(selected, true, false, 1, false, true, AutofacResourceEn, true, false, 1)
            && !ProjectResourcePolicy(selected, true, true, 1, true, true, AutofacResourceEn, true, false, 1)
            && !ProjectResourcePolicy(selected, true, true, 1, false, false, AutofacResourceEn, true, false, 1)
            && !ProjectResourcePolicy(selected, true, true, 1, false, true, AutofacResourceEn, false, false, 1)
            && !ProjectResourcePolicy(selected, true, true, 1, false, true, AutofacResourceEn, true, true, 1), "OWNED_PROJECT_RESOURCE_FACTS"); negatives += 6;
        ProbeFiles.Require(AutofacResourcePolicy("original-project-genuine-factory", true, true, true, AutofacResourceEn, true, false, 8)
            && !AutofacResourcePolicy(selected, true, true, true, AutofacResourceEn, true, false, 1), "OWNED_PROJECT_RESOURCE_LEGACY_POLICY");
        WithProjectResourceBudget(delegate {
            WithProjectResourceWindow(1, delegate { for (int i = 0; i < 3; i++) ProbeFiles.Require(ConsumeProjectResourceProbe(true), "OWNED_RESOURCE_FACTORY_CAPACITY"); });
            ProbeFiles.Require(projectResourceWindow == 0 && projectResourceProbeCount == 3, "OWNED_RESOURCE_BETWEEN_CONSTRUCTORS");
            WithProjectResourceWindow(2, delegate {
                for (int i = 0; i < 5; i++) ProbeFiles.Require(ConsumeProjectResourceProbe(true), "OWNED_RESOURCE_PROJECT_CAPACITY");
                ProbeFiles.Require(projectResourceProbeCount == 8, "OWNED_RESOURCE_EXACT_EIGHTH");
                int prior = SuspendProjectResourceCallback();
                try { ProbeFiles.Require(projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == 8, "OWNED_RESOURCE_GETTER_SUSPENDED"); }
                finally { RestoreProjectResourceCallback(prior); }
                ProbeFiles.Require(projectResourceWindow == 2 && !projectResourceCallbackSuspended && projectResourceProbeCount == 8, "OWNED_RESOURCE_GETTER_RESTORED");
                for (int i = 0; i < 3; i++) ProbeFiles.Require(!ConsumeProjectResourceProbe(true) && projectResourceProbeCount == 9, "OWNED_RESOURCE_NINTH_SATURATES");
                ProbeFiles.Require(!AllowAutofacResourceNull(new ResolveEventArgs(AutofacResourceEn)), "OWNED_RESOURCE_ACTUAL_OWNER_NOT_FAKE");
            });
            bool otherBudget = true; int otherWindow = -1, otherCount = -1;
            var thread = new Thread(delegate() { otherBudget = projectResourceBudgetActive; otherWindow = projectResourceWindow; otherCount = projectResourceProbeCount; });
            thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000) && !otherBudget && otherWindow == 0 && otherCount == 0, "OWNED_RESOURCE_THREAD_LOCAL");
        });
        ProbeFiles.Require(!projectResourceBudgetActive && projectResourceWindow == 0 && projectResourceProbeCount == 0, "OWNED_RESOURCE_BUDGET_RETURN_CLEAR");
        bool threw = false;
        try { WithProjectResourceBudget(delegate { WithProjectResourceWindow(2, delegate { throw new NotSupportedException("OWNED_RESOURCE_WINDOW_THROW"); }); }); }
        catch (NotSupportedException) { threw = true; }
        ProbeFiles.Require(threw && !projectResourceBudgetActive && projectResourceWindow == 0 && projectResourceProbeCount == 0 && !projectResourceCallbackSuspended, "OWNED_RESOURCE_THROW_CLEAR");
        Record("OWNED_PROJECT_RESOURCE_POLICY_PASS positives=" + positives + " negatives=" + negatives + " shared8=3+5 ninth-saturates=True thread-local=True getter-restores=True actual-owner=NOT_PROVIDED modeled=True original=NOT_RUN");
    }
    static void OwnedProjectResources()
    {
        ProbeFiles.Require(ProjectResourcesOwned, "OWNED_PROJECT_RESOURCE_MODE");
        if (ownedResourceCase == null) OwnedProjectResourcePolicy();
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type control = type("ProjectScopeControl"), traceControl = type("TraceLifetimeControl");
        control.GetMethod("Reset").Invoke(null, new object[] { ownedResourceCase == "error" ? "project-throw" : ownedResourceCase == "factory" ? "resource-provider" : "return" });
        type("RegistrationControl").GetMethod("Reset").Invoke(null, new object[] { "return" }); type("EmptyControlFixture").GetMethod("Reset").Invoke(null, new object[] { "return" });
        traceControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
        control.GetField("ResourceProbe").SetValue(null, new Action<string>(OwnedProjectResourcePoint));
        traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { type("RegistrationControl").GetMethod("Event").Invoke(null, new object[] { "trace-ctor" }); OwnedProjectResourcePoint("trace-constructor"); }));
        traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { type("RegistrationControl").GetMethod("Event").Invoke(null, new object[] { "trace-dispose" }); OwnedProjectResourcePoint("trace-dispose"); }));
        Type factory = type("ScopedGenuineFactoryFixture"), contract = type("IFactoryFixture"), style = type("GroupAddressStyleFixture"), storeType = type("IScopedProjectStoreFixture"), id = type("ScopedIdFixture");
        ProjectScopeApi scope = ScopedProjectApi(type("ScopedProjectFixture"), factory, contract, style, storeType);
        EmptyContainerApi container = EmptyApi(type("ScopedBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture"));
        TraceRegistrationApi registration = RegistrationApi(container, type("ScopedRegistrationExtensionsFixture"), type("IRegistrationFixture`3"), type("RegistrationDataFixture"), type("RegistrationStyleFixture"), trace.Contract);
        ConstructorInfo factoryCtor = FactoryCtor(factory, container.Context, type("IDataDependencyFixture")), idCtor = id.GetConstructor(new Type[] { typeof(uint) });
        MethodInfo idGetter = id.GetMethod("get_ProjectNumber"), storeGetter = storeType.GetMethod("get_Id");
        object selectedStyle = Enum.Parse(style, "ThreeLevel", false), value = CreateFixtureId(id, idCtor, idGetter);
        ProjectStoreIdProxy store = null; bool callbackReentryAttempted = false;
        store = new ProjectStoreIdProxy(storeType, storeGetter, value, delegate(string message) {
            if (message.StartsWith("PROJECT_SERVICE_FIXTURE", StringComparison.Ordinal)) {
                OwnedProjectResourcePoint("getter");
                if (ownedResourceCase == "nested-callback" && !callbackReentryAttempted) {
                    callbackReentryAttempted = true; int before = projectResourceProbeCount; bool refused = false;
                    try { storeGetter.Invoke(store.GetTransparentProxy(), new object[0]); }
                    catch (TargetInvocationException error) {
                        var denial = error.InnerException as IOException;
                        refused = denial != null && denial.Message == "PROJECT_RESOURCE_NESTED_CALLBACK";
                        if (!refused) throw;
                    } catch (IOException error) { refused = error.Message == "PROJECT_RESOURCE_NESTED_CALLBACK"; if (!refused) throw; }
                    ProbeFiles.Require(refused && rejected && projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == before, "OWNED_NESTED_CALLBACK_STATE");
                    Record("OWNED_RESOURCE_REFUSAL case=nested-callback caught=True sticky=True diagnostic=PROJECT_RESOURCE_NESTED_CALLBACK outer-window=0 outer-suspended=True aggregate-unchanged=True attempts=1 original=NOT_RUN");
                }
            }
            Record(message);
        });
        var provider = new ProjectDenyProxy(type("IDataDependencyFixture"), delegate(string message) { if (message.StartsWith("PROJECT_SERVICE_DENIED", StringComparison.Ordinal)) OwnedProjectResourcePoint("provider"); Record(message); });
        WithProjectResourceBudget(delegate { WithRegisteredTrace(trace, container, registration, factoryCtor, provider, delegate(object actual) { WithProjectScope(scope, actual, selectedStyle, store, null); }, store); });
        ProbeFiles.Require(ownedResourceCase == null && !rejected && !unexpected && !projectResourceBudgetActive && projectResourceWindow == 0 && projectResourceProbeCount == 0
            && store.FixtureCalls == 2 && store.DeniedRequests == 0 && provider.Requests == 0, "OWNED_RESOURCE_LIFETIME_RESULT");
        Record("OWNED_PROJECT_RESOURCE_LIFETIME_PASS old-modes=UNCHANGED phase=exact-invoke-only callback-suspended=True cleanup-no-window=True original=NOT_RUN qualification=NOT_ESTABLISHED");
    }
    static MethodInfo[] ownedUndoMethods;
    static bool ownedUndoReentry;
    sealed class InvalidUndoMessage : IMessage
    { public System.Collections.IDictionary Properties { get { return new System.Collections.Hashtable(); } } }
    static void OwnedUndoRecord(string message)
    {
        ProbeFiles.Require(UndoDeniedOwned && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_UNDO_CALLBACK_EXCLUDED");
        if (!ownedUndoReentry && activeUndo != null && (ownedUndoCase == "nested" || ownedUndoCase == "resource")) {
            ownedUndoReentry = true; bool refused = false;
            try { if (ownedUndoCase == "nested") activeUndo.Invoke(null); else Resolve(null, new ResolveEventArgs(AutofacResourceEnGb)); }
            catch (IOException) { refused = true; }
            ProbeFiles.Require(refused && rejected && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_UNDO_NESTED_REFUSAL");
            Record("OWNED_UNDO_CALLBACK_REFUSAL case=" + ownedUndoCase + " caught=True sticky=True window=0 attempts=1");
        }
        Record("OWNED_UNDO_CALLBACK window=0 suspended=True " + message);
    }
    static void OwnedUndoPoint(string point)
    {
        ProbeFiles.Require(UndoDeniedOwned, "OWNED_UNDO_POINT_MODE");
        int expected = point == "factory" ? 1 : point == "project" ? 2 : 0;
        ProbeFiles.Require(projectResourceWindow == expected && !projectResourceCallbackSuspended, "OWNED_UNDO_PHASE_WINDOW:" + point);
        Record("OWNED_UNDO_POINT phase=" + point + " window=" + projectResourceWindow + " denied=" + (activeUndo == null ? 0 : activeUndo.Requests));
        bool selected = ownedUndoCase == point || (ownedUndoCase == "external" && point == "undo-external")
            || (point == "project" && Array.IndexOf(new string[] { "get", "new", "root", "invalid", "saturation", "nested", "resource" }, ownedUndoCase) >= 0);
        if (!selected || activeUndo == null) return;
        int before = projectResourceProbeCount, count = ownedUndoCase == "saturation" ? 20 : 1;
        for (int i = 0; i < count; i++) {
            bool denied = false;
            try {
                if (ownedUndoCase == "invalid" || ownedUndoCase == "saturation") activeUndo.Invoke(i % 2 == 0 ? null : new InvalidUndoMessage());
                else {
                    int slot = ownedUndoCase == "get" ? 0 : ownedUndoCase == "new" ? 1 : 2;
                    ownedUndoMethods[slot].Invoke(activeUndo.GetTransparentProxy(), slot == 0 ? new object[] { Guid.Empty } : new object[0]);
                }
            } catch (TargetInvocationException error) { denied = error.InnerException is NotSupportedException; if (!denied) throw; }
            catch (NotSupportedException) { denied = true; }
            ProbeFiles.Require(denied && projectResourceWindow == expected && !projectResourceCallbackSuspended && projectResourceProbeCount == before, "OWNED_UNDO_DENIAL_RESTORES");
        }
        ProbeFiles.Require(activeUndo.Requests == (ownedUndoCase == "saturation" ? 17 : ownedUndoCase == "nested" ? 2 : 1), "OWNED_UNDO_STICKY_COUNT");
        Record("OWNED_UNDO_DENIAL case=" + ownedUndoCase + " phase=" + point + " caught=True counter-sticky=True window-restored=" + expected + " dependency-flags=" + (unexpected || rejected));
    }
    static void OwnedUndoProvider()
    {
        ProbeFiles.Require(UndoDeniedOwned, "OWNED_UNDO_MODE");
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type contract = type("IUndoDeniedFixture"), opaque = type("IOpaqueUndoFixture"), control = type("UndoOwnedControl"), traceControl = type("TraceLifetimeControl");
        ownedUndoMethods = UndoContract(contract, opaque);
        if (ownedUndoCase == null) {
            int negatives = 0;
            foreach (string name in new string[] { "IUndoWrongGuidFixture", "IUndoWrongReturnFixture", "IUndoExtraFixture", "IUndoInheritedFixture", "IUndoGenericFixture`1", "IUndoHiddenFixture", "IUndoWrongNameFixture", "IUndoGenericMethodFixture", "IUndoWrongGetterFixture", "UndoProjectFixture" }) {
                bool failed = false; try { UndoContract(type(name), opaque); } catch (IOException) { failed = true; }
                ProbeFiles.Require(failed, "OWNED_UNDO_CONTRACT_NEGATIVE:" + name); negatives++;
            }
            ProbeFiles.Require(ReviewedUndoTokens(0x02000121, 0x06000af3, 0x06000af4, 0x06000af5)
                && !ReviewedUndoTokens(0x02000122, 0x06000af3, 0x06000af4, 0x06000af5)
                && !ReviewedUndoTokens(0x02000121, 0x06000af2, 0x06000af4, 0x06000af5)
                && !ReviewedUndoTokens(0x02000121, 0x06000af3, 0x06000af6, 0x06000af5)
                && !ReviewedUndoTokens(0x02000121, 0x06000af3, 0x06000af4, 0x06000af6), "OWNED_UNDO_TOKEN_POLICY");
            var unit = new UndoProviderDenyProxy(contract, OwnedUndoRecord);
            foreach (MethodInfo method in ownedUndoMethods) {
                bool denied = false; try { method.Invoke(unit.GetTransparentProxy(), method.GetParameters().Length == 1 ? new object[] { Guid.Empty } : new object[0]); }
                catch (TargetInvocationException error) { denied = error.InnerException is NotSupportedException; }
                ProbeFiles.Require(denied, "OWNED_UNDO_ALL_THREE_UNSUPPORTED");
            }
            for (int i = 0; i < 20; i++) { bool denied = false; try { unit.Invoke(i % 2 == 0 ? null : new InvalidUndoMessage()); } catch (NotSupportedException) { denied = true; } ProbeFiles.Require(denied, "OWNED_UNDO_INVALID_DENIED"); }
            var legacy = new ProjectDenyProxy(contract, Record); try { legacy.Invoke(null); } catch (NotSupportedException) { }
            ProbeFiles.Require(unit.Requests == 17 && legacy.Requests == 0 && !unexpected && !rejected, "OWNED_UNDO_COUNTER_AND_LEGACY");
            var faulty = new UndoProviderDenyProxy(contract, delegate(string ignored) { throw new IOException("OWNED_UNDO_RECORDER_FAULT"); });
            bool fault = false; try { faulty.Invoke(null); } catch (IOException error) { fault = error.Message == "OWNED_UNDO_RECORDER_FAULT"; }
            ProbeFiles.Require(fault && faulty.Requests == 1 && projectResourceWindow == 0 && !projectResourceCallbackSuspended, "OWNED_UNDO_FAULT_COUNTED_RESTORED");
            foreach (int window in new int[] { 1, 2 }) foreach (string resource in new string[] { AutofacResourceEnGb, AutofacResourceEn })
                ProbeFiles.Require(ProjectResourcePolicy("original-project-undo-provider-denied", true, true, window, false, true, resource, true, false, 8)
                    && !ProjectResourcePolicy("original-project-undo-provider-denied", true, true, window, false, true, resource, true, false, 9)
                    && !ProjectResourcePolicy("owned-project-undo-provider-denied", true, true, window, false, true, resource, true, false, 1)
                    && !ProjectResourcePolicy("original-project-undo-provider-denied", true, true, window, true, true, resource, true, false, 1), "OWNED_UNDO_EXACT_RESOURCE_POLICY");
            OwnedProjectResourcePolicy(); // Same production budget/window helpers under the new owned mode; no pinned owner supplied.
            Record("OWNED_UNDO_API_PASS shape-negatives=" + negatives + " token-negatives=4 callbacks=3 invalid-null-counted=True saturated=17 legacy-invalid-count=0 before-effects=True");
        }
        Type factoryType = type("UndoFactoryFixture"), factoryContract = type("IFactoryFixture"), storeType = type("IScopedProjectStoreFixture"), style = type("GroupAddressStyleFixture"), id = type("ScopedIdFixture");
        ProjectScopeApi scope = ScopedProjectApi(type("UndoProjectFixture"), factoryType, factoryContract, style, storeType);
        EmptyContainerApi container = EmptyApi(type("UndoBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture"));
        Type extensions = type("UndoRegistrationExtensionsFixture"), open = type("IRegistrationFixture`3"), data = type("RegistrationDataFixture"), registrationStyle = type("RegistrationStyleFixture");
        TraceRegistrationApi traceRegistration = RegistrationApi(container, extensions, open, data, registrationStyle, trace.Contract), undoRegistration = RegistrationApi(container, extensions, open, data, registrationStyle, contract);
        ConstructorInfo factory = FactoryCtor(factoryType, container.Context, type("IDataDependencyFixture"));
        control.GetField("Callback").SetValue(null, new Action<string>(OwnedUndoPoint));
        control.GetField("ThrowProject").SetValue(null, ownedUndoCase == "error"); control.GetField("Failure").SetValue(null, ownedUndoCase);
        traceControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
        traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-ctor" }); }));
        traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-dispose" }); }));
        object value = CreateFixtureId(id, id.GetConstructor(new Type[] { typeof(uint) }), id.GetMethod("get_ProjectNumber"));
        var store = new ProjectStoreIdProxy(storeType, storeType.GetMethod("get_Id"), value, Record); var kim = new ProjectDenyProxy(type("IDataDependencyFixture"), Record);
        var undo = new UndoProviderDenyProxy(contract, OwnedUndoRecord); object selectedStyle = Enum.Parse(style, "ThreeLevel", false);
        WithProjectResourceBudget(delegate { WithUndoProvider(trace, container, traceRegistration, undoRegistration, factory, kim, store, undo,
            delegate(object actual) { WithProjectScope(scope, actual, selectedStyle, store, null); }); });
        ProbeFiles.Require(ownedUndoCase == null && undo.Requests == 0 && kim.Requests == 0 && store.DeniedRequests == 0 && store.FixtureCalls == 2
            && Object.ReferenceEquals(control.GetField("Undo").GetValue(null), undo.GetTransparentProxy())
            && (int)control.GetField("Registrations").GetValue(null) == 2 && (int)control.GetField("ExternalCalls").GetValue(null) == 2
            && (int)control.GetField("ProjectDisposals").GetValue(null) == 1 && (int)control.GetField("ContainerDisposals").GetValue(null) == 1
            && (int)traceControl.GetField("Disposals").GetValue(null) == 1, "OWNED_UNDO_LIFETIME_COUNTS");
        Record("OWNED_UNDO_LIFETIME_PASS exact-two-external=True original=NOT_RUN provider-value=NOT_PROVIDED qualification=NOT_ESTABLISHED order=" + control.GetField("Order").GetValue(null));
    }
    sealed class UndoFactoryRegistrationApi
    { internal MethodInfo Register, Single, External; internal Type Return, Contract, CallbackType; }
    static UndoFactoryRegistrationApi FactoryRegistrationApi(EmptyContainerApi container, Type extension, Type open, Type data, Type style, Type contract)
    {
        Assembly owner = container.Builder.Assembly;
        ProbeFiles.Require(UndoFactoryMode && extension != null && extension.IsPublic && extension.IsAbstract && extension.IsSealed && !extension.IsGenericType
            && open != null && open.IsPublic && open.IsInterface && open.IsGenericTypeDefinition && open.GetGenericArguments().Length == 3
            && data != null && data.IsPublic && !data.ContainsGenericParameters && style != null && style.IsPublic && !style.ContainsGenericParameters
            && extension.Assembly == owner && open.Assembly == owner && data.Assembly == owner && style.Assembly == owner && !owner.ReflectionOnly
            && PublicInterface(contract), "UNDO_FACTORY_REGISTRATION_TYPES");
        MethodInfo selected = null; int matches = 0;
        MethodInfo[] methods = extension.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(methods.Length <= 512, "UNDO_FACTORY_METHOD_BOUND");
        foreach (MethodInfo method in methods) {
            if (method.Name != "Register" || !method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 1) continue;
            Type generic = method.GetGenericArguments()[0]; ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 2 || parameters[0].ParameterType != container.Builder) continue;
            Type callback = parameters[1].ParameterType;
            if (!callback.IsGenericType || callback.GetGenericTypeDefinition() != typeof(Func<,>)) continue;
            Type[] parts = callback.GetGenericArguments(); if (parts[0] != container.Context || parts[1] != generic) continue;
            ProbeFiles.Require(method.DeclaringType == extension && method.CallingConvention == CallingConventions.Standard
                && generic.GenericParameterAttributes == GenericParameterAttributes.None && generic.GetGenericParameterConstraints().Length == 0
                && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == open, "UNDO_FACTORY_REGISTER_SIGNATURE");
            Type[] result = method.ReturnType.GetGenericArguments();
            ProbeFiles.Require(result[0] == generic && result[1] == data && result[2] == style, "UNDO_FACTORY_REGISTER_RETURN"); selected = method; matches++;
        }
        ProbeFiles.Require(matches == 1, "UNDO_FACTORY_REGISTER_OVERLOAD");
        Type closed = open.MakeGenericType(contract, data, style), callbackType = typeof(Func<,>).MakeGenericType(container.Context, contract);
        MethodInfo register = selected.MakeGenericMethod(contract);
        ProbeFiles.Require(register.ReturnType == closed && register.GetParameters()[1].ParameterType == callbackType && !register.ContainsGenericParameters, "UNDO_FACTORY_REGISTER_CLOSED");
        MethodInfo single = FactoryRegistrationMember(closed, "SingleInstance"), external = FactoryRegistrationMember(closed, "ExternallyOwned");
        return new UndoFactoryRegistrationApi { Register = register, Single = single, External = external, Return = closed, Contract = contract, CallbackType = callbackType };
    }
    static MethodInfo FactoryRegistrationMember(Type closed, string name)
    {
        MethodInfo method = closed.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
        ProbeFiles.Require(method != null && method.DeclaringType == closed && method.IsPublic && !method.IsStatic && !method.IsGenericMethod
            && method.GetParameters().Length == 0 && method.ReturnType == closed && method.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "UNDO_FACTORY_PUBLIC_OWNERSHIP_SHARING"); return method;
    }
    static void CheckUndoFactory(string phase)
    { if (UndoFactoryMode && activeFactory != null) ProbeFiles.Require(!activeFactory.Faulted && activeFactory.Entries <= 1 && activeFactory.Successes <= 1, "UNDO_FACTORY_DENIED_" + phase); }
    static void FinishUndoFactoryAudit(int entries, int successes, bool faulted)
    {
        try { Record("UNDO_FACTORY_FINAL_AUDIT after-all-managed-disposal=True entries=" + entries + " successes=" + successes + " faulted=" + faulted + " provider-value=NOT_PROVIDED"); }
        finally { if (faulted || entries > 1 || successes > 1) throw new NotSupportedException("UNDO_FACTORY_FINAL_DENIAL"); }
    }
    static void RegisterUndoFactory(UndoFactoryRegistrationApi api, object builder, UndoProviderFactoryHolder holder)
    {
        ProbeFiles.Require(UndoFactoryMode && holder.Callback.GetType() == api.CallbackType, "UNDO_FACTORY_REGISTRATION_CALLBACK");
        TraceDependencyGuard(unexpected, rejected, "UNDO_FACTORY_PRE_REGISTER");
        object registration = api.Register.Invoke(null, new object[] { builder, holder.Callback });
        ProbeFiles.Require(registration != null && api.Return.IsInstanceOfType(registration) && registration.GetType().Assembly == api.Return.Assembly
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration), "UNDO_FACTORY_REGISTRATION_RETURN");
        TraceDependencyGuard(unexpected, rejected, "UNDO_FACTORY_POST_REGISTER");
        ProbeFiles.Require(Object.ReferenceEquals(registration, api.Single.Invoke(registration, new object[0])), "UNDO_FACTORY_SINGLE_SAME_BUILDER");
        TraceDependencyGuard(unexpected, rejected, "UNDO_FACTORY_POST_SINGLE");
        ProbeFiles.Require(Object.ReferenceEquals(registration, api.External.Invoke(registration, new object[0])), "UNDO_FACTORY_EXTERNAL_SAME_BUILDER");
        TraceDependencyGuard(unexpected, rejected, "UNDO_FACTORY_POST_EXTERNAL");
        Record("UNDO_FACTORY_REGISTER_END exact-two-registrations=True sharing=SingleInstance external-ownership=True before-build=True provider-value=NOT_PROVIDED");
    }
    static void WithUndoFactoryProvider(TraceLifetimeApi trace, EmptyContainerApi container, TraceRegistrationApi traceRegistration, UndoFactoryRegistrationApi factoryRegistration,
        ConstructorInfo factory, ProjectDenyProxy kim, ProjectStoreIdProxy store, UndoProviderDenyProxy undo, UndoProviderFactoryHolder holder, Action<object> projectUse)
    {
        ProbeFiles.Require(UndoFactoryMode && activeUndo == null && activeFactory == null && undo != null && holder != null, "UNDO_FACTORY_LIFETIME_CONTEXT");
        activeUndo = undo; activeFactory = holder; holder.Begin(); object scopedFactory = null;
        try {
            try { WithTraceLifetimeCore(trace, null, delegate(object instance) {
                WithEmptyContainerCore(container, delegate(object context) {
                    InvokeFactoryForProject(factory, context, kim, delegate(object actual) { scopedFactory = actual; projectUse(actual); });
                }, null, delegate(object builder) { RegisterTrace(traceRegistration, builder, instance); RegisterUndoFactory(factoryRegistration, builder, holder); });
            }); }
            catch { if (UndoFactoryOwned) OwnedFactoryPoint("error"); throw; }
        } finally {
            holder.Close();
            try {
                if (UndoFactoryOwned) { OwnedFactoryPoint("final-audit"); OwnedFactoryPoint("late"); }
                FinishUndoFactoryAudit(holder.Entries, holder.Successes, holder.Faulted);
            } finally {
                try { FinishUndoProviderAudit(undo.Requests); }
                finally {
                    try { Record("UNDO_FACTORY_OTHER_FINAL_AUDIT kim-denied=" + kim.Requests + " store-denied=" + store.DeniedRequests);
                        ProbeFiles.Require(kim.Requests == 0 && store.DeniedRequests == 0, "UNDO_FACTORY_OTHER_LIFETIME_DENIAL"); }
                    finally { GC.KeepAlive(scopedFactory); GC.KeepAlive(store); GC.KeepAlive(kim); GC.KeepAlive(undo); GC.KeepAlive(holder.Callback); GC.KeepAlive(holder); }
                }
            }
        }
    }
    static bool ownedFactoryReentry;
    static MethodInfo[] ownedFactoryProviderMethods;
    static void InvokeOwnedFactory(object context, bool wrongType)
    {
        ProbeFiles.Require(UndoFactoryOwned && activeFactory != null, "OWNED_FACTORY_INVOKE_MODE");
        if (wrongType) activeFactory.Provide<object>(context);
        else activeFactory.Callback.Method.Invoke(activeFactory, new object[] { context });
    }
    static void OwnedFactoryRecord(string message)
    {
        ProbeFiles.Require(UndoFactoryOwned && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_FACTORY_CALLBACK_EXCLUDED");
        if (ownedFactoryCase == "recorder") throw new IOException("OWNED_FACTORY_RECORDER_FAULT");
        if (!ownedFactoryReentry && ownedFactoryCase == "nested") {
            ownedFactoryReentry = true; int before = projectResourceProbeCount; bool refused = false;
            try { InvokeOwnedFactory(new object(), false); } catch (Exception) { refused = true; }
            ProbeFiles.Require(refused && activeFactory.Faulted && activeFactory.Entries == 2 && projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == before, "OWNED_FACTORY_NESTED_STICKY");
            Record("OWNED_FACTORY_NESTED_REFUSAL sticky=True window=0 suspended=True aggregate-unchanged=True");
        }
        if (ownedFactoryCase == "resource") Resolve(null, new ResolveEventArgs(AutofacResourceEnGb));
        Record("OWNED_FACTORY_CALLBACK_EXCLUDED window=0 suspended=True " + message);
    }
    static void OwnedFactoryProviderRecord(string message)
    { ProbeFiles.Require(UndoFactoryOwned && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_FACTORY_PROVIDER_EXCLUDED"); Record(message); }
    static void OwnedFactoryPoint(string point)
    {
        if (CacheOwned) { if (point != "final-audit" && point != "late") OwnedCachePoint(point); return; }
        ProbeFiles.Require(UndoFactoryOwned, "OWNED_FACTORY_POINT_MODE");
        int previous = projectResourceWindow, probes = projectResourceProbeCount;
        Record("OWNED_FACTORY_POINT phase=" + point + " window=" + previous + " entries=" + (activeFactory == null ? 0 : activeFactory.Entries));
        bool after = point == "after-activation" && Array.IndexOf(new string[] { "second", "saturation", "get", "new", "root", "nested", "recorder", "resource" }, ownedFactoryCase) >= 0;
        bool selected = after || ownedFactoryCase == point || (point == "factory" && Array.IndexOf(new string[] { "null", "wrong-type", "off-thread" }, ownedFactoryCase) >= 0);
        if (!selected || activeFactory == null || ownedFactoryCase == "project-activation") return;
        bool refused = false;
        try {
            if (after && activeFactory.Faulted && (ownedFactoryCase == "nested" || ownedFactoryCase == "recorder" || ownedFactoryCase == "resource")) refused = true;
            else if (after && (ownedFactoryCase == "get" || ownedFactoryCase == "new" || ownedFactoryCase == "root")) {
                int slot = ownedFactoryCase == "get" ? 0 : ownedFactoryCase == "new" ? 1 : 2;
                ownedFactoryProviderMethods[slot].Invoke(activeUndo.GetTransparentProxy(), slot == 0 ? new object[] { Guid.Empty } : new object[0]);
            } else if (ownedFactoryCase == "off-thread") {
                Exception failure = null; var thread = new Thread(new ThreadStart(delegate { try { InvokeOwnedFactory(new object(), false); } catch (Exception error) { failure = error; } }));
                thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000) && failure != null, "OWNED_FACTORY_OFF_THREAD_REFUSAL"); refused = true;
            } else {
                int count = ownedFactoryCase == "saturation" ? 20 : 1;
                for (int i = 0; i < count; i++) try { InvokeOwnedFactory(ownedFactoryCase == "null" || ownedFactoryCase == "factory" || ownedFactoryCase == "final-audit" ? null : new object(), ownedFactoryCase == "wrong-type"); }
                    catch (Exception) { refused = true; }
            }
        } catch (Exception) { refused = true; }
        ProbeFiles.Require(refused && projectResourceWindow == previous && !projectResourceCallbackSuspended && projectResourceProbeCount == probes, "OWNED_FACTORY_REFUSAL_RESTORED");
        ProbeFiles.Require(activeFactory.Faulted || activeUndo.Requests != 0, "OWNED_FACTORY_REFUSAL_STICKY");
        Record("OWNED_FACTORY_REFUSAL case=" + ownedFactoryCase + " phase=" + point + " caught=True entries=" + activeFactory.Entries + " faulted=" + activeFactory.Faulted
            + " successes=" + activeFactory.Successes + " window-restored=" + previous + " dependency-flags=" + (unexpected || rejected));
    }
    static void OwnedUndoFactoryProvider()
    {
        ProbeFiles.Require(UndoFactoryOwned, "OWNED_UNDO_FACTORY_MODE");
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type contract = type("IUndoDeniedFixture"), opaque = type("IOpaqueUndoFixture"), control = type("UndoOwnedControl"), delivery = type("UndoFactoryOwnedControl"), traceControl = type("TraceLifetimeControl");
        ownedFactoryProviderMethods = UndoContract(contract, opaque);
        Type factoryType = type("UndoFactoryDeliveryFixture"), factoryContract = type("IFactoryFixture"), storeType = type("IScopedProjectStoreFixture"), style = type("GroupAddressStyleFixture"), id = type("ScopedIdFixture");
        ProjectScopeApi scope = ScopedProjectApi(type("UndoFactoryProjectFixture"), factoryType, factoryContract, style, storeType);
        EmptyContainerApi container = EmptyApi(type("UndoBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture"));
        Type extension = type("UndoFactoryRegistrationExtensionsFixture"), open = type("IUndoFactoryRegistrationFixture`3"), data = type("RegistrationDataFixture"), registrationStyle = type("RegistrationStyleFixture");
        UndoFactoryRegistrationApi api = FactoryRegistrationApi(container, extension, open, data, registrationStyle, contract);
        if (ownedFactoryCase == null) {
            int negatives = 0;
            foreach (string name in new string[] { "UndoFactoryWrongContextFixture", "UndoFactoryWrongArityFixture", "UndoFactoryWrongConstraintFixture", "UndoFactoryWrongReturnFixture" }) {
                bool refused = false; try { FactoryRegistrationApi(container, type(name), open, data, registrationStyle, contract); } catch (IOException) { refused = true; }
                ProbeFiles.Require(refused, "OWNED_FACTORY_API_NEGATIVE"); negatives++;
            }
            foreach (string name in new string[] { "IUndoFactoryWrongSingleFixture`3", "IUndoFactoryWrongExternalFixture`3" }) {
                bool refused = false; try { FactoryRegistrationMember(type(name).MakeGenericType(contract, data, registrationStyle), name.Contains("Single") ? "SingleInstance" : "ExternallyOwned"); } catch (IOException) { refused = true; }
                ProbeFiles.Require(refused, "OWNED_FACTORY_MEMBER_NEGATIVE"); negatives++;
            }
            var testProxy = new UndoProviderDenyProxy(contract, Record);
            bool invalid = false; try { new UndoProviderFactoryHolder(contract, testProxy.GetTransparentProxy(), typeof(Func<object, object>), Record); } catch (IOException) { invalid = true; }
            ProbeFiles.Require(invalid && testProxy.Requests == 0, "OWNED_FACTORY_BIND_NEGATIVE"); negatives++;
            Record("OWNED_FACTORY_API_PASS negatives=" + negatives + " before-effects=True provider-calls=0 arbitrary-reflection=False");
        }
        TraceRegistrationApi traceRegistration = RegistrationApi(container, extension, type("IRegistrationFixture`3"), data, registrationStyle, trace.Contract);
        ConstructorInfo factory = FactoryCtor(factoryType, container.Context, type("IDataDependencyFixture"));
        control.GetField("Callback").SetValue(null, new Action<string>(OwnedFactoryPoint));
        control.GetField("ThrowProject").SetValue(null, ownedFactoryCase == "error");
        delivery.GetField("SkipActivation").SetValue(null, Array.IndexOf(new string[] { "null", "wrong-type", "off-thread", "no-activation", "final-audit" }, ownedFactoryCase) >= 0);
        delivery.GetField("ProjectActivation").SetValue(null, ownedFactoryCase == "project-activation");
        traceControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
        traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-ctor" }); }));
        traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-dispose" }); }));
        object value = CreateFixtureId(id, id.GetConstructor(new Type[] { typeof(uint) }), id.GetMethod("get_ProjectNumber"));
        var store = new ProjectStoreIdProxy(storeType, storeType.GetMethod("get_Id"), value, Record); var kim = new ProjectDenyProxy(type("IDataDependencyFixture"), Record);
        var undo = new UndoProviderDenyProxy(contract, OwnedFactoryProviderRecord); var holder = new UndoProviderFactoryHolder(contract, undo.GetTransparentProxy(), api.CallbackType, OwnedFactoryRecord);
        object selectedStyle = Enum.Parse(style, "ThreeLevel", false);
        WithProjectResourceBudget(delegate { WithUndoFactoryProvider(trace, container, traceRegistration, api, factory, kim, store, undo, holder,
            delegate(object actual) { WithProjectScope(scope, actual, selectedStyle, store, null); }); });
        int expected = ownedFactoryCase == "no-activation" ? 0 : 1;
        ProbeFiles.Require((ownedFactoryCase == null || ownedFactoryCase == "project-activation" || ownedFactoryCase == "no-activation") && !holder.Faulted && holder.Entries == expected && holder.Successes == expected
            && undo.Requests == 0 && kim.Requests == 0 && store.DeniedRequests == 0 && store.FixtureCalls == 2
            && (expected == 0 || Object.ReferenceEquals(control.GetField("Undo").GetValue(null), undo.GetTransparentProxy()))
            && (int)control.GetField("Registrations").GetValue(null) == 2 && (int)control.GetField("ExternalCalls").GetValue(null) == 2 && (int)delivery.GetField("SingleCalls").GetValue(null) == 1
            && (int)control.GetField("ProjectDisposals").GetValue(null) == 1 && (int)control.GetField("ContainerDisposals").GetValue(null) == 1 && (int)traceControl.GetField("Disposals").GetValue(null) == 1, "OWNED_FACTORY_LIFETIME_COUNTS");
        Record("OWNED_FACTORY_LIFETIME_PASS entries=" + holder.Entries + " successes=" + holder.Successes + " exact-two-external=True single-instance=True same-existing-proxy=True provider-value=NOT_PROVIDED order=" + control.GetField("Order").GetValue(null));
    }
    internal static bool ExactRuntimeMethod(MethodInfo method, MethodInfo expected)
    {
        if (method == null || expected == null || System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method)
            || method.GetType() != typeof(object).GetMethod("GetType").GetType()) return false;
        return expected.Equals(method) && method.Module == expected.Module && method.MetadataToken == expected.MetadataToken && method.DeclaringType == expected.DeclaringType;
    }
    internal static void CacheGetterSignature(Type owner, MethodInfo method, Type stream)
    {
        ProbeFiles.Require(CacheMode && PublicInterface(owner) && PublicInterface(stream) && owner.Assembly == stream.Assembly && method != null && method.DeclaringType == owner
            && method.Name == "get_LargeStringCache" && method.IsPublic && !method.IsStatic && method.IsSpecialName && !method.IsGenericMethod && !method.ContainsGenericParameters
            && method.ReturnType == stream && method.GetParameters().Length == 0 && method.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis) && (int)method.Attributes == 3526, "CACHE_GETTER_SIGNATURE");
        if (CacheOriginal) ProbeFiles.Require(owner.Assembly.FullName == OmIdentity && !owner.Assembly.ReflectionOnly && owner.MetadataToken == 0x02000057
            && (int)owner.Attributes == 1048737 && method.MetadataToken == 0x06000313, "CACHE_GETTER_ORIGINAL_TOKENS");
    }
    static MethodInfo[] StreamContract(Type type, bool original)
    {
        ProbeFiles.Require(CacheMode && PublicInterface(type) && type.BaseType == null && type.GetInterfaces().Length == 0, "STREAM_PUBLIC_ROOT");
        string[] names = { "get_Id", "get_Exists", "AsStream", "AsMultiWriteWithinOneTransactionStream", "Delete", "GetUtcFileTime", "set_Mask" };
        Type[] returns = { typeof(string), typeof(bool), typeof(Stream), typeof(Stream), typeof(void), typeof(long), typeof(void) };
        MethodInfo[] declared = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly), methods = new MethodInfo[7];
        ProbeFiles.Require(declared.Length == 7, "STREAM_EXACT_METHOD_COUNT");
        for (int i = 0; i < names.Length; i++) {
            Type[] parameters = i == 2 ? new Type[] { typeof(bool) } : i == 6 ? new Type[] { typeof(ushort) } : Type.EmptyTypes;
            MethodInfo method = type.GetMethod(names[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly, null, parameters, null);
            bool accessor = i == 0 || i == 1 || i == 6;
            ProbeFiles.Require(method != null && method.DeclaringType == type && method.IsPublic && !method.IsStatic && !method.IsGenericMethod && !method.ContainsGenericParameters
                && method.IsSpecialName == accessor && method.ReturnType == returns[i] && method.GetParameters().Length == parameters.Length
                && method.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis) && (int)method.Attributes == (accessor ? 3526 : 1478), "STREAM_METHOD_SIGNATURE");
            if (original) ProbeFiles.Require(method.MetadataToken == 0x06000366 + i, "STREAM_ORIGINAL_METHOD_TOKEN"); methods[i] = method;
        }
        if (original) ProbeFiles.Require(CacheOriginal && type.Assembly.FullName == OmIdentity && !type.Assembly.ReflectionOnly && type.MetadataToken == 0x02000061 && (int)type.Attributes == 1048737
            && typeof(Stream).Assembly.FullName == "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "STREAM_ORIGINAL_ROOT_TOKEN_IDENTITY");
        Record("STREAM_DECLARATIONS_READY methods=7 behavior=NOT_QUALIFIED metadata-sha256=734cc04300ee84a8a2de29a4cbef9b489cc948bad7e3184d29041174faa61b07"); return methods;
    }
    static void CheckCache(string phase)
    {
        if (!CacheMode || activeCache == null || activeStream == null) return;
        ProbeFiles.Require(!activeCache.State.Faulted && activeCache.State.Entries <= 16 && activeCache.State.Attempts <= 1 && activeCache.State.Replies <= 1, "CACHE_WRAPPER_DENIED_" + phase);
        ProbeFiles.Require(activeStream.Requests == 0, "STREAM_STORE_DENIED_" + phase);
    }
    static void FinishCacheAudit(int entries, int attempts, int replies, bool faulted, int streamDenied)
    {
        try { Record("CACHE_FINAL_AUDIT after-all-managed-disposal=True entries=" + entries + " attempts=" + attempts + " replies=" + replies + " faulted=" + faulted + " stream-denied=" + streamDenied + " values=NOT_PROVIDED"); }
        finally { if (faulted || entries > 16 || attempts > 1 || replies > 1 || streamDenied != 0) throw new NotSupportedException("CACHE_STREAM_FINAL_DENIAL"); }
    }
    static void WithCacheLifetime(LargeStringCacheStoreProxy store, StreamStoreDenyProxy stream, Action run)
    {
        ProbeFiles.Require(CacheMode && activeCache == null && activeStream == null && stream != null && store != null, "CACHE_LIFETIME_CONTEXT");
        activeCache = store; activeStream = stream; store.State.Begin();
        try { run(); }
        finally {
            store.State.Close();
            try {
                if (CacheOwned) { OwnedCachePoint("wrapper-final"); OwnedCachePoint("stream-final"); OwnedCachePoint("late"); }
                FinishCacheAudit(store.State.Entries, store.State.Attempts, store.State.Replies, store.State.Faulted, stream.Requests);
            } finally { GC.KeepAlive(store); GC.KeepAlive(stream); GC.KeepAlive(stream.GetTransparentProxy()); }
        }
    }
    static MethodInfo ownedCacheGetter, ownedCacheIdGetter; static object ownedCacheIdValue; static MethodInfo[] ownedStreamMethods; static Type ownedCacheMessageType, ownedCacheCustomType;
    static bool ownedCacheReentry;
    static object NewCacheMessage(string selected)
    {
        object message = Activator.CreateInstance(ownedCacheMessageType); ownedCacheMessageType.GetField("Method").SetValue(message, ownedCacheGetter);
        ownedCacheMessageType.GetField("Name").SetValue(message, "get_LargeStringCache");
        if (selected == "custom-method") ownedCacheMessageType.GetField("Method").SetValue(message, Activator.CreateInstance(ownedCacheCustomType));
        if (selected == "throw-method") ownedCacheMessageType.GetField("Failure").SetValue(message, "method");
        if (selected == "throw-args") ownedCacheMessageType.GetField("Failure").SetValue(message, "args");
        if (selected == "throw-context") ownedCacheMessageType.GetField("Failure").SetValue(message, "context");
        if (selected == "forged-name") ownedCacheMessageType.GetField("Name").SetValue(message, "get_Id");
        if (selected == "forged-count") ownedCacheMessageType.GetField("Count").SetValue(message, 1);
        if (selected == "forged-in") ownedCacheMessageType.GetField("Incoming").SetValue(message, 1);
        if (selected == "forged-args") ownedCacheMessageType.GetField("Arguments").SetValue(message, new object[] { new object() });
        if (selected == "mutable-id") { ownedCacheMessageType.GetField("Method").SetValue(message, ownedCacheIdGetter); ownedCacheMessageType.GetField("Name").SetValue(message, "get_Id"); }
        if (selected == "mutable" || selected == "mutable-id") ownedCacheMessageType.GetField("SingleRead").SetValue(message, true);
        if (selected == "message-reentry") ownedCacheMessageType.GetField("ContextCallback").SetValue(message, new Action(delegate { try { activeCache.Invoke(null); } catch (Exception) { } }));
        return message;
    }
    static void OwnedCacheRecord(string message)
    {
        ProbeFiles.Require(CacheOwned && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_CACHE_CALLBACK_EXCLUDED");
        if (message.StartsWith("LARGE_STRING_CACHE_REFERENCE", StringComparison.Ordinal)) {
            if (ownedCacheCase == "recorder") throw new IOException("OWNED_CACHE_RECORDER_FAULT");
            if (ownedCacheCase == "wrapper-restore") projectResourceCallbackSuspended = false;
            if (ownedCacheCase == "nested" && !ownedCacheReentry) {
                ownedCacheReentry = true; int probes = projectResourceProbeCount; bool refused = false;
                try { activeCache.Invoke(null); } catch (Exception) { refused = true; }
                ProbeFiles.Require(refused && activeCache.State.Faulted && projectResourceWindow == 0 && projectResourceCallbackSuspended && projectResourceProbeCount == probes, "OWNED_CACHE_NESTED_STATE");
                Record("OWNED_CACHE_NESTED_REFUSAL outer-exclusion-preserved=True sticky=True");
            }
        }
        Record("OWNED_CACHE_EXCLUDED " + message);
    }
    static void OwnedStreamRecord(string message)
    { ProbeFiles.Require(CacheOwned && projectResourceWindow == 0 && projectResourceCallbackSuspended, "OWNED_STREAM_CALLBACK_EXCLUDED"); if (ownedCacheCase == "stream-recorder") throw new IOException("OWNED_STREAM_RECORDER_FAULT"); if (ownedCacheCase == "stream-restore") projectResourceCallbackSuspended = false; Record(message); }
    static bool RawCacheCase(string selected)
    { return Array.IndexOf(new string[] { "invalid", "custom-method", "throw-method", "throw-args", "throw-context", "mutable", "mutable-id", "message-reentry", "forged-name", "forged-count", "forged-in", "forged-args", "off-thread" }, selected) >= 0; }
    static void OwnedCachePoint(string point)
    {
        ProbeFiles.Require(CacheOwned, "OWNED_CACHE_POINT_MODE");
        int previous = projectResourceWindow, probes = projectResourceProbeCount;
        Record("OWNED_CACHE_POINT phase=" + point + " window=" + previous);
        bool raw = point == "before-cache" && RawCacheCase(ownedCacheCase);
        bool after = point == "after-cache" && (ownedCacheCase == "second" || ownedCacheCase == "nested" || ownedCacheCase == "recorder" || ownedCacheCase == "wrapper-restore" || ownedCacheCase == "total-limit" || (ownedCacheCase != null && ((ownedCacheCase.StartsWith("stream-", StringComparison.Ordinal) && ownedCacheCase != "stream-final") || ownedCacheCase.StartsWith("object-", StringComparison.Ordinal) || ownedCacheCase == "dispose-message" || ownedCacheCase == "foreign")));
        bool selected = raw || after || ownedCacheCase == point;
        if (!selected || activeCache == null || ownedCacheCase == "total-boundary") return;
        if (point == "stream-final" && ownedCacheCase != "stream-final") return;
        bool refused = false; object rawMessage = null;
        try {
            if (point == "stream-final") activeStream.Invoke(null);
            else if (after && (activeCache.State.Faulted || ownedCacheCase == "total-limit")) refused = activeCache.State.Faulted;
            else if (after && ownedCacheCase != "second") {
                object reference = activeStream.GetTransparentProxy(); string name = ownedCacheCase;
                int slot = Array.IndexOf(new string[] { "stream-id", "stream-exists", "stream-read", "stream-multi", "stream-delete", "stream-time", "stream-mask" }, name);
                if (name == "stream-recorder" || name == "stream-restore") slot = 0;
                if (slot >= 0) ownedStreamMethods[slot].Invoke(reference, slot == 2 ? new object[] { false } : slot == 6 ? new object[] { (ushort)0 } : new object[0]);
                else if (name == "object-type") ((object)reference).GetType();
                else if (name == "object-string") ((object)reference).ToString();
                else if (name == "object-hash") ((object)reference).GetHashCode();
                else if (name == "object-equals") ((object)reference).Equals(reference);
                else { rawMessage = NewCacheMessage(null); ownedCacheMessageType.GetField("Method").SetValue(rawMessage, name == "dispose-message" ? typeof(IDisposable).GetMethod("Dispose") : ownedCacheGetter); activeStream.Invoke((IMessage)rawMessage); refused = activeStream.Requests != 0; }
            } else if (ownedCacheCase == "off-thread") {
                Exception failure = null; var thread = new Thread(new ThreadStart(delegate { try { activeCache.Invoke((IMessage)NewCacheMessage(null)); } catch (Exception error) { failure = error; } }));
                thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000) && failure != null, "OWNED_CACHE_OFF_THREAD_REFUSAL"); refused = true;
            } else {
                rawMessage = NewCacheMessage(ownedCacheCase); IMessage response = activeCache.Invoke(ownedCacheCase == "invalid" ? null : (IMessage)rawMessage);
                var returned = response as IMethodReturnMessage;
                if (ownedCacheCase == "mutable" || ownedCacheCase == "mutable-id") {
                    ProbeFiles.Require(returned != null && returned.Exception == null && Object.ReferenceEquals(returned.ReturnValue, ownedCacheCase == "mutable-id" ? ownedCacheIdValue : activeStream.GetTransparentProxy())
                        && (int)ownedCacheMessageType.GetField("MethodReads").GetValue(rawMessage) == 1 && (int)ownedCacheMessageType.GetField("ArgsReads").GetValue(rawMessage) == 1
                        && (int)ownedCacheMessageType.GetField("ContextReads").GetValue(rawMessage) == 1, "OWNED_CACHE_MUTABLE_SNAPSHOT");
                    if (ownedCacheCase == "mutable-id") { var cacheResponse = (IMethodReturnMessage)activeCache.Invoke((IMessage)NewCacheMessage(null)); ProbeFiles.Require(cacheResponse.Exception == null && Object.ReferenceEquals(cacheResponse.ReturnValue, activeStream.GetTransparentProxy()), "OWNED_CACHE_AFTER_MUTABLE_ID"); }
                    Record("OWNED_CACHE_MUTABLE_SNAPSHOT_PASS case=" + ownedCacheCase + " original-method-args-context-reads=1 same-reference=True protected-id-core=" + (ownedCacheCase == "mutable-id")); return;
                }
            }
        } catch (Exception) { refused = true; }
        bool restoreFault = ownedCacheCase == "wrapper-restore" || ownedCacheCase == "stream-restore";
        ProbeFiles.Require(refused && (restoreFault ? rejected && projectResourceWindow == 0 : previous == projectResourceWindow) && !projectResourceCallbackSuspended && probes == projectResourceProbeCount, "OWNED_CACHE_REFUSAL_RESTORED");
        ProbeFiles.Require(activeCache.State.Faulted || activeStream.Requests != 0, "OWNED_CACHE_REFUSAL_STICKY");
        Record("OWNED_CACHE_REFUSAL case=" + ownedCacheCase + " phase=" + point + " caught=True entries=" + activeCache.State.Entries + " attempts=" + activeCache.State.Attempts + " replies=" + activeCache.State.Replies
            + " wrapper-fault=" + activeCache.State.Faulted + " stream-denied=" + activeStream.Requests + " dependency-flags=" + (unexpected || rejected));
    }
    static void OwnedCacheProvider()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type control = type("UndoOwnedControl"), delivery = type("UndoFactoryOwnedControl"), cacheControl = type("CacheOwnedControl"), traceControl = type("TraceLifetimeControl");
        Type streamType = type("ICacheStreamFixture"), storeType = type("ICacheProjectStoreFixture"), factoryType = type("UndoFactoryDeliveryFixture"), style = type("GroupAddressStyleFixture"), id = type("ScopedIdFixture");
        ownedStreamMethods = StreamContract(streamType, false); ownedCacheGetter = storeType.GetMethod("get_LargeStringCache"); CacheGetterSignature(storeType, ownedCacheGetter, streamType);
        ownedCacheIdGetter = storeType.GetMethod("get_Id"); ownedCacheMessageType = type("CacheMessageFixture"); ownedCacheCustomType = type("CacheCustomMethodFixture");
        if (ownedCacheCase == null) {
            int negatives = 0;
            foreach (string name in new string[] { "ICacheWrongStreamFixture", "ICacheInheritedStreamFixture", "ICacheGenericStreamFixture`1" }) {
                bool bad = false; try { StreamContract(type(name), false); } catch (IOException) { bad = true; } ProbeFiles.Require(bad, "OWNED_CACHE_STREAM_SHAPE_NEGATIVE"); negatives++;
            }
            bool wrong = false; try { CacheGetterSignature(storeType, storeType.GetMethod("get_Id"), streamType); } catch (IOException) { wrong = true; }
            ProbeFiles.Require(wrong && !ExactRuntimeMethod((MethodInfo)Activator.CreateInstance(ownedCacheCustomType), ownedCacheGetter), "OWNED_CACHE_GETTER_NEGATIVE"); negatives += 2;
            Record("OWNED_CACHE_API_PASS negatives=" + negatives + " before-effects=True stream-methods=7");
        }
        ProjectScopeApi scope = ScopedProjectApi(type("CacheProjectFixture"), factoryType, type("IFactoryFixture"), style, storeType);
        EmptyContainerApi container = EmptyApi(type("UndoBuilderFixture"), type("EmptyOptionsFixture"), type("IRegisteredContainerFixture"), type("IContainerDependencyFixture"));
        TraceLifetimeApi trace = TraceApi(type("TraceLifetimeFixture"), type("ITraceLifetimeFixture"));
        Type extension = type("UndoFactoryRegistrationExtensionsFixture"), data = type("RegistrationDataFixture"), registrationStyle = type("RegistrationStyleFixture");
        TraceRegistrationApi traceRegistration = RegistrationApi(container, extension, type("IRegistrationFixture`3"), data, registrationStyle, trace.Contract);
        Type undoType = type("IUndoDeniedFixture"); UndoContract(undoType, type("IOpaqueUndoFixture"));
        UndoFactoryRegistrationApi registration = FactoryRegistrationApi(container, extension, type("IUndoFactoryRegistrationFixture`3"), data, registrationStyle, undoType);
        ConstructorInfo factory = FactoryCtor(factoryType, container.Context, type("IDataDependencyFixture"));
        control.GetField("Callback").SetValue(null, new Action<string>(OwnedCachePoint));
        cacheControl.GetField("SkipCache").SetValue(null, RawCacheCase(ownedCacheCase) || ownedCacheCase == "wrapper-final");
        cacheControl.GetField("IdCalls").SetValue(null, ownedCacheCase == "total-boundary" ? 15 : ownedCacheCase == "total-limit" ? 16 : 2);
        cacheControl.GetField("ThrowProject").SetValue(null, ownedCacheCase == "error");
        traceControl.GetMethod("Reset").Invoke(null, new object[] { "return" });
        traceControl.GetField("ConstructorCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-ctor" }); }));
        traceControl.GetField("DisposeCallback").SetValue(null, new Action(delegate { control.GetMethod("Point").Invoke(null, new object[] { "trace-dispose" }); }));
        var stream = new StreamStoreDenyProxy(streamType, ownedStreamMethods, OwnedStreamRecord);
        object value = CreateFixtureId(id, id.GetConstructor(new Type[] { typeof(uint) }), id.GetMethod("get_ProjectNumber"));
        ownedCacheIdValue = value;
        var store = new LargeStringCacheStoreProxy(storeType, storeType.GetMethod("get_Id"), value, ownedCacheGetter, streamType, stream.GetTransparentProxy(), OwnedCacheRecord);
        var kim = new ProjectDenyProxy(type("IDataDependencyFixture"), Record); var undo = new UndoProviderDenyProxy(undoType, Record);
        var holder = new UndoProviderFactoryHolder(undoType, undo.GetTransparentProxy(), registration.CallbackType, Record); object selectedStyle = Enum.Parse(style, "ThreeLevel", false);
        WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate { WithUndoFactoryProvider(trace, container, traceRegistration, registration, factory, kim, store, undo, holder,
            delegate(object actual) { WithProjectScope(scope, actual, selectedStyle, store, delegate(string phase) { if (phase == "PRE_CTOR") OwnedCachePoint("validation"); if (phase == "POST_CTOR") OwnedCachePoint("logging"); }); }); }); });
        ProbeFiles.Require((ownedCacheCase == null || ownedCacheCase == "total-boundary" || ownedCacheCase == "mutable" || ownedCacheCase == "mutable-id") && store.State.Replies == 1 && store.State.Attempts == 1 && !store.State.Faulted
            && (int)cacheControl.GetField("Caught").GetValue(null) == 0 && ((ownedCacheCase == "mutable" || ownedCacheCase == "mutable-id") || Object.ReferenceEquals(cacheControl.GetField("Reference").GetValue(null), stream.GetTransparentProxy()))
            && stream.Requests == 0 && store.DeniedRequests == 0 && holder.Entries == 1 && holder.Successes == 1 && !holder.Faulted && undo.Requests == 0 && kim.Requests == 0
            && (int)control.GetField("Registrations").GetValue(null) == 2 && (int)control.GetField("ExternalCalls").GetValue(null) == 2 && (int)control.GetField("ProjectDisposals").GetValue(null) == 1
            && (int)control.GetField("ContainerDisposals").GetValue(null) == 1 && (int)traceControl.GetField("Disposals").GetValue(null) == 1, "OWNED_CACHE_LIFETIME_COUNTS");
        Record("OWNED_CACHE_LIFETIME_PASS entries=" + store.State.Entries + " attempts=1 replies=1 same-preexisting-reference=True exact-two-registrations=True stream-values=NOT_PROVIDED");
    }
    static MethodInfo[] UndoContract(Type contract, Type opaque)
    {
        ProbeFiles.Require(PublicInterface(contract) && !contract.IsGenericType && contract.GetInterfaces().Length == 0
            && opaque != null && opaque.Assembly == contract.Assembly && !opaque.Assembly.ReflectionOnly, "UNDO_PUBLIC_CONTRACT");
        MethodInfo[] methods = contract.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        ProbeFiles.Require(methods.Length == 3, "UNDO_METHOD_COUNT");
        var selected = new MethodInfo[3];
        foreach (MethodInfo method in methods) {
            int slot = method.Name == "GetStore" ? 0 : method.Name == "NewStore" ? 1 : method.Name == "get_RootPath" ? 2 : -1;
            ProbeFiles.Require(slot >= 0 && selected[slot] == null && method.DeclaringType == contract && method.IsPublic && !method.IsStatic
                && !method.IsGenericMethod && !method.ContainsGenericParameters && method.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis)
                && method.ReturnType == (slot == 2 ? typeof(string) : opaque) && method.IsSpecialName == (slot == 2), "UNDO_METHOD_SHAPE");
            ParameterInfo[] parameters = method.GetParameters();
            ProbeFiles.Require(parameters.Length == (slot == 0 ? 1 : 0) && (slot != 0 || parameters[0].ParameterType == typeof(Guid)), "UNDO_EXACT_PARAMETERS");
            selected[slot] = method;
        }
        return selected;
    }
    static bool ReviewedUndoTokens(int type, int first, int second, int third)
    { return type == 0x02000121 && first == 0x06000af3 && second == 0x06000af4 && third == 0x06000af5; }
    static void OriginalUndoTokens(Type contract)
    {
        MethodInfo[] methods = UndoContract(contract, contract.Assembly.GetType("Knx.Ets.ObjectModel.IStreamStore", true));
        ProbeFiles.Require(UndoDeniedOriginal && contract.Assembly.FullName == OmIdentity && !contract.Assembly.ReflectionOnly
            && (int)contract.Attributes == 1048737 && ReviewedUndoTokens(contract.MetadataToken, methods[0].MetadataToken, methods[1].MetadataToken, methods[2].MetadataToken)
            && (int)methods[0].Attributes == 1478 && (int)methods[1].Attributes == 1478 && (int)methods[2].Attributes == 3526, "UNDO_ORIGINAL_DECLARATIONS");
        Record("UNDO_API_READY metadata-sha256=fe67edd07a92917d4b5d059379b6083876f4c82cf22e9026c9f3afea09d1f9ed type=02000121 methods=06000AF3,06000AF4,06000AF5 return-type=IDENTITY_ONLY service=DENY_ONLY qualification=NOT_ESTABLISHED");
    }
    static void CheckUndoProvider(string phase)
    { if (UndoDeniedMode && activeUndo != null) ProbeFiles.Require(activeUndo.Requests == 0, "UNDO_PROVIDER_DENIED_" + phase); }
    static void FinishUndoProviderAudit(int count)
    {
        try { Record("UNDO_PROVIDER_FINAL_AUDIT after-all-managed-disposal=True denied=" + count + " value=NOT_PROVIDED qualification=NOT_ESTABLISHED"); }
        finally { if (count != 0) throw new NotSupportedException("UNDO_PROVIDER_FINAL_DENIAL"); }
    }
    static void RegisterUndoProvider(TraceRegistrationApi api, object builder, UndoProviderDenyProxy undo)
    {
        ProbeFiles.Require(UndoDeniedMode && api.Contract.IsInstanceOfType(undo.GetTransparentProxy()), "UNDO_REGISTRATION_CONTRACT");
        TraceDependencyGuard(unexpected, rejected, "UNDO_PRE_REGISTER");
        object registration = api.Register.Invoke(null, new object[] { builder, undo.GetTransparentProxy() });
        ProbeFiles.Require(registration != null && api.Return.IsInstanceOfType(registration) && registration.GetType().Assembly == api.Return.Assembly
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration), "UNDO_REGISTRATION_RETURN");
        TraceDependencyGuard(unexpected, rejected, "UNDO_POST_REGISTER");
        object external = api.External.Invoke(registration, new object[0]);
        ProbeFiles.Require(Object.ReferenceEquals(registration, external), "UNDO_EXTERNAL_SAME_BUILDER");
        TraceDependencyGuard(unexpected, rejected, "UNDO_POST_EXTERNAL");
        Record("UNDO_REGISTER_END external-ownership=True exact-interface=True application-registrations=2 before-build=True");
    }
    static void WithUndoProvider(TraceLifetimeApi trace, EmptyContainerApi container, TraceRegistrationApi traceRegistration, TraceRegistrationApi undoRegistration,
        ConstructorInfo factory, ProjectDenyProxy kim, ProjectStoreIdProxy store, UndoProviderDenyProxy undo, Action<object> projectUse)
    {
        ProbeFiles.Require(UndoDeniedMode && activeUndo == null && undo != null, "UNDO_LIFETIME_CONTEXT"); activeUndo = undo;
        object scopedFactory = null;
        try {
            try { WithTraceLifetimeCore(trace, null, delegate(object instance) {
                WithEmptyContainerCore(container, delegate(object context) {
                    InvokeFactoryForProject(factory, context, kim, delegate(object actual) { scopedFactory = actual; projectUse(actual); });
                }, null, delegate(object builder) { RegisterTrace(traceRegistration, builder, instance); RegisterUndoProvider(undoRegistration, builder, undo); });
            }); }
            catch { if (UndoDeniedOwned) OwnedUndoPoint("error"); throw; }
        } finally {
            try { if (UndoDeniedOwned) OwnedUndoPoint("final-audit"); FinishUndoProviderAudit(undo.Requests); }
            finally {
                try {
                    Record("UNDO_OTHER_FINAL_AUDIT after-all-managed-disposal=True kim-denied=" + kim.Requests + " store-denied=" + store.DeniedRequests);
                    ProbeFiles.Require(kim.Requests == 0 && store.DeniedRequests == 0, "UNDO_OTHER_LIFETIME_DENIAL");
                } finally { GC.KeepAlive(scopedFactory); GC.KeepAlive(store); GC.KeepAlive(kim); GC.KeepAlive(undo); }
            }
        }
    }
    [STAThread]
    internal static int Main(string[] args)
    {
        int outcome = 2;
        try { outcome = MainCore(args); }
        finally {
            try { if (ContextOwned && contextRetained != null) OwnedContextPoint("process-final"); CheckContextArguments("PROCESS_FINAL"); }
            catch (Exception error) { outcome = 2; try { if (log != null) Errors(error); } catch { } }
            finally { GC.KeepAlive(contextRetained); }
        }
        return outcome;
    }
    static int MainCore(string[] args)
    {
        int result = 2;
        try {
            ProbeFiles.Require((args.Length == 6 || args.Length == 8) && args[0] == "--mode" && args[2] == "--input" && args[4] == "--log", "PROJECT_ARGUMENTS");
            mode = args[1]; ProbeFiles.Require(EnumOnly || Original || OwnedProject || FactoryOnly, "PROJECT_MODE");
            ProbeFiles.Require(args.Length == 6 || (mode == "owned-project-genuine-factory-resources" && args[6] == "--owned-resource-case" && Array.IndexOf(OwnedResourceCases, args[7]) >= 0)
                || (mode == "owned-project-undo-provider-denied" && args[6] == "--owned-undo-case" && Array.IndexOf(OwnedUndoCases, args[7]) >= 0)
                || (mode == "owned-project-undo-provider-factory" && args[6] == "--owned-factory-case" && Array.IndexOf(OwnedFactoryCases, args[7]) >= 0)
                || (mode == "owned-project-large-string-cache-denied" && args[6] == "--owned-cache-case" && Array.IndexOf(OwnedCacheCases, args[7]) >= 0)
                || (ContextOwned && args[6] == "--owned-context-case" && Array.IndexOf(OwnedContextCases, args[7]) >= 0)
                || (XmlRegistrationOwned && args[6] == "--owned-store-diagnostic-case" && Array.IndexOf(OwnedStoreDiagnosticCases, args[7]) >= 0)
                || (XmlRegistrationOwned && args[6] == "--owned-xml-pin-case" && Array.IndexOf(new string[] { "missing", "size", "hash" }, args[7]) >= 0), "OWNED_RESOURCE_CASE_ARGUMENTS");
            if (args.Length == 8) { if (XmlRegistrationOwned && args[6] == "--owned-store-diagnostic-case") ownedStoreDiagnosticCase = args[7]; else if (XmlRegistrationOwned && args[6] == "--owned-xml-pin-case") ownedXmlPinCase = args[7]; else if (ContextOwned) ownedContextCase = args[7]; else if (CacheOwned) ownedCacheCase = args[7]; else if (UndoFactoryOwned) ownedFactoryCase = args[7]; else if (UndoDeniedOwned) ownedUndoCase = args[7]; else ownedResourceCase = args[7]; }
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
            } else if (XmlRegistrationOriginal) {
                PinXmlRegistrationInputs();
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
                        if (ProjectScope) { if (XmlRegistrationOwned && ownedStoreDiagnosticCase != null) OwnedStoreRequestDiagnostics(); else if (XmlRegistrationOwned && ownedXmlPinCase != null) OwnedXmlRegistrationPinNegative(); else if (ContextMode) { if (ContextOriginal) OriginalContextConstruction(); else OwnedContextConstruction(); } else if (ProjectScopeOriginal) OriginalProjectScope(); else if (CacheOwned) OwnedCacheProvider(); else if (UndoFactoryOwned) OwnedUndoFactoryProvider(); else if (UndoDeniedOwned) OwnedUndoProvider(); else if (ProjectResourcesOwned) OwnedProjectResources(); else OwnedProjectScope(); }
                        else if (RegisteredTrace) { if (RegisteredOriginal) OriginalRegisteredTrace(); else OwnedRegisteredTrace(); }
                        else if (TraceOnly) { if (TraceOriginal) OriginalTraceEncryptor(); else OwnedTraceEncryptor(); }
                        else if (EmptyContainer) { if (FactoryOriginal) OriginalEmptyContainer(); else OwnedEmptyContainer(); }
                        else if (FactoryOnly) { if (FactoryOriginal) OriginalFactory(); else OwnedFactory(); }
                        else if (StoreIdFixture) { if (Original) OriginalProjectStoreId(); else OwnedProjectStoreId(); }
                        else if (Original) OriginalProject(); else Owned(); result = 0;
                    }
                    catch (Exception error) { Errors(error); result = 10; }
                    finally { app.Shutdown(result); }
                }));
                app.Run();
            }
            CheckUndoProvider("PROCESS_FINAL"); CheckUndoFactory("PROCESS_FINAL"); CheckCache("PROCESS_FINAL"); CheckContextArguments("PROCESS_FINAL");
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
