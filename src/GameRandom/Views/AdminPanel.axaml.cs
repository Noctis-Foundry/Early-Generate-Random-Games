using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class AdminPanel : UserControl
{
    public AdminPanel()
    {
        InitializeComponent();
        var vm = new AdminPanelViewModel();
        DataContext = vm;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await vm.LoadGameProgresses();
        });
    }
}