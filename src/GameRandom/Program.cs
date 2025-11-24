using Avalonia;
using System;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
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
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
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
    }
}