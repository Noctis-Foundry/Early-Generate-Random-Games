using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameRandom.DISystem.DiSystem;
using GameRandom.Scripts.Service;
using GameRandom.ViewModels;

namespace GameRandom.Views.MainWindowSystem;

public partial class LoadControl : UserControl, IDisposable
{
    public LoadControl()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        DataContext = new LoadViewModel();
    }
    
    public void Dispose()
    {
        WaitBar.IsIndeterminate = false;
    }
}