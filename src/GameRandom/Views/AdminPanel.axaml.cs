using System;
using GameRandom.DependenceInjectSystem;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using Avalonia;
using GameRandom.DependenceInjectSystem;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using Avalonia.Interactivity;
using GameRandom.DependenceInjectSystem;
using Avalonia.Markup.Xaml;
using GameRandom.DependenceInjectSystem;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.UserData;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Views;

public sealed partial class AdminPanel : MainWindowUserControlAbstract<AdminPanelViewModel>
{
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private PostgresListener? _postgresListener;
    private AdminRegistrationWindow _registrationWindow;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private Action<PayloadStructure> _savedHandler;
    
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
    }

    protected override void LoadUserControl()
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            TaskRunner.RunWithDispatcherAsync(async () =>
            {
                await vm.LoadGameProgresses().WithCancellation(_cts.Token);
            });
        }
    }

    public override void CloseUserControl(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.MainWindow); //Call dispose for user control
    }

    private void HideAdminPanel()
    {
        TaskRunner.RunWithDispatcherAsync(HidePanel);
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
        _savedHandler = structure =>
        {
            if (structure.TableCode == (int)TableEnum.AdminTable)
                return;

            HideAdminPanel();
        };
        
        _postgresListener?.Subscribe(TableEnum.AdminTable, _savedHandler);
    }
    private void UnsubscribeListener()
    {
        _postgresListener?.Unsubscribe(TableEnum.AdminTable, _savedHandler);
    }
    private void ShowRegistration(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel { IsCanShow: false })
            return;

        TaskRunner.RunWithDispatcherAsync(async () =>
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
        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));
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