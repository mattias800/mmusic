# ServerLibrary2 Cache System

This cache system provides fast, in-memory access to music library metadata without requiring disk I/O for searches.

## Overview

The cache loads all artist, release, and track metadata from the JSON files on disk into memory for fast searching and retrieval.

## Key Components

- **`ServerLibraryCache`** - Main cache service with search and retrieval methods
- **`CachedArtist`** - In-memory representation of an artist with releases
- **`CachedRelease`** - In-memory representation of a release/album with tracks  
- **`CachedTrack`** - In-memory representation of a track
- **`CacheStatistics`** - Cache statistics and status information

## Usage

### Setup

```csharp
// Create the reader and cache
var reader = new ServerLibraryJsonReader();
var cache = new ServerLibraryCache(reader);

// Initialize cache (loads all data from disk)
await cache.UpdateCacheAsync();
```

### Artist Operations

```csharp
// Get artist by exact ID
var artist = await cache.GetArtistByIdAsync("Matt & Dyle");

// Get artist by exact name (case-insensitive)
var artist = await cache.GetArtistByNameAsync("Matt & Dyle");

// Search artists by partial name match
var artists = await cache.SearchArtistsByNameAsync("Matt", limit: 10);

// Get all artists
var allArtists = await cache.GetAllArtistsAsync();
```

### Release/Album Operations

```csharp
// Search releases by title
var releases = await cache.SearchReleasesByTitleAsync("album title", limit: 20);

// Get all releases
var allReleases = await cache.GetAllReleasesAsync();

// Access releases through artist
var artist = await cache.GetArtistByNameAsync("Artist Name");
var artistReleases = artist?.Releases ?? new List<CachedRelease>();
```

### Track Operations

```csharp
// Search tracks by title
var tracks = await cache.SearchTracksByTitleAsync("song title", limit: 30);

// Get all tracks
var allTracks = await cache.GetAllTracksAsync();

// Access tracks through release
var releases = await cache.SearchReleasesByTitleAsync("album");
var tracks = releases.FirstOrDefault()?.Tracks ?? new List<CachedTrack>();
```

### Cache Management

```csharp
// Update cache (re-read from disk)
await cache.UpdateCacheAsync();

// Get cache statistics
var stats = await cache.GetCacheStatisticsAsync();
Console.WriteLine($"Artists: {stats.ArtistCount}, Releases: {stats.ReleaseCount}, Tracks: {stats.TrackCount}");
Console.WriteLine($"Last updated: {stats.LastUpdated}");

// Check if cache is initialized
if (cache.IsInitialized)
{
    // Cache is ready to use
}
```

## Features

### Thread-Safe
All operations are thread-safe and can be called from multiple threads simultaneously.

### Auto-Initialization
The cache automatically initializes itself on first use. You can also manually trigger updates.

### Case-Insensitive Search
All searches are case-insensitive and support partial matching.

### Smart Ordering
Search results prioritize exact matches and "starts with" matches over partial matches.

### Error Handling
Cache updates handle errors gracefully and don't clear existing cache data on failure.

### Performance
- In-memory operations provide fast search and retrieval
- No disk I/O required for searches once cache is loaded
- Efficient indexing by ID and name for O(1) lookups

## Data Structure

Each cached item contains:
- Original JSON data for full metadata access
- Flattened properties for easy access
- Pre-computed lowercase search terms for fast searching
- Hierarchical relationships (Artist → Releases → Tracks)

## Best Practices

1. **Initialize Early**: Call `UpdateCacheAsync()` at application startup
2. **Periodic Updates**: Refresh cache when library changes on disk
3. **Monitor Performance**: Use `GetCacheStatisticsAsync()` to monitor cache status
4. **Handle Nulls**: Always check for null results from lookup methods
5. **Use Limits**: Specify reasonable limits for search operations to avoid performance issues 