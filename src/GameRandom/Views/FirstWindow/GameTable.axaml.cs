using System;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using Avalonia.Interactivity;
using GameRandom.DependenceInjectSystem;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;

namespace GameRandom.Views;

/// <summary>
/// User control for displaying game progress table with real-time updates.
/// </summary>
public sealed partial class GameTable : MainWindowUserControlAbstract<GameTableViewModel>
{
    [Inject] private ErrorService _errorService = null!;
    
    /// <summary>
    /// Delegate for handling table update notifications from PostgresListener.
    /// </summary>
    private Action<PayloadStructure>? _savedDelegate;

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
        
        InitializeProcessingHandler();
        InitializePostgresListener();
        UpdateTableData();
    }

    /// <summary>
    /// Called when the control is opened. Refreshes table data.
    /// </summary>
    public override void LoadUserControl()
    {
        UpdateTableData();
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
    /// <param name="tableCode">Code identifying which table was updated.</param>
    private void InitializePostgresListener()
    {
        _savedDelegate = e => 
        {
            if (e.OpCode == (int)TableEnum.GameProgress)
            
            UpdateTableData();
        };
        
        if (Di.ResolveInstance.TryGetInstance<PostgresListener>() is { } listener)
        {
            listener.Subscribe(TableEnum.GameProgress, _savedDelegate);
        }
    }

    /// <summary>
    /// Refreshes table data by calling ViewModel's LoadData method on UI thread.
    /// </summary>
    private void UpdateTableData()
    {
        if (DataContext is GameTableViewModel vm)
            TaskRunner.RunWithDispatcherAsync(() => vm.LoadData());
    }
    
    /// <summary>
    /// Disposes resources and unsubscribes from database notifications.
    /// </summary>
    public override void Dispose()
    {
        _changeWindowAction = null;

        if (Di.ResolveInstance.TryGetInstance<PostgresListener>() is { } listener)
        {
            if (_savedDelegate is not null) 
                listener.Unsubscribe(TableEnum.GameProgress, _savedDelegate);
        }
        
        _savedDelegate = null;
        _errorService = null!;
        
        base.Dispose();
    }
}