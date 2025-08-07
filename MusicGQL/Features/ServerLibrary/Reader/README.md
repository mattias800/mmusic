# Library Assets HTTP Endpoints

This system serves images, audio files, and other assets from the music library through HTTP endpoints that mirror the cache structure.

## Features

- **Portable Assets**: All assets are stored alongside the JSON metadata in the library
- **Cache-Aligned URLs**: URL structure matches the `ServerLibraryCache` lookup system  
- **Multiple Content Types**: Supports images (JPG, PNG, GIF, WebP) and audio (MP3, WAV, FLAC, M4A, OGG)
- **Multiple Photo Types**: Supports thumbs, backgrounds, banners, and logos
- **Flexible Extensions**: Works with or without file extensions in URLs
- **Direct File Streaming**: Efficient file streaming from disk

## URL Structure

### Artist Photos by Type
```
GET /library/{artistId}/photos/{photoType}/{photoIndex}
GET /library/{artistId}/photos/{photoType}/{photoIndex}.{extension}
```

**Photo Types:**
- `thumbs` - Thumbnail photos
- `backgrounds` - Background images  
- `banners` - Banner images
- `logos` - Logo images

**Examples:**
- `/library/Matt%20%26%20Dyle/photos/thumbs/0` - First thumbnail
- `/library/Matt%20%26%20Dyle/photos/backgrounds/0.jpg` - First background with extension
- `/library/Matt%20%26%20Dyle/photos/banners/1` - Second banner
- `/library/Matt%20%26%20Dyle/photos/logos/0.png` - First logo with extension

### Backward Compatibility (Thumbnails Only)
```
GET /library/{artistId}/photos/thumbs/{photoIndex}
GET /library/{artistId}/photos/thumbs/{photoIndex}.{extension}
```

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

### Artist Photos by Type
1. Reads `artist.json` from `Library/{artistId}/`
2. Looks up the appropriate photo array based on type:
   - `thumbs` → `photos.thumbs[photoIndex]`
   - `backgrounds` → `photos.backgrounds[photoIndex]`
   - `banners` → `photos.banners[photoIndex]`
   - `logos` → `photos.logos[photoIndex]`
3. Resolves relative path (handles `./` prefix)
4. Serves the image file

### Release Cover Art
1. Reads `release.json` from `Library/{artistId}/{releaseFolderName}/`
2. Looks for common cover art files: `cover.jpg`, `cover.png`, `cover.jpeg`, `folder.jpg`, `folder.png`
3. Serves the first found cover art file

### Track Audio
1. Reads `release.json` from `Library/{artistId}/{releaseFolderName}/`
2. Finds track with matching `trackNumber` in `tracks` array
3. Uses `audioFilePath` property to locate the audio file
4. Resolves relative path and serves the audio file

## Artist JSON Structure

The `artist.json` file supports multiple photo types:

```json
{
  "id": "Matt & Dyle",
  "name": "Matt & Dyle",
  "photos": {
    "thumbs": ["./thumb1.png", "./thumb2.jpg"],
    "backgrounds": ["./bg1.jpg", "./bg2.png"], 
    "banners": ["./banner1.png"],
    "logos": ["./logo.svg", "./logo_hd.png"]
  }
}
```

## GraphQL Integration

The `ArtistImages` GraphQL type provides URLs for all photo types:

```graphql
type ArtistImages {
  # Lists of all photos by type
  thumbs: [String!]!
  backgrounds: [String!]!
  banners: [String!]!
  logos: [String!]!
  
  # First photo of each type (for convenience)
  firstThumb: String
  firstBackground: String  
  firstBanner: String
  firstLogo: String
}
```

**Usage in GraphQL queries:**
```graphql
query GetArtist($id: ID!) {
  serverLibrary {
    artistById(id: $id) {
      name
      images {
        thumbs      # ["http://localhost:5000/library/Matt%20%26%20Dyle/photos/thumbs/0"]
        backgrounds # ["http://localhost:5000/library/Matt%20%26%20Dyle/photos/backgrounds/0"]
        firstThumb  # "http://localhost:5000/library/Matt%20%26%20Dyle/photos/thumbs/0"
      }
    }
  }
}
```

**Usage in React/HTML:**
```jsx
const artist = data.serverLibrary.artistById;

return (
  <div>
    {/* Show first thumbnail */}
    {artist.images?.firstThumb && (
      <img src={artist.images.firstThumb} alt={`${artist.name} thumbnail`} />
    )}
    
    {/* Show all backgrounds */}
    {artist.images?.backgrounds.map((bgUrl, index) => (
      <img key={index} src={bgUrl} alt={`${artist.name} background ${index + 1}`} />
    ))}
  </div>
);
```

## Content Types & Error Responses

Same as before - automatic content type detection and proper 404/400 responses.

## Benefits

1. **📸 Complete Photo Support**: All four photo types (thumbs, backgrounds, banners, logos)
2. **🎨 Rich UI Possibilities**: Multiple images per type for varied display options
3. **📁 Portable**: Assets stored with metadata, no external dependencies
4. **⚡ Performance**: Direct file streaming, efficient for large images
5. **🔗 GraphQL Ready**: Easy integration with existing schema
6. **🛡️ Type Safety**: Validates photo types and provides clear error messages

This system now provides complete support for all artist photo types while maintaining the portable, self-contained nature of the JSON-based library structure. 