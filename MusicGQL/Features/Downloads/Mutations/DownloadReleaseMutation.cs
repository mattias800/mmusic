using MusicGQL.Features.Downloads.Sagas;
using MusicGQL.Types;
using Rebus.Bus;

namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class StartDownloadReleaseMutation
{
    public async Task<StartDownloadReleaseResult> StartDownloadRelease(
        [Service] IBus bus,
        StartDownloadReleaseInput input
    )
    {
        await bus.Send(new DownloadReleaseQueuedEvent(input.ReleaseId));
        return new StartDownloadReleaseSuccess(true);
    }
}

public record StartDownloadReleaseInput(string ReleaseId);

[UnionType("StartDownloadReleaseResult")]
public abstract record StartDownloadReleaseResult { };

public record StartDownloadReleaseSuccess(bool Success) : StartDownloadReleaseResult;
