using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels.AdminSystem;

public class StatisticViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;
    [Inject] private PostgresListener? _postgresListener = null!;

    private Action<PayloadStructure>? _tableListener;
    
    public List<StatisticCardInfo> StatisticCardInfos { get; private set; } = new();

    public StatisticViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null)
            throw new NullReferenceException(nameof(_databaseService));
        if (_postgresListener is null)
            throw new NullReferenceException(nameof(_postgresListener));

        _tableListener += e =>
        {
            if (e.TableCode != (int)TableEnum.GameProgress)
                return;

            Dispatcher.UIThread.InvokeAsync(async () => await LoadStatisticAsync());
        };
        
        _postgresListener.Subscribe(TableEnum.GameProgress, _tableListener);
    }
    
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

public class StatisticCardInfo(string title, string data, int row, int column)
{
    public string Title { get; private set; } = title;
    public string Data { get; set; } = data;

    public int Row { get; private set; } = row;
    public int Column { get; private set; } = column;
}