using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.Scr.Service;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.ViewModels.BaseClasses;
using GameRandom.ViewModels.ConfirmFinishGameSystem;
using GameRandom.ViewModels.ConfirmFinishGameSystem.Interface;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel responsible for confirming that a game has been finished.
/// Handles validation, screenshot processing, database transition, and UI feedback.
/// </summary>
public sealed class ConfirmFinishGameViewModel : ViewModelBase
{
    private IConfirmFinishGame _confirmFinishGame = new ConfirmFinishGameActions();
    
    #region BindingProperty

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
    
    #endregion
    
    public bool IsUpdated { get; private set; }
    
    public ConfirmFinishGameViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();
    }
    
    /// <summary>
    /// Initializes the ViewModel with the provided game progress
    /// and resolves required services through the DI container.
    /// </summary>
    /// <param name="gameProgress">Game progress to finalize.</param>
    public void LoadData(GameProgresses gameProgress)
    {
        GameProgress = gameProgress;
    }

    /// <summary>
    /// Validates input parameters and saves the finished game state to the database.
    /// </summary>
    /// <returns>True if the operation succeeded; otherwise false.</returns>
    public async Task<bool> SaveEditAsync()
    {
        if (!IsRequiredParameters()) return false;

        if (!await SemaphoreSlimWaitAsync())
            return false;
        
        StartTaskWaiter();

        IsUpdated =  await TaskRunner.RunWithFinallyAction(
            async () => await _confirmFinishGame.SaveEditAsync(GameProgress, Comment, ImageBitmap), CloseTaskWaiterWithSemaphore); //GameProgress and Comment checked in IsRequiredParameters

        ShowResultWindow(IsUpdated);
        
        return IsUpdated;
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
            ErrorService?.ShowWindow(new ErrorStruct
            {
                ErrorMessage = "Comment is required",
                ErrorType = ErrorEnum.Error
            });
            return false;
        }

        if (ImageBitmap is null)
        {
            ErrorService?.ShowWindow(new ErrorStruct
            {
                ErrorMessage = "Image is required",
                ErrorType = ErrorEnum.Error
            });
            return false;
        }

        return true;
    }

    /// <summary>
    /// Displays a result message depending on whether the update succeeded.
    /// </summary>
    /// <param name="isUpdate">Indicates success of the database operation.</param>
    private void ShowResultWindow(bool isUpdate)
    {
        if (isUpdate)
            ErrorService.ShowWindow("Game is finished successfully");
        else
            ErrorService.ShowWindow("Failed to finish game");
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
        
        _confirmFinishGame.Dispose();
        _confirmFinishGame = null!;
        
        base.Dispose();
    }
}