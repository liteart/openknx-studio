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
            ProbeFiles.Require((args.Length == 8 || args.Length == 10) && args[0] == "--seconds" && Int32.TryParse(args[1], out seconds) && seconds >= 1 && seconds <= 30 && args[2] == "--mode" && args[4] == "--input" && args[6] == "--log", "LAUNCH_ARGUMENTS");
            // Definite assignment is explicit for the C#5 compiler after compound validation.
            seconds = Int32.Parse(args[1]); string mode = args[3];
            string[] resourceCases = { "id-constructor", "id-getter", "getter", "builder", "registration", "external", "build", "factory", "project", "validation", "logging", "error", "project-dispose", "container-dispose", "trace-dispose", "nested-window", "nested-budget", "nested-callback", "off-thread" };
            string[] undoCases = { "get", "new", "root", "invalid", "saturation", "trace-ctor", "builder", "registration", "external", "build", "factory", "project", "project-dispose", "container-dispose", "trace-dispose", "error", "final-audit", "nested", "resource", "register-null", "external-null" };
            string[] factoryCases = { "project-activation", "no-activation", "null", "wrong-type", "second", "saturation", "nested", "off-thread", "recorder", "resource", "get", "new", "root", "trace-ctor", "builder", "registration", "single", "external", "build", "factory", "project", "project-dispose", "container-dispose", "trace-dispose", "error", "final-audit", "late" };
            string[] cacheCases = { "second", "total-boundary", "total-limit", "stream-id", "stream-exists", "stream-read", "stream-multi", "stream-delete", "stream-time", "stream-mask", "object-type", "object-string", "object-hash", "object-equals", "dispose-message", "foreign", "invalid", "custom-method", "throw-method", "throw-args", "throw-context", "mutable", "mutable-id", "message-reentry", "forged-name", "forged-count", "forged-in", "forged-args", "recorder", "wrapper-restore", "stream-recorder", "stream-restore", "nested", "off-thread", "trace-ctor", "builder", "registration", "single", "external", "build", "factory", "validation", "logging", "error", "project-dispose", "container-dispose", "trace-dispose", "wrapper-final", "stream-final", "late" };
            string[] contextCases = { "collection-parent", "collection-importer", "collection-throw", "project-context", "parent-inherited", "parent-generic", "parent-void", "parent-object", "context-object", "parent-null", "context-null", "parent-custom", "parent-mutable", "parent-throw", "parent-saturation", "context-saturation", "importer-saturation", "importer-null", "nested-parent", "nested-context", "nested-importer", "off-thread-parent", "off-thread-context", "off-thread-importer", "early-parent", "early-context", "early-importer", "late-parent", "late-context", "late-importer", "parent-recorder", "context-recorder", "importer-recorder", "parent-restore", "context-restore", "importer-restore", "project-dispose-parent", "container-dispose-context", "trace-dispose-importer", "error-context", "parent-count-only", "context-count-only", "importer-count-only", "parent-fault-only", "context-fault-only", "importer-fault-only", "process-final-parent", "parent-getter-reentry", "dispose-begin-log-fault" };
            bool selectedExistsCase = args.Length == 10 && mode == "owned-project-content-store-exists-fixture" && args[8] == "--owned-exists-case" && Array.IndexOf(new string[] { "positive", "second", "next-denial", "wrong-receiver", "shape", "outside", "off-thread", "nested", "late", "malformed", "recorder", "restore", "budget", "audit" }, args[9]) >= 0;
            bool selectedContentDiagnosticCase = args.Length == 10 && mode == "owned-project-content-store-diagnostics" && args[8] == "--owned-content-diagnostic-case" && Array.IndexOf(new string[] { "stored", "object", "dispose", "first-only", "reentry", "recorder", "cap", "trust", "legacy" }, args[9]) >= 0;
            bool selectedContentCase = args.Length == 10 && mode == "owned-project-content-store-reference" && args[8] == "--owned-content-store-case" && Array.IndexOf(new string[] { "positive", "shape", "second", "outside", "off-thread", "nested", "late", "callback", "object", "dispose", "saturated", "malformed", "recorder", "restore", "audit" }, args[9]) >= 0;
            bool selectedDiagnosticCase = args.Length == 10 && mode == "owned-project-context-xml-registration" && args[8] == "--owned-store-diagnostic-case" && Array.IndexOf(new string[] { "positive", "declared", "forged", "custom", "foreign", "method-throw", "args", "recorder", "reentry", "saturated" }, args[9]) >= 0;
            bool selectedXmlCase = args.Length == 10 && mode == "owned-project-context-xml-registration" && args[8] == "--owned-xml-pin-case" && Array.IndexOf(new string[] { "missing", "size", "hash" }, args[9]) >= 0;
            bool selectedContextCase = args.Length == 10 && (mode == "owned-project-context-denied" || mode == "owned-project-context-xml-registration") && args[8] == "--owned-context-case" && Array.IndexOf(contextCases, args[9]) >= 0;
            bool selectedCacheCase = args.Length == 10 && mode == "owned-project-large-string-cache-denied" && args[8] == "--owned-cache-case" && Array.IndexOf(cacheCases, args[9]) >= 0;
            bool selectedFactoryCase = args.Length == 10 && mode == "owned-project-undo-provider-factory" && args[8] == "--owned-factory-case" && Array.IndexOf(factoryCases, args[9]) >= 0;
            bool selectedUndoCase = args.Length == 10 && mode == "owned-project-undo-provider-denied" && args[8] == "--owned-undo-case" && Array.IndexOf(undoCases, args[9]) >= 0;
            bool selectedResourceCase = args.Length == 10 && !selectedUndoCase && !selectedFactoryCase && !selectedCacheCase && !selectedContextCase && !selectedXmlCase && !selectedDiagnosticCase && !selectedContentCase && !selectedContentDiagnosticCase && !selectedExistsCase;
            ProbeFiles.Require(!selectedResourceCase || (mode == "owned-project-genuine-factory-resources" && args[8] == "--owned-resource-case"
                && Array.IndexOf(resourceCases, args[9]) >= 0), "OWNED_RESOURCE_CASE_ARGUMENTS");
            bool emptyContainer = mode == "original-project-content-store-exists-fixture" || mode == "owned-project-content-store-exists-fixture" || mode == "original-project-content-store-diagnostics" || mode == "owned-project-content-store-diagnostics" || mode == "original-project-content-store-reference" || mode == "owned-project-content-store-reference" || mode == "original-project-context-xml-registration" || mode == "owned-project-context-xml-registration" || mode == "original-project-context-denied" || mode == "owned-project-context-denied" || mode == "original-factory-empty-container" || mode == "owned-factory-empty-container" || mode == "original-trace-encryptor" || mode == "owned-trace-encryptor" || mode == "original-factory-registered-trace" || mode == "owned-factory-registered-trace" || mode == "original-project-genuine-factory" || mode == "owned-project-genuine-factory" || mode == "original-project-genuine-factory-resources" || mode == "owned-project-genuine-factory-resources" || mode == "original-project-undo-provider-denied" || mode == "owned-project-undo-provider-denied" || mode == "original-project-undo-provider-factory" || mode == "owned-project-undo-provider-factory" || mode == "original-project-large-string-cache-denied" || mode == "owned-project-large-string-cache-denied";
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
            if (selectedExistsCase) child.StartInfo.Arguments += " --owned-exists-case " + ProbeFiles.Quote(args[9]);
            if (selectedContentDiagnosticCase) child.StartInfo.Arguments += " --owned-content-diagnostic-case " + ProbeFiles.Quote(args[9]);
            if (selectedContentCase) child.StartInfo.Arguments += " --owned-content-store-case " + ProbeFiles.Quote(args[9]);
            if (selectedDiagnosticCase) child.StartInfo.Arguments += " --owned-store-diagnostic-case " + ProbeFiles.Quote(args[9]);
            if (selectedXmlCase) child.StartInfo.Arguments += " --owned-xml-pin-case " + ProbeFiles.Quote(args[9]);
            if (selectedContextCase) child.StartInfo.Arguments += " --owned-context-case " + ProbeFiles.Quote(args[9]);
            if (selectedCacheCase) child.StartInfo.Arguments += " --owned-cache-case " + ProbeFiles.Quote(args[9]);
            if (selectedFactoryCase) child.StartInfo.Arguments += " --owned-factory-case " + ProbeFiles.Quote(args[9]);
            if (selectedUndoCase) child.StartInfo.Arguments += " --owned-undo-case " + ProbeFiles.Quote(args[9]);
            if (selectedResourceCase) child.StartInfo.Arguments += " --owned-resource-case " + ProbeFiles.Quote(args[9]);
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
