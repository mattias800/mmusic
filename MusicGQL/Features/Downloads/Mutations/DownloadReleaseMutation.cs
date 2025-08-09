using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Types;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartDownloadReleaseMutation
{
    public async Task<StartDownloadReleaseResult> StartDownloadRelease(
        [Service] StartDownloadReleaseService service,
        StartDownloadReleaseInput input
    )
    {
        var (success, error) = await service.StartAsync(input.ArtistId, input.ReleaseFolderName);
        if (!success)
        {
            return new StartDownloadReleaseUnknownError(error ?? "Unknown error");
        }
        return new StartDownloadReleaseSuccess(true);
    }
}

public record StartDownloadReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(bool Success) : StartDownloadReleaseResult;

public record StartDownloadReleaseUnknownError(string Message) : StartDownloadReleaseResult;
