using Avalonia;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GameRandom.Providers;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.SteamSDK;

namespace GameRandom;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    
    [STAThread]
    public static void Main(string[] args)
    { 
        // LoadImpotentDependence().GetAwaiter().GetResult();
        
        GlobalExceptionHandler();
        
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logger.Error("App is closed with error: " + e);
            
            if (SteamManager.GetSteamManager().IsInitialized) 
                SteamManager.GetSteamManager().ShutdownSteam();
            
            ThrowMessageBox($"Fatal Error: {e.Message}");
        }
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    
    private static void GlobalExceptionHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                Logger.Error("UnhandledException: " + exception.Message);
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Logger.Error("UnobservedTaskException: " + args.Exception.Message);
            args.SetObserved();
        };
    }

    private static void ThrowMessageBox(string message)
    {
        var nameBox = OperatingSystem.IsWindows() ? "MessageBox.exe" : "MessageBox";

        var processingInfo = new ProcessStartInfo
        {
            FileName = nameBox,
            UseShellExecute = false,
        };
        
        processingInfo.ArgumentList.Add(message);
        
        var process = Process.Start(processingInfo);

        if (process is null)
            return;
        
        process.WaitForExit();
    }
}