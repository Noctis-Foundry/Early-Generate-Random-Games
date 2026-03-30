using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.CoreApp;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.ChooseGameSystem;
using GameRandom.ViewModels.ChooseGameSystem.Interface;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for choosing and setting up a game from the random selection.
/// Handles game selection, validation, and database persistence.
/// </summary>
public sealed class ChooseGameViewModel : ViewModelBase
{
    private IChooseGame _chooseGameService = new ChooseGameActions();
    
    private AppInfo? _appInfo;
    /// <summary>
    /// Gets or sets the currently selected application information.
    /// </summary>
    public AppInfo? AppInfo
    {
        get => _appInfo;
        set => SetProperty(ref _appInfo, value);
    }

    public ChooseGameViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();
    }
    
    /// <summary>
    /// Selects the current game and saves it to the database.
    /// Validates that the user can start a new game before proceeding.
    /// </summary>
    /// <returns>True if the game was successfully chosen and saved; otherwise, false.</returns>
    /// <exception cref="NullReferenceException">Thrown when AppInfo or UserGame is null.</exception>
    public async Task<bool> ChooseGame()
    {
        if (!await SemaphoreSlim.WaitAsync(0))
        {
            Logger.Error("Thread is not empty");
            return false;
        }

        if (_appInfo is null)
        {
            Logger.Error("Failed to load _appInfo");
            return false;
        }
        
        StartTaskWaiter();

        return await TaskRunner.RunWithFinallyAction(async () => await _chooseGameService.ChooseGame(_appInfo),
            CloseTaskWaiterWithSemaphore);
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
        _appInfo = null;
        AppInfo = null;
        
        _chooseGameService.Dispose();
        _chooseGameService = null!;
        
        base.Dispose();
    }
}