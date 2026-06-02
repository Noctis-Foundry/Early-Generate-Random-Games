using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.Providers;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Views;
using GameRandom.Views.MainWindowSystem;
using MainWindow = GameRandom.Views.MainWindowSystem.MainWindow;

namespace GameRandom;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var desktopWindow = new MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            desktop.MainWindow = desktopWindow;
        }

        //PriorityInitialization
        base.OnFrameworkInitializationCompleted();

        if (Design.IsDesignMode)
            return;

        var windowProvider = new PriorityWindowProvider(desktopWindow);
        windowProvider.BindingInstance();

        Dispatcher.UIThread.Post(async () => { await Bootstrap(desktopWindow); });
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

    private async Task Bootstrap(IInitializeMainWindow desktopWindow)
    {
        await Dispatcher.UIThread.InvokeAsync(desktopWindow.SetLoadControl);

        var bootstrap = new AppBootstrap();
        await Task.Run(bootstrap.PriorityInitialization);

        await Dispatcher.UIThread.InvokeAsync(desktopWindow.InitializeUi);

        await Dispatcher.UIThread.InvokeAsync(desktopWindow.EndLoadingData);
    }
}