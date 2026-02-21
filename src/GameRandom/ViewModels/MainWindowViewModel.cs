using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Scr.WindowScr;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.UserData;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Inject] private readonly MainWindowFactory _mainWindowFactory = null!;
    [Inject] private readonly SteamWebApi _steamWebApi = null!;
    [Inject] private readonly DatabaseService _databaseService = null!;
    [Inject] private readonly ErrorService _errorService = null!;

    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public ICommand CreateLobbyCommand { get; }

    private Dictionary<ulong, Image> _avatars = new();
    private bool _isInitialized;

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
        CreateLobbyCommand = new RelayCommand(OpenCreateLobbyWindow);

        Di.Container.ResolveFieldsFromClassInstance(this);

        _isInitialized = true;
    }

    public async Task UpdateLobby(Grid lobbyGrid, int tableCode)
    {
        if (!_isInitialized)
        {
            _errorService.ShowErrorWindow("Not initialized MainWindowViewModel, Cant update lobby", ErrorEnum.Error);
            return;
        }
        
        lobbyGrid.Children.Clear();
        _avatars.Clear();

        if ((TableEnum)tableCode != TableEnum.Lobby)
        {
            _errorService.ShowErrorWindow($"Table code {tableCode} not correct for this method", ErrorEnum.Error);
            return;
        }

        var userData = await User.GetInstance().GetUserInfo();

        if (userData is null)
        {
            _errorService.ShowErrorWindow("Not find user in database", ErrorEnum.Error);
            return;
        }

        var lobbyContexts = await _databaseService.GetLobbyById(userData.LobbyID);

        if (lobbyContexts == null || lobbyContexts.LobbyData.Count <= 0)
        {
            _errorService.ShowErrorWindow($"No lobby context found with {userData.LobbyID}", ErrorEnum.Error);
            return;
        }
        
        var lobbyData = lobbyContexts.LobbyData;

        var images = _mainWindowFactory.CreateImagesInGrid(lobbyData.Count, lobbyGrid);

        for (int i = 0; i < lobbyData.Count; i++)
        {
            try
            {
                var profileContext = await _steamWebApi.GetUserData(lobbyData[i].UserId);

                if (profileContext == null)
                {
                    _errorService.ShowErrorWindow("Not found profile context", ErrorEnum.Error);
                    return;
                }
                
                var bitmap = await SteamService.Instance.GetImage(profileContext.avatarUrl);
                images[i].Source = bitmap;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }

    public async void OpenLobby()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }

    public async void OpenCreateLobbyWindow()
    {
        await _windowService.ShowDialogAsync<Rules>();
    }

    public void ShowError()
    {
        _errorService.ShowErrorWindow("Open error modal", ErrorEnum.Warning);
    }
}