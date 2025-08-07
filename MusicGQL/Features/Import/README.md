# Music Library Import System

A comprehensive system for importing artist and release data from multiple external services into the local music library.

## 🎯 Overview

The import system fetches metadata from external services, downloads assets, and creates local JSON files in the library folder structure. It supports importing from:

- **🎵 MusicBrainz** - Primary metadata source for artists and releases
- **🎧 Spotify** - Additional artist information and popularity data
- **🖼️ fanart.tv** - High-quality artist photos and album cover art

## 🏗️ Architecture

```
LibraryImportService (Main orchestrator)
├── MusicBrainzImportService (Artist/release metadata)
├── SpotifyImportService (Artist matching)
└── FanArtDownloadService (Photo/cover art download)
```

## 🚀 Quick Start

### Import an Artist (Ghost)

**Via GraphQL:**
```graphql
mutation {
  importArtist(artistName: "Ghost") {
    success
    artistId
    musicBrainzId
    spotifyId
    photosDownloaded {
      thumbs
      backgrounds
      banners
      logos
    }
    errorMessage
  }
}
```

**Via C# Code:**
```csharp
var result = await importService.ImportArtistAsync("Ghost");
if (result.Success) {
    Console.WriteLine($"Imported artist: {result.ArtistJson.Name}");
    Console.WriteLine($"MusicBrainz ID: {result.MusicBrainzId}");
    Console.WriteLine($"Spotify ID: {result.SpotifyId}");
}
```

### Import Artist's Releases

**Via GraphQL:**
```graphql
mutation {
  importArtistReleases(artistId: "Ghost") {
    success
    totalReleases
    successfulReleases
    failedReleases
    importedReleases {
      success
      title
      errorMessage
    }
  }
}
```

## 📋 Import Process

### Artist Import Flow

1. **🔍 MusicBrainz Search**
   - Search for artist by name
   - Select best match (first result)
   - Extract artist metadata

2. **🎵 Spotify Matching**
   - Search Spotify for matching artist
   - Use exact name match or popularity ranking
   - Extract Spotify ID and metadata

3. **📁 Folder Creation**
   - Sanitize artist name for folder usage
   - Create `./Library/{ArtistName}/` directory
   - Check for existing folders to avoid duplicates

4. **🖼️ Photo Download**
   - Fetch artist images from fanart.tv using MusicBrainz ID
   - Download multiple photo types:
     - **Thumbs** (up to 5 images)
     - **Backgrounds** (up to 3 images)
     - **Banners** (up to 3 images)
     - **Logos** (up to 3 regular + 2 HD images)

5. **📄 JSON Creation**
   - Create `artist.json` with complete metadata
   - Include service connections (MusicBrainz, Spotify)
   - Reference downloaded photo files

6. **🔄 Cache Update**
   - Refresh the in-memory cache
   - Artist immediately available via GraphQL

### Release Import Flow

1. **🎭 Artist Lookup**
   - Find artist in local cache
   - Extract MusicBrainz artist ID

2. **📀 Release Group Fetching**
   - Get all release groups from MusicBrainz
   - Filter by artist ID

3. **💿 Release Processing**
   - For each release group:
     - Get detailed releases with track listings
     - Select primary release (usually first)
     - Map release type (Album/EP/Single)

4. **📁 Folder Structure**
   - Create `./Library/{ArtistName}/{ReleaseName}/`
   - Sanitize release names for filesystem

5. **🎨 Cover Art Download**
   - Fetch album cover from fanart.tv
   - Download as `cover.jpg/png`
   - Store in release folder

6. **📋 Track Metadata**
   - Extract track listings from MusicBrainz
   - Include track numbers, titles, lengths
   - No audio files downloaded (metadata only)

7. **📄 Release JSON**
   - Create `release.json` with complete metadata
   - Reference cover art file
   - Include track listings

## 🗂️ File Structure

After importing "Ghost" with their album "Opus Eponymous":

```
Library/
└── Ghost/
    ├── artist.json
    ├── thumb_00.jpg
    ├── background_00.jpg
    ├── banner_00.jpg
    ├── logo_00.png
    └── Opus Eponymous/
        ├── release.json
        ├── cover.jpg
        └── (future: audio files)
```

## 📊 Import Results

### Artist Import Result
```csharp
public class ArtistImportResult
{
    public bool Success { get; set; }
    public string ArtistName { get; set; }
    public string? ArtistId { get; set; }          // Folder name
    public string? MusicBrainzId { get; set; }     // External ID
    public string? SpotifyId { get; set; }         // External ID
    public PhotosDownloaded PhotosDownloaded { get; set; }
    public ArtistJson? ArtistJson { get; set; }    // Complete metadata
    public string? ErrorMessage { get; set; }
}
```

### Release Import Result
```csharp
public class ReleaseImportResult
{
    public bool Success { get; set; }
    public string ArtistId { get; set; }
    public int TotalReleases { get; set; }
    public int SuccessfulReleases { get; set; }
    public int FailedReleases { get; set; }
    public List<ImportedRelease> ImportedReleases { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## 🔧 Configuration & Dependencies

### Service Registration (Program.cs)
```csharp
builder.Services
    .AddScoped<MusicBrainzImportService>()
    .AddScoped<SpotifyImportService>()
    .AddScoped<FanArtDownloadService>()
    .AddScoped<LibraryImportService>();
```

### GraphQL Registration
```csharp
.AddTypeExtension<ImportArtistMutation>()
```

### Required External Services
- **MusicBrainzService** - Existing integration
- **SpotifyService** - Existing integration  
- **IFanArtTVClient** - FanArt.tv API client
- **HttpClient** - For downloading images
- **ServerLibraryCache** - For updating local cache

## 🛠️ Error Handling

### Graceful Failures
- **Artist not found**: Returns error message, no cleanup needed
- **Spotify unavailable**: Continues without Spotify data
- **Photo download fails**: Continues with available photos
- **Release import fails**: Imports successful releases, reports failed ones

### Cleanup on Error
- **Artist folder cleanup**: Removes created folder if import fails
- **Release folder cleanup**: Removes individual release folders on failure
- **Partial success**: Keeps successfully imported releases

## 🧪 Testing

### Manual Testing
```csharp
// Test single artist
await TestImport.TestImportGhostAsync(importService);

// Test multiple artists
await TestImport.TestImportMultipleArtistsAsync(importService);
```

### GraphQL Testing
Use the provided mutations in `ImportTestQueries`:
- `ImportArtistMutation` - Import single artist
- `ImportReleasesMutation` - Import artist releases
- `QueryImportedData` - Verify imported data

## 📝 JSON Schema

### artist.json
```json
{
  "id": "Ghost",
  "name": "Ghost",
  "sortName": "Ghost",
  "photos": {
    "thumbs": ["./thumb_00.jpg", "./thumb_01.jpg"],
    "backgrounds": ["./background_00.jpg"],
    "banners": ["./banner_00.jpg"],
    "logos": ["./logo_00.png"]
  },
  "connections": {
    "musicBrainzArtistId": "b70c38d8-392a-4013-8e8e-bf35b2cc6c1e",
    "spotifyId": "1Qp56T7n950O3EGMsSl81D"
  }
}
```

### release.json
```json
{
  "title": "Opus Eponymous",
  "sortTitle": "Opus Eponymous",
  "type": "album",
  "firstReleaseDate": "2010-10-18",
  "coverArt": "./cover.jpg",
  "tracks": [
    {
      "title": "Deus Culpa",
      "trackNumber": 1,
      "trackLength": 91000
    }
  ]
}
```

## 🔮 Future Enhancements

### Phase 2: Audio Files
- Download audio previews from Spotify
- Support for full audio file imports
- Audio format conversion and optimization

### Phase 3: Additional Services
- **Discogs** - Vinyl/physical release data
- **Last.fm** - Listening statistics and recommendations  
- **Bandcamp** - Independent artist support
- **YouTube** - Music video and live performance links

### Phase 4: Smart Matching
- Fuzzy name matching for better artist detection
- Duplicate detection and merging
- Release variant selection (remastered, deluxe, etc.)

## 📚 API Reference

### LibraryImportService
- `ImportArtistAsync(artistName)` - Import single artist
- `ImportArtistReleasesAsync(artistId)` - Import artist's releases

### MusicBrainzImportService  
- `SearchArtistsAsync(artistName)` - Search artists
- `GetArtistByIdAsync(mbId)` - Get artist details
- `GetArtistReleaseGroupsAsync(mbId)` - Get release groups
- `GetReleaseGroupReleasesAsync(rgId)` - Get releases with tracks

### SpotifyImportService
- `SearchArtistsAsync(artistName)` - Search Spotify artists
- `GetArtistByIdAsync(spotifyId)` - Get artist details
- `FindBestMatchAsync(artistName)` - Find best matching artist

### FanArtDownloadService
- `DownloadArtistPhotosAsync(mbId, folderPath)` - Download artist photos
- `DownloadReleaseCoverArtAsync(rgId, folderPath)` - Download cover art

This import system provides a robust foundation for building a comprehensive music library from external metadata sources! 