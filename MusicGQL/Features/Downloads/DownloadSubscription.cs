using MusicGQL.Types;

namespace MusicGQL.Features.Downloads;

[ExtendObjectType(typeof(Subscription))]
public record DownloadSubscription
{
    [Subscribe]
    public DownloadStatus DownloadStatusUpdated([EventMessage] DownloadStatus s) => s;

    [Subscribe]
    public DownloadStatus DownloadStarted([EventMessage] DownloadStatus s) => s;
};
