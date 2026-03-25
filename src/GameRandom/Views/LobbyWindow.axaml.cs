using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GameRandom.Scr.Events;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.LobbySystem;
using GameRandom.ViewModels.AdminSystem;

namespace GameRandom.Views.LobbyModalWindow;

public sealed partial class LobbyWindow : WindowBase<LobbyWindowViewModel>
{
    private const int MaxLenghtId = 18;
    [Inject] private EventBus _eventBus = null!;
    [Inject] private LobbyService _lobbyService = null!;
    [Inject] private ErrorService _errorService = null!;

    public LobbyWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;
        
        InitializeViewModel();
        InitializeDiContainer();
        InitializeProcessingHandler();
    }

    private async void Connect(object? sender, RoutedEventArgs e)
    {
        if (long.TryParse(IdBox.Text, out var lobbyId))
        {
            await _lobbyService.ConnectToLobby(lobbyId);
        }
        else
            ShowMessage("Failed connect to lobby. Not corrent id");
    }

    private async void Create(object? sender, RoutedEventArgs e)
    {
        await _lobbyService.CreateLobby();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (IsClosing)
            return;

        IsClosing = true;
        
        Hide();

        IsActive = false;
        
        e.Cancel = true;
    }

    protected override void InitializeDiContainer()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);

        _eventBus = _eventBus ?? throw new InvalidOperationException("EventBus is null");
        _lobbyService = _lobbyService ?? throw new InvalidOperationException("LobbyService is null");
        _errorService = _errorService ?? throw new InvalidOperationException("ErrorService is null");
    }
}