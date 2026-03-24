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
    private Action? _savedHandler;
    
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeListener();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void InitializeViewModel()
    {
        var vm = new AdminRegistrationViewModel();
        
        _savedHandler += () => ProcessingWindowShow(vm);
        
        vm.StartProcessing += _savedHandler;
        
        DataContext = vm;
    }
    private void InitializeListener()
    {
        if (Di.Container.GetInstance<EventBus>() is not EventBus eventBus)
            throw new NullReferenceException(nameof(EventBus));
        
        eventBus.Subscribe<AdminRulesUpdating>(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(CheckAdminStatus);
        });
    }
    private void CheckAdminStatus()
    {
        if (!User.GetInstance().IsTopLevelAdmin())
        {
            if (DataContext is AdminRegistrationViewModel vm)
                vm.Dispose();
            
            Close();
        }
            
    }
    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminRegistrationViewModel vm)
        {
            if (vm.StartProcessing is not null && _savedHandler is not null) 
                vm.StartProcessing -= _savedHandler;
            
            vm.Dispose();
        }
        
        Close();
    }
}