using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views;

public partial class GameTable : UserControl
{
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly ObservableConverter _converter = null!;
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
            Logger.Error($"An error occured while loading GameTable + {e.Message}");
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
            Logger.Debug($"TableCode: {tableCode} not correct");
            return;
        }

        try
        {
            var gameProgresses = await _databaseService.GetTableListAsync<GameProgress>();
            UpdateTable(gameProgresses);
        }
        catch (Exception ex)
        {
            Logger.Error(ex + " Failed to load GameTable");
        }
    }
    
    private void UpdateTable(List<GameProgress> gameProgress)
    {
        if (DataContext is GameTableViewModel viewModel)
            viewModel.GameProgress = _converter.ToObservableCollection(gameProgress);
    }
}