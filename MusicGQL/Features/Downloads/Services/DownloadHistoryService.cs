using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.Downloads;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadHistoryService
{
    private readonly LinkedList<DownloadHistoryItem> _items = new();
    private readonly Dictionary<string, EnhancedDownloadHistoryItem> _enhancedItems = new();
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

    public void AddEnhanced(EnhancedDownloadHistoryItem item)
    {
        lock (_sync)
        {
            var key = $"{item.ArtistId}|{item.ReleaseFolderName}";
            _enhancedItems[key] = item;
            
            // Also add to legacy list for backward compatibility
            var legacyItem = new DownloadHistoryItem(
                item.TimestampUtc,
                item.ArtistId,
                item.ReleaseFolderName,
                item.ArtistName,
                item.ReleaseTitle,
                item.FinalResult == DownloadResult.Success,
                item.ErrorMessage,
                item.ProviderUsed
            );
            Add(legacyItem);
        }
    }

    public void UpdateState(string artistId, string releaseFolderName, DownloadState newState, string? notes = null)
    {
        lock (_sync)
        {
            var key = $"{artistId}|{releaseFolderName}";
            if (_enhancedItems.TryGetValue(key, out var existing))
            {
                var now = DateTime.UtcNow;
                var durationInPreviousState = now - existing.StateStartTime;
                
                var transition = new DownloadStateTransition
                {
                    FromState = existing.CurrentState,
                    ToState = newState,
                    Timestamp = now,
                    DurationInPreviousState = durationInPreviousState,
                    Notes = notes
                };

                var updated = existing with
                {
                    CurrentState = newState,
                    StateStartTime = now,
                    StateTransitions = existing.StateTransitions.Append(transition).ToList()
                };

                _enhancedItems[key] = updated;
            }
        }
    }

    public void UpdateResult(string artistId, string releaseFolderName, DownloadResult result, string? errorMessage = null)
    {
        lock (_sync)
        {
            var key = $"{artistId}|{releaseFolderName}";
            if (_enhancedItems.TryGetValue(key, out var existing))
            {
                var now = DateTime.UtcNow;
                var totalDuration = now - existing.TimestampUtc;
                
                var updated = existing with
                {
                    FinalResult = result,
                    ErrorMessage = errorMessage,
                    TotalDuration = totalDuration,
                    CurrentState = DownloadState.Finished
                };

                _enhancedItems[key] = updated;
            }
        }
    }

    public EnhancedDownloadHistoryItem? GetEnhanced(string artistId, string releaseFolderName)
    {
        lock (_sync)
        {
            var key = $"{artistId}|{releaseFolderName}";
            return _enhancedItems.TryGetValue(key, out var item) ? item : null;
        }
    }

    public List<DownloadHistoryItem> List()
    {
        lock (_sync)
        {
            return _items.ToList();
        }
    }

    public List<EnhancedDownloadHistoryItem> ListEnhanced()
    {
        lock (_sync)
        {
            return _enhancedItems.Values.ToList();
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _items.Clear();
            _enhancedItems.Clear();
        }
    }
}


