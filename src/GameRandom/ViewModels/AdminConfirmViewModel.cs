using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class AdminConfirmViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    private SemaphoreSlim _semaphoreConfirmSlim = new(1, 1);
    private SemaphoreSlim _semaphoreRejectSlim = new(1, 1);
    
    private FinishedGames? _finishedGame;

    public FinishedGames? FinishedGame
    {
        get => _finishedGame;
        set => SetProperty(ref _finishedGame, value);
    }

    private string? _nickName;

    public string? NickName
    {
        get => _nickName;
        set => SetProperty(ref _nickName, value);
    }

    public AdminConfirmViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (_databaseService is null)
            throw new NullReferenceException("Failed to resolve database from DI");
    }

    public async Task UpdateElementData(FinishedGames elementData)
    {
        if (elementData.GameProgresses is null) return;

        if (Di.Container.GetInstance<DatabaseService>() is DatabaseService databaseService)
        {
            var user = await databaseService.GetUserByUlongId(elementData.GameProgresses.PlayerId);

            if (user is null)
                throw new NullReferenceException("Failed to find user in the database");

            NickName = user.Nickname;
            FinishedGame = elementData;
        }
    }

    public async Task<bool> ConfirmGame(CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreConfirmSlim.WaitAsync(0))
        {
            Logger.Error("Failed to acquire semaphore");
            return false;
        }

        try
        {
            if (FinishedGame is null || FinishedGame.GameProgresses is null || _databaseService is null)
                return false;

            FinishedGame.IsImprove = true;

            var isUpdated = await _databaseService.UpdateAsync(FinishedGame);

            if (isUpdated)
            {
                FinishedGame = null;
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to confirm game with exception: {e}");
        }
        finally
        {
            _semaphoreConfirmSlim.Release();
        }

        return false;
    }

    public async Task<bool> RejectGame(CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreRejectSlim.WaitAsync(0))
        {
            Logger.Error("Failed to acquire semaphore");
            return false;
        }

        try
        {
            var gameProgress = FinishedGame?.GameProgresses;

            if (gameProgress is null || FinishedGame is null || _databaseService is null)
                return false;

            gameProgress.FinishTime = default;
            gameProgress.IsFinished = false;
            
            var user = await _databaseService.GetUserGameAsync(gameProgress.PlayerId, cancellationToken);

            if (user is null)
                throw new NullReferenceException("Failed to get user game info");

            if (user.AppId == 0)
                user.AppId = gameProgress.AppId;
            else
            {
                if (user.AppIdList is null) 
                    user.AppIdList = new List<int>();
                
                user.AppIdList.Add(gameProgress.AppId);
            }

            bool isUserGameUpdate = await _databaseService.UpdateAsync(user, cancellationToken);

            if (!isUserGameUpdate)
            {
                Logger.Error("Failed to update user game");
                return false;
            }
            
            bool isGameProgressUpdate = await _databaseService.UpdateAsync(gameProgress, cancellationToken);
            
            if (!isGameProgressUpdate)
            {
                Logger.Error("Failed to update game progresses");
                return false;
            }

            bool isRemoved = await _databaseService.DeleteItemAsync(FinishedGame);

            if (isRemoved)
            {
                FinishedGame = null;
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to reject game");
        }
        finally
        {
            _semaphoreRejectSlim.Release();
        }

        return false;
    }
}