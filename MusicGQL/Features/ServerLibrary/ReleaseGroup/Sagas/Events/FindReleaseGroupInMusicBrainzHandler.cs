using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Sagas.DownloadRelease.Handlers;
using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas.Events;

public class FindReleaseGroupInMusicBrainzHandler(
    ILogger<LookupRecordingsForReleaseInMusicBrainzHandler> logger,
    IBus bus,
    MusicBrainzService service
) : IHandleMessages<AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz>
{
    public async Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz message
    )
    {
        logger.LogInformation("Looking up artist ID: {Id}", message.ReleaseGroupMbId);

        try
        {
            var artist = await service.GetReleaseGroupByIdAsync(message.ReleaseGroupMbId);

            if (artist is null)
            {
                await bus.Send(
                    new AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz(
                        message.ReleaseGroupMbId
                    )
                );
                return;
            }

            logger.LogInformation("Found artist ID: {Id}", message.ReleaseGroupMbId);

            await bus.Send(
                new AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz(
                    message.ReleaseGroupMbId,
                    artist
                )
            );
        }
        catch
        {
            logger.LogWarning("Error while finding artist ID: {Id}", message.ReleaseGroupMbId);

            await bus.Send(
                new AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz(
                    message.ReleaseGroupMbId
                )
            );
        }
    }
}
