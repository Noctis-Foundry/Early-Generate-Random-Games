using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DbContext;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts;
using GameRandom.Scripts.Database;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Scripts.UserControls;
using GameRandom.Scripts.UserData;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.ViewModels.AdminPanelSystem;
using GameRandom.ViewModels.MainWindowSystem.Enums;

namespace GameRandom.Views;

public sealed partial class AdminPanel : MainWindowUserControlAbstract<AdminPanelViewModel>
{
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private IRouteManager _routeManager = null!;
    private AdminRegistrationWindow _registrationWindow;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private Func<PayloadStructure, Task> _onAdminHideHandler;
    
    public AdminPanel()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;

        InitializeDiContainer();
        
        IsInitializeTaskWaiter = true;
        InitializeViewModel();
        
        HideAdminPanel();
        InitializePostgresListener();
        LoadUserControl();
    }

    protected override void LoadUserControl()
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await vm.LoadGameProgresses().WithCancellation(_cts.Token);
            });
        }
    }

    public override void CloseUserControl(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.MainWindow); //Call dispose for user control
    }

    private async Task HideAdminPanel()
    {
        await Dispatcher.UIThread.InvokeAsync(HidePanel);
    }
    private async Task HidePanel()
    {
        if (Di.ResolveInstance.TryGetInstance<DatabaseService>() is { } service)
        {
            using var token = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var admin = await service.GetFirstOrDefaultAsync<Admins>(
                e => e.SteamId == User.GetInstance().GetUserId(), token.Token);

            if (admin is not null && admin.IsTopAdmin && DataContext is AdminPanelViewModel vm)
                vm.IsCanShow = true;

        }
        else
            throw new NullReferenceException(nameof(DatabaseService));
    }
    private void InitializePostgresListener()
    {
        _onAdminHideHandler = async structure =>
        {
            if (structure.TableCode == (int)TableEnum.AdminTable)
                return;

            await HideAdminPanel();
        };
        
        _routeManager.GetRouteService(TableEnum.AdminTable).Subscribe(RouteStage.View, _onAdminHideHandler);
    }
    private void UnsubscribeListener()
    {
        _routeManager.GetRouteService(TableEnum.AdminTable).Subscribe(RouteStage.View, _onAdminHideHandler);
    }
    
    private void ShowRegistration(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel { IsCanShow: false })
            return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            _registrationWindow = new AdminRegistrationWindow();

            var window = TopLevel.GetTopLevel(this) as Window;

            if (window is null)
                return;

            await _registrationWindow.ShowDialog(window);
        });
    }
    protected sealed override void InitializeDiContainer()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
        if (_routeManager is null)
            throw new NullReferenceException(nameof(_routeManager));
    }

    public override void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        
        UnsubscribeListener();

        _changeWindowAction = null!;
        
        base.Dispose();
    }
}