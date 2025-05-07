using MusicGQL.Features.Downloads;
using MusicGQL.Sagas.DownloadRelease;

namespace MusicGQL.Types;

public class Subscription
{
    [Subscribe]
    public DownloadStatus DownloadStatusUpdated([EventMessage] DownloadStatus s) => s;

    [Subscribe]
    public DownloadStatus DownloadStarted([EventMessage] DownloadStatus s) => s;
}
