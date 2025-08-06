using MusicGQL.Features.ServerLibrary2.Reader;

namespace MusicGQL.Features.ServerLibrary2.Cache;

public class ServerLibraryCache(ServerLibraryJsonReader reader)
{
    private readonly Dictionary<string, CachedArtist> _artistsById = new();
    private readonly Dictionary<string, CachedArtist> _artistsByName = new();
    private readonly List<CachedArtist> _allArtists = new();
    private readonly List<CachedRelease> _allReleases = new();
    private readonly List<CachedTrack> _allTracks = new();
    private readonly object _lockObject = new();
    private DateTime _lastUpdated = DateTime.MinValue;
    private bool _isInitialized = false;

    /// <summary>
    /// Gets when the cache was last updated
    /// </summary>
    public DateTime LastUpdated
    {
        get
        {
            lock (_lockObject)
            {
                return _lastUpdated;
            }
        }
    }

    /// <summary>
    /// Gets whether the cache has been initialized
    /// </summary>
    public bool IsInitialized
    {
        get
        {
            lock (_lockObject)
            {
                return _isInitialized;
            }
        }
    }

    /// <summary>
    /// Updates the cache by reading all data from disk
    /// </summary>
    /// <returns>Task that completes when cache is updated</returns>
    public async Task UpdateCacheAsync()
    {
        var newArtistsById = new Dictionary<string, CachedArtist>();
        var newArtistsByName = new Dictionary<string, CachedArtist>();
        var newAllArtists = new List<CachedArtist>();
        var newAllReleases = new List<CachedRelease>();
        var newAllTracks = new List<CachedTrack>();

        try
        {
            var artistsData = await reader.ReadAllArtistsAsync();

            foreach (var (artistPath, artistJson) in artistsData)
            {
                var cachedArtist = new CachedArtist
                {
                    Id = artistJson.Id,
                    Name = artistJson.Name,
                    SortName = artistJson.SortName,
                    ArtistPath = artistPath,
                    ArtistJson = artistJson,
                    Releases = new List<CachedRelease>(),
                };

                // Read releases for this artist
                var releasesData = await reader.ReadArtistAlbumsAsync(artistPath);

                foreach (var (releasePath, releaseJson) in releasesData)
                {
                    var cachedRelease = new CachedRelease
                    {
                        Title = releaseJson.Title,
                        SortTitle = releaseJson.SortTitle,
                        Type = releaseJson.Type,
                        ReleasePath = releasePath,
                        ArtistId = artistJson.Id,
                        ArtistName = artistJson.Name,
                        ReleaseJson = releaseJson,
                        Tracks = new List<CachedTrack>(),
                    };

                    // Process tracks if they exist
                    if (releaseJson.Tracks != null)
                    {
                        foreach (var trackJson in releaseJson.Tracks)
                        {
                            var cachedTrack = new CachedTrack
                            {
                                Title = trackJson.Title,
                                SortTitle = trackJson.SortTitle,
                                TrackNumber = trackJson.TrackNumber,
                                AudioFilePath = trackJson.AudioFilePath,
                                ArtistId = artistJson.Id,
                                ArtistName = artistJson.Name,
                                ReleaseTitle = releaseJson.Title,
                                ReleaseType = releaseJson.Type,
                                TrackJson = trackJson,
                            };

                            cachedRelease.Tracks.Add(cachedTrack);
                            newAllTracks.Add(cachedTrack);
                        }
                    }

                    cachedArtist.Releases.Add(cachedRelease);
                    newAllReleases.Add(cachedRelease);
                }

                newArtistsById[artistJson.Id.ToLowerInvariant()] = cachedArtist;
                newArtistsByName[artistJson.Name.ToLowerInvariant()] = cachedArtist;
                newAllArtists.Add(cachedArtist);
            }

            // Update the cache atomically
            lock (_lockObject)
            {
                _artistsById.Clear();
                _artistsByName.Clear();
                _allArtists.Clear();
                _allReleases.Clear();
                _allTracks.Clear();

                foreach (var kvp in newArtistsById)
                    _artistsById[kvp.Key] = kvp.Value;

                foreach (var kvp in newArtistsByName)
                    _artistsByName[kvp.Key] = kvp.Value;

                _allArtists.AddRange(newAllArtists);
                _allReleases.AddRange(newAllReleases);
                _allTracks.AddRange(newAllTracks);

                _lastUpdated = DateTime.UtcNow;
                _isInitialized = true;
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't clear existing cache
            Console.WriteLine($"Error updating cache: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Ensures the cache is initialized, updating it if necessary
    /// </summary>
    private async Task EnsureCacheInitializedAsync()
    {
        if (!_isInitialized)
        {
            await UpdateCacheAsync();
        }
    }

    /// <summary>
    /// Gets an artist by ID (case-insensitive)
    /// </summary>
    /// <param name="id">Artist ID</param>
    /// <returns>Cached artist or null if not found</returns>
    public async Task<CachedArtist?> GetArtistByIdAsync(string id)
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            _artistsById.TryGetValue(id.ToLowerInvariant(), out var artist);
            return artist;
        }
    }

    /// <summary>
    /// Gets an artist by name (case-insensitive exact match)
    /// </summary>
    /// <param name="name">Artist name</param>
    /// <returns>Cached artist or null if not found</returns>
    public async Task<CachedArtist?> GetArtistByNameAsync(string name)
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            _artistsByName.TryGetValue(name.ToLowerInvariant(), out var artist);
            return artist;
        }
    }

    /// <summary>
    /// Searches for artists by name (case-insensitive partial match)
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="limit">Maximum number of results (default: 50)</param>
    /// <returns>List of matching artists</returns>
    public async Task<List<CachedArtist>> SearchArtistsByNameAsync(
        string searchTerm,
        int limit = 50
    )
    {
        await EnsureCacheInitializedAsync();

        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        lock (_lockObject)
        {
            return _allArtists
                .Where(a =>
                    a.SearchName.Contains(lowerSearchTerm)
                    || (a.SearchSortName?.Contains(lowerSearchTerm) ?? false)
                )
                .OrderBy(a => a.SearchName.StartsWith(lowerSearchTerm) ? 0 : 1) // Prioritize starts-with matches
                .ThenBy(a => a.Name)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Searches for releases/albums by title (case-insensitive partial match)
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="limit">Maximum number of results (default: 50)</param>
    /// <returns>List of matching releases</returns>
    public async Task<List<CachedRelease>> SearchReleasesByTitleAsync(
        string searchTerm,
        int limit = 50
    )
    {
        await EnsureCacheInitializedAsync();

        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        lock (_lockObject)
        {
            return _allReleases
                .Where(r =>
                    r.SearchTitle.Contains(lowerSearchTerm)
                    || (r.SearchSortTitle?.Contains(lowerSearchTerm) ?? false)
                )
                .OrderBy(r => r.SearchTitle.StartsWith(lowerSearchTerm) ? 0 : 1) // Prioritize starts-with matches
                .ThenBy(r => r.Title)
                .ThenBy(r => r.ArtistName)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Searches for tracks by title (case-insensitive partial match)
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="limit">Maximum number of results (default: 50)</param>
    /// <returns>List of matching tracks</returns>
    public async Task<List<CachedTrack>> SearchTracksByTitleAsync(string searchTerm, int limit = 50)
    {
        await EnsureCacheInitializedAsync();

        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        lock (_lockObject)
        {
            return _allTracks
                .Where(t =>
                    t.SearchTitle.Contains(lowerSearchTerm)
                    || (t.SearchSortTitle?.Contains(lowerSearchTerm) ?? false)
                )
                .OrderBy(t => t.SearchTitle.StartsWith(lowerSearchTerm) ? 0 : 1) // Prioritize starts-with matches
                .ThenBy(t => t.Title)
                .ThenBy(t => t.ArtistName)
                .ThenBy(t => t.ReleaseTitle)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Gets all artists from cache
    /// </summary>
    /// <returns>List of all cached artists</returns>
    public async Task<List<CachedArtist>> GetAllArtistsAsync()
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            return _allArtists.ToList();
        }
    }

    /// <summary>
    /// Gets all releases from cache
    /// </summary>
    /// <returns>List of all cached releases</returns>
    public async Task<List<CachedRelease>> GetAllReleasesAsync()
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            return _allReleases.ToList();
        }
    }

    /// <summary>
    /// Gets all tracks from cache
    /// </summary>
    /// <returns>List of all cached tracks</returns>
    public async Task<List<CachedTrack>> GetAllTracksAsync()
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            return _allTracks.ToList();
        }
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    /// <returns>Cache statistics</returns>
    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            return new CacheStatistics
            {
                ArtistCount = _allArtists.Count,
                ReleaseCount = _allReleases.Count,
                TrackCount = _allTracks.Count,
                LastUpdated = _lastUpdated,
                IsInitialized = _isInitialized,
            };
        }
    }
}
