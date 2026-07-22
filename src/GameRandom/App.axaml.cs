using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.Bootstrap;
using GameRandom.Providers;
using GameRandom.ViewModels.MainWindowSystem.Interface;
using GameRandom.Views;
using GameRandom.Views.MainWindowSystem;
using MainWindow = GameRandom.Views.MainWindowSystem.MainWindow;

namespace GameRandom;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var desktopWindow = new MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
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

    private async Task Bootstrap(IInitializeMainWindow desktopWindow)
    {
        await Dispatcher.UIThread.InvokeAsync(desktopWindow.SetLoadControl);

        var bootstrap = new AppBootstrap();
        await Task.Run(bootstrap.PriorityInitialization);

        await Dispatcher.UIThread.InvokeAsync(desktopWindow.InitializeUi);

        await Dispatcher.UIThread.InvokeAsync(desktopWindow.EndLoadingData);
    }
}