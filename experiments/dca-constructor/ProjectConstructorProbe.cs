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

// Task011 fixed-reference manifest selection, owned from the observed ETS predicate.
internal static class ReferenceDcaManifestSelection
{
    internal const string ManifestHash="ad508caccbd89efad6d1c7c3bd783bd075614fecd77ba45e1b096f3cfaa6a4f6";
    internal const string Hardware2ProgramId="M-0083_H-193-1-O00EF_HP-0155-40-4D9F-O00EF";
    internal static string[] Declarations()
    {return new string[]{"M-0083_A-0154-10-0000","M-0083_A-0155-10-0000","M-0083_A-0154-11-0000","M-0083_A-0155-11-0000","M-0083_A-0154-40-0000","M-0083_A-0155-40-0000"};}
    static bool BoundedAscii(string value)
    {if(String.IsNullOrEmpty(value)||value.Length>128)return false;foreach(char c in value)if(c<0x21||c>0x7e)return false;return true;}
    internal static bool MatchesProjection(string[] declarations,string originalProjection)
    {
        if(declarations==null||declarations.Length==0||declarations.Length>16||!BoundedAscii(originalProjection))return false;
        bool selected=false;
        foreach(string declaration in declarations){if(declaration==null)continue;if(!BoundedAscii(declaration))return false;if(declaration.StartsWith(originalProjection,StringComparison.Ordinal))selected=true;}
        return selected;
    }
    internal static void BeforeContextMutation(object selectedApplication,string[] declarations,string originalProjection,Action mutation)
    {
        if(selectedApplication==null)throw new InvalidDataException("DCA_REQUIRED_PRODUCT_CONTEXT_MISSING");
        BeforeMutation(declarations,originalProjection,mutation);
    }
    internal static void BeforeMutation(string[] declarations,string originalProjection,Action mutation)
    {
        if(mutation==null)throw new ArgumentNullException("mutation");
        if(!MatchesProjection(declarations,originalProjection))throw new InvalidDataException("DCA_MANIFEST_APPLICATION_MISMATCH");
        mutation();
    }
}

// An absent device is rejected at the owned entry to the real GetDcaUi path.
internal static class ReferenceDcaRequiredDevice
{
    internal static void BeforeUi(object device,Action invokeUi)
    {
        if(invokeUi==null)throw new ArgumentNullException("invokeUi");
        if(device==null)throw new InvalidDataException("DCA_REQUIRED_SDK_DEVICE_MISSING");
        invokeUi();
    }
}

// A required original parameter-reference definition must exist before original UI use.
// Negative tests may omit an owned view reference; they never alter the original SDK object.
internal static class ReferenceDcaRequiredParameters
{
    internal static bool CompactDiagnostics(bool ui,bool negative){return ui||negative;}
    internal static bool AllowsPreparedService(bool active,bool ownerThread,bool knownValidator,int operation)
    {return active&&((knownValidator&&ownerThread)||operation==14);}
    internal static int ContextBudget(bool negative,int ordinary){return negative?1000000:ordinary;}
    internal static int ReadBudget(bool negative,int ordinary){return negative?200000:ordinary;}
    internal static int IdentityWriteBudget(bool identity,int ordinary){return identity?128:ordinary;}
    internal static int IdentityContextBudget(bool identity,int ordinary){return identity?256:ordinary;}
    internal static bool AllowsProductStoreRead(bool active,int slot)
    {return active&&slot>=0&&slot<=4;}
    internal static bool AllowsDerivedReadCreation(bool completed,bool uiLifetime,bool parameterLifetime)
    {return completed&&(uiLifetime||parameterLifetime);}
    internal static bool AllowsBackgroundRead(bool active,int operation)
    {return active&&(operation==10||operation==11||operation==12);}
    internal static bool AllowsReadOperation(bool active,int operation)
    {return active&&(operation==3||operation==10||operation==11||operation==12||operation==13);}
    internal static void BeforeUi(object definition,Action invokeUi)
    {
        if(invokeUi==null)throw new ArgumentNullException("invokeUi");
        if(definition==null)throw new InvalidDataException("DCA_REQUIRED_PARAMETER_DEFINITION_MISSING");
        invokeUi();
    }
}

// Exact repeat suppression for known healthy records only; never an admission decision.
internal sealed class ReferenceHealthyLogCoalescer
{
    readonly object sync=new object();readonly Dictionary<string,bool> seen=new Dictionary<string,bool>(StringComparer.Ordinal);int omitted;
    internal int Omitted{get{lock(sync){return omitted;}}}
    internal int Kinds{get{lock(sync){return seen.Count;}}}
    internal bool ShouldEmit(bool compact,string value){return ShouldEmitCore(compact,value,false);}
    internal bool ShouldEmitIdentity(bool compact,string value){return ShouldEmitCore(compact,value,true);}
    bool ShouldEmitCore(bool compact,string value,bool identity)
    {
        if(!compact||value==null||value.Length>4096)return true;
        bool known=value.StartsWith("FACTORY_BINDING_RESOLVED requested=",StringComparison.Ordinal)||value.StartsWith("PRODUCT_STORE_REFERENCE member=",StringComparison.Ordinal)
            ||value.StartsWith("UNDO_PROVIDER_FACTORY_CALLBACK entries=1 same-existing-proxy=True",StringComparison.Ordinal)||value.StartsWith("SDK_REFERENCE_PRODUCT_ID input=",StringComparison.Ordinal)
            ||value.StartsWith("PROJECT_CONTENT_REFERENCE reply=PREPARED same-preexisting-reference=True",StringComparison.Ordinal)||value.StartsWith("SDK_CONTEXT_CANONICAL_REFERENCE member=",StringComparison.Ordinal);
        string counter=null;
        if(identity&&value.StartsWith("CONTEXT_ADAPTER_REPLY method=",StringComparison.Ordinal))known=true;
        if(identity&&value.StartsWith("SDK_WRITE_FIXTURE_REPLY method=",StringComparison.Ordinal)){known=true;counter="entry=";}
        if(value.StartsWith("SDK_NAME_RESOURCE_PROBE allowed=True ",StringComparison.Ordinal)){known=true;counter="aggregate-probe=";}
        else if(value.StartsWith("DCA_UI_RESOURCE_PROBE allowed=True ",StringComparison.Ordinal)){known=true;counter="aggregate=";}
        if(!known)return true;
        string key=value;
        if(counter!=null){int a=value.IndexOf(counter,StringComparison.Ordinal);if(a>=0){a+=counter.Length;int b=a;while(b<value.Length&&b-a<8&&value[b]>='0'&&value[b]<='9')b++;if(b>a)key=value.Substring(0,a)+"COUNT"+value.Substring(b);}}
        lock(sync){if(seen.ContainsKey(key)){omitted++;return false;}if(seen.Count<64)seen.Add(key,true);return true;}
    }
}

// Admission accounting only: this never grants an SDK service or a successful reply.
internal sealed class ParameterReadPublication
{
    readonly object sync=new object();bool started,accepting,closed,fault;int inflight;
    internal bool Started{get{lock(sync){return started;}}}
    internal bool Accepting{get{lock(sync){return accepting&&!fault;}}}
    internal int Inflight{get{lock(sync){return inflight;}}}
    internal bool Faulted{get{lock(sync){return fault;}}}
    internal void Begin(){lock(sync){if(started||fault){fault=true;throw new InvalidOperationException("PARAMETER_READ_BEGIN_ONCE");}started=true;accepting=true;}}
    internal IDisposable TryEnter(){lock(sync){if(!accepting||fault)return null;if(inflight>=1024){fault=true;throw new InvalidOperationException("PARAMETER_READ_LEASE_LIMIT");}inflight++;return new Lease(this);}}
    internal void Close(){lock(sync){accepting=false;closed=true;if(inflight!=0){fault=true;throw new InvalidOperationException("PARAMETER_READ_INFLIGHT:"+inflight);}if(fault)throw new InvalidOperationException("PARAMETER_READ_FENCE_FAULT");}}
    internal void CheckClosed(){lock(sync){if(accepting||inflight!=0||fault||(started&&!closed))throw new InvalidOperationException("PARAMETER_READ_FINAL_FENCE");}}
    void Release(){lock(sync){if(inflight<=0){fault=true;throw new InvalidOperationException("PARAMETER_READ_LEASE_RELEASE");}inflight--;}}
    sealed class Lease:IDisposable
    {
        ParameterReadPublication source;internal Lease(ParameterReadPublication value){source=value;}
        public void Dispose(){ParameterReadPublication value=Interlocked.Exchange(ref source,null);if(value!=null)value.Release();}
    }
}

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
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback();
        try { return InvokeCore(message); }
        finally { ProjectConstructorProbe.RestoreProjectResourceCallback(suspended); }

        }
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
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int suspended = ProjectConstructorProbe.SuspendProjectResourceCallback();
        try { return InvokeCore(message); }
        finally { ProjectConstructorProbe.RestoreProjectResourceCallback(suspended); }

        }
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
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
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
}

// A service-reference factory only. The captured provider still denies every method.
internal sealed class UndoProviderFactoryHolder
{
    readonly object sync = new object(), instance;
    readonly Type contract;
    readonly Func<bool> specificPhase;
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
    internal UndoProviderFactoryHolder(Type service,object proxy,Type callbackType,Action<string> recorder,Func<bool> phase) : this(service,proxy,callbackType,recorder)
    { ProbeFiles.Require(ProjectConstructorProbe.SdkMode&&phase!=null,"SDK_KIM_REFERENCE_MODE");specificPhase=phase; }
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
            bool disposalReference = ProjectConstructorProbe.GuidUndoProviderReferenceEligible();
            bool eligible = specificPhase!=null?specificPhase():disposalReference || ProjectConstructorProbe.UndoFactoryWindowEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            lock (sync) {
                ProbeFiles.Require(count == 1 && lifetime == 1 && !inCallback && !violation && (!excluded || disposalReference) && typeof(T) == contract
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
    internal int Count(int maximum=16) { lock (sync) { if (entries < maximum+1) entries++; return entries; } }
    internal void Fail() { lock (sync) { faulted = true; } }
    internal void Begin() { lock (sync) { if (lifetime != 0) { faulted = true; throw new NotSupportedException("CACHE_LIFETIME_BEGIN"); } lifetime = 1; } }
    internal void Close() { lock (sync) { if (lifetime != 1) faulted = true; lifetime = 2; } }
    internal string EntryDiagnostic(){lock(sync){return "shared-lifetime="+lifetime+" shared-entered="+entered+" shared-fault="+faulted+" shared-entries="+entries;}}
    internal void Enter(int count, bool excluded,int maximum=16)
    { lock (sync) { if (count > maximum || excluded || entered || faulted || lifetime != 1) { faulted = true; throw new NotSupportedException("CACHE_CALLBACK_CONTEXT"); } entered = true; } }
    internal void CacheAttempt(bool eligible)
    { lock (sync) { if (attempts < 2) attempts++; if (attempts != 1 || !eligible || Thread.CurrentThread.ManagedThreadId != ownerThread || lifetime != 1 || faulted) { faulted = true; throw new NotSupportedException("CACHE_REFERENCE_UNSUPPORTED"); } } }
    internal void Exit(bool ownsEntry, bool prepared)
    { lock (sync) { if (ownsEntry) entered = false; if (prepared && !faulted) replies++; } }
    internal void ExitSdkMapping(bool ownsEntry,bool prepared,SdkMappingPolicy policy)
    { lock(sync){if(ownsEntry)entered=false;policy.CompleteReference(prepared&&!faulted);} }
    internal void ExitSdkRead(bool ownsEntry,bool prepared,SdkMappingPolicy policy)
    { lock(sync){if(ownsEntry)entered=false;policy.CompleteRead(prepared&&!faulted);} }
    internal void ExitProductApplication(bool ownsEntry,bool prepared,SdkProductApplicationPolicy policy)
    {lock(sync){if(ownsEntry)entered=false;policy.Complete(prepared&&!faulted);}}
    internal void ExitGuid(bool ownsEntry, bool prepared, bool setter, Guid value, ProjectGuidFixture policy)
    { lock (sync) { if (ownsEntry) entered = false; policy.Complete(prepared && !faulted, setter, value); } }
    internal void ExitExists(bool ownsEntry, bool prepared, ContentExistsPolicy policy)
    { lock (sync) { if (ownsEntry) entered = false; policy.Complete(prepared && !faulted); } }
    // New policy only: shared-fault acceptance and reply commit occur while this lock is held.
    // A shared fault before completion suppresses the reply; a later fault preserves history.
    internal void ExitContent(bool ownsEntry, bool cachePrepared, bool contentPrepared, ProjectContentReference content)
    {
        lock (sync) {
            if (ownsEntry) entered = false;
            if (cachePrepared && !faulted) replies++;
            bool accepted = contentPrepared && !faulted;
            content.Complete(accepted);
        }
    }
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
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
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
}
// Source35: the supplied object is only a typed denial reference, never an IStore implementation.
internal class ProjectContentDenyProxy : RealProxy
{
    internal readonly ContextDenialState State = new ContextDenialState(16);
    readonly Action<string> record;
    internal ProjectContentDenyProxy(Type contract, Action<string> recorder) : base(contract) { record = recorder; }
    readonly bool firstDiagnostic; readonly Type diagnosticOwner;
    internal ProjectContentDenyProxy(Type contract, Action<string> recorder, bool diagnoseFirst) : this(contract, recorder)
    {
        ProbeFiles.Require(diagnoseFirst && ProjectConstructorProbe.ContentDiagnosticsMode && contract != null && contract.GetType() == typeof(object).GetType()
            && contract.IsPublic && contract.IsInterface && !contract.IsGenericType && !contract.ContainsGenericParameters, "CONTENT_DIAGNOSTIC_OPT_IN");
        firstDiagnostic = true; diagnosticOwner = contract;
    }
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int count = State.Deny(); bool suspended = false; int previous = 0;
        try {
            if (!ProjectConstructorProbe.UndoFactoryCallbackExcluded()) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            if (count > 16) throw new NotSupportedException("PROJECT_CONTENT_STORE_BUDGET");
            if (firstDiagnostic && count == 1)
                record("PROJECT_CONTENT_REFUSAL entry=1 " + ContentMessageDescriptor(message, diagnosticOwner) + " values=NOT_INSPECTED caller-origin=UNKNOWN");
            record("PROJECT_CONTENT_STORE_DENIED entry=" + count + " values=NOT_INSPECTED message=" + (firstDiagnostic && count == 1 ? "FIRST_METHOD_METADATA_ONLY" : "NOT_INSPECTED"));
            throw new NotSupportedException("PROJECT_CONTENT_STORE_UNSUPPORTED");
        } catch { State.Fail(); throw; }
        finally { try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); } catch { State.Fail(); throw; } }

        }
    }
    internal static string ContentMessageDescriptor(IMessage message, Type owner)
    {
        if (message == null || System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message)) return "category=UNTRUSTED_MESSAGE";
        Type kind = message.GetType();
        if (kind.Module != typeof(IMethodCallMessage).Module || kind.Assembly != typeof(IMethodCallMessage).Assembly) return "category=UNTRUSTED_MESSAGE_TYPE";
        var call = message as IMethodCallMessage; if (call == null) return "category=UNTRUSTED_MESSAGE";
        MethodInfo method;
        try { method = call.MethodBase as MethodInfo; }
        catch { return "category=METHOD_READ_FAILED"; }
        return ContentMethodDescriptor(method, owner);
    }
    internal static MethodInfo CaptureContentMethod(IMessage message)
    {
        if(message==null||System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message))return null;
        Type kind=message.GetType();if(kind.Module!=typeof(IMethodCallMessage).Module||kind.Assembly!=typeof(IMethodCallMessage).Assembly)return null;
        var call=message as IMethodCallMessage;if(call==null)return null;return call.MethodBase as MethodInfo;
    }
    internal static string ContentMessageDescriptor(IMessage message, Type owner, out MethodInfo captured)
    {
        captured = null;
        if (message == null || System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message)) return "category=UNTRUSTED_MESSAGE";
        Type kind = message.GetType();
        if (kind.Module != typeof(IMethodCallMessage).Module || kind.Assembly != typeof(IMethodCallMessage).Assembly) return "category=UNTRUSTED_MESSAGE_TYPE";
        var call = message as IMethodCallMessage; if (call == null) return "category=UNTRUSTED_MESSAGE";
        try { captured = call.MethodBase as MethodInfo; } catch { return "category=METHOD_READ_FAILED"; }
        return ContentMethodDescriptor(captured, owner);
    }
    internal static string ContentMethodDescriptor(MethodInfo method, Type owner)
    {
        if (method == null) return "category=MEMBER_UNAVAILABLE";
        if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method) || method.GetType() != typeof(object).GetMethod("GetType").GetType()) return "category=UNTRUSTED_METHOD_KIND";
        foreach (MethodInfo expected in new MethodInfo[] { typeof(object).GetMethod("GetType"), typeof(object).GetMethod("ToString"), typeof(object).GetMethod("GetHashCode"), typeof(object).GetMethod("Equals", new Type[] { typeof(object) }), typeof(IDisposable).GetMethod("Dispose") })
            if (ProjectConstructorProbe.ExactRuntimeMethod(method, expected)) return "category=TRUSTED_CORE_MEMBER owner=" + ProjectConstructorProbe.Text(expected.DeclaringType.FullName) + " token=" + expected.MetadataToken.ToString("X8") + " name=" + ProjectConstructorProbe.Text(expected.Name);
        Module module = method.Module; Type declaring = method.DeclaringType;
        if (module != owner.Module || declaring != owner) return ForeignContentMethodDescriptor(method, module, declaring);
        int token = method.MetadataToken;
        if ((token & unchecked((int)0xff000000)) != 0x06000000 || (token & 0x00ffffff) == 0) return "category=UNSUPPORTED_TOKEN";
        return "category=TRUSTED_CONTENT_MEMBER owner=" + ProjectConstructorProbe.Text(owner.FullName) + " token=" + token.ToString("X8") + " name=" + ProjectConstructorProbe.Text(method.Name);
    }

    static string ForeignContentMethodDescriptor(MethodInfo method, Module module, Type declaring)
    {
        const string unsupported = "category=FOREIGN_OWNER detail=UNSUPPORTED_OWNER_SHAPE";
        if (declaring == null || declaring.GetType() != typeof(object).GetType() || declaring.HasElementType || module == null) return unsupported;
        Type definition = declaring.IsGenericType ? declaring.GetGenericTypeDefinition() : declaring;
        if (definition.GetType() != typeof(object).GetType() || definition.HasElementType || (definition.IsGenericType && !definition.IsGenericTypeDefinition)
            || declaring.Module != module || definition.Module != module || module.Assembly != definition.Assembly || module.Assembly.IsDynamic || module.Assembly.ReflectionOnly) return unsupported;
        int typeToken = definition.MetadataToken, methodToken = method.MetadataToken;
        if ((typeToken & unchecked((int)0xff000000)) != 0x02000000 || (typeToken & 0x00ffffff) == 0
            || (methodToken & unchecked((int)0xff000000)) != 0x06000000 || (methodToken & 0x00ffffff) == 0) return unsupported;
        return "category=FOREIGN_OWNER qualification=UNVERIFIED_CONTRACT_MEMBER declaring-type=" + ProjectConstructorProbe.Text(definition.FullName)
            + " generic-instantiation=NOT_INSPECTED declaring-type-token=" + typeToken.ToString("X8") + " module-scope=" + ProjectConstructorProbe.Text(module.ScopeName)
            + " module-assembly=" + ProjectConstructorProbe.Text(module.Assembly.FullName) + " token=" + methodToken.ToString("X8") + " name=" + ProjectConstructorProbe.Text(method.Name);
    }

}
// Source38 binds once; never holds this lock while acquiring shared/reference/proxy locks.
internal sealed class ContentExistsPolicy
{
    readonly object sync = new object(); readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
    internal readonly MethodInfo Getter; CacheReferenceState shared; ProjectContentReference reference;
    int attempts, replies, lifetime; bool faulted;
    internal ContentExistsPolicy(MethodInfo getter) { Getter = getter; }
    internal int Attempts { get { lock (sync) { return attempts; } } } internal int Replies { get { lock (sync) { return replies; } } }
    internal int Lifetime { get { lock (sync) { return lifetime; } } } internal bool Faulted { get { lock (sync) { return faulted; } } }
    internal void Fail() { lock (sync) { faulted = true; } }
    internal void Bind(CacheReferenceState state, ProjectContentReference value)
    { lock (sync) { if (shared != null || reference != null || state == null || value == null || lifetime != 0) { faulted = true; throw new NotSupportedException("EXISTS_BIND_ONCE"); } shared = state; reference = value; } }
    internal CacheReferenceState Shared { get { lock (sync) { if (shared == null) { faulted = true; throw new NotSupportedException("EXISTS_UNBOUND"); } return shared; } } }
    internal bool Bound { get { lock (sync) { return shared != null && reference != null; } } }
    internal void Begin() { lock (sync) { if (shared == null || reference == null || lifetime != 0) { faulted = true; throw new NotSupportedException("EXISTS_LIFETIME_BEGIN"); } lifetime = 1; } }
    internal void Close() { lock (sync) { if (lifetime != 1) faulted = true; lifetime = 2; } }
    internal void RequireContext(bool eligible)
    {
        ProjectContentReference value; lock (sync) { value = reference; }
        bool committed = value != null && value.Replies == 1 && !value.Faulted;
        lock (sync) { if (lifetime != 1 || faulted || !eligible || !committed || Thread.CurrentThread.ManagedThreadId != ownerThread) { faulted = true; throw new NotSupportedException("EXISTS_CALLBACK_CONTEXT"); } }
    }
    internal void Attempt() { lock (sync) { if (attempts < 2) attempts++; if (attempts != 1 || lifetime != 1 || faulted) { faulted = true; throw new NotSupportedException("EXISTS_FIXTURE_LIMIT"); } } }
    internal void Complete(bool accepted) { lock (sync) { if (accepted && !faulted) replies++; } }
}
internal sealed class ProjectContentExistsProxy : ProjectContentDenyProxy
{
    readonly ContentExistsPolicy policy; readonly Type contract; readonly Action<string> record;
    SdkProductApplicationPolicy productWriter;
    internal void ConfigureProductWriter(SdkProductApplicationPolicy value)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productWriter==null&&value!=null&&value.Arity==0,"PRODUCT_CONTENT_WRITER_CONFIGURE_ONCE");productWriter=value;}
    internal ProjectContentExistsProxy(Type type, Action<string> recorder, ContentExistsPolicy value) : base(type, recorder)
    { contract = type; record = recorder; policy = value; }
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        CacheReferenceState shared = policy.Shared; int storeLimit=ProjectConstructorProbe.ProductStoreCallbackLimit();int count = shared.Count(storeLimit);
        bool suspended = false, entered = false, prepared = false, denied = false, writerRequest=false, writerPrepared=false; int previous = 0; MethodInfo method = null;string writerStage="ENTRY";bool cleanupEligibleForDiagnostic=false;
        try {
            bool eligible = ProjectConstructorProbe.CacheWindowEligible(), cleanupEligible=ProjectConstructorProbe.ProductStoreDisposalEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();cleanupEligibleForDiagnostic=cleanupEligible;
            if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            shared.Enter(count, excluded,storeLimit); entered = true;if(productWriter==null)policy.RequireContext(eligible);
            ProbeFiles.Require(message != null && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(message), "EXISTS_MESSAGE_KIND");
            Type kind = message.GetType();
            ProbeFiles.Require(kind.Module == typeof(IMethodCallMessage).Module && kind.Assembly == typeof(IMethodCallMessage).Assembly, "EXISTS_CORE_MESSAGE_OWNER");
            var call = message as IMethodCallMessage; ProbeFiles.Require(call != null, "EXISTS_CALL_MESSAGE");
            method = call.MethodBase as MethodInfo;
            writerRequest=productWriter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productWriter.Method);
            if(writerRequest){
                writerStage="SHAPE";ProbeFiles.Require(call.MethodName==productWriter.Method.Name&&call.ArgCount==0&&call.InArgCount==0,"PRODUCT_CONTENT_WRITER_SHAPE");object[] values=call.Args;ProbeFiles.Require(values!=null&&values.Length==0,"PRODUCT_CONTENT_WRITER_ARGS");
                LogicalCallContext writerContext=call.LogicalCallContext;string writerUri=call.Uri;ProbeFiles.Require(writerUri==null||writerUri.Length<=1024,"PRODUCT_CONTENT_WRITER_URI");
                writerStage="ORIGINAL_READ";object writer=productWriter.Read(null,cleanupEligible);writerStage="ORIGINAL_RETURNED";var writerSnapshot=new CacheCallSnapshot(productWriter.Method,writerContext,writerUri);
                ProbeFiles.Require(!shared.Faulted&&!policy.Faulted&&State.Entries==0&&!State.Faulted,"PRODUCT_CONTENT_WRITER_CAUGHT_FAULT");
                writerStage="REPLY";record("PROJECT_CONTENT_WRITER reply=PREPARED source=ORIGINAL_TRANSACTIONAL_FILE_XML_WRITER destination=OWNED_TRANSIENT_SCRATCH accepted-configuration=False");
                IMessage writerResponse=new ReturnMessage(writer,new object[0],0,writerContext,writerSnapshot);ProbeFiles.Require(!shared.Faulted&&!policy.Faulted&&!State.Faulted,"PRODUCT_CONTENT_WRITER_REPLY_FAULT");writerPrepared=true;return writerResponse;
            }
            if(productWriter!=null)policy.RequireContext(eligible);
            ProbeFiles.Require(ProjectConstructorProbe.ExactRuntimeMethod(method, policy.Getter), "EXISTS_UNSUPPORTED_MEMBER");
            policy.Attempt();
            ProbeFiles.Require(call.MethodName == policy.Getter.Name && call.ArgCount == 0 && call.InArgCount == 0, "EXISTS_MESSAGE_SHAPE");
            object[] args = call.Args; ProbeFiles.Require(args != null && args.Length == 0, "EXISTS_NO_ARGUMENTS");
            LogicalCallContext context = call.LogicalCallContext; string uri = call.Uri; ProbeFiles.Require(uri == null || uri.Length <= 1024, "EXISTS_URI_BOUND");
            var snapshot = new CacheCallSnapshot(policy.Getter, context, uri);
            ProbeFiles.Require(!shared.Faulted && !policy.Faulted && State.Entries == 0 && !State.Faulted, "EXISTS_CAUGHT_FAULT");
            record("CONTENT_EXISTS_FIXTURE reply=PREPARED value=False qualification=CONTROLLED_EMPTY_CONTENT_STORE_FIXTURE_NOT_REAL_STATE");
            IMessage response = new ReturnMessage((object)false, new object[0], 0, context, snapshot);
            ProbeFiles.Require(!shared.Faulted && !policy.Faulted && State.Entries == 0 && !State.Faulted, "EXISTS_REPLY_CAUGHT_FAULT");
            prepared = true; return response;
        } catch(Exception error) {
            shared.Fail(); policy.Fail(); if(writerRequest)productWriter.Fail(); int refusal = State.Deny(); denied = true;
            if(writerRequest&&refusal==1){
                try{Type errorType=error.GetType();string code=(errorType==typeof(IOException)||errorType==typeof(NotSupportedException))?error.Message:"NOT_LOGGED";
                    if(code==null||code.Length>128)code="NOT_LOGGED";foreach(char c in code)if(!((c>='A'&&c<='Z')||(c>='0'&&c<='9')||c=='_')){code="NOT_LOGGED";break;}
                    record("PROJECT_CONTENT_WRITER_FAILURE stage="+writerStage+" type="+ProjectConstructorProbe.Text(errorType.FullName)+" hresult="+error.HResult.ToString("X8")+" owned-code="+code+" cleanup-eligible-captured="+cleanupEligibleForDiagnostic+" values=NOT_INSPECTED");
                }catch{ /* Original failure stays fatal even when bounded diagnostic recording fails. */ }
            }
            if (refusal == 1 && count <= storeLimit && ProjectConstructorProbe.UndoFactoryCallbackExcluded())
                record("PROJECT_CONTENT_REFUSAL entry=" + count + " denial=1 " + ContentMethodDescriptor(method, contract) + " values=NOT_INSPECTED caller-origin=UNKNOWN");
            throw;
        } finally {
            try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
            catch { shared.Fail(); policy.Fail(); if(writerRequest)productWriter.Fail(); if (!denied) State.Deny(); throw; }
            finally { if(writerRequest)shared.ExitProductApplication(entered,writerPrepared,productWriter);else shared.ExitExists(entered, prepared, policy); }
        }

        }
    }
}

internal sealed class ProjectContentReference
{
    readonly object sync = new object(); readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
    int attempts, replies, lifetime; bool faulted;
    internal readonly MethodInfo Getter; internal readonly ProjectContentDenyProxy Proxy; internal readonly object Reference;
    internal ProjectContentReference(MethodInfo getter, Type contract, Action<string> record)
    { Getter = getter; Proxy = new ProjectContentDenyProxy(contract, record); Reference = Proxy.GetTransparentProxy(); }
    internal ProjectContentReference(MethodInfo getter, Type contract, Action<string> record, bool diagnoseFirst)
    { Getter = getter; Proxy = new ProjectContentDenyProxy(contract, record, diagnoseFirst); Reference = Proxy.GetTransparentProxy(); }
    internal ProjectContentReference(MethodInfo getter, Type contract, Action<string> record, ContentExistsPolicy policy)
    { Getter = getter; Proxy = new ProjectContentExistsProxy(contract, record, policy); Reference = Proxy.GetTransparentProxy(); }
    internal int Attempts { get { lock (sync) { return attempts; } } } internal int Replies { get { lock (sync) { return replies; } } }
    internal int Lifetime { get { lock (sync) { return lifetime; } } } internal bool Faulted { get { lock (sync) { return faulted; } } }
    internal void Fail() { lock (sync) { faulted = true; } }
    internal void Begin() { lock (sync) { if (lifetime != 0) { faulted = true; throw new NotSupportedException("CONTENT_LIFETIME_BEGIN"); } lifetime = 1; } }
    internal void Close() { lock (sync) { if (lifetime != 1) faulted = true; lifetime = 2; } }
    internal void Attempt(bool eligible,bool productCleanup=false)
    { lock (sync) { bool cleanup=productCleanup&&ProjectConstructorProbe.ProductFixtureMode();int maximum=cleanup?16:1;bool prior=attempts>=1&&replies>=1;
        if(attempts<maximum+1)attempts++;if(attempts>maximum||(cleanup&&!prior)||lifetime!=1||faulted||!eligible||Thread.CurrentThread.ManagedThreadId!=ownerThread||Proxy.State.Entries!=0||Proxy.State.Faulted){faulted=true;throw new NotSupportedException("CONTENT_REFERENCE_CONTEXT");} } }
    internal void Complete(bool accepted) { lock (sync) { if (accepted && !faulted && Proxy.State.Entries == 0 && !Proxy.State.Faulted) replies++; } }
}

// Necessary host adaptation: real WPF dispatcher and an explicitly empty data-store fixture.
internal sealed class SdkHostProxy : RealProxy
{
    readonly object sync=new object();readonly int owner=Thread.CurrentThread.ManagedThreadId;readonly Type contract;readonly string role;
    readonly MethodInfo query,invoke;readonly object emptyStores;readonly Action<string> record;readonly Dispatcher dispatcher;
    int entries,replies,lifetime;bool fault,inAction;
    internal SdkHostProxy(Type type,string kind,MethodInfo getter,MethodInfo action,object empty,Action<string> output):base(type)
    {contract=type;role=kind;query=getter;invoke=action;emptyStores=empty;record=output;if(kind=="DISPATCHER"){dispatcher=Dispatcher.CurrentDispatcher;ProbeFiles.Require(dispatcher.CheckAccess()&&Thread.CurrentThread.GetApartmentState()==ApartmentState.STA,"SDK_REAL_WPF_STA");}}
    internal bool Faulted{get{lock(sync){return fault;}}}
    int contextActions,contextActionReplies,contextActionsActive;
    internal void InvokeContextUiAction(Action action)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        bool started=false,prepared=false;
        try{
            lock(sync){if(contextActions<200001)contextActions++;ProbeFiles.Require(role=="DISPATCHER"&&lifetime==1&&!fault&&contextActions<=200000&&contextActionsActive<8&&ProjectConstructorProbe.ProductDataReadLifetime(),"SDK_CONTEXT_UI_DISPATCH_LIFETIME_OR_LIMIT");contextActionsActive++;started=true;}
            ProbeFiles.Require(action!=null&&action.GetType()==typeof(Action)&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(action),"SDK_CONTEXT_UI_EXACT_ACTION");
            Action original=delegate{using(IDisposable targetInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){ProbeFiles.Require(dispatcher.CheckAccess()&&!dispatcher.HasShutdownStarted&&!dispatcher.HasShutdownFinished&&ProjectConstructorProbe.ProductDataReadLifetime(),"SDK_CONTEXT_UI_ACTION_ON_REAL_DISPATCHER");action();}};
            if(dispatcher.CheckAccess())original();else dispatcher.Invoke(original,DispatcherPriority.Send,CancellationToken.None,TimeSpan.FromSeconds(2));
            lock(sync){ProbeFiles.Require(!fault&&lifetime==1&&ProjectConstructorProbe.ProductDataReadLifetime(),"SDK_CONTEXT_UI_ACTION_COMPLETION");prepared=true;}
        }catch{lock(sync){fault=true;}throw;}
        finally{lock(sync){if(started)contextActionsActive--;if(prepared&&!fault)contextActionReplies++;}}

        }
    }
    internal void Begin(){lock(sync){ProbeFiles.Require(lifetime==0,"SDK_HOST_BEGIN_ONCE");lifetime=1;}}
    internal void Close(){lock(sync){if(lifetime!=1)fault=true;lifetime=2;}}
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int count;lock(sync){if(entries<17)entries++;count=entries;}int previous=0;bool suspended=false,prepared=false,owns=false;MethodInfo method=null;
        try{bool eligible=ProjectConstructorProbe.SdkHostPhaseEligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}
            lock(sync){if(count>16||fault||lifetime!=1||Thread.CurrentThread.ManagedThreadId!=owner||!eligible||excluded){fault=true;throw new NotSupportedException("SDK_HOST_PHASE_OR_BUDGET:"+role);}}
            method=ProjectContentDenyProxy.CaptureContentMethod(message);bool action=invoke!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,invoke);ProbeFiles.Require(action||ProjectConstructorProbe.ExactRuntimeMethod(method,query),"SDK_HOST_UNSUPPORTED:"+role);
            var call=(IMethodCallMessage)message;int arity=action?1:0;ProbeFiles.Require(call.MethodName==method.Name&&call.ArgCount==arity&&call.InArgCount==arity,"SDK_HOST_MESSAGE_SHAPE");object[] values=call.Args;ProbeFiles.Require(values!=null&&values.Length==arity,"SDK_HOST_ARGUMENT_COUNT");object argument=action?values[0]:null;
            if(action)ProbeFiles.Require(argument!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(argument)&&argument.GetType()==typeof(Action),"SDK_HOST_EXACT_ACTION");
            var context=call.LogicalCallContext;string uri=call.Uri;ProbeFiles.Require(uri==null||uri.Length<=1024,"SDK_HOST_URI_BOUND");var snapshot=new ValidatorCallSnapshot(method,context,uri,action?new object[]{argument}:new object[0],action?new Type[]{typeof(Action)}:Type.EmptyTypes);
            object result;
            if(role=="DISPATCHER"){
                if(action){lock(sync){ProbeFiles.Require(!inAction,"SDK_HOST_NESTED_ACTION");inAction=true;owns=true;}if(suspended){ProjectConstructorProbe.RestoreProjectResourceCallback(previous);suspended=false;}dispatcher.Invoke((Action)argument,DispatcherPriority.Send,CancellationToken.None,TimeSpan.FromSeconds(2));result=null;}
                else result=dispatcher.CheckAccess();
            }else result=emptyStores;
            lock(sync){ProbeFiles.Require(!fault,"SDK_HOST_CAUGHT_FAULT");}record("SDK_HOST_REPLY role="+role+" method="+ProjectConstructorProbe.Text(method.Name)+" source="+(role=="DISPATCHER"?"REAL_WPF_DISPATCHER":"EXPLICIT_EMPTY_ENUMERATION_FIXTURE")+" values=NOT_LOGGED");
            IMessage response=new ReturnMessage(result,new object[0],0,context,snapshot);prepared=true;return response;
        }catch{lock(sync){fault=true;}if(count<=16)record("SDK_HOST_DENIED role="+role+" "+ProjectContentDenyProxy.ContentMethodDescriptor(method,contract)+" values=NOT_INSPECTED");throw;}
        finally{try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{lock(sync){fault=true;}throw;}finally{lock(sync){if(owns)inAction=false;if(prepared&&!fault)replies++;}}}

        }
    }
    internal void Audit(string phase){int e,r,l;bool f,active;lock(sync){e=entries;r=replies;l=lifetime;f=fault;active=inAction||contextActionsActive!=0;}try{if(f||e>16||r>e||active||ProjectConstructorProbe.SdkAuditLog(phase))record("SDK_HOST_AUDIT role="+role+" phase="+phase+" entries="+e+" replies="+r+" fault="+f+" lifetime="+l+" context-actions="+contextActions+" context-action-replies="+contextActionReplies);}finally{if(f||e>16||r>e||active||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&l!=2))throw new NotSupportedException("SDK_HOST_FINAL_DENIAL:"+role);GC.KeepAlive(dispatcher);GC.KeepAlive(emptyStores);}}
}

// SDK-only empty IdMapping cache fixture. No bytes/stream or unknown operation is supplied.
internal sealed class SdkMappingPolicy
{
    readonly object sync=new object();readonly int owner=Thread.CurrentThread.ManagedThreadId;readonly Action<string> record;
    internal readonly MethodInfo Getter,Exists;internal readonly object Reference;internal CacheReferenceState Shared;
    int lifetime,references,referenceReplies,reads,readReplies,denials;bool fault;
    internal SdkMappingPolicy(MethodInfo getter,Type stream,MethodInfo exists,Action<string> output)
    {Getter=getter;Exists=exists;record=output;Reference=new SdkMappingStreamProxy(stream,this).GetTransparentProxy();}
    internal void Bind(CacheReferenceState state){lock(sync){ProbeFiles.Require(Shared==null&&state!=null,"SDK_MAPPING_BIND_ONCE");Shared=state;}}
    internal void Begin(){lock(sync){ProbeFiles.Require(lifetime==0,"SDK_MAPPING_BEGIN_ONCE");lifetime=1;}}
    internal void Close(){lock(sync){if(lifetime!=1)fault=true;lifetime=2;}}
    internal void Fail(){lock(sync){fault=true;}}
    internal void Enter(bool reference,bool eligible){lock(sync){if(reference)references++;else reads++;if(!eligible||fault||lifetime!=1||owner!=Thread.CurrentThread.ManagedThreadId){fault=true;throw new NotSupportedException("SDK_MAPPING_PHASE");}}}
    internal void CompleteReference(bool accepted){lock(sync){if(accepted&&!fault)referenceReplies++;}}
    internal void CompleteRead(bool accepted){lock(sync){if(accepted&&!fault)readReplies++;}}
    internal void Deny(MethodInfo method){int n;lock(sync){fault=true;if(denials<17)denials++;n=denials;}if(n==1)record("SDK_MAPPING_DENIED "+ProjectContentDenyProxy.ContentMethodDescriptor(method,Exists.DeclaringType)+" values=NOT_INSPECTED caller-origin=UNKNOWN");}
    internal void Audit(string phase){bool f;int refs,replies,e,r,denied,l;lock(sync){f=fault;refs=references;replies=referenceReplies;e=reads;r=readReplies;denied=denials;l=lifetime;}try{if(f||denied!=0||Shared==null||Shared.Faulted||ProjectConstructorProbe.SdkAuditLog(phase))record("SDK_MAPPING_AUDIT phase="+phase+" reference-attempts="+refs+" reference-replies="+replies+" exists-attempts="+e+" exists-replies="+r+" denied="+denied+" fault="+f+" lifetime="+l+" fixture=EMPTY_NOT_REAL_STATE");}finally{if(f||denied!=0||Shared==null||Shared.Faulted||Shared.Entries>ProjectConstructorProbe.ProductStoreFinalEntryLimit()||replies>refs||r>e||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&l!=2))throw new NotSupportedException("SDK_MAPPING_FINAL_DENIAL");GC.KeepAlive(Reference);}}
    sealed class SdkMappingStreamProxy : RealProxy
    {
        readonly SdkMappingPolicy policy;
        internal SdkMappingStreamProxy(Type stream,SdkMappingPolicy p):base(stream){policy=p;}
        public override IMessage Invoke(IMessage message){
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){int count=policy.Shared.Count(),previous=0;bool suspended=false,entered=false,prepared=false;MethodInfo method=null;
            try{bool eligible=ProjectConstructorProbe.SdkWrapperEligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}policy.Shared.Enter(count,excluded);entered=true;policy.Enter(false,eligible);method=ProjectContentDenyProxy.CaptureContentMethod(message);ProbeFiles.Require(ProjectConstructorProbe.ExactRuntimeMethod(method,policy.Exists),"SDK_MAPPING_EXISTS_ONLY");var call=(IMethodCallMessage)message;ProbeFiles.Require(call.MethodName=="get_Exists"&&call.ArgCount==0&&call.InArgCount==0,"SDK_MAPPING_EXISTS_SHAPE");object[] args=call.Args;ProbeFiles.Require(args!=null&&args.Length==0,"SDK_MAPPING_ZERO_ARGS");var context=call.LogicalCallContext;string uri=call.Uri;ProbeFiles.Require(uri==null||uri.Length<=1024,"SDK_MAPPING_URI_BOUND");var snapshot=new CacheCallSnapshot(policy.Exists,context,uri);ProbeFiles.Require(!policy.Shared.Faulted,"SDK_MAPPING_CAUGHT_FAULT");IMessage response=new ReturnMessage((object)false,new object[0],0,context,snapshot);prepared=true;return response;}
            catch{policy.Shared.Fail();policy.Deny(method);throw;}
            finally{try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{policy.Shared.Fail();policy.Fail();throw;}finally{policy.Shared.ExitSdkRead(entered,prepared,policy);}}

        }
    }
    }
}

// Exploratory owned Guid fixture: coherent memory only, never persistent project state.
internal sealed class ProjectGuidFixture
{
    readonly object sync = new object(); readonly int owner = Thread.CurrentThread.ManagedThreadId;
    internal readonly MethodInfo Getter, Setter; CacheReferenceState shared; Guid value = Guid.Empty;
    int gets, sets, reads, writes; bool faulted;
    internal ProjectGuidFixture(MethodInfo get, MethodInfo set) { Getter = get; Setter = set; }
    internal void Bind(CacheReferenceState state) { lock (sync) { if (shared != null || state == null) { faulted = true; throw new NotSupportedException("GUID_BIND_ONCE"); } shared = state; } }
    internal void Fail() { lock (sync) { faulted = true; } }
    internal void Attempt(bool setter, bool eligible)
    { lock (sync) { if (setter) sets++; else gets++; if (shared == null || faulted || !eligible || Thread.CurrentThread.ManagedThreadId != owner) { faulted = true; throw new NotSupportedException("GUID_CALLBACK_CONTEXT"); } } }
    internal Guid Current { get { lock (sync) { return value; } } }
    internal bool Faulted { get { lock (sync) { return faulted; } } }
    internal void Complete(bool accepted, bool setter, Guid pending)
    { lock (sync) { if (accepted && !faulted) { if (setter) { value = pending; writes++; } else reads++; } } }
    internal void Audit(string phase, Action<string> record)
    {
        int g, s, r, w; bool bad; CacheReferenceState state;
        lock (sync) { g = gets; s = sets; r = reads; w = writes; bad = faulted; state = shared; }
        bool sharedFault = state != null && state.Faulted;
        try { if(bad||sharedFault||state==null||state.Entries>ProjectConstructorProbe.ProductStoreFinalEntryLimit()||r>g||w>s||ProjectConstructorProbe.InitializeAuditLog(phase))record("PROJECT_GUID_AUDIT phase=" + phase + " get-attempts=" + g + " set-attempts=" + s + " read-commits=" + r + " write-commits=" + w + " fault=" + bad + " shared-fault=" + sharedFault + " fixture-only=True values=NOT_LOGGED"); }
        finally { if (bad || sharedFault || state == null || state.Entries > ProjectConstructorProbe.ProductStoreFinalEntryLimit() || r > g || w > s) throw new NotSupportedException("PROJECT_GUID_FINAL_DENIAL"); }
    }
}
internal sealed class GuidCallSnapshot : IMethodCallMessage
{
    readonly MethodInfo method; readonly LogicalCallContext context; readonly string uri; readonly object[] arguments;
    readonly System.Collections.IDictionary properties = new System.Collections.Hashtable();
    internal GuidCallSnapshot(MethodInfo m, LogicalCallContext c, string u, bool setter, Guid value) { method=m; context=c; uri=u; arguments=setter ? new object[] { value } : new object[0]; }
    public MethodBase MethodBase { get { return method; } } public string MethodName { get { return method.Name; } }
    public int ArgCount { get { return arguments.Length; } } public int InArgCount { get { return arguments.Length; } }
    public object[] Args { get { return (object[])arguments.Clone(); } } public object[] InArgs { get { return (object[])arguments.Clone(); } }
    public bool HasVarArgs { get { return false; } } public LogicalCallContext LogicalCallContext { get { return context; } }
    public object MethodSignature { get { return arguments.Length==0 ? Type.EmptyTypes : new Type[] { typeof(Guid) }; } }
    public string TypeName { get { return method.DeclaringType.AssemblyQualifiedName; } } public string Uri { get { return uri; } set { throw new NotSupportedException("GUID_SNAPSHOT_IMMUTABLE"); } }
    public System.Collections.IDictionary Properties { get { return properties; } }
    public object GetArg(int i) { if(i!=0 || arguments.Length!=1) throw new NotSupportedException("GUID_ARG_INDEX"); return arguments[0]; }
    public string GetArgName(int i) { GetArg(i); return "value"; } public object GetInArg(int i) { return GetArg(i); } public string GetInArgName(int i) { return GetArgName(i); }
}

internal sealed class SdkProductApplicationPolicy
{
    readonly object sync=new object();readonly object application;readonly Func<object,object> read;readonly Action<string> record;readonly Type exactResult;readonly Func<bool> completionLifetime;
    internal readonly MethodInfo Method;int attempts,replies;bool fault;
    internal int Arity{get{return application==null?0:1;}}
    internal SdkProductApplicationPolicy(MethodInfo method,object selected,Func<object,object> original,Action<string> output,Type concreteResult=null,Func<bool> lifetime=null)
    {Method=method;application=selected;read=original;record=output;exactResult=concreteResult;completionLifetime=lifetime;}
    internal void Fail(){lock(sync){fault=true;}}
    internal object Read(object argument,bool eligible)
    {
        lock(sync){if(attempts<17)attempts++;ProbeFiles.Require(attempts<=16&&!fault&&eligible&&(application==null?argument==null:argument!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(argument)&&argument.GetType()==application.GetType()&&Object.Equals(argument,application)),"PRODUCT_APPLICATION_STORE_SELECTED_UI_ONLY");}
        object value=read(argument);ProbeFiles.Require(value!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)&&Method.ReturnType.IsInstanceOfType(value)&&(exactResult!=null?value.GetType()==exactResult:value.GetType().Module==Method.Module),"PRODUCT_APPLICATION_STORE_GENUINE_RETURN");return value;
    }
    internal void Complete(bool accepted)
    {lock(sync){if(accepted){if(fault||!(completionLifetime==null?ProjectConstructorProbe.ProductUiCacheLifetime():completionLifetime())){fault=true;throw new NotSupportedException("PRODUCT_APPLICATION_STORE_COMPLETION_LIFETIME");}replies++;}}}
    internal bool Used{get{lock(sync){return attempts!=0||replies!=0;}}}
    internal bool Faulted{get{lock(sync){return fault;}}}
    internal void Audit(string phase)
    {lock(sync){try{bool ui=ProjectConstructorProbe.CompactProductDiagnostics();if(fault||(ui?ProjectConstructorProbe.SdkAuditLog(phase)&&(attempts!=0||replies!=0):ProjectConstructorProbe.SdkAuditLog(phase)))record("PRODUCT_STORE_REFERENCE_AUDIT member="+Method.Name+" phase="+phase+" attempts="+attempts+" reply-commits="+replies+" fault="+fault+(ui&&!fault?String.Empty:" content=ORIGINAL_IMPORTED_STORE caller-receipt=UNKNOWN"));}finally{ProbeFiles.Require(!fault&&attempts<=16&&replies<=attempts,"PRODUCT_APPLICATION_STORE_FINAL_DENIAL");GC.KeepAlive(application);}}}
}
internal sealed class LargeStringCacheStoreProxy : ProjectStoreIdProxy
{
    readonly MethodInfo idGetter, cacheGetter; readonly object reference; readonly Action<string> record;
    internal readonly CacheReferenceState State = new CacheReferenceState();
    readonly ProjectContentReference content; readonly ProjectGuidFixture guid; SdkMappingPolicy sdkMapping;
    internal void ConfigureSdkMapping(SdkMappingPolicy policy){ProbeFiles.Require(ProjectConstructorProbe.SdkMode&&sdkMapping==null&&policy!=null,"SDK_MAPPING_WRAPPER");sdkMapping=policy;policy.Bind(State);}
    internal LargeStringCacheStoreProxy(Type owner, MethodInfo idMethod, object id, MethodInfo cacheMethod, Type streamType, object stream, Action<string> recorder)
        : base(owner, idMethod, id, recorder)
    { ProjectConstructorProbe.CacheGetterSignature(owner, cacheMethod, streamType); ProbeFiles.Require(ProjectConstructorProbe.CacheMode && stream != null && streamType.IsInstanceOfType(stream), "CACHE_REFERENCE_CONTRACT"); idGetter = idMethod; cacheGetter = cacheMethod; reference = stream; record = recorder; }
    internal LargeStringCacheStoreProxy(Type owner, MethodInfo idMethod, object id, MethodInfo cacheMethod, Type streamType, object stream, Action<string> recorder, ProjectContentReference policy)
        : this(owner, idMethod, id, cacheMethod, streamType, stream, recorder)
    { ProbeFiles.Require(ProjectConstructorProbe.ContentMode && policy != null && policy.Getter.DeclaringType == owner && policy.Reference != null, "CONTENT_WRAPPER_POLICY"); content = policy; }
    internal LargeStringCacheStoreProxy(Type owner, MethodInfo idMethod, object id, MethodInfo cacheMethod, Type streamType, object stream, Action<string> recorder, ProjectContentReference policy, ProjectGuidFixture fixture)
        : this(owner, idMethod, id, cacheMethod, streamType, stream, recorder, policy)
    { guid = fixture; ProbeFiles.Require(guid != null, "GUID_WRAPPER_POLICY"); guid.Bind(State); }
    int parameterStoreFailureRecorded;
    SdkProductApplicationPolicy productApplication,productStringStream,productFilePath;
    SdkProductApplicationPolicy[] productManufacturerReads,productProjectReads;
    internal void ConfigureProductProjectReads(SdkProductApplicationPolicy[] policies)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productProjectReads==null&&policies!=null&&policies.Length==18,"PRODUCT_PROJECT_READS_CONFIGURE_ONCE");productProjectReads=(SdkProductApplicationPolicy[])policies.Clone();}
    internal void ConfigureProductManufacturerReads(SdkProductApplicationPolicy[] policies)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productManufacturerReads==null&&policies!=null&&policies.Length==3,"PRODUCT_MANUFACTURER_READ_CONFIGURE_ONCE");productManufacturerReads=(SdkProductApplicationPolicy[])policies.Clone();}
    internal void ConfigureProductApplication(SdkProductApplicationPolicy policy)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productApplication==null&&policy!=null,"PRODUCT_APPLICATION_STORE_CONFIGURE_ONCE");productApplication=policy;}
    internal void ConfigureProductStringStream(SdkProductApplicationPolicy policy)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productStringStream==null&&policy!=null&&policy.Arity==0,"PRODUCT_LARGE_STRING_STORE_CONFIGURE_ONCE");productStringStream=policy;}
    internal void ConfigureProductFilePath(SdkProductApplicationPolicy policy)
    {ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&productFilePath==null&&policy!=null&&policy.Arity==0&&policy.Method.ReturnType==typeof(string),"PRODUCT_FILE_PATH_CONFIGURE_ONCE");productFilePath=policy;}
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int storeLimit=ProjectConstructorProbe.ProductStoreCallbackLimit();int count = State.Count(storeLimit); bool suspended = false, entered = false, prepared = false; int previous = 0;
        MethodInfo observedMethod = null; bool methodCaptured = false, diagnosticAttempted = false; string phase = "ENTRY";
        bool mappingRequest=false,mappingPrepared=false,productApplicationRequest=false,productApplicationPrepared=false;SdkProductApplicationPolicy selectedProductStore=null;
        bool contentRequest = false, contentPrepared = false, guidRequest = false, guidSetter = false, guidPrepared = false; Guid pendingGuid = Guid.Empty;
        bool parameterStorePhase=false,entryExcluded=false;int parameterStoreSlot=-1;
        try {
            bool productApplicationEligible=ProjectConstructorProbe.ProductUiCacheEligible();
            parameterStorePhase=ProjectConstructorProbe.ParameterDefinitionCacheEligible();
            bool productFilePathEligible=ProjectConstructorProbe.ProductStoreDisposalEligible();
            bool mappingEligible=ProjectConstructorProbe.SdkWrapperEligible();
            bool eligible = ProjectConstructorProbe.CacheWindowEligible(), excluded = ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            entryExcluded=excluded;
            if (!excluded) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            State.Enter(count, excluded,storeLimit); entered = true;
            phase = "METHOD_READ";
            IMethodCallMessage call = message as IMethodCallMessage; MethodInfo method = call == null ? null : call.MethodBase as MethodInfo;
            observedMethod = method; methodCaptured = true; phase = "METHOD_MATCH";
            bool cache = ProjectConstructorProbe.ExactRuntimeMethod(method, cacheGetter), id = ProjectConstructorProbe.ExactRuntimeMethod(method, idGetter);
            if(productApplication!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productApplication.Method))selectedProductStore=productApplication;
            else if(productStringStream!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productStringStream.Method))selectedProductStore=productStringStream;
            else if(productFilePath!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productFilePath.Method))selectedProductStore=productFilePath;
            if(selectedProductStore==null&&productManufacturerReads!=null)foreach(var candidate in productManufacturerReads)if(ProjectConstructorProbe.ExactRuntimeMethod(method,candidate.Method)){selectedProductStore=candidate;break;}
            if(selectedProductStore==null&&productProjectReads!=null)foreach(var candidate in productProjectReads)if(ProjectConstructorProbe.ExactRuntimeMethod(method,candidate.Method)){selectedProductStore=candidate;break;}
            productApplicationRequest=selectedProductStore!=null;
            if(productApplicationRequest){
                phase="PRODUCT_APPLICATION_SHAPE";ProbeFiles.Require(call.MethodName==selectedProductStore.Method.Name&&call.ArgCount==selectedProductStore.Arity&&call.InArgCount==selectedProductStore.Arity,"PRODUCT_APPLICATION_STORE_CALL_SHAPE");
                object[] values=call.Args;ProbeFiles.Require(values!=null&&values.Length==selectedProductStore.Arity,"PRODUCT_APPLICATION_STORE_ARGUMENTS");object argument=selectedProductStore.Arity==0?null:values[0];
                var productContext=call.LogicalCallContext;string productUri=call.Uri;ProbeFiles.Require(productUri==null||productUri.Length<=1024,"PRODUCT_APPLICATION_STORE_URI");
                parameterStoreSlot=Object.ReferenceEquals(selectedProductStore,productApplication)?0:Object.ReferenceEquals(selectedProductStore,productStringStream)?4:productManufacturerReads==null?-1:Array.IndexOf(productManufacturerReads,selectedProductStore)+1;
                if(parameterStoreSlot==0&&!Object.ReferenceEquals(selectedProductStore,productApplication))parameterStoreSlot=-1;
                phase="PRODUCT_APPLICATION_ORIGINAL";object value=selectedProductStore.Read(argument,Object.ReferenceEquals(selectedProductStore,productFilePath)||(productProjectReads!=null&&Array.IndexOf(productProjectReads,selectedProductStore)>=0)?productFilePathEligible:productApplicationEligible||ReferenceDcaRequiredParameters.AllowsProductStoreRead(parameterStorePhase,parameterStoreSlot));
                var productSnapshot=new ValidatorCallSnapshot(selectedProductStore.Method,productContext,productUri,selectedProductStore.Arity==0?new object[0]:new object[]{argument},selectedProductStore.Arity==0?Type.EmptyTypes:new Type[]{selectedProductStore.Method.GetParameters()[0].ParameterType});
                ProbeFiles.Require(!State.Faulted,"PRODUCT_APPLICATION_STORE_SHARED_FAULT");
                record("PRODUCT_STORE_REFERENCE member="+selectedProductStore.Method.Name+" reply=PREPARED source=ORIGINAL_OWNED_SCRATCH_STORE selected-application=EXACT_NO_REMAP values=NOT_LOGGED caller-receipt=UNKNOWN");
                IMessage productResponse=new ReturnMessage(value,new object[0],0,productContext,productSnapshot);ProbeFiles.Require(!State.Faulted,"PRODUCT_APPLICATION_STORE_REPLY_FAULT");productApplicationPrepared=true;return productResponse;
            }
            mappingRequest=sdkMapping!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,sdkMapping.Getter);
            if(mappingRequest){phase="SDK_MAPPING_REFERENCE";sdkMapping.Enter(true,mappingEligible);ProbeFiles.Require(call.MethodName==sdkMapping.Getter.Name&&call.ArgCount==0&&call.InArgCount==0,"SDK_MAPPING_REFERENCE_SHAPE");object[] values=call.Args;ProbeFiles.Require(values!=null&&values.Length==0,"SDK_MAPPING_REFERENCE_ARGS");var mappingContext=call.LogicalCallContext;string mappingUri=call.Uri;ProbeFiles.Require(mappingUri==null||mappingUri.Length<=1024,"SDK_MAPPING_REFERENCE_URI");var mappingSnapshot=new CacheCallSnapshot(sdkMapping.Getter,mappingContext,mappingUri);ProbeFiles.Require(!State.Faulted,"SDK_MAPPING_REFERENCE_FAULT");record("SDK_MAPPING_REFERENCE reply=PREPARED fixture=EMPTY_DENIAL_REFERENCE");IMessage mappingResponse=new ReturnMessage(sdkMapping.Reference,new object[0],0,mappingContext,mappingSnapshot);mappingPrepared=true;return mappingResponse;}
            guidSetter = guid != null && ProjectConstructorProbe.ExactRuntimeMethod(method, guid.Setter);
            guidRequest = guid != null && (guidSetter || ProjectConstructorProbe.ExactRuntimeMethod(method, guid.Getter));
            if (guidRequest) {
                phase = "GUID_SHAPE"; guid.Attempt(guidSetter, eligible);
                ProbeFiles.Require(call.MethodName == (guidSetter ? guid.Setter.Name : guid.Getter.Name) && call.ArgCount == (guidSetter ? 1 : 0) && call.InArgCount == (guidSetter ? 1 : 0), "GUID_MESSAGE_SHAPE");
                object[] values = call.Args; ProbeFiles.Require(values != null && values.Length == (guidSetter ? 1 : 0), "GUID_ARGUMENT_COUNT");
                if (guidSetter) { object argument = values[0]; ProbeFiles.Require(argument != null && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(argument) && argument.GetType() == typeof(Guid), "GUID_EXACT_BOXED_VALUE"); pendingGuid = (Guid)argument; }
                else pendingGuid = guid.Current;
                LogicalCallContext guidContext = call.LogicalCallContext; string guidUri = call.Uri; ProbeFiles.Require(guidUri == null || guidUri.Length <= 1024, "GUID_URI_BOUND");
                var guidSnapshot = new GuidCallSnapshot(guidSetter ? guid.Setter : guid.Getter, guidContext, guidUri, guidSetter, pendingGuid);
                ProbeFiles.Require(!State.Faulted && !guid.Faulted, "GUID_CAUGHT_FAULT");
                record("PROJECT_GUID_FIXTURE operation=" + (guidSetter ? "SET" : "GET") + " reply=PREPARED fixture-only=True values=NOT_LOGGED");
                IMessage guidResponse = new ReturnMessage(guidSetter ? null : (object)pendingGuid, new object[0], 0, guidContext, guidSnapshot);
                ProbeFiles.Require(!State.Faulted && !guid.Faulted, "GUID_REPLY_FAULT"); guidPrepared = true; return guidResponse;
            }
            contentRequest = content != null && ProjectConstructorProbe.ExactRuntimeMethod(method, content.Getter);
            if (contentRequest) { phase = "CONTENT_ELIGIBILITY"; content.Attempt(eligible||productFilePathEligible,productFilePathEligible); }
            phase = "CACHE_ELIGIBILITY";
            if (cache) State.CacheAttempt(eligible);
            phase = "MESSAGE_SHAPE";
            ProbeFiles.Require((cache || id || contentRequest) && call.MethodName == (contentRequest ? content.Getter.Name : cache ? cacheGetter.Name : idGetter.Name) && call.ArgCount == 0 && call.InArgCount == 0, "CACHE_MESSAGE_SHAPE");
            phase = "ARGS_READ";
            object[] arguments = call.Args; phase = "ARGS_SHAPE"; ProbeFiles.Require(arguments != null && arguments.Length == 0, "CACHE_MESSAGE_ARGUMENT_SHAPE");
            phase = "CALL_CONTEXT_READ";
            LogicalCallContext context = call.LogicalCallContext; phase = "URI_READ"; string uri = call.Uri;
            phase = "URI_SHAPE";
            ProbeFiles.Require(uri == null || uri.Length <= 1024, "CACHE_MESSAGE_URI_BOUND");
            phase = "SNAPSHOT";
            var snapshot = new CacheCallSnapshot(contentRequest ? content.Getter : cache ? cacheGetter : idGetter, context, uri);
            phase = "REENTRY_GUARD";
            ProbeFiles.Require(!State.Faulted, "CACHE_MESSAGE_CAUGHT_REENTRY");
            if (!cache && !contentRequest) {
                phase = "ID_REPLY";
                IMessage result = base.InvokeCore(snapshot);
                var returned = result as IMethodReturnMessage; ProbeFiles.Require(returned != null && returned.Exception == null && !State.Faulted, "CACHE_ID_REPLY_FAULT"); return result;
            }
            if (contentRequest) {
                phase = "CONTENT_RECORD";
                record("PROJECT_CONTENT_REFERENCE reply=PREPARED same-preexisting-reference=True kind=CONTROLLED_DENIAL_REFERENCE values=NOT_PROVIDED");
                phase = "CONTENT_REPLY";
                IMessage contentResponse = new ReturnMessage(content.Reference, new object[0], 0, context, snapshot);
                ProbeFiles.Require(!State.Faulted && !content.Faulted && content.Proxy.State.Entries == 0 && !content.Proxy.State.Faulted, "CONTENT_REPLY_CAUGHT_FAULT");
                contentPrepared = true; return contentResponse;
            }
            phase = "CACHE_RECORD";
            record("LARGE_STRING_CACHE_REFERENCE reply=PREPARED same-preexisting-reference=True provider-values=NOT_PROVIDED");
            phase = "CACHE_REPLY";
            IMessage response = new ReturnMessage(reference, new object[0], 0, context, snapshot);
            ProbeFiles.Require(!State.Faulted, "CACHE_REPLY_CAUGHT_REENTRY"); prepared = true; return response;
        } catch (Exception error) {
            State.Fail();
            if(productApplicationRequest)selectedProductStore.Fail();
                if(mappingRequest)sdkMapping.Fail();
            if (guidRequest) guid.Fail();
            if (contentRequest) content.Fail();
            if(parameterStorePhase&&Interlocked.CompareExchange(ref parameterStoreFailureRecorded,1,0)==0){
                try{record("STORE_PARAMETER_READ_FAILURE phase="+phase+" count="+count+" policy-slot="+parameterStoreSlot+" excluded-at-entry="+entryExcluded+" owns-entry="+entered+" "+State.EntryDiagnostic()+" type="+ProjectConstructorProbe.Text(error.GetType().FullName)+" message="+ProjectConstructorProbe.Text(error.Message)+" arguments=NOT_READ_BY_DIAGNOSTIC");}
                catch(Exception diagnosticError){throw new AggregateException("STORE_PARAMETER_READ_DIAGNOSTIC",error,diagnosticError);}
            }
            DiagnoseFailure(observedMethod, methodCaptured, phase, count, entered, ref diagnosticAttempted);
            throw;
        }
        finally {
            try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); }
            catch {
                State.Fail();
                if(productApplicationRequest)selectedProductStore.Fail();
                if(mappingRequest)sdkMapping.Fail();
            if (guidRequest) guid.Fail();
                if (contentRequest) content.Fail();
                DiagnoseFailure(observedMethod, methodCaptured, "RESTORE", count, entered, ref diagnosticAttempted);
                throw;
            }
            finally {
                if(productApplicationRequest)State.ExitProductApplication(entered,productApplicationPrepared,selectedProductStore);
                else if(mappingRequest)State.ExitSdkMapping(entered,mappingPrepared,sdkMapping);
                else if (guidRequest) State.ExitGuid(entered, guidPrepared, guidSetter, pendingGuid, guid);
                else if (content == null) State.Exit(entered, prepared);
                else State.ExitContent(entered, prepared, contentPrepared, content);
            }
        }

        }
    }
    void DiagnoseFailure(MethodInfo method, bool captured, string phase, int count, bool entered, ref bool attempted)
    {
        if (!ProjectConstructorProbe.StoreRequestDiagnostics || attempted || !entered || count > ProjectConstructorProbe.ProductStoreFinalEntryLimit() || !ProjectConstructorProbe.UndoFactoryCallbackExcluded()) return;
        attempted = true;
        record("STORE_REQUEST_DIAGNOSTIC phase=" + phase + " entry=" + count + " " + StoreRequestDescriptor(method, cacheGetter, captured) + " values=NOT_INSPECTED"+(ProjectConstructorProbe.ProductFixtureMode()?" product-ui-active="+ProjectConstructorProbe.ProductUiCacheLifetime()+" thread="+Thread.CurrentThread.ManagedThreadId+" caller-origin=UNKNOWN":""));
    }
    internal static string StoreRequestDescriptor(MethodInfo method, MethodInfo anchor, bool captured)
    {
        if (!captured || method == null) return "category=MEMBER_UNAVAILABLE";
        if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method) || method.GetType() != typeof(object).GetMethod("GetType").GetType()) return "category=UNTRUSTED_METHOD_KIND";
        Type owner = anchor.DeclaringType;
        if (method.Module != anchor.Module || method.DeclaringType != owner) return ProjectConstructorProbe.ProductFixtureMode()?ProjectContentDenyProxy.ContentMethodDescriptor(method,owner):"category=FOREIGN_OWNER";
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
// Exact validator factory plus four explicit context fixtures, GUID diagnostic profile only.
internal sealed class ValidatorCallSnapshot : IMethodCallMessage
{
    readonly MethodInfo method; readonly LogicalCallContext context; readonly string uri; readonly object[] args; readonly Type[] signature;
    readonly System.Collections.IDictionary properties = new System.Collections.Hashtable();
    internal ValidatorCallSnapshot(MethodInfo m, LogicalCallContext c, string u, object[] a, Type[] s) { method=m;context=c;uri=u;args=(object[])a.Clone();signature=(Type[])s.Clone(); }
    public MethodBase MethodBase { get { return method; } } public string MethodName { get { return method.Name; } }
    public int ArgCount { get { return args.Length; } } public int InArgCount { get { return args.Length; } }
    public object[] Args { get { return (object[])args.Clone(); } } public object[] InArgs { get { return (object[])args.Clone(); } }
    public bool HasVarArgs { get { return false; } } public LogicalCallContext LogicalCallContext { get { return context; } }
    public object MethodSignature { get { return (Type[])signature.Clone(); } } public string TypeName { get { return method.DeclaringType.AssemblyQualifiedName; } }
    public string Uri { get { return uri; } set { throw new NotSupportedException("VALIDATOR_SNAPSHOT_IMMUTABLE"); } }
    public System.Collections.IDictionary Properties { get { return properties; } }
    public object GetArg(int i) { return args[i]; } public object GetInArg(int i) { return args[i]; }
    public string GetArgName(int i) { if(i<0||i>=args.Length)throw new NotSupportedException("VALIDATOR_ARG_INDEX");return i==0?"project":"collection"; }
    public string GetInArgName(int i) { return GetArgName(i); }
}
internal sealed class SdkChildValidatorCase
{internal MethodInfo Method;internal ConstructorInfo Constructor;internal object Child,Collection,Instance;internal int Constructions;}
internal sealed class ContextValidatorAdapter
{
    readonly object sync=new object(); readonly int owner=Thread.CurrentThread.ManagedThreadId;
    readonly Type projectType,collectionType,checkerType; readonly ConstructorInfo constructor; readonly MethodInfo closedMethod;
    readonly MethodInfo[] scalarMethods; readonly Type[] validatorParameters; readonly object style;
    readonly Func<object> collection; readonly Func<bool> eligible; readonly Action<string> record;
    int uiProjectReferences,uiOtherCalls,uiProjectReplies,uiOtherReplies;
    int entries,replies,constructions,lifetime,nextPuid=1; bool fault,entered,constructorActive,scalarActive,dirty;
    int activeCallbackThread,activeCallbackOperation=-1;
    object project,checker,retainedChecker,notification,retainedNotification,undoManager,retainedUndoManager;
    readonly List<SdkChildValidatorCase> childValidators=new List<SdkChildValidatorCase>();
    internal sealed class UiValidatorType{internal Type ObjectType,CheckerType;internal MethodInfo Method,Parent,Context,Project;internal ConstructorInfo Constructor;}
    sealed class UiValidatorEntry{internal UiValidatorType Type;internal object Child,Collection,Instance,Retained;}
    sealed class UiObjectIdentityComparer:IEqualityComparer<object>
    {public new bool Equals(object left,object right){return Object.ReferenceEquals(left,right);}public int GetHashCode(object value){return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);}}
    UiValidatorType[] uiValidatorTypes;readonly Dictionary<object,UiValidatorEntry> uiValidatorEntries=new Dictionary<object,UiValidatorEntry>(new UiObjectIdentityComparer());
    internal void ConfigureUiValidators(UiValidatorType[] types,object expectedProject)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductDefinitionContextRequested()&&lifetime==1&&!fault&&uiValidatorTypes==null&&types.Length==2&&Object.ReferenceEquals(project,expectedProject),"PRODUCT_UI_VALIDATOR_CONFIGURE_ONCE");uiValidatorTypes=(UiValidatorType[])types.Clone();}}
    internal void ConfigureChildValidator(SdkChildValidatorCase value)
    {lock(sync){ProbeFiles.Require(lifetime==1&&!fault&&childValidators.Count<2,"SDK_CHILD_VALIDATOR_CONFIGURE");foreach(var existing in childValidators)ProbeFiles.Require(!Object.ReferenceEquals(existing.Child,value.Child)&&!existing.Method.Equals(value.Method),"SDK_CHILD_VALIDATOR_UNIQUE");childValidators.Add(value);}}
    MethodInfo idCreatorGetter;ConstructorInfo idCreatorConstructor;Type idCreatorType;Func<object> idContext;object idCreator,retainedIdCreator;int idCreatorConstructions;
    internal void ConfigureIdCreator(MethodInfo getter,ConstructorInfo ctor,Type concrete,Func<object> context)
    {ProbeFiles.Require(lifetime==0&&idCreatorGetter==null,"SDK_ID_CREATOR_CONFIGURE_ONCE");idCreatorGetter=getter;idCreatorConstructor=ctor;idCreatorType=concrete;idContext=context;}
    MethodInfo[] canonicalReferenceMethods;Func<object>[] canonicalReferenceValues;
    internal void ConfigureCanonicalReferences(MethodInfo[] methods,Func<object>[] values)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.DeviceFixtureMode()&&lifetime==1&&!fault&&canonicalReferenceMethods==null&&methods.Length==10&&values.Length==10,"SDK_CANONICAL_REFERENCES_CONFIGURE_ONCE");canonicalReferenceMethods=(MethodInfo[])methods.Clone();canonicalReferenceValues=(Func<object>[])values.Clone();}}
    MethodInfo[] productCacheMethods;Func<int,object,object> productCache;readonly object productUiCacheGate=new object();
    sealed class UiMethodCount{internal string Label;internal int Count;}
    readonly Dictionary<MethodInfo,UiMethodCount> uiMethodCounts=new Dictionary<MethodInfo,UiMethodCount>();bool budgetDiagnosticRecorded;string lastUiMethod="NONE";
    void BudgetDiagnostic(int count,int limit,string bucket="GLOBAL",int bucketCount=0,int bucketLimit=0)
    {
        List<UiMethodCount> snapshot;bool before;
        lock(sync){if(budgetDiagnosticRecorded)return;budgetDiagnosticRecorded=true;before=fault;fault=true;snapshot=new List<UiMethodCount>();foreach(var value in uiMethodCounts.Values)snapshot.Add(new UiMethodCount{Label=value.Label,Count=value.Count});}
        snapshot.Sort(delegate(UiMethodCount left,UiMethodCount right){return right.Count.CompareTo(left.Count);});
        record("CONTEXT_ADAPTER_BUDGET_DIAGNOSTIC count="+count+" limit="+limit+" thread="+Thread.CurrentThread.ManagedThreadId+" owner="+owner+" fault-before="+before+" bucket="+bucket+" bucket-count="+bucketCount+" bucket-limit="+bucketLimit+" current-operation="+(bucket=="GLOBAL"?"NOT_READ_AFTER_CAP":"EXACT_METHOD_ONLY_WITHIN_GLOBAL_CAP")+" current-arguments=NOT_READ last-selected="+lastUiMethod+" "+ProjectConstructorProbe.ContextBudgetModeDescription()+" summary-kind=SELECTED_METHOD_ATTEMPTS values=NOT_INSPECTED");
        for(int i=0;i<snapshot.Count&&i<6;i++)record("CONTEXT_ADAPTER_METHOD_SUMMARY rank="+(i+1)+" method="+snapshot[i].Label+" selected-attempts="+snapshot[i].Count);
    }
    MethodInfo[] productCacheWeakMethods;Func<bool,object,object> productCacheWeak;
    internal void ConfigureProductCacheWeak(MethodInfo[] methods,Func<bool,object,object> invoke)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productCacheWeakMethods==null&&methods.Length==2&&invoke!=null,"PRODUCT_CACHE_WEAK_CONFIGURE_ONCE");productCacheWeakMethods=(MethodInfo[])methods.Clone();productCacheWeak=invoke;}}
    MethodInfo productCacheReleaseMethod;Action productCacheRelease;
    internal void ConfigureProductCacheRelease(MethodInfo method,Action invoke)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productCacheReleaseMethod==null&&invoke!=null,"PRODUCT_CACHE_RELEASE_CONFIGURE_ONCE");productCacheReleaseMethod=method;productCacheRelease=invoke;}}
    MethodInfo productFacadeGetter;object productFacadeCache;
    MethodInfo productLargeStringGetter;object productLargeStrings;
    MethodInfo productMasterMethod;object productMasterReference,productMasterFactory;
    MethodInfo productDispatchMethod;Action<Action> productDispatch;
    internal void ConfigureProductDispatcher(MethodInfo method,Action<Action> invoke)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductDefinitionContextRequested()&&lifetime==1&&!fault&&productDispatchMethod==null&&invoke!=null,"PRODUCT_CONTEXT_DISPATCH_CONFIGURE_ONCE");productDispatchMethod=method;productDispatch=invoke;}}
    internal void ConfigureProductMaster(MethodInfo method,object master,object factory)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productMasterMethod==null&&master!=null&&master.GetType()==method.ReturnType&&factory!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(factory),"PRODUCT_CONTEXT_MASTER_CONFIGURE_ONCE");productMasterMethod=method;productMasterReference=master;productMasterFactory=factory;}}
    internal void ConfigureProductLargeStrings(MethodInfo getter,object value)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productLargeStringGetter==null&&value!=null&&value.GetType()==getter.ReturnType,"PRODUCT_LARGE_STRING_CONTEXT_CONFIGURE_ONCE");productLargeStringGetter=getter;productLargeStrings=value;}}
    internal void ConfigureProductFacadeCache(MethodInfo getter,object value)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productFacadeGetter==null&&value!=null&&value.GetType()==getter.ReturnType,"PRODUCT_FACADE_CACHE_CONFIGURE_ONCE");productFacadeGetter=getter;productFacadeCache=value;}}
    internal void ConfigureProductCaches(MethodInfo[] methods,Func<int,object,object> values)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&lifetime==1&&!fault&&productCacheMethods==null&&methods.Length==7,"PRODUCT_IMPORT_CONTEXT_CONFIGURE_ONCE");productCacheMethods=(MethodInfo[])methods.Clone();productCache=values;}}
    MethodInfo allDevicesGetter;Func<object> allDevicesReference;Func<bool> allDevicesEligible;
    internal void ConfigureAllDevices(MethodInfo getter,Func<object> reference,Func<bool> phase)
    {lock(sync){ProbeFiles.Require(lifetime==1&&!fault&&allDevicesGetter==null,"SDK_ALL_DEVICES_CONFIGURE_ONCE");allDevicesGetter=getter;allDevicesReference=reference;allDevicesEligible=phase;}}
    MethodInfo writeGetter;object writeReference;Func<bool> writeEligible;
    internal void ConfigureWrite(MethodInfo getter,object reference,Func<bool> phase)
    {lock(sync){ProbeFiles.Require(lifetime==1&&writeGetter==null&&!fault,"SDK_WRITE_CONTEXT_CONFIGURE_ONCE");writeGetter=getter;writeReference=reference;writeEligible=phase;}}
    MethodInfo undoGetter;Type undoType;Func<object> undoFactory;Func<bool> undoEligible;int undoFactoryCalls,undoReplies;
    internal void ConfigureUndo(MethodInfo getter,Type resultType,Func<object> create,Func<bool> phase)
    { ProbeFiles.Require(lifetime==0&&undoGetter==null,"UNDO_CONTEXT_CONFIGURE_ONCE");undoGetter=getter;undoType=resultType;undoFactory=create;undoEligible=phase; }
    MethodInfo notificationGetter;ConstructorInfo notificationConstructor;Type notificationType;Func<bool> notificationEligible;int notificationConstructions,notificationReplies;
    internal void ConfigureNotification(MethodInfo getter,ConstructorInfo ctor,Type concrete,Func<bool> phase)
    { ProbeFiles.Require(lifetime==0&&notificationGetter==null,"NOTIFICATION_CONFIGURE_ONCE");notificationGetter=getter;notificationConstructor=ctor;notificationType=concrete;notificationEligible=phase; }
    internal ContextValidatorAdapter(Type projectClass,Type collectionClass,Type checkerClass,ConstructorInfo ctor,MethodInfo closed,MethodInfo[] scalar,Type[] parameters,object styleValue,Func<object> getCollection,Func<bool> phase,Action<string> recorder)
    { projectType=projectClass;collectionType=collectionClass;checkerType=checkerClass;constructor=ctor;closedMethod=closed;scalarMethods=scalar;validatorParameters=parameters;style=styleValue;collection=getCollection;eligible=phase;record=recorder; }
    internal void Begin(){lock(sync){if(lifetime!=0){fault=true;throw new NotSupportedException("VALIDATOR_BEGIN_ONCE");}lifetime=1;}}
    internal void Close(){lock(sync){if(lifetime!=1)fault=true;lifetime=2;}}
    void Fail(){lock(sync){fault=true;}}
    bool firstFailureDiagnosticRecorded;
    internal void ParameterReadEntrySnapshot()
    {
        int e,r,l;bool f,pending;lock(sync){e=entries;r=replies;l=lifetime;f=fault;pending=entered||constructorActive||scalarActive;}
        record("DCA_PARAMETER_CONTEXT_SNAPSHOT entries="+e+" replies="+r+" lifetime="+l+" fault="+f+" pending="+pending+" owner-thread="+owner+" current-thread="+Thread.CurrentThread.ManagedThreadId);
    }
    void FirstFailure(Exception error,string stage,int count,int limit,int operation,bool faultAtEntry,MethodInfo method)
    {
        if(!ProjectConstructorProbe.ProductFixtureMode())return;
        lock(sync){if(firstFailureDiagnosticRecorded)return;firstFailureDiagnosticRecorded=true;}
        try{record("CONTEXT_ADAPTER_FIRST_FAILURE stage="+stage+" count="+count+" limit="+limit+" operation="+operation+" fault-at-entry="+faultAtEntry+" owner-thread="+owner+" current-thread="+Thread.CurrentThread.ManagedThreadId
            +" type="+ProjectConstructorProbe.Text(error.GetType().FullName)+" hresult="+error.HResult.ToString("X8")+" message="+ProjectConstructorProbe.Text(error.Message)+" "+ProjectContentDenyProxy.ContentMethodDescriptor(method,closedMethod.DeclaringType)+" arguments=NOT_READ_BY_DIAGNOSTIC");
            Exception cause=error.InnerException;int depth=1;
            for(;cause!=null&&depth<4;depth++,cause=cause.InnerException)record("CONTEXT_ADAPTER_FIRST_FAILURE_CAUSE depth="+depth+" type="+ProjectConstructorProbe.Text(cause.GetType().FullName)+" hresult="+cause.HResult.ToString("X8")+" message="+ProjectConstructorProbe.Text(cause.Message));
            if(cause!=null)record("CONTEXT_ADAPTER_FIRST_FAILURE_CAUSE truncated=True maximum=4");}
        catch(Exception diagnosticError){throw new AggregateException("CONTEXT_ADAPTER_FIRST_FAILURE_DIAGNOSTIC",error,diagnosticError);}
    }
    static string ArgumentType(object value)
    { if(value==null)return "NULL";if(System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value))return "TRANSPARENT_PROXY";return ProjectConstructorProbe.Text(value.GetType().FullName); }
    internal IMessage Invoke(IMessage message,ContextArgumentDenyProxy proxy)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        bool uiProfile=ProjectConstructorProbe.ProductUiFixtureMode(),uiProjectRequest=false;
        bool compactProfile=uiProfile||ProjectConstructorProbe.ParameterNegativeProfile();
        int limit=ProjectConstructorProbe.ContextAdapterLimit();int count;bool faultAtEntry;lock(sync){faultAtEntry=fault;if(entries<limit+1)entries++;count=entries;}
        string attemptStage="ELIGIBILITY";
        bool suspended=false,ownsEntry=false,ownsScalar=false,prepared=false,denied=false,ownsProductCacheGate=false,uiCacheOperation=false,parameterCacheOperation=false;int previous=0,operation=-1,childCase=-1,referenceCase=-1,uiValidatorCase=-1;MethodInfo method=null;object result=null;int puid=0;UiValidatorEntry uiValidatorEntry=null;
        try{
            bool uiCachePhase=ProjectConstructorProbe.ProductUiCacheEligible();
            bool parameterReadPhase=ProjectConstructorProbe.ParameterDefinitionReadEligible();
            bool parameterCachePhase=ProjectConstructorProbe.ParameterDefinitionCacheEligible();
            bool productCleanupPhase=ProjectConstructorProbe.ProductStoreDisposalEligible();
            bool phase=eligible(),notificationPhase=notificationEligible!=null&&notificationEligible(),undoPhase=undoEligible!=null&&undoEligible(),writePhase=writeEligible!=null&&writeEligible(),allDevicesPhase=allDevicesEligible!=null&&allDevicesEligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}
            attemptStage="BUDGET";
            if(count>limit&&ProjectConstructorProbe.ProductFixtureMode())BudgetDiagnostic(count,limit);
            ProbeFiles.Require(count<=limit,"VALIDATOR_CONTEXT_BUDGET");
            if(uiProfile){lock(sync){ProbeFiles.Require(!fault,"VALIDATOR_UI_PREVIOUS_FAULT_NO_MESSAGE_READ");}}
            attemptStage="METHOD_CAPTURE";method=ProjectContentDenyProxy.CaptureContentMethod(message);
            uiProjectRequest=uiProfile&&ProjectConstructorProbe.ExactRuntimeMethod(method,scalarMethods[3]);
            if(uiProfile){int bucketCount,bucketLimit=uiProjectRequest?ProjectConstructorProbe.ProductUiProjectReferenceLimit():ProjectConstructorProbe.ProductUiOtherContextLimit();lock(sync){if(uiProjectRequest){if(uiProjectReferences<bucketLimit+1)uiProjectReferences++;bucketCount=uiProjectReferences;}else{if(uiOtherCalls<bucketLimit+1)uiOtherCalls++;bucketCount=uiOtherCalls;}}
                if(bucketCount>bucketLimit)BudgetDiagnostic(count,limit,uiProjectRequest?"PROJECT_REFERENCE":"OTHER_CONTEXT",bucketCount,bucketLimit);
                ProbeFiles.Require(bucketCount<=bucketLimit,uiProjectRequest?"VALIDATOR_UI_PROJECT_REFERENCE_BUDGET":"VALIDATOR_UI_OTHER_CONTEXT_BUDGET");}
            attemptStage="METHOD_SELECTION";bool validator=false;
            if(uiProjectRequest)operation=3;
            else{
            validator=ProjectConstructorProbe.ExactRuntimeMethod(method,closedMethod);
            if(!validator){lock(sync){for(int i=0;i<childValidators.Count;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,childValidators[i].Method)){validator=true;childCase=i;break;}}}
            if(!validator&&uiValidatorTypes!=null){for(int i=0;i<uiValidatorTypes.Length;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,uiValidatorTypes[i].Method)){validator=true;uiValidatorCase=i;break;}}
            if(!validator)for(int i=0;i<scalarMethods.Length;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,scalarMethods[i]))operation=i;
            if(!validator&&notificationGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,notificationGetter))operation=4;
            if(!validator&&undoGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,undoGetter))operation=5;
            if(!validator&&writeGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,writeGetter))operation=6;
            if(!validator&&idCreatorGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,idCreatorGetter))operation=7;
            if(!validator&&allDevicesGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,allDevicesGetter))operation=8;
            if(!validator&&canonicalReferenceMethods!=null){for(int i=0;i<canonicalReferenceMethods.Length;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,canonicalReferenceMethods[i])){operation=9;referenceCase=i;break;}}
            if(!validator&&productCacheMethods!=null){for(int i=0;i<productCacheMethods.Length;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,productCacheMethods[i])){operation=10;referenceCase=i;break;}}
            if(!validator&&productFacadeGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productFacadeGetter))operation=11;
            if(!validator&&productLargeStringGetter!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productLargeStringGetter))operation=12;
            if(!validator&&productMasterMethod!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productMasterMethod))operation=13;
            if(!validator&&productDispatchMethod!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productDispatchMethod))operation=14;
            if(!validator&&productCacheWeakMethods!=null){for(int i=0;i<2;i++)if(ProjectConstructorProbe.ExactRuntimeMethod(method,productCacheWeakMethods[i])){operation=15+i;break;}}
            if(!validator&&productCacheReleaseMethod!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,productCacheReleaseMethod))operation=17;
            }
            ProbeFiles.Require(validator||operation>=0,"VALIDATOR_CONTEXT_UNSUPPORTED");
            if(compactProfile){lock(sync){UiMethodCount tally;if(!uiMethodCounts.TryGetValue(method,out tally)){ProbeFiles.Require(uiMethodCounts.Count<64,"UI_METHOD_SUMMARY_BOUND");string label=uiValidatorCase>=0?"GetValidator:"+uiValidatorTypes[uiValidatorCase].CheckerType.Name:validator?"GetValidator:"+(childCase>=0?"Child"+childCase:"Project"):method.Name;tally=new UiMethodCount{Label=ProjectConstructorProbe.Text(label)};uiMethodCounts.Add(method,tally);}if(tally.Count<ProjectConstructorProbe.ContextAdapterLimit()+1)tally.Count++;lastUiMethod=tally.Label;}}

            attemptStage="PHASE";
            if(operation==4)phase=notificationPhase;else if(operation==5)phase=undoPhase;else if(operation==6)phase=writePhase;else if(operation==8||operation==9||operation==10)phase=allDevicesPhase;else if(operation==11)phase=uiCachePhase;else if(operation==12)phase=uiCachePhase||undoPhase;else if(operation==13||operation==14)phase=uiCachePhase;
            if(operation==3&&productCleanupPhase){lock(sync){ProbeFiles.Require(project!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project)&&project.GetType()==projectType&&project.GetType().Module==scalarMethods[3].Module,"PRODUCT_DISPOSAL_EXACT_RETAINED_PROJECT");}phase=true;}
            if(ReferenceDcaRequiredParameters.AllowsReadOperation(parameterReadPhase,operation)){
                if(operation==3){lock(sync){ProbeFiles.Require(project!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project)&&project.GetType()==projectType&&project.GetType().Module==scalarMethods[3].Module,"PARAMETER_READ_EXACT_RETAINED_PROJECT");}}
                phase=true;
            }
            if(operation==15||operation==16||operation==17)phase=productCleanupPhase;
            bool parameterPreparedService=ReferenceDcaRequiredParameters.AllowsPreparedService(parameterCachePhase,Thread.CurrentThread.ManagedThreadId==owner,uiValidatorCase>=0,operation);
            if(uiValidatorCase>=0)phase=uiCachePhase||parameterPreparedService;
            if(operation==14&&parameterPreparedService){phase=true;parameterCacheOperation=true;}
            if(operation==14&&uiCachePhase)uiCacheOperation=true;
            if((operation==10||operation==11||operation==12||operation==13||uiValidatorCase>=0)&&uiCachePhase){ProbeFiles.Require(Monitor.TryEnter(productUiCacheGate,2000),"PRODUCT_UI_CACHE_SERIALIZATION_TIMEOUT");ownsProductCacheGate=true;uiCacheOperation=true;phase=true;}
            if(ReferenceDcaRequiredParameters.AllowsBackgroundRead(parameterCachePhase,operation)||ReferenceDcaRequiredParameters.AllowsReadOperation(parameterReadPhase,operation)||(parameterPreparedService&&operation!=14)){
                ProbeFiles.Require(!ownsProductCacheGate&&Monitor.TryEnter(productUiCacheGate,2000),"PARAMETER_CACHE_SERIALIZATION_TIMEOUT");ownsProductCacheGate=true;
                ProbeFiles.Require(ProjectConstructorProbe.ParameterDefinitionCacheLifetime(),"PARAMETER_CACHE_SCOPE_EXPIRED");parameterCacheOperation=true;phase=true;
            }
            lock(sync){
                bool nestedScalar=entered&&constructorActive&&((operation>=0&&operation<4)||(ProjectConstructorProbe.DeviceFixtureMode()&&(operation==8||operation==9||operation==10)))&&!scalarActive&&excluded;
                if(fault||lifetime!=1||(Thread.CurrentThread.ManagedThreadId!=owner&&!uiCacheOperation&&!parameterCacheOperation)||(!nestedScalar&&(!phase||excluded||entered))){
                    bool before=fault;fault=true;
                    if(ProjectConstructorProbe.ProductUiFixtureMode()||parameterReadPhase||parameterCachePhase)record("CONTEXT_ADAPTER_PHASE_DENIAL operation="+operation+" owner-thread="+owner+" current-thread="+Thread.CurrentThread.ManagedThreadId+" lifetime="+lifetime+" fault-before="+before+" phase="+phase+" excluded="+excluded+" entered="+entered+" constructor-active="+constructorActive+" scalar-active="+scalarActive+" nested-selected="+nestedScalar+" active-callback-thread="+activeCallbackThread+" active-callback-operation="+activeCallbackOperation+" same-callback-thread="+(activeCallbackThread==Thread.CurrentThread.ManagedThreadId)+" values=NOT_INSPECTED");
                    throw new NotSupportedException("VALIDATOR_CONTEXT_PHASE");
                }
                if(!nestedScalar){entered=true;ownsEntry=true;activeCallbackThread=Thread.CurrentThread.ManagedThreadId;activeCallbackOperation=operation;}if(operation>=0){scalarActive=true;ownsScalar=true;}
            }
            attemptStage="CALL_SHAPE";var call=(IMethodCallMessage)message;int arity=validator?2:(operation==10||operation==13||operation==14||operation==16)?1:0;
            ProbeFiles.Require(call.MethodName==method.Name&&call.ArgCount==arity&&call.InArgCount==arity,"VALIDATOR_CALL_SHAPE");
            object[] arguments=call.Args;ProbeFiles.Require(arguments!=null&&arguments.Length==arity,"VALIDATOR_ARGUMENT_COUNT");
            object[] captured=new object[arity];for(int i=0;i<arity;i++)captured[i]=arguments[i];
            LogicalCallContext context=call.LogicalCallContext;string uri=call.Uri;ProbeFiles.Require(uri==null||uri.Length<=1024,"VALIDATOR_URI_BOUND");
            var snapshot=new ValidatorCallSnapshot(method,context,uri,captured,validator?validatorParameters:operation==10?new Type[]{productCacheMethods[referenceCase].GetParameters()[0].ParameterType}:operation==13?new Type[]{productMasterMethod.GetParameters()[0].ParameterType}:operation==14?new Type[]{typeof(Action)}:operation==16?new Type[]{typeof(bool)}:Type.EmptyTypes);
            attemptStage="OPERATION";
            if(validator&&uiValidatorCase>=0){
                UiValidatorType selected=uiValidatorTypes[uiValidatorCase];object child=captured[0],parent=captured[1];
                ProbeFiles.Require(child!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(child)&&child.GetType()==selected.ObjectType&&parent!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(parent)&&parent.GetType().Module==selected.ObjectType.Module,"PRODUCT_UI_VALIDATOR_ACTUAL_TYPES");
                ProbeFiles.Require(Object.ReferenceEquals(selected.Parent.Invoke(child,new object[0]),parent)&&Object.ReferenceEquals(selected.Context.Invoke(child,new object[0]),proxy.GetTransparentProxy())&&Object.ReferenceEquals(selected.Project.Invoke(child,new object[0]),project),"PRODUCT_UI_VALIDATOR_CONTEXT_PROJECT_PARENT");
                lock(sync){uiValidatorEntries.TryGetValue(child,out uiValidatorEntry);
                    if(uiValidatorEntry==null){ProbeFiles.Require(uiValidatorEntries.Count<20000,"PRODUCT_UI_VALIDATOR_OBJECT_LIMIT");uiValidatorEntry=new UiValidatorEntry{Type=selected,Child=child,Collection=parent};uiValidatorEntries.Add(child,uiValidatorEntry);}else ProbeFiles.Require(uiValidatorEntry.Type==selected&&Object.ReferenceEquals(uiValidatorEntry.Collection,parent),"PRODUCT_UI_VALIDATOR_PARENT_STABLE");result=uiValidatorEntry.Instance;}
                if(result==null){lock(sync){ProbeFiles.Require(uiValidatorEntry.Retained==null&&!fault,"PRODUCT_UI_VALIDATOR_ONE_CTOR_PER_OBJECT");constructorActive=true;}
                    try{result=selected.Constructor.Invoke(new object[]{child});}finally{lock(sync){constructorActive=false;}}
                    ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==selected.CheckerType,"PRODUCT_UI_VALIDATOR_ORIGINAL_CTOR_RETURN");lock(sync){uiValidatorEntry.Retained=result;}
                }
            }else if(validator&&childCase>=0){
                SdkChildValidatorCase selected;lock(sync){selected=childValidators[childCase];result=selected.Instance;}
                ProbeFiles.Require(Object.ReferenceEquals(captured[0],selected.Child)&&(captured[1]==null||Object.ReferenceEquals(captured[1],selected.Collection)),"SDK_CHILD_VALIDATOR_EXACT_ARGUMENTS");
                if(result==null){lock(sync){ProbeFiles.Require(!fault&&selected.Constructions==0,"SDK_CHILD_VALIDATOR_ONE_CONSTRUCTION");selected.Constructions++;constructorActive=true;}record("SDK_CHILD_VALIDATOR_CTOR_BEGIN token="+selected.Constructor.MetadataToken.ToString("X8")+" arguments=GENUINE_COLLECTION_CHILD");try{result=selected.Constructor.Invoke(new object[]{selected.Collection,selected.Child});}finally{lock(sync){constructorActive=false;}}ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==selected.Constructor.DeclaringType,"SDK_CHILD_VALIDATOR_EXACT_RESULT");record("SDK_CHILD_VALIDATOR_CTOR_END returned=True exact-genuine-type=True");}
            }else if(validator){
                object actualProject=captured[0],actualCollection=captured[1],retainedCollection=collection();
                bool goodProject=actualProject!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actualProject)&&actualProject.GetType()==projectType;
                bool goodCollection=retainedCollection!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(retainedCollection)&&retainedCollection.GetType()==collectionType&&(actualCollection==null||Object.ReferenceEquals(actualCollection,retainedCollection));
                if(!goodProject||!goodCollection){record("CONTEXT_VALIDATOR_ARGUMENT_MISMATCH arg0-type="+ArgumentType(actualProject)+" arg1-type="+ArgumentType(actualCollection)+" retained-type="+ArgumentType(retainedCollection)+" values=NOT_INSPECTED");throw new NotSupportedException("VALIDATOR_ACTUAL_ARGUMENT_TYPES");}
                lock(sync){if(project!=null&&!Object.ReferenceEquals(project,actualProject)){fault=true;throw new NotSupportedException("VALIDATOR_SAME_PROJECT_ONLY");}project=actualProject;result=checker;}
                if(result==null){
                    record("CONTEXT_VALIDATOR_CTOR_BEGIN token="+constructor.MetadataToken.ToString("X8")+" exact-instance=True arguments=GENUINE_COLLECTION_PROJECT values=NOT_INSPECTED");
                    lock(sync){if(fault||constructions!=0){fault=true;throw new NotSupportedException("VALIDATOR_ONE_CONSTRUCTION");}constructions++;constructorActive=true;}
                    try{result=constructor.Invoke(new object[]{retainedCollection,actualProject});}
                    finally{lock(sync){constructorActive=false;}}
                    ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==checkerType,"VALIDATOR_EXACT_CONSTRUCTOR_RESULT");
                    lock(sync){retainedChecker=result;}
                    record("CONTEXT_VALIDATOR_CTOR_END returned=True exact-type=True disposal-owner=CALLER_NOT_INVENTED");
                }
            }else if(operation==4){
                lock(sync){result=notification;}
                if(result==null){
                    record("CONTEXT_NOTIFICATION_CTOR_BEGIN token="+notificationConstructor.MetadataToken.ToString("X8")+" phase=EXPLICIT_PROJECT_DISPOSAL");
                    lock(sync){ProbeFiles.Require(!fault&&notificationConstructions==0,"NOTIFICATION_ONE_CONSTRUCTION");notificationConstructions++;}
                    result=notificationConstructor.Invoke(new object[0]);
                    ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==notificationType,"NOTIFICATION_EXACT_CONSTRUCTOR_RESULT");
                    lock(sync){retainedNotification=result;}
                    record("CONTEXT_NOTIFICATION_CTOR_END returned=True exact-type=True disposal-owner=CALLER_NOT_INVENTED");
                }
            }else if(operation==5){
                lock(sync){result=undoManager;}
                if(result==null){
                    record("CONTEXT_UNDO_FACTORY_BEGIN route=VERIFIED_PUBLIC_IFACTORY phase=EXPLICIT_PROJECT_DISPOSAL");
                    lock(sync){ProbeFiles.Require(!fault&&undoFactoryCalls==0,"UNDO_CONTEXT_ONE_FACTORY_CALL");undoFactoryCalls++;}
                    result=undoFactory();
                    ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==undoType,"UNDO_CONTEXT_EXACT_FACTORY_RESULT");
                    lock(sync){retainedUndoManager=result;}
                    record("CONTEXT_UNDO_FACTORY_END returned=True exact-type=True disposal-owner=CALLER_NOT_INVENTED");
                }
            }else if(operation==6){result=writeReference;
            }else if(operation==8){result=allDevicesReference();ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==allDevicesGetter.ReturnType,"SDK_ALL_DEVICES_GENUINE_REFERENCE");
            }else if(operation==9){result=canonicalReferenceValues[referenceCase]();ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==canonicalReferenceMethods[referenceCase].ReturnType,"SDK_CONTEXT_GENUINE_CANONICAL_REFERENCE");
            }else if(operation==17){productCacheRelease();result=null;
            }else if(operation==15||operation==16){
                object argument=operation==16?captured[0]:null;
                ProbeFiles.Require(operation==15||(argument!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(argument)&&argument.GetType()==typeof(bool)),"PRODUCT_CACHE_WEAK_EXACT_BOOL");
                result=productCacheWeak(operation==16,argument);ProbeFiles.Require(operation==16?result==null:result!=null&&result.GetType()==typeof(bool),"PRODUCT_CACHE_WEAK_ORIGINAL_RESULT");
            }else if(operation==14){
                object action=captured[0];ProbeFiles.Require(action!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(action)&&action.GetType()==typeof(Action),"PRODUCT_CONTEXT_UI_EXACT_ACTION");
                lock(sync){if(ownsEntry){entered=false;ownsEntry=false;activeCallbackThread=0;activeCallbackOperation=-1;}if(ownsScalar){scalarActive=false;ownsScalar=false;}}
                if(suspended){ProjectConstructorProbe.RestoreProjectResourceCallback(previous);suspended=false;}
                productDispatch((Action)action);result=null;
            }else if(operation==13){ProbeFiles.Require(Object.ReferenceEquals(captured[0],productMasterFactory),"PRODUCT_CONTEXT_MASTER_SAME_GENUINE_FACTORY");result=productMasterReference;ProbeFiles.Require(result!=null&&result.GetType()==productMasterMethod.ReturnType,"PRODUCT_CONTEXT_MASTER_ORIGINAL_REFERENCE");
            }else if(operation==12){result=productLargeStrings;ProbeFiles.Require(result!=null&&result.GetType()==productLargeStringGetter.ReturnType,"PRODUCT_LARGE_STRING_CONTEXT_EXACT_REFERENCE");
            }else if(operation==11){result=productFacadeCache;ProbeFiles.Require(result!=null&&result.GetType()==productFacadeGetter.ReturnType&&((System.Collections.IDictionary)result).Count<=16,"PRODUCT_FACADE_CACHE_REFERENCE_BOUND");
            }else if(operation==10){result=productCache(referenceCase,captured[0]);ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&productCacheMethods[referenceCase].ReturnType.IsInstanceOfType(result)&&result.GetType().Module==productCacheMethods[referenceCase].ReturnType.Module,"PRODUCT_IMPORT_CONTEXT_CACHE_REFERENCE");
            }else if(operation==7){
                lock(sync){result=idCreator;}
                if(result==null){lock(sync){ProbeFiles.Require(!fault&&idCreatorConstructions==0,"SDK_ID_CREATOR_ONE_CONSTRUCTION");idCreatorConstructions++;}record("SDK_ID_CREATOR_CTOR_BEGIN token=06000593 context=SAME_CONTROLLED_CONTEXT");result=idCreatorConstructor.Invoke(new object[]{idContext()});ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==idCreatorType,"SDK_ID_CREATOR_GENUINE_RESULT");lock(sync){retainedIdCreator=result;}record("SDK_ID_CREATOR_CTOR_END returned=True genuine=True IDs=ORIGINAL_CREATOR");}
            }else{
                lock(sync){
                    if(operation==0)result=style;
                    else if(operation==1){puid=nextPuid;ProbeFiles.Require(puid>=1&&puid<=16,"CONTEXT_PUID_FIXTURE_BOUND");result=puid;}
                    else if(operation==2)result=null; // Exact SetProjectModified tracks dirty at accepted completion.
                    else {ProbeFiles.Require(project!=null,"CONTEXT_PROJECT_NOT_CAPTURED");result=project;}
                }
            }
            attemptStage="REPLY";lock(sync){ProbeFiles.Require(!fault,"VALIDATOR_CAUGHT_FAULT");}
            bool logReply;lock(sync){logReply=!compactProfile||uiMethodCounts[method].Count==1;}
            if(logReply)record("CONTEXT_ADAPTER_REPLY method="+ProjectConstructorProbe.Text(method.Name)+" kind="+(validator?"GENUINE_CACHED_CHECKER":operation==3?"GENUINE_CAPTURED_PROJECT":operation==4?"GENUINE_CACHED_NOTIFICATION_SERVICE":operation==5?"GENUINE_CACHED_UNDO_MANAGER":operation==6?"OFFLINE_CONTROLLED_WRITE_FIXTURE_NOT_PERSISTENT_CONFIGURATION":operation==7?"GENUINE_CACHED_ID_CREATOR":operation==8?"GENUINE_CANONICAL_DEVICE_COLLECTION":operation==9?"GENUINE_CANONICAL_CONTEXT_REFERENCE":operation==10?"GENUINE_IMPORTED_PRODUCT_CACHE":operation==11?"OWNED_TRANSIENT_WEAK_FACADE_CACHE":operation==12?"GENUINE_ORIGINAL_PROJECT_LARGE_STRING_CACHE":operation==13?"GENUINE_LOADED_MASTER_REFERENCE":operation==14?"REAL_WPF_DISPATCHED_ACTION":operation==15||operation==16?"GENUINE_IMPORTED_CACHE_WEAK_POLICY":operation==17?"GENUINE_IMPORTED_CACHE_RELEASE":"EXPLICIT_CONTEXT_FIXTURE")+" values=NOT_LOGGED");
            IMessage response=new ReturnMessage(result,new object[0],0,context,snapshot);
            ProbeFiles.Require(!uiCacheOperation||ProjectConstructorProbe.ProductUiCacheLifetime(),"PRODUCT_UI_CACHE_COMPLETION_LIFETIME");
            ProbeFiles.Require(!parameterCacheOperation||ProjectConstructorProbe.ParameterDefinitionCacheLifetime(),"PARAMETER_CACHE_COMPLETION_LIFETIME");
            lock(sync){ProbeFiles.Require(!fault&&!proxy.State.Faulted,"VALIDATOR_REPLY_FAULT");}prepared=true;return response;
        }catch(Exception error){
            Fail();int refusal=proxy.State.Deny();denied=true;
            FirstFailure(error,attemptStage,count,limit,operation,faultAtEntry,method);
            if(refusal==1&&count<=limit&&ProjectConstructorProbe.UndoFactoryCallbackExcluded())proxy.DiagnoseCaptured(method,refusal);
            throw;
        }finally{
            try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}
            catch{Fail();if(!denied){proxy.State.Deny();denied=true;}throw;}
            finally{
                try{
                    bool accepted;lock(sync){
                        if(ownsEntry){entered=false;activeCallbackThread=0;activeCallbackOperation=-1;}if(ownsScalar)scalarActive=false;
                        accepted=prepared&&!fault&&!proxy.State.Faulted&&(!uiCacheOperation||ProjectConstructorProbe.ProductUiCacheLifetime())&&(!parameterCacheOperation||ProjectConstructorProbe.ParameterDefinitionCacheLifetime());
                        if(accepted){if(operation<0){if(uiValidatorEntry!=null)uiValidatorEntry.Instance=result;else if(childCase>=0)childValidators[childCase].Instance=result;else checker=result;}else if(operation==7)idCreator=result;else if(operation==1)nextPuid=puid+1;else if(operation==2)dirty=true;else if(operation==4){notification=result;notificationReplies++;}else if(operation==5){undoManager=result;undoReplies++;}replies++;if(uiProfile){if(uiProjectRequest)uiProjectReplies++;else uiOtherReplies++;}}
                    }
                    if(prepared&&!accepted){Fail();if(!denied)proxy.State.Deny();throw new NotSupportedException("VALIDATOR_COMPLETION_FAULT");}
                }finally{if(ownsProductCacheGate)Monitor.Exit(productUiCacheGate);}
            }
        }

        }
    }
    bool parameterSummaryRecorded;
    void ParameterMethodSummary(string phase)
    {
        if(!ProjectConstructorProbe.ParameterNegativeProfile()||phase!="PROCESS_FINAL")return;
        List<UiMethodCount> snapshot;int e,r;lock(sync){if(parameterSummaryRecorded)return;parameterSummaryRecorded=true;e=entries;r=replies;snapshot=new List<UiMethodCount>();foreach(var item in uiMethodCounts.Values)snapshot.Add(new UiMethodCount{Label=item.Label,Count=item.Count});}
        snapshot.Sort(delegate(UiMethodCount left,UiMethodCount right){return right.Count.CompareTo(left.Count);});
        record("PARAMETER_READ_CALLBACK_SUMMARY phase=PROCESS_FINAL_FIRST entries="+e+" reply-commits="+r+" context-cap="+ProjectConstructorProbe.ContextAdapterLimit()+" selected-method-kinds="+snapshot.Count+" healthy-reply-logs=FIRST_PER_METHOD checks=EVERY_CALLBACK");
        for(int i=0;i<snapshot.Count&&i<6;i++)record("PARAMETER_READ_METHOD_SUMMARY rank="+(i+1)+" method="+snapshot[i].Label+" selected-attempts="+snapshot[i].Count);
    }
    internal void Audit(string phase,Action<string> output)
    {
        int e,r,c,l;bool f,d,active;lock(sync){e=entries;r=replies;c=constructions;l=lifetime;f=fault;d=dirty;active=constructorActive||entered;}
        try{ParameterMethodSummary(phase);if(f||e>ProjectConstructorProbe.ContextAdapterLimit()||r>e||c>1||notificationConstructions>1||undoFactoryCalls>1||active||ProjectConstructorProbe.InitializeAuditLog(phase))output("CONTEXT_ADAPTER_AUDIT phase="+phase+" entries="+e+" replies="+r+" ui-project-attempts="+uiProjectReferences+" ui-project-replies="+uiProjectReplies+" ui-other-attempts="+uiOtherCalls+" ui-other-replies="+uiOtherReplies+" constructions="+c+" lifetime="+l+" fault="+f+" dirty-fixture="+d+" pending="+active+" checker-retained="+(retainedChecker!=null)+" notification-constructions="+notificationConstructions+" notification-replies="+notificationReplies+" notification-retained="+(retainedNotification!=null)+" undo-factory-calls="+undoFactoryCalls+" undo-replies="+undoReplies+" undo-retained="+(retainedUndoManager!=null)+" values=NOT_LOGGED");}
        finally{if(f||e>ProjectConstructorProbe.ContextAdapterLimit()||r>e||c>1||notificationConstructions>1||undoFactoryCalls>1||idCreatorConstructions>1||uiProjectReferences>ProjectConstructorProbe.ProductUiProjectReferenceLimit()||uiOtherCalls>ProjectConstructorProbe.ProductUiOtherContextLimit()||uiProjectReplies>uiProjectReferences||uiOtherReplies>uiOtherCalls||active||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&l!=2))throw new NotSupportedException("CONTEXT_ADAPTER_FINAL_DENIAL");GC.KeepAlive(retainedChecker);GC.KeepAlive(project);GC.KeepAlive(retainedNotification);GC.KeepAlive(retainedUndoManager);GC.KeepAlive(retainedIdCreator);foreach(var value in childValidators){if(value.Constructions>1)throw new NotSupportedException("SDK_CHILD_VALIDATOR_FINAL_DENIAL");GC.KeepAlive(value);}}
    }
}

internal sealed class ContextArgumentDenyProxy : RealProxy
{
    internal readonly ContextDenialState State = new ContextDenialState(16);
    readonly string name; readonly Type contract; readonly Action<string> record; readonly ContextValidatorAdapter adapter;
    internal ContextArgumentDenyProxy(Type type, string label, Action<string> recorder) : base(type) { name = label; contract = type; record = recorder; }
    internal ContextArgumentDenyProxy(Type type,string label,Action<string> recorder,ContextValidatorAdapter positive) : this(type,label,recorder) { adapter=positive; }
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        if(adapter!=null)return adapter.Invoke(message,this);
        int count = State.Deny(); bool suspended = false; int previous = 0;
        try {
            if (!ProjectConstructorProbe.UndoFactoryCallbackExcluded()) { previous = ProjectConstructorProbe.SuspendProjectResourceCallback(); suspended = true; }
            if (count > 16) throw new NotSupportedException("CONTEXT_ARGUMENT_BUDGET:" + name);
            record("CONTEXT_ARGUMENT_DENIED argument=" + name + " entry=" + count + " values=NOT_INSPECTED");
            var error = new NotSupportedException("CONTEXT_ARGUMENT_UNSUPPORTED:" + name);
            if (ProjectConstructorProbe.GuidMode) {
                if (count == 1) {
                    MethodInfo captured; string descriptor = ProjectContentDenyProxy.ContentMessageDescriptor(message, contract, out captured);
                    record("CONTEXT_ARGUMENT_REFUSAL argument=" + name + " entry=1 " + descriptor + " values=NOT_INSPECTED caller-origin=UNKNOWN");
                    DescribeValidator(captured);
                }
                throw error; // Diagnostic profile never redispatches/rereads the original message.
            }
            IMethodCallMessage call = message as IMethodCallMessage;
            if (call == null) throw error;
            return new ReturnMessage(error, call);
        } catch { State.Fail(); throw; }
        finally { try { if (suspended) ProjectConstructorProbe.RestoreProjectResourceCallback(previous); } catch { State.Fail(); throw; } }

        }
    }
    internal void DiagnoseCaptured(MethodInfo method,int count)
    { record("CONTEXT_ARGUMENT_REFUSAL argument="+name+" denial="+count+" "+ProjectContentDenyProxy.ContentMethodDescriptor(method,contract)+" values=NOT_INSPECTED caller-origin=UNKNOWN");DescribeValidator(method); }
    void DescribeValidator(MethodInfo method)
    {
        if (method == null || System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method) || method.GetType() != typeof(object).GetMethod("GetType").GetType()
            || method.Module != contract.Module || method.DeclaringType != contract || method.MetadataToken != 0x06000255 || !method.IsGenericMethod) return;
        string phase = "GENERIC_ARGUMENT"; int chars = 0;
        Action<string> emit = delegate(string line) { ProbeFiles.Require(line.Length <= 2048 && chars <= 8192 - line.Length, "VALIDATOR_METADATA_BOUND"); chars += line.Length; record(line); };
        try {
            emit("CONTEXT_VALIDATOR_METADATA_BEGIN method=06000255 generic-definition=" + method.IsGenericMethodDefinition + " caller-origin=UNKNOWN values=NOT_INSPECTED");
            Type[] arguments = method.GetGenericArguments(); ProbeFiles.Require(arguments.Length == 1, "VALIDATOR_ONE_GENERIC_ARGUMENT"); Type actual = arguments[0];
            ProbeFiles.Require(actual != null && actual.GetType() == typeof(object).GetType() && !actual.Assembly.IsDynamic && !actual.Assembly.ReflectionOnly, "VALIDATOR_RUNTIME_TYPE");
            if (actual.IsGenericParameter) { emit("CONTEXT_VALIDATOR_TYPE kind=UNBOUND_GENERIC_PARAMETER name=" + ProjectConstructorProbe.Text(actual.Name)); return; }
            emit("CONTEXT_VALIDATOR_TYPE full-name=" + ProjectConstructorProbe.Text(actual.FullName) + " assembly=" + ProjectConstructorProbe.Text(actual.Assembly.FullName)
                + " module=" + ProjectConstructorProbe.Text(actual.Module.ScopeName) + " token=" + actual.MetadataToken.ToString("X8") + " flags=" + (int)actual.Attributes + " contains-generic-parameters=" + actual.ContainsGenericParameters);
            ProbeFiles.Require(!actual.HasElementType && !actual.ContainsGenericParameters && (actual.MetadataToken & unchecked((int)0xff000000)) == 0x02000000, "VALIDATOR_CONCRETE_TYPEDEF");
            phase = "CONSTRUCTORS"; ConstructorInfo[] constructors = actual.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            ProbeFiles.Require(constructors.Length <= 8, "VALIDATOR_CONSTRUCTOR_BOUND");
            foreach (ConstructorInfo ctor in constructors) {
                emit("CONTEXT_VALIDATOR_CTOR token=" + ctor.MetadataToken.ToString("X8") + " flags=" + (int)ctor.Attributes + " public=" + ctor.IsPublic + " static=" + ctor.IsStatic);
                phase = "CONSTRUCTOR_PARAMETERS"; ParameterInfo[] parameters = ctor.GetParameters(); ProbeFiles.Require(parameters.Length <= 6, "VALIDATOR_PARAMETER_BOUND");
                for (int i = 0; i < parameters.Length; i++) {
                    Type type = parameters[i].ParameterType; ProbeFiles.Require(type.GetType() == typeof(object).GetType(), "VALIDATOR_PARAMETER_RUNTIME_TYPE");
                    emit("CONTEXT_VALIDATOR_CTOR_PARAMETER ctor=" + ctor.MetadataToken.ToString("X8") + " index=" + i + " type=" + ProjectConstructorProbe.Text(type.FullName) + " assembly=" + ProjectConstructorProbe.Text(type.Assembly.FullName));
                }
            }
            phase = "GENERIC_CONSTRAINTS"; Type parameter = method.GetGenericMethodDefinition().GetGenericArguments()[0]; Type[] constraints = parameter.GetGenericParameterConstraints();
            ProbeFiles.Require(constraints.Length <= 6, "VALIDATOR_CONSTRAINT_BOUND"); emit("CONTEXT_VALIDATOR_CONSTRAINT_FLAGS value=" + (int)parameter.GenericParameterAttributes);
            foreach (Type constraint in constraints) emit("CONTEXT_VALIDATOR_CONSTRAINT type=" + ProjectConstructorProbe.Text(constraint.FullName) + " assembly=" + ProjectConstructorProbe.Text(constraint.Assembly.FullName));
            emit("CONTEXT_VALIDATOR_METADATA_END constructors=" + constructors.Length + " constraints=" + constraints.Length + " activation=NOT_RUN");
        } catch (Exception error) { State.Fail(); record("CONTEXT_VALIDATOR_METADATA_PARTIAL phase=" + phase + " error-type=" + ProjectConstructorProbe.Text(error.GetType().FullName) + " activation=NOT_RUN"); }
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

internal sealed class SdkDevicePluginDenyHolder
{
    internal readonly ContextDenialState State=new ContextDenialState(1);
    internal readonly Delegate Callback;readonly Action<string> record;
    internal SdkDevicePluginDenyHolder(Type callback,Type result,Action<string> recorder)
    {record=recorder;MethodInfo method=typeof(SdkDevicePluginDenyHolder).GetMethod("Deny",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).MakeGenericMethod(result);Callback=Delegate.CreateDelegate(callback,this,method,true);ProbeFiles.Require(Callback.GetType()==callback&&Object.ReferenceEquals(Callback.Target,this)&&Callback.Method.Equals(method)&&Callback.GetInvocationList().Length==1,"SDK_DEVICE_PLUGIN_EXACT_DELEGATE");}
    public T Deny<T>(object device)
    {
        int count=State.Deny();int previous=0;bool suspended=false;
        try{if(!ProjectConstructorProbe.UndoFactoryCallbackExcluded()){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}if(count==1)record("SDK_DEVICE_PLUGIN_DENIED values=NOT_INSPECTED operation=UNSUPPORTED");throw new NotSupportedException("SDK_DEVICE_PLUGIN_UNSUPPORTED");}
        catch{State.Fail();throw;}finally{try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{State.Fail();throw;}}
    }
}

// Explicit transient lock/dirty fixture; never an accepted configuration backend.
internal sealed class SdkWriteContextProxy : RealProxy
{
    readonly object sync=new object(),project;readonly Type contract;readonly MethodInfo wait,setDirty,iamDirty,isLockedForWriting,writeScope;
    MethodInfo setClean,dirtyParent;int cleanCommits;
    internal void ConfigureProductDirtyQueries(MethodInfo clean,MethodInfo parent)
    {lock(sync){ProbeFiles.Require(ProjectConstructorProbe.ProductFixtureMode()&&active&&!fault&&setClean==null&&dirtyParent==null,"SDK_WRITE_DIRTY_QUERY_CONFIGURE_ONCE");setClean=clean;dirtyParent=parent;}}
    bool SameProjectNode(object value)
    {
        var seen=new List<object>();object current=value;
        for(int depth=0;depth<32;depth++){
            if(Object.ReferenceEquals(current,project))return true;
            if(current==null||System.Runtime.Remoting.RemotingServices.IsTransparentProxy(current)||current.GetType().Module!=project.GetType().Module||!dirtyParent.DeclaringType.IsInstanceOfType(current))return false;
            foreach(object prior in seen)if(Object.ReferenceEquals(prior,current))return false;seen.Add(current);current=dirtyParent.Invoke(current,new object[0]);
        }
        return false;
    }
    readonly ManualResetEvent unlocked=new ManualResetEvent(true);readonly Action<string> record;readonly Func<bool> eligible;
    readonly object writeGate=new object();readonly int scopeOwner=Thread.CurrentThread.ManagedThreadId;WriteLease currentLease;bool scopeHeld;int scopeAcquires,scopeReleases,scopeAborts;
    int entries,replies,dirtyCommits;bool active=true,fault,dirty;readonly Dictionary<MethodInfo,bool> uiLoggedWriteReplies=new Dictionary<MethodInfo,bool>();readonly List<object> objects=new List<object>();readonly List<bool> dirtyObjects=new List<bool>();
    internal SdkWriteContextProxy(Type type,MethodInfo waitMethod,MethodInfo dirtyMethod,MethodInfo queryMethod,MethodInfo lockQuery,MethodInfo scopeMethod,object actualProject,Action<string> recorder,Func<bool> phase):base(type)
    {contract=type;wait=waitMethod;setDirty=dirtyMethod;iamDirty=queryMethod;isLockedForWriting=lockQuery;writeScope=scopeMethod;project=actualProject;record=recorder;eligible=phase;objects.Add(project);dirtyObjects.Add(false);}
    internal void AuthorizeChild(object child)
    {ProbeFiles.Require(ProjectConstructorProbe.DeviceFixtureMode()&&eligible()&&child!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(child)&&child.GetType().Module==project.GetType().Module&&(child.GetType().MetadataToken==0x02000324||child.GetType().MetadataToken==0x02000230),"SDK_WRITE_FIXTURE_GENUINE_CHILD");lock(sync){ProbeFiles.Require(active&&!fault&&objects.Count<3,"SDK_WRITE_FIXTURE_CHILD_BOUND");foreach(object value in objects)ProbeFiles.Require(!Object.ReferenceEquals(value,child),"SDK_WRITE_FIXTURE_CHILD_ONCE");objects.Add(child);dirtyObjects.Add(false);}}
    internal void Close(){lock(sync){if(scopeHeld){fault=true;AbortScope(currentLease);}active=false;}unlocked.Dispose();}
    internal void Audit(string phase)
    {int e,r,d;bool f,a,changed;lock(sync){e=entries;r=replies;d=dirtyCommits;f=fault;a=active;changed=dirty;}try{if(ProjectConstructorProbe.SdkAuditLog(phase)||f)record("SDK_WRITE_FIXTURE_AUDIT phase="+phase+" entries="+e+" replies="+r+" dirty-commits="+d+" dirty="+changed+" clean-commits="+cleanCommits+" active="+a+" fault="+f+" service=OFFLINE_CONTROLLED_FIXTURE_NOT_PERSISTENT_CONFIGURATION"+(writeScope==null?String.Empty:" scopes="+scopeAcquires+"/"+scopeReleases+"/"+scopeAborts+" scope-held="+scopeHeld));}finally{if(f||e>ProjectConstructorProbe.ProductUiWriteLimit()||r>e||d>r||cleanCommits>r||(writeScope!=null&&(scopeAborts!=0||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&(scopeHeld||scopeAcquires!=scopeReleases))))||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&a))throw new NotSupportedException("SDK_WRITE_FIXTURE_FINAL_DENIAL");GC.KeepAlive(project);}}
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int limit=ProjectConstructorProbe.ProductUiWriteLimit();int count;lock(sync){if(entries<limit+1)entries++;count=entries;}
        bool suspended=false,prepared=false,makeDirty=false,makeClean=false,queryDirty=false,queryLock=false,openScope=false,productCleanup=false;WriteLease lease=null;int previous=0,objectIndex=-1;MethodInfo method=null;string failureStage="ENTRY";object observedArgument=null;
        try{
            bool allowed=eligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();productCleanup=ProjectConstructorProbe.ProductStoreDisposalEligible();if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}
            ProbeFiles.Require(count<=limit,"SDK_WRITE_FIXTURE_BUDGET");lock(sync){ProbeFiles.Require(active&&!fault&&allowed&&!excluded,"SDK_WRITE_FIXTURE_PHASE");}
            method=ProjectContentDenyProxy.CaptureContentMethod(message);makeDirty=ProjectConstructorProbe.ExactRuntimeMethod(method,setDirty);queryDirty=ProjectConstructorProbe.ExactRuntimeMethod(method,iamDirty);queryLock=ProjectConstructorProbe.ExactRuntimeMethod(method,isLockedForWriting);openScope=writeScope!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,writeScope);
            makeClean=setClean!=null&&ProjectConstructorProbe.ExactRuntimeMethod(method,setClean);failureStage="METHOD";
            ProbeFiles.Require(makeDirty||makeClean||queryDirty||queryLock||openScope||ProjectConstructorProbe.ExactRuntimeMethod(method,wait),"SDK_WRITE_FIXTURE_UNSUPPORTED");ProbeFiles.Require(!makeClean||productCleanup,"SDK_WRITE_CLEAN_DISPOSAL_ONLY");
            var call=(IMethodCallMessage)message;int arity=makeDirty||makeClean||queryDirty?1:0;ProbeFiles.Require(call.ArgCount==arity&&call.InArgCount==arity,"SDK_WRITE_FIXTURE_ARITY");object[] args=call.Args;ProbeFiles.Require(args!=null&&args.Length==arity,"SDK_WRITE_FIXTURE_ARGUMENTS");
            object argument=arity==1?args[0]:null;observedArgument=argument;failureStage="OBJECT_MEMBERSHIP";
            if(arity==1){lock(sync){for(int i=0;i<objects.Count;i++)if(Object.ReferenceEquals(argument,objects[i])){objectIndex=i;break;}}
                ProbeFiles.Require(objectIndex>=0||((queryDirty||makeClean)&&productCleanup&&dirtyParent!=null&&SameProjectNode(argument)),"SDK_WRITE_FIXTURE_EXACT_TRACKED_OR_PROJECT_QUERY_OBJECT");}
            LogicalCallContext context=call.LogicalCallContext;string uri=call.Uri;ProbeFiles.Require(uri==null||uri.Length<=1024,"SDK_WRITE_FIXTURE_URI_BOUND");
            var snapshot=new ValidatorCallSnapshot(method,context,uri,arity==1?new object[]{argument}:new object[0],arity==1?new Type[]{method.GetParameters()[0].ParameterType}:Type.EmptyTypes);
            failureStage="STATE";object result;if(openScope){lease=AcquireScope();result=lease;}else if(makeDirty||makeClean)result=null;else if(queryDirty){lock(sync){result=objectIndex>=0&&dirtyObjects[objectIndex];}}else if(queryLock)result=!unlocked.WaitOne(TimeSpan.Zero);else result=unlocked.WaitOne(TimeSpan.Zero);
            bool logReply;lock(sync){bool ui=ProjectConstructorProbe.ProductUiFixtureMode()||ProjectConstructorProbe.ParameterNegativeProfile();logReply=!ui||!uiLoggedWriteReplies.ContainsKey(method);if(ui&&logReply)uiLoggedWriteReplies.Add(method,true);}
            if(logReply)record("SDK_WRITE_FIXTURE_REPLY method="+method.Name+" entry="+count+" synchronization=OWNED_EVENT_NONBLOCKING service=OFFLINE_CONTROLLED_FIXTURE_NOT_PERSISTENT_CONFIGURATION");
            IMessage response=new ReturnMessage(result,new object[0],0,context,snapshot);lock(sync){ProbeFiles.Require(active&&!fault,"SDK_WRITE_FIXTURE_CAUGHT_FAULT");}prepared=true;return response;
        }catch{lock(sync){fault=true;}if(count<=limit){
                if(ProjectConstructorProbe.ProductFixtureMode()){string argumentType=observedArgument==null?"NULL":System.Runtime.Remoting.RemotingServices.IsTransparentProxy(observedArgument)?"PROXY":ProjectConstructorProbe.Text(observedArgument.GetType().FullName);record("SDK_WRITE_FIXTURE_FAILURE stage="+failureStage+" count="+count+" limit="+limit+" product-cleanup="+productCleanup+" scope-held="+scopeHeld+" thread="+Thread.CurrentThread.ManagedThreadId+" argument-type="+argumentType+" values=NOT_INSPECTED");}
                record("SDK_WRITE_FIXTURE_DENIED "+ProjectContentDenyProxy.ContentMethodDescriptor(method,contract)+" values=NOT_INSPECTED");}throw;}
        finally{try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{lock(sync){fault=true;}throw;}finally{lock(sync){if(lease!=null&&(!prepared||fault))AbortScope(lease);if(prepared){ProbeFiles.Require(active&&!fault,"SDK_WRITE_FIXTURE_COMPLETION_FAULT");replies++;if(makeDirty){dirty=true;dirtyObjects[objectIndex]=true;dirtyCommits++;}else if(makeClean){if(objectIndex>=0)dirtyObjects[objectIndex]=false;dirty=false;foreach(bool value in dirtyObjects)if(value){dirty=true;break;}cleanCommits++;}}}}}

        }
    }
    sealed class WriteLease:IDisposable
    {
        readonly SdkWriteContextProxy owner;
        internal WriteLease(SdkWriteContextProxy value){owner=value;}
        public void Dispose(){owner.ReleaseScope(this);}
    }
    WriteLease AcquireScope()
    {
        lock(sync){
            ProbeFiles.Require(writeScope!=null&&ProjectConstructorProbe.ProductFixtureMode()&&active&&!fault&&Thread.CurrentThread.ManagedThreadId==scopeOwner&&!scopeHeld&&currentLease==null&&unlocked.WaitOne(TimeSpan.Zero),"SDK_WRITE_SCOPE_NONREENTRANT");
            ProbeFiles.Require(Monitor.TryEnter(writeGate,0),"SDK_WRITE_SCOPE_LOCK_UNAVAILABLE");scopeHeld=true;
            try{unlocked.Reset();currentLease=new WriteLease(this);scopeAcquires++;return currentLease;}
            catch{fault=true;scopeHeld=false;unlocked.Set();Monitor.Exit(writeGate);throw;}
        }
    }
    void AbortScope(WriteLease lease)
    {
        if(scopeHeld&&Object.ReferenceEquals(currentLease,lease)&&Thread.CurrentThread.ManagedThreadId==scopeOwner){
            currentLease=null;scopeHeld=false;scopeAborts++;try{unlocked.Set();}finally{Monitor.Exit(writeGate);}
        }
    }
    void ReleaseScope(WriteLease lease)
    {
        int limit=ProjectConstructorProbe.ProductUiWriteLimit();int count;lock(sync){if(entries<limit+1)entries++;count=entries;}
        bool suspended=false,prepared=false;int previous=0;
        try{
            bool allowed=eligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}
            ProbeFiles.Require(count<=limit,"SDK_WRITE_SCOPE_RELEASE_BUDGET");
            lock(sync){ProbeFiles.Require(active&&!fault&&allowed&&!excluded&&Thread.CurrentThread.ManagedThreadId==scopeOwner&&scopeHeld&&Object.ReferenceEquals(currentLease,lease),"SDK_WRITE_SCOPE_RELEASE_PHASE");}
            record("SDK_WRITE_SCOPE_RELEASE entry="+count+" synchronization=REAL_OWNED_LOCK_AND_EVENT persistence=NOT_IMPLEMENTED");
            lock(sync){ProbeFiles.Require(active&&!fault,"SDK_WRITE_SCOPE_RELEASE_CAUGHT_FAULT");}prepared=true;
        }catch{lock(sync){fault=true;}throw;}
        finally{
            try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{lock(sync){fault=true;}throw;}
            finally{lock(sync){
                if(scopeHeld&&Object.ReferenceEquals(currentLease,lease)&&Thread.CurrentThread.ManagedThreadId==scopeOwner){currentLease=null;scopeHeld=false;try{unlocked.Set();}finally{Monitor.Exit(writeGate);}scopeReleases++;}
                if(prepared){ProbeFiles.Require(active&&!fault,"SDK_WRITE_SCOPE_RELEASE_COMPLETION_FAULT");replies++;}
            }}
        }
    }

}

// Separate positive initialization profile; the original deny-all context is unchanged.
internal sealed class SdkInitializationProjectProxy : RealProxy
{
    readonly object sync=new object(),project;readonly Type contract;readonly MethodInfo getter,userConfiguration;readonly Action<string> record;readonly Func<bool> eligible;
    int entries,replies,denials,absentReplies;bool active=true,fault;
    internal SdkInitializationProjectProxy(Type context,MethodInfo projectGetter,MethodInfo absentGetter,object sdkProject,Action<string> recorder,Func<bool> phase):base(context)
    {contract=context;getter=projectGetter;userConfiguration=absentGetter;project=sdkProject;record=recorder;eligible=phase;ProbeFiles.Require(project!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project)&&project.GetType()==getter.ReturnType,"DCA_INITIALIZE_EXACT_SDK_PROJECT");}
    internal void Close(){lock(sync){active=false;}}
    internal void Audit(string phase)
    {int e,r,d,u;bool f,a;lock(sync){e=entries;r=replies;d=denials;u=absentReplies;f=fault;a=active;}try{if(ProjectConstructorProbe.SdkAuditLog(phase)||f)record("DCA_INITIALIZE_CONTEXT_AUDIT phase="+phase+" entries="+e+" project-reply-commits="+(r-u)+" absent-user-configuration-commits="+u+" denials="+d+" fault="+f+" active="+a+" caller-receipt=UNKNOWN");}finally{if(f||d!=0||e>16||r>e||((phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")&&a))throw new NotSupportedException("DCA_INITIALIZE_CONTEXT_FINAL_DENIAL");GC.KeepAlive(project);}}
    public override IMessage Invoke(IMessage message)
    {
        using(IDisposable parameterInvocation=ProjectConstructorProbe.AcquireParameterReadInvocation()){
        int count;lock(sync){if(entries<17)entries++;count=entries;}
        bool suspended=false;int previous=0;MethodInfo method=null;
        try{
            bool allowed=eligible(),excluded=ProjectConstructorProbe.UndoFactoryCallbackExcluded();
            if(!excluded){previous=ProjectConstructorProbe.SuspendProjectResourceCallback();suspended=true;}
            ProbeFiles.Require(count<=16,"DCA_INITIALIZE_CONTEXT_LIMIT");
            lock(sync){ProbeFiles.Require(active&&!fault&&allowed&&!excluded,"DCA_INITIALIZE_CONTEXT_PHASE");}
            method=ProjectContentDenyProxy.CaptureContentMethod(message);
            bool absent=ProjectConstructorProbe.ExactRuntimeMethod(method,userConfiguration);
            ProbeFiles.Require(absent||ProjectConstructorProbe.ExactRuntimeMethod(method,getter),"DCA_INITIALIZE_CONTEXT_UNSUPPORTED");
            var call=(IMethodCallMessage)message;
            ProbeFiles.Require(call.ArgCount==0&&call.InArgCount==0,"DCA_INITIALIZE_CONTEXT_ZERO_ARGUMENTS");object[] args=call.Args;ProbeFiles.Require(args!=null&&args.Length==0,"DCA_INITIALIZE_CONTEXT_ARGUMENT_VECTOR");
            LogicalCallContext context=call.LogicalCallContext;string uri=call.Uri;ProbeFiles.Require(uri==null||uri.Length<=1024,"DCA_INITIALIZE_CONTEXT_URI_BOUND");
            var snapshot=new CacheCallSnapshot(absent?userConfiguration:getter,context,uri);object value=absent?null:project;IMessage response=new ReturnMessage(value,new object[0],0,context,snapshot);
            if(absent)record("DCA_INITIALIZE_USER_CONFIGURATION_REPLY entry="+count+" state=CONTROLLED_ABSENT_INITIAL_STATE value=NULL default-document=VENDOR_OWN_FALLBACK persistence=NOT_IMPLEMENTED caller-receipt=UNKNOWN");else record("DCA_INITIALIZE_PROJECT_REPLY entry="+count+" reference=GENUINE_SDK_PROJECT caller-receipt=UNKNOWN");
            lock(sync){ProbeFiles.Require(active&&!fault,"DCA_INITIALIZE_CONTEXT_CAUGHT_FAULT");replies++;if(absent)absentReplies++;}return response;
        }catch{
            int denied;lock(sync){fault=true;if(denials<17)denials++;denied=denials;}
            if(count<=16&&denied==1)record("DCA_INITIALIZE_CONTEXT_REFUSAL entry="+count+" "+ProjectContentDenyProxy.ContentMethodDescriptor(method,contract)+" values=NOT_INSPECTED caller-origin=UNKNOWN");throw;
        }finally{try{if(suspended)ProjectConstructorProbe.RestoreProjectResourceCallback(previous);}catch{lock(sync){fault=true;}throw;}}

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
    const string SdkNameResourceEnGb="Knx.Ets.ObjectModel.resources, Version=6.4.8718.0, Culture=en-GB, PublicKeyToken=61439873ec5e1159";
    const string SdkNameResourceEn="Knx.Ets.ObjectModel.resources, Version=6.4.8718.0, Culture=en, PublicKeyToken=61439873ec5e1159";
    [ThreadStatic] static bool sdkNameSetterActive;
    [ThreadStatic] static bool sdkProductImportInvokeActive, sdkProductImportEngineActive, sdkProductConversionActive;
    [ThreadStatic] static Assembly sdkProductImportResourceOwner;
    [ThreadStatic] static bool sdkDeviceFactoryInvokeActive;
    static Assembly sdkNameResourceOwner;
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
    static int logBytes, assemblyCount,sdkUiCompactedAssemblyLogs;
    static bool sdkUiMainCoreFinalRecorded,sdkUiSuppressRepeatedHealthyFinal;
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
    static bool SdkOriginal { get { return mode == "original-sdk-project-fixture" || (mode == "original-sdk-initialize-project" || (mode == "original-sdk-device-fixture" || mode == OriginalProductImportProbe.Mode)); } }
    static bool SdkInitializing { get { return (mode == "original-sdk-initialize-project" || (mode == "original-sdk-device-fixture" || mode == OriginalProductImportProbe.Mode)); } }
    static bool SdkProductMode {get{return mode==OriginalProductImportProbe.Mode;}}
    static OriginalProductImportProbe sdkProductImport;
    static bool SdkDeviceMode {get{return (mode=="original-sdk-device-fixture"||mode==OriginalProductImportProbe.Mode);}}
    internal static bool DeviceFixtureMode(){return SdkDeviceMode;}
    internal static bool ProductFixtureMode(){return SdkProductMode;}
    internal static int ContextAdapterLimit(){return ReferenceDcaRequiredParameters.IdentityContextBudget(FixtureObservationProfile(),ReferenceDcaRequiredParameters.ContextBudget(ParameterNegativeProfile(),SdkUiRequested?10200000:SdkProductMode?64:SdkDeviceMode?32:16));}
    static bool SdkOwned { get { return mode == "owned-sdk-project-fixture"; } }
    internal static bool SdkMode { get { return SdkOriginal || SdkOwned; } }
    static bool GuidOriginal { get { return mode == "original-project-guid-fixture" || SdkOriginal; } }
    static bool GuidOwned { get { return mode == "owned-project-guid-fixture" || SdkOwned; } }
    internal static bool GuidMode { get { return GuidOriginal || GuidOwned; } }
    static bool ExistsOriginal { get { return mode == "original-project-content-store-exists-fixture" || GuidOriginal; } }
    static bool ExistsOwned { get { return mode == "owned-project-content-store-exists-fixture" || GuidOwned; } }
    static bool ExistsMode { get { return ExistsOriginal || ExistsOwned; } }
    static bool ContentDiagnosticsOriginal { get { return mode == "original-project-content-store-diagnostics" || ExistsOriginal; } }
    static bool ContentDiagnosticsOwned { get { return mode == "owned-project-content-store-diagnostics" || ExistsOwned; } }
    internal static bool ContentDiagnosticsMode { get { return ContentDiagnosticsOriginal || ContentDiagnosticsOwned; } }
    static bool ContentOriginal { get { return mode == "original-project-content-store-reference" || ContentDiagnosticsOriginal; } }
    static bool ContentOwned { get { return mode == "owned-project-content-store-reference" || ContentDiagnosticsOwned; } }
    internal static bool ContentMode { get { return ContentOriginal || ContentOwned; } }
    static bool XmlRegistrationOriginal { get { return mode == "original-project-context-xml-registration" || ContentOriginal; } }
    internal static bool StoreRequestDiagnostics { get { return XmlRegistrationOriginal || XmlRegistrationOwned; } }
    static bool XmlRegistrationOwned { get { return mode == "owned-project-context-xml-registration" || ContentOwned; } }
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
    static readonly ReferenceHealthyLogCoalescer healthyLog=new ReferenceHealthyLogCoalescer();
    static void Record(string value)
    {
        if(!(FixtureObservationProfile()?healthyLog.ShouldEmitIdentity(CompactProductDiagnostics(),value):healthyLog.ShouldEmit(CompactProductDiagnostics(),value)))return;
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
        return (selectedMode == "original-factory-empty-container" || selectedMode == "original-trace-encryptor" || selectedMode == "original-factory-registered-trace" || selectedMode == "original-project-genuine-factory" || selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration" || selectedMode == "original-project-content-store-reference" || selectedMode == "original-project-content-store-diagnostics" || selectedMode == "original-project-content-store-exists-fixture" || selectedMode == "original-project-guid-fixture" || (selectedMode == "original-sdk-project-fixture" || (selectedMode == "original-sdk-initialize-project" || (selectedMode == "original-sdk-device-fixture" || selectedMode == OriginalProductImportProbe.Mode)))) && complete && identity == ValueTupleIdentity && cache && !reflectionOnly
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
    // Only the twelve actually requested/verified GUID-profile dependencies; no discovery/closure loader.
    static readonly string[] GuidExtraFiles = { "Knx.Ets.HistoryCompare.dll", "Knx.Ets.DynamicData.dll", "Knx.Ets.ActiveTree.dll", "log4net.dll", "Newtonsoft.Json.dll", "Knx.Ets.Telemetry.Abstractions.dll", "Microsoft.Bcl.HashCode.dll", "System.Collections.Immutable.dll", "System.Text.Json.dll", "System.IO.Pipelines.dll", "System.Text.Encodings.Web.dll", "System.Reactive.dll" };
    static readonly int[] GuidExtraSizes = { 66000, 344528, 240080, 294912, 711952, 109520, 26920, 259360, 726320, 84792, 79664, 1359952 };
    static readonly string[] GuidExtraHashes = { "b7be3b711d244ff40e2a0deaf64fa6ead2e5437ee6f20e7ab0e998465de48b47", "29862fe6678d099706d4d40556e98f62c1a8f9dd532551ef445f4cc56fed3426", "ceec892812bde84ffa0fac4a6cc896cba5cbe56a077d7c3ca3ddabbaecf22078", "232bda95cd37864a3650834015ee76fd42fc2548176a15ceddbed386baa1812b", "e1e27af7b07eeedf5ce71a9255f0422816a6fc5849a483c6714e1b472044fa9d", "3c870c63d4b00009520421a23b3c788ceab737d0dffbc359089fd794e387edf8", "3a4e851ee5fc0f6182aa5a3d65dc56fcd6979b65334b5c3b92fbdc791457c0ab", "77d705fe51f224d77b25fc06ede39e2921f08b46a961a027d6ee49a4bb19c127", "ead4137d1b7ed06d55f609acfedfdad041977b029a9d2c7e8b596f698fedd8ac", "d881b5e01c527bb7cafa3c2482f7cbfbf158fcbb6f93efb4381b638c2d4ea6f7", "ad6be4be7e0183f3eabac3f747b834c557156942c7d2ce12039477ad0874fbfb", "2e998da005362da69f2e87186f675e930d3666ab99683dcff62d8b8b7b67bb38" };
    static readonly string[] GuidExtraIdentities = { "Knx.Ets.HistoryCompare, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", "Knx.Ets.DynamicData, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", "Knx.Ets.ActiveTree, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", "log4net, Version=3.0.3.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a", "Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed", "Knx.Ets.Telemetry.Abstractions, Version=6.0.171.0, Culture=neutral, PublicKeyToken=61439873ec5e1159", "Microsoft.Bcl.HashCode, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Collections.Immutable, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Text.Json, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.IO.Pipelines, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Text.Encodings.Web, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51", "System.Reactive, Version=6.0.0.0, Culture=neutral, PublicKeyToken=94bc3704cddfc263" };
    static bool guidExtrasReady;
    static bool GuidExtraResolvePolicy(string selected, bool complete, string raw, string policy, int index)
    { return (selected == "original-project-guid-fixture" || (selected == "original-sdk-project-fixture" || (selected == "original-sdk-initialize-project" || (selected == "original-sdk-device-fixture" || selected == OriginalProductImportProbe.Mode)))) && complete && index >= 0 && index < GuidExtraFiles.Length && raw == GuidExtraIdentities[index] && policy == GuidExtraIdentities[index]; }
    static bool GuidExtraReferencePolicy(string selected, bool complete, string identity, string location, bool gac, bool reflection, bool dynamicAssembly, string directory, int index)
    { return (selected == "original-project-guid-fixture" || (selected == "original-sdk-project-fixture" || (selected == "original-sdk-initialize-project" || (selected == "original-sdk-device-fixture" || selected == OriginalProductImportProbe.Mode)))) && complete && index >= 0 && index < GuidExtraFiles.Length && identity == GuidExtraIdentities[index] && !gac && !reflection && !dynamicAssembly && !String.IsNullOrEmpty(location) && !String.IsNullOrEmpty(directory) && ProbeFiles.Same(location, Path.Combine(directory, GuidExtraFiles[index])); }
    static void PinGuidExtras()
    {
        for (int i = 0; i < GuidExtraFiles.Length; i++) {
            var file = new FileStream(ProbeFiles.Local(Path.Combine(stage, GuidExtraFiles[i]), true, false), FileMode.Open, FileAccess.Read, FileShare.Read); Inputs.Add(file);
            ProbeFiles.Require(file.Length == GuidExtraSizes[i] && ProbeFiles.Hash(file) == GuidExtraHashes[i], "GUID_EXTRA_INPUT_PIN");
            Record("GUID_EXTRA_PIN name=" + GuidExtraFiles[i] + " bytes=" + file.Length + " sha256=" + GuidExtraHashes[i]);
        }
    }
    static Assembly ResolveGuidExtra(string raw, string policy, int index)
    {
        try {
            ProbeFiles.Require(GuidExtraResolvePolicy(mode, guidExtrasReady && xmlRegistrationReady && containerInputsReady, raw, policy, index), "GUID_EXTRA_EXACT_PROFILE_REQUIRED");
            Assembly assembly = Assembly.LoadFrom(Path.Combine(stage, GuidExtraFiles[index]));
            ProbeFiles.Require(GuidExtraReferencePolicy(mode, guidExtrasReady && containerInputsReady, assembly.FullName, assembly.Location, assembly.GlobalAssemblyCache, assembly.ReflectionOnly, assembly.IsDynamic, stage, index), "GUID_EXTRA_PRIVATE_IDENTITY");
            Record("GUID_EXTRA_RESOLVED identity=" + Text(assembly.FullName) + " held-pin=" + GuidExtraHashes[index]); return assembly;
        } catch { rejected = true; throw; }
    }

    const string SdkHawkName="Knx.Ets.HawkNET";
    const string SdkHawkFile="Knx.Ets.HawkNET.dll";
    const string SdkHawkIdentity="Knx.Ets.HawkNET, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    const string SdkHawkHash="98358825322e75667c8b77ff148ccdad0c78b9ad2f694f93ce02c323399f8aba";
    const int SdkHawkSize=5086672;
    static bool sdkHawkReady;
    static bool SdkHawkResolvePolicy(string selected,bool ready,string raw,string policy)
    {return (selected=="original-sdk-project-fixture" || (selected == "original-sdk-initialize-project" || (selected == "original-sdk-device-fixture" || selected == OriginalProductImportProbe.Mode)))&&ready&&raw==SdkHawkIdentity&&policy==SdkHawkIdentity;}
    static bool SdkHawkReferencePolicy(string selected,bool ready,string identity,string location,bool gac,bool reflection,bool dynamicAssembly,string directory)
    {return (selected=="original-sdk-project-fixture" || (selected == "original-sdk-initialize-project" || (selected == "original-sdk-device-fixture" || selected == OriginalProductImportProbe.Mode)))&&ready&&identity==SdkHawkIdentity&&!gac&&!reflection&&!dynamicAssembly&&!String.IsNullOrEmpty(location)&&!String.IsNullOrEmpty(directory)&&ProbeFiles.Same(location,Path.Combine(directory,SdkHawkFile));}
    static void PinSdkHawk()
    {
        var file=new FileStream(ProbeFiles.Local(Path.Combine(stage,SdkHawkFile),true,false),FileMode.Open,FileAccess.Read,FileShare.Read);Inputs.Add(file);
        ProbeFiles.Require(file.Length==SdkHawkSize&&ProbeFiles.Hash(file)==SdkHawkHash,"SDK_HAWK_INPUT_PIN");Record("SDK_HAWK_PIN bytes="+file.Length+" sha256="+SdkHawkHash+" purpose=IHOSTCONTEXT_ENUM_ABI_ONLY");
    }
    static Assembly ResolveSdkHawk(string raw,string policy)
    {
        try{ProbeFiles.Require(SdkHawkResolvePolicy(mode,sdkHawkReady&&guidExtrasReady&&xmlRegistrationReady&&containerInputsReady,raw,policy),"SDK_HAWK_EXACT_PROFILE");Assembly assembly=Assembly.LoadFrom(Path.Combine(stage,SdkHawkFile));ProbeFiles.Require(SdkHawkReferencePolicy(mode,sdkHawkReady&&containerInputsReady,assembly.FullName,assembly.Location,assembly.GlobalAssemblyCache,assembly.ReflectionOnly,assembly.IsDynamic,stage),"SDK_HAWK_PRIVATE_ORIGIN");Record("SDK_HAWK_RESOLVED identity="+Text(assembly.FullName)+" held-pin="+SdkHawkHash+" ABI_ONLY=True");return assembly;}
        catch{rejected=true;throw;}
    }
    static readonly string[] ProductDependencyFiles={OriginalProductImportProbe.DependencyFile,OriginalProductImportProbe.CodePagesFile,OriginalProductImportProbe.CatalogAbiFile,OriginalProductImportProbe.XmlModelFile,OriginalProductImportProbe.CvNextFile,OriginalProductImportProbe.ExtractFile,OriginalProductImportProbe.SigningFile,OriginalProductImportProbe.CryptoXmlFile,OriginalProductImportProbe.LoggingFile};
    static readonly int[] ProductDependencySizes={OriginalProductImportProbe.DependencySize,OriginalProductImportProbe.CodePagesSize,OriginalProductImportProbe.CatalogAbiSize,OriginalProductImportProbe.XmlModelSize,OriginalProductImportProbe.CvNextSize,OriginalProductImportProbe.ExtractSize,OriginalProductImportProbe.SigningSize,OriginalProductImportProbe.CryptoXmlSize,OriginalProductImportProbe.LoggingSize};
    static readonly string[] ProductDependencyHashes={OriginalProductImportProbe.DependencyHash,OriginalProductImportProbe.CodePagesHash,OriginalProductImportProbe.CatalogAbiHash,OriginalProductImportProbe.XmlModelHash,OriginalProductImportProbe.CvNextHash,OriginalProductImportProbe.ExtractHash,OriginalProductImportProbe.SigningHash,OriginalProductImportProbe.CryptoXmlHash,OriginalProductImportProbe.LoggingHash};
    static readonly string[] ProductDependencyIdentities={OriginalProductImportProbe.DependencyIdentity,OriginalProductImportProbe.CodePagesIdentity,OriginalProductImportProbe.CatalogAbiIdentity,OriginalProductImportProbe.XmlModelIdentity,OriginalProductImportProbe.CvNextIdentity,OriginalProductImportProbe.ExtractIdentity,OriginalProductImportProbe.SigningIdentity,OriginalProductImportProbe.CryptoXmlIdentity,OriginalProductImportProbe.LoggingIdentity};
    static bool sdkProductDependencyReady;
    static bool ProductDependencyPolicy(string selected,bool complete,string raw,string policy,int index)
    {return selected==OriginalProductImportProbe.Mode&&complete&&index>=0&&index<ProductDependencyFiles.Length&&raw==ProductDependencyIdentities[index]&&policy==ProductDependencyIdentities[index];}
    static bool ProductDependencyReferencePolicy(string selected,bool complete,Assembly assembly,int index)
    {return selected==OriginalProductImportProbe.Mode&&complete&&index>=0&&index<ProductDependencyFiles.Length&&!assembly.IsDynamic&&!assembly.ReflectionOnly&&!assembly.GlobalAssemblyCache&&assembly.FullName==ProductDependencyIdentities[index]&&ProbeFiles.Same(assembly.Location,Path.Combine(stage,ProductDependencyFiles[index]));}
    static bool ProductDependencyComplete{get{return SdkProductMode&&sdkProductDependencyReady&&sdkHawkReady&&guidExtrasReady&&xmlRegistrationReady&&containerInputsReady;}}
    static void PinSdkProductDependency()
    {
        ProbeFiles.Require(SdkProductMode&&!sdkProductDependencyReady,"PRODUCT_IMPORT_DEPENDENCY_PIN_PHASE");
        for(int i=0;i<ProductDependencyFiles.Length;i++){
            Pin(Path.Combine(stage,ProductDependencyFiles[i]),ProductDependencyHashes[i]);
            ProbeFiles.Require(Inputs[Inputs.Count-1].Length==ProductDependencySizes[i],"PRODUCT_IMPORT_DEPENDENCY_SIZE");
        }
    }
    internal static Assembly LoadProductImportConverter()
    {
        const int index=4;
        ProjectResourceRequire(SdkProductMode&&ProductDependencyComplete&&SdkWrapperEligible()&&ProductDependencyFiles.Length>index
            &&ProductDependencyFiles[index]==OriginalProductImportProbe.CvNextFile&&ProductDependencyIdentities[index]==OriginalProductImportProbe.CvNextIdentity,"PRODUCT_IMPORT_CONVERTER_LOAD_PHASE");
        string identity=ProductDependencyIdentities[index];return ResolveProductDependency(identity,AppDomain.CurrentDomain.ApplyPolicy(identity),index);
    }
    static Assembly ResolveProductDependency(string raw,string policy,int index)
    {
        try{
            ProbeFiles.Require(ProductDependencyPolicy(mode,ProductDependencyComplete,raw,policy,index),"PRODUCT_IMPORT_DEPENDENCY_EXACT_PROFILE");
            Assembly value=Assembly.LoadFrom(Path.Combine(stage,ProductDependencyFiles[index]));
            ProbeFiles.Require(ProductDependencyReferencePolicy(mode,ProductDependencyComplete,value,index),"PRODUCT_IMPORT_DEPENDENCY_PRIVATE_ORIGIN");
            Record("PRODUCT_IMPORT_DEPENDENCY_RESOLVED identity="+Text(value.FullName)+" held-pin="+ProductDependencyHashes[index]+" purpose="+(index==2?"SHARED_LOCAL_CACHE_ENUM_ABI_ONLY":index==3?"ORIGINAL_LOCAL_XML_SCHEMA_MODEL":index==4?"ORIGINAL_LOCAL_SCHEMA_VERSION_CONVERTER":index==8?"ORIGINAL_FLUENTRESULTS_LOGLEVEL_ABI":index>=5?"ORIGINAL_LOCAL_CONVERSION_EXTRACT_AND_SIGNATURES":"ORIGINAL_LOCAL_ZIP_ENCODING"));return value;
        }catch{rejected=true;throw;}
    }
    static Assembly ResolveProductLoggingLegacy(ResolveEventArgs args)
    {
        const int index=8;
        try{
            string policy=AppDomain.CurrentDomain.ApplyPolicy(args.Name);Assembly requester=args.RequestingAssembly;
            bool matches=requester!=null&&!requester.IsDynamic&&!requester.ReflectionOnly&&!requester.GlobalAssemblyCache&&requester.FullName==Identities[9]
                &&ProbeFiles.Same(requester.Location,Path.Combine(stage,Names[9]+".dll"));
            ProbeFiles.Require(SdkProductMode&&ProductDependencyComplete&&SdkWrapperEligible()&&sdkProductImportInvokeActive
                &&args.Name==OriginalProductImportProbe.LoggingLegacyIdentity&&(policy==args.Name||policy==OriginalProductImportProbe.LoggingIdentity)
                &&(requester==null||matches)&&ProductDependencyFiles.Length>index&&ProductDependencyIdentities[index]==OriginalProductImportProbe.LoggingIdentity,"PRODUCT_IMPORT_LOGGING_SOURCE_POLICY");
            Record("PRODUCT_IMPORT_BINDING_ALIAS requested="+Text(args.Name)+" clr-post-policy="+Text(policy)+" source-policy-target="+OriginalProductImportProbe.LoggingIdentity
                +" requester="+(requester==null?"NULL_UNKNOWN":"PINNED_FLUENTRESULTS")+" policy-source=ORIGINAL_ETS6C_AND_ETS6N_CONFIG strategy=EXACT_PRODUCT_ASSEMBLYRESOLVE_ALIAS");
            Assembly value=Assembly.LoadFrom(Path.Combine(stage,ProductDependencyFiles[index]));
            ProbeFiles.Require(ProductDependencyReferencePolicy(mode,ProductDependencyComplete,value,index),"PRODUCT_IMPORT_LOGGING_PRIVATE_ORIGIN");
            Record("PRODUCT_IMPORT_LOGGING_ALIAS_RETURN identity="+Text(value.FullName)+" held-pin="+ProductDependencyHashes[index]);return value;
        }catch{rejected=true;throw;}
    }
    static void PinXmlRegistrationInputs()
    {
        ProbeFiles.Require(XmlRegistrationOriginal && !xmlRegistrationReady && !containerInputsReady, "XML_PROFILE_NEW_MODE");
        CountInputs(SdkOriginal ? 21 + GuidExtraFiles.Length + (SdkProductMode?ProductDependencyFiles.Length:0) : GuidOriginal ? 20 + GuidExtraFiles.Length : 20);
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
        if (GuidOriginal) PinGuidExtras();
        if(SdkOriginal)PinSdkHawk();
        if(SdkProductMode){
            PinSdkProductDependency();
            Pin(Path.Combine(Path.GetDirectoryName(stage),OriginalProductImportProbe.ArchiveFile),OriginalProductImportProbe.ArchiveHash);
            ProbeFiles.Require(Inputs[Inputs.Count-1].Length==OriginalProductImportProbe.ArchiveSize,"PRODUCT_IMPORT_SOURCE_SIZE");
        }
        PinValueTupleReference();
        PinPlatformFacade();
        for (int i = 0; i < Names.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, Names[i] + ".dll")).FullName == Identities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < FactoryExtraNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, FactoryExtraNames[i] + ".dll")).FullName == FactoryExtraIdentities[i], "CONTAINER_INPUT_IDENTITY");
        for (int i = 0; i < ContainerNames.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, ContainerNames[i] + ".dll")).FullName == ContainerIdentities[i], "CONTAINER_INPUT_IDENTITY");
        ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, XmlRegistrationName + ".dll")).FullName == XmlRegistrationIdentity, "XML_INPUT_IDENTITY");
        if (GuidOriginal) { for (int i = 0; i < GuidExtraFiles.Length; i++) ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage, GuidExtraFiles[i])).FullName == GuidExtraIdentities[i], "GUID_EXTRA_INPUT_IDENTITY"); guidExtrasReady = true; }
        if(SdkOriginal){ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage,SdkHawkFile)).FullName==SdkHawkIdentity,"SDK_HAWK_INPUT_IDENTITY");sdkHawkReady=true;}
        if(SdkProductMode){for(int i=0;i<ProductDependencyFiles.Length;i++)ProbeFiles.Require(AssemblyName.GetAssemblyName(Path.Combine(stage,ProductDependencyFiles[i])).FullName==ProductDependencyIdentities[i],"PRODUCT_IMPORT_DEPENDENCY_IDENTITY");sdkProductDependencyReady=true;}
        xmlRegistrationReady = true;
        containerInputsReady = true;
        if(SdkProductMode){OriginalProductImportProbe.RequireDependencyReady(sdkProductDependencyReady&&sdkHawkReady&&guidExtrasReady&&xmlRegistrationReady&&containerInputsReady);Record("SDK_PRODUCT_PROFILE_READY private-pins=42 total-bytes=46602392 archive-pin=SEPARATE_IMMUTABLE exact-extra-dependencies=9 operations=NOT_GRANTED");}
        else if(SdkOriginal)Record("SDK_PROFILE_READY private-pins=33 total-bytes=33857608 sdk-only-references=1 guid-extra-references=12 operations=NOT_GRANTED");
        else if (GuidOriginal) Record("GUID_EXTRA_PROFILE_READY private-pins=32 total-bytes=28770936 exact-extra-references=12");
        else Record("XML_REGISTRATION_PROFILE_READY private-pins=20 total-bytes=24466936 extra-reference-admission=THIS_MODE_ONLY");
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
    { return (selectedMode == "original-project-context-xml-registration" || selectedMode == "original-project-content-store-reference" || selectedMode == "original-project-content-store-diagnostics" || selectedMode == "original-project-content-store-exists-fixture" || selectedMode == "original-project-guid-fixture" || (selectedMode == "original-sdk-project-fixture" || (selectedMode == "original-sdk-initialize-project" || (selectedMode == "original-sdk-device-fixture" || selectedMode == OriginalProductImportProbe.Mode)))) && complete && requested == XmlRegistrationIdentity && policy == XmlRegistrationIdentity; }
    static bool XmlRegistrationReferencePolicy(string selectedMode, bool complete, string identity, string location, bool gac, bool reflectionOnly, bool dynamicAssembly, string directory)
    {
        return (selectedMode == "original-project-context-xml-registration" || selectedMode == "original-project-content-store-reference" || selectedMode == "original-project-content-store-diagnostics" || selectedMode == "original-project-content-store-exists-fixture" || selectedMode == "original-project-guid-fixture" || (selectedMode == "original-sdk-project-fixture" || (selectedMode == "original-sdk-initialize-project" || (selectedMode == "original-sdk-device-fixture" || selectedMode == OriginalProductImportProbe.Mode)))) && complete && identity == XmlRegistrationIdentity && !gac && !reflectionOnly && !dynamicAssembly
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
        if ((selectedMode != "original-factory-empty-container" && selectedMode != "original-trace-encryptor" && selectedMode != "original-factory-registered-trace" && selectedMode != "original-project-genuine-factory" && selectedMode != "original-project-genuine-factory-resources" && selectedMode != "original-project-undo-provider-denied" && selectedMode != "original-project-undo-provider-factory" && selectedMode != "original-project-large-string-cache-denied" && selectedMode != "original-project-context-denied" && selectedMode != "original-project-context-xml-registration" && selectedMode != "original-project-content-store-reference" && selectedMode != "original-project-content-store-diagnostics" && selectedMode != "original-project-content-store-exists-fixture" && selectedMode != "original-project-guid-fixture" && (selectedMode != "original-sdk-project-fixture" && (selectedMode != "original-sdk-initialize-project" && (selectedMode != "original-sdk-device-fixture" && selectedMode != OriginalProductImportProbe.Mode)))) || !complete || reflectionOnly) return false;
        for (int i = 0; i < ContainerNames.Length; i++)
            if (identity == ContainerIdentities[i] && ProbeFiles.Same(location, Path.Combine(stage, ContainerNames[i] + ".dll"))) return true;
        return false;
    }
    // Private finite policy seam; real facts come from the completed pin and Assembly.
    static bool FactoryReferencePolicy(string selectedMode, bool complete, string identity, string location, bool reflectionOnly)
    {
        if ((selectedMode != "original-factory" && selectedMode != "original-factory-bcl9" && selectedMode != "original-factory-empty-container" && selectedMode != "original-trace-encryptor" && selectedMode != "original-factory-registered-trace" && selectedMode != "original-project-genuine-factory" && selectedMode != "original-project-genuine-factory-resources" && selectedMode != "original-project-undo-provider-denied" && selectedMode != "original-project-undo-provider-factory" && selectedMode != "original-project-large-string-cache-denied" && selectedMode != "original-project-context-denied" && selectedMode != "original-project-context-xml-registration" && selectedMode != "original-project-content-store-reference" && selectedMode != "original-project-content-store-diagnostics" && selectedMode != "original-project-content-store-exists-fixture" && selectedMode != "original-project-guid-fixture" && (selectedMode != "original-sdk-project-fixture" && (selectedMode != "original-sdk-initialize-project" && (selectedMode != "original-sdk-device-fixture" && selectedMode != OriginalProductImportProbe.Mode)))) || !complete || reflectionOnly) return false;
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
        bool allowed = assembly == typeof(ProjectConstructorProbe).Assembly, productDetail=false;
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
        for (int i = 0; i < GuidExtraFiles.Length; i++) if (String.Equals(assembly.GetName().Name, Path.GetFileNameWithoutExtension(GuidExtraFiles[i]), StringComparison.OrdinalIgnoreCase)) {
            bool selected = GuidExtraReferencePolicy(mode, guidExtrasReady && xmlRegistrationReady && containerInputsReady, assembly.FullName, assembly.IsDynamic ? "UNAVAILABLE_DYNAMIC" : assembly.Location, !assembly.IsDynamic && assembly.GlobalAssemblyCache, assembly.ReflectionOnly, assembly.IsDynamic, stage, i);
            allowed = selected; if (!selected) unexpected = true;productDetail=SdkProductMode;
            Record("GUID_EXTRA_AUDIT allowed=" + selected + " identity=" + Text(assembly.FullName) + " held-pin=" + (selected ? GuidExtraHashes[i] : "NOT_ESTABLISHED") + " detection=after-load prevention=NOT_ESTABLISHED");
        }
        if(String.Equals(assembly.GetName().Name,SdkHawkName,StringComparison.OrdinalIgnoreCase)){
            bool selected=SdkHawkReferencePolicy(mode,sdkHawkReady&&guidExtrasReady&&xmlRegistrationReady&&containerInputsReady,assembly.FullName,assembly.IsDynamic?"UNAVAILABLE_DYNAMIC":assembly.Location,!assembly.IsDynamic&&assembly.GlobalAssemblyCache,assembly.ReflectionOnly,assembly.IsDynamic,stage);allowed=selected;if(!selected)unexpected=true;
            Record("SDK_HAWK_AUDIT allowed="+selected+" identity="+Text(assembly.FullName)+" held-pin="+(selected?SdkHawkHash:"NOT_ESTABLISHED")+" detection=after-load prevention=NOT_ESTABLISHED operations=NOT_GRANTED");
        }
        for(int i=0;i<ProductDependencyFiles.Length;i++)if(String.Equals(assembly.GetName().Name,Path.GetFileNameWithoutExtension(ProductDependencyFiles[i]),StringComparison.OrdinalIgnoreCase)){
            bool selected=ProductDependencyReferencePolicy(mode,ProductDependencyComplete,assembly,i);allowed=selected;if(!selected)unexpected=true;productDetail=SdkProductMode;
            Record("PRODUCT_IMPORT_DEPENDENCY_AUDIT allowed="+selected+" identity="+Text(assembly.FullName)+" held-pin="+(selected?ProductDependencyHashes[i]:"NOT_ESTABLISHED")+" detection=after-load prevention=NOT_ESTABLISHED");
        }
        if(!allowed&&SdkProductMode)allowed=AllowProductGenerated(assembly);
        if(!allowed||!productDetail){
            if(allowed&&CompactProductDiagnostics()&&!assembly.IsDynamic)Interlocked.Increment(ref sdkUiCompactedAssemblyLogs);
            else Record("PROJECT_ASSEMBLY allowed=" + allowed + " reflection-only=" + assembly.ReflectionOnly + " identity=" + Text(assembly.FullName));
        }
        if (!allowed) { unexpected = true;
            if(SdkProductMode&&(assembly.GetName().Name=="Anonymously Hosted DynamicMethods Assembly"||assembly.GetName().Name=="Microsoft.GeneratedCode"))RecordProductOrigin("GENERATED_LOAD",assembly.FullName,assembly);
            Record("UNEXPECTED_ASSEMBLY_BLOCKER detection=after-load prevention=NOT_ESTABLISHED"); }
    }
    const string ProductXmlSerializerIdentity="Knx.Ets.Xml.ObjectModel.XmlSerializers, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159";
    static readonly object productCodegenGate=new object();static int productAnonymousLoads,productXmlLoads,productXmlProbes;
    static int CodegenEntry(ref int count){lock(productCodegenGate){if(count<2)count++;return count;}}
    static bool ProductFrame(MethodBase method,int token,string owner,string name,string identity,string file,bool framework)
    {
        if(method.MetadataToken!=token||method.Name!=name||method.DeclaringType==null||method.DeclaringType.FullName!=owner)return false;
        Assembly assembly=method.Module.Assembly;
        if(identity==null)return assembly==typeof(object).Assembly&&method.Module==typeof(object).Module;
        return !assembly.IsDynamic&&!assembly.ReflectionOnly&&assembly.FullName==identity
            &&(framework?Framework(assembly):!assembly.GlobalAssemblyCache&&ProbeFiles.Same(assembly.Location,Path.Combine(stage,file)));
    }
    static int ProductOriginProof()
    {
        int proof=0;var trace=new StackTrace(false);MethodInfo owned=typeof(OriginalProductImportProbe).GetMethod("CreateContainer",BindingFlags.NonPublic|BindingFlags.Static);
        for(int i=0;i<trace.FrameCount&&i<96;i++){
            MethodBase method=trace.GetFrame(i).GetMethod();if(method==null)continue;Type runtime=method.GetType();
            if(runtime!=typeof(object).GetMethod("GetType").GetType()&&runtime!=typeof(object).GetConstructor(Type.EmptyTypes).GetType())continue;
            if(ProductFrame(method,0x06004a3c,"System.Reflection.Emit.DynamicMethod","GetDynamicMethodsModule",null,null,false))proof|=1;
            if(ProductFrame(method,0x0600172d,"System.Linq.Expressions.Compiler.LambdaCompiler","Compile","System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",null,true))proof|=2;
            if(ProductFrame(method,0x0600069b,"Autofac.Core.Activators.Reflection.ConstructorBinder","GetConstructorInvoker",FactoryExtraIdentities[0],"Autofac.dll",false))proof|=4;
            if(method.Module.Assembly==typeof(ProjectConstructorProbe).Assembly&&method.Equals(owned))proof|=8;
            if(ProductFrame(method,0x060016d5,"System.Xml.Serialization.TempAssembly","LoadGeneratedAssembly","System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",null,true))proof|=16;
            if(ProductFrame(method,0x060016db,"System.Xml.Serialization.TempAssembly","GenerateRefEmitAssembly","System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",null,true))proof|=32;
            if(ProductFrame(method,0x06001e92,"System.Xml.Serialization.XmlSerializer",".ctor","System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",null,true))proof|=64;
            if(ProductFrame(method,0x0600096b,"Knx.Ets.Xml.ObjectModel.Document",".cctor",OriginalProductImportProbe.XmlModelIdentity,OriginalProductImportProbe.XmlModelFile,false))proof|=128;
            if(ProductFrame(method,0x0600018a,"Knx.Ets.XmlSigning.Signer.RegistrationInfoSigner",".ctor",OriginalProductImportProbe.SigningIdentity,OriginalProductImportProbe.SigningFile,false))proof|=256;
            if(ProductFrame(method,0x0600002e,"Knx.Ets.CvNext.Converter","Convert",OriginalProductImportProbe.CvNextIdentity,OriginalProductImportProbe.CvNextFile,false))proof|=512;
            if(ProductFrame(method,0x060007bc,"System.AppDomain","DefineDynamicAssembly",null,null,false))proof|=1024;
        }
        return proof;
    }
    static bool AllowProductGenerated(Assembly assembly)
    {
        string identity=assembly.FullName;bool anonymous=identity=="Anonymously Hosted DynamicMethods Assembly, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        bool xml=identity=="Microsoft.GeneratedCode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        if(!SdkProductMode||!anonymous&&!xml)return false;int count=anonymous?CodegenEntry(ref productAnonymousLoads):CodegenEntry(ref productXmlLoads);
        int proof=ProductOriginProof(),required=anonymous?15:2016;
        bool allowed=count==1&&ProductDependencyComplete&&SdkWrapperEligible()&&(!xml||sdkProductConversionActive)
            &&assembly.IsDynamic&&!assembly.ReflectionOnly&&!assembly.GlobalAssemblyCache&&assembly.GetType().Assembly==typeof(object).Assembly
            &&assembly.GetType().FullName=="System.Reflection.Emit.InternalAssemblyBuilder"&&assembly.ManifestModule.ScopeName=="RefEmit_InMemoryManifestModule"&&(proof&required)==required;
        Record("PRODUCT_GENERATED_PATH_AUDIT allowed="+allowed+" kind="+(anonymous?"AUTOFAC_CONSTRUCTOR_EXPRESSION":"ORIGINAL_XML_SIGNING_SERIALIZER")+" entry="+count+" proof-mask="+proof.ToString("X4")+" required-mask="+required.ToString("X4")+" module-content-hash=NOT_AVAILABLE prevention=NOT_ESTABLISHED");
        if(allowed)RecordProductOrigin("QUALIFIED_GENERATED_LOAD",identity,assembly,true);return allowed;
    }
    static bool AllowProductXmlSerializerProbe(ResolveEventArgs args)
    {
        int count=CodegenEntry(ref productXmlProbes),proof=ProductOriginProof();Assembly requester=args.RequestingAssembly;
        bool match=requester!=null&&!requester.IsDynamic&&!requester.ReflectionOnly&&!requester.GlobalAssemblyCache&&requester.FullName==OriginalProductImportProbe.XmlModelIdentity&&ProbeFiles.Same(requester.Location,Path.Combine(stage,OriginalProductImportProbe.XmlModelFile));
        bool allowed=SdkProductMode&&count==1&&ProductDependencyComplete&&SdkWrapperEligible()&&sdkProductConversionActive&&args.Name==ProductXmlSerializerIdentity
            &&AppDomain.CurrentDomain.ApplyPolicy(args.Name)==args.Name&&(requester==null||match)&&(proof&976)==976;
        Record("PRODUCT_XMLSERIALIZER_PROBE allowed="+allowed+" entry="+count+" proof-mask="+proof.ToString("X4")+" requester="+(requester==null?"NULL_UNKNOWN":match?"PINNED_XML_MODEL":"FOREIGN")+" action="+(allowed?"RETURN_NULL_FRAMEWORK_GENERATION":"DENY"));
        if(allowed)RecordProductOrigin("QUALIFIED_XMLSERIALIZERS_PROBE",args.Name,requester,true);return allowed;
    }
    static readonly object productOriginGate=new object();
    static int productOriginEvents,productOriginBytes;
    static string productOriginPath;
    [ThreadStatic] static bool productOriginRecording;
    static string OriginText(string value,int bound){string text=Text(value??"NULL");return text.Length<=bound?text:text.Substring(0,bound)+"[truncated]";}
    static void RecordProductOrigin(string kind,string identity,Assembly assembly,bool qualified=false)
    {
        if(!SdkProductMode||!ProductDependencyComplete||productOriginRecording)return;
        lock(productOriginGate){
            if(productOriginEvents>=3)return;productOriginEvents++;productOriginRecording=true;
            try{
                var text=new StringBuilder();text.AppendLine("event="+productOriginEvents+" kind="+kind+" identity="+OriginText(identity,256)+" thread="+Thread.CurrentThread.ManagedThreadId+" wrapper="+sdkWrapperPhase+" import="+sdkProductImportEngineActive+" admission="+(qualified?"QUALIFIED_OBSERVED_PATH":"DENIED"));
                if(assembly!=null)text.AppendLine("assembly dynamic="+assembly.IsDynamic+" reflection-only="+assembly.ReflectionOnly+" gac="+assembly.GlobalAssemblyCache+" runtime-kind="+OriginText(assembly.GetType().FullName,128)+" module="+OriginText(assembly.ManifestModule.ScopeName,128)+" location-category="+(assembly.IsDynamic?"DYNAMIC":String.IsNullOrEmpty(assembly.Location)?"EMPTY":"NONEMPTY_UNQUALIFIED"));
                var trace=new StackTrace(false);int count=Math.Min(trace.FrameCount,96);text.AppendLine("frames-total="+trace.FrameCount+" frames-scanned="+count+" emitted=FIRST24_LAST8_WITHIN96 values=NOT_INSPECTED caller-origin=UNQUALIFIED");
                for(int i=0;i<count;i++){
                    if(i>=24&&i<count-8)continue;
                    try{MethodBase method=trace.GetFrame(i).GetMethod();if(method==null){text.AppendLine("frame="+i+" kind=UNAVAILABLE");continue;}
                        Type runtime=method.GetType();if(runtime!=typeof(object).GetMethod("GetType").GetType()&&runtime!=typeof(object).GetConstructor(Type.EmptyTypes).GetType()){text.AppendLine("frame="+i+" kind=NONSTANDARD_RUNTIME_METHOD");continue;}
                        text.AppendLine("frame="+i+" assembly="+OriginText(method.Module.Assembly.GetName().Name,80)+" owner="+OriginText(method.DeclaringType==null?"GLOBAL":method.DeclaringType.FullName,128)+" token="+method.MetadataToken.ToString("X8")+" name="+OriginText(method.Name,80));
                    }catch(Exception unavailable){text.AppendLine("frame="+i+" kind=UNAVAILABLE type="+OriginText(unavailable.GetType().FullName,80));}
                }
                byte[] data=Encoding.UTF8.GetBytes(text.ToString());ProbeFiles.Require(data.Length<=32768-productOriginBytes,"PRODUCT_ORIGIN_EVIDENCE_BOUND");
                bool fresh=productOriginPath==null;if(fresh)productOriginPath=ProbeFiles.Local(Path.Combine(Environment.CurrentDirectory,"product-dependency-origin.txt"),false,false);else{ProbeFiles.Local(productOriginPath,true,false);ProbeFiles.Require(new FileInfo(productOriginPath).Length==productOriginBytes,"PRODUCT_ORIGIN_EVIDENCE_CHANGED");}
                using(var output=new FileStream(productOriginPath,fresh?FileMode.CreateNew:FileMode.Append,FileAccess.Write,FileShare.Read)){output.Write(data,0,data.Length);output.Flush(true);}productOriginBytes+=data.Length;
                Record("PRODUCT_DEPENDENCY_ORIGIN_CAPTURE event="+productOriginEvents+" kind="+kind+" evidence=product-dependency-origin.txt admission="+(qualified?"QUALIFIED_OBSERVED_PATH":"DENIED"));
            }catch(Exception diagnostic){Record("PRODUCT_DEPENDENCY_ORIGIN_UNAVAILABLE type="+OriginText(diagnostic.GetType().FullName,80)+" admission=UNCHANGED_DENIED");}
            finally{productOriginRecording=false;}
        }
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
        return (selectedMode == "original-project-genuine-factory-resources" || selectedMode == "original-project-undo-provider-denied" || selectedMode == "original-project-undo-provider-factory" || selectedMode == "original-project-large-string-cache-denied" || selectedMode == "original-project-context-denied" || selectedMode == "original-project-context-xml-registration" || selectedMode == "original-project-content-store-reference" || selectedMode == "original-project-content-store-diagnostics" || selectedMode == "original-project-content-store-exists-fixture" || selectedMode == "original-project-guid-fixture" || (selectedMode == "original-sdk-project-fixture" || (selectedMode == "original-sdk-initialize-project" || (selectedMode == "original-sdk-device-fixture" || selectedMode == OriginalProductImportProbe.Mode)))) && complete && budget && (window == 1 || window == 2) && !suspended && pinned
            && (request == AutofacResourceEnGb || request == AutofacResourceEn) && (requesterNull ? !matches : matches) && count >= 1 && count <= 8;
    }
    static bool SdkRootResourcePolicy(string selectedMode,bool complete,bool budget,bool rootOwnerPhase,int window,bool suspended,bool pinned,string request,bool requesterNull,bool matches,int count)
    {return (selectedMode=="original-sdk-project-fixture" || (selectedMode == "original-sdk-initialize-project" || (selectedMode == "original-sdk-device-fixture" || selectedMode == OriginalProductImportProbe.Mode)))&&complete&&budget&&rootOwnerPhase&&window==0&&!suspended&&pinned&&(request==AutofacResourceEnGb||request==AutofacResourceEn)&&(requesterNull?!matches:matches)&&count>=1&&count<=8;}
    static bool AllowProjectResourceNull(string request, bool pinned, bool requesterNull, bool matches, Assembly requester)
    {
        bool eligible = ProjectResourcePolicy(mode, containerInputsReady, projectResourceBudgetActive, projectResourceWindow, projectResourceCallbackSuspended,
            pinned, request, requesterNull, matches, 1);
        bool sdkRootOwner=SdkOriginal&&sdkRootSetupPhase&&sdkModelRootConstructorActive&&contextLifetime&&Thread.CurrentThread.ManagedThreadId==sdkOwner;
        eligible=eligible||SdkRootResourcePolicy(mode,containerInputsReady&&xmlRegistrationReady&&guidExtrasReady&&sdkHawkReady,projectResourceBudgetActive,sdkRootOwner,projectResourceWindow,projectResourceCallbackSuspended,pinned,request,requesterNull,matches,1);
        bool sdkDeviceFactoryOwner=SdkDeviceMode&&sdkDeviceFactoryInvokeActive&&sdkWrapperPhase&&contextLifetime&&Thread.CurrentThread.ManagedThreadId==sdkOwner;
        eligible=eligible||(SdkDeviceMode&&SdkRootResourcePolicy(mode,containerInputsReady&&xmlRegistrationReady&&guidExtrasReady&&sdkHawkReady,projectResourceBudgetActive,sdkDeviceFactoryOwner,projectResourceWindow,projectResourceCallbackSuspended,pinned,request,requesterNull,matches,1));
        bool sdkImportOwner=SdkProductMode&&ProductDependencyComplete&&sdkProductImportInvokeActive&&sdkProductImportEngineActive&&sdkWrapperPhase&&contextLifetime&&Thread.CurrentThread.ManagedThreadId==sdkOwner;
        eligible=eligible||(SdkProductMode&&AppDomain.CurrentDomain.ApplyPolicy(request)==request&&SdkRootResourcePolicy(mode,ProductDependencyComplete,projectResourceBudgetActive,sdkImportOwner,projectResourceWindow,projectResourceCallbackSuspended,pinned,request,requesterNull,matches,1));
        bool allowed = ConsumeProjectResourceProbe(eligible);
        Record("PROJECT_AUTOFAC_RESOURCE_PROBE allowed=" + allowed + " requested=" + Text(request)
            + " requester=" + (requesterNull ? "NULL_UNKNOWN" : Text(requester.FullName))
            + " requester-is-pinned-owner=" + matches + " owner-proof=" + (requesterNull ? "NOT_ESTABLISHED" : matches ? "PINNED_AUTOFAC" : "FOREIGN")
            + " profile-complete=" + containerInputsReady + " invocation-thread-window=" + projectResourceWindow + " budget-active=" + projectResourceBudgetActive
            + " callback-suspended=" + projectResourceCallbackSuspended + " sdk-root-constructor-owner="+sdkRootOwner+" sdk-device-factory-owner="+sdkDeviceFactoryOwner+" product-import-engine-owner="+sdkImportOwner+" aggregate-probe=" + projectResourceProbeCount
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
    static readonly object dcaUiResourceGate=new object();static int dcaUiResourceProbes;static bool dcaUiResourceFault;
    const string DcaUiResourceEnGb="MDT-DaliDcaPlus.resources, Version=4.0.0.0, Culture=en-GB, PublicKeyToken=1a3211b0275a5581";
    const string DcaUiResourceEn="MDT-DaliDcaPlus.resources, Version=4.0.0.0, Culture=en, PublicKeyToken=1a3211b0275a5581";
    static bool AllowDcaUiResourceNull(ResolveEventArgs args)
    {
        if(!SdkUiRequested||(args.Name!=DcaUiResourceEnGb&&args.Name!=DcaUiResourceEn))return false;
        int count;lock(dcaUiResourceGate){if(dcaUiResourceProbes<9)dcaUiResourceProbes++;count=dcaUiResourceProbes;}
        Assembly owner=sdkDcaInstance==null?null:sdkDcaInstance.GetType().Assembly,requester=args.RequestingAssembly;
        bool pinned=owner!=null&&!owner.IsDynamic&&!owner.ReflectionOnly&&!owner.GlobalAssemblyCache&&owner.FullName==Identities[0]&&ProbeFiles.Same(owner.Location,Path.Combine(stage,Names[0]+".dll"));
        bool matches=pinned&&Object.ReferenceEquals(owner,requester);string policy=AppDomain.CurrentDomain.ApplyPolicy(args.Name);bool allowed;
        lock(dcaUiResourceGate){allowed=!dcaUiResourceFault&&count<=8&&ProductUiCacheEligible()&&pinned&&(requester==null||matches)&&policy==args.Name;if(!allowed)dcaUiResourceFault=true;}
        Record("DCA_UI_RESOURCE_PROBE allowed="+allowed+" requested="+Text(args.Name)+" post-policy="+Text(policy)+" requester="+(requester==null?"NULL_UNKNOWN":Text(requester.FullName))+" requester-is-pinned-owner="+matches+" aggregate="+count+" maximum=8 action="+(allowed?"RETURN_NULL_ORIGINAL_NEUTRAL_FALLBACK":"CONTINUE_FATAL_DENY"));
        if(!allowed)rejected=true;return allowed;
    }
    static void CheckDcaUiResources(string phase)
    {
        if(!SdkUiRequested)return;lock(dcaUiResourceGate){try{if(dcaUiResourceFault||SdkAuditLog(phase))Record("DCA_UI_RESOURCE_AUDIT phase="+phase+" probes="+dcaUiResourceProbes+" maximum=8 fault="+dcaUiResourceFault+" content=ORIGINAL_EMBEDDED_NEUTRAL_ONLY");}finally{ProbeFiles.Require(!dcaUiResourceFault&&dcaUiResourceProbes<=8,"DCA_UI_RESOURCE_FINAL_DENIAL");}}
    }
    static bool AllowSdkNameResourceNull(ResolveEventArgs args)
    {
        if(args.Name!=SdkNameResourceEnGb&&args.Name!=SdkNameResourceEn)return false;
        bool importScope=SdkProductMode&&sdkProductImportInvokeActive&&ProductDependencyComplete;
        Assembly owner=importScope?sdkProductImportResourceOwner:sdkNameResourceOwner,requester=args.RequestingAssembly;
        bool pinned=owner!=null&&!owner.IsDynamic&&!owner.ReflectionOnly&&!owner.GlobalAssemblyCache&&owner.FullName==OmIdentity&&ProbeFiles.Same(owner.Location,Path.Combine(stage,Names[8]+".dll"));
        bool matches=pinned&&Object.ReferenceEquals(owner,requester);string policy=AppDomain.CurrentDomain.ApplyPolicy(args.Name);
        bool eligible=SdkInitializing&&containerInputsReady&&xmlRegistrationReady&&guidExtrasReady&&sdkHawkReady&&pinned&&(requester==null||matches)&&policy==args.Name
            &&(sdkNameSetterActive||importScope)&&sdkWrapperPhase&&contextLifetime&&Thread.CurrentThread.ManagedThreadId==sdkOwner&&projectResourceBudgetActive&&projectResourceWindow==0&&!projectResourceCallbackSuspended;
        bool allowed=ConsumeProjectResourceProbe(eligible);
        Record("SDK_NAME_RESOURCE_PROBE allowed="+allowed+" requested="+Text(args.Name)+" post-policy="+Text(policy)+" requester="+(requester==null?"NULL_UNKNOWN":Text(requester.FullName))+" requester-is-pinned-owner="+matches+" name-setter-active="+sdkNameSetterActive+" product-import-active="+importScope+" aggregate-probe="+projectResourceProbeCount+" action="+(allowed?"RETURN_NULL_NO_ASSEMBLY":"CONTINUE_FATAL_DENY"));
        return allowed;
    }
    static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        Record("PROJECT_RESOLVE requested=" + Text(args.Name));
        if(SdkProductMode&&args.Name==ProductXmlSerializerIdentity){
            if(AllowProductXmlSerializerProbe(args))return null;
            rejected=true;RecordProductOrigin("XMLSERIALIZERS_PROBE",args.Name,args.RequestingAssembly);
        }
        if(AllowDcaUiResourceNull(args))return null;
        if (AllowSdkNameResourceNull(args)) return null;
        if (AllowAutofacResourceNull(args)) return null;
        if(SdkProductMode&&args.Name==OriginalProductImportProbe.LoggingLegacyIdentity)return ResolveProductLoggingLegacy(args);
        if(SdkProductMode)for(int i=0;i<ProductDependencyFiles.Length;i++)if(args.Name==ProductDependencyIdentities[i])return ResolveProductDependency(args.Name,AppDomain.CurrentDomain.ApplyPolicy(args.Name),i);
        if(SdkOriginal&&args.Name==SdkHawkIdentity)return ResolveSdkHawk(args.Name,AppDomain.CurrentDomain.ApplyPolicy(args.Name));
        if (GuidOriginal) for (int i = 0; i < GuidExtraFiles.Length; i++) if (args.Name == GuidExtraIdentities[i]) return ResolveGuidExtra(args.Name, AppDomain.CurrentDomain.ApplyPolicy(args.Name), i);
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
    static string ownedContentDiagnosticCase;
    static readonly string[] OwnedContentDiagnosticCases = { "stored", "object", "dispose", "first-only", "reentry", "recorder", "cap", "trust", "legacy" };
    static void OwnedContentDiagnostics()
    {
        ProbeFiles.Require(ContentDiagnosticsOwned, "OWNED_CONTENT_DIAGNOSTIC_MODE");
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type owner = type("IContentProjectStoreFixture"), typed = type("ContentTypedFixture"); MethodInfo getter = owner.GetMethod("get_Project");
        Type contract = ContentStoreContract(owner, getter, false); string selected = ownedContentDiagnosticCase;
        if (selected == "trust") {
            ProbeFiles.Require(ProjectContentDenyProxy.ContentMethodDescriptor((MethodInfo)Activator.CreateInstance(type("CacheCustomMethodFixture")), contract) == "category=UNTRUSTED_METHOD_KIND"
                && ProjectContentDenyProxy.ContentMethodDescriptor(null, contract) == "category=MEMBER_UNAVAILABLE"
                && ProjectContentDenyProxy.ContentMethodDescriptor(typeof(string).GetMethod("ToString", Type.EmptyTypes), contract).Contains("qualification=UNVERIFIED_CONTRACT_MEMBER declaring-type=System.String ")
                && ProjectContentDenyProxy.ContentMethodDescriptor(contract.GetMethod("get_Unknown"), contract).Contains("name=get_Unknown"), "OWNED_CONTENT_DIAGNOSTIC_TRUST");
            foreach (MethodInfo core in new MethodInfo[] { typeof(object).GetMethod("GetType"), typeof(object).GetMethod("ToString"), typeof(object).GetMethod("GetHashCode"), typeof(object).GetMethod("Equals", new Type[] { typeof(object) }), typeof(IDisposable).GetMethod("Dispose") })
                ProbeFiles.Require(ProjectContentDenyProxy.ContentMethodDescriptor(core, contract).Contains("category=TRUSTED_CORE_MEMBER") && ProjectContentDenyProxy.ContentMethodDescriptor(core, contract).Contains("name=" + core.Name), "OWNED_CONTENT_DIAGNOSTIC_CORE");
            string foreign = ProjectContentDenyProxy.ContentMethodDescriptor(typeof(string).GetMethod("ToString", Type.EmptyTypes), contract);
            string generic = ProjectContentDenyProxy.ContentMethodDescriptor(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), contract);
            ProbeFiles.Require(foreign.Contains("module-assembly=" + Text(typeof(string).Assembly.FullName)) && foreign.Contains("name=ToString") && foreign.Length <= 4096
                && generic.Contains("declaring-type=System.Collections.Generic.List`1 generic-instantiation=NOT_INSPECTED ") && !generic.Contains("System.String") && !generic.Contains("[["), "OWNED_FOREIGN_CONTENT_DIRECT");
            string captured = null; int metadata = 0; ProjectContentDenyProxy inherited = null;
            inherited = new ProjectContentDenyProxy(type("IContextParentFixture"), delegate(string line) {
                if (line.StartsWith("PROJECT_CONTENT_REFUSAL ", StringComparison.Ordinal)) { metadata++; captured = line; ProbeFiles.Require(inherited.State.Faulted && UndoFactoryCallbackExcluded(), "OWNED_FOREIGN_DENY_FIRST"); }
                Record(line);
            }, true);
            Type control = type("ContextOwnedControl"); int caught = (int)control.GetField("Caught").GetValue(null);
            control.GetField("Parent").SetValue(null, inherited.GetTransparentProxy());
            WithProjectResourceBudget(delegate { WithProjectResourceWindow(2, delegate { control.GetMethod("ParentInherited").Invoke(null, null); }); });
            ProbeFiles.Require(metadata == 1 && inherited.State.Entries == 1 && inherited.State.Faulted && (int)control.GetField("Caught").GetValue(null) == caught + 1
                && captured != null && captured.Contains("category=FOREIGN_OWNER qualification=UNVERIFIED_CONTRACT_MEMBER declaring-type=Owned.Task009.IContextParentBaseFixture ")
                && captured.Contains("token=" + type("IContextParentBaseFixture").GetMethod("get_DisplayName").MetadataToken.ToString("X8")) && captured.Contains("name=get_DisplayName"), "OWNED_FOREIGN_CONTENT_TYPED");
            Record("OWNED_FOREIGN_CONTENT_PASS metadata=1 denied=1 typed-inherited=True qualification=UNVERIFIED_CONTRACT_MEMBER caller-origin=UNKNOWN");
            Record("OWNED_CONTENT_DIAGNOSTIC_PASS case=trust descriptor-checks=10 typed-inherited=1 no-runtime-caller-proof=True"); return;
        }
        int attempts = 0, emitted = 0; string descriptor = null; bool nested = false;
        Type poisonType = type("ContextReentryMessageFixture"); object poison = Activator.CreateInstance(poisonType);
        Action<string> recorder = delegate(string line) {
            if (line.StartsWith("PROJECT_CONTENT_REFUSAL ", StringComparison.Ordinal)) {
                attempts++; descriptor = line; ProbeFiles.Require(activeContent.Proxy.State.Faulted && UndoFactoryCallbackExcluded(), "OWNED_CONTENT_DIAGNOSTIC_DENY_FIRST");
                if (selected == "recorder") throw new IOException("OWNED_CONTENT_DIAGNOSTIC_RECORDER");
                if (selected == "reentry" && !nested) { nested = true; try { activeContent.Proxy.Invoke((IMessage)poison); } catch (NotSupportedException) { } ProbeFiles.Require(UndoFactoryCallbackExcluded(), "OWNED_CONTENT_DIAGNOSTIC_OUTER_EXCLUSION"); }
                emitted++;
            }
            Record(line);
        };
        activeContent = selected == "legacy" ? new ProjectContentReference(getter, contract, recorder) : new ProjectContentReference(getter, contract, recorder, true);
        WithContentLifetime(delegate { WithProjectResourceBudget(delegate { WithProjectResourceWindow(2, delegate {
            Action typedCall = delegate { typed.GetMethod(selected == "object" ? "ObjectType" : selected == "dispose" ? "Dispose" : "Callback").Invoke(null, new object[] { activeContent.Reference }); };
            int calls = selected == "cap" ? 20 : selected == "legacy" ? 3 : selected == "first-only" ? 2 : 1;
            for (int i = 0; i < calls; i++) { bool refused = false; try { if (selected == "cap" || selected == "legacy") activeContent.Proxy.Invoke((IMessage)poison); else typedCall(); } catch (Exception) { refused = true; } ProbeFiles.Require(refused, "OWNED_CONTENT_DIAGNOSTIC_REFUSAL_REQUIRED"); }
            int entries = selected == "cap" ? 17 : selected == "legacy" ? 3 : selected == "first-only" || selected == "reentry" ? 2 : 1;
            ProbeFiles.Require(activeContent.Proxy.State.Entries == entries && activeContent.Proxy.State.Faulted && activeContent.Attempts == 0 && activeContent.Replies == 0
                && attempts == (selected == "legacy" ? 0 : 1) && emitted == (selected == "legacy" || selected == "recorder" ? 0 : 1)
                && (int)poisonType.GetField("Reads").GetValue(poison) == 0 && !UndoFactoryCallbackExcluded(), "OWNED_CONTENT_DIAGNOSTIC_BOUNDS");
            if (selected != "legacy") {
                string expected = selected == "cap" ? "category=UNTRUSTED_MESSAGE_TYPE" : selected == "object" ? "name=GetType" : selected == "dispose" ? "name=Dispose" : "name=get_Unknown";
                ProbeFiles.Require(descriptor != null && descriptor.Contains(expected), "OWNED_CONTENT_DIAGNOSTIC_IDENTITY");
            }
            Record("OWNED_CONTENT_DIAGNOSTIC_PASS case=" + selected + " metadata-attempts=" + attempts + " emitted=" + emitted + " denied=" + entries + " message-getter-reads=0 typed-call=" + (selected != "cap" && selected != "legacy") + " caller-origin=UNKNOWN");
        }); }); });
    }

    static void OwnedExistsFixture()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type owner = type("IEmptyProjectStoreFixture"), contract = type("IEmptyContentFixture"), streamType = type("ICacheStreamFixture"), typed = type("EmptyContentTypedFixture");
        MethodInfo getter = owner.GetMethod("get_Project"); MethodInfo[] methods = StreamContract(streamType, false);
        string selected = ownedExistsCase ?? "positive";
        if (selected == "shape") {
            int refused = 0;
            foreach (Action test in new Action[] {
                delegate { ExistsContract(type("IContentStoreFixture"), streamType, methods[1], false); },
                delegate { ExistsContract(contract, streamType, methods[0], false); },
                delegate { ExistsContract(new TypeDelegator(contract), streamType, methods[1], false); },
                delegate { ExistsContract(contract, streamType, (MethodInfo)Activator.CreateInstance(type("CacheCustomMethodFixture")), false); },
                delegate { ExistsContract(contract, streamType, methods[1], true); } }) {
                try { test(); } catch (IOException) { refused++; }
            }
            ProbeFiles.Require(refused == 5 && ExistsContract(contract, streamType, methods[1], false) == methods[1] && activeExists == null && activeContent == null, "OWNED_EXISTS_SHAPE");
            Record("OWNED_EXISTS_CASE_PASS case=shape negatives=5 assignability=True before-effects=True actual-SDK-assignability=NOT_RUN"); return;
        }
        if (selected == "audit") {
            int refused = 0;
            for (int i = 0; i < 5; i++) {
                try { ExistsFinalAudit(i == 0 ? 17 : 0, i == 1 ? 2 : 0, 0, i == 2, i == 3, true, 2, "OWNED_AUDIT"); if (i == 4) ExistsFinalAudit(0, 0, 2, false, false, true, 2, "OWNED_AUDIT"); }
                catch (NotSupportedException) { refused++; }
            }
            for (int i = 0; i < 2; i++) {
                var state = new CacheReferenceState(); var policy = new ContentExistsPolicy(methods[1]); var reference = new ProjectContentReference(getter, contract, Record, policy);
                policy.Bind(state, reference); state.Begin(); reference.Begin(); policy.Begin(); state.Enter(state.Count(), false); policy.Attempt();
                if (i == 0) state.Fail(); state.ExitExists(true, true, policy); if (i == 1) state.Fail();
                ProbeFiles.Require(policy.Replies == i && !policy.Faulted && state.Faulted && reference.Replies == 0, "OWNED_EXISTS_COMPLETION");
                policy.Close(); reference.Close(); state.Close(); GC.KeepAlive(reference.Reference);
            }
            ProbeFiles.Require(refused == 5 && activeExists == null, "OWNED_EXISTS_INDEPENDENT_AUDITS");
            Record("OWNED_EXISTS_CASE_PASS case=audit independent-count-and-fault=True completion-before=0 completion-after=1"); return;
        }
        Type idType = type("ScopedIdFixture"), poisonType = type("ContextReentryMessageFixture"); object poison = Activator.CreateInstance(poisonType);
        var stream = new StreamStoreDenyProxy(streamType, methods, Record);
        object id = CreateFixtureId(idType, idType.GetConstructor(new Type[] { typeof(uint) }), idType.GetMethod("get_ProjectNumber"));
        bool nested = false; string descriptor = null;
        Action<string> recorder = delegate(string line) {
            if (line.StartsWith("PROJECT_CONTENT_REFUSAL ", StringComparison.Ordinal)) descriptor = line;
            if (line.StartsWith("CONTENT_EXISTS_FIXTURE ", StringComparison.Ordinal)) {
                ProbeFiles.Require(UndoFactoryCallbackExcluded(), "OWNED_EXISTS_RECORD_EXCLUDED");
                if (selected == "recorder") throw new IOException("OWNED_EXISTS_RECORDER");
                if (selected == "restore") projectResourceCallbackSuspended = false;
                if (selected == "nested" && !nested) { nested = true; try { activeContent.Proxy.Invoke((IMessage)poison); } catch (Exception) { } }
            }
            Record(line);
        };
        PrepareExistsStore(owner, getter, streamType, methods[1], false, recorder);
        var store = new LargeStringCacheStoreProxy(owner, owner.GetMethod("get_Id"), id, owner.GetMethod("get_LargeStringCache"), streamType, stream.GetTransparentProxy(), recorder, activeContent);
        activeExists.Bind(store.State, activeContent);
        object receiver = activeContent.Reference;
        Action referenceCall = delegate { ProbeFiles.Require(Object.ReferenceEquals(typed.GetMethod("Reference").Invoke(null, new object[] { store.GetTransparentProxy() }), receiver), "OWNED_EXISTS_REFERENCE"); };
        Action falseCall = delegate { ProbeFiles.Require(Object.Equals(typed.GetMethod("Exists").Invoke(null, new object[] { receiver }), false), "OWNED_EXISTS_TYPED_FALSE"); };
        Action run = delegate {
            bool caught = false;
            try {
                if (selected == "positive") {
                    ProbeFiles.Require(Object.ReferenceEquals(typed.GetMethod("Id").Invoke(null, new object[] { store.GetTransparentProxy() }), id)
                        && Object.ReferenceEquals(typed.GetMethod("Cache").Invoke(null, new object[] { store.GetTransparentProxy() }), stream.GetTransparentProxy()), "OWNED_EXISTS_ID_CACHE"); falseCall();
                } else if (selected == "second") { falseCall(); falseCall(); }
                else if (selected == "next-denial") { falseCall(); typed.GetMethod("Other").Invoke(null, new object[] { receiver }); }
                else if (selected == "wrong-receiver") typed.GetMethod("Exists").Invoke(null, new object[] { stream.GetTransparentProxy() });
                else if (selected == "malformed") activeContent.Proxy.Invoke((IMessage)poison);
                else if (selected == "off-thread") { Exception error = null; var thread = new Thread(new ThreadStart(delegate { try { falseCall(); } catch (Exception e) { error = e; } })); thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000) && error != null, "OWNED_EXISTS_OFF_THREAD"); caught = true; }
                else if (selected == "budget") {
                    for (int i = 0; i < 15; i++) typed.GetMethod("Id").Invoke(null, new object[] { store.GetTransparentProxy() });
                    activeContent.Proxy.Invoke((IMessage)poison);
                } else falseCall();
            } catch (Exception) { caught = true; }
            if (selected == "positive") ProbeFiles.Require(!caught && store.State.Entries == 4 && store.State.Attempts == 1 && store.State.Replies == 1 && !store.State.Faulted
                && activeContent.Replies == 1 && activeContent.Proxy.State.Entries == 0 && activeExists.Attempts == 1 && activeExists.Replies == 1 && !activeExists.Faulted && stream.Requests == 0, "OWNED_EXISTS_POSITIVE_COUNTS");
            else if (selected == "wrong-receiver") ProbeFiles.Require(caught && stream.Requests == 1 && activeExists.Attempts == 0 && activeExists.Replies == 0, "OWNED_EXISTS_CACHE_UNCHANGED");
            else {
                ProbeFiles.Require(caught && store.State.Faulted && activeExists.Faulted && activeContent.Proxy.State.Entries != 0 && activeContent.Proxy.State.Faulted, "OWNED_EXISTS_EXPECTED_REFUSAL");
                ProbeFiles.Require(activeExists.Replies == (selected == "second" || selected == "next-denial" ? 1 : 0), "OWNED_EXISTS_NO_CAUGHT_SUCCESS");
                if (selected == "next-denial") ProbeFiles.Require(descriptor != null && descriptor.Contains("name=get_Id") && descriptor.Contains("denial=1"), "OWNED_EXISTS_FIRST_DENIAL_AFTER_FALSE");
                if (selected == "budget") ProbeFiles.Require(store.State.Entries == 17 && activeExists.Attempts == 0, "OWNED_EXISTS_AGGREGATE_CAP");
            }
            ProbeFiles.Require((int)poisonType.GetField("Reads").GetValue(poison) == 0 && !UndoFactoryCallbackExcluded(), "OWNED_EXISTS_EXCLUSION_RESTORED");
            Record("OWNED_EXISTS_CASE_PASS case=" + selected + " aggregate=" + store.State.Entries + " attempts=" + activeExists.Attempts + " replies=" + activeExists.Replies + " fault=" + activeExists.Faulted + " denials=" + activeContent.Proxy.State.Entries + " stream-denied=" + stream.Requests + " getter-reads=0 typed-ABI=" + (selected != "malformed" && selected != "budget") + " fixture-only=True receipt=UNKNOWN");
        };
        WithContentLifetime(delegate { WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate {
            WithProjectResourceWindow(2, referenceCall);
            if (selected == "late") activeExists.Close();
            if (selected == "outside") run(); else WithProjectResourceWindow(2, run);
        }); }); });
    }

    static ProjectGuidFixture activeGuid;
    static ProjectGuidFixture GuidContract(Type owner, bool original)
    {
        ProbeFiles.Require(GuidMode && owner != null && owner.GetType() == typeof(object).GetType() && PublicInterface(owner), "GUID_OWNER");
        MethodInfo get = owner.GetMethod("get_ProjectGuid", BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        MethodInfo set = owner.GetMethod("set_ProjectGuid", BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        foreach (MethodInfo m in new MethodInfo[] { get, set }) ProbeFiles.Require(m != null && m.GetType() == typeof(object).GetMethod("GetType").GetType() && m.Module == owner.Module && m.DeclaringType == owner && !m.IsStatic && m.IsPublic && m.IsSpecialName && !m.IsGenericMethod && !m.ContainsGenericParameters && (int)m.Attributes == 3526 && m.CallingConvention == (CallingConventions.Standard|CallingConventions.HasThis), "GUID_METHOD_PROFILE");
        ParameterInfo[] parameters = set.GetParameters();
        ProbeFiles.Require(get.ReturnType == typeof(Guid) && get.GetParameters().Length == 0 && set.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Guid) && !parameters[0].IsOut, "GUID_EXACT_ABI");
        if (original) ProbeFiles.Require(GuidOriginal && owner.MetadataToken == 0x02000057 && owner.Assembly.FullName == OmIdentity && ProbeFiles.Same(owner.Assembly.Location, Path.Combine(stage,"Knx.Ets.ObjectModel.dll")) && get.MetadataToken == 0x060002f6 && set.MetadataToken == 0x060002f7, "GUID_ORIGINAL_PIN");
        return new ProjectGuidFixture(get,set);
    }
    static void OwnedGuidFixture()
    {
        for (int i = 0; i < GuidExtraFiles.Length; i++) {
            string path = Path.Combine(stage, GuidExtraFiles[i]); string identity = GuidExtraIdentities[i];
            ProbeFiles.Require(GuidExtraResolvePolicy("original-project-guid-fixture", true, identity, identity, i)
                && !GuidExtraResolvePolicy("original-project-content-store-exists-fixture", true, identity, identity, i)
                && !GuidExtraResolvePolicy("original-project-guid-fixture", false, identity, identity, i)
                && !GuidExtraResolvePolicy("original-project-guid-fixture", true, identity + ".resources", identity, i)
                && GuidExtraReferencePolicy("original-project-guid-fixture", true, identity, path, false, false, false, stage, i)
                && !GuidExtraReferencePolicy("original-project-guid-fixture", true, identity, path, true, false, false, stage, i), "OWNED_GUID_EXTRA_FIXED_POLICY");
        }
        Record("OWNED_GUID_EXTRA_POLICY_PASS entries=12 predicate-only=True actual-DLLs=NOT_RUN");
        Assembly a=Assembly.LoadFrom(fixture); Func<string,Type> type=delegate(string n){return a.GetType("Owned.Task009."+n,true);};
        Type owner=type("IGuidProjectStoreFixture"), streamType=type("ICacheStreamFixture"), idType=type("ScopedIdFixture"), typed=type("GuidTypedFixture");
        MethodInfo[] methods=StreamContract(streamType,false); var stream=new StreamStoreDenyProxy(streamType,methods,Record);
        PrepareExistsStore(owner,owner.GetMethod("get_Project"),streamType,methods[1],false,Record); activeGuid=GuidContract(owner,false);
        object id=CreateFixtureId(idType,idType.GetConstructor(new Type[]{typeof(uint)}),idType.GetMethod("get_ProjectNumber"));
        var store=new LargeStringCacheStoreProxy(owner,owner.GetMethod("get_Id"),id,owner.GetMethod("get_LargeStringCache"),streamType,stream.GetTransparentProxy(),Record,activeContent,activeGuid);
        activeExists.Bind(store.State,activeContent);
        WithContentLifetime(delegate { WithCacheLifetime(store,stream,delegate { WithProjectResourceBudget(delegate { WithProjectResourceWindow(2,delegate {
            object proxy=store.GetTransparentProxy();typed.GetMethod("RoundTrip").Invoke(null,new object[]{proxy});
            ProbeFiles.Require(activeGuid.Current==Guid.Empty && !activeGuid.Faulted && !store.State.Faulted,"OWNED_GUID_ROUNDTRIP");
            Record("OWNED_GUID_ROUNDTRIP_PASS initial-empty=True repeated-read-write=True copied=True fixture-only=True values=NOT_LOGGED");
            string descriptor = null; var deniedContext = new ContextArgumentDenyProxy(type("IContextArgumentFixture"), "owned-smoke", delegate(string line) { if (line.StartsWith("CONTEXT_ARGUMENT_REFUSAL ", StringComparison.Ordinal)) descriptor = line; Record(line); });
            Type control = type("ContextOwnedControl"); control.GetField("Context").SetValue(null, deniedContext.GetTransparentProxy()); control.GetMethod("ContextProject").Invoke(null, new object[0]);
            ProbeFiles.Require(deniedContext.State.Entries == 1 && deniedContext.State.Faulted && descriptor != null && descriptor.Contains("name=get_Project"), "OWNED_CONTEXT_DESCRIPTOR_SMOKE");
            Record("OWNED_CONTEXT_DESCRIPTOR_SMOKE_PASS denied=1 method=get_Project values=NOT_INSPECTED"); GC.KeepAlive(deniedContext);
            OwnedValidatorSmoke(a);
            try { typed.GetMethod("Unsupported").Invoke(null,new object[]{proxy}); } catch (TargetInvocationException) { }
            ProbeFiles.Require(store.State.Faulted,"OWNED_GUID_UNKNOWN_REFUSED");Record("OWNED_GUID_UNSUPPORTED_PASS fatal-final-audit=True");
        }); }); }); });
    }

    static ContentExistsPolicy activeExists;
    static string ownedExistsCase;
    static readonly string[] OwnedExistsCases = { "positive", "second", "next-denial", "wrong-receiver", "shape", "outside", "off-thread", "nested", "late", "malformed", "recorder", "restore", "budget", "audit" };
    internal static MethodInfo ExistsContract(Type content, Type stream, MethodInfo getter, bool original)
    {
        ProbeFiles.Require(ExistsMode && content != null && stream != null && content.GetType() == typeof(object).GetType() && stream.GetType() == typeof(object).GetType()
            && PublicInterface(content) && PublicInterface(stream) && content.Module == stream.Module && !stream.Assembly.ReflectionOnly, "EXISTS_RUNTIME_TYPES");
        ProbeFiles.Require(stream.IsAssignableFrom(content), "EXISTS_INCOMPATIBLE_ASSIGNABILITY");
        ProbeFiles.Require(getter != null && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(getter) && getter.GetType() == typeof(object).GetMethod("GetType").GetType()
            && getter.DeclaringType == stream && getter.Module == stream.Module && getter.Name == "get_Exists" && getter.IsPublic && !getter.IsStatic && getter.IsSpecialName
            && !getter.IsGenericMethod && !getter.ContainsGenericParameters && getter.GetParameters().Length == 0 && getter.ReturnType == typeof(bool)
            && (int)getter.Attributes == 3526 && getter.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "EXISTS_EXACT_BOOLEAN_GETTER");
        if (original) ProbeFiles.Require(ExistsOriginal && containerInputsReady && xmlRegistrationReady && content.FullName == "Knx.Ets.ObjectModel.IStore" && content.MetadataToken == 0x0200005e
            && stream.FullName == "Knx.Ets.ObjectModel.IStreamStore" && stream.MetadataToken == 0x02000061 && (int)content.Attributes == 1048737 && (int)stream.Attributes == 1048737
            && getter.MetadataToken == 0x06000367 && stream.Assembly.FullName == OmIdentity && ProbeFiles.Same(stream.Assembly.Location, Path.Combine(stage, "Knx.Ets.ObjectModel.dll")), "EXISTS_ORIGINAL_PINNED_CONTRACT");
        return getter;
    }
    static void PrepareExistsStore(Type owner, MethodInfo referenceGetter, Type stream, MethodInfo existsGetter, bool original, Action<string> recorder)
    {
        Type contract = ContentStoreContract(owner, referenceGetter, original);
        MethodInfo method = ExistsContract(contract, stream, existsGetter, original);
        ProbeFiles.Require(activeContent == null && activeExists == null, "EXISTS_PREPARE_ONCE");
        activeExists = new ContentExistsPolicy(method); activeContent = new ProjectContentReference(referenceGetter, contract, recorder, activeExists);
        Record("CONTENT_EXISTS_CONTRACT_READY assignability=True exact-boolean-getter=" + method.MetadataToken.ToString("X8") + " fixture-only=True real-store-state=UNKNOWN");
    }
    internal static void ExistsFinalAudit(int entries, int attempts, int replies, bool fault, bool sharedFault, bool bound, int lifetime, string phase)
    {
        bool bad = entries > ProductStoreFinalEntryLimit() || attempts > 1 || replies > 1 || replies > attempts || fault || sharedFault
            || ((phase == "PROCESS_FINAL" || phase == "AFTER_ALL_MANAGED_CLEANUP") && (!bound || lifetime != 2));
        try { if(bad||InitializeAuditLog(phase))Record("CONTENT_EXISTS_AUDIT phase=" + phase + " aggregate-entries=" + entries + " attempts=" + attempts + " replies=" + replies + " fault=" + fault + " shared-fault=" + sharedFault + " bound=" + bound + " lifetime=" + lifetime + " value=False fixture-only=True"); }
        finally { if (bad) throw new NotSupportedException("CONTENT_EXISTS_FINAL_DENIAL"); }
    }
    static void CheckExists(string phase)
    {
        if (!ExistsMode || activeExists == null) return;
        bool bound = activeExists.Bound;
        ExistsFinalAudit(bound ? activeExists.Shared.Entries : 0, activeExists.Attempts, activeExists.Replies, activeExists.Faulted,
            bound && activeExists.Shared.Faulted, bound, activeExists.Lifetime, phase);
    }

    static ProjectContentReference activeContent;
    static string ownedContentCase;
    static readonly string[] OwnedContentCases = { "positive", "shape", "second", "outside", "off-thread", "nested", "late", "callback", "object", "dispose", "saturated", "malformed", "recorder", "restore", "audit" };
    internal static Type ContentStoreContract(Type owner, MethodInfo method, bool original)
    {
        ProbeFiles.Require(ContentMode && owner != null && owner.GetType() == typeof(object).GetType() && PublicInterface(owner)
            && method != null && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(method) && method.GetType() == typeof(object).GetMethod("GetType").GetType(), "CONTENT_TRUSTED_GETTER");
        ProbeFiles.Require(method.DeclaringType == owner && method.Module == owner.Module && method.Name == "get_Project" && method.IsPublic && !method.IsStatic && method.IsSpecialName
            && !method.IsGenericMethod && !method.ContainsGenericParameters && method.GetParameters().Length == 0 && (int)method.Attributes == 3526
            && method.CallingConvention == (CallingConventions.Standard | CallingConventions.HasThis), "CONTENT_GETTER_SIGNATURE");
        Type contract = method.ReturnType;
        ProbeFiles.Require(contract != null && contract.GetType() == typeof(object).GetType() && PublicInterface(contract) && contract.DeclaringType == null
            && !contract.IsGenericType && !contract.ContainsGenericParameters && contract.Module == owner.Module && contract.Assembly == owner.Assembly && !contract.Assembly.ReflectionOnly, "CONTENT_RETURN_INTERFACE");
        if (original) ProbeFiles.Require(ContentOriginal && containerInputsReady && xmlRegistrationReady && owner.FullName == "Knx.Ets.ObjectModel.IProjectStore" && owner.MetadataToken == 0x02000057
            && (int)owner.Attributes == 1048737 && method.MetadataToken == 0x060002f9 && contract.FullName == "Knx.Ets.ObjectModel.IStore" && owner.Assembly.FullName == OmIdentity
            && ProbeFiles.Same(owner.Assembly.Location, Path.Combine(stage, "Knx.Ets.ObjectModel.dll")), "CONTENT_ORIGINAL_PINNED_RETURN");
        return contract;
    }
    static void PrepareContentStore(Type owner, MethodInfo method, bool original, Action<string> recorder)
    {
        Type contract = ContentStoreContract(owner, method, original);
        ProbeFiles.Require(activeContent == null, "CONTENT_PREPARE_ONCE");
        if (ContentDiagnosticsOriginal) ProbeFiles.Require(contract.MetadataToken == 0x0200005e && (int)contract.Attributes == 1048737, "CONTENT_DIAGNOSTIC_OBSERVED_ROOT");
        if (ContentDiagnosticsMode) activeContent = new ProjectContentReference(method, contract, recorder, true);
        else activeContent = new ProjectContentReference(method, contract, recorder);
        Record("PROJECT_CONTENT_SHAPE_READY owner=" + Text(owner.FullName) + " getter=" + method.MetadataToken.ToString("X8") + " return=" + Text(contract.FullName)
            + " return-token=" + contract.MetadataToken.ToString("X8") + " return-flags=" + (int)contract.Attributes + " public-interface=True generic=False behavior=DENY_ONLY");
    }
    internal static void ContentFinalAudit(int attempts, int replies, bool fault, int denied, bool denyFault, int lifetime, string phase)
    {
        int referenceLimit=ProductFixtureMode()?16:1;
        bool bad = attempts > referenceLimit || replies > referenceLimit || replies > attempts || fault || denied != 0 || denyFault
            || ((phase == "PROCESS_FINAL" || phase == "AFTER_ALL_MANAGED_CLEANUP") && lifetime != 2);
        try { if(bad||InitializeAuditLog(phase))Record("PROJECT_CONTENT_AUDIT phase=" + phase + " attempts=" + attempts + " replies=" + replies + " fault=" + fault + " denied=" + denied + " deny-fault=" + denyFault + " lifetime=" + lifetime + " kind=CONTROLLED_DENIAL_REFERENCE"); }
        finally { if (bad) throw new NotSupportedException("PROJECT_CONTENT_FINAL_DENIAL"); }
    }
    static void CheckContentStore(string phase)
    {
        if (ExistsMode) {
            try { if (GuidMode && activeGuid != null) activeGuid.Audit(phase, Record); }
            finally { try { CheckExists(phase); } finally { if (activeContent != null) ContentFinalAudit(activeContent.Attempts, activeContent.Replies, activeContent.Faulted, activeContent.Proxy.State.Entries, activeContent.Proxy.State.Faulted, activeContent.Lifetime, phase); } }
            return;
        }
        if (!ContentMode || activeContent == null) return;
        ContentFinalAudit(activeContent.Attempts, activeContent.Replies, activeContent.Faulted, activeContent.Proxy.State.Entries, activeContent.Proxy.State.Faulted, activeContent.Lifetime, phase);
    }
    static void WithContentLifetime(Action run)
    {
        ProbeFiles.Require(ContentMode && activeContent != null, "CONTENT_LIFETIME_CONTEXT");
        activeContent.Begin();
        if (ExistsMode) activeExists.Begin();
        try { run(); }
        finally {
            activeContent.Close();
            if (ExistsMode) activeExists.Close();
            try { CheckContentStore("AFTER_ALL_MANAGED_CLEANUP"); }
            finally { GC.KeepAlive(activeContent); GC.KeepAlive(activeContent.Reference); GC.KeepAlive(activeContent.Proxy); }
        }
    }

    static void OwnedContentCompletionRegression(Type owner, MethodInfo getter)
    {
        ProbeFiles.Require(ContentOwned && ownedContentCase == "audit", "OWNED_CONTENT_COMPLETION_CASE");
        Type contract = ContentStoreContract(owner, getter, false);
        for (int i = 0; i < 2; i++) {
            var shared = new CacheReferenceState(); var policy = new ProjectContentReference(getter, contract, Record);
            shared.Begin(); shared.Enter(shared.Count(), false); policy.Begin(); policy.Attempt(true);
            if (i == 0) shared.Fail(); // Controlled injection after the response guard, before completion.
            shared.ExitContent(true, false, true, policy);
            if (i == 1) shared.Fail(); // Later faults retain the already accepted historical reply.
            ProbeFiles.Require(shared.Faulted && policy.Replies == i && !policy.Faulted && policy.Proxy.State.Entries == 0 && !policy.Proxy.State.Faulted,
                "OWNED_CONTENT_SHARED_COMPLETION");
            shared.Close(); policy.Close(); GC.KeepAlive(policy.Reference);
        }
        Record("OWNED_CONTENT_COMPLETION_PASS before-fault-replies=0 later-fault-replies=1 own-faults=False proxy-denials=0 controlled-state-injection=True");
    }

    static void OwnedContentCasesRun()
    {
        Assembly owned = Assembly.LoadFrom(fixture); Func<string, Type> type = delegate(string name) { return owned.GetType("Owned.Task009." + name, true); };
        Type owner = type("IContentProjectStoreFixture"), typed = type("ContentTypedFixture"); MethodInfo getter = owner.GetMethod("get_Project");
        if (ownedContentCase == "shape") {
            int refused = 0;
            foreach (Action test in new Action[] {
                delegate { ContentStoreContract(owner, null, false); }, delegate { ContentStoreContract(new TypeDelegator(owner), getter, false); },
                delegate { ContentStoreContract(owner, (MethodInfo)Activator.CreateInstance(type("CacheCustomMethodFixture")), false); },
                delegate { ContentStoreContract(owner, typeof(object).GetMethod("ToString"), false); },
                delegate { Type bad = type("IContentClassStoreFixture"); ContentStoreContract(bad, bad.GetMethod("get_Project"), false); },
                delegate { Type bad = type("IContentGenericStoreFixture"); ContentStoreContract(bad, bad.GetMethod("get_Project"), false); },
                delegate { Type bad = type("IContentNestedStoreFixture"); ContentStoreContract(bad, bad.GetMethod("get_Project"), false); },
                delegate { ContentStoreContract(owner, getter, true); } }) {
                try { test(); } catch (IOException) { refused++; }
            }
            ProbeFiles.Require(refused == 8 && activeContent == null && ContentStoreContract(owner, getter, false) == type("IContentStoreFixture"), "OWNED_CONTENT_SHAPES");
            Record("OWNED_CONTENT_CASE_PASS case=shape negatives=8 before-effects=True actual-SDK-return-shape=NOT_RUN"); return;
        }
        if (ownedContentCase == "audit") {
            OwnedContentCompletionRegression(owner, getter);
            int refused = 0;
            for (int i = 0; i < 4; i++) { try { ContentFinalAudit(i == 0 ? 2 : 0, 0, i == 1, i == 2 ? 1 : 0, i == 3, 2, "OWNED_AUDIT_NEGATIVE"); } catch (NotSupportedException) { refused++; } }
            ProbeFiles.Require(refused == 4 && activeContent == null, "OWNED_CONTENT_INDEPENDENT_AUDITS");
            Record("OWNED_CONTENT_CASE_PASS case=audit count-only=2 fault-only=2 otherwise-clean=True"); return;
        }
        Type streamType = type("ICacheStreamFixture"), idType = type("ScopedIdFixture"), messageType = type("StoreDiagnosticMessageFixture");
        var stream = new StreamStoreDenyProxy(streamType, StreamContract(streamType, false), Record);
        object id = CreateFixtureId(idType, idType.GetConstructor(new Type[] { typeof(uint) }), idType.GetMethod("get_ProjectNumber"));
        LargeStringCacheStoreProxy store = null; bool nested = false;
        Action<string> recorder = delegate(string line) {
            if (line.StartsWith("PROJECT_CONTENT_REFERENCE ", StringComparison.Ordinal)) {
                ProbeFiles.Require(UndoFactoryCallbackExcluded(), "OWNED_CONTENT_RECORD_EXCLUDED");
                if (ownedContentCase == "recorder") throw new IOException("OWNED_CONTENT_RECORDER");
                if (ownedContentCase == "restore") projectResourceCallbackSuspended = false;
                if (ownedContentCase == "nested" && !nested) { nested = true; object raw = Activator.CreateInstance(messageType); messageType.GetField("Method").SetValue(raw, getter); messageType.GetField("Name").SetValue(raw, "get_Project"); try { store.Invoke((IMessage)raw); } catch (Exception) { } }
            }
            Record(line);
        };
        PrepareContentStore(owner, getter, false, recorder);
        store = new LargeStringCacheStoreProxy(owner, owner.GetMethod("get_Id"), id, owner.GetMethod("get_LargeStringCache"), streamType, stream.GetTransparentProxy(), recorder, activeContent);
        Action reference = delegate { typed.GetMethod("Reference").Invoke(null, new object[] { store.GetTransparentProxy() }); };
        Action run = delegate {
            string selected = ownedContentCase; bool caught = false;
            try {
                if (selected == "positive") {
                    object value = typed.GetMethod("Reference").Invoke(null, new object[] { store.GetTransparentProxy() });
                    ProbeFiles.Require(Object.ReferenceEquals(value, activeContent.Reference) && Object.ReferenceEquals(typed.GetMethod("Id").Invoke(null, new object[] { store.GetTransparentProxy() }), id)
                        && Object.ReferenceEquals(typed.GetMethod("Cache").Invoke(null, new object[] { store.GetTransparentProxy() }), stream.GetTransparentProxy()), "OWNED_CONTENT_TYPED_REFERENCE");
                    ProbeFiles.Require(activeContent.Attempts == 1 && activeContent.Replies == 1 && !activeContent.Faulted && store.State.Attempts == 1 && store.State.Replies == 1 && store.State.Entries == 3 && !store.State.Faulted, "OWNED_CONTENT_SEPARATE_COUNTS");
                } else if (selected == "second") { reference(); reference(); }
                else if (selected == "off-thread") { Exception error = null; var thread = new Thread(new ThreadStart(delegate { try { reference(); } catch (Exception e) { error = e; } })); thread.IsBackground = true; thread.Start(); ProbeFiles.Require(thread.Join(1000) && error != null, "OWNED_CONTENT_OFF_THREAD"); caught = true; }
                else if (selected == "callback" || selected == "object" || selected == "dispose") {
                    typed.GetMethod(selected == "callback" ? "Callback" : selected == "object" ? "ObjectType" : "Dispose").Invoke(null, new object[] { activeContent.Reference });
                } else if (selected == "saturated") {
                    Type poisonType = type("ContextReentryMessageFixture"); object poison = Activator.CreateInstance(poisonType);
                    for (int i = 0; i < 20; i++) { try { activeContent.Proxy.Invoke((IMessage)poison); } catch (NotSupportedException) { } }
                    ProbeFiles.Require(activeContent.Proxy.State.Entries == 17 && (int)poisonType.GetField("Reads").GetValue(poison) == 0, "OWNED_CONTENT_NO_MESSAGE_READS"); caught = true;
                } else if (selected == "malformed") {
                    object raw = Activator.CreateInstance(messageType); messageType.GetField("Method").SetValue(raw, getter); messageType.GetField("Name").SetValue(raw, "forged"); store.Invoke((IMessage)raw);
                } else reference();
            } catch (Exception) { caught = true; }
            ProbeFiles.Require(selected == "positive" ? !caught : caught && (store.State.Faulted || activeContent.Faulted || activeContent.Proxy.State.Entries != 0 || activeContent.Proxy.State.Faulted), "OWNED_CONTENT_EXPECTED_REFUSAL");
            Record("OWNED_CONTENT_CASE_PASS case=" + selected + " attempts=" + activeContent.Attempts + " replies=" + activeContent.Replies + " fault=" + activeContent.Faulted + " denied=" + activeContent.Proxy.State.Entries + " deny-fault=" + activeContent.Proxy.State.Faulted + " cache-replies=" + store.State.Replies + " typed-ABI=" + (selected == "positive" || selected == "callback" || selected == "object" || selected == "dispose"));
        };
        WithContentLifetime(delegate { WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate {
            if (ownedContentCase == "late") activeContent.Close();
            if (ownedContentCase == "outside") run(); else WithProjectResourceWindow(2, run);
        }); }); });
    }

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

    static void OwnedContentContextConstruction()
    {
        ProbeFiles.Require(ContentOwned, "OWNED_CONTENT_CONTEXT_MODE");
        contextOwnedAssembly = Assembly.LoadFrom(fixture); Func<string,Type> type = ContextOwnedType;
        Type project = type("ContentContextProjectFixture"), collection = type("ContentContextCollectionFixture"), parent = type("IContextParentFixture"), argument = type("IContextArgumentFixture"), factoryType = type("UndoFactoryDeliveryFixture"), contract = type("IFactoryFixture"), storeType = type("IContentProjectStoreFixture");
        Type[] importerTypes = new Type[] { storeType, project, type("IContextKeyFixture"), typeof(Func<string,string>), typeof(Func<,>).MakeGenericType(new Type[] { typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(new Type[] { type("ContextBaggageFixture") }), typeof(bool) }), typeof(Action<>).MakeGenericType(new Type[] { type("IContextImportInformationFixture") }), typeof(Predicate<>).MakeGenericType(new Type[] { type("IContextImportInformationFixture") }), type("ContextImporterFixture") };
        ContextConstructionApi api = ContextApi(project, collection, parent, argument, factoryType, contract, storeType, importerTypes, false);
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
        PrepareContentStore(storeType, storeType.GetMethod("get_Project"), false, Record);
        object idValue = CreateFixtureId(id, id.GetConstructor(new Type[] { typeof(uint) }), id.GetMethod("get_ProjectNumber"));
        var store = new LargeStringCacheStoreProxy(storeType, storeType.GetMethod("get_Id"), idValue, storeType.GetMethod("get_LargeStringCache"), streamType, stream.GetTransparentProxy(), Record, activeContent);
        var kim = new ProjectDenyProxy(type("IDataDependencyFixture"), Record); var undo = new UndoProviderDenyProxy(undoType, Record);
        var holder = new UndoProviderFactoryHolder(undoType, undo.GetTransparentProxy(), registration.CallbackType, Record);
        WithContentLifetime(delegate { WithContextLifetime(delegate { WithCacheLifetime(store, stream, delegate { WithProjectResourceBudget(delegate { WithUndoFactoryProvider(trace, container, traceRegistration, registration, factory, kim, store, undo, holder,
            delegate(object actual) { WithContextConstruction(api, actual, store); }); }); }); }); });
        ProbeFiles.Require(ownedContextCase == null && (int)control.GetField("Collections").GetValue(null) == 1 && (int)control.GetField("Projects").GetValue(null) == 1 && (int)control.GetField("Disposals").GetValue(null) == 1
            && store.State.Replies == 1 && store.State.Attempts == 1 && store.State.Entries == 4 && activeContent.Attempts == 1 && activeContent.Replies == 1 && !activeContent.Faulted && activeContent.Proxy.State.Entries == 0 && !activeContent.Proxy.State.Faulted && !store.State.Faulted && stream.Requests == 0 && store.DeniedRequests == 0 && holder.Entries == 1 && holder.Successes == 1 && !holder.Faulted
            && (int)undoControl.GetField("Registrations").GetValue(null) == 2 && (int)undoControl.GetField("ContainerDisposals").GetValue(null) == 1 && (int)traceControl.GetField("Disposals").GetValue(null) == 1, "OWNED_CONTEXT_LIFETIME");
        Record("OWNED_CONTENT_CONTEXT_PASS content-reference=1 content-denied=0 collection=1 project=1 project-dispose=1 collection-dispose=NOT_PROVIDED typed-arguments=True cache-reference=1 factory-success=1");
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
    [ThreadStatic] static bool sdkWrapperPhase;
    static int sdkOwner;
    static bool sdkProbeFault;static int sdkProbeAttempts;
    static object[] sdkRetained;
    static SdkMappingPolicy sdkMapping;
    static UndoFactoryRegistrationApi sdkKimApi;static UndoProviderFactoryHolder sdkKim;
    [ThreadStatic] static bool sdkRootSetupPhase;
    [ThreadStatic] static bool sdkModelRootConstructorActive;
    static SdkHostProxy sdkRootData,sdkUi;
    static UndoFactoryRegistrationApi sdkUiApi;static UndoProviderFactoryHolder sdkUiHolder;
    static object sdkModelRoot;
    sealed class SdkModelRootApi{internal Type Root,Collection;internal ConstructorInfo Constructor;internal MethodInfo Projects;}
    static SdkModelRootApi sdkModelRootApi;
    internal static bool SdkHostPhaseEligible()
    {return SdkMode&&Thread.CurrentThread.ManagedThreadId==sdkOwner&&(SdkOwned||contextLifetime)&&projectResourceBudgetActive&&!projectResourceCallbackSuspended&&(sdkRootSetupPhase||sdkWrapperPhase||guidProjectDisposal||projectResourceWindow==2);}
    static void PrepareSdkModelServices(Type root,Type collection,Type factory,Type dataStore,Type projectStore,Type usage,Type dispatcher,bool original)
    {
        ProbeFiles.Require(sdkRootData==null&&sdkUi==null&&PublicInterface(dataStore)&&PublicInterface(dispatcher),"SDK_MODEL_SERVICES_ONCE");
        MethodInfo stores=dataStore.GetMethod("get_ProjectStores",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null),check=dispatcher.GetMethod("CheckAccess",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null),invoke=dispatcher.GetMethod("Invoke",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{typeof(Action)},null);
        ProbeFiles.Require(stores!=null&&stores.DeclaringType==dataStore&&(int)stores.Attributes==3526&&stores.ReturnType.IsGenericType&&stores.ReturnType.GetGenericTypeDefinition()==typeof(IEnumerable<>)&&stores.ReturnType.GetGenericArguments()[0]==projectStore&&stores.GetParameters().Length==0&&check!=null&&check.DeclaringType==dispatcher&&check.ReturnType==typeof(bool)&&check.GetParameters().Length==0&&(int)check.Attributes==1478&&invoke!=null&&invoke.DeclaringType==dispatcher&&invoke.ReturnType==typeof(void)&&invoke.GetParameters().Length==1&&invoke.GetParameters()[0].ParameterType==typeof(Action)&&(int)invoke.Attributes==1478,"SDK_MODEL_HOST_EXACT_ABI");
        foreach(MethodInfo method in new MethodInfo[]{stores,check,invoke})ProbeFiles.Require(method.GetType()==typeof(object).GetMethod("GetType").GetType()&&method.IsPublic&&!method.IsStatic&&!method.IsGenericMethod&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_HOST_METHOD_PROFILE");
        ConstructorInfo ctor=root.GetConstructor(new Type[]{dataStore,factory,usage});MethodInfo projects=root.GetMethod("get_Projects",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ProbeFiles.Require(root.GetType()==typeof(object).GetType()&&root.IsPublic&&root.IsClass&&!root.IsAbstract&&!root.IsGenericType&&ctor!=null&&ctor.DeclaringType==root&&ctor.IsPublic&&!ctor.IsStatic&&(int)ctor.Attributes==6278&&projects!=null&&projects.DeclaringType==root&&projects.ReturnType==collection&&projects.GetParameters().Length==0&&(int)projects.Attributes==2534,"SDK_GENUINE_MODEL_ROOT_API");
        ParameterInfo[] vector=ctor.GetParameters();ProbeFiles.Require(vector.Length==3&&vector[0].ParameterType==dataStore&&vector[1].ParameterType==factory&&vector[2].ParameterType==usage&&vector[2].IsOptional&&vector[2].HasDefaultValue&&(int)vector[2].Attributes==4112&&vector[2].RawDefaultValue==null,"SDK_MODEL_ROOT_USAGE_DEFAULT");
        if(original)ProbeFiles.Require(SdkOriginal&&root.Assembly.FullName==OmIdentity&&root.MetadataToken==0x02000093&&root.Module==collection.Module&&root.Module==dataStore.Module&&root.Module==factory.Module&&dataStore.MetadataToken==0x02000030&&stores.MetadataToken==0x060001cc&&ctor.MetadataToken==0x0600053e&&projects.MetadataToken==0x06000537&&dispatcher.Assembly.FullName==Identities[10]&&dispatcher.MetadataToken==0x0200002f&&check.MetadataToken==0x060001a5&&invoke.MetadataToken==0x060001a6,"SDK_MODEL_ROOT_ORIGINAL_TOKENS");
        sdkRootData=new SdkHostProxy(dataStore,"DATA_STORE",stores,null,Array.CreateInstance(projectStore,0),Record);sdkUi=new SdkHostProxy(dispatcher,"DISPATCHER",check,invoke,null,Record);sdkModelRootApi=new SdkModelRootApi{Root=root,Collection=collection,Constructor=ctor,Projects=projects};
        Record("SDK_MODEL_SERVICES_READY data=EXPLICIT_EMPTY_ENUMERATION dispatcher=OWNED_REAL_WPF_MARSHAL_ADAPTER usage-default=VERIFIED_NULL");
    }
    static object CreateSdkCanonicalCollection(object factory)
    {
        ProbeFiles.Require(!sdkRootSetupPhase&&sdkModelRoot==null&&projectResourceWindow==0&&!projectResourceCallbackSuspended,"SDK_MODEL_ROOT_SETUP_ONCE");sdkRootSetupPhase=true;
        try{Record("SDK_MODEL_ROOT_BEGIN token="+sdkModelRootApi.Constructor.MetadataToken.ToString("X8")+" parent=GENUINE_ROOT data=EMPTY_FIXTURE");sdkModelRootConstructorActive=true;try{sdkModelRoot=sdkModelRootApi.Constructor.Invoke(new object[]{sdkRootData.GetTransparentProxy(),factory,Type.Missing});}finally{sdkModelRootConstructorActive=false;}ProbeFiles.Require(sdkModelRoot!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(sdkModelRoot)&&sdkModelRoot.GetType()==sdkModelRootApi.Root,"SDK_MODEL_ROOT_EXACT_RETURN");object collection=sdkModelRootApi.Projects.Invoke(sdkModelRoot,new object[0]);ProbeFiles.Require(collection!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection)&&collection.GetType()==sdkModelRootApi.Collection,"SDK_MODEL_CANONICAL_COLLECTION");Record("SDK_MODEL_ROOT_END returned=True canonical-collection=True dispatcher=REAL_WPF_ADAPTER");return collection;}
        catch(Exception error){sdkProbeFault=true;Record("SDK_MODEL_ROOT_FAILED type="+error.GetType().FullName+" message="+Text(error.Message));Errors(error);Exception cause=error;for(int depth=0;cause!=null&&depth<8;depth++,cause=cause.InnerException){try{MethodBase site=cause.TargetSite;if(site!=null)Record("SDK_MODEL_ROOT_EXCEPTION_SITE depth="+depth+" owner="+Text(site.DeclaringType==null?"GLOBAL":site.DeclaringType.FullName)+" token="+site.MetadataToken.ToString("X8")+" name="+Text(site.Name));}catch(Exception diagnostic){Record("SDK_MODEL_ROOT_EXCEPTION_SITE_UNAVAILABLE type="+diagnostic.GetType().FullName);}}throw;}
        finally{sdkModelRootConstructorActive=false;sdkRootSetupPhase=false;GC.KeepAlive(sdkModelRoot);}
    }
    static SdkDevicePluginDenyHolder sdkDevicePlugin;
    static UndoFactoryRegistrationApi sdkDevicePluginApi;
    static object[] sdkDeviceRetained;
    static void PrepareSdkDevicePlugin(TraceRegistrationApi verified,Assembly om)
    {
        Type device=om.GetType("Knx.Ets.ObjectModel.Project.Device",true),plugin=om.GetType("Knx.Ets.ObjectModel.Plugin.PluginWrapper",true);
        ProbeFiles.Require(SdkDeviceMode&&device.IsPublic&&device.IsClass&&!device.IsGenericType&&device.MetadataToken==0x02000230&&plugin.IsClass&&!plugin.IsGenericType&&plugin.Module==device.Module&&om.FullName==OmIdentity,"SDK_DEVICE_PLUGIN_TYPES");
        Type callback=typeof(Func<,>).MakeGenericType(device,plugin);MethodInfo resolve=om.ManifestModule.ResolveMethod(0x2b000019) as MethodInfo;
        ProbeFiles.Require(resolve!=null&&resolve.IsPublic&&resolve.IsStatic&&resolve.IsGenericMethod&&!resolve.ContainsGenericParameters&&resolve.GetGenericArguments().Length==1&&resolve.GetGenericArguments()[0]==callback&&resolve.ReturnType==callback&&resolve.Name=="Resolve"&&resolve.DeclaringType.FullName=="Autofac.ResolutionExtensions"&&resolve.DeclaringType.Assembly.FullName==FactoryExtraIdentities[0],"SDK_DEVICE_PLUGIN_EXACT_FACTORY_RESOLVE_TYPE");
        sdkDevicePlugin=new SdkDevicePluginDenyHolder(callback,plugin,Record);
        MethodInfo register=verified.Register.GetGenericMethodDefinition().MakeGenericMethod(callback);Type closed=register.ReturnType;
        ProbeFiles.Require(register.IsPublic&&register.IsStatic&&!register.ContainsGenericParameters&&register.GetParameters()[1].ParameterType==callback&&closed.IsGenericType&&closed.GetGenericTypeDefinition()==verified.Return.GetGenericTypeDefinition()&&closed.GetGenericArguments()[0]==callback&&closed.GetGenericArguments()[1]==verified.Return.GetGenericArguments()[1]&&closed.GetGenericArguments()[2]==verified.Return.GetGenericArguments()[2],"SDK_DEVICE_PLUGIN_REGISTER_ABI");
        sdkDevicePluginApi=new UndoFactoryRegistrationApi{Register=register,Return=closed,Contract=callback,Single=FactoryRegistrationMember(closed,"SingleInstance"),External=FactoryRegistrationMember(closed,"ExternallyOwned")};
    }
    static void RegisterSdkDevicePlugin(object builder)
    {
        ProbeFiles.Require(SdkDeviceMode&&sdkDevicePlugin!=null&&sdkDevicePluginApi!=null&&sdkDevicePlugin.State.Entries==0&&!sdkDevicePlugin.State.Faulted,"SDK_DEVICE_PLUGIN_REGISTRATION");
        object registration=sdkDevicePluginApi.Register.Invoke(null,new object[]{builder,sdkDevicePlugin.Callback});
        ProbeFiles.Require(registration!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration)&&registration.GetType().Assembly==sdkDevicePluginApi.Return.Assembly&&sdkDevicePluginApi.Return.IsInstanceOfType(registration),"SDK_DEVICE_PLUGIN_REGISTER_RETURN");
        ProbeFiles.Require(Object.ReferenceEquals(registration,sdkDevicePluginApi.Single.Invoke(registration,new object[0]))&&Object.ReferenceEquals(registration,sdkDevicePluginApi.External.Invoke(registration,new object[0])),"SDK_DEVICE_PLUGIN_SINGLE_EXTERNAL");
        TraceDependencyGuard(unexpected,rejected,"SDK_DEVICE_PLUGIN_REGISTER");Record("SDK_DEVICE_PLUGIN_REGISTER_END same-existing-delegate=True SingleInstance=True ExternallyOwned=True callback=DENY_ONLY return-type=ORIGINAL_PLUGIN_WRAPPER_NOT_CONSTRUCTED registrations=6");
    }
    static UndoFactoryRegistrationApi sdkImporterApi;
    static void PrepareSdkImporter(TraceRegistrationApi verified, Type callback)
    {
        ProbeFiles.Require(SdkOriginal && contextImporter != null && callback == contextImporter.Callback.GetType()
            && callback.IsGenericType && callback.GetGenericTypeDefinition() == typeof(Func<,,,,,,,>)
            && !callback.ContainsGenericParameters && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(contextImporter.Callback)
            && Object.ReferenceEquals(contextRetained[5], contextImporter.Callback), "SDK_IMPORTER_EXISTING_EXACT_DELEGATE");
        // Reuse the already validated public RegisterInstance definition; trace's genuine-instance guard is unchanged.
        MethodInfo register = verified.Register.GetGenericMethodDefinition().MakeGenericMethod(callback);
        Type closed = register.ReturnType;
        ProbeFiles.Require(register.IsPublic && register.IsStatic && !register.ContainsGenericParameters
            && register.GetParameters()[1].ParameterType == callback && closed.IsGenericType
            && closed.GetGenericTypeDefinition() == verified.Return.GetGenericTypeDefinition()
            && closed.GetGenericArguments()[0] == callback && closed.GetGenericArguments()[1] == verified.Return.GetGenericArguments()[1]
            && closed.GetGenericArguments()[2] == verified.Return.GetGenericArguments()[2], "SDK_IMPORTER_CLOSED_REGISTRATION");
        sdkImporterApi = new UndoFactoryRegistrationApi { Register=register, Return=closed, Contract=callback,
            Single=FactoryRegistrationMember(closed,"SingleInstance"), External=FactoryRegistrationMember(closed,"ExternallyOwned") };
    }
    static void RegisterSdkImporter(object builder)
    {
        ProbeFiles.Require(SdkOriginal && sdkImporterApi != null && contextImporter != null
            && contextImporter.Callback.GetType() == sdkImporterApi.Contract && Object.ReferenceEquals(contextRetained[5],contextImporter.Callback)
            && contextImporter.State.Entries == 0 && !contextImporter.State.Faulted, "SDK_IMPORTER_REGISTRATION_REFERENCE");
        TraceDependencyGuard(unexpected,rejected,"SDK_IMPORTER_PRE_REGISTER");
        object registration=sdkImporterApi.Register.Invoke(null,new object[]{builder,contextImporter.Callback});
        ProbeFiles.Require(registration!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration)
            && sdkImporterApi.Return.IsInstanceOfType(registration)&&registration.GetType().Assembly==sdkImporterApi.Return.Assembly,"SDK_IMPORTER_REGISTRATION_RETURN");
        ProbeFiles.Require(Object.ReferenceEquals(registration,sdkImporterApi.Single.Invoke(registration,new object[0]))
            && Object.ReferenceEquals(registration,sdkImporterApi.External.Invoke(registration,new object[0])),"SDK_IMPORTER_SINGLE_EXTERNAL");
        TraceDependencyGuard(unexpected,rejected,"SDK_IMPORTER_POST_REGISTER");
        Record("SDK_IMPORTER_REGISTER_END same-existing-delegate=True SingleInstance=True ExternallyOwned=True invocation=UNSUPPORTED registrations=5");
    }
    internal static bool InitializeAuditLog(string phase)
    {return CompactProductDiagnostics()?!sdkUiSuppressRepeatedHealthyFinal&&phase=="PROCESS_FINAL":!SdkInitializing||phase=="POST_DISPOSE"||phase=="AFTER_ALL_MANAGED_CLEANUP"||phase=="PROCESS_FINAL";}
    internal static bool SdkAuditLog(string phase)
    { return CompactProductDiagnostics()?!sdkUiSuppressRepeatedHealthyFinal&&phase=="PROCESS_FINAL":!SdkOriginal || phase=="AFTER_ALL_MANAGED_CLEANUP" || phase=="PROCESS_FINAL" || phase=="POST_DISPOSE"; }
    static void RegisterSdkUi(object builder)
    {
        object registration=sdkUiApi.Register.Invoke(null,new object[]{builder,sdkUiHolder.Callback});ProbeFiles.Require(registration!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration)&&sdkUiApi.Return.IsInstanceOfType(registration)&&registration.GetType().Assembly==sdkUiApi.Return.Assembly,"SDK_UI_REGISTRATION_RETURN");ProbeFiles.Require(Object.ReferenceEquals(registration,sdkUiApi.Single.Invoke(registration,new object[0]))&&Object.ReferenceEquals(registration,sdkUiApi.External.Invoke(registration,new object[0])),"SDK_UI_SINGLE_EXTERNAL");TraceDependencyGuard(unexpected,rejected,"SDK_UI_REGISTER");Record("SDK_UI_REGISTER_END contract=ORIGINAL_IDISPATCHER implementation=OWNED_REAL_WPF_ADAPTER SingleInstance=True ExternallyOwned=True registrations=4");
    }

    internal static bool SdkWrapperEligible()
    {return SdkMode&&sdkWrapperPhase&&Thread.CurrentThread.ManagedThreadId==sdkOwner&&(SdkOwned||contextLifetime)&&projectResourceBudgetActive&&projectResourceWindow==0&&!projectResourceCallbackSuspended;}
    static void PrepareSdkMapping(LargeStringCacheStoreProxy store,Type owner,Type stream,MethodInfo exists)
    {
        MethodInfo getter=owner.GetMethod("get_IdMappingCache",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ProbeFiles.Require(SdkMode&&sdkMapping==null&&getter!=null&&getter.DeclaringType==owner&&getter.Module==owner.Module&&getter.IsPublic&&!getter.IsStatic&&!getter.IsGenericMethod&&getter.ReturnType==stream&&(int)getter.Attributes==3526&&getter.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)&&getter.GetParameters().Length==0,"SDK_MAPPING_EXACT_GETTER");
        if(SdkOriginal)ProbeFiles.Require(owner.MetadataToken==0x02000057&&getter.MetadataToken==0x06000314&&stream.MetadataToken==0x02000061&&exists.MetadataToken==0x06000367&&owner.Assembly.FullName==OmIdentity,"SDK_MAPPING_ORIGINAL_TOKENS");
        sdkOwner=Thread.CurrentThread.ManagedThreadId;sdkMapping=new SdkMappingPolicy(getter,stream,exists,Record);store.ConfigureSdkMapping(sdkMapping);
    }
    static void RegisterSdkKim(object builder)
    {
        ProbeFiles.Require(SdkOriginal&&sdkKim!=null&&sdkKimApi!=null,"SDK_KIM_REGISTRATION");
        object registration=sdkKimApi.Register.Invoke(null,new object[]{builder,sdkKim.Callback});
        ProbeFiles.Require(registration!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration)&&sdkKimApi.Return.IsInstanceOfType(registration)&&registration.GetType().Assembly==sdkKimApi.Return.Assembly,"SDK_KIM_REGISTRATION_RETURN");
        ProbeFiles.Require(Object.ReferenceEquals(registration,sdkKimApi.Single.Invoke(registration,new object[0]))&&Object.ReferenceEquals(registration,sdkKimApi.External.Invoke(registration,new object[0])),"SDK_KIM_SINGLE_EXTERNAL");
        TraceDependencyGuard(unexpected,rejected,"SDK_KIM_REGISTER");Record("SDK_KIM_REGISTER_END same-existing-deny-proxy=True SingleInstance=True ExternallyOwned=True methods=UNSUPPORTED registrations=3");
    }
    static SdkWriteContextProxy sdkWriteFixture;
    static SdkInitializationProjectProxy sdkInitialization;
    static object sdkDcaInstance;
    static int sdkUiSeconds;
    static bool sdkUiCallAttempted,sdkUiCallReturned,sdkUiOriginalViewReturned,sdkUiRendered,sdkUiWindowClosed,sdkUiLifecycleFault;
    internal static string UiOutcome()
    {
        if(!SdkUiRequested||!sdkUiCallAttempted)return "NOT_RUN";
        string observed=sdkUiRendered?(sdkUiWindowClosed?"RENDERED_CLOSED":"RENDERED_NOT_CLOSED")
            :sdkUiWindowClosed?"CLOSED_BEFORE_RENDER":sdkUiOriginalViewReturned?"ORIGINAL_VIEW_NOT_RENDERED"
            :sdkUiCallReturned?"RETURNED_WITHOUT_ORIGINAL_VIEW":"CALL_NOT_RETURNED";
        return observed+(sdkUiLifecycleFault?"_WITH_FAILURE":String.Empty);
    }
    static bool SdkUiRequested{get{return SdkProductMode&&sdkUiSeconds==600;}}
    internal static bool ProductUiFixtureMode(){return SdkUiRequested;}
    internal static bool CompactProductDiagnostics(){return FixtureObservationProfile()||ReferenceDcaRequiredParameters.CompactDiagnostics(ProductUiFixtureMode(),ParameterNegativeProfile());}
    internal static int ProductUiCacheRequestLimit(){return ReferenceDcaRequiredParameters.ReadBudget(ParameterNegativeProfile(),SdkUiRequested?200000:32);}
    internal static int ProductUiCreatorLimit(){return ReferenceDcaRequiredParameters.ReadBudget(ParameterNegativeProfile(),SdkUiRequested?200000:128);}
    internal static int ProductUiWriteLimit(){return ReferenceDcaRequiredParameters.IdentityWriteBudget(FixtureObservationProfile(),ReferenceDcaRequiredParameters.ReadBudget(ParameterNegativeProfile(),SdkUiRequested?200000:16));}
    internal static int ProductUiProjectReferenceLimit(){return SdkUiRequested?10000000:0;}
    internal static int ProductUiOtherContextLimit(){return SdkUiRequested?200000:ContextAdapterLimit();}
    internal static string ContextBudgetModeDescription(){return "mode="+Text(mode)+" ui-requested="+SdkUiRequested+" ui-active="+sdkProductUiActive+" ui-seconds="+sdkUiSeconds+" parameter-negative="+ParameterNegativeProfile();}
    static volatile bool sdkProductUiActive;
    internal static bool ProductUiCacheLifetime(){return sdkProductUiActive&&SdkUiRequested&&ProductDependencyComplete&&contextLifetime;}
    internal static bool ProductUiCacheEligible(){return ProductUiCacheLifetime()&&(Thread.CurrentThread.ManagedThreadId!=sdkOwner||(sdkWrapperPhase&&projectResourceBudgetActive))&&projectResourceWindow==0&&!projectResourceCallbackSuspended;}
    static void CheckSdkFixture(string phase)
    {try{CheckDcaUiResources(phase);}finally{CheckSdkFixtureWithoutDcaUiResource(phase);}}
    static void CheckSdkFixtureWithoutDcaUiResource(string phase)
    {try{if(sdkProductFilePath!=null)sdkProductFilePath.Audit(phase);}finally{CheckSdkFixtureWithoutProductFilePath(phase);}}
    static void CheckSdkFixtureWithoutProductFilePath(string phase)
    {try{if(sdkProductContentWriter!=null)sdkProductContentWriter.Audit(phase);}finally{CheckSdkFixtureWithoutProductContentWriter(phase);}}
    static void CheckSdkFixtureWithoutProductContentWriter(string phase)
    {CheckProductProjectReads(phase,0);}
    static void CheckProductProjectReads(string phase,int index)
    {if(sdkProductProjectReads==null||index==sdkProductProjectReads.Length){CheckSdkFixtureWithoutProjectReads(phase);return;}try{sdkProductProjectReads[index].Audit(phase);}finally{CheckProductProjectReads(phase,index+1);}}
    static void CheckSdkFixtureWithoutProjectReads(string phase)
    {try{if(sdkProductApplication!=null)sdkProductApplication.Audit(phase);}finally{try{if(sdkProductStringStream!=null)sdkProductStringStream.Audit(phase);}finally{CheckProductManufacturerReads(phase,0);}}}
    static void CheckProductManufacturerReads(string phase,int index)
    {if(sdkProductManufacturerReads==null||index==sdkProductManufacturerReads.Length){CheckSdkFixtureWithoutProductApplication(phase);return;}try{sdkProductManufacturerReads[index].Audit(phase);}finally{CheckProductManufacturerReads(phase,index+1);}}
    static void CheckSdkFixtureWithoutProductApplication(string phase)
    {try{if(sdkWriteFixture!=null)sdkWriteFixture.Audit(phase);}finally{try{if(sdkInitialization!=null)sdkInitialization.Audit(phase);}finally{try{CheckSdkFixtureCore(phase);}finally{CheckProductMaster(phase);}}}}
    static void CheckSdkFixtureCore(string phase)
    {
        try{if(phase=="PROCESS_FINAL"||phase=="AFTER_ALL_MANAGED_CLEANUP")sdkParameterReadPublication.CheckClosed();}
        finally{
        if(CompactProductDiagnostics()&&SdkAuditLog(phase)){
            var policies=new List<SdkProductApplicationPolicy>{sdkProductFilePath,sdkProductContentWriter,sdkProductApplication,sdkProductStringStream};
            if(sdkProductManufacturerReads!=null)policies.AddRange(sdkProductManufacturerReads);if(sdkProductProjectReads!=null)policies.AddRange(sdkProductProjectReads);
            int configured=0,used=0,faults=0;foreach(var policy in policies)if(policy!=null){configured++;if(policy.Used)used++;if(policy.Faulted)faults++;}
            Record("PRODUCT_STORE_POLICY_SUMMARY phase=PROCESS_FINAL configured="+configured+" used="+used+" unused="+(configured-used)+" faulted="+faults+" validation=EVERY_PHASE source=ORIGINAL_STORES caller-receipt=UNKNOWN");
            Record("SDK_UI_ASSEMBLY_SUMMARY phase=PROCESS_FINAL checked-load-events="+Volatile.Read(ref assemblyCount)+" compacted-healthy-nondynamic-logs="+Volatile.Read(ref sdkUiCompactedAssemblyLogs)+" admission-checks=UNCHANGED dynamic-and-rejected=FULL_LOGS dedicated-provenance=UNCHANGED prevention=NOT_ESTABLISHED");
        }
        try{try{if(sdkMapping!=null)sdkMapping.Audit(phase);}finally{try{if(sdkRootData!=null)sdkRootData.Audit(phase);}finally{try{if(sdkUi!=null)sdkUi.Audit(phase);}finally{if(sdkUiHolder!=null)ProbeFiles.Require(!sdkUiHolder.Faulted&&sdkUiHolder.Entries<=1&&sdkUiHolder.Successes<=1,"SDK_UI_REFERENCE_FINAL_DENIAL");}}}}
        finally{try{if(sdkKim!=null){if(sdkKim.Faulted||SdkAuditLog(phase))Record("SDK_KIM_AUDIT phase="+phase+" entries="+sdkKim.Entries+" successes="+sdkKim.Successes+" fault="+sdkKim.Faulted);ProbeFiles.Require(!sdkKim.Faulted&&sdkKim.Entries<=1&&sdkKim.Successes<=1,"SDK_KIM_FINAL_DENIAL");}}
            finally{try{if(sdkDevicePlugin!=null)ProbeFiles.Require(sdkDevicePlugin.State.Entries==0&&!sdkDevicePlugin.State.Faulted,"SDK_DEVICE_PLUGIN_FINAL_DENIAL");}finally{GC.KeepAlive(sdkDeviceRetained);GC.KeepAlive(sdkDevicePreviousEmptyCollections);}ProbeFiles.Require(!sdkProbeFault&&!sdkFirstChanceFault&&sdkProbeAttempts<=1&&((phase!="PROCESS_FINAL"&&phase!="AFTER_ALL_MANAGED_CLEANUP")||!sdkWrapperPhase&&!sdkRootSetupPhase&&!sdkModelRootConstructorActive&&!sdkDeviceFactoryInvokeActive&&!sdkParameterDefinitionReadActive&&!sdkParameterReadPublication.Accepting&&sdkParameterReadPublication.Inflight==0&&!sdkParameterReadPublication.Faulted),"SDK_WRAPPER_FINAL_DENIAL");GC.KeepAlive(sdkRetained);GC.KeepAlive(sdkModelRoot);}}

        }
    }
    sealed class SdkProjectApi
    {internal Type Root,Collection,Project,Execution;internal ConstructorInfo Constructor,ExecutionConstructor;internal MethodInfo Projects,Item,ValidProject;}
    static SdkProjectApi SdkApi(Type root,Type collection,Type project,Type modelProject,Type store,Type execution,Type plugin,bool original)
    {
        foreach(Type type in new Type[]{root,collection,project,modelProject,store,execution,plugin})ProbeFiles.Require(type.GetType()==typeof(object).GetType()&&!type.IsGenericType&&!type.ContainsGenericParameters&&!type.Assembly.IsDynamic&&!type.Assembly.ReflectionOnly,"SDK_RUNTIME_TYPES");
        ConstructorInfo ctor=root.GetConstructor(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance,null,new Type[]{modelProject,store,execution,plugin},null);
        ProbeFiles.Require(root.IsPublic&&root.IsSealed&&collection.IsPublic&&project.IsPublic&&root.IsClass&&collection.IsClass&&project.IsClass&&typeof(System.Collections.IEnumerable).IsAssignableFrom(collection)&&ctor!=null&&ctor.DeclaringType==root&&!ctor.IsStatic&&(int)ctor.Attributes==6275,"SDK_EXACT_ROOT_API");
        ParameterInfo[] args=ctor.GetParameters();ProbeFiles.Require(args.Length==4&&args[0].ParameterType==modelProject&&args[1].ParameterType==store&&args[2].ParameterType==execution&&args[3].ParameterType==plugin,"SDK_ROOT_PARAMETER_VECTOR");
        for(int i=2;i<4;i++)ProbeFiles.Require(args[i].IsOptional&&args[i].HasDefaultValue&&(int)args[i].Attributes==4112&&args[i].RawDefaultValue==null,"SDK_ROOT_DECLARED_NULL_DEFAULT");
        MethodInfo projects=root.GetMethod("get_Projects",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null),item=collection.GetMethod("get_Item",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{typeof(int)},null);
        ProbeFiles.Require(projects!=null&&projects.DeclaringType==root&&projects.ReturnType==collection&&projects.GetParameters().Length==0&&item!=null&&item.DeclaringType==collection&&item.ReturnType==project&&item.GetParameters().Length==1&&item.GetParameters()[0].ParameterType==typeof(int),"SDK_EXACT_COLLECTION_VECTOR");
        foreach(MethodInfo method in new MethodInfo[]{projects,item})ProbeFiles.Require(method.GetType()==typeof(object).GetMethod("GetType").GetType()&&!method.IsStatic&&!method.IsGenericMethod&&method.IsPublic&&(int)method.Attributes==2182&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_COLLECTION_METHOD_FLAGS");
        if(original)ProbeFiles.Require(SdkOriginal&&root.Module==collection.Module&&root.Module==project.Module&&root.Assembly.FullName==Identities[1]&&ProbeFiles.Same(root.Assembly.Location,Path.Combine(stage,"Knx.Ets.Sdk.dll"))&&root.MetadataToken==0x0200002f&&collection.MetadataToken==0x020001d4&&project.MetadataToken==0x020001d1&&ctor.MetadataToken==0x0600026c&&projects.MetadataToken==0x06000274&&item.MetadataToken==0x0600144c&&modelProject.Module.Assembly.FullName==OmIdentity&&modelProject.MetadataToken==0x020003a5,"SDK_ORIGINAL_PINNED_API");
        Type addin=execution.Assembly.GetType(original?"Knx.Ets.Sdk.AddinExecutionContext":"Owned.Task009.SdkAddinExecutionFixture",true);
        ConstructorInfo executionCtor=addin.GetConstructor(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance,null,new Type[]{modelProject,typeof(string)},null);
        MethodInfo valid=addin.GetMethod("IsValidProject",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{modelProject},null);
        ProbeFiles.Require(addin.GetType()==typeof(object).GetType()&&!addin.IsGenericType&&!addin.ContainsGenericParameters&&!addin.IsAbstract&&!addin.Assembly.IsDynamic&&!addin.Assembly.ReflectionOnly&&execution.IsAssignableFrom(addin)&&executionCtor!=null&&executionCtor.DeclaringType==addin&&executionCtor.GetType()==typeof(object).GetConstructor(Type.EmptyTypes).GetType()&&!executionCtor.IsStatic&&(int)executionCtor.Attributes==6275,"SDK_EXACT_ADDIN_EXECUTION_CTOR");
        ParameterInfo[] executionArgs=executionCtor.GetParameters();ProbeFiles.Require(executionArgs.Length==2&&executionArgs[0].ParameterType==modelProject&&executionArgs[1].ParameterType==typeof(string)&&valid!=null&&valid.GetType()==typeof(object).GetMethod("GetType").GetType()&&valid.DeclaringType==addin&&!valid.IsStatic&&!valid.IsGenericMethod&&valid.ReturnType==typeof(bool)&&valid.GetParameters().Length==1&&valid.GetParameters()[0].ParameterType==modelProject&&(int)valid.Attributes==198&&valid.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_EXECUTION_EXACT_PROJECT_FILTER");
        if(original)ProbeFiles.Require(addin.Module==root.Module&&addin.MetadataToken==0x02000007&&(int)addin.Attributes==1048576&&executionCtor.MetadataToken==0x06000015&&valid.MetadataToken==0x0600001a,"SDK_EXECUTION_ORIGINAL_TOKENS");
        return new SdkProjectApi{Root=root,Collection=collection,Project=project,Execution=addin,Constructor=ctor,ExecutionConstructor=executionCtor,Projects=projects,Item=item,ValidProject=valid};
    }
    static object[] ConstructSdkRoute(SdkProjectApi api,object project,object store)
    {
        const string fixtureScope="OpenKNXStudio.OfflineSdkFixture";
        Record("SDK_EXECUTION_CONTEXT_BEGIN token="+api.ExecutionConstructor.MetadataToken.ToString("X8")+" scope=OWNED_OFFLINE_FIXTURE_NOT_DCA_DEVICE_APPLICATION_ID");
        object execution=api.ExecutionConstructor.Invoke(new object[]{project,fixtureScope});
        ProbeFiles.Require(execution!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(execution)&&execution.GetType()==api.Execution,"SDK_EXECUTION_EXACT_RETURN");
        sdkRetained=new object[]{null,null,null,execution};
        ProbeFiles.Require(Object.Equals(api.ValidProject.Invoke(execution,new object[]{project}),true),"SDK_GENUINE_EXECUTION_PROJECT_FILTER");
        Record("SDK_EXECUTION_CONTEXT_END returned=True genuine-project-filter=True host-context=SDK_TWO_ARGUMENT_OVERLOAD_DEFAULT");
        Record("SDK_ROOT_BEGIN token="+api.Constructor.MetadataToken.ToString("X8")+" execution-context=GENUINE_ADDIN_CONTEXT plugin-default=VERIFIED_TYPE_MISSING window=0");
        object root=api.Constructor.Invoke(new object[]{project,store,execution,Type.Missing});
        ProbeFiles.Require(root!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(root)&&root.GetType()==api.Root,"SDK_ROOT_EXACT_RETURN");
        sdkRetained[0]=root;Record("SDK_ROOT_END returned=True exact=True");
        object collection=api.Projects.Invoke(root,new object[0]);ProbeFiles.Require(collection!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection)&&collection.GetType()==api.Collection,"SDK_COLLECTION_EXACT_RETURN");sdkRetained[1]=collection;
        var enumerator=((System.Collections.IEnumerable)collection).GetEnumerator();ProbeFiles.Require(enumerator!=null,"SDK_COLLECTION_ENUMERATOR");object first;
        try{ProbeFiles.Require(enumerator.MoveNext(),"SDK_COLLECTION_EMPTY");first=enumerator.Current;}finally{var disposable=enumerator as IDisposable;if(disposable!=null)disposable.Dispose();}
        object wrapper=api.Item.Invoke(collection,new object[]{0});ProbeFiles.Require(wrapper!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(wrapper)&&wrapper.GetType()==api.Project&&Object.ReferenceEquals(first,wrapper),"SDK_PROJECT_EXACT_FIRST_MEMBER");sdkRetained[2]=wrapper;
        Record("SDK_PROJECT_WRAPPER returned=True exact=True membership=OBSERVED_FIRST_ITEM Initialize=NOT_RUN UI=NOT_RUN");return sdkRetained;
    }
    static bool SdkContainsExact(object collection,object project)
    {
        var enumerator=((System.Collections.IEnumerable)collection).GetEnumerator();ProbeFiles.Require(enumerator!=null,"SDK_MEMBERSHIP_ENUMERATOR");bool found=false;int count=0;
        try{while(enumerator.MoveNext()){ProbeFiles.Require(++count<=16,"SDK_MEMBERSHIP_ENUMERATION_BOUND");if(Object.ReferenceEquals(enumerator.Current,project))found=true;}}finally{var disposable=enumerator as IDisposable;if(disposable!=null)disposable.Dispose();}return found;
    }
    static void AttachSdkProjectFixture(object project,object collection,Type parent,bool original)
    {
        ProbeFiles.Require(SdkWrapperEligible()&&project!=null&&collection!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project)&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection)&&parent.IsAssignableFrom(project.GetType())&&collection is System.Collections.IEnumerable,"SDK_MEMBERSHIP_PHASE_TYPES");
        Type collectionType=collection.GetType();MethodInfo add=collectionType.GetMethod("Add",BindingFlags.Public|BindingFlags.Instance,null,new Type[]{parent},null),count=collectionType.GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
        ProbeFiles.Require(add!=null&&count!=null&&add.GetType()==typeof(object).GetMethod("GetType").GetType()&&count.GetType()==add.GetType()&&add.IsPublic&&!add.IsStatic&&!add.IsGenericMethod&&add.ReturnType==typeof(void)&&add.GetParameters().Length==1&&add.GetParameters()[0].ParameterType==parent&&(int)add.Attributes==486&&count.IsPublic&&!count.IsStatic&&!count.IsGenericMethod&&count.ReturnType==typeof(int)&&count.GetParameters().Length==0&&(int)count.Attributes==2534&&add.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)&&count.CallingConvention==add.CallingConvention,"SDK_PUBLIC_MEMBERSHIP_API");
        if(original)ProbeFiles.Require(SdkOriginal&&collectionType.Assembly.FullName==OmIdentity&&collectionType.MetadataToken==0x020003bd&&project.GetType().Module==collectionType.Module&&project.GetType().MetadataToken==0x020003a5&&parent.Module==collectionType.Module&&parent.MetadataToken==0x02000c99&&add.Module==collectionType.Module&&count.Module==collectionType.Module&&add.DeclaringType.MetadataToken==0x02000017&&count.DeclaringType==add.DeclaringType&&add.MetadataToken==0x060000b5&&count.MetadataToken==0x060000a0,"SDK_MEMBERSHIP_ORIGINAL_TOKENS");
        int before=(int)count.Invoke(collection,new object[0]);ProbeFiles.Require(before>=0&&before<16,"SDK_MEMBERSHIP_COUNT_BOUND");bool present=SdkContainsExact(collection,project);
        Record("SDK_MEMBERSHIP_BEGIN present="+present+" count="+before+" add-token="+add.MetadataToken.ToString("X8")+" scope=OWNED_OFFLINE_FIXTURE_NOT_ACCEPTED_CONFIGURATION");
        if(!present)add.Invoke(collection,new object[]{project});
        int after=(int)count.Invoke(collection,new object[0]);ProbeFiles.Require(after==(present?before:before+1)&&SdkContainsExact(collection,project),"SDK_MEMBERSHIP_EXACT_PROJECT_REQUIRED");
        Record("SDK_MEMBERSHIP_END exact-instance=True count="+after+" added="+!present+" values=NOT_LOGGED");
    }
    [ThreadStatic] static bool sdkFirstChanceRecording;
    static int sdkFirstChanceCount;
    static bool sdkFirstChanceFault;
    static void InvokeSdkInitialize(MethodInfo initialize,object instance,object context)
    {
        int owner=Thread.CurrentThread.ManagedThreadId;
        EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs> handler=delegate(object sender,System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args){
            if(Thread.CurrentThread.ManagedThreadId!=owner||sdkFirstChanceRecording)return;
            sdkFirstChanceRecording=true;
            try{
                if(sdkFirstChanceCount<9)sdkFirstChanceCount++;
                int count=sdkFirstChanceCount;
                if(count<=8){Exception error=args.Exception;MethodBase site=error.TargetSite;
                    Record("DCA_INITIALIZE_FIRST_CHANCE number="+count+" type="+Text(error.GetType().FullName)+" message="+Text(error.Message)
                        +" owner="+Text(site==null||site.DeclaringType==null?"UNKNOWN":site.DeclaringType.FullName)
                        +" token="+(site==null?"UNKNOWN":site.MetadataToken.ToString("X8"))+" method="+Text(site==null?"UNKNOWN":site.Name));}
                else if(count==9){sdkFirstChanceCount=10;Record("DCA_INITIALIZE_FIRST_CHANCE_LIMIT maximum-records=8 further-exceptions=NOT_RECORDED");}
            }catch{sdkFirstChanceFault=true;}finally{sdkFirstChanceRecording=false;}
        };
        // Observe without swallowing or replacing the vendor exception; never automate its modal UI.
        AppDomain.CurrentDomain.FirstChanceException+=handler;
        try{initialize.Invoke(instance,new object[]{context});}
        finally{AppDomain.CurrentDomain.FirstChanceException-=handler;}
    }
    static void InitializeSdkProject(object project)
    {
        ProbeFiles.Require(SdkInitializing&&SdkWrapperEligible()&&sdkInitialization==null,"DCA_INITIALIZE_ONCE_ACTIVE_SDK_WRAPPER");
        Assembly app=Assembly.LoadFrom(Path.Combine(stage,Names[0]+".dll"));
        ProbeFiles.Require(app.FullName==Identities[0]&&ProbeFiles.Same(app.Location,Path.Combine(stage,Names[0]+".dll"))&&!app.IsDynamic&&!app.ReflectionOnly,"DCA_INITIALIZE_PINNED_ASSEMBLY");
        Type type=app.GetType("Mdt.Dali.DcaApp.DcaAppMain",true,false);
        ConstructorInfo ctor=type.GetConstructor(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ProbeFiles.Require(type.MetadataToken==0x02000004&&ctor!=null&&ctor.MetadataToken==0x0600000e,"DCA_INITIALIZE_ORIGINAL_CONSTRUCTOR");
        // Reuse the existing six-getter ABI preflight, without changing its deny-all baseline.
        InitializeContextProbe baseline=InitializeContextProbe.ForOriginal(type,Record);
        MethodInfo initialize=(MethodInfo)type.Module.ResolveMethod(0x0600001a);Type context=initialize.GetParameters()[0].ParameterType;
        MethodInfo getter=context.GetMethod("get_Project",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        ProbeFiles.Require(getter!=null&&getter.MetadataToken==0x0600002f&&getter.ReturnType==project.GetType()&&project.GetType().MetadataToken==0x020001d1&&project.GetType().Assembly.FullName==Identities[1],"DCA_INITIALIZE_PROJECT_ABI");
        MethodInfo absentGetter=context.GetMethod("get_UserConfiguration",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ProbeFiles.Require(absentGetter!=null&&absentGetter.DeclaringType==context&&absentGetter.Module==getter.Module&&absentGetter.MetadataToken==0x06000032&&absentGetter.IsPublic&&!absentGetter.IsStatic&&!absentGetter.IsGenericMethod&&absentGetter.IsSpecialName&&absentGetter.GetParameters().Length==0&&absentGetter.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)
            &&absentGetter.ReturnType.FullName=="System.Xml.XmlDocument"&&absentGetter.ReturnType.Assembly.FullName=="System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089","DCA_INITIALIZE_ABSENT_USER_CONFIGURATION_ABI");
        sdkInitialization=new SdkInitializationProjectProxy(context,getter,absentGetter,project,Record,SdkWrapperEligible);
        bool returned=false;
        try{TraceDependencyGuard(unexpected,rejected,"DCA_CONSTRUCTOR_PRE");Record("DCA_CONSTRUCTOR_BEGIN token=0600000E assembly-sha256="+Hashes[0]);sdkDcaInstance=ctor.Invoke(new object[0]);ProbeFiles.Require(sdkDcaInstance!=null&&sdkDcaInstance.GetType()==type,"DCA_CONSTRUCTOR_EXACT_RETURN");Record("DCA_CONSTRUCTOR_END returned=True UI=NOT_RUN");TraceDependencyGuard(unexpected,rejected,"DCA_INITIALIZE_PRE");Record("DCA_INITIALIZE_BEGIN token=0600001A project=GENUINE_SDK_PROJECT UserConfiguration=CONTROLLED_ABSENT_INITIAL_STATE other-services=DENIED");InvokeSdkInitialize(initialize,sdkDcaInstance,sdkInitialization.GetTransparentProxy());returned=true;TraceDependencyGuard(unexpected,rejected,"DCA_INITIALIZE_POST");}
        finally{if(!returned||!SdkUiRequested)sdkInitialization.Close();Record("DCA_INITIALIZE_END returned="+returned+" UI=NOT_RUN hardware=NOT_RUN qualification=NOT_ESTABLISHED");GC.KeepAlive(baseline);GC.KeepAlive(sdkDcaInstance);}
    }
    static bool SdkWriteFixtureEligible()
    {return SdkInitializing&&(SdkWrapperEligible()||GuidProjectDisposalEligible());}
    static void PrepareSdkWriteFixture(object project)
    {
        Type context=project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.IObjectContext",true),type=project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.IWriteContext",true);
        MethodInfo getter=context.GetMethod("get_WriteContext",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        MethodInfo wait=type.GetMethod("Wait",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null),setDirty=type.GetMethod("SetDirty",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        MethodInfo iamDirty=type.GetMethod("IamDirty",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        MethodInfo locked=type.GetMethod("get_IsLockedForWriting",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);ProbeFiles.Require(locked!=null&&locked.DeclaringType==type&&locked.MetadataToken==0x0600037e&&(int)locked.Attributes==3526&&locked.ReturnType==typeof(bool)&&locked.GetParameters().Length==0,"SDK_WRITE_FIXTURE_LOCK_QUERY_ABI");
        ProbeFiles.Require(iamDirty!=null&&iamDirty.DeclaringType==type&&iamDirty.MetadataToken==0x0600037a&&(int)iamDirty.Attributes==1478&&iamDirty.ReturnType==typeof(bool)&&iamDirty.GetParameters().Length==1&&iamDirty.GetParameters()[0].ParameterType.FullName=="Knx.Ets.ObjectModel.IDomObject"&&iamDirty.GetParameters()[0].ParameterType.Module==type.Module,"SDK_WRITE_FIXTURE_DIRTY_QUERY_ABI");
        ProbeFiles.Require(SdkInitializing&&sdkWriteFixture==null&&PublicInterface(type)&&!type.IsGenericType&&type.Module==context.Module&&type.MetadataToken==0x02000065
            &&context.Assembly.FullName==OmIdentity&&context.MetadataToken==0x02000044&&getter!=null&&getter.DeclaringType==context&&getter.MetadataToken==0x06000266&&getter.ReturnType==type&&getter.GetParameters().Length==0&&(int)getter.Attributes==3526
            &&wait!=null&&wait.DeclaringType==type&&wait.MetadataToken==0x0600037f&&wait.ReturnType==typeof(bool)&&wait.GetParameters().Length==0&&(int)wait.Attributes==1478
            &&setDirty!=null&&setDirty.DeclaringType==type&&setDirty.MetadataToken==0x06000379&&setDirty.ReturnType==typeof(void)&&setDirty.GetParameters().Length==1&&(int)setDirty.Attributes==1478,"SDK_WRITE_FIXTURE_EXACT_ABI");
        Type argument=setDirty.GetParameters()[0].ParameterType;
        ProbeFiles.Require(PublicInterface(argument)&&argument.Module==type.Module&&argument.FullName=="Knx.Ets.ObjectModel.Project.IProjectDomObject"&&argument.IsAssignableFrom(project.GetType()),"SDK_WRITE_FIXTURE_PROJECT_ARGUMENT");
        foreach(MethodInfo method in new MethodInfo[]{getter,wait,setDirty,iamDirty,locked})ProbeFiles.Require(method.GetType()==typeof(object).GetMethod("GetType").GetType()&&method.IsPublic&&!method.IsStatic&&!method.IsGenericMethod&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_WRITE_FIXTURE_METHOD_PROFILE");
        MethodInfo scopeMethod=null;if(SdkProductMode){scopeMethod=type.GetMethod("WriteScope",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);ProbeFiles.Require(scopeMethod!=null&&scopeMethod.DeclaringType==type&&scopeMethod.Module==type.Module&&scopeMethod.MetadataToken==0x06000380&&(int)scopeMethod.Attributes==1478&&!scopeMethod.IsStatic&&!scopeMethod.IsGenericMethod&&scopeMethod.ReturnType==typeof(IDisposable)&&scopeMethod.GetParameters().Length==0,"SDK_WRITE_SCOPE_EXACT_ABI");}
        sdkWriteFixture=new SdkWriteContextProxy(type,wait,setDirty,iamDirty,locked,scopeMethod,project,Record,SdkWriteFixtureEligible);activeValidator.ConfigureWrite(getter,sdkWriteFixture.GetTransparentProxy(),SdkWriteFixtureEligible);
        if(SdkProductMode){
            MethodInfo clean=type.GetMethod("SetClean",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);Type domObject=type.Assembly.GetType("Knx.Ets.ObjectModel.IDomObject",true);MethodInfo parent=SdkDeviceMethod(domObject,0x060001d5);
            ProbeFiles.Require(clean!=null&&clean.DeclaringType==type&&clean.MetadataToken==0x0600037b&&clean.Module==type.Module&&(int)clean.Attributes==1478&&!clean.IsStatic&&!clean.IsGenericMethod&&clean.ReturnType==typeof(void)&&clean.GetParameters().Length==1&&clean.GetParameters()[0].ParameterType==domObject&&domObject.MetadataToken==0x02000033&&parent.Name=="get_Parent"&&parent.ReturnType==domObject&&parent.GetParameters().Length==0&&(int)parent.Attributes==3526,"SDK_WRITE_PRODUCT_DIRTY_QUERY_ABI");
            sdkWriteFixture.ConfigureProductDirtyQueries(clean,parent);
        }
        Record("SDK_WRITE_FIXTURE_READY service=OFFLINE_CONTROLLED_FIXTURE_NOT_PERSISTENT_CONFIGURATION operations=Wait_SetDirty_IamDirty_IsLockedForWriting"+(SdkProductMode?"_WriteScope_SetClean_SameProjectDirtyQueries":"")+" other-methods=DENIED");
    }
    static void PrepareSdkFixtureName(object project)
    {
        ProbeFiles.Require(SdkInitializing&&SdkWrapperEligible()&&project!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(project),"SDK_FIXTURE_NAME_PHASE");
        Type type=project.GetType();MethodInfo get=type.GetMethod("get_Name",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        MethodInfo set=type.GetMethod("set_Name",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{typeof(string)},null);
        ProbeFiles.Require(type.Assembly.FullName==OmIdentity&&type.MetadataToken==0x020003a5&&get!=null&&set!=null&&get.DeclaringType==type&&set.DeclaringType==type&&get.Module==type.Module&&set.Module==type.Module
            &&get.MetadataToken==0x06002879&&set.MetadataToken==0x0600287a&&get.ReturnType==typeof(string)&&set.ReturnType==typeof(void)
            &&get.GetParameters().Length==0&&set.GetParameters().Length==1&&set.GetParameters()[0].ParameterType==typeof(string)
            &&get.IsPublic&&set.IsPublic&&get.IsSpecialName&&set.IsSpecialName&&!get.IsStatic&&!set.IsStatic&&!get.IsGenericMethod&&!set.IsGenericMethod
            &&(int)get.Attributes==2534&&(int)set.Attributes==2534&&get.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)&&set.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_FIXTURE_NAME_EXACT_PUBLIC_ABI");
        PrepareSdkWriteFixture(project);
        const string name="OpenKNX Studio offline fixture";
        Record("SDK_FIXTURE_NAME_BEGIN token=0600287A initial-null="+(get.Invoke(project,new object[0])==null)+" scope=TRANSIENT_OWNED_FIXTURE_NOT_ACCEPTED_CONFIGURATION");
        Type context=type.Assembly.GetType("Knx.Ets.ObjectModel.IObjectContext",true),undoType=type.Assembly.GetType("Knx.Ets.ObjectModel.UndoRedo.UndoManager",true);
        MethodInfo undoGetter=context.GetMethod("get_UndoManager",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        MethodInfo onDisabled=undoType.GetMethod("OnDisabled",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{typeof(Action)},null);
        MethodInfo isEnabled=undoType.GetMethod("get_IsEnabled",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ProbeFiles.Require(context.MetadataToken==0x02000044&&undoType.MetadataToken==0x02000131&&undoType.Module==type.Module&&undoGetter!=null&&undoGetter.DeclaringType==context&&undoGetter.MetadataToken==0x06000257&&undoGetter.ReturnType==undoType
            &&onDisabled!=null&&onDisabled.DeclaringType==undoType&&onDisabled.MetadataToken==0x06000ba1&&(int)onDisabled.Attributes==134&&onDisabled.ReturnType==typeof(void)&&onDisabled.GetParameters().Length==1&&onDisabled.GetParameters()[0].ParameterType==typeof(Action)
            &&isEnabled!=null&&isEnabled.DeclaringType==undoType&&isEnabled.MetadataToken==0x06000ba4&&isEnabled.IsAssembly&&(int)isEnabled.Attributes==2179&&isEnabled.ReturnType==typeof(bool)&&isEnabled.GetParameters().Length==0,"SDK_FIXTURE_NAME_UNDO_INITIALIZATION_API");
        foreach(MethodInfo method in new MethodInfo[]{undoGetter,onDisabled,isEnabled})ProbeFiles.Require((method==isEnabled?method.IsAssembly:method.IsPublic)&&!method.IsStatic&&!method.IsGenericMethod&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_FIXTURE_NAME_UNDO_METHOD_PROFILE");
        object undo=undoGetter.Invoke(contextRetained[4],new object[0]);ProbeFiles.Require(undo!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(undo)&&undo.GetType()==undoType,"SDK_FIXTURE_NAME_GENUINE_UNDO");
        bool initiallyEnabled=(bool)isEnabled.Invoke(undo,new object[0]);
        Action setup=delegate{
            ProbeFiles.Require(!(bool)isEnabled.Invoke(undo,new object[0]),"SDK_FIXTURE_NAME_UNDO_DISABLED_SCOPE");
            ProbeFiles.Require(!sdkNameSetterActive,"SDK_FIXTURE_NAME_RESOURCE_PHASE_ONCE");sdkNameResourceOwner=type.Assembly;sdkNameSetterActive=true;
            try{set.Invoke(project,new object[]{name});}finally{sdkNameSetterActive=false;}
            ProbeFiles.Require(Object.Equals(get.Invoke(project,new object[0]),name),"SDK_FIXTURE_NAME_EXACT_READBACK");
        };
        Record("SDK_FIXTURE_NAME_UNDO_SCOPE_BEGIN token=06000BA1 API=GENUINE_PUBLIC_ON_DISABLED purpose=INITIAL_TRANSIENT_FIXTURE_NO_UNDO_HISTORY");
        try{onDisabled.Invoke(undo,new object[]{setup});}
        finally{ProbeFiles.Require((bool)isEnabled.Invoke(undo,new object[0])==initiallyEnabled,"SDK_FIXTURE_NAME_UNDO_SCOPE_RESTORED");GC.KeepAlive(undo);}
        Record("SDK_FIXTURE_NAME_UNDO_SCOPE_END original-frame-restored=True genuine-validation=True");
        TraceDependencyGuard(unexpected,rejected,"SDK_FIXTURE_NAME_POST");
        Record("SDK_FIXTURE_NAME_END genuine-public-setter=True exact-readback=True vendor-identity=NOT_CLAIMED persistence=NOT_IMPLEMENTED");
    }
    static void PrepareSdkIdCreator(Assembly om,Type context)
    {
        Type type=om.GetType("Knx.Ets.ObjectModel.UniqueIdCreator",true);MethodInfo getter=SdkDeviceMethod(context,0x06000259);ConstructorInfo ctor=type.GetConstructor(new Type[]{context});
        ProbeFiles.Require(SdkDeviceMode&&type.MetadataToken==0x0200009e&&(int)type.Attributes==1048577&&type.Module==context.Module&&ctor!=null&&ctor.DeclaringType==type&&ctor.MetadataToken==0x06000593&&(int)ctor.Attributes==6278&&getter.GetParameters().Length==0&&getter.ReturnType.FullName=="Knx.Ets.ObjectModel.IIdCreator"&&getter.ReturnType.Module==om.ManifestModule&&PublicInterface(getter.ReturnType)&&getter.ReturnType.IsAssignableFrom(type),"SDK_DEVICE_GENUINE_ID_CREATOR_API");
        activeValidator.ConfigureIdCreator(getter,ctor,type,delegate{return contextRetained[4];});Record("SDK_DEVICE_ID_CREATOR_READY original-constructor=06000593 context-callback-limit=32 legacy-limit=16 IDs=ORIGINAL_DOMAIN_CREATOR");
    }
    static void BindSdkReferenceObject(object child,object collection)
    {
        ProbeFiles.Require(SdkDeviceMode&&SdkWrapperEligible()&&sdkWriteFixture!=null&&child!=null&&collection!=null,"SDK_DEVICE_BIND_PHASE");Assembly om=child.GetType().Assembly;Type context=om.GetType("Knx.Ets.ObjectModel.IObjectContext",true),domCollection=om.GetType("Knx.Ets.ObjectModel.IDomCollection",true),domObject=om.GetType("Knx.Ets.ObjectModel.IDomObject",true),parentBase=om.GetType("Knx.Ets.ObjectModel.Project.ProjectDomObject",true);
        MethodInfo bind=SdkDeviceMethod(parentBase,0x060029dd),id=SdkDeviceMethod(domObject,0x060001d4);ParameterInfo[] vector=bind.GetParameters();
        ProbeFiles.Require(parentBase.IsAssignableFrom(child.GetType())&&domCollection.IsAssignableFrom(collection.GetType())&&vector.Length==3&&vector[0].ParameterType==domCollection&&vector[1].ParameterType==context&&vector[2].ParameterType==typeof(bool)&&vector[2].IsOptional&&vector[2].HasDefaultValue&&(int)vector[2].Attributes==4112&&Object.Equals(vector[2].RawDefaultValue,true),"SDK_DEVICE_BIND_PUBLIC_ABI");
        sdkWriteFixture.AuthorizeChild(child);bool installation=child.GetType().MetadataToken==0x02000324;ProbeFiles.Require(installation||child.GetType().MetadataToken==0x02000230,"SDK_CHILD_VALIDATOR_FIXED_CHILD");
        Type checker=om.GetType(installation?"Knx.Ets.ObjectModel.ConsistencyRules.InstallationConsistencyChecker":"Knx.Ets.ObjectModel.ConsistencyRules.DeviceConsistencyChecker",true),argument=installation?child.GetType():om.GetType("Knx.Ets.ObjectModel.Project.IDevice",true);
        ConstructorInfo ctor=checker.GetConstructor(new Type[]{collection.GetType(),argument});Type validator=om.GetType("Knx.Ets.ObjectModel.ConsistencyRules.Validator",true);MethodInfo definition=context.Module.ResolveMethod(0x06000255) as MethodInfo;ProbeFiles.Require(definition!=null&&definition.DeclaringType==context&&definition.IsPublic&&!definition.IsStatic&&definition.IsGenericMethodDefinition&&definition.GetGenericArguments().Length==1,"SDK_CHILD_VALIDATOR_GENERIC_DEFINITION");
        // GetValidator is the one explicitly verified generic definition, unlike the nongeneric helper above.
        ProbeFiles.Require(ctor!=null&&ctor.DeclaringType==checker&&ctor.MetadataToken==(installation?0x06008a3c:0x06008a12)&&(int)ctor.Attributes==6278&&checker.MetadataToken==(installation?0x02000e86:0x02000e7f)&&checker.IsPublic&&!checker.IsAbstract&&!checker.IsGenericType&&validator.IsAssignableFrom(checker)&&argument.IsAssignableFrom(child.GetType()),"SDK_CHILD_VALIDATOR_EXACT_CTOR");
        activeValidator.ConfigureChildValidator(new SdkChildValidatorCase{Method=definition.MakeGenericMethod(checker),Constructor=ctor,Child=child,Collection=collection});
        Record("SDK_DEVICE_BIND_BEGIN token=060029DD child="+child.GetType().FullName+" initial-id-null="+(id.Invoke(child,new object[0])==null)+" setProjectModified=VERIFIED_DEFAULT_TRUE");
        bind.Invoke(child,new object[]{collection,contextRetained[4],Type.Missing});ProbeFiles.Require(id.Invoke(child,new object[0])!=null,"SDK_DEVICE_BIND_GENERATED_ID");
        MethodInfo target=om.GetType("Knx.Ets.ObjectModel.DomObject",true).GetMethod("get_InternalParentCollection",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);ProbeFiles.Require(target!=null&&target.MetadataToken==0x06000114&&Object.ReferenceEquals(target.Invoke(child,new object[0]),collection),"SDK_DEVICE_BIND_EXACT_COLLECTION");
        Record("SDK_DEVICE_BIND_END original-id=True original-after-bind=True exact-target=True");
    }
    static MethodInfo SdkDeviceMethod(Type owner,int token)
    {
        MethodInfo method=owner.Module.ResolveMethod(token) as MethodInfo;
        ProbeFiles.Require(method!=null&&method.DeclaringType==owner&&method.IsPublic&&!method.IsStatic&&!method.IsGenericMethod&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"SDK_DEVICE_EXACT_PUBLIC_METHOD:"+token.ToString("X8"));return method;
    }
    static object SdkReferenceId(Assembly ids,string typeName,int typeToken,int ctorToken,string literal)
    {
        Type section=ids.GetType("Knx.Ets.Ids.StringSection",true),type=ids.GetType(typeName,true);ConstructorInfo sectionCtor=section.GetConstructor(new Type[]{typeof(string)}),ctor=type.GetConstructor(new Type[]{section});
        ProbeFiles.Require(ids.FullName==Identities[11]&&section.MetadataToken==0x02000013&&sectionCtor!=null&&sectionCtor.MetadataToken==0x060000bb&&sectionCtor.IsPublic&&type.MetadataToken==typeToken&&type.IsPublic&&!type.IsGenericType&&ctor!=null&&ctor.DeclaringType==type&&ctor.IsPublic&&ctor.MetadataToken==ctorToken,"SDK_DEVICE_REFERENCE_ID_CTOR");
        object result=ctor.Invoke(new object[]{sectionCtor.Invoke(new object[]{literal})});ProbeFiles.Require(result!=null&&result.GetType()==type,"SDK_DEVICE_REFERENCE_ID_TYPE");
        string parsed=result.ToString();Record("SDK_REFERENCE_PRODUCT_ID input="+literal+" parsed="+Text(parsed)+" identity=MANUFACTURER_PRODUCT_NOT_PHYSICAL_DEVICE");ProbeFiles.Require(parsed==literal,"SDK_DEVICE_REFERENCE_ID_NO_REMAP");return result;
    }
    static void SdkAttachReferenceObject(object collection,object child,string collectionName,int childToken)
    {
        ProbeFiles.Require(collection!=null,"SDK_DEVICE_ATTACHMENT_COLLECTION_NULL:"+collectionName);ProbeFiles.Require(child!=null,"SDK_DEVICE_ATTACHMENT_CHILD_NULL:"+collectionName);
        Type type=collection.GetType(),childType=child.GetType(),parent=type.Assembly.GetType("Knx.Ets.ObjectModel.Interfaces.INamedDomObject",true);
        ProbeFiles.Require(type.Assembly.FullName==OmIdentity&&type.FullName==collectionName&&type.IsPublic&&!type.IsGenericType&&childType.Module==type.Module&&childType.MetadataToken==childToken&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection)&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(child)&&parent.IsAssignableFrom(childType),"SDK_DEVICE_ATTACHMENT_TYPES");
        MethodInfo add=type.GetMethod("Add",BindingFlags.Public|BindingFlags.Instance,null,new Type[]{parent},null),count=type.GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
        ProbeFiles.Require(add!=null&&count!=null&&add.Module==type.Module&&count.Module==type.Module&&add.DeclaringType.MetadataToken==0x02000017&&count.DeclaringType==add.DeclaringType&&add.MetadataToken==0x060000b5&&count.MetadataToken==0x060000a0&&add.ReturnType==typeof(void)&&count.ReturnType==typeof(int)&&(int)add.Attributes==486&&(int)count.Attributes==2534,"SDK_DEVICE_PUBLIC_ATTACHMENT_API");
        int before=(int)count.Invoke(collection,new object[0]);bool present=SdkContainsExact(collection,child);ProbeFiles.Require(before==(present?1:0),"SDK_DEVICE_ONLY_EXPECTED_BOUND_MEMBER");
        Record("SDK_DEVICE_ATTACHMENT_BEGIN collection="+collectionName+" present="+present+" add-token=060000B5 scope=TRANSIENT_OFFLINE_FIXTURE");if(!present)add.Invoke(collection,new object[]{child});
        ProbeFiles.Require((int)count.Invoke(collection,new object[0])==1&&SdkContainsExact(collection,child),"SDK_DEVICE_ATTACHMENT_EXACT_MEMBER");Record("SDK_DEVICE_ATTACHMENT_END exact-instance=True count=1");
    }
    static object SdkSingleReferenceItem(object collection,Type expected,int indexToken)
    {
        object first=null;var sequence=collection as System.Collections.IEnumerable;ProbeFiles.Require(sequence!=null,"SDK_DEVICE_SDK_ENUMERABLE");var iterator=sequence.GetEnumerator();
        try{ProbeFiles.Require(iterator.MoveNext(),"SDK_DEVICE_SDK_COLLECTION_EMPTY");first=iterator.Current;ProbeFiles.Require(first!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(first)&&first.GetType()==expected&&!iterator.MoveNext(),"SDK_DEVICE_SDK_EXACTLY_ONE_MEMBER");}
        finally{IDisposable disposable=iterator as IDisposable;if(disposable!=null)disposable.Dispose();}
        MethodInfo item=SdkDeviceMethod(collection.GetType(),indexToken);ParameterInfo[] args=item.GetParameters();ProbeFiles.Require(item.ReturnType==expected&&args.Length==1&&args[0].ParameterType==typeof(int)&&Object.ReferenceEquals(item.Invoke(collection,new object[]{0}),first),"SDK_DEVICE_SDK_INDEX_REFERENCE");return first;
    }
    static bool sdkDeviceCollectionsAttempted;
    static object sdkDeviceInstallations;
    static object[] sdkDevicePreviousEmptyCollections;
    static object InitializeSdkDeviceCollections(object project)
    {
        ProbeFiles.Require(SdkDeviceMode&&SdkWrapperEligible()&&!sdkDeviceCollectionsAttempted&&sdkRetained==null&&sdkDcaInstance==null&&sdkInitialization==null,"SDK_DEVICE_COLLECTION_INIT_ONCE_BEFORE_SDK_CONSUMERS");
        Type type=project.GetType(),context=type.Assembly.GetType("Knx.Ets.ObjectModel.IObjectContext",true);
        int[] tokens={0x06002852,0x06002854,0x06002872,0x06002874,0x06002876,0x06002877,0x06002891,0x060028b8,0x060028b9};
        string[] names={"Knx.Ets.ObjectModel.Project.InstallationCollection","Knx.Ets.ObjectModel.Product.ManufacturerCollection","Knx.Ets.ObjectModel.Project.HistoryEntryCollection","Knx.Ets.ObjectModel.Project.DeviceCertificateCollection","Knx.Ets.ObjectModel.Project.ToDoItemCollection","Knx.Ets.ObjectModel.Project.ProjectTraceCollection","Knx.Ets.ObjectModel.Project.ProjectTagCollection","Knx.Ets.ObjectModel.Project.AddinDataCollection","Knx.Ets.ObjectModel.Project.UserFileCollection"};
        MethodInfo[] getters=new MethodInfo[tokens.Length];
        object[] previous=new object[getters.Length];
        for(int i=0;i<getters.Length;i++){
            getters[i]=SdkDeviceMethod(type,tokens[i]);ProbeFiles.Require(getters[i].GetParameters().Length==0&&getters[i].ReturnType.Module==type.Module&&getters[i].ReturnType.FullName==names[i]&&(int)getters[i].Attributes==2534,"SDK_DEVICE_CANONICAL_COLLECTION_GETTER:"+i);
            if(i==4||i==7)continue; // Original IsOpen-guarded getters are not needed during this closed setup.
            object value=getters[i].Invoke(project,new object[0]);
            if(value!=null){ProbeFiles.Require(!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)&&value.GetType()==getters[i].ReturnType&&Object.ReferenceEquals(getters[i].Invoke(project,new object[0]),value),"SDK_DEVICE_EXISTING_CANONICAL_COLLECTION:"+getters[i].Name);MethodInfo count=value.GetType().GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);ProbeFiles.Require(count!=null&&count.MetadataToken==0x060000a0&&count.ReturnType==typeof(int)&&(int)count.Invoke(value,new object[0])==0,"SDK_DEVICE_EXISTING_COLLECTION_NOT_EMPTY:"+getters[i].Name);previous[i]=value;Record("SDK_DEVICE_PRIME_COLLECTION member="+getters[i].Name+" genuine=True empty=True no-SDK-consumer=True");}
        }
        sdkDevicePreviousEmptyCollections=previous;
        MethodInfo getter=type.Module.ResolveMethod(0x0600283b) as MethodInfo,initializer=type.Module.ResolveMethod(0x060028ea) as MethodInfo;
        Type callbackType=typeof(Action<>).MakeGenericType(context);
        ProbeFiles.Require(type.MetadataToken==0x020003a5&&type.Assembly.FullName==OmIdentity&&getter!=null&&getter.DeclaringType==type&&getter.IsFamily&&(int)getter.Attributes==2244&&getter.ReturnType==callbackType&&getter.GetParameters().Length==0
            &&initializer!=null&&initializer.DeclaringType==type&&initializer.IsPrivate&&(int)initializer.Attributes==129&&initializer.ReturnType==typeof(void)&&initializer.GetParameters().Length==1&&initializer.GetParameters()[0].ParameterType==context&&!initializer.IsGenericMethod&&!initializer.IsStatic,"SDK_DEVICE_ORIGINAL_AFTER_BIND_API");
        Delegate callback=getter.Invoke(project,new object[0]) as Delegate;
        ProbeFiles.Require(callback!=null&&callback.GetType()==callbackType&&callback.GetInvocationList().Length==1&&Object.ReferenceEquals(callback.Target,project)&&callback.Method.Equals(initializer)&&contextRetained!=null&&context.IsInstanceOfType(contextRetained[4]),"SDK_DEVICE_ORIGINAL_AFTER_BIND_DELEGATE");
        MethodInfo invoke=callbackType.GetMethod("Invoke",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{context},null);
        ProbeFiles.Require(invoke!=null&&invoke.DeclaringType==callbackType&&invoke.ReturnType==typeof(void)&&!invoke.IsGenericMethod,"SDK_DEVICE_TYPED_AFTER_BIND_INVOKE");
        sdkDeviceCollectionsAttempted=true;Record("SDK_DEVICE_COLLECTION_INIT_BEGIN API=ORIGINAL_AFTER_BIND_ACTION token=060028EA initial-canonical-collections=NULL_OR_GENUINE_EMPTY before-SDK-consumers=True scope=NEW_TRANSIENT_FIXTURE_ONLY");
        invoke.Invoke(callback,new object[]{contextRetained[4]});
        object installations=null;
        for(int i=0;i<getters.Length;i++){if(i==4||i==7)continue;object value=getters[i].Invoke(project,new object[0]);ProbeFiles.Require(value!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)&&value.GetType()==getters[i].ReturnType,"SDK_DEVICE_CANONICAL_COLLECTION_RESULT:"+getters[i].Name);MethodInfo count=value.GetType().GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);ProbeFiles.Require(count!=null&&count.MetadataToken==0x060000a0&&count.ReturnType==typeof(int)&&(int)count.Invoke(value,new object[0])==0,"SDK_DEVICE_CANONICAL_COLLECTION_EMPTY:"+getters[i].Name);if(i==0)installations=value;}
        sdkDeviceInstallations=installations;Record("SDK_DEVICE_COLLECTION_INIT_END original-domain-constructors=True genuine-verified-direct-collections=7 IsOpen-guarded-getters=NOT_READ raw-field-writes=NONE");GC.KeepAlive(callback);GC.KeepAlive(previous);return installations;
    }
    static void PrepareSdkCanonicalReferences(object project,object installations,object installation)
    {
        Type context=project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.IObjectContext",true),type=installation.GetType();
        MethodInfo projectCollections=SdkDeviceMethod(project.GetType(),0x06002852),defaultInstallation=SdkDeviceMethod(project.GetType(),0x0600284b);
        int[] contextTokens={0x06000262,0x0600025b,0x0600025c,0x0600025d,0x0600025e,0x0600025f,0x06000260,0x06000263,0x06000264,0x06000265};
        int[] installationTokens={0,0x0600234b,0x06002351,0x06002348,0x06002341,0x0600234d,0x0600234f,0x06002344,0x06002346,0x06002347};
        MethodInfo[] methods=new MethodInfo[contextTokens.Length];Func<object>[] references=new Func<object>[contextTokens.Length];
        for(int i=0;i<methods.Length;i++){
            MethodInfo member=SdkDeviceMethod(context,contextTokens[i]),source=i==0?null:SdkDeviceMethod(type,installationTokens[i]);
            ProbeFiles.Require(SdkDeviceMode&&member.GetParameters().Length==0&&(int)member.Attributes==3526&&member.ReturnType.Module==type.Module&&member.ReturnType.IsPublic&&!member.ReturnType.IsGenericType
                &&(i==0?member.ReturnType==type:source.ReturnType==member.ReturnType&&source.GetParameters().Length==0&&(int)source.Attributes==2534),"SDK_CONTEXT_CANONICAL_REFERENCE_ABI:"+contextTokens[i].ToString("X8"));
            methods[i]=member;
            references[i]=delegate{
                ProbeFiles.Require(Object.ReferenceEquals(projectCollections.Invoke(project,new object[0]),installations)&&Object.ReferenceEquals(defaultInstallation.Invoke(project,new object[0]),installation)&&SdkContainsExact(installations,installation),"SDK_CONTEXT_REAL_DEFAULT_INSTALLATION");
                MethodInfo count=installations.GetType().GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);ProbeFiles.Require(count!=null&&count.MetadataToken==0x060000a0&&(int)count.Invoke(installations,new object[0])==1,"SDK_CONTEXT_SINGLE_INSTALLATION_SCOPE");
                object result=source==null?installation:source.Invoke(installation,new object[0]);
                ProbeFiles.Require(result!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(result)&&result.GetType()==member.ReturnType,"SDK_CONTEXT_CANONICAL_REFERENCE_RESULT");
                Record("SDK_CONTEXT_CANONICAL_REFERENCE member="+member.Name+" source="+(source==null?"GENUINE_DEFAULT_INSTALLATION":source.Name)+" original-reference=True values=NOT_INSPECTED");return result;
            };
        }
        activeValidator.ConfigureCanonicalReferences(methods,references);Record("SDK_CONTEXT_CANONICAL_REFERENCES_READY exact-readonly-getters=10 source=ORIGINAL_DEFAULT_INSTALLATION limits=UNCHANGED");
    }
    static void PrepareSdkAllDevices(object project,object installations,object installation,object devices)
    {
        Type context=project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.IObjectContext",true);MethodInfo getter=SdkDeviceMethod(context,0x0600025a);
        MethodInfo projectCollections=SdkDeviceMethod(project.GetType(),0x06002852),deviceCollection=SdkDeviceMethod(installation.GetType(),0x0600233d);
        ProbeFiles.Require(SdkDeviceMode&&devices!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(devices)&&devices.GetType().MetadataToken==0x0200026c&&devices.GetType().Module==context.Module&&getter.ReturnType==devices.GetType()&&getter.GetParameters().Length==0&&(int)getter.Attributes==3526,"SDK_ALL_DEVICES_EXACT_CONTEXT_ABI");
        activeValidator.ConfigureAllDevices(getter,delegate{
            ProbeFiles.Require(Object.ReferenceEquals(projectCollections.Invoke(project,new object[0]),installations)&&Object.ReferenceEquals(deviceCollection.Invoke(installation,new object[0]),devices)&&SdkContainsExact(installations,installation),"SDK_ALL_DEVICES_CANONICAL_OWNERSHIP");
            MethodInfo count=installations.GetType().GetMethod("get_Count",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
            ProbeFiles.Require(count!=null&&count.MetadataToken==0x060000a0&&(int)count.Invoke(installations,new object[0])==1,"SDK_ALL_DEVICES_SINGLE_ACTUAL_INSTALLATION");
            int members=0;var sequence=devices as System.Collections.IEnumerable;ProbeFiles.Require(sequence!=null,"SDK_ALL_DEVICES_ORIGINAL_ENUMERABLE");var iterator=sequence.GetEnumerator();try{while(iterator.MoveNext()){if(++members>1)throw new NotSupportedException("SDK_ALL_DEVICES_FIXED_FIXTURE_BOUND");ProbeFiles.Require(sdkDeviceRetained!=null&&Object.ReferenceEquals(iterator.Current,sdkDeviceRetained[3]),"SDK_ALL_DEVICES_ONLY_ORIGINAL_MEMBER");}}finally{IDisposable disposable=iterator as IDisposable;if(disposable!=null)disposable.Dispose();}
            Record("SDK_ALL_DEVICES_REFERENCE actual-installations=1 actual-members="+members+" membership=ORIGINAL_CANONICAL_COLLECTION no-invented-member=True");return devices;
        },delegate{return SdkWrapperEligible()||GuidProjectDisposalEligible();});
        Record("SDK_ALL_DEVICES_READY token=0600025A return=GENUINE_SINGLE_INSTALLATION_CANONICAL_DEVICE_COLLECTION");
        PrepareSdkCanonicalReferences(project,installations,installation);
    }
    internal static void WithSdkProductConversionResources(Assembly owner,Action invoke)
    {
        ProjectResourceRequire(!sdkProductConversionActive,"PRODUCT_CONVERSION_SCOPE_NESTED");sdkProductConversionActive=true;
        try{WithSdkProductImportResources(owner,invoke);}finally{sdkProductConversionActive=false;}
    }
    internal static void WithSdkProductImportResources(Assembly owner,Action invoke,bool importEngine=false)
    {
        ProjectResourceRequire(SdkProductMode&&ProductDependencyComplete&&SdkWrapperEligible()&&contextLifetime&&!sdkProductImportInvokeActive
            &&owner!=null&&!owner.IsDynamic&&!owner.ReflectionOnly&&!owner.GlobalAssemblyCache&&owner.FullName==OmIdentity
            &&ProbeFiles.Same(owner.Location,Path.Combine(stage,Names[8]+".dll"))&&invoke!=null,"PRODUCT_IMPORT_RESOURCE_SCOPE");
        sdkProductImportResourceOwner=owner;sdkProductImportInvokeActive=true;sdkProductImportEngineActive=importEngine;
        try{invoke();}finally{sdkProductImportEngineActive=false;sdkProductImportInvokeActive=false;sdkProductImportResourceOwner=null;}
    }
    static readonly object productMasterGate=new object();
    static UndoFactoryRegistrationApi productMasterApi;
    static Type productMasterType;static Delegate productMasterCallback;static object productMaster;
    static bool productMasterBound,productMasterFault;static int productMasterEntries,productMasterReplies;
    static void PrepareProductMaster(UndoFactoryRegistrationApi verified,Assembly om)
    {
        ProbeFiles.Require(SdkProductMode&&productMasterApi==null&&verified!=null,"PRODUCT_MASTER_PREPARE_ONCE");
        productMasterType=om.GetType("Knx.Ets.ObjectModel.MasterData.KnxMasterData",true);
        ProbeFiles.Require(productMasterType.IsPublic&&productMasterType.IsClass&&!productMasterType.IsGenericType&&productMasterType.Module==om.ManifestModule
            &&productMasterType.MetadataToken==0x02000c45&&om.FullName==OmIdentity,"PRODUCT_MASTER_ORIGINAL_TYPE");
        MethodInfo register=verified.Register.GetGenericMethodDefinition().MakeGenericMethod(productMasterType);Type closed=register.ReturnType;
        Type callback=typeof(Func<,>).MakeGenericType(verified.CallbackType.GetGenericArguments()[0],productMasterType);
        ProbeFiles.Require(register.GetParameters()[1].ParameterType==callback&&closed.GetGenericTypeDefinition()==verified.Return.GetGenericTypeDefinition()
            &&closed.GetGenericArguments()[0]==productMasterType&&closed.GetGenericArguments()[1]==verified.Return.GetGenericArguments()[1]
            &&closed.GetGenericArguments()[2]==verified.Return.GetGenericArguments()[2],"PRODUCT_MASTER_REGISTER_ABI");
        productMasterApi=new UndoFactoryRegistrationApi{Register=register,Return=closed,Contract=productMasterType,CallbackType=callback,Single=FactoryRegistrationMember(closed,"SingleInstance"),External=FactoryRegistrationMember(closed,"ExternallyOwned")};
        MethodInfo method=typeof(ProjectConstructorProbe).GetMethod("ProvideProductMaster",BindingFlags.NonPublic|BindingFlags.Static).MakeGenericMethod(productMasterType);
        productMasterCallback=Delegate.CreateDelegate(callback,method,true);
        ProbeFiles.Require(productMasterCallback.GetType()==callback&&productMasterCallback.Target==null&&productMasterCallback.Method.Equals(method),"PRODUCT_MASTER_TYPED_CALLBACK");
        Record("PRODUCT_MASTER_REFERENCE_READY state=UNBOUND source=ORIGINAL_MASTER_LOAD before-container-build=True");
    }
    static void RegisterProductMaster(object builder)
    {
        ProbeFiles.Require(SdkProductMode&&productMasterApi!=null&&productMasterCallback!=null,"PRODUCT_MASTER_REGISTER_PHASE");
        object registration=productMasterApi.Register.Invoke(null,new object[]{builder,productMasterCallback});
        ProbeFiles.Require(registration!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(registration)&&registration.GetType().Assembly==productMasterApi.Return.Assembly
            &&productMasterApi.Return.IsInstanceOfType(registration)&&Object.ReferenceEquals(registration,productMasterApi.Single.Invoke(registration,new object[0]))
            &&Object.ReferenceEquals(registration,productMasterApi.External.Invoke(registration,new object[0])),"PRODUCT_MASTER_SINGLE_EXTERNAL");
        Record("PRODUCT_MASTER_REGISTER_END deferred-original-reference=True SingleInstance=True ExternallyOwned=True before-build=True");
    }
    internal static void BindProductMaster(object value)
    {
        lock(productMasterGate){
            if(!SdkProductMode||!SdkWrapperEligible()||productMasterApi==null||productMasterBound||productMasterFault||value==null
                ||System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)||value.GetType()!=productMasterType){productMasterFault=true;throw new NotSupportedException("PRODUCT_MASTER_BIND_CONTEXT");}
            productMaster=value;productMasterBound=true;
        }
        Record("PRODUCT_MASTER_BOUND exact-loaded-original-instance=True callback-activation=NOT_YET_REQUIRED");
    }
    static T ProvideProductMaster<T>(object context) where T:class
    {
        int count;lock(productMasterGate){if(productMasterEntries<2)productMasterEntries++;count=productMasterEntries;}
        bool suspended=false;int previous=0;T result=null;
        try{
            bool eligible=SdkProductMode&&SdkWrapperEligible()&&sdkProductImportEngineActive,excluded=UndoFactoryCallbackExcluded();
            if(!excluded){previous=SuspendProjectResourceCallback();suspended=true;}
            lock(productMasterGate){
                if(count!=1||excluded||!eligible||productMasterFault||!productMasterBound||typeof(T)!=productMasterType||context==null
                    ||System.Runtime.Remoting.RemotingServices.IsTransparentProxy(context)||!productMasterApi.CallbackType.GetGenericArguments()[0].IsInstanceOfType(context)
                    ||context.GetType().Assembly!=productMasterApi.Register.DeclaringType.Assembly){productMasterFault=true;throw new NotSupportedException("PRODUCT_MASTER_REFERENCE_UNAVAILABLE");}
                result=(T)productMaster;
            }
            Record("PRODUCT_MASTER_REFERENCE_REPLY exact-loaded-original-instance=True service=KnxMasterData");
        }catch{lock(productMasterGate){productMasterFault=true;}throw;}
        finally{if(suspended){try{RestoreProjectResourceCallback(previous);}catch{lock(productMasterGate){productMasterFault=true;}throw;}}}
        lock(productMasterGate){if(productMasterFault){throw new NotSupportedException("PRODUCT_MASTER_CAUGHT_FAULT");}productMasterReplies++;}return result;
    }
    static void CheckProductMaster(string phase)
    {
        if(!SdkProductMode||productMasterApi==null)return;
        lock(productMasterGate){
            if(productMasterFault||SdkAuditLog(phase))Record("PRODUCT_MASTER_REFERENCE_AUDIT phase="+phase+" bound="+productMasterBound+" entries="+productMasterEntries+" replies="+productMasterReplies+" fault="+productMasterFault+" ownership=EXTERNAL");
            ProbeFiles.Require(!productMasterFault&&productMasterEntries<=1&&productMasterReplies<=1&&productMasterReplies<=productMasterEntries
                &&((phase!="PROCESS_FINAL"&&phase!="AFTER_ALL_MANAGED_CLEANUP")||productMasterBound&&productMaster!=null),"PRODUCT_MASTER_FINAL_DENIAL");GC.KeepAlive(productMaster);
        }
    }
    static object sdkProductManufacturer;
    static SdkProductApplicationPolicy sdkProductApplication;
    static SdkProductApplicationPolicy sdkProductStringStream;
    static SdkProductApplicationPolicy sdkProductFilePath,sdkProductContentWriter;
    static SdkProductApplicationPolicy[] sdkProductManufacturerReads,sdkProductProjectReads;
    static void PrepareSdkProductImport(Assembly om,object project,object product,object hp)
    {
        ProbeFiles.Require(SdkProductMode&&SdkWrapperEligible()&&sdkProductImport==null&&activeValidator!=null,"PRODUCT_IMPORT_HOST_PHASE");
        OriginalProductImportProbe.RequireDependencyReady(ProductDependencyComplete);
        Assembly common=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Common.dll")),autofac=Assembly.LoadFrom(Path.Combine(stage,"Autofac.dll"));
        string archive=Path.Combine(Path.GetDirectoryName(stage),OriginalProductImportProbe.ArchiveFile);
        sdkProductImport=new OriginalProductImportProbe(archive,Environment.CurrentDirectory,om,common,autofac,contextGenuineFactory,Record);
        Type provider=om.GetType("Knx.Ets.ObjectModel.ICacheProvider",true);MethodInfo[] methods={provider.GetMethod("HardwareCache"),provider.GetMethod("ApplicationProgramCache"),provider.GetMethod("CacheProvider"),provider.GetMethod("GetDynamicTranslations"),provider.GetMethod("GetParameterRefCache"),provider.GetMethod("GetComObjectRefCache"),provider.GetMethod("GetCalculatedParameterCache")};
        MethodInfo idGetter=product.GetType().GetMethod("get_ManufacturerId");ProbeFiles.Require(idGetter!=null&&idGetter.ReturnType==methods[0].GetParameters()[0].ParameterType&&idGetter.Module==product.GetType().Module,"PRODUCT_IMPORT_SELECTED_MANUFACTURER_API");
        object manufacturer=idGetter.Invoke(product,new object[0]);ProbeFiles.Require(manufacturer!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(manufacturer),"PRODUCT_IMPORT_SELECTED_MANUFACTURER");
        MethodInfo selectedAppGetter=SdkDeviceMethod(hp.GetType(),0x060003a5);Type appIdType=methods[3].GetParameters()[0].ParameterType;
        ProbeFiles.Require(hp.GetType().MetadataToken==0x0200004c&&hp.GetType().Assembly==product.GetType().Assembly&&selectedAppGetter.Name=="get_ApplicationProgramId"&&selectedAppGetter.ReturnType==appIdType&&selectedAppGetter.GetParameters().Length==0&&appIdType.FullName=="Knx.Ets.Ids.Product.ApplicationProgramId"&&appIdType.Module==hp.GetType().Module,"PRODUCT_IMPORT_SELECTED_APPLICATION_API");
        object selectedApp=selectedAppGetter.Invoke(hp,new object[0]);ProbeFiles.Require(selectedApp!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(selectedApp)&&selectedApp.GetType()==appIdType,"PRODUCT_IMPORT_SELECTED_APPLICATION_ID");
        int[] tokens={0x060001b6,0x060001b7,0x060001bf,0x060001be,0x060001ba,0x060001b8,0x060001bc};for(int i=0;i<methods.Length;i++)ProbeFiles.Require(methods[i]!=null&&methods[i].DeclaringType==provider&&methods[i].Module==om.ManifestModule&&methods[i].MetadataToken==tokens[i]&&methods[i].IsPublic&&!methods[i].IsStatic&&!methods[i].IsGenericMethod&&methods[i].GetParameters().Length==1&&methods[i].GetParameters()[0].ParameterType==(i>=3?appIdType:manufacturer.GetType())&&(int)methods[i].Attributes==1478,"PRODUCT_IMPORT_CONTEXT_EXACT_ABI");
        ProbeFiles.Require(methods[3].ReturnType==typeof(Dictionary<string,string>),"PRODUCT_IMPORT_TRANSLATION_RETURN_ABI");
        activeValidator.ConfigureProductCaches(methods,delegate(int selected,object actual){object expected=selected>=3?selectedApp:manufacturer;ProbeFiles.Require(actual!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actual)&&actual.GetType()==expected.GetType()&&Object.Equals(expected,actual),"PRODUCT_IMPORT_SELECTED_MANUFACTURER_OR_APPLICATION_ONLY");return sdkProductImport.Cache(selected,actual);});
        MethodInfo weakGetter=SdkDeviceMethod(provider,0x060001c4),weakSetter=SdkDeviceMethod(provider,0x060001c5);
        ProbeFiles.Require(provider.MetadataToken==0x0200002d&&provider.Module==om.ManifestModule&&weakGetter.Name=="get_AllowCachesToGoWeak"&&weakGetter.ReturnType==typeof(bool)&&weakGetter.GetParameters().Length==0&&(int)weakGetter.Attributes==3526&&weakSetter.Name=="set_AllowCachesToGoWeak"&&weakSetter.ReturnType==typeof(void)&&weakSetter.GetParameters().Length==1&&weakSetter.GetParameters()[0].ParameterType==typeof(bool)&&(int)weakSetter.Attributes==3526,"PRODUCT_CACHE_WEAK_EXACT_ABI");
        activeValidator.ConfigureProductCacheWeak(new MethodInfo[]{weakGetter,weakSetter},sdkProductImport.CacheWeakPolicy);
        MethodInfo releaseCaches=SdkDeviceMethod(provider,0x060001c0);ProbeFiles.Require(releaseCaches.Name=="DisposeCachedInformation"&&releaseCaches.ReturnType==typeof(void)&&releaseCaches.GetParameters().Length==0&&(int)releaseCaches.Attributes==1478,"PRODUCT_CACHE_RELEASE_EXACT_ABI");
        activeValidator.ConfigureProductCacheRelease(releaseCaches,sdkProductImport.DisposeCachedInformation);
        Record("PRODUCT_IMPORT_CONTEXT_READY exact-cache-methods=7 manufacturer-and-application=ACTUAL_SELECTED_PRODUCT_IDS values=NOT_LOGGED");
        Type objectContext=om.GetType("Knx.Ets.ObjectModel.IObjectContext",true);MethodInfo facadeGetter=SdkDeviceMethod(objectContext,0x06000268);
        Type facadeCacheType=typeof(Dictionary<,>).MakeGenericType(appIdType,typeof(WeakReference));ConstructorInfo facadeCtor=facadeCacheType.GetConstructor(Type.EmptyTypes);
        ProbeFiles.Require(objectContext.MetadataToken==0x02000044&&facadeGetter.Name=="get_ApplicationProgramFacadeCache"&&facadeGetter.GetParameters().Length==0&&facadeGetter.ReturnType==facadeCacheType&&(int)facadeGetter.Attributes==3526&&facadeCtor!=null&&facadeCtor.DeclaringType==facadeCacheType&&facadeCtor.Module==typeof(Dictionary<,>).Module,"PRODUCT_FACADE_CACHE_EXACT_BCL_ABI");
        object facadeCache=facadeCtor.Invoke(new object[0]);ProbeFiles.Require(facadeCache!=null&&facadeCache.GetType()==facadeCacheType&&((System.Collections.IDictionary)facadeCache).Count==0,"PRODUCT_FACADE_CACHE_INITIAL_EMPTY");
        activeValidator.ConfigureProductFacadeCache(facadeGetter,facadeCache);
        MethodInfo contextMaster=SdkDeviceMethod(objectContext,0x0600026b);Type factoryContract=om.GetType("Knx.Ets.ObjectModel.IFactory",true);
        ProbeFiles.Require(contextMaster.Name=="MasterData"&&contextMaster.ReturnType==productMasterType&&contextMaster.GetParameters().Length==1&&contextMaster.GetParameters()[0].ParameterType==factoryContract&&(int)contextMaster.Attributes==1478&&productMasterBound&&productMaster!=null&&productMaster.GetType()==productMasterType&&factoryContract.IsInstanceOfType(contextGenuineFactory),"PRODUCT_CONTEXT_MASTER_EXACT_LOADED_ABI");
        activeValidator.ConfigureProductMaster(contextMaster,productMaster,contextGenuineFactory);
        Type productStoreContract=om.GetType("Knx.Ets.ObjectModel.IProductStore",true);MethodInfo applicationStore=SdkDeviceMethod(productStoreContract,0x060002d9);
        ProbeFiles.Require(productStoreContract.MetadataToken==0x02000052&&applicationStore.Name=="ApplicationProgram"&&applicationStore.ReturnType==om.GetType("Knx.Ets.ObjectModel.IStore",true)&&applicationStore.GetParameters().Length==1&&applicationStore.GetParameters()[0].ParameterType==appIdType&&(int)applicationStore.Attributes==1478&&activeCache!=null&&sdkProductApplication==null,"PRODUCT_APPLICATION_STORE_EXACT_ABI");
        sdkProductApplication=new SdkProductApplicationPolicy(applicationStore,selectedApp,sdkProductImport.ApplicationProgram,Record,null,ProductDataReadLifetime);activeCache.ConfigureProductApplication(sdkProductApplication);
        string[] readNames={"ApplicationPrograms","Hardware","Baggages"};int[] readTokens={0x060002e0,0x060002d7,0x060002d8};sdkProductManufacturerReads=new SdkProductApplicationPolicy[3];
        for(int i=0;i<3;i++){MethodInfo readMethod=SdkDeviceMethod(productStoreContract,readTokens[i]);Type expected=i==0?typeof(IEnumerable<>).MakeGenericType(appIdType):applicationStore.ReturnType;ProbeFiles.Require(readMethod.Name==readNames[i]&&readMethod.ReturnType==expected&&readMethod.GetParameters().Length==1&&readMethod.GetParameters()[0].ParameterType==manufacturer.GetType()&&(int)readMethod.Attributes==1478,"PRODUCT_MANUFACTURER_READ_EXACT_ABI");int selectedRead=i;sdkProductManufacturerReads[i]=new SdkProductApplicationPolicy(readMethod,manufacturer,delegate(object id){return sdkProductImport.ManufacturerRead(selectedRead,id,selectedApp);},Record,i==0?typeof(List<>).MakeGenericType(appIdType):null,ProductDataReadLifetime);}
        activeCache.ConfigureProductManufacturerReads(sdkProductManufacturerReads);
        MethodInfo filePath=SdkDeviceMethod(productStoreContract,0x060002e4);
        ProbeFiles.Require(filePath.Name=="get_FilePath"&&filePath.ReturnType==typeof(string)&&filePath.GetParameters().Length==0&&(int)filePath.Attributes==3526&&sdkProductFilePath==null,"PRODUCT_FILE_PATH_EXACT_ABI");
        sdkProductFilePath=new SdkProductApplicationPolicy(filePath,null,sdkProductImport.FilePath,Record,typeof(string),ProductStoreDisposalLifetime);activeCache.ConfigureProductFilePath(sdkProductFilePath);
        Type contentType=om.GetType("Knx.Ets.ObjectModel.IStore",true),writerType=om.GetType("Knx.Ets.ObjectModel.IWriter",true);MethodInfo asWriter=SdkDeviceMethod(contentType,0x06000363);
        ProbeFiles.Require(contentType.MetadataToken==0x0200005e&&asWriter.Name=="AsWriter"&&asWriter.ReturnType==writerType&&asWriter.GetParameters().Length==0&&(int)asWriter.Attributes==1478&&activeContent!=null&&activeContent.Proxy is ProjectContentExistsProxy,"PRODUCT_CONTENT_WRITER_EXACT_ABI");
        sdkProductContentWriter=new SdkProductApplicationPolicy(asWriter,null,sdkProductImport.ProjectContentWriter,Record,null,ProductStoreDisposalLifetime);((ProjectContentExistsProxy)activeContent.Proxy).ConfigureProductWriter(sdkProductContentWriter);
        Type projectStoreContract=om.GetType("Knx.Ets.ObjectModel.IProjectStore",true);
        int[] saveTokens={0x060002fb,0x060002fc,0x060002fd,0x060002fe,0x060002ff,0x06000300,0x06000301,0x06000302,0x06000307,0x06000308,0x06000309,0x06000310,0x06000311,0x060002fa,0x06000303,0x06000304,0x06000305,0x06000306};
        string[] saveNames={"get_HistoryEntries","get_DeviceCertificates","get_ProjectTraces","get_IpSecurityDomains","get_GroupAddresses","get_BusAccess","get_Buildings","get_Areas","get_Trades","get_SplitInfos","get_UnassignedDevices","get_UserFiles","get_AddinDatas","get_Installation","get_AllAreas","get_AllUserFiles","get_AllDeviceBinaryData","get_AllAddInData"};
        sdkProductProjectReads=new SdkProductApplicationPolicy[18];string[] enumerationIds={"Knx.Ets.Ids.Project.AreaId","Knx.Ets.Ids.Project.UserFileId","Knx.Ets.Ids.Id","Knx.Ets.Ids.Project.AddinDataId"};
        for(int i=0;i<saveTokens.Length;i++){MethodInfo getter=SdkDeviceMethod(projectStoreContract,saveTokens[i]);Type element=i<14?null:appIdType.Assembly.GetType(enumerationIds[i-14],true,false),result=i<14?contentType:typeof(IEnumerable<>).MakeGenericType(element);
            ProbeFiles.Require(projectStoreContract.MetadataToken==0x02000057&&getter.Name==saveNames[i]&&getter.ReturnType==result&&getter.GetParameters().Length==0&&(int)getter.Attributes==3526,"PRODUCT_PROJECT_SAVE_GETTER_EXACT_ABI");int selected=i;
            sdkProductProjectReads[i]=new SdkProductApplicationPolicy(getter,null,delegate(object ignored){return sdkProductImport.ProjectStateRead(selected,ignored);},Record,i<14?null:typeof(List<>).MakeGenericType(element),ProductStoreDisposalLifetime);}
        activeCache.ConfigureProductProjectReads(sdkProductProjectReads);
        if(ProductDefinitionContextRequested()){
            MethodInfo contextStrings=SdkDeviceMethod(objectContext,0x06000258),storeStrings=SdkDeviceMethod(om.GetType("Knx.Ets.ObjectModel.IProjectStore",true),0x06000313);
            ProbeFiles.Require(contextStrings.Name=="get_LargeStringCache"&&contextStrings.ReturnType.MetadataToken==0x02000f24&&contextStrings.ReturnType.Module==om.ManifestModule&&contextStrings.GetParameters().Length==0&&(int)contextStrings.Attributes==3526&&storeStrings.Name=="get_LargeStringCache"&&storeStrings.ReturnType==om.GetType("Knx.Ets.ObjectModel.IStreamStore",true)&&storeStrings.GetParameters().Length==0,"PRODUCT_LARGE_STRING_CONTEXT_STORE_ABI");
            object[] strings=sdkProductImport.PrepareLargeStrings();activeValidator.ConfigureProductLargeStrings(contextStrings,strings[1]);
            sdkProductStringStream=new SdkProductApplicationPolicy(storeStrings,null,sdkProductImport.LargeStringStream,Record,null,ProductDataReadLifetime);activeCache.ConfigureProductStringStream(sdkProductStringStream);
        }
        if(ProductDefinitionContextRequested()){
            PrepareProductUiValidators(om,objectContext,project);
            MethodInfo contextDispatch=SdkDeviceMethod(objectContext,0x0600026a);ProbeFiles.Require(contextDispatch.Name=="CheckUiDispatcherAccess"&&contextDispatch.ReturnType==typeof(void)&&contextDispatch.GetParameters().Length==1&&contextDispatch.GetParameters()[0].ParameterType==typeof(Action)&&(int)contextDispatch.Attributes==1478&&sdkUi!=null,"PRODUCT_CONTEXT_UI_DISPATCH_EXACT_ABI");
            activeValidator.ConfigureProductDispatcher(contextDispatch,sdkUi.InvokeContextUiAction);
        }
        Record("PRODUCT_FACADE_CACHE_READY getter=06000268 implementation=ORIGINAL_DECLARED_BCL_DICTIONARY ownership=TRANSIENT_CONTEXT_WEAK_CACHE initial-count=0 population=ORIGINAL_SDK_UI_ONLY product-data=NOT_SUBSTITUTED");
        ProbeFiles.Require(productMasterBound&&productMaster!=null&&sdkProductManufacturer==null,"PRODUCT_IMPORT_MASTER_MANUFACTURER_PHASE");
        Type masterType=om.GetType("Knx.Ets.ObjectModel.MasterData.KnxMasterData",true),masterManufacturerType=om.GetType("Knx.Ets.ObjectModel.MasterData.KnxManufacturer",true),manufacturerType=om.GetType("Knx.Ets.ObjectModel.Product.Manufacturer",true);
        ProbeFiles.Require(productMaster.GetType()==masterType&&masterType.MetadataToken==0x02000c45&&manufacturerType.MetadataToken==0x0200044a,"PRODUCT_IMPORT_MASTER_MANUFACTURER_TYPES");
        MethodInfo masterManufacturers=SdkDeviceMethod(masterType,0x06007894),projectManufacturers=SdkDeviceMethod(project.GetType(),0x06002854);
        object masterCollection=masterManufacturers.Invoke(productMaster,new object[0]),projectCollection=projectManufacturers.Invoke(project,new object[0]);
        ProbeFiles.Require(masterCollection!=null&&projectCollection!=null&&masterCollection.GetType()==masterManufacturers.ReturnType&&projectCollection.GetType()==projectManufacturers.ReturnType,"PRODUCT_IMPORT_MANUFACTURER_COLLECTIONS");
        MethodInfo masterItem=SdkDeviceMethod(masterCollection.GetType(),0x0600788d);ParameterInfo[] key=masterItem.GetParameters();
        ProbeFiles.Require(masterItem.ReturnType==masterManufacturerType&&key.Length==1&&key[0].ParameterType.IsInstanceOfType(manufacturer),"PRODUCT_IMPORT_MASTER_MANUFACTURER_INDEX_ABI");
        object masterManufacturer=masterItem.Invoke(masterCollection,new object[]{manufacturer});
        ProbeFiles.Require(masterManufacturer!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(masterManufacturer)&&masterManufacturer.GetType()==masterManufacturerType,"PRODUCT_IMPORT_SELECTED_MASTER_MANUFACTURER_REQUIRED");
        MethodInfo masterId=masterManufacturerType.GetMethod("get_Id",BindingFlags.Public|BindingFlags.Instance,null,Type.EmptyTypes,null);
        ProbeFiles.Require(masterId!=null&&masterId.Module==om.ManifestModule&&masterId.MetadataToken==0x06000116&&Object.Equals(masterId.Invoke(masterManufacturer,new object[0]),manufacturer),"PRODUCT_IMPORT_SELECTED_MASTER_IDENTITY");
        ConstructorInfo ctor=manufacturerType.GetConstructor(new Type[]{om.GetType("Knx.Ets.ObjectModel.DomCollection",true),masterManufacturerType});
        ProbeFiles.Require(ctor!=null&&ctor.Module==om.ManifestModule&&ctor.MetadataToken==0x06002eb4&&(int)ctor.Attributes==6278&&ctor.IsPublic&&!ctor.IsStatic,"PRODUCT_IMPORT_MANUFACTURER_PUBLIC_CTOR");
        Record("PRODUCT_IMPORT_PROJECT_MANUFACTURER_BEGIN source=GENUINE_LOADED_MASTER selected-id=EXACT_NO_REMAP ctor=06002EB4");
        sdkProductManufacturer=ctor.Invoke(new object[]{projectCollection,masterManufacturer});
        ProbeFiles.Require(sdkProductManufacturer!=null&&sdkProductManufacturer.GetType()==manufacturerType&&Object.Equals(masterId.Invoke(sdkProductManufacturer,new object[0]),manufacturer),"PRODUCT_IMPORT_PROJECT_MANUFACTURER_IDENTITY");
        SdkAttachReferenceObject(projectCollection,sdkProductManufacturer,"Knx.Ets.ObjectModel.Product.ManufacturerCollection",0x0200044a);
        Record("PRODUCT_IMPORT_PROJECT_MANUFACTURER_END exact-genuine-instance=True source=ORIGINAL_MASTER hardware-products=ORIGINAL_IMPORTED_CACHES");
    }
    static void PrepareProductUiValidators(Assembly om,Type context,object project)
    {
        string[] names={"ComObjectInstance","AddinData"};int[] objectTokens={0x020001ff,0x0200018d},checkerTokens={0x02000e7e,0x02000e6d},ctorTokens={0x06008a09,0x060089a3};
        var cases=new ContextValidatorAdapter.UiValidatorType[2];Type parent=om.GetType("Knx.Ets.ObjectModel.Interfaces.INamedDomObject",true),baseObject=om.GetType("Knx.Ets.ObjectModel.Project.ProjectDomObject",true),validator=om.GetType("Knx.Ets.ObjectModel.ConsistencyRules.Validator",true);
        MethodInfo definition=context.Module.ResolveMethod(0x06000255) as MethodInfo;ProbeFiles.Require(definition!=null&&definition.DeclaringType==context&&definition.IsGenericMethodDefinition&&definition.GetGenericArguments().Length==1,"PRODUCT_UI_VALIDATOR_GENERIC_ABI");
        for(int i=0;i<cases.Length;i++){
            Type child=om.GetType("Knx.Ets.ObjectModel.Project."+names[i],true),checker=om.GetType("Knx.Ets.ObjectModel.ConsistencyRules."+(i==0?"ComObjectInstanceConsistencyChecker":"AddinDataConsistencyChecker"),true);
            ConstructorInfo ctor=checker.GetConstructor(new Type[]{i==0?parent:child});
            ProbeFiles.Require(child.MetadataToken==objectTokens[i]&&child.IsPublic&&!child.IsGenericType&&baseObject.IsAssignableFrom(child)&&checker.MetadataToken==checkerTokens[i]&&checker.IsPublic&&!checker.IsAbstract&&!checker.IsGenericType&&validator.IsAssignableFrom(checker)&&ctor!=null&&ctor.IsPublic&&!ctor.IsStatic&&ctor.MetadataToken==ctorTokens[i]&&(int)ctor.Attributes==6278,"PRODUCT_UI_VALIDATOR_FIXED_ORIGINAL_CONSTRUCTOR");
            cases[i]=new ContextValidatorAdapter.UiValidatorType{ObjectType=child,CheckerType=checker,Constructor=ctor,Method=definition.MakeGenericMethod(checker),Parent=i==0?SdkDeviceMethod(child,0x06001852):SdkDeviceMethod(om.GetType("Knx.Ets.ObjectModel.DomObject",true),0x06000114),Context=SdkDeviceMethod(baseObject,0x060029cd),Project=SdkDeviceMethod(baseObject,0x060029e4)};
        }
        activeValidator.ConfigureUiValidators(cases,project);Record("PRODUCT_UI_VALIDATORS_READY types=2 original-public-constructors=True cache=OBJECT_AND_CLOSED_TYPE max-objects=20000 other-types=DENIED profile=UI_OR_EXPLICIT_DEFINITION_READ");
    }
    static void CreateSdkReferenceDevice(object project,object sdkProject)
    {
        ProbeFiles.Require(SdkDeviceMode&&SdkWrapperEligible()&&sdkDeviceRetained==null&&contextGenuineFactory!=null,"SDK_DEVICE_FIXTURE_ONCE_PHASE");
        Assembly om=project.GetType().Assembly,ids=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Ids.dll")),common=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Common.Types.dll"));
        Type installationType=om.GetType("Knx.Ets.ObjectModel.Project.Installation",true),deviceType=om.GetType("Knx.Ets.ObjectModel.Project.Device",true),factoryType=om.GetType("Knx.Ets.ObjectModel.Factory",true);
        ProbeFiles.Require(om.FullName==OmIdentity&&installationType.MetadataToken==0x02000324&&deviceType.MetadataToken==0x02000230&&contextGenuineFactory.GetType()==factoryType,"SDK_DEVICE_ORIGINAL_TYPES");
        object hp=SdkReferenceId(ids,"Knx.Ets.Ids.Product.Hardware2ProgramId",0x0200004c,0x060003ab,ReferenceDcaManifestSelection.Hardware2ProgramId);
        object product=SdkReferenceId(ids,"Knx.Ets.Ids.Product.ProductId",0x0200005f,0x060004b3,"M-0083_H-193-1-O00EF_P-SCN.2DDA641.2E04");
        if(SdkProductMode)PrepareSdkProductImport(om,project,product,hp);
        MethodInfo projectIdGetter=SdkDeviceMethod(project.GetType(),0x06002836),installationsGetter=SdkDeviceMethod(project.GetType(),0x06002852);
        MethodInfo createInstallation=SdkDeviceMethod(factoryType,0x06000142),createDevice=SdkDeviceMethod(factoryType,0x0600014f),devicesGetter=SdkDeviceMethod(installationType,0x0600233d);
        object projectId=projectIdGetter.Invoke(project,new object[0]);Type idType=ids.GetType("Knx.Ets.Ids.Project.ProjectId",true);ParameterInfo[] installationArgs=createInstallation.GetParameters();
        ProbeFiles.Require(projectId!=null&&projectId.GetType()==idType&&installationArgs.Length==3&&installationArgs[0].ParameterType==typeof(string)&&installationArgs[1].ParameterType==typeof(ushort)&&installationArgs[2].ParameterType==idType&&createInstallation.ReturnType==installationType&&devicesGetter.ReturnType.FullName=="Knx.Ets.ObjectModel.Project.DeviceCollection","SDK_DEVICE_INSTALLATION_FACTORY_ABI");
        Type completion=common.GetType("Knx.Ets.Common.Types.Enumerations.CompletionStatus",true),serial=om.GetType("Knx.Ets.ObjectModel.Project.SerialNumber",true),ip=common.GetType("Knx.Ets.Common.Types.IPConfig",true);
        MethodInfo none=ip.GetMethod("get_None",BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);ProbeFiles.Require(common.FullName==CommonIdentity&&ip.MetadataToken==0x02000013&&ip.IsValueType&&none!=null&&none.MetadataToken==0x06000115&&(int)none.Attributes==2198&&none.ReturnType==ip&&none.GetParameters().Length==0,"SDK_DEVICE_GENUINE_IP_NONE");
        object ipNone=none.Invoke(null,new object[0]);ParameterInfo[] vector=createDevice.GetParameters();Type[] expected={hp.GetType(),product.GetType(),typeof(Nullable<byte>),typeof(string),typeof(string),typeof(Nullable<>).MakeGenericType(completion),typeof(string),typeof(string),typeof(Nullable<>).MakeGenericType(serial),typeof(bool),ip,typeof(string)};
        ProbeFiles.Require(vector.Length==expected.Length&&createDevice.ReturnType==deviceType,"SDK_DEVICE_FACTORY_VECTOR_COUNT");for(int i=0;i<vector.Length;i++)ProbeFiles.Require(vector[i].ParameterType==expected[i],"SDK_DEVICE_FACTORY_VECTOR:"+i);
        ProbeFiles.Require(vector[11].IsOptional&&vector[11].HasDefaultValue&&(int)vector[11].Attributes==4112&&vector[11].RawDefaultValue==null&&ipNone!=null&&ipNone.GetType()==ip,"SDK_DEVICE_FACTORY_VERIFIED_CONTEXT_DEFAULT");
        object installations=sdkDeviceInstallations;ProbeFiles.Require(sdkDeviceCollectionsAttempted,"SDK_DEVICE_COLLECTION_SETUP_REQUIRED");ProbeFiles.Require(installations!=null&&Object.ReferenceEquals(installationsGetter.Invoke(project,new object[0]),installations),"SDK_DEVICE_PROJECT_INSTALLATIONS_READY");Record("SDK_DEVICE_INSTALLATION_BEGIN token=06000142 id=OWNED_LOCAL_FIXTURE_1 physical-address=NOT_ASSIGNED");
        object installation=createInstallation.Invoke(contextGenuineFactory,new object[]{"OpenKNX Studio offline installation",(ushort)1,projectId});ProbeFiles.Require(installation!=null&&installation.GetType()==installationType,"SDK_DEVICE_INSTALLATION_EXACT_RETURN");
        Record("SDK_DEVICE_INSTALLATION_END returned=True exact-genuine-type=True");
        sdkDeviceRetained=new object[]{hp,product,installation,null,null,null,null,ipNone,null};BindSdkReferenceObject(installation,installations);SdkAttachReferenceObject(installations,installation,"Knx.Ets.ObjectModel.Project.InstallationCollection",0x02000324);
        object devices=devicesGetter.Invoke(installation,new object[0]);ProbeFiles.Require(devices!=null,"SDK_DEVICE_CANONICAL_COLLECTION");PrepareSdkAllDevices(project,installations,installation,devices);
        Record("SDK_REFERENCE_DEVICE_BEGIN token=0600014F product=SCN-DA641.04 application=M-0083_A-0155-40-4D9F-O00EF medium=MT-0_TP address=ABSENT_UNCOMMISSIONED_FIXTURE completion=ABSENT serial=ABSENT persistence=NOT_IMPLEMENTED");
        object device;ProbeFiles.Require(!sdkDeviceFactoryInvokeActive,"SDK_DEVICE_FACTORY_RESOURCE_PHASE_ONCE");sdkDeviceFactoryInvokeActive=true;
        try{device=createDevice.Invoke(contextGenuineFactory,new object[]{hp,product,null,"DALI Control 64 Gateway",String.Empty,null,"SCN-DA641.04 offline fixture",String.Empty,null,false,ipNone,Type.Missing});}finally{sdkDeviceFactoryInvokeActive=false;}
        ProbeFiles.Require(device!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(device)&&device.GetType()==deviceType,"SDK_DEVICE_GENUINE_FACTORY_RETURN");sdkDeviceRetained[3]=device;
        ProbeFiles.Require(Object.ReferenceEquals(devicesGetter.Invoke(installation,new object[0]),devices),"SDK_DEVICE_CANONICAL_COLLECTION_STABLE");BindSdkReferenceObject(device,devices);SdkAttachReferenceObject(devices,device,"Knx.Ets.ObjectModel.Project.DeviceCollection",0x02000230);
        bool parameterReads=SdkUiRequested||ownedParameterNegative;object sdkDevice=null,requiredDefinition=null;
        if(parameterReads)BeginParameterCacheReadScope();
        try{
        Type sdkType=sdkProject.GetType();MethodInfo sdkInstallations=SdkDeviceMethod(sdkType,0x0600141d);object sdkCollection=sdkInstallations.Invoke(sdkProject,new object[0]);Type sdkInstallationType=sdkType.Assembly.GetType("Knx.Ets.Sdk.Project.Installation",true),sdkDeviceType=sdkType.Assembly.GetType("Knx.Ets.Sdk.Project.Device",true);
        ProbeFiles.Require(sdkCollection!=null&&sdkInstallationType.MetadataToken==0x020001ba&&sdkDeviceType.MetadataToken==0x020001a0,"SDK_DEVICE_SDK_TYPES");object sdkInstallation=SdkSingleReferenceItem(sdkCollection,sdkInstallationType,0x0600133b);
        object sdkDevices=SdkDeviceMethod(sdkInstallationType,0x06001308).Invoke(sdkInstallation,new object[0]);ProbeFiles.Require(sdkDevices!=null,"SDK_DEVICE_SDK_DEVICE_COLLECTION");sdkDevice=SdkSingleReferenceItem(sdkDevices,sdkDeviceType,0x060011af);
        sdkDeviceRetained[4]=sdkCollection;sdkDeviceRetained[5]=sdkInstallation;sdkDeviceRetained[6]=sdkDevice;Record("SDK_REFERENCE_DEVICE_OBTAINED exact-genuine-wrapper=True before-dependency-postcondition=True dependency-clean=NOT_ASSERTED GetDcaUi=NOT_RUN");TraceDependencyGuard(unexpected,rejected,"SDK_REFERENCE_DEVICE_POST");
        Record("SDK_REFERENCE_DEVICE_END returned=True exact-genuine-wrapper=True original-product-lookups=NOT_QUALIFIED GetDcaUi=NOT_RUN hardware=NOT_RUN");
        if(ownedStableIdentity){ObserveStableFixtureIdentity(project,installation,device,installations,devices);return;}
        if(ownedMissingDevice){RunMissingSdkDeviceNegative(sdkDevice);return;}
        MethodInfo productGetter=SdkDeviceMethod(sdkDeviceType,0x060010e2);ProbeFiles.Require(productGetter.GetParameters().Length==0&&productGetter.ReturnType.Module==sdkDeviceType.Module&&productGetter.ReturnType.FullName=="Knx.Ets.Sdk.Product.Product","SDK_DEVICE_REFERENCE_PRODUCT_READ_ABI");
        Record("SDK_REFERENCE_PRODUCT_READ_BEGIN token=060010E2 operation=OFFLINE_LOCAL_MODEL_READ GetDcaUi=NOT_RUN");object actualProduct=productGetter.Invoke(sdkDevice,new object[0]);
        Record("SDK_REFERENCE_PRODUCT_READ_END returned="+(actualProduct!=null)+" genuine="+(actualProduct!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actualProduct)&&actualProduct.GetType()==productGetter.ReturnType)+" application-match=NOT_QUALIFIED");
        ProbeFiles.Require(actualProduct!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actualProduct)&&actualProduct.GetType()==productGetter.ReturnType,"SDK_DEVICE_REFERENCE_PRODUCT_NOT_AVAILABLE");sdkDeviceRetained[8]=actualProduct;
        if(parameterReads)requiredDefinition=OriginalRequiredParameterDefinition(sdkDevice);
        }finally{if(parameterReads)EndParameterCacheReadScope();}
        if(parameterReads){
            if(ownedParameterNegative)RunMissingParameterDefinitionNegative(requiredDefinition);
            else ReferenceDcaRequiredParameters.BeforeUi(requiredDefinition,delegate{ShowSdkProductUi(sdkDevice);});
        }
    }
    // Observation only: normal original Bind creates IDs; no ID is injected or rewritten.
    static bool ownedStableIdentity;
    static bool ownedMissingDevice;
    static bool FixtureObservationProfile(){return ownedStableIdentity||ownedMissingDevice;}
    static string StableFixtureId(object value,Type exact)
    {
        ProbeFiles.Require(value!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(value)&&value.GetType()==exact&&exact.Assembly.FullName==Identities[11],"STABLE_FIXTURE_ORIGINAL_ID_TYPE");
        string text=value.ToString();ProbeFiles.Require(!String.IsNullOrEmpty(text)&&text.Length<=256,"STABLE_FIXTURE_ID_BOUND");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }
    static void ObserveStableFixtureIdentity(object project,object installation,object device,object installations,object devices)
    {
        ProbeFiles.Require(ownedStableIdentity&&SdkProductMode&&!SdkUiRequested&&!ownedParameterNegative&&ownedSelectionNegativeCase==null&&SdkWrapperEligible()&&contextLifetime&&sdkDeviceRetained!=null,"STABLE_FIXTURE_OBSERVATION_PHASE");
        Assembly om=project.GetType().Assembly,ids=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Ids.dll"));
        Type dom=om.GetType("Knx.Ets.ObjectModel.DomObject",true),context=om.GetType("Knx.Ets.ObjectModel.IObjectContext",true);
        MethodInfo projectId=SdkDeviceMethod(project.GetType(),0x06002836),childId=SdkDeviceMethod(dom,0x06000116);
        ProbeFiles.Require(projectId.GetParameters().Length==0&&childId.GetParameters().Length==0&&SdkContainsExact(installations,installation)&&SdkContainsExact(devices,device),"STABLE_FIXTURE_LIVE_MEMBERSHIP");
        object p=projectId.Invoke(project,new object[0]),i=childId.Invoke(installation,new object[0]),d=childId.Invoke(device,new object[0]);
        string projectText=StableFixtureId(p,ids.GetType("Knx.Ets.Ids.Project.ProjectId",true)),installationText=StableFixtureId(i,ids.GetType("Knx.Ets.Ids.Project.InstallationId",true)),deviceText=StableFixtureId(d,ids.GetType("Knx.Ets.Ids.Project.DeviceId",true));
        ProbeFiles.Require(Object.Equals(p,projectId.Invoke(project,new object[0]))&&Object.Equals(i,childId.Invoke(installation,new object[0]))&&Object.Equals(d,childId.Invoke(device,new object[0])),"STABLE_FIXTURE_REPEATED_PUBLIC_IDS");
        object receiver=contextRetained[4];MethodInfo projectCallback=SdkDeviceMethod(context,0x06000261),installationCallback=SdkDeviceMethod(context,0x06000262),creatorCallback=SdkDeviceMethod(context,0x06000259),puidCallback=SdkDeviceMethod(context,0x06000269);
        ProbeFiles.Require(Object.ReferenceEquals(projectCallback.Invoke(receiver,new object[0]),project)&&Object.ReferenceEquals(projectCallback.Invoke(receiver,new object[0]),project)&&Object.ReferenceEquals(installationCallback.Invoke(receiver,new object[0]),installation)&&Object.ReferenceEquals(installationCallback.Invoke(receiver,new object[0]),installation),"STABLE_FIXTURE_CALLBACK_REFERENCES");
        object creator=creatorCallback.Invoke(receiver,new object[0]);ProbeFiles.Require(creator!=null&&creator.GetType()==om.GetType("Knx.Ets.ObjectModel.UniqueIdCreator",true)&&Object.ReferenceEquals(creatorCallback.Invoke(receiver,new object[0]),creator),"STABLE_FIXTURE_CALLBACK_CREATOR_REFERENCE");
        object first=puidCallback.Invoke(receiver,new object[0]),second=puidCallback.Invoke(receiver,new object[0]);ProbeFiles.Require(first is int&&second is int&&(int)first>=1&&(int)second<=16&&(int)second==(int)first+1,"STABLE_FIXTURE_CALLBACK_PUID_SEQUENCE");
        int processId;using(Process process=Process.GetCurrentProcess())processId=process.Id;
        Record("OWNED_STABLE_IDENTITY process="+processId+" project="+projectText+" installation="+installationText+" device="+deviceText+" puid-first="+first+" puid-second="+second+" encoding=UTF8_BASE64 identity=OFFLINE_TEST_NOT_PHYSICAL");
        Record("OWNED_STABLE_CALLBACKS project-same=True installation-same=True creator-same=True repeated-public-IDs=True puid-consecutive=True source=ACTUAL_TYPED_REMOTING_CALLBACKS GetDcaUi=NOT_RUN");
        GC.KeepAlive(creator);GC.KeepAlive(receiver);
    }
    static bool ownedParameterNegative;
    [ThreadStatic] static bool sdkParameterDefinitionReadActive;
    static readonly ParameterReadPublication sdkParameterReadPublication=new ParameterReadPublication();
    [ThreadStatic] static int sdkParameterInvocationDepth;
    sealed class ParameterInvocationLease:IDisposable
    {
        IDisposable lease;readonly int thread=Thread.CurrentThread.ManagedThreadId;
        internal ParameterInvocationLease(IDisposable value){lease=value;}
        public void Dispose(){IDisposable value=Interlocked.Exchange(ref lease,null);if(value==null)return;try{ProbeFiles.Require(Thread.CurrentThread.ManagedThreadId==thread&&sdkParameterInvocationDepth>0,"PARAMETER_READ_LEASE_THREAD");sdkParameterInvocationDepth--;}finally{value.Dispose();}}
    }
    internal static IDisposable AcquireParameterReadInvocation()
    {
        if(!ProductDefinitionContextRequested())return null;
        IDisposable lease=sdkParameterReadPublication.TryEnter();if(lease==null)return null;
        sdkParameterInvocationDepth++;return new ParameterInvocationLease(lease);
    }
    internal static bool ParameterNegativeProfile(){return SdkProductMode&&ownedParameterNegative&&!SdkUiRequested;}
    internal static bool ProductDefinitionContextRequested(){return SdkProductMode&&(SdkUiRequested||ownedParameterNegative);}
    internal static bool ProductDataReadLifetime(){return ProductUiCacheLifetime()||ParameterDefinitionCacheLifetime();}
    internal static bool ParameterDefinitionCacheLifetime()
    {return sdkParameterInvocationDepth>0&&sdkParameterReadPublication.Accepting&&SdkProductMode&&ProductDependencyComplete&&contextLifetime;}
    internal static bool ParameterDefinitionCacheEligible()
    {return ParameterDefinitionCacheLifetime()&&projectResourceWindow==0&&!projectResourceCallbackSuspended&&(Thread.CurrentThread.ManagedThreadId!=sdkOwner||SdkWrapperEligible());}
    static void BeginParameterCacheReadScope()
    {
        ProbeFiles.Require(SdkProductMode&&ProductDependencyComplete&&contextLifetime&&SdkWrapperEligible()&&!sdkProductUiActive&&activeValidator!=null&&!sdkParameterReadPublication.Started,"PARAMETER_CACHE_SCOPE_OWNER_ENTRY");
        sdkParameterReadPublication.Begin();
        try{Record("DCA_PARAMETER_CACHE_SCOPE_BEGIN owner-validated=True SDK-wrapper-and-definition-read=True worker-service=VERIFIED_READ_REFERENCES_AND_REAL_DISPATCH owner-validators=GENUINE_FIXED_FAMILY2 UI-active=False context-cap="+ContextAdapterLimit()+" cache-cap="+ProductUiCacheRequestLimit()+" creator-cap="+ProductUiCreatorLimit()+" write-cap="+ProductUiWriteLimit()+" healthy-reply-logs=FIRST_PER_METHOD");}
        catch{sdkParameterReadPublication.Close();throw;}
    }
    static void EndParameterCacheReadScope()
    {
        bool quiescent=false;
        try{sdkParameterReadPublication.Close();quiescent=true;}
        finally{Record("DCA_PARAMETER_CACHE_SCOPE_END active=False admission=CLOSED inflight="+sdkParameterReadPublication.Inflight+" quiescent="+quiescent+" fence-fault="+sdkParameterReadPublication.Faulted+" wait=NONE_FAIL_CLOSED");}
    }
    internal static bool ParameterDefinitionReadEligible()
    {return sdkParameterDefinitionReadActive&&sdkParameterInvocationDepth>0&&SdkProductMode&&ProductDependencyComplete&&contextLifetime&&SdkWrapperEligible();}
    static object OriginalRequiredParameterDefinition(object device)
    {
        ProbeFiles.Require(!sdkParameterDefinitionReadActive&&!sdkProductUiActive&&SdkProductMode&&ProductDependencyComplete&&SdkWrapperEligible()&&contextLifetime&&sdkParameterReadPublication.Accepting,"DCA_PARAMETER_READ_SCOPE_ENTRY");
        bool returned=false;sdkParameterDefinitionReadActive=true;
        try{Record("DCA_PARAMETER_READ_SCOPE_BEGIN owner-only=True operations=EXISTING_VERIFIED_READ_SERVICES owner-validators=GENUINE_FIXED_FAMILY2 dispatcher=REAL_WPF UI-active=False values=NOT_READ");activeValidator.ParameterReadEntrySnapshot();object definition=OriginalRequiredParameterDefinitionCore(device);returned=true;return definition;}
        finally{sdkParameterDefinitionReadActive=false;Record("DCA_PARAMETER_READ_SCOPE_END returned="+returned+" active=False");}
    }
    static object OriginalRequiredParameterDefinitionCore(object device)
    {
        ProbeFiles.Require(SdkProductMode&&SdkWrapperEligible()&&contextLifetime&&device!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(device),"DCA_PARAMETER_PREFLIGHT_PHASE");
        Assembly sdk=device.GetType().Assembly;
        ProbeFiles.Require(sdk.FullName==Identities[1]&&!sdk.IsDynamic&&!sdk.ReflectionOnly&&!sdk.GlobalAssemblyCache&&ProbeFiles.Same(sdk.Location,Path.Combine(stage,"Knx.Ets.Sdk.dll")),"DCA_PARAMETER_PINNED_SDK");
        Type deviceType=sdk.GetType("Knx.Ets.Sdk.Project.Device",true),collectionType=sdk.GetType("Knx.Ets.Sdk.Project.ParameterInstanceRefCollection",true),instanceType=sdk.GetType("Knx.Ets.Sdk.Project.ParameterInstanceRef",true),definitionType=sdk.GetType("Knx.Ets.Sdk.Product.ParameterRef",true);
        ProbeFiles.Require(device.GetType()==deviceType&&deviceType.MetadataToken==0x020001a0&&collectionType.MetadataToken==0x020001ce&&instanceType.MetadataToken==0x020001ca&&definitionType.MetadataToken==0x0200024b,"DCA_PARAMETER_ORIGINAL_TYPES");
        MethodInfo collectionGetter=SdkDeviceMethod(deviceType,0x060010df),item=SdkDeviceMethod(collectionType,0x060013f7),definitionGetter=SdkDeviceMethod(instanceType,0x060013cf),nameGetter=SdkDeviceMethod(definitionType,0x060019c8);
        ProbeFiles.Require(collectionGetter.Name=="get_ParameterInstanceRefs"&&collectionGetter.ReturnType==collectionType&&collectionGetter.GetParameters().Length==0
            &&item.Name=="get_Item"&&item.ReturnType==instanceType&&item.GetParameters().Length==1&&item.GetParameters()[0].ParameterType==typeof(ushort)
            &&definitionGetter.Name=="get_ParameterRef"&&definitionGetter.ReturnType==definitionType&&definitionGetter.GetParameters().Length==0
            &&nameGetter.Name=="get_Name"&&nameGetter.ReturnType==typeof(string)&&nameGetter.GetParameters().Length==0,"DCA_PARAMETER_EXACT_READ_ABI");
        object collection=collectionGetter.Invoke(device,new object[0]);ProbeFiles.Require(collection!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(collection)&&collection.GetType()==collectionType,"DCA_REQUIRED_PARAMETER_COLLECTION_MISSING");
        object instance=item.Invoke(collection,new object[]{(ushort)0});ProbeFiles.Require(instance!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(instance)&&instance.GetType()==instanceType,"DCA_REQUIRED_PARAMETER_INSTANCE_MISSING");
        object definition=definitionGetter.Invoke(instance,new object[0]);
        if(definition==null)throw new InvalidDataException("DCA_REQUIRED_PARAMETER_DEFINITION_MISSING");
        ProbeFiles.Require(!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(definition)&&definition.GetType()==definitionType,"DCA_PARAMETER_GENUINE_DEFINITION");
        string name=nameGetter.Invoke(definition,new object[0]) as string;ProbeFiles.Require(!String.IsNullOrEmpty(name)&&name.Length<=512,"DCA_PARAMETER_DEFINITION_NAME_MISSING");
        Record("DCA_REQUIRED_PARAMETER_DEFINITION_READY source=ORIGINAL_SDK original-DCA-first-access=06000123 index=0 instance=060013F7 definition=060013CF type=0200024B name-present=True parameter-values=NOT_READ GetDcaUi=NOT_INVOKED");
        return definition;
    }
    static void RunMissingParameterDefinitionNegative(object actualDefinition)
    {
        ProbeFiles.Require(ownedParameterNegative&&!SdkUiRequested&&!sdkUiCallAttempted&&actualDefinition!=null,"PARAMETER_NEGATIVE_PROFILE");
        int positive=0,forbiddenUiEntries=0;
        ReferenceDcaRequiredParameters.BeforeUi(actualDefinition,delegate{positive++;});
        bool denied=false;
        try{ReferenceDcaRequiredParameters.BeforeUi(null,delegate{forbiddenUiEntries++;throw new InvalidDataException("PARAMETER_NEGATIVE_UI_GUARD_BYPASS");});}
        catch(InvalidDataException error){if(error.Message!="DCA_REQUIRED_PARAMETER_DEFINITION_MISSING")throw;denied=true;}
        ProbeFiles.Require(positive==1&&denied&&forbiddenUiEntries==0&&!sdkUiCallAttempted&&!sdkUiCallReturned&&!unexpected&&!rejected,"PARAMETER_NEGATIVE_BEFORE_UI");
        Record("OWNED_PARAMETER_DEFINITION_NEGATIVE_PASS genuine-SDK-definition-control=1 omission=OWNED_CONTEXT_VIEW_ONLY rejection=DCA_REQUIRED_PARAMETER_DEFINITION_MISSING original-GetDcaUi-attempts=0 UI-config-action-entries=0 model-fixture=ALREADY_CONSTRUCTED vendor-missing-definition-behavior=NOT_RUN original-data=UNCHANGED");
        GC.KeepAlive(actualDefinition);
    }
    static int sdkUiBoundaryEntries;
    static void RunMissingSdkDeviceNegative(object actualDevice)
    {
        ProbeFiles.Require(ownedMissingDevice&&SdkProductMode&&!SdkUiRequested&&!ownedStableIdentity&&!ownedParameterNegative&&SdkWrapperEligible()&&sdkUiBoundaryEntries==0&&!sdkUiCallAttempted,"DEVICE_NEGATIVE_PROFILE");
        Type original=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Sdk.dll")).GetType("Knx.Ets.Sdk.Project.Device",true);
        ProbeFiles.Require(actualDevice!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actualDevice)&&actualDevice.GetType()==original&&original.MetadataToken==0x020001a0&&Object.ReferenceEquals(actualDevice,sdkDeviceRetained[6]),"DEVICE_NEGATIVE_GENUINE_CONTROL");
        int positive=0;ReferenceDcaRequiredDevice.BeforeUi(actualDevice,delegate{positive++;});
        bool denied=false;try{ShowSdkProductUi(null);}catch(InvalidDataException error){if(error.Message!="DCA_REQUIRED_SDK_DEVICE_MISSING")throw;denied=true;}
        ProbeFiles.Require(positive==1&&denied&&sdkUiBoundaryEntries==0&&!sdkUiCallAttempted&&!sdkUiCallReturned&&!unexpected&&!rejected,"DEVICE_NEGATIVE_BEFORE_UI");
        Record("OWNED_UI_DEVICE_NEGATIVE_PASS genuine-SDK-device-control=1 omission=OWNED_UI_VIEW_ONLY rejection=DCA_REQUIRED_SDK_DEVICE_MISSING production-UI-boundary-entries=0 original-GetDcaUi-attempts=0 original-device=UNCHANGED vendor-missing-device-behavior=NOT_RUN");
        GC.KeepAlive(actualDevice);
    }
    static void ShowSdkProductUi(object device)
    {
        ReferenceDcaRequiredDevice.BeforeUi(device,delegate{sdkUiBoundaryEntries++;ShowSdkProductUiCore(device);});
    }
    static void ShowSdkProductUiCore(object device)
    {
        ProbeFiles.Require(SdkUiRequested&&SdkWrapperEligible()&&contextLifetime&&sdkDcaInstance!=null&&sdkInitialization!=null
            &&Thread.CurrentThread.GetApartmentState()==ApartmentState.STA&&Environment.UserInteractive,"DCA_UI_EXPLICIT_ACTIVE_STA_PHASE");
        using(Process process=Process.GetCurrentProcess())ProbeFiles.Require(process.SessionId!=0,"DCA_UI_INTERACTIVE_SESSION");
        Type deviceType=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Sdk.dll")).GetType("Knx.Ets.Sdk.Project.Device",true);
        ProbeFiles.Require(device!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(device)&&device.GetType()==deviceType&&deviceType.MetadataToken==0x020001a0,"DCA_UI_GENUINE_DEVICE_ONLY");
        MethodInfo method=sdkDcaInstance.GetType().Module.ResolveMethod(0x0600001e) as MethodInfo;Type sequence=typeof(IEnumerable<>).MakeGenericType(deviceType);
        ProbeFiles.Require(method!=null&&method.DeclaringType==sdkDcaInstance.GetType()&&method.Module.Assembly.FullName==Identities[0]&&method.Name=="GetDcaUi"&&method.IsPublic&&!method.IsStatic&&!method.IsGenericMethod&&method.ReturnType==typeof(FrameworkElement)&&method.GetParameters().Length==1&&method.GetParameters()[0].ParameterType==sequence,"DCA_UI_ORIGINAL_TYPED_ABI");
        Array devices=Array.CreateInstance(deviceType,1);devices.SetValue(device,0);
        int owner=Thread.CurrentThread.ManagedThreadId,firstChance=0;bool recording=false,diagnosticFault=false,visible=false,expired=false,closed=false;Window window=null;FrameworkElement view=null;
        DispatcherTimer timer=null;
        EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs> observe=delegate(object sender,System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args){
            if(Thread.CurrentThread.ManagedThreadId!=owner||recording)return;recording=true;
            try{if(firstChance<9)firstChance++;if(firstChance<=8){Exception error=args.Exception;MethodBase site=error.TargetSite;Record("DCA_UI_FIRST_CHANCE number="+firstChance+" type="+Text(error.GetType().FullName)+" message="+Text(error.Message)+" owner="+Text(site==null||site.DeclaringType==null?"UNKNOWN":site.DeclaringType.FullName)+" token="+(site==null?"UNKNOWN":site.MetadataToken.ToString("X8"))+" method="+Text(site==null?"UNKNOWN":site.Name));}}
            catch{diagnosticFault=true;}finally{recording=false;}
        };
        ProbeFiles.Require(SdkWrapperEligible()&&contextLifetime&&ProductDependencyComplete&&!sdkProductUiActive,"DCA_UI_ACTIVE_ONCE_OWNER_VALIDATED");sdkProductUiActive=true;
        AppDomain.CurrentDomain.FirstChanceException+=observe;
        try{
        try{
            Record("DCA_UI_CALLBACK_POLICY context-cap="+ContextAdapterLimit()+" project-reference-cap="+ProductUiProjectReferenceLimit()+" other-context-cap="+ProductUiOtherContextLimit()+" cache-cap="+ProductUiCacheRequestLimit()+" creator-cap="+ProductUiCreatorLimit()+" write-cap="+ProductUiWriteLimit()+" "+ContextBudgetModeDescription());
            TraceDependencyGuard(unexpected,rejected,"DCA_UI_PRE");Record("DCA_UI_BEGIN token=0600001E devices=1 genuine=True application-match=NOT_QUALIFIED bus-capability=NONE");
            sdkUiCallAttempted=true;view=method.Invoke(sdkDcaInstance,new object[]{devices}) as FrameworkElement;sdkUiCallReturned=true;
            ProbeFiles.Require(view!=null&&view.GetType().Assembly==sdkDcaInstance.GetType().Assembly&&view.Parent==null,"DCA_UI_ORIGINAL_VIEW_REQUIRED");sdkUiOriginalViewReturned=true;
            sdkProductImport.Audit("DCA_UI_RETURNED");
            Record("DCA_UI_RETURNED genuine-original-view=True view-type="+Text(view.GetType().FullName)+" device-qualified=NOT_ASSERTED");TraceDependencyGuard(unexpected,rejected,"DCA_UI_RETURNED");
            window=new Window{Title="OpenKNX Studio - original MDT DALI UI - offline fixture",Content=view,Width=1200,Height=800};
            timer=new DispatcherTimer(DispatcherPriority.Normal,Dispatcher.CurrentDispatcher);timer.Interval=TimeSpan.FromSeconds(sdkUiSeconds);
            timer.Tick+=delegate{timer.Stop();expired=true;Record("DCA_UI_HOLD_EXPIRED seconds="+sdkUiSeconds+" action=CLOSE_OWNED_WINDOW");window.Close();};
            window.ContentRendered+=delegate{if(visible)return;ProbeFiles.Require(window.IsVisible&&view.IsVisible&&window.Dispatcher.CheckAccess(),"DCA_UI_VISIBLE_RENDERED_REQUIRED");visible=true;sdkUiRendered=true;Record("DCA_UI_VISIBLE original-control=True rendered=True hold-seconds="+sdkUiSeconds+" close=USER_OR_DEADLINE hardware=NOT_RUN application-match=NOT_QUALIFIED");timer.Start();};
            window.Closed+=delegate{closed=true;sdkUiWindowClosed=true;timer.Stop();};
            window.ShowDialog();
            ProbeFiles.Require(visible&&closed&&!diagnosticFault,"DCA_UI_VISIBLE_CLOSED_DIAGNOSTICS");TraceDependencyGuard(unexpected,rejected,"DCA_UI_CLOSED");
            Record("DCA_UI_CLOSED visible=True reason="+(expired?"HOLD_DEADLINE":"USER_OR_ORIGINAL_CLOSE")+" original-ui=True configuration-acceptance=NOT_ESTABLISHED");
        }finally{
            sdkProductUiActive=false;
            AppDomain.CurrentDomain.FirstChanceException-=observe;
            if(timer!=null)timer.Stop();if(window!=null){try{if(window.IsVisible)window.Close();}finally{window.Content=null;}}
            IDisposable dispose=view as IDisposable;if(dispose!=null){Record("DCA_UI_VIEW_DISPOSE_BEGIN owner=HOST_RETURNED_VIEW");try{dispose.Dispose();}finally{Record("DCA_UI_VIEW_DISPOSE_END attempted=True");}}
            GC.KeepAlive(view);GC.KeepAlive(device);GC.KeepAlive(devices);
        }
        }catch{sdkUiLifecycleFault=true;throw;}
    }
    static void ProbeSdkProject(object project,object store)
    {
        ProbeFiles.Require(SdkOriginal&&!sdkWrapperPhase&&projectResourceWindow==0&&!projectResourceCallbackSuspended&&sdkProbeAttempts==0,"SDK_WRAPPER_PHASE");sdkProbeAttempts++;
        try{Assembly sdk=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Sdk.dll"));var api=SdkApi(sdk.GetType("Knx.Ets.Sdk.Root",true),sdk.GetType("Knx.Ets.Sdk.Project.ProjectCollection",true),sdk.GetType("Knx.Ets.Sdk.Project.Project",true),project.GetType(),project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.IProjectStore",true),sdk.GetType("Knx.Ets.Sdk.ExecutionContext",true),project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.Compatibility.IPluginInstaller",true),true);
            sdkWrapperPhase=true;AttachSdkProjectFixture(project,contextRetained[6],project.GetType().Assembly.GetType("Knx.Ets.ObjectModel.Interfaces.INamedDomObject",true),true);if(SdkDeviceMode)InitializeSdkDeviceCollections(project);if(SdkInitializing)PrepareSdkFixtureName(project);ConstructSdkRoute(api,project,store);if(SdkInitializing)InitializeSdkProject(sdkRetained[2]);if(SdkDeviceMode)CreateSdkReferenceDevice(project,sdkRetained[2]);TraceDependencyGuard(unexpected,rejected,"SDK_WRAPPER_POST");}
        catch(Exception error){sdkProbeFault=true;Record("SDK_WRAPPER_FAILED type="+error.GetType().FullName+" message="+Text(error.Message)+" ui="+UiOutcome());Errors(error);Exception cause=error;for(int depth=0;cause!=null&&depth<8;depth++,cause=cause.InnerException){try{MethodBase site=cause.TargetSite;if(site!=null)Record("SDK_EXCEPTION_SITE depth="+depth+" owner="+Text(site.DeclaringType==null?"GLOBAL":site.DeclaringType.FullName)+" token="+site.MetadataToken.ToString("X8")+" name="+Text(site.Name));}catch(Exception diagnostic){Record("SDK_EXCEPTION_SITE_UNAVAILABLE type="+diagnostic.GetType().FullName);}}throw;}
        finally{if(SdkUiRequested&&sdkInitialization!=null)sdkInitialization.Close();sdkWrapperPhase=false;GC.KeepAlive(sdkRetained);}
    }
    static void OwnedSdkFixture()
    {
        Type importerType=typeof(Func<object,object,object,object,object,object,object,object>);
        var importer=new ContextImporterDenyHolder(importerType,typeof(object),Record);
        Delegate sameImporter=importer.Callback;
        ProbeFiles.Require(Object.ReferenceEquals(sameImporter,importer.Callback)&&sameImporter.GetType()==importerType,"OWNED_SDK_IMPORTER_REFERENCE");
        bool importerDenied=false;
        try{((Func<object,object,object,object,object,object,object,object>)sameImporter)(null,null,null,null,null,null,null);}
        catch(NotSupportedException error){importerDenied=error.Message=="CONTEXT_IMPORTER_UNSUPPORTED";}
        ProbeFiles.Require(importerDenied&&importer.State.Entries==1&&importer.State.Faulted&&!UndoFactoryCallbackExcluded(),"OWNED_SDK_IMPORTER_DENIAL");
        Record("OWNED_SDK_IMPORTER_REFERENCE_PASS same-existing-delegate=True invocation-denied=True fixture-only=True");
        string hawkPath=Path.Combine(stage,SdkHawkFile);
        ProbeFiles.Require(SdkHawkResolvePolicy("original-sdk-project-fixture",true,SdkHawkIdentity,SdkHawkIdentity)&&!SdkHawkResolvePolicy("original-project-guid-fixture",true,SdkHawkIdentity,SdkHawkIdentity)&&!SdkHawkResolvePolicy("original-sdk-project-fixture",false,SdkHawkIdentity,SdkHawkIdentity)&&!SdkHawkResolvePolicy("original-sdk-project-fixture",true,SdkHawkIdentity+".resources",SdkHawkIdentity)&&SdkHawkReferencePolicy("original-sdk-project-fixture",true,SdkHawkIdentity,hawkPath,false,false,false,stage)&&!SdkHawkReferencePolicy("original-project-guid-fixture",true,SdkHawkIdentity,hawkPath,false,false,false,stage)&&!SdkHawkReferencePolicy("original-sdk-project-fixture",true,SdkHawkIdentity,hawkPath,true,false,false,stage)&&!SdkHawkReferencePolicy("original-sdk-project-fixture",true,SdkHawkIdentity,hawkPath,false,true,false,stage),"OWNED_SDK_HAWK_POLICY");
        Record("OWNED_SDK_HAWK_POLICY_PASS sdk-only=True guid-denied=True wrong-incomplete-origin-denied=True predicate-only=True");
        foreach(string resource in new string[]{AutofacResourceEnGb,AutofacResourceEn}){
            ProbeFiles.Require(SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,false,true,resource,true,false,8)&&SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,false,true,resource,false,true,1)&&!SdkRootResourcePolicy("original-project-guid-fixture",true,true,true,0,false,true,resource,true,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,false,0,false,true,resource,true,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,1,false,true,resource,true,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,true,true,resource,true,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",false,true,true,0,false,true,resource,true,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,false,true,resource,false,false,1)&&!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,false,true,resource,true,false,9),"OWNED_SDK_ROOT_RESOURCE_POLICY");}
        ProbeFiles.Require(!SdkRootResourcePolicy("original-sdk-project-fixture",true,true,true,0,false,true,AutofacResourceEn+".invalid",true,false,1)&&NextResourceProbe(7)==8&&NextResourceProbe(8)==9&&NextResourceProbe(9)==9,"OWNED_SDK_ROOT_SHARED_RESOURCE_BOUND");
        Record("OWNED_SDK_ROOT_RESOURCE_POLICY_PASS root-constructor-only=True owner-required=True shared-budget8=True ninth-denied=True callback-denied=True NULL-requester=UNKNOWN predicate-only=True");
        Assembly a=Assembly.LoadFrom(fixture);Func<string,Type> type=delegate(string n){return a.GetType("Owned.Task009."+n,true);};Type owner=type("ISdkProjectStoreFixture"),streamType=type("ICacheStreamFixture"),idType=type("ScopedIdFixture");
        MethodInfo[] methods=StreamContract(streamType,false);var stream=new StreamStoreDenyProxy(streamType,methods,Record);PrepareExistsStore(owner,owner.GetMethod("get_Project"),streamType,methods[1],false,Record);activeGuid=GuidContract(owner,false);
        object id=CreateFixtureId(idType,idType.GetConstructor(new Type[]{typeof(uint)}),idType.GetMethod("get_ProjectNumber"));var store=new LargeStringCacheStoreProxy(owner,owner.GetMethod("get_Id"),id,owner.GetMethod("get_LargeStringCache"),streamType,stream.GetTransparentProxy(),Record,activeContent,activeGuid);activeExists.Bind(store.State,activeContent);PrepareSdkMapping(store,owner,streamType,methods[1]);
        PrepareSdkModelServices(type("SdkModelRootFixture"),type("SdkModelCollectionFixture"),type("ISdkModelFactoryFixture"),type("ISdkEmptyDataStoreFixture"),owner,type("ISdkUsageDataFixture"),type("ISdkDispatcherFixture"),false);
        object modelFactory=type("SdkModelFactoryFixture").GetConstructor(new Type[]{type("ISdkDispatcherFixture")}).Invoke(new object[]{sdkUi.GetTransparentProxy()});object model=null,modelCollection=null;var api=SdkApi(type("SdkRootFixture"),type("SdkCollectionFixture"),type("SdkProjectFixture"),type("SdkModelProjectFixture"),owner,type("SdkExecutionFixture"),type("ISdkPluginFixture"),false);sdkMapping.Begin();sdkRootData.Begin();sdkUi.Begin();
        try{WithContentLifetime(delegate{WithCacheLifetime(store,stream,delegate{WithProjectResourceBudget(delegate{sdkWrapperPhase=true;try{
            modelCollection=CreateSdkCanonicalCollection(modelFactory);model=type("SdkModelProjectFixture").GetConstructor(new Type[]{modelCollection.GetType()}).Invoke(new object[]{modelCollection});
            type("SdkTypedFixture").GetMethod("HostRoundTrip").Invoke(null,new object[]{sdkUi.GetTransparentProxy(),sdkRootData.GetTransparentProxy(),sdkModelRoot});Record("OWNED_SDK_HOST_ROUNDTRIP_PASS real-WPF-query=True real-WPF-action=True canonical-collection=True empty-data-fixture=True");
            ProbeFiles.Require(!SdkContainsExact(modelCollection,model),"OWNED_SDK_INITIAL_MEMBERSHIP_EMPTY");AttachSdkProjectFixture(model,modelCollection,type("IValidatorParentFixture"),false);AttachSdkProjectFixture(model,modelCollection,type("IValidatorParentFixture"),false);object[] result=ConstructSdkRoute(api,model,store.GetTransparentProxy());type("SdkTypedFixture").GetMethod("Check").Invoke(null,new object[]{result[2],model});
            type("SdkRootFixture").GetField("Empty").SetValue(null,true);bool empty=false;try{ConstructSdkRoute(api,model,store.GetTransparentProxy());}catch(IOException e){empty=e.Message=="SDK_COLLECTION_EMPTY";}finally{type("SdkRootFixture").GetField("Empty").SetValue(null,false);}ProbeFiles.Require(empty,"OWNED_SDK_EMPTY_MEMBERSHIP_REFUSAL");Record("OWNED_SDK_EMPTY_MEMBERSHIP_PASS diagnostic=SDK_COLLECTION_EMPTY");
            Record("OWNED_SDK_PROJECT_ROUNDTRIP_PASS plugin-default=True genuine-owned-context=True project-reference-filter=True typed-original-reference=True empty-membership-refused=True fixture-only=True");
            try{type("SdkTypedFixture").GetMethod("Unsupported").Invoke(null,new object[]{sdkMapping.Reference});}catch(TargetInvocationException){}ProbeFiles.Require(store.State.Faulted,"OWNED_SDK_UNKNOWN_STREAM_REFUSED");
            try{type("SdkTypedFixture").GetMethod("DataUnsupported").Invoke(null,new object[]{sdkRootData.GetTransparentProxy()});}catch(TargetInvocationException){}
            try{type("SdkTypedFixture").GetMethod("DispatcherUnsupported").Invoke(null,new object[]{sdkUi.GetTransparentProxy()});}catch(TargetInvocationException){}
            ProbeFiles.Require(sdkRootData.Faulted&&sdkUi.Faulted,"OWNED_SDK_HOST_UNKNOWN_REFUSED");Record("OWNED_SDK_PROJECT_UNSUPPORTED_PASS all-other-streams-denied=True unknown-data-dispatcher-denied=True sticky=True");
        }finally{sdkWrapperPhase=false;}});});});}
        finally{sdkMapping.Close();sdkRootData.Close();sdkUi.Close();GC.KeepAlive(sdkRetained);GC.KeepAlive(sdkModelRoot);}
    }

    static ContextValidatorAdapter activeValidator;
    static ContextValidatorAdapter ValidatorApi(Type context,Type project,Type collection,Type parent,Type domCollection,Type checker,Type validatorBase,Type style,object styleValue,bool original,Func<object> retainedCollection,Func<bool> phase)
    {
        foreach(Type type in new Type[]{context,project,collection,parent,domCollection,checker,validatorBase,style})ProbeFiles.Require(type!=null&&type.GetType()==typeof(object).GetType()&&!type.ContainsGenericParameters&&!type.Assembly.IsDynamic&&!type.Assembly.ReflectionOnly,"VALIDATOR_RUNTIME_TYPES");
        ProbeFiles.Require(PublicInterface(context)&&PublicInterface(parent)&&PublicInterface(domCollection)&&parent.IsAssignableFrom(project)&&domCollection.IsAssignableFrom(collection)&&checker.IsPublic&&checker.IsClass&&!checker.IsAbstract&&!checker.IsGenericType&&validatorBase.IsAssignableFrom(checker)&&style.IsEnum&&styleValue!=null&&styleValue.GetType()==style,"VALIDATOR_CLASS_INTERFACES");
        MethodInfo definition=context.GetMethod("GetValidator",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
        ProbeFiles.Require(definition!=null&&definition.IsGenericMethodDefinition&&definition.GetGenericArguments().Length==1&&(int)definition.Attributes==1478&&definition.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"VALIDATOR_GENERIC_DEFINITION");
        Type generic=definition.GetGenericArguments()[0];Type[] constraints=generic.GetGenericParameterConstraints();ParameterInfo[] args=definition.GetParameters();
        ProbeFiles.Require(generic.GenericParameterAttributes==GenericParameterAttributes.None&&constraints.Length==1&&constraints[0]==validatorBase&&definition.ReturnType==generic&&args.Length==2&&args[0].ParameterType==parent&&args[1].ParameterType==domCollection,"VALIDATOR_GENERIC_SIGNATURE");
        ConstructorInfo ctor=checker.GetConstructor(new Type[]{collection,project});
        ProbeFiles.Require(ctor!=null&&ctor.GetType()==typeof(object).GetConstructor(Type.EmptyTypes).GetType()&&ctor.IsPublic&&!ctor.IsStatic&&ctor.DeclaringType==checker&&(int)ctor.Attributes==6278,"VALIDATOR_EXACT_PUBLIC_CTOR");
        ParameterInfo[] vector=ctor.GetParameters();ProbeFiles.Require(vector.Length==2&&vector[0].ParameterType==collection&&vector[1].ParameterType==project,"VALIDATOR_CTOR_VECTOR");
        Type projectReturn=original?project.Assembly.GetType("Knx.Ets.ObjectModel.Project.IProject",true):project;
        ProbeFiles.Require(projectReturn.IsAssignableFrom(project)&&(!original||(projectReturn.GetType()==typeof(object).GetType()&&projectReturn.Module==context.Module&&PublicInterface(projectReturn)&&!projectReturn.IsGenericType)),"VALIDATOR_PROJECT_RETURN_INTERFACE");
        string[] names={"get_GroupAddressStyle","GetNextPuid","SetProjectModified","get_Project"};Type[] returns={style,typeof(int),typeof(void),projectReturn};int[] tokens={0x06000267,0x06000269,0x06000256,0x06000261};MethodInfo[] scalar=new MethodInfo[4];
        for(int i=0;i<names.Length;i++){
            scalar[i]=context.GetMethod(names[i],BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);MethodInfo method=scalar[i];
            ProbeFiles.Require(method!=null&&method.Module==context.Module&&method.DeclaringType==context&&!method.IsStatic&&method.IsPublic&&!method.IsGenericMethod&&method.ReturnType==returns[i]&&method.GetParameters().Length==0&&(int)method.Attributes==(i==0||i==3?3526:1478)&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"VALIDATOR_SCALAR_PROFILE:"+names[i]);
            if(original)ProbeFiles.Require(method.MetadataToken==tokens[i],"VALIDATOR_SCALAR_TOKEN");
        }
        if(original)ProbeFiles.Require(GuidOriginal&&context.Assembly.FullName==OmIdentity&&context.Module==project.Module&&checker.Module==project.Module&&collection.Module==project.Module&&context.MetadataToken==0x02000044&&project.MetadataToken==0x020003a5&&collection.MetadataToken==0x020003bd&&checker.MetadataToken==0x02000e89&&(int)checker.Attributes==1048577&&ctor.MetadataToken==0x06008a52&&definition.MetadataToken==0x06000255&&checker.FullName=="Knx.Ets.ObjectModel.ConsistencyRules.ProjectConsistencyChecker"&&validatorBase.FullName=="Knx.Ets.ObjectModel.ConsistencyRules.Validator"&&style.MetadataToken==0x0200008e&&style.Assembly.FullName==CommonIdentity&&ProbeFiles.Same(context.Assembly.Location,Path.Combine(stage,"Knx.Ets.ObjectModel.dll")),"VALIDATOR_ORIGINAL_PINNED_PROFILE");
        MethodInfo closed=definition.MakeGenericMethod(new Type[]{checker});ProbeFiles.Require(closed.ReturnType==checker&&!closed.ContainsGenericParameters,"VALIDATOR_CLOSED_METHOD");
        Record("CONTEXT_VALIDATOR_API_READY type="+Text(checker.FullName)+" ctor="+ctor.MetadataToken.ToString("X8")+" generic-method="+definition.MetadataToken.ToString("X8")+" scalar-fixtures=4 activation=NOT_YET_RUN");
        return new ContextValidatorAdapter(project,collection,checker,ctor,closed,scalar,new Type[]{parent,domCollection},styleValue,retainedCollection,phase,ContextArgumentRecord);
    }
    static void PrepareNotificationApi(ContextValidatorAdapter adapter,Type context,Type service,Type implementation,bool original,Func<bool> phase)
    {
        foreach(Type type in new Type[]{service,implementation})ProbeFiles.Require(type.GetType()==typeof(object).GetType()&&!type.IsGenericType&&!type.ContainsGenericParameters&&!type.Assembly.IsDynamic&&!type.Assembly.ReflectionOnly,"NOTIFICATION_RUNTIME_TYPES");
        MethodInfo getter=context.GetMethod("get_ProjectChangedNotificationService",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        ConstructorInfo ctor=implementation.GetConstructor(Type.EmptyTypes);
        ProbeFiles.Require(PublicInterface(service)&&implementation.IsPublic&&implementation.IsClass&&!implementation.IsAbstract&&service.IsAssignableFrom(implementation)&&typeof(IDisposable).IsAssignableFrom(implementation)&&getter!=null&&getter.DeclaringType==context&&getter.Module==context.Module&&getter.IsPublic&&!getter.IsStatic&&!getter.IsGenericMethod&&getter.ReturnType==service&&getter.GetParameters().Length==0&&(int)getter.Attributes==3526&&getter.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)&&ctor!=null&&ctor.GetType()==typeof(object).GetConstructor(Type.EmptyTypes).GetType()&&ctor.DeclaringType==implementation&&ctor.IsPublic&&!ctor.IsStatic&&(int)ctor.Attributes==6278,"NOTIFICATION_EXACT_PUBLIC_API");
        if(original)ProbeFiles.Require(GuidOriginal&&service.Module==context.Module&&implementation.Module==context.Module&&service.Assembly.FullName==OmIdentity&&(int)service.Attributes==1048737&&(int)implementation.Attributes==1048577&&service.FullName=="Knx.Ets.ObjectModel.IProjectChangedNotificationService"&&implementation.FullName=="Knx.Ets.ObjectModel.ProjectChangedNotificationService"&&getter.MetadataToken==0x0600026d&&ctor.MetadataToken==0x060004ee,"NOTIFICATION_PINNED_ORIGINAL_API");
        adapter.ConfigureNotification(getter,ctor,implementation,phase);Record("CONTEXT_NOTIFICATION_API_READY ctor="+ctor.MetadataToken.ToString("X8")+" activation=NOT_YET_RUN owner=CALLER_NOT_INVENTED");
    }
    [ThreadStatic] static bool guidUndoFactoryActive;
    static object contextGenuineFactory;
    internal static bool GuidUndoProviderReferenceEligible()
    { return GuidOriginal&&guidUndoFactoryActive&&(guidProjectDisposal||(SdkOriginal&&sdkWrapperPhase))&&contextLifetime&&projectResourceBudgetActive&&projectResourceWindow==0&&projectResourceCallbackSuspended; }
    static void PrepareUndoManagerApi(ContextValidatorAdapter adapter,Type context,Type result,Type factoryContract,Type factoryClass,Type idType,object id,Func<object> factory,Func<object> contextValue,bool original,Func<bool> phase)
    {
        foreach(Type type in new Type[]{context,result,factoryContract,factoryClass,idType})ProbeFiles.Require(type.GetType()==typeof(object).GetType()&&!type.IsGenericType&&!type.ContainsGenericParameters&&!type.Assembly.IsDynamic&&!type.Assembly.ReflectionOnly,"UNDO_CONTEXT_RUNTIME_TYPES");
        MethodInfo getter=context.GetMethod("get_UndoManager",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
        MethodInfo method=factoryContract.GetMethod("UndoManager",BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly,null,new Type[]{idType,context},null);
        ProbeFiles.Require(PublicInterface(context)&&PublicInterface(factoryContract)&&factoryClass.IsPublic&&factoryClass.IsClass&&factoryContract.IsAssignableFrom(factoryClass)&&result.IsPublic&&result.IsClass&&!result.IsAbstract&&typeof(IDisposable).IsAssignableFrom(result)&&id!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(id)&&id.GetType()==idType&&getter!=null&&getter.DeclaringType==context&&getter.IsPublic&&!getter.IsStatic&&!getter.IsGenericMethod&&getter.ReturnType==result&&getter.GetParameters().Length==0&&(int)getter.Attributes==3526&&getter.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis)&&method!=null&&method.DeclaringType==factoryContract&&method.IsPublic&&!method.IsStatic&&!method.IsGenericMethod&&method.ReturnType==result&&(int)method.Attributes==1478&&method.CallingConvention==(CallingConventions.Standard|CallingConventions.HasThis),"UNDO_CONTEXT_EXACT_PUBLIC_FACTORY");
        ParameterInfo[] parameters=method.GetParameters();ProbeFiles.Require(parameters.Length==2&&parameters[0].ParameterType==idType&&parameters[1].ParameterType==context,"UNDO_CONTEXT_FACTORY_PARAMETERS");
        if(original)ProbeFiles.Require(GuidOriginal&&context.Assembly.FullName==OmIdentity&&result.Module==context.Module&&factoryContract.Module==context.Module&&factoryClass.Module==context.Module&&result.MetadataToken==0x02000131&&(int)result.Attributes==1048577&&result.FullName=="Knx.Ets.ObjectModel.UndoRedo.UndoManager"&&getter.MetadataToken==0x06000257&&method.MetadataToken==0x060001fd&&idType.FullName=="Knx.Ets.Ids.Project.ProjectId"&&idType.Assembly.FullName=="Knx.Ets.Ids, Version=6.4.8718.0, Culture=neutral, PublicKeyToken=61439873ec5e1159","UNDO_CONTEXT_ORIGINAL_TOKENS");
        adapter.ConfigureUndo(getter,result,delegate{
            object actualFactory=factory(),actualContext=contextValue();
            ProbeFiles.Require(actualFactory!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(actualFactory)&&actualFactory.GetType()==factoryClass&&actualContext!=null&&context.IsInstanceOfType(actualContext),"UNDO_CONTEXT_ACTUAL_ARGUMENTS");
            ProbeFiles.Require(!guidUndoFactoryActive,"UNDO_CONTEXT_FACTORY_NONNESTED");guidUndoFactoryActive=true;
            try{return method.Invoke(actualFactory,new object[]{id,actualContext});}finally{guidUndoFactoryActive=false;}
        },phase);
        Record("CONTEXT_UNDO_API_READY method="+method.MetadataToken.ToString("X8")+" ctor=NOT_INVOKED_BY_HOST result="+Text(result.FullName)+" activation=NOT_YET_RUN");
    }
    static void PrepareOriginalValidator(ContextConstructionApi api)
    {
        Assembly om=api.Project.Assembly;Type style=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Common.Types.dll")).GetType(EnumName,true);FieldInfo literal=style.GetField(ReviewedLiteral,BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
        ProbeFiles.Require(literal!=null&&literal.MetadataToken==0x04000176&&literal.IsLiteral&&Enum.GetUnderlyingType(style)==typeof(byte)&&Object.Equals(literal.GetRawConstantValue(),(byte)1),"VALIDATOR_REVIEWED_STYLE");
        activeValidator=ValidatorApi(api.Context,api.Project,api.Collection,api.Parent,om.GetType("Knx.Ets.ObjectModel.IDomCollection",true),om.GetType("Knx.Ets.ObjectModel.ConsistencyRules.ProjectConsistencyChecker",true),om.GetType("Knx.Ets.ObjectModel.ConsistencyRules.Validator",true),style,Enum.ToObject(style,literal.GetRawConstantValue()),true,delegate{return contextRetained==null?null:contextRetained[6];},delegate{return contextLifetime&&(CacheWindowEligible()||SdkWrapperEligible());});
        PrepareNotificationApi(activeValidator,api.Context,om.GetType("Knx.Ets.ObjectModel.IProjectChangedNotificationService",true),om.GetType("Knx.Ets.ObjectModel.ProjectChangedNotificationService",true),true,delegate{return GuidProjectDisposalEligible()||SdkWrapperEligible();});
    }
    static void OwnedValidatorSmoke(Assembly assembly)
    {
        Func<string,Type> type=delegate(string name){return assembly.GetType("Owned.Task009."+name,true);};
        Type c=type("ValidatorCollectionFixture"),p=type("ValidatorProjectFixture"),context=type("IValidatorContextFixture"),style=type("ValidatorStyleFixture");
        object collection=c.GetConstructor(Type.EmptyTypes).Invoke(new object[0]),project=p.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        activeValidator=ValidatorApi(context,p,c,type("IValidatorParentFixture"),type("IValidatorCollectionFixture"),type("ValidatorCheckerFixture"),type("ValidatorBaseFixture"),style,Enum.ToObject(style,(byte)1),false,delegate{return collection;},delegate{return CacheWindowEligible();});
        bool notificationPhase=false;PrepareNotificationApi(activeValidator,context,type("INotificationServiceFixture"),type("NotificationServiceFixture"),false,delegate{return notificationPhase&&CacheWindowEligible();});
        object ownedValidatorContext=null;
        object ownedUndoFactory=type("ValidatorUndoFactoryFixture").GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        PrepareUndoManagerApi(activeValidator,context,type("ValidatorUndoManagerFixture"),type("IValidatorUndoFactoryFixture"),type("ValidatorUndoFactoryFixture"),p,project,delegate{return ownedUndoFactory;},delegate{return ownedValidatorContext;},false,delegate{return notificationPhase&&CacheWindowEligible();});
        var proxy=new ContextArgumentDenyProxy(context,"context",ContextArgumentRecord,activeValidator);ownedValidatorContext=proxy.GetTransparentProxy();activeValidator.Begin();
        try{
            type("ValidatorTypedFixture").GetMethod("RoundTrip").Invoke(null,new object[]{proxy.GetTransparentProxy(),collection,project});
            notificationPhase=true;try{type("ValidatorTypedFixture").GetMethod("Notification").Invoke(null,new object[]{proxy.GetTransparentProxy()});type("ValidatorTypedFixture").GetMethod("Undo").Invoke(null,new object[]{proxy.GetTransparentProxy()});}finally{notificationPhase=false;}
            Record("OWNED_NOTIFICATION_ROUNDTRIP_PASS genuine-owned-ctor=1 cached-reference=True fixture-disposal-phase=True host-dispose=0");
            Record("OWNED_UNDO_FACTORY_ROUNDTRIP_PASS genuine-owned-factory=1 cached-reference=True fixture-disposal-phase=True host-dispose=0");
            ProbeFiles.Require(proxy.State.Entries==0&&!proxy.State.Faulted,"OWNED_VALIDATOR_NO_DENIAL");Record("OWNED_VALIDATOR_ROUNDTRIP_PASS genuine-owned-ctor=1 cached-reference=True scalar-coherence=True constructor-scalar-reentry=True");
            try{type("ValidatorTypedFixture").GetMethod("Unsupported").Invoke(null,new object[]{proxy.GetTransparentProxy()});}catch(TargetInvocationException){}
            ProbeFiles.Require(proxy.State.Entries==1&&proxy.State.Faulted,"OWNED_VALIDATOR_UNKNOWN_DENIED");Record("OWNED_VALIDATOR_UNSUPPORTED_PASS denial=1 fatal=True");
        }finally{activeValidator.Close();GC.KeepAlive(proxy);}
    }
    static ContextArgumentDenyProxy contextParent, contextObject; static ContextImporterDenyHolder contextImporter;
    static object[] contextRetained; static bool contextLifetime;
    static void PrepareContextArguments(ContextConstructionApi api)
    {
        ProbeFiles.Require(ContextMode && contextParent == null && contextObject == null && contextImporter == null, "CONTEXT_PREPARE_ONCE");
        contextParent = new ContextArgumentDenyProxy(api.Parent, "parent", ContextArgumentRecord);
        if(GuidOriginal){PrepareOriginalValidator(api);contextObject=new ContextArgumentDenyProxy(api.Context,"context",ContextArgumentRecord,activeValidator);}
        else contextObject = new ContextArgumentDenyProxy(api.Context, "context", ContextArgumentRecord);
        contextImporter = new ContextImporterDenyHolder(api.Callback, api.Callback.GetGenericArguments()[7], ContextArgumentRecord);
        contextRetained = new object[] { contextParent, contextObject, contextImporter, contextParent.GetTransparentProxy(), contextObject.GetTransparentProxy(), contextImporter.Callback, null };
    }
    static void ContextFinalAudit(int parentCount, bool parentFault, int objectCount, bool objectFault, int importerCount, bool importerFault, string phase)
    {
        bool denied = parentCount != 0 || parentFault || objectCount != 0 || objectFault || importerCount != 0 || importerFault;
        try { if(denied||InitializeAuditLog(phase))Record("CONTEXT_FINAL_AUDIT phase=" + phase + " parent=" + parentCount + "/" + parentFault + " context=" + objectCount + "/" + objectFault + " importer=" + importerCount + "/" + importerFault + " after-managed-cleanup=" + !contextLifetime); }
        finally { if (denied) throw new NotSupportedException("CONTEXT_ARGUMENT_FINAL_DENIAL"); }
    }
    static void CheckContextArguments(string phase)
    {
        try { if(activeValidator!=null)activeValidator.Audit(phase,Record); }
        finally {try{if(SdkProductMode&&sdkProductImport!=null)sdkProductImport.Audit(phase);}finally{try{if(SdkOriginal)CheckSdkFixture(phase);}finally{CheckContextDenials(phase);}} }
    }
    static void CheckContextDenials(string phase)
    {
        if (!ContextMode || (contextParent == null && contextObject == null && contextImporter == null)) return;
        ContextFinalAudit(contextParent == null ? 0 : contextParent.State.Entries, contextParent != null && contextParent.State.Faulted,
            contextObject == null ? 0 : contextObject.State.Entries, contextObject != null && contextObject.State.Faulted,
            contextImporter == null ? 0 : contextImporter.State.Entries, contextImporter != null && contextImporter.State.Faulted, phase);
    }
    static void WithContextLifetime(Action run)
    {
        ProbeFiles.Require(ContextMode && !contextLifetime, "CONTEXT_LIFETIME_ONCE"); contextLifetime = true;
        try { if(activeValidator!=null)activeValidator.Begin();if(SdkOriginal){sdkMapping.Begin();sdkKim.Begin();sdkRootData.Begin();sdkUi.Begin();sdkUiHolder.Begin();}run(); }
        finally {
            if(activeValidator!=null)activeValidator.Close();if(SdkOriginal){sdkMapping.Close();sdkKim.Close();sdkRootData.Close();sdkUi.Close();sdkUiHolder.Close();if(sdkWriteFixture!=null)sdkWriteFixture.Close();}
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
    [ThreadStatic] static bool guidProjectDisposal;
    static bool GuidProjectDisposalEligible()
    { return GuidOriginal && guidProjectDisposal && contextLifetime && projectResourceBudgetActive && projectResourceWindow == 0 && !projectResourceCallbackSuspended; }
    // The callback captures eligibility before exclusion; completion checks the same owner/lifetime after restoration.
    internal static bool ProductStoreDisposalLifetime()
    {return ProductFixtureMode()&&ProductDependencyComplete&&Thread.CurrentThread.ManagedThreadId==sdkOwner&&GuidOriginal&&guidProjectDisposal&&contextLifetime&&projectResourceBudgetActive&&projectResourceWindow==0;}
    internal static bool ProductStoreDisposalEligible()
    {return ProductStoreDisposalLifetime()&&!projectResourceCallbackSuspended;}
    internal static int ProductStoreCallbackLimit(){return ProductStoreDisposalEligible()?80:16;}
    internal static int ProductStoreFinalEntryLimit(){return ProductFixtureMode()?80:16;}
    static void DisposeContextProject(object project)
    {
        if(!GuidOriginal){((IDisposable)project).Dispose();return;}
        ProbeFiles.Require(!guidProjectDisposal && projectResourceWindow == 0 && !projectResourceCallbackSuspended,"GUID_DISPOSAL_PHASE");
        guidProjectDisposal=true;
        try{((IDisposable)project).Dispose();}finally{guidProjectDisposal=false;}
    }
    static void WithContextConstruction(ContextConstructionApi api, object factory, ProjectStoreIdProxy store)
    {
        object collection = null, project = null; bool exact = false, collectionReturned = false, projectReturned = false;
        if(GuidOriginal){ProbeFiles.Require(contextGenuineFactory==null,"CONTEXT_FACTORY_RETAIN_ONCE");contextGenuineFactory=factory;}
        ProbeFiles.Require(ContextMode && contextLifetime && factory != null && factory.GetType() == api.Factory && api.Contract.IsInstanceOfType(factory)
            && !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(factory), "CONTEXT_GENUINE_FACTORY");
        try {
            TraceDependencyGuard(unexpected, rejected, "CONTEXT_PRE_COLLECTION");
            ProbeFiles.Require(projectResourceWindow == 0 && !projectResourceCallbackSuspended, "CONTEXT_COLLECTION_WINDOW_ZERO");
            if(SdkOriginal)Record("CONTEXT_COLLECTION_BEGIN route=GENUINE_MODEL_ROOT_PROJECTS window=0 explicit-root-attempts=1");else Record("CONTEXT_COLLECTION_BEGIN token=" + api.CollectionConstructor.MetadataToken.ToString("X8") + " window=0 explicit-attempts=1");
            try { collection = SdkOriginal?CreateSdkCanonicalCollection(factory):api.CollectionConstructor.Invoke(new object[] { contextRetained[3], factory, contextImporter.Callback }); collectionReturned = true; }
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
            if(SdkOriginal)ProbeSdkProject(project,store.GetTransparentProxy());
        } catch { if (ContextOwned) OwnedContextPoint("error"); throw; } finally {
            try { if (exact) {
                try { ContextConstructionRecord("CONTEXT_PROJECT_DISPOSE_BEGIN attempts=1"); }
                finally { try { DisposeContextProject(project); } finally { Record("CONTEXT_PROJECT_DISPOSE_END attempts=1"); } }
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
    static string ownedSelectionNegativeCase;
    static int selectionConstructorPathEntries;
    // Metadata snapshot from the immutable unrelated MDT Binary Input archive, not an imported model.
    const string NegativeHardware2ProgramId="M-0083_H-177-0_HP-00B1-32-7488";
    const string NegativeProductArchiveHash="51ebce4f5e8b3adb364be0f8681d5d91d08ab11e8608703eb8973baf3a5e1709";
    static string OriginalReferenceApplicationProjection(out object application)
    {
        ProbeFiles.Require(SdkProductMode&&ProductDependencyComplete&&containerInputsReady,"DCA_MANIFEST_COMPLETE_PINNED_PROFILE");
        Assembly ids=Assembly.LoadFrom(Path.Combine(stage,"Knx.Ets.Ids.dll"));
        ProbeFiles.Require(ids.FullName==Identities[11]&&!ids.IsDynamic&&!ids.ReflectionOnly&&!ids.GlobalAssemblyCache&&ProbeFiles.Same(ids.Location,Path.Combine(stage,"Knx.Ets.Ids.dll")),"DCA_MANIFEST_ORIGINAL_IDS_OWNER");
        object hp=SdkReferenceId(ids,"Knx.Ets.Ids.Product.Hardware2ProgramId",0x0200004c,0x060003ab,ownedSelectionNegativeCase=="actual-product"?NegativeHardware2ProgramId:ReferenceDcaManifestSelection.Hardware2ProgramId);
        MethodInfo applicationGetter=SdkDeviceMethod(hp.GetType(),0x060003a5);Type applicationType=ids.GetType("Knx.Ets.Ids.Product.ApplicationProgramId",true,false);
        ProbeFiles.Require(applicationGetter.Name=="get_ApplicationProgramId"&&applicationGetter.GetParameters().Length==0&&applicationGetter.ReturnType==applicationType&&applicationType.Module==ids.ManifestModule&&applicationType.MetadataToken==0x0200003a&&(int)applicationType.Attributes==1056769,"DCA_MANIFEST_APPLICATION_ID_API");
        application=applicationGetter.Invoke(hp,new object[0]);ProbeFiles.Require(application!=null&&!System.Runtime.Remoting.RemotingServices.IsTransparentProxy(application)&&application.GetType()==applicationType,"DCA_MANIFEST_GENUINE_APPLICATION_ID");
        MethodInfo projection=SdkDeviceMethod(applicationType,0x06000299);ProbeFiles.Require(projection.Name=="ToStringWithoutFingerprintAndOrigManuf"&&projection.ReturnType==typeof(string)&&projection.GetParameters().Length==0&&(int)projection.Attributes==134,"DCA_MANIFEST_ORIGINAL_PROJECTION_API");
        string value=projection.Invoke(application,new object[0]) as string;ProbeFiles.Require(!String.IsNullOrEmpty(value)&&value.Length<=128,"DCA_MANIFEST_ORIGINAL_PROJECTION_RESULT");return value;
    }
    static void OriginalContextConstruction()
    {
        if(!SdkProductMode){OriginalContextConstructionCore();return;}
        object application=null;
        string projection=ownedSelectionNegativeCase=="missing-context"?null:OriginalReferenceApplicationProjection(out application);
        if(ownedSelectionNegativeCase!=null)Record("SELECTION_NEGATIVE_INPUT case="+ownedSelectionNegativeCase+" original-Ids-projection="+(application!=null)+" projection="+(projection==null?"ABSENT":Text(projection))+" product-metadata=VERIFIED_IMMUTABLE_SNAPSHOT archive-sha256="+NegativeProductArchiveHash+" manifest-sha256="+ReferenceDcaManifestSelection.ManifestHash+" vendor-UI-missing-parameter-test=NOT_RUN");
        ReferenceDcaManifestSelection.BeforeContextMutation(application,ReferenceDcaManifestSelection.Declarations(),projection,delegate{
            Record("DCA_MANIFEST_SELECTION selected=True rule=DECLARATION_STARTS_WITH_ORIGINAL_PROJECTION comparison=ORDINAL_ASCII_REFERENCE projection="+Text(projection)+" manifest-sha256="+ReferenceDcaManifestSelection.ManifestHash+" mutations=NOT_STARTED qualification=OWNED_RULE_ORIGINAL_IDS_API");
            OriginalContextConstructionCore();
        });
    }
    static void RunSelectionNegative()
    {
        ProbeFiles.Require(SdkProductMode&&sdkUiSeconds==0&&(ownedSelectionNegativeCase=="actual-product"||ownedSelectionNegativeCase=="missing-context"),"SELECTION_NEGATIVE_PROFILE");
        bool rejectedBeforeMutation=false;
        string expected=ownedSelectionNegativeCase=="actual-product"?"DCA_MANIFEST_APPLICATION_MISMATCH":"DCA_REQUIRED_PRODUCT_CONTEXT_MISSING";
        try{OriginalContextConstruction();}
        catch(InvalidDataException error){if(error.Message!=expected)throw;rejectedBeforeMutation=true;}
        ProbeFiles.Require(rejectedBeforeMutation&&selectionConstructorPathEntries==0&&contextGenuineFactory==null&&contextRetained==null&&!contextLifetime
            &&sdkRetained==null&&sdkDeviceRetained==null&&sdkProductImport==null&&sdkDcaInstance==null&&sdkModelRoot==null&&activeCache==null&&activeValidator==null
            &&!unexpected&&!rejected,"SELECTION_NEGATIVE_BEFORE_CONSTRUCTION");
        Record("OWNED_SELECTION_NEGATIVE_PASS case="+ownedSelectionNegativeCase+" rejection="+expected+" constructor-path-entries=0 host-factory=NOT_CREATED host-project=NOT_CREATED configuration=NOT_CREATED UI=NOT_RUN qualification=OWNED_PREFLIGHT original-Ids-API="+(ownedSelectionNegativeCase=="actual-product")+" caller-receipt=UNKNOWN vendor-missing-parameter-behavior=NOT_RUN");
    }
    static void OriginalContextConstructionCore()
    {
        // Negative-only tripwire: a regressed selection guard must never enter any constructor setup.
        if(ownedSelectionNegativeCase!=null){selectionConstructorPathEntries++;throw new InvalidDataException("DCA_NEGATIVE_CONSTRUCTOR_GUARD_BYPASS");}
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
        StreamStoreDenyProxy stream = null; Type streamType = null; MethodInfo cacheGetter = null; MethodInfo[] streamDeclarations = null;
        if (CacheOriginal) { streamType = om.GetType("Knx.Ets.ObjectModel.IStreamStore", true); MethodInfo[] declarations = StreamContract(streamType, true); streamDeclarations = declarations; cacheGetter = storeType.GetMethod("get_LargeStringCache", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly); CacheGetterSignature(storeType, cacheGetter, streamType); stream = new StreamStoreDenyProxy(streamType, declarations, Record); }
        if (ExistsOriginal) PrepareExistsStore(storeType, storeType.GetMethod("get_Project", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly), streamType, streamDeclarations[1], true, Record);
        else if (ContentOriginal) PrepareContentStore(storeType, storeType.GetMethod("get_Project", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly), true, Record);
        if (GuidOriginal) activeGuid = GuidContract(storeType, true);
        object value = CreateFixtureId(id, idCtor, idGetter); ProjectStoreIdProxy store;
        if (GuidOriginal) { store = new LargeStringCacheStoreProxy(storeType, storeGetter, value, cacheGetter, streamType, stream.GetTransparentProxy(), Record, activeContent, activeGuid); }
        else if (ContentOriginal) store = new LargeStringCacheStoreProxy(storeType, storeGetter, value, cacheGetter, streamType, stream.GetTransparentProxy(), Record, activeContent);
        else if (CacheOriginal) store = new LargeStringCacheStoreProxy(storeType, storeGetter, value, cacheGetter, streamType, stream.GetTransparentProxy(), Record);
        else store = new ProjectStoreIdProxy(storeType, storeGetter, value, Record);
        if (ExistsOriginal) activeExists.Bind(((LargeStringCacheStoreProxy)store).State, activeContent);
        var denied = new ProjectDenyProxy(providerType, Record);
        if(SdkOriginal){PrepareSdkMapping((LargeStringCacheStoreProxy)store,storeType,streamType,streamDeclarations[1]);sdkKimApi=FactoryRegistrationApi(container,autofac.GetType("Autofac.RegistrationExtensions",true),autofac.GetType("Autofac.Builder.IRegistrationBuilder`3",true),autofac.GetType("Autofac.Builder.SimpleActivatorData",true),autofac.GetType("Autofac.Builder.SingleRegistrationStyle",true),providerType);sdkKim=new UndoProviderFactoryHolder(providerType,denied.GetTransparentProxy(),sdkKimApi.CallbackType,Record,SdkWrapperEligible);Type dispatcher=common.GetType("Knx.Ets.Common.Wpf.Contracts.IDispatcher",true);PrepareSdkModelServices(om.GetType("Knx.Ets.ObjectModel.Root",true),collection,factoryContract,om.GetType("Knx.Ets.ObjectModel.IDataStore",true),storeType,om.GetType("Knx.Ets.ObjectModel.Usages.IUsageDataStore",true),dispatcher,true);sdkUiApi=FactoryRegistrationApi(container,autofac.GetType("Autofac.RegistrationExtensions",true),autofac.GetType("Autofac.Builder.IRegistrationBuilder`3",true),autofac.GetType("Autofac.Builder.SimpleActivatorData",true),autofac.GetType("Autofac.Builder.SingleRegistrationStyle",true),dispatcher);sdkUiHolder=new UndoProviderFactoryHolder(dispatcher,sdkUi.GetTransparentProxy(),sdkUiApi.CallbackType,Record,SdkHostPhaseEligible);}
        resourceOwner = autofac;
        PrepareContextArguments(scope);
        if(SdkOriginal)PrepareSdkImporter(registration,scope.Callback);
        if(SdkDeviceMode)PrepareSdkDevicePlugin(registration,om);
        if(SdkProductMode)PrepareProductMaster(sdkUiApi,om);
        if(SdkDeviceMode)PrepareSdkIdCreator(om,objectContext);
        if(GuidOriginal)PrepareUndoManagerApi(activeValidator,objectContext,om.GetType("Knx.Ets.ObjectModel.UndoRedo.UndoManager",true),factoryContract,factory,id,value,delegate{return contextGenuineFactory;},delegate{return contextRetained[4];},true,delegate{return GuidProjectDisposalEligible()||SdkWrapperEligible();});
        Action run = delegate {
            if (UndoFactoryOriginal) WithUndoFactoryProvider(trace, container, registration, factoryRegistration, factoryCtor, denied, store, undo, holder,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); });
            else if (UndoDeniedOriginal) WithUndoProvider(trace, container, registration, undoRegistration, factoryCtor, denied, store, undo,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); });
            else WithRegisteredTrace(trace, container, registration, factoryCtor, denied,
                delegate(object actualFactory) { WithContextConstruction(scope, actualFactory, store); }, store);
        };
        Action lifetime = delegate { WithContextLifetime(delegate { WithCacheLifetime((LargeStringCacheStoreProxy)store, stream, delegate { WithProjectResourceBudget(run); }); }); };
        try { if (ContentOriginal) WithContentLifetime(lifetime); else lifetime(); }
        finally { if(SdkProductMode&&sdkProductImport!=null)sdkProductImport.Dispose(); }
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
                }, null, delegate(object builder) { RegisterTrace(traceRegistration, builder, instance); RegisterUndoFactory(factoryRegistration, builder, holder);if(SdkOriginal){RegisterSdkKim(builder);RegisterSdkUi(builder);RegisterSdkImporter(builder);if(SdkDeviceMode)RegisterSdkDevicePlugin(builder);if(SdkProductMode)RegisterProductMaster(builder);} });
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
        CheckContentStore(phase);
        if (!CacheMode || activeCache == null || activeStream == null) return;
        ProbeFiles.Require(!activeCache.State.Faulted && activeCache.State.Entries <= ProductStoreFinalEntryLimit() && activeCache.State.Attempts <= 1 && activeCache.State.Replies <= 1, "CACHE_WRAPPER_DENIED_" + phase);
        ProbeFiles.Require(activeStream.Requests == 0, "STREAM_STORE_DENIED_" + phase);
    }
    static void FinishCacheAudit(int entries, int attempts, int replies, bool faulted, int streamDenied)
    {
        try { Record("CACHE_FINAL_AUDIT after-all-managed-disposal=True entries=" + entries + " attempts=" + attempts + " replies=" + replies + " faulted=" + faulted + " stream-denied=" + streamDenied + " values=NOT_PROVIDED"); }
        finally { if (faulted || entries > ProductStoreFinalEntryLimit() || attempts > 1 || replies > 1 || streamDenied != 0) throw new NotSupportedException("CACHE_STREAM_FINAL_DENIAL"); }
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
            bool suppress=CompactProductDiagnostics()&&sdkUiMainCoreFinalRecorded,prior=sdkUiSuppressRepeatedHealthyFinal;
            sdkUiSuppressRepeatedHealthyFinal=suppress;
            try{
                try { if (ContextOwned && contextRetained != null) OwnedContextPoint("process-final"); CheckContextArguments("PROCESS_FINAL"); }
                catch (Exception error) { outcome = 2; try { if (log != null) Errors(error); } catch { } }
                finally {
                    try { CheckContentStore("PROCESS_FINAL"); }
                    catch (Exception error) { outcome = 2; try { if (log != null) Errors(error); } catch { } }
                    finally { GC.KeepAlive(activeContent); GC.KeepAlive(contextRetained); }
                }
            }finally{sdkUiSuppressRepeatedHealthyFinal=prior;}
            if(suppress){try{Record((ownedMissingDevice?"SDK_DEVICE_NEGATIVE_FINAL_REVALIDATION exit=":ownedStableIdentity?"SDK_IDENTITY_FINAL_REVALIDATION exit=":ParameterNegativeProfile()?"SDK_READ_FINAL_REVALIDATION exit=":"SDK_UI_FINAL_REVALIDATION exit=")+outcome+" checks=EXECUTED healthy-repeat-logs=OMITTED fault-logs=UNCHANGED");}catch(Exception error){outcome=2;try{if(log!=null)Errors(error);}catch{}}}
        }
        return outcome;
    }
    static int MainCore(string[] args)
    {
        int result = 2;
        try {
            ProbeFiles.Require((args.Length == 6 || args.Length == 8) && args[0] == "--mode" && args[2] == "--input" && args[4] == "--log", "PROJECT_ARGUMENTS");
            mode = args[1]; ProbeFiles.Require(EnumOnly || Original || OwnedProject || FactoryOnly, "PROJECT_MODE");
            ProbeFiles.Require(args.Length == 6 || (SdkProductMode&&args[6]=="--owned-ui-context-negative"&&args[7]=="missing-device") || (SdkProductMode&&args[6]=="--owned-stable-identity"&&args[7]=="observe") || (SdkProductMode&&args[6]=="--owned-parameter-negative"&&args[7]=="missing-definition") || (SdkProductMode&&args[6]=="--owned-selection-negative"&&(args[7]=="actual-product"||args[7]=="missing-context")) || (SdkProductMode&&args[6]=="--ui-seconds"&&args[7]=="600") || (mode == "owned-project-genuine-factory-resources" && args[6] == "--owned-resource-case" && Array.IndexOf(OwnedResourceCases, args[7]) >= 0)
                || (mode == "owned-project-undo-provider-denied" && args[6] == "--owned-undo-case" && Array.IndexOf(OwnedUndoCases, args[7]) >= 0)
                || (mode == "owned-project-undo-provider-factory" && args[6] == "--owned-factory-case" && Array.IndexOf(OwnedFactoryCases, args[7]) >= 0)
                || (mode == "owned-project-large-string-cache-denied" && args[6] == "--owned-cache-case" && Array.IndexOf(OwnedCacheCases, args[7]) >= 0)
                || ((mode == "owned-project-context-denied" || mode == "owned-project-context-xml-registration") && args[6] == "--owned-context-case" && Array.IndexOf(OwnedContextCases, args[7]) >= 0)
                || (mode == "owned-project-content-store-exists-fixture" && args[6] == "--owned-exists-case" && Array.IndexOf(OwnedExistsCases, args[7]) >= 0)
                || (mode == "owned-project-content-store-diagnostics" && args[6] == "--owned-content-diagnostic-case" && Array.IndexOf(OwnedContentDiagnosticCases, args[7]) >= 0)
                || (mode == "owned-project-content-store-reference" && args[6] == "--owned-content-store-case" && Array.IndexOf(OwnedContentCases, args[7]) >= 0)
                || (mode == "owned-project-context-xml-registration" && args[6] == "--owned-store-diagnostic-case" && Array.IndexOf(OwnedStoreDiagnosticCases, args[7]) >= 0)
                || (mode == "owned-project-context-xml-registration" && args[6] == "--owned-xml-pin-case" && Array.IndexOf(new string[] { "missing", "size", "hash" }, args[7]) >= 0), "OWNED_RESOURCE_CASE_ARGUMENTS");
            if (args.Length == 8) { if(SdkProductMode){if(args[6]=="--ui-seconds")sdkUiSeconds=600;else if(args[6]=="--owned-stable-identity")ownedStableIdentity=true;else if(args[6]=="--owned-ui-context-negative")ownedMissingDevice=true;else if(args[6]=="--owned-parameter-negative")ownedParameterNegative=true;else ownedSelectionNegativeCase=args[7];}else if (ExistsOwned) ownedExistsCase = args[7]; else if (ContentDiagnosticsOwned) ownedContentDiagnosticCase = args[7]; else if (ContentOwned) ownedContentCase = args[7]; else if (mode == "owned-project-context-xml-registration" && args[6] == "--owned-store-diagnostic-case") ownedStoreDiagnosticCase = args[7]; else if (mode == "owned-project-context-xml-registration" && args[6] == "--owned-xml-pin-case") ownedXmlPinCase = args[7]; else if (ContextOwned) ownedContextCase = args[7]; else if (CacheOwned) ownedCacheCase = args[7]; else if (UndoFactoryOwned) ownedFactoryCase = args[7]; else if (UndoDeniedOwned) ownedUndoCase = args[7]; else ownedResourceCase = args[7]; }
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
            if(ownedSelectionNegativeCase!=null){RunSelectionNegative();result=0;}
            else if (EnumOnly) {
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
                        if (ProjectScope) { if(SdkOwned)OwnedSdkFixture();else if (GuidOwned) OwnedGuidFixture(); else if (ExistsOwned) OwnedExistsFixture(); else if (ContentDiagnosticsOwned && ownedContentDiagnosticCase != null) OwnedContentDiagnostics(); else if (ContentOwned) { if (ownedContentCase == null) OwnedContentContextConstruction(); else OwnedContentCasesRun(); } else if (XmlRegistrationOwned && ownedStoreDiagnosticCase != null) OwnedStoreRequestDiagnostics(); else if (XmlRegistrationOwned && ownedXmlPinCase != null) OwnedXmlRegistrationPinNegative(); else if (ContextMode) { if (ContextOriginal) OriginalContextConstruction(); else OwnedContextConstruction(); } else if (ProjectScopeOriginal) OriginalProjectScope(); else if (CacheOwned) OwnedCacheProvider(); else if (UndoFactoryOwned) OwnedUndoFactoryProvider(); else if (UndoDeniedOwned) OwnedUndoProvider(); else if (ProjectResourcesOwned) OwnedProjectResources(); else OwnedProjectScope(); }
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
            if(CompactProductDiagnostics())Record("HEALTHY_DIAGNOSTIC_SUMMARY distinct-known-records="+healthyLog.Kinds+" repeated-healthy-records-omitted="+healthyLog.Omitted+" first-provenance=RETAINED errors-and-validation=UNCHANGED");
            Record("PROJECT_RESULT exit=" + result + " metadata-only=" + EnumOnly + " qualification=NOT_ESTABLISHED ui="+UiOutcome()+" state=NOT_RUN hardware=NOT_RUN");
            if(CompactProductDiagnostics())sdkUiMainCoreFinalRecorded=true;
            return result;
        } catch (Exception error) { if (log != null) { try { Errors(error); } catch (IOException) { } } Console.WriteLine("PROJECT_FAILURE " + error.GetType().FullName + " " + Text(error.Message)+" ui="+UiOutcome()); return 2; }
        finally { GC.KeepAlive(constructed); /* Held input/log handles live until supervised process exit. */ }
    }
    static void Errors(Exception error)
    {
        for (int i = 0; error != null && i < 8; i++, error = error.InnerException)
            Record("PROJECT_EXCEPTION depth=" + i + " type=" + error.GetType().FullName + " hresult=" + error.HResult.ToString("X8") + " message=" + Text(error.Message));
        if (error != null) Record("PROJECT_EXCEPTION_CHAIN_TRUNCATED");
    }
}
