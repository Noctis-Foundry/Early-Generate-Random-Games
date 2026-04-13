using Avalonia;
using System;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Factory;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem.Providers;
using GameRandom.Providers;
using GameRandom.Src.LobbySystem;
using GameRandom.Src.UserData;

namespace GameRandom;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    
    [STAThread]
    public static void Main(string[] args)
    { 
        LoadImpotentDependence().GetAwaiter().GetResult();
        
        GlobalExceptionHandler();
        
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            // System.Diagnostics.Process.Start("MessageBox.exe", new []
            // {
            //     e.Message,
            //     nameof(ErrorEnum.Critical)
            // });
            
            throw new Exception(e.Message, e);
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

    private static async Task LoadImpotentDependence()
    {
        var priorityStartup = new PriorityStartup();
        await priorityStartup.PriorityInitialization();
    }
}