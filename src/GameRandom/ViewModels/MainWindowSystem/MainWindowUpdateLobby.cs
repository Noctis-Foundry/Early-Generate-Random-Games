using System;
using GameRandom.DependenceInjectSystem;
using System.Collections.Generic;
using GameRandom.DependenceInjectSystem;
using System.Threading;
using GameRandom.DependenceInjectSystem;
using System.Threading.Tasks;
using GameRandom.DependenceInjectSystem;
using GameRandom.DataBaseContexts;
using GameRandom.DependenceInjectSystem;
using GameRandom.DependenceInjectSystem.DiSystem;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scr.Service;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.Enums;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.SteamsContexts;
using GameRandom.DependenceInjectSystem;
using GameRandom.Src.UserData;
using GameRandom.DependenceInjectSystem;
using GameRandom.Scripts.SteamSDK;
using GameRandom.Scripts.WindowServices.ErrorServiceSystem;

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class MainWindowUpdateLobby : BaseModelService, ILobbyUpdate
{
    [Inject] private ISteamWebService _steamWebApi = null!;
    
    private const int LobbyUpdateTimeout = 3;

    public MainWindowUpdateLobby()
    {
        if (_steamWebApi is null) //Base model service injecting base property.
            throw new NullReferenceException(nameof(_steamWebApi));
    }
    public async Task<List<ProfileContext>> UpdateLobby()
    {

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(LobbyUpdateTimeout));
        var userData = User.GetInstance().GetUserInfo();

        if (await GetLobby(userData.LobbyId, cts.Token) is not { } lobbyContexts)
            return null!;

        return await LoadLobbyProfiles(lobbyContexts.LobbyData);
    }
    /// <summary>
    /// Loads profile information for all users in the lobby.
    /// </summary>
    /// <param name="lobbyData">List of lobby users to load profiles for.</param>
    private async Task<List<ProfileContext>> LoadLobbyProfiles(List<LobbyData> lobbyData)
    {
        var users = new List<ProfileContext>();
        
        foreach (var lobbyUser in lobbyData)
        {
            var profileContext = await _steamWebApi.GetProfile(lobbyUser.UserId);
            
            if (profileContext == null)
            {
                Logger.Error("Not find profile context");
                return null!;
            }

            users.Add(profileContext);
        }

        return users;
    }
    /// <summary>
    /// Retrieves lobby information from the database.
    /// </summary>
    /// <param name="lobbyId">Lobby identifier.</param>
    /// <param name="cts">Cancellation token for the operation.</param>
    /// <returns>Lobby context if found; otherwise, null.</returns>
    private async Task<Lobbies?> GetLobby(long lobbyId, CancellationToken cts)
    {
        var lobbyContexts = await DatabaseService.GetLobbyById(lobbyId, cts);

        if (lobbyContexts == null || lobbyContexts.LobbyData.Count <= 0)
        {
            ErrorService.ShowWindow(new ErrorStruct
                { ErrorMessage = $"No lobby context found with {lobbyId}", ErrorType = ErrorEnum.Error });
            return null;
        }

        return lobbyContexts;
    }
    public override void Dispose()
    {
        _steamWebApi = null!;
        
        base.Dispose();
    }
}