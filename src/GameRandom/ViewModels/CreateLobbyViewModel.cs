using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK;

namespace GameRandom.ViewModels;

public class CreateLobbyViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService = null!;
    
    private const string DefaultIdMessage = "No find lobby id";

    private string _currentLobbyId;
    
    public string CurrentLobbyID
    {
        get => _currentLobbyId;
        set => SetProperty(ref _currentLobbyId, value);
    }

    public CreateLobbyViewModel()
    {
        Di.Container.ResolveFieldsFromClassInstance(this);
        GetCurrentId();
    }
    
    private void GetCurrentId()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_databaseService is null)
                return;

            Users? user = await _databaseService.GetUserByUlongId(SteamManager.GetSteamIdAsLong());

            if (user is null)
                throw new ArgumentNullException(nameof(user));

            CurrentLobbyID = user.LobbyID > 0 ? user.LobbyID.ToString() : DefaultIdMessage;
        });
    }
}