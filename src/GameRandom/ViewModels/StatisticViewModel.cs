using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class StatisticViewModel : ViewModelBase, IDisposable
{
    [Inject] private DatabaseService? _dbService = null!;

    private ObservableCollection<StatisticCardInfo>? _statisticCards;

    public ObservableCollection<StatisticCardInfo>? StatisticCards
    {
        get => _statisticCards;
        set => SetProperty(ref _statisticCards, value);
    }

    public async Task LoadStatisticAsync()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_dbService == null)
            return;

        var list = await _dbService.Where<GameProgress>(e =>
            e.ClientId == SteamManager.GetSteamManager().GetSteamId().m_SteamID);

        if (list is null || list.Count == 0)
            return;

        int finishedGamesCount = list.Count(e => e.IsFinished);

        List<StatisticCardInfo> statisticCardInfos = new();

        statisticCardInfos.Add(new StatisticCardInfo("Games count", list.Count.ToString()));
        statisticCardInfos.Add(new StatisticCardInfo("Games finished", finishedGamesCount.ToString()));

        StatisticCards = new ObservableCollection<StatisticCardInfo>(statisticCardInfos);
    }

    public void Dispose()
    {
        _dbService = null;

        if (_statisticCards is not null)
        {
            _statisticCards.Clear();
            _statisticCards = null;
        }

        if (StatisticCards is null) return;

        StatisticCards.Clear();
        StatisticCards = null;
    }
}

public class StatisticCardInfo(string title, string data)
{
    public string Title { get; private set; } = title;
    public string Data { get; set; } = data;
}