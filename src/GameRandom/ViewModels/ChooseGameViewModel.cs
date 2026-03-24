using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels.AdminSystem;

/// <summary>
/// ViewModel for choosing and setting up a game from the random selection.
/// Handles game selection, validation, and database persistence.
/// </summary>
public class ChooseGameViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;
    [Inject] private ErrorService? _errorService = null!;
    [Inject] private SteamService? _steamService;
    
    private const int DefaultGameDurationDays = 30;
    private const int DatabaseOperationDelay = 5;
    
    private AppInfo? _appInfo;

    /// <summary>
    /// Gets or sets the currently selected application information.
    /// </summary>
    public AppInfo? AppInfo
    {
        get => _appInfo;
        set => SetProperty(ref _appInfo, value);
    }

    /// <summary>
    /// Initializes a new instance of the ChooseGameViewModel class.
    /// Resolves dependencies through dependency injection.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when required services are not initialized.</exception>
    public ChooseGameViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (_databaseService is null)
            throw new NullReferenceException("DatabaseService is not initialized.");
        if (_errorService is null)
            throw new NullReferenceException("ErrorService is not initialized.");
        if (_steamService is null)
            throw new NullReferenceException("Steam service is not initialized.");
    }
    
    /// <summary>
    /// Selects the current game and saves it to the database.
    /// Validates that the user can start a new game before proceeding.
    /// </summary>
    /// <returns>True if the game was successfully chosen and saved; otherwise, false.</returns>
    /// <exception cref="NullReferenceException">Thrown when AppInfo or UserGame is null.</exception>
    public async Task<bool> ChooseGame()
    {
        if (_appInfo is null)
            throw new NullReferenceException("AppInfo is null. Cannot choose game without loaded app information.");

        using var cts = new CancellationTokenSource(DatabaseOperationDelay);
        
        var userGame = await _databaseService.GetUserGameAsync(User.GetInstance().GetUserId(), cts.Token) 
            ?? throw new NullReferenceException("User game is not initialized.");

        if (!ValidateUserCanStartNewGame(userGame))
            return false;

        var webpBytes = ConvertAppImageToWebp();
        var gameInfo = CreateGameProgress(webpBytes);

        if (!await _databaseService.AddItemAsync(gameInfo,  cts.Token))
        {
            _errorService?.ShowWindow("Failed to add game to database");
            return false;
        }

        userGame.AppId = _appInfo.AppData.AppId;
        await _databaseService.UpdateAsync(userGame,  cts.Token);

        return true;
    }

    /// <summary>
    /// Converts the application image from bytes to WebP format.
    /// </summary>
    /// <returns>WebP image as byte array.</returns>
    /// <exception cref="NullReferenceException">Thrown when bitmap conversion fails.</exception>
    private byte[] ConvertAppImageToWebp()
    {
        var bitmap = _steamService.GetImageSyncFromBytes(_appInfo!.ImageBytes) 
            ?? throw new NullReferenceException("Failed to load bitmap from image bytes.");
        
        return AvaloniaService.Instance.ConvertToWebpBytes(bitmap);
    }

    /// <summary>
    /// Creates a new GameProgresses entity with the provided image and current app information.
    /// </summary>
    /// <param name="webpBytes">Game header image in WebP format.</param>
    /// <returns>Configured GameProgresses entity ready for database insertion.</returns>
    private GameProgresses CreateGameProgress(byte[] webpBytes)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(DefaultGameDurationDays);

        return new GameProgresses
        {
            AppHeaderImage = webpBytes,
            AppId = _appInfo!.AppData.AppId,
            AppName = _appInfo.AppData.AppName,
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
    /// <param name="userGame">Current user game state.</param>
    /// <returns>True if the user can start a new game; otherwise, false.</returns>
    private bool ValidateUserCanStartNewGame(UserGame userGame)
    {
        if (userGame.AppId == 0)
            return true;
        
        _errorService?.ShowWindow(new ErrorStruct
        {
            ErrorMessage = "Failed to set new game. Finish your current game",
            ErrorType = ErrorEnum.Message
        });

        return false;
    }

    /// <summary>
    /// Loads game information from saved context and image data.
    /// </summary>
    /// <param name="appSavedContext">Saved application context data.</param>
    /// <param name="imageBytes">Game header image as byte array.</param>
    public void LoadGameInfo(AppSavedContext appSavedContext, byte[] imageBytes)
    {
        AppInfo = new AppInfo(appSavedContext, imageBytes);
    }

    /// <summary>
    /// Opens the Steam store page for the current game in the default browser.
    /// </summary>
    public void ShowSteamStore()
    {
        var url = $"https://store.steampowered.com/app/{_appInfo?.AppData.AppId}";
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Releases resources and clears the current app information.
    /// </summary>
    public override void Dispose()
    {
        AppInfo = null;
        
        _databaseService = null;
        _errorService = null;
    }
}