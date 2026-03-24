using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameRandom.Scr.Service;
using GameRandom.Views;

namespace GameRandom.SteamSDK;

public class TaskWaiterWindow(Window ownerWindow) : AbstractWindowService<TaskLoading>(ownerWindow), ITaskWaiter
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    
    public async Task<bool> ShowAsyncWaiter(Func<bool> isProcessing, Window? owner = null!)
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Debug("Thread is not empty");
            return false;
        }
        
        if (!isProcessing())
            return true;

        try
        {
            var localOwner = owner ?? OwnerWindow;

            ControlWindow = new();
            ControlWindow.LoadingWindow(isProcessing);
            var isEnd = await ControlWindow.ShowDialog<bool>(localOwner);

            if (isEnd)
                ControlWindow = null!;

            return isEnd;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to open processing window" + e.Message);
            return false;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}

public interface ITaskWaiter
{
    public Task<bool> ShowAsyncWaiter(Func<bool> isProcessing, Window? owner = null!);
}