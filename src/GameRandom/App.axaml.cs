using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.Views;

namespace GameRandom;

public partial class App : Application
{
    private SteamManager _steamManager;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (!Design.IsDesignMode)
        {
            Di.Container.ResolveFieldsFromClassInstance(this);
        
            InitializeSteam();
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            desktop.MainWindow = new MainWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            
            RegisterUiService(desktop.MainWindow);
        }
        
        if (!Design.IsDesignMode) 
            Di.Container.InjectDependenciesAcrossAssembly();
        
        base.OnFrameworkInitializationCompleted();
    }
    
    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void InitializeSteam()
    {
        try
        {
            _steamManager = new SteamManager();
            _steamManager.InitSteam();
            
            var steamId = _steamManager.GetSteamId();
            Console.WriteLine("SteamID: " + steamId);
        }
        catch (Exception e)
        {
            throw new Exception("Error initializing Steam: " + e.Message);
        }
        
        Di.Container.RegisterSingleInstance(new LobbyService());
    }
}