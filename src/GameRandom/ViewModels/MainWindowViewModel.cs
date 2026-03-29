using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.SteamsContexts;
using GameRandom.Src.UserData;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for the main application window. Manages lobby and challenge rules.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private const int LobbyUpdateTimeoutSeconds = 5;
    
    [Inject] private SteamWebApi? _steamWebApi = null!;
    [Inject] private DatabaseService? _databaseService = null!;
    [Inject] private ErrorService? _errorService = null!;

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
    private HashSet<ProfilerContext> _usersToLobby = new HashSet<ProfilerContext>();

    /// <summary>
    /// Gets the collection of users in the lobby.
    /// </summary>
    public HashSet<ProfilerContext> UsersToLobby => _usersToLobby;

    /// <summary>
    /// Semaphore to ensure thread-safe access to lobby data updates.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_steamWebApi is null)
            throw new NullReferenceException("Failed to inject Steam Web Service from di");
        if (_databaseService is null)
            throw new NullReferenceException("Failed to inject Database service from di");
        if (_errorService is null)
            throw new NullReferenceException("Failed to inject Error service from di");
    }

    /// <summary>
    /// Updates lobby data by loading information about all participants.
    /// </summary>
    /// <param name="tableCode">Table code for update (must be TableEnum.Lobby).</param>
    public async Task UpdateLobby(int tableCode)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!IsValidTableCode(tableCode))
                return;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(LobbyUpdateTimeoutSeconds));
            var userData = User.GetInstance().GetUserInfo();

            if (await GetLobby(userData.LobbyId, cts.Token) is not { } lobbyContexts)
                return;

            await LoadLobbyProfiles(lobbyContexts.LobbyData);
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

    /// <summary>
    /// Validates whether the table code is correct for lobby operations.
    /// </summary>
    /// <param name="tableCode">Table code to validate.</param>
    /// <returns>True if validation passes; otherwise, false.</returns>
    private bool IsValidTableCode(int tableCode)
    {
        if ((TableEnum)tableCode != TableEnum.Lobby)
        {
            _errorService.ShowWindow(new ErrorStruct
                { ErrorMessage = $"Table code {tableCode} not correct for this method", ErrorType = ErrorEnum.Error });
            return false;
        }

        return true;
    }

    /// <summary>
    /// Loads profile information for all users in the lobby.
    /// </summary>
    /// <param name="lobbyData">List of lobby users to load profiles for.</param>
    private async Task LoadLobbyProfiles(List<LobbyData> lobbyData)
    {
        foreach (var lobbyUser in lobbyData)
        {
            var profileContext = await _steamWebApi.GetUserData(lobbyUser.UserId);
            
            if (profileContext == null)
            {
                _errorService.ShowWindow(new ErrorStruct
                    { ErrorMessage = "Not found profile context", ErrorType = ErrorEnum.Error });
                return;
            }

            _usersToLobby.Add(profileContext);
        }
    }

    /// <summary>
    /// Retrieves lobby information from the database.
    /// </summary>
    /// <param name="lobbyId">Lobby identifier.</param>
    /// <param name="cts">Cancellation token for the operation.</param>
    /// <returns>Lobby context if found; otherwise, null.</returns>
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

    /// <summary>
    /// Binds the admin panel opening command to the specified action.
    /// </summary>
    /// <param name="func">Action executed when opening the admin panel.</param>
    public void BindingAdminPanel(Action func)
    {
        if (AdminPanelOpen is null)
            AdminPanelOpen = new RelayCommand(func);
    }
}