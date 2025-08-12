namespace MusicGQL.Features.Downloads.Services;

public record DownloadHistoryItem
(
    DateTime TimestampUtc,
    string ArtistId,
    string ReleaseFolderName,
    string? ArtistName,
    string? ReleaseTitle,
    bool Success,
    string? ErrorMessage
);

public class DownloadHistoryService
{
    private readonly LinkedList<DownloadHistoryItem> _items = new();
    private readonly object _sync = new();
    private const int MaxItems = 200;

    public void Add(DownloadHistoryItem item)
    {
        lock (_sync)
        {
            _items.AddFirst(item);
            while (_items.Count > MaxItems)
            {
                _items.RemoveLast();
            }
        }
    }

    public List<DownloadHistoryItem> List()
    {
        lock (_sync)
        {
            return _items.ToList();
        }
    }
}


