using System.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;

namespace MusicGQL.Integration;

public abstract class CachedService(HybridCache cache)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);

    protected async Task<T?> ExecuteThrottledAsync<T>(
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
