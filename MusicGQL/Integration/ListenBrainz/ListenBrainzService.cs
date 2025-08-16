using System.Diagnostics;
using MetaBrainz.ListenBrainz;
using MetaBrainz.ListenBrainz.Interfaces;
using MetaBrainz.ListenBrainz.Objects;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Integration.ListenBrainz;

public class ListenBrainzService(MetaBrainz.ListenBrainz.ListenBrainz client, HybridCache cache, ILogger<ListenBrainzService> logger)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);

    // Submit Listens (using the client's currently-set token)

    public async Task<bool> SubmitListensAsync(IEnumerable<ISubmittedListen> listens)
    {
        try
        {
            await client.ImportListensAsync(listens);
            logger.LogInformation("Successfully submitted {Count} listens to ListenBrainz", listens.Count());
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit listens to ListenBrainz");
            return false;
        }
    }

    public async Task<bool> SubmitSingleListenAsync(ISubmittedListen listen)
    {
        try
        {
            await client.SubmitSingleListenAsync(listen);
            logger.LogInformation("Successfully submitted single listen to ListenBrainz");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit single listen to ListenBrainz");
            return false;
        }
    }

    // Submit using a specific token (useful for per-user submissions)

    public async Task<bool> SubmitListensAsync(IEnumerable<ISubmittedListen> listens, string userToken)
    {
        var previous = client.UserToken;
        try
        {
            client.UserToken = userToken;
            await client.ImportListensAsync(listens);
            logger.LogInformation("Successfully submitted {Count} listens to ListenBrainz (per-user token)", listens.Count());
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit listens to ListenBrainz with provided token");
            return false;
        }
        finally
        {
            client.UserToken = previous;
        }
    }

    public async Task<bool> SubmitSingleListenAsync(ISubmittedListen listen, string userToken)
    {
        var previous = client.UserToken;
        try
        {
            client.UserToken = userToken;
            await client.SubmitSingleListenAsync(listen);
            logger.LogInformation("Successfully submitted single listen to ListenBrainz (per-user token)");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit single listen to ListenBrainz with provided token");
            return false;
        }
        finally
        {
            client.UserToken = previous;
        }
    }

    // Validate Token (active call to LB API)

    public async Task<(bool IsValid, string? User, string? Message)> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await client.ValidateTokenAsync(token, cancellationToken);
            // Valid can be null -> fall back to message when needed
            var isValid = result.Valid ?? string.Equals(result.Message, "ok", StringComparison.OrdinalIgnoreCase);
            return (isValid, result.User, result.Message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to validate ListenBrainz token via API");
            return (false, null, ex.Message);
        }
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

                    // ListenBrainz rate limit: 1 request per second
                    var timeLeftForListenBrainzLimit = Math.Max(
                        1000 - (int)stopwatch.ElapsedMilliseconds,
                        0
                    );
                    await Task.Delay(timeLeftForListenBrainzLimit, cancellationToken);
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
