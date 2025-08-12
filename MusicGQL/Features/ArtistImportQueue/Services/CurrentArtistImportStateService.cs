using HotChocolate.Subscriptions;

namespace MusicGQL.Features.ArtistImportQueue.Services;

public class CurrentArtistImportStateService(
    ITopicEventSender eventSender,
    ILogger<CurrentArtistImportStateService> logger
)
{
    private ArtistImportProgress? _state = null;

    public ArtistImportProgress? Get() => _state;

    public void Reset()
    {
        // Publish an explicit Idle state instead of null so subscribers receive
        // a non-null payload and UI can reliably transition to Idle.
        _state = new ArtistImportProgress
        {
            Status = ArtistImportStatus.Idle,
        };
        Publish();
    }

    public void Set(ArtistImportProgress progress)
    {
        _state = progress;
        Publish();
    }

    public void SetStatus(ArtistImportStatus status)
    {
        _state = (_state ?? new ArtistImportProgress()) with { Status = status };
        Publish();
    }

    public void SetReleaseProgress(int completed, int total)
    {
        _state = (_state ?? new ArtistImportProgress()) with { CompletedReleases = completed, TotalReleases = total };
        Publish();
    }

    public void SetError(string error)
    {
        _state = (_state ?? new ArtistImportProgress()) with { Status = ArtistImportStatus.Failed, ErrorMessage = error };
        Publish();
    }

    private void Publish()
    {
        _ = eventSender.SendAsync(
            ArtistImportSubscription.CurrentArtistImportUpdatedTopic,
            _state
        );
    }
}


