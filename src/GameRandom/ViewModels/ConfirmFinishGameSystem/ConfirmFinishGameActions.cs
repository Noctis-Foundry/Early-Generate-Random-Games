using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.ConfirmFinishGameSystem.Interface;

namespace GameRandom.ViewModels.ConfirmFinishGameSystem;

public sealed class ConfirmFinishGameActions : BaseModelService, IConfirmFinishGame
{
    [Inject] private DatabaseTransitionService _transitionService = null!;

    public ConfirmFinishGameActions() : base()
    {
        if (_transitionService is null)
            throw new NullReferenceException("Failed to inject dependence 'transition service'");
    }
    
    public async Task<bool> SaveEditAsync(GameProgresses gameInfo, string comment, Bitmap image)
    {
        var finishedGame = CreateFinishedGame(gameInfo, image);
        UpdatingGameProgress(gameInfo, comment);

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));

        var IsUpdated = await _transitionService.TransitionFinishGame(
            finishedGame,
            gameInfo,
            cancellationTokenSource.Token);
        
        return IsUpdated;
    }

    /// <summary>
    /// Creates a new FinishedGames entity using the current ViewModel state.
    /// Converts the screenshot bitmap into WebP byte format.
    /// </summary>
    /// <returns>Prepared FinishedGames entity.</returns>
    private FinishedGames CreateFinishedGame(GameProgresses gameInfo, Bitmap image)
    {
        // Convert screenshot bitmap to WebP byte array
        byte[]? imageBytes = AvaloniaService.Instance.ConvertToWebpBytes(image);

        return new FinishedGames
        {
            GameProgressId = gameInfo.Id,
            ScreenShot = imageBytes,
            IsImprove = false
        };
    }

    /// <summary>
    /// Updates the associated GameProgress entity to mark the game as finished.
    /// </summary>
    private void UpdatingGameProgress(GameProgresses gameInfo, string comment)
    {
        gameInfo.IsFinished = true;
        gameInfo.Comment = comment;
        gameInfo.FinishTime = DateTime.UtcNow;
    }
}
