using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.ViewModels;

namespace MessageBox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void InitializeTextBlock(string message, string title)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.WindowContent = message;
            vm.WindowTitle = title;
        }
    }
    
    private void OkAction(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(1);
    }
}