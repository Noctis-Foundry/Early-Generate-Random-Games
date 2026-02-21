ц## AppDbContext
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Users>           Users            { get; set; }
    public DbSet<LobbyUserContext> LobbyUserContext { get; set; }
    public DbSet<GameProgress>    GameProgress     { get; set; }
    public DbSet<Lobbies>         Lobbies          { get; set; }

    public const string HostPath =
        "Host=***;Database=steamdata;Username=users;Password=***";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(HostPath);
    }
}
```

---

## Модели

### Users
```csharp
public class Users
{
    public int    Id       { get; set; }
    public ulong  ClientID { get; set; }
    public string Username { get; set; }
}
```

### Lobbies
```csharp
public class Lobbies
{
    public int  Id          { get; set; }
    public long LobbyID     { get; set; }
    public int  MemberCount { get; set; }
}
```

### LobbyUserContext
```csharp
public class LobbyUserContext
{
    public int    Id         { get; set; }
    public long   LobbyID   { get; set; }
    public ulong  MemberID  { get; set; }
    public string NickName  { get; set; }
    public int    PlayerIcon { get; set; }
}
```

### GameProgress
```csharp
public class GameProgress
{
    public int     Id         { get; set; }
    public ulong   ClientId   { get; set; }
    public string  GameName   { get; set; }
    public int     Grade      { get; set; }
    public string? Comment    { get; set; }
    public string  DataBegin  { get; set; }
    public string  DataEnd    { get; set; }
    public bool    IsFinished { get; set; }
}
```