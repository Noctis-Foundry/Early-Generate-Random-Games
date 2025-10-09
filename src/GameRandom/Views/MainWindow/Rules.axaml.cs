using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class Rules : UserControl
{
    public Rules()
    {
        InitializeComponent();
    }
    private Action<string> _onShowContent;

    public void AddListener(Action<string> _onChangeContent) => _onShowContent = _onChangeContent;
    
    private void Close(Object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }
}