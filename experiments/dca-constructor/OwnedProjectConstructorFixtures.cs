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
        public static void Reset(string behavior) { Constructors = Builds = Disposals = Factories = 0; Behavior = behavior; DisposalProvider = null; }
    }
    public sealed class EmptyBuilderFixture
    {
        public EmptyBuilderFixture() { EmptyControlFixture.Constructors++; if (EmptyControlFixture.Behavior == "ctor-throw") throw new NotSupportedException("OWNED_BUILDER_THROW"); }
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
