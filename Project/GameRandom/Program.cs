using Avalonia;
using System;
using GameRandom.Service;
using GameRandom.SteamSDK;

namespace GameRandom;

sealed class Program
{
    private static SteamManager _steamManager;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            _steamManager = new SteamManager();
            _steamManager.InitSteam();
            
            var steamId = _steamManager.GetSteamID();
            Console.WriteLine("SteamID: " + steamId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}