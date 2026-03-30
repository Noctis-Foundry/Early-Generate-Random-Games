using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameRandom.Scr.Service;

public class TaskRunner
{
    #region Run Methods

    /// <summary>
    /// Executes an asynchronous function and returns success status.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Executes an asynchronous function that returns a value, and returns success status and result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>A tuple containing success status and the result.</returns>
    public async Task<(bool Success, T? Result)> RunT<T>(Func<Task<T>> func)
    {
        try
        {
            var result = await func.Invoke();
            return (true, result);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return (false, default)!;
        }
    }

    #endregion

    #region Run With Semaphore Methods

    /// <summary>
    /// Executes an asynchronous function within a semaphore lock and returns success status.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="semaphoreSlim">The semaphore to use for synchronization.</param>
    /// <param name="waitTime">The maximum time in milliseconds to wait for the semaphore.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Executes an asynchronous function that returns a value within a semaphore lock, and returns success status and result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="semaphoreSlim">The semaphore to use for synchronization.</param>
    /// <param name="waitTime">The maximum time in milliseconds to wait for the semaphore.</param>
    /// <returns>A tuple containing success status and the result.</returns>
    public async Task<(bool Success, T? Result)> RunWithSemaphoreT<T>(Func<Task<T>> func, SemaphoreSlim semaphoreSlim, int waitTime)
    {
        if (!await semaphoreSlim.WaitAsync(waitTime))
        {
            Logger.Warning("Failed to acquire semaphoreSlim lock for method");
            return (false, default)!;
        }

        try
        {
            var result = await func.Invoke();
            return (true, result);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return (false, default)!;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    #endregion

    #region Run With Finally Action Methods

    /// <summary>
    /// Executes an asynchronous function and ensures a finalizer action is called, returning success status.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="closeWaiter">The action to perform in the finally block.</param>
    /// <returns>True if successful, false otherwise.</returns>
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

    /// <summary>
    /// Executes an asynchronous function that returns a value and ensures a finalizer action is called, returning success status and result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="closeWaiter">The action to perform in the finally block.</param>
    /// <returns>A tuple containing success status and the result.</returns>
    public async Task<(bool Success, T? Value)> RunWithFinallyActionT<T>(Func<Task<T>> func, Action closeWaiter)
    {
        try
        {
            var result = await func.Invoke();
            return (true, result);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to complete method with error: {e.Message}");
            return (false, default)!;
        }
        finally
        {
            closeWaiter?.Invoke();
        }
    }

    #endregion
}