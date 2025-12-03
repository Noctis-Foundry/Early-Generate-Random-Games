using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.DataBaseContexts;

public class AppDbContext : DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<LobbyUserContext> LobbyContexts { get; set; }
    public DbSet<GameProgress> GameTables { get; set; }
    public DbSet<Lobbies> Lobbies { get; set; }
    
    public const string HostPath = "Host=80.93.62.153;Database=steamdata;Username=users;Password=ninokuriko212410";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(HostPath);
    }
}

public class Users
{
    public int Id { get; set; }
    public ulong ClientID { get; set; }
    public string Username { get; set; }
}

public class LobbyUserContext
{
    public int Id { get; set; }
    public long LobbyID { get; set; }
    public ulong MemberID { get; set; }
    public string NickName { get; set; }
    
    public int PlayerIcon { get; set; }
}

public class Lobbies
{
    public int Id { get; set; }
    public long LobbyID { get; set; }
    public List<ulong> MemberCount { get; set; }
}
public class GameProgress
{
    public int Id { get; set; }
    public ulong ClientId { get; set; }
    public string GameName { get; set; }
    public int Grade {get; set;}
    public string? Comment { get; set; }
    public string DataBegin { get; set; }
    public string DataEnd { get; set; }
    public bool IsFinished { get; set; }
}
