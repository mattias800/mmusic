using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public class MoveCurrentDownloadToBackMutation
{
    public async Task<bool> MoveCurrentDownloadToBack(
        [Service] DownloadQueueService queue,
        [Service] CurrentDownloadStateService current,
        [Service] DownloadCancellationService cancellation
    )
    {
        var cur = current.Get();
        if (cur == null) return false;
        // Re-enqueue current item at back of normal queue
        try
        {
            // Cancel active transfer
            try { cancellation.CancelActiveForArtist(cur.ArtistId); } catch { }
            queue.Enqueue(new DownloadQueueItem(cur.ArtistId, cur.ReleaseFolderName)
            {
                ArtistName = cur.ArtistName,
                ReleaseTitle = cur.ReleaseTitle,
            });
        }
        catch { }
        return true;
    }
}


