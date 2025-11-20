using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Views;

public partial class GameTable : UserControl
{
    private ObservableConverter _converter;
    private Action<string> _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        DataContext = new GameTableViewModel();
        
        if (Design.IsDesignMode)
            return;
        
        //if(Di.Container.TryGetInstance<EventBus>() is EventBus eventBus) 
            //eventBus?.Subscribe<UpdateTableEvent>(e => UpdateTable(e.GameProgress));
        //else
       // {
           // Logger.Error("No EventBus found in game table");
       // }

        if (Di.Container.TryGetInstance<ObservableConverter>() is ObservableConverter converter)
        {
            _converter = converter;
        }

        Task.Run(async () => await InitializeTable());
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