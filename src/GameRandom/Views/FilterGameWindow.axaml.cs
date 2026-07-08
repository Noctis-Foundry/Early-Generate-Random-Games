using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GameRandom.Scripts;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.FilterGameSystem;

namespace GameRandom.Views;

public sealed partial class FilterGameWindow : WindowBase<FilterGameViewModel>
{
    public FilterGameWindow()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
    }
    
    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;
        
        SetInactive();
        Hide();
        
        e.Cancel = true;
    }

    public FilterOutputData? GetFilters()
    {
        if (DataContext is FilterGameViewModel viewModel)
            return viewModel.GetFilters();

        return null;
    }
}