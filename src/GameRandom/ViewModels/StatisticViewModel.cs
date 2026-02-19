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

namespace GameRandom.ViewModels;

public class StatisticViewModel : ViewModelBase, IDisposable
{
    [Inject] private DatabaseService? _dbService = null!;

    public async Task LoadStatisticAsync(Grid grid)
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_dbService == null)
            return;

        var list = await _dbService.Where<GameProgresses>(e =>
            e.PlayerID == SteamManager.GetSteamManager().GetSteamId().m_SteamID);

        if (list is null || list.Count == 0)
            return;

        grid.Children.Clear();

        int finishedGamesCount = list.Count(e => e.IsFinished);

        FactoryNewCard(grid, new StatisticCardInfo("Games count", list.Count.ToString(), 0, 0));
        FactoryNewCard(grid, new StatisticCardInfo("Finished games count", finishedGamesCount.ToString(), 0, 1));
    }

    private void FactoryNewCard(Grid grid, StatisticCardInfo cardInfo)
    {
        Border cardBorder = new Border();
        cardBorder.Classes.Add("StatisticPropertyBorder");

        Grid cardGrid = new Grid();
        cardGrid.RowDefinitions = new RowDefinitions("Auto, Auto, Auto");
        cardGrid.Classes.Add("CardBorderGrid");

        cardBorder.Child = cardGrid;

        TextBlock cardTitle = new TextBlock();
        cardTitle.Text = cardInfo.Title;
        cardTitle.Classes.Add("StatisticCardText");
        Grid.SetRow(cardTitle, 0);
        cardGrid.Children.Add(cardTitle);

        Separator separator = new Separator();
        Grid.SetRow(separator, 1);
        cardGrid.Children.Add(separator);

        TextBlock data = new TextBlock();
        data.Text = cardInfo.Data;
        data.Classes.Add("StatisticCardText");
        Grid.SetRow(data, 2);
        cardGrid.Children.Add(data);

        Grid.SetRow(cardBorder, cardInfo.Row);
        Grid.SetColumn(cardBorder, cardInfo.Column);
        grid.Children.Add(cardBorder);
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