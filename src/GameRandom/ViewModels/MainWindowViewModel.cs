using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
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
    [Inject] private DatabaseService? _databaseService;
    [Inject] private MainWindowFactory? _mainWindowFactory;
    [Inject] private UserData? _userData;
    [Inject] private ErrorService? _error;

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
    }

    public void UpdateLobby(Grid lobbyGrid, List<LobbyUserContext>? lobbyContext)
    {
        lobbyGrid.Children.Clear();
        _avatars.Clear();

        if (lobbyContext == null || lobbyContext.Count == 0)
        {
            Logger.Warning("No lobby context found");
            return;
        }

        CreateAvatarsUi(lobbyContext, lobbyGrid);
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
    

    private void CreateAvatarsUi(List<LobbyUserContext> usersContext, Grid lobbyGrid)
    {
        if (_mainWindowFactory == null)
            throw new NullReferenceException();

        for (int i = 0; i < usersContext.Count; i++)
        {
            Image avatar = _mainWindowFactory.CreateImageInGrid(lobbyGrid, i);
            Logger.Debug($"Current request user id {usersContext[i].MemberID}");
            if (!_avatars.TryAdd(usersContext[i].MemberID, avatar))
            {
                Logger.Error($"Failed to add avatar {usersContext[i].MemberID}. Avatar already exists");
            }
        }
    }

    private async Task LoadAvatars(CSteamID id)
    {
        Logger.Debug("Current on persona state: " + id.m_SteamID);
        
        if (_avatars.TryGetValue(id.m_SteamID, out var image))
        {
            int avatar = SteamFriends.GetLargeFriendAvatar(id);

            if (avatar == 0)
                return;
            
            Bitmap? bitmap = AvaloniaService.CreateSteamImage(avatar);

            if (bitmap != null)
            {
                image.Source = bitmap;
            }
        }
    }
}