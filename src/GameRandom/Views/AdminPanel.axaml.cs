using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class AdminPanel : MainWindowUserControlAbstract
{
    private const string CloseTarget = "Main";
    
    public AdminPanel()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        DataContext = new AdminPanelViewModel();
    }

    public override void Open()
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await vm.LoadGameProgresses();
            });
        }
    }

    public override void Close(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AdminPanelViewModel vm)
        {
            vm.Dispose();
        }
        
        _changeWindowAction?.Invoke(CloseTarget);
    }
}