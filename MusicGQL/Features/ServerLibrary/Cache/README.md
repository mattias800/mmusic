# ServerLibrary2 Cache System

This caching system provides fast in-memory access to music library data read from JSON files.

## Features

- **Fast Lookups**: All data is stored in memory for instant access
- **Artist Management**: Get artists by ID or name, search by partial name matches
- **Album Management**: Get albums by artist ID + folder name (using actual folder structure)
- **Track Management**: Get tracks by artist ID + album folder name + track number
- **Search Functionality**: Search artists, albums, and tracks by name/title
- **Thread Safe**: All operations are thread-safe using lock mechanisms
- **Auto-Initialization**: Cache automatically initializes on first use

## Folder-Based Lookup System

The cache uses the actual file system folder structure for lookups instead of generated IDs:

- **Releases**: Identified by `artistId` + `releaseFolderName` (e.g., "Matt & Dyle" + "Demo EP")
- **Tracks**: Identified by `artistId` + `releaseFolderName` + `trackNumber`
- **Natural Structure**: Mirrors the actual Library/{Artist}/{Album}/ folder organization

## Usage Examples

### Basic Usage

```csharp
// Get the cache from DI
var cache = serviceProvider.GetRequiredService<ServerLibraryCache>();

// Get an artist by ID
var artist = await cache.GetArtistByIdAsync("artist-id");

// Get all releases for an artist
var releases = await cache.GetAllReleasesForArtistAsync("artist-id");

// Get a specific release by artist ID + folder name
var release = await cache.GetReleaseByArtistAndFolderAsync("artist-id", "release-folder");

// Get all tracks for a release
var tracks = await cache.GetAllTracksForReleaseAsync("artist-id", "release-folder");

// Get a specific track by artist + release + track number
var track = await cache.GetTrackByArtistReleaseAndNumberAsync("artist-id", "release-folder", 3);

// Get all tracks for an artist
var allTracks = await cache.GetTracksByArtistIdAsync("artist-id");
```

### Real-World Example

```csharp
// Working with "Matt & Dyle" artist and "Demo EP" release
var artist = await cache.GetArtistByIdAsync("Matt & Dyle");
var demoEP = await cache.GetReleaseByArtistAndFolderAsync("Matt & Dyle", "Demo EP");
var tracks = await cache.GetAllTracksForReleaseAsync("Matt & Dyle", "Demo EP");
var track2 = await cache.GetTrackByArtistReleaseAndNumberAsync("Matt & Dyle", "Demo EP", 2);
```

### Search Operations

```csharp
// Search for artists
var artists = await cache.SearchArtistsByNameAsync("Beatles", limit: 10);

// Search for albums
var albums = await cache.SearchReleasesByTitleAsync("Abbey Road", limit: 10);

// Search for tracks
var tracks = await cache.SearchTracksByTitleAsync("Yesterday", limit: 10);
```

### Cache Management

```csharp
// Update cache from disk
await cache.UpdateCacheAsync();

// Get cache statistics
var stats = await cache.GetCacheStatisticsAsync();
Console.WriteLine($"Cache contains {stats.ArtistCount} artists, {stats.ReleaseCount} releases, {stats.TrackCount} tracks");
```

## Data Models

### CachedArtist
- Contains artist metadata and list of releases
- Indexed by ID and name for fast lookups

### CachedRelease  
- Contains release metadata and list of tracks
- Includes `FolderName` property matching the actual folder name
- Linked to artist via `ArtistId`

### CachedTrack
- Contains track metadata
- Includes `ReleaseFolderName` for linking to release
- Linked to both artist and release via natural identifiers

## Available Methods

### Artist Methods
- `GetArtistByIdAsync(id)` - Get artist by ID
- `GetArtistByNameAsync(name)` - Get artist by exact name match
- `SearchArtistsByNameAsync(searchTerm, limit)` - Search artists by partial name
- `GetAllArtistsAsync()` - Get all artists

### Release Methods  
- `GetReleaseByArtistAndFolderAsync(artistId, releaseFolderName)` - Get release by artist + folder name
- `GetAllReleasesForArtistAsync(artistId)` - Get all releases for an artist
- `SearchReleasesByTitleAsync(searchTerm, limit)` - Search releases by title
- `GetAllReleasesAsync()` - Get all releases

### Track Methods
- `GetAllTracksForReleaseAsync(artistId, releaseFolderName)` - Get all tracks for a release
- `GetTrackByArtistReleaseAndNumberAsync(artistId, releaseFolderName, trackNumber)` - Get specific track
- `GetTracksByArtistIdAsync(artistId)` - Get all tracks for an artist  
- `SearchTracksByTitleAsync(searchTerm, limit)` - Search tracks by title
- `GetAllTracksAsync()` - Get all tracks

### Utility Methods
- `UpdateCacheAsync()` - Refresh cache from disk
- `GetCacheStatisticsAsync()` - Get cache statistics

## Folder Structure Mapping

The cache directly maps to your file system structure:

```
Library/
├── Matt & Dyle/                    (Artist ID: "Matt & Dyle")
│   ├── artist.json
│   ├── Demo EP/                    (Album Folder: "Demo EP")
│   │   └── release.json
│   └── Example Album/              (Album Folder: "Example Album")
│       └── release.json
└── Another Artist/                 (Artist ID: "Another Artist")
    ├── artist.json
    └── Their Album/                (Album Folder: "Their Album")
        └── release.json
```

Cache lookup examples:
- `GetReleaseByArtistAndFolderAsync("Matt & Dyle", "Demo EP")`
- `GetAllTracksForReleaseAsync("Matt & Dyle", "Example Album")`
- `GetTrackByArtistReleaseAndNumberAsync("Another Artist", "Their Album", 1)`

## Benefits

- **Natural Navigation**: Uses actual folder names that users can see
- **No ID Generation**: No need to generate or remember artificial IDs
- **Intuitive**: Lookup parameters match the real file system structure
- **Efficient**: O(1) lookups using nested dictionaries
- **File System Aligned**: Cache structure mirrors disk organization

## Thread Safety

All cache operations are thread-safe and can be called concurrently from multiple threads. The cache uses internal locking to ensure data consistency.

## Performance

- **Memory Usage**: All data is kept in memory for fast access
- **Lookup Speed**: O(1) for folder-based lookups, O(log n) for searches
- **Thread Safety**: Minimal locking overhead for read operations 