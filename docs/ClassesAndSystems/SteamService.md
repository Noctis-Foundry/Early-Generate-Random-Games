# SteamService

## Overview
Singleton service for downloading and converting web images to Avalonia Bitmap format. Primarily used for Steam game header images.

## Purpose
- Download images from URLs
- Convert to Avalonia Bitmap
- Singleton access pattern
- Error handling for network requests

## Properties

### Instance
Static singleton instance.

**Type**: `SteamService`

**Access**: `SteamService.Instance`

**Pattern**: Eager initialization

## Methods

### GetImage(string imageUrl)
Downloads image from URL and converts to Avalonia Bitmap.

**Parameters**:
- `imageUrl` - HTTP/HTTPS URL to image

**Returns**: `Task<Bitmap?>` - Bitmap or null on error

**Process**:
1. Create HttpClient
2. Send GET request to URL
3. Read response as byte array
4. Create MemoryStream from bytes
5. Construct Bitmap from stream
6. Return bitmap

**Error Handling**:
- Returns null on any exception
- Logs exception to console
- Safe for invalid URLs or network errors

## Usage Examples

### Load Game Header
```csharp
string headerUrl = "https://cdn.akamai.steamstatic.com/steam/apps/570/header.jpg";
Bitmap? headerImage = await SteamService.Instance.GetImage(headerUrl);

if (headerImage != null)
{
    gameImage.Source = headerImage;
}
else
{
    gameImage.Source = defaultImage;
}
```

### Load Multiple Images
```csharp
var tasks = games.Select(game => 
    SteamService.Instance.GetImage(game.HeaderImage)
);

Bitmap?[] images = await Task.WhenAll(tasks);

for (int i = 0; i < images.Length; i++)
{
    if (images[i] != null)
    {
        imageControls[i].Source = images[i];
    }
}
```

### With Error Handling
```csharp
try
{
    var bitmap = await SteamService.Instance.GetImage(imageUrl);
    
    if (bitmap != null)
    {
        DisplayImage(bitmap);
    }
    else
    {
        Logger.Warning($"Failed to load image: {imageUrl}");
        DisplayPlaceholder();
    }
}
catch (Exception ex)
{
    Logger.Error($"Image load error: {ex.Message}");
}
```

## Integration with AppSavedContext

```csharp
public async Task DisplayGame(AppSavedContext game)
{
    // Load game header image
    var headerBitmap = await SteamService.Instance.GetImage(game.HeaderImage);
    
    if (headerBitmap != null)
    {
        gameHeaderImage.Source = headerBitmap;
    }
    
    gameTitle.Text = game.AppName;
    gameDescription.Text = game.AppDescription;
}
```

## Singleton Pattern

### Access
```csharp
// Always use Instance property
var service = SteamService.Instance;
var bitmap = await service.GetImage(url);

// Don't create new instances
// var service = new SteamService(); // ❌ Possible but not intended
```

### Benefits
- Single HttpClient management
- Consistent access point
- Reduced memory overhead

## Features

- **Async/Await**: Non-blocking image downloads
- **Null Safety**: Returns null instead of throwing
- **Automatic Disposal**: Using statements for resource cleanup
- **Format Agnostic**: Supports any image format Avalonia can decode
- **Singleton**: Single instance across application

## Supported Image Formats

Avalonia Bitmap supports:
- JPEG
- PNG
- BMP
- GIF
- TIFF
- WebP (platform-dependent)

## Performance Considerations

### Caching
```csharp
private Dictionary<string, Bitmap> _imageCache = new();

public async Task<Bitmap?> GetImageCached(string url)
{
    if (_imageCache.TryGetValue(url, out var cached))
        return cached;
    
    var bitmap = await SteamService.Instance.GetImage(url);
    if (bitmap != null)
        _imageCache[url] = bitmap;
    
    return bitmap;
}
```

### Parallel Loading
```csharp
// Load multiple images concurrently
var imageUrls = games.Select(g => g.HeaderImage);
var bitmaps = await Task.WhenAll(
    imageUrls.Select(url => SteamService.Instance.GetImage(url))
);
```

## Error Scenarios

**Network Errors**:
- No internet connection
- DNS resolution failure
- Connection timeout

**HTTP Errors**:
- 404 Not Found
- 403 Forbidden
- 500 Server Error

**Image Errors**:
- Invalid image format
- Corrupted data
- Unsupported encoding

All scenarios return null and log exception.

## Best Practices

1. **Check for Null**: Always validate return value
2. **Provide Fallback**: Use placeholder images
3. **Cache Results**: Avoid redundant downloads
4. **Batch Requests**: Use Task.WhenAll for multiple images
5. **Handle Errors**: Log failures for debugging

## Limitations

- No caching (downloads every time)
- No retry logic
- No timeout configuration
- No progress reporting
- Creates new HttpClient per request (not optimal)

## Potential Improvements

```csharp
public class SteamService
{
    private static readonly HttpClient _httpClient = new();
    private Dictionary<string, Bitmap> _cache = new();
    
    public async Task<Bitmap?> GetImage(string imageUrl, bool useCache = true)
    {
        if (useCache && _cache.TryGetValue(imageUrl, out var cached))
            return cached;
        
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            
            var bytes = await response.Content.ReadAsByteArrayAsync();
            using var stream = new MemoryStream(bytes);
            var bitmap = new Bitmap(stream);
            
            if (useCache)
                _cache[imageUrl] = bitmap;
            
            return bitmap;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load image {imageUrl}: {ex.Message}");
            return null;
        }
    }
}
```
