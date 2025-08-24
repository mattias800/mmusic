using System.Text.RegularExpressions;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;
using Soulseek;

namespace MusicGQL.Features.Downloads.Services;

/// <summary>
/// Service for discovering additional releases from Soulseek users after successful downloads
/// </summary>
public class SoulSeekUserDiscoveryService(
    SoulseekClient client,
    ServerLibraryCache cache,
    DownloadQueueService downloadQueue,
    ServerSettingsAccessor serverSettingsAccessor,
    ILogger<SoulSeekUserDiscoveryService> logger
)
{
    /// <summary>
    /// Discovers additional releases from a user after a successful download
    /// </summary>
    public async Task DiscoverAdditionalReleasesAsync(
        string username,
        string currentArtistId,
        string currentReleaseFolderName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            logger.LogInformation(
                "[SoulSeek] Discovering additional releases from user '{User}'",
                username
            );

            // Get user's shared files by searching for common patterns
            var additionalReleases = await FindWantedReleasesFromUserAsync(
                username,
                currentArtistId,
                currentReleaseFolderName,
                cancellationToken
            );

            if (additionalReleases.Count > 0)
            {
                // Get the maximum number of releases to discover per user
                var settings = await serverSettingsAccessor.GetAsync();
                var maxReleases = settings.SoulSeekMaxReleasesPerUserDiscovery;

                // Limit the number of releases to prevent queue overflow
                var releasesToQueue = additionalReleases.Take(maxReleases).ToList();

                logger.LogInformation(
                    "[SoulSeek] Found {TotalCount} additional releases from user '{User}', queuing {QueuedCount} (limited by MaxReleasesPerUserDiscovery={MaxReleases})",
                    additionalReleases.Count,
                    username,
                    releasesToQueue.Count,
                    maxReleases
                );

                // Add releases to download queue with priority (same user = better connection)
                foreach (var release in releasesToQueue)
                {
                    downloadQueue.EnqueueFront(
                        new DownloadQueueItem(release.ArtistId, release.ReleaseFolderName)
                        {
                            ArtistName = release.ArtistName,
                            ReleaseTitle = release.ReleaseTitle,
                        }
                    );

                    logger.LogDebug(
                        "[SoulSeek] Queued additional release: {Artist} - {Release} from user {User}",
                        release.ArtistName,
                        release.ReleaseTitle,
                        username
                    );
                }

                if (additionalReleases.Count > maxReleases)
                {
                    logger.LogInformation(
                        "[SoulSeek] Limited discovery to {QueuedCount} releases from user '{User}' (found {TotalCount} total)",
                        maxReleases,
                        username,
                        additionalReleases.Count
                    );
                }
            }
            else
            {
                logger.LogDebug(
                    "[SoulSeek] No additional releases found from user '{User}'",
                    username
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[SoulSeek] Failed to discover additional releases from user '{User}'",
                username
            );
        }
    }

    /// <summary>
    /// Searches for additional releases from a specific user
    /// </summary>
    private async Task<List<DiscoveredRelease>> FindWantedReleasesFromUserAsync(
        string username,
        string currentArtistId,
        string currentReleaseFolderName,
        CancellationToken cancellationToken
    )
    {
        var discoveredReleases = new List<DiscoveredRelease>();

        try
        {
            // Get the current release info to understand what we're looking for
            var currentRelease = await cache.GetReleaseByArtistAndFolderAsync(
                currentArtistId,
                currentReleaseFolderName
            );
            if (currentRelease == null)
            {
                logger.LogWarning("[SoulSeek] Could not get current release info for discovery");
                return discoveredReleases;
            }

            // Search for releases from the same artist first (likely to be available)
            var artistReleases = await SearchArtistReleasesAsync(
                currentRelease.ArtistName,
                cancellationToken
            );
            discoveredReleases.AddRange(artistReleases);

            // Also search for releases with similar characteristics
            var similarReleases = await SearchSimilarReleasesAsync(
                currentRelease,
                cancellationToken
            );
            discoveredReleases.AddRange(similarReleases);

            // Remove duplicates and the current release
            var uniqueReleases = discoveredReleases
                .Where(r =>
                    !(
                        r.ArtistId == currentArtistId
                        && r.ReleaseFolderName == currentReleaseFolderName
                    )
                )
                .GroupBy(r => $"{r.ArtistId}|{r.ReleaseFolderName}")
                .Select(g => g.First())
                .ToList();

            return uniqueReleases;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[SoulSeek] Error searching for additional releases from user '{User}'",
                username
            );
            return discoveredReleases;
        }
    }

    /// <summary>
    /// Searches for releases from the same artist
    /// </summary>
    private async Task<List<DiscoveredRelease>> SearchArtistReleasesAsync(
        string artistName,
        CancellationToken cancellationToken
    )
    {
        var releases = new List<DiscoveredRelease>();

        try
        {
            // Search for the artist name to find other releases
            var searchQuery = new SearchQuery(artistName);
            var searchResult = await client.SearchAsync(searchQuery);

            if (searchResult.Responses == null || searchResult.Responses.Count == 0)
                return releases;

            // Group files by potential release folders
            var releaseGroups = GroupFilesByRelease(
                searchResult.Responses.First().Files.ToList(),
                artistName
            );

            foreach (var group in releaseGroups)
            {
                if (IsValidRelease(group))
                {
                    var release = new DiscoveredRelease
                    {
                        ArtistId = group.ArtistId,
                        ReleaseFolderName = group.ReleaseFolderName,
                        ArtistName = group.ArtistName,
                        ReleaseTitle = group.ReleaseTitle,
                        TrackCount = group.TrackCount,
                        Quality = group.Quality,
                    };

                    releases.Add(release);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[SoulSeek] Error searching for artist releases");
        }

        return releases;
    }

    /// <summary>
    /// Searches for releases similar to the current one
    /// </summary>
    private async Task<List<DiscoveredRelease>> SearchSimilarReleasesAsync(
        CachedRelease currentRelease,
        CancellationToken cancellationToken
    )
    {
        var releases = new List<DiscoveredRelease>();

        try
        {
            // Search for releases with similar characteristics
            var searchTerms = GenerateSimilarSearchTerms(currentRelease);

            foreach (var term in searchTerms.Take(3)) // Limit to avoid overwhelming the user
            {
                var searchQuery = new SearchQuery(term);
                var searchResult = await client.SearchAsync(searchQuery);

                if (searchResult.Responses == null || searchResult.Responses.Count == 0)
                    continue;

                var releaseGroups = GroupFilesByRelease(
                    searchResult.Responses.First().Files.ToList(),
                    term
                );

                foreach (var group in releaseGroups)
                {
                    if (IsValidRelease(group) && !IsCurrentRelease(group, currentRelease))
                    {
                        var release = new DiscoveredRelease
                        {
                            ArtistId = group.ArtistId,
                            ReleaseFolderName = group.ReleaseFolderName,
                            ArtistName = group.ArtistName,
                            ReleaseTitle = group.ReleaseTitle,
                            TrackCount = group.TrackCount,
                            Quality = group.Quality,
                        };

                        releases.Add(release);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[SoulSeek] Error searching for similar releases");
        }

        return releases;
    }

    /// <summary>
    /// Groups files by potential release structure
    /// </summary>
    private List<ReleaseGroup> GroupFilesByRelease(List<Soulseek.File> files, string searchTerm)
    {
        var groups = new List<ReleaseGroup>();

        if (files == null || files.Count == 0)
            return groups;

        // Group files by directory structure
        var directoryGroups = files
            .Where(f => IsAudioFile(f))
            .GroupBy(f => GetDirectoryPath(f.Filename))
            .ToList();

        foreach (var dirGroup in directoryGroups)
        {
            var releaseGroup = ParseReleaseFromDirectory(
                dirGroup.Key,
                dirGroup.ToList(),
                searchTerm
            );
            if (releaseGroup != null)
            {
                groups.Add(releaseGroup);
            }
        }

        return groups;
    }

    /// <summary>
    /// Parses release information from a directory path
    /// </summary>
    private ReleaseGroup? ParseReleaseFromDirectory(
        string directoryPath,
        List<Soulseek.File> files,
        string searchTerm
    )
    {
        try
        {
            var segments = directoryPath.Split(
                new[] { '\\', '/' },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (segments.Length < 2)
                return null;

            // Look for artist and album patterns
            var artistSegment = segments.FirstOrDefault(s =>
                s.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || searchTerm.Contains(s, StringComparison.OrdinalIgnoreCase)
            );

            if (artistSegment == null)
                return null;

            var artistIndex = Array.IndexOf(segments, artistSegment);
            if (artistIndex < 0 || artistIndex >= segments.Length - 1)
                return null;

            var albumSegment = segments[artistIndex + 1];

            // Generate artist and release IDs
            var artistId = GenerateArtistId(artistSegment);
            var releaseFolderName = GenerateReleaseFolderName(albumSegment);

            var audioFiles = files.Where(IsAudioFile).ToList();

            return new ReleaseGroup
            {
                ArtistId = artistId,
                ReleaseFolderName = releaseFolderName,
                ArtistName = artistSegment,
                ReleaseTitle = albumSegment,
                TrackCount = audioFiles.Count,
                Quality = DetermineQuality(audioFiles),
            };
        }
        catch (Exception ex)
        {
            logger.LogDebug(
                ex,
                "[SoulSeek] Error parsing release from directory: {Path}",
                directoryPath
            );
            return null;
        }
    }

    /// <summary>
    /// Generates search terms similar to the current release
    /// </summary>
    private List<string> GenerateSimilarSearchTerms(CachedRelease currentRelease)
    {
        var terms = new List<string>();

        if (currentRelease?.JsonRelease == null)
            return terms;

        // Add year-based searches
        if (!string.IsNullOrEmpty(currentRelease.JsonRelease.FirstReleaseYear))
        {
            terms.Add(currentRelease.JsonRelease.FirstReleaseYear);
        }

        // Add type-based searches
        terms.Add(currentRelease.JsonRelease.Type.ToString().ToLower());

        // Add artist name variations
        if (!string.IsNullOrEmpty(currentRelease.ArtistName))
        {
            terms.Add(currentRelease.ArtistName);
        }

        return terms.Distinct().ToList();
    }

    /// <summary>
    /// Checks if a release group is valid
    /// </summary>
    private bool IsValidRelease(ReleaseGroup group)
    {
        return !string.IsNullOrEmpty(group.ArtistId)
            && !string.IsNullOrEmpty(group.ReleaseFolderName)
            && !string.IsNullOrEmpty(group.ArtistName)
            && !string.IsNullOrEmpty(group.ReleaseTitle)
            && group.TrackCount >= 3; // Minimum tracks to be considered a valid release
    }

    /// <summary>
    /// Checks if a release group is the current release
    /// </summary>
    private bool IsCurrentRelease(ReleaseGroup group, CachedRelease currentRelease)
    {
        return group.ArtistId == currentRelease.ArtistId
            && group.ReleaseFolderName == currentRelease.FolderName;
    }

    /// <summary>
    /// Checks if a file is an audio file
    /// </summary>
    private bool IsAudioFile(Soulseek.File file)
    {
        return file.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase)
            || file.Extension.Equals("flac", StringComparison.OrdinalIgnoreCase)
            || file.Extension.Equals("m4a", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the directory path from a filename
    /// </summary>
    private string GetDirectoryPath(string filename)
    {
        var lastSlash = filename.LastIndexOfAny(new[] { '\\', '/' });
        return lastSlash > 0 ? filename.Substring(0, lastSlash) : filename;
    }

    /// <summary>
    /// Generates an artist ID from artist name
    /// </summary>
    private string GenerateArtistId(string artistName)
    {
        // Simple hash-based ID generation
        var hash = artistName.GetHashCode().ToString("X");
        return $"discovered_{hash}";
    }

    /// <summary>
    /// Generates a release folder name from album title
    /// </summary>
    private string GenerateReleaseFolderName(string albumTitle)
    {
        // Clean the album title for folder naming
        var cleanTitle = Regex.Replace(albumTitle, @"[<>:""/\\|?*]", "_");
        return cleanTitle.Trim();
    }

    /// <summary>
    /// Determines the quality of audio files
    /// </summary>
    private string DetermineQuality(List<Soulseek.File> audioFiles)
    {
        var hasFlac = audioFiles.Any(f =>
            f.Extension.Equals("flac", StringComparison.OrdinalIgnoreCase)
        );
        var hasHighBitrateMp3 = audioFiles.Any(f =>
            f.Extension.Equals("mp3", StringComparison.OrdinalIgnoreCase) && f.BitRate >= 320
        );

        if (hasFlac)
            return "FLAC";
        if (hasHighBitrateMp3)
            return "MP3-320";
        return "MP3";
    }
}

/// <summary>
/// Represents a discovered release from a user
/// </summary>
public class DiscoveredRelease
{
    public string ArtistId { get; set; } = string.Empty;
    public string ReleaseFolderName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string ReleaseTitle { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public string Quality { get; set; } = string.Empty;
}

/// <summary>
/// Represents a group of files that might be a release
/// </summary>
public class ReleaseGroup
{
    public string ArtistId { get; set; } = string.Empty;
    public string ReleaseFolderName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string ReleaseTitle { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    public string Quality { get; set; } = string.Empty;
}
