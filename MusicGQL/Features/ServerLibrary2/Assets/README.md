# Library Assets HTTP Endpoints

This system serves images, audio files, and other assets from the music library through HTTP endpoints that mirror the cache structure.

## Features

- **Portable Assets**: All assets are stored alongside the JSON metadata in the library
- **Cache-Aligned URLs**: URL structure matches the `ServerLibraryCache` lookup system  
- **Multiple Content Types**: Supports images (JPG, PNG, GIF, WebP) and audio (MP3, WAV, FLAC, M4A, OGG)
- **Flexible Extensions**: Works with or without file extensions in URLs
- **Direct File Streaming**: Efficient file streaming from disk

## URL Structure

### Artist Photos
```
GET /library/{artistId}/photos/thumbPhotos/{photoIndex}
GET /library/{artistId}/photos/thumbPhotos/{photoIndex}.{extension}
```

**Examples:**
- `/library/Matt%20%26%20Dyle/photos/thumbPhotos/0` - First photo (without extension)
- `/library/Matt%20%26%20Dyle/photos/thumbPhotos/0.png` - First photo (with extension)
- `/library/Matt%20%26%20Dyle/photos/thumbPhotos/1.jpg` - Second photo

### Release Cover Art
```
GET /library/{artistId}/releases/{releaseFolderName}/coverart
GET /library/{artistId}/releases/{releaseFolderName}/coverart.{extension}
```

**Examples:**
- `/library/Matt%20%26%20Dyle/releases/Demo%20EP/coverart` - Cover art (without extension)
- `/library/Matt%20%26%20Dyle/releases/Demo%20EP/coverart.jpg` - Cover art (with extension)
- `/library/Matt%20%26%20Dyle/releases/Example%20Album/coverart.png` - Example Album cover

### Track Audio Files
```
GET /library/{artistId}/releases/{releaseFolderName}/tracks/{trackNumber}/audio
```

**Examples:**
- `/library/Matt%20%26%20Dyle/releases/Demo%20EP/tracks/1/audio` - First track
- `/library/Matt%20%26%20Dyle/releases/Demo%20EP/tracks/2/audio` - Second track

## File Resolution

### Artist Photos
1. Reads `artist.json` from `Library/{artistId}/`
2. Looks up `photos.thumbPhotos[photoIndex]` array
3. Resolves relative path (handles `./` prefix)
4. Serves the image file

### Release Cover Art
1. Reads `release.json` from `Library/{artistId}/{releaseFolderName}/`
2. Looks for common cover art files: `cover.jpg`, `cover.png`, `cover.jpeg`, `folder.jpg`, `folder.png`
3. Serves the first found cover art file

*Note: Future enhancement could add explicit cover art references in `release.json`*

### Track Audio
1. Reads `release.json` from `Library/{artistId}/{releaseFolderName}/`
2. Finds track with matching `trackNumber` in `tracks` array
3. Uses `audioFilePath` property to locate the audio file
4. Resolves relative path and serves the audio file

## Content Types

The system automatically detects content types based on file extensions:

**Images:**
- `.jpg`, `.jpeg` → `image/jpeg`
- `.png` → `image/png`  
- `.gif` → `image/gif`
- `.webp` → `image/webp`

**Audio:**
- `.mp3` → `audio/mpeg`
- `.wav` → `audio/wav`
- `.flac` → `audio/flac`
- `.m4a` → `audio/mp4`
- `.ogg` → `audio/ogg`

**Default:** `application/octet-stream`

## URL Encoding

Remember to URL-encode special characters in artist names and folder names:

- `Matt & Dyle` → `Matt%20%26%20Dyle`
- `AC/DC` → `AC%2FDC`
- `Guns N' Roses` → `Guns%20N%27%20Roses`

## Error Responses

- **404 Not Found**: Asset file doesn't exist or isn't referenced in JSON
- **404 Extension Mismatch**: Requested extension doesn't match actual file extension (for extension-specific endpoints)

## Implementation Details

### ServerLibraryAssetReader
Located in `Features/ServerLibrary2/Reader/ServerLibraryAssetReader.cs`

**Key Methods:**
- `GetArtistPhotoAsync(artistId, photoIndex)` - Gets artist photos
- `GetReleaseCoverArtAsync(artistId, releaseFolderName)` - Gets release cover art  
- `GetTrackAudioAsync(artistId, releaseFolderName, trackNumber)` - Gets track audio

### LibraryAssetsController
Located in `Controllers/LibraryAssetsController.cs`

**Features:**
- HTTP endpoint routing
- Content type detection
- File streaming
- Error handling
- Extension validation

## GraphQL Integration

These HTTP endpoints can be used in GraphQL resolvers to provide direct asset URLs:

```csharp
// In Artist GraphQL type
public string? PhotoUrl(int index = 0) 
{
    if (Model.ArtistJson.Photos?.ThumbPhotos?.Count > index)
    {
        return $"/library/{Uri.EscapeDataString(Model.Id)}/photos/thumbPhotos/{index}";
    }
    return null;
}

// In Release GraphQL type  
public string? CoverArtUrl()
{
    return $"/library/{Uri.EscapeDataString(Model.ArtistId)}/releases/{Uri.EscapeDataString(Model.FolderName)}/coverart";
}

// In Track GraphQL type
public string? AudioUrl()
{
    return $"/library/{Uri.EscapeDataString(Model.ArtistId)}/releases/{Uri.EscapeDataString(Model.ReleaseFolderName)}/tracks/{Model.TrackNumber}/audio";
}
```

## File System Structure

The endpoints expect this folder structure:

```
Library/
├── Matt & Dyle/                    (Artist)
│   ├── artist.json                 (References photos)
│   ├── imagematt.png              (Artist photo)
│   ├── Demo EP/                   (Release folder)
│   │   ├── release.json           (References tracks)
│   │   ├── cover.jpg              (Cover art)
│   │   ├── 01-track1.mp3          (Track audio)
│   │   └── 02-track2.mp3
│   └── Example Album/
│       ├── release.json
│       ├── folder.png
│       └── track.wav
```

## Benefits

1. **Portability**: Assets stored with metadata, no external dependencies
2. **Performance**: Direct file streaming, efficient for large audio files
3. **Consistency**: URLs mirror cache structure for intuitive usage
4. **Flexibility**: Works with any file format and naming convention
5. **GraphQL Ready**: Easy integration with GraphQL resolvers

This system provides a complete solution for serving music library assets while maintaining the portable, self-contained nature of the JSON-based library structure. 