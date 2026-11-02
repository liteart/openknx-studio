// Owned direct-child supervisor. This is not a process-tree or network sandbox.
using System;
using System.Diagnostics;
using System.Collections.Generic;
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
    static string ProductTempMarker(string root) { return Path.Combine(root,".openknx-import-owner"); }
    static void FinishProductTemp(string root,string token,string evidence)
    {
        ProbeFiles.Local(root,true,true);string marker=ProbeFiles.Local(ProductTempMarker(root),true,false);
        ProbeFiles.Require(new FileInfo(marker).Length==32&&File.ReadAllText(marker)==token,"LAUNCH_PRODUCT_TEMP_OWNERSHIP");
        var directories=new List<string>();var files=new List<string>();var pending=new Queue<string>();pending.Enqueue(root);
        var manifest=new StringBuilder();manifest.AppendLine("root="+root);long bytes=0;
        while(pending.Count!=0){
            string current=pending.Dequeue();ProbeFiles.Local(current,true,true);directories.Add(current);ProbeFiles.Require(directories.Count<=128,"LAUNCH_PRODUCT_TEMP_DIRECTORY_LIMIT");
            foreach(string item in Directory.EnumerateFileSystemEntries(current)){
                FileAttributes attributes=File.GetAttributes(item);ProbeFiles.Require((attributes&(FileAttributes.ReparsePoint|FileAttributes.Device))==0,"LAUNCH_PRODUCT_TEMP_REPARSE");
                if((attributes&FileAttributes.Directory)!=0){ProbeFiles.Require(pending.Count+directories.Count<128,"LAUNCH_PRODUCT_TEMP_DIRECTORY_LIMIT");pending.Enqueue(item);}
                else{
                    ProbeFiles.Local(item,true,false);long length=new FileInfo(item).Length;bytes+=length;ProbeFiles.Require(length<=16*1024*1024&&bytes<=64L*1024*1024&&files.Count<128,"LAUNCH_PRODUCT_TEMP_FILE_LIMIT");
                    files.Add(item);manifest.AppendLine("file bytes="+length+" sha256="+ProbeFiles.HashFile(item)+" relative-utf8-base64="+Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Substring(root.Length+1))));
                    ProbeFiles.Require(Encoding.UTF8.GetByteCount(manifest.ToString())<=60000,"LAUNCH_PRODUCT_TEMP_MANIFEST_LIMIT");
                }
            }
        }
        manifest.AppendLine("files="+files.Count+" directories="+directories.Count+" bytes="+bytes+" deletion=EXACT_OWNED_NO_REPARSE_ENTRIES");
        ProbeFiles.Local(evidence,false,false);using(var output=new FileStream(evidence,FileMode.CreateNew,FileAccess.Write,FileShare.None)){byte[] data=new UTF8Encoding(false).GetBytes(manifest.ToString());output.Write(data,0,data.Length);output.Flush(true);}
        ProbeFiles.Require(new FileInfo(marker).Length==32&&File.ReadAllText(ProbeFiles.Local(marker,true,false))==token,"LAUNCH_PRODUCT_TEMP_OWNERSHIP");
        foreach(string file in files)if(!ProbeFiles.Same(file,marker)){ProbeFiles.Local(file,true,false);File.Delete(file);}
        for(int i=directories.Count-1;i>0;i--){ProbeFiles.Local(directories[i],true,true);Directory.Delete(directories[i],false);}
        File.Delete(ProbeFiles.Local(marker,true,false));ProbeFiles.Local(root,true,true);Directory.Delete(root,false);
        Console.WriteLine("LAUNCH_PRODUCT_TEMP removed=True files="+files.Count+" directories="+directories.Count+" bytes="+bytes+" evidence=conversion-temp-evidence.txt descendants=NOT_CONTAINED");
    }
    internal static int Main(string[] args)
    {
        Process child = null; bool started = false, confirmed = false;
        string productTemp=null,productToken=null,productEvidence=null;
        try {
            bool stableIdentity=args.Length==10&&args[3]=="original-sdk-product-store-fixture"&&args[8]=="--owned-stable-identity"&&args[9]=="observe";
            bool parameterNegative=args.Length==10&&args[3]=="original-sdk-product-store-fixture"&&args[8]=="--owned-parameter-negative"&&args[9]=="missing-definition";
            bool selectionNegative=args.Length==10&&args[3]=="original-sdk-product-store-fixture"&&args[8]=="--owned-selection-negative"&&(args[9]=="actual-product"||args[9]=="missing-context");
            bool interactiveProductUi=args.Length==10&&args[3]=="original-sdk-product-store-fixture"&&args[8]=="--ui-seconds"&&args[9]=="600";
            int seconds;
            ProbeFiles.Require((args.Length == 8 || args.Length == 10) && args[0] == "--seconds" && Int32.TryParse(args[1], out seconds) && seconds >= 1 && seconds <= (interactiveProductUi ? 660 : args[3] == "original-sdk-product-store-fixture" ? 45 : 30) && args[2] == "--mode" && args[4] == "--input" && args[6] == "--log", "LAUNCH_ARGUMENTS");
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
            bool selectedResourceCase = args.Length == 10 && !selectedUndoCase && !selectedFactoryCase && !selectedCacheCase && !selectedContextCase && !selectedXmlCase && !selectedDiagnosticCase && !selectedContentCase && !selectedContentDiagnosticCase && !selectedExistsCase && !interactiveProductUi && !selectionNegative && !parameterNegative && !stableIdentity;
            ProbeFiles.Require(!selectedResourceCase || (mode == "owned-project-genuine-factory-resources" && args[8] == "--owned-resource-case"
                && Array.IndexOf(resourceCases, args[9]) >= 0), "OWNED_RESOURCE_CASE_ARGUMENTS");
            ProbeFiles.Require(!selectionNegative||seconds<=15,"SELECTION_NEGATIVE_DEADLINE");
            bool productImport = mode == "original-sdk-product-store-fixture";
            bool emptyContainer = productImport || mode == "original-sdk-device-fixture" || mode == "original-sdk-initialize-project" || mode == "original-sdk-project-fixture" || mode == "owned-sdk-project-fixture" || mode == "original-project-guid-fixture" || mode == "owned-project-guid-fixture" || mode == "original-project-content-store-exists-fixture" || mode == "owned-project-content-store-exists-fixture" || mode == "original-project-content-store-diagnostics" || mode == "owned-project-content-store-diagnostics" || mode == "original-project-content-store-reference" || mode == "owned-project-content-store-reference" || mode == "original-project-context-xml-registration" || mode == "owned-project-context-xml-registration" || mode == "original-project-context-denied" || mode == "owned-project-context-denied" || mode == "original-factory-empty-container" || mode == "owned-factory-empty-container" || mode == "original-trace-encryptor" || mode == "owned-trace-encryptor" || mode == "original-factory-registered-trace" || mode == "owned-factory-registered-trace" || mode == "original-project-genuine-factory" || mode == "owned-project-genuine-factory" || mode == "original-project-genuine-factory-resources" || mode == "owned-project-genuine-factory-resources" || mode == "original-project-undo-provider-denied" || mode == "owned-project-undo-provider-denied" || mode == "original-project-undo-provider-factory" || mode == "owned-project-undo-provider-factory" || mode == "original-project-large-string-cache-denied" || mode == "owned-project-large-string-cache-denied";
            bool factoryBinding = mode == "original-factory-bcl9" || mode == "owned-factory-bcl9";
            bool project = emptyContainer || factoryBinding || mode == "project-enum-metadata" || mode == "original-project" || mode == "owned-project" || mode == "original-project-store-id" || mode == "owned-project-store-id" || mode == "original-factory" || mode == "owned-factory";
            ProbeFiles.Require((mode != "original-project-store-id" && mode != "owned-project-store-id") || seconds <= 15, "PROJECT_FIXTURE_DEADLINE");
            ProbeFiles.Require((mode != "original-factory" && mode != "owned-factory") || seconds <= 15, "FACTORY_DIAGNOSTIC_DEADLINE");
            ProbeFiles.Require(!factoryBinding || seconds <= 15, "FACTORY_BINDING_DEADLINE");
            ProbeFiles.Require(!emptyContainer || seconds <= (interactiveProductUi ? 660 : productImport ? 45 : 15), "FACTORY_CONTAINER_DEADLINE");
            ProbeFiles.Require(project || mode == "neutral" || mode == "original" || mode == "original-initialize" || mode == "owned-initialize" || mode == "owned-Success" || mode == "owned-Throw" || mode == "owned-Missing" || mode == "owned-Hang" || mode == "owned-Flood", "LAUNCH_MODE");
            string input = ProbeFiles.Local(args[5], true, true), log = ProbeFiles.Local(args[7], false, false);
            string root = Path.GetDirectoryName(typeof(ProbeLauncher).Assembly.Location);
            string exe = ProbeFiles.Local(Path.Combine(root, emptyContainer ? "ProjectFactoryContainerProbe.exe" : factoryBinding ? "ProjectFactoryBindingProbe.exe" : mode == "neutral" ? "NeutralStaHost.exe" : project ? "ProjectConstructorProbe.exe" : "ConstructorProbe.exe"), true, false);
            ProbeFiles.Require(ProbeFiles.HashFile(exe) == ProbeFiles.OwnedHash(exe), "LAUNCH_CHILD_HASH");
            child = new Process(); child.StartInfo.FileName = exe;
            child.StartInfo.Arguments = mode == "neutral" ? "--seconds 1 --log " + ProbeFiles.Quote(log)
                : "--mode " + ProbeFiles.Quote(mode) + " --input " + ProbeFiles.Quote(input) + " --log " + ProbeFiles.Quote(log);
            ProbeFiles.Require(!interactiveProductUi||seconds==660,"PRODUCT_UI_EXPLICIT_DEADLINE");
            if(interactiveProductUi)child.StartInfo.Arguments+=" --ui-seconds 600";
            if(parameterNegative)child.StartInfo.Arguments+=" --owned-parameter-negative missing-definition";
            if(stableIdentity)child.StartInfo.Arguments+=" --owned-stable-identity observe";
            if(selectionNegative)child.StartInfo.Arguments+=" --owned-selection-negative "+ProbeFiles.Quote(args[9]);
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
            child.StartInfo.WorkingDirectory = productImport ? ProbeFiles.Local(Environment.CurrentDirectory,true,true) : Path.GetDirectoryName(log);
            if(productImport&&!selectionNegative){
                string userTemp=ProbeFiles.Local(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Temp"),true,true);
                productToken=Guid.NewGuid().ToString("N");productTemp=ProbeFiles.Local(Path.Combine(userTemp,"kc-"+productToken.Substring(0,12)),false,true);
                productEvidence=ProbeFiles.Local(Path.Combine(child.StartInfo.WorkingDirectory,"conversion-temp-evidence.txt"),false,false);
                Directory.CreateDirectory(productTemp);ProbeFiles.Local(productTemp,true,true);
                string marker=ProbeFiles.Local(ProductTempMarker(productTemp),false,false);using(var file=new FileStream(marker,FileMode.CreateNew,FileAccess.Write,FileShare.None)){byte[] data=Encoding.ASCII.GetBytes(productToken);file.Write(data,0,data.Length);file.Flush(true);}
                child.StartInfo.EnvironmentVariables["OPENKNX_IMPORT_TEMP"]=productTemp;child.StartInfo.EnvironmentVariables["OPENKNX_IMPORT_TEMP_TOKEN"]=productToken;
                Console.WriteLine("LAUNCH_PRODUCT_TEMP created=True root="+productTemp+" owner-marker=EXACT_PER_RUN cleanup=AFTER_CONFIRMED_CHILD_EXIT");
            }
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
            if(productTemp!=null){string owned=productTemp;productTemp=null;FinishProductTemp(owned,productToken,productEvidence);}
            if (reason == "TIMEOUT") return 124;
            if (reason != "EXIT") return 125;
            return child.ExitCode;
        } catch (Exception error) {
            Console.WriteLine("LAUNCH_FAILURE " + error.GetType().FullName + " " + error.Message.Replace("\r", " ").Replace("\n", " "));
            return 127;
        } finally {
            if (started && !confirmed) { try { if (!child.HasExited) child.Kill(); confirmed = child.WaitForExit(2000); Console.WriteLine("LAUNCH_CLEANUP child-exit-confirmed=" + confirmed); } catch (Exception error) { Console.WriteLine("LAUNCH_CLEANUP_UNCONFIRMED " + error.GetType().FullName); } }
            if(productTemp!=null){
                if(!started||confirmed){try{FinishProductTemp(productTemp,productToken,productEvidence);}catch(Exception error){Console.WriteLine("LAUNCH_PRODUCT_TEMP_CLEANUP_FAILED root="+productTemp+" type="+error.GetType().FullName+" action=PRESERVED_FOR_INSPECTION");}}
                else Console.WriteLine("LAUNCH_PRODUCT_TEMP_CLEANUP_NOT_RUN root="+productTemp+" reason=CHILD_EXIT_UNCONFIRMED");
            }
            if (child != null) child.Dispose();
        }
    }
}
