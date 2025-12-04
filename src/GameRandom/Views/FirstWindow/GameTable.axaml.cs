using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

public partial class GameTable : UserControl
{
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly ObservableConverter _converter = null!;
    [Inject] private readonly ErrorService _errorService = null!;
    [Inject] private readonly UserData _userData = null!;
    private Action<string>? _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        DataContext = new GameTableViewModel();
        
        if (Design.IsDesignMode)
            return;

        Task.Run(async () => await InitializeTable());

        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Subscribe(TableEnum.GameTable, 
                e => Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable(e.TableCode)));
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
}