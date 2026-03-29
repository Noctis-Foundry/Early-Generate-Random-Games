using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.CoreApp;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public sealed partial class FilterGameWindow : WindowBase<FilterGameViewModel>
{
    public FilterGameWindow()
    {
        InitializeComponent();
        InitializeViewModel();
    }
    
    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Dispose();
        Close();
    }

    public FilteredData? GetFilters()
    {
        if (DataContext is FilterGameViewModel viewModel)
            return viewModel.GetFilters();

        return null;
    }
}