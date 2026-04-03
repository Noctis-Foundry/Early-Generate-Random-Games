using System;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.Views;
using GameRandom.DependenceInjectSystem;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class ViewModelBase : ObservableObject, IDisposable
{
    [Inject] protected TaskRunner TaskRunner = null!;
    [Inject] protected ErrorService ErrorService = null!;
    
    protected const int SemaphoreTimeWait = 2;
    protected const int DatabaseOperationDelay = 5;
    
    protected SemaphoreSlim SemaphoreSlim { get; private set; }
    
    protected virtual void InitializeDiContainer()
    {
        Di.ResolveInstance.ResolveInstanceFromClass(this);

        if (TaskRunner is null)
            throw new NullReferenceException(nameof(TaskRunner));
        if (ErrorService is null)
            throw new NullReferenceException(nameof(ErrorService));
    }
    
    protected void InitializeSemaphoreSlim() => SemaphoreSlim = new SemaphoreSlim(1, 1);
    
    protected async Task<bool> SemaphoreSlimWaitAsync()
    {
        if (!await SemaphoreSlim.WaitAsync(0))
        {
            ErrorService.ShowWindow("Operation is completing, please wait");
            return false;
        }

        return true;
    }
    
    #region TaskWaiter

    public Action? StartProcessing { get; set; }

    protected bool IsProcess;
    
    protected void StartTaskWaiter()
    {
        IsProcess = true;
        StartProcessing?.Invoke();
    }
    
    protected void CloseTaskWaiter()
    {
        IsProcess = false;
    }

    protected void CloseTaskWaiterWithSemaphore()
    {
        IsProcess  = false;
        SemaphoreSlim.Release();
    }
    
    public bool IsProcessing() => IsProcess;

    #endregion
    
    public virtual void Dispose()
    {
        StartProcessing = null;
        
        if (SemaphoreSlim?.CurrentCount == 0)
            SemaphoreSlim.Release();
        
        SemaphoreSlim?.Dispose();
        SemaphoreSlim = null!;

        TaskRunner = null!;
        ErrorService = null!;
    }

}