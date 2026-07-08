using System;
using System.Collections.Generic;
using System.Text.Json;
using GameRandom.Scripts.StartupLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GameRandom.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Lobbies> Lobbies { get; set; }
    public DbSet<GameProgresses> GameProgresses { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<LobbyData> LobbyData { get; set; }
    public DbSet<FinishedGames> FinishedGames { get; set; }
    public DbSet<Admins> Admins { get; set; }
    
    private string? _hostPath = "";
    
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        
        if (GameEnvLoad._envCollection.TryGetValue(EnvType.DatabaseEnv, out var databaseApi))
        {
            _hostPath = databaseApi;
        }
        else 
            throw new NullReferenceException("Failed to load database api, loading in offline mode or check network connection");
        
        if (_hostPath is null)
            throw new ArgumentNullException(nameof(_hostPath));
        
        optionsBuilder.UseNpgsql(_hostPath).LogTo(Console.WriteLine, LogLevel.Information).EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGame>().HasIndex(u => u.UserId).IsUnique();

        modelBuilder.Entity<FinishedGames>().HasIndex(f => f.GameProgressId).IsUnique();
        
        modelBuilder.Entity<Lobbies>().HasMany(u => u.LobbyData).WithOne().HasPrincipalKey(e => e.LobbyId)
            .HasForeignKey(e => e.LobbyId);

        modelBuilder.Entity<Lobbies>().HasMany(u => u.AdminsList).WithOne().HasPrincipalKey(e => e.LobbyId)
            .HasPrincipalKey(e => e.LobbyId);

        modelBuilder.Entity<FinishedGames>().HasOne(e => e.GameProgresses).WithMany()
            .HasForeignKey(e => e.GameProgressId );

        modelBuilder.Entity<UserGame>().Property(e => e.AppIdList).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null));

        //TODO: Optimization UserGame Table Post/Fetch
        // modelBuilder.Entity<UserGame>().HasOne(e => e.Users).WithMany().HasForeignKey(e => e.UserId);
        // modelBuilder.Entity<UserGame>().HasOne(e => e.GameProgresses).WithMany()
        //     .HasForeignKey(e => new { e.UserId, e.AppId });
    }
}

public abstract class BaseTable
{
    public int Id { get; set; }
}

public class Users : BaseTable
{
    public ulong SteamId { get; set; }
    public long LobbyId { get; set; }
    public string? Nickname { get; set; }
    public int AvatarURL { get; set; } //TODO Change to byte + webp format
}

public class Lobbies : BaseTable
{
    public long LobbyId { get; set; }
    public int MembersCount { get; set; }
    public List<LobbyData> LobbyData { get; set; } //Navigation
    
    public List<Admins> AdminsList { get; set; } //Navigation
}

public class GameProgresses : BaseTable
{
    public int AppId { get; set; }
    public ulong PlayerId { get; set; }
    public string? AppName { get; set; }
    public string? Comment { get; set; }
    public byte[]? AppHeaderImage { get; set; }
    public int Grade { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime FinishTime { get; set; }
    public bool IsFinished { get; set; }
}

public class UserGame : BaseTable
{
    public ulong UserId { get; set; } // Взятие листа из базы данных UserGame конкретного пользователя
    public int AppId { get; set; }
    
    public List<int>? AppIdList { get; set;}
    
    // public Users? Users { get; set; } // Навигационное свойство
    // public GameProgresses? GameProgresses { get; set; } // Навигационное свойство
}

public class FinishedGames : BaseTable
{
    public int GameProgressId { get; set; }
    
    public byte[]? ScreenShot { get; set; }
    
    public bool IsImprove { get; set; }
    
    public GameProgresses? GameProgresses { get; set; }
}

public class Admins : BaseTable
{
    public ulong SteamId { get; set; }
    public long LobbyId { get; set; }
    public bool IsTopAdmin { get; set; }
}

public class LobbyData : BaseTable
{
    public long LobbyId { get; set; }
    public ulong UserId { get; set; }
}



