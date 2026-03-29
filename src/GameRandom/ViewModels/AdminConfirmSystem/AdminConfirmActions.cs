using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.AdminConfirmSystem.Interface;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public sealed class AdminConfirmActions : BaseModelService, IAdminConfirm
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
    
    public async Task<bool> RejectGame(FinishedGames finishedGame)
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            ErrorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
            return false;
        }

        using var cts = new CancellationTokenSource
            (TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));
        
        try
        {
            var gameProgress = finishedGame.GameProgresses;

            if (!IsDataNotNull(finishedGame))
                return false;
                
            gameProgress.FinishTime = default;
            gameProgress.IsFinished = false;

            var user = await ChangeUserGame(gameProgress, cts.Token);
            
            var isUpdated = await DatabaseService.TransitionRejectGame(finishedGame, gameProgress, user, cts.Token);

            return isUpdated;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> AcceptGame(FinishedGames finishedGame)
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            ErrorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
            return false;
        }

        try
        {
            using var cts = new CancellationTokenSource
                (TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));

            finishedGame.IsImprove = true;

            var isUpdating = await DatabaseService.UpdateAsync(finishedGame, cts.Token);

            return isUpdating;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    /// <summary>
    /// Updates the user's current game or adds the game back to their pending list.
    /// </summary>
    /// <param name="steamId">Steam ID of the user.</param>
    /// <param name="gameProgresses"></param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated UserGame object.</returns>
    /// <exception cref="NullReferenceException">Thrown if user game info cannot be retrieved.</exception>
    private async Task<UserGame?> ChangeUserGame(GameProgresses gameProgresses,
        CancellationToken cancellationToken)
    {
        var user = await DatabaseService.GetUserGameAsync(gameProgresses.PlayerId, 
            cancellationToken);

        if (user is null)
        {
            ErrorService?.ShowWindow("User info not found", ErrorEnum.Error);
            return null;
        }

        if (user.AppId == 0)
            user.AppId = gameProgresses.AppId;
        else
        {
            if (user.AppIdList is null)
                user.AppIdList = new List<int>();

            user.AppIdList.Add(gameProgresses.AppId);
        }

        return user;
    }
    
    public override void Dispose()
    {
        _semaphoreSlim.Dispose();
    }
}