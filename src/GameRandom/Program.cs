using Avalonia;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.LobbySystem;

namespace GameRandom;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    
    [STAThread]
    public static void Main(string[] args)
    { 
        InitializeDependenceInjection();
        
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

    private static void InitializeDependenceInjection()
    {
        Di.Container.RegisterSingleInstance(new DiFactory());
        Di.Container.RegisterSingleInstance(new EventBus());
        Di.Container.RegisterSingleInstance(new ObservableConverter());
        Di.Container.RegisterSingleInstance(new DatabaseService());
        Di.Container.RegisterSingleInstance(new MainWindowFactory());
        Di.Container.RegisterSingleInstance(new SteamWebApi());
        Di.Container.RegisterSingleInstance(new PostgresListener());
    }
}