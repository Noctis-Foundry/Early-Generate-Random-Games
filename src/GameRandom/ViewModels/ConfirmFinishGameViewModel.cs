using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.ViewModels.AdminSystem;

/// <summary>
/// ViewModel responsible for confirming that a game has been finished.
/// Handles validation, screenshot processing, database transition, and UI feedback.
/// </summary>
public class ConfirmFinishGameViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    [Inject] private ErrorService? _errorService;

    /// <summary>
    /// Timeout in seconds for database operations.
    /// </summary>
    private const int DatabaseOperationSecDelay = 5;
    
    private GameProgresses? _gameProgress;

    /// <summary>
    /// Gets or sets the game progress associated with the finish confirmation.
    /// </summary>
    public GameProgresses? GameProgress
    {
        get => _gameProgress;
        set => SetProperty(ref _gameProgress, value);
    }
    
    private Bitmap? _imageBitmap;

    /// <summary>
    /// Gets or sets the screenshot bitmap.
    /// </summary>
    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }
    
    private string? _comment;

    /// <summary>
    /// Gets or sets the comment attached to the finished game.
    /// </summary>
    public string? Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    /// <summary>
    /// Indicates whether the database update was successful.
    /// </summary>
    public bool IsUpdated { get; private set; }

    /// <summary>
    /// Initializes the ViewModel with the provided game progress
    /// and resolves required services through the DI container.
    /// </summary>
    /// <param name="gameProgress">Game progress to finalize.</param>
    public void LoadData(GameProgresses gameProgress)
    {
        GameProgress = gameProgress;
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService is null)
            throw new NullReferenceException("Failed to inject database service");
        if (_errorService is null)
            throw new NullReferenceException("Failed to inject error service");
    }

    /// <summary>
    /// Validates input parameters and saves the finished game state to the database.
    /// </summary>
    /// <returns>True if the operation succeeded; otherwise false.</returns>
    public async Task<bool> SaveEditAsync()
    {
        if (!IsRequiredParameters()) return false;

        var finishedGame = CreateFinishedGame();
        UpdatingGameProgress();

        using var cancellationTokenSource = new CancellationTokenSource(DatabaseOperationSecDelay);
        
        IsUpdated = await _databaseService?.TransitionFinishGame(
            finishedGame,
            GameProgress,
            cancellationTokenSource.Token);

        ShowResultWindow(IsUpdated);
        return IsUpdated;
    }

    /// <summary>
    /// Creates a new FinishedGames entity using the current ViewModel state.
    /// Converts the screenshot bitmap into WebP byte format.
    /// </summary>
    /// <returns>Prepared FinishedGames entity.</returns>
    private FinishedGames CreateFinishedGame()
    {
        // Convert screenshot bitmap to WebP byte array
        byte[]? imageBytes = AvaloniaService.Instance.ConvertToWebpBytes(ImageBitmap);

        return new FinishedGames
        {
            GameProgressId = GameProgress.Id,
            ScreenShot = imageBytes,
            IsImprove = false
        };
    }

    /// <summary>
    /// Updates the associated GameProgress entity to mark the game as finished.
    /// </summary>
    private void UpdatingGameProgress()
    {
        GameProgress.IsFinished = true;
        GameProgress.Comment = Comment;
        GameProgress.FinishTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Displays a result message depending on whether the update succeeded.
    /// </summary>
    /// <param name="isUpdate">Indicates success of the database operation.</param>
    private void ShowResultWindow(bool isUpdate)
    {
        if (isUpdate)
            _errorService.ShowWindow("Game is finished successfully");
        else
            _errorService.ShowWindow("Failed to finish game");
    }

    /// <summary>
    /// Validates that all required parameters are present before saving.
    /// </summary>
    /// <returns>True if validation passed; otherwise false.</returns>
    private bool IsRequiredParameters()
    {
        if (GameProgress is null)
            throw new NullReferenceException("Game progress is not initialized");

        if (Comment is null)
        {
            _errorService?.ShowWindow(new ErrorStruct
            {
                ErrorMessage = "Comment is required",
                ErrorType = ErrorEnum.Error
            });
            return false;
        }

        if (ImageBitmap is null)
        {
            _errorService?.ShowWindow(new ErrorStruct
            {
                ErrorMessage = "Image is required",
                ErrorType = ErrorEnum.Error
            });
            return false;
        }

        return true;
    }

    /// <summary>
    /// Releases resources and cancels any ongoing operations.
    /// </summary>
    public override void Dispose()
    {
        // Clear references
        GameProgress = null;
        ImageBitmap = null;
        Comment = null;

        _databaseService = null;
        _errorService = null;
    }
}