using MusicGQL.Integration.MusicBrainz;
using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Features.Downloads.Sagas.Handlers;

public class LookupRecordingsForReleaseInMusicBrainzHandler(
    ILogger<LookupRecordingsForReleaseInMusicBrainzHandler> logger,
    IBus bus,
    MusicBrainzService service
) : IHandleMessages<LookupRecordingsForReleaseInMusicBrainz>
{
    public async Task Handle(LookupRecordingsForReleaseInMusicBrainz message)
    {
        logger.LogInformation(
            "Looking up recordings for release ID: {MusicBrainzReleaseId}",
            message.MusicBrainzReleaseId
        );

        try
        {
            var recordings = await service.GetRecordingsForReleaseAsync(
                message.MusicBrainzReleaseId
            );

            logger.LogInformation(
                "Found {Num} recordings for release ID: {MusicBrainzReleaseId}",
                recordings.Count,
                message.MusicBrainzReleaseId
            );

            await bus.Send(
                new FoundRecordingsForReleaseInMusicBrainz(
                    message.MusicBrainzReleaseId,
                    message.Release,
                    recordings
                )
            );
        }
        catch
        {
            logger.LogWarning(
                "Could not find recordings for release ID: {MusicBrainzReleaseId}",
                message.MusicBrainzReleaseId
            );

            await bus.Send(new NoRecordingsFoundInMusicBrainz(message.MusicBrainzReleaseId));
        }
    }
}
