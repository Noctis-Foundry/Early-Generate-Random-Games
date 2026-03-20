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
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class AdminRegistrationWindow : WindowAbstract
{
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        DataContext = new AdminRegistrationViewModel();
        
        if (Di.Container.GetInstance<EventBus>() is not EventBus eventBus)
            throw new NullReferenceException(nameof(EventBus));
        
        eventBus.Subscribe<AdminRulesUpdating>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(CheckAdminStatus);
        });
    }

    private void CheckAdminStatus()
    {
        if (!User.GetInstance().IsAdmin)
        {
            if (DataContext is AdminRegistrationViewModel vm)
                vm.Dispose();
            
            Close();
        }
            
    }
    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminRegistrationViewModel vm)
            vm.Dispose();
        
        Close();
    }
}