using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public class MoveCurrentDownloadToBackMutation
{
    public async Task<MoveCurrentDownloadToBackResult> MoveCurrentDownloadToBack(
        [Service] DownloadQueueService queue,
        [Service] CurrentDownloadStateService current,
        [Service] DownloadCancellationService cancellation
    )
    {
        var cur = current.Get();
        if (cur == null)
        {
            return new MoveCurrentDownloadToBackError("No current download");
        }

        try
        {
            // Cancel active transfer and re-enqueue this item at the back
            try
            {
                cancellation.CancelActiveForArtist(cur.ArtistId);
            }
            catch
            {
            }

            queue.Enqueue(new DownloadQueueItem(cur.ArtistId, cur.ReleaseFolderName)
            {
                ArtistName = cur.ArtistName,
                ReleaseTitle = cur.ReleaseTitle,
            });
        }
        catch (Exception ex)
        {
            return new MoveCurrentDownloadToBackError(ex.Message);
        }

        return new MoveCurrentDownloadToBackSuccess(queue.Snapshot());
    }
}

[UnionType("MoveCurrentDownloadToBackResult")]
public abstract record MoveCurrentDownloadToBackResult;

public record MoveCurrentDownloadToBackSuccess(DownloadQueueState DownloadQueue) : MoveCurrentDownloadToBackResult;

public record MoveCurrentDownloadToBackError(string Message) : MoveCurrentDownloadToBackResult;