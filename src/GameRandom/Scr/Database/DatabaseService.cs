using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.SteamSDK.UserData;
using Microsoft.EntityFrameworkCore;
using Steamworks;

namespace GameRandom.Scr.Service;

public interface IDatabaseService
{
    Task<bool> AddItemAsync<TEntity>(TEntity item) where TEntity : class;
    Task<List<TEntity>?> GetTableListAsync<TEntity>() where TEntity : class;
    Task<bool> DeleteItemAsync<TEntity>(TEntity item) where TEntity : class;
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
    
    public async Task<bool> AddItemAsync<TEntity>(TEntity item) where TEntity : class
    {
        await using var db = new AppDbContext();

        if (_restrictedAddTypes.Contains(item.GetType()))
            throw new NotSupportedException($"Type {typeof(TEntity).Name} is restricted. Use Get{typeof(TEntity).Name}Async method");
        
        try
        {
            var dbContext = db.Set<TEntity>();
            await dbContext.AddAsync(item);
            await db.SaveChangesAsync();
            
            Logger.Debug($"Added {item} to db");
        }
        catch (Exception e)
        {
            Logger.Error("Failed to add new item: " + e.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> AddUserGameAsync(Users userInfo)
    {
        await using var db = new AppDbContext();

        if (await db.UserGames.AnyAsync(e => e.UserId == userInfo.SteamId))
            return true;

        UserGame newUserGame = new UserGame
        {
            UserId = userInfo.SteamId,
            AppId = 0
        };
        
        await db.UserGames.AddAsync(newUserGame);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync<TEntity>(Func<TEntity, bool> predicate) where TEntity : class
    {
        try
        {
            await using var context = new AppDbContext();
            var list = await context.Set<TEntity>().AsNoTracking().ToListAsync();
            
            return list.FirstOrDefault(predicate);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<List<TEntity>?> GetTableListAsync<TEntity>() 
        where TEntity : class 
    {
        if (_restrictedGetTypes.Contains(typeof(TEntity)))
            throw new NotSupportedException($"Type {typeof(TEntity).Name} is restricted. Use Get{typeof(TEntity).Name}Async method");
        
        try
        {
            await using var context = new AppDbContext();
            return await context.Set<TEntity>().AsNoTracking().ToListAsync();
        }
        catch (Exception e)
        {
            Logger.Error("Failed to get table list from table " + e.Message);
            return null;
        }
    }
    
    public async Task<bool>  DeleteItemAsync<TEntity>(TEntity item) where TEntity : class
    {
        await using var context = new AppDbContext();

        try
        {
            var db = context.Set<TEntity>();
            db.Remove(item);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Logger.Error($"Failed deleted item with type {typeof(TEntity).Name}. Error: {e.Message}");
            return false;
        }

        return true;
    }
    
    public async Task<bool> UpdateAsync<TEntity>(TEntity item) where TEntity : class
    {
        try
        {
            await using var context = new AppDbContext();
            context.Set<TEntity>().Update(item);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed update database data {e.Message}");
            return false;
        }
        
    
    }
    
    public async Task<List<TEntity>?> Where<TEntity>(Func<TEntity,bool> predicate)
        where TEntity : class
    {
        await using var context = new AppDbContext();
        var list = await context.Set<TEntity>().AsNoTracking().ToListAsync();

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

    public async Task<TResult?> ExecuteDbOperation<TResult>(Func<DatabaseService, Task<TResult>> operation,
        string errorMessage) where TResult : class
    {
        try
        {
            return await operation(this);
        }
        catch (Exception e)
        {
            Logger.Error($"{errorMessage}, {e.Message}");
            return null;
        }
    }

    public async Task<UserGame?> GetUserGameAsync(Users userData)
    {
        try
        {
            await using var appDb = new AppDbContext();
            return await appDb.UserGames.FirstOrDefaultAsync(e => e.UserId == userData.SteamId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<Users?> GetUserByUlongId(ulong steamId)
    {
        await using var appDb = new AppDbContext();
        Users? user = await appDb.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);

        return user ?? null;
    }

    public async Task<Lobbies?> GetLobbyById(long lobbyId)
    {
        await using var appDb = new AppDbContext();
        Lobbies? lobby = await appDb.Lobbies
            .Include(l => l.LobbyData)
            .FirstOrDefaultAsync(l => l.LobbyId == lobbyId);
        
        return lobby ?? null;
    }
}