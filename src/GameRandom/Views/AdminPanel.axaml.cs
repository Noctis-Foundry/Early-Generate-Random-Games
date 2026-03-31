using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public partial class AdminPanel : MainWindowUserControlAbstract
{
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private PostgresListener? _postgresListener;
    private AdminRegistrationWindow _registrationWindow;
    private const string CloseTarget = "Main";

    private CancellationTokenSource _cts = new CancellationTokenSource();

    private Action<PayloadStructure> _savedHandler;
    
    public AdminPanel()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;

        InitializeDiContainer();
        
        IsInitializeTaskWaiter = true;
        DataContext = new AdminPanelViewModel();
        
        HideAdminPanel();
        InitializePostgresListener();
    }

    public override void Open()
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            TaskRunner.RunWithDispatcherAsync(async () =>
            {
                await vm.LoadGameProgresses().WithCancellation(_cts.Token);
            });
        }
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            vm.Dispose();
        }
        
        UnsubscribeListener();
        
        _changeWindowAction?.Invoke(CloseTarget);
    }

    private void HideAdminPanel()
    {
        TaskRunner.RunWithDispatcherAsync(HidePanel);
    }
    private async Task HidePanel()
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
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));
    }
}