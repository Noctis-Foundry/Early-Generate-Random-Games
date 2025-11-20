using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Scr.WindowScr;
using GameRandom.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.UserSystem;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    [Inject] private MainWindowFactory? _mainWindowFactory;
    [Inject] private UserData?  _userData;
    [Inject] private ErrorService? _error;
    
    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public ICommand CreateLobbyCommand { get; }

    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
        CreateLobbyCommand = new RelayCommand(OpenCreateLobbyWindow);
        
        Di.Container.ResolveFieldsFromClassInstance(this);
    }

    public async Task UpdateLobby(Grid lobbyGrid)
    {
        lobbyGrid.Children.Clear();

        if (_databaseService == null || _mainWindowFactory == null || _userData == null)
            throw new NullReferenceException();
        
        //List<Image>? imageList = _mainWindowFactory.CreateImageInGrid(currentLobbyList.Count, lobbyGrid);
        
        //for (int i = 0; i < currentLobbyList.Count; i++)
        //{
            //CSteamID memberId = new CSteamID(currentLobbyList[i].ClientID);
            //int imageUrl = SteamFriends.GetLargeFriendAvatar(memberId);
            //Bitmap bitmap = AvaloniaService.CreateSteamImage(imageUrl);
            //imageList[i].Source = bitmap;   
        //}
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
}