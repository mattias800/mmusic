using HotChocolate.Subscriptions;

namespace MusicGQL.Features.ArtistImportQueue.Services;

public class CurrentArtistImportStateService(
    ITopicEventSender eventSender,
    ILogger<CurrentArtistImportStateService> logger
)
{
    private ArtistImportProgress _state = new();

    public ArtistImportProgress Get() => _state;

    public void Reset()
    {
        _state = new ArtistImportProgress();
        Publish();
    }

    public void Set(ArtistImportProgress progress)
    {
        _state = progress;
        Publish();
    }

    public void SetStatus(ArtistImportStatus status)
    {
        _state = _state with { Status = status };
        Publish();
    }

    public void SetReleaseProgress(int completed, int total)
    {
        _state = _state with { CompletedReleases = completed, TotalReleases = total };
        Publish();
    }

    public void SetError(string error)
    {
        _state = _state with { Status = ArtistImportStatus.Failed, ErrorMessage = error };
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


