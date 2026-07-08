using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameRandom.DbContext;
using GameRandom.Scripts.Service;
using GameRandom.Scripts.UserData;
using GameRandom.ViewModels.TablesSystem.Abstract;

namespace GameRandom.ViewModels.TablesSystem;

/// <summary>
/// ViewModel for managing game progress table data with lobby-based filtering.
/// </summary>
public class GameTableViewModel : AbstractTableWindowViewModel<GameTableData>
{
    /// <summary>
    /// Loads game progress data for all players in the current user's lobby.
    /// </summary>
    /// <param name="predicate">Optional filter predicate (not used in current implementation).</param>
    public override async Task LoadData(Func<GameTableData, bool>? predicate = null)
    {
        if (IsNotValidateInjectingData()) throw new NullReferenceException();
        
        var userData = User.GetInstance().GetUserInfo();

        StartTaskWaiter();

        try
        {
            var lobbies = await _databaseService.GetLobbyById(userData.LobbyId);

            if (lobbies is null || lobbies.LobbyData.Count <= 0)
                return;

            var gameTable = await LoadGroupTableData(lobbies.LobbyData);

            if (gameTable is null) return;

            TableData = new ObservableCollection<GameTableData>(gameTable);
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
        }
        finally
        {
            CloseTaskWaiter();
        }
    }
    
    /// <summary>
    /// Loads game progress data for each player in the lobby.
    /// </summary>
    /// <param name="lobbyData">List of lobby members.</param>
    /// <returns>List of game table data entries or null if no data found.</returns>
    private async Task<List<GameTableData>?> LoadGroupTableData(List<LobbyData> lobbyData)
    {
        List<GameTableData> gameTableData = new();

        foreach (var data in lobbyData)
        {
            var (user, gameProgresses) 
                = await _databaseService.GetGameTableData(data.UserId);
            
            if (user is null || gameProgresses is null)
                continue;
            
            foreach (var game in gameProgresses)
            {
                var userNickName = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : "---";
                
                gameTableData.Add(new GameTableData
                {
                    PlayerName = userNickName,
                    GameInfo = game
                });
            }
        }

        return gameTableData;
    }
}

/// <summary>
/// Data model representing a single row in the game progress table.
/// </summary>
public class GameTableData
{
    /// <summary>
    /// Gets or sets the player's display name.
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the game progress information.
    /// </summary>
    public GameProgresses GameInfo { get; set; } = null!;
}