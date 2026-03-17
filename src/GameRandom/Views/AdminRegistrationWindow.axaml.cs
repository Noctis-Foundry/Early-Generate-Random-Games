using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.SteamSDK;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views;

public partial class AdminRegistrationWindow : WindowAbstract
{
    public AdminRegistrationWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        DataContext = new AdminRegistrationViewModel();
    }

    private void CloseAsyncWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}