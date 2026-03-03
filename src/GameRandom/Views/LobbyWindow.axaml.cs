using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;
using GameRandom.SteamSDK.Enums;
using GameRandom.SteamSDK.LobbySystem;
using GameRandom.ViewModels;

namespace GameRandom.Views.LobbyModalWindow;

public partial class LobbyWindow : WindowAbstract
{
    private const int MaxLenghtId = 18;
    [Inject] private readonly EventBus _eventBus = null!;
    [Inject] private readonly LobbyService _lobbyService = null!;
    [Inject] private readonly ErrorService _errorService = null!;

    public LobbyWindow()
    {
        Console.WriteLine("Initialize Create Lobby");

        InitializeComponent();

        var viewModel = new CreateLobbyViewModel();
        DataContext = viewModel;

        Di.Container.RegisterSingleInstance(viewModel);
        Di.Container.ResolveFieldsFromClassInstance(this);

        _eventBus = _eventBus ?? throw new InvalidOperationException("EventBus is null");
        _lobbyService = _lobbyService ?? throw new InvalidOperationException("LobbyService is null");
        _errorService = _errorService ?? throw new InvalidOperationException("ErrorService is null");
    }

    private async void Connect(object? sender, RoutedEventArgs e)
    {
        if (long.TryParse(IdBox.Text, out var lobbyId))
        {
            await _lobbyService.ConnectToLobby(lobbyId);
        }
        else
            _errorService.ShowErrorWindow("Failed connect to the lobby", ErrorEnum.Error);
    }

    private async void Create(object? sender, RoutedEventArgs e)
    {
        await _lobbyService.CreateLobby();
    }
}