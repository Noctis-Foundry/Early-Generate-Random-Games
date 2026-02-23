using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.Views;

public partial class ErrorWindow : Window
{
    private ErrorEnum _currentErrorType;
    public bool IsActiveWindow { get; set; } = false;
    
    public ErrorWindow()
    {
        InitializeComponent();
        
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public void ChangeTextOnModal(string text, ErrorEnum errorType)
    {
        ErrorLabel.Text = text;
        Title = errorType.ToString();
        
        _currentErrorType = errorType;
        
        ErrorButton.Content = errorType == ErrorEnum.Critical ? "Close app" : "Ok"; 
    }
    

    private void MessageBoxButtonAction(object? sender, RoutedEventArgs e)
    {
        Hide();
        IsActiveWindow = false;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        IsActiveWindow = false;
        Hide();
    }
}