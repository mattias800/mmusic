namespace MusicGQL.Features.Downloads.Services;

public class DownloadCancellationService
{
    private CancellationTokenSource? _cts;
    private string? _artistId;
    private string? _releaseFolderName;

    public CancellationToken CreateFor(string artistId, string releaseFolderName, CancellationToken? linkedWith = null)
    {
        try { _cts?.Cancel(); } catch { }
        try { _cts?.Dispose(); } catch { }

        _artistId = artistId;
        _releaseFolderName = releaseFolderName;
        _cts = linkedWith.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(linkedWith.Value)
            : new CancellationTokenSource();
        return _cts.Token;
    }

    public bool CancelActiveForArtist(string artistId)
    {
        if (!string.Equals(_artistId, artistId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        try { _cts?.Cancel(); } catch { }
        return true;
    }

    public void Clear()
    {
        try { _cts?.Dispose(); } catch { }
        _cts = null;
        _artistId = null;
        _releaseFolderName = null;
    }
}


