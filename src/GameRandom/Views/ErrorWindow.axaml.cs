using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.AdminConfirmSystem;

namespace GameRandom.Views;

public partial class ErrorWindow : WindowBase<ViewModelBase>
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;

        IsClosing = true;
        IsActive = false;
        
        Hide();

        e.Cancel = true;
    }
}