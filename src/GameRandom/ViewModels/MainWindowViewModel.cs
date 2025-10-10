using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.WindowScr;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.DI;
using GameRandom.Views;
using GameRandom.Views.LobbyModalWindow;

namespace GameRandom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    public ICommand OpenLobbyCommand { get; }
    public MainWindowViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        OpenLobbyCommand = new RelayCommand(OpenLobby);
    }

    public async void OpenLobby()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }

    public void ShowError()
    {
        IError? error = Di.Container.TryGetInstance(typeof(ErrorService)) as ErrorService;

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