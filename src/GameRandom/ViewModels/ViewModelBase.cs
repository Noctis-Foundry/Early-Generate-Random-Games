using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Views;

namespace GameRandom.ViewModels.AdminSystem;

public class ViewModelBase : ObservableObject, IDisposable
{
    public Action? StartProcessing { get; set; }

    protected bool IsProcess;

    protected void StartTaskWaiter()
    {
        IsProcess = true;
        StartProcessing?.Invoke();
    }
    
    public virtual void Dispose()
    {
        StartProcessing = null;
    }

    public bool IsProcessing() => IsProcess;
}