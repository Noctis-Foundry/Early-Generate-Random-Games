using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.Views;

namespace GameRandom.ViewModels.AdminSystem;

public class ViewModelBase : ObservableObject, IDisposable
{
    public Action? StartProcessing { get; set; }

    protected bool IsProcess;
    
    public virtual void Dispose()
    {
        StartProcessing = null;
    }

    public bool IsProcessing() => IsProcess;
}