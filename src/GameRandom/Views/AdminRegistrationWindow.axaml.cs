using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public sealed partial class AdminRegistrationWindow : WindowBase<AdminRegistrationViewModel>
{
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeProcessingHandler();
        InitializeEventBusListener<AdminRulesUpdating>(CheckAdminStatus);

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
    
    private void CheckAdminStatus()
    {
        if (!User.GetInstance().IsTopLevelAdmin())
        {
            Dispose();
            Close();
        }
    }

    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        Dispose();
        
        Close();
    }
}