using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.Service;
using GameRandom.SteamSDK.UserData;

namespace GameRandom.ViewModels;

public class GameTableViewModel : AbstractTableWindowViewModel<GameTableData>
{
    public override async Task LoadData(Func<GameTableData, bool>? predicate = null)
    {
        if (IsNotValidateInjectingData()) throw new NullReferenceException();
        
        var _userData = User.GetInstance().GetUserInfo();
        
        try
        {
            var lobbies = await _databaseService.GetLobbyById(_userData.LobbyId);

            if (lobbies is null || lobbies.LobbyData.Count <= 0)
                return;

            var gameTable = await LoadGroupTableData(lobbies.LobbyData);
            
            if (gameTable is null) return;
            
            TableData = new ObservableCollection<GameTableData>(gameTable);
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return;
        }
    }

    private async Task<List<GameTableData>?> LoadGroupTableData(List<LobbyData> lobbyData)
    {
        List<GameTableData> gameTableData = new();

        foreach (var data in lobbyData)
        {
            var user = await _databaseService.GetUserByUlongId(data.UserId);
            
            if (user is null) continue;
            
            var playerGames = await _databaseService.Where<GameProgresses>(e => e.PlayerId == user.SteamId);

            if (playerGames is null) continue;
            
            foreach (var game in playerGames)
            {
                gameTableData.Add(new GameTableData
                {
                    PlayerName = user.Nickname,
                    GameInfo = game
                });
            }
        }

        return gameTableData;
    }
}

public class GameTableData
{
    public string PlayerName { get; set; } = string.Empty;
    public GameProgresses GameInfo { get; set; } = null!;
}