using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels.AdminSystem;

public class AdminRegistrationViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    [Inject] private PostgresListener? _postgresListener;
    
    private ObservableCollection<AdminRegistrationData> _admins;
    public ObservableCollection<AdminRegistrationData> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }
    
    private SemaphoreSlim _semaphoreSlim = new(1, 1);
    
    private const string AddAdmin = "Add admin";
    private const string RemoveAdmin = "Remove admin";

    private Lobbies _currentLobby;

    public AdminRegistrationViewModel()
    {
        Admins = new ObservableCollection<AdminRegistrationData>();
        
        Di.Container.ResolveFieldsFromClassInstance(this);

        if (_databaseService == null)
            throw new NullReferenceException(nameof(_databaseService));
        if (_postgresListener == null)
            throw new NullReferenceException(nameof(_postgresListener));
        
        Dispatcher.UIThread.InvokeAsync(async () => await LoadData());
        
        _postgresListener.Subscribe(TableEnum.AdminTable, e =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await UpdateData(e);
            });
        });
    }
    
    private async Task LoadData()
    {
        if (!await _semaphoreSlim.WaitAsync(0))
        {
            Logger.Info("Admin is loading");
            return;
        }
        
        try
        {
            Admins.Clear();
            var userInfo = User.GetInstance().GetUserInfo();

            if (userInfo.LobbyId <= 0)
                return;

            _currentLobby = await _databaseService.GetLobbyById(userInfo.LobbyId);

            if (_currentLobby == null)
                throw new Exception("Lobby not found");

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

    private async Task UpdateData(PayloadStructure payloadStructure)
    {
        if (payloadStructure.TableCode != (int)TableEnum.AdminTable)
            return;
        
        await LoadData();
    }

    private async Task<List<Users>> NotAdminUsers(Lobbies lobbies)
    {
        var users = new List<Users>();

        foreach (var lobby in lobbies.LobbyData)
        {
            var user = await _databaseService.GetUserByUlongId(lobby.UserId);
            if (user is null) continue;
            
            if (lobbies.AdminsList.Exists(e => e.SteamId == lobby.UserId))
            {
                var admin = lobbies.AdminsList.Find(e => e.SteamId == lobby.UserId);
                
                if (admin is null || admin.IsTopAdmin)
                    continue;
                
                Admins.Add(new AdminRegistrationData(user, RemoveAdmin, RemoveAdminCommand(user, admin.IsTopAdmin), true));
                
                continue;
            }
            
            users.Add(user);
        }

        return users;
    }

    private AsyncRelayCommand AddAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand( async () => 
        {
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
            {
                var lobbies = await databaseService.GetLobbyById(userInfo.LobbyId, cancellationToken.Token);

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

                Logger.Info($"Add admin command: Lobby hash code = {lobbies.GetHashCode()}");
                
                var isUpdating = await databaseService.UpdateAsync(lobbies, cancellationToken.Token);

                if (!isUpdating)
                    Logger.Error("Failed to add admin");
                else
                    Logger.Info("Admin is added");
            }
            else
                throw new NullReferenceException("Failed resolve database service");
        });
    }

    private AsyncRelayCommand RemoveAdminCommand(Users userInfo, bool isTopAdmin)
    {
        if (isTopAdmin)
            return null!;

        return new AsyncRelayCommand(async () =>
        {
            using var cancellationToken = new CancellationTokenSource();

            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
            {
                var isRemoving =
                    await databaseService.DeleteItemWithPredicate<Admins>(e => e.SteamId == userInfo.SteamId,
                        cancellationToken.Token);

                if (!isRemoving)
                {
                    Logger.Error("Failed to remove admin");
                    return;
                }
            }
            else
                throw new NullReferenceException("Failed resolve database service");
        });
    }
}

public class AdminRegistrationData(Users userInfo, string buttonText, AsyncRelayCommand buttonCommand, bool isAdmin)
{
    public Users UserInfo { get; private set; } = userInfo;
    public string ButtonText { get; private set; } = buttonText;
    public AsyncRelayCommand ButtonCommand { get; private set; } = buttonCommand;
    
    public bool IsAdmin { get; private set; } = isAdmin;
}