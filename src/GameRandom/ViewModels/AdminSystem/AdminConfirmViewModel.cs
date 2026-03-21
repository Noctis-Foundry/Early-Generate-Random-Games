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
    [Inject] private DatabaseService? _databaseService;
    [Inject] private ErrorService? _errorService;

    private readonly SemaphoreSlim _actionSlim = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromSeconds(10));

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
            throw new NullReferenceException(nameof(_databaseService));
        if (_errorService is null)
            throw new NullReferenceException(nameof(_errorService));
    }

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

    public async Task<bool> ConfirmGame()
    {
        if (!await _actionSlim.WaitAsync(0))
        {
            _errorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
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
            _actionSlim.Release();
        }

        return false;
    }

    public async Task<bool> RejectGame()
    {
        if (!await _actionSlim.WaitAsync(0))
        {
            _errorService?.ShowWindow("Processing, please wait…", ErrorEnum.Message);
            return false;
        }

        try
        {
            if (FinishedGame is null || FinishedGame.GameProgresses is null)
                return false;
            
            var gameProgress = FinishedGame.GameProgresses;

            gameProgress.FinishTime = default;
            gameProgress.IsFinished = false;

            var user = await ChangeUserGame(gameProgress.PlayerId, _cancellationTokenSource.Token);
            
            var isUpdated = await _databaseService.TransitionRejectGame(FinishedGame, gameProgress, user);

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
    
    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _actionSlim.Release();
        _actionSlim.Dispose();

        FinishedGame = null;
        _finishedGame = null;

        _nickName = null;
        NickName = null;

        _databaseService = null;
    }
}