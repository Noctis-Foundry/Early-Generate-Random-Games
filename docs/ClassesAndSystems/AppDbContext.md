# AppDbContext

## Overview
`AppDbContext` is the main Entity Framework Core database context for the GameRandom application. It manages database connections and entity relationships for the PostgreSQL database.

## Database Connection
- **Provider**: PostgreSQL (Npgsql)
- **Connection String**: Stored in `HostPath` constant

## DbSets (Tables)

### Users
Stores user information from Steam.
- `ID` (int) - Primary key
- `SteamID` (ulong) - Steam user identifier
- `LobbyID` (long) - Current lobby identifier
- `Nickname` (string) - User display name
- `AvatarURL` (int) - Avatar reference

### Lobbies
Manages game lobbies.
- `ID` (int) - Primary key
- `LobbyId` (long) - Unique lobby identifier
- `MembersCount` (int) - Number of members
- `LobbyData` (List<LobbyData>) - Navigation property to lobby members

### GameProgresses
Tracks game completion progress.
- `ID` (int) - Primary key
- `AppID` (int) - Steam application ID
- `PlayerID` (ulong) - Player Steam ID
- `AppName` (string) - Game name
- `Comment` (string?) - Optional player comment
- `Grade` (int) - Player rating
- `BeginTime` (DateTime) - Start date
- `EndTime` (DateTime) - End date
- `IsFinished` (bool) - Completion status

### UserGame
Links users to their games with time tracking.
- `ID` (int) - Primary key
- `IsHaveGame` (bool) - Ownership status
- `GameID` (int) - Foreign key to GameProgresses
- `AppName` (string) - Game name
- `LeftDays` (int) - Days remaining
- `BeginData` (DateTime?) - Start date
- `EndData` (DateTime?) - End date
- `GameProgresses` (GameProgresses) - Navigation property

### LobbyData
Junction table for lobby members.
- `Id` (int) - Primary key
- `LobbyId` (long) - Foreign key to Lobbies
- `UserId` (ulong) - Steam user ID

## Relationships

### UserGame → GameProgresses
One-to-one relationship:
```csharp
UserGame.GameID → GameProgresses.ID
```

### Lobbies → LobbyData
One-to-many relationship:
```csharp
Lobbies.LobbyId → LobbyData.LobbyId (multiple)
```

## Configuration
Configured in `OnModelCreating` method with explicit foreign key mappings and navigation properties.

## Usage Example
```csharp
using (var context = new AppDbContext())
{
    var users = context.Users.ToList();
    var lobbies = context.Lobbies.Include(l => l.LobbyData).ToList();
}
```
