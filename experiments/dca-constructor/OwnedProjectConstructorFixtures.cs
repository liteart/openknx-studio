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
    // Two fixed owned slots model registration/lifetime only; no general container or storage behavior.
    public interface IOpaqueUndoFixture { }
    public interface IUndoDeniedFixture { IOpaqueUndoFixture GetStore(Guid value); IOpaqueUndoFixture NewStore(); string RootPath { get; } }
    public interface IUndoWrongGuidFixture { IOpaqueUndoFixture GetStore(object value); IOpaqueUndoFixture NewStore(); string RootPath { get; } }
    public interface IUndoWrongReturnFixture { object GetStore(Guid value); IOpaqueUndoFixture NewStore(); string RootPath { get; } }
    public interface IUndoExtraFixture { IOpaqueUndoFixture GetStore(Guid value); IOpaqueUndoFixture NewStore(); string RootPath { get; } void Extra(); }
    public interface IUndoWrongNameFixture { IOpaqueUndoFixture GetStore(Guid value); IOpaqueUndoFixture FreshStore(); string RootPath { get; } }
    public interface IUndoGenericMethodFixture { IOpaqueUndoFixture GetStore<T>(Guid value); IOpaqueUndoFixture NewStore(); string RootPath { get; } }
    public interface IUndoWrongGetterFixture { IOpaqueUndoFixture GetStore(Guid value); IOpaqueUndoFixture NewStore(); string get_RootPath(); }
    public interface IUndoInheritedFixture : IUndoDeniedFixture { }
    public interface IUndoGenericFixture<T> { }
    internal interface IUndoHiddenFixture { }
    public static class UndoOwnedControl
    {
        public static object Trace, Undo;
        public static bool TraceExternal, UndoExternal;
        public static int Registrations, ExternalCalls, Builds, Factories, Projects, ProjectDisposals, ContainerDisposals;
        public static string Order = "";
        public static bool ThrowProject;
        public static string Failure;
        public static Action<string> Callback;
        public static void Point(string phase) { Order += phase + ">"; if (Callback != null) Callback(phase); }
    }
    public sealed class UndoBuilderFixture
    {
        public UndoBuilderFixture() { UndoOwnedControl.Point("builder"); }
        public IRegisteredContainerFixture Build(EmptyOptionsFixture options)
        {
            UndoOwnedControl.Builds++; UndoOwnedControl.Point("build");
            if (options != EmptyOptionsFixture.None || UndoOwnedControl.Registrations != 2 || UndoOwnedControl.ExternalCalls != 2 || !UndoOwnedControl.TraceExternal || !UndoOwnedControl.UndoExternal) throw new InvalidOperationException("OWNED_UNDO_MAPPING_REQUIRED");
            return new UndoContainerFixture();
        }
    }
    public static class UndoRegistrationExtensionsFixture
    {
        public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(UndoBuilderFixture builder, T instance) where T : class
        {
            if (builder == null || instance == null) throw new InvalidOperationException("OWNED_UNDO_REGISTRATION_ARGUMENTS");
            int slot = ++UndoOwnedControl.Registrations;
            if (slot == 1 && typeof(T) == typeof(ITraceLifetimeFixture)) UndoOwnedControl.Trace = instance;
            else if (slot == 2 && typeof(T) == typeof(IUndoDeniedFixture)) UndoOwnedControl.Undo = instance;
            else throw new InvalidOperationException("OWNED_UNDO_EXACT_TWO_SLOTS");
            UndoOwnedControl.Point(slot == 1 ? "trace-registration" : "registration");
            if (slot == 2 && UndoOwnedControl.Failure == "register-null") return null;
            return new UndoRegistrationFixture<T>(slot);
        }
    }
    internal sealed class UndoRegistrationFixture<T> : IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture>
    {
        readonly int slot;
        internal UndoRegistrationFixture(int selected) { slot = selected; }
        public IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> ExternallyOwned()
        {
            UndoOwnedControl.ExternalCalls++;
            if (slot == 2 && UndoOwnedControl.Failure == "external-null") return null;
            if (slot == 1) UndoOwnedControl.TraceExternal = true; else UndoOwnedControl.UndoExternal = true;
            UndoOwnedControl.Point(slot == 1 ? "trace-external" : "undo-external"); return this;
        }
    }
    internal sealed class UndoContainerFixture : IRegisteredContainerFixture
    {
        public object RegisteredTrace { get { return UndoOwnedControl.Trace; } }
        public void Dispose() { UndoOwnedControl.ContainerDisposals++; UndoOwnedControl.Point("container-dispose"); }
        public string Name { get { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); } }
        public void Touch() { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); }
        public bool TryRead(out int value) { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); }
        public T Read<T>() { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); }
        public void Download() { throw new NotSupportedException("OWNED_UNDO_CONTEXT_UNSUPPORTED"); }
    }
    public sealed class UndoFactoryFixture : IFactoryFixture
    {
        public UndoFactoryFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        {
            if (!(context is IRegisteredContainerFixture) || !UndoOwnedControl.TraceExternal || !UndoOwnedControl.UndoExternal) throw new InvalidOperationException("OWNED_UNDO_LIVE_CONTEXT_REQUIRED");
            UndoOwnedControl.Factories++; UndoOwnedControl.Point("factory");
        }
        public T Create<T>() { throw new NotSupportedException("OWNED_UNDO_FACTORY_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_UNDO_FACTORY_UNSUPPORTED"); }
    }
    public sealed class UndoProjectFixture : IDisposable
    {
        public UndoProjectFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IScopedProjectStoreFixture store)
        {
            if (!(factory is UndoFactoryFixture) || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel) throw new InvalidOperationException("OWNED_UNDO_PROJECT_ARGUMENTS");
            UndoOwnedControl.Projects++; UndoOwnedControl.Point("project");
            if (UndoOwnedControl.ThrowProject) throw new NotSupportedException("OWNED_UNDO_PROJECT_THROW");
            ScopedIdFixture first = store.Id, second = store.Id;
            if (!Object.ReferenceEquals(first, second)) throw new InvalidOperationException("OWNED_UNDO_ID_IDENTITY");
        }
        public void Dispose() { UndoOwnedControl.ProjectDisposals++; UndoOwnedControl.Point("project-dispose"); }
    }
    // Source25 fixtures model only two registrations, one callback and lifetime observations.
    public interface IUndoFactoryRegistrationFixture<T, TData, TStyle>
    {
        IUndoFactoryRegistrationFixture<T, TData, TStyle> SingleInstance();
        IUndoFactoryRegistrationFixture<T, TData, TStyle> ExternallyOwned();
    }
    public interface IUndoFactoryWrongSingleFixture<T, TData, TStyle> { object SingleInstance(); }
    public interface IUndoFactoryWrongExternalFixture<T, TData, TStyle> { object ExternallyOwned(); }
    public static class UndoFactoryOwnedControl
    {
        public static Action<IContainerDependencyFixture> Activate;
        public static Action ProjectActivate;
        public static bool SkipActivation, ProjectActivation, Shared, External;
        public static int SingleCalls, Activations, CaughtFaults;
    }
    public static class UndoFactoryRegistrationExtensionsFixture
    {
        public static IRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> RegisterInstance<T>(UndoBuilderFixture builder, T instance) where T : class
        { return UndoRegistrationExtensionsFixture.RegisterInstance<T>(builder, instance); }
        public static IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> Register<T>(UndoBuilderFixture builder, Func<IContainerDependencyFixture, T> callback)
        {
            if (builder == null || callback == null || typeof(T) != typeof(IUndoDeniedFixture) || ++UndoOwnedControl.Registrations != 2) throw new InvalidOperationException("OWNED_FACTORY_EXACT_TWO_SLOTS");
            UndoFactoryOwnedControl.Activate = delegate(IContainerDependencyFixture context) {
                UndoFactoryOwnedControl.Activations++;
                try { UndoOwnedControl.Undo = callback(context); }
                catch (Exception) { UndoFactoryOwnedControl.CaughtFaults++; }
                UndoOwnedControl.Point("after-activation");
            };
            UndoOwnedControl.Point("registration"); return new UndoFactoryRegistrationFixture<T>();
        }
    }
    internal sealed class UndoFactoryRegistrationFixture<T> : IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture>
    {
        public IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> SingleInstance()
        {
            if (++UndoFactoryOwnedControl.SingleCalls != 1 || UndoFactoryOwnedControl.External) throw new InvalidOperationException("OWNED_FACTORY_SHARING_ORDER");
            UndoFactoryOwnedControl.Shared = true; UndoOwnedControl.Point("single"); return this;
        }
        public IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> ExternallyOwned()
        {
            if (!UndoFactoryOwnedControl.Shared || UndoFactoryOwnedControl.External) throw new InvalidOperationException("OWNED_FACTORY_OWNERSHIP_ORDER");
            UndoFactoryOwnedControl.External = true; UndoOwnedControl.UndoExternal = true; UndoOwnedControl.ExternalCalls++; UndoOwnedControl.Point("external"); return this;
        }
    }
    public sealed class UndoFactoryDeliveryFixture : IFactoryFixture
    {
        public UndoFactoryDeliveryFixture(IContainerDependencyFixture context, IDataDependencyFixture provider)
        {
            if (!(context is IRegisteredContainerFixture) || !UndoFactoryOwnedControl.Shared || !UndoFactoryOwnedControl.External) throw new InvalidOperationException("OWNED_FACTORY_LIVE_CONTEXT");
            UndoOwnedControl.Factories++; UndoOwnedControl.Point("factory");
            if (UndoFactoryOwnedControl.ProjectActivation) UndoFactoryOwnedControl.ProjectActivate = delegate { UndoFactoryOwnedControl.Activate(context); };
            else if (!UndoFactoryOwnedControl.SkipActivation) UndoFactoryOwnedControl.Activate(context);
        }
        public T Create<T>() { throw new NotSupportedException("OWNED_FACTORY_UNSUPPORTED"); }
        public void Change(ref int value) { throw new NotSupportedException("OWNED_FACTORY_UNSUPPORTED"); }
    }
    public sealed class UndoFactoryProjectFixture : IDisposable
    {
        public UndoFactoryProjectFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, IScopedProjectStoreFixture store)
        {
            if (!(factory is UndoFactoryDeliveryFixture) || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel) throw new InvalidOperationException("OWNED_FACTORY_PROJECT_ARGUMENTS");
            UndoOwnedControl.Projects++; UndoOwnedControl.Point("project");
            if (UndoFactoryOwnedControl.ProjectActivation) UndoFactoryOwnedControl.ProjectActivate();
            if (UndoOwnedControl.ThrowProject) throw new NotSupportedException("OWNED_FACTORY_PROJECT_THROW");
            ScopedIdFixture first = store.Id, second = store.Id;
            if (!Object.ReferenceEquals(first, second)) throw new InvalidOperationException("OWNED_FACTORY_ID_IDENTITY");
        }
        public void Dispose() { UndoOwnedControl.ProjectDisposals++; UndoOwnedControl.Point("project-dispose"); }
    }
    public static class UndoFactoryWrongContextFixture
    { public static IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> Register<T>(UndoBuilderFixture builder, Func<object, T> callback) { throw new InvalidOperationException("OWNED_INVALID_FACTORY_API_INVOKED"); } }
    public static class UndoFactoryWrongArityFixture
    { public static IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> Register<T>(UndoBuilderFixture builder, Func<IContainerDependencyFixture, object, T> callback) { throw new InvalidOperationException("OWNED_INVALID_FACTORY_API_INVOKED"); } }
    public static class UndoFactoryWrongConstraintFixture
    { public static IUndoFactoryRegistrationFixture<T, RegistrationDataFixture, RegistrationStyleFixture> Register<T>(UndoBuilderFixture builder, Func<IContainerDependencyFixture, T> callback) where T : class { throw new InvalidOperationException("OWNED_INVALID_FACTORY_API_INVOKED"); } }
    public static class UndoFactoryWrongReturnFixture
    { public static object Register<T>(UndoBuilderFixture builder, Func<IContainerDependencyFixture, T> callback) { throw new InvalidOperationException("OWNED_INVALID_FACTORY_API_INVOKED"); } }
    // Source27 uses new owned contracts; it implements no stream/cache business operation.
    public interface ICacheStreamFixture
    {
        string Id { get; } bool Exists { get; }
        System.IO.Stream AsStream(bool value); System.IO.Stream AsMultiWriteWithinOneTransactionStream();
        void Delete(); long GetUtcFileTime(); ushort Mask { set; }
    }
    public interface ICacheProjectStoreFixture { ScopedIdFixture Id { get; } ICacheStreamFixture LargeStringCache { get; } }
    public interface ICacheWrongStreamFixture
    {
        string Id { get; } int Exists { get; }
        System.IO.Stream AsStream(bool value); System.IO.Stream AsMultiWriteWithinOneTransactionStream();
        void Delete(); long GetUtcFileTime(); ushort Mask { set; }
    }
    public interface ICacheInheritedStreamFixture : ICacheStreamFixture { }
    public interface ICacheGenericStreamFixture<T> { }
    public static class CacheOwnedControl
    { public static int IdCalls = 2, Caught; public static bool SkipCache, ThrowProject; public static object Reference; }
    public sealed class CacheProjectFixture : IDisposable
    {
        public CacheProjectFixture(IFactoryFixture factory, string name, GroupAddressStyleFixture style, ICacheProjectStoreFixture store)
        {
            if (!(factory is UndoFactoryDeliveryFixture) || name != "Task009 offline constructor diagnostic" || style != GroupAddressStyleFixture.ThreeLevel) throw new InvalidOperationException("OWNED_CACHE_PROJECT_ARGUMENTS");
            UndoOwnedControl.Projects++; UndoOwnedControl.Point("project");
            ScopedIdFixture first = null;
            for (int i = 0; i < CacheOwnedControl.IdCalls; i++) { ScopedIdFixture value = store.Id; if (i == 0) first = value; else if (!Object.ReferenceEquals(first, value)) throw new InvalidOperationException("OWNED_CACHE_ID_IDENTITY"); }
            UndoOwnedControl.Point("before-cache");
            if (!CacheOwnedControl.SkipCache) {
                try { CacheOwnedControl.Reference = store.LargeStringCache; }
                catch (Exception) { CacheOwnedControl.Caught++; }
            }
            UndoOwnedControl.Point("after-cache");
            if (CacheOwnedControl.ThrowProject) throw new NotSupportedException("OWNED_CACHE_PROJECT_THROW");
        }
        public void Dispose() { UndoOwnedControl.ProjectDisposals++; UndoOwnedControl.Point("project-dispose"); }
    }
    public sealed class CacheMessageFixture : IMethodCallMessage
    {
        public MethodBase Method; public string Name = "get_LargeStringCache", Failure;
        public int Count, Incoming, MethodReads, ArgsReads, ContextReads; public bool SingleRead;
        public object[] Arguments = new object[0]; public Action ContextCallback;
        void Check(string field, int reads) { if (Failure == field || (SingleRead && reads > 1)) throw new InvalidOperationException("OWNED_ORIGINAL_MESSAGE_GETTER:" + field); }
        public MethodBase MethodBase { get { MethodReads++; Check("method", MethodReads); return Method; } }
        public string MethodName { get { return Name; } }
        public int ArgCount { get { return Count; } } public int InArgCount { get { return Incoming; } }
        public object[] Args { get { ArgsReads++; Check("args", ArgsReads); return Arguments; } }
        public LogicalCallContext LogicalCallContext { get { ContextReads++; Check("context", ContextReads); if (ContextCallback != null) ContextCallback(); return null; } }
        public bool HasVarArgs { get { throw new InvalidOperationException("OWNED_MESSAGE_UNUSED_PROPERTY"); } }
        public object MethodSignature { get { throw new InvalidOperationException("OWNED_MESSAGE_UNUSED_PROPERTY"); } }
        public string TypeName { get { throw new InvalidOperationException("OWNED_MESSAGE_UNUSED_PROPERTY"); } }
        public string Uri { get { return null; } set { throw new InvalidOperationException("OWNED_MESSAGE_UNUSED_PROPERTY"); } }
        public IDictionary Properties { get { throw new InvalidOperationException("OWNED_MESSAGE_UNUSED_PROPERTY"); } }
        public object GetArg(int index) { throw new InvalidOperationException("OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED"); }
        public string GetArgName(int index) { throw new InvalidOperationException("OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED"); }
        public object[] InArgs { get { throw new InvalidOperationException("OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED"); } }
        public object GetInArg(int index) { throw new InvalidOperationException("OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED"); }
        public string GetInArgName(int index) { throw new InvalidOperationException("OWNED_MESSAGE_ARGUMENT_VALUE_INSPECTED"); }
    }
    public sealed class CacheCustomMethodFixture : MethodInfo
    {
        static Exception Unsupported() { return new InvalidOperationException("OWNED_CUSTOM_METHOD_METADATA_INSPECTED"); }
        public override MethodAttributes Attributes { get { throw Unsupported(); } }
        public override RuntimeMethodHandle MethodHandle { get { throw Unsupported(); } }
        public override Type DeclaringType { get { throw Unsupported(); } }
        public override string Name { get { throw Unsupported(); } }
        public override Type ReflectedType { get { throw Unsupported(); } }
        public override Type ReturnType { get { throw Unsupported(); } }
        public override ICustomAttributeProvider ReturnTypeCustomAttributes { get { throw Unsupported(); } }
        public override MethodInfo GetBaseDefinition() { throw Unsupported(); }
        public override MethodImplAttributes GetMethodImplementationFlags() { throw Unsupported(); }
        public override ParameterInfo[] GetParameters() { throw Unsupported(); }
        public override object Invoke(object obj, BindingFlags flags, Binder binder, object[] arguments, System.Globalization.CultureInfo culture) { throw Unsupported(); }
        public override object[] GetCustomAttributes(bool inherit) { throw Unsupported(); }
        public override object[] GetCustomAttributes(Type attribute, bool inherit) { throw Unsupported(); }
        public override bool IsDefined(Type attribute, bool inherit) { throw Unsupported(); }
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

// Distinct Source32 owned contracts; no proprietary SDK namespaces or values.
namespace Owned.Task009
{
    public interface IContextParentBaseFixture { string DisplayName { get; } }
    public interface IContextParentFixture : IContextParentBaseFixture { string Name { get; } void Unsupported(); T Read<T>(); }
    public interface IContextArgumentFixture { object Project { get; } void Unsupported(); }
    public interface IContextKeyFixture { }
    public sealed class ContextBaggageFixture { }
    public interface IContextImportInformationFixture { }
    public sealed class ContextImporterFixture { }
    public static class ContextOwnedControl
    {
        public static int Collections, Projects, Disposals, Caught;
        public static object Parent, Context, Collection, Factory; public static Action<string> Callback;
        public static bool ThrowCollection, ThrowProject;
        public static void Point(string phase) { if (Callback != null) Callback(phase); }
        public static void ParentName() { try { GC.KeepAlive(((IContextParentFixture)Parent).Name); } catch (Exception) { Caught++; } }
        public static void ContextProject() { try { GC.KeepAlive(((IContextArgumentFixture)Context).Project); } catch (Exception) { Caught++; } }
        public static void ParentInherited() { try { GC.KeepAlive(((IContextParentFixture)Parent).DisplayName); } catch (Exception) { Caught++; } }
        public static void ParentGeneric() { try { GC.KeepAlive(((IContextParentFixture)Parent).Read<string>()); } catch (Exception) { Caught++; } }
        public static void ParentVoid() { try { ((IContextParentFixture)Parent).Unsupported(); } catch (Exception) { Caught++; } }
        public static void ParentObject() { try { GC.KeepAlive(Parent.GetType()); } catch (Exception) { Caught++; } }
        public static void ContextObject() { try { GC.KeepAlive(Context.ToString()); } catch (Exception) { Caught++; } }
        public static void Importer(Delegate value)
        {
            var callback = (Func<ICacheProjectStoreFixture, ContextProjectFixture, IContextKeyFixture, Func<string,string>, Func<System.Collections.Generic.IEnumerable<ContextBaggageFixture>,bool>, Action<IContextImportInformationFixture>, Predicate<IContextImportInformationFixture>, ContextImporterFixture>)value;
            try { callback(null, null, null, null, null, null, null); }
            catch (NotSupportedException error) { if (error.Message != "CONTEXT_IMPORTER_UNSUPPORTED") throw; Caught++; return; }
            throw new InvalidOperationException("OWNED_CONTEXT_IMPORTER_NO_SUCCESS");
        }
    }
    public sealed class ContextCollectionFixture
    {
        public ContextCollectionFixture(IContextParentFixture parent, IFactoryFixture factory, Func<ICacheProjectStoreFixture, ContextProjectFixture, IContextKeyFixture, Func<string,string>, Func<System.Collections.Generic.IEnumerable<ContextBaggageFixture>,bool>, Action<IContextImportInformationFixture>, Predicate<IContextImportInformationFixture>, ContextImporterFixture> importer)
        {
            if (parent == null || !(factory is UndoFactoryDeliveryFixture) || importer == null) throw new InvalidOperationException("OWNED_CONTEXT_COLLECTION_ARGUMENTS");
            ContextOwnedControl.Parent = parent; ContextOwnedControl.Factory = factory; ContextOwnedControl.Collections++;
            ContextOwnedControl.Point("collection"); if (ContextOwnedControl.ThrowCollection) throw new NotSupportedException("OWNED_CONTEXT_COLLECTION_THROW");
            ContextOwnedControl.Collection = this;
        }
    }
    public sealed class ContextProjectFixture : IDisposable
    {
        public ContextProjectFixture(ContextCollectionFixture collection, ICacheProjectStoreFixture store, IFactoryFixture factory, IContextArgumentFixture context)
        {
            if (collection == null || store == null || context == null || !Object.ReferenceEquals(collection, ContextOwnedControl.Collection) || !Object.ReferenceEquals(factory, ContextOwnedControl.Factory)) throw new InvalidOperationException("OWNED_CONTEXT_PROJECT_ARGUMENTS");
            ContextOwnedControl.Context = context; ContextOwnedControl.Projects++; ContextOwnedControl.Point("project");
            if (ContextOwnedControl.ThrowProject) throw new NotSupportedException("OWNED_CONTEXT_PROJECT_THROW");
            ScopedIdFixture id = store.Id; if (!Object.ReferenceEquals(id, store.Id) || store.LargeStringCache == null) throw new InvalidOperationException("OWNED_CONTEXT_STORE_PROGRESS");
        }
        public void Dispose() { ContextOwnedControl.Disposals++; UndoOwnedControl.ProjectDisposals++; ContextOwnedControl.Point("project-dispose"); }
    }
    public sealed class ContextWrongCollectionFixture { public ContextWrongCollectionFixture(object parent, IFactoryFixture factory, Delegate importer) { throw new InvalidOperationException("OWNED_INVALID_CONTEXT_API_EFFECT"); } }
    public sealed class ContextWrongProjectFixture : IDisposable { public ContextWrongProjectFixture(object collection, ICacheProjectStoreFixture store, IFactoryFixture factory, IContextArgumentFixture context) { throw new InvalidOperationException("OWNED_INVALID_CONTEXT_API_EFFECT"); } public void Dispose() { } }
}

namespace Owned.Task009
{
    // Every possible message property takes the same finite recursive refusal path.
    public sealed class ContextReentryMessageFixture : System.Runtime.Remoting.Messaging.IMethodCallMessage
    {
        public Action Callback; public int Reads, Cutoffs; public bool Sentinel;
        object Touch()
        {
            Reads++;
            if (Reads > 16) { Sentinel = true; throw new NotSupportedException("OWNED_CONTEXT_REENTRY_SENTINEL"); }
            try { Callback(); }
            catch (NotSupportedException error) { if (error.Message == "CONTEXT_ARGUMENT_BUDGET:parent") Cutoffs++; }
            throw new NotSupportedException("OWNED_CONTEXT_REENTRY_UNWIND");
        }
        public System.Reflection.MethodBase MethodBase { get { return (System.Reflection.MethodBase)Touch(); } }
        public string MethodName { get { return (string)Touch(); } } public int ArgCount { get { return (int)Touch(); } }
        public object[] Args { get { return (object[])Touch(); } } public int InArgCount { get { return (int)Touch(); } }
        public object[] InArgs { get { return (object[])Touch(); } } public bool HasVarArgs { get { return (bool)Touch(); } }
        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext { get { return (System.Runtime.Remoting.Messaging.LogicalCallContext)Touch(); } }
        public object MethodSignature { get { return Touch(); } } public string TypeName { get { return (string)Touch(); } }
        public string Uri { get { return (string)Touch(); } set { Touch(); } }
        public System.Collections.IDictionary Properties { get { return (System.Collections.IDictionary)Touch(); } }
        public object GetArg(int index) { return Touch(); } public string GetArgName(int index) { return (string)Touch(); }
        public object GetInArg(int index) { return Touch(); } public string GetInArgName(int index) { return (string)Touch(); }
    }
}

// Source34 declarations are separate from all earlier owned fixture families.
namespace Owned.Task009
{
    public interface IStoreRequestFixture { ScopedIdFixture Id { get; } ICacheStreamFixture LargeStringCache { get; } object Unprovided { get; } }
    public sealed class StoreDiagnosticMessageFixture : System.Runtime.Remoting.Messaging.IMethodCallMessage
    {
        public System.Reflection.MethodBase Method; public string Name, Failure; public int MethodReads, NameReads, ArgsReads;
        public System.Reflection.MethodBase MethodBase { get { MethodReads++; if (Failure == "method") throw new System.InvalidOperationException("OWNED_METHOD_READ"); return Method; } }
        public string MethodName { get { NameReads++; if (Failure == "name") throw new System.InvalidOperationException("OWNED_NAME_READ"); return Name; } }
        public int ArgCount { get { return 0; } } public int InArgCount { get { return 0; } }
        public object[] Args { get { ArgsReads++; if (Failure == "args") throw new System.InvalidOperationException("OWNED_ARGS_READ"); return new object[0]; } }
        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext { get { return null; } }
        public string Uri { get { return null; } set { throw new System.NotSupportedException(); } }
        public bool HasVarArgs { get { throw new System.NotSupportedException(); } } public object MethodSignature { get { throw new System.NotSupportedException(); } }
        public string TypeName { get { throw new System.NotSupportedException(); } } public System.Collections.IDictionary Properties { get { throw new System.NotSupportedException(); } }
        public object GetArg(int i) { throw new System.NotSupportedException(); } public string GetArgName(int i) { throw new System.NotSupportedException(); }
        public object[] InArgs { get { throw new System.NotSupportedException(); } } public object GetInArg(int i) { throw new System.NotSupportedException(); } public string GetInArgName(int i) { throw new System.NotSupportedException(); }
    }
}

namespace Owned.Task009
{
    public static class StoreDiagnosticTypedFixture
    {
        public static object ReadId(IStoreRequestFixture store) { return store.Id; }
        public static object ReadCache(IStoreRequestFixture store) { return store.LargeStringCache; }
        public static object ReadRefused(IStoreRequestFixture store) { return store.Unprovided; }
    }
}

// Source35 separate typed ABI declarations; no actual SDK namespace or store values.
namespace Owned.Task009
{
    public interface IContentStoreFixture : IDisposable { string Unknown { get; } }
    public interface IContentProjectStoreFixture { ScopedIdFixture Id { get; } ICacheStreamFixture LargeStringCache { get; } IContentStoreFixture Project { get; } }
    public sealed class ContentClassFixture { }
    public interface IContentClassStoreFixture { ContentClassFixture Project { get; } }
    public interface IContentGenericFixture<T> { }
    public interface IContentGenericStoreFixture { IContentGenericFixture<string> Project { get; } }
    public class ContentNestedFixture { public interface IStore { } }
    public interface IContentNestedStoreFixture { ContentNestedFixture.IStore Project { get; } }
    public static class ContentTypedFixture
    {
        public static object Id(IContentProjectStoreFixture store) { return store.Id; }
        public static object Cache(IContentProjectStoreFixture store) { return store.LargeStringCache; }
        public static object Reference(IContentProjectStoreFixture store) { return store.Project; }
        public static void Callback(object value) { GC.KeepAlive(((IContentStoreFixture)value).Unknown); }
        public static void ObjectType(object value) { GC.KeepAlive(value.GetType()); }
        public static void Dispose(object value) { ((IDisposable)value).Dispose(); }
    }
    public sealed class ContentContextCollectionFixture
    {
        public ContentContextCollectionFixture(IContextParentFixture parent, IFactoryFixture factory, Func<IContentProjectStoreFixture, ContentContextProjectFixture, IContextKeyFixture, Func<string,string>, Func<System.Collections.Generic.IEnumerable<ContextBaggageFixture>,bool>, Action<IContextImportInformationFixture>, Predicate<IContextImportInformationFixture>, ContextImporterFixture> importer)
        {
            if (parent == null || !(factory is UndoFactoryDeliveryFixture) || importer == null) throw new InvalidOperationException("OWNED_CONTENT_COLLECTION_ARGUMENTS");
            ContextOwnedControl.Parent = parent; ContextOwnedControl.Factory = factory; ContextOwnedControl.Collections++; ContextOwnedControl.Point("collection"); ContextOwnedControl.Collection = this;
        }
    }
    public sealed class ContentContextProjectFixture : IDisposable
    {
        public ContentContextProjectFixture(ContentContextCollectionFixture collection, IContentProjectStoreFixture store, IFactoryFixture factory, IContextArgumentFixture context)
        {
            if (collection == null || store == null || context == null || !Object.ReferenceEquals(collection, ContextOwnedControl.Collection) || !Object.ReferenceEquals(factory, ContextOwnedControl.Factory)) throw new InvalidOperationException("OWNED_CONTENT_PROJECT_ARGUMENTS");
            ContextOwnedControl.Context = context; ContextOwnedControl.Projects++; ContextOwnedControl.Point("project");
            ScopedIdFixture id = store.Id; IContentStoreFixture reference = store.Project;
            if (!Object.ReferenceEquals(id, store.Id) || reference == null || store.LargeStringCache == null) throw new InvalidOperationException("OWNED_CONTENT_TYPED_PROGRESS");
        }
        public void Dispose() { ContextOwnedControl.Disposals++; UndoOwnedControl.ProjectDisposals++; ContextOwnedControl.Point("project-dispose"); }
    }
}

// Source38: owned empty-content fixture; no real store/files or SDK namespace.
namespace Owned.Task009
{
    public interface IEmptyContentFixture : ICacheStreamFixture { }
    public interface IEmptyProjectStoreFixture { ScopedIdFixture Id { get; } ICacheStreamFixture LargeStringCache { get; } IEmptyContentFixture Project { get; } }
    public static class EmptyContentTypedFixture
    {
        public static object Reference(IEmptyProjectStoreFixture store) { return store.Project; }
        public static object Id(IEmptyProjectStoreFixture store) { return store.Id; }
        public static object Cache(IEmptyProjectStoreFixture store) { return store.LargeStringCache; }
        public static bool Exists(object value) { return ((ICacheStreamFixture)value).Exists; }
        public static void Other(object value) { GC.KeepAlive(((ICacheStreamFixture)value).Id); }
    }
}
