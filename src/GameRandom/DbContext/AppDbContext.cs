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
        modelBuilder.Entity<UserGame>().HasOne(u => u.GameProgresses).WithOne()
            .HasForeignKey<UserGame>(gp => gp.GameID);

        modelBuilder.Entity<Lobbies>().HasMany(u => u.LobbyData).WithOne().HasPrincipalKey(e => e.LobbyId)
            .HasForeignKey(e => e.LobbyId);
    }
}

public class Users
{
    public int ID { get; set; }
    public ulong SteamID { get; set; }
    public long LobbyID { get; set; }
    public string Nickname { get; set; }
    public int AvatarURL { get; set; }
}

public class Lobbies
{
    public int ID { get; set; }
    public long LobbyId { get; set; }
    public int MembersCount { get; set; }
    
    public List<LobbyData> LobbyData { get; set; } //Navigation
}

public class GameProgresses
{
    public int ID { get; set; }
    public int AppID { get; set; }
    public ulong PlayerID { get; set; }
    public string AppName { get; set; }
    public string? Comment { get; set; }
    public int Grade { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsFinished { get; set; }
}

public class UserGame
{
    public int ID { get; set; }
    public bool IsHaveGame { get; set; }
    public int GameID { get; set; }                  // FK → GameProgress.ID
    public string AppName { get; set; }              // ← GameProgress.AppName
    public int LeftDays { get; set; }
    public DateTime? BeginData { get; set; }          // ← GameProgress.BeginTime
    public DateTime? EndData { get; set; }            // ← GameProgress.EndTime

    public GameProgresses GameProgresses { get; set; }   // Navigation property
}

public class LobbyData
{
    public int Id { get; set; }
    public long LobbyId { get; set; }
    public ulong UserId { get; set; }
}



