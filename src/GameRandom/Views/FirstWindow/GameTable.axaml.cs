using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class GameTable : UserControl, IDisposable
{
    [Inject] private ErrorService _errorService = null!;
    
    private Action<PayloadStructure>? _savedDelegate;
    
    private Action<string>? _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        DataContext = new GameTableViewModel();
        Di.Container.ResolveFieldsFromClassInstance(this);

        _savedDelegate = e => 
        {
            Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable(e.TableCode));
        };
        
        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Subscribe(TableEnum.GameProgress, _savedDelegate);
        }
        
        Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable((int)TableEnum.GameProgress));
    }

    public void Open()
    {
        UpdateTableData();
    }

    public void AddListener(Action<string> onChangeContent) => _onShowContent = onChangeContent;

    public void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
        Dispose();
    }

    private void SubscribeToUpdateTable(int tableCode)
    {
        if (tableCode != (int)TableEnum.GameProgress)
        {
            Logger.Info("Failed to update table, not correctly table code");
            return;
        }
        
        UpdateTableData();
    }

    private void UpdateTableData()
    {
        if (DataContext is GameTableViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await vm.LoadData());
        }
    }
    
    public void Dispose()
    {
        _onShowContent = null;

        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Unsubscribe(TableEnum.GameProgress, _savedDelegate);
        }

        _savedDelegate = null;
        
        _errorService = null!;
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}