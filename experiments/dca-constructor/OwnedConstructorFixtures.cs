// Owned fixtures only. Never use these identities as vendor/SDK services.
using System;
using System.Reflection;
using System.Threading;
[assembly: AssemblyVersion("1.0.0.0")]
namespace Owned.Task148
{
    public sealed class Success { public Success() { Console.WriteLine("OWNED_FIXTURE_SUCCESS"); } }
    public sealed class Throw { public Throw() { throw new InvalidOperationException("OWNED_FIXTURE_THROW"); } }
    public sealed class Missing { public Missing() { Assembly.Load("Owned.Task148.Absent, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"); } }
    public sealed class Hang { public Hang() { Console.WriteLine("OWNED_FIXTURE_HANG"); Thread.Sleep(Timeout.Infinite); } }
    public sealed class Flood { public Flood() { byte[] bytes = new byte[4096]; for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)'x'; var output = Console.OpenStandardOutput(); while (true) { output.Write(bytes, 0, bytes.Length); output.Flush(); } } }
}
