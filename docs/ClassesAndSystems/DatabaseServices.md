 # Database Services

## Overview
Database layer providing CRUD operations and real-time change notifications for PostgreSQL database. Consists of DatabaseService for data operations and PostgresListener for real-time updates.

---

## DatabaseService

### Purpose
Generic repository service for database operations using Entity Framework Core with cancellation token support. Implements IDatabaseService interface.

### Interface: IDatabaseService
```csharp
Task<bool> AddItemAsync<TEntity>(TEntity item, CancellationToken ct = default);
Task<List<TEntity>?> GetTableListAsync<TEntity>(CancellationToken ct = default);
Task<bool> DeleteItemAsync<TEntity>(TEntity item, CancellationToken ct = default);
```

### Restricted Types

#### _restrictedAddTypes
- `UserGame` - Must use AddUserGameAsync instead

#### _restrictedGetTypes
- `UserGame` - Must use GetUserGameAsync instead
- `Lobbies` - Must use GetLobbyById instead

### Core Methods

#### AddItemAsync<TEntity>(TEntity item, CancellationToken ct = default)
Adds new entity to database.

**Parameters**:
- `item` - Entity to add
- `ct` - Cancellation token (default)

**Returns**: `bool` - Success status

**Process**:
1. Validate type not restricted
2. Create AppDbContext
3. Add item to DbSet with cancellation token
4. Save changes with cancellation token
5. Log operation

**Error Handling**: 
- Catches OperationCanceledException
- Catches general Exception
- Logs "Operation failed {message}"
- Returns false on error

**Throws**: NotSupportedException for restricted types

#### AddUserGameAsync(Users userInfo, CancellationToken ct = default)
Adds UserGame for user if not exists.

**Parameters**:
- `userInfo` - User entity
- `ct` - Cancellation token

**Returns**: `bool` - Success status

**Process**:
1. Check if UserGame exists for user
2. If exists, return true
3. Create new UserGame with AppId=0
4. Add and save with cancellation token

**Error Handling**: Same as AddItemAsync

#### GetFirstOrDefaultAsync<TEntity>(Func<TEntity, bool> predicate, CancellationToken ct = default)
Retrieves first entity matching predicate.

**Parameters**:
- `predicate` - Filter function
- `ct` - Cancellation token

**Returns**: `TEntity?` - First match or null

**Process**:
1. Load all entities with AsNoTracking and cancellation token
2. Apply predicate in memory
3. Return first match

**Error Handling**: Returns null on error

#### GetTableListAsync<TEntity>(CancellationToken ct = default)
Retrieves all entities from table with no tracking.

**Parameters**:
- `ct` - Cancellation token

**Returns**: `List<TEntity>?` - Entity list or null on error

**Features**:
- Validates type not restricted
- Uses AsNoTracking() for read-only queries
- Supports cancellation
- Better performance for non-modifying operations

**Throws**: NotSupportedException for restricted types

#### DeleteItemAsync<TEntity>(TEntity item, CancellationToken ct = default)
Removes entity from database.

**Parameters**:
- `item` - Entity to delete
- `ct` - Cancellation token

**Returns**: `bool` - Success status

**Process**:
1. Create context
2. Remove item from DbSet
3. Save changes with cancellation token

**Error Handling**: Same as AddItemAsync

#### UpdateAsync<TEntity>(TEntity item, CancellationToken ct = default)
Updates existing entity in database.

**Parameters**:
- `item` - Entity to update
- `ct` - Cancellation token

**Returns**: `bool` - Success status

**Process**:
1. Create context
2. Mark entity as modified
3. Save changes with cancellation token

**Error Handling**: 
- Catches OperationCanceledException
- Catches DbUpdateException
- Catches general Exception
- Returns false on error

#### Where<TEntity>(Func<TEntity, bool> predicate, CancellationToken ct = default)
Filters entities by predicate.

**Parameters**:
- `predicate` - Filter function
- `ct` - Cancellation token

**Returns**: `List<TEntity>?` - Filtered results or null

**Implementation**:
- Loads all entities with AsNoTracking and cancellation token
- Checks if list is empty, logs error if so
- Applies predicate in memory via loop
- Returns matching entities

**Error Handling**: Returns null on error or empty list

#### ExecuteDbOperation<TResult>(Func<DatabaseService, Task<TResult>> operation, string errorMessage, CancellationToken ct = default)
Generic wrapper for database operations with error handling.

**Parameters**:
- `operation` - Database operation to execute
- `errorMessage` - Error message prefix (not used in current implementation)
- `ct` - Cancellation token

**Returns**: `TResult?` - Operation result or null on error

**Error Handling**: Catches OperationCanceledException and Exception

### Specialized Query Methods

#### GetUserGameAsync(Users userData, CancellationToken ct = default)
Retrieves UserGame by user Steam ID.

**Parameters**:
- `userData` - User entity
- `ct` - Cancellation token

**Returns**: `UserGame?` - User game or null

**Error Handling**: Returns null on error

#### GetUserByUlongId(ulong steamId, CancellationToken ct = default)
Finds user by Steam ID.

**Parameters**:
- `steamId` - Steam user ID
- `ct` - Cancellation token

**Returns**: `Users?` - User entity or null

**Error Handling**: Returns null on error

#### GetLobbyById(long lobbyId, CancellationToken ct = default)
Retrieves lobby with member data.

**Parameters**:
- `lobbyId` - Lobby identifier
- `ct` - Cancellation token

**Returns**: `Lobbies?` - Lobby with LobbyData collection or null

**Features**:
- Includes LobbyData navigation property via Include()
- Loads all lobby members
- Supports cancellation

**Error Handling**: Returns null on error

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

### With Cancellation Token
```csharp
var cts = new CancellationTokenSource();

// Add with cancellation
await dbService.AddItemAsync(user, cts.Token);

// Query with cancellation
var users = await dbService.GetTableListAsync<Users>(cts.Token);

// Cancel operation
cts.Cancel();
```

### Filtered Queries
```csharp
var activePlayers = await dbService.Where<Users>(u => u.LobbyID > 0);

var firstAdmin = await dbService.GetFirstOrDefaultAsync<Users>(u => u.IsAdmin);
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
- Cancellation token support for all operations
- No-tracking queries for performance
- Specialized queries for complex relationships
- Type restrictions for safety
- Comprehensive error logging
- Handles OperationCanceledException separately

**PostgresListener**:
- Real-time change notifications
- Pub/sub pattern for multiple subscribers
- Automatic reconnection handling
- Type-safe table and operation enums
- Payload validation

## Error Handling
- All operations return bool or nullable types
- Separate handling for OperationCanceledException
- Exceptions logged via Logger service with "Operation failed" prefix
- Graceful degradation on failures
- NotSupportedException for restricted type access
