using Hqub.MusicBrainz;
using Hqub.MusicBrainz.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MusicGQL.Integration.MusicBrainz;

public class MusicBrainzService(MusicBrainzClient client, IMemoryCache cache)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);

    // Artists

    public Task<Artist?> GetArtistByIdAsync(string id)
        => ExecuteThrottledAsync($"artist:{id}", TimeSpan.FromDays(1), () => client.Artists.GetAsync(id));

    public async Task<List<Artist>> GetArtistsForRecordingAsync(string recordingId)
    {
        var result = await ExecuteThrottledAsync($"recording:{recordingId}:artists", TimeSpan.FromDays(1),
            () => client.Artists.BrowseAsync("recording", recordingId));
        return result?.Items ?? [];
    }

    public async Task<List<Artist>> SearchArtistByNameAsync(string name)
    {
        var result = await ExecuteThrottledAsync($"search_artist:{name}", TimeSpan.FromHours(1),
            () => client.Artists.SearchAsync(name, 100));
        return result?.Items ?? [];
    }

    // Recordings

    public Task<Recording?> GetRecordingByIdAsync(string id)
        => ExecuteThrottledAsync($"recording:{id}", TimeSpan.FromDays(1), () => client.Recordings.GetAsync(id));

    public async Task<List<Recording>> GetRecordingsForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync($"artist:{artistId}:recordings", TimeSpan.FromDays(1),
            () => client.Recordings.BrowseAsync("artist", artistId, 100));
        return result?.Items ?? [];
    }

    public async Task<List<Recording>> GetRecordingsForReleaseAsync(string releaseId)
    {
        var result = await ExecuteThrottledAsync($"release:{releaseId}:recordings", TimeSpan.FromDays(1),
            () => client.Recordings.BrowseAsync("release", releaseId, 100));
        return result?.Items ?? [];
    }

    public async Task<List<Recording>> SearchRecordingByNameAsync(string name)
    {
        var result = await ExecuteThrottledAsync($"search_recording:{name}", TimeSpan.FromHours(1),
            () => client.Recordings.SearchAsync(name, 100));
        return result?.Items ?? [];
    }

    // Releases

    public Task<Release?> GetReleaseByIdAsync(string releaseId)
        => ExecuteThrottledAsync($"release:{releaseId}", TimeSpan.FromDays(1),
            () => client.Releases.GetAsync(releaseId, "recordings", "genres", "release-groups", "artist-credits"));

    public async Task<List<Release>> GetReleasesForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync($"artist:{artistId}:releases", TimeSpan.FromDays(1),
            () => client.Releases.BrowseAsync("artist", artistId, 100, 0, "recordings", "genres", "release-groups",
                "artist-credits"));
        return result?.Items ?? [];
    }

    public async Task<List<Release>> GetReleasesForRecordingAsync(string recordingId)
    {
        var result = await ExecuteThrottledAsync($"recording:{recordingId}:releases", TimeSpan.FromDays(1),
            () => client.Releases.BrowseAsync("recording", recordingId, 100, 0, "recordings", "genres",
                "release-groups",
                "artist-credits"));
        return result?.Items ?? [];
    }

    public async Task<List<Release>> GetReleasesForReleaseGroupAsync(string releaseGroupId)
    {
        var result = await ExecuteThrottledAsync($"release-group:{releaseGroupId}:releases", TimeSpan.FromDays(1),
            () => client.Releases.BrowseAsync("release-group", releaseGroupId, 100, 0, "recordings", "genres",
                "release-groups",
                "artist-credits"));
        return result?.Items ?? [];
    }

    public async Task<List<Release>> SearchReleaseByNameAsync(string name)
    {
        var result = await ExecuteThrottledAsync($"search_release:{name}", TimeSpan.FromDays(1),
            () => client.Releases.SearchAsync(name));

        return result?.Items ?? [];
    }

    // Release groups

    public async Task<List<ReleaseGroup>> GetReleaseGroupsForArtistAsync(string artistId)
    {
        var result = await ExecuteThrottledAsync($"artist:{artistId}:release-groups", TimeSpan.FromDays(1),
            () => client.ReleaseGroups.BrowseAsync("artist", artistId, 100, 0));
        return result?.Items ?? [];
    }


    private async Task<T?> ExecuteThrottledAsync<T>(string cacheKey, TimeSpan cacheDuration, Func<Task<T>> fetch)
    {
        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheDuration;
            await _throttle.WaitAsync();
            try
            {
                return await fetch();
            }
            finally
            {
                await Task.Delay(1000); // Rate limit: 1 req/sec
                _throttle.Release();
            }
        });
    }
}