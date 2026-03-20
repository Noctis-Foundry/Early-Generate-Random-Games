using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;

namespace GameRandom.ViewModels;

public class ConfirmFinishGameViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;
    [Inject] private ErrorService? _errorService = null!;

    private GameProgresses? _gameProgress;
    public GameProgresses? GameProgress
    {
        get => _gameProgress;
        set => SetProperty(ref _gameProgress, value);
    }

    private Bitmap? _imageBitmap;
    public Bitmap? ImageBitmap
    {
        get => _imageBitmap;
        set => SetProperty(ref _imageBitmap, value);
    }

    private string? _comment;

    public string? Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public bool IsAdded;

    public void LoadData(GameProgresses gameProgress)
    {
        GameProgress = gameProgress;
        Di.Container.ResolveFieldsFromClassInstance(this);
    }
    
    public async Task<bool> SaveEditAsync()
    {
        if (!IsRequiredParameters()) return false;

        byte[]? imageBytes = AvaloniaService.Instance.ConvertToWebpBytes(ImageBitmap);
        
        var finishedGame = new FinishedGames
        {
            GameProgressId = GameProgress.Id,
            ScreenShot = imageBytes,
            IsImprove = false
        };

        GameProgress.IsFinished = true;
        GameProgress.Comment = Comment;
        GameProgress.FinishTime = DateTime.UtcNow;

        var game = await _databaseService.GetFinishedGamesFromId(GameProgress.Id);

        if (game is not null)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "This app with game id is finished, skipping game"});
            IsAdded = true;
            return true;
        }
        
        IsAdded = await _databaseService.AddItemAsync(finishedGame);

        if (IsAdded)
            IsAdded = await _databaseService.UpdateAsync(GameProgress);

        if (IsAdded)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Game finished successfully", ErrorType = ErrorEnum.Message});
            return true;
        }
        
        _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Failed to save finished game", ErrorType = ErrorEnum.Error});
        return false;
    }
    
    private bool IsRequiredParameters()
    {
        if (GameProgress is null)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Game progress is not set", ErrorType = ErrorEnum.Error});
            return false;
        }

        if (Comment is null)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Comment is required", ErrorType = ErrorEnum.Error});
            return false;
        }

        if (ImageBitmap is null)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Image is required", ErrorType = ErrorEnum.Error});
            return false;
        }
        
        if (_databaseService is null)
        {
            _errorService?.ShowWindow(new ErrorStruct{ErrorMessage = "Database service is not available", ErrorType = ErrorEnum.Error});
            return false;
        }

        return true;
    }
}
