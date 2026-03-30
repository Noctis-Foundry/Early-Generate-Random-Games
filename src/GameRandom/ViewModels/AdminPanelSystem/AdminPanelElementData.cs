using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;

namespace GameRandom.ViewModels.AdminPanelSystem;

/// <summary>
/// Data structure representing a single element in the admin panel's game list.
/// </summary>
/// <param name="gameInfo">Information about the finished game.</param>
/// <param name="openCommand">Command to open the confirmation window.</param>
/// <param name="nickname">The player's nickname.</param>
public class AdminPanelElementData(FinishedGames gameInfo, RelayCommand openCommand, string nickname)
{
    /// <summary>
    /// Gets information about the finished game.
    /// </summary>
    public FinishedGames GameInfo { get; private set; } = gameInfo;

    /// <summary>
    /// Gets the command to open the game confirmation window.
    /// </summary>
    public RelayCommand OpenCommand { get; private set; } = openCommand;

    /// <summary>
    /// Gets the player's nickname.
    /// </summary>
    public string Nickname { get; private set; } = nickname;
}