using System;
using Avalonia.Controls;
using Avalonia.Threading;
using GameRandom.DISystem;
using GameRandom.Scripts.HandleSystem.HandleEvents;
using GameRandom.Scripts.LobbySystem;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.BaseClasses;

namespace GameRandom.ViewModels.LobbySystem;

public sealed class LobbyWindowViewModel : ViewModelBase
{
    [Inject] private EventBus _eventBus = null!;
    [Inject] private LobbyService _lobbyService = null!;

    private const long DefaultIdMessage = 0;

    private long _currentLobbyId;

    public long CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public LobbyWindowViewModel()
    {
        if (Design.IsDesignMode)
            return;

        InitializeDiContainer();

        GetCurrentId();
        
        _eventBus.Subscribe<LobbyEvent>(e => GetCurrentId());
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_eventBus is null)
            throw new NullReferenceException(nameof(_eventBus));
        if (_lobbyService is null)
            throw new NullReferenceException(nameof(_lobbyService));
    }

    public void ConnectToLobby(string id)
    {
        StartTaskWaiter();
        
        try
        {
            if (long.TryParse(id, out var lobbyId))
            {
                Dispatcher.UIThread.InvokeAsync(async () => await _lobbyService.ConnectToLobby(lobbyId));
            }
            else
                ErrorService.ShowWindow("Failed connect to lobby. Not correct id");
        }
        finally
        {
            CloseTaskWaiter();
        }
    }
    
    public void CreateNewLobby()
    {
        StartTaskWaiter();

        try
        {
            Dispatcher.UIThread.InvokeAsync(async () => await _lobbyService.CreateLobby());
        }
        finally
        {
            CloseTaskWaiter();
        }
    }

    private void GetCurrentId()
    {
        var userInfo = User.GetInstance().GetUserInfo();
        CurrentLobbyID = userInfo.LobbyId > 0 ? userInfo.LobbyId : DefaultIdMessage;
    }
}