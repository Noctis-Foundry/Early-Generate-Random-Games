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
using GameRandom.Views;
using GameRandom.Views.MainWindowSystem;

namespace GameRandom.ViewModels.MainWindowSystem;

/// <summary>
/// ViewModel for the main application window. Manages lobby and challenge rules.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    public IControlNavigate UserControlNavigate { get; private set; }
    
    #region BindingArea
    
    /// <summary>
    /// Command to open the lobby window.
    /// </summary>
    private ICommand? _openLobbyCommand;

    public ICommand? OpenLobbyCommand
    {
        get => _openLobbyCommand;
        set => SetProperty(ref _openLobbyCommand, value);
    }

    /// <summary>
    /// Command to open the challenge rules window.
    /// </summary>
    private ICommand? _rulesOpen;

    public ICommand? RulesOpen
    {
        get => _rulesOpen;
        set => SetProperty(ref _rulesOpen, value);
    }

    /// <summary>
    /// Command to open the admin panel window.
    /// </summary>
    private ICommand? _adminPanelOpen;

    /// <summary>
    /// Gets or sets the command to open the admin panel.
    /// </summary>
    public ICommand? AdminPanelOpen
    {
        get => _adminPanelOpen;
        set => SetProperty(ref _adminPanelOpen, value);
    }

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
        set => SetProperty(ref _usersToLobby, value);
    }
    
    #endregion
    
    private ILobbyUpdate _lobbyUpdate = new MainWindowUpdateLobby();
    
    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel()
    {
        InitializeDiContainer();
        InitializeSemaphoreSlim();

        UserControlNavigate = new NavigateUserControls();
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

    #region BindingMenuItems

    /// <summary>
    /// Binds the lobby opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the lobby.</param>
    public void BindingOpenLobbyCommand(Action func)
    {
        if (OpenLobbyCommand is null)
            OpenLobbyCommand = new RelayCommand(func); //TODO Вынести в Navigate system
    }

    /// <summary>
    /// Binds the rules window opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the rules window.</param>
    public void BindingRulesWindow(Action func)
    {
        if (RulesOpen is null)
            RulesOpen = new RelayCommand(func); //TODO Вынести в Navigate system
    }

    /// <summary>
    /// Binds the admin panel opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the admin panel.</param>
    public void BindingAdminPanel()
    {
        if (AdminPanelOpen is not null) return;
        
        AdminPanelOpen = new RelayCommand(() =>
        {
            if (!User.GetInstance().IsAdmin())
                return;
            
            UserControlNavigate.Navigate(ControlTypes.Admin);
        });
    }

    #endregion
    
    public override void Dispose()
    {
        _lobbyUpdate?.Dispose();
        _lobbyUpdate = null!;
        
        base.Dispose();
    }
}