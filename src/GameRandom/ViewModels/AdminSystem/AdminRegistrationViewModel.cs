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
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels.AdminSystem;

public class AdminRegistrationViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    [Inject] private PostgresListener? _postgresListener;

    private CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromSeconds(5));

    private ObservableCollection<AdminRegistrationData> _admins;

    public ObservableCollection<AdminRegistrationData> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }

    private SemaphoreSlim _semaphoreSlim = new(1, 1);
    private SemaphoreSlim _isActionSemaphore = new(1, 1);
    private Action<PayloadStructure> _loadAdminTable;

    private const string AddAdmin = "Add admin";
    private const string RemoveAdmin = "Remove admin";

    private Lobbies _currentLobby;

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

    private async Task LoadData()
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Info("Admin is loading");
            return;
        }

        if (!User.GetInstance().IsTopLevelAdmin())
            return;

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
        }
    }

    private async Task UpdateData()
    {
        if (User.GetInstance().IsTopLevelAdmin())
            await LoadData();
    }

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
                var admin = lobbies.AdminsList.Find(e => e.SteamId == lobbyMember.UserId);

                if (admin is null || admin.IsTopAdmin)
                    continue;

                Admins.Add(new AdminRegistrationData(user, RemoveAdmin, RemoveAdminCommand(user), true));

                continue;
            }

            users.Add(user);
        }

        return users;
    }

    private AsyncRelayCommand AddAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (!await _isActionSemaphore.WaitAsync(0))
            {
                ShowError("Wait for the previous command to complete");
                return;
            }

            try
            {
                if (!User.GetInstance().IsTopLevelAdmin())
                    return;

                if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
                {
                    var lobbies = await databaseService.GetLobbyById(userInfo.LobbyId, _cancellationTokenSource.Token);

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

                    var isUpdating = await databaseService.UpdateAsync(lobbies, _cancellationTokenSource.Token);

                    if (!isUpdating)
                        Logger.Error("Failed to add admin");
                    else
                        Logger.Info("Admin is added");
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
            }
        });
    }

    private AsyncRelayCommand RemoveAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (!await _isActionSemaphore.WaitAsync(0))
            {
                ShowError("Wait for the previous command to complete");
                return;
            }

            try
            {
                if (!User.GetInstance().IsTopLevelAdmin())
                    return;

                if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
                {
                    var isRemoving =
                        await databaseService.DeleteItemWithPredicate<Admins>(e => e.SteamId == userInfo.SteamId,
                            _cancellationTokenSource.Token);

                    if (!isRemoving)
                    {
                        Logger.Error("Failed to remove admin");
                    }
                }
                else
                    throw new NullReferenceException("Failed resolve database service");
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message);
            }
            finally
            {
                _isActionSemaphore.Release();
            }
        });
    }

    private void ShowError(string message)
    {
        if (Di.Container.GetInstance<ErrorService>() is not ErrorService errorService)
            throw new NullReferenceException(nameof(ErrorService));

        errorService.ShowWindow(new ErrorStruct { ErrorMessage = message, ErrorType = ErrorEnum.Error });
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _semaphoreSlim.Release();

        _databaseService = null;

        _postgresListener?.Unsubscribe(TableEnum.AdminTable, _loadAdminTable);
        _postgresListener = null;

        _admins.Clear();
        Admins.Clear();
    }
}

public class AdminRegistrationData(Users userInfo, string buttonText, AsyncRelayCommand buttonCommand, bool isAdmin)
{
    public Users UserInfo { get; private set; } = userInfo;
    public string ButtonText { get; private set; } = buttonText;
    public AsyncRelayCommand ButtonCommand { get; private set; } = buttonCommand;

    public bool IsAdmin { get; private set; } = isAdmin;
}