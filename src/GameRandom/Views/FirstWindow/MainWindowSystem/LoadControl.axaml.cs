using System;
using Avalonia.Controls;
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