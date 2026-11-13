// SPDX-License-Identifier: Apache-2.0
// Owned Windows job tests. No SDK, vendor input, network or device operation.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

internal static class ProcessContainmentProbe
{
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    static Process Start(string role,bool capture)
    {
        var value=new Process();value.StartInfo.FileName=typeof(ProcessContainmentProbe).Assembly.Location;
        value.StartInfo.Arguments=role;value.StartInfo.UseShellExecute=false;
        value.StartInfo.RedirectStandardOutput=capture;value.StartInfo.RedirectStandardError=capture;value.StartInfo.RedirectStandardInput=capture;
        try{Require(value.Start(),"START");return value;}catch{value.Dispose();throw;}
    }
    static string Line(Process value)
    {
        var line=value.StandardOutput.ReadLineAsync();Require(line.Wait(5000),"OUTPUT_TIMEOUT");
        string result=line.Result;Require(result!=null&&result.Length<=128,"OUTPUT_LINE");return result;
    }
    static void Stop(Process value){if(!value.HasExited)value.Kill();Require(value.WaitForExit(3000),"EXIT_UNCONFIRMED");}
    static void Normal(string role)
    {
        using(var root=Start(role,true))try{
            string line=Line(root);Require(root.WaitForExit(5000),"ROOT_TIMEOUT");
            Require(root.ExitCode==0&&line=="PASS "+role,"ROOT_FAILURE "+line);
            Require(root.StandardError.ReadToEnd().Length==0,"ROOT_STDERR");
        }finally{Stop(root);}
        Console.WriteLine("OWNED_JOB_CASE PASS "+role);
    }
    static void Crash()
    {
        using(var root=Start("root-crash",true))try{
            string line=Line(root);int pid;Require(line.StartsWith("CHILD ")&&Int32.TryParse(line.Substring(6),out pid),"CHILD_PID");
            pid=Int32.Parse(line.Substring(6));
            using(var child=Process.GetProcessById(pid)){
                // Open the actual child handle before requesting abrupt launcher termination.
                IntPtr held=child.Handle;Require(held!=IntPtr.Zero,"CHILD_HANDLE");
                root.StandardInput.WriteLine("KILL");root.StandardInput.Flush();
                Require(root.WaitForExit(5000),"CRASH_ROOT_TIMEOUT");
                Require(child.WaitForExit(3000),"CRASH_CHILD_SURVIVED");
            }
        }finally{Stop(root);}
        Console.WriteLine("OWNED_JOB_CASE PASS launcher-crash-kills-host");
    }
    public static int Main(string[] args)
    {
        try {
            Require(Environment.OSVersion.Platform==PlatformID.Win32NT&&Environment.Is64BitProcess,"WINDOWS_X64_REQUIRED");
            if(args.Length==0){Normal("root-descendant");Normal("root-timeout");Normal("root-repeat");Crash();Console.WriteLine("OWNED_JOB_SUITE PASS cases=4 vendor=NOT_RUN hardware=NOT_RUN");return 0;}
            Require(args.Length==1,"ARGUMENTS");string role=args[0];
            if(role=="forbidden")return 91;
            if(role=="hang"){Thread.Sleep(60000);return 92;}
            if(role=="attempt"){
                try{using(var escaped=Start("forbidden",false)){Stop(escaped);}return 93;}
                catch(Win32Exception){Console.WriteLine("DESCENDANT_DENIED");return 0;}
            }
            Require(role=="root-descendant"||role=="root-timeout"||role=="root-repeat"||role=="root-crash","ROLE");
            if(role=="root-descendant"){
                // The identical image/role must execute successfully before the new job is entered.
                // This prevents a missing image or unrelated startup failure from passing denial.
                using(var control=Start("forbidden",false))try{
                    Require(control.WaitForExit(3000)&&control.ExitCode==91,"DESCENDANT_CONTROL_FAILED");
                }finally{Stop(control);}
            }
            ProbeProcessContainment.Enter();
            if(role=="root-repeat"){
                bool denied=false;try{ProbeProcessContainment.Enter();}catch(InvalidOperationException){denied=true;}
                Require(denied&&ProbeProcessContainment.OnlyLauncher(2000),"REPEAT_NOT_DENIED");Console.WriteLine("PASS "+role);return 0;
            }
            using(var child=Start(role=="root-descendant"?"attempt":"hang",role=="root-descendant")){
                ProbeProcessContainment.VerifyChild(child);
                if(role=="root-crash"){
                    Console.WriteLine("CHILD "+child.Id);Console.Out.Flush();Require(Console.ReadLine()=="KILL","CRASH_HANDSHAKE");
                    Process.GetCurrentProcess().Kill();return 94;
                }
                try {
                    if(role=="root-descendant"){Require(Line(child)=="DESCENDANT_DENIED","DESCENDANT_STARTED");Require(child.WaitForExit(3000)&&child.ExitCode==0,"CHILD_FAILURE");}
                    else {Require(!child.WaitForExit(50),"HOST_DID_NOT_WAIT");Stop(child);}
                    Require(ProbeProcessContainment.OnlyLauncher(2000),"JOB_NOT_EMPTY");Console.WriteLine("PASS "+role);return 0;
                }finally{Stop(child);}
            }
        } catch(Exception error){Console.Error.WriteLine("OWNED_JOB_FAIL "+error.GetType().Name+" "+error.Message);return 1;}
    }
}
