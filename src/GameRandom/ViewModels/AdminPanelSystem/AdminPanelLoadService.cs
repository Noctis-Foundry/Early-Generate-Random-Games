using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminSystem.Interface;

namespace GameRandom.ViewModels.AdminPanelSystem;

public class AdminPanelLoadService : BaseModelService, IAdminLoad
{
    private AdminPanelFactory _adminPanelFactory = new AdminPanelFactory();

    /// <summary>
    /// Asynchronously loads game progresses from the database and updates the <see cref="GameList"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<List<AdminPanelElementData>> LoadElementsData()
    {
        var finallyList = new List<AdminPanelElementData>();

        using var cts = new CancellationTokenSource();

        if (await GetFinishedGame(cts.Token) is not { } gameList)
            return null!;

        foreach (var game in gameList)
        {
            if (!IterationRequired(game)) //Check game in null
                continue;

            var user = await DatabaseService.GetUserByUlongId(game.GameProgresses.PlayerId, cts.Token);

            if (user is null) continue;

            if (_adminPanelFactory.CreateAdminPanelElement(user, game) is { } adminPanelData)
                finallyList.Add(adminPanelData);
        }

        return finallyList;
    }

    /// <summary>
    /// Retrieves the list of finished games from the database.
    /// </summary>
    /// <returns>A task that returns a list of finished games, or null if loading fails.</returns>
    private async Task<List<FinishedGames>?> GetFinishedGame(CancellationToken cts = default)
    {
        var gameList = await DatabaseService.GetFinishedGames(cts);

        if (gameList is null)
        {
            Logger.Error("Failed to load game progresses from database");
            return null;
        }

        return gameList;
    }

    /// <summary>
    /// Determines if a game iteration is required based on its status.
    /// </summary>
    /// <param name="game">The finished game to check.</param>
    /// <returns>True if the game should be processed; otherwise, false.</returns>
    private bool IterationRequired(FinishedGames game)
    {
        if (game.IsImprove)
            return false;

        if (game.GameProgresses is null || game.GameProgresses.PlayerId == 0)
            return false;

        return true;
    }

    public override void Dispose()
    {
        _adminPanelFactory = null!;
        
        base.Dispose();
    }
}