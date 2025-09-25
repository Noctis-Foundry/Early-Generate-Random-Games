using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class GameTable : UserControl
{
    private Action<string> _onShowContent;

    public GameTable()
    {
        InitializeComponent();
        
        if (Design.IsDesignMode)
            return;
        
    }

    public void AddListener(Action<string> _onChangeContent) => _onShowContent = _onChangeContent;

    private void Close(object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }
}