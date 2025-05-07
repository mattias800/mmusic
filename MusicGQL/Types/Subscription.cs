using MusicGQL.Features.Downloads;
using MusicGQL.Sagas.DownloadRelease;

namespace MusicGQL.Types;

public class Subscription
{
    [Subscribe]
    public DownloadStatus DownloadStatusUpdated([EventMessage] DownloadReleaseSagaData model) => new(model);

    [Subscribe]
    public DownloadStatus DownloadStarted([EventMessage] DownloadReleaseSagaData model) => new(model);
}