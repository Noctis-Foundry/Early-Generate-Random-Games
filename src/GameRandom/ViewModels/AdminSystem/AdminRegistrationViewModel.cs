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
    
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    
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
        
        if (!User.GetInstance().IsAdmin)
            return;
        
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
        
        if (User.GetInstance().IsAdmin || User.GetInstance().IsTopLevelAdmin)
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
        return new AsyncRelayCommand( async () => 
        {
            if (!IsHaveRules())
                return;
            
            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
            {
                try
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

                    Logger.Info($"Add admin command: Lobby hash code = {lobbies.GetHashCode()}");

                    var isUpdating = await databaseService.UpdateAsync(lobbies, _cancellationTokenSource.Token);

                    if (!isUpdating)
                        Logger.Error("Failed to add admin");
                    else
                        Logger.Info("Admin is added");
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    return;
                }
            }
            else
                throw new NullReferenceException("Failed resolve database service");
        });
    }

    private AsyncRelayCommand RemoveAdminCommand(Users userInfo)
    {
        return new AsyncRelayCommand(async () =>
        {
            if (!IsHaveRules())
                return;

            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
            {
                var isRemoving =
                    await databaseService.DeleteItemWithPredicate<Admins>(e => e.SteamId == userInfo.SteamId,
                        _cancellationTokenSource.Token);

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

    private bool IsHaveRules()
    {
        return User.GetInstance().IsAdmin || User.GetInstance().IsTopLevelAdmin;
    }
    
    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        
        base.Dispose();
    }
}

public class AdminRegistrationData(Users userInfo, string buttonText, AsyncRelayCommand buttonCommand, bool isAdmin)
{
    public Users UserInfo { get; private set; } = userInfo;
    public string ButtonText { get; private set; } = buttonText;
    public AsyncRelayCommand ButtonCommand { get; private set; } = buttonCommand;
    
    public bool IsAdmin { get; private set; } = isAdmin;
}