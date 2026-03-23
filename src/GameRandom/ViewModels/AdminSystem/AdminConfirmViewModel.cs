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

namespace GameRandom.ViewModels.AdminSystem;

public class AdminConfirmViewModel : ViewModelBase
{
    /// <summary>
    /// Service for database operations.
    /// </summary>
    [Inject] private DatabaseService? _databaseService;

    /// <summary>
    /// Service for showing error and status messages.
    /// </summary>
    [Inject] private ErrorService? _errorService;

    /// <summary>
    /// A semaphore with one slot ensures that either game confirmation or rejection is performed at a time.
    /// </summary>
    private readonly SemaphoreSlim _actionSlim = new(1, 1);
    private const int DatabaseOperationTimeoutSec = 5;

    /// <summary>
    /// The finished game object being processed.
    /// </summary>
    private FinishedGames? _finishedGame;

    /// <summary>
    /// Property for getting or setting the finished game.
    /// </summary>
    public FinishedGames? FinishedGame
    {
        get => _finishedGame;
        set => SetProperty(ref _finishedGame, value);
    }

    /// <summary>
    /// The nickname of the player associated with the game.
    /// </summary>
    private string? _nickName;

    /// <summary>
    /// Property for getting or setting the nickname.
    /// </summary>
    public string? NickName
    {
        get => _nickName;
        set => SetProperty(ref _nickName, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminConfirmViewModel"/> class.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if required services are not injected.</exception>
    public AdminConfirmViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null)
            throw new NullReferenceException(nameof(_databaseService));
        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
    }

    /// <summary>
    /// Updates the view model data with the provided finished game information.
    /// </summary>
    /// <param name="elementData">The finished game data to display.</param>
    /// <exception cref="NullReferenceException">Thrown if database service or user is not found.</exception>
    public async Task UpdateElementData(FinishedGames elementData)
    {
        if (elementData.GameProgresses is null) return;

        if (_databaseService is null)
            throw new NullReferenceException(nameof(_databaseService));
        
        var user = await _databaseService.GetUserByUlongId(elementData.GameProgresses.PlayerId);

        if (user is null)
            throw new NullReferenceException(nameof(user));

        NickName = user.Nickname;
        FinishedGame = elementData;
    }

    /// <summary>
    /// Confirms the game, marking it as improved in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    public async Task<bool> ConfirmGame()
    {
        // Attempt to acquire the semaphore. If busy (e.g., RejectGame is running), show an error.
        if (!await _actionSlim.WaitAsync(0))
        {
            _errorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
            return false;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationTimeoutSec));
        
        try
        {
            if (!IsCanConfirm())
                return false;

            FinishedGame.IsImprove = true;

            var isUpdated = await _databaseService.UpdateAsync(FinishedGame, cts.Token);

            if (isUpdated)
                FinishedGame = null;

            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to confirm game with exception: {e}");
        }
        finally
        {
            _actionSlim.Release();
        }

        return false;
    }

    /// <summary>
    /// Checks if the game can be confirmed.
    /// </summary>
    private bool IsCanConfirm()
    {
        return FinishedGame is not null || FinishedGame.GameProgresses is not null || _databaseService is not null;
    }
    
    /// <summary>
    /// Rejects the game, resetting progress and updating the user's game list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    public async Task<bool> RejectGame()
    {
        // Attempt to acquire the semaphore. If busy (e.g., ConfirmGame is running), show an error.
        if (!await _actionSlim.WaitAsync(0))
        {
            _errorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
            return false;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseOperationTimeoutSec));
        
        try
        {
            if (!IsCanReject())
                return false;
            
            var gameProgress = FinishedGame.GameProgresses;

            gameProgress.FinishTime = default;
            gameProgress.IsFinished = false;

            var user = await ChangeUserGame(gameProgress.PlayerId, cts.Token);
            
            var isUpdated = await _databaseService.TransitionRejectGame(FinishedGame, gameProgress, user, cts.Token);

            return isUpdated;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to reject game: " + e.Message);
        }
        finally
        {
            _actionSlim.Release();
        }

        return false;
    }

    /// <summary>
    /// Checks if the game can be rejected.
    /// </summary>
    private bool IsCanReject() => FinishedGame is not null && FinishedGame.GameProgresses is not null;

    /// <summary>
    /// Updates the user's current game or adds the game back to their pending list.
    /// </summary>
    /// <param name="steamId">Steam ID of the user.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated UserGame object.</returns>
    /// <exception cref="NullReferenceException">Thrown if user game info cannot be retrieved.</exception>
    private async Task<UserGame> ChangeUserGame(ulong steamId, CancellationToken cancellationToken)
    {
        var user = await _databaseService.GetUserGameAsync(steamId, cancellationToken);

        if (user is null)
            throw new NullReferenceException("Failed to get user game info");

        if (user.AppId == 0)
            user.AppId = FinishedGame.GameProgresses.AppId;
        else
        {
            if (user.AppIdList is null)
                user.AppIdList = new List<int>();

            user.AppIdList.Add(FinishedGame.GameProgresses.AppId);
        }

        return user;
    }
    
    /// <summary>
    /// Releases resources and resets the view model state.
    /// </summary>
    public override void Dispose()
    {
        _actionSlim.Release();
        _actionSlim.Dispose();

        FinishedGame = null;
        _finishedGame = null;

        _nickName = null;
        NickName = null;

        _databaseService = null;
    }
}