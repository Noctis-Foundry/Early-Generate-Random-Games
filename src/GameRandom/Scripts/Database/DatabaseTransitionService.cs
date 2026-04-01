using System;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.Scr.Service;

public class DatabaseTransitionService : DependenceBase
{
    [Inject] private DatabaseService _databaseService = null!;

    public DatabaseTransitionService()
    {
        Di.Container.ResolveField(out _databaseService);

        if (_databaseService is null)
            throw new NullReferenceException("Failed to inject dependence 'Database service'");
    }
    
    public async Task<bool> TransitionRejectGame(FinishedGames finishedGames, GameProgresses gameProgresses, UserGame userGame, CancellationToken ct = default)
    {
        await using var db = new AppDbContext();
        await using var transition = await db.Database.BeginTransactionAsync(ct);
        
        try
        {
            db.UserGames.Update(userGame);
            db.GameProgresses.Update(gameProgresses);
            db.FinishedGames.Remove(finishedGames);
            
            await db.SaveChangesAsync(ct);
            await transition.CommitAsync(ct);

            return true;
        }
        catch (Exception e)
        {
            await transition.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> TransitionFinishGame(FinishedGames finishedGames, GameProgresses gameProgresses, CancellationToken token = default)
    {
        await using var db = new AppDbContext();
        await using var transition = await db.Database.BeginTransactionAsync(token);

        try
        {
            if (await db.FinishedGames
                    .FirstOrDefaultAsync(e => e.GameProgressId == gameProgresses.Id, token) is not null)
            {
                Logger.Debug($"Database is have finished game with gameProgressId {gameProgresses.Id}");
                return true;
            }
            
            db.FinishedGames.Add(finishedGames);
            db.GameProgresses.Update(gameProgresses);
            await db.SaveChangesAsync(token);
            await transition.CommitAsync(token);

            return true;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to transition add finish game" + e.Message);
            await transition.RollbackAsync(token);
            return false;
        }
    }
    
    public async Task<bool> ChooseGameTransition(GameProgresses gameInfo, ulong steamId, CancellationToken ct = default)
    {
        await using var db = new AppDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            db.GameProgresses.Add(gameInfo);

            var userGame = await db.UserGames.FirstOrDefaultAsync(e => e.UserId == steamId, ct);

            if (userGame is null)
                throw new NullReferenceException(nameof(UserGame));

            userGame.AppId = gameInfo.AppId;
            
            db.UserGames.Update(userGame);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to load changes to database: " + e.Message);
            await transaction.RollbackAsync(ct);
            return false;
        }
    }
    
    public async Task<bool> TransitionAddUser(Users user, CancellationToken ct = default)
    {
        await using var db = new AppDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            if (await db.Users.FirstOrDefaultAsync(e => e.SteamId == user.SteamId, ct) is not null)
                Logger.Debug($"Database already have user with steamId {user.SteamId}");
            else
                db.Users.Add(user);
            
            var isUserGame = await _databaseService.TryGetOrCreateUserGame(user, ct);

            if (isUserGame)
                throw new NullReferenceException("Failed to create or get user game");
            
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception e)
        {
            Logger.Error("Failed to transition add user: " + e.Message);
            await transaction.RollbackAsync(ct);
            return false;
        }
    }
}