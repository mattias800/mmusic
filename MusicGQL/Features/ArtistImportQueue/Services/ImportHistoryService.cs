namespace MusicGQL.Features.ArtistImportQueue.Services;

public record ArtistImportHistoryItem
(
    DateTime TimestampUtc,
    ArtistImportJobKind JobKind,
    string ArtistName,
    string? LocalArtistId,
    string? ReleaseFolderName,
    string? MusicBrainzArtistId,
    string? SongTitle,
    bool Success,
    string? ErrorMessage
);

public class ImportHistoryService
{
    private readonly LinkedList<ArtistImportHistoryItem> _items = new();
    private readonly object _sync = new();
    private const int MaxItems = 200;

    public void Add(ArtistImportHistoryItem item)
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

    public List<ArtistImportHistoryItem> List()
    {
        lock (_sync)
        {
            return _items.ToList();
        }
    }
}


