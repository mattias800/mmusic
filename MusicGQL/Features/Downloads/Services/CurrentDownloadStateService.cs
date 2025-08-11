using HotChocolate.Subscriptions;

namespace MusicGQL.Features.Downloads.Services;

public class CurrentDownloadStateService(
    ITopicEventSender eventSender,
    ILogger<CurrentDownloadStateService> logger
)
{
    private DownloadProgress? _state = null;

    public DownloadProgress? Get() => _state;

    public void Reset()
    {
        _state = null;
        Publish();
    }

    public void Set(DownloadProgress progress)
    {
        _state = progress;
        Publish();
    }

    public void SetStatus(DownloadStatus status)
    {
        _state = (_state ?? new DownloadProgress()) with { Status = status };
        Publish();
    }

    public void SetTrackProgress(int completed, int total)
    {
        _state = (_state ?? new DownloadProgress()) with { CompletedTracks = completed, TotalTracks = total };
        Publish();
    }

    public void SetError(string error)
    {
        _state = (_state ?? new DownloadProgress()) with { Status = DownloadStatus.Failed, ErrorMessage = error };
        Publish();
    }

    private void Publish()
    {
        _ = eventSender.SendAsync(
            DownloadsSubscription.CurrentDownloadUpdatedTopic,
            _state
        );
    }
}


