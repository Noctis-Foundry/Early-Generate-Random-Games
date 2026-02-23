using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace GameRandom.DataBaseContexts;

public class AppDbContext : DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Lobbies> Lobbies { get; set; }
    public DbSet<GameProgresses> GameProgresses { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    public DbSet<LobbyData> LobbyData { get; set; }
    
    public const string HostPath = "Host=80.93.62.153;Database=steamdata;Username=users;Password=ninokuriko212410";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(HostPath);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGame>().HasIndex(u => u.UserId).IsUnique();
        
        modelBuilder.Entity<Lobbies>().HasMany(u => u.LobbyData).WithOne().HasPrincipalKey(e => e.LobbyId)
            .HasForeignKey(e => e.LobbyId);
    }
}

public class Users
{
    public int Id { get; set; }
    public ulong SteamId { get; set; }
    public long LobbyId { get; set; }
    public string Nickname { get; set; }
    public int AvatarURL { get; set; }
}

public class Lobbies
{
    public int Id { get; set; }
    public long LobbyId { get; set; }
    public int MembersCount { get; set; }
    
    public List<LobbyData> LobbyData { get; set; } //Navigation
}

public class GameProgresses
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public ulong PlayerId { get; set; }
    public string? AppName { get; set; }
    public string? Comment { get; set; }
    public byte[]? AppHeaderImage { get; set; }
    public int Grade { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsFinished { get; set; }
}

public class UserGame
{
    public int Id { get; set; }
    public ulong UserId { get; set; } // Взятие листа из базы данных UserGame конкретного пользователя
    public int AppId { get; set; }
}

public class EndGame
{
    public ulong UserId { get; set; }
    public int AppId { get; set; }
    public byte[]? ScreenShot { get; set; }
    public string? Nickname { get; set; }
    public DateTime FinishTime { get; set; }
    public GameProgresses? GameProgresses { get; set; }
}

public class LobbyData
{
    public int Id { get; set; }
    public long LobbyId { get; set; }
    public ulong UserId { get; set; }
}



