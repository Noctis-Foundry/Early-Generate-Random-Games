using System;
using GameRandom.DependenceInjectSystem;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using GameRandom.CoreApp;
using GameRandom.DependenceInjectSystem;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.Enums;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.UserData;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.ViewModels.ChooseGameSystem.Interface;
using GameRandom.DependenceInjectSystem;

namespace GameRandom.ViewModels.ChooseGameSystem;

public sealed class ChooseGameActions : BaseModelService, IChooseGame
{
    [Inject] private readonly SteamService _steamService = null!;
    [Inject] private readonly DatabaseTransitionService _transitionService = null!;
    private const int DefaultGameDurationDays = 30;
    private const int NoGameId = 0;

    public ChooseGameActions() : base()
    {
        if (_transitionService is null)
            throw new NullReferenceException("Failed to inject dependence 'transition service'");
    }
    
    public async Task<bool> ChooseGame(AppInfo appInfo)
    {
        if (!await ValidateUserCanStartNewGame())
            return false;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));

        var webpBytes = ConvertAppImageToWebp(appInfo);
        var gameInfo = CreateGameProgress(webpBytes, appInfo);

        var isAdd = await _transitionService.ChooseGameTransition(gameInfo, User.GetInstance().GetUserId(), cts.Token);

        ShowResult(isAdd);

        return isAdd;
    }
    
    /// <summary>
    /// Converts the application image from bytes to WebP format.
    /// </summary>
    /// <returns>WebP image as byte array.</returns>
    /// <exception cref="NullReferenceException">Thrown when bitmap conversion fails.</exception>
    private byte[] ConvertAppImageToWebp(AppInfo appInfo)
    {
        var bitmap = _steamService.GetImageSyncFromBytes(appInfo!.ImageBytes)
                     ?? throw new NullReferenceException("Failed to load bitmap from image bytes.");

        return AvaloniaService.Instance.ConvertToWebpBytes(bitmap);
    }

    private void ShowResult(bool isAdded)
    {
        if (isAdded)
            ErrorService?.ShowWindow("Game is successful added");
        if (!isAdded)
            ErrorService?.ShowWindow("Failed to added game to database");
    }

    /// <summary>
    /// Creates a new GameProgresses entity with the provided image and current app information.
    /// </summary>
    /// <param name="webpBytes">Game header image in WebP format.</param>
    /// <param name="appInfo"></param>
    /// <returns>Configured GameProgresses entity ready for database insertion.</returns>
    private GameProgresses CreateGameProgress(byte[] webpBytes, AppInfo appInfo)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(DefaultGameDurationDays);

        return new GameProgresses
        {
            AppHeaderImage = webpBytes,
            AppId = appInfo!.AppData.AppId,
            AppName = appInfo.AppData.AppName,
            BeginTime = startDate,
            Comment = "Default",
            EndTime = endDate,
            Grade = 0,
            IsFinished = false,
            FinishTime = endDate,
            PlayerId = SteamManager.GetSteamIdAsLong()
        };
    }

    /// <summary>
    /// Validates whether the user can start a new game.
    /// Shows an error message if the user already has an active game.
    /// </summary>
    /// <returns>True if the user can start a new game; otherwise, false.</returns>
    private async Task<bool> ValidateUserCanStartNewGame()
    {
        if (_steamService is null)
            throw new ArgumentNullException(nameof(_steamService));
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultDatabaseTimeLimitSecond));

        var userGame = await DatabaseService.GetUserGameAsync(User.GetInstance().GetUserId(), cts.Token);
        
        if (userGame is not null && userGame.AppId == NoGameId)
            return true;

        ErrorService.ShowWindow(new ErrorStruct
        {
            ErrorMessage = "Failed to set new game. Finish your current game",
            ErrorType = ErrorEnum.Message
        });

        return false;
    }

    public override void Dispose()
    {
    }
}