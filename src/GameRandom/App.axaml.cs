using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GameRandom.Scr.DI;
using GameRandom.SteamSDK;
using GameRandom.Views;

namespace GameRandom;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow()
            {
            };
            
            RegisterUiService(desktop.MainWindow);
        }
        
        base.OnFrameworkInitializationCompleted();
    }
    
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void RegisterUiService(Window window)
    {
        var factory = Di.Container.GetInstance<DiFactory>() as DiFactory;
        
        if (factory == null)
            throw new Exception("DiFactory not found");

        if (window is MainWindow mainWindow)
            factory.Create<IError, ErrorService, MainWindow>(new ErrorService(), mainWindow);
        else
            throw new Exception("Window not found");
    }
}