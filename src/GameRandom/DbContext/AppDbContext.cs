using Microsoft.EntityFrameworkCore;

namespace GameRandom.DataBaseContexts;

public class AppDbContext : DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<LobbyContext> LobbyContexts { get; set; }
    public DbSet<GameProgress> GameTables { get; set; }
    
    public const string HostPath = "Host=217.11.167.137;Database=rdb;Username=ara;Password=m7a4k5s9i2m";

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

public class LobbyContext
{
    public int Id { get; set; }
    public ulong LobbyID { get; set; }
    public ulong MemberID { get; set; }
    public string NickName { get; set; }
}
public class GameProgress
{
    public int Id { get; set; }
    public uint ClientId { get; set; }
    public uint LobbyId { get; set; }
    public string GameName { get; set; }
    public string DataBegin { get; set; }
    public string DataEnd { get; set; }
    public bool IsFinished { get; set; }
}
