using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class StatisticControl : UserControl
{
    public StatisticControl()
    {
        InitializeComponent();
        DataContext = new StatisticViewModel();
    }

    public void Open()
    {
        if (DataContext is StatisticViewModel statisticViewModel)
            Dispatcher.UIThread.InvokeAsync(async () => await statisticViewModel.LoadStatisticAsync());
    }

    public void Close()
    {
        if (DataContext is StatisticViewModel statisticViewModel)
            statisticViewModel.Dispose();
    }
}