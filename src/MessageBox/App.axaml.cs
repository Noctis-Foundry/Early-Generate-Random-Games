using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MessageBox.ViewModels;
using MessageBox.Views;

namespace MessageBox;

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
            string message = "Message";
            string title = "Critical Error";
            
            if (desktop.Args.Length > 0)
            {
                message = desktop.Args[0];
                
                Console.WriteLine("Error: " + message);
            }
            
            var window = new MainWindow(){
                DataContext = new MainWindowViewModel(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen  
            };
            
            window.InitializeTextBlock(message, title);
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}