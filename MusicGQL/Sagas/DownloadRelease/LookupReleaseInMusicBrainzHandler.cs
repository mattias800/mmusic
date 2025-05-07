using MusicGQL.Integration.MusicBrainz;
using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Sagas.DownloadRelease;

public class LookupReleaseInMusicBrainzHandler(
    ILogger<LookupReleaseInMusicBrainzHandler> logger,
    IBus bus,
    MusicBrainzService service
) : IHandleMessages<LookupReleaseInMusicBrainz>
{
    public async Task Handle(LookupReleaseInMusicBrainz message)
    {
        logger.LogInformation(
            "Looking up release with ID: {MusicBrainzReleaseId}",
            message.MusicBrainzReleaseId
        );

        await Task.Delay(5000);

        var release = await service.GetReleaseByIdAsync(message.MusicBrainzReleaseId);
        if (release is null)
        {
            logger.LogWarning(
                "Could not find release with ID: {MusicBrainzReleaseId}",
                message.MusicBrainzReleaseId
            );
            await bus.Send(new ReleaseNotFoundInMusicBrainz(message.MusicBrainzReleaseId));
        }
        else
        {
            logger.LogInformation(
                "Found release with ID: {MusicBrainzReleaseId}, {Artist} - {Title}",
                message.MusicBrainzReleaseId,
                release.Credits.First().Artist.Name,
                release.Title
            );

            await bus.Send(new FoundReleaseInMusicBrainz(message.MusicBrainzReleaseId, release));
        }
    }
}
