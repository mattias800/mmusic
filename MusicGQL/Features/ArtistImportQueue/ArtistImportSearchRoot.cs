using MusicGQL.Features.ArtistImportQueue.Services;

namespace MusicGQL.Features.ArtistImportQueue;

public class ArtistImportSearchRoot
{
    public ArtistImportQueueState ArtistImportQueue(ArtistImportQueueService queue) => queue.Snapshot();

    public ArtistImportProgress? CurrentArtistImport(CurrentArtistImportStateService state) => state.Get();

    public List<Services.ArtistImportHistoryItem> ArtistImportHistory(Services.ImportHistoryService history) => history.List();
}