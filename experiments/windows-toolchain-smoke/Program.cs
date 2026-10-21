using System;
using System.Runtime.Versioning;
using System.Threading;

[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]

namespace ToolchainSmoke
{
    internal static class Program
    {
        [STAThread]
        private static int Main()
        {
            try
            {
                if (!Environment.Is64BitProcess || Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    Console.Error.WriteLine("SMOKE_PROCESS_OR_APARTMENT");
                    return 2;
                }

                // Type use only: no WPF object, window, dispatcher or message loop.
                Type[] frameworkTypes =
                {
                    typeof(System.Windows.Threading.DispatcherObject),
                    typeof(System.Windows.UIElement),
                    typeof(System.Windows.FrameworkElement)
                };
                if (frameworkTypes.Length != 3)
                {
                    return 3;
                }

                Console.WriteLine("toolchain-smoke target=net48 process=x64 apartment=STA wpf-types=3 vendor=none protocol=none");
                return 0;
            }
            catch (Exception)
            {
                Console.Error.WriteLine("SMOKE_FRAMEWORK_RUNTIME");
                return 4;
            }
        }
    }
}
