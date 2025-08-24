using HotChocolate.Subscriptions;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerLibrary.Subscription;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Cache;

public class ServerLibraryCache(ServerLibraryJsonReader reader, ITopicEventSender eventSender)
{
    private readonly Dictionary<string, CachedArtist> _artistsById = new();
    private readonly Dictionary<string, CachedArtist> _artistsByName = new();

    private readonly Dictionary<
        string,
        Dictionary<string, CachedRelease>
    > _releasesByArtistAndFolder = new();

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
        var newReleasesByArtistAndFolder =
            new Dictionary<string, Dictionary<string, CachedRelease>>();
        var newAllArtists = new List<CachedArtist>();
        var newAllReleases = new List<CachedRelease>();
        var newAllTracks = new List<CachedTrack>();

        // Snapshot previous per-track media availability statuses so we can preserve them
        var previousStatuses = new Dictionary<string, CachedMediaAvailabilityStatus>();
        // Snapshot previous per-release download statuses
        var previousReleaseStatuses = new Dictionary<string, CachedReleaseDownloadStatus>();
        lock (_lockObject)
        {
            foreach (var (artistKey, releases) in _releasesByArtistAndFolder)
            {
                foreach (var (releaseKey, release) in releases)
                {
                    previousReleaseStatuses[$"{artistKey}|{releaseKey}"] = release.DownloadStatus;
                    foreach (var t in release.Tracks)
                    {
                        var disc = t.DiscNumber > 0 ? t.DiscNumber : 1;
                        var k = $"{artistKey}|{releaseKey}|{disc}|{t.TrackNumber}";
                        previousStatuses[k] = t.CachedMediaAvailabilityStatus;
                    }
                }
            }
        }

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
                    JsonArtist = artistJson,
                    Releases = new List<CachedRelease>(),
                };

                var artistReleases = new Dictionary<string, CachedRelease>();

                // Read releases for this artist
                var releasesData = await reader.ReadArtistAlbumsAsync(artistPath);

                foreach (var (releasePath, releaseJson) in releasesData)
                {
                    var folderName = Path.GetFileName(releasePath) ?? "";

                    var cachedRelease = new CachedRelease
                    {
                        Title = releaseJson.Title,
                        SortTitle = releaseJson.SortTitle,
                        Type = releaseJson.Type,
                        ReleasePath = releasePath,
                        FolderName = folderName,
                        ArtistId = artistJson.Id,
                        ArtistName = releaseJson.ArtistName, // Use the artist name from release.json (historical name)
                        JsonRelease = releaseJson,
                        Tracks = new List<CachedTrack>(),
                        Discs = new List<CachedDisc>(),
                    };

                    // Preserve previous per-release download status if present
                    var relKey =
                        $"{artistJson.Id.ToLowerInvariant()}|{folderName.ToLowerInvariant()}";
                    if (previousReleaseStatuses.TryGetValue(relKey, out var prevRelStatus))
                    {
                        cachedRelease.DownloadStatus = prevRelStatus;
                    }

                    // Process tracks from discs if present, otherwise use top-level tracks
                    if (releaseJson.Discs is { Count: > 0 })
                    {
                        foreach (var disc in releaseJson.Discs.OrderBy(d => d.DiscNumber))
                        {
                            var discNumber = disc.DiscNumber <= 0 ? 1 : disc.DiscNumber;
                            var discTracks = new List<CachedTrack>();
                            if (disc.Tracks == null)
                                disc.Tracks = new List<Json.JsonTrack>();
                            foreach (var trackJson in disc.Tracks)
                            {
                                var cachedTrack = new CachedTrack
                                {
                                    Title = trackJson.Title,
                                    SortTitle = trackJson.SortTitle,
                                    TrackNumber = trackJson.TrackNumber,
                                    DiscNumber = discNumber,
                                    AudioFilePath = trackJson.AudioFilePath,
                                    ArtistId = artistJson.Id,
                                    ArtistName = releaseJson.ArtistName, // Use the artist name from release.json (historical name)
                                    ReleaseFolderName = folderName,
                                    ReleaseTitle = releaseJson.Title,
                                    JsonReleaseType = releaseJson.Type,
                                    JsonTrack = trackJson,
                                };

                                // Preserve previous media availability status if present; otherwise infer from disk
                                var statusKey =
                                    $"{artistJson.Id.ToLowerInvariant()}|{folderName.ToLowerInvariant()}|{cachedTrack.DiscNumber}|{cachedTrack.TrackNumber}";
                                if (previousStatuses.TryGetValue(statusKey, out var prevStatus))
                                {
                                    cachedTrack.CachedMediaAvailabilityStatus = prevStatus;
                                }
                                else
                                {
                                    try
                                    {
                                        var audioRef = trackJson.AudioFilePath;
                                        if (!string.IsNullOrWhiteSpace(audioRef))
                                        {
                                            string resolved = audioRef!;
                                            if (!Path.IsPathRooted(resolved))
                                            {
                                                if (resolved.StartsWith("./"))
                                                    resolved = resolved.Substring(2);
                                                resolved = Path.Combine(releasePath, resolved);
                                            }
                                            cachedTrack.CachedMediaAvailabilityStatus = File.Exists(
                                                resolved
                                            )
                                                ? CachedMediaAvailabilityStatus.Available
                                                : CachedMediaAvailabilityStatus.Missing;
                                        }
                                    }
                                    catch { }
                                }

                                cachedRelease.Tracks.Add(cachedTrack);
                                discTracks.Add(cachedTrack);
                                newAllTracks.Add(cachedTrack);
                            }

                            cachedRelease.Discs.Add(
                                new CachedDisc
                                {
                                    DiscNumber = discNumber,
                                    Title = disc.Title,
                                    Tracks = discTracks.OrderBy(t => t.TrackNumber).ToList(),
                                }
                            );
                        }
                    }
                    else if (releaseJson.Tracks != null)
                    {
                        foreach (var trackJson in releaseJson.Tracks)
                        {
                            var cachedTrack = new CachedTrack
                            {
                                Title = trackJson.Title,
                                SortTitle = trackJson.SortTitle,
                                TrackNumber = trackJson.TrackNumber,
                                DiscNumber = trackJson.DiscNumber ?? 1,
                                AudioFilePath = trackJson.AudioFilePath,
                                ArtistId = artistJson.Id,
                                ArtistName = releaseJson.ArtistName, // Use the artist name from release.json (historical name)
                                ReleaseFolderName = folderName,
                                ReleaseTitle = releaseJson.Title,
                                JsonReleaseType = releaseJson.Type,
                                JsonTrack = trackJson,
                            };

                            var statusKey =
                                $"{artistJson.Id.ToLowerInvariant()}|{folderName.ToLowerInvariant()}|{cachedTrack.DiscNumber}|{cachedTrack.TrackNumber}";
                            if (previousStatuses.TryGetValue(statusKey, out var prevStatus))
                            {
                                cachedTrack.CachedMediaAvailabilityStatus = prevStatus;
                            }
                            else
                            {
                                try
                                {
                                    var audioRef = trackJson.AudioFilePath;
                                    if (!string.IsNullOrWhiteSpace(audioRef))
                                    {
                                        string resolved = audioRef!;
                                        if (!Path.IsPathRooted(resolved))
                                        {
                                            if (resolved.StartsWith("./"))
                                                resolved = resolved.Substring(2);
                                            resolved = Path.Combine(releasePath, resolved);
                                        }
                                        cachedTrack.CachedMediaAvailabilityStatus = File.Exists(
                                            resolved
                                        )
                                            ? CachedMediaAvailabilityStatus.Available
                                            : CachedMediaAvailabilityStatus.Missing;
                                    }
                                }
                                catch { }
                            }

                            cachedRelease.Tracks.Add(cachedTrack);
                            newAllTracks.Add(cachedTrack);
                        }

                        // Build discs from flattened view if multiple disc numbers exist, otherwise one disc
                        var groups = cachedRelease
                            .Tracks.GroupBy(t => t.DiscNumber > 0 ? t.DiscNumber : 1)
                            .OrderBy(g => g.Key)
                            .ToList();
                        foreach (var g in groups)
                        {
                            cachedRelease.Discs.Add(
                                new CachedDisc
                                {
                                    DiscNumber = g.Key,
                                    Title = null,
                                    Tracks = g.OrderBy(t => t.TrackNumber).ToList(),
                                }
                            );
                        }
                    }

                    cachedArtist.Releases.Add(cachedRelease);
                    newAllReleases.Add(cachedRelease);
                    artistReleases[folderName.ToLowerInvariant()] = cachedRelease;
                }

                newArtistsById[artistJson.Id.ToLowerInvariant()] = cachedArtist;
                newArtistsByName[artistJson.Name.ToLowerInvariant()] = cachedArtist;
                newReleasesByArtistAndFolder[artistJson.Id.ToLowerInvariant()] = artistReleases;
                newAllArtists.Add(cachedArtist);
            }

            // Update the cache atomically
            lock (_lockObject)
            {
                _artistsById.Clear();
                _artistsByName.Clear();
                _releasesByArtistAndFolder.Clear();
                _allArtists.Clear();
                _allReleases.Clear();
                _allTracks.Clear();

                foreach (var kvp in newArtistsById)
                    _artistsById[kvp.Key] = kvp.Value;

                foreach (var kvp in newArtistsByName)
                    _artistsByName[kvp.Key] = kvp.Value;

                foreach (var kvp in newReleasesByArtistAndFolder)
                    _releasesByArtistAndFolder[kvp.Key] = kvp.Value;

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

    public async Task UpdateMediaAvailabilityStatus(
        string artistId,
        string releaseFolderName,
        int trackNumber,
        CachedMediaAvailabilityStatus status
    )
    {
        await UpdateMediaAvailabilityStatus(
            artistId,
            releaseFolderName,
            discNumber: null,
            trackNumber,
            status
        );
    }

    // New overload: disc-aware availability update
    public async Task UpdateMediaAvailabilityStatus(
        string artistId,
        string releaseFolderName,
        int? discNumber,
        int trackNumber,
        CachedMediaAvailabilityStatus status
    )
    {
        CachedTrack? track = null;
        CachedRelease? releaseRef = null;
        lock (_lockObject)
        {
            if (
                !_releasesByArtistAndFolder.TryGetValue(
                    artistId.ToLowerInvariant(),
                    out var artistReleases
                )
            )
                return;

            if (!artistReleases.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release))
                return;

            if (discNumber.HasValue)
            {
                var d = discNumber.Value <= 0 ? 1 : discNumber.Value;
                track = release.Tracks.FirstOrDefault(t =>
                    t.TrackNumber == trackNumber && (t.DiscNumber > 0 ? t.DiscNumber : 1) == d
                );
            }
            else
            {
                // Back-compat: first match on trackNumber regardless of disc
                track = release.Tracks.FirstOrDefault(t => t.TrackNumber == trackNumber);
            }

            if (track != null)
            {
                track.CachedMediaAvailabilityStatus = status;
                releaseRef = release;
            }
        }

        if (track != null)
        {
            // Publish to the exact topics that the subscriptions are listening to
            await eventSender.SendAsync(
                LibrarySubscription.LibraryCacheTrackUpdatedTopic(
                    artistId,
                    releaseFolderName,
                    trackNumber
                ),
                new LibraryCacheTrackStatus(artistId, releaseFolderName, trackNumber)
            );

            await eventSender.SendAsync(
                LibrarySubscription.LibraryCacheInTracksReleaseUpdatedTopic(
                    artistId,
                    releaseFolderName
                ),
                new LibraryCacheTrackStatus(artistId, releaseFolderName, trackNumber)
            );

            // Publish centralized track and release updates
            await eventSender.SendAsync(
                LibrarySubscription.LibraryTrackUpdatedTopic(
                    artistId,
                    releaseFolderName,
                    trackNumber
                ),
                new Track(track)
            );
            await eventSender.SendAsync(
                LibrarySubscription.LibraryArtistTrackUpdatedTopic(artistId),
                new Track(track)
            );

            if (releaseRef != null)
            {
                await eventSender.SendAsync(
                    LibrarySubscription.LibraryReleaseUpdatedTopic(artistId, releaseFolderName),
                    new Release(releaseRef)
                );
                await eventSender.SendAsync(
                    LibrarySubscription.LibraryArtistReleaseUpdatedTopic(artistId),
                    new Release(releaseRef)
                );
            }
        }
    }

    // Incremental: reload a single release from JSON and update cache structures
    public async Task UpdateReleaseFromJsonAsync(string artistId, string releaseFolderName)
    {
        CachedRelease? oldRelease = null;
        CachedArtist? artist = null;
        string releasePath = string.Empty;
        string artistIdLower = artistId.ToLowerInvariant();
        string folderLower = releaseFolderName.ToLowerInvariant();

        // Snapshot previous statuses by track number for this release
        var previousStatusesByTrackNumber = new Dictionary<int, CachedMediaAvailabilityStatus>();

        lock (_lockObject)
        {
            if (!_releasesByArtistAndFolder.TryGetValue(artistIdLower, out var artistReleases))
            {
                return;
            }

            if (!artistReleases.TryGetValue(folderLower, out oldRelease))
            {
                return;
            }

            _artistsById.TryGetValue(artistIdLower, out artist);
            releasePath = oldRelease.ReleasePath;

            foreach (var t in oldRelease.Tracks)
            {
                var disc = t.DiscNumber > 0 ? t.DiscNumber : 1;
                previousStatusesByTrackNumber[(disc * 1000) + t.TrackNumber] =
                    t.CachedMediaAvailabilityStatus;
            }
        }

        if (oldRelease == null || artist == null)
        {
            return;
        }

        // Read JSON for this release from disk
        var releaseJson = await reader.ReadReleaseFromPathAsync(releasePath);
        if (releaseJson == null)
        {
            return;
        }

        // Build a new CachedRelease from JSON
        var newRelease = new CachedRelease
        {
            Title = releaseJson.Title,
            SortTitle = releaseJson.SortTitle,
            Type = releaseJson.Type,
            ReleasePath = releasePath,
            FolderName = releaseFolderName,
            ArtistId = artist.Id,
            ArtistName = artist.Name,
            JsonRelease = releaseJson,
            Tracks = new List<CachedTrack>(),
            Discs = new List<CachedDisc>(),
            DownloadStatus = oldRelease.DownloadStatus,
        };

        if (releaseJson.Discs is { Count: > 0 })
        {
            foreach (var disc in releaseJson.Discs.OrderBy(d => d.DiscNumber))
            {
                var dnum = disc.DiscNumber <= 0 ? 1 : disc.DiscNumber;
                var discTracks = new List<CachedTrack>();
                if (disc.Tracks == null)
                    disc.Tracks = new List<Json.JsonTrack>();
                foreach (var jt in disc.Tracks)
                {
                    var ct = new CachedTrack
                    {
                        Title = jt.Title,
                        SortTitle = jt.SortTitle,
                        TrackNumber = jt.TrackNumber,
                        DiscNumber = dnum,
                        AudioFilePath = jt.AudioFilePath,
                        ArtistId = artist.Id,
                        ArtistName = artist.Name,
                        ReleaseFolderName = releaseFolderName,
                        ReleaseTitle = newRelease.Title,
                        JsonReleaseType = releaseJson.Type,
                        JsonTrack = jt,
                    };

                    var key = (dnum * 1000) + ct.TrackNumber;
                    if (previousStatusesByTrackNumber.TryGetValue(key, out var prev))
                    {
                        ct.CachedMediaAvailabilityStatus = prev;
                    }

                    newRelease.Tracks.Add(ct);
                    discTracks.Add(ct);
                }

                newRelease.Discs.Add(
                    new CachedDisc
                    {
                        DiscNumber = dnum,
                        Title = disc.Title,
                        Tracks = discTracks.OrderBy(t => t.TrackNumber).ToList(),
                    }
                );
            }
        }
        else if (releaseJson.Tracks != null)
        {
            foreach (var jt in releaseJson.Tracks)
            {
                var ct = new CachedTrack
                {
                    Title = jt.Title,
                    SortTitle = jt.SortTitle,
                    TrackNumber = jt.TrackNumber,
                    DiscNumber = jt.DiscNumber ?? 1,
                    AudioFilePath = jt.AudioFilePath,
                    ArtistId = artist.Id,
                    ArtistName = artist.Name,
                    ReleaseFolderName = releaseFolderName,
                    ReleaseTitle = newRelease.Title,
                    JsonReleaseType = releaseJson.Type,
                    JsonTrack = jt,
                };

                var key = (ct.DiscNumber * 1000) + ct.TrackNumber;
                if (previousStatusesByTrackNumber.TryGetValue(key, out var prev))
                {
                    ct.CachedMediaAvailabilityStatus = prev;
                }

                newRelease.Tracks.Add(ct);
            }

            var groups = newRelease
                .Tracks.GroupBy(t => t.DiscNumber > 0 ? t.DiscNumber : 1)
                .OrderBy(g => g.Key)
                .ToList();
            foreach (var g in groups)
            {
                newRelease.Discs.Add(
                    new CachedDisc
                    {
                        DiscNumber = g.Key,
                        Title = null,
                        Tracks = g.OrderBy(t => t.TrackNumber).ToList(),
                    }
                );
            }
        }

        // Replace the release in cache structures
        lock (_lockObject)
        {
            if (_releasesByArtistAndFolder.TryGetValue(artistIdLower, out var artistReleases))
            {
                artistReleases[folderLower] = newRelease;
            }

            // Replace in artist's Releases list
            var idx = artist.Releases.FindIndex(r =>
                r.FolderName.Equals(releaseFolderName, StringComparison.OrdinalIgnoreCase)
            );
            if (idx >= 0)
            {
                artist.Releases[idx] = newRelease;
            }

            // Update _allReleases: remove old instance and add new
            _allReleases.Remove(oldRelease);
            _allReleases.Add(newRelease);

            // Update _allTracks: remove old tracks for this release and add new tracks
            _allTracks.RemoveAll(t =>
                t.ArtistId.Equals(artistId, StringComparison.OrdinalIgnoreCase)
                && t.ReleaseFolderName.Equals(releaseFolderName, StringComparison.OrdinalIgnoreCase)
            );
            _allTracks.AddRange(newRelease.Tracks);

            _lastUpdated = DateTime.UtcNow;
            _isInitialized = true;
        }

        // Publish centralized release-updated events (scoped and artist-broadcast)
        await eventSender.SendAsync(
            LibrarySubscription.LibraryReleaseUpdatedTopic(artistId, releaseFolderName),
            new Release(newRelease)
        );
        await eventSender.SendAsync(
            LibrarySubscription.LibraryArtistReleaseUpdatedTopic(artistId),
            new Release(newRelease)
        );
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
    /// Gets an artist by MusicBrainz ID
    /// </summary>
    /// <param name="musicBrainzId">MusicBrainz artist ID</param>
    /// <returns>Cached artist or null if not found</returns>
    public async Task<CachedArtist?> GetArtistByMusicBrainzIdAsync(string musicBrainzId)
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            // Search through all artists to find one with matching MusicBrainz ID
            foreach (var artist in _artistsById.Values)
            {
                if (artist.JsonArtist.Connections?.MusicBrainzArtistId == musicBrainzId)
                {
                    return artist;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Gets a release by artist ID and release folder name
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <returns>Cached release or null if not found</returns>
    public async Task<CachedRelease?> GetReleaseByArtistAndFolderAsync(
        string artistId,
        string releaseFolderName
    )
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            var artistReleases = _releasesByArtistAndFolder.GetValueOrDefault(
                artistId.ToLowerInvariant()
            );
            if (artistReleases == null)
                return null;

            artistReleases.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release);
            return release;
        }
    }

    /// <summary>
    /// Gets all releases for an artist by artist ID
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <returns>List of all releases for the artist</returns>
    public async Task<List<CachedRelease>> GetAllReleasesForArtistAsync(string artistId)
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            var artist = _artistsById.GetValueOrDefault(artistId.ToLowerInvariant());
            return artist?.Releases ?? new List<CachedRelease>();
        }
    }

    /// <summary>
    /// Gets all tracks for a specific release by artist ID and release folder name
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <returns>List of tracks for the release</returns>
    public async Task<List<CachedTrack>> GetAllTracksForReleaseAsync(
        string artistId,
        string releaseFolderName
    )
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            var artistReleases = _releasesByArtistAndFolder.GetValueOrDefault(
                artistId.ToLowerInvariant()
            );
            if (artistReleases == null)
                return new List<CachedTrack>();

            artistReleases.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release);
            return release?.Tracks ?? new List<CachedTrack>();
        }
    }

    /// <summary>
    /// Gets a specific track by artist ID, release folder name, and track number
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <param name="releaseFolderName">Release folder name</param>
    /// <param name="trackNumber">Track number</param>
    /// <returns>Cached track or null if not found</returns>
    public async Task<CachedTrack?> GetTrackByArtistReleaseAndNumberAsync(
        string artistId,
        string releaseFolderName,
        int trackNumber
    )
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            var artistReleases = _releasesByArtistAndFolder.GetValueOrDefault(
                artistId.ToLowerInvariant()
            );
            if (artistReleases == null)
                return null;

            artistReleases.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release);
            return release?.Tracks.FirstOrDefault(t => t.TrackNumber == trackNumber);
        }
    }

    /// <summary>
    /// Gets a specific track by artist ID, release folder name, disc number and track number
    /// </summary>
    public async Task<CachedTrack?> GetTrackByArtistReleaseDiscAndNumberAsync(
        string artistId,
        string releaseFolderName,
        int discNumber,
        int trackNumber
    )
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            var artistReleases = _releasesByArtistAndFolder.GetValueOrDefault(
                artistId.ToLowerInvariant()
            );
            if (artistReleases == null)
                return null;

            if (!artistReleases.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release))
                return null;

            var d = discNumber <= 0 ? 1 : discNumber;
            return release.Tracks.FirstOrDefault(t =>
                (t.DiscNumber > 0 ? t.DiscNumber : 1) == d && t.TrackNumber == trackNumber
            );
        }
    }

    /// <summary>
    /// Gets all tracks for a specific artist by artist ID
    /// </summary>
    /// <param name="artistId">Artist ID</param>
    /// <returns>List of all tracks for the artist</returns>
    public async Task<List<CachedTrack>> GetTracksByArtistIdAsync(string artistId)
    {
        await EnsureCacheInitializedAsync();

        lock (_lockObject)
        {
            return _allTracks
                .Where(t => t.ArtistId.Equals(artistId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.ReleaseTitle)
                .ThenBy(t => t.TrackNumber)
                .ToList();
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

    // Update per-release download status and notify subscribers
    public async Task UpdateReleaseDownloadStatus(
        string artistId,
        string releaseFolderName,
        CachedReleaseDownloadStatus status
    )
    {
        CachedRelease? cachedRelease = null;
        lock (_lockObject)
        {
            if (
                _releasesByArtistAndFolder.TryGetValue(artistId.ToLowerInvariant(), out var rels)
                && rels.TryGetValue(releaseFolderName.ToLowerInvariant(), out var release)
            )
            {
                release.DownloadStatus = status;
                cachedRelease = release;
            }
        }

        if (cachedRelease != null)
        {
            await eventSender.SendAsync(
                LibrarySubscription.LibraryReleaseDownloadStatusUpdatedTopic(
                    artistId,
                    releaseFolderName
                ),
                new LibraryReleaseDownloadStatusUpdate(new Release(cachedRelease))
            );
        }
    }
}
