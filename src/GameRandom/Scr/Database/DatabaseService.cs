using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK.UserData;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.Scr.Service;

public interface IDatabaseService
{
    Task<bool> AddItemAsync<TEntity>(TEntity item, CancellationToken ct = default) where TEntity : class;
    Task<List<TEntity>?> GetTableListAsync<TEntity>(CancellationToken ct = default) where TEntity : class;
    Task<bool> DeleteItemAsync<TEntity>(TEntity item, CancellationToken ct = default) where TEntity : class;
}

public class DatabaseService : IDatabaseService
{
    private static readonly HashSet<Type> _restrictedAddTypes = new()
    {
        typeof(UserGame)
    };

    private static readonly HashSet<Type> _restrictedGetTypes = new()
    {
        typeof(UserGame),
        typeof(Lobbies)
    };
    
    public async Task<bool> AddItemAsync<TEntity>(TEntity item, CancellationToken ct = default) where TEntity : class
    {
        await using var db = new AppDbContext();

        if (_restrictedAddTypes.Contains(item.GetType()))
            throw new NotSupportedException($"Type {typeof(TEntity).Name} is restricted. Use Get{typeof(TEntity).Name}Async method");
        
        try
        {
            var dbContext = db.Set<TEntity>();
            await dbContext.AddAsync(item, ct);
            await db.SaveChangesAsync(ct);
            
            Logger.Debug($"Added {item} to db");
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
        catch (DbUpdateException e)
        {
            // Внутреннее исключение с реальной причиной
            var innerMessage = e.InnerException?.Message;
            Console.WriteLine($"DB Error: {innerMessage}");

            return false;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }

        return true;
    }

    public async Task<bool> AddUserGameAsync(Users userInfo, CancellationToken ct = default)
    {
        try
        {
            await using var db = new AppDbContext();

            if (await db.UserGames.AnyAsync(e => e.UserId == userInfo.SteamId, ct))
                return true;

            UserGame newUserGame = new UserGame
            {
                UserId = userInfo.SteamId,
                AppId = 0
            };
            
            await db.UserGames.AddAsync(newUserGame, ct);
            await db.SaveChangesAsync(ct);

            return true;
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync<TEntity>(Func<TEntity, bool> predicate, CancellationToken ct = default) where TEntity : class
    {
        try
        {
            await using var context = new AppDbContext();
            var list = await context.Set<TEntity>().AsNoTracking().ToListAsync(ct);
            
            return list.FirstOrDefault(predicate);
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }

    public async Task<List<TEntity>?> GetTableListAsync<TEntity>(CancellationToken ct = default) 
        where TEntity : class 
    {
        if (_restrictedGetTypes.Contains(typeof(TEntity)))
            throw new NotSupportedException($"Type {typeof(TEntity).Name} is restricted. Use Get{typeof(TEntity).Name}Async method");
        
        try
        {
            await using var context = new AppDbContext();
            return await context.Set<TEntity>().AsNoTracking().ToListAsync(ct);
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }
    
    public async Task<bool>  DeleteItemAsync<TEntity>(TEntity item, CancellationToken ct = default) where TEntity : class
    {
        await using var context = new AppDbContext();

        try
        {
            var db = context.Set<TEntity>();
            db.Remove(item);
            await context.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }

        return true;
    }
    
    public async Task<bool> UpdateAsync<TEntity>(TEntity item, CancellationToken ct = default) where TEntity : class
    {
        try
        {
            await using var context = new AppDbContext();
            context.Set<TEntity>().Update(item);
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
        catch (DbUpdateException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return false;
        }
    }
    
    public async Task<List<TEntity>?> Where<TEntity>(Func<TEntity,bool> predicate, CancellationToken ct = default)
        where TEntity : class
    {
        try
        {
            await using var context = new AppDbContext();
            var list = await context.Set<TEntity>().AsNoTracking().ToListAsync(ct);

            if (list.Count <= 0)
            {
                Logger.Error($"List with name {typeof(TEntity).Name} not found");
                return null;
            }
            
            List<TEntity>? result = new();

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    result.Add(list[i]);
                }
            }
            
            return result;
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }
    
    public async Task<List<FinishedGames>?> GetFinishedGames(CancellationToken ct = default)
    {
        try
        {
            await using var db = new AppDbContext();
            return await db.FinishedGames
                .Include(x => x.GameProgresses)
                .ToListAsync(ct);
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }

    public async Task<FinishedGames?> GetFinishedGamesFromId(int rowId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new AppDbContext();
            return await db.FinishedGames.Include(e => e.GameProgresses).FirstOrDefaultAsync(g => g.Id == rowId, ct);

        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }

    public async Task<UserGame?> GetUserGameAsync(Users userData, CancellationToken ct = default)
    {
        try
        {
            await using var appDb = new AppDbContext();
            return await appDb.UserGames.FirstOrDefaultAsync(e => e.UserId == userData.SteamId, ct);
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }

    public async Task<Users?> GetUserByUlongId(ulong steamId, CancellationToken ct = default)
    {
        try
        {
            await using var appDb = new AppDbContext();
            Users? user = await appDb.Users.FirstOrDefaultAsync(u => u.SteamId == steamId, ct);

            return user ?? null;
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }

    public async Task<Lobbies?> GetLobbyById(long lobbyId, CancellationToken ct = default)
    {
        try
        {
            await using var appDb = new AppDbContext();
            Lobbies? lobby = await appDb.Lobbies
                .Include(l => l.LobbyData).Include(a => a.AdminsList)
                .FirstOrDefaultAsync(l => l.LobbyId == lobbyId, ct);
            
            return lobby ?? null;
        }
        catch (OperationCanceledException e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Operation failed {e.Message}");
            return null;
        }
    }
}