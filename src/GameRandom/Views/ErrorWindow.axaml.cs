using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.Views;

public partial class ErrorWindow : WindowAbstract
{
    private ErrorEnum _currentErrorType;
    
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
        Close();
    }
}