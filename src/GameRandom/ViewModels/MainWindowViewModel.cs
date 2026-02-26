using System;
using System.Collections.Generic;
using System.Threading;
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

    private Rules _rules = new();
    
    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public ICommand RulesOpen { get; }

    private Dictionary<ulong, Image> _avatars = new();
    private bool _isInitialized;
    
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
        RulesOpen = new RelayCommand(OpenRules);

        Di.Container.ResolveFieldsFromClassInstance(this);

        _isInitialized = true;
    }

    public async Task UpdateLobby(Grid lobbyGrid, int tableCode)
    {
        await _semaphore.WaitAsync();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
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

        var userData = User.GetInstance().GetUserInfo();

        var lobbyContexts = await _databaseService.GetLobbyById(userData.LobbyId, cts.Token);

        if (lobbyContexts == null || lobbyContexts.LobbyData.Count <= 0)
        {
            _errorService.ShowErrorWindow($"No lobby context found with {userData.LobbyId}", ErrorEnum.Error);
            return;
        }
        
        var lobbyData = lobbyContexts.LobbyData;

        var images = _mainWindowFactory.CreateImageInGrid(lobbyGrid, lobbyData.Count);

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

    public  void OpenRules()
    {
       _rules.Open();
    }

    public void ShowError()
    {
        _errorService.ShowErrorWindow("Open error modal", ErrorEnum.Warning);
    }
}