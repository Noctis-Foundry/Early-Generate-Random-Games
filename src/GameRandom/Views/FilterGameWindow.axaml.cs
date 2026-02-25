using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.SteamSDK;

namespace GameRandom.Views;

public partial class FilterGameWindow : WindowAbstract
{
    public FilterGameWindow()
    {
        InitializeComponent();
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        CloseWindow();
    }
}