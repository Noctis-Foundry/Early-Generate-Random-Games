using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.Service;
using GameRandom.Src.SteamsContexts;
using GameRandom.Src.UserData;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminConfirmSystem.Enums;
using GameRandom.ViewModels.BaseClasses;
using GameRandom.ViewModels.MainWindowSystem.Interface;
using GameRandom.ViewModels.MainWindowSystem.Services;
using GameRandom.Views;
using GameRandom.Views.MainWindowSystem;

namespace GameRandom.ViewModels.MainWindowSystem;

/// <summary>
/// ViewModel for the main application window. Manages lobby and challenge rules.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    public IControlNavigate UserControlNavigate { get; private set; }
    public ITopConCommandBinding ContainerCommandBinding { get; private set; }
    public IAdminLock AdminLock { get; private set; }
    
    /// <summary>
    /// Collection of users in the current lobby.
    /// </summary>
    private HashSet<ProfileContext> _usersToLobby = new();

    /// <summary>
    /// Gets or sets the collection of users in the lobby.
    /// </summary>
    public HashSet<ProfileContext> UsersToLobby
    {
        get => _usersToLobby;
        private set => SetProperty(ref _usersToLobby, value);
    }
    
    private ILobbyUpdate _lobbyUpdate = new MainWindowUpdateLobby();
    
    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();

        UserControlNavigate = new NavigateUserControls();
        AdminLock = new AdminLockService();

        AdminLock.Initialize();
    }

    public void InitializeCommands(Action openLobby, Action openRules)
    {
        ContainerCommandBinding = new TopConContainerCommands();
        
        ContainerCommandBinding.BindingLobby(openLobby);
        ContainerCommandBinding.BindingRules(openRules);
        ContainerCommandBinding.BindingAdmin(OpenAdminPanel);
    }

    private void OpenAdminPanel()
    {
        if (!User.GetInstance().IsAdmin())
            return;
        
        UserControlNavigate.Navigate(ControlTypes.Admin);
    }
    
    #region LobbyFunc

    /// <summary>
    /// Updates lobby data by loading information about all participants.
    /// </summary>
    /// <param name="tableCode">Table code for update (must be TableEnum.Lobby).</param>
    public async Task UpdateLobby()
    {
        if (!await SemaphoreSlim.WaitAsync(SemaphoreTimeWait))
        {
            Logger.Info("Thread is not empty");
            return;
        }

        try
        {
            var result = await _lobbyUpdate.UpdateLobby();

            if (result.Count == 0)
                return;

            UsersToLobby.Clear();
            UsersToLobby = new HashSet<ProfileContext>(result);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to update lobby: {e}");
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    #endregion
    
    public override void Dispose()
    {
        _lobbyUpdate?.Dispose();
        _lobbyUpdate = null!;
        
        UserControlNavigate.Dispose();
        ContainerCommandBinding.Dispose();
        AdminLock.Dispose();
        
        base.Dispose();
    }
}