using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Events;
using GameRandom.Scr.Service;
using GameRandom.Scr.WindowScr;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.SteamSDK.UserSystem;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;
using Microsoft.Extensions.Logging;
using Steamworks;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Inject] private MainWindowFactory? _mainWindowFactory;
    [Inject] private SteamWebApi? _steamWebApi;
    [Inject] private UserData? _userData;
    [Inject] private DatabaseService? _databaseService;

    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public ICommand CreateLobbyCommand { get; }
    
    private Dictionary<ulong, Image> _avatars = new();

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
        CreateLobbyCommand = new RelayCommand(OpenCreateLobbyWindow);

        Di.Container.ResolveFieldsFromClassInstance(this);
        
        if (_steamWebApi == null)
            throw new NotImplementedException("_steamWebApi is not implemented");
        if (_mainWindowFactory == null)
            throw new NotImplementedException("_mainWindowFactory is not implemented");
        if (_userData == null)
            throw new NotImplementedException("_userData is not implemented");
        if (_databaseService == null)
            throw new NotImplementedException("_databaseService is not implemented");
    }

    public async Task UpdateLobby(Grid lobbyGrid, PayloadStructure payloadStructure)
    {
        if (_userData.LobbyId == 0)
        {
            Logger.Debug("Player lobby not found");
            return;
        }
        
        lobbyGrid.Children.Clear();
        _avatars.Clear();

        if ((TableEnum)payloadStructure.TableCode != TableEnum.LobbyContext)
        {
            Logger.Debug($"Table code {payloadStructure.TableCode} not correct for this method");
            return;
        }

        List<LobbyUserContext> lobbyContexts =
            await _databaseService.Where<LobbyUserContext>(e => e.LobbyID == _userData.LobbyId);

        if (lobbyContexts.Count == 0)
        {
            Logger.Debug($"No lobby context found with {_userData.LobbyId}");
            return;
        }
    }

    public async void OpenLobby()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }

    public async void OpenCreateLobbyWindow()
    {
        await _windowService.ShowDialogAsync<CreateLobby>();
    }

    public async void ShowRules()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }

    public void ShowError()
    {
        IError? error = Di.Container.TryGetInstance<IError>() as ErrorService;

        if (error != null)
        {
            error.ShowErrorWindow("Open error modal");
        }
        else
        {
            throw new Exception("Not fount error modal");
        }
    }
    
    private async Task CreateAvatarsUi(List<LobbyUserContext> usersContext, Grid lobbyGrid)
    {
        for (int i = 0; i < usersContext.Count; i++)
        {
            ulong memberId = usersContext[i].MemberID;
            
            Image avatar = _mainWindowFactory.CreateImageInGrid(lobbyGrid, i);
            Logger.Debug($"Current request user id {memberId}");
            
            if (!_avatars.TryAdd(memberId, avatar))
            {
                Logger.Error($"Failed to add avatar {memberId}. Avatar already exists");
            }

            await LoadAvatars(usersContext[i].MemberID);
        }
    }

    private async Task LoadAvatars(ulong id)
    {
        Logger.Debug("Current on persona state: " + id);
        
        if (_avatars.TryGetValue(id, out var image))
        {
            var userData = await _steamWebApi.GetUserData(id);

            if (userData == null)
            {
                Logger.Error("Steam not callback player data");
                return;
            }
            
            Bitmap? avatar = await SteamService.Instance.GetImage(userData.avatarId);
            image.Source = avatar;
        }
    }
}