# Database Services

## Overview
Database layer providing CRUD operations and real-time change notifications for PostgreSQL database. Consists of DatabaseService for data operations and PostgresListener for real-time updates.

---

## DatabaseService

### Purpose
Generic repository service for database operations using Entity Framework Core. Implements IDatabaseService interface.

### Interface: IDatabaseService
```csharp
Task<bool> AddItemAsync<TEntity>(TEntity item);
Task<List<TEntity>?> GetTableListAsync<TEntity>();
Task<bool> DeleteItemAsync<TEntity>(TEntity item);
```

### Core Methods

#### AddItemAsync<TEntity>(TEntity item)
Adds new entity to database.

**Returns**: `bool` - Success status

**Process**:
1. Create AppDbContext
2. Add item to DbSet
3. Save changes
4. Log operation

**Error Handling**: Returns false on exception, logs error

#### GetTableListAsync<TEntity>()
Retrieves all entities from table with no tracking.

**Returns**: `List<TEntity>?` - Entity list or null on error

**Features**:
- Uses AsNoTracking() for read-only queries
- Better performance for non-modifying operations

#### DeleteItemAsync<TEntity>(TEntity item)
Removes entity from database.

**Returns**: `bool` - Success status

**Process**:
1. Create context
2. Remove item from DbSet
3. Save changes

#### UpdateAsync<TEntity>(TEntity item)
Updates existing entity in database.

**Returns**: `bool` - Success status

**Process**:
1. Create context
2. Mark entity as modified
3. Save changes

#### Where<TEntity>(Func<TEntity, bool> predicate)
Filters entities by predicate.

**Parameters**:
- `predicate` - Filter function

**Returns**: `List<TEntity>?` - Filtered results

**Implementation**:
- Loads all entities with AsNoTracking()
- Applies predicate in memory
- Returns matching entities

#### ExecuteDbOperation<TResult>(Func<DatabaseService, Task<TResult>> operation, string errorMessage)
Generic wrapper for database operations with error handling.

**Parameters**:
- `operation` - Database operation to execute
- `errorMessage` - Error message prefix

**Returns**: `TResult?` - Operation result or null on error

### Specialized Query Methods

#### GetUserGameByAppId(int appId)
Retrieves UserGame with related GameProgresses data.

**Returns**: `UserGame?` - Game with populated navigation properties

**Features**:
- Includes GameProgresses via navigation property
- Maps related fields (AppName, BeginData, EndData)

#### GetUserByUlongId(ulong steamId)
Finds user by Steam ID.

**Returns**: `Users?` - User entity or null

#### GetLobbyById(long lobbyId)
Retrieves lobby with member data.

**Returns**: `Lobbies?` - Lobby with LobbyData collection

**Features**:
- Includes LobbyData navigation property
- Loads all lobby members

---

## PostgresListener

### Purpose
Real-time database change notification system using PostgreSQL LISTEN/NOTIFY mechanism. Monitors database changes and triggers callbacks.

### Architecture

**Connection**: Persistent NpgsqlConnection to PostgreSQL

**Channel**: Listens to `changes_channel`

**Payload Format**: `{OpCode}.{TableCode}.{RowId}`

### Initialization
```csharp
var listener = new PostgresListener();
```

**Process**:
1. Opens connection to PostgreSQL
2. Executes `LISTEN changes_channel`
3. Starts async notification loop

### Subscription System

#### Subscribe(TableEnum table, Action<PayloadStructure> subscriber)
Registers callback for table changes.

**Parameters**:
- `table` - Table to monitor
- `subscriber` - Callback function

**Example**:
```csharp
listener.Subscribe(TableEnum.Lobby, payload => 
{
    Console.WriteLine($"Lobby changed: Row {payload.RowId}");
});
```

#### Unsubscribe(TableEnum table, Action<PayloadStructure> subscriber)
Removes callback subscription.

### Payload Structure

**PayloadStructure**:
- `OpCode` (int) - Operation type (Add/Update/Delete)
- `TableCode` (int) - Table identifier
- `RowId` (int) - Affected row ID

**Parsing**:
- Format: `"0.1.42"` → OpCode=0, TableCode=1, RowId=42
- Validates format and parses integers
- Returns null on invalid payload

### Enums

#### TableEnum
```csharp
Lobby = 0
LobbyContext = 1
GameTable = 2
Users = 3
```

#### OperationsEnum
```csharp
Add = 0
Update = 1
Delete = 2
```

### Internal Methods

**ListenChanel()**
Sets up notification listener and async polling loop.

**ParsingPayload(string payload)**
Parses notification payload into PayloadStructure.

**SendCallbacks(TableEnum table, PayloadStructure payload)**
Invokes registered callbacks for table.

### Disposal
Implements IDisposable to close database connection.

```csharp
listener.Dispose();
```

---

## Usage Examples

### Basic CRUD Operations
```csharp
var dbService = new DatabaseService();

// Add
var user = new Users { SteamID = 123, Nickname = "Player1" };
await dbService.AddItemAsync(user);

// Read
var users = await dbService.GetTableListAsync<Users>();

// Update
user.Nickname = "UpdatedName";
await dbService.UpdateAsync(user);

// Delete
await dbService.DeleteItemAsync(user);
```

### Filtered Queries
```csharp
var activePlayers = await dbService.Where<Users>(u => u.LobbyID > 0);
```

### Real-Time Notifications
```csharp
var listener = new PostgresListener();

listener.Subscribe(TableEnum.Lobby, payload =>
{
    if (payload.OpCode == (int)OperationsEnum.Add)
    {
        Console.WriteLine($"New lobby created: {payload.RowId}");
    }
});
```

### Combined Usage
```csharp
// Listen for lobby changes
listener.Subscribe(TableEnum.Lobby, async payload =>
{
    var lobby = await dbService.GetLobbyById(payload.RowId);
    UpdateUI(lobby);
});
```

---

## Features

**DatabaseService**:
- Generic repository pattern
- Async/await throughout
- No-tracking queries for performance
- Specialized queries for complex relationships
- Comprehensive error logging

**PostgresListener**:
- Real-time change notifications
- Pub/sub pattern for multiple subscribers
- Automatic reconnection handling
- Type-safe table and operation enums
- Payload validation

## Connection String
Both services use same connection:
```
Host=80.93.62.153;Database=steamdata;Username=users;Password=ninokuriko212410
```

## Error Handling
- All operations return bool or nullable types
- Exceptions logged via Logger service
- Graceful degradation on failures
