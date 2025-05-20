using MusicGQL.Features.Downloads.Sagas.Handlers;
using MusicGQL.Integration.MusicBrainz;
using Rebus.Bus;
using Rebus.Handlers;

namespace MusicGQL.Features.ServerLibrary.Artist.Sagas.Events;

public class FindArtistInMusicBrainzHandler(
    ILogger<LookupRecordingsForReleaseInMusicBrainzHandler> logger,
    IBus bus,
    MusicBrainzService service
) : IHandleMessages<AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz>
{
    public async Task Handle(AddArtistToServerLibrarySagaEvents.FindArtistInMusicBrainz message)
    {
        logger.LogInformation("Looking up artist ID: {Id}", message.ArtistMbId);

        try
        {
            var artist = await service.GetArtistByIdAsync(message.ArtistMbId);

            if (artist is null)
            {
                await bus.Send(
                    new AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz(
                        message.ArtistMbId
                    )
                );
                return;
            }

            logger.LogInformation("Found artist ID: {Id}", message.ArtistMbId);

            var releaseGroups = await service.GetReleaseGroupsForArtistAsync(artist.Id);

            await bus.Send(
                new AddArtistToServerLibrarySagaEvents.FoundArtistInMusicBrainz(
                    message.ArtistMbId,
                    artist,
                    releaseGroups
                )
            );
        }
        catch
        {
            logger.LogWarning("Error while finding artist ID: {Id}", message.ArtistMbId);

            await bus.Send(
                new AddArtistToServerLibrarySagaEvents.DidNotFindArtistInMusicBrainz(
                    message.ArtistMbId
                )
            );
        }
    }
}
