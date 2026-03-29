using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.UserData;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for managing admin registration and permissions within a lobby.
/// </summary>
public class AdminRegistrationViewModel : ViewModelBase
{
    /// <summary>
    /// Service for database operations.
    /// </summary>
    [Inject] private DatabaseService? _databaseService;

    /// <summary>
    /// Listener for database changes in PostgreSQL.
    /// </summary>
    [Inject] private PostgresListener? _postgresListener;

    /// <summary>
    /// Cancellation token timeout
    /// </summary>
    private const int DatabaseTimeoutSec = 5;

    private ObservableCollection<AdminRegistrationData> _admins;

    /// <summary>
    /// Gets or sets the collection of potential and current admins for display.
    /// </summary>
    public ObservableCollection<AdminRegistrationData> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }

    /// <summary>
    /// Semaphore to ensure thread-safe operations on data loading.
    /// </summary>
    private SemaphoreSlim _semaphoreSlim = new(1, 1);

    /// <summary>
    /// Semaphore to ensure thread-safe execution of admin actions (add/remove).
    /// </summary>
    private SemaphoreSlim _isActionSemaphore = new(1, 1);

    /// <summary>
    /// Action to handle admin table updates from the database listener.
    /// </summary>
    private Action<PayloadStructure> _loadAdminTable;

    private const string AddAdmin = "Add admin";
    private const string RemoveAdmin = "Remove admin";

    /// <summary>
    /// The current lobby being managed.
    /// </summary>
    private Lobbies _currentLobby;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRegistrationViewModel"/> class.
    /// Resolves dependencies and starts data loading.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if required services are not injected.</exception>
    public AdminRegistrationViewModel()
    {
        _admins = new ObservableCollection<AdminRegistrationData>();
        Admins = new ObservableCollection<AdminRegistrationData>();

        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService == null)
            throw new NullReferenceException(nameof(_databaseService));
        if (_postgresListener == null)
            throw new NullReferenceException(nameof(_postgresListener));

        InitializeListeners();

        Dispatcher.UIThread.InvokeAsync(async () => await LoadData());
    }

    /// <summary>
    /// Initializes listeners for database update notifications.
    /// </summary>
    private void InitializeListeners()
    {
        _loadAdminTable += e =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (e.TableCode != (int)TableEnum.AdminTable)
                    return;

                await UpdateData();
            });
        };

        _postgresListener?.Subscribe(TableEnum.AdminTable, _loadAdminTable);
    }

    /// <summary>
    /// Loads the list of users and their admin status for the current lobby.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadData()
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Info("Admin is loading");
            return;
        }

        if (!User.GetInstance().IsTopLevelAdmin())
            return;

        IsProcess = true;
        StartProcessing?.Invoke();
        
        try
        {
            Admins.Clear();
            var userInfo = User.GetInstance().GetUserInfo();

            if (userInfo.LobbyId <= 0)
                return;

            _currentLobby = await _databaseService.GetLobbyById(userInfo.LobbyId);

            if (_currentLobby == null)
                throw new Exception("Lobby is not found");

            foreach (var user in await NotAdminUsers(_currentLobby))
            {
                Admins.Add(new AdminRegistrationData(user, AddAdmin, AddAdminCommand(user), false));
            }
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }
        finally
        {
            _semaphoreSlim.Release();
            IsProcess = false;
        }
    }

    /// <summary>
    /// Triggers a data reload if the current user has administrative rights.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateData()
    {
        if (User.GetInstance().IsTopLevelAdmin())
            await LoadData();
    }

    /// <summary>
    /// Filters members of the lobby who are not currently admins.
    /// </summary>
    /// <param name="lobbies">The lobby containing members.</param>
    /// <returns>A list of users who are not admins.</returns>
    private async Task<List<Users>> NotAdminUsers(Lobbies lobbies)
    {
        var users = new List<Users>();

        foreach (var lobbyMember in lobbies.LobbyData)
        {
            if (lobbyMember.UserId == User.GetInstance().GetUserId())
                continue;

            var user = await _databaseService.GetUserByUlongId(lobbyMember.UserId);

            if (user is null)
                continue;

            if (lobbies.AdminsList.Exists(e => e.SteamId == lobbyMember.UserId))
            {
                AddNewAdminToList(lobbies, user);
                continue;
            }

            users.Add(user);
        }

        return users;
    }

    /// <summary>
    /// Adds a user to the admin list if they are recognized as an admin in the lobby data.
    /// </summary>
    /// <param name="lobbies">The lobby context.</param>
    /// <param name="user">The user to check and add.</param>
    private void AddNewAdminToList(Lobbies lobbies, Users user)
    {
        var admin = lobbies.AdminsList.Find(e => e.SteamId == user.SteamId);

        if (admin is null || admin.IsTopAdmin)
            return;

        Admins.Add(new AdminRegistrationData(user, RemoveAdmin, RemoveAdminCommand(user), true));
    }

    /// <summary>
    /// Creates a command to grant administrative rights to a user.
    /// </summary>
    /// <param name="userInfo">The user to be promoted.</param>
    /// <returns>An <see cref="AsyncRelayCommand"/> that executes the promotion logic.</returns>
    private AsyncRelayCommand AddAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (!await _isActionSemaphore.WaitAsync(0))
            {
                ShowError("Wait for the previous command to complete");
                return;
            }

            IsProcess = true;
            StartProcessing?.Invoke();

            try
            {
                if (!User.GetInstance().IsTopLevelAdmin())
                    return;

                if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
                {
                    await UpdateLobby(userInfo, databaseService);

                }
                else
                    throw new NullReferenceException("Failed resolve database service");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
            finally
            {
                _isActionSemaphore.Release();
                IsProcess = false;
            }
        });
    }

    private async Task UpdateLobby(Users userInfo, DatabaseService databaseService)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeoutSec));

        var lobbies = await databaseService.GetLobbyById(userInfo.LobbyId, cts.Token);

        if (lobbies is null)
        {
            Logger.Error("Lobby is not founded");
            return;
        }

        lobbies.AdminsList.Add(new Admins
        {
            SteamId = userInfo.SteamId,
            LobbyId = userInfo.LobbyId,
            IsTopAdmin = false
        });

        var isUpdating = await databaseService.UpdateAsync(lobbies, cts.Token);

        if (!isUpdating)
            Logger.Error("Failed to add admin");
        else
            Logger.Info("Admin is added");
    }

    /// <summary>
    /// Creates a command to revoke administrative rights from a user.
    /// </summary>
    /// <param name="userInfo">The user whose rights are to be revoked.</param>
    /// <returns>An <see cref="AsyncRelayCommand"/> that executes the revocation logic.</returns>
    private AsyncRelayCommand RemoveAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (!User.GetInstance().IsTopLevelAdmin())
                return;
            
            if (!await _isActionSemaphore.WaitAsync(0))
            {
                ShowError("Wait for the previous command to complete");
                return;
            }

            IsProcess = true;
            StartProcessing?.Invoke();

            try
            {
                if (Di.Container.TryGetInstance<DatabaseService>() is not DatabaseService databaseService)
                    throw new NullReferenceException("Failed resolve database service");

                await DeleteAdminRules(databaseService, userInfo);
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
            }
            finally
            {
                _isActionSemaphore.Release();
                IsProcess = false;
            }
        });
    }

    private async Task DeleteAdminRules(DatabaseService databaseService, Users userInfo)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeoutSec));

        var isRemoving =
            await databaseService.DeleteItemWithPredicate<Admins>(e => e.SteamId == userInfo.SteamId,
                cts.Token);

        if (!isRemoving)
        {
            Logger.Error("Failed to remove admin");
        }
    }

    /// <summary>
    /// Displays an error message using the <see cref="ErrorService"/>.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <exception cref="NullReferenceException">Thrown if ErrorService is not available.</exception>
    private void ShowError(string message)
    {
        if (Di.Container.GetInstance<ErrorService>() is not ErrorService errorService)
            throw new NullReferenceException(nameof(ErrorService));

        errorService.ShowWindow(new ErrorStruct { ErrorMessage = message, ErrorType = ErrorEnum.Error });
    }

    /// <summary>
    /// Releases resources and unsubscribes from events.
    /// </summary>
    public override void Dispose()
    {
        _databaseService = null;

        _postgresListener?.Unsubscribe(TableEnum.AdminTable, _loadAdminTable);
        _postgresListener = null;

        _loadAdminTable = null!;
        

        _admins.Clear();
        Admins.Clear();
        
        base.Dispose();
    }
}

/// <summary>
/// Represents data for a single user in the admin registration view.
/// </summary>
/// <param name="userInfo">Information about the user.</param>
/// <param name="buttonText">The text for the action button.</param>
/// <param name="buttonCommand">The command to execute when the button is clicked.</param>
/// <param name="isAdmin">Indicates whether the user is currently an admin.</param>
public class AdminRegistrationData(Users userInfo, string buttonText, AsyncRelayCommand buttonCommand, bool isAdmin)
{
    /// <summary>
    /// Gets information about the user.
    /// </summary>
    public Users UserInfo { get; private set; } = userInfo;

    /// <summary>
    /// Gets the text for the action button.
    /// </summary>
    public string ButtonText { get; private set; } = buttonText;

    /// <summary>
    /// Gets the command to execute when the button is clicked.
    /// </summary>
    public AsyncRelayCommand ButtonCommand { get; private set; } = buttonCommand;

    /// <summary>
    /// Gets a value indicating whether the user is currently an admin.
    /// </summary>
    public bool IsAdmin { get; private set; } = isAdmin;
}