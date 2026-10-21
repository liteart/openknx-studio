// Owned neutral WPF lifecycle experiment. No SDK, vendor loader, bridge or bus access.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

internal static class NeutralStaHost {
 static StreamWriter log;
 static int thread;
 static void Record(string code) { log.WriteLine(DateTime.UtcNow.ToString("O")+" "+code);log.Flush(); }
 static void OnUi() { if(Thread.CurrentThread.ManagedThreadId!=thread||Thread.CurrentThread.GetApartmentState()!=ApartmentState.STA)throw new InvalidOperationException("NEUTRAL_UI_THREAD"); }
 // Reject network/relative strings before any filesystem operation.
 static bool LocalLog(string path) {
  if(path.Length<3||!char.IsLetter(path[0])||path[1]!=':'||path[2]!='\\'||path.IndexOf('"')>=0||path.IndexOf('\r')>=0||path.IndexOf('\n')>=0)return false;
  string full=Path.GetFullPath(path);var drive=new DriveInfo(Path.GetPathRoot(full));
  if(drive.DriveType!=DriveType.Fixed&&drive.DriveType!=DriveType.Removable&&drive.DriveType!=DriveType.Ram)return false;
  string root=Path.GetPathRoot(full),directory=Path.GetDirectoryName(full),current=root;
  // Walk from the trusted local root: never touch a child through an unchecked junction.
  if((File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)return false;
  foreach(string part in directory.Substring(root.Length).Split('\\')){
   if(part.Length==0)continue;current=Path.Combine(current,part);var attributes=File.GetAttributes(current);
   if((attributes&FileAttributes.ReparsePoint)!=0||(attributes&FileAttributes.Directory)==0)return false;
  }
  try{File.GetAttributes(full);return false;}catch(FileNotFoundException){return true;}

 }
 [STAThread]
 static int Main(string[] args) {
  try {
   int seconds;
   if(args.Length!=4||args[0]!="--seconds"||!int.TryParse(args[1],out seconds)||seconds<1||seconds>60||args[2]!="--log"||!LocalLog(args[3]))return 2;
   using(log=new StreamWriter(new FileStream(args[3],FileMode.CreateNew,FileAccess.Write,FileShare.Read),new UTF8Encoding(false))){
    int session;using(var p=Process.GetCurrentProcess())session=p.SessionId;
    Record("NEUTRAL_START process="+(Environment.Is64BitProcess?"x64":"x86")+" apartment="+Thread.CurrentThread.GetApartmentState()+" session="+session+" interactive="+Environment.UserInteractive+" vendor=none hardware=NOT_RUN");
    if(Environment.OSVersion.Platform!=PlatformID.Win32NT||!Environment.Is64BitProcess){Record("NEUTRAL_PLATFORM_UNSUPPORTED");return 3;}
    if(!Environment.UserInteractive||session==0){Record("NEUTRAL_INTERACTIVE_SESSION_REQUIRED");return 4;}
    thread=Thread.CurrentThread.ManagedThreadId;OnUi();
    var app=new Application();app.ShutdownMode=ShutdownMode.OnMainWindowClose;
    bool loaded=false,rendered=false,closed=false,failed=false;
    app.DispatcherUnhandledException+=(sender,e)=>{Record("NEUTRAL_UI_ERROR "+e.Exception.GetType().FullName);failed=true;e.Handled=true;app.Shutdown(5);};
    var text=new TextBlock{Text="Owned STA lifecycle test. Vendor code is not loaded.",Margin=new Thickness(16),TextWrapping=TextWrapping.Wrap};
    var button=new Button{Content="Check UI thread",Margin=new Thickness(16),Padding=new Thickness(12)};
    button.Click+=(sender,e)=>{OnUi();text.Text="UI thread check passed. Vendor code is not loaded.";Record("NEUTRAL_BUTTON_UI_THREAD_PASS");};
    var panel=new StackPanel();panel.Children.Add(text);panel.Children.Add(button);
    var window=new Window{Title="KNX Task-144 neutral STA experiment",Width=560,Height=220,Content=panel,WindowStartupLocation=WindowStartupLocation.CenterScreen};
    var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(seconds)};
    timer.Tick+=(sender,e)=>{OnUi();Record("NEUTRAL_CLOSE_TIMER");timer.Stop();window.Close();};
    window.Loaded+=(sender,e)=>{OnUi();loaded=true;Record("NEUTRAL_LOADED_UI_THREAD_PASS");timer.Start();};
    window.ContentRendered+=(sender,e)=>{OnUi();rendered=true;Record("NEUTRAL_CONTENT_RENDERED");};
    window.Closed+=(sender,e)=>{OnUi();closed=true;timer.Stop();Record("NEUTRAL_CLOSED");};
    app.Exit+=(sender,e)=>{OnUi();Record("NEUTRAL_APPLICATION_EXIT code="+e.ApplicationExitCode);};
    int result=app.Run(window);
    if(failed||result!=0||!loaded||!rendered||!closed){Record("NEUTRAL_RESULT FAIL");return 5;}
    Record("NEUTRAL_RESULT PASS owned-ui-only vendor=NOT_RUN hardware=NOT_RUN");return 0;
   }
  }catch(Exception e){if(log!=null){try{Record("NEUTRAL_ERROR "+e.GetType().FullName);}catch(ObjectDisposedException){}}return 6;}
 }
}
