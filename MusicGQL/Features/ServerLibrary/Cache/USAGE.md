# ServerLibraryCache - Dependency Injection Usage

The `ServerLibraryCache` is now registered as a singleton in the dependency injection container and ready to use throughout your application.

## Dependency Injection Setup

The following services are registered in `Program.cs`:

```csharp
builder.Services
    .AddSingleton<ServerLibraryJsonReader>()
    .AddSingleton<ServerLibraryCache>();
```

## Usage in GraphQL Resolvers

You can inject the cache into any GraphQL resolver using the `[Service]` attribute:

```csharp
[ExtendObjectType<Query>]
public class MySearchRoot
{
    public async Task<List<CachedArtist>> SearchArtists(
        [Service] ServerLibraryCache cache,
        string searchTerm
    )
    {
        return await cache.SearchArtistsByNameAsync(searchTerm);
    }
}
```

## Usage in Controllers/Services

You can inject the cache into any controller or service:

```csharp
[ApiController]
public class MusicController : ControllerBase
{
    private readonly ServerLibraryCache _cache;

    public MusicController(ServerLibraryCache cache)
    {
        _cache = cache;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchArtists(string query)
    {
        var artists = await _cache.SearchArtistsByNameAsync(query);
        return Ok(artists);
    }
}
```

## Available GraphQL Queries

The `ServerLibraryCacheSearchRoot` provides the following GraphQL queries:

### Search Operations
- `searchCachedArtists(searchTerm: String!, limit: Int = 20): [CachedArtist!]!`
- `searchCachedReleases(searchTerm: String!, limit: Int = 20): [CachedRelease!]!`
- `searchCachedTracks(searchTerm: String!, limit: Int = 20): [CachedTrack!]!`

### Get Operations
- `getCachedArtist(artistId: String!): CachedArtist`

### Cache Management
- `getCacheStatistics: CacheStatistics!`
- `updateCache: CacheUpdateResult!`

## Example GraphQL Queries

```graphql
# Search for artists
query SearchArtists {
  searchCachedArtists(searchTerm: "Beatles", limit: 10) {
    id
    name
    releases {
      title
      type
    }
  }
}

# Search for tracks
query SearchTracks {
  searchCachedTracks(searchTerm: "Yesterday", limit: 5) {
    title
    artistName
    releaseTitle
    trackNumber
  }
}

# Get cache statistics
query CacheStats {
  getCacheStatistics {
    artistCount
    releaseCount
    trackCount
    lastUpdated
    isInitialized
  }
}

# Update cache
mutation UpdateCache {
  updateCache {
    success
    message
    statistics {
      artistCount
      releaseCount
      trackCount
    }
  }
}
```

## Performance Benefits

- **No Disk I/O**: All searches operate on in-memory data
- **Fast Lookups**: O(1) exact match lookups using dictionaries
- **Singleton Scope**: Single instance shared across all requests
- **Thread-Safe**: All operations are thread-safe with proper locking
- **Auto-Initialization**: Cache initializes automatically on first use

## Cache Lifecycle

1. **Application Startup**: Services are registered in DI container
2. **First Use**: Cache automatically initializes by reading from disk
3. **Ongoing Operations**: All searches use cached data (no disk I/O)
4. **Manual Updates**: Call `UpdateCacheAsync()` to refresh from disk
5. **Application Shutdown**: Cache is disposed with the DI container

## Best Practices

1. **Initialize Early**: Consider calling `UpdateCacheAsync()` at startup
2. **Monitor Performance**: Use `GetCacheStatisticsAsync()` to track cache status
3. **Update Strategically**: Refresh cache when library changes on disk
4. **Handle Errors**: Always check for null results from lookup methods
5. **Set Limits**: Use appropriate limits for search operations 