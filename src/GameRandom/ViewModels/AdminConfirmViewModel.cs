using System;
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

    public async Task ConfirmGame(CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreConfirmSlim.WaitAsync(0))
        {
            Logger.Error("Failed to acquire semaphore");
            return;
        }

        try
        {
            if (FinishedGame is null || FinishedGame.GameProgresses is null || _databaseService is null)
                return;

            FinishedGame.IsImprove = true;

            var isUpdated = await _databaseService.UpdateAsync(FinishedGame);

            if (isUpdated)
            {
                FinishedGame = null;
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to confirm game with exception: {e}");
        }
        finally
        {
            _semaphoreConfirmSlim.Release();
        }
    }

    public async Task RejectGame(CancellationToken cancellationToken = default)
    {
        if (!await _semaphoreRejectSlim.WaitAsync(0))
        {
            Logger.Error("Failed to acquire semaphore");
            return;
        }

        try
        {
            var gameProgress = FinishedGame?.GameProgresses;

            if (gameProgress is null || FinishedGame is null || _databaseService is null)
                return;

            gameProgress.FinishTime = default;
            gameProgress.IsFinished = false;
            
            var user = await _databaseService.GetUserGameAsync(User.GetInstance().GetUserInfo(), cancellationToken);

            if (user is null)
                throw new NullReferenceException("Failed to get user game info");

            user.AppId = gameProgress.AppId;

            bool isUpdated = await _databaseService.UpdateAsync(user, cancellationToken);
            
            if (isUpdated)
                isUpdated = await _databaseService.UpdateAsync(gameProgress, cancellationToken);

            if (!isUpdated)
            {
                Logger.Error("Failed to update game progress in the table");
                return;
            }

            bool isRemoved = await _databaseService.DeleteItemAsync(FinishedGame);

            if (isRemoved)
            {
                FinishedGame = null;
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to reject game");
        }
        finally
        {
            _semaphoreRejectSlim.Release();
        }
    }
}