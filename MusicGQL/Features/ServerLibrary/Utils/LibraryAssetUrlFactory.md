# LibraryAssetUrlFactory

A centralized factory for generating library asset URLs using modern C# features.

## Features

- **🏭 Centralized URL Generation**: All asset URL creation in one place
- **🔗 Consistent URL Patterns**: Ensures consistent URL structure across the application
- **⚡ Performance**: Uses modern C# collection expressions and efficient LINQ
- **🛡️ URL Encoding**: Automatic URL encoding for artist names and folder names
- **📱 GraphQL Ready**: Direct integration with GraphQL resolvers

## Collection Expressions Usage

The factory leverages C# 12 collection expressions for cleaner, more performant code:

```csharp
// Empty collections
if (photoCount <= 0) return [];

// Array initialization  
string[] validPhotoTypes = ["thumbs", "backgrounds", "banners", "logos"];

// Collection spread with LINQ
return [.. Enumerable.Range(0, photoCount)
    .Select(index => $"/library/{escapedArtistId}/photos/{photoType}/{index}")];

// Dictionary initialization with collection expressions
return new Dictionary<string, List<string>>
{
    ["thumbs"] = CreateArtistPhotoUrls(artistId, "thumbs", photos.Thumbs?.Count ?? 0),
    ["backgrounds"] = CreateArtistPhotoUrls(artistId, "backgrounds", photos.Backgrounds?.Count ?? 0),
    ["banners"] = CreateArtistPhotoUrls(artistId, "banners", photos.Banners?.Count ?? 0),
    ["logos"] = CreateArtistPhotoUrls(artistId, "logos", photos.Logos?.Count ?? 0)
};
```

## API Reference

### Artist Photo URLs

#### `CreateArtistPhotoUrls(artistId, photoType, photoCount)`
Generates a list of photo URLs for a specific type:
```csharp
var thumbUrls = LibraryAssetUrlFactory.CreateArtistPhotoUrls("Matt & Dyle", "thumbs", 3);
// Returns: ["/library/Matt%20%26%20Dyle/photos/thumbs/0", 
//           "/library/Matt%20%26%20Dyle/photos/thumbs/1",
//           "/library/Matt%20%26%20Dyle/photos/thumbs/2"]
```

#### Individual Photo Type Methods
- `CreateArtistThumbnailUrl(artistId, photoIndex = 0)`
- `CreateArtistBackgroundUrl(artistId, photoIndex = 0)`  
- `CreateArtistBannerUrl(artistId, photoIndex = 0)`
- `CreateArtistLogoUrl(artistId, photoIndex = 0)`

#### `CreateAllArtistPhotoUrls(artistId, photos)`
Generates URLs for all available photo types:
```csharp
var allUrls = LibraryAssetUrlFactory.CreateAllArtistPhotoUrls("Matt & Dyle", artistPhotos);
// Returns: Dictionary with keys "thumbs", "backgrounds", "banners", "logos"
```

### Release & Track URLs

#### `CreateReleaseCoverArtUrl(artistId, releaseFolderName)`
```csharp
var coverUrl = LibraryAssetUrlFactory.CreateReleaseCoverArtUrl("Matt & Dyle", "Demo EP");
// Returns: "/library/Matt%20%26%20Dyle/releases/Demo%20EP/coverart"
```

#### `CreateTrackAudioUrl(artistId, releaseFolderName, trackNumber)`
```csharp
var audioUrl = LibraryAssetUrlFactory.CreateTrackAudioUrl("Matt & Dyle", "Demo EP", 1);
// Returns: "/library/Matt%20%26%20Dyle/releases/Demo%20EP/tracks/1/audio"
```

## GraphQL Integration

### ArtistImages
```csharp
public record ArtistImages(ArtistPhotosJson Model, string ArtistId)
{
    public List<string> Thumbs() => 
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(ArtistId, "thumbs", Model.Thumbs?.Count ?? 0);
        
    public List<string> Backgrounds() => 
        LibraryAssetUrlFactory.CreateArtistPhotoUrls(ArtistId, "backgrounds", Model.Backgrounds?.Count ?? 0);
        
    // ... etc for banners and logos
}
```

### Release
```csharp
public record Release(CachedRelease Model)
{
    public string CoverArtUrl() => 
        LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(Model.ArtistId, Model.FolderName);
}
```

### Track
```csharp
public record Track(CachedTrack Model)
{
    public string AudioUrl() => 
        LibraryAssetUrlFactory.CreateTrackAudioUrl(Model.ArtistId, Model.ReleaseFolderName, Model.TrackNumber);
}
```

## Modern C# Features Used

### 1. Collection Expressions (C# 12)
- **Empty Collections**: `return [];` instead of `new List<string>()`
- **Array Initialization**: `["thumbs", "backgrounds"]` instead of `new[] { "thumbs", "backgrounds" }`
- **Collection Spread**: `[.. enumerable]` for efficient collection creation

### 2. Static Factory Pattern
- All methods are static for easy access
- No instance state, pure functions
- Thread-safe by design

### 3. Performance Optimizations
- Lazy evaluation with LINQ
- Efficient string formatting
- Minimal allocations with collection expressions

## Benefits

1. **🔧 Maintainability**: URL patterns centralized and easy to update
2. **🧪 Testability**: Pure functions easy to unit test
3. **⚡ Performance**: Modern C# features for optimal performance
4. **🛡️ Type Safety**: Strong typing prevents URL construction errors
5. **📖 Readability**: Clean, expressive code using collection expressions
6. **🔄 Consistency**: All asset URLs follow the same patterns

## Usage Examples

### React Frontend
```jsx
// GraphQL query result
const artist = data.serverLibrary.artistById;

return (
  <div>
    {/* All thumbnails */}
    {artist.images.thumbs().map((url, index) => (
      <img key={index} src={url} alt={`${artist.name} thumbnail ${index + 1}`} />
    ))}
    
    {/* Release cover art */}
    {artist.releases.map(release => (
      <img key={release.id} src={release.coverArtUrl()} alt={release.title} />
    ))}
  </div>
);
```

### Direct Usage
```csharp
// Generate specific URLs
var thumbnailUrl = LibraryAssetUrlFactory.CreateArtistThumbnailUrl("Matt & Dyle");
var coverArtUrl = LibraryAssetUrlFactory.CreateReleaseCoverArtUrl("Matt & Dyle", "Demo EP");

// Generate URL lists  
var allThumbs = LibraryAssetUrlFactory.CreateArtistPhotoUrls("Matt & Dyle", "thumbs", 5);
```

This factory provides a modern, efficient, and maintainable solution for generating library asset URLs throughout the application. 