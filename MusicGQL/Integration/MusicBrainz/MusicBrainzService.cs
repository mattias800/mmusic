using Hqub.MusicBrainz;
using Hqub.MusicBrainz.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MusicGQL.Integration.MusicBrainz;

public class MusicBrainzService(MusicBrainzClient client, IMemoryCache cache)
{
    private readonly SemaphoreSlim _throttle = new(1, 1);

    public async Task<Artist?> GetArtistAsync(string id)
    {
        return await cache.GetOrCreateAsync($"artist:{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            await _throttle.WaitAsync(); // 1 request/sec
            try
            {
                return await client.Artists.GetAsync(id);
            }
            finally
            {
                await Task.Delay(1000); // enforce delay
                _throttle.Release();
            }
        });
    }

    public async Task<List<Artist>?> SearchAsync(string name)
    {
        return await cache.GetOrCreateAsync($"search_artist:{name}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            await _throttle.WaitAsync();
            try
            {
                var result = await client.Artists.SearchAsync(name, 20);
                return result?.Items;
            }
            finally
            {
                await Task.Delay(1000);
                _throttle.Release();
            }
        });
    }
}