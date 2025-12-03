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
    [Inject] private DatabaseService _databaseService;
    [Inject] private EventBus _eventBus;
    
    private ObservableConverter _converter;
    private Action<string> _onShowContent;
    private const int GameTableId = 2;

    public GameTable()
    {
        InitializeComponent();
        Di.Container.ResolveFieldsFromClassInstance(this);
        DataContext = new GameTableViewModel();
        
        if (Design.IsDesignMode)
            return;

        if (Di.Container.TryGetInstance<ObservableConverter>() is ObservableConverter converter)
        {
            _converter = converter;
        }

        Task.Run(async () => await InitializeTable());

        if (Di.Container.TryGetInstance<PostgresListener>() is PostgresListener listener)
        {
            listener.Subscribe(TableEnum.GameTable, 
                e => Dispatcher.UIThread.InvokeAsync(() => SubscribeToUpdateTable(e)));
        }
    }

    private async Task InitializeTable()
    {
        await using var db = new AppDbContext();
        var gameProgresses = await db.GameTables.ToListAsync();
        
        await Dispatcher.UIThread.InvokeAsync(() => UpdateTable(gameProgresses));
    }
    
    public void AddListener(Action<string> onChangeContent) => _onShowContent = onChangeContent;

    private void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }

    private async Task SubscribeToUpdateTable(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != GameTableId)
        {
            Logger.Debug($"TableCode: {payloadStructure.TableCode} not correct");
            return;
        }
        
        List<GameProgress>? gameProgresses = await _databaseService.GetTableListAsync<GameProgress>();

        if (gameProgresses == null || gameProgresses.Count == 0)
        {
            Logger.Debug($"No data found in GameTable.axaml.cs");
            return;
        }

        UpdateTable(gameProgresses);
    }
    
    private void UpdateTable(List<GameProgress> gameProgress)
    {
        foreach (var item in gameProgress)
        {
            Console.WriteLine($"item = {item.GameName}");
        }
        
        if (DataContext is GameTableViewModel viewModel)
        {
            viewModel.GameProgress = _converter.ToObservableCollection(gameProgress);
        }
        else
        {
            Console.WriteLine("DataContext is not GameTableViewModel");
        }
    }
}