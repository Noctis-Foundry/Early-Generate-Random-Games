using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class AdminPanel : MainWindowUserControlAbstract
{
    [Inject] private ErrorService? _errorService = null!;
    private AdminRegistrationWindow _registrationWindow;
    private const string CloseTarget = "Main";
    
    public AdminPanel()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        DataContext = new AdminPanelViewModel();
        
        HideAdminPanel();

        Di.Container.ResolveField(out _errorService);

        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
        
        //TODO Prostgres listener
    }

    public override void Open()
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await vm.LoadGameProgresses();
            });
        }
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            vm.Dispose();
        }
        
        _changeWindowAction?.Invoke(CloseTarget);
    }

    private void HideAdminPanel()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService service)
            {
                using var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var admin = await service.GetFirstOrDefaultAsync<Admins>(
                    e => e.SteamId == User.GetInstance().GetUserId(), token.Token);

                if (admin is not null && admin.IsTopAdmin && DataContext is AdminPanelViewModel vm)
                    vm.IsCanShow = true;

            }
            else
                throw new NullReferenceException(nameof(DatabaseService));
        });
    }

    private async void ShowRegistration(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel { IsCanShow: false })
            return;
        
        try
        {
            _registrationWindow = new AdminRegistrationWindow();
            
            var window = TopLevel.GetTopLevel(this) as Window;

            if (window is null)
                return;

            await _registrationWindow.ShowDialog(window);
        }
        catch (Exception exception)
        {
            _errorService?.ShowWindow($"Failed to show registration window {exception.Message}");
        }
    }
}