using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public partial class StatisticControl : UserControl, IDisposable
{
    private GamesTableWindow? _gameTableWindow;

    public StatisticControl()
    {
        InitializeComponent();
        DataContext = new StatisticViewModel();

        if (Design.IsDesignMode)
            return;
    }

    public void Open()
    {
        if (DataContext is not StatisticViewModel statisticViewModel) return;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await statisticViewModel.LoadStatisticAsync();

            StatisticCardGrid.Children.Clear();
            
            foreach (var cardInfo in statisticViewModel.StatisticCardInfos)
            {
                FactoryNewCard(cardInfo);
            }
        });
    }

    private void FactoryNewCard(StatisticCardInfo cardInfo)
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
        StatisticCardGrid.Children.Add(cardBorder);
    }

    private void OpenTable(object? sender, RoutedEventArgs e)
    {
        _gameTableWindow = new GamesTableWindow();

        if (_gameTableWindow is null)
        {
            Console.WriteLine("Statistic Control: error for open table window. Element is null");
            return;
        }

        _gameTableWindow.Show();
    }

    public void Dispose()
    {
        if (DataContext is StatisticViewModel vm)
            vm.Dispose();
        
        _gameTableWindow?.Close();
    }
}