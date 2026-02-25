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

    public bool CheckFilters(AppSavedContext apps)
    {
        if (DataContext is not FilterGameViewModel vm) return false;

        var selectedItems = vm.GetCategory();

        if (selectedItems.Categories.Count > 0 &&
            !selectedItems.Categories.Any(g => apps.AppCategoris.Contains(g)))
            return false;

        if (selectedItems.Genres.Count > 0 &&
            !selectedItems.Genres.Any(g => apps.AppGenres.Contains(g)))
            return false;
        
        return true;
    }

    public int GetYear()
    {
        if (DataContext is not FilterGameViewModel vm) return Random.Shared.Next(2003, 2026);

        var vmSelectedYears = vm.SelectedYears;
        
        int year = vmSelectedYears.Count > 0 ? vmSelectedYears[Random.Shared.Next(0, vmSelectedYears.Count)] : Random.Shared.Next(2003, 2026);
        
        return year;
    }
}