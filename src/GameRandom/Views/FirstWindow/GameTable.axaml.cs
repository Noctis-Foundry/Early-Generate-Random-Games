using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserSystem;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views;

public partial class GameTable : UserControl, IAddListener, IDisposable
{
    [Inject] private DatabaseService _databaseService = null!;
    [Inject] private ObservableConverter _converter = null!;
    [Inject] private ErrorService _errorService = null!;
    [Inject] private UserData _userData = null!;
    
    private readonly CancellationTokenSource _cts = new();
    
    private Action<PayloadStructure>? _savedDelegate;
    
    private Action<string>? _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        DataContext = new GameTableViewModel();
        
        if (Design.IsDesignMode)
            return;

        Task.Run(async () => await InitializeTable(), _cts.Token);

        _savedDelegate = e => 
        {
            Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable(e.TableCode));
        };
        
        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Subscribe(TableEnum.GameTable, _savedDelegate);
        }
        
        Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable((int)TableEnum.GameTable));
    }

    private async Task InitializeTable()
    {
        try
        {
            var gameProgresses = await _databaseService.GetTableListAsync<GameProgress>();
            await Dispatcher.UIThread.InvokeAsync(() => UpdateTable(gameProgresses));
        }
        catch (Exception e)
        {
            _errorService.ShowErrorWindow($"An error occured while loading GameTable + {e.Message}", ErrorEnum.Error);
        }
    }
    
    public void AddListener(Action<string> onChangeContent) => _onShowContent = onChangeContent;

    private void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
        // Dispose();
    }

    private async Task SubscribeToUpdateTable(int tableCode)
    {
        if (tableCode != (int)TableEnum.GameTable)
        {
            _errorService.ShowErrorWindow($"TableCode: {tableCode} not correct", ErrorEnum.Error);
            return;
        }

        var finalyTable = new List<GameProgress>();
        
        try
        {
            var lobbyContexts = await _databaseService.Where<LobbyUserContext>(e => e.LobbyID == _userData.LobbyId);
            var gameProgresses = await _databaseService.GetTableListAsync<GameProgress>();
            
            if (lobbyContexts == null || lobbyContexts.Count <= 0)
            {
                _errorService.ShowErrorWindow($"Not founded lobby members with lobby id {_userData.LobbyId}", ErrorEnum.Error);
                return;
            }

            if (gameProgresses == null || gameProgresses.Count <= 0)
            {
                _errorService.ShowErrorWindow($"Not founded started game with lobby id {_userData.LobbyId}", ErrorEnum.Error);
                return;
            }
            
            foreach (var lobbyContext in lobbyContexts)
            {
                finalyTable.AddRange(gameProgresses.Where(e => e.ClientId == lobbyContext.MemberID));
            }
            
            UpdateTable(finalyTable);
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorWindow(ex + " Failed to load GameTable", ErrorEnum.Error);
        }
    }
    
    private void UpdateTable(List<GameProgress> gameProgress)
    {
        if (DataContext is GameTableViewModel viewModel)
            viewModel.GameProgress = _converter.ToObservableCollection(gameProgress);
    }

    public void Dispose()
    {
        _onShowContent = null;

        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Unsubscribe(TableEnum.GameTable, _savedDelegate);
        }

        _savedDelegate = null;
        
        _databaseService = null!;
        _errorService = null!;
        _converter = null!;
        _userData = null!;
        
        _cts.Cancel();
        _cts.Dispose();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}