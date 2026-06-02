using System;
using GameRandom.DependenceInjectSystem;
using System.Collections.Generic;
using GameRandom.DependenceInjectSystem;
using System.Collections.ObjectModel;
using GameRandom.DependenceInjectSystem;
using System.Linq;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using Avalonia.Controls;
using GameRandom.DependenceInjectSystem;
using Avalonia.Threading;
using GameRandom.DependenceInjectSystem;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scripts.HandleSystem;
using GameRandom.Scripts.HandleSystem.PostgresListener;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for the statistics screen.
/// </summary>
public class StatisticViewModel : ViewModelBase
{
    /// <summary>
    /// Database service for querying game data.
    /// </summary>
    [Inject] private DatabaseService? _databaseService = null!;
    
    /// <summary>
    /// PostgreSQL listener for real-time database updates.
    /// </summary>
    [Inject] private PostgresListener? _postgresListener = null!;

    /// <summary>
    /// Listener callback for table updates.
    /// </summary>
    private Action<PayloadStructure>? _tableListener;
    
    /// <summary>
    /// Collection of statistic cards to display.
    /// </summary>
    public List<StatisticCardInfo> StatisticCardInfos { get; private set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticViewModel"/> class.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if injected services are null.</exception>
    public StatisticViewModel()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (_databaseService is null)
            throw new NullReferenceException(nameof(_databaseService));
        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));

        InitializeListener();
    }

    /// <summary>
    /// Sets up the database listener for game progress updates.
    /// </summary>
    private void InitializeListener()
    {
        _tableListener += e =>
        {
            if (e.TableCode != (int)TableEnum.EndGameTable)
                return;

            Dispatcher.UIThread.InvokeAsync(async () => await LoadStatisticAsync());
        };
        
        _postgresListener.Subscribe(TableEnum.EndGameTable, _tableListener);
    }
    
    /// <summary>
    /// Loads statistics for the current Steam player.
    /// </summary>
    public async Task LoadStatisticAsync()
    {
        var list = await _databaseService.Where<GameProgresses>(e =>
            e.PlayerId == SteamManager.GetSteamManager().GetSteamId().m_SteamID);

        if (list is null || list.Count == 0)
            return;

        int finishedGamesCount = list.Count(e => e.IsFinished);
        
        StatisticCardInfos.Add(new StatisticCardInfo("Games count", list.Count.ToString(), 0, 0));
        StatisticCardInfos.Add(new StatisticCardInfo("Finished games count", finishedGamesCount.ToString(), 0, 1));
    }

    /// <summary>
    /// Disposes resources and unsubscribes from events.
    /// </summary>
    public override void Dispose()
    {
        _databaseService = null;
        
        if (_tableListener is not null) 
            _postgresListener?.Unsubscribe(TableEnum.GameProgress, _tableListener);
        
        _postgresListener = null;
        _tableListener = null;
        
        StatisticCardInfos.Clear();
    }
}

/// <summary>
/// Information model for a statistic card.
/// </summary>
/// <param name="title">Card title.</param>
/// <param name="data">Card data (value).</param>
/// <param name="row">Grid row index.</param>
/// <param name="column">Grid column index.</param>
public class StatisticCardInfo(string title, string data, int row, int column)
{
    /// <summary>
    /// Gets the card title.
    /// </summary>
    public string Title { get; private set; } = title;
    
    /// <summary>
    /// Gets or sets the card data (value).
    /// </summary>
    public string Data { get; set; } = data;

    /// <summary>
    /// Gets the grid row index.
    /// </summary>
    public int Row { get; private set; } = row;
    
    /// <summary>
    /// Gets the grid column index.
    /// </summary>
    public int Column { get; private set; } = column;
}