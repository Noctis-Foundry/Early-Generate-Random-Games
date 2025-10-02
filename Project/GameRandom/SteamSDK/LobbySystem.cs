using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameRandom.DataBaseContexts;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.SteamSDK;

public class LobbySystem
{
    private const int MemberToGroup = 6;
    
    private Callback<LobbyCreated_t> _lobbyCreated;
    private TaskCompletionSource<List<LobbyContext>> _lobbyCreatedTcs;
    
    public async Task<List<LobbyContext>> CreateLobby()
    {
        await using var database = new AppDbContext();
        
        var members = await database.LobbyContexts.ToListAsync();

        if (members.Count > 0)
        {
            return members;
        }
        
        _lobbyCreatedTcs = new TaskCompletionSource<List<LobbyContext>>();
        
        _lobbyCreated = new Callback<LobbyCreated_t>(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MemberToGroup);
        
        return await _lobbyCreatedTcs.Task;
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Console.WriteLine("Lobby creation failed");
            return;
        }

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        ulong userId = SteamUser.GetSteamID().m_SteamID;
        string nickname = SteamFriends.GetPersonaName();
        
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                Console.WriteLine($"Start added data to database. {lobbyId}, {userId}, {nickname}");
                
                await using var db = new AppDbContext();
                var lobby = new LobbyContext
                {
                    LobbyID = lobbyId.m_SteamID,
                    MemberID = userId,
                    NickName = nickname
                };
                
                await db.LobbyContexts.AddAsync(lobby);
                await db.SaveChangesAsync();
                
                _lobbyCreatedTcs.SetResult(await db.LobbyContexts.ToListAsync());
            }
            catch (Exception e)
            {
                _lobbyCreatedTcs.SetException(e);
            }
        });
    }
    
    public void MemberToJoin(ulong lobbyId, ulong userId)
    {
        //Логика обработки коннекта игрока
    }

    public void InvateToLobby()
    {
        
    }
}