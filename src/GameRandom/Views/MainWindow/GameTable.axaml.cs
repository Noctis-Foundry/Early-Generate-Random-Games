using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.Events;
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
        
        var eventBus = Di.Container.GetInstance<EventBus>() as EventBus;
        eventBus?.Subscribe<UpdateTableEvent>(e => UpdateTable(e.GameProgress));

        if (Di.Container.GetInstance<ObservableConverter>() is ObservableConverter converter)
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