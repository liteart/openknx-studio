// Owned direct-child supervisor. This is not a process-tree or network sandbox.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

internal static class ProbeLauncher
{
    static readonly object CaptureLock = new object();
    static readonly MemoryStream Captured = new MemoryStream(65536);
    static int total, overflow, readFailure;
    static void Read(Stream input)
    {
        try {
            byte[] chunk = new byte[4096]; int count;
            while ((count = input.Read(chunk, 0, chunk.Length)) != 0) {
                lock (CaptureLock) {
                    if (count > 65536 - total) { Interlocked.Exchange(ref overflow, 1); return; }
                    Captured.Write(chunk, 0, count); total += count;
                }
            }
        } catch (IOException) { Interlocked.Exchange(ref readFailure, 1); }
        catch (ObjectDisposedException) { Interlocked.Exchange(ref readFailure, 1); }
    }
    internal static int Main(string[] args)
    {
        Process child = null; bool started = false, confirmed = false;
        try {
            int seconds;
            ProbeFiles.Require(args.Length == 8 && args[0] == "--seconds" && Int32.TryParse(args[1], out seconds) && seconds >= 1 && seconds <= 30 && args[2] == "--mode" && args[4] == "--input" && args[6] == "--log", "LAUNCH_ARGUMENTS");
            // Definite assignment is explicit for the C#5 compiler after compound validation.
            seconds = Int32.Parse(args[1]); string mode = args[3];
            bool emptyContainer = mode == "original-factory-empty-container" || mode == "owned-factory-empty-container" || mode == "original-trace-encryptor" || mode == "owned-trace-encryptor";
            bool factoryBinding = mode == "original-factory-bcl9" || mode == "owned-factory-bcl9";
            bool project = emptyContainer || factoryBinding || mode == "project-enum-metadata" || mode == "original-project" || mode == "owned-project" || mode == "original-project-store-id" || mode == "owned-project-store-id" || mode == "original-factory" || mode == "owned-factory";
            ProbeFiles.Require((mode != "original-project-store-id" && mode != "owned-project-store-id") || seconds <= 15, "PROJECT_FIXTURE_DEADLINE");
            ProbeFiles.Require((mode != "original-factory" && mode != "owned-factory") || seconds <= 15, "FACTORY_DIAGNOSTIC_DEADLINE");
            ProbeFiles.Require(!factoryBinding || seconds <= 15, "FACTORY_BINDING_DEADLINE");
            ProbeFiles.Require(!emptyContainer || seconds <= 15, "FACTORY_CONTAINER_DEADLINE");
            ProbeFiles.Require(project || mode == "neutral" || mode == "original" || mode == "original-initialize" || mode == "owned-initialize" || mode == "owned-Success" || mode == "owned-Throw" || mode == "owned-Missing" || mode == "owned-Hang" || mode == "owned-Flood", "LAUNCH_MODE");
            string input = ProbeFiles.Local(args[5], true, true), log = ProbeFiles.Local(args[7], false, false);
            string root = Path.GetDirectoryName(typeof(ProbeLauncher).Assembly.Location);
            string exe = ProbeFiles.Local(Path.Combine(root, emptyContainer ? "ProjectFactoryContainerProbe.exe" : factoryBinding ? "ProjectFactoryBindingProbe.exe" : mode == "neutral" ? "NeutralStaHost.exe" : project ? "ProjectConstructorProbe.exe" : "ConstructorProbe.exe"), true, false);
            ProbeFiles.Require(ProbeFiles.HashFile(exe) == ProbeFiles.OwnedHash(exe), "LAUNCH_CHILD_HASH");
            child = new Process(); child.StartInfo.FileName = exe;
            child.StartInfo.Arguments = mode == "neutral" ? "--seconds 1 --log " + ProbeFiles.Quote(log)
                : "--mode " + ProbeFiles.Quote(mode) + " --input " + ProbeFiles.Quote(input) + " --log " + ProbeFiles.Quote(log);
            child.StartInfo.WorkingDirectory = Path.GetDirectoryName(log);
            child.StartInfo.UseShellExecute = false; child.StartInfo.CreateNoWindow = false;
            child.StartInfo.RedirectStandardOutput = true; child.StartInfo.RedirectStandardError = true;
            // Prevent caller-supplied managed startup instrumentation in the owned child.
            foreach (string name in new string[] { "COR_ENABLE_PROFILING", "COR_PROFILER", "COR_PROFILER_PATH", "COR_PROFILER_PATH_64", "APPDOMAIN_MANAGER_ASM", "APPDOMAIN_MANAGER_TYPE", "DEVPATH", "COMPLUS_Version", "DOTNET_STARTUP_HOOKS" }) child.StartInfo.EnvironmentVariables.Remove(name);
            ProbeFiles.Require(child.Start(), "LAUNCH_START"); started = true;
            Console.WriteLine("LAUNCH_START pid=" + child.Id + " mode=" + mode + " deadline-seconds=" + seconds + " capture-bytes=65536 descendants=NOT_CONTAINED");
            var stdout = new Thread(delegate() { Read(child.StandardOutput.BaseStream); }); stdout.IsBackground = true;
            var stderr = new Thread(delegate() { Read(child.StandardError.BaseStream); }); stderr.IsBackground = true;
            stdout.Start(); stderr.Start();
            Stopwatch clock = Stopwatch.StartNew(); string reason = "EXIT";
            while (!child.WaitForExit(25)) {
                if (Volatile.Read(ref overflow) != 0) { reason = "OUTPUT_LIMIT"; break; }
                if (Volatile.Read(ref readFailure) != 0) { reason = "CAPTURE_FAILURE"; break; }
                if (clock.ElapsedMilliseconds >= seconds * 1000L) { reason = "TIMEOUT"; break; }
            }
            if (!child.HasExited) child.Kill();
            confirmed = child.WaitForExit(2000);
            if (!confirmed) { Console.WriteLine("LAUNCH_RESULT child-exit-confirmed=false"); return 126; }
            bool readersDone = stdout.Join(1000) & stderr.Join(1000);
            // An inherited pipe in a descendant must never make the supervisor hang.
            if (!readersDone) { child.StandardOutput.Close(); child.StandardError.Close(); reason = "CAPTURE_INCOMPLETE"; }
            else if (Volatile.Read(ref overflow) != 0) reason = "OUTPUT_LIMIT";
            else if (Volatile.Read(ref readFailure) != 0) reason = "CAPTURE_FAILURE";
            if (reason == "EXIT") {
                lock (CaptureLock) { Console.Write(Encoding.UTF8.GetString(Captured.ToArray())); }
            }
            Console.WriteLine("LAUNCH_RESULT reason=" + reason + " child-exit=" + child.ExitCode + " child-exit-confirmed=true captured-bytes=" + total + " descendants=NOT_CONTAINED");
            if (reason == "TIMEOUT") return 124;
            if (reason != "EXIT") return 125;
            return child.ExitCode;
        } catch (Exception error) {
            Console.WriteLine("LAUNCH_FAILURE " + error.GetType().FullName + " " + error.Message.Replace("\r", " ").Replace("\n", " "));
            return 127;
        } finally {
            if (started && !confirmed) { try { if (!child.HasExited) child.Kill(); confirmed = child.WaitForExit(2000); Console.WriteLine("LAUNCH_CLEANUP child-exit-confirmed=" + confirmed); } catch (Exception error) { Console.WriteLine("LAUNCH_CLEANUP_UNCONFIRMED " + error.GetType().FullName); } }
            if (child != null) child.Dispose();
        }
    }
}
