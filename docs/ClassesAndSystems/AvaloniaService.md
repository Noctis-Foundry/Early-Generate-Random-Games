# AvaloniaService

## Overview
Static utility service for Avalonia UI bitmap operations. Handles image format conversion and Steam avatar rendering.

## Methods

### CreateBitmap(byte[] rawRgba, int width, int height)
Converts raw RGBA byte array to Avalonia Bitmap with BGRA format.

**Parameters**:
- `rawRgba` - Raw image data in RGBA format
- `width` - Image width in pixels
- `height` - Image height in pixels

**Returns**: `Bitmap` - Avalonia bitmap in BGRA8888 format

**Process**:
1. Convert RGBA to BGRA by swapping R and B channels
2. Create WriteableBitmap with specified dimensions
3. Copy converted data to bitmap buffer
4. Return bitmap

**Color Conversion**:
```
RGBA → BGRA
R → B (index 0 → 2)
G → G (index 1 → 1)
B → R (index 2 → 0)
A → A (index 3 → 3)
```

### CreateSteamImage(int image)
Creates Avalonia Bitmap from Steam image handle.

**Parameters**:
- `image` - Steam image handle ID

**Returns**: `Bitmap?` - Bitmap or null if invalid handle

**Process**:
1. Validate image handle (returns null if 0)
2. Get image dimensions from Steam API
3. Retrieve RGBA data from Steam
4. Convert to Avalonia Bitmap using CreateBitmap

**Steam API Calls**:
- `SteamUtils.GetImageSize()` - Get width/height
- `SteamUtils.GetImageRGBA()` - Get raw pixel data

## Usage Examples

### Steam Avatar
```csharp
int avatarHandle = SteamFriends.GetLargeFriendAvatar(steamId);
Bitmap? avatar = AvaloniaService.CreateSteamImage(avatarHandle);

if (avatar != null)
{
    avatarImage.Source = avatar;
}
```

### Manual Bitmap Creation
```csharp
byte[] rgbaData = GetImageData();
Bitmap bitmap = AvaloniaService.CreateBitmap(rgbaData, 128, 128);
```

## Technical Details

**DPI**: 96x96 (standard screen DPI)

**Pixel Format**: BGRA8888 (8 bits per channel)

**Memory Management**: Uses WriteableBitmap.Lock() for safe buffer access

## Dependencies
- Avalonia.Media.Imaging
- Steamworks.NET (SteamUtils API)
