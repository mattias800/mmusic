using System.Diagnostics;
using Hqub.MusicBrainz;
using Hqub.MusicBrainz.Entities;
using Microsoft.Extensions.Caching.Hybrid;

namespace MusicGQL.Integration.MusicBrainz;

public class MusicBrainzService(MusicBrainzClient client, HybridCache cache)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);
    private readonly string cacheKeyPrefix = "musicbrainz";

    // Artists

    public Task<Artist?> GetArtistByIdAsync(string id) =>
        ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:artist:{id}",
            TimeSpan.FromDays(1),
            () => client.Artists.GetAsync(id)
        );

    public async Task<List<Artist>> GetArtistsForRecordingAsync(string recordingId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:recording:{recordingId}:artists",
            TimeSpan.FromDays(1),
            async () => (await client.Artists.BrowseAsync("recording", recordingId)).Items
        );
        return result ?? [];
    }

    public async Task<List<Artist>> SearchArtistByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:search_artist:{name}:{limit}:{offset}",
            TimeSpan.FromHours(1),
            async () => (await client.Artists.SearchAsync(name, limit, offset)).Items
        );

        return result?.OrderByDescending(r => r.Score).ToList() ?? [];
    }

    // Recordings

    public async Task<Recording?> GetRecordingByIdAsync(string id)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:recording:{id}",
            TimeSpan.FromDays(1),
            () => client.Recordings.GetAsync(id, "artist-credits", "url-rels")
        );
        return result;
    }

    public async Task<List<Recording>> GetRecordingsForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:artist:{artistId}:recordings",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.Recordings.BrowseAsync(
                        "artist",
                        artistId,
                        100,
                        0,
                        "artist-credits",
                        "url-rels"
                    )
                ).Items
        );
        return result ?? [];
    }

    public async Task<List<Recording>> GetRecordingsForReleaseAsync(string releaseId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:release:{releaseId}:recordings",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.Recordings.BrowseAsync(
                        "release",
                        releaseId,
                        100,
                        0,
                        "artist-credits",
                        "url-rels"
                    )
                ).Items
        );
        return result ?? [];
    }

    public async Task<List<Recording>> SearchRecordingByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:search_recording:{name}:{limit}:{offset}",
            TimeSpan.FromHours(1),
            async () => (await client.Recordings.SearchAsync(name, limit, offset)).Items
        );
        return result?.OrderByDescending(r => r.Score).ToList() ?? [];
    }

    // Releases

    public Task<Release?> GetReleaseByIdAsync(string releaseId) =>
        ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:release:{releaseId}",
            TimeSpan.FromDays(1),
            () =>
                client.Releases.GetAsync(
                    releaseId,
                    "recordings",
                    "genres",
                    "release-groups",
                    "artist-credits",
                    "annotation",
                    "tags",
                    "ratings",
                    "labels"
                )
        );

    public async Task<List<Release>> GetReleasesForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:artist:{artistId}:releases",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.Releases.BrowseAsync(
                        "artist",
                        artistId,
                        100,
                        0,
                        "recordings",
                        "genres",
                        "release-groups",
                        "artist-credits",
                        "annotation",
                        "tags",
                        "ratings",
                        "labels"
                    )
                ).Items
        );
        return result ?? [];
    }

    public async Task<List<Release>> GetReleasesForRecordingAsync(string recordingId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:recording:{recordingId}:releases",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.Releases.BrowseAsync(
                        "recording",
                        recordingId,
                        100,
                        0,
                        "recordings",
                        "genres",
                        "release-groups",
                        "artist-credits",
                        "annotation",
                        "tags",
                        "ratings",
                        "labels"
                    )
                ).Items
        );
        return result ?? [];
    }

    public async Task<List<Release>> GetReleasesForReleaseGroupAsync(string releaseGroupId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:release-group:{releaseGroupId}:releases",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.Releases.BrowseAsync(
                        "release-group",
                        releaseGroupId,
                        100,
                        0,
                        "recordings",
                        "genres",
                        "release-groups",
                        "artist-credits",
                        "annotation",
                        "tags",
                        "ratings",
                        "labels"
                    )
                ).Items
        );
        return result ?? [];
    }

    public async Task<List<Release>> SearchReleaseByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:search_release:{name}:{limit}:{offset}",
            TimeSpan.FromDays(1),
            async () => (await client.Releases.SearchAsync(name, limit, offset)).Items
        );

        return result?.OrderByDescending(r => r.Score).ToList() ?? [];
    }

    // Release groups

    public Task<ReleaseGroup?> GetReleaseGroupByIdAsync(string releaseGroupId) =>
        ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:release_group:{releaseGroupId}",
            TimeSpan.FromDays(1),
            () => client.ReleaseGroups.GetAsync(releaseGroupId, "genres", "artist-credits")
        );

    public async Task<List<ReleaseGroup>> SearchReleaseGroupByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:search_release_group:{name}:{limit}:{offset}",
            TimeSpan.FromDays(1),
            async () => (await client.ReleaseGroups.SearchAsync(name, limit, offset)).Items
        );

        return result ?? [];
    }

    public async Task<List<ReleaseGroup>> GetReleaseGroupsForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync(
            $"{cacheKeyPrefix}:artist:{artistId}:release-groups",
            TimeSpan.FromDays(1),
            async () =>
                (
                    await client.ReleaseGroups.BrowseAsync(
                        "artist",
                        artistId,
                        100,
                        0,
                        "genres",
                        "artist-credits"
                    )
                ).Items
        );
        return result ?? [];
    }

    private async Task<T?> ExecuteThrottledAsync<T>(
        string cacheKey,
        TimeSpan cacheDuration,
        Func<Task<T>> fetch
    )
    {
        return await cache.GetOrCreateAsync(
            cacheKey,
            async cancellationToken =>
            {
                await _throttle.WaitAsync(cancellationToken);
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    return await fetch();
                }
                finally
                {
                    stopwatch.Stop();

                    var timeLeftForMusicBrainzLimit = Math.Max(
                        1500 - (int)stopwatch.ElapsedMilliseconds,
                        0
                    );
                    await Task.Delay(timeLeftForMusicBrainzLimit, cancellationToken); // Rate limit: 1 req/sec
                    _throttle.Release();
                }
            },
            new HybridCacheEntryOptions
            {
                LocalCacheExpiration = cacheDuration,
                Expiration = cacheDuration,
            }
        );
    }
}
