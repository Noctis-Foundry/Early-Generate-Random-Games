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
using GameRandom.SteamSDK.UserSystem;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Inject] private readonly MainWindowFactory _mainWindowFactory = null!;
    [Inject] private readonly SteamWebApi _steamWebApi = null!;
    [Inject] private readonly UserData _userData = null!;
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

        if (_userData.LobbyId == 0)
        {
            _errorService.ShowErrorWindow("Not fount user data", ErrorEnum.Warning);
            return;
        }

        lobbyGrid.Children.Clear();
        _avatars.Clear();

        if ((TableEnum)tableCode != TableEnum.LobbyContext)
        {
            _errorService.ShowErrorWindow($"Table code {tableCode} not correct for this method", ErrorEnum.Error);
            return;
        }

        var lobbyContexts =
            await _databaseService.Where<LobbyUserContext>(e => e.LobbyID == _userData.LobbyId);

        if (lobbyContexts == null || lobbyContexts.Count == 0)
        {
            _errorService.ShowErrorWindow($"No lobby context found with {_userData.LobbyId}", ErrorEnum.Error);
            return;
        }

        var images = _mainWindowFactory.CreateImagesInGrid(lobbyContexts.Count, lobbyGrid);

        for (int i = 0; i < lobbyContexts.Count; i++)
        {
            try
            {
                var profileContext = await _steamWebApi.GetUserData(lobbyContexts[i].MemberID);

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

            Bitmap? avatar = await SteamService.Instance.GetImage(userData.avatarUrl);
            image.Source = avatar;
        }
    }
}