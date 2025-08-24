namespace MusicGQL.Features.Downloads.Services;

public class DownloadCancellationService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, CancellationTokenSource> _byRelease = new(
        StringComparer.OrdinalIgnoreCase
    );

    private static string Key(string artistId, string releaseFolderName) =>
        $"{artistId}|{releaseFolderName}";

    // Create or reuse a cancellation token for a specific artist/release. Does NOT cancel tokens for other releases.
    public CancellationToken CreateFor(
        string artistId,
        string releaseFolderName,
        CancellationToken? linkedWith = null
    )
    {
        var key = Key(artistId, releaseFolderName);
        lock (_lock)
        {
            if (_byRelease.TryGetValue(key, out var existing))
            {
                // Reuse existing if not canceled; otherwise replace
                if (!existing.IsCancellationRequested)
                {
                    return existing.Token;
                }
                try
                {
                    existing.Dispose();
                }
                catch { }
                _byRelease.Remove(key);
            }

            var cts = linkedWith.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(linkedWith.Value)
                : new CancellationTokenSource();
            _byRelease[key] = cts;
            return cts.Token;
        }
    }

    // Cancels all active tokens for the given artist. Returns true if any were canceled.
    public bool CancelActiveForArtist(string artistId)
    {
        var prefix = artistId + "|";
        var canceled = false;
        lock (_lock)
        {
            var keys = _byRelease
                .Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var k in keys)
            {
                if (_byRelease.TryGetValue(k, out var cts))
                {
                    try
                    {
                        cts.Cancel();
                    }
                    catch { }
                    try
                    {
                        cts.Dispose();
                    }
                    catch { }
                    _byRelease.Remove(k);
                    canceled = true;
                }
            }
        }
        return canceled;
    }

    // Cancels a specific artist/release token, if present.
    public bool CancelForRelease(string artistId, string releaseFolderName)
    {
        var key = Key(artistId, releaseFolderName);
        lock (_lock)
        {
            if (_byRelease.TryGetValue(key, out var cts))
            {
                try
                {
                    cts.Cancel();
                }
                catch { }
                try
                {
                    cts.Dispose();
                }
                catch { }
                _byRelease.Remove(key);
                return true;
            }
        }
        return false;
    }

    // Clears all tokens (disposes without cancel).
    public void Clear()
    {
        lock (_lock)
        {
            foreach (var cts in _byRelease.Values)
            {
                try
                {
                    cts.Dispose();
                }
                catch { }
            }
            _byRelease.Clear();
        }
    }
}
