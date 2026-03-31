using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem.Interface;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public sealed class AdminConfirmViewModel : ViewModelBase
{
    #region BindingProperty
    
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
    
    #endregion

    private IAdminConfirm _adminConfirm = new AdminConfirmActions();

    /// <summary>
    /// Initializes a new dependence of the <see cref="AdminConfirmViewModel"/> class.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if required services are not injected.</exception>
    public AdminConfirmViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();
    }

    /// <summary>
    /// Updates the view model data with the provided finished game information.
    /// </summary>
    /// <param name="elementData">The finished game data to display.</param>
    /// <param name="cts"></param>
    /// <exception cref="NullReferenceException">Thrown if database service or user is not found.</exception>
    public async Task UpdateElementData(FinishedGames elementData, CancellationToken cts = default)
    {
        if (elementData.GameProgresses is null) return;
        
        if (Di.Container.GetInstance<DatabaseService>() is not DatabaseService databaseService)
            throw new NullReferenceException(nameof(databaseService));

        if (databaseService is null)
            throw new NullReferenceException(nameof(databaseService));
        
        var user = await databaseService.GetUserByUlongId(elementData.GameProgresses.PlayerId, cts);

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
        if (FinishedGame is null) return false;

        StartTaskWaiter();
        
        return await TaskRunner.RunWithFinallyAction(() => _adminConfirm.AcceptGame(FinishedGame),
            CloseTaskWaiter);
    }
    
    /// <summary>
    /// Rejects the game, resetting progress and updating the user's game list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    public async Task<bool> RejectGame()
    {
        if (FinishedGame is null) return false;

        StartTaskWaiter();
        
        return await TaskRunner.RunWithFinallyAction(() => _adminConfirm.RejectGame(FinishedGame), CloseTaskWaiter);
    }
    
    /// <summary>
    /// Releases resources and resets the view model state.
    /// </summary>
    public override void Dispose()
    {
        FinishedGame = null;
        _finishedGame = null;

        _nickName = null;
        NickName = null;
        
        _adminConfirm.Dispose();
        
        base.Dispose();
    }
}