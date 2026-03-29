using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Views;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class ViewModelBase : ObservableObject, IDisposable
{
    [Inject] protected TaskRunner? TaskRunner;
    
    public Action? StartProcessing { get; set; }

    protected bool IsProcess;

    protected void StartTaskWaiter()
    {
        IsProcess = true;
        StartProcessing?.Invoke();
    }

    protected virtual void InitializeDiContainer()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (TaskRunner is null)
            throw new NullReferenceException();
    }

    protected void CloseTaskWaiter()
    {
        IsProcess = false;
    }
    
    public virtual void Dispose()
    {
        StartProcessing = null;
    }

    public bool IsProcessing() => IsProcess;
}