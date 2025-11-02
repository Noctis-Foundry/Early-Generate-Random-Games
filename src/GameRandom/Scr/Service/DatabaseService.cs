using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using Microsoft.EntityFrameworkCore;

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
        await using var context = new AppDbContext();

        try
        {
            var db = context.Set<TEntity>();

            List<TEntity> result = await db.ToListAsync();
            return result;
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
}