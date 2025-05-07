using MusicGQL.Integration.MusicBrainz;
using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Sagas.DownloadRelease;

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

        await Task.Delay(5000);

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
