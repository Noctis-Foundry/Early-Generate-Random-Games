using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameRandom.Scr.Service;

public class TaskRunner
{
    public async Task LaunchMethodWithTaskWaiter(Func<Task> func, CancellationToken cts = default)
    {
        try
        {
            await func.Invoke();
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
        }
    }
    public async Task LaunchMethodWithTaskWaiter(Func<Task> func, SemaphoreSlim semaphoreSlim, int waitTime, CancellationToken cts = default)
    {
        if (!await semaphoreSlim.WaitAsync(waitTime, cts))
        {
            Logger.Warning("Failed to acquire semaphoreSlim lock for method");
            return;
        }

        try
        {
            await func.Invoke();
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}