using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views.MainWindowSystem;

public partial class LoadControl : UserControl, IDisposable
{
    public LoadControl()
    {
        InitializeComponent();
    }
    
    public void Dispose()
    {
        WaitBar.IsIndeterminate = false;
    }
}