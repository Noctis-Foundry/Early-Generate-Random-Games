using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.CoreApp;
using GameRandom.SteamSDK;
using GameRandom.ViewModels;

namespace GameRandom.Views;

public partial class FilterGameWindow : WindowAbstract
{
    public FilterGameWindow()
    {
        InitializeComponent();
        DataContext = new FilterGameViewModel();
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        CloseWindow();
    }

    public FilteredData? GetFilters()
    {
        if (DataContext is FilterGameViewModel viewModel)
            return viewModel.GetFilters();

        return null;
    }
}