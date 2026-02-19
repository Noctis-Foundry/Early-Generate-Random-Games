using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
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
    public async Task<bool> AddItemAsync<TEntity>(TEntity item) where TEntity : class
    {
        await using var db = new AppDbContext();

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

    public async Task<List<TEntity>?> GetTableListAsync<TEntity>() 
        where TEntity : class 
    {
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

    public async Task<UserGame?> GetUserGameByAppId(int appId)
    {
        try
        {
            await using var appDb = new AppDbContext();
            UserGame? game = await appDb.UserGames
                .Include(ug => ug.GameProgresses).FirstOrDefaultAsync(e => e.GameID == appId);

            if (game is null)
                return null;

            game.AppName = game.GameProgresses.AppName;
            game.BeginData = game.GameProgresses.BeginTime;
            game.EndData = game.GameProgresses.EndTime;

            return game;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}