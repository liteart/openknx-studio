// Owned shapes only; these are not SDK implementations or usable services.
using System;

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
}
