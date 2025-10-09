using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.WindowScr;
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

    private async void OpenLobby()
    {
        await _windowService.ShowDialogAsync<LobbyWindow>();
    }
}