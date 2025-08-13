using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public class CancelCurrentDownloadMutation
{
    public async Task<CancelCurrentDownloadResult> CancelCurrentDownload(
        CancelCurrentDownloadInput input,
        [Service] DownloadCancellationService cancellation,
        [Service] CurrentDownloadStateService current
    )
    {
        var ok = cancellation.CancelActiveForArtist(input.ArtistId);
        if (ok)
        {
            try { current.SetError("Cancelled by user"); } catch { }
            try { current.Reset(); } catch { }
            return new CancelCurrentDownloadSuccess();
        }
        return new CancelCurrentDownloadError("No active download for this artist");
    }
}

public record CancelCurrentDownloadInput(string ArtistId);

[UnionType("CancelCurrentDownloadResult")]
public abstract record CancelCurrentDownloadResult;

public record CancelCurrentDownloadSuccess(bool Ok = true) : CancelCurrentDownloadResult;

public record CancelCurrentDownloadError(string Message) : CancelCurrentDownloadResult;


