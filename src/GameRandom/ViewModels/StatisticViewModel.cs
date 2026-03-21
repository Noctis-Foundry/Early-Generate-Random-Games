using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels.AdminSystem;

public class StatisticViewModel : ViewModelBase, IDisposable
{
    [Inject] private DatabaseService? _dbService = null!;

    public List<StatisticCardInfo> StatisticCardInfos { get; private set; } = new();

    public async Task LoadStatisticAsync()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_dbService == null)
            return;

        var list = await _dbService.Where<GameProgresses>(e =>
            e.PlayerId == SteamManager.GetSteamManager().GetSteamId().m_SteamID);

        if (list is null || list.Count == 0)
            return;

        int finishedGamesCount = list.Count(e => e.IsFinished);
        
        StatisticCardInfos.Add(new StatisticCardInfo("Games count", list.Count.ToString(), 0, 0));
        StatisticCardInfos.Add(new StatisticCardInfo("Finished games count", finishedGamesCount.ToString(), 0, 1));
    }

    public void Dispose()
    {
        _dbService = null;
    }
}

public class StatisticCardInfo(string title, string data, int row, int column)
{
    public string Title { get; private set; } = title;
    public string Data { get; set; } = data;

    public int Row { get; private set; } = row;
    public int Column { get; private set; } = column;
}