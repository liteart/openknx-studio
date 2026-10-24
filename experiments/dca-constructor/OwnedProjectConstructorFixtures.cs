// Owned shapes only; these are not SDK implementations or usable services.
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Owned.Task009
{
    public enum GroupAddressStyleFixture : short { Free = -2, ThreeLevel = 7 }
    public enum TooManyFieldsFixture { A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,A1,B1,C1,D1,E1,F1,G1,H1,I1,J1,K1,L1,M1,N1,O1,P1,Q1,R1,S1,T1,U1,V1,W1,X1,Y1,Z1,A2,B2,C2,D2,E2,F2,G2,H2,I2,J2,K2,L2 }
    public sealed class NotEnumFixture { public static int Value = Fail(); static int Fail() { throw new InvalidOperationException("OWNED_CCTOR_EXECUTED"); } }
    public interface IProductStoreFixture { string Name { get; } }
    public interface IProjectStoreFixture : IProductStoreFixture { void Save(); bool TryRead(out int value); }
    public interface IFactoryFixture { T Create<T>(); void Change(ref int value); }
    public sealed class ProjectThrowsFixture
    {
        public static int Calls;
        public static bool CalledOnce() { return Calls == 1; }
        public ProjectThrowsFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IProjectStoreFixture store)
        {
            Calls++; Check(factory, name, style, store); store.Save();
        }
        internal static void Check(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IProjectStoreFixture store)
        {
            if (factory == null || store == null || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel)
                throw new InvalidOperationException("OWNED_ARGUMENTS");
        }
    }
    public sealed class ProjectCatchesFixture
    {
        public ProjectCatchesFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IProjectStoreFixture store)
        { ProjectThrowsFixture.Check(factory, name, style, store); try { store.Save(); } catch (NotSupportedException) { } }
    }
    public sealed class ProjectNoRequestsFixture
    {
        public ProjectNoRequestsFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IProjectStoreFixture store)
        { ProjectThrowsFixture.Check(factory, name, style, store); }
    }
    public static class ProjectAssertions
    {
        static void Denied(Action call)
        {
            try { call(); } catch (NotSupportedException error) {
                if (error.Message.StartsWith("PROJECT_FIXTURE_UNSUPPORTED:", StringComparison.Ordinal)) return;
                throw;
            }
            throw new InvalidOperationException("OWNED_DENIAL_REQUIRED");
        }
        public static void AssertAll(object factory, object store)
        {
            var f = (IFactoryFixture)factory; var s = (IProjectStoreFixture)store;
            Denied(delegate { GC.KeepAlive(s.Name); });
            Denied(delegate { s.Save(); });
            Denied(delegate { int value; s.TryRead(out value); });
            Denied(delegate { GC.KeepAlive(f.Create<string>()); });
            Denied(delegate { int value = 9; f.Change(ref value); });
            for (int i = 0; i < 20; i++) Denied(delegate { s.Save(); });
        }
    }
    // These unsigned owned shapes do not impersonate original type names or tokens.
    public sealed class ProjectIdFixture
    {
        static int constructors, getters;
        readonly uint number;
        public ProjectIdFixture(uint value) { constructors++; number = value; }
        public uint ProjectNumber { get { getters++; return number; } }
        public static bool Untouched() { return constructors == 0 && getters == 0; }
        public static bool CreatedAndValidatedOnce() { return constructors == 1 && getters == 1; }
        public static bool IsFixture(object value) { var id = value as ProjectIdFixture; return id != null && id.number == 1; }
    }
    public sealed class WrongIdShapeFixture
    {
        public WrongIdShapeFixture(uint value) { throw new InvalidOperationException("OWNED_INVALID_SHAPE_EXECUTED"); }
        public string ProjectNumber { get { throw new InvalidOperationException("OWNED_INVALID_GETTER_EXECUTED"); } }
    }
    public sealed class PrivateIdCtorFixture
    {
        private PrivateIdCtorFixture(uint value) { throw new InvalidOperationException("OWNED_PRIVATE_CONSTRUCTOR_EXECUTED"); }
        public uint ProjectNumber { get { throw new InvalidOperationException("OWNED_PRIVATE_CONSTRUCTOR_GETTER_EXECUTED"); } }
    }
    public sealed class PrivateIdGetterFixture
    {
        public PrivateIdGetterFixture(uint value) { throw new InvalidOperationException("OWNED_PRIVATE_GETTER_CONSTRUCTOR_EXECUTED"); }
        private uint ProjectNumber { get { throw new InvalidOperationException("OWNED_PRIVATE_GETTER_EXECUTED"); } }
    }
    public sealed class ThrowingIdFixture
    {
        static int calls;
        public ThrowingIdFixture(uint value) { calls++; throw new NotSupportedException("OWNED_ID_CONSTRUCTOR_THROW"); }
        public uint ProjectNumber { get { throw new InvalidOperationException("OWNED_GETTER_AFTER_FAILED_CONSTRUCTOR"); } }
        public static bool AttemptedOnce() { return calls == 1; }
    }
    public sealed class WrongNumberIdFixture
    {
        static int constructors, getters;
        public WrongNumberIdFixture(uint value) { constructors++; }
        public uint ProjectNumber { get { getters++; return 2; } }
        public static bool AttemptedOnce() { return constructors == 1 && getters == 1; }
    }
    public interface IForeignIdStoreFixture { ProjectIdFixture Id { get; } }
    public interface IProjectIdStoreFixture : IProductStoreFixture
    {
        ProjectIdFixture Id { get; }
        ProjectIdFixture OtherId { get; }
        void Save();
        void Download();
        bool TryRead(out int value);
        T Read<T>();
        void Change(ref int value);
    }
    public sealed class ProjectReadsIdFixture
    {
        public ProjectReadsIdFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IProjectIdStoreFixture store)
        {
            if (factory == null || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel)
                throw new InvalidOperationException("OWNED_ID_PROJECT_ARGUMENTS");
            object first = store.Id, second = store.Id;
            if (!Object.ReferenceEquals(first, second) || !ProjectIdFixture.IsFixture(first)) throw new InvalidOperationException("OWNED_ID_PROJECT_STABILITY");
        }
    }
    // Bounded malformed call fixtures exercise rejection without invoking their MethodInfo.
    public sealed class MethodMessageFixture : IMethodCallMessage
    {
        readonly MethodBase method;
        readonly object[] args;
        readonly int count;
        readonly string name;
        readonly IDictionary properties = new Hashtable();
        public MethodMessageFixture(MethodBase value, object[] arguments, int argCount, string methodName) { method = value; args = arguments; count = argCount; name = methodName; }
        public int ArgCount { get { return count; } }
        public object[] Args { get { return args; } }
        public bool HasVarArgs { get { return false; } }
        public LogicalCallContext LogicalCallContext { get { return null; } }
        public MethodBase MethodBase { get { return method; } }
        public string MethodName { get { return name; } }
        public object MethodSignature { get { return null; } }
        public string TypeName { get { return method == null ? "Owned.Invalid" : method.DeclaringType.AssemblyQualifiedName; } }
        public string Uri { get; set; }
        public IDictionary Properties { get { return properties; } }
        public object GetArg(int index) { return args[index]; }
        public string GetArgName(int index) { return "OwnedArg" + index; }
        public int InArgCount { get { return count; } }
        public object[] InArgs { get { return args; } }
        public object GetInArg(int index) { return args[index]; }
        public string GetInArgName(int index) { return GetArgName(index); }
    }
    public interface IDenyDependencyFixture
    {
        string Name { get; }
        void Touch();
        bool TryRead(out int value);
        T Read<T>();
        void Change(ref int value);
        void Download();
    }
    public interface IContainerDependencyFixture : IDenyDependencyFixture { }
    public interface IDataDependencyFixture : IDenyDependencyFixture { }
    public sealed class FactoryReturnsFixture
    {
        static int calls;
        public FactoryReturnsFixture(IContainerDependencyFixture first, IDataDependencyFixture second)
        { calls++; if (first == null || second == null) throw new InvalidOperationException("OWNED_FACTORY_ARGUMENTS"); }
        public static bool Untouched() { return calls == 0; }
        public static bool CalledOnce() { return calls == 1; }
    }
    public sealed class FactoryThrowsFixture
    {
        static int calls;
        public FactoryThrowsFixture(IContainerDependencyFixture first, IDataDependencyFixture second) { calls++; first.Touch(); }
        public static bool CalledOnce() { return calls == 1; }
    }
    public sealed class FactoryCatchesFixture
    {
        static int calls;
        public FactoryCatchesFixture(IContainerDependencyFixture first, IDataDependencyFixture second)
        {
            calls++;
            try { first.Touch(); } catch (NotSupportedException) { }
            try { second.Touch(); } catch (NotSupportedException) { }
        }
        public static bool CalledOnce() { return calls == 1; }
    }
    public sealed class FactoryPrivateConstructorFixture
    {
        private FactoryPrivateConstructorFixture(IContainerDependencyFixture first, IDataDependencyFixture second)
        { throw new InvalidOperationException("OWNED_PRIVATE_FACTORY_CONSTRUCTOR_EXECUTED"); }
    }
    public static class FactoryAssertions
    {
        static void Denied(Action action)
        {
            try { action(); } catch (NotSupportedException error) {
                if (error.Message.StartsWith("PROJECT_FIXTURE_UNSUPPORTED:", StringComparison.Ordinal)) return;
                throw;
            }
            throw new InvalidOperationException("OWNED_FACTORY_DENIAL_REQUIRED");
        }
        public static void AssertAll(object first, object second)
        {
            foreach (IDenyDependencyFixture proxy in new IDenyDependencyFixture[] { (IContainerDependencyFixture)first, (IDataDependencyFixture)second }) {
                Denied(delegate { GC.KeepAlive(proxy.Name); });
                Denied(delegate { proxy.Touch(); });
                Denied(delegate { int value; proxy.TryRead(out value); });
                Denied(delegate { GC.KeepAlive(proxy.Read<string>()); });
                Denied(delegate { int value = 9; proxy.Change(ref value); });
                Denied(delegate { proxy.Download(); });
                for (int i = 0; i < 20; i++) Denied(delegate { proxy.Touch(); });
            }
        }
    }
    // Owned lifetime shapes only: no Autofac/SDK names, signing or token imitation.
    public enum EmptyOptionsFixture : int { None = 0 }
    public enum EmptyByteOptionsFixture : byte { None = 0 }
    public enum EmptyWrongOptionsFixture : int { None = 1 }
    public interface IEmptyContainerFixture : IContainerDependencyFixture, IDisposable { }
    public static class EmptyControlFixture
    {
        public static int Constructors, Builds, Disposals, Factories;
        public static string Behavior;
        public static IDataDependencyFixture DisposalProvider;
        public static Action BuilderCallback;
        public static void Reset(string behavior) { Constructors = Builds = Disposals = Factories = 0; Behavior = behavior; DisposalProvider = null; BuilderCallback = null; }
    }
    public sealed class EmptyBuilderFixture
    {
        public EmptyBuilderFixture()
        {
            EmptyControlFixture.Constructors++;
            if (EmptyControlFixture.Behavior == "ctor-throw") throw new NotSupportedException("OWNED_BUILDER_THROW");
            if (EmptyControlFixture.BuilderCallback != null) EmptyControlFixture.BuilderCallback();
        }
        public IEmptyContainerFixture Build(EmptyOptionsFixture options)
        {
            EmptyControlFixture.Builds++;
            if (options != EmptyOptionsFixture.None) throw new InvalidOperationException("OWNED_NONE_REQUIRED");
            if (EmptyControlFixture.Behavior == "build-throw") throw new NotSupportedException("OWNED_BUILD_THROW");
            return EmptyControlFixture.Behavior == "null" ? null : new EmptyContainerFixture();
        }
    }
    public sealed class EmptyContainerFixture : IEmptyContainerFixture
    {
        public void Dispose()
        {
            EmptyControlFixture.Disposals++;
            if (EmptyControlFixture.Behavior == "dispose-throw") throw new NotSupportedException("OWNED_DISPOSE_THROW");
            if (EmptyControlFixture.Behavior == "dispose-denial-caught") {
                try { EmptyControlFixture.DisposalProvider.Touch(); } catch (NotSupportedException) { }
            }
        }
        public string Name { get { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); } }
        public void Touch() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public bool TryRead(out int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public T Read<T>() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Download() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
    }
    public sealed class EmptyPrivateBuilderFixture
    {
        private EmptyPrivateBuilderFixture() { throw new InvalidOperationException("OWNED_INVALID_BUILDER_EXECUTED"); }
        public IEmptyContainerFixture Build(EmptyOptionsFixture options) { throw new InvalidOperationException("OWNED_INVALID_BUILD_EXECUTED"); }
    }
    public sealed class EmptyStaticBuilderFixture
    {
        public EmptyStaticBuilderFixture() { throw new InvalidOperationException("OWNED_INVALID_BUILDER_EXECUTED"); }
        public static IEmptyContainerFixture Build(EmptyOptionsFixture options) { throw new InvalidOperationException("OWNED_INVALID_BUILD_EXECUTED"); }
    }
    public sealed class EmptyGenericBuilderFixture
    {
        public EmptyGenericBuilderFixture() { throw new InvalidOperationException("OWNED_INVALID_BUILDER_EXECUTED"); }
        public IEmptyContainerFixture Build<T>(EmptyOptionsFixture options) { throw new InvalidOperationException("OWNED_INVALID_BUILD_EXECUTED"); }
    }
    public sealed class EmptyWrongReturnBuilderFixture
    {
        public EmptyWrongReturnBuilderFixture() { throw new InvalidOperationException("OWNED_INVALID_BUILDER_EXECUTED"); }
        public object Build(EmptyOptionsFixture options) { throw new InvalidOperationException("OWNED_INVALID_BUILD_EXECUTED"); }
    }
    public sealed class EmptyFactoryReturnsFixture
    {
        public EmptyFactoryReturnsFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        { EmptyControlFixture.Factories++; if (!(context is IEmptyContainerFixture) || provider == null) throw new InvalidOperationException("OWNED_EMPTY_ARGUMENTS"); }
    }
    public sealed class EmptyFactoryDisposalProviderFixture
    {
        public EmptyFactoryDisposalProviderFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        { EmptyControlFixture.Factories++; EmptyControlFixture.DisposalProvider = provider; }
    }
    public sealed class EmptyFactoryThrowsFixture
    {
        public EmptyFactoryThrowsFixture(IContainerDependencyFixture context, IDataDependencyFixture provider) { EmptyControlFixture.Factories++; provider.Touch(); }
    }
    public sealed class EmptyFactoryCatchesFixture
    {
        public EmptyFactoryCatchesFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        { EmptyControlFixture.Factories++; try { provider.Touch(); } catch (NotSupportedException) { } }
    }
    public interface ITraceLifetimeFixture : IDisposable { void UnsupportedUse(); }
    public static class TraceLifetimeControl
    {
        public static int Constructors, Disposals, Uses, Attributes;
        public static string Behavior;
        public static Action ConstructorCallback, DisposeCallback;
        public static void Reset(string behavior) { Constructors = Disposals = Uses = Attributes = 0; Behavior = behavior; ConstructorCallback = DisposeCallback = null; }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class TraceTrapAttribute : Attribute
    { public TraceTrapAttribute() { TraceLifetimeControl.Attributes++; throw new InvalidOperationException("OWNED_ATTRIBUTE_EXECUTED"); } }
    public sealed class TraceLifetimeFixture : ITraceLifetimeFixture
    {
        public TraceLifetimeFixture([TraceTrap] int iterations = 1)
        {
            TraceLifetimeControl.Constructors++;
            if (iterations != 1) throw new InvalidOperationException("OWNED_DEFAULT_ARGUMENT");
            if (TraceLifetimeControl.ConstructorCallback != null) {
                try { TraceLifetimeControl.ConstructorCallback(); } catch (NotSupportedException) { if (TraceLifetimeControl.Behavior != "ctor-catch") throw; }
            }
            if (TraceLifetimeControl.Behavior == "ctor-throw") throw new InvalidOperationException("OWNED_TRACE_CTOR_THROW");
        }
        public void Dispose()
        {
            TraceLifetimeControl.Disposals++;
            if (TraceLifetimeControl.DisposeCallback != null) {
                try { TraceLifetimeControl.DisposeCallback(); } catch (NotSupportedException) { if (TraceLifetimeControl.Behavior != "dispose-catch") throw; }
            }
            if (TraceLifetimeControl.Behavior == "dispose-throw") throw new InvalidOperationException("OWNED_TRACE_DISPOSE_THROW");
        }
        public void UnsupportedUse() { TraceLifetimeControl.Uses++; throw new NotSupportedException("OWNED_TRACE_USE_DENIED"); }
    }
    public class TraceUnsealedFixture : ITraceLifetimeFixture { public TraceUnsealedFixture(int iterations = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public abstract class TraceAbstractFixture : ITraceLifetimeFixture { public TraceAbstractFixture(int iterations = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TraceGenericFixture<T> : ITraceLifetimeFixture { public TraceGenericFixture(int iterations = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TracePrivateConstructorFixture : ITraceLifetimeFixture { private TracePrivateConstructorFixture(int iterations = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TraceWrongTypeFixture : ITraceLifetimeFixture { public TraceWrongTypeFixture(uint iterations = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TraceWrongNameFixture : ITraceLifetimeFixture { public TraceWrongNameFixture(int other = 1) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TraceRequiredFixture : ITraceLifetimeFixture { public TraceRequiredFixture(int iterations) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public sealed class TraceWrongDefaultFixture : ITraceLifetimeFixture { public TraceWrongDefaultFixture(int iterations = 2) { TraceLifetimeControl.Constructors++; } public void Dispose() { } public void UnsupportedUse() { } }
    public interface ITraceWrongContractFixture { }
    // Independently owned API shapes; public interface invocation on an internal implementation.
    public sealed class RegistrationDataFixture { }
    public sealed class RegistrationStyleFixture { }
    public interface IRegistrationFixture<out T, out D, out S>
    { IRegistrationFixture<T, D, S> ExternallyOwned(); }
    public interface IWrongExternalFixture<out T, out D, out S>
    { object ExternallyOwned(); }
    public static class RegistrationAssertions
    { public static void DenyProvider(object provider) { try { ((IDataDependencyFixture)provider).Touch(); } catch (NotSupportedException) { } } }
    public static class RegistrationControl
    {
        public static int Registrations, ExternalCalls;
        public static object Instance;
        public static Type Service;
        public static bool External;
        public static string Behavior, Order;
        public static void Reset(string behavior) { Registrations = ExternalCalls = 0; Instance = null; Service = null; External = false; Behavior = behavior; Order = ""; }
        public static void Event(string value) { Order += value + ">"; }
    }
    internal sealed class RegistrationFixture<T, D, S> : IRegistrationFixture<T, D, S>
    {
        public IRegistrationFixture<T, D, S> ExternallyOwned()
        {
            RegistrationControl.ExternalCalls++; RegistrationControl.Event("external");
            if (RegistrationControl.Behavior == "external-throw") throw new NotSupportedException("OWNED_EXTERNAL_THROW");
            if (RegistrationControl.Behavior == "external-null") return null;
            RegistrationControl.External = true; return this;
        }
    }
    public static class RegistrationExtensionsFixture
    {
        public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) where T : class
        {
            RegistrationControl.Registrations++; RegistrationControl.Event("register");
            if (builder == null || instance == null) throw new InvalidOperationException("OWNED_REGISTER_ARGUMENTS");
            RegistrationControl.Instance = instance; RegistrationControl.Service = typeof(T);
            if (RegistrationControl.Behavior == "register-throw") throw new NotSupportedException("OWNED_REGISTER_THROW");
            if (RegistrationControl.Behavior == "register-null") return null;
            return new RegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture>();
        }
    }
    public static class RegistrationNoConstraintFixture
    { public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public static class RegistrationWrongParameterFixture
    { public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(object builder, T instance) where T : class { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public static class RegistrationWrongReturnFixture
    { public static object RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) where T : class { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public static class RegistrationWrongArgumentsFixture
    { public static IRegistrationFixture<object, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) where T : class { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public sealed class RegistrationInstanceMethodFixture
    { public IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) where T : class { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public static class RegistrationWrongExternalFixture
    { public static IWrongExternalFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(RegisteredBuilderFixture builder, T instance) where T : class { throw new InvalidOperationException("OWNED_INVALID_REGISTRATION_EXECUTED"); } }
    public interface IRegisteredContainerFixture : IEmptyContainerFixture { object RegisteredTrace { get; } }
    public sealed class RegisteredBuilderFixture
    {
        public RegisteredBuilderFixture() { EmptyControlFixture.Constructors++; RegistrationControl.Event("builder"); }
        public IRegisteredContainerFixture Build(EmptyOptionsFixture options)
        {
            EmptyControlFixture.Builds++; RegistrationControl.Event("build");
            if (options != EmptyOptionsFixture.None || !RegistrationControl.External || RegistrationControl.Service != typeof(ITraceLifetimeFixture)
                || RegistrationControl.Registrations != 1 || RegistrationControl.ExternalCalls != 1) throw new InvalidOperationException("OWNED_EXACT_MAPPING_OWNERSHIP_REQUIRED");
            if (RegistrationControl.Behavior == "build-throw") throw new NotSupportedException("OWNED_REGISTERED_BUILD_THROW");
            return new RegisteredContainerFixture();
        }
    }
    internal sealed class RegisteredContainerFixture : IRegisteredContainerFixture
    {
        public object RegisteredTrace { get { return RegistrationControl.Instance; } }
        public void Dispose()
        {
            EmptyControlFixture.Disposals++; RegistrationControl.Event("container-dispose");
            if (!RegistrationControl.External) ((IDisposable)RegistrationControl.Instance).Dispose();
            if (RegistrationControl.Behavior == "container-dispose-throw") throw new NotSupportedException("OWNED_REGISTERED_DISPOSE_THROW");
            if (RegistrationControl.Behavior == "container-denial-caught") try { EmptyControlFixture.DisposalProvider.Touch(); } catch (NotSupportedException) { }
        }
        public string Name { get { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); } }
        public void Touch() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public bool TryRead(out int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public T Read<T>() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Download() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
    }
    public sealed class RegisteredFactoryFixture
    {
        public RegisteredFactoryFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        {
            EmptyControlFixture.Factories++; RegistrationControl.Event("factory"); EmptyControlFixture.DisposalProvider = provider;
            IRegisteredContainerFixture registered = context as IRegisteredContainerFixture;
            if (registered == null || !Object.ReferenceEquals(registered.RegisteredTrace, RegistrationControl.Instance)
                || !(registered.RegisteredTrace is ITraceLifetimeFixture) || TraceLifetimeControl.Disposals != 0) throw new InvalidOperationException("OWNED_SAME_LIVE_TRACE_REQUIRED");
            if (RegistrationControl.Behavior == "factory-throw") throw new NotSupportedException("OWNED_REGISTERED_FACTORY_THROW");
            if (RegistrationControl.Behavior == "provider-caught") try { provider.Touch(); } catch (NotSupportedException) { }
        }
    }
    // Project lifetime model only; no SDK identity or original service semantics.
    public interface IScopedProjectStoreFixture : IProductStoreFixture { ScopedIdFixture Id { get; } void Save(); }
    public sealed class ScopedIdFixture
    {
        readonly uint number;
        public ScopedIdFixture(uint value) { ProjectScopeControl.IdConstructors++; number = value; ProjectScopeControl.Probe("id-constructor"); }
        public uint ProjectNumber { get { ProjectScopeControl.IdGetters++; ProjectScopeControl.Probe("id-getter"); return number; } }
        public bool IsFixture { get { return number == 1; } }
    }
    public static class ProjectScopeControl
    {
        public static int Constructors, Disposals, IdConstructors, IdGetters;
        public static string Behavior;
        public static IScopedProjectStoreFixture Store;
        public static IDataDependencyFixture Provider;
        public static Action<string> ResourceProbe;
        public static void Reset(string behavior) { Constructors = Disposals = IdConstructors = IdGetters = 0; Behavior = behavior; Store = null; Provider = null; ResourceProbe = null; }
        public static void Probe(string phase) { if (ResourceProbe != null) ResourceProbe(phase); }
        public static void Later(string phase)
        {
            if (Behavior == phase + "-store-caught") try { Store.Save(); } catch (NotSupportedException) { }
            if (Behavior == phase + "-provider-caught") try { Provider.Touch(); } catch (NotSupportedException) { }
        }
    }
    public static class ScopedRegistrationExtensionsFixture
    {
        public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(ScopedBuilderFixture builder, T instance) where T : class
        {
            if (builder == null || instance == null) throw new InvalidOperationException("OWNED_SCOPE_REGISTRATION_ARGUMENTS");
            RegistrationControl.Registrations++; RegistrationControl.Event("register"); RegistrationControl.Instance = instance; RegistrationControl.Service = typeof(T);
            ProjectScopeControl.Probe("registration");
            if (ProjectScopeControl.ResourceProbe != null) return new ScopedRegistrationFixture<T>();
            return new RegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture>();
        }
    }
    internal sealed class ScopedRegistrationFixture<T> : IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture>
    {
        public IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> ExternallyOwned()
        {
            RegistrationControl.ExternalCalls++; RegistrationControl.Event("external"); ProjectScopeControl.Probe("external");
            RegistrationControl.External = true; return this;
        }
    }
    public sealed class ScopedBuilderFixture
    {
        public ScopedBuilderFixture() { EmptyControlFixture.Constructors++; RegistrationControl.Event("builder"); ProjectScopeControl.Probe("builder"); }
        public IRegisteredContainerFixture Build(EmptyOptionsFixture options)
        {
            EmptyControlFixture.Builds++; RegistrationControl.Event("build");
            ProjectScopeControl.Probe("build");
            if (options != EmptyOptionsFixture.None || !RegistrationControl.External || RegistrationControl.Service != typeof(ITraceLifetimeFixture)
                || RegistrationControl.Registrations != 1 || RegistrationControl.ExternalCalls != 1) throw new InvalidOperationException("OWNED_SCOPE_EXTERNAL_MAPPING_REQUIRED");
            return new ScopedContainerFixture();
        }
    }
    internal sealed class ScopedContainerFixture : IRegisteredContainerFixture
    {
        public object RegisteredTrace { get { return RegistrationControl.Instance; } }
        public void Dispose()
        {
            EmptyControlFixture.Disposals++; RegistrationControl.Event("container-dispose"); ProjectScopeControl.Later("container");
            ProjectScopeControl.Probe("container-dispose");
            if (!RegistrationControl.External) ((IDisposable)RegistrationControl.Instance).Dispose();
            if (ProjectScopeControl.Behavior == "container-dispose-throw") throw new NotSupportedException("OWNED_SCOPE_CONTAINER_DISPOSE_THROW");
        }
        public string Name { get { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); } }
        public void Touch() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public bool TryRead(out int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public T Read<T>() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
        public void Download() { throw new NotSupportedException("OWNED_CONTEXT_UNSUPPORTED"); }
    }
    public sealed class ScopedGenuineFactoryFixture : IFactoryFixture
    {
        public ScopedGenuineFactoryFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        {
            EmptyControlFixture.Factories++; RegistrationControl.Event("factory"); ProjectScopeControl.Provider = provider;
            if (!(context is IRegisteredContainerFixture) || TraceLifetimeControl.Disposals != 0) throw new InvalidOperationException("OWNED_SCOPE_LIVE_CONTEXT_REQUIRED");
            if (ProjectScopeControl.Behavior == "resource-provider") try { provider.Touch(); } catch (NotSupportedException) { }
            ProjectScopeControl.Probe("factory");
        }
        public T Create<T>() { throw new NotSupportedException("OWNED_FACTORY_METHOD_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_FACTORY_METHOD_UNSUPPORTED"); }
    }
    public sealed class ScopedProjectFixture : IDisposable
    {
        public ScopedProjectFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IScopedProjectStoreFixture store)
        {
            ProjectScopeControl.Constructors++; RegistrationControl.Event("project"); ProjectScopeControl.Store = store;
            ProjectScopeControl.Probe("project");
            if (!(factory is ScopedGenuineFactoryFixture) || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel
                || TraceLifetimeControl.Disposals != 0 || EmptyControlFixture.Disposals != 0) throw new InvalidOperationException("OWNED_SCOPE_ARGUMENTS_LIFETIME");
            ScopedIdFixture first = store.Id, second = store.Id;
            ProjectScopeControl.Probe("project-after-getter");
            if (!Object.ReferenceEquals(first, second) || !first.IsFixture) throw new InvalidOperationException("OWNED_SCOPE_STABLE_ID_REQUIRED");
            if (ProjectScopeControl.Behavior == "project-throw") throw new NotSupportedException("OWNED_SCOPE_PROJECT_THROW");
        }
        public void Dispose()
        {
            ProjectScopeControl.Disposals++; RegistrationControl.Event("project-dispose");
            ProjectScopeControl.Probe("project-dispose");
            if (TraceLifetimeControl.Disposals != 0 || EmptyControlFixture.Disposals != 0) throw new InvalidOperationException("OWNED_SCOPE_DISPOSAL_ORDER");
            ProjectScopeControl.Later("project");
            if (ProjectScopeControl.Behavior == "project-dispose-throw") throw new NotSupportedException("OWNED_SCOPE_PROJECT_DISPOSE_THROW");
        }
    }
    public sealed class ScopedNoDisposeFixture
    { public ScopedNoDisposeFixture(IFactoryFixture f, string n, GroupAddressStyleFixture s, IScopedProjectStoreFixture p) { throw new InvalidOperationException("OWNED_INVALID_SCOPE_EXECUTED"); } public void Dispose() { } }
    public sealed class ScopedExplicitDisposeFixture : IDisposable
    { public ScopedExplicitDisposeFixture(IFactoryFixture f, string n, GroupAddressStyleFixture s, IScopedProjectStoreFixture p) { throw new InvalidOperationException("OWNED_INVALID_SCOPE_EXECUTED"); } void IDisposable.Dispose() { } }
    public sealed class ScopedWrongMapFixture : IDisposable
    { public ScopedWrongMapFixture(IFactoryFixture f, string n, GroupAddressStyleFixture s, IScopedProjectStoreFixture p) { throw new InvalidOperationException("OWNED_INVALID_SCOPE_EXECUTED"); } public void Dispose() { } void IDisposable.Dispose() { } }
    public sealed class ScopedWrongConstructorFixture : IDisposable
    { public ScopedWrongConstructorFixture(object f, string n, GroupAddressStyleFixture s, IScopedProjectStoreFixture p) { throw new InvalidOperationException("OWNED_INVALID_SCOPE_EXECUTED"); } public void Dispose() { } }
    public sealed class ScopedGenericProjectFixture<T> : IDisposable
    { public ScopedGenericProjectFixture(IFactoryFixture f, string n, GroupAddressStyleFixture s, IScopedProjectStoreFixture p) { throw new InvalidOperationException("OWNED_INVALID_SCOPE_EXECUTED"); } public void Dispose() { } }
    public static class ProjectIdAssertions
    {
        static void Denied(Action action)
        {
            try { action(); } catch (NotSupportedException error) {
                if (error.Message.StartsWith("PROJECT_FIXTURE_UNSUPPORTED:", StringComparison.Ordinal)) return;
                throw;
            }
            throw new InvalidOperationException("OWNED_ID_DENIAL_REQUIRED");
        }
        static void Rejected(RealProxy proxy, IMessage message)
        {
            try {
                var response = proxy.Invoke(message) as IMethodReturnMessage;
                if (response != null && response.Exception is NotSupportedException && response.Exception.Message.StartsWith("PROJECT_FIXTURE_UNSUPPORTED:", StringComparison.Ordinal)) return;
            } catch (NotSupportedException error) { if (error.Message.StartsWith("PROJECT_FIXTURE_UNSUPPORTED:", StringComparison.Ordinal)) return; throw; }
            throw new InvalidOperationException("OWNED_ID_MESSAGE_DENIAL_REQUIRED");
        }
        public static void AssertAll(object expected, object store)
        {
            var s = (IProjectIdStoreFixture)store;
            if (!Object.ReferenceEquals(expected, s.Id) || !Object.ReferenceEquals(expected, s.Id) || !ProjectIdFixture.IsFixture(expected))
                throw new InvalidOperationException("OWNED_ID_STABLE_OBJECT_VALUE");
            Denied(delegate { GC.KeepAlive(s.OtherId); });
            Denied(delegate { GC.KeepAlive(s.Name); });
            Denied(delegate { s.Save(); });
            Denied(delegate { s.Download(); });
            Denied(delegate { int value = 9; s.TryRead(out value); });
            Denied(delegate { GC.KeepAlive(s.Read<string>()); });
            Denied(delegate { int value = 9; s.Change(ref value); });
            RealProxy proxy = RemotingServices.GetRealProxy(store);
            MethodInfo getter = typeof(IProjectIdStoreFixture).GetMethod("get_Id");
            Rejected(proxy, new MethodMessageFixture(typeof(IForeignIdStoreFixture).GetMethod("get_Id"), new object[0], 0, "get_Id"));
            Rejected(proxy, new MethodMessageFixture(getter, new object[] { 1 }, 0, "get_Id"));
            Rejected(proxy, new MethodMessageFixture(getter, new object[0], 1, "get_Id"));
            Rejected(proxy, new MethodMessageFixture(getter, new object[0], 0, "Unknown"));
            Rejected(proxy, new MethodMessageFixture(null, new object[0], 0, "get_Id"));
            Rejected(proxy, null);
            Denied(delegate { GC.KeepAlive(s.OtherId); });
            for (int i = 0; i < 20; i++) Denied(delegate { GC.KeepAlive(s.Id); });
        }
        public static void AssertSaturation(object expected, object store)
        {
            var s = (IProjectIdStoreFixture)store;
            for (int i = 0; i < 16; i++) if (!Object.ReferenceEquals(expected, s.Id)) throw new InvalidOperationException("OWNED_ID_REFERENCE_CHANGED");
            for (int i = 0; i < 20; i++) Denied(delegate { GC.KeepAlive(s.Id); });
        }
    }
}
