using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace GameRandom.DataBaseContexts;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Lobby> Lobbies { get; set; }
    public DbSet<GameProgress> GameProgresses { get; set; }
    public DbSet<UserGame> UserGames { get; set; }
    
    public const string HostPath = "Host=80.93.62.153;Database=steamdata;Username=users;Password=ninokuriko212410";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(HostPath);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGame>().HasOne(u => u.GameProgress).WithOne()
            .HasForeignKey<UserGame>(gp => gp.GameID);
    }
}

public class User
{
    public int ID { get; set; }
    public ulong SteamID { get; set; }
    public long LobbyID { get; set; }
    public string Nickname { get; set; }
    public int AvatarURL { get; set; }
}

public class Lobby
{
    public int ID { get; set; }
    public long LobbyID { get; set; }
    public int MembersCount { get; set; }
}

public class GameProgress
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

    public GameProgress GameProgress { get; set; }   // Navigation property
}



