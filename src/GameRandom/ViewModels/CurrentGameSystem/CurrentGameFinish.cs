using System;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Src;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.CurrentGameSystem.Interface;

namespace GameRandom.ViewModels.CurrentGameSystem;

public sealed class CurrentGameFinish : BaseModelService, ICurrentGameFinish
{
    [Inject] private FinishedGameDialogService _finishedGameDialogService = null!;
    [Inject] private SteamService _steamService = null!;

    public CurrentGameFinish()
    {
        CheckPropertyInNull();
    }

    private void CheckPropertyInNull()
    {
        if (_finishedGameDialogService is null)
            throw new NullReferenceException(nameof(_finishedGameDialogService));
    } // Check dependency in null after InitializeDiContainer from BaseModelService

    public async Task<UserGame> FinishingGame(GameProgresses gameInfo)
    {
        if (!await _finishedGameDialogService.ShowWindowAsync(gameInfo))
            return null!;
        
        
        return await ChangeUserGame();
    }
    
    /// <summary>
    /// Updates the user's game status in the database (moves to the next game or resets).
    /// </summary>
    private async Task<UserGame> ChangeUserGame()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));
        var userGame = await DatabaseService.GetUserGameAsync(User.GetInstance().GetUserId(), cts.Token);

        if (userGame is null)
            throw new NullReferenceException("User game is not initialized");

        if (userGame.AppIdList is not null && userGame.AppIdList.Count > 0)
        {
            userGame.AppId = userGame.AppIdList[0];
            userGame.AppIdList.RemoveAt(0);
        }
        else
            userGame.AppId = 0;

        var isUpdating = await DatabaseService.UpdateAsync(userGame, cts.Token);

        if (isUpdating)
            return userGame;

        return null!;
    }

    public override void Dispose()
    {
        _finishedGameDialogService = null!;
        _steamService = null!;
        
        base.Dispose();
    }
}
