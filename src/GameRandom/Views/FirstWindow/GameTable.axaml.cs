using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DISystem;
using GameRandom.DISystem.DiSystem;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.Enums;
using GameRandom.Scripts.HandleSystem.Interfaces;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.Scripts.HandleSystem.RoutSystem;
using GameRandom.Scripts.UserControls;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.ViewModels.MainWindowSystem.Enums;
using GameRandom.ViewModels.TablesSystem;

namespace GameRandom.Views;

/// <summary>
/// User control for displaying game progress table with real-time updates.
/// </summary>
public sealed partial class GameTable : MainWindowUserControlAbstract<GameTableViewModel>
{
    [Inject] private ErrorService _errorService = null!;
    [Inject] private IRouteManager _routeManager = null!;
    
    /// <summary>
    /// Delegate for handling table update notifications from PostgresListener.
    /// </summary>
    private Func<PayloadStructure, Task> _savedDelegate;

    /// <summary>
    /// Initializes the GameTable control and subscribes to database notifications.
    /// </summary>
    public GameTable()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        Di.ResolveInstance.ResolveInstanceFromClass(this);
        
        if (_routeManager == null || _errorService == null)
            throw new InvalidOperationException("Dependencies are not properly injected.");
        
        InitializeProcessingHandler();
        InitializePostgresListener();
        
        Dispatcher.UIThread.InvokeAsync(async () => await LoadTableData());
    }

    /// <summary>
    /// Closes the control, navigates to main view, and disposes resources.
    /// </summary>
    public override void CloseUserControl(object? sender, RoutedEventArgs e)
    {
        _changeWindowAction?.Invoke(ControlTypes.MainWindow);
        Dispose();
    }

    /// <summary>
    /// Handles table update notifications from database listener.
    /// </summary>
    private void InitializePostgresListener()
    {
        _savedDelegate = async (e) =>
        {
            if (e.TableCode != (int)TableEnum.GameProgress)
                return;
            
            await LoadTableData();
        };
        
        _routeManager.GetRouteService(TableEnum.GameProgress).Subscribe(RouteStage.View, _savedDelegate);
    }

    private async Task LoadTableData()
    {
        var viewModel = GetViewModel();
        
        if (viewModel is null)
            throw new ApplicationException("ViewModel is null");

        await viewModel.LoadData();
    }
    
    /// <summary>
    /// Disposes resources and unsubscribes from database notifications.
    /// </summary>
    public override void Dispose()
    {
        _changeWindowAction = null;

        if (Di.ResolveInstance.TryGetInstance<PostgresListener>() is { } listener)
        {
            _routeManager.GetRouteService(TableEnum.GameProgress).Unsubscribe(RouteStage.View, _savedDelegate);
        }
        
        _savedDelegate = null!;
        _errorService = null!;
        
        base.Dispose();
    }
}