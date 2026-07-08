using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameRandom.Scripts;

public static class TaskExtension
{
    public static async Task WithCancellation(this Task task, CancellationToken token)
    {
        await using var reg = token.Register(() => throw new OperationCanceledException(token));
        await task;
    }
    
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token)
    {
        await using var reg = token.Register(() => throw new OperationCanceledException(token));
        return await task;
    }
}