using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.SteamsContexts;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels.AdminSystem;

/// <summary>
/// ViewModel for the main application window. Manages lobby and challenge rules.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    [Inject] private readonly SteamWebApi _steamWebApi = null!;
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly ErrorService _errorService = null!;

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

    private ICommand? _adminPanelOpen;

    public ICommand? AdminPanelOpen
    {
        get => _adminPanelOpen;
        set => SetProperty(ref _adminPanelOpen, value);
    }

    /// <summary>
    /// Collection of users in the current lobby.
    /// </summary>
    private HashSet<ProfilerContext> _usersToLobby = new HashSet<ProfilerContext>();

    /// <summary>
    /// Gets the collection of users in the lobby.
    /// </summary>
    public HashSet<ProfilerContext> UsersToLobby => _usersToLobby;

    private readonly bool _isInitialized;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        _isInitialized = true;
    }

    /// <summary>
    /// Updates lobby data by loading information about all participants.
    /// </summary>
    /// <param name="tableCode">Table code for update (must be TableEnum.Lobby).</param>
    public async Task UpdateLobby(int tableCode)
    {
        await _semaphore.WaitAsync();

        if (!IsValidationUpdateLobby(tableCode))
            return;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var userData = User.GetInstance().GetUserInfo();

        try
        {
            if (await GetLobby(userData.LobbyId, cts.Token) is not { } lobbyContexts)
                return;

            var lobbyData = lobbyContexts.LobbyData;

            for (int i = 0; i < lobbyData.Count; i++)
            {
                var profileContext = await _steamWebApi.GetUserData(lobbyData[i].UserId);

                if (profileContext == null)
                {
                    _errorService.ShowWindow(new ErrorStruct
                        { ErrorMessage = "Not found profile context", ErrorType = ErrorEnum.Error });
                    return;
                }

                _usersToLobby.Add(profileContext);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to update lobby data: " + e.Message);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsValidationUpdateLobby(int tableCode)
    {
        if (!_isInitialized)
        {
            _errorService.ShowWindow(new ErrorStruct
            {
                ErrorMessage = "Not initialized MainWindowViewModel, Cant update lobby", ErrorType = ErrorEnum.Error
            });
            return false;
        }

        if ((TableEnum)tableCode != TableEnum.Lobby)
        {
            _errorService.ShowWindow(new ErrorStruct
                { ErrorMessage = $"Table code {tableCode} not correct for this method", ErrorType = ErrorEnum.Error });
            return false;
        }

        return true;
    }

    private async Task<Lobbies?> GetLobby(long lobbyId, CancellationToken cts)
    {
        var lobbyContexts = await _databaseService.GetLobbyById(lobbyId, cts);

        if (lobbyContexts == null || lobbyContexts.LobbyData.Count <= 0)
        {
            _errorService.ShowWindow(new ErrorStruct
                { ErrorMessage = $"No lobby context found with {lobbyId}", ErrorType = ErrorEnum.Error });
            return null;
        }

        return lobbyContexts;
    }

    /// <summary>
    /// Binds the lobby opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the lobby.</param>
    public void BindingOpenLobbyCommand(Action func)
    {
        if (OpenLobbyCommand is null)
            OpenLobbyCommand = new RelayCommand(func);
    }

    /// <summary>
    /// Binds the rules window opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the rules window.</param>
    public void BindingRulesWindow(Action func)
    {
        if (RulesOpen is null)
            RulesOpen = new RelayCommand(func);
    }

    public void BindingAdminPanel(Action func)
    {
        if (AdminPanelOpen is null)
            AdminPanelOpen = new RelayCommand(func);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}