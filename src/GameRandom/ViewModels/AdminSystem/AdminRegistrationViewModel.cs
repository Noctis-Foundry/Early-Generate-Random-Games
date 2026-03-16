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
    
    private ObservableCollection<AdminRegistrationData> _admins;
    public ObservableCollection<AdminRegistrationData> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }

    public async Task LoadData()
    {
        Di.Container.ResolveField(out _databaseService);
        
        if (_databaseService == null)
            throw new NullReferenceException(nameof(_databaseService));
        
        var userInfo = User.GetInstance().GetUserInfo();
        
        if (userInfo.LobbyId <= 0)
            return;

        var lobbies = await _databaseService.GetLobbyById(userInfo.LobbyId);
        
        if (lobbies == null)
            throw new Exception("Lobby not found");

        foreach (var user in await NotAdminUsers(lobbies))
        {
            Admins.Add(new AdminRegistrationData(user));
        }
    }

    public async Task<List<Users>> NotAdminUsers(Lobbies lobbies)
    {
        var users = new List<Users>();

        foreach (var lobby in lobbies.LobbyData)
        {
            if (lobbies.AdminsList.Exists(e => e.SteamId == lobby.UserId))
                continue;

            var user = await _databaseService.GetUserByUlongId(lobby.UserId);
        }

        return users;
    }
}

public class AdminRegistrationData(Users userInfo)
{
    public Users UserInfo { get; private set; } = userInfo;
    
    public void ButtonAction(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (Di.Container.TryGetInstance<DatabaseService>() is DatabaseService databaseService)
            {
                var isAdded = await databaseService.AddItemAsync(new Admins
                {
                    SteamId = UserInfo.SteamId,
                    LobbyId = UserInfo.LobbyId,
                    IsTopAdmin = false
                }, cancellationToken.Token);

                if (!isAdded)
                {
                    Logger.Error("Failed to add admin");
                    return;
                }
            }
            else
                throw new NullReferenceException("Failed resolve database service");
        });
    }
}