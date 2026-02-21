# SteamWebApi

## Overview
Service for interacting with Steam Web API to retrieve user profile data. Fetches player summaries including avatar URLs.

## Purpose
- Query Steam Web API for user data
- Parse JSON responses
- Retrieve user avatars
- Provide profile information

## Constants

**ApiKey** = "74FCA50C2D6D2C3ACF641458D135AA3A"

Steam Web API key for authentication.

## Fields

**_client** (HttpClient) - HTTP client for API requests

## Methods

### GetUserData(ulong userId)
Retrieves user profile data from Steam Web API.

**Parameters**:
- `userId` - Steam ID as 64-bit integer

**Returns**: `Task<ProfilerContext?>` - Profile data or null on error

**Process**:
1. Construct API URL with key and user ID
2. Log request URL
3. Send GET request to Steam API
4. Check response status
5. Parse JSON response
6. Extract avatar URL
7. Create ProfilerContext
8. Return profile data

**API Endpoint**:
```
https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/
?key={ApiKey}&steamids={userId}
```

**JSON Response Structure**:
```json
{
  "response": {
    "players": [
      {
        "steamid": "76561198000000000",
        "avatar": "https://avatars.steamstatic.com/...",
        "personaname": "PlayerName",
        ...
      }
    ]
  }
}
```

**Error Handling**:
- Returns null if HTTP request fails
- Throws exception with inner exception on parsing errors
- Logs errors via Logger

## ProfilerContext

### Purpose
Data model for Steam user profile information.

### Fields

**MemberID** (ulong) - Steam user ID

**avatarUrl** (string) - URL to user's avatar image

### Usage
```csharp
var profile = await steamWebApi.GetUserData(steamId);
if (profile != null)
{
    Console.WriteLine($"User: {profile.MemberID}");
    Console.WriteLine($"Avatar: {profile.avatarUrl}");
}
```

## Usage Examples

### Fetch User Avatar
```csharp
var steamWebApi = new SteamWebApi();
ulong steamId = SteamManager.GetSteamIdAsLong();

ProfilerContext? profile = await steamWebApi.GetUserData(steamId);

if (profile != null)
{
    var avatarBitmap = await SteamService.Instance.GetImage(profile.avatarUrl);
    avatarImage.Source = avatarBitmap;
}
```

### Load Multiple User Profiles
```csharp
var steamWebApi = new SteamWebApi();
var profiles = new List<ProfilerContext>();

foreach (var member in lobbyMembers)
{
    var profile = await steamWebApi.GetUserData(member.UserId);
    if (profile != null)
    {
        profiles.Add(profile);
    }
}
```

### With Error Handling
```csharp
try
{
    var profile = await steamWebApi.GetUserData(userId);
    
    if (profile != null)
    {
        DisplayProfile(profile);
    }
    else
    {
        Logger.Warning($"Profile not found for user {userId}");
        DisplayDefaultAvatar();
    }
}
catch (Exception ex)
{
    Logger.Error($"Failed to fetch profile: {ex.Message}");
}
```

## Integration with Lobby System

```csharp
public async Task LoadLobbyAvatars(List<LobbyData> members)
{
    var steamWebApi = new SteamWebApi();
    
    foreach (var member in members)
    {
        var profile = await steamWebApi.GetUserData(member.UserId);
        
        if (profile != null)
        {
            var avatar = await SteamService.Instance.GetImage(profile.avatarUrl);
            DisplayAvatar(avatar);
        }
    }
}
```

## Steam Web API Details

### Authentication
Uses API key for authentication (passed as query parameter).

### Rate Limiting
Steam Web API has rate limits:
- 100,000 calls per day per API key
- Recommended: Cache results

### Available Data
GetPlayerSummaries returns:
- steamid
- personaname (display name)
- profileurl
- avatar (small 32x32)
- avatarmedium (medium 64x64)
- avatarfull (large 184x184)
- personastate (online status)
- communityvisibilitystate
- profilestate
- lastlogoff
- commentpermission

**Current Implementation**: Only extracts avatar URL

## Features

- **Async/Await**: Non-blocking API calls
- **JSON Parsing**: JsonDocument for response parsing
- **Error Logging**: Detailed debug and error logs
- **Null Safety**: Returns null on failure

## Limitations

- Only retrieves avatar URL (other data ignored)
- No caching (repeated calls for same user)
- No retry logic
- No timeout configuration
- API key hardcoded
- Single user per request (no batch support)
- HttpClient not disposed (should be static or injected)

## Security Considerations

⚠️ **API Key Exposure**: API key is hardcoded in source code.

**Recommendations**:
1. Move to configuration file
2. Use environment variables
3. Implement key rotation
4. Restrict key permissions

## Potential Improvements

```csharp
public class SteamWebApi
{
    private static readonly HttpClient _client = new();
    private readonly string _apiKey;
    private Dictionary<ulong, ProfilerContext> _cache = new();
    
    public SteamWebApi(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public async Task<ProfilerContext?> GetUserData(ulong userId, bool useCache = true)
    {
        if (useCache && _cache.TryGetValue(userId, out var cached))
            return cached;
        
        try
        {
            var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={userId}";
            
            var response = await _client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Steam API returned {response.StatusCode} for user {userId}");
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var player = doc.RootElement
                .GetProperty("response")
                .GetProperty("players")[0];
            
            var profile = new ProfilerContext
            {
                MemberID = userId,
                avatarUrl = player.GetProperty("avatarfull").GetString() ?? "",
                personaName = player.GetProperty("personaname").GetString() ?? "",
                profileUrl = player.GetProperty("profileurl").GetString() ?? ""
            };
            
            if (useCache)
                _cache[userId] = profile;
            
            return profile;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get user data: {ex.Message}");
            return null;
        }
    }
    
    public async Task<List<ProfilerContext>> GetMultipleUserData(IEnumerable<ulong> userIds)
    {
        var ids = string.Join(",", userIds);
        var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={ids}";
        
        // Parse multiple players...
    }
}
```

## Best Practices

1. **Cache Results**: Avoid redundant API calls
2. **Batch Requests**: Use comma-separated IDs for multiple users
3. **Handle Failures**: Always check for null returns
4. **Rate Limit**: Implement request throttling
5. **Secure API Key**: Use configuration management
