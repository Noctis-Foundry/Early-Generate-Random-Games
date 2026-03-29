using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameRandom.Scr.Service;

public class TaskRunner
{
    public async Task<bool> Run(Func<Task> func)
    {
        try
        {
            await func.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return false;
        }
    }
    public async Task<bool> RunWithSemaphore(Func<Task> func, SemaphoreSlim semaphoreSlim, int waitTime)
    {
        if (!await semaphoreSlim.WaitAsync(waitTime))
        {
            Logger.Warning("Failed to acquire semaphoreSlim lock for method");
            return false;
        }

        try
        {
            await func.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return false;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task<bool> RunWithFinallyAction(Func<Task> func, Action closeWaiter)
    {
        try
        {
            await func.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return false;
        }
        finally
        {
            closeWaiter?.Invoke();
        }
    }
}