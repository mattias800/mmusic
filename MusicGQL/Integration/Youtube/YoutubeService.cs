using System.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;

namespace MusicGQL.Integration.Youtube;

public class YouTubeService(Google.Apis.YouTube.v3.YouTubeService youTubeService, HybridCache cache)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);

    public async Task<string?> GetVideoIdForSearchText(string searchText)
    {
        return await ExecuteThrottledAsync(
            $"youtube:search:{searchText}",
            TimeSpan.FromDays(1),
            async () =>
            {
                var request = youTubeService.Search.List("snippet");
                request.Q = searchText;
                request.MaxResults = 10;

                var r = await request.ExecuteAsync();

                var video = r.Items.FirstOrDefault(item => item.Kind == "youtube#video");
                return video?.Id.VideoId;
            }
        );
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

                    var timeLeftForYoutubeLimit = Math.Max(
                        1500 - (int)stopwatch.ElapsedMilliseconds,
                        0
                    );
                    await Task.Delay(timeLeftForYoutubeLimit, cancellationToken); // Rate limit: 1 req/sec
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
